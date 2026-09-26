/* ================================================================
 *  BlfxMod — 爆裂防线（com.muyegame.blfx）1.01.06  ★星空版
 *  斌哥改包复刻
 *
 *  ── 功能位（BlfxEntry.cpp featureList 下标）─────────────────
 *    1 伤害倍率 SeekBar 1..100（1 = 关）
 *    2 无敌     Toggle
 *    3 战斗加速 SeekBar 1..10（1 = 关）
 *    免广告：不做（无任何可达广告入口，已实证）
 *
 *  ── 实机取证结论（勿凭猜测推翻）────────────────────────────
 *  · 热更类可解析：hybridclr 加载后 domain 有 72 个 image，
 *    zbMonster / zbCombatDefense / zbCombatSpeed 走
 *    image[72] 的 il2cpp_class_from_name 即命中（L1），无需内存扫描。
 *  · v2 实测：两者 methodPointer 相同（共享惰性桩），DobbyHook rc=0 但 0 命中
 *    ⇒ v3 起功能改走【字段路线】（零 hook）：
 *       zbCombatSystem.sInstance → .combatData → .defense / .monsters.ActiveList
 *  · 无敌 = 防线 curHp 钉 maxHp；伤害倍率 = 每只怪 curHp 压到 maxHp/mul
 *    ⇒ 有效血量 1/mul，等效 mul 倍伤害（客户端无任何检查覆盖怪/防线 HP）。
 *  · 第一次 il2cpp_domain_get 可能 SIGSEGV（非确定性），必须包 SafeProbe。
 *  · 内存扫描 = 每秒 1.5 GB → 卡死游戏。永久禁用。
 *
 *  ── 反作弊铁律 ────────────────────────────────────────────
 *  · curHp/maxHp 是 zbSafeLong 双影子，禁止裸写单副本（否则 ReportCheat 强退）。
 *  · 战斗加速必须同时改写 zbCombatSpeed.speed1/2/3：
 *    zbCombatHandler 用 speed3 作时间加速阈值（第 526-528 行），
 *    只改 Time.timeScale 会让 accumTime/speed3 超速而被上报。
 * ================================================================ */
#include "BlfxMod.h"

#include <atomic>
#include <cstdarg>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>
#include <pthread.h>
#include <unistd.h>
#include <vector>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define BLFX_LOG  "[blfx]"
#define BLFX_LIB  sec_lib_il2cpp()
#define BLFX_DUMP "/data/data/com.muyegame.blfx/files/blfx_probe.txt"
// 诊断开关：只有当 <游戏私有目录>/blfx_diag 存在时才落盘。
// 正式包默认【零写盘】；排查时 `adb shell su -c 'touch .../blfx_diag'` 后再启动即可开启全部取证日志。
#define BLFX_DIAG "/data/data/com.muyegame.blfx/files/blfx_diag"
static bool DiagOn();

// 官方 libil2cpp.so dynsym 实测（st_value）：
//   il2cpp_field_static_get_value = 0x2120f68
//   il2cpp_field_static_set_value = 0x2120f6c   ⇒ 同库相邻，delta = +4
#define BLFX_SYMDELTA_STATIC_SET 4

// ============================================================
// ① 状态
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<int>  g_dmgMul{1};
static std::atomic<bool> g_inv{false};
static std::atomic<int>  g_speed{1};

/* 构建门禁锚点 + 目标类名清单。必须真实存在于二进制（勿删、勿仅留在注释里）；
 * TryInit 里会逐条落盘，防止链接器把它优化掉。 */
static const char* const kAnchorNames[] = {
    "zbMonster", "zbCombatDefense", "zbCombatSpeed", "zbSafeLong",
    "zbCheatContext", "zbHero", "zbAttackerBase", "ChangeHp",
    "blfx_probe.txt", "SetSpeed", "curHp", "maxHp",
};
static const size_t kAnchorCount = sizeof(kAnchorNames) / sizeof(kAnchorNames[0]);

// ============================================================
// ② 落盘日志（PD，逐行 flush）+ 工具
// ============================================================
static void PD(const char* fmt, ...) {
    if (!DiagOn()) return;              // 正式包默认不写盘（见 BLFX_DIAG）
    FILE* f = fopen(BLFX_DUMP, "a");
    if (!f) return;
    va_list ap;
    va_start(ap, fmt);
    vfprintf(f, fmt, ap);
    va_end(ap);
    fputc('\n', f);
    fclose(f);
}

static bool AddrInExecMap(void* addr) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool found = false;
    const uintptr_t a = (uintptr_t)addr;
    while (fgets(line, sizeof(line), fp)) {
        unsigned long s = 0, e = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &s, &e, perm, &off) != 4) continue;
        if (a >= s && a < e) { found = strchr(perm, 'x') != nullptr; break; }
    }
    fclose(fp);
    return found;
}

