# TOOLBOOK.md — 安卓逆向工具文档与使用规范

> 📅 最后更新：2026-08-08（_common 脚本、SDK 签名路径、Python3.11、Cursor skill）
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

**Git 资产分层（2026-09-09 对接 GitHub 私仓 `Aliang369/nixiang` 起）**

- 分层基线：**Git 资产**（源码/文档/脚本/模板/配置/`06_dumped` 文本白名单，按 `.gitignore`）已推远端=已保全，本地副本可清理可再clone；**本地滚动物**（`03_decompiled`/`07_patched` 大包/`06_dumped` 大二进制/`99_archive`）按游戏活跃度滚动清理（活跃=复测进行中，见 current.md）；**本地工具**（`01_tools`/`.kilo` 等）不清理不入库。
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
| `06_dumped/{游戏}` 文本（已入 git） | 留 | dump.cs/反编译/解密产物 = 版本档案，更新时 RVA 对照复用 |
| `07_patched/{游戏}` | 删 | surgical 复现 = 重跑管线；删后须按 `stages.md` 收官清单第 4 条同步修正 `{游戏}/pipeline.json` 快照，否则 `archive` 被上游陈旧度门禁拦截 |
| `09_test/{游戏}` | 删 | 可再生 |
| `08_output/{游戏}` | 收尾口令时裁剪为唯一正式 APK | 用户已定：收尾+正式包+清理时，用 `_common/prune_output_final.sh` 校验并保留指定正式包，删除其它候选 APK/idsig，被删清单入 10_docs；被删的 unsigned/idsig 属门禁依赖产物，须按 `stages.md` 收官清单第 4 条同步修正 `pipeline.json` 快照 |
| `10_docs`、`04_scripts`、mod 源码 | 永留 | 复现链本体，git 已保全 |

不可再生例外（**勿按"可提取"误清**）：热更历史版本 dll/bin（官方 CDN 滚动覆盖，旧版拉不回，如 `07_patched/无限升级/targetfuntional*` = OTA 复现链）；设备拉取的装机备份；`02/08` 官包与正式包（用户手动管理）；未收官或暂停研究游戏的滚动物（按 current.md 状态判活跃）。

git 入库纪律（2026-09-12 实清后定，勿再犯）：过程日志（`dump_log.txt`/`maps.txt`）不入库；游戏资源提取物（语言 CSV/atlas/skel 等非分析产物）不入库；重试堆积目录（同分钟多份 dump_out）须确认互异且无引用后才决定去留。

**保留与候选按内容判断，不按目录整删**

- `02_apks/` 原包由用户手动管理，AI 不默认删除、整理、覆盖；普通清理排除整个 `02_apks/`。
- `08_output/` 默认按用户管理；当用户明确执行「收尾+正式包+清理+git」口令时，使用 `04_scripts/_common/prune_output_final.sh` 将该游戏目录裁剪为唯一正式 APK。脚本先校验保留包签名/哈希并生成被删清单，只处理 `*.apk`/`*.idsig` 类文件；单独出包、普通清理或文档任务不得推定此项授权。
- `10_docs/` 保留为复现入口，但不能替代源码、配置、规则、补丁、构建入口与自写脚本。保留 `04_scripts/`、`11_proj_tools/`、`mod菜单/` 等处的小源码和自写工具（包括一次性探针）；工程要保留这些内容，而非无条件保留所有缓存。目录名不是删除依据。
- 签名私钥、密码及必要后台状态须安全备份；普通文档只记安全引用、保管责任与备份验证状态，不读取秘密内容，不把明文写入文档、日志或普通归档。未完成安全保全，不清相关输入。
- 人工 IDA 注释/类型/重命名所在工程、唯一 dump/解密/热更样本、无法再取得的输入、未结故障的日志/崩溃现场和对照样本，未被完整保全并证明替代有效前条件保留。可重建不等于可重新研究，重新抓点或重新获取在线资源不能冒充可再生。
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
