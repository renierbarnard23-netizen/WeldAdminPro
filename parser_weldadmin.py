"""
parser_weldadmin.py
-------------------
Lightweight helpers for WeldAdmin Pro WPS/PQR/WPQR documents.

Important:
- ocr.py already contains the main parse_fields() implementation.
- This module is only used by weldadmin_auto_map and other helpers
  that import parse_weldtrace_layout().
- We intentionally do NOT define parse_fields() here so that ocr.py
  uses its own, more advanced parser.
"""

from typing import Dict
import re


def _norm(s: str) -> str:
    """Collapse repeated whitespace and trim."""
    return re.sub(r"[^\S\r\n]+", " ", s or "").strip()


def parse_weldtrace_layout(text: str) -> Dict[str, str]:
    """
    Extra WeldTrace-specific heuristics.

    At the moment, ocr.py.parse_fields() already covers most of the
    WeldTrace-style layouts, so this function only provides optional
    hints that other parts of WeldAdmin (e.g. weldadmin_auto_map)
    may use.

    You can extend this later with more regex rules if you want to
    tune auto-mapping from WeldTrace exports.
    """
    data: Dict[str, str] = {}

    # Be robust against any weird input
    try:
        t = _norm(text.replace("–", "-").replace("—", "-"))
    except Exception:
        t = _norm(text)

    # Example: try to recover a process from a header line like
    # "Designation WPS ASME BPVC Sec. IX - 2023; ...; GTAW; ..."
    m = re.search(r"\b(GTAW|SMAW|GMAW|FCAW|SAW)\b", t)
    if m:
        data["process"] = m.group(1)

    # You can add more heuristics here later, e.g. joint type,
    # thickness ranges, etc.

    return data
