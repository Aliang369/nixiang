# NJK 跨进程框架 —— 原理分析

- 分析日期：2026-09-29
- 设备：f9a116a2（OPD2404 / ColorOS）
- 分析方式：真机只读拉包 + 静态反编译，未安装/未运行/未改动
- 结论一句话：**它是一个"应用虚拟化 / 多开（分身）容器"，和 VirtualApp / BlackBox / 平行空间 / 双开助手 同一类；但内核不是 VirtualApp/BlackBox，而是商业闭源 SDK —— 乐变（Excelliance）的虚拟化引擎 `kxqp / KXQPPLATFORM`（产品线 `dualaid` 双开 / `multiaccount` 多开）。**

---

## 1. 样本标识

| 项 | 值 |
|---|---|
| 桌面名称 | `NJK跨进程框架` |
| 包名 | `com.pubg.xiaohan`（伪装成 PUBG 相关，与真实功能无关） |
| versionName | `白玖牛逼888`（戏谑字符串） |
| versionCode | `1` |
| minSdk / targetSdk | 14 / 28 |
| compileSdk | 32 |
| 启动 Activity | `com.rv2k.eqr.vzs2moh2.gc9vrp6nrv3u`（强混淆类名） |
| Application | `com.rv2k.eqr.ujvjyh211yo4` |
| sharedUserId | `b.j` |
| android:debuggable | `true`（调试态构建） |
| 首次安装 | 2026-09-29 00:38（本次会话前刚装） |
| APK 大小 | 27,061,046 字节 |
| SHA-256 | `230a30d0f9e3976fc16028a1c97cb8423c446523601f1e8337202acc3c5ac84f` |
| MD5 | `cb3a1cd9a0ed3421595f79bcd6237778` |

本地留存：

- APK：`06_dumped/NJK跨进程框架/com.pubg.xiaohan_NJK跨进程框架_base.apk`
- 反编译：`/tmp/njk_njk_apktool`（apktool）
- 引擎 jar 解包：`/tmp/njk_zxplat`、`/tmp/njk_jar2`

> 注：同目录下的 `top.niunaijun.blackdexa64_3.2.0_base.apk` 是排查过程中的误命中样本（BlackDex64 脱壳工具），**与本程序无关**，仅作对照保留。

---

## 2. CLAIM / VERDICT / EVIDENCE

**CLAIM**：NJK 跨进程框架是 VirtualApp/BlackBox 那一类"应用分身/双开/虚拟化容器"。

**VERDICT**：成立，且已定位到具体引擎。它是**免 root 的应用虚拟化容器**，内核为**乐变（Excelliance）虚拟化平台 `com.excelliance.kxqp.platform`（KXQPPLATFORM）**，而非 VirtualApp/BlackBox 代码。

**EVIDENCE（实测）**

1. 清单里存在完整虚拟化代理体系（未被混淆，来自内置 SDK）：
   - `com.excelliance.kxqp.platform.PluginManagerService` / `PluginManagerProvider`
   - `com.excelliance.kxqp.platform.TrampolineService` / `TrampolineProvider` / `TrampolineReceiver`（蹦床/中转组件）
   - `com.excelliance.kxqp.platform.proxy.gameplugin[0-9]*.{ActivityProxy,ServiceProxy,ContentProviderProxy,ReceiverProxy}$P00..$P09`
   - 计数：`ActivityProxy` 100 个、`ServiceProxy` 100 个、`ContentProviderProxy` 100 个、`ReceiverProxy` 100 个（10 组 × 10 个占位）
   - 清单内 `<activity>` 总数 313
2. 专用进程名（跨进程隔离的证据）：
   - `:lbcore`、`:lebian`、`:olle`（核心进程；`lebian` = 乐变拼音）
   - `:platform.gameplugin*.P00..P09`（大量 guest 占位进程）
3. 原生引擎自报身份：
   - `assets/zxplat.jar` 内 `lib/arm64-v8a/libkxqpplatform.so` 含字符串 `KXQPPLATFORM`、`KXQPPLATFORM-utils`、`KXQP_SANDBOX_BASE_POLICY`、`KXQP_SANDBOX_POLICIES`
   - 同一 so 含硬编码路径 `/data/data/com.excelliance.dualaid.b32/...` 与 `/lebian/`、`.../gameplugins/com.tencent.ig`
   - 同一 so 含 `HookSocket`、`libInlineHook`、`libhookwrapper.so`、`nativeAMRHook`、`nativeNGSTHook`、`setHookDlopenEnabled`、`reportAppHaveDetected`
   - 两个内置 jar 的 AndroidManifest 均为 `package="com.excelliance.kxqp.platform"`（版本 `980.00` 与 `983.12`，label `kxqpplatform`）
