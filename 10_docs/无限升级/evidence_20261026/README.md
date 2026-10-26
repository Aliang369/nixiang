# 无限升级 20261026 证据（v11.266018 / vc1637 重出）

本轮目标：官方 v11 原包重出星空版，吃 **2026-10-26 现网热更 DLL**，保留 15 项功能 + 新增「转生时间无限制」，
并把「广告奖励 / 物品翻倍 / 经验倍率」三项改成可任意填值的输入框。

## 一、身份链（输入 → 产物，逐环可复算）

| 环节 | 身份 |
|---|---|
| 官方底包 | `02_apks/无限升级12.0.apk`（实为 v11.266018/vc1637）113,081,282 B / sha256 `7dc45298c46e80dbf3cbf9ce926c9fb0114192835f3d348dbdfd071d6720b099` / 签名 MD5 `40fb0d3074290c0fd60a0dd5303b1757` |
| 现网热更 DLL | `07_patched/无限升级/newdump_20261026/targetfuntional_NEW_20261026.dll` 2,028,544 B / sha256 `eb149a78…` / fnv `0x1bf6c08f1ba38809` |
| A 线补丁产物（hf4 基线） | 同目录 `patched_new.dll` 2,022,912 B（`ilpatch` v1 31 patches → `ilpatch_delta`） |
| hf4 补丁 DLL | `07_patched/无限升级/targetfuntional_vc1637_hf4_patched.dll` 2,025,984 B / sha256 `d87a7f9419f8049fe0cae723356e4635f99e7f4717e8f4c4fd544121f7b62c3b`（官方数组余量 2,560 B） |
| so | `libstarry_magic.so` 6,205,832 B / sha256 `6be6b8841c44cc4f2bad383a5e393cd40506490d1b02d0f0d5bc32b80c51327d`（内嵌 hf2/hf4/hf3） |
| APK | `08_output/无限升级/无限升级_11.266018_vc1637_starry-releaseSigned.apk` 115,928,793 B / md5 `5963c9bc59341310a4cde99077bb482b` / 签名 CN=StarryMagic |

## 二、决定性日志行（设备 f9a116a2，2026-10-26 16:50 冷启）

```text
I Mod_Menu: [WxsjVC1637] hook installed @base=0x7348635000 +0x2b3f3c4 orig=0x756f5eb000 variants=3(hf2:2028032B hf4:2025984B hf3:2025984B)
I Mod_Menu: [WxsjVC1637] Exec call len=2028544 head=4d5a9000 armed=1
I Mod_Menu: [WxsjVC1637] patched dll injected (vc1637 hf4, hit#1 loaded=2028544 fnv=1bf6c08f1ba38809 patched=2025984 pad=2560)
```

⇒ **v11 客户端下发的就是 2026-10-26 现网 DLL**（长度+fnv 双命中 hf4），补丁按变体精确注入，无 PASSTHROUGH / 无 MISMATCH。
这条把「11 版也能吃最新热更」从推断升级为实测。

## 三、本轮代码/数据改动（增量）

- `04_scripts/无限升级/ilpatch_vc1637/Program.cs`
  - 新增 `hf4` VariantSpec（基线 = `newdump_20261026/patched_new.dll`，2,022,912 / official 2,028,544）。
  - `--incremental` 由「只追加 item_reverse」扩为补齐 vc1637 全套：item_reverse + minus_item 本体 + 防封开关 + BanGame 门 + exp_multi + shop_ads_unlimited + 版本钉死 + **item_double_x** + **zhuansheng_free**，并加基线长度闸门与幂等判据。
  - `PatchShopAdsUnlimited` / `PatchVersionPin` 改幂等（返回是否改动，重跑不重复插门）。
  - 新增 `PatchItemDoubleMultiplier`：`GameData::AddItem(string,long,bool,string)` 头部 `ldstr "item_double"/GetBool/brfalse/ldarg.2/ldc.i8 2/mul/starg.s` → `int v=GetInt("item_double_x",1); if(v>1) amount*=v;`。
  - 新增 `PatchZhuanShengFree`：`LunHuiWindow::WindowOpen` 两处 `TotalHours >= 1.0`（IL_0037 前 `clt.un`、IL_008E 前 `blt.un.s`）改成 `zhuansheng_free ? -1.0 : 1.0`。
