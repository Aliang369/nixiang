# 04_scripts — 自写脚本
Frida hook 脚本（.js）、Python 解密脚本（.py）、Shell 自动化脚本（.sh）。

- 游戏相关：`{游戏名}/功能_目标.js`（如 `动物餐厅/hook_xxtea.js`）
- 跨游戏通用：`_common/`（拆包 / Manifest / 重打包签名）
- **机械操作管线（2026-09-09 起）**：`pipeline.sh` 统一入口 + `{游戏}/pipeline.json` 状态机

## 机械操作管线（弱 AI 唯一合法入口）

```bash
bash 04_scripts/pipeline.sh <游戏> status          # 开工第一步：看当前在哪
bash 04_scripts/pipeline.sh <游戏> next            # 执行下一阶段（自动路由）
bash 04_scripts/pipeline.sh <游戏> run <stage_id>   # 显式重跑某阶段
bash 04_scripts/pipeline.sh <游戏> verify <stage_id># 只校验产物（含指纹比对）
bash 04_scripts/pipeline.sh <游戏> reset <stage_id># 状态回退 pending
```

规则（详见 [`_common/stages.md`](_common/stages.md)）：
- 机械操作（拆/打/注/移/签/装/清理）禁止直接敲 adb/apktool/zip/ndk-build——命令全部收进管线，AI 只填"游戏+阶段"
- `pipeline.sh` / `pipeline.lock` 禁止修改（启动即 sha256 自检，被改动拒绝执行）
- 合法参数改 `{游戏}/pipeline.json` 的 `params`；阶段失败即停（读日志修原因，禁止换命令绕过）
- 每阶段完成自动记录产物指纹——改错文件会立刻在 verify/下一阶段暴露

## preflight 机器关（改完任何文件必跑）

```bash
bash 04_scripts/_common/preflight.sh <改过的文件...>
```

按扩展名自动检查：`.sh`→bash -n、`.py`→compile、`.js`→node --check、`.json`→json.tool；
`.cpp/.mk` 提示必须重跑管线构建阶段验证。多符号/少括号/尾逗号等弱模型高频错误在装机前抓。
弱 AI 会话纪律模板：[`_common/weak-ai-system-prompt.md`](_common/weak-ai-system-prompt.md)（贴到 Cursor 等工具 Rules）。

## 旧通用工具（继续可用，逐步并入管线后端）

```bash
./_common/decode_apk.sh <游戏名> <apk路径> [版本]
./_common/manifest_summary.sh <AndroidManifest.xml>
./_common/rebuild_sign_install.sh <游戏名> <07_patched目录> [标签] [--install]
```

环境变量与 SDK 路径解析见 `_common/env.sh`。
