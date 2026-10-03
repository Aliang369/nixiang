/* ================================================================
 *  WorMod — 境界守望者（Watcher of Realms / 国际服 com.ap.watcherofrealms 1.6.38.829.1）
 *
 *  母本：Games/CxswzMod.cpp（国服潮汐守望者 com.gf.cxswz 1.2.56.375.3，同产品线同架构）
 *
 *  ⚠️ 本作架构与国服完全相同（2026-10-03 实测，改动前必读）：
 *    APK 内 lib/arm64-v8a/libil2cpp.so 仅 232KB，是 7z 解压壳
 *    （com.moba.unityplugin.Z7Extractor）；真 IL2CPP 库首启解压到
 *    getExternalFilesDir()/dragon2019/assets/comlibs/arm64-v8a/liblogic.bytes
 *    （137,648,336 B / md5 9c600b9c…），复制为 liblogic.so 后动态装进进程。
 *    ⇒ 目标库名 = liblogic.so；RVA→运行时地址必须走 /proc/self/maps 换算。
 *
 *  ★★ e2 功能阶段（2026-10-03）—— 本文件当前交付的就是这一形态：
 *    阶段目标：**无敌**（我方单位承伤清零）+ **伤害倍率**（对敌放大 1..10），kSlots 装载。
 *    ⚠️ 国际服 liblogic.so 的 .dynsym **没有任何方法级符号**（ExcuteOnce/HasComponent
 *    /HpChange 全 0 条），Il2CppDumper 对它的 CodeRegistration 定位是假阳性
 *    （0x1226bd8 落在 .rela.dyn 区）⇒ 国服那套静态 RVA 路线**已彻底放弃、禁止恢复**。
 *    ⇒ e2 的解法：**运行时按名解析 + 运行时字段偏移，零静态 RVA**。
 *       · ResolveRuntimeLayout() 在 TryInit 内、InstallBattleHooks() **之前**跑一次（只读）；
 *         用 api.class_fields / field_name / field_offset / field_type 迭代字段全表，
 *         再用 class_from_type 走「字段类型链」拿到 stHpChangeData / HPChangeComponent / Entity，
 *         把 g_off_* 全部按名解析覆盖（失败保持国服兜底值，功能降级但不崩）。
 *       · RVA_EXCUTE_ONCE 故意置 0 ⇒ HookSlotInstall 的 `if (s.rva && ...)` 直接短路
 *         ⇒ **强制走 HookByName**（HookByName 不做序言校验，HookAtRva 才校验）。
 *       · HookByName 拿到 MethodInfo::methodPointer 后**必须**过 IsExecutableRange 校验
 *         （HybridCLR/解释器方法的 methodPointer 可能是 0x400000000000 这类标记值）。
 *    e1 环境阶段的形态（WOR_ENV_STAGE=1、0 hook）已关闭，脚手架保留在 #if 里。
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 *
 * ================================================================ */

#include "WorMod.h"

#include <atomic>
#include <cerrno>
#include <chrono>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctime>
#include <dlfcn.h>
#include <pthread.h>
#include <sys/stat.h>   // C1 热重载：stat(mtime/size) 判定配置文件是否被改

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[Wor]"  // 本作日志标签（构建自检/真机 logcat 对账用）

// 目标库：运行时解压出来的真 IL2CPP 库（包内 libil2cpp.so 是 7z 解压壳）
#define TPL_TARGET_LIB "liblogic.so"

// 构建身份（出包/装机/真机日志三处对账用；改功能时同步递增）
#define WOR_BUILD_TAG "wor-e2-20261004b"

// ============================================================
// ★★ C2：诊断总开关（2026-10-04 加）★★
// ------------------------------------------------------------
// 改动前，诊断代码**永远编进 so**：WorClassLookupSelfTest / DumpBattleDiagnostics /
// DumpTargetFields / ScanAllImages 及其全部调用点都是无条件编译，排障构建与正式构建
// 出的是同一个 so。两个后果：
//   ① 正式包日志被探针洪水灌满 16M 环形缓冲，[Wor] 关键行容易被回绕掉；
//   ② so 里多一大批可被特征化的字符串（类名/字段名/probe 格式串）与额外代码面。
//
// 现在：
//   WOR_DIAG_FULL=1（默认）→ 全部诊断编入 =「排障构建」（一次真机把所有假设打尽）
//   WOR_DIAG_FULL=0        → 诊断函数与调用点全部编掉 =「正式构建」（so 更小、日志只留功能行）
// 由 04_scripts/境界守望者/wor_build_so.sh 的 STARRY_WOR_DIAG=1|0 传进来，并写进
// Android_wor.mk 的 CFG_HASH ⇒ 改档位会强制全量重建，杜绝拿到混档的 so。
//
// ★ Release 档下 WorMod.cpp 唯一保留的诊断代码是 ReconcileLog 的空壳 —— 它走运行时
//   g_cfgDiag 门（见 C1），代码量为零，保留是为了让 OffOfField* 的调用点不用一起编掉。
// ============================================================
#ifndef WOR_DIAG_FULL
#define WOR_DIAG_FULL 1
#endif

#if WOR_DIAG_FULL
#define WOR_DIAG_TIER "diag-tier=FULL"
#else
#define WOR_DIAG_TIER "diag-tier=RELEASE"
#endif

// ============================================================
// ★★ 目标库句柄解析（真机血证，勿删勿简化）★★
//  实测 fact（读 06_dumped/境界守望者/base_1.6.38.829.1/liblogic.bytes 的 ELF64 PT_DYNAMIC）：
//    · 这个 137MB 的「liblogic.so」**DT_SONAME = "libil2cpp.so"**（文件名 ≠ soname！）
//    · split 里那个 232KB 的 lib/arm64-v8a/libil2cpp.so 的 **DT_SONAME 也是 "libil2cpp.so"**
//  ⇒ ① `dlopen("liblogic.so")` 按 soname 查表必 miss，且该名字不在任何 linker 搜索路径
//       （游戏是用绝对路径 /data/user/0/<pkg>/app_libs/liblogic.so dlopen 的）⇒ **必失败**；
//     ② `dlopen("libil2cpp.so")` 有命中那 232KB 转发壳的风险。
//  ⇒ 按「裸名 → /proc/self/maps 里的真实绝对路径 → soname」三级依次试，逐级打日志留证。
// ============================================================
#define WOR_LIB_SONAME "libil2cpp.so"

static char g_libRealPath[512] = {0};

// 从 /proc/self/maps 取 TPL_TARGET_LIB 的真实映射路径（末列）。本进程必可读，
// 且这是**唯一**能区分「文件名 liblogic.so / soname libil2cpp.so」的依据。
static const char* WorLibRealPath() {
    if (g_libRealPath[0]) return g_libRealPath;
    FILE* f = fopen("/proc/self/maps", "re");
    if (!f) {
        LOGW(TPL_LOG " maps: fopen /proc/self/maps fail errno=%d", errno);
        return nullptr;
    }
    char line[1024];
    while (fgets(line, sizeof(line), f)) {
        if (!strstr(line, TPL_TARGET_LIB)) continue;   // liblogic.so
        char* nl = strchr(line, '\n');
        if (nl) *nl = 0;
        char* sp = strrchr(line, ' ');
        if (!sp) continue;
        const char* path = sp + 1;
        if (path[0] != '/') continue;
        size_t n = strlen(path);
        if (n == 0 || n >= sizeof(g_libRealPath)) continue;
        memcpy(g_libRealPath, path, n + 1);
        break;
    }
    fclose(f);
    LOGI(TPL_LOG " maps: %s real path = %s", TPL_TARGET_LIB,
         g_libRealPath[0] ? g_libRealPath : "(none)");
    return g_libRealPath[0] ? g_libRealPath : nullptr;
}

// 三级兜底取 dlopen 句柄；返回 nullptr = 三级全失败。命中的那一级会打日志。
static void* WorDlopenTarget() {
    const char* rp = WorLibRealPath();
    struct Cand { const char* name; const char* how; };
    const Cand cands[3] = {
        { TPL_TARGET_LIB,  "bare-name" },
        { rp,              "maps-path" },
        { WOR_LIB_SONAME,  "soname"    },
    };
    for (size_t i = 0; i < 3; i++) {
        const char* n = cands[i].name;
        if (!n || !*n) continue;
        void* h = dlopen(n, RTLD_NOW | RTLD_NOLOAD);
        const int noLoadHit = h ? 1 : 0;
        if (!h) h = dlopen(n, RTLD_NOW);
        if (h) {
            LOGI(TPL_LOG " dlopen ok via %s (NOLOAD hit=%d): %s", cands[i].how, noLoadHit, n);
            return h;
        }
        LOGW(TPL_LOG " dlopen miss via %s: %s", cands[i].how, n);
    }
    LOGE(TPL_LOG " dlopen 三级全失败 —— 无法取 IL2CPP 句柄");
    return nullptr;
}

// ============================================================
// ★ 环境阶段开关：1 = 一个 hook 都不装（e1 交付形态）；0 = 启用下方挂点表（功能阶段）
//   ★ e2：已翻为 0 —— 功能阶段已启用（无敌 + 伤害倍率），走运行时按名解析。
//   #if WOR_ENV_STAGE 脚手架整体保留（回退环境阶段只需翻回 1）。
// ============================================================
#define WOR_ENV_STAGE 0

// 伤害倍率上限（必须与 WorEntry.cpp 的 "Slider_伤害倍率:1:10:1" 一致）
#define DMG_MUL_MAX 10

// ============================================================
// ① RVA 与偏移（e2：全部改为「运行时按名解析」，零静态 RVA）
//    国服 dump.cs 参照：HpChangeEntitySystem(16410) / HPChangeComponent(16331) /
//                      stHpChangeData(16702) / Entity(16387) / EComponentType(16296)
// ============================================================
// ★ 故意置 0 ⇒ HookSlotInstall 的 `if (s.rva && HookAtRva(...))` 直接短路 ⇒ **强制走 HookByName**。
//   国服 RVA 0x4927B28 是**国服** liblogic 1.2.56.375.3 的偏移，与国际服二进制无关，
//   **禁止恢复成任何非 0 值**（国际服 .dynsym 无方法级符号，静态 RVA 无法定位）。
#define RVA_EXCUTE_ONCE    0u
// 仅国服序言字（文件字节 fc 67 bb a9，小端读为 0xA9BB67FC）；国际服不适用，
// 已**不参与任何判定**（RVA_EXCUTE_ONCE=0 时 expectInsn 根本不会被读到）。
#define INSN_EXCUTE_ONCE   0xA9BB67FCu
// 纯文档：国服 Entity.HasComponent 的 RVA。国际服改为按名解析（见 ResolveRuntimeLayout），
// **代码里不许再引用这个宏**（保留仅为与国服母本对账）。
#define WOR_RVA_HAS_COMPONENT_DOC_ONLY 0x497368Cu

// ------------------------------------------------------------
// ★ e2：以下偏移从宏改为**运行时可写变量**，初值 = 国服 build 的原值（兜底），
//   由 ResolveRuntimeLayout() 在初始化阶段按名解析覆盖（失败则保持兜底值）。
//   ⚠️ hook 热路径只读这些变量，**绝不调用任何 IL2CPP API**（每帧调用 = 性能与稳定性双杀）。
// ------------------------------------------------------------
// HpChangeEntitySystem 字段（self 偏移）
static int32_t g_off_SYS_COM_HP   = 0x50;  // HPChangeComponent* com_hp
static int32_t g_off_SYS_HDATA    = 0x60;  // stHpChangeData hData（当前正在结算的一条）
static int32_t g_off_SYS_MATK     = 0xD0;  // Entity* mAtkEntity
static int32_t g_off_SYS_FDAMAGE  = 0xD8;  // Fix64 fDamage
// HPChangeComponent
static int32_t g_off_HPCOMP_LVALUES = 0x20; // List<stHpChangeData> lValues
// stHpChangeData（stride 0x70）
static int32_t g_off_HPD_FVALUE   = 0x00;  // Fix64 fValue
static int32_t g_off_HPD_SRC      = 0x08;  // EDamageSourceType
static int32_t g_off_HPD_SETTLE   = 0x0C;  // EDamageSettleType
static int32_t g_off_HPD_SHOW     = 0x10;  // ETextShowType
static int32_t g_off_HPD_ATK_EUID = 0x1C;  // eRelationData.eUid（攻击者 entity uid）
static int32_t g_off_HPD_ATK_CUID = 0x20;  // eRelationData.cUid
static int32_t g_off_HPD_ATK_UNIT = 0x24;  // eRelationData.iUnitID
static int32_t g_off_HPD_SKILL    = 0x28;  // iAttackSkillID
static int32_t g_off_HPD_FORMULA  = 0x2C;  // iFormularID
// Entity
static int32_t g_off_ENTITY_MUID  = 0x10;  // <m_uID>k__BackingField

// ---- 以下保持宏不变：List<T> / 托管数组布局跨 Unity 版本稳定，
//      泛型实例类也不好按名取（GetMethod/class_fields 对泛型实例的行为不稳）。
#define LIST_OFF_ITEMS     0x10  // T[] _items
#define LIST_OFF_SIZE      0x18  // int _size
#define ARRAY_OFF_DATA     0x20  // 首元素（数组头 0x20）
#define HPD_STRIDE        0x70   // stHpChangeData 元素跨度（仅文档/对账用，代码不引用）

// EComponentType
#define ECOMP_CARDSOLDIER 43     // CardSoldier（我方英雄/卡牌单位）
#define ECOMP_MONSTERSIGN 60     // MonsterSign（敌方怪物）
#define ECOMP_TOWERSIGN   61     // TowerSign（我方塔/建筑）

// Fix64 = Q20.12（FRACTIONAL_PLACES=12 ⇒ ONE=4096）
#define FIX64_ONE 4096.0

// 写前合理性闸门（B5）：raw 是垃圾值时的护栏。
// 偏移解析错误时 raw 会是任意 64bit 垃圾，直接写回游戏内存 = 必崩/必乱屏；
// 正常承伤永远是「负数且量级远小于 2^44」，故只允许该区间被改写。
// ★ C1（2026-10-04）：实际比较用的是 g_rawSaneMax（运行时可被 wor_cfg.txt 覆盖），
//   这里只留内建默认值。换版本后承伤量级若变化，改 cfg 即可，不用重编。
#define RAW_SANE_MAX_DEFAULT (1LL << 44)

// ============================================================
// ============================================================
static std::atomic<bool> g_libReady{false};  // liblogic.so 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<int>  g_dmgMul{1};      // 伤害倍率（1 = 关闭；菜单 Slider 1..DMG_MUL_MAX）
static std::atomic<bool> g_inv{false};      // 无敌开关
static std::atomic<bool> g_reenter{false};
static std::atomic<int>  g_probeCount{0};   // 探针阶段：已打印条数（有界）
static std::atomic<bool> g_probeReady{false}; // ExcuteOnce 已成功挂上
#define PROBE_MAX 400                     // 探针日志上限（防刷爆 logcat）
static std::atomic<int>  g_modCount{0};     // 功能生效日志计数（有界）
#define MOD_LOG_MAX 200                   // 功能生效日志上限

// ============================================================
// ★★ C1：设备侧配置化（wor_cfg.txt，2026-10-04 加）★★
// ------------------------------------------------------------
// 动机：下面这些值「最可能要调」，但改动前全是编译期常量 ⇒ 调一个数字要走
//   wor_build_so.sh → surgical_wor_1.6.38.829.1.sh（6 步）→ wor_sign.sh →
//   wor_install.sh → 手点菜单，一轮十几分钟且必须重装重签名。
// 现在：往 /sdcard/Android/data/com.ap.watcherofrealms/files/wor_cfg.txt 写一行即生效
//   （不重编、不重装、不重签，也不用重启游戏 —— 见下面的「热重载」）。
//   通道是现成的：wor_tables.txt 就写在同一个目录，读/写/pull 都已在真机验证过。
//
// ★ 热重载（2026-10-04 加）：每 `cfg_poll_ms` 毫秒 stat 一次该文件，mtime/size 变了就重读，
//   改完约 2 秒生效。三条边界（缺一条这个模式就会从「省事」变成「埋雷」）：
//     ① 缺键 = 内建默认：重载时先把上一次的覆盖**还原到基线**再套新文件 ⇒ 删掉一个键也生效，
//        配置语义不会退化成「只增不减」；
//     ② 写错不致命：值越界 / 非数字 / 未知键一律只 LOGW 忽略，绝不报错退出；
//     ③ 只在初始化期被读取的键（layout_min_asm / layout_min_ms / scan_max_classes /
//        scan_max_sec / tbl_path）改完**必须重启游戏**才用得上 —— 重载会明确打「需重启」告警，
//        不要以为它当场生效了。
//
// 文件格式（纯文本；`#` 起首到行尾为注释；`键 = 值`；未知键只告警不致命）：
//   diag             = 1|0        诊断日志总闸（不重编即可静音排障构建；默认 = WOR_DIAG_FULL）
//   probe_max        = <int>      P# 探针行上限（默认 400）
//   raw_sane_max     = <int64>    写闸门阈值（默认 2^44）
//   layout_min_asm   = <int>      EnsureLayout 闸门2：最少程序集数（默认 6）
//   layout_min_ms    = <int>      EnsureLayout 闸门2：最短等待毫秒（默认 1500）
//   scan_max_classes = <int>      S3 结构扫描类数上限（默认 40000）
//   scan_max_sec     = <int>      S3 结构扫描秒数上限（默认 60）
//   tbl_path         = <路径>     覆盖 wor_tables.txt 落盘路径（默认走候选表）
//   cfg_poll_ms      = <int>      热重载轮询间隔毫秒（默认 2000；0 = 关闭热重载，
//                                且关掉后改回非 0 需重启游戏 —— 不轮询就读不到这一行）
//   off.<key>        = 0x..       覆盖 15 个字段偏移（换版本必变的那批）
//     键名：sys_com_hp sys_hdata sys_matk sys_fdamage hpcomp_lvalues
//           hpd_fvalue hpd_src hpd_settle hpd_show hpd_atk_euid hpd_atk_cuid
//           hpd_atk_unit hpd_skill hpd_formula entity_muid
//
// ★ 兜底原则（重要）：任何一项解析失败 / 越界 / 文件不存在 ⇒ **保持内建默认值**。
//   绝不让「配置文件写错」升级成「布局解析错 ⇒ 崩溃」——写坏最多影响功能，不影响启动。
// ============================================================
#define WOR_CFG_PRIMARY "/sdcard/Android/data/com.ap.watcherofrealms/files/wor_cfg.txt"
#define WOR_CFG_ALT     "/storage/emulated/0/Android/data/com.ap.watcherofrealms/files/wor_cfg.txt"
// 候选路径表：首载与热重载共用同一份，避免两处顺序不一致导致「首载读 A、重载读 B」
static const char* const kWorCfgCands[2] = { WOR_CFG_PRIMARY, WOR_CFG_ALT };

