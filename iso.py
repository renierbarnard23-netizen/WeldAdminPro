"""iso.py
Simple ISO documents widget.
"""
from PySide6.QtWidgets import QWidget, QVBoxLayout, QLabel, QPushButton, QTableWidget, QTableWidgetItem, QHBoxLayout, QLineEdit, QComboBox, QMessageBox
import sqlite3

class ISOWidget(QWidget):
    def __init__(self, conn: sqlite3.Connection):
        super().__init__(); self.conn = conn; self.layout = QVBoxLayout(self); self.layout.addWidget(QLabel('ISO Documents'))
        self.iso_select = QComboBox(); self.iso_select.addItems(['ISO 9001','ISO 3834','ISO 14001']); self.iso_select.currentTextChanged.connect(self.refresh)
        self.layout.addWidget(self.iso_select)
        self.table = QTableWidget(0,4); self.table.setHorizontalHeaderLabels(['Name','Revision','Date','Status']); self.layout.addWidget(self.table)
        ctrl = QHBoxLayout(); self.name = QLineEdit(); self.rev=QLineEdit(); self.date=QLineEdit(); self.status=QLineEdit(); add=QPushButton('Add')
        add.clicked.connect(self._add); ctrl.addWidget(self.name); ctrl.addWidget(self.rev); ctrl.addWidget(self.date); ctrl.addWidget(self.status); ctrl.addWidget(add)
        self.layout.addLayout(ctrl)
        self.refresh()

    def refresh(self):
        iso = self.iso_select.currentText()
        cur = self.conn.cursor(); cur.execute('SELECT id,name,revision,doc_date,status FROM iso_documents WHERE iso_type=?', (iso,)); rows = cur.fetchall(); self.table.setRowCount(len(rows))
        for i,r in enumerate(rows): self.table.setItem(i,0,QTableWidgetItem(r[1] or '')); self.table.setItem(i,1,QTableWidgetItem(r[2] or '')); self.table.setItem(i,2,QTableWidgetItem(r[3] or '')); self.table.setItem(i,3,QTableWidgetItem(r[4] or ''))

    def _add(self):
        iso = self.iso_select.currentText(); n=self.name.text().strip(); rv=self.rev.text().strip(); d=self.date.text().strip(); s=self.status.text().strip()
        if not n: QMessageBox.warning(self,'Add','Name required'); return
        cur = self.conn.cursor(); cur.execute('INSERT INTO iso_documents (iso_type,name,revision,doc_date,status) VALUES (?,?,?,?,?)', (iso,n,rv,d,s)); self.conn.commit(); self.name.clear(); self.rev.clear(); self.date.clear(); self.status.clear(); self.refresh()
