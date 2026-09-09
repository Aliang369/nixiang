# QQ/微信 SSO 登录限制

> 📍 re-analyze/login/qq-wechat-sso.md
> 🎯 改签后 QQ/微信 SSO 登录失败，需要了解限制并找到替代方案

---

## 问题

QQ/微信 App 读系统真实签名进行 SSO 认证。改签后证书不匹配 → QQ/微信拒绝 SSO 授权。

## 为什么应用内 hook 无效

QQ/微信 SSO 是**跨进程通信**：
1. 游戏 → 启动 QQ/微信 App
2. QQ/微信 App → 读系统真实签名（`PackageManager.getPackageInfo`）
3. 签名不匹配 → 拒绝授权

应用内的 SignSpoof / Killer 只 hook **本进程**的 PM 查询，QQ/微信进程读真签不受影响。

## 解决方案

### 方案 1：系统级伪签（LSPosed）
- 安装 LSPosed + SignHook 模块
- 在系统层 hook 所有进程的 `PackageManager.getPackageInfo`
- 对目标包名返回原厂签名
- 需要设备 root + LSPosed 框架

### 方案 2：原厂私钥
- 如果有原厂签名私钥，直接用原厂签名打包
- 实际上几乎不可能拿到

### 方案 3：用 TapTap 网页登录替代（推荐）
- 详见 [taptap-web-login.md](taptap-web-login.md)
- TapTap 网页登录不依赖系统签名
- 日常使用完全够用

## 微信小程序支付

微信小程序内支付走 `getLatestUserKey` 平台密钥，外部无法伪造。不建议绕过。

## 验证方法

TapTap 网页登录可用即足够。QQ/微信 SSO 在改签环境下无法使用是预期行为。
