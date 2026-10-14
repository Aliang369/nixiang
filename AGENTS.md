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
   - **先例检索（仓内 → 仓外）→ `bash 04_scripts/_common/prior_art.sh <关键词>`；仓内零命中（rc=1）⇒ 再按 `skill/diagnosis-index.md`「外部检索协议」≤3 次外部检索，结果落 `10_docs/_state/外部方案台账.md`**
   - 纯评估 / 文档维护 → 只读待评估文档及直接引用，不自动进入游戏执行流程。
2.5 **动手前先查「现成能力」**（2026-10-02 消消英雄2 教训，最省 token 的一条）：
   `ls 04_scripts/_common` + `grep -rn "<关键词>" skill/ 04_scripts/_common/`，
   命中就直接用，**禁止重造**。已存在且被重复造过的例子：`bash32_check.sh`（bash 3.2 三坑机检）、
   `lua_struct_check.py`（内嵌 Lua chunk 体检，`--disasm` 可反汇编运行时 dump）、
   `finalize_game_delivery.sh`（收尾交付一键）、`prune_output_final.sh`、`stage_shared_file.py`、
   `pre_install_gate.sh`、`check_release_clean.sh`；技能侧先读 `skill/SKILL.md` 索引再决定自研。
- **入口文件边界（2026-10-15 用户 m00006 定）**：本文件**只放通用规则与路由**。判断标准一句话：**这条规则对所有游戏都成立吗？** 不成立 ⇒ 写进 `10_docs/{游戏}/`（`硬约束.md` 管「必须／禁止／照 XX 做」、`路线锁定.md` 管挂点对照、`_state/games/{游戏}.md` 管状态与结论），本文件只在路由里指过去；通用规则的**来源游戏**只留一句出处，不展开细节（细节进 `10_docs/_state/规范变更日志.md`）。
3. 流程细节按需读 `PLAYBOOK.md`（接手）、`WORKFLOW.md`（日常）、`TOOLBOOK.md`（工具/目录），不是串行必读链。
4. 多代理协作先划定文件归属和禁止修改项；不覆盖他人改动，跨归属改动先协调，最终汇总改动与证据。
5. 外部方法论引用已收敛（2026-09-10）：原 `/Users/aliang/Documents/reverse-skill` 外部库经评估整体与本工作区低相关，唯一有价值的 ida-pro-mcp 工具速查已择录进 `skill/ida-plugin-flow/references/ida-mcp-cheatsheet.md`，外部库本体已删除（可从 github.com/zhaoxuya520/reverse-skill 重新克隆）。不再引用外部路由，一切以本入口与 `skill/` 为准。

## 双机协作（每台机器，每轮开工第一步）

本仓由 **mac 端**与 **Windows 端**共用同一个仓库（`git@github.com:Aliang369/nixiang.git`，`main`）。**任何机器上的任何 AI，接手任务后的第一步先做本节。**
细则（终端分流、兼容垫片、Syncthing 大头层同步、验收清单、跨机冲突姿势、心跳字段）已归档到 `10_docs/_state/规范变更日志.md` §归档正文 F。

```bash
cd "$NIXIANG_ROOT"                 # Win 端在 Git Bash 里；mac 端在 zsh 里
bash 04_scripts/双机协作/twin_status.sh   # 双机体检：有 FAIL 先修再开工（同时自动重写本机心跳）
git status -sb                     # 必须干净；不干净先报告处理，不要 pull
git pull --rebase --autostash      # 拿到另一台机器已推送的提交
git log --oneline -5               # 确认拿到了什么
sed -n '1,6p' "10_docs/_state/机器心跳/win.md" 2>/dev/null    # pull 后看对方心跳（win 端则看 mac.md）：超 7 天未更新或含 FAIL → 先提醒对方复检
```

