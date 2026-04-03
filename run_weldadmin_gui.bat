@echo off
echo Starting WeldAdmin Pro GUI...
echo.

REM --- Path to your virtual environment python ---
set PYTHON_PATH=C:\Users\renie\Documents\WeldAdminPro\buildenv\Scripts\python.exe

REM --- Path to your GUI script ---
set GUI_PATH=C:\Users\renie\Documents\WeldAdminPro\weldadmin_gui.py

REM --- Check if python exists ---
if not exist "%PYTHON_PATH%" (
    echo ERROR: Python inside buildenv was not found!
    echo Expected at:
    echo   %PYTHON_PATH%
    echo.
    echo Please recreate buildenv or update the path.
    pause
    exit /b
)

REM --- Check if GUI file exists ---
if not exist "%GUI_PATH%" (
    echo ERROR: weldadmin_gui.py not found!
    echo Expected at:
    echo   %GUI_PATH%
    echo.
    pause
    exit /b
)

echo Launching WeldAdmin GUI...
start "" "%PYTHON_PATH%" "%GUI_PATH%"
exit