static void DumpMapsFor(const char* needle) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return;
    char line[768];
    int n = 0;
    unsigned long total = 0;
    while (fgets(line, sizeof(line), fp) && n < 14) {
        if (!strstr(line, needle)) continue;
        PD("MAP %s", line);
        unsigned long s = 0, e = 0;
        if (sscanf(line, "%lx-%lx", &s, &e) == 2 && e > s) total += (e - s);
        n++;
    }
    fclose(fp);
    PD("MAP-TOTAL %s = %lu bytes (shown %d lines)", needle, total, n);
}

static int64_t NowMs() {
    struct timespec ts{};
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (int64_t)ts.tv_sec * 1000 + ts.tv_nsec / 1000000;
}

// 诊断开关（5 秒缓存一次 stat，不热路径 I/O）
static bool DiagOn() {
    static int64_t s_last = -1;
    static bool    s_on   = false;
    const int64_t  now    = NowMs();
    if (s_last < 0 || now - s_last > 5000) {
        s_last = now;
        s_on   = (::access(BLFX_DIAG, F_OK) == 0);
    }
    return s_on;
}

// zbSafeLong 视图（40 B）
struct SafeLongView { int64_t obf, key, sob, skey, fname; };

// ============================================================
// ③ il2cpp 符号补充（Api 未封装的）
// ============================================================
typedef void (*field_static_set_value_t)(void*, void*);
static field_static_set_value_t g_fieldStaticSet = nullptr;

// ============================================================
// ④ 目标全局
// ============================================================
static void* g_clsMonster = nullptr;   // zbMonster
static void* g_clsDefense = nullptr;   // zbCombatDefense
static void* g_clsSpeed   = nullptr;   // zbCombatSpeed
static void* g_clsTime    = nullptr;   // UnityEngine.Time

static void* g_fSpeed1 = nullptr;      // FieldInfo*
static void* g_fSpeed2 = nullptr;
static void* g_fSpeed3 = nullptr;

static void* g_miMonsterChangeHp = nullptr;
static void* g_miDefenseChangeHp = nullptr;
static void* g_miSetTimeScale    = nullptr;

static size_t g_offDefenseCurHp = 0;

static std::atomic<bool> g_resolved{false};
static std::atomic<bool> g_hookTried{false};
static std::atomic<int>  g_hitsMonster{0};
static std::atomic<int>  g_hitsDefense{0};

// ============================================================
// ⑤ 解析（只走 L1/L2；内存扫描已证实会卡死游戏，永久移除）
// ============================================================
static bool ClassHasMember(void* klass, const char* mName, int argc) {
    if (!klass) return false;
    bool ok = false;
    Il2Cpp::SafeProbe([&] {
        auto& api = GetApi();
        if (api.class_method) {
            void* mi = api.class_method(klass, mName, argc);
            if (mi) { ok = true; return true; }
        }
        if (api.class_get_field) {
            void* fi = api.class_get_field(klass, mName);
            if (fi) { ok = true; return true; }
        }
        return true;
    });
    return ok;
}

static const char* kHotfixAsmNames[] = {
    "Game.dll", "Hotfix.dll", "HotFix.dll", "Assembly-CSharp.dll", "Main.dll",
    "Logic.dll", "scriptdll.dll", "Script.dll", "Blade.dll", "blfx.dll",
    "ZbGame.dll", "GameLogic.dll", "Battle.dll", "HotfixGame.dll", "Assembly-CSharp",
};

static void* ResolveClass(const char* name, const char* memberName, int argc) {
    void* found = nullptr;
    Il2Cpp::SafeProbe([&] {
        auto& api = GetApi();
        void* domain = api.Domain();
        if (!domain) return true;

        size_t n = 0;
        void** asms = api.domain_assemblies ? api.domain_assemblies(domain, &n) : nullptr;
        static size_t s_lastCount = 0;
        if (n != s_lastCount) {
            s_lastCount = n;
            PD("L1 images count=%zu (hotfix loaded=%d)", n, n > 20 ? 1 : 0);
        }
        for (size_t i = 0; i < n && !found; ++i) {
            void* img = (void*)(api.assembly_image ? api.assembly_image(asms[i]) : nullptr);
            if (!img || !api.class_from_name) continue;
            void* k = api.class_from_name(img, "", name);
            if (k && (!memberName || ClassHasMember(k, memberName, argc))) {
                PD("L1-HIT %s klass=%p via image[%zu]", name, k, i);
                found = k;
            }
        }
        // L2：按名 open 热更程序集
        if (!found && api.domain_assembly_open && api.class_from_name) {
            for (const char* an : kHotfixAsmNames) {
                void* asm_ = api.domain_assembly_open(domain, an);
                if (!asm_) continue;
                void* img = (void*)(api.assembly_image ? api.assembly_image(asm_) : nullptr);
                if (!img) continue;
                void* k = api.class_from_name(img, "", name);
                if (k && (!memberName || ClassHasMember(k, memberName, argc))) {
                    PD("L2-HIT %s klass=%p via asm=%s", name, k, an);
                    found = k;
                    break;
                }
            }
        }
        return true;
    });
    return found;
}

