# 运行时签名伪装（SignSpoof）— **已删除，禁止再使用**

> 🚫 **2026-10-28 用户指令：`sign/SignSpoof.java` 与 `sign/SignSpoofAppComponentFactory.java` 已从源码整体删除。**
> 本文件自此**只作历史机制参考**（理解老包为什么这样写、读懂旧日志），**不得再作为实施步骤**。
> 现行做法＝**改包法**（静态改游戏 dex：门闩恒放行 + 弹窗 no-op + 登录入口恒 Web）：
> 见 [`taptap-web-login.md`](taptap-web-login.md) 顶部「现行口径」与
> `10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md`。
> 机器关：`framework_entry_audit.sh [B]`（dex 不得含 `Lcom/starry/magic/sign/SignSpoof;`）+ `pre_install_gate.sh 查12`。

> 📍 re-analyze/login/sign-spoof.md
> 🎯 （历史）需要对本进程 PackageManager 查询返回原版签名，让游戏自检通过

---

## 机制（历史，照做无效：类已删）

`SignSpoof.install()` Hook `PackageInfo.CREATOR` 的反序列化过程：
1. 当本进程调用 `PackageManager.getPackageInfo` 时
2. 系统返回的 `PackageInfo` 中包含真实签名
3. SignSpoof 在反序列化层拦截，替换为原厂 DER 证书
4. 本进程所有签名查询都看到原版签名

## 安装时机（历史）

**必须在 `StarryApp.attachBaseContext` 中最早执行**（super 之后立即 install）：

```java
public class StarryApp extends Application {
    @Override
    protected void attachBaseContext(Context base) {
        super.attachBaseContext(base);
        SignSpoof.install();  // 尽早执行
        // ... 其他初始化
    }
}
```

原因：其他组件（如 SDK 初始化、签名校验）可能在 `onCreate` 之前就读签名，install 越晚越容易漏。

## 作用范围

| 维度 | 效果 |
|------|------|
| 本进程 PM 查询 | ✅ 返回原版签名 |
| 本进程 PackageInfo.CREATOR | ✅ 返回原版签名 |
| 外部进程（QQ/微信）读签名 | ❌ 读真签，不受影响 |
| native 层自检（如 ACE） | ❌ native 直接读文件，不受影响 |

## 提取原版证书（证书指纹本身仍有记录价值，但**不要再塞进 SignSpoof**）

```bash
keytool -exportcert -jarfile original.apk -rfc > orig.b64
```

将 Base64 DER 证书嵌入 SignSpoof 配置。

## 与 MT Killer 对比

| 维度 | SignSpoof | MT Killer (193/643) |
|------|-----------|---------------------|
| 执行时机 | `attachBaseContext` | 静态块（更早） |
| 实现方式 | Hook PackageInfo.CREATOR | Hook PackageManager.getPackageInfo |
| 覆盖范围 | 本进程 | 本进程 |
| 灵活性 | 可配置、可动态切换 | 固定写死 |
| 外部进程 | 无效 | 无效 |

两者功能重叠，一般用其一即可。如果游戏已有 Killer，不需要再加 SignSpoof。

## 验证方法（判据已换成「dex 不得含该类」+ `StarrySignSpoof` 日志 0 行）

```bash
adb logcat -s StarrySignSpoof:V
# 看到 installed pkgs=[...] 即成功
```

或在游戏内检查签名校验相关逻辑是否通过。

## 坑/弯路

- **只对本进程有效**：QQ/微信等外部进程读真签不受影响，详见 [qq-wechat-sso.md](qq-wechat-sso.md)
- **执行时机要尽早**：晚于 SDK 初始化就可能导致 SDK 读到真签
- **不替代 native 过检**：native 层直接读 APK 文件算哈希，SignSpoof 无法拦截
- **与 Killer 叠加无意义**：两者做同一件事，叠加反而可能冲突
