/* ================================================================
 *  霓虹深渊 3.2.860 — 正式版 8 项（2026-11-26 第八轮收敛）
 *  无敌 / 秒杀 / 技能无CD / 移速 / 金币使用不减 / 钥匙使用不减 /
 *  彩色钥匙使用不减 / 无限跳跃
 *  【本轮删除】攻速、伤害倍率、丝血无敌、全屏攻击 —— 菜单下线，
 *  InstallHooks 不再安装对应钩子、gate/tick 不再有通道（实现代码保留但不可达）。
 *
 *  —— 以下为 2026-10-02 起的历史口径（保留供 RVA 追溯，功能已下线）——
 *  无敌 / 秒杀 / 技能无CD / 移速 / 攻速（统一一刀切：枪械 + 多段近战）
 *  RVA 一律 dump.cs；禁止 Il2Cpp::Api::Load；禁止非主线程调 timeScale
 *
 *  2026-10-02 迁移 modern 注册表：库名改走 sec_lib_il2cpp()（不再依赖
 *  GameConfig.h，那是 legacy 配置中心）。地址解析机制不变（file_off）。
 *
 *  【攻速】单一滑条（2026-10-02 第四轮，按用户要求把原「近战攻速/远程攻速」合并）：
 *    g_atkLvl(1..5) → AtkMul()=LvlMul(档位)；枪械与多段近战统一按同一倍率加速。
 *    原因：分开做要先判定「手上这把武器是近战还是远程」，而真机上该判定不可靠
 *    （实测近战滑条会把玩家的枪一起加速），合并后既无串扰又省一套分流逻辑。
 *
 *    (1) 枪械/枪图武器：GunGraph.<AttackRoutine>d__45/d__46::MoveNext
 *        0x292AE4C（无事件版，玩家枪械实际走的这条）/ 0x292B190（带事件版）
 *        节奏＝每帧 timer -= deltaTime，归零后 timer = GunOutput::get_FireDelay
 *        → 缩放「新赋值的 timer」（帧内判突然变大），只影响玩家路径
 *          （d__46 由 [sm+0x50]＝<neonPlayerState>5__2 非空门控；d__45 走
 *           GunGraphBase.netPlayer(0x28) 非空且非 isActorShadow(0x68)）。
 *    (2) 多段近战（梅那把刀＝FireWeaponSlot_MeleeMultiSegGun，: DeputyWeaponSlot_MeleeMultiSeg）
 *        真节奏＝ _busyTime(0x98) 这个「剩余忙碌倒计时」（满值实测 0.75s）：
 *          OnUpdate(0x3DAB76C)：if(_busyTimePaused) return;
 *                               b=get_BusyTime(); if(b>0){ set_BusyTime(b-deltaTime);
 *                                 if(get_BusyTime()<0) OnBusyTimePassed(); }
 *          ⇒ 每帧写回「剩余秒数」，倒计时归零后由 OnBusyTimePassed 写入满值。
 *        → 只缩放「上升跳变」（＝新一轮窗口起点）÷mul，逐帧递减原样透传；
 *          AsBusyTime(0x3DABB30) 的 get+add+set 累加也走同一函数，天然覆盖。
 *    (3) 真身＝ get_FireDelay 0x3A8F3F4（= 60/RPM，枪械）+ set_BusyTime 0x3DA9D70（多段近战）。
 *        旧点仍保留为后备：0x292AB08 shootingInterval d__96、0x2929E78 MeleeCheck d__85
 *        （实测两者在真机武器上零命中，玩家武器由枪图协程/副武器槽驱动）。
 *
 *  对局内资源（2026-10-02 第二轮，dump.cs + 反汇编双向核对）：
 *    ⑦ 金币使用不减   NEONPlayerState.UseCoin(0x289295C) / HasCoin(0x28928EC)
 *                 / GetCoinNum(0x28952AC)；数值字段 Coin(PlayerStateInt 0x30)
 *    ⑧ 钥匙使用不减   NEONPlayerState.UseKey(0x2894094) / HasKey()(0x28921C4)
 *                 / HasKey(int)(0x2893FB0)；数值字段 Key(PlayerStateInt 0x380)
 *    ⑨ 彩色钥匙使用不减 NEONPlayerState.UseGameplayKey(0x2894EC8)
 *                 / HasGameplayKey(0x28951C4)；GameplayKey 不是 PlayerStateInt，
 *                 而是属性(attrid)：HasGameplayKey 走
 *                 Global::get_PlayerState→[+0x48]→EhiEntity::GetAttri；
 *                 拾取物 SpecialKeyPowerUp::PickItUp(0x33E49B8) 唯一调用
 *                 NEONPlayerState::AddGameplayKey ⇒ 彩色钥匙 = GameplayKey
 *    ⇒ 三项都只做「消耗变空操作 + 够不够恒真」，不写数值字段（零残留）
 *
 *  技能无CD（2026-10-02 实测失效后加强）：
 *    CanUseSkill 在 dump 里共 35 个实现（全部派生自 SecSkillPowerUp），
 *    旧版只挂了基类那一处 → 各角色改写的 override 直接 return false 时
 *    绕不过去。现在一次性全挂，并同时把该实例的
 *    CoolingTime(0x25C)/LastCoolingTime(0x260) 清零后强制返回 true。
 * ================================================================ */

#include "NeonMod.h"

#include <atomic>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <unwind.h>
#include <utility>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"
#include "Includes/Utils.hpp"
#include "xdl.h"

// 目标库名（注册表模式不依赖 GameConfig.h）
#define NEON_LIB sec_lib_il2cpp()

enum : uintptr_t {
    RVA_HITPOINTS_DEDUCT = 0x346D8C4,
    RVA_HITPOINTS_FIX    = 0x346B738,
    RVA_HITPOINTS_DO     = 0x346DE04,
    RVA_HEARTS_DEDUCT    = 0x3545ED0,
    RVA_BUILD_PASS       = 0x3676B3C,
    RVA_BUILD_PASS_BASE  = 0x3676C64,

    RVA_IS_SKILL_CD      = 0x35C01A8,
    RVA_SET_SKILL_CD     = 0x35BFF90,
    RVA_GET_REMAIN_CD    = 0x35C0270,
    RVA_SEC_COOL_TIME    = 0x2FBCA54,
    RVA_SEC_CUR_COOL     = 0x2FBCAAC,
    RVA_SEC_SKILL_BEGIN  = 0x2FB6654,  // SecSkillPowerUp::SkillBegin(bool)（探针）
    RVA_SEC_SKILL_END    = 0x2FB70A4,  // SecSkillPowerUp::SkillEnd()（探针）

    RVA_DICT_MOVE_SPEED  = 0x5D7511C,
    RVA_PLAYER_SPEED_MF  = 0x2669740,
    RVA_COMP_MOVE_SPEED  = 0x2645A20,
    RVA_GET_MAX_SPEED    = 0x264590C,

    // 武器攻速（2026-10-02 dump.cs + 反汇编核对）
    RVA_SHOOT_INTERVAL_MN = 0x292AB08,  // Gun.<shootingInterval>d__96::MoveNext（后备）
    RVA_MELEE_CHECK_MN    = 0x2929E78,  // Gun.<MeleeCheck>d__85::MoveNext
    RVA_GRAPH_ATTACK_MN   = 0x292B190,  // GunGraph.<AttackRoutineWithEvents>d__46::MoveNext（近战在此）
    // 2026-10-02 实测定位：玩家枪械走的是「无事件」版 <AttackRoutine>d__45（d__46 零命中）。
    // 依据：GunOutput::NeedCharge/Attack/MuzzleFlash::Fire 的调用点全在 d__45/d__46/d__47；
    //       而 GunGraph.meleeDelegate::Invoke 全场只有 1 个调用点 0x292B3C8＝d__46 内部
    //       ⇒ 近战必走 d__46，d__45 只可能是枪械（远程）。
    RVA_GRAPH45_ATTACK_MN = 0x292AE4C,  // GunGraph.<AttackRoutine>d__45::MoveNext

    // 对局内资源（NEONPlayerState，2026-10-02 dump.cs 行号→RVA 核对）
    RVA_PS_USE_COIN       = 0x289295C,  // void UseCoin(int amount)
    RVA_PS_HAS_COIN       = 0x28928EC,  // bool HasCoin(int amount)
    RVA_PS_GET_COIN       = 0x28952AC,  // int  GetCoinNum()
    RVA_PS_USE_KEY        = 0x2894094,  // void UseKey()
    RVA_PS_HAS_KEY        = 0x28921C4,  // bool HasKey()
    RVA_PS_HAS_KEY_N      = 0x2893FB0,  // bool HasKey(int needCount)
    RVA_PS_USE_GKEY       = 0x2894EC8,  // void UseGameplayKey()
    RVA_PS_HAS_GKEY       = 0x28951C4,  // bool HasGameplayKey()

    // ＝＝ 2026-10-02 第六轮新增：⑨无限跳跃 / ⑩伤害倍率 / ⑪丝血无敌 ＝＝
    // ⑨ 无限跳跃：MultipleJump 里的门槛是 `if (get_multipleJumpCount() >= multipleJumpCountLimit) return;`
    //     （0x264FD84 bl getter → 0x264FDA4 cmp/b.ge），跳完再 `set_multipleJumpCount(count + 2)`。
    //     把 getter 恒返回负数即恒过门槛，且 +2 后仍为负（另一读点在 PlayerActor::Jump 0x264F98C）。
    RVA_MULTI_JUMP_COUNT  = 0x26472BC,  // PlayerActor::get_multipleJumpCount() -> int
    // ⑪ 丝血无敌：Hearts 的心数是 int（set_currentHearts 走 w1），且 get_currentHearts 自身
    //     不使用第 2 个参数（反汇编里 x1 未被读）⇒ 以 (thiz, nullptr) 调用安全。
    RVA_HEARTS_GET_CURRENT = 0x3541E54, // Hearts::get_currentHearts() -> int
    RVA_HEARTS_SET_CURRENT = 0x3542000, // Hearts::set_currentHearts(int)

    // 第六轮实测补充（2026-10-02 晚）：⑨ 的三个附加闸门 + ⑪ 的四个扣血/死亡落点
    RVA_OP_IMPLICIT      = 0x30DABD0, // PlayerStateInt::op_Implicit(PlayerStateInt) -> int（跳跃次数上限）
    RVA_CAN_NOT_JUMP     = 0x3345630, // PlayerActorBase::get_canNotJump() -> bool
    RVA_CAN_MULTI_JUMP   = 0x3CB2AA4, // BaseMechaControl::CanMultipleJump() -> bool
    RVA_HEARTS_GET_ARMOR = 0x3545344, // Hearts::get_currentArmor() -> int
    RVA_HEARTS_GET_SANE  = 0x35440C0, // Hearts::get_Sane() -> int
    RVA_HEART_FIRST      = 0x3547218, // Hearts::DeductHeartFirst(DamageInfo*)（心数真正扣减点 set_currentHearts(cur-w22)）
    RVA_ARMOR_FIRST      = 0x35475EC, // Hearts::DeductArmorFirst(DamageInfo*)
    RVA_HEART_APPORTION  = 0x3547824, // Hearts::DeductHeart_ApplyApportion(DamageInfo*)
    RVA_FORCE_DIE        = 0x3547AFC, // Hearts::ForceDie()（死亡入口，不死兜底）

    // ＝＝ 2026-11-26 第七轮新增：⑫全屏攻击（dump.cs 逐条核对）＝＝
    // ⑫ 全屏攻击：挂点就是上面的 RVA_HITPOINTS_DEDUCT（0x346D8C4 = HitPoints.DeductHP，
    //    即任务书里的 RVA_DEDUCTHP_HP）。已有钩子，不新建第二个。
    RVA_ROOM_GET           = 0x3369A64, // NPActor.get_actorCurRoomMB() -> RoomMB*（实测恒返回 0：父链上没有 RoomMB）
    // 2026-11-26 第二版：RoomMB 改由「进/离房间回调」捕获。NPActor.get_actorCurRoomMB 是
    // GetComponentInParent<RoomMB>()，真机实测返回 0（怪物不在 RoomMB 的父子链上）⇒ 用不了。
    // YiHai.OnEnterRoom(RoomMB) / OnLeaveRoom(RoomMB) 是静态回调，x0 就是当前房间。
    RVA_ROOM_ENTER         = 0x26BC52C, // YiHai.OnEnterRoom(RoomMB rmb)（真机未触发：很少走这条路）
    RVA_ROOM_LEAVE         = 0x26BD268, // YiHai.OnLeaveRoom(RoomMB rmb)
    RVA_ROOM_BATTLE        = 0x26BD324, // YiHai.OnEnterBattle(RoomMB rmb)
    RVA_ROOM_CLEANUP       = 0x32F2A34, // RoomMB.MonsterListCleanup()（this=RoomMB，怪死亡时调）
    RVA_ROOM_ACTIVE_CNT    = 0x32F2E90, // RoomMB.get_ActiveMonsterCount() -> int（this=RoomMB，高频）
    // ★2026-11-26 定案路径：MapManager（MonoBehaviour）每帧 LateUpdate，它自己持有 curRoom。
    //   dump.cs 1400445 字段表：public static MapManager Instance; // 0x0
    //                           private RoomMB curRoom;          // 0x20   ← 就在这里
    //   ⇒ hook LateUpdate，this 就是 MapManager，读 [this+0x20] 必得当前房间（不在房间时为 null）。
    RVA_MAP_LATE_UPDATE    = 0x2CBE5D0, // MapManager.LateUpdate()
    RVA_MAP_REFRESH_ROOM   = 0x2CBE7CC, // MapManager.RefreshRoom(RoomMB)（备用：参数即房间）
    // EhiEntity.getCurrentRoomMb()：拿任意 EhiEntity（MonsterActor 就是）调一下，内部会
    // GetComponentsInParent 找 RoomMB 并回写 [this+0x48] 缓存（反汇编 0x3E77648–0x3E776F8）。
    RVA_EHI_GET_ROOM       = 0x3E77580,
};

// 技能可用性门：dump.cs 里 CanUseSkill 共 35 个实现，全部派生自 SecSkillPowerUp
// （2026-10-02 由 dump.cs 生成；只挂基类会被各角色 override 的 return false 绕过）
struct NeonSkillGate { uintptr_t rva; const char* name; };
static const NeonSkillGate kSkillGates[] = {
    { 0x2CBC0CC, "ManaSecSkillPowerUp" },
    { 0x2CE6BA4, "Nezha_Active_001" },
    { 0x2E91ACC, "Buro_Active_001" },
    { 0x2ED95D8, "MinionRevivePowerUp" },
    { 0x2FB6418, "SecSkillBlink" },
    { 0x2FB6498, "SecSkillPowerUp" },
    { 0x2FB68C0, "SecSkillBuildLauncherPlatform" },
    { 0x2FB6B04, "SecSkillBuyEgg" },
    { 0x2FB77B0, "SecSkillDropKey" },
    { 0x2FB7C40, "SecSkillFishing" },
    { 0x2FB8264, "SecSkillGamble" },
    { 0x2FB8D4C, "SecSkillGoldenEgg" },
    { 0x2FB978C, "SecSkillGunReroll" },
    { 0x2FB9AB8, "SecSkillHeartToCoin" },
    { 0x2FBA940, "SecSkillHoldObject" },
    { 0x2FBB820, "SecSkillPlaceMinePowerUp" },
    { 0x2FBD450, "SecSkillPowerupReloot" },
    { 0x2FBDB3C, "SecSkillProtectiveShellEnergy" },
    { 0x2FBE78C, "SecSkillRoll" },
    { 0x2FBEBA4, "SecSkillSelfExplode" },
    { 0x2FC094C, "SecSkillSmashdown" },
    { 0x2FC2BB0, "SecSkillTeleportToTarget" },
    { 0x3137450, "Lin_Active_001" },
    { 0x31652C4, "LuKaSi_Active_001" },
    { 0x32CA110, "IncreaseGunGraphStatPower" },
    { 0x34D3E58, "SecSkillAUnlock" },
    { 0x34D46F8, "SecSkillBackOff" },
    { 0x34D4820, "SecSkillBalloon" },
    { 0x34D4C1C, "SecSkillBlankBomb" },
    { 0x353EBC0, "Hacker_Active_001" },
    { 0x35BA378, "SiShe_Active_001" },
    { 0x3606CBC, "Shell_Active_001" },
    { 0x3C4BE60, "Neon_Active_001" },
    { 0x3D55FC8, "Icarus_Active_001" },
    { 0x3DCEAD4, "ZhaKe_SP_Active_001" },
};
enum : size_t { NEON_SKILL_GATE_COUNT = sizeof(kSkillGates) / sizeof(kSkillGates[0]) };

