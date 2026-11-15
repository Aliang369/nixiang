# 03 · TapTap 去签名校验 + 强制网页登录 —— 路线判定与补丁清单

> 包：com.wks.abyss 2.36 / vc236（TapTap 渠道包）
> 底包：`02_apks/阿比斯少年冒险团/阿比斯少年冒险团_2.36_taptap_original.apk`（1,133,253,810 B，sha256 `C3CC17EE…29C0A0`）
> 反汇编基准：`03_decompiled/阿比斯少年冒险团/smali_out/classes{,2}/`（recon 产物，baksmali 直出）
> 脚本：`04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py`
> 判定时间 2026-11-15 · status：**已判定 · 补丁已落地并通过本地汇编/回读断言；装机验收待 T5**

---

## 0. 结论

**走「改包法」（静态改 dex），不走运行时 SPI。** TapSDK 明文在 `classes2.dex`（`com/taptap/sdk` 2647 处引用），
门闩（`TapGatekeeperManager` / `TapGatekeeperKt`）、登录（`LoginActivityImpl` / `ClientLoginHandler`）、
签名上报（`TapSignKit`）三类锚点全部可定位可改写 ⇒ 不需要 `GameHookImpl` + `TapLoginForceWeb`，
**不复活 SignSpoof**、不加包名白名单、不改 `StarryApp` / `StarryInitProvider`。

---

## 1. 路线判定证据（TapSDK 类可达性）

### 1.1 dex 结构（实测：python 直读 zip / 字符串表）

| 项 | 值 |
|---|---|
| dex 数量 | 2：`classes.dex` 1,826,036 B；`classes2.dex` 8,632,396 B |
| ABI | arm64-v8a / armeabi-v7a |
| 引擎 | Unity IL2CPP（`libil2cpp.so` 141 MB + `global-metadata.dat` 21 MB） |
| 渠道壳 | U8：`application android:name=com.u8.sdk.U8Application`（recon 实测） |
| TapSDK native | `lib/arm64-v8a/libtapsdkcore.so` 4,605,352 B；`libthemis.so` 907,448 B |

### 1.2 `com/taptap/sdk/**` 命中（classes2.dex 原始字节计数）

| 串 | 命中数 |
|---|---|
| `com/taptap/sdk` | 2647 |
| `WebLoginFragment` | 27 |
| `TapTapSdkInitializer` | 17 |
| `LoginActivityImpl` | 7 |
| `TapTapKitInitProvider` | 4 |
| `InitializerServiceImpl` / `ClientLoginHandler` / `TapSignKit` / `WebHandler` | 各 2 |

⚠️ `ClientLoginUtils`、`DefaultTapLoginFragmentProvider`、`GatekeeperRepository`、`checkInitialize(Z)Z`
**均 0 命中** —— 这些是 TapSDK 4.8.x / `tap-login_release` 的旧锚点。**本包是 3.x 形态，锚点必须现取**；
把绿梦/蛋壳战机的改法直接抄过来会全部锚点缺失。

### 1.3 本包实际类位（全部位于 `smali_out/classes2/`）

| 职能 | 类 |
|---|---|
| 业务统一门闩 | `Lcom/taptap/sdk/initializer/service/InitializerServiceImpl;` |
| 登录前检查 | `Lcom/taptap/sdk/initializer/api/check/TapSdkChecker;` |
| 门闩状态 | `Lcom/taptap/sdk/initializer/gate/TapGatekeeperManager;` |
| 门闩弹窗 | `Lcom/taptap/sdk/initializer/gate/TapGatekeeperKt;` |
| 签名上报 | `Lcom/taptap/sdk/kit/internal/utils/TapSignKit;` |
| 登录 Activity（SDK 侧） | `Lcom/taptap/sdk/login/ui/TapTapLoginActivity;` |
| 登录分发 | `Lcom/taptap/sdk/login/internal/LoginActivityImpl;` |
| 客户端 SSO handler | `Lcom/taptap/sdk/login/internal/handlers/client/ClientLoginHandler;` |
| 网页登录 handler | `Lcom/taptap/sdk/login/internal/handlers/web/WebHandler;` |
| 自动初始化 Provider | `Lcom/taptap/sdk/kit/internal/TapTapKitInitProvider;` |

