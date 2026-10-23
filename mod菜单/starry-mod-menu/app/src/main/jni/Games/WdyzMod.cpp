/* ================================================================
 *  星光菜单游戏模块 — 我的勇者（wdyz，build 357 热更版）
 *
 *  路线（星光自注入，仅挂 Battle 结算层安全热路径）：
 *    - 五倍攻击 / 伤害倍率：BattleObjectCore.SettleDamage 基类 + MonsterCore 覆盖入口
 *    - 无敌：HeroCore.SettleDamage 承伤归零
 *    - 无限技能：免蓝 = HeroCore.settleMp 扣蓝归零；无CD = CheckSkill 返回值归零
 *
 *  2026-10-18 热更 build 357 重 dump 重定位（so sha256 aeb62966…）：
 *    - 运行时加载 files/update/current/lib/arm64-v8a/libil2cpp.so（非 base.apk 内旧 so）
 *    - 类/方法名零漂移（Battle 全家桶未混淆）；RVA 全漂移，按新 dump 回填：
 *        HeroSettle   0x4794970 -> 0x4795FAC（序言 6DBC2BEB 不变）
 *        BaseSettle   0x47748A4 -> 0x4775EE0（序言 D10503FF 不变）
 *        MonsterSettle 0x47BF274 -> 0x47C08B0（序言 6DBB2BEB 不变）
 *        HeroSettleMp 新增 0x47960F0（序言 FE0F1EF8）
 *    - 铁律：不挂 TryEnterCd/任何技能/CD 簇函数（§10.1-1 SIGILL 自杀）；
 *            不直调任何 IL2CPP 方法（GoDeath 等必崩）；ObscuredFloat 字段不直写。
 * ================================================================ */

#include "WdyzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <dirent.h>   // §④ 就绪门读 /proc/self/task
#include <mutex>      // §④ 装钩互斥
#include <sys/mman.h>
#include <time.h>     // §④ 就绪门退避计时（CLOCK_MONOTONIC）

#include "Dobby/dobby.h"

// Dobby 自带的指令缓存刷新（读 CTR_EL0 拿真实行大小 + 查 IDC/DIC 位）。
// 本机自写的 dc cvau/ic ivau 写死 64 字节行、不查 IDC/DIC → 无效，故直接用 Dobby 的实现。
extern "C" void ClearCache(void* start, void* end);
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define WDYZ_LOG "[Wdyz]"

// ============================================================
// ① 开关与 orig
// ============================================================
static std::atomic<bool> g_libReady{false};  // il2cpp 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};    // 五倍攻击
static std::atomic<bool> g_inv{false};    // 无敌
static std::atomic<bool> g_skill{false};  // 无限技能（无CD+免蓝）
static std::atomic<int>  g_dmgx{1};       // 伤害倍率 Slider 1-50（1=关闭）

// BattleObjectCore.SettleDamage(value, shield, toughness, energy, fromgroup)
// 参数寄存器：s0=value s1=shield s2=toughness s3=energy w1=fromgroup（GP 与 FP 寄存器独立分配）
typedef void (*SettleDamage_t)(void* self, float value, float shield, float toughness,
                               float energy, int fromgroup);
static SettleDamage_t g_origMonsterSettle = nullptr;
static SettleDamage_t g_origHeroSettle = nullptr;
static SettleDamage_t g_origBaseSettle = nullptr;

// HeroCore.settleMp(int value)：value<0 为扣蓝
typedef void (*SettleMp_t)(void* self, int value);
static SettleMp_t g_origHeroSettleMp = nullptr;

// ============================================================
// ② 偏移与 RVA（7.6.4 base 重 dump：rt360 热更 = 本表 +0x208）
//   绑定产物：/tmp/wdyz_dump/base/libil2cpp.so sha256 288c1a24…（7.6.4 包内 so，base 配对）
//   地址口径 = dump.cs 的 Offset（文件偏移），供 FileOffToVa 在 /proc/self/maps 换算 VA。
//   RVA 序言首 4 字节（小端，2026-10-23 从 so 逐字节实测）：
//     MonsterCore.SettleDamage   Offset 0x47ECE5C  "eb 2b bb 6d" → 0x6DBB2BEB
//     HeroCore.SettleDamage      Offset 0x47E26BC  "eb 2b bc 6d" → 0x6DBC2BEB
//     BattleObjectCore.SettleDamage Offset 0x47A25B0 "ff 03 05 d1" → 0xD10503FF
//     HeroCore.settleMp          Offset 0x47E27F8  "fe 0f 1e f8" → 0xF81E0FFE
//     SKillBase.CheckSkill(bool) Offset 0x485B26C  "fe 4f bf a9" → 0xA9BF4FFE
//       （无CD 4B 补丁点 = 入口 +0x60 = 0x485B2CC，原指令 "00 00 13 2a"=0x2A130000）
//   历史 357（7.6.3 包用 357 热更 so，勿删）：Monster 0x47C08B0 / Hero 0x4795FAC /
//     Base 0x4775EE0 / settleMp 0x47960F0 / 无CD ResolveFn 0x482ECB8。
//   任一序言不符（热更/漂移）→ 自动转按名，不挂错地址。
// ============================================================
enum : size_t {
    kRvaMonsterSettle = 0x47ECE5C,
    kRvaHeroSettle    = 0x47E26BC,
    kRvaBaseSettle    = 0x47A25B0,
    kRvaHeroSettleMp  = 0x47E27F8,
};

