@echo off
rem ============================================================
rem  nixiang 双机体检 - 双击运行（Windows 端）
rem
rem  这个文件只做一件事：找到 Git Bash，把 twin_status.sh 交给它执行。
rem  体检逻辑全部在 .sh 里，cmd 不参与解释该脚本。
rem  退出码：0 全绿 / 1 有 FAIL / 2 仅 WARN
rem ============================================================
chcp 65001 >nul
setlocal
set "BASH=C:\Program Files\Git\bin\bash.exe"
set "HERE=%~dp0"
if not exist "%BASH%" (
  echo [FAIL] 没找到 Git Bash: "%BASH%"
  echo        请先装 Git for Windows，或改本文件里的 BASH 路径。
  echo.
  pause
  exit /b 1
)
cd /d "%HERE%..\.."
echo 仓库根: %CD%
echo.
"%BASH%" -l "%HERE%twin_status.sh"
set "RC=%ERRORLEVEL%"
echo.
echo ==================================================
if "%RC%"=="0" echo [OK]   全绿，双机环境没问题
if "%RC%"=="1" echo [FAIL] 有失败项，往上翻 [FAIL] 行
if "%RC%"=="2" echo [WARN] 没有失败项，有警告项
echo 退出码 %RC%
echo ==================================================
if not "%NX_NOPAUSE%"=="1" pause
exit /b %RC%
