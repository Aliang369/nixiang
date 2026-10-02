"""Step 13: write _aot_report.md (结论报告)"""
# -*- coding: utf-8 -*-
import os, sys, io, re, hashlib
sys.path.insert(0, r'E:\nixiang\06_dumped\冒险与征途\aot')
from _mdread import Meta

AOT = r'E:\nixiang\06_dumped\冒险与征途\aot'
DLL = os.path.join(AOT, 'Assembly-CSharp.dll')
BUNDLE = os.path.join(AOT, 'hot1.unity3d')

def sha256(p):
    h = hashlib.sha256()
    with open(p, 'rb') as f:
        for c in iter(lambda: f.read(1 << 20), b''):
            h.update(c)
    return h.hexdigest()

def nlines(p):
    n = 0
    with io.open(p, encoding='utf-8', errors='replace') as f:
        for _ in f:
            n += 1
    return n

dll_sha = sha256(DLL); dll_sz = os.path.getsize(DLL)
bnd_sha = sha256(BUNDLE); bnd_sz = os.path.getsize(BUNDLE)

m = Meta(DLL)
n_td = len(m.tbl['TypeDef']); n_md = len(m.tbl['MethodDef'])
mlist = [r[5] for r in m.tbl['TypeDef']]
classes = []
for td in range(1, n_td + 1):
    classes.append((m.td_fullname(td), mlist[td - 1], mlist[td] if td < n_td else n_md + 1))

def sigline(full, rid):
    r = m.tbl['MethodDef'][rid - 1]
    nm = m.s(r[3])
    blob, _ = m.blob(r[4])
    try:
        ret, params, gen = m.sig_blob(blob)
    except Exception as e:
        ret, params, gen = ('<ERR %s>' % e, [], [])
    g = '<%s>' % ','.join(gen) if gen else ''
    return nm, '%s :: %s%s(%s) -> %s' % (full, nm, g, ', '.join(params), ret), 0x06000000 | rid

TARGETS = ['Damage', 'Character', 'DoAttack', 'StarsCollider', 'AddProjectile', 'ShowWin', 'MW_GameNotice']
meth_hits = {t: [] for t in TARGETS}
cls_hits = {t: [] for t in TARGETS}
for full, s0, e0 in classes:
    for t in TARGETS:
        if full == t or full.endswith('.' + t):
            cls_hits[t].append((full, e0 - s0))
    for rid in range(s0, e0):
        nm = m.s(m.tbl['MethodDef'][rid - 1][3])
        if nm in meth_hits:
            meth_hits[nm].append(sigline(full, rid))

files = ['hotupdate_methods.txt', 'hotupdate_hits.txt', 'hotupdate_ad_hits.txt']
sizes = {f: (os.path.getsize(os.path.join(AOT, f)), nlines(os.path.join(AOT, f))) for f in files}

