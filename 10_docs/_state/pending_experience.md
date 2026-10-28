# 经验生命周期元数据

本文件维护旧经验清单与审批说明；新经验卡的机器可审计状态以经验卡 frontmatter 为准。旧条目暂不批量迁移，审计器将其报告为 `legacy`，不阻断检查。

**状态与门槛的唯一正文＝`10_docs/_templates/经验候选卡.md`（§0 状态口径、§4 合并门槛）**；本文件只做登记，不改定义。

状态含义（与模板 §0 一致）：

- `candidate`（候选）：证据或落点还不齐。
- `approved`（待验证／已自批待合并）：证据齐＋落点明确，已按自动门槛自批，尚未写入正文；仍不得当规则执行。
- `merged`（已合并）：已经落入目标正文，必须记录实际目标位置与合并 commit。
- `rejected`（已否定）：有充分反证不采纳，保留否定依据。
- `stale`（已过时）：适用条件变化或被新证据推翻，保留历史与替代关系。

**维度补充（2026-10-27）**：卡上新增 `class: tech|policy`。`tech` 满足门槛可自动合并；`policy`（改用户长期要求／项目政策／授权边界／门禁强度）**永不自动合并**，只能提案等用户当轮指令。本文件下文历史条目若属政策类，一律按"已批准"仅表示证据齐，不等于政策已改。

2026-10-04 起审批语义改为证据门槛（见 `AGENTS.md` §7「规范/技能优化会话全自动」）；下文历史条目原文不改，其中的「待用户批准」按新门槛重新判定即可。

新建经验卡请使用 `10_docs/_templates/经验候选卡.md` 的 YAML frontmatter，并运行：

```bash
python3 -B 04_scripts/_common/audit_experience_registry.py
```

本工具只审计 `10_docs/_templates/经验*.md`，不自行审批经验、不改写卡片、不迁移历史内容。

## 现有候选/已合并记录