// ============================================================
// ⑥ 解释器桩 hook —— 本版仅作观测（不改实参）
// ------------------------------------------------------------
// v2 实测：zbMonster.ChangeHp / zbCombatDefense.ChangeHp 的
// MethodInfo->methodPointer 完全相同（共享惰性桩），DobbyHook rc=0
// 但 60 s 内 0 命中 ⇒ 需要 (a) 动态重挂（惰性桩首次调用后被改写）
//              (b) 无条件记录（旧版只在槽位匹配时记录 ⇒ 盲区）
typedef int64_t (*WideBridge_t)(void*, float, void*, void*, void*, void*, void*, void*);

struct BlfxHookSlot {
    void*        mi;
    const char*  name;
    WideBridge_t orig;
    void*        hookedPtr;
    int          rehooks;
};

static BlfxHookSlot g_slotMon{nullptr, "zbMonster.ChangeHp",      nullptr, nullptr, 0};
static BlfxHookSlot g_slotDef{nullptr, "zbCombatDefense.ChangeHp", nullptr, nullptr, 0};
static WideBridge_t g_origShared = nullptr;   // 首个 hook 的 trampoline（共享桩真身）
static std::atomic<int> g_hitsUnknown{0};

static int64_t Hook_Bridge(void* self, float s0, void* x1, void* x2, void* x3,
                           void* x4, void* x5, void* x6) {
    if (g_slotMon.mi && x6 == g_slotMon.mi) {
        if (g_hitsMonster.fetch_add(1) < 24) {
            PD("CALL[mon] self=%p hero=%p def=%p val=%lld mi=%p",
               self, x1, x2, (long long)(intptr_t)x3, x6);
        }
        return g_slotMon.orig ? g_slotMon.orig(self, s0, x1, x2, x3, x4, x5, x6)
                              : (g_origShared ? g_origShared(self, s0, x1, x2, x3, x4, x5, x6) : 0);
    }
    if (g_slotDef.mi && x2 == g_slotDef.mi) {
        if (g_hitsDefense.fetch_add(1) < 24) {
            PD("CALL[def] self=%p px=%p val=%lld mi=%p",
               self, (void*)(intptr_t)s0, (long long)(intptr_t)x1, x2);
        }
        return g_slotDef.orig ? g_slotDef.orig(self, s0, x1, x2, x3, x4, x5, x6)
                              : (g_origShared ? g_origShared(self, s0, x1, x2, x3, x4, x5, x6) : 0);
    }
    if (g_hitsUnknown.fetch_add(1) < 24) {
        PD("CALL[?] self=%p s0=%p x1=%p x2=%p x3=%p x4=%p x5=%p x6=%p",
           self, (void*)(intptr_t)s0, x1, x2, x3, x4, x5, x6);
    }
    return g_origShared ? g_origShared(self, s0, x1, x2, x3, x4, x5, x6) : 0;
}

static void HookSlotTick(BlfxHookSlot& s) {
    if (!s.mi) return;
    void* p = *(void**)s.mi;
    if (!p || p == s.hookedPtr || s.rehooks >= 12) return;
    if (!AddrInExecMap(p)) {
        PD("HOOK %s ptr=%p not-exec skip", s.name, p);
        s.rehooks++;
        return;
    }
    void* orig = nullptr;
    const int rc = DobbyHook(p, (void*)Hook_Bridge, &orig);
    s.rehooks++;
    if (rc == 0 && orig) {
        s.orig = (WideBridge_t)orig;
        if (!g_origShared) g_origShared = (WideBridge_t)orig;
        s.hookedPtr = p;
        PD("HOOK %s ptr=%p rc=0 orig=%p (#%d)", s.name, p, orig, s.rehooks);
    } else {
        s.hookedPtr = p;
        PD("HOOK %s ptr=%p rc=%d FAIL", s.name, p, rc);
    }
}

static void InstallHookOnce() {
    if (g_hookTried.exchange(true)) return;
    auto& api = GetApi();
    if (g_clsMonster && api.class_method)
        g_miMonsterChangeHp = api.class_method(g_clsMonster, "ChangeHp", 5);
    if (g_clsDefense && api.class_method)
        g_miDefenseChangeHp = api.class_method(g_clsDefense, "ChangeHp", 2);
    g_slotMon.mi = g_miMonsterChangeHp;
    g_slotDef.mi = g_miDefenseChangeHp;
    void* fpMon = g_miMonsterChangeHp ? *(void**)g_miMonsterChangeHp : nullptr;
    void* fpDef = g_miDefenseChangeHp ? *(void**)g_miDefenseChangeHp : nullptr;
    PD("MI monster=%p ptr=%p | defense=%p ptr=%p | shared=%d",
       g_miMonsterChangeHp, fpMon, g_miDefenseChangeHp, fpDef,
       (int)(fpMon && fpMon == fpDef));
}

