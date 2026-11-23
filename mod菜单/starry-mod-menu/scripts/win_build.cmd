@echo off
setlocal enabledelayedexpansion
rem ============================================================
rem  Windows build helper for starry-mod-menu (verified 2026-11-23)
rem  File: mod<cjk>/starry-mod-menu/scripts/win_build.cmd
rem  Usage: win_build.cmd [gradle-task] [extra gradle args...]
rem         win_build.cmd
rem         win_build.cmd assembleDebug
rem         win_build.cmd assembleRelease --offline
rem  It: probes SDK/JDK -> writes local.properties -> builds -> restores.
rem  Docs: docs/win_build_20261123.md
rem  Keep this file ASCII-only: cmd.exe parses the file in the OEM
rem  codepage, so UTF-8 Chinese comments break parsing on zh-CN Windows.
rem ============================================================
set "ROOT=%~dp0.."
set "LP=%ROOT%\local.properties"
set "TASK=%~1"
if "%TASK%"=="" set "TASK=assembleRelease"
if not "%~1"=="" shift
set "EXTRA="
:args
if "%~1"=="" goto args_done
set "EXTRA=%EXTRA% %~1"
shift
goto args
:args_done

rem ---- 1) Android SDK ----
set "SDK="
if defined ANDROID_HOME if exist "%ANDROID_HOME%\platforms" set "SDK=%ANDROID_HOME%"
if not defined SDK if defined ANDROID_SDK_ROOT if exist "%ANDROID_SDK_ROOT%\platforms" set "SDK=%ANDROID_SDK_ROOT%"
if not defined SDK if exist "%LOCALAPPDATA%\Android\Sdk\platforms" set "SDK=%LOCALAPPDATA%\Android\Sdk"
if not defined SDK if exist "%USERPROFILE%\AppData\Local\Android\Sdk\platforms" set "SDK=%USERPROFILE%\AppData\Local\Android\Sdk"
if not defined SDK if exist "%HOMEDRIVE%%HOMEPATH%\AppData\Local\Android\Sdk\platforms" set "SDK=%HOMEDRIVE%%HOMEPATH%\AppData\Local\Android\Sdk"
if not defined SDK if exist "E:\nixiang\01_tools\android-sdk\platforms" set "SDK=E:\nixiang\01_tools\android-sdk"
if not defined SDK (
  echo [win_build] FAIL: Android SDK not found. Set ANDROID_HOME to a dir containing platforms\.
  echo [win_build] checked: ANDROID_HOME, ANDROID_SDK_ROOT, %LOCALAPPDATA%\Android\Sdk
  exit /b 2
)
echo [win_build] SDK = %SDK%

rem ---- 2) JDK 17 ----
set "JDK="
if defined JAVA_HOME if exist "%JAVA_HOME%\bin\java.exe" set "JDK=%JAVA_HOME%"
if not defined JDK for /d %%D in ("C:\Program Files\Microsoft\jdk-17*") do if exist "%%D\bin\java.exe" set "JDK=%%D"
if not defined JDK for /d %%D in ("C:\Program Files\Java\jdk-17*") do if exist "%%D\bin\java.exe" set "JDK=%%D"
if not defined JDK for /d %%D in ("C:\Program Files\Eclipse Adoptium\jdk-17*") do if exist "%%D\bin\java.exe" set "JDK=%%D"
if not defined JDK if exist "C:\Program Files\Android\Android Studio\jbr\bin\java.exe" set "JDK=C:\Program Files\Android\Android Studio\jbr"
if not defined JDK (
  echo [win_build] FAIL: JDK 17 not found. Set JAVA_HOME to a JDK 17 home.
  exit /b 3
)
set "JAVA_HOME=%JDK%"
echo [win_build] JAVA_HOME = %JAVA_HOME%

rem ---- 3) local.properties: repo copy holds a macOS path, swap it in ----
set "LPBAK=%TEMP%\starry_local_%RANDOM%.properties"
if exist "%LP%" copy /y "%LP%" "%LPBAK%" >nul
set "SDK_FWD=%SDK:\=/%"
> "%LP%" echo sdk.dir=%SDK_FWD%

rem ---- 4) build ----
pushd "%ROOT%"
call gradlew.bat %TASK% %EXTRA%
set "RC=%ERRORLEVEL%"
popd

rem ---- 5) restore ----
if exist "%LPBAK%" (
  copy /y "%LPBAK%" "%LP%" >nul
  del /q "%LPBAK%"
)
if not "%RC%"=="0" (
  echo [win_build] FAIL: gradle exit=%RC%
  exit /b %RC%
)

set "APK=%ROOT%\app\build\outputs\apk\release\app-release.apk"
if exist "%APK%" (
  echo [win_build] APK = %APK%
  for %%A in ("%APK%") do echo [win_build] size = %%~zA bytes
) else (
  echo [win_build] WARN: no release apk at expected path
)
echo [win_build] DONE. Inject into a game via that game's surgical_*.sh, do not install this apk directly.
exit /b 0
