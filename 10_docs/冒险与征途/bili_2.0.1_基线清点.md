# 冒险与征途 · B站渠道 2.0.1 原版静态基线清点

> 生成：2026-11-16（scout，只读取证）
> 范围：仅静态清点与旧版对照。**未改任何源码、未装机、未卸载、未清数据、未做 RVA/符号恢复**（AGENTS.md §6 阶段闸门）。
> 证据命令一律用绝对路径 adb/aapt2/apksigner；解包临时目录 `13_scratch/bili_201`（951 MB，未删）。

## 1. 包信息

| 项 | 值 |
|---|---|
| 拉取产物 | `02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk` |
| 大小 | 907,126,398 B（865.1 MiB）；zip 条目 6,241；解压总计 1,023,878,588 B |
| md5 | `56128ac55012c7a9d3cd881776777170` |
| sha256 | `1868b782eb0986313993c93425f4c32df22578e11e23c349fd0c1c96ae3950aa` |
| 设备侧路径 | `/data/app/~~1GQ0yPGF7xGB0hWFrXQTbQ==/com.my.mxyzt.bilibili-i-9TqIK-1rEU7NPakfWB-w==/base.apk` |
| split | **仅 base.apk，无 split**（`pm path` 单行输出已确认） |
| package | `com.my.mxyzt.bilibili` |
| versionName / versionCode | 2.0.1 / 201 |
| minSdk / targetSdk / compileSdk | 22 / 33 / 32 |
| 应用名 | 冒险与征途 |
| application / 主 Activity | `com.hdwj.afk.MainApplication` / `com.hdwj.afk.MainActivity` |
| ABI | 仅 `arm64-v8a`（`lib/` 下只此一个 ABI，21 个 .so） |
| extractNativeLibs / debuggable / allowBackup | true / false / false |
| 签名 DN | `CN=Bbcallen, OU=danmaku.tv, O=danmaku.tv, L=Zhuhai, ST=Guangdong, C=CN`（B站官方签名） |
| 证书 SHA-256 / SHA-1 / MD5 | `93ba270f5521139ecafe4bb638ac5b1198bc548f62d9fd8f8580a079faf5910e` / `96dc605b9519bab94eddbeaaa059a669fba2c211` / `7194d531cbe7960a22007b9f6bdaa38b` |
| 签名方案 | v1 = true、v2 = true、v3 = true；v3.1 / v4 = false |
| 引擎指纹 | `META-INF/com.android.games.engine.build_fingerprint` = `2022.3.14f1;IL2CPP;Release;StripEngineCode:1;OptimizedFramePacing:0` |

## 2. 顶层结构与 dex

| 区域 | 条目数 | 说明 |
|---|---|---|
| assets | 5072 | 其中 `assets/dataroot` 4888（见 §5） |
| res | 1111 | 资源 |
| META-INF | 29 | 含 `BILI_KEY.SF/RSA`、`MANIFEST.MF` |
| lib | 21 | 仅 arm64-v8a |
| 其余 | — | `resources.arsc` 742,196；`AndroidManifest.xml` 61,788；`okhttp3/internal/publicsuffix/*`（2 条目，SDK 误打包资源，无代码） |

| dex | 大小 | 类数(class_defs) | md5 |
|---|---|---|---|
| classes.dex | 8,341,908 | 6,908 | `496e902ccfe1513c3237ce18eb57ef9a` |
| classes2.dex | 7,618,504 | 6,819 | `6829c35fecbe75eabe9f6f0afd6406e1` |
| classes3.dex | 1,642,972 | 1,345 | `79f9c22aef769aebae78839c8b7e9803` |

三个 dex 魔数均为 `dex\n035\0`（明文），`dexdump -f` 正常解析 ⇒ Java 层未加密、可反编译。
Java 层主要包：广告/统计 `com.sigmob.sdk`(558)、`com.windmill.sdk`(233)、`com.gsc.*`(腾讯系 GSC 登录/支付, ~400+)、`com.biligame.gsc`(22)、`com.alipay.sdk`(261)、`com.bsgamesdk.android`(109)、`com.sina.weibo`(81)、`com.tencent.open`(80)、OAID `com.bun.miitmdid`(52)、B站插件容器 `com.base.bgcplugin`；游戏自身 Java 壳 `com.czhj.*`、`com.hnyl.core`、`com.mxyzt.channel`、`com.hdwj.afk`。

## 3. lib/arm64-v8a 清单（21 个）