- `Games/WxsjVc1637Entry.cpp`：feat 7/8/12 → `InputValue_1:广告奖励倍数 / 物品倍数 / 经验倍数`；新增 feat 17 `Toggle_转生时间无限制`。
- `Games/WxsjVc1637Mod.h/.cpp`：加 `WXSJVC_FEAT_ZHUANSHENG=17`；`OnInputChanged` 承接 4 个倍率键（纯数字 + 1..1000000 夹取）；`kDllVariants` 加 hf4 并置于 hf3 之前（同长回退优先现网）；**停用 base 1,960,960**。
- `Android_wxsj_vc1637.mk` / `scripts/wxsj_vc1637_so_only.sh`：三变体静态库 + `INCR:` 基线语义 + 断言表（25 必须有 / 3 必须无）。
- `10_docs/无限升级/wxsj_vc1637_menu_template.json`：feat 1/2/4–17 共 16 项。

**为什么停用 base 1,960,960**：那份官方 DLL 早于 2026-09-23 二次热更，方法体里没有 `AddItem(string,long,bool,string)` 4 参主路径
（实测 `FindAddItemMain` 命中 0 个），物品倍数/转生无限两项物理装不进去；留着只会产出「菜单有开关但无效」的错版。
未知长度/哈希仍由 so 侧 `PASSTHROUGH 官方版` 兜底（宁可不生效，不能错版）。

## 四、校验记录

```text
check_release_apk.sh  --expect-vc 1637 --expect-vn 11.266018   → ✓ 干净正式包（188 文件扫禁用串 0 命中、无 debuggable）
check_release_clean.sh --src Games/WxsjVc1637Mod.cpp --src Games/WxsjVc1637Entry.cpp --so libstarry_magic.so
                                                              → PASS（违规 0 / so 无诊断 chunk）
pre_install_gate.sh   （控件类型 / 门禁串 / Manifest AXML / so+metadata 与底包一致）→ PASS=4 FAIL=0
ilpatch_vc1637 --variant hf4 --incremental（重跑）             → 全部「已是最新形态」原样复制（幂等）
包内断言                                                     → InputValue_1:广告奖励倍数/物品倍数/经验倍数、Toggle_转生时间无限制、
                                                               zhuansheng_free、item_double_x 各 1 次；旧 Slider_经验倍率/Toggle_物品翻倍 0 次
install_starry_apk.sh                                         → 设备 base.apk md5 == 产物；运行 so md5 == 包内 so
post_install_precheck.sh                                      → PASS_TO_HUMAN（so 加载 / 菜单弹出 / auth game_id=wxsj / 空转 1min 不崩）
被动崩溃监控（dropbox，装机后 ~30min）                          → 无新增 data_app_crash / data_app_native_crash / ANR（最近一条为 2026-10-25 16:30 的旧记录）；
                                                                进程 pid=32614 自 16:50 冷启后未重启（同一 pid 存活）
```

## 四之二、静态未知项（非阻断，逐条登记）

- 菜单 `InputValue_` 控件在 Java 侧是否有长度上限未验（框架侧控件，native 侧 1..1000000 会夹取）；L3 若填 6–7 位数被截断，回 `classes4.dex` 查控件实现。
- 转生冷却是否另由服务端判定未验（客户端 `lastLunHuiTime` 判定已全部改掉；`ClearZhuanShengLengQue` 说明服务端知晓该字段）⇒ 必须连续转生两次验证。