// 热重载轮询间隔内建默认（0 = 关闭热重载，退回「只在 Wor_TryInit 读一次」的老行为）
#define WOR_CFG_POLL_MS_DEFAULT 2000

static bool    g_cfgLoaded  = false;                      // 已尝试加载（只做一次）
static bool    g_cfgOk      = false;                      // 文件存在且至少解析成功一项
static bool    g_cfgDiag    = (WOR_DIAG_FULL != 0);       // 诊断日志门（运行时可关）
static int     g_cfgProbeMax = PROBE_MAX;                 // 探针上限（运行时可调）
static int64_t g_rawSaneMax  = RAW_SANE_MAX_DEFAULT;      // 写闸门阈值（运行时可调）
static size_t  g_cfgMinAsm       = 6;                     // 同 WOR_LAYOUT_MIN_ASM
static int64_t g_cfgMinElapsedMs = 1500;                  // 同 WOR_LAYOUT_MIN_ELAPSED_MS
static int     g_cfgScanMaxClasses = 40000;               // 同 WOR_SCAN_MAX_CLASSES
static int     g_cfgScanMaxSec     = 60;                  // 同 WOR_SCAN_MAX_SEC
static char    g_cfgTblPath[256] = {0};                   // 非空则覆盖 wor_tables.txt 路径

// ---- C1 热重载状态（2026-10-04 加）----
static int     g_cfgPollMs     = WOR_CFG_POLL_MS_DEFAULT; // 热重载轮询间隔（0 = 关）
static char    g_cfgPath[256]  = {0};                     // 实际解析成功的路径（热重载盯它）
static int64_t g_cfgMtime      = -1;                      // 上次读到的 mtime
static int64_t g_cfgSize       = -1;                      // 上次读到的 size
static int64_t g_cfgLastPollMs = 0;                       // 上次轮询时刻（WorMonoMs）
static int     g_cfgReloadN    = 0;                       // 已成功重载次数（日志用）

struct WorCfgOff { const char* key; int32_t* dst; const char* what; };
static const WorCfgOff kWorCfgOffs[] = {
    {"off.sys_com_hp",     &g_off_SYS_COM_HP,     "SYS_COM_HP"},
    {"off.sys_hdata",      &g_off_SYS_HDATA,      "SYS_HDATA"},
    {"off.sys_matk",       &g_off_SYS_MATK,       "SYS_MATK"},
    {"off.sys_fdamage",    &g_off_SYS_FDAMAGE,    "SYS_FDAMAGE"},
    {"off.hpcomp_lvalues", &g_off_HPCOMP_LVALUES, "HPCOMP_LVALUES"},
    {"off.hpd_fvalue",     &g_off_HPD_FVALUE,     "HPD_FVALUE"},
    {"off.hpd_src",        &g_off_HPD_SRC,        "HPD_SRC"},
    {"off.hpd_settle",     &g_off_HPD_SETTLE,     "HPD_SETTLE"},
    {"off.hpd_show",       &g_off_HPD_SHOW,       "HPD_SHOW"},
    {"off.hpd_atk_euid",   &g_off_HPD_ATK_EUID,   "HPD_ATK_EUID"},
    {"off.hpd_atk_cuid",   &g_off_HPD_ATK_CUID,   "HPD_ATK_CUID"},
    {"off.hpd_atk_unit",   &g_off_HPD_ATK_UNIT,   "HPD_ATK_UNIT"},
    {"off.hpd_skill",      &g_off_HPD_SKILL,      "HPD_SKILL"},
    {"off.hpd_formula",    &g_off_HPD_FORMULA,    "HPD_FORMULA"},
    {"off.entity_muid",    &g_off_ENTITY_MUID,    "ENTITY_MUID"},
};
#define WOR_CFG_OFF_N ((int)(sizeof(kWorCfgOffs) / sizeof(kWorCfgOffs[0])))
// kWorCfgOffs 里 SYS_HDATA 的下标 —— g_offHdataRuntime（装机闸门）必须跟它同步，
// 故显式命名，防止以后插入/重排键名时静默错位。
#define WOR_CFG_OFFIDX_HDATA 1
static bool g_cfgOffSet[WOR_CFG_OFF_N] = {false};   // 该项是否被 cfg 显式覆盖
// 覆盖前的基线值（= 运行时对账/国服真值的最终结果）。
// ★ 热重载必须靠它把 g_off_* 还原干净再重新套覆盖：否则「新文件删掉某个键」会残留上一次的
//   覆盖值，配置语义退化成「只增不减」，与「缺键 = 内建默认」的兜底原则自相矛盾。
static int32_t g_offBase[WOR_CFG_OFF_N] = {0};
static bool    g_cfgOffBaseTaken = false;           // 基线只取一次

// 就地去首尾空白，返回新起点
static char* CfgTrim(char* s) {
    while (*s == ' ' || *s == '\t' || *s == '\r' || *s == '\n') s++;
    char* e = s + strlen(s);
    while (e > s && (e[-1] == ' ' || e[-1] == '\t' || e[-1] == '\r' || e[-1] == '\n')) *--e = 0;
    return s;
}

// 解析 int64（base=0 ⇒ 自动识别 0x 前缀）；只允许尾部空白，任何垃圾字符都算失败
static bool CfgParseI64(const char* s, int64_t* out) {
    if (!s || !*s) return false;
    errno = 0;
    char* end = nullptr;
    const long long v = strtoll(s, &end, 0);
    if (end == s || errno != 0) return false;
    while (*end == ' ' || *end == '\t' || *end == '\r') end++;
    if (*end != 0) return false;
    *out = (int64_t)v;
    return true;
}

// 读一个配置文件；文件存在即返回 true（解析失败项只告警，绝不致命）
static bool WorCfgLoadFile(const char* path) {
    FILE* fp = fopen(path, "r");
    if (!fp) return false;
    char line[512];
    int nOk = 0, nBad = 0;
    while (fgets(line, (int)sizeof(line), fp)) {
        char* hash = strchr(line, '#');
        if (hash) *hash = 0;                       // 整行/行内注释
        char* s = CfgTrim(line);
        if (!*s) continue;
        char* eq = strchr(s, '=');
        if (!eq) { nBad++; continue; }
        *eq = 0;
        char* key = CfgTrim(s);
        char* val = CfgTrim(eq + 1);
        if (!*key) { nBad++; continue; }

        int64_t iv = 0;
        const bool isNum = CfgParseI64(val, &iv);
        bool handled = false;

        if (strcmp(key, "diag") == 0) {
            if (isNum) { g_cfgDiag = (iv != 0); handled = true; }
        } else if (strcmp(key, "probe_max") == 0) {
            if (isNum && iv > 0 && iv <= 100000) { g_cfgProbeMax = (int)iv; handled = true; }
        } else if (strcmp(key, "raw_sane_max") == 0) {
            if (isNum && iv > 0) { g_rawSaneMax = iv; handled = true; }
        } else if (strcmp(key, "layout_min_asm") == 0) {
            if (isNum && iv >= 1 && iv <= 4096) { g_cfgMinAsm = (size_t)iv; handled = true; }
        } else if (strcmp(key, "layout_min_ms") == 0) {
            if (isNum && iv >= 0 && iv <= 600000) { g_cfgMinElapsedMs = iv; handled = true; }
        } else if (strcmp(key, "scan_max_classes") == 0) {
            if (isNum && iv > 0 && iv <= 5000000) { g_cfgScanMaxClasses = (int)iv; handled = true; }
        } else if (strcmp(key, "scan_max_sec") == 0) {
            if (isNum && iv > 0 && iv <= 3600) { g_cfgScanMaxSec = (int)iv; handled = true; }
        } else if (strcmp(key, "cfg_poll_ms") == 0) {
            if (isNum && iv >= 0 && iv <= 600000) { g_cfgPollMs = (int)iv; handled = true; }
        } else if (strcmp(key, "tbl_path") == 0) {
            if (val && *val && strlen(val) < sizeof(g_cfgTblPath)) {
                snprintf(g_cfgTblPath, sizeof(g_cfgTblPath), "%s", val);
                handled = true;
            }
        } else {
            for (int i = 0; i < WOR_CFG_OFF_N; i++) {     // off.<key>
                if (strcmp(key, kWorCfgOffs[i].key) != 0) continue;
                if (isNum && iv >= 0 && iv <= 0xFFFF) {
                    *kWorCfgOffs[i].dst = (int32_t)iv;
                    g_cfgOffSet[i] = true;
                    handled = true;
                }
                break;
            }
        }

        if (handled) {
            nOk++;
        } else {
            nBad++;
            LOGW(TPL_LOG " cfg: 忽略无效行 key='%s' val='%s'%s", key, val,
                 isNum ? "（值越界）" : "（值非数字）");
        }
    }
    fclose(fp);
    LOGI(TPL_LOG " cfg: %s ok=%d bad=%d", path, nOk, nBad);
    return true;
}

// 记录一个路径的 mtime/size（热重载的「有没有变」判据）；失败则置 -1（视为「不存在」）
static bool WorCfgStamp(const char* path, int64_t* mtimeOut, int64_t* sizeOut) {
    *mtimeOut = -1;
    *sizeOut  = -1;
    if (!path || !*path) return false;
    struct stat st;
    if (stat(path, &st) != 0) return false;
    if (!S_ISREG(st.st_mode)) return false;
    *mtimeOut = (int64_t)st.st_mtime;
    *sizeOut  = (int64_t)st.st_size;
    return true;
}

// 尝试两个候选路径；只做一次。失败 → 全部保持内建默认值（不影响启动）
static void WorCfgLoad() {
    if (g_cfgLoaded) return;
    g_cfgLoaded = true;
    for (int i = 0; i < 2; i++) {
        if (WorCfgLoadFile(kWorCfgCands[i])) {
            g_cfgOk = true;
            snprintf(g_cfgPath, sizeof(g_cfgPath), "%s", kWorCfgCands[i]);
            break;
        }
    }
    if (!g_cfgOk) {
        LOGI(TPL_LOG " cfg: 未发现 wor_cfg.txt（全部使用内建默认值；热重载每 %dms 轮询一次，"
                     "push 上去后无需重启即生效）", g_cfgPollMs);
        return;
    }
    WorCfgStamp(g_cfgPath, &g_cfgMtime, &g_cfgSize);
    LOGI(TPL_LOG " cfg: diag=%d probe_max=%d raw_sane_max=%lld min_asm=%d min_ms=%lld "
                 "scan_cls=%d scan_sec=%d poll_ms=%d tier=%s",
         g_cfgDiag ? 1 : 0, g_cfgProbeMax, (long long)g_rawSaneMax, (int)g_cfgMinAsm,
         (long long)g_cfgMinElapsedMs, g_cfgScanMaxClasses, g_cfgScanMaxSec, g_cfgPollMs,
         WOR_DIAG_TIER);
}


// ============================================================
// ② 字段偏移与 RVA（功能阶段填：dump.cs / IDA / Trace 实测，禁止猜；换包重 dump）
// ============================================================
// enum : size_t { kOffXxx = 0x0 };  // 示例占位已删：功能阶段按 dump 实测回填

// ============================================================
// ③ 安全助手（防闪退核心，勿删；新代码禁止绕开它们裸读写内存）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (OkPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (OkPtr(obj)) memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

// 类名缓存：热路径禁止每帧 object_class + class_name 解析
struct ClassNameCache {
    void* klass = nullptr;
    char  name[40] = {};
    bool  valid = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return nullptr;
    auto& api = GetApi();
    void* k = api.object_class ? api.object_class(obj) : nullptr;
    if (!k || !api.class_name) return nullptr;
    if (cache.klass != k) {
        const char* n = api.class_name(k);
        cache.klass = k;
        cache.valid = false;
        if (n) {
            size_t i = 0;
            for (; i + 1 < sizeof(cache.name) && n[i]; i++) cache.name[i] = n[i];
            cache.name[i] = 0;
            cache.valid = i > 0;
        }
    }
    return cache.valid ? cache.name : nullptr;
}

static bool NameHas(const char* got, const char* needle) {
    if (!got || !needle) return false;
    for (size_t i = 0; got[i]; i++) {
        size_t j = 0;
        while (needle[j] && got[i + j] == needle[j]) j++;
        if (!needle[j]) return true;
    }
    return false;
}

// ============================================================
// ④ 双路径挂点机器（提炼自 WhrhxMod 实测；禁 base+RVA、禁全 RX 误扫）
// ============================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

// file_off → 运行时 VA：只在 RX 段找（libil2cpp 分段映射，base+RVA 会挂错）
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return found;
}

