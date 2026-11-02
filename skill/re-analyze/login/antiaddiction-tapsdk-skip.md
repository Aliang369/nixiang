# TapTap 防沉迷「快速认证/实名」门禁改判（TapSDK antiaddiction）

> 📍 `re-analyze/login/antiaddiction-tapsdk-skip.md`
> 🎯 触发场景：游戏登录后弹「《游戏名》已接入 TapTap 快速认证服务…是否使用快速认证？[不使用][使用]」，
> 点「使用」提示**授权失败**；或用户明确要求「完全不实名直接进」。
> ⚠️ **这是绕过防沉迷实名门禁的客户端改判，只在用户当轮明确点名时做**（普通玩法功能不适用）。

---

## 一、先判两件事，别急着改包

| 现象 | 判据 | 处置 |
|---|---|---|
| 点「使用」**0.5 秒内**弹回游戏并提示授权失败 | `logcat`：`START … cmp=com.taptap/.common.account.ui.login.CheckOneKeyLoginActivity` + `dumpsys telephony.registry` 里 `mDataRegState=OUT_OF_SERVICE` | **运营商一键登录**：认证在 TapTap 客户端进程里做，签名杀手（B 轨）管不到；**改包救不了**，只能插 SIM 开移动数据 |
| 有「手动实名」入口 | `AntiAddictionImpl.authIdentityManually(...)`，端点 `https://tds-tapsdk.cn.tapapis.com`（ping 可达即正常） | 官方支持路径，服务器实名，**不依赖蜂窝**；能用就别改包 |
| 用户要「直接跳过」 | 用户原话 | 走下面第二节的客户端改判 |

先例：`10_docs/phigros/复核_TapTap签名与网页登录_20260921.md` §3.2（同一个弹窗资源名 `antiaddictionui_dialog_tap_authorization`）。

## 二、链路（SDK 3.18.x，本仓实证：我的技能无限触发 1.6.3）

```
AntiAddictionUIImpl.startup(Activity,String,Z)
  → checkUserAuthIdentify(Activity,String,Z)
  → AntiAddictionKit.checkStandaloneCacheAndFetchUserIdentifyState(cb = AntiAddictionUIImpl$7)
  → $7.onSuccess(IdentifyState)   identifyState==0 ⇒ tryLoginAntiAddiction(已实名)
                                  其它 + !hasAuthRecord ⇒ 拉实名 UI（快速认证 / 手动）
  → AntiAddictionKit.login → AntiAddictionImpl.login(userId,token,ageLimit,loginCb)
  → identifyStateObservable / PlayLogModel.checkUserStateSync（两次服务端请求）
  → AntiAddictionImpl$7.onNext(TwoTuple) → notifyAntiAddictionMessage(500) + loginCb.onSuccess()
```

字段语义（`IdentifyState`）：`identifyState`（0=已实名）、`ageLimit`、`antiAddictionToken`、`hasAuthRecord`；
`UserInfo`：`userId / accessToken / ageLimit / remainTime / clientId`。
`notifyAntiAddictionMessage(500)` ⇒ `canPlay=true` 并把结果回投 Unity（`AntiAddictionUICallback.onCallback`）。

## 三、改判三处（smali，全部集中在 SDK 内，官方其它类不动）

1. **`AntiAddictionUIImpl$7.onSuccess(IdentifyState)` 入口**：强制
   `identifyState=0`、`ageLimit=18`、`antiAddictionToken="standalone_anti_addiction_token"`、`hasAuthRecord=true`
   ⇒ 永远走「已实名」分支，不再弹实名 UI。
2. **`...$7.onError(Throwable)` 整方法**：改成构造空 `IdentifyState` 直接调 `onSuccess`。
   （服务端没有实名记录时走的就是 onError；不改这条，第 1 条永远跑不到。）
3. **`AntiAddictionImpl.login(String,String,ILoginCallback)` 整方法短路**：不请求服务端，直接
   `new UserInfo{userId, accessToken, ageLimit=18, remainTime=0x7fffffff, clientId=access$800()}`
   → `UserModel.setCurrentUser` → `notifyAntiAddictionMessage(0x1f4, null)` → `loginCallback.onSuccess()`。

可用脚本范式：`04_scripts/我的技能无限触发/patch_antiaddiction_wdjnwcf.py`（幂等，锚点计数 1 才写，
自带 `assert_registers_ok()` 自检）。

## 四、坑（血账，2026-11-02 真机实证）

- **`.registers` 必须 = 局部数 + 参数数（含 this）**。把 5 参数方法写成 `.registers 7`（只剩 2 个局部）⇒
  `p0` 落在 `v2`，随后 `const/4 v2, 0x0` 把 **this 清零** ⇒
  `NullPointerException: notifyAntiAddictionMessage(int,Map) on a null object reference`，
  主线程未捕获 → **整个游戏崩**（用户体感「登录后菜单直接崩溃」）。
  改写方法后先数寄存器，或加脚本自检：`locals = registers - (params+1) ≥ 3`，且正文不得引用参数寄存器。
- **只改 onSuccess 不够**：真实环境里实名状态查询多半走 onError（无实名记录），必须两路都改。
- **崩溃取证走 `adb logcat -d -b crash`**，一行 Caused by 就能定位到具体方法；别猜。
- 改判后 `notifyAntiAddictionMessage(500)` 是「可玩/已实名」的唯一信号，漏了它 Unity 侧会一直等。

## 五、范围与纪律

- 只对本包本版本成立；换版本必须按当前 SDK 重新核对类与方法（`baksmali` 现取锚点，禁止跨版本抄）。
- 属**防沉迷实名门禁**改动，非玩法功能：没有用户当轮明确点名，不得作为默认步骤。
- 交付时必须如实告知「这是绕过实名门禁」以及验证边界（做到"能启动/能进"才算实测）。
