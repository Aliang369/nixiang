# TOOLBOOK.md — 安卓逆向工具文档与使用规范

> 📅 最后更新：2026-08-08（_common 脚本、SDK 签名路径、Python3.11、Cursor skill）
> 📍 工作区：/Users/aliang/Documents/nixiang
> 🎯 用途：个人安卓游戏逆向学习
> 🤖 AI 读取指引：唯一入口 `AGENTS.md` → 按 `skill/SKILL.md` 总纲读取；层级 `skill/SKILL.md > PLAYBOOK > WORKFLOW > TOOLBOOK`。

## ⚡ 当前执行规范（2026-08-14 起，优先级高于旧流程）

1. **游戏 APK 禁止 `apktool b` 整包重建**：只对 `mod菜单/starry-mod-menu` 小模板用 apktool；游戏包走 zip 级外科组装。
2. **动态分析默认自注入 Trace**：受保护/ACE/Frida 闪退游戏先走 `libstarry_magic.so` + `StarryTrace`；Frida/Florida 只用于无壳、无强反作弊游戏的短探针。
3. 构建选择：
   - 只改 native → `*_so_only.sh`
   - 首包/菜单/smali/assets/Manifest → 外科组装（样板 `mod菜单/starry-mod-menu/scripts/surgical_pcr_11.7.2.sh`）
   - 快速试改 → MT MCP / `smali`+`baksmali`
4. **新游戏/新版本**先读 `PLAYBOOK.md`；日常执行看 `WORKFLOW.md`；硬规则看 `AGENTS.md` 铁律 + `skill/SKILL.md`（Cursor 用户另见 `.cursor/rules/apk-build-surgical.mdc`、`.cursor/rules/self-hosted-trace.mdc` 指针）。

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
| frida | **17.9.1**（对齐 Florida） | `pip install frida==17.9.1`；CLI 用 `$HOME/Library/Python/3.11/bin` | 动态 Hook 核心 |
| frida-tools | 随 17.9.1 | 同上 | Frida CLI |
| Florida（魔改 server） | 17.9.1 | `01_tools/florida/` + `start_florida_server.sh` | **默认**真机 server（反检测） |
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

```bash
# 解包
apktool d 02_apks/应用名_版本.apk -o 03_decompiled/应用名_版本_apktool/

# 重打包
apktool b 07_patched/应用名_版本/ -o 08_output/应用名_版本_unsigned.apk

# 重打包（不强制资源处理，快速模式）
apktool b -f 07_patched/应用名_版本/ -o 08_output/应用名_版本_unsigned.apk
```

### IDA Pro — Native 分析（⚠️ AI 必读：插件使用强制规则）

> **强制规则：** 当 AI 使用 IDA 分析 .so 文件时，**必须**按以下流程使用插件，不得跳过。
> 这些插件不是可选的辅助工具，而是标准分析流程的必经步骤。AI 如果引导用户分析 .so 但不提插件，视为流程缺失。

#### IDA 分析标准流程（6 步）

**Step 1 — 打开目标 .so**
```bash
# GUI 模式（推荐）
open -a "IDA Professional 9.3" 06_dumped/libxxx.so
# 或命令行
/Applications/IDA\ Professional\ 9.3.app/Contents/MacOS/idat -A -B 06_dumped/libxxx.so
```

**Step 2 — 【强制】FindCrypt：扫描加密算法特征**
- 路径：Edit → Plugins → FindCrypt（快捷键 `Ctrl+Alt+F`）
- **必做原因**：Cocos/Unity 游戏的加密解密函数（XXTEA/AES/RSA/MD5/SHA）不会明文标注函数名，FindCrypt 通过常量表自动定位
- 扫描后会列出所有匹配的加密算法及其地址，**必须将结果记录到 `10_docs/` 分析笔记中**
- 典型场景：找到 AES → 跟进该函数 → 可能是资源解密或网络通信加密

**Step 3 — 【条件强制】Il2CppDumper：Unity IL2CPP 符号还原**
- **触发条件**：如果目标是 Unity IL2CPP 游戏（存在 `libil2cpp.so` + `global-metadata.dat`），**必须执行**
- **不执行后果**：IDA 中 libil2cpp.so 全是 `sub_XXXX` 无意义函数名，无法定位游戏逻辑

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

#### IDA 插件速查卡（AI 分析 .so 时必须检查的清单）