**可达性结论：类/方法/字段全部明文可改 ⇒ 改包法成立**（不满足 SPI 触发条件「加密 payload / 壳内动态装载」）。

---

## 2. 调用链证据（不靠类名，靠调用关系）

### 2.1 登录链：谁调 fallbackToWeb、什么条件下

~~~text
LoginManager.loginWithScope(...)                      LoginManager.smali:1326
  └─ TapSdkChecker.checkInitialize()Z                 :1356  ← 业务门闩；false 直接 return（点了没反应）
       └─ InitializerServiceImpl.checkInitialize()Z   InitializerServiceImpl.smali:52
             └─ TapTapKit.hasInitialized() && TapGatekeeperManager.isSuccess()
  └─ LoginManager.assertInitialized()                 :261   （isInitialized 静态字段 false ⇒ 抛 TapTapException）
  └─ LoginActivityImpl$Companion.startActivity → 拉起 TapTapLoginActivity

TapTapLoginActivity.onCreate(Bundle)                  TapTapLoginActivity.smali:118-136  (if savedInstanceState==null)
  └─ LoginActivityImpl.onCreate(Activity)             LoginActivityImpl.smali:307
       ├─ intent.getParcelableExtra("request")        :315-325
       ├─ if (request == null) goto :cond_98          :327
       ├─ try { for (h : loginHandlers) {             :331-347
       │        if (h.canHandle(activity, request)) { ... } }   ← :343 取第一个能处理的 handler
       │      } catch (Exception) { loge("login handler error") }
       └─ :cond_92  fallbackToWeb(activity, request)   :483   ← handler 循环走空 / 抛异常都落到这里
~~~

`loginHandlers` 数组在构造函数里装配为 **3 个**：`[CloudLoginHandler, ClientLoginHandler, PhoneLoginHandler]`，
另有一个独立字段 `webHandler`。三个 `canHandle` 的真实条件（逐个读方法体）：

| handler | canHandle 条件 | 本机（模拟器）预期 |
|---|---|---|
| `CloudLoginHandler.canHandle` | 云游戏连接：解析 `Companion.getIntent()`（`CloudLoginHandler$canHandle$connection$1`） | false（无云游戏连接） |
| `ClientLoginHandler.canHandle` | `new Intent(IntentHelper.getAction(request)).resolveActivity(pm) != null` ⇒ **装了 TapTap App 才 true** | 装了 ⇒ true ⇒ 客户端 SSO（要掐的就是这条） |
| `PhoneLoginHandler.canHandle` | 静态字段 `phoneLoginCallback != null` | false（未注册回调） |

`fallbackToWeb(Activity, LoginRequest)` 体内做的正是我们要的结果：

~~~text
LoginManager.setCurrentLoginEntrance(WEB)
fragmentManager.findFragmentByTag("login_fragment")?.remove()
webHandler.getFragment() → new WebLoginFragment()      ← WebView 登录页
add(android.R.id.content, fragment, "login_fragment").commit()
~~~

**SDK 自己还有两条退化路径调同一个出口**（证明 fallbackToWeb 是设计内正常分支，不是我们臆造）：
`CloudLoginFragment.smali:956`、`PhoneLoginFragment$onViewCreated$1.smali:232` 均调
`TapLoginActivityCallback.fallbackToWeb(request)`。

**结论：「canHandle 恒 false ⇒ 走 fallbackToWeb」这条路成立**；但只靠 P5 不稳（Cloud/Phone 任一返回 true 就先被选中），
故 P4 在 `onCreate` 取到 request 后**无条件 fallbackToWeb + return**，把「选 handler」整段短路，不依赖任何 handler 返回值。

### 2.2 门闩链：验签失败为何变成硬门闩

