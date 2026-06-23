"""WeldAdmin Pro — Landing Page

This module defines the initial landing page for WeldAdmin Pro.
It is designed to run with PySide6 on your local machine.

If PySide6 is **not** available, the module:
- Prints a clear warning message to stderr, and
- Returns cleanly from ``main()`` **without** raising SystemExit,
  so that it does not crash test/sandbox environments.

Current behaviour
-----------------
- Overall theme:
  * Background: black
  * Text: orange
- Left sidebar with navigation sections and dropdowns (combo boxes):
  * Customer Info
      - Existing Customers (shows a table of customers in the centre)
      - New Customer (opens a form dialog; saving adds to Existing Customers)
  * ISO
      - ISO 9001
      - ISO 3834
      - ISO 14001
  * Welding Documents
      - WPS
      - PQR
      - WPQR
  * Stock Control
      - New Stock  (with Add button to add more items to the dropdown)
      - Current Stock  (with Add button to add more items to the dropdown)
  * Machines
      - Welding machines
      - Small grinder
      - Big grinder
      - Drill
      - Magnetic drill
      - Add button to add more machine types to the dropdown
- Centre area:
  * Black background with centred Tetracube logo by default
  * When Customer Info → Existing Customers is chosen:
      - Logo is hidden
      - A table with customer details is shown
      - Buttons above the table allow **Edit Selected** and **Delete Selected**
        (Option C as requested).

Notes for usage
---------------
- To run the GUI, make sure PySide6 is installed in your environment:
    pip install PySide6
- Ensure that the Tetracube logo image is present in the same directory
  as this script, with the exact filename: "Tetracube_Logo.jpg".
- Customers are persisted in the SQLite database ``weldadmin.db`` in a
  table called ``customers``.
- ISO documents are persisted in the ``iso_documents`` table in the same
  database.
- When PySide6 is not installed, running this file will only print an
  informative error message and **return**, instead of exiting the process
  with SystemExit(1).

Simple sanity tests are included at the bottom of the file; they can be
run by setting the environment variable RUN_WELDADMIN_TESTS=1.
"""

from __future__ import annotations

import os
import sys
import sqlite3
from typing import Optional, List, Dict

# ---------------------------------------------------------------------------
# Optional PySide6 import with graceful fallback
# ---------------------------------------------------------------------------

PYSIDE_AVAILABLE = True
try:
    from PySide6.QtWidgets import (
        QApplication,
        QWidget,
        QLabel,
        QVBoxLayout,
        QHBoxLayout,
        QFrame,
        QPushButton,
        QComboBox,
        QInputDialog,
        QDialog,
        QFormLayout,
        QLineEdit,
        QDialogButtonBox,
        QTableWidget,
        QTableWidgetItem,
        QMessageBox,
    )
    from PySide6.QtGui import QPixmap
    from PySide6.QtCore import Qt
except ModuleNotFoundError:
    PYSIDE_AVAILABLE = False
    # When PySide6 is missing we do not define the GUI classes.


# ---------------------------------------------------------------------------
# Helper functions
# ---------------------------------------------------------------------------

def is_pyside_available() -> bool:
    """Return True if PySide6 imported successfully, else False."""

    return PYSIDE_AVAILABLE


def get_logo_path(preferred_name: str = "Tetracube_Logo.jpg") -> Optional[str]:
    """Return the absolute path to the Tetracube logo if it exists.

    Parameters
    ----------
    preferred_name:
        The filename we expect the logo to have. By default this is
        "Tetracube_Logo.jpg" in the current working directory.

    Returns
    -------
    Optional[str]
        Absolute path to the logo file if it exists, otherwise ``None``.
    """

    candidate = os.path.abspath(preferred_name)
    return candidate if os.path.exists(candidate) else None


# ---------------------------------------------------------------------------
# GUI definition (only when PySide6 is available)
# ---------------------------------------------------------------------------

