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

## 待办（框架侧）

见 `10_docs/_state/框架待办.md`（框架侧发现的统一落点；本文件只记**已发生**的改动）。
