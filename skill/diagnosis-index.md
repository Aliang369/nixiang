# 按症状反查诊断入口

> 用途：先按可观察症状选择最小诊断路径，再进入专项技能；不替代 `AGENTS.md` 的授权、双机协作和安全边界。证据不足时保留 NOT PROVEN，不把可能原因写成事实。

| 症状 / 信号 | 第一判定与最小证据 | 不要先做 | 入口 |
|---|---|---|---|
| 功能无效、数值不变 | 先查自检行、hook 命中计数、入参/返回值、偏移与写入结果；区分「未触发、未命中、命中但下游不接」 | 不要先改业务逻辑或猜偏移 | `re-implement/hook-rules.md` §29；`10_docs/_templates/经验_功能不对先Trace证实挂点偏移.md` |
| 热更后功能失效 | 三查资源版本、核心 so/dll 指纹、业务载体是否在热更目录；先重建当前基线 | 不要先重编 native 或沿用旧锚点 | `re-extract/hotupdate-fingerprint-recheck.md` |
| 构建/装机成功但行为像旧包 | 比对本地产物与设备 APK/DEX/SO 指纹，确认安装包身份 | 不要把 adb install 返回成功当成安装正确 | `04_scripts/_common/verify_installed_apk.sh`；`04_scripts/_common/stages.md` |
| RVA 命中数为零或悄然回退按名 | 确认 so/build ID、RVA/file offset 口径、小端序言与运行日志实际命中路径 | 不要把功能成功或构建成功当作 RVA 路径证据 | `re-implement/version-update-relocate.md`；`re-implement/hook-rules.md` §6 |
| 启动后静默退出、无 Java 崩溃栈 | 区分主动 exit/保护线程退出与崩溃；检查系统 crash/dropbox 和探针自检结果 | 不要盲目 patch exit/kill 或把「干净退出」当成功 | `re-implement/anti-cheat-pattern.md`；`re-analyze/native-probe-on-device.md` |
| 服务端拒绝、结算失败或收益回滚 | 先查服务端是否独立重放战斗/结算输入，明确客户端与服务端各自证据 | 不要伪造胜利、跳过上报或把客户端显示当服务端接受 | `re-analyze/login/server-rejection-triage.md`；`re-implement/anti-cheat-pattern.md` |
| 登录/签名校验失败 | 确认渠道 SDK、包名、证书来源及校验发生在 Application/provider 哪个时序 | 不要先全局替换签名或修改证书表 | `re-analyze/login/sign-spoof.md`；`re-analyze/login/taptap-web-login.md` |
| 多机并行时提交混入其他工作 | 先检查 Git status 和 staged paths；按共享文件规则做内容级暂存并划定单写者 | 不要 `git add -A` 或 pull 脏工作区 | `../AGENTS.md`「双机协作」；`04_scripts/_common/stage_shared_file.py`；`archive_scoped_git.sh` |
| 正式包夹带诊断日志/探针 | 查编译期诊断开关、obj 输出目录隔离、包内字符串及 release gate | 不要靠最后手工删日志代码 | `re-implement/hook-rules.md` §26；`04_scripts/_common/check_release_clean.sh` |
| 会话结束但源码、证据、产物与 Git 状态对不上 | 对照 pipeline 状态、交付 manifest、Git 范围和 NOT PROVEN 清单 | 不要手写 manifest 或只凭文档说已交付 | `04_scripts/_common/archive_close_git.sh`；`04_scripts/_common/delivery_manifest.py` |

## 判定纪律

1. 先写出症状与判定段（发生/未发生、命中/未命中、下游接受/拒绝），再选下一步；每一步说明它能区分哪些假设。
2. 报告中区分实测、历史记录、推断和 NOT PROVEN；缺少运行证据时不得写「功能通过」。
3. 同一故障反复出现时查经验卡和游戏摘要；新经验先按 `10_docs/_templates/经验候选卡.md` 登记为 candidate，未获批准不得改 skill。
4. 审计新建经验卡元数据：`python3 -B 04_scripts/_common/audit_experience_registry.py`。旧卡会标为 legacy，逐步迁移，不因历史格式导致全仓失败。
