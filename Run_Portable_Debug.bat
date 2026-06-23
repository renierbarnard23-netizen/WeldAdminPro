@echo off
setlocal enabledelayedexpansion
REM Ensure we run from the script folder
set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"

set EXE=dist\app.exe
set LOGDIR=%USERPROFILE%\Documents\WeldAdminProData
set LOGFILE=%LOGDIR%\weldadmin.log
set TEMPLOG=%TEMP%\weldadmin.log
set PORT=5000

echo === WeldAdmin Pro (Debug) ===
echo Checking files and folders...

if not exist "%LOGDIR%" (
  echo Creating data directory: "%LOGDIR%"
  mkdir "%LOGDIR%"
  if errorlevel 1 (
    echo ERROR: Could not create "%LOGDIR%". Try running as Administrator.
    pause
    exit /b 1
  )
)

if not exist "%EXE%" (
  echo ERROR: Portable EXE not found at "%CD%\%EXE%"
  echo -> Run Build_EXE.bat first, or use Start_WeldAdminPro.bat to run from source.
  pause
  exit /b 1
)

echo Logging to: "%LOGFILE%"
echo (If Documents logging is blocked, the app will fall back to: "%TEMPLOG%")
echo Port: %PORT%
echo Starting app.exe with console attached...
echo (If it closes, check the log file)
echo. > "%LOGFILE%"
del "%TEMPLOG%" 2>nul

REM Launch EXE in the background, redirecting to Documents log (the app may switch to TEMP internally)
set PORT=%PORT%
start "" /B "%EXE%" 1>>"%LOGFILE%" 2>&1

REM Wait for startup signature or error for up to ~30 seconds
set /a tries=0
:waitloop
timeout /t 1 >nul
set /a tries+=1

REM Detect readiness lines in either log
for %%F in ("%LOGFILE%" "%TEMPLOG%") do (
  if exist "%%~fF" (
    findstr /c:"Running on http://" "%%~fF" >nul && set READY=1
    findstr /c:"Starting server on http://127.0.0.1" "%%~fF" >nul && set READY=1
    findstr /c:"Serving Flask app" "%%~fF" >nul && set READY=1
    findstr /c:"Address already in use" "%%~fF" >nul && set PORTBUSY=1
  )
)

if defined PORTBUSY goto portbusy
if defined READY goto ready

if %tries% GEQ 30 goto notready
goto waitloop

:ready
echo Server is up (detected in log). Opening browser...
start "" http://127.0.0.1:%PORT%/
echo --- Live log tail (Ctrl+C to stop viewing; app keeps running) ---
if exist "%TEMPLOG%" (
  echo (Tailing TEMP log: %TEMPLOG%)
  powershell -NoProfile -Command "Get-Content -Path '%TEMPLOG%' -Wait -Tail 20"
) else (
  echo (Tailing Documents log: %LOGFILE%)
  powershell -NoProfile -Command "Get-Content -Path '%LOGFILE%' -Wait -Tail 20"
)
goto end

:portbusy
echo ERROR: Port %PORT% appears to be in use.
echo Try setting a different port, e.g. 5050. Press any key to retry on 5050...
pause >nul
set PORT=5050
set PORT=%PORT%
echo Retrying on port %PORT%...
echo. > "%LOGFILE%"
del "%TEMPLOG%" 2>nul
start "" /B "%EXE%" 1>>"%LOGFILE%" 2>&1
set READY=
set PORTBUSY=
set /a tries=0
goto waitloop

:notready
echo The app did not report readiness in time. Last 40 log lines:
if exist "%TEMPLOG%" (
  echo (TEMP log: %TEMPLOG%)
  powershell -NoProfile -Command "Get-Content -Path '%TEMPLOG%' -Tail 40"
) else (
  echo (Documents log: %LOGFILE%)
  powershell -NoProfile -Command "Get-Content -Path '%LOGFILE%' -Tail 40"
)
echo You can still try opening http://127.0.0.1:%PORT%/
pause
goto end

:end
pause