## 五、CLAIM / VERDICT

| # | 声明 | VERDICT | 依据 / 缺口 |
|---|---|---|---|
| 1 | v11 客户端吃 2026-10-26 现网热更 DLL，hf4 精确命中 | **PROVEN** | 设备日志 `len=2028544 fnv=1bf6c08f1ba38809 → injected hf4` |
| 2 | 补丁 DLL 内含 item_double_x（1 处，缺省 1）与 zhuansheng_free（WindowOpen 2 处） | **PROVEN（静态）** | Cecil 校验 + 包内字符串断言 |
| 3 | 三项倍率按输入框填写值放大 | **NOT PROVEN** | 待用户 L3（输入 5 / 10 观察物品与经验） |
| 4 | 转生 1 小时冷却取消 | **NOT PROVEN（客户端静态已改）** | `lastLunHuiTime` 全库仅 `WindowOpen` 两处判定，`Turn()` 无前置校验；真机行为待 L3，**服务端是否另判未知**（需连续转生两次） |
| 5 | 15 项既有功能保留 | **PROVEN（静态）** | Entry 16 项（含 2 分类）+ so 断言；逐项行为沿用 2026-10-03 轮 L3 结论，本包未逐项回归 |
| 6 | 包与产物一致、签名正确 | **PROVEN** | 设备 base.apk md5 / 运行 so md5 双闭环 |

已知非本轮引入：`[OTA] 补丁签名校验失败` 在 2026-10-26 A 线各轮日志同样存在（框架 OTA 通道自检），与本包功能无关。

## 六、用户报障修正：菜单状态不保存（2026-10-26 17:00 起，根因定案 + 已修）

**现象**（用户原话）：「我重启后发现还是默认全关」。

**根因（已定案）**：首版包复用了 `templates/wxsj_menu_vc1637/classes4.dex`（2026-09-29 生成，源自 `templates/wxsj_menu_20260926/classes2.dex`），
该 dex 属 **2026-10-07 修复之前**的菜单代码：`ModBridge` 每次构造（菜单每次重开 / 每次启动）都调 `Preferences.clearFeaturePrefs()`，
把玩家刚点开的开关状态立刻抹掉。证据：该 dex 内 `clearFeaturePrefs` 存在，但 `clearFeaturePrefsIfListChanged` / `canonSig` / 日志串「功能列表变化」**全部不存在**
（2026-10-07 与 10-15 的模板 `hzsj_menu_20261007`、`bwxrk_menu_20261015` 里这三项都有）。
设备侧同源证据：`shared_prefs/starry_magic_prefs.xml` 里除 `auth_*` 外只有 `16`（防封）一个功能键，其余开关状态从未留存。

**为何 so 是新的、菜单却是旧的**：本轮 so 从当前源码现编（`wxsj_vc1637_so_only.sh`），而菜单 dex 走「沿用仓库模板」分支
（`build_wxsj_11.266018_vc1637.sh` 的第 4 步优先用 `templates/wxsj_menu_vc1637/classes4.dex`），模板本身停在 09-29。

**修法（走既有脚本，不改框架代码）**：
1. `./gradlew --offline assembleRelease` → 取 `app-release.apk` 的 `classes.dex`（510,100 B）。
   **必须用 release 包**：`MainActivity` 的自检入口由 `if (BuildConfig.DEBUG && …--ez starry_selftest…)` 守卫，release 被 javac 常量折叠后
   该串消失；debug 包会带上它，被 `check_release_apk` 判为「调试脚手架」拦下（实测 debug 版 classes2.dex 命中 `starry_selftest` ×1 → FAIL）。
   R8 侧 `-dontobfuscate` + `-keep class com.starry.magic.** { *; }` 保证类名可读、成员不被裁。
