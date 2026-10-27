# PLAYBOOK.md — 新游戏接手标准流程

> 📍 `PLAYBOOK.md`（相对仓库根）；🎯 AI 接手**新游戏/新版本**时读这个，日常操作读 `WORKFLOW.md`，工具用法查 `TOOLBOOK.md`（短索引，正文在 `10_docs/tools/`）。
> 读取、授权、协作与通用政策统一见 `AGENTS.md`；本书按任务查阅，不另设读取顺序。

---

## 0. 通用政策入口与案例

通用政策统一见 `AGENTS.md`；构建细节见 `WORKFLOW.md` §4，Hook 细节见 `skill/re-implement/hook-rules.md`。

- 对照改包是情报而非产品：借用玩法语义，落地用星光框架。
- 官方 dex 完整性约束的历史案例：终末地 1.5.3 的 ACE/TSS 实验；适用范围须按目标版本验证，参考 `skill/re-analyze/login/` 与 `10_docs/终末地/ACE过检复盘与SOP_20260903.md`。

---

## 1. 流程总览

```
A. 拉包与初判 → B. 保护面探测 → C. 玩法抓点 → D. 星光落地 → E. 验证收尾
```

阶段产出用于跟踪证据，可按问题往返补证；未完成项明确记录，不把阶段顺序当作禁止低成本验证的门禁。

---

## 2. 阶段 A：拉包与初判

### A1. 拉包

```bash
# 真机已装
adb shell pm path <包名> && adb pull <path>

# 文件放入
02_apks/{游戏名}/{游戏名}_{版本}_{来源}.apk   # 来源 = official / binge_mod / from_device
```

### A2. 静态初判

```bash
# jadx 反编译（只读不改）
jadx -d 03_decompiled/{游戏名}/jadx_{版本}/ 02_apks/{游戏名}/{游戏名}_{版本}.apk

# smali/资源结构 → 优先 MT MCP，不对游戏包跑 apktool d
```

### A3. 引擎识别

| 引擎 | 标志文件 | 后续主路径 |
|------|---------|-----------|
| Unity IL2CPP | `libil2cpp.so` + `global-metadata.dat` | Dobby hook / HybridCLR invoke 跳板 |
| Unity Mono | `assets/bin/Data/Managed/Assembly-CSharp.dll` | ilspycmd 反编译 / 运行时 hook |
| Cocos JS | `libcocos2djs.so` + `*.jsc` / `main.js` | `se::ScriptEngine::evalString` splice |
| Cocos C++ | `libcocos2dcpp.so` + `*.jsc` | Keypatch / Dobby hook |
| 自研 | 无上述标志 | IDA + 字符串搜索，逐案分析 |

### A4. 产出：初判文档

`10_docs/{游戏名}/{游戏名}_改包初判_{日期}.md`，至少包含：包名、引擎、壳/保护、证书、登录方式、native 库清单、菜单形态（若有对照包）。

---

## 3. 阶段 B：保护面探测

**这是碰壁最多的环节。** 壳和保护决定了能改什么、不能改什么，判断错了后面全白做。

### B1. 保护组件识别

| 组件 | 检测标志 | 影响 |
|------|---------|------|
| **腾讯 TSS/ACE** | `libtersafe2.so` / `libtprt.so` / `com.tencent.tp.*` / `AceApplication` | dex + arsc 可能受保护，必须外科组装 |
| **百度加固** | `libbaiduprotect.so` / `StubApplication` | Application 被替换，dex 运行时解密 |
| **RootBeer** | `libtoolChecker.so` / `checkForRoot` | 需过 root 检测 |
| **完整性黑名单** | 热更代码中搜 `非法`/`完整性`/`classes4`/`battlehook`/`overlay` | 体积阈值 + 路径双条件 |
| **签名校验** | `getSign` / `TapSignKit` / `PackageManager` | 改签后登录挂 |

### B2. 受保护/可动文件清单（必须实测）

