/* ============================================================================
 *  星光菜单游戏模块 — 绝区零 3.2.0 (vc429) arm64-v8a  [ZzzMod.cpp]
 *
 *  站点全部来自 10_docs/绝区零/星空版站点定位_20261005.md 的实测反汇编：
 *    ① 无敌     file 0x12114318 = LFLFDPIEKIH（3.3.0 站点 RVA 0x12118318，32B 唯一）
 *              池鸳 handler 过滤条件：事件 +0x16(u16)==0x85（伤害事件） 且
 *              +0x38(u32)==玩家 actorId → 不调原函数 ⇒ 本模块同款过滤
 *    ② 伤害倍率 —— 3.3.0 未实现（no-op；属性写回链弃用，见 RealModuleReady 注释）；
 *              唯一调用者 file 0x8238550（先取 x0，再 fmov s0,s8 后 bl）⇒ (self, float)
 *    ③ 移动速度 file 0xfb2700c 函数内 3 条写指令（str s8,[x0,#0xf8] / str s8,[x0,#0x100] /
 *              strb w8,[x0,#0x104]），入口 hook 拿不到 s8 ⇒ 走 12B 原地桩 + 自带 handler
 *              【R20】功能已删（2026-10-07 用户令）：桩与看护全部停装，注释仅备查，禁止复活
 *    ④ 定怪     file 0x1aae0e1c = BehaviorManager.Tick（3.3.0 站点 RVA 0x1aae4e1c，32B 唯一）
 *    ⑤ 无限技能 v1 未定稿（候选 0x1391de04 三簇调用者，扣费点未确证）→ 只记开关，不挂 hook
 *
 *  ⚠️ 地址口径（2026-10-06 修正，MuMu 实战零命中事故）：
 *     站点用【文件偏移】空间；VA 必须取自 loader 真实模块（dl_iterate_phdr）的
 *     PT_LOAD 换算：base + p_vaddr + (fileOff - p_offset)（本例各段 p_vaddr-p_offset=0x4000）；
 *     【教训】MuMu 启动早期 /proc/self/maps 有游戏校验/预读同名临时分块映射（0x77xx 宿主堆区），
 *     按它装钩 = 装上了但永不命中。maps 分段口径仅作回退（真机/稳定期与 phdr 同值）。
 * ============================================================================ */

#include "ZzzMod.h"

#include <atomic>
#include <map>
#include <vector>
#include <string>
#include <thread>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <cerrno>
#include <ctime>
#include <cmath>
#include <time.h>
#include <fcntl.h>
#include <elf.h>
#include <link.h>
#include <mutex>
#include <chrono>
#include <sys/mman.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节）——2026-10-25 修复
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[jql]"
#define TPL_TARGET_LIB sec_lib_il2cpp()
#define TPL_LIB_NAME "libil2cpp.so"

// ============================================================================
//  ① 站点常量（文件偏移空间，禁改——换包必须重 dump 并回填序言校验）
// ============================================================================
enum : uintptr_t {
    // 3.3.0（2026-10-22 重定位）：无敌入口 RVA 0x12118318（7v7 caller 对齐 + frida watch 23/60s）
    kFileOffInvEntry    = 0x12114318,
    // R34 实体 getter 三站（RVA-0x4000）
    kFileOffEntHp       = 0xa046370,   // RVA 0xa04a370 - 0x4000
    kFileOffEntAtk      = 0xa0463c0,   // RVA 0xa04a3c0 - 0x4000
    kFileOffEntCamp     = 0xa046410,   // RVA 0xa04a410 - 0x4000
    // 3.3.0：定怪 = BehaviorManager.Tick，MethodInfo 堆表名字 Tick + 同类 78 方法语义确证
    kFileOffFreezeEntry = 0x1aae0e1c,
    kFileOffMoveWrite   = 0xfb2700c,   // str s8,[x0,#0xf8]
    kFileOffMoveResume  = 0xfb27018,   // 原函数收尾（ldp x20,x19,[sp,#0x30]）
    kFileOffMoveApplyEntry = 0xfb26f14, // SetMoveSpeedRatio.apply entry (R5f hit diag)
    kFileOffActorRoot   = 0x1d749418,  // libil2cpp 尾部 BSS 全局（玩家 actorId 链根）
    // R19（2026-10-07）：R14/R16/R17/R18 的伤害倍率诊断站点（DmgSettle / VicLoopTail /
    //   VicMid）整条移除。MuMu/Houdini 实测卡死栈 = Hook_VicTail -> VicMidDump ->
    //   ReadU32Quiet：热路径上的 0x100 深读 + 故障守卫会把 UnityMain 卡死。
    //   详见 10_docs/绝区零/轮次台账.tsv 的 R18b/R19。
};

// ⚠️ RVA（池鸳/loader 口径）＝ 模块首段映射起点起的偏移；本 so 各段 p_vaddr-p_offset 不同
//    （代码段 0x4000 / 数据段 0x8000 / BSS 段 0xC000）⇒ BSS 全局【不能】套代码段公式。
//    2026-10-06 R3 实证：actor 链根正确 VA = 首段起点 + 0x1d749418；套 phdr 文件偏移公式得 +0xC000 → NULL。
enum : uintptr_t {
    kRvaActorRoot       = 0x1d749418,  // 玩家 actorId 链根（RVA 空间）
    kRvaUltGate         = 0x120faea4,  // 3.3.0：cset w0,ls → mov w0,#1（16B 上下文全 so 唯一）
};
static const uint32_t kUltGateOrig  = 0x1a9f87e0u;   // cset  w0, ls（原字节）
static const uint32_t kUltGatePatch = 0x52800020u;   // mov   w0, #1（池鸳写法，强制返回 true）
// 3.3.0 站点 16B 上下文（cset w0,ls; ldr d8,[sp],#0x30; ret; mov x20,xzr）——全 so 唯一，装前校验
static const uint32_t kUltGateCtx[4] = {0x1a9f87e0u, 0xfc4307e8u, 0xd65f03c0u, 0xaa1f03f4u};

// R39（2026-10-25）：actor id 分带判据（三局实测，见 10_docs/绝区零/伤害结算定位_3.3.0_静态.md §12.2 / §14）
//   玩家侧受击/攻击 actor id：5 / 9 / 11 / 12（≤ 12）
//   怪物侧 actor id：22 / 52 / 56 / 57 / 59 / 60 / 62 / 64 / 65 / 67 / 69 / 72 / 90 / 98 / 103 / 106（≥ 22）
//   中间 13..21 为死区：任一 id 落在死区 ⇒ 不判定（fail-open，宁漏保护不误伤怪物）
static const uint32_t kIdPlayerMax  = 12;
static const uint32_t kIdMonsterMin = 22;

// 序言校验（磁盘文件实测原始字节，u32 小端）
// 3.3.0 无敌入口 32B（uniq16=1/uniq32=1）：stp x30,x21,[sp,#-0x20]! ; stp x20,x19,[sp,#0x10] ;
//   adrp x8,#0x1e542000 ; mov w21,w2 ; mov x19,x1 ; mov x20,x0 ; ldrb w8,[x8,#0xf6f] ; cbnz
static const uint32_t kPrologueInv[8]    = {0xa9be57feu, 0xa9014ff4u, 0xf00621a8u, 0x2a0203f5u,
                                            0xaa0103f3u, 0xaa0003f4u, 0x397dbd08u, 0x35000768u};
static const uint32_t kPrologueMove[3]   = {0xbd00f808u, 0xbd010008u, 0x39041008u};
static const uint32_t kPrologueMoveApply[4] = {0xd10103ffu, 0xfd0013e8u, 0xf90017feu, 0xa9034ff4u};
// 3.3.0 定怪（BehaviorManager.Tick）32B：uniq16=2（不可用）/uniq32=1
static const uint32_t kPrologueFreeze[8] = {0xa9be57feu, 0xa9014ff4u, 0xf001d0e8u, 0xaa0103f4u,
                                            0xaa0003f3u, 0x396b2d08u, 0x35000548u, 0xd001d5d5u};

// R34（2026-10-22）实体访问器三站（可读名 getter，MethodInfo 表定位）：
//   get_hp RVA 0xa04a370 / get_attack 0xa04a3c0 / get_camp 0xa04a410。
//   这三个 getter 的 x0 就是**实体对象**，钩它们的唯一目的＝抓实体指针（只读 pass-through，不改返回值）。
//   uniq16/uniq32 实测：Hp uniq16=1、Atk uniq16=1、Camp uniq16=15 不可用 ⇒ Camp 取 32B（uniq32=1）。
static const uint32_t kPrologueEntHp[4]   = {0xa9bf4ffeu, 0xd00a2628u, 0xaa0003f3u, 0x395a3908u};
static const uint32_t kPrologueEntAtk[4]  = {0xa9bf4ffeu, 0xd00a2628u, 0xaa0003f3u, 0x395a3d08u};
static const uint32_t kPrologueEntCamp[8] = {0x3942e000u, 0xd65f03c0u, 0x3902e001u, 0xd65f03c0u,
                                             0xa9bf4ffeu, 0xd00a2628u, 0xaa0003f3u, 0x395a4908u};

// ============================================================================
//  R36（2026-10-23）只读探针四站 —— 三问一次战斗定案：
//    ① 0x16de0acc 是否真是 id→实体解析
//    ② 实体真实 HP / 阵营偏移（+0x90..+0xBC 全量 dump：谁随受伤下降、+0xB8 是否分组）
//    ③ 哪个 HP 写候选在受伤时被调用（0x5a5a400 / 0x1aa964b0 / 0x1aa95b14 计数）
//  静态定位与判据见 10_docs/绝区零/伤害结算定位_3.3.0_静态.md。
//  ★铁律（本段不许演化成功能）：**pass-through** —— 调原函数、不改入参、不写内存、不替换返回值。
//  序言 32B 均为磁盘 so 直读原字节（取 file_off = RVA - 0x4000），装钩前 CheckPrologue 逐字校验。
static constexpr uintptr_t kFileOffResolve = 0x16ddcacc;   // RVA 0x16de0acc id→对象解析
static constexpr uintptr_t kFileOffHpWa    = 0x05a56400;   // RVA 0x05a5a400 带护盾血条型，写 +0xA4/+0x98
static constexpr uintptr_t kFileOffHpWb    = 0x1aa924b0;   // RVA 0x1aa964b0 对称两块，写 +0xA0
static constexpr uintptr_t kFileOffHpWc    = 0x1aa91b14;   // RVA 0x1aa95b14 对称两块，写 +0xA0
// sub sp,sp,#0x50 ; stp x30,x23 ; stp x22,x21 ; stp x20,x19 ; mov w19,w1 ; mov x20,x0 ...
static const uint32_t kPrologueResolve[8] = {0xd10143ffu, 0xa9025ffeu, 0xa90357f6u, 0xa9044ff4u,
                                             0x2a0103f3u, 0xaa0003f4u, 0xf9400808u, 0xa9017fffu};
// stp x29,x30 ; stp x22,x21 ; stp x20,x19 ; mov x29,sp ; ldr x8,[x0,#8] ; mov x19,x0 ; ldr w9,[x0,#0x9c] ; ldr w13,[x0,#0xa4]
static const uint32_t kPrologueHpWa[8]    = {0xa9bd7bfdu, 0xa90157f6u, 0xa9024ff4u, 0x910003fdu,
                                             0xf9400408u, 0xaa0003f3u, 0xb9409c09u, 0xb940a40du};
// stp x30,x23 ; stp x22,x21 ; stp x20,x19 ; adrp x22 ; mov x19,x2 ; mov x20,x1 ; mov x21,x0 ; ldrb w8,[x22,#0xce]
static const uint32_t kPrologueHpWb[8]    = {0xa9bd5ffeu, 0xa90157f6u, 0xa9024ff4u, 0x9001fa16u,
                                             0xaa0203f3u, 0xaa0103f4u, 0xaa0003f5u, 0x39433ac8u};
// stp x30,x25 ; stp x24,x23 ; stp x22,x21 ; stp x20,x19 ; cbz x1 ; mov x19,x2 ; cbz x2 ; mov x21,x4
static const uint32_t kPrologueHpWc[8]    = {0xa9bc67feu, 0xa9015ff8u, 0xa90257f6u, 0xa9034ff4u,
                                             0xb4002741u, 0xaa0203f3u, 0xb4002722u, 0xaa0403f5u};

// R5b（2026-10-06）：移速类「把速度重置为常量 1.0f」的两处站点（方法3 / 方法4 内 3 条写指令）。
//   反汇编实证（file 0xfb270e4 / 0xfb2720c 两个函数体）：
//     0x...16c  strb wzr,[x0,#0x104]  (0x3904101f)
//     0x...170  str  w8,[x0,#0xf8]   (0xb900f808)   w8 = 0x3f800000 = 1.0f
//     0x...174  str  w8,[x0,#0x100]  (0xb9010008)  ★R5b 勘误：原注释/常量误写 0xb9001008（偏移 0x10）
//   x0 = 按 [x19+0x60] 查表得到的「速度组件」；x19 = 实体（函数入口 mov x19,x0）。
//   与 apply 方法（file 0xfb26f14，已打桩的 0xfb2700c）同属一个速度比率子系统：
//   apply 写 ratio/flag=1，本两处 reset 写 ratio=1.0f/flag=0。
static const uintptr_t kFileOffMoveResetA     = 0xfb2716c;
static const uintptr_t kFileOffMoveResetAEnd  = 0xfb27178;
static const uintptr_t kFileOffMoveResetB     = 0xfb27294;
static const uintptr_t kFileOffMoveResetBEnd  = 0xfb272a0;
static const uint32_t  kPrologueMoveReset[3]  = {0x3904101fu, 0xb900f808u, 0xb9010008u};   // 0xb9010008 = str w8,[x0,#0x100]

// ============================================================================
//  ② 开关 / 数值（铁律：滑块 1 = 关闭，默认必须 1）
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_reenter{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};        // 无限技能 = 无限大招（池鸳 kRvaUltGate 补丁）
static std::atomic<bool> g_freeze{false};
static std::atomic<int>  g_moveMul{1};          // 移速功能已删（2026-10-07），此值恒为 1，仅供给保留的桩做诊断
static std::atomic<int>  g_dmgMul{1};           // 1..ZZZ_DMG_MUL_MAX
static std::atomic<bool> g_ultApplied{false};   // 大招补丁是否已落下（回读看护）
static std::atomic<unsigned> g_moveHitsLast{0}; // 移速 handler 命中计数快照（每 5s 对比）
static std::atomic<unsigned> g_moveResetALast{0};   // R5b：resetA 命中快照
static std::atomic<unsigned> g_moveResetBLast{0};   // R5b：resetB 命中快照
static std::atomic<int>  g_moveIdleTicks{0};    // 开了倍率但长期零命中 → 打一次告警

static std::atomic<bool> g_invHooked{false};
static std::atomic<bool> g_dmgHooked{false};
static std::atomic<bool> g_movePatched{false};
static std::atomic<bool> g_moveResetAPatched{false};   // R5b：reset 方法3 内 12B 桩
static std::atomic<bool> g_moveResetBPatched{false};   // R5b：reset 方法4 内 12B 桩
static std::atomic<bool> g_freezeHooked{false};
static std::atomic<bool> g_entHpHooked{false};     // R34 实体 getter 钩（只读抓实体指针）
static std::atomic<bool> g_entAtkHooked{false};
static std::atomic<bool> g_entCampHooked{false};
static std::atomic<bool> g_resolveHooked{false};      // R36 id→实体解析探针
static std::atomic<bool> g_hpWaHooked{false};         // R36 HP 写候选 A
static std::atomic<bool> g_hpWbHooked{false};         // R36 HP 写候选 B
static std::atomic<bool> g_hpWcHooked{false};         // R36 HP 写候选 C
static std::atomic<bool> g_moveEntryHooked{false};   // R5f: SetMoveSpeedRatio.apply entry hook (diag)
static std::atomic<unsigned> g_moveEntryHits{0};
static std::atomic<unsigned> g_moveEntryLast{0};

// ---- 移动速度原地桩 handler 用的 C 链接全局（asm 直接 adrp/add 引用，名字必须稳定） ----
extern "C" {
volatile unsigned char g_zzzMoveOn     = 0;      // 0 = 只跑原生 3 条指令
volatile float         g_zzzMoveMul    = 1.0f;   // 乘区（1.0 = 不放大）
volatile void*         g_zzzMoveResume = nullptr; // 原函数收尾 VA
volatile unsigned int  g_zzzMoveHits   = 0;      // handler 进入次数（诊断：站点是否真跑）
volatile unsigned int  g_zzzMoveOnHits = 0;      // 其中真正乘过倍率的次数
// R5b：两个 reset 站点各自的回收地址 / 命中数 / 学到的实体 id（全为诊断 + 回收用）
volatile void*         g_zzzMoveResetAResume = nullptr;
volatile void*         g_zzzMoveResetBResume = nullptr;
volatile unsigned int  g_zzzMoveResetAHits   = 0;
volatile unsigned int  g_zzzMoveResetBHits   = 0;
volatile unsigned int  g_zzzMoveSiteIdA      = 0;
volatile unsigned int  g_zzzMoveSiteIdB      = 0;
}