- **顺序不可颠倒**：先 `status` 再 `pull`。工作区不干净时 pull/rebase 会把未提交改动卷进去。
- **一机一游戏**：开工前先划定归属并 `bash 04_scripts/双机协作/claim_game.sh <游戏>`（收工 `--done` 释放，占用表 `10_docs/双机协作/本轮归属.tsv`）；pull 后发现对方已认领未释放的游戏禁止写。
- **状态索引单一写者**：`10_docs/_state/current.md` 与 `games/{游戏}.md` 由本轮动了该游戏的那台机器写，另一台只读。跨机接手先核对 `games/{游戏}.md` 的「接手重建：」行（dump 产物不入库，按历史路径找会扑空）。
- **push 一律经 `push_main`**（`source 04_scripts/_common/git_push_main.sh && push_main main`；v2.1 四通道自愈，**禁裸 `git push` 盲等、禁 `--force`**）；被拒 ⇒ `git pull --rebase --autostash` 再推；收工走 `archive_scoped_git.sh` 白名单 add（**禁 `git add -A`**）。

## 授权与测试边界

- 以用户明确指定的目标、文件、设备、账户、版本和测试范围为边界；本地阅读、检索、构建、离线测试及范围内低成本可逆探索无需逐次请示。真机安装、运行、抓取仅在相应设备与测试范围已授权时开展；文档任务不自动授权真机操作。
- 破坏性操作、清数据、卸载、不可逆覆盖、影响其他游戏/设备/账户、外部发布/分发/上传或服务端写操作，须先明确说明具体范围、影响与恢复方案并取得确认；已有具体授权覆盖时不重复询问，不从「修好/重装/出包」推定这些权限。
- 游戏 APK 默认外科 zip 组装；`apktool b` 游戏整包重建例外须取得针对该游戏/版本/改动的具体授权，并提供原包、外科方案与例外方案的对照证据。模板构建细节见 `WORKFLOW.md` §4；未验证例外不得当作默认路径或正式交付。
- 不默认清数据或卸载；大资源游戏优先覆盖安装。异签名导致无法覆盖时先报告，不自动转为卸载。**游戏级例外一律不写在本文件**：任何游戏级例外（含「必须卸载重装」这类）只写在该游戏 `10_docs/{游戏}/硬约束.md`，本文件只承认「按该文件逐条生效、只对该游戏有效、不得外推」这一条通用规则（现存先例：`10_docs/无悔华夏/硬约束.md` 第 4 条；其余按 `TOOLBOOK.md` 规则 #77 三档）。
- **数据备份默认不做**：未当轮明确要求时，禁止创建 `09_test/**/backup_*` 或任何存档/数据备份，`install_*` 脚本默认不带备份步骤；「修好 / 重装 / 出包」不构成备份授权。
- **功能边界默认不扩张**：只实现用户当轮点名的功能与菜单项，禁止以「顺手 / 顺便 / 反正要做」为由新增未点名的功能（事故出处见 `10_docs/_state/规范变更日志.md`）。收尾时把 `10_docs/{游戏}/{id}_menu_template.json` 的功能集与用户点名单逐项对照，多一项即视为越界。
- **规范/技能优化全自动（2026-10-04 用户定「不审批」；2026-10-15 m00005 定框架层范围）**：以规范优化 / 复盘 / 文档维护为任务的**优化会话**可直接修改 `skill/`、`AGENTS.md`、`10_docs/_templates/`、`04_scripts/_common/`（`pipeline.sh` 仍走指纹锁跨机姿势）、`mod菜单/**` 的**文档与游戏私有脚本**（`surgical_<游戏>_*.sh`／`*_so_only.sh`／`Games/<Tag>Mod.*`；`Main.cpp`／`MainModern.cpp`／`GameConfig.h`／公共 `Android.mk`／`SignSpoof.java` **仍禁改**），无须逐次请示；每笔改动必须 ① 按适用维度自查 `skill/re-implement/code-review-checklist.md` ② 跑 `04_scripts/_common/preflight.sh` ③ 在「规范变更日志」追加一行（日期＋文件＋改动＋依据）。**游戏执行会话（拆包/hook/装机等日常任务）仍禁改上述文件**——撤的是审批流，不是防线。日志供用户抽查，不阻塞执行。
- **规范变更日志不在本文件（2026-10-15 用户 m00006 定）**：日志正文（只增不删，含日期／依据消息号／文件／改动／落点）**全部**在 `10_docs/_state/规范变更日志.md`；本文件不留任何日志条目，只留这一行指针。每笔规范改动仍必须在那里追加一行（见上一条 ③）。