4. 乐变多开产品标识：
   - 字符串 `(MultiAccount App)`、`com.excelliance.packageplugin.Loader`、`com.excelliance.open.action.HU_ENTRY`
   - 资源名前缀 `lebian_vm_*`（如 `lebian_vm_cata_choose_account_row`、`lebian_vm_noti_layout_contentview`）——"lebian VM"
   - dex 内 `com.excelliance.multiaccount.product0` / `product1`
   - guest 类被改写为 `com.excean.masaid.vm<随机>.…`（`com.excean` 为乐变新品牌命名空间）

---

## 3. 结构拆解

### 3.1 组件与进程模型（虚拟化容器的标准形态）

```
宿主进程（NJK 主进程, com.pubg.xiaohan）
 ├─ :lbcore / :lebian / :olle        核心管理进程（乐变引擎）
 └─ :platform.gameplugin[0-9].P00..P09
       每个 guest 应用被放到一个占位进程里，通过代理组件转发

代理组件（全部声明在宿主清单里，系统只认宿主）：
  ActivityProxy$P00..P09         ← guest Activity 走这里
  ServiceProxy$P00..P09          ← guest Service
  ContentProviderProxy$P00..P09  ← guest Provider
  ReceiverProxy$P00..P09         ← guest 广播
  Trampoline*/PluginManager*     ← 中转与插件管理
```

这与 VirtualApp 的 `StubActivity/StubService/StubProvider` + `:p0..:pN` 思路一致，只是命名与分组不同（乐变用 `gameplugin` 前缀 + `$P00..$P09`）。

### 3.2 引擎交付方式（两层 APK）

- 主 APK（27 MB）里只有外壳 + `libstub.so`（arm64-v8a / armeabi-v7a / armeabi / x86 / x86_64 五个 ABI）
- 真正的引擎在 `assets/` 里以"第二个 APK"形式存在：
  - `assets/zxplat.jar`（3.1 MB）：`classes.dex` + `libkxqpplatform.so`(32/64/x86) + `libzvmhelper.so` + `libexecloader.so`
  - `assets/masdt9z2j.jar`（3.7 MB）：随机名 + 随机名原生库 `libe7inxj6bn2lm.so`，同样是 `com.excelliance.kxqp.platform`
  - `assets/yap.cfg`：实为 zip/APK（内含 `classes.dex`）
  - `assets/dkcnt.jar`：`com.tool.filedownload` 下载组件
- 运行时把 jar 里的 dex/so 释放/加载，guest 应用经代理组件 + 自定义 ClassLoader 载入 stub 进程执行；原生层用 inline hook 接管进程内系统调用。

### 3.3 加固 / 混淆（"简陋"观感的一大来源）

- VMP/加固：`assets/zmvmp/**`（`libkxqpzmvmpinf.so`、`libzmvmpcls.so`）、`lib/arm64-v8a/libvmobserver.so`、`libEncryptorP.so`（含 `ttEncrypt`）、`libsavminfph.so`、`assets/18positive0..N`
- 代码混淆：`com.rv2k.eqr.*`（1541 个类）+ `com.excelliance.kxqp.platform.a..z` 单字母包
- 统计/广告/支付 SDK 堆叠：友盟（`libumeng-spy.so`、`libcrashsdk.so`）、AppsFlyer、AppLovin、AdMob、Facebook、Google Play Billing、微信/支付宝支付、MSA OAID（`libmsaoaidsec.so`）
- 清单 meta-data 为乐变渠道 SDK 标准字段：`OTAVersion / CompVersion / MainVersion / ClibVersion / PluginVersion / ShareVersion / MainChId / SubChId`
- `debuggable=true`、`sharedUserId=b.j`、`minSdk 14 / targetSdk 28`：底座很旧，属"拿来主义"改装

---

## 4. 关于"跳过防御 / 内置菜单"

- **它不做 root 绕过，也不做反作弊对抗。** 它只是一个**免 root 的虚拟化容器**：把 guest 应用放进自己的进程/沙箱里跑，宿主被授予什么权限，guest 就继承什么权限，不会额外提权。
- 反而内置了一份**黑名单**：`assets/recommend.json` → `forbidList`（93 项），会拒绝/标记以下类应用：
  - 逆向/自动化工具：`bin.mt.plus`、`bin.mt.plus.canary`、`com.qihoo360.mobilesafe`、`io.appium.unlock`、`io.appium.settings`
  - 同类多开：`com.mjbrother.mutil`、`com.excean.dualaid`、`com.excean.multiopen`、`com.excelliance.dualaid`、`com.excelliance.dualaid.huawei` 等
  - 部分腾讯游戏：`com.tencent.tmgp.pubgmhd`（和平精英）、`com.tencent.tmgp.sgame`（王者荣耀）、`com.tencent.tmgp.cf`、`com.tencent.tmgp.speedmobile` 等
