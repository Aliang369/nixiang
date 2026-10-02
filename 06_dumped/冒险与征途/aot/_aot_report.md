# 冒险与征途 · hot1.unity3d → Assembly-CSharp.dll 元数据解包报告

> 生成脚本：`_unityfs_lib.py` / `_step1.._step9*.py` / `_mdread.py` / `_step11_final.py` / `_step12_ad.py` / `_step13_report.py`（均在 `E:\nixiang\06_dumped\冒险与征途\aot`）
> 纯 Python 标准库（zlib/lzma/struct/mmap 级），无外网、无 capstone/lief/pyelftools/lz4/UnityPy。

## 1. 结论摘要

1. **DLL 已成功取出**：`E:\nixiang\06_dumped\冒险与征途\aot\Assembly-CSharp.dll`（22647808 字节，sha256=`7fa836eeed42257f7a35a048281af4e3bdcd620e471d0852b5788207597be03d`），PE/CLR/元数据全部自校验通过。
2. **该 DLL 是完整可解析的 .NET 程序集**：TypeDef=14971，MethodDef=173184，程序集名 `Assembly-CSharp`，
   元数据 v4.0.30319 / CLR runtime 2.5（HybridCLR 热更程序集，AOT 部分在 libil2cpp.so 里）。
3. **7 个目标名全部定位**：`Damage`/`DoAttack`/`AddProjectile`/`ShowWin` 是方法名；
   `Character`/`StarsCollider`/`MW_GameNotice` **在这份 DLL 里不是方法名而是类名**（同名方法 0 命中），已给出类与全部方法。
4. **广告结论：游戏内确实有「看广告 / 看激励视频得奖励」的完整代码路径**（不是只有 SDK 占位）：
   客户端 `DS_Advertising` → 三方 SDK `PlayADVideo/showRewardVideoAd` → 服务器 `start_watch_ad_req`/`finish_watch_ad_req` → 发奖 `DispatchResult`。

## 2. UnityFS 头字段实测值（输入包）

文件：`E:\nixiang\06_dumped\冒险与征途\aot\hot1.unity3d`（9534635 字节，sha256=`2c49e331bc10752f653825e5c22b8a802414de267e2c4263a1414956d814f2f7`）

| 字段 | 实测值 |
|---|---|
| signature | `UnityFS` |
| version | **8** |
| unityVersion | `5.x.x` |
| unityRevision | `2022.3.14f1` |
| size | 9534622（= 文件大小 - 13） |
| compressedBlocksInfoSize | 862 |
| uncompressedBlocksInfoSize | 1811 |
| flags | **0x243** |
| 头起始偏移 | **0x0D**（文件头有 13 字节垃圾前缀 `55 6e 69 74 79 46 53 00 37 f3 57 12 9a`） |
| blocksInfo 的 LZ4 流起点 | **0x4D**（0x3F..0x4C 是 16 字节对齐填充） |
| 数据区起点 | **0x3AD**（= 0x4D + 862 + 2 字节尾填充；`len(file)-0x3AD == Σcsize == 9533694`） |
| blocks / nodes | 173 块（flags&0x3F 全为 3）/ 1 个 node `CAB-fac7085faa161ea4294c5251c5071ee0` |

**压缩方式**：flags 低 6 位 = 3。Unity 枚举里 **3 = LZ4HC（不是 LZMA）**，1=LZMA / 2=LZ4 / 3=LZ4HC；
2 与 3 解压方式相同，都是 LZ4 block 格式。任务书里「3=LZMA」的假设不成立 —— 按 LZMA 解会失败，
按自写 LZ4 block 解压器（token 高 4 位=literal、低 4 位=match-4、offset 2 字节小端）解则一次成功，
blocksInfo 解出 1811 字节、173 个数据块全部与头里的 uncompressedSize 一致（173/173 零失配）。
DLL 从**解压后数据的 0xA4C 偏移**开始（目录项在 0xA34 给出文件名 `Assembly-CSharp.dll`，0xA48 给出大小 22647808）。

## 3. DLL 自校验与元数据布局

