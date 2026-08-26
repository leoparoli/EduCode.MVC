@echo off
setlocal
chcp 65001 >nul
cd /d "%~dp0"
title EduCode Web MVC 3.1.2

echo ================================================
echo            EDUCODE WEB MVC 3.1.2
echo ================================================
echo Web: https://localhost:7180
echo API esperada: http://localhost:5168
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: No se encontro el SDK de .NET en el PATH.
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "try { $r = Invoke-WebRequest -UseBasicParsing -Uri 'http://localhost:5168/api/estado' -TimeoutSec 2; if ($r.StatusCode -eq 200) { exit 0 } } catch { }; exit 1"
if errorlevel 1 (
    echo ERROR: EduCode API no esta disponible en http://localhost:5168.
    echo Ejecuta ..\INICIAR_EDUCODE.bat para iniciar API y Web en el orden correcto.
    pause
    exit /b 2
)

dotnet restore EduCode.WebMVC.sln
if errorlevel 1 goto ERROR_WEB

dotnet run --project ".\src\EduCode.WebMVC\EduCode.WebMVC.csproj" --launch-profile https
if errorlevel 1 goto ERROR_WEB

exit /b 0

:ERROR_WEB
echo.
echo ERROR: EduCode Web MVC no pudo iniciarse.
pause
exit /b 1
