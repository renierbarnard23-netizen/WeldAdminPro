"""
GUI Import Module for WeldAdmin Pro (Tkinter, standard library only)

- Provides an import window to select and ingest one or more PDF files
  using the ingestion pipeline (OCR → parse → validate → store).
- Displays results in a table with doc type/number, company, date, avg conf, and status.
- Shows validation issues for the selected row.
- Includes a simple FTS5 search bar against the same SQLite DB.

Requirements:
- Python 3.9+
- ingestion_pipeline.py (from the canvas) in the same folder or on PYTHONPATH
- Tesseract and Poppler installed and on PATH

Usage (standalone):
    python gui_import.py --db data.sqlite

Embedded usage:
    from gui_import import open_import_window
    open_import_window(db_path="data.sqlite")
"""
from __future__ import annotations

import argparse
import threading
import queue
from pathlib import Path
from typing import List, Optional
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

# Import your ingestion pipeline
try:
    from ingestion_pipeline import Repo, ingest_pdf
except Exception as e:
    raise SystemExit(
        "Could not import ingestion_pipeline. Ensure ingestion_pipeline.py is next to this file.\n" + str(e)
    )


# OLD
# class ImportWorker(threading.Thread):
#     def __init__(self, repo: Repo, files: List[Path], out_q: queue.Queue):
#         ...
#         self.repo = repo

# NEW
class ImportWorker(threading.Thread):
    def __init__(self, db_or_repo, files: List[Path], out_q: queue.Queue):
        super().__init__(daemon=True)
        self.db_or_repo = db_or_repo          # can be a Path/str OR a Repo
        self.files = files
        self.out_q = out_q

    def run(self):
        # Open/resolve a Repo in THIS thread
        repo = None
        owns_repo = False
        try:
            if hasattr(self.db_or_repo, "conn"):   # looks like a Repo already
                repo = self.db_or_repo
            else:
                # treat as path
                repo = Repo(Path(self.db_or_repo))
                owns_repo = True

            for f in self.files:
                try:
                    res = ingest_pdf(repo, f)
                    self.out_q.put(("RESULT", f, res))
                except Exception as e:
                    self.out_q.put(("ERROR", f, str(e)))
        finally:
            if owns_repo and repo is not None:
                try:
                    repo.conn.close()
                except Exception:
                    pass
        self.out_q.put(("DONE", None, None))

