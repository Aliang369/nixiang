# 安卓游戏逆向工作区（nixiang）— 唯一入口

> 所有 AI 程序（Cursor / Claude Code / Kilo / Codex / Cline / QClaw 等）先读本文件。
> 本文件是读取、授权、协作与通用政策的唯一来源；技术细节按需查 `skill/` 与三个 BOOK。引用文档中的旧读取协议或通用政策与本文件冲突时，以本文件为准。

## 读取与协作路由

1. 开始时只读 `10_docs/_state/current.md` 的摘要索引；按目标游戏再读
   `10_docs/_state/games/{游戏}.md` 的“当前摘要”和 `skill/SKILL.md` 的相关入口。
   **再读该游戏的 `10_docs/{游戏}/硬约束.md`（存在即必读；模板 `10_docs/_templates/硬约束.md`）** ——
   用户下达过的「必须 / 禁止 / 照 XX 做」逐字都在那里，长会话被压缩后不得凭记忆行事。
   只有摘要明确要求追溯时才读该文件下方迁移历史。禁止为了“了解全仓”而通读
   全部游戏摘要、完整 logcat 或 `06_dumped` dump。
2. 按任务选首个入口，问题跨领域时再补相关章节，不限死为一门：
   - 新游戏 / 新版本接手 → `skill/re-analyze/SKILL.md`
   - 已有包提取玩法 / 抓点 → `skill/re-extract/SKILL.md`
   - **用户说「先过环境 / 先别直接做功能 / 菜单进游戏能正常玩才算完成」→ `skill/re-analyze/environment-min-path.md`**（环境阶段最小充分集五步 + 每步退出条件 + 「推迟到第二阶段」清单；别在第一阶段做 IL2CPP 符号/RVA/metadata 分析）
   - **已有星光包在跑、游戏内热更后功能失效 → `skill/re-extract/hotupdate-fingerprint-recheck.md`**（先三查分流：热更资源版本 / 核心 so SHA / 玩法分包是否只在热更目录；确认是 JS 指纹漂移再动手，不先改 native）
   - 写功能 / Hook → `skill/re-implement/SKILL.md`（指纹铁律见 `re-implement/hook-rules.md` §24：禁止写死会被混淆器改名的短标识符；**挂点五步门禁见 §22**，第 5 步＝`10_docs/{游戏}/路线锁定.md` 填满且无「未确认偏离」才准装机，模板 `10_docs/_templates/路线锁定.md`）
   - 星光落地 / 切游戏 / 出包 → `skill/starry-mod/SKILL.md`
   - **已有星光包，只重打 dex/APK/重装 → 游戏摘要 + `skill/starry-mod/SKILL.md`「存量游戏快速重打」+ `04_scripts/_common/stages.md` 收尾入口**（只改 Java/dex 不重编 native；复用正式包用 `STARRY_INSTALL_ONLY=1`）
   - 机械操作（拆包/打包/注入/移植/签名/装机/清理）→ `04_scripts/_common/stages.md`（第一步先 `ls 04_scripts/{游戏}/pipeline.json`：存在→走 `pipeline.sh`；不存在→用该游戏的 `surgical_*/inject_*` 脚本，仍禁直接敲 adb/apktool/zip/ndk-build；分析操作照常推理产出落 `10_docs/`）。**有 `pipeline.json` 的游戏直接用 pipeline.sh 的原生动词**：`bash 04_scripts/pipeline.sh <游戏> status`（钉硬约束 + 台账）/ `next --loop N`（连跑，逐次回显日志尾 40 行）/ `log [N]` / `round`+`rounds`（轮次门禁，见「执行纪律补充」）；旧写法 `bash 04_scripts/_common/stage_next.sh <游戏> [status|next|auto N|rounds]` 仍在，但已缩成同义转发器。
   - 分析 `.so` → `skill/ida-plugin-flow/SKILL.md`（扫描与插件按具体问题触发，不作无差别全扫）
   - 经验查阅 → `skill/re-lessons/SKILL.md`
   - 纯评估 / 文档维护 → 只读待评估文档及直接引用，不自动进入游戏执行流程。
2.5 **动手前先查「现成能力」**（2026-10-02 消消英雄2 教训，最省 token 的一条）：
   `ls 04_scripts/_common` + `grep -rn "<关键词>" skill/ 04_scripts/_common/`，
   命中就直接用，**禁止重造**。已存在且被重复造过的例子：`bash32_check.sh`（bash 3.2 三坑机检）、
   `lua_struct_check.py`（内嵌 Lua chunk 体检，`--disasm` 可反汇编运行时 dump）、
   `finalize_game_delivery.sh`（收尾交付一键）、`prune_output_final.sh`、`stage_shared_file.py`、
   `pre_install_gate.sh`、`check_release_clean.sh`；技能侧先读 `skill/SKILL.md` 索引再决定自研。
3. 流程细节按需读 `PLAYBOOK.md`（接手）、`WORKFLOW.md`（日常）、`TOOLBOOK.md`（工具/目录），不是串行必读链。
4. 多代理协作先划定文件归属和禁止修改项；不覆盖他人改动，跨归属改动先协调，最终汇总改动与证据。
5. 外部方法论引用已收敛（2026-09-10）：原 `/Users/aliang/Documents/reverse-skill` 外部库经评估整体与本工作区低相关，唯一有价值的 ida-pro-mcp 工具速查已择录进 `skill/ida-plugin-flow/references/ida-mcp-cheatsheet.md`，外部库本体已删除（可从 github.com/zhaoxuya520/reverse-skill 重新克隆）。不再引用外部路由，一切以本入口与 `skill/` 为准。

## 双机协作（每台机器，每轮开工第一步）

本仓由 **mac 端**与 **Windows 端**共用同一个仓库（`git@github.com:Aliang369/nixiang.git`，分支 `main`）。
**任何机器上的任何 AI，接手任务后的第一步先做本节，再读其它内容。**