| 步骤 | 插件/功能 | 快捷键 | 何时用 | AI 提醒义务 |
|------|-----------|--------|--------|-------------|
| 1 | 打开 .so | — | 总是 | — |
| 2 | FindCrypt | Ctrl+Alt+F | **总是** | AI 必须主动建议用户运行 FindCrypt |
| 3 | Il2CppDumper | 命令行+脚本 | 仅 Unity IL2CPP 游戏 | AI 检测到 libil2cpp.so 必须提示 |
| 4 | 字符串搜索 | Shift+F12 | **总是** | AI 必须建议搜索关键词 |
| 5 | F5 反编译 | F5 | 分析具体函数时 | — |
| 6 | Keypatch | Ctrl+Alt+K | 需要修改 .so 时 | AI 必须建议用 Keypatch 而非手动改字节 |

> **⚠️ AI 责任声明**：如果 AI 引导用户分析 .so 文件但未提及上述插件，视为分析流程不完整。AI 必须在给出分析建议时，明确提示用户执行对应插件步骤。

### Frida — 动态 Hook（默认 Florida 魔改）

> **约定**：真机优先 **Florida**（`01_tools/florida/`），不要默认官方 `frida-server`。  
> 规则：`skill/` 总纲 + `TOOLBOOK.md`「Frida — 动态 Hook」；Cursor 用户另见 `.cursor/rules/frida-florida.mdc` 指针。  
> 用户配合必须聊天置顶：`skill/SKILL.md` 铁律 8（Cursor 用户见 `.cursor/rules/user-coop-chat-first.mdc` 指针）。

```bash
# 启动魔改 server（推到 /data/local/tmp/fs-arm64，当前 17.9.1）
04_scripts/_common/start_florida_server.sh

# 客户端必须同版本；CLI 用 3.11 user path
export PATH="$HOME/Library/Python/3.11/bin:$PATH"
python3.11 -c "import frida; print(frida.__version__)"   # 期望 17.9.1

frida-ps -U
frida -U -n "应用包名" -l 04_scripts/hook.js
frida -U -f "应用包名" -l 04_scripts/hook.js --no-pause

# 官方 server 仅作对照/备用（01_tools/frida-server-*-android-arm64）
# objection -g "应用包名" explore
```

下载新档（代理示例）：`https_proxy=http://127.0.0.1:7897 gh release download <ver> -R Ylarod/Florida -p 'florida-server-*-android-arm64.gz'`。详见 `01_tools/florida/README.md`。

### adb — 设备控制

```bash
# 安装 APK
adb install 08_output/应用名_版本_modified.apk

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
一键重建签名安装：

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

## 三、工作区目录结构规范

```
nixiang/
├── TOOLBOOK.md          ← 本文件（工具文档，AI 必读）
├── WORKFLOW.md          ← 工作流程规范（AI 必读）
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
├── mod菜单/             ← 星光菜单工程（starry-mod-menu）；构建路径见 WORKFLOW 阶段 6b
└── 99_archive/          ← 归档（已完成项目）
```

**星光菜单执行路径（摘要）：** 只改 native → `*_so_only.sh`；改菜单/smali/正式包 → `inject_*.sh`。细则与检查点在 **`WORKFLOW.md` 阶段 6b**。切游戏：**只改** `Android.mk` 的 `STARRY_GAME_*`（`SWITCH_GAME.md`；规则 `starry-mod-game-id.mdc`）；`game_id` 由 `GameConfig.h` 映射。

**MT 管理器 APK MCP（摘要）：** 手机 MT 启动 APK MCP → 电脑 `adb forward tcp:8787 tcp:8787` → Cursor 启用 `MT_APK_MCP`（`.cursor/mcp.json`）。静态 Smali/资源/Dex xref/so 反汇编·CFG·试 patch·重打包优先走 MCP；运行时仍 Frida/星光。详备 `10_docs/_templates/MT管理器_MCP配合.md`；全局/仓内规则 `mt-apk-mcp.mdc`。

**IDA Pro MCP（摘要，用的时候再开）：** `ida-pro-mcp 2.0.0` 已装（pip → `/opt/homebrew/lib/python3.11/site-packages/ida_pro_mcp/`；Codex 配置 `~/.codex/config.toml` → `http://127.0.0.1:13337/mcp`；IDA 插件 symlink `~/Library/Application Support/Hex-Rays/IDA Pro/plugins/ida_mcp.py`）。启动：IDA Pro 9.3 打开目标 `.so`/`.i64` → **Edit → Plugins → MCP**（macOS `Ctrl+Option+M`）→ 监听 `13337`。服务端跑在 IDA 内部，不随 IDA 启动，**用的时候再开**。