// ============================================================
// ⑦ 字段路线（无 hook，功能主力）
// ------------------------------------------------------------
// 对象图（已从反编译源码确证）：
//   zbCombatSystem.sInstance (static)      — zbCombatSystem.cs:12
//     .combatData                          — zbCombatSystem.cs:30
//       .defense   (zbCombatDefense)       — zbCombatData.cs:46
//       .monsters  (zbCombatMonster)       — zbCombatData.cs:44
//         .ActiveList (List<zbMonster>)    — zbCombatMonster.cs:14
// HP 都是内联 struct zbSafeLong（40 B，双影子），必须双副本同写。
//   _obfValue@0 _key@8 _shadowObf@16 _shadowKey@24 _fieldName@32
// ============================================================
#define BLFX_LIST_ITEMS 0x10   // List<T>._items
#define BLFX_LIST_SIZE  0x18   // List<T>._size
#define BLFX_ARR_DATA   0x20   // Il2CppArray 首个元素

static void*  g_clsCombatSystem  = nullptr;
static void*  g_clsCombatData    = nullptr;
static void*  g_clsCombatMonster = nullptr;

static size_t g_offCombatData = 0, g_offDefense = 0, g_offMonsters = 0, g_offActiveList = 0;
static size_t g_offDefCurHp = 0, g_offDefMaxHp = 0, g_offMonCurHp = 0, g_offMonMaxHp = 0;

static std::atomic<bool> g_fieldOk{false};

static inline int64_t RdI64(const void* base, size_t off) {
    return *reinterpret_cast<const int64_t*>(static_cast<const char*>(base) + off);
}
static inline int32_t RdI32(const void* base, size_t off) {
    return *reinterpret_cast<const int32_t*>(static_cast<const char*>(base) + off);
}
static inline void WrI64(void* base, size_t off, int64_t v) {
    *reinterpret_cast<int64_t*>(static_cast<char*>(base) + off) = v;
}
static inline void* RdPtr(const void* base, size_t off) {
    if (!base || !off) return nullptr;
    return *reinterpret_cast<void* const*>(static_cast<const char*>(base) + off);
}
static inline int64_t SlRead(const void* p) { return RdI64(p, 0) ^ RdI64(p, 8); }
static inline void SlWrite(void* p, int64_t v) {
    const int64_t k  = RdI64(p, 8);
    const int64_t sk = RdI64(p, 24);
    WrI64(p, 0,  v ^ k);      // _obfValue
    WrI64(p, 16, v ^ sk);     // _shadowObf  —— 双影子必须一致，否则 zbSafeLong 自己 ReportCheat
}
static bool ClassOfIs(void* obj, void* klass) {
    auto& api = GetApi();
    if (!obj || !klass || !api.object_class) return false;
    return api.object_class(obj) == klass;
}

struct BlfxChain { void* sys; void* data; void* def; void* mons; void* list; };

static bool ResolveChain(BlfxChain& c) {
    auto& api = GetApi();
    c = BlfxChain{};
    if (!g_clsCombatSystem || !g_offCombatData) return false;
    c.sys = api.StaticRefField(g_clsCombatSystem, "sInstance");
    if (!ClassOfIs(c.sys, g_clsCombatSystem)) return false;
    c.data = RdPtr(c.sys, g_offCombatData);
    if (!ClassOfIs(c.data, g_clsCombatData)) return false;
    if (g_offDefense) c.def = RdPtr(c.data, g_offDefense);
    if (g_offMonsters) {
        c.mons = RdPtr(c.data, g_offMonsters);
        if (c.mons && g_offActiveList) c.list = RdPtr(c.mons, g_offActiveList);
    }
    return true;
}

