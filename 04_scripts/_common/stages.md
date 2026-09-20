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
| **机械操作**（拆包/打包/注入/移植/签名/构建/装机/清理） | `bash 04_scripts/pipeline.sh <游戏> <动作>` | 弱模型禁止直接敲 adb/apktool/zip/ndk-build/jarsigner 等命令序列；报错时读日志与校验输出，禁止换命令试错 |
| **分析操作**（逆向/锚点抓取/反作弊研判/方案设计） | 照常自由推理 | 产出必须落 `10_docs/{游戏}/` 文档（证据闭环不变）；状态机的"锚点抓取"阶段产出入文档、指针记录入 pipeline.json |

机械管线文件（pipeline.sh / pipeline.lock）**禁止修改**：入口启动即校验 sha256 指纹（登记于
`_common/pipeline.lock`），被改动直接拒绝执行。合法参数修改走 pipeline.json 的 `params`
字段（game_id/版本/设备 serial 等——该改的给它改，脚本本体不动）。

## 弱 AI 使用词汇表（全部操作只此四个动词）

```bash
bash 04_scripts/pipeline.sh <游戏> status        # 开工第一步：看当前在哪
bash 04_scripts/pipeline.sh <游戏> next          # 执行下一阶段（自动路由）
bash 04_scripts/pipeline.sh <游戏> run <stage_id> # 显式重跑某阶段（重跑前建议先 reset）
bash 04_scripts/_common/preflight.sh <文件...>    # 改完任何文件必跑的机器关
bash 04_scripts/_common/pre_install_gate.sh ...   # 装机前产物四查门禁（见下方专节）
```

辅助：`reset <stage_id>`（状态回退 pending）、`verify <stage_id>`（只校验产物不执行）。

### preflight 机器关（改完文件的必经之点，2026-09-09 起）

- 改完任何 `.sh/.py/.js/.json` 文件 → 必跑 preflight（bash -n / python compile / node --check / json.tool，多符号少括号立刻抓）
- 改完 `.cpp/.mk` → preflight 会 REMIND：必须重跑管线构建阶段验证（C++ 无独立语法检查器，构建才是检查）
- 管线阶段可在 json 里声明 `preflight` 文件列表（相对 workdir 或绝对路径）：执行阶段命令**前**自动过机器关，FAIL 即拦截——散装改脚本没过检查就跑，在管线侧不可能发生
- 弱 AI 会话级注入纪律见 `weak-ai-system-prompt.md`（贴到 Cursor/Claude Code 等工具 Rules）

### 装机前产物门禁 pre_install_gate.sh（2026-09-17 起，install 阶段必过）

> 来源：18 份会话记录审计——多游戏装机失败属机器可拦截错误（JS 拼接截断、so/dex JNI 配套不齐、
> P/Invoke 符号不符、滑块被 getRet 布尔读），每次都烧掉一整轮装机+用户实测。

```bash
bash 04_scripts/_common/pre_install_gate.sh \
  --js <注入JS产物> [--js <更多>] \        # 查1 JS语法（截断缺尾/语法错）
  --cpp <Mod源码> --dex <包内dex> [--dex <更多>] \  # 查2 JNI配套（so注册方法 vs dex声明）
  --so <so> --pinvoke-json <EntryPoint清单> \  # 查3 P/Invoke符号 vs so导出表
  --menu-json <菜单模板> --cpp <含getVal/getRet的源码>  # 查4 控件类型 vs 读取方式
```

- 各查独立可选（传了才查），有 FAIL 即禁止装机（exit 1）
- 查4 局限：只抓字面量索引调用（`getRet(4)` 形式）；变量传参（`getRet(n)`）靠人工核对
- 检出 FAIL 若属故意保留（如兼容空实现），须在装机报告显式说明并经用户确认才可放行
- 与 preflight 职责区分：preflight 管"改完源文件"的语法；本门禁管"装机前"的产物交叉核对

## 标准阶段表（新游戏从模板生成后按此裁剪）

