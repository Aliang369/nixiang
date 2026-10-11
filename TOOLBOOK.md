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
| 39 | 「到底是哪个静默守卫挡住了 `Api::ImageOf`」——靠读代码猜会猜反 | 本轮踩坑实录：先按源码推断「第一道 `IsValidHeapPtr`（内部 `PermCached` maps 快照，未命中**无**精确 `fopen` 兜底）在本进程恒假」，并据此改了效果路径判据；随后**标定**证明完全猜反——第一道健康，被挡的是第二道。正确手法＝在包内加**判据标定行**：① 取本 so 内一个「确定可读」的全局地址打印 `IsValidHeapPtr` / `Il2Cpp::IsReadableRange` / `starptr::LiveObj` 三个布尔（**锚点必须是 `void*` 等 8 字节对齐类型**：取 `std::atomic<int>` 的地址会得 `live=0`，从而误判「判据失效」——本轮就这样误报过一次）；② 对真程序集指针**分别**打印「原指针 vs `starptr::Untag()` 后」的可读判定。明日方舟 2.7.81 实测：`[mrfzz] calib: anchor=0x756ad10500 heap=1 read=1 live=1` + `diag: asm[0]=0xb4000073e0dbf000 valid=1 readRaw=0 readUntag=1` ⇒ `IsValidHeapPtr` **健康**（它内部先 `starptr::Untag`），挡下的是第二道 `detail::IsReadableRange((uintptr_t)assembly, 8)`：它走精确 `fopen("/proc/self/maps")` 且**不清顶字节标签**，而真机指针带 `0xb4` 标签（maps 里是 `0x73e0dbf000…`）⇒ 区间比对必然不中，于是静默 nullptr | 判据标定一律「确定可读的自身全局（8 字节对齐）+ 同一指针 `raw`/`untag` 双判」；诊断结论最好由包内自报（如 `calib: guard2 raw=0 untag=1 ⇒ Api::ImageOf 死于此守卫(未清标签)`），并把诊断行放在任何提前 return **之前**（否则功能一旦装齐就永远看不到）。共享层修法＝先 `starptr::Untag` 再判；游戏模块内绕过＝自读首字段（本仓 `Games/ArknightsMod.cpp` 的 `ImageOfRobust()` 路径②） |
| 40 | 「hook 明明装上了，判读脚本却说『未装上』」——判读口径两个坑（2026-10-10 明日方舟实战踩） | 坑①**键名不匹配**：安装日志打的是**方法名**（`[mrfzz] hooked get_atk argc=0 target=…`），只有命中日志打**功能键名**（`[mrfzz] hit damage10`）；判读脚本若按功能键名去 grep `hooked`，永远 0 ⇒ 10/10 全装也会显示「未装上（查下方 miss / give up）」。坑②**跨会话累计**：一份 `09_test/_recorder/{game}_*/logcat.txt` 覆盖多次装机/多个游戏进程，全量 grep 会把历次装机的行全算上（实测 `[mrfzz] hooked ` 全量 **30**＝三轮×10）⇒ 必须按 pid 切会话。修法（已落 `04_scripts/明日方舟/实测判读_mrfzz.sh` v2）：`LASTPID=$(awk '/\[mrfzz\]/{p=$3} END{print p}' logcat.txt)`（logcat 行格式 `MM-DD HH:MM:SS.mmm PID TID L TAG:` ⇒ pid 是 `$3`），grep 后再 `awk -v p=$LASTPID '$3==p'`；每功能一行映射表 `功能键名|标签|最少挂点数|方法名(可多个)`：`attack_speed→CalculateAttackInterval`、`invincible→get_isInvincible`、`base_lock→get_lifePointReduce`、`damage10→get_atk`、`deploy_anywhere→get_buildableType`、`infinite_sp→get_sp`、`infinite_cnt→get_availableCnt`、`infinite_usedup→get_isUsedUp`、`zero_cost_card→get_cost(≥1)`、`zero_cost_token→get_cost(≥2)`（两条 `get_cost` 分别是 `Deck.Card` 与 `Deck.TokenCard`）；并显式打印「本会话 hooked 行数」「hit 行数」两个总数 | 判读脚本一律「① 先按 pid 切会话 ② 挂点统计只认方法名 ③ 只认 hit 的键名」；结论行必须区分「装钩层 OK」与「效果层（hit/看画面）」，`hit` 只证明钩子被调用、不证明返回值生效；功能判定给「最少挂点数」以容纳一键多挂点 |

