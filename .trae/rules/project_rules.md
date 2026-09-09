---
name: nixiang-workspace-rules
description: 安卓游戏逆向工作区硬纪律：机械操作管线、preflight 机器关、禁止清单
alwaysApply: true
---

# 工作区硬纪律（TRAE 会话每次生效）

> 正文源头（勿在此复制全文，规则变更改源头不改本文件）：
> 1. `/Users/aliang/Documents/nixiang/AGENTS.md` —— 工作区唯一规则入口（路由/授权/审查门禁）
> 2. `/Users/aliang/Documents/nixiang/04_scripts/_common/stages.md` —— 机械操作管线（操作二分 + 阶段表）
> 3. `/Users/aliang/Documents/nixiang/04_scripts/_common/weak-ai-system-prompt.md` —— 会话纪律全文

## 立即生效的五条硬纪律

1. **机械操作（拆包/打包/注入/移植/签名/装机/清理）只许走管线**：
   `bash 04_scripts/pipeline.sh <游戏> status / next / verify <阶段> / reset <阶段>`
   禁止直接敲 adb / apktool / zip / ndk-build / jarsigner 等命令序列
2. **改完任何文件必跑机器关**：`bash 04_scripts/_common/preflight.sh <改过的文件>`
   有 FAIL 必须修复；C++/mk 改动必须重跑管线构建阶段才算完成
3. **管线报错**：读 `04_scripts/<游戏>/pipeline.log` 尾部，修复原因后重跑；禁止换命令绕过
4. **禁改清单**：`pipeline.sh` / `pipeline.lock` / `skill/` 下任何文件 / `Main.cpp` / `GameConfig.h` / 公共 `Android.mk` / `02_apks/` 与 `08_output/` 下产物
5. **分析操作（逆向/找锚点/读代码）照常推理**，产出写 `10_docs/{游戏}/`，注明证据来源；同一错误连续两次没头绪停下问用户
