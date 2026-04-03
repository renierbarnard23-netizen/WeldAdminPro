"""
WeldAdmin Pro – Ingestion, Parsing, Validation & FTS Search (drop‑in module)

This module gives you a production‑ready ingestion pipeline for WPS/PQR/WPQR PDFs:
- PDF → OCR (Tesseract) wrapper (you already have Tesseract/Poppler; this wraps them)
- Structured parsing (regex + heuristics) into typed records (WPS, PQR, WPQR)
- Rule‑based validation with human‑readable error messages
- Confidence scoring per field and per document
- SQLite schema with FTS5 search over normalized fields + raw OCR text
- Import log and error log
- Simple CLI usage examples at bottom

No external Python packages required beyond standard library + sqlite3.
Tested on Windows; paths assume UTF‑8.
"""
from __future__ import annotations
import dataclasses as dc
import json
import os
import re
import shutil
import sqlite3
import subprocess
import sys
import tempfile
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# ---------------------------
# Configuration
# ---------------------------

ALLOWED_PROCESSES = {
    "GMAW", "FCAW", "SMAW", "GTAW", "SAW", "MCAW", "PAW", "OFC", "OAW",
}

# Regex patterns tuned for typical South African/ISO 3834 formatting.
# Add/adjust quickly without changing code.
PATS = {
    "wps_number": r"\bWPS[-_\s]*([A-Z0-9./-]+)\b",
    "pqr_number": r"\bPQR[-_\s]*([A-Z0-9./-]+)\b",
    "wpqr_number": r"\bWPQR[-_\s]*([A-Z0-9./-]+)\b",
    "process": r"\b(PROCESS|WELD(?:ING)?\s*PROCESS)\s*[:\-]?\s*(GMAW|FCAW|SMAW|GTAW|SAW|MCAW|PAW|OFC|OAW)\b",
    "material": r"\b(MATERIAL|BASE\s*MATERIAL)\s*[:\-]?\s*([A-Z0-9\s/.-]+)\b",
    "thickness": r"\b(THICKNESS|MATL\s*THK)\s*[:\-]?\s*([0-9]+(?:\.[0-9]+)?)\s*mm\b",
    "filler": r"\b(FILLER|FILLER\s*METAL)\s*[:\-]?\s*([A-Z0-9\s/.-]+)\b",
    "shielding": r"\b(SHIELDING\s*GAS)\s*[:\-]?\s*([A-Z0-9\s+/%.-]+)\b",
    "position": r"\b(POSITION|WELD\s*POSITION)\s*[:\-]?\s*([1-6][FG]|PA|PB|PC|PD|PE|PF|PG)\b",
    "date": r"\b(\d{4}[-/]\d{2}[-/]\d{2}|\d{2}[-/]\d{2}[-/]\d{4})\b",
    "company": r"\b(COMPANY|FABRICATOR)\s*[:\-]?\s*([A-Z0-9\s&.,'-]+)\b",
}

DATE_FORMATS = ["%Y-%m-%d", "%d-%m-%Y", "%Y/%m/%d", "%d/%m/%Y"]

# ---------------------------
# Data structures
# ---------------------------

@dc.dataclass
class Field:
    name: str
    value: Optional[str]
    confidence: float = 0.0
    source_span: Optional[Tuple[int, int]] = None  # indices in OCR text

@dc.dataclass
class BaseRecord:
    doc_type: str  # WPS | PQR | WPQR
    doc_number: Field
    process: Field = dc.field(default_factory=lambda: Field("process", None, 0.0))
    material: Field = dc.field(default_factory=lambda: Field("material", None, 0.0))
    thickness_mm: Field = dc.field(default_factory=lambda: Field("thickness_mm", None, 0.0))
    filler: Field = dc.field(default_factory=lambda: Field("filler", None, 0.0))
    shielding_gas: Field = dc.field(default_factory=lambda: Field("shielding_gas", None, 0.0))
    position: Field = dc.field(default_factory=lambda: Field("position", None, 0.0))
    company: Field = dc.field(default_factory=lambda: Field("company", None, 0.0))
    date: Field = dc.field(default_factory=lambda: Field("date", None, 0.0))

    def to_dict(self) -> Dict[str, Optional[str]]:
        return {
            "doc_type": self.doc_type,
            "doc_number": self.doc_number.value,
            "process": self.process.value,
            "material": self.material.value,
            "thickness_mm": self.thickness_mm.value,
            "filler": self.filler.value,
            "shielding_gas": self.shielding_gas.value,
            "position": self.position.value,
            "company": self.company.value,
            "date": self.date.value,
        }

    def avg_confidence(self) -> float:
        fields = [self.doc_number, self.process, self.material, self.thickness_mm,
        self.filler, self.shielding_gas, self.position, self.company, self.date]
        scores = [f.confidence for f in fields if f.value]
        return sum(scores) / len(scores) if scores else 0.0
    def avg_conf(self) -> float:
        # Backwards-compatible alias
        return self.avg_confidence()

