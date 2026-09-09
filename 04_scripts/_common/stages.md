# 机械操作管线 — 标准阶段定义（stages.md）

> 配套统一入口 `04_scripts/pipeline.sh`。本文定义全游戏通用的标准阶段、
> 前置条件与产物校验要求；每游戏的具体命令/参数在 `04_scripts/{游戏}/pipeline.json`。

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
```

辅助：`reset <stage_id>`（状态回退 pending）、`verify <stage_id>`（只校验产物不执行）。

### preflight 机器关（改完文件的必经之点，2026-09-09 起）

- 改完任何 `.sh/.py/.js/.json` 文件 → 必跑 preflight（bash -n / python compile / node --check / json.tool，多符号少括号立刻抓）
- 改完 `.cpp/.mk` → preflight 会 REMIND：必须重跑管线构建阶段验证（C++ 无独立语法检查器，构建才是检查）
- 管线阶段可在 json 里声明 `preflight` 文件列表（相对 workdir 或绝对路径）：执行阶段命令**前**自动过机器关，FAIL 即拦截——散装改脚本没过检查就跑，在管线侧不可能发生
- 弱 AI 会话级注入纪律见 `weak-ai-system-prompt.md`（贴到 Cursor/Claude Code 等工具 Rules）

## 标准阶段表（新游戏从模板生成后按此裁剪）

| # | stage_id | 名称 | 前置 | 后端命令形态 | 产物与校验 |
|---|----------|------|------|--------------|------------|
| 1 | `check_apk` | 官包核对 | — | md5/大小/包名核对（对照 02_apks 登记） | 官包指纹入 json |
| 2 | `dump` | 拆包 dump | 1 | decode_apk.sh + Il2CppDumper / jsc 解密 | dump.cs 等产物存在 |
| 3 | `anchor` | 锚点抓取 | 2 | 分析性阶段（AI 自由推理），产出落 10_docs | 锚点文档路径入 json |
| 4 | `impl_mod` | 功能实现 | 3 | new_game_mod.sh 四件套 + 填 TODO | Mod/Entry/mk 三件套存在 + 构建通过 |
| 5 | `build_so` | 构建 so | 4 | ndk-build（私有 mk + 独立 obj；**换证书须先 `xxd -i` 重烘 `CertPinned.h`**，只换 dex/assets 无效） | so 含游戏标签 + JNI_OnLoad 导出 + md5 入 json |
| 6 | `surgical` | 外科注入 | 5 | surgical_{游戏}_{版本}.sh（迭代场景后端配 `*_so_only.sh`，见下方内循环） | 注入包存在 + so 标签复核 |
| 7 | `sign` | 签名 | 6 | uber-apk-signer | 签名包存在 + hash 入 json |
| 8 | `device_ready` | 设备准备 | — | adb devices / connect（MuMu `127.0.0.1:5555`，重启后端口可能变 16384）/ 端口转发（MT MCP `adb forward tcp:8787 tcp:8787`） | 目标设备在线 |
| 9 | `install` | 装机验证 | 7,8 | adb install（须真机授权；大包 push+pm install） | 安装成功日志 |
| 10 | `archive` | 收官归档 | 9 | 文档收尾 + current.md 索引 + **git 固化**（add -A → commit → push） | 索引行存在 + git status 干净 |

规则：
- **顺序执行**：`next` 只取第一个 pending 阶段；跳步=用 `run` 显式指定（会做前置检查，前置未满足直接拒绝）。
- **失败即停**：阶段失败状态置 `failed` 并附日志路径；禁止换命令绕过，修完原因后 `reset` 该阶段重跑。
- **产物即证据**：每阶段完成把产物指纹（md5/标签检查结果）写回 json——"改错文件返工"由此兜底：下一阶段前置校验产物指纹，改动过的产物立刻暴露。
- **每游戏一份 pipeline.json**，位于 `04_scripts/{游戏}/pipeline.json`；新游戏由强 AI 生成（或后续并入 new_game_mod.sh 生成第五件套）。
- 日志统一追加至 `04_scripts/{游戏}/pipeline.log`。

## 调试内循环（最高频场景：改 Mod → 重打 → 装机 → 看日志 → 再改）

已有外科底包的迭代**不需要重走 10 阶段**，管线标准动作：

```bash
bash 04_scripts/pipeline.sh <游戏> reset build_so   # 回退构建阶段
bash 04_scripts/pipeline.sh <游戏> next             # 连跑到 install（build_so → surgical → device_ready → install）
```

- surgical 阶段的 `command` 配 `*_so_only.sh` 后端（每游戏 pipeline.json 自行声明；so_only 语义=已有底包只换 so 重签，不碰原包）
- 装机后的**观测采集属分析操作**（非管线阶段，AI 按诊断需要执行并记录）：
  - `adb logcat -s StarryMagic StarryTrace`（功能/hook 日志）
  - `adb shell ls /data/system/dropbox/`（闪退归因：`data_app_crash`=Java 层、`data_app_native_crash`=native 层，分开计数）
- 迭代收敛后把最终 so 指纹与验证结论落 10_docs 游戏文档

## 收官口令（用户说「收尾+正式包+清理+git」时，AI 展开此清单）

1. 文档收尾：游戏文档更新（结论/证据/遗留），current.md 索引行同步
2. 正式包：08_output 唯一留存（用户手动管理，AI 只核对 hash 并记录）
3. 官包归位：02_apks（用户手动管理）
4. 滚动物清理：收官游戏删 03/06 大二进制/07 大包（git 有文本；删前 `git status` 必须干净；中文路径先 `git config core.quotepath false`）
5. 设备残留：真机会话按 TOOLBOOK「真机残留清理」回收证据与残留
6. **git 固化**：`git add -A && git commit -m "<描述>" && git push`（git 资产有变更即固化；push 冲突/报错停下问用户）
7. 验证收口：`git status` 干净 + current.md 有本游戏行 + 汇报 CLAIM/VERDICT/EVIDENCE

## 现有脚本资产的角色

113 个存量脚本（surgical_* / *_so_only / inject_*）是各游戏各阶段的**后端**，pipeline 只做
路由/状态/校验，不重写。存量游戏可逐步接入（出包时顺手），新游戏出生即接入。
