@echo off
REM build-windows.bat - creates venv, installs deps and runs PyInstaller

python -m venv build-venv
call build-venv\Scripts\activate.bat

python -m pip install --upgrade pip
python -m pip install PyQt5 pyinstaller

pyinstaller --onefile --noconsole --add-data "weldadmin_auto_map.py;." --add-data "weldadmin_import_to_db.py;." --add-data "parser_weldadmin.py;." --add-data "ocr.py;." weldadmin_gui.py

echo.
echo Build finished. Check the dist\ folder for weldadmin_gui.exe
pause