enum : size_t {
    OFF_HP_CUR        = 0x13C,
    OFF_HEARTS_INV    = 0x148,
    OFF_HEARTS_UNKILL = 0x160,
    OFF_DMG_DAMAGE    = 0x10,
    OFF_DMG_FINAL     = 0x18,
    OFF_DMG_BASE      = 0x1C,

    // 攻速协程状态机 / 玩家武器过滤
    OFF_SM_GUN        = 0x20,  // <>4__this（Gun*）
    OFF_SM_CURRENT    = 0x18,  // <>2__current（yield 对象）
    OFF_SM_TIMER      = 0x28,  // <MeleeCheck>d__85 的 <timer>5__2（挥砍冷却）
    OFF_WFS_SECONDS   = 0x10,  // UnityEngine.WaitForSeconds.m_Seconds
    OFF_INSTIGATOR_IS_PLAYER = 0xC0,  // GunBase.InstigatorIsPlayer

    // GunGraph.<AttackRoutineWithEvents>d__46 状态机布局
    OFF_SM_NEON_PS     = 0x50,  // private NEONPlayerState <neonPlayerState>5__2
    OFF_SM_GRAPH_TIMER = 0x58,  // private float <timer>5__3（射击间隔倒计时）
    OFF_SM_MELEE_EVENT = 0x38,  // GunGraphEvents.meleeEvent（0x38 处 struct 的首字段；非空=近战图）

    // GunGraph.<AttackRoutine>d__45 状态机布局（无 timer / 无 GGEvents / 无 ps 缓存）
    OFF_SM45_THIS      = 0x30,  // public GunGraph <>4__this
    OFF_SM45_CURRENT   = 0x18,  // <>2__current（此处唯一 yield = WaitForSeconds(FireDelay)）
    OFF_SM45_INSTIG    = 0x28,  // public MonoBehaviour Instigator

    // GunGraphBase / GunGraph 上的「是不是玩家这把枪」
    OFF_GRAPH_NETPLAYER = 0x28,  // GunGraphBase.netPlayer（NetPlayerActor；非空＝玩家自己的枪）
    OFF_GRAPH_SHADOW    = 0x68,  // GunGraph.isActorShadow（技能复制出来的影子枪，不算）
    OFF_GRAPH_OUTPUT    = 0x80,  // GunGraph.output
    OFF_GRAPH_FIRING    = 0x90,
    OFF_GRAPH_LATEST_INST = 0xA0, // GunGraph.LatestInstigator（dump.cs 1484445）  // GunGraph.firing

    // SecSkillPowerUp 冷却字段（35 个门共用同一基类布局）
    OFF_SEC_COOLING_TIME = 0x25C,
    OFF_SEC_LAST_COOL    = 0x260,

    // ＝＝ 2026-11-26 第七轮：⑫全屏攻击（dump.cs 核对）＝＝
    OFF_DMG_OP_ID        = 0x04,   // DamageInfo.damageOpId（同一次攻击的多段结算共用）
    OFF_DMG_INSTIGATOR   = 0x70,   // DamageInfo._instigater（Component）
    OFF_DMG_REAL_INST    = 0x90,   // DamageInfo.RealInstigator
    OFF_DMG_PLAYER_HIT   = 0x78,   // DamageInfo.instigaterIsPlayer（死字段：全 so 无写入，仅诊断）
    OFF_DMG_TARGET_FLT   = 0x98,   // DamageInfo.targetFilter（Factions flags：Player=1/Monster=2/Environment=4/All=7）
    OFF_DMG_SRC_TYPE     = 0x64,   // DamageInfo.SourceType（DamageSourceType，诊断用）
    OFF_KILLABLE_FACTION = 0x68,   // Killable.faction（2＝怪物）
    OFF_HP_MON_ACTOR     = 0x1B0,  // HitPoints._monsterActor（受害者 MonsterActor）
    OFF_KILLABLE_MON_OWN = 0xF0,   // Killable._monsterActorOwner（备用取法）
    OFF_MON_HP           = 0x180,  // MonsterActor.hp（HitPoints）
    OFF_MAP_CURROOM      = 0x20,   // MapManager.curRoom（dump.cs 1400445 字段表）
    OFF_EHI_CURROOM      = 0x48,   // EhiEntity._currentRoomMb（反汇编 0x3E77580 内 ldr [this+0x48]）
    OFF_ROOM_ENTERED     = 0x109,  // RoomMB.roomIsEnteren（反汇编 0x32F2788: ldrb w0,[x19,#0x109]）
    OFF_ROOM_BUFFMONS    = 0x80,   // RoomMB._buffSpawnMons（反汇编 0x32F2BD8: ldr x20,[x19,#0x80]）
    OFF_ARRAY_LEN        = 0x18,   // T[].max_length（反汇编 List.Add: ldr w10,[x9,#0x18]）
    // Il2CppClass 布局（反汇编 0x2C6A054–0x2C6A084 与 0x31B7EB8 两处一致）
    OFF_KLASS_DEPTH      = 0x12C,  // uint8 typeHierarchyDepth
    OFF_KLASS_TYPEH      = 0xC8,   // Il2CppClass** typeHierarchy
    // 类型槽（so 内 .got/.data.rel.ro 里的 Il2CppClass* 引用，运行时 = *(void**)(base + RVA)）
    RVA_SLOT_PLAYER_KLASS    = 0x9E99068,  // PlayerActor
    RVA_SLOT_NETPLAYER_KLASS = 0x9EBC1F8,  // NetPlayerActor
    OFF_ROOM_MONS        = 0x78,   // RoomMB.mons（List<MonsterActor>）
    OFF_LIST_ITEMS       = 0x10,   // List<T>._items
    OFF_LIST_SIZE        = 0x18,   // List<T>._size
    OFF_ARRAY_FIRST      = 0x20,   // T[] 元素数组起点
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_invincible{false};
static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_noCd{false};
static std::atomic<int>  g_moveLvl{1};
// ---------------------------------------------------------------------------
// 临时诊断开关（NEON_DIAG）：排查真机攻击走哪条路时置 1；正式包必须为 0。
//  2026-10-02 第五轮收尾：真身已定位且实测通过（枪 + 多段近战同一把滑条生效）→ 置 0。
//  历史：第三/四轮置 1 抓到 d__45/d__46 分流、GunOutput::get_FireDelay、
//  DeputyWeaponSlot_MeleeMultiSeg._busyTime(0x98) 三条关键证据。
// ---------------------------------------------------------------------------
#define NEON_DIAG 0

// 攻速档位 1..5（1 = 关）：2026-10-02 第四轮按用户要求，把原「近战攻速/远程攻速」
// 两把滑条合并为一把「攻速」，所有玩家攻击节奏统一按 LvlMul 缩放（不再分流判断）。
static std::atomic<int>  g_atkLvl{1};
static std::atomic<bool> g_infCoin{false};  // 金币使用不减
static std::atomic<bool> g_infKey{false};   // 钥匙使用不减
static std::atomic<bool> g_infGkey{false};  // 彩色钥匙使用不减（GameplayKey）

// ＝＝ 2026-10-02 第六轮新增：⑨无限跳跃 / ⑩伤害倍率 / ⑪丝血无敌 ＝＝
static std::atomic<bool> g_noJumpLimit{false};  // ⑨ 无限跳跃（get_multipleJumpCount 恒返回负数）
static std::atomic<int>  g_dmgMul{1};           // ⑩ 伤害倍率 1..50（1 = 关）
static std::atomic<bool> g_lastHeart{false};    // ⑪ 丝血无敌（致命一击不结算，锁血到 1）

// ＝＝ 2026-11-26 第七轮新增：⑫全屏攻击 ＝＝
static std::atomic<bool> g_fullAtk{false};        // ⑫ 全屏攻击（一次玩家伤害镜像到同房间所有怪物）
// ⑫ 玩家攻击时间窗：d__45（玩家枪图攻击协程）每次迭代刷新。真机上 DamageInfo.instigaterIsPlayer
// （+0x78）恒 0、targetFilter（+0x98）也不专属于玩家（实测 filter=6=Monster|Environment 是怪物互伤），
// 所以「这次伤害是不是玩家打出来的」改用时间窗判定：玩家攻击后 800ms 内的怪物受伤视为玩家的。
static std::atomic<uint64_t> g_lastPlayerAtkMs{0};
// ⑫ 当前房间缓存：由 YiHai.OnEnterRoom 写入、OnLeaveRoom 清空。
// 全屏攻击要枚举「同房间所有怪」，RoomMB.mons(List<MonsterActor>) 在 RoomMB+0x78。
static std::atomic<void*> g_curRoom{nullptr};

// libil2cpp 映射范围：base 由已解析的钩子地址反推，end 由 /proc/self/maps 周期重扫得到
static uintptr_t g_libBase = 0;
static uintptr_t g_libEnd  = 0;

// ⑫ 玩家伤害归属（2026-11-26 定案，全静态可证）：
//   真机实测 instigaterIsPlayer(+0x78) 恒 0 —— 全 so 里 STRB [.,#0x78] 命中 0 次，是死字段；
//   targetFilter(+0x98) 是「受击侧过滤」（能命中哪些阵营），与施放者无关，怪物互伤同样含 Monster 位。
//   游戏自己判玩家的唯一写法 = ToolCommon.IsMatchInstigaterType(Player, di.instigator)
//     = il2cpp 的 instigator is PlayerActor（沿 typeHierarchy 判，非裸 klass 比较）。
//   ⇒ 照它的语义自实现 NeonInstOf，并额外缓存「玩家枪当前 instigator」覆盖枪/子弹路径。
static void* g_playerKlass    = nullptr;   // PlayerActor
static void* g_netPlayerKlass = nullptr;   // NetPlayerActor

static uint64_t NeonNowMs();                  // 前置声明（实现位于下方工具区）

struct NeonRecentInst { void* ptr; uint64_t ms; };
static NeonRecentInst g_recentInst[8] = {};
static std::atomic<unsigned> g_recentInstIdx{0};

static bool NeonInstOf(void* obj, void* klass) {
    if (!obj || !klass) return false;
    void* k = *reinterpret_cast<void**>(obj);
    if (!k) return false;
    const uint8_t dk = *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(k) + OFF_KLASS_DEPTH);
    const uint8_t dt = *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(klass) + OFF_KLASS_DEPTH);
    if (dt == 0 || dk < dt) return false;
    void** th = *reinterpret_cast<void***>(reinterpret_cast<uintptr_t>(k) + OFF_KLASS_TYPEH);
    return th && th[dt - 1] == klass;
}

static bool NeonIsPlayerInstigator(void* inst) {
    if (!inst) return false;
    if (NeonInstOf(inst, g_playerKlass)) return true;
    if (NeonInstOf(inst, g_netPlayerKlass)) return true;
    // 类名兜底：沿 typeHierarchy 逐级比类名（类型槽地址一旦读错也不会失灵）
    void* k = *reinterpret_cast<void**>(inst);
    if (k) {
        const uintptr_t kp = reinterpret_cast<uintptr_t>(k);
        const uint8_t depth = *reinterpret_cast<uint8_t*>(kp + OFF_KLASS_DEPTH);
        void** th = *reinterpret_cast<void***>(kp + OFF_KLASS_TYPEH);
        if (th) {
            for (uint8_t i = 0; i < depth && i < 16; ++i) {
                void* c = th[i];
                if (!c) continue;
                const char* nm = *reinterpret_cast<const char**>(reinterpret_cast<uintptr_t>(c) + 0x10);
                if (!nm) continue;
                if (strcmp(nm, "PlayerActor") == 0 || strcmp(nm, "NetPlayerActor") == 0)
                    return true;
            }
        }
    }
    return false;
}

static void NeonRememberPlayerInstigator(void* inst) {
    if (!inst) return;
    const unsigned i = g_recentInstIdx.fetch_add(1) % 8u;
    g_recentInst[i].ptr = inst;
    g_recentInst[i].ms  = NeonNowMs();
}

static bool NeonIsRecentPlayerInstigator(void* inst) {
    if (!inst) return false;
    const uint64_t now = NeonNowMs();
    for (int i = 0; i < 8; ++i) {
        if (g_recentInst[i].ptr == inst && now >= g_recentInst[i].ms && (now - g_recentInst[i].ms) <= 3000ull)
            return true;
    }
    return false;
}

typedef bool (*DeductHP_t)(void* thiz, void* damage, int32_t count, void* method);
typedef void (*DoDeduct_t)(void* thiz, void* damage, void* method);
typedef void* (*BuildPass_t)(void* thiz, void* method);
typedef bool (*IsSkillCD_t)(void* thiz, void* skillKey, void* method);
typedef void (*SetSkillCD_t)(void* thiz, void* skillKey, float countTime, void* method);
typedef float (*GetRemainCD_t)(void* thiz, void* skillKey, void* method);
typedef bool (*CanUseSkill_t)(void* thiz, void* method);
typedef void (*SkillBegin_t)(void* thiz, bool isSecSkill, void* method);
typedef void (*SkillEnd_t)(void* thiz, void* method);
typedef float (*CoolFn_t)(void* thiz, void* method);
typedef double (*DictMove_t)(void* inst, void* method);
typedef float (*SpeedFn_t)(void* thiz, void* method);
typedef bool (*MoveNext_t)(void* self, void* method);  // IEnumerator.MoveNext
typedef int  (*GetJumpCount_t)(void* thiz, void* method);             // PlayerActor::get_multipleJumpCount
typedef int  (*GetCurHearts_t)(void* thiz, void* method);             // Hearts::get_currentHearts
typedef void (*SetCurHearts_t)(void* thiz, int32_t v, void* method);  // Hearts::set_currentHearts
// 第六轮补充：⑨ 门槛链 / ⑪ 扣血链（IL2CPP 实例方法：x0=this，尾参 = MethodInfo*）
typedef int  (*IntFn_t)(void* thiz, void* method);                  // op_Implicit / get_*() -> int
typedef bool (*BoolFn_t)(void* thiz, void* method);                 // get_canNotJump / CanMultipleJump
typedef void (*HeartDmg_t)(void* thiz, void* damage, void* method); // DeductHeartFirst / ArmorFirst / ApplyApportion

static DeductHP_t g_origHitDeduct = nullptr;
static DeductHP_t g_origHitFix = nullptr;
static DoDeduct_t g_origDoDeduct = nullptr;
static DeductHP_t g_origHeartDeduct = nullptr;
static BuildPass_t g_origBuildPass = nullptr;
static BuildPass_t g_buildPassBase = nullptr;
static IsSkillCD_t g_origIsSkillCD = nullptr;
static SetSkillCD_t g_origSetSkillCD = nullptr;
static GetRemainCD_t g_origGetRemainCD = nullptr;
static CanUseSkill_t g_origSkillGate[NEON_SKILL_GATE_COUNT] = {};
static SkillBegin_t g_origSkillBegin = nullptr;
static SkillEnd_t g_origSkillEnd = nullptr;
static CoolFn_t g_origSecCool = nullptr;
static CoolFn_t g_origSecCurCool = nullptr;
static DictMove_t g_origDictMove = nullptr;
static SpeedFn_t g_origPlayerSpeedMf = nullptr;
static SpeedFn_t g_origCompMove = nullptr;
static SpeedFn_t g_origMaxSpeed = nullptr;
static MoveNext_t g_origShootIntervalMn = nullptr;
static MoveNext_t g_origMeleeCheckMn = nullptr;
static MoveNext_t g_origGraphAttackMn = nullptr;
static MoveNext_t g_origGraph45Mn = nullptr;

// 对局内资源（NEONPlayerState 成员方法；x0=this，尾参 = MethodInfo*）
typedef void    (*UseCoin_t)(void* thiz, int32_t amount, void* method);
typedef bool    (*HasCoin_t)(void* thiz, int32_t amount, void* method);
typedef int32_t (*GetCoinNum_t)(void* thiz, void* method);
typedef void    (*UseKey_t)(void* thiz, void* method);
typedef bool    (*HasKey0_t)(void* thiz, void* method);
typedef bool    (*HasKeyN_t)(void* thiz, int32_t needCount, void* method);
typedef void    (*UseGKey_t)(void* thiz, void* method);
typedef bool    (*HasGKey_t)(void* thiz, void* method);

