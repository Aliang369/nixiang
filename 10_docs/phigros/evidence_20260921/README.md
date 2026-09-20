# Phigros 3.20.0 — TapTap 登录路由复核证据（2026-09-21）

> 设备：`f9a116a2`（OPD2404，已 root，已装《Phigros》taptap 渠道 **改签包** + `com.taptap`）
> 主文档：`../复核_TapTap签名与网页登录_20260921.md`

## 决定性证据（4 条）

1. **改签包在跑、无崩溃** — `logcat_启动_20260921.txt`
   `nativeloader ... libstarry_magic.so ... ok`、`StarryMagic: [桥接] RegisterNatives 19/19 全部成功`。
   同文件含 `StarrySignSpoof: installed pkgs=[...]`，**列表不含 `com.PigeonGames.Phigros`** → Phigros 未做签名伪装。

2. **点「使用」后走客户端 SSO** — `logcat_点击使用_login路由_20260921.txt`
   `START ... com.taptap.sdk.TapLoginHelperActivity` → `START ... com.taptap.sdk.ui.TapTapActivity`
   → 系统服务 `com.oplus.securitypermission ... 「Phigros」想要打开「TapTap」`。

3. **点「允许」后 SSO 走通（本次定稿证据）** — `logcat_允许打开TapTap_SSO结果_20260921.txt`
   `com.taptap/.common.account.ui.login.CheckOneKeyLoginActivity`（TapTap 登录页）→ SDK 容器 `CLOSE` 返回游戏
   → 回到游戏后 `TDSCommonService.initWithConfig{clientID=rAK3FfdieFob2Nn8Am, clientToken=Qr9AEqtuo…}`（**已带 token**）。
   = 改签包 `CN=StarryMagic` 下客户端 SSO 未被渠道签名不一致阻塞。

4. **「TapTap 快速认证」是防沉迷实名弹窗，不是登录页** — 本文件「UI 文本」节
   `uiautomator dump` 原文：`《Phigros》 已接入 TapTap 快速认证服务。…是否使用快速认证？[不使用][使用]`。
   SDK 侧同刻：`TapAntiAddictionService.startupWithTapTap(userIdentifier:...)` +
   `AntiAddiction: checkUserAuthIdentify: onError:NotFound desc=实名信息不存在`。

## 旁证：鉴权已通过（原「交付前需运营侧配 game_id」待办解除）

`/data/data/com.PigeonGames.Phigros/shared_prefs/starry_magic_prefs.xml` 实读：

```
auth_grant        1|dev_760f1cc7de536cd8c34dd8f79c6f62b4|phigros|1790012307|1|1|78e288f9.…
auth_last_ok_at   1789925907694   (≈2026-09-21 01:38:27)
auth_expire_at    2026-09-27T17:38:27+00:00
auth_menu_enabled true
auth_bootstrap_json  features[0]={"feat_num":1,"name":"满分","enabled":true}
```

## UI 文本（uiautomator，实读）

```
text="TapTap 快速认证"
text="《Phigros》 已接入 TapTap 快速认证服务。"
text="如果你已经在 TapTap 通过了国家网络游戏防沉迷实名认证系统的真实认证，此服务可以帮助你："
text="・ 快速通过游戏内的实名认证流程。・ 保护个人敏感信息..."
text="是否使用快速认证？"
text="不使用"  bounds="[1129,1323][1483,1418]"
text="使用"    bounds="[1515,1323][1870,1418]"
# 系统拦截弹窗：
text="“Phigros”想要打开“TapTap”"   (com.oplus.securitypermission)
```

## 最短复现命令

```bash
# 1) 前台路由轮询（用户操作同时跑，观察是否切到 com.taptap）
for i in $(seq 1 60); do
  adb -s f9a116a2 logcat -d -v time | grep -iE "TapLoginHelperActivity|TapTapActivity|com.taptap/" | tail -3
  adb -s f9a116a2 logcat -c
  adb -s f9a116a2 shell dumpsys activity activities | grep -m1 topResumedActivity
  sleep 2
done
# 2) 只读当前弹窗文案（不改状态）
adb -s f9a116a2 shell uiautomator dump /sdcard/ui.xml && adb -s f9a116a2 shell cat /sdcard/ui.xml
```

## NOT PROVEN / 未做

- **未复现**：任何因改签导致的登录拒绝（本次 SSO 全程无 `ActivityNotFound`、无异常、正常携带 token 返回）。
- **未验证**：TapTap 服务端在换包/换签名/换版本后是否可能拒绝（客户端补丁无法覆盖这一层）。
- **未验证**：`WebViewHandler` 网页登录兜底在本改签包下能否走通（已无必要，SSO 已通）。
- **已解除**：`[验证] flag=false live=0`（2026-09-21 01:25 冷启瞬时态）→ 01:38 鉴权已通过，见上「旁证」。

## 设备残留（本次产生）

- `/sdcard/ui2.xml`、`/sdcard/ui3.xml`（uiautomator dump，可随时删）
- 真机拦截弹窗 `com.oplus.securitypermission` 停留在前台（未选择允许/拒绝），游戏 `UnityPlayerActivity` 处于其下方。
- 本机临时件：`/tmp/phig_drain.log`、`/tmp/phig_fg.log`、`/tmp/phig_tap_final.log`、`/tmp/ph2/`（classes.dex + baksmali 产物）、`/tmp/ph2r/resources.arsc`。