| .so | 大小(B) | md5 |
|---|---|---|
| **libil2cpp.so** | **87,013,984** | **`2e088d84e71273516de0403f490140ed`** |
| libunity.so | 21,037,864 | `fc71482efa64852cb353f0dc2a9f2bbe` |
| libgmesdk.so | 5,611,368 | `cb3b49fb8de805d206ab75454fb069dd` |
| libcryptopp.so | 4,960,128 | `ba4d2ffda884ac3ce215c9853ab909a7` |
| libcrypto_1_1.so | 2,590,088 | `f8d42824c1465e5fe0c0452f205a96aa` |
| libgmeogg.so | 1,007,224 | `a903ed443beeb772cbb68d0250464eea` |
| libcustom_rules.so | 855,416 | `58565ee7d848a451276d4790eb8523c8` |
| libgmefdkaac.so | 828,472 | `c7425668de608e7792b31ae657ba7765` |
| libsmsdk.so | 826,104 | `6bf3bbc7680d78403110e002e1a5436b` |
| libmsaoaidsec.so | 688,128 | `3068263e206187a37ef9676d5afb1112` |
| libssl_1_1.so | 563,088 | `bdcc27df438ab87aa82e4dcb65981ee3` |
| libgmelamemp3.so | 502,864 | `5948de67c1aff2592f309eff5c816e21` |
| libgmefaad2.so | 452,072 | `ad197d48ea7d5a502613e140d73ba1ab` |
| libmsaoaidauth.so | 421,888 | `ed5f1ac3616db1f40367ebc097f968f8` |
| libgmesoundtouch.so | 240,696 | `c34705bbbdabc5f968c11f190f015fcf` |
| libenet.so | 46,888 | `929f10ddc97d70acb0278536992ea580` |
| libweibosdkcore.so | 26,136 | `ba1fc90a432e7b79d6f7566b3160d35c` |
| libandfix.so | 13,928 | `559410a335b27a18575949eb962ed82c` |
| libbsutils.so | 9,904 | `4209bf259f3edd5e1884e5b9828912c1` |
| libmain.so | 6,712 | `3767633ca3f71b76f0d55d537a6c8b4d` |
| libsharewind.so | 67,608 | `b3a61a6785a1c3b1295ca52e6593ec3f` |

`libil2cpp.so` 在包内是 Deflate 存储（csize 22,813,559）。导出符号 2,388 个，其中 `il2cpp_*` 243 个。

## 4. 加固判定：**未加固**

| 判据 | 结果 |
|---|---|
| 加固 so | 无 `libtprt` / `libDexHelper` / `libjiagu` / `libshella` / `libsecexe` / `libsecmain` / `libSecShell` / `libmobisec`（zip 名单与 dex 字符串双查，0 命中） |
| dex 加密 | 无：3 个 dex 全 `dex\n035` 明文，`dexdump` 可解析 |
| 壳入口 | 无：manifest application 是游戏自身 `com.hdwj.afk.MainApplication`，非 StubApp 类 |
| 易混淆项 | `libmsaoaidsec.so`/`libmsaoaidauth.so`＝移动安全联盟(MSA) OAID 设备标识，**非加固**；`libandfix.so`＝阿里 AndFix **Java 热修复**运行时（`androidsupportmultidexversion.txt` 同源），**非加固** |
| 未确认 | `libcustom_rules.so`(855KB) / `libsharewind.so`(67KB) / `libbsutils.so`(9.9KB) 的归属（疑为渠道/风控/SDK 组件）——未做 IDA，缺证据：库内字符串溯源或符号分析 |

## 5. IL2CPP 判定：**是（IL2CPP + HybridCLR 分支构建）**

| 项 | 值 |
|---|---|
| metadata | `assets/bin/Data/Managed/Metadata/global-metadata.dat`，13,324,172 B，md5 `7d177446104ca55942b666ecf09b1c35` |
| metadata 头 | magic `0xFAB11BAF` 合法，version `0x1d`(29) |
| il2cpp 导出 API | `il2cpp_class_from_name` / `il2cpp_method_get_name` / `il2cpp_runtime_invoke` / `il2cpp_domain_get` / `il2cpp_assembly_get_image` / `il2cpp_class_get_methods` / `il2cpp_field_get_offset` / `il2cpp_image_get_class_count` / `il2cpp_thread_attach` 全部为 `T`（已导出）⇒ modern 注册表可静态解析，无需字符串硬找 |
| HybridCLR 证据 | `libil2cpp.so` 内残留构建路径 `/Volumes/work/afk/release/work/HybridCLRData/LocalIl2CppData-OSXEditor/il2cpp/libil2cpp/icalls/...` ⇒ 由 HybridCLR 分支的 il2cpp 构建（与旧版同一路线） |
| Unity | 2022.3.14f1（Release，StripEngineCode:1） |
| Managed 内容 | 仅 `Metadata/global-metadata.dat` + `Resources/*.dll-resources.dat`（无托管 dll） |