~~~text
TapTapSdkInitializer.initialize → TapGatekeeperManager.queryGatekeeper(Context, Function0)  :759
    JSONObject{ platform, package_name=getPackageName(), package_sign=TapSignKit.getSign(ctx) } :787-811
    → 协程 TapGatekeeperManager$queryGatekeeper$1 → 服务端比对后台登记的包名/证书
    → 失败：gatekeeperError != null、isGatekeeperSuccess = false
         → InitializerServiceImpl.checkInitialize() 里 showGatekeeperError(...)   :52  (banner + Toast + loge TapInitializer)
    → 成功：isGatekeeperSuccess = true
~~~

`isSuccess()Z`（`TapGatekeeperManager.smali:750-757`）只读静态字段（`:96` 声明）：

~~~smali
.method public final isSuccess()Z
    .registers 2
    .line 59
    sget-boolean v0, Lcom/taptap/sdk/initializer/gate/TapGatekeeperManager;->isGatekeeperSuccess:Z
    return v0
.end method
~~~

`showGatekeeperError(TapGatekeeperError)Job`（`TapGatekeeperKt.smali:37`）用 `MainScope()` 启协程
`TapGatekeeperKt$showGatekeeperError$1`（体内 Toast + banner + `loge`）。

**只掐弹窗不放门闩 = 「点了没反应」**（2026-10-28 踩过的坑）⇒ P1/P2/P3 三条一起上。

### 2.3 签名上报链（为什么要动 TapSignKit）

`TapSignKit.getSign(Context)String`（`:59`；`:101` 读 `PackageInfo.signatures[0]` → `CommonUtils.getMD5`）
是本 dex 里**唯一**把「本机证书」交给 SDK 的地方：`queryGatekeeper` 把它塞进 `package_sign` 上报。
改签后本机证书 ≠ 后台登记证书 ⇒ 服务端回 CONFIG_ERROR。⇒ P6 恒定为官方证书 MD5。

官方证书 MD5（**实测** `apksigner verify --print-certs` 底包）：`87ef7a7df2bf22058004aea8ae4ce837`
（DN `CN=lsl…` 自签；SHA-256 `4ef45649…`）。脚本用 `sign-md5` 子命令现取（apksigner 优先，回退 PKCS#7 解析），
写死值只当一致性告警。

---

## 3. THEMIS 是否做签名校验 → 静态结论：**不参与**

| 检查项 | 结果 |
|---|---|
| `com/taptap/sdk/themis/**` Java 侧读签名 | **无**。包内 9 个类，`rg -i "getPackageInfo\|signature\|certificate\|getSign"` 只命中 3 处 `dalvik/annotation/Signature`（注解元数据，非逻辑） |
| `ThemisProvider.onCreate` 做什么 | 只有 `ThemisLite.Register(Context)` + `LogUtil.init(Context)`，无版本/签名读取 |
| `libthemis.so`（907 KB）字符串 | **无** `getPackageInfo` / `signatures` / `PackageManager` / `getPackageName` 任一命中 ⇒ 无 JNI 回读 Java 签名的痕迹 |
| `libtapsdkcore.so`（4.6 MB）中 `certificate`(42) / `Signature`(14) | 看上下文全是 `X509v3 Freshest CRL`、`issuer public key`、`OCSP request`、`TLSv1.3` ⇒ **OpenSSL/BoringSSL 的 TLS 校验，不是 App 签名校验** |

**结论：静态证据支持「THEMIS 不参与 App 签名校验」**，其职责是反作弊/心跳（`app_THEMIS/THEMIS.dat`）。
是否做 root/hook/模拟器检测，dex 与 so 都判不出 ⇒ 标 **待运行时验证**（属防封审计，不是本任务判据）。

---

## 4. 补丁点清单（原始 smali → 替换体）

所有锚点用 `method_span()` 保证**唯一命中**：命中 0 次或 >1 次一律 `die`，不静默跳过。
`.registers` **保留原值**（改小会汇编失败：`This method requires at least N registers`）。