static UseCoin_t   g_origUseCoin = nullptr;
static HasCoin_t   g_origHasCoin = nullptr;
static GetCoinNum_t g_origGetCoin = nullptr;
static UseKey_t    g_origUseKey = nullptr;
static HasKey0_t   g_origHasKey0 = nullptr;
static HasKeyN_t   g_origHasKeyN = nullptr;
static UseGKey_t   g_origUseGKey = nullptr;
static HasGKey_t   g_origHasGKey = nullptr;

// 第六轮：⑨ 只替换 getter；⑪ 的血量读写只“直调”（不做 Dobby 替换，避免扰动正常扣血）
static GetJumpCount_t g_origGetJumpCount = nullptr;
static GetCurHearts_t g_fnGetCurHearts = nullptr;
static SetCurHearts_t g_fnSetCurHearts = nullptr;
// 第六轮补充：⑨ 门槛链 / ⑪ 扣血链
static IntFn_t    g_origOpImplicit   = nullptr;
static BoolFn_t   g_origCanNotJump   = nullptr;
static BoolFn_t   g_origCanMultiJump = nullptr;
static HeartDmg_t g_origHeartFirst   = nullptr;
static HeartDmg_t g_origArmorFirst   = nullptr;
static HeartDmg_t g_origApportion    = nullptr;
static void     (*g_origForceDie)(void* thiz, void* method) = nullptr;
static IntFn_t    g_fnGetArmor       = nullptr;
static IntFn_t    g_fnGetSane        = nullptr;

// ＝＝ 第七轮：⑫ 只做「直调」（不新增 Dobby 钩子，全部复用已有钩子）＝＝
typedef void* (*RoomGet_t)(void* thiz, void* method);                            // NPActor.get_actorCurRoomMB

static RoomGet_t     g_fnRoomGet     = nullptr;
static RoomGet_t     g_fnEhiGetRoom  = nullptr;   // EhiEntity.getCurrentRoomMb（0x3E77580，自愈+回写缓存）

static void* Abs(uintptr_t rva) {
    char buf[32];
    snprintf(buf, sizeof(buf), "0x%lX", (unsigned long)rva);
    return getAbsoluteAddress(NEON_LIB, buf);
}

static bool HookOne(const char* tag, uintptr_t rva, void* hook, void** orig) {
    void* addr = Abs(rva);
    if (!addr) {
        LOGE("[Neon] %s resolve failed (0x%lX)", tag, (unsigned long)rva);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Neon] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Neon] hooked %s @ %p (0x%lX)", tag, addr, (unsigned long)rva);
    return true;
}

// 静默版（技能门有 35 处，逐个打日志会淹没 logcat）
static bool HookOneQ(uintptr_t rva, void* hook, void** orig) {
    void* addr = Abs(rva);
    if (!addr) return false;
    if (*orig) return true;
    return DobbyHook(addr, hook, orig) == 0;
}

// 档位 → 倍率（与移速同表；1 = 关）
static float LvlMul(int lvl) {
    switch (lvl) {
        case 2: return 1.5f;
        case 3: return 2.0f;
        case 4: return 3.0f;
        case 5: return 4.0f;
        default: return 1.0f;
    }
}

static float MoveMul() { return LvlMul(g_moveLvl.load()); }
static float AtkMul()  { return LvlMul(g_atkLvl.load()); }   // 攻速统一倍率（近战/远程同一把滑条）

// ==================================================================
//  栈回溯：用来按「调用者身份」区分玩家武器 / 怪物 / 仆从
//  （get_FireDelay 被玩家枪图协程、炮塔、仆从、短剑 RPM 共用，
//    不能全局改；只能看栈里有没有玩家那条协程）
// ==================================================================
// （g_libBase / g_libEnd 的声明已前移到工具区之前，供 NeonInstOf / 类名判定使用）

struct NeonStack {
    uintptr_t pc[24];
    int n;
};

static _Unwind_Reason_Code NeonStackCb(struct _Unwind_Context* ctx, void* arg) {
    NeonStack* s = reinterpret_cast<NeonStack*>(arg);
    uintptr_t pc = reinterpret_cast<uintptr_t>(_Unwind_GetIP(ctx));
    if (pc && s->n < (int)(sizeof(s->pc) / sizeof(s->pc[0]))) s->pc[s->n++] = pc;
    return (s->n >= (int)(sizeof(s->pc) / sizeof(s->pc[0]))) ? _URC_END_OF_STACK : _URC_NO_REASON;
}

static int NeonCapture(NeonStack* s) {
    s->n = 0;
    _Unwind_Backtrace(NeonStackCb, s);
    return s->n;
}

// 栈里第一个落在 [lo,hi) 的 RVA；找不到返回 0
static uintptr_t NeonStackHit(const NeonStack* s, uintptr_t lo, uintptr_t hi) {
    if (!g_libBase || !s) return 0;
    for (int i = 0; i < s->n; ++i) {
        if (s->pc[i] < g_libBase || s->pc[i] >= g_libEnd) continue;
        uintptr_t rva = s->pc[i] - g_libBase;
        if (rva >= lo && rva < hi) return rva;
    }
    return 0;
}

#if NEON_DIAG
static void NeonLogStack(const char* tag, const NeonStack* s) {
    char buf[320];
    int off = 0;
    for (int i = 0; i < s->n && off < 270; ++i) {
        if (s->pc[i] < g_libBase || s->pc[i] >= g_libEnd) continue;
        off += snprintf(buf + off, sizeof(buf) - (size_t)off, " %lx",
                        (unsigned long)(s->pc[i] - g_libBase));
    }
    LOGI("[Neon][stk] %s%s", tag, buf);
}
#endif

// ＝＝ 第七轮公共小件：⑫ 用 ＝＝
// 托管对象体检：klass 指针必须落在 libil2cpp 映射内，否则一律当「不是可用对象」。
// 用途：快照里的目标指针复核（⑫ 全屏攻击）。
// ★2026-11-26 修正：旧实现 g_libEnd = base + 0x8000000（128MB）判「klass 是否落在
//  libil2cpp 内」是错的。实测该 so（llvm-objdump -h，md5 61c2eecb…）段布局：
//    .text 0x170db00 / .rodata 0x7823aa0 / .data.rel.ro 0x99B1060–0x9E5A408
//    .data 0x9F74888–0xA42CC00 / .bss 0xA42CC00–0xA66AA18
//  ⇒ Il2CppClass 元数据全在 ≥0x99B1060（≈154MB），恒 > 0x8000000 ⇒ 本函数恒 false，
//  ⇒ 全屏攻击链路的第一句守卫全部静默 return（真机日志零命中、零崩溃）。
//  ⇒ 改为从 /proc/self/maps 取实测 [最小 start, 最大 end)。
// ★2026-11-26 定案：删掉「klass 必须落在 libil2cpp 映射内」这个自造校验。
//   它害了三轮：libil2cpp 是分段 mmap 的，[min,max) 永远只能覆盖一部分，
//   于是把游戏自己传进来的合法对象（RoomMB / MonsterActor / instigator）全判成野指针。
//   而这些指针的来源全是游戏维护的数据结构（HitPoints._monsterActor、DamageInfo.instigator、
//   RoomMB.mons 元素），本来就必然有效 —— 校验纯属多余，且是唯一失败点。
//   ⇒ 只判空。读 klass 对合法对象永远安全。
static bool NeonLooksLikeManaged(void* obj) {
    return obj != nullptr;
}

// libil2cpp 实测映射范围（g_libEnd 的权威来源）。成功=最小 start/最大 end；
// 失败=base+0xB000000（176MB，覆盖 .bss 末 0xA66AA18）。幂等：同值不重复打日志。
static uintptr_t g_rangeLoggedBase = 0;
static uintptr_t g_rangeLoggedEnd  = 0;

static void NeonRefreshLibRangeFromMaps() {
    const char* soname = NEON_LIB;
    uintptr_t lo = 0, hi = 0;
    if (soname && *soname) {
        FILE* f = fopen("/proc/self/maps", "r");
        if (f) {
            char line[512];
            while (fgets(line, sizeof(line), f)) {
                unsigned long a = 0, b = 0;
                if (sscanf(line, "%lx-%lx", &a, &b) != 2) continue;
                if (!strstr(line, soname)) continue;
                if (!lo || (uintptr_t)a < lo) lo = (uintptr_t)a;
                if ((uintptr_t)b > hi) hi = (uintptr_t)b;
            }
            fclose(f);
        }
    }
    if (lo && hi > lo) {
        // ★2026-11-26 根因修复（无限跳跃真机无效）：RVA 基准必须与钩子解析用的 base 完全一致，
        //   即「第一段 LOAD（off==vaddr==0）」的起始地址。maps 里的最小 start 可能是前置保护段
        //   （实测 maps lo=0x6ddac2c000，而钩子基准 base=0x6f24af4000，差 0x46DCB000）——
        //   旧写法 `if (!g_libBase || lo < g_libBase) g_libBase = lo;` 把基准改小了 74 MB，
        //   于是 NeonStackHit 里 `rva = pc - g_libBase` 整体偏移，所有按 RVA 的栈判据恒不命中
        //   ⇒ PlayerStateInt.op_Implicit 的跳跃上限判据静默失效（无限跳跃「没效果」）。
        //   ⇒ base 只由锚点（Abs(RVA_GRAPH_ATTACK_MN) - RVA）决定；锚点也没有时才退回 lo。
        //   end 只扩不缩（分段 mmap，取最大 end 才能覆盖 .bss 末）。
        if (!g_libBase) g_libBase = lo;
        if (hi > g_libEnd) g_libEnd = hi;
        if (g_libEnd <= g_libBase) g_libEnd = g_libBase + 0xB000000UL;
    } else {
        if (!g_libBase) return;                  // 连 base 都还没有：等下一次
        g_libEnd = g_libBase + 0xB000000UL;      // 兜底 176MB
        LOGI("[Neon] libRange fallback end=+0x%lx (maps 无 %s)",
             (unsigned long)(g_libEnd - g_libBase), soname ? soname : "?");
    }
    if (g_libBase != g_rangeLoggedBase || g_libEnd != g_rangeLoggedEnd) {
        g_rangeLoggedBase = g_libBase;
        g_rangeLoggedEnd  = g_libEnd;
        LOGI("[Neon] libRange base=0x%lx end=0x%lx size=0x%lx",
             (unsigned long)g_libBase, (unsigned long)g_libEnd,
             (unsigned long)(g_libEnd - g_libBase));
    }
}

static uint64_t NeonNowMs() {
    struct timespec ts;
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<uint64_t>(ts.tv_sec) * 1000ull +
           static_cast<uint64_t>(ts.tv_nsec / 1000000L);
}

// 玩家枪械协程地址带：Gun.<MeleeCheck>d__85 / <shootingInterval>d__96 /
// GunGraph.<AttackRoutine>d__45 / <AttackRoutineWithEvents>d__46 / <ChargeRoutine>d__47
#define RVA_GUNROUTINE_LO  0x2929E78UL
#define RVA_GUNROUTINE_HI  0x292C100UL
// Long_Active_001::RefreshShortSwordRPM（短剑 RPM 写入点，属近战路）
#define RVA_SHORTSWORD_LO  0x32E7100UL
#define RVA_SHORTSWORD_HI  0x32E7400UL

// ==================================================================
//  攻速真身（2026-10-02 第三轮）
//    FireDelay(0x3A8F3F4) = 60/RPM：所有枪图协程取间隔都走这里，
//      按栈里是不是玩家枪械协程决定缩放（远程滑条）/ 短剑 RPM（近战滑条）。
//      零残留：只改本次返回值，不写任何字段。
//    AsBusyTime(0x3DABB30)：多段近战副武器槽的「本段忙碌时间」累加器，
//      传入的是增量秒数 ⇒ 只缩放这个增量（挂 set_BusyTime 会因累加器
//      被反复整值除法而崩成 0，故 set_BusyTime 只做观测）。
// ==================================================================
#define RVA_OUT_FIRE_DELAY   0x3A8F3F4UL  // GunOutput::get_FireDelay()
#define RVA_MELEE_AS_BUSY    0x3DABB30UL  // DeputyWeaponSlot_MeleeMultiSeg::AsBusyTime(float)

typedef float (*FireDelay_t)(void*, void*);
typedef void  (*AsBusy_t)(void*, float, void*);

static FireDelay_t g_origFireDelay = nullptr;
static AsBusy_t    g_origMeleeAsBusy = nullptr;

#if NEON_DIAG
static unsigned g_fdDiagN = 0;
static unsigned g_busyDiagN = 0;
#endif

static float Hooked_GetFireDelay(void* thiz, void* method) {
    float v = g_origFireDelay ? g_origFireDelay(thiz, method) : 0.f;
    if (!(v > 0.f) || v > 600.f) return v;

    const float mul = AtkMul();
    const bool needScale = (mul > 1.001f);
#if NEON_DIAG
    const bool wantDiag = (g_fdDiagN < 60u);
#else
    const bool wantDiag = false;
#endif
    if (!needScale && !wantDiag) return v;

    NeonStack st;
    NeonCapture(&st);
    const uintptr_t hitGun = NeonStackHit(&st, RVA_GUNROUTINE_LO, RVA_GUNROUTINE_HI);
    if (hitGun) {
        if (needScale) {
            float nv = v / mul;
            static unsigned n = 0;
            if ((n++ % 6u) == 0u)
                LOGI("[Neon] 攻速(60/RPM·玩家枪) x%.2f 间隔 %.4f->%.4f @0x%lx", mul, v, nv,
                     (unsigned long)hitGun);
            return nv;
        }
#if NEON_DIAG
        if (wantDiag && (g_fdDiagN++ % 3u) == 0u) NeonLogStack("fire.gun", &st);
#endif
        return v;
    }

    const uintptr_t hitSw = NeonStackHit(&st, RVA_SHORTSWORD_LO, RVA_SHORTSWORD_HI);
    if (hitSw) {
        if (needScale) {
            float nv = v / mul;
            static unsigned n = 0;
            if ((n++ % 4u) == 0u)
                LOGI("[Neon] 攻速(短剑RPM) x%.2f 间隔 %.4f->%.4f @0x%lx", mul, v, nv,
                     (unsigned long)hitSw);
            return nv;
        }
        return v;
    }

#if NEON_DIAG
    if (wantDiag && (g_fdDiagN++ % 3u) == 0u) {
        LOGI("[Neon][diag] fire 非玩家路 v=%.4f", v);
        NeonLogStack("fire.other", &st);
    }
#endif
    return v;
}

static void Hooked_MeleeAsBusyTime(void* thiz, float sec, void* method) {
    const float ml = AtkMul();
    if (ml > 1.001f && sec > 0.0001f && sec < 60.f) {
        float ns = sec / ml;
        static unsigned n = 0;
        if ((n++ % 4u) == 0u) {
            LOGI("[Neon] 攻速(多段近战·累加) x%.2f busy %.3f->%.3f", ml, sec, ns);
#if NEON_DIAG
            if (g_busyDiagN++ < 8u) { NeonStack st; NeonCapture(&st); NeonLogStack("asBusy", &st); }
#endif
        }
        sec = ns;
    }
    if (g_origMeleeAsBusy) g_origMeleeAsBusy(thiz, sec, method);
}

// ==================================================================
//  ⑤ 攻速·近战真身（2026-10-02 第四轮真机定位 + 用户实测）
//    梅那把「刀」＝ FireWeaponSlot_MeleeMultiSegGun（: DeputyWeaponSlot_MeleeMultiSeg），
//    挥砍节奏由 _busyTime(0x98) 这个「剩余忙碌倒计时」控制：
//      OnUpdate(0x3DAB76C)：if (_busyTimePaused) return;
//                            b = get_BusyTime();
//                            if (b > 0) { set_BusyTime(b - Time.deltaTime);
//                                         if (get_BusyTime() < 0) OnBusyTimePassed(); }
//      ⇒ 每帧写回「剩余秒数」，归零后由 OnBusyTimePassed 重新写入满值（实测 0.75s）。
//    做法：只缩放「上升跳变」（＝新窗口起点），逐帧递减按原样透传 ⇒
//      窗口线性缩短 mul 倍，不污染累加器（AsBusyTime 的 get+add+set 也走本函数，天然覆盖）。
// ==================================================================
#define RVA_MELEE_SET_BUSY   0x3DA9D70UL  // DeputyWeaponSlot_MeleeMultiSeg::set_BusyTime(float)
typedef void (*SetBusy_t)(void*, float, void*);