L = []
w = L.append
w('# 冒险与征途 · hot1.unity3d → Assembly-CSharp.dll 元数据解包报告')
w('')
w('> 生成脚本：`_unityfs_lib.py` / `_step1.._step9*.py` / `_mdread.py` / `_step11_final.py` / `_step12_ad.py` / `_step13_report.py`（均在 `%s`）' % AOT)
w('> 纯 Python 标准库（zlib/lzma/struct/mmap 级），无外网、无 capstone/lief/pyelftools/lz4/UnityPy。')
w('')
w('## 1. 结论摘要')
w('')
w('1. **DLL 已成功取出**：`%s`（%d 字节，sha256=`%s`），PE/CLR/元数据全部自校验通过。' % (DLL, dll_sz, dll_sha))
w('2. **该 DLL 是完整可解析的 .NET 程序集**：TypeDef=%d，MethodDef=%d，程序集名 `Assembly-CSharp`，' % (n_td, n_md))
w('   元数据 v4.0.30319 / CLR runtime 2.5（HybridCLR 热更程序集，AOT 部分在 libil2cpp.so 里）。')
w('3. **7 个目标名全部定位**：`Damage`/`DoAttack`/`AddProjectile`/`ShowWin` 是方法名；')
w('   `Character`/`StarsCollider`/`MW_GameNotice` **在这份 DLL 里不是方法名而是类名**（同名方法 0 命中），已给出类与全部方法。')
w('4. **广告结论：游戏内确实有「看广告 / 看激励视频得奖励」的完整代码路径**（不是只有 SDK 占位）：')
w('   客户端 `DS_Advertising` → 三方 SDK `PlayADVideo/showRewardVideoAd` → 服务器 `start_watch_ad_req`/`finish_watch_ad_req` → 发奖 `DispatchResult`。')
w('')
w('## 2. UnityFS 头字段实测值（输入包）')
w('')
w('文件：`%s`（%d 字节，sha256=`%s`）' % (BUNDLE, bnd_sz, bnd_sha))
w('')
w('| 字段 | 实测值 |')
w('|---|---|')
w('| signature | `UnityFS` |')
w('| version | **8** |')
w('| unityVersion | `5.x.x` |')
w('| unityRevision | `2022.3.14f1` |')
w('| size | 9534622（= 文件大小 - 13） |')
w('| compressedBlocksInfoSize | 862 |')
w('| uncompressedBlocksInfoSize | 1811 |')
w('| flags | **0x243** |')
w('| 头起始偏移 | **0x0D**（文件头有 13 字节垃圾前缀 `55 6e 69 74 79 46 53 00 37 f3 57 12 9a`） |')
w('| blocksInfo 的 LZ4 流起点 | **0x4D**（0x3F..0x4C 是 16 字节对齐填充） |')
w('| 数据区起点 | **0x3AD**（= 0x4D + 862 + 2 字节尾填充；`len(file)-0x3AD == Σcsize == 9533694`） |')
w('| blocks / nodes | 173 块（flags&0x3F 全为 3）/ 1 个 node `CAB-fac7085faa161ea4294c5251c5071ee0` |')
w('')
w('**压缩方式**：flags 低 6 位 = 3。Unity 枚举里 **3 = LZ4HC（不是 LZMA）**，1=LZMA / 2=LZ4 / 3=LZ4HC；')
w('2 与 3 解压方式相同，都是 LZ4 block 格式。任务书里「3=LZMA」的假设不成立 —— 按 LZMA 解会失败，')
w('按自写 LZ4 block 解压器（token 高 4 位=literal、低 4 位=match-4、offset 2 字节小端）解则一次成功，')
w('blocksInfo 解出 1811 字节、173 个数据块全部与头里的 uncompressedSize 一致（173/173 零失配）。')
w('DLL 从**解压后数据的 0xA4C 偏移**开始（目录项在 0xA34 给出文件名 `Assembly-CSharp.dll`，0xA48 给出大小 22647808）。')
w('')
w('## 3. DLL 自校验与元数据布局')
w('')
seclines = ' / '.join('%s vsize=0x%X vaddr=0x%X rawsz=0x%X rawoff=0x%X' % s for s in m.sects)
w('- `MZ`@0x0、e_lfanew=0x80、`PE\\0\\0`@0x80、machine=**0x14C**（i386 / AnyCPU-32）、3 个 section：%s' % seclines)
w('- CLR 头：cb=0x48、runtime=2.5、MetaDataRVA=0x90CBA0、MetaDataSize=0xBB2A50')
w('- 元数据根 `BSJB` @DLL 偏移 **0x90ADA0**，version `v4.0.30319`，5 个流：')
w('  - `#~` off=0x90AE0C size=8003212')
w('  - `#Strings` off=0x10ACC98 size=2305232')
w('  - `#US` off=0x12DF968 size=655400（解析出 18372 条用户字符串，Blob 尾部奇数字节为 flag，**无前置 flag 字节**）')
w('  - `#GUID` off=0x137F990 size=16 / `#Blob` off=0x137F9A0 size=1302096')
w('')
w('## 4. 元数据统计')
w('')
w('- 程序集名 **Assembly-CSharp**（AssemblyRef 47 个）；**TypeDef=%d，MethodDef=%d**' % (n_td, n_md))
w('- Field=118,820 / Param=80,741 / Property=50,742 / MethodSemantics=68,211 / MemberRef=24,574')
w('- CustomAttribute=26,249 / TypeSpec=6,870 / NestedClass=5,483 / MethodSpec=6,154 / TypeRef=647 / GenericParam=353')
w('- 表区结束于 0x10ACC94（= `#~` 起点 0x90AE0C + 8003212 − 4 填充），与解析结果完全对齐 → 表解析可信')
w('')
w('## 5. 目标名字答案（8 个）')
w('')
w('| 目标名 | 性质 | 归属 |')
w('|---|---|---|')
for t in TARGETS:
    mh = meth_hits[t]; ch = cls_hits[t]
    kind = '方法' if mh else ('类' if ch else '未命中')
    where = '；'.join(sorted({h[1].split(' :: ')[0] for h in mh})) or '；'.join(c[0] for c in ch) or '-'
    w('| `%s` | %s | %s |' % (t, kind, where))
