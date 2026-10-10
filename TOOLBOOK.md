# TOOLBOOK.md — 安卓逆向工具文档与使用规范

> 📅 最后更新：2026-09-29（正式交付目录、安全隔离清理、Build Tools 自动探测）
> 📍 工作区：/Users/aliang/Documents/nixiang
> 🎯 用途：个人安卓游戏逆向学习
> 读取、授权与协作唯一入口：`AGENTS.md`；本书按工具问题查阅，不另设优先级或必读协议。

## 执行政策与历史观测说明

通用政策统一见 `AGENTS.md`；接手阶段按需查 `PLAYBOOK.md`；构建选型见 `WORKFLOW.md` §4（native → `*_so_only.sh`；首包/菜单/smali/assets/Manifest → 外科组装；快速试改 → MT MCP / smali+baksmali）。样板：`mod菜单/starry-mod-menu/scripts/surgical_pcr_11.7.2.sh`。

**以下工具清单、已安装/未安装标签、版本、路径、环境变量、端口与 MCP 工具暴露情况均为文中日期的历史观测，不是当前会话的实时事实。** 使用前只核验本次需要的工具、目标文件与连接；不据此默认服务在线、设备已授权或工具可调用。旧命令保留作查阅，执行须满足 `AGENTS.md` 边界。

---

## 一、工具清单与安装状态

### ✅ 已安装

| 工具 | 版本 | 路径/安装方式 | 用途 |
|------|------|---------------|------|
| IDA Professional | 9.3 | `/Applications/IDA Professional 9.3.app` | Native .so 反编译分析（主力） |
| Java | 25.0.3 LTS | 系统安装 | jadx/apktool 运行依赖 |
| Python | 3.11.10 | 系统安装 | 脚本运行、算法还原 |
| Homebrew | 6.0.15 | 系统安装 | 包管理 |
| jadx | 1.5.6 | brew install jadx | APK/DEX → Java 源码反编译 |
| apktool | 3.0.3 | brew install apktool | 解包/重打包 APK |
| scrcpy | 4.1 | brew install scrcpy | 投屏控制 Android 设备 |
| android-platform-tools (adb) | 1.0.41 | brew install --cask android-platform-tools | adb 设备控制 |
| mitmproxy | 12.2.3 | brew install --cask mitmproxy | HTTP/HTTPS 抓包 |
| frida（PC 客户端） | **17.16.4**（2026-09-10 实测，两个 CLI path 均此版本，与魔改 server 原生匹配） | `python3.11 -m pip install --user frida==<ver>`；CLI 用 `$HOME/Library/Python/3.11/bin` | 动态 Hook 核心 |
| frida-tools | 14.4.5 | 同上 | Frida CLI |
| phantom-frida（魔改 server） | 17.16.4（run39.1） | `01_tools/phantom-frida/` + `start_frida_server.sh` | 非强检测游戏动态 Hook（选型依据见 Frida 节与其 README） |
| objection | 1.12.5 | pip3 install objection | Frida 封装，快速操作 |
| frida-dexdump | 2.0.1 | pip3 install frida-dexdump | 内存 dump DEX（脱壳） |
| pycryptodome | 3.23.0 | pip3 install pycryptodome | AES/RSA/HMAC 加解密 |
| capstone | 5.0.7 | pip3 install capstone | 反汇编引擎，脚本化分析 .so |
| Gradle | 8.13 | `01_tools/gradle-8.13/` | 构建 Android-Mod-Menu 工程（需 JAVA_HOME=JDK 17） |
| Android SDK build-tools | 34/35/36 | `~/Library/Android/sdk/build-tools/` | `zipalign` / `apksigner`（**默认不在 PATH**，见下方签名说明） |
| uber-apk-signer | 1.3.0 | `01_tools/uber-apk-signer.jar` | 一键对齐+签名（日常优先） |

### ✅ 已安装（IDA 插件 + Il2CppDumper）

| 工具 | 版本 | 路径 | 用途 |
|------|------|------|------|
| FindCrypt (findcrypt3) | — | IDA plugins 目录 | 自动识别加密算法特征（AES/RSA/SHA 等） |
| Keypatch | 0.9.2 | IDA plugins 目录 + keystone 0.9.2_1 (brew) | 在 IDA 中 patch native 代码 |
| Il2CppDumper | 6.7.46 | `01_tools/Il2CppDumper/` | Unity IL2CPP 符号还原 |
| .NET Runtime | 6.0.428 | `~/.dotnet/` | Il2CppDumper 运行依赖（已写入 .zshrc PATH） |
| yara-python | 4.5.4 | brew python3.11 site-packages | FindCrypt 依赖 |
| keystone (brew) | 0.9.2_1 | `/opt/homebrew/lib/libkeystone.dylib` | Keypatch 动态库依赖 |
| keystone-engine (pip) | 0.9.2 | brew python3.11 site-packages | Keypatch Python 绑定 |
| xxtea (pip) | — | brew python3.11 site-packages | Cocos XXTEA 解密 |
| ilspycmd | 10.1.1.8388 | `~/.dotnet/tools/` | .NET DLL 反编译（AI 可用，CLI 工具） |
| .NET SDK | 10.0.302 | `~/.dotnet/` | ilspycmd 运行依赖（DOTNET_ROOT 已写入 .zshrc） |

### ❌ 未安装 — 后续按需安装

### ✅ 动物餐厅云存档改档工具（2026-08-06 新增）

| 工具 | 路径 | 用途 |
|------|------|------|
| 云存档网页工具 | `11_proj_tools/动物餐厅/cloud_save_tool/server.py` | 读取/修改云端存档配置，网页 `http://127.0.0.1:8787` |
| mitmproxy 改档插件 | `11_proj_tools/动物餐厅/cloud_save_tool/modify_addon.py` | 拦截 `uploadEncryptRecord/checkBlackInfo/checkRecord` 自动改档 |
| 加解密核心 | `11_proj_tools/动物餐厅/cloud_save_tool/cloud_save_core.py` | 平台 AES/存档字段/签名共用逻辑 |
| 密钥同步 | `11_proj_tools/动物餐厅/cloud_save_tool/sync_key.py` | 同步 key/version/openid 到 `state.json` |
| 已知字段清单 | `11_proj_tools/动物餐厅/cloud_save_tool/known_fields.json` | 自动补齐缺失物品字段 |
| 一键启动/抓钥/清缓存/停止 | `11_proj_tools/动物餐厅/cloud_save_tool/` 下四个 `.sh` | 桌面有对应 `.command` 快捷方式 |
| Frida 抓钥 hook | `11_proj_tools/动物餐厅/hook_v8_key_动物餐厅.js` | 抓取 `getLatestUserKey` |


### ✅ 战魂铭人 MOD 菜单工程（2026-08-06 新增）

| 文件 | 路径 | 用途 |
|------|------|------|
| 菜单工程 | `11_proj_tools/战魂铭人/mod_menu/` | Android-Mod-Menu 5.0 模板 + 战魂铭人 C++ 功能 |
| 构建注入脚本 | `04_scripts/战魂铭人/mod_menu_build_inject.sh` | 构建/注入/重打包/签名一键执行 |
| 改包工作树 | `07_patched/战魂铭人/apktool_3.4.0_modmenu/` | 已注入 smali/service/主 Activity 调用 |
| 成品 APK | `08_output/战魂铭人/战魂铭人_3.4.0_modmenu_unsigned.apk` | 签名后的改包安装包 |
| 记录 | `10_docs/战魂铭人/战魂铭人_3.4.0_MOD菜单改包.md` | 功能/偏移/注入/验证进度 |

#### 通过 Android Studio 安装

| 工具 | 说明 | 优先级 |
|------|------|--------|
| Android SDK | 提供 build-tools（zipalign/apksigner） | 🔴 重打包时必装 |
| Android Emulator | 模拟器，ARM 镜像 | 🟡 重要 |

安装方式：下载 Android Studio → SDK Manager → 安装 Android SDK + Build Tools + Platform Tools

#### 手动下载到 01_tools/

| 工具 | 下载地址 | 用途 | 优先级 |
|------|----------|------|--------|
| BlackDex | github.com/CodingGay/BlackDex/releases | 备用脱壳工具 | 🟢 可选 |
| dotPeek | jetbrains.com/decompiler/download | .NET/DLL 反编译（Unity Mono 后端） | 🟡 重要 |

> `uber-apk-signer.jar` 已在 `01_tools/`（见上方已安装表），无需再下。

---

## 二、工具使用速查

### jadx — Java 层反编译

```bash
# GUI 模式（推荐日常使用）
jadx-gui 02_apks/应用名_版本.apk

# 命令行模式（批量/脚本化）
jadx -d 03_decompiled/应用名_版本/ 02_apks/应用名_版本.apk

# 只导出 sources 和 resources
jadx -d 03_decompiled/应用名_版本/ --no-imports 02_apks/应用名_版本.apk
```

