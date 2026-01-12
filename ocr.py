"""
OCR utilities (pattern-safe) for WeldAdmin Pro
---------------------------------------------
Fixes noisy Poppler "PatternX invalid float" warnings by:
- Preferring PyMuPDF (fitz) rasterization when installed
- Using pdf2image with pdftocairo backend and grayscale fallback
- Allowing backend override via env var OCR_RASTER_BACKEND = pymupdf|poppler

Public API:

    from ocr import extract_and_parse
    fields = extract_and_parse(r"C:\path\to\your\file.pdf")

Returns a dictionary of parsed fields (WPS/PQR/WPQ) plus raw text.
"""

import os
import re
from typing import Dict, List, Optional, Any

from PIL import Image, ImageFilter, ImageOps

# Optional imports
try:
    import pytesseract  # type: ignore
except Exception:  # pragma: no cover
    pytesseract = None

try:
    import pdfplumber  # type: ignore
except Exception:  # pragma: no cover
    pdfplumber = None

try:
    from pdf2image import convert_from_path  # type: ignore
except Exception:  # pragma: no cover
    convert_from_path = None

try:
    import fitz  # PyMuPDF  # type: ignore
except Exception:  # pragma: no cover
    fitz = None


# -----------------------------
# Small helpers
# -----------------------------

def _first_match(pattern: str, text: str, group: int = 1, flags: int = re.IGNORECASE) -> str:
    """Return the first regex group match or empty string."""
    m = re.search(pattern, text, flags)
    return m.group(group).strip() if m else ""


def _find_likely_thickness_mm(raw: str) -> str:
    """
    Heuristic to guess base material thickness in mm from the text.

    Strategy:
      1) Prefer numbers written like 'NN mm' or 'NN.N mm'
      2) If none, look for decimal numbers (e.g. 15.24) <= 100
         and pick the largest one.
    """
    mm_candidates: List[float] = []
    for m in re.finditer(r'(\d{1,3}(?:[.,]\d{1,2})?)\s*mm', raw, re.IGNORECASE):
        try:
            num = float(m.group(1).replace(",", "."))
            mm_candidates.append(num)
        except ValueError:
            continue
    if mm_candidates:
        return str(max(mm_candidates))

    decimal_candidates: List[float] = []
    for m in re.finditer(r'\b(\d{1,2}[.,]\d{1,2})\b', raw):
        try:
            num = float(m.group(1).replace(",", "."))
        except ValueError:
            continue
        if 0.5 <= num <= 100.0:
            decimal_candidates.append(num)

    if decimal_candidates:
        return str(max(decimal_candidates))

    return ""


# -----------------------------
# PQR / WPQ parsing helpers
# -----------------------------

