@echo off
title WeldAdmin Pro - Environment Setup
echo ============================================================
echo   WELDADMIN PRO - OCR MODULE SETUP (Windows)
echo ============================================================
echo.
echo Installing required Python libraries...
pip install pillow pytesseract pdfplumber pdf2image python-dotenv
echo.
echo ============================================================
echo   INSTALLATION COMPLETE
echo.
echo   NEXT STEPS:
echo   1. Install Tesseract OCR:
echo      https://github.com/UB-Mannheim/tesseract/wiki
echo.
echo   2. Install Poppler for Windows:
echo      https://github.com/oschwartz10612/poppler-windows/releases/
echo.
echo   3. Create a .env file in your WeldAdminPro folder with:
echo      TESSERACT_PATH=C:\Program Files\Tesseract-OCR\tesseract.exe
echo      POPPLER_PATH=C:\Program Files\poppler-24.08.0\bin
echo.
echo ============================================================
echo   All done! You can now run WeldAdmin Pro OCR modules.
echo ============================================================
pause
