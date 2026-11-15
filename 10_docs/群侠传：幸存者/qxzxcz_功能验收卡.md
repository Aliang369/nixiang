# 功能验收卡 · 群侠传：幸存者 0.2.3.10

> 模板：`10_docs/_templates/功能验收卡.md`。本卡只留判断所需最短信息；完整日志在 `09_logs/群侠传：幸存者/`。

## 1. 身份

- 游戏 / 版本 / vc：群侠传：幸存者（com.nenlystudio.qunxiasurvivor.cn）/ 0.2.3.10 / 20310
- 功能菜单名：`Category_功能` + `Slider_伤害倍率:1:100:1` + `Toggle_无敌` + `Toggle_无限银钱`
- 用户预期：开无敌不掉血、伤害倍率随滑条（1..100）放大、无限银钱恒满；关掉即恢复原样
- 当前状态：**已生效**（L3 用户实机验收通过，2026-11-15）

## 2. 实现点

- 引擎与加载路径：Godot 4.6.1（无 il2cpp）。菜单入口 `StarryInitProvider`（Manifest 首 provider）→ `MenuService`（ACTIVITY 覆盖层）→ `GameHooks` SPI
- Hook / 数据写入点：GDScript 侧三处等长替换 —— `components/health_component.gdc`（invulnerable 改属性）、`systems/combat_hit_pipeline.gdc`（`target_damage *= StarrySwitch.num(1,1.0)`）、`core/model/economy_store.gdc`（set_silver/install_draft 判定）；开关桥为新增文件 `scripts/starry_switch.gdc` + `.gd.remap`
- 开关与默认值：`[1]` 伤害倍率默认 1、`[2]` 无敌默认关、`[3]` 无限银钱默认关；状态文件 `files/starry_state.txt`（单行 8 个空格分隔整数）
- 关键文件：`07_patched/群侠传：幸存者/gdc_menu_0.2.3.10/`（5 个产物）、`04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.10.sh`（组装）、`mod菜单/starry-mod-menu/app/src/main/jni/Games/QxzxczEntry.cpp`（功能表）

## 3. 已实证

| 结论 | 证据入口 | 日期 / 设备 |
|---|---|---|
| 无敌开关开/关即时生效，用户实机确认「功能都开了」 | `09_logs/群侠传：幸存者/20261115_feature_channel.log`：`[开关] #2 Toggle_无敌 = OFF → ON`、`starry_state.txt = 0 100 1 1 0 0 0 0`（[2]=1） | 2026-11-15 / OPD2404 f9a116a2 |
| 伤害倍率滑条 1..100 全量生效（拉到 100） | 同上：`StarryMagic: [数值] #1 Slider_伤害倍率:1:100:1 = 1 / 70 / 77 / 92 / 99 / 100`；`StateMirror mirrored … 0 100 1 1 …`；`godot: [StarrySwitch] 0 100 1 1 0 0 0 0` | 2026-11-15 / OPD2404 f9a116a2 |
| 无限银钱开启后恒满（[3]=1） | 同上：`[开关] #3 Toggle_无限银钱 = ON`、state 第 4 位由 0 → 1，GDScript 读到同一串 | 2026-11-15 / OPD2404 f9a116a2 |
| 开关通道闭环（Java→native→StateMirror→GDScript） | `Mod_Menu: [Qxzxcz] value#1 = 1` / `feat#2 = 0` → `StateMirror` 双写 → `godot [StarrySwitch]` 同值 | 2026-11-15 / OPD2404 f9a116a2 |

## 4. NOT PROVEN

- 长时游玩回归（连续战斗/切场景 30 分钟以上）未做专项取证；只做了本次拨开关 + 用户实际操作确认。
- XPosed / 其它 ROM 上的表现未验证（仅 OPD2404 / ColorOS 实测）。

## 5. 已知限制

- 现象：`W MenuService: SPECIAL_USE not in Manifest, fallback plain startForeground`
  适用范围：全部星光包（跨游戏既有现象，非本作特有）
  用户决定：记录（服务回退后照常运行，菜单 overlayVisible=true）
  若继续修复，下一条最小区分实验：查 framework 对插入 service 元素的 foregroundServiceType 解析
- 现象：TapSDK 首次初始化在本轮已不再报 1001「包名、签名错误」（补丁 D 回灌官方 cert MD5 后消失）
- 授权墙：仅首次需要用户输一次授权卡（`logged_in=false` 时功能位会被拒），重装同签名覆盖不重复要求

## 6. 下次会话入口

- 先读：`10_docs/群侠传：幸存者/_state/0.2.3.10_组装与装机.md`（§2 自查 / §6 第三轮 / §7 功能通道）
- 直接复现命令：`bash 04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.10.sh` → `STARRY_INSTALL=1 SERIAL=f9a116a2 bash …`
- 只需向用户确认的问题：本次三个功能是否还有边界场景（如高倍率下结算溢出）需要复测

## 7. 回退方式

- 回退到官方：官方底包 `02_apks/群侠传：幸存者/群侠传：幸存者_0.2.3.10_official_from_device.apk`（590,953,445 B，md5 7677c9c2…）；**异签名，必须先在设备上 `pm uninstall` 再装官方包 ⇒ 会清掉本机存档/登录态**
- 本次改动的可逆性：只改了包内 5 个条目 + 新增 4 个条目（classes.dex / classes2.dex / so / Manifest / 3 个 gdc + 2 个 starry_switch 文件）；原始副本：底包 APK 全量保留，gdc 原文件在底包内，dex 原文件 `03_decompiled/群侠传：幸存者/0.2.3.10/` 下
- 残留清理：设备侧 `files/starry_state.txt` + `Android/media/<pkg>/starry_state.txt`（卸载即消失）
- 回退验证：未验证（本轮只做正向安装）
