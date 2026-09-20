# Phigros 状态摘要

> 由 2026-09-20 `current.md` 索引拆分生成。后续只更新本文件；
> 原始完整历史见 `../history/current_before_split_20260920.md`。

## 2026-09-21 ✅ 已修复并实机验证：TapTap 强制内嵌网页登录

> 详情：`phigros/修复_TapTap强制网页登录_20260921.md`

- **修复**：`TapLoginForceWeb` 白名单加 `com.PigeonGames.Phigros` + 注入 `YnttLoginRequest`（Parcel 往返把 intent extra `request` 换成 `getRequestCode()` 抛异常的假类）→ `LoginClient.sendLoginRequest` 走 catch → 官方 `WebViewHandler` 内嵌网页登录。
- **踩坑**：首版钩子用 `Class.asSubclass` 泛型造型 → 装机后 `VerifyError [0x54] expected 2 argument registers`（跨 dex 泛型擦除签名判错）；改 `Parcel.obtain` + `Creator.createFromParcel` + `check-cast` 后通过。
- **产物**：`08_output/phigros/phigros_3.20.0_taptapweb-releaseSigned.apk`（2,673,602,571 B，sha256 `c72e37e6…`，md5 `57d9be0d11b55734f2c28c7a7825cc05`，CN=StarryMagic）；基准=真机已装改签包（sha256 `a18d9fdb…`）；zip 审计**只有 classes2.dex 变化**。已覆盖安装（versionCode 154，lastUpdateTime 2026-09-21 02:16:54）。`08_output/phigros` 已裁剪为唯一正式包（removed=0）。
- **复现入口**：`mod菜单/starry-mod-menu/scripts/build_phigros_dex_patch.sh`（重建补丁 dex，幂等）+ `surgical_phigros_dex_patch.sh`（换 dex/重签/装机，含 zip 条目审计与签名指纹校验）。
- **实机验证**：点「账号与统计 → TapTap 登录」后顶层 Activity = `com.PigeonGames.Phigros/com.taptap.sdk.ui.TapTapActivity`，uiautomator 实读「登录 | TapTap / 手机号登录注册 / 老用户邮箱登录」→ **包内网页登录页，未拉起 TapTap 客户端，`signature not match` 消失**；用户在内嵌网页**实际完成登录，登录没问题**；`TapLoginHelperActivity` 正常销毁回到 `UnityPlayerActivity`。冷启动无 VerifyError，mod `RegisterNatives 19/19` 正常。
- **NOT PROVEN**：登录后 `clientToken` 回填/账号态持久化未做 token 级取证；首页防沉迷链路未在新包回归。
- **登录态取证（2026-09-21，用户确认两处均登录成功）**：`/data/data/com.PigeonGames.Phigros/shared_prefs/taptap_sharepreference.xml` 实读 `profile={"name":"晓亮","openid":"hJzcqWw+…","unionid":"rU4fZajJdbSQme5O7VSC5w=="}`、`token_data` 含真 `access_token`/`mac_key`/`scope:"public_profile compliance"`；`SP_AntiAddictionUiTip.xml` 有 `RealNameToast=true`（防沉迷实名链路也走通）。
- **包管理约定**：`02_apks/`、`08_output/` 由用户管理，**每次安装后清理**，需要时会告知位置——不是缺口。

## 2026-09-21 ⚠️ 追加实测：游戏内账号登录因渠道签名失败（上一段结论部分作废）

