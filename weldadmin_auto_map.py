"""
weldadmin_auto_map.py
----------------------
Helper functions to connect OCR + parser_weldadmin to WeldAdmin Pro
models / forms for WPS, PQR and WPQ (WPQR) documents.
"""

from typing import Dict, Any, Tuple

from ocr import extract_and_parse
from parser_weldadmin import parse_weldtrace_layout


# ---------- Core OCR + parser integration ----------


def parse_pdf(path: str) -> Dict[str, Any]:
    """
    Run OCR + WeldTrace layout parser and return a merged data dict.

    - Uses ocr.extract_and_parse(path) to get basic fields + _raw_text
    - Uses parser_weldadmin.parse_weldtrace_layout on the raw text
    - Merges the two dicts (parser_weldadmin values win on key clashes)
    """
    result = extract_and_parse(path)
    raw_text = result.get("_raw_text", "") or ""
    extra = parse_weldtrace_layout(raw_text)

    data: Dict[str, Any] = {}
    data.update(result)
    data.update(extra)
    data.pop("_raw_text", None)
    return data


# ---------- Helpers ----------


def _parse_range_mm(range_text: str) -> Tuple[float | None, float | None]:
    """
    Turn '1.5 - 15.24' / '1.5 – 15.24' into (1.5, 15.24).
    Returns (None, None) if it can't parse.
    """
    if not range_text:
        return None, None

    txt = range_text.replace("–", "-").replace("—", "-")
    parts = txt.split("-")
    if len(parts) != 2:
        return None, None

    try:
        low = float(parts[0].strip().replace(",", "."))
        high = float(parts[1].strip().replace(",", "."))
        return low, high
    except ValueError:
        return None, None


# ---------- Record builders for each document type ----------