// 无敌：防线 HP 钉在 maxHp
// 伤害倍率：把每只活着的怪 curHp 压到 maxHp/mul ⇒ 有效血量 1/mul，等效 N 倍伤害
static void FieldApplyTick() {
    static int s_logCount = 0;
    static int64_t s_lastLog = 0;

    if (!g_fieldOk.load()) return;
    const int  mul = g_dmgMul.load();
    const bool inv = g_inv.load();
    if (mul <= 1 && !inv) return;

    BlfxChain c{};
    if (!ResolveChain(c)) return;

    int64_t dcur = 0, dmax = 0;
    if (inv && c.def && g_offDefCurHp && g_offDefMaxHp) {
        void* pc = static_cast<char*>(c.def) + g_offDefCurHp;
        void* pm = static_cast<char*>(c.def) + g_offDefMaxHp;
        dmax = SlRead(pm);
        dcur = SlRead(pc);
        if (dmax > 0 && dcur < dmax) SlWrite(pc, dmax);
    }

    int nMon = 0, nMod = 0;
    void* m0 = nullptr; int64_t m0c = 0, m0m = 0;
    if (mul > 1 && c.list) {
        void* items = RdPtr(c.list, BLFX_LIST_ITEMS);
        const int size = RdI32(c.list, BLFX_LIST_SIZE);
        const int cap  = items ? RdI32(items, BLFX_LIST_SIZE) : 0;
        if (items && size > 0 && size <= cap && size < 4096) {
            void** arr = reinterpret_cast<void**>(static_cast<char*>(items) + BLFX_ARR_DATA);
            for (int i = 0; i < size; ++i) {
                void* m = arr[i];
                if (!m) continue;
                ++nMon;
                void* pc = static_cast<char*>(m) + g_offMonCurHp;
                void* pm = static_cast<char*>(m) + g_offMonMaxHp;
                const int64_t mx = SlRead(pm), cur = SlRead(pc);
                if (i == 0) { m0 = m; m0c = cur; m0m = mx; }
                if (mx <= 0 || cur <= 0) continue;
                int64_t capHp = mx / mul;
                if (capHp < 1) capHp = 1;
                if (cur > capHp) { SlWrite(pc, capHp); ++nMod; }
            }
        }
    }

    const int64_t now = NowMs();
    if (s_logCount < 40 || now - s_lastLog > 10000) {
        ++s_logCount;
        s_lastLog = now;
        PD("FAPPLY inv=%d mul=%d sys=%p data=%p def=%p dcur=%lld dmax=%lld list=%p n=%d mod=%d m0=%p m0c=%lld m0m=%lld",
           (int)inv, mul, c.sys, c.data, c.def, dcur, dmax, c.list, nMon, nMod, m0, m0c, m0m);
    }
}

// ============================================================
// ⑦.9 反作弊取证：读 zbSelf.Instance.combatHandler.outString
//   csout/zbCombatHandler.cs:35  private string outString = "";
//   csout/zbCombatHandler.cs:333+ IsZuobi() 开头写 "UUID:...,battleFuncType=" 再按触发项追加
//   （"monsterHp:" / "选择MOD 超过上限:" / "heroAttr:" / "teamAttack:" / "attrItem:" /
//     "teamAttrTable:" / "时间加速:" / 各种 param 总量）
//   全游戏只有 3 处调用 IsZuobi()，其中 2 处会弹 [lang.10668+"_6"]：
//     csout/zbModSelectWnd.cs:387 / csout/zbEndlessHordeFightForwardWnd.cs:400 / csout/zbCombatState_Balance.cs:39
//   ⇒ 把 outString 打出来即可确证是哪个检查在报。
// ============================================================
static void*  g_clsSelf            = nullptr;
static void*  g_clsHandler         = nullptr;
static size_t g_offSelfHandler     = 0;
static size_t g_offHandlerOutStr   = 0;
static size_t g_offHandlerMonHpTot = 0;
static size_t g_offSysAccumTime    = 0;
static size_t g_offSysStartServer  = 0;

static void Utf16ToAscii(void* s, char* out, int cap) {
    out[0] = 0;
    if (!s || cap < 8) return;
    const int len = RdI32(s, 0x10);          // System.String: length@0x10, chars@0x14
    if (len <= 0 || len > 400) return;
    const uint16_t* ch = reinterpret_cast<const uint16_t*>(static_cast<const char*>(s) + 0x14);
    int n = 0;
    for (int i = 0; i < len && n < cap - 1; ++i) {
        const uint16_t c = ch[i];
        out[n++] = (c >= 32 && c < 127) ? static_cast<char>(c) : '?';
    }
    out[n] = 0;
}

// zbSafeFloat = 内联 struct：_obfValue@0 _key@4 _shadowObf@8 _shadowKey@12 _fieldName@16
static float SlfRead(const void* p) {
    if (!p) return 0.f;
    const uint32_t x = static_cast<uint32_t>(RdI32(p, 0)) ^ static_cast<uint32_t>(RdI32(p, 4));
    float f = 0.f;
    __builtin_memcpy(&f, &x, sizeof(f));
    return f;
}

static void AntiCheatProbe() {
    auto& api = GetApi();
    if (!g_clsSelf || !g_clsHandler || !g_offSelfHandler) return;
    void* self = api.StaticRefField(g_clsSelf, "sInstance");
    if (!ClassOfIs(self, g_clsSelf)) { PD("AC self=null"); return; }
    void* h = RdPtr(self, g_offSelfHandler);
    if (!ClassOfIs(h, g_clsHandler)) { PD("AC handler=null"); return; }

    char buf[256];
    Utf16ToAscii(RdPtr(h, g_offHandlerOutStr), buf, static_cast<int>(sizeof(buf)));
    const int64_t mh = g_offHandlerMonHpTot ? RdI64(h, g_offHandlerMonHpTot) : -1;
    PD("AC out=\"%s\" monHpTotal=%lld", buf, static_cast<long long>(mh));

    void* sys = api.StaticRefField(g_clsCombatSystem, "sInstance");
    if (sys && ClassOfIs(sys, g_clsCombatSystem) && g_offSysAccumTime) {
        const float accum = SlfRead(static_cast<const char*>(sys) + g_offSysAccumTime);
        const int64_t start = g_offSysStartServer
            ? SlRead(static_cast<const char*>(sys) + g_offSysStartServer) : -1;
        float f1 = -1.f, f2 = -1.f, f3 = -1.f;
        if (api.field_static_get && g_fSpeed1) {
            api.field_static_get(g_fSpeed1, &f1);
            if (g_fSpeed2) api.field_static_get(g_fSpeed2, &f2);
            if (g_fSpeed3) api.field_static_get(g_fSpeed3, &f3);
        }
        PD("AC2 accum=%.2f startServer=%lld sp=%.3f/%.3f/%.3f",
           accum, static_cast<long long>(start), f1, f2, f3);
    }
}