```bash
cd "$NIXIANG_ROOT"                 # Win 端在 Git Bash 里；mac 端在 zsh 里
bash 04_scripts/双机协作/twin_status.sh   # 双机体检：有 FAIL 先修再开工（同时自动重写本机心跳）
git status -sb                     # 必须干净；不干净先报告处理，不要 pull
git pull --rebase --autostash      # 拿到另一台机器已推送的提交
git log --oneline -5               # 确认拿到了什么
sed -n '1,6p' "10_docs/_state/机器心跳/win.md" 2>/dev/null    # pull 后看对方心跳（win 端则看 mac.md）：超 7 天未更新或含 FAIL → 先提醒对方复检
```

- **顺序不可颠倒**：先 `status` 再 `pull`。工作区不干净时 pull/rebase 会把未提交改动卷进去。
- **一机一游戏**（`10_docs/双机协作/双机协作同步方案.md` §2.1）：同一轮只由一台机器写同一个游戏；开工前先划定归属（如 `mac → 游戏 A`，`win → 游戏 B`），登记 `bash 04_scripts/双机协作/claim_game.sh <游戏>`（收工 `--done` 释放，占用表 `10_docs/双机协作/本轮归属.tsv`）；pull 后发现对方已认领未释放的游戏禁止写。两台要改同一游戏须先协调，否则会产生真实合并冲突。
- **机器心跳**：`twin_status.sh` 每次运行自动重写 `10_docs/_state/机器心跳/{mac|win}.md`（只写本机那台，收工随 git 提交）。对方心跳超 7 天未更新或含 FAIL 时，先提醒对方复检，不默认其环境正常。
- **跨机接手先看重建行**：接手另一台机器分析过的游戏时，先核对该游戏 `games/{游戏}.md` 摘要里的「接手重建：」行（需重新 dump/下载的产物与再生成命令）；没有就先补写再动手——dump 产物不入库，按历史路径找文件会扑空。
- **状态索引单一写者**（§2.2）：`10_docs/_state/current.md` 与 `10_docs/_state/games/{游戏}.md` 由本轮动了该游戏的那台机器写，另一台只读。
- **push 被拒**（另一台已先推）→ `git pull --rebase --autostash` 后再 push；**禁止 `push --force`**（§6.3）。
- **push 一律经 `push_main`**（2026-10-10 追迹迷城复盘 P0-5）：收官脚本 `archive_scoped_git.sh` / `archive_close_git.sh` 已接线 `04_scripts/_common/git_push_main.sh` 的 `push_main` —— 先做 ≤5 秒 SSH 探测，不通且 `gh auth token` 可用即自动改走 HTTPS 推送（不改 remote 配置），成功后 `fetch` 刷新 `refs/remotes/origin/main`。**别再裸 `git push` 盲等超时，也别在 URL 推送后以为跟踪引用已更新。**
- **收工**（§6.2）走现成口令对应的 `04_scripts/_common/archive_scoped_git.sh`：只按本机归属范围做白名单 `git add`，禁止 `git add -A`。
- Git Bash 里可直接用：`nixst` / `nxfetch` / `nxpull` / `nxlog` / `nxclean`。
- 完整规则（终端分流、兼容垫片、Syncthing 大头层同步、验收清单）见 `10_docs/双机协作/双机协作同步方案.md`。

## 授权与测试边界

