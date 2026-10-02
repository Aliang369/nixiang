# evidence_20261002 — 疾风骑士 60.0.1 星光版 装机与功能验收

游戏：`com.emay.wxsy.topon` 60.0.1 / vc116（TapTap 渠道，Unity IL2CPP Metadata v31，arm64-v8a）
设备：`f9a116a2`；装机会话 pid 5147；日期 2026-10-02

本目录只留决定性证据行（`logcat_decisive_20261002.txt`）；两张 PNG 是本机视觉 QA 用，不入库（与仓内「真机交互零截图」约定一致）。

## 决定性证据（按重要性）

1. `logcat_decisive_20261002.txt`（新包冷启动 20:39:36，pid 5147）
   - `StarryApp: onCreate proc=com.emay.wxsy.topon` — 注入路线 (c) 生效（`application@name` → `com.starry.magic.StarryApp extends com.mar.sdk.MARApplication`）
   - `MenuService: auth game_id=jfqs` / `native menu shown mode=ACTIVITY` / `overlayVisible=true attached=true host=MarMainActivity` — 菜单授权通过、悬浮层挂上
   - `[jfqs] lib ready (dual-path, lazy hooks)` — native 侧就绪
   - `[jfqs] fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=65536)` — 致命信号兜底已武装
   - **54 条 `hooked <类>.<方法>(argc=N)`** — 开关打开后 54 个槽全部装钩成功，零 `dobby fail` / 零 `熔断` / 零 `signal 11`
   - `MRSDK_ShowReward6 class miss MarSDKManager — degrade` 紧跟 `MRSDK_ShowReward6 hooked MarSDKManager.ShowReward(argc=6)` —— degrade 只是「首个 image 未命中」的降级分支，随后按名路径命中并装钩成功，非失败
   - `trace <fn> orig=<v>` 探针行证明 hook 在实战中被真实调用（技能/移动/神器/协同等）
2. 装机完整性：设备内 `lib/arm64-v8a/libstarry_magic.so` md5 == 本地产物 md5（`71740d478eb9571566bb8a51513c781a`），`pm install -r -d --user 0` → `Success`，同签名覆盖、登录态与存档保留
3. 用户回执验收（m02751 / m02797）：「功能都没问题了」——17 项菜单全部实测通过

## 关键指纹

| 项 | 值 |
|---|---|
| 唯一正式包 | `08_output/疾风骑士/疾风骑士_60.0.1_surgical-releaseSigned.apk`（625,404,073 B） |
| 包 md5 / sha256 | `d4e2507eb74db349a1550fd756e861a9` / `6800c619abfcc3030633cf205012f2488728849dc03d7a9f0cfcff365dd41b6a` |
| 包签名 | `CN=StarryMagic`（v1+v2+v3，`META-INF/STARRY.{SF,RSA}`） |
| native so（v6） | `libstarry_magic.so` md5 `71740d478eb9571566bb8a51513c781a` |
| 底包（原版） | `02_apks/疾风骑士_60.0.1_原版.apk` 623,957,976 B md5 `033c55fec862c39d2d1c5723e56377d0` |
| 对照物（斌哥改包） | `02_apks/疾风骑士_60.0.1_com.emay.wxsy.topon.apk` 580,965,344 B md5 `7b9d5a3caf0a8bf1b07716b95d7aacad` |
| 原厂证书 MD5 | `e28a5f390ab1a7590a032f05b3fde72a`（CN=Morefuntek，SignSpoof 回灌值） |
| 菜单 | `authGameId=jfqs`，17 项（feat 0–16） |

## 最短复现命令

```bash
cd /Users/aliang/Documents/nixiang
# 1) 包身份
md5 08_output/疾风骑士/疾风骑士_60.0.1_surgical-releaseSigned.apk
# 2) 设备侧 so 与本地产物一致
adb -s f9a116a2 shell 'su -c "md5sum $(pm path com.emay.wxsy.topon | sed s/package:// | xargs dirname)/lib/arm64/libstarry_magic.so"'
# 3) 清缓冲 → 启动 → 抓装钩
adb -s f9a116a2 logcat -c
adb -s f9a116a2 shell am start -n com.emay.wxsy.topon/com.mar.sdk.permission.MARPermissionActivity
adb -s f9a116a2 logcat -d -s Mod_Menu:V StarryApp:V MenuService:V | grep -c hooked   # 需用户在菜单里开功能
# 4) 广告是否真弹（免广告验证）
adb -s f9a116a2 logcat -d | grep -E 'MARSDK-AD|ad skipped'
```

## 已验收（用户回执）

- 伤害倍率 / 超级无敌 / 无限货币 / 移动速度 / 技能无限 / 无限跳跃 / 免广告 / 解锁全角色·皮肤·武器·宠物·神器·全部技能·协同 / 百分百改造成功 —— 用户口径「功能都没问题了」

## NOT PROVEN

- **免广告的实际生效路径**：日志中无 `ad skipped` 行，`ADXManager.ShowRewardedAd` 未被调用过；真实链路走 MarSDK（Java `com.mar.sdk.gg.topon.ToponAd`）。免广告由 `MarSDKManager.ShowReward`(argc 6/7) 承担，但该 hook 是否在每次广告位都被命中未逐条日志证明。
- **`MarSDKManager` 不在 Assembly-CSharp.dll**（`FindClassInImage` 扫 8191 类未命中）—— 依赖 `FindClass` 的跨 image 兜底路径，跨版本可能变化。
- **全部挂点均为 by-name（`rva=0, expectInsn=0`）**：抗 RVA 漂移，但换包版本后类名/方法名漂移会静默降级（只降级、不整包失败）。
- **`JFQS_TRACE` 探针在出货二进制里仍为 1**（每 hook 前 3 次调用打 `trace` 行，噪声有界）。下次重编时置 0。
- 云同步一致性：无限货币为本地读满 + `UseGold` no-op，未验证服务端对账。
- 解锁类改为真写存档（`Load*Unlock` → 对应 `Save*Unlock`，`MemoFirst` 去重），但**未验证关闭功能后重启是否仍拥有**（v4 卡死「尚未拥有该宠物」的回归测试）。
