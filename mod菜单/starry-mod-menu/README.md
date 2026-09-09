# 星光魔法 MOD 菜单

> **版本**：1.6  
> **日期**：2026-08-08  
> **作者**：阿亮  
> **当前构建目标**：以 `Android.mk` 激活行为准（并行会话可能切换；自测包用私有 `Android_self_test.mk`）
> **框架版本**：2026-09-06 大改版（冷启动对账/CrashHandler/混淆/RegisterNatives）——详见 [经验_星光框架大改版_20260906.md](../../10_docs/_templates/经验_星光框架大改版_20260906.md)

## 一句话介绍

Android 游戏 MOD 菜单框架。UI 为全原生「星光魔法阵」（`NativeMenuController`），C 层用 Dobby + xDL。已落地 **Unity（il2cpp）** 与 **Cocos Creator（jsb）** 两套样板。**2026-09-09 起双轨制**：新游戏走 modern 注册表（`new_game_mod.sh` 四件套 + `MainModern.cpp`，共享文件零接触）；存量游戏仍切公共 `Android.mk` 的 `STARRY_GAME_*`（`game_id` 由 `GameConfig.h` 映射）。见 `SWITCH_GAME.md` 顶部，**一次只编进当前游戏逻辑**。

## 快速导航

- [技术架构文档](ARCHITECTURE.md) — 三层架构、引擎模块、体积控制
- [打包注入流程](INJECT_GUIDE.md) — 编译 / 注入脚本 / 进包清单
- [功能说明文档](功能说明文档.md) — 控件与当前功能列表
- [**原生菜单「星光魔法阵」**](docs/原生菜单_星光魔法阵_20260813.md) — Activity 主路径 / 星空·花藤 / 本轮 UI 结案
- [设计规范](设计风格美术规范.md) — UI / 美术权威规范（菜单 + 后台）
- [设计规范索引](DESIGN_SPEC.md) — 速查
- [后台 UI 摘要](auth-server/docs/ADMIN_UI_SPEC.md)
- [网络验证后台](auth-server/README.md) — FastAPI + Docker 卡密/公告/门控/看板
- [验证对接文档](auth-server/docs/API.md) — 客户端 / 管理端 API
- [授权短票 / 防破解](auth-server/docs/GRANT_AUTH.md) — **日常三步操作** + 给 AI 改代码的说明

## 网络验证（MVP 已落地）

目录：`auth-server/`

```bash
cd auth-server && docker compose up -d --build
# 管理台 http://IP:8000/admin/  · 文档 http://IP:8000/docs
```

菜单账号页登录卡密后，服务器会下发「授权短票」；Native 验过才跑游戏修改（正常玩家无感）。  
发版顺序：**先更新服务器 → 再跑 `./scripts/inject_dwct_12.37.sh`（默认 release/R8）→ 玩家重新登录**。  
勿用 debug 包上线。详情：[INJECT_GUIDE.md](INJECT_GUIDE.md)、[GRANT_AUTH.md](auth-server/docs/GRANT_AUTH.md)。

## 引擎与样板

| 引擎 | 样板游戏 | Native 模块 | 注入脚本 | 当前 mk |
|------|----------|-------------|----------|---------|
| Unity (il2cpp) | 战魂铭人 | `Games/ZhmrMod` + `Unity/Il2CppApi` | `scripts/inject_zhmr.sh` | 关闭 |
| Cocos (jsb) | 动物餐厅 | `Games/DwctMod` + Java `cocos/*` | `scripts/inject_dwct_12.37.sh` | **开启** |

体积原则：仓库可并存多套 `Games/*.cpp`，但 `Android.mk` 的 `LOCAL_SRC_FILES` / `ENABLE_*_HOOK` **只编当前目标** → `libstarry_magic.so` 不会随历史样板线性膨胀。文档与脚本不进游戏 APK。

## 项目结构