### apktool — 解包与重打包

以下为历史语法示例，不是游戏整包重建授权；默认仅模板使用，游戏包例外按 `AGENTS.md` 的具体授权与对照证据条件。

```bash
# 解包
apktool d 02_apks/应用名_版本.apk -o 03_decompiled/应用名_版本_apktool/

# 重打包
apktool b 07_patched/应用名_版本/ -o 08_output/应用名_版本_unsigned.apk

# 重打包（不强制资源处理，快速模式）
apktool b -f 07_patched/应用名_版本/ -o 08_output/应用名_版本_unsigned.apk
```

### IDA Pro — Native 分析（按问题选插件）

先明确要回答的问题与已有证据，再选下列步骤；不因打开 .so 就全量扫描、导入或 patch。无相关问题可跳过，记录适用理由。

#### IDA 分析步骤参考（非串行必跑）

**Step 1 — 打开目标 .so**
```bash
# GUI 模式（推荐）
open -a "IDA Professional 9.3" 06_dumped/libxxx.so
# 或命令行
/Applications/IDA\ Professional\ 9.3.app/Contents/MacOS/idat -A -B 06_dumped/libxxx.so
```

**Step 2 — 【按需】FindCrypt：扫描加密算法特征**
- 路径：Edit → Plugins → FindCrypt（快捷键 `Ctrl+Alt+F`）
- **触发条件**：需要定位加密算法或解密路径且现有证据不足时；Cocos/Unity 游戏的加密解密函数（XXTEA/AES/RSA/MD5/SHA）不会明文标注函数名，FindCrypt 通过常量表自动定位
- 扫描后会列出所有匹配的加密算法及其地址，将与问题相关的结果及适用版本记录到 `10_docs/{游戏名}/` 分析笔记中
- 典型场景：找到 AES → 跟进该函数 → 可能是资源解密或网络通信加密

**Step 3 — 【按需】Il2CppDumper：Unity IL2CPP 符号还原**
- **触发条件**：目标是 Unity IL2CPP，且当前问题需要类/方法符号、没有可用同版本结果时执行
- **用途与限制**：补充方法符号；已有符号或问题可由局部证据解决时无需重跑，工具不兼容时不把失败当作无法分析的结论

```bash
# 1. 从 APK 提取两个文件
# libil2cpp.so → lib/arm64-v8a/libil2cpp.so
# global-metadata.dat → assets/bin/Data/Managed/Metadata/global-metadata.dat

# 2. 运行 Il2CppDumper
export PATH="$HOME/.dotnet:$PATH"
cd 01_tools/Il2CppDumper
dotnet Il2CppDumper.dll 06_dumped/libil2cpp.so 06_dumped/global-metadata.dat 06_dumped/应用名_il2cpp/
# 输出：dump.cs（C# 类/方法定义）、script.json（IDA 脚本数据）、stringliteral.json

# 3. IDA 中导入符号
# IDA → File → Script File → 选择 01_tools/Il2CppDumper/ida_py3.py → 选择 script.json
# 等待符号还原完成，函数名将变为 ClassName_MethodName 格式
```

**Step 4 — 字符串搜索：定位关键函数**
- IDA 中 `Shift+F12` 打开字符串窗口
- 搜索关键词：`key`、`secret`、`token`、`encrypt`、`decrypt`、`xxtea`、`sign`、`verify`、`http`
- 双击字符串 → `X`（交叉引用）→ 跳转到引用该字符串的函数
- **必须记录**：找到的密钥、API 地址、关键函数地址

**Step 5 — 分析目标函数**
- F5 反编译为伪 C 代码
- 结合 FindCrypt 结果判断加密算法
- 记录函数签名、参数含义、返回值

**Step 6 — 【按需】Keypatch：直接 Patch Native 代码**
- 路径：Edit → Plugins → Keypatch（快捷键 `Ctrl+Alt+K`）
- **使用场景**：
  - 修改条件跳转（`B.NE` → `B` / `CBZ` → `CBNZ`）绕过校验
  - NOP 掉验证函数（`MOV X0, #0; RET`）
  - 修改硬编码密钥
- **操作流程**：
  1. 光标定位到要修改的指令地址
  2. `Ctrl+Alt+K` 打开 Keypatch
  3. 输入新的汇编指令（支持 ARM64/ARM/x86）
  4. 预览确认后 Apply
  5. IDA 中 Edit → Patch program → Apply patches to input file 导出修改后的 .so
- **替代方案**：如果 Keypatch 不可用，用 IDA 自带 Edit → Patch program（十六进制编辑，不如 Keypatch 直观）

#### IDA 插件速查卡（按问题触发）

| 步骤 | 插件/功能 | 快捷键 | 何时用 | AI 提醒义务 |
|------|-----------|--------|--------|-------------|
| 1 | 打开 .so | — | 总是 | — |
| 2 | FindCrypt | Ctrl+Alt+F | 需定位加密算法且缺证据 | 有相关问题再建议 |
| 3 | Il2CppDumper | 命令行+脚本 | Unity IL2CPP 且缺少所需符号 | 优先复用同版本结果 |
| 4 | 字符串搜索 | Shift+F12 | 问题可通过字面量/xref 定位 | 选与问题相关关键词 |
| 5 | F5 反编译 | F5 | 分析具体函数时 | — |
| 6 | Keypatch | Ctrl+Alt+K | 已授权的指令修改确有需要时 | 先核对改动边界与证据 |

### Frida — 动态 Hook（使用矩阵）

适用边界见 `WORKFLOW.md` §5（5.1 自注入 Trace 默认 / 5.2 Frida 备选）；授权、用户协作与清理见 `AGENTS.md`。参见 `01_tools/phantom-frida/README.md`、`.cursor/rules/frida-florida.mdc` 指针。

**使用矩阵**（2026-09-10 定稿；本地只留一个魔改 server = phantom-frida 17.16.4，官方版与 Florida 已移除——二进制均可随时重下，删除不省成本，留多版反而误用）：

| 游戏检测强度 | 用什么 | 判断方式 |
|------|------|------|
| 强（ACE/TSS/MHP/HTProtect/已知 frida 秒断） | 星光 so 自注入 Trace（唯一路线，见 WORKFLOW §5.1） | WORKFLOW §5 名单 + 历史记录 |
| 中/弱/无（其余全部） | phantom-frida 17.16.4 短时探针；attach 上=赚一轮，被杀=立即撤回走 so | 无保护特征 |

架构依据：so 路线的分析能力与交付功能同载体——用 so Trace 分析通的方案天然已过检测、可直接交付；frida 分析通了还要翻译回 so 再过一遍检测（等于过两次）。强检测游戏 frida 是「先偷懒后还债」。

**phantom-frida 实测结论**（2026-09-10，`grep -a -o` 静态特征计数，SHA256 与发布清单核对一致）：

| 检测特征 | 官方 17.16.4 | phantom 17.16.4 | 结论 |
|------|------|------|------|
| frida:rpc（协议 magic） | 20 | 0 | ✅ 全清 |
| frida-agent / frida-server / frida-gadget（名） | 9/6/2 | 0/0/0 | ✅ 全清 |
| gum-js-loop / gmain（线程名） | 2/4 | 0/0 | ✅ 全清 |
| frida_agent_main（符号） | 3 | 0 | ✅ 全清 |
| re.frida（仅剩 GLib 错误域名串） | 79 | 76 | ◐ 残 GLib 内部串，非路径/memfd 名 |
| gdbus（仅剩 GLib 内部串） | 42 | 38 | ◐ 同上 |
| 默认端口 | 27042 | 39571（随机构建） | ✅ 防默认端口扫描；**`-U` 使用时必须 `-l 127.0.0.1:27042` 覆盖**（见下） |

定性（静态实测 + 项目自述）：~90 补丁覆盖 16 类检测向量，周构建随机改名+随机端口+出包「禁止标记」门禁+真机冒烟验证，build-info.json 全程溯源。静态隐蔽性全面优于 Florida（Florida 留 frida:rpc 16 处/frida-agent 10 处未清）。仍是「症状级」对抗——中级（协议行为）与高级（inline hook 行为特征、ptrace 检测）不保证，强检测游戏一律走 so。

**实机验证 2026-09-10（四项 NOT PROVEN 全转 PROVEN，详见 `01_tools/phantom-frida/README.md`）**：真机 OPD2515/Android 16/KSU Enforcing，目标=魂坠深境 1.1.1 原版——①`-U` 直连 ②spawn+探针 61s 存活且 agent 可响应 ③objection 1.12.5 attach+列 activity ④frida-dexdump 43 dex 落盘，全部通过。**端口铁律**：phantom 出厂随机端口会让客户端 `-U` 静默降级为「jailed adb 兜底」——`frida-ps -U` 假阳性能列进程，spawn/attach 全灭（`need Gadget on jailed Android`）；server 必须以 `-l 127.0.0.1:27042` 启动（start_frida_server.sh 已固化为默认）；隐匿备选=随机端口+`adb forward`+`-H`。**MuMu 12 同日四项同口径全过**（aarch64 原生内核无 houdini，arm64 server 直接可用；多设备用 `-D`/objection `-S`，见 README）。

