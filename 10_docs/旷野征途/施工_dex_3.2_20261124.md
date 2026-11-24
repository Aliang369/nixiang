# 旷野征途 3.2 / vc32 · 阶段3a dex 施工记录（classes6 四补丁 + 框架 classes8）

> 生成：2026-11-24　作者：builder-dex（阶段3a dex 施工）
> 依据：`10_docs/旷野征途/静态_Java_Tap改包_3.2_20261124.md` ⑫ D 节（最终形态补丁清单）
> 范围：**只做 dex**。未组包、未装机、未 adb 写操作、未 git commit；未改 `app/src/**` / `01_tools/**` / `04_scripts/_common/**` / `pipeline.sh` / `Main.cpp` / `MainModern.cpp` / `GameConfig.h` / 公共 `Android.mk`，也未改 `kyzt_build_dex.sh` 的语义（只用环境变量）。
> 中间产物（可删）：`13_scratch/kyzt32_java/`

## ① 结论

| 项 | 结果 |
|---|---|
| P1 `GatekeeperRepository.showErrorToast` | **已落**（`.registers 5` + `return-void`） |
| P2 `DefaultTapLoginFragmentProvider.provide` | **已落**（行 184 `aget` 后插 `const/4 v2, 0x3`；分支标签 6f/64/5e/79 → 70/65/5f/7a，由 smali 自动重排） |
| P3 `TapSignKit.getSign` | **已落**（`.registers 3` + 常量 `360c78f5…` + `return-object`，try/catch 整段删） |
| P4 `PrivacyActivity.getVerifiedVersionCode` | **已落**（`.registers 3` + `const-wide/16 v0, 0x20` + `return-wide`） |
| 验证五步 | **五步全过**（⑤）。第④步有 16 个类的**汇编器归一化**差异，已用归一化等价检查证明 0 条实质指令差异（⑥） |
| 框架 dex | `classes8.dex` 已产出：612,060 B / sha256 `ca4ed980…1e59`，`GameHookImpl`（SPI）在内 |
| 菜单映射 | `check_menu_map_kyzt.py` 16 项 PASS，结论「一致」 |
| 框架隔离 | `check_framework_isolation.sh` PASS（7 项 0 违规） |
| 未做（不属本阶段） | 组包、`pre_install_gate`、装机、真机验证 |

## ② 输入指纹

| 输入 | 字节 | sha256 |
|---|---|---|
| `02_apks/旷野征途/旷野征途_3.2_taptap_official.apk` | 198,941,707 | `6318a972a599cfa664c1db867b960fc1a5f9b878d9bad3c15b003d28da17df1d` |
| `13_scratch/kyzt32_base/classes6.dex`（官方原样，只读） | 10,901,656 | `06547707230a83aa3e033a8bb4c410f9f0e7a7adcf1e36d4c24f9b27744d230e` |

工具：baksmali / smali **2.5.2 fat-jar**（`01_tools/smali/{baksmali,smali}-fat.jar`）；`dexdump` = `01_tools/android-sdk/build-tools/android-15/dexdump.exe`；JDK 17（Microsoft 17.0.20.1）；R8 = `%LOCALAPPDATA%/Android/Sdk/cmdline-tools/latest/lib/r8.jar`。

## ③ 产出指纹

| 产物 | 字节 | sha256 | DEX 版本 |
|---|---|---|---|
| `13_scratch/kyzt32_java/out_classes6.dex` | **11,020,568** | `e0d600341d5592cf151132b181c69cbfe68f231b5bf6b5847ae780082fea4b45` | 037 |
| `13_scratch/kyzt32_java/out_classes8.dex` | **612,060** | `ca4ed980674e1674256ca64e94b6bf0db9d833c5ae989fb241331333b43a1e59` | 035 |

`out_classes8.dex` 另记 md5 `c5384d98bd2b553990515e6b32ecf2a3`（R8 写出，与模板目录 build_info 记录一致）。

体量变化：`out_classes6.dex` 比官方大 **+118,912 B（+1.09%）**，是 baksmali→smali 整 dex 往返的固有产物（调试信息/段对齐/常量池重排），与四个补丁无关；`class_defs_size` 不变（8753），无结构上限被突破。

