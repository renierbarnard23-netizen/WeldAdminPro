"""
test_ocr_parse.py
-----------------
Simple test harness for WeldAdmin Pro OCR + parser_weldadmin.

Usage (from your WeldAdminPro folder):

    python test_ocr_parse.py
"""

import os
from ocr import extract_and_parse

try:
    from parser_weldadmin import parse_weldtrace_layout
    HAS_CUSTOM_PARSER = True
    print("parser_weldadmin imported OK ✅")
except Exception as e:
    HAS_CUSTOM_PARSER = False
    print("Could not import parser_weldadmin ❌:", e)


def main() -> None:
    default_pdf = "WPS-SA304L.pdf"
    user_path = input(
        f"Enter PDF path to test (or press Enter to use '{default_pdf}'): "
    ).strip()

    pdf_path = user_path or default_pdf

    if not os.path.isfile(pdf_path):
        print(f"⚠️ File not found: {pdf_path}")
        print("Make sure the PDF is in this folder or give a full path.")
        input("\nPress Enter to exit...")
        return

    print(f"\nUsing PDF: {pdf_path}\n")

    result = extract_and_parse(pdf_path)

    print("----- BASIC FIELDS FROM ocr.extract_and_parse -----")
    for k, v in result.items():
        if k == "_raw_text":
            continue
        print(f"{k}: {v}")

    if HAS_CUSTOM_PARSER and "_raw_text" in result:
        print("\n----- EXTRA WELDTRACE FIELDS FROM parser_weldadmin -----")
        extra = parse_weldtrace_layout(result["_raw_text"])
        if not extra:
            print("No extra WeldTrace fields found (dict is empty).")
        else:
            for k, v in extra.items():
                print(f"{k}: {v}")

    input("\n\nDone. Press Enter to close...")


if __name__ == "__main__":
    main()