// 12B 原地桩跳进来的 handler：
//   入参沿用站点处寄存器现场：x0 = 目标组件，x8 = 1（strb 用），s8 = 待写入的比率
//   只在需要时对 s8 乘倍率，随后原样执行被替换掉的 3 条指令并跳回收尾。
extern "C" __attribute__((naked)) void Asm_ZzzMoveApply() {
    __asm__ volatile(
        "adrp x9, g_zzzMoveOn\n"
        "add  x9, x9, :lo12:g_zzzMoveOn\n"
        "ldrb w10, [x9]\n"
        "cbz  w10, 1f\n"
        "adrp x9, g_zzzMoveMul\n"
        "add  x9, x9, :lo12:g_zzzMoveMul\n"
        "ldr  s0, [x9]\n"
        "fmul s8, s8, s0\n"
        "adrp x9, g_zzzMoveOnHits\n"
        "add  x9, x9, :lo12:g_zzzMoveOnHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "1:\n"
        "adrp x9, g_zzzMoveHits\n"
        "add  x9, x9, :lo12:g_zzzMoveHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "str  s8, [x0, #0xf8]\n"
        "str  s8, [x0, #0x100]\n"
        "strb w8, [x0, #0x104]\n"
        "adrp x16, g_zzzMoveResume\n"
        "add  x16, x16, :lo12:g_zzzMoveResume\n"
        "ldr  x16, [x16]\n"
        "br   x16\n");
}

// R5b：两处「把速度重置为 1.0f」站点各自的 handler（方法3 / 方法4 内的 3 条写指令）。
//   开倍率：ratio=g_zzzMoveMul 写 [+0xf8]/[+0x100]，[+0x104]=1（与 apply 同语义＝覆盖生效）
//   关倍率：原样执行被替换的 3 条指令（ratio=1.0f / flag=0）
//   两路都记录 [x19+0x60]（实体 id）与命中计数，供诊断与按实体收窄
//   ⚠️ 只用 x8/x9/x10/x16/s0（caller-saved）；v8~v15 是被调用者保存，碰了会污染上层返回值
extern "C" __attribute__((naked)) void Asm_ZzzMoveResetA() {
    __asm__ volatile(
        "adrp x9, g_zzzMoveOn\n"
        "add  x9, x9, :lo12:g_zzzMoveOn\n"
        "ldrb w10, [x9]\n"
        "cbz  w10, 1f\n"
        "adrp x9, g_zzzMoveMul\n"
        "add  x9, x9, :lo12:g_zzzMoveMul\n"
        "ldr  s0, [x9]\n"
        "str  s0, [x0, #0xf8]\n"
        "str  s0, [x0, #0x100]\n"
        "mov  w10, #1\n"
        "strb w10, [x0, #0x104]\n"
        "adrp x9, g_zzzMoveResetAHits\n"
        "add  x9, x9, :lo12:g_zzzMoveResetAHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "b    2f\n"
        "1:\n"
        "strb wzr, [x0, #0x104]\n"
        "mov  w8, #0x3f800000\n"
        "str  w8, [x0, #0xf8]\n"
        "str  w8, [x0, #0x100]\n"
        "2:\n"
        "ldr  w10, [x19, #0x60]\n"
        "adrp x9, g_zzzMoveSiteIdA\n"
        "add  x9, x9, :lo12:g_zzzMoveSiteIdA\n"
        "str  w10, [x9]\n"
        "adrp x16, g_zzzMoveResetAResume\n"
        "add  x16, x16, :lo12:g_zzzMoveResetAResume\n"
        "ldr  x16, [x16]\n"
        "br   x16\n");
}

extern "C" __attribute__((naked)) void Asm_ZzzMoveResetB() {
    __asm__ volatile(
        "adrp x9, g_zzzMoveOn\n"
        "add  x9, x9, :lo12:g_zzzMoveOn\n"
        "ldrb w10, [x9]\n"
        "cbz  w10, 1f\n"
        "adrp x9, g_zzzMoveMul\n"
        "add  x9, x9, :lo12:g_zzzMoveMul\n"
        "ldr  s0, [x9]\n"
        "str  s0, [x0, #0xf8]\n"
        "str  s0, [x0, #0x100]\n"
        "mov  w10, #1\n"
        "strb w10, [x0, #0x104]\n"
        "adrp x9, g_zzzMoveResetBHits\n"
        "add  x9, x9, :lo12:g_zzzMoveResetBHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "b    2f\n"
        "1:\n"
        "strb wzr, [x0, #0x104]\n"
        "mov  w8, #0x3f800000\n"
        "str  w8, [x0, #0xf8]\n"
        "str  w8, [x0, #0x100]\n"
        "2:\n"
        "ldr  w10, [x19, #0x60]\n"
        "adrp x9, g_zzzMoveSiteIdB\n"
        "add  x9, x9, :lo12:g_zzzMoveSiteIdB\n"
        "str  w10, [x9]\n"
        "adrp x16, g_zzzMoveResetBResume\n"
        "add  x16, x16, :lo12:g_zzzMoveResetBResume\n"
        "ldr  x16, [x16]\n"
        "br   x16\n");
}

// ============================================================================
//  ③ /proc/self/maps → 文件偏移换算 VA（分段版；MuMu 分块映射实测口径）
// ============================================================================
struct MapSeg {
    uintptr_t start = 0, end = 0, off = 0;
    char      perm[8] = {};
};

static int ReadLibSegs(const char* lib, MapSeg* out, int maxOut) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[1024];
    int n = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (!std::strstr(line, lib)) continue;
        uintptr_t s = 0, e = 0, o = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &s, &e, &perm[0], &o) != 4) continue;
        if (n < maxOut) {
            out[n].start = s;
            out[n].end   = e;
            out[n].off   = o;
            std::snprintf(out[n].perm, sizeof(out[n].perm), "%s", perm);
            ++n;
        }
    }
    std::fclose(fp);
    // 按文件偏移升序（段数很少，插入排序足够）
    for (int i = 1; i < n; ++i) {
        MapSeg key = out[i];
        int j = i - 1;
        while (j >= 0 && out[j].off > key.off) { out[j + 1] = out[j]; --j; }
        out[j + 1] = key;
    }
    return n;
}

// 尾部匿名 BSS：MuMu 下 arm64 so 的 .bss 落在最后一段之后的匿名 rw-p 映射里
static uintptr_t FindTailAnonBss(const char* lib, uintptr_t tailEnd) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[1024];
    bool sawLib = false;
    uintptr_t anon = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (std::strstr(line, lib)) { sawLib = true; continue; }
        if (!sawLib) continue;
        uintptr_t s = 0, e = 0, o = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &s, &e, &perm[0], &o) == 4) {
            if (s == tailEnd && std::strchr(perm, 'w') && !std::strstr(line, "/")) {
                anon = s;   // 紧跟其后、无路径、可写 ⇒ BSS 延伸
            }
        }
        break;              // 只看紧随 lib 最后一段之后的那一行
    }
    std::fclose(fp);
    return anon;
}

// 文件偏移 → VA（含 MuMu 分块 + 尾部 BSS）
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    static MapSeg segs[32];
    int n = ReadLibSegs(lib, segs, 32);
    if (n <= 0) return nullptr;

    for (int i = 0; i < n; ++i) {
        uintptr_t span = segs[i].end - segs[i].start;
        if (fileOff >= segs[i].off && fileOff < segs[i].off + span) {
            return reinterpret_cast<void*>(segs[i].start + (fileOff - segs[i].off));
        }
    }
    // 超出文件段范围 ⇒ 试尾部匿名 BSS
    MapSeg& tail = segs[n - 1];
    uintptr_t tailEnd = tail.off + (tail.end - tail.start);
    if (fileOff >= tailEnd) {
        uintptr_t bss = FindTailAnonBss(lib, tail.end);
        if (bss) return reinterpret_cast<void*>(bss + (fileOff - tailEnd));
    }
    return nullptr;
}

// ============================================================================
//  ④ 读取原语（R20：故障信号守卫整段移除）
//  R19 现场（09_test/_recorder/20261007_142643）：四项功能全关仍冻死，UnityMain /
//  UnityGfxDeviceW / SRPSubmitThread 全卡在 libhoudini 同一组帧，而菜单层（Android UI
//  线程）还活着 ⇒ 冻的是 Houdini 翻译层，不是我们的 handler。
//  头号根因：进程级替换 SIGSEGV/SIGBUS/SIGILL 处理器 + 热路径 sigsetjmp/siglongjmp。
//  Houdini 的惰性翻译/自修改代码处理本身依赖信号，guard 的 longjmp 会打断翻译器
//  内部状态机 ⇒ 全进程卡死。池鸳与常规做法都是裸读。
//  R20 铁律：本文件禁止再出现 sigaction / sigsetjmp / siglongjmp；所有读取只允许指向
//  「活对象或本进程已装载模块」的地址（调用点自行保证），裸读本身即安全。
// ============================================================================
static std::mutex g_installMutex;

static bool ReadU32Guarded(const char* tag, const void* p, uint32_t* out) {
    (void)tag;
    *out = *reinterpret_cast<const volatile uint32_t*>(p);
    return true;
}

static bool ReadPtrGuarded(const char* tag, const void* p, uint64_t* out) {
    (void)tag;
    *out = *reinterpret_cast<const volatile uint64_t*>(p);
    return true;
}

static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    int rc = DobbyHook(target, (dobby_dummy_func_t)hook, (dobby_dummy_func_t*)orig);
    if (rc != 0) {
        LOGW(TPL_LOG " %s dobby fail rc=%d", tag, rc);
        if (orig) *orig = nullptr;
        return false;
    }
    return true;
}

// 同一 key 的重复日志限制（防「每 tick 重试 + 刷日志」把主循环拖垮）
static bool RateLimitLog(const char* key, long long intervalSec) {
    constexpr int kMaxKeys = 16;
    static const char* s_keys[kMaxKeys] = {};
    static long long s_last[kMaxKeys] = {};
    int slot = -1;
    for (int i = 0; i < kMaxKeys; ++i) {
        if (s_keys[i] == key) { slot = i; break; }
        if (!s_keys[i]) { s_keys[i] = key; slot = i; break; }
    }
    if (slot < 0) slot = 0;
    const long long now = static_cast<long long>(time(nullptr));
    if (now - s_last[slot] < intervalSec) return false;
    s_last[slot] = now;
    return true;
}

// 序言校验：读地址前 16B；首访打印每条期望/实际，重试路径静默（由调用方限流）
static bool CheckPrologue(const char* tag, void* va, const uint32_t* want, int words, bool firstAttempt) {
    for (int i = 0; i < words; ++i) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, reinterpret_cast<const char*>(va) + i * 4, &got)) return false;
        if (got != want[i]) {
            if (firstAttempt) {
                LOGW(TPL_LOG " %s drift [+%d] got=0x%08X want=0x%08X — 拒绝挂钩", tag, i * 4, got, want[i]);
            }
            return false;
        }
    }
    return true;
}

// R27（2026-10-22）：读前指针闸门。
//   背景：3.3.0 上旧链根 0x1d749418（全进程共享全局）读到垃圾 0x1200000095be94d1 后仍继续
//   +0x18 解引用 ⇒ 后台线程 SIGSEGV 带崩进程（用户 L3：开无敌挨打即闪退）。任何裸读前必须过这道闸。
//   口径：清 TBI/MTE 顶字节后 ∈ [4GiB, 128TiB)（starptr 唯一实现）&& 按 align 对齐。
// ★设备无关修正（2026-10-25，LDPlayer/Houdini）：上界原为 0x8000000000（512GiB）。
//   那个值只对 39 位用户态布局成立；LDPlayer 的 houdini 把 arm64 客户库映射在
//   0x4000_23e0_0000（≈70TB）⇒ 越界即整片判否 ⇒ PrologueMatches / UltGateCtxOk 全 false
//   ⇒ Zzz_TryInit 永不就绪、四项功能全哑（菜单照常弹出）。真机 39 位布局下正常，故只在模拟器暴露。
static inline bool ValidPtrForRead(const void* p, size_t align) {
    if (!starptr::LiveFast(p)) return false;
    if (align && (reinterpret_cast<uintptr_t>(p) & (align - 1)) != 0) return false;
    return true;
}

// 静默读（就绪探测/丢失看护用）。R20 曾改成裸读；R27 改为「先校验、不合法返 false、绝不裸解引用」。
static bool ReadU32Quiet(const void* p, uint32_t* out) {
    if (!ValidPtrForRead(p, 4)) return false;
    *out = *reinterpret_cast<const volatile uint32_t*>(p);
    return true;
}

// 静默序言比对（无日志；就绪门/丢失看护用）
static bool PrologueMatches(void* va, const uint32_t* want, int words) {
    for (int i = 0; i < words; ++i) {
        uint32_t got = 0;
        if (!ReadU32Quiet(reinterpret_cast<const char*>(va) + i * 4, &got)) return false;
        if (got != want[i]) return false;
    }
    return true;
}

// ============================================================================
//  ⑤a 真实模块 VA 解析（2026-10-06，MuMu 实战零命中根因修正）
//   MuMu 启动早期 maps 有游戏校验/预读 .so 的【同名临时分块映射】（0x77xx 宿主堆区，
//   随后被回收/复用）——isLibraryLoaded 按名字命中是「假就绪」，按那些映射装钩
//   = 装上了永不命中（实测：0x771014230138 零命中；同一函数后来按稳定 maps 口径
//   0x40002f62a9f0 装钩，30ms 内即打印命中日志）。
//   真实模块以 loader 记录（dl_iterate_phdr）为准；VA = base + p_vaddr + (fileOff - p_offset)。
// ============================================================================
struct PhdrCtx {
    const char*       lib;
    uintptr_t         base[4];
    const Elf64_Phdr* phdr[4];
    int               phnum[4];
    int               n;
};

static int PhdrCallback(struct dl_phdr_info* info, size_t, void* data) {
    PhdrCtx* ctx = static_cast<PhdrCtx*>(data);
    const char* n = info->dlpi_name;
    if (!n || !ctx->lib || ctx->n >= 4) return 0;
    size_t ln = std::strlen(ctx->lib), l = std::strlen(n);
    if (l < ln || std::strcmp(n + (l - ln), ctx->lib) != 0) return 0;
    ctx->base[ctx->n]  = static_cast<uintptr_t>(info->dlpi_addr);
    ctx->phdr[ctx->n]  = info->dlpi_phdr;
    ctx->phnum[ctx->n] = info->dlpi_phnum;
    ctx->n++;
    return 0;   // 收全（同名假库用序言筛）
}

// pro=nullptr：返回首个候选；pro 给定：返回首个序言匹配的候选（全不匹配 = nullptr，fail-closed）
static void* PhdrVaForFileOff(const char* lib, uintptr_t fileOff,
                              const uint32_t* pro = nullptr, int words = 0) {
    PhdrCtx ctx = {};
    ctx.lib = lib;
    dl_iterate_phdr(PhdrCallback, &ctx);
    void* first = nullptr;
    for (int m = 0; m < ctx.n; ++m) {
        const Elf64_Phdr* ph = ctx.phdr[m];
        if (!ph) continue;
        for (int i = 0; i < ctx.phnum[m]; ++i) {
            const Elf64_Phdr& p = ph[i];
            if (p.p_type != PT_LOAD) continue;
            uintptr_t span = p.p_filesz > p.p_memsz ? p.p_filesz : p.p_memsz;   // BSS 计入 memsz
            if (fileOff < p.p_offset || fileOff >= p.p_offset + span) continue;
            void* va = reinterpret_cast<void*>(ctx.base[m] + p.p_vaddr + (fileOff - p.p_offset));
            if (!first) first = va;
            if (!pro) return va;
            if (PrologueMatches(va, pro, words)) return va;
        }
    }
    return pro ? nullptr : first;
}

// RVA 空间原点：libil2cpp 里【文件偏移 0】那个 PT_LOAD 的映射起点。
// 池鸳与我们的常量（0x120faea4 / 0x1d749418 …）都是相对它的偏移 ⇒ VA = origin + RVA。
// 实证 2026-10-06 MuMu：origin=0x400023600000，origin+0x1d749418 链通（actorId=5）；
// 同一常量套 phdr 文件偏移公式（该段 p_vaddr-p_offset=0xC000）读到 NULL ⇒ 必须分开两条公式。
static uintptr_t PhdrRvaOrigin(const char* lib) {
    PhdrCtx ctx = {};
    ctx.lib = lib;
    dl_iterate_phdr(PhdrCallback, &ctx);
    uintptr_t best = 0;
    for (int m = 0; m < ctx.n; ++m) {
        const Elf64_Phdr* ph = ctx.phdr[m];
        if (!ph) continue;
        for (int i = 0; i < ctx.phnum[m]; ++i) {
            const Elf64_Phdr& p = ph[i];
            if (p.p_type != PT_LOAD || p.p_offset != 0) continue;
            uintptr_t va = ctx.base[m] + p.p_vaddr;
            if (!best || va < best) best = va;
        }
    }
    return best;
}

// ============================================================================
//  ⑤ 玩家 actorId（池鸳 libcymod 0x4ed30 同款链；读失败 = fail-open 调原函数）
// ============================================================================
// R20：裸读。调用点只在功能开启时走链（见 Hook_Inv 门控）。
static bool ReadPtrQuiet(const void* p, uint64_t* out) {
    if (!ValidPtrForRead(p, 8)) return false;   // R27：先校验再读
    *out = *reinterpret_cast<const volatile uint64_t*>(p);
    return true;
}

// R5（2026-10-06）：进程内自读第二路径 —— /proc/self/mem。
//   R4 实测：同一地址 PC 侧 /proc/<pid>/mem 读得到（actorId=5/6），进程内 direct load 却 10/10 走不通
//   ⇒ 链读取改 direct → pread(/proc/self/mem) 双路径；下面的自检会把两条路径逐级结果都打出来。
//   R5b：一旦失败就永久关闭（MuMu 实测这条路被拒）——否则伤害热路径上每个事件都要白开 7 个 fd。
static std::atomic<bool> g_selfMemDead{false};

static bool ReadMemSelf(void* dst, const void* src, size_t len) {
    if (g_selfMemDead.load()) return false;
    int fd = open("/proc/self/mem", O_RDONLY | O_CLOEXEC);
    if (fd < 0) { g_selfMemDead.store(true); return false; }
    ssize_t got = pread(fd, dst, len, static_cast<off_t>(reinterpret_cast<uintptr_t>(src)));
    close(fd);
    if (got != static_cast<ssize_t>(len)) { g_selfMemDead.store(true); return false; }
    return true;
}

static bool ReadPtrRobust(const void* p, uint64_t* out) {
    if (ReadPtrQuiet(p, out)) return true;
    return ReadMemSelf(out, p, sizeof(*out));
}

static bool ReadU32Robust(const void* p, uint32_t* out) {
    if (ReadU32Quiet(p, out)) return true;
    return ReadMemSelf(out, p, sizeof(*out));
}
static const size_t kActorChain[7] = {0x08, 0x18, 0x10, 0x78, 0x00, 0xb8, 0x00};

// ============================================================================
//  R5b：玩家侧 id 集（多源学习 + 跨场景缓存）
//   R4 实测教训：actor 链只在【大世界】成立，战斗里（01:57–01:59 连续 10 次重选）全候选失败
//   ⇒ me=0 ⇒ 无敌/伤害倍率同时哑。R5b 修法：
//     ① 链成功就把 id 记进集合，【不随场景清空】——战斗里链失效也能用缓存判归属；
//     ② 事件自带的所有权字段（+0x1C 攻击方 / +0x3C 受击方）继续按集合判定（R2/R4 两局实测：
//        玩家攻击事件 atk 所有权=5、怪物受害方所有权=0 ⇒ 0 是“无玩家归属”的 AI 侧）；
//     ③ 集合为空时用「本局连续出现最多的攻击方所有权值」兑底（≥3 次才认），并落日志。
// ============================================================================
static std::mutex   g_meMutex;
static uint32_t     g_meIds[4] = {0, 0, 0, 0};
static int          g_meIdN    = 0;
static std::atomic<unsigned> g_ownVoteVal{0};   // +0x1C 最近连续同值
static std::atomic<unsigned> g_ownVoteN{0};     // 该值连续出现次数
static std::atomic<unsigned> g_meTeam{0};      // 玩家方所有权 id（R5e：与 actor id 分开存，实测 =5）

static void LearnMeId(uint32_t id) {
    if (!id) return;
    std::lock_guard<std::mutex> lk(g_meMutex);
    for (int i = 0; i < g_meIdN; ++i) if (g_meIds[i] == id) return;
    if (g_meIdN >= 4) {
        static std::atomic<int> n{0};
        if (n.fetch_add(1) < 3) LOGW(TPL_LOG " 玩家 id 集已满（%u 忽略；现有 %u/%u/%u/%u）",
                                     (unsigned)id, (unsigned)g_meIds[0], (unsigned)g_meIds[1],
                                     (unsigned)g_meIds[2], (unsigned)g_meIds[3]);
        return;
    }
    g_meIds[g_meIdN++] = id;
    LOGI(TPL_LOG " 玩家 id 集 +%u（现 %d 个：%u/%u/%u/%u）", (unsigned)id, g_meIdN,
         (unsigned)g_meIds[0], (unsigned)g_meIds[1], (unsigned)g_meIds[2], (unsigned)g_meIds[3]);
}

