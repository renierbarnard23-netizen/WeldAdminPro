@echo off
setlocal enabledelayedexpansion
REM Ensure we run from the folder where this script lives:
set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"
@echo off
setlocal
echo === WeldAdmin Pro Quick Start ===
where python >nul 2>nul
if errorlevel 1 (
  echo Python not found. Please install Python 3.11+ from https://www.python.org/downloads/windows/
  pause
  exit /b 1
)
if not exist .venv (
  echo Creating virtual environment...
  python -m venv .venv
)
call .venv\Scripts\activate
python -m pip install --upgrade pip
pip install -r requirements.txt
if not exist .env (
  echo Copying .env.example to .env
  copy /y .env.example .env >nul
  echo IMPORTANT: Edit .env and set FLASK_SECRET_KEY to a strong value.
)
echo Initializing database (admin/admin)...
python - <<PY
from app import app, db, User
with app.app_context():
    db.create_all()
    if not User.query.filter_by(username="admin").first():
        db.session.add(User(username="admin", password="admin", is_admin=True))
        db.session.commit()
print("DB ready.")
PY
echo Starting WeldAdmin Pro on http://127.0.0.1:5000 ...
python app.py


echo Note: For Smart Upload OCR, install Tesseract OCR and (optional) Poppler, then set TESSERACT_PATH and POPPLER_PATH in .env.