// RVA 路径：序言不匹配 = 版本漂移/空页，拒绝挂（防挂错地址闪退），转按名
static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn, void* hook, void** orig) {
    void* va = FileOffToVa(TPL_TARGET_LIB, rva);
    if (!va) {
        LOGW("%s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        memcpy(&got, va, sizeof(got));
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW("%s rva drift got=0x%08X want=0x%08X — try by-name", tag, got, expectInsn);
            return false;
        }
    }
    if (DobbyHook(va, hook, orig) != 0) {
        LOGW("%s dobby fail @rva", tag);
        return false;
    }
    LOGI("%s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

// ★★★ round-3 真机血证（2026-10-03 22:48，务必勿删该注释）★★★
// 症状：闸门修好后不再崩，`asm count=63` 却 **每个按名 probe 全 MISS**，
//       `probe short Entity hits=0`，`wor_tables.txt` 的 SECTION 1 一个镜像名都没打出来。
// 根因：`Api::Load("liblogic.so")` 成功，但 `Unity/Il2CppApi.cpp` 的「进程内 dynsym 重解析」
//       把符号地址**改写成 Qigsaw 转发壳**里的地址：
//   [Il2Cpp] phdr match: /data/app/~~fuuYrl-.../lib/arm64/libil2cpp.so base=0x744d944000
//   [Il2Cpp] probe[D] il2cpp_class_from_name ... runtime=0x744d95a49c[28010090080147f9] -> KEEP-RUNTIME
//   [Il2Cpp] probe[D] applied=0 keep=18
//   ⇒ 落点 = 0x744d944000+0x1649c = **232,520 B 的壳内部**，调用恒返回 0。
//   真运行时的 DT_SONAME 与壳的 DT_SONAME **都是 `libil2cpp.so`**（真运行时文件在
//   /data/data/<pkg>/app_libs/liblogic.so），所以任何按 soname 的匹配都可能先撞上壳。
// 对策：**不信任 Api::Load**，直接用「已证可靠」的句柄（WorDlopenTarget() →
//       `dlopen ok via maps-path (NOLOAD hit=1)`）dlsym 出全部 il2cpp_* 覆盖进 Api 成员。
//       该句柄已被 EnsureWorSyms() 证明可用（`syms cf=0x74284cca14 ... core=1`）。
static void* g_realLibHandle = nullptr;
static bool  g_forceApiDone  = false;
static bool  g_forceApiOk    = false;

static bool WorForceApiFromRealLib() {
    if (g_forceApiDone) return g_forceApiOk;
    if (!g_realLibHandle) g_realLibHandle = WorDlopenTarget();
    if (!g_realLibHandle) return false;      // 不置 done：句柄可能稍后才可 dlopen
    auto& api = GetApi();
    int n = 0;
#define WOR_SYM(field, sym)                                                    \
    do {                                                                       \
        void* p_ = dlsym(g_realLibHandle, "il2cpp_" sym);                       \
        if (p_) { api.field = reinterpret_cast<decltype(api.field)>(p_); n++; } \
    } while (0)
    WOR_SYM(class_from_name,        "class_from_name");
    WOR_SYM(class_get_field,        "class_get_field_from_name");
    WOR_SYM(field_offset,           "field_get_offset");
    WOR_SYM(field_static_get,       "field_static_get_value");
    WOR_SYM(domain_get,             "domain_get");
    WOR_SYM(domain_assemblies,      "domain_get_assemblies");
    WOR_SYM(domain_assembly_open,   "domain_assembly_open");
    WOR_SYM(assembly_image,         "assembly_get_image");
    WOR_SYM(object_class,           "object_get_class");
    WOR_SYM(class_name,             "class_get_name");
    WOR_SYM(class_method,           "class_get_method_from_name");
    WOR_SYM(runtime_invoke,         "runtime_invoke");
    WOR_SYM(string_new,             "string_new");
    if (!api.string_new) WOR_SYM(string_new, "string_new_wrapper");
    WOR_SYM(image_class_count,      "image_get_class_count");
    WOR_SYM(image_class,            "image_get_class");
    WOR_SYM(value_box,              "value_box");
    WOR_SYM(method_get_name,        "method_get_name");
    WOR_SYM(get_corlib,             "get_corlib");
    WOR_SYM(class_fields,           "class_get_fields");
    WOR_SYM(class_methods,          "class_get_methods");
    WOR_SYM(field_name,             "field_get_name");
    WOR_SYM(class_parent,           "class_get_parent");
    WOR_SYM(field_type,             "field_get_type");
    WOR_SYM(type_name,              "type_get_name");
    WOR_SYM(thread_attach,          "thread_attach");
    WOR_SYM(method_ret_type,        "method_get_return_type");
    WOR_SYM(method_param,           "method_get_param");
    WOR_SYM(method_flags,           "method_get_flags");
#undef WOR_SYM
    g_forceApiOk = (n >= 8) && api.class_from_name && api.class_get_field && api.field_offset &&
                   api.domain_get && api.domain_assemblies && api.object_class && api.class_name;
    if (g_forceApiOk) api.ready = true;      // Load() 有 `if (ready) return true;` ⇒ 之后不会被壳覆盖
    g_forceApiDone = g_forceApiOk;           // 失败允许后续重试
    LOGI(TPL_LOG " ForceApi: 真句柄 dlsym 覆盖 %d 个 il2cpp_* (ready=%d) cf=%p dfn=%p fp=%p img=%p cls=%p",
         n, g_forceApiOk ? 1 : 0, (void*)api.class_from_name, (void*)api.field_offset,
         (void*)api.image_class_count, (void*)api.image_class, (void*)api.class_fields);
    if (!g_forceApiOk)
        LOGE(TPL_LOG " ForceApi: 核心符号不足 (%d) —— 退回 Api::Load", n);
    return g_forceApiOk;
}

static bool EnsureApi() {
    // ★ 真句柄直取优先（唯一可靠路线；血证见 WorForceApiFromRealLib 上方的注释块）
    if (WorForceApiFromRealLib()) return true;
    if (GetApi().ready) return true;
    // 兜底：原有 Api::Load 三级（可能命中 232KB 转发壳，仅作最后手段）
    if (GetApi().Load(TPL_TARGET_LIB)) return true;
    const char* rp = WorLibRealPath();
    if (rp && rp[0] && GetApi().Load(rp)) return true;
    if (GetApi().Load(WOR_LIB_SONAME)) return true;
    LOGE(TPL_LOG " EnsureApi: Load 三级全失败（xdl_open + dynsym）");
    return false;
}

// ★★★ round-4 真机血证（2026-10-03 22:51）★★★
// ForceApi 修好后 api 指针已全部落在真 liblogic.so（`ForceApi: ... cf=0x74288f79d8`，
// 与 `syms cf=0x74288f7a14` 同库），但 SECTION 1 **一行 `img#` 都没打出来**、
// `probe short Entity hits=0`、10 个按名 probe 全 MISS ⇒ 真正的卡点是
// **`Api::ImageOf()` 对全部 63 个程序集都返回 nullptr**（`Unity/Il2CppApi.cpp:895` 那个
// 为「境界刀鸣 1.39.211507」做的直读首字段 + `IsValidHeapPtr` 绕行，在 WOR 上判空）。
// 对策：模块内自建 WorImageOf —— 优先真导出 `il2cpp_assembly_get_image`（src=1），
//       失败则返回 `asm` 自身（src=2，IL2CPP 里 Il2CppImage 内嵌在 Il2CppAssembly 偏移 0，
//       所以 image == asm）；两条都裹 SafeProbe，路线打进日志便于定论。
static void* WorImageOf(void* asm_, int* srcOut) {
    if (!asm_) { if (srcOut) *srcOut = 0; return nullptr; }
    auto& api = GetApi();
    const void* viaExport = nullptr;
    if (api.assembly_image) {
        Il2Cpp::SafeProbe([&]() -> bool { viaExport = api.assembly_image(asm_); return true; });
    }
    if (viaExport) { if (srcOut) *srcOut = 1; return const_cast<void*>(viaExport); }
    // 兜底：IL2CPP 里 `Il2CppImage` 是**内嵌**在 `Il2CppAssembly` 偏移 0 的
    // （所以 `il2cpp_assembly_get_image` 等价于 `&asm->image` == `asm`），
    // 而 `Il2CppApi.cpp:895` 那个「直读首字段」读到的其实是 `Il2CppImage.name`（const char*），
    // 拿它当 image 传给 class_from_name 必然 MISS —— 这正是 round-4 全 MISS 的机制。
    if (srcOut) *srcOut = 2;
    return asm_;
}

static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = WorImageOf(asms[i], nullptr);
        if (!img) continue;
        void* k = api.ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

// ---- 前向声明：⑤-b 段（定义在文件后半）里的两个工具，HookByName 的 B4 诊断要用 ----
static int WorParamCount(void* mi);              // il2cpp_method_get_param_count 包装（缺符号返回 -1）
static const char* TypeNameOf(void* typeObj);    // Il2CppType* -> 类名（不 free）

// 运行时解析出的 ExcuteOnce(argc=1) 的 MethodInfo：非 nullptr = 已过
// argc==1 + IsExecutableRange 双校验（由 ResolveRuntimeLayout 填入），HookByName 优先使用。
static void* g_miExcuteOnce = nullptr;

// 按名路径：类/方法缺失只 log 降级，不整包失败（hook-rules §17）
static bool HookByName(const char* tag, const char* ns, const char* cls, const char* method,
                       int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW("%s api not ready — %s will retry via Tick", tag, method);
        return false;
    }
    auto& api = GetApi();
    void* k = FindClassNs(ns, cls);
    if (!k) k = api.FindClassByShortName(cls);
    if (!k) {
        LOGW("%s class miss %s — degrade", tag, cls);
        return false;
    }
    // 优先用 ResolveRuntimeLayout() 已做 argc==1 + 可执行段双校验的 MethodInfo；
    // 只有 ExcuteOnce 这一条有该待遇（其它槽仍走 GetMethod 精确签名查表）。
    void* mi = nullptr;
    if (g_miExcuteOnce && strcmp(method, "ExcuteOnce") == 0) {
        mi = g_miExcuteOnce;
        LOGI("%s by-name: 使用运行时校验过的 ExcuteOnce mi=%p", tag, mi);
    }
    if (!mi) mi = api.GetMethod(k, method, argc);
    if (!mi) {
        LOGW("%s method miss %s.%s — degrade, install others", tag, cls, method);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW("%s bad method ptr %s.%s", tag, cls, method);
        return false;
    }
    // ---- B4 硬性安全底线：可执行段校验 ----
    // HybridCLR/解释器方法的 MethodInfo::methodPointer 可能是 0x400000000000 这类标记值，
    // 不校验就往那儿写 trampoline 等于自杀。校验不过一律拒绝安装（功能降级但不崩）。
    if (!Il2Cpp::IsExecutableRange((uintptr_t)fn, 16)) {
        LOGE(TPL_LOG " %s by-name: fn=%p 不在可执行段，拒绝 DobbyHook", tag, fn);
        return false;
    }
    // 诊断 INFO：mi/fn/argc/返回类型名（返回类型名取不到就跳过该段）
    {
        const int pc = WorParamCount(mi);
        const int rtArgc = (pc >= 0) ? pc : argc;
        const char* rtn = GetApi().method_ret_type ? TypeNameOf(GetApi().method_ret_type(mi)) : nullptr;
        if (rtn)
            LOGI(TPL_LOG " %s by-name ok: mi=%p fn=%p argc=%d ret=%s", tag, mi, fn, rtArgc, rtn);
        else
            LOGI(TPL_LOG " %s by-name ok: mi=%p fn=%p argc=%d ret=?", tag, mi, fn, rtArgc);
    }
    if (DobbyHook(fn, hook, orig) != 0) {
        LOGW("%s dobby fail @name %s", tag, method);
        return false;
    }
    LOGI("%s hooked %s.%s", tag, cls, method);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;         // 0 = 只走按名
    uint32_t    expectInsn;  // 0 = 不校验序言；建议实测后回填防漂移
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

// 幂等 + 双路径：*orig 非空 = 已挂过（防双 Dobby 叠钩 SIGSEGV）；RVA 失败转按名
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================
// ⑤ 探针挂点（只读）：HpChangeEntitySystem.ExcuteOnce
//    ExcuteAll 循环把 com_hp.lValues[i] 拷到 self+0x60(hData) 后调 ExcuteOnce；
//    故 hook 入口处 self+0x60 = 当前这条 stHpChangeData（含 fValue/AtkRelationData）。
//    环境阶段（WOR_ENV_STAGE=1）本函数不会被安装，代码保留给功能阶段。
// ============================================================
typedef bool (*HasComponentFn)(void* entity, int type, void* methodInfo);
typedef bool (*ExcuteOnceFn)(void* self, void* entity);
static HasComponentFn g_hasComponent = nullptr;
static ExcuteOnceFn  orig_ExcuteOnce = nullptr;

// Entity.m_uID 后备字段偏移已改为运行时可写变量 g_off_ENTITY_MUID（见文件头偏移段）
// g_miExcuteOnce 已在 HookByName 之前定义（前向声明区）

// 读托管 List<T> 的 items 数据指针 + size（只做存活/边界检查，不遍历）
static bool ReadListData(void* list, void** outData, int32_t* outSize) {
    if (!OkPtr(list)) return false;
    void*    items = nullptr;
    int32_t  size  = 0;
    memcpy(&items, reinterpret_cast<uint8_t*>(list) + LIST_OFF_ITEMS, sizeof(items));
    memcpy(&size,  reinterpret_cast<uint8_t*>(list) + LIST_OFF_SIZE,  sizeof(size));
    if (!OkPtr(items) || size < 0) return false;
    *outData = reinterpret_cast<uint8_t*>(items) + ARRAY_OFF_DATA;
    *outSize = size;
    return true;
}

// Fix64 放大（Q20.12：乘整数倍不改变定点标度）；__int128 防溢出后夹取
static inline int64_t ScaleFix64(int64_t raw, int mul) {
    __int128 t = (__int128)raw * (__int128)mul;
    if (t < (__int128)INT64_MIN) return INT64_MIN;
    if (t > (__int128)INT64_MAX) return INT64_MAX;
    return (int64_t)t;
}

static bool Hook_ExcuteOnce(void* self, void* entity) {
    if (!orig_ExcuteOnce) return false;  // orig 未就绪，绝不伪造成功
    if (g_reenter.load()) return orig_ExcuteOnce(self, entity);
    g_reenter.store(true);

    const bool wantInv = g_inv.load();
    const int  mul     = g_dmgMul.load();
    const bool wantLog = g_cfgDiag && g_probeReady.load() && g_probeCount.load() < g_cfgProbeMax;

    if ((wantInv || mul > 1 || wantLog) && OkPtr(self)) {
        const int n = wantLog ? (g_probeCount.fetch_add(1) + 1) : 0;
        uint8_t* sys = reinterpret_cast<uint8_t*>(self);
        uint8_t* hd  = sys + g_off_SYS_HDATA;   // 热路径只读运行时可写变量，绝不调 API

        int64_t  raw = 0;
        int32_t  src = 0, settle = 0, show = 0, skill = 0, formula = 0;
        uint32_t atkE = 0, atkC = 0;
        int32_t  atkU = 0;
        memcpy(&raw,     hd + g_off_HPD_FVALUE,  sizeof(raw));
        memcpy(&src,     hd + g_off_HPD_SRC,      4);
        memcpy(&settle,  hd + g_off_HPD_SETTLE,   4);
        memcpy(&show,    hd + g_off_HPD_SHOW,     4);
        memcpy(&skill,   hd + g_off_HPD_SKILL,    4);
        memcpy(&formula, hd + g_off_HPD_FORMULA,  4);
        memcpy(&atkE,    hd + g_off_HPD_ATK_EUID, 4);
        memcpy(&atkC,    hd + g_off_HPD_ATK_CUID, 4);
        memcpy(&atkU,    hd + g_off_HPD_ATK_UNIT, 4);
        const int64_t rawIn = raw;   // 改写前的原始值（日志用）

        void* comHp = nullptr;
        memcpy(&comHp, sys + g_off_SYS_COM_HP, sizeof(comHp));
        int32_t sz = -1;
        void* lvData = nullptr;            // ★ round-6：提出来，供「经验定位」扫描用
        if (OkPtr(comHp)) {
            void* lv = nullptr;
            memcpy(&lv, reinterpret_cast<uint8_t*>(comHp) + g_off_HPCOMP_LVALUES, sizeof(lv));
            void* data = nullptr;
            if (ReadListData(lv, &data, &sz)) lvData = data;
        }

        int32_t mon = -1, twr = -1, cs = -1;
        uint32_t euid = 0;
        if (OkPtr(entity)) {
            memcpy(&euid, reinterpret_cast<uint8_t*>(entity) + g_off_ENTITY_MUID, 4);
            if (g_hasComponent) {
                mon = g_hasComponent(entity, ECOMP_MONSTERSIGN, nullptr) ? 1 : 0;
                if (mon == 0) {
                    twr = g_hasComponent(entity, ECOMP_TOWERSIGN,   nullptr) ? 1 : 0;
                    if (twr == 0)
                        cs = g_hasComponent(entity, ECOMP_CARDSOLDIER, nullptr) ? 1 : 0;
                }
            }
        }
        const bool isFriendly = (twr == 1 || cs == 1);

        // ★★★ round-6 经验定位（一次跑完就能定偏移，不再靠猜）★★★
        //   真机 round-5 症状：`raw` 恒为 8589934593（正数、20 条不变）⇒ 写闸门 `raw<0`
        //   永不通过 ⇒ 功能形同未装。对策：把 `hData`（0x80 范围）与 `lValues._items`
        //   首区（0x180 范围）按 8 字节步进扫一遍，把所有落在「合理扣血」区间
        //   (raw<0 && raw>-g_rawSaneMax) 的槽位连值打出来。真正的 fValue 必在其中，
        //   同时也能判明「hData 是模板 / 真数据在 lValues」以及 EN 的 stride 到底多少。
        if (n > 0 && n <= 16) {
            char lanes[1400]; lanes[0] = 0; int ln = 0;
            for (int o = 0; o < 0x80; o += 8) {
                int64_t q = 0; memcpy(&q, hd + o, 8);
                if (q < 0 && q > -g_rawSaneMax && ln < (int)sizeof(lanes) - 48)
                    ln += snprintf(lanes + ln, (size_t)(sizeof(lanes) - ln),
                                   " [%02X]=%lld", o, (long long)q);
            }
            LOGI(TPL_LOG " P#%d hd=%p sz=%d negLanes(hd):%s", n, hd, sz, ln ? lanes : " none");
            if (lvData) {
                char lb[1400]; lb[0] = 0; int l2 = 0;
                for (int o = 0; o < 0x180; o += 8) {
                    int64_t q = 0; memcpy(&q, reinterpret_cast<uint8_t*>(lvData) + o, 8);
                    if (q < 0 && q > -g_rawSaneMax && l2 < (int)sizeof(lb) - 48)
                        l2 += snprintf(lb + l2, (size_t)(sizeof(lb) - l2),
                                       " [%02X]=%lld", o, (long long)q);
                }
                LOGI(TPL_LOG " P#%d items=%p negLanes(items):%s", n, lvData, l2 ? lb : " none");
            }
            // ★ round-7：只在前 3 帧打「全 64bit 视图」，用来终审定案（勿常态化，日志会爆）
            if (n <= 3) {
                char qb[420]; int qn = 0; qb[0] = 0;
                for (int o = 0; o < 0x80; o += 8) {
                    int64_t q = 0; memcpy(&q, hd + o, 8);
                    qn += snprintf(qb + qn, (size_t)(sizeof(qb) - qn), " %02X:%016llX",
                                   o, (unsigned long long)q);
                }
                LOGI(TPL_LOG " P#%d qmap(hd):%s", n, qb);
            }
        }

        // ---- 写前合理性闸门（B5）：偏移解析错时 raw 会是垃圾值，垃圾值绝不能写回游戏内存 ----
        //   正常承伤 = 负数且量级远小于 2^44；超出范围只留探针日志，不改写。
        //   `raw < 0` 本身必须保留：负值 = 扣血，这是改装核心语义。
        const bool rawSane = (raw < 0 && raw > -g_rawSaneMax);

        // ---- 无敌：我方单位（Tower/CardSoldier）承伤清零 ----
        if (wantInv && rawSane && isFriendly) {
            const int64_t zero = 0;
            memcpy(hd + g_off_HPD_FVALUE, &zero, sizeof(zero));
            const int m = g_modCount.fetch_add(1);
            if (m < MOD_LOG_MAX)
                LOGI(TPL_LOG " INV e=%p uid=%u raw=%lld->0", entity, euid, (long long)rawIn);
        }
        // ---- 伤害倍率：对敌（MonsterSign）伤害放大 ----
        else if (mul > 1 && rawSane && mon == 1) {
            const int64_t scaled = ScaleFix64(raw, mul);
            memcpy(hd + g_off_HPD_FVALUE, &scaled, sizeof(scaled));
            const int m = g_modCount.fetch_add(1);
            if (m < MOD_LOG_MAX)
                LOGI(TPL_LOG " DMG x%d e=%p uid=%u raw=%lld->%lld", mul, entity, euid,
                     (long long)rawIn, (long long)scaled);
        }

        // ★ C2/C1 修正（2026-10-04）：探针行**只在真正处于探针窗口时**打。
        //   原实现把 P# 行放在 if 块内无条件执行，`n` 到 0 之后仍每帧打 `P#0 …`
        //   ⇒ 开关一打开就洪水日志（与「正式构建静音」的目标矛盾）。INV/DMG 行不受影响
        //   （它们由 g_modCount < MOD_LOG_MAX 单独限流），故正式档仍能凭 INV/DMG 判定生效。
        if (n > 0)
        LOGI(TPL_LOG " P#%d e=%p uid=%u mon=%d twr=%d cs=%d sz=%d raw=%lld v=%.3f "  // NOLINT
                    "src=%d set=%d show=%d atk=%u/%u u%d sk=%d fm=%d",
             n, entity, euid, mon, twr, cs, sz, (long long)rawIn, (double)rawIn / FIX64_ONE,
             src, settle, show, atkE, atkC, atkU, skill, formula);
    }

    g_reenter.store(false);
    return orig_ExcuteOnce(self, entity);
}

// ============================================================
// ⑤-b e2 运行时布局解析（按名 + 按类型链，零静态 RVA）
//   · 额外导出走**本地 dlsym**（**不动共享层 Unity/Il2CppApi**）：本仓库约定
//     「游戏模块自己 dlsym」，见 Games/Archero2Mod.cpp:1164-1241 同款写法。
//   · 目标库 = liblogic.so（**不是** sec_lib_il2cpp()；包内 libil2cpp.so 只是 7z 解压壳）。
//   · 本段**只读**：绝不写游戏内存；任何一步失败只记日志 + 保持国服兜底偏移、功能降级。
//   · 名字事实（父代理 2026-10-03 对 base_1.6.38.829.1/global-metadata.dat 逐条 exact 命中）：
//     HpChangeEntitySystem / CSharpBattle.Battle / com_hp / hData / mAtkEntity / fDamage /
//     lValues / fValue / eRelationData / eUid / cUid / iUnitID / iAttackSkillID / iFormularID /
//     ExcuteOnce / HasComponent / <m_uID>k__BackingField / EComponentType 全部在；
//     ⚠️ eDamageShowType **0 次** ⇒ HPD_SHOW 走「eTextShowType → textShowType → 类型名
//        ETextShowType」三段兜底；
//     ⚠️ CSharpBattle.Battle.Entity **0 次** ⇒ Entity 只许从 mAtkEntity 类型链拿，
//        **禁止** FindClassByShortName("Entity")（会撞到别的同名类）。
//   · S1~S6 结构化侦察（一次装机定案）：S1/S2/S3/S5/S6 无条件全跑，S4 对命中类跑；
//     全部只读，结果同时进 logcat 与 wor_tables.txt（adb pull 取回）。
// ============================================================
#define WOR_OFF_MISS 0xFFFFFFFFu   // 对账表用：运行时未解析到（保持国服兜底值）

// ---- 本地符号表（9 个额外 il2cpp_* 导出，全部可空）----
typedef void*       (*w_fields_t)(void*, void**);
typedef const char* (*w_fname_t)(void*);
typedef void*       (*w_ftype_t)(void*);
typedef void*       (*w_from_type_t)(void*);
typedef void*       (*w_methods_t)(void*, void**);
typedef uint32_t    (*w_pcount_t)(void*);
typedef const char* (*w_cns_t)(void*);
typedef const char* (*w_iname_t)(void*);
typedef void*       (*w_cimage_t)(void*);

struct WorSyms {
    w_fields_t    class_fields    = nullptr;
    w_fname_t     field_name      = nullptr;
    w_ftype_t     field_type      = nullptr;
    w_from_type_t class_from_type = nullptr;
    w_methods_t   class_methods   = nullptr;
    w_pcount_t    param_count     = nullptr;
    w_cns_t       class_ns        = nullptr;
    w_iname_t     image_name      = nullptr;
    w_cimage_t    class_image     = nullptr;
    bool core = false;   // 关键集齐；缺任一 ⇒ 放弃运行时布局解析，保持国服兜底偏移
};
static WorSyms g_sym;
static bool g_symTried = false;

static void EnsureWorSyms() {
    if (g_sym.core) return;          // 已就绪才短路；失败允许后续重试（否则一次失败就永久放弃）
    g_symTried = true;
    void* h = WorDlopenTarget();     // ★ 三级兜底（裸名必失败，见文件上方血证段）
    if (!h) {
        LOGW(TPL_LOG " syms: 取 %s 句柄失败 — 保持国服兜底偏移", TPL_TARGET_LIB);
        return;
    }
    g_sym.class_fields    = (w_fields_t)dlsym(h, "il2cpp_class_get_fields");
    g_sym.field_name      = (w_fname_t)dlsym(h, "il2cpp_field_get_name");
    g_sym.field_type      = (w_ftype_t)dlsym(h, "il2cpp_field_get_type");
    g_sym.class_from_type = (w_from_type_t)dlsym(h, "il2cpp_class_from_il2cpp_type");
    if (!g_sym.class_from_type)
        g_sym.class_from_type = (w_from_type_t)dlsym(h, "il2cpp_class_from_type");
    g_sym.class_methods   = (w_methods_t)dlsym(h, "il2cpp_class_get_methods");
    g_sym.param_count     = (w_pcount_t)dlsym(h, "il2cpp_method_get_param_count");
    g_sym.class_ns        = (w_cns_t)dlsym(h, "il2cpp_class_get_namespace");
    g_sym.image_name      = (w_iname_t)dlsym(h, "il2cpp_image_get_name");
    g_sym.class_image     = (w_cimage_t)dlsym(h, "il2cpp_class_get_image");
    g_sym.core = g_sym.class_fields && g_sym.field_name && g_sym.field_type &&
                 g_sym.class_from_type && g_sym.class_methods && g_sym.param_count;
    LOGI(TPL_LOG " syms cf=%p fn=%p ft=%p cft=%p cm=%p pc=%p ns=%p img=%p cimg=%p core=%d",
         (void*)g_sym.class_fields, (void*)g_sym.field_name, (void*)g_sym.field_type,
         (void*)g_sym.class_from_type, (void*)g_sym.class_methods, (void*)g_sym.param_count,
         (void*)g_sym.class_ns, (void*)g_sym.image_name, (void*)g_sym.class_image,
         g_sym.core ? 1 : 0);
    if (!g_sym.core)
        LOGW(TPL_LOG " syms: 关键导出缺失 — 放弃运行时布局解析，保持国服兜底偏移");
}

// 前向声明过的两个包装（HookByName 的 B4 诊断用）
static int WorParamCount(void* mi) {
    if (!mi || !g_sym.param_count) return -1;
    return (int)g_sym.param_count(mi);
}

// ---- 落盘：logcat 会被轮转冲掉，文件可 adb pull 全量取回 ----
//  实测（父代理在 96f0d883 上验过）：/sdcard/Android/data/<pkg>/files/ 本进程可写、shell 可读回；
//  /data/data/<pkg>/ 连列目录都 Permission denied（非 debuggable），下面两条是**预期失败**的兜底。
#define WOR_TBL_PRIMARY "/sdcard/Android/data/com.ap.watcherofrealms/files/wor_tables.txt"
#define WOR_TBL_ALT     "/storage/emulated/0/Android/data/com.ap.watcherofrealms/files/wor_tables.txt"
static FILE* g_tblFile = nullptr;
static const char* g_tblPath = nullptr;

static FILE* WorTblOpen(const char* mode) {
    static const char* kPaths[] = {
        WOR_TBL_PRIMARY,
        WOR_TBL_ALT,
        "/data/data/com.ap.watcherofrealms/files/wor_tables.txt",  // 预期 deny
        "/data/data/com.ap.watcherofrealms/wor_tables.txt",        // 预期 deny
    };
    // ★ C1：wor_cfg.txt 的 tbl_path 若给了，最优先试它（换设备/换包名时不用重编）
    if (g_cfgTblPath[0]) {
        FILE* fc = fopen(g_cfgTblPath, mode);
        if (fc) {
            if (!g_tblPath) g_tblPath = g_cfgTblPath;
            LOGI(TPL_LOG " tables file open ok (cfg %s): %s", mode, g_cfgTblPath);
            return fc;
        }
        LOGW(TPL_LOG " tables file: cfg tbl_path 打不开 %s errno=%d（回落候选表）",
             g_cfgTblPath, errno);
    }
    for (size_t i = 0; i < sizeof(kPaths) / sizeof(kPaths[0]); i++) {
        FILE* f = fopen(kPaths[i], mode);
        if (f) {
            if (!g_tblPath) g_tblPath = kPaths[i];
            LOGI(TPL_LOG " tables file open ok (%s): %s", mode, kPaths[i]);
            return f;
        }
        LOGW(TPL_LOG " tables file open fail (expect-denied) %s: %s errno=%d", mode, kPaths[i], errno);
    }
    LOGW(TPL_LOG " tables file: 无可用落盘路径，只走 logcat");
    return nullptr;
}

// 文件输出（显式传 FILE*，热路径/后台线程各用各的，避免跨线程共用）
#define WTBL(fp, ...) do { if (fp) { fprintf((fp), __VA_ARGS__); fflush(fp); } } while (0)

// ---- 名字工具 ----
static const char* ShortNameOf(const char* n) {   // 取 '.' / '/' / '+' 之后的短名
    if (!n) return "";
    const char* s = n;
    for (const char* p = n; *p; p++)
        if (*p == '.' || *p == '/' || *p == '+') s = p + 1;
    return s;
}

static bool NameHasCI(const char* got, const char* needle) {   // 大小写不敏感子串
    if (!got || !needle) return false;
    for (size_t i = 0; got[i]; i++) {
        size_t j = 0;
        while (needle[j]) {
            char a = got[i + j], b = needle[j];
            if (a >= 'A' && a <= 'Z') a = (char)(a - 'A' + 'a');
            if (b >= 'A' && b <= 'Z') b = (char)(b - 'A' + 'a');
            if (a != b) break;
            j++;
        }
        if (!needle[j]) return true;
    }
    return false;
}

// 归一化字段名：只留字母数字并小写、丢掉 '<' '>' '_'，再截掉 "kbackingfield" 尾缀
// ⇒ "<m_uID>k__BackingField" 与 "m_uID" 归一化后同为 "muid"（跨版本命名容差）
static void NormName(const char* s, char* out, size_t cap) {
    if (!out || cap == 0) return;
    size_t o = 0;
    for (size_t i = 0; s && s[i] && o + 1 < cap; i++) {
        char c = s[i];
        if (c >= 'A' && c <= 'Z') c = (char)(c - 'A' + 'a');
        if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) out[o++] = c;
    }
    out[o] = 0;
    static const char kSuffix[] = "kbackingfield";
    const size_t sl = sizeof(kSuffix) - 1;
    if (o > sl && strcmp(out + o - sl, kSuffix) == 0) out[o - sl] = 0;
}

static bool NameEqNorm(const char* a, const char* b) {
    char na[80] = {0}, nb[80] = {0};
    NormName(a, na, sizeof(na));
    NormName(b, nb, sizeof(nb));
    return na[0] && strcmp(na, nb) == 0;
}

// ---- 类/字段工具（全部防御式：迭代器带 4096 上限，热更类迭代器可能不终止）----
static const char* NsOf(void* k) {
    if (!k || !g_sym.class_ns) return "?";
    const char* n = g_sym.class_ns(k);
    return n ? n : "";
}

static void* FieldClsOf(void* f) {
    if (!f || !g_sym.field_type || !g_sym.class_from_type) return nullptr;
    void* t = g_sym.field_type(f);
    return t ? g_sym.class_from_type(t) : nullptr;
}

static bool TypeNameEq(void* klass, const char* want) {
    if (!klass || !want) return false;
    auto& api = GetApi();
    const char* cn = api.class_name ? api.class_name(klass) : nullptr;
    if (!cn) return false;
    return strcmp(cn, want) == 0 || strcmp(ShortNameOf(cn), want) == 0;
}

static const char* TypeNameOf(void* typeObj) {   // Il2CppType* -> 类名（不 free，见 Il2CppApi.cpp 警告）
    if (!typeObj || !g_sym.class_from_type) return "?";
    void* k = g_sym.class_from_type(typeObj);
    if (!k) return "?";
    auto& api = GetApi();
    const char* n = api.class_name ? api.class_name(k) : nullptr;
    return n ? n : "?";
}

static void* FindFieldByName(void* klass, const char* name) {
    if (!klass || !name || !g_sym.class_fields || !g_sym.field_name) return nullptr;
    void* iter = nullptr;
    int   guard = 0;
    void* normHit = nullptr;
    while (void* f = g_sym.class_fields(klass, &iter)) {
        if (++guard > 4096) break;
        const char* fn = g_sym.field_name(f);
        if (!fn) continue;
        if (strcmp(fn, name) == 0) return f;                 // 精确优先
        if (!normHit && NameEqNorm(fn, name)) normHit = f;   // 归一化兜底
    }
    return normHit;
}

static bool HasField(void* klass, const char* name) { return FindFieldByName(klass, name) != nullptr; }

static void* FieldTypeKlass(void* klass, const char* fname) {
    void* f = FindFieldByName(klass, fname);
    return f ? FieldClsOf(f) : nullptr;
}

// 对账行（统一格式）：
//   logcat : [Wor] off %-22s rt=0x%X cn=0x%X %s %s      ← %s#1 = match / DIFF / keep-cn(default)
//   文件   : off %-24s cn=0x%-4X rt=0x%-4X %s %s        ← S6 对账表格式
//   %s#2 = 命中段：name / alt-name / type-name / (未命中原因)
static void ReconcileLog(const char* what, int32_t cn, int32_t rt, const char* how) {
#if !WOR_DIAG_FULL
    // ★ C2：正式构建下偏移对账整体编掉（连同它的格式串）。
    //   调用点（OffOfField*）保持不变 —— 它们只是把参数传进来，这里直接丢弃。
    (void)what; (void)cn; (void)rt; (void)how;
#else
    if (!g_cfgDiag) return;   // ★ C1：运行时总闸 —— 排障构建也能用 cfg 静音洪水日志
    const bool miss = ((uint32_t)rt == WOR_OFF_MISS);
    const char* verdict = miss ? "keep-cn(default)" : (rt == cn ? "match" : "DIFF");
    if (miss)
        LOGW(TPL_LOG " off %-22s rt=0x%X cn=0x%X %s %s", what, (unsigned)rt, (unsigned)cn, verdict,
             how ? how : "");
    else
        LOGI(TPL_LOG " off %-22s rt=0x%X cn=0x%X %s %s", what, (unsigned)rt, (unsigned)cn, verdict,
             how ? how : "");
    WTBL(g_tblFile, "off %-24s cn=0x%-4X rt=0x%-4X %s %s\n", what, (unsigned)cn, (unsigned)rt,
         verdict, how ? how : "");
#endif  // WOR_DIAG_FULL
}

// 按名解析字段偏移；找不到保持国服兜底值（功能降级但不崩）
static int32_t OffOfField(void* klass, const char* fname, int32_t cn, const char* what) {
    if (!klass) {
        ReconcileLog(what, cn, (int32_t)WOR_OFF_MISS, "(klass=null)");
        return cn;
    }
    void* f = FindFieldByName(klass, fname);
    if (!f) {
        ReconcileLog(what, cn, (int32_t)WOR_OFF_MISS, "(field 未找到)");
        return cn;
    }
    const int32_t rt = (int32_t)GetApi().field_offset(f);
    ReconcileLog(what, cn, rt, "name");
    return rt;
}

// 三段兜底：① 字段名精确 ② 备选字段名 ③ 按字段**类型名**（短名相等）
// 第 ③ 段可能多命中：全打出来并取第一个（`off <what> multi-cand` + `cand %s @0x%X`）
static int32_t OffOfFieldChain(void* klass, const char* n1, const char* n2, const char* typeName,
                               int32_t cn, const char* what) {
    if (!klass) {
        ReconcileLog(what, cn, (int32_t)WOR_OFF_MISS, "(klass=null)");
        return cn;
    }
    void* f = nullptr;
    const char* how = "name";
    if (n1) f = FindFieldByName(klass, n1);
    if (!f && n2) {
        f = FindFieldByName(klass, n2);
        how = "alt-name";
    }
    if (!f && typeName) {
        void* iter = nullptr;
        int   guard = 0, cnt = 0;
        while (void* ff = g_sym.class_fields(klass, &iter)) {
            if (++guard > 4096) break;
            if (!TypeNameEq(FieldClsOf(ff), typeName)) continue;
            const char* fn = g_sym.field_name ? g_sym.field_name(ff) : nullptr;
            const int32_t off = (int32_t)GetApi().field_offset(ff);
            LOGI(TPL_LOG " off %s cand %s @0x%X", what, fn ? fn : "?", (unsigned)off);
            WTBL(g_tblFile, "  cand %s @0x%X\n", fn ? fn : "?", (unsigned)off);
            if (!f) f = ff;
            cnt++;
        }
        if (cnt > 1) {
            LOGW(TPL_LOG " off %s multi-cand n=%d — 取第一个（见 cand 行）", what, cnt);
            WTBL(g_tblFile, "off %s multi-cand n=%d\n", what, cnt);
        }
        how = "type-name";
    }
    if (!f) {
        ReconcileLog(what, cn, (int32_t)WOR_OFF_MISS, "(3 段兜底全失败)");
        return cn;
    }
    const int32_t rt = (int32_t)GetApi().field_offset(f);
    ReconcileLog(what, cn, rt, how);
    return rt;
}

// 嵌套结构（eRelationData.eUid 等）：外层字段偏移 + 内层字段偏移
static int32_t OffOfNested(void* outerK, const char* outerF, void* innerK, const char* innerF,
                           int32_t cn, const char* what) {
    void* of = FindFieldByName(outerK, outerF);
    void* inf = innerK ? FindFieldByName(innerK, innerF) : nullptr;
    if (!of || !inf) {
        ReconcileLog(what, cn, (int32_t)WOR_OFF_MISS,
                     of ? "(内层字段未找到)" : "(外层字段未找到)");
        return cn;
    }
    const int32_t rt = (int32_t)GetApi().field_offset(of) + (int32_t)GetApi().field_offset(inf);
    ReconcileLog(what, cn, rt, "nested");
    return rt;
}

#if WOR_DIAG_FULL
// ---- 字段表 / 方法表 dump（S3 命中类与 S5 目标类共用）----
// ★ C2：本段（DumpClassFields/DumpClassMethods）在 WOR_DIAG_FULL=0 时整段编掉。
//   紧随其后的 CollectCandidates 是**功能代码**（ResolveHookCandidates 依赖），不在本段内。
#define WOR_FIELD_DUMP_MAX 40
static void DumpClassFields(FILE* fp, void* klass, const char* tag, bool toLog) {
    if (!klass) {
        LOGW(TPL_LOG " field %s: klass=null", tag);
        return;
    }
    if (!g_sym.class_fields || !g_sym.field_name) return;
    auto& api = GetApi();
    void* iter = nullptr;
    int n = 0, idx = 0, guard = 0;
    while (void* f = g_sym.class_fields(klass, &iter)) {
        if (++guard > 4096) break;
        if (n >= WOR_FIELD_DUMP_MAX) {
            LOGI(TPL_LOG " field %s ... (truncated)", tag);
            WTBL(fp, "field %s ... (truncated)\n", tag);
            break;
        }
        const char* fn = g_sym.field_name(f);
        const uint32_t off = (uint32_t)api.field_offset(f);
        const char* tn = "?";
        void* fk = FieldClsOf(f);
        if (fk && api.class_name) {
            const char* t = api.class_name(fk);
            if (t) tn = t;
        }
        // logcat 用 B3 规定格式；文件沿用 Archero2Mod.cpp 的 f[%d]=%s : %s（带下标 + 偏移）
        if (toLog) LOGI(TPL_LOG " field %s.%s = 0x%X : %s", tag, fn ? fn : "?", (unsigned)off, tn);
        WTBL(fp, "f[%d]=%s : %s @0x%X\n", idx, fn ? fn : "?", tn, (unsigned)off);
        n++;
        idx++;
    }
    if (toLog) LOGI(TPL_LOG " field %s total=%d", tag, n);
    WTBL(fp, "field %s total=%d\n", tag, n);
}

static void DumpClassMethods(FILE* fp, void* klass, const char* tag, bool toLog) {
    if (!klass) {
        LOGW(TPL_LOG " m %s: klass=null", tag);
        return;
    }
    if (!g_sym.class_methods) return;
    auto& api = GetApi();
    void* iter = nullptr;
    int n = 0, guard = 0;
    while (void* m = g_sym.class_methods(klass, &iter)) {
        if (++guard > 4096) break;
        const char* mn = api.method_get_name ? api.method_get_name(m) : nullptr;
        const uint32_t argc = g_sym.param_count ? g_sym.param_count(m) : 0;
        char sig[128] = {0};
        size_t so = 0;
        if (api.method_param && g_sym.class_from_type) {
            for (uint32_t pi = 0; pi < argc && pi < 6 && so + 2 < sizeof(sig); pi++) {
                const char* pn = TypeNameOf(api.method_param(m, pi));
                so += (size_t)snprintf(sig + so, sizeof(sig) - so, "%s%s", pi ? "," : "", pn);
            }
        }
        const void* fn = *reinterpret_cast<void**>(m);
        const char* rtn = "?";
        if (api.method_ret_type) rtn = TypeNameOf(api.method_ret_type(m));
        char extra[192] = {0};
        if (mn && NameHasCI(mn, "xcute") && api.method_param && argc >= 1)
            snprintf(extra, sizeof(extra), " p0=%s", TypeNameOf(api.method_param(m, 0)));
        if (toLog)
            LOGI(TPL_LOG " m[%d]=%s(%s) argc=%u mi=%p fn=%p ret=%s%s", n, mn ? mn : "?", sig, argc,
                 m, fn, rtn, extra);
        WTBL(fp, "m[%d]=%s(%s) argc=%u mi=%p fn=%p ret=%s%s\n", n, mn ? mn : "?", sig, argc, m, fn,
             rtn, extra);
        n++;
    }
    if (toLog) LOGI(TPL_LOG " m %s total=%d", tag, n);
    WTBL(fp, "m %s total=%d\n", tag, n);
}
#endif  // WOR_DIAG_FULL（字段表/方法表 dump）

// ---- 方法候选（含父类链：HasComponent 可能在 Entity 的基类上）----
struct CandInfo {
    void*    mi;
    void*    fn;
    uint32_t argc;
    char     p0[80];
    char     ret[80];
};
#define WOR_CAND_MAX 32
#define WOR_PARENT_MAX 8

static int CollectCandidates(void* klass, const char* name, CandInfo* out, int cap) {
    if (!klass || !name || !out || cap <= 0 || !g_sym.class_methods) return 0;
    auto& api = GetApi();
    int n = 0;
    void* k = klass;
    for (int depth = 0; k && depth < WOR_PARENT_MAX && n < cap; depth++) {
        void* iter = nullptr;
        int guard = 0;
        while (void* m = g_sym.class_methods(k, &iter)) {
            if (++guard > 4096) break;
            const char* mn = api.method_get_name ? api.method_get_name(m) : nullptr;
            if (!mn || strcmp(mn, name) != 0) continue;
            CandInfo& c = out[n];
            memset(&c, 0, sizeof(c));
            c.mi = m;
            c.fn = *reinterpret_cast<void**>(m);
            c.argc = g_sym.param_count ? g_sym.param_count(m) : 0;
            snprintf(c.p0, sizeof(c.p0), "%s",
                     (api.method_param && c.argc >= 1) ? TypeNameOf(api.method_param(m, 0)) : "-");
            const char* rtn = api.method_ret_type ? TypeNameOf(api.method_ret_type(m)) : "?";
            snprintf(c.ret, sizeof(c.ret), "%s", rtn ? rtn : "?");
            n++;
            if (n >= cap) break;
        }
        k = api.class_parent ? api.class_parent(k) : nullptr;
    }
    return n;
}

#if WOR_DIAG_FULL
// ============================================================
// ★ C2：以下一直到 ScanAllImages 结束（S2 名探针 / S1 镜像诊断 / SELFTEST /
//   S3 结构扫描与其预算全局量）**整段都是诊断**，WOR_DIAG_FULL=0 时全部编掉。
//   功能路径不受影响：它只经过 FindSysClass / FieldTypeKlass / ApplyRuntimeOffsets /
//   ResolveHookCandidates（这些都在本段之外）。
// ============================================================
// ---- S2 name-probe ----
static void ProbeName(const char* ns, const char* name) {
    auto& api = GetApi();
    void* found = nullptr;
    if (ns && ns[0]) {
        size_t an = 0;
        void** asms = api.Assemblies(&an);
        for (size_t i = 0; asms && i < an && !found; i++) {
            void* img = WorImageOf(asms[i], nullptr);
            if (img) found = api.ClassFromName(img, ns, name);
        }
    } else {
        found = api.FindClassByShortName(name);   // 短名完全相等（strcmp）
    }
    LOGI(TPL_LOG " probe ns=%s cls=%s -> %p (%s)", ns ? ns : "", name, found, found ? "HIT" : "MISS");
    WTBL(g_tblFile, "probe ns=%s cls=%s -> %p (%s)\n", ns ? ns : "", name, found,
         found ? "HIT" : "MISS");
}

static int ProbeShortAll(const char* shortName, int cap) {
    auto& api = GetApi();
    int hit = 0;
    size_t an = 0;
    void** asms = api.Assemblies(&an);
    if (!asms || !api.image_class_count || !api.image_class || !api.class_name) return 0;
    for (size_t a = 0; a < an && hit < cap; a++) {
        void* img = WorImageOf(asms[a], nullptr);
        if (!img) continue;
        const uint32_t cc = api.image_class_count(img);
        for (uint32_t i = 0; i < cc && hit < cap; i++) {
            void* k = api.image_class(img, i);
            if (!k) continue;
            const char* cn = api.class_name(k);
            if (!cn || strcmp(cn, shortName) != 0) continue;
            // ⚠️ 这里只是**报告**同名类有哪些；Entity 的取用只许走 mAtkEntity 类型链
            LOGI(TPL_LOG " probe short %s -> %p (HIT) ns=%s", shortName, k, NsOf(k));
            WTBL(g_tblFile, "probe short %s -> %p (HIT) ns=%s\n", shortName, k, NsOf(k));
            hit++;
        }
    }
    LOGI(TPL_LOG " probe short %s hits=%d %s", shortName, hit, hit ? "" : "(MISS)");
    WTBL(g_tblFile, "probe short %s hits=%d\n", shortName, hit);
    return hit;
}

static void ProbeMethodName(void* klass, const char* name) {
    auto& api = GetApi();
    void* found = nullptr;
    int argc = -1;
    void* k = klass;
    for (int d = 0; k && d < WOR_PARENT_MAX && !found; d++) {
        void* iter = nullptr;
        int guard = 0;
        while (g_sym.class_methods && (guard <= 4096)) {
            void* m = g_sym.class_methods(k, &iter);
            if (!m) break;
            guard++;
            const char* mn = api.method_get_name ? api.method_get_name(m) : nullptr;
            if (mn && strcmp(mn, name) == 0) {
                found = m;
                argc = g_sym.param_count ? (int)g_sym.param_count(m) : -1;
                break;
            }
        }
        k = api.class_parent ? api.class_parent(k) : nullptr;
    }
    LOGI(TPL_LOG " probe m %s -> %p argc=%d (%s)", name, found, argc, found ? "HIT" : "MISS");
    WTBL(g_tblFile, "probe m %s -> %p argc=%d (%s)\n", name, found, argc, found ? "HIT" : "MISS");
}

// ============================================================
// S1：images + 环境事实（e2 下无条件跑；= B6 自诊断的等价物，只读不崩）
// ============================================================
// 安全取 C 字符串（指针可能无效；逐字节拷贝，只留可打印 ASCII）
static void WorSafeStr(const char* p, char* buf, int cap) {
    if (cap <= 0) return;
    buf[0] = 0;
    if (!p) return;
    Il2Cpp::SafeProbe([&]() -> bool {
        for (int i = 0; i < cap - 1; i++) {
            const char c = p[i];
            if (!c) break;
            buf[i] = (c >= 32 && c < 127) ? c : '?';
            buf[i + 1] = 0;
        }
        return true;
    });
}

// ★★★ round-5 决定性自证（一次把「到底是 image 指针错、还是 class_from_name 本身不可用」
//     这两条路分开）。三组对照：
//     ① 控制实验：`get_corlib()` + `class_from_name(img,"System","String")` —— 任何 IL2CPP 都该命中。
//        若连它都 MISS ⇒ 不是 image 问题，而是 class_from_name 语义/metadata 加密问题。
//     ② image 候选对照：对 asm[0] 分别用 (a) 真导出 assembly_get_image、(b) asm 自身、
//        (c) 直读首字段（IL2CPP v24 里那其实是内嵌 Il2CppImage.name 的 const char*）
//        各做一次 `System.String` 查询 + image_class_count + image_get_name。
//     ③ 把首字段当 C 字符串打印 —— 若打出 "Assembly-CSharp" 之类，就彻底坐实 (c) 是名字而非 image。
static void WorClassLookupSelfTest() {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    LOGI(TPL_LOG " SELFTEST asm=%zu", n);
    WTBL(g_tblFile, "\n===== SELFTEST =====\nasm=%zu\n", n);

    void* corlib = nullptr;
    if (api.get_corlib) Il2Cpp::SafeProbe([&]() -> bool { corlib = api.get_corlib(); return true; });
    void* kSys = nullptr;
    if (corlib) Il2Cpp::SafeProbe([&]() -> bool { kSys = api.ClassFromName(corlib, "System", "String"); return true; });
    LOGI(TPL_LOG " SELFTEST ① corlib=%p class_from_name(corlib,System,String)=%p", corlib, kSys);
    WTBL(g_tblFile, "① corlib=%p System.String=%p\n", corlib, kSys);

    if (!asms || n == 0) return;
    void* a0 = asms[0];
    const void* viaExpC = nullptr;
    if (api.assembly_image) Il2Cpp::SafeProbe([&]() -> bool { viaExpC = api.assembly_image(a0); return true; });
    void* viaExp = const_cast<void*>(viaExpC);
    void* viaRaw = nullptr;
    Il2Cpp::SafeProbe([&]() -> bool { viaRaw = *reinterpret_cast<void**>(a0); return true; });
    char rawStr[64];
    WorSafeStr(reinterpret_cast<const char*>(viaRaw), rawStr, (int)sizeof(rawStr));
    LOGI(TPL_LOG " SELFTEST ③ asm0=%p export=%p rawFirst=%p rawAsCstr='%s'", a0, viaExp, viaRaw, rawStr);
    WTBL(g_tblFile, "③ asm0=%p export=%p rawFirst=%p rawAsCstr='%s'\n", a0, viaExp, viaRaw, rawStr);

    struct Cand { const char* nm; void* img; };
    const Cand cs[3] = { { "export", viaExp }, { "asm自身", a0 }, { "raw首字段", viaRaw } };
    for (int i = 0; i < 3; i++) {
        if (!cs[i].img) { LOGI(TPL_LOG " SELFTEST ② cand[%s]=null", cs[i].nm); continue; }
        void* k1 = nullptr; uint32_t cc = 0; const char* inm = nullptr;
        Il2Cpp::SafeProbe([&]() -> bool { k1 = api.ClassFromName(cs[i].img, "System", "String"); return true; });
        if (api.image_class_count)
            Il2Cpp::SafeProbe([&]() -> bool { cc = api.image_class_count(cs[i].img); return true; });
        if (g_sym.image_name)
            Il2Cpp::SafeProbe([&]() -> bool { inm = g_sym.image_name(cs[i].img); return true; });
        char nb[64];
        WorSafeStr(inm, nb, (int)sizeof(nb));
        LOGI(TPL_LOG " SELFTEST ② cand[%s] img=%p System.String=%p cnt=%u name='%s'",
             cs[i].nm, cs[i].img, k1, cc, nb);
        WTBL(g_tblFile, "② cand[%s] img=%p System.String=%p cnt=%u name='%s'\n",
             cs[i].nm, cs[i].img, k1, cc, nb);
    }
}

static void DumpBattleDiagnostics(const char* why) {
    auto& api = GetApi();
    LOGI(TPL_LOG " ===== SECTION 1: images ===== why=%s ready=%d domain=%p core=%d",
         why, api.ready ? 1 : 0, api.Domain(), g_sym.core ? 1 : 0);
    WTBL(g_tblFile, "===== SECTION 1: images =====\nwhy=%s ready=%d domain=%p core=%d\n", why,
         api.ready ? 1 : 0, api.Domain(), g_sym.core ? 1 : 0);
    size_t an = 0;
    void** asms = api.Assemblies(&an);
    LOGI(TPL_LOG " asm count=%d", (int)an);
    WTBL(g_tblFile, "asm count=%d\n", (int)an);
    if (!asms) return;
    WorClassLookupSelfTest();          // ★ round-5：先做决定性对照实验，再列镜像
    if (an > 64) an = 64;
    // ★ round-4：**不再 `if (!img) continue;`** —— 全部打印（asm/src/img/cnt/name），
    //   否则 ImageOf 判空时整段 SECTION 1 静默为空，白白浪费一次诊断机会（已踩过）。
    for (size_t i = 0; i < an; i++) {
        int src = 0;
        void* img = WorImageOf(asms[i], &src);
        uint32_t cc = 0;
        const char* in = nullptr;
        if (img) {
            if (api.image_class_count)
                Il2Cpp::SafeProbe([&]() -> bool { cc = api.image_class_count(img); return true; });
            if (g_sym.image_name)
                Il2Cpp::SafeProbe([&]() -> bool { in = g_sym.image_name(img); return true; });
        }
        char nb[64];
        WorSafeStr(in, nb, (int)sizeof(nb));
        LOGI(TPL_LOG " img#%d asm=%p src=%d img=%p cnt=%u name='%s'",
             (int)i, asms[i], src, img, cc, nb);
        WTBL(g_tblFile, "img#%d asm=%p src=%d img=%p cnt=%u name='%s'\n",
             (int)i, asms[i], src, img, cc, nb);
    }
}

// ============================================================
// S3：结构化全域扫描（后台线程；结构特征而非名字，命中即 dump 全表）
//   判据 A：方法名含 "xcute"（覆盖 ExcuteOnce/ExecuteOnce/Excute/Execute）
//   判据 B：字段同时有 hData 与 com_hp（SYS_OFF_COM_HP + SYS_OFF_HDATA 极特征化搭配）
//   判据 C：类短名含 HpChange / HPChange / ChangeHp
//   判据 D：类短名含 Battle 且（有 hData 字段 或 有含 xcute 的方法）
// ============================================================
// ★ C1：扫描预算改为可运行时调（g_cfgScanMaxClasses / g_cfgScanMaxSec），
//   内建默认值 40000 / 60s 写在上方 C1 段。
static std::atomic<int> g_scanCls{0};
static std::atomic<int> g_scanHits{0};
static std::atomic<bool> g_scanStarted{false};
static void* g_dumpedK[64];
static int   g_dumpedN = 0;

static bool AlreadyDumped(void* k) {
    for (int i = 0; i < g_dumpedN; i++)
        if (g_dumpedK[i] == k) return true;
    if (g_dumpedN < 64) g_dumpedK[g_dumpedN++] = k;
    return false;
}

static void ScanOneImage(FILE* fp, void* img, int imgIdx, int imgTotal, time_t t0) {
    auto& api = GetApi();
    if (!img || !api.image_class_count || !api.image_class || !api.class_name) return;
    const uint32_t cc = api.image_class_count(img);
    for (uint32_t i = 0; i < cc; i++) {
        const int scanned = g_scanCls.load();
        if (scanned >= g_cfgScanMaxClasses) {
            LOGW(TPL_LOG " scan ABORT budget cls=%d (img#%d)", scanned, imgIdx);
            WTBL(fp, "scan ABORT budget cls=%d\n", scanned);
            return;
        }
        if ((i & 0x3FF) == 0 && (time(nullptr) - t0) > g_cfgScanMaxSec) {
            LOGW(TPL_LOG " scan ABORT budget time=%lds (img#%d/%d)", (long)(time(nullptr) - t0),
                 imgIdx, imgTotal);
            WTBL(fp, "scan ABORT budget time=%lds\n", (long)(time(nullptr) - t0));
            return;
        }
        void* k = api.image_class(img, i);
        if (!k) continue;
        g_scanCls.fetch_add(1);
        const char* cn = api.class_name(k);
        if (!cn) continue;

        char full[224];
        const char* ns = NsOf(k);
        if (ns && ns[0] && strcmp(ns, "?") != 0)
            snprintf(full, sizeof(full), "%s.%s", ns, cn);
        else
            snprintf(full, sizeof(full), "%s", cn);

        // 判据 A
        const char* hitM = nullptr;
        int hitArgc = -1;
        const void* hitFn = nullptr;
        if (g_sym.class_methods) {
            void* iter = nullptr;
            int guard = 0;
            while (void* m = g_sym.class_methods(k, &iter)) {
                if (++guard > 4096) break;
                const char* mn = api.method_get_name ? api.method_get_name(m) : nullptr;
                if (mn && NameHasCI(mn, "xcute")) {
                    hitM = mn;
                    hitArgc = g_sym.param_count ? (int)g_sym.param_count(m) : -1;
                    hitFn = *reinterpret_cast<void**>(m);
                    break;
                }
            }
        }
        const bool aHit = hitM != nullptr;
        // 判据 B / D 的字段探针（hData 命中才继续探 com_hp）
        void* fHData = FindFieldByName(k, "hData");
        void* fComHp = fHData ? FindFieldByName(k, "com_hp") : nullptr;
        const bool bHit = fHData && fComHp;
        const bool cHit = NameHasCI(cn, "hpchange") || NameHasCI(cn, "changehp");
        const bool dHit = NameHasCI(cn, "battle") && (fHData != nullptr || aHit);

        if (aHit) {
            LOGI(TPL_LOG " HIT-A %s :: %s argc=%d fn=%p", full, hitM, hitArgc, hitFn);
            WTBL(fp, "HIT-A %s :: %s argc=%d fn=%p\n", full, hitM, hitArgc, hitFn);
            g_scanHits.fetch_add(1);
        }
        if (bHit) {
            LOGI(TPL_LOG " HIT-B %s", full);
            WTBL(fp, "HIT-B %s\n", full);
            g_scanHits.fetch_add(1);
        }
        if (cHit) {
            LOGI(TPL_LOG " HIT-C %s", full);
            WTBL(fp, "HIT-C %s\n", full);
            g_scanHits.fetch_add(1);
        }
        if (dHit) {
            LOGI(TPL_LOG " HIT-D %s", full);
            WTBL(fp, "HIT-D %s\n", full);
            g_scanHits.fetch_add(1);
        }
        if ((aHit || bHit || cHit || dHit) && !AlreadyDumped(k)) {
            WTBL(fp, "==== class %s (%p) ====\n", full, k);
            DumpClassFields(fp, k, full, false);   // 只进文件，不刷 logcat
            DumpClassMethods(fp, k, full, false);
        }
    }
}

static void ScanAllImages(FILE* fp) {
    auto& api = GetApi();
    size_t an = 0;
    void** asms = api.Assemblies(&an);
    if (!asms || !an) {
        LOGW(TPL_LOG " scan: no assemblies");
        return;
    }
    if (an > 64) an = 64;
    const time_t t0 = time(nullptr);
    int imgN = 0;
    for (size_t a = 0; a < an; a++) {
        void* img = WorImageOf(asms[a], nullptr);
        if (!img) continue;
        imgN++;
        const char* in = g_sym.image_name ? g_sym.image_name(img) : nullptr;
        WTBL(fp, "==== scan img#%d %s ====\n", imgN, in ? in : "(?)");
        ScanOneImage(fp, img, imgN, (int)an, t0);
        LOGI(TPL_LOG " scan img#%d/%d cls=%d hits=%d", imgN, (int)an, g_scanCls.load(),
             g_scanHits.load());
        WTBL(fp, "scan img#%d/%d cls=%d hits=%d\n", imgN, (int)an, g_scanCls.load(),
             g_scanHits.load());
        if (g_scanCls.load() >= g_cfgScanMaxClasses) break;
        if (time(nullptr) - t0 > g_cfgScanMaxSec) break;
    }
}
#endif  // WOR_DIAG_FULL（S2 名探针 / S1 镜像诊断 / S3 结构扫描）

// ★缺陷2修复：非 il2cpp 线程 invoke 前必须 attach（Il2CppApi.h:47 语义）。
//   本模块的解析路径跑在框架工作线程（Android/Java 线程），不是 il2cpp 线程；
//   不 attach 直接调 class_from_name / class_get_fields / class_get_methods 有失败甚至崩的风险
//   （class_get_methods 会触发方法数组惰性 Setup，class_from_name 会触发类型层级初始化）。
//   thread_attach 幂等（同线程重复调用返回同一个 Il2CppThread*）⇒ 每次 retry 都调一次是安全的。
static void EnsureThreadAttached() {
    auto& api = GetApi();
    if (!api.thread_attach || !api.domain_get) {
        LOGW(TPL_LOG " thread_attach 不可用 —— 解析可能在未 attach 线程上跑");
        return;
    }
    void* d = api.domain_get();
    if (!d) { LOGW(TPL_LOG " thread_attach: domain null"); return; }
    void* t = api.thread_attach(d);
    LOGI(TPL_LOG " thread attached t=%p domain=%p", t, d);
}

#if WOR_DIAG_FULL
static void* WorScanThread(void*) {
    Il2Cpp::SetScanThread(pthread_self());   // SEGV handler 按线程匹配，必须先登记
    EnsureThreadAttached();                  // 复用同一助手（幂等），不再自带一份
    FILE* fp = WorTblOpen("a");              // 自己开一份（append），不跨线程共用 FILE*
    if (fp) WTBL(fp, "\n===== SECTION 3: structural-scan =====\n");
    Il2Cpp::SafeScan([&]() { ScanAllImages(fp); });
    if (fp) fclose(fp);
    LOGI(TPL_LOG " scan thread done: cls=%d hits=%d", g_scanCls.load(), g_scanHits.load());
    return nullptr;
}

static void StartScanThread() {
    if (g_scanStarted.exchange(true)) return;
    pthread_t t;
    if (pthread_create(&t, nullptr, WorScanThread, nullptr) == 0) {
        pthread_detach(t);
        LOGI(TPL_LOG " scan thread started (budget cls=%d sec=%d)", g_cfgScanMaxClasses,
             g_cfgScanMaxSec);
    } else {
        g_scanStarted.store(false);
        LOGW(TPL_LOG " scan thread create failed — 跳过 S3（不阻塞主线程）");
    }
}
#endif  // WOR_DIAG_FULL（S3 扫描线程）

// ============================================================
// B3：运行时布局解析（零静态 RVA）
//   S1 images → S2 name-probe → 类型链取 3 个依赖类 → S5 字段表 → S6 对账
//   → S4 候选（ExcuteOnce / HasComponent）→ 启动 S3 后台全域扫描
//   全程只读，绝不写游戏内存；任何一步失败只记日志并继续（不 return false）
// ============================================================
static void* g_sysKlass = nullptr;         // CSharpBattle.Battle.HpChangeEntitySystem
static void* g_dataKlass = nullptr;        // stHpChangeData（hData 类型链优先）
static void* g_comKlass = nullptr;         // HPChangeComponent（com_hp 类型链优先）
static void* g_entKlass = nullptr;         // Entity（**只许**从 mAtkEntity 类型链取）
// g_miExcuteOnce 定义在 ⑤ 段（HookByName 之前），此处不再重复定义
static bool  g_offHdataRuntime = false;    // hData 字段是否真被运行时解析到（装机闸门）
static std::atomic<bool> g_layoutDone{false};
// ★缺陷1修复：g_layoutDone 只代表「S1~S6 诊断那一次已消耗」，**不代表解析成功**。
//   il2cpp 域晚就绪时（liblogic.so 被游戏 DynamicSOLoader 主动 dlopen，早于 il2cpp_init 完成）
//   domain_get() 为 NULL ⇒ 首次解析必然 MISS ⇒ 装机闸门拒绝 ⇒ 0 hook。故：
//   - g_layoutOk：解析真正成功（判据 = g_offHdataRuntime，与装机闸门同一判据）
//   - g_layoutTries：重试次数上限（有界，不无限刷日志）
static std::atomic<bool> g_layoutOk{false};
static std::atomic<int>  g_layoutTries{0};

static void* FindSysClass(const char*& how) {
    auto& api = GetApi();
    size_t an = 0;
    void** asms = api.Assemblies(&an);
    for (size_t i = 0; asms && i < an; i++) {
        void* img = WorImageOf(asms[i], nullptr);
        if (!img) continue;
        void* k = api.ClassFromName(img, "CSharpBattle.Battle", "HpChangeEntitySystem");
        if (k) { how = "ns"; return k; }
    }
    void* k2 = api.FindClassByShortName("HpChangeEntitySystem");   // 短名完全相等
    if (k2) { how = "short"; return k2; }
    how = "MISS";
    return nullptr;
}

#if WOR_DIAG_FULL
// S5：四个目标类的字段表（logcat 40 行上限，文件完整）
static void DumpTargetFields() {
    auto& api = GetApi();
    if (!g_sym.class_fields) { LOGW(TPL_LOG " field dump skipped: no class_get_fields"); return; }
    DumpClassFields(g_tblFile, g_sysKlass, "HpChangeEntitySystem", true);
    DumpClassFields(g_tblFile, g_dataKlass, "stHpChangeData", true);
    DumpClassFields(g_tblFile, g_comKlass, "HPChangeComponent", true);
    DumpClassFields(g_tblFile, g_entKlass, "Entity", true);
    if (g_comKlass) {   // lValues 的 List<T> 类型 → _items/_size
        void* lk = FieldTypeKlass(g_comKlass, "lValues");
        if (lk) {
            const char* ln = api.class_name(lk);
            LOGI(TPL_LOG " list type = %s", ln ? ln : "?");
            DumpClassFields(g_tblFile, lk, ln ? ln : "List", true);
            void* fi = FindFieldByName(lk, "_items");
            void* fs = FindFieldByName(lk, "_size");
            ReconcileLog("LIST_ITEMS", LIST_OFF_ITEMS,
                         fi ? (int32_t)api.field_offset(fi) : (int32_t)WOR_OFF_MISS, "name");
            ReconcileLog("LIST_SIZE", LIST_OFF_SIZE,
                         fs ? (int32_t)api.field_offset(fs) : (int32_t)WOR_OFF_MISS, "name");
            void* ak = FieldTypeKlass(lk, "_items");
            if (ak) {
                const char* an2 = api.class_name(ak);
                LOGI(TPL_LOG " array type = %s", an2 ? an2 : "?");
                DumpClassFields(g_tblFile, ak, an2 ? an2 : "Array", true);
                void* fm = FindFieldByName(ak, "m_Length");
                // ARRAY_OFF_DATA(0x20) 是首个元素偏移，IL2CPP 无字段名可查 ⇒ 只核对 m_Length
                ReconcileLog("ARRAY_LEN", 0x18,
                             fm ? (int32_t)api.field_offset(fm) : (int32_t)WOR_OFF_MISS, "name");
            }
        } else {
            LOGW(TPL_LOG " off LIST_ITEMS/LIST_SIZE: lValues 类型链拿不到，保留国服宏");
        }
    }
}
#endif  // WOR_DIAG_FULL（S5 目标类字段表 dump）

// ★ C1：把 wor_cfg.txt 里显式给出的偏移作为「最终真值」写回（优先级最高）。
//   为什么放在这里而不是解析时：ApplyRuntimeOffsets() 内部会**强制恢复国服真值**
//   （HybridCLR 的 il2cpp_field_get_offset 对值类型多报 0x10），所以 cfg 必须最后落。
static void WorCfgApplyOffsets() {
    // ★ 基线只取一次，且必须在套用任何覆盖之前取：此时 g_off_* 还是「运行时对账/国服真值」。
    //   热重载靠它还原干净，否则删键不生效（见 g_offBase 的注释）。
    if (!g_cfgOffBaseTaken) {
        for (int i = 0; i < WOR_CFG_OFF_N; i++) g_offBase[i] = *kWorCfgOffs[i].dst;
        g_cfgOffBaseTaken = true;
    }
    int n = 0;
    for (int i = 0; i < WOR_CFG_OFF_N; i++) {
        if (!g_cfgOffSet[i]) continue;
        LOGI(TPL_LOG " cfg-off %-18s = 0x%X（wor_cfg.txt 覆盖）", kWorCfgOffs[i].what,
             (unsigned)*kWorCfgOffs[i].dst);
        n++;
    }
    if (n > 0)
        LOGI(TPL_LOG " cfg-off: %d 项由 wor_cfg.txt 覆盖（优先于内建/国服真值）", n);
    // hData 是装机闸门的依据 —— 被 cfg 覆盖时必须同步闸门变量，否则 hook 会被拒绝装机
    if (g_cfgOffSet[WOR_CFG_OFFIDX_HDATA] && g_off_SYS_HDATA != 0) g_offHdataRuntime = true;
}

// S6 + 覆盖：把运行时真值写进 g_off_*（解析到就覆盖，否则**保持国服兜底值**）
// 注意：OffOfField/OffOfFieldChain/OffOfNested 的返回语义 = 成功返回运行时偏移，
//       失败返回传入的 cn（国服兜底值）并自己打对账行 ⇒ 这里直接赋值即可。
static void ApplyRuntimeOffsets() {
    if (g_sysKlass) {
        g_off_SYS_COM_HP = OffOfField(g_sysKlass, "com_hp", g_off_SYS_COM_HP, "SYS_COM_HP");
        const bool hadHData = HasField(g_sysKlass, "hData");
        g_off_SYS_HDATA = OffOfField(g_sysKlass, "hData", g_off_SYS_HDATA, "SYS_HDATA");
        if (hadHData) g_offHdataRuntime = true;
        g_off_SYS_MATK = OffOfField(g_sysKlass, "mAtkEntity", g_off_SYS_MATK, "SYS_MATK");
        g_off_SYS_FDAMAGE = OffOfField(g_sysKlass, "fDamage", g_off_SYS_FDAMAGE, "SYS_FDAMAGE");
    }
    if (g_comKlass) {
        g_off_HPCOMP_LVALUES = OffOfField(g_comKlass, "lValues", g_off_HPCOMP_LVALUES,
                                          "HPCOMP_LVALUES");
    }
    if (g_dataKlass) {
        // ★★★ round-7 真机血证：`stHpChangeData` 是**值类型**，本机 HybridCLR 的
        //   `il2cpp_field_get_offset` 对它（以及它嵌套的 `eRelationData`）的字段**统一返回 +0x10 偏置**。
        //   逐字段对账（rt / 真值）：FVALUE 0x10|0x00、SRC 0x18|0x08、SETTLE 0x1C|0x0C、SHOW 0x20|0x10、
        //   SKILL 0x38|0x28、FORMULA 0x3C|0x2C、ATK 外层 0x2C|0x1C（内外层各偏 0x10）。
        //   内存实证（negLanes 扫 hd 的 0x00–0x80、以及 lValues._items[0] 的 0x00–0x180）：
        //     hd+0x00 = -1056768/-647168/-69632/-839680（÷4096 = -258/-158/-17/-205）
        //                ← 随事件变化的负 Fix64，**这才是真 fValue**
        //     hd+0x10 = 0x{1,2}_0000_0001 → 低 32 位 =1（ETextShowType）、高 32 位 =1/2（eShowText）
        //   ⇒ 真布局 = 国服那一套（fValue@0x00、SHOW@0x10）；**国服兜底值就是真值**，
        //     运行时解析值一律要减 0x10。故本组「只记对账日志，随后强制恢复国服真值」，
        //     绝不把 +0x10 的错值留在 g_off_* 里（上一版就是因为写进了 0x10，
        //     导致 `raw` 恒为 0x2_0000_0001 正数、写闸门 `raw<0` 永不通过）。
        g_off_HPD_FVALUE = OffOfField(g_dataKlass, "fValue", g_off_HPD_FVALUE, "HPD_FVALUE");
        // 三项「字段名随版本变」：三段兜底（主名 → 备选名 → 类型名）
        g_off_HPD_SRC = OffOfFieldChain(g_dataKlass, "eDamageSourceType", "eSourceType",
                                        "EDamageSourceType", g_off_HPD_SRC, "HPD_SRC");
        g_off_HPD_SETTLE = OffOfFieldChain(g_dataKlass, "eDamageSettleType", nullptr,
                                           "EDamageSettleType", g_off_HPD_SETTLE, "HPD_SETTLE");
        g_off_HPD_SHOW = OffOfFieldChain(g_dataKlass, "eTextShowType", "textShowType",
                                         "ETextShowType", g_off_HPD_SHOW, "HPD_SHOW");
        g_off_HPD_SKILL = OffOfField(g_dataKlass, "iAttackSkillID", g_off_HPD_SKILL, "HPD_SKILL");
        g_off_HPD_FORMULA = OffOfField(g_dataKlass, "iFormularID", g_off_HPD_FORMULA,
                                       "HPD_FORMULA");
        // ▼ 强制恢复国服真值（= 真运行时布局；对账日志已在上方打过 rt）
        g_off_HPD_FVALUE  = 0x00;
        g_off_HPD_SRC     = 0x08;
        g_off_HPD_SETTLE  = 0x0C;
        g_off_HPD_SHOW    = 0x10;
        g_off_HPD_SKILL   = 0x28;
        g_off_HPD_FORMULA = 0x2C;
        LOGI(TPL_LOG " HPD 值类型 +0x10 偏置已纠正 -> FVALUE=0x%X SRC=0x%X SETTLE=0x%X "
                     "SHOW=0x%X SKILL=0x%X FORMULA=0x%X",
             g_off_HPD_FVALUE, g_off_HPD_SRC, g_off_HPD_SETTLE, g_off_HPD_SHOW,
             g_off_HPD_SKILL, g_off_HPD_FORMULA);
        // AtkRelationData（类型 eRelationData）的 .eUid / .cUid / .iUnitID
        // ★ round-5 真机血证：EN 的外层字段名是 **`AtkRelationData`** 而不是 `eRelationData`
        //   （`field stHpChangeData.AtkRelationData = 0x2C : eRelationData`）⇒ 旧代码报
        //   `(外层字段未找到)`，三个 ATK 偏移回落到 CN 的 0x1C/0x20/0x24；而 EN 的
        //   0x1C/0x20/0x24 恰好是 eDamageSettleType/eTextShowType/eShowText ⇒ P# 里
        //   `atk=9/1 u2015` 恒等于 `set=9 show=1` —— 全是假数据。
        //   正确值应由外层字段偏移 + 内层固定布局 (eUid@+0 / cUid@+4 / iUnitID@+8) 得出。
        const char* kRelOuter[3] = { "AtkRelationData", "eRelationData", "stAtkRelationData" };
        void* relOf = nullptr;
        for (int i = 0; i < 3 && !relOf; i++) relOf = FindFieldByName(g_dataKlass, kRelOuter[i]);
        void* relK = nullptr;
        for (int i = 0; i < 3 && !relK; i++) relK = FieldTypeKlass(g_dataKlass, kRelOuter[i]);
        const int32_t relBase = relOf ? (int32_t)GetApi().field_offset(relOf) : -1;
        LOGI(TPL_LOG " AtkRelationData outer=%p base=0x%X inner klass=%p", relOf, relBase, relK);
        {
            struct { const char* n[3]; int32_t d; int32_t* dst; const char* tag; } nt[3] = {
                { { "eUid", "uId", "uid" },         0, &g_off_HPD_ATK_EUID, "HPD_ATK_EUID" },
                { { "cUid", "uCid", "cid" },        4, &g_off_HPD_ATK_CUID, "HPD_ATK_CUID" },
                { { "iUnitID", "iUnitId", "unitID" }, 8, &g_off_HPD_ATK_UNIT, "HPD_ATK_UNIT" },
            };
            for (int t = 0; t < 3; t++) {
                void* inf = nullptr;
                if (relK)
                    for (int i = 0; i < 3 && !inf; i++) inf = FindFieldByName(relK, nt[t].n[i]);
                if (relBase >= 0 && inf) {
                    const int32_t rt = relBase + (int32_t)GetApi().field_offset(inf);
                    ReconcileLog(nt[t].tag, *nt[t].dst, rt, "nested");
                    *nt[t].dst = rt;
                } else if (relBase >= 0) {
                    const int32_t rt = relBase + nt[t].d;
                    ReconcileLog(nt[t].tag, *nt[t].dst, rt, "(内层类缺失→外层+固定)");
                    *nt[t].dst = rt;
                } else {
                    ReconcileLog(nt[t].tag, *nt[t].dst, (int32_t)WOR_OFF_MISS, "(外层字段未找到)");
                }
            }
        }
        // ▼ 同上的 +0x10 偏置纠正：`eRelationData` 内层也是值类型，外层/内层各偏 0x10。
        //   真值 = 国服表：外层 AtkRelationData@0x1C（eUid@+0、cUid@+4、iUnitID@+8）。
        g_off_HPD_ATK_EUID = 0x1C;
        g_off_HPD_ATK_CUID = 0x20;
        g_off_HPD_ATK_UNIT = 0x24;
        LOGI(TPL_LOG " ATK 值类型 +0x10 偏置已纠正 -> EUID=0x%X CUID=0x%X UNIT=0x%X",
             g_off_HPD_ATK_EUID, g_off_HPD_ATK_CUID, g_off_HPD_ATK_UNIT);
        (void)OffOfNested;
    }
    if (g_entKlass) {
        const bool hadBacking = HasField(g_entKlass, "<m_uID>k__BackingField");
        int32_t o = OffOfField(g_entKlass, "<m_uID>k__BackingField", g_off_ENTITY_MUID,
                               "ENTITY_MUID");
        if (!hadBacking)
            o = OffOfField(g_entKlass, "m_uID", o, "ENTITY_MUID_ALT");
        g_off_ENTITY_MUID = o;
    }
    WorCfgApplyOffsets();   // ★ C1：cfg 覆盖最后落，优先级高于上面的国服真值恢复
}

// S4 + 钩点候选：ExcuteOnce 必须 argc==1；HasComponent 必须 argc==1 且 p0 含 EComponentType
static void ResolveHookCandidates() {
    CandInfo c[WOR_CAND_MAX];
    if (g_sysKlass) {
        int n = CollectCandidates(g_sysKlass, "ExcuteOnce", c, WOR_CAND_MAX);
        int picked = 0;
        for (int i = 0; i < n; i++) {
            const bool exec = c[i].fn && Il2Cpp::IsExecutableRange((uintptr_t)c[i].fn, 16);
            LOGI(TPL_LOG " cand ExcuteOnce argc=%u mi=%p fn=%p exec=%d ret=%s p0=%s", c[i].argc,
                 c[i].mi, c[i].fn, exec ? 1 : 0, c[i].ret, c[i].p0);
            WTBL(g_tblFile, "cand ExcuteOnce argc=%u mi=%p fn=%p exec=%d ret=%s p0=%s\n", c[i].argc,
                 c[i].mi, c[i].fn, exec ? 1 : 0, c[i].ret, c[i].p0);
            if (c[i].argc == 1 && exec && !picked) {
                g_miExcuteOnce = c[i].mi;
                picked = 1;
            }
        }
        if (!picked) {
            LOGE(TPL_LOG " FATAL no ExcuteOnce(1) candidate n=%d —— 拒绝猜测重载，HookByName 自然失败",
                 n);
            WTBL(g_tblFile, "FATAL no ExcuteOnce(1) candidate n=%d\n", n);
            g_miExcuteOnce = nullptr;
        } else {
            LOGI(TPL_LOG " ExcuteOnce(1) picked mi=%p", g_miExcuteOnce);
        }
    }
    if (!g_hasComponent && g_entKlass) {
        int n = CollectCandidates(g_entKlass, "HasComponent", c, WOR_CAND_MAX);
        for (int i = 0; i < n; i++) {
            const bool exec = c[i].fn && Il2Cpp::IsExecutableRange((uintptr_t)c[i].fn, 16);
            LOGI(TPL_LOG " cand HasComponent argc=%u mi=%p fn=%p p0=%s exec=%d", c[i].argc, c[i].mi,
                 c[i].fn, c[i].p0, exec ? 1 : 0);
            WTBL(g_tblFile, "cand HasComponent argc=%u mi=%p fn=%p p0=%s exec=%d\n", c[i].argc,
                 c[i].mi, c[i].fn, c[i].p0, exec ? 1 : 0);
            if (c[i].argc == 1 && exec && NameHasCI(c[i].p0, "EComponentType")) {
                g_hasComponent = (HasComponentFn)c[i].fn;
            }
        }
        if (g_hasComponent) {
            LOGI(TPL_LOG " hasComponent bound fn=%p", (void*)g_hasComponent);
        } else {
            LOGE(TPL_LOG " hasComponent 未绑定（无 argc=1 且 p0 含 EComponentType 的候选）—— 功能降级不崩");
        }
    }
}

static void ResolveRuntimeLayout() {
    auto& api = GetApi();
    if (!api.ready || !g_sym.core) {
        LOGW(TPL_LOG " layout skip: api.ready=%d core=%d", api.ready ? 1 : 0, g_sym.core ? 1 : 0);
        return;
    }
    LOGI(TPL_LOG " ===== runtime layout begin =====");
    WTBL(g_tblFile, "\n===== runtime layout begin =====\n");
#if WOR_DIAG_FULL
    DumpBattleDiagnostics("resolve");

    // ---- S2 name-probe：3 个类名的全部合理写法都试（如实报告哪个 HIT）----
    WTBL(g_tblFile, "===== SECTION 2: name-probe =====\n");
    ProbeName("CSharpBattle.Battle", "HpChangeEntitySystem");
    ProbeName("CSharpBattle.Battle", "HPChangeEntitySystem");
    ProbeName("", "HpChangeEntitySystem");
    ProbeName("CSharpBattle.Battle", "HPChangeComponent");
    ProbeName("CSharpBattle.Battle", "HpChangeComponent");
    ProbeName("", "HPChangeComponent");
    ProbeName("CSharpBattle.Battle", "stHpChangeData");
    ProbeName("CSharpBattle.Battle", "StHpChangeData");
    ProbeName("CSharpBattle.Battle", "HpChangeData");
    ProbeName("", "stHpChangeData");
    ProbeShortAll("Entity", 20);
#endif  // WOR_DIAG_FULL（S1 镜像诊断 + S2 名探针）

    // ---- 定位系统类（唯一按名依赖）----
    const char* how = "MISS";
    g_sysKlass = FindSysClass(how);
    LOGI(TPL_LOG " sys class HpChangeEntitySystem = %p (%s) ns=%s", g_sysKlass, how,
         g_sysKlass ? NsOf(g_sysKlass) : "-");
    WTBL(g_tblFile, "sys class = %p (%s) ns=%s\n", g_sysKlass, how,
         g_sysKlass ? NsOf(g_sysKlass) : "-");

    // ---- 类型链：只用 HpChangeEntitySystem 一个名字，其余三个类全靠字段类型 ----
    if (g_sysKlass) {
        void* fCom = FindFieldByName(g_sysKlass, "com_hp");
        void* fHd = FindFieldByName(g_sysKlass, "hData");
        void* fAtk = FindFieldByName(g_sysKlass, "mAtkEntity");
        g_comKlass = FieldTypeKlass(g_sysKlass, "com_hp");
        g_dataKlass = FieldTypeKlass(g_sysKlass, "hData");
        g_entKlass = FieldTypeKlass(g_sysKlass, "mAtkEntity");   // Entity 只许走这条链
        // 类型链失败才退回按名（Entity 例外：禁止按名，短名太常见必撞）
        if (!g_comKlass) g_comKlass = api.FindClassByShortName("HPChangeComponent");
        if (!g_dataKlass) g_dataKlass = api.FindClassByShortName("stHpChangeData");
        LOGI(TPL_LOG " chain com_hp=%p->%p hData=%p->%p mAtkEntity=%p->%p", fCom, g_comKlass,
             fHd, g_dataKlass, fAtk, g_entKlass);
        WTBL(g_tblFile, "chain com_hp=%p->%p hData=%p->%p mAtkEntity=%p->%p\n", fCom, g_comKlass,
             fHd, g_dataKlass, fAtk, g_entKlass);
        if (!g_dataKlass) LOGE(TPL_LOG " hData 类型链失败且短名未命中 —— 偏移不可信，功能拒绝装机");
        if (!g_entKlass) LOGE(TPL_LOG " mAtkEntity 类型链失败 —— Entity 禁止按名找，euid 读不到");
    }

#if WOR_DIAG_FULL
    DumpTargetFields();
#endif
    ApplyRuntimeOffsets();
    ResolveHookCandidates();

#if WOR_DIAG_FULL
    // S3 全域结构扫描（后台线程，主线程立即返回）
    StartScanThread();
#endif

    LOGI(TPL_LOG " ===== runtime layout end sys=%p data=%p com=%p ent=%p hdataRT=%d =====", g_sysKlass,
         g_dataKlass, g_comKlass, g_entKlass, g_offHdataRuntime ? 1 : 0);
    WTBL(g_tblFile, "===== runtime layout end =====\n");
}

// ★缺陷1修复：轻量重试 —— 只补解析（类 + 类型链 + 偏移 + 钩点候选），
//   **不重打 S1/S2/S3 诊断**（那一次机会由 g_layoutDone 守，保持「诊断只跑一次」的语义）。
static void RetryResolveOffsets() {
    const char* how = "MISS";
    if (!g_sysKlass) g_sysKlass = FindSysClass(how);
    if (!g_sysKlass) { LOGW(TPL_LOG " retry: sys class 仍未找到"); return; }
    if (!g_comKlass)  g_comKlass  = FieldTypeKlass(g_sysKlass, "com_hp");
    if (!g_dataKlass) g_dataKlass = FieldTypeKlass(g_sysKlass, "hData");
    if (!g_entKlass)  g_entKlass  = FieldTypeKlass(g_sysKlass, "mAtkEntity");
    if (!g_comKlass)  g_comKlass  = GetApi().FindClassByShortName("HPChangeComponent");
    if (!g_dataKlass) g_dataKlass = GetApi().FindClassByShortName("stHpChangeData");
    ApplyRuntimeOffsets();
    ResolveHookCandidates();
}

// 接线点：Wor_TryInit 里 lib 就绪后调用；Wor_Tick / Wor_OnFeatureChanged 里做兜底重试
//   ★关键：il2cpp 域未就绪时**不许消耗**那一次诊断机会（g_layoutDone 不置位），下次再试。
//
// ★★ 2026-10-03 二次审查修正（本段是我自己上一版设计的缺陷，务必不要再改回「次数上限」）：
//   1) 原实现是 `const int n = g_layoutTries.fetch_add(1); if (n >= 20) return;` —— **按次数限流**。
//      但 EnsureLayout 由 Wor_Tick 驱动，而 Tick 是每帧/每百毫秒级的：20 次重试可能在 **< 1 秒内**
//      就烧光，而 il2cpp_init 加载 137MB/60MB metadata 需要**数秒**。⇒ 重试窗口在实际需要的
//      时刻早已关闭，等于没重试。**必须按时间节流**（下见 WOR_LAYOUT_RETRY_MS）。
//   2) 程序集尚未加载完时（asmN 很小）跑 S1~S6 会得到空表：S1 的 image 列表为空、
//      FindSysClass 遍历 0 个程序集必然 MISS ⇒ wor_tables.txt 毫无价值、唯一的诊断机会被浪费。
//      ⇒ 加程序集就绪闸门（默认 6 个程序集 + 至少 1500ms；见 C1 的
//        layout_min_asm / layout_min_ms 两个 cfg 键）；
//      并在「诊断跑过但仍未解析成功」时**允许再补一次**（上限 2 次，见 g_diagRuns）。
static const int64_t WOR_LAYOUT_RETRY_MS        = 500;    // 轻量重试最小间隔
static const int64_t WOR_LAYOUT_GIVEUP_MS       = 60000;  // 距首次接触超此值则停止重试
// ★ C1：上面两个闸门阈值已改为运行时变量 g_cfgMinElapsedMs / g_cfgMinAsm
//   （内建默认 1500ms / 6，可用 wor_cfg.txt 的 layout_min_ms / layout_min_asm 覆盖）。
static const int     WOR_LAYOUT_MAX_DIAG        = 2;      // 重量级诊断最多跑几次
static const int64_t WOR_LAYOUT_DIAG_REARM_MS   = 10000;  // 首次诊断后仍未成功，满此时间允许补一次

static int64_t WorMonoMs() {
    return (int64_t)std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::steady_clock::now().time_since_epoch()).count();
}

