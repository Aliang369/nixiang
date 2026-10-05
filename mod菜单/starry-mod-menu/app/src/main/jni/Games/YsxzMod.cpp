/* ================================================================
 *  勇士之心 MOD - se::ScriptEngine::evalString splice
 *
 *  目标：TapTap com.fightingstars.games.warrior 1.0.4 / vc5
 *  引擎：Cocos Creator 2.4.15 / libcocos2djs.so
 *
 *  本包与斌哥包的核心资产一致：
 *    - libcocos2djs.so / assets/main.js / assets/src/cocos2d-jsb.js
 *    - assets/assets/main/index.js
 *  因此直接复用斌哥自愈式 JS 注入体，只把 MOD.featureN 的来源替换为
 *  Starry YsxzHooks.getRet(n)。
 *
 *  se::ScriptEngine::evalString 在本包被 strip，按静态 RVA 0x3138e4 定位，
 *  序言校验 0xD10543FF；不匹配则拒绝挂载并等待后续 Tick 重试。
 * ================================================================ */

#include "YsxzMod.h"
#include "YsxzPatchJs.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <string>
#include <pthread.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Games/YsxzSecShell.h"
#include "Games/YsxzExitTrace.h"

#define YSXZ_LOG "[Ysxz]"

static const char* kTargetLib() { return sec_lib_cocosjs(); }

// 已知静态 RVA 回退表（新→旧）；仅当 JNI 桥自适应定位失败时才用
static constexpr uintptr_t kEvalRvaFallbacks[] = {0x3152dc, 0x3138e4};
static constexpr uint32_t kEvalInsn = 0xD10543FFu;  // sub sp, sp, #0x150 序言
static constexpr long kBundleMinLen = 200L * 1024L;
static constexpr long kBundleMaxLen = 64L * 1024L * 1024L;

static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<int> g_spliceHits{0};
static std::atomic<bool> g_feat[YSXZ_FEAT_COUNT];
static std::atomic<int> g_val[YSXZ_FEAT_COUNT];
static thread_local bool t_inSplice = false;

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t g_origSeEval = nullptr;

static bool FindBytes(const char* buf, size_t len, const char* needle) {
    if (!buf || !needle) return false;
    const size_t n = strlen(needle);
    if (n == 0 || len < n) return false;
    for (size_t i = 0; i + n <= len; i++) {
        if (memcmp(buf + i, needle, n) == 0) return true;
    }
    return false;
}

static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

static bool IsTargetBundle(const char* fileName) {
    return fileName && (strstr(fileName, "assets/battleLogic/index.") ||
                        strstr(fileName, "assets/game/index."));
}

// 两轮探测：先 x 段（真机快路径），找不到再退到最大可读段
// （MuMu/Houdini 下 ARM 代码段常为 r--p，无 x 位）。运行时判据，禁机型分叉。
static void* FileOffToVa(const char* lib, uintptr_t fileOff, bool* viaReadable) {
    if (viaReadable) *viaReadable = false;
    for (int pass = 0; pass < 2; pass++) {
        const bool wantExec = (pass == 0);
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
            if (end <= start) continue;
            const bool hasX = strchr(perm, 'x') != nullptr;
            const bool hasR = strchr(perm, 'r') != nullptr;
            if (wantExec ? !hasX : (!hasR || hasX)) continue;
            if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
            const size_t span = static_cast<size_t>(end - start);
            if (span > bestSpan) {
                bestSpan = span;
                found = reinterpret_cast<void*>(start + (fileOff - off));
            }
        }
        fclose(fp);
        if (found) {
            if (!wantExec) {
                LOGI("%s readable-map fallback engaged lib=%s", YSXZ_LOG, lib);
                if (viaReadable) *viaReadable = true;
            }
            return found;
        }
    }
    return nullptr;
}

// ---------------------------------------------------------------
//  自适应定位 se::ScriptEngine::evalString
//    本作 so 被 strip（无 se:: 符号），但导出了 JNI 桥
//    Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString；
//    该 wrapper 内 “bl 目标首字 == 0xD10543FF” 的那一个就是 evalString。
//    1.0.4 实测：wrapper @rva 0x2c3318，bl @0x2c33a4 → evalString @rva 0x3152dc
//    （旧 0x3138e4 已漂移 +0x19F8，序言校验会直接拒绝）。
// ---------------------------------------------------------------
struct SoRange {
    uintptr_t base = 0;
    size_t size = 0;
    bool ok = false;
};