// ============================================================
// ③ 安全助手（防闪退核心，勿删）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

// ============================================================
// ④ 并发装钩互斥 + 槽位熔断 + il2cpp 就绪门（勿删）
//   并发根因（2026-10-23 实测）：UI 线程与 200ms Tick 线程同时进入 SyncHooks()，
//   HookSlotInstall 的 `if (origPtr && *origPtr)` 是 check-then-act、非原子 ⇒ 同一地址被
//   DobbyHook 两次 ⇒ DobbyCodePatch 越界 memcpy ⇒ SIGSEGV(SEGV_ACCERR)。
//   修法：g_installMutex 串行化 + 持锁后 double-check；失败槽位进 g_slotDead[] 熔断。
//
//   ⚠️ R20 铁律（照 Games/ZzzMod.cpp「头号根因」反面教材）：本文件禁止出现
//   sigaction / sigsetjmp / siglongjmp，禁止进程级替换 SIGSEGV/SIGBUS/SIGILL。
//   2026-10-23 冷启崩实测：装上进程级处理器后，启动期 Unity/ART 自发的可恢复 SIGSEGV
//   探针被我们抢先接管，链回旧处理器时 ucontext 传 nullptr ⇒ 下游从空 mcontext 读
//   （SEGV_MAPERR fault 0xb8、size 0x110，__memcpy ← libunity ← libil2cpp），
//   backtrace 里没有 libstarry_magic 帧。故一律裸读裸写，地址只指向「活对象或本进程
//   已装载模块」（调用点保证）。
// ============================================================
static std::mutex g_installMutex;   // 保护「装钩全过程」，只在 SyncHooks 里 **try_lock** 取
static std::mutex g_patchMutex;     // 只保护无CD 补丁的读-校验-写
// 2026-10-23 ANR 修：装钩锁一律 try_lock，抢不到就让「本轮装钩」作废（开关状态在原子量里，
// 不丢状态），绝不让任何线程（尤其 UI 主线程 nativeSetFeature → onFeatureChanged）阻塞等这把锁。
// 反查依据：0x12da4 就是 SyncHooks 里这把锁的 bl mutex::lock（锁对象 0x221a20 = g_installMutex）。
static std::atomic<int> g_installDeferCount{0};   // try_lock 未取到的次数（只用于限频日志）

// ---- 4.1 原语（裸读/裸写；失败只返回 false，交给熔断） ----
static bool DobbyHookChecked(const char* tag, void* target, void* hook, void** orig) {
    const int rc = DobbyHook(target, hook, orig);
    if (rc != 0) {
        LOGW(WDYZ_LOG " %s dobby fail rc=%d", tag, rc);
        if (orig) *orig = nullptr;   // 半写保护：失败必须清 orig，否则后续调用跳野指针
        return false;
    }
    return true;
}

static bool ReadU32At(const char* tag, const void* p, uint32_t* out) {
    (void)tag;
    *out = *reinterpret_cast<const volatile uint32_t*>(p);
    return true;
}

static bool WriteU32At(const char* tag, void* p, uint32_t v) {
    (void)tag;
    *reinterpret_cast<volatile uint32_t*>(p) = v;
    return true;
}

// ---- 4.2 槽位熔断：同一槽位连续失败 kSlotFailLimit 次即本次会话不再重试 ----
static const int kSlotsMax = 16;                    // kSlots 只有 4 个，留余量
static const int kSlotFailLimit = 5;                // 有界重试，防每 Tick 无限重试
static bool g_slotDead[kSlotsMax];                  // 只在 g_installMutex 下读写
static int  g_slotFails[kSlotsMax];                 // 只在 g_installMutex 下读写
static std::atomic<int> g_installedCount{0};        // 已成功装上的槽位数
static std::atomic<int> g_deadCount{0};             // 已熔断的槽位数