> **AI 直连 MCP（2026-08-15 验证）：** 工作区 `run_mcp` 工具当前只挂 `integrated_code_mode`/`integrated_goal`；MT/IDA 两个 MCP 均为 HTTP 型，AI 直接 `curl` 打 `http://127.0.0.1:8787/mcp`（MT）与 `http://127.0.0.1:13337/mcp`（IDA）即可，等价于 MCP 客户端。连接前提：MT 需手机开 APK MCP + `adb forward tcp:8787 tcp:8787`；IDA 需先起 IDA 内 MCP 插件。

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

**AI 执行规则：**
- AI 在任何分类目录下创建文件时，**必须先创建 `游戏名/` 子目录**
- 如果游戏名子目录不存在，AI 必须自动创建
- 禁止将游戏相关文件直接放在 `02_apks/`、`03_decompiled/` 等根目录下
- 例外：
  - `01_tools/`（工具不按游戏分类）
  - `99_archive/`（按 `游戏名_版本_日期.zip` 归档）
  - `04_scripts/_common/`（跨游戏通用脚本）
  - `10_docs/_templates/`（笔记模板）

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
| 99_archive | 已完成项目的完整打包 | 进行中的项目 | `游戏名_版本_日期.zip`（不按游戏分子目录） |

### 清理规则（省磁盘：可复现则删，不默认打满盘归档）

原则：分析结论写在 `10_docs/` 即可复现；中间产物优先直接删除，**不必**为省心打大包进 `99_archive`（除非用户明确要求备份）。

**删前必须确认（不可擅自删）**
- `10_docs/`、`10_docs/_templates/`
- `11_proj_tools/`
- `12_images/`
- `mod菜单/`（含星光菜单工程与构建产物策略见下：工程本身勿删；`app/build` 等缓存可清）
- `01_tools/`
- `04_scripts/_common/`

**用户手动清理（AI 不要动）**
- `02_apks/{游戏}/` 原始 APK
- `08_output/{游戏}/` 里的**最终成品**（用户确认过的那一份可分发/可安装正式包，例如 `*_starry-releaseSigned.apk`；不是整个 `08_output` 目录）

**可直接清理（任务完成或用户说清理时，AI 可删）**
- `03_decompiled/`、`05_ida/`、`06_dumped/`、`07_patched/`、`09_test/`
- `04_scripts/{游戏名}/` 项目一次性探针脚本（**不含** `_common/`）
- `08_output` 内非最终成品：unsigned、aligned、`.idsig`、`.prev/`、debug/试装包、`from_device`、旧版本包等
- Gradle/`app/build`、设备 `/data/local/tmp/` 临时安装包等构建与临时缓存
- `.DS_Store`：`find /Users/aliang/Documents/nixiang -name .DS_Store -delete`

清理前先列出将删路径给用户过目（用户已明确「全部清理」且范围清晰时可直接执行）。

### 真机残留清理（强制：每次逆向会话结束）

> **教训（2026-08-11）**：雷霆战机 ACE 实验后，`/data/local/tmp` 的 frida/ptrace、KSU 全局 umount、应用目录改过的 `libtersafe*` 等残留，导致腾讯游戏异常，并污染「ACE 必杀」判断。清理后官方原包可玩。详见 `10_docs/雷霆战机/官方原包_ACE崩溃与过检候选_20260810.md` §13。

**AI / 人工在下列时机必须执行真机清理（有 adb 设备时）：**

1. 本轮 Frida / ptrace / 磁盘 so 补丁 / 试装 / 注入 **结束或暂停**  
2. 用户说「清理设备 / 逆向完了 / 能玩别的游戏了」  
3. 开始新的 ACE/过检实验 **之前**（先干净再动手）

```bash
# 标准（推荐每次结束都跑）
./04_scripts/_common/cleanup_device_re_residue.sh

# 只看将删什么
./04_scripts/_common/cleanup_device_re_residue.sh --dry-run

# 额外清 tmp 大 dump
./04_scripts/_common/cleanup_device_re_residue.sh --deep
```