- 需要用户操作（进游戏、开菜单、打开工具等）时在聊天说明操作、成功标志和所需配合，等待确认并核对结果；只暂停依赖该结果的步骤，不假定用户已操作，独立且已授权工作可继续。
- **真机交互零截图（2026-10-01 定）**：不做 `screencap` 猜坐标、不 `input tap`、不靠截图判断界面状态 —— 真机取证一律走 `logcat`、`/proc/<pid>/{mem,maps}`、frida。屏幕上的点按、输入、翻页**全部由用户完成**。（历史浪费出处见 `10_docs/_state/规范变更日志.md`。）
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

## 技术与清理入口（正文已归档：`10_docs/_state/规范变更日志.md` §归档正文 D）

- 动态分析与工具选择见 `WORKFLOW.md` §5；Hook 技术约束见 `skill/re-implement/hook-rules.md`；目录分类见 `TOOLBOOK.md`「工作区目录结构规范」。
- **动手前先查「现成能力」**：`ls 04_scripts/_common` + `grep -rn "<关键词>" skill/ 04_scripts/_common/`，命中就直接用、禁止重造。
- **菜单模板 JSON 格式已定死**：照抄 `10_docs/_templates/menu_template.example.json`（顶层 `template_id`/`name`/`package`/`source`/`features`；feature 只 `feat_num`/`name`）；
  落 `10_docs/{游戏}/{id}_menu_template.json`，收尾按 `10_docs/_templates/菜单模板_规范与示例.md` §3 自检字段集严格相等，不逐次询问用户。
- **输入与产物生命周期（保留/清理/可再生判断）**：`02_apks/`、`06_dumped/` dump、`08_output/`、构建产物、热更样本、JS 解密产物的
  **保留与否、清理授权、入库规则**全在 §归档正文 D（含「APK 不留本地」「dump 不入库」「`02_apks`/`08_output` 由用户手动管理、开工等用户声明」等硬条目）；
  收尾裁剪唯一正式包走 `04_scripts/_common/prune_output_final.sh` / `finalize_game_delivery.sh`，**禁手工重造**。
- **收尾与 git**：口令「收尾+正式包+清理+git」= 白名单提交 + 推送 origin/main 连体（`archive_scoped_git.sh` 已接线 `push_main`，不需追问「要不要推」）；
  收官汇报必须写出提交号 + 是否已推送 + 远端是否同步。真机残留回收与设备清理见 §归档正文 D。

## 审查与分阶段门禁（正文已归档：`10_docs/_state/规范变更日志.md` §归档正文 E）

- 改完代码或用户要求「按边界审 / 自查」⇒ 按 `skill/re-implement/code-review-checklist.md` 走一轮（①改动范围 → ②四条闪退红线 → ③按改动类型查维度 → ④证据门禁）；纯文档修改只核对范围/链接/规则冲突/授权。
- 装机前「可测试」门禁：静态·构建检查 + 包结构与签名 + 测试授权 + 证据采集 + 恢复方案；**已知阻断问题不得装机**。
- 「运行验证」以目标版本实际日志、功能开关与回归结果判定；「可交付」须满足对应运行证据，不以构建成功替代。缺证据列 CLAIM / VERDICT / EVIDENCE。
- **阶段闸门（2026-10-01 定）：环境阶段与功能阶段分离** —— 收到用户明确的「开始做功能」口令前，**禁止启动** IL2CPP 符号恢复 / RVA 定位 /
  metadata 类型字段枚举 / ABI 分析 / 玩法 hook 派生分析；先列最小充分集（`skill/re-analyze/environment-min-path.md`）并声明推迟项。
- 一轮输出结论即停，不借审查扩大任务；仅修已确认问题，修复后复查该问题与直接影响面。