// ---- 4.3 il2cpp 运行时就绪门（只用 /proc，不调任何 il2cpp API） ----
// 实测（2026-10-23）：prefs 持久化「无敌 ON」时，200ms tick 在进程启动 ~2s 就发起装钩；
// 此时游戏仍停在 PrivacyActivity、Unity/il2cpp 运行时尚未初始化。此前任何 il2cpp API 探测
// 都会把进程打死，故就绪判据改用 /proc 只读信号，二选一（任一命中即认为可安全调 API）：
//   ① /proc/self/maps 出现 global-metadata 映射（il2cpp 装载 metadata 的直接证据）；
//   ② /proc/self/task/*/comm 出现 UnityMain，且已持续 ≥10s（Unity 播放器起来后再等一个
//      初始化窗口，避免播放器刚建线程就调 API）。
// 未就绪：本轮 SyncHooks 直接 return，不下发任何 il2cpp API；节流 1s 复查一次。
static std::atomic<bool> g_runtimeReady{false};
static std::atomic<int>  g_readyProbeLogs{0};
static long long g_nextReadyMs = 0;                  // 下次探测时间；只在 g_installMutex 下读写
static long long g_unityMainSeenMs = 0;              // 首次看到 UnityMain 的时间；同上
static long long g_nextResolveMs = 0;                // 类解析重试退避；只在 g_installMutex 下读写
static int g_resolveFails = 0;                       // 类解析失败次数；只在 g_installMutex 下读写

static void ResolveBattleClasses();                  // 定义在 ⑥

static long long NowMs() {
    struct timespec ts;
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return (long long)ts.tv_sec * 1000 + ts.tv_nsec / 1000000;
}

// /proc/self/maps 是否已有 global-metadata 映射（纯文件读，不触 il2cpp）
static bool Il2CppMetadataMapped() {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool found = false;
    while (fgets(line, sizeof(line), fp)) {
        if (strstr(line, "global-metadata")) { found = true; break; }
    }
    fclose(fp);
    return found;
}

// /proc/self/task 里是否存在指定线程名（纯文件读，不触 il2cpp）
static bool ThreadNamedExists(const char* name) {
    DIR* d = opendir("/proc/self/task");
    if (!d) return false;
    bool found = false;
    struct dirent* e;
    while ((e = readdir(d)) != nullptr) {
        if (e->d_name[0] == '.') continue;
        char path[160];
        snprintf(path, sizeof(path), "/proc/self/task/%s/comm", e->d_name);
        FILE* f = fopen(path, "rt");
        if (!f) continue;
        char comm[64] = {0};
        if (fgets(comm, sizeof(comm), f)) {
            size_t n = strlen(comm);
            while (n > 0 && (comm[n - 1] == '\n' || comm[n - 1] == '\r')) comm[--n] = 0;
            if (strcmp(comm, name) == 0) found = true;
        }
        fclose(f);
        if (found) break;
    }
    closedir(d);
    return found;
}

// 就绪门：未就绪返回 false，调用方必须直接 return（不得再下发任何 il2cpp API）。
static bool Il2CppRuntimeReady() {
    if (g_runtimeReady.load()) return true;
    const long long now = NowMs();
    if (now < g_nextReadyMs) return false;           // 1s 节流
    g_nextReadyMs = now + 1000;

    if (Il2CppMetadataMapped()) {
        g_runtimeReady.store(true);
        LOGI(WDYZ_LOG " il2cpp ready (metadata mapped) — 进入装钩");
        return true;
    }
    if (ThreadNamedExists("UnityMain")) {
        if (g_unityMainSeenMs == 0) {
            g_unityMainSeenMs = now;
            LOGI(WDYZ_LOG " UnityMain 已出现，等待 10s 初始化窗口后再判就绪");
        } else if (now - g_unityMainSeenMs >= 10000) {
            g_runtimeReady.store(true);
            LOGI(WDYZ_LOG " il2cpp ready (UnityMain+%lldms) — 进入装钩", now - g_unityMainSeenMs);
            return true;
        }
    }
    const int k = g_readyProbeLogs.fetch_add(1) + 1;
    if (k == 1 || (k % 30) == 0) {
        LOGI(WDYZ_LOG " il2cpp not ready — skip install（不下发 il2cpp API, probe#%d）", k);
    }
    return false;
}