// ============================================================
// ★ C1 热重载（2026-10-04 加）
// ------------------------------------------------------------
// 问题：没有它，改一行 wor_cfg.txt 仍要 force-stop + 重启游戏 —— C1 只省掉了「重编+重装」，
//   没省掉「重启」；而调偏移/阈值恰恰是要反复试的活。
// 方案：每 g_cfgPollMs 毫秒 stat 一次配置文件，mtime 或 size 变了就整体重读。
//   · 判据用 mtime+size 而非内容哈希：sdcardfs/FUSE 上 stat 只是一次元数据读，而每次轮询
//     读全文+哈希是要付 I/O 的。残留漏判风险：同一秒内改成**同长度**内容才会漏（本场景改的
//     都是数值文本，长度几乎必变）。要绝对确定就把 cfg_poll_ms 调大些、或改完 touch 一下。
//   · 为什么先还原基线再解析：见 g_offBase 注释 —— 「缺键 = 内建默认」要求删键也生效。
//   · 线程安全：只在 Wor_Tick（Unity 主线程）调用；hook 侧只读 g_off_*（int32_t，ARM64 上
//     对齐 32 位读写不撕裂）。最坏是某帧读到半新半旧的偏移，此时写闸门 rawSane 大概率拒绝
//     ⇒ 表现为「那一帧没效果」，不会崩。
//   · 本函数只做 fopen/stat/字符串解析，绝不在这里做任何 il2cpp 调用 / 扫描 / 遍历对象。
// ============================================================
static void WorCfgReloadCheck() {
    if (g_cfgPollMs <= 0) return;                       // 0 = 关闭热重载
    const int64_t now = WorMonoMs();
    if (g_cfgLastPollMs != 0 && (now - g_cfgLastPollMs) < (int64_t)g_cfgPollMs) return;
    g_cfgLastPollMs = now;

    // 1) 现在有没有文件、指纹多少（首载失败也照样轮询 ⇒ push 上去即生效，连第一次都不用重启）
    const char* found = nullptr;
    int64_t mt = -1, sz = -1;
    for (int i = 0; i < 2; i++) {
        if (WorCfgStamp(kWorCfgCands[i], &mt, &sz)) { found = kWorCfgCands[i]; break; }
    }
    if (!found && !g_cfgOk) return;                     // 一直没有、现在也还是没有 → 无事
    if (found && g_cfgOk && strcmp(found, g_cfgPath) == 0 &&
        mt == g_cfgMtime && sz == g_cfgSize) return;     // 有、且指纹没变 → 无事

    // 2) 变更前快照（只为打「什么变了」的对账日志）
    const bool    oldDiag     = g_cfgDiag;
    const int     oldProbe    = g_cfgProbeMax;
    const int64_t oldRawSane  = g_rawSaneMax;
    const size_t  oldMinAsm   = g_cfgMinAsm;
    const int64_t oldMinMs    = g_cfgMinElapsedMs;
    const int     oldScanCls  = g_cfgScanMaxClasses;
    const int     oldScanSec  = g_cfgScanMaxSec;
    char          oldTbl[256];
    snprintf(oldTbl, sizeof(oldTbl), "%s", g_cfgTblPath);
    int32_t       before[WOR_CFG_OFF_N];
    for (int i = 0; i < WOR_CFG_OFF_N; i++) before[i] = *kWorCfgOffs[i].dst;

    // 3) 全部回内建默认值（这一步是「缺键 = 默认」的实现；不做它，配置就变成只增不减）
    //    g_cfgPollMs 也一起回默认 —— 语义一致：文件里没写就用内建 2000ms。
    const bool applyOff = g_cfgOffBaseTaken;            // 布局已解析过才需要动偏移
    g_cfgOk            = false;
    g_cfgDiag          = (WOR_DIAG_FULL != 0);
    g_cfgProbeMax      = PROBE_MAX;
    g_rawSaneMax       = RAW_SANE_MAX_DEFAULT;
    g_cfgMinAsm        = 6;
    g_cfgMinElapsedMs  = 1500;
    g_cfgScanMaxClasses = 40000;
    g_cfgScanMaxSec    = 60;
    g_cfgPollMs        = WOR_CFG_POLL_MS_DEFAULT;
    g_cfgTblPath[0]    = 0;
    g_cfgPath[0]       = 0;
    if (applyOff) {
        // 偏移先还原到基线（= 运行时对账/国服真值），再让下面的解析套新覆盖
        for (int i = 0; i < WOR_CFG_OFF_N; i++) *kWorCfgOffs[i].dst = g_offBase[i];
    }
    for (int i = 0; i < WOR_CFG_OFF_N; i++) g_cfgOffSet[i] = false;

    // 4) 重新解析
    if (found) {
        if (WorCfgLoadFile(found)) {
            g_cfgOk = true;
            snprintf(g_cfgPath, sizeof(g_cfgPath), "%s", found);
        } else {
            LOGW(TPL_LOG " cfg-reload: %s 打开失败（保持内建默认值）", found);
        }
        g_cfgMtime = mt;
        g_cfgSize  = sz;
    } else {
        g_cfgMtime = -1;
        g_cfgSize  = -1;
    }
    // g_offHdataRuntime 是「只置真」的闩（ApplyRuntimeOffsets 解析到 hData 也会置真），这里不动它。
    if (applyOff) WorCfgApplyOffsets();

    // 5) 对账日志：什么变了、哪些立即生效、哪些必须重启
    g_cfgReloadN++;
    LOGI(TPL_LOG " cfg-reload #%d: %s 已重载（mtime=%lld size=%lld）", g_cfgReloadN,
         found ? found : "(文件已删除)", (long long)mt, (long long)sz);
    if (!found)
        LOGW(TPL_LOG " cfg-reload: 配置文件已消失 → 全部回退内建默认值");

    if (g_cfgDiag != oldDiag)
        LOGI(TPL_LOG " cfg-reload: diag %d→%d（立即生效）", (int)oldDiag, (int)g_cfgDiag);
    if (g_cfgProbeMax != oldProbe)
        LOGI(TPL_LOG " cfg-reload: probe_max %d→%d（立即生效；当前已打印 %d 条）",
             oldProbe, g_cfgProbeMax, g_probeCount.load());
    if (g_rawSaneMax != oldRawSane)
        LOGI(TPL_LOG " cfg-reload: raw_sane_max %lld→%lld（立即生效）",
             (long long)oldRawSane, (long long)g_rawSaneMax);

    int nRestart = 0;
    if (g_cfgMinAsm != oldMinAsm) {
        nRestart++;
        LOGW(TPL_LOG " cfg-reload: layout_min_asm %d→%d 属「仅初始化期生效」→ 需重启游戏",
             (int)oldMinAsm, (int)g_cfgMinAsm);
    }
    if (g_cfgMinElapsedMs != oldMinMs) {
        nRestart++;
        LOGW(TPL_LOG " cfg-reload: layout_min_ms %lld→%lld 属「仅初始化期生效」→ 需重启游戏",
             (long long)oldMinMs, (long long)g_cfgMinElapsedMs);
    }
    if (g_cfgScanMaxClasses != oldScanCls) {
        nRestart++;
        LOGW(TPL_LOG " cfg-reload: scan_max_classes %d→%d 属「仅初始化期生效」→ 需重启游戏",
             oldScanCls, g_cfgScanMaxClasses);
    }
    if (g_cfgScanMaxSec != oldScanSec) {
        nRestart++;
        LOGW(TPL_LOG " cfg-reload: scan_max_sec %d→%d 属「仅初始化期生效」→ 需重启游戏",
             oldScanSec, g_cfgScanMaxSec);
    }
    if (strcmp(oldTbl, g_cfgTblPath) != 0) {
        nRestart++;
        LOGW(TPL_LOG " cfg-reload: tbl_path '%s'→'%s' 属「仅初始化期生效」→ 需重启游戏",
             oldTbl, g_cfgTblPath);
    }
    if (nRestart > 0)
        LOGW(TPL_LOG " cfg-reload: 共 %d 项属「仅初始化期生效」，本次运行不再使用"
                     "（改这些键请重启游戏）", nRestart);

    if (applyOff) {
        int nOff = 0;
        for (int i = 0; i < WOR_CFG_OFF_N; i++) {
            const int32_t nv = *kWorCfgOffs[i].dst;
            if (nv == before[i]) continue;
            nOff++;
            LOGW(TPL_LOG " cfg-reload: off.%-18s 0x%X→0x%X（当帧起生效）",
                 kWorCfgOffs[i].what, (unsigned)before[i], (unsigned)nv);
        }
        if (nOff > 0)
            LOGW(TPL_LOG " cfg-reload: %d 项字段偏移已变更 —— 偏移是热路径直接寻址，写错只会"
                         "「没效果」不会越界（有写闸门 + raw_sane_max 兜底），"
                         "但请在有掉血/治疗的场景验证，别在对局中途盲改", nOff);
    }
}