static SetBusy_t g_origSetBusy = nullptr;
static void*     g_lastBusyObj = nullptr;
static float     g_lastBusyRaw = -999.f;   // 上一次「透传/写入」的原始秒数（用于识别新一轮）

static void Hooked_SetBusyTime(void* thiz, float sec, void* method) {
    float out = sec;
    const float mul = AtkMul();
    if (mul > 1.001f && thiz && sec > 0.30f && sec < 60.f) {
        // 同一对象：只有比上次值高出 0.30 以上才算「新一轮起点」（逐帧递减不会触发）
        const bool same = (thiz == g_lastBusyObj);
        if (!same || sec > g_lastBusyRaw + 0.30f) {
            out = sec / mul;
            static unsigned n = 0;
            if ((n++ % 4u) == 0u)
                LOGI("[Neon] 攻速(多段近战CD) x%.2f 忙碌 %.3f->%.3f", mul, sec, out);
        }
    }
    if (sec > 0.0001f) { g_lastBusyObj = thiz; g_lastBusyRaw = sec; }
    if (g_origSetBusy) g_origSetBusy(thiz, out, method);
}

static void AmplifyDamage(void* damage) {
    if (!damage) return;
    auto p = reinterpret_cast<uintptr_t>(damage);
    *reinterpret_cast<float*>(p + OFF_DMG_DAMAGE) = 999999.f;
    *reinterpret_cast<float*>(p + OFF_DMG_FINAL) = 999999.f;
    *reinterpret_cast<float*>(p + OFF_DMG_BASE) = 999999.f;
}

static void ForceKillHp(void* thiz) {
    if (!thiz) return;
    *reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(thiz) + OFF_HP_CUR) = 0.f;
}

// ⑨ 无限跳跃：MultipleJump 的门槛是 if (get_multipleJumpCount() >= multipleJumpCountLimit) return;
//    返回负数即可永远通过；跳完 set_multipleJumpCount(count + 2) 依然为负，不会回正。
static int Hooked_GetMultipleJumpCount(void* thiz, void* method) {
    if (g_noJumpLimit.load()) return -1000;
    return g_origGetJumpCount ? g_origGetJumpCount(thiz, method) : 0;
}

// ⑨ 无限跳跃（补）：UpdateJump / MultipleJump 都用 op_Implicit(multipleJumpCountLimit) 取上限，
//    玩家没有二段跳道具时上限 = 0，`cmp w0,#1 / b.lt` 直接 return —— 这才是“只能跳一次”的真凶。
//    栈窄带只在跳跃门槛那两条返回地址上生效，其它 PlayerStateInt 转换原样透传。
static int Hooked_PlayerStateInt_OpImplicit(void* thiz, void* method) {
    if (g_noJumpLimit.load()) {
        NeonStack st; NeonCapture(&st);
        if (NeonStackHit(&st, 0x2652F60UL, 0x2652F90UL) ||   // PlayerActor::UpdateJump 多跳分支
            NeonStackHit(&st, 0x264FD90UL, 0x264FDACUL)) {   // PlayerActor::MultipleJump 次数闸
            static unsigned jn = 0;
            if ((jn++ % 20u) == 0u) LOGI("[Neon] 无限跳跃：次数上限 -> 999");
            return 999;
        }
    }
    return g_origOpImplicit ? g_origOpImplicit(thiz, method) : 0;
}

// ⑨ 无限跳跃（补）：输入层每帧写 [inputData+0x4C] = !get_canNotJump，MultipleJump 末尾也查它
static bool Hooked_GetCanNotJump(void* thiz, void* method) {
    if (g_noJumpLimit.load()) return false;
    return g_origCanNotJump ? g_origCanNotJump(thiz, method) : false;
}

// ⑨ 无限跳跃（补）：MultipleJump 内唯一调用点（全游戏仅此一处）
static bool Hooked_CanMultipleJump(void* thiz, void* method) {
    if (g_noJumpLimit.load()) return true;
    return g_origCanMultiJump ? g_origCanMultiJump(thiz, method) : false;
}

// ==================================================================
//  ⑫ 全屏攻击（2026-11-26 第七轮）
//
//  挂点：**扩展现有 Hooked_HitPoints_DeductHP**（RVA 0x346D8C4 = HitPoints.DeductHP），
//        不新建第二个钩子（同一个函数上挂两次 Dobby 会互相踩）。
//  触发：g_fullAtk 开 && 不在镜像递归里 && 受害者 faction==2（怪物）
//        && DamageInfo+0x18(finalDamage) > 0 && 攻击者是玩家。
//        玩家判据：先看 DamageInfo+0x78(instigaterIsPlayer)；为假再退判
//        DamageInfo+0x98(targetFilter)==2 —— 这份伤害本身就是冲着怪物阵营做的，
//        造它的一方只能是玩家侧（怪物打怪物用的是别的 filter）。
//  动作：受害者 MonsterActor → NPActor.get_actorCurRoomMB(0x3369A64) 取 RoomMB*
//        → RoomMB.mons(List<MonsterActor>) → **先把元素指针快照到本地数组**
//          （扣血过程会从 mons 里移除死亡项，边遍历边用原表必炸）
//        → 逐个跳过「自己」「CurrentHp<=0」→ memcpy 0xD0 字节 DamageInfo 副本
//        → 直调原 DeductHP 打一下。
//  闸门：t_mirrorDepth 用 RAII 严格成对（任何 early return 都不会漏减）；
//        单次最多 32 个目标；同一 damageOpId 在 80ms 内只镜像一次。
// ==================================================================
static thread_local int t_mirrorDepth = 0;

namespace {
struct NeonMirrorScope {                       // 递归闸：构造 ++ / 析构 --，异常路径也成对
    NeonMirrorScope() { ++t_mirrorDepth; }
    ~NeonMirrorScope() { --t_mirrorDepth; }
};
}  // namespace

static int32_t  g_fsLastOpId = 0;
static uint64_t g_fsLastMs   = 0;

// ⑫ 当前房间捕获：静态回调 YiHai.OnEnterRoom/OnLeaveRoom(RoomMB)，x0 = RoomMB*
typedef void (*RoomCb_t)(void* rmb, void* method);
static RoomCb_t g_origRoomEnter = nullptr;
static RoomCb_t g_origRoomLeave = nullptr;

// 读 Il2CppClass.name（类名字符串落在 .rodata，属 libil2cpp 映射，但不是托管对象——
// 不能拿 NeonLooksLikeManaged 判它）。返回是否读到可打印串。
static bool NeonClassNameOf(void* obj, char* out, size_t outsz) {
    if (!out || outsz < 8) return false;
    out[0] = 0;
    if (!obj) return false;
    void* klass = *reinterpret_cast<void**>(obj);
    if (!klass) return false;
    const char* nm = *reinterpret_cast<const char**>(reinterpret_cast<uintptr_t>(klass) + 0x10);
    if (!nm) return false;
    size_t i = 0;
    for (; i + 1 < outsz && nm[i]; ++i) {
        const unsigned char c = static_cast<unsigned char>(nm[i]);
        if (c < 0x20 || c > 0x7e) { out[0] = 0; return false; }
        out[i] = static_cast<char>(c);
    }
    out[i] = 0;
    return i > 0;
}

// 房间可用性：不按类名判（真机 OnEnterRoom 的候选被 name 校验误杀过），只按形态——
//   ① room 是托管对象 ② room+0x78 是合法 List（_items 托管、_size 0..512）
static bool NeonRoomUsable(void* room) {
    if (!NeonLooksLikeManaged(room)) return false;
    void* list = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(room) + OFF_ROOM_MONS);
    if (!NeonLooksLikeManaged(list)) return false;
    void* items = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(list) + OFF_LIST_ITEMS);
    const int32_t n = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + OFF_LIST_SIZE);
    return NeonLooksLikeManaged(items) && n >= 0 && n <= 512;
}

// 房间候选：任何一路钩子拿到 RoomMB 都往这里送。只有第一次（或换房间）才打日志。
static void NeonOfferRoom(const char* src, void* rmb) {
    if (!rmb) return;
    void* cur = g_curRoom.load();
    if (cur == rmb) return;                     // 同一房间，不重复处理（高频钩子零日志开销）
    // 校验放宽（2026-11-26 第三版）：真机日志显示 OnEnterRoom 的候选被 NeonLooksLikeRoomMB 拒了，
    // 而该判定依赖 Il2CppClass.name == "RoomMB"（klass+0x10）。偏移一旦不符就全部误杀。
    // 改为：① klass 必须落在 libil2cpp 映射内；② [rmb+0x78] 必须像一个 List（_items 合法、
    // _size 在 0..512）。两者都过即接受，并把 klass/name 打进日志，下一次就能看清真实类型名。
    if (!NeonLooksLikeManaged(rmb)) {
        static std::atomic<unsigned> badn0{0};
        if (badn0.fetch_add(1) < 5u)
            LOGI("[Neon][fs] 房间候选被拒(非托管) src=%s ptr=%p", src, rmb);
        return;
    }
    char nm[48] = {0};
    const bool nameOk = NeonClassNameOf(rmb, nm, sizeof(nm));
    void* list = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(rmb) + OFF_ROOM_MONS);
    const bool listOk = NeonLooksLikeManaged(list);
    int32_t sz = -1;
    if (listOk) {
        void* items = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(list) + OFF_LIST_ITEMS);
        const int32_t n = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + OFF_LIST_SIZE);
        sz = (NeonLooksLikeManaged(items) && n >= 0 && n <= 512) ? n : -1;
    }
    // 日志：无论接不接受都打（每次换候选一行，不会刷屏），便于一眼确认类型名与列表形态
    LOGI("[Neon][fs] 房间候选 src=%s rmb=%p klass=%p name=%s list=%p size=%d",
         src, rmb, *reinterpret_cast<void**>(rmb), nameOk ? nm : "?", list, static_cast<int>(sz));
    if (sz < 0) return;                         // 不像 RoomMB（+0x78 不是合法 List）⇒ 不当房间用
    g_curRoom.store(rmb);
    LOGI("[Neon][fs] 当前房间已捕获 src=%s rmb=%p size=%d", src, rmb, static_cast<int>(sz));
}

static void Hooked_OnEnterRoom(void* rmb, void* method) {
    NeonOfferRoom("OnEnterRoom", rmb);
    if (g_origRoomEnter) g_origRoomEnter(rmb, method);
}

static void Hooked_OnLeaveRoom(void* rmb, void* method) {
    void* cur = g_curRoom.load();
    if (!rmb || rmb == cur) g_curRoom.store(nullptr);
    if (g_origRoomLeave) g_origRoomLeave(rmb, method);
}

// 路 2：YiHai.OnEnterBattle(RoomMB) —— 进战斗时一定带 rmb
typedef void (*RoomCb2_t)(void* rmb, void* method);
static RoomCb2_t g_origRoomBattle = nullptr;
static void Hooked_OnEnterBattle(void* rmb, void* method) {
    NeonOfferRoom("OnEnterBattle", rmb);
    if (g_origRoomBattle) g_origRoomBattle(rmb, method);
}

// 路 3：RoomMB.MonsterListCleanup()（this=RoomMB，怪死亡时调）
typedef void (*RoomThis_t)(void* thiz, void* method);
static RoomThis_t g_origRoomCleanup = nullptr;
static void Hooked_MonsterListCleanup(void* thiz, void* method) {
    NeonOfferRoom("MonsterListCleanup", thiz);
    if (g_origRoomCleanup) g_origRoomCleanup(thiz, method);
}

// ★主路：MapManager.LateUpdate()（每帧，this=MapManager）→ 读 [this+0x20] = curRoom
typedef void (*MapLate_t)(void* thiz, void* method);
static MapLate_t g_origMapLate = nullptr;
static void Hooked_MapLateUpdate(void* thiz, void* method) {
    if (thiz && g_libBase) {
        void* r = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(thiz) + OFF_MAP_CURROOM);
        if (r) NeonOfferRoom("MapManager.curRoom", r);
        else if (g_curRoom.load()) g_curRoom.store(nullptr);   // 不在房间：清空
    }
    if (g_origMapLate) g_origMapLate(thiz, method);
}

// 备用：MapManager.RefreshRoom(RoomMB)（x0=this, x1=RoomMB）
typedef void (*MapRefresh_t)(void* thiz, void* rmb, void* method);
static MapRefresh_t g_origMapRefresh = nullptr;
static void Hooked_MapRefreshRoom(void* thiz, void* rmb, void* method) {
    NeonOfferRoom("MapManager.RefreshRoom", rmb);
    if (g_origMapRefresh) g_origMapRefresh(thiz, rmb, method);
}

// 路 4：RoomMB.get_ActiveMonsterCount()（this=RoomMB，最高频，兜底必中）
typedef int32_t (*RoomCnt_t)(void* thiz, void* method);
static RoomCnt_t g_origRoomActiveCnt = nullptr;
static int32_t Hooked_ActiveMonsterCount(void* thiz, void* method) {
    NeonOfferRoom("ActiveMonsterCount", thiz);
    return g_origRoomActiveCnt ? g_origRoomActiveCnt(thiz, method) : 0;
}

