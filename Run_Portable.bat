@echo off
setlocal enabledelayedexpansion
REM Ensure we run from the folder where this script lives:
set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"
@echo off
setlocal
set EXE=dist\app.exe
if exist "%EXE%" (
  echo Data will be stored in "%USERPROFILE%\Documents\WeldAdminProData"
  if not exist "%USERPROFILE%\Documents\WeldAdminProData" mkdir "%USERPROFILE%\Documents\WeldAdminProData"
  echo Starting portable WeldAdmin Pro...
  "%EXE%"
) else (
  echo Portable EXE not found at %EXE%.
  echo Run Build_EXE.bat to create it, or use Start_WeldAdminPro.bat to run from source.
  pause
)