if PYSIDE_AVAILABLE:

    class LandingPage(QWidget):
        """Main landing page for WeldAdmin Pro.

        Layout:
        - Left sidebar: sections with dropdowns for
          Customer Info, ISO, Welding Documents, Stock Control, Machines.
        - Centre area: black background with the Tetracube logo and
          a dynamic area that shows either text, the customer table,
          or the ISO documents table.
        """

        def __init__(self) -> None:
            super().__init__()
            self.setWindowTitle("WeldAdmin Pro — Landing Page")
            self.setMinimumSize(1100, 700)

            # Database connection (for customers and ISO)
            self.db_path = "weldadmin.db"
            self.conn = sqlite3.connect(self.db_path)

            # Customers
            self._init_customer_table()
            self.customers: List[Dict[str, str]] = []
            self._reload_customers_from_db()

            # Customer-related projects / jobs and files
            self._init_project_table()
            self._init_customer_files_table()

            # Customer-related projects / jobs and files
            self._init_project_table()
            self._init_customer_files_table()

            # ISO documents
            self._init_iso_table()
            self.iso_documents: List[Dict[str, str]] = []
            self.current_iso_docs: List[Dict[str, str]] = []
            self._reload_iso_from_db()

            # Set a global dark theme for the window
            self.setStyleSheet(
                "background-color: black; color: orange; font-family: Segoe UI, Arial;"
            )

            # === Main layout ===
            main_layout = QHBoxLayout(self)

            # --- Left Sidebar ---
            sidebar = QFrame()
            sidebar.setStyleSheet(
                "background-color: #000000; color: orange; border-right: 1px solid #444;"
            )
            sidebar.setFixedWidth(280)

            sidebar_layout = QVBoxLayout(sidebar)
            sidebar_layout.setContentsMargins(15, 20, 15, 20)
            sidebar_layout.setSpacing(18)

            # Helper to style labels, combos and buttons
            def make_section_label(text: str) -> QLabel:
                lbl = QLabel(text)
                lbl.setStyleSheet("font-size: 16px; font-weight: bold;")
                return lbl

            def style_combo(combo: QComboBox) -> None:
                combo.setStyleSheet(
                    "QComboBox {"
                    "  background-color: #111;"
                    "  color: orange;"
                    "  padding: 4px;"
                    "  border: 1px solid #444;"
                    "}"
                    "QComboBox QAbstractItemView {"
                    "  background-color: #111;"
                    "  color: orange;"
                    "  selection-background-color: #222;"
                    "}"
                )

            def style_button(btn: QPushButton) -> None:
                btn.setStyleSheet(
                    "QPushButton {"
                    "  background-color: #111;"
                    "  color: orange;"
                    "  padding: 6px 10px;"
                    "  border: 1px solid #444;"
                    "  border-radius: 3px;"
                    "}"
                    "QPushButton:hover {"
                    "  background-color: #222;"
                    "}"
                )

            # ---- Customer Info section ----
            lbl_customer = make_section_label("Customer Info")
            self.combo_customer = QComboBox()
            self.combo_customer.addItems(
                [
                    "Select option",
                    "Existing Customers",
                    "New Customer",
                ]
            )
            style_combo(self.combo_customer)
            self.combo_customer.currentTextChanged.connect(
                lambda text: self._handle_customer_selection(text)
            )

            sidebar_layout.addWidget(lbl_customer)
            sidebar_layout.addWidget(self.combo_customer)

            # ---- ISO section ----
            lbl_iso = make_section_label("ISO")
            self.combo_iso = QComboBox()
            self.combo_iso.addItems(
                [
                    "Select ISO",
                    "ISO 9001",
                    "ISO 3834",
                    "ISO 14001",
                ]
            )
            style_combo(self.combo_iso)
            self.combo_iso.currentTextChanged.connect(self._handle_iso_selection)

            sidebar_layout.addWidget(lbl_iso)
            sidebar_layout.addWidget(self.combo_iso)

            # ---- Welding Documents section ----
            lbl_weld = make_section_label("Welding Documents")
            self.combo_weld = QComboBox()
            self.combo_weld.addItems(["Select document", "WPS", "PQR", "WPQR"])
            style_combo(self.combo_weld)
            self.combo_weld.currentTextChanged.connect(
                lambda text: self._on_selection("Welding Documents", text)
            )

            sidebar_layout.addWidget(lbl_weld)
            sidebar_layout.addWidget(self.combo_weld)

            # ---- Stock Control section ----
            lbl_stock = make_section_label("Stock Control")
            self.combo_new_stock = QComboBox()
            self.combo_new_stock.addItem("New Stock")
            style_combo(self.combo_new_stock)

            btn_add_new_stock = QPushButton("Add New Stock Item")
            style_button(btn_add_new_stock)
            btn_add_new_stock.clicked.connect(
                lambda: self._add_item_to_combo("New Stock Item", self.combo_new_stock)
            )

            self.combo_current_stock = QComboBox()
            self.combo_current_stock.addItem("Current Stock")
            style_combo(self.combo_current_stock)

            btn_add_current_stock = QPushButton("Add Current Stock Item")
            style_button(btn_add_current_stock)
            btn_add_current_stock.clicked.connect(
                lambda: self._add_item_to_combo(
                    "Current Stock Item", self.combo_current_stock
                )
            )

            self.combo_new_stock.currentTextChanged.connect(
                lambda text: self._on_selection("Stock Control - New Stock", text)
            )
            self.combo_current_stock.currentTextChanged.connect(
                lambda text: self._on_selection("Stock Control - Current Stock", text)
            )

            sidebar_layout.addWidget(lbl_stock)
            sidebar_layout.addWidget(self.combo_new_stock)
            sidebar_layout.addWidget(btn_add_new_stock)
            sidebar_layout.addWidget(self.combo_current_stock)
            sidebar_layout.addWidget(btn_add_current_stock)

            # ---- Machines section ----
            lbl_machines = make_section_label("Machines")
            self.combo_machines = QComboBox()
            self.combo_machines.addItems(
                [
                    "Select machine",
                    "Welding machines",
                    "Small grinder",
                    "Big grinder",
                    "Drill",
                    "Magnetic drill",
                ]
            )
            style_combo(self.combo_machines)

            btn_add_machine = QPushButton("Add Machine Type")
            style_button(btn_add_machine)
            btn_add_machine.clicked.connect(
                lambda: self._add_item_to_combo("Machine Type", self.combo_machines)
            )

            self.combo_machines.currentTextChanged.connect(
                lambda text: self._on_selection("Machines", text)
            )

            sidebar_layout.addWidget(lbl_machines)
            sidebar_layout.addWidget(self.combo_machines)
            sidebar_layout.addWidget(btn_add_machine)

            sidebar_layout.addStretch()

            # --- Main Centre Area ---
            center = QFrame()
            center.setStyleSheet("background-color: black; color: orange;")
            center_layout = QVBoxLayout(center)
            center_layout.setContentsMargins(40, 40, 40, 40)
            center_layout.setSpacing(20)

            # Tetracube Logo
            self.logo_label = QLabel()
            self.logo_label.setAlignment(Qt.AlignCenter)

            logo_path = get_logo_path("Tetracube_Logo.jpg")
            if logo_path is not None:
                pix = QPixmap(logo_path).scaled(
                    400,
                    400,
                    Qt.KeepAspectRatio,
                    Qt.SmoothTransformation,
                )
                self.logo_label.setPixmap(pix)
            else:
                self.logo_label.setText("Tetracube Logo Here")
                self.logo_label.setStyleSheet("font-size: 28px; color: orange;")

            # Dynamic content labels
            self.content_title = QLabel("Welcome to WeldAdmin Pro")
            self.content_title.setAlignment(Qt.AlignCenter)
            self.content_title.setStyleSheet("font-size: 22px; font-weight: bold;")

            self.content_body = QLabel(
                "Select an option from the dropdowns on the left to "
                "view the related information here."
            )
            self.content_body.setAlignment(Qt.AlignCenter)
            self.content_body.setWordWrap(True)
            self.content_body.setStyleSheet("font-size: 14px;")

            center_layout.addStretch()
            center_layout.addWidget(self.logo_label)
            center_layout.addSpacing(10)
            center_layout.addWidget(self.content_title)
            center_layout.addWidget(self.content_body)

            # Customer action buttons (View / Edit / Delete), initially hidden
            self.customer_actions_frame = QFrame()
            actions_layout = QHBoxLayout(self.customer_actions_frame)
            actions_layout.setContentsMargins(0, 0, 0, 0)
            actions_layout.setSpacing(10)

            self.btn_view_customer = QPushButton("View Details")
            self.btn_edit_customer = QPushButton("Edit Selected")
            self.btn_delete_customer = QPushButton("Delete Selected")
            style_button(self.btn_view_customer)
            style_button(self.btn_edit_customer)
            style_button(self.btn_delete_customer)

            self.btn_view_customer.clicked.connect(self._view_selected_customer)
            self.btn_edit_customer.clicked.connect(self._edit_selected_customer)
            self.btn_delete_customer.clicked.connect(self._delete_selected_customer)

            actions_layout.addWidget(self.btn_view_customer)
            actions_layout.addWidget(self.btn_edit_customer)
            actions_layout.addWidget(self.btn_delete_customer)
            self.customer_actions_frame.hide()
            center_layout.addWidget(self.customer_actions_frame)

            # ISO action buttons (Add / Edit / Delete), initially hidden
            self.iso_actions_frame = QFrame()
            iso_actions_layout = QHBoxLayout(self.iso_actions_frame)
            iso_actions_layout.setContentsMargins(0, 0, 0, 0)
            iso_actions_layout.setSpacing(10)

            self.btn_iso_add = QPushButton("Add ISO Document")
            self.btn_iso_edit = QPushButton("Edit Selected ISO")
            self.btn_iso_delete = QPushButton("Delete Selected ISO")
            style_button(self.btn_iso_add)
            style_button(self.btn_iso_edit)
            style_button(self.btn_iso_delete)

            self.btn_iso_add.clicked.connect(self._add_iso_document)
            self.btn_iso_edit.clicked.connect(self._edit_selected_iso)
            self.btn_iso_delete.clicked.connect(self._delete_selected_iso)

            iso_actions_layout.addWidget(self.btn_iso_add)
            iso_actions_layout.addWidget(self.btn_iso_edit)
            iso_actions_layout.addWidget(self.btn_iso_delete)
            self.iso_actions_frame.hide()
            center_layout.addWidget(self.iso_actions_frame)

            # Table for displaying existing customers (initially hidden)
            self.customer_table = QTableWidget(0, 6)
            self.customer_table.setHorizontalHeaderLabels(
                [
                    "Customer Name",
                    "Address",
                    "Contact Person",
                    "Telephone",
                    "Registration No.",
                    "VAT No.",
                ]
            )
            self.customer_table.setStyleSheet(
                "QTableWidget {"
                "  background-color: #000000;"
                "  color: orange;"
                "  gridline-color: #444444;"
                "}"
                "QHeaderView::section {"
                "  background-color: #111111;"
                "  color: orange;"
                "}"
            )
            # Double-click on a row opens the detailed customer profile dialog
            self.customer_table.itemDoubleClicked.connect(
                lambda *_: self._view_selected_customer()
            )
            # When selection changes and the jobs view is visible, refresh jobs
            self.customer_table.itemSelectionChanged.connect(
                lambda: self._refresh_jobs_table_for_selected_customer()
                if hasattr(self, "jobs_table") and self.jobs_table.isVisible()
                else None
            )
            self.customer_table.hide()
            center_layout.addWidget(self.customer_table)

            # Toggle button and table for jobs of the selected customer
            self.jobs_toggle_btn = QPushButton("+ Jobs for selected customer")
            style_button(self.jobs_toggle_btn)
            self.jobs_toggle_btn.setCheckable(True)
            self.jobs_toggle_btn.clicked.connect(self._toggle_jobs_view)
            self.jobs_toggle_btn.hide()  # only shown in Existing Customers view
            center_layout.addWidget(self.jobs_toggle_btn)

            self.jobs_table = QTableWidget(0, 8)
            self.jobs_table.setHorizontalHeaderLabels(
                [
                    "Job Number",
                    "Client / Representative",
                    "Amount",
                    "Quote #",
                    "Description",
                    "Order #",
                    "Invoice #",
                    "Invoiced",
                ]
            )
            self.jobs_table.setStyleSheet(
                "QTableWidget {"
                "  background-color: #000000;"
                "  color: orange;"
                "  gridline-color: #444444;"
                "}"
                "QHeaderView::section {"
                "  background-color: #111111;"
                "  color: orange;"
                "}"
            )
            self.jobs_table.hide()
            center_layout.addWidget(self.jobs_table)

            # Table for displaying ISO documents (initially hidden)
            self.iso_table = QTableWidget(0, 5)
            self.iso_table.setHorizontalHeaderLabels(
                [
                    "ISO Standard",
                    "Document Name",
                    "Revision",
                    "Date",
                    "Status",
                ]
            )
            self.iso_table.setStyleSheet(
                "QTableWidget {"
                "  background-color: #000000;"
                "  color: orange;"
                "  gridline-color: #444444;"
                "}"
                "QHeaderView::section {"
                "  background-color: #111111;"
                "  color: orange;"
                "}"
            )
            self.iso_table.hide()
            center_layout.addWidget(self.iso_table)

            center_layout.addStretch()

            # Add sidebar + centre to main layout
            main_layout.addWidget(sidebar)
            main_layout.addWidget(center)

        # ---------------------- Database helpers ------------------------

        def _init_customer_table(self) -> None:
            """Create the customers table if it does not exist."""

            cur = self.conn.cursor()
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
            self.conn.commit()

        def _reload_customers_from_db(self) -> None:
            """Load all customers from the database into self.customers.

            If no customers are present, seed the table with defaults.

            This method also ensures that all text fields are non-None
            when stored in the in-memory list, simplifying table display.
            """

            cur = self.conn.cursor()
            cur.execute("SELECT id, name, address, contact, phone, reg, vat FROM customers")
            rows = cur.fetchall()

            self.customers = [
                {
                    "id": r[0],
                    "name": r[1] or "",
                    "address": r[2] or "",
                    "contact": r[3] or "",
                    "phone": r[4] or "",
                    "reg": r[5] or "",
                    "vat": r[6] or "",
                }
                for r in rows
            ]

            if not self.customers:
                defaults = [
                    ("NCP Chlorchem", "", "", "", "", ""),
                    ("AECI Mining", "", "", "", "", ""),
                    ("Mixtec", "", "", "", "", ""),
                    ("Lodex", "", "", "", "", ""),
                    ("ASK Chemicals", "", "", "", "", ""),
                ]
                cur.executemany(
                    "INSERT INTO customers (name, address, contact, phone, reg, vat) "
                    "VALUES (?, ?, ?, ?, ?, ?)",
                    defaults,
                )
                self.conn.commit()
                cur.execute("SELECT id, name, address, contact, phone, reg, vat FROM customers")
                rows = cur.fetchall()
                self.customers = [
                    {
                        "id": r[0],
                        "name": r[1] or "",
                        "address": r[2] or "",
                        "contact": r[3] or "",
                        "phone": r[4] or "",
                        "reg": r[5] or "",
                        "vat": r[6] or "",
                    }
                    for r in rows
                ]

        def _init_project_table(self) -> None:
            """Create or migrate the customer_projects table.

            Earlier versions of the application may have created this
            table with fewer columns. This helper makes sure that all
            required columns exist by adding missing ones with
            sensible defaults.
            """

            cur = self.conn.cursor()
            # Base table (minimal set, if it does not exist yet)
            cur.execute(
                """
                CREATE TABLE IF NOT EXISTS customer_projects (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    customer_id INTEGER,
                    job_number TEXT,
                    description TEXT,
                    status TEXT
                )
                """
            )
            self.conn.commit()

            # Ensure all expected columns exist (simple schema migration)
            required_columns = {
                "customer_id": "INTEGER",
                "job_number": "TEXT",
                "client_name": "TEXT",
                "amount": "TEXT",
                "quote_number": "TEXT",
                "description": "TEXT",
                "order_number": "TEXT",
                "invoice_number": "TEXT",
                "invoiced": "INTEGER",
                "status": "TEXT",
            }

            cur.execute("PRAGMA table_info(customer_projects)")
            existing_info = cur.fetchall()
            existing_cols = {row[1] for row in existing_info}

            for col_name, col_type in required_columns.items():
                if col_name not in existing_cols:
                    # Add missing column with a safe default
                    if col_name == "invoiced":
                        default_clause = "DEFAULT 0"
                    else:
                        default_clause = "DEFAULT ''"
                    cur.execute(
                        f"ALTER TABLE customer_projects ADD COLUMN {col_name} {col_type} {default_clause}"
                    )

            self.conn.commit()

        def _init_customer_files_table(self) -> None:
            """Create the customer_files table if it does not exist."""

            cur = self.conn.cursor()
            cur.execute(
                """
                CREATE TABLE IF NOT EXISTS customer_files (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    customer_id INTEGER,
                    file_path TEXT,
                    description TEXT
                )
                """
            )
            self.conn.commit()

        def _init_project_table(self) -> None:
            """Create the customer_projects table if it does not exist."""

            cur = self.conn.cursor()
            cur.execute(
                """
                CREATE TABLE IF NOT EXISTS customer_projects (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    customer_id INTEGER,
                    job_number TEXT,
                    description TEXT,
                    status TEXT
                )
                """
            )
            self.conn.commit()

        def _init_customer_files_table(self) -> None:
            """Create the customer_files table if it does not exist."""

            cur = self.conn.cursor()
            cur.execute(
                """
                CREATE TABLE IF NOT EXISTS customer_files (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    customer_id INTEGER,
                    file_path TEXT,
                    description TEXT
                )
                """
            )
            self.conn.commit()

        # ---------------------- ISO database helpers --------------------

        def _init_iso_table(self) -> None:
            """Create the iso_documents table if it does not exist."""

            cur = self.conn.cursor()
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
            self.conn.commit()

        def _reload_iso_from_db(self) -> None:
            """Load all ISO documents from the database into self.iso_documents.

            If no records are present, seed with one example per ISO type.
            """

            cur = self.conn.cursor()
            cur.execute(
                "SELECT id, iso_type, name, revision, doc_date, status FROM iso_documents"
            )
            rows = cur.fetchall()

            self.iso_documents = [
                {
                    "id": r[0],
                    "iso_type": r[1] or "",
                    "name": r[2] or "",
                    "revision": r[3] or "",
                    "doc_date": r[4] or "",
                    "status": r[5] or "",
                }
                for r in rows
            ]

            if not self.iso_documents:
                defaults = [
                    ("ISO 9001", "Quality Manual", "A", "2024-01-01", "Active"),
                    ("ISO 3834", "Welding Quality Requirements", "A", "2024-01-01", "Active"),
                    ("ISO 14001", "Environmental Manual", "A", "2024-01-01", "Active"),
                ]
                cur.executemany(
                    "INSERT INTO iso_documents (iso_type, name, revision, doc_date, status) "
                    "VALUES (?, ?, ?, ?, ?)",
                    defaults,
                )
                self.conn.commit()

                cur.execute(
                    "SELECT id, iso_type, name, revision, doc_date, status FROM iso_documents"
                )
                rows = cur.fetchall()
                self.iso_documents = [
                    {
                        "id": r[0],
                        "iso_type": r[1] or "",
                        "name": r[2] or "",
                        "revision": r[3] or "",
                        "doc_date": r[4] or "",
                        "status": r[5] or "",
                    }
                    for r in rows
                ]

        # --------------------------- UI helpers -------------------------

        def _handle_customer_selection(self, text: str) -> None:
            """Special handler for the Customer Info dropdown.

            - If "Existing Customers": show list of customers in centre in a table.
            - If "New Customer": open a form dialog and, on save, add to list.
            """

            if text == "Existing Customers":
                self.content_title.setText("Existing Customers")
                self.content_body.setText("Below is the list of existing customers.")
                self._refresh_customer_table()
                # Show the jobs toggle when viewing existing customers
                if hasattr(self, "jobs_toggle_btn"):
                    self.jobs_toggle_btn.show()
                return

            if text == "New Customer":
                self._open_new_customer_form()
                return

            # For other options in this combo, hide jobs view and fall back
            if hasattr(self, "jobs_table"):
                self.jobs_table.hide()
            if hasattr(self, "jobs_toggle_btn"):
                self.jobs_toggle_btn.hide()

            self._on_selection("Customer Info", text)

        def _handle_iso_selection(self, text: str) -> None:
            """Special handler for the ISO dropdown.

            When a specific ISO standard is selected, show related
            ISO documents in the ISO table.
            """

            if not text or text.lower().startswith("select"):
                return

            # Filter documents for this ISO standard; if none match,
            # fall back to showing all documents so the user still
            # sees the table and data.
            docs = [d for d in self.iso_documents if d.get("iso_type") == text]
            if not docs:
                docs = list(self.iso_documents)

            self.content_title.setText(f"ISO: {text}")
            self.content_body.setText(
                f"Showing {len(docs)} document(s) for {text}."
            )
            self._show_iso_table(docs)

        def _open_new_customer_form(self) -> None:
            """Open a dialog form for adding a new customer.

            Fields (all optional):
            - Customer name
            - Address
            - Contact Person
            - Telephone number
            - Registration number
            - Vat number

            On save, if a customer name is provided, the new customer is
            added to the database and the table is refreshed.
            """

            dialog = QDialog(self)
            dialog.setWindowTitle("New Customer")
            form_layout = QFormLayout(dialog)

            name_edit = QLineEdit()
            address_edit = QLineEdit()
            contact_edit = QLineEdit()
            phone_edit = QLineEdit()
            reg_edit = QLineEdit()
            vat_edit = QLineEdit()

            form_layout.addRow("Customer Name:", name_edit)
            form_layout.addRow("Address:", address_edit)
            form_layout.addRow("Contact Person:", contact_edit)
            form_layout.addRow("Telephone Number:", phone_edit)
            form_layout.addRow("Registration Number:", reg_edit)
            form_layout.addRow("VAT Number:", vat_edit)

            buttons = QDialogButtonBox(
                QDialogButtonBox.Save | QDialogButtonBox.Cancel,
                parent=dialog,
            )
            form_layout.addWidget(buttons)

            buttons.accepted.connect(dialog.accept)
            buttons.rejected.connect(dialog.reject)

            if dialog.exec() == QDialog.Accepted:
                name = name_edit.text().strip()
                address = address_edit.text().strip()
                contact = contact_edit.text().strip()
                phone = phone_edit.text().strip()
                reg = reg_edit.text().strip()
                vat = vat_edit.text().strip()

                # All fields are optional, but we only add to list if name exists
                if name:
                    cur = self.conn.cursor()
                    cur.execute(
                        "INSERT INTO customers (name, address, contact, phone, reg, vat) "
                        "VALUES (?, ?, ?, ?, ?, ?)",
                        (name, address, contact, phone, reg, vat),
                    )
                    self.conn.commit()

                    self._reload_customers_from_db()

                    self.content_title.setText("Customer Saved")
                    self.content_body.setText(
                        f"New customer '{name}' added to Existing Customers."
                    )
                    self._refresh_customer_table()
                else:
                    self.content_title.setText("No Name Provided")
                    self.content_body.setText(
                        "Customer details were not saved because no name was given."
                    )

        def _on_selection(self, category: str, text: str) -> None:
            """Update centre content when a sidebar dropdown option is chosen.

            Placeholder behaviour:
            - Ignores placeholder entries like "Select option".
            - For non-customer sections, shows the logo and hides the table.
            """

            if not text or text.lower().startswith("select"):
                return

            # For non-customer sections, show the logo and hide the table
            if category != "Customer Info":
                self._show_logo_mode()

            title = f"{category}: {text}"
            body = (
                f"You selected '{text}' under '{category}'. "
                "This area will later show the related documents "
                "or management screens for that choice."
            )

            self.content_title.setText(title)
            self.content_body.setText(body)

        def _add_item_to_combo(self, label: str, combo: QComboBox) -> None:
            """Prompt the user for a new item and add it to a combo box."""

            text, ok = QInputDialog.getText(
                self,
                f"Add {label}",
                f"Enter {label.lower()} name:",
            )
            if ok and text.strip():
                combo.addItem(text.strip())

        def _refresh_customer_table(self) -> None:
            """Populate the customer table with the current customers list.

            Hides the logo, shows the table and action buttons.
            """

            self.logo_label.setVisible(False)
            self.customer_table.show()
            self.customer_actions_frame.show()
            if hasattr(self, "iso_table"):
                self.iso_table.hide()
            if hasattr(self, "iso_actions_frame"):
                self.iso_actions_frame.hide()

            self.customer_table.setRowCount(len(self.customers))
            keys = ["name", "address", "contact", "phone", "reg", "vat"]
            for row, customer in enumerate(self.customers):
                for col, key in enumerate(keys):
                    value = customer.get(key, "")
                    item = QTableWidgetItem(value)
                    self.customer_table.setItem(row, col, item)

        def _toggle_jobs_view(self) -> None:
            """Show or hide the jobs table for the currently selected customer."""

            if not self.jobs_toggle_btn.isChecked():
                # Hiding the jobs table
                self.jobs_toggle_btn.setText("+ Jobs for selected customer")
                self.jobs_table.hide()
                return

            # Showing the jobs table
            index = self._get_selected_customer_index()
            if index is None:
                QMessageBox.information(
                    self,
                    "Jobs for Customer",
                    "Please select a customer first.",
                )
                # Reset toggle
                self.jobs_toggle_btn.setChecked(False)
                self.jobs_toggle_btn.setText("+ Jobs for selected customer")
                self.jobs_table.hide()
                return

            self.jobs_toggle_btn.setText("- Jobs for selected customer")
            self._refresh_jobs_table_for_selected_customer()
            self.jobs_table.show()

        def _refresh_jobs_table_for_selected_customer(self) -> None:
            """Refresh the jobs table for the currently selected customer.

            This only has an effect when the jobs table is visible.
            """

            if not hasattr(self, "jobs_table") or not self.jobs_table.isVisible():
                return

            index = self._get_selected_customer_index()
            if index is None:
                self.jobs_table.setRowCount(0)
                return

            customer = self.customers[index]
            cur = self.conn.cursor()
            cur.execute(
                """
                SELECT job_number, client_name, amount, quote_number,
                       description, order_number, invoice_number, invoiced
                FROM customer_projects
                WHERE customer_id = ?
                """,
                (customer["id"],),
            )
            rows = cur.fetchall()

            self.jobs_table.setRowCount(len(rows))
            for row_index, r in enumerate(rows):
                for col_index, value in enumerate(r):
                    if col_index == 7:  # invoiced flag
                        display = "Yes" if value else "No"
                    else:
                        display = value or ""
                    item = QTableWidgetItem(display)
                    self.jobs_table.setItem(row_index, col_index, item)

        def _show_iso_table(self, docs: List[Dict[str, str]]) -> None:
            """Show ISO documents in the ISO table and hide logo/customer views.

            If the list is empty, the table is still shown with zero rows,
            so the user can see the headers and know there is no data yet.
            """

            self.logo_label.setVisible(False)
            self.customer_table.hide()
            self.customer_actions_frame.hide()
            self.iso_table.show()
            if hasattr(self, "iso_actions_frame"):
                self.iso_actions_frame.show()

            # Keep track of which ISO documents are currently shown so
            # that edit/delete actions work on the correct records even
            # when the table is filtered.
            self.current_iso_docs = list(docs)

            self.iso_table.setRowCount(len(docs))
            keys = ["iso_type", "name", "revision", "doc_date", "status"]
            for row, doc in enumerate(docs):
                for col, key in enumerate(keys):
                    value = doc.get(key, "")
                    item = QTableWidgetItem(value)
                    self.iso_table.setItem(row, col, item)

        def _show_logo_mode(self) -> None:
            """Show the logo and hide the customer/ISO tables (used for other sections)."""

            self.customer_table.hide()
            self.customer_actions_frame.hide()
            if hasattr(self, "iso_table"):
                self.iso_table.hide()
            if hasattr(self, "iso_actions_frame"):
                self.iso_actions_frame.hide()
            self.logo_label.setVisible(True)

        def _get_selected_customer_index(self) -> Optional[int]:
            """Return the index of the currently selected customer row, or None."""

            row = self.customer_table.currentRow()
            if row < 0 or row >= len(self.customers):
                return None
            return row

        def _view_selected_customer(self) -> None:
            """Show a detailed profile view for the selected customer.

            This opens a dialog with all stored customer information in
            a clean form layout and provides buttons to view the
            customer's Projects/Jobs and Files/Documents.
            """

            index = self._get_selected_customer_index()
            if index is None:
                QMessageBox.information(
                    self,
                    "View Customer",
                    "Please select a customer row to view.",
                )
                return

            customer = self.customers[index]

            dialog = QDialog(self)
            dialog.setWindowTitle("Customer Profile")
            form_layout = QFormLayout(dialog)

            def make_label(value: str) -> QLabel:
                lbl = QLabel(value or "-")
                lbl.setStyleSheet("color: orange;")
                return lbl

            form_layout.addRow("Customer Name:", make_label(customer.get("name", "")))
            form_layout.addRow("Address:", make_label(customer.get("address", "")))
            form_layout.addRow("Contact Person:", make_label(customer.get("contact", "")))
            form_layout.addRow("Telephone Number:", make_label(customer.get("phone", "")))
            form_layout.addRow("Registration Number:", make_label(customer.get("reg", "")))
            form_layout.addRow("VAT Number:", make_label(customer.get("vat", "")))

            # Row with buttons to open Projects/Jobs and Files views
            buttons_row_widget = QFrame(dialog)
            buttons_row_layout = QHBoxLayout(buttons_row_widget)
            buttons_row_layout.setContentsMargins(0, 0, 0, 0)
            buttons_row_layout.setSpacing(10)

            btn_projects = QPushButton("View Projects / Jobs", parent=buttons_row_widget)
            btn_files = QPushButton("View Files / Documents", parent=buttons_row_widget)
            btn_projects.setStyleSheet(
                "QPushButton { background-color: #111; color: orange; padding: 6px 10px; border: 1px solid #444; }"
                "QPushButton:hover { background-color: #222; }"
            )
            btn_files.setStyleSheet(
                "QPushButton { background-color: #111; color: orange; padding: 6px 10px; border: 1px solid #444; }"
                "QPushButton:hover { background-color: #222; }"
            )

            btn_projects.clicked.connect(lambda: self._show_customer_projects(customer))
            btn_files.clicked.connect(lambda: self._show_customer_files(customer))

            buttons_row_layout.addWidget(btn_projects)
            buttons_row_layout.addWidget(btn_files)

            form_layout.addRow("", buttons_row_widget)

            buttons = QDialogButtonBox(QDialogButtonBox.Close, parent=dialog)
            buttons.rejected.connect(dialog.reject)
            buttons.accepted.connect(dialog.accept)
            form_layout.addWidget(buttons)

            dialog.exec()

        def _show_customer_projects(self, customer: Dict[str, str]) -> None:
            """Show a dialog listing Projects/Jobs for a given customer.

            Provides the ability to add and delete jobs. Each job
            contains job number, client representative, amount, quote
            number, description, order number, invoice number, invoiced
            flag, and an optional status.
            """

            dialog = QDialog(self)
            dialog.setWindowTitle(f"Projects / Jobs for {customer.get('name', '')}")
            layout = QVBoxLayout(dialog)

            table = QTableWidget(0, 9, parent=dialog)
            table.setHorizontalHeaderLabels(
                [
                    "Job Number",
                    "Client / Representative",
                    "Amount",
                    "Quote #",
                    "Description",
                    "Order #",
                    "Invoice #",
                    "Invoiced",
                    "Status",
                ]
            )
            table.setStyleSheet(
                "QTableWidget { background-color: #000000; color: orange; gridline-color: #444444; }"
                "QHeaderView::section { background-color: #111111; color: orange; }"
            )
            layout.addWidget(table)

            # Helper to refresh table contents from DB
            def refresh() -> None:
                cur = self.conn.cursor()
                cur.execute(
                    """
                    SELECT id, job_number, client_name, amount, quote_number,
                           description, order_number, invoice_number,
                           invoiced, status
                    FROM customer_projects
                    WHERE customer_id = ?
                    """,
                    (customer["id"],),
                )
                rows = cur.fetchall()
                table.setRowCount(len(rows))
                # Attach IDs to the table for delete operations
                table.project_ids = [r[0] for r in rows]  # type: ignore[attr-defined]
                for row_index, r in enumerate(rows):
                    # r[1:] are the display columns
                    for col_index, value in enumerate(r[1:]):
                        if col_index == 7:  # invoiced flag
                            display = "Yes" if value else "No"
                        else:
                            display = value or ""
                        item = QTableWidgetItem(display)
                        table.setItem(row_index, col_index, item)

            # Buttons row
            btn_row = QHBoxLayout()
            btn_add = QPushButton("Add Project / Job", parent=dialog)
            btn_delete = QPushButton("Delete Selected", parent=dialog)
            btn_add.setStyleSheet(
                "QPushButton { background-color: #111; color: orange; padding: 6px 10px; border: 1px solid #444; }"
                "QPushButton:hover { background-color: #222; }"
            )
            btn_delete.setStyleSheet(
                "QPushButton { background-color: #111; color: orange; padding: 6px 10px; border: 1px solid #444; }"
                "QPushButton:hover { background-color: #222; }"
            )

            btn_row.addWidget(btn_add)
            btn_row.addWidget(btn_delete)
            btn_row_container = QFrame(dialog)
            btn_row_container.setLayout(btn_row)
            layout.addWidget(btn_row_container)

            # Close button at bottom
            close_buttons = QDialogButtonBox(QDialogButtonBox.Close, parent=dialog)
            close_buttons.rejected.connect(dialog.reject)
            close_buttons.accepted.connect(dialog.accept)
            layout.addWidget(close_buttons)

            def on_add() -> None:
                sub = QDialog(dialog)
                sub.setWindowTitle("Add Project / Job")
                sub_layout = QFormLayout(sub)

                job_edit = QLineEdit()
                client_edit = QLineEdit()
                amount_edit = QLineEdit()
                quote_edit = QLineEdit()
                desc_edit = QLineEdit()
                order_edit = QLineEdit()
                invoice_edit = QLineEdit()

                invoiced_combo = QComboBox()
                invoiced_combo.addItems(["No", "Yes"])

                status_edit = QLineEdit("Open")

                sub_layout.addRow("Job Number:", job_edit)
                sub_layout.addRow("Client / Representative:", client_edit)
                sub_layout.addRow("Amount:", amount_edit)
                sub_layout.addRow("Quote #:", quote_edit)
                sub_layout.addRow("Description:", desc_edit)
                sub_layout.addRow("Order #:", order_edit)
                sub_layout.addRow("Invoice #:", invoice_edit)
                sub_layout.addRow("Invoiced:", invoiced_combo)
                sub_layout.addRow("Status:", status_edit)

                sub_buttons = QDialogButtonBox(
                    QDialogButtonBox.Save | QDialogButtonBox.Cancel,
                    parent=sub,
                )
                sub_layout.addWidget(sub_buttons)

                sub_buttons.accepted.connect(sub.accept)
                sub_buttons.rejected.connect(sub.reject)

                if sub.exec() == QDialog.Accepted:
                    job_number = job_edit.text().strip()
                    client_name = client_edit.text().strip()
                    amount = amount_edit.text().strip()
                    quote_number = quote_edit.text().strip()
                    description = desc_edit.text().strip()
                    order_number = order_edit.text().strip()
                    invoice_number = invoice_edit.text().strip()
                    invoiced_flag = 1 if invoiced_combo.currentText() == "Yes" else 0
                    status_val = status_edit.text().strip() or "Open"

                    if not job_number:
                        QMessageBox.warning(
                            dialog,
                            "Add Project / Job",
                            "Job Number cannot be empty.",
                        )
                        return

                    cur = self.conn.cursor()
                    cur.execute(
                        """
                        INSERT INTO customer_projects (
                            customer_id, job_number, client_name, amount,
                            quote_number, description, order_number,
                            invoice_number, invoiced, status
                        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                        """,
                        (
                            customer["id"],
                            job_number,
                            client_name,
                            amount,
                            quote_number,
                            description,
                            order_number,
                            invoice_number,
                            invoiced_flag,
                            status_val,
                        ),
                    )
                    self.conn.commit()
                    refresh()
                    # Also refresh inline jobs table if visible
                    self._refresh_jobs_table_for_selected_customer()

            def on_delete() -> None:
                row_index = table.currentRow()
                if row_index < 0:
                    QMessageBox.information(
                        dialog,
                        "Delete Project / Job",
                        "Please select a row to delete.",
                    )
                    return

                try:
                    proj_id = table.project_ids[row_index]  # type: ignore[attr-defined]
                except Exception:
                    QMessageBox.warning(
                        dialog,
                        "Delete Project / Job",
                        "Internal error: could not determine selected project.",
                    )
                    return

                reply = QMessageBox.question(
                    dialog,
                    "Delete Project / Job",
                    "Are you sure you want to delete this project/job?",
                    QMessageBox.Yes | QMessageBox.No,
                    QMessageBox.No,
                )
                if reply == QMessageBox.Yes:
                    cur = self.conn.cursor()
                    cur.execute(
                        "DELETE FROM customer_projects WHERE id = ?",
                        (proj_id,),
                    )
                    self.conn.commit()
                    refresh()
                    # Also refresh inline jobs table if visible
                    self._refresh_jobs_table_for_selected_customer()

            btn_add.clicked.connect(on_add)
            btn_delete.clicked.connect(on_delete)

            refresh()
            dialog.exec()

        def _show_customer_files(self, customer: Dict[str, str]) -> None:
            """Show a dialog listing Files/Documents for a given customer."""

            dialog = QDialog(self)
            dialog.setWindowTitle(f"Files / Documents for {customer.get('name', '')}")
            layout = QVBoxLayout(dialog)

            table = QTableWidget(0, 2, parent=dialog)
            table.setHorizontalHeaderLabels(["File Path", "Description"])
            table.setStyleSheet(
                "QTableWidget { background-color: #000000; color: orange; gridline-color: #444444; }"
                "QHeaderView::section { background-color: #111111; color: orange; }"
            )
            layout.addWidget(table)

            def refresh() -> None:
                cur = self.conn.cursor()
                cur.execute(
                    "SELECT id, file_path, description FROM customer_files WHERE customer_id = ?",
                    (customer["id"],),
                )
                rows = cur.fetchall()
                table.setRowCount(len(rows))
                table.file_ids = [r[0] for r in rows]  # type: ignore[attr-defined]
                for row_index, r in enumerate(rows):
                    for col_index, value in enumerate(r[1:]):
                        item = QTableWidgetItem(value or "")
                        table.setItem(row_index, col_index, item)

            btn_row = QHBoxLayout()
            btn_add = QPushButton("Add File", parent=dialog)
            btn_delete = QPushButton("Delete Selected", parent=dialog)
            btn_add.setStyleSheet(
                "QPushButton { background-color: #111; color: orange; padding: 6px 10px; border: 1px solid #444; }"
                "QPushButton:hover { background-color: #222; }"
            )
            btn_delete.setStyleSheet(
                "QPushButton { background-color: #111; color: orange; padding: 6px 10px; border: 1px solid #444; }"
                "QPushButton:hover { background-color: #222; }"
            )

            btn_row.addWidget(btn_add)
            btn_row.addWidget(btn_delete)
            btn_row_container = QFrame(dialog)
            btn_row_container.setLayout(btn_row)
            layout.addWidget(btn_row_container)

            close_buttons = QDialogButtonBox(QDialogButtonBox.Close, parent=dialog)
            close_buttons.rejected.connect(dialog.reject)
            close_buttons.accepted.connect(dialog.accept)
            layout.addWidget(close_buttons)

            def on_add_file() -> None:
                sub = QDialog(dialog)
                sub.setWindowTitle("Add File / Document")
                sub_layout = QFormLayout(sub)

                path_edit = QLineEdit()
                desc_edit = QLineEdit()

                sub_layout.addRow("File Path:", path_edit)
                sub_layout.addRow("Description:", desc_edit)

                sub_buttons = QDialogButtonBox(
                    QDialogButtonBox.Save | QDialogButtonBox.Cancel,
                    parent=sub,
                )
                sub_layout.addWidget(sub_buttons)

                sub_buttons.accepted.connect(sub.accept)
                sub_buttons.rejected.connect(sub.reject)

                if sub.exec() == QDialog.Accepted:
                    file_path = path_edit.text().strip()
                    description = desc_edit.text().strip()

                    if not file_path:
                        QMessageBox.warning(
                            dialog,
                            "Add File / Document",
                            "File Path cannot be empty.",
                        )
                        return

                    cur = self.conn.cursor()
                    cur.execute(
                        "INSERT INTO customer_files (customer_id, file_path, description) "
                        "VALUES (?, ?, ?)",
                        (customer["id"], file_path, description),
                    )
                    self.conn.commit()
                    refresh()

            def on_delete_file() -> None:
                row_index = table.currentRow()
                if row_index < 0:
                    QMessageBox.information(
                        dialog,
                        "Delete File / Document",
                        "Please select a row to delete.",
                    )
                    return

                try:
                    file_id = table.file_ids[row_index]  # type: ignore[attr-defined]
                except Exception:
                    QMessageBox.warning(
                        dialog,
                        "Delete File / Document",
                        "Internal error: could not determine selected file.",
                    )
                    return

                reply = QMessageBox.question(
                    dialog,
                    "Delete File / Document",
                    "Are you sure you want to delete this file/document entry?",
                    QMessageBox.Yes | QMessageBox.No,
                    QMessageBox.No,
                )
                if reply == QMessageBox.Yes:
                    cur = self.conn.cursor()
                    cur.execute(
                        "DELETE FROM customer_files WHERE id = ?",
                        (file_id,),
                    )
                    self.conn.commit()
                    refresh()

            btn_add.clicked.connect(on_add_file)
            btn_delete.clicked.connect(on_delete_file)

            refresh()
            dialog.exec()

        def _edit_selected_customer(self) -> None:
            """Open an edit dialog for the selected customer.

            Updates the database and refreshes the table on save.
            """

            index = self._get_selected_customer_index()
            if index is None:
                QMessageBox.information(
                    self,
                    "Edit Customer",
                    "Please select a customer row to edit.",
                )
                return

            customer = self.customers[index]

            dialog = QDialog(self)
            dialog.setWindowTitle("Edit Customer")
            form_layout = QFormLayout(dialog)

            name_edit = QLineEdit(customer.get("name", ""))
            address_edit = QLineEdit(customer.get("address", ""))
            contact_edit = QLineEdit(customer.get("contact", ""))
            phone_edit = QLineEdit(customer.get("phone", ""))
            reg_edit = QLineEdit(customer.get("reg", ""))
            vat_edit = QLineEdit(customer.get("vat", ""))

            form_layout.addRow("Customer Name:", name_edit)
            form_layout.addRow("Address:", address_edit)
            form_layout.addRow("Contact Person:", contact_edit)
            form_layout.addRow("Telephone Number:", phone_edit)
            form_layout.addRow("Registration Number:", reg_edit)
            form_layout.addRow("VAT Number:", vat_edit)

            buttons = QDialogButtonBox(
                QDialogButtonBox.Save | QDialogButtonBox.Cancel,
                parent=dialog,
            )
            form_layout.addWidget(buttons)

            buttons.accepted.connect(dialog.accept)
            buttons.rejected.connect(dialog.reject)

            if dialog.exec() == QDialog.Accepted:
                name = name_edit.text().strip()
                address = address_edit.text().strip()
                contact = contact_edit.text().strip()
                phone = phone_edit.text().strip()
                reg = reg_edit.text().strip()
                vat = vat_edit.text().strip()

                if not name:
                    QMessageBox.warning(
                        self,
                        "Edit Customer",
                        "Customer name cannot be empty.",
                    )
                    return

                cur = self.conn.cursor()
                cur.execute(
                    "UPDATE customers SET name = ?, address = ?, contact = ?, phone = ?, reg = ?, vat = ? "
                    "WHERE id = ?",
                    (name, address, contact, phone, reg, vat, customer["id"]),
                )
                self.conn.commit()

                self._reload_customers_from_db()
                self._refresh_customer_table()
                self.content_title.setText("Customer Updated")
                self.content_body.setText(
                    f"Customer '{name}' has been updated successfully."
                )

        def _delete_selected_customer(self) -> None:
            """Delete the selected customer after confirmation."""

            index = self._get_selected_customer_index()
            if index is None:
                QMessageBox.information(
                    self,
                    "Delete Customer",
                    "Please select a customer row to delete.",
                )
                return

            customer = self.customers[index]
            name = customer.get("name", "(unnamed)")

            reply = QMessageBox.question(
                self,
                "Delete Customer",
                f"Are you sure you want to delete '{name}'?",
                QMessageBox.Yes | QMessageBox.No,
                QMessageBox.No,
            )

            if reply == QMessageBox.Yes:
                cur = self.conn.cursor()
                cur.execute("DELETE FROM customers WHERE id = ?", (customer["id"],))
                self.conn.commit()

                self._reload_customers_from_db()
                self._refresh_customer_table()
                self.content_title.setText("Customer Deleted")
                self.content_body.setText(
                    f"Customer '{name}' has been deleted from the database."
                )

        def _get_selected_iso_index(self) -> Optional[int]:
            """Return the index of the currently selected ISO row, or None.

            The index refers to ``self.current_iso_docs``, i.e. the list of
            documents currently shown in the ISO table.
            """

            if not hasattr(self, "current_iso_docs"):
                return None

            row = self.iso_table.currentRow()
            if row < 0 or row >= len(self.current_iso_docs):
                return None
            return row

        def _add_iso_document(self) -> None:
            """Open a dialog to add a new ISO document.

            Defaults the ISO type to the currently selected ISO in the
            sidebar dropdown, but allows overriding.
            """

            dialog = QDialog(self)
            dialog.setWindowTitle("Add ISO Document")
            form_layout = QFormLayout(dialog)

            current_iso = self.combo_iso.currentText()
            if not current_iso or current_iso.lower().startswith("select"):
                current_iso = "ISO 9001"

            iso_type_edit = QLineEdit(current_iso)
            name_edit = QLineEdit()
            revision_edit = QLineEdit("A")
            date_edit = QLineEdit("2024-01-01")
            status_edit = QLineEdit("Active")

            form_layout.addRow("ISO Type:", iso_type_edit)
            form_layout.addRow("Document Name:", name_edit)
            form_layout.addRow("Revision:", revision_edit)
            form_layout.addRow("Date (YYYY-MM-DD):", date_edit)
            form_layout.addRow("Status:", status_edit)

            buttons = QDialogButtonBox(
                QDialogButtonBox.Save | QDialogButtonBox.Cancel,
                parent=dialog,
            )
            form_layout.addWidget(buttons)

            buttons.accepted.connect(dialog.accept)
            buttons.rejected.connect(dialog.reject)

            if dialog.exec() == QDialog.Accepted:
                iso_type = iso_type_edit.text().strip()
                name = name_edit.text().strip()
                revision = revision_edit.text().strip()
                doc_date = date_edit.text().strip()
                status = status_edit.text().strip()

                if not iso_type or not name:
                    QMessageBox.warning(
                        self,
                        "Add ISO Document",
                        "ISO Type and Document Name cannot be empty.",
                    )
                    return

                cur = self.conn.cursor()
                cur.execute(
                    "INSERT INTO iso_documents (iso_type, name, revision, doc_date, status) "
                    "VALUES (?, ?, ?, ?, ?)",
                    (iso_type, name, revision, doc_date, status),
                )
                self.conn.commit()

                self._reload_iso_from_db()
                # Refresh display for current ISO selection
                self._handle_iso_selection(self.combo_iso.currentText())

        def _edit_selected_iso(self) -> None:
            """Open an edit dialog for the selected ISO document."""

            index = self._get_selected_iso_index()
            if index is None:
                QMessageBox.information(
                    self,
                    "Edit ISO Document",
                    "Please select an ISO document row to edit.",
                )
                return

            doc = self.current_iso_docs[index]

            dialog = QDialog(self)
            dialog.setWindowTitle("Edit ISO Document")
            form_layout = QFormLayout(dialog)

            iso_type_edit = QLineEdit(doc.get("iso_type", ""))
            name_edit = QLineEdit(doc.get("name", ""))
            revision_edit = QLineEdit(doc.get("revision", ""))
            date_edit = QLineEdit(doc.get("doc_date", ""))
            status_edit = QLineEdit(doc.get("status", ""))

            form_layout.addRow("ISO Type:", iso_type_edit)
            form_layout.addRow("Document Name:", name_edit)
            form_layout.addRow("Revision:", revision_edit)
            form_layout.addRow("Date (YYYY-MM-DD):", date_edit)
            form_layout.addRow("Status:", status_edit)

            buttons = QDialogButtonBox(
                QDialogButtonBox.Save | QDialogButtonBox.Cancel,
                parent=dialog,
            )
            form_layout.addWidget(buttons)

            buttons.accepted.connect(dialog.accept)
            buttons.rejected.connect(dialog.reject)

            if dialog.exec() == QDialog.Accepted:
                iso_type = iso_type_edit.text().strip()
                name = name_edit.text().strip()
                revision = revision_edit.text().strip()
                doc_date = date_edit.text().strip()
                status = status_edit.text().strip()

                if not iso_type or not name:
                    QMessageBox.warning(
                        self,
                        "Edit ISO Document",
                        "ISO Type and Document Name cannot be empty.",
                    )
                    return

                cur = self.conn.cursor()
                cur.execute(
                    "UPDATE iso_documents SET iso_type = ?, name = ?, revision = ?, doc_date = ?, status = ? "
                    "WHERE id = ?",
                    (
                        iso_type,
                        name,
                        revision,
                        doc_date,
                        status,
                        doc["id"],
                    ),
                )
                self.conn.commit()

                self._reload_iso_from_db()
                self._handle_iso_selection(self.combo_iso.currentText())

        def _delete_selected_iso(self) -> None:
            """Delete the selected ISO document after confirmation."""

            index = self._get_selected_iso_index()
            if index is None:
                QMessageBox.information(
                    self,
                    "Delete ISO Document",
                    "Please select an ISO document row to delete.",
                )
                return

            doc = self.current_iso_docs[index]
            name = doc.get("name", "(unnamed)")

            reply = QMessageBox.question(
                self,
                "Delete ISO Document",
                f"Are you sure you want to delete '{name}'?",
                QMessageBox.Yes | QMessageBox.No,
                QMessageBox.No,
            )

            if reply == QMessageBox.Yes:
                cur = self.conn.cursor()
                cur.execute("DELETE FROM iso_documents WHERE id = ?", (doc["id"],))
                self.conn.commit()

                self._reload_iso_from_db()
                self._handle_iso_selection(self.combo_iso.currentText())

        def closeEvent(self, event) -> None:  # type: ignore[override]
            """Ensure the database connection is closed on window close."""

            try:
                if hasattr(self, "conn"):
                    self.conn.close()
            except Exception:
                pass
            super().closeEvent(event)