static SoRange GetLoadedRange(const char* lib) {
    SoRange r;
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return r;
    char line[768];
    uintptr_t lo = 0, hi = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0;
        if (sscanf(line, "%lx-%lx", &start, &end) != 2 || end <= start) continue;
        if (!r.ok || start < lo) lo = start;
        if (!r.ok || end > hi) hi = end;
        r.ok = true;
    }
    fclose(fp);
    if (r.ok) {
        r.base = lo;
        r.size = static_cast<size_t>(hi - lo);
    }
    return r;
}

struct YsxzEhdr {
    uint8_t ident[16];
    uint16_t type, machine;
    uint32_t version;
    uint64_t entry, phoff, shoff;
    uint32_t flags;
    uint16_t ehsize, phentsize, phnum, shentsize, shnum, shstrndx;
};
struct YsxzPhdr {
    uint32_t type, flags;
    uint64_t offset, vaddr, paddr, filesz, memsz, align;
};
struct YsxzDyn {
    int64_t tag;
    uint64_t val;
};
struct YsxzSym {
    uint32_t name;
    uint8_t info, other;
    uint16_t shndx;
    uint64_t value, size;
};

static constexpr uint32_t kPtDynamic = 2;
static constexpr int64_t kDtHash = 4, kDtStrtab = 5, kDtSymtab = 6, kDtStrsz = 10;
static constexpr int64_t kDtGnuHash = 0x6ffffef5ll;

static bool ReadMapped(const SoRange& r, uintptr_t addr, void* dst, size_t n) {
    if (addr < r.base || addr - r.base + n > r.size) return false;
    memcpy(dst, reinterpret_cast<const void*>(addr), n);
    return true;
}

// GNU_HASH 符号计数：bucket 最大值 → chain 链尾（低位为 1）
static uint32_t GnuHashSymCount(const SoRange& r, uintptr_t gh) {
    uint32_t hdr[4] = {0, 0, 0, 0};
    if (!ReadMapped(r, gh, hdr, sizeof(hdr))) return 0;
    const uint32_t nbuckets = hdr[0], symoffset = hdr[1], bloomSize = hdr[2];
    if (!nbuckets || !symoffset) return 0;
    const uintptr_t buckets = gh + 16 + static_cast<uintptr_t>(bloomSize) * 8;
    uint32_t maxIdx = symoffset;
    for (uint32_t i = 0; i < nbuckets; i++) {
        uint32_t b = 0;
        if (!ReadMapped(r, buckets + i * 4u, &b, 4)) return 0;
        if (b > maxIdx) maxIdx = b;
    }
    const uintptr_t chain = buckets + static_cast<uintptr_t>(nbuckets) * 4;
    for (uint32_t k = 0; k < 0x100000u; k++) {
        uint32_t c = 0;
        if (!ReadMapped(r, chain + (maxIdx - symoffset + k) * 4u, &c, 4)) return 0;
        if (c & 1u) return maxIdx + k + 1;
    }
    return 0;
}