**方法**：A/B 测试——只对与当前问题相关的文件做最小改动，在已授权测试环境对照验证；默认覆盖安装，干净安装另按 `AGENTS.md` 确认。

| 文件 | 通常状态 | 测试方法 |
|------|---------|---------|
| `classes*.dex` | 取决于保护 | 逐个改一字节 |
| `resources.arsc` | TSS 游戏通常受保护 | 尾部追加 40 字节（注意 header.size 自洽 + STORED） |
| 新增 `classes{N+1}.dex` | 通常可 | 注入空 dex |
| 新增 `.so` | 通常可 | 注入测试 so |
| `assets/` | 通常可 | 加测试文件 |
| `AndroidManifest.xml` | 通常可改 | MT 改 manifest |

**ART 连续 dex 规则**：注入必须落在已有最高连续 dex 的下一号。有 `classes5` 无 `classes4` 时不能跳到 `classes6`。

**TSS 双校验路径**（公主连结实证）：
1. Java 路径：`AceApplication.attachBaseContext` → native `initialize` → 读 APK 校验
2. Native 路径：`libtprt.so` 被 `libil2cpp.so`/`libunity.so` DT_NEEDED 依赖，随引擎加载自动触发
→ 换 Application 也绕不过第二条

### B3. 产出：保护面清单

明确写出：受保护文件列表、可动文件列表、校验方、校验路径、登录方式、过签方案。

---

## 4. 阶段 C：玩法抓点

**这是分析最耗时的环节。** 找到改哪里、改什么值、怎么过滤。

### C1. 两条路径

| 路径 | 前提 | 效率 |
|------|------|------|
| **对照包抓点** | 有斌哥改包 | 高（差分直接出结果） |
| **从零抓点** | 无对照 | 低（逐步探测） |

### C2. 对照包抓点（优先）

```
1. 基线快照：dump shared_prefs + 菜单功能名（**以用户提供的截图/口述为准**；AI 不自行 `screencap`，见 `AGENTS.md` §4「真机交互零截图」）
2. 逐项 ON → dump shared_prefs → 差分 → 功能名 ↔ prefs key ↔ value
3. 全开 → BSS dump（dump_binge_shell_mem.sh）→ 提取：
   - Cocos JS：evalString 拼接的明文 JS payload
   - Unity IL2CPP：被 hook 的方法名 / BSS 旗标地址
   - 远程菜单 URL + game_id
4. 交叉验证：prefs ↔ BSS 旗标 ↔ JS/方法名 三方对齐
```

### C3. 从零抓点

**Unity IL2CPP**：

```
1. Il2CppDumper（libil2cpp.so + global-metadata.dat → dump.cs + script.json）
2. IDA 导入 script.json 还原符号
3. 仅需定位加密算法/资源解密路径时运行 FindCrypt，已有充分证据可跳过并记录依据
4. 字符串搜索：damage / hp / coin / skill / cd / attack / defense
5. 定位候选 → 运行时 hook 验证
6. file_off → RVA 转换（注意 ELF bias，通常 +0x4000）
```

**Cocos JS**：

```
1. dlsym 查找 se::ScriptEngine::evalString（Dobby 钩点）
2. Hook evalString → 打印传入的 JS 脚本字符串
3. 分析 JS：哪些函数被替换、返回值改了什么
4. prefs ↔ get_ret(n) ↔ JS 段落映射
```

### C4. 通用规则（碰壁总结）

| 规则 | 原因 |
|------|------|
| 禁止无过滤钩共用入口 | HybridCLR 共用解释器入口无过滤 → 断连服 |
| 禁止堆扫托管对象 | Unity GC 移动对象，扫堆必崩 |
| hook 必须调原函数 | 不调 orig → 参数被当指针 → SIGSEGV |
| 只动特定方法的特定字段 | 整类写字段 → 结算崩溃 |
| 钩"出结果的那一跳"，不钩"看起来像开关"的字段 | 托管字段写了局内可能无效 |
| 偏移必须来自实测，不猜测 | RVA/字段 offset 换包必变 |
| file_off ≠ RVA，注意 ELF bias | 通常 `RVA = file_off + 0x4000` |

