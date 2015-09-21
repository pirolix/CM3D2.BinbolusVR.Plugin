@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\..\scripts\base.bat" || exit /b 1

set TYPE=/t:library
set OUT=%UNITY_INJECTOR_DIR%\CM3D2.BinbolusVR.Plugin.dll
set SRCS="CM3D2.BinbolusVR.Plugin.cs"
set OPTS=/r:ExIni.dll

call "%~dp0..\..\scripts\csc-compile.bat" || exit /b 1
popd

REM PAUSE