// ============================================================
// ⑤ 双路径挂点机器（禁 base+RVA、禁全 RX 误扫）
// ============================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

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

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn, void* hook, void** orig) {
    void* va = FileOffToVa(GameUnity::TARGET_LIB, rva);
    if (!va) {
        LOGW("%s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        if (!ReadU32At(tag, va, &got)) return false;   // 读序言也走保护区
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW("%s rva drift got=0x%08X want=0x%08X — try by-name", tag, got, expectInsn);
            return false;
        }
    }
    if (!DobbyHookChecked(tag, va, hook, orig)) {
        LOGW("%s dobby fail @rva", tag);
        return false;
    }
    LOGI("%s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(GameUnity::TARGET_LIB);
}

static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

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
    void* mi = api.GetMethod(k, method, argc);
    if (!mi) {
        LOGW("%s method miss %s.%s — degrade, install others", tag, cls, method);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW("%s bad method ptr %s.%s", tag, cls, method);
        return false;
    }
    if (!DobbyHookChecked(tag, fn, hook, orig)) {
        LOGW("%s dobby fail @name %s", tag, method);
        return false;
    }
    LOGI("%s hooked %s.%s", tag, cls, method);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;         // 0 = 只走按名
    uint32_t    expectInsn;  // 0 = 不校验序言
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

// ⚠️ 只在 g_installMutex 保护下调用（由 InstallBattleHooks 持锁循环）。
// 这里的 `if (origPtr && *origPtr)` 是 check-then-act，本身不是原子的。
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;   // 已装：幂等返回（配合持锁 double-check）
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================
// ⑥ 敌我/类别判定（复用 7.6.3 已验证实现）
// ============================================================
static std::atomic<int> g_monSamp{0};
static std::atomic<int> g_heroSamp{0};
static std::atomic<int> g_rawBaseSamp{0};
static std::atomic<int> g_rawHeroSamp{0};
static std::atomic<int> g_rawMonSamp{0};

static void* g_monsterCoreClass = nullptr;
static void* g_heroCoreClass = nullptr;
static std::atomic<bool> g_classCacheReady{false};

static bool IsSameOrSubclass(void* klass, void* base) {
    if (!klass || !base) return false;
    if (klass == base) return true;
    auto& api = GetApi();
    if (!api.class_parent) return false;
    for (int i = 0; i < 32 && klass; i++) {
        klass = api.class_parent(klass);
        if (klass == base) return true;
    }
    return false;
}

static void ResolveBattleClasses() {
    if (g_classCacheReady.load() || !EnsureApi()) return;
    void* monster = FindClassNs("Battle", "MonsterCore");
    void* hero = FindClassNs("Battle", "HeroCore");
    if (monster) g_monsterCoreClass = monster;
    if (hero) g_heroCoreClass = hero;
    if (monster && hero) {
        g_classCacheReady.store(true);
        LOGI(WDYZ_LOG " class cache MonsterCore=%p HeroCore=%p", monster, hero);
    }
}

static const char* ClassNameOf(void* self) {
    auto& api = GetApi();
    if (!self || !api.object_class || !api.class_name) return "?";
    void* k = api.object_class(self);
    const char* n = k ? api.class_name(k) : nullptr;
    return n ? n : "?";
}

static int ClassTagOf(void* self) {
    auto& api = GetApi();
    if (!api.object_class || !self) return 0;
    void* k = api.object_class(self);
    if (!k) return 0;
    if (IsSameOrSubclass(k, g_monsterCoreClass)) return 1;
    if (IsSameOrSubclass(k, g_heroCoreClass)) return 2;
    if (!api.class_name) return 0;
    const char* n = api.class_name(k);
    if (!n) return 0;
    if (strstr(n, "Monster") || strstr(n, "Boss") || strstr(n, "mob") || strstr(n, "Mob")) return 1;
    if (strstr(n, "Hero")) return 2;
    return 0;
}

static int MonsterTypeOf(void* self) {
    if (!self) return -1;
    auto* core = static_cast<uint8_t*>(self);
    void* monster = *reinterpret_cast<void**>(core + 0xD8);
    if (!monster) return -1;
    auto* obj = static_cast<uint8_t*>(monster);
    return *reinterpret_cast<int*>(obj + 0x60);
}

// ============================================================
// ⑦ 无限技能：无CD 直写链（见 10_docs/我的勇者/挂点分析_无限技能.md）
//   HeroCore → skillEntity(0x30, ISkillable) → skillContainer(0x18) →
//   skillDic(0x18, Dictionary<int,MSkill>) → 枚举 value → MSkill →
//   skillBase(0x18, SKillBase) → CDTime(0x38)=0 / cding(0x3C)=false
//   Dictionary<int,MSkill> 布局（.NET 标准，Metadata v31）：0x18=_entries 0x20=_count；
//   Entry=24B：hashCode(0) next(4) key(8) pad(12) value(16)。防御性判空，布局误读时静默不写。
// ============================================================
static void ClearHeroSkillCd(void* heroCore) {
    if (!heroCore || !OkPtr(heroCore)) return;
    static std::atomic<int> chainSamp{0};
    auto* core = static_cast<uint8_t*>(heroCore);
    void* skillEntity = *reinterpret_cast<void**>(core + 0x30);
    if (!OkPtr(skillEntity)) return;
    void* container = *reinterpret_cast<void**>(static_cast<uint8_t*>(skillEntity) + 0x18);
    if (!OkPtr(container)) return;
    void* dic = *reinterpret_cast<void**>(static_cast<uint8_t*>(container) + 0x18);
    if (!OkPtr(dic)) return;
    void* entries = *reinterpret_cast<void**>(static_cast<uint8_t*>(dic) + 0x18);
    int count = *reinterpret_cast<int*>(static_cast<uint8_t*>(dic) + 0x20);
    if (!entries || count <= 0 || count > 64) {
        if (chainSamp.fetch_add(1) < 6) {
            LOGI(WDYZ_LOG "CD chain stop hero=%p entity=%p cont=%p dic=%p entries=%p count=%d",
                 heroCore, skillEntity, container, dic, entries, count);
        }
        return;
    }
    // Il2CppArray 头 = 0x20（klass0x0+monitor0x8+bounds0x10+max_length0x18）；
    // Entry<int,MSkill*> = 0x18，value 指针在 entry+0x10。
    int cleared = 0;
    for (int i = 0; i < count && cleared < 16; i++) {
        void* mskill = *reinterpret_cast<void**>(static_cast<uint8_t*>(entries) + 0x20 + i * 0x18 + 0x10);
        if (!OkPtr(mskill)) continue;
        void* skillBase = *reinterpret_cast<void**>(static_cast<uint8_t*>(mskill) + 0x18);
        if (!OkPtr(skillBase)) continue;
        *reinterpret_cast<float*>(static_cast<uint8_t*>(skillBase) + 0x38) = 0.0f;  // CDTime
        *reinterpret_cast<bool*>(static_cast<uint8_t*>(skillBase) + 0x3C) = false;   // cding
        cleared++;
    }
    if (chainSamp.fetch_add(1) < 6) {
        LOGI(WDYZ_LOG "CD clear hero=%p dic=%p count=%d cleared=%d",
             heroCore, dic, count, cleared);
    }
}

// ============================================================
// ⑧ hook 函数与挂点表
// ============================================================
// 基类 BattleObjectCore.SettleDamage — 真正 HP 落地点。
// 普通怪的五倍攻击/伤害倍率必须在这里改：MonsterCore 覆盖入口会重算 value。
static void Wdyz_HookedBaseSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (g_rawBaseSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "BASE raw v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p class=%p tag=%d name=%s",
             value, shield, toughness, fromgroup, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassTagOf(self), ClassNameOf(self));
    }
    const int tag = ClassTagOf(self);
    if (tag == 1) {
        if (g_dmg.load()) {
            const float inValue = value;
            if (value != 0.0f) value *= 5.0f;
            if (g_monSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "BASE enemy x5 v=%+.3f->%+.3f sh=%+.3f g=%d self=%p",
                     inValue, value, shield, fromgroup, self);
            }
        }
        const float N = static_cast<float>(g_dmgx.load());
        if (N > 1.0f) {
            const float inValue = value, inShield = shield;
            if (value > 0.0f) value *= N;     // 正=伤害，只放大伤害（负=治疗不动）
            if (shield > 0.0f) shield *= N;   // 正=扣盾，独立 if（全吸收时 value==0 也要放大盾伤）
            if (g_monSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "BASE enemy x%.1f v=%+.3f->%+.3f sh=%+.3f->%+.3f g=%d self=%p",
                     N, inValue, value, inShield, shield, fromgroup, self);
            }
        }
    } else if (tag == 2 && g_inv.load()) {
        if (g_heroSamp.fetch_add(1) < 12) {
            LOGI(WDYZ_LOG "BASE hero inv v=%+.3f->0 sh=%+.3f->0 g=%d self=%p",
                 value, shield, fromgroup, self);
        }
        value = 0.0f;
        shield = 0.0f;
    }
    if (g_origBaseSettle) g_origBaseSettle(self, value, shield, toughness, energy, fromgroup);
}