static std::atomic<int64_t> g_firstContactMs{0};
static std::atomic<int64_t> g_lastRetryMs{0};
static std::atomic<int>     g_diagRuns{0};

static void EnsureLayout() {
    if (g_layoutOk.load()) return;

    // ★★ 硬闸门 1：真 IL2CPP 运行时（liblogic.so）必须**先被游戏映射进本进程**。
    //  真机血证（2026-10-03 e2 round2 无限重启，崩溃栈实锤）：
    //    · libstarry_magic.so 0x167e8 = EnsureThreadAttached() 里那句 `blr x8`
    //      （api.thread_attach，Api 结构体偏移 +0xC0；前一帧 0x135ac/0x13700 = EnsureLayout 链）
    //    · 下一帧 liblogic.so 0x3A74A78 = **il2cpp_thread_attach+8** → abort+164
    //    · 时序：进程起来 ~1.0s 就崩（游戏此时连 Resources.dat 都还没解压完）
    //  机制：liblogic.so 尚未映射时，EnsureApi() 会命中 split 里那个**同 soname 的
    //        232KB 转发壳 lib/arm64-v8a/libil2cpp.so**（实测 DT_SONAME 也是 libil2cpp.so）
    //        ⇒ api.ready 提前变 true ⇒ 紧接着 EnsureThreadAttached() → thread_attach
    //        → il2cpp 运行时还没 init → IL2CPP_ASSERT abort。
    //  ⇒ 未映射前**一个 il2cpp API 都不许碰**，只节流打日志并等待。
    if (!WorLibRealPath()) {
        const int64_t t = WorMonoMs();
        if ((t - g_lastRetryMs.load()) >= WOR_LAYOUT_RETRY_MS) {
            g_lastRetryMs.store(t);
            LOGW(TPL_LOG " EnsureLayout: 目标库 liblogic.so 尚未映射，等游戏自解压…");
        }
        return;   // ★绝不置 g_layoutDone
    }

    auto& api = GetApi();
    if (!api.ready) {
        // ★★ 致命修正（真机 e2 首跑血证，勿改回只 LOGW 就 return）★★
        //   旧代码在此只打日志就 return，而 EnsureApi() 的**唯一**调用点在 HookByName 里，
        //   HookByName 又被「布局未解析 ⇒ 拒绝装机」闸门挡着 ⇒ **循环依赖**：
        //   api 永远 load 不了 → 布局永远解析不出来 → hooked 永远 0。
        //   真机证据：22:34:27.5~22:34:42 每 200ms 一条 `EnsureLayout: api not ready, retry later`，
        //   全程 **0 条** `[Il2Cpp] xdl_open ... failed` / `[Il2Cpp] api loaded from`
        //   ⇒ 证明 Api::Load() **从未被调用过**。
        if (!EnsureApi()) {
            const int64_t t = WorMonoMs();
            const int64_t last = g_lastRetryMs.load();
            if ((t - last) >= WOR_LAYOUT_RETRY_MS) {
                g_lastRetryMs.store(t);
                LOGW(TPL_LOG " EnsureLayout: api load 失败，稍后重试（见 [Il2Cpp] xdl_open 行）");
            }
            return;   // ★不置 g_layoutDone，保留重试机会
        }
    }

    const int64_t nowMs = WorMonoMs();
    if (g_firstContactMs.load() == 0) g_firstContactMs.store(nowMs);
    const int64_t elapsed = nowMs - g_firstContactMs.load();

    // ★★ 硬闸门 2：先只问「程序集有几个」，**凑够数才允许 attach / 解析**。
    //   理由同闸门 1：il2cpp_thread_attach 在 il2cpp_init 完成前调用 = abort（真机实锤）。
    //   程序集数量是「运行时已 init」的唯一廉价证据——init 之前必然为 0。
    //   整段裹 SafeProbe（sigsetjmp + SIGSEGV/SIGBUS）：即便此刻 domain 指针还是垃圾值，
    //   也只会被判失败，不会崩掉游戏。
    size_t asmN = 0;
    const bool probed = Il2Cpp::SafeProbe([&]() -> bool {
        void** asms = api.Assemblies(&asmN);
        return asms != nullptr || asmN == 0;
    });
    if (!probed || asmN < g_cfgMinAsm || elapsed < g_cfgMinElapsedMs) {
        const int64_t last = g_lastRetryMs.load();
        if ((nowMs - last) >= WOR_LAYOUT_RETRY_MS) {
            g_lastRetryMs.store(nowMs);
            LOGW(TPL_LOG " EnsureLayout: 未就绪 asm=%zu elapsed=%lldms probe=%d 等待中",
                 asmN, (long long)elapsed, probed ? 1 : 0);
        }
        return;   // ★不置 g_layoutDone
    }

    // ★★ attach 必须排在上面两道闸门之后（早于 il2cpp_init 调用 = abort，真机崩溃栈实锤）。
    EnsureThreadAttached();

    // ---- 重量级诊断（S1~S6 + 启动 S3 后台扫描线程）判定 ----
    const int runs = g_diagRuns.load();
    const bool diagDue = (runs == 0) || (runs < WOR_LAYOUT_MAX_DIAG && elapsed > WOR_LAYOUT_DIAG_REARM_MS);
    if (diagDue) {
        g_diagRuns.fetch_add(1);
        g_layoutDone.store(true);
        EnsureWorSyms();
        // ★ C1/C2：正式构建或 cfg diag=0 时连侦察表都不开（不留任何诊断落盘痕迹）
        if (g_cfgDiag && !g_tblFile) g_tblFile = WorTblOpen("w");   // 主线程持有一份（诊断/对账）
        if (g_tblFile)
            WTBL(g_tblFile, "===== WOR e2 layout run#%d %s %s =====\n", runs + 1, WOR_BUILD_TAG,
                 WOR_DIAG_TIER);
        LOGI(TPL_LOG " layout diag run#%d asm=%zu elapsed=%lldms", runs + 1, asmN, (long long)elapsed);
        ResolveRuntimeLayout();
    } else {
        if (elapsed > WOR_LAYOUT_GIVEUP_MS) {
            const int n = g_layoutTries.fetch_add(1);
            if (n == 0)
                LOGW(TPL_LOG " EnsureLayout: 超 %llds 仍未解析成功，停止重试 sys=%p data=%p hdataRT=%d",
                     (long long)(WOR_LAYOUT_GIVEUP_MS / 1000), g_sysKlass, g_dataKlass,
                     g_offHdataRuntime ? 1 : 0);
            return;
        }
        const int64_t last = g_lastRetryMs.load();
        if ((nowMs - last) < WOR_LAYOUT_RETRY_MS) return;   // ★按时间节流，绝不按次数
        g_lastRetryMs.store(nowMs);
        const int n = g_layoutTries.fetch_add(1);
        LOGW(TPL_LOG " EnsureLayout retry #%d t=%lldms asm=%zu sys=%p data=%p hdataRT=%d",
             n + 1, (long long)elapsed, asmN, g_sysKlass, g_dataKlass, g_offHdataRuntime ? 1 : 0);
        RetryResolveOffsets();
    }
    if (g_offHdataRuntime) g_layoutOk.store(true);
}

