"""
WeldAdmin Pro - Import & Records (v3 with Edit)
----------------------------------------------
Adds:
- Edit Selected dialog (double-click row or button)
- Update records in SQLite safely
- Keep all features: Import, Save, Search/Filter, Export, Delete, Ctrl+S

Run:
    python import_wps_manager_v3_edit.py
"""

import os
import json
import csv
import sqlite3
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

# OCR import
try:
    from ocr import extract_and_parse
except Exception as e:
    raise RuntimeError("Could not import 'extract_and_parse' from ocr.py.") from e

APP_TITLE = "WeldAdmin Pro - Import & Records (v3 Edit)"
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


def update_record(row_id: int, payload: dict):
    ensure_db()
    fields = [
        "doc_type","doc_code","pqr","process","material","thickness","gas","filler",
        "positions","issue_date","expiry_date","code_family","confidence","parser_excerpt","raw_text"
    ]
    sets = ", ".join([f"{k} = ?" for k in fields])
    values = [payload.get(k) for k in fields]
    values.append(row_id)
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"UPDATE {TABLE_NAME} SET {sets} WHERE id = ?", values)
    conn.commit()
    conn.close()


def fetch_record(row_id: int):
    ensure_db()
    conn = sqlite3.connect(DB_NAME)
    cur = conn.cursor()
    cur.execute(f"""
        SELECT id, file_path, doc_type, doc_code, pqr, process, material, thickness, gas, filler,
               positions, issue_date, expiry_date, code_family, confidence, parser_excerpt, raw_text, created_at
        FROM {TABLE_NAME} WHERE id = ?
    """, (row_id,))
    row = cur.fetchone()
    conn.close()
    return row


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


