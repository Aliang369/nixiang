# 功能实现总入口

> 📍 路径：`re-implement/SKILL.md`
> 🎯 触发场景：拿到偏移/RVA/方法名后，需要选择正确的 Hook 模式来实现功能

---

## 决策流程

拿到偏移后，**先按"改什么"分层，再按 Hook 模式选择实现方式**。

### 第零步：功能分类分层（定稿 2026-09-09）

| 功能类型 | 实现方式 | 理由 |
|----------|----------|------|
| **数据型**（无敌=HP 回写、属性增益、资源锁定） | 偏移定位 + SafeWrite 直写，Tick 轮询维持 | 零 hook 痕迹（未开功能=零改写，防封最优）；挂错不会闪退；范例=超维进化、`mod菜单/starry-mod-menu/docs/reference/无RVA便携模板.cpp` |
| **行为型**（改判定、伤害结算、拦截上报） | Dobby hook：RVA 优先（序言校验），失效自动转按名 | Dobby 是"RVA 定位后安全改函数"的工程化实现（trampoline 保 orig）；手写字节 patch 是死路 |
| 字节 patch 改函数 | **默认禁用** | E15-E18 断闸全灭、1Hz 完整性校验、arm64 手编易错、RVA 漂移写错位置=闪退 |

### 第一步：判断引擎类型

下表仅供形成候选，组件存在不证明实际执行路径；用加载关系和运行证据确认，详见 `../re-analyze/SKILL.md`。

| 特征 | 候选引擎 | 候选实现路径 |
|------|------|----------|
| `libcocos2djs.so` + `.jsc` | Cocos Creator JS | evalString splice（见 `utility-pattern.md`） |
| `libil2cpp.so` + `global-metadata.dat` | Unity IL2CPP (AOT) | Dobby hook RVA |
| `libil2cpp.so` + HybridCLR | Unity IL2CPP (热更) | 公共 invoke 跳板 / 运行时按名解析 |
| `libxlua.so` | Unity + xLua | LuaEnv.DoString monkey-patch |

### 第二步：阅读 hook-rules.md（铁律）

**在任何功能实现之前，先读 `hook-rules.md`。** 默认保留原函数必要副作用，并核对 ABI、地址坐标系、生命周期、重入及并发契约；历史事故不等于违反某条就必崩，也不能证明唯一根因。

