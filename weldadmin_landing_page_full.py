"""WeldAdmin Pro - Landing Page (consolidated)

This file implements the landing page UI with Customers, ISO, Jobs/Projects,
Welding Documents linking, Job Details, Job History, file attachments,
inline job editing, and more. It includes automatic SQLite migrations.

Note: Requires PySide6 for GUI. If PySide6 is not available the module
prints an error and exits gracefully.

Save this file to your WeldAdminPro folder and run:

    python weldadmin_landing_page_full.py

"""
from __future__ import annotations
import os
import sys
import sqlite3
import datetime
import subprocess
from pathlib import Path
from typing import List, Dict, Optional

# Optional PySide6 import
PYSIDE_AVAILABLE = True
try:
    from PySide6.QtWidgets import (
        QApplication, QWidget, QLabel, QVBoxLayout, QHBoxLayout, QFrame,
        QPushButton, QComboBox, QInputDialog, QDialog, QFormLayout,
        QLineEdit, QDialogButtonBox, QTableWidget, QTableWidgetItem,
        QMessageBox, QFileDialog, QListWidget
    )
    from PySide6.QtGui import QPixmap
    from PySide6.QtCore import Qt
except Exception:
    PYSIDE_AVAILABLE = False


# ---------------------- Helpers & DB migrations -------------------------

def get_logo_path(preferred_name: str = "Tetracube_Logo.jpg") -> Optional[str]:
    candidate = os.path.abspath(preferred_name)
    return candidate if os.path.exists(candidate) else None


def ensure_migrations(db_path: str) -> None:
    """Run safe migrations to ensure required tables/columns exist."""
    conn = sqlite3.connect(db_path)
    cur = conn.cursor()

    # customers table
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

    # base customer_projects
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

    # ensure columns for projects
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
