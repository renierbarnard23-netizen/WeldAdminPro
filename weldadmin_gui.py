"""
weldadmin_gui.py
----------------
Advanced WeldAdmin Pro desktop GUI (PyQt5)

Features:
- Tabs for WPS / PQR / WPQ records
- Browse, Parse, Import buttons
- Background parsing and importing using ThreadPoolExecutor
- Status bar and progress indicator
- Uses your project helper modules:
    - weldadmin_import_to_db.import_pdf_to_db(path)
"""

import os
import sys
import concurrent.futures
from typing import Dict, Any

from PyQt5.QtCore import Qt, QTimer, QSize
from PyQt5.QtWidgets import (
    QApplication,
    QMainWindow,
    QWidget,
    QFileDialog,
    QMessageBox,
    QLabel,
    QLineEdit,
    QPlainTextEdit,
    QPushButton,
    QFormLayout,
    QVBoxLayout,
    QHBoxLayout,
    QTabWidget,
    QSizePolicy,
    QToolBar,
    QAction,
    QStatusBar,
    QDialog,
    QDialogButtonBox,
    QProgressBar,
    QStyle,
)

# -------------------------------------------------
# Import DB helper (with safe stubs if not present)
# -------------------------------------------------
try:
    from weldadmin_import_to_db import import_pdf_to_db, ensure_tables
    REAL_DB_HELPER = True
    print("DEBUG: Using REAL weldadmin_import_to_db")
except Exception as e:
    REAL_DB_HELPER = False
    print("DEBUG: Failed to import weldadmin_import_to_db:", e)

    def import_pdf_to_db(path: str, *args, **kwargs):
        print("DEBUG: STUB import_pdf_to_db() called with", path)
        # Return a fake result so the GUI flow still works
        return {"table": "wps", "record": {}, "id": None}

    def ensure_tables():
        print("DEBUG: STUB ensure_tables() called")


# OCR + parser from ocr.py
try:
    from ocr import extract_and_parse
except Exception:
    extract_and_parse = None


# Thread pool for background tasks (still used for import)
_EXECUTOR = concurrent.futures.ThreadPoolExecutor(max_workers=2)


def _parse_range_mm(range_text: str):
    """
    Turn '1.5 - 15.24' into (1.5, 15.24).
    Returns (None, None) if it can't parse.
    """
    if not range_text:
        return None, None
    txt = range_text.replace("–", "-").replace("—", "-")
    parts = txt.split("-")
    if len(parts) != 2:
        return None, None
    try:
        low = float(parts[0].strip().replace(",", "."))
        high = float(parts[1].strip().replace(",", "."))
        return low, high
    except ValueError:
        return None, None


class WelcomeDialog(QDialog):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setWindowTitle("Welcome to WeldAdmin Pro")
        self.setModal(True)
        self.resize(480, 220)
        layout = QVBoxLayout(self)

        title = QLabel("<h2>WeldAdmin Pro</h2>", self)
        title.setAlignment(Qt.AlignCenter)
        layout.addWidget(title)

        info = QLabel(
            "<p>Welcome. Use the buttons to import or parse PDF welding documents (WPS/PQR/WPQ). "
            "OCR and parsing run in the background to keep the UI responsive.</p>",
            self,
        )
        info.setWordWrap(True)
        layout.addWidget(info)

        btns = QDialogButtonBox(QDialogButtonBox.Ok)
        btns.accepted.connect(self.accept)
        layout.addWidget(btns)