static void FullScreenAtkMirror(void* thiz, void* damage) {
    if (!thiz || !damage) return;
    if (t_mirrorDepth != 0) return;             // 已在镜像里：绝不再镜像（防连锁爆炸）
    if (!g_origHitDeduct) return;

    const uintptr_t di = reinterpret_cast<uintptr_t>(damage);
    const int32_t opId = *reinterpret_cast<int32_t*>(di + OFF_DMG_OP_ID);
    const uint64_t now = NeonNowMs();
    // 同一次攻击的多段结算（damageOpId 相同）在 80ms 内只镜像一次
    if (opId != 0 && opId == g_fsLastOpId && now >= g_fsLastMs && (now - g_fsLastMs) < 80ull)
        return;
    g_fsLastOpId = opId;
    g_fsLastMs   = now;

    NeonMirrorScope scope;                      // 递归闸：从这一行起算「在了」

    // 受害者 MonsterActor：HitPoints._monsterActor(0x1B0) → Killable._monsterActorOwner(0xF0)
    const uintptr_t hp = reinterpret_cast<uintptr_t>(thiz);
    void* mon = *reinterpret_cast<void**>(hp + OFF_HP_MON_ACTOR);
    if (!mon) mon = *reinterpret_cast<void**>(hp + OFF_KILLABLE_MON_OWN);
    const bool monOk = NeonLooksLikeManaged(mon);

    // 房间来源优先级：① OnEnterRoom 捕获到的 RoomMB（真机唯一可用路径）
    //                  ② 回退 NPActor.get_actorCurRoomMB（真机实测返回 0，仅兜底）
    // 房间解析四级兜底（2026-11-26 定案，每级都有反汇编依据）：
    //   ① g_curRoom：MapManager.LateUpdate 每帧读 [this+0x20]
    //   ② EhiEntity._currentRoomMb：直接读 [mon+0x48]（游戏自己缓存的当前房间）
    //   ③ EhiEntity.getCurrentRoomMb(0x3E77580)：主动调用，内部 GetComponentsInParent 并回写缓存
    //   ④ NPActor.get_actorCurRoomMB(0x3369A64)：GetComponentInParent，真机常返回 0，仅最后兜底
    void* room = g_curRoom.load();
    if (!NeonRoomUsable(room) && monOk)
        room = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(mon) + OFF_EHI_CURROOM);
    if (!NeonRoomUsable(room) && monOk && g_fnEhiGetRoom)
        room = g_fnEhiGetRoom(mon, nullptr);
    if (!NeonRoomUsable(room) && monOk)
        room = g_fnRoomGet(mon, nullptr);
    const bool roomOk = NeonRoomUsable(room);
    // 两个列表都要：mons(+0x78) 是常规刷怪，_buffSpawnMons(+0x80) 是 buff 召唤出来的怪。
    const int32_t roomEntered = roomOk
        ? static_cast<int32_t>(*reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(room) + OFF_ROOM_ENTERED)) : -1;
    void* listMain = roomOk ? *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(room) + OFF_ROOM_MONS) : nullptr;
    void* listBuff = roomOk ? *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(room) + OFF_ROOM_BUFFMONS) : nullptr;
    auto listSize = [](void* l) -> int32_t {
        if (!NeonLooksLikeManaged(l)) return -1;
        void* it = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(l) + OFF_LIST_ITEMS);
        const int32_t n = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(l) + OFF_LIST_SIZE);
        if (!NeonLooksLikeManaged(it) || n < 0) return -1;
        const int32_t alen = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(it) + OFF_ARRAY_LEN);
        return (n < alen) ? n : alen;              // 双上限：_size 与数组长度取小，防越界
    };
    const int32_t sizeMain = listSize(listMain);
    const int32_t sizeBuff = listSize(listBuff);

    const uintptr_t dpEarly = reinterpret_cast<uintptr_t>(damage);
    void* inst  = *reinterpret_cast<void**>(dpEarly + OFF_DMG_INSTIGATOR);
    void* rinst = *reinterpret_cast<void**>(dpEarly + OFF_DMG_REAL_INST);

    // [Neon][fs] 一次性诊断（默认开，最多 10 行）：卡在哪个判据一眼可见。
    // 判据顺序：fac==2（受害者是怪物） / filter&2（目标含怪物） / isP（玩家发起） / fin>0
    {
        static std::atomic<unsigned> fsn{0};
        if (fsn.fetch_add(1) < 10u) {
            const uintptr_t dp = reinterpret_cast<uintptr_t>(damage);
            LOGI("[Neon][fs] hit fac=%d filter=%d isP=%d fin=%.1f inst=%p pl=%d mon=%p monOk=%d room=%p entered=%d mons=%d buff=%d",
                 *reinterpret_cast<int32_t*>(hp + OFF_KILLABLE_FACTION),
                 *reinterpret_cast<int32_t*>(dp + OFF_DMG_TARGET_FLT),
                 static_cast<int>(*reinterpret_cast<uint8_t*>(dp + OFF_DMG_PLAYER_HIT)),
                 *reinterpret_cast<float*>(dp + OFF_DMG_FINAL),
                 inst, (NeonIsPlayerInstigator(inst) ? 1 : 0),
                 mon, monOk ? 1 : 0, room, static_cast<int>(roomEntered),
                 static_cast<int>(sizeMain), static_cast<int>(sizeBuff));
        }
    }
    // 不再强制 monOk：房间可用就足够枚举（mon 仅用于「跳过受害者自己」）
    if (!roomOk) return;
    if (sizeMain < 0 && sizeBuff < 0) return;
    // roomIsEnteren(+0x109) 不再作为硬门槛：真机实测战斗中它仍是 0（该字段只表示
    // 「进入房间的过场是否播完」，不是「玩家此刻在不在这个房间」）。房间本身已经来自
    // 玩家自己的 EhiEntity._currentRoomMb / MapManager.curRoom，不存在打错房间的问题。
    (void)roomEntered;

    // ① 先快照：后面每一次 DirectDeduct 都可能改写列表（死亡移除），必须用本地副本
    void* snap[48];
    int n = 0;
    auto drain = [&](void* lst, int32_t lim) {
        if (!NeonLooksLikeManaged(lst) || lim <= 0) return;
        void* items = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(lst) + OFF_LIST_ITEMS);
        if (!NeonLooksLikeManaged(items)) return;
        if (lim > 48) lim = 48;
        for (int32_t i = 0; i < lim && n < 48; ++i) {
            void* m = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(items) +
                                                OFF_ARRAY_FIRST +
                                                static_cast<uintptr_t>(i) * sizeof(void*));
            if (!m || (monOk && m == mon)) continue;
            bool dup = false;
            for (int k = 0; k < n; ++k) if (snap[k] == m) { dup = true; break; }
            if (!dup) snap[n++] = m;
        }
    };
    drain(listMain, sizeMain);
    drain(listBuff, sizeBuff);
    if (n <= 0) return;

    // ② 再逐个打
    int mirrored = 0;
    for (int i = 0; i < n; ++i) {
        void* target = snap[i];
        if (!target) continue;
        if (monOk && target == mon) continue;    // 跳过受害者自己（它正在被原逻辑结算）
        void* tHp = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(target) + OFF_MON_HP);
        if (!NeonLooksLikeManaged(tHp)) continue;  // 读不到＝指针不可用：跳过（不硬读）
        const float cur = *reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(tHp) + OFF_HP_CUR);
        if (!(cur > 0.f)) continue;              // 已死（或血量读成 0）的打不动
        alignas(16) unsigned char copy[0xD0];    // 与 DeductHP 内部复制量一致
        memcpy(copy, damage, 0xD0);
        g_origHitDeduct(tHp, copy, 1, nullptr);
        ++mirrored;
    }
    if (mirrored > 0)
        LOGI("[Neon] 全屏攻击 镜像 n=%d op=%d", mirrored, static_cast<int>(opId));
}

static bool Hooked_HitPoints_DeductHP(void* thiz, void* damage, int32_t count, void* method) {
    // ⑩ 伤害倍率：DeductHP 会把 DamageInfo 复制 0xD0 字节后交给 DoDeductHP，
    //    DoDeductHP 实际读取的伤害浮点是 DamageInfo+0x18（FINAL）。只在这一处缩放，
    //    绝不在 DoDeduct 钩子里再缩放一次（否则倍率被平方）。
    const int dmul = g_dmgMul.load();
    if (dmul > 1 && !g_seckill.load() && damage) {
        auto dp = reinterpret_cast<uintptr_t>(damage);
        *reinterpret_cast<float*>(dp + OFF_DMG_BASE)   *= static_cast<float>(dmul);
        *reinterpret_cast<float*>(dp + OFF_DMG_FINAL)  *= static_cast<float>(dmul);
        *reinterpret_cast<float*>(dp + OFF_DMG_DAMAGE) *= static_cast<float>(dmul);
        static unsigned dn = 0;
        if ((dn++ % 20u) == 0u)
            LOGI("[Neon] 伤害倍率 x%d（damage+0x18 已放大）", dmul);
    }
    if (g_seckill.load()) {
        AmplifyDamage(damage);
        ForceKillHp(thiz);
    }
    // ⑫ 全屏攻击**不在这里**做：DeductHP 入口处 DamageInfo+0x18(finalDamage) 尚未计算，
    //    它由函数内部第 2 步 FixDeductHpValue 才写入（实测 0x346B9C8: str s8,[x20,#0x18]）。
    //    ⇒ 触发点挪到 Hooked_HitPoints_DoDeduct（那里 +0x18 已是真实扣血量）。
    return g_origHitDeduct ? g_origHitDeduct(thiz, damage, count, method) : false;
}

static bool Hooked_HitPoints_FixDeduct(void* thiz, void* damage, int32_t count, void* method) {
    if (g_seckill.load()) AmplifyDamage(damage);
    return g_origHitFix ? g_origHitFix(thiz, damage, count, method) : false;
}

static void Hooked_HitPoints_DoDeduct(void* thiz, void* damage, void* method) {
    if (g_seckill.load()) {
        AmplifyDamage(damage);
        ForceKillHp(thiz);
    }
    // ⑫ 全屏攻击：**唯一触发点**（2026-11-26 第二版）
    //   为什么挂这里而不是 DeductHP：DeductHP 入口的 DamageInfo+0x18 还没算（实测恒 0），
    //   它由 DeductHP 内部 FixDeductHpValue(0x346B738) 在 0x346B9C8 写入；DoDeductHP 是
    //   FixDeductHpValue 之后的第一站，此处 +0x18 = 真实扣血量，且每次伤害只调一次。
    if (g_fullAtk.load() && thiz && damage && t_mirrorDepth == 0) {
        const uintptr_t dp = reinterpret_cast<uintptr_t>(damage);
        const int faction = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(thiz) +
                                                       OFF_KILLABLE_FACTION);
        const int32_t tflt = *reinterpret_cast<int32_t*>(dp + OFF_DMG_TARGET_FLT);
        const int     isP  = static_cast<int>(*reinterpret_cast<uint8_t*>(dp + OFF_DMG_PLAYER_HIT));
        const float   dmg  = *reinterpret_cast<float*>(dp + OFF_DMG_DAMAGE);
        const float   fin  = *reinterpret_cast<float*>(dp + OFF_DMG_FINAL);
        const float   base = *reinterpret_cast<float*>(dp + OFF_DMG_BASE);
        const int     src  = *reinterpret_cast<int32_t*>(dp + OFF_DMG_SRC_TYPE);
        // 玩家判据（2026-11-26 静态定案）：
        //   ① di.instigator(+0x70)/RealInstigator(+0x90) is PlayerActor / NetPlayerActor
        //      —— 这与游戏自己的 ToolCommon.IsMatchInstigaterType(Player,…) 逐指令等价
        //   ② 该 instigator 在「玩家枪最近开火」缓存里（覆盖玩家枪路径上 instigator 是 Gun 的情况）
        //   ③ 玩家枪图攻击协程 800ms 时间窗
        //   注：+0x78 instigaterIsPlayer 是死字段（全 so 无写入），targetFilter 与施放者无关，均不作主判据。
        void* instDo  = *reinterpret_cast<void**>(dp + OFF_DMG_INSTIGATOR);
        void* rinstDo = *reinterpret_cast<void**>(dp + OFF_DMG_REAL_INST);
        const uint64_t lastAtk = g_lastPlayerAtkMs.load();
        const uint64_t nowMs   = NeonNowMs();
        const bool inWindow = lastAtk != 0 && nowMs >= lastAtk && (nowMs - lastAtk) <= 800ull;
        const bool playerInst = NeonIsPlayerInstigator(instDo) || NeonIsPlayerInstigator(rinstDo);
        const bool recentInst = NeonIsRecentPlayerInstigator(instDo) || NeonIsRecentPlayerInstigator(rinstDo);
        const bool byPlayer = playerInst || recentInst || inWindow;
        if (faction == 2 && byPlayer && fin > 0.f) {
            FullScreenAtkMirror(thiz, damage);
        } else {
            // 诊断（最多 20 行）：一次打全 fac/filter/isP/三伤害值/src/时间窗，便于定位
            static std::atomic<unsigned> gn{0};
            if (gn.fetch_add(1) < 20u)
                LOGI("[Neon][fs] gate-deny fac=%d filter=%d isP=%d dmg=%.1f fin=%.1f base=%.1f src=%d win=%d",
                     faction, tflt, isP, dmg, fin, base, src, inWindow ? 1 : 0);
        }
    }
    if (g_origDoDeduct) g_origDoDeduct(thiz, damage, method);
}

static bool Hooked_Hearts_DeductHP(void* thiz, void* damage, int32_t count, void* method) {
    // ⑪ 丝血无敌：改写点在下面三处「扣血叶函数」（见 LastStandClamp）。
    //    旧实现用第三参 count 当“要扣的心数”是错的——Hearts::DeductHP 里 w21 早被 hearts 覆盖，
    //    条件是恒假的，所以真机一条日志都没出。这里只留诊断探针：心/甲/理智/本次伤害。
    if (g_lastHeart.load() && thiz && damage) {
        static unsigned sn = 0;
        if ((sn++ % 20u) == 0u) {
            const int cur   = g_fnGetCurHearts ? g_fnGetCurHearts(thiz, nullptr) : -1;
            const int armor = g_fnGetArmor    ? g_fnGetArmor(thiz, nullptr)    : -1;
            const int sane  = g_fnGetSane     ? g_fnGetSane(thiz, nullptr)     : -1;
            const float f   = *reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(damage) + 0x10);
            LOGI("[Neon] 丝血无敌[入口] 心=%d 甲=%d 理智=%d 伤害=%.2f", cur, armor, sane, f);
        }
    }
    if (g_invincible.load()) {
        if (thiz) {
            *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(thiz) + OFF_HEARTS_INV) = 1;
            *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(thiz) + OFF_HEARTS_UNKILL) = 1;
        }
        return false;
    }
    return g_origHeartDeduct ? g_origHeartDeduct(thiz, damage, count, method) : false;
}

// ⑪ 丝血无敌（补）：真正的心数扣减发生在 Hearts::DeductHeartFirst 的
//    `set_currentHearts(get_currentHearts() - w22)`，而 w22 = (int)(DamageInfo+0x10)。
//    所以直接截断 DamageInfo+0x10，让总血量（心+甲）最低只掉到 1：
//      · 已经只剩 1 点 → 伤害写 0，完全免伤
//      · 这一击本该打死 → 只允许扣到剩 1 点
//    护甲分支 / 分摊分支同签名，一并处理。
static void LastStandClamp(void* thiz, void* damage, const char* tag) {
    if (!thiz || !damage) return;
    float* pf = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(damage) + 0x10);
    const int dmg = static_cast<int>(*pf);
    if (dmg <= 0) return;
    const int hearts = g_fnGetCurHearts ? g_fnGetCurHearts(thiz, nullptr) : 1;
    const int armor  = g_fnGetArmor    ? g_fnGetArmor(thiz, nullptr)    : 0;
    const int tot = (hearts > 0 ? hearts : 0) + (armor > 0 ? armor : 0);
    if (tot >= 1 && tot - dmg < 1) {
        const int allowed = (tot > 1) ? (tot - 1) : 0;   // 最低保留 1 点
        *pf = static_cast<float>(allowed);
        static unsigned ln = 0;
        if ((ln++ % 10u) == 0u)
            LOGI("[Neon] 丝血无敌(%s)：心=%d 甲=%d 伤害 %d->%d", tag, hearts, armor, dmg, allowed);
    }
}

static void Hooked_HeartFirst(void* thiz, void* damage, void* method) {
    if (g_lastHeart.load()) LastStandClamp(thiz, damage, "心");
    if (g_origHeartFirst) g_origHeartFirst(thiz, damage, method);
}

static void Hooked_ArmorFirst(void* thiz, void* damage, void* method) {
    if (g_lastHeart.load()) LastStandClamp(thiz, damage, "甲");
    if (g_origArmorFirst) g_origArmorFirst(thiz, damage, method);
}

static void Hooked_HeartApportion(void* thiz, void* damage, void* method) {
    if (g_lastHeart.load()) LastStandClamp(thiz, damage, "分摊");
    if (g_origApportion) g_origApportion(thiz, damage, method);
}

// ⑪ 兜底：心/甲归零后的死亡入口（内部按 (心+甲)×5 造伤害再 TakeDamage），直接不执行并钉回 1 心
static void Hooked_Hearts_ForceDie(void* thiz, void* method) {
    if (g_lastHeart.load()) {
        if (thiz && g_fnSetCurHearts) g_fnSetCurHearts(thiz, 1, nullptr);
        static unsigned fn = 0;
        if ((fn++ % 5u) == 0u) LOGI("[Neon] 丝血无敌：拦截 ForceDie");
        return;
    }
    if (g_origForceDie) g_origForceDie(thiz, method);
}

static void* Hooked_BuildPassCheckInfo(void* thiz, void* method) {
    if ((g_invincible.load() || g_seckill.load()) && g_buildPassBase)
        return g_buildPassBase(thiz, method);
    return g_origBuildPass ? g_origBuildPass(thiz, method) : nullptr;
}

static bool Hooked_IsSkillCD(void* thiz, void* key, void* method) {
    if (g_noCd.load()) return false;
    return g_origIsSkillCD ? g_origIsSkillCD(thiz, key, method) : false;
}

static void Hooked_SetSkillCD(void* thiz, void* key, float countTime, void* method) {
    if (g_noCd.load()) {
        if (g_origSetSkillCD) g_origSetSkillCD(thiz, key, 0.f, method);
        return;
    }
    if (g_origSetSkillCD) g_origSetSkillCD(thiz, key, countTime, method);
}

static float Hooked_GetRemainCD(void* thiz, void* key, void* method) {
    if (g_noCd.load()) return 0.f;
    return g_origGetRemainCD ? g_origGetRemainCD(thiz, key, method) : 0.f;
}

// ---- 技能门：35 个 CanUseSkill 实现，逐实例清零冷却字段后强制可用 ----
template <int N>
static bool Hooked_SkillGate(void* thiz, void* method) {
    if (g_noCd.load()) {
        if (thiz) {
            auto p = reinterpret_cast<uintptr_t>(thiz);
            *reinterpret_cast<float*>(p + OFF_SEC_COOLING_TIME) = 0.f;
            *reinterpret_cast<float*>(p + OFF_SEC_LAST_COOL) = 0.f;
        }
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u)
            LOGI("[Neon] 技能无CD: %s.CanUseSkill -> true", kSkillGates[N].name);
        return true;
    }
    return g_origSkillGate[N] ? g_origSkillGate[N](thiz, method) : false;
}

