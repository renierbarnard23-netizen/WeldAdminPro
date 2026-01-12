@echo off
REM build-and-package.bat
REM Activate venv, install deps, and run PyInstaller to build weldadmin_gui.exe
REM Save this file in the same folder as weldadmin_gui.py and your helper .py files.

REM Change to the folder where this script is located
cd /d "%~dp0"

REM Check for virtualenv
if not exist "buildenv\Scripts\activate.bat" (
  echo ERROR: Virtual environment 'buildenv' not found in %CD%
  echo Create it first: python -m venv buildenv
  pause
  exit /b 1
)

REM Activate venv
call "buildenv\Scripts\activate.bat"
if errorlevel 1 (
  echo Failed to activate virtualenv.
  pause
  exit /b 1
)

REM Upgrade pip and install dependencies (will skip if already present)
echo.
echo Installing / confirming dependencies...
python -m pip install --upgrade pip
python -m pip install PyQt5 pyinstaller

REM Build with PyInstaller
echo.
echo Running PyInstaller (this may take a few minutes)...
pyinstaller --onefile --noconsole ^
  --add-data "weldadmin_auto_map.py;." ^
  --add-data "weldadmin_import_to_db.py;." ^
  --add-data "parser_weldadmin.py;." ^
  --add-data "ocr.py;." ^
  weldadmin_gui.py

REM Check result
if exist "dist\weldadmin_gui.exe" (
  echo.
  echo BUILD SUCCESSFUL.
  echo Executable created at: %CD%\dist\weldadmin_gui.exe
) else (
  echo.
  echo BUILD FAILED. See messages above for errors from PyInstaller.
)

REM Keep the window open so you can read output
pause

REM Deactivate venv (optional)
if defined VIRTUAL_ENV (
  deactivate 2>nul
)
exit /b 0