| 41 | 只改 so 的轮次还要重打 1.8 GB 包 + 装机（每轮 ~20 分钟、用户白测一次） | 不知道包内 so 已解压：包 `extractNativeLibs=true` 时 so 就在 `/data/app/~~<随机>/<pkg>-<随机>/lib/arm64/<lib>.so`（实测该目录 **root 可写**，回读 md5 一致）；但 `adb push /data/local/tmp/starry_magic/x.so` 会被拒（该目录 root 700） | 用 `04_scripts/_common/so_hotswap.sh --serial S --pkg P --lib-name libstarry_magic.so --so <本地so> [--restart] [--judge]`（游戏包装 `04_scripts/明日方舟/so_hotswap_mrfzz.sh`，`--restore` 从正式包抽 so 还原）：push 到 `/data/local/tmp/_hs_$$.so` → `su -c 'cp … && chmod 755 && chcon u:object_r:apk_data_file:s0 …'` → 回读 md5 比对。**硬边界**：热替换态与包内 so 已不一致 ⇒ 本轮结论必须标注 so md5；改 dex/清单/签名、或交付轮 ⇒ 回完整链路 `pipeline.sh <游戏> reset build_so → run → next --loop` |
| 42 | 想「首包自带诊断、交付包自动剥掉」时，用 `if (DIAG) LOG(...)` 或运行期开关关日志 | 只要字符串进了 `.rodata`，交付包里就能被扫到（G1 的禁用串会拦，`starry_selftest` 就是按这条被禁的）；运行期开关还留了「诊断代码在包里」这一层 | 用**编译期宏闸**：`#ifndef STARRY_DIAG → 默认 1`，`SDIAG(...)`＝`#if STARRY_DIAG do{__VA_ARGS__;}while(0) #else do{}while(0)`（**必须 `#if`，`if(常量)` 不删串**）；mk 里照 `ifdef STARRY_TRACE` 先例加 `ifdef STARRY_DIAG → LOCAL_CFLAGS += -DSTARRY_DIAG=$(STARRY_DIAG)`；build 脚本把 `DIAG=` 并入 CFG_HASH 并做**正/负双向断言**（`STARRY_DIAG=0` 时诊断串命中数必须 0、证据串必须 ≥1）。分界写进头注：`hooked/hit/method miss/nested miss/dobby fail/重试/lib ready`＝证据层（永久保留，判读脚本依赖），`diag:/calib:/OkPtr probe/by-name ok/feature #N/status/SELFCHECK`＝诊断层（随包剥除） |
| 43 | 判「本段有没有 ANR/闪退」用**时间窗**（`since "MM-DD HH:MM:SS"` 之后的行） | 设备 logcat 的 threadtime 时间戳**没有年份**，跨月即失效：设备时钟停在 `11-09`，与 `date '+%m-%d'` 的 `10-10` 做字符串比较 ⇒ 旧会话的行被算进本段（实测把 3 个月前的 `Fatal signal 6 … pid 1054 (init)` 算成本段崩溃） | 段内口径一律**按 pid 集合**：段开始时取 `pidof -s <pkg>`（起游戏后轮询等）＋末尾「最后一条 `[mrfzz]` 行的 pid」，两者去重后 `grep -aE … \| awk -v p=$pid '$3==p'` 求和；再加**重启判据**「启动 pid ≠ 最后 MOD 会话 pid ⇒ 期间死过一次」（仅当本段是自己冷启时成立，`--no-launch` 复盘要关掉）。已落 `04_scripts/明日方舟/l3_round.sh` |
| 44 | 「踩过就记一行」还是拦不住同一个坑再犯（本轮又撞 `rg` / `timeout` / BRE 方括号） | 记录是给人看的，脚本里的坑要在**跑之前**由机器抓；而 `rg` 缺失这类坑**不报错**，只在结果里静默少东西 | 做成机器 lint：`04_scripts/_common/lint_shell_traps.py`（R1 `rg` 不存在→**ERROR**、R2 设备侧 `$((0x…))` 32 位截断、R3 未加 `-F/-E` 的 BRE 方括号、R4 对大 `.apk` 直接哈希、R5 用 `timeout`、R6 `grep -c` 进 `&&` 短路），已挂进 `preflight.sh` 的 `.sh` 分支（ERROR 级挡关，WARN 只提示）。实测抓到真问题：`04_scripts/明日方舟/inspect_binge_mrfzz.sh:21,38` 两处裸 `rg`（本机会静默空结果）已改 `grep -E`。误报也要收：`[^ -~]`、`[a-z]` 这类**有意**用字符类的模式要放过（规则里显式 skip `^`/`-`） |
| 45 | 检查类脚本（判读/闸门/lint）本身没有自检 ⇒ 它给的「通过」不可信 | 本轮三个检查器各自都带过真 bug：判读夹具 `hn=0` 仍写 1 行 hooked、根因闸门把 `last` 取成 streak 里**最早**的轮次、lint 对 `[^ -~]` 误报 —— 都是「检查器错一次，结论全反」 | 每个检查器都加 `--selfcheck`：**正控**（该抓出的一律抓出）+ **负控**（干净输入零命中/该放行的放行），夹具自带 `RCS_ROOT`/临时目录等隔离手段；`--selfcheck` 全绿才算这个检查器可用。已落：`实测判读_mrfzz.sh --selfcheck`、`rootcause_note.sh --selfcheck`、`lint_shell_traps.py --selfcheck` |
| 47 | 外科包（底包+单条目替换）跑 `check_release_apk.sh` 被判 `TEMP_DEBUG ×5` FAIL，但包是干净的 | `TEMP_DEBUG` 是**官方游戏自己的**调试串（官方 `assets/bin/Data/Managed/Metadata/global-metadata.dat`、`assets/bin/Data/globalgamemanagers.assets.split0` 里本来就有）；该门禁的内置清单是为 **gradle 助手包**设计的，对「底包原样条目」没有白名单机制 | 判据换成**与底包对照**：同一条目在官方包里也命中同一串、且两侧 sha256 相等 ⇒ 判 `OUT-OF-SCOPE（底包自带）`，不算违规。外科包的等效门禁 = 自脚本里的「未改动条目 sha256 全等」断言 + `aapt2 dump badging` 版本一致 + Manifest xmltree 前后 diff 仅预期变更。**不要把官方串加进 `10_docs/<游戏>/出包禁用串.txt`**（那是追加禁用项，没有放行语义） |
| 48 | 给二进制 Manifest 新增字符串后要补它的平台 res id（resource map） | 把 map 追加基址当成 `(总长-16)//4`（少算 2 个 u32 头）⇒ 追加项**整体错位 2 位**：新属性名 `foregroundServiceType` 的 res id 平台读成 `0`（属性等于无效），而 `aapt2 dump xmltree` **照样正常复读**成 `foregroundServiceType=0x40000000 (Raw: "specialUse")` —— 视觉上「看起来成功」 | resource map 语义 = **按字符串索引稠密排列**（`map[i]` = 字符串池第 `i` 条的 res id，元素名对应项为 0）；追加基址 = `(chunk总长 - 8) // 4`。**判据不能只看 aapt2**：补丁器必须自带「回读 `map[idx(新属性)]` == 期望 res id」断言，并同时校验一条**基线属性**未被破坏（`android:label=0x01010001`、`appComponentFactory=0x0101057A`）。落 `04_scripts/追迹迷城/patch_zjmc_binary_manifest_v3.py` + `zjmc_manifest.sh` 双重回读 |
| 49 | 想用「替换 Application」当注入入口（`<application android:name=自己的类>`） | 属性记录完全合法、`aapt2 dump xmltree` 复读也显示 `application name=…`，但**平台不实例化它**：自家类零日志、自家 so 未映射（`/proc/<pid>/maps` 无命中），而且「类缺失本该 `Unable to instantiate application` 崩溃」也没发生 ⇒ 该属性在这台机器/这个包上等于没写。另：`dumpsys activity processes` 只列已实例化的 `ContentProviderRecord`，`dumpsys package -v` / `cmd package dump` 都**读不到 `ApplicationInfo.className`**（本 ROM 也不打印无 intent-filter 的组件），这些都当不了判据 | 入口改走 **ContentProvider**（`ContentProvider.onCreate` 早于 `Activity.onCreate`；本框架现成件 `com.starry.magic.StarryInitProvider`），并把 `<provider>` 插在 `<application>` 的**首个子元素位置**（先于官方/广告 SDK 的 provider 跑）。判据只认：① 自家 tag 的 logcat（先 `logcat -c` 再冷启，窗口覆盖进程启动）② `/proc/<pid>/maps` 有没有自家 so ③ `am start-foreground-service -n <pkg>/<类>` 的报错文本（`Requires permission not exported from uid …` = 组件已被平台登记；`Not found` = 没登记）。踩坑记录：`logcat -v time` 的 pid 带括号、行格式 `L/Tag ( PID):`，用 `awk '$3==<pid>'` 过滤会全空，正解 `grep -a "( <pid>)"` |
| 50 | `check_release_clean.sh --tag <TAG>` 报「没找到扫描对象」 | `--tag` 不是游戏名而是**文件名前缀**：它按 `ls "$JNI/Games/${TAG}"*.cpp *.h` 展开，传 `--tag zjmc_1.0.20261009`（带版本号、小写）⇒ 匹配不到 `Games/ZjmcMod.cpp` | 传实际文件前缀：`bash 04_scripts/_common/check_release_clean.sh --tag ZjmcMod --so <libstarry_magic.so>`（大小写敏感）。native 侧门禁与 `check_release_apk.sh`（包侧）是两件，都要跑 |
| 51 | 换新游戏时「框架的反射入口（TapTap 强制网页登录、签名伪装等）会自动生效」 | 这类入口普遍带 **`PKG_WHITELIST` 门禁**：包里没列该游戏 ⇒ 每个入口首行 `if (!PKG_WHITELIST.contains(pkg)) return;` **静默返回**，日志里连一行都没有（实测追迹迷城 1.0.20261009：`StarryTapLogin` 零命中，SDK 默认走 `mobile_client` 拉起 `com.taptap` 客户端，玩家侧看到「没强制网页」）。容易误判成「入口没挂上」或「服务端问题」 | ① 新游戏接入先显式搜白名单：`grep -n "<包名>" mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/*.java`，零命中就先加（同时看类注释里的版本路径判定依据）；② 判「有没有拉起第三方客户端」看 `AppSwitchEvent(targetName=…com.taptap)` / `DCSEX-AppSwitchManager: onAppEnter … exitActivity=`；③ 判「自家入口有没有跑」只认自家 tag（`StarryTapLogin`/`StarrySignSpoof`），**未命中 ≠ 没挂上**——签名伪装类入口每包只打一次日志（`LOGGED_HITS.add(pkg)`），进程启动窗口之外看不到。**系统化入口清单见 `skill/starry-mod/SKILL.md`「TapTap 接入检查表」，机器六项检查见 `04_scripts/_common/framework_entry_audit.sh`（漏配即 rc=1，禁止出包）** |
| 52 | `trash_paths.sh` 清理 `06_dumped/<游戏>/` 的某个 dump 目录 | 清单按**目录**写（如 `06_dumped/追迹迷城/1.0.20260912_il2cpp`），而该目录里可能压着**已入库（tracked）的锚点文件**（`rva_targets.json` 这类「复现入口」）⇒ 清理把 tracked 文件也一起移走，工作区出现 ` D` 删除；事后只看 `ls` 会以为「目录本来就没东西」 | 清理**前**对清单逐项核对入库状态：`git ls-files --error-unmatch <路径>`（或清理后 `git status --porcelain` 查 ` D`），命中的 tracked 路径**从清单剔除**；误删用 `git checkout -- <路径>` 恢复。禁通配符只是第一层，第二层是 track 白名单核对。实测：2026-10-10 追迹迷城收官时误移 `06_dumped/追迹迷城/1.0.20260912_il2cpp/rva_targets.json`，同轮 checkout 恢复。**2026-10-10 起脚本已内建该门禁**（`trash_paths.sh` dry-run 直接拒绝 tracked 项，另有 `--suggest --game <游戏>` 生成已排 tracked 的草案） |
| 53 | 在 macOS 上用 frida 做 Java 侧取证（判「自家类有没有被实例化」） | 三个坑会叠在一起：① macOS **没有 `timeout`** 命令（`bash: timeout: command not found`，rc=127）；② frida CLI 的 `console.log` 在 stdout **重定向到文件时被缓冲**，脚本被 `kill` 后不 flush ⇒ **输出全丢**（看起来像「脚本没跑」）；③ **Frida 17 起不再内置 Java 桥**（脚本里 `Java is not defined`），要用 Java 侧 API 得自备 `frida-java-bridge` + `Compiler()` 打包（仓库设施 `01_tools/phantom-frida/src/`） | ① 用 `nohup … & sleep …; kill $!` 或 `gtimeout` 代替 `timeout`；② 一律加 `-o out.log`（frida 自带落盘、逐条 flush），不要靠 shell 重定向；③ 判「so 有没有加载 / 入口类有没有实例化」**别默认开 frida**：`logcat`（先 `-c` 再冷启）+ `/proc/<pid>/maps` 两条更快且无依赖（见 `skill/re-implement/hook-rules.md` §29.5.1）。设备侧 frida-server 需手动起：`su -c 'nohup /data/local/tmp/frida-server -D &'`；**用完必须回收**：`adb shell "ps -A \| grep -i frida"` → `su -c 'kill <pid>'`，别把 root 常驻进程留在用户设备上 |
| 54 | 装机后功能全无，想用「重工具」一步定位（frida spawn / 全量 `dumpsys` dump / 读系统包缓存） | 四条**结构性死路**（2026-10-10 追迹迷城实测浪费约半小时）：① `dumpsys package -v`、`cmd package dump`、全量 `dumpsys package` 都**读不到 `ApplicationInfo.className`**（只打印 `applicationInfo=ApplicationInfo{<hash> <包名>}`）；② `/data/system/package_cache/` 只缓存**系统包**，第三方包不在其中；③ `/data/system/packages.xml` 不列组件、不记 className；④ `dumpsys package` 的 Service/Activity Resolver Table **只收录带 `intent-filter` 的组件**，自家新增的 `<service>`/`<provider>` 本来就不在里面 ⇒ 当不了判据 | 先按症状查判据表 `skill/re-implement/hook-rules.md` **§29.5.1「症状 → 第一条判据命令」**（一条 logcat 就能定段=A/B/C）；Manifest/入口类的前置检查与出包前断言用两个新工具：`bash 04_scripts/_common/framework_entry_audit.sh --game <游戏> --pkg <包名> --apk <apk>`（入口四路 + 白名单/映射 + 包内 dex/manifest 自检）与 `python3 04_scripts/_common/axml_inspect.py <AndroidManifest.xml> --assert-map`（resource map res id 回读断言）。组件是否被平台登记：`am start-foreground-service -n <pkg>/<类>` 报 `Requires permission … not exported from uid` ＝已登记、`Not found` ＝未登记 |
| 55 | 二进制 Manifest 补丁器里的**平台属性 res id 常量**（`stopWithTask`、`foregroundServiceType` …）凭记忆写 | 记忆值错得离谱却**测不出来**：初版把 `stopWithTask` 写成 `0x01010281`（真值是 **`android:glEsVersion`**）——补丁器自带的「回读 map == 期望值」断言、`aapt2 dump xmltree` 复读、aapt2 编译全部照样通过（它们比的是**同一个错常量**）；平台则按 res id 取属性 ⇒ 读到的是另一个属性（本次恰好落在 `<service>` 上不合法而被忽略，且值 `false` 与默认值相同 ⇒ 无功能影响，只有真值表能抓到）。同类误值：`hardwareAccelerated` 0x0101027d✗/0x010102d3✓、`largeHeap` 0x0101025a✗/0x0101035a✓、`noHistory` 0x0101002d✗/0x0101022d✓、`resizeableActivity` 0x010104f8✗/0x010104f6✓、`banner` 0x01010030✗/0x010103f2✓ | 真值**唯一来源**是 android.jar 的 `resources.arsc`：`$ANDROID_HOME/build-tools/<ver>/aapt2 dump resources $ANDROID_HOME/platforms/android-<api>/android.jar \| grep -E "resource 0x0101[0-9a-f]{4} attr/" \| sed -E 's/.*resource (0x0101[0-9a-f]{4}) attr\/([A-Za-z_0-9]+).*/\2 \1/' \| sort -u`（本机实测 1570 条 attr）。表落地在 `04_scripts/_common/axml_inspect.py` 的 `KNOWN_ATTR_RES_ID`（头部已写明再生成命令，勿凭记忆改），校验用 `python3 04_scripts/_common/axml_inspect.py <AndroidManifest.xml> --assert-map`（`idx < len(map)` 且与真值不符即 FAIL）。**改了这张表必须跑跨游戏回归**：`07_patched/*/surgical_*/AndroidManifest.xml` 全量跑一遍，6/6 rc=0 才算表对 |
| 56 | `gradle_cycle.sh` 出包成功、门禁全绿，装机后菜单却是**别的游戏**的功能表 | `mod菜单/starry-mod-menu/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so` 是**跨游戏共用**路径（`.gitignore` 忽略、本文件 §「构建产物不入库」列了 `jniLibs`），而 `app/build.gradle:36` 明确 `externalNativeBuild disabled (ASCII CI): so from jniLibs` ⇒ gradle **不编 so、原样打包** jniLibs 里躺着的那个。并发会话/上一轮跑别的游戏的 `scripts/<游戏>_so_only.sh`（例：`zjmc_so_only.sh` 走 `Android.mk` + `-DSTARRY_GAME_ZJMC`）会把 so 覆盖成**那个游戏**的；出包门禁只看可调试位/禁用串，**不看 so 身份** ⇒ 「门禁绿 + 菜单错」。实测 2026-10-10 邂逅在迷宫 v3.0：装出的菜单是追迹迷城的 `秒杀/无敌/全屏秒杀/无限技能/免广告`（＝ `Main.cpp` 的 `STARRY_GAME_ZJMC` 表），so md5 `5dba6b41830703f1cf526c07f9658b88`、4,217,360 B、mtime 17:22，早于 APK 构建时刻 18:34 | ① 出包前**必须**确认 jniLibs 里的 so 是**本游戏**的；② 本游戏 so 用自己的私有 mk 重建：`cd app/src/main/jni && <NDK>/ndk-build NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_<游戏>.mk NDK_APPLICATION_MK=./Application.mk NDK_OUT=../../obj_<tag> NDK_LIBS_OUT=../../libs_<tag>`（`NDK_PROJECT_PATH=../..` 从 jni 目录算起 ⇒ 产物落 `app/src/libs_<tag>/arm64-v8a/`，**不是** `app/libs_...`）；③ 身份判据用**本游戏串**：`python3 -c "d=open(p,'rb').read();[print(s,d.count(s.encode())) for s in ['邂逅在迷宫','伤害倍率','SeekBar_伤害倍率_1_100_1','免广告','秒杀','全屏秒杀','[Zjmc]']]"`（中文多字节，`grep -a -o` **假阴性**；ASCII 标记如 `[Zjmc]` 更可靠）——本游戏串 >0 且**别的游戏串 ==0** 才算对；④ 换 so 前把旧的**备份到 `/tmp`**（别删，可能是并发会话正在用的）；⑤ 换完重跑 `gradle_cycle.sh`，再 `unzip -p <apk> lib/arm64-v8a/libstarry_magic.so \| md5` 与 jniLibs 对得上才算打出本游戏的包。同类先例：`scripts/surgical_phigros_3.20.0.sh:123`（注释「so 必须是本游戏，防 jniLibs 旧 so 污染」）、`scripts/surgical_gjc2_1.1.8.sh:96`（断言 `ERROR: so 仍含 [Dwct]，jniLibs 未刷新`） |
| 57 | HybridCLR 热更 DLL 里插「诊断日志」做取证，装完面板/UI 卡死，logcat 出 `MethodNotFind UnityEngine.Debug::Log` | 手搓 Unity API 引用：`new MethodReference("Log", void, tDbg)` 且把参数写成 `System.String`（`Debug.Log` 真实签名是 `Log(object)`）；更早一版还把 `Debug` 的 scope 解成 `AnimationModule`（`Could not load type 'UnityEngine.Debug' from assembly 'UnityEngine.AnimationModule'`）。两次都卡在 `AdsRewardMenu.SetGetBtn → AdsRewardMenu.Init()`，是我方自伤而非游戏问题。Mono.Cecil 里手搓的签名**不会**被运行期按方法名兜底 | 借用 DLL 里**真实调用点**上的 `MethodReference`（在 `mod.GetMemberReferences()` 里筛 `DeclaringType` = `UnityEngine.Debug` 且 `Scope` = `UnityEngine.CoreModule`、恰好 1 参的那个），scope 与签名必然与运行期一致；实在没有调用点才自建 `Log(object)`（参数用 `mod.TypeSystem.Object`，**不要** `System.String`）。两者都拿不到就**整个不出探针**（诊断只是取证手段，不能因为它卡出包/卡界面）。实现见 `04_scripts/邂逅在迷宫/ilpatch/Patch.cs::ResolveUnityDebugLog`（v3.5 起会打印借到的真实签名） |
| 58 | 想要「热更注入一次、之后永久生效、不再开通道」：补丁 bundle 随便命名，第二轮冷启就被官方版本覆盖/回源 | 下载器「要不要下」的判据是**文件名存在性**（同名即视为已是最新，不校验内容）；而官方清单里的 bundle 名 = `endlessstrikedll_<官方构建哈希>.bundle` ⇒ 本地那个文件若叫别的名字/带别的哈希，官方清单就认为缺文件并回源覆盖 | 让**补丁 bundle 顶着官方构建哈希**命名（`04_scripts/邂逅在迷宫/xhzm_forge_cdn.py --hash <官方 bundle 的 41 字节前缀哈希>`，脚本会同步改写包内前缀里的构建哈希保持自洽），注入轮就把 payload 落在**官方文件名**下；此后每局真 CDN 的官方 buildpatch 都指向这个「已存在」的文件 ⇒ `dll :不需要下载更新` 且装载我们的 DLL。实测 2026-10-10 邂逅在迷宫 v3.5：第一轮开通道注入（`load endless dll, using 181 ms,len=24681472`）→ 第二轮零通道冷启（`Versions.UpdateAsync Success,diff size=4408` 只回源一个 4.4 KB 官方 buildpatch、`GetDownloadSizeAsync dld count =0`、`len=24681472`）⇒ 永久免开通道。判据：第二轮 logcat 里**一条 `StarryCdn` 都没有**（证明确实没走通道） |
| 59 | 想让免 root 助手的悬浮菜单「重启手机 / 被杀之后自己回来」：加 `BOOT_COMPLETED` 广播接收器 + `START_STICKY` | 国产 ROM 有**应用自启动管控**：实测 ColorOS/OPPO（设备 f9a116a2，包 `com.starry.magic`）`am broadcast -a android.intent.action.BOOT_COMPLETED -p com.starry.magic` 直接被拦，logcat `W OplusAppStartupManager: pkg: com.starry.magic not in sps black list, check top false, prevent start` / `I OplusAppStartupManager: prevent start com.starry.magic, cmp  by broadcast  callingUid 0, strict mode = true`；`am force-stop` 后广播被 stopped 态拦截；`kill -9` 后 `dumpsys activity services` 显示 `Restarting services: * Restarting ServiceRecord{…MenuService}` 但 `nextRestartTime` 已过仍未重启（`restartReschedulingCount=1 RestartSceneMask=2`）⇒ `START_STICKY` 重启被**无限期推迟**。广播自启只在用户手动把 App 加进自启动白名单后才可能有效，做不到开箱即用 | **别做自启动**：分发版改走「重开一次 App」的自救路径（见 #60），把 `BootReceiver` / `RomSettings` / `RECEIVE_BOOT_COMPLETED` / `<receiver>` 全部删掉（本作用户 m05963 明确要求：别人重启手机自己会重开 App）。若坚持要做，唯一可靠形态 = 引导用户手动进系统「自启动 / 后台白名单」页（各 ROM 类名不同，须实测），且**必须**同时保留「点通知回前台」的兜底 |
| 60 | 把免 root 助手**发给别人**：装完能用，但用户「菜单球怎么没了」——划掉最近任务、系统清后台、重启手机任一情况都会掉球，收件人以为坏了 | 免 root 形态下菜单**不能**进游戏 APK（FairGuard 按文件级校验杀改包），只能由独立助手进程用 `TYPE_APPLICATION_OVERLAY` 悬浮窗盖在游戏上 ⇒ 球的生命周期 = 助手进程的生命周期。而默认实现还会**主动自杀**：`onTaskRemoved → stopSelf()`、启动 25 s 没等到宿主 Activity 的 `zombieWatchdog → stopSelf()`（这两套逻辑本是给「菜单跑在游戏进程内」写的，独立形态下全是自伤） | 分发版必须做三件事：① `MenuService` 在 standalone 模式下 `onTaskRemoved` 不 `stopSelf`、`zombieWatchdog` 直接豁免、`onStartCommand` 返回 `START_STICKY`，Manifest 上 `android:stopWithTask="false"`；② 前台通知 `contentIntent` 指向助手 `MainActivity`，通知文案写成「点此恢复菜单球」（球的救急入口）；③ 主界面收成**两步**（①悬浮窗权限 → ②一键注入并打开游戏）+ 底部一行「球不见了？重新打开一次本 App 即可」，另附一页《使用说明》。**不要**擅自加自启动或多余系统权限（用户会当成瞎操心，见 #59），更**不要**去动卡密 / 授权校验逻辑 |
| 61 | 项目收尾清理：一堆「以前调试用、现在没人用」的残留（隐藏按钮、只声明未用的常量/计数、已废弃分支与路由） | 这类残留**不报错、不影响跑**（所以极易漏掉），但会误导下一轮：例 `FakeCdn.FAKE_CDN_IP = 10.9.9.9` 与它的 `/32` 路由，在 v3.3 改成「DNS 应答真实 CDN IP」之后已成死概念，留着会让人以为还在伪造 DNS；`MainActivity` 里内测用的穿透按钮/计数则是历史形态（菜单跑在游戏进程内）的遗留 | 判据 = **引用计数**：对候选符号逐个 `grep -rn "\b<符号>\b" java/ | wc -l`，计数 1（只有声明自身）即可删；计数 >1 的要看清是不是死分支（本例 `isCdnDest` 里的假 IP 分支就是死的）。清理范围**仅限本任务涉及的文件**（并发会话里别的游戏的文件一律不动）。删完必须重跑：出包门禁（`check_release_apk` 干净正式包）+ 装机三件套与 `.so` md5 对源码一致 + **注入通道实跑**（覆盖命中 + 回源透传各一发），最后 `git add` 只加本任务的路径白名单 |
| 62 | 出包/长命令用 `\| tail` 收尾：**产物指纹丢了**——v3.6/3.7/3.8 三版的 APK md5 就是被调用方 `bash gradle_cycle.sh … \| tail -14` 截断而永久丢失（只剩「产物」标题行，md5/大小被切掉），事后无法补证；同理拒绝路径的日志被截掉就看不到判据 | 管道被截断 ≠ 命令失败，它只是把证据扔掉；合并 `tee`/进程替换后还可能因**异步**（`ls` 看不到刚写的日志）造成「日志没生成」的错觉 | ① 出包走 `gradle_cycle.sh --game <游戏>`：全量日志自动 `tee 09_test/{游戏}/logs/gradle_cycle_{tag}_{ts}.log`，完成块直接打印 **APK/大小/md5/versionCode/日志路径**；要看尾巴就 `tail` **日志文件**，**禁止**给命令本身接 `\| tail`/`\| head`；② `exec > >(tee …)` 是异步的，脚本退出前 `sleep 0.3` 等落盘；③ 出包成功后自动 `gate_note.sh {游戏} G2 通过 "… md5=… size=… 日志=…"`，指纹即时进台账（AGENTS P19） |
| 63 | `08_output/{游戏}` 里出现「一堆没用的文件」——每出一个版本就手搓一个 `delivery_20261010_vNN/`，旧版顶层包、旧交付目录、`prune_manifest.json`/`清理记录_*.json`、`.DS_Store` 全部堆着（用户原话 m06417：「08文件夹内为什么会有一堆没用的文件？看不懂规则吗？」） | 三层成因：① 我按版本手搓 delivery 目录而不是走 `finalize_game_delivery.sh`（AGENTS.md:136–145 明写收尾一律一键脚本、禁手工重造）；② `prune_output_final.sh` **只扫 `-maxdepth 1` 的 `*.apk/.apk.part/.idsig`**，对子目录里的旧包、json 记录、`.DS_Store` 一律不动 ⇒ 只跑 prune ≠ 目录干净；③ `trash_paths.sh` **硬拒绝 `08_output`**（那是它的设计边界），所以 08 的清理只能手工列清单 | 收口判据 = 目录里**恰一个交付目录 + 恰一个 APK**：`finalize_game_delivery.sh --game {游戏} --apk {正式包} --usage-doc … --copy-apk --prune --sync-doc --apply`（delivery 目录名是 `delivery_{YYYYMMDD}`，**不带版本后缀**，同游戏只留最新一个）；动手顺序：先 `md5` 校验要保留的包 ⇒ 逐条**显式绝对路径**（禁通配符）把旧交付目录/旧顶层包/决策 json/`.DS_Store` 移到 `~/.Trash/nixiang_{游戏}_08clean_{ts}/`（可回滚，不 `rm`）⇒ 复验 `find 08_output/{游戏} -type f \( -name '*.apk' -o -name '*.idsig' -o -name '*.part' \)` 输出恰 1 行 + `apksigner verify --print-certs` + `prune_output_final.sh … --dry-run` 报 `removed=0` |