template <std::size_t... I>
static int InstallSkillGates(std::index_sequence<I...>) {
    int ok = 0;
    ((HookOneQ(kSkillGates[I].rva, (void*)&Hooked_SkillGate<(int)I>,
               (void**)&g_origSkillGate[I]) ? ++ok : 0), ...);
    return ok;
}

// 探针：技能是否真的开始/结束（排障用，只打日志不改行为）
static void Hooked_SkillBegin(void* thiz, bool isSecSkill, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u)
        LOGI("[Neon] probe SkillBegin thiz=%p isSec=%d noCd=%d",
             thiz, isSecSkill ? 1 : 0, g_noCd.load() ? 1 : 0);
    if (g_origSkillBegin) g_origSkillBegin(thiz, isSecSkill, method);
}

static void Hooked_SkillEnd(void* thiz, void* method) {
    if (g_origSkillEnd) g_origSkillEnd(thiz, method);
}

static float Hooked_SecCoolTime(void* thiz, void* method) {
    if (g_noCd.load()) return 0.01f;
    return g_origSecCool ? g_origSecCool(thiz, method) : 0.f;
}

static float Hooked_SecCurCool(void* thiz, void* method) {
    if (g_noCd.load()) return 0.f;
    return g_origSecCurCool ? g_origSecCurCool(thiz, method) : 0.f;
}

static double Hooked_DictMoveSpeed(void* inst, void* method) {
    double v = g_origDictMove ? g_origDictMove(inst, method) : 1.0;
    float mul = MoveMul();
    if (mul > 1.01f) return v * (double)mul;
    return v;
}

static float Hooked_PlayerSpeedMf(void* thiz, void* method) {
    float v = g_origPlayerSpeedMf ? g_origPlayerSpeedMf(thiz, method) : 1.f;
    float mul = MoveMul();
    if (mul > 1.01f) return v * mul;
    return v;
}

static float Hooked_CompMoveSpeed(void* thiz, void* method) {
    float v = g_origCompMove ? g_origCompMove(thiz, method) : 0.f;
    float mul = MoveMul();
    if (mul > 1.01f) return v * mul;
    return v;
}

static float Hooked_GetMaxSpeed(void* thiz, void* method) {
    float v = g_origMaxSpeed ? g_origMaxSpeed(thiz, method) : 0.f;
    float mul = MoveMul();
    if (mul > 1.01f) return v * mul;
    return v;
}

// ================================================================
//  武器攻速（2026-10-02）
//  仅动「协程本地等待量」，不写 Gun/GunOutput 的持久字段
//  ⇒ 失效即恢复原值，随时可零残留关掉
// ================================================================

// 取状态机的 <>4__this（Gun）并校验是玩家武器（InstigatorIsPlayer）
static bool PlayerGunOf(void* sm, void** outGun) {
    void* gun = nullptr;
    if (sm) gun = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(sm) + OFF_SM_GUN);
    if (outGun) *outGun = gun;
    if (!gun) return false;
    return *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(gun) +
                                      OFF_INSTIGATOR_IS_PLAYER) != 0;
}

// 通用槽位表：按「状态机对象指针」记账，判「数值突然变大 = 新赋值」后缩放一次
template <int MAX>
struct NeonSlots {
    struct Slot { void* sm; float prev; };
    Slot tab[MAX]{};
    int Find(void* sm) {
        int freeIdx = -1;
        for (int i = 0; i < MAX; ++i) {
            if (tab[i].sm == sm) return i;
            if (!tab[i].sm && freeIdx < 0) freeIdx = i;
        }
        if (freeIdx >= 0) { tab[freeIdx].sm = sm; tab[freeIdx].prev = 0.f; }
        return freeIdx;   // -1 = 槽满：宁可本次不生效，也不做会连乘的抢占
    }
};

static unsigned g_speedLogN = 0;

// ⑤ 攻速·近战后备：MeleeCheck 每次挥砍后把「冷却」写进 <timer>5__2，逐帧减 deltaTime。
static NeonSlots<16> g_meleeSlots;

static bool Hooked_MeleeCheckMoveNext(void* self, void* method) {
    bool r = g_origMeleeCheckMn ? g_origMeleeCheckMn(self, method) : false;
    float mul = AtkMul();
    if (!self || mul <= 1.01f) return r;
    if (!PlayerGunOf(self, nullptr)) return r;

    int idx = g_meleeSlots.Find(self);
    if (idx < 0) return r;

    float* timer = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(self) + OFF_SM_TIMER);
    float t = *timer;
    if (t > g_meleeSlots.tab[idx].prev + 0.0001f && t > 0.0001f && t < 60.f) {
        float nt = t / mul;
        *timer = nt;
        g_meleeSlots.tab[idx].prev = nt;
        if ((++g_speedLogN % 20u) == 1u)
            LOGI("[Neon] 攻速(挥砍CD) x%.2f %.3f->%.3f", mul, t, nt);
    } else {
        g_meleeSlots.tab[idx].prev = t;
    }
    return r;
}

// ⑤ 攻速（主）：枪械蓝图协程 <timer>5__3，初值 = graph.output.FireDelay(60/RPM)，
//     逐帧减 deltaTime，归零即出刀/开枪。缩放新赋值的 timer ＝ 整条节奏 ×mul。
//     玩家门控：[sm+0x50]<neonPlayerState>5__2 非空才算玩家路径。
//     [sm+0x38]＝GunGraphEvents.meleeEvent（官方自己的近战/远程分流条件），
//     第四轮合并成一把「攻速」后仍保留它，仅用于日志区分两种分支。
static NeonSlots<24> g_graphSlots;

static bool Hooked_GraphAttackMoveNext(void* self, void* method) {
    bool r = g_origGraphAttackMn ? g_origGraphAttackMn(self, method) : false;
    if (!self) return r;
#if NEON_DIAG
    {
        static std::atomic<unsigned> dn{0};
        if ((dn.fetch_add(1) % 20u) == 0u) {
            const uintptr_t p = reinterpret_cast<uintptr_t>(self);
            void* graph = *reinterpret_cast<void**>(p + 0x20);   // <>4__this = GunGraph
            void* mev   = *reinterpret_cast<void**>(p + 0x38);   // GGEvents.meleeEvent
            void* ps    = *reinterpret_cast<void**>(p + 0x50);   // <neonPlayerState>5__2
            const float tm = *reinterpret_cast<float*>(p + 0x58); // <timer>5__3
            const int firing = graph ? (int)*reinterpret_cast<unsigned char*>(
                                    reinterpret_cast<uintptr_t>(graph) + 0x90) : -1;
            void* dmp = mev ? *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(mev) + 0x10) : nullptr;
            LOGI("[Neon][diag] d__46 sm=%p state=%d firing=%d meleeEv=%p dMethod=%p ps=%p timer=%.3f atkLvl=%d",
                 self, *reinterpret_cast<int*>(p + 0x10), firing, mev, dmp, ps, tm,
                 g_atkLvl.load());
        }
    }
#endif
    if (!*reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) + OFF_SM_NEON_PS)) return r;

    const bool isMelee = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) +
                                                   OFF_SM_MELEE_EVENT) != nullptr;

    const float mul = AtkMul();
    if (mul <= 1.01f) {
        // 诊断：滑条没开时也记一笔，用来确认近战武器是否真走枪图
        static std::atomic<unsigned> diag{0};
        if (isMelee && (diag.fetch_add(1) % 200u) == 0u)
            LOGI("[Neon] 枪图近战分支命中(未加速) atkLvl=%d", g_atkLvl.load());
        return r;
    }

    int idx = g_graphSlots.Find(self);
    if (idx < 0) return r;

    float* timer = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(self) + OFF_SM_GRAPH_TIMER);
    float t = *timer;
    if (t > g_graphSlots.tab[idx].prev + 0.0001f && t > 0.0001f && t < 60.f) {
        float nt = t / mul;
        *timer = nt;
        g_graphSlots.tab[idx].prev = nt;
        if ((++g_speedLogN % 20u) == 1u) {
            if (isMelee)
                LOGI("[Neon] 攻速(枪图·近战分支) x%.2f 挥砍间隔 %.3f->%.3f", mul, t, nt);
            else
                LOGI("[Neon] 攻速(枪图) x%.2f 间隔 %.3f->%.3f", mul, t, nt);
        }
    } else {
        g_graphSlots.tab[idx].prev = t;
    }
    return r;
}

// ⑤b 攻速（后备）：shootingInterval 唯一 yield = WaitForSeconds(60/RPM)。
//     实测该协程在玩家武器上未被启动，留着以防个别武器走这条老路。
static bool Hooked_ShootIntervalMoveNext(void* self, void* method) {
    bool r = g_origShootIntervalMn ? g_origShootIntervalMn(self, method) : false;
    float mul = AtkMul();
    if (!r || !self || mul <= 1.01f) return r;
    if (!PlayerGunOf(self, nullptr)) return r;

    void* cur = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) + OFF_SM_CURRENT);
    if (!cur) return r;
    float* sec = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(cur) + OFF_WFS_SECONDS);
    float v = *sec;
    if (v > 0.0001f && v < 60.f) {
        *sec = v / mul;
        if ((++g_speedLogN % 20u) == 1u)
            LOGI("[Neon] 攻速(协程) x%.2f 间隔 %.3f->%.3f", mul, v, v / mul);
    }
    return r;
}

// ⑤c 攻速（主·2026-10-02 真机定位）：玩家枪械实际走「无事件」版
//     GunGraph.<AttackRoutine>d__45::MoveNext(0x292AE4C)，而不是 d__46。
//     依据：GunOutput::NeedCharge/Attack、MuzzleFlash::Fire 的调用点全部落在
//     d__45/d__46/d__47 内；而 GunGraph.meleeDelegate::Invoke 全场只有 1 个调用点
//       0x292B3C8＝d__46 内部 ⇒ 近战必走 d__46，d__45 只能是枪械（远程）。
//     d__45 没有 timer 字段，它的节奏是 `yield return new WaitForSeconds(FireDelay)`：
//     反汇编 0x292AFB0 get_FireDelay → 0x5249C6C WaitForSeconds::.ctor → 存 <>2__current(0x18)。
//     做法：照跑 MoveNext，再把它刚 yield 出来的 WaitForSeconds.m_Seconds ÷ mul。
//     只认「玩家自己这把枪」：GunGraphBase.netPlayer(0x28) 非空且非 isActorShadow(0x68)。
static bool IsWaitForSecondsObj(void* obj) {
    if (!obj) return false;
    void* klass = *reinterpret_cast<void**>(obj);   // Il2CppObject.klass
    if (!klass) return false;
    const char* name = *reinterpret_cast<const char**>(
        reinterpret_cast<uintptr_t>(klass) + 0x10);  // Il2CppClass.name
    if (!name) return false;
    return strcmp(name, "WaitForSeconds") == 0 || strcmp(name, "WaitForSecondsRealtime") == 0;
}

// 返回：1 = 玩家枪（np 非空且非影子），0 = np 为空，2 = 影子枪，-1 = graph 为空
static bool IsPlayerGunGraph(void* graph, int* dbg) {
    if (!graph) { if (dbg) *dbg = -1; return false; }
    const uintptr_t g = reinterpret_cast<uintptr_t>(graph);
    void* np = *reinterpret_cast<void**>(g + OFF_GRAPH_NETPLAYER);
    const bool shadow = *reinterpret_cast<uint8_t*>(g + OFF_GRAPH_SHADOW) != 0;
    if (dbg) *dbg = np ? (shadow ? 2 : 1) : 0;
    return np != nullptr && !shadow;
}

static void* g_lastScaledWfs = nullptr;   // 同一个等待对象只缩放一次

static bool Hooked_Graph45AttackMoveNext(void* self, void* method) {
    bool r = g_origGraph45Mn ? g_origGraph45Mn(self, method) : false;
    if (!self) return r;
    const uintptr_t p = reinterpret_cast<uintptr_t>(self);
    void* graph = *reinterpret_cast<void**>(p + OFF_SM45_THIS);
    int dbg = -1;
    const bool playerGun = IsPlayerGunGraph(graph, &dbg);
    // ⑫ 玩家攻击时间窗：玩家自己这把枪的攻击协程在跑 ⇒ 记一次时间戳（全屏攻击用它判「玩家打出来的伤害」）
    if (playerGun) {
        g_lastPlayerAtkMs.store(NeonNowMs());
        // GunGraph.LatestInstigator(+0xA0) = 这一枪的 instigator（就是 DamageInfo+0x70 的来源）
        if (graph) {
            void* inst = *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(graph) + OFF_GRAPH_LATEST_INST);
            if (inst) NeonRememberPlayerInstigator(inst);
        }
    }
    const float mul = AtkMul();
#if NEON_DIAG
    {
        static std::atomic<unsigned> dn{0};
        if ((dn.fetch_add(1) % 60u) == 0u) {
            void* cur0 = *reinterpret_cast<void**>(p + OFF_SM45_CURRENT);
            const uintptr_t g = reinterpret_cast<uintptr_t>(graph);
            LOGI("[Neon][diag] d__45 sm=%p state=%d graph=%p gate=%d(1=玩家枪) output=%p firing=%d cur=%p wfs=%d mul=%.2f",
                 self, *reinterpret_cast<int*>(p + 0x10), graph, dbg,
                 graph ? *reinterpret_cast<void**>(g + OFF_GRAPH_OUTPUT) : nullptr,
                 graph ? (int)*reinterpret_cast<uint8_t*>(g + OFF_GRAPH_FIRING) : -1,
                 cur0, IsWaitForSecondsObj(cur0) ? 1 : 0, mul);
        }
    }
#endif
    if (!r || !playerGun || mul <= 1.01f) return r;

    void* cur = *reinterpret_cast<void**>(p + OFF_SM45_CURRENT);
    if (!IsWaitForSecondsObj(cur)) return r;
    if (cur == g_lastScaledWfs) return r;   // 防同一等待对象被反复除
    float* sec = reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(cur) + OFF_WFS_SECONDS);
    const float v = *sec;
    if (v > 0.0001f && v < 60.f) {
        const float nv = v / mul;
        *sec = nv;
        g_lastScaledWfs = cur;
        if ((++g_speedLogN % 20u) == 1u)
            LOGI("[Neon] 攻速(枪图45) x%.2f 间隔 %.3f->%.3f", mul, v, nv);
    }
    return r;
}

// ================================================================
//  ⑦⑧⑨ 对局内资源：金币 / 钥匙 / 彩色钥匙（2026-10-02）
//  只拦「消耗」+ 让「够不够」恒真 ⇒ 用不完；不改数值字段，关掉即恢复
// ================================================================
static void Hooked_UseCoin(void* thiz, int32_t amount, void* method) {
    if (g_infCoin.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u) LOGI("[Neon] 金币使用不减: UseCoin(%d) 已拦截", amount);
        return;
    }
    if (g_origUseCoin) g_origUseCoin(thiz, amount, method);
}

static bool Hooked_HasCoin(void* thiz, int32_t amount, void* method) {
    if (g_infCoin.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u) LOGI("[Neon] 金币使用不减: HasCoin(%d) -> true", amount);
        return true;
    }
    return g_origHasCoin ? g_origHasCoin(thiz, amount, method) : false;
}

static int32_t Hooked_GetCoinNum(void* thiz, void* method) {
    if (g_infCoin.load()) return 99999;
    return g_origGetCoin ? g_origGetCoin(thiz, method) : 0;
}

static void Hooked_UseKey(void* thiz, void* method) {
    if (g_infKey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 20u) == 0u) LOGI("[Neon] 钥匙使用不减: UseKey() 已拦截");
        return;
    }
    if (g_origUseKey) g_origUseKey(thiz, method);
}

static bool Hooked_HasKey0(void* thiz, void* method) {
    if (g_infKey.load()) return true;
    return g_origHasKey0 ? g_origHasKey0(thiz, method) : false;
}

static bool Hooked_HasKeyN(void* thiz, int32_t needCount, void* method) {
    if (g_infKey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u)
            LOGI("[Neon] 钥匙使用不减: HasKey(%d) -> true", needCount);
        return true;
    }
    return g_origHasKeyN ? g_origHasKeyN(thiz, needCount, method) : false;
}