**魔改谱系落选记录**（选型时实测/核实）：strongR-frida-android（hluwa）仓库已 404 下架；zer0def/undetected-frida 构建流程有 bug——未打补丁的 server 被上传、anti-anti 处理产物未发布（实测线程名全在≈裸奔）；Florida（Ylarod）上游 17.16.x 起只发空 release（CI 停产二进制），且 17.9.1 须降级 pip 客户端，2026-09-10 已从 `01_tools/` 移除。gadget 版按需从 phantom 同 release 下载。

**客户端版本现状**（2026-09-10 实测）：两个 CLI（`$HOME/Library/Python/3.11/bin/frida`、`~/.local/bin/frida`）均为 **17.16.4**，与 phantom server **原生匹配，无需任何 pip 降级**（Florida 时代的降级流程作废）。

```bash
# 启动魔改 server（推到 /data/local/tmp/fs-arm64，phantom 17.16.4，端口随机构建）
04_scripts/_common/start_frida_server.sh

# 客户端版本核对（应输出 17.16.4）
export PATH="$HOME/Library/Python/3.11/bin:$PATH"
python3.11 -c "import frida; print(frida.__version__)"

frida-ps -U
frida -U -n "应用包名" -l 04_scripts/hook.js
frida -U -f "应用包名" -l 04_scripts/hook.js --no-pause

# objection -g "应用包名" explore
```

升级新档（代理示例，详见 `01_tools/phantom-frida/README.md`）：`https_proxy=http://127.0.0.1:7897 curl -sL -o /tmp/phantom-server.gz "https://github.com/TheQmaks/phantom-frida/releases/download/<tag>/ipcmodule-server-<ver>-android-arm64.gz"`，替换后重跑同口径扫描（关键字串应为 0）。

### adb — 设备控制

```bash
# 已授权且通过可测试门禁后覆盖安装；失败不自动卸载
adb install -r 08_output/应用名_版本_modified.apk

# 推送文件
adb push frida-server /data/local/tmp/

# 查看日志
adb logcat | grep -i "关键词"

# 截图
adb exec-out screencap -p > 09_test/screenshot.png
```

### scrcpy — 投屏

```bash
# 基本投屏
scrcpy

# 只投屏不控制
scrcpy --no-control

# 录屏
scrcpy --record 09_test/demo.mp4
```

### mitmproxy — 抓包

```bash
# 启动代理
mitmproxy -p 8080

# Web 界面（更直观）
mitmweb -p 8080

# 手机端设置 WiFi 代理指向电脑 IP:8080
# 安装证书：手机浏览器访问 mitm.it
```

### 签名工具链（zipalign / apksigner / uber）

**要不要装进 PATH？**  
重打包要安装到手机时，**需要「对齐 + 签名」这件事**，但不强制把 `zipalign`/`apksigner` 加进 PATH：

| 方式 | 说明 |
|------|------|
| **推荐** | `java -jar 01_tools/uber-apk-signer.jar ...`（内部会 zipalign；脚本会把 SDK 里的 zipalign 路径传给它） |
| **备选** | 直接调 `~/Library/Android/sdk/build-tools/35.0.0/{zipalign,apksigner}` |
| **可选** | 把某个 build-tools 目录 export 进 PATH，便于手敲命令 |

本机已装 SDK build-tools（34/35/36），`04_scripts/_common/env.sh` 会自动解析路径。  
历史重建脚本入口（内部含 apktool b；使用前阅读脚本，游戏包须满足 AGENTS.md 的例外条件，不作为默认路径）：

```bash
04_scripts/_common/rebuild_sign_install.sh <游戏名> 07_patched/<游戏名>/<目录> [标签] [--install]
```

手敲示例：

```bash
BT="$HOME/Library/Android/sdk/build-tools/35.0.0"

# 对齐
"$BT/zipalign" -v -p 4 08_output/游戏/xxx_unsigned.apk 08_output/游戏/xxx_aligned.apk

# 签名
"$BT/apksigner" sign --ks your.jks --out 08_output/游戏/xxx_signed.apk 08_output/游戏/xxx_aligned.apk

# 或 uber（推荐）
java -jar 01_tools/uber-apk-signer.jar -a 08_output/游戏/xxx_unsigned.apk -o 08_output/游戏/ \
  --zipAlignPath "$BT/zipalign"
```

### frida-dexdump — 脱壳

```bash
# dump 当前运行应用的 DEX
frida-dexdump -U -n "应用包名"

# 指定输出目录
frida-dexdump -U -n "应用包名" -o 06_dumped/应用名_版本/
```

### Il2CppDumper — Unity 符号还原

```bash
# 需要：libil2cpp.so + global-metadata.dat
# 从 APK 中提取：
# libil2cpp.so → lib/arm64-v8a/libil2cpp.so
# global-metadata.dat → assets/bin/Data/Managed/Metadata/global-metadata.dat

Il2CppDumper libil2cpp.so global-metadata.dat 06_dumped/应用名_版本_il2cpp/
# 输出：dump.cs（C# 类/方法定义）、script.py（IDA 脚本）、stringliteral.json
```

### ilspycmd — .NET DLL 反编译（AI 可用）

> **AI 使用场景**：当需要分析 Unity 游戏的 C# 逻辑时，AI 可直接调用 ilspycmd 反编译 .NET DLL，无需 GUI 操作。

```bash
# 确保 dotnet 环境变量（已写入 .zshrc，新终端自动生效）
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"

# 反编译单个 DLL 到控制台
ilspycmd 06_dumped/游戏名/Assembly-CSharp.dll

# 反编译到指定目录（生成 .cs 文件）
ilspycmd 06_dumped/游戏名/Assembly-CSharp.dll -o 03_decompiled/游戏名/ilspy/

# 反编译特定命名空间或类
ilspycmd 06_dumped/游戏名/Assembly-CSharp.dll -t GameLogic.Player

# 反编译 Il2CppDumper 导出的伪 DLL（如有）
ilspycmd 06_dumped/游戏名_il2cpp/Assembly-CSharp.dll -o 03_decompiled/游戏名/ilspy_il2cpp/
```

**典型流程（Unity IL2CPP 游戏）：**
1. Il2CppDumper 从 libil2cpp.so + global-metadata.dat 提取 dump.cs + script.json
2. script.json 导入 IDA 还原符号（见 IDA 流程 Step 3）
3. 如有伪 DLL 输出，用 ilspycmd 反编译查看 C# 逻辑
4. dump.cs 可直接阅读，无需反编译

**典型流程（Unity Mono 游戏）：**
1. 从 APK 中提取 `assets/bin/Data/Managed/Assembly-CSharp.dll`
2. 直接 `ilspycmd Assembly-CSharp.dll -o 03_decompiled/游戏名/ilspy/`
3. 阅读反编译的 C# 源码，定位游戏逻辑

---

## 二-b、本机 / 真机坑位（命令级，2026-10-10 起）

> 来源：邂逅在迷宫会话复盘 P4（`10_docs/_state/规范优化_20261010_邂逅在迷宫会话复盘.md`）。
> 规则：**踩到即落一行** —— 命令级坑（引号/路径/权限/工具行为）当轮补进本表，
> 不要只在对话里说一遍。开工时 `bash 04_scripts/_common/session_start.sh <游戏>` 会回读本表。

