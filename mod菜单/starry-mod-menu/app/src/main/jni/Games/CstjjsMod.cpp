/* ================================================================
 *  长生：天机降世 1.0.2 / vc1020
 *
 *  0.9.9 壳挂点 ABI：x1=MethodInfo, x2=args[+0]=CBRole* [+8]=int64 delta
 *  那不是 Execute 入口，是 HybridCLR InterpFrameGroup::EnterFrameFromInterpreter：
 *    x0=this, x1=MethodInfo, x2=StackObject* argBase
 *    源码：AllocStackSlot(maxStack - argStackObjectSize)，不拷参数
 *  C#→C# 走 CallInterp → 本函数；独桩/Execute 只承接 native→C#，战斗扣血看不见。
 *
 *  1.0.2 磁盘（真机官方 base.apk）：
 *    EnterFrameFromInterpreter  file 0x2657350（VA 0x265b350）
 *    Interpreter::Execute       VA 0x265d1a8（已否：interp live 但 Health 名 0）
 *    历史：0.9.10 file 0x2656920 / 1.0.1 file 0x2657084；仅证明同一语义链，勿再沿用。
 *
 *  过滤只改 ET.CBRoleValueSystem.RoleValue_Health（0.9.9 已验）。
 *  禁止 ELF base+RVA；禁止再钩独桩 / Execute（会漏或双改）。
 * ================================================================ */

#include "CstjjsMod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;
using Il2Cpp::IsValidHeapPtr;

enum : uintptr_t {
    FILE_ENTER_INTERP = 0x2657350,  // 1.0.2 EnterFrameFromInterpreter（VA 0x265b350）
};
enum : size_t {
    OFF_ROLE_SIDE_FALLBACK = 0x74,
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_dmg10{false};
static std::atomic<bool> g_inv{false};
static std::atomic<uint32_t> g_hits{0};
static std::atomic<uint32_t> g_trace{0};
static std::atomic<uint32_t> g_see{0};
static std::atomic<uint32_t> g_resolveLog{0};
static std::atomic<uint32_t> g_tickN{0};
static size_t g_offRoleSide = OFF_ROLE_SIDE_FALLBACK;
static void* g_miHealth = nullptr;

typedef const char* (*method_get_name_t)(void* method);
typedef void* (*method_get_class_t)(void* method);
typedef const char* (*class_get_name_t)(void* klass);
static method_get_name_t g_methodGetName = nullptr;
static method_get_class_t g_methodGetClass = nullptr;
static class_get_name_t g_classGetName = nullptr;

typedef void* (*EnterInterp_t)(void* self, void* methodInfo, void* argsPkt);
static EnterInterp_t g_origEnterInterp = nullptr;

static bool EnsureSyms() {
    if (g_methodGetName) return true;
    void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
    if (!h) return false;
    g_methodGetName = (method_get_name_t)dlsym(h, "il2cpp_method_get_name");
    g_methodGetClass = (method_get_class_t)dlsym(h, "il2cpp_method_get_class");
    g_classGetName = (class_get_name_t)dlsym(h, "il2cpp_class_get_name");
    return g_methodGetName != nullptr;
}

/** file_off → 运行时 VA。必须认 r-xp 和 HybridCLR 的 rwxp（perm 含 x）。 */
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (end <= start) continue;
        if (fileOff >= off && fileOff < off + (end - start)) {
            found = (void*)(start + (fileOff - off));
            break;
        }
    }
    fclose(fp);
    return found;
}

static const char* MiName(void* mi) {
    if (!mi || !g_methodGetName) return "";
    const char* n = g_methodGetName(mi);
    return n ? n : "";
}

static const char* MiClass(void* mi) {
    if (!mi || !g_methodGetClass || !g_classGetName) return "";
    void* k = g_methodGetClass(mi);
    if (!k) return "";
    const char* n = g_classGetName(k);
    return n ? n : "";
}

static bool NameIsHealth(const char* n) {
    return n && n[0] && strcmp(n, "RoleValue_Health") == 0;
}

enum : uint8_t { KIND_EMPTY = 0, KIND_SKIP = 1, KIND_HEALTH = 2 };
struct MiCacheEnt {
    void* mi;
    uint8_t kind;
};
static MiCacheEnt g_miCache[4096];

static uint8_t KindOf(void* mi) {
    if (!mi) return KIND_SKIP;
    if (mi == g_miHealth) return KIND_HEALTH;
    uintptr_t h = (reinterpret_cast<uintptr_t>(mi) >> 4) & 4095u;
    for (int p = 0; p < 12; p++) {
        MiCacheEnt& e = g_miCache[(h + static_cast<uintptr_t>(p)) & 4095u];
        if (e.mi == mi) return e.kind;
        if (!e.mi) {
            const char* n = MiName(mi);
            uint8_t k = NameIsHealth(n) ? KIND_HEALTH : KIND_SKIP;
            e.mi = mi;
            e.kind = k;
            if (k == KIND_HEALTH) {
                uint32_t s = g_see.fetch_add(1);
                if (s < 8)
                    LOGI("[Cstjjs] see %s.%s", MiClass(mi), n);
            }
            return k;
        }
    }
    return NameIsHealth(MiName(mi)) ? KIND_HEALTH : KIND_SKIP;
}

static int32_t RoleSideOf(void* role) {
    if (!role || !IsValidHeapPtr(role)) return 0;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(role) + g_offRoleSide);
}