static void Hooked_UseGKey(void* thiz, void* method) {
    if (g_infGkey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 20u) == 0u) LOGI("[Neon] 彩色钥匙使用不减: UseGameplayKey() 已拦截");
        return;
    }
    if (g_origUseGKey) g_origUseGKey(thiz, method);
}

static bool Hooked_HasGKey(void* thiz, void* method) {
    if (g_infGkey.load()) {
        static std::atomic<unsigned> n{0};
        if ((n.fetch_add(1) % 40u) == 0u) LOGI("[Neon] 彩色钥匙使用不减: HasGameplayKey() -> true");
        return true;
    }
    return g_origHasGKey ? g_origHasGKey(thiz, method) : false;
}

#if NEON_DIAG
// ===================== 临时诊断（NEON_DIAG）BEGIN =====================
// 只有观测、不改行为：全部 log-then-forward。收尾整块删除。
#define RVA_DIAG_MF_FIRE      0x462BBE0UL  // MuzzleFlash::Fire()              开枪视觉
#define RVA_DIAG_NEED_CHARGE  0x3A933F8UL  // GunOutput::NeedCharge()           枪是否蓄力
#define RVA_DIAG_GUN_MELEE    0x3961F28UL  // Gun::Melee()                      近战旧路径
#define RVA_DIAG_KNIFE_JUDGE  0x3960208UL  // Gun::JudgeCanKnifeAttack()        近战判定
#define RVA_DIAG_MELEE_INVOKE 0x292B4E0UL  // GunGraph.meleeDelegate::Invoke()  枪图内近战委托

// 第三轮加的近战真身候选（set_BusyTime 0x3DA9D70 已在第四轮转为正式缩放钩，见 Hooked_SetBusyTime）
#define RVA_DIAG_MSEG_GETBUSY 0x3DA9D18UL  // DeputyWeaponSlot_MeleeMultiSeg::get_BusyTime()
#define RVA_DIAG_MSEG_UPDATE  0x3DAB76CUL  // DeputyWeaponSlot_MeleeMultiSeg::OnUpdate()
#define RVA_DIAG_MSEG_TRIGGER 0x3DAC248UL  // DeputyWeaponSlot_MeleeMultiSeg::TriggerShoot(PlayerActor)
#define RVA_DIAG_NETGUN_MELEE 0x36AE1DCUL  // NetGun::MeleeLauncherAttack_NetPlayer(NetPlayerActor)
#define RVA_DIAG_GUN_LAUNCH   0x39625B0UL  // Gun::MeleeLauncherAttack()
#define RVA_DIAG_HERO_ANIM    0x2EFA614UL  // HeroAnimationEvents::MeleeLauncherAttack()
#define RVA_DIAG_CAPMELEE_ATK 0x3BE2B84UL  // CapabilityControl_Melee::Attack(CapabilityMeleeContext)
#define RVA_DIAG_CAPMELEE_HIT 0x3BE87BCUL  // CapabilityControl_MeleeAgent::SwordHitEvent()
#define RVA_DIAG_OUT_ATTACK   0x3A93520UL  // GunOutput::Attack(Transform,MonoBehaviour,PayLoadExtraInfo)

typedef void  (*DiagFire_t)(void*, void*);
typedef bool  (*DiagBool_t)(void*, void*);
typedef void* (*DiagInvoke_t)(void*, void*);
typedef void  (*DiagSetF_t)(void*, float, void*);
typedef float (*DiagRetF_t)(void*, void*);
typedef void  (*DiagVoid2_t)(void*, void*, void*);
typedef void* (*DiagAtt4_t)(void*, void*, void*, void*, void*);

static DiagFire_t   g_origDiagFire = nullptr;
static DiagBool_t   g_origDiagCharge = nullptr;
static DiagBool_t   g_origDiagGunMelee = nullptr;
static DiagBool_t   g_origDiagKnife = nullptr;
static DiagInvoke_t g_origDiagMeleeInvoke = nullptr;
static DiagRetF_t   g_origDiagMGetBusy = nullptr;
static DiagFire_t   g_origDiagMUpdate = nullptr;
static DiagVoid2_t  g_origDiagMTrigger = nullptr;
static DiagVoid2_t  g_origDiagNetGunMelee = nullptr;
static DiagFire_t   g_origDiagGunLaunch = nullptr;
static DiagFire_t   g_origDiagHeroAnim = nullptr;
static DiagVoid2_t  g_origDiagCapAtk = nullptr;
static DiagFire_t   g_origDiagCapHit = nullptr;
static DiagAtt4_t   g_origDiagOutAttack = nullptr;

static void Hooked_DiagFire(void* thiz, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u)
        LOGI("[Neon][diag] 开枪路 MuzzleFlash.Fire() thiz=%p", thiz);
    if (g_origDiagFire) g_origDiagFire(thiz, method);
}
static bool Hooked_DiagCharge(void* thiz, void* method) {
    bool r = g_origDiagCharge ? g_origDiagCharge(thiz, method) : false;
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 8u) == 0u)
        LOGI("[Neon][diag] 枪路 NeedCharge()=%d", (int)r);
    return r;
}
static bool Hooked_DiagGunMelee(void* thiz, void* method) {
    bool r = g_origDiagGunMelee ? g_origDiagGunMelee(thiz, method) : false;
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 2u) == 0u)
        LOGI("[Neon][diag] 近战路 Gun.Melee()=%d thiz=%p", (int)r, thiz);
    return r;
}
static bool Hooked_DiagKnife(void* thiz, void* method) {
    bool r = g_origDiagKnife ? g_origDiagKnife(thiz, method) : false;
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 8u) == 0u)
        LOGI("[Neon][diag] 近战判定 JudgeCanKnifeAttack()=%d", (int)r);
    return r;
}
static void* Hooked_DiagMeleeInvoke(void* dlg, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 2u) == 0u) {
        void* mp = dlg ? *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(dlg) + 0x10) : nullptr;
        void* tg = dlg ? *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(dlg) + 0x20) : nullptr;
        LOGI("[Neon][diag] 近战委托 Invoke dlg=%p method_ptr=%p target=%p", dlg, mp, tg);
    }
    return g_origDiagMeleeInvoke ? g_origDiagMeleeInvoke(dlg, method) : nullptr;
}

// ---- 第三轮：近战真身候选（全部 log-then-forward，不改行为）----
// （原 set_BusyTime 探针已升级为正式缩放钩 Hooked_SetBusyTime，此处只留只读探针）
static float Hooked_DiagMGetBusy(void* thiz, void* method) {
    float r = g_origDiagMGetBusy ? g_origDiagMGetBusy(thiz, method) : 0.f;
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 240u) == 0u)
        LOGI("[Neon][diag] 副武器槽 get_BusyTime()=%.4f thiz=%p", r, thiz);
    return r;
}
static void Hooked_DiagMUpdate(void* thiz, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 600u) == 0u)
        LOGI("[Neon][diag] 副武器槽 OnUpdate thiz=%p", thiz);
    if (g_origDiagMUpdate) g_origDiagMUpdate(thiz, method);
}
static void Hooked_DiagMTrigger(void* thiz, void* pla, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u) {
        LOGI("[Neon][diag] 副武器槽 TriggerShoot thiz=%p player=%p", thiz, pla);
        NeonStack st; NeonCapture(&st); NeonLogStack("meleeTrigger", &st);
    }
    if (g_origDiagMTrigger) g_origDiagMTrigger(thiz, pla, method);
}
static void Hooked_DiagNetGunMelee(void* thiz, void* npa, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u) {
        LOGI("[Neon][diag] NetGun.MeleeLauncherAttack_NetPlayer thiz=%p npa=%p", thiz, npa);
        NeonStack st; NeonCapture(&st); NeonLogStack("netGunMelee", &st);
    }
    if (g_origDiagNetGunMelee) g_origDiagNetGunMelee(thiz, npa, method);
}
static void Hooked_DiagGunLaunch(void* thiz, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u)
        LOGI("[Neon][diag] Gun.MeleeLauncherAttack thiz=%p", thiz);
    if (g_origDiagGunLaunch) g_origDiagGunLaunch(thiz, method);
}
static void Hooked_DiagHeroAnim(void* thiz, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u) {
        LOGI("[Neon][diag] HeroAnimationEvents.MeleeLauncherAttack thiz=%p", thiz);
        NeonStack st; NeonCapture(&st); NeonLogStack("heroAnimMelee", &st);
    }
    if (g_origDiagHeroAnim) g_origDiagHeroAnim(thiz, method);
}
static void Hooked_DiagCapAtk(void* thiz, void* ctx, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u) {
        LOGI("[Neon][diag] CapabilityControl_Melee.Attack ctx=%p", ctx);
        NeonStack st; NeonCapture(&st); NeonLogStack("capMeleeAtk", &st);
    }
    if (g_origDiagCapAtk) g_origDiagCapAtk(thiz, ctx, method);
}
static void Hooked_DiagCapHit(void* thiz, void* method) {
    static std::atomic<unsigned> n{0};
    if ((n.fetch_add(1) % 4u) == 0u)
        LOGI("[Neon][diag] MeleeAgent.SwordHitEvent thiz=%p", thiz);
    if (g_origDiagCapHit) g_origDiagCapHit(thiz, method);
}
static void* Hooked_DiagOutAttack(void* thiz, void* tr, void* mb, void* payload, void* method) {
    static std::atomic<unsigned> n{0};
    unsigned k = n.fetch_add(1);
    if (k < 40u && (k % 2u) == 0u) {
        LOGI("[Neon][diag] GunOutput.Attack thiz=%p", thiz);
        NeonStack st; NeonCapture(&st); NeonLogStack("outAttack", &st);
    }
    return g_origDiagOutAttack ? g_origDiagOutAttack(thiz, tr, mb, payload, method) : nullptr;
}
static void InstallDiagHooks() {
    HookOneQ(RVA_DIAG_MF_FIRE,      (void*)Hooked_DiagFire,        (void**)&g_origDiagFire);
    HookOneQ(RVA_DIAG_NEED_CHARGE,  (void*)Hooked_DiagCharge,      (void**)&g_origDiagCharge);
    HookOneQ(RVA_DIAG_GUN_MELEE,    (void*)Hooked_DiagGunMelee,    (void**)&g_origDiagGunMelee);
    HookOneQ(RVA_DIAG_KNIFE_JUDGE,  (void*)Hooked_DiagKnife,       (void**)&g_origDiagKnife);
    HookOneQ(RVA_DIAG_MELEE_INVOKE, (void*)Hooked_DiagMeleeInvoke, (void**)&g_origDiagMeleeInvoke);

    // 第三轮：近战真身候选（多段近战副武器槽 + 发射器/动画事件 + 近战能力控制）
    // （set_BusyTime 已转为正式缩放钩，见 Hooked_SetBusyTime，不再重复挂诊断）
    HookOneQ(RVA_DIAG_MSEG_GETBUSY,  (void*)Hooked_DiagMGetBusy,    (void**)&g_origDiagMGetBusy);
    HookOneQ(RVA_DIAG_MSEG_UPDATE,   (void*)Hooked_DiagMUpdate,     (void**)&g_origDiagMUpdate);
    HookOneQ(RVA_DIAG_MSEG_TRIGGER,  (void*)Hooked_DiagMTrigger,    (void**)&g_origDiagMTrigger);
    HookOneQ(RVA_DIAG_NETGUN_MELEE,  (void*)Hooked_DiagNetGunMelee, (void**)&g_origDiagNetGunMelee);
    HookOneQ(RVA_DIAG_GUN_LAUNCH,    (void*)Hooked_DiagGunLaunch,   (void**)&g_origDiagGunLaunch);
    HookOneQ(RVA_DIAG_HERO_ANIM,     (void*)Hooked_DiagHeroAnim,    (void**)&g_origDiagHeroAnim);
    HookOneQ(RVA_DIAG_CAPMELEE_ATK,  (void*)Hooked_DiagCapAtk,      (void**)&g_origDiagCapAtk);
    HookOneQ(RVA_DIAG_CAPMELEE_HIT,  (void*)Hooked_DiagCapHit,      (void**)&g_origDiagCapHit);
    HookOneQ(RVA_DIAG_OUT_ATTACK,    (void*)Hooked_DiagOutAttack,   (void**)&g_origDiagOutAttack);

    LOGI("[Neon][diag] diag hooks fire=%d charge=%d gunMelee=%d knife=%d invoke=%d "
         "| mgetbusy=%d mupdate=%d mtrigger=%d netgun=%d launch=%d hero=%d "
         "capAtk=%d capHit=%d outAtk=%d",
         g_origDiagFire ? 1 : 0, g_origDiagCharge ? 1 : 0, g_origDiagGunMelee ? 1 : 0,
         g_origDiagKnife ? 1 : 0, g_origDiagMeleeInvoke ? 1 : 0,
         g_origDiagMGetBusy ? 1 : 0, g_origDiagMUpdate ? 1 : 0,
         g_origDiagMTrigger ? 1 : 0, g_origDiagNetGunMelee ? 1 : 0, g_origDiagGunLaunch ? 1 : 0,
         g_origDiagHeroAnim ? 1 : 0, g_origDiagCapAtk ? 1 : 0, g_origDiagCapHit ? 1 : 0,
         g_origDiagOutAttack ? 1 : 0);
}
// ====================== 临时诊断（NEON_DIAG）END ======================
#endif

bool Neon_TryInit(JavaVM* vm) {
    (void)vm;
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(NEON_LIB)) return false;

    LOGI("[Neon] init combat-only (3.2.860)");
    g_buildPassBase = reinterpret_cast<BuildPass_t>(Abs(RVA_BUILD_PASS_BASE));

    // libil2cpp 基址（栈回溯换算用）：由已能解析的固定 RVA 反推，不依赖 getAbsoluteAddress 语义
    {
        void* anchor = Abs(RVA_GRAPH_ATTACK_MN);
        if (anchor) {
            g_libBase = reinterpret_cast<uintptr_t>(anchor) - RVA_GRAPH_ATTACK_MN;
            g_libEnd  = g_libBase + 0x8000000UL;   // 临时值，下面立刻用 maps 实测值覆盖
            LOGI("[Neon] libil2cpp base=0x%lx", (unsigned long)g_libBase);
            NeonRefreshLibRangeFromMaps();         // ★实测 [min start, max end) 覆盖上界
        }
    }

    if (!HookOne("HitPoints.DeductHP", RVA_HITPOINTS_DEDUCT,
                 (void*)Hooked_HitPoints_DeductHP, (void**)&g_origHitDeduct))
        return false;
    HookOne("HitPoints.FixDeduct", RVA_HITPOINTS_FIX,
            (void*)Hooked_HitPoints_FixDeduct, (void**)&g_origHitFix);
    HookOne("HitPoints.DoDeduct", RVA_HITPOINTS_DO,
            (void*)Hooked_HitPoints_DoDeduct, (void**)&g_origDoDeduct);
    if (!HookOne("Hearts.DeductHP", RVA_HEARTS_DEDUCT,
                 (void*)Hooked_Hearts_DeductHP, (void**)&g_origHeartDeduct))
        return false;
    if (!HookOne("BuildPassCheckInfo", RVA_BUILD_PASS,
                 (void*)Hooked_BuildPassCheckInfo, (void**)&g_origBuildPass))
        return false;

    HookOne("IsSkillCD", RVA_IS_SKILL_CD, (void*)Hooked_IsSkillCD, (void**)&g_origIsSkillCD);
    HookOne("SetSkillCD", RVA_SET_SKILL_CD, (void*)Hooked_SetSkillCD, (void**)&g_origSetSkillCD);
    HookOne("GetRemainCD", RVA_GET_REMAIN_CD, (void*)Hooked_GetRemainCD, (void**)&g_origGetRemainCD);
    HookOne("SecCoolTime", RVA_SEC_COOL_TIME, (void*)Hooked_SecCoolTime, (void**)&g_origSecCool);
    HookOne("SecCurCool", RVA_SEC_CUR_COOL, (void*)Hooked_SecCurCool, (void**)&g_origSecCurCool);
    HookOne("SecSkillBegin(probe)", RVA_SEC_SKILL_BEGIN, (void*)Hooked_SkillBegin, (void**)&g_origSkillBegin);
    HookOne("SecSkillEnd(probe)", RVA_SEC_SKILL_END, (void*)Hooked_SkillEnd, (void**)&g_origSkillEnd);

    // 技能门：35 处 CanUseSkill 全挂（旧版只挂基类那一处，被 override 绕过）
    int gateOk = InstallSkillGates(std::make_index_sequence<NEON_SKILL_GATE_COUNT>{});
    LOGI("[Neon] skill gates hooked %d/%d", gateOk, (int)NEON_SKILL_GATE_COUNT);

    HookOne("Dict_MoveSpeed", RVA_DICT_MOVE_SPEED, (void*)Hooked_DictMoveSpeed, (void**)&g_origDictMove);
    HookOne("PlayerSpeedMf", RVA_PLAYER_SPEED_MF, (void*)Hooked_PlayerSpeedMf, (void**)&g_origPlayerSpeedMf);
    HookOne("CompMoveSpeed", RVA_COMP_MOVE_SPEED, (void*)Hooked_CompMoveSpeed, (void**)&g_origCompMove);
    HookOne("GetMaxSpeed", RVA_GET_MAX_SPEED, (void*)Hooked_GetMaxSpeed, (void**)&g_origMaxSpeed);