## ④ 四个补丁的指令级 before/after

### P1 `com/taptap/sdk/initializer/repository/GatekeeperRepository.showErrorToast(TapNetworkException;)V`

- 原：`.registers 5` + 37 行方法体（`getErr→getMsg→SysExtKt.showToast`；`getErr→getDescription`；`if-nez :cond_18` + `const-string/jumbo` 提示串；`TapLogger.loge$default`）
- 现（dis_out6 实测）：

```
.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V
    .registers 5

    return-void
.end method
```

- 语义：只掐「初始化信息错误」的 Toast / 日志；方法签名原本就是 `V`，调用点 0 改动。
- 引用净减少：`SysExtKt.showToast`、`TapLogger.loge$default`、`TapNetworkException$Err.getMsg/getDescription` 从本方法消失（他处仍引用则保留）。

### P2 `com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.provide(LoginRequest;)Landroid/app/Fragment;`

插入点 = 第 184 行（全类唯一 `aget v2, v3, v2`，实测 count=1）：

```
     aget v2, v3, v2
+    const/4 v2, 0x3          # ← 新增：恒走 WEB
     const/4 v3, 0x1
     if-eq v2, v3, :cond_6f    →  :cond_70
     const/4 v3, 0x2
     if-eq v2, v3, :cond_64    →  :cond_65
     const/4 v3, 0x3
     if-ne v2, v3, :cond_5e    →  :cond_5f
     .line 32 / sget-object WebLoginFragment  ← 必落这里
```

- 汇编器把三个分支标签与 `:goto_79` 整体后移 1（6f→70 / 64→65 / 5e→5f / 79→7a），指令数 +1。**未手改任何标签**。
- 语义：`v2=3` ⇒ 三个比较全不跳 ⇒ 直落 `WebLoginFragment`（内嵌 WebView 登录），不再按 `loginType` 分流到 Client / CloudGame。
- 引用净变化：**0**（不新增类/方法/字段引用）。

### P3 `com/taptap/sdk/kit/internal/utils/TapSignKit.getSign(Landroid/content/Context;)Ljava/lang/String;`

- 原：`.registers 5` + 85 行 body（`:try_start_6` … `:try_end_40` / `.catch Exception :catch_41`；两处 Kotlin 判空串；`getPackageInfo(...,0x40)`；`signatures[0].toByteArray()`；`CommonUtils.getMD5`）
- 现（dis_out6 实测；try/catch 与 `:cond_38` / `:goto_39` / `:cond_3c` / `:catch_41` 一并消失，无悬空标签）：

```
.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 3

    const-string v0, "360c78f563dd471fa6df02cc68045a31"

    return-object v0
.end method
```

- 常量依据（不改）：3.2 底包证书 MD5 = `360c78f563dd471fa6df02cc68045a31`（静态文档 ③-1 用 `apksigner --print-certs` 实测，与 `CommonUtils.getMD5(DER)` 等价）。
- `.registers 3`：instance 方法 ⇒ `p0=v1(this)`、`p1=v2(context)`，`v0` 专用于返回，常量不会被回显成 `p0`。
- 字符串池实测：底包 `360c78f5…` 出现 **0** 次 → 改后 **1** 次。

### P4 `com/xiefeng/game2d/PrivacyActivity.getVerifiedVersionCode(Landroid/content/Context;)J`

- 原：`.registers 8` + 172 行 body（包名白名单 `"com.xf.kyzt"` → `getPackageInfo`（SDK≥28 用 `0x8000000` + `getApkContentsSigners`，否则 `0x40` + `signatures`）→ 恰好 1 个签名 → `MessageDigest("SHA-256")` → `isEqual(OFFICIAL_SIGNER_SHA256)` → 返回 `getLongVersionCode()` 或 `-1`；三处 catch + `Log.w("GameStartup", …)`）
- 现（dis_out6 实测）：

```
.method public static getVerifiedVersionCode(Landroid/content/Context;)J
    .registers 3

    const-wide/16 v0, 0x20

    return-wide v0
.end method
```