// 挂点表（功能阶段数据）
//   RVA_EXCUTE_ONCE = 0 ⇒ HookSlotInstall 的 `if (s.rva && ...)` 短路 ⇒ 强制走
//   HookByName（按名 + 运行时 argc==1 校验），零静态 RVA。INSN 字段仅历史记录，
//   HookAtRva 不会被调用，故不参与任何判定。
static const HookSlot kSlots[] = {
    {"ExcuteOnce", RVA_EXCUTE_ONCE, INSN_EXCUTE_ONCE,
     "CSharpBattle.Battle", "HpChangeEntitySystem", "ExcuteOnce", 1,
     (void*)Hook_ExcuteOnce, (void**)&orig_ExcuteOnce},
};
static const size_t kSlotsCount = sizeof(kSlots) / sizeof(kSlots[0]);

// ⚠️ 原 ResolveProbeHelpers() 已删除：它只有 RVA 一条路（FileOffToVa(RVA_HAS_COMPONENT)），
//   国服 RVA 在国际服指向无关函数，调用即必崩。其职能已并入 ResolveRuntimeLayout()
//   第 4 步（按名解析 Entity.HasComponent + 可执行段校验）。**不许保留任何 RVA fallback**。

// ============================================================
// ⑥ 分组安装 + 懒装（功能阶段按功能组拆；开关全关 = 0 hook）
// ============================================================
static void InstallBattleHooks() {
#if WOR_ENV_STAGE
    // e1 环境阶段：0 hook（唯一出口）。RVA 未按国际服重定位前禁止安装，详见文件头。
    LOGI(TPL_LOG " env stage 0-hook: kSlots disabled tag=%s", WOR_BUILD_TAG);
    return;
#else
    // 装机闸门：运行时布局没解析到 hData ⇒ 偏移全是国服值、极可能错 ⇒ 拒绝安装。
    // 此时 S1~S6 诊断已落盘（wor_tables.txt），靠诊断定案，不冒误写内存的险。
    if (!g_offHdataRuntime) {
        LOGE(TPL_LOG " feature stage: 运行时布局未解析(hData miss) — 拒绝装机 tag=%s", WOR_BUILD_TAG);
        return;
    }
    for (size_t i = 0; i < kSlotsCount; i++) {
        if (HookSlotInstall(kSlots[i])) g_anyHooked.store(true);
    }
    if (orig_ExcuteOnce) g_probeReady.store(true);
#endif
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (kSlotsCount == 0) return;   // 无挂点表 = 不安装
#if WOR_ENV_STAGE
    return;                          // e1：开关全开也不装
#else
    if (g_inv.load() || g_dmgMul.load() > 1) InstallBattleHooks();
#endif
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Wor_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    g_libReady.store(true);
    WorCfgLoad();   // ★ C1：读设备侧 wor_cfg.txt（不存在/写坏 ⇒ 全部保持内建默认值）
#if WOR_ENV_STAGE
    // 环境阶段：不解析探针助手、不装任何 hook —— 只标记「真库已就位」
    LOGI(TPL_LOG " lib ready (liblogic.so, ENV stage e1) hooked=0 tag=%s %s", WOR_BUILD_TAG,
         WOR_DIAG_TIER);
#else
    // 功能阶段：先做**只读**运行时布局解析（S1~S6 诊断 + 后台 S3 扫描 + g_off_* 覆盖），
    // 再装 hook；布局解析失败时 InstallBattleHooks 内部会拒绝装机（功能降级但不崩）。
    EnsureLayout();
    InstallBattleHooks();
    LOGI(TPL_LOG " lib ready (liblogic.so, feature stage) hooked=%d tag=%s %s",
         g_anyHooked.load() ? 1 : 0, WOR_BUILD_TAG, WOR_DIAG_TIER);
#endif
    return true;
}

