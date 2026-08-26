@echo off
setlocal
cd /d "%~dp0"

echo Verificando archivos duplicados...
if exist ".\src\EduCode.WebMVC\ApiClientInfrastructure.cs" (
    echo Eliminando ApiClientInfrastructure.cs de una version anterior...
    del /f /q ".\src\EduCode.WebMVC\ApiClientInfrastructure.cs"
)

if not exist ".\src\EduCode.WebMVC\Services\ApiResult.cs" (
    echo ERROR: Falta Services\ApiResult.cs
    exit /b 1
)
if not exist ".\src\EduCode.WebMVC\Services\IEduCodeApiClient.cs" (
    echo ERROR: Falta Services\IEduCodeApiClient.cs
    exit /b 1
)
if not exist ".\src\EduCode.WebMVC\Services\EduCodeApiClient.cs" (
    echo ERROR: Falta Services\EduCodeApiClient.cs
    exit /b 1
)

echo Limpiando compilaciones anteriores...
dotnet clean
if errorlevel 1 exit /b 1

for /d /r %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"

echo Restaurando paquetes...
dotnet restore
if errorlevel 1 exit /b 1

echo Compilando Web MVC...
dotnet build
if errorlevel 1 exit /b 1

echo.
echo Compilacion MVC completada correctamente.
endlocal