## 执行纪律补充（2026-10-03 规范优化全量落地）

> 来源：冒险与征途 1.5.9 两会话复盘（`10_docs/_state/规范优化_20261003_冒险与征途1.5.9会话复盘.md`）。
> 本节只放**机器可执行/可审查**的增量；技术正文在 `skill/re-implement/hook-rules.md`（§22 五步门禁、§29.5 三段判定、§30 模拟器装钩判据）。

- **硬约束与路线锁定落盘**：用户下达「必须 / 禁止 / 照 XX 做」时，**当轮**抄其原话（带消息号）进 `10_docs/{游戏}/硬约束.md`（模板 `10_docs/_templates/硬约束.md`）；有对照包的移植必须维护 `10_docs/{游戏}/路线锁定.md`（模板 `_templates/路线锁定.md`），表内存在「未确认偏离」即禁止装机。约束文件**永不删行**，失效只改状态。
- **管线原生内循环（P5，2026-10-03 起；正文已归档）**：`bash 04_scripts/pipeline.sh <游戏> {status|rounds|next --loop N|log [N]|round <功能#> <有效|无效|未测> "摘要"|gate <阶段id>}`；细则（旧 `stage_next.sh` 同义转发、**pipeline.sh 是弱 AI 禁改文件**、指纹锁 `_common/pipeline.lock` 与跨机改法）见 `10_docs/_state/规范变更日志.md` §归档正文 C 与 `04_scripts/_common/stages.md` 文末。
- **会话规模预算（P6）**：单个会话的机械行为预算 —— 工具调用 ≈600 次、装机 ≈40 次、prune 阴影 token ≈150K。任一触顶即**交接**：把「已确认事实 / 未决问题 / 下一步 / 证据落点」写进 `10_docs/_state/games/{游戏}.md`，然后开新会话，不硬撑（旧会话实测 2428 次调用 / 380K 阴影 / 99 条用户消息，后段全靠压缩维持）。
- **P4 轮次门禁 / P7 IL 一次到位 / P9 清理禁通配符 / 诊断三段判定 / 诊断与最可能修复同包**：正文已归档 `10_docs/_state/规范变更日志.md` §归档正文 A（第一轮 P10–P16 节内）与 §执行纪律补充；要点：同一功能第 3 轮仍无效禁装机、`aot/_tgt_q.py dump-class <正则>` 一次取全、删除禁通配符须写显式名单、报无效先写 `判定段=A|B|C`、能锁定根因就「诊断计数器+最可能修复」同包装机。**新增（2026-10-15）：同一功能第 2 轮无效时强制一次外部检索**（≤3 次调用；通道与关键词模板见 `skill/diagnosis-index.md`「外部检索协议」，结果落 `10_docs/_state/外部方案台账.md`）。
- **一轮一个变量、一轮只问一次**：同一轮只改一个自变量（多改则失败无法归因，也违反 P4 轮次门禁）；需要用户配合时一轮只发一张操作请求卡，不连续追问。

## 会话效率纪律（D1–D7，2026-10-14 定；长正文在 `10_docs/_state/规范变更日志.md`）

> 依据：`10_docs/_state/规范优化_20261014_双会话效率复盘与可执行方案.md`（两会话全量统计）。
> **本工作区的成本单位是 step**：缓存命中率已到顶（96–98%），每多一个 step 就多一条 ≈82–84 K 缓存的整请求。
> 本机实测反例：640 / 556 次工具调用里 91.5% / 93.5% 的 step 只发 1 个调用；bash 占 70% / 68%，其中 73.6% / 75.3% 是用来 grep/sed/head 做文本检索；单个 `DkzjMod.cpp` 被触达 147 / 158 次。
> **凡「检索 / 读取」一律优先原生工具**（`grep` / `glob` / `read`），**不要把 bash 当检索器**；下面每条都给阈值，不做则本条视为违规。

