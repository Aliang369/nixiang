# IDA 插件使用流程

> 路径：`skill/ida-plugin-flow/SKILL.md`
> 分析 .so 时按问题选用的 6 步索引，不是强制全量扫描。

---

## 触发场景

- 分析 .so、定位函数或核对调用契约时按需查阅。
- 仅打开文件不触发全套工具；先明确线索、候选和所需区分证据。

---

## 6 步流程（按需选用，保留步骤编号）

### ① 打开 .so（需要 IDA 分析时）

先记录 ELF 身份、架构及来源，再加载并检查分析状态。已有证据足够时不强制打开 IDA；可选程序头、符号表、反汇编或运行日志等最小检查。

### ② FindCrypt（有加密定位问题时）

`Edit → Plugins → FindCrypt` 可搜索算法常量。只有需要用该线索区分候选时才扫描，不要求分析每个 .so 都建议运行。常量命中不能证明算法被执行，未命中也不能证明没有加密；继续核对引用和数据流。

### ③ Il2CppDumper（需要且输入适配时）

若需要 IL2CPP 符号/元数据关联，先核对配套 ELF、`global-metadata.dat` 和工具版本。
1. 输入适配时可运行 `Il2CppDumper libil2cpp.so global-metadata.dat output/`。
2. 按所用版本文档将生成脚本和数据导入 IDA。
3. 结果是定位线索；地址坐标、签名及运行时入口仍需核对。

库名称不触发强制运行；失败先区分输入不匹配、格式版本和工具限制，不立即定性为加密或保护。

### ④ 字符串搜索（关键词能区分候选时）

`Shift+F12` 搜任务相关关键词并检查交叉引用。无关的全量字符串扫描不作为完成流程的指标。

### ⑤ F5 反编译（需要恢复逻辑时）

伪代码用于提出解释；关键类型、ABI、隐含参数与控制流回到指令和调用点核对，不把反编译猜测当证明。

### ⑥ Keypatch（已有授权且确需指令修改时）

`Edit → Plugins → Keypatch Patcher` 是可选汇编辅助工具，不强制品牌，也不在本流程新增修改或规避操作。工具选择不替代改动范围、地址身份与结果验证。

---

## 已安装插件

以下为已有环境记录，使用前按需确认可用性，不据此强制安装或扫描。

| 插件 | 位置 | 用途 |
|------|------|------|
| FindCrypt | `plugins/findcrypt3.py` + `findcrypt3.rules` | 加密算法常量线索 |
| Keypatch | `plugins/keypatch.py` | 汇编指令编辑辅助 |
| Il2CppDumper | `01_tools/Il2CppDumper/` | IL2CPP 符号/元数据关联 |
| ida-pro-mcp 2.0.0 | IDA 内 MCP 插件，HTTP `127.0.0.1:13337` | MCP 工具化分析（survey/xref/decompile 等 ~66 工具）；用法速查见 [references/ida-mcp-cheatsheet.md](references/ida-mcp-cheatsheet.md)（2026-09-10 自外部 reverse-skill 库择录，Windows 包装脚本不适用） |

## IDA Python 环境

- 历史环境：IDA Python 3.11.0；yara-python 4.5.4、keystone-engine 0.9.2。
- 曾有 Homebrew Python 的 pyexpat 故障；按当前实际解释器与依赖错误确认，不能推广为所有 Homebrew Python 不可用。

---

## 强制规则

- 此节保留旧链接；不再以是否运行 FindCrypt、Il2CppDumper 或 Keypatch 判断流程完整。
- 地址技术引用 `../re-implement/hook-rules.md` §2、§6–7：区分 file offset、ELF VA、load bias、IDA EA；定义匹配时允许 base+RVA。
- ABI、生命周期、并发规则引用同文件 §3、§9、§11、§14、§18、§20；工具输出不替代这些契约。
- 结论采用 CLAIM + 适用范围 + 证据 + VERDICT；分阶段门禁见 `../re-implement/code-review-checklist.md`。按问题选最小可区分实验，通用政策见 `../../AGENTS.md`。