- 以用户明确指定的目标、文件、设备、账户、版本和测试范围为边界；本地阅读、检索、构建、离线测试及范围内低成本可逆探索无需逐次请示。真机安装、运行、抓取仅在相应设备与测试范围已授权时开展；文档任务不自动授权真机操作。
- 破坏性操作、清数据、卸载、不可逆覆盖、影响其他游戏/设备/账户、外部发布/分发/上传或服务端写操作，须先明确说明具体范围、影响与恢复方案并取得确认；已有具体授权覆盖时不重复询问，不从「修好/重装/出包」推定这些权限。
- 游戏 APK 默认外科 zip 组装；`apktool b` 游戏整包重建例外须取得针对该游戏/版本/改动的具体授权，并提供原包、外科方案与例外方案的对照证据。模板构建细节见 `WORKFLOW.md` §4；未验证例外不得当作默认路径或正式交付。
- 不默认清数据或卸载；大资源游戏优先覆盖安装。异签名导致无法覆盖时先报告，不自动转为卸载。
- **数据备份默认不做**：未当轮明确要求时，禁止创建 `09_test/**/backup_*` 或任何存档/数据备份，`install_*` 脚本默认不带备份步骤；「修好 / 重装 / 出包」不构成备份授权。
- **功能边界默认不扩张**：只实现用户当轮点名的功能与菜单项，禁止以「顺手 / 顺便 / 反正要做」为由新增未点名的功能（历史事故：擅自加「宝箱全拿」菜单项）。收尾时把 `10_docs/{游戏}/{id}_menu_template.json` 的功能集与用户点名单逐项对照，多一项即视为越界。
- **规范/技能优化全自动（2026-10-04 定，用户明示「不审批，AI 自动完成优化」）**：以规范优化 / 复盘 / 文档维护为任务的**优化会话**可直接修改 `skill/`、`AGENTS.md`、`10_docs/_templates/`、`04_scripts/_common/`（`pipeline.sh` 仍走指纹锁跨机姿势），无须逐次请示；每笔改动必须 ① 按适用维度自查 `skill/re-implement/code-review-checklist.md` ② 跑 `04_scripts/_common/preflight.sh` ③ 在「规范变更日志」追加一行（日期＋文件＋改动＋依据）。**游戏执行会话（拆包/hook/装机等日常任务）仍禁改上述文件**——撤的是审批流，不是防线。日志供用户抽查，不阻塞执行。
- **规范变更日志**（追加在末尾，只增不删）：
  - 2026-10-03（用户授权）：「规范优化全量落地（P1–P9）」改 `skill/re-implement/hook-rules.md`（§22 扩第五步、新增 §29.5 与 §30）。
  - 2026-10-04（用户明示全自动）：去审批化——优化会话直改规则/skill＋日志制；经验生命周期改证据门槛自动合并；复盘模板新增违规复现表。
  - 2026-10-04（用户授权「按这个方案来」）：真机零截图节补录三条禁令根因（坐标定位歪烧流程与 token / 菜单授权墙机器过不去且易误诊服务端 / 人工实测比机器判定准）；新增「装机预检闸门＋飞行记录仪」条目与 `04_scripts/_common/post_install_precheck.sh`、`04_scripts/_common/flight_recorder.sh`，分工定死：人工＝终审＋标记，机器＝预检员＋记录仪。
  - 2026-10-04（用户明示「按照你说的进行调整」）：新增 `04_scripts/_common/device_profile.sh`（设备画像：对设备只读，八节报告落 `10_docs/_state/设备画像/<serial>.md`，含 libil2cpp 映射分组/基址判据、RVA 段权限与内存 vs 文件字节、`extractNativeLibs`/`ro.apk_verity.mode`）；`AGENTS.md` §授权与测试边界 新增「验收阶梯 L1/L2/L3」「设备画像先行」，§执行纪律补充 新增「诊断与最可能修复同包」「一轮一个变量、一轮只问一次」；`skill/re-implement/hook-rules.md` 新增 §31 设备无关单路径与 SELFCHECK。实测依据：真机 `f9a116a2` 画像 `组数=2 选中组=#1 rw-p=0x2b7000`、两条 RVA 均 `perm=r-xp inbest=yes`、`ro.apk_verity.mode=enforcing` ⇒ **D3 原地改包快路在该机不可用**。
  - 2026-10-04（同轮，出包提速 D1/D2）：`04_scripts/冒险与征途/mxyzt_so_only_1.5.9.sh` 复刻底包改 `cp -c`（APFS clonefile，实测 886MB 从 ≈5 秒降到 0.005 秒，失败回落 `cp -f`）、zipalign 前加 `-c -p 4` 预检（预检 rc=1 未对齐 ⇒ 本游戏不触发；zipalign 实测仅 1.46 秒）、订正「原样搬压缩流」的错误注释（实测会重压 deflate 条目）；`hook-rules.md` 追加 §31.7 出包提速实测表。结论：**打包不是瓶颈，大头是编译+装机 ⇒ 省时间靠减少轮次**。
  - 2026-10-05（用户口述「生成图标和菜单模板不该每次收尾都做，新游戏首次、或我说了才生成」）：`04_scripts/_common/finalize_game_delivery.sh` 改为 icon/菜单模板**按需交付**——删掉菜单模板自动扫描（不传参不再报错/不再复制），两参数都不传时不创建 delivery 目录、`--sync-doc` 的 delivery 行/摘要行/回扫行全部条件化，首次交付未传时打 NOTE 提醒；`AGENTS.md` 收尾节、`04_scripts/_common/stages.md` 收官步 2、`skill/starry-mod/SKILL.md` 收尾产出三处同步改口径；冒烟 T1–T6（已交付游戏跳过/显式 icon+模板/首交付 NOTE/apply 产物/缺模板报错/apply 跳过）PASS。
  - 2026-10-10（用户 m04481「按照方案全量修改」）：邂逅在迷宫会话复盘 P0–P7 全量落地。新建 `04_scripts/_common/session_start.sh`（开工六查：硬约束/路线锁定/台账+**禁止重试清单**/管线出口/TOOLBOOK 坑位/设备·预算）、`gradle_cycle.sh`（bump→assembleRelease→zipalign→apksigner→verify→出包门禁→装机→可选诊断自测抓日志）、`check_release_apk.sh`（查 `application-debuggable` + 禁用串 + versionCode/Name 一致）、`gate_note.sh`（G1/G2/G3 门禁 append-only 台账）、`prior_art.sh`（动手前先例检索，全无命中 rc=1）；`post_install_precheck.sh` 新增 `--helper-pkg/--helper-start/--helper-delay`（治独立助手形态下 `native menu shown` 落在录制窗口外的段=B 假阴性）；新增 `10_docs/_templates/{台账.md,方案选型卡.md}`；`TOOLBOOK.md` 新增「二-b、本机 / 真机坑位（命令级）」22 行；本文件新增下方「增量（2026-10-10…）」子节 P10–P16。依据 = `10_docs/_state/规范优化_20261010_邂逅在迷宫会话复盘.md`（该会话 4.4k 条消息 / 9 版装机包 / 11.4 GB 死路产物 / 1 次外部服务器事故；根因不是缺文档而是**机制被旁路**——本游戏既无 `硬约束.md` 也无 `pipeline.json`，纪律全靠对话记忆，prune 一到就稀释）。机器关：全部改动过 `04_scripts/_common/preflight.sh`（PASS=4 FAIL=0，其中 bash 3.2 门禁实测抓出 5 处 `$VAR` 紧跟中文）；`check_release_apk.sh` 对交付包 `星光助手_v2.9_xhzm.apk` 实跑 rc=0，反向灵敏度自检（`--forbid CdnVpnService`）命中 ×3。
  - 2026-10-10（用户 m01108「按照方案全量修改」）：追迹迷城 1.0.20261009 会话复盘 P0–P2 共 9 项全量落地。新建 `04_scripts/_common/framework_entry_audit.sh`（入口四路 + `PKG_WHITELIST`/`SignSpoof`/`GameConfig`/`Android.mk`/入口类/包内 dex·AXML 六项审计，任一 ✗ ⇒ rc=1 禁止出包，自带 `--selfcheck` 正负控）、`04_scripts/_common/axml_inspect.py`（AXML 结构 dump + `--assert-map` 平台属性 res id 真值断言）、`04_scripts/_common/git_push_main.sh`（`push_main`：SSH 5 秒探测不通即自动改走 HTTPS + `gh auth token`，成功后刷新 `refs/remotes/origin/main`；不盲等、不强推）、`mod菜单/starry-mod-menu/scripts/_lib/dex_target.sh`（按 `method_ids` 余量自动选定 dex 合并目标，外科脚本禁再硬编码 `classesN`）；`trash_paths.sh` 补 tracked 门禁（dry-run 逐项 `git ls-files --error-unmatch` 命中即拒）+ `--suggest --game <游戏>`；`archive_scoped_git.sh`/`archive_close_git.sh` 接线 `push_main`；`skill/re-implement/hook-rules.md` 新增 §29.5.1「症状 → 第一条判据命令」+ 三条禁止当判据的死路；`skill/re-analyze/SKILL.md` 新增 §1.1「新版本接手必测三件」；`skill/starry-mod/SKILL.md` 新增「入口点决策表」「TapTap 接入检查表」；`session_start.sh` 追加入口自检 / 出包前审计 / `device_profile.sh` 提示；`TOOLBOOK.md` 二-b 新增 #53–#55。依据 = `10_docs/_state/规范优化_20261010_追迹迷城1.0.20261009会话复盘.md`（根因＝入口点未先查框架先例 + 判据顺序倒置）。**期间用新工具复查已交付包，抓出一处真实缺陷**：`MenuService` 的 `android:stopWithTask` res id 被凭记忆写成 `0x01010281`（＝`android:glEsVersion`），真值 `0x0101036A`；无功能影响（该属性平台默认即 `false`），已修正补丁器并落 `10_docs/追迹迷城/台账.md`。机器关：`preflight.sh` PASS、`bash32_check.sh` 无硬错误、`axml_inspect.py` 跨游戏回归 6/6 rc=0（真值表订正前 6/6 误报 FAIL，见 TOOLBOOK #55）、`trash_paths.sh` T1–T7 全绿、`framework_entry_audit.sh` 追迹迷城 rc=0 / 反例 rc=1 / `--selfcheck` 全绿。**合并提交 `ae8b3d56`（已推送，`origin/main` 同步）**；§6 落地回填表见该复盘文档，经验登记见 `10_docs/_state/pending_experience.md`。