// 链学到的 id 同时喂给「对攻关系表」（锁序：g_meMutex → g_sideMutex，单向）

static bool IsMineId(uint32_t id) {
    if (!id) return false;
    std::lock_guard<std::mutex> lk(g_meMutex);
    for (int i = 0; i < g_meIdN; ++i) if (g_meIds[i] == id) return true;
    return false;
}

static int MeIdCount() {
    std::lock_guard<std::mutex> lk(g_meMutex);
    return g_meIdN;
}

[[maybe_unused]] static uint32_t FirstMeId() {
    std::lock_guard<std::mutex> lk(g_meMutex);
    return g_meIdN > 0 ? g_meIds[0] : 0u;
}

// R5e：改成「频次投票」而不是「连续同值」——怪打我方时会插入 own=0 的受击事件，
//   连续口径容易被非玩家值抢跑；频次口径下玩家攻击占绝对多数，稳。
//   投票结果只写 g_meTeam（所有权口径），【不再】混进 actor id 集。
// ============================================================================
//  R8：多角色归属（ZZZ 一队最多 3 人）。
//   R7b 实测（09_test/_recorder/20261007_015629）：同局我方出现两个 actor id
//     —— 5 和 6，两者都打同一条怪 12；而事件 +0x1C(own) 恒为 5。
//     只靠 own 只能保护 5 号（用户实测：角色2 无敌、角色1 不无敌）。
//   做法：不依赖链、不依赖新场景，用「对攻关系」在线扩我方集合：
//     种子：own（投票值）视为我方（实测 = 5）。
//     规则1：已知我方当攻击方，受击方未知 ⇒ 受击方 = 怪
//     规则2：已知我是怪打未知 ⇒ 未知记「被怪打过」
//     规则3：未知 id 打已知怪，且它被怪打过 ⇒ 同阵营 = 我方（互相对攻才是敌对关系）
//   安全边界：只有规则 3 才扩我方，且需「互相对攻」两条证据；怪打怪才会误判，本作单队对怪，风险低。
// ============================================================================
struct SideRec { uint32_t id; uint8_t mine, monster, hitByMonster, hitMonster; };
static SideRec    g_side[16] = {};
static std::mutex g_sideMutex;

// 调用方持 g_sideMutex
static SideRec* SideSlotLocked(uint32_t id) {
    SideRec* empty = nullptr;
    for (int i = 0; i < 16; ++i) {
        if (g_side[i].id == id) return &g_side[i];
        if (!g_side[i].id && !empty) empty = &g_side[i];
    }
    return empty;
}

static void SideMarkMine(uint32_t id) {
    if (!id) return;
    std::lock_guard<std::mutex> lk(g_sideMutex);
    SideRec* s = SideSlotLocked(id);
    if (!s) return;
    s->id = id;
    s->mine = 1;
    s->monster = 0;
}

static void SideResetHitStats() {
    std::lock_guard<std::mutex> lk(g_sideMutex);
    for (int i = 0; i < 16; ++i) {
        g_side[i].hitMonster = 0;
        g_side[i].hitByMonster = 0;
    }
}

static bool SideIsMine(uint32_t id) {
    if (!id) return false;
    std::lock_guard<std::mutex> lk(g_sideMutex);
    for (int i = 0; i < 16; ++i) if (g_side[i].id == id) return g_side[i].mine != 0;
    return false;
}

// 只吃 g_sideMutex（绝不碰 g_meMutex，避免与 IsMineActor 的锁序交叉）
static void ObserveDamagePair(uint32_t atk, uint32_t vic, uint32_t ownId) {
    if (!atk || !vic || atk == vic) return;
    std::lock_guard<std::mutex> lk(g_sideMutex);
    SideRec* A = SideSlotLocked(atk);
    SideRec* V = SideSlotLocked(vic);
    if (!A || !V) return;
    A->id = atk;
    V->id = vic;
    if (ownId) {
        if (atk == ownId) { A->mine = 1; A->monster = 0; }
        if (vic == ownId) { V->mine = 1; V->monster = 0; }
    }
    // 规则1：已知我方 ⇒ 对方是怪
    if (A->mine && !V->mine) V->monster = 1;
    if (V->mine && !A->mine) A->monster = 1;
    // 规则2：已知怪打未知 ⇒ 未知记「被怪打过」
    if (A->monster && !V->mine) V->hitByMonster = 1;
    // 规则3（R12 放宽）：未知 id 打已知怪 ⇒ 累计「打过怪」次数，≥2 次认定我方。
    //   R11 实测：旧口径还要求「被怪打过」（互相对攻）才扩列，角色 2（id 6）整局没被怪打中
    //   就永远进不了我方集合 ⇒ 切到该角色照样掉血。现在以「打过怪」为主证据，连续 2 次即扩
    //   （防单次误判）；「被怪打过」这条旧证据保留，命中即可一次生效。
    if (V->monster && !A->mine && !A->monster) {
        if (A->hitMonster < 255) ++A->hitMonster;
        if (A->hitMonster >= 2 || A->hitByMonster) {
            A->mine = 1;
            LOGI(TPL_LOG " 我方 id +%u（对攻判定：打过怪 %u ×%u，被怪打过=%u）",
                 (unsigned)atk, (unsigned)vic, (unsigned)A->hitMonster, (unsigned)A->hitByMonster);
        }
    }
}

struct OwnVote { uint32_t val; unsigned n; };
static OwnVote    g_ownVotes[4] = {};
static std::mutex g_ownVoteMutex;

static void ObserveAttackerOwner(uint32_t own) {
    if (!own) return;
    std::lock_guard<std::mutex> lk(g_ownVoteMutex);
    int slot = -1, empty = -1, minI = 0;
    for (int i = 0; i < 4; ++i) {
        if (g_ownVotes[i].val == own) { slot = i; break; }
        if (!g_ownVotes[i].val) { if (empty < 0) empty = i; }
        else if (g_ownVotes[i].n < g_ownVotes[minI].n) minI = i;
    }
    if (slot < 0) {
        slot = (empty >= 0) ? empty : minI;   // 表满时淘汰计数最小的槽
        g_ownVotes[slot].val = own;
        g_ownVotes[slot].n = 0;
    }
    const unsigned n = ++g_ownVotes[slot].n;
    int best = 0;
    for (int i = 1; i < 4; ++i) if (g_ownVotes[i].n > g_ownVotes[best].n) best = i;
    g_ownVoteVal.store(own);
    g_ownVoteN.store(n);
    const unsigned bestN = g_ownVotes[best].n;
    const uint32_t bestVal = g_ownVotes[best].val;
    if (bestN >= 3 && (g_meTeam.load() == 0 || bestN == 50)) {
        g_meTeam.store(bestVal);
        LOGI(TPL_LOG " 玩家方所有权 id = %u（投票 %u/%u/%u/%u，本次 own=%u）",
             (unsigned)bestVal, (unsigned)g_ownVotes[0].n, (unsigned)g_ownVotes[1].n,
             (unsigned)g_ownVotes[2].n, (unsigned)g_ownVotes[3].n, (unsigned)own);
    }
}

// R31（2026-10-22）：归属学习改为「互攻」判据（R29 的「被>=3 个不同攻击方打」被证会误学怪物：
//   R29 那场把 vic=5（怪）学成我方并拦了 12 次；R30 那场我方是 atk 侧 5/9、敌方 vic 侧 56/61/64）。
//   规则：维护每个 id 的 atkCount / vicCount；某 id **两侧都 >=1**（既打人也挨打）⇒ LearnMeId(id)。
//   首个人 id 同步写 g_meTeam（兼容旧日志/兜底）。
static void ObserveExchange(uint32_t atk, uint32_t vic) {
    if (!atk || !vic || atk == vic) return;
    struct Rec { uint32_t id; uint32_t atkN, vicN; };
    static Rec s_rec[16] = {};
    static std::mutex s_m;
    std::lock_guard<std::mutex> lk(s_m);
    Rec* ra = nullptr; Rec* rv = nullptr; Rec* empty = nullptr;
    for (int i = 0; i < 16; ++i) {
        if (s_rec[i].id == atk) ra = &s_rec[i];
        if (s_rec[i].id == vic) rv = &s_rec[i];
        if (!s_rec[i].id && !empty) empty = &s_rec[i];
    }
    if (!ra) { if (!empty) return; ra = empty; ra->id = atk; ra->atkN = 0; ra->vicN = 0; empty = nullptr; }
    if (!rv) {
        Rec* e2 = nullptr;
        for (int i = 0; i < 16; ++i) if (!s_rec[i].id) { e2 = &s_rec[i]; break; }
        if (!e2) return;
        rv = e2; rv->id = vic; rv->atkN = 0; rv->vicN = 0;
    }
    if (ra->atkN < 0xFFFF) ++ra->atkN;
    if (rv->vicN < 0xFFFF) ++rv->vicN;
    // R32：只统计、**不再 LearnMeId**（互攻判据对敌我同时成立，学习必然污染 g_meIds）。
    for (Rec* r : { ra, rv }) {
        if (r->atkN >= 1 && r->vicN >= 1) {
            static std::atomic<int> nObs{0};
            if (nObs.fetch_add(1) < 20)
                LOGI(TPL_LOG " 互攻观察 id=%u（atk数=%u vic数=%u，仅观察不学习）",
                     (unsigned)r->id, (unsigned)r->atkN, (unsigned)r->vicN);
        }
    }
}

// NOTE: g_meTeam == the voted value of ev+0x1C (own) == the local player actor id.
// R5f (2026-10-07): the single rule for enemy/ally separation.
//   Evidence (R5e: two full event dumps + 40 INV-EVT lines):
//     enemy hits me  atk=12 own=5 vic=5 vow=0  |  I hit enemy  atk=5 own=5 vic=12 vow=0
//     => ev+0x1C(own) is 5 on BOTH sides, ev+0x3C(vow) is always 0.
//     own means "local player actor id" (the game's own SetMoveSpeedRatio / MonitorMoveSpeed
//     dereference the same global to find the local player), NOT the attacker's owner.
//     R5e bug: treating atkOwn==team as "attacker is mine" is always true => monster id 12
//     got learned as mine => monsters were invincible and the damage path was swallowed.
//   id is mine <=> (a) chain-derived player actor id | (b) own (local player id)
//                 (c) id set learned ONLY from 0x104 local-damage-record events
static bool IsMineActor(uint32_t id, uint32_t chainId, const char** why) {
    if (!id) return false;
    if (chainId && id == chainId) { if (why) *why = "chain"; return true; }
    const uint32_t own = g_meTeam.load();
    if (own && id == own) { if (why) *why = "own"; return true; }
    // R7b: the id set is a valid source again, but ONLY because its feed was cut down
    //   to chain successes (LearnMeId is now called from WalkActorChain / ChainSelfTest
    //   only - the 0x104-record learner that polluted it with enemy id 12 is gone).
    //   Why we need it: R6 battle evidence shows me=0(none) on all 40 INV-EVT lines,
    //   i.e. the actor chain does NOT resolve inside combat (root deref = 0x0), yet the
    //   player actor id learned earlier (hub / open world, e.g. 6) stays valid in
    //   combat - the event fields atk=6/vic=12 prove it. The 30s ChainSelfTest keeps
    //   feeding this set whenever the chain is alive, so one trip through a resolvable
    //   scene is enough for the whole session.
    if (IsMineId(id)) { if (why) *why = "set"; return true; }
    // R24: 暂停使用 pair 学习结果；实测出现过怪物 id 被扩成我方（vic=270/273）。
    return false;
}

// 链本体（池鸳 libcymod 0x4ed30 反汇编同款）；R5：走 Robust 双路径读，读不到不算成功
static bool WalkActorChain(void* root, uint32_t* outId) {
    uint64_t p = 0;
    if (!ReadPtrRobust(root, &p) || !p) return false;
    for (size_t i = 0; i < sizeof(kActorChain) / sizeof(kActorChain[0]); ++i) {
        if (!ReadPtrRobust(reinterpret_cast<const char*>(p) + kActorChain[i], &p) || !p) return false;
    }
    uint32_t id = 0;
    if (!ReadU32Robust(reinterpret_cast<const char*>(p) + 0x38, &id)) return false;
    if (!id) return false;
    *outId = id;
    return true;
}

// R5 自检：逐级把 direct / pread 两条路径的结果落日志（verbose=true 时）。
//  目的：把 R4 那句笼统的「链根全候选失败」变成可追溯到具体级别的证据。
//  R5b：加 verbose 开关 —— 30s 周期自检走简明档（只 direct、不打逐级日志），
//  结果翻转时再补一份逐级详情，避免刷屏。
static bool ChainSelfTest(bool verbose, uint32_t* outId) {
    uintptr_t org = PhdrRvaOrigin(TPL_LIB_NAME);
    void* root = org ? reinterpret_cast<void*>(org + kRvaActorRoot) : nullptr;
    if (verbose) LOGI(TPL_LOG " 链自检 org=0x%lX root=%p", (unsigned long)org, root);
    if (!root) return false;
    // R27：root 本身先过闸（3.3.0 该槽读出的垃圾值曾被直接解引用）
    if (!ValidPtrForRead(root, 8)) {
        if (verbose) LOGI(TPL_LOG " 链自检 root=%p 不合法（R27 闸门拒绝解引用）", root);
        return false;
    }
    const int paths = verbose ? 2 : 1;   // selfmem 兜底已实证被拒（/proc/self/mem EPERM），只在详查时再看
    for (int path = 0; path < paths; ++path) {
        const char* pt = (path == 0) ? "direct" : "selfmem";
        uint64_t cur = 0;
        bool ok = (path == 0) ? ReadPtrQuiet(root, &cur) : ReadMemSelf(&cur, root, 8);
        if (verbose) LOGI(TPL_LOG "  链[%s] root=%p ok=%d val=0x%llX", pt, root, (int)ok, (unsigned long long)cur);
        if (!ok || !cur) continue;
        bool good = true;
        for (int i = 0; i < 7; ++i) {
            const void* a = reinterpret_cast<const char*>(cur) + kActorChain[i];
            uint64_t nxt = 0;
            // R27：每跳先过闸，不合法立即放弃（绝不裸读野指针）
            if (!ValidPtrForRead(a, 8)) {
                if (verbose) LOGI(TPL_LOG "  链[%s] +0x%02X @%p 不合法（R27 拒绝）", pt, (unsigned)kActorChain[i], a);
                good = false; break;
            }
            bool ok2 = (path == 0) ? ReadPtrQuiet(a, &nxt) : ReadMemSelf(&nxt, a, 8);
            if (verbose) LOGI(TPL_LOG "  链[%s] +0x%02X @%p ok=%d val=0x%llX", pt,
                                   (unsigned)kActorChain[i], a, (int)ok2, (unsigned long long)nxt);
            if (!ok2 || !nxt) { good = false; break; }
            cur = nxt;
        }
        if (!good) continue;
        const void* ia = reinterpret_cast<const char*>(cur) + 0x38;
        uint32_t id = 0;
        if (!ValidPtrForRead(ia, 4)) { if (verbose) LOGI(TPL_LOG "  链[%s] id@%p 不合法（R27 拒绝）", pt, ia); continue; }
        bool ok3 = (path == 0) ? ReadU32Quiet(ia, &id) : ReadMemSelf(&id, ia, 4);
        if (verbose) LOGI(TPL_LOG "  链[%s] id@%p ok=%d id=%u", pt, ia, (int)ok3, (unsigned)id);
        if (ok3 && id) { if (outId) *outId = id; LearnMeId(id); return true; }
        if (ok3 && id) { if (outId) *outId = id; LearnMeId(id); SideMarkMine(id); return true; }
    }
    return false;
}

static bool ChainSelfTestVerbose(uint32_t* outId) { return ChainSelfTest(true, outId); }

// 候选交叉校验（能用就用，不能用就换）：① RVA 口径（实证正确）② phdr 文件偏移口径（旧实现）
// ③ maps 分段口径。逐个走链，取首个能得到非 0 actorId 的根地址并落盘日志；全失败则返回首个候选。
static void* ResolveActorRoot() {
    void* cand[3] = {nullptr, nullptr, nullptr};
    int n = 0;
    uintptr_t org = PhdrRvaOrigin(TPL_LIB_NAME);
    if (org) cand[n++] = reinterpret_cast<void*>(org + kRvaActorRoot);
    auto push = [&](void* v) {
        if (!v) return;
        for (int i = 0; i < n; ++i) if (cand[i] == v) return;
        if (n < 3) cand[n++] = v;
    };
    push(PhdrVaForFileOff(TPL_LIB_NAME, kFileOffActorRoot));   // BSS 段：memsz 口径
    push(FileOffToVa(TPL_LIB_NAME, kFileOffActorRoot));
    for (int i = 0; i < n; ++i) {
        uint32_t id = 0;
        if (WalkActorChain(cand[i], &id)) {
            LOGI(TPL_LOG " actor 链根选定 cand#%d @%p (RVA 0x%lX, 首个非0 actorId=%u)",
                 i, cand[i], (unsigned long)kRvaActorRoot, (unsigned)id);
            return cand[i];
        }
    }
    LOGW(TPL_LOG " actor 链根全候选失败（cand#0=%p）— 改用 id 集/所有权字段（R5b）",
         n > 0 ? cand[0] : nullptr);
    return n > 0 ? cand[0] : nullptr;
}

static bool ResolvePlayerActorId(uint32_t* outId) {
    static void* root = nullptr;
    static std::atomic<int> fails{0};
    if (!root) root = ResolveActorRoot();
    if (!root) return false;
    if (WalkActorChain(root, outId)) { fails.store(0); LearnMeId(*outId); return true; }
    if (WalkActorChain(root, outId)) { fails.store(0); LearnMeId(*outId); SideMarkMine(*outId); return true; }
    // 场景切换后链根可能重生：连续失败达阈值就重选候选（不频繁重算）
    if (fails.fetch_add(1) >= 64) { fails.store(0); root = nullptr; }
    return false;
}

// ============================================================================
//  ⑥ hook 本体
// ============================================================================
typedef void (*FnInv_t)(void* self, void* ev, size_t a2, void* a3, void* mth);
typedef void (*FnDmg_t)(void* self, float mult, void* mth);
typedef void (*FnFreeze_t)(void* self, void* a1, void* mth);
typedef uint32_t (*FnEnt_t)(void* self);   // R34：get_hp / get_attack / get_camp 均 (self) -> u32

static FnInv_t    g_origInv    = nullptr;
static FnDmg_t    g_origDmg    = nullptr;
static FnFreeze_t g_origFreeze = nullptr;
static FnEnt_t    g_origEntHp   = nullptr;
static FnEnt_t    g_origEntAtk  = nullptr;
static FnEnt_t    g_origEntCamp = nullptr;

static std::atomic<int> g_evtCalls{0};

