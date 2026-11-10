# 游戏模块编写铁律（GAME_MOD_GUIDE）

> 📍 路径：`mod菜单/starry-mod-menu/docs/GAME_MOD_GUIDE.md`
> 🎯 触发场景：AI 为任何新游戏编写 `Games/*Mod.cpp` 之前**必读**，违反任意一条 = 闪退/掉帧风险
> 📦 标准骨架：`Games/_TemplateMod.h` + `Games/_TemplateMod.cpp`（从这里复制，勿重造轮子）
> ✅ 正面范例：`Games/Archero2Mod.cpp`（弓箭传说2，具体方法 hook 的正确写法）

---

## 核心结论（为什么之前的写法会闪退/掉帧）

| 错误写法（已否决） | 正确写法（本规范强制） |
|------|------|
| hook HybridCLR/解释器**共享 invoke 入口**，每个方法调用都进 hook | hook **具体方法**（`SetTarget` / `UpdateHp` / `AddSkill` 等），只有该方法被调才触发 |
| 每次内存读写裸解引用 `*(int*)p`，无指针校验 | 读写前先 `IsValidHeapPtr`，或走 `SafeWriteI32` 等助手 |
| 每次 Tick / 热路径里 `SafeProbe`（装/卸 SIGSEGV 信号处理器） | `SafeProbe` 只在**一次性初始化**用，热路径用指针校验 + 缓存 |
| 每帧重复 `object_class` + `class_name` 解析类名 | 类名解析结果**缓存**（单槽或按 klass 缓存） |
| 共享入口 hook 里做方法名分类 + 字符串比较（96 槽缓存） | 具体方法直接 hook，无需分类 |

---

## 7 条铁律

### 1. 只 hook 具体方法，绝不 hook 共享入口

HybridCLR / 解释器的 `invoke`、`runtime_invoke`、`evalString` 这类**所有方法调用都经过的入口**，一旦 hook，游戏每次走热更逻辑都要进你的函数，且无法精确过滤 → 掉帧 + 闪退。

```c
// ❌ 禁止：hook 共享 invoke，靠方法名分类
DobbyHook(invoke_addr, Hooked_Invoke, &orig);

// ✅ 正确：hook 具体方法
void* mi = api.GetMethod(cls, "TakeDamage", 2);
DobbyHook(*reinterpret_cast<void**>(mi), Hooked_TakeDamage, &orig);
```

### 2. 每个 hook 必须调原函数（orig）

不调 orig → 参数被当指针解引用 → SIGSEGV。即使只想改返回值，也要先调 orig。

```c
static void Hooked_Foo(void* self) {
    if (g_origFoo) g_origFoo(self);   // 铁律：先调原函数
    if (!g_feat.load()) return;       // 功能关 → 直接返回，不做事
    // 功能开 → 在这里做额外处理
}
```

### 3. 内存读写前先 IsValidHeapPtr，禁止裸解引用

```c
// ❌ 禁止
*reinterpret_cast<int32_t*>((uintptr_t)obj + off) = 999;

// ✅ 正确
static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (obj && Il2Cpp::IsValidHeapPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}
```

### 4. SafeProbe 只在一次性初始化里用

`SafeProbe` 每次调用都会 `sigaction` 装/卸 SIGSEGV+SIGBUS 信号处理器，且跨线程的 SIGSEGV 会直接 `raise` 让进程死掉。**热路径 / Tick / 每帧调用里绝不允许出现 SafeProbe**。

```c
// ✅ 允许：InstallHooks（只跑一次）
Il2Cpp::SafeProbe([&]() { /* 一次性探测 */ });

// ❌ 禁止：Hooked_XXX / Tick / 循环里
Il2Cpp::SafeProbe([&]() { /* 每帧都跑 */ });
```

### 5. 类名/方法名解析结果缓存

`object_class` + `class_name` 是字符串解析，每帧调用会累积开销。用单槽缓存（按 klass 判断是否命中）：

```c
struct ClassNameCache { void* klass; char name[40]; bool valid; };
static ClassNameCache g_heroCache;
// CacheClassName() 只在 obj 的 klass 变化时才重新解析 class_name
```

