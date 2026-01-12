"""
WeldAdmin Pro - Import & Records (v6.1 Bulk + Integrated OCR + Drag & Drop)
----------------------------------------------------------------------------
Fix in this build:
- Safe fallback for tkinterdnd2 on Windows where TkinterDnD.Toplevel isn't exposed.
  We now use TkinterDnD.Tk for the main window (if available) and fall back to
  tk.Toplevel for dialogs when TkinterDnD.Toplevel is missing.

Features kept:
- Pattern-safe OCR (PyMuPDF preferred; pdf2image/pdftocairo fallback)
- Drag & Drop on Import tab (drop file)
- Drag & Drop in Bulk Import dialog (drop folder onto Folder box, or files into list)
- Bulk Import with progress bar + per-file status
- Search/Filter, Edit, Export, Delete, Auto-DB

Run:
    python import_wps_manager_v6_1_bulk_integrated_dnd.py
"""

import os
import json
import csv
import re
import sqlite3
from datetime import datetime
import tkinter as tk
from tkinter import ttk, filedialog, messagebox
from typing import Dict, List
from PIL import Image

# ----- Optional: Drag & Drop (tkinterdnd2) -----
HAVE_DND = False
DND_FILES = None
try:
    from tkinterdnd2 import DND_FILES as _DND_FILES, TkinterDnD
    HAVE_DND = True
    DND_FILES = _DND_FILES
except Exception:
    HAVE_DND = False

# Root/Toplevel class selection for DnD (safe cross-version handling)
if HAVE_DND:
    try:
        BaseTk = TkinterDnD.Tk  # drag-enabled root
        BaseToplevel = getattr(TkinterDnD, "Toplevel", tk.Toplevel)  # fallback if missing
    except Exception:
        BaseTk = tk.Tk
        BaseToplevel = tk.Toplevel
else:
    BaseTk = tk.Tk
    BaseToplevel = tk.Toplevel

# ---------------- Optional OCR dependencies ----------------
try:
    import pytesseract
except Exception:
    pytesseract = None

try:
    import pdfplumber
except Exception:
    pdfplumber = None

try:
    from pdf2image import convert_from_path
except Exception:
    convert_from_path = None

try:
    import fitz  # PyMuPDF
except Exception:
    fitz = None

# ---------------- Parser import (with safe fallback) ----------------
try:
    from parser_weldadmin import parse_fields as _parse_fields_ext, parse_weldtrace_layout as _parse_wt_ext  # type: ignore
    HAVE_EXT_PARSER = True
except Exception:
    HAVE_EXT_PARSER = False

def normalize_text(s: str) -> str:
    return re.sub(r"[^\S\r\n]+", " ", s or "").strip()