// ============================================================
// ⑧ 目标解析
// ============================================================
static void ResolveTargets() {
    auto& api = GetApi();

    // il2cpp_field_static_set_value = get + 4（同库相邻；dlsym 拿不到：TP 重排了 dynsym）
    if (!g_fieldStaticSet && api.field_static_get) {
        g_fieldStaticSet = (field_static_set_value_t)((uintptr_t)api.field_static_get + BLFX_SYMDELTA_STATIC_SET);
        PD("field_static_get=%p -> derived set=%p (delta=%d)",
           (void*)api.field_static_get, (void*)g_fieldStaticSet, BLFX_SYMDELTA_STATIC_SET);
    }

    if (!g_clsMonster) g_clsMonster = ResolveClass("zbMonster", "ChangeHp", 5);
    if (!g_clsDefense) g_clsDefense = ResolveClass("zbCombatDefense", "ChangeHp", 2);
    if (!g_clsSpeed)   g_clsSpeed   = ResolveClass("zbCombatSpeed", "speed1", 0);

    if (g_clsSpeed && !g_fSpeed1 && api.class_get_field) {
        g_fSpeed1 = api.class_get_field(g_clsSpeed, "speed1");
        g_fSpeed2 = api.class_get_field(g_clsSpeed, "speed2");
        g_fSpeed3 = api.class_get_field(g_clsSpeed, "speed3");
        PD("SPEEDFIELDS f1=%p f2=%p f3=%p", g_fSpeed1, g_fSpeed2, g_fSpeed3);
    }

    // ── 字段路线目标链 ──
    if (!g_clsCombatSystem)  g_clsCombatSystem  = ResolveClass("zbCombatSystem", "combatData", 0);
    if (!g_clsCombatData)    g_clsCombatData    = ResolveClass("zbCombatData", "defense", 0);
    if (!g_clsCombatMonster) g_clsCombatMonster = ResolveClass("zbCombatMonster", "monsterList", 0);

    if (g_clsCombatSystem && !g_offCombatData) {
        g_offCombatData = api.FieldOffset(g_clsCombatSystem, "combatData", 0);
        if (g_clsCombatData) {
            g_offDefense  = api.FieldOffset(g_clsCombatData, "defense", 0);
            g_offMonsters = api.FieldOffset(g_clsCombatData, "monsters", 0);
        }
        if (g_clsCombatMonster)
            g_offActiveList = api.FieldOffset(g_clsCombatMonster, "ActiveList", 0);
        if (g_clsDefense) {
            g_offDefCurHp = api.FieldOffset(g_clsDefense, "curHp", 0);
            g_offDefMaxHp = api.FieldOffset(g_clsDefense, "maxHp", 0);
        }
        if (g_clsMonster) {
            g_offMonCurHp = api.FieldOffset(g_clsMonster, "curHp", 0);
            g_offMonMaxHp = api.FieldOffset(g_clsMonster, "maxHp", 0);
        }
        PD("OFFSETS data=%zu def=%zu mons=%zu list=%zu | dcur=%zu dmax=%zu mcur=%zu mmax=%zu",
           g_offCombatData, g_offDefense, g_offMonsters, g_offActiveList,
           g_offDefCurHp, g_offDefMaxHp, g_offMonCurHp, g_offMonMaxHp);
        const bool ok = g_offCombatData && g_offDefense && g_offMonsters && g_offActiveList &&
                        g_offDefCurHp && g_offDefMaxHp && g_offMonCurHp && g_offMonMaxHp;
        g_fieldOk.store(ok);
        PD("FIELD route ready=%d", (int)ok);
        g_resolved.store(true);
    }

    // UnityEngine.Time（AOT，timeScale 心跳兜底）
    if (!g_clsTime && api.class_from_name) {
        size_t n = 0;
        void** asms = api.domain_assemblies ? api.domain_assemblies(api.Domain(), &n) : nullptr;
        for (size_t i = 0; asms && i < n && !g_clsTime; ++i) {
            void* img = (void*)(api.assembly_image ? api.assembly_image(asms[i]) : nullptr);
            if (!img) continue;
            void* t = api.class_from_name(img, "UnityEngine", "Time");
            if (t) g_clsTime = t;
        }
        if (g_clsTime && api.class_method) {
            g_miSetTimeScale = api.class_method(g_clsTime, "set_timeScale", 1);
            void* fp = g_miSetTimeScale ? *(void**)g_miSetTimeScale : nullptr;
            PD("Time.timeScale setter mi=%p ptr=%p exec=%d", g_miSetTimeScale, fp,
               (int)AddrInExecMap(fp));
        }
    }

    // ── 反作弊取证目标（zbSelf.sInstance.combatHandler.outString）──
    if (!g_clsSelf)    g_clsSelf    = ResolveClass("zbSelf", "combatHandler", 0);
    if (!g_clsHandler) g_clsHandler = ResolveClass("zbCombatHandler", "outString", 0);
    if (g_clsSelf && g_clsHandler && !g_offSelfHandler) {
        g_offSelfHandler     = api.FieldOffset(g_clsSelf, "combatHandler", 0);
        g_offHandlerOutStr   = api.FieldOffset(g_clsHandler, "outString", 0);
        g_offHandlerMonHpTot = api.FieldOffset(g_clsHandler, "battleMonsterHpTotal", 0);
        if (g_clsCombatSystem) {
            g_offSysAccumTime   = api.FieldOffset(g_clsCombatSystem, "accumTime", 0);
            g_offSysStartServer = api.FieldOffset(g_clsCombatSystem, "startServerTime", 0);
        }
        PD("ACOFF self=%p handler=%p | self.handler=%zu outString=%zu monHpTotal=%zu accum=%zu start=%zu",
           g_clsSelf, g_clsHandler, g_offSelfHandler, g_offHandlerOutStr,
           g_offHandlerMonHpTot, g_offSysAccumTime, g_offSysStartServer);
    }

    if ((g_clsMonster || g_clsDefense || g_clsSpeed) && g_resolved.exchange(true)) {
        LOGI(BLFX_LOG " resolved monster=%p defense=%p speed=%p time=%p",
             g_clsMonster, g_clsDefense, g_clsSpeed, g_clsTime);
        PD("RESOLVED monster=%p defense=%p speed=%p time=%p sf1=%p sf2=%p sf3=%p",
           g_clsMonster, g_clsDefense, g_clsSpeed, g_clsTime, g_fSpeed1, g_fSpeed2, g_fSpeed3);
    }
    InstallHookOnce();
}

