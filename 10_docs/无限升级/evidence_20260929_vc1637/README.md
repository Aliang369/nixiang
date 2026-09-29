# 无限升级 11.266018 / vc1637 星空版（B 线）真机与收尾证据

## CLAIM

1. 真机运行时服务端下发的是 **hotfix2 2,030,080 B** 的 `targetfuntional.dll`，不是分析期假设的
   1,960,960 B；星空包内置三变体（base/hf2/hf3）按运行时长度命中并强制覆写，注入链与菜单均正常。
2. `称号满级` 原先"无效果"，**不是服务端覆盖**，而是 `hounerLevel` 只在 `PlayerGo.ChangeClothes`
   里被读取来选徽章贴图，而该函数只在启动"应用时装"/换装时调用；用户是在游戏运行中拨的开关，
   因此当次无任何变化（该次运行 `title_max=0` 的配置读取即实锤）。该项已按用户要求整体删除。
3. `封号号看不了排行榜` 的第一道坎在客户端：`RankListDataWindow → NetWorkPlayerMgr.GetRank()
   → SendBA("GetRankList")`，而 `NetWorkPlayerBase.SendBA` 带 `if (!GameData.I.ban && ...)` 门，
   `ban=true` 时请求根本不发（`NetBehaviour.SendRT` 本身无此门）。

## 决定性证据（真机 /tmp/wxsj_run2.log，设备 f9a116a2，2026-09-29 16:21）

```text
I/Mod_Menu: [WxsjVC1637] hook installed @base=0x78d6e09000 +0x2b3f3c4 orig=0x7a88498000 variants=hf2:2024960B+hf3:2023424B+base:1955840B
I/Mod_Menu: [WxsjVC1637] Exec call len=2030080 head=4d5a9000 armed=1
I/Mod_Menu: [WxsjVC1637] patched dll injected (vc1637 hf2, hit#1 loaded=2030080 fnv=91f9acbd064fa875 patched=2024960 pad=5120)
I/StarryMagic: [桥接] RegisterNatives 19/19 全部成功
I/StarryMagic: 星光魔法 MOD 菜单已加载 (MainModern tag=wxsj_vc1637)
I/StarryMagic: [wxsj_vc1637] 初始化成功
I/Mod_Menu: [WxsjVC1637] config title_max=0 (exists=1)
I/StarryMagic: [开关] #13 Toggle_称号满级 = OFF
I/AuthClient: POST https://47.100.216.101:8443/v1/auth/heartbeat ← 200
```

设备侧固定化标记（`/data/data/<pkg>/files/starry_hotfix_pinned`）：

```text
vc1637 loaded=2030080 patched=2024960 variant=hf2 action=forced-patch
```

## 关键静态结论

- 挂点：`RuntimeAssemblyLoader::ExecuteRuntimeAssembly` RVA `0x2b3f3c4`（真机 maps 实锤）。
- `GameData.hounerLevel` 全工程只有两处使用：服务端 `NetWorkPlayerMgr.Funtion` case `"称号等级"` 写、
  `PlayerGo.ChangeClothes(Sprite)` 读来选 `angleSign_icon` 贴图（满级=20）。纯展示、无属性收益。
- 封号相关方法在本补丁 DLL 中为空实现：`InfinityCoreNormal.NetConver.CheckGamePlayerAction` /
  `ForceBanGameAciton` / `UpDateBanStatu`、`NetWorkPlayerMgr.BanGameAciton`；
  `BanGame()` / `UnBan()` 直接 `GameData.I.ban = false`。故本包不会因封号标志而停发请求。

## 正式包（本轮唯一终包）

- 路径：`08_output/无限升级/无限升级_11.266018_vc1637_starry-releaseSigned.apk`
- 大小：`115904217` B；SHA-256：`32bb13cdc79b098539de18186091bfdf041656d7606bb679d429206febcfc176`
- 签名：v2 + v3，`CN=StarryMagic`；versionCode 1637 / versionName 11.266018
- 菜单 12 项（feat 1/2/4–13），已删除 `称号满级`；so `a4a295195596…`
- 设备 f9a116a2 同签名 `--upgrade` 覆盖安装，设备 `base.apk` md5 与产物一致
  （`d40a6239c38a5957b50c02874c1dbe78`），`firstInstallTime` 未变 ⇒ 存档保留

## VERDICT
## 收尾清理（2026-09-29）

- `08_output/无限升级` 已用 `prune_output_final.sh` 裁剪为唯一终包（本轮 APK）；被删的
  `无限升级_12.10701_unshelled-menu-releaseSigned.apk` 与旧 `.idsig` 记入
  `10_docs/无限升级/正式包裁剪清单_vc1637_20260929.json`。
- 本机 `/tmp/wxsj*` 临时产物（分析反编译目录 / 运行日志 / 截图 / 中间 DLL）已全部删除，约 `234 MB`。
- 设备 f9a116a2 删除 `/data/local/tmp/wxsj_bundle_*` 与 `wxsj_manifest.bytes`（本轮资源调查残留，约 `844 KB`）。
- 保留（不可再生输入）：设备 `wxsj_salvage_current.sav`、`starry_magic` 种子、官方底包
  `07_patched/无限升级/installed_official_11.266018_vc1637_base.apk`、三版官方热更 DLL 与三份补丁 DLL。


- 注入链 / 三变体命中 / 菜单加载 / 鉴权心跳：真机通过。
- IL 层功能（无敌、秒杀、免广告、物品不减、技能免费升级、广告奖励翻倍、物品翻倍、无限挑战秘境、
  皮肤全解锁、功能全解锁、经验倍率、商店广告无限）：静态通过，逐项**运行效果 NOT PROVEN**。
- `商店广告无限` 是否被服务端独立限次拦截：NOT PROVEN。
- 封号号排行榜：客户端侧已通（ban 恒 false）；**服务端是否为封号账号回 `GetRanListCallback` NOT PROVEN**。
- 版本钉死与防封的长期运行证据：NOT PROVEN。