static int64_t PatchDelta(void* role, int64_t delta) {
    if (!(g_dmg10.load() || g_inv.load())) return delta;
    if (!(delta < 0 && delta > -100000000)) return delta;
    int32_t side = RoleSideOf(role);
    int64_t nd = delta;
    if (g_dmg10.load() && side == 2) nd = delta * 10;
    if (g_inv.load() && side == 1) nd = 0;
    if (nd != delta) {
        uint32_t n = g_hits.fetch_add(1) + 1;
        LOGI("[Cstjjs] patch Health n=%u side=%d %lld -> %lld", n, side, (long long)delta,
             (long long)nd);
    }
    return nd;
}

static void PatchHealthArgs(void* argsPkt) {
    if (!argsPkt) return;
    void* role = *reinterpret_cast<void**>(argsPkt);
    int64_t delta = *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + 8);
    int64_t nd = PatchDelta(role, delta);
    if (nd != delta)
        *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(argsPkt) + 8) = nd;
}

static void* Hooked_EnterInterp(void* self, void* methodInfo, void* argsPkt) {
    if (g_dmg10.load() || g_inv.load()) {
        if (KindOf(methodInfo) == KIND_HEALTH) {
            uint32_t t = g_trace.fetch_add(1);
            if (t < 12)
                LOGI("[Cstjjs] enter-interp #%u %s.%s pkt=%p", t, MiClass(methodInfo),
                     MiName(methodInfo), argsPkt);
            PatchHealthArgs(argsPkt);
        }
    }
    return g_origEnterInterp ? g_origEnterInterp(self, methodInfo, argsPkt) : nullptr;
}

static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

static void* GetMethodAny(void* klass, const char* name) {
    auto& api = GetApi();
    static const int kArgc[] = {2, -1, 1, 3, 0};
    for (int a : kArgc) {
        void* mi = api.GetMethod(klass, name, a);
        if (mi) return mi;
    }
    return nullptr;
}

static void TryResolve() {
    auto& api = GetApi();
    if (!api.ready && !api.Load(GameUnity::TARGET_LIB)) return;
    void* klass = FindClassNs("ET", "CBRoleValueSystem");
    if (!klass) klass = FindClassNs("", "CBRoleValueSystem");
    if (klass) {
        if (!g_miHealth) g_miHealth = GetMethodAny(klass, "RoleValue_Health");
        if (g_miHealth && g_resolveLog.exchange(1) == 0)
            LOGI("[Cstjjs] MI ready miH=%p", g_miHealth);
    }
    void* roleKlass = FindClassNs("ET", "CBRole");
    if (!roleKlass) roleKlass = FindClassNs("", "CBRole");
    if (roleKlass) {
        size_t off = api.FieldOffset(roleKlass, "<RoleSide>k__BackingField",
                                     GameUnity::HYBRIDCLR_FIELD_BIAS);
        if (!off) off = api.FieldOffset(roleKlass, "RoleSide", GameUnity::HYBRIDCLR_FIELD_BIAS);
        if (off) g_offRoleSide = off;
    }
}

static bool HookPtr(void* addr, void* repl, void** orig, const char* tag) {
    if (!addr) {
        LOGW("[Cstjjs] %s addr null", tag);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, repl, orig) != 0) {
        LOGE("[Cstjjs] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Cstjjs] hooked %s @ %p", tag, addr);
    return true;
}

bool Cstjjs_TryInit() {
    if (g_ready.load()) {
        if (!g_miHealth) TryResolve();
        return true;
    }
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    EnsureSyms();
    // TryResolve 不在此调用：il2cpp_init 可能未完成（1.0.1 SIGSEGV 实锤），交给 Tick 门控重试

    void* enter = FileOffToVa(sec_lib_il2cpp(), FILE_ENTER_INTERP);
    LOGI("[Cstjjs] enter-interp maps VA=%p file=0x%lX miH=%p", enter,
         (unsigned long)FILE_ENTER_INTERP, g_miHealth);
    if (!HookPtr(enter, (void*)Hooked_EnterInterp, (void**)&g_origEnterInterp, "enter-interp"))
        return false;

    if (!g_miHealth)
        LOGI("[Cstjjs] Health MI not ready (Code.dll), Tick 会补解析");

    g_ready.store(true);
    LOGI("[Cstjjs] 初始化成功 RoleSide=0x%zx miH=%p", g_offRoleSide, g_miHealth);
    return true;
}

bool Cstjjs_IsReady() { return g_ready.load(); }

void Cstjjs_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case CSTJJS_FEAT_DMG10: g_dmg10.store(enabled); break;
        case CSTJJS_FEAT_INVULN: g_inv.store(enabled); break;
        default: break;
    }
    LOGI("[Cstjjs] feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Cstjjs_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}

void Cstjjs_Tick(const bool* features) {
    uint32_t tn = g_tickN.fetch_add(1);
    if (!g_miHealth) {
        TryResolve();
        if ((tn % 25) == 0)
            LOGI("[Cstjjs] waiting MI Code.dll tick=%u", tn);
    }
    if (!features) return;
    g_dmg10.store(features[CSTJJS_FEAT_DMG10]);
    g_inv.store(features[CSTJJS_FEAT_INVULN]);
}

void Cstjjs_ForceClear() {
    g_dmg10.store(false);
    g_inv.store(false);
    LOGI("[Cstjjs] ForceClear");
}