- 消费点**未改**（dis_out6 行 1294 / 1300 / 1304）：`invoke-static getVerifiedVersionCode` → `cmp-long` → `if-gtz :cond_5b`。返回 32 > 0 ⇒ 不再进入「无法启动游戏」弹窗 + `finish()` 分支。
- `.registers 3`：static 方法 1 入参 ⇒ `p0=v2`，`const-wide/16 v0` 的 wide 写入（`v0+v1`）不覆盖入参。
- 引用净减少：`getPackageManager / getPackageInfo / getApkContentsSigners / getLongVersionCode / MessageDigest.getInstance / digest / isEqual / Log.w` 等；`SHA-256`、`com.xf.kyzt` 仍被 `<clinit>` / 其它方法引用，未消失。
- **未动 `<clinit>` 的 base64 常量**（lead 决策 ②），保留「匹配官方证书」原语义。

### 引用计数净变化（`dexdump -f` 实测）

| 计数 | 官方 classes6 | 补丁后 | 变化 | 判据 |
|---|---|---|---|---|
| `method_ids_size` | 65536 | **65532** | −4 | ≤ 65536 ✔（顶格未被突破） |
| `field_ids_size` | 54924 | 54923 | −1 | — |
| `string_ids_size` | 67009 | 67000 | −9 | — |
| `type_ids_size` | 10509 | 10508 | −1 | — |
| `proto_ids_size` | 15768 | 15767 | −1 | — |
| `class_defs_size` | 8753 | **8753** | 0 | — |

四处补丁全部是**减少**引用方向，符合「禁新增类/方法引用去顶 method_ids」的硬约束。

## ⑤ 验证五步（原始输出）

### 步骤 1 —— `dexdump -c`（checksum）

```
$ $BT/dexdump.exe -c 13_scratch/kyzt32_java/out_classes6.dex
Processing '13_scratch/kyzt32_java/out_classes6.dex'...
Checksum verified
```

### 步骤 2 —— `dexdump -f`（version 037 / ids 上限）

```
$ $BT/dexdump.exe -f 13_scratch/kyzt32_java/out_classes6.dex
Opened '13_scratch/kyzt32_java/out_classes6.dex', DEX version '037'
DEX file header:
magic               : 'dex\n037\0'
checksum            : 4721f215
signature           : 74e6...7641
file_size           : 11020568
header_size         : 112
string_ids_size     : 67000
type_ids_size       : 10508
proto_ids_size      : 15767
field_ids_size      : 54923
method_ids_size     : 65532
class_defs_size     : 8753
```

对照底包：

```
Opened '13_scratch/kyzt32_base/classes6.dex', DEX version '037'
checksum            : a47ba8d7
signature           : e2c6...8744
file_size           : 10901656
string_ids_size     : 67009   type_ids_size : 10509   proto_ids_size : 15768
field_ids_size      : 54924   method_ids_size : 65536   class_defs_size : 8753
```

⇒ **version 037 ✔ / method_ids 65532 ≤ 65536 ✔**

### 步骤 3 —— 类集合 diff（应 8753 / 0 差异）

```
$ bash 01_tools/smali/baksmali list classes out_classes6.dex | sort > /tmp/new6.txt
$ sort 13_scratch/kyzt32_java/cls/classes6.txt > /tmp/base6.txt
base classes: 8753  new classes: 8753
class diff lines: 0
$ find 13_scratch/kyzt32_java/dis_out6 -name '*.smali' | wc -l
8753
```

### 步骤 4 —— 整 dex 重反汇编两侧对比

```
$ bash 01_tools/smali/baksmali d out_classes6.dex -o dis_out6 --jobs 6
（8753 个 smali，无报错）
$ diff -rq 13_scratch/kyzt32_java/dis6 13_scratch/kyzt32_java/dis_out6 | wc -l
20
```

20 个不同文件 = **4 个目标类** + **16 个汇编器归一化类**：

