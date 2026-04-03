# weldadmin_web.py
# Flask blueprint for the WeldAdmin Pro web UI/API

from flask import Blueprint, render_template, jsonify, request
import os

from weldadmin_auto_map import parse_pdf_to_model
from weldadmin_import_to_db import import_pdf_to_db

bp = Blueprint("weldadmin", __name__, template_folder="templates")

UPLOAD_FOLDER = os.path.join(os.getcwd(), "uploads")
os.makedirs(UPLOAD_FOLDER, exist_ok=True)


@bp.route("/")
def home():
    """Web UI homepage"""
    return render_template("index.html")


@bp.route("/api/parse", methods=["POST"])
def api_parse():
    """Parse PDF without saving to DB"""
    if "file" not in request.files:
        return jsonify({"error": "no file uploaded"}), 400

    f = request.files["file"]
    filename = f.filename
    saved_path = os.path.join(UPLOAD_FOLDER, filename)
    f.save(saved_path)

    result = parse_pdf_to_model(saved_path)
    return jsonify({"ok": True, "parsed": result})


@bp.route("/api/import", methods=["POST"])
def api_import():
    """Parse AND import to DB"""
    if "file" not in request.files:
        return jsonify({"error": "no file uploaded"}), 400

    f = request.files["file"]
    filename = f.filename
    saved_path = os.path.join(UPLOAD_FOLDER, filename)
    f.save(saved_path)

    result = import_pdf_to_db(saved_path)
    return jsonify({"ok": True, "imported": result})
