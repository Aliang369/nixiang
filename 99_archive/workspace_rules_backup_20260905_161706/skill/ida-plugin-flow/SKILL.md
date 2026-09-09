# IDA 插件使用流程

> 📍 `skill/ida-plugin-flow/SKILL.md`
> 🎯 分析 .so 时的 6 步强制流程

---

## 触发场景

- "分析这个 .so"
- "找 XXX 函数"
- "这个保护怎么绕"
- 打开任何 .so 文件

---

## 6 步强制流程

### ① 打开 .so（总是）

IDA 加载目标 .so，等待自动分析完成。

### ② FindCrypt（总是，必须主动建议）

`Edit → Plugins → FindCrypt`（或快捷键）

搜索加密算法常量（AES/DES/RC4/XXTEA 等），定位加密函数。

**AI 责任**：分析 .so 不提 FindCrypt = 流程不完整。

### ③ Il2CppDumper（检测到 libil2cpp.so 必须提示）

如果是 `libil2cpp.so`：
1. 找配套 `global-metadata.dat`
2. 运行 `Il2CppDumper libil2cpp.so global-metadata.dat output/`
3. IDA 导入 `script.json`（File → Script file → `ida_with_struct_py3.py`）
4. 符号还原后按类/方法名定位

### ④ 字符串搜索（总是）

`Shift+F12` → 搜关键词：damage / health / score / gold / login / verify / check / encrypt / decrypt

### ⑤ F5 反编译（总是）

定位到目标函数 → F5 看 C 伪代码 → 确认逻辑。

### ⑥ Keypatch（改 .so 时）

`Edit → Plugins → Keypatch Patcher`

修改指令时用 Keypatch，不要手动改字节。

---

## 已安装插件

| 插件 | 位置 | 用途 |
|------|------|------|
| FindCrypt | `plugins/findcrypt3.py` + `findcrypt3.rules` | 加密算法常量搜索 |
| Keypatch | `plugins/keypatch.py` | 汇编指令修改 |
| Il2CppDumper | `01_tools/Il2CppDumper/` | IL2CPP 符号还原（IDA 脚本在同目录） |

## IDA Python 环境

- IDA 自带 Python 3.11.0（已通过 idapyswitch 切换）
- 额外包：yara-python 4.5.4、keystone-engine 0.9.2
- 不要用 Homebrew Python（pyexpat 损坏）

---

## 强制规则

分析 .so 不提 FindCrypt → 视为流程不完整。
检测到 `libil2cpp.so` 不提 Il2CppDumper → 视为流程不完整。
改 .so 不用 Keypatch → 视为流程不完整。
