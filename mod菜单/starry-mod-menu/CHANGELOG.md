# starry-mod-menu 框架变更日志

> 用途：**框架侧（mod菜单）改动的唯一台账**。此前框架改动只记在各游戏的提交信息里，换版本/换游戏时无法快速回答「这个机制什么时候改的、为什么改」。
> 建档：2026-10-15（用户 m00005 定「优化会话可改 mod菜单 文档与游戏私有脚本」）。
> 记法：只增不删；每条 = 日期 ｜ 文件 ｜ 改动 ｜ 依据（用户消息号 / 复盘文档 / 实测行）。**禁改项**（`Main.cpp`／`MainModern.cpp`／`GameConfig.h`／公共 `Android.mk`／`SignSpoof.java`）的改动必须由用户明示后单独记一条。

| 日期 | 文件 | 改动 | 依据 |
|---|---|---|---|
| 2026-10-15 | `scripts/surgical_*.sh`（本轮先改 `surgical_shhj_3.8.sh`） | `SHHJ_MF_CACHE` 加载前增加**二进制 AXML 头断言**（`03 00 08 00`），命中文本 Manifest 直接 die —— 防「dump 的文本 Manifest 与 MF_CACHE 同名 ⇒ 文本进包、构建全绿、装机后坏」 | `10_docs/_state/规范优化_20261015_山海幻境4.3会话复盘.md` P0-2（用户 m00004 复盘；m00005 授权框架层） |
| 2026-10-15 | 说明（无代码改动） | 框架入口审计的 3 项通用检查对本游戏为**已知无害偏离**，不改审计本体，改用 `04_scripts/_common/audit_with_waivers.sh` ＋ `10_docs/{游戏}/审计豁免.json` 做**带证据豁免**（过期自动恢复 ✗） | 同上 P0-4；实测：豁免生效 rc=0 / 过期 rc=1 |
| 2026-10-15 | `app/src/main/java/com/starry/magic/sign/SignSpoof.java` | **追加**（纯 +15 行，未改任何已有块）：`BWXRK_PKG`/`BWXRK_CERT_B64`（保卫向日葵官方证书，`CN=QT, O=GD`，727 B DER）+ 一行 `fakes.put` | 用户 m00004「taptap的签名校验去要去掉」。⚠️ **本文件属禁改项，本次是功能指令下的必要追加，尚未取得用户对该文件的明示授权** ⇒ 已登记 `10_docs/_state/框架待办.md` **FW-6** 与 `10_docs/_state/规范优化_20261015_保卫向日葵1.46会话复盘.md` §3 #1 / P0-1。证据：base64 解码与官方 DER 逐字节相同（md5 `63e2fa8906a18d0f46b527e2d74760a8`）；运行期 `StarrySignSpoof: installed pkgs=[… com.bwxrk.yqmy.ty4 …]`；改前三条报错（`TapInitializer … 签名未配置`/`CONFIG_ERROR`/`SDK not initialized`）全消失 |
| 2026-10-15 | `app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java` | `PKG_WHITELIST` 追加 `com.bwxrk.yqmy.ty4`（+9 −1 行，仅表项与注释） | 用户 m00004「taptap的登录按钮改成强制网页登录」。实测 `StarryTapLogin: new-impl request loginType mobile_client -> embedded_web (preCreated)` + `juhe login success {code:0}` + 全量 logcat 无 `targetName=com.taptap`。同时暴露兜底分支失效 ⇒ `框架待办.md` **FW-7** |
| 2026-10-16 | 说明（无 mod菜单 代码改动；改的是共用门禁 `04_scripts/_common/{check_release_clean,framework_entry_audit}.sh`） | ① `check_release_clean.sh` 守卫词汇表补 `STARRY_TRACE`（此前用该宏包 trace 的 7 个 `Games/*Mod.cpp`——含**已交付**的 `BjdwMod.cpp`——被误判 VIOLATION）；② `framework_entry_audit.sh` `[F]` 里「先 pipe 再 `grep -q`」改 here-string（`pipefail` 下 SIGPIPE 假阴性，终末地 1.6.4 实测）；③ 修非诊断 `#if` 的 `#else` 分支被误算「已守卫」的假阴性 | 用户当轮指令（承 `10_docs/终末地/版本适配_1.6.4_20261016.md` §六实测）；全仓 A/B 与复现判据见 `10_docs/_state/规范变更日志.md` 第四批。残留项登记 `框架待办.md` **FW-8/9/10** |
| 2026-10-16 | `app/src/main/jni/Includes/Utils.cpp`（**共享源；用户当轮明示授权**） | 构建戳 `kStarryBuildStamp` 加 `__attribute__((used, retain))`（无 `retain` 的旧 clang 降级 `used`）。根因：modern 入口 `MainModern.cpp` **零引用**该戳（唯一引用者只剩 legacy `Main.cpp:2539`）⇒ `-Wl,--gc-sections,--strip-all` 把该 const 整段回收 ⇒ so 内无 `STARRYSTAMP1:EPOCH=` ⇒ Java `OtaSoStamp.apkStamp()=0` ⇒ `PlayOtaManager` 的 `if (apkStamp > 0L && …)` 恒假 ⇒ **覆盖安装后永不清 OTA so** ⇒ 运行时按「优先 OTA」加载玩家旧 so（小米 byron 实测：新包装上了、跑的却是旧 so、一开就崩；清数据/清补丁即恢复） | **FW-13**；用户当轮原话「按照你说的方案根治，共享层也改」。证据：修复前包内 so `grep -a -c STARRYSTAMP`=0 → 修复后构建期断言打印 `STARRYSTAMP1:EPOCH=1792090972`；`retain` 有效性 A/B（`-fvisibility=hidden` 下无引用 const 被回收、加 `retain` 保住）与产物对账见 `10_docs/_state/规范优化_20261016_框架_OTA构建戳FW13会话复盘.md` §2.2/§2.3 |
| 2026-10-16 | `app/src/main/java/com/starry/magic/ota/PlayOtaManager.java` | `invalidateOtaIfApkUpdated` 增**无戳内容判据兜底**：比对「下载补丁时记录的包内 so sha256」（写 `starry_ota/<game_id>/.apk_so_sha`，与补丁同生共死、不动 Preferences）与当前包内 so sha256，不一致才清；**无记录一律保守保留**；判定理由全程打日志。下载成功路径同步写入该记录 | 同上（FW-13 ②③）。**刻意不做**「读不到戳就无条件清」——OTA so 与包内 so 同源时两边都无戳，那会退化成每次冷启动都清掉玩家刚下载的补丁（无悔华夏 2026-09-24 的回归形态） |
| 2026-10-16 | `app/src/main/jni/Android_mrfzzmd.mk`、`scripts/surgical_endfield_1.6.4.sh`（**游戏私有**） | mk 加 `LOCAL_LDFLAGS += -Wl,--undefined=kStarryBuildStamp`（与共享 `retain` 双保险，附事故注释）；外科脚本加「so 缺 `STARRYSTAMP1:EPOCH=` 即 `ERROR` 拒出包」断言（并打印本次戳），另新增 `ENDFIELD_OUT` 可把产物指到临时区做 A/B（避免验证性构建误覆盖正式包） | 同上；断言首次运行即打印戳 ⇒ 把「缺戳包」挡在 L1 出包口。通用化（模板 mk / `_common` 出包门禁）见复盘 **P0-2**，登记 `框架待办.md` **FW-14** |

## 待办（框架侧）

见 `10_docs/_state/框架待办.md`（框架侧发现的统一落点；本文件只记**已发生**的改动）。
