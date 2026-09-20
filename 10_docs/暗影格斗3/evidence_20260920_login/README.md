package com.wanda.sf3.wd 1.45.3/vc907 星光包登录链实测证据（2026-09-20，只读采集，未改包未清数据）

设备 f9a116a2 (OPPO OPD2515 / Android 16)，KernelSU root。

## 1. 在装包身份（实测）
- dumpsys: versionName=1.45.3 versionCode=907 firstInstallTime=lastUpdateTime=2026-09-20 01:07:42
- codePath=/data/app/~~_-rwjx_Xt4z1en9wUc1dyg==/com.wanda.sf3.wd-n7VTML7qIDugs2buE-x5zA==（单 base.apk）
- base.apk 2,715,438,777B；lib/arm64-v8a 含 libstarry_magic.so → 我方星光包，非斌哥包
- 本地只读拉取件：02_apks/暗影格斗3/暗影格斗3_1.45.3_starry_from_device.apk
  md5 fcb6c4f32a0a804a38567b142ffa7e93 == 2026-09-19 v2 正式包 md5（逐字节同一件）
- 包内注入面（aapt2 dump xmltree 实测）：
  application android:name = com.accesssdkwrapper.AccessSDKApplicationWrapper（官方原样保留）
  application android:appComponentFactory = com.starry.magic.sign.SignSpoofAppComponentFactory
  provider com.starry.magic.StarryInitProvider / service com.starry.magic.menu.MenuService
- 包内 classes8.dex（星光 dex）实测含 com/starry/magic/sign/SignSpoof.smali 与
  TapLoginForceWeb.smali，且两处均含字符串 "com.wanda.sf3.wd"；SignSpoof 内含 SF3 证书 base64。

## 2. 本机登录成功证据（实测）
- /data/user/0/com.wanda.sf3.wd/files/access_account.json（mtime 2026-09-20 17:05）base64→JSON：
  accountType=2, showPassport=137****7848, uid=3009937009
  token=HS256 JWT（已在此文档中脱敏，原文只在设备私有目录）：
    authInfo.channel=hoolai  channelId=10247  productId=3015
    deviceId=3DDEE5Q1MW227VHL  region=成都市  refreshToken=<redacted>
    iss=sdk  iat=2026-09-20 17:05:37  exp=2026-09-27 17:05:37
  ⇒ 同一改签包在本机 17:05 完成过账号下发令牌，登录链不是必然失败。
- files/access_preferences.json：autoLogin=true, installId=1789837748374_2ddc6b5493b3274e, caid=""
- shared_prefs/com.wanda.sf3.wd.v2.playerprefs.xml：unity.player_session_count=3（今天跑了 3 个会话）
- 无崩溃证据：/data/system/dropbox 无 wanda/sf3 条目；/data/tombstones 最新为 2026-09-18（本次无关）
- files/tapsdk/logs/20260920.log（6 行，01:09 首启）：
  TapSDK-Android/4.5.0 client_id=hffgckwkd8awmlryxd taptap_app_id=430024
  device_id/android_id=2ddc6b5493b3274e install_uuid=b006a5ed-… dv=OPPO md=OPD2515 sv=16

## 3. TapTap 登录态（实测缺失 = 09-19 NOT PROVEN 项仍未补）
- shared_prefs 只有 TapSdkCore_SP / GATEKEEPER_SETTING / PlayGameDurationStore / tap_sdk_sp（gid 缓存）
- 无任何 TapTap 登录令牌/账号 pref ⇒ 本机 3 个会话里 Tap 登录一次都没走通（或没被走）
- 本机 logcat 缓冲最早 17:53（已滚过 17:05 的会话），抓不到当次登录日志

## 4. 签名读取面（静态全量，baksmali 2.5.2 拆包内 8 个 dex）
- 全 APK 唯一读 PackageManager.GET_SIGNATURES/GET_SIGNING_CERTIFICATES 常量的类：
  com/taptap/sdk/kit/internal/utils/TapSignKit（getSign: getPackageInfo(pkg,0x40) →
  signatures[0].toByteArray() → MD5）
- 直接读 PackageInfo.signatures / signingInfo 字段的第三方（其余为 androidx/okio 噪声）：
  com/chuanglan/shanyan_sdk/tool/h（闪验一键登录，取 MD5）
  com/cmic/gen/sdk/e/l（移动一键登录）
  com/tencent/turingfd/…（图灵盾设备指纹）
  com/tencent/open/utils/n、com/tencent/mm/opensdk/…（QQ/微信 SDK）
  com/alipay/sdk/…（支付宝）、com/kwad、com/mbridge、com/bytedance/dr、
  com/ss/android/downloadlib、com/unity3d/services/core/device、com/heytap/openid、com/czhj
  ⇒ 以上全部走 getPackageInfo + signatures[] 数组，均在 SignSpoof.applyFake 改写面上
- 用 SigningInfo（Android P+ 新 API）的只有 androidx.browser.trusted.PackageIdentityUtils（TWA），与登录无关
- com/hoolai（账号 SDK）与 com/access*（发行 SDK）的 Java 层无任何签名校验取数点

## 5. 烘焙证书复核（实测）
从 classes8 源码 SignSpoof.SF3_CERT_B64 还原 DER（base64 无缺字符、无多出的 "TMP="）：
  bytes=845  MD5=c9950f9c3790803c7ccf42470142afe7
  SHA-1=9078e787fb5246bfd147193343cf7944dadc8fd8
  SHA-256=b147f6a485dfaa6dcb9154e5e5190d342264b0568566749df0aa38cfe55e76e3
  subject=issuer=C=cn, ST=gd, L=sz, O=szu, OU=atr, CN=sagely  有效期 2020-03-11 → 2074-12-13
与 10_docs/暗影格斗3/星空落地_1.45.3_20260919.md §4.1 记录一致。
（注：官方原包已不在 02_apks，本次无法再拿原包签名块二次对拍 ⇒ 「烘焙证书 == 官方证书」仍是 09-19 的历史记录事实。）