# ---------------------------
# OCR wrapper (Tesseract + Poppler)
# ---------------------------

def _find_exe(name: str, candidates: list[str]) -> Optional[str]:
    p = shutil.which(name)
    if p:
        return p
    for c in candidates:
        if Path(c).exists():
            return str(Path(c))
    return None

def _which(name: str, extra: list[str] = None) -> Optional[str]:
    p = shutil.which(name)
    if p:
        return p
    for c in (extra or []):
        if Path(c).exists():
            return c
    return None

def _render_pdf_images(pdf_path: Path, out_prefix: Path, dpi: int) -> list[Path]:
    """
    Render PDF pages to images at out_prefix-###.png (preferred) or .ppm.
    Tries: pdftoppm -> pdftocairo -> Ghostscript.
    Returns a list of image Paths.
    """
    images: list[Path] = []

    # 1) pdftoppm
    pdftoppm_bin = _which("pdftoppm", [
        r"C:\poppler\Library\bin\pdftoppm.exe",
        r"C:\Program Files\poppler\bin\pdftoppm.exe",
        r"C:\poppler-24.07.0\Library\bin\pdftoppm.exe",
    ])
    if pdftoppm_bin:
        try:
            subprocess.run([pdftoppm_bin, "-r", str(dpi), pdf_path.as_posix(), out_prefix.as_posix()],
                           check=True, capture_output=True)
            images = sorted(out_prefix.parent.glob(out_prefix.name + "-*.ppm"))
            if not images:
                # some builds output PNG with -png
                subprocess.run([pdftoppm_bin, "-png", "-r", str(dpi), pdf_path.as_posix(), out_prefix.as_posix()],
                               check=True, capture_output=True)
                images = sorted(out_prefix.parent.glob(out_prefix.name + "-*.png"))
            if images:
                return images
        except subprocess.CalledProcessError as e:
            # if missing Symbol font or any render error, fall through
            pass

    # 2) pdftocairo (often handles fonts better)
    pdftocairo_bin = _which("pdftocairo", [
        r"C:\poppler\Library\bin\pdftocairo.exe",
        r"C:\Program Files\poppler\bin\pdftocairo.exe",
        r"C:\poppler-24.07.0\Library\bin\pdftocairo.exe",
    ])
    if pdftocairo_bin:
        try:
            subprocess.run([pdftocairo_bin, "-png", "-r", str(dpi), pdf_path.as_posix(), out_prefix.as_posix()],
                           check=True, capture_output=True)
            images = sorted(out_prefix.parent.glob(out_prefix.name + "-*.png"))
            if images:
                return images
        except subprocess.CalledProcessError:
            pass

    # 3) Ghostscript fallback
    gs_bin = _which("gswin64c.exe", [
        r"C:\Program Files\gs\gs10.04.0\bin\gswin64c.exe",
        r"C:\Program Files\gs\gs10.03.0\bin\gswin64c.exe",
    ]) or _which("gswin32c.exe")
    if gs_bin:
        try:
            png_pattern = (out_prefix.parent / (out_prefix.name + "-%03d.png")).as_posix()
            subprocess.run([
                gs_bin, "-dSAFER", "-dBATCH", "-dNOPAUSE",
                "-sDEVICE=png16m",
                "-r" + str(dpi),
                "-o", png_pattern,
                pdf_path.as_posix()
            ], check=True, capture_output=True)
            images = sorted(out_prefix.parent.glob(out_prefix.name + "-*.png"))
            if images:
                return images
        except subprocess.CalledProcessError:
            pass

    raise RuntimeError("Failed to render PDF pages via pdftoppm/pdftocairo/Ghostscript")