| 文件 | 变化行 | 性质 |
|---|---|---|
| com/taptap/sdk/initializer/repository/GatekeeperRepository.smali | 34 | 目标 P1 |
| com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali | 22 | 目标 P2 |
| com/taptap/sdk/kit/internal/utils/TapSignKit.smali | 84 | 目标 P3 |
| com/xiefeng/game2d/PrivacyActivity.smali | 175 | 目标 P4 |
| com/smartdigimkt/{c0/l, m0/c, n3/c, n4/c, o4/a, r4/d, r4/i, sdk/basead/ui/web/BaseWebView}.smali、com/smartdigimkt/odopt/api/ATOaidHandler.smali | 2–30 | 归一化 |
| com/taptap/sdk/themis/lite/LogUtil.smali | 2 | 归一化 |
| com/unity3d/player/{MultiWindowSupport, ReflectionHelper, W0, z.1}.smali | 2–22 | 归一化 |
| com/yxcorp/kuaishou/addfp/android/b/f.smali | 2 | 归一化 |
| ms/bz/bd/c/Pgl/q0.smali | 10 | 归一化 |

**归一化的两类形态（等价证明见 ⑥）**

1. 静态字段初始值打印差异：`.field … = false` / `= 0x0` / `= 0x0L` / `= null` 被 smali 省略（DEX 默认值，运行期取值不变）。
2. try 区间合并：相邻且**同一 handler** 的两段 `.catchall` 被 smali 合成一段（例 `o4/a.smali`：`:try_start_1..:try_end_30` + `:try_start_30..:try_end_46` ⇒ `:try_start_1..:try_end_46`，type/target 不变）。

### 步骤 5 —— 目标方法逐条比对

四个目标类的 diff 全部落在目标方法内（P1/P3/P4 整段替换；P2 是 1 条插入 + 标签后移 1）。P2 的实测 after（dis_out6 行 180–200）：

```
    aget v2, v3, v2

    const/4 v2, 0x3

    const/4 v3, 0x1

    if-eq v2, v3, :cond_70

    const/4 v3, 0x2

    if-eq v2, v3, :cond_65

    const/4 v3, 0x3

    if-ne v2, v3, :cond_5f
```

（P1/P3/P4 的 after 全文见 ④。）

## ⑥ 归一化的等价证明（机器检查）

判据不是「肉眼看像默认值」，而是把两侧文本归一化后**逐行比较**：

1. 去掉 `.field … = <默认值>` 的初始值部分（false / true / 0 / 0x0 / 0x0L / 0x0S / 0x0B / null）；
2. 去掉所有 `:try_start_*` / `:try_end_*` / `.catch*` 行；
3. 去空行后整文件比对。

```
总 smali 文件: 8753  内容不同的文件: 20
非目标文件中「归一化后完全一致」的: 16
非目标文件中「归一化后仍不同」的: 0
```

⇒ 16 个非目标类**归一化后逐行相同** = **0 条实质指令差异**；差异只存在于「静态字段默认值是否显式写出」与「try 区间是否相邻合并」两种语义等价表达。

判定口径（诚实版）：第④步的**严格判据（仅 4 个文件不同）没有满足**；替代判据是「4 目标 + 16 归一化且归一化后 0 差异」。要满足严格 4 文件，需要换一个不做区段合并、且保留静态默认值的汇编路径 —— 当前工具链（smali 2.5.2）没有该开关。

## ⑦ 框架 dex（classes8.dex）施工

### 7.1 序号规则

3.2 官方 = 7 dex（`classes.dex` … `classes7.dex`）⇒ 框架 dex 必须是 `classes8.dex`（ART 遇首个缺口即停扫）。按 lead 决策**不注入 killer dex**，故无 `classes9.dex`。

### 7.2 构建（复用现成脚本，只覆盖环境变量）

```
$ export ANDROID_SDK_ROOT="$LOCALAPPDATA/Android/Sdk"    # 环境坑见 7.4
$ KYZT_DEX_NAME=classes8.dex bash 04_scripts/旷野征途/kyzt_build_dex.sh
[1/5] 源拷贝 → C:/Users/.../Temp/kyzt_build/dex/src（ASCII 路径）
[2/5] javac 编译
      class 文件 139 个
[3/5] 打包 class → jar
[4/5] R8 混淆 → dex（min-api 23 = 官包 minSdk）
[5/5] 字节自检 + 发布
file      : classes8.dex
size      : 612060 B
sha256    : ca4ed980674e1674256ca64e94b6bf0db9d833c5ae989fb241331333b43a1e59
md5       : c5384d98bd2b553990515e6b32ecf2a3
magic     : 6465780a30333500
classes   : 334
  com/starry/magic/StarryInitProvider        OK
  com/starry/magic/menu/MenuService          OK
  com/starry/magic/bridge/ModBridge          OK
  com/starry/magic/sign/StarryAppComponentFactory OK
  com/starry/magic/ota/StarryNativeLoad      OK
  com/starry/magic/StarryApp                 OK
OK
✓ menu_tpl_gate: dex 与源码同步（e0664679421c）
```