- **触发点**：游戏内「账号与统计」→ TapTap 登录（与首页防沉迷快速认证是两条链路）。
- **失败原因实锤 = `signature not match`**：`TDSLoginService.startTapLogin` → `TapTapActivity` → TapTap 客户端 `CheckOneKeyLoginActivity` 立即 finish → 回调 `{"code":-1,"msg":"signature not match","error":"permission_result"}`。游戏把 `code=-1` 显示成「登录失败，请检查网络」→ **不是网络问题，是渠道签名**。
- **失败后不降级网页**：`LoginBlock.onActivityResult` 收到 `resultCode=-1` 直接 `finish()`；全包无 `fallbackToWeb`。只有 `LoginClient.sendLoginRequest` 的 catch 分支才落 `WebViewHandler`。
- **修正**：本文件上一段"不需要强制网页登录补丁"**仅对首页 SSO 成立，对账号登录不成立**，需强制内嵌网页登录。
- **补丁已有、只差开白名单**：`mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java` 有 14 包白名单，`com.PigeonGames.Phigros` 不在其中；其 `forceOldImplWebEarly` 路径恰好命中本包（`com.taptap.sdk.ui.TapTapActivity` + intent extra `request` + `LoginClient.getRequestCode()` 在 try 内 + 失败落 `WebViewHandler`）。**最小改动 = 白名单加一行 + 注入 `yntt_smali` 的 `YnttLoginRequest(.$1).smali`**，官方 `classes.dex` 仍可字节保留。
- **证据**：`phigros/evidence_20260921/logcat_游戏内账号登录_signature_not_match_20260921.txt` + `游戏内登录_关键行.txt`；完整判定见 `phigros/复核_TapTap签名与网页登录_20260921.md` §3.4。
- **未做**（需授权）：白名单改动 + 出包 + 装机验证（属 `skill/`与工程改动，且需重新组装/签名）。

## 2026-09-21 动态实测定稿：首页 SSO 走通（结论范围已限缩为"仅首页链路"）

- **「点击屏幕开始」触发的是防沉迷实名弹窗，不是登录**：`uiautomator` 实读「《Phigros》 已接入 TapTap 快速认证服务…是否使用快速认证？[不使用][使用]」（资源 `antiaddictionui_dialog_tap_authorization` / `ll_use_tap_real_name_tips`），同刻 SDK 桥接 `TapAntiAddictionService.startupWithTapTap(userIdentifier:f484fd17…)` + `AntiAddiction: checkUserAuthIdentify NotFound: 实名信息不存在`。→ 游戏没有独立 TapTap 登录按钮，是登录/实名入口都并到首页那次点击。
- **完整 SSO 链路实锤**（`evidence_20260921/logcat_允许打开TapTap_SSO结果_20260921.txt`）：点「使用」→ `TapLoginHelperActivity` + `taptap.sdk.ui.TapTapActivity` → 系统弹「「Phigros」想要打开「TapTap」」→ 点「允许」→ `com.taptap/.common.account.ui.login.CheckOneKeyLoginActivity`（TapTap 登录页）→ 容器 `CLOSE` 返回游戏 → 回来时 `TDSCommonService.initWithConfig` **已带 `clientToken=Qr9AEqtuo…`**。
- **结论（已被上方 2026-09-21 ⚠️ 追加实测部分作废/限缩）**：首页这条 SSO 走通；但**游戏内「账号与统计」登录实测失败于 `signature not match`**（见本文件最上方 ⚠️ 段）——**需要**强制网页登录补丁，范围是游戏内账号登录链路。原「跳过」结论不成立。
- **鉴权阻塞已解除**：`starry_magic_prefs.xml` 实读 `auth_grant=1|dev_760f…|phigros|1790012307|1|1|78e288f9.…`、`auth_last_ok_at=1789925907694`(≈01:38:27)、`auth_menu_enabled=true`、`auth_bootstrap_json.features[0]={"name":"满分","enabled":true}`。冷启时的 `[验证] flag=false live=0` 是鉴权未完成的瞬时状态 → 旧待办「交付前需运营侧配 game_id」不再成立。
- **首页 SSO 结论的适用范围**：仅首页防沉迷那条链路；账号登录那条为失败（见下方 ⚠️ 段）。
- **设备残留**：`/sdcard/ui2.xml`~`ui4.xml`；本机 `/tmp/ph2`、`/tmp/ph2r`、`/tmp/phig_*.log`。