| # | 类 · 方法 | 目的 |
|---|---|---|
| P1 | `InitializerServiceImpl.checkInitialize()Z` | 业务统一门闩恒放行 |
| P2 | `TapGatekeeperManager.isSuccess()Z` | 门闩状态恒 true |
| P3 | `TapGatekeeperKt.showGatekeeperError(TapGatekeeperError)Job` | 掐掉 banner/Toast/loge |
| P4 | `LoginActivityImpl.onCreate(Activity)V` | 登录入口恒 Web（短路 handler 选择） |
| P5 | `ClientLoginHandler.canHandle(Activity, LoginRequest)Z` | 客户端 SSO 兜底关闭 |
| P6 | `TapSignKit.getSign(Context)String` | 上报官方证书 MD5 |

### P1 `InitializerServiceImpl.checkInitialize()Z`（:52）

原始（关键指令）：

~~~smali
.method public checkInitialize()Z
    .registers 6
    .line 42
    sget-object v0, Lcom/taptap/sdk/kit/internal/TapTapKit;->INSTANCE:Lcom/taptap/sdk/kit/internal/TapTapKit;
    invoke-virtual {v0}, Lcom/taptap/sdk/kit/internal/TapTapKit;->hasInitialized()Z
    move-result v0
    const/4 v1, 0x0
    if-nez v0, :cond_24
    ...  Toast "当前应用还未初始化" + loge("TapInitializer", ...)
    :cond_24
    sget-object v0, Lcom/taptap/sdk/initializer/gate/TapGatekeeperManager;->INSTANCE:...
    invoke-virtual {v0}, ...TapGatekeeperManager;->isSuccess()Z
    move-result v0
    if-nez v0, :cond_38
    ...  showGatekeeperError(getGatekeeperError$tap_initializer_release())
    :cond_38
    const/4 v1, 0x1
    :cond_39
    return v1
.end method
~~~

替换：

~~~smali
.method public checkInitialize()Z
    .registers 6

    # [tapbot T4 改包法] 验签门闩恒放行（原实现: TapTapKit.hasInitialized && TapGatekeeperManager.isSuccess）
    const/4 v0, 0x1

    return v0
.end method
~~~

### P2 `TapGatekeeperManager.isSuccess()Z`（:750）

原始 = 读静态字段 `isGatekeeperSuccess:Z`（`:96`）直接返回。替换：

~~~smali
.method public final isSuccess()Z
    .registers 2

    # [tapbot T4 改包法] 门闩恒放行（原实现: return isGatekeeperSuccess）
    const/4 v0, 0x1

    return v0
.end method
~~~

### P3 `TapGatekeeperKt.showGatekeeperError(TapGatekeeperError)Job`（:37）

原始 = `CoroutineScopeKt.MainScope()` + `new TapGatekeeperKt$showGatekeeperError$1(...)` + `BuildersKt.launch$default` + 返回 Job。
**不能**改 `return-void`（返回类型是 Job）⇒ 返回 null：

~~~smali
.method public static final showGatekeeperError(Lcom/taptap/sdk/initializer/gate/TapGatekeeperError;)Lkotlinx/coroutines/Job;
    .registers 8

    # [tapbot T4 改包法] 掐掉验签失败弹窗（原实现: MainScope 启协程 → Toast + banner + loge）
    const/4 v0, 0x0

    return-object v0
.end method
~~~

### P4 `LoginActivityImpl.onCreate(Landroid/app/Activity;)V`（:307）

原始（`:315-347`、`:483`）：