if not HAVE_EXT_PARSER:
    FIELD_PATTERNS = [
        ("type", r"\b(WPQR|PQR|WPS)\b"),
        ("code", r"\b(?:WPS|WPQR|PQR)[\s:\-]*([A-Za-z0-9_.\-\/]+)"),
        ("process", r"\b(Process|Welding\s*Process(?:es)?)\s*[:\-]\s*([A-Z0-9+\/\s,]+)"),
        ("material", r"\b(Base\s*Material|Material|Parent\s*Material)\s*[:\-]\s*([A-Za-z0-9\-\s\.]+)"),
        ("thickness", r"\b(Thickness|Range|Qualified\s*Range)\s*[:\-]\s*([0-9.,\-\smmMM]+)"),
        ("positions", r"\b(Position|Positions)\s*[:\-]\s*([0-9A-Za-z,\sGFOULR]+)"),
        ("filler", r"\b(Filler|Filler\s*Metal|Electrode)\s*[:\-]\s*([A-Za-z0-9\-\s\/]+)"),
        ("gas", r"\b(Shielding\s*Gas|Gas)\s*[:\-]\s*([A-Za-z0-9\-\s%\/]+)"),
        ("issue_date", r"\b(Issue\s*Date|Date\s*of\s*Issue|Qualified\s*Date)\s*[:\-]\s*([0-9]{4}[-/][0-9]{1,2}[-/][0-9]{1,2}|[0-9]{1,2}[-/][0-9]{1,2}[-/][0-9]{2,4})"),
        ("expiry_date", r"\b(Expiry\s*Date|Valid\s*Until|Expiration)\s*[:\-]\s*([0-9]{4}[-/][0-9]{1,2}[-/][0-9]{1,2}|[0-9]{1,2}[-/][0-9]{1,2}[-/][0-9]{2,4})"),
        ("welders", r"\b(Welder|Welder\s*Name[s]?)\s*[:\-]\s*([A-Za-z ,.\-]+)"),
    ]

    def _first_group(text: str, pattern: str, flags=re.IGNORECASE | re.MULTILINE):
        m = re.search(pattern, text, flags)
        return normalize_text(m.group(1)) if m else ""

    def parse_fields(text: str) -> Dict[str, str]:
        data: Dict[str, str] = {}
        for key, pattern in FIELD_PATTERNS:
            m = re.search(pattern, text, flags=re.IGNORECASE)
            if m:
                val = m.group(m.lastindex) if m.lastindex else m.group(0)
                data[key] = normalize_text(val)
        if "type" not in data:
            if re.search(r"\bWPS\b", text): data["type"] = "WPS"
            elif re.search(r"\bWPQR\b", text): data["type"] = "WPQR"
            elif re.search(r"\bPQR\b", text): data["type"] = "PQR"
        return data

    def parse_weldtrace_layout(text: str) -> Dict[str, str]:
        data: Dict[str, str] = {}
        if "WELDING PROCEDURE SPECIFICATION" in text.upper() or re.search(r"\bWPS\b", text):
            data["type"] = "WPS"
        code = _first_group(text, r"WPS\s*(?:Number|No\.?)\s*\n?\s*([A-Z0-9_\-\/\.]+)")
        if not code:
            code = _first_group(text, r"\bWPS[-\s]*([A-Z0-9][A-Z0-9_\-\/\.]+)")
        if code:
            data["code"] = code
        pqr = _first_group(text, r"PQR\s*(?:Number|No\.?)\s*\n?\s*([A-Z0-9_\-\/\.]+)")
        if pqr:
            data["pqr"] = pqr
        proc = _first_group(text, r"(GTAW|SMAW|GMAW|FCAW|SAW|TIG|MIG|MAG)")
        if proc:
            data["process"] = proc.upper()
        thick = _first_group(text, r"\bT\s*([0-9]+(?:\.[0-9]+)?)\s*[-–]\s*([0-9]+(?:\.[0-9]+)?)\s*mm")
        if not thick:
            thick = _first_group(text, r"Thickness.*?\b([0-9]+(?:\.[0-9]+)?)\s*[-–]\s*([0-9]+(?:\.[0-9]+)?)")
        if thick:
            data["thickness"] = thick
        filler = _first_group(text, r"\bAWS\s*(?:No\.|A\d\.\d)?\s*([A-Z0-9\-\/]*ER[0-9]+[A-Z0-9\-]*)")
        if filler:
            data["filler"] = filler.upper()
        pos = _first_group(text, r"\bPositions?\s*([A-Za-z0-9, \-]+)")
        if pos:
            data["positions"] = pos
        date1 = _first_group(text, r"WPS\s*(?:Number|No\.?).*?Date\s*([0-9]{1,2}[/-][0-9]{1,2}[/-][0-9]{2,4})", flags=re.IGNORECASE | re.DOTALL)
        date2 = _first_group(text, r"PQR\s*(?:Number|No\.?).*?Date\s*([0-9]{1,2}[/-][0-9]{1,2}[/-][0-9]{2,4})", flags=re.IGNORECASE | re.DOTALL)
        if date1:
            data["issue_date"] = date1
        if date2:
            data["expiry_date"] = date2
        return data
else:
    def parse_fields(text: str) -> Dict[str, str]:
        return _parse_fields_ext(text)

    def parse_weldtrace_layout(text: str) -> Dict[str, str]:
        return _parse_wt_ext(text)

# ---------------- Pattern-safe PDF rendering ----------------
def _render_with_pymupdf(path: str, max_pages: int = 3, dpi: int = 300) -> List[Image.Image]:
    if not fitz:
        return []
    imgs: List[Image.Image] = []
    try:
        doc = fitz.open(path)
        try:
            n = min(len(doc), max_pages)
            zoom = dpi / 72.0
            mat = fitz.Matrix(zoom, zoom)
            for i in range(n):
                page = doc.load_page(i)
                pix = page.get_pixmap(matrix=mat, alpha=False)
                img = Image.frombytes("RGB", (pix.width, pix.height), pix.samples)
                imgs.append(img)
        finally:
            doc.close()
    except Exception:
        pass
    return imgs

def _render_with_poppler(path: str, poppler_path: str = None, max_pages: int = 3, dpi: int = 300) -> List[Image.Image]:
    if not convert_from_path:
        return []
    try:
        images = convert_from_path(
            path, dpi=dpi, poppler_path=poppler_path, first_page=1, last_page=max_pages,
            fmt="png", thread_count=1, use_pdftocairo=True, grayscale=True
        )
        return images
    except Exception:
        try:
            images = convert_from_path(
                path, dpi=dpi, poppler_path=poppler_path, first_page=1, last_page=max_pages,
                fmt="png", thread_count=1, use_pdftocairo=True
            )
            return images
        except Exception:
            return []

def render_pdf_to_images(path: str, poppler_path: str = None, max_pages: int = 3, dpi: int = 300) -> List[Image.Image]:
    backend = (os.getenv("OCR_RASTER_BACKEND") or "").strip().lower()
    order = []
    if backend == "pymupdf":
        order = ["pymupdf", "poppler"]
    elif backend == "poppler":
        order = ["poppler", "pymupdf"]
    else:
        order = ["pymupdf", "poppler"]  # prefer PyMuPDF
    for b in order:
        imgs = _render_with_pymupdf(path, max_pages, dpi) if b == "pymupdf" else _render_with_poppler(path, poppler_path, max_pages, dpi)
        if imgs:
            return imgs
    return []