## 2026-09-21 复核：TapTap 签名 / 强制网页登录

> 详情与证据链：`../../phigros/复核_TapTap签名与网页登录_20260921.md`（+ `phigros/evidence_20260921/`）

- **上一轮确实跳过了两项**：`surgical_phigros_3.20.0.sh` 注释显式写「无签名校验绕过需求，不做 milthm 的 TapTap WebLogin 补丁」；交付文档 §5.3 也只记「TapTap 登录兼容性观察」。
- **跳过对游戏自身成立（实证）**：`libil2cpp.so` 无任何 `getPackageInfo / Signature` 字符串；metadata 里的 `CheckSignature/GetSignature/VerifySignature` 三方字符串堆邻域实锤属 mono `AuthenticodeDeformatter`、`BigInteger/DSAManaged`、`Remoting`/`AndroidJNIHelper` 标准库，非游戏自研校验。
- **渠道 SDK 形态**：老线 `com.taptap.sdk`（TDS 3.30.3，clientID `rAK3FfdieFob2Nn8Am`），**无 `TapSignKit`**（新版去验签点不适用），dex 全域无包签名校验。登录路由 `LoginClient.sendLoginRequest`：`ActivityHandler.authorize`（拉起 TapTap 客户端 SSO）→ `catch Exception` → `handlePhoneVerify(2)` → `WebViewHandler.authorize`（内嵌 WebView）；`onLoginFailed()` 也直接降级 WebView → **不 patch 也有网页兜底**。
- **签名伪装现状（实证）**：`StarrySignSpoof: installed pkgs=[...]` 不含 `com.PigeonGames.Phigros` → Phigros 未做签名伪装（因游戏自身无校验，暂不影响）。
- **真机启动实证**（`f9a116a2`，改签包）：正常启动、JNI 19/19 PASS、TapTap/防沉迷 SDK 初始化完成、无 FATAL；本次启动无登录动作。
- **NOT PROVEN**：TapTap 登录端到端（需人工点一次登录后抓 `LoginClient/ActivityHandler/WebViewHandler` 日志判定走 SSO 还是降级 WebView，命令见复核文档 §3.1）。
- **硬阻塞提醒**：日志实证 `[验证] flag=false live=0`、`[数值] 拒绝 #0（未授权）` → 后台 game_id=phigros 未配置时「满分」不可用，优先级高于 TapTap 登录。
- 若最终需强制网页登录：本包落点为 `ActivityHandler.authorize` 恒抛 / 直接调 `WebViewHandler.authorize`；因官方 `classes.dex` 是字节级保留对象，改动属方案级变更需重新定方案。

## 2026-09-20 索引迁移记录

> 来源：`../history/current_before_split_20260920.md` 的原始行。

<!-- source-line:94 -->
| [Phigros](../../phigros/分析_斌哥包抓点_20260908.md) | 3.20.0 / vc 154 | **✅ 20260908 定稿交付（MuMu 实测通过）**：正式包=…3.20.0_starry-releaseSigned.apk（2.5G，包内 so=最新构建字节一致，v2/v3 签名过）；功能=「玩法→满分」（复刻斌哥增强链：hook RVA 0x1AF0A7C 每帧函数，ON 调 0x1AEC318 Perfect 事件处理驱动准度积分；实测分数 10 万级+连击叠高=斌哥一致）；**真·自动跳未完成**（4 轮 Trace 判定点未锁死，方法名全混淆；字段语义+弯路+重启路线见抓点文档 §5）；**>2G 新坑实锤**=zip64 EOCD 归一化（surgical 脚本）；复现链=scripts/surgical_phigros_3.20.0.sh + so_only.sh（阈值已修）；遗留=后台 game_id=phigros 配置（鉴权卡，交付前运营侧完成） |