def _parse_pqr_from_text(raw: str, base_fields: Dict[str, Any]) -> Dict[str, Any]:
    """
    Extract PQR/WPQ-specific fields from raw OCR text.

    Targets fields for a PQR/WPQ tab:
      - pqr_number / wpq_number
      - wps_number
      - code_standard
      - pqr_date / wpq_date
      - wps_date
      - process
      - position
      - joint_type
      - base_material_spec
      - base_material_thickness_mm
      - welder_name
      - welder_id
      - stamp_number
      - test_lab
      - test_report_no
    """
    out: Dict[str, Any] = {}

    lines = [ln.strip() for ln in raw.splitlines() if ln.strip()]
    upper_lines = [ln.upper() for ln in lines]

    # ------------ PQR NUMBER + DATE ------------
    pqr_candidates: List[str] = []
    for ln, up in zip(lines, upper_lines):
        if "PQR" in up and "ASME BPVC SEC" not in up:
            pqr_candidates.append(ln)

    pqr_line = ""
    if pqr_candidates:
        preferred = [
            ln for ln in pqr_candidates
            if re.search(r'\b(REV|VER|DATE)\b', ln, re.IGNORECASE)
        ]
        pqr_line = preferred[0] if preferred else pqr_candidates[0]

    if pqr_line:
        date = _first_match(
            r'(?:Date\s*)?(\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2})',
            pqr_line,
        )
        if date:
            out["pqr_date"] = date

        tmp = re.sub(
            r'^\s*PQR\s*(?:No\.?|Number)?\s*[:\-]?\s*',
            '',
            pqr_line,
            flags=re.IGNORECASE,
        )
        core = re.split(r'\bRev\b|\bVer\b|\bDate\b', tmp, flags=re.IGNORECASE)[0]
        out["pqr_number"] = core.strip()

    # ------------ WPS NUMBER + DATE ------------
    wps_candidates: List[str] = []
    for ln, up in zip(lines, upper_lines):
        if "WPS" in up and "ASME BPVC SEC" not in up:
            wps_candidates.append(ln)

    wps_line = ""
    if wps_candidates:
        preferred = [
            ln for ln in wps_candidates
            if re.search(r'\b(REV|VER|DATE)\b', ln, re.IGNORECASE)
        ]
        wps_line = preferred[0] if preferred else wps_candidates[0]

    if wps_line:
        date = _first_match(
            r'(?:Date\s*)?(\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2})',
            wps_line,
        )
        if date:
            out["wps_date"] = date

        tmp = re.sub(
            r'^\s*WPS\s*(?:No\.?|Number)?\s*[:\-]?\s*',
            '',
            wps_line,
            flags=re.IGNORECASE,
        )
        core = re.split(r'\bRev\b|\bVer\b|\bDate\b', tmp, flags=re.IGNORECASE)[0]
        out["wps_number"] = core.strip()

    # If we still don't have pqr_date, infer from dates
    if not out.get("pqr_date"):
        all_dates = re.findall(r'(\d{2}/\d{2}/\d{4})', raw)
        uniq_dates = sorted(set(all_dates))
        if len(uniq_dates) == 2 and out.get("wps_date") in uniq_dates:
            other = [d for d in uniq_dates if d != out["wps_date"]]
            if other:
                out["pqr_date"] = other[0]

    # ------------ CODE / STANDARD ------------
    if base_fields.get("code_standard"):
        out["code_standard"] = base_fields["code_standard"]

    # ------------ PROCESS ------------
    process = _first_match(
        r'\b(?:Process|Welding\s*Process)\s*[:\-]?\s*([A-Z0-9 /,+]+)',
        raw,
    )
    if not process:
        process = _first_match(r'(GTAW|SMAW|GMAW|FCAW|SAW)', raw)
    if process:
        out["process"] = process.strip(" .,")

    # ------------ POSITION ------------
    position = _first_match(
        r'\b(?:Test\s*Position|Welding\s*Position|Position)\s*[:\-]?\s*([0-9A-Z/ ]{1,10})',
        raw,
    )
    if position:
        out["position"] = position.strip()

    # ------------ JOINT TYPE ------------
    joint = _first_match(
        r'\bJoint\s*Type\s*[:\-]?\s*([A-Za-z0-9 /\-]+)',
        raw,
    )
    if joint:
        out["joint_type"] = joint.strip()

    # ------------ BASE MATERIAL SPEC ------------
    base_spec = _first_match(
        r'\b(?:Base|Parent)\s*(?:Material|Metal)\s*(?:Spec(?:ification)?|Grade|Type)?\s*[:\-]?\s*([A-Za-z0-9 /,\-]+)',
        raw,
    )
    if not base_spec:
        m = re.search(
            r'(?:Base|Parent)\s*(?:Material|Metal)[^\n]*',
            raw,
            re.IGNORECASE,
        )
        if m:
            line = m.group(0)
            line = re.sub(
                r'(?:Base|Parent)\s*(?:Material|Metal)\s*(?:Spec(?:ification)?|Grade|Type)?\s*[:\-]?',
                "",
                line,
                flags=re.IGNORECASE,
            )
            base_spec = line.strip(" :-")

    if base_spec and len(base_spec.strip()) <= 2:
        m = re.search(r'(?:SA\s*)?304L', raw, re.IGNORECASE)
        if m:
            base_spec = m.group(0)

    if base_spec:
        out["base_material_spec"] = base_spec.strip()

    # ------------ BASE MATERIAL THICKNESS ------------
    base_thk = _first_match(
        r'\b(?:Base|Parent)\s*(?:Material|Metal)\s*Thickness\s*[:\-]?\s*([0-9.,]+)',
        raw,
    )
    if not base_thk:
        base_thk = _first_match(
            r'\bThickness\s*(?:of\s*(?:Test\s*Coupon|Test\s*Piece|Test\s*Plate))?\s*[:\-]?\s*([0-9.,]+)\s*mm',
            raw,
        )
    if not base_thk:
        base_thk = _find_likely_thickness_mm(raw)

    if base_thk:
        base_thk = base_thk.replace(",", ".")
        out["base_material_thickness_mm"] = base_thk

    # ------------ WELDER INFO ------------
    welder_name = _first_match(
        r'\bWelder(?:\'s)?\s*(?:Name)?\s*[:\-]?\s*([A-Za-z][A-Za-z .\-]{2,})',
        raw,
    )
    if welder_name:
        welder_name = re.sub(r'\bWelder\s*ID\b.*$', '', welder_name, flags=re.IGNORECASE)
        out["welder_name"] = welder_name.strip(" :-")

    welder_id = _first_match(
        r'\bWelder\s*(?:ID|No\.?|Number)?\s*[:\-]?\s*([A-Za-z0-9\-]+)',
        raw,
    )
    if not welder_id and base_fields.get("welder_id"):
        welder_id = str(base_fields["welder_id"])
    if welder_id:
        out["welder_id"] = welder_id.strip()

    if ("welder_id" in out) and ("welder_name" not in out):
        mid = re.escape(out["welder_id"])
        m = re.search(
            rf'([A-Z][a-z]+(?:\s+[A-Z][a-z]+){{0,2}})\s+{mid}',
            raw,
        )
        if m:
            out["welder_name"] = m.group(1).strip()

    # ------------ STAMP NUMBER ------------
    stamp = _first_match(
        r'\bStamp\s*(?:No\.?|Number)?\s*[:\-]?\s*([A-Za-z0-9\-]+)',
        raw,
    )
    if stamp:
        out["stamp_number"] = stamp.strip()

    # ------------ LAB / REPORT ------------
    lab = _first_match(
        r'\b(?:Testing\s*Laboratory|Test\s*Lab|Laboratory)\s*[:\-]?\s*([A-Za-z0-9 .,&\-]+)',
        raw,
    )
    if lab:
        out["test_lab"] = lab.strip()

    report_no = _first_match(
        r'\bTest\s*Report\s*(?:No\.?|Number)?\s*[:\-]?\s*([A-Za-z0-9\-]+)',
        raw,
    )
    if report_no:
        out["test_report_no"] = report_no.strip()

    out = {k: v for k, v in out.items() if v}
    return out


