# git-hooks/

本目录是 `core.hooksPath` 指向的钩子目录（两台机器各配一次：`git config core.hooksPath 04_scripts/_common/git-hooks`）。

## 文件

| 文件 | 谁执行 | 说明 |
|---|---|---|
| `pre-push` | mac：git 直接执行 | 本次 push 涉及 `origin/<branch>..HEAD` 的 `*.sh` ⇒ 过 `04_scripts/_common/preflight.sh`，FAIL 即阻断（`--no-verify` 仅应急）。shebang 保持 `#!/usr/bin/env bash`。 |
| `pre-push.cmd` | 人工 / CI（**git 不会自动执行**） | Windows 包装：CRLF + `chcp 65001`，`%~dp0` 取自身目录，`exit /b %ERRORLEVEL%` 透传退出码。stdin 需给 git 格式的 ref 行。 |

## Windows 侧为什么还要 push_main 内联（2026-11-25）

Win 上 `git push` 触发本目录 `pre-push` 曾整轮报 `env: failed to run command '04_scripts/_common/git-hooks/pre-push': Permission denied`（ssh22/ssh443/https 三通道都同一句）——钩子能否被 git 直启取决于本机 exec 位/NTFS ACL 与相对 `core.hooksPath` 的解析，不是脚本内容问题。因此 Windows 侧的实际门禁是 `04_scripts/_common/git_push_main.sh` 里的 `_pg_preflight_gate`。

**两处判据必须一致**：同样取 `origin/<branch>..HEAD` 的 `*.sh`（排除本次删除）、同样交给 `preflight.sh`。改一处必须同步另一处。