| 64 | 给 H5/WebView 游戏做「静默某个 netcode 弹窗」时，白名单写成数字语义 | `w.__QSW_NETSKIP={9998:1,9999:1}` 配 `!0===NETSKIP[e]`（严格比较）⇒ `1!==true` **恒不命中**，静默一直是空操作；现象是「提示没了」从而误判已修（实际是账号当时没触发该码），下一轮同码复现才发现 | 配置值与守卫**同一类型**：布尔用 `!0`/`!1`（一起改），并补一条「守卫命中」的离线断言（`__QSW_NETGUARD(9998)===true`）与运行期证据行；别用「弹窗消失」当判据 |
| 65 | 想在**已有注入点**再加一条埋点/补丁，另开一个新锚点 | 新锚点命中 **0** 次（同一处文本已被前一个锚点改写过），而脚本对「命中 != 1」是硬失败 ⇒ 白跑一轮；更糟的是若改成「命中 0 就跳过」会静默少做事 | 同一注入点**复用旧锚点**，把新语句追加进既存注入体；对「同一文件多处注入」的顺序先自查（改过的地方不再作为后一个锚点的 old 文本） |
| 66 | H5 游戏加「战斗加速」类功能，按手感调倍率 | 本作战斗是**服务端权威制**（`authorityVersion:1` + `authorityTicket{battleId,validationMode}`）且**速度是服务端协商项**（`authorityControl`，协议只认 `speed1=1.2`/`speed2=1.6`，游戏自带 1x/2x 也须服务端放行）⇒ 客户端单方面提速会让结算「到得太早」，服务端回 `code=9998` 拒判 ⇒ 战斗收不了尾 + 新战斗点不动（叠加游戏自带的「开始战斗 10 秒重击锁」`_clickStart`，观感是「按钮点不动」） | 先埋点取**地面真值**（发送/错误码两条证据行），用「同一链路不同倍率的时序对照」定天花板：10× 时 `barrierUpdateTroop→fightOver` 隔 3 s 被判 9998，3× 时隔 7.5 s 被接受 ⇒ 上限 = 3。别用「能打几场」试出来 |
| 67 | 想给 H5 游戏「关掉某个服务端错误提示弹窗」，逐个找 `alert(` 调用点 | 调用点极多且别名各异（`r=e.GameUtil` / `s=t.GameUtil` …；实测 400+ 处 `X=Y.GameUtil` 全部绑**类对象本身**、零例绑 `alert` 函数值），逐点改必漏（还有 `alert(t.data…?t.data:"netcode_-2")` 把服务端 body 原文当文案的 6 处） | 包装**类的静态方法**一处覆盖全部：`GameUtil.alert`（参数序 `content / okFunc / ? / okWord`），在包装体内按 key 白名单决定放行或静默，并先打一行证据日志（`StarryH5.log("QSWALERT <key>")`）。注意先确认该 key 的弹窗**没有** `okFunc`（本作 9998 的 `r.alert("netcode_"+e)` 无 okFunc ⇒ 点「确定」只是关窗），否则静默会改流程状态 |
| 68 | 飞行记录仪：同一 pkg 重复 `start`（或 `stop --dir` 后不核查） | 会留多个 worker，且**先前的 logcat 采集会中途断流**（本轮 `logcat.txt` 停在 20:59:50，恰好漏掉 21:17–21:22 的 L3 窗口），而 `stop --dir` 只杀 `recorder.meta` 里记录的 pid | 收尾必须 `pgrep -fl "flight_recorder.sh __worker"` 逐个核；证据以**当场 `adb logcat -d` 落文件**为准（`logcat -d` 的环形缓冲也会滚掉旧行，L3 证据要在用户报「好了」时**立刻 dump**） |
| 69 | 免 root 助手往**游戏**的 `Android/media/<游戏包>` 或 `Android/data/<游戏包>` 写状态文件 | 国产 ROM（HyperOS/MIUI 实测）默认**不授予** `MANAGE_EXTERNAL_STORAGE`，写入报 `java.io.FileNotFoundException: … open failed: EPERM` + `E MediaProvider: Primary directory Android not allowed for content://media/external_primary/file; allowed directories are [Download, Documents]`；现象是「注入成功但功能全失效」，极易误判成代码/路径写错 | 装机第一步先查 appop：`adb shell appops get <助手包> MANAGE_EXTERNAL_STORAGE`（`default`/`deny` = 未授予）；再让用户到系统设置开「所有文件访问权限」（HyperOS 里**没有**按包名直达页，`resolve-activity -a android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION` 回 `No activity found`，只有不带包名的列表页 `android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION` → `com.android.settings.Settings$ManageExternalStorageActivity`）；诊断时务必同时看 `StateMirror` 的私有目录回落行，它证明菜单/开关本身是好的 |
| 70 | 拿 `framework_entry_audit.sh` 当**独立助手 APK**（`com.starry.magic` 这种免 root 助手形态）的出包门禁 | 必然报 ✗：`[D] Android.mk:265 激活 -DSTARRY_GAME_ZJMC`（预编译 `jniLibs` so，不跑 NDK 编译，激活宏与本游戏无关）＋`[F] AXML 未含 com.starry.magic.StarryInitProvider`（独立助手入口是 `MainActivity`，不做壳入口注入；dex 里 `Lcom/starry/magic/StarryInitProvider;` 只是框架历史残留）⇒ 4 项 ✗ 全是**形态不匹配**的假失败，照着「修」会把好形态改坏 | 先看脚本自身说明其门禁对象是「壳/宿主 Activity 注入分支 / ContentProvider 入口」（`shell: ysxz/ conch/ h5box/ dwdlm/ ltzj/`）。**独立助手形态的真正出包门禁 = `gradle_cycle.sh` 内建 `apksigner verify` + `check_release_apk.sh`**（manifest 无 `application-debuggable` + 禁用串 0 命中），两者 ✓ 即可出包；审计结果写进 `交付说明.md` 的「机器验收证据」里并注明不适用原因 |
| 71 | 给助手新增的持久化键名 / 类名 / intent extra 随正式包发布 | 撞上 `check_release_apk.sh` 的**内置禁用串**（`04_scripts/_common/check_release_apk.sh:33-35`：`starry_selftest` / `starry_diag` / `starry_debug` / `--ez starry` / `MOD_DIAG` / `DIAG_ONLY` / `TEMP_DEBUG`）⇒ `✗ 命中「starry_diag」×1 → classes.dex`、**包不可交付**（v3.11 首轮就这么废了一个包，md5 `faa99a10…`） | 定键名前先 `bash 04_scripts/_common/check_release_apk.sh --list` 对一遍；功能**必须**留在正式包内时**改中性名**（`starry_diag` → `starry_health`）而不是 `--no-gate` 跳过。注意「能过」的假象：v3.10 的 `starry_selftest` 之所以没命中，是因为它只在 `BuildConfig.DEBUG` 分支里、release 被 R8 剥掉了字符串 —— 非 DEBUG 分支的同类字面量一定会进 dex |
| 72 | 想「机器侧读一下 App 屏幕上的文字」来给 UI 交付做 L2 预检：`adb shell uiautomator dump` | 设备屏息/锁屏时（`dumpsys power` → `mWakefulness=Asleep`、`mScreenOn=false`）+ `dumpsys window` 的 `mCurrentFocus=Window{… NotificationShade}` 状态下，dump 出来的只是状态栏/锁屏（`星期六` `10月10日` `14°`），**拿不到任何应用 View 文本**；且按 #2 我也不能 `input keyevent` 去唤醒（白碰一次屏幕） | 屏幕文字的验收**只能走 L3 人工肉眼**（或让用户念/复制报告发回）；机器侧能做的替代 = 装机包 md5 与产物逐字节比对、进程/`MenuService: overlayVisible=true attached=true` 日志、`check_release_apk` 门禁。要机器可读就把结论**落文件**（如状态文件）而不是只放 UI |
| 73 | 「免 root 一次性注入」方案（补丁 bundle **顶着官方构建哈希命名**投递）遇到**目标机游戏目录里已有同名官方文件** | 客户端「要不要下载」只按**文件名存在性**判定（`Versions.UpdateAsync Success,diff size=0`）⇒ 官方同名件已在位，下载器直接认为「已是最新」而**根本不下载**；现象极具迷惑性：助手侧**全绿**（① ② 权限 ✓、状态文件与菜单一致 ✓、通道建立→隧道就绪→自动关闭 ✓、「游戏来取补丁 ✓ versions.json（505 B）」），游戏里却**零效果**。判据 = **游戏取包清单的条数与大小**：成功轮三件（`505` + `4408` + `9801205`），失败轮**只有 `505` 那一个** ⇒ 补丁 DLL 从没进游戏、跑的仍是官方 DLL（而官方 DLL 不读我们的状态文件）。一加 `PJE110`/Android 16 实测（m00801）：面板结论 ✅ 但用户实测三功能全失效 | ① **交付说明/使用说明必须先写这条硬前提**：让目标机把游戏**卸载重下（全新安装）+ 进过一次登录页**再注入（干净目录下与成功轮口径一致；用户 m00856 实测「重下游戏就没问题了」）；② **判定注入成败别看助手侧状态，要看游戏取走了几个文件**（三件齐全才算成功，只有 505 B＝命中本坑）；③ 别急着改下发策略（换版本号/换文件名会牵动 `versions.json` 一致性）——先把「重下游戏」这条代价最低的路走完 |
| 74 | 拿 `framework_entry_audit.sh` 当 **modern 注册表**游戏（`Games/<x>Entry.cpp` + `Android_<x>.mk`，`GameConfig.h` / 公共 `Android.mk` 零接触）的出包门禁 | 必然报 ✗×2，且都是**形态不匹配假失败**：`[C] GameConfig.h 无 defined(STARRY_GAME_<X>)`（该文件 40 条映射全是 legacy 游戏）、`[D] Android.mk:265 激活 -DSTARRY_GAME_ZJMC`（legacy 单文件切换分支；modern 游戏按 `Android_<x>.mk` 编译 ⇒ 激活行与本游戏无关）。审计自身会打两条 `⚠ …可能是 modern 注册表，人工确认包名映射位置`（霓虹深渊2 / 2026.10.10 底包 2026-10-11 实测：rc=1，2 ✗ / 2 ⚠，而 `[A][B][E][F]` 全 ✓） | 判据 = **先自证「包里的 so 不是别人的」再判不适用**，别只喊形态不符：① `Games/<x>Entry.cpp` 里有 `.authGameId = "<slug>"` 且文件注释写明「MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动」；② 该游戏 `Android_<x>.mk` 内 **0 处** `-DSTARRY_GAME_*`；③ 包内 `lib/arm64-v8a/libstarry_magic.so` 的 md5 == `libs_<x>/arm64-v8a/` 构建产物；④ `.so` 里本游戏 featureList 串**各命中 1 次**、他游戏游戏名 **0 命中**（用 `open(f,'rb').read().count(b'…')`；`grep -a` 打多字节中文串必须加 `LC_ALL=C`，否则 0 命中假阴性）。四条齐 ⇒ `gate_note.sh <游戏> 审计C/D 不适用 "<证据>"` + `交付说明.md`「机器验收证据」注明原因；**真正门禁仍是 `gradle_cycle.sh` 内建 `apksigner verify` + `check_release_apk.sh`**（#70 同类）。注意 `[A][B]` 是**该修的真项**（漏配 `TapLoginForceWeb.PKG_WHITELIST` ⇒ 6 处 `contains` 静默 return 零日志；漏配 `SignSpoof` 证书表 ⇒ 渠道 SDK 进程内读到原签），补进两张表后必须**重编 gradle → 重跑 surgical → 重签名**，旧包不许复用 |
| 75 | 把**枚举的实例背衬字段**（`value__`）当成静态字段喂给 `il2cpp_field_static_get_value`（本仓 `Api::field_static_get`） | **整进程 SIGSEGV**，而且崩在游戏线程里、看起来像「游戏自己崩」：`il2cpp_class_get_field_from_name` 对实例字段一样返回有效 `FieldInfo`（所以「字段取到了」的日志会骗人），但实例字段没有静态存储 ⇒ 该 API 拿 `static_fields(NULL) + offset` 当**源地址**去 memcpy（霓虹深渊2 2026-10-11 实测：CostType 枚举表首项就是 `"value__"`，`memcpy(dst=0x7421e97fa0, src=0xb8, n=0x110)`、tombstone `Cause: null pointer dereference` / `fault addr 0xb8`；栈顶 `libil2cpp.so` ← 游戏 `SyncHooks` 的字段解析段） | ①**白名单只列真静态字面量**（枚举值项），永不迭代「类里所有 field」；②每个静态读取都过 `RunGuarded`（`sigsetjmp` 保护区）⇒ 退化成一行动 `CostType 跳过 N 项` 而不是死进程；③判别事故是否属于本坑：`fault addr` 小且是 `offset`（如 `0xb8`/`0x110`）、`x1` 也是小地址、栈里出现 `field_static_get`。**注意保护区不可嵌套**：本仓 `t_faultJmp` 是**单层** thread_local jmpbuf（每进一次 `RunGuarded` 就 `sigsetjmp` 覆盖），在已有保护区的函数外再包一层 ⇒ 内层返回后若再出信号，`siglongjmp` 会跳回**已返回的栈帧**；加固必须逐调用点做，别整函数包 |
| 76 | 菜单里**滑块（slider）项**的功能，MOD 侧把「开」判定写成 `features[N] && values[N] > 1` | 功能全程**零效果**且日志里**一行 `install pass` 都没有**（＝一个 hook 都没装）：框架对 slider 只调 `mb_nativeSetValue`（`MainModern.cpp:397`）**只写 `g_values[]`，从不写 `g_features[]`**（写 features 的是 `mb_nativeSetFeature`，只对 toggle 项发）⇒ `features[滑块下标]` 恒假 ⇒ `AnyFeatureOn()` 恒假 ⇒ `SyncHooks()` 早退；toggle 项不受影响，所以现象是「开关都灵、滑块全死」，极易误判成「hook 挂错点/数值没生效」 | **滑块项即开关**：`values[N] > 1`（1 = 关，滑块默认位）单独成立即可，不要 `&& features[N]`；并抽成 `DmgMulActive()/SpdMulActive()` 供 `AnyFeatureOn()` 与各 hook 门禁统一引用。**装机自检第一眼先数 `install pass: ok=N dead=M`**：没有这一行就别去怀疑 hook 逻辑（本次真机上「移速拉到 10 无效」的真因就是 0 hook，不是挂点错） |
| 77 | 换了代码要重装到真机，机械照抄「卸载 → 安装」流程 | **同签名包之间的 `uninstall` 是纯破坏：白清一次游戏本地存档**。真正的两条判据被漏看：① `apksigner verify --print-certs` 的证书指纹（官方包 vs 自签包 **必然不同** ⇒ 必须卸载；而我自己的星包每一版都是同一把 `starry_release.jks` ⇒ **不许卸载**）；② versionCode 是否相同/更高（相同也可 `install -r` 原地覆盖）。霓虹深渊2 2026-10-11 轮次2 实测踩中：已装的是星光包（证书 `83c5cc0b…` / versionCode 565），新包同证书同 versionCode，我仍走了 `uninstall` → 本地存档被清（云端存档不受影响） | 改代码后的重装按**改动面**从轻到重选：① 只换 `lib/arm64-v8a/libstarry_magic.so` ⇒ **root 覆盖已释放的 so**（`/data/app/~~xxxx/<pkg>-yyyy/lib/arm64/`，`chmod 644` + `chcon` 与原文件同型 + `am force-stop` → 冷启），**完全不碰包安装**；② 换了 dex/smali/manifest ⇒ `adb install -r <new.apk>`（同证书原地覆盖，数据保留）；③ **只有证书变了**（官方→自签、换 jks）才 `uninstall`，且卸载前必须把「本地存档会清、云端存档不受影响」讲给用户。装机方案选型卡里也要按这三档写，别只写一个「卸装」。**★适用范围（用户 m02896 裁决，必读）**：本规则只管**常规改包**游戏；带**热更/派生负载层**的包（乐变、其它 runtime 解密壳）不适用 —— 覆盖安装会让壳弃用派生负载却保留版本标记 ⇒ 游戏退回映射 APK 内加密的 `libil2cpp.so`、hook 全灭。此类游戏目前**只有无悔华夏**（`com.yhhy.whrhx.aligames`）：它的装机**只许**卸载重装（`bash 04_scripts/无悔华夏/whrhx_clean_reinstall.sh --uninstall`），依据用户 m00888，见 `10_docs/无悔华夏/硬约束.md` 第 4 条。**该例外不得外推到任何其它游戏**（用户 m02896：「只许卸载重装这个只对无悔华夏生效」） |
| 78 | 自签改包游戏的**启动登录失败**被当成「我们改坏了登录」 | 本仓 dex 层强制 TapTap 网页登录（`ClientLoginUtils.isClientLogin` 恒 false ⇒ TapSDK `LoginTypes.from(false)` 走 `embedded_web`，WebView OAuth 页）⇒ 登录耗时 28–37 s，而游戏自己的 `StartInteractiveLogin` 有固定 ~37 s 交互登录预算 ⇒ **变成竞态**。霓虹深渊2 2026-10-11 实测：轮次1 网页 28.3 s 返回 ⇒ `CloudBase Login Success!`（进得去）；轮次2 网页 37.3 s 返回，比超时晚 0.2 s ⇒ `E Unity: [TapSDK] LoginWithScopes timed out, likely TapTap client unavailable/unresponsive.` + `E Unity: LoginService: Login Failed - code=5040, message=TapTap login timed out` + `NEON.Framework.Login.LoginService:HandleLoginFailure(Int32,String)` + `UIConfirmDialog.Show`（玩家看到的就是「登录失败，请通过 taptap 客户端启动游戏」） | ① **判「是不是我们的包改坏了」只看这三条**：`Mod_Menu: [nhsy2] lib ready (by-name hooks only, lazy install)`（我们的 so 只加载、未挂钩）＋ `classes5.dex`/manifest 与上一成功轮同尺寸 ＋ 失败点全在 `TapTap`/`Java`/`Unity` 侧，栈里没有 `libstarry_magic.so` ⇒ **跟 MOD 无关，别去改 hook**；② 处置＝**重试登录**（WebView 缓存/会话热起来后常能压进预算；失败时游戏会打 `CloudBaseService: Logged out and tokens cleared.` ⇒ token 已清，可干净重试），关键时间戳对照：`button click LoginButton confirm` → `TapTapLoginActivity onActivityCreated` → `ActivityClient: activity finished by caller … WebLoginFragment$initializeWebView$2.shouldOverrideUrlLoading → TapTapLoginActivity$fragmentProvider$1.onSuccess`（这一行就是网页登录回调，晚于超时即 5040）；③ 若连续多轮仍超时，再上更大的实验：**放开客户端 SSO 路径**（`isClientLogin` 不恒 false，靠 `TapSignKit.getSign` 回灌官签去过 `com.taptap` 客户端的签名校验，客户端握手是秒级、天然不进超时） |
| 79 | 手跑 `ndk-build` 编某个 modern 游戏的 so（如 `libs_nhsy2`），沿用「把参数写成环境变量前缀」的直觉 | 两种错法都会失败：① **cwd 不在 `app/src/main/jni`** ⇒ `make: *** No rule to make target 'MainModern.cpp', needed by '…/obj_nhsy2/local/arm64-v8a/objs/starry_magic/MainModern.o'.  Stop.`（`Android_<x>.mk` 里 `LOCAL_SRC_FILES` 是相对路径，make 不 chdir，只按**当前目录**找；`NDK_PROJECT_PATH`/`APP_BUILD_SCRIPT` 给绝对路径也没用）；② **把变量当 env 传**（`NDK_APPLICATION_MK=… bash ndk-build` 之外还指望 make 读到）⇒ 参数进不去，`Application.mk` 被别的 wildcard 覆盖。另：macOS 上满屏 `fcntl(): Bad file descriptor` 是 ndk-build 的无害噪音，别当成错误 | 唯一可用形态（霓虹深渊2 2026-10-11 实测 rc=0）：**先 `cd <repo>/app/src/main/jni`**，再把变量当 **make 参数**传给 `ndk-build`：`bash $NDK/ndk-build NDK_PROJECT_PATH=$ROOT/app APP_BUILD_SCRIPT=$ROOT/app/src/main/jni/Android_<x>.mk NDK_APPLICATION_MK=$ROOT/app/src/main/jni/Application.mk NDK_OUT=$ROOT/obj_<x> NDK_LIBS_OUT=$ROOT/libs_<x> -j8`（$ROOT = mod菜单/starry-mod-menu）。产物只在 `libs_<x>/arm64-v8a/libstarry_magic.so`；编完必须重跑 surgical（脚本自带 `LC_ALL=C grep -aq 'nhsy2' $SO` 的自证守卫）再签名 —— `surgical_*.sh` 见到旧 so 会**跳过编译直接用旧的**（`SO_PREBUILT`，可用 `NHSY2_SO=` 显式指向新 so 防呆） |
| 80 | 用 `MetadataSearch` 的 `fields`/`methods` 输出里那些 `typeIndex=N` 去反查「这个字段到底是什么类型」 | 直接 `grep "typeIndex=43356"` 或按行首匹配都取不到有效样本：dump 行格式是 `F <name> typeIndex=N token=0x…`（**token 在后面**），所以要 `grep -a "typeIndex=43356 " <dump>`（**N 后面必须带尾随空格**）；同理类型行 `TYPE index=N asm=X name=Y methods=M fields=K` 要匹配 `name=<全名> `（尾随空格）。另注意 dump 只覆盖现存类型（本作最大 `index=26128`）⇒ 更大 index 查不到属正常 | 用例：`NEON.Framework.Login.LoginService` 的 `PostLoginTimeoutSeconds` 读出来 `typeIndex=43356`，用 602 条同型字段（半径/延时/伤害/尺寸）抽样确认 **43356 = System.Single**（`34262`=`<>1__state`=Int32、`44464`=string）⇒ 判定它是 `30f` 量级的超时秒数。**推论纪律**：typeIndex 只证明类型，不证明语义（名字里带 `PostLogin` ≠ 一定是那 30 s 预算），要落地仍须真机日志或 hook 验证 |
| 81 | 让长脚本在后台跑（`run_in_background`），**同一时间又去改这个脚本文件**（想「顺手修个坑」） | bash 执行脚本是按**字节偏移增量读取**的，被改动的文件会让后续读取落到**编辑后的错行**上：霓虹深渊2 2026-10-11 实测 `surgical_nhsy2_2026.10.10.sh` 运行中被插入 11 行 ⇒ 日志里冒出**未被重定向的 heredoc 源文本**（本该写进 `merge_out.py` 的 python 代码被打进 `surgical_r5b.log`），并且后续步骤不可预测；表面上像「脚本自己坏了/被谁 echo 了」 | 纪律：**脚本与它读取的配置文件，运行期间一律只读**。要改就先 `job_kill <job>`；改完先 `bash -n <脚本>`（输出为空即 SYNTAX_OK）再重跑，并把这一轮日志与上一轮分开命名（`surgical_r5b.log`）。改脚本后的重跑宁可多花几分钟，也不要相信「跑到一半被改」的日志 |
| 82 | 出包脚本用「候选路径存在即采用」的方式找**底包/输入 APK** | 只要有同名的旧文件在候选位（如 `/tmp/neonabyss2_base/base.apk` = 上一版 10.5 底包），脚本就会拿它开工，直到**内部指纹断言**才失败（`ERROR: 官包指纹不符 got=c17fe80… want=c7d0825…`）⇒ 白跑一轮（本例 surgical 已跑到真机底包校验，浪费数分钟 + 一次错误日志）。危险变体：若脚本没有指纹断言，就会**用错底包出包且无人察觉** | 选文件必须**内容优先**：逐个候选比 md5/sha（`$ORIG_MD5`），不符只打印「跳过候选（底包指纹不符）」继续找下一个，全部不符再 `adb pull` 兜底；同时把期望指纹**写在脚本头部**做单一真值源。调用侧显式传 `NHSY2_ORIG_APK=<绝对路径>` 比依赖搜索顺序更稳（霓虹深渊2 `scripts/surgical_nhsy2_2026.10.10.sh:60` 起已按此改） |
| 83 | 装机预检（`post_install_precheck.sh`）报 `判定段=A 运行中崩溃`，但游戏里一切正常 | 装了 **themis（腾讯反外挂）** 的包会把**任何被捕获的自报信号**上报成崩溃日志：我方 `Il2CppApi` 的 `SafeProbe`（故意触发并捕获 SEGV 来探测 themis 诱饵符号）会让它在 3 s 内打 `D [THEMIS]: send crashlog successed, path=…/app_[THEMIS]/Undertaker/tombstone_*.native.log` ⇒ 被默认 crash 正则命中，**进程其实一直活着**（霓虹深渊2 2026-10-11 连续两轮 r5/r5b 同形假 A） | 判假必须给**同 pid 的后续存活证据**（不要只看那一行）：同一 pid 在此行**之后**继续打 `[nhsy2] lib ready` / `[nhsy2] 初始化成功` / `MOD 已就绪 tag=<game> ready=1`，且 `adb shell pidof <pkg>` 仍在 ⇒ 判「THEMIS 自报假失败」，结论与日志一并记入 `10_docs/{游戏}/台账.md`（已成立表），**不要**去改共享的 `SafeProbe` 逻辑、也不要往标记文件里加 `THEMIS` 白名单（真崩溃时它们也打这一行，白名单会盖住真问题） |
| 84 | `push_main`（`04_scripts/_common/git_push_main.sh`）报「SSH 不可达 ⇒ 改走 HTTPS」，但改用 HTTPS 后**照样全轮失败**；同一分钟内单独手测四条通道却全是 FAIL（2026-10-11 m03016 实测：`curl -x 127.0.0.1:7897 https://github.com` → `rc=35` 且 **~5 秒内秒失败** `LibreSSL SSL_connect: SSL_ERROR_SYSCALL`＝代理节点挂；直连 → `rc=35`/`Connection reset by peer`；`ssh -T git@github.com` 前一条成功 `Hi Aliang369! You've successfully authenticated`、几分钟后 `Operation timed out`；`ssh.github.com:443` 时通时断） | 到 github 的通道是**分钟级抖动**，不是 git 配置问题：任何「开头探一次、失败就锁死某条通道」的写法都会把整轮拖死。正解＝**四通道按序试 + 探测与推送同路径 + 一次调用内重试**（v2.1）：`ssh22` → `ssh443`（`-p 443 -o Hostname=ssh.github.com`）→ `https-proxy` → `https-direct`，每条先 `git ls-remote --heads origin <branch>` 探测（**别用 `ssh -T` 当探测**：v2.0 实测「ssh443 探测通过、push 仍失败」，探针与 push 认证路径不一致）、失败即换下一条并在 `--attempts` 轮次里重来；一律 `git push origin <branch>`（URL 推送不刷新跟踪引用，且 `refs/remotes/*` 会陈旧 ⇒ **判断「推上去没有」以 `git ls-remote origin main` 为准**）。配套：`http.version=HTTP/1.1`、`http.lowSpeedLimit=0`、`http.lowSpeedTime=600`（默认 1000 B/s × 60 s 会把慢代理推送掐断成 `curl 28 Operation too slow`）；被拒即 `PUSH_RESULT=DIVERGED` rc=3 短路，**绝不强推**；`--selftest` 只体检；离线可 `PUSH_MIRROR_DIR=<裸仓>` 落本地镜像。**已推送过的提交不要再 `--amend`**（本会话正是 amend 造成远端/本地分叉、白搭一整轮排障） |
| 85 | 用 Dobby（`DobbyHook`，本仓 `DobbyHookGuarded`）挂 **IL2CPP 的小方法**（4 字节转发器 `b <真实现>`、8/12 字节 getter） | Dobby 在 arm64 上的 inline patch **固定 16 字节**（`ldr x17,#8` / `br x17` / `.quad hook`）⇒ 入口前 16 字节内的**邻居方法入口被改写成补丁中间指令**（`+0x4` 变 `br x17`、`+0x8` 变地址高低半字），游戏以后调用邻居即乱跳或执行未定义指令；霓虹深渊2 实测：`Attri.get_Item` 4 字节转发器，邻居入口紧邻 `+0x4`/`+0x8`、共享实现 `+0xc` ⇒ 三轮装机（`install pass: ok=60 dead=0/60`）后战斗/关卡切换必崩 `SIGSEGV fault addr 0xb8`，且**崩栈里没有我们任何一帧**（`#00 __memcpy_aarch64_simd` ← `libunity.so`）⇒ 极易误诊为「游戏自己崩」 | 按**入口形态**决定挂不挂，别按方法名/业务重要性：① `w[0]` 是 `br xN` veneer ⇒ 跳过；② `w[0]` 是无条件 `B`（`(w0 & 0xFC000000)==0x14000000`）⇒ **顺链解转发器**（≤4 跳，`imm26` 符号扩展 ×4），解到的真实现若已被前面槽位挂钩 ⇒ 判冗余跳过（功能不减，调用会经真实现命中已挂钩点）；③ 解析后 `w[0..2]` 出现 RET/B/BR/BLR（且非入口即 RET 的极小函数）⇒ 跳过（函数体不足 16 字节，邻居入口会被吃）；④ 极小函数（`+0..+3` 内 RET）走 4 字节 `B` 路径（`HookTiny4`），且 `TinyHookable` 要逐条检查 `w[0..retAt]` 的 PC 相对形式（B/BL/BR/B.cond/CBZ/TBZ/ADRP/ADR/**BLR/LDR(literal)**）；⑤ 记「已成功挂钩入口表」而不是只警告重复 ⇒ 重复入口必须跳过（第二次 Dobby 会把我们的补丁字节当原序言重定位＝自毁）。真机判据：`install pass: ok=N dead=0/M` 与 `install skip: K 个槽位因「入口形态不安全」主动跳过`（N+K=M） |
| 87 | 用 `api.type_name()`（`il2cpp_type_get_name`）打一行「返回类型诊断」，打完顺手 `free(tn)`（直觉：il2cpp 返回 `char*`，该由调用方释放） | **整进程 SIGABRT**，且崩溃点看起来与那行诊断无关（4 秒后才 abort、栈里没有 free 那一帧）：霓虹深渊2 2026-10-12 r8 实测 `F/libc: Pointer tag for 0x7441496f90 was truncated, see …tagged-pointers` → `F/libc: Fatal signal 6 (SIGABRT), code -1 (SI_QUEUE) in tid <我方装钩线程>` → `F/DEBUG: Abort message: 'Pointer tag for … was truncated'`（scudo 分配器对「带 pointer tag / 非 malloc 内存」的堆调用直接 abort）；同时 `install pass: ok=N dead=0/M` **一行都没打出来** ⇒ 现象是「换新 so 就崩」，极易误判成 hook 挂错点或 THEMIS 反外挂 | ① **`api.type_name()` 的返回值绝不 `free()`**（任何 `il2cpp_*` 返回的 `char*` 同理：只读 + `printf`，不释放；`Unity/Il2CppApi.h` 里那句「需 free」以本条实测为准）；② 类型名只当**一次性诊断**：拿到结论就把整段删掉（本次拿到的是「`get_ProjectileRange` 返回 `Veewo.Stats.Stat` 对象而非数值」，见 #88）；③ 判据：Abort message 含 `Pointer tag … was truncated` 且崩溃 tid 是 MOD 自己的装钩线程 ⇒ 先查我们最近新增的堆调用（`free/realloc/strdup`），既不是游戏也不是反外挂 |
| 88 | 离线只查到「返回类型是个 Il2CppType 索引」（dump 里 `ret=44232`），就赌「AAPCS64 里 s0/d0/w0 同属 v0」去**伪造一个 getter 的返回值**（一份位型三种读法都对） | 该 getter 的真实返回类型是**类对象** `Veewo.Stats.Stat`（Veewo.Stat.dll 的属性包装对象，TYPE index=25553，字段 `baseValue`/`_cachedValue` + 属性 `get_Value`）⇒ 伪造出的数值被游戏当**对象指针**解引用 ⇒ 崩；这类错误离线完全看不出来（`ret=44232`＝对象 与 `ret=43335`＝Single 在 dump 里长得一样，只能靠真机打印真名） | ① 返回类型无法从 dump 判定时，**先在装钩那一刻打印真名（`method_ret_type` + `type_name`）并原样透传**，别在没见过真名的情况下伪造返回值；② 「返回 stat / 属性对象」的读点，正确姿势＝**本层只透传 + 记实例指针**，把强制点挂到该对象的**数值访问器**（如 `Veewo.Stats.Stat.get_Value`，argc=0 -> System.Single）上加 `thiz == 记住的实例` 的**身份闸** ⇒ 1:1 只改目标那一条 stat，不波及其它对象、不写字段与缓存（最坏只是不生效） |
| 89 | 往槽位表（`kSlots[]`）里加条目，但忘了同步抬高**上限常量**（`static const int kSlotsMax = 64;`） | 安装循环 `for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i)` 会在 `i == kSlotsMax` 处**静默截断**：末尾槽位既没有 `probe` 行、也没有 `install skip` 行、也不进 `dead` ⇒ 唯一的线索是三个数字对不上（霓虹深渊2 2026-10-12 r8b 实测 `install pass: ok=61 dead=0/66`，而 `probe … ptr=0x` 行只有 64 条主表 + 6 条登录槽 = 70 条，最后一个被尝试的槽位是 `Res_CrystalCount`，其后直接跳到 `install pass`）。表现＝「功能没效果」且**日志里查不到任何错误**（本次是 `Res_UseCrystal`＝`SpriteManager.UseCrystalSprite`、`Res_SpriteAtk`＝`NEONPlayerState.ConsumeSpriteAtkRaw` 两槽从未被尝试安装 ⇒ 无限水晶整条通道其实一个钩子都没挂） | ① 上限常量必须配一条 `static_assert(kSlotsCount <= kSlotsMax, "…")`，把「表比上限长」变成**编译期**错误（改完 96 即可）；② 真机判据：`install pass: ok=N dead=D/M` 里 **N + D + skip ≠ M** ⇒ 第一嫌疑就是循环上限与表长不一致，别先去查挂点/入口形态；③ 排查时先数日志行数（`grep -c "probe .* ptr=0x"`）与表长对比，比逐个槽位找日志快得多 |
| 90 | 给「按名字抬数值」型功能**猜**匹配键名（按 C# 字段名/直觉写 needle，如 `projectilerange`、`range`），以为名字对得上就能命中 | 真机上一局里 **0 次命中**，功能自然「无效果」，而日志、装机、挂点全绿 ⇒ 看起来像 hook 挂错点：霓虹深渊2 2026-10-12 r5–r8c 全走这条死路（`kFullRules` 三个 needle 里只有 `lifespan` 偶然命中，全场只打出一条 `fullscreen: finalbulletlifespanmult(needle=lifespan) 1.000 -> 60.000`，游戏内零观感变化）。真机 81 个实测键名（`/tmp/nhsy2/keys_observed.txt`，来自 `attri probe`）里**没有任何键含 `range`** —— 游戏的键名是 `finalbulletsizemult` / `finalbulletlifespanmult` / `finalbulletcount` / `maxspritecount` 这类**final* 前缀 + 单位后缀** | ① 这类功能的键名**必须以真机探针名单为准**：先在真机开一次功能跑一局，把 `attri probe`（或同类去重探针）打出的全名单存成文件，再据此定 needle —— 禁止凭字段名/直觉猜；② 探针必须**去重 + 上限**（本仓 ≤120 个键）否则刷屏，但**去重不影响判断命中**（命中计数照打）；③ 判据：日志里 **一条 `fullscreen: <key>(needle=…) x -> y` 都没有** ⇒ 键名错（不是 hook 错）；有该行但游戏无变化 ⇒ 见 #91 |
| 91 | 挂点/闸门**全部命中并强制成功**，但游戏里**毫无变化**，于是继续在同一挂点上调数值（改大改小、换返回值类型） | 该值根本不是游戏的**决策输入**，再怎么调都是死的：霓虹深渊2 2026-10-12 r8b 实测 `fullscreen range probe` 24 条 + `fullscreen stat force #N get_Value 0.000 -> 99999.0（射程 stat 实例闸命中）` 24 条（每发子弹一次、实例身份闸成立 ⇒ 挂点与闸门都对），用户 L3 实测「全屏攻击无效果」。用户原话（m04677）：「我尝试了全屏攻击和无限水晶，都是无效果……不会自己有胡乱散发思考没按照斌哥的走吧」 | ① 「命中但无变化」＝**换通道**，不是调数值：立刻回到**对照包（斌哥）的机制**上（本例＝`Attri::TryGetValue` 按名抬值，见 `10_docs/霓虹深渊2/斌哥改包抓点.md` §5/§8），别在自创挂点上迭代；② 把「已命中但无变化」的挂点记入台账**已否证**行（本仓 P12 的禁止重试清单），下轮不再投入（可保留代码但不作为方案）；③ 同一功能连续 3 轮无效 ⇒ 停手填假设矩阵（AGENTS.md P4），不要出第 4 个包 |
| 92 | 想知道「AutoDict 式按名读值」的**键名从哪来**（`Attri::TryGetValue(key)` 这类，键是运行时字符串），只能靠真机跑一局看探针，或干脆猜 | 键名通常就是某个 **`*ConstVariables` 类的字段名，小写后逐字相等**：霓虹深渊2 里 `NEON.Game.Managers.GameConstVariables`（TYPE index=10208，`dump_full.txt:131728`–`:132110`，**380 字段**）的字段名小写＝AutoDict 键名，真机实测 81 键（`keys_observed.txt`）**逐字可对**，连 `basemeleeorbitalradius`、`currentspriteattackcount` 这种只有玩到才请求的键都在；反过来 `comm -23 gcv_lc.txt obs_lc.txt` 能列出「存在但本局没被请求」的键（本仓据此找到几何族 `meleecheckdistance` / `finalmeleecheckdistance` / `damagesize` / `projectilerange` / `bulletsizemult` 等全屏攻击候选） | ① 找键**先翻 metadata dump 里的字段表**，别猜也别全靠真机跑：`grep -a "^ TYPE " -A N` 取那个 `*ConstVariables`/`*Constant`/`*Default` 类的字段块，小写导出成名单；② 用真机探针名单做**交集/差集**：交集＝确认的键，差集＝「存在但没走到」的候选（要造出触发条件才谈得上命中）；③ 名字形态可当分类线索：`*mult` 倍率、`*count` 计数、`checkdistance`/`range`/`size` 判定几何；④ 计数键可以安全抬高，**「保底/下限」键（如 `leastonespritecrystal`）禁止抬高**——内部若有不走该读点的「补到保底」循环会被拉成死循环/爆量生成 |
| 93 | 用「按名挂点」去挂一个 **8 字节 veneer** 方法（入口是 `mov wN,#imm ; b <共享实现>`，IL2CPP 里大量 getter 长这样），期望它像普通方法一样被挂上 | 该槽位每轮都被入口形态闸**主动跳过**（日志 `… 入口 +4 处即 b（函数体不足 16 字节）— 跳过该槽位`），而只盯着 `install pass: ok=N` 与「功能无效」⇒ 一路误判成「通道选错」，实际是这个 getter **从来没挂上过**：霓虹深渊2 `SpriteManager.GetCrystalSpriteCount`（水晶计数 getter，入口 `52800401 17FFF1C0`）从 r8c 到 r10 连续被跳过三轮，用户三轮都看到「需要更多水晶」 | ① veneer **不解开也能拦**：解 `b` 的符号扩展 imm26 得目标地址，直接在**目标地址**上装钩；② 处理函数必须与跳板**同声明足够多的参数（x0..x5）并原样转发**，否则未知参数被截断；③ 用「选择子」区分共享实现里的不同调用者（veneer 里 `mov w1,#imm` 的 imm 即选择子/偏移，寄存器比对 `a1 == imm` 才拦截）；④ 判据：`install skip` 明细里出现该槽位 + 功能无效 ⇒ **先查它到底挂上没有**，不要继续换通道 |
| 94 | 把「打一只怪只掉一只的血」这类**范围/几何型**需求，实现成「改数值」（射程/判定半径/子弹体积）或「猜配置键」 | 连续 3 轮改键名+数值，游戏内零观感变化（r5–r10 全败），用户直接判「自己找到的就有问题，从源头就错」 | ① 换成**行为广播**：在伤害链最后一跳（`HitPoints::ReallyDeductHP(DamageInfo, Single)`）把**同一个 DamageInfo + 同一个最终伤害值**用**原函数跳板**原样转发给其它同类目标 —— 不猜字段、不猜参数类型、不猜伤害公式（与玩家自己打那一下完全等价）；② 目标「类」从被命中对象的 `*(void**)thiz`（klass）**现场学到**，玩家自己的 `Hearts` 是另一个类 ⇒ 天然不自伤；③ 候选调用前复核「首字仍是同一 klass」再打（释放/复用即跳过），类名含 player/pet/follower/npc/hearts 的跳过；④ 同类实例用 `Api::ScanInstances(klass, list, MB)`（结果缓存 2 s、单次上限 ~48 目标、`thread_local` 重入闸）；⑤ 判据：`fullscreen bcast scan #N klass=… found=…` + `fullscreen bcast N targets=… damage=…` |
| 95 | 手写 AArch64 指令字段解码（`MOVZ Wd,#imm`）时把 **Rd 的位段写成 `bits[9:5]`** | 真机日志 `入口首条不是 mov w1,#imm（w0=0x52800401）— 跳过` —— `0x52800401` 正是 `mov w1,#0x20`；Rd 在 **bits[4:0]**，误写使 rd 恒为 0 ⇒ veneer 目标从未挂载（r11 首版装完功能依旧无效，日志里只有那一条 WARN） | `MOVZ/MOVN` 解码：`Rd = w & 0x1F`、`imm16 = (w>>5) & 0xFFFF`、`hw = (w>>21) & 3`；写完先用一条**已知指令**自检（`0x52800401` ⇒ rd=1、imm16=0x20）再上真机 |
| 86 | `push_main` 全通道失败、报 `error: failed to push some refs to 'github.com:Aliang369/nixiang.git'` | 两个原因**同形**、极易只往网络方向查：① 本仓 `core.hooksPath=04_scripts/_common/git-hooks` 的 **pre-push「机器关」** 拒绝推送，git 只吐一句 `failed to push some refs`（真正的 FAIL 清单在门禁输出里，不在 git 的错误里）；② 本机 GitHub 三件套同时坏：22 端口 `Operation timed out`、`ssh.github.com:443` 分钟级 `Connection refused/Timeout`（重试十来次会有一次通过，但推流阶段容易断）、`gh auth status` 报 `The token in keyring is invalid`（⇒ `push_main` 的 https-proxy/https-direct 两通道用 `gh auth token`，必然失败） | ① **先跑 `git push --dry-run origin <branch>` 看门禁输出**（`pre-push 本次 push 涉及以下 .sh, 跑机器关` → `汇总: PASS=4 FAIL=4`），FAIL 不修就别再重试网络；② macOS 上 git 自己的凭证（`git credential-osxkeychain get`，service=`github.com`）常与 `gh` 的 keyring token **不是同一条**，用 `curl -H "Authorization: token $TOK" -x $HTTPS_PROXY https://api.github.com/repos/<owner>/<repo>` 验 `"push": true`；可用则绕开 gh 直接顶：`git -c url."https://github.com/".insteadOf="git@github.com:" -c http.proxy=$HTTPS_PROXY -c credential.helper=osxkeychain push origin <branch>`（本机 `HTTPS_PROXY=http://127.0.0.1:7897` 实测 https://github.com 200 / 1.2s，直连 000） |


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