脚本会：杀 frida/ltzj_ptrace 等注入进程；去掉试验性 KSU umount（如 `/data/adb`、`/debug_ramdisk`）；删除 `/data/local/tmp` 下 frida-server、ltzj_*、ace 试验 so、星光临时 apk 等；官方 so 备份挪到 `/sdcard/MT2/backup/re_cleanup_日期/`。

**不做**：卸载 SukiSU/Root、删游戏存档、改用户个人文件。

**自检（清理后应为空）：**

```bash
adb shell "ps -A | grep -iE 'frida|ptrace|ltzj' || true"
adb shell "su -c 'ls /data/local/tmp/frida-server /data/local/tmp/ltzj_* 2>/dev/null || echo clean'"
adb shell "su -c 'ksud umount list 2>/dev/null'"
```

若某游戏安装目录内 so 仍被改过：重装官方包，或 `PKG=包名 ./cleanup_device_re_residue.sh --restore-so`（需备份里有 `*_official_orig.so`）。

---

## 三-b、通用脚本与运行环境（2026-08-08）

### `04_scripts/_common/`

| 脚本 | 用途 |
|------|------|
| `env.sh` | 解析 `NIXIANG_ROOT`、python3.11、SDK build-tools、uber-apk-signer |
| `decode_apk.sh` | jadx + apktool → `03_decompiled/{游戏}/`，写拆包摘要 |
| `manifest_summary.sh` | 抽 package / 权限 / 组件 / launcher（输入须为 **apktool 解码后的** Manifest，不要用 `original/` 二进制） |
| `rebuild_sign_install.sh` | apktool b → uber 或 zipalign+apksigner → 可选 `adb install` |
| `cleanup_device_re_residue.sh` | **真机逆向残留一键清理**（Frida/ptrace/tmp 试验 so、KSU 试验 umount 等） |

笔记模板：`10_docs/_templates/分析笔记模板.md`  
薄入口（所有工具统一）：`AGENTS.md` · 薄 skill `skill/SKILL.md`（强制读本文件 + WORKFLOW，不替代二者）

### Python

- RE 库（pycryptodome / xxtea / yara / keystone）装在 **brew `python3.11`**
- 默认 `python3` 可能是 3.14且无这些库 → 脚本用 `/opt/homebrew/bin/python3.11` 或 `NIXIANG_PYTHON`
- `capstone`：文档曾列出，若 `import capstone` 失败可 `python3.11 -m pip install capstone`
- Frida CLI（`~/.local/bin/frida`）自带独立 Python，与 brew 3.11 分离，不影响 `frida -U`

### 进阶参考（未整包引入）

未把外部「全量逆向 skill 包」接进本仓库。若遇到 OLLVM 控制流平坦化、前端 JS 参数加密等，可按需查阅公开资料或临时 clone 参考，**仍以本目录规范与本文件为准**。

## 四、AI 使用流程

当 AI 被要求在逆向工作区执行任务时，按以下流程操作：

### Step 1: 读取工具状态
```
读取 TOOLBOOK.md（本文件）→ 了解已装/未装工具
读取 WORKFLOW.md → 了解当前任务对应的操作流程
可选：`skill/SKILL.md`（作业硬规则速查）
```

### Step 2: 检查工具可用性
```bash
# 执行任务前先确认工具已安装
which jadx && jadx --version
which apktool && apktool --version
which frida && frida --version
# 如果未安装，先提示用户安装，给出安装命令
```

### Step 3: 按目录规范操作
- 输入文件从对应目录读取（如 APK 从 `02_apks/游戏名/`）
- 输出文件写到对应目录（如反编译输出到 `03_decompiled/游戏名/`）
- **强制：必须在分类目录下先创建 `游戏名/` 子目录，再放文件**
- 临时文件用完即删，不要留在工作区
- 命名遵循上方表格规范

### Step 4: 记录操作
- 每次实质性操作后在 `10_docs/` 写分析笔记
- 关键发现（密钥、算法、入口函数）记录到笔记中
- 脚本保存到 `04_scripts/` 并加注释

### Step 5: 清理
- 按上方「清理规则」执行：可复现中间产物直接删；`02` 原包与 `08` **最终成品**留给用户手动；`10`/`11`/`mod菜单`/`01_tools`/`_common` 删前确认
- **有真机时必跑**：`./04_scripts/_common/cleanup_device_re_residue.sh`（见上文「真机残留清理」）
- 仅在用户明确要求时才打包装进 `99_archive/`