// ============================================================
// ⑨ 战斗加速：写 zbCombatSpeed.speed1/2/3 + 一次性 timeScale（含回读验证）
// ============================================================
typedef void (*set_time_scale_t)(float, void*);
static set_time_scale_t g_setTimeScale = nullptr;
static std::atomic<int> g_appliedSpeed{0};

static void ReadbackSpeed(const char* tag) {
    auto& api = GetApi();
    if (!api.field_static_get || !g_fSpeed1) return;
    float a = -1.f, b = -1.f, c = -1.f;
    api.field_static_get(g_fSpeed1, &a);
    if (g_fSpeed2) api.field_static_get(g_fSpeed2, &b);
    if (g_fSpeed3) api.field_static_get(g_fSpeed3, &c);
    PD("SPEEDREAD %s f1=%.3f f2=%.3f f3=%.3f", tag, a, b, c);
}

static void WriteSpeedStatics(int mult) {
    if (!g_fieldStaticSet || !g_fSpeed1) return;
    const float f = (float)mult;
    Il2Cpp::SafeProbe([&] {
        if (g_fSpeed1) g_fieldStaticSet(g_fSpeed1, (void*)&f);
        if (g_fSpeed2) g_fieldStaticSet(g_fSpeed2, (void*)&f);
        if (g_fSpeed3) g_fieldStaticSet(g_fSpeed3, (void*)&f);
        return true;
    });
}

static void ApplySpeed(int mult, bool forceTimeScale) {
    WriteSpeedStatics(mult);
    if (forceTimeScale) {
        if (!g_setTimeScale && g_miSetTimeScale) {
            void* fp = *(void**)g_miSetTimeScale;
            if (fp && AddrInExecMap(fp)) g_setTimeScale = (set_time_scale_t)fp;
        }
        if (g_setTimeScale) {
            Il2Cpp::SafeProbe([&] { g_setTimeScale((float)mult, nullptr); return true; });
        }
    }
    g_appliedSpeed.store(mult);
    ReadbackSpeed("after-write");
}