// ============================================================================
//  R36：id→实体 16 槽小表 + 实体字段 dump（只读探针的公共件）
//  写：Hook_Resolve 每次解析成功写一格（round-robin，旧值被覆盖是预期行为）。
//  读：Hook_Inv 的伤害分支按 vic(+0x3C) 反查，命中才 dump。
//  只用于**观测**：不参与无敌/倍率的任何判定；表内容永不被当成可信指针使用，
//  dump 前一律先 ValidPtrForRead。
// ============================================================================
struct ResSlot { uint32_t id; uint32_t pad; uintptr_t ent; };
static ResSlot    g_resv[16] = {};
static std::mutex g_resvMutex;
static std::atomic<unsigned> g_resvNext{0};

static void ResTablePut(uint32_t id, uintptr_t ent) {
    if (!id || !ent) return;
    const unsigned k = g_resvNext.fetch_add(1) % 16u;
    std::lock_guard<std::mutex> lk(g_resvMutex);
    g_resv[k].id = id;
    g_resv[k].ent = ent;
}

static bool ResTableLookup(uint32_t id, uintptr_t* out) {
    if (!id || !out) return false;
    bool ok = false;
    std::lock_guard<std::mutex> lk(g_resvMutex);
    for (int i = 0; i < 16; ++i) {
        if (g_resv[i].id == id && g_resv[i].ent) { *out = g_resv[i].ent; ok = true; break; }
    }
    return ok;
}

// 实体 +0x90..+0xBC 共 12 个 u32（HP=+0xA0? / ATK=+0xA4? / camp=+0xB8? / canBattle=+0x99）
// 只读；任一 word 读失败该位打 DEADBEEF，不中断、不重试、不扩大读取范围（R12 深读卡死教训）。
static void DumpEnt12(const char* tag, int n, uintptr_t ent, unsigned atk, unsigned vic) {
    const void* pv = reinterpret_cast<const void*>(ent);
    if (!ValidPtrForRead(pv, 4)) {
        LOGW(TPL_LOG " ENT12 %s n=%d ent=%p 不合法（跳过）", tag, n, (void*)ent);
        return;
    }
    const char* pb = reinterpret_cast<const char*>(pv);
    uint32_t w[12]; int nOk = 0;
    for (int m = 0; m < 12; ++m) {
        if (ReadU32Quiet(pb + 0x90 + m * 4, &w[m])) ++nOk; else w[m] = 0xDEADBEEFu;
    }
    LOGI(TPL_LOG " ENT12 %s n=%d ent=%p ok=%d/12 atk=%u vic=%u", tag, n, (void*)ent, nOk,
         (unsigned)atk, (unsigned)vic);
    LOGI(TPL_LOG "  +90=%08X +94=%08X +98=%08X +9C=%08X | +A0=%08X +A4=%08X +A8=%08X +AC=%08X",
         (unsigned)w[0], (unsigned)w[1], (unsigned)w[2], (unsigned)w[3],
         (unsigned)w[4], (unsigned)w[5], (unsigned)w[6], (unsigned)w[7]);
    LOGI(TPL_LOG "  +B0=%08X +B4=%08X +B8=%08X +BC=%08X",
         (unsigned)w[8], (unsigned)w[9], (unsigned)w[10], (unsigned)w[11]);
}

static float U32AsF(uint32_t u) { float f = 0.0f; std::memcpy(&f, &u, sizeof(f)); return f; }

// R29（2026-10-22）：DiagDumpEvent（3.2.0 的 own/vow 字段摘要）已删除 —— 3.3.0 的 +0x1C 是位掩码、
//   +0x38/+0x3C 语义已换，继续打印只会误导排查；字段观测统一走 Hook_Inv 里的 INV-PROBE2 全量 dump。

// R19（2026-10-07）：0x104 事件记录路线按 R15 定案为死路（写回成功但画面伤害不变），
//   配套的写回/还原/回读诊断一律删除，不再对事件记录做任何字段级扫描或写入。
//   伤害倍率在找到新的数值消费点前保持 no-op，禁止复活本段。

// R19：R9/R10 的 HP 探测/差分/学习整块删除（R12 卡死根因，0x200B×12 对象扫描）。
//   禁止以任何形式复活；伤害倍率新路线不得对任意对象做深读扫描。

// 事件字段（R2+R4 两局实测）：+0x14 高 16 位 = 事件类型；+0x16==0x85 即伤害事件；
//   +0x18 攻击方 actorId / +0x1C 攻击方所有权（玩家攻击时=玩家 id，怪物=0）
//   +0x38 受击方 actorId / +0x3C 受击方所有权（怪物=0）
// ① 无敌：受击方是玩家侧 ⇒ 不调原函数（= 池鸳 0x4e2c8 的过滤，但归属判定扩成多源，见下）
// ② 伤害倍率：R19 起暂时 no-op。R14~R18 的 DmgSettle/VicTail/VicMid 只读诊断
//   与 0x104 写回全部删除（前者在 MuMu/Houdini 热路径卡死，后者已证无效果）。
// R19: 诊断段（VicProbeEntity / VicMidDump / Hook_VicTail / Hook_VicMid /
//   DiagDumpAttackEvent）已整段删除，禁止以任何形式重新启用：卡死栈证据见轮次台账 R18b。
static void MaybeStartEntScan();   // R35 前向声明（定义在实体钩子段之后）
static void MaybeStartAttrScan();  // R45：属性记录链运行时扫描（诊断用，只读）
static bool ZMapped(uintptr_t a, size_t n);
static void ZLoadMaps();

// R42（2026-10-25）：ENTSCAN 采到的实体表快照，供「倍率差值补扣」使用。
//   只存 16 字节对齐、hp 合理、klass 可读的对象；写回前一律重新读一次校验。
struct EntTrackRec { uintptr_t addr; uint32_t hp; uint64_t klass; };
static std::mutex g_entTrackMu;
static std::vector<EntTrackRec> g_entTrack;

// R43（2026-10-25）：只收「ENTSCAN 里真的掉过血」的地址（垃圾候选全被筛掉）。
static std::mutex g_dropMu;
static std::vector<uint64_t> g_dropAddrs;

// R43：伤害不是在同一次调用里同步写入的 ⇒ 事件后延迟复查再补扣。
struct PendSnap { uintptr_t addr; uint32_t hp0; long long due; int mul; };
static std::mutex g_pendMu;
static std::vector<PendSnap> g_pend;
static std::atomic<bool> g_pendStarted{false};
static inline long long NowMs() {
    return std::chrono::duration_cast<std::chrono::milliseconds>(
        std::chrono::steady_clock::now().time_since_epoch()).count();
}
static void PendWatchThread();

static void Hook_Inv(void* self, void* ev, size_t a2, void* a3, void* mth) {
    if (!ev) {
        if (g_origInv) g_origInv(self, ev, a2, a3, mth);
        return;
    }

    // R29（2026-10-22）全量探针（替代 R28 的 INV-PROBE）：3.3.0 payload 结构已重排
    //   （callee 实测 +0x10→+0x14、+0x1c→+0x1e），先无条件 dump 前 24 次的 ev+0x00..+0x60
    //   共 24 个 word（u32 视图 3 行 + u16 视图 6 行），用来定 type / atk / victim 的真实偏移。
    //   只读：全部走 ReadU32Quiet（自带 ValidPtrForRead 闸门），不写任何游戏内存。
    {
        static std::atomic<int> s_probe2{0};
        static std::atomic<int> s_invCalls{0};
        const int n = s_invCalls.fetch_add(1) + 1;
        if (n == 1 || (n % 256) == 0)
            LOGI(TPL_LOG " INV-CALLS total=%d self=%p ev=%p", n, self, ev);
        const int idx = s_probe2.fetch_add(1);
        if (idx < 24) {
            const char* b = reinterpret_cast<const char*>(ev);
            uint32_t w[24]; int nOk = 0;
            for (int k = 0; k < 24; ++k) {
                if (ReadU32Quiet(b + k * 4, &w[k])) ++nOk; else w[k] = 0xDEADBEEFu;
            }
            if (nOk == 24) LOGI(TPL_LOG " INV-PROBE2 n=%d ev=%p self=%p ok=24/24", idx, ev, self);
            else           LOGW(TPL_LOG " INV-PROBE2 n=%d ev=%p self=%p FAIL ok=%d/24", idx, ev, self, nOk);
            for (int line = 0; line < 3; ++line) {
                LOGI(TPL_LOG "  P2 n=%d u32 +0x%02X: %08X %08X %08X %08X %08X %08X %08X %08X",
                     idx, line * 0x20, (unsigned)w[line*8+0], (unsigned)w[line*8+1],
                     (unsigned)w[line*8+2], (unsigned)w[line*8+3], (unsigned)w[line*8+4],
                     (unsigned)w[line*8+5], (unsigned)w[line*8+6], (unsigned)w[line*8+7]);
            }
            for (int line = 0; line < 6; ++line) {
                const uint32_t a0 = w[line*4+0], a1 = w[line*4+1], a2 = w[line*4+2], a3 = w[line*4+3];
                LOGI(TPL_LOG "  P2 n=%d u16 +0x%02X: %04X %04X %04X %04X %04X %04X %04X %04X",
                     idx, line * 0x10,
                     (unsigned)(a0 & 0xFFFF), (unsigned)(a0 >> 16), (unsigned)(a1 & 0xFFFF), (unsigned)(a1 >> 16),
                     (unsigned)(a2 & 0xFFFF), (unsigned)(a2 >> 16), (unsigned)(a3 & 0xFFFF), (unsigned)(a3 >> 16));
            }
        }
    }

    // R29（2026-10-22）3.3.0 字段重映射（依据：语义对比 —— payload 结构重排 + R28 探针）
    //   · type   = u32@+0x14 的高 16 位（3.3.0 疑似把伤害类型 0x85 重编号为 0x86，两个都收）
    //   · atk    = u32@+0x18（未变）
    //   · victim = u32@+0x3C（3.3.0 里恒=5，正是 3.2.0 的玩家 id）
    //   · +0x38 与 +0x1C 仅只读留观（+0x1C 实测是位掩码 0x60000，不是 own）
    uint32_t wt = 0, victim = 0, atk = 0;
    bool okT = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x14, &wt);
    uint16_t type = okT ? static_cast<uint16_t>(wt >> 16) : 0xFFFFu;
    bool okK = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x18, &atk);
    bool okV = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x3C, &victim);
    const bool isDmg = okT && (type == 0x85 || type == 0x86);

    // R31（2026-10-22）实体字段只读探针：验证 HP/攻击/阵营 偏移（HP=+0xA0 / attack=+0xA4 /
    //   camp=+0xB8 / canBattle=+0x99，来自可读名 getter 反汇编）。**只读**，失败只打 FAIL，不影响逻辑。
    //   p = *(ev+0x20)、q = *(ev+0x30)，各自读 +0x90..+0xBC 共 12 个 u32。
    if (isDmg) {
        static std::atomic<int> s_entProbe{0};
        const int pn = s_entProbe.fetch_add(1);
        // R48：诊断关闭（ENTSCAN / ATTR-SCAN 仅保留函数体，正式包不启动）
        (void)&MaybeStartEntScan;
        (void)&MaybeStartAttrScan;
        // R46（2026-10-25）：事件里两个指针（+0x20 / +0x30）全量 dump 0x00..0x1FF（前 3 次事件）。
        //   目的：判定它们是不是「受害者实体 / 伤害记录」，以及伤害数值落在哪个偏移（int 或 float）。
        if (false && pn < 3) {
            ZLoadMaps();
            const char* eb = reinterpret_cast<const char*>(ev);
            for (int k = 0; k < 2; ++k) {
                const uint32_t off = (k == 0) ? 0x20u : 0x30u;
                uint64_t ptr = 0;
                if (!ReadPtrQuiet(eb + off, &ptr) || !ptr) { LOGW(TPL_LOG " FDUMP +0x%02X 读失败", (unsigned)off); continue; }
                const uintptr_t pa = static_cast<uintptr_t>(ptr);
                if (!ZMapped(pa, 0x200)) { LOGW(TPL_LOG " FDUMP +0x%02X ptr=%p 未映射（跳过）", (unsigned)off, (void*)pa); continue; }
                LOGI(TPL_LOG " FDUMP tag=%c ev+0x%02X ptr=%p vec=%u", (k == 0) ? 'p' : 'q', (unsigned)off, (void*)pa, (unsigned)victim);
                // R47：把「每次命中都换新」的 5 个子对象也 dump 0x00..0xFC（伤害数值最可能在这里）
                if (k == 0) {
                    static const uint32_t kChild[5] = {0x10, 0x68, 0x80, 0x88, 0xA8};
                    for (int ci = 0; ci < 5; ++ci) {
                        uint64_t cp = 0;
                        if (!ReadPtrQuiet(reinterpret_cast<const char*>(pa) + kChild[ci], &cp) || !cp) continue;
                        const uintptr_t ca = static_cast<uintptr_t>(cp);
                        if (!ZMapped(ca, 0x100)) continue;
                        LOGI(TPL_LOG "  CHILD p+0x%02X -> %p", (unsigned)kChild[ci], (void*)ca);
                        for (uint32_t base = 0; base < 0x100; base += 0x20) {
                            const char* cb2 = reinterpret_cast<const char*>(ca + base);
                            uint32_t cw[8]; int cok = 0;
                            for (int i = 0; i < 8; ++i) { if (ReadU32Quiet(cb2 + i * 4, &cw[i])) ++cok; else cw[i] = 0xDEADBEEFu; }
                            if (cok == 8)
                                LOGI(TPL_LOG "   C%02X +%03X: %08X %08X %08X %08X %08X %08X %08X %08X",
                                     (unsigned)kChild[ci], (unsigned)base, (unsigned)cw[0], (unsigned)cw[1],
                                     (unsigned)cw[2], (unsigned)cw[3], (unsigned)cw[4], (unsigned)cw[5],
                                     (unsigned)cw[6], (unsigned)cw[7]);
                        }
                    }
                }
                for (uint32_t base = 0; base < 0x200; base += 0x20) {
                    const char* b = reinterpret_cast<const char*>(pa + base);
                    uint32_t w[8]; int ok = 0;
                    for (int i = 0; i < 8; ++i) { if (ReadU32Quiet(b + i * 4, &w[i])) ++ok; else w[i] = 0xDEADBEEFu; }
                    if (ok == 8)
                        LOGI(TPL_LOG "  F%c +%03X: %08X %08X %08X %08X %08X %08X %08X %08X",
                             (k == 0) ? 'p' : 'q', (unsigned)base, (unsigned)w[0], (unsigned)w[1],
                             (unsigned)w[2], (unsigned)w[3], (unsigned)w[4], (unsigned)w[5],
                             (unsigned)w[6], (unsigned)w[7]);
                }
            }
        }
        if (pn < 24) {
            const char* eb = reinterpret_cast<const char*>(ev);
            for (int k = 0; k < 2; ++k) {
                const uint32_t off = (k == 0) ? 0x20u : 0x30u;
                const char tag = (k == 0) ? 'p' : 'q';
                uint64_t ptr = 0;
                if (!ReadPtrQuiet(eb + off, &ptr) || !ptr) {
                    LOGW(TPL_LOG " ENT-PROBE n=%d tag=%c ev+0x%02X ptr FAIL", pn, tag, (unsigned)off);
                    continue;
                }
                const void* pv = reinterpret_cast<const void*>(ptr);
                if (!ValidPtrForRead(pv, 4)) {
                    LOGW(TPL_LOG " ENT-PROBE n=%d tag=%c ptr=%p 不合法（跳过）", pn, tag, (void*)ptr);
                    continue;
                }
                const char* pb = reinterpret_cast<const char*>(pv);
                uint32_t w[12]; int nOk = 0;
                for (int m = 0; m < 12; ++m) {
                    if (ReadU32Quiet(pb + 0x90 + m * 4, &w[m])) ++nOk; else w[m] = 0xDEADBEEFu;
                }
                LOGI(TPL_LOG " ENT-PROBE n=%d tag=%c addr=%p ok=%d/12 atk=%u vic=%u",
                     pn, tag, (void*)ptr, nOk, (unsigned)atk, (unsigned)victim);
                LOGI(TPL_LOG "   ENT %c +90=%08X +94=%08X +98=%08X +9C=%08X | +A0=%08X(HP?) +A4=%08X(ATK?) +A8=%08X +AC=%08X",
                     tag, (unsigned)w[0], (unsigned)w[1], (unsigned)w[2], (unsigned)w[3],
                     (unsigned)w[4], (unsigned)w[5], (unsigned)w[6], (unsigned)w[7]);
                LOGI(TPL_LOG "   ENT %c +B0=%08X +B4=%08X +B8=%08X(camp?) +BC=%08X",
                     tag, (unsigned)w[8], (unsigned)w[9], (unsigned)w[10], (unsigned)w[11]);
            }
        }

        // R36（2026-10-23）②：用 (id→实体) 表把 vic(+0x3C) 反查成实体指针，dump +0x90..+0xBC。
        //   判据：同一实体连续受伤时 +0xA0 应单调下降（真 HP）；+0xB8 应把「我方/怪物」分成两组。
        //   只读；表未命中就静默跳过（战斗早期解析器还没被调用过是正常的）。
        if (okV && victim) {
            static std::atomic<int> s_rsvVic{0}, s_rsvMiss{0};
            uintptr_t ent = 0;
            if (ResTableLookup(victim, &ent) && ent) {
                const int rn = s_rsvVic.fetch_add(1) + 1;
                if (rn <= 24 || (rn % 64) == 0) DumpEnt12("VIC", rn, ent, atk, victim);
            } else {
                const int mn = s_rsvMiss.fetch_add(1) + 1;
                if (mn <= 6 || (mn % 128) == 0)
                    LOGW(TPL_LOG " VIC-MISS n=%d vic=%u 表内无记录（RSV 命中数=%d）",
                         mn, (unsigned)victim, s_rsvVic.load());
            }
        }
    }


    // R27（2026-10-22）fail-open：关键字段读不到（ev 非法 / 3.3.0 结构比 0x40 短）⇒ 不做任何归属判定，
    //   打一行日志后直接走原函数。宁可漏保护，也绝不再拿垃圾值/metadata 走进链路（上轮 SIGSEGV 教训）。
    if (!okT || !okK) {
        static std::atomic<int> nEvtBad{0};
        if (nEvtBad.fetch_add(1) < 8)
            LOGW(TPL_LOG " INV-EVT 字段不可读(okT=%d okK=%d) ev=%p — fail-open 直通原函数",
                 (int)okT, (int)okK, ev);
        if (g_origInv) g_origInv(self, ev, a2, a3, mth);
        return;
    }
    // victim 读失败（结构更短）⇒ 置 0 继续：vicMine 必为 false ⇒ 天然不拦截（fail-open）
    if (!okV) victim = 0;

    const bool invOn  = g_inv.load();
    const int  mul    = g_dmgMul.load();

    // R31：互攻判据（某 id 既当攻击方又当受击方 ⇒ 我方）；R29 的「被>=3 攻击方打」已停用（会误学怪物）。
    if (isDmg && okV && okK) ObserveExchange(atk, victim);

    uint32_t me = 0;
    const char* meSrc = "none";
    if (invOn) {   // R20：伤害倍率已 no-op，链只在无敌开启时走（全关态不碰游戏堆）
        if (ResolvePlayerActorId(&me)) { meSrc = "chain"; }
        else { me = 0; meSrc = "none"; }   // R5f: stale id cache no longer counts as the player
    }

    // R5f identity rule (see IsMineActor): chain id | own(local player id) | 0x104-learned set.
    //   R5e's own/vow based owner comparison is gone - own is constant 5 on both sides。
    if (isDmg && !g_reenter.load()) {
        const uint32_t team = g_meTeam.load();

        // R29：R8 的「对攻关系表」与 R24 的 SideResetHitStats 不再参与归属（IsMineActor 只看
        //   chain/own/id 集），已从热路径移除，避免误导排查。
        (void)team;

        const uint32_t chainId = me;                   // 0 = chain did not resolve this time
        const char* whyV = nullptr;
        const char* whyA = nullptr;
        const bool vicMine = okV && IsMineActor(victim, chainId, &whyV);
        const bool atkMine = okK && IsMineActor(atk, chainId, &whyA);

        // 诊断（限流）：无敌开启时把伤害事件的归属判定打进日志；
        //   未判为我方的受击事件优先记录（这就是漏保护的失败形态）。
        if (invOn) {
            static std::atomic<int> nInvEvt{0}, nInvLog{0};
            // R11 lesson: the old cap (40) hid the interesting events (the unprotected victim).
            //   Always log a compact line when the victim is NOT recognised as mine (that IS the
            //   failure mode we hunt); keep a small budget for the rest.
            const bool interesting = !vicMine;
            if ((interesting || nInvEvt.fetch_add(1) < 24) && nInvLog.fetch_add(1) < 400) {
                LOGI(TPL_LOG " INV-EVT type=0x%X atk=%u vic(+0x3C)=%u team=%u set=%d me=%u(%s) atkMine=%d vicMine=%d",
                     (unsigned)type, (unsigned)atk, (unsigned)victim,
                     (unsigned)team, MeIdCount(), (unsigned)me, meSrc, (int)atkMine, (int)vicMine);
            }
        }

        // ① 无敌已停用（R32 no-op 止血）：R29/R31 的 id 归属规则被证会把怪物学成我方（用户实测
        //   「个别怪物也变无敌」），在拿到可靠 camp 判据前一律不拦，只在前 3 次打一行说明后照常调原函数。
        // ① 无敌（R39 2026-10-25 启用）：actor id 分带判据
        //   受击方 ≤ kIdPlayerMax(12) 且 攻击方 ≥ kIdMonsterMin(22) ⇒ 判为「怪物打玩家」⇒ 不调原函数。
        //   方向不对或任一 id 落死区 13..21 ⇒ 不判定，照常调原函数（fail-open）。
        //   R32 的「学习式 id 集」已废止：它会把 22 这类怪物学成我方（用户实测小怪也变无敌）。
        // R51（2026-10-25）：新关卡实测怪物 id 会落到 17/21/24/27（<22），旧「攻击方≥22」规则会漏保护。
        //   改为**只看受击方**：victim ∈ [1,12] 即玩家侧 ⇒ 拦截（不要求攻击方数值）。
        //   实测依据：`atk=17 vic=5`、`atk=21 vic=5` 漏拦；而 `atk=5 vic=17/21`（玩家打怪）不受影响。
        if (invOn && okV && victim >= 1 && victim <= kIdPlayerMax && atk != victim) {
            static std::atomic<int> nBlk{0};
            const int c = nBlk.fetch_add(1) + 1;
            if (c <= 12 || (c % 64) == 0)
                LOGI(TPL_LOG " 无敌拦截 n=%d type=0x%X atk=%u vic=%u（不调原函数）",
                     c, (unsigned)type, (unsigned)atk, (unsigned)victim);
            return;
        }
        if (invOn && !vicMine) {
            static std::atomic<int> nMiss{0};
            if (nMiss.fetch_add(1) < 6)
                LOGW(TPL_LOG " 无敌未拦 type=0x%X atk=%u vic=%u（id 带外，fail-open）",
                     (unsigned)type, (unsigned)atk, (unsigned)victim);
        }

        // ② 伤害倍率（R41 2026-10-25）：玩家打怪事件 ⇒ **重复调用原函数 reps 次**，每次=原生一击，
        //   总伤害 ≈ 原生 × reps。判据与无敌正相反：攻击方 1..12（玩家），受击方 ≥22（怪物）。1=关。
        //   上限 20 次（滑块 >20 按 20 计），避免单帧重复过多打爆帧时间。
        //   依据：R39 已实证「不调原函数 ⇒ 伤害不结算」⇒ 原函数就是伤害结算入口。
        // ③ 伤害倍率（R49 2026-10-25）：用户实测「重复调用确实扣血更多」⇒ 恢复多段结算。
        //   玩家打怪事件（攻击方 ≤12、受击方 ≥22）时把原函数调用 reps 次；第 2 次起用**事件副本**
        //   （浅拷贝 0x100 字节），避免游戏把同一事件对象当「已处理」丢弃。reps 上限 20。
        if (mul > 1 && okV && okK && atk >= 1 && atk <= kIdPlayerMax && victim >= kIdMonsterMin) {
            int reps = mul;
            if (reps > 20) reps = 20;
            // R52：21 个下游探针已证明抓不到伤害数值，正式包不装（探针会插 21 个热函数，影响帧率）
            static thread_local unsigned char s_evCopy[0x100];
            if (g_origInv) {
                g_origInv(self, ev, a2, a3, mth);
                for (int i = 1; i < reps; ++i) {
                    memcpy(s_evCopy, ev, sizeof(s_evCopy));
                    g_origInv(self, s_evCopy, a2, a3, mth);
                }
            }
            static std::atomic<int> nRep2{0};
            const int c = nRep2.fetch_add(1) + 1;
            if (c <= 10 || (c % 128) == 0)
                LOGI(TPL_LOG " 倍率(多段) n=%d atk=%u vic=%u reps=%d（含事件副本）",
                     c, (unsigned)atk, (unsigned)victim, reps);
            return;
        }
    }

    if (g_origInv) g_origInv(self, ev, a2, a3, mth);
}

