# 技术架构文档

> ⚠️ **本文档已过时（2026-09-22）**：文中的 WebView/HTML 菜单架构（`mod-menu-v2.html` / `PassThroughWebView` / JS 桥）已整体废弃。
> 现行架构为**全原生菜单**（`NativeMenuController`，WindowManager 悬浮窗 + 原生 View 自绘），见 `docs/原生菜单_星光魔法阵_20260813.md`。
> 本文仅作历史架构参考，请勿按此文档开发。

> **版本**：1.6  
> **日期**：2026-08-08（更新：全屏穿透悬浮层；文档与战魂/卡密现状对齐）

## 一、架构总览

```
┌──────────────────────────────────────────────┐
│  HTML / JS (UI 层)                           │
│  mod-menu-v2.html (星光魔法设计稿)            │
│  用户操作 → ModBridge.onXxx() 调用 Java       │
└──────────────────┬───────────────────────────┘
                   │ @JavascriptInterface
                   ▼
┌──────────────────────────────────────────────┐
│  Java (Bridge 层)                            │
│  ModBridge / MenuService / Preferences       │
│  cocos/DwctHooks · CocosJsBridge（Cocos 用） │
└──────────────────┬───────────────────────────┘
                   │ JNI
                   ▼
┌──────────────────────────────────────────────┐
│  C/C++ (Native 层)                           │
│  MainModern.cpp — modern 入口（注册表模式）  │
│    └ Games/{Game}Entry.cpp — 游戏注册表      │
│  Main.cpp — legacy 入口（存量游戏 #if 路径） │
│  GameConfig.h — legacy 游戏配置              │
│  Games/ZhmrMod — Unity 样板（战魂）          │
│  Games/DwctMod — Cocos 样板（动物餐厅）      │
│  Unity/Il2CppApi — il2cpp 公共层             │
│                                             │
│  工具链：Dobby / xDL（KittyMemory/Keystone  │
│  为历史遗留依赖，零调用，勿在新代码使用）    │
└──────────────────────────────────────────────┘
```

**双轨入口（2026-09-09 起）**：
- **新游戏一律走 modern**：`scripts/new_game_mod.sh <Game>` 生成 Mod+Entry+私有 `Android_<tag>.mk`，入口 `MainModern.cpp`（注册表模式，游戏差异全在 Entry 的 `StarryGameMod` 表）。`Main.cpp` / `GameConfig.h` / 公共 `Android.mk` **零修改**。详见 `SWITCH_GAME.md` 顶部。
- **存量游戏走 legacy**：公共 `Android.mk` 启用一个 `STARRY_GAME_*`（+ `ENABLE_*_HOOK` / `LOCAL_SRC_FILES`），`game_id` 由 `GameConfig.h` 自动映射；重新出包时顺手迁移注册表。**不要同时链两套游戏逻辑**。同步后台 `games.game_id` 与映射表同名（modern 的 id 在 Entry 的 `authGameId`）。

### 体积不会越堆越大

| 层 | 规则 |
|----|------|
| `.so` | 只编译 `Android.mk` 列出的源文件；仓库里闲置的 `ZhmrMod.cpp` 等**不进 so** |
| 菜单 APK / 注入 smali | 会带上 `com.starry.magic.*`（含少量 Cocos 辅助类，体积可忽略）；文档与 `scripts/` **不进** |
| 游戏 APK | 注入脚本拷贝：so + starry smali + certs（可选）+ 游戏侧少量 smali/jsc 补丁；**无** HTML 菜单 |

因此「每适配一个新游戏」在仓库多一份 `Games/XxxMod`，但**打进目标包的只有当前 mk 选中的那一份**。

### 数据流示例：用户开启"无限生命"

```
1. 用户点击 HTML 上的"无限生命"开关
2. JS: ModBridge.onToggleChanged(0, "无限生命", true)
3. Java ModBridge:
   a. Preferences.changeFeatureBool() — 存偏好
   b. nativeSetFeature(0, "无限生命", true) — 调 JNI
4. C Main.cpp: g_features[0] = true
5. 后台 mod_thread 检测到 g_features[0]==true:
   → 获取玩家对象 → 写入 HP=满血
```

### 数据流示例：C 层显示 Toast

```
1. C 层操作成功
2. ShowToast("已秒杀敌人")
3. JNI 调用 MenuService.showToast("已秒杀敌人")
4. Java: menuWebView.evaluateJavascript("showToast('已秒杀敌人')")
5. HTML JS showToast() 显示动画
```