- `MZ`@0x0、e_lfanew=0x80、`PE\0\0`@0x80、machine=**0x14C**（i386 / AnyCPU-32）、3 个 section：.text vsize=0x1598828 vaddr=0x2000 rawsz=0x1598A00 rawoff=0x200 / .rsrc vsize=0x428 vaddr=0x159C000 rawsz=0x600 rawoff=0x1598C00 / .reloc vsize=0xC vaddr=0x159E000 rawsz=0x200 rawoff=0x1599200
- CLR 头：cb=0x48、runtime=2.5、MetaDataRVA=0x90CBA0、MetaDataSize=0xBB2A50
- 元数据根 `BSJB` @DLL 偏移 **0x90ADA0**，version `v4.0.30319`，5 个流：
  - `#~` off=0x90AE0C size=8003212
  - `#Strings` off=0x10ACC98 size=2305232
  - `#US` off=0x12DF968 size=655400（解析出 18372 条用户字符串，Blob 尾部奇数字节为 flag，**无前置 flag 字节**）
  - `#GUID` off=0x137F990 size=16 / `#Blob` off=0x137F9A0 size=1302096

## 4. 元数据统计

- 程序集名 **Assembly-CSharp**（AssemblyRef 47 个）；**TypeDef=14971，MethodDef=173184**
- Field=118,820 / Param=80,741 / Property=50,742 / MethodSemantics=68,211 / MemberRef=24,574
- CustomAttribute=26,249 / TypeSpec=6,870 / NestedClass=5,483 / MethodSpec=6,154 / TypeRef=647 / GenericParam=353
- 表区结束于 0x10ACC94（= `#~` 起点 0x90AE0C + 8003212 − 4 填充），与解析结果完全对齐 → 表解析可信

## 5. 目标名字答案（8 个）

| 目标名 | 性质 | 归属 |
|---|---|---|
| `Damage` | 方法 | ActionGameLibrary.Character；ActionGameLibrary.InteractiveGameObject |
| `Character` | 类 | ActionGameLibrary.Character |
| `DoAttack` | 方法 | ActionGameLibrary.StarsCollider |
| `StarsCollider` | 类 | ActionGameLibrary.StarsCollider |
| `AddProjectile` | 方法 | ActionGameLibrary.ActionCommand_AddProjectile；ActionGameLibrary.GameObjectManager；BaseUIMiniMap |
| `ShowWin` | 方法 | HelpDefaultTips；MW_AFKAchievement；MW_AFKSkillGachaNewWishSelect；MW_AFKSkillGachaWishSelect；MW_AFKTeamBattleWin；MW_AbyssDropAtlas；MW_ActivityTimesBuy；MW_CelebrationShop；MW_DarkEnergyDropAtlas；MW_EquipDropAtlas；MW_EquipIdentify；MW_EquipRecastLibrary；MW_EquipRecastResult；MW_EquipmentInheritResult；MW_EquipmentInheritSelect；MW_EquipmentInheritVerify；MW_ExchangeActShop；MW_FriendGetGift；MW_GMSimple；MW_GameNotice；MW_GuildActivityMemberDetail；MW_GuildAltar；MW_GuildApply；MW_GuildApplyList；MW_GuildAvatarSetting；MW_GuildBargain；MW_GuildBargainConfirm；MW_GuildBaseInfo；MW_GuildBlessHelp；MW_GuildBossDamageRank；MW_GuildBossRank；MW_GuildBossResult；MW_GuildBossReward；MW_GuildBuildList；MW_GuildCreate；MW_GuildInviteList；MW_GuildJobPermissions；MW_GuildLog；MW_GuildMemberList；MW_GuildNotice；MW_GuildRaidShop；MW_GuildRank；MW_GuildRecharge；MW_GuildRename；MW_GuildSignIn；MW_GuildUnBargainMember；MW_KingRacingGuildScoreInfo；MW_KingRacingLimitedTimeSprint；MW_LimitedTimeSprintTaskTips；MW_LuckykoiJoin；MW_LuckykoiReward；MW_MagicCardComposeConfirm；MW_MagicCardComposeResult；MW_MagicCardLevelUp；MW_MagicCardLevelUpResult；MW_MagicCardLevelUpSelect；MW_MallQuiteBuy；MW_NumberRunResult；MW_PartnerIntoBattle；MW_PartnerLibInfo；MW_PartnerLibScoreTips；MW_PartnerLibUpgrade；MW_PartnerSkillVideo；MW_PartnerStarResult；MW_Privacy；MW_RelicAttrFilter；MW_RelicGachaResult；MW_RelicImprovePanelNew；MW_RewardShow；MW_RoleTitleAttrib；MW_SeceretRealmDropAtlas；MW_SectSetMissRecord；MW_SingleArenaDamage；MW_SingleArenaRank；MW_SingleArenaRecord；MW_SingleArena_BattleDefense；MW_SpaceTimeDropAtlas；MW_TeamExploration；MW_TeamExplorationDoor；MW_TeamExplorationDoorEvent；MW_TeamExplorationEvent；MW_TeamExplorationFund；MW_TeamExplorationRecord；MW_TeamExplorationShop；MW_TeamExplorationSpcEvent；MW_TeamTarget_Set；MW_WardrobeAttr；UI_BossBuffShow；UI_FriendPrivateChatTips；UI_SystemOpenFlyEffect |
| `MW_GameNotice` | 类 | MW_GameNotice |