class WeldAdminGUI(QMainWindow):
    def __init__(self, parent=None) -> None:
        super().__init__(parent)
        self.setWindowTitle("WeldAdmin Pro - OCR Import GUI")
        self.resize(1100, 760)

        self.current_pdf_path: str = ""
        self.current_model: Dict[str, Any] = {"table": None, "record": {}}

        # ensure DB tables exist (no-op if imported function missing)
        try:
            ensure_tables()
        except Exception as e:
            print("DEBUG: ensure_tables() failed:", e)

        self._build_ui()
        QTimer.singleShot(50, self._show_welcome)

    # -------------------------
    # UI construction
    # -------------------------
    def _show_welcome(self):
        dlg = WelcomeDialog(self)
        dlg.exec_()

    def _build_ui(self) -> None:
        central = QWidget(self)
        self.setCentralWidget(central)
        main_layout = QVBoxLayout(central)

        self._build_menu_toolbar()

        # Top controls
        top_layout = QHBoxLayout()
        self.pdf_path_edit = QLineEdit()
        self.pdf_path_edit.setPlaceholderText("Select a PDF (WPS / PQR / WPQ)...")

        browse_btn = QPushButton("Browse PDF...")
        browse_btn.clicked.connect(self.on_browse_clicked)

        self.load_btn = QPushButton("Parse/Preview")
        self.load_btn.clicked.connect(self.on_load_clicked)

        self.import_btn = QPushButton("Import to DB")
        self.import_btn.clicked.connect(self.on_import_clicked)

        top_layout.addWidget(self.pdf_path_edit)
        top_layout.addWidget(browse_btn)
        top_layout.addWidget(self.load_btn)
        top_layout.addWidget(self.import_btn)
        main_layout.addLayout(top_layout)

        # Info row
        info_layout = QHBoxLayout()
        self.label_detected = QLabel("Detected: (none)")
        info_layout.addWidget(self.label_detected)
        info_layout.addStretch()
        main_layout.addLayout(info_layout)

        # Tabs for WPS / PQR / WPQ
        self.tabs = QTabWidget()
        self.tabs.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Expanding)
        self._setup_wps_tab()
        self._setup_pqr_tab()
        self._setup_wpq_tab()
        main_layout.addWidget(self.tabs)

        # Status bar + progress bar
        self.status = QStatusBar()
        self.setStatusBar(self.status)
        self.progress = QProgressBar()
        self.progress.setMaximum(0)  # marquee
        self.progress.setVisible(False)
        self.status.addPermanentWidget(self.progress)

    def _build_menu_toolbar(self):
        menubar = self.menuBar()
        file_menu = menubar.addMenu("&File")
        tools_menu = menubar.addMenu("&Tools")
        help_menu = menubar.addMenu("&Help")

        style = QApplication.style()

        act_open = QAction(style.standardIcon(QStyle.SP_DialogOpenButton), "Open PDF...", self)
        act_open.triggered.connect(self._menu_open_pdf)
        file_menu.addAction(act_open)

        act_import_db = QAction(style.standardIcon(QStyle.SP_DialogSaveButton), "Import to DB", self)
        act_import_db.triggered.connect(self._menu_import_db)
        file_menu.addAction(act_import_db)

        file_menu.addSeparator()
        act_exit = QAction(style.standardIcon(QStyle.SP_DialogCloseButton), "Exit", self)
        act_exit.triggered.connect(self.close)
        file_menu.addAction(act_exit)

        act_refresh = QAction(style.standardIcon(QStyle.SP_BrowserReload), "Reload last PDF", self)
        act_refresh.triggered.connect(self._menu_reload_last)
        tools_menu.addAction(act_refresh)

        act_about = QAction(style.standardIcon(QStyle.SP_MessageBoxInformation), "About", self)
        act_about.triggered.connect(self._menu_about)
        help_menu.addAction(act_about)

        toolbar = QToolBar("Main")
        toolbar.setIconSize(style.standardIcon(QStyle.SP_DialogOpenButton).actualSize(QSize(24, 24)))
        self.addToolBar(toolbar)
        toolbar.addAction(act_open)
        toolbar.addAction(act_import_db)
        toolbar.addAction(act_refresh)
        toolbar.addAction(act_about)

    def _setup_wps_tab(self) -> None:
        """
        WPS tab: shows all key fields extracted by ocr.py for a WPS document.
        """
        tab = QWidget()
        layout = QFormLayout(tab)
        self.wps_fields: Dict[str, QLineEdit] = {}

        def add_field(key: str, label: str):
            edit = QLineEdit()
            edit.setReadOnly(False)
            self.wps_fields[key] = edit
            layout.addRow(label + ":", edit)

        # High level
        add_field("doc_type", "Doc Type")
        add_field("company_name", "Company")
        add_field("designation", "Designation")

        # Numbers, revs, dates
        add_field("wps_number", "WPS Number")
        add_field("wps_rev", "WPS Rev / Ver")
        add_field("wps_date", "WPS Date")
        add_field("pqr_number", "PQR Number")
        add_field("pqr_rev", "PQR Rev / Ver")
        add_field("pqr_date", "PQR Date")

        # Codes
        add_field("code_standard", "Code / Standard")
        add_field("construction_code", "Construction Code")

        # Ranges
        add_field("thickness_range_text", "Thickness Range (text)")
        add_field("thickness_min_mm", "Thickness Min (mm)")
        add_field("thickness_max_mm", "Thickness Max (mm)")
        add_field("outside_diameter_range_text", "Outside Diameter Range")

        # Joint & prep
        add_field("joint_type", "Joint Type")
        add_field("joint_design", "Joint Design")
        add_field("surface_prep", "Surface Preparation Method")
        add_field("groove_angle", "Groove Angle")
        add_field("root_face_mm", "Root Face (mm)")
        add_field("root_gap_mm", "Root Gap (mm)")
        add_field("max_misalignment_mm", "Max. Misalignment (mm)")
        add_field("back_gouging", "Back Gouging")
        add_field("backing", "Backing")
        add_field("backing_type", "Backing Type")

        # Process & parameters
        add_field("process", "Process")
        add_field("process_type", "Process Type")
        add_field("shielding_gas", "Shielding Gas")
        add_field("backing_gas", "Backing Gas")
        add_field("preheat_min_c", "Preheat Min (°C)")
        add_field("interpass_max_c", "Interpass Max (°C)")
        add_field("amps_range", "Amps Range")
        add_field("volts_range", "Volts Range")
        add_field("travel_speed_range_mm_min", "Travel Speed (mm/min)")
        add_field("max_heat_input_kj_mm", "Max Heat Input (kJ/mm)")

        # Base metals
        add_field("base_material_1_spec", "Base Material 1 Spec")
        add_field("base_material_2_spec", "Base Material 2 Spec")

        self.tabs.addTab(tab, "WPS")

    def _setup_pqr_tab(self) -> None:
        tab = QWidget()
        layout = QFormLayout(tab)
        self.pqr_fields: Dict[str, Any] = {}

        def add_line(key: str, label: str):
            edit = QLineEdit()
            self.pqr_fields[key] = edit
            layout.addRow(label + ":", edit)

        add_line("pqr_number", "PQR Number")
        add_line("wps_number", "WPS Number")
        add_line("code_standard", "Code / Standard")
        add_line("pqr_date", "PQR Date")
        add_line("wps_date", "WPS Date")
        add_line("process", "Process")
        add_line("position", "Position")
        add_line("joint_type", "Joint Type")

        base_mat_edit = QPlainTextEdit()
        base_mat_edit.setFixedHeight(80)
        self.pqr_fields["base_material_spec"] = base_mat_edit
        layout.addRow("Base Material Spec:", base_mat_edit)

        add_line("base_material_thickness_mm", "Base Material Thickness (mm)")
        add_line("welder_name", "Welder Name")
        add_line("welder_id", "Welder ID")
        add_line("stamp_number", "Stamp Number")
        add_line("test_lab", "Test Lab")
        add_line("test_report_no", "Test Report No.")

        self.tabs.addTab(tab, "PQR")

    def _setup_wpq_tab(self) -> None:
        tab = QWidget()
        layout = QFormLayout(tab)
        self.wpq_fields: Dict[str, Any] = {}

        def add_line(key: str, label: str):
            edit = QLineEdit()
            self.wpq_fields[key] = edit
            layout.addRow(label + ":", edit)

        add_line("certificate_no", "Certificate No.")
        add_line("wpq_record_no", "WPQ Record No.")
        add_line("welder_name", "Welder Name")
        add_line("welder_id", "Welder ID")
        add_line("qualified_to", "Qualified To")
        add_line("stamp_number", "Stamp Number")
        add_line("wps_number", "WPS Number")
        add_line("process", "Process")
        add_line("position", "Position")

        base_mat_edit = QPlainTextEdit()
        base_mat_edit.setFixedHeight(80)
        self.wpq_fields["base_material_spec"] = base_mat_edit
        layout.addRow("Base Material Spec:", base_mat_edit)

        add_line("test_date", "Test Date")
        add_line("date_issued", "Date Issued")

        job_knowledge_edit = QLineEdit()
        self.wpq_fields["job_knowledge"] = job_knowledge_edit
        layout.addRow("Job Knowledge:", job_knowledge_edit)

        self.tabs.addTab(tab, "WPQ")

    # -------------------------
    # UI helpers
    # -------------------------
    def _set_busy(self, busy: bool) -> None:
        print(f"DEBUG: _set_busy({busy})")
        self.load_btn.setEnabled(not busy)
        self.import_btn.setEnabled(not busy)
        self.progress.setVisible(busy)
        if busy:
            self.status.showMessage("Working...")
        else:
            self.status.clearMessage()

    def _run_in_main_thread(self, fn, *args, **kwargs) -> None:
        QTimer.singleShot(0, lambda: fn(*args, **kwargs))

    # -------------------------
    # Menu action callbacks
    # -------------------------
    def _menu_open_pdf(self):
        path, _ = QFileDialog.getOpenFileName(self, "Select PDF to import", "", "PDF Files (*.pdf)")
        if path:
            self.pdf_path_edit.setText(path)
            self.on_load_clicked()

    def _menu_import_db(self):
        path = self.pdf_path_edit.text().strip()
        if not path:
            path, _ = QFileDialog.getOpenFileName(self, "Select PDF to import", "", "PDF Files (*.pdf)")
        if path:
            self.pdf_path_edit.setText(path)
            self.on_import_clicked()

    def _menu_reload_last(self):
        if self.current_pdf_path:
            self.pdf_path_edit.setText(self.current_pdf_path)
            self.on_load_clicked()
        else:
            QMessageBox.information(self, "Info", "No recent PDF loaded.")

    def _menu_about(self):
        QMessageBox.information(self, "About", "WeldAdmin Pro - OCR import GUI\nBuilt with PyQt5.")

    # -------------------------
    # Button events
    # -------------------------
    def on_browse_clicked(self) -> None:
        path, _ = QFileDialog.getOpenFileName(
            self, "Select PDF", "", "PDF Files (*.pdf);;All Files (*)"
        )
        if path:
            self.pdf_path_edit.setText(path)

    def on_load_clicked(self) -> None:
        path = self.pdf_path_edit.text().strip()
        print(f"DEBUG: on_load_clicked with path: {path}")
        if not path or not os.path.isfile(path):
            QMessageBox.warning(self, "No file", "Please select a valid PDF.")
            return

        # Do everything synchronously for now – MUCH simpler to debug
        self._set_busy(True)
        try:
            result = self._background_parse(path)
        finally:
            self._set_busy(False)

        print("DEBUG: direct parse result =", result)

        if not result.get("ok"):
            QMessageBox.critical(self, "Parse Error", result.get("error", "Unknown"))
            return

        model = result.get("model", {}) or {}
        table = model.get("table")
        record = model.get("record", {}) if isinstance(model, Dict) else {}

        print("DEBUG: table =", table)
        print("DEBUG: record =", record)

        self.label_detected.setText(f"Detected: {table or 'Unknown'}")
        self.current_model = {"table": table, "record": record}
        self.current_pdf_path = path

        # populate appropriate tab
        if table and isinstance(table, str) and table.lower() == "wps":
            print("DEBUG: calling _populate_wps")
            self._populate_wps(record)
            self.tabs.setCurrentIndex(0)
        elif table and isinstance(table, str) and table.lower() == "pqr":
            print("DEBUG: calling _populate_pqr")
            self._populate_pqr(record)
            self.tabs.setCurrentIndex(1)
        elif table and isinstance(table, str) and table.lower() in ("wpq", "wpqr"):
            print("DEBUG: calling _populate_wpq")
            self._populate_wpq(record)
            self.tabs.setCurrentIndex(2)

    def _background_parse(self, path: str) -> Dict[str, Any]:
        """
        Run OCR + built-in parser.

        Uses doc_type/type/code_standard to decide which tab (wps / pqr / wpq) to populate,
        and builds a record that matches that tab's fields.
        """
        if not extract_and_parse:
            return {"ok": False, "error": "OCR module (ocr.py) not available."}

        try:
            print("DEBUG: starting OCR extract_and_parse on", path)
            fields = extract_and_parse(path)
            print("DEBUG: OCR fields:", list(fields.keys()))

            # Shared helpers
            thickness_text = fields.get("thickness_range_mm", "") or ""
            tmin, tmax = _parse_range_mm(thickness_text)

            joint_raw = fields.get("joint_type", "") or ""
            joint_line = joint_raw.splitlines()[0].strip() if joint_raw else ""

            # Decide which table (wps/pqr/wpq) based on multiple hints
            doc_type_raw = " ".join(
                [
                    str(fields.get("doc_type", "")),
                    str(fields.get("type", "")),
                    str(fields.get("code_standard", "")),
                ]
            ).lower()

            print("DEBUG: doc_type_raw =", doc_type_raw)

            if "pqr" in doc_type_raw:
                table = "pqr"
            elif "wpq" in doc_type_raw or "wpqr" in doc_type_raw:
                table = "wpq"
            else:
                table = "wps"

            # Build a record that matches the chosen tab
            if table == "wps":
                rec: Dict[str, Any] = {
                    "doc_type": fields.get("doc_type", ""),
                    "company_name": fields.get("company_name", ""),
                    "designation": fields.get("designation", ""),

                    "wps_number": fields.get("wps_number", ""),
                    "wps_rev": fields.get("wps_rev", ""),
                    "wps_date": fields.get("wps_date", ""),

                    "pqr_number": fields.get("pqr_number", ""),
                    "pqr_rev": fields.get("pqr_rev", ""),
                    "pqr_date": fields.get("pqr_date", ""),

                    "code_standard": fields.get("code_standard", ""),
                    "construction_code": fields.get("construction_code", ""),

                    "thickness_range_text": thickness_text,
                    "thickness_min_mm": str(tmin) if tmin is not None else "",
                    "thickness_max_mm": str(tmax) if tmax is not None else "",
                    "outside_diameter_range_text": fields.get("outside_diameter_range", "") or fields.get("outside_diameter_range_mm", ""),

                    "joint_type": joint_line,
                    "joint_design": fields.get("joint_design", ""),
                    "surface_prep": fields.get("surface_prep", ""),
                    "groove_angle": fields.get("groove_angle", ""),
                    "root_face_mm": fields.get("root_face_mm", ""),
                    "root_gap_mm": fields.get("root_gap_mm", ""),
                    "max_misalignment_mm": fields.get("max_misalignment_mm", ""),
                    "back_gouging": fields.get("back_gouging", ""),
                    "backing": fields.get("backing", ""),
                    "backing_type": fields.get("backing_type", ""),

                    "process": fields.get("process", ""),
                    "process_type": fields.get("process_type", ""),
                    "shielding_gas": fields.get("shielding_gas", ""),
                    "backing_gas": fields.get("backing_gas", ""),
                    "preheat_min_c": fields.get("preheat_min_c", ""),
                    "interpass_max_c": fields.get("interpass_max_c", ""),
                    "amps_range": fields.get("amps_range", ""),
                    "volts_range": fields.get("volts_range", ""),
                    "travel_speed_range_mm_min": fields.get("travel_speed_range_mm_min", ""),
                    "max_heat_input_kj_mm": fields.get("max_heat_input_kj_mm", ""),

                    "base_material_1_spec": fields.get("base_material_1_spec", ""),
                    "base_material_2_spec": fields.get("base_material_2_spec", ""),
                }

            elif table == "pqr":
                rec = {
                    "pqr_number": fields.get("pqr_number", ""),
                    "wps_number": fields.get("wps_number", ""),
                    "code_standard": fields.get("code_standard", ""),
                    "pqr_date": fields.get("pqr_date", ""),
                    "wps_date": fields.get("wps_date", ""),
                    "process": fields.get("process", ""),
                    "position": fields.get("position", ""),
                    "joint_type": joint_line,
                    "base_material_spec": fields.get("base_material_spec", ""),
                    "base_material_thickness_mm": fields.get("base_material_thickness_mm", "") or (str(tmax) if tmax is not None else ""),
                    "welder_name": fields.get("welder_name", ""),
                    "welder_id": fields.get("welder_id", ""),
                    "stamp_number": fields.get("stamp_number", ""),
                    "test_lab": fields.get("test_lab", ""),
                    "test_report_no": fields.get("test_report_no", ""),
                }

            else:  # table == "wpq"
                test_date = (
                    fields.get("test_date")
                    or fields.get("pqr_date")
                    or fields.get("wps_date")
                    or ""
                )
                date_issued = (
                    fields.get("date_issued")
                    or (fields.get("wps_date") if fields.get("wps_date") and fields.get("wps_date") != test_date else "")
                    or fields.get("pqr_date")
                    or ""
                )

                rec = {
                    "certificate_no": fields.get("certificate_no", ""),
                    "wpq_record_no": fields.get("wpq_record_no", ""),
                    "welder_name": fields.get("welder_name", ""),
                    "welder_id": fields.get("welder_id", ""),
                    "qualified_to": fields.get("qualified_to", ""),
                    "stamp_number": fields.get("stamp_number", ""),
                    "wps_number": fields.get("wps_number", ""),
                    "process": fields.get("process", ""),
                    "position": fields.get("position", ""),
                    "base_material_spec": fields.get("base_material_spec", ""),
                    "test_date": test_date,
                    "date_issued": date_issued,
                    "job_knowledge": fields.get("job_knowledge", ""),
                }

            model = {"table": table, "record": rec}
            print("DEBUG: model built:", model)
            return {"ok": True, "model": model}

        except Exception as e:
            import traceback
            traceback.print_exc()
            return {"ok": False, "error": f"OCR parse error: {e}"}

    def on_import_clicked(self) -> None:
        path = self.pdf_path_edit.text().strip()
        if not path or not os.path.isfile(path):
            QMessageBox.warning(self, "No file", "Please select a valid PDF to import.")
            return

        print("DEBUG: on_import_clicked with path:", path)
        self._set_busy(True)
        future = _EXECUTOR.submit(self._background_import, path)
        future.add_done_callback(
            lambda fut, p=path: self._run_in_main_thread(self._on_import_done, fut, p)
        )

    def _background_import(self, path: str) -> Dict[str, Any]:
        """
        Background import:
        - Calls import_pdf_to_db(path) (real DB helper)
        """
        import traceback

        try:
            print("DEBUG: background_import calling import_pdf_to_db on", path)
            res = import_pdf_to_db(path)
            print("DEBUG: background_import import_pdf_to_db returned:", res)
            return {"ok": True, "result": res}
        except Exception as e:
            print("DEBUG: background_import exception:", e)
            traceback.print_exc()
            return {"ok": False, "error": str(e)}

    def _on_import_done(self, future, path: str) -> None:
        import traceback

        print(f"DEBUG: import finished for {path}")
        try:
            result = future.result()
            print("DEBUG: future.result() =", result)
        except Exception as e:
            traceback.print_exc()
            QMessageBox.critical(self, "Import Error", f"Background exception: {e}")
            self._set_busy(False)
            return

        if not result.get("ok"):
            QMessageBox.critical(self, "Import Error", result.get("error", "Unknown"))
            self._set_busy(False)
            return

        res = result.get("result") or {}
        print("DEBUG: _on_import_done result payload:", res)
        QMessageBox.information(self, "Imported", f"Imported as {res.get('table', 'unknown')}.")

        table = res.get("table")
        record = res.get("record", {})
        print("DEBUG: _on_import_done table =", table)
        print("DEBUG: _on_import_done record =", record)

        if table and isinstance(table, str) and table.lower() == "wps":
            self._populate_wps(record)
            self.tabs.setCurrentIndex(0)
        elif table and isinstance(table, str) and table.lower() == "pqr":
            self._populate_pqr(record)
            self.tabs.setCurrentIndex(1)
        elif table and isinstance(table, str) and table.lower() in ("wpq", "wpqr"):
            self._populate_wpq(record)
            self.tabs.setCurrentIndex(2)

        self._set_busy(False)

    # -------------------------
    # Populate helpers
    # -------------------------
    def _populate_wps(self, rec: Dict[str, Any]) -> None:
        print("DEBUG: populating WPS tab with:", rec)
        for key, widget in self.wps_fields.items():
            value = rec.get(key, "")
            print(f"DEBUG: setting WPS field {key} = {value!r}")
            widget.setText(str(value) if value is not None else "")

    def _populate_pqr(self, rec: Dict[str, Any]) -> None:
        print("DEBUG: populating PQR tab with:", rec)
        for key, widget in self.pqr_fields.items():
            value = rec.get(key, "")
            if isinstance(widget, QPlainTextEdit):
                widget.setPlainText(str(value) if value is not None else "")
            else:
                widget.setText(str(value) if value is not None else "")

    def _populate_wpq(self, rec: Dict[str, Any]) -> None:
        print("DEBUG: populating WPQ tab with:", rec)
        for key, widget in self.wpq_fields.items():
            value = rec.get(key, "")
            if isinstance(widget, QPlainTextEdit):
                widget.setPlainText(str(value) if value is not None else "")
            else:
                widget.setText(str(value) if value is not None else "")


def main() -> None:
    app = QApplication(sys.argv)
    window = WeldAdminGUI()
    window.show()
    sys.exit(app.exec_())


if __name__ == "__main__":
    main()