| # | 场景 | 坑（错法） | 正解 |
|---|---|---|---|
| 1 | adb + root 重定向 | `adb shell su -c "cmd" > local.txt` 在**非 root shell** 里执行重定向 ⇒ 文件空/权限错 | `adb exec-out su -c "cat /path" > local.txt` |
| 2 | KernelSU 引号 | `adb shell su -c 'am force-stop X'` 里再套引号被拆 | 统一 `adb shell "su -c '<cmd>'"`（外层双引号，内层单引号） |
| 3 | adb 安装设备侧文件 | `adb install -r -d /data/local/tmp/x.apk` ⇒ `failed to stat`（绝对路径被当**本地**路径） | `adb shell pm install -r -d /data/local/tmp/x.apk` |
| 4 | 换签包必须卸载 | `pm uninstall -k` 在部分机型不生效，改签包覆盖安装失败 | `pm uninstall <pkg>`（会清 `/sdcard/Android/data/<pkg>`，让游戏自建，别用 shell 预建） |
| 5 | 自杀式 pkill | `pkill -f xhzm_autobind.sh` 匹配到自身命令行 ⇒ 把自己杀掉 | `pkill -9 -f '[x]hzm_autobind'` |
| 6 | 开机固化脚本目录 | `/data/adb/service.d/` 不存在（KernelSU/不同机型差异） | 先 `ls /data/adb/`；多数只有 `/data/adb/post-fs-data.d/` |
| 7 | root 写 app 私有目录 | 写完 app 在 Enforcing 下读不到（SELinux 上下文不对） | `chcon u:object_r:app_data_file:s0:c251,c259,c512,c768 <file>`；`setenforce 0` 不足以解锁 |
| 8 | 悬浮窗权限授予 | shell(uid 2000) `appops set ... SYSTEM_ALERT_WINDOW allow` ⇒ `SecurityException: uid 2000 does not have MANAGE_APP_OPS_MODES` | 走 root：`adb shell "su -c 'appops set <pkg> SYSTEM_ALERT_WINDOW allow'"` |
| 9 | macOS 无 timeout | `timeout 30 cmd` 不存在 | `perl -e 'alarm 30; exec @ARGV' cmd` |
| 10 | bash 3.2 门禁 | `mapfile`/`readarray`/`declare -A`/`local -n`/`${v^^}`/`;;&`/`&>>`、以及 `$VAR` 紧跟中文（bash 把中文并进变量名）⇒ 整套脚本跑不起来/静默少做事 | 跑 `bash 04_scripts/_common/preflight.sh <file>`；`$VAR` 一律写 `${VAR}`；有自动修法 `python3 04_scripts/_common/fix_bash32_var_brace.py --apply <file>` |
| 11 | frida CLI 17.16.4 | 遇 stdin EOF **立刻退出**，且无 `--no-pause`/`-o`，spawn 需 Gadget 不可用 | `sleep 120 \| frida -U -n <name> -l hook.js`（attach 可用；`tail -f /dev/null` 会让管道永不结束） |
| 12 | 启动 Activity | `am start -n <pkg>/<错类名>` ⇒ `Error type 3` | 用真实组件名（`aapt2 dump badging` 的 `launchable-activity`），或 `monkey -p <pkg> -c android.intent.category.LAUNCHER 1` |
| 13 | pipefail 假失败 | `unzip -p a.apk f \| grep -q x` 在 `set -o pipefail` 下因 SIGPIPE 判失败 | 先落临时文件再 grep，或 `\|\| true` 局部豁免 |
| 14 | logcat 选项 | `logcat -a` 在部分设备/本机不支持（`Unknown option '(null)'`） | 用 `-T 1 -v threadtime` 起录 |
| 15 | 二进制串计数 | `grep -c` 数的是**行**，不是出现次数；.NET 的 `#US` 字符串是 **UTF-16LE**，UTF-8 grep 假阴性 | `grep -a -o -F '<串>' \| wc -l`；必要时转码/按 UTF-16 搜 |
| 16 | apksigner 断言字节串 | 写 `d2800bc8` ⇒ `AssertionError: unexpected bytes` | 必须是**小端字节序**（`c80b80d2`） |
| 17 | tcpdump 抓包偏移 | `-i any` 的 pcap 是 **LINUX_SLL2**（link-type 276） | IP 头偏移按 **20**（不是 14）算 |
| 18 | toybox nsenter | `nsenter -t <pid> -m ls -la` ⇒ `Unknown option`（不支持组合选项） | 一次只进一个 ns：`nsenter -t <pid> -m md5sum /path` |
| 19 | curl 自测格式化 | 本地 shell 吃掉 `-w '%{http_code}'` 的引号 ⇒ 报 `code=000`（把格式串当 URL） | 在设备侧整体加引号：`adb shell "curl -s -o /dev/null -w 'code=%{http_code}' http://…"` |
| 20 | 多 VPN/多路由表 | 只看 `ip route` 会误判（策略路由在 table 1026 / 1000000026 等） | `ip route show table all \| grep tun0`；`dumpsys connectivity` 看 VPN 网络状态 |
| 21 | md5 取法跨机 | `md5sum` 在 macOS 不存在（反之 Linux 无 `md5`） | `md5 -q f 2>/dev/null \|\| md5sum f \| cut -d' ' -f1` |
| 22 | dumpsys 服务状态 | 只看 `ps` 判断前台服务是否活着会漏（进程内服务无独立 pid） | `dumpsys activity services <pkg>` 看 `isForeground=true`；VPN 看 `dumpsys connectivity` 的 `VPN CONNECTED` |
| 23 | 运行期解密 metadata 定位 | Unity IL2CPP 游戏在内存里**搜不到**标准 magic `af1bb1fa`（首 4 字节 sanity 被清零），全可读映射扫完 0 命中 | 按「可读段尺寸落在 `<包内 global-metadata.dat 大小>`…`+65536` 窗口 + 首部 `version∈[16..32]`」定位 ashmem/memfd 段（明日方舟 2.7.81＝`73e2531000` 段 44,175,360 B，version=24）；别只靠 magic 扫描 |
| 24 | Il2CppDumper 吃新版 metadata | `dotnet Il2CppDumper.dll libil2cpp.so magicfixed.dat out` ⇒ `System.Collections.Generic.KeyNotFoundException: 'kIl2CppMetadataUsageInvalid' was not present in the dictionary.`（`Metadata.cs:line 235` ← `:121`），输出目录为空 | 新版 metadata（v24+）先做**按名解析核验**（自研解析器 / `Cpp2IL`），别在 Dumper 上反复试；判据只取「类/方法名 + argc 是否命中」 |
| 25 | bash 3.2 空数组展开 | `set -u` 下 `"${ARR[@]}"` 当 `ARR=()` ⇒ `unbound variable`（macOS 自带 bash 3.2；Linux bash 4+ 不复现，容易漏） | 写 `${ARR[@]+"${ARR[@]}"}`；可选参数数组一律用这个形态 |
| 26 | 共享入口引用新 helper 符号 | 共享 `MainModern.cpp` 的 `JNI_OnLoad` 引用了某 helper（如 `SignalQuarantine_Start`/`FgoBoot_InjectEarlyFromOnLoad`）而该游戏 mk 未收录其源 ⇒ `ld.lld: error: undefined symbol` | 在游戏 mk 加 `LOCAL_SRC_FILES += Games/NoAceStubs.cpp`（**B 类 no-op 桩**，不改变交付形态）；**不要**顺手拉 A 类实现（会引入早期注入，偏离既有已验证形态） |
| 27 | 脚本依赖 ripgrep | 脚本里写 `rg ...`，本机 `command -v rg` 为空 ⇒ 静默返回空串，抽取/过滤结果全空（不报错，最难查） | 逆向脚本统一用 `grep -E`（BSD/GNU 都有）；新增脚本禁止引用 `rg`/`fd` 等未验证工具 |
| 28 | Il2CppTypeDefinition 字段偏移 | 照抄 Il2CppDumper 声明顺序读 `+0x28` 当 `methodStart` ⇒ 大量 -1/垃圾区间，误判「本版不能用」 | 该结构实为 `td_size=0x5C`：`0x20 fieldStart / 0x24 methodStart / 0x28 eventStart / 0x2c propertyStart / 0x30 nestedTypesStart`；用「`method_count` == 按 `declaringType` 分组条数」判据反推偏移（明日方舟 2.7.81＝43,341/43,341） |
| 29 | 二进制文件里搜**多字节（中文）串** | 默认 UTF-8 locale 的 BSD grep 对 `.so`/`.dex` 搜中文串会**假阴性**：`grep -a -c "十倍伤害" libstarry_magic.so` ⇒ `0`（实测该串确实存在 1 处）；`LC_ALL=C grep -a -c` ⇒ `1`。同文件里 ASCII 串（`MenuService`/`StarryMagic`）两种 locale 结果一致 ⇒ **只坑多字节串**，且不报错，最容易误判「菜单项丢了/门禁干净」 | 二进制内找中文串一律 `LC_ALL=C grep -a -o -F ...`（或直接用 python `blob.count(s.encode())`）；门禁类脚本的禁用串清单尽量只放 ASCII，含中文必须复核 |
| 30 | GitHub SSH 出网被拦时的 `git push` | `git push` ⇒ `fatal: Could not read from remote repository. Please make sure you have the correct access rights`；`ssh -T -o ConnectTimeout=8 git@github.com` 与 `ssh -p 443 git@ssh.github.com` **双双 Operation timed out**，而普通 HTTPS 出网正常（`curl -s -o /dev/null -w '%{http_code}' -m10 https://www.baidu.com` ⇒ `200`）；`~/.ssh/config` 里的 github 段无异常 | 判定＝「网络能通、GitHub 22/443 被拦」⇒ **只本地 `git commit`，push 等网络恢复再做**，不要反复重试（每次都会挂满超时，还占轮次）；`git status -sb` 的 `ahead N` 就是待推队列 |
| 31 | 判断「by-name Hook 的 IL2CPP API 面是否随游戏版本变化」 | 星光类 MOD 的 `libstarry_magic.so` 对 `il2cpp_*` **零静态导入**（`llvm-nm -D --undefined-only` 110 个未定义符号全是 libc/libc++）⇒ **不能**用导入表判断 API 依赖。正确做法：①扫 so 内 `.rodata` 的 `il2cpp_*` 字符串（本轮 33 个）；②`llvm-nm -D --defined-only <目标版 libil2cpp.so>` 取导出表（2.7.81 有 2,363 个）；③逐条比对。实测 `llvm-nm` 的 `unable to read symbol versions: SHT_GNU_versym …` 警告**无害**（导出表照读，0.04s 出结果） | 出包/装机前用它把「版本升级是否动了 API」一次判死；再用 md5 三方比对（`key_files` 抽出的 / 官方底包内 / 我签的包内，本轮同为 `c591bcb97af82a105ace637b1c4fb694`）证明「核对的正是运行时加载的那份」 |
| 32 | grep 搜「带方括号的标签串」（本仓日志一律带 `[tag]` 前缀） | BRE 里 `[mrfzz]` 是**字符类**（等价于「m/r/f/z 之一」），`grep -c "[mrfzz] hooked x" <log>` 恒为 0 ⇒ 判读脚本「七项全未装上」，而同一日志用 `grep -a "\[mrfzz\] hooked "` 明明能打出行，**假阴性且自洽**（最难查） | 搜字面量一律 `grep -aF -- '[mrfzz] hooked ' file`（`-F` 固定串，方括号字面匹配）；或转义 `\[mrfzz\]`。判读类脚本用 `-F` 最稳 |
| 33 | 「点开关卡 1~6 秒、最后游戏被杀」的真凶定位 | 症状像「菜单卡」，实际是**游戏主线程被 native 安装阻塞**：UI 线程 `nativeSetFeature` → `Game_OnFeatureChanged` → `SyncHooks()`（`std::lock_guard g_installMutex` + Dobby 安装），worker tick（`MainModern.cpp:248` 默认 `tickIntervalMs=200000`μs）抢同一把锁并在安装期间持锁 ⇒ 每次拨开关 UI 阻塞数秒，最后 `E ActivityManager: ANR in … Reason: Input dispatching timed out (… Waited 5000ms for MotionEvent)` → `Killing <pid> … user request after error` → `Process exited due to signal 9`；**没有 tombstone、没有 SIGSEGV** ⇒ 「闪退」不一定是 native crash，先搜 `grep -a 'ANR in\|Input dispatching timed out' logcat` | 判据：**开关回调（UI 线程）只置位，安装一律交 worker**；对照 `Java Toggle:` 与 native `[开关]` 两条时间戳的差值（正常 <100 ms，阻塞时 1~6 s）。修复点＝`mod菜单/starry-mod-menu/app/src/main/jni/Games/ArknightsMod.cpp` 的 `Arknights_OnFeatureChanged` |
| 34 | Hook 安装器里带「尝试 N 次就永久放弃」计数器 | 形如 `if (++g_xAttempts > 40) { g_xDone.store(true); LOGE(" install give up"); return; }`：worker tick 每 200 ms 一次 ⇒ 约 8 秒耗尽 40 次；而 IL2CPP 游戏**进战斗前那些类还没加载/domain 还没就绪** ⇒ 开关被持久化恢复成 ON 的场合，全部功能在用户进战斗**之前**就永久放弃，日志常只留一行 `install give up` | 「类还没加载」不是终态：改成**永不放弃** —— 首次失败打一条明确原因（`class X not resolvable yet (will retry)`），之后每 40 次降频 `LOGW`，类加载成功后自然装上 |
| 35 | by-name Hook 全链静默失败（零 hooked、零 miss、零报错） | `HookMethod(klass,…)` 开头 `if (!klass) return false;` **不打日志**；`FindClassNs` 在 `api.Assemblies()` 返回 0 时也静默 null ⇒ 现象是「一个功能都没装上，日志里连一条失败原因都没有」，只能翻源码猜 | 每个安装器自己 guard 并记录**首次**失败原因；再补一条**周期性状态行**（如每 10 s `[mrfzz] status hooked=%d/7 assemblies=%zu entity=%p apiReady=%d`）：`assemblies=0` ⇒ domain 未就绪；`entity=0` ⇒ 类名/命名空间变了；一次实测即可判死 |
| 36 | 合法重编后管线 `verify` 硬失败「产物指纹变化」 | 改了源码重跑 `pipeline.sh <游戏> run build_so` ⇒ 命令成功、产物与标签都对，但 verify 拿 `stage.artifacts` 里**上一轮的 md5** 比对 ⇒ `⚠ 产物指纹变化 … ❌ 阶段 build_so 产物校验失败`（设计如此，防「改源码没重编」） | 正确流程：先 `bash 04_scripts/pipeline.sh <游戏> reset <stage>`（清 artifacts/inputs）再 `run`；上游一 reset，下游 done 阶段会自动回退 pending（回显 `⚠ 自动回退 [surgical] → pending`），随后 `next --loop N` 连跑 |
| 37 | 共享层 `Il2CppApi::ImageOf()` 对某个游戏**所有**程序集静默返回 nullptr ⇒ by-name 全链判死 | `Api::ImageOf`（`Unity/Il2CppApi.cpp:992-1020`）在打自己的 `[Il2Cpp] ImageOf asm=…` 日志**之前**有三道静默守卫：`!IsValidHeapPtr(assembly)` → `!detail::IsReadableRange((uintptr_t)assembly,8)` → `SafeProbe{ img = *(void**)assembly; }`，任一挡下都**只 return 不报错**。判定手法：so 里 `LC_ALL=C grep -acF '[Il2Cpp] ImageOf asm='` = 1（串在），而 logcat 里 `ImageOf` 出现 **0 次** ⇒ 就是被守卫挡下的，不要再怀疑类名/命名空间。先排除 MTE/TBI 误判（`grep -c '^b4' /proc/<pid>/maps`＝0、`auxv` AT_HWCAP2 bit18 未置、无 `TaggedAddrMask`） | 在**游戏模块内**做三级兜底（不动共享层，免得影响别的游戏）：① `api.ImageOf()`；② `Il2Cpp::IsReadableRange(starptr::Untag(raw),8)` 守卫下自己 `memcpy` 读 `Il2CppAssembly` 首字段当 `Il2CppImage*`；③ 可信指针才用 `api.assembly_image(asm)`；再补一趟按名兜底 `api.domain_assembly_open(api.Domain(),"<主程序集名>")`（名字从离线 metadata images 表拿，见下条）。`ImageOfRobust()` 计数日志必须**打在状态行提前 return 之前**，否则功能一旦全部装齐就永远看不到诊断 |
| 38 | 想知道某个 `Torappu.Battle.X` 属于哪个程序集（按名兜底要用） | 直接读运行时 metadata 头部：`0xA0/0xA4`＝`Il2CppTypeDefinition` 表 offset/count（元素 0x5C，可自校验：count×0x5C 是否等于下一段 offset）；`0xA8/0xAC` 起的表＝ **images 表**，首条即 `Assembly-CSharp.dll`，元素 0x28 ＝ `nameIndex, assemblyIndex, typeStart, typeCount`；**type idx ∈ [typeStart, typeStart+typeCount) ⇒ 该类型属此程序集**（明日方舟 2.7.81 实测：`Assembly-CSharp.dll` `typeStart=0,typeCount=38093`，而 `Torappu.Battle.Entity` 的 type idx 是 10202 ⇒ 属主程序集） | 别猜程序集名、也别枚举 3520 条 images 全打：只算目标 type idx 落在哪个区间即可；按名兜底的候选串同时备 `X` 与 `X.dll` 两种写法（运行时 `Il2CppImage` 名带 `.dll`，`domain_assembly_open` 常吃不带后缀的） |