### Damage
方法名精确命中 **3** 处：
- `ActionGameLibrary.InteractiveGameObject :: Damage(long, DamageEnumerate.DamageType, bool) -> void`  token=0x0601E0B9
- `ActionGameLibrary.Character :: Damage(long, DamageEnumerate.DamageType, bool) -> void`  token=0x0601F131
- `ActionGameLibrary.Character :: Damage(BigInt, DamageEnumerate.DamageType, bool) -> void`  token=0x0601F132

### Character
方法名精确命中 **0** 处 → 它在目标表里是**类名**。
类名命中：`ActionGameLibrary.Character`（31 个方法）

### DoAttack
方法名精确命中 **1** 处：
- `ActionGameLibrary.StarsCollider :: DoAttack(ActionGameLibrary.IAttack, ActionGameLibrary.IAccept, ActionGameLibrary.AttackData, ActionGameLibrary.AcceptData, UnityEngine.Vector3, int, ActionGameLibrary.SynHitData) -> void`  token=0x0601E8D9

### StarsCollider
方法名精确命中 **0** 处 → 它在目标表里是**类名**。
类名命中：`ActionGameLibrary.StarsCollider`（45 个方法）

### AddProjectile
方法名精确命中 **3** 处：
- `BaseUIMiniMap :: AddProjectile(ActionGameLibrary.Event) -> void`  token=0x060040B4
- `ActionGameLibrary.GameObjectManager :: AddProjectile(ActionGameLibrary.ActionGameProjectile) -> void`  token=0x0601E994
- `ActionGameLibrary.ActionCommand_AddProjectile :: AddProjectile(ActionGameLibrary.Character, ActionGameLibrary.ActionGameObject, ActionData.LAddProjectile, int, UnityEngine.Vector3, UnityEngine.Vector3, ActionGameLibrary.Action, int, int, ActionGameLibrary.IActionGameObject, int, bool, int, int, bool, int, int) -> ActionGameLibrary.ActionGameProjectile`  token=0x0601EB9A

> 任务书说 `AddProjectile` 出现 2 次；实测是 **3 处、3 个不同类的 3 个不同签名**（无同签名重载）：
> ① `BaseUIMiniMap::AddProjectile(ActionGameLibrary.Event)` —— 小地图 UI 的事件图标添加，**与投射物无关**；
> ② `ActionGameLibrary.GameObjectManager::AddProjectile(ActionGameProjectile)` —— 战斗对象管理器登记投射物实例（**最像「投射物进入世界」的收口点**）；
> ③ `ActionGameLibrary.ActionCommand_AddProjectile::AddProjectile(Character, ..., ActionData.LAddProjectile, ...) -> ActionGameProjectile` —— 战斗指令「添加投射物」的执行体（输入发射者+行为数据，输出投射物）。
> 要改「射出子弹/箭矢」的逻辑，看 ②③（都在 `ActionGameLibrary` 战斗命名空间）；① 可忽略。

