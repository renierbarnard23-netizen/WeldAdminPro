@echo off
setlocal enabledelayedexpansion
REM Ensure we run from the folder where this script lives:
set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"
@echo off
setlocal enabledelayedexpansion
if not exist .venv (
  echo Virtual env not found. Run Start_WeldAdminPro.bat first.
  pause
  exit /b 1
)
call .venv\Scripts\activate
python --version
if errorlevel 1 (
  echo Python not available in venv.
  pause
  exit /b 1
)
echo Installing a PyInstaller version compatible with your Python...
pip install "pyinstaller>=6.10,<7"
if errorlevel 1 (
  echo Failed to install PyInstaller. Check your internet/proxy and try again.
  pause
  exit /b 1
)
where pyinstaller
if errorlevel 1 (
  echo PyInstaller not on PATH even after install.
  pause
  exit /b 1
)
pyinstaller --onefile --collect-all docx --collect-all pdfplumber --collect-all PIL --collect-all pytesseract --collect-all apscheduler --collect-all flask_wtf --add-data "templates;templates" --add-data "static;static" app.py
if errorlevel 1 (
  echo Build failed.
  pause
  exit /b 1
)
echo:
echo EXE created at dist\app.exe
pause


echo NOTE: The portable EXE stores data in "%USERPROFILE%\Documents\WeldAdminProData".
