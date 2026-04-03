# weldadmin_import_to_db.py – temporary stub

def ensure_tables():
    # later: create sqlite tables etc.
    print("DEBUG: ensure_tables() stub called")

def import_pdf_to_db(pdf_path: str):
    # later: real insert into DB
    print("DEBUG: import_pdf_to_db() stub called with", pdf_path)
    # return something that looks like a successful import
    return {
        "table": "wps",
        "record": {
            "wps_number": "STUB-WPS",
            "pqr_number": "STUB-PQR",
        },
        "id": 1,
    }