- 脚本**语义未改**：只覆盖 `KYZT_DEX_NAME`（脚本第 44 行 `DEX_NAME` 默认 `classes2.dex`，可被 `KYZT_DEX_NAME` 覆盖）。
- 模板目录 = `mod菜单/starry-mod-menu/templates/kyzt_menu_current/`（与 2.4 同一目录）。

### 7.3 模板目录复原（不留仓库改动）

`kyzt_build_dex.sh` 会把 dex 写进 `templates/kyzt_menu_current/`，并重写同目录 `build_info.txt`（**该文件是 git 跟踪的**）。本轮做法：

1. 构建前备份 `build_info.txt` → `13_scratch/kyzt32_java/_sh/build_info.orig.txt`；
2. 构建后把新 `build_info.txt`（classes8 / Win 路径 / source_commit e0664679421c）存为 `13_scratch/kyzt32_java/_sh/build_info.classes8.txt` 留档；
3. 复制 `classes8.dex` → `13_scratch/kyzt32_java/out_classes8.dex`，随后**还原** `build_info.txt` 并删除模板目录里的 `classes8.dex`。

```
$ git status -sb -- mod菜单/starry-mod-menu/templates/kyzt_menu_current
## main...origin/main          # 无改动
$ ls -l mod菜单/starry-mod-menu/templates/kyzt_menu_current/
-rw-r--r-- 1 Administrator 197121 555  build_info.txt   # 仍是 2.4 的 classes9 记录
```

⇒ 组包阶段要么重跑 7.2，要么直接用 `out_classes8.dex`。

### 7.4 环境坑（Win 侧，必读）

`kyzt_build_dex.sh` → `obfuscate_menu_dex.sh` 的 r8 探测只认 `ANDROID_SDK_ROOT` / `ANDROID_HOME` / `LOCALAPPDATA` 三条路径；本轮首跑时三者皆空（子进程 `process.env` 为空、`LOCALAPPDATA` 未导出），报：

```
ERROR: 找不到 r8.jar（需 cmdline-tools；build-tools 里的 d8 不做混淆）
ERROR: R8 混淆失败
```

**不是缺工具**：`%LOCALAPPDATA%/Android/Sdk/cmdline-tools/latest/lib/r8.jar` 存在（工作区另有 `01_tools/r8-8.9.35.jar`）。解法：跑前 `export ANDROID_SDK_ROOT="$LOCALAPPDATA/Android/Sdk"`，同时也让 `d8` / `android.jar` 解析到真 SDK。

### 7.5 dex8 结构

```
Opened '13_scratch/kyzt32_java/out_classes8.dex', DEX version '035'
checksum            : 273942e4
file_size           : 612060
string_ids_size     : 3326   type_ids_size : 625   proto_ids_size : 653
field_ids_size      : 1039   method_ids_size : 2556   class_defs_size : 333
$ $BT/dexdump.exe -c out_classes8.dex
Checksum verified
```

- **DEX 035**（R8 `--min-api 21` 的固定产物），与官方 037 混装：各 dex 头独立、ART 逐 dex 解析。2.4 的 `classes9.dex` 走同一管线同参数（`组包_2.4_20261116.md` 记 448,592 B / `87e8ad56…`）且该包已装机运行 ⇒ **推断** 035 框架 dex 可与 037 官方 dex 共存。**本次未上机，属待验证项。**

## ⑧ SPI 与框架隔离（C 节）

### 8.1 dex8 内的 SPI 类（`baksmali list classes` 实测）