---

## 三、工作区目录结构规范

```
nixiang/
├── TOOLBOOK.md          ← 本文件（工具文档，按需查阅）
├── WORKFLOW.md          ← 工作流程规范（按需查阅）
├── 01_tools/            ← 手动下载的工具
├── 02_apks/             ← 原始 APK
├── 03_decompiled/       ← 反编译输出（jadx/apktool）
├── 04_scripts/          ← 通用逆向脚本（Frida/Python/Shell）
├── 05_ida/              ← IDA Pro 工程文件（.i64）
├── 06_dumped/           ← 脱壳/解密产物（dex/so/key）
├── 07_patched/          ← 修改后的文件（待重打包）
├── 08_output/           ← 成品 APK（签名后可安装）
├── 09_test/             ← 测试文件（日志/截图/抓包）
├── 10_docs/             ← 学习笔记与分析文档（单游戏结论；整仓流程写 WORKFLOW/TOOLBOOK）
├── 11_proj_tools/      ← 项目工具链（按游戏名分子目录，通过接口构建的自动化工具）
│   └── 动物餐厅/        ← 含 Frida hook、抓包分析、云存档改档、JSC 解密等完整工具链
├── 12_images/           ← 宣传图（按游戏名分子目录）
├── mod菜单/             ← 星光菜单工程（starry-mod-menu）；构建路径见 WORKFLOW §4
└── 99_archive/          ← 归档（已完成项目）
```