### ShowWin
方法名精确命中 **94** 处：
- `MW_RoleTitleAttrib :: ShowWin() -> void`  token=0x06002A45
- `MW_LuckykoiJoin :: ShowWin(int) -> void`  token=0x06002E95
- `MW_LuckykoiReward :: ShowWin(int) -> void`  token=0x06002E9E
- `MW_TeamExploration :: ShowWin(int) -> void`  token=0x06003070
- `MW_TeamExplorationDoor :: ShowWin(System.Action) -> void`  token=0x060030DC
- `MW_TeamExplorationDoorEvent :: ShowWin(GameConfigData.MapEvent, System.Action) -> void`  token=0x060030E6
- `MW_TeamExplorationEvent :: ShowWin(GameConfigData.MapEvent) -> void`  token=0x060030F0
- `MW_TeamExplorationFund :: ShowWin(int, int) -> void`  token=0x060030FE
- `MW_TeamExplorationRecord :: ShowWin() -> void`  token=0x06003137
- `MW_TeamExplorationShop :: ShowWin(int, int) -> void`  token=0x0600315A
- `MW_TeamExplorationSpcEvent :: ShowWin(GameConfigData.MapEvent) -> void`  token=0x06003165
- `MW_GMSimple :: ShowWin(string) -> bool`  token=0x060042EB
- `MW_AbyssDropAtlas :: ShowWin(int) -> void`  token=0x0600454C
- `MW_CelebrationShop :: ShowWin(int) -> void`  token=0x0600462F
- `MW_AFKTeamBattleWin :: ShowWin() -> void`  token=0x06004997
- `MW_EquipDropAtlas :: ShowWin(int, int) -> void`  token=0x06004A35
- `MW_AFKSkillGachaNewWishSelect :: ShowWin(int, int, int) -> void`  token=0x06004EDC
- `MW_AFKSkillGachaWishSelect :: ShowWin(int, int, int) -> void`  token=0x06004EE8
- `UI_BossBuffShow :: ShowWin(GameConfigData.StateData, UnityEngine.Vector3, UnityEngine.Vector3, bool) -> void`  token=0x06005374
- `MW_SeceretRealmDropAtlas :: ShowWin(int, int) -> void`  token=0x06005601
- `MW_AFKAchievement :: ShowWin(int, string) -> void`  token=0x060056B4
- `MW_DarkEnergyDropAtlas :: ShowWin() -> void`  token=0x06005786
- `MW_EquipmentInheritResult :: ShowWin(ulong) -> void`  token=0x060057C2
- `MW_EquipmentInheritSelect :: ShowWin(SystemKey, UnityEngine.Vector3, System.Action`1<EquipInfo>, EquipInfo, EquipInfo) -> void`  token=0x060057C6
- `MW_EquipmentInheritVerify :: ShowWin(EquipInfo, EquipInfo, System.Action, int, bool, bool) -> void`  token=0x060057D2
- `MW_SingleArenaDamage :: ShowWin(bool, int, int) -> void`  token=0x060058EE
- `MW_SingleArenaRank :: ShowWin(int) -> void`  token=0x060058F4
- `MW_SingleArenaRecord :: ShowWin() -> void`  token=0x060058FA
- `MW_SingleArena_BattleDefense :: ShowWin(int, System.Collections.Generic.List`1<proto_ff.Item>) -> void`  token=0x0600590A
- `MW_MagicCardComposeConfirm :: ShowWin(System.Collections.Generic.List`1<int>, System.Action) -> void`  token=0x060059BE
- … 其余 64 处见 `hotupdate_hits.txt`

> 94 处全部是**各玩法结算/开奖 UI 的 ShowWin 弹窗**（如 `MW_LuckykoiReward::ShowWin(int)`、`MW_SingleArenaRank::ShowWin(int)`、
> `MW_GameNotice::ShowWin(string,string)`），**没有战斗内的胜负判定逻辑**；战斗胜负应找 `BattleManager`/`*Result` 类。

### MW_GameNotice
方法名精确命中 **0** 处 → 它在目标表里是**类名**。
类名命中：`MW_GameNotice`（5 个方法）

## 6. 广告 / 激励视频 结论

**结论：有。** 这份热更程序集里存在完整的「看广告（激励视频）得奖励」链路，且有服务器协议与配置表配套，不是遗留占位代码。

### 6.1 最像入口的 3 条路径

1. **`DS_Advertising`（广告数据段，广告系统主控制器）**
   - `DS_Advertising::WatchAdvertisements(int) -> void`（token `0x06001251`）—— 顶层入口
   - `DS_Advertising::StartWatchAdReq(int)`（`0x06001252`）→ 发 `start_watch_ad_req`
   - `DS_Advertising::OnADVideoResult(bool,int,ulong)` / `ReqADVideoResult` / `RspADVideoResult(Proto_CSRsp)` → `finish_watch_ad_req/rsp`
   - `DS_Advertising::DispatchResult(int,bool)` —— 结果分发/发奖；`GetADTimes(int)->int`、`GetADSysParam(int)->string`、`GetADCdsParam(int)->int`、`get_FunctionEnable()->bool`
2. **三方 SDK 激励视频（妙月 MiaoYue）**
   - `BaseSDKAdapter::PlayADVideo(int,string)` / `MiaoYueSDKProxy::PlayADVideo(int,string)` / `MiaoYueSDKGameObject::PlayADVideo(Hashtable)`
   - 回调 `MiaoYueADCallBack::onRewardArrived(bool,AndroidJavaObject)`；日志串 `MiaoYue SDK showRewardVideoAd`
3. **悬浮广告吉祥物 AdMascotManager（游戏内可见的「点广告领奖」UI）**
   - `AdMascotManager::OnClickWatchAd()`、`CanClaimReward()->bool`、`GetNextRewardTime()->uint`、`SetRewardClaimed(bool)`
   - `GetAdRewardInfo(ref int,ref int,ref bool)`、`RefreshAdRewardData()`、`OnFloatingAdRewardDataChanged(GEvent)`
   - 数据来自 `proto_ff.Proto_SCFloatingAdRewardDataNtf`；`DS_MainCity::FloatingAdRewardDataNtf(proto_ff.Proto_CSRsp)`

### 6.2 判定依据（证据链）

- **协议**：`proto_ff.Proto_CSStartWatchAdReq/Rsp`、`proto_ff.Proto_CSFinishWatchAdReq/Rsp`、`proto_ff.Proto_SCFloatingAdRewardDataNtf`；
  角色数据 `proto_ff.AccAdvertiseData{ ad_id, ad_uuid, ad_start_time }` 挂在 `proto_ff.RoleData::advertise_data` 上。
- **配置表**：`GameConfigData.ConfigData::get_CFG_AdvertisingRewards()`、`get_CFG_FloatingAdReward()`、`get_CFG_WatchAdvertisement()`；
  类型 `GameConfigData.AdvertisingRewards` / `FloatingAdReward` / `FloatingAdConst` / `WatchAdvertisement`。
- **枚举/字段**：`EN_REWARD_NOTIFY_TYPE_WATCH_AD_REWARD`、`ERR_CODE_FLOATING_AD_REWARD_NOT_IN_TIME`、
  `EN_ICR_FLOATING_AD_REWARD`、`E_CS_FLOATING_AD_REWARD_DATA_NTF`、`EVENT_WATCHAD_RESULT`(串 `Event_WatchADResult`)、
  各玩法「看广告位」开关字段：`AFKBlessAdShow`/`AFKDailyBossAdShow`/`ArenaAdShow`/`TormentTowerAdReward`/`MallPropItemAdShow`/`NewGuildDailyLivenessAdReward` 等。
- **UI 入口**：`UI_PanelDailyBoss::OnClickBtnAdvertise()`、`DS_MallData::IsShowAdFree(int)->bool`、`MW_MallQuiteBuy::RefreshADShow(GEvent)`、`MW_TowerJingMo::SetWatchAdData()`、`MW_AFKBlessLevelAD`（挂机祝福等级广告）。
- **#US 用户字符串（UTF-16 原文，决定性）**：
  - `0x12EC58F`  是否确认进入广告系统 观看广告获取奖励\n开通月卡可无限制的[d57917]免看广告[-]直接领取
  - `0x12EC5F1`  进入广告
  - `0x12EC683`  已使用免广告特权跳过观看
  - `0x1311241`  观看视频后，缩短{0}分钟升级时间
  - `0x1309647`  广告次数已用完
  - `0x131A1F4`  飘屏广告已观看
  - `0x1327AA6`  开通月卡后，每日广告收益次数+{0}\n是否前往开通？
  - `0x1302356`  MiaoYue SDK showRewardVideoAd
- **#US 字段名**：`start_watch_ad_req/rsp`、`finish_watch_ad_req/rsp`、`watch_ad_times`、`last_watch_ad_time`、
  `active_double_reward_req/rsp`、`use_reward_double`、`is_double`、`isRewardValid`、`FloatingAdRewardData_Change`。
  （`active_double_reward_req` + `use_reward_double` 即「看广告双倍奖励」这一类的服务端请求/消耗标记。）
- **免广告能力**：`已使用免广告特权跳过观看` + `开通月卡可无限制免看广告` → 月卡用户走跳过分支（同一套 DispatchResult 里的分支）。

### 6.3 广告相关统计

- 强相关(词元级命中广告词) **类型 20 个 / 方法 44 个**；中相关(Reward/SDK/Video 词元级) **类型 681 / 方法 2179**；弱相关 14/796。
- 属性 16 个、字段 46 个含广告强相关词；`#US` 共 18372 条用户串，其中含「广告」7 条、「观看」4 条。
- 说明：`proto_ff.Adventure*`（冒险）、`Advanced`、`Adsorption`、`DownLoadManager` 这类**已按词元边界排除**，不再算广告。