bool Wor_IsReady() { return g_libReady.load(); }

void Wor_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == WOR_FEAT_INV) {
        g_inv.store(enabled);
        LOGI(TPL_LOG " 无敌 = %d", (int)enabled);
    }
    // ★缺陷1修复：开关动作是「域晚就绪」时最关键的补救入口 —— 若此刻域已就绪则补解析后可立刻装机
    if (!g_layoutOk.load()) EnsureLayout();
    SyncHooks();
}

void Wor_OnValueChanged(int featNum, int value) {
    if (featNum == WOR_FEAT_DMG) {
        const int v = value < 1 ? 1 : (value > DMG_MUL_MAX ? DMG_MUL_MAX : value);
        g_dmgMul.store(v);
        LOGI(TPL_LOG " 伤害倍率 = %d", v);
    }
}

void Wor_Tick(const bool* features, const int* values) {
    // 只做：同步开关 + 兜底安装。禁止在这里 SafeProbe/扫堆/遍历对象/逐对象写
    if (features) g_inv.store(features[WOR_FEAT_INV]);
    if (values) {
        const int v = values[WOR_FEAT_DMG];
        g_dmgMul.store(v < 1 ? 1 : (v > DMG_MUL_MAX ? DMG_MUL_MAX : v));
    }
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) g_libReady.store(true);
    // ★ C1 热重载（2026-10-04 加）：改 wor_cfg.txt 不用重启游戏。
    //   内部按 g_cfgPollMs 节流 —— 绝大多数帧只是一次 WorMonoMs 比较就返回；
    //   到点才 stat 一次文件；只有 mtime/size 真变了才 fopen 重解析。全程不碰游戏内存。
    WorCfgReloadCheck();
    // ★缺陷1修复兜底：il2cpp 域晚就绪时补解析（未就绪时开销≈一次原子读，不碰游戏内存）
    if (!g_layoutOk.load()) EnsureLayout();
    SyncHooks();
}

void Wor_ForceClear() {
    g_dmgMul.store(1);
    g_inv.store(false);
}