def build_wps_record(data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Map parsed data to a clean WPS record dict.

    This is aligned with the WPS tab in weldadmin_gui.py, i.e.:
        doc_type, company_name, designation,
        wps_number, wps_rev, wps_date,
        pqr_number, pqr_rev, pqr_date,
        code_standard, construction_code,
        thickness_range_text, thickness_min_mm, thickness_max_mm,
        outside_diameter_range_text,
        joint_type, joint_design, surface_prep, groove_angle,
        root_face_mm, root_gap_mm, max_misalignment_mm,
        back_gouging, backing, backing_type,
        process, process_type,
        shielding_gas, backing_gas,
        preheat_min_c, interpass_max_c,
        amps_range, volts_range, travel_speed_range_mm_min,
        max_heat_input_kj_mm,
        base_material_1_spec, base_material_2_spec
    """
    rec: Dict[str, Any] = {}

    # Doc type
    rec["doc_type"] = (data.get("doc_type") or data.get("type") or "WPS").upper()

    # High-level info
    rec["company_name"] = data.get("company_name", "")
    rec["designation"] = data.get("designation", "")

    # Numbers, revs, dates
    rec["wps_number"] = data.get("wps_number", "")
    rec["wps_rev"] = data.get("wps_rev", "")
    rec["wps_date"] = data.get("wps_date", "")
    rec["pqr_number"] = data.get("pqr_number", "")
    rec["pqr_rev"] = data.get("pqr_rev", "")
    rec["pqr_date"] = data.get("pqr_date", "")

    # Codes
    rec["code_standard"] = data.get("code_standard", "")
    rec["construction_code"] = data.get("construction_code", "")

    # Thickness & OD ranges
    thickness_text = (
        data.get("thickness_range_mm", "")
        or data.get("thickness_range_text", "")
        or ""
    )
    rec["thickness_range_text"] = thickness_text

    tmin, tmax = _parse_range_mm(thickness_text)
    rec["thickness_min_mm"] = tmin
    rec["thickness_max_mm"] = tmax

    rec["outside_diameter_range_text"] = (
        data.get("outside_diameter_range_mm", "")
        or data.get("outside_diameter_range", "")
        or ""
    )

    # Joint & prep
    rec["joint_type"] = data.get("joint_type", "")
    rec["joint_design"] = data.get("joint_design", "")
    rec["surface_prep"] = data.get("surface_prep", "")
    rec["groove_angle"] = data.get("groove_angle", "")
    rec["root_face_mm"] = data.get("root_face_mm", "")
    rec["root_gap_mm"] = data.get("root_gap_mm", "")
    rec["max_misalignment_mm"] = data.get("max_misalignment_mm", "")
    rec["back_gouging"] = data.get("back_gouging", "")
    rec["backing"] = data.get("backing", "")
    rec["backing_type"] = data.get("backing_type", "")

    # Process & parameters
    rec["process"] = data.get("process", "")
    rec["process_type"] = data.get("process_type", "")
    rec["shielding_gas"] = data.get("shielding_gas", "")
    rec["backing_gas"] = data.get("backing_gas", "")
    rec["preheat_min_c"] = data.get("preheat_min_c", "")
    rec["interpass_max_c"] = data.get("interpass_max_c", "")
    rec["amps_range"] = data.get("amps_range", "")
    rec["volts_range"] = data.get("volts_range", "")
    rec["travel_speed_range_mm_min"] = data.get("travel_speed_range_mm_min", "")
    rec["max_heat_input_kj_mm"] = data.get("max_heat_input_kj_mm", "")

    # Base metals
    rec["base_material_1_spec"] = data.get("base_material_1_spec", "")
    rec["base_material_2_spec"] = data.get("base_material_2_spec", "")

    return rec


def build_pqr_record(data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Map parsed data to a clean PQR record dict.

    Expected keys (if available) – aligned with PQR tab:
        pqr_number, wps_number, code_standard,
        pqr_date, wps_date, process, position, joint_type,
        base_material_spec, base_material_thickness_mm,
        welder_name, welder_id, stamp_number, test_lab, test_report_no
    """
    rec: Dict[str, Any] = {}

    rec["doc_type"] = (data.get("doc_type") or data.get("type") or "PQR").upper()
    rec["pqr_number"] = data.get("pqr_number", "")
    rec["wps_number"] = data.get("wps_number", "")
    rec["code_standard"] = data.get("code_standard", "")
    rec["pqr_date"] = data.get("pqr_date", "")
    rec["wps_date"] = data.get("wps_date", "")
    rec["process"] = data.get("process", "")
    rec["position"] = data.get("position", "")
    rec["joint_type"] = data.get("joint_type", "")
    rec["base_material_spec"] = data.get("base_material_spec", "")
    rec["base_material_thickness_mm"] = data.get("base_material_thickness_mm", "")
    rec["welder_name"] = data.get("welder_name", "")
    rec["welder_id"] = data.get("welder_id", "")
    rec["stamp_number"] = data.get("stamp_number", "")
    rec["test_lab"] = data.get("test_lab", "")
    rec["test_report_no"] = data.get("test_report_no", "")

    return rec


def build_wpq_record(data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Map parsed data to a clean WPQ / WPQR record dict.

    Expected keys (if available) – aligned with WPQ tab:
        certificate_no, wpq_record_no, welder_name, welder_id,
        qualified_to, stamp_number, wps_number, process, position,
        base_material_spec, test_date, date_issued, job_knowledge
    """
    rec: Dict[str, Any] = {}

    rec["doc_type"] = (data.get("doc_type") or data.get("type") or "WPQ").upper()
    rec["certificate_no"] = data.get("certificate_no", "")
    rec["wpq_record_no"] = data.get("wpq_record_no", "")
    rec["welder_name"] = data.get("welder_name", "")
    rec["welder_id"] = data.get("welder_id", "")
    rec["qualified_to"] = data.get("qualified_to", "")
    rec["stamp_number"] = data.get("stamp_number", "")
    rec["wps_number"] = data.get("wps_number", "")
    rec["process"] = data.get("process", "")
    rec["position"] = data.get("position", "")
    rec["base_material_spec"] = data.get("base_material_spec", "")
    rec["test_date"] = data.get("test_date", "")
    rec["date_issued"] = data.get("date_issued", "")
    rec["job_knowledge"] = data.get("job_knowledge", "")

    return rec


def parse_pdf_to_model(path: str) -> Dict[str, Any]:
    """
    Parse any WPS/PQR/WPQ(WPQR) PDF and return a model dict:

        {
            "table": "wps" | "pqr" | "wpq" | None,
            "record": { ...mapped fields... }
        }
    """
    data = parse_pdf(path)
    doc_type = (data.get("doc_type") or data.get("type") or "").upper()

    if doc_type == "WPS":
        return {"table": "wps", "record": build_wps_record(data)}
    if doc_type == "PQR":
        return {"table": "pqr", "record": build_pqr_record(data)}
    if doc_type in ("WPQ", "WPQR"):
        return {"table": "wpq", "record": build_wpq_record(data)}

    # Fallback – unknown type, return raw parsed data
    return {"table": None, "record": data}


if __name__ == "__main__":
    import os

    pdf_path = input("Enter PDF path to parse: ").strip()
    if not pdf_path:
        print("No path entered, exiting.")
    elif not os.path.isfile(pdf_path):
        print(f"File not found: {pdf_path}")
    else:
        model = parse_pdf_to_model(pdf_path)
        print(f"Detected target table: {model['table']}")
        print("----- MAPPED RECORD -----")
        for k, v in model["record"].items():
            print(f"{k}: {v}")

    input("\nDone. Press Enter to close...")
