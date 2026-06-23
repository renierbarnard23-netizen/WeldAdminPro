print("STARTING...")
import traceback
try:
    import weldadmin_landing_page_full
    print("IMPORTED OK")
except Exception as e:
    print("IMPORT ERROR:", e)
    traceback.print_exc()
