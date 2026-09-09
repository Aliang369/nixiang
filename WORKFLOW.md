# WORKFLOW.md - 逆向工作流程规范

> 读取、授权、协作、测试与清理政策唯一入口：`AGENTS.md`。
> 工作区：`/Users/aliang/Documents/nixiang`；本书按任务查阅，不是启动必读协议。
> 工具版本/可用性是历史记录，使用前按需核验。

## 0. 执行政策入口

通用政策见 `AGENTS.md`；本文件保留目录、构建、动态分析与验证细节。Cursor 指针见 `.cursor/rules/apk-build-surgical.mdc`、`.cursor/rules/self-hosted-trace.mdc`；接手阶段参考 `PLAYBOOK.md`，知识索引见 `skill/SKILL.md`，工具细节查 `TOOLBOOK.md`。

---

## 1. 标准目录

| 目录 | 用途 | 本次会话规则 |
|------|------|--------------|
| `02_apks/{游戏}/` | 原始 APK | 用户手动管理，不默认整理或覆盖 |
| `03_decompiled/{游戏}/` | jadx / 静态分析产物 | 可复用中间产物 |
| `04_scripts/{游戏}/` | 自写 hook/trace/构建脚本 | 游戏专属脚本放这里 |
| `05_ida/{游戏}/` | IDA 工程 | 逐项保留/再生判断见 TOOLBOOK「清理规则」 |
| `06_dumped/{游戏}/` | dump/dex/so/解密产物 | 逐项保留/再生判断见 TOOLBOOK「清理规则」 |
| `07_patched/{游戏}/` | 待重打包工作树 | 逐项保留/再生判断见 TOOLBOOK「清理规则」 |
| `08_output/{游戏}/` | 成品 APK | 用户手动管理，不默认整理或覆盖 |
| `09_test/{游戏}/` | 测试日志、截图、抓包 | 逐项保留/再生判断见 TOOLBOOK「清理规则」 |
| `10_docs/{游戏}/` | 分析笔记与复现文档 | **必留** |
| `11_proj_tools/{游戏}/` | 长期工具/签名/后台工具 | 删前确认 |
| `12_images/{游戏}/` | 宣传图 | 删前确认；不要放进 10_docs |
| `99_archive/` | 已完成/放弃项目归档 | 不默认大包归档 |

---

## 2. 新游戏/新版本接手路径

1. 原包放入 `02_apks/{游戏}/`
2. 只做静态分析时:
   - `jadx -d 03_decompiled/{游戏}/jadx_版本/ 02_apks/{游戏}/原包.apk`
   - 查看 Manifest、dex 结构、lib、assets,判断引擎与壳
3. 需要 APK 结构/Smali 时,**优先 MT MCP**,而不是把整个游戏包 `apktool d/b`:
   - 手机 MT 打开 APK/已装应用 → `adb forward tcp:8787 tcp:8787` → 启用 `MT_APK_MCP`
4. 建立本游戏配置笔记:
   - `10_docs/{游戏}/{游戏}_执行配置_YYYYMMDD.md`
   - 内容至少包括:包名、原包路径、`game_id`、引擎、壳/保护、可动文件、入口方案、签名密钥安全引用（无明文）、是否允许干净安装。

---

## 3. 静态分析路径

**Java 层**
- 首选:`jadx` 搜索关键字符串/类/方法。
- 快速 xref / Smali / 字符串:MT MCP。
- 不把 MT 当运行时 Hook。

**Native 层**
- `.so` 拉入 IDA Professional 9.3。
- FindCrypt 仅在需要加密算法定位时运行；Unity IL2CPP 缺符号且需定位托管方法时用 Il2CppDumper；Mono 用 ilspycmd。
- 改指令优先 IDA Keypatch;文档记录到 `10_docs/{游戏}/`。

**保护面探测**
- 用 MT 或最小改动 A/B,确认哪些 dex/arsc 可动。
- 对带 TSS/ACE 的游戏,先按公主连结经验:
  - `classes4` / `classes8` 这类 ACE/TSS 宿主 dex 通常不可动。
  - 可增改的 dex、新增 so、assets、Manifest 可能可动。
  - 每款游戏都要实测,不能抄结论。