# -----------------------------
# Low-level helpers (PDF → text)
# -----------------------------

def _ensure_grayscale(img: Image.Image) -> Image.Image:
    """Convert an image to grayscale; no-op if already L."""
    if img.mode != "L":
        img = ImageOps.grayscale(img)
    return img


def _preprocess_for_ocr(img: Image.Image) -> Image.Image:
    """Basic pre-processing: grayscale + sharpen."""
    img = _ensure_grayscale(img)
    img = img.filter(ImageFilter.SHARPEN)
    return img


def _ocr_image(img: Image.Image, lang: str = "eng") -> str:
    """Run Tesseract OCR on a PIL image."""
    if not pytesseract:
        raise RuntimeError("pytesseract is not installed / not importable")
    config = "--psm 6"
    text = pytesseract.image_to_string(img, lang=lang, config=config)
    return text or ""


def _extract_text_pdfplumber(pdf_path: str) -> str:
    """Try text extraction with pdfplumber (no OCR)."""
    if not pdfplumber:
        return ""
    try:
        with pdfplumber.open(pdf_path) as pdf:
            parts: List[str] = []
            for page in pdf.pages:
                page_text = page.extract_text() or ""
                parts.append(page_text)
        return "\n\n".join(parts).strip()
    except Exception:
        return ""