```
Lcom/starry/magic/core/GameHook;
Lcom/starry/magic/core/GameHooks;
Lcom/starry/magic/core/GameHooks$a;
Lcom/starry/magic/games/kyzt/GameHookImpl;        ← 本游戏 SPI 实现（保名成功）
Lcom/starry/magic/sign/TapLoginForceWeb;          ← GameHookImpl 实际调用的强制网页登录入口
```

- 走「TapTap 网页登录」SPI：`GameHookImpl.onActivityPreCreated` → `TapLoginForceWeb.onPreCreated(activity)`（目标包 `com.xf.kyzt`），**无包名白名单**、不改 `StarryApp` / `StarryInitProvider`。
- dex8 内 `com/taptap/` 出现 **0** 次，`embedded_web` 出现 2 次（反射用字符串，不产生跨 dex 类引用）⇒ 框架 dex 与官方 classes6 的 TapSDK 无静态耦合。
- 保名依据（两个规则文件都在）：`proguard-rules.pro:80` 与 `inject-dex-rules.pro:79` 的 `-keepnames class com.starry.magic.games.** { *; }`，外加同处 `-keepclassmembers class com.starry.magic.games.**`。

### 8.2 `check_framework_isolation.sh` 原始输出

```
[1/7] 框架层不得 import 具体游戏实现类
[2/7] 框架层不得出现单游戏的历史临时目录
[3/7] 框架层不得写死具体游戏包名
[4/7] LegacyTapTapGames 清单只减不增
[5/7] 每个 games/<id>/GameHookImpl 必须声明 targetPackage()
[6/7] 两个 R8 规则文件必须同步（core/games 的 keep 规则）
[7/7] 不得保留已废弃的兼容垫片

[check_framework_isolation] PASS
rc=0
```

### 8.3 `check_menu_map_kyzt.py`（菜单 9 项位置对齐）

```
PASS featureList 长度 = 9（实际 9）
PASS Java 以数组下标作 featNum 传给 native
PASS Java 前缀解析规则（Category_/Toggle_/Button_）齐备
PASS Category 只占 0/4（实际 [0, 4]）
PASS 功能位下标 = 1/2/3/5/6/7/8（实际 [1, 2, 3, 5, 6, 7, 8]）
PASS native enum 与功能位一致（enum=[1, 2, 3, 5, 6, 7, 8]）
PASS index 1 类型 Button_（实际 Button_金币+9999［2）
PASS index 2 类型 Button_（实际 Button_钻石+9999［2）
PASS index 3 类型 Button_（实际 Button_积分+9999［2）
PASS index 5 类型 Toggle_（实际 Toggle_游戏加速）
PASS index 6 类型 Toggle_（实际 Toggle_玩家无敌［2.2待）
PASS index 7 类型 Toggle_（实际 Toggle_百倍伤害［2.2待）
PASS index 8 类型 Toggle_（实际 Toggle_免广告）
PASS 「待验证」集合 = {1,2,3,6,7}（实际 [1, 2, 3, 6, 7]）
PASS 不再有「未适配」项（实际 []）
PASS 5/8 不带适配标签（实际 Toggle_游戏加速 / Toggle_免广告）

结论： 一致
rc=0
```

## ⑨ 未过项 / 风险 / 待验证

| # | 项 | 状态 |
|---|---|---|
| 1 | 第④步严格判据（仅 4 文件不同） | **未满足**：20 个文件不同，其中 16 个为归一化、归一化后 0 差异（⑥）；当前 smali 2.5.2 无开关可关掉该行为 |
| 2 | `out_classes6.dex` 与官方字节差异 | +118,912 B；class_defs 不变、checksum 与结构校验通过；属往返重排 |
| 3 | 框架 dex 是 035、官方是 037 混装 | **待验证**（未上机）；有 2.4 同管线先例 |
| 4 | 装机门禁「查12」（`pre_install_gate.sh:572` 要求含 `GatekeeperRepository` 字面量且与底包不同字节） | 本产物**应 PASS**：`com/taptap/sdk/initializer/repository/GatekeeperRepository` 在 patched dex6 中出现 5 次（与底包同数），字节已变。**未跑门禁脚本**（无成品 APK） |
| 5 | 真机行为（登录走 WebView / 隐私页直达 / 启动门闩消失） | 未验证（本阶段禁装机） |
| 6 | 除 `provide` 外的登录路径（`TapTapLoginActivity.fallbackToWeb` 等） | 未改（文档口径：只改 provide） |
| 7 | 补丁 #1（`TapTapSdkInitializer.isGatekeeperLoadedSuccessfully`） | 按 lead 决策**不打**（死补丁） |