```
starry-mod-menu/
├── ARCHITECTURE.md / INJECT_GUIDE.md / 功能说明文档.md / DESIGN_SPEC.md
├── scripts/
│   ├── inject_dwct_12.37.sh             ← 动物餐厅注入
│   ├── inject_zhmr.sh                   ← 战魂铭人注入（当前）
│   └── patch_dwct_iap_gate.js           ← 动物餐厅 jsc 免内购门控
├── app/src/main/
│   ├── assets/certs/                    ← TLS 钉扎证书（无 web/）
│   ├── java/com/starry/magic/
│   │   ├── app/ / bridge/ / menu/       ← 原生菜单框架（NativeMenuController）
│   │   └── cocos/                       ← Cocos 辅助（DwctHooks / CocosJsBridge）
│   └── jni/
│       ├── Main.cpp / GameConfig.h / Android.mk
│       ├── Games/ZhmrMod.* / DwctMod.*  ← 按游戏；mk 只链当前一份
│       ├── Unity/Il2CppApi.*            ← Unity 公共层（切 Unity 时编入）
│       ├── Dobby/ / KittyMemory/ / xDL/
│       └── Includes/
```

## 通信架构

```
NativeMenuController  →  ModBridge.java  →  Main.cpp (JNI)
（星光魔法阵 UI）         （Java Bridge）      （Hook/作弊逻辑）
                                              ↑
                                         GameConfig.h（游戏配置）
```

用户在原生面板点开关 → `ModBridge.onToggleChanged(...)` → Java 存偏好 + 调 JNI → C 层设置 `g_features[...]` → 后台线程执行作弊逻辑。

## 当前状态

> **版本**：1.6（2026-08-08）· 构建目标：**动物餐厅**

- ✅ Cocos 样板：动物餐厅（`DwctMod`，**当前**：免广告 / 免内购）
- ✅ Unity 样板：战魂铭人（`ZhmrMod`，mk 可切回）
- ✅ Java 层：MainActivity / ModBridge / MenuService / NativeMenuController / Preferences；Cocos 辅助 `DwctHooks` / `CocosJsBridge`
- ✅ 全原生 UI「星光魔法阵」：功能 Tab 由 `g_featureList` 动态渲染；账号页到期/次数/解绑/退出；远程公告/Toast 自绘
- ✅ 偏好：SharedPreferences；featNum ≥ 50 只写偏好不进 C
- ✅ 挂载：Activity `TYPE_APPLICATION`（无悬浮窗权限）为主；已授权时 OVERLAY 备用
- ✅ 构建：compileSdk/targetSdk **35**，**JDK 17**；发版用 release（R8）
- ✅ 网络验证：`auth-server/`（卡密/公告/门控/Toast/看板）+ 授权短票 HMAC
- ✅ HTTPS 证书固定：`assets/certs/starlight.crt`（`AuthClient`）
- ⬜ Unreal 专用 Hook 未实现（检测后回落 Generic）
- ⬜ Cocos 通用「特征码扫 .so」路径仍为框架预留（动物餐厅走 JS/smali，不依赖该项）

### Unreal Hook 是什么？

检测到 `libUE4.so` / `libUE5.so` 时引擎类型为 Unreal。UE 与 Unity/il2cpp 不同，通常要解析 GNames/GObjects 等，本仓库**尚未实现专用 Hook**。`Android.mk` 也未开启 `-DENABLE_UNREAL_HOOK`。当前会在 `mod_thread` 里尝试走已启用的 **Generic Hook** 兜底。

### 构建 / 注入

```bash
./scripts/inject_dwct_12.37.sh              # 动物餐厅：默认 release（R8）→ 注入 → 签名安装
# STARRY_BUILD_TYPE=debug ./scripts/inject_dwct_12.37.sh   # 仅联调
# STARRY_SKIP_INSTALL=1 ./scripts/inject_dwct_12.37.sh     # 只出包
# ./scripts/inject_zhmr.sh                               # 战魂铭人（备用）
```