static uintptr_t ResolveEvalViaJniBridge(const SoRange& r) {
    YsxzEhdr eh;
    if (!ReadMapped(r, r.base, &eh, sizeof(eh))) return 0;
    if (memcmp(eh.ident, "\x7f""ELF", 4) != 0 || eh.ident[4] != 2) return 0;
    uintptr_t dynAddr = 0;
    uint64_t dynSize = 0;
    for (uint16_t i = 0; i < eh.phnum; i++) {
        YsxzPhdr ph;
        if (!ReadMapped(r, r.base + eh.phoff + static_cast<uint64_t>(i) * eh.phentsize, &ph,
                        sizeof(ph)))
            break;
        if (ph.type == kPtDynamic) {
            dynAddr = r.base + ph.vaddr;
            dynSize = ph.filesz;
            break;
        }
    }
    if (!dynAddr) return 0;

    uintptr_t symtab = 0, strtab = 0, hash = 0, gnuHash = 0;
    uint64_t strsz = 0;
    for (uint64_t o = 0; o + 16 <= dynSize; o += 16) {
        YsxzDyn dd;
        if (!ReadMapped(r, dynAddr + o, &dd, sizeof(dd))) break;
        if (dd.tag == 0) break;
        const uintptr_t v = r.base + dd.val;
        if (dd.tag == kDtSymtab) symtab = v;
        else if (dd.tag == kDtStrtab) strtab = v;
        else if (dd.tag == kDtStrsz) strsz = dd.val;
        else if (dd.tag == kDtHash) hash = v;
        else if (dd.tag == kDtGnuHash) gnuHash = v;
    }
    if (!symtab || !strtab) return 0;

    uint32_t count = 0;
    if (hash) {
        uint32_t h2[2] = {0, 0};
        if (ReadMapped(r, hash, h2, sizeof(h2))) count = h2[1];  // nchain == 符号数
    }
    if (!count && gnuHash) count = GnuHashSymCount(r, gnuHash);
    if (!count) count = 60000;  // 兜底上限，遇 name 越界即 break

    const char* want = sec_sym_cocos_jsb_eval();
    const size_t wantLen = strlen(want);
    for (uint32_t i = 0; i < count; i++) {
        YsxzSym sym;
        if (!ReadMapped(r, symtab + static_cast<uint64_t>(i) * sizeof(YsxzSym), &sym, sizeof(sym)))
            break;
        if (!sym.name) continue;
        if (strsz && sym.name >= strsz) break;
        if (sym.value >= r.size) continue;
        char nm[128] = {0};
        if (!ReadMapped(r, strtab + sym.name, nm, sizeof(nm) - 1)) break;
        if (strncmp(nm, want, wantLen) != 0 || nm[wantLen] != '\0') continue;

        const uintptr_t fn = r.base + sym.value;
        const size_t span =
            (sym.size > 0 && sym.size <= 0x400) ? static_cast<size_t>(sym.size) : (size_t)0x300;
        for (size_t b = 0; b + 4 <= span; b += 4) {
            uint32_t w = 0;
            if (!ReadMapped(r, fn + b, &w, 4)) return 0;
            if ((w & 0xFC000000u) != 0x94000000u) continue;  // BL imm26
            int64_t off = static_cast<int64_t>(w & 0x03FFFFFFu);
            if (off & 0x02000000) off -= 0x04000000;  // 26bit 符号扩展
            const uintptr_t tgt = fn + b + static_cast<uintptr_t>(off * 4);
            if (tgt < r.base || tgt >= r.base + r.size) continue;
            uint32_t pro = 0;
            if (!ReadMapped(r, tgt, &pro, 4)) continue;
            if (pro != kEvalInsn) continue;
            LOGI("%s jni bridge wrapper=%p bl@%p -> evalString=%p", YSXZ_LOG, (void*)fn,
                 (void*)(fn + b), (void*)tgt);
            return tgt;
        }
        LOGW("%s jni bridge found but no evalString callee in %zu bytes", YSXZ_LOG, span);
        return 0;
    }
    return 0;
}

static bool g_evalViaReadable = false;

static uintptr_t LocateEvalString() {
    const char* lib = kTargetLib();
    SoRange r = GetLoadedRange(lib);
    if (r.ok) {
        uintptr_t va = ResolveEvalViaJniBridge(r);
        if (va) {
            g_evalViaReadable = false;
            return va;
        }
        LOGW("%s adaptive resolve failed span=%zu, fallback static rva", YSXZ_LOG, r.size);
    } else {
        LOGW("%s lib mapping not found in /proc/self/maps", YSXZ_LOG);
    }
    for (uintptr_t rva : kEvalRvaFallbacks) {
        bool viaReadable = false;
        void* va = FileOffToVa(lib, rva, &viaReadable);
        if (!va) {
            LOGW("%s static rva 0x%lX not mapped", YSXZ_LOG, (unsigned long)rva);
            continue;
        }
        uint32_t got = 0;
        memcpy(&got, va, sizeof(got));
        if (got != kEvalInsn) {
            LOGW("%s static rva 0x%lX prologue drift got=0x%08X", YSXZ_LOG, (unsigned long)rva,
                 got);
            continue;
        }
        g_evalViaReadable = viaReadable;
        LOGI("%s evalString via static rva=0x%lX perm=%s", YSXZ_LOG, (unsigned long)rva,
             viaReadable ? "readable" : "rx");
        return reinterpret_cast<uintptr_t>(va);
    }
    return 0;
}