w('')
for t in TARGETS:
    w('### %s' % t)
    if meth_hits[t]:
        w('方法名精确命中 **%d** 处：' % len(meth_hits[t]))
        for full, line, tok in meth_hits[t][:30]:
            w('- `%s`  token=0x%08X' % (line, tok))
        if len(meth_hits[t]) > 30:
            w('- … 其余 %d 处见 `hotupdate_hits.txt`' % (len(meth_hits[t]) - 30))
    else:
        w('方法名精确命中 **0** 处 → 它在目标表里是**类名**。')
    if cls_hits[t]:
        w('类名命中：' + '；'.join('`%s`（%d 个方法）' % c for c in cls_hits[t]))
    if t == 'AddProjectile':
        w('')
        w('> 任务书说 `AddProjectile` 出现 2 次；实测是 **3 处、3 个不同类的 3 个不同签名**（无同签名重载）：')
        w('> ① `BaseUIMiniMap::AddProjectile(ActionGameLibrary.Event)` —— 小地图 UI 的事件图标添加，**与投射物无关**；')
        w('> ② `ActionGameLibrary.GameObjectManager::AddProjectile(ActionGameProjectile)` —— 战斗对象管理器登记投射物实例（**最像「投射物进入世界」的收口点**）；')
        w('> ③ `ActionGameLibrary.ActionCommand_AddProjectile::AddProjectile(Character, ..., ActionData.LAddProjectile, ...) -> ActionGameProjectile` —— 战斗指令「添加投射物」的执行体（输入发射者+行为数据，输出投射物）。')
        w('> 要改「射出子弹/箭矢」的逻辑，看 ②③（都在 `ActionGameLibrary` 战斗命名空间）；① 可忽略。')
    if t == 'ShowWin':
        w('')
        w('> 94 处全部是**各玩法结算/开奖 UI 的 ShowWin 弹窗**（如 `MW_LuckykoiReward::ShowWin(int)`、`MW_SingleArenaRank::ShowWin(int)`、')
        w('> `MW_GameNotice::ShowWin(string,string)`），**没有战斗内的胜负判定逻辑**；战斗胜负应找 `BattleManager`/`*Result` 类。')
    w('')