### 更新记录（v1.7 · 2026-08-13）

1. 菜单 UI 全原生：删除 `assets/web`、`PassThroughWebView`、HTML 回退
2. 注入脚本改为清掉旧包 `assets/web`，不再拷贝菜单页

### 更新记录（v1.6）

1. 文档与现状对齐：账号页真实数据、证书固定已落地、悬浮全屏穿透、退出卡密回游客
2. 设置「定制 MOD」复制 QQ；公告 `every_open` / `once_device`；看板活跃窗口
3. 关闭菜单先 `lockAllFeatures`，避免 native 功能残留

### 更新记录（v1.5）

1. 网络验证 / 授权短票 / 管理后台运营能力落地
2. 构建目标切回战魂铭人；`inject_zhmr.sh` 默认 release

### 更新记录（v1.4）

1. 曾短暂以动物餐厅为构建目标；注明 U/C 双引擎样板与 mk 互斥编译
2. 进包原则：未编进 `Android.mk` 的游戏逻辑不进 `libstarry_magic.so`

### 框架修复记录（v1.3）

1. 触摸穿透 → `PassThroughWebView` + `ModBridge.updateTouchTargets`
2. 偏好单写 → 仅 SharedPreferences；废弃 `Changes()` 双写
3. 功能列表动态渲染 → `getFeatureListJSON()` + `renderFeatureList()`（复用原有 CSS）
4. 偏好补全 → Toggle/Slider/Spinner/Input + UI 设置均可恢复；含 Spinner
5. 删除主题色/位置网格死代码
6. Unreal → 文档说明 + Generic 兜底
7. 文档与 compileSdk/偏好链路同步
8. 设置项 ≥50 不进 C
9. JDK 17 构建固定

### UI 修复记录（v1.2）

**第一轮（5 项）：**
1. 下拉菜单不显示 → `position:fixed` + JS `getBoundingClientRect` 屏幕坐标计算
2. 设置声明还原 → 设计稿完整免责声明文案
3. 功能偏好不保存 →（v1.3 已改为 SharedPreferences）
4. 后台恢复公告/toast 重复弹 → `onPause`/`onResume` 改为只切可见性 + `__nativeInited` 标志位
5. toast/公告按设计稿 → 星空背景 + 四角魔法装饰 SVG + 旋转魔法阵 + 双按钮

**第二轮（4 项）：**
1. 设置声明术语精调
2. 下拉菜单被遮挡 → 菜单挂 body + 宽度对齐触发器
3. 旋转屏幕重复弹 → `configChanges` + `onConfigurationChanged`
4. Toast 四角装饰

## 适配新游戏时改什么

**（modern 流程，2026-09-09 起）** `bash scripts/new_game_mod.sh <Game>` 生成四件套后：

1. **`Games/{Game}Mod.cpp`** — 该游戏逻辑（骨架自带防闪退铁律）  
2. **`Games/{Game}Entry.cpp`** — 注册表：`authGameId` / `waitLib` / `featureList`（功能 Tab 与 featNum）/ 初始化参数  
3. **`Android_<tag>.mk`** — 私有构建（差异区只填源文件组合，构建传独立 `NDK_OUT=../../obj_<tag>`）  
4. **注入脚本** — 参考 `scripts/inject_dwct_12.37.sh` / `inject_zhmr.sh`；Cocos 若需改 jsc，另用 patch 脚本  

`MainModern.cpp` / `Main.cpp` / `GameConfig.h` / 公共 `Android.mk` **零接触**。HTML 外观一般不用动。详见 [ARCHITECTURE.md](ARCHITECTURE.md#七适配新游戏)。

（legacy 存量三处接线 `GameConfig.h` / `Main.cpp g_featureList` / 公共 `Android.mk` 仅为旧游戏保留，重新出包时顺手迁移注册表。）