// R19: R9/R10 的 HP 差分/补扣后处理（Hook_InvPostDmg）已删除，禁止复活。

// ============================================================================
//  R36（2026-10-23）只读探针 hook 本体
//  全部 pass-through：先/后调原函数，入参原样、内存不写、返回值只用原函数的。
//  日志一律限流（首 N 次 + 每 256/64 次），避免热路径刷 logcat 拖慢 UnityMain。
// ============================================================================
typedef void*    (*FnResolve_t)(void* mgr, uint32_t id);
typedef uint64_t (*FnHpW2_t)(void* a0, uint64_t a1);
typedef uint64_t (*FnHpW3_t)(void* a0, void* a1, void* a2);
typedef uint64_t (*FnHpW5_t)(void* a0, void* a1, void* a2, void* a3, void* a4);

static FnResolve_t g_origResolve = nullptr;
static FnHpW2_t    g_origHpWa    = nullptr;
static FnHpW3_t    g_origHpWb    = nullptr;
static FnHpW5_t    g_origHpWc    = nullptr;

// ① 0x16de0acc：id→实体解析。判据：ret 非空且 id 与我们已知的 actor id 域一致；
//   同时把 (id→ret) 记进 16 槽表，供 Hook_Inv 的受击分支反查。
static void* Hook_Resolve(void* mgr, uint32_t id) {
    void* ret = g_origResolve ? g_origResolve(mgr, id) : nullptr;
    if (ret) ResTablePut(id, reinterpret_cast<uintptr_t>(ret));
    static std::atomic<int> s_calls{0}, s_log{0};
    const int n = s_calls.fetch_add(1) + 1;
    if ((n <= 8 || (n % 256) == 0) && s_log.fetch_add(1) < 40) {
        LOGI(TPL_LOG " RSV n=%d mgr=%p id=%u ret=%p", n, mgr, (unsigned)id, ret);
        if (ret && (n <= 8)) DumpEnt12("RSV", n, reinterpret_cast<uintptr_t>(ret), 0, id);
    }
    return ret;
}

// ② 0x5a5a400：x0=self，w1=数值（静态分析：与 [+0x9c] 取大后按系数扣减，写 +0xA4/+0x98）
static uint64_t Hook_HpWa(void* a0, uint64_t a1) {
    static std::atomic<int> s_calls{0};
    const int n = s_calls.fetch_add(1) + 1;
    if (n <= 12)
        LOGI(TPL_LOG " HPW-A n=%d x0=%p x1=%#lx (w1=%u)", n, a0, (unsigned long)a1,
             (unsigned)(a1 & 0xFFFFFFFFu));
    return g_origHpWa ? g_origHpWa(a0, a1) : 0;
}

// ③ 0x1aa964b0：x0/x1/x2 三个入参（静态分析：x19=x2、x20=x1、x21=x0，对称两块写 +0xA0）
static uint64_t Hook_HpWb(void* a0, void* a1, void* a2) {
    static std::atomic<int> s_calls{0};
    const int n = s_calls.fetch_add(1) + 1;
    if (n <= 12) LOGI(TPL_LOG " HPW-B n=%d x0=%p x1=%p x2=%p", n, a0, a1, a2);
    return g_origHpWb ? g_origHpWb(a0, a1, a2) : 0;
}

// ④ 0x1aa95b14：入口即 cbz x1 / cbz x2，且 mov x21,x4 ⇒ 读 x0..x4 五个入参
static uint64_t Hook_HpWc(void* a0, void* a1, void* a2, void* a3, void* a4) {
    static std::atomic<int> s_calls{0};
    const int n = s_calls.fetch_add(1) + 1;
    if (n <= 12) LOGI(TPL_LOG " HPW-C n=%d x0=%p x1=%p x2=%p x3=%p x4=%p", n, a0, a1, a2, a3, a4);
    return g_origHpWc ? g_origHpWc(a0, a1, a2, a3, a4) : 0;
}

// ============================================================================
//  R22（2026-10-07）：伤害倍率 = 玩家「伤害加成属性」写回（池鸳 attack-writeback 机制，自研实现）
//
//  R21 失败的根因（本轮逐条反汇编池鸳 libcymod 后定案 —— 不是路线问题，是实现漏了两级）：
//    ① 记录解析漏了 5 级指针链：R21 直接读 *(P+0xb8)（P = *(base+0x1d749418)）⇒ 恒为 0；
//       池鸳 0x4f684 是从同一个根走 +0x08,+0x18,+0x10,+0x78,+0x00 五级后才 +0xb8 取 R。
//    ② selfSet/selfGet 取错来源：不是 R+0x08/R+0x10，而是 getComponent(R,0x1a) 派生出来的两个对象。
//    ③ actor id 在 R+0x38（不是 R+0x20）。
//  池鸳语义（libcymod 反汇编，RVA 全部为 libil2cpp）：
//    0x4e45c（池鸳 handler）→ 0x4f684(0x1d749418) 走 5 级 → +0xB8 = R；[3.3.0 该链弃用]
//    0x4f118(R, out)：actor = *(u32*)(R+0x38)（为 0 即放弃）；
//        comp = getComponent(R, 0x1a, 0)（槽 0x130f50 = base+0xd937a30）；
//        C = *(comp+0xc8)；out+0x08 = C（写对象 selfSet）；out+0x10 = *(C+0x20)（读对象 selfGet）；
//    0x4f350(delta)：method = *(base+0x1d9299f8)；
//        get(selfGet, out2, propId=2) → cvt(out2+0x20, out2+0x28, 0) = 当前加成值；
//        set(selfSet, propId=2, method, mode=2, delta)；再 get 回读确认。
//    池鸳把滑块值 clamp 到 2000（0x7d0）后传 value-1，即「1 = 关闭」。
//  实测（2026-10-07 MuMu pid 26844，adb 只读步进）：修正后的链解析成功 ⇒
//    R = 0x780C22EC60、*(u32*)(R+0x38) = 5（我方玩家）⇒ 这条链在当前状态下是活的。
//
//  我们的差异（自研，不照抄池鸳）：
//    ① delta = 目标 - 当前（自校正），不是池鸳「上一次写入值」的缓存；
//    ② 解析结果缓存复用：战斗中链根可能为空（池鸳同样只写一次），解析失败时回落到上次成功的记录；
//    ③ 写前 actor 必须是我方；写后回读不符 ⇒ 永久停写（fail-closed，绝不盲写）；
//    ④ 只用定长裸读 + 有效性检查；无深读、无扫描、无信号守卫。
//  R19/R20 已否决路线（禁止复活）：0x104 事件记录写回、DmgSettle、VicTail/VicMid 深读、HP 探测。
// ============================================================================
enum : uintptr_t {
    kRvaAttrRecordRoot = 0x1d749418,   // 玩家记录链根（与 actor 链同根，RVA 空间）
    kRvaAttrGetComp    = 0xd937a30,    // getComponent(R, 0x1a, 0) → 属性组件
    kRvaAttrMethod     = 0x1d9299f8,   // 全局 method 对象槽
    kRvaAttrGet        = 0x10a02394,   // 读属性（out 结构，值在 +0x20）
    kRvaAttrCvt        = 0x13f27c60,   // 结构 → double
    kRvaAttrSet        = 0xddc8600,    // (self, propId, method, mode, double delta)
};
// 记录链：池鸳 0x4f684 的五级 + 调用方的 0xb8（R21 漏掉前五级 ⇒ 恒空）
static const uintptr_t kAttrChain[6] = {0x08, 0x18, 0x10, 0x78, 0x00, 0xb8};
static const uint32_t kAttrPropId = 2;
static const uint32_t kAttrMode   = 2;
// R24：2000 是池鸳自家滑块映射的 clamp，不是游戏端上限（0xddc8600 setter 反汇编无 2000 校验）。
// 我们仍防呆，但允许更高等级的伤害加成落到属性上。
static const double   kAttrMaxBonus = 9900.0;

static std::atomic<uintptr_t> g_rvaOrigin{0};        // libil2cpp RVA 原点（一次解析后缓存）
static std::atomic<bool>      g_dmgWriteDead{false};  // 写后校验失败 ⇒ 永久停写（fail-closed）
static std::atomic<bool>      g_dmgEverApplied{false};// 是否曾成功写入（关倍率时要回 0）
static std::atomic<unsigned>  g_dmgApplyCount{0};     // 成功写回次数
// R25：不再缓存 DmgAttrCtx；重新挑战/切战斗会释放旧对象，旧缓存就是本轮 SIGSEGV 根因。

// 用户态指针有效性：池鸳同款 x<0x10000 拒绝 + 8 字节对齐 + 高位上限（防止把垃圾当对象调）
static inline bool ValidUserPtr(uint64_t p) {
    return p >= 0x10000ULL && (p & 7ULL) == 0 && p < 0x0000800000000000ULL;
}

struct DmgAttrCtx { uint64_t rec, selfSet, selfGet, method; uint32_t actor; };

static long long MonoMs() {
    struct timespec ts;
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<long long>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

// 逐级裸读解析玩家记录（池鸳 0x4f684 + 0x4f118 同口径）；
//   任何一级不合法即返回 false（调用方限流重试 / 回落缓存，不写内存）。
//   注：getComponent 是本段唯一一次「进入游戏代码」的调用，与池鸳在同一个钩子上下文里做，
//   调用前已把 R 逐级校验过；返回值再校验一次才使用。
// 返回值：0 = 成功；非 0 = 失败的级号（落日志用，省得下次只能看到笼统的「未就绪」）
//   1=根指针空 2=链第 i 级断 3=链尾非法 4=actor=0 5=getComponent 失败
//   6=*(comp+0xc8) 非法 7=*(C+0x20) 非法 8=method 槽为空
static int DmgAttrResolve(uintptr_t org, DmgAttrCtx* c) {
    uint64_t p = 0, method = 0;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(org + kRvaAttrRecordRoot), &p) || !ValidUserPtr(p)) return 1;
    for (size_t i = 0; i < sizeof(kAttrChain) / sizeof(kAttrChain[0]); ++i) {
        if (!ValidUserPtr(p)) return 2;
        uint64_t nxt = 0;
        if (!ReadPtrQuiet(reinterpret_cast<const char*>(p) + kAttrChain[i], &nxt)) return 2;
        p = nxt;
    }
    if (!ValidUserPtr(p)) return 3;
    const uint64_t rec = p;
    uint32_t actor = 0;
    if (!ReadU32Quiet(reinterpret_cast<const void*>(rec + 0x38), &actor) || !actor) return 4;

    typedef void* (*FnGetComp)(void*, uint32_t, void*);
    FnGetComp getComp = reinterpret_cast<FnGetComp>(org + kRvaAttrGetComp);
    const uint64_t comp = reinterpret_cast<uint64_t>(getComp(reinterpret_cast<void*>(rec), 0x1a, nullptr));
    if (!ValidUserPtr(comp)) return 5;
    uint64_t selfSet = 0, selfGet = 0;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(comp + 0xc8), &selfSet) || !ValidUserPtr(selfSet)) return 6;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(selfSet + 0x20), &selfGet) || !ValidUserPtr(selfGet)) return 7;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(org + kRvaAttrMethod), &method) || !ValidUserPtr(method)) return 8;
    c->rec = rec; c->selfSet = selfSet; c->selfGet = selfGet; c->method = method; c->actor = actor;
    return 0;
}

// 读当前加成值（getter → 结构 → cvt 转 double）；任何一步不合法返回 false
//   self 只传「读对象 selfGet」（池鸳 0x4f350 同口径：getter 只用在 selfGet 上，
//   selfSet 是另一个对象，用 getter 去探它反而是多余的崩溃面，已删）。
static bool DmgAttrRead(uintptr_t org, uint64_t self, double* outVal) {
    typedef bool   (*FnGet)(void*, void*, uint32_t);
    typedef double (*FnCvt)(uint64_t, uint64_t, void*);
    if (!ValidUserPtr(self)) return false;
    alignas(16) unsigned char buf[0x80];
    std::memset(buf, 0, sizeof(buf));
    FnGet getFn = reinterpret_cast<FnGet>(org + kRvaAttrGet);
    if (!getFn(reinterpret_cast<void*>(self), buf, kAttrPropId)) return false;
    uint64_t v0 = 0, v1 = 0;
    std::memcpy(&v0, buf + 0x20, 8);
    std::memcpy(&v1, buf + 0x28, 8);
    FnCvt cvtFn = reinterpret_cast<FnCvt>(org + kRvaAttrCvt);
    const double v = cvtFn(v0, v1, nullptr);
    if (!std::isfinite(v) || v > 1e12 || v < -1e12) return false;   // NaN/异常值一律拒绝
    *outVal = v;
    return true;
}

