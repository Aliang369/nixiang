# TapTap 强制网页登录

> 📍 re-analyze/login/taptap-web-login.md
> 🎯 改签后 TapTap 客户端 SSO 失败（签名不一致），需要强制走 WebView 网页登录

## ⚠️ 现行口径（2026-10-29 起为「双轨制」，本节为摘要；正文＝`10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md` 的 2026-10-29 增补节）

**签名/完整性校验 → 运行时签名替换（换 `PackageInfo.CREATOR` 的「签名杀手」形态，改 `MultiDexApplication.super` 挂载 + 清 `Parcel.mCreators`/`sPairedCreators`/`PackageManager.sPackageInfoCache`）；登录方式/渠道分支 → 改包法（如改 `IntentHelper` 的 action 走网页登录）；加固壳与「直读 APK 文件／签名块摘要」类校验 → 仍走改包法。同一链条禁止双修。**

（2026-10-28 原文：**TapTap 签名校验 + 网页强制登录 = 「改包法」（静态改游戏包体 dex）。** 该口径已被上句替代，保留下方文字作历史与「为什么放弃 SignSpoof」的依据。）
**运行时签名伪装已整体删除**（`sign/SignSpoof.java` 与 `sign/SignSpoofAppComponentFactory.java` 于 2026-10-28 按用户指令删除，框架里已无该类；`TapLoginForceWeb` 白名单那套同样停用，仅服务未迁移的老包）。

- 为什么停用：`SignSpoof` 依赖隐藏 API 豁免，**Android 14 user 版真机必失败**
  （小米 M2102K1AC 实测 `E StarrySignSpoof: exemptHiddenApis(meta) failed`；只有雷电 userdebug 能过）。
  蛋壳战机 1.2.1 首版误用旧法，用户当场点出——不要再犯。
- 配方与坑：`10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md`
- 定案原文与落地记录：`10_docs/_state/规范变更日志.md`「2026-10-28 · TapTap 签名校验/登录改为「改包法」」
- 机器关：`04_scripts/_common/framework_entry_audit.sh` **[G]** + `04_scripts/_common/pre_install_gate.sh` **查12**
  （底包含 `GatekeeperRepository` 而成品同名 dex 与底包逐字节相同 ⇒ 直接 FAIL 拦装机）

改包法四步（锚点必须按**当前 SDK 版本**现取，不能跨版本抄）：

| # | 目标 | 已有实测锚点 |
|---|---|---|
| 1 | 验签/初始化门闩恒放行 | 绿梦 4.8.2：`InitializerServiceImpl.isGatekeeperLoadedSuccessfully` + `GatekeeperRepository`；蛋壳战机 `tap-login_release`：`InitializerServiceImpl.checkInitialize(Z)Z` / `checkLocalInitialize(Z)Z` 恒 true |
| 2 | 验签失败弹窗掐掉 | `GatekeeperRepository.showErrorToast(TapNetworkException)` → `return-void`（保留原 `.registers`） |
| 3 | 登录入口恒 Web | 绿梦：`LoginActivityImpl` 两处 handler → `WebHandler`；蛋壳战机：`DefaultTapLoginFragmentProvider.provide` 在 `aget v2,v3,v2` 后插 `const/4 v2, 0x3` |
| 4 | 确认伪装不存在（源码已删，无需再改） | manifest 不得挂已删除的 `SignSpoofAppComponentFactory`；判据＝dex 无 `Lcom/starry/magic/sign/SignSpoof;` 且 `StarrySignSpoof` 日志 0 行（`framework_entry_audit [B]` 已机器化） |

> 下面「登录诊断与授权边界」「三种 SDK 版本」保留为历史/按版本核对材料；其中 `SignSpoof`、`ClientLoginUtils` 等固定改法
> **不再作为新包的默认操作**，先看本节。

> 如果登录已经进到游戏登录服务器并返回错误码，先看 [server-rejection-triage.md](server-rejection-triage.md)，不要先把问题归因到 TapTap SSO 或 root 检测。

---

## 登录诊断与授权边界（合并 2026-09-05）

- **适用范围**：获授权项目的 TapTap 登录集成诊断；下述 4.8.1 仅指无限升级 11.266018 的项目记载，不泛化到 TapSDK 4.x。
- **历史证据**：[无限升级记录](../../../10_docs/无限升级/运行时dump与YAES破解_20260905.md) §八、§九。§八曾将工具类处理无效视作可忽略，§九随后记录实际渠道登录失败；应按时间线区分，不能只取早期结论。
- **历史事实（非本轮核验）**：记录将 SDK 归为 4.8.1，并记载 `ClientLoginUtils.isClientLogin` 路径处理无效、实际涉及 `internal/handlers/client/ClientLoginHandler.canHandle`。版本归属来自记录，本轮未核验依赖或 APK；这不是所有 4.x 的路由表。
- **诊断规则**：旧工具类仍存在不等于它参与执行。handler 路径须由当前构建的调用关系、选择条件和实际登录日志支持；仅搜到类名不能证明路由，证据不足为 **NOT PROVEN**。
- **支持配置**：先核对实际 SDK 依赖、渠道初始化、应用标识、包名、开发者后台登记的签名与当前构建身份；再按对应版本的官方文档核对回调和官方支持的客户端/Web 登录选项。未确认该版本提供的配置项，不臆造开关；配置不匹配交由有权限的维护者修正。
- **验证闭环**：在授权测试账户/构建上观察入口、路由、回调、成功与取消/失败结果及会话恢复。只看到 WebView 页面不等于登录成功，也不证明服务端授权有效。
- **授权边界**：登录路由选择不替代正常身份认证、用户同意或服务端授权；不新增签名伪造、强制绕过登录或校验的操作。无法通过官方支持配置解决时保留失败证据并请求项目方支持。
- **旧文定位**：以下旧章节保留作历史材料，其中“必须 patch”“恒返回”等不是本次合并认可的通用操作指令；新任务以本节的诊断、官方配置和授权边界为准。“已合并”只表示经验已收录，不等于全部命题 PROVEN。