- **D1 并行批次纪律（一次发多个独立调用）**：同一 step 内**下一步动作不依赖其返回**的调用必须合并发出。
  典型可合并组：① 侦察组（`pwd` + `ls` + `glob` + 读状态索引）；② 取证组（两个文件 `read` / 两处无关 `grep`）；③ 构建组（脚本 + 并行校验）。
  **阈值：calls/step ≥1.6、单发占比 <60%**（现状 1.09 / 91.5% 与 1.06 / 93.5%）。
- **D2 多代理按需（别把主线程的活外包）**：子代理只用于「可独立落盘的深度探查」；能一条命令/一次 `read` 拿到的结论自己拿。
  派发时按 D5 约定交付；**不派出「帮我统计 / 帮我找文件」类子代理**。
- **D3 大 dump / 大日志检索协议（专治 1,653,151 行的 dump.cs）**：对 `06_dumped/**/dump.cs`、`dump_all.txt`、`types.txt`、原始 logcat、`pipeline.log`
  **禁止无 `-m` / 无 `head` / 无 `sed -n 'A,Bp'` 的检索**；模板：`grep -n -m 20 -- '<符号>' <dump>`、`sed -n 'A,Bp' <file>`。
  **阈值：同一 dump 每轮 grep ≤10 次、单条工具结果 ≤8 KB**。
- **D4 硬约束三行回填 + 压缩后复读（专治「我让你修 A，你在干什么」）**：用户每下达功能类指令**当轮**先落三行进 `10_docs/{游戏}/硬约束.md`：
  ① 原话逐字（带消息号）；② 交付形态；③ 判定方式。**每次 `compress` 后动手前，第一动作固定为 `read 10_docs/{游戏}/硬约束.md`**（回读时机见 `10_docs/_templates/硬约束.md`）。
  **阈值：用户「你在干什么 / 我让你做什么」类质问 = 0**（两会话共 7 次）。
- **D5 子代理交付协议**：子代理回报**只允许**「① 结论 ≤20 行 ② 落盘路径 ③ 行数/md5 ④ 未闭环项」；全文落盘、父按段 `read`。
  禁止同一报告「回答 + settled 通知」两次全量入父上下文。**阈值：单条子代理消息入父 ≤3,000 字符**（现状 35,557）。
- **D6 入口文件体量闸门（本文件）**：`AGENTS.md` **≤32,768 B**，超了就是新会话拿不到规则（预算 65,536 B 一旦被其它工作区文件挤占，本文件整份丢弃）。
  新增长正文（>约 3 KB）一律追加到 `10_docs/_state/规范变更日志.md`，本文件只留一行指针（规则可下移，规则不得消失）。
  校验：`wc -c AGENTS.md` ≤32768，且新会话不再出现 `omitted AGENTS.md`。`TOOLBOOK.md`（≈140 KB）只读所需章节。
- **D7 压缩前置与开工节拍**：`compress` 触发点前移到**上下文 ≈90 K**（不再等 ~140 K 才动，现状 19/20 次全部贴峰值触发）；一个压缩条目只装一个主题。
  开工节拍固定为三读一次并发完成：`_state/current.md` 摘要行 → `_state/games/{游戏}.md` 当前摘要 → `10_docs/{游戏}/硬约束.md`
  （现状引用落差 8 倍：`games/蛋壳战机.md` 被读 34 次 vs 4 次）。
  **会话切点硬阈值（2026-10-15 补，霓虹深渊2 r19 实测）**：同一共享文件被触达 >10 次，或连续 3 个 step 单发，即先 `compress`／写交接再继续 —— 本轮峰值 774,719 tokens、`pipeline.sh` 触达 22 次、`pipeline.json` 23 次，D1/D3/D5 三项 FAIL 全出在后段。

**度量口（只读，D1/D3/D5 的阈值机检）**：`python3 04_scripts/_common/session_metrics.py --session-id <片段> --gate`
（`--list` 列会话、`--latest N` 合计、`--top N`、`--out <文件>`；违规 rc=3）。它直接输出 calls/step 分布、单发占比、bash 检索占比、
单条结果 >8 KB 条数、单文件触达 top-N、单条消息最大字符数与缓存命中率——**收官时跑一次当前会话，别凭感觉汇报效率**。