---

## 4. 构建/打包路径(核心)

> **管线优先（2026-09-09 起）**：构建/打包/签名/装机等**机械操作已收进统一管线**——
> AI 一律走 `bash 04_scripts/pipeline.sh <游戏> status/next/verify/reset`（规则见
> `04_scripts/_common/stages.md`，含最高频的"调试内循环"：reset build_so → next 连跑到 install）。
> 本节手工流程仅供理解后端原理、排障和强 AI 选脚本参考，**不是执行入口**。

### 4.1 工具选择(先看再跑)

| 改了什么 | 首选 | 是否 apktool b 游戏 |
|----------|------|--------------------|
| 只改 `Games/*Mod.cpp` / native | `*_so_only.sh` | 否 |
| 首次出包 / 改菜单 / smali / assets / Manifest | 外科组装 | 否 |
| 小改动试一下 | MT `mt_apk_build` 或 `smali/baksmali` | 否 |
| 无保护普通游戏且明确需要 | 原 `inject_*.sh` | 仅在 AGENTS.md 要求的具体授权与对照证据满足后 |

### 4.2 外科组装在做什么

以现有公主连结样例为蓝本:
`mod菜单/starry-mod-menu/scripts/surgical_pcr_11.7.2.sh`

1. 只构建小模板 `starry-mod-menu`,不构建游戏 APK。
2. `apktool d` 小模板 APK,取 `com/starry` smali、`libstarry_magic.so`、certs。
   - 若只需要证书/so,直接用 `unzip` 从小模板 APK 提取,不要误以为证书保护必须对游戏包跑 apktool。
3. `smali assemble` 生成新的 `classesN.dex`。
4. 从预编译/已验证的二进制 Manifest 取 `AndroidManifest.xml`。
5. Python zip 合并原包:
   - 原包受保护 dex / `resources.arsc` 字节级不动。
   - 只替换 Manifest,新增 dex、so、assets。
6. `zipalign` + `uber-apk-signer` 签名。
7. 通过安装前可测试门禁且目标设备测试已授权后才设置 `STARRY_INSTALL=1`；覆盖安装，降级参数 `-d` 仅在目标版本确有需要且获授权时使用。

**当前执行方式**：按游戏手写 `surgical_{游戏}_{版本}.sh`，参考已有 `surgical_pcr_11.7.2.sh` / `surgical_gyws_1.0.2.sh` 样例。不追求通用脚本，每款游戏保护面不同，手写更可控。

### 4.3 只换 so 在做什么

1. `ndk-build` 编译 `libstarry_magic.so`。
2. 从已有成品生成独立工作副本，再 `zip -u` 写入；不默认覆盖用户管理的最终包。
3. 重签并可选覆盖安装。
4. **前提**:磁盘上已有一份可用的正式外科包。

公主连结样例:
`mod菜单/starry-mod-menu/scripts/pcr_so_only.sh`

### 4.4 允许 `apktool` 的边界

- `mod菜单/starry-mod-menu` 小模板可做 `apktool d/b`。
- 游戏包默认外科组装；整包重建例外统一按 `AGENTS.md` 的具体授权与对照证据条件，不由脚本名称推定允许。

---

## 5. 动态分析路径(自注入 Trace 优先)

> 涉及装机/装包的机械步骤（下述 5.1 的 2/3/5 步）同 §4 **管线优先**；
> logcat/dropbox 采集属分析观测，AI 按诊断需要自由执行并记录。

### 5.1 默认:游戏内自注入 Hook

执行方式:

1. 在 `libstarry_magic.so` 内加 `STARRY_TRACE=1` 编译。
2. 用 `*_so_only.sh` 或外科包覆盖安装。
3. 启动游戏,抓 `adb logcat -s StarryTrace`。
4. 定位到目标函数/偏移后写进 `Games/{游戏}Mod.cpp`。
5. 关闭 Trace,重编正式 so,覆盖安装验证。

