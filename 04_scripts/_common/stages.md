# 机械操作管线 — 标准阶段定义（stages.md）

> 配套统一入口 `04_scripts/pipeline.sh`。本文定义全游戏通用的标准阶段、
> 前置条件与产物校验要求；每游戏的具体命令/参数在 `04_scripts/{游戏}/pipeline.json`。

**入口判定（第一步，2026-09-20 起）**：`ls 04_scripts/{游戏}/pipeline.json`——
- **存在** → 机械操作走 `pipeline.sh`（下方铁律）；
- **不存在**（未接入管线的存量游戏）→ 该游戏的 `04_scripts/{游戏}/surgical_*.sh` /
  `*_so_only.sh` / `inject_*.sh`，或 `mod菜单/starry-mod-menu/scripts/surgical_*.sh`
  即合法机械入口；**仍禁止手敲 adb/apktool/zip/ndk-build 命令序列**，报错时读脚本输出
  而不是换命令试错，并在收尾时评估是否顺手接入管线。

## 操作二分（大框架铁律，2026-09-09 定稿）

| 操作类型 | 唯一合法方式 | 说明 |
|----------|--------------|------|
| **机械操作**（拆包/打包/注入/移植/签名/构建/装机/清理） | `bash 04_scripts/pipeline.sh <游戏> <动作>` | 禁止直接敲 adb/apktool/zip/ndk-build/jarsigner 等命令序列；报错时读日志与校验输出，禁止换命令试错 |
| **分析操作**（逆向/锚点抓取/反作弊研判/方案设计） | 照常自由推理 | 产出必须落 `10_docs/{游戏}/` 文档（证据闭环不变）；状态机的"锚点抓取"阶段产出入文档、指针记录入 pipeline.json |

机械管线文件（pipeline.sh / pipeline.lock）**变更纪律（对所有会话/所有模型一体适用）**：入口启动即校验
sha256 指纹（登记于 `_common/pipeline.lock`），被改动直接拒绝执行；**禁止随手修改，也禁止只改一边或绕过指纹门禁**。
合法参数修改走 pipeline.json 的 `params` 字段（game_id/版本/设备 serial 等——该改的给它改，脚本本体不动）。
确需改本体时，唯一合法姿势＝**同一提交**改 `pipeline.sh` + 刷新 `pipeline.lock` 的
`shasum -a 256` 并 push（见文末「真要改 `pipeline.sh` 时的跨机姿势」）；权限不按模型档次划分，按任务范围与本节纪律划分。

## 管线原生命令（机械操作的全部动词）

```bash
bash 04_scripts/pipeline.sh <游戏> status          # 开工第一步：看当前在哪（同时钉硬约束 + 轮次台账）
bash 04_scripts/pipeline.sh <游戏> next            # 执行下一阶段（自动路由）
bash 04_scripts/pipeline.sh <游戏> next --loop 4   # 连跑最多 4 个阶段（逐次回显日志尾 40 行；失败即停）
bash 04_scripts/pipeline.sh <游戏> run <stage_id>  # 显式重跑某阶段（重跑前建议先 reset）
bash 04_scripts/_common/preflight.sh <文件...>     # 改完任何文件必跑的机器关
bash 04_scripts/_common/pre_install_gate.sh ...    # 装机前产物门禁（现 11 查，见下方专节）
```

辅助：`reset <stage_id>`（状态回退 pending）、`verify <stage_id>`（只校验产物不执行）、
`log [N]`（只回显 `pipeline.log` 尾 N 行，默认 40）、
`round <功能#> <有效|无效|未测> [摘要]` + `rounds`（记迭代轮次 / 打假设矩阵，见「调试内循环」的轮次门禁）、
`gate <stage_id>`（只跑装机前轮次门禁，不执行阶段）。

### preflight 机器关（改完文件的必经之点，2026-09-09 起）