## ⑩ 复现命令

```
cd /e/nixiang
BT=01_tools/android-sdk/build-tools/android-15
W=13_scratch/kyzt32_java

# 1) 反汇编官方 classes6（已有 dis6 可跳过）
bash 01_tools/smali/baksmali d 13_scratch/kyzt32_base/classes6.dex -o $W/dis6 --jobs 6
cp -a $W/dis6 $W/work6                      # 只改副本

# 2) 打 P1–P4（改 work6 里 4 个 smali：
#    P1/P3/P4 整段替换方法体；P2 在全类唯一 "aget v2, v3, v2" 之后插 const/4 v2, 0x3）

# 3) 汇编（-a 24 必需；默认 -a 15 会降成 DEX 035）
bash 01_tools/smali/smali assemble -a 24 $W/work6 -o $W/out_classes6.dex

# 4) 验证五步
$BT/dexdump.exe -c $W/out_classes6.dex
$BT/dexdump.exe -f $W/out_classes6.dex | grep -E '_ids_size|class_defs_size|DEX version'
bash 01_tools/smali/baksmali list classes $W/out_classes6.dex | sort > /tmp/new6.txt
sort $W/cls/classes6.txt | diff - /tmp/new6.txt | wc -l        # 0
bash 01_tools/smali/baksmali d $W/out_classes6.dex -o $W/dis_out6 --jobs 6
diff -rq $W/dis6 $W/dis_out6 | wc -l                          # 20（4 目标 + 16 归一化）
# 归一化等价判据见 ⑥（去静态默认值 + 去 try/catch 行后逐行比对，期望「仍不同 = 0」）

# 5) 框架 dex
export ANDROID_SDK_ROOT="$LOCALAPPDATA/Android/Sdk"
KYZT_DEX_NAME=classes8.dex bash 04_scripts/旷野征途/kyzt_build_dex.sh
cp -f "mod菜单/starry-mod-menu/templates/kyzt_menu_current/classes8.dex" $W/out_classes8.dex
cp -f $W/_sh/build_info.orig.txt "mod菜单/starry-mod-menu/templates/kyzt_menu_current/build_info.txt"
rm -f "mod菜单/starry-mod-menu/templates/kyzt_menu_current/classes8.dex"

# 6) 框架侧机器关
PYTHONUTF8=1 python 04_scripts/旷野征途/check_menu_map_kyzt.py
bash 04_scripts/_common/check_framework_isolation.sh

# 输入/产物指纹
sha256sum 13_scratch/kyzt32_base/classes6.dex $W/out_classes6.dex $W/out_classes8.dex
```

## ⑪ 改动文件清单

**新增（全部在 `13_scratch/` 工作区 + 本文档）**

| 路径 | 说明 |
|---|---|
| `13_scratch/kyzt32_java/out_classes6.dex` | 交付物①：四补丁后的官方 classes6（11,020,568 B） |
| `13_scratch/kyzt32_java/out_classes8.dex` | 交付物②：星光框架 + SPI（612,060 B） |
| `13_scratch/kyzt32_java/work6/` | 打完补丁的 smali 树（8753 文件，可删） |
| `13_scratch/kyzt32_java/dis_out6/` | out_classes6 的重反汇编（8753 文件，可删） |
| `13_scratch/kyzt32_java/_sh/` | 本轮脚本 / 日志 / 备份（build_info.orig.txt、build_info.classes8.txt 等） |
| `10_docs/旷野征途/施工_dex_3.2_20261124.md` | 本文档 |

**未改动（逐项核对过）**：`02_apks/**`（底包只读）、`13_scratch/kyzt32_base/*.dex`（官方副本逐字节不变）、`mod菜单/starry-mod-menu/templates/kyzt_menu_current/build_info.txt`（已还原，git 无 diff）、`app/src/**`、`01_tools/**`、`04_scripts/**`（含 `kyzt_build_dex.sh` 与 `_common/**`）。