| # | stage_id | 名称 | 前置 | 后端命令形态 | 产物与校验 |
|---|----------|------|------|--------------|------------|
| 1 | `check_apk` | 官包核对 | — | md5/大小/包名核对（对照 02_apks 登记） | 官包指纹入 json |
| 2 | `dump` | 拆包 dump | 1 | decode_apk.sh + Il2CppDumper / jsc 解密 | dump.cs 等产物存在 |
| 3 | `anchor` | 锚点抓取 | 2 | 分析性阶段（AI 自由推理），产出落 10_docs | 锚点文档路径入 json |
| 4 | `impl_mod` | 功能实现 | 3 | new_game_mod.sh 四件套 + 填 TODO | Mod/Entry/mk 三件套存在 + 构建通过 |
| 5 | `build_so` | 构建 so | 4 | ndk-build（私有 mk + 独立 obj；**换证书须先 `xxd -i` 重烘 `CertPinned.h`**，只换 dex/assets 无效） | so 含游戏标签 + JNI_OnLoad 导出 + md5 入 json；后端防呆四件套：**绝对路径 NDK_OUT/LIBS_OUT + obj stamp（mk/探针开关变化清 obj 重编）+ 链接活动保险（有新 .o 而 so 未变=FAIL）+ 菜单串按字节校验**（范本 `04_scripts/魂坠深境/hzsj_build_so.sh`） |
| 6 | `surgical` | 外科注入 | 5 | surgical_{游戏}_{版本}.sh（迭代场景后端配 `*_so_only.sh`，见下方内循环） | 注入包存在 + so 标签复核 + **包内 so md5 == build_so 产物**（旧 so 混包直装防线） |
| 7 | `sign` | 签名 | 6 | uber-apk-signer | 签名包存在 + hash 入 json |
| 8 | `device_ready` | 设备准备 | — | adb devices / connect（MuMu `127.0.0.1:5555`，重启后端口可能变 16384）/ 端口转发（MT MCP `adb forward tcp:8787 tcp:8787`） | 目标设备在线 |
| 9 | `install` | 装机验证 | 7,8 | **装机前必过 `_common/pre_install_gate.sh` 四查**（JS 语法/JNI 配套/P/Invoke 符号/控件读取，见上方专节）→ adb install（须真机授权；大包 push+pm install；覆盖安装失败不自动卸载） | 安装成功日志 + **`_common/verify_installed_apk.sh` 设备包 md5 == 本地 APK**（root 真机加 `--su`；防"装机成功"假象装旧包） |
| 10 | `archive` | 收官归档 | 9 | 文档/游戏摘要 + current.md 单行索引 + `_common/archive_scoped_git.sh` 白名单**提交并推送 origin/main**（用户口令「git」= 提交+推送连体；仅明确不想推送时才 `--no-push`） | 索引行存在 + 唯一正式包/裁剪清单存在 + 远端已同步 |
| 11 | `archive_close` | 状态收口提交 | 10 | pipeline 写回 archive done 后，运行 `_common/archive_close_git.sh --game {游戏}`，只提交 `pipeline.json` 状态变化并推送（同上，`--no-push` 才关） | 本作路径无未提交状态 + 远端已同步 |

规则：
- **顺序执行**：`next` 只取第一个 pending 阶段；跳步=用 `run` 显式指定（会做前置检查，前置未满足直接拒绝）。
- **失败即停**：阶段失败状态置 `failed` 并附日志路径；禁止换命令绕过，修完原因后 `reset` 该阶段重跑。
- **产物即证据**：每阶段完成把产物指纹（md5/标签检查结果）写回 json——"改错文件返工"由此兜底：下一阶段前置校验产物指纹，改动过的产物立刻暴露。
- **inputs 快照与陈旧度（2026-09-10 起，防旧产物直装）**：阶段完成时记录上游产物 md5 快照。快照≠现状 = 该阶段过期：① 仍处 done 的过期阶段被**自动回退 pending**（上游产物一变化即触发，`reset 上游忘重跑下游`不再可能装旧包）；② 过期链路上的阶段**拒绝执行**并点名环节（改了源码没重编 so、换了 so 没重打 APK 都会拦），remedy = `reset <被点名阶段>` 后 `next` 连跑；③ 产物未变化的重跑不引发级联（幂等）。`pipeline.json` 状态文件与 `*.md` 活文档不参与指纹/新鲜度判定。
- **每游戏一份 pipeline.json**，位于 `04_scripts/{游戏}/pipeline.json`；新游戏由强 AI 生成（或后续并入 new_game_mod.sh 生成第五件套）。
- 日志统一追加至 `04_scripts/{游戏}/pipeline.log`。完整日志是本地可再生物，
  `.gitignore` 已排除；Git 只保留结论、决定性日志行和复现命令。

## 调试内循环（最高频场景：改 Mod → 重打 → 装机 → 看日志 → 再改）

已有外科底包的迭代**不需要重走完整标准阶段**，管线标准动作：

```bash
bash 04_scripts/pipeline.sh <游戏> reset build_so   # 回退构建阶段
bash 04_scripts/pipeline.sh <游戏> next             # 连跑到 install（build_so → surgical → device_ready → install）
```

