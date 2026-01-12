@echo off
setlocal enabledelayedexpansion
title WeldAdmin Pro - Poppler & Tesseract Check

echo ==================================================
echo   WeldAdmin Pro - Poppler and Tesseract Checker
echo ==================================================
echo.

REM --- Locate .env (dist\.env, current folder, parent) ---
set "ENV_FILE="
if exist ".env" set "ENV_FILE=.env"
if not defined ENV_FILE if exist "dist\.env" set "ENV_FILE=dist\.env"
if not defined ENV_FILE if exist "..\.env" set "ENV_FILE=..\.env"

if defined ENV_FILE (
  echo Using environment file: %ENV_FILE%
) else (
  echo No .env file found next to this script.
  echo I will try common default locations...
)

REM --- Read paths from .env if present ---
set "TESSERACT_PATH="
set "POPPLER_PATH="
if defined ENV_FILE (
  for /f "usebackq tokens=1,* delims==" %%A in ("%ENV_FILE%") do (
    set "key=%%~A"
    set "val=%%~B"
    if /i "!key!"=="TESSERACT_PATH" set "TESSERACT_PATH=!val!"
    if /i "!key!"=="POPPLER_PATH" set "POPPLER_PATH=!val!"
  )
)

REM --- Apply sensible defaults if missing ---
if not defined TESSERACT_PATH set "TESSERACT_PATH=C:\Program Files\Tesseract-OCR\tesseract.exe"
if not defined POPPLER_PATH set "POPPLER_PATH=C:\tools\poppler\Library\bin"

echo.
echo Expected Tesseract: %TESSERACT_PATH%
echo Expected Poppler bin: %POPPLER_PATH%
echo.

REM --- Check Tesseract ---
set "TESS_OK=0"
if exist "%TESSERACT_PATH%" (
  "%TESSERACT_PATH%" --version > "%TEMP%\tess_ver.txt" 2>&1
  if errorlevel 1 (
    echo [X] Tesseract found but failed to run. See "%TEMP%\tess_ver.txt"
  ) else (
    echo [OK] Tesseract is installed and working:
    for /f "usebackq tokens=*" %%L in ("%TEMP%\tess_ver.txt") do (
      echo   %%L
      goto :after_tess_head
    )
    :after_tess_head
    set "TESS_OK=1"
  )
) else (
  echo [X] Tesseract not found at:
  echo     %TESSERACT_PATH%
  echo     Install Tesseract or correct TESSERACT_PATH in your .env
)

REM --- Check Poppler ---
set "POPP_OK=0"
if exist "%POPPLER_PATH%\pdftoppm.exe" (
  "%POPPLER_PATH%\pdftoppm.exe" -v > "%TEMP%\poppler_ver.txt" 2>&1
  if errorlevel 1 (
    echo [X] Poppler found but failed to run. See "%TEMP%\poppler_ver.txt"
  ) else (
    echo [OK] Poppler is installed and working:
    for /f "usebackq tokens=*" %%L in ("%TEMP%\poppler_ver.txt") do (
      echo   %%L
      goto :after_poppler_head
    )
    :after_poppler_head
    set "POPP_OK=1"
  )
) else (
  echo [X] Poppler not found at:
  echo     %POPPLER_PATH%
  echo     Ensure this folder contains pdftoppm.exe and pdftocairo.exe
  echo     or correct POPPLER_PATH in your .env
)

echo.
if "%TESS_OK%"=="1" if "%POPP_OK%"=="1" (
  echo ================================================
  echo   RESULT:  All good! OCR is ready to use.  ✅
  echo ================================================
  exit /b 0
) else (
  echo ================================================
  echo   RESULT:  Something is missing.  ❌
  echo   Fix the paths above, then run this again.
  echo ================================================
  exit /b 1
)