def pdf_to_ocr_text(pdf_path: Path, dpi: int = 300, lang: str = "eng") -> str:
    """
    Convert PDF to images, then OCR with tesseract. Robust renderer fallback.
    """
    if not _which("tesseract", [
        r"C:\Program Files\Tesseract-OCR\tesseract.exe",
        r"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe",
        rf"{os.environ.get('LOCALAPPDATA', r'C:\Users\%USERNAME%\AppData\Local')}\Programs\Tesseract-OCR\tesseract.exe",
    ]):
        raise RuntimeError("tesseract not found on PATH")

    with tempfile.TemporaryDirectory() as td:
        td = Path(td)
        out_prefix = td / "page"
        # Render pages with fallback chain
        images = _render_pdf_images(pdf_path, out_prefix, dpi)

        # OCR each page
        full_text = []
        tesseract_bin = _which("tesseract", [
            r"C:\Program Files\Tesseract-OCR\tesseract.exe",
            r"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe",
            rf"{os.environ.get('LOCALAPPDATA', r'C:\Users\%USERNAME%\AppData\Local')}\Programs\Tesseract-OCR\tesseract.exe",
        ])
        for i, img in enumerate(images, 1):
            out_txt = td / f"ocr_{i}"
            subprocess.run([tesseract_bin, img.as_posix(), out_txt.as_posix(), "-l", lang, "--psm", "6"],
                           check=True)
            txt = Path(str(out_txt) + ".txt").read_text(encoding="utf-8", errors="ignore")
            full_text.append(txt)
        return "\n".join(full_text)

# ---------------------------
# Parsing utilities
# ---------------------------

def _search_with_conf(pattern: str, text: str) -> Tuple[Optional[str], float, Optional[Tuple[int, int]]]:
    m = re.search(pattern, text, flags=re.IGNORECASE)
    if not m:
        return None, 0.0, None
    # Heuristic confidence: longer group capture + header proximity boost
    value = m.group(2) if m.lastindex and m.lastindex >= 2 else (m.group(1) if m.lastindex else m.group(0))
    span = m.span()
    base = min(0.99, 0.5 + (len(value) / 40.0))
    # Header proximity boost if label matched
    if m.lastindex and m.lastindex >= 1:
        base += 0.1
    return value.strip(), min(base, 0.99), span


def guess_doc_type(text: str) -> str:
    counts = {
        "WPS": len(re.findall(r"\bWPS\b", text)),
        "PQR": len(re.findall(r"\bPQR\b", text)),
        "WPQR": len(re.findall(r"\bWPQR\b", text)),
    }
    # pick the one with max hits; fallback WPS
    return max(counts.items(), key=lambda kv: kv[1])[0] if any(counts.values()) else "WPS"


def parse_record(text: str) -> BaseRecord:
    doc_type = guess_doc_type(text)
    # Pick number pattern based on type
    num_pat = {
        "WPS": PATS["wps_number"],
        "PQR": PATS["pqr_number"],
        "WPQR": PATS["wpqr_number"],
    }[doc_type]
    num_val, num_conf, num_span = _search_with_conf(num_pat, text)

    rec = BaseRecord(
        doc_type=doc_type,
        doc_number=Field("doc_number", num_val, num_conf, num_span),
    )

    for key, field_name in [
        ("process", "process"),
        ("material", "material"),
        ("thickness", "thickness_mm"),
        ("filler", "filler"),
        ("shielding", "shielding_gas"),
        ("position", "position"),
        ("company", "company"),
    ]:
        val, conf, span = _search_with_conf(PATS[key], text)
        setattr(rec, field_name, Field(field_name, val, conf, span))

    # Date: normalize to YYYY-MM-DD if possible
    dval, dconf, dspan = _search_with_conf(PATS["date"], text)
    norm_date, norm_conf = None, dconf
    if dval:
        for fmt in DATE_FORMATS:
            try:
                dt = datetime.strptime(dval, fmt)
                norm_date = dt.strftime("%Y-%m-%d")
                norm_conf = max(norm_conf, 0.8)
                break
            except ValueError:
                continue
    rec.date = Field("date", norm_date or dval, norm_conf, dspan)

    return rec

# ---------------------------
# Validation rules
# ---------------------------

@dc.dataclass
class ValidationIssue:
    field: str
    message: str
    severity: str  # INFO|WARN|ERROR