class ImportApp(ttk.Frame):
    def __init__(self, master: tk.Tk, db_path: Path):
        super().__init__(master)
        self.master = master
        self.db_path = Path(db_path)
        self.repo = Repo(self.db_path)

        self.pack(fill="both", expand=True)
        self._build_ui()
        self._wire_events()

        self.worker_q: queue.Queue = queue.Queue()
        self.worker: Optional[ImportWorker] = None

    # UI
    def _build_ui(self):
        self.master.title("WeldAdmin Pro – Import & Search")
        self.master.geometry("1000x620")
        self.master.minsize(900, 560)

        # Top bar
        top = ttk.Frame(self)
        top.pack(fill="x", padx=10, pady=8)

        self.btn_choose = ttk.Button(top, text="Choose PDFs…", command=self.on_choose)
        self.btn_choose.pack(side="left")

        self.btn_import = ttk.Button(top, text="Import", command=self.on_import, state="disabled")
        self.btn_import.pack(side="left", padx=(8, 0))

        self.lbl_status = ttk.Label(top, text="Idle")
        self.lbl_status.pack(side="right")

        # Middle: Treeview + Issues
        mid = ttk.Panedwindow(self, orient="horizontal")
        mid.pack(fill="both", expand=True, padx=10, pady=(0, 8))

        # Left pane: results table
        left = ttk.Frame(mid)
        self.tree = ttk.Treeview(left, columns=(
            "status", "doc_type", "doc_number", "company", "date", "avg_conf", "file"
        ), show="headings", height=12)
        self.tree.heading("status", text="Status")
        self.tree.heading("doc_type", text="Type")
        self.tree.heading("doc_number", text="Number")
        self.tree.heading("company", text="Company")
        self.tree.heading("date", text="Date")
        self.tree.heading("avg_conf", text="Avg Conf")
        self.tree.heading("file", text="File")

        self.tree.column("status", width=90, anchor="center")
        self.tree.column("doc_type", width=70, anchor="center")
        self.tree.column("doc_number", width=160)
        self.tree.column("company", width=200)
        self.tree.column("date", width=100, anchor="center")
        self.tree.column("avg_conf", width=80, anchor="e")
        self.tree.column("file", width=320)

        vsb = ttk.Scrollbar(left, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscroll=vsb.set)
        self.tree.grid(row=0, column=0, sticky="nsew")
        vsb.grid(row=0, column=1, sticky="ns")
        left.rowconfigure(0, weight=1)
        left.columnconfigure(0, weight=1)

        mid.add(left, weight=3)

        # Right pane: issues
        right = ttk.Frame(mid)
        ttk.Label(right, text="Validation / Import Messages:").pack(anchor="w")
        self.txt_issues = tk.Text(right, height=10, wrap="word")
        self.txt_issues.pack(fill="both", expand=True)
        mid.add(right, weight=2)

        # Bottom search bar + results
        bottom = ttk.LabelFrame(self, text="Search (FTS5)")
        bottom.pack(fill="both", expand=False, padx=10, pady=(0, 10))

        bar = ttk.Frame(bottom)
        bar.pack(fill="x", padx=8, pady=6)
        ttk.Label(bar, text="Query:").pack(side="left")
        self.ent_query = ttk.Entry(bar)
        self.ent_query.pack(side="left", fill="x", expand=True, padx=6)
        self.btn_search = ttk.Button(bar, text="Search", command=self.on_search)
        self.btn_search.pack(side="left")

        self.tree_search = ttk.Treeview(bottom, columns=(
            "id", "doc_type", "doc_number", "company", "date", "avg_conf", "snippet"
        ), show="headings", height=8)
        for c, t in [
            ("id", "#"), ("doc_type", "Type"), ("doc_number", "Number"), ("company", "Company"),
            ("date", "Date"), ("avg_conf", "Conf"), ("snippet", "Snippet")
        ]:
            self.tree_search.heading(c, text=t)
        self.tree_search.column("id", width=50, anchor="e")
        self.tree_search.column("doc_type", width=70, anchor="center")
        self.tree_search.column("doc_number", width=180)
        self.tree_search.column("company", width=200)
        self.tree_search.column("date", width=100, anchor="center")
        self.tree_search.column("avg_conf", width=70, anchor="e")
        self.tree_search.column("snippet", width=500)
        self.tree_search.pack(fill="both", expand=True, padx=8, pady=(0, 8))

    def _wire_events(self):
        self.tree.bind("<<TreeviewSelect>>", self.on_select_row)
        self.master.protocol("WM_DELETE_WINDOW", self.on_close)

    # Event handlers
    def on_choose(self):
        files = filedialog.askopenfilenames(
            title="Select WPS/PQR/WPQR PDFs",
            filetypes=[("PDF files", "*.pdf"), ("All files", "*.*")]
        )
        if not files:
            return
        self.selected_files = [Path(f) for f in files]
        self._populate_pending(files)
        self.btn_import.config(state="normal")
        self.lbl_status.config(text=f"Selected {len(files)} file(s)")

    def _populate_pending(self, files: List[str]):
        for f in files:
            self.tree.insert("", "end", values=("PENDING", "", "", "", "", "", str(f)))

    def on_import(self):
        if not getattr(self, "selected_files", None):
            messagebox.showinfo("Import", "Please choose one or more PDF files first.")
            return
        if self.worker and self.worker.is_alive():
            messagebox.showwarning("Busy", "An import is already running.")
            return
        self.lbl_status.config(text="Importing… (this can take a moment)")
        self.btn_import.config(state="disabled")
        self.worker_q = queue.Queue()
        self.worker = ImportWorker(self.db_path, self.selected_files, self.worker_q)
        self.worker.start()
        self.after(100, self._poll_worker)

    def _poll_worker(self):
        try:
            while True:
                evt, f, payload = self.worker_q.get_nowait()
                if evt == "RESULT":
                    self._append_result(f, payload)
                elif evt == "ERROR":
                    self._append_error(f, str(payload))
                elif evt == "DONE":
                    self.lbl_status.config(text="Done")
                    self.btn_import.config(state="normal")
                    return
        except queue.Empty:
            pass
        self.after(150, self._poll_worker)

    def _append_result(self, f: Path, res):
        # Find pending row with this file and update it
        iid = self._find_row_by_file(str(f))
        vals = (
            res.status,
            res.summary.get("doc_type", ""),
            res.summary.get("doc_number", ""),
            self._field_from_summary(res, "company"),
            self._field_from_summary(res, "date"),
            res.summary.get("avg_conf", ""),
            str(f),
        )
        if iid:
            self.tree.item(iid, values=vals)
        else:
            self.tree.insert("", "end", values=vals)
        # Store issues on the item for quick display
        issues_text = "\n".join([f"- {i.severity}: {i.field}: {i.message}" for i in res.issues]) if res.issues else "No issues."
        self.txt_issues.delete("1.0", "end")
        self.txt_issues.insert("1.0", issues_text)
        self.txt_issues.see("1.0")
        self.tree.set(self._find_row_by_file(str(f)), column="status", value=res.status)
        self.tree.item(self._find_row_by_file(str(f)), tags=(f"doc_{res.document_id}",))
        self.tree.tag_bind(f"doc_{res.document_id}", sequence="<ButtonRelease-1>")
        self.tree.set(self._find_row_by_file(str(f)), column="file", value=f"{f}")
        self.tree.set(self._find_row_by_file(str(f)), column="doc_type", value=res.summary.get("doc_type", ""))
        self.tree.set(self._find_row_by_file(str(f)), column="doc_number", value=res.summary.get("doc_number", ""))
        self.tree.set(self._find_row_by_file(str(f)), column="company", value=self._field_from_summary(res, "company"))
        self.tree.set(self._find_row_by_file(str(f)), column="date", value=self._field_from_summary(res, "date"))
        self.tree.set(self._find_row_by_file(str(f)), column="avg_conf", value=res.summary.get("avg_conf", ""))
        self.tree.item(self._find_row_by_file(str(f)), open=False)
        self.tree.item(self._find_row_by_file(str(f)), tags=(issues_text,))

    def _append_error(self, f: Path, msg: str):
        iid = self._find_row_by_file(str(f))
        vals = ("FAILED", "", "", "", "", "", str(f))
        if iid:
            self.tree.item(iid, values=vals)
        else:
            self.tree.insert("", "end", values=vals)
        self.tree.item(self._find_row_by_file(str(f)), tags=(f"- ERROR: {msg}",))
        self.txt_issues.delete("1.0", "end")
        self.txt_issues.insert("1.0", f"- ERROR: {msg}")
        self.txt_issues.see("1.0")


    def _find_row_by_file(self, file_path: str) -> Optional[str]:
        for iid in self.tree.get_children(""):
            vals = self.tree.item(iid, "values")
            if vals and vals[-1] == file_path:
                return iid
        return None

    def _field_from_summary(self, res, key: str) -> str:
        try:
            fields_json = res.summary.get("fields")
            if not fields_json:
                return ""
            import json
            data = json.loads(fields_json)
            return str(data.get(key, "") or "")
        except Exception:
            return ""

    def on_select_row(self, _evt=None):
        sel = self.tree.selection()
        self.txt_issues.delete("1.0", "end")
        if not sel:
            return
        iid = sel[0]
        tags = self.tree.item(iid, "tags")
        text = tags[0] if tags else ""
        if not text:
            text = "No messages."
        self.txt_issues.insert("1.0", text)

    def on_search(self):
        q = self.ent_query.get().strip()
        if not q:
            return
        rows = self.repo.search(q)
        for iid in self.tree_search.get_children(""):
            self.tree_search.delete(iid)
        for r in rows:
            self.tree_search.insert("", "end", values=(
                r.get("id", ""), r.get("doc_type", ""), r.get("doc_number", ""),
                r.get("company", ""), r.get("date", ""), r.get("avg_conf", ""), r.get("snippet", "")
            ))

    def on_close(self):
        try:
            self.master.destroy()
        except Exception:
            pass


def open_import_window(db_path: str | Path = "data.sqlite"):
    root = tk.Tk()
    style = ttk.Style(root)
    try:
        style.theme_use("clam")
    except Exception:
        pass
    app = ImportApp(root, Path(db_path))
    app.mainloop()


def main():
    parser = argparse.ArgumentParser(description="WeldAdmin Pro – GUI Import")
    parser.add_argument("--db", default="data.sqlite", help="Path to SQLite DB (will be created if missing)")
    args = parser.parse_args()
    open_import_window(args.db)


if __name__ == "__main__":
    main()