- 需要用户操作（进游戏、开菜单、打开工具等）时在聊天说明操作、成功标志和所需配合，等待确认并核对结果；只暂停依赖该结果的步骤，不假定用户已操作，独立且已授权工作可继续。
- **真机交互零截图（2026-10-01 定）**：不做 `screencap` 猜坐标、不 `input tap`、不靠截图判断界面状态 —— 真机取证一律走 `logcat`、`/proc/<pid>/{mem,maps}`、frida。屏幕上的点按、输入、翻页**全部由用户完成**。（历史浪费：为算 `input tap` 坐标花掉 `wm size` + `screencap` 的调用，最终未用上。）
  **三条根因（2026-10-04 用户口述定案，禁止再提机器实测 / 自动点击类提案）**：① 坐标定位会歪，失败重试烧流程和 token；② 菜单授权墙机器过不去、还会误诊成服务端问题，而用户输一次账号即可解决；③ 人工实测比机器判定准——有些问题日志里看不出来，使用时才知道。分工由此定死：**人工＝终审＋标记，机器＝预检员＋记录仪**（工具见下条）。
- 需要用户操作时按 **「操作请求卡」** 输出三项：① 请你做什么（进哪个界面 / 点哪个开关）；② 成功标志（界面上会看到什么）；③ 我接着看哪条日志或哪个文件。一次只请求最小一组动作，不连续追问。
- **装机预检闸门＋飞行记录仪（2026-10-04 定，`04_scripts/_common/`）**：装机后、用户上手前先跑 `bash 04_scripts/_common/post_install_precheck.sh --game <游戏>`——机器只管装机层证据（包在不在、启动崩不崩、菜单 so 加载、菜单弹出、授权墙发操作请求卡、空转不崩），**预检全过才请用户上手；预检挂了直接出 A/B 判定，不消耗用户实测轮**。游玩取证用 `flight_recorder.sh`：`start` 开录（logcat 连续＋`/proc` 周期采样）→ 用户发现问题时喊「标记」→ AI 立刻 `mark` 打时间戳（note 用用户原话）→ 事后 `slice` 按标记±窗口切证据落文件，按需切片读勿整读。游戏级标记落 `10_docs/{游戏}/预检标记.txt`（格式见脚本头注释，缺省用星光框架默认标记）。
- **验收阶梯 L1/L2/L3（2026-10-04 定，专治「改一次要两台设备各测一轮」）**：**同一个包**（同 so、同签名）走三层验收，**人工只出现在 L3**。
  - **L1 静态（AI，出包前，不碰设备）**：`bash 04_scripts/_common/check_release_clean.sh --tag <tag>` + 本地产物与包内 so md5 一致 + 菜单串自检。
  - **L2 机器（AI 全自动跑模拟器/任一可自动化设备，不打扰用户）**：装机 → `post_install_precheck.sh` → `flight_recorder.sh` 冷启录一段 → `device_profile.sh` 取三行自检，AI 自判 PASS/FAIL。**L2 失败不消耗用户的实测轮，也不打扰用户**；L2 通过同样**不构成**「要用户在真机再测一遍」的理由。
  - **L3 人工（用户，唯一终审）**：真机实测功能，AI 只发操作请求卡 + 收 logcat 定案。
  - 硬规则：**禁止为模拟器单独出包**（L2/L3 必须同一个包）；模拟器与真机差异一律落**运行时判据**，禁止编译期设备分叉（见 `hook-rules.md` §31）。
- **设备画像先行（2026-10-04 定，`04_scripts/_common/device_profile.sh`）**：凡涉及 `libil2cpp` 基址/hook 的游戏，装机前先跑 `bash 04_scripts/_common/device_profile.sh --serial <S> --pkg <游戏包名> --rva <名>=0x<HEX>...`，报告落 `10_docs/_state/设备画像/<serial>.md`（**对设备只读**，可随 git 跨机共享）。它给**静态预判**：ABI/页大小/root、`extractNativeLibs`、`ro.apk_verity.mode`、**libil2cpp 映射组数与选中基址判据**（与源码 `LibBaseResolve()` 同规则）、每个 RVA 的段权限/所属组/内存 vs 文件字节。退出码 0=正常、2=判据异常（**判据异常就不装机**）。画像只是预判，运行时真值仍以三行自检为准。

