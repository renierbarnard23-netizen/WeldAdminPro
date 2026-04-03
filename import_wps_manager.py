"""
WeldAdmin Pro - Import, Database & Search Module
------------------------------------------------
- Import WPS/PQR/WPQR files via GUI and run OCR + parser (uses your ocr.py + parser_weldadmin.py)
- Auto-creates SQLite database (weld_docs.db) and table if not exists
- View, search, and filter previously imported records
- Export selected rows to JSON/CSV, and delete records

Usage:
    python import_wps_manager.py
"""

import os
import json
import csv
import sqlite3
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

# Import your OCR pipeline
try:
    from ocr import extract_and_parse
except Exception as e:
    raise RuntimeError(
        "Could not import 'extract_and_parse' from ocr.py. "
        "Ensure 'ocr.py' and 'parser_weldadmin.py' are in the same folder."
    ) from e

APP_TITLE = "WeldAdmin Pro - Import & Records"
DB_NAME = "weld_docs.db"
TABLE_NAME = "weld_documents"

VISIBLE_FIELDS = [
    ("type", "Type"),
    ("code", "Document Number / Code"),
    ("pqr", "Linked PQR"),
    ("process", "Process"),
    ("material", "Material"),
    ("thickness", "Thickness"),
    ("gas", "Gas"),
    ("filler", "Filler"),
    ("positions", "Positions"),
    ("issue_date", "Issue Date"),
    ("expiry_date", "Expiry Date"),
    ("_parser_code_family", "Code Family"),
    ("_parser_confidence", "Confidence"),
]

ALL_FIELDS_DB_ORDER = [
    "file_path","doc_type","doc_code","pqr","process","material","thickness","gas","filler",
    "positions","issue_date","expiry_date","code_family","confidence","parser_excerpt","raw_text"
]


