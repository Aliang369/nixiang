# 敢达争锋对决 · 星空版实测证据索引（2026-10-03）

> 结论一句话：**星空版 4 个抓点全部命中**（`[gdzfdj] install pass: ok=4 dead=0/4`），内存落点实测为星空自己的 16 字节绝对 veneer，指向 `libstarry_magic.so` 内的 Dobby trampoline；功能全开下登录链跑通、无崩溃。
> 关联文档：[移植记录_星空版_9.3.0_20261003.md](../移植记录_星空版_9.3.0_20261003.md) · [星空hook锚点_20261003.json](../星空hook锚点_20261003.json) · [斌哥抓点](../斌哥hook锚点_20261003.json) · [斌哥证据目录](../evidence_20261003/README.md)
> 设备/进程：MuMu `emulator-5554`，`com.kingnet.gundam.mg.normal` pid **63814**（2026-10-03 15:27 启动）

## 1. 决定性证据

| # | 结论 | 证据（本目录文件 / 日志行） |
|---|---|---|
| 1 | 4 个抓点全部装上钩 | `starry_logcat_verified.txt`：`[gdzfdj] install pass: ok=4 dead=0/4` + 4 条 `hooked by-name ...` |
| 2 | 装钩走 by-name 而非 RVA | 同文件：`UpdateAI rva 0x3D9CF54 not in RX — try by-name`（四点各一条）⇒ MuMu/houdini 下 guest text 页显示 `r--p`，RVA 快路被 RX 守卫拒绝 |
| 3 | 四点内存字 = 16 字节绝对 veneer | `hookprobe_on_state.txt`：`D0FCA3B1 / B0FCA2D1 / 90FB52B1 / 90FB52B1`，均 ≠ 原 prologue（`F81D0FFE / D10383FF / A9BF4FFE / FC1E0FE8`） |
| 4 | veneer 指向本 mod 自己的 so | 解码 `ADRP X17,0x400020812000 + ADD + BR X17` ⇒ 目标 `0x400020812DBC / 0x400020812DF0 / 0x400020812E9C / 0x400020812EE4`，落在 `libstarry_magic.so` guest `0x400020800000`+0x26000 |
| 5 | guest base 自校准 | 同文件：`[Il2Cpp] phdr match: .../libil2cpp.so base=0x400023600000` ⇒ 推翻早前 0x400025000000 的错误记录 |
| 6 | 签名伪装生效 | `starry_logcat_verified.txt`：`StarrySignSpoof: spoof hit pkg=com.kingnet.gundam.mg.normal md5=28185fbfd55a46a024bd75bd1569fd72` |
| 7 | 注入链完整 | 同文件：`RegisterNatives 19/19 全部成功`、`StarryBridgeTest: PASS=15 FAIL=0`、`ensureLoaded=true`、`MenuService.start ok`、`Background started FGS`、`MOD 已就绪 tag=gdzfdj ready=1` |
| 8 | 菜单三项已注册 | 同文件：`[gdzfdj] probe: nav=Category_战斗 dmg=Slider_伤害倍率:1:100:1 root=Toggle_超级定怪 spd=Slider_全局加速:1:10:1` |
| 9 | 授权链路活 | 同文件：`[验证] grant ok exp=1791098349` → `renew ok exp=1791098861` |
| 10 | 稳定性 | 同文件内含 `OnLoginSucc` / `run_client.php err_no:0` / `session.php` / `Msg_Login_VerifyAccount_CS`，且无 `SIGSEGV` / `FATAL` |
| 11 | 被验包 == 交付物 | `08_output/敢达争锋对决/敢达争锋对决_9.3.0_surgical-starSigned.apk` 336,405,961 B，APK sha256 `1b4e9a3b1e00f32f16234e345ef585903a8c1387f6a300791de70f96b9689bdc`，与设备已装 `/data/app/~~ULq5Ycd4GBvIAi_P3ZcTqQ==/com.kingnet.gundam.mg.normal-VJJB1tcpZf_AhKDbh68sOA==/base.apk` 的 `sha256sum` **逐字节一致** ⇒ 本目录所有实测都跑在这一份产物上 |
| 12 | 签名可用 | `apksigner verify --print-certs -v` → Verifies，v1/v2/v3 = true；DN `CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN`；证书 SHA-256 `83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d`、SHA-1 `821c92e404ab5fdac8dbf157c2e467e71440917d`、MD5 `161fca32a36d3df274458682a44cb774`（RSA 2048） |

## 2. 复现命令

```bash
# ① 装上星空版并启动（装机前先按移植记录 §6.3 处理僵尸包）
adb -s emulator-5554 shell am start -n com.kingnet.gundam.mg.normal/com.kingnet.gundam.mg.MainActivity

# ② 驱动任一功能 ON（三开关全关时按设计不装钩）—— Frida Java 桥直调 static native
python 04_scripts/敢达争锋对决/gdzfdj_javabridge_set.py --dmg 20 --root --spd 3

# ③ 看装钩日志
adb -s emulator-5554 shell "logcat -d -v time" | grep -E 'gdzfdj|install pass|hooked'

# ④ 直读 guest 内存核对四点
python 04_scripts/敢达争锋对决/gdzfdj_hookprobe.py --pid $(adb -s emulator-5554 shell pidof com.kingnet.gundam.mg.normal)
# 现场原始输出见 hookprobe_on_state.txt（预期：已挂钩 4 / 原始 0 / 读取失败 0）

# ⑤ 关闭功能（钩子常驻，语义透传）
python 04_scripts/敢达争锋对决/gdzfdj_javabridge_set.py --off

# ⑥ 核对「被验包 == 交付物」与签名（本次交付的封口证据）
sha256sum "08_output/敢达争锋对决/敢达争锋对决_9.3.0_surgical-starSigned.apk"
adb -s emulator-5554 shell "sha256sum $(adb -s emulator-5554 shell pm path com.kingnet.gundam.mg.normal | tr -d '\r' | sed 's/^package://')"
"C:\Users\Administrator\AppData\Local\Android\Sdk\build-tools\36.1.0\apksigner.bat" verify --print-certs -v \
  "08_output/敢达争锋对决/敢达争锋对决_9.3.0_surgical-starSigned.apk"
```

## 3. 未证明项 / 边界

| 项 | 状态 |
|---|---|
| RVA 快路（`hooked @rva 0x…`） | ⏳ 未验证 —— MuMu/houdini 下降级 by-name；真机 arm64 应命中，两路径功能等价 |
| 三项功能实战效果 | ✅ 用户已实测正常（伤害倍率 / 超级定怪 / 全局加速），2026-10-03 结案 |
| 关闭功能后是否需要卸载钩子 | ✅ 已证**不需要也不支持**：OFF 后四点仍是 veneer，靠 handler `mul<=1` 透传 |
| 菜单卡密验证 / 后台功能门控 | ⚪ **范围外** —— 由后台管理，本工程不涉及服务端登记或验证 |

## 4. 原始资料

| 文件 | 内容 |
|---|---|
| `starry_logcat_verified.txt` | 849 行现场 logcat（自检 / 装钩 / 授权 / 登录 / il2cpp by-name 解析全量行） |
| `hookprobe_on_state.txt` | `gdzfdj_hookprobe.py --pid 63814` 原始输出（guest_base + 四点内存字与判定） |
| `../星空hook锚点_20261003.json` | 机读版锚点（4 钩 / 注入机制 / 验证结果 / 复现命令 / 遗留项） |
| `../移植记录_星空版_9.3.0_20261003.md` | 人类可读版移植记录（含构建与装机铁律、踩坑表） |
