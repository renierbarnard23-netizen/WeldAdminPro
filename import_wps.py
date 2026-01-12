# WeldAdmin Pro - Document Import & Preview Module
# ------------------------------------------------
# GUI integration for testing WPS/PQR/WPQR uploads and OCR parsing

import os
import json
import csv
import sqlite3
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

try:
    from ocr import extract_and_parse
except Exception as e:
    raise RuntimeError("Could not import extract_and_parse from ocr.py. Ensure ocr.py and parser_weldadmin.py are in the same folder.") from e

APP_TITLE = "WeldAdmin Pro - Import & Preview"
DB_NAME = "weld_docs.db"
TABLE_NAME = "weld_documents"

FIELDS = [
    ("type", "Document Type"),
    ("code", "Document Number / Code"),
    ("pqr", "Linked PQR"),
    ("process", "Process"),
    ("material", "Material"),
    ("thickness", "Thickness Range"),
    ("gas", "Shielding Gas"),
    ("filler", "Filler"),
    ("positions", "Positions"),
    ("issue_date", "Issue Date"),
    ("expiry_date", "Expiry Date"),
    ("_parser_code_family", "Code Family"),
    ("_parser_confidence", "Parser Confidence"),
]

def ensure_db():
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        CREATE TABLE IF NOT EXISTS {TABLE_NAME} (
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
            raw_text TEXT,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )
    """)
    conn.commit()
    conn.close()

def save_to_db(file_path, data):
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        INSERT INTO {TABLE_NAME} (
            file_path, doc_type, doc_code, pqr, process, material, thickness,
            gas, filler, positions, issue_date, expiry_date, code_family, confidence, raw_text
        ) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)
    """, (
        file_path,
        data.get("type"),
        data.get("code"),
        data.get("pqr"),
        data.get("process"),
        data.get("material"),
        data.get("thickness"),
        data.get("gas"),
        data.get("filler"),
        data.get("positions"),
        data.get("issue_date"),
        data.get("expiry_date"),
        data.get("_parser_code_family"),
        data.get("_parser_confidence"),
        data.get("_raw_text")
    ))
    conn.commit()
    conn.close()

class ImportApp(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title(APP_TITLE)
        self.geometry("900x640")
        self.file_path = tk.StringVar()
        self.data = {}
        self._build_ui()

    def _build_ui(self):
        top = ttk.Frame(self, padding=10)
        top.pack(fill="x")
        ttk.Label(top, text=APP_TITLE, font=("Segoe UI", 14, "bold")).pack(side="left")
        ttk.Button(top, text="Open File", command=self.on_open).pack(side="right", padx=5)
        ttk.Button(top, text="Run Import", command=self.on_import).pack(side="right")

        path_frame = ttk.Frame(self, padding=10)
        path_frame.pack(fill="x")
        ttk.Label(path_frame, text="Selected File:").pack(side="left")
        ttk.Entry(path_frame, textvariable=self.file_path).pack(side="left", fill="x", expand=True, padx=5)

        self.entries = {}
        form = ttk.LabelFrame(self, text="Parsed Fields", padding=10)
        form.pack(fill="both", expand=True, padx=10, pady=10)
        for key, label in FIELDS:
            frame = ttk.Frame(form)
            frame.pack(fill="x", pady=2)
            ttk.Label(frame, text=f"{label}:", width=24).pack(side="left")
            var = tk.StringVar()
            ttk.Entry(frame, textvariable=var).pack(side="left", fill="x", expand=True)
            self.entries[key] = var

        self.txt_raw = tk.Text(self, height=8, wrap="word")
        self.txt_raw.pack(fill="both", expand=True, padx=10, pady=(0, 10))

        bottom = ttk.Frame(self, padding=10)
        bottom.pack(fill="x")
        ttk.Button(bottom, text="Save to DB", command=self.on_save).pack(side="left")
        ttk.Button(bottom, text="Export JSON", command=self.on_json).pack(side="left", padx=5)
        ttk.Button(bottom, text="Export CSV", command=self.on_csv).pack(side="left", padx=5)
        self.status = tk.StringVar(value="Ready")
        ttk.Label(bottom, textvariable=self.status).pack(side="right")

    def on_open(self):
        path = filedialog.askopenfilename(filetypes=[("PDF & Images", "*.pdf;*.png;*.jpg;*.jpeg;*.tif;*.bmp")])
        if path:
            self.file_path.set(path)

    def on_import(self):
        path = self.file_path.get().strip()
        if not path or not os.path.exists(path):
            messagebox.showerror("Error", "Please select a valid file.")
            return
        self.status.set("Running OCR...")
        self.update()
        try:
            data = extract_and_parse(path)
            self.data = data
            for k, _ in FIELDS:
                self.entries[k].set(data.get(k, ""))
            self.txt_raw.delete("1.0", "end")
            self.txt_raw.insert("1.0", data.get("_raw_text", "")[:1500])
            self.status.set("Import complete.")
        except Exception as e:
            messagebox.showerror("Error", str(e))
            self.status.set("Error.")

    def on_save(self):
        if not self.data:
            messagebox.showinfo("Nothing to save", "Please import a file first.")
            return
        save_to_db(self.file_path.get(), self.data)
        messagebox.showinfo("Saved", "Record saved to weld_docs.db")

    def on_json(self):
        if not self.data:
            return
        path = filedialog.asksaveasfilename(defaultextension=".json")
        if path:
            with open(path, "w", encoding="utf-8") as f:
                json.dump(self.data, f, indent=2)

    def on_csv(self):
        if not self.data:
            return
        path = filedialog.asksaveasfilename(defaultextension=".csv")
        if path:
            with open(path, "w", newline="", encoding="utf-8") as f:
                w = csv.writer(f)
                w.writerow([label for _, label in FIELDS])
                w.writerow([self.data.get(k, "") for k, _ in FIELDS])

if __name__ == "__main__":
    app = ImportApp()
    app.mainloop()