#ifdef NEON_FULL_MENU
    // 攻速（定制版独有）
    HookOne("GunGraph.AttackRoutineWithEvents.MoveNext", RVA_GRAPH_ATTACK_MN,
            (void*)Hooked_GraphAttackMoveNext, (void**)&g_origGraphAttackMn);
    HookOne("GunGraph.AttackRoutine.MoveNext", RVA_GRAPH45_ATTACK_MN,
            (void*)Hooked_Graph45AttackMoveNext, (void**)&g_origGraph45Mn);
    HookOne("Gun.shootingInterval.MoveNext", RVA_SHOOT_INTERVAL_MN,
            (void*)Hooked_ShootIntervalMoveNext, (void**)&g_origShootIntervalMn);
    HookOne("Gun.MeleeCheck.MoveNext", RVA_MELEE_CHECK_MN,
            (void*)Hooked_MeleeCheckMoveNext, (void**)&g_origMeleeCheckMn);
#endif

    // 对局内资源（挂不上不影响战斗项）
    HookOne("NEONPlayerState.UseCoin", RVA_PS_USE_COIN, (void*)Hooked_UseCoin, (void**)&g_origUseCoin);
    HookOne("NEONPlayerState.HasCoin", RVA_PS_HAS_COIN, (void*)Hooked_HasCoin, (void**)&g_origHasCoin);
    HookOne("NEONPlayerState.GetCoinNum", RVA_PS_GET_COIN, (void*)Hooked_GetCoinNum, (void**)&g_origGetCoin);
    HookOne("NEONPlayerState.UseKey", RVA_PS_USE_KEY, (void*)Hooked_UseKey, (void**)&g_origUseKey);
    HookOne("NEONPlayerState.HasKey", RVA_PS_HAS_KEY, (void*)Hooked_HasKey0, (void**)&g_origHasKey0);
    HookOne("NEONPlayerState.HasKey(n)", RVA_PS_HAS_KEY_N, (void*)Hooked_HasKeyN, (void**)&g_origHasKeyN);
    HookOne("NEONPlayerState.UseGameplayKey", RVA_PS_USE_GKEY, (void*)Hooked_UseGKey, (void**)&g_origUseGKey);
    HookOne("NEONPlayerState.HasGameplayKey", RVA_PS_HAS_GKEY, (void*)Hooked_HasGKey, (void**)&g_origHasGKey);

#ifdef NEON_FULL_MENU
    // 攻速真身（定制版独有）
    HookOne("GunOutput.FireDelay", RVA_OUT_FIRE_DELAY, (void*)Hooked_GetFireDelay, (void**)&g_origFireDelay);
    HookOne("MeleeMultiSeg.AsBusyTime", RVA_MELEE_AS_BUSY, (void*)Hooked_MeleeAsBusyTime, (void**)&g_origMeleeAsBusy);
    HookOne("MeleeMultiSeg.SetBusyTime", RVA_MELEE_SET_BUSY, (void*)Hooked_SetBusyTime, (void**)&g_origSetBusy);
#endif

    // ＝＝ 第六轮新增 ＝＝
    //   ⑨ 无限跳跃：替换 PlayerActor.get_multipleJumpCount
    //   ⑩ 伤害倍率：复用上面的 HitPoints.DeductHP 钩子，无需新钩
    //   ⑪ 丝血无敌：Hearts 血量读写只直调，不进 Dobby
    HookOne("PlayerActor.get_multipleJumpCount", RVA_MULTI_JUMP_COUNT,
            (void*)Hooked_GetMultipleJumpCount, (void**)&g_origGetJumpCount);
    g_fnGetCurHearts = reinterpret_cast<GetCurHearts_t>(Abs(RVA_HEARTS_GET_CURRENT));
    g_fnSetCurHearts = reinterpret_cast<SetCurHearts_t>(Abs(RVA_HEARTS_SET_CURRENT));
    LOGI("[Neon] round6 jump=%d heartGet=%d heartSet=%d",
         g_origGetJumpCount ? 1 : 0, g_fnGetCurHearts ? 1 : 0, g_fnSetCurHearts ? 1 : 0);

    // ＝＝ 第六轮补丁（2026-10-02 晚）：⑨ 门槛链补齐 + ⑪ 改成在扣血叶函数截断伤害 ＝＝
    HookOne("PlayerStateInt.op_Implicit", RVA_OP_IMPLICIT, (void*)Hooked_PlayerStateInt_OpImplicit, (void**)&g_origOpImplicit);
    HookOne("PlayerActorBase.get_canNotJump", RVA_CAN_NOT_JUMP, (void*)Hooked_GetCanNotJump, (void**)&g_origCanNotJump);
    HookOne("BaseMechaControl.CanMultipleJump", RVA_CAN_MULTI_JUMP, (void*)Hooked_CanMultipleJump, (void**)&g_origCanMultiJump);
#ifdef NEON_FULL_MENU
    // 丝血无敌（定制版独有）：扣血叶函数三处 + 死亡入口兜底
    HookOne("Hearts.DeductHeartFirst", RVA_HEART_FIRST, (void*)Hooked_HeartFirst, (void**)&g_origHeartFirst);
    HookOne("Hearts.DeductArmorFirst", RVA_ARMOR_FIRST, (void*)Hooked_ArmorFirst, (void**)&g_origArmorFirst);
    HookOne("Hearts.DeductHeart_ApplyApportion", RVA_HEART_APPORTION, (void*)Hooked_HeartApportion, (void**)&g_origApportion);
    HookOne("Hearts.ForceDie", RVA_FORCE_DIE, (void*)Hooked_Hearts_ForceDie, (void**)&g_origForceDie);
#endif
    g_fnGetArmor = reinterpret_cast<IntFn_t>(Abs(RVA_HEARTS_GET_ARMOR));
    g_fnGetSane  = reinterpret_cast<IntFn_t>(Abs(RVA_HEARTS_GET_SANE));
    LOGI("[Neon] round6b jumpChain=%d/%d/%d heartChain=%d/%d/%d die=%d armor=%d sane=%d",
         g_origOpImplicit ? 1 : 0, g_origCanNotJump ? 1 : 0, g_origCanMultiJump ? 1 : 0,
         g_origHeartFirst ? 1 : 0, g_origArmorFirst ? 1 : 0, g_origApportion ? 1 : 0,
         g_origForceDie ? 1 : 0, g_fnGetArmor ? 1 : 0, g_fnGetSane ? 1 : 0);

#ifdef NEON_FULL_MENU
    // 全屏攻击（定制版独有）：玩家类型槽 + 房间捕获七钩子
    g_fnRoomGet      = reinterpret_cast<RoomGet_t>(Abs(RVA_ROOM_GET));
    g_fnEhiGetRoom   = reinterpret_cast<RoomGet_t>(Abs(RVA_EHI_GET_ROOM));
    if (g_libBase) {
        g_playerKlass    = *reinterpret_cast<void**>(g_libBase + RVA_SLOT_PLAYER_KLASS);
        g_netPlayerKlass = *reinterpret_cast<void**>(g_libBase + RVA_SLOT_NETPLAYER_KLASS);
    }
    LOGI("[Neon][fs] 玩家类型槽 player=%p netPlayer=%p", g_playerKlass, g_netPlayerKlass);
    HookOne("YiHai.OnEnterRoom(RoomMB)", RVA_ROOM_ENTER,
            (void*)Hooked_OnEnterRoom, (void**)&g_origRoomEnter);
    HookOne("YiHai.OnLeaveRoom(RoomMB)", RVA_ROOM_LEAVE,
            (void*)Hooked_OnLeaveRoom, (void**)&g_origRoomLeave);
    HookOne("YiHai.OnEnterBattle(RoomMB)", RVA_ROOM_BATTLE,
            (void*)Hooked_OnEnterBattle, (void**)&g_origRoomBattle);
    HookOne("RoomMB.MonsterListCleanup", RVA_ROOM_CLEANUP,
            (void*)Hooked_MonsterListCleanup, (void**)&g_origRoomCleanup);
    HookOne("MapManager.LateUpdate", RVA_MAP_LATE_UPDATE,
            (void*)Hooked_MapLateUpdate, (void**)&g_origMapLate);
    HookOne("MapManager.RefreshRoom(RoomMB)", RVA_MAP_REFRESH_ROOM,
            (void*)Hooked_MapRefreshRoom, (void**)&g_origMapRefresh);
    HookOne("RoomMB.get_ActiveMonsterCount", RVA_ROOM_ACTIVE_CNT,
            (void*)Hooked_ActiveMonsterCount, (void**)&g_origRoomActiveCnt);
    LOGI("[Neon] round7 roomHooks enter=%d leave=%d battle=%d cleanup=%d activeCnt=%d",
         g_origRoomEnter ? 1 : 0, g_origRoomLeave ? 1 : 0, g_origRoomBattle ? 1 : 0,
         g_origRoomCleanup ? 1 : 0, g_origRoomActiveCnt ? 1 : 0);
#endif

#if NEON_DIAG
    InstallDiagHooks();
#endif

    g_ready.store(true);
    LOGI("[Neon] init ok combat cd=%d move=%d ranged=%d rangedgraph=%d graph45=%d melee=%d coin=%d key=%d gkey=%d firedelay=%d meleeBusy=%d setbusy=%d",
         g_origIsSkillCD ? 1 : 0, g_origDictMove ? 1 : 0,
         g_origShootIntervalMn ? 1 : 0, g_origGraphAttackMn ? 1 : 0, g_origGraph45Mn ? 1 : 0,
         g_origMeleeCheckMn ? 1 : 0,
         g_origUseCoin ? 1 : 0, g_origUseKey ? 1 : 0, g_origUseGKey ? 1 : 0,
         g_origFireDelay ? 1 : 0, g_origMeleeAsBusy ? 1 : 0, g_origSetBusy ? 1 : 0);
    return true;
}

bool Neon_IsReady() { return g_ready.load(); }

void Neon_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case NEON_FEAT_INVINCIBLE:
            g_invincible.store(enabled);
            LOGI("[Neon] 无敌=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_SECKILL:
            g_seckill.store(enabled);
            LOGI("[Neon] 秒杀=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_NO_CD:
            g_noCd.store(enabled);
            LOGI("[Neon] 技能无CD=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_MOVE_SPEED:
            if (!enabled) g_moveLvl.store(1);
            break;
#ifdef NEON_FULL_MENU
        case NEON_FEAT_ATTACK_SPEED:
            if (!enabled) g_atkLvl.store(1);
            break;
#endif
        case NEON_FEAT_INF_COIN:
            g_infCoin.store(enabled);
            LOGI("[Neon] 金币使用不减=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_INF_KEY:
            g_infKey.store(enabled);
            LOGI("[Neon] 钥匙使用不减=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_INF_GKEY:
            g_infGkey.store(enabled);
            LOGI("[Neon] 彩色钥匙使用不减=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_INF_JUMP:
            g_noJumpLimit.store(enabled);
            LOGI("[Neon] 无限跳跃=%s", enabled ? "ON" : "OFF");
            break;
#ifdef NEON_FULL_MENU
        case NEON_FEAT_LAST_HEART:
            g_lastHeart.store(enabled);
            LOGI("[Neon] 丝血无敌=%s", enabled ? "ON" : "OFF");
            break;
        case NEON_FEAT_FULLSCREEN_ATK:
            g_fullAtk.store(enabled);
            LOGI("[Neon] 全屏攻击=%s", enabled ? "ON" : "OFF");
            break;
#endif
        default:
            break;
    }
}

static int ClampLvl(int v) {
    if (v < 1) return 1;
    if (v > 5) return 5;
    return v;
}

// ⑩ 伤害倍率单独限幅 1..50（不复用 ClampLvl 的 1..5）
static int DmgClamp(int v) {
    if (v < 1) return 1;
    if (v > 50) return 50;
    return v;
}

void Neon_OnValueChanged(int featNum, int value) {
    if (featNum == NEON_FEAT_MOVE_SPEED) {
        g_moveLvl.store(ClampLvl(value));
        LOGI("[Neon] 移速 lvl=%d mul=%.1f", ClampLvl(value), MoveMul());
        return;
    }
#ifdef NEON_FULL_MENU
    if (featNum == NEON_FEAT_ATTACK_SPEED) {
        int lvl = ClampLvl(value);
        g_atkLvl.store(lvl);
        LOGI("[Neon] 攻速 lvl=%d mul=%.1f", lvl, AtkMul());
        return;
    }
    if (featNum == NEON_FEAT_DAMAGE_MUL) {
        int d = DmgClamp(value);
        g_dmgMul.store(d);
        LOGI("[Neon] 伤害倍率 x%d", d);
        return;
    }
#endif
}

void Neon_ForceClear() {
    g_invincible.store(false);
    g_seckill.store(false);
    g_noCd.store(false);
    g_moveLvl.store(1);
    g_atkLvl.store(1);
    g_infCoin.store(false);
    g_infKey.store(false);
    g_infGkey.store(false);
    g_noJumpLimit.store(false);
    g_dmgMul.store(1);
    g_lastHeart.store(false);
    // 第七轮：全屏攻击一并复位
    g_fullAtk.store(false);
    g_lastPlayerAtkMs.store(0);
}

void Neon_Tick(const bool* features, const int* values) {
    if (!features) return;
    g_invincible.store(features[NEON_FEAT_INVINCIBLE]);
    g_seckill.store(features[NEON_FEAT_SECKILL]);
    g_noCd.store(features[NEON_FEAT_NO_CD]);
    g_infCoin.store(features[NEON_FEAT_INF_COIN]);
    g_infKey.store(features[NEON_FEAT_INF_KEY]);
    g_infGkey.store(features[NEON_FEAT_INF_GKEY]);
    g_noJumpLimit.store(features[NEON_FEAT_INF_JUMP]);
#ifdef NEON_FULL_MENU
    g_lastHeart.store(features[NEON_FEAT_LAST_HEART]);
    g_fullAtk.store(features[NEON_FEAT_FULLSCREEN_ATK]);
#endif
    if (values) {
        // 滑块值每轮由框架同步（未动过时 = 下限 1 = 关）
        g_moveLvl.store(ClampLvl(values[NEON_FEAT_MOVE_SPEED]));
#ifdef NEON_FULL_MENU
        g_atkLvl.store(ClampLvl(values[NEON_FEAT_ATTACK_SPEED]));
        g_dmgMul.store(DmgClamp(values[NEON_FEAT_DAMAGE_MUL]));
#endif
    }
    if (!g_ready.load()) Neon_TryInit();
    // ★libil2cpp 是按需分段 mmap 的（真机实测 79 段、跨 6 GB；开局只映射了一部分），
    //   初始化那一刻扫到的 [min,max) 会比真实范围小很多 ⇒ klass 判空全部误杀。
    //   实测 20:23 那轮：libRange=0x6dd681a000..0x6f3ad9c000，而 klass 在 0x70a0d24000 段，
    //   于是 MapManager 送来的 RoomMB 被判「非托管」拒掉。⇒ 必须周期性重扫。
    //   节流：前 60 拍（≈12 s）每拍扫，此后每 10 拍（≈2 s）扫一次。
    // maps 周期重扫已删除：托管对象校验不再依赖 libil2cpp 映射范围（见 NeonLooksLikeManaged 注释）。
}
