# 经验生命周期元数据

本文件维护旧经验清单与审批说明；新经验卡的机器可审计状态以经验卡 frontmatter 为准。旧条目暂不批量迁移，审计器将其报告为 `legacy`，不阻断检查。

状态含义：

- `candidate`：候选经验，证据或落点还不齐。
- `approved`：证据齐＋落点明确，已按自动门槛自批，待合并（2026-10-04 起不再等用户审批）。
- `merged`：已经落入目标规则，必须记录实际目标位置与合并 commit。
- `rejected`：有充分反证不采纳（不要求用户逐条决定），保留否定依据。
- `stale`：适用条件变化或被新证据推翻，保留历史与替代关系。

2026-10-04 起审批语义改为证据门槛（见 AGENTS.md「规范/技能优化全自动」）；下文历史条目原文不改，其中的「待用户批准」按新门槛重新判定即可。

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
  落点拟：`skill/re-implement/hook-rules.md` 新节「反作弊压制与上报拦截分层」＋ `skill/starry-mod/SKILL.md` 出包自检清单（DNS 4 入口、黑名单后缀必含 SDK 的**长连接配置域名**，否则纯 IP 长连接绕过）。
  证据＝`10_docs/消消英雄2/过检测与上报拦截_1.2.28.md` ＋ `09_test/消消英雄2/precheck_20261009_001702/logcat.txt`（实测 `DNS BLOCK android.bugly.qq.com / openlog.tapapis.cn / e.tapdb.net`，且 `tapsdk.tapapis.cn`、`*.alina-game.cn` 正常放行）。
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
