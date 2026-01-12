"""welding_docs.py
List and linking helpers for welding documents.
"""
from PySide6.QtWidgets import QWidget, QVBoxLayout, QLabel, QTableWidget, QTableWidgetItem, QHBoxLayout, QComboBox, QLineEdit, QPushButton, QFileDialog, QMessageBox
import sqlite3

class WeldingDocsWidget(QWidget):
    def __init__(self, conn: sqlite3.Connection):
        super().__init__()
        self.conn = conn
        self.layout = QVBoxLayout(self)
        self.layout.addWidget(QLabel('Welding Documents'))
        self.table = QTableWidget(0,3)
        self.table.setHorizontalHeaderLabels(['Type','Name','File'])
        self.layout.addWidget(self.table)

        ctrl = QHBoxLayout()
        self.type_cb = QComboBox(); self.type_cb.addItems(['WPS','PQR','WPQR'])
        self.name_edit = QLineEdit(); self.path_edit = QLineEdit(); self.browse = QPushButton('Browse'); self.add = QPushButton('Add')
        self.browse.clicked.connect(self._browse); self.add.clicked.connect(self._add)
        ctrl.addWidget(self.type_cb); ctrl.addWidget(self.name_edit); ctrl.addWidget(self.path_edit); ctrl.addWidget(self.browse); ctrl.addWidget(self.add)
        self.layout.addLayout(ctrl)
        self.refresh()

    def _browse(self):
        f,_ = QFileDialog.getOpenFileName(self, 'Select file', '', 'PDF Files (*.pdf);;All Files (*)')
        if f: self.path_edit.setText(f)

    def _add(self):
        name = self.name_edit.text().strip(); dtype=self.type_cb.currentText(); path=self.path_edit.text().strip()
        if not name: QMessageBox.warning(self, 'Add', 'Name required'); return
        cur = self.conn.cursor(); cur.execute('INSERT INTO welding_documents (doc_type,name,file_path) VALUES (?,?,?)', (dtype,name,path)); self.conn.commit(); QMessageBox.information(self, 'Add', 'Added'); self.name_edit.clear(); self.path_edit.clear(); self.refresh()

    def refresh(self):
        cur = self.conn.cursor(); cur.execute('SELECT id, doc_type, name, file_path FROM welding_documents'); rows = cur.fetchall(); self.table.setRowCount(len(rows))
        for i,r in enumerate(rows): self.table.setItem(i,0,QTableWidgetItem(r[1] or '')); self.table.setItem(i,1,QTableWidgetItem(r[2] or '')); self.table.setItem(i,2,QTableWidgetItem(r[3] or ''))