- [证书嵌入 so 经验](../_templates/经验_证书嵌入so_20260906.md)：**已批准并合并**（2026-09-06）——证书固定改 so 烘焙（RegisterNatives 19/19），自测壳实测 200；已写入 SKILL.md 分发自检、surgical-build.md、auth-chain.md。跨样本 3 项 NOT PROVEN 随首个新游戏外科包回归，存量包/旧脚本不追溯。
- 超维进化卡死、追迹迷城偶发闪退、无限升级运行与授权、旁路小屏验证：保留在各摘要，不自动恢复执行或操作设备。
- [追迹迷城经验候选](games/追迹迷城.md#待审批经验索引)：classes7 自动发现失效与 classes4 合并、MenuService2 等长槽；待用户批准写入 skill，不能视为通用定律。
- [终末地已处理经验](games/终末地.md#经验索引)：历史记载已获批沉淀及补战报；本次未修改 skill。
- [dump.cs 字段口径与序言校验（我的勇者 7.6.1）](../_templates/经验_dump偏移口径与序言校验_20260920.md)：**已合并**（提交 `e83da4f`，2026-09-20 用户明确授权）——`FileOffToVa` 收 file_off（dump.cs 的 `Offset`，本例 RVA−0x4000，常数须按 PT_LOAD 实测）；序言期望值按小端读成 u32；填错会静默退回按名，唯一判据是 logcat 的 `@rva`/`@by-name`。落地于 `skill/re-implement/version-update-relocate.md` §0.1 + `hook-rules.md` §6。
- [收官工具链四处摩擦（2026-09-20）](../_templates/经验_收官工具链摩擦_20260920.md)：**已合并**（提交 `8d04780`，2026-09-20 用户「1234都改」）——落地于 `skill/starry-mod/SKILL.md`、`surgical-build.md`、`stages.md`、`pre_install_gate.sh` 和 `AGENTS.md`。
- [splice 离线闭环 + early hook 超时（超维进化 0.3.5，2026-09-21）](../_templates/经验_splice离线闭环与earlyhook_20260921.md)：**候选，未审批**——splice 型改动的离线镜像需抽真源码、主机编译并逐字节比 delta；early hook timeout 的含义待适用条件验证。拟落 `hook-rules.md` + `starry-mod/SKILL.md`。
- [热更指纹漂移与令牌浪费（勇士之心 1.0.3）](../_templates/经验_热更指纹漂移_20260921.md)：**已合并**（2026-09-21 用户明确授权）——已落入 `hook-rules.md` §24、`re-extract/hotupdate-fingerprint-recheck.md`、`AGENTS.md` 和 `stages.md`。
- 新实验使用 [逆向实验记录](../_templates/逆向实验记录.md)；新经验使用 [经验候选卡](../_templates/经验候选卡.md)，证据与落点齐备前保持候选状态。
- [反作弊压制 + 上报拦截的分层落点（消消英雄2 1.2.28，2026-10-09）](games/消消英雄2.md)：**已批准，待合并**（证据齐＋落点明确）——
  ①「检测到战斗作弊」这类**本地判定不是上报**，压判定（恒假/空实现）不会在服端留缺报痕迹；要处理的是喂给服端的 **BI**
  （本例 `Cheated/RealDamagePerRound/PredictedDamagePerRound` 全仓只被 `SideQuestApi._BIEndQuest` 写、只经 `BIRequest.EndQuest` 出设备 ⇒ 十倍伤害必被判 Cheated，但只走 BI）。
  ②上报拦截必须分层：第三方 SDK（Mob/cn.fly、Bugly、TapDB、运营商一键登录）→ **native DNS 黑名单**（`getaddrinfo` / `android_getaddrinfofornet` / `gethostbyname[2]` 四入口，`__attribute__((constructor))` 即装，
  早于 `Application.onCreate` 里的 SDK init）；游戏自家域名上的 BI 埋点 → **只能在 Lua 漏斗层拦**（BI 与登录/进度同域，DNS 拦会连坐）。
  ③从 Lua 拦必须同时处理三处：`BIRequest` 8 出口、`BINetworkManager.CustomSend`（唯一真发送器，`Send` 只转发、`CachedSend` 只写缓存）、以及 C#→Lua 入口 `CS.LuaManager.Instance.LuaBISend` **重指**（不重指等于没拦）。
  ④包装必须**逐表幂等 + 周期性重跑**（游戏热重载会换掉表），计数写全局诊断表并用一行日志报「哪几面装上了」，否则「拦了没有」不可观测。
  ⑤通用弹窗兜底只按「窗口名 + 文案」收窄（本例 `ForcedWindow`＝通用多选窗 `multi_choice_window`，无条件拦会误伤正常多选弹窗）。
  ⑥**口径＝「让数据伪装成正常的」**（用户 m00665/m00796）：能照发就**不改函数**——`WrapForward` 记录→清洗→原样转发（表走**深拷贝副本**，调用方表零改动），
  缺一条上报本身就是特征；硬拦只留逃生口 `_G.__starry_bi_hard`。需要改值的三类：聚合伤害字段（压到预测值×1.8 内）、同比例明细伤害、
  作弊/环境类布尔键（`Cheated`/`IsCheat`/`IsRoot`/`IsEmulator`/`IsFrida`…）置 false；**请求头里的环境标识同样要改**（本例 `Microfun-Client` 头 12 段中一段 `root/jailbroken/hacked` → `stock`）。
  ⑦**别按方法名拦业务存档**：同一 `BLSaveCustomData` 有 9 个 Lua 调用者，只有 1 个是反作弊 `anticheat`，其余 8 个是业务存档（`relic_draw`/`batch_purchase`/`kings_hard`/首充…）⇒ 只能按 key 过滤。
  ⑧**DNS 黑名单必须含 SDK 的「配置下发域名」**，否则长连接数据面纯 IP 绕过（本例 `tgc.zztfly.com/tcp/config/init` ⇒ 后缀必含 `.zztfly.com`）。
  ⑨**异步回调型反作弊方法不能替换成裸 `return`**：`CheckPlay(self,levelData,cb)` 的回调是上位机流程的必经环节，必须照它自己的通过分支回调（本例 `pcall(cb,false)`），否则**打完不结算卡死**（用户 m00726 报障）。
  落点拟：`skill/re-implement/hook-rules.md` 新节「反作弊压制与上报拦截分层」＋ `skill/starry-mod/SKILL.md` 出包自检清单（DNS 4 入口、黑名单后缀必含 SDK 的**长连接配置域名**，否则纯 IP 长连接绕过）。
  证据＝`10_docs/消消英雄2/过检测与上报拦截_1.2.28.md` ＋ `09_test/消消英雄2/precheck_20261009_005259/logcat.txt`（实测 `DNS BLOCK taptap-ad-callback.service.alina-game.cn / android.bugly.qq.com / openlog.tapapis.cn / e.tapdb.net`、`[StarryACG] BI清洗 BIRequest.EndQuest 聚合=1 布尔=1 键=Cheated=1`、`指纹头=true`，且 `tapsdk.tapapis.cn`、`*.alina-game.cn` 正常放行）。
- [emutls 静默降级：so 带 UND `__emutls_get_address` → 模拟器/真机 dlopen 失败，菜单静默降级 Java 版（绝区零 3.2.0，2026-10-05）](games/绝区零.md)：**已批准，待合并**（证据齐＋落点明确，自动门槛）——Android clang 默认 `-femulated-tls`，任何 TU 用 `thread_local`（本仓 30+ 个 `*Mod.cpp` 都在用）就会生成这条 UND；而它只有 **NDK 版 libc++_shared.so** 弱定义，游戏自带的旧 libc++_shared、MuMu 的 arm64 垫片 libc/libc++、真机 bionic libc **都没有**。失败时的坑＝菜单照弹（Java 降级 `native menu shown mode=ACTIVITY`）但 `ModBridge … Unavailable`、hook 全哑，极易误诊成“功能没实现”。修法＝mk 加 `-fno-emulated-tls` ＋ `-Wl,--exclude-libs,ALL`（后者防 libChiy 式静态库符号外露）；一行自检 `llvm-nm -D --undefined-only <so> | grep emutls` 必须为空。**模板已直接落地** `mod菜单/starry-mod-menu/app/src/main/jni/Android_modern_template.mk`；待并入 `skill/re-implement/hook-rules.md`（新节「so 装载硬门禁」）＋ `skill/starry-mod/SKILL.md` 出包自检清单。证据＝`10_docs/绝区零/出包记录_20261005.md` L2 节。
  对存量游戏：旧 `libs_*/` so 可用同一行命令扫；命中即在下次重编时补上这两个标志（存量包不追改）。
- [MuMu 早期假映射下的 VA 口径与装钩就绪门（绝区零 3.2.0，2026-10-06；原「转译缓存」条已更正）](games/绝区零.md)：**已批准，待合并**（证据齐＋落点明确）——
  ⚠️ 原根因叙述「转译缓存不回读 guest 字节」**已被 2026-10-06 凌晨第三轮实测否定**（反证：同一函数 Dmg@0x40002f62a9f0
  在激战中晚装钩——进程起 5 分钟后、函数热跑时——仍 30ms 内命中 10 条）；真因＝**VA 口径选错（假映射）**：
  MuMu 启动早期 /proc/self/maps 会出现游戏**校验/预读 .so 的同名临时分块映射**（0x77xx 宿主堆区，
  随后被回收/复用；本会话实测那些 VA 后来属于 Chrome 库或已 unmapped），isLibraryLoaded 名字命中＝假就绪；
  桩全落假副本 ⇒ 功能全哑。修法（已入包，L2 实测四条 `src=phdr`）：就绪门用 `dl_iterate_phdr` 真模块＋目标站
  序言交叉校验；VA＝`base + p_vaddr + (fileOff-p_offset)`（PT_LOAD 口径，maps 仅回退）；再加每 Tick
  「序言重现 ⇒ 清 doneFlag 自动重挂」看护。修后同一站点 VA 与当年命中会话逐字节一致。
  两条保留项：① **lib ready 即全量装钩**（开关只在 handler 内门控，关掉时原样转发原函数）；
  ② 12B 原地桩的 `adrp` ±4GB 限制 ⇒ 目标与 handler 相距过远时需 target ±4GB 内的**近距跳板**
  （`movz/movk ×4 + br x16`；或直接用 DobbyHook 自带 trampoline）。注意：口径修正后
  target/handler 本就落在 ±4GB 内（∈同一片 0x4000 区），跳板转为安全冗余而非必需。
  待并入 `skill/re-implement/hook-rules.md`「模拟器/转译层」节 ＋ `skill/starry-mod/SKILL.md` 出包自检。
  证据＝`10_docs/绝区零/出包记录_20261006.md` §第三轮 ＋ `09_test/绝区零/precheck_20261006_010317/`
- [追迹迷城 1.0.20261009 会话复盘 P0–P2 共 9 项全量落地（2026-10-10，用户 m01108「按照方案全量修改」）](history/reviews/规范优化_20261010_追迹迷城1.0.20261009会话复盘.md)：**已合并**——
  ① 入口点靠试错（未先查框架先例）与 ② 判据顺序倒置（先上 frida/dumpsys，最后才用 `am start-foreground-service` 报错文本）
  两个黑洞的机制化对策已全部落入目标位置，合并 commit `ae8b3d56`：
  新建 `04_scripts/_common/framework_entry_audit.sh`（入口四路 + 六项审计，✗ 即禁出包）、`04_scripts/_common/axml_inspect.py`（AXML dump + `--assert-map` 真值断言）、
  `04_scripts/_common/git_push_main.sh`（`push_main`：SSH 5s 探测不通即 HTTPS + `gh auth token`，刷新 `refs/remotes/origin/main`）、
  `mod菜单/starry-mod-menu/scripts/_lib/dex_target.sh`（按 `method_ids` 余量自动选定 dex 合并目标）；
  `trash_paths.sh` tracked 门禁 + `--suggest`；`archive_scoped_git.sh`/`archive_close_git.sh` 接线 `push_main` + 轮次台账复盘提醒；
  `skill/re-implement/hook-rules.md` §29.5.1 判据表；`skill/re-analyze/SKILL.md` §1.1 换代必测三件；`skill/starry-mod/SKILL.md` 入口点决策表 + TapTap 接入检查表；
  `session_start.sh` 入口自检/审计/`device_profile.sh` 提示；`stages.md` 收官脚本参数契约表 + 三脚本 usage 补注；`TOOLBOOK.md` #52 补注与 #53–#55；`AGENTS.md` 变更日志 + 「push 一律经 `push_main`」。
  附带收益：用新工具复查已交付包，抓出 `MenuService` 的 `android:stopWithTask` res id 被凭记忆写成 `0x01010281`（真值 `0x0101036A`），已修 `04_scripts/追迹迷城/patch_zjmc_binary_manifest_v3.py`（无功能影响，包不重出）。
  机器关：`preflight.sh` PASS=14 FAIL=0、`bash32_check.sh` 无硬错误、`axml_inspect.py` 跨游戏回归 6/6 rc=0、`trash_paths.sh` T1–T7 全绿、`push_main` 实跑 HTTPS 绕行成功。
- [邂逅在迷宫 2.6.53 第二轮会话复盘 P17–P22（把纪律做成闸门，2026-10-10，用户「按照方案全量修改」）](history/reviews/规范优化_20261010_邂逅在迷宫会话复盘.md)：**已合并**，commit `e4336e75`（已推送）——
  机制（P10 preflight / P13 方案选型卡 / P15 即时记账）**第一轮就存在却整段旁路**，故本轮不再加「请记住」型规则，改成**闸门 + 模板 + 快照**：
  ① 装机闸门：`session_start.sh` 落 `preflight_{游戏}.stamp`，`gradle_cycle.sh --install` 缺凭证/超 12h/硬约束 md5 变过 ⇒ **rc=4**（实测无凭证 rc=4、有凭证 dry-run 通过）；
  ② 出包即记账：`--game` 全量日志 `tee 09_test/{游戏}/logs/`、完成块打印完整 md5/大小/日志路径、成功自动 `gate_note.sh G2`（治「调用方 `| tail` 截断 ⇒ md5 永久丢失」）；
  ③ 交付：`finalize_game_delivery.sh --usage-doc`（`{占位符}` 残留即 rc=1，正反两路实测）；
  ④ 新工具 `code_locate.sh`（>200 行禁整读，直接给 `read offset/limit`）、`selftest_env.sh`（自测影响面 before/after 快照 + 需复原提示）；
  ⑤ 模板/台账 `侦察笔记.md`、`轮次台账.tsv`（免广告面板 3 轮、菜单球 3 轮已回填）；
  ⑥ `硬约束.md` #13–#18（被提问不得擅自改动 / 不再管卡密 / 不做自启动 / 方案 A 授权 / 菜单必须同时存在）+ `games/邂逅在迷宫.md` 交接文件 + `current.md` 索引行 + AGENTS.md「增量（第二轮）」与变更日志。
  机器关：`preflight.sh` 五脚本 PASS=5 FAIL=0（首轮抓出 `finalize_game_delivery.sh` L385 `$USAGE_DOC）` 硬错误）、`bash32_check.sh` 五脚本全过、`gate_note.sh` 记 G1 一行。
- [霓虹深渊2 会话 9a67c280 效率复盘 P0–P2 全量落地（2026-10-12，用户 m00133「按照方案全量修改」）](history/reviews/规范优化_20261012_霓虹深渊2会话效率复盘.md)：**已合并**——
  根因不是缺规则，而是**三条既有闸门全程 0 次调用**（`pre_install_gate.sh` / `install_guard.sh` / `rootcause_note.sh`），加上更隐蔽的一条：**旧台账列格式与门禁取列错配 ⇒ 门禁数学上不可能触发**。对策：
  ① 补 `04_scripts/霓虹深渊2/pipeline.json`（9 阶段 + `install_policy: overlay_only` + `rounds.threshold:3`；`workdir`=仓库根、`verify.path` 全部根相对 —— 实测 `pipeline.sh` verify 不展开 `$NIXIANG`，全仓同型隐患）；
  ② 台账 `轮次台账.tsv` 迁为 6 列正统的九列超集（22 行→40 行，无损校验）+ `rootcause_note.sh` 改按列名识别判定列、功能键精确匹配、`judged_rows()`、历史累计无效告警（真实台账：功能#4=8 轮、功能#11=8 轮；表头坏 ⇒ WARN「本节对本游戏失明」不再静默 PASS）；
  ③ `install_guard.sh` 新增 `overlay_only`（卸载/清数据 rc=66，需 `--allow-uninstall`）、`finalize_game_delivery.sh` 新增 `--expect-icon/--expect-menu-template`（点名缺内容 rc=1 且不写文件；给全则打印 JSON 原文）；
  ④ `state_digest.sh`（状态文档速读 92 KB→19 KB）、`evidence.sh`（logcat 参数化取证）、`so_inspect.py`（sha/strings/zip/hex/disasm/ptr，**strings 默认 UTF-8 感知**：mod 包 37 条中文串 vs 系统 strings 0 条）、`10_docs/霓虹深渊2/侦察笔记_锚点索引.md`（nhsy2Mod.cpp 24 段锚点表，替代 202 次整读）、`round.sh`（一键轮次环，不重造 cycle 脚本）；
  ⑤ `session_start.sh` 出口 4 组命令 + 「被旁路的闸门」点名；`hook-rules.md` §32「编辑一把刀」；`AGENTS.md` P23–P30 + 变更日志；`TOOLBOOK.md` #100（bash32 全仓 63/633 带硬错误，`bash32_check.sh` 按文件传参 ⇒ 无人全仓扫）。
  机器关：`preflight.sh` 14 文件 PASS=13→修 2 处 `$VAR` 紧跟全角字后 FAIL=0、`rootcause_note.sh --selfcheck` 三组全过、`bash32_check.sh` 无硬错误。

## 2026-10-15 霓虹深渊2 r19 会话复盘（优化会话·已落地）

- 规则目标位置：`skill/starry-mod/SKILL.md`（新增「授权会话（卡密）预置与诊断」小节）／`04_scripts/_common/stages.md`（收尾三条固定命令）／`10_docs/_templates/路线锁定.md`（回读时机第 5 条「数值来源纪律」）／`AGENTS.md`（一行指针，32,725 B ≤ 32,768）／`04_scripts/_common/starry_prefs_edit.sh`（新工具）。
- 依据：`10_docs/_state/history/reviews/规范优化_20261015_霓虹深渊2r19会话复盘.md`；`session_metrics.py --gate` FAIL 5 项（calls/step 1.15、单发 85.9%、bash 检索 78.9%、单条 >8 KB 32 条、单条消息 33,093 字符）。
- 状态：**已合并**（证据＝本轮真机实测行 + 反证排除；落点＝上述文件；提交号见本轮 `archive_scoped_git.sh` 输出）。
- 已合并（同日第二批「按照方案全量修改」）：① P2-2 会话切点硬阈值 → `AGENTS.md` D7；② `pipeline.sh status --brief` ＋ `verify <stage> --set` → `04_scripts/pipeline.sh`＋`_common/pipeline.lock`（指纹 `5efdea57…`）＋`stages.md` 收尾四条固定命令；③ 数值来源纪律 → `skill/re-implement/hook-rules.md` §5；④ `starry_prefs_edit.sh` 校验器改标签流解析。
- 仍留候选：无（本批清空）。下一个观察口＝`session_metrics.py --gate` 在下一次会话是否把 calls/step、bash 检索占比、单条 >8 KB 三项拉回阈值内。

## 2026-10-15 新增（保卫向日葵 1.46 斌哥改包抓点）

### A. 待办：技能表回填（本会话是游戏执行会话，按 AGENTS 禁改 `skill/`，留给优化会话）

- `skill/re-extract/binge-memdiff.md` §1.2 game_id 表追加一行：`| 806 | 保卫向日葵（com.bwxrk.yqmy.ty4） |`
- 同文件 §二「斌哥游戏档案表」追加一行：

  | 保卫向日葵 1.46 | Unity IL2CPP v29 + xLua + HybridCLR(热更) | 全壳(无嵌套 base.apk, 但有 shell_config) | 806 | B：内存 RX vs 磁盘 so 全段 diff（仅 2 处 12B 跳桩）+ 壳 BSS 自述中文串 + 双构建 RVA 对照 | 拿全部技能(帧指令) / 战斗加速(改返回值，壳自检拒装) | `10_docs/保卫向日葵/斌哥包抓点_20261015.md` |

- 注意：本次触碰该文件需同时执行「触碰即清」（该文件在 `技能层待办.md` 队列 #2）⇒ 回填与去游戏化**同批做**。

### B. 候选经验（证据齐、落点明确，待优化会话合并）

1. **新版斌哥模板把功能自述写成 BSS 中文串**（落点：`re-extract/binge-memdiff.md` §一路径 C 第 5 步 + §3.5）
   证据：保卫向日葵 `libSDKloading.so` rw 段直接可读 `[BW] 钩点 …=%p …=%p`、`[BW路标] n/3 准备装 <类::方法>(★功能名)`、
   `[BW☠] … -> 不装这个钩子`、`[BW] 开关: 第N格=★<功能名>`、`[BW诊断] 心跳活着 … 格子1=%d 格子2=%d`，
   以及一整段「已判死清单」。⇒ **抓点第一步应先 dump 壳 rw/BSS 并抽 UTF-8 串（`so_inspect.py strings --utf8`）**，
   功能名↔钩点↔原理一次拿全，可省掉大量反汇编；`so_inspect.py strings` 默认 UTF-8 是关键（原生 `strings` 会漏中文）。
2. **必须区分「APK 内嵌构建」与「运行时热更构建」两份 libil2cpp / metadata**（落点：`re-extract/binge-memdiff.md` §六坑表新增一条 + §1.1 判据）
   → **已合并（2026-10-19，跨游戏第 2 例：我的勇者 7.6.3 首装闪退）**：出包侧正文＝
   `skill/starry-mod/SKILL.md`「直装包不得『只换 libil2cpp.so』」；装机前强制＝`04_scripts/_common/pre_install_gate.sh` 查9
   （`--apk` + `--base-apk`，只换 so 不换 metadata ⇒ FAIL）。本条保留在 re-extract 侧作为**取证侧**判据。
   证据：保卫向日葵运行时 dlopen 的是 dodjoy 热更落地的 `/data/data/{pkg}/files/libil2cpp.so`（62,235,240）而非 APK `lib/arm64-v8a/`（62,032,888）；
   maps 里 `files/` 那份才是真身；metadata 同理（14,295,428 vs 14,253,536）。两套 RVA 不同（本例 +0x25194 / +0x253A8）。
   判据命令：`su -c 'grep libil2cpp /proc/<pid>/maps'` 看路径，再看 `/sdcard/Android/data/{pkg}/files/StreamingAssets/ab/` 是否有 `arm64-v8a_libil2cpp.so`。
3. **壳的「建档偏移守卫」会在游戏首启热更后拒绝装钩（静默失效）**（落点：`re-extract/binge-memdiff.md` §六坑表 + `re-implement/hook-rules.md` §24 指纹铁律邻近）
   → **已合并（2026-10-19）**：通用表述已写入 `skill/starry-mod/SKILL.md`「直装包不得『只换 libil2cpp.so』」末条
   （写死 RVA 的补丁在换构建后必须显式跳过并留日志，先例 `Games/WdyzMod.cpp` 无CD `expect drift → skip`）。
   证据：原话 `[BW☠] GetFinalTimeScale 偏移不符(实际2609984 期望2609452) -> 可能游戏已更新, 不装这个钩子`；
   建档值＝APK 内嵌构建布局，现场值＝热更构建；读数口径＝`GetFinalTimeScale − SelectRandomSkill` + 常量 `0xE8`
   （期望 `0x27D12C`＝`0x27D044`+0xE8、实际 `0x27D340`＝`0x27D258`+0xE8），两构建差 `0x214`，与壳打印差逐字节吻合。
   ⇒ **移植结论（可外推）**：在这类「热更 IL2CPP + 会自校验偏移」的游戏上，星光侧挂点必须**按名解析 + 序言字节校验**，
   禁止写死 RVA；且抓到的 RVA 必须标明属于哪一份构建。

4. **`il2cpp_runtime_invoke` 的返回值约定：引用类型直接给对象指针、值类型是装箱对象（真值在 `+0x10`）**（状态：**已合并**（2026-10-16，提交 `46c1ac8e`）；落点：`re-implement/il2cpp-abi-pitfalls.md` **坑 9**「`runtime_invoke` 返回值方向的 ABI」＋ `hook-rules.md` **§28.6**「读侧变体：读错偏移的返回值同样是恒真」；先例：`10_docs/冒险与征途/技能先例.md`）
   证据（冒险与征途 1.5.9 / v50→v52，2026-10-16 真机）：
   · v50 按「`r` = 指向 retval 槽的指针」写 `*(void**)r` ⇒ 取到的"对象"类名是垃圾（`cls=0WŐt`），
     因为读到的其实是**对象自己的 klass**；改成候选① `r` 即对象 + 类名自证后即命中。
   · 值类型同一次调用三个标量双读对比（`AD52 raw` 行，同一帧）：
     `候选A(读 r): adid=-1608307296 result=-1608307296 uuid=500902869376`（= klass 指针残段，**错**）
     `候选B(r+0x10): adid=18 result=0 uuid=131202526972572400`（= 真值，**对**，且与 AD43 从字符串独立解析出的 uuid 完全一致）
   ⇒ **结论（可外推）**：用 `runtime_invoke` 读返回值时，引用类型按「返回值即对象」处理；
     值类型必须**按 `+0x10` 取装箱数据区**（与本仓 `CdInvoke` 现在按 `*(uint8_t*)r` 读 bool 的写法**不一致**，
     该写法在别的游戏上可能一直读到 klass 低位 0 ⇒ 需按本卡复核）。
   ⇒ **自证姿势（建议固化）**：不要赌约定，**双读 + 业务约束自证**（本作用 `uuid` 量级 >1e15、
     `ad_id ∈ (0,1e5)`、`result ∈ [0,16]` 三条同时成立来定案并缓存），任何一条不成立就不动手。
   ⇒ **同轮复核（2026-10-16，结论已落盘）**：全仓 `runtime_invoke` 返回值用法普查 ⇒ 需复核的只有本作
     `MxytzMod.cpp` 的 `CdInvoke`（`EndfieldMod` 已按 `+0x10` 读、`WhrhxMod:1163` 按对象读、其余不看返回值）。
     其旧读法 `*(uint8_t*)r` 读到的是装箱 `System.Boolean` 的 **klass 低位常量** ⇒ 判据恒真、候选「行为探测」形同虚设
     （源码 line 5021 的「`KILLDEAD ok=1` 真死」不是有效证据）。按该游戏硬约束 #7③
     **只加双读探针、未改判据**（切 `+0x10` 会让秒杀兜底一个候选都锁不上 ⇒ 落点语义变更，须授权 + 实测轮）；
     源码已改、**未装机 / 未实测**（定案需 `MXYTZ_VERBOSE=1` 诊断档一次装机）。

## 候选：可逆写入的「还原路径」禁止对可能已回收的对象调 il2cpp 反射 API

- 状态：候选（2026-10-26 冒险与征途 v60 真机血证；落点建议 `skill/re-implement/hook-rules.md`「可逆写入/清理」节 + `il2cpp-abi-pitfalls.md`）
- 证据（实测，可复现）：用户关掉 #3 开关后 0.58 s，UnityMain SIGSEGV；tombstone 帧序 `#00-#02 libil2cpp.so+0x205aa5c` ← `#03-#07 libstarry_magic.so`，`fault addr 0xffffff00000166`、`x0=0x00ffffff00000030`（垃圾指针）。反汇编定案：崩点在我们按名解析缓存的入口 `Off()` → `api.FieldOffset(klass, name)`，而 `klass` 来自 `api.object_class(已回收的配置对象)`。
- 结论：**记录下来的对象指针只保证「当时有效」**。任何延迟写入/还原/补偿路径（开关关闭还原、ForceClear、超时回滚、批量撤销）都必须：
  ① 写入时同时记录 `klass` 与字段偏移（不要还原时再按名解析）；
  ② 还原前 `Il2Cpp::IsReadableRange` 探一次可读（冷路径，允许 maps 查询）；
  ③ 裸比对对象首字 `klass` 与记录值一致，不一致 = 已回收/复用 ⇒ 直接放弃，不写。
  ④ 兜底再给所有「按名解析」入口加零 API 自证：`klass` 与 `klass+0x10`（Klass::name）都必须在用户态，否则返回 0（fail-safe）。
- 反例（本轮修掉）：`RestoreProj` 里 `api.object_class(slot.data)` + `Off("_count")`；`RestoreInvulnBits` 直接写记录地址（现已加可读性闸）。

## 候选：改含中文的文件**禁用 PowerShell 文本管道**（会按代码页/UTF-16 转码）

- 状态：候选（2026-10-27 勇士之心会话实证事故；落点建议 `AGENTS.md` §5「中文路径乱码」邻近 + `TOOLBOOK.md`）
- 事故：给后台 `static/admin/index.html` 改一行版本号，用
  `(Get-Content -Raw) -replace ... | Set-Content -NoNewline -Encoding UTF8` ⇒ 整个文件 227 增 / 231 删，
  中文被按系统代码页读出再以 UTF-8 写回，**全页乱码**（无 U+FFFD、无 ???，字符数只差 502，肉眼与门禁都不报错）。
  补救时 `git show HEAD~1:path > file` 又踩第二次：PowerShell 重定向默认写 **UTF-16LE**（51,070 B → 98,836 B）。
- 正确姿势（已实施）：`git checkout HEAD~1 -- <path>` 还原字节 → **Python 字节级**替换
  `io.open(p,"rb").read().replace(b"v=5", b"v=6")` → `io.open(p,"wb").write(...)`。
  结果 diff 从 227/231 降到 **1 行**，文件 51,070 字节与原文件完全一致。
- 结论（可外推）：本仓一切文件改写走 ① `edit`/`write` 工具 ② Git Bash + Python 字节级读写。
  **禁止** PowerShell 的 `Get-Content`/`Set-Content`/`-replace`/`>` 重定向参与文本改写；
  校验必须比字节数 + 关键中文串（`"...".encode("utf-8") in b`），不能只看 PowerShell 打印。

## 候选：JSON null 经 `org.json` `optString` 会变成字符串 `"null"`（授权链静默失败）

- 状态：候选（2026-10-27 冒险与征途加固轮实测；落点建议 `skill/re-starry`/`starry-mod` 授权链章节 + `10_docs/防御加固_通用路线图.md` §2 第 6/7 步）
- 事实：服务端 FastAPI 对 `None` 字段输出 `"grant3": null`；Android `JSONObject.optString(k, "")`
  走 `JSON.toString(JSONObject.NULL)` → **字符串 `"null"`（非空）**，不是空串、也不等于 `null` 引用。
- 后果：客户端把 `"null"` 当票交给 native ⇒ `sgauth::apply()` 找不到 `.` 分隔的签名 ⇒ 直接 false
  ⇒ 菜单 UI 说「登录成功」、功能一个都不授权（日志 `[V2] r=0`）。**所有没登记载荷 sha 的游戏**都会中
  （`mint_grant_v3()` 无载荷钥时返回 None ⇒ grant3 恒为 null）。
- 判据/修法：所有「多版本字段回退」取值一律过 `isValidGrant()`（非空 && 不等于 `"null"`），
  且**读侧**（从 prefs 读历史值）也要过滤，否则修好写入仍被脏值卡住。
- 反例：修复前 `pickGrant` 用 `g3 != null && !g3.isEmpty()` 判断，看着对、跑起来全游戏登录失效。
## 候选：往 18MB 加密脚本里 splice，必须过「括号配平 + node --check」双关

- 状态：候选（2026-10-27 造梦无双 1.34 实测黑屏事故；落点建议 `skill/re-implement/hook-rules.md` 新增「JS splice 语法关」小节 + `04_scripts/_common/stages.md` 装机前产物门禁）
- 事故：给 Cocos `index.jsc`（运行时解密成 17.98MB JS）注入「结算时长补齐」段时多打一个 `}`。
  运行日志 **splice 全部成功**（`spliced starry_zmws_timepad …`、`combat splice #1 len …→…`），
  但整段 eval 抛 SyntaxError ⇒ **引擎不渲染 = 黑屏**，用户看到的是「装完黑屏」，极易被误判为封号/加固/签名问题。
- 双关（已实施）：
  ① 运行期：注入串构建后先 `BracesBalanced()`（跳过字符串/注释、括号与花括号双重配平），**不配平就不注入**并 `LOGE ... brace check FAILED`；
  ② 构建期：`04_scripts/造梦无双/zmws_splice_syntax_check.py` —— 从**已构建的 so** 里按注释标签把注入串原样抠出来（`%s`→真实包名），
     按与 C++ 相同的锚点表插入**探针 dump 的明文脚本**，`node --check` 全量验语法。改 C++ 必须同步改脚本锚点表。
- 可外推判据：任何「字符串拼接进加密/大体积 JS（Cocos jsc、Lua 字节码源、HybridCLR DLL）」的注入，
  **日志 splice 成功 ≠ 脚本可执行**；装机前必须有「插入后的完整脚本过解释器语法检查」这一关。
- 反例：只能靠用户说「黑屏」才发现——本轮已浪费用户一次配合与两轮热替换。

## 候选：`pre_install_gate.sh` 查5 只查 dex，so 里的门禁串会假 FAIL

- 状态：候选（2026-10-27 造梦无双实证；落点建议 `04_scripts/_common/stages.md` 门禁章节 + `pre_install_gate.sh` 用法注释）
- 事实：`dex_literal_count` 对 `*.apk` 只 `unzip -p 'classes*.dex' | grep`，so 里的字面量（如 `starry_zmws_timepad`）永远查不到。
  本轮把 so 侧注入标签当门禁串传给 `--apk` ⇒ 全项 FAIL 拦装机，但产物其实是好的。
- 正确姿势（已在游戏文档记录并跑通）：拆两次
  `--apk <apk> --needle '<dex 里的类名/方法名>'` + `--dex <so 文件> --needle '<so 里的字面量>'`。
- 建议改法（未实施）：`--apk` 分支同时扫 `lib/**/libstarry_magic.so`，或新增 `--so-needle` 参数。

## 候选：菜单框架里「持锁 + 无超时 `su` 兜底」= 点击即卡死（class: tech）

- 状态：候选（2026-10-28 绿梦真机实证；落点建议 `skill/hook-rules.md` 或 `skill/starry-mod/SKILL.md` 的框架红线 +
  `mod菜单/starry-mod-menu/.../DeviceFingerprint.java` 类注释已写死本轮结论）
- 事实：`Preferences.getOrCreateDeviceId()` 原是 `static synchronized`，内部 `legacyCandidates()` 会 exec `su`；
  在「有 `/system/bin/su` 但本 uid 未授权」（KernelSU/Magisk 未点允许）的机器上，`fork` 卡进 kernel D 态
  （`wchan=kernel_clone`），`destroyForcibly()` 也杀不掉 ⇒ 类锁永不释放 ⇒ 主线程 `showMenuNative →
  Preferences.with()` 永久 BLOCKED ⇒ `Input dispatching timed out` ANR ⇒ ColorOS `o-stop` 杀进程。
  表象就是「点悬浮球直接卡死闪退」。
- 判据（可复用）：`kill -3 <pid>` 拉 `/data/anr/trace_*`，查 `- locked <…> (a java.lang.Class<…>)` 那个线程的栈里
  有没有 `java.lang.UNIXProcess.forkAndExec`；主线程若 BLOCKED 在同一把锁上，就是这类缺陷。
- 正确姿势：① 所有外部进程调用 `exec` 只走一个带硬超时的封装（读 stdout 与等待都要能超时）；
  ② 绝不在 `synchronized` 区内做 exec/慢 IO；③ 采集型兜底再加一层「旁路线程 + 总预算」，超时按空结果放行，
  保证主身份/主流程先落盘；④ 无 root 设备上 `exec("su")` 会立刻失败，**别拿无 root 机器验收当"没问题"**。
- 覆盖范围：所有用 starry-mod-menu 框架 + 会探测 root 的游戏；本次只修了框架源码，其它已出包游戏需重出包才带上。
