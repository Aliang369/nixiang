# 功能实现总入口

> 📍 路径：`re-implement/SKILL.md`
> 🎯 触发场景：拿到偏移/RVA/方法名后，需要选择正确的 Hook 模式来实现功能

---

## 决策流程

拿到偏移后，**按 Hook 模式选择实现方式**，不是按功能分类。同一个 Hook 模式可以承载多种功能（如 ChangeHP 既做秒杀又做无敌）。

### 第一步：判断引擎类型

| 特征 | 引擎 | 实现路径 |
|------|------|----------|
| `libcocos2djs.so` + `.jsc` | Cocos Creator JS | evalString splice（见 `utility-pattern.md`） |
| `libil2cpp.so` + `global-metadata.dat` | Unity IL2CPP (AOT) | Dobby hook RVA |
| `libil2cpp.so` + HybridCLR | Unity IL2CPP (热更) | 公共 invoke 跳板 / 运行时按名解析 |
| `libxlua.so` | Unity + xLua | LuaEnv.DoString monkey-patch |

### 第二步：阅读 hook-rules.md（铁律）

**在任何功能实现之前，必须先读 `hook-rules.md`。** 这是所有 Hook 的基础规则，违反任何一条都会导致 SIGSEGV 或功能不生效。

**同时必读标准框架（写 Games/*Mod.cpp 前）：**
- 铁律全文：`mod菜单/starry-mod-menu/docs/GAME_MOD_GUIDE.md`
- 标准骨架（v2，防闪退默认实现）：`mod菜单/starry-mod-menu/app/src/main/jni/Games/_TemplateMod.h/.cpp`
- 正面范例：`mod菜单/starry-mod-menu/app/src/main/jni/Games/Archero2Mod.cpp`（按名 hook）、`Games/WhrhxMod.cpp`（RVA 双路径）
- 反面教材：`mod菜单/starry-mod-menu/app/src/main/jni/Games/HzsjMod.cpp`（SafeProbe 热路径 + 共享跳板，闪退/掉帧教训）

新游戏 cpp 一律用生成器产出：`bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}`，禁止从旧游戏（尤其 Hzsj）复制。存量旧 Mod 不回改，但新增 hook 必须沿用模板安全助手与懒装模式。

### 第三步：按 Hook 模式选择子文件

| Hook 模式 | 子文件 | 典型功能 |
|-----------|--------|----------|
| ChangeHP 拦截 | `damage-pattern.md` | 秒杀、无敌、全屏攻击 |
| DeductHP 拦截 | `damage-pattern.md` | 秒杀、无敌（霓虹深渊类） |
| getAtk 放大/归零 | `damage-pattern.md` | 秒杀、无敌（Cocos JS 类） |
| 全屏攻击扩散 | `damage-pattern.md` | 一次攻击全屏受伤 |
| 货币 XOR 加密 | `economy-pattern.md` | 金币/钻石修改 |
| 免支付 Cocos JS | `economy-pattern.md` | 内购免费 |
| 免支付 Unity | `economy-pattern.md` | 内购免费 |
| 免广告 | `economy-pattern.md` | 广告奖励直接发放 |
| 角色解锁 dic 直写 | `unlock-pattern.md` | 全角色解锁 |
| 零 CD / 无限技能 | `utility-pattern.md` | 技能无冷却 |
| 加速 | `utility-pattern.md` | 游戏倍速 |
| 挂机菜单修复 | `utility-pattern.md` | 后台返回菜单消失 |
| 服务端重放校验 | `anti-cheat-pattern.md` | 结算失败应对 |
| JS 反作弊屏蔽 | `anti-cheat-pattern.md` | 客户端上报屏蔽 |
| 通关校验旁路 | `anti-cheat-pattern.md` | BuildPassCheckInfo |
| 封号风险评估 | `anti-ban-analysis.md` | 写功能前评估量级安全线 |
| 代码审查 | `code-review-checklist.md` | 改完代码走审查边界①→④ |

### 第四步：实现

1. 按 `hook-rules.md` 规则编写 Hook 代码
2. 参考对应子文件中的模式和坑/弯路
3. 敌我判定参考 `damage-pattern.md` 中的通用法
4. 游戏更新后功能失效（`hits=0`）→ 按 [`version-update-relocate.md`](version-update-relocate.md) 重定位，禁止沿用旧 offset 或叠猜

### 第五步：封号风险评估（功能写完后、装机前）

**必须读 `anti-ban-analysis.md`，逐条评估本功能的封号风险。**

1. 本功能是否触发服务端校验？
2. 量级是否在安全线内？
3. 上报路径是否已屏蔽？

### 第六步：代码审查（走审查边界，装机前④须 PROVEN）

**改完代码必走 `code-review-checklist.md`「审查边界」①→④（①改动范围 → ②四条闪退红线 → ③按改动类型查维度 → ④证据门禁）。停止条件：一轮四步输出结论即停；❌ 只修 ❌ 说的问题；证据不足标 NOT PROVEN 并问用户。**

五维展开按③命中的改动类型查，不是每次都全量见下节：
1. 稳定性（防闪退）：内存安全、数值安全、线程安全、对局生命周期
2. 性能（防掉帧）：调用频率、I/O 开销、日志控制
3. 功能有效性（防突然失效）：偏移可靠性、hook 挂载、开关同步、边界条件
4. 反作弊（防封号）：上报检测、数值合理性
5. 审查结果记录到文档

### 第七步：实测验证

**所有审查通过后才能装机测试。**

1. **Log 验证**：`adb logcat` 确认 Hook 安装成功、功能触发日志正确
2. **实测验证**：在游戏中实际操作，确认功能生效且不崩溃
3. **副作用检查**：
   - 通关是否报"数据异常"（反作弊）
   - 结算是否失败（服务端校验）
   - 其他功能是否受影响
   - 长时间运行是否稳定（≥5 局）

## 子文件索引

- [`hook-rules.md`](hook-rules.md) — Hook 通用铁律（**必读**）
- [`version-update-relocate.md`](version-update-relocate.md) — 版本更新重定位方法论（小版本失效 `hits=0` 时照做）
- [`damage-pattern.md`](damage-pattern.md) — 伤害修改模式
- [`economy-pattern.md`](economy-pattern.md) — 经济修改模式
- [`unlock-pattern.md`](unlock-pattern.md) — 解锁修改模式
- [`utility-pattern.md`](utility-pattern.md) — 工具模式
- [`anti-cheat-pattern.md`](anti-cheat-pattern.md) — 反作弊应对
- [`anti-ban-analysis.md`](anti-ban-analysis.md) — 封号风险与量级安全线（**写功能前必读**）
- [`code-review-checklist.md`](code-review-checklist.md) — 审查边界入口（**改完代码必走①→④；装机前④须 PROVEN**）
