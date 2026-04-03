@echo off
setlocal
echo === WeldAdmin Pro Environment Check ===
echo Current folder: %CD%
echo User profile: %USERPROFILE%
echo Documents path: %USERPROFILE%\Documents
echo Data dir expected: %USERPROFILE%\Documents\WeldAdminProData
echo Python available?
where python
echo PyInstaller available?
where pyinstaller
echo dist\app.exe present?
if exist dist\app.exe (echo YES) else (echo NO)
echo.
pause