### C5. 产出：抓点定稿

`10_docs/{游戏名}/抓点定稿_{日期}.md`：功能总表（UI ↔ prefs ↔ get_ret ↔ 语义）、挂点 RVA、字段偏移、敌我判定、星光配方。

---

## 5. 阶段 D：星光落地

### D1. 切游戏

```makefile
# Android.mk 只改这一处
LOCAL_CFLAGS += -DENABLE_UNITY_HOOK -DSTARRY_GAME_{GAME}
# game_id 由 GameConfig.h 自动映射，禁止在 mk 写 STARRY_AUTH_GAME_ID
```

### D2. 构建路径

| 改了什么 | 用什么 |
|----------|--------|
| 只改 `Games/*Mod.cpp` | `*_so_only.sh` |
| 首包 / 改菜单 / smali / Manifest | `inject_*.sh` 或外科组装 |
| TSS/ACE 游戏 | **必须外科组装** |
| 快速试改 smali | MT MCP |

**外科组装核心**：官方受保护文件字节级保留，只增不改。

### D3. 过检通用清单

| 项 | 方法 |
|----|------|
| 签名校验 | `SignSpoof`（运行时 hook `PackageInfo.CREATOR`，按包名回灌官方 DER；新游戏只需提取官方证书 DER/base64 加一条表项） |
| TapTap 登录 | 改签后 SSO 必挂 → `TapLoginForceWeb`：`onActivityPreCreated` 反射把 `LoginActivityImpl.loginHandlers` 数组中的 `ClientLoginHandler` 换成 `CloudLoginHandler` → 全 canHandle=false → SDK 自带 `fallbackToWeb` 内嵌网页。**禁止 dex patch**（ACE 校验官方 dex，实证闪退） |
| 完整性黑名单 | 搜热更代码中的黑名单路径 + 体积阈值，确保黑名单路径不存在 |
| Root | RootBeer / KSU 隐藏 |

### D4. 设备指纹

```bash
# 仅在标识恢复的具体范围已授权且恢复前置满足时使用；见 TOOLBOOK「真机残留清理」
bash 04_scripts/_common/ensure_starry_device_id_seed.sh
```

改指纹相关代码**必须重打含菜单 DEX 的包**，只换 so 无效。

---

## 6. 阶段 E：验证收尾

### E1. 验证

```bash
adb logcat -s StarryTrace AuthClient DeviceFingerprint | grep -E \
  'auth game_id|native menu shown|source=shared'
```

逐项：菜单弹出、每个功能有效、关闭后恢复、登录正常、对局结束不闪退。

### E2. 收尾

按 [《目录结构脚本环境与交付清理》](10_docs/tools/05_目录结构脚本环境与交付清理.md)「清理规则」「真机残留清理」执行保留、隔离重建与授权流程，填写 [游戏收尾与最小复现清单](10_docs/_templates/游戏收尾与最小复现清单.md)。现有设备清理脚本未改，不默认运行；未处理项如实记录。

### E3. 产出：功能复用手册

`10_docs/{游戏名}/功能复用手册_{版本}_定稿_{日期}.md`——游戏大更后的复现入口，不替代保留的源码、脚本与必要输入。

---

## 7. 游戏更新后的复用

```
[ ] 拉新包
[ ] 读功能复用手册 → 引擎/保护是否变化
[ ] 引擎没变 → 重新 dump → 按类/方法名重算 RVA → 更新 *Mod.cpp
[ ] 保护变了 → 重新做保护面探测
[ ] 核对 cert MD5 / 黑名单路径 / 登录方式
[ ] 构建 → 验证 → 更新手册
[ ] 按 TOOLBOOK「真机残留清理」记录本次变更、恢复前置与待处理项
```

---

## 8. 异常处理（通用）

### 闪退