---

## 二、Java 层

### 2.1 MainActivity.java

入口 Activity，职责：
- `System.loadLibrary("starry_magic")` 加载 native 库
- 检查悬浮窗权限（`Settings.canDrawOverlays`）
- 无权限则跳转授权页面
- 有权限则启动 `MenuService`
- **`onConfigurationChanged()`** — 空实现，旋转屏幕时不重建 Activity（配合 Manifest 的 `configChanges` 声明）
- **`onPause()`** — 仅 `setVisibility(INVISIBLE)` 隐藏悬浮窗，不 destroy WebView（避免后台返回时公告/toast 重复弹）
- **`onResume()`** — 仅 `setVisibility(VISIBLE)` 恢复可见性，不重建 WebView

集成到目标游戏时，可设 `gameActivity` 字段为游戏 Activity 全限定名，MainActivity 会先启动游戏再启动菜单。

> **变更说明（v1.2）：** `onPause`/`onResume` 从 destroy/recreate WebView 改为仅切换可见性；新增 `onConfigurationChanged` + Manifest `configChanges` 声明，防止旋转屏幕时 Activity 重建导致公告/toast 重复弹出。

### 2.2 ModBridge.java

`@JavascriptInterface` 注入到 WebView，HTML JS 通过 `window.ModBridge.xxx()` 调用。

核心方法：
- `onToggleChanged(featNum, featName, enabled)` — 开关
- `onSliderChanged(featNum, featName, value)` — 滑块
- `onButtonClicked(featNum, featName)` — 按钮
- `onInputChanged(featNum, featName, text)` — 输入框
- `onSpinnerChanged(featNum, featName, position)` — 下拉
- `getFeatureListJSON()` — 获取 C 层定义的功能列表
- `closeMenu()` / `collapseMenu()` / `expandMenu()` — 菜单控制

每个方法做两件事：存 Preferences + 调 JNI native 方法。

### 2.3 MenuService.java

`Service` 子类，用 `WindowManager` 添加全屏透明 `PassThroughWebView` 悬浮窗。

- `showMenu()` — 创建 PassThroughWebView，加载 `mod-menu-v2.html`，注入 `ModBridge`
- `updateTouchTargets(json)` — JS 上报可交互矩形，透明区触摸穿透到下层应用
- `hideMenu()` / `collapseToBall()` / `expandFromBall()` / `showToast` / `showAnnouncement`
- `setVisibility(int)` — 切换悬浮窗可见性，不重建 WebView
- `setFocusable(boolean)` — 输入框聚焦时临时允许窗口获焦以弹键盘

窗口类型：`TYPE_APPLICATION_OVERLAY`（API 26+）或 `TYPE_PHONE`。  
标志：`FLAG_NOT_TOUCH_MODAL` + 非交互区 `dispatchTouchEvent` 返回 false 实现穿透。

> **变更说明（v1.3）：** 使用 `PassThroughWebView`；JS `reportTouchTargets()` 上报面板/悬浮球/Toast/公告/下拉菜单区域。

### 2.4 Preferences.java

`SharedPreferences` 封装（**唯一持久化层**），用 featNum (int) 作为 key。

- featNum &lt; 50：功能项，受「保存功能偏好」开关控制
- featNum ≥ 50：UI 设置，始终持久化，且 **不写 C 层**
- `exportJSON()` / `getAllPrefsJSON()` 供 HTML 启动恢复
- **已移除** `Changes()` JNI 双写；C 状态仅由 `ModBridge.nativeXxx` 更新

### 2.5 PassThroughWebView.java

全屏透明 WebView 子类：仅当触摸落在 JS 上报的矩形内时消费事件，否则返回 false 穿透。

---

## 三、JS Bridge 接口规范

### 3.1 JS → Java（UI 回调 C 层）

| JS 方法 | 参数 | 说明 |
|---------|------|------|
| `onToggleChanged(featNum, featName, enabled)` | int, String, boolean | 开关 |
| `onSliderChanged(featNum, featName, value)` | int, String, int | 滑块 |
| `onButtonClicked(featNum, featName)` | int, String | 按钮 |
| `onInputChanged(featNum, featName, text)` | int, String, String | 输入框 |
| `onSpinnerChanged(featNum, featName, position)` | int, String, int | 下拉 |
| `getFeatureListJSON()` | → String (JSON) | 获取功能列表（HTML 动态渲染） |
| `getAllPrefsJSON()` | → String (JSON) | 获取 SharedPreferences 全量偏好 |
| `updateTouchTargets(json)` | String | 上报可触摸区域（穿透用） |
| `isGameLibLoaded()` | → boolean | 游戏库是否加载 |
| `closeMenu()` | — | 关闭菜单 |
| `collapseMenu()` | — | 收起为悬浮球 |
| `expandMenu()` | — | 展开菜单 |
| `setFocusable(boolean)` | boolean | 输入法焦点 |