- 另有 `hotList`（300 项）预置热门应用，里面混有大量 ESP/外挂类包名（`com.*.esp`、`com.xera.hax`、`com.xontwol.haxcontainer`、`com.simmu.mods`、`catch_.me_.if_.you_.can_` 等）——说明该容器的主要使用场景就是"套壳跑改包/外挂"。
- 原生含 `reportAppHaveDetected`，会对检测结果做上报。
- 悬浮窗能力存在（`FloatWindowService`、`LauncherFloatWindow`、`RedFloatView`、`MyWindowManager`、偏好项 `launch_float_window_switch_boolean`），但那是它自己的**启动悬浮球/多开入口**，**不是"游戏内置菜单"**，也不会给 guest 游戏注入任何菜单。
- 结论：**想靠它在里面跑"需要 root 绕过防御 + 内置菜单"的游戏，做不到。** 要么 guest 包本身已改好（内置菜单），要么你自己去改这个容器（引擎是闭源 + 加固 + 混淆，改造成本很高）。

---

## 5. 与冲鸭变速器 / 斌哥游戏宝盒 对比

| 程序 | 包名 | 类型 | 容器内核 | 额外功能 |
|---|---|---|---|---|
| 冲鸭变速器 | `com.joke.chongya` | 应用虚拟化沙盒 | VirtualApp/BlackBox 系（`top.bienvenido.mundo` 代理 + `:stub1..24`） | 自带变速/加速 |
| 斌哥游戏宝盒 | `com.yf.yuji.bgyxbh` | 虚拟化容器 + 辅助 | 同类（需另测） | 辅助/宝盒 |
| **NJK 跨进程框架** | `com.pubg.xiaohan` | 应用虚拟化容器（纯外壳） | **乐变 kxqp（`com.excelliance.kxqp.platform`）** | 几乎没有，只有容器 + 悬浮球 |

三者同属"免 root 应用虚拟化"，但 NJK 的功能层最薄，且内核换成了商业闭源 SDK，因此观感"很简陋"。

---

## 6. 如果要自己做一个：可参考的开源底座

乐变引擎（`com.excelliance.kxqp`）是闭源商业 SDK，GitHub 上**没有**对应源码，无法直接拿来复用。同类可自建的开源方案：

- **VirtualApp**（`asLody/VirtualApp`）：最经典的 Android 应用虚拟化框架，代理组件 + stub 进程思路与 NJK 完全一致
- **BlackBox**（`FBlackBox/BlackBox`、`niunaijun/BlackBox`）：VirtualApp 的免 root 增强分支，兼容性更好
- 插件化路线：`DroidPlugin`（奇虎360）、`VirtualAPK`（滴滴）、`Shadow`（腾讯）
- 注入/菜单路线（与本容器正交）：`LSPosed` / `Xposed`、`frida`、native inline hook（`Dobby`、`And64InlineHook`）

要"内置菜单 + 绕检测"，通常不是靠容器，而是：容器（VirtualApp/BlackBox）负责免 root 跑 guest，菜单靠**改 guest 包**或**在容器原生层注入**；反检测则需要单独针对目标游戏的风控做对抗，和"是不是分身容器"是两件事。

---

## 7. NOT PROVEN（未验证）

- 未实际运行该 APK，未验证其在当前设备/当前 Android 版本上的 guest 启动成功率。
- "乐变（Excelliance）"品牌归属为**高置信推断**：依据是包名 `com.excelliance.*`、资源前缀 `lebian_vm_*`、目录 `/lebian/`、产品名 `com.excelliance.dualaid` / `com.excean.masaid`；未联网比对官方文档。
- 黑名单/白名单的实际生效逻辑（是拒绝安装、拒绝启动还是仅上报）未做动态验证。
- `zmvmp` 加固壳的具体厂商未最终确认（仅有 `zmvmp`、`libvmobserver`、`libkxqpzmvmpinf` 等特征）。
- 引擎版本号（980.00 / 983.12）来自内置 jar 的 manifest，与主 APK 的 `CompVersion=69503 / MainVersion=234800` 的对应关系未确认。