// 五倍攻击/伤害倍率：仅处理 DamageCollectionBoss=7（覆盖入口内提前返回，不走基类）。
static void Wdyz_HookedMonsterSettle(void* self, float value, float shield, float toughness,
                                     float energy, int fromgroup) {
    const int monsterType = MonsterTypeOf(self);
    if (g_rawMonSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "MON raw v=%+.3f sh=%+.3f tg=%+.3f g=%d type=%d self=%p class=%p name=%s",
             value, shield, toughness, fromgroup, monsterType, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassNameOf(self));
    }
    if (monsterType == 7) {
        if (g_dmg.load()) {
            const float inValue = value;
            if (value != 0.0f) value *= 5.0f;
            if (g_monSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "MON type7 x5 v=%+.3f->%+.3f sh=%+.3f tg=%+.3f g=%d self=%p",
                     inValue, value, shield, toughness, fromgroup, self);
            }
        }
        const float N = static_cast<float>(g_dmgx.load());
        if (N > 1.0f) {
            if (value > 0.0f) value *= N;     // 正=伤害
            if (shield > 0.0f) shield *= N;   // 正=扣盾，独立 if
        }
    }
    if (g_origMonsterSettle) g_origMonsterSettle(self, value, shield, toughness, energy, fromgroup);
}