static uintptr_t g_evalVa = 0;
static uintptr_t g_libBase = 0;

static bool HookedEvalString(void* thiz, const char* script, long length, void* ret,
                             const char* fileName);

static bool HookEvalString() {
    if (g_hooked.load()) return true;
    const char* lib = kTargetLib();
    if (!isLibraryLoaded(lib)) return false;

    const uintptr_t va = LocateEvalString();
    if (!va) {
        LOGW("%s evalString locate failed", YSXZ_LOG);
        return false;
    }
    g_evalVa = va;
    g_libBase = GetLoadedRange(lib).base;
    if (DobbyHook((void*)va, (dobby_dummy_func_t)HookedEvalString,
                  (dobby_dummy_func_t*)&g_origSeEval) != 0) {
        LOGW("%s DobbyHook evalString failed @%p", YSXZ_LOG, (void*)va);
        return false;
    }
    g_hooked.store(true);
    const unsigned long rva = (g_libBase && va > g_libBase) ? (unsigned long)(va - g_libBase) : 0;
    LOGI("%s SELFCHECK evalString va=%p rva=0x%lX via=%s rd=1 hook=1", YSXZ_LOG, (void*)va,
         rva, g_evalViaReadable ? "readable" : "rx");
    LOGI("%s hooked se::evalString @%p rva=0x%lX", YSXZ_LOG, (void*)va, rva);
    return true;
}

static bool PatchRegistryCall(std::string* src) {
    const std::string begin = "\n}({\n";
    const std::string beginNew = "\n}(window.__starry_capture({\n";
    const std::string end = "\n}, {}, [";
    const std::string endNew = "\n}), {}, [";
    const size_t p = src->find(begin);
    if (p == std::string::npos) return false;
    src->replace(p, begin.size(), beginNew);
    const size_t q = src->find(end, p + beginNew.size());
    if (q == std::string::npos) return false;
    src->replace(q, end.size(), endNew);
    return true;
}

static bool HookedEvalString(void* thiz, const char* script, long length, void* ret,
                             const char* fileName) {
    if (!g_origSeEval) return false;
    if (!script || t_inSplice) return g_origSeEval(thiz, script, length, ret, fileName);

    long n = length;
    if (n < 0) n = (long)strlen(script);
    if (n < kBundleMinLen || n > kBundleMaxLen) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
    if (!IsTargetBundle(fileName)) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
    if (!FindBytes(script, (size_t)n, "window.__require") ||
        (!FindBytes(script, (size_t)n, "maxApDefRate") &&
         !FindBytes(script, (size_t)n, "AUTO_BATTLE")) ||
        FindBytes(script, (size_t)n, "starry_ysxz_v1")) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    std::string patched;
    try {
        std::string source(script, (size_t)n);
        if (!PatchRegistryCall(&source)) {
            LOGW("%s registry call pattern missing in bundle=%s", YSXZ_LOG, fileName);
            return g_origSeEval(thiz, script, length, ret, fileName);
        }
        patched.reserve(source.size() + strlen(kYsxzPatchJs) + 64);
        patched.assign(kYsxzPatchJs, strlen(kYsxzPatchJs));
        patched += "\n";
        patched += source;
    } catch (...) {
        LOGW("%s patch build exception; fallback original script", YSXZ_LOG);
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("%s logic splice #%d len %ld->%zu file=%s", YSXZ_LOG, hits, n, patched.size(),
         fileName ? fileName : "(null)");

    t_inSplice = true;
    bool ok = g_origSeEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    t_inSplice = false;
    return ok;
}

static JavaVM* g_jvm = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_setRet = nullptr;
static jmethodID g_setVal = nullptr;
static jmethodID g_clearAll = nullptr;

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static jclass LoadAppClass(JNIEnv* env, const char* name) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jmethodID curApp = env->GetStaticMethodID(
        atCls, "currentApplication", "()Landroid/app/Application;");
    if (!curApp) {
        env->ExceptionClear();
        env->DeleteLocalRef(atCls);
        return nullptr;
    }
    jobject app = env->CallStaticObjectMethod(atCls, curApp);
    env->DeleteLocalRef(atCls);
    if (!app) {
        env->ExceptionClear();
        return nullptr;
    }
    jclass appCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(appCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject cl = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (!cl) {
        env->ExceptionClear();
        return nullptr;
    }
    jclass clCls = env->GetObjectClass(cl);
    jmethodID loadClass =
        env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF(name);
    jclass cls = nullptr;
    if (loadClass && jname) {
        cls = (jclass)env->CallObjectMethod(cl, loadClass, jname);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            cls = nullptr;
        }
    }
    if (jname) env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(cl);
    return cls;
}

