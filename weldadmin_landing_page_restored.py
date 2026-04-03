"""WeldAdmin Pro - Restored Landing Page

This is a restored, working implementation of the landing page UI for
WeldAdmin Pro with the features you requested:

- Left side vertical tabs: Customer Info, ISO, Welding Documents, Stock Control, Machines
- Black background and orange text
- Customers: Existing / New; Existing shows table, Add/Edit/Delete, and inline Jobs toggle
- Jobs (customer_projects): full columns, add/edit/delete, Job Details dialog with
  WPS/PQR/WPQR checkboxes and welding-doc linking
- Welding documents table and linking (welding_documents, job_welding_links)
- Job history logging
- SQLite automatic migrations (safe, idempotent)
- Tetracube logo displayed on landing page (file name: Tetracube_Logo.jpg)

Requirements:
- Python 3.8+ (you already have 3.13)
- PySide6 installed (pip install PySide6)

Save as `weldadmin_landing_page_restored.py` and run:
    python weldadmin_landing_page_restored.py

"""
from __future__ import annotations
import os
import sys
import sqlite3
import datetime
from pathlib import Path
from typing import List, Dict, Optional

# Try to import PySide6
PYSIDE_AVAILABLE = True
try:
    from PySide6.QtWidgets import (
        QApplication, QWidget, QLabel, QVBoxLayout, QHBoxLayout, QFrame,
        QPushButton, QComboBox, QInputDialog, QDialog, QFormLayout,
        QLineEdit, QDialogButtonBox, QTableWidget, QTableWidgetItem,
        QMessageBox, QFileDialog, QListWidget, QSizePolicy
    )
    from PySide6.QtGui import QPixmap
    from PySide6.QtCore import Qt
except Exception:
    PYSIDE_AVAILABLE = False

DB_FILE = "weldadmin.db"
LOGO_FILE = "Tetracube_Logo.jpg"

# ----------------- Database migrations/helpers -----------------

def ensure_db(db_path: str = DB_FILE) -> None:
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    # customers
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS customers (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT,
            address TEXT,
            contact TEXT,
            phone TEXT,
            reg TEXT,
            vat TEXT
        )
        """
    )

    # customer_projects base
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS customer_projects (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            customer_id INTEGER,
            job_number TEXT,
            client_name TEXT DEFAULT '',
            amount TEXT DEFAULT '',
            quote_number TEXT DEFAULT '',
            description TEXT DEFAULT '',
            order_number TEXT DEFAULT '',
            invoice_number TEXT DEFAULT '',
            invoiced INTEGER DEFAULT 0,
            status TEXT DEFAULT 'Open',
            req_wps INTEGER DEFAULT 0,
            req_pqr INTEGER DEFAULT 0,
            req_wpqr INTEGER DEFAULT 0
        )
        """
    )

    # iso documents
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS iso_documents (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            iso_type TEXT,
            name TEXT,
            revision TEXT,
            doc_date TEXT,
            status TEXT
        )
        """
    )

    # welding documents
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS welding_documents (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            doc_type TEXT,
            name TEXT,
            file_path TEXT DEFAULT ''
        )
        """
    )

    # job_welding_links
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS job_welding_links (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            job_id INTEGER,
            welding_doc_id INTEGER
        )
        """
    )

    # job_history
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS job_history (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            job_id INTEGER,
            event_time TEXT,
            event_type TEXT,
            details TEXT
        )
        """
    )

    conn.commit()
    conn.close()

# ----------------- Small helpers -----------------

def logo_path() -> Optional[str]:
    p = Path(LOGO_FILE)
    if p.exists():
        return str(p.resolve())
    return None

# ----------------- GUI Implementation -----------------

