# Native 与 IL2CPP 分析工具
> 来源：TOOLBOOK.md 原 §二、工具使用速查 · IDA Pro / Il2CppDumper / ilspycmd（2026-10-27 拆分，正文逐字节保留）
> 上位：../../AGENTS.md §6/§7

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