static void CacheJava(JNIEnv* env) {
    if (g_hooksCls) return;
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.YsxzHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/YsxzHooks");
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_setRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_setVal = env->GetStaticMethodID(g_hooksCls, "setVal", "(II)V");
        g_clearAll = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
    } else {
        env->ExceptionClear();
    }
}

static void SyncJava() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_setRet) {
        for (int i = 0; i < YSXZ_FEAT_COUNT; i++) {
            env->CallStaticVoidMethod(g_hooksCls, g_setRet, i,
                                      g_feat[i].load() ? JNI_TRUE : JNI_FALSE);
            if (g_setVal) env->CallStaticVoidMethod(g_hooksCls, g_setVal, i, g_val[i].load());
        }
    }
    if (detach) g_jvm->DetachCurrentThread();
}

static void* EarlyHookThread(void* arg) {
    g_jvm = (JavaVM*)arg;
    for (int i = 0; i < 3000; i++) {
        if (isLibraryLoaded(kTargetLib())) {
            g_libReady.store(true);
            if (HookEvalString()) {
                SyncJava();
                LOGI("%s early hook ok iter=%d", YSXZ_LOG, i);
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("%s early hook timeout", YSXZ_LOG);
    return nullptr;
}

void Ysxz_OnLoad(JavaVM* vm) {
    g_jvm = vm;
    // 逃查 5~13s 的干净退出（exit(0)）是谁调的；只在 ysxz 构建链里生效，日志前缀 [YsxzExit]。
    YsxzExitTrace_Start(vm);
    // 兜底：正常路径由 com.starry.magic.ysxz.YsxzAppComponentFactory 在 ~365ms（早于
    // 壳自检 413ms）触发；这里再调一次保证任何入口（含 OTA 热更 so）都起看门狗。
    YsxzSecShell_Start();
    static std::atomic<bool> started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
    }
}

bool Ysxz_TryInit(JavaVM* vm) {
    g_jvm = vm;
    if (!isLibraryLoaded(kTargetLib())) return false;
    g_libReady.store(true);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_jvm->DetachCurrentThread();
    }
    if (!HookEvalString()) return false;
    SyncJava();
    return true;
}

bool Ysxz_IsReady() { return g_hooked.load(); }

void Ysxz_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum >= 0 && featNum < YSXZ_FEAT_COUNT) g_feat[featNum].store(enabled);
    SyncJava();
    LOGI("%s feature #%d=%s", YSXZ_LOG, featNum, enabled ? "ON" : "OFF");
}

void Ysxz_OnValueChanged(int featNum, int value) {
    if (featNum >= 0 && featNum < YSXZ_FEAT_COUNT) g_val[featNum].store(value);
    SyncJava();
}

void Ysxz_Tick(const bool* features, const int* values) {
    if (features) {
        for (int i = 0; i < YSXZ_FEAT_COUNT; i++) g_feat[i].store(features[i]);
    }
    if (values) {
        for (int i = 0; i < YSXZ_FEAT_COUNT; i++) g_val[i].store(values[i]);
    }
    if (!g_hooked.load() && isLibraryLoaded(kTargetLib())) {
        g_libReady.store(true);
        HookEvalString();
    }
    SyncJava();
}

void Ysxz_ForceClear() {
    for (int i = 0; i < YSXZ_FEAT_COUNT; i++) {
        g_feat[i].store(false);
        g_val[i].store(0);
    }
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_clearAll) env->CallStaticVoidMethod(g_hooksCls, g_clearAll);
        if (detach) g_jvm->DetachCurrentThread();
    }
}