2. `WXSJ_SRC_TPL=<release dex 暂存目录> bash scripts/wxsj_vc1637_menu_tpl.sh` → `templates/wxsj_menu_vc1637/classes4.dex`
   **1,045,620 B / 197 类 / sha256 `4739f487c83d72ba82df0325f3b8febdce8a0416aa9c41aac0a1f399c12a6368`**（StarryApp.super 已是 Application，0 处改动；含「功能列表变化」×1、`starry_magic_prefs`×3、无 `starry_selftest`、无易盾引用）。
3. 重出 APK：`08_output/无限升级/无限升级_11.266018_vc1637_starry-releaseSigned.apk` md5 `8685133815c1b2bdf83800a8bdc1a79e`（so 未变 `6be6b884…`）。
4. 门禁：`check_release_apk` ✓（改前 FAIL：`starry_selftest` 命中）／`pre_install_gate` 5 PASS，其中 **`[JNI配套] 19 个动态注册方法名在 dex 中全部有声明`** —— 这是本轮第一次真正跑通「新 so × 新 dex」的 JNI 对齐检查（首版 dex 太旧时该项被跳过过一次）。
5. 覆盖安装（同签名、同 versionCode，数据保留）→ 设备 base.apk md5 闭环一致。

**修后机器侧验证（差分法，用户未点任何按钮）**：
- 修前基线：`starry_magic_prefs.xml` 只有 `16=true`；冷启后 ini 里 `block_report` 为 **0**（旧 dex 抹状态 + 本 mod 启动时写 0）。
- 修后连续两次冷启：`16=true` **仍在**（未被清）；ini `block_report=**1**`（由 prefs 还原后推回 native），
  且 `exp_multi=10000 / item_reverse_x=1000 / ads_reward_x=1000 / item_double_x=1000` 等**用户此前填的数值原样保留**；全程无「功能列表变化 → 已清功能偏好」日志。

**遗留提示（需用户一次性重填）**：ini 里那四个倍率值是旧 dex 时代写进去的，从未进过 SharedPreferences；
新 dex 的输入框按 prefs 恢复，所以界面会显示默认 1 而实际仍按 1000/10000 放大。
请用户在新包里把四个框按意愿重填一次，之后开关与数值都会自动记住。

## 七、最终产物身份（2026-10-26 17:24，菜单 dex 修复后重建）

| 项 | 值 |
|---|---|
| APK | `08_output/无限升级/无限升级_11.266018_vc1637_starry-releaseSigned.apk` md5 `2002fdac6e7c290e5d33ec8e2f02653f`（sha256 `3fafcb38…`） |
| 菜单 dex（classes4.dex） | 571,124 B / sha256 `2afbd1fb30d3ee0edb2c0e65a97a00f3794d7f59ad89080b47a425c5e2d73c9d` / 构建戳 `starry-menu:57a970d39406` |
| 模板 | `templates/wxsj_menu_current`（classes2.dex `c526f406…`，release 现编）→ 派生 `templates/wxsj_menu_vc1637` |
| so | 未变 `6be6b8841c44cc4f2bad383a5e393cd40506490d1b02d0f0d5bc32b80c51327d` |
| 设备内 dex 戳 | `starry-menu:57a970d39406`（装机后从设备 base.apk 取 classes4.dex 实测） |
| 中间态 | md5 `8685133815…`（debug 源 dex 的版本）已作废，不再作为交付身份 |

机制侧新增机器关（随本轮落地，规则见 `skill/starry-mod/SKILL.md`「菜单 dex 新鲜度铁律」）：
- `menu_tpl_gate.sh --tpl <模板目录> --dex-name classesN.dex`：模板过期 exit 1（实测旧模板 `wxsj_menu_20260926` 与「有 dex 无 build_info」目录均被拦）。
- `pre_install_gate.sh --menu-dex <成品APK> --menu-dex-name classes4.dex`：查10 PASS。
- 两个 wxsj 出包脚本模板分支已前置该门禁（过期直接 FAIL 并打印刷新命令）。