**星光菜单执行路径（摘要）：** 只改 native → `*_so_only.sh`；改菜单/smali/正式包 → 外科组装；历史 `inject_*.sh` 先核对实现与授权。细则与检查点在 **`WORKFLOW.md` §4**。切游戏：**只改** `Android.mk` 的 `STARRY_GAME_*`（`SWITCH_GAME.md`；规则 `starry-mod-game-id.mdc`）；`game_id` 由 `GameConfig.h` 映射。

**MT 管理器 APK MCP（摘要）：** 手机 MT 启动 APK MCP → 电脑 `adb forward tcp:8787 tcp:8787` → Cursor 启用 `MT_APK_MCP`（`.cursor/mcp.json`）。静态 Smali/资源/Dex xref/so 反汇编·CFG·试 patch·重打包优先走 MCP；运行时仍 Frida/星光。详备 `10_docs/_templates/MT管理器_MCP配合.md`；全局/仓内规则 `mt-apk-mcp.mdc`。

**IDA Pro MCP（摘要，用的时候再开）：** `ida-pro-mcp 2.0.0` 已装（pip → `/opt/homebrew/lib/python3.11/site-packages/ida_pro_mcp/`；Codex 配置 `~/.codex/config.toml` → `http://127.0.0.1:13337/mcp`；IDA 插件 symlink `~/Library/Application Support/Hex-Rays/IDA Pro/plugins/ida_mcp.py`）。启动：IDA Pro 9.3 打开目标 `.so`/`.i64` → **Edit → Plugins → MCP**（macOS `Ctrl+Option+M`）→ 监听 `13337`。服务端跑在 IDA 内部，不随 IDA 启动，**用的时候再开**。

> **AI 直连 MCP（2026-08-15 验证）：** 当时工作区 `run_mcp` 工具只挂 `integrated_code_mode`/`integrated_goal`；MT/IDA 两个 MCP 均为 HTTP 型，当时可由客户端 `curl` 访问 `http://127.0.0.1:8787/mcp`（MT）与 `http://127.0.0.1:13337/mcp`（IDA）；当前应优先使用会话实际暴露且获授权的 MCP 工具，历史端点不证明连接可用。连接前提：MT 需手机开 APK MCP + `adb forward tcp:8787 tcp:8787`；IDA 需先起 IDA 内 MCP 插件。

### ⚠️ 强制规则：按游戏名建子目录

**所有分类目录下，必须按游戏名创建子目录存放内容，禁止将文件直接堆在分类目录根下。**

示例（以「动物餐厅」为例）：
```
02_apks/
  └── 动物餐厅/
      └── 动物餐厅_12.35.apk

03_decompiled/
  └── 动物餐厅/
      ├── jadx_12.35/
      └── apktool_12.35/

04_scripts/
  └── 动物餐厅/
      ├── hook_xxtea.js
      └── decrypt_cocos.py

05_ida/
  └── 动物餐厅/
      └── libcocos2dcpp.so.i64

06_dumped/
  └── 动物餐厅/
      └── libxxx.so

07_patched/
  └── 动物餐厅/
      └── smali/

08_output/
  └── 动物餐厅/
      └── 动物餐厅_12.35_modified.apk

09_test/
  └── 动物餐厅/
      └── 20260805/

10_docs/
  └── 动物餐厅/
      └── 分析笔记_20260805.md

12_images/
  └── 动物餐厅/
      └── 动物餐厅_宣传图.png
```

### 正式交付目录（2026-09-29 固定）

收尾口令执行后，`08_output/{游戏}` 顶层只允许唯一正式 APK；图标和后台菜单模板放入同一日期交付子目录，后续会话不需要在多个目录搜索：

```text
08_output/{游戏}/
  唯一正式.apk
  delivery_{YYYYMMDD}/
    icon_{游戏}_512.png
    menu_template.json
```

- `menu_template.json` 是 `10_docs/{游戏}/{game_id}_menu_template.json` 的原样副本，不另改字段。
- 图标源优先从官方 APK 的最高密度资源取，生成 512×512 PNG；不改 `02_apks` 原包。
- 中间 APK、`.idsig`、未签名包不得留在 `08_output/{游戏}` 顶层。

**AI 执行规则：**
- AI 在任何分类目录下创建文件时，**必须先创建 `游戏名/` 子目录**
- 如果游戏名子目录不存在，AI 必须自动创建
- 禁止将游戏相关文件直接放在 `02_apks/`、`03_decompiled/` 等根目录下
- 例外：
  - `01_tools/`（工具不按游戏分类）
  - `99_archive/`（按授权命名归档；小文档备份可用任务时间目录/原路径）
  - `04_scripts/_common/`（跨游戏通用脚本）
  - `10_docs/_templates/`（笔记模板）
  - `10_docs/_state/`（跨游戏状态索引，见 AGENTS.md）

### 11_proj_tools/ 目录专项说明（项目工具链）

`11_proj_tools/` 与 `04_scripts/` 的区别：

| 对比项 | `04_scripts/` 通用逆向脚本 | `11_proj_tools/` 项目工具链 |
|--------|--------------------------|----------------------------|
| 定位 | 通用、可复用于不同游戏的脚本 | 针对具体游戏通过接口构建的自动化工具集 |
| 内容 | 单个 Frida .js、解密 .py 等 | 完整工具链：hook + 抓包 + 分析 + 改档 + 测试 |
| 命名 | `游戏名/功能_目标.js` | `游戏名/工具名/`（子目录组织） |
| 复杂度 | 单文件为主 | 多文件协作，可能含 node_modules、配置文件等 |

`11_proj_tools/游戏名/` 下常见子目录/文件：
- `cloud_save_tool/` — 云存档改档工具（Web UI + API）
- `cocos_jsc_decrypt/` — Cocos JSC 解密工具
- `hook_xxx.js` — Frida hook 脚本
- `test_xxx.py` — 自动化测试用例
- `probe_xxx.js` — 探测脚本
- `forge_xxx.py` — 请求伪造/验证脚本
- `audit_xxx.py` — 审计/状态监控脚本

**AI 规则：**
- 当 AI 通过接口为某个游戏构建自动化工具时，工具文件放入 `11_proj_tools/游戏名/`
- `11_proj_tools/游戏名/` 下按工具类型建子目录（如 `cloud_save_tool/`、`cocos_jsc_decrypt/`）
- 单个脚本可直接放 `11_proj_tools/游戏名/` 下，复杂工具建子目录

### 目录使用规则

| 目录 | 放什么 | 不放什么 | 命名规范 |
|------|--------|----------|----------|
| 01_tools | 手动下载的 jar/exe/bin 工具 | brew/pip 装的工具 | 保持原始包名（不按游戏分） |
| 02_apks | 原始未修改的 APK | 反编译后的文件 | `游戏名/游戏名_版本号.apk` |
| 03_decompiled | jadx/apktool 输出 | 原始 APK | `游戏名/jadx_版本/`、`游戏名/apktool_版本/` |
| 04_scripts | Frida .js、Python .py、Shell .sh | 工具自带的脚本 | `游戏名/功能_目标.js` |
| 05_ida | .i64 工程文件 | 原始 .so（放 06_dumped） | `游戏名/libxxx.so.i64` |
| 06_dumped | dump 出的 dex/so、提取的 key | 分析脚本（放 04_scripts） | `游戏名/` 子目录 |
| 07_patched | 改过的 smali/资源 | 未修改的文件 | `游戏名/` 子目录 |
| 08_output | 签名后可安装的 APK | 未签名的中间产物 | `游戏名/游戏名_版本_modified.apk` |
| 09_test | 日志、截图、抓包记录 | 源码文件 | `游戏名/日期/` 子目录 |
| 10_docs | 笔记、分析报告、学习资料 | 临时文件、宣传图（放 12_images） | `游戏名/主题_日期.md` |
| 11_proj_tools | **项目工具链**：通过接口构建的针对具体游戏的自动化工具集（Frida hook 脚本、抓包分析、云存档改档、解密工具、测试用例等） | 通用逆向脚本（放 04_scripts） | `游戏名/` 子目录，内含多个工具子目录 |
| 12_images | 游戏宣传图（1:1 等） | 分析截图（放 09_test）、文档配图以外的笔记 | `游戏名/游戏名_宣传图.png` |
| 99_archive | 用户授权的归档或小文档备份 | 默认大目录副本或未授权秘密备份 | 项目归档按游戏/版本/日期；文档备份按任务/时间/原路径 |

### 清理规则（保留输入、验证再生、具体授权）

政策唯一入口为 [AGENTS.md](AGENTS.md)；本节集中操作细则，填写 [游戏收尾与最小复现清单](10_docs/_templates/游戏收尾与最小复现清单.md)。本次规范授权不是删除授权，空白与未验证不算通过。