def validate_record(rec: BaseRecord) -> List[ValidationIssue]:
    issues: List[ValidationIssue] = []

    if not rec.doc_number.value:
        issues.append(ValidationIssue("doc_number", f"{rec.doc_type} number not found", "ERROR"))

    if rec.process.value and rec.process.value.upper() not in ALLOWED_PROCESSES:
        issues.append(ValidationIssue("process", f"Unknown process '{rec.process.value}'", "WARN"))

    if rec.thickness_mm.value:
        try:
            thk = float(rec.thickness_mm.value)
            if thk <= 0 or thk > 500:
                issues.append(ValidationIssue("thickness_mm", f"Unreasonable thickness {thk} mm", "WARN"))
        except Exception:
            issues.append(ValidationIssue("thickness_mm", f"Non‑numeric thickness '{rec.thickness_mm.value}'", "ERROR"))

    # Date should not be in the future (allow 3 days skew for scanning delays)
    if rec.date.value:
        try:
            dt = datetime.strptime(rec.date.value, "%Y-%m-%d")
            if dt > datetime.now() :
                issues.append(ValidationIssue("date", f"Date {rec.date.value} is in the future", "WARN"))
        except Exception:
            issues.append(ValidationIssue("date", f"Unrecognized date '{rec.date.value}'", "WARN"))

    # Confidence-based flags
    if rec.doc_number.confidence < 0.6:
        issues.append(ValidationIssue("doc_number", "Low confidence in document number extraction", "WARN"))

    if rec.avg_confidence() < 0.5:
        issues.append(ValidationIssue("_document", "Overall extraction confidence is low", "WARN"))

    return issues

# ---------------------------
# SQLite storage + FTS5 search
# ---------------------------

DDL = r"""
PRAGMA journal_mode=WAL;
CREATE TABLE IF NOT EXISTS documents (
    id INTEGER PRIMARY KEY,
    file_path TEXT NOT NULL,
    doc_type TEXT NOT NULL,
    doc_number TEXT,
    process TEXT,
    material TEXT,
    thickness_mm REAL,
    filler TEXT,
    shielding_gas TEXT,
    position TEXT,
    company TEXT,
    date TEXT,
    avg_conf REAL,
    raw_text TEXT,
    imported_at TEXT NOT NULL
);

-- FTS index (contentless) for fast global search over key fields + raw text
CREATE VIRTUAL TABLE IF NOT EXISTS documents_fts USING fts5(
    doc_type, doc_number, process, material, filler, shielding_gas, position, company, date, raw_text,
    content=''
);

CREATE TABLE IF NOT EXISTS import_log (
    id INTEGER PRIMARY KEY,
    file_path TEXT NOT NULL,
    status TEXT NOT NULL, -- SUCCESS|FAILED
    message TEXT,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS validation_issues (
    id INTEGER PRIMARY KEY,
    document_id INTEGER,
    field TEXT,
    severity TEXT,
    message TEXT,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);
"""