## 证据与信息增益预算

- 区分实测事实、文档历史记录、证据支持的推断与待验证假设；未经实测的判断标「待验证」（沿用旧文档标记亦可），注明来源和适用版本，不把假设当事实。
- 收集、判断、验证可迭代；在授权范围内以低成本可逆实验补证，不要求先证明运行效果才允许测试。
- 每条探索路线明确问题、已有证据、预期新增信息、成本与停止条件。重复无信息增益时换方法或收窄范围，不以固定两方法、三步骤、五分钟强制停工/询问。
- 读取状态和管线时按最小切片：状态只查目标游戏摘要；`pipeline.json` 只查目标
  stage，不把全量状态和全量日志重复灌入上下文。
- **读取体量纪律**（按体量选手段，别把 MB 级文件灌进上下文）：
  - KB 级可整读：`10_docs/_state/games/{游戏}.md`、`skill/**/*.md`、`04_scripts/{游戏}/*.sh`。
  - MB / KB-heavy 级禁整读：`06_dumped/**/dump.cs`、`06_dumped/**/meta/dump_all.txt`、`06_dumped/**/meta/types.txt`、`pipeline.log`、原始 logcat、`_state/current.md` —— 一律用 `rg -n -m <N>` 或 `sed -n 'A,Bp'` 按行切片；metadata 全量清单（单份常 10 MB 级）禁整读，只按 TYPE/类名片段取值。
  - 禁止 `cat 04_scripts/{游戏}/pipeline.json`（会把历史阶段一起灌入）；只取目标 stage。
  - `git status` / `git diff` 必须带游戏 pathspec，禁止裸跑后自行过滤。
- 预算将超出、没有可区分假设的下一步、出现新风险/越权或确需用户决策时，汇报已知/未知、已试方法、剩余选项并请求决定；能由数据验证的偏移/地址候选不让用户代猜。
- 阶段性汇报结论与下一步；无证据支撑的经验留在候选，证据齐即按「状态索引与经验生命周期」的自动门槛合并，不等审批。

## 状态索引与经验生命周期

- `10_docs/_state/current.md` 只保留跨游戏摘要索引：每个游戏一行，包含最近更新、
  粗粒度状态和 `games/{游戏}.md` 链接；长结论、日志摘要、hash 和下一步写入
  `10_docs/_state/games/{游戏}.md`。执行任务收尾或卡点变化时先更新游戏摘要，
  再同步索引的一行，不把详细内容堆回索引。
- **索引行硬上限**：`current.md` 每个游戏行 ≤ 400 字符（最近更新 + 一句状态 + 链接），超出部分必须写进 `games/{游戏}.md`；该文件整体目标 < 15 KB。单游戏摘要文件 > 约 12 KB 时，把旧轮次下沉到 `10_docs/_state/history/`，当前摘要只留最近 1–2 轮。
- 单一功能反复反馈、跨会话续查或证据日志较多时，使用
  `10_docs/_templates/功能验收卡.md`。会话收尾的复盘与规范优化提案使用
  `10_docs/_templates/会话复盘模板.md`（六节：实际动作表 / token·时间黑洞 top-N / 已验证可复用事实 / 违规复现表 /
  P0～P2 提案（每项必带 `[落点]`）/ 落地状态回填表）。`evidence_日期/` 目录优先放
  `README.md`，只列决定性证据、最短复现命令和 NOT PROVEN 项；不要让后续会话
  默认通读完整日志。
- 纯评估、规则/文档维护且未改变游戏执行状态时，不改游戏状态或索引；用户指定不碰的游戏、文件与产物保持不动。
- 经验状态使用「候选 / 待验证 / 已合并 / 已否定 / 已过时」（卡 frontmatter 词汇沿用 candidate/approved/merged/rejected/stale；`approved` 语义＝证据齐＋落点明确、已自批待合并）：记录来源、适用条件、证据与下一步。
- **合并自动门槛（2026-10-04 起，不再等用户审批）**：证据齐（实测行或反证排除）＋落点明确（目标规则文件＋节）即由优化会话直接合并，并在 `10_docs/_state/pending_experience.md` 登记目标位置与提交；证据或落点不齐留候选，不腐烂也不自动提升。已否定需反证；已过时需版本/适用条件变化依据。会话开始只查看与任务相关的候选。

## 技术与清理入口

- 动态分析与工具选择见 `WORKFLOW.md` §5；Hook 技术约束（调原函数、共享入口过滤、主线程与对象访问等）见 `skill/re-implement/hook-rules.md`，本文件不复制技术正文。
- 分类目录与例外见 `TOOLBOOK.md`「工作区目录结构规范」，包括 `01_tools/`、`99_archive/`、`04_scripts/_common/`、`10_docs/_templates/` 与状态索引 `10_docs/_state/`。
- **菜单模板 JSON 格式已定死，不再逐次询问用户**：照抄 `10_docs/_templates/menu_template.example.json` —— 顶层只有 `template_id`/`name`/`package`/`source`/`features`，每个 feature 只有 `feat_num`/`name`（纯功能名，不带 `Slider_`/`Toggle_` 前缀与参数）。落 `10_docs/{游戏}/{id}_menu_template.json`，并在收尾时用 `10_docs/_templates/菜单模板_规范与示例.md` §3 的脚本自检字段集严格相等；`version`/`note`/`type`/`min`/`max`/`default` 等一律不加。
- `02_apks/` 原包由用户手动管理；AI 不默认删除、整理或覆盖，操作须有指定文件的具体授权。普通清理清单排除整个 `02_apks/`。
- **`02_apks/` 与 `08_output/` 的内容由用户手动增删，AI 不可控、也不可预判（2026-09-21 起）**：
  - **开工时等用户声明**该游戏当前是否有原版包 / 星光包 / 上一轮正式包，把声明作为本次会话的输入边界；用户没说就先问一句，不要靠扫目录推断，也不要因为"上次文档写着有"就假定还在。
  - 禁止为确认这两个目录是否存在而连续探测（`ls`/`find`/`md5` 逐个试）；用户声明即事实。
  - 文档与游戏摘要里记录的包路径/hash 一律是**历史记录**，标注"用户可能已手动删除"；任何出包/装机步骤在动手前先确认当前是否有可用底包，缺底包就报告并停下，不自动重下、不自动替换。
  - 需要底包而用户未提供时，优先询问用户提供；若设备上还装着官方原版，可只读拉取 `base.apk` 当临时底包。**APK 一律不留本地（2026-09-30 定）**：官方包 / 渠道包 / 上一轮星光包都能重新下载或从设备只读拉取，属可再生输入；用完即删，只在 `10_docs/_state/底包再获取台账.md` 登记「包名 + 版本 + 体积 + md5/sha256 + 再获取路径」，不保留 `.apk` 文件。唯一例外：设备已被星光包覆盖、官方包再也拉不回时，可**当轮**申请保留一份并写明理由。
  - **热更样本不属不可再生**（2026-09-30 定）：官方 CDN/热更目录下发的 dll/js/差分清单可由游戏重新下发或 DIAG 重取，可删，只保留当前构建链直接引用的那一份。
