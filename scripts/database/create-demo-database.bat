@echo off
setlocal

set "ROOT=%~dp0..\.."
set "PROJECT=%ROOT%\demo\APO-BOT.DemoApi\APO-BOT.DemoApi.csproj"
set "DATABASE=%ROOT%\demo\APO-BOT.DemoApi\Data\apobot-demo.db"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: No se ha encontrado el SDK de .NET en PATH.
    exit /b 1
)

echo Creando la base de datos de demostracion de APObot...
del /q "%DATABASE%" "%DATABASE%-shm" "%DATABASE%-wal" 2>nul

pushd "%ROOT%"
dotnet run --project "%PROJECT%" -- --create-database
set "RESULT=%ERRORLEVEL%"
popd

if not "%RESULT%"=="0" (
    echo ERROR: No se ha podido crear la base de datos.
    exit /b %RESULT%
)

echo Base de datos creada correctamente:
echo %DATABASE%
exit /b 0