**同时必读标准框架（写 Games/*Mod.cpp 前）：**
- 铁律全文：`mod菜单/starry-mod-menu/docs/GAME_MOD_GUIDE.md`（含 2026-09-09 定稿的功能实现分层）
- 标准骨架（v3，注册表版防闪退默认实现）：`mod菜单/starry-mod-menu/app/src/main/jni/Games/_TemplateMod.h/.cpp`
- 正面范例：`mod菜单/starry-mod-menu/app/src/main/jni/Games/Archero2Mod.cpp`（按名 hook）、`Games/WhrhxMod.cpp`（RVA 双路径）、`mod菜单/starry-mod-menu/docs/reference/无RVA便携模板.cpp`（纯数据直写，仅作历史参考）
- 反面教材：`mod菜单/starry-mod-menu/app/src/main/jni/Games/HzsjMod.cpp`（SafeProbe 热路径 + 共享跳板，闪退/掉帧教训）

**新游戏（2026-09-09 起 modern 注册表路线）：**
1. cpp 一律用生成器产出：`bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}`，产出四件套（Mod.h/.cpp + **Entry.cpp + 私有 mk**），禁止手复制、禁止从旧游戏（尤其 Hzsj）复制
2. 写完 Mod 后接线只碰新文件：`{Game}Entry.cpp` 填注册表（authGameId/waitLib/featureList/初始化参数），构建走 `Android_{tag}.mk` + 独立 obj
3. **共享文件禁改**：`MainModern.cpp` / `Main.cpp`（连注释都不行，OBFUSCATE 行号敏感）/ `GameConfig.h` / 公共 `Android.mk`——往这些文件加游戏分支=堆积复发，见 `mod菜单/starry-mod-menu/SWITCH_GAME.md` 顶部

**存量旧游戏更新**：不重写、不迁移、不按新模板改造——按 `version-update-relocate.md` 重定位锚点 + 最小改动，新增 hook 沿用模板安全助手与懒装模式；完整迁移注册表只在重新出包且真机回归时做（见 `10_docs/_state/games/infrastructure.md` mod 菜单框架记录）。

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

### 第六步：代码审查（G1 可测试 / G2 运行 / G3 交付）

**改完代码走 `code-review-checklist.md`「审查边界」①→④（改动范围 → 四类安全契约 → 按改动类型查维度 → 证据门禁）。逐条输出 CLAIM + 适用范围 + 证据 + VERDICT。一轮结论后停止扩展，只修已发现问题；修后复查修复点及直接影响面，不无限审计。**

首次受控测试前满足 G1，不要求先证明尚未测试的运行行为；G2 需要行为与副作用实测，G3 需要确切交付产物及约定回归证据。NOT PROVEN 明示缺口；可授权执行的最小验证可继续，需要用户操作/决策或安全边界不明时再等待。构建脚本须查输入身份、错误传播、输出身份，不仅游戏标签。

五维展开按③命中的改动类型查，不是每次都全量见下节：
1. 稳定性（防闪退）：内存安全、数值安全、线程安全、对局生命周期
2. 性能（防掉帧）：调用频率、I/O 开销、日志控制
3. 功能有效性（防突然失效）：偏移可靠性、hook 挂载、开关同步、边界条件
4. 反作弊（防封号）：上报检测、数值合理性
5. 审查结果记录到文档

### 第七步：实测验证

**G1 通过且具备授权与受控测试条件后可以首次装机测试。编译成功、安装日志和 hit/patch 计数仅证明对应局部事实，不能证明目标行为或稳定性。未覆盖的运行声明保持 NOT PROVEN。**

1. **Log 验证**：`adb logcat` 确认 Hook 安装成功、功能触发日志正确
2. **实测验证**：在游戏中实际操作，确认功能生效且不崩溃
3. **副作用检查**：
   - 通关是否报"数据异常"（反作弊）
   - 结算是否失败（服务端校验）
   - 其他功能是否受影响
   - 长时间运行是否稳定（≥5 局）

### 重复反馈与跨会话交接

- 同一功能被用户第二次反馈，或日志已经超过阅读预算时，按
  [`功能验收卡.md`](../../10_docs/_templates/功能验收卡.md) 压缩为：用户预期、
  挂点、已实证、NOT PROVEN、已知限制、下一条最小区分实验。
- `evidence_日期/` 增加
  [`README.md`](../../10_docs/_templates/证据索引README.md)，只列决定性证据和
  最短复现命令；后续会话先读该 README，不通读完整 logcat。
- 用户决定“只记录、不修复”时，更新功能验收卡和游戏状态摘要，然后停止扩展实现。

## 子文件索引

- [`hook-rules.md`](hook-rules.md) — Hook 通用铁律（**必读**）
- [`version-update-relocate.md`](version-update-relocate.md) — 版本更新重定位方法论（小版本失效 `hits=0` 时照做）
- [`damage-pattern.md`](damage-pattern.md) — 伤害修改模式
- [`economy-pattern.md`](economy-pattern.md) — 经济修改模式（含消费路径全覆盖清单）
- [`il2cpp-abi-pitfalls.md`](il2cpp-abi-pitfalls.md) — IL2CPP ABI 坑清单（delegate/Vector3Int/instance self/void 返回值；写 Dobby hook 前对照）
- [`unlock-pattern.md`](unlock-pattern.md) — 解锁修改模式（模式三＝**门控判定函数幂等覆盖**：游戏内置功能被进度门控时的「解锁」『消消英雄2 自动战斗/3倍速』）
- [`utility-pattern.md`](utility-pattern.md) — 工具模式
- [`hotfix-dll-replace-pattern.md`](hotfix-dll-replace-pattern.md) — 授权热更程序集变更的项目模式（合并 2026-09-05；适用范围、无限升级历史证据及事实/NOT PROVEN 边界见正文，非官方解法，非全命题已证明）
- [`anti-cheat-pattern.md`](anti-cheat-pattern.md) — 反作弊应对
- [`anti-ban-analysis.md`](anti-ban-analysis.md) — 封号风险与量级安全线（**写功能前必读**）
- [`code-review-checklist.md`](code-review-checklist.md) — 审查边界入口（**改完代码必走①→④；首次测试前 G1，运行 G2，交付 G3**）
