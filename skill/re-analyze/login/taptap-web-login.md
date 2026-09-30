# TapTap 强制网页登录

> 📍 re-analyze/login/taptap-web-login.md
> 🎯 改签后 TapTap 客户端 SSO 失败（签名不一致），需要强制走 WebView 网页登录

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