| 信号 | 原因 | 方向 |
|------|------|------|
| SIGBUS/SIGSEGV（1-2s 内） | 完整性校验（TSS/ACE） | 是否改了受保护文件？走外科组装 |
| SIGBUS/SIGSEGV（对局中） | hook 写错/溢出 | 检查偏移、字段类型、是否调 orig |
| ClassNotFoundException | dex 注入问题 | dex 序号是否连续？ProGuard 改了类名？ |
| 登录弹窗/踢出 | 签名/完整性 | 查 cert MD5、查黑名单路径、查 TapTap SSO |

### 功能无效

```
1. log 是否命中目标函数？
   - 命中但无效果 → 偏移错了（重新抓点）
   - 未命中 → RVA 变了（重新 dump）
2. 菜单开关是否到 native？→ 查 prefs key / get_ret 映射
3. 是否被"看起来像开关"的字段骗了？→ 改钩"出结果的那一跳"
```

### 服务端校验

客户端硬刚服务端重放性价比极低。合理方向：纯本地模式、非战斗类、关外挂后正常打。

---

## 9. AI 行为规范

### 9.1 读取与协作入口

统一见 `AGENTS.md`；本节仅保留证据判断的例子，不另设通用政策。历史经验按目标游戏/版本核验。相关入口：`skill/SKILL.md` 按需索引、`10_docs/_state/current.md` 状态索引。

### 9.2 数据先于行动（最重要的规则，必须完整遵守）

AI 在逆向中最致命的失败模式，按发生频率排序：

1. **数据不够就猜** — 还没 dump、没抓包、没反编译，就开始「应该是这样」
2. **猜错当事实** — 把猜测当结论，基于猜测继续往下推理，越走越远
3. **一个方向死磕不回头** — 试了一个方向不对，不换方向也不问人，继续钻
4. **没等用户就自己继续** — 说了「需要你进游戏抓点」，没等回复就自己假设结果往下做

这些是历史复盘中的常见失败模式；以下例子辅助执行 `AGENTS.md` 的证据政策，不代表统计结论。

---

#### 示例 1：证据驱动的迭代

收集数据、提出判断、验证可循环迭代；可逆实验本身也是收集证据，授权范围内无需逐次询问。

| 阶段 | 该做什么 | 不该做什么 |
|------|---------|------------|
| **收集** | dump、抓包、反编译、strings、Il2CppDumper、BSS 提取 | 还没看数据就想「应该是这样」 |
| **判断** | 基于收集到的证据做结论 | 基于猜测做结论 |
| **行动** | 按结论写代码/改偏移/构建 | 把待验证假设当作已证实结论交付 |

---

#### 规则 2：禁止猜测，必须标注不确定性

任何未经实测验证的判断，必须标注「⚠️ 待验证」，不能当事实用。

| ❌ 错误 | ✅ 正确 |
|---------|---------|
| 「这个偏移应该是 0x20」 | 「dump.cs 显示 field offset=0x20」或「不确定，需运行时验证」 |
| 「这个函数应该是处理伤害的」 | 「strings 搜 damage 命中这个函数，需 hook 确认」 |
| 「这个游戏应该是 Unity IL2CPP」 | 「有 libil2cpp.so + global-metadata.dat，确认是 Unity IL2CPP」 |
| 假设了一个值然后基于假设继续推理 | 假设必须标注「⚠️ 待验证」，不能当事实用 |
| 「这个保护机制大概是 ACE」 | 「检测到 libtersafe2.so + AceApplication，推断是 ACE，需装机验证」 |

**关键区分：推理 ≠ 猜测**
- 推理 = 有证据链（A 存在 + B 存在 → 推断 C）→ ✅ 允许，标注「推断」
- 猜测 = 无证据，凭经验/直觉硬填 → ❌ 禁止

---

#### 规则 3：数据不足时必须做以下之一（不许跳过）

1. **补数据** — 说明缺什么数据，给出获取方法
2. **问用户** — 把已知的和未知的列出来，问用户知不知道
3. **停下来** — 说「当前数据不足以判断，需要先 XX」