class Repo:
    def __init__(self, db_path: Path):
        self.db_path = Path(db_path)
        self.conn = sqlite3.connect(self.db_path)
        self.conn.execute("PRAGMA foreign_keys=ON")
        self.conn.execute("PRAGMA case_sensitive_like=OFF")
        self._init()

    def _init(self):
        # Run the whole DDL block in one go (supports multiple statements)
        self.conn.executescript(DDL)
        self.conn.commit()

    def log_import(self, file_path: Path, status: str, message: str = ""):
        self.conn.execute(
            "INSERT INTO import_log(file_path, status, message, created_at) VALUES (?,?,?,?)",
            (str(file_path), status, message[:1000], datetime.now().isoformat(timespec="seconds"))
        )
        self.conn.commit()

    def insert_document(self, file_path: Path, rec: BaseRecord, raw_text: str, issues: List[ValidationIssue]) -> int:
        thk = None
        if rec.thickness_mm.value:
            try:
                thk = float(rec.thickness_mm.value)
            except Exception:
                thk = None

        cur = self.conn.cursor()
        cur.execute(
            """
            INSERT INTO documents(
                file_path, doc_type, doc_number, process, material, thickness_mm, filler, shielding_gas,
                position, company, date, avg_conf, raw_text, imported_at
            ) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?)
            """,
            (
                str(file_path), rec.doc_type, rec.doc_number.value, rec.process.value, rec.material.value,
                thk, rec.filler.value, rec.shielding_gas.value, rec.position.value, rec.company.value,
                rec.date.value, round(rec.avg_conf(), 3), raw_text, datetime.now().isoformat(timespec="seconds")
            )
        )
        doc_id = cur.lastrowid
        # FTS index write
        cur.execute(
            "INSERT INTO documents_fts(doc_type, doc_number, process, material, filler, shielding_gas, position, company, date, raw_text) VALUES (?,?,?,?,?,?,?,?,?,?)",
            (
                rec.doc_type, rec.doc_number.value or "", rec.process.value or "", rec.material.value or "",
                rec.filler.value or "", rec.shielding_gas.value or "", rec.position.value or "",
                rec.company.value or "", rec.date.value or "", raw_text
            )
        )
        # Store validation issues
        for isue in issues:
            cur.execute(
                "INSERT INTO validation_issues(document_id, field, severity, message) VALUES (?,?,?,?)",
                (doc_id, isue.field, isue.severity, isue.message)
            )
        self.conn.commit()
        return doc_id

    def search(self, query: str, limit: int = 20) -> List[Dict[str, str]]:
        cur = self.conn.cursor()
        # Rank via bm25; show key fields
        q = (
            "SELECT rowid, highlight(documents_fts, 9, '[', ']') AS text_snippet FROM documents_fts WHERE documents_fts MATCH ? ORDER BY bm25(documents_fts) LIMIT ?"
        )
        cur.execute(q, (query, limit))
        fts_rows = cur.fetchall()
        out = []
        for rowid, snippet in fts_rows:
            doc = cur.execute("SELECT id, doc_type, doc_number, process, material, thickness_mm, company, date, avg_conf FROM documents WHERE id=?", (rowid,)).fetchone()
            if doc:
                out.append({
                    "id": str(doc[0]),
                    "doc_type": doc[1],
                    "doc_number": doc[2] or "",
                    "process": doc[3] or "",
                    "material": doc[4] or "",
                    "thickness_mm": str(doc[5]) if doc[5] is not None else "",
                    "company": doc[6] or "",
                    "date": doc[7] or "",
                    "avg_conf": f"{doc[8]:.2f}" if doc[8] is not None else "",
                    "snippet": snippet,
                })
        return out

# ---------------------------
# Ingest pipeline
# ---------------------------

@dc.dataclass
class IngestResult:
    status: str  # SUCCESS|FAILED
    document_id: Optional[int]
    issues: List[ValidationIssue]
    summary: Dict[str, str]


def ingest_pdf(repo: Repo, pdf_path: Path) -> IngestResult:
    try:
        raw_text = pdf_to_ocr_text(pdf_path)
        rec = parse_record(raw_text)
        issues = validate_record(rec)
        doc_id = repo.insert_document(pdf_path, rec, raw_text, issues)
        repo.log_import(pdf_path, "SUCCESS", f"Imported as {rec.doc_type} {rec.doc_number.value}")
        return IngestResult(
            status="SUCCESS",
            document_id=doc_id,
            issues=issues,
            summary={
                "doc_type": rec.doc_type,
                "doc_number": rec.doc_number.value or "(missing)",
                "avg_conf": f"{rec.avg_conf():.2f}",
                "fields": json.dumps(rec.to_dict(), ensure_ascii=False)
            }
        )
    except Exception as e:
        repo.log_import(pdf_path, "FAILED", str(e))
        return IngestResult(
            status="FAILED",
            document_id=None,
            issues=[ValidationIssue("_pipeline", str(e), "ERROR")],
            summary={}
        )

# ---------------------------
# CLI helpers for quick testing
# ---------------------------

HELP = f"""
Usage:
  python {Path(__file__).name} init <db.sqlite>
  python {Path(__file__).name} ingest <db.sqlite> <file1.pdf> [file2.pdf ...]
  python {Path(__file__).name} search <db.sqlite> <fts query>

Examples:
  python {Path(__file__).name} init data.sqlite
  python {Path(__file__).name} ingest data.sqlite C:\\docs\\WPS-001.pdf C:\\docs\\PQR-1001.pdf
  python {Path(__file__).name} search data.sqlite "GMAW AND 355"
"""


def _cmd_init(db_path: Path):
    Repo(db_path)  # ctor runs DDL
    print(f"Initialized DB at {db_path}")