> **约定：** featNum ≥ 50 为 UI 设置，Java 只写 Preferences，不调用 `nativeSetXxx`。

### 3.2 Java → JS（C 层回调 UI）

HTML 中需实现以下全局 JS 函数：

| JS 函数 | 参数 | 调用时机 |
|---------|------|---------|
| `onNativeReady()` | 无 | 页面加载完成，可渲染菜单 |
| `showToast(text)` | String | 操作提示 |
| `showAnnouncement(title, content)` | String, String | 远程公告推送 |
| `collapseToBall()` | 无 | Java 请求收起 |
| `expandFromBall()` | 无 | Java 请求展开 |

### 3.3 功能列表格式

C 层 `g_featureList[]` 返回的字符串数组，格式：

| 前缀 | 示例 | 含义 |
|------|------|------|
| `Category_` | `Category_战斗修改` | 分类标题 |
| `Toggle_` | `Toggle_秒杀` | 开关 |
| `Button_` | `Button_立即秒杀` | 执行按钮 |
| `Slider_` | `Slider_移动速度:1:10` | 滑块（标签:最小:最大） |
| `Spinner_` | `Spinner_难度:简单,普通,困难` | 下拉（标签:选项列表） |
| `InputText_` | `InputText_备注` | 文本输入 |
| `InputValue_` | `InputValue_99999:金币数` | 数字输入（默认值:标签） |

featNum = 数组下标，从 0 开始。

---

## 四、C 层 (Main.cpp)

### 4.1 全局状态

```cpp
static bool g_features[64];       // 开关状态
static int  g_values[64];         // 滑块/下拉值
static char g_texts[64][256];     // 输入框文本
static bool g_btnTrigger[64];     // 按钮触发标志
```

### 4.2 功能列表

```cpp
static const char* g_featureList[] = {
    "Category_战斗增强",           // 0 仅 UI 分组
    "Toggle_无限生命",             // 1
    "Toggle_无限魔法",             // 2
    "Toggle_一击必杀",             // 3
    "Toggle_无敌模式",             // 4
    "Category_角色属性",           // 5
    "Slider_移动速度:1:10",        // 6
    "Spinner_难度等级:简单,普通,困难,地狱", // 7
    "Category_资源工具",           // 8
    "Button_获取金币",             // 9
    "InputValue_99999:自定义数值", // 10
};
```

HTML 在 `onNativeReady()` 中调用 `getFeatureListJSON()` → `renderFeatureList()` 动态生成功能 Tab（复用原有 CSS，不改视觉规范）。`Category_` 行不读写 `g_features[i]`。

### 4.3 后台线程

```cpp
static void* mod_thread(void* arg) {
    // 1. 等待游戏库加载
    // 2. 初始化 Hook（按引擎类型调用对应 resolveXxxOffsets()）
    // 3. 循环检测 g_features[]，执行对应操作
    while (true) {
        if (g_features[0]) { /* 无限生命逻辑 */ }
        if (g_features[2]) { /* 一击必杀逻辑 */ }
        if (g_btnTrigger[6]) { /* 获取金币按钮 */ }
        usleep(100000); // 100ms
    }
}
```

### 4.4 Unity il2cpp

**公共层：** `Unity/Il2CppApi.h` / `Il2CppApi.cpp`（Load、找类、FieldOffset+bias、堆扫描、SafeProbe、runtime_invoke 直调）  
**游戏样板：** `Games/ZhmrMod.cpp`（战魂铭人 / HybridCLR）

**启动时序：** 等 `libil2cpp.so` → `Il2Cpp::GetApi().Load()` → 重试热更类 → Tick。  
**改值：** 堆扫描写字段 / setter 直调；**默认禁止** Dobby 内联短导出桩 `il2cpp_runtime_invoke`（与 `object_init*` 相邻会 SIGILL）。  
**新游戏：** 改 `GameConfig.h` + 新 `Games/XxxMod.cpp` 调 `Il2Cpp::GetApi()`。  
悬浮层为始终全屏 OVERLAY，靠 `PassThroughWebView` + JS hit 区穿透；不改小窗口。

