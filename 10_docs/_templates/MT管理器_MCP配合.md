# MT 管理器 × Cursor MCP 配合（备忘）

> 官网：https://mt2.cn/guide/reverse/apk-mcp.html  
> 传输：**Streamable HTTP**  
> Cursor 配置：工作区 / 全局 `mcp.json` → `MT_APK_MCP`  
> **规则（全局 alwaysApply）**：`.cursor/rules/mt-apk-mcp.mdc`（仓内）+ `~/.cursor/rules/mt-apk-mcp.mdc`（用户全局）

## 标准使用流程

1. 手机 MT → 侧栏 → 工具 → **APK MCP** → 启动（可开悬浮球保活）  
2. 电脑：`adb forward tcp:8787 tcp:8787`（**forward**；断线重跑）  
3. Cursor → Settings → Tools & MCP → 刷新并**启用** `MT_APK_MCP`  
4. 提供 APK：点开「当前 APK/已装应用」信息页不关，或放入 MCP 操作目录  
5. 聊天示例：「请分析当前 APK：包名、入口、权限、关键字符串与 so 符号」  
6. 结论写入 `10_docs/{游戏名}/`；试改包在 MCP 操作目录，正式星光包仍走本仓 inject 脚本  

## 连接地址（手机 APK MCP 页复制）

| 场景 | 地址 |
|------|------|
| 电脑 Cursor + USB（推荐） | `http://127.0.0.1:8787/mcp`（先 `adb forward`） |
| 同局域网 Wi‑Fi | `http://192.168.10.34:8787/mcp`（IP 以手机页为准；若超时多半是只监听本机/防火墙） |
| 手机本机其它 App | `http://127.0.0.1:8787/mcp` |

```bash
# USB 推荐：手机 MT 已「启动」APK MCP 后
# 注意：电脑访问手机服务用 forward（不是 reverse）
adb forward tcp:8787 tcp:8787
# 自检（GET 会 405；POST initialize 应返回 MT APK MCP）
curl -m 3 -sS -X POST http://127.0.0.1:8787/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"probe","version":"1"}}}'
```

Cursor：`Settings → Tools & MCP` 刷新 `MT_APK_MCP`；聊天里启用该 MCP。  
若改用局域网，把 `.cursor/mcp.json` 的 `url` 改成手机页上的局域网地址。

## 能力边界

| 层 | MT 全功能（手机） | APK MCP（给 AI） |
|----|-------------------|------------------|
| 去签 / KillerApplication | ✅ | ❌ 不做运行时去签 |
| Dex/Smali/资源/布局/字符串 | ✅ | ✅ |
| so 静态（ELF/反汇编/CFG/补丁） | ✅ | ✅（有预算上限） |
| 重打包签名 | ✅ | ✅ `mt_apk_build` |
| 动态调试 / Frida / 堆字段 | ✅（部分） | ❌ |

## 把 APK 交给 AI（优先顺序）

1. **当前 APK 文件**：MT 点开 APK/已安装应用信息页不关 → 聊天说「当前 APK 文件」  
2. **MCP 操作目录**：设置里目录（如 `/storage/emulated/0/MT2/mcp`）→ 说文件名/包名  
3. 打包产物也落在操作目录

## 工具清单（官网更新，AI 自动调）

**打开**：`mt_apk_list_available_apks` / `mt_apk_open`  
**读搜**：`mt_apk_list` / `mt_apk_search` / `mt_apk_read_text` / `mt_apk_read_bytes` / `mt_apk_dex_outline_class` / `mt_apk_dex_xref` / `mt_apk_resource_read` / `mt_apk_resource_xref` / `mt_apk_continue`  
**so**：`mt_apk_native_inspect` / `mt_apk_native_read_items` / `mt_apk_native_map_address` / `mt_apk_native_xref` / `mt_apk_native_disassemble` / `mt_apk_native_function_cfg`  
**改包**：`mt_apk_edit_open` / `mt_apk_edit_text` / `mt_apk_edit_resource` / `mt_apk_patch_bytes` / `mt_apk_native_patch_instructions` / `mt_apk_native_patch_string` / `mt_apk_edit_check` / `mt_apk_build`  
**清理**：`mt_apk_close`（仅临时工作区）

不支持：Java 反编译、arsc 新增词条、so 动态调试/完整伪代码。

## 工作区模型（简记）

- 打开 APK → 只读工作区（可复用）  
- 修改 → 编辑会话（隔离，不污染源包）  
- `mt_apk_build` 出新包到操作目录；可选按 MCP 设置签名

## 与星光工程分工

- **静态读改 / 快速试 patch / 重打包**：MT MCP  
- **运行时 hook、卡密、注入正式包**：星光 / Frida / 本仓脚本  
- 音舞：`KillerApplication193` = MT 去签产物；星光侧用 `SignSpoof`（killPM 同款）