**未确认**：本版热更方法是否仍全部指向解释器共享桩（旧版 `libil2cpp+0x207F04C` 桩、入口 RVA `0x207F798`/push-frame `0x207DCCC`）。缺证据＝RVA 定位与 dump.cs，属 AGENTS.md §6「开始做功能」闸门后的工作，本轮未做。

## 6. 热更 / 外置资源线索

| 项 | 值 |
|---|---|
| `assets/dataroot` | 828,057,214 B（789.7 MB），4888 文件：`3714 .unity3d` + `1172 .bytes` + `20 .mp4` + `16 info/*.json` + txt |
| bundle 加密 | **无**：头 `55 6e 69 74 79 46 53 … 2022.3.14f1`（UnityFS 5.x 明文）；`.bytes` 内容头即 protobuf 明文字段（如 `\x0a\x0bHurtGroup_0`） |
| 热更地址 | `assets/dataroot/info/version_mxyzt.json` → `ResUrl=https://myscdn.gzmywl.cn/patch/wechat/`、`TreeUrl=https://mys-login.gzmywl.cn/afk0406.json`；`ver.json` = `fileVersion 201 / logicVersion 2.0.1 / platform Channel / patchVersion 0` |
| 程序集热更 | 包内 **0 个 `.dll`/`.dll.bytes`** ⇒ 热更程序集不在包内（运行时按 ResUrl 拉取，**未确认**；缺证据：首启后 `/data` 目录或 ResUrl 清单） |
| 渠道配置 | `assets/plugin_bilibili.properties`（merchantId 4205 / appId 12665 / serverId 11800）、`assets/biligame_package_info.json`、`assets/distributor.txt`= `1`、`assets/yl_channle_config.properties`（头条/百度/GDT/快手/UC/wx appid）、`assets/gsc_plugins` |
| assets/bin/Data | 137 文件 23,764,217 B：110 个 32-hex 命名资源块 + `sharedassets*.assets.splitN` + `ScriptingAssemblies.json` |

## 7. 与旧版对照（旧＝TapTap `com.mxyzt.gzmy` 1.5.9 / vc159）

> 旧版口径来源：`10_docs/冒险与征途/`（`斌哥包抓点_20261002.md` §包结构、`验收卡_v44`、`防御加固落地_20261027`、`_state/games/冒险与征途.md`）＋ `06_dumped/冒险与征途/apk_extract/_md5.txt`。
> **重要限制**：旧官方底包 `冒险与征途_1.5.9_official_base.apk`（883,770,021 B）**已不在本机**（`02_apks/冒险与征途/` 本轮才新建；`06_dumped` 那份是"斌哥包"＝官方底包＋斌哥壳）。因此 so/dex 逐一对比的旧侧基准＝**斌哥包减去已知壳件**（`libSDKloading.so` 3,893,752B、`libtoolChecker.so` 5,512B 为斌哥注入）。

