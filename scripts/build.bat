@echo off
setlocal
echo ============================================================
echo   Gelesiye WPF (.NET 10) Derleme Araci
echo ============================================================
echo.

set "SCRIPT_DIR=%~dp0"
set "ROOT_DIR=%SCRIPT_DIR%..\"

echo [1/1] .NET 10 Cozumu Derleniyor (Release)...
dotnet build "%ROOT_DIR%GelesiyeWPF.sln" -c Release --nologo
if %errorlevel% neq 0 (
    echo.
    echo [HATA] Derleme basarisiz oldu! .NET 10 SDK yuklu oldugundan emin olunuz.
    exit /b %errorlevel%
)

echo.
echo ============================================================
echo [BASARILI] Gelesiye WPF basariyla derlendi!
echo Konum: bin\Release\net10.0-windows\GelesiyeWPF.exe
echo ============================================================
