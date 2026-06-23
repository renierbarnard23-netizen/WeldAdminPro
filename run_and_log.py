import runpy, traceback, sys
fn = "weldadmin_landing_page_full.py"
try:
    runpy.run_path(fn, run_name="__main__")
except Exception:
    with open("weldadmin_error.log", "w", encoding="utf-8") as f:
        traceback.print_exc(file=f)
    print("An exception was logged to weldadmin_error.log")
    # also print to console
    traceback.print_exc()
    sys.exit(1)
