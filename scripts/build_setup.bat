@echo off
setlocal
echo ============================================================
echo   Gelesiye WPF Inno Setup x64 Kurulum Paketi Derleme Araci
echo ============================================================
echo.

set "SCRIPT_DIR=%~dp0"
set "ROOT_DIR=%SCRIPT_DIR%..\"
set "ISCC_EXE=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if not exist "%ISCC_EXE%" (
    echo [HATA] Inno Setup 6 derleyicisi bulunamadi: "%ISCC_EXE%"
    echo Lutfen Inno Setup 6 yukleyiniz.
    exit /b 1
)

if not exist "%ROOT_DIR%releases" mkdir "%ROOT_DIR%releases"

echo [1/2] Release Kurulum Paketi Olusturuluyor...
"%ISCC_EXE%" /DSourceDir="..\bin\Release\net10.0-windows" /DOutputBaseFilename="Gelesiye_WPF_Setup_x64" "%ROOT_DIR%installer\Gelesiye_WPF_Setup.iss"
if %errorlevel% neq 0 (
    echo [HATA] Release kurulum paketi derlemesi basarisiz oldu!
    exit /b %errorlevel%
)

echo [2/2] Debug Kurulum Paketi Olusturuluyor...
"%ISCC_EXE%" /DSourceDir="..\bin\Debug\net10.0-windows" /DOutputBaseFilename="Gelesiye_WPF_Setup_Debug_x64" /DAppVerSuffix=" (Debug)" "%ROOT_DIR%installer\Gelesiye_WPF_Setup.iss"
if %errorlevel% neq 0 (
    echo [HATA] Debug kurulum paketi derlemesi basarisiz oldu!
    exit /b %errorlevel%
)

echo.
echo ============================================================
echo [BASARILI] Gelesiye WPF Kurulum Paketleri basariyla olusturuldu!
echo Konum: releases\Gelesiye_WPF_Setup_x64.exe
echo Konum: releases\Gelesiye_WPF_Setup_Debug_x64.exe
echo ============================================================