// 无敌：HeroCore.SettleDamage — value 与 shield 都是承伤分量，必须同时归零。
// 无限技能(无CD)：self=HeroCore 时走链清 CD。
static void Wdyz_HookedHeroSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (g_rawHeroSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "HERO raw v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p class=%p name=%s",
             value, shield, toughness, fromgroup, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassNameOf(self));
    }
    if (g_inv.load()) {
        if (value != 0.0f || shield != 0.0f) {
            if (g_heroSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "HERO inv v=%+.3f->0 sh=%+.3f->0 tg=%+.3f g=%d self=%p",
                     value, shield, toughness, fromgroup, self);
            }
        }
        value = 0.0f;
        shield = 0.0f;
    }
    if (g_skill.load()) {
        // ⚠️ 无CD（直写 CDTime/cding）已停用：与游戏 CD 状态机冲突，会把倒计时冻结、技能无法点击。
        // 目前「无限技能」只保留免蓝（settleMp）。无CD 需换方案（见 挂点分析_无限技能_重查.md 未闭环）。
        // ClearHeroSkillCd(self);
    }
    if (g_origHeroSettle) g_origHeroSettle(self, value, shield, toughness, energy, fromgroup);
}

// 免蓝：HeroCore.settleMp(int value)。
// 符号（反汇编 0x477A3C4 scvtf s1,w19 + 0x477A3C8 fsub s1,s8,s1 实证）：
//   newMP = 当前MP − value ⇒ value>0 才是扣蓝、value<0 是加蓝。故归零 value>0。
static void Wdyz_HookedHeroSettleMp(void* self, int value) {
    if (g_skill.load() && value > 0) {
        static std::atomic<int> samp{0};
        if (samp.fetch_add(1) < 12) {
            LOGI(WDYZ_LOG "MP free v=%d->0 self=%p class=%p name=%s",
                 value, self,
                 GetApi().object_class ? GetApi().object_class(self) : nullptr,
                 ClassNameOf(self));
        }
        value = 0;
    }
    if (g_origHeroSettleMp) g_origHeroSettleMp(self, value);
}

static const HookSlot kSlots[] = {
    {"HeroSettle",    kRvaHeroSettle,    0x6DBC2BEB, "Battle", "HeroCore",     "SettleDamage", 5,
     (void*)Wdyz_HookedHeroSettle,    (void**)&g_origHeroSettle},
    {"MonsterSettle", kRvaMonsterSettle, 0x6DBB2BEB, "Battle", "MonsterCore",  "SettleDamage", 5,
     (void*)Wdyz_HookedMonsterSettle, (void**)&g_origMonsterSettle},
    {"BaseSettle",    kRvaBaseSettle,    0xD10503FF, "Battle", "BattleObjectCore", "SettleDamage", 5,
     (void*)Wdyz_HookedBaseSettle,    (void**)&g_origBaseSettle},
    {"HeroSettleMp",  kRvaHeroSettleMp,  0xF81E0FFE, "Battle", "HeroCore",     "settleMp", 1,
     (void*)Wdyz_HookedHeroSettleMp,  (void**)&g_origHeroSettleMp},
};

// ============================================================
// ⑨ 受保护函数补丁助手（无CD 用）：thunk 跟随 + 4B 补丁
// ============================================================
// 跟随 thunk 解析真实函数体基址。游戏把受保护函数搬到匿名内存，原 .text 位置只留
//   ldr x17, [pc,#imm]; br x17; <8字节字面量=wrapper地址>
// wrapper 固定 0x34 字节后才是真实函数体（已实测 CheckSkill / SettleDamage 两处一致）。
static void* FollowThunk(void* fn) {
    if (!fn) return nullptr;
    uint32_t insn = 0;
    if (!ReadU32At("nocd", fn, &insn)) return nullptr;
    if (insn == 0x58000051u) {  // ldr x17, [pc, #imm]
        uint64_t t = 0;
        memcpy(&t, (char*)fn + 8, 8);
        if (t > 0x10000 && t < 0x7FFFFFFFFFFFull) {
            return (char*)t + 0x34;  // wrapper + 0x34 = 真实函数体
        }
    }
    return fn;  // 未 thunk，返回原入口
}