- `08_output/` 默认按用户管理，不自动整理；但当用户明确执行「收尾+正式包+清理+git」口令时，应将该次游戏的 `08_output/{游戏}` 裁剪为**唯一正式最终 APK**，使用 `04_scripts/_common/prune_output_final.sh` 保留指定包并记录被删清单，删除前校验签名与 hash。单独出包、普通清理或文档任务不得推定此项授权。
  - **收尾一律走一键脚本，禁手工重造（2026-10-02 消消英雄2 定）**：动手前先读
    `04_scripts/_common/stages.md` 的收尾节，然后跑
    `bash 04_scripts/_common/finalize_game_delivery.sh --game {游戏} --apk {正式包} --prune --sync-doc --apply`
    —— `--prune` 裁剪为唯一正式包、`--sync-doc` 打印「粘到游戏摘要 + `current.md`」的同步片段。
    **icon / 菜单模板是按需交付物（2026-10-05 用户定，禁每次收尾都生成）**：只有「新游戏首次交付」
    （`08_output/{游戏}` 下还没有 `delivery_*` 目录）或用户当轮明确说「图标和菜单模板」时，才追加
    `--icon-source {原图} --menu-template 10_docs/{游戏}/{id}_menu_template.json`；其余收尾一律不传，
    脚本默认跳过并**不创建 delivery 目录**（首次交付未传时脚本会打 NOTE 提醒）。
    （历史浪费：2026-10-02 手工 PIL 放大 icon、手调 `prune_output_final.sh`、手写文档行 —— 三件都是它的本职。）
  - 口令中的 **「git」= 白名单提交 + 推送 origin/main 连体**（`archive_scoped_git.sh` 默认已推送），不需要再问「要不要推」；仅当用户明确说不要推时才 `--no-push`，并在汇报里写明「已提交未推送 + 远端落后 N 个提交」。push 冲突/网络失败保留本地提交并报告，不自动强推。
  - 收官汇报必须显式写出 git 结果：提交号 + 是否已推送 + 远端是否同步（`git status -sb` 无 `ahead`）。
- `10_docs/` 是复现入口，不是源码替代；保留小源码、自写脚本、配置规则、密钥与必要输入，按可再生性逐项判断，不按目录整删。本次规范修改授权不等于任何产物删除授权。
- **构建产物不留本地（2026-09-30 定，2026-09-30 加强）**：NDK 中间件与暂存 `.so`（`mod菜单/**/app/src/**/obj_*`、`libs_*`、`jniLibs`、`app/src/**/libs`）、Gradle 输出（`mod菜单/**/build/`、`.gradle/`、`.gradle-home/`）、.NET 输出（`04_scripts/**/{bin,obj}`、`06_dumped/**/{bin,obj}`）、脚本生成清单/缓存（`apk_entries.txt`、`rva_map.pkl`）都由源码 + 工具重建，收尾时清理，既不留在工作区也不入库。**唯一例外**：当前构建链直接引用的那一份（如 `jni/targetfuntional_dll*.bin`、`Games/WxsjPatchedDll*.a`、`Games/WxsjVc1637PatchedDll.a`）保留到该游戏收尾。
- **dump 入库默认收紧（2026-09-28 起，2026-09-30 加强）**：默认**不把 `06_dumped/**/{dump.cs,script.json,stringliteral.json,il2cpp.h,DummyDll}` 入库**（单份 dump.cs ~60 MB、script.json ~69 MB；曾使 `.git` 膨胀到 279 MB），改为入库「锚点摘录」（被引用的类/方法名 + RVA/file offset + 证据行）。`.gitignore` 已硬排除这些大产物。**2026-09-30 已授权重写历史**：`git filter-repo` 清除历史大 blob；此后新增大 dump 一律不入库。**JS 解密产物同样按大 dump 处理（2026-09-30 追加）**：`06_dumped/**/*.js`（热更/主包解密出的明文 JS，单轮常达 10 MB 且每次重抓整片更换）默认不入库，`.gitignore` 已硬排除；复现依据改为入库 `decrypt_summary.json` 锚点（含 key/count/逐文件尺寸）+ 重抓与解密脚本，确需入库时用 `git add -f` 并在当轮说明理由。
- 真机会话结束/暂停/切换时先回收证据，再按设备 serial 和本次变更处理残留，不影响其他会话；无设备操作不清设备。会话清理、基于修改前快照的环境恢复、深度修复分开授权与记录。
- 不默认运行 `04_scripts/_common/cleanup_device_re_residue.sh`：现有脚本有全局副作用，dry-run 非完整预演，且未实现新会话协议；范围无法约束时禁止默认跑，报告待处理项。工作区保留/清理、设备职责及执行记录统一见 [TOOLBOOK.md](TOOLBOOK.md)「清理规则」「真机残留清理」。

