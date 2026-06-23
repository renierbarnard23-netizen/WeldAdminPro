"""stock.py
Stock control simple view with add option.
"""
from PySide6.QtWidgets import QWidget, QVBoxLayout, QLabel, QComboBox, QPushButton, QInputDialog, QMessageBox
import sqlite3

class StockWidget(QWidget):
    def __init__(self, conn: sqlite3.Connection):
        super().__init__(); self.conn = conn; self.layout=QVBoxLayout(self); self.layout.addWidget(QLabel('Stock Control'))
        self.combo = QComboBox(); self.layout.addWidget(self.combo)
        self.btn_add = QPushButton('Add Item'); self.btn_add.clicked.connect(self._add)
        self.layout.addWidget(self.btn_add)
        self.refresh()

    def refresh(self):
        cur = self.conn.cursor(); cur.execute('SELECT name FROM stock_items'); rows = [r[0] for r in cur.fetchall()]
        self.combo.clear(); self.combo.addItem('Select item'); self.combo.addItems(rows)

    def _add(self):
        text, ok = QInputDialog.getText(self, 'Add Stock Item', 'Item name:')
        if ok and text.strip(): cur=self.conn.cursor(); cur.execute('INSERT INTO stock_items (name,qty) VALUES (?,0)', (text.strip(),)); self.conn.commit(); self.refresh(); QMessageBox.information(self,'Add','Added')