// 返回 true = 已处于目标态（含幂等跳过）；false = 失配/写入失败，调用方据此置闩锁。
static bool Patch4At(void* va, uint32_t want, uint32_t revert, const char* tag) {
    if (!va) {
        LOGW(WDYZ_LOG " %s patch va null", tag);
        return false;
    }
    uint32_t cur = 0;
    if (!ReadU32At(tag, va, &cur)) return false;   // 读补丁点也走保护区
    if (cur == want) return true;                       // 已是目标态，幂等跳过
    if (cur != revert) {
        LOGW(WDYZ_LOG " %s patch expect drift cur=0x%08X (skip)", tag, cur);
        return false;
    }
    uintptr_t page = (uintptr_t)va & ~(uintptr_t)0xFFFu;
    if (mprotect((void*)page, 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        LOGW(WDYZ_LOG " %s patch mprotect fail @%p", tag, va);
        return false;
    }
    const bool wrote = WriteU32At(tag, va, want);  // 写只读页也走保护区
    ClearCache(va, (char*)va + 4);
    mprotect((void*)page, 0x1000, PROT_READ | PROT_EXEC);
    if (wrote) LOGI(WDYZ_LOG " %s patch applied @%p", tag, va);
    return wrote;
}

// 无CD：SKillBase.CheckSkill 最终 `orr w0,w0,w19`（补丁点 = 入口 +0x60）→ mov w0,#0。
// 入口按名解析（Battle.Skill.SKillBase::CheckSkill(bool)，与 kSlots 同口径），不再用
// 硬编码文件偏移：热更副本上 0x485B26C 已落到无关代码（实测 +0x60 读回 0x94388E9F）。
// 解析/身份校验/补丁点校验失败 ⇒ 置闩锁：同一开关状态不再每 Tick 重试、不在错地址读 4B；
// 只有开关切换时才清闩重试一次。
static std::atomic<bool> g_nocdLatched{false};   // 本次开关状态已放弃
static std::atomic<bool> g_nocdLastOn{false};    // 上次处理过的开关状态
static std::atomic<bool> g_nocdApplied{false};   // 补丁当前是否为目标态

static void* ResolveCheckSkillEntry() {
    if (!EnsureApi()) return nullptr;
    auto& api = GetApi();
    void* k = FindClassNs("Battle.Skill", "SKillBase");
    if (!k) k = api.FindClassByShortName("SKillBase");
    if (!k) {
        LOGW(WDYZ_LOG " nocd class miss SKillBase — 已置闩锁，本次不再重试");
        return nullptr;
    }
    void* mi = api.GetMethod(k, "CheckSkill", 1);
    if (!mi) {
        LOGW(WDYZ_LOG " nocd method miss SKillBase.CheckSkill — 已置闩锁，本次不再重试");
        return nullptr;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW(WDYZ_LOG " nocd bad method ptr SKillBase.CheckSkill=%p", fn);
        return nullptr;
    }
    return FollowThunk(fn);
}

static void ApplyNoCdPatch(bool on) {
    if (!g_runtimeReady.load()) return;   // 就绪门前不碰 il2cpp API（开关状态保留，不置闩）
    std::lock_guard<std::mutex> lk(g_patchMutex);   // 读-校验-写整体串行，防双线程互踩页权限

    if (on == g_nocdLastOn.load()) {
        if (g_nocdLatched.load()) return;           // ★ 同状态失配闩：不重试、不读 4B
        if (g_nocdApplied.load() == on) return;     // 已处于目标态
    } else {
        g_nocdLatched.store(false);                 // 开关切换：清闩，允许重试一次
        g_nocdLastOn.store(on);
    }

    void* entry = ResolveCheckSkillEntry();
    if (!entry) {
        g_nocdLatched.store(true);
        return;
    }

    uint32_t pro = 0;
    if (!ReadU32At("nocd", entry, &pro)) {     // 身份校验：入口序言
        g_nocdLatched.store(true);
        return;
    }
    if (pro != 0xA9BF4FFE) {                        // stp x30,x19,[sp,#-0x10]!
        LOGW(WDYZ_LOG " nocd entry prologue 0x%08X != 0xA9BF4FFE (latched, no retry)", pro);
        g_nocdLatched.store(true);
        return;
    }

    static const uint32_t kOn  = 0x52800000u;  // mov w0, #0
    static const uint32_t kOff = 0x2A130000u;  // orr w0, w0, w19（原指令）
    const uint32_t want   = on ? kOn  : kOff;
    const uint32_t revert = on ? kOff : kOn;
    if (!Patch4At((char*)entry + 0x60, want, revert, "nocd")) {
        g_nocdLatched.store(true);
        LOGW(WDYZ_LOG " nocd patch 未生效 — 已置闩锁（同一状态不再重试）");
        return;
    }
    g_nocdApplied.store(on);
}

// ============================================================
// ⑩ 分组安装 + 懒装（安装全程持 g_installMutex，防 UI/Tick 同址双装）
// ============================================================
static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

static void InstallBattleHooks() {
    // 类解析：未命中时退避重试（0.5s→8s），超过 40 次本次会话放弃；不阻塞其他槽位逻辑
    if (!g_classCacheReady.load()) {
        const long long now = NowMs();
        if (now < g_nextResolveMs) return;
        ResolveBattleClasses();
        if (!g_classCacheReady.load()) {
            g_resolveFails++;
            long long delay = 500LL << (g_resolveFails > 4 ? 4 : (g_resolveFails - 1));
            if (delay > 8000) delay = 8000;
            g_nextResolveMs = now + delay;
            if (g_resolveFails == 1 || g_resolveFails % 10 == 0) {
                LOGW(WDYZ_LOG " Battle 类解析未命中 #%d — 退避 %lldms 后重试",
                     g_resolveFails, delay);
            }
            if (g_resolveFails >= 40) {
                LOGW(WDYZ_LOG " Battle 类始终未命中（%d 次）— 本次会话不再解析", g_resolveFails);
                g_nextResolveMs = now + 3600000LL;
            }
            return;   // 没拿到类就不装钩
        }
    }
    int ok = 0, dead = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (g_slotDead[i]) { ++dead; continue; }        // 熔断，跳过
        if (HookSlotInstall(kSlots[i])) {
            ++ok;
            g_slotFails[i] = 0;
        } else if (++g_slotFails[i] >= kSlotFailLimit) {
            g_slotDead[i] = true;                       // 本次会话永久拉黑
            LOGW(WDYZ_LOG " slot[%d] %s 连续失败 %d 次 — 本次会话熔断，不再重试",
                 i, kSlots[i].tag, g_slotFails[i]);
        }
    }
    const int prevOk = g_installedCount.exchange(ok);
    const int prevDead = g_deadCount.exchange(dead);
    if (ok != prevOk || dead != prevDead) {
        LOGI(WDYZ_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    }
    if (ok > 0) g_anyHooked.store(true);
}

static bool AllSlotsSettled() {
    return g_installedCount.load() + g_deadCount.load() >= kSlotsCount;
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    // 全关：不探测、不装钩、不给 noCD 下发任何 il2cpp API
    if (!g_dmg.load() && !g_inv.load() && !g_skill.load()) return;

    // ---- 就绪门：il2cpp 运行时没起来之前，跳过本轮，不下发任何 il2cpp API ----
    if (!Il2CppRuntimeReady()) return;

    if (g_dmg.load() || g_inv.load() || g_skill.load()) {
        if (!AllSlotsSettled()) {                              // 快路径：装齐/熔断后不进锁
            // ★ 2026-10-23 ANR 修：lock_guard（futex_wait 阻塞）→ try_lock（不等待）。
            //   二次打开时菜单 refreshFeatures 触发 Switch.setChecked → nativeSetFeature →
            //   onFeatureChanged → 本函数；此刻 Tick 线程正持 g_installMutex 做类解析/装钩，
            //   主线程卡锁 >5s ⇒ Input dispatching timed out（主线程栈 0xfc30 → 0x12da4）。
            //   现在抢不到锁就跳过「本轮装钩」：g_dmg/g_inv/g_skill 是原子量，状态不丢，
            //   Tick 下一拍（200ms）会带同样状态重来；防双装语义由「只有持锁者装」保证。
            std::unique_lock<std::mutex> lk(g_installMutex, std::try_to_lock);
            if (!lk.owns_lock()) {
                const int k = g_installDeferCount.fetch_add(1) + 1;
                if (k == 1 || (k % 30) == 0) {
                    LOGI(WDYZ_LOG " install busy — try_lock 未取到，本轮跳过装钩（下次 tick 继续 #%d）", k);
                }
            } else if (!AllSlotsSettled()) {                   // double-check
                InstallBattleHooks();
            }
        }
    }
    ApplyNoCdPatch(g_skill.load());
}

// ============================================================
// ⑪ 对外接口（MainModern.cpp 注册表契约）
// ============================================================
bool Wdyz_TryInit() {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    g_libReady.store(true);
    SyncHooks();
    LOGI(WDYZ_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Wdyz_IsReady() { return g_libReady.load(); }

void Wdyz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WDYZ_FEAT_DMG:  g_dmg.store(enabled); break;
        case WDYZ_FEAT_INV:  g_inv.store(enabled); break;
        case WDYZ_FEAT_SKILL:g_skill.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Wdyz_OnValueChanged(int featNum, int value) {
    if (featNum == WDYZ_FEAT_DMGX) {
        if (value < 1) value = 1;
        if (value > 50) value = 50;
        g_dmgx.store(value);
        LOGI(WDYZ_LOG " dmgx=%d", value);
    }
}

void Wdyz_Tick(const bool* features, const int* values) {
    if (features) {
        g_dmg.store(features[WDYZ_FEAT_DMG]);
        g_inv.store(features[WDYZ_FEAT_INV]);
        g_skill.store(features[WDYZ_FEAT_SKILL]);
    }
    if (values) {
        int v = values[WDYZ_FEAT_DMGX];
        if (v < 1) v = 1;
        if (v > 50) v = 50;
        g_dmgx.store(v);
    }
    if (!g_libReady.load() && isLibraryLoaded(GameUnity::TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Wdyz_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_skill.store(false);
    g_dmgx.store(1);
    ApplyNoCdPatch(false);
}