## 审查与分阶段门禁

- 改完代码或用户要求「按边界审 / 自查」时，按 `skill/re-implement/code-review-checklist.md` 走一轮：①改动范围 → ②四条闪退红线 → ③按改动类型查维度 → ④证据门禁。纯文档修改核对范围、链接、规则冲突与授权，不假装完成代码/运行测试。
- 安装前的「可测试」门禁核对静态/构建检查、包结构与签名、测试授权、证据采集和恢复方案；已知阻断问题不得装机。只能通过运行获得的效果证据可标 NOT PROVEN，在前置检查通过后受控测试。
- 「运行验证」以目标版本实际日志、功能开关与回归结果判定；「可交付」须满足交付范围对应的运行证据，不以构建成功或可测试替代可交付。缺证据列 CLAIM / VERDICT / EVIDENCE 与补证动作，授权内可自行补证，否则询问用户。
- **阶段闸门：环境阶段与功能阶段分离（2026-10-01 定）**。用户说「先过环境，不要直接做功能」时的真实含义是**别做与「能正常进游戏」无关的任何分析**——不是仅仅「先别写 hook」。开工先列出本阶段的**最小充分集**（见 `skill/re-analyze/environment-min-path.md`）并声明「以下分析推迟到第二阶段」，然后只做这几件事。**未收到用户明确的「开始做功能」口令前，禁止启动**：IL2CPP 符号恢复 / RVA 定位 / metadata 类型与字段枚举 / ABI 分析 / 为玩法 hook 做的任何脱壳派生分析。违反的代价是拿不到任何环境交付物却吃掉整段时间（历史事故：失落城堡环境阶段顺手做了大块静态 IL2CPP 定位，两次工具全失败，最终仍回到按名 Hook）。
- 一轮输出结论即停，不借审查扩大任务；仅修已确认问题，修复后复查该问题与直接影响面，必要的直接回归不可省略，不重启无边界全仓审查。

## 执行纪律补充（2026-10-03 规范优化全量落地）

> 来源：冒险与征途 1.5.9 两会话复盘（`10_docs/_state/规范优化_20261003_冒险与征途1.5.9会话复盘.md`）。
> 本节只放**机器可执行/可审查**的增量；技术正文在 `skill/re-implement/hook-rules.md`（§22 五步门禁、§29.5 三段判定、§30 模拟器装钩判据）。

- **硬约束与路线锁定落盘**：用户下达「必须 / 禁止 / 照 XX 做」时，**当轮**抄其原话（带消息号）进 `10_docs/{游戏}/硬约束.md`（模板 `10_docs/_templates/硬约束.md`）；有对照包的移植必须维护 `10_docs/{游戏}/路线锁定.md`（模板 `_templates/路线锁定.md`），表内存在「未确认偏离」即禁止装机。约束文件**永不删行**，失效只改状态。
- **迭代轮次门禁（P4，已并入 pipeline.sh 本体）**：同一功能「改代码 → 装机 → 用户实测」第 3 轮仍无效果时**拒绝再装机**：`bash 04_scripts/pipeline.sh <游戏> round <功能#> <有效|无效|未测> "摘要"` 记轮次，含 `install` 的阶段会被**直接拒绝（退出码 3）**并打印假设矩阵，先跑 `rounds` 把矩阵（已排除的假设+证据 / 待验证的假设+验证方法 / 已知确定的事实）填完贴给用户确认方向；仅当用户当轮明确要求再试一次才用 `STAGE_NEXT_FORCE=1` 放行。台账落在 `10_docs/{游戏}/轮次台账.tsv`。绕圈不是勤快，是烧用户的测试时间。
- **管线原生内循环（P5，2026-10-03 起）**：`bash 04_scripts/pipeline.sh <游戏> status`（钉硬约束生效条目 + 台账摘要）/ `next --loop N`（连跑最多 N 个阶段，逐次只回显日志尾 40 行，失败即停）/ `log [N]` / `round` / `rounds` / `gate <阶段id>`。旧命令 `bash 04_scripts/_common/stage_next.sh <游戏> [status|next|auto N|rounds]` 保留但**已缩成同义转发器**（不再自己实现逻辑）。**pipeline.sh 是弱 AI 禁改文件**（指纹锁 `_common/pipeline.lock`）；强 AI 确需改时唯一姿势＝同一提交改脚本 + 刷新 lock 的 `shasum -a 256` + push，另一台 `git pull --rebase --autostash`（见 `_common/stages.md` 文末「真要改 pipeline.sh 时的跨机姿势」）。
- **会话规模预算（P6）**：单个会话的机械行为预算 —— 工具调用 ≈600 次、装机 ≈40 次、prune 阴影 token ≈150K。任一触顶即**交接**：把「已确认事实 / 未决问题 / 下一步 / 证据落点」写进 `10_docs/_state/games/{游戏}.md`，然后开新会话，不硬撑（旧会话实测 2428 次调用 / 380K 阴影 / 99 条用户消息，后段全靠压缩维持）。
- **清理禁通配符（P9）**：删除操作**禁止**用通配符批量匹配（`_*.py`、`Remove-Item ...\[a-z]*`、`rm -rf <dir>/*`）；一律写显式名单，先打印解析出的绝对路径复核，删后复扫一次。历史事故：`Remove-Item '...\06_dumped\冒险与征途\_*.py'` 误删 16 个已入库 tracked 脚本。
- **IL 查询一次到位（P7）**：`06_dumped/{游戏}/aot/_tgt_q.py dump-class <类名正则>` 一次打印「类头 + 字段（含偏移口径）+ 方法（token/RVA）+ 关键方法 IL 摘要」，避免为同一个类反复跑 `fields`/`methods`/`il`/`iluse` 三四次（旧会话同一 IL 查询 ≈40 次）。
- **诊断三段判定**：功能报无效时先写结论行 `判定段=A|B|C 依据=<证据行>` 再动代码（A 上游没发生 / B 装上了没命中 / C 命中了下游不接），做法见 `hook-rules.md` §29.5。
- **诊断与最可能修复同包（2026-10-04 定）**：证据已能锁定最可能根因时，**同一轮就把「诊断计数器 + 最可能修复」打进同一个包**装机，禁止先出「只加日志」的诊断包、再出修复包——每次装机都在烧用户的实测时间。范例：冒险与征途 v46b 一次装机同时拿到「命中 免广告=25267/s ⇒ 自激环」的证据与修复（一次性事件门），一轮定案。
- **一轮一个变量、一轮只问一次**：同一轮只改一个自变量（多改则失败无法归因，也违反 P4 轮次门禁）；需要用户配合时一轮只发一张操作请求卡，不连续追问。

