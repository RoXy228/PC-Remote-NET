@echo off
setlocal

REM ======================================
REM PATHS
REM ======================================

set ROOT=C:\Users\tuktu\source\repos\PCRemote
set INSTALLER=%ROOT%\installer
set PUBLISH=%INSTALLER%\publish
set OUTPUT=%INSTALLER%\output

REM ======================================
REM CERTIFICATE
REM ======================================

set CERT=C:\cert\PCRemote.pfx
set CERTPASS=1926

REM ======================================
REM TOOLS
REM ======================================

set SIGNTOOL="C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

REM ======================================
REM CLEAN
REM ======================================

echo ======================
echo CLEAN
echo ======================

if exist "%PUBLISH%" (
    del /s /q "%PUBLISH%\*" 2>nul
    for /d %%i in ("%PUBLISH%\*") do rmdir /s /q "%%i"
)

if exist "%OUTPUT%" (
    del /s /q "%OUTPUT%\*" 2>nul
    for /d %%i in ("%OUTPUT%\*") do rmdir /s /q "%%i"
)

REM ======================================
REM BUILD UI
REM ======================================

echo ======================
echo BUILD UI
echo ======================

dotnet publish "%ROOT%\PCRemote.WPF\PCRemote.WPF.csproj" ^
 -c Release ^
 -r win-x64 ^
 --self-contained false ^
 -o "%PUBLISH%" ^
 /p:PublishSingleFile=true ^
 /p:IncludeNativeLibrariesForSelfExtract=true

REM ======================================
REM BUILD SERVICE
REM ======================================

echo ======================
echo BUILD SERVICE
echo ======================

dotnet publish "%ROOT%\PCRemote.Service\PCRemote.Service.csproj" ^
 -c Release ^
 -r win-x64 ^
 --self-contained false ^
 -o "%PUBLISH%" ^
 /p:PublishSingleFile=true ^
 /p:IncludeNativeLibrariesForSelfExtract=true

REM ======================================
REM REMOVE DEBUG FILES
REM ======================================

echo ======================
echo CLEAN DEBUG FILES
echo ======================

del "%PUBLISH%\*.pdb" /q 2>nul
del "%PUBLISH%\appsettings*.json" /q 2>nul

REM ======================================
REM SIGN EXE FILES
REM ======================================

echo ======================
echo SIGN EXE
echo ======================

%SIGNTOOL% sign ^
 /f "%CERT%" ^
 /p %CERTPASS% ^
 /fd SHA256 ^
 /tr http://timestamp.digicert.com ^
 /td SHA256 ^
 "%PUBLISH%\PCRemote.exe"

%SIGNTOOL% sign ^
 /f "%CERT%" ^
 /p %CERTPASS% ^
 /fd SHA256 ^
 /tr http://timestamp.digicert.com ^
 /td SHA256 ^
 "%PUBLISH%\PCRemote.Service.exe"

REM ======================================
REM BUILD INSTALLER
REM ======================================

echo ======================
echo BUILD INSTALLER
echo ======================

%ISCC% "%INSTALLER%\PCRemote.iss"

REM ======================================
REM SIGN INSTALLER
REM ======================================

echo ======================
echo SIGN INSTALLER
echo ======================

%SIGNTOOL% sign ^
 /f "%CERT%" ^
 /p %CERTPASS% ^
 /fd SHA256 ^
 /tr http://timestamp.digicert.com ^
 /td SHA256 ^
 "%OUTPUT%\PCRemoteSetup.exe"

echo ======================
echo DONE
echo ======================

pause