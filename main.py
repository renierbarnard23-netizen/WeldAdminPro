
# ===== main.py =====
# Save this as main.py

"""
main.py - Patched version
Ensures migrations run early and fixes logo path check.
"""
import sys
import database
from pathlib import Path

# --- Run DB migrations early ---
database.ensure_migrations()
database.seed_customers()

# Try importing PySide6
try:
    from PySide6.QtWidgets import QApplication, QWidget, QHBoxLayout, QVBoxLayout, QLabel, QFrame, QPushButton
    from PySide6.QtGui import QPixmap
    from PySide6.QtCore import Qt
    PYSIDE = True
except Exception:
    PYSIDE = False

if PYSIDE:
    import customers, jobs, welding_docs, iso, stock, machines

    class MainWindow(QWidget):
        def __init__(self):
            super().__init__()
            self.setWindowTitle('WeldAdmin Pro')
            self.resize(1100, 760)
            self.conn = database.get_conn()

            self.setStyleSheet('background-color: black; color: orange; font-family: Segoe UI, Arial;')
            layout = QHBoxLayout(self)

            # --- Sidebar ---
            sidebar = QFrame()
            sl = QVBoxLayout(sidebar)
            sl.setContentsMargins(8, 8, 8, 8)

            title = QLabel('WeldAdmin Pro')
            title.setStyleSheet('font-size:18px; font-weight:bold; color:orange;')
            title.setAlignment(Qt.AlignCenter)
            sl.addWidget(title)

            self.btn_customer = QPushButton('Customer Info')
            self.btn_iso = QPushButton('ISO')
            self.btn_weld = QPushButton('Welding Documents')
            self.btn_stock = QPushButton('Stock Control')
            self.btn_machines = QPushButton('Machines')

            for b in (self.btn_customer, self.btn_iso, self.btn_weld, self.btn_stock, self.btn_machines):
                b.setStyleSheet('background:#111;color:orange;')
                b.setFixedHeight(36)
                sl.addWidget(b)

            sl.addStretch()

            # --- Main content area ---
            self.content = QFrame()
            self.c_layout = QVBoxLayout(self.content)

            self.logo = QLabel()
            self.logo.setAlignment(Qt.AlignCenter)

            if Path('Tetracube_Logo.jpg').exists():
                try:
                    pix = QPixmap('Tetracube_Logo.jpg').scaled(300, 300, Qt.KeepAspectRatio, Qt.SmoothTransformation)
                    self.logo.setPixmap(pix)
                except Exception:
                    self.logo.setText('Tetracube Logo')
            else:
                self.logo.setText('Tetracube Logo')

            self.c_layout.addWidget(self.logo)

            self.header = QLabel('Select a section')
            self.header.setAlignment(Qt.AlignCenter)
            self.header.setStyleSheet('font-size:20px; font-weight:bold;')
            self.c_layout.addWidget(self.header)

            # Instantiate widgets (each gets its own DB connection)
            self.customers_widget = customers.CustomersWidget(database.get_conn())
            self.welding_widget = welding_docs.WeldingDocsWidget(database.get_conn())
            self.iso_widget = iso.ISOWidget(database.get_conn())
            self.stock_widget = stock.StockWidget(database.get_conn())
            self.machines_widget = machines.MachinesWidget(database.get_conn())

            # Wire buttons
            self.btn_customer.clicked.connect(self.show_customers)
            self.btn_weld.clicked.connect(self.show_welding)
            self.btn_iso.clicked.connect(self.show_iso)
            self.btn_stock.clicked.connect(self.show_stock)
            self.btn_machines.clicked.connect(self.show_machines)

            layout.addWidget(sidebar)
            layout.addWidget(self.content, 1)
            self.show_home()

        def _clear_content(self):
            for w in self.content.findChildren(QWidget):
                try:
                    w.hide()
                    self.c_layout.removeWidget(w)
                except Exception:
                    pass

        def show_home(self):
            self._clear_content()
            self.logo.show()
            self.header.setText('Welcome to WeldAdmin Pro')

        def show_customers(self):
            self._clear_content()
            self.c_layout.addWidget(self.customers_widget)
            self.customers_widget.show()
            self.header.setText('Customer Info')

        def show_welding(self):
            self._clear_content()
            self.c_layout.addWidget(self.welding_widget)
            self.welding_widget.show()
            self.header.setText('Welding Documents')

        def show_iso(self):
            self._clear_content()
            self.c_layout.addWidget(self.iso_widget)
            self.iso_widget.show()
            self.header.setText('ISO Documents')

        def show_stock(self):
            self._clear_content()
            self.c_layout.addWidget(self.stock_widget)
            self.stock_widget.show()
            self.header.setText('Stock Control')

        def show_machines(self):
            self._clear_content()
            self.c_layout.addWidget(self.machines_widget)
            self.machines_widget.show()
            self.header.setText('Machines')

    def main():
        app = QApplication(sys.argv)
        w = MainWindow()
        w.show()
        sys.exit(app.exec())

else:
    def main():
        print("PySide6 is not installed. Install with: python -m pip install PySide6")

if __name__ == '__main__':
    main()