## 规则指针表（P1–P31；正文已归档，此处只留编号 + 一句话）

> 归档全文见 `10_docs/_state/规范变更日志.md`（2026-10-10 第一轮 / 第二轮 / 2026-10-12 三节）。
> 用 `grep -n '^- \*\*P2[0-9]' 10_docs/_state/规范变更日志.md` 取原文。

- P1–P9（2026-10-03 冒险与征途）：见本文件上方「## 执行纪律补充（2026-10-03 规范优化全量落地）」——**不归档，仍在本文**。
- P10–P16（2026-10-10 邂逅在迷宫第一轮）：开工 preflight 六查 `session_start.sh`（**P33 起为七查，新增查7 外部先例**）／判据先行 + 先例检索 `prior_art.sh` ／决策台账 append-only ＋「已否证=禁止重试」／方案选型卡 ／一键出包链路 `gradle_cycle.sh` ＋出包门禁 `check_release_apk.sh` ／装机预检带宿主助手 `post_install_precheck.sh` ／门禁即时记账 `gate_note.sh` ／坑位即落 `TOOLBOOK.md` 二-b。
- P17–P22（2026-10-10 第二轮，把纪律做成闸门）：自测影响面 before/after 快照 `selftest_env.sh` ／装机闸门（preflight 凭证 `preflight_{游戏}.stamp`，缺失·超 12h·硬约束变过 ⇒ **rc=4**，`--allow-stale-preflight` 才放行）／读文件纪律 `code_locate.sh`（>200 行禁整读）／出包即记账（全量日志 `tee`，**禁 `| tail` 截断**，自动 G2 记账）／默认分发型交付 `finalize_game_delivery.sh --usage-doc` ／push 只走 `push_main`（四通道自愈，**禁裸 `git push` 盲等**）／轮次台账 `轮次台账.tsv`。
- P32（2026-10-14 潮汐守望者复盘落地）：设备就绪门禁 `device_ready_check.sh` ／崩溃取证 `crash_forensics.sh` ／收官编排 `finalize_round.sh` ／假设预注册 `new_hypothesis.sh` ／超时垫片 `with_timeout.sh` ／装钩表前缀规则 R4-prefix —— 正文归档 `规范变更日志.md` §同批落地（2026-10-14 第二轮）。
- P33（2026-10-15 外部检索闸门）：先例检索两层（`prior_art.sh` 仓内 rc=1 ⇒ 外部检索 ≤3 次）／开工**七查**（新增查7 外部先例）／外部方案**采纳四问**（时效·痕迹·边界·重叠）／结论落 `10_docs/_state/外部方案台账.md`（候选 / 已评估·不接入 / 空结果）—— 正文归档 `规范变更日志.md` §归档正文 G；候选与空结果见台账。
- P23–P31（2026-10-12 霓虹深渊2）：编辑一把刀（同轮同文件禁混用 bash 改写与 `edit`）／大文档禁整读 `state_digest.sh` ＋ `_digest.md` ／轮次台账 6 列正统九列超集（`pipeline.sh <游戏> round` 为唯一写入口）／口令点名即必须交付（`--expect-icon` / `--expect-menu-template`，缺则 rc=1 不写文件）／`install_policy: overlay_only` ＋卸载语义 **rc=66** ／一键轮次环 `round.sh` ／取证收敛 `evidence.sh` ＋ `so_inspect.py`（`strings` 默认 `--utf8`）／会话切点（每 2 个用户实测轮或 3 h / 600 次调用即交接）／门禁全仓入口 `preflight.sh` 无参扫描（638 文件 ≈24 s）。

## 工具专属文件

工具规则/技能入口只放指向本文件及具体技术来源的短指针，保留必要 frontmatter 与匹配条件，不复制政策或操作正文；不借文档优化修改 MCP/JSON 配置。工具列表、版本、路径、端口和连接状态是历史观测，使用前按需核验，不作实时事实。