w('## 6. 广告 / 激励视频 结论')
w('')
w('**结论：有。** 这份热更程序集里存在完整的「看广告（激励视频）得奖励」链路，且有服务器协议与配置表配套，不是遗留占位代码。')
w('')
w('### 6.1 最像入口的 3 条路径')
w('')
w('1. **`DS_Advertising`（广告数据段，广告系统主控制器）**')
w('   - `DS_Advertising::WatchAdvertisements(int) -> void`（token `0x06001251`）—— 顶层入口')
w('   - `DS_Advertising::StartWatchAdReq(int)`（`0x06001252`）→ 发 `start_watch_ad_req`')
w('   - `DS_Advertising::OnADVideoResult(bool,int,ulong)` / `ReqADVideoResult` / `RspADVideoResult(Proto_CSRsp)` → `finish_watch_ad_req/rsp`')
w('   - `DS_Advertising::DispatchResult(int,bool)` —— 结果分发/发奖；`GetADTimes(int)->int`、`GetADSysParam(int)->string`、`GetADCdsParam(int)->int`、`get_FunctionEnable()->bool`')
w('2. **三方 SDK 激励视频（妙月 MiaoYue）**')
w('   - `BaseSDKAdapter::PlayADVideo(int,string)` / `MiaoYueSDKProxy::PlayADVideo(int,string)` / `MiaoYueSDKGameObject::PlayADVideo(Hashtable)`')
w('   - 回调 `MiaoYueADCallBack::onRewardArrived(bool,AndroidJavaObject)`；日志串 `MiaoYue SDK showRewardVideoAd`')
w('3. **悬浮广告吉祥物 AdMascotManager（游戏内可见的「点广告领奖」UI）**')
w('   - `AdMascotManager::OnClickWatchAd()`、`CanClaimReward()->bool`、`GetNextRewardTime()->uint`、`SetRewardClaimed(bool)`')
w('   - `GetAdRewardInfo(ref int,ref int,ref bool)`、`RefreshAdRewardData()`、`OnFloatingAdRewardDataChanged(GEvent)`')
w('   - 数据来自 `proto_ff.Proto_SCFloatingAdRewardDataNtf`；`DS_MainCity::FloatingAdRewardDataNtf(proto_ff.Proto_CSRsp)`')
w('')
w('### 6.2 判定依据（证据链）')
w('')
w('- **协议**：`proto_ff.Proto_CSStartWatchAdReq/Rsp`、`proto_ff.Proto_CSFinishWatchAdReq/Rsp`、`proto_ff.Proto_SCFloatingAdRewardDataNtf`；')
w('  角色数据 `proto_ff.AccAdvertiseData{ ad_id, ad_uuid, ad_start_time }` 挂在 `proto_ff.RoleData::advertise_data` 上。')
w('- **配置表**：`GameConfigData.ConfigData::get_CFG_AdvertisingRewards()`、`get_CFG_FloatingAdReward()`、`get_CFG_WatchAdvertisement()`；')
w('  类型 `GameConfigData.AdvertisingRewards` / `FloatingAdReward` / `FloatingAdConst` / `WatchAdvertisement`。')
w('- **枚举/字段**：`EN_REWARD_NOTIFY_TYPE_WATCH_AD_REWARD`、`ERR_CODE_FLOATING_AD_REWARD_NOT_IN_TIME`、')
w('  `EN_ICR_FLOATING_AD_REWARD`、`E_CS_FLOATING_AD_REWARD_DATA_NTF`、`EVENT_WATCHAD_RESULT`(串 `Event_WatchADResult`)、')
w('  各玩法「看广告位」开关字段：`AFKBlessAdShow`/`AFKDailyBossAdShow`/`ArenaAdShow`/`TormentTowerAdReward`/`MallPropItemAdShow`/`NewGuildDailyLivenessAdReward` 等。')
w('- **UI 入口**：`UI_PanelDailyBoss::OnClickBtnAdvertise()`、`DS_MallData::IsShowAdFree(int)->bool`、`MW_MallQuiteBuy::RefreshADShow(GEvent)`、`MW_TowerJingMo::SetWatchAdData()`、`MW_AFKBlessLevelAD`（挂机祝福等级广告）。')
w('- **#US 用户字符串（UTF-16 原文，决定性）**：')
for q, s in [(0x12EC58F, '是否确认进入广告系统 观看广告获取奖励\\n开通月卡可无限制的[d57917]免看广告[-]直接领取'),
             (0x12EC5F1, '进入广告'),
             (0x12EC683, '已使用免广告特权跳过观看'),
             (0x1311241, '观看视频后，缩短{0}分钟升级时间'),
             (0x1309647, '广告次数已用完'),
             (0x131A1F4, '飘屏广告已观看'),
             (0x1327AA6, '开通月卡后，每日广告收益次数+{0}\\n是否前往开通？'),
             (0x1302356, 'MiaoYue SDK showRewardVideoAd')]:
    w('  - `0x%X`  %s' % (q, s))
