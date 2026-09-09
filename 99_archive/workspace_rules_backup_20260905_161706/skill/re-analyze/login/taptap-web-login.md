# TapTap 强制网页登录

> 📍 re-analyze/login/taptap-web-login.md
> 🎯 改签后 TapTap 客户端 SSO 失败（签名不一致），需要强制走 WebView 网页登录

---

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

## mocika 壳加密 DEX 处理

**案例：虚无之刃**

当 `classes.dex` 被 mocika 壳加密时：
- 静态改 `classes.dex` 不可行（无法重加密）
- 运行时改缓存 c2.dex → 首次生效但重装被覆盖
- **推荐方案**：在 `libstarry_magic.so` 中用 Pine/ART hook 运行时 patch `ClientLoginHandler.canHandle`

## 弯路（不要踩）

- **`isTapTapInstalled` 改了无效**：这是渠道 SDK 的安装判断，非登录 SSO 判断，改了不会影响登录流程
- **`ClientLoginUtils.isClientLogin` 被多处调用**：但只有登录 handler 选择依赖它，改它不影响其他调用
- **新旧 SDK 混淆**：新版用 `ClientLoginUtils`，旧版用 `ClientLoginHandler.canHandle`，先确认游戏用哪个版本

## 验证方法

1. 安装改包后的 APK
2. 点击 TapTap 登录
3. 出现 WebView 登录页（而非跳转 TapTap App）= 成功
4. 网页登录可正常输入账号密码完成登录