本游戏滚动物清理优先用 `bash 04_scripts/_common/trash_paths.sh --tag <游戏>_<日期> --path <显式路径> ...`：默认 dry-run，`--apply` 才移动到 `~/.Trash/nixiang_*` 下的隔离目录并写 `manifest.json`；脚本硬拒绝 `01_tools`、`02_apks`、`08_output` 和仓库根（`01_tools` 仅有下方「可再生输出」白名单例外）。只有专用隔离目录另获清理授权后才考虑永久删除。

**`01_tools/` 可再生输出（2026-09-30 定，允许清理）**

`01_tools/` 整体仍不入库、不整目录清理；但工具自身的**构建产物**与**运行输出**属于可再生，允许用 `trash_paths.sh` 清理（脚本按显式白名单 `TOOLS_REGEN_ALLOW` 放行，禁止通配）：

- 构建产物：工具源码目录下的 `bin/`、`obj/`（如 `01_tools/Il2CppDumper/Il2CppDumper/{bin,obj}`）——重新构建即得。
- 运行输出：工具在自身目录内落下的 dump 产物（如 `01_tools/Il2CppDumper/{dump.cs,script.json,il2cpp.h,stringliteral.json,DummyDll}`）——由 `02_apks` 原包 + 工具重跑再生。
- 上游素材：工具源码 clone 内未被本地改动的测试数据（如 `01_tools/Cpp2IL_src/TestFiles`）——`git -C 01_tools/Cpp2IL_src checkout -- TestFiles` 即恢复。
- 其余 `01_tools/` 内容（工具二进制、脚本、签名密钥、`.git`）一律保留，不清理。
新增白名单条目须显式写入 `trash_paths.sh` 的 `TOOLS_REGEN_ALLOW`，不接受通配匹配。

**Git 资产分层（2026-09-09 对接 GitHub 私仓 `Aliang369/nixiang` 起）**

- 分层基线：**Git 资产**（源码/文档/脚本/模板/配置/`06_dumped` 文本白名单，按 `.gitignore`）已推远端=已保全，本地副本可清理可再clone；**本地滚动物**（`03_decompiled`/`07_patched` 大包/`06_dumped` 大二进制/完整 `pipeline.log`/`99_archive`）按游戏活跃度滚动清理（活跃=复测进行中，见 `10_docs/_state/current.md` 的目标摘要）；**本地工具**（`01_tools`/`.kilo` 等）不清理不入库。
- `04_scripts/{游戏}/pipeline.log` 是可再生日志：物理文件可保留用于故障排查，
  但不再入 Git。需要长期证据时，在 `10_docs/{游戏}/evidence_日期/README.md`
  记录决定性行、定位和最短复现命令，不提交整份日志。
- 复测两场景的恢复路径：同版本重给包=clone→管线 check_apk→dump（03 重建）→锚点从 git 文本/Mod 源码恢复→build_so→surgical→装机；换新版本=同上+version-update-relocate 重抓锚点。
- 清理滚动物铁律：**删除前 `git status` 必须干净**（git 追踪文件本地不可单删——要么走 commit 语义，要么恢复）；中文路径操作前 `git config core.quotepath false`（否则路径匹配失败会误删 git 文本，20260909 实锤）。
- 敏感文件（`*.jks`/`*.keystore`/`.env`）永不入库（.gitignore 硬排除，add 前验证）；`02_apks`/`08_output` 用户手动管理不入库；`.gitignore` 的例外规则变更=资产分类变更，须具体授权。
- 空间基线：git 资产 ~490M（含 Dobby/Keystone 静态库=clone 即构建）；本地滚动物只保留活跃游戏（20260912 收官清理后：`03` = 月圆之夜/城主别慌张，`06` ignored 大头 = 月圆之夜 ~890M + 弹力果冻 ~120M，`07` = 城主别慌张/无限升级/月圆之夜 + 收官小尾巴 ~10M 待后续收官动作顺手清），收官即清大二进制留文本。

**收官归档清单（游戏收官时执行；2026-09-12 用户定稿原则）**

判定原则：**凡能从游戏包重新提取、脚本重新生成的产物（反编译工程、dump、so、解密中间物、memdiff 二进制）= 滚动物，收官即清**；复现链 = 管线脚本 + 10_docs 文档 + git 内文本，三者永留。

| 对象 | 收官动作 | 依据 |
|---|---|---|
| `03_decompiled/{游戏}` | 删 | 管线 dump 阶段从原包重建（02_apks 原包在或渠道重下） |
| `06_dumped/{游戏}` ignored 大二进制 | `git clean -xdn` dry-run 核对后 `git clean -xdf` | 可再生（重跑 dump） |
| `06_dumped/{游戏}` 锚点/文本摘录 | 留 | 只留锚点、脚本、小证据与必要输入；`dump.cs`/`script.json`/`stringliteral.json`/`il2cpp.h`/`DummyDll`/反编译/解密/pretty 产物不入库，按原包 + 工具重跑。 |
| `07_patched/{游戏}` | 删 | surgical 复现 = 重跑管线；删后须按 `stages.md` 收官清单第 4 条同步修正 `{游戏}/pipeline.json` 快照，否则 `archive` 被上游陈旧度门禁拦截 |
| `09_test/{游戏}` | 删 | 可再生 |
| `08_output/{游戏}` | 收尾口令时裁剪为唯一正式 APK | 用户已定：收尾+正式包+清理时，用 `_common/prune_output_final.sh` 校验并保留指定正式包，删除其它候选 APK/idsig，被删清单入 10_docs；被删的 unsigned/idsig 属门禁依赖产物，须按 `stages.md` 收官清单第 4 条同步修正 `pipeline.json` 快照 |
| `10_docs`、`04_scripts`、mod 源码 | 永留 | 复现链本体，git 已保全 |

不可再生例外（**勿按"可提取"误清**）：设备只读拉取的官方底包 `base.apk`（装机覆盖后官方包拉不回，如 `09_test/*/base*.apk`、`07_patched/无限升级/installed_orig_*/base.apk`，外科组装的唯一输入）；用户手改且无源文件对照的产物；签名私钥/密钥；`02/08` 官包与正式包（用户手动管理）；未收官或暂停研究游戏的滚动物（按目标游戏状态摘要判活跃）。

**热更样本一律可再生、可删（2026-09-30 用户定，推翻旧例外）**：官方 CDN/热更目录的下发物（`targetfuntional*.dll`、`index.js`/`.jsc`、差分清单等）可随时重新下发或由 DIAG in-hook dump 重取；旧版被滚动覆盖不构成不可再生。补丁 dll（`ilpatch*` 产物）同样由源码 + 官方 dll 重放。清理时保留"当前构建链直接引用的那一份"即可，历史版本样本可删。

git 入库纪律（2026-09-12 实清后定，勿再犯）：过程日志（`dump_log.txt`/`maps.txt`）不入库；游戏资源提取物（语言 CSV/atlas/skel 等非分析产物）不入库；重试堆积目录（同分钟多份 dump_out）须确认互异且无引用后才决定去留。

**构建产物与生成清单不入库（2026-09-30 定）**：`mod菜单/**/app/src/**/{obj*,libs*,jniLibs}`、`mod菜单/**/{build,.gradle,.gradle-home}`、`04_scripts/**/{bin,obj}`、`06_dumped/**/{bin,obj}` 均由源码 + 工具重建；`apk_entries.txt`、`rva_map.pkl`、热更/拼接 JS 样本属于脚本生成物，不保留本地、不入库。仅当前构建链直接引用的 `targetfuntional_dll*.bin`、`Games/WxsjPatchedDll*.a`、`Games/WxsjVc1637PatchedDll.a` 和第三方预编译 Dobby/Keystone `.a` 例外。

**保留与候选按内容判断，不按目录整删**

- `02_apks/` 原包由用户手动管理，AI 不默认删除、整理、覆盖；普通清理排除整个 `02_apks/`。
- `08_output/` 默认按用户管理；当用户明确执行「收尾+正式包+清理+git」口令时，使用 `04_scripts/_common/prune_output_final.sh` 将该游戏目录裁剪为唯一正式 APK。脚本先校验保留包签名/哈希并生成被删清单，只处理 `*.apk`/`*.idsig` 类文件；单独出包、普通清理或文档任务不得推定此项授权。
- `10_docs/` 保留为复现入口，但不能替代源码、配置、规则、补丁、构建入口与自写脚本。保留 `04_scripts/`、`11_proj_tools/`、`mod菜单/` 等处的小源码和自写工具（包括一次性探针）；工程要保留这些内容，而非无条件保留所有缓存。目录名不是删除依据。
- 签名私钥、密码及必要后台状态须安全备份；普通文档只记安全引用、保管责任与备份验证状态，不读取秘密内容，不把明文写入文档、日志或普通归档。未完成安全保全，不清相关输入。
- 人工 IDA 注释/类型/重命名所在工程、无法再取得的输入（**限设备只读拉取的官方底包、用户手改且无源文件的产物**）、未结故障的日志/崩溃现场和对照样本，未被完整保全并证明替代有效前条件保留。可重建不等于可重新研究：重新抓点或重新获取在线资源不能冒充可再生；**但热更样本（官方下发 dll/js/差分清单）与补丁产物（`ilpatch*` 输出）明确属可再生，不在此列**。
- `03_decompiled/` 反编译输出、build cache、冗余中间包，先确认无手改、无唯一依赖；可能承载构建/复现依赖的对象须通过下述隔离重建才能删除。经逐项证实无依赖、无手改、无证据价值的独立垃圾（如失败下载残片）可凭独立性证据和具体授权处理，不要求整个游戏先重建成功；不能只因文件名像缓存就认定独立。`05_ida/`、`06_dumped/`、`07_patched/`、`09_test/` 同样逐文件核对，不整目录推定可删；保留文档、个人文件、宣传资料及仍需复现的证据。

