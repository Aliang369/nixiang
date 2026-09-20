# 证据索引 — 我的勇者 7.6.1 星光复刻（2026-09-20）

> 只列决定性证据、最短复现命令与未证明项；不要求后续会话通读完整 logcat。
> 正文分析见 `../分析_战斗_20260906.md` §13；收尾清单见 `../收尾与最小复现清单_7.6.1.md`。

## 结论

- 官方 7.6.1 (vc203) 原包 → 星光版（无敌 / 五倍攻击 / TapTap 登录绕过）复刻成功，已卸载官方包并装机；**用户实测功能正常**。
- 支持范围：TapTap 渠道 `com.rsg.wdyz` 7.6.1 / vc203；设备 f9a116a2（OnePlus OPD2404，KernelSU root，Android 14）。
- 版本判定：类/方法/参数零漂移；RVA 全漂移已回填；**历史 RVA 口径错误已纠正**（详见下）。

## 决定性证据

| # | 证据 | 文件与定位 | 说明 |
|---|---|---|---|
| 1 | 官包签名与 so 内烘焙证书一致 | `02_apks/我的勇者/我的勇者_7.6.1_base.apk`，apksigner：CN=rn / SHA-256 `ce7afa489c222725…` | 与 `SignSpoof.java` WDYZ_CERT_B64 注释值相同 → 7.6.1 无需换证书 |
| 2 | 类/方法与参数零漂移 | `06_dumped/我的勇者/7.6.1_il2cpp/dump.cs:571366 / 573758 / 576292` | `Battle.{BattleObjectCore,HeroCore,MonsterCore}.SettleDamage(float,float,float,float,int)` 全在 |
| 3 | RVA 重定位锚点（文件偏移口径） | `06_dumped/我的勇者/7.6.1_il2cpp/rva_targets.json` | HeroSettle `0x45F2308`(0x6DBB2C6D) / BaseSettle `0x45B2404`(0xD10503FF) / MonsterSettle `0x45FC6BC`(0x6DBB2BEB) |
| 4 | il2cpp 导出完整 | `llvm-nm -D --defined-only <解包>/lib/arm64-v8a/libil2cpp.so`（12/12 命中） | 按名兜底路径可用（版本漂移时仍能挂）；解包目录已按收尾清理，用复现命令第 2 步再生 |
| 5 | 外科透传未破坏官方内容 | 构建日志 `官方条目透传校验: 19726/19726`；`resources.arsc`/`classes.dex` hash 一致 | 官方 Application（易盾 MyApplication）保留，只加 Provider/Service/权限 |
| 6 | 装机闭环 | `04_scripts/_common/verify_installed_apk.sh f9a116a2 com.rsg.wdyz <APK> --su` → 设备 md5 == 本地产物 `5585d913…` | 防「装了旧包」假成功 |
| 7 | 运行初始化 | logcat（冷启动）：`StarryMagic: 检测到 Unity 引擎` → `初始化我的勇者 / Battle 结算按 RVA Hook...` → `[Wdyz] 初始化成功` → `MOD 已就绪 engine=1 unityGame=1` | 菜单功能名 `[开关] #1 Toggle_无敌` / `#2 Toggle_五倍攻击` 与本游戏一致 |
| 8 | 登录绕过生效 | logcat：`StarryTapLogin: TapTapLoginActivity sanitized=true noopProxy=true`；`AuthClient /v1/auth/login → 200`、`/v1/auth/heartbeat → 200` | 该 SDK 走 `loginHandlers` 清洗路径；`forceOldImplWeb failed(NoSuchField request)` 属预期（新版类名不同），非阻断 |
| 9 | 无崩溃 | 冷启动/重启全程 logcat 无 FATAL / SIGILL / SIGSEGV / ANR | 进程存活 |
| 10 | 功能效果 | **用户实测反馈（2026-09-20 会话）**：无敌 / 五倍攻击正常 | 用户确认，无对应日志留存（见未证明项） |

## 复现命令

```bash
# 1) 取官方底包（真机已装原版时）并核对签名
adb pull /data/app/~~*/com.rsg.wdyz-*/base.apk /tmp/game_id/wdyz_7.6.1_base.apk
~/Library/Android/sdk/build-tools/36.0.0/apksigner verify --print-certs /tmp/game_id/wdyz_7.6.1_base.apk

# 2) 重 dump（得到 Offset 锚点）
cd 01_tools/Il2CppDumper && dotnet Il2CppDumper.dll \
  <extract>/lib/arm64-v8a/libil2cpp.so <extract>/assets/bin/Data/Managed/Metadata/global-metadata.dat /tmp/wdyz761/il2cpp_dump/

# 3) 序言命中自检（Offset 口径；字节小端读作 0x6DBB2C6D 等）
xxd -s 0x45F2308 -l 4 <extract>/lib/arm64-v8a/libil2cpp.so   # eb 2c bb 6d

# 4) 出包（含 so 三查 + md5 闭环 + 透传断言）
cd mod菜单/starry-mod-menu && bash scripts/surgical_wdyz_7.6.1.sh

# 5) 装机（异签名需先卸载官方包；本命令含 STARRY_ALLOW_UNINSTALL 显式开关）
STARRY_INSTALL=1 STARRY_ALLOW_UNINSTALL=1 bash scripts/surgical_wdyz_7.6.1.sh

# 6) 装机闭环 + 日志
bash 04_scripts/_common/verify_installed_apk.sh f9a116a2 com.rsg.wdyz \
  08_output/我的勇者/我的勇者_7.6.1_surgical-releaseSigned.apk --su
adb logcat -s StarryMagic:V MenuService:V StarryApp:V
```

## 未证明项

- CLAIM：本次修正后的 RVA 快路径真的命中（而非退回按名）。
- EVIDENCE：需在功能开关打开后看到 `[Wdyz] HeroSettle hooked @rva 0x45F2308`（若为 `@by-name` 亦属可用，说明运行时 so 与静态 dump 不同源）。
- VERDICT：**NOT PROVEN**（重启后开关读回 OFF，未采集到挂钩行；功能生效已由用户实测确认，兜底路径覆盖该风险）。

- CLAIM：登录链路在 7.6.1 上端到端可用（账号进游戏）。
- EVIDENCE：已见 `sanitized=true noopProxy=true` 与授权后台 200；未见用户登录完成的日志留存。
- VERDICT：**NOT PROVEN**（用户实测可玩，视为通过；如需留证可补抓登录成功后的日志）。

- CLAIM：公告弹窗（§12 修复）在 7.6.1 无回归。
- VERDICT：**NOT PROVEN**（本次未专门复测）。

## 原始资料

- 完整日志：未长期留存（logcat 256 KiB 环形缓冲已滚动）；关键行见上表，冷启动序列可由复现命令第 6 步重抓。
- 原始 dump：`06_dumped/我的勇者/7.6.1_il2cpp/dump.cs`（56 MB，已入库）；`/tmp` 解包与 DummyDll 已清理，可由复现命令第 2 步再生。
- 截图：未留存（本轮未做图像证据）。
- 保留期限或清理条件：dump.cs 与 rva_targets.json 作为 7.6.1 版本档案长期保留；再次换版本时新增同构目录。