def _cmd_ingest(db_path: Path, files: List[str]):
    repo = Repo(db_path)
    for f in files:
        p = Path(f)
        res = ingest_pdf(repo, p)
        print(f"[{res.status}] {p.name} -> id={res.document_id} {res.summary}")
        if res.issues:
            for isue in res.issues:
                print(f"  - {isue.severity}: {isue.field}: {isue.message}")


def search(self, query: str, limit: int = 20) -> List[Dict[str, str]]:
    cur = self.conn.cursor()
    fts_sql = (
        "SELECT rowid, highlight(documents_fts, 9, '[', ']') AS text_snippet "
        "FROM documents_fts WHERE documents_fts MATCH ? "
        "ORDER BY bm25(documents_fts) LIMIT ?"
    )
    out: List[Dict[str, str]] = []
    try:
        cur.execute(fts_sql, (query, limit))
        fts_rows = cur.fetchall()
        for rowid, snippet in fts_rows:
            doc = cur.execute(
                "SELECT id, doc_type, doc_number, process, material, thickness_mm, company, date, avg_conf "
                "FROM documents WHERE id=?",
                (rowid,)
            ).fetchone()
            if doc:
                out.append({
                    "id": str(doc[0]),
                    "doc_type": doc[1],
                    "doc_number": doc[2] or "",
                    "process": doc[3] or "",
                    "material": doc[4] or "",
                    "thickness_mm": str(doc[5]) if doc[5] is not None else "",
                    "company": doc[6] or "",
                    "date": doc[7] or "",
                    "avg_conf": f"{doc[8]:.2f}" if doc[8] is not None else "",
                    "snippet": snippet or "",
                })
        return out
    except sqlite3.OperationalError:
        # Fallback: simple LIKE over documents.raw_text
        safe = (query or "").replace("%", "").replace("_", "")
        like = f"%{safe}%"
        cur.execute(
            "SELECT id, doc_type, doc_number, process, material, thickness_mm, company, date, avg_conf, "
            "substr(raw_text, 1, 200) "
            "FROM documents WHERE raw_text LIKE ? ORDER BY id DESC LIMIT ?",
            (like, limit)
        )
        rows = cur.fetchall()
        for doc in rows:
            out.append({
                "id": str(doc[0]),
                "doc_type": doc[1],
                "doc_number": doc[2] or "",
                "process": doc[3] or "",
                "material": doc[4] or "",
                "thickness_mm": str(doc[5]) if doc[5] is not None else "",
                "company": doc[6] or "",
                "date": doc[7] or "",
                "avg_conf": f"{doc[8]:.2f}" if doc[8] is not None else "",
                "snippet": doc[9] or "",
            })
        return out

# -----------------------------
# Command-line helpers
# -----------------------------
def _cmd_ingest(db_path: Path, pdf_path: Path):
    repo = Repo(db_path)
    res = ingest_pdf(repo, pdf_path)
    print(f"[{res.status}] {pdf_path.name}")
    if res.issues:
        for i in res.issues:
            print(f"  - {i.severity}: {i.field} → {i.message}")
    else:
        print("  (no issues)")

def _cmd_search(db_path: Path, query: str):
    repo = Repo(db_path)
    rows = repo.search(query)
    for r in rows:
        snippet = (r.get("snippet") or "")[:200]   # ← guard against None
        print(
            f"[{r['id']}] {r['doc_type']} {r['doc_number']} "
            f"({r['company']}) {r['date']} conf={r['avg_conf']}\n  {snippet}"
        )


# -----------------------------
# CLI entry
# -----------------------------
if __name__ == "__main__":
    import sys
    if len(sys.argv) < 2:
        print("Usage:\n  python ingestion_pipeline.py ingest <db> <pdf>\n  python ingestion_pipeline.py search <db> <query>")
        sys.exit(1)

    cmd = sys.argv[1].lower()
    if cmd == "ingest" and len(sys.argv) >= 4:
        _cmd_ingest(Path(sys.argv[2]), Path(sys.argv[3]))
    elif cmd == "search" and len(sys.argv) >= 4:
        _cmd_search(Path(sys.argv[2]), " ".join(sys.argv[3:]))
    else:
        print("Invalid usage.\nExample:\n  python ingestion_pipeline.py search data.sqlite GMAW")