### 6. 全局开关用 std::atomic<bool>，重入用 reenter 旗标

```c
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_reenter{false};   // 防递归死循环
```

### 7. Tick 只做轻量同步，不做重活

`Tick` 每 200ms 被调一次，只能做「同步开关 + 兜底重装 hook」。**禁止在 Tick 里：装信号处理器、扫堆、遍历对象列表、逐对象写字段**。

```c
void Xxx_Tick(const bool* features) {
    if (!features) return;
    g_dmg.store(features[X_FEAT_DMG]);
    if (!g_ready.load()) InstallHooks();   // 兜底：还没装就再试
}
```

---

## 功能实现分层（定稿 2026-09-09：数据优先、行为 hook、字节 patch 禁用）

拿到功能需求先分类，**按"改什么"分层，不是按"找什么"分层**：

| 功能类型 | 实现方式 | 理由 |
|----------|----------|------|
| **数据型**（无敌=HP 回写、属性增益、资源锁定、数值锁定） | 偏移定位（dump 实测）+ SafeWrite 直写，Tick 轮询维持 | 零 hook 痕迹（未开功能时零改写，防封最优）；不会挂错闪退；正面范例=超维进化属性增益、`docs/reference/无RVA便携模板.cpp`（仅作历史参考） |
| **行为型**（改判定、伤害结算、拦截上报、CD 重置） | Dobby hook：RVA 优先（maps RX + 序言校验），RVA 失效自动转按名解析 | Dobby 是"RVA 定位后安全改函数"的工程化实现（trampoline 保 orig、指令迁移）；手写字节 patch 改函数是死路 |
| **字节 patch 改函数** | **默认禁用** | E15-E18 断闸全灭、1Hz 完整性校验扫描、arm64 手编易错、RVA 漂移后写错位置=闪退（工作区死路清单） |

## 新游戏接入检查清单

写 `Games/{Game}Mod.cpp` 前逐条确认：

- [ ] 用 `bash scripts/new_game_mod.sh {Game}` 生成四件套（Mod + **Entry + 私有 mk**），不要手复制、不要从旧游戏（尤其 Hzsj）复制
- [ ] `{Game}Entry.cpp` 填好：`authGameId`（game_id）/ `waitLib` / `featureList`（featNum 对齐）/ 初始化参数
- [ ] 共享文件零改动：`MainModern.cpp` / `Main.cpp` / `GameConfig.h` / 公共 `Android.mk`（Main.cpp 连注释都禁改——OBFUSCATE 行号敏感）
- [ ] 按上面「功能实现分层」先分类再选实现方式
- [ ] 只 hook 具体方法，没有 hook invoke/runtime_invoke/evalString 共享入口
- [ ] 每个 hook 都调 orig
- [ ] 所有内存读写前有 IsValidHeapPtr（或 SafeWrite 助手）
- [ ] 热路径/Tick 里没有 SafeProbe
- [ ] 类名解析有缓存
- [ ] 有 reenter 重入保护
- [ ] Tick 只同步开关 + 兜底安装
- [ ] 偏移来自 dump.cs 实测，标注来源，不猜测
- [ ] 构建传独立 obj：`NDK_OUT=../../obj_<tag>`（并行会话互不踩）

---

## 常见错误对照（审查时直接查）

| 症状 | 根因 | 对应铁律 |
|------|------|---------|
| 进游戏 1-2 秒闪退 | hook 共享入口 + 方法分类错 | 1 |
| 对局中闪退 | 裸解引用写错偏移 / 没调 orig | 2、3 |
| 掉帧卡顿 | 每帧 SafeProbe / 每帧 class_name 解析 | 4、5 |
| 功能偶尔失效 | 偏移猜的 / 缓存满了被静默丢弃 | 7 |
| 死循环 SIGSEGV | hook 函数里递归触发自己，无 reenter | 6 |

---

## 八、Java 侧单游戏差异：只写 `games/<game_id>/GameHookImpl`

> 2026-11-11 起。判据唯一正文＝`app/src/main/java/com/starry/magic/core/GameHook.java`。