def ensure_db():
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        CREATE TABLE IF NOT EXISTS {TABLE_NAME}(
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
            parser_excerpt TEXT,
            raw_text TEXT,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )
    """)
    conn.commit()
    conn.close()


def save_to_db(file_path: str, data: dict):
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        INSERT INTO {TABLE_NAME}(
            file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
            positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text
        ) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)
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
        data.get("_parser_excerpt"),
        data.get("_raw_text"),
    ))
    conn.commit()
    conn.close()


def query_records(doc_type="ALL", search_text="", limit=500):
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()

    where = []
    params = []

    if doc_type and doc_type != "ALL":
        where.append("doc_type = ?")
        params.append(doc_type)

    if search_text:
        like = f"%{search_text}%"
        where.append("(" +
                     " OR ".join([
                         "doc_code LIKE ?",
                         "pqr LIKE ?",
                         "process LIKE ?",
                         "material LIKE ?",
                         "gas LIKE ?",
                         "filler LIKE ?",
                         "positions LIKE ?",
                         "issue_date LIKE ?",
                         "expiry_date LIKE ?",
                         "code_family LIKE ?"
                     ]) +
                     ")")
        params.extend([like]*10)

    where_clause = "WHERE " + " AND ".join(where) if where else ""
    sql = f"""
        SELECT id, file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
               positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text, created_at
        FROM {TABLE_NAME}
        {where_clause}
        ORDER BY id DESC
        LIMIT ?
    """
    params.append(limit)
    cur.execute(sql, params)
    rows = cur.fetchall()
    conn.close()
    return rows


def delete_records(ids):
    if not ids:
        return
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    qmarks = ",".join(["?"]*len(ids))
    cur.execute(f"DELETE FROM {TABLE_NAME} WHERE id IN ({qmarks})", ids)
    conn.commit()
    conn.close()


class ImportTab(ttk.Frame):
    def __init__(self, parent, on_saved_callback=None):
        super().__init__(parent, padding=(10, 10))
        self.on_saved_callback = on_saved_callback
        self.file_path = tk.StringVar()
        self.data = {}

        # Header
        header = ttk.Frame(self)
        header.pack(fill="x")
        ttk.Label(header, text="Import & Preview", font=("Segoe UI", 12, "bold")).pack(side="left")
        ttk.Button(header, text="Open File…", command=self.on_open).pack(side="right", padx=6)
        ttk.Button(header, text="Run Import", command=self.on_import).pack(side="right")

        # File path
        row = ttk.Frame(self)
        row.pack(fill="x", pady=(8, 6))
        ttk.Label(row, text="Selected File:", width=14).pack(side="left")
        ttk.Entry(row, textvariable=self.file_path).pack(side="left", fill="x", expand=True, padx=6)
        ttk.Button(row, text="Browse…", command=self.on_open).pack(side="left")

        # Fields grid
        grid = ttk.LabelFrame(self, text="Parsed Fields", padding=10)
        grid.pack(fill="both", expand=True)

        self.vars = {}
        for key, label in VISIBLE_FIELDS:
            r = ttk.Frame(grid)
            r.pack(fill="x", pady=3)
            ttk.Label(r, text=f"{label}:", width=24).pack(side="left")
            var = tk.StringVar()
            ent = ttk.Entry(r, textvariable=var)
            ent.pack(side="left", fill="x", expand=True)
            self.vars[key] = var

        # Excerpt + Raw
        meta = ttk.Frame(self)
        meta.pack(fill="both", expand=True, pady=(6, 0))
        ttk.Label(meta, text="Parser Excerpt:").pack(anchor="w")
        self.txt_excerpt = tk.Text(meta, height=5, wrap="word")
        self.txt_excerpt.pack(fill="x", pady=(0, 6))

        ttk.Label(meta, text="Raw Text (first 1500 chars):").pack(anchor="w")
        self.txt_raw = tk.Text(meta, height=10, wrap="word")
        self.txt_raw.pack(fill="both", expand=True)

        # Footer
        footer = ttk.Frame(self)
        footer.pack(fill="x", pady=(6, 0))
        ttk.Button(footer, text="Save to Database", command=self.on_save).pack(side="left")
        ttk.Button(footer, text="Export JSON", command=self.on_export_json).pack(side="left", padx=6)
        ttk.Button(footer, text="Export CSV", command=self.on_export_csv).pack(side="left")
        self.status = tk.StringVar(value="Ready.")
        ttk.Label(footer, textvariable=self.status).pack(side="right")

    def on_open(self):
        path = filedialog.askopenfilename(
            title="Select WPS/PQR/WPQR file",
            filetypes=[
                ("PDF files", "*.pdf"),
                ("Images", "*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.bmp"),
                ("All files", "*.*"),
            ]
        )
        if path:
            self.file_path.set(path)
            self.status.set(f"Selected: {path}")

    def on_import(self):
        path = self.file_path.get().strip()
        if not path or not os.path.exists(path):
            messagebox.showerror("No file", "Please select a valid file.")
            return
        self.status.set("Running OCR + parser…")
        self.update_idletasks()
        try:
            self.data = extract_and_parse(path) or {}
            # populate
            for key, _ in VISIBLE_FIELDS:
                self.vars[key].set(self.data.get(key, ""))
            self.txt_excerpt.delete("1.0", "end")
            self.txt_excerpt.insert("1.0", self.data.get("_parser_excerpt", "") or "")
            self.txt_raw.delete("1.0", "end")
            self.txt_raw.insert("1.0", (self.data.get("_raw_text", "") or "")[:1500])
            self.status.set("Import complete.")
        except Exception as e:
            messagebox.showerror("Error", f"Failed to import document:\n{e}")
            self.status.set("Error during import.")

    def on_export_json(self):
        if not self.data:
            messagebox.showinfo("Nothing to export", "Import a file first.")
            return
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON", "*.json")])
        if not path:
            return
        with open(path, "w", encoding="utf-8") as f:
            json.dump(self.data, f, ensure_ascii=False, indent=2)
        messagebox.showinfo("Exported", f"Saved JSON to:\n{path}")

    def on_export_csv(self):
        if not self.data:
            messagebox.showinfo("Nothing to export", "Import a file first.")
            return
        path = filedialog.asksaveasfilename(defaultextension=".csv", filetypes=[("CSV", "*.csv")])
        if not path:
            return
        with open(path, "w", newline="", encoding="utf-8") as f:
            writer = csv.writer(f)
            writer.writerow([label for _, label in VISIBLE_FIELDS])
            row = [self.data.get(key, "") for key, _ in VISIBLE_FIELDS]
            writer.writerow(row)
        messagebox.showinfo("Exported", f"Saved CSV to:\n{path}")

    def on_save(self):
        if not self.data:
            messagebox.showinfo("Nothing to save", "Import a file first.")
            return
        save_to_db(self.file_path.get(), self.data)
        messagebox.showinfo("Saved", "Record saved to database.")
        if self.on_saved_callback:
            self.on_saved_callback()


class RecordsTab(ttk.Frame):
    def __init__(self, parent):
        super().__init__(parent, padding=(10, 10))
        self._build_ui()
        self.refresh()

    def _build_ui(self):
        # Search controls
        ctrl = ttk.Frame(self)
        ctrl.pack(fill="x")
        ttk.Label(ctrl, text="Filter:", font=("Segoe UI", 12, "bold")).pack(side="left", padx=(0, 10))

        ttk.Label(ctrl, text="Type").pack(side="left")
        self.type_var = tk.StringVar(value="ALL")
        ttk.Combobox(ctrl, textvariable=self.type_var, values=["ALL","WPS","PQR","WPQR"], width=8).pack(side="left", padx=6)

        ttk.Label(ctrl, text="Search").pack(side="left")
        self.search_var = tk.StringVar()
        ttk.Entry(ctrl, textvariable=self.search_var, width=40).pack(side="left", padx=6)

        ttk.Button(ctrl, text="Apply", command=self.refresh).pack(side="left", padx=6)
        ttk.Button(ctrl, text="Clear", command=self.clear_filters).pack(side="left")

        # Treeview
        cols = ("id","type","code","pqr","process","material","thickness","gas","filler","positions",
                "issue_date","expiry_date","code_family","confidence","created_at")
        self.tree = ttk.Treeview(self, columns=cols, show="headings", height=18)
        self.tree.pack(fill="both", expand=True, pady=(8, 6))

        headers = {
            "id":"ID","type":"Type","code":"Code","pqr":"PQR","process":"Process","material":"Material",
            "thickness":"Thickness","gas":"Gas","filler":"Filler","positions":"Positions",
            "issue_date":"Issue Date","expiry_date":"Expiry Date","code_family":"Family",
            "confidence":"Conf.","created_at":"Created"
        }
        widths = {
            "id":50,"type":70,"code":160,"pqr":140,"process":90,"material":150,"thickness":110,"gas":150,
            "filler":120,"positions":120,"issue_date":100,"expiry_date":100,"code_family":90,"confidence":80,"created_at":140
        }
        for c in cols:
            self.tree.heading(c, text=headers.get(c,c))
            self.tree.column(c, width=widths.get(c,120), anchor="w")

        # Buttons
        btns = ttk.Frame(self)
        btns.pack(fill="x")
        ttk.Button(btns, text="Refresh", command=self.refresh).pack(side="left")
        ttk.Button(btns, text="Export Selected → JSON", command=self.export_selected_json).pack(side="left", padx=6)
        ttk.Button(btns, text="Export Selected → CSV", command=self.export_selected_csv).pack(side="left")
        ttk.Button(btns, text="Delete Selected", command=self.delete_selected).pack(side="right")

    def clear_filters(self):
        self.type_var.set("ALL")
        self.search_var.set("")
        self.refresh()

    def refresh(self):
        for i in self.tree.get_children():
            self.tree.delete(i)
        rows = query_records(self.type_var.get(), self.search_var.get(), limit=2000)
        for row in rows:
            (rid, file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
             positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text, created_at) = row
            self.tree.insert("", "end", iid=str(rid), values=(
                rid, doc_type or "", doc_code or "", pqr or "", process or "", material or "",
                thickness or "", gas or "", filler or "", positions or "", issue_date or "", expiry_date or "",
                code_family or "", confidence or "", created_at or ""
            ))

    def _get_selected_ids(self):
        sel = self.tree.selection()
        return [int(s) for s in sel]

    def _get_rows_by_ids(self, ids):
        if not ids:
            return []
        ensure_db()
        conn = sqlite3.connect(DB_NAME)
        cur = conn.cursor()
        qmarks = ",".join(["?"]*len(ids))
        cur.execute(f"""
            SELECT id, file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
                   positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text, created_at
            FROM {TABLE_NAME}
            WHERE id IN ({qmarks})
            ORDER BY id DESC
        """, ids)
        rows = cur.fetchall()
        conn.close()
        return rows

    def export_selected_json(self):
        ids = self._get_selected_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        rows = self._get_rows_by_ids(ids)
        # Choose save path
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON","*.json")])
        if not path:
            return
        out = []
        for r in rows:
            d = dict(
                id=r[0], file_path=r[1], doc_type=r[2], doc_code=r[3], pqr=r[4], process=r[5],
                material=r[6], thickness=r[7], gas=r[8], filler=r[9], positions=r[10],
                issue_date=r[11], expiry_date=r[12], code_family=r[13], confidence=r[14],
                parser_excerpt=r[15], raw_text=r[16], created_at=r[17]
            )
            out.append(d)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(out, f, ensure_ascii=False, indent=2)
        messagebox.showinfo("Exported", f"Saved JSON to:\n{path}")

    def export_selected_csv(self):
        ids = self._get_selected_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        rows = self._get_rows_by_ids(ids)
        path = filedialog.asksaveasfilename(defaultextension=".csv", filetypes=[("CSV","*.csv")])
        if not path:
            return
        with open(path, "w", newline="", encoding="utf-8") as f:
            w = csv.writer(f)
            header = ["id","file_path","doc_type","doc_code","pqr","process","material","thickness","gas","filler",
                      "positions","issue_date","expiry_date","code_family","confidence","created_at"]
            w.writerow(header)
            for r in rows:
                w.writerow([r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[17]])
        messagebox.showinfo("Exported", f"Saved CSV to:\n{path}")

    def delete_selected(self):
        ids = self._get_selected_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        if not messagebox.askyesno("Confirm Delete", f"Delete {len(ids)} record(s)? This cannot be undone."):
            return
        delete_records(ids)
        self.refresh()


class MainApp(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title(APP_TITLE)
        self.geometry("1100x720")
        ensure_db()

        tabs = ttk.Notebook(self)
        tabs.pack(fill="both", expand=True)

        self.import_tab = ImportTab(tabs, on_saved_callback=self.on_saved_record)
        self.records_tab = RecordsTab(tabs)

        tabs.add(self.import_tab, text="Import")
        tabs.add(self.records_tab, text="Records")

    def on_saved_record(self):
        # auto-refresh records tab when a new record is saved
        self.records_tab.refresh()


if __name__ == "__main__":
    app = MainApp()
    try:
        app.iconbitmap(default="")  # optional icon
    except Exception:
        pass
    app.mainloop()
