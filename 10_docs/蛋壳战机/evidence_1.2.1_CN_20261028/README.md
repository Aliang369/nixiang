# 蛋壳战机 国服 1.2.1 (vc44) 证据索引 — 2026-10-28

> 设备：`emulator-5554`（雷电 LDPlayer，x86_64 + libhoudini）+ `96f0d883`（小米 M2102K1AC，arm64 原生，无 root）
> 包名：`com.survivor.acecn`
> **当轮最终正式包 md5：`bafd52ca16c4c7a0365e6b8d186b3d53`**（TapTap 改包法版；旧的两版 `49817640…`/`2e0755e1…` 已作废）
> 本轮记录：`../星空落地_1.2.1_CN_20261028.md`

## 0. 版本沿革（同一轮，三次出包）

| 版本 | md5 | 变更点 | 状态 |
|---|---|---|---|
| v1（首版） | `49817640c926a7fda17f24463c22e831` | 照 1.1.8 形态（含运行时签名伪装）；偷盗技能 `steal abort` | 作废 |
| v2 | `2e0755e1ad113f799433119d7d275edb` | + aux by-name 兜底（偷盗技能修好，用户实测 PASS） | 作废 |
| **v3（最终）** | **`bafd52ca16c4c7a0365e6b8d186b3d53`** | **+ TapTap 改包法**（验签门闩恒 true / 登录恒 WebLoginFragment / 摘掉 SignSpoof；manifest 恢复官方 CoreComponentFactory） | 已装两台设备 |


## 1. 决定性行

- [`decisive_l2_20261028.txt`](decisive_l2_20261028.txt)
  - §1 菜单层 PASS：`RegisterNatives 19/19` / `auth game_id synced from native: dkzj` / `native menu shown mode=ACTIVITY` / `[dkzj] 初始化成功`
  - §2 首轮 hook：12/12 `not in RX` + `by-name deferred (il2cpp runtime not up)`（il2cpp 未起来，40s 后重试成功）
  - §4 maps：`libil2cpp.so` 在本进程内**无 `r-xp` 段**（4×`r--p`+2×`rw-p`+1×`r--s`）⇒ file-offset→VA 不可用
  - §5 by-name 命中：12/12 `hooked by-name …`
  - §6 aux by-name + 偷盗授予：`AddSkillToCharacter/GetSkill_Main/Game.get_Table resolved by-name` +
    `steal picked=10400101 granted=2 … ids=[10000601,10001501]`
  - §7 用户口述：偷盗技能 PASS

## 2. 完整 logcat（不入库，按约定本地留档）

- `09_test/蛋壳战机/precheck_20261028_164602/logcat.txt`（1,309,335 B）、`monkey.log`

## 3. 产物与指纹（当轮最终版 = 修 aux 之后）

| 项 | 值 |
|---|---|
| 正式包 | `08_output/蛋壳战机/蛋壳战机_1.2.1_starry-releaseSigned.apk`（201 MB） |
| md5 / sha256 | `2e0755e1ad113f799433119d7d275edb` / `3499c0d7ff6d57aeb1d281cf1eb7cef324f06da2bbb9c5b6a75051bc2999a84e` |
| 签名 | `CN=StarryMagic`（证书 sha256 `83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d`） |
| 包内 so | `libstarry_magic.so` md5 `0c791a5b3b7c1f108990a0aefa328f3e`（214,168 B） |
| 菜单 dex | `classes5.dex` md5 `41c560fea5fa803d54ee97283d7014c2` |
| unsigned | md5 `978f794f7d420138327bb079f1c694c7` |
| 底包 | `02_apks/蛋壳战机/蛋壳战机_1.2.1_official_vc44_from_emulator.apk` md5 `075b9d6a84ad87679301eb94246f8b92` |
| 设备装机 | `base.apk` md5 == 产物 ✓；运行 so md5 == 包内 so ✓（首装 16:44:14 全新安装；修后同签覆盖，数据保留） |

首版（被替换）：包 md5 `49817640c926a7fda17f24463c22e831` / so md5 `d39b20418480d07d0ba4424e43ecfb3b` —— 偷盗技能 `steal abort`，已作废。

## 4. 锚点

- `10_docs/蛋壳战机/抓点_锚点_1.2.1_CN_20261028.json`（12 hook + 3 aux，序言 12/12 命中；与私服 1.2.1 逐位一致）

## 5. 结论分级

| 项 | 结论 |
|---|---|
| 构建/组装/签名/门禁（L1） | **PASS** |
| 装机完整性 | **PASS** |
| 菜单/鉴权/服务（L2 机器） | **PASS** |
| hook 装上（L2 机器） | **PASS（12/12 by-name）** |
| 偷盗技能（L3 用户） | **PASS**（`steal … granted=2` + 用户口述「已实测，偷盗技能正常」） |
| 攻击倍率 / 上帝模式 / 免广告 | **NOT PROVEN**（用户未测） |
| 拦截上报常开 | **NOT PROVEN**（hook 命中，结算面效果未取信） |