### 4.5 Cocos Creator / cocos2d-jsb

`ENABLE_COCOS_HOOK` 控制编译。`Main.cpp` 仍保留通用的 `resolveCocosOffsets()`（符号/RVA/特征码框架）；**动物餐厅实际逻辑在 `Games/DwctMod`**：

| 功能 | 路径 |
|------|------|
| 免广告 | smali：`AppActivity.showRewardAd` / `isRewardVideoADReady` → `DwctHooks.shouldSkipAd()`；JS 辅助 patch `userData` |
| 免内购 | jsc 门控 `window.__starryNoIap`（`PayMentModel` 渠道 + `PayMentSystem.changeOrderStatus`）；菜单开关 `evalString` 置位；`wxPay` smali 兜底 |

检测：`libcocos2djs.so` / `libcocos2dcpp.so` → `ENGINE_COCOS` → `Dwct_TryInit` / `Dwct_Tick`。

纯「改 .so 偏移」类 Cocos 游戏仍可走 `GameConfig.h` 的 Symbols/Offsets；jsb 业务更常走 **evalString + 少量 Java 钩子**（本样板方式）。

### 4.6 通用 Hook（自研引擎 / 任意 .so）

`ENABLE_GENERIC_HOOK` 宏控制编译。兜底方案：
- `detectGameEngine()` 检测到 `libgame.so` 或未知引擎 → ENGINE_CUSTOM / ENGINE_UNKNOWN
- `resolveGenericOffsets()` 用 RVA 定位数据地址和函数地址
- 配合 `writeValue<T>()` / `readValue<T>()` 直接读写内存

