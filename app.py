# app.py
import os
from flask import Flask

# blueprint that contains the upload/parse/import pages (from earlier)
from weldadmin_web import bp as weldadmin_bp

app = Flask(__name__, template_folder="templates", static_folder="static")

# register weldadmin blueprint (which contains /, /upload, /import, /download routes)
app.register_blueprint(weldadmin_bp)

# minimal health-check API (optional)
@app.route("/health")
def health():
    return {"status": "ok"}

if __name__ == "__main__":
    # ensure upload dir exists (blueprint creates it too, but safe to ensure here)
    uploads = os.path.join(os.getcwd(), "uploads")
    os.makedirs(uploads, exist_ok=True)

    # run the app
    app.run(host="127.0.0.1", port=5000, debug=True)