// 把「伤害加成」同步到 desired（0 = 还原）。全程 fail-closed：解析不到/校验不过只打日志，不写。
// R25：解析失败绝不复用旧记录。重新挑战/换战斗会释放旧 rec/selfSet/selfGet；链断后继续调
// 游戏 getter/setter 就是 UnityMain SIGSEGV（fault 0x38）。宁可在过渡期不加成，也不用已释放对象。
static void DmgAttrSync(double desired) {
    if (g_dmgWriteDead.load()) return;
    if (desired <= 0.0 && !g_dmgEverApplied.load()) return;   // 从未写过 + 要 0 ⇒ 完全不动游戏内存
    const uintptr_t org = g_rvaOrigin.load();
    if (!org) return;

    DmgAttrCtx c = {};
    const int stage = DmgAttrResolve(org, &c);
    if (stage == 0) {
        if (g_dmgEverApplied.exchange(true)) {
            if (RateLimitLog("dmgResolveOk", 30))
                LOGI(TPL_LOG " 伤害倍率: 记录重新解析成功 actor=%u rec=%p selfSet=%p selfGet=%p method=%p",
                     (unsigned)c.actor, reinterpret_cast<void*>(c.rec), reinterpret_cast<void*>(c.selfSet),
                     reinterpret_cast<void*>(c.selfGet), reinterpret_cast<void*>(c.method));
        }
    } else {
        // R25：解析失败（战斗释放/过场/链根清空）时必须完全停手。
        // fault addr 0x38 的 SIGSEGV 就是旧 selfGet 继续传进游戏 getter/setter。
        if (RateLimitLog("dmgResolve", 5))
            LOGW(TPL_LOG " 伤害倍率: 玩家记录未就绪 stage=%d（本轮跳过，会自动重试）", stage);
        return;
    }
    // 铁律：记录里的 actor 必须是我方，否则拒写（防止把怪物记录当成玩家属性写）
    if (!IsMineActor(c.actor, 0, nullptr)) {
        if (RateLimitLog("dmgActor", 5)) LOGW(TPL_LOG " 伤害倍率: 记录 actor=%u 非我方，拒写", (unsigned)c.actor);
        return;
    }

    double cur = 0.0;
    if (!DmgAttrRead(org, c.selfGet, &cur)) {
        if (RateLimitLog("dmgRead", 5)) LOGW(TPL_LOG " 伤害倍率: 读属性失败（actor=%u），本轮不写", (unsigned)c.actor);
        return;
    }

    // R23 诊断：每 2s 一行状态 —— 「游戏会不会自己把加成清回 0」「滑块值有没有真的跟过去」
    //   全看这一行（R22 的 200ms 限频下，游戏自己清 0 时我们只看到零星几条 OK）。
    static std::atomic<long long> s_lastState{0};
    const long long nowStateMs = MonoMs();
    if (nowStateMs - s_lastState.load() >= 2000) {
        s_lastState.store(nowStateMs);
        LOGI(TPL_LOG " 伤害倍率: 状态 mul=%d cur=%.3f target=%.3f actor=%u",
             g_dmgMul.load(), cur, desired, (unsigned)c.actor);
    }

    const double delta = desired - cur;
    if (delta < 1e-6 && delta > -1e-6) { g_dmgEverApplied.store(true); return; }   // 已经等于目标

    // 首写/首次读回都把「当前值」落日志：属性单位（百分比 / 小数 / 原始点数）以它为校准依据
    if (g_dmgApplyCount.load() == 0)
        LOGI(TPL_LOG " 伤害倍率: 写前 cur=%.6f target=%.6f delta=%.6f (actor=%u)",
             cur, desired, delta, (unsigned)c.actor);

    typedef double (*FnSet)(void*, uint32_t, uint64_t, uint32_t, double);
    FnSet setFn = reinterpret_cast<FnSet>(org + kRvaAttrSet);
    setFn(reinterpret_cast<void*>(c.selfSet), kAttrPropId, c.method, kAttrMode, delta);

    double after = 0.0;
    if (!DmgAttrRead(org, c.selfGet, &after) || std::fabs(after - desired) > 1e-3) {
        g_dmgWriteDead.store(true);   // 写后校验失败：永久停写，绝不反复盲写
        LOGW(TPL_LOG " 伤害倍率: 写后校验失败 actor=%u cur=%.6f delta=%.6f after=%.6f（已永久停写）",
             (unsigned)c.actor, cur, delta, after);
        return;
    }

    g_dmgEverApplied.store(true);
    const unsigned n = g_dmgApplyCount.fetch_add(1) + 1;
    if (n <= 6 || n % 60 == 0)
        LOGI(TPL_LOG " 伤害倍率: 属性写回 OK #%u actor=%u %.6f -> %.6f (delta=%.6f)",
             n, (unsigned)c.actor, cur, after, delta);
}

// 菜单滑块是「倍率」（1..100，1=关）；池鸳那个属性是「加成值」（它的滑块默认 3000、clamp 2000）。
//   映射：bonus = (倍率 - 1) × 100（百分比口径），上限 9900（防呆，非游戏端 clamp）。
//   单位若不符，首写日志里的 cur/after 就是校准依据 —— 下一轮只改这一个常量。
static double DmgBonusFor(int mul) {
    if (mul <= 1) return 0.0;
    double v = static_cast<double>(mul - 1) * 100.0;
    if (v > kAttrMaxBonus) v = kAttrMaxBonus;
    return v;
}

// 每帧站点作为触发。R23：**去掉 200ms 限频** —— 实测（R22 记录仪 16:07–16:08）游戏会自己把
//   加成清回 0（#3 写 1600 → 8s 后 #4 读到 cur=0），限频会让加成只存在于 1/12 的帧里，
//   这正是「伤害不跟着滑块走」的直接原因。池鸳也是每帧都跑同一套（resolve + get + 必要时 set），
//   真正的写只在 cur != target 时发生，所以稳态下每帧只多一次 get。调用次数由状态日志控制。
static void DmgSyncTick() {
    const int mul = g_dmgMul.load();
    if (mul <= 1 && !g_dmgEverApplied.load()) return;   // 关着且从没写过 ⇒ 零开销
    DmgAttrSync(DmgBonusFor(mul));
}

// ② 伤害倍率：入口 s0 即倍率入参，乘滑块后透传（x1 原样转发，语义不变）
// ②b 该站点 R2/R3 实测是每帧 Update(dt)（入参 0.030~0.061 = 帧间隔），乘 dt 只会搞乱时序。
// R22：本站点只当「每帧触发」，不改入参；真正的伤害倍率走属性写回（DmgSyncTick）。
static void Hook_Dmg(void* self, float mult, void* mth) {
    if (g_origDmg) g_origDmg(self, mult, mth);
    (void)self; (void)mult; (void)mth;
    DmgSyncTick();
}

// R19: R14 的 DmgSettle 只读诊断钩已删除（R14b 定案：它是帧时间累积器，不是伤害结算）。

// 定怪：BehaviorManager 是全局单例（调用者自全局取 x0），Tick(self, entity, ...) 的 entity 才是「谁的行为」。
// v1（2026-10-06）：entity 类名可读且像玩家 ⇒ 不冻；其余跳过原函数（怪物行为停摆）。
// entity 类名前 12 次进日志，用于按实测类名收窄过滤。
static const char* SafeClassName(const char* tag, void* obj) {
    if (!obj) return nullptr;
    // R7: 本包 xdl+dynsym 解析 il2cpp 核心符号持续失败（R6 实测刷屏 347 行，且每次都是
    //   一次 xdl_open + 全量符号解析，落在每帧热路径上）。熔断：前 24 次允许重试
    //   （覆盖「首次调用早于 libil2cpp 映射完成」），之后不再进 Load。
    if (!GetApi().ready) {
        static std::atomic<int> nTry{0};
        if (nTry.fetch_add(1) >= 24) {
            static std::atomic<int> nFuse{0};
            if (nFuse.fetch_add(1) == 0)
                LOGW(TPL_LOG " %s: il2cpp api 不可用，类名过滤熔断（后续不再重试）", tag);
            return nullptr;
        }
        if (!GetApi().Load(TPL_TARGET_LIB)) return nullptr;
    }
    void* k = GetApi().object_class(obj);
    if (!k) return nullptr;
    const char* n = GetApi().class_name(k);
    if (!n || !*n) return nullptr;
    static std::atomic<int> nLog{0};
    if (nLog.fetch_add(1) < 12) LOGI(TPL_LOG " %s 候选类名: %s", tag, n);
    return n;
}

// ---- MoveSpeed entry hit diagnostic (R5f) -----------------------------------
//   The three 12B inline stubs (apply write site + two reset write sites) showed ZERO
//   hits over two 30s windows in R5e. This probe only counts, it changes nothing:
//   entry hits but no write-site hits => the method runs but bails out early (component
//   lookup failed); no entry hits either => the whole SetMoveSpeedRatio subsystem is
//   not on the player's movement path and the station has to be replaced.
typedef void (*FnMoveEntry_t)(void* self, void* a1, void* mth);
static FnMoveEntry_t g_origMoveEntry = nullptr;
static void Hook_MoveEntry(void* self, void* a1, void* mth) {
    const unsigned n = g_moveEntryHits.fetch_add(1);
    if (n < 5) {
        uint32_t ru = 0;
        float ratio = -1.0f;
        if (self && ReadU32Quiet(reinterpret_cast<const char*>(self) + 0x64, &ru)) ratio = U32AsF(ru);
        LOGI(TPL_LOG " MoveEntry hit #%u self=%p ratio(+0x64)=%.4f mul=%d",
             n, self, ratio, g_moveMul.load());
    }
    if (g_origMoveEntry) g_origMoveEntry(self, a1, mth);
}

static void Hook_Freeze(void* self, void* a1, void* mth) {
    // R32（2026-10-22）只读探针：在定怪钩的 a1 上找实体字段（HP=+0xA0? 攻击=+0xA4? camp=+0xB8?）。
    //   前 12 次调用（不限开关）打印 a1 的 +0x00..+0x40 与 +0x90..+0xC0；只读、失败打 FAIL、不影响冻结逻辑。
    {
        static std::atomic<int> s_fe{0};
        const int fn = s_fe.fetch_add(1);
        if (fn < 12) {
            uint32_t ru = 0; bool okId = false;
            if (a1) okId = ReadU32Quiet(reinterpret_cast<const char*>(a1) + 0x18, &ru);
            LOGI(TPL_LOG " FREEZE-ENT #%d self=%p a1=%p id(+0x18)=%u ok=%d freeze=%d",
                 fn, self, a1, (unsigned)ru, (int)okId, (int)g_freeze.load());
            if (a1 && ValidPtrForRead(a1, 4)) {
                const char* ab = reinterpret_cast<const char*>(a1);
                for (int row = 0; row < 4; ++row) {
                    uint32_t v[4]; int nOk = 0;
                    for (int c = 0; c < 4; ++c) {
                        if (ReadU32Quiet(ab + row * 0x10 + c * 4, &v[c])) ++nOk; else v[c] = 0xDEADBEEFu;
                    }
                    LOGI(TPL_LOG "   FE-1 #%d +%02X=%08X +%02X=%08X +%02X=%08X +%02X=%08X ok=%d/4",
                         fn, row * 0x10, (unsigned)v[0], row * 0x10 + 4, (unsigned)v[1],
                         row * 0x10 + 8, (unsigned)v[2], row * 0x10 + 12, (unsigned)v[3], nOk);
                }
                for (int row = 0; row < 3; ++row) {
                    uint32_t v[4]; int nOk = 0;
                    for (int c = 0; c < 4; ++c) {
                        const uint32_t off = 0x90 + row * 0x10 + c * 4;
                        if (ReadU32Quiet(ab + off, &v[c])) ++nOk; else v[c] = 0xDEADBEEFu;
                    }
                    LOGI(TPL_LOG "   FE-2 #%d +%02X=%08X +%02X=%08X +%02X=%08X +%02X=%08X ok=%d/4 row=%d",
                         fn, 0x90 + row * 0x10, (unsigned)v[0], 0x94 + row * 0x10, (unsigned)v[1],
                         0x98 + row * 0x10, (unsigned)v[2], 0x9C + row * 0x10, (unsigned)v[3], nOk, row);
                }

                // R33（2026-10-22）顺藤摸瓜：a1 的 +0x20 / +0x98 / +0xA8 三个 64 位指针各再 dump
                //   前 0x40（16 word）+ 0x90..0xC0（12 word），找真正的实体对象（HP=+0xA0/攻击=+0xA4/camp=+0xB8）。
                //   纯只读，非法指针跳过、读失败打 FAIL，不影响冻结/无敌逻辑。
                static const uint32_t kPtrOffs[3] = { 0x20u, 0x98u, 0xA8u };
                for (int pi = 0; pi < 3; ++pi) {
                    const uint32_t po = kPtrOffs[pi];
                    uint64_t p = 0;
                    if (!ReadPtrQuiet(ab + po, &p) || !p) {
                        LOGW(TPL_LOG "   FREEZE-PTR #%d ptr%d(a1+0x%02X) 读失败(FAIL)", fn, pi, (unsigned)po);
                        continue;
                    }
                    const void* pv2 = reinterpret_cast<const void*>(p);
                    if (!ValidPtrForRead(pv2, 4)) {
                        LOGW(TPL_LOG "   FREEZE-PTR #%d ptr%d(a1+0x%02X)=%p 不合法（跳过）", fn, pi, (unsigned)po, (void*)p);
                        continue;
                    }
                    LOGI(TPL_LOG "   FREEZE-PTR #%d ptr%d(a1+0x%02X)=%p", fn, pi, (unsigned)po, (void*)p);
                    const char* pb = reinterpret_cast<const char*>(pv2);
                    for (int row = 0; row < 4; ++row) {
                        uint32_t v[4]; int nOk = 0;
                        for (int c = 0; c < 4; ++c) {
                            if (ReadU32Quiet(pb + row * 0x10 + c * 4, &v[c])) ++nOk; else v[c] = 0xDEADBEEFu;
                        }
                        LOGI(TPL_LOG "     FP-1 #%d ptr%d +%02X=%08X +%02X=%08X +%02X=%08X +%02X=%08X ok=%d/4",
                             fn, pi, row * 0x10, (unsigned)v[0], row * 0x10 + 4, (unsigned)v[1],
                             row * 0x10 + 8, (unsigned)v[2], row * 0x10 + 12, (unsigned)v[3], nOk);
                    }
                    for (int row = 0; row < 3; ++row) {
                        uint32_t v[4]; int nOk = 0;
                        for (int c = 0; c < 4; ++c) {
                            const uint32_t off = 0x90 + row * 0x10 + c * 4;
                            if (ReadU32Quiet(pb + off, &v[c])) ++nOk; else v[c] = 0xDEADBEEFu;
                        }
                        LOGI(TPL_LOG "     FP-2 #%d ptr%d +%02X=%08X +%02X=%08X +%02X=%08X +%02X=%08X ok=%d/4 row=%d",
                             fn, pi, 0x90 + row * 0x10, (unsigned)v[0], 0x94 + row * 0x10, (unsigned)v[1],
                             0x98 + row * 0x10, (unsigned)v[2], 0x9C + row * 0x10, (unsigned)v[3], nOk, row);
                    }
                }
            } else {
                LOGW(TPL_LOG "   FREEZE-ENT #%d a1=%p 不可读（FAIL）", fn, a1);
            }
        }
    }
    if (g_freeze.load()) {
        const char* n = SafeClassName("定怪 entity", a1);
        bool isPlayer = n && (std::strstr(n, "Avatar") || std::strstr(n, "Player") ||
                              std::strstr(n, "Hero")   || std::strstr(n, "Agent"));
        if (!isPlayer) {
            static std::atomic<int> n2{0};
            if (n2.fetch_add(1) < 10) LOGI(TPL_LOG " 定怪拦截 BehaviorManager.Tick");
            return;   // 跳过更新 ⇒ 该实体行为不再推进
        }
    }
    if (g_origFreeze) g_origFreeze(self, a1, mth);
}

// R34（2026-10-22）实体 getter 只读 pass-through 钩：x0 就是实体对象，我们把它的
//   关键字段（HP=+0xA0 / 攻击=+0xA4 / 阵营=+0xB8 / canBattle=+0x99）打出来，然后原样调原函数返回。
//   绝不改返回值、绝不写内存；读失败只打 FAIL，不影响游戏逻辑。
static void EntDumpImpl(const char* tag, void* self, std::atomic<int>* ctr, int limit) {
    if (!self) return;
    const char* sb = reinterpret_cast<const char*>(self);
    uint32_t hp = 0, atk = 0, camp = 0, cb = 0;
    const bool b1 = ReadU32Quiet(sb + 0xA0, &hp);
    const bool b2 = ReadU32Quiet(sb + 0xA4, &atk);
    const bool b3 = ReadU32Quiet(sb + 0xB8, &camp);
    const bool b4 = ReadU32Quiet(sb + 0x99, &cb);
    if (!(b1 && b2 && b3 && b4)) {
        LOGW(TPL_LOG " %s self=%p 读失败 b=%d%d%d%d", tag, self, (int)b1, (int)b2, (int)b3, (int)b4);
        return;
    }
    const int n = ctr->fetch_add(1);
    if (n < limit)
        LOGI(TPL_LOG " %s self=%p hp(+0xA0)=%u atk(+0xA4)=%u camp(+0xB8)=%u(0x%X) cb(+0x99)=%u",
             tag, self, (unsigned)hp, (unsigned)atk, (unsigned)(camp & 0xFF), (unsigned)(camp & 0xFFFFFFFF),
             (unsigned)(cb & 0xFF));
}
static std::atomic<int> g_entHpN{0}, g_entAtkN{0}, g_entCampN{0};

static uint32_t Hook_EntHp(void* self) {
    EntDumpImpl("ENT-HP", self, &g_entHpN, 24);
    return g_origEntHp ? g_origEntHp(self) : 0u;
}
static uint32_t Hook_EntAtk(void* self) {
    EntDumpImpl("ENT-ATK", self, &g_entAtkN, 12);
    const uint32_t v = g_origEntAtk ? g_origEntAtk(self) : 0u;
    if (!self) return v;
    // R40（2026-10-25）：倍率 = 对「玩家侧实体」的攻击力返回值放大（不写任何游戏内存）。
    //   actor id 取自 +0x40（R37/R38 实测：玩家 5/9/11/12，怪物 22/52..106）
    uint32_t id = 0;
    const bool okId = ReadU32Quiet(reinterpret_cast<const char*>(self) + 0x40, &id);
    const int mul = g_dmgMul.load();
    {
        static std::atomic<int> s_diag{0};
        const int n = s_diag.fetch_add(1);
        if (n < 24)
            LOGI(TPL_LOG " ENT-ATK call n=%d self=%p id(+0x40)=%u ok=%d ret=%u mul=%d",
                 n + 1, self, (unsigned)id, (int)okId, (unsigned)v, mul);
    }
    if (mul > 1 && okId && id >= 1 && id <= kIdPlayerMax) {
        uint64_t s64 = static_cast<uint64_t>(v) * static_cast<uint64_t>(mul);
        if (s64 > 0x7FFFFFFFULL) s64 = 0x7FFFFFFFULL;
        static std::atomic<int> s_boost{0};
        const int nb = s_boost.fetch_add(1) + 1;
        if (nb <= 8 || (nb % 256) == 0)
            LOGI(TPL_LOG " 倍率(ATK) id=%u %u -> %u (x%d) n=%d",
                 (unsigned)id, (unsigned)v, (unsigned)s64, mul, nb);
        return static_cast<uint32_t>(s64);
    }
    return v;
}
static uint32_t Hook_EntCamp(void* self) {
    EntDumpImpl("ENT-CAMP", self, &g_entCampN, 12);
    return g_origEntCamp ? g_origEntCamp(self) : 0u;
}