**绝对不许**：数据不够 → 猜一个 → 当真 → 继续往下做 → 全盘皆错

典型场景：
- 偏移不确定且有多个候选 → 设计能区分候选的低成本验证，不让用户代猜技术事实
- .so 太大，分析陷入细节 → 按已知目标收窄范围；只有目标确实缺失才询问用户
- 文档和实际行为不一致 → 不要自己圆逻辑，把矛盾告诉用户

---

#### 规则 4：用户协作

遵循 `AGENTS.md` §4「授权、安全边界与用户操作纪律」（旧称「授权与测试边界」）：需要用户进游戏/开菜单时在聊天提出请求，暂停依赖该结果的步骤；收到回复后核对实际数据。用户说「好了」不等于证据已采集成功，独立且已授权任务不必一并停工。

---

#### 规则 5：每个结论必须有来源

AI 给出的每个事实性判断，必须能回答：「这个结论从哪来的？」

| 来源 | 可信度 | 例子 |
|------|--------|------|
| 工具输出/实测 | ✅ 高 | 「Il2CppDumper 输出 RVA=0x170C2B8」 |
| 文档记录 | 历史证据，核对版本与来源 | 「抓点定稿写了 prefs key='0'」 |
| 用户告知 | ✅ 高 | 「用户说菜单有四个功能」 |
| 推理（有证据链） | ⚠️ 中 | 「有 libtersafe2 + AceApplication → TSS/ACE 保护」 |
| 猜测（无证据） | ❌ 禁止 | 「这个偏移应该是 0x20」 |

**如果被问到「这个结论从哪来的？」答不上来 → 就是猜测，必须收回。**

---

#### 规则 6：行动前自查清单（动手前过一遍）

写代码 / 改偏移 / 构建之前，逐条确认：

- [ ] 我要改的偏移/RVA/函数名，有实测或文档依据吗？（没有 → 补数据或问用户）
- [ ] 有没有「待验证」的假设被当成了事实？（有 → 先验证）
- [ ] 需要用户配合的操作，用户确认做完且我核对了数据吗？（没确认 → 停下等）
- [ ] 我的结论能说出「从哪来的」吗？（说不出 → 那是猜）
- [ ] 下一步能区分哪些假设、带来什么新增信息？无信息增益时按 AGENTS.md 调整路线或报告卡点
- [ ] 新建/改动 game_id、template_id 前读过 `skill/starry-mod/SKILL.md`「game_id 命名规则」吗？（全名拼音首字母、不缩短；**存量 id 用户拍板保留的一律不改**）；Cursor 用户另见 `.cursor/rules/starry-mod-game-id.mdc` 指针
- [ ] 用户说「别动/已完成」的游戏，不再改它的任何产物（含 id、文档、包）；经验先作为候选记录，获明确授权后才改相应全局规范


### 9.3 授权与记录

授权、安装、清理、状态记录和经验审批统一见 `AGENTS.md`；技术禁区查 `skill/re-implement/hook-rules.md`。仅在游戏执行状态改变时更新游戏记录与索引，纯评估不改游戏状态。

### 9.4 信息增益预算（操作例子）

预算与求助条件统一见 `AGENTS.md`，不按方法数、调用数、步骤数或时长机械停工。

| 现象 | 下一步 |
|------|--------|
| RVA 未命中 | 核对版本、地址换算与命中证据，选择能区分原因的实验，不盲试相邻偏移 |
| 多次检索重复相同信息 | 收窄问题或换证据来源，记录已排除项 |
| .so 调用链过深 | 回到目标函数/现象，评估继续深入是否能回答问题 |
| 文档和运行矛盾 | 保留两侧证据并验证适用版本，不自行圆逻辑 |
| 需要用户操作或新授权 | 汇报已知、未知、建议方案与影响，等待相关步骤确认 |

完成阶段或预算接近耗尽时汇报结论和下一步；授权内可逆、低成本且有信息增益的探索直接继续，技术分叉优先用证据解决。