~~~smali
.method public final onCreate(Landroid/app/Activity;)V
    .registers 9
    const-string v0, "activity"
    invoke-static {p1, v0}, Lkotlin/jvm/internal/Intrinsics;->checkNotNullParameter(...)
    .line 38
    invoke-virtual {p1}, Landroid/app/Activity;->getIntent()Landroid/content/Intent;
    const-string v1, "request"
    invoke-virtual {v0, v1}, Landroid/content/Intent;->getParcelableExtra(Ljava/lang/String;)Landroid/os/Parcelable;
    move-result-object v0
    check-cast v0, Lcom/taptap/sdk/login/internal/bean/LoginRequest;      ← :325
    if-eqz v0, :cond_98                                              ← :327
    :try_start_13
    iget-object v2, p0, ...LoginActivityImpl;->loginHandlers:[Lcom/taptap/sdk/login/internal/handlers/LoginHandler;
    :goto_17
    if-ge v3, v4, :cond_92
    aget-object v5, v2, v3
    invoke-interface {v5, p1, v0}, Lcom/taptap/sdk/login/internal/handlers/LoginHandler;->canHandle(...)Z   ← :343
    move-result v6
    if-eqz v6, :cond_85
    ...（选中 ⇒ 记 LoginEntrance + add fragment + return）
    :cond_85
    add-int/lit8 v3, v3, 0x1
    goto :goto_17
    :cond_92
    invoke-virtual {p0, p1, v0}, ...LoginActivityImpl;->fallbackToWeb(...)V   ← :483
~~~

替换 = 在 `:327` 之后插两句（`v0`=request，`p0`=impl，`p1`=activity，寄存器都已在用，`.registers` 不动）：

~~~smali
    if-eqz v0, :cond_98

    # [tapbot T4 改包法] 登录入口恒 Web（绕过 Cloud/Client/Phone handler 选择，等同 SDK 自己的 fallbackToWeb）
    invoke-virtual {p0, p1, v0}, Lcom/taptap/sdk/login/internal/LoginActivityImpl;->fallbackToWeb(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;)V

    return-void
~~~

> 不改 `instance-of`（会误判 LoginEntrance）、不改 handler 数组元素类型 —— 走 SDK 自带出口，
> `LoginEntrance.WEB` 由 `fallbackToWeb` 自己设置。

### P5 `ClientLoginHandler.canHandle(Activity, LoginRequest)Z`（:50）

原始（`:66`）：

~~~smali
    new-instance v0, Landroid/content/Intent;
    invoke-virtual {v1, p2}, ...IntentHelper;->getAction(...)Ljava/lang/String;
    invoke-direct {v0, p2}, Landroid/content/Intent;-><init>(Ljava/lang/String;)V
    invoke-virtual {p1}, Landroid/app/Activity;->getPackageManager()Landroid/content/pm/PackageManager;
    invoke-virtual {v0, p1}, Landroid/content/Intent;->resolveActivity(...)Landroid/content/ComponentName;
    move-result-object p1
    if-eqz p1, :cond_21
    const/4 p1, 0x1
    goto :goto_22
    :cond_21
    const/4 p1, 0x0
    :goto_22
    return p1
~~~

替换：

~~~smali
.method public canHandle(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;)Z
    .registers 5

    # [tapbot T4 改包法] 禁止拉起 TapTap 客户端 SSO（原实现: Intent.resolveActivity(com.taptap...) != null）
    const/4 v0, 0x0

    return v0
.end method
~~~

### P6 `TapSignKit.getSign(Landroid/content/Context;)Ljava/lang/String;`（:59）

原始（`:101`）= `pm.getPackageInfo(pkg, 0x40).signatures[0].toByteArray()` → `CommonUtils.getMD5`；异常 `printStackTrace` 后返回 null。替换：

~~~smali
.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 4

    # [tapbot T4 改包法] 恒返回官方证书 MD5（原实现: pm.getPackageInfo(pkg,0x40).signatures[0] 的 MD5）
    const-string v0, "87ef7a7df2bf22058004aea8ae4ce837"

    return-object v0
.end method
~~~

`--no-sign` 可关（只想验门闩、签名另走 B 轨 killer 时）。

---

## 5. 脚本与实测（T3 直接调两个入口）

`04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py`

~~~bash
# A) 就地打补丁（apktool 目录 / baksmali 扁平目录都认；幂等）
python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py patch \
    --dec-dir 03_decompiled/阿比斯少年冒险团 \
    --base-apk 02_apks/阿比斯少年冒险团/阿比斯少年冒险团_2.36_taptap_original.apk
#   --no-sign            不打 P6
#   --sign-md5 <32hex>   直接指定官方证书 MD5
#   --dry-run            只判锚点不写盘

# B) 成品验收（只读：baksmali 成品 dex 回读断言，不解整包）
python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py verify --apk 07_patched/阿比斯少年冒险团/<成品>.apk

# C) 取官方证书 MD5
python 04_scripts/阿比斯少年冒险团/abssnmxt_tap_patch.py sign-md5
~~~

| 要求 | 实现 |
|---|---|
| 幂等 | 方法体内已有 `# [tapbot T4 改包法]` → `[skip]`，不重复插 |
| 原 smali 备份 | 首次改动前 `cp` 到 `<dec-dir>_tap_backup/<文件名>.orig`（重跑不覆盖第一份原件；备份**放目录外**，避免 apktool b 把 unknown 文件打进包） |
| 字节断言 | 改完立刻 `assert_patched(require_marker=True)` 逐点校验（P4 要求 `fallbackToWeb` 出现 **2** 次且位于 `canHandle` 之前；P5 要求 `IntentHelper;->getAction` 消失） |
| 失败即 die | 锚点缺失/不唯一/断言不过 → stderr + `SystemExit(1)`，无 try/except 吞 |
| 可复跑记录 | 写 `<dec-dir>_tap_backup/patch_manifest.json`（类/方法/状态/sha256 before-after） |
| 铁律自检 | 扫全目录，出现 `Lcom/starry/magic/sign/SignSpoof;` 或 `SignSpoofAppComponentFactory` 即 die |

### 5.1 本地实测（2026-11-15，未装机）

| 步骤 | 结果 |
|---|---|
| `patch --dec-dir 07_tmp/abs_tap/test_dec --no-sign` | P1–P5 `[patched]`，断言全过 |
| 同命令重跑 | 5 条全 `[skip]`，断言仍过（幂等成立） |
| `patch --sign-md5 87ef…` | P6 `[patched]`，6 点断言全过 |
| `smali assemble`（`01_tools/smali/smali-fat.jar`） | **RC=0**，产物 8,694,776 B ⇒ 补丁后 smali 可汇编 |
| `verify --dec-dir`（已打） | **PASS** |
| `verify --apk`（已打 dex 装进测试 zip） | **PASS**（断言不依赖注释，dex 回读看指令） |
| `verify --apk`（原始 classes2.dex） | **FAIL**，6 点全报（反向验证：断言真的会咬） |
| `patch --dry-run --dec-dir 03_decompiled/阿比斯少年冒险团` | 6 类锚点全部命中 ⇒ 与 recon 官方解包目录一致 |

---

## 6. 验收判据（装机后，T5 执行）

### 6.1 logcat：三类日志不得出现 CONFIG_ERROR / 包名签名错误

~~~bash
adb logcat -c
adb logcat -s TapInitializer:V JuHeSDK:V TapTapException:V TapTapSDK:V TapLoginLog:V
~~~

- 不得出现：`SDK not initialized`、`CONFIG_ERROR`、`包名、签名`、`未在 TapTap 开发者中心正确配置`
- 硬判据 = 「无 CONFIG_ERROR」，不以某条成功文案为准（文案随版本变）

### 6.2 点登录必须是网页登录（不是拉起 TapTap App）

~~~bash
adb logcat | grep -E "AppSwitchEvent|TapTapLoginActivity|WebView"
~~~

- 成立：`mCurrentFocus=com.taptap.sdk.login.ui.TapTapLoginActivity` + 出现 `android.webkit.WebView`
  + **无** `AppSwitchEvent(targetName=com.taptap)`
- 不成立（P4 未生效）：出现 `AppSwitchEvent(targetName=com.taptap)` ⇒ 仍走客户端 SSO

### 6.3 运行时签名伪装必须 0

~~~bash
adb logcat | grep -c StarrySignSpoof                     # 必须 0
unzip -p <成品>.apk classes2.dex | strings | grep -c "Lcom/starry/magic/sign/SignSpoof;"   # 必须 0
~~~

### 6.4 机器关

- `python …/abssnmxt_tap_patch.py verify --apk <成品>` → PASS
- `bash 04_scripts/_common/framework_entry_audit.sh --game 阿比斯少年冒险团 --pkg com.wks.abyss --apk <成品>`
- `bash 04_scripts/_common/pre_install_gate.sh --apk <成品> --base-apk <底包>`（**查12**：底包有门闩类，成品同名 dex 必须已改）

---

## 7. 未解决风险

1. **服务端授权码阶段可能二次校验签名。** P6 只覆盖 `queryGatekeeper` 上报的 `package_sign`（dex 内唯一取本机证书处，已 grep 确认）。
   若换 code 时服务端另判 ⇒ 现象是「WebView 出来了，输账号后报错」。那时按经验卡的 **B 轨**（MT killer 替换 `PackageInfo.CREATOR`）补，
   不在本任务范围，需 lead 另派。
2. **`Signature.toByteArray()` 与 `META-INF/*.RSA` 证书 DER 的 MD5 是否恒等**：`apksigner --print-certs` 的
   `MD5 digest` 与 recon 从 zip 取的 `87ef7a7d…` 一致，按 Android 实现应为同一值，**但只有装机 logcat 能 100% 证伪**。
   若 CONFIG_ERROR 依旧，第一件事是比对实际 `package_sign` 上报值。
3. **门闩放行后 `@getTapGatekeeper` 可能为 null**：`getAppId()` 等直接解引用（无空判）。已核对登录链不调这些
   （`LoginManager` 内无 `getAppId` 引用），成就/评价/分享才用。若后续功能 NPE，再补 `getTapGatekeeper` 空值兜底。
4. **THEMIS 运行时行为未知**（第 3 节）：静态判「不校验签名」，防作弊/环境检测未排除。
5. **出包路径**：补丁落在 `smali_out/classes2/`，apktool b 不认识该目录名（它要 `smali_classes2`）。
   推荐 T3 走外科 zip 组装（本脚本已实测 smali assemble 可出合法 dex），出包后先 `verify --apk`。
6. **P4 只覆盖 `TapTapLoginActivity` 这条入口**：若先弹云游戏再退化，会多一层 UI（SDK 自带 fallback，功能不破）。
   装机后若看到云游戏连接提示，再补 `CloudLoginHandler.canHandle` 恒 false。

---

## 8. 边界对照

- 只写 `04_scripts/阿比斯少年冒险团/`（本脚本）、`10_docs/阿比斯少年冒险团/`（本文）。
- **未创建** `GameHookImpl.java`（改包法不需要 SPI；框架侧 `-keepnames` 与本任务无关）。
- 未改 `Main.cpp` / `MainModern.cpp` / `GameConfig.h` / 公共 `Android.mk`；未签包、未装机。
- 未复活 SignSpoof、未加包名白名单、未动 `StarryApp` / `StarryInitProvider`。

---

## 9. Manifest 事实复核（本机实测，2026-11-15）

| 项 | 实测值 |
|---|---|
| `<application android:name>` | `com.u8.sdk.U8Application`（U8 渠道壳） |
| `appComponentFactory` | `androidx.core.app.CoreComponentFactory`（**未被替换** ⇒ 入口点决策表第 3 行形态不适用，仍走 Provider 入口） |
| Provider（顺序） | ① `com.taptap.sdk.kit.internal.TapTapKitInitProvider` ② `com.taptap.sdk.themis.lite.ThemisProvider` |
| `SignSpoof` 字样 | **0**（底包干净，无需摘除；也禁止再注入） |
| `TapTapLoginActivity` | 已注册（登录链入口 = 本 Activity，与 §2.1 一致） |

> Provider 顺序铁律（`skill/starry-mod` 入口点决策表）：星光的 `StarryInitProvider` 必须排在上面两个 TapTap Provider **之前**。属 T3 出包的事，本任务不动 Manifest。