// ============================================================================
// R35（2026-10-22）后台 GC 堆实体扫描（只读；独立线程，绝不写游戏内存）
//   目的：getter 是死代码（R34 实测 0 次调用）、事件里没有实体指针 ⇒ 最后一条自给路径：
//   直接扫 GC 堆 arena，用「实体特征」筛候选，再靠 hp 下降抓受击实体。
//   段选择：/proc/self/maps 里 rw-p、匿名、start∈[0x7800000000,0x7810000000)、size>=64MB。
//   候选判据：vtable∈(0x1000000000,0x8000000000)、hp(+0xA0)∈[100,1e7]、atk(+0xA4)∈[1,1e6]、
//            camp(+0xB8)<4、cb(+0x99)<2。
//   触发：首次收到 0x86 伤害事件 或 无敌开关打开；之后每 5 秒一轮，最多 20 轮；每轮只打前 60 CAND / 40 DROP。
// ============================================================================
static std::atomic<bool> g_entScanStarted{false};

static void EntScanOnce(std::map<uint64_t, uint32_t>* last) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return;
    char line[512];
    uintptr_t segStart = 0, segEnd = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        unsigned long long a = 0, b = 0;
        char perms[8] = {0};
        if (std::sscanf(line, "%llx-%llx %7s", &a, &b, perms) != 3) continue;
        if (perms[0] != 'r' || perms[1] != 'w') continue;
        if (a < 0x7800000000ULL || a >= 0x7810000000ULL) continue;
        if (b - a < (64ULL << 20)) continue;
        // 匿名段：地址后无路径名（或仅有空格）
        const char* p = line;
        int spaces = 0; while (*p && spaces < 5) { if (*p == ' ') ++spaces; ++p; }
        while (*p == ' ') ++p;
        if (*p != '\n' && *p != 0 && *p != '/') {
            // 有非路径内容（如 [anon:...]）⇒ 也算匿名，继续
        }
        if (*p == '/' ) continue;   // 有名映射跳过
        segStart = static_cast<uintptr_t>(a);
        segEnd   = static_cast<uintptr_t>(b);
        break;
    }
    std::fclose(fp);
    if (!segStart || segEnd <= segStart) {
        LOGW(TPL_LOG " ENTSCAN 未找到 GC 堆段（rw-p 匿名 >=64MB @0x7800_000000）");
        return;
    }
    static std::atomic<int> s_round{0};
    const int round = s_round.fetch_add(1);
    const uintptr_t limit = segEnd - 0xC8;
    int nCand = 0, nDrop = 0, nScanned = 0;
    std::map<uint64_t, uint32_t> now;
    for (uintptr_t a = segStart; a < limit; a += 8) {
        ++nScanned;
        const volatile uint64_t* qw = reinterpret_cast<const volatile uint64_t*>(a);
        const uint64_t vt = *qw;
        if (vt <= 0x1000000000ULL || vt >= 0x8000000000ULL) continue;
        const volatile uint32_t* pd = reinterpret_cast<const volatile uint32_t*>(a);
        const uint32_t hp  = pd[0xA0 / 4];
        if (hp < 100 || hp > 10000000u) continue;
        const uint32_t atk = pd[0xA4 / 4];
        if (atk < 1 || atk > 1000000u) continue;
        const volatile uint8_t* pb = reinterpret_cast<const volatile uint8_t*>(a);
        const uint8_t camp = pb[0xB8];
        if (camp > 3) continue;
        const uint8_t cb = pb[0x99];
        if (cb > 1) continue;
        nCand++;
        const uint64_t key = static_cast<uint64_t>(a);
        now[key] = hp;
        auto it = last->find(key);
        const bool isNew = (it == last->end());
        if (isNew) {
            if (nCand <= 60)
                LOGI(TPL_LOG " ENTSCAN CAND r%d a=%p hp=%u atk=%u camp=%u cb=%u id=%u vt=%p",
                     round, reinterpret_cast<void*>(a), (unsigned)hp, (unsigned)atk,
                     (unsigned)camp, (unsigned)cb, (unsigned)pd[0x40 / 4],
                     reinterpret_cast<void*>(static_cast<uintptr_t>(vt)));
        } else if (it->second >= hp + 10) {
            {
                std::lock_guard<std::mutex> dl(g_dropMu);
                bool dup = false;
                for (size_t z = 0; z < g_dropAddrs.size(); ++z)
                    if (g_dropAddrs[z] == key) { dup = true; break; }
                if (!dup && g_dropAddrs.size() < 96) g_dropAddrs.push_back(key);
            }
            if (nDrop < 40)
                LOGI(TPL_LOG " ENTSCAN DROP r%d a=%p hp: %u -> %u atk=%u camp=%u id=%u",
                     round, reinterpret_cast<void*>(a), (unsigned)it->second, (unsigned)hp,
                     (unsigned)atk, (unsigned)camp, (unsigned)pd[0x40 / 4]);
            ++nDrop;
        }
    }
    *last = now;
    {
        std::lock_guard<std::mutex> lk(g_entTrackMu);
        g_entTrack.clear();
        std::lock_guard<std::mutex> dl(g_dropMu);
        for (size_t z = 0; z < g_dropAddrs.size(); ++z) {
            const uintptr_t a = static_cast<uintptr_t>(g_dropAddrs[z]);
            if (a & 15ull) continue;
            uint32_t hp = 0, atk = 0;
            if (!ReadU32Quiet(reinterpret_cast<const char*>(a) + 0xA0, &hp)) continue;
            if (!ReadU32Quiet(reinterpret_cast<const char*>(a) + 0xA4, &atk)) continue;
            if (hp < 50u || hp > 200000u || atk < 30u || atk > 30000u) continue;
            uint64_t k = 0;
            if (!ReadPtrQuiet(reinterpret_cast<const void*>(a), &k)) continue;
            EntTrackRec r;
            r.addr = a; r.hp = hp; r.klass = k;
            g_entTrack.push_back(r);
        }
        if (!g_dropAddrs.empty())
            LOGI(TPL_LOG " ENT-TRACK 表=%d（来自掉血地址 %d）", (int)g_entTrack.size(), (int)g_dropAddrs.size());
    }
    LOGI(TPL_LOG " ENTSCAN round=%d 段=[%p,%p) 扫描=%d 候选=%d 下降=%d",
         round, reinterpret_cast<void*>(segStart), reinterpret_cast<void*>(segEnd),
         nScanned, nCand, nDrop);
}

// R43：延迟补扣线程 —— 事件后 120 ms 复查实体血量，真掉血才按倍率补扣。
static void PendWatchThread() {
    for (;;) {
        std::this_thread::sleep_for(std::chrono::milliseconds(40));
        const long long now = NowMs();
        std::lock_guard<std::mutex> lk(g_pendMu);
        for (size_t i = 0; i < g_pend.size(); ) {
            const PendSnap p = g_pend[i];
            if (now < p.due) { ++i; continue; }
            if (!(p.addr & 15ull)) {
                uint32_t idr = 0;
                const bool playerBand = ReadU32Quiet(reinterpret_cast<const char*>(p.addr) + 0x40, &idr) &&
                                        idr >= 1 && idr <= kIdPlayerMax;
                uint32_t hp = 0;
                if (!playerBand && ReadU32Quiet(reinterpret_cast<const char*>(p.addr) + 0xA0, &hp) &&
                    hp < p.hp0 && (p.hp0 - hp) >= 1u && (p.hp0 - hp) <= 100000u) {
                    const uint32_t d = p.hp0 - hp;
                    const uint64_t extra = static_cast<uint64_t>(d) * static_cast<uint64_t>(p.mul - 1);
                    const uint32_t target = (hp > extra) ? static_cast<uint32_t>(hp - extra) : 1u;
                    *reinterpret_cast<volatile uint32_t*>(p.addr + 0xA0) = target;
                    static std::atomic<int> nAp{0};
                    const int c = nAp.fetch_add(1) + 1;
                    if (c <= 12 || (c % 64) == 0)
                        LOGI(TPL_LOG " 倍率(延迟补扣) n=%d a=%p %u -> %u (x%d, 原生伤害=%u)",
                             c, (void*)p.addr, (unsigned)hp, (unsigned)target, p.mul, (unsigned)d);
                }
            }
            g_pend.erase(g_pend.begin() + i);
        }
    }
}

// R45（2026-10-25）：按 3.2.0 的链形找「属性记录根」——不需要旧包。
//   链：slot -> +0x08 -> +0x18 -> +0x10 -> +0x78 -> +0x00 = R，读 R+0x38(actor)/R+0xb8(value)。
//   全部读前过 maps 可读校验（R27 教训：ValidPtrForRead 不查映射，裸读会 SIGSEGV）。
struct ZMapRange { uintptr_t a, b; };
static std::vector<ZMapRange> g_rdMaps;
static std::vector<ZMapRange> g_il2Rw;

static void ZLoadMaps() {
    g_rdMaps.clear(); g_il2Rw.clear();
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        unsigned long long a = 0, b = 0; char perm[8] = {0}, path[384] = {0};
        if (sscanf(line, "%llx-%llx %7s %*s %*s %*s %383s", &a, &b, perm, path) < 3) continue;
        if (perm[0] == 'r') { ZMapRange r; r.a = (uintptr_t)a; r.b = (uintptr_t)b; g_rdMaps.push_back(r); }
        if (perm[0] == 'r' && perm[1] == 'w' && strstr(path, "libil2cpp.so")) {
            ZMapRange r; r.a = (uintptr_t)a; r.b = (uintptr_t)b; g_il2Rw.push_back(r);
        }
    }
    fclose(f);
}
static bool ZMapped(uintptr_t a, size_t n) {
    for (size_t i = 0; i < g_rdMaps.size(); ++i)
        if (a >= g_rdMaps[i].a && a + n <= g_rdMaps[i].b) return true;
    return false;
}
static bool ZRdU64(uintptr_t a, uint64_t* out) {
    if (!ZMapped(a, 8)) return false;
    *out = *reinterpret_cast<const volatile uint64_t*>(a);
    return true;
}
static bool ZRdU32(uintptr_t a, uint32_t* out) {
    if (!ZMapped(a, 4)) return false;
    *out = *reinterpret_cast<const volatile uint32_t*>(a);
    return true;
}
static void AttrChainScanOnce() {
    ZLoadMaps();
    if (g_il2Rw.empty()) { LOGW(TPL_LOG " ATTR-SCAN 找不到 libil2cpp rw 段"); return; }
    static const uintptr_t kChain[5] = {0x08, 0x18, 0x10, 0x78, 0x00};
    int nSlot = 0, nChain = 0, nLog = 0;
    for (size_t si = 0; si < g_il2Rw.size(); ++si) {
        const uintptr_t s0 = g_il2Rw[si].a, e0 = g_il2Rw[si].b;
        for (uintptr_t slot = (s0 + 7) & ~7ull; slot + 8 <= e0; slot += 8) {
            uint64_t v = 0;
            if (!ZRdU64(slot, &v)) continue;
            if (v < 0x7000000000ull || v >= 0x8000000000ull) continue;
            ++nSlot;
            uintptr_t cur = (uintptr_t)v; bool ok = true;
            for (int k = 0; k < 5; ++k) {
                uint64_t nx = 0;
                if (!ZRdU64(cur + kChain[k], &nx) || nx < 0x7000000000ull || nx >= 0x8000000000ull) { ok = false; break; }
                cur = (uintptr_t)nx;
            }
            if (!ok) continue;
            ++nChain;
            uint32_t a38 = 0, a40 = 0, vB8 = 0, vB0 = 0;
            const bool okA = ZRdU32(cur + 0x38, &a38);
            ZRdU32(cur + 0x40, &a40);
            const bool okV = ZRdU32(cur + 0xb8, &vB8);
            ZRdU32(cur + 0xb0, &vB0);
            if (okA && okV && a38 <= 200 && vB8 <= 200000 && nLog < 24) {
                ++nLog;
                LOGI(TPL_LOG " ATTR-CAND slot=0x%X R=%p act@38=%u @40=%u val@b8=%u @b0=%u",
                     (unsigned)slot, (void*)cur, (unsigned)a38, (unsigned)a40, (unsigned)vB8, (unsigned)vB0);
            }
        }
    }
    LOGI(TPL_LOG " ATTR-SCAN 完成：slot(堆指针)=%d 链走通=%d", nSlot, nChain);
}
static void MaybeStartAttrScan() {
    static std::atomic<bool> started{false};
    if (started.exchange(true)) return;
    std::thread t([]() { std::this_thread::sleep_for(std::chrono::milliseconds(800)); AttrChainScanOnce(); });
    t.detach();
}

static void EntScanThread() {
    std::map<uint64_t, uint32_t> last;
    for (int i = 0; i < 20; ++i) {
        EntScanOnce(&last);
        for (int s = 0; s < 50; ++s) {
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
        }
    }
    LOGI(TPL_LOG " ENTSCAN 结束（20 轮）");
}

// 只读、独立线程、最多启动一次。
static void MaybeStartEntScan() {
    if (g_entScanStarted.exchange(true)) return;
    LOGI(TPL_LOG " ENTSCAN 启动（首次伤害事件触发，20 轮 × 5s，只读）");
    std::thread t(EntScanThread);
    t.detach();
}

// ============================================================================
//  ⑦ 安装器（幂等；lib ready 即全装，开关由 handler 内门控 — MuMu 转译缓存见下文 SyncHooks）
// ============================================================================
static bool InstallEntryHook(const char* tag, uintptr_t fileOff, const uint32_t* pro, int words,
                             void* hookFn, void** origOut, std::atomic<bool>* doneFlag) {
    if (doneFlag->load()) return true;
    // ① loader 真实模块（序言交叉校验；假模块/未装好 = nullptr 走②）
    void* va = PhdrVaForFileOff(TPL_LIB_NAME, fileOff, pro, words);
    const char* src = "phdr";
    // ② 回退 maps 分段口径（真机/稳定期与 phdr 同值；libReady 已要求真模块门）
    if (!va) {
        va = FileOffToVa(TPL_LIB_NAME, fileOff);
        src = "maps";
    }
    if (!va) {
        LOGW(TPL_LOG " %s: file 0x%lX 未解析到 VA（phdr 无条目且 maps 无覆盖）", tag, (unsigned long)fileOff);
        return false;
    }
    if (!CheckPrologue(tag, va, pro, words, true)) return false;
    if (!DobbyHookGuarded(tag, va, hookFn, origOut)) return false;
    doneFlag->store(true);
    LOGI(TPL_LOG " %s hooked @va %p (file 0x%lX src=%s)", tag, va, (unsigned long)fileOff, src);
    return true;
}

