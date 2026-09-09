# 超维进化 2.1 TapTap 签名伪装 + 强制网页登录 修改与测试

2026-09-05，用户明确指示"按照技能，游戏TapTap签名和强制网页登陆修改测试"。设备 f9a116a2。

## 方案选型（按证据，不照搬旧补丁）

先核对 TapSDK 4.9.3 实际路由（jadx + baksmali 实读）：

- `LoginTypes.from(activity, action)`：`ClientLoginUtils.isClientLogin`（Intent resolveActivity 检测 TapTap 客户端）→ CLIENT；`CloudGameUtils.isCloudGame` → CLOUD_GAME；否则 WEB（`com/taptap/sdk/login/impl/data/model/LoginTypes.java:59-69`）。
- `TapTapLoginActivity.onCreate` → `DefaultTapLoginFragmentProvider.provide` 三分支直接选 Fragment；**4.7.0 的 `LoginActivityImpl.loginHandlers` 数组在本版本不存在**（终末地 `TapLoginForceWeb` 反射方案的目标字段缺失，直接套用无效）。
- 渠道签名检查链：`GatekeeperRepository$initialize$1$networkData$result$1` 以 `(packageName, TapSignKit.getSign=签名DER的MD5)` 调 gatekeeper 接口；服务端对未登记组合返回 `invalid_client`（`ApiErr.INVALID_CLIENT`，`TapErrorConstants.ERROR_INVALID_CLIENT="invalid_client"`）。客户端在 `GatekeeperRepository$initialize$1.invokeSuspend` catch 分支置 `loadGatekeeperSuccess=false` + `gatekeeperError` + toast；`TapSdkChecker.checkInitialize` 读取该位，`TapTapLogin.loginWithScopes` 不过闸即静默不登录（实测日志 TapInitializer"包名/签名未正确配置"两处、MS_LOGIN 处无下文）。

因此本轮实施三层、均按既有模式：

1. **签名伪装（SignSpoof 常规新增）**：从原包 v2 签名块导出 DER（899 字节，MD5 da9981084dc72772be8f80e1d756df26 / SHA1 0565a5a3343ca4ed9dd9a4352e8729a0bdd103c3，CN=elliot lee），Base64 注入 `SignSpoof.java` 新增 `CWJH_PKG`；脚本校验 Java 字符串还原后与原 DER 逐字节一致（MATCH True）。TapSignKit/渠道读签名走 PackageManager，装上后 getSign 返回原 MD5。
2. **强制网页（dex 最小 patch，classes.dex）**：`ClientLoginUtils.isClientLogin` 方法体替换为恒 `return 0`（smali 注释 starry_force_web_login）→ LoginTypes.from 走 WEB → WebLoginFragment 内嵌网页。此版本无 loginHandlers 可反射，dex patch 是 4.9.3 的对应做法；本游戏无 ACE 类 dex 校验（此前整包静态排查 + 启动实测无 dex 完整性反馈），且官方原包 classes.dex 仅此一方法改动。
3. **gatekeeper 放行（dex 最小 patch，classes.dex）**：INVALID_CLIENT 分支内 `setLoadGatekeeperSuccess(false)` 改传 `const/4 v1, 0x1`；删除分支内 `access$setGatekeeperError` 与 `access$showErrorToast` 两调用（baksmali 注释行兼容的正则匹配，`# starry_gatekeeper_pass`）。网页登录本身不需要本地 gatekeeper 配置正确，放行的是 `TapSdkChecker.checkInitialize` 闸门。
   - 边界说明：gatekeeper 服务端按包名+签名 MD5 校验；SignSpoof 已让请求侧返回原 MD5，理论上服务端可放行，此 patch 是让"服务端仍拒绝"时客户端不卡死。两层独立，运行期验证见下。

## 改动清单（本轮新增/修改）

- `mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/SignSpoof.java`：+CWJH 证书（27 行）+ fakes.put 一行。
- `mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java`：白名单加 `com.grinning.beta.yszl`，注明 4.9.3 无 handler 数组、实际由 dex patch 承担。
- `mod菜单/starry-mod-menu/scripts/surgical_cwjh_2.1.sh`：新增 [3c/6] classes.dex baksmali→两 patch→smali；merge replace 增加 classes.dex；自检断言 classes.dex 必须变化、resources.arsc/classes2.dex 不变。
- `mod菜单/starry-mod-menu/app/src/main/jni/Android.mk`：编译宏 WXSJ→CWJH（恢复为原注释行，CWJH 启用）。
- 其他游戏模块、游戏 native 库（libcocos2djs.so 本轮未再改，仍含上一轮 4 字节签名表修正）、Manifest 均沿用。

## G1 静态自检（PASS）

- ndk-build so 含 `[Cwjh]` 无 `[Zxr]/[Qlfh]`；gradle APK 内 so 同检通过。
- merge 后与原包 diff：REMOVED=[]；CHANGED 仅 AndroidManifest.xml、classes.dex、classes3.dex；ADDED 仅星光件（与上一致）。
- 新包 classes.dex 反汇编复核：isClientLogin 恒 return 0；INVALID_CLIENT 分支只剩 `const/4 v1, 0x1` + setLoadGatekeeperSuccess(true)，error/toot 调用消失，成功路径 setGatekeeperError(v2) 保留。
- 签名证书不变（StarryMagic，与已装包一致），覆盖安装合法。
- APK SHA-256 `fe5c401140a050d56da9af6aa351d6ecdc2a681c8008b0161f142fd2b4476a80`。

## G2 运行（BLOCKED — 需要解锁设备）

- 覆盖安装 Success；启动采集 90 秒窗口内：无 SIGKILL、无新增退出记录、进程存活。
- 但启动后屏幕熄灭（锁屏），游戏停在 SplashActivity（`isSleeping=true`，无 jswrapper/引擎日志），未到登录页。尝试 WAKEUP/KEYCODE_WAKEUP/KEYCODE 82/wm dismiss-keyguard 均停在锁屏（截图 lockscreen/黑屏已归档）——**设备锁屏密码/指纹只有用户能解**。
- 因此本轮无法断言：网页登录页出现、gatekeeper 不再报错、TapTap 登录成功。全部标 NOT PROVEN。

## 下一步（等用户配合）

1. 解锁设备 → 告知后重跑启动采集；到达登录页点 TapTap 登录。
2. 预期成功标志：出现内嵌 WebView 登录页（不跳 TapTap App）；logcat 无 TapInitializer"未正确配置"；登录完成进入游戏。
3. 若 WebView 页报渠道错误（如 client_id 对应包名/签名校验在网页侧），则需项目方在 TapTap 开发者中心登记或提供测试配置——那是服务端契约，客户端侧已到边界。

## 收尾

已 force-stop 目标进程，无目标 PID、stopped=true；无卸载/清数据/清日志；本轮未动其他游戏。锁屏截图 2 张归档于本目录。