// ============================================================
// ⑩ 对外接口（ModEntry.h 契约，勿改签名）
// ============================================================
bool Blfx_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(BLFX_LIB)) return false;
    g_libReady.store(true);
    { FILE* f = fopen(BLFX_DUMP, "w"); if (f) fclose(f); }   // 每次冷启动清空探针
    LOGI(BLFX_LOG " lib ready (impl v3)");
    PD("=== blfx impl v3 start ===");

    if (!GetApi().ready) GetApi().Load(BLFX_LIB);

    void* domain = nullptr;
    Il2Cpp::SafeProbe([&] { domain = GetApi().Domain(); return true; });
    if (!domain) {
        Il2Cpp::SafeProbe([&] { GetApi().ReprobeCore(); return true; });
        Il2Cpp::SafeProbe([&] { domain = GetApi().Domain(); return true; });
    }
    PD("api.ready=%d domain=%p", (int)GetApi().ready, domain);
    for (size_t i = 0; i < kAnchorCount; ++i) PD("anchor[%zu]=%s", i, kAnchorNames[i]);
    DumpMapsFor("libil2cpp.so");
    return true;
}

bool Blfx_IsReady() { return g_libReady.load(); }

void Blfx_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == BLFX_FEAT_INV) {
        g_inv.store(enabled);
        LOGI(BLFX_LOG " 无敌 = %d", (int)enabled);
        PD("SET inv=%d", (int)enabled);
    }
}

void Blfx_OnValueChanged(int featNum, int value) {
    if (featNum == BLFX_FEAT_DMG) {
        g_dmgMul.store(value < 1 ? 1 : value);
        LOGI(BLFX_LOG " 伤害倍率 = %d", value);
        PD("SET dmg=%d", value);
    }
    if (featNum == BLFX_FEAT_SPEED) {
        const int v = (value < 1 ? 1 : value);
        g_speed.store(v);
        ApplySpeed(v, true);
        LOGI(BLFX_LOG " 战斗加速 = %d", v);
        PD("SET speed=%d", v);
    }
}

void Blfx_Tick(const bool* features, const int* values) {
    if (features) g_inv.store(features[BLFX_FEAT_INV]);
    if (values) {
        g_dmgMul.store(values[BLFX_FEAT_DMG] < 1 ? 1 : values[BLFX_FEAT_DMG]);
        g_speed.store(values[BLFX_FEAT_SPEED] < 1 ? 1 : values[BLFX_FEAT_SPEED]);
    }
    if (!g_libReady.load()) {
        if (isLibraryLoaded(BLFX_LIB)) {
            g_libReady.store(true);
            if (!GetApi().ready) GetApi().Load(BLFX_LIB);
        } else {
            return;
        }
    }

    const int64_t now = NowMs();

    // 目标解析：前 60 s 每 500 ms，之后每 5 s（直到字段路线就绪）
    static int64_t s_lastResolve = 0;
    static int     s_resolveTries = 0;
    if (!g_fieldOk.load()) {
        const int64_t iv = (s_resolveTries < 120) ? 500 : 5000;
        if (now - s_lastResolve > iv) {
            s_lastResolve = now;
            s_resolveTries++;
            Il2Cpp::SafeProbe([&] { ResolveTargets(); return true; });
        }
    }

    // hook 动态重挂（惰性桩被 HybridCLR 改写后立刻跟上）
    if (g_slotMon.mi || g_slotDef.mi) {
        static int64_t s_lastHook = 0;
        if (now - s_lastHook > 400) {
            s_lastHook = now;
            HookSlotTick(g_slotMon);
            HookSlotTick(g_slotDef);
        }
    }

    // 指针监视（观察 methodPointer 是否被改写）
    static int64_t s_lastPtrLog = 0;
    if (now - s_lastPtrLog > 5000) {
        s_lastPtrLog = now;
        PD("PTRMON mon=%p def=%p hookedMon=%p hookedDef=%p hits=%d/%d/%d",
           g_slotMon.mi ? *(void**)g_slotMon.mi : nullptr,
           g_slotDef.mi ? *(void**)g_slotDef.mi : nullptr,
           g_slotMon.hookedPtr, g_slotDef.hookedPtr,
           g_hitsMonster.load(), g_hitsDefense.load(), g_hitsUnknown.load());
    }

    // 功能生效：字段路线（每 tick）
    Il2Cpp::SafeProbe([&] { FieldApplyTick(); return true; });

    // 反作弊取证：1 Hz 打印 IsZuobi 判定理由（outString）
    static int64_t s_lastAc = 0;
    if (now - s_lastAc > 1000) {
        s_lastAc = now;
        Il2Cpp::SafeProbe([&] { AntiCheatProbe(); return true; });
    }

    // 速度静态字段维持（秒级重写，防游戏自身复位；不反复动 timeScale）
    static int64_t s_lastSpeed = 0;
    const int sp = g_speed.load();
    if (sp > 1 && now - s_lastSpeed > 1000) {
        s_lastSpeed = now;
        WriteSpeedStatics(sp);
        if (g_appliedSpeed.load() == 0) ApplySpeed(sp, true);   // 冷启动补一次
    }
}

void Blfx_ForceClear() {
    g_dmgMul.store(1);
    g_inv.store(false);
    const int prev = g_speed.exchange(1);
    if (prev != 1) ApplySpeed(1, true);
}