- **防旧包直装是自动的（2026-09-10 起）**：reset 上游后忘 reset 下游不再需要记——上游产物一变化，
  过期的下游 done 阶段自动回退 pending（终端会打 `⚠ 自动回退 [...]`），连跑 `next` 整条链刷新；
  改了源码直接 `run` 下游会被陈旧度门禁拒绝并点名过期环节。产物未变化的重跑不会级联（幂等）。
- surgical 阶段的 `command` 配 `*_so_only.sh` 后端（每游戏 pipeline.json 自行声明；so_only 语义=已有底包只换 so 重签，不碰原包）
- 装机后的**观测采集属分析操作**（非管线阶段，AI 按诊断需要执行并记录）：
  - `adb logcat -s StarryMagic StarryTrace`（功能/hook 日志）
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

1. 文档收尾：游戏文档更新（结论/证据/遗留）；详情写入
   `10_docs/_state/games/{游戏}.md`，`current.md` 只同步一行摘要。反复反馈的
   功能补 `10_docs/_templates/功能验收卡.md`，多日志证据补
   `evidence_日期/README.md`。
2. 正式包：核对指定正式包签名/哈希后，执行 `bash 04_scripts/_common/prune_output_final.sh 08_output/{游戏} {正式包} --apply --manifest 10_docs/{游戏}/正式包裁剪清单_日期.json`；最终目录只保留一个正式 APK，被删候选包/idsig 的 hash/大小入清单
3. 官包归位：02_apks（用户手动管理）
4. 滚动物清理：收官游戏删 03/06 大二进制/07 大包（git 有文本；只清理本游戏路径；删前逐项核对再生依据；中文路径先 `git config core.quotepath false`）。
   **清理后必须同步修正 `{游戏}/pipeline.json` 快照**（2026-09-19 补，弓箭传说2 踩坑；2026-09-20 起脚本化）：`archive` 执行前的 `check_stale` 会遍历**所有祖先阶段**的 `inputs` 快照逐个比对 md5，被清理删掉的中间产物（unsigned APK、`07_patched` 留证、用户手动删除的 `02_apks` 官包）会让它判定「上游链路过期」并拒绝执行——**清理越彻底越跑不过**。先跑 dry-run：

   ```bash
   python3 04_scripts/_common/pipeline_record_cleanup.py \
     --game {游戏} --manifest {清理或裁剪清单}.json
   ```

   核对输出后加 `--apply`。脚本会把命中的 `verify` 改为 `not_exists`、删除
   对应 `artifacts`/下游 `inputs`，并把原 md5 追加到 stage `note`；`archive.verify`
   仍应显式声明应清理的中间产物。**顺序：清理 → 脚本修快照 → 跑 `archive`**。
5. 设备残留：真机会话按 TOOLBOOK「真机残留清理」回收证据与残留
6. **git 固化**：优先用 `_common/archive_scoped_git.sh --path ... --message ...`，
   只提交本任务白名单，**提交后默认推送 origin/main**（用户口令「git」= 提交+推送连体，
   不需要再问「要不要推」）。仅当明确不想推送时才加 `--no-push`，且必须在汇报里写明
   「已提交未推送 + 远端落后几个提交」。push 冲突/网络失败则保留本地提交并明确报告，
   不自动强推。
   `--path` 只传**非忽略**路径（2026-09-20 实测）：传 `.gitignore` 覆盖的
   `mod菜单/**/app/src/libs*/` 下副本时，`git add -A` 打印「ignored by one of your
   .gitignore files」并返回非 0 → 脚本 `set -e` 中止，已暂存内容需 `git reset` 收拾
   （`obj*/`、`.gradle/` 实测不中止）。星光项目收官只提交
   `app/src/main/jniLibs/arm64-v8a/libstarry_magic.so` 那份（gradle 实际打包的），
   其余同内容副本在收尾清单写明「忽略目录副本，保留未暂存」。
7. 管线写回 `archive=done` 后，运行
   `_common/archive_close_git.sh --game {游戏}` 收口状态，避免手工补第二次提交。
8. 验证收口：本任务相关路径无未提交改动；若工作区仍有无关改动，列表报告且不纳入提交。`current.md` 有本游戏单行索引 + 汇报 CLAIM/VERDICT/EVIDENCE。

## 现有脚本资产的角色

113 个存量脚本（surgical_* / *_so_only / inject_*）是各游戏各阶段的**后端**，pipeline 只做
路由/状态/校验，不重写。存量游戏可逐步接入（出包时顺手），新游戏出生即接入。