# ---------------- OCR + parsing ----------------
def set_tesseract_path(tesseract_path: str):
    global pytesseract
    if pytesseract and tesseract_path:
        pytesseract.pytesseract.tesseract_cmd = tesseract_path

def extract_text_from_pdf(path: str, poppler_path: str = None, use_ocr_if_needed: bool = True, max_pages_ocr: int = 3) -> str:
    text_parts: List[str] = []
    text = ""
    if pdfplumber:
        try:
            with pdfplumber.open(path) as pdf:
                for page in pdf.pages:
                    t = page.extract_text() or ""
                    if t.strip():
                        text_parts.append(t)
            text = "\n".join(text_parts).strip()
        except Exception:
            text = ""
    if not text and use_ocr_if_needed and pytesseract:
        images = render_pdf_to_images(path, poppler_path=poppler_path, max_pages=max_pages_ocr, dpi=300)
        if images:
            ocr_texts = []
            for img in images:
                try:
                    ocr_texts.append(pytesseract.image_to_string(img))
                except Exception:
                    pass
            text = "\n".join(ocr_texts)
    return normalize_text(text)

def extract_text_from_image(path: str) -> str:
    if not pytesseract:
        return ""
    try:
        img = Image.open(path)
        return normalize_text(pytesseract.image_to_string(img))
    except Exception:
        return ""

def extract_and_parse(path: str, tesseract_path: str = "", poppler_path: str = "") -> Dict[str, str]:
    set_tesseract_path(tesseract_path)
    lower = path.lower()
    if lower.endswith(".pdf"):
        text = extract_text_from_pdf(path, poppler_path=poppler_path, use_ocr_if_needed=True)
    elif any(lower.endswith(ext) for ext in (".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp")):
        text = extract_text_from_image(path)
    else:
        text = extract_text_from_pdf(path, poppler_path=poppler_path, use_ocr_if_needed=True) or extract_text_from_image(path)

    fields = parse_fields(text) if text else {}
    try:
        wt = parse_weldtrace_layout(text)
        for k, v in (wt or {}).items():
            if v and not fields.get(k):
                fields[k] = v
    except Exception:
        pass

    fields["_raw_text"] = text or ""
    return fields

# ---------------- App constants & DB helpers ----------------
APP_TITLE = "WeldAdmin Pro - Import & Records (v6.1 Bulk + DnD)"
DB_NAME = "weld_docs.db"
TABLE_NAME = "weld_documents"
ALLOWED_EXTS = (".pdf", ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp")

def ensure_db():
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        CREATE TABLE IF NOT EXISTS {TABLE_NAME}(
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            file_path TEXT,
            doc_type TEXT,
            doc_code TEXT,
            pqr TEXT,
            process TEXT,
            material TEXT,
            thickness TEXT,
            gas TEXT,
            filler TEXT,
            positions TEXT,
            issue_date TEXT,
            expiry_date TEXT,
            code_family TEXT,
            confidence TEXT,
            parser_excerpt TEXT,
            raw_text TEXT,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )
    """)
    cur.execute(f"CREATE INDEX IF NOT EXISTS idx_{TABLE_NAME}_path ON {TABLE_NAME}(file_path)")
    cur.execute(f"CREATE INDEX IF NOT EXISTS idx_{TABLE_NAME}_code ON {TABLE_NAME}(doc_code)")
    conn.commit()
    conn.close()

def _normalize_iso_date(s: str) -> str:
    s = (s or "").strip()
    if not s:
        return s
    try:
        dt = datetime.strptime(s, "%Y-%m-%d")
        return dt.strftime("%Y-%m-%d")
    except Exception:
        pass
    m = re.match(r"^(\d{1,2})[/-](\d{1,2})[/-](\d{2,4})$", s)
    if m:
        d, mo, y = int(m.group(1)), int(m.group(2)), int(m.group(3))
        if y < 100: y += 2000 if y < 70 else 1900
        try:
            return datetime(y, mo, d).strftime("%Y-%m-%d")
        except Exception:
            return s
    m = re.match(r"^(\d{4})[/-](\d{1,2})[/-](\d{1,2})$", s)
    if m:
        y, mo, d = int(m.group(1)), int(m.group(2)), int(m.group(3))
        try:
            return datetime(y, mo, d).strftime("%Y-%m-%d")
        except Exception:
            return s
    return s

def _normalize_thickness(s: str) -> str:
    if not s:
        return s
    s = s.strip().replace("–", "-").replace("—", "-")
    m = re.match(r"^\s*(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)(\s*mm)?\s*$", s, flags=re.I)
    if m:
        a, b = m.group(1), m.group(2)
        return f"{a}-{b} mm"
    return s

def save_to_db(file_path: str, data: dict):
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        INSERT INTO {TABLE_NAME}(
            file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
            positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text
        ) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)
    """, (
        file_path,
        data.get("type"),
        data.get("code"),
        data.get("pqr"),
        data.get("process"),
        data.get("material"),
        _normalize_thickness(data.get("thickness") or ""),
        data.get("gas"),
        data.get("filler"),
        data.get("positions"),
        _normalize_iso_date(data.get("issue_date") or ""),
        _normalize_iso_date(data.get("expiry_date") or ""),
        data.get("_parser_code_family") or data.get("code_family"),
        data.get("_parser_confidence") or data.get("confidence"),
        data.get("_parser_excerpt") or data.get("parser_excerpt"),
        data.get("_raw_text") or data.get("raw_text"),
    ))
    conn.commit()
    conn.close()