def _pdf_to_images_pymupdf(pdf_path: str, dpi: int = 300) -> List[Image.Image]:
    """Rasterize PDF pages using PyMuPDF if available."""
    if not fitz:
        return []
    images: List[Image.Image] = []
    try:
        with fitz.open(pdf_path) as doc:
            for page_index in range(len(doc)):
                page = doc.load_page(page_index)
                zoom = dpi / 72.0
                mat = fitz.Matrix(zoom, zoom)
                pix = page.get_pixmap(matrix=mat, alpha=False)
                img = Image.frombytes("RGB", (pix.width, pix.height), pix.samples)
                images.append(img)
    except Exception:
        return []
    return images


def _pdf_to_images_poppler(pdf_path: str, dpi: int = 300) -> List[Image.Image]:
    """Rasterize PDF pages using pdf2image + Poppler."""
    if not convert_from_path:
        return []
    try:
        images = convert_from_path(
            pdf_path,
            dpi=dpi,
            fmt="ppm",
            grayscale=True,
        )
        return images
    except Exception:
        return []


def _pdf_to_images(pdf_path: str, dpi: int = 300) -> List[Image.Image]:
    """Convert a PDF into a list of PIL images."""
    backend_override = os.environ.get("OCR_RASTER_BACKEND", "").strip().lower()

    if backend_override in ("pymupdf", "") and fitz:
        imgs = _pdf_to_images_pymupdf(pdf_path, dpi=dpi)
        if imgs:
            return imgs

    if backend_override in ("poppler", "") and convert_from_path:
        imgs = _pdf_to_images_poppler(pdf_path, dpi=dpi)
        if imgs:
            return imgs

    return []


def _ocr_pdf_pages(pdf_path: str, max_pages: Optional[int] = 3) -> str:
    """OCR the first N pages of the PDF, returning concatenated text."""
    images = _pdf_to_images(pdf_path, dpi=300)
    if not images:
        return ""
    texts: List[str] = []
    for i, img in enumerate(images):
        if max_pages is not None and i >= max_pages:
            break
        pre = _preprocess_for_ocr(img)
        page_text = _ocr_image(pre)
        texts.append(page_text)
    return "\n\n".join(texts).strip()


# -----------------------------
# Generic field parsing
# -----------------------------