w('- **#US 字段名**：`start_watch_ad_req/rsp`、`finish_watch_ad_req/rsp`、`watch_ad_times`、`last_watch_ad_time`、')
w('  `active_double_reward_req/rsp`、`use_reward_double`、`is_double`、`isRewardValid`、`FloatingAdRewardData_Change`。')
w('  （`active_double_reward_req` + `use_reward_double` 即「看广告双倍奖励」这一类的服务端请求/消耗标记。）')
w('- **免广告能力**：`已使用免广告特权跳过观看` + `开通月卡可无限制免看广告` → 月卡用户走跳过分支（同一套 DispatchResult 里的分支）。')
w('')
w('### 6.3 广告相关统计')
w('')
w('- 强相关(词元级命中广告词) **类型 20 个 / 方法 44 个**；中相关(Reward/SDK/Video 词元级) **类型 681 / 方法 2179**；弱相关 14/796。')
w('- 属性 16 个、字段 46 个含广告强相关词；`#US` 共 18372 条用户串，其中含「广告」7 条、「观看」4 条。')
w('- 说明：`proto_ff.Adventure*`（冒险）、`Advanced`、`Adsorption`、`DownLoadManager` 这类**已按词元边界排除**，不再算广告。')
w('')
w('## 7. 交付物')
w('')
w('| 文件 | 绝对路径 | 大小 | 行数 |')
w('|---|---|---|---|')
for f in ['Assembly-CSharp.dll'] + files:
    p = os.path.join(AOT, f)
    if f.endswith('.dll'):
        w('| %s | `%s` | %d | - |' % (f, p, os.path.getsize(p)))
    else:
        sz, ln = sizes[f]
        w('| %s | `%s` | %d | %d |' % (f, p, sz, ln))
w('| _aot_report.md | `%s\\_aot_report.md` | - | - |' % AOT)
w('')
w('## 8. 不确定点 / 待验证')
w('')
w('- 只有**元数据**证据，没有 IL 反编译：`DS_Advertising::WatchAdvertisements` 内部到底走「直接发奖」还是「走 SDK 回调再 finish」的分支，')
w('  需要反编译（ilspycmd/dnSpy 缺失，本轮不可用）或运行时 Hook（`frida` 在 `WatchAdvertisements`/`OnADVideoResult`/`DispatchResult`）才能确认。')
w('- `MiaoYue`（妙月）是三方 SDK；实际是否可播广告取决于**发行渠道**是否接了该 SDK，DLL 里只能说「有这套接口与回调」。')
w('- `TalkAd`(14 个方法：`OnAdAttack`/`OnAdDef`/`OnAdSupport`/`ShowTalkAdWord`) 名字带 Ad，但从方法看更像「战斗口播/喊话词条」，')
w('  **待验证** 是否与激励视频同源（已保留在 `hotupdate_ad_hits.txt` A 段）。')
w('- 类名哈希 `[d57917]`、`[-]` 是游戏富文本颜色标记，不是数据错误。')
w('- `Character`/`StarsCollider`/`MW_GameNotice` 在目标表里以「方法名」给出，但实测是类名；若你要的是别的上下文（例如字符串或字段），需再定点。')
w('- HybridCLR 热更只覆盖这 60 万行元数据对应的程序集；AOT 侧（libil2cpp.so）未纳入本轮范围。')
w('')
open(os.path.join(AOT, '_aot_report.md'), 'w', encoding='utf-8').write('\n'.join(L))
print('report lines:', len(L))
print('dll sha256:', dll_sha, dll_sz)
for f in files:
    print(f, sizes[f])