# ---------------------------------------------------------------------------
# Simple sanity tests (soft warnings only)
# ---------------------------------------------------------------------------

def _run_sanity_tests() -> None:
    """Run small internal tests.

    These are intentionally lightweight so they can run in environments
    without PySide6. They don't create any windows; they only test the
    helper functions and module-level flags.

    The tests now print warnings instead of raising hard AssertionErrors,
    so that the application can still start (Option 2: soft behaviour).
    """

    def warn(msg: str) -> None:
        sys.stderr.write(f"[sanity warning] {msg}\n")

    # Test 1: availability flag is boolean
    try:
        if not isinstance(is_pyside_available(), bool):
            warn("PYSIDE_AVAILABLE must be a bool.")
    except Exception as exc:  # very defensive
        warn(f"Test 1 failed unexpectedly: {exc}")

    # Test 2: get_logo_path returns either None or an existing path
    try:
        path = get_logo_path("Tetracube_Logo.jpg")
        if path is not None and not os.path.exists(path):
            warn("If get_logo_path returns a path, it should exist.")
    except Exception as exc:
        warn(f"Test 2 failed unexpectedly: {exc}")

    # Test 3: get_logo_path returns None for a clearly non-existent file
    try:
        missing = get_logo_path("this_file_should_not_exist_12345.xyz")
        if missing is not None:
            warn("Non-existent files should return None from get_logo_path.")
    except Exception as exc:
        warn(f"Test 3 failed unexpectedly: {exc}")

    # Test 4: is_pyside_available agrees with the PYSIDE_AVAILABLE flag
    try:
        if is_pyside_available() is not PYSIDE_AVAILABLE:
            warn("is_pyside_available should mirror PYSIDE_AVAILABLE.")
    except Exception as exc:
        warn(f"Test 4 failed unexpectedly: {exc}")


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main() -> None:
    """Entry point for running the landing page.

    If PySide6 is missing, a clear error message is printed and the
    function simply returns instead of raising SystemExit. This keeps
    automated or sandboxed environments from failing hard while still
    informing the user what is wrong.
    """

    if not PYSIDE_AVAILABLE:
        sys.stderr.write(
            "Error: PySide6 is not installed or not available in this environment.\n"
        )
        sys.stderr.write(
            "Install it with: pip install PySide6  (inside your active environment)\n"
        )
        # Do not terminate the whole process; just return gracefully.
        return

    app = QApplication(sys.argv)
    window = LandingPage()  # type: ignore[name-defined]
    window.show()
    sys.exit(app.exec())


if __name__ == "__main__":
    # Optionally run tests if requested
    if os.environ.get("RUN_WELDADMIN_TESTS") == "1":
        _run_sanity_tests()

    main()