def parse_fields(text: str) -> Dict[str, str]:
    """
    Extract common WPS / PQR / WPQ fields from the given OCR text.
    This is a heuristic parser; adjust regexes as needed for your templates.
    """
    fields: Dict[str, str] = {}
    t = text or ""

    # --- Document type detection (header-based, then fallback) ---
    if re.search(r'WELDING\s+PROCEDURE\s+SPECIFICATION', t, re.IGNORECASE):
        fields["doc_type"] = "WPS"
        fields["type"] = "wps"

    if re.search(r'PROCEDURE\s+QUALIFICATION\s+RECORD', t, re.IGNORECASE):
        fields["doc_type"] = "PQR"
        fields["type"] = "pqr"

    if re.search(r'WELDER\s+PERFORMANCE\s+QUALIFICATION', t, re.IGNORECASE):
        fields["doc_type"] = "WPQ"
        fields["type"] = "wpq"

    if "doc_type" not in fields:
        if re.search(r'\bWPQ\b|\bWPQR\b', t):
            fields["doc_type"] = "WPQ"
            fields["type"] = "wpq"
        elif re.search(r'\bPQR\b', t):
            fields["doc_type"] = "PQR"
            fields["type"] = "pqr"
        elif re.search(r'\bWPS\b', t):
            fields["doc_type"] = "WPS"
            fields["type"] = "wps"

    # --- Company name (line after WPS header) ---
    m = re.search(
        r'WELDING\s+PROCEDURE\s+SPECIFICATION[^\n]*\n([^\n]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["company_name"] = m.group(1).strip()

    # --- Designation line ---
    m = re.search(r'\bDesignation\s+(.+)', t, re.IGNORECASE)
    if m:
        fields["designation"] = m.group(1).strip()

    # --- Code / Construction code line ---
    m = re.search(
        r'Code/Standard\s+(.+?)\s+Constr\.?\s*Code\s+(.+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["code_standard"] = m.group(1).strip()
        fields["construction_code"] = m.group(2).strip()

    # --- Improved WPS number + date extraction (matches WeldTrace style) ---
    m = re.search(
        r'WPS\s*Number\s*([A-Z0-9\-\/]+).*?Rev/?\s*Ver\s*([0-9]+).*?Date\s*(\d{2}/\d{2}/\d{4})',
        t,
        re.IGNORECASE | re.DOTALL,
    )
    if m:
        fields["wps_number"] = m.group(1).strip()
        fields["wps_rev"] = m.group(2).strip()
        fields["wps_date"] = m.group(3).strip()

    # --- Improved PQR number + date extraction ---
    m = re.search(
        r'PQR\s*Number\s*([A-Z0-9\-\/]+).*?Rev/?\s*Ver\s*([0-9]+).*?Date\s*(\d{2}/\d{2}/\d{4})',
        t,
        re.IGNORECASE | re.DOTALL,
    )
    if m:
        fields["pqr_number"] = m.group(1).strip()
        fields["pqr_rev"] = m.group(2).strip()
        fields["pqr_date"] = m.group(3).strip()

    # --- Generic fallbacks if still missing ---
    if "wps_number" not in fields:
        m = re.search(
            r'\bWPS\s*(?:No\.?|Number)?\s*[:\-]?\s*([A-Z0-9][A-Z0-9\-/ ]+)',
            t,
            re.IGNORECASE,
        )
        if m:
            fields["wps_number"] = m.group(1).strip()

    if "pqr_number" not in fields:
        m = re.search(
            r'\bPQR\s*(?:No\.?|Number)?\s*[:\-]?\s*((?!ASME)[A-Z0-9][A-Z0-9\-/ ]+)',
            t,
            re.IGNORECASE,
        )
        if m:
            fields["pqr_number"] = m.group(1).strip()

    # --- Thickness & OD ranges (WPS) ---
    m = re.search(
        r'Thickness,\s*T\s*\(mm\)\s*([0-9.,]+\s*[–\-]\s*[0-9.,]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["thickness_range_mm"] = m.group(1).strip()

    m = re.search(
        r'Outside\s*Diameter\s*\(mm\)\s*(.+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["outside_diameter_range"] = m.group(1).strip()

    # --- Joint section ---
    m = re.search(r'Joint\s*Type\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["joint_type"] = m.group(1).strip()

    m = re.search(r'Joint\s*Design\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["joint_design"] = m.group(1).strip()

    m = re.search(r'Surface\s*Preparation\s*Method\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["surface_prep"] = m.group(1).strip()

    m = re.search(r'Groove\s*Angle[°]?\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["groove_angle"] = m.group(1).strip()

    m = re.search(r'Root\s*Face\s*\(mm\)\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["root_face_mm"] = m.group(1).strip()

    m = re.search(r'Root\s*Gap\s*\(mm\)\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["root_gap_mm"] = m.group(1).strip()

    m = re.search(r'Max\.\s*misalignment\s*\(mm\)\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["max_misalignment_mm"] = m.group(1).strip()

    m = re.search(r'Back\s*Gouging\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["back_gouging"] = m.group(1).strip()

    m = re.search(r'\bBacking\s+([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["backing"] = m.group(1).strip()

    # Backing type often appears as "Backing\nType Machining & Grinding"
    m = re.search(r'Backing\s*[\r\n]+Type\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["backing_type"] = m.group(1).strip()

    # --- Process & type ---
    m = re.search(r'\bPROCESS\s+([A-Z0-9 /,+]+)', t, re.IGNORECASE)
    if m:
        fields["process"] = m.group(1).strip(" .,")

    m = re.search(r'\bType\s+([A-Za-z]+)', t, re.IGNORECASE)
    if m:
        fields["process_type"] = m.group(1).strip()

    # --- Base metals (very WeldTrace-specific, first two rows) ---
    base_rows = re.findall(
        r'Steel\s*&\s*steel\s*alloy\s+Pipe\s+([A/0-9A-Z\- ,]+)\s+[0-9]+\s+[0-9]+\s+[A-Z0-9]+',
        t,
        re.IGNORECASE,
    )
    if base_rows:
        fields["base_material_1_spec"] = base_rows[0].strip()
        if len(base_rows) > 1:
            fields["base_material_2_spec"] = base_rows[1].strip()

    # --- Shielding / backing gas ---
    m = re.search(r'Shielding\s*Gas\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["shielding_gas"] = m.group(1).strip()

    m = re.search(r'Backing\s*Gas\s*([^\n]+)', t, re.IGNORECASE)
    if m:
        fields["backing_gas"] = m.group(1).strip()

    # --- Preheat / interpass ---
    m = re.search(r'Preheat\s*Temp\.\s*Min\s*\(°C\)\s*([0-9.,]+)', t, re.IGNORECASE)
    if m:
        fields["preheat_min_c"] = m.group(1).strip()

    m = re.search(r'Interpass\s*Temp\.\s*Max\s*\(°C\)\s*([0-9.,]+)', t, re.IGNORECASE)
    if m:
        fields["interpass_max_c"] = m.group(1).strip()

    # --- Amps / Volts / Travel speed / Heat input ---
    m = re.search(r'Amps\s*range,\s*A\s*([0-9–\- ]+)', t, re.IGNORECASE)
    if m:
        fields["amps_range"] = m.group(1).strip()

    m = re.search(r'Volts\s*range,\s*V\s*([0-9–\- ]+)', t, re.IGNORECASE)
    if m:
        fields["volts_range"] = m.group(1).strip()

    m = re.search(
        r'Travel\s*speed,\s*\(mm/min\)\s*([0-9–\- ]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["travel_speed_range_mm_min"] = m.group(1).strip()

    m = re.search(
        r'Max\.\s*Heat\s*input,\s*\(kJ/mm\)\s*([0-9., –\-]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["max_heat_input_kj_mm"] = m.group(1).strip()

    # --- Fallback date scan if needed ---
    if "wps_date" not in fields or "pqr_date" not in fields:
        dates = re.findall(r'\d{2}/\d{2}/\d{4}', t)
        if dates:
            if "wps_date" not in fields:
                fields["wps_date"] = dates[0]
            if "pqr_date" not in fields and len(dates) > 1:
                fields["pqr_date"] = dates[-1]

    # --- WPQ / WPQR specific fields ---

    # Certificate number
    m = re.search(
        r'Certificate\s*(?:No\.?|Number)?\s*[:\-]?\s*([A-Za-z0-9\-\/]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["certificate_no"] = m.group(1).strip()

    # WPQ / Welder Performance Qualification Record number
    m = re.search(
        r'(?:WPQ\s*Record|Welder\s*Performance\s*Qualification\s*Record)\s*(?:No\.?|Number)?\s*[:\-]?\s*([A-Za-z0-9\-\/]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["wpq_record_no"] = m.group(1).strip()

    # Qualified To (e.g. process / range / position)
    m = re.search(
        r'Qualified\s*To\s*[:\-]?\s*([A-Za-z0-9 /,\-]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["qualified_to"] = m.group(1).strip()

    # Test date (date of test)
    m = re.search(
        r'(?:Test\s*Date|Date\s*of\s*Test)\s*[:\-]?\s*(\d{2}/\d{2}/\d{4})',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["test_date"] = m.group(1).strip()

    # Date issued
    m = re.search(
        r'(?:Date\s*Issued|Date\s*of\s*Issue)\s*[:\-]?\s*(\d{2}/\d{2}/\d{4})',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["date_issued"] = m.group(1).strip()

    # Job knowledge (often a short phrase / rating)
    m = re.search(
        r'Job\s*Knowledge\s*[:\-]?\s*([A-Za-z0-9 ,]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["job_knowledge"] = m.group(1).strip()

    # --- WPQ / WPQR specific fields ---

    # Certificate number (often "Certificate No." or "Certificate Number")
    m = re.search(
        r'Certificate\s*(?:No\.?|Number)?\s*[:\-]?\s*([A-Za-z0-9\-\/]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["certificate_no"] = m.group(1).strip()

    # WPQ / WPQR record number (handles "WPQ Record No", "WPQR No", etc.)
    m = re.search(
        r'(?:WPQ|WPQR)\s*(?:Record\s*)?(?:No\.?|Number)?\s*[:\-]?\s*([A-Za-z0-9\-\/]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["wpq_record_no"] = m.group(1).strip()

    # Qualified To (e.g. "Qualified To: ASME IX", "Qualified To: WPS-SA304L")
    m = re.search(
        r'Qualified\s*To\s*[:\-]?\s*([A-Za-z0-9 /,\-]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["qualified_to"] = m.group(1).strip()

    # Test date (Date of test)
    m = re.search(
        r'(?:Test\s*Date|Date\s*of\s*Test|Date\s*Tested)\s*[:\-]?\s*(\d{2}/\d{2}/\d{4})',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["test_date"] = m.group(1).strip()

    # Date issued (certificate issue date)
    m = re.search(
        r'(?:Date\s*Issued|Date\s*of\s*Issue|Issue\s*Date)\s*[:\-]?\s*(\d{2}/\d{2}/\d{4})',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["date_issued"] = m.group(1).strip()

    # Job knowledge (can be a rating or short phrase)
    m = re.search(
        r'Job\s*Knowledge\s*[:\-]?\s*([A-Za-z0-9 ,]+)',
        t,
        re.IGNORECASE,
    )
    if m:
        fields["job_knowledge"] = m.group(1).strip()

    return fields


# -----------------------------
# WeldTrace-style layout parser (light overrides)
# -----------------------------

def parse_weldtrace_layout(text: str) -> Dict[str, str]:
    """
    Additional layout-specific parsing for documents that look like WeldTrace exports.
    Currently very minimal; extend as needed.
    """
    out: Dict[str, str] = {}

    # Example: "Process: GTAW"
    m = re.search(r'\bProcess\s*:\s*([A-Z0-9 /,+]+)', text, re.IGNORECASE)
    if m:
        out["process"] = m.group(1).strip(" .,")

    return out


# -----------------------------
# PUBLIC API
# -----------------------------

def extract_and_parse(pdf_path: str, max_ocr_pages: int = 3) -> Dict[str, Any]:
    """
    High-level function:
        1. Try direct text extraction (pdfplumber).
        2. If too little text, OCR the first N pages.
        3. Parse fields from the resulting text.
        4. For WPS/PQR docs, enrich with PQR-specific regex parsing.
    """
    if not os.path.isfile(pdf_path):
        raise FileNotFoundError(pdf_path)

    # 1) Try direct text extraction
    base_text = _extract_text_pdfplumber(pdf_path)

    # 2) If not enough text, run OCR
    if len((base_text or "").strip()) < 50:
        text = _ocr_pdf_pages(pdf_path, max_pages=max_ocr_pages)
    else:
        text = base_text

    # 3) Parse generic fields
    fields = parse_fields(text)

    # 3a) WeldTrace-specific overrides
    wt_fields = parse_weldtrace_layout(text)
    for k, v in wt_fields.items():
        if v and not fields.get(k):
            fields[k] = v

    # 4) PQR/WPS-specific enrichment
    doc_type_main = (fields.get("doc_type") or fields.get("type") or "").lower().strip()
    print(f"DEBUG: doc_type_raw = {doc_type_main}")

    if doc_type_main in ("pqr", "wps", "wpq"):
        try:
            pqr_fields = _parse_pqr_from_text(text, fields)

            override_keys = {
                "pqr_number",
                "wps_number",
                "pqr_date",
                "wps_date",
                "base_material_spec",
                "base_material_thickness_mm",
                "position",
                "welder_name",
                "welder_id",
                "stamp_number",
                "test_lab",
                "test_report_no",
            }

            for k, v in pqr_fields.items():
                if not v:
                    continue
                if k in override_keys or not fields.get(k):
                    fields[k] = v
        except Exception as e:
            print(f"DEBUG: _parse_pqr_from_text error: {e}")

    # Always include raw text
    fields["_raw_text"] = text

    return fields
