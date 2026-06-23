"""jobs.py
Job dialog and helpers.
"""
from typing import Optional, Dict
from PySide6.QtWidgets import QDialog, QFormLayout, QLineEdit, QComboBox, QDialogButtonBox, QMessageBox, QFileDialog, QLabel, QVBoxLayout, QTableWidget, QTableWidgetItem, QListWidget, QPushButton
import sqlite3
import datetime


def open_job_dialog(parent, conn: sqlite3.Connection, customer_id: int, job: Optional[Dict]):
    dlg = QDialog(parent); dlg.setWindowTitle('Job Details'); dlg.resize(900,500)
    main_layout = QVBoxLayout(dlg)
    form = QFormLayout()
    main_layout.addLayout(form)

    job_number = QLineEdit(job.get('job_number','') if job else '')
    client = QLineEdit(job.get('client_name','') if job else '')
    amount = QLineEdit(job.get('amount','') if job else '')
    quote = QLineEdit(job.get('quote_number','') if job else '')
    desc = QLineEdit(job.get('description','') if job else '')
    ordern = QLineEdit(job.get('order_number','') if job else '')
    inv = QLineEdit(job.get('invoice_number','') if job else '')
    invoiced = QComboBox(); invoiced.addItems(['No','Yes']); invoiced.setCurrentText('Yes' if job and job.get('invoiced') else 'No')

    req_wps = QComboBox(); req_wps.addItems(['No','Yes']); req_wps.setCurrentText('Yes' if job and job.get('req_wps') else 'No')
    req_pqr = QComboBox(); req_pqr.addItems(['No','Yes']); req_pqr.setCurrentText('Yes' if job and job.get('req_pqr') else 'No')
    req_wpqr = QComboBox(); req_wpqr.addItems(['No','Yes']); req_wpqr.setCurrentText('Yes' if job and job.get('req_wpqr') else 'No')

    form.addRow('Job Number:', job_number)
    form.addRow('Client / Rep:', client)
    form.addRow('Amount:', amount)
    form.addRow('Quote #:', quote)
    form.addRow('Description:', desc)
    form.addRow('Order #:', ordern)
    form.addRow('Invoice #:', inv)
    form.addRow('Invoiced:', invoiced)
    form.addRow('WPS required:', req_wps)
    form.addRow('PQR required:', req_pqr)
    form.addRow('WPQR required:', req_wpqr)

    buttons = QDialogButtonBox(QDialogButtonBox.Save | QDialogButtonBox.Cancel, parent=dlg)
    main_layout.addWidget(buttons)
    buttons.accepted.connect(dlg.accept); buttons.rejected.connect(dlg.reject)

    if dlg.exec() != QDialog.Accepted:
        return False

    # validate and save
    jnum = job_number.text().strip()
    if not jnum:
        QMessageBox.warning(parent, 'Save Job', 'Job number required')
        return False
    invoiced_val = 1 if invoiced.currentText()=='Yes' else 0
    req_wps_val = 1 if req_wps.currentText()=='Yes' else 0
    req_pqr_val = 1 if req_pqr.currentText()=='Yes' else 0
    req_wpqr_val = 1 if req_wpqr.currentText()=='Yes' else 0

    cur = conn.cursor()
    if job and job.get('id'):
        cur.execute('UPDATE customer_projects SET customer_id=?, job_number=?, client_name=?, amount=?, quote_number=?, description=?, order_number=?, invoice_number=?, invoiced=?, req_wps=?, req_pqr=?, req_wpqr=? WHERE id=?', (
            customer_id, jnum, client.text().strip(), amount.text().strip(), quote.text().strip(), desc.text().strip(), ordern.text().strip(), inv.text().strip(), invoiced_val, req_wps_val, req_pqr_val, req_wpqr_val, job['id']
        ))
        conn.commit()
        return True
    else:
        cur.execute('INSERT INTO customer_projects (customer_id, job_number, client_name, amount, quote_number, description, order_number, invoice_number, invoiced, req_wps, req_pqr, req_wpqr, created_at) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,datetime("now"))', (
            customer_id, jnum, client.text().strip(), amount.text().strip(), quote.text().strip(), desc.text().strip(), ordern.text().strip(), inv.text().strip(), invoiced_val, req_wps_val, req_pqr_val, req_wpqr_val
        ))
        conn.commit()
        return True
