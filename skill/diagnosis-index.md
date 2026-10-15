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
| 多机并行时提交混入其他工作 | 先检查 Git status 和 staged paths；按共享文件规则做内容级暂存并划定单写者 | 不要 `git add -A` 或 pull 脏工作区 | `../AGENTS.md`「双机协作」；`04_scripts/_common/stage_shared_file.py`（**逐行归属校验 `--own-pattern`**）；`archive_scoped_git.sh`（共享文件在暂存区时**强制逐行打印**，需 `ACK_SHARED=1` 放行） |
| 第三方反馈「装上后像旧版 / 打开就闪退」，而 `base.apk` 核对正常 | 先查**运行中的 so 是否等于包内 so**：`su -c md5sum /data/app/*/<pkg>-*/lib/arm64/libstarry_magic.so` vs 包内该 so md5；再看是否有 `files/starry_ota/<game_id>/` 旧补丁 so 顶替 | 不要先说「包坏了」或让玩家清数据（会丢热更资源） | `04_scripts/_common/verify_installed_apk.sh`（第二查）；`skill/starry-mod/SKILL.md`「清数据」节；`04_scripts/_common/so_hotswap.sh --restore-from` |
| 正式包夹带诊断日志/探针 | 查编译期诊断开关、obj 输出目录隔离、包内字符串及 release gate | 不要靠最后手工删日志代码 | `re-implement/hook-rules.md` §26；`04_scripts/_common/check_release_clean.sh` |
| 会话结束但源码、证据、产物与 Git 状态对不上 | 对照 pipeline 状态、交付 manifest、Git 范围和 NOT PROVEN 清单 | 不要手写 manifest 或只凭文档说已交付 | `04_scripts/_common/archive_close_git.sh`；`04_scripts/_common/delivery_manifest.py` |

## 判定纪律

1. 先写出症状与判定段（发生/未发生、命中/未命中、下游接受/拒绝），再选下一步；每一步说明它能区分哪些假设。
2. 报告中区分实测、历史记录、推断和 NOT PROVEN；缺少运行证据时不得写「功能通过」。
3. 同一故障反复出现时查经验卡和游戏摘要；新经验先按 `10_docs/_templates/经验候选卡.md` 登记为 candidate，未获批准不得改 skill。
4. 审计新建经验卡元数据：`python3 -B 04_scripts/_common/audit_experience_registry.py`。旧卡会标为 legacy，逐步迁移，不因历史格式导致全仓失败。

## 外部检索协议（2026-10-15 新增；与仓内先例检索配对）

> 触发：`bash 04_scripts/_common/prior_art.sh <关键词>` 报 **rc=1（仓内零命中）**；或同一功能**第 2 轮无效**（AGENTS.md「执行纪律补充」P4）。
> 落点：结果写 `10_docs/_state/外部方案台账.md`（候选 / 已评估·不接入 / 空结果 三区）。
> 依据（已查实）：境界刀鸣 2026-09-23「`web_search` 不可用」；霓虹深渊2 2026-10-11 会话 2393 次调用里 web_search=0 次、web_fetch 2 次全失败 ⇒ 该动作长期消失，全靠自研硬解。
> 文档维护口径：本协议只放通用机制；单次检索的具体结果落台账，不写回本文件。

### 通道与预算（2026-10-15 实测）

| 通道 | 实测 | 备注 |
|---|---|---|
| DuckDuckGo html 端点 | ✅ 返回真实结果 | 首选；支持 `site:` 限定中文论坛 |
| GitHub API / raw / shields | ✅ | 按星排序取星数 + 最后更新 |
| 看雪 `bbs.kanxue.com` | ✅ | `bbs.pediy.com` 已 301 迁此 |
| 吾爱 `52pojie.cn` / MT 论坛 `bbs.binmt.cc` | ✅ | 手游加固/脱壳实操帖 |
| `curl` 走 Bing | ❌ 返回无关结果（实测查 il2cpp 给"新加坡假日"） | 不用 |
| `web_search` 工具 | ❌ 本机常年不可用（2026-09-23 无 key；2026-10-15 额度不足） | **不依赖**（用 curl 直连上面几项） |

**预算：单次 ≤3 次调用，命中即止**；未命中的关键词进台账「空结果」栏，不再重查。

### 关键词模板（按优先级）

1. **报错原文加引号**（例：`"kIl2CppMetadataUsageInvalid"` 一次命中 Il2CppDumper issue #723/#872 + 看雪 2 帖）
2. `<壳名 / so 名> + <症状> + (脱壳|反调试|dump|bypass)`
3. `site:bbs.kanxue.com <关键词>` / `site:bbs.binmt.cc <关键词>`
4. 英文兜底：UnknownCheats / GameGuardian（手游壳类命中率意外地高）

### 按症状取关键词（与本文件上表同一批症状）

| 症状 | 外部关键词 |
|---|---|
| 功能无效、数值不变 | `<引擎> hook 不生效` / `<方法名> il2cpp hook` |
| 热更后功能失效 | `<游戏> 热更 指纹` / `HybridCLR 热更 失效` |
| RVA 命中数为零 | `<so 名> RVA 偏移 变了` / `il2cpp 导出 只剩 N 个` |
| 启动后静默退出 | `seccomp exit_group 反调试` / `保护线程 主动 exit` |
| 登录/签名校验失败 | `签名校验 过检 ApkSignatureKiller` / `PM 证书 弹窗` |
| 命中新的壳/加固产品 | `<壳名> 加固 脱壳 手法` / `<壳名> so 解密 dump` |
| 服务端拒绝、结算失败 | ⚠ 多为私服/服务端模拟 ⇒ 只记「已评估·不接入」，**不做候选** |
| 构建装机像旧包 / 正式包夹带日志 / 多机混提交 / 收官状态对不上 | 仓内机制问题，**不用外查** |

### 三条约束

1. **证据分级**：外部来源＝文档/自述级证据，一律先落「候选」，本仓实测通过才升「已合并」。
2. **越界红线**：私服 / 服务端模拟类 ⇒ 只记「已评估·不接入」（服务端写操作超授权边界）。
3. **采纳四问**（进框架前）：来源时效 / 痕迹面 / 适用边界 / 重叠度 —— 见 `re-implement/code-review-checklist.md`「外部方案专项」。