// 移动速度：12B 原地桩（adrp/add/br → 近距跳板 → 自有 asm handler），handler 内乘倍率后跳回收尾
//
// 2026-10-06 L2 实测：MuMu/Houdini 下本 so 与 libil2cpp 常相距 >4GB（delta ≈ -0x371A…），
//   12B 桩里的 adrp 够不到 handler ⇒ 先在 target ±4GB 内分配一页「近距跳板」：
//   跳板用 movz/movk 组装 64 位 handler 地址再 br；目标处只 adrp/add/br → 跳板。
static uintptr_t AllocNearIsland(void* target, size_t* outSize) {
    const long ps = sysconf(_SC_PAGESIZE);
    const size_t sz = static_cast<size_t>(ps > 0 ? ps : 4096);
    *outSize = sz;
    const uintptr_t base = reinterpret_cast<uintptr_t>(target) & ~static_cast<uintptr_t>(sz - 1);
    const int64_t kLimit = static_cast<int64_t>(0x7FF00000);   // < ±4GB，留余量
    for (int i = 0; i < 32; ++i) {
        uintptr_t hint = 0;
        if (i > 0) {
            uintptr_t step = static_cast<uintptr_t>(0x4000000ULL) * static_cast<uintptr_t>((i + 1) / 2);
            hint = (i & 1) ? ((base > step) ? (base - step) : 0) : (base + step);
        }
        // MuMu 下 execmem 可用（Houdini 自身就有大块 rwxp）；直接 RWX 避免 mprotect 被 SELinux 拦
        void* p = mmap(reinterpret_cast<void*>(hint), sz, PROT_READ | PROT_WRITE | PROT_EXEC,
                       MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
        if (p == MAP_FAILED) {
            p = mmap(reinterpret_cast<void*>(hint), sz, PROT_READ | PROT_WRITE,
                     MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
        }
        if (p == MAP_FAILED) continue;
        int64_t d = static_cast<int64_t>(reinterpret_cast<uintptr_t>(p)) - static_cast<int64_t>(base);
        if (d > -kLimit && d < kLimit) return reinterpret_cast<uintptr_t>(p);
        munmap(p, sz);
    }
    return 0;
}

//   R5b：抽成通用安装器（apply 站点 + 两个 reset 站点，共 3 处同构 12B 桩）。
static bool InstallInlineStub(const char* tag, uintptr_t fileOff, uintptr_t resumeOff,
                              const uint32_t pro[3], void* handler, volatile void** resumeSlot,
                              std::atomic<bool>* doneFlag) {
    if (doneFlag->load()) return true;
    // target 与 resume 必须同一口径（混口径 ⇒ stub 跳回假副本必闪退）
    const char* src = "phdr";
    void* target = PhdrVaForFileOff(TPL_LIB_NAME, fileOff, pro, 3);
    void* resume = PhdrVaForFileOff(TPL_LIB_NAME, resumeOff);
    if (!target || !resume) {
        target = FileOffToVa(TPL_LIB_NAME, fileOff);
        resume = FileOffToVa(TPL_LIB_NAME, resumeOff);
        src = "maps";
    }
    if (!target || !resume) {
        LOGW(TPL_LOG " %s: 站点未映射（target=%p resume=%p）", tag, target, resume);
        return false;
    }
    const bool firstAttempt = RateLimitLog(tag, 5);
    // 幂等：若桩已在本轮落地（12B 为 adrp/add/br），直接认领，不重打（避免反复 DobbyCodePatch）
    {
        void* va0 = PhdrVaForFileOff(TPL_LIB_NAME, fileOff);
        if (!va0) va0 = FileOffToVa(TPL_LIB_NAME, fileOff);
        uint32_t g0 = 0, g1 = 0, g2 = 0;
        if (va0 && ReadU32Quiet(va0, &g0) &&
            ReadU32Quiet(reinterpret_cast<const char*>(va0) + 4, &g1) &&
            ReadU32Quiet(reinterpret_cast<const char*>(va0) + 8, &g2) &&
            g2 == 0xD61F0200u && (g0 & 0x9F00001Fu) == 0x90000010u &&
            (g1 & 0xFFC003FFu) == 0x91000210u) {
            *resumeSlot = resume;
            doneFlag->store(true);
            return true;
        }
    }
    static bool s_loggedDrift = false;
    const bool wantLog = firstAttempt && !s_loggedDrift;
    if (!CheckPrologue(tag, target, pro, 3, wantLog)) {
        if (wantLog) {
            uint32_t g0 = 0;
            ReadU32Guarded(tag, reinterpret_cast<const char*>(target), &g0);
            LOGW(TPL_LOG " %s: 序言不符（首指令 got=0x%08X want=0x%08X）— 降级重试（5s 一次）", tag, g0, pro[0]);
        }
        if (wantLog) s_loggedDrift = true;
        return false;
    }
    s_loggedDrift = false;

    *resumeSlot = resume;

    uintptr_t h = reinterpret_cast<uintptr_t>(handler);

    size_t islandSize = 0;
    uintptr_t island = AllocNearIsland(target, &islandSize);
    if (!island) {
        LOGW(TPL_LOG " %s: 跳板页分配失败（target=%p handler=%p）", tag,
             target, reinterpret_cast<void*>(h));
        return false;
    }

    uint32_t* isl = reinterpret_cast<uint32_t*>(island);
    static const uint32_t kMovk[4] = {0xD2800000u, 0xF2A00000u, 0xF2C00000u, 0xF2E00000u};
    for (int i = 0; i < 4; ++i) {
        isl[i] = kMovk[i] | (static_cast<uint32_t>((h >> (16 * i)) & 0xFFFFu) << 5) | 16u;
    }
    isl[4] = 0xD61F0200u;   // br x16
    __builtin___clear_cache(reinterpret_cast<char*>(island), reinterpret_cast<char*>(island) + 20);
    mprotect(reinterpret_cast<void*>(island), islandSize, PROT_READ | PROT_EXEC);

    uintptr_t herePage = reinterpret_cast<uintptr_t>(target) & ~static_cast<uintptr_t>(0xFFF);
    int64_t pageDelta = static_cast<int64_t>(island & ~static_cast<uintptr_t>(0xFFF)) - static_cast<int64_t>(herePage);
    if (pageDelta > static_cast<int64_t>(0xFFFFF000) || pageDelta < static_cast<int64_t>(-0x100000000)) {
        LOGW(TPL_LOG " %s: 跳板超出 adrp 范围 (%lld)", tag, (long long)pageDelta);
        return false;
    }
    uint32_t immPages = static_cast<uint32_t>(pageDelta >> 12);
    uint32_t immlo = immPages & 0x3u;
    uint32_t immhi = (immPages >> 2) & 0x7FFFFu;
    uint32_t lo12  = static_cast<uint32_t>(island & 0xFFFu);

    uint32_t stub[3];
    stub[0] = 0x90000010u | (immlo << 29) | (immhi << 5);   // adrp x16, <island page>
    stub[1] = 0x91000210u | (lo12 << 10);                   // add  x16, x16, #lo12
    stub[2] = 0xD61F0200u;                                  // br   x16

    int rc = DobbyCodePatch(target, reinterpret_cast<uint8_t*>(stub), sizeof(stub));
    if (rc != 0) {
        LOGW(TPL_LOG " %s: DobbyCodePatch rc=%d", tag, rc);
        return false;
    }

    // 写回自检：确认 12B 桩真的落下去（防"改了但没写进去"的假成功）
    uint32_t rb = 0;
    for (int i = 0; i < 3; ++i) {
        if (!ReadU32Guarded(tag, reinterpret_cast<const char*>(target) + i * 4, &rb) ||
            rb != stub[i]) {
            LOGW(TPL_LOG " %s: 自检失败 [+%d] got=0x%08X want=0x%08X", tag, i * 4, rb, stub[i]);
            return false;
        }
    }
    doneFlag->store(true);
    LOGI(TPL_LOG " %s patched @va %p (file 0x%lX src=%s) -> island %p -> handler %p",
         tag, target, (unsigned long)fileOff, src, reinterpret_cast<void*>(island), (void*)h);
    return true;
}

static bool InstallMoveEntryHook() {
    return InstallEntryHook("MoveEntry", kFileOffMoveApplyEntry, kPrologueMoveApply, 4,
                            (void*)Hook_MoveEntry, (void**)&g_origMoveEntry, &g_moveEntryHooked);
}

static bool InstallMovePatch() {
    return InstallInlineStub("MoveSpeed", kFileOffMoveWrite, kFileOffMoveResume, kPrologueMove,
                             reinterpret_cast<void*>(&Asm_ZzzMoveApply), &g_zzzMoveResume,
                             &g_movePatched);
}

// R5b：两处 reset 站点。命中就说明该实体的「速度被重置」，我们改成写自己的倍率。
static bool InstallMoveResetA() {
    return InstallInlineStub("MoveResetA", kFileOffMoveResetA, kFileOffMoveResetAEnd,
                             kPrologueMoveReset, reinterpret_cast<void*>(&Asm_ZzzMoveResetA),
                             &g_zzzMoveResetAResume, &g_moveResetAPatched);
}

static bool InstallMoveResetB() {
    return InstallInlineStub("MoveResetB", kFileOffMoveResetB, kFileOffMoveResetBEnd,
                             kPrologueMoveReset, reinterpret_cast<void*>(&Asm_ZzzMoveResetB),
                             &g_zzzMoveResetBResume, &g_moveResetBPatched);
}

// ============================================================================
//  ⑦b 无限大招（借池鸳的「改哪一条指令」结论，实现走我们自己的字节补丁 + 自检 + 看护）
//    池鸳 Nap32Extra 同款语义：把站点（3.3.0 RVA 0x120faea4，原字节 0x1a9f87e0 = cset w0,ls）改成 0x52800020
//    = mov w0,#1，强制该“大招可用”判定恒真；其自检逻辑（libcymod 0x50520 附近）也按这两个
//    值区分“已补丁 / 未补丁” ⇒ 确认这就是它「大招」的唯一落点。
//    开关关掉时还原原字节；每 5s 回读，被游戏还原/重映射则自动重打。
// ============================================================================
static void* UltGateVa() {
    static void* cached = nullptr;
    if (cached) return cached;
    uintptr_t org = PhdrRvaOrigin(TPL_LIB_NAME);
    if (org) {
        void* cand = reinterpret_cast<void*>(org + kRvaUltGate);
        uint32_t cur = 0;
        if (ReadU32Quiet(cand, &cur) && (cur == kUltGateOrig || cur == kUltGatePatch)) {
            cached = cand;
            return cached;
        }
    }
    // 回退：文件偏移口径（代码段 p_vaddr-p_offset=0x4000 ⇒ fileOff = RVA - 0x4000）
    void* fb = PhdrVaForFileOff(TPL_LIB_NAME, kRvaUltGate - 0x4000, &kUltGateOrig, 1);
    if (fb) cached = fb;
    return cached;
}

static bool UltGateCtxOk();   // 定义在 RealModuleReady 段

static bool WriteUltGate(uint32_t expect, uint32_t want, const char* what) {
    void* va = UltGateVa();
    if (!va) {
        LOGW(TPL_LOG " 无限大招: 站点未解析（RVA 0x%lX）", (unsigned long)kRvaUltGate);
        return false;
    }
    uint32_t cur = 0;
    if (!ReadU32Quiet(va, &cur)) {
        LOGW(TPL_LOG " 无限大招: 站点不可读 @%p", va);
        return false;
    }
    if (cur == want) return true;
    if (!UltGateCtxOk()) {
        LOGW(TPL_LOG " 无限大招: 16B 上下文不符（站点可能被换/被重写）— 拒绝写入");
        return false;
    }
    if (cur != expect) {
        LOGW(TPL_LOG " 无限大招: 字节漂移 @%p got=0x%08X want=0x%08X — 拒绝写入",
             va, (unsigned)cur, (unsigned)expect);
        return false;
    }
    uint32_t v = want;
    int rc = DobbyCodePatch(va, reinterpret_cast<uint8_t*>(&v), 4);
    if (rc != 0) {
        LOGW(TPL_LOG " 无限大招: DobbyCodePatch rc=%d", rc);
        return false;
    }
    uint32_t rb = 0;
    if (!ReadU32Quiet(va, &rb) || rb != want) {
        LOGW(TPL_LOG " 无限大招: 自检失败 got=0x%08X want=0x%08X", (unsigned)rb, (unsigned)want);
        return false;
    }
    LOGI(TPL_LOG " 无限大招: %s @%p (0x%08X)", what, va, (unsigned)want);
    return true;
}

static void SyncUltGate() {
    if (g_skill.load()) {
        if (g_ultApplied.load()) {
            void* va = UltGateVa();
            uint32_t cur = 0;
            if (va && ReadU32Quiet(va, &cur) && cur == kUltGatePatch) return;   // 还在，不动
            g_ultApplied.store(false);   // 被还原/外部修改 ⇒ 重打
        }
        if (WriteUltGate(kUltGateOrig, kUltGatePatch, "补丁已应用")) g_ultApplied.store(true);
    } else if (g_ultApplied.load()) {
        if (WriteUltGate(kUltGatePatch, kUltGateOrig, "已还原原字节")) g_ultApplied.store(false);
    }
}

// 2026-10-06（MuMu 实战零命中真因，修正前一版「转译缓存不回读」误判）：
//   同一函数在激战中晚装钩也能 30ms 内命中（Dmg@0x40002f62a9f0 十条日志）⇒ 不是缓存问题。
//   真因是【口径选错】：MuMu 早期 maps 有游戏校验/预读同名临时分块映射（0x77xx），
//   isLibraryLoaded 早命中 ⇒ 桩全落在随即被回收的假副本上 ⇒ 功能全哑。
//   现在：就绪门=phdr 真模块+序言交叉校验；VA=phdr PT_LOAD 换算（maps 回退）；
//   每 Tick 读回看护，原始序言重现即自动重挂。
static void SyncHooks() {
    if (!g_libReady.load()) return;

    std::lock_guard<std::mutex> lk(g_installMutex);

    if (!g_invHooked.load()) {
        InstallEntryHook("Inv", kFileOffInvEntry, kPrologueInv, 8,
                         (void*)Hook_Inv, (void**)&g_origInv, &g_invHooked);
    }
    // 3.3.0：伤害倍率不装钩（属性写回链已弃用，见 RealModuleReady 注释）；Hook_Dmg 保留但永不注册。
    if (!g_freezeHooked.load()) {
        InstallEntryHook("Freeze", kFileOffFreezeEntry, kPrologueFreeze, 8,
                         (void*)Hook_Freeze, (void**)&g_origFreeze, &g_freezeHooked);
    }
    // R34/R36 诊断探针（ENT-HP/ENT-ATK/ENT-CAMP/RSV/HPW-A/B/C）已全部停装（R54 2026-10-25）：
    //   ① 它们是纯诊断，不该进交付包（hook-rules §30：模拟器翻译层对代码改写面敏感，
    //      hook 数量必须收敛）；
    //   ② 雷电/houdini 实测（tombstone_03/04）：装这 9 个钩后进游戏 4 分钟内 SIGSEGV，
    //      一次在 malloc 里（堆损坏），一次栈全在 libhoudini.so 内（fault 0xdead1007）。
    //      RSV 是其中唯一在「加载期」就被高频调用的（实测 1280+ 次），是首要嫌疑。
    //   ③ ENT-ATK 还会改 getter 返回值（R40 路线），与 R49 的多段结算重复。
    //   钩函数本体保留（静态会话要用），但**永不注册**；需要再开必须带用户当轮授权与新证据。
    // R20: 移速三桩 + MoveEntry 诊断钩全部停装（功能已删；残留桩/看护＝全关态活动源，禁止复活）。
    SyncUltGate();   // 无限大招：开关跟随；被还原则重打（内部自带回读）
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
// 真就绪判据（2026-10-06）：maps 名字命中 ≠ 真加载（早期有校验/预读同名临时映射）。
// 必须：① loader（dl_iterate_phdr）有 libil2cpp；② Dmg 站序言 RVA 交叉校验通过
// ⇒ 真实模块最终映射就位。任一不满足继续等（2s 一次，不写内存）。
// 3.3.0 无限技能站点 16B 上下文校验（装补丁前必过；word0 允许已是补丁值 = 已打过）
static bool UltGateCtxOk() {
    uintptr_t org = PhdrRvaOrigin(TPL_LIB_NAME);
    if (!org) return false;
    const char* p = reinterpret_cast<const char*>(org + kRvaUltGate);
    for (int i = 0; i < 4; ++i) {
        uint32_t cur = 0;
        if (!ReadU32Quiet(p + i * 4, &cur)) return false;
        uint32_t want = kUltGateCtx[i];
        if (i == 0 && cur == kUltGatePatch) continue;
        if (cur != want) return false;
    }
    return true;
}

// 真就绪判据（3.3.0）：无敌 32B + 定怪 32B + 无限技能 16B 三处必须全过。
//   伤害倍率的旧序言已作废（旧站点在新 so 0 命中），不再参与就绪门。
static bool RealModuleReady() {
    if (PhdrVaForFileOff(TPL_LIB_NAME, kFileOffInvEntry, kPrologueInv, 8) == nullptr) return false;
    if (PhdrVaForFileOff(TPL_LIB_NAME, kFileOffFreezeEntry, kPrologueFreeze, 8) == nullptr) return false;
    return UltGateCtxOk();
}

bool Zzz_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    if (!RealModuleReady()) {
        // 一次性就绪门诊断（R53 2026-10-25）：三段判据各打成一行，避免再出现
        // 「菜单能开、功能全哑」却无从定位。只打一次，不参与热路径。
        static std::atomic<bool> s_gateLogged{false};
        if (!s_gateLogged.exchange(true)) {
            void* vaInv = PhdrVaForFileOff(TPL_LIB_NAME, kFileOffInvEntry);
            void* vaFrz = PhdrVaForFileOff(TPL_LIB_NAME, kFileOffFreezeEntry);
            LOGW(TPL_LOG " 就绪门未过：origin=%p inv=%p freeze=%p ultCtx=%d（指针区间 4GiB~128TiB）",
                 (void*)PhdrRvaOrigin(TPL_LIB_NAME), vaInv, vaFrz, (int)UltGateCtxOk());
        }
        return false;
    }

    g_libReady.store(true);
    // R21：缓存 libil2cpp RVA 原点（伤害倍率属性写回用；dl_iterate_phdr 只在此处跑一次）
    if (!g_rvaOrigin.load()) g_rvaOrigin.store(PhdrRvaOrigin(TPL_LIB_NAME));
    LOGI(TPL_LOG " lib ready (real module via dl_iterate_phdr; PT_LOAD VA)");
    SyncHooks();
    // R20：启动期 ChainSelfTestVerbose 已移除——全关态不碰游戏堆（守卫已删，裸读链上指针不受控）。
    return true;
}

bool Zzz_IsReady() {
    return g_libReady.load();
}

void Zzz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ZZZ_FEAT_INV:
            g_inv.store(enabled);
            break;
        case ZZZ_FEAT_SKILL:
            g_skill.store(enabled);
            // 无限技能 = 无限大招：3.3.0 站点 RVA 0x120faea4 的字节补丁，见 SyncUltGate
            LOGI(TPL_LOG " 无限大招开关 = %s", enabled ? "on" : "off");
            break;
        case ZZZ_FEAT_FREEZE:
            g_freeze.store(enabled);
            break;
        default:
            break;
    }
    SyncHooks();
}

void Zzz_OnValueChanged(int featNum, int value) {
    if (featNum == ZZZ_FEAT_DMG) {
        // R40（2026-10-25）：倍率改走「攻击力 getter 放大」（见 Hook_EntAtk），不写游戏内存。
        int m = value;
        if (m < 1) m = 1;
        if (m > 100) m = 100;
        g_dmgMul.store(m);
        static std::atomic<int> nSet{0};
        if (nSet.fetch_add(1) < 12)
            LOGI(TPL_LOG " 伤害倍率 = x%d（ATK getter 路线）", m);
    }
    SyncHooks();
}

// 装钩丢失看护：目标处原始序言重新出现 ⇒ 映射被替换/还原，认丢失并清 doneFlag，
// 交给 SyncHooks 重挂（覆盖 MuMu 迟到重映射场景）。
// R20：① 30s 一轮（每轮 3 次 dl_iterate_phdr——loader 锁，禁止按 Tick 频率碰）；
//       ② 移速桩与命中诊断整块移除（功能已删，禁止复活）；
//       ③ 链自检只在无敌开启时跑（全关态不碰游戏堆对象）。
static void WatchHooks() {
    const long long nowS = static_cast<long long>(time(nullptr));

    static std::atomic<long long> s_lastWatch{0};
    if (nowS - s_lastWatch.load() >= 30) {
        s_lastWatch.store(nowS);
        struct Watch { const char* tag; uintptr_t off; const uint32_t* pro; int words; std::atomic<bool>* done; };
        static Watch kWatch[] = {
            {"Inv",    kFileOffInvEntry,    kPrologueInv,    8, &g_invHooked},
            {"Freeze", kFileOffFreezeEntry, kPrologueFreeze, 8, &g_freezeHooked},
        };
        for (Watch& w : kWatch) {
            if (!w.done->load()) continue;
            void* va = PhdrVaForFileOff(TPL_LIB_NAME, w.off, w.pro, w.words);
            if (va) {   // 序言匹配 ⇒ 该 VA 上是原始代码 ⇒ 我们的桩不在了
                LOGW(TPL_LOG " %s 装钩丢失（原始序言 @%p）— 重新装钩", w.tag, va);
                w.done->store(false);
            }
        }
    }

    // R27（2026-10-22）：3.3.0 **禁止跑链自检**。
    //   取证定案：旧链根 0x1d749418 在 3.3.0 已失效（全进程共享全局、2,201 处引用），30s 自检
    //   读到垃圾 0x1200000095be94d1 后继续 +0x18 解引用 ⇒ 后台线程 SIGSEGV 带崩进程。
    //   链路整体作废：ChainSelfTest 函数体保留（已加 R27 指针闸门）但此处**不再调用**。
    //   玩家 id 集改为只由 Hook_Inv 的 ev 字段投票（+0x1C）喂养。
    (void)nowS;
}

void Zzz_Tick(const bool* features, const int* values) {
    if (features && values) {
        g_inv.store(features[ZZZ_FEAT_INV]);
        g_skill.store(features[ZZZ_FEAT_SKILL]);
        g_freeze.store(features[ZZZ_FEAT_FREEZE]);
        // R40：倍率读滑块值（1..100）
        int m = values[ZZZ_FEAT_DMG];
        if (m < 1) m = 1;
        if (m > 100) m = 100;
        g_dmgMul.store(m);
    }

    if (!g_libReady.load()) {
        // R20：就绪探测降频到 1s（dl_iterate_phdr 是 loader 锁，禁止按 Tick 频率碰）。
        static std::atomic<long long> s_lastReadyProbe{0};
        const long long nowS = static_cast<long long>(time(nullptr));
        if (nowS - s_lastReadyProbe.load() >= 1) {
            s_lastReadyProbe.store(nowS);
            if (isLibraryLoaded(TPL_TARGET_LIB) && RealModuleReady()) {
                g_libReady.store(true);
                if (!g_rvaOrigin.load()) g_rvaOrigin.store(PhdrRvaOrigin(TPL_LIB_NAME));
                LOGI(TPL_LOG " lib ready (via Tick fallback, real module)");
            }
        }
    }
    if (g_libReady.load()) WatchHooks();
    SyncHooks();
}

void Zzz_ForceClear() {
    g_inv.store(false);
    g_skill.store(false);
    g_freeze.store(false);
    g_moveMul.store(1);
    g_dmgMul.store(1);
    g_zzzMoveOn  = 0;
    g_zzzMoveMul = 1.0f;
    g_reenter.store(false);
}
