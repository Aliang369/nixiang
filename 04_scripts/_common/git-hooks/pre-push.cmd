@echo off
rem ================================================================
rem  pre-push.cmd — Windows 侧 pre-push 门禁的**人工入口 / CI 用**包装
rem  （2026-11-25；CRLF + chcp 65001，Windows 才能正确显示中文）
rem
rem  事实（必须先读，否则会误以为它会被 git 自动调用）：
rem    git 在 Windows 上不会自动执行 .cmd 钩子。core.hooksPath 里 git 认的是
rem    无扩展名的 `pre-push`（经 shebang 交给 bash）；`.cmd` 只有在钩子名恰好
rem    是 pre-push.cmd 时才可能被 git 找到，而本仓的钩子名是 pre-push。
rem    所以本文件**不是**主门禁，实际门禁由 git_push_main.sh 内联的
rem    _pg_preflight_gate 调用（跨平台一致，见该文件 v2.3 注释）。
rem
rem  两处判据必须一致：取 origin/<branch>..HEAD 范围内改动的 *.sh（排除本次删除），
rem  交给 04_scripts/_common/preflight.sh。改这里必须同步 git_push_main.sh，反之亦然。
rem
rem  人工用法（stdin 要给 git 格式的 ref 行，否则钩子读不到待推范围）：
rem    echo refs/heads/main LOCAL_SHA refs/heads/main REMOTE_SHA
rem      | 04_scripts\_common\git-hooks\pre-push.cmd origin git@github.com:Aliang369/nixiang.git
rem    （先试跑只看不改：git rev-parse HEAD / git rev-parse origin/main）
rem ================================================================
chcp 65001>nul
setlocal
set "HERE=%~dp0"
rem %~dp0 以反斜杠结尾；转成正斜杠再交给 bash（MSYS bash 认 E:/a/b，不认 E:\a\b 更稳）
set "HERE_POSIX=%HERE:\=/%"
set "BASHPF=%HERE_POSIX%pre-push"
if not exist "%HERE%pre-push" (
  echo [pre-push.cmd] 找不到 %HERE%pre-push
  exit /b 9
)
bash "%BASHPF%" %*
exit /b %ERRORLEVEL%