**最小复现与隔离重建**

1. 保留清单列具体路径、hash（样本身份/关键输入必要时）、角色、生成入口、所需环境/工具版本及外部依赖；包含正常完整构建入口，不能只记依赖旧成品的 so-only 快捷路径。
2. `/tmp` 中唯一输入先移入保留位置（涉及移动/覆盖须符合授权），或提供可执行且验证过的生成方式；记录外部工具、热更来源、服务状态与不可离线取得的依赖。无法重新取得时保留现有输入。
3. 清理可能承载构建/复现依赖的对象前，至少完成一次隔离空工作目录重建，只使用清单内保留输入及已声明、可取得的工具/外部依赖；不得为了测试先删原目录，也不得隐式借用原工作树缓存、未列出的旧包或 `/tmp` 文件。记录空目录、输入来源、命令、环境、退出码、日志与结果路径。测试若会操作设备、覆盖文件或写后台，须单独满足相应授权，不能借复现测试扩大范围。
4. 重建失败或未验证，保留相关输入并记录缺口。build 不等于 runtime：G1 可测试、G2 运行验证、G3 可交付分别留证。APK hash 可因 ZIP/签名元数据变化而不同，不强求整包逐字节一致；按声明的比较标准核对关键条目、包结构、签名与目标版本，不能忽略实质差异或把构建成功冒充运行通过。

**备份与执行顺序**

1. 不默认把大目录复制到同一 SSD 的 `99_archive/` 当释放空间；同盘归档不是释放空间。小文档按任务授权逐文件备份即可。外置备份须用户授权目标介质/路径，复制后验证完整性与可读性；未配置目标不擅自上传，不把未验证备份当删除前提已满足。
2. 先只读盘点，向用户列出候选的具体绝对路径、实测大小、游戏/会话归属、可再生依据、保留前提和重建证据（独立垃圾可改列无需重建的独立性证据）；不运行宽泛删除命令。排除 02/08、源码/自写脚本/配置规则、密钥、跨游戏内容和符号链接跳出的目标；`.git` 不可整删。目录内混有保留项时细化到文件，不跟随符号链接扩大范围。
3. 取得对应清单的具体删除授权后才执行；已有具体授权覆盖的范围不重复询问，但「收尾/省空间/全部清理」不能代替未明确的路径与边界。执行前复核目标及前提未变化，新出现的文件不自动纳入授权。
4. 事后记录实际删除路径、成功/失败/跳过项，以及同一文件系统、同一口径的前后实测容量与时间。候选大小不等于实际释放量；快照、硬链接或并发写入会影响结果，无法测量则记未测，不臆算释放量。

### 真机残留清理（会话清理、环境恢复、深度修复分开）

> 历史教训（2026-08-11）：雷霆战机实验残留曾影响其他游戏并污染判断，见 `10_docs/雷霆战机/官方原包_ACE崩溃与过检候选_20260810.md` §13。历史全局清理方法不是当前默认操作。

实际设备会话结束、暂停、切换时先回收日志、dump、崩溃现场及恢复所需证据，再填写模板的设备变更表；仅连接 adb 或仅做文档任务不清设备。

| 职责 | 范围与前置 | 验证与未满足处理 |
|---|---|---|
| 会话清理 | 明确设备 serial，只处理本次创建的具体文件与进程；核对进程身份/归属，不按全局名称批杀 | 按同一 serial 检查已授权目标，不要求所有 frida/ptrace 或全局列表为空；其他会话不影响 |
| 环境恢复 | 依据修改前快照，只撤销本次设置/文件变更；列出前值、后值、恢复目标及授权 | 缺快照、归属不明或存在后续并发修改时不猜默认值，保留现场并报告 |
| 深度修复 | 全局 KSU、设备标识恢复、安装目录 so 恢复、批量 dump 删除分别列具体范围、影响及恢复方案，另取具体授权 | 不混入日常清理；缺必要备份或无法约束范围时不执行，记录未处理风险 |

**现有脚本限制（2026-09-05 只读核对，脚本未改）**

`04_scripts/_common/cleanup_device_re_residue.sh` 当前按全局名称杀进程、修改 KSU umount、按通配符删除 tmp 文件、移动官方 so 备份，并可能自动恢复设备标识；可选 `--deep` 批量删 dump、`--restore-so` 恢复 so。它没有本次文件/进程白名单、修改前快照或显式 serial 参数，不能声称已支持上述协议。

`--dry-run` 不是完整预演：进程展示与实际杀进程集合不完全一致，未完整展示额外 APK 删除、备份移动和 deep 删除，仍连接设备并读取/输出标识等信息。不能用 dry-run 输出作为完整授权清单，本轮也不运行它。

因此不默认运行该脚本（包括 dry-run），不以其旧注释的“必跑”作为政策。只有核对当前实现且全部实际影响已有具体授权、能够约束目标时才可考虑调用；范围无法约束则禁止默认跑，报告本轮残留、缺失前置和待处理项，不称“已清干净”。

保留游戏存档、个人文件及设备标识种子，不卸载 SukiSU/Root。标识和 so 恢复属于单独评估，不自动触发 `ensure_starry_device_id_seed.sh`、覆盖重装或重启；历史资料见 `10_docs/_templates/全游戏卡密_复现路径与防再发.md`。最终按具体设备/本次变更记录实际动作与验证结果，而非全设备“应为空”。

---

## 三-b、通用脚本与运行环境（2026-08-08）

### `04_scripts/_common/`

| 脚本 | 用途 |
|------|------|
| `env.sh` | 解析 `NIXIANG_ROOT`、python3.11、SDK build-tools、uber-apk-signer |
| `decode_apk.sh` | jadx + apktool → `03_decompiled/{游戏}/`，写拆包摘要 |
| `manifest_summary.sh` | 抽 package / 权限 / 组件 / launcher（输入须为 **apktool 解码后的** Manifest，不要用 `original/` 二进制） |
| `rebuild_sign_install.sh` | apktool b → uber 或 zipalign+apksigner → 可选 `adb install` |
| `cleanup_device_re_residue.sh` | 历史全局清理脚本；未改且不支持会话协议，不默认运行，见「真机残留清理」 |

笔记模板：`10_docs/_templates/分析笔记模板.md`  
唯一入口（所有工具统一）：`AGENTS.md`；`skill/SKILL.md` 为按需索引，不强制通读本书或 WORKFLOW。

### Python

- RE 库（pycryptodome / xxtea / yara / keystone）装在 **brew `python3.11`**
- 默认 `python3` 可能是 3.14且无这些库 → 脚本用 `/opt/homebrew/bin/python3.11` 或 `NIXIANG_PYTHON`
- `capstone`：文档曾列出，若 `import capstone` 失败可 `python3.11 -m pip install capstone`
- Frida CLI（`~/.local/bin/frida`）自带独立 Python，与 brew 3.11 分离，不影响 `frida -U`

### 进阶参考（未整包引入）

未把外部「全量逆向 skill 包」接进本仓库。若遇到 OLLVM 控制流平坦化、前端 JS 参数加密等，可按需查阅公开资料或临时 clone 参考，**授权与路由仍以 AGENTS.md 为准**。

## 四、按需使用与核验

读取、授权、协作及状态记录统一见 `AGENTS.md`，不另设启动协议。

- 只核验当前任务需要的工具：例如 Java 分析用 `command -v jadx` 与 `jadx --version`，其他工具同理；不全量扫描或默认安装缺失工具。
- 输入/输出按上方目录表，临时产物先确认无证据价值再按已授权范围清理。
- 实质性游戏执行发现记录来源、版本与验证状态到 `10_docs/{游戏名}/`；脚本放 `04_scripts/{游戏名}/`。纯评估不改变游戏状态。
- 收尾参见本书「清理规则」「真机残留清理」及 `AGENTS.md`，不因文档任务或检测到设备而自动清理；归档仅在用户明确要求时进行。