## 7. 交付物

| 文件 | 绝对路径 | 大小 | 行数 |
|---|---|---|---|
| Assembly-CSharp.dll | `E:\nixiang\06_dumped\冒险与征途\aot\Assembly-CSharp.dll` | 22647808 | - |
| hotupdate_methods.txt | `E:\nixiang\06_dumped\冒险与征途\aot\hotupdate_methods.txt` | 11476456 | 187312 |
| hotupdate_hits.txt | `E:\nixiang\06_dumped\冒险与征途\aot\hotupdate_hits.txt` | 28779 | 351 |
| hotupdate_ad_hits.txt | `E:\nixiang\06_dumped\冒险与征途\aot\hotupdate_ad_hits.txt` | 265235 | 3804 |
| _aot_report.md | `E:\nixiang\06_dumped\冒险与征途\aot\_aot_report.md` | - | - |

## 8. 不确定点 / 待验证

- 只有**元数据**证据，没有 IL 反编译：`DS_Advertising::WatchAdvertisements` 内部到底走「直接发奖」还是「走 SDK 回调再 finish」的分支，
  需要反编译（ilspycmd/dnSpy 缺失，本轮不可用）或运行时 Hook（`frida` 在 `WatchAdvertisements`/`OnADVideoResult`/`DispatchResult`）才能确认。
- `MiaoYue`（妙月）是三方 SDK；实际是否可播广告取决于**发行渠道**是否接了该 SDK，DLL 里只能说「有这套接口与回调」。
- `TalkAd`(14 个方法：`OnAdAttack`/`OnAdDef`/`OnAdSupport`/`ShowTalkAdWord`) 名字带 Ad，但从方法看更像「战斗口播/喊话词条」，
  **待验证** 是否与激励视频同源（已保留在 `hotupdate_ad_hits.txt` A 段）。
- 类名哈希 `[d57917]`、`[-]` 是游戏富文本颜色标记，不是数据错误。
- `Character`/`StarsCollider`/`MW_GameNotice` 在目标表里以「方法名」给出，但实测是类名；若你要的是别的上下文（例如字符串或字段），需再定点。
- HybridCLR 热更只覆盖这 60 万行元数据对应的程序集；AOT 侧（libil2cpp.so）未纳入本轮范围。