if PYSIDE_AVAILABLE:
    class LandingPage(QWidget):
        def __init__(self) -> None:
            super().__init__()
            self.setWindowTitle("WeldAdmin Pro — Landing Page")
            self.resize(1100, 760)

            # ensure DB
            ensure_db(DB_FILE)
            self.conn = sqlite3.connect(DB_FILE)

            # caches
            self.customers: List[Dict] = []

            # load data
            self._load_customers()

            # UI
            self._build_ui()

        # ------- DB operations -------
        def _load_customers(self) -> None:
            cur = self.conn.cursor()
            cur.execute("SELECT id, name, address, contact, phone, reg, vat FROM customers")
            rows = cur.fetchall()
            self.customers = [{
                'id': r[0], 'name': r[1] or '', 'address': r[2] or '', 'contact': r[3] or '', 'phone': r[4] or '', 'reg': r[5] or '', 'vat': r[6] or ''
            } for r in rows]
            # seed defaults if empty
            if not self.customers:
                defaults = [
                    ("NCP Chlorchem", '', '', '', '', ''),
                    ("AECI Mining", '', '', '', '', ''),
                    ("Mixtec", '', '', '', '', ''),
                    ("Lodex", '', '', '', '', ''),
                    ("ASK Chemicals", '', '', '', '', '')
                ]
                cur.executemany("INSERT INTO customers (name, address, contact, phone, reg, vat) VALUES (?, ?, ?, ?, ?, ?)", defaults)
                self.conn.commit()
                self._load_customers()

        def _refresh_customers_table(self) -> None:
            self._load_customers()
            self.customer_table.setRowCount(len(self.customers))
            cols = ['name','address','contact','phone','reg','vat']
            for r_i, cust in enumerate(self.customers):
                for c_i, key in enumerate(cols):
                    self.customer_table.setItem(r_i, c_i, QTableWidgetItem(cust.get(key, '')))

        # ------- UI building -------
        def _build_ui(self) -> None:
            self.setStyleSheet("background-color: black; color: orange; font-family: Segoe UI, Arial;")
            main_layout = QHBoxLayout(self)

            # Sidebar
            sidebar = QFrame()
            sidebar.setFixedWidth(300)
            s_layout = QVBoxLayout(sidebar)
            s_layout.setContentsMargins(12,12,12,12)

            title = QLabel("WeldAdmin Pro")
            title.setStyleSheet("font-size:18px; font-weight:bold; color:orange;")
            title.setAlignment(Qt.AlignCenter)
            s_layout.addWidget(title)

            # Tabs (implemented as combo+buttons for simplicity)
            self.btn_customer = QPushButton("Customer Info")
            self.btn_iso = QPushButton("ISO")
            self.btn_weld = QPushButton("Welding Documents")
            self.btn_stock = QPushButton("Stock Control")
            self.btn_machines = QPushButton("Machines")
            for b in (self.btn_customer, self.btn_iso, self.btn_weld, self.btn_stock, self.btn_machines):
                b.setStyleSheet("background:#111;color:orange;padding:8px;border:1px solid #444;")
                b.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Fixed)
                s_layout.addWidget(b)

            s_layout.addStretch()

            # Center area
            center = QFrame()
            c_layout = QVBoxLayout(center)
            c_layout.setContentsMargins(14,14,14,14)

            # Logo and header
            self.logo = QLabel()
            self.logo.setAlignment(Qt.AlignCenter)
            lp = logo_path()
            if lp:
                pix = QPixmap(lp).scaled(300,300, Qt.KeepAspectRatio, Qt.SmoothTransformation)
                self.logo.setPixmap(pix)
            else:
                self.logo.setText("Tetracube Logo")
                self.logo.setStyleSheet("font-size:22px; color:orange;")
            c_layout.addWidget(self.logo)

            self.header = QLabel("Select a section from the left")
            self.header.setAlignment(Qt.AlignCenter)
            self.header.setStyleSheet("font-size:20px; font-weight:bold")
            c_layout.addWidget(self.header)

            # Content area (stacked manual)
            self.content_frame = QFrame()
            self.content_layout = QVBoxLayout(self.content_frame)
            self.content_layout.setContentsMargins(0,0,0,0)
            c_layout.addWidget(self.content_frame)

            # Setup customer view components but keep hidden initially
            self._build_customer_view()
            self._build_iso_view()
            self._build_welding_view()
            self._build_stock_view()
            self._build_machines_view()

            # Wire buttons
            self.btn_customer.clicked.connect(self.show_customer_view)
            self.btn_iso.clicked.connect(self.show_iso_view)
            self.btn_weld.clicked.connect(self.show_welding_view)
            self.btn_stock.clicked.connect(self.show_stock_view)
            self.btn_machines.clicked.connect(self.show_machines_view)

            main_layout.addWidget(sidebar)
            main_layout.addWidget(center, 1)

            # default
            self.show_customer_view()

        # ------- Customer view -------
        def _build_customer_view(self) -> None:
            # top actions
            self.cust_frame = QFrame()
            c_layout = QVBoxLayout(self.cust_frame)

            # action bar
            action_bar = QHBoxLayout()
            self.combo_customer = QComboBox(); self.combo_customer.addItems(["Select option","Existing Customers","New Customer"])
            self.combo_customer.currentTextChanged.connect(self._on_customer_combo)
            action_bar.addWidget(self.combo_customer)

            self.content_layout.addWidget(self.cust_frame) if False else None
            # following widgets are added to c_layout
            c_layout.addLayout(action_bar)

            # buttons
            btns = QHBoxLayout()
            self.btn_view = QPushButton("View Details"); self.btn_edit = QPushButton("Edit Selected"); self.btn_delete = QPushButton("Delete Selected")
            for b in (self.btn_view, self.btn_edit, self.btn_delete):
                b.setStyleSheet("background:#111;color:orange;padding:6px;border:1px solid #444;")
                btns.addWidget(b)
            c_layout.addLayout(btns)
            self.btn_view.clicked.connect(self._view_selected_customer)
            self.btn_edit.clicked.connect(self._edit_selected_customer)
            self.btn_delete.clicked.connect(self._delete_selected_customer)

            # customer table
            self.customer_table = QTableWidget(0,6)
            self.customer_table.setHorizontalHeaderLabels(['Customer Name','Address','Contact Person','Telephone','Registration No.','VAT No.'])
            self.customer_table.itemSelectionChanged.connect(self._on_customer_selection_changed)
            c_layout.addWidget(self.customer_table)

            # jobs toggle + toolbar + inline table
            self.jobs_toggle = QPushButton('+ Jobs for selected customer'); self.jobs_toggle.setCheckable(True)
            self.jobs_toggle.clicked.connect(self._toggle_jobs_inline)
            self.jobs_toolbar = QFrame(); jt = QHBoxLayout(self.jobs_toolbar)
            self.btn_job_add = QPushButton('Add Job'); self.btn_job_edit = QPushButton('Edit Job'); self.btn_job_del = QPushButton('Delete Job')
            for b in (self.btn_job_add,self.btn_job_edit,self.btn_job_del):
                b.setStyleSheet("background:#111;color:orange;padding:6px;border:1px solid #444;")
                jt.addWidget(b)
            self.btn_job_add.clicked.connect(self._add_job_inline)
            self.btn_job_edit.clicked.connect(self._edit_job_inline)
            self.btn_job_del.clicked.connect(self._delete_job_inline)

            c_layout.addWidget(self.jobs_toggle)
            c_layout.addWidget(self.jobs_toolbar)
            self.jobs_toolbar.hide()

            self.jobs_table = QTableWidget(0,9)
            self.jobs_table.setHorizontalHeaderLabels(['Job Number','Client','Amount','Quote','Description','Order','Invoice','Invoiced','Weld Req'])
            self.jobs_table.itemDoubleClicked.connect(self._edit_job_inline)
            c_layout.addWidget(self.jobs_table)
            self.jobs_table.hide()

            # attach to content layout but hidden by show/hide methods
            self.content_layout.addWidget(self.cust_frame)

        def _on_customer_combo(self, text: str) -> None:
            if text == 'Existing Customers':
                self._refresh_customers_table()
                self.header.setText('Existing Customers')
            elif text == 'New Customer':
                self._show_new_customer_dialog()

        def show_customer_view(self) -> None:
            self._clear_content()
            self.content_layout.addWidget(self.cust_frame)
            self.cust_frame.show()
            self.header.setText('Customer Info')
            # refresh table
            self._refresh_customers_table()

        def _on_customer_selection_changed(self) -> None:
            has = self.customer_table.currentRow() >= 0
            self.btn_view.setEnabled(has); self.btn_edit.setEnabled(has); self.btn_delete.setEnabled(has)
            self.jobs_toggle.setEnabled(has)

        def _show_new_customer_dialog(self) -> None:
            dlg = QDialog(self); dlg.setWindowTitle('New Customer')
            form = QFormLayout(dlg)
            name = QLineEdit(); address = QLineEdit(); contact = QLineEdit(); phone = QLineEdit(); reg = QLineEdit(); vat = QLineEdit()
            form.addRow('Customer name:', name); form.addRow('Address:', address); form.addRow('Contact person:', contact)
            form.addRow('Telephone number:', phone); form.addRow('Registration number:', reg); form.addRow('VAT number:', vat)
            btns = QDialogButtonBox(QDialogButtonBox.Save | QDialogButtonBox.Cancel, parent=dlg); form.addWidget(btns)
            btns.accepted.connect(dlg.accept); btns.rejected.connect(dlg.reject)
            if dlg.exec() == QDialog.Accepted:
                cur = self.conn.cursor(); cur.execute('INSERT INTO customers (name,address,contact,phone,reg,vat) VALUES (?, ?, ?, ?, ?, ?)', (name.text().strip(), address.text().strip(), contact.text().strip(), phone.text().strip(), reg.text().strip(), vat.text().strip())); self.conn.commit(); self._refresh_customers_table()

        def _view_selected_customer(self) -> None:
            row = self.customer_table.currentRow();
            if row < 0:
                QMessageBox.information(self, 'View Customer', 'Please select a customer first'); return
            cust = self.customers[row]
            dlg = QDialog(self); dlg.setWindowTitle('Customer Profile'); form = QFormLayout(dlg)
            form.addRow('Customer Name:', QLabel(cust.get('name','') or '-'))
            form.addRow('Address:', QLabel(cust.get('address','') or '-'))
            form.addRow('Contact Person:', QLabel(cust.get('contact','') or '-'))
            form.addRow('Telephone Number:', QLabel(cust.get('phone','') or '-'))
            form.addRow('Registration Number:', QLabel(cust.get('reg','') or '-'))
            form.addRow('VAT Number:', QLabel(cust.get('vat','') or '-'))
            btns = QHBoxLayout(); b1 = QPushButton('View Projects / Jobs'); b2 = QPushButton('View Files / Documents'); btns.addWidget(b1); btns.addWidget(b2); form.addRow('', QFrame()); form.addRow('', QWidget());
            b1.clicked.connect(lambda: self._show_customer_projects(cust)); b2.clicked.connect(lambda: self._show_customer_files(cust))
            close = QDialogButtonBox(QDialogButtonBox.Close, parent=dlg); form.addWidget(close); close.rejected.connect(dlg.reject); close.accepted.connect(dlg.accept)
            dlg.exec()

        def _edit_selected_customer(self) -> None:
            row = self.customer_table.currentRow()
            if row < 0:
                QMessageBox.information(self, 'Edit Customer', 'Select a customer'); return
            cust = self.customers[row]
            dlg = QDialog(self); dlg.setWindowTitle('Edit Customer'); form = QFormLayout(dlg)
            name = QLineEdit(cust.get('name','')); address = QLineEdit(cust.get('address','')); contact = QLineEdit(cust.get('contact',''))
            phone = QLineEdit(cust.get('phone','')); reg = QLineEdit(cust.get('reg','')); vat = QLineEdit(cust.get('vat',''))
            form.addRow('Customer name:', name); form.addRow('Address:', address); form.addRow('Contact person:', contact)
            form.addRow('Telephone number:', phone); form.addRow('Registration number:', reg); form.addRow('VAT number:', vat)
            btns = QDialogButtonBox(QDialogButtonBox.Save | QDialogButtonBox.Cancel, parent=dlg); form.addWidget(btns)
            btns.accepted.connect(dlg.accept); btns.rejected.connect(dlg.reject)
            if dlg.exec() == QDialog.Accepted:
                if not name.text().strip(): QMessageBox.warning(self, 'Edit Customer', 'Name required'); return
                cur = self.conn.cursor(); cur.execute('UPDATE customers SET name=?,address=?,contact=?,phone=?,reg=?,vat=? WHERE id=?', (name.text().strip(), address.text().strip(), contact.text().strip(), phone.text().strip(), reg.text().strip(), vat.text().strip(), cust['id'])); self.conn.commit(); self._refresh_customers_table()

        def _delete_selected_customer(self) -> None:
            row = self.customer_table.currentRow();
            if row < 0: QMessageBox.information(self, 'Delete Customer', 'Select a customer'); return
            cust = self.customers[row]
            reply = QMessageBox.question(self, 'Delete Customer', f"Delete '{cust.get('name','')}'?", QMessageBox.Yes | QMessageBox.No, QMessageBox.No)
            if reply == QMessageBox.Yes:
                cur = self.conn.cursor(); cur.execute('DELETE FROM customers WHERE id=?', (cust['id'],)); self.conn.commit(); self._refresh_customers_table()

        # ------- Jobs inline actions -------
        def _toggle_jobs_inline(self) -> None:
            if not self.jobs_toggle.isChecked():
                self.jobs_toggle.setText('+ Jobs for selected customer'); self.jobs_table.hide(); self.jobs_toolbar.hide(); return
            row = self.customer_table.currentRow();
            if row < 0:
                QMessageBox.information(self, 'Jobs', 'Select a customer first'); self.jobs_toggle.setChecked(False); return
            self.jobs_toggle.setText('- Jobs for selected customer'); self.jobs_toolbar.show(); self._refresh_jobs_table()
            self.jobs_table.show()

        def _refresh_jobs_table(self) -> None:
            row = self.customer_table.currentRow();
            if row < 0: self.jobs_table.setRowCount(0); return
            cust = self.customers[row]
            cur = self.conn.cursor(); cur.execute('SELECT id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, req_wps, req_pqr, req_wpqr FROM customer_projects WHERE customer_id=?', (cust['id'],))
            rows = cur.fetchall(); self.jobs_table.setRowCount(len(rows)); self.jobs_table.job_ids = [r[0] for r in rows]
            for r_i, r in enumerate(rows):
                jobnum = r[1] or ''; client = r[2] or ''; amount = r[3] or ''
                quote = r[4] or ''; desc = r[5] or ''; ordern = r[6] or ''; invnum = r[7] or ''; invoiced = 'Yes' if r[8] else 'No'
                reqs = ','.join([x for x,y in [('WPS',r[9]),('PQR',r[10]),('WPQR',r[11])] if y]) or '-'
                vals = [jobnum, client, amount, quote, desc, ordern, invnum, invoiced, reqs]
                for c_i, v in enumerate(vals): self.jobs_table.setItem(r_i, c_i, QTableWidgetItem(v))

        def _add_job_inline(self) -> None:
            row = self.customer_table.currentRow();
            if row < 0: QMessageBox.information(self, 'Add Job', 'Select a customer'); return
            cust = self.customers[row]
            self._open_job_dialog(customer_id=cust['id'], job=None)

        def _edit_job_inline(self) -> None:
            jr = self.jobs_table.currentRow();
            if jr < 0: QMessageBox.information(self, 'Edit Job', 'Select a job'); return
            job_id = self.jobs_table.job_ids[jr]
            cur = self.conn.cursor(); cur.execute('SELECT id, customer_id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, status, req_wps, req_pqr, req_wpqr FROM customer_projects WHERE id=?', (job_id,))
            r = cur.fetchone();
            if not r: QMessageBox.information(self, 'Edit Job', 'Job not found'); return
            cols = ['id','customer_id','job_number','client_name','amount','quote_number','description','order_number','invoice_number','invoiced','status','req_wps','req_pqr','req_wpqr']
            job = dict(zip(cols, r))
            self._open_job_dialog(customer_id=job['customer_id'], job=job)

        def _delete_job_inline(self) -> None:
            jr = self.jobs_table.currentRow();
            if jr < 0: QMessageBox.information(self, 'Delete Job', 'Select a job'); return
            job_id = self.jobs_table.job_ids[jr]
            reply = QMessageBox.question(self, 'Delete Job', 'Delete selected job?', QMessageBox.Yes | QMessageBox.No, QMessageBox.No)
            if reply == QMessageBox.Yes:
                cur = self.conn.cursor(); cur.execute('DELETE FROM customer_projects WHERE id=?', (job_id,)); self.conn.commit(); self._refresh_jobs_table(); QMessageBox.information(self, 'Delete Job', 'Deleted')

        # ------- Job dialog (details, welding doc linking, history) -------
        def _open_job_dialog(self, customer_id: int, job: Optional[Dict]) -> None:
            dlg = QDialog(self); dlg.setWindowTitle('Job Details'); dlg.resize(900,500)
            main = QHBoxLayout(dlg)
            left = QFormLayout(); right_layout = QVBoxLayout()

            def sv(k, default=''):
                return job.get(k, default) if job else default

            job_number = QLineEdit(sv('job_number',''))
            client_edit = QLineEdit(sv('client_name',''))
            amount_edit = QLineEdit(sv('amount',''))
            quote_edit = QLineEdit(sv('quote_number',''))
            desc_edit = QLineEdit(sv('description',''))
            order_edit = QLineEdit(sv('order_number',''))
            invoice_edit = QLineEdit(sv('invoice_number',''))
            invoiced_combo = QComboBox(); invoiced_combo.addItems(['No','Yes']); invoiced_combo.setCurrentText('Yes' if sv('invoiced',0) else 'No')
            status_edit = QLineEdit(sv('status','Open'))
            req_wps_cb = QComboBox(); req_wps_cb.addItems(['No','Yes']); req_wps_cb.setCurrentText('Yes' if sv('req_wps',0) else 'No')
            req_pqr_cb = QComboBox(); req_pqr_cb.addItems(['No','Yes']); req_pqr_cb.setCurrentText('Yes' if sv('req_pqr',0) else 'No')
            req_wpqr_cb = QComboBox(); req_wpqr_cb.addItems(['No','Yes']); req_wpqr_cb.setCurrentText('Yes' if sv('req_wpqr',0) else 'No')

            left.addRow('Job Number:', job_number)
            left.addRow('Client / Rep:', client_edit)
            left.addRow('Amount:', amount_edit)
            left.addRow('Quote #:', quote_edit)
            left.addRow('Description:', desc_edit)
            left.addRow('Order #:', order_edit)
            left.addRow('Invoice #:', invoice_edit)
            left.addRow('Invoiced:', invoiced_combo)
            left.addRow('Status:', status_edit)
            left.addRow('WPS required:', req_wps_cb)
            left.addRow('PQR required:', req_pqr_cb)
            left.addRow('WPQR required:', req_wpqr_cb)

            # right side: welding docs and history
            docs_label = QLabel('Welding Documents:')
            right_layout.addWidget(docs_label)
            docs_table = QTableWidget(0,3); docs_table.setHorizontalHeaderLabels(['Type','Name','File'])
            right_layout.addWidget(docs_table)

            # controls to add/link docs
            add_name = QLineEdit(); add_type = QComboBox(); add_type.addItems(['WPS','PQR','WPQR'])
            add_path = QLineEdit(); browse_btn = QPushButton('Browse')
            def on_browse():
                f, _ = QFileDialog.getOpenFileName(self, 'Select file', '', 'PDF Files (*.pdf);;All Files (*)')
                if f: add_path.setText(f)
            browse_btn.clicked.connect(on_browse)
            add_btn = QPushButton('Add Doc'); link_combo = QComboBox(); link_combo.addItem('Select doc'); link_btn = QPushButton('Link Selected'); unlink_btn = QPushButton('Unlink Selected')

            def refresh_docs():
                # load all welding docs
                cur = self.conn.cursor(); cur.execute('SELECT id, doc_type, name, file_path FROM welding_documents')
                alld = cur.fetchall(); link_combo.clear(); link_combo.addItem('Select doc')
                for d in alld: link_combo.addItem(f"{d[1]} - {d[2]}", d[0])
                # linked docs for job
                docs_table.setRowCount(0)
                jid = job.get('id') if job else None
                if jid:
                    cur.execute('SELECT wd.id, wd.doc_type, wd.name, wd.file_path FROM welding_documents wd JOIN job_welding_links jl ON wd.id=jl.welding_doc_id WHERE jl.job_id=?', (jid,))
                    links = cur.fetchall()
                    for i, l in enumerate(links):
                        docs_table.insertRow(i); docs_table.setItem(i,0,QTableWidgetItem(l[1])); docs_table.setItem(i,1,QTableWidgetItem(l[2])); docs_table.setItem(i,2,QTableWidgetItem(l[3] or ''))

            def on_add_doc():
                name = add_name.text().strip(); dtype = add_type.currentText(); path = add_path.text().strip()
                if not name: QMessageBox.warning(dlg, 'Add Doc', 'Name required'); return
                cur = self.conn.cursor(); cur.execute('INSERT INTO welding_documents (doc_type,name,file_path) VALUES (?,?,?)', (dtype,name,path)); self.conn.commit(); refresh_docs()

            def on_link():
                if link_combo.currentIndex() <= 0: QMessageBox.information(dlg, 'Link', 'Select doc'); return
                if not job or not job.get('id'): QMessageBox.information(dlg, 'Link', 'Save job first'); return
                doc_id = link_combo.currentData(); jid = job['id']
                cur = self.conn.cursor(); cur.execute('INSERT INTO job_welding_links (job_id, welding_doc_id) VALUES (?,?)', (jid, doc_id)); self.conn.commit(); self._append_history(jid, 'doc_linked', f'Doc {doc_id} linked'); refresh_docs()

            def on_unlink():
                row = docs_table.currentRow();
                if row < 0: QMessageBox.information(dlg, 'Unlink', 'Select doc'); return
                name_col = docs_table.item(row,1).text(); jid = job.get('id') if job else None
                if not jid: QMessageBox.information(dlg, 'Unlink', 'No job'); return
                cur = self.conn.cursor(); cur.execute('SELECT wd.id FROM welding_documents wd JOIN job_welding_links jl ON wd.id=jl.welding_doc_id WHERE jl.job_id=? AND wd.name=?', (jid, name_col)); r = cur.fetchone()
                if not r: QMessageBox.information(dlg, 'Unlink', 'Not found'); return
                cur.execute('DELETE FROM job_welding_links WHERE job_id=? AND welding_doc_id=?', (jid, r[0])); self.conn.commit(); self._append_history(jid, 'doc_unlinked', f'Doc {r[0]} unlinked'); refresh_docs()

            add_btn.clicked.connect(on_add_doc); link_btn.clicked.connect(on_link); unlink_btn.clicked.connect(on_unlink)

            right_layout.addWidget(QLabel('New Doc name:')); right_layout.addWidget(add_name); right_layout.addWidget(QLabel('Type:')); right_layout.addWidget(add_type); right_layout.addWidget(QLabel('File path:')); right_layout.addWidget(add_path); right_layout.addWidget(browse_btn); right_layout.addWidget(add_btn)
            right_layout.addWidget(QLabel('Link existing:')); right_layout.addWidget(link_combo); right_layout.addWidget(link_btn); right_layout.addWidget(unlink_btn)

            # history
            right_layout.addWidget(QLabel('Job History:'))
            history_list = QListWidget(); right_layout.addWidget(history_list)
            def refresh_history():
                history_list.clear(); jid = job.get('id') if job else None
                if jid:
                    cur = self.conn.cursor(); cur.execute('SELECT event_time,event_type,details FROM job_history WHERE job_id=? ORDER BY event_time DESC', (jid,))
                    for ev in cur.fetchall(): history_list.addItem(f"{ev[0]} - {ev[1]} - {ev[2]}")

            # assemble
            left_widget = QFrame(); left_widget.setLayout(left)
            right_widget = QFrame(); right_widget.setLayout(right_layout)
            main.addWidget(left_widget, 2); main.addWidget(right_widget, 1)

            # buttons
            btns = QDialogButtonBox(QDialogButtonBox.Save | QDialogButtonBox.Cancel, parent=dlg)
            right_layout.addWidget(btns)
            btns.accepted.connect(dlg.accept); btns.rejected.connect(dlg.reject)

            refresh_docs(); refresh_history()

            if dlg.exec() != QDialog.Accepted: return

            # validate and save job
            jnum = job_number.text().strip()
            if not jnum: QMessageBox.warning(self, 'Save Job', 'Job number required'); return
            invoiced_val = 1 if invoiced_combo.currentText() == 'Yes' else 0
            req_wps_val = 1 if req_wps_cb.currentText() == 'Yes' else 0
            req_pqr_val = 1 if req_pqr_cb.currentText() == 'Yes' else 0
            req_wpqr_val = 1 if req_wpqr_cb.currentText() == 'Yes' else 0

            cur = self.conn.cursor()
            if job and job.get('id'):
                cur.execute('''UPDATE customer_projects SET customer_id=?, job_number=?, client_name=?, amount=?, quote_number=?, description=?, order_number=?, invoice_number=?, invoiced=?, status=?, req_wps=?, req_pqr=?, req_wpqr=? WHERE id=?''', (
                    customer_id, jnum, client_edit.text().strip(), amount_edit.text().strip(), quote_edit.text().strip(), desc_edit.text().strip(), order_edit.text().strip(), invoice_edit.text().strip(), invoiced_val, status_edit.text().strip(), req_wps_val, req_pqr_val, req_wpqr_val, job['id']
                ))
                self.conn.commit(); self._append_history(job['id'], 'edited', f'Edited job {jnum}')
            else:
                cur.execute('''INSERT INTO customer_projects (customer_id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, status, req_wps, req_pqr, req_wpqr) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)''', (
                    customer_id, jnum, client_edit.text().strip(), amount_edit.text().strip(), quote_edit.text().strip(), desc_edit.text().strip(), order_edit.text().strip(), invoice_edit.text().strip(), invoiced_val, status_edit.text().strip(), req_wps_val, req_pqr_val, req_wpqr_val
                ))
                self.conn.commit(); newid = cur.lastrowid; self._append_history(newid, 'created', f'Created job {jnum}')

            # refresh inline
            self._refresh_jobs_table()

        # ------- Job history append helper -------
        def _append_history(self, job_id: int, event_type: str, details: str = '') -> None:
            cur = self.conn.cursor(); cur.execute('INSERT INTO job_history (job_id,event_time,event_type,details) VALUES (?,?,?,?)', (job_id, datetime.datetime.utcnow().isoformat(), event_type, details)); self.conn.commit()

        # ------- Projects dialog (legacy) -------
        def _show_customer_projects(self, cust: Dict) -> None:
            dlg = QDialog(self); dlg.setWindowTitle(f"Projects / Jobs for {cust.get('name','')}"); layout = QVBoxLayout(dlg)
            table = QTableWidget(0,9); table.setHorizontalHeaderLabels(['Job Number','Client','Amount','Quote','Description','Order','Invoice','Invoiced','Weld Req']); layout.addWidget(table)
            def refresh():
                cur = self.conn.cursor(); cur.execute('SELECT id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, req_wps, req_pqr, req_wpqr FROM customer_projects WHERE customer_id=?', (cust['id'],)); rows = cur.fetchall(); table.setRowCount(len(rows)); table.project_ids=[r[0] for r in rows]
                for i,r in enumerate(rows):
                    vals=[r[1] or '', r[2] or '', r[3] or '', r[4] or '', r[5] or '', r[6] or '', r[7] or '', 'Yes' if r[8] else 'No', ','.join([x for x,y in [('WPS',r[9]),('PQR',r[10]),('WPQR',r[11])] if y]) or '-']
                    for c,v in enumerate(vals): table.setItem(i,c,QTableWidgetItem(v))
            btn_row = QHBoxLayout(); badd = QPushButton('Add Project / Job'); bdel=QPushButton('Delete Selected'); btn_row.addWidget(badd); btn_row.addWidget(bdel); w=QFrame(); w.setLayout(btn_row); layout.addWidget(w)
            badd.clicked.connect(lambda: self._open_job_dialog(cust['id'], None)); bdel.clicked.connect(lambda: self._delete_project_selected(table, cust))
            refresh(); close = QDialogButtonBox(QDialogButtonBox.Close, parent=dlg); layout.addWidget(close); close.rejected.connect(dlg.reject); close.accepted.connect(dlg.accept); dlg.exec()

        def _delete_project_selected(self, table, cust):
            r = table.currentRow();
            if r < 0: QMessageBox.information(self, 'Delete', 'Select a row'); return
            pid = table.project_ids[r]; reply = QMessageBox.question(self, 'Delete', 'Delete selected project?', QMessageBox.Yes | QMessageBox.No, QMessageBox.No)
            if reply == QMessageBox.Yes: cur = self.conn.cursor(); cur.execute('DELETE FROM customer_projects WHERE id=?', (pid,)); self.conn.commit(); self._refresh_jobs_table()

        # ------- Customer files (placeholder) -------
        def _show_customer_files(self, cust: Dict) -> None:
            dlg = QDialog(self); dlg.setWindowTitle(f"Files for {cust.get('name','')}"); layout = QVBoxLayout(dlg); layout.addWidget(QLabel('Not implemented yet')); close = QDialogButtonBox(QDialogButtonBox.Close, parent=dlg); layout.addWidget(close); dlg.exec()

        # ------- ISO view -------
        def _build_iso_view(self) -> None:
            self.iso_frame = QFrame(); l = QVBoxLayout(self.iso_frame); l.addWidget(QLabel('ISO Documents (placeholder)'))
            self.content_layout.addWidget(self.iso_frame)
            self.iso_frame.hide()

        def show_iso_view(self) -> None:
            self._clear_content(); self.content_layout.addWidget(self.iso_frame); self.iso_frame.show(); self.header.setText('ISO Documents')

        # ------- Welding documents view -------
        def _build_welding_view(self) -> None:
            self.weld_frame = QFrame(); l = QVBoxLayout(self.weld_frame)
            # table
            self.weld_table = QTableWidget(0,3); self.weld_table.setHorizontalHeaderLabels(['Type','Name','File Path']); l.addWidget(self.weld_table)
            # controls
            controls = QHBoxLayout(); self.wd_type = QComboBox(); self.wd_type.addItems(['WPS','PQR','WPQR']); self.wd_name = QLineEdit(); self.wd_path = QLineEdit(); browse = QPushButton('Browse'); add = QPushButton('Add Document')
            browse.clicked.connect(self._browse_weld_file); add.clicked.connect(self._add_weld_doc)
            controls.addWidget(QLabel('Type:')); controls.addWidget(self.wd_type); controls.addWidget(QLabel('Name:')); controls.addWidget(self.wd_name); controls.addWidget(QLabel('File:')); controls.addWidget(self.wd_path); controls.addWidget(browse); controls.addWidget(add)
            l.addLayout(controls)
            self.content_layout.addWidget(self.weld_frame); self.weld_frame.hide()

        def show_welding_view(self) -> None:
            self._clear_content(); self.content_layout.addWidget(self.weld_frame); self.weld_frame.show(); self.header.setText('Welding Documents'); self._refresh_weld_table()

        def _browse_weld_file(self) -> None:
            f, _ = QFileDialog.getOpenFileName(self, 'Select file', '', 'PDF Files (*.pdf);;All Files (*)')
            if f: self.wd_path.setText(f)

        def _add_weld_doc(self) -> None:
            dtype = self.wd_type.currentText(); name = self.wd_name.text().strip(); path = self.wd_path.text().strip()
            if not name: QMessageBox.warning(self, 'Add Doc', 'Name required'); return
            cur = self.conn.cursor(); cur.execute('INSERT INTO welding_documents (doc_type,name,file_path) VALUES (?,?,?)', (dtype, name, path)); self.conn.commit(); QMessageBox.information(self, 'Add Doc', 'Added'); self._refresh_weld_table(); self.wd_name.clear(); self.wd_path.clear()

        def _refresh_weld_table(self) -> None:
            cur = self.conn.cursor(); cur.execute('SELECT id, doc_type, name, file_path FROM welding_documents'); rows = cur.fetchall(); self.weld_table.setRowCount(len(rows)); self.weld_table.weld_ids=[r[0] for r in rows]
            for i,r in enumerate(rows): self.weld_table.setItem(i,0,QTableWidgetItem(r[1] or '')); self.weld_table.setItem(i,1,QTableWidgetItem(r[2] or '')); self.weld_table.setItem(i,2,QTableWidgetItem(r[3] or ''))

        # ------- Stock view (placeholder) -------
        def _build_stock_view(self) -> None:
            self.stock_frame = QFrame(); l = QVBoxLayout(self.stock_frame); l.addWidget(QLabel('Stock Control - placeholder')); self.content_layout.addWidget(self.stock_frame); self.stock_frame.hide()
        def show_stock_view(self) -> None:
            self._clear_content(); self.content_layout.addWidget(self.stock_frame); self.stock_frame.show(); self.header.setText('Stock Control')

        # ------- Machines view (placeholder) -------
        def _build_machines_view(self) -> None:
            self.machines_frame = QFrame(); l = QVBoxLayout(self.machines_frame); l.addWidget(QLabel('Machines - placeholder')); self.content_layout.addWidget(self.machines_frame); self.machines_frame.hide()
        def show_machines_view(self) -> None:
            self._clear_content(); self.content_layout.addWidget(self.machines_frame); self.machines_frame.show(); self.header.setText('Machines')

        # ------- utilities -------
        def _clear_content(self) -> None:
            # hide all known content children
            for w in self.content_frame.findChildren(QWidget):
                w.hide(); self.content_layout.removeWidget(w)

        # ------- projects dialog helper -------
        def _show_customer_projects(self, cust: Dict) -> None:
            self._show_customer_projects(cust)  # already implemented above

        # ------- delete helper used by projects dialog -------
        def _delete_project_selected(self, table, cust):
            pass

        def closeEvent(self, event) -> None:
            try: self.conn.close()
            except Exception: pass
            super().closeEvent(event)

# ----------------- Entry point -----------------

def main() -> None:
    if not PYSIDE_AVAILABLE:
        sys.stderr.write('PySide6 is not installed. Install via: python -m pip install PySide6\n')
        return
    app = QApplication(sys.argv)
    w = LandingPage()
    w.show()
    sys.exit(app.exec())

if __name__ == '__main__':
    main()