class EditDialog(tk.Toplevel):
    # Fields mapped to DB columns
    FIELD_MAP = [
        ("doc_type","Type"),
        ("doc_code","Document Number / Code"),
        ("pqr","Linked PQR"),
        ("process","Process"),
        ("material","Material"),
        ("thickness","Thickness"),
        ("gas","Gas"),
        ("filler","Filler"),
        ("positions","Positions"),
        ("issue_date","Issue Date (YYYY-MM-DD)"),
        ("expiry_date","Expiry Date (YYYY-MM-DD)"),
        ("code_family","Code Family"),
        ("confidence","Confidence"),
    ]

    def __init__(self, parent, row_id: int, record_tuple, on_saved):
        super().__init__(parent)
        self.title(f"Edit Record #{row_id}")
        self.resizable(True, True)
        self.row_id = row_id
        self.on_saved = on_saved
        self.vars = {}

        # record_tuple indices (match query order)
        # 0:id, 1:file_path, 2:doc_type, 3:doc_code, 4:pqr, 5:process, 6:material, 7:thickness, 8:gas, 9:filler,
        # 10:positions, 11:issue_date, 12:expiry_date, 13:code_family, 14:confidence, 15:parser_excerpt, 16:raw_text, 17:created_at

        form = ttk.Frame(self, padding=10)
        form.pack(fill="both", expand=True)

        # Simple fields
        idx_map = {
            "doc_type":2, "doc_code":3, "pqr":4, "process":5, "material":6,
            "thickness":7, "gas":8, "filler":9, "positions":10, "issue_date":11,
            "expiry_date":12, "code_family":13, "confidence":14
        }
        for key, label in self.FIELD_MAP:
            row = ttk.Frame(form)
            row.pack(fill="x", pady=3)
            ttk.Label(row, text=label+":", width=26).pack(side="left")
            var = tk.StringVar(value=str(record_tuple[idx_map[key]] or ""))
            ent = ttk.Entry(row, textvariable=var)
            ent.pack(side="left", fill="x", expand=True)
            self.vars[key] = var

        # Multiline for excerpt and raw text
        meta = ttk.LabelFrame(form, text="Metadata", padding=8)
        meta.pack(fill="both", expand=True, pady=(8,0))

        ttk.Label(meta, text="Parser Excerpt:").pack(anchor="w")
        self.txt_excerpt = tk.Text(meta, height=4, wrap="word")
        self.txt_excerpt.pack(fill="x", pady=(0,6))
        self.txt_excerpt.insert("1.0", record_tuple[15] or "")

        ttk.Label(meta, text="Raw Text:").pack(anchor="w")
        self.txt_raw = tk.Text(meta, height=8, wrap="word")
        self.txt_raw.pack(fill="both", expand=True)
        self.txt_raw.insert("1.0", record_tuple[16] or "")

        # Buttons
        btns = ttk.Frame(form)
        btns.pack(fill="x", pady=(8,0))
        ttk.Button(btns, text="Save Changes", command=self._save).pack(side="left")
        ttk.Button(btns, text="Cancel", command=self.destroy).pack(side="right")

        self.grab_set()
        self.focus_set()

    def _save(self):
        payload = {k: v.get().strip() for k, v in self.vars.items()}
        payload["parser_excerpt"] = self.txt_excerpt.get("1.0", "end").strip()
        payload["raw_text"] = self.txt_raw.get("1.0", "end").strip()

        try:
            update_record(self.row_id, payload)
            messagebox.showinfo("Saved", f"Record #{self.row_id} updated.")
            if self.on_saved:
                self.on_saved()
            self.destroy()
        except Exception as e:
            messagebox.showerror("Update failed", str(e))


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

        cols = ("id","file_path","doc_type","doc_code","pqr","process","material","thickness","gas","filler",
                "positions","issue_date","expiry_date","code_family","confidence","parser_excerpt","raw_text","created_at")
        self.tree = ttk.Treeview(self, columns=cols, show="headings", height=18)
        self.tree.pack(fill="both", expand=True, pady=(8,6))

        headers = {
            "id":"ID","file_path":"File","doc_type":"Type","doc_code":"Code","pqr":"PQR","process":"Process",
            "material":"Material","thickness":"Thickness","gas":"Gas","filler":"Filler","positions":"Positions",
            "issue_date":"Issue Date","expiry_date":"Expiry Date","code_family":"Family","confidence":"Conf.",
            "parser_excerpt":"Excerpt","raw_text":"Raw Text","created_at":"Created"
        }
        widths = {
            "id":60,"file_path":180,"doc_type":80,"doc_code":160,"pqr":140,"process":110,"material":160,"thickness":120,
            "gas":160,"filler":120,"positions":120,"issue_date":110,"expiry_date":110,"code_family":100,"confidence":90,
            "parser_excerpt":180,"raw_text":200,"created_at":150
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
        ttk.Button(btns, text="Edit Selected", command=self.edit_selected).pack(side="right")
        ttk.Button(btns, text="Delete Selected", command=self.delete_selected).pack(side="right", padx=6)

        # Double-click to edit
        self.tree.bind("<Double-1>", self._on_double_click)

    def clear_filters(self):
        self.type_var.set("ALL")
        self.search_var.set("")
        self.refresh()

    def refresh(self):
        for i in self.tree.get_children():
            self.tree.delete(i)
        rows = query_records(self.type_var.get(), self.search_var.get(), limit=2000)
        for r in rows:
            self.tree.insert("", "end", iid=str(r[0]), values=r)

    def _sel_ids(self):
        return [int(i) for i in self.tree.selection()]

    def _rows_by_ids(self, ids):
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
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        rows = self._rows_by_ids(ids)
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON","*.json")])
        if not path:
            return
        out = []
        for r in rows:
            out.append({
                "id":r[0], "file_path":r[1], "doc_type":r[2], "doc_code":r[3], "pqr":r[4], "process":r[5],
                "material":r[6], "thickness":r[7], "gas":r[8], "filler":r[9], "positions":r[10],
                "issue_date":r[11], "expiry_date":r[12], "code_family":r[13], "confidence":r[14],
                "parser_excerpt":r[15], "raw_text":r[16], "created_at":r[17]
            })
        with open(path, "w", encoding="utf-8") as f:
            json.dump(out, f, ensure_ascii=False, indent=2)
        messagebox.showinfo("Exported", f"Saved JSON to:\n{path}")

    def export_selected_csv(self):
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        rows = self._rows_by_ids(ids)
        path = filedialog.asksaveasfilename(defaultextension=".csv", filetypes=[("CSV","*.csv")])
        if not path:
            return
        with open(path, "w", newline="", encoding="utf-8") as f:
            w = csv.writer(f)
            header = ["id","file_path","doc_type","doc_code","pqr","process","material","thickness","gas","filler",
                      "positions","issue_date","expiry_date","code_family","confidence","parser_excerpt","raw_text","created_at"]
            w.writerow(header)
            for r in rows:
                w.writerow([r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[15], r[16], r[17]])
        messagebox.showinfo("Exported", f"Saved CSV to:\n{path}")

    def delete_selected(self):
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select one or more rows first.")
            return
        if not messagebox.askyesno("Confirm Delete", f"Delete {len(ids)} record(s)? This cannot be undone."):
            return
        delete_records(ids)
        self.refresh()

    def edit_selected(self):
        ids = self._sel_ids()
        if not ids:
            messagebox.showinfo("No selection", "Select a row to edit.")
            return
        row_id = ids[0]
        row = fetch_record(row_id)
        if not row:
            messagebox.showerror("Not found", "Record not found.")
            return
        EditDialog(self, row_id, row, on_saved=self.refresh)

    def _on_double_click(self, event):
        # Open editor for the row under cursor
        item = self.tree.identify_row(event.y)
        if not item:
            return
        try:
            row_id = int(item)
        except Exception:
            return
        row = fetch_record(row_id)
        if not row:
            return
        EditDialog(self, row_id, row, on_saved=self.refresh)


class MainApp(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title(APP_TITLE)
        self.geometry("1280x800")
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