**一句话**：native 侧的游戏差异在 `Games/{Game}Entry.cpp`；Java 侧的游戏差异在
`app/src/main/java/com/starry/magic/games/<game_id>/GameHookImpl.java`。
**除此之外，任何框架类都不许出现具体游戏的名字。**

### 为什么（真实事故形态）

| 形态 | 历史例子 | 后果 |
|---|---|---|
| 无条件调某游戏补丁 | `StarryApp.attachBaseContext` 里直接 `DwctIapGate.apply()` | 每款游戏启动都跑动物餐厅的 IAP 补丁 |
| 写死单游戏临时目录 | `/data/local/tmp/ltzj_st/libstarry_magic.so` 出现在 `ModBridge`/`MenuService` | 雷霆战机的落盘路径成了全游戏的备选路径 |
| 写死单游戏包名 | `ModTarget.GAME_PKG = "com.nekki.shadowfight3"`；`StarryNativeLoad` 里 `"com.qxjc.hxwjmlq".equals(...)` | 换游戏必须改框架文件 |
| 平台级包名白名单 | `TapLoginForceWeb` 里 47 个包名 | 新游戏继承全部历史补丁 |

### 怎么写（新游戏标准动作）

```java
package com.starry.magic.games.<game_id>;

import android.app.Application;
import com.starry.magic.core.GameHook;

public final class GameHookImpl implements GameHook {

    private static final String PKG = "com.example.game";   // 独立助手形态要用

    @Override public String targetPackage() { return PKG; }   // ★ 必写

    // 以下按需覆写，用不到就整个删掉（接口有默认空实现）
    @Override public void onAttachBaseContext(Application app) { /* 最早的补丁 */ }
    @Override public void onApplicationCreate(Application app) { /* native 已加载后 */ }
    @Override public void onActivityPreCreated(android.app.Activity a) { /* 登录门闩等 */ }
    @Override public void onActivityCreated(android.app.Activity a) { /* 兜底 */ }
    @Override public boolean usesPackagedSoAlias() { return false; }  // SecShell 类壳才 true
}
```

**框架怎么找到它**：`core/GameHooks.get()` 用
`Class.forName("com.starry.magic.games." + game_id + ".GameHookImpl")` 反射装载，
`game_id` 来自 native 烘焙值（`GameConfig.h ← Android_<tag>.mk`）。
找不到 ⇒ 退回空实现，**不报错、不崩**。所以「本游戏不需要任何 Java 补丁」＝
不写这个类，或者只写 `targetPackage()`。

### 注入脚本怎么裁（隔离的关键一步）

按 `game_id` 只拷两棵子树：

```
com/starry/magic/core/**            ← 框架，所有游戏都要
com/starry/magic/games/<game_id>/** ← 只拷本游戏的
```

`games/` 下别的游戏目录**不要拷**。存量脚本仍整拷 `com/starry`（兼容期），
新游戏脚本按上面裁。

### 机器关

```bash
bash 04_scripts/_common/check_framework_isolation.sh --verbose
```

它在 `preflight.sh` 里自动跑（每次改完文件必过）。6 项检查：
① 框架层无 `games.<id>.<Class>` 引用 ② 无单游戏临时目录 ③ 无写死的外部游戏包名
④ `LegacyTapTapGames` 只减不增 ⑤ 每个 `GameHookImpl` 有 `targetPackage()`
⑥ `TapLoginForceWeb` 白名单只减不增。

### 存量兼容垫片（只减不增，迁移完即删）

- `core/legacy/LegacyTapTapGames.java` —— 8 个 game_id 仍走旧的 TapTap 强制网页登录。
  该游戏迁到自己的 `GameHookImpl` 后，从清单里删掉对应行并把基线数字减一。
- `sign/TapLoginForceWeb.java` —— 47 项包名白名单，同上。
- `ltzj/NativeLinker.java`、`ltzj/DexInstaller.java` —— 旧包还在引用，保留；
  新代码用 `core/NativeLinker` / `core/NativeLoader`。

> 这三个文件的存在本身就是「框架收口未完成」的进度条，条目数只许降。