### 增量（2026-10-10 邂逅在迷宫会话复盘全量落地）

> 来源：`10_docs/_state/规范优化_20261010_邂逅在迷宫会话复盘.md`。该会话 4.4k 条消息、9 版装机包、
> 11.4 GB 死路产物，根因**不是缺文档**（机制早就齐了），而是**机制被旁路**：本游戏既没有
> `10_docs/{游戏}/硬约束.md` 也没有 `pipeline.json`，于是纪律全靠对话记忆 → prune 一到就稀释。
> 下面每条都对应一个**可执行入口**，不是口号。

- **开工 preflight（P10）**：开工第一步 `bash 04_scripts/_common/session_start.sh <游戏> [--serial S] [--pkg P]`。
  六查 = 硬约束 / 路线锁定 / 台账（**打印"已否证=禁止重试清单"**）/ 管线出口 / TOOLBOOK 坑位 / 设备与预算。
  硬约束或台账缺失时脚本会**生成骨架并判 FAIL**（"生成空表"≠"抄了用户原话"）。
  **未跑 preflight，不得改功能代码、不得装机。**
- **判据先行 + 先例检索（P11）**：① 任何「客户端如何判定 / 机制怎么走」的结论，先写成**可判真假的判据 + 决定性证据行**（日志原文带时间戳）再动手，禁止"先改代码再看能不能跑"；② 动手前 3 分钟先例检索
  `bash 04_scripts/_common/prior_art.sh <关键词...>`（搜 `10_docs/ skill/ 04_scripts/ TOOLBOOK.md AGENTS.md`，
  全无命中时 rc=1 = 这是新问题可放心推）。命中先例先读先例；结论冲突时先解释冲突再推翻它。
- **决策台账 append-only（P12）**：每得出一个结论（成立/否证）**当轮**落一行进 `10_docs/{游戏}/台账.md`
  （模板 `10_docs/_templates/台账.md`，列 = 结论 | 决定性证据 | 状态 | 归属）。**「已否证」区 = 禁止重试清单**：
  除非写出新证据推翻它，否则不得重试。不写 = 下一个会话重推一遍（本会话 11.4 GB 死路产物的直接成因）。
- **方案选型卡（P13）**：命中「出网 / 改外部机器或用户资产 / 变更交付形态 / 需要用户做额外动作（点按钮、开权限、每次开局操作）/ ≥2 条代价不可比路线」任一条时，**先**填 `10_docs/_templates/方案选型卡.md`
  （≥3 案，含"不做"与推荐）**等用户选定再动手**；用户否掉推荐项后不得在执行时悄悄带回。
  历史事故：顺手动外部服务器、擅自把形态做成 root 版、把"需要电脑推包"当默认前提 —— 三次都是没让用户选。
- **一键出包链路（P14）**：gradle 助手包一律走
  `bash 04_scripts/_common/gradle_cycle.sh --tag vX --bump-version [--install] [--arm-service <组件> --watch N --logs '<ERE>']`
  （bump → assembleRelease → zipalign → apksigner → verify → 出包门禁 → 装机 →（可选）诊断自测抓日志），
  禁止再手敲 6~10 条命令（本会话同一链路手敲 9 遍，3 次因忘 bump/忘 verify 返工）。
  **出包门禁 `04_scripts/_common/check_release_apk.sh --apk <apk>` 默认不可跳过**：查 `application-debuggable`、
  查禁用串（`classes*.dex`/`assets/*`/`lib/*`；清单 = 内置 + `10_docs/{游戏}/出包禁用串.txt` + `--forbid`）、
  查 versionCode/versionName 与期望一致；native 侧另跑 `check_release_clean.sh`。
- **装机预检带宿主助手（P14-b）**：独立助手形态下，宿主助手的一次性 attach 日志（`native menu shown`）
  若落在录制窗口外会判成 FAIL_B 假阴性。用
  `bash 04_scripts/_common/post_install_precheck.sh --game <游戏> --helper-pkg <助手包名> [--helper-start] [--helper-delay 20]`
  —— 起录前先 force-stop 助手，再在窗口内拉起它。
- **门禁即时记账（P15）**：过任一阶段门禁（G1 静态 / G2 机器 / G3 人验）**当轮**记一行
  `bash 04_scripts/_common/gate_note.sh <游戏> <G1|G2|G3> <通过|失败|未验证> "<证据一句话>"`，
  落 `10_docs/{游戏}/门禁台账.tsv`（只追加）。收尾时**不许靠回忆补门禁表**，也不许把"未验证"当"已通过"。
- **坑位即落（P16）**：踩到命令级坑（引号/路径/权限/工具行为/端序/表遍历）当轮在
  `TOOLBOOK.md`「二-b、本机 / 真机坑位」补一行（场景 | 坑 | 正解）。开工 preflight 会回读该表。

## 工具专属文件

工具规则/技能入口只放指向本文件及具体技术来源的短指针，保留必要 frontmatter 与匹配条件，不复制政策或操作正文；不借文档优化修改 MCP/JSON 配置。工具列表、版本、路径、端口和连接状态是历史观测，使用前按需核验，不作实时事实。