## 原因

改签后 APK 证书与 TapTap 客户端期望的签名不匹配 → TapTap SSO（客户端登录）失败。需要 patch SDK 让其 fallback 到 WebView 网页登录。

## 三种 SDK 版本

### 版本 1：新版 SDK（ClientLoginUtils）

**适用游戏**：追迹迷城、魂坠深境、虚无之刃、物质与魔法、长生天机降世

**patch 点**：
```
com.taptap.sdk.login.impl.utils.ClientLoginUtils.isClientLogin(Activity, Actions)
```
smali patch：恒返回 `false`
```smali
.method public static isClientLogin(...)Z
    .locals 1
    const/4 v0, 0x0
    return v0
.end method
```
SDK fallback 到 `WebLoginFragment` → 内嵌 WebView 登录。

### 版本 2：旧版 SDK（ClientLoginHandler）

**适用游戏**：消消英雄2、孤勇无双

**patch 点**：
```
com.taptap.sdk.login.internal.handlers.client.ClientLoginHandler.canHandle(Activity, LoginRequest)
```
smali patch：恒返回 `false`
handler 遍历 fallback 到 `WebHandler`。

### 版本 3：ActivityHandler 版本

**适用游戏**：物质与魔法、迷雾大陆

**patch 点**：
```
ActivityHandler.authorize
```
smali patch：恒抛 `ActivityNotFoundException("starry_force_web_login")`
异常后 fallback 到 `WebViewHandler`。

## 同时处理 TapSignKit

**必须**同时 patch `TapSignKit.getSign` → 固定返回原版 cert MD5。

否则 SDK 内部签名校验仍会失败。

## 证书回灌自检（SignSpoof，2026-10-15 起）— **已删除（2026-10-28）**

> ⚠️ 本节整套机制**已从源码删除**（类文件不在了，运行时签名伪装在 Android 14 user 版真机也必失败）。
> 新包一律按文件顶部「现行口径」用改包法；本节仅在**核对/迁移未迁移的老包**时作对照材料保留。
> 判据换成：装机后 `StarrySignSpoof` 日志行数必须为 **0**。

改签包后 TapTap 用**本机 PackageManager 里的签名**去服务端核对，不一致即报：
`TapInitializer: 应用的包名、签名（certificate fingerprint）未在 TapTap 开发者中心正确配置`
→ `JuHeSDK TapTap CONFIG_ERROR: 包名、签名错误` → `TapTapException: SDK not initialized`。
⚠️ **症状里不出现「证书」字样**，只报 CONFIG_ERROR ⇒ 先自己验证书，别去查登录流程或服务端。

机制：`SignSpoof` 按包名回灌官方证书 DER（每款游戏一段 `XXX_PKG` + `XXX_CERT_B64` + 一行 `fakes.put`）。
**追加**证书块属允许的模式化操作（`AGENTS.md` 授权节），但**抄错一位就复现上面三条报错且极难定位**，故必须逐字节自检：

```bash
unzip -o -q <官方.apk> 'META-INF/*.RSA' -d /tmp/c
openssl pkcs7 -inform DER -in /tmp/c/META-INF/*.RSA -print_certs -out /tmp/c/cert.pem
openssl x509 -in /tmp/c/cert.pem -outform DER -out /tmp/c/cert.der
md5 -q /tmp/c/cert.der                                          # ① 官方 DER md5
apksigner verify --print-certs <官方.apk> | grep "MD5 digest"    # ② 必须与 ① 相等
# ③ Java 源里 XXX_CERT_B64 各段拼起来 base64 解码后的 md5 也必须相等
```

装机后判据：`StarrySignSpoof: installed pkgs=[… <包名> …]` 且上述三条报错消失。

> 先例：见 `10_docs/保卫向日葵/审计_20261015.md` §3.3（727 B DER 逐字节相同，md5 `63e2fa89…`）。

## mocika 壳加密 DEX 处理

`classes.dex` 被 mocika 壳加密时：静态改不可行（无法重加密）、改运行时缓存 `c2.dex` 首次生效但重装即被覆盖
⇒ 走 `libstarry_magic.so` 内运行时 patch `ClientLoginHandler.canHandle`（Pine/ART hook）。

> 先例：见 `10_docs/虚无之刃/技能先例.md`。

## 弯路（不要踩）

- **`isTapTapInstalled` 改了无效**：这是渠道 SDK 的安装判断，非登录 SSO 判断，改了不会影响登录流程
- **`ClientLoginUtils.isClientLogin` 被多处调用**：但只有登录 handler 选择依赖它，改它不影响其他调用
- **新旧 SDK 混淆**：新版用 `ClientLoginUtils`，旧版用 `ClientLoginHandler.canHandle`，先确认游戏用哪个版本

## 验证方法

1. 安装改包后的 APK
2. 点击 TapTap 登录
3. 出现 WebView 登录页（而非跳转 TapTap App）= 成功
4. 网页登录可正常输入账号密码完成登录