自注入链路:
- Java 入口:`StarryApp` 加载 `libstarry_magic.so`。
- native hook:Dobby 改函数头。
- Java hook:`JNI RegisterNatives` 或接入 Pine/SandHook/Whale。
- 内存读写:xDL / `memdump64`（KittyMemory 为遗留依赖，全仓零调用，勿在新代码使用）。
- 日志标签:`StarryTrace`。

**适用**
- TSS/ACE/反调试强保护游戏。
- Frida 秒断、闪退、被检测的游戏。
- 热路径需要稳定观察的场景。

### 5.2 备选:Frida

仅用于无壳、无强反作弊游戏,且**短时探针**:
- server:`04_scripts/_common/start_frida_server.sh`（phantom-frida 17.16.4，随机名/随机端口；选型与实测见 TOOLBOOK「Frida — 动态 Hook」及 `01_tools/phantom-frida/README.md`）
- 版本:server 17.16.4 ↔ 客户端 `frida==17.16.4` 原生匹配，无需 pip 降级
- 禁止热路径长挂。
- 结束按 `AGENTS.md` 先保全证据、核对脚本范围与清理授权，再处理本轮残留。

### 5.3 抓包 / 解密

- HTTP/HTTPS:`mitmproxy`;SSL Pinning 仍优先短时脚本,不做长挂。
- dump/dex/so:产物统一 `06_dumped/{游戏}/`。

---

## 6. 验证与收尾

### 6.1 分阶段验证

安装前按 `AGENTS.md` 的「可测试」门禁核对；运行结果缺证据可标 NOT PROVEN 后受控测试，不要求安装前证明运行效果。以下为装机后检查，满足交付范围运行证据才可标「可交付」。

每次装机后

- [ ] 覆盖安装成功,未卸载大包游戏。
- [ ] 日志命中目标函数/`auth game_id synced from native: <id>` 正确。
- [ ] 菜单 `feat_num` 变更时后台模板同步。
- [ ] 产物在 `08_output/{游戏}/` 并标注可测试/运行验证/可交付阶段，中间包未误当最终包。

### 6.2 会话结束/暂停/换游戏

按 [TOOLBOOK.md](TOOLBOOK.md)「清理规则」「真机残留清理」完成收尾，填写 [游戏收尾与最小复现清单](10_docs/_templates/游戏收尾与最小复现清单.md)。设备动作按本次变更分责记录；现有清理脚本未改、不默认运行，无法约束范围时报告待处理。

### 6.3 文档必留

- 每个游戏必须有 `10_docs/{游戏}/` 的当前执行手册或 README。
- 关键结论:保护面、可动文件、入口、偏移、脚本、构建方式、验证结果。
- 文档是复现入口，不替代源码与必要输入；最小保留清单及隔离重建证据见 TOOLBOOK「清理规则」。
- 作废路线单独标注"作废",不要混进现行步骤。

---

## 7. 快速参考

| 任务 | 首选 | 备选 |
|------|------|------|
| 看 Java 源码 | jadx | MT MCP |
| 看 smali/资源/xref | MT MCP | jadx |
| 分析 .so | IDA Pro | Ghidra |
| Unity IL2CPP | Il2CppDumper | - |
| Hook Java | 自注入 `libstarry_magic.so` | 短时 Frida |
| Hook Native | Dobby 自注入 | 短时 Frida |
| 内存读写 | KittyMemory / `memdump64` | `/proc/pid/mem` |
| dump DEX | frida-dexdump(普通游戏) | BlackDex |
| 抓包 | mitmproxy | - |
| 重打包游戏 | 外科 zip 组装 | MT `mt_apk_build` |
| 只换 so | `*_so_only.sh` | `zip -u` |
| 签名 | `01_tools/uber-apk-signer.jar` | apksigner |
| 真机收尾 | TOOLBOOK「真机残留清理」 | 现有脚本未改，不默认运行 |
