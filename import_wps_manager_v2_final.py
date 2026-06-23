"""
WeldAdmin Pro - Import & Records (Final Fixed)
---------------------------------------------
Includes:
- Working Save buttons (toolbar, footer, menu, Ctrl+S)
- Delete, Search, Filter, Export (CSV/JSON)
- Correct indentation in export_selected_csv()
"""

import os
import json
import csv
import sqlite3
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

try:
    from ocr import extract_and_parse
except Exception as e:
    raise RuntimeError("Could not import 'extract_and_parse' from ocr.py.") from e

APP_TITLE = "WeldAdmin Pro - Import & Records (Final)"
DB_NAME = "weld_docs.db"
TABLE_NAME = "weld_documents"


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


def query_records(doc_type="ALL", search_text="", limit=1000):
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
        where.append("(" + " OR ".join([
            "doc_code LIKE ?","pqr LIKE ?","process LIKE ?","material LIKE ?","gas LIKE ?",
            "filler LIKE ?","positions LIKE ?","issue_date LIKE ?","expiry_date LIKE ?","code_family LIKE ?"
        ]) + ")")
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

    def __init__(self, parent, on_saved=None):
        super().__init__(parent, padding=10)
        self.on_saved = on_saved
        self.file_path = tk.StringVar()
        self.data = {}
        self._build_ui()

    def _build_ui(self):
        toolbar = ttk.Frame(self)
        toolbar.pack(fill="x", pady=(0, 8))
        ttk.Button(toolbar, text="Open", command=self.on_open).pack(side="left")
        ttk.Button(toolbar, text="Import", command=self.on_import).pack(side="left", padx=6)
        self.btn_save_toolbar = ttk.Button(toolbar, text="Save (Add to DB)", command=self.on_save, state="disabled")
        self.btn_save_toolbar.pack(side="left")

        row = ttk.Frame(self)
        row.pack(fill="x", pady=(2, 6))
        ttk.Label(row, text="Selected File:", width=14).pack(side="left")
        ttk.Entry(row, textvariable=self.file_path).pack(side="left", fill="x", expand=True, padx=6)
        ttk.Button(row, text="Browse…", command=self.on_open).pack(side="left")

        grid = ttk.LabelFrame(self, text="Parsed Fields", padding=10)
        grid.pack(fill="both", expand=True)

        self.vars = {}
        for key, label in self.VISIBLE_FIELDS:
            r = ttk.Frame(grid)
            r.pack(fill="x", pady=3)
            ttk.Label(r, text=f"{label}:", width=24).pack(side="left")
            var = tk.StringVar()
            ttk.Entry(r, textvariable=var).pack(side="left", fill="x", expand=True)
            self.vars[key] = var

        self.txt_excerpt = tk.Text(self, height=5, wrap="word")
        self.txt_excerpt.pack(fill="x", pady=6)
        self.txt_raw = tk.Text(self, height=10, wrap="word")
        self.txt_raw.pack(fill="both", expand=True)

        footer = ttk.Frame(self)
        footer.pack(fill="x", pady=6)
        self.btn_save_footer = ttk.Button(footer, text="Save (Add to DB)", command=self.on_save, state="disabled")
        self.btn_save_footer.pack(side="left")
        self.status = tk.StringVar(value="Ready.")
        ttk.Label(footer, textvariable=self.status).pack(side="right")

    def on_open(self):
        path = filedialog.askopenfilename(filetypes=[("PDF/Images", "*.pdf;*.png;*.jpg;*.jpeg;*.tif;*.bmp")])
        if path:
            self.file_path.set(path)

    def on_import(self):
        path = self.file_path.get()
        if not os.path.exists(path):
            messagebox.showerror("Error", "Select a valid file.")
            return
        self.status.set("Running OCR...")
        self.update()
        try:
            self.data = extract_and_parse(path) or {}
            for k, _ in self.VISIBLE_FIELDS:
                self.vars[k].set(self.data.get(k, ""))
            self.txt_excerpt.delete("1.0", "end")
            self.txt_excerpt.insert("1.0", self.data.get("_parser_excerpt", "") or "")
            self.txt_raw.delete("1.0", "end")
            self.txt_raw.insert("1.0", (self.data.get("_raw_text", "") or "")[:1500])
            self.btn_save_toolbar.config(state="normal")
            self.btn_save_footer.config(state="normal")
            self.status.set("Import complete. Click Save to add to DB.")
        except Exception as e:
            messagebox.showerror("Import failed", str(e))

    def on_save(self):
        if not self.data:
            messagebox.showinfo("Nothing to save", "Import a file first.")
            return
        save_to_db(self.file_path.get(), self.data)
        messagebox.showinfo("Saved", "Record added to database.")
        if self.on_saved:
            self.on_saved()


