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
- `skill/` 下任何修改、新增、删除仍须用户明确授权；授权仅覆盖指定范围。**当轮授权记录**：2026-10-03「规范优化全量落地（P1–P9）」授权修改 `skill/re-implement/hook-rules.md`（§22 扩第五步、新增 §29.5 与 §30）；不含其它 skill 文件，也不代表以后可自行修改。
- 需要用户操作（进游戏、开菜单、打开工具等）时在聊天说明操作、成功标志和所需配合，等待确认并核对结果；只暂停依赖该结果的步骤，不假定用户已操作，独立且已授权工作可继续。
- **真机交互零截图（2026-10-01 定）**：不做 `screencap` 猜坐标、不 `input tap`、不靠截图判断界面状态 —— 真机取证一律走 `logcat`、`/proc/<pid>/{mem,maps}`、frida。屏幕上的点按、输入、翻页**全部由用户完成**。（历史浪费：为算 `input tap` 坐标花掉 `wm size` + `screencap` 的调用，最终未用上。）
- 需要用户操作时按 **「操作请求卡」** 输出三项：① 请你做什么（进哪个界面 / 点哪个开关）；② 成功标志（界面上会看到什么）；③ 我接着看哪条日志或哪个文件。一次只请求最小一组动作，不连续追问。

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
- 阶段性汇报结论与下一步；未经审批的经验只作为候选，不自动提升为规则。

## 状态索引与经验生命周期

- `10_docs/_state/current.md` 只保留跨游戏摘要索引：每个游戏一行，包含最近更新、
  粗粒度状态和 `games/{游戏}.md` 链接；长结论、日志摘要、hash 和下一步写入
  `10_docs/_state/games/{游戏}.md`。执行任务收尾或卡点变化时先更新游戏摘要，
  再同步索引的一行，不把详细内容堆回索引。
- **索引行硬上限**：`current.md` 每个游戏行 ≤ 400 字符（最近更新 + 一句状态 + 链接），超出部分必须写进 `games/{游戏}.md`；该文件整体目标 < 15 KB。单游戏摘要文件 > 约 12 KB 时，把旧轮次下沉到 `10_docs/_state/history/`，当前摘要只留最近 1–2 轮。
- 单一功能反复反馈、跨会话续查或证据日志较多时，使用
  `10_docs/_templates/功能验收卡.md`。会话收尾的复盘与规范优化提案使用
  `10_docs/_templates/会话复盘模板.md`（五节：实际动作表 / token·时间黑洞 top-N / 已验证可复用事实 /
  P0～P2 提案（每项必带 `[落点]`）/ 落地状态回填表）。`evidence_日期/` 目录优先放
  `README.md`，只列决定性证据、最短复现命令和 NOT PROVEN 项；不要让后续会话
  默认通读完整日志。
- 纯评估、规则/文档维护且未改变游戏执行状态时，不改游戏状态或索引；用户指定不碰的游戏、文件与产物保持不动。
- 经验状态使用「候选 / 待验证 / 待审批 / 已合并 / 已否定 / 已过时」：记录来源、适用条件、证据与下一步。待审批可保留，不因跨会话而强制合并或废弃。
- 「已合并」需用户批准及目标规则链接；「已否定」需反证；「已过时」需版本/适用条件变化依据。会话开始只查看与任务相关的候选，需改 skill 时按授权规则办理。

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
    `bash 04_scripts/_common/finalize_game_delivery.sh --game {游戏} --apk {正式包} --icon-source {原图} --menu-template 10_docs/{游戏}/{id}_menu_template.json --prune --sync-doc --apply`
    —— 它一次生成 `08_output/{游戏}/delivery_日期/`（icon 512/144 + 菜单模板副本 + 字段自检）、
    `--prune` 裁剪为唯一正式包、`--sync-doc` 打印「粘到游戏摘要 + `current.md`」的同步片段。
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

## 工具专属文件

工具规则/技能入口只放指向本文件及具体技术来源的短指针，保留必要 frontmatter 与匹配条件，不复制政策或操作正文；不借文档优化修改 MCP/JSON 配置。工具列表、版本、路径、端口和连接状态是历史观测，使用前按需核验，不作实时事实。