| 维度 | 旧 1.5.9（TapTap） | 新 2.0.1（B站） | 同/异 |
|---|---|---|---|
| 包名 / vc | `com.mxyzt.gzmy` / 159 | `com.my.mxyzt.bilibili` / 201 | **异** |
| 签名 | 渠道官方签名；本机**无旧官方证书摘要记录**（只记了斌哥 `CN=binge2.keystore` / SHA-256 `11491cb6…`） | `CN=Bbcallen, OU=danmaku.tv` / SHA-256 `93ba270f…` | **异**（异签名⇒不可覆盖安装；旧侧摘要**未确认**，缺证据＝旧官方底包或旧装机） |
| APK 大小 | 883,770,021 | 907,126,398 | +23,356,377 B（+2.6%） |
| so 数量 | 19（斌哥包 21 − 壳 2） | 21 | **异（+2）** |
| so 名称集合 | 旧独有 7：`libPglbizssdk_ml`、`libavmdl_lite`、`libpanglearmor`、`libpangleflipped`、`libtobEmbedEncrypt`、`libtt_ugen_layout`、`libttmplayer_lite`（TapTap 登录＋穿山甲/字节广告系） | 新独有 9：`libandfix`、`libbsutils`、`libcrypto_1_1`、`libcustom_rules`、`libmsaoaidsec`、`libmsaoaidauth`、`libsharewind`、`libssl_1_1`、`libweibosdkcore` | **异**（渠道 SDK 全换：B站/微博/阿里热修/OAID） |
| 共有 so | 12 个：`libcryptopp`、`libenet`、`libgme*`×6、`libil2cpp`、`libmain`、`libsmsdk`、`libunity` | 同 | **同**：其中 11 个 **md5 逐一相同** |
| `libil2cpp.so` | 87,013,984 / `ab8935f68619ff4715d0b3664888e356` | 87,013,984 / `2e088d84e71273516de0403f490140ed` | **尺寸完全相同、md5 不同**（推断：同一 native 核同源重建，逻辑差异主要走热更 bundle；证据＝尺寸逐字节等大＋Unity/libunity md5 完全一致＋ResUrl 热更结构） |
| `libunity.so` | 21,037,864 / `fc71482e…` | 21,037,864 / `fc71482e…` | **完全同** |
| `global-metadata.dat` | 13,324,172（旧版未记 md5） | 13,324,172 / `7d177446104ca55942b666ecf09b1c35` | **尺寸同**；md5 无法对比（旧侧未留） |
| dex 数量 | 官方底包 3（推断：v44 外科日志 `added: classes4.dex` ⇒ 底包无 classes4）；斌哥包 7（含壳桥 `classes5.dex` 446,136） | 3 | **同（3）**；旧官方 dex 逐包大小未记（斌哥包 dex 已被改，不可比） |
| 加固 | 无（斌哥包另加 `libSDKloading` 壳，非官方加固） | 无 | **同**（官方包均未加固） |
| IL2CPP / HybridCLR | IL2CPP + HybridCLR（热更方法走解释器桩） | IL2CPP + HybridCLR 分支构建（构建路径同型） | **同**（解释器启用待 RVA 确认） |
| 引擎版本 | 未记（libunity md5 与新包一致 ⇒ 同为 2022.3.14f1） | 2022.3.14f1 | **同** |
| 资源形态 | 旧版 dataroot 无基线（`_md5.txt` 只落 `assets/bin/Data` 与顶层，147 条 assets） | `assets/dataroot` 789.7 MB 明文 UnityFS | 不可比（旧侧**未确认**） |

## 8. 结论：能否走既有 starry 复刻路线（modern 注册表 + il2cpp hook + 静态分析定位挂点）

**能，但必须整轮重做挂点定位，不能沿用旧 RVA。** 分项：

| 前提 | 判定 | 依据 |
|---|---|---|
| 能否静态分析 | ✅ 可以 | 无加固、3 dex 明文可反编译、metadata 可读、bundle 明文 |
| modern 注册表 | ✅ 可用 | `il2cpp_class_from_name`/`method_get_name`/`runtime_invoke`/`field_get_offset` 等全导出（`T`） |
| il2cpp hook 形态 | ✅ 结构同型 | 同 Unity 2022.3.14f1、同 HybridCLR 分支、`libil2cpp`/`metadata` 尺寸与旧版逐字节等大 |
| 挂点定位 | ❌ **必须重做** | 旧 `0x207F798`/`0x207DCCC`/`0x207F04C` 不可假设同偏移；`06_dumped/冒险与征途` 的 dump.cs 是 1.5.9 的 |
| 出包/装机 | ⚠️ 有阻断 | ① 异签名（B站官方 `Bbcallen`）⇒ 无法覆盖装 TapTap 版，切换需卸载旧包（**破坏性，须用户确认**）；② `game_id` 须按渠道新建，**禁复用** TapTap 的 game_id |
| 渠道面新变量 | ⚠️ 需重新确认 | 广告链路换成 `com.sigmob.sdk`/`com.windmill.sdk`/`com.gsc.*`；旧 `DS_Advertising::PlayADVideo` 落点是否仍在，**未确认**（缺证据＝新 dump.cs / metadata 类型检索，属阶段闸门后工作） |

**下一步建议（未执行，等指令）**：①（闸门后）出 2.0.1 的 dump.cs 与 metadata 类型表 → 复核四个挂点类/方法是否存活、重定解释器桩 RVA；② 用新渠道 game_id 建 `Games/` 目录，先做"只加载 so + 空菜单"的最小验证包；③ 装机前先解决异签名/旧包共存方案（需用户决定卸载或换设备）。

## 9. 复现命令（只读）

```bash
ADB=/Users/aliang/Library/Android/sdk/platform-tools/adb
BT=/Users/aliang/Library/Android/sdk/build-tools/36.0.0
$ADB shell pm path com.my.mxyzt.bilibili
$ADB pull <上面的路径> "02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk"
md5 -q <apk>; shasum -a 256 <apk>
$BT/aapt2 dump badging <apk>; $BT/apksigner verify --print-certs <apk>
unzip -l <apk>   # so/dex/assets 清单
$BT/dexdump -f <dex>   # 反编译探测
```