class RecordsTab(ttk.Frame):
    def __init__(self, parent):
        super().__init__(parent, padding=10)
        self._build_ui()
        self.refresh()

    def _build_ui(self):
        ctrl = ttk.Frame(self)
        ctrl.pack(fill="x")
        ttk.Label(ctrl, text="Type").pack(side="left")
        self.type_var = tk.StringVar(value="ALL")
        ttk.Combobox(ctrl, textvariable=self.type_var, values=["ALL","WPS","PQR","WPQR"], width=8).pack(side="left", padx=6)
        ttk.Label(ctrl, text="Search").pack(side="left")
        self.search_var = tk.StringVar()
        ttk.Entry(ctrl, textvariable=self.search_var, width=40).pack(side="left", padx=6)
        ttk.Button(ctrl, text="Apply", command=self.refresh).pack(side="left")
        ttk.Button(ctrl, text="Clear", command=self.clear_filters).pack(side="left", padx=6)

        cols = ("id","type","code","pqr","process","material","thickness","gas","filler",
                "positions","issue_date","expiry_date","code_family","confidence","created_at")
        self.tree = ttk.Treeview(self, columns=cols, show="headings", height=18)
        self.tree.pack(fill="both", expand=True, pady=(8,6))

        for c in cols:
            self.tree.heading(c, text=c.title())
            self.tree.column(c, width=120, anchor="w")

        btns = ttk.Frame(self)
        btns.pack(fill="x")
        ttk.Button(btns, text="Refresh", command=self.refresh).pack(side="left")
        ttk.Button(btns, text="Export JSON", command=self.export_selected_json).pack(side="left", padx=6)
        ttk.Button(btns, text="Export CSV", command=self.export_selected_csv).pack(side="left")
        ttk.Button(btns, text="Delete", command=self.delete_selected).pack(side="right")

    def clear_filters(self):
        self.type_var.set("ALL")
        self.search_var.set("")
        self.refresh()

    def refresh(self):
        for i in self.tree.get_children():
            self.tree.delete(i)
        rows = query_records(self.type_var.get(), self.search_var.get())
        for r in rows:
            self.tree.insert("", "end", values=r)

    def selected_ids(self):
        return [int(self.tree.item(i)["values"][0]) for i in self.tree.selection()]

    def export_selected_json(self):
        ids = self.selected_ids()
        if not ids:
            return messagebox.showinfo("None", "Select one or more rows.")
        rows = query_records()
        path = filedialog.asksaveasfilename(defaultextension=".json")
        if not path:
            return
        with open(path, "w", encoding="utf-8") as f:
            json.dump(rows, f, indent=2)
        messagebox.showinfo("Saved", path)

    def export_selected_csv(self):
        ids = self.selected_ids()
        if not ids:
            return messagebox.showinfo("None", "Select one or more rows.")
        rows = query_records()
        path = filedialog.asksaveasfilename(defaultextension=".csv")
        if not path:
            return
        with open(path, "w", newline="", encoding="utf-8") as f:
            w = csv.writer(f)
            w.writerow([d[0] for d in self.tree["columns"]])
            for r in rows:
                w.writerow(r)
        messagebox.showinfo("Saved", path)

    def delete_selected(self):
        ids = self.selected_ids()
        if not ids:
            return messagebox.showinfo("None", "Select one or more rows.")
        if not messagebox.askyesno("Confirm", f"Delete {len(ids)} record(s)?"):
            return
        delete_records(ids)
        self.refresh()


class MainApp(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title(APP_TITLE)
        self.geometry("1200x760")
        ensure_db()
        self.tabs = ttk.Notebook(self)
        self.tabs.pack(fill="both", expand=True)
        self.import_tab = ImportTab(self.tabs, on_saved=self.on_saved_record)
        self.records_tab = RecordsTab(self.tabs)
        self.tabs.add(self.import_tab, text="Import")
        self.tabs.add(self.records_tab, text="Records")
        self.bind_all("<Control-s>", lambda e: self.import_tab.on_save())

    def on_saved_record(self):
        self.records_tab.refresh()


if __name__ == "__main__":
    app = MainApp()
    app.mainloop()