适配任意游戏：修改 [GameConfig.h](file:///Users/aliang/Documents/nixiang/mod菜单/starry-mod-menu/app/src/main/jni/GameConfig.h) 中 `GameGeneric` 命名空间的 `TARGET_LIB` / `Offsets`。

---

## 五、Hook 工具链

### 5.1 HOOK — 函数 Hook

```cpp
// Hook 指定偏移的函数（需保留原函数指针）
HOOK("libgame.so", "0x243AEDC", myFunc, origFunc);

// Hook 符号名
HOOK("libgame.so", "_ZN6Player5GetHPEv", myGetHP, origGetHP);

// 不需要原函数
HOOK_NO_ORIG("libgame.so", "0x123456", myReplaceFunc);
```

### 5.2 PATCH — 内存补丁

```cpp
// hex 补丁
PATCH("libgame.so", "0x123456", "00 00 A0 E1 1E FF 2F E1");

// 汇编补丁
PATCH("libgame.so", "0x123456", "mov r0, #999; bx lr");

// 恢复原始字节
RESTORE("libgame.so", "0x123456");

// 条件开关（bool 为 true 时打补丁，false 时恢复）
PATCH_SWITCH("libgame.so", "0x123456", "mov r0, #0; bx lr", g_features[0]);
```

### 5.3 工具函数

```cpp
// 获取库基址
uintptr_t base = getLibraryAddress("libgame.so");

// 获取绝对地址（支持偏移或符号名）
void* addr = getAbsoluteAddress("libgame.so", "0x123456");
void* sym  = getAbsoluteAddress("libgame.so", "Player_GetHP");

// 检查库是否加载
bool loaded = isLibraryLoaded("libil2cpp.so");
```

### 5.4 工具链来源

| 库 | 功能 | 来源 |
|----|------|------|
| Dobby | inline hook | github.com/jmpews/Dobby |
| KittyMemory | 内存 patch | github.com/MJx0/KittyMemory |
| xDL | 符号查找 | github.com/hexhacking/xDL |
| Keystone | 运行时汇编 | github.com/keystone-engine/keystone |

这些都是第三方开源库，非 LGL 原创。

---

## 六、JNI 方法对应关系

> **2026-09-06 起：RegisterNatives 动态注册**，符号表不再有 `Java_com_starry_magic_bridge_*` 导出。
> 实现函数为 `Main.cpp` 内 `static mb_nativeXxx`，由 `registerBridgeNatives()`（JNI_OnLoad 触发，可重入）
> 用 OBFUSCATE 的类名/方法名/签名注册；新增/改动方法必须同步注册表并跑 `ModBridge.runBridgeSelfTest()`（14 项自检）。
> 唯一保留的导出：`Java_com_starry_magic_menu_Preferences_Changes`（空兼容桩）。

| Java 方法 (ModBridge.java) | native 实现 (Main.cpp，static) | 作用 |
|---|---|---|
| `nativeInit(context)` | `mb_nativeInit` | 初始化，启动后台线程 |
| `nativeSetFeature(featNum, name, on)` | `mb_nativeSetFeature` | 设置开关 |
| `nativeSetValue(featNum, name, val)` | `mb_nativeSetValue` | 设置数值 |
| `nativeSetLongValue(featNum, name, val)` | `mb_nativeSetLongValue` | 设置长整数值 |
| `nativeSetText(featNum, name, text)` | `mb_nativeSetText` | 设置文本 |
| `nativeExecButton(featNum, name)` | `mb_nativeExecButton` | 触发按钮 |
| `nativeGetFeatureList()` | `mb_nativeGetFeatureList` | 获取功能列表 |
| `nativeGetSettingsList()` | `mb_nativeGetSettingsList` | 获取设置列表 |
| `nativeIsGameLibLoaded()` | `mb_nativeIsGameLibLoaded` | 查询状态 |
| `nativeSetAuthorized(on)` | `mb_nativeSetAuthorized` | 诱饵（单独改不够） |
| `nativeApplyGrant(grant, dev, game)` | `mb_nativeApplyGrant` | 验票+置会话 |
| `nativeHasLiveSession()` | `mb_nativeHasLiveSession` | 会话查询 |
| `nativeClearGrant()` | `mb_nativeClearGrant` | 清会话 |
| `nativeGetAuthGameId()` | `mb_nativeGetAuthGameId` | 鉴权 game_id |
| `nativeGetAceGateRemainSec()` | `mb_nativeGetAceGateRemainSec` | 过检门剩余秒 |
| `nativeVerifyOtaSig(payload, sig)` | `mb_nativeVerifyOtaSig` | OTA 验签 |
| `nativeApplyPlayPatch(json)` | `mb_nativeApplyPlayPatch` | 应用玩法补丁 |
| `nativeBridgeRegistered()` | `mb_bridgeRegistered` | 注册自检（v2.1+） |

---

## 七、适配新游戏

### 7.1 改哪些文件（modern 注册表流程，2026-09-09 起）

```bash
bash scripts/new_game_mod.sh <Game> [日志标签]   # 生成四件套
```

| 步骤 | 文件 | 说明 |
|------|------|------|
| 1 | `Games/{Game}Mod.cpp` | 游戏逻辑（生成器从 `_TemplateMod` 骨架产出，含防闪退铁律） |
| 2 | `Games/{Game}Entry.cpp` | 注册表：`authGameId`（game_id）/ `waitLib`（目标库）/ `featureList`（功能 Tab）/ 初始化参数 |
| 3 | `Android_<tag>.mk` | 私有构建（差异区只填源文件组合；构建传 `NDK_OUT=../../obj_<tag>`） |
| 4 | `scripts/inject_*.sh` | 拷 so / assets / smali，改启动 Activity |

**共享文件零接触**：`MainModern.cpp` / `Main.cpp` / `GameConfig.h` / 公共 `Android.mk` 都不改。
（legacy 存量游戏仍走旧三处接线 `GameConfig.h` / `Main.cpp g_featureList` / 公共 `Android.mk`，重新出包时顺手迁移。）

### 7.2 引擎对照

| 引擎 | 样板 | 资料 |
|------|------|------|
| Unity il2cpp | `ZhmrMod` | Il2CppDumper |
| Cocos jsb | `DwctMod` | 解密 jsc + 必要时 smali |
| 自研 / 任意 | Generic 区段 | IDA RVA |

### 7.3 切回战魂（Unity）示例（legacy 路径）

```makefile
# Android.mk
LOCAL_CFLAGS += -DENABLE_UNITY_HOOK
# LOCAL_CFLAGS += -DENABLE_COCOS_HOOK
LOCAL_SRC_FILES := Main.cpp \
                   Unity/Il2CppApi.cpp \
                   Games/ZhmrMod.cpp
```

同时把 `g_featureList` 改回战魂四项，并用 `scripts/inject_zhmr.sh` 注入。
（modern 游戏切游戏=切私有 `Android_<tag>.mk`，无此步骤；战魂迁移注册表后此节删除。）