def record_exists(file_path: str) -> bool:
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"SELECT 1 FROM {TABLE_NAME} WHERE file_path = ? LIMIT 1", (file_path,))
    row = cur.fetchone()
    conn.close()
    return bool(row)

def update_record(row_id: int, payload: dict):
    ensure_db()
    fields = [
        "doc_type","doc_code","pqr","process","material","thickness","gas","filler",
        "positions","issue_date","expiry_date","code_family","confidence","parser_excerpt","raw_text"
    ]
    payload = dict(payload)
    payload["issue_date"] = _normalize_iso_date(payload.get("issue_date") or "")
    payload["expiry_date"] = _normalize_iso_date(payload.get("expiry_date") or "")
    payload["thickness"] = _normalize_thickness(payload.get("thickness") or "")
    sets = ", ".join([f"{k} = ?" for k in fields])
    values = [payload.get(k) for k in fields]
    values.append(row_id)
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"UPDATE {TABLE_NAME} SET {sets} WHERE id = ?", values)
    conn.commit()
    conn.close()

def fetch_record(row_id: int):
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        SELECT id, file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
               positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text, created_at
        FROM {TABLE_NAME} WHERE id = ?
    """, (row_id,))
    row = cur.fetchone()
    conn.close()
    return row

def query_records(doc_type="ALL", search_text="", limit=1000):
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    where = []
    params = []
    if doc_type and doc_type != "ALL":
        where.append("doc_type = ?")
        params.append(doc_type)
    if search_text:
        like = f"%{search_text}%"
        where.append("(" + " OR ".join([
            "doc_code LIKE ?","pqr LIKE ?","process LIKE ?","material LIKE ?","gas LIKE ?",
            "filler LIKE ?","positions LIKE ?","issue_date LIKE ?","expiry_date LIKE ?","code_family LIKE ?"
        ]) + ")")
        params.extend([like]*10)
    where_clause = "WHERE " + " AND ".join(where) if where else ""
    sql = f"""
        SELECT id, file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
               positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text, created_at
        FROM {TABLE_NAME}
        {where_clause}
        ORDER BY id DESC
        LIMIT ?
    """
    params.append(limit)
    cur.execute(sql, params)
    rows = cur.fetchall()
    conn.close()
    return rows

def delete_records(ids):
    if not ids:
        return
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    qmarks = ",".join(["?"]*len(ids))
    cur.execute(f"DELETE FROM {TABLE_NAME} WHERE id IN ({qmarks})", ids)
    conn.commit()
    conn.close()

# ---------------- Utilities for Drag & Drop ----------------
def _split_dnd_paths(widget, data: str):
    """
    Split a DND_FILES string into a list of paths.
    If tkinterdnd2 is available, use tk.splitlist; else do a best-effort split.
    """
    if HAVE_DND:
        try:
            return list(widget.tk.splitlist(data))
        except Exception:
            pass
    # Fallback: naive split, remove surrounding braces
    items = []
    buf = ""
    in_brace = False
    for ch in data:
        if ch == "{":
            in_brace = True
            buf = ""
        elif ch == "}":
            in_brace = False
            items.append(buf)
            buf = ""
        elif ch == " " and not in_brace:
            if buf:
                items.append(buf)
                buf = ""
        else:
            buf += ch
    if buf:
        items.append(buf)
    return [s.strip() for s in items if s.strip()]

# ---------------- GUI: Import tab ----------------
class ImportTab(ttk.Frame):
    VISIBLE_FIELDS = [
        ("type", "Type"),
        ("code", "Document Number / Code"),
        ("pqr", "Linked PQR"),
        ("process", "Process"),
        ("material", "Material"),
        ("thickness", "Thickness"),
        ("gas", "Gas"),
        ("filler", "Filler"),
        ("positions", "Positions"),
        ("issue_date", "Issue Date"),
        ("expiry_date", "Expiry Date"),
        ("_parser_code_family", "Code Family"),
        ("_parser_confidence", "Confidence"),
    ]

    def __init__(self, parent, on_saved=None):
        super().__init__(parent, padding=10)
        self.on_saved = on_saved
        self.file_path = tk.StringVar()
        self.data = {}
        self._build_ui()

    def _build_ui(self):
        # Top toolbar with open/import/save
        toolbar = ttk.Frame(self)
        toolbar.pack(fill="x", pady=(0, 8))

        ttk.Button(toolbar, text="Open", command=self.on_open).pack(side="left")
        ttk.Button(toolbar, text="Import", command=self.on_import).pack(side="left", padx=6)

        self.btn_save = ttk.Button(
            toolbar,
            text="Save (Add to DB)",
            command=self.on_save,
            state="disabled",
        )
        self.btn_save.pack(side="left")

        # Row: file picker
        row = ttk.Frame(self)
        row.pack(fill="x", pady=(2, 6))

        ttk.Label(row, text="Selected File:", width=14).pack(side="left")

        self.entry_path = ttk.Entry(row, textvariable=self.file_path)
        self.entry_path.pack(side="left", fill="x", expand=True, padx=6)

        ttk.Button(row, text="Browse…", command=self.on_open).pack(side="left")

        # Optional drag & drop
        if HAVE_DND:
            try:
                self.entry_path.drop_target_register(DND_FILES)
                self.entry_path.dnd_bind("<<Drop>>", self._on_drop_file)
            except Exception:
                # If DnD fails, just ignore – UI still works
                pass

        # Parsed fields grid
        grid = ttk.LabelFrame(self, text="Parsed Fields", padding=10)
        grid.pack(fill="both", expand=True)

        self.vars = {}
        for key, label in self.VISIBLE_FIELDS:
            r = ttk.Frame(grid)
            r.pack(fill="x", pady=3)

            ttk.Label(r, text=f"{label}:", width=24).pack(side="left")

            var = tk.StringVar()
            ttk.Entry(r, textvariable=var).pack(side="left", fill="x", expand=True)

            self.vars[key] = var

        # Short excerpt of parsed text
        self.txt_excerpt = tk.Text(self, height=5, wrap="word")
        self.txt_excerpt.pack(fill="x", pady=6)

        # Full raw text from OCR / PDF text layer
        self.txt_raw = tk.Text(self, height=10, wrap="word")
        self.txt_raw.pack(fill="both", expand=True)

    def _on_drop_file(self, event):
        files = _split_dnd_paths(self.entry_path, event.data)
        for fp in files:
            if os.path.splitext(fp)[1].lower() in ALLOWED_EXTS:
                self.file_path.set(fp)
                self.after(50, self.on_import)
                return
        messagebox.showinfo("Drag & Drop", "Please drop a PDF or image file.")

    def on_open(self):
        path = filedialog.askopenfilename(filetypes=[("PDF/Images", "*.pdf;*.png;*.jpg;*.jpeg;*.tif;*.bmp")])
        if path:
            self.file_path.set(path)

    def on_import(self):
        path = self.file_path.get()
        if not os.path.exists(path):
            messagebox.showerror("Error", "Select a valid file.")
            return
        try:
            self.data = extract_and_parse(path) or {}
            for k, _ in self.VISIBLE_FIELDS:
                self.vars[k].set(self.data.get(k, ""))
            self.txt_excerpt.delete("1.0", "end")
            self.txt_excerpt.insert("1.0", self.data.get("_parser_excerpt", "") or "")
            self.txt_raw.delete("1.0", "end")
            self.txt_raw.insert("1.0", (self.data.get("_raw_text", "") or "")[:1500])
            self.btn_save.config(state="normal")
        except Exception as e:
            messagebox.showerror("Import failed", str(e))

    def on_save(self):
        if not self.data:
            messagebox.showinfo("Nothing to save", "Import a file first.")
            return
        save_to_db(self.file_path.get(), self.data)
        messagebox.showinfo("Saved", "Record added to database.")
        if self.on_saved:
            self.on_saved()

# ---------------- GUI: Edit dialog ----------------
class EditDialog(BaseToplevel):
    FIELD_MAP = [
        ("doc_type","Type"),
        ("doc_code","Document Number / Code"),
        ("pqr","Linked PQR"),
        ("process","Process"),
        ("material","Material"),
        ("thickness","Thickness"),
        ("gas","Gas"),
        ("filler","Filler"),
        ("positions","Positions"),
        ("issue_date","Issue Date (YYYY-MM-DD)"),
        ("expiry_date","Expiry Date (YYYY-MM-DD)"),
        ("code_family","Code Family"),
        ("confidence","Confidence"),
    ]

    def __init__(self, parent, row_id: int, record_tuple, on_saved):
        super().__init__(parent)
        self.title(f"Edit Record #{row_id}")
        self.resizable(True, True)
        self.row_id = row_id
        self.on_saved = on_saved
        self.entries = {}
        self.vars = {}

        form = ttk.Frame(self, padding=10)
        form.pack(fill="both", expand=True)

        idx_map = {
            "doc_type":2, "doc_code":3, "pqr":4, "process":5, "material":6,
            "thickness":7, "gas":8, "filler":9, "positions":10, "issue_date":11,
            "expiry_date":12, "code_family":13, "confidence":14
        }
        for key, label in self.FIELD_MAP:
            row = ttk.Frame(form)
            row.pack(fill="x", pady=3)
            ttk.Label(row, text=label+":", width=28).pack(side="left")
            var = tk.StringVar(value=str(record_tuple[idx_map[key]] or ""))
            ent = ttk.Entry(row, textvariable=var)
            ent.pack(side="left", fill="x", expand=True)
            self.entries[key] = ent
            self.vars[key] = var

        meta = ttk.LabelFrame(form, text="Metadata", padding=8)
        meta.pack(fill="both", expand=True, pady=(8,0))
        ttk.Label(meta, text="Parser Excerpt:").pack(anchor="w")
        self.txt_excerpt = tk.Text(meta, height=4, wrap="word")
        self.txt_excerpt.pack(fill="x", pady=(0,6))
        self.txt_excerpt.insert("1.0", record_tuple[15] or "")
        ttk.Label(meta, text="Raw Text:").pack(anchor="w")
        self.txt_raw = tk.Text(meta, height=8, wrap="word")
        self.txt_raw.pack(fill="both", expand=True)
        self.txt_raw.insert("1.0", record_tuple[16] or "")

        btns = ttk.Frame(form)
        btns.pack(fill="x", pady=(8,0))
        ttk.Button(btns, text="Save Changes", command=self._save).pack(side="left")
        ttk.Button(btns, text="Cancel", command=self.destroy).pack(side="right")

        self.grab_set()
        self.focus_set()

    def _save(self):
        payload = {k: v.get().strip() for k, v in self.vars.items()}
        payload["parser_excerpt"] = self.txt_excerpt.get("1.0", "end").strip()
        payload["raw_text"] = self.txt_raw.get("1.0", "end").strip()
        update_record(self.row_id, payload)
        messagebox.showinfo("Saved", f"Record #{self.row_id} updated.")
        if self.on_saved:
            self.on_saved()
        self.destroy()

# ---------------- GUI: Records tab (with Bulk Import + DnD) ----------------
class RecordsTab(ttk.Frame):
    def __init__(self, parent):
        super().__init__(parent, padding=10)
        self._build_ui()
        self.refresh()

    def _build_ui(self):
        ctrl = ttk.Frame(self)
        ctrl.pack(fill="x")
        ttk.Label(ctrl, text="Type").pack(side="left")
        self.type_var = tk.StringVar(value="ALL")
        ttk.Combobox(ctrl, textvariable=self.type_var, values=["ALL","WPS","PQR","WPQR"], width=8).pack(side="left", padx=6)
        ttk.Label(ctrl, text="Search").pack(side="left")
        self.search_var = tk.StringVar()
        ttk.Entry(ctrl, textvariable=self.search_var, width=40).pack(side="left", padx=6)
        ttk.Button(ctrl, text="Apply", command=self.refresh).pack(side="left")
        ttk.Button(ctrl, text="Clear", command=self.clear_filters).pack(side="left", padx=6)

        cols = ("id","file_path","doc_type","doc_code","pqr","process","material","thickness","gas","filler",
                "positions","issue_date","expiry_date","code_family","confidence","parser_excerpt","raw_text","created_at")
        self.tree = ttk.Treeview(self, columns=cols, show="headings", height=18)
        self.tree.pack(fill="both", expand=True, pady=(8,6))

        headers = {
            "id":"ID","file_path":"File","doc_type":"Type","doc_code":"Code","pqr":"PQR","process":"Process",
            "material":"Material","thickness":"Thickness","gas":"Gas","filler":"Filler","positions":"Positions",
            "issue_date":"Issue Date","expiry_date":"Expiry Date","code_family":"Family","confidence":"Conf.",
            "parser_excerpt":"Excerpt","raw_text":"Raw Text","created_at":"Created"
        }
        widths = {
            "id":60,"file_path":220,"doc_type":80,"doc_code":160,"pqr":140,"process":110,"material":160,"thickness":120,
            "gas":160,"filler":120,"positions":120,"issue_date":110,"expiry_date":110,"code_family":100,"confidence":90,
            "parser_excerpt":180,"raw_text":200,"created_at":150
        }
        for c in cols:
            self.tree.heading(c, text=headers.get(c,c))
            self.tree.column(c, width=widths.get(c,120), anchor="w")

        btns = ttk.Frame(self)
        btns.pack(fill="x")
        ttk.Button(btns, text="Refresh", command=self.refresh).pack(side="left")
        ttk.Button(btns, text="Export Selected → JSON", command=self.export_selected_json).pack(side="left", padx=6)
        ttk.Button(btns, text="Export Selected → CSV", command=self.export_selected_csv).pack(side="left")
        ttk.Button(btns, text="Bulk Import…", command=self.open_bulk).pack(side="right")
        ttk.Button(btns, text="Edit Selected", command=self.edit_selected).pack(side="right", padx=6)
        ttk.Button(btns, text="Delete Selected", command=self.delete_selected).pack(side="right", padx=6)

        self.tree.bind("<Double-1>", self._on_double_click)

    def clear_filters(self):
        self.type_var.set("ALL")
        self.search_var.set("")
        self.refresh()

    def refresh(self):
        for i in self.tree.get_children():
            self.tree.delete(i)
        rows = query_records(self.type_var.get(), self.search_var.get(), limit=5000)
        for r in rows:
            self.tree.insert("", "end", iid=str(r[0]), values=r)

    def _sel_ids(self):
        return [int(i) for i in self.tree.selection()]

    def _rows_by_ids(self, ids):
        if not ids:
            return []
        ensure_db()
        conn = sqlite3.connect(DB_NAME)
        cur = conn.cursor()
        qmarks = ",".join(["?"]*len(ids))
        cur.execute(f"""
            SELECT id, file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
                   positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text, created_at
            FROM {TABLE_NAME}
            WHERE id IN ({qmarks})
            ORDER BY id DESC
        """, ids)
        rows = cur.fetchall()
        conn.close()
        return rows

    def export_selected_json(self):
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        rows = self._rows_by_ids(ids)
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON","*.json")])
        if not path:
            return
        out = []
        for r in rows:
            out.append({
                "id":r[0], "file_path":r[1], "doc_type":r[2], "doc_code":r[3], "pqr":r[4], "process":r[5],
                "material":r[6], "thickness":r[7], "gas":r[8], "filler":r[9], "positions":r[10],
                "issue_date":r[11], "expiry_date":r[12], "code_family":r[13], "confidence":r[14],
                "parser_excerpt":r[15], "raw_text":r[16], "created_at":r[17]
            })
        with open(path, "w", encoding="utf-8") as f:
            json.dump(out, f, ensure_ascii=False, indent=2)
        messagebox.showinfo("Exported", f"Saved JSON to:\n{path}")

    def export_selected_csv(self):
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        rows = self._rows_by_ids(ids)
        path = filedialog.asksaveasfilename(defaultextension=".csv", filetypes=[("CSV","*.csv")])
        if not path:
            return
        with open(path, "w", newline="", encoding="utf-8") as f:
            w = csv.writer(f)
            header = ["id","file_path","doc_type","doc_code","pqr","process","material","thickness","gas","filler",
                      "positions","issue_date","expiry_date","code_family","confidence","parser_excerpt","raw_text","created_at"]
            w.writerow(header)
            for r in rows:
                w.writerow([r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[15], r[16], r[17]])
        messagebox.showinfo("Exported", f"Saved CSV to:\n{path}")

    def delete_selected(self):
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        if not messagebox.askyesno("Confirm Delete", f"Delete {len(ids)} record(s)? This cannot be undone."):
            return
        delete_records(ids)
        self.refresh()

    def edit_selected(self):
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select a row to edit.")
            return
        row_id = ids[0]
        row = fetch_record(row_id)
        if not row:
            messagebox.showerror("Not found", "Record not found.")
            return
        EditDialog(self, row_id, row, on_saved=self.refresh)

    def _on_double_click(self, event):
        item = self.tree.identify_row(event.y)
        if not item:
            return
        try:
            row_id = int(item)
        except Exception:
            return
        row = fetch_record(row_id)
        if not row:
            return
        EditDialog(self, row_id, row, on_saved=self.refresh)

    def open_bulk(self):
        BulkImportDialog(self, on_done=self.refresh)

# ---------------- GUI: Bulk Import dialog (DnD enabled) ----------------
class BulkImportDialog(BaseToplevel):
    def __init__(self, parent, on_done=None):
        super().__init__(parent)
        self.title("Bulk Import WPS/PQR/WPQR")
        self.geometry("780x560")
        self.resizable(True, True)
        self.on_done = on_done

        self.dir_var = tk.StringVar()
        self.skip_dups = tk.BooleanVar(value=True)
        self.files = []

        row = ttk.Frame(self, padding=8)
        row.pack(fill="x")
        ttk.Label(row, text="Folder:").pack(side="left")
        self.entry_dir = ttk.Entry(row, textvariable=self.dir_var)
        self.entry_dir.pack(side="left", fill="x", expand=True, padx=6)
        ttk.Button(row, text="Browse…", command=self.choose_dir).pack(side="left")

        if HAVE_DND:
            try:
                self.entry_dir.drop_target_register(DND_FILES)
                self.entry_dir.dnd_bind("<<Drop>>", self._on_drop_folder)
            except Exception:
                pass

        opt = ttk.Frame(self, padding=(8,0))
        opt.pack(fill="x")
        ttk.Checkbutton(opt, text="Skip if file already imported (exact path)", variable=self.skip_dups).pack(side="left")

        lst = ttk.LabelFrame(self, text="Files to import (drop files here too)", padding=8)
        lst.pack(fill="both", expand=True, padx=8, pady=8)

        self.tree = ttk.Treeview(lst, columns=("path","status"), show="headings", height=14)
        self.tree.heading("path", text="Path")
        self.tree.heading("status", text="Status")
        self.tree.column("path", width=560, anchor="w")
        self.tree.column("status", width=140, anchor="w")
        self.tree.pack(fill="both", expand=True)

        if HAVE_DND:
            try:
                self.tree.drop_target_register(DND_FILES)
                self.tree.dnd_bind("<<Drop>>", self._on_drop_files)
            except Exception:
                pass

        p = ttk.Frame(self, padding=(8,0))
        p.pack(fill="x")
        self.pb = ttk.Progressbar(p, maximum=100)
        self.pb.pack(fill="x", expand=True)
        self.lbl_prog = ttk.Label(p, text="0 / 0")
        self.lbl_prog.pack(anchor="e")

        btns = ttk.Frame(self, padding=8)
        btns.pack(fill="x")
        ttk.Button(btns, text="Scan Folder", command=self.scan_folder).pack(side="left")
        self.btn_start = ttk.Button(btns, text="Start Import", command=self.start_import, state="disabled")
        self.btn_start.pack(side="left", padx=6)
        ttk.Button(btns, text="Close", command=self.destroy).pack(side="right")

        tip = "Tip: Drag a folder to the Folder box; or drop files onto the list." if HAVE_DND else "Tip: Install 'tkinterdnd2' for drag & drop (pip install tkinterdnd2)."
        ttk.Label(self, text=tip).pack(anchor="w", padx=10, pady=(0,8))

        self.grab_set()
        self.focus_set()

    def _on_drop_folder(self, event):
        items = _split_dnd_paths(self.entry_dir, event.data)
        for pth in items:
            if os.path.isdir(pth):
                self.dir_var.set(pth)
                return
        messagebox.showinfo("Drag & Drop", "Please drop a folder onto the Folder box.")

    def _on_drop_files(self, event):
        items = _split_dnd_paths(self.tree, event.data)
        allowed = [p for p in items if os.path.splitext(p)[1].lower() in ALLOWED_EXTS and os.path.exists(p)]
        if not allowed:
            messagebox.showinfo("Drag & Drop", "Drop PDF/image files here to queue them.")
            return
        for fp in allowed:
            self.files.append(fp)
            self.tree.insert("", "end", values=(fp, "Queued"))
        self.btn_start.config(state="normal")
        self.pb['value'] = 0
        self.lbl_prog.config(text=f"0 / {len(self.files)}")

    def choose_dir(self):
        d = filedialog.askdirectory()
        if d:
            self.dir_var.set(d)

    def scan_folder(self):
        root = self.dir_var.get().strip()
        if not root or not os.path.isdir(root):
            messagebox.showerror("Folder", "Please choose a valid folder.")
            return
        self.tree.delete(*self.tree.get_children())
        self.files = []
        for dirpath, _, filenames in os.walk(root):
            for fn in filenames:
                if os.path.splitext(fn)[1].lower() in ALLOWED_EXTS:
                    path = os.path.join(dirpath, fn)
                    self.files.append(path)
        if not self.files:
            messagebox.showinfo("No files", "No PDF or image files found.")
            self.btn_start.config(state="disabled")
            self.pb['value'] = 0
            self.lbl_prog.config(text="0 / 0")
            return

        for fp in self.files:
            self.tree.insert("", "end", values=(fp, "Queued"))
        self.btn_start.config(state="normal")
        self.pb['value'] = 0
        self.lbl_prog.config(text=f"0 / {len(self.files)}")

    def start_import(self):
        if not self.files:
            return
        self.btn_start.config(state="disabled")
        total = len(self.files)
        done = 0
        for iid, fp in zip(self.tree.get_children(), self.files):
            if self.skip_dups.get() and record_exists(fp):
                self.tree.set(iid, "status", "Skipped (duplicate)")
                done += 1
                self.pb['value'] = (done / total) * 100
                self.lbl_prog.config(text=f"{done} / {total}")
                self.update_idletasks()
                continue

            try:
                data = extract_and_parse(fp) or {}
                save_to_db(fp, data)
                self.tree.set(iid, "status", "Imported")
            except Exception as e:
                self.tree.set(iid, "status", f"Error: {e}")
            done += 1
            self.pb['value'] = (done / total) * 100
            self.lbl_prog.config(text=f"{done} / {total}")
            self.update_idletasks()

        messagebox.showinfo("Bulk Import", f"Completed: {done} file(s).")
        if self.on_done:
            self.on_done()

# ---------------- Main app ----------------
class MainApp(BaseTk):
    def __init__(self):
        super().__init__()
        self.title(APP_TITLE)
        self.geometry("1280x800")
        ensure_db()
        self.tabs = ttk.Notebook(self)
        self.tabs.pack(fill="both", expand=True)
        self.import_tab = ImportTab(self.tabs, on_saved=self.on_saved_record)
        self.records_tab = RecordsTab(self.tabs)
        self.tabs.add(self.import_tab, text="Import")
        self.tabs.add(self.records_tab, text="Records")

    def on_saved_record(self):
        self.records_tab.refresh()

if __name__ == "__main__":
    app = MainApp()
    app.mainloop()
