"""machines.py
Machines view with add option.
"""
from PySide6.QtWidgets import QWidget, QVBoxLayout, QLabel, QComboBox, QPushButton, QInputDialog, QMessageBox
import sqlite3

class MachinesWidget(QWidget):
    def __init__(self, conn: sqlite3.Connection):
        super().__init__(); self.conn = conn; self.layout=QVBoxLayout(self); self.layout.addWidget(QLabel('Machines'))
        self.combo = QComboBox(); self.layout.addWidget(self.combo)
        self.btn_add = QPushButton('Add Machine'); self.btn_add.clicked.connect(self._add)
        self.layout.addWidget(self.btn_add)
        self.refresh()

    def refresh(self):
        cur = self.conn.cursor(); cur.execute('SELECT name FROM machines'); rows = [r[0] for r in cur.fetchall()]
        self.combo.clear(); self.combo.addItem('Select machine'); self.combo.addItems(rows)

    def _add(self):
        text, ok = QInputDialog.getText(self, 'Add Machine', 'Machine name:')
        if ok and text.strip(): cur=self.conn.cursor(); cur.execute('INSERT INTO machines (name) VALUES (?)', (text.strip(),)); self.conn.commit(); self.refresh(); QMessageBox.information(self,'Add','Added')
