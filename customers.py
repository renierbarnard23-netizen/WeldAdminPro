"""customers.py
Customer UI widget and controllers.
"""
from typing import List, Dict
from PySide6.QtWidgets import QWidget, QVBoxLayout, QLabel, QComboBox, QPushButton, QTableWidget, QTableWidgetItem, QDialog, QFormLayout, QLineEdit, QDialogButtonBox, QMessageBox, QHBoxLayout
from PySide6.QtCore import Qt
import database


class CustomersWidget(QWidget):
    def __init__(self, conn):
        super().__init__()
        self.conn = conn
        self.layout = QVBoxLayout(self)
        self.header = QLabel('Customers')
        self.layout.addWidget(self.header)

        self.combo = QComboBox(); self.combo.addItems(['Select option','Existing Customers','New Customer'])
        self.layout.addWidget(self.combo)
        self.combo.currentTextChanged.connect(self._on_combo)

        # action buttons
        btn_row = QHBoxLayout()
        self.btn_view = QPushButton('View Details'); self.btn_edit = QPushButton('Edit Selected'); self.btn_delete = QPushButton('Delete Selected')
        for b in (self.btn_view,self.btn_edit,self.btn_delete): b.setEnabled(False); btn_row.addWidget(b)
        self.layout.addLayout(btn_row)

        self.table = QTableWidget(0,6)
        self.table.setHorizontalHeaderLabels(['Customer','Address','Contact','Phone','Reg','VAT'])
        self.table.itemSelectionChanged.connect(self._selection_changed)
        self.layout.addWidget(self.table)

        self._refresh()

    def _on_combo(self, text):
        if text == 'Existing Customers':
            self._refresh()
        elif text == 'New Customer':
            self._new_customer_dialog()

    def _refresh(self):
        cur = self.conn.cursor()
        cur.execute('SELECT id, name, address, contact, phone, reg, vat FROM customers')
        rows = cur.fetchall()
        self.table.setRowCount(len(rows))
        self.customers = []
        for i,r in enumerate(rows):
            self.customers.append({'id':r[0],'name':r[1],'address':r[2],'contact':r[3],'phone':r[4],'reg':r[5],'vat':r[6]})
            for j,val in enumerate(r[1:7]):
                self.table.setItem(i,j,QTableWidgetItem(val or ''))

    def _selection_changed(self):
        has = self.table.currentRow() >= 0
        self.btn_view.setEnabled(has); self.btn_edit.setEnabled(has); self.btn_delete.setEnabled(has)

    def _new_customer_dialog(self):
        dlg = QDialog(self); dlg.setWindowTitle('New Customer'); form = QFormLayout(dlg)
        name = QLineEdit(); address=QLineEdit(); contact=QLineEdit(); phone=QLineEdit(); reg=QLineEdit(); vat=QLineEdit()
        form.addRow('Customer name:', name); form.addRow('Address:', address); form.addRow('Contact person:', contact); form.addRow('Telephone:', phone); form.addRow('Reg #:', reg); form.addRow('VAT #:', vat)
        btns = QDialogButtonBox(QDialogButtonBox.Save | QDialogButtonBox.Cancel, parent=dlg); form.addWidget(btns)
        btns.accepted.connect(dlg.accept); btns.rejected.connect(dlg.reject)
        if dlg.exec() == QDialog.Accepted:
            cur = self.conn.cursor(); cur.execute('INSERT INTO customers (name,address,contact,phone,reg,vat,created_at) VALUES (?,?,?,?,?,?,datetime("now"))', (name.text().strip(), address.text().strip(), contact.text().strip(), phone.text().strip(), reg.text().strip(), vat.text().strip())); self.conn.commit(); self._refresh()