- 改完任何 `.sh/.py/.js/.json` 文件 → 必跑 preflight（bash -n / python compile / node --check / json.tool，多符号少括号立刻抓）
- 改完 `.cpp/.mk` → preflight 会 REMIND：必须重跑管线构建阶段验证（C++ 无独立语法检查器，构建才是检查）
- 管线阶段可在 json 里声明 `preflight` 文件列表（相对 workdir 或绝对路径）：执行阶段命令**前**自动过机器关，FAIL 即拦截——散装改脚本没过检查就跑，在管线侧不可能发生
- 会话纪律、授权边界与协作政策正文见仓库根 `AGENTS.md`；本文件只写机械操作细则，不另设一份通用政策。
- **bash 3.2 兼容门禁（2026-09-30 起）**：`preflight.sh` 对每个 `.sh` 追加调 `bash32_check.sh`，
  `✗` 计入 FAIL、`⚠` 计 REMIND：
  ① `$VAR` 紧跟非 ASCII（bash 把中文并进变量名 ⇒ `unbound variable`；一律写 `${VAR}`，
     实测 `$HAS_CXXSHARED（` 就是这么炸的；被 `\` 转义的字面量不算）
  ② bash 4+ 语法直接起不来：`mapfile` / `readarray` / `declare -A` / `local -n` / `${v^^}` / `${v,,}` / `;;&` / `&>>` / `&>` / `[[ -v x ]]`
  ③ `set -u` 下裸空数组 `"${arr[@]}"` / `${#arr[@]}` ⇒ 写 `${arr[@]+"${arr[@]}"}` 或用显式计数器 `NSRC=$((NSRC+1))`
  （另：`[[ … ]] && exit 0` 在 `set -e` 下会因假值退出 ⇒ 写成 `if … then exit 0; fi`）

### 装机前产物门禁 pre_install_gate.sh（2026-09-17 起，install 阶段必过）

> 来源：18 份会话记录审计——多游戏装机失败属机器可拦截错误（JS 拼接截断、so/dex JNI 配套不齐、
> P/Invoke 符号不符、滑块被 getRet 布尔读），每次都烧掉一整轮装机+用户实测。

```bash
bash 04_scripts/_common/pre_install_gate.sh \
  --js <注入JS产物> [--js <更多>] \        # 查1 JS语法（截断缺尾/语法错）
  --cpp <Mod源码> --dex <包内dex> [--dex <更多>] \  # 查2 JNI配套（so注册方法 vs dex声明）
  --so <so> --pinvoke-json <EntryPoint清单> \  # 查3 P/Invoke符号 vs so导出表
  --menu-json <菜单模板> --cpp <含getVal/getRet的源码>  # 查4 控件类型 vs 读取方式
  --runtime-menu-json <运行时控件清单>                     # 可选；用于查4，允许 Slider_/Toggle_ 前缀
  --apk <目标APK> --base-apk <官方底包> \                  # 查9 so/metadata 配套（IL2CPP 直装包必传）
  --expect-so-sha256 <sha> --expect-metadata-sha256 <sha>  # 查9 精确断言（可选）
```

- 各查独立可选（传了才查），有 FAIL 即禁止装机（exit 1）

**查项总表（2026-10-27 现状：11 查；「四查」是 2026-09-17 的旧称，别再用）**

| # | 管什么 | 起始 | 逃生门 |
|---|---|---|---|
| 查1 | 注入 JS 语法（截断缺尾 / 语法错） | 2026-09-17 | — |
| 查2 | JNI 配套：so 注册方法 vs dex 声明 | 2026-09-17 | — |
| 查3 | P/Invoke 符号：EntryPoint vs so 导出表 | 2026-09-17 | — |
| 查4 | 控件读取一致性：菜单 JSON vs `getVal`/`getRet`（只抓字面量调用） | 2026-09-17 | — |
| 查5 | 门禁串字面量存在性（**禁用 smali `#` 注释当门禁串**，汇编器会丢弃） | 2026-10-02 | — |
| 查6 | 轮次根因关：同功能连续 ≥2 轮无效必须留根因定案 | 2026-10-10 | `--allow-rounds` / `STAGE_NEXT_FORCE=1` |
| 查7 | 包内 `AndroidManifest.xml` 二进制形态（真 AXML 头 `03000800`） | 2026-10-15 | — |
| 查8 | 包内 so 构建戳 `STARRYSTAMP1:EPOCH=`（缺戳 ⇒ 覆盖安装永不清 starry_ota 旧 so） | 2026-10-16 | `STAMP_CHECK=0` |
| 查9 | so / metadata 配套（IL2CPP 直装包：只换一边 = 混搭 ⇒ FAIL） | 2026-10-19 | `SO_META_CHECK=0` |
| 查10 | 菜单 dex 新鲜度（dex 内 `starry-menu:<commit>` 必须等于当前 Java 源码 HEAD） | 2026-10-26 | 见下方「菜单 dex 新鲜度门禁」 |
| 查11 | **加固凭证**：`09_test/<游戏>/harden/<apk_md5>.stamp` 必须与当前 APK md5 对应 | 2026-10-27 | `--allow-no-harden` / `STAGE_NEXT_FORCE=1`，**且必须同时给 `--harden-reason "<理由>"`（空/纯空白被拒，2026-10-27 第六轮机器强制）** |

> 查11 的上游是 `bash 04_scripts/_common/harden_gate.sh <成品APK> [--game <名\|tag>]`（内部调 `harden_apk.py check`，PASS 才写凭证）；加固规则正文见 `AGENTS.md` §6 与 `10_docs/防御加固_通用路线图.md`。逃生门用了必须在装机报告写明理由。
- 查4 局限：只抓字面量索引调用（`getRet(4)` 形式）；变量传参（`getRet(n)`）靠人工核对
- **查9 是什么（2026-10-19 补，我的勇者 7.6.3 首装闪退）**：包内 `lib/arm64-v8a/libil2cpp.so` 与
  `assets/bin/Data/Managed/Metadata/global-metadata.dat` 的「换没换」必须一致；**只换 so 不换 metadata
  （或反之）= 混搭 ⇒ FAIL**。事故链：把热更 build 357 的 so 塞进底包、metadata 留底包版 ⇒ 干净机
  （无热更副本）被游戏自带 `r2.labs.unitypatch` 回退加载包内 so ⇒ il2cpp 互递归爆栈、启动约 7 s 崩；
  旧机因 `files/update/current` 有配套副本（运行时根本不加载包内 so）才一直正常。逃生门 `SO_META_CHECK=0`。
  两类都替换时只 WARN（机器无法判定二者同源，需文档/人工确认）。
- 检出 FAIL 若属故意保留（如兼容空实现），须在装机报告显式说明并经用户确认才可放行
- 与 preflight 职责区分：preflight 管"改完源文件"的语法；本门禁管"装机前"的产物交叉核对
- **装机脚本内建门禁（2026-09-30 起）**：`install_*.sh` 应自带门禁并"不过即 exit 1 阻止装机"，
  避免依赖"记得手动跑"。先例 `04_scripts/勇士之心/install_ysxz.sh`：自己从 `Games/YsxzPatchJs.h`
  用 `R"YSXZJS\((.*?)\)YSXZJS"` 提取注入 JS → `pre_install_gate.sh --js … --menu-json …`
  → 失败直接 `exit 1`；要跳过必须显式 `SKIP_PRE_INSTALL_GATE=1`（并在装机报告说明原因）。
- **双态支持（2026-09-30 起）**：`install_*.sh` 要同时支持 root 机与非 root 机——
  非 root 读不到 `/data/app/**`，md5 校验不可用，改用组件指纹
  （`VERIFY_MODE=sig`：`dumpsys package` 里本包独有组件 + 打印 `versionName=`/`signatures=`）；
  序列号支持 `SERIAL` 环境变量覆盖，便于临时装到第二台测试机而不改 `.device_serial`。

### 菜单 dex 新鲜度门禁（2026-10-26 起，出包阶段必过）

**背景**：出包脚本「模板优先」——模板在就跳过 apktool/smali 现编。模板一旦停在旧日期，改过的 Java 永远
进不了包（无限升级 vc1637「重启后功能全关」、镇邪人 `7f94233e`／`a3748024`，两天三次同一根因）。

```bash
# 1) 改过 app/src/main/java 后必刷（release 现编 + 注入构建戳 + 写 build_info.txt）
bash mod菜单/starry-mod-menu/scripts/wxsj_refresh_menu_tpl.sh          # → templates/wxsj_menu_current/
# 2) 派生字体线模板（如 vc1637）
bash mod菜单/starry-mod-menu/scripts/wxsj_vc1637_menu_tpl.sh
# 3) 出包前/装机前任一时刻校验（模板或成品包都行）
bash 04_scripts/_common/menu_tpl_gate.sh --tpl <模板目录> --dex-name classes4.dex
bash 04_scripts/_common/pre_install_gate.sh --apk <成品APK> --menu-dex <成品APK> --menu-dex-name classes4.dex
```

判据：`build_info.txt` 的 `source_commit` 与 dex 内 `starry-menu:<commit12>` 都必须等于
`git log -1 -- app/src/main/java` 的提交。`menu_tpl_gate.sh` 退出码 1 = 过期（禁止出包）；
`--allow-stale` 仅用于复现历史产物，用了必须在汇报里写明。

### 装机验收口径：干净安装（2026-10-19 起，install 阶段必做）

> 依据：我的勇者 7.6.3 —— 2026-10-18 只在「已热更过的旧机」做同签名**覆盖安装**验证通过就交付，
> 2026-10-19 在干净机（全新安装）复现启动约 7 s 必崩。**覆盖安装 ≠ 首装验收。**

- **任何对外分发 / 换设备的包，install 阶段必须包含一次「卸载 → 全新安装 → `post_install_precheck.sh`」**；
  覆盖安装只作附加证据（需要保留用户数据时才用）。
- 未做干净安装时必须显式标注 `NOT PROVEN：仅覆盖安装验证`，**不得写成"已交付 / 用户实测通过"**。
- 卸载属破坏性操作（连带清 `/sdcard/Android/data/<pkg>` 与 app 私有数据），**须先取得用户同意**。
- 判定证据：同一包、同设备的预检记录（`09_test/<游戏>/precheck_*`），「干净安装」那条的 pid 与时间戳可核对。

## 标准阶段表（新游戏从模板生成后按此裁剪）

| # | stage_id | 名称 | 前置 | 后端命令形态 | 产物与校验 |
|---|----------|------|------|--------------|------------|
| 1 | `check_apk` | 官包核对 | — | md5/大小/包名核对（对照 02_apks 登记） | 官包指纹入 json |
| 2 | `dump` | 拆包 dump | 1 | decode_apk.sh + Il2CppDumper / jsc 解密 | dump.cs 等产物存在 |
| 3 | `anchor` | 锚点抓取 | 2 | 分析性阶段（AI 自由推理），产出落 10_docs | 锚点文档路径入 json |
| 4 | `impl_mod` | 功能实现 | 3 | new_game_mod.sh 四件套 + 填 TODO | Mod/Entry/mk 三件套存在 + 构建通过 |
| 5 | `build_so` | 构建 so | 4 | ndk-build（私有 mk + 独立 obj；**换证书须先 `xxd -i` 重烘 `CertPinned.h`**，只换 dex/assets 无效） | so 含游戏标签 + JNI_OnLoad 导出 + md5 入 json；后端防呆四件套：**绝对路径 NDK_OUT/LIBS_OUT + obj stamp（mk/探针开关变化清 obj 重编）+ 链接活动保险（有新 .o 而 so 未变=FAIL）+ 菜单串按字节校验**（范本 `04_scripts/魂坠深境/hzsj_build_so.sh`） |
| 6 | `surgical` | 外科注入 | 5 | surgical_{游戏}_{版本}.sh（迭代场景后端配 `*_so_only.sh`，见下方内循环） | 注入包存在 + so 标签复核 + **包内 so md5 == build_so 产物**（旧 so 混包直装防线） + **若替换了 `libil2cpp.so` 就必须同时替换 `global-metadata.dat`（查9 配套关；2026-10-19 我的勇者首装闪退）** |
| 7 | `sign` | 签名 | 6 | uber-apk-signer | 签名包存在 + hash 入 json |
| 8 | `device_ready` | 设备准备 | — | adb devices / connect（MuMu `127.0.0.1:5555`，重启后端口可能变 16384）/ 端口转发（MT MCP `adb forward tcp:8787 tcp:8787`） | 目标设备在线 |
| 9 | `install` | 装机验证 | 7,8 | **装机前必过 `_common/pre_install_gate.sh` 查1–查9**（JS 语法/JNI 配套/P/Invoke 符号/控件读取/门禁串/Manifest 形态/构建戳/**so-metadata 配套**，见上方专节；**优先由 `install_*.sh` 内建并自动阻止装机**）→ adb install（须真机授权；大包 push+pm install；覆盖安装失败不自动卸载；非 root 机用组件指纹校验，见 `VERIFY_MODE=sig`） | 安装成功日志 + **`_common/verify_installed_apk.sh` 设备包 md5 == 本地 APK**（root 真机加 `--su`；非 root 机改组件指纹；防"装机成功"假象装旧包） + **干净安装预检**（见上方「装机验收口径：干净安装」；只做覆盖安装须标 NOT PROVEN） |
| 10 | `archive` | 收官归档 | 9 | 文档/游戏摘要 + current.md 单行索引 + `_common/archive_scoped_git.sh` 白名单**提交并推送 origin/main**（用户口令「git」= 提交+推送连体；仅明确不想推送时才 `--no-push`） | 索引行存在 + 唯一正式包/裁剪清单存在 + 远端已同步 |
| 11 | `archive_close` | 状态收口提交 | 10 | pipeline 写回 archive done 后，运行 `_common/archive_close_git.sh --game {游戏}`；校验游戏范围无漏提交/未跟踪文件，自动生成 `10_docs/{游戏}/delivery_manifest.json`，与 `pipeline.json` 状态一并提交并推送 | manifest 存在且 APK/artifact hash 可核对；本作路径无未提交状态 + 远端已同步 |

规则：
- **顺序执行**：`next` 只取第一个 pending 阶段；跳步=用 `run` 显式指定（会做前置检查，前置未满足直接拒绝）。
- **失败即停**：阶段失败状态置 `failed` 并附日志路径；禁止换命令绕过，修完原因后 `reset` 该阶段重跑。
- **产物即证据**：每阶段完成把产物指纹（md5/标签检查结果）写回 json——"改错文件返工"由此兜底：下一阶段前置校验产物指纹，改动过的产物立刻暴露。
- **inputs 快照与陈旧度（2026-09-10 起，防旧产物直装）**：阶段完成时记录上游产物 md5 快照。快照≠现状 = 该阶段过期：① 仍处 done 的过期阶段被**自动回退 pending**（上游产物一变化即触发，`reset 上游忘重跑下游`不再可能装旧包）；② 过期链路上的阶段**拒绝执行**并点名环节（改了源码没重编 so、换了 so 没重打 APK 都会拦），remedy = `reset <被点名阶段>` 后 `next` 连跑；③ 产物未变化的重跑不引发级联（幂等）。`pipeline.json` 状态文件与 `*.md` 活文档不参与指纹/新鲜度判定。
- **每游戏一份 pipeline.json**，位于 `04_scripts/{游戏}/pipeline.json`；新游戏按本节阶段定义生成（或由 `new_game_mod.sh` 生成第五件套）。
- 日志统一追加至 `04_scripts/{游戏}/pipeline.log`。完整日志是本地可再生物，
  `.gitignore` 已排除；Git 只保留结论、决定性日志行和复现命令。

## 调试内循环（最高频场景：改 Mod → 重打 → 装机 → 看日志 → 再改）

已有外科底包的迭代**不需要重走完整标准阶段**，管线标准动作（**2026-10-03 起动词已原生在 pipeline.sh**）：

```bash
bash 04_scripts/pipeline.sh <游戏> status                  # 开工第一步：硬约束生效条目 + 管线状态 + 轮次台账
bash 04_scripts/pipeline.sh <游戏> reset build_so          # 回退构建阶段
bash 04_scripts/pipeline.sh <游戏> next --loop 4           # 连跑最多 4 个阶段（build_so→surgical→device_ready→install）
bash 04_scripts/pipeline.sh <游戏> log 40                  # 只回显日志尾 40 行
bash 04_scripts/pipeline.sh <游戏> round 1 无效 "改了 X"    # 记一轮「改→装→实测」结果
```

- 旧命令形态（`_common/stage_next.sh <游戏> status|next|auto N|log N|rounds|round|gate`）仍可用：
  它现在是**同义转发器**，只转发到上面的原生动词，不再自己实现逻辑（P1/P4/P5 已并入 pipeline.sh 本体）。
- **轮次门禁（P4）**：同一功能连续 3 轮「改代码→装机→用户实测仍无效」时，含 `install` 的阶段**直接拒绝执行**（exit 3）并打印假设矩阵；先跑 `rounds` 把矩阵填完贴给用户定方向，再谈继续。仅当用户当轮明确要求「再试一次」才用 `STAGE_NEXT_FORCE=1` 放行（会大声留痕）。台账落在 `10_docs/<文档游戏名>/轮次台账.tsv`。
- **Windows 编码（2026-10-03 已根治）**：`pipeline.sh` 顶部固定 `PYTHONIOENCODING=utf-8` / `PYTHONUTF8=1`，Windows 侧不会再因状态符号 ⬜/✅ 触发 `UnicodeEncodeError: 'gbk' codec ...`（旧版靠包装器注入垫片绕过，现已进本体）。

### 存量未接管线：已有星光包只重打 dex/APK/重装（2026-09-29）

只改 Java/dex、Manifest 或菜单资源时，不需要重编 native，直接运行该游戏的外科脚本；脚本内部一次完成构建、签名、产物验证、可选安装。若 C++/so 改过，先跑该游戏的 `*_build_so.sh`，再走同一条外科链。

```bash
# 默认：构建+签名+验证，不安装
bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh

# 构建+验证+覆盖安装（只做已授权的真机测试）
STARRY_INSTALL=1 SERIAL=<serial> bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh

# 复用现有正式包，只验证+安装；不跑 Gradle/apktool/smali/签名
STARRY_INSTALL_ONLY=1 SERIAL=<serial> bash mod菜单/starry-mod-menu/scripts/surgical_<game>_<ver>.sh
```

- 验证不要依赖 `/tmp` 里上一轮残留的 dex/JS：验证脚本应从 APK 重新解出 `classesN.dex` 和 so，按包内实际字节判定。
- 重新签名后完整 APK md5 会变化；混合包判断必须比较 dex md5、so md5、包结构/Manifest，不得用整个 APK md5 代替。
- 游戏提供独立验证入口（如 `sjzz_verify.sh`）时，外科脚本和装机单跑都应调用它，避免两套判定漂移。

- **防旧包直装是自动的（2026-09-10 起）**：reset 上游后忘 reset 下游不再需要记——上游产物一变化，
  过期的下游 done 阶段自动回退 pending（终端会打 `⚠ 自动回退 [...]`），连跑 `next` 整条链刷新；
  改了源码直接 `run` 下游会被陈旧度门禁拒绝并点名过期环节。产物未变化的重跑不会级联（幂等）。
- surgical 阶段的 `command` 配 `*_so_only.sh` 后端（每游戏 pipeline.json 自行声明；so_only 语义=已有底包只换 so 重签，不碰原包）
- 装机后的**观测采集属分析操作**（非管线阶段，AI 按诊断需要执行并记录）：
  - `adb logcat -s StarryMagic StarryTrace`（功能/hook 日志）

### Manifest / 资源查看：aapt2 正确形态（2026-10-02 起，写死别再猜）

查看包内 `AndroidManifest.xml`（注入后核对 `application@name`、Activity、权限、`usesCleartextTraffic` 等）：

```bash
# ✅ 正确：xmltree 必须配 --file 指定包内路径
"$AAPT2" dump xmltree --file AndroidManifest.xml <apk>

# ✅ 正确：图标/启动 Activity/版本 一眼看
"$AAPT2" dump badging <apk> | grep -E "^(package|launchable-activity|application-icon)"

# ❌ 错误形态（会直接报错，别再试）
"$AAPT2" xmltree <apk>              # 缺 dump 子命令
"$AAPT2" dump xmltree <apk>         # 缺 --file AndroidManifest.xml
```

- `AAPT2` 默认取 `$HOME/Library/Android/sdk/build-tools/<ver>/aapt2`（macOS）；先 `ls` 取最新版本号，不要写死。
- **门禁串必须能在包内二进制里查到**：`unzip -p <apk> 'classes*.dex' | grep -c -F -a -- '<字面量>'`。
  **绝不要把门禁串写成 smali `#` 注释**——汇编器会丢弃注释，产物里根本没有该字面量，
  而"文本包含"式门禁仍显示通过（2026-10-02 疾风骑士为此连续 2 轮装机失败）。
  机检入口：`04_scripts/_common/pre_install_gate.sh --apk <apk> --needle '<字面量>'`（查5）。

### 日志采集铁律（2026-09-21 起：省 token + 保信号）

系统日志里存在**单行数千字符**的输出（如 `AppOrder{...}`、`ContextAware_*`），
全缓冲 `grep` 一次就能吃掉大量上下文，且把真正的 hook 行淹没。规则：

```bash
# ✅ 首选：按 tag 白名单过滤（不读系统日志）
adb -s $SERIAL logcat -v time -s Mod_Menu:V Ysxz:V StarryMagic:V MenuService:V

# ✅ 需要离线归档：先落盘，再只 grep 文件、只 tail 命中行
adb -s $SERIAL logcat -v time > /tmp/run.log
grep -E 'MODMOD|Ysxz|splice|REPORT' /tmp/run.log | tail -40

# ❌ 禁止：全缓冲 adb logcat -d | grep -Ei ...（会被超长系统行淹没/放大）
```

- 判定 hook 是否挂上，**只 grep 白名单关键字**：`hooked se::evalString`、`logic splice`、
  `[MODMOD] names:`、`REPORT`、`feature #`；不读整段日志。
- 收尾时把决定性行写进 `10_docs/{游戏}/evidence_日期/README.md`，**完整 logcat 不入库**。
- 崩溃哨兵另算（只看 `data_app_crash`/`data_app_native_crash`/ANR 关键字），
  与功能验证分开，不因"日志里还没出现 X"就改代码。

### 设备批量拉取铁律（2026-09-21 起：先量后拉）

逐文件 `adb exec-out` 在文件多时**必然超时且可能丢 stdout**（勇士之心实测：
`files/res` 下 22,000 个 json，首版逐条拉取 10 分钟超时、零产出）。规则：

```bash
# 1) 先量：文件数 + 总量
adb -s $SERIAL shell "su -c 'cd <dir> && find . -name \"*.js\" | wc -l; du -sh .'"
# 2) >50 个文件或 >20MB → tar 单流一次拉走
adb -s $SERIAL exec-out "su -c 'cd <dir> && tar -cf - \$(find . -name \"*.js\")'" > out.tar
tar -xf out.tar -C <dest>
```

- 只拉**分析需要的类型**（`*.js` + `version`/`config`/清单），资源序列化数据
  （`import/*.json`）不纳入分析拉取。
- 拉完必须**逐文件比对设备端 md5**，数量与指纹全对才继续。
  - `adb shell ls /data/system/dropbox/`（闪退归因：`data_app_crash`=Java 层、`data_app_native_crash`=native 层，分开计数）
- 迭代收敛后把最终 so 指纹与验证结论落 10_docs 游戏文档

### 人工测试时序（铁律：行为证据以人的反馈为第一来源）

```
AI 装好包 → 告知用户「请测试 X 功能，测完告诉我结果」→ 停下等待（不发指令不抓日志不下结论）
  ├─ 用户反馈"正常" → 收官/下一阶段（不需要主动抓日志复查——人的确认就是行为证据）
  └─ 用户反馈"有问题" → 这时才抓 logcat/dropbox 诊断
```

- **等待期间 AI 唯一允许的主动动作**：被动崩溃监控（只看 `data_app_crash`/`data_app_native_crash`/ANR 关键字是否出现——这是异常哨兵，不是功能验证）
- 禁止：轮询业务日志行、基于"日志里还没出现 X"幻想出错、人没测完就先动手改代码
- **自动化点击同理禁止**：人不测试而 AI 用 `adb input tap` 瞎猜坐标点界面=低价值高污染（点错状态污染+行为特征异常）。行为验证=人的职责；AI=日志监控+异常诊断。真要 UI 自动化必须先建专门基建（uiautomator/accessibility）并作为管线阶段接入，不允许临场发挥

## 收官口令（用户说「收尾+正式包+清理+git」时，AI 展开此清单）

压缩顺序：**验证正式包 → 裁剪 `08_output` → 清理滚动物（`trash_paths.sh` dry-run→apply）→ 修 pipeline 快照 → 文档状态 → `archive_scoped_git.sh` 提交推送**。每一步先打印实际路径/影响；缺底包、缺正式包或签名校验失败即停止。

### 收官脚本参数契约表（2026-10-10 追迹迷城收官复盘 P0-4 落地）

> 踩坑背景：收官链上**两种清单长得像但互不通用**（prune 清单 vs trash 清单），2026-10-10 实测各踩一次返工。

| 脚本 | 典型入参 | 清单 / 输入契约 | 通过判据 | 常见报错 → 正解 |
|---|---|---|---|---|
| `prune_output_final.sh` | `<游戏输出目录> <保留APK> --apply --manifest <文件>` | **写** prune 清单：`removed:[{name,size,md5}]` | 目录只剩唯一正式包 | 无参数 ⇒ 打 usage（`<游戏输出目录>` 与 `<保留APK>` 均必填） |
| `trash_paths.sh` | `--tag <标签> --path <相对路径>…`（可多个）`[--apply]`；建议先 `--suggest --game <游戏>` | **吃**清理（trash）清单：**显式 `--path` 列表，禁通配符** | dry-run 先打印解析后的绝对路径 + Trash 目标；含 tracked 文件的路径**直接拒绝**（rc=1）；`--suggest` 只打印 `--path` 草案 | `ERROR: 路径含已入库(tracked)文件，禁止清理` ⇒ 从清单剔除该路径（确实要清才加 `--allow-tracked`）；`--suggest` 报 rc=2 ⇒ 忘了 `--game` |
| `pipeline_record_cleanup.py` | `--game <游戏> --manifest <文件>`（或 `--removed name:size:md5`） | **只吃 prune 清单**（键 `removed:[{name,size,md5}]`） | 输出「已更新」或「无需修改: 指定路径未出现在 pipeline 快照中」 | `错误: 至少需要一个 --removed 或 --manifest 条目` ⇒ 传的是 **trash 清单**，改传 `正式包裁剪清单_*.json` |
| `check_state_hash_sync.py` | `--game <游戏> --manifest <prune 清单>` | state 文件须含**完整** md5 + sha256；`current.md` 本游戏行须含二者 **8 位前缀**，且行 ≤400 **字符**（不是字节） | `OK … row_chars=N` | `FAIL … 缺 current sha256 prefix` ⇒ 补 state 的 sha256 全值 + 索引行 8 位前缀 |
| `archive_scoped_git.sh` | `--message <说明> --path <白名单路径>…`；另 `--no-push` / `--staged-only` / `--dry-run` / `--require-clean-paths` | 白名单 `git add`（**禁 `-A`**）；push 走 `git_push_main.sh::push_main` | 提交 + 推送 origin/main（`git status -sb` 无 ahead） | push 失败 ⇒ 保留本地提交、**`exit` 非 0**，不 `--force`。**调用方必须查 rc**：`1`＝已提交但未推送（网络）、`3`＝远端分叉需 fetch/rebase（2026-10-16 前此处 rc 恒为 0，`finalize_round.sh` 的 `step_fail` 因此是死代码） |
| `archive_close_git.sh` | `--game <游戏> [--no-push] [--message <说明>]` | 管线状态收口 + 生成 `delivery_manifest.json` | 同上 + manifest `--check` 通过 | 同 `archive_scoped_git.sh`（同一处 rc 修复）；manifest `--check` 仍在其后先跑 |
| `check_release_clean.sh` | `--tag <文件名前缀> --so <so>` | `--tag` 是**文件前缀**（大小写敏感，如 `ZjmcMod`），**不是**游戏名或版本号 | 扫描命中 + 无禁用串 | `没找到扫描对象` ⇒ 传 `Games/<Tag>Mod.cpp` 的前缀 |
| `axml_inspect.py` | `<AndroidManifest.xml> --assert-map` | 平台属性 res id 真值表（`KNOWN_ATTR_RES_ID`，值取自 android.jar） | `[assert-map]` 无 FAIL（rc=0） | `[FAIL] <android:xx> res id 期望 … 实际 …` ⇒ 补丁器写错了 res id（见 TOOLBOOK #48/#55） |

1. 文档收尾：游戏文档更新（结论/证据/遗留）；详情写入
   `10_docs/_state/games/{游戏}.md`，`current.md` 只同步一行摘要。反复反馈的
   功能补 `10_docs/_templates/功能验收卡.md`，多日志证据补
   `evidence_日期/README.md`。
   文档 hash 同步自检：`python3 04_scripts/_common/check_state_hash_sync.py --game {游戏} --manifest 10_docs/{游戏}/正式包裁剪清单_日期.json`。
2. 正式包：**先确认当前 `08_output/{游戏}` 里有哪些包**（用户手动增删，可能已空）。
   核对指定正式包签名/哈希后，执行 `bash 04_scripts/_common/prune_output_final.sh 08_output/{游戏} {正式包} --apply --manifest 10_docs/{游戏}/正式包裁剪清单_日期.json`；最终目录只保留一个正式 APK，被删候选包/idsig 的 hash/大小入清单。
   目录为空或正式包不存在 → 报告并停下，不要从 `09_test` 里翻中间包冒充正式包。
   icon / 菜单模板是**按需交付物**（2026-10-05 用户定）：仅「新游戏首次交付」（`08_output/{游戏}` 下无 `delivery_*`）或用户当轮点名「图标和菜单模板」时，才用 `bash 04_scripts/_common/finalize_game_delivery.sh --game {游戏} --apk {正式包} --icon-source {原图} --menu-template 10_docs/{游戏}/{id}_menu_template.json [--prune --sync-doc] --apply` 生成 delivery 目录（icon 512/144 + 模板副本 + 字段自检）；其余收尾**不传这两个参数**（脚本默认跳过、不建 delivery 目录），裁剪与文档片段分别走上面的 `prune_output_final.sh` 与 `--sync-doc`。

   **收尾四条固定命令（2026-10-15 霓虹深渊2 r19 补；都是本轮实测踩出来的）**：
   - 看状态**用 `--brief`**（全量 `status` 会回显「硬约束生效条目全文 + 台账尾部」≈34 KB，整读等于白烧一次压缩；`--brief` 实测 0.9 KB）：
     `bash 04_scripts/pipeline.sh {游戏} status --brief`（要全量/要钉硬约束时才用不带 `--brief` 的 status）。
   - `next` 报 `missing …<某阶段产物>` 且该阶段**已经跑过**（典型：`archive` 的 prune 删掉了 `sign` 的输入 unsigned apk）⇒ 先按产物裁剪清单登记清理
     `python3 04_scripts/_common/pipeline_record_cleanup.py --game {游戏} --manifest 10_docs/{游戏}/正式包裁剪清单_日期.json [--apply]`（**先 dry-run**；它会把 `verify` 改 `not_exists` 并从快照里移除该产物/下游 inputs），
     再用 `bash 04_scripts/pipeline.sh {游戏} verify <stage> --set` 把「产物已在、状态需补」的阶段置回 done；**禁**手改 stage 的 `verify` 语义。
     （2026-10-15 实例：`sign` 因 prune 被标 failed 且一路留在台账里，`--brief` 一跑就暴露 ⇒ 登记清理 + 三个 `--set` 后 9/9 全绿、无 stale 标记。）
   - 真机残留清理：**已获当轮授权收尾**时才走现成脚本（**禁手打 rm 名单**：非 root 会静默删不动，实测 `su 0` 才删得掉）：
     `bash 04_scripts/_common/cleanup_device_re_residue.sh --deep`（先 `--dry-run` 看范围；保留项写进台账）。
     **未获授权时（复盘/分析/纯文档会话）不跑该脚本**，只做定向核验并报告待处理项 —— 本行按 `AGENTS.md` §6 从严口径统一（2026-10-16 消解与「归档正文 D」的冲突）。定向核验示例：
     `adb -s <serial> shell "su -c 'ls -la /data/local/tmp/ | grep -iE \"<pkg>|nxreserve\" || echo 无残留'"`。
3. 官包归位：`02_apks`（用户手动管理，AI 不探测不整理）；本轮是否有原版以**用户声明**为准。
4. 滚动物清理：收官游戏删 03/06 大二进制/07 大包（git 有文本；只清理本游戏路径；删前逐项核对再生依据；中文路径先 `git config core.quotepath false`）。
   优先用 `bash 04_scripts/_common/trash_paths.sh --tag {游戏}_{日期} --path 03_decompiled/{游戏} ...`；默认 dry-run，核对路径/大小/Trash 目标后再加 `--apply`。只清理本游戏可再生路径（03/06 大二进制、07 大包、09_test），中文路径先 `git config core.quotepath false`。
   - **先看摘要的「本地产物与关键输入」表**：标"不可再生"的输入（设备拉取的官方底包、
     唯一热更样本、已被用户删除的原包对应的解密产物）**一律保留**。
   - **旧版本 dump 的生命周期（2026-10-14 补，蛋壳战机 1.1.8 实测）**：接手新版本、要重跑
     Il2CppDumper 之前先确认「上一版的 `06_dumped/{游戏}/il2cpp_dump_{旧版本}*` / `static_inspect_{旧版本}*`
     是否还要复用」——不再复用就**别生成第二份**（实测：1.1.7 两份（≈321 MiB）在新版本接手当天生成、
     当天清理，白跑一轮 Il2CppDumper + 一轮 static 检查）。旧版本的挂点对照写进
     `10_docs/{游戏}/抓点_锚点_{新版本}.json` 的旧版本字段即可，不必留整份 dump 存档。
   - **清理后必须同步修正 `{游戏}/pipeline.json` 快照**（2026-09-19 补，弓箭传说2 踩坑；2026-09-20 起脚本化）：`archive` 执行前的 `check_stale` 会遍历**所有祖先阶段**的 `inputs` 快照逐个比对 md5，被清理删掉的中间产物（unsigned APK、`07_patched` 留证、用户手动删除的 `02_apks` 官包）会让它判定「上游链路过期」并拒绝执行——**清理越彻底越跑不过**。先跑 dry-run：

   ```bash
   python3 04_scripts/_common/pipeline_record_cleanup.py \
     --game {游戏} --manifest {清理或裁剪清单}.json
   ```

   核对输出后加 `--apply`。脚本会把命中的 `verify` 改为 `not_exists`、删除
   对应 `artifacts`/下游 `inputs`，并把原 md5 追加到 stage `note`；`archive.verify`
   仍应显式声明应清理的中间产物。**顺序：清理 → 脚本修快照 → 跑 `archive`**。

   **禁止手改 stage 的 `verify` 语义（2026-09-30 补，复盘 2.2，实测踩过）**：清理产物只能通过
   `pipeline_record_cleanup.py` 登记；`verify` 永远保持"该阶段的真产物"语义。清理后下游显示
   「⚠️上游已更新，本阶段过期（需重跑）」**属正常**，不要为了让界面好看把 `verify` 改成 `not_exists`
   ——上轮这么改过一次，下一轮 `pipeline.sh {游戏} next` 直接报「仍存在 …libstarry_magic.so」自相矛盾，
   白耗一轮定位。

   **manifest schema 对照（2026-09-21 补，实测）**：脚本认 `removed_paths` /
   `removed_roots` / `removed` 三种键，条目可以是字符串路径，也可以是带 `name` 的对象
   （`name` 会补成 `08_output/{游戏}/{name}`）。`prune_output_final.sh` 产出的裁剪清单
   用 `removed: [{name,size,md5}]`，**可直接喂给脚本**，无需手改：

   ```bash
   python3 04_scripts/_common/pipeline_record_cleanup.py \
     --game {游戏} --manifest 10_docs/{游戏}/正式包裁剪清单_日期.json --apply
   ```
5. 设备残留：真机会话按 TOOLBOOK「真机残留清理」回收证据与残留
6. **git 固化**：优先用 `_common/archive_scoped_git.sh --path ... --message ...`，
   只提交本任务白名单，**提交后默认推送 origin/main**（用户口令「git」= 提交+推送连体，
   不需要再问「要不要推」）。仅当明确不想推送时才加 `--no-push`，且必须在汇报里写明
   「已提交未推送 + 远端落后几个提交」。push 冲突/网络失败则保留本地提交并明确报告，
   不自动强推。
   如果已经用 `git add` 精确暂存，可加 `--staged-only`；该模式不会再次 `git add`，只校验暂存区全部落在显式白名单、过空白门禁后提交并推送。
   若白名单文件可能混有其它会话改动，加 `--require-clean-paths`。该检查会在提交前确认白名单路径没有未暂存改动；有混合改动时先做 hunk 级暂存，再走 `--staged-only`，禁止用路径级 `git add -A` 直接吞入无关改动。

   **提交时机（2026-10-19 霓虹深渊2 r22 补）**：git 提交要放在「pipeline settle」之后——先把
   `pipeline_record_cleanup.py --apply` 与 `pipeline.sh <游戏> verify <stage> --set`（置回 9/9 done）
   全部跑完，再走本步提交；否则提交后 `verify --set` 会再改写 `pipeline.json` 的阶段状态/指纹，
   产生二次未提交 diff（r22 实测：先提交时 8/9 done，后 settle 到 9/9，pipeline.json 留 17 行未提交）。

   **三条共享文件硬规则（2026-09-30 补，勇士之心收官实际踩过，提交 `9aed48b9` 混入他会话 17 行 → `7d5e331c` 修正）**：

   a. **共享文件禁止路径级 add**：`mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java`、`10_docs/_state/current.md`、`10_docs/_state/底包再获取台账.md` 是多会话同时改的文件。
   只能先内容级暂存再提交：
   ```bash
   python3 04_scripts/_common/stage_shared_file.py \
     --path mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java \
     --drop-block '伊莫 官方原版::^\s+"[A-Za-z0-9+/=]+";\s*$' \
     --drop-pattern 'YIMO_' --assert-absent 伊莫
   bash 04_scripts/_common/archive_scoped_git.sh --staged-only --message "…" --path <同名文件>
   ```
   `archive_scoped_git.sh` 已内置硬门禁：共享文件存在未暂存改动时，路径级 add 直接 exit 1 并给出上面两条命令。
   `stage_shared_file.py` 只写索引、不动工作区（他人改动原样留在工作区），并自带**截断语句门禁**：被丢弃的行若以 `= + , ( \` 结尾且下一行被保留，报错 exit 4（防只按标记串丢弃、留下他人无标记续行）。

   b. **共享文件核验必须逐行，禁止用「grep 标记串 = 0」代替**：实例如 `private static final String YIMO_CERT_B64 =` 后面跟 15 行 base64 字符串续行，续行里既无「伊莫」也无 `YIMO_` ⇒ `grep -c` 全为 0 但提交里躺着 17 行他人代码。正确做法：读 `git diff --cached`（或 `stage_shared_file.py` 打印的「索引 vs HEAD」差异与 `git show <commit>:<file>`）**逐行确认归属**，并回看 `git diff --cached --stat` 的改动量是否与预期一致。

   c. **目录白名单会顺带纳入他会话新文件**：`git add -A -- <目录>` 实测把同目录下伊莫会话的 `YimoEntry.cpp`/`YimoMod.cpp`/`YimoMod.h` 一起暂存。提交前必须 `git diff --cached --diff-filter=A --name-only` 逐条确认归属，误入的用 `git restore --staged -- <path>` 撤下；脚本对非字面白名单的新增文件会打印「提醒: 目录白名单顺带纳入的新增文件」。**优先给字面文件路径，少用目录路径**。
   `--path` 只传**非忽略**路径（2026-09-20 实测）：传 `.gitignore` 覆盖的
   `mod菜单/**/app/src/libs*/` 下副本时，`git add -A` 打印「ignored by one of your
   .gitignore files」并返回非 0 → 脚本 `set -e` 中止，已暂存内容需 `git reset` 收拾
   （`obj*/`、`.gradle/` 实测不中止）。星光项目收官只提交
   `app/src/main/jniLibs/arm64-v8a/libstarry_magic.so` 那份（gradle 实际打包的），
   其余同内容副本在收尾清单写明「忽略目录副本，保留未暂存」。
   脚本有 `git diff --check` 空白门禁：**文件尾部多余空行会中止提交**，先归一化再提交。
   查未提交改动时只查白名单路径，不要全量 `git status`（会把无关会话的改动全打出来）：
   `git status --porcelain -- 04_scripts/{游戏} 10_docs/{游戏} 10_docs/_state`。
7. 管线写回 `archive=done` 后，运行
   `_common/archive_close_git.sh --game {游戏}` 收口状态，避免手工补第二次提交。
   **降级路径（2026-09-21 补）**：若该游戏 `pipeline.json` **没有 `archive` 阶段**
   （存量游戏常见），第 7 步无对象——跳过 `archive_close_git.sh`，改为
   在 `pipeline.json` 的 `notes` 与本轮新阶段 `note` 里记录收官事实（提交号/产物指纹），
   并在汇报里注明"该游戏未接入 archive 阶段，无收口提交"。
8. 验证收口：本任务相关路径无未提交改动；若工作区仍有无关改动，列表报告且不纳入提交。`current.md` 有本游戏单行索引 + 汇报 CLAIM/VERDICT/EVIDENCE。

## 现有脚本资产的角色

113 个存量脚本（surgical_* / *_so_only / inject_*）是各游戏各阶段的**后端**，pipeline 只做
路由/状态/校验，不重写。存量游戏可逐步接入（出包时顺手），新游戏出生即接入。

`_common/` 下与「跑管线」直接相关的现成能力（**动手前先看这里，禁止重造**）：

| 脚本 | 角色 | 触发场景 |
|---|---|---|
| `pipeline.sh` + `pipeline.lock` | 状态机本体（**禁随手改；确需改只按「同一提交 + 刷新 lock + push」**，详见文末跨机姿势；lock 由 git 跟踪、被 Syncthing 忽略，见下注）。**2026-10-03 起原生带**：硬约束钉行（P1）/ 轮次门禁（P4）/ 内循环 `next --loop N` + `log`（P5）/ UTF-8 输出垫片 | 所有机械动词 |
| `stage_next.sh` | pipeline.sh 的**同义转发器**（旧命令形态兼容层，不再自己实现逻辑） | 旧习惯命令 / 文档里的旧写法 |
| `preflight.sh` | 阶段执行前的语法/格式机器关 | 阶段声明 `preflight` 时自动调用 |
| `bash32_check.sh` | macOS bash 3.2 兼容门禁（本仓脚本都要在 3.2 下跑） | 新增/改 `*.sh` 后 |
| `pre_install_gate.sh` | 装机前产物门禁 | 装机前 |
| `finalize_game_delivery.sh` | 收尾交付一键（delivery 目录 + prune + 文档片段） | 用户说「收尾」 |
| `prune_output_final.sh` / `archive_scoped_git.sh` / `stage_shared_file.py` | 08 裁剪 / 白名单提交推送 / 共享文件入库 | 收尾 |
| `trash_paths.sh` | 受控删除（**拒绝仓库根/.git/01_tools/02_apks/08_output**） | 清理 |

**游戏侧「配置化 + 诊断分档」（C1/C2）的通用规范见 [`GAME_CONFIG_PATTERN.md`](GAME_CONFIG_PATTERN.md)**：
C1 = 把「会变的数值」抽成设备侧配置文件（读不到就回内建默认；**加键仍要重编**，所以首次要抽全）；
C2 = 把只在排障时要的诊断/扫描代码抽成编译期开关（`STARRY_DIAG` / `WOR_DIAG_FULL`，整段 `#if` 才省体积）。
给某款游戏加配置化**动手前先读它** —— 三条边界、两条互不兼容的接线路线（Mono 的 `StarryConfig` vs
native 自读）、热重载做法、四条验收与可 grep 的日志锚点都在里面。

- **清理禁通配符（P9）**：删除一律写**显式名单**，先打印解析出的绝对路径复核、删后复扫一次；禁止 `_*.py`、`rm -rf <dir>/*` 这类会命中库内文件的通配符（历史事故：误删 16 个已入库 tracked 脚本，靠 `git checkout` 才救回）。
- **真要改 `pipeline.sh` 时的跨机姿势（2026-10-03 澄清，当日已按此流程改过一次）**：`pipeline.sh` 与 `_common/pipeline.lock` 都是 **git 跟踪文件**（`git ls-files 04_scripts/_common/pipeline.lock` 命中），只是 `_common/pipeline.lock` 被 Syncthing `.stignore` 忽略（`10_docs/双机协作/双机协作同步方案.md:482`，避免双通道冲突），而 `pipeline.sh` 本身走 Syncthing。所以改法是：**同一提交**改 `pipeline.sh` + 刷新 `pipeline.lock` 的 `shasum -a 256`，然后 push，另一台 `git pull --rebase --autostash` 后即一致（不必手工碰 lock）；**禁止**只改一边或两端同时改 —— Syncthing 会先把新 `pipeline.sh` 送到另一台而那边 lock 还是旧的，该机任何 `pipeline.sh` 命令都会 die「指纹不匹配」，`git pull` 一次即恢复。
  - 2026-10-03 实例（P1/P4/P5 并入 + UTF-8 垫片）：新指纹 `pipeline_sh_sha256=1bf3e07b9486b9a8b5b596df3823c78e65c4a58b2cffc31e5fd0a16d25233b42`（旧 `0a2e133157760ce0b4a715b293d3718574b495772a46b48cac81fc089f72a435`），`stage_next.sh` 同批缩为转发器；另一台机器拉取后先跑一次 `bash 04_scripts/pipeline.sh <游戏> status` 确认不再 die 即为同步成功。
  - 2026-10-15 实例（霓虹深渊2 r19 复盘：`status --brief` + `verify --set`）：新指纹 `pipeline_sh_sha256=5efdea573dc23c381e1f701369abff636afbea760a35fe383ad2b58f05a83029`（旧 `1bf3e07b…`）。同一提交内改 `pipeline.sh`（`status [--brief]`＝跳过 说明/阶段名/前置 与「硬约束+台账」回显；`verify <stage> [--set]`＝校验通过即 `set_status done`，用于「产物已在、状态需补」的收尾）＋刷新 `_common/pipeline.lock`。另一台机器 `git pull --rebase --autostash` 后先跑 `bash 04_scripts/pipeline.sh <游戏> status --brief` 验证不再 die。
  - 2026-10-15 实例二（规范去重：弱模型旧规则退役）：新指纹 `pipeline_sh_sha256=bf5eac4f6eee1da7ce1fb737d42e7bb32a98366733394f94e0f00c14b2a3aebd`（旧 `5efdea57…`）。改动**仅注释与 3 处 die 提示文案**（把原先按模型档次划分的「禁改」表述改成模型无关的变更纪律，指向本节），已用「剥注释逐行比对」证明可执行代码零差异；指纹门禁正/负例实测：刷新后 `status --brief` rc=0、把 lock 改成全 0 后 die `rc=1`。


## 装机预检与验收阶梯（2026-10-27 从 `AGENTS.md` §6 移入，原文保留）

- **装机预检闸门＋验收阶梯 L1/L2/L3**（`04_scripts/_common/`）：装机后、用户上手前先 `bash 04_scripts/_common/post_install_precheck.sh --game <游戏>`（机器只管装机层证据：包在不在、启动崩不崩、菜单 so 加载与弹出、授权墙发卡、空转不崩），**预检全过才请用户上手；挂了直接出 A/B 判定，不消耗用户实测轮**。三层走**同一个包**（同 so、同签名）：**L1 静态**（AI 出包前不碰设备；`check_release_clean.sh --tag <tag>` + 本地产物与包内 so md5 一致 + 菜单串自检）／**L2 机器**（AI 全自动跑模拟器：装机→预检→`flight_recorder.sh` 冷启→`device_profile.sh` 三行自检，自判 PASS/FAIL，**不打扰用户、不消耗实测轮**，通过也**不构成**再测理由）／**L3 人工**（用户唯一终审，AI 只发操作请求卡 + 收 logcat 定案）。**禁止为模拟器单独出包**；设备差异一律落运行时判据（`hook-rules.md` §31）。涉及 `libil2cpp` 基址/hook 的游戏装机前先 `device_profile.sh`（报告落 `_state/设备画像/<serial>.md`，rc=2 判据异常**就不装机**）；游玩取证、标记与切片见 `TOOLBOOK.md` 与 `hook-rules.md`。

## 会话规模与效率阈值（2026-10-27 从 `AGENTS.md` §6 移入，原文保留）

- **会话规模、切点与效率硬指标**（长正文与实测数据在 `10_docs/_state/规范变更日志.md`）：单会话机械行为预算 ≈600 次工具调用 / ≈40 次装机 / ≈150K prune 阴影 token，任一触顶即**交接**（把「已确认事实 / 未决问题 / 下一步 / 证据落点」写进 `games/{游戏}.md` 后开新会话，不硬撑）。① 同一 step 内**下一步动作不依赖其返回**的独立调用必须合并发出（阈值 calls/step ≥1.6、单发占比 <60%）；② 子代理只用于「可独立落盘的深度探查」，不派「帮我统计 / 帮我找文件」类；**子代理不可访问会话历史工具（`session_event_search`/`session_event_read`/`session_trace` 等）——依赖会话历史/上下文的探查（含会话重建）必须主代理自己做，派给子代理必空转**（2026-10-18 实测 3/3 子代理在会话工具处失败、历史决策官会话子代理单条回传仅 53 字符）；③ 子代理回报只允许「结论 ≤20 行 + 落盘路径 + 行数/md5 + 未闭环项」，**单条入父 ≤3,000 字符**；④ 同一共享文件被触达 >10 次、或连续 3 个 step 单发，即先 `compress` / 写交接再继续；`compress` 触发点前移到上下文 ≈90 K，一个压缩条目只装一个主题；同一状态文件每会话触达 ≤4 次（结论变化 + 收尾各一次）。
