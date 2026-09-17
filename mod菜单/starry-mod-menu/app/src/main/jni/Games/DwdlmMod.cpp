/* ================================================================
 *  动物大联盟 — Cocos2d-x JS evalString splice + 功能状态桥
 *
 *  运行时 libcocos2djs.so 由 libsecret 以删除文件映射加载，名称仍为
 *  libcocos2djs.so。动态符号表导出 JNI 桥，但不导出 se::ScriptEngine::evalString；
 *  同版本内 evalString 的 ELF 文件偏移为 0x30C79C。
 * ================================================================ */

#include "DwdlmMod.h"

#include <atomic>
#include <cstring>
#include <string>
#include <dlfcn.h>
#include <pthread.h>
#include <unistd.h>
#include <fcntl.h>
#include <elf.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"
#include "Includes/Utils.hpp"
#include "DwdlmPatchJs.h"

#define DWDLM_LOG "[dwdlm]"

// Keep native diagnostics on the same visible tag as the Java loader. Some
// ColorOS builds aggressively throttle long-lived custom tags.
#undef LOGI
#undef LOGW
#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "StarryMagic", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "StarryMagic", __VA_ARGS__))

static const char* kTargetLib() { return sec_lib_cocosjs(); }
static const char* kJniEvalSym() {
    return "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";
}
static constexpr uintptr_t kEvalFileOff = 0x30C79C;
static constexpr uint32_t kEvalPrologue = 0xD10543FF;  // sub sp, sp, #0x150

static std::atomic<bool> g_feat[DWDLM_FEAT_COUNT];
static std::atomic<int> g_atkValue{DWDLM_ATK_DEFAULT};
static std::atomic<int> g_speedValue{DWDLM_SPEED_DEFAULT};
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_evalHooked{false};
static std::atomic<bool> g_runScriptHooked{false};
static std::atomic<bool> g_pathProbeHooked{false};
static char g_lastScriptPath[256] = {0};
static FILE* (*g_origFopen)(const char*, const char*) = nullptr;
static std::atomic<bool> g_secureHookInstalled{false};
static std::atomic<int> g_spliceHits{0};
static std::atomic<int> g_evalSeen{0};
static thread_local bool t_inSplice = false;
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
    if (!atCls) { env->ExceptionClear(); return nullptr; }
    jmethodID curApp = env->GetStaticMethodID(atCls, "currentApplication", "()Landroid/app/Application;");
    if (!curApp) { env->ExceptionClear(); env->DeleteLocalRef(atCls); return nullptr; }
    jobject app = env->CallStaticObjectMethod(atCls, curApp);
    env->DeleteLocalRef(atCls);
    if (!app) { env->ExceptionClear(); return nullptr; }
    jclass appCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(appCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject cl = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (!cl) { env->ExceptionClear(); return nullptr; }
    jclass clCls = env->GetObjectClass(cl);
    jmethodID loadClass = env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF(name);
    jclass cls = nullptr;
    if (loadClass && jname) {
        cls = (jclass)env->CallObjectMethod(cl, loadClass, jname);
        if (env->ExceptionCheck()) { env->ExceptionClear(); cls = nullptr; }
    }
    if (jname) env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(cl);
    return cls;
}

static void CacheJava(JNIEnv* env) {
    if (g_hooksCls) return;
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.DwdlmHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/DwdlmHooks");
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_setRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_setVal = env->GetStaticMethodID(g_hooksCls, "setVal", "(II)V");
        g_clearAll = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
        if (env->ExceptionCheck()) env->ExceptionClear();
    } else {
        env->ExceptionClear();
    }
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_setRet) {
        for (int i = 1; i < DWDLM_FEAT_COUNT; i++) {
            env->CallStaticVoidMethod(g_hooksCls, g_setRet, i,
                                      g_feat[i].load() ? JNI_TRUE : JNI_FALSE);
        }
    }
    if (g_hooksCls && g_setVal) {
        env->CallStaticVoidMethod(g_hooksCls, g_setVal, DWDLM_FEAT_ATK, g_atkValue.load());
        env->CallStaticVoidMethod(g_hooksCls, g_setVal, DWDLM_FEAT_SPEED, g_speedValue.load());
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_jvm->DetachCurrentThread();
}

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
typedef bool (*SeRunScript_t)(void*, void*, void*);
static SeEvalString_t g_origSeEval = nullptr;
static SeRunScript_t g_origSeRunScript = nullptr;
typedef void (*VoidFn5_t)(void*, void*, void*, void*, void*);
static VoidFn5_t g_origNativeInit = nullptr;
static VoidFn5_t g_origBootStage = nullptr;
typedef void (*VoidFn2_t)(void*, void*);
static VoidFn2_t g_origNativeRender = nullptr;
static std::atomic<bool> g_stageHooksInstalled{false};
static std::atomic<bool> g_nativeInitSeen{false};
static std::atomic<bool> g_bootStageSeen{false};
static std::atomic<bool> g_bootForced{false};
static std::atomic<bool> g_nativeRenderHooked{false};
static std::atomic<int> g_renderFrames{0};

static void* FileOffToAnyVa(const char* lib, uintptr_t fileOff);
static void* ImageAddr(const char* lib, uintptr_t vaddr);
static void* FileOffToVa(const char* lib, uintptr_t fileOff, char* hitLine, size_t hitLineSize);

static FILE* hooked_fopen(const char* path, const char* mode) {
    static std::atomic<int> seen{0};
    int n = ++seen;
    if (path && (strstr(path, ".js") || strstr(path, ".jsc") ||
                strstr(path, "/files/res/") || strstr(path, "assets/"))) {
        snprintf(g_lastScriptPath, sizeof(g_lastScriptPath), "%s", path);
        LOGI("%s fopen %s mode=%s", DWDLM_LOG, path, mode ? mode : "(null)");
    }
    if (n <= 400) LOGI("%s fopen#%d %s mode=%s", DWDLM_LOG, n, path ? path : "(null)", mode ? mode : "(null)");
    if (!g_origFopen) return nullptr;
    return g_origFopen(path, mode);
}

typedef void* (*AAssetManagerOpen_t)(void*, const char*, int);
static AAssetManagerOpen_t g_origAAssetOpen = nullptr;

static void* hooked_AAssetOpen(void* mgr, const char* filename, int mode) {
    if (filename && (strstr(filename, ".js") || strstr(filename, ".jsc") ||
                     strstr(filename, "config.json") || strstr(filename, "main.js") ||
                     strstr(filename, "scripts") || strstr(filename, "settings"))) {
        LOGI("%s AAssetManager_open %s mode=%d", DWDLM_LOG, filename, mode);
    }
    if (!g_origAAssetOpen) return nullptr;
    return g_origAAssetOpen(mgr, filename, mode);
}

static void InstallPathProbe() {
    if (g_pathProbeHooked.load()) return;
    void* symbol = dlsym(RTLD_DEFAULT, "fopen");
    if (!symbol) {
        LOGW("%s fopen symbol missing", DWDLM_LOG);
        return;
    }
    if (DobbyHook(symbol, (dobby_dummy_func_t)hooked_fopen,
                  (dobby_dummy_func_t*)&g_origFopen) != 0) {
        LOGW("%s DobbyHook fopen failed", DWDLM_LOG);
        return;
    }
    void* androidLib = dlopen("libandroid.so", RTLD_NOW);
    void* assetOpen = androidLib ? dlsym(androidLib, "AAssetManager_open") : nullptr;
    if (!assetOpen) assetOpen = dlsym(RTLD_DEFAULT, "AAssetManager_open");
    if (assetOpen) {
        DobbyHook(assetOpen, (dobby_dummy_func_t)hooked_AAssetOpen,
                  (dobby_dummy_func_t*)&g_origAAssetOpen);
        LOGI("%s hooked AAssetManager_open @%p via libandroid=%p", DWDLM_LOG,
             assetOpen, androidLib);
    } else {
        LOGW("%s AAssetManager_open symbol missing", DWDLM_LOG);
    }
    g_pathProbeHooked.store(true);
}

static void hooked_bootStage(void* a0, void* a1, void* a2, void* a3, void* a4);

static void hooked_nativeInit(void* a0, void* a1, void* a2, void* a3, void* a4) {
    JNIEnv* env = reinterpret_cast<JNIEnv*>(a0);
    jstring path = reinterpret_cast<jstring>(a4);
    jstring replacement = nullptr;
    if (env) {
        const char* pathText = nullptr;
        if (path) pathText = env->GetStringUTFChars(path, nullptr);
        if (!pathText || !pathText[0]) {
            LOGI("%s nativeInit resourcePath empty; keeping original", DWDLM_LOG);
        } else {
            LOGI("%s nativeInit resourcePath=%s", DWDLM_LOG, pathText);
        }
        if (pathText) env->ReleaseStringUTFChars(reinterpret_cast<jstring>(a4), pathText);
    }
    g_nativeInitSeen.store(true);
    LOGI("%s nativeInit enter", DWDLM_LOG);
    if (g_origNativeInit) g_origNativeInit(a0, a1, a2, a3, path);
    LOGI("%s nativeInit exit", DWDLM_LOG);
    if (replacement && env) env->DeleteLocalRef(replacement);
}

static void hooked_bootStage(void* a0, void* a1, void* a2, void* a3, void* a4) {
    g_bootStageSeen.store(true);
    LOGI("%s bootStage enter", DWDLM_LOG);
    if (g_origBootStage) g_origBootStage(a0, a1, a2, a3, a4);
    LOGI("%s bootStage exit", DWDLM_LOG);
}

static void hooked_nativeRender(void* env, void* thiz) {
    const int frame = ++g_renderFrames;
    if (frame <= 3 || frame == 10 || frame == 60) {
        LOGI("%s nativeRender frame=%d boot=%d native=%d forced=%d",
             DWDLM_LOG, frame, g_bootStageSeen.load() ? 1 : 0,
             g_nativeInitSeen.load() ? 1 : 0, g_bootForced.load() ? 1 : 0);
    }
    if (g_origNativeRender) g_origNativeRender(env, thiz);
    if (frame >= 5 && g_evalSeen.load() == 0 && !g_bootForced.exchange(true)) {
        typedef void (*BootBodyFn_t)();
        void* boot = FileOffToVa(kTargetLib(), 0x279F38, nullptr, 0);
        LOGW("%s invoking applicationDidFinishLaunching body %p after %d frames",
             DWDLM_LOG, boot, frame);
        if (boot) reinterpret_cast<BootBodyFn_t>(boot)();
    }
}

static bool HasMarker(const std::string& s) {
    return s.find("window.__require") != std::string::npos &&
           s.find("cc._RF.push") != std::string::npos;
}

static bool hooked_seRunScript(void* thiz, void* scriptPtr, void* arg2) {
    int seen = ++g_evalSeen;
    std::string* source = reinterpret_cast<std::string*>(scriptPtr);
    if (seen <= 12) {
        LOGI("%s runScript call #%d script=%p arg2=%p", DWDLM_LOG, seen, scriptPtr, arg2);
    }
    if (!g_origSeRunScript) return false;
    if (source && HasMarker(*source) &&
        source->find("starry_dwdlm_v1") == std::string::npos) {
        LOGI("%s runScript splice #%d len %zu", DWDLM_LOG, ++g_spliceHits,
             source->size());
        std::string patched = *source;
        patched.append("\n");
        patched.append(kPatchJs);
        return g_origSeRunScript(thiz, &patched, arg2);
    }
    return g_origSeRunScript(thiz, scriptPtr, arg2);
}

static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!g_origSeEval) return false;
    if (!script || t_inSplice) return g_origSeEval(thiz, script, length, ret, fileName);
    int seen = ++g_evalSeen;
    if (seen <= 8) {
        LOGI("%s evalString call #%d len=%ld file=%s", DWDLM_LOG, seen,
             length, fileName ? fileName : "(null)");
    }
    size_t n = length < 0 ? strlen(script) : (size_t)length;
    if (n < 128u * 1024u || n > 64u * 1024u * 1024u) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
    try {
        std::string source(script, n);
        if (!HasMarker(source) || source.find("starry_dwdlm_v1") != std::string::npos) {
            return g_origSeEval(thiz, script, length, ret, fileName);
        }
        source.reserve(n + 20u * 1024u);
        source += "\n";
        source += kPatchJs;
        LOGI("%s evalString splice #%d len %zu->%zu file=%s", DWDLM_LOG,
             ++g_spliceHits, n, source.size(), fileName ? fileName : "(null)");
        t_inSplice = true;
        bool ok = g_origSeEval(thiz, source.c_str(), (long)source.size(), ret, fileName);
        t_inSplice = false;
        return ok;
    } catch (...) {
        t_inSplice = false;
        LOGW("%s evalString splice exception; fallback original", DWDLM_LOG);
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
}

// ELF file offset -> runtime VA. APKSecure maps the deleted library in multiple
// segments, so dladdr's image base plus an interior RVA is not reliable here.
static void* FileOffToVa(const char* lib, uintptr_t fileOff, char* hitLine, size_t hitLineSize) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib) || !strstr(line, ".so")) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
            if (hitLine && hitLineSize) snprintf(hitLine, hitLineSize, "%s", line);
        }
    }
    fclose(fp);
    return found;
}

// Runtime address for an ELF virtual address (not a file offset). The first
// PT_LOAD has p_vaddr == 0, so image_base + vaddr is stable for .text,
// .data and .bss even when bionic remaps individual segments.
static void* ImageAddr(const char* lib, uintptr_t vaddr) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* base = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib) || !strstr(line, ".so")) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (off == 0 && strchr(perm, 'r') && end > start) {
            base = reinterpret_cast<void*>(start);
            break;
        }
    }
    fclose(fp);
    if (!base) return nullptr;
    return reinterpret_cast<void*>(reinterpret_cast<uintptr_t>(base) + vaddr);
}

// Resolve an exported symbol by walking the in-memory ELF dynamic symbol
// table. The APKSecure loader keeps libcocos2djs.so in a private linker
// namespace, so dlsym(RTLD_DEFAULT, ...) cannot see its JNI bridge.
static void* FindDynSymbolFromLib(const char* lib, const char* name) {
    void* base = ImageAddr(lib, 0);
    if (!base || !name) return nullptr;
    auto* eh = reinterpret_cast<Elf64_Ehdr*>(base);
    if (memcmp(eh->e_ident, ELFMAG, SELFMAG) != 0) return nullptr;
    Elf64_Dyn* dyn = nullptr;
    for (int i = 0; i < eh->e_phnum; i++) {
        auto* ph = reinterpret_cast<Elf64_Phdr*>(
            reinterpret_cast<char*>(base) + eh->e_phoff + i * eh->e_phentsize);
        if (ph->p_type == PT_DYNAMIC) {
            dyn = reinterpret_cast<Elf64_Dyn*>(
                reinterpret_cast<char*>(base) + ph->p_vaddr);
            break;
        }
    }
    if (!dyn) return nullptr;

    Elf64_Sym* symtab = nullptr;
    const char* strtab = nullptr;
    size_t strsz = 0;
    size_t symCount = 0;
    for (Elf64_Dyn* d = dyn; d->d_tag != DT_NULL; d++) {
        switch (d->d_tag) {
            case DT_SYMTAB:
                symtab = reinterpret_cast<Elf64_Sym*>(
                    reinterpret_cast<char*>(base) + d->d_un.d_ptr);
                break;
            case DT_STRTAB:
                strtab = reinterpret_cast<const char*>(
                    reinterpret_cast<char*>(base) + d->d_un.d_ptr);
                break;
            case DT_STRSZ:
                strsz = static_cast<size_t>(d->d_un.d_val);
                break;
            case DT_HASH: {
                auto* hash = reinterpret_cast<uint32_t*>(
                    reinterpret_cast<char*>(base) + d->d_un.d_ptr);
                symCount = hash[1];
                break;
            }
            default:
                break;
        }
    }
    if (!symtab || !strtab || strsz == 0) return nullptr;
    if (symCount == 0 || symCount > 200000) symCount = 20000;
    for (size_t i = 0; i < symCount; i++) {
        if (symtab[i].st_name == 0 || symtab[i].st_name >= strsz) continue;
        const char* symName = strtab + symtab[i].st_name;
        if (strcmp(symName, name) == 0) {
            return reinterpret_cast<char*>(base) + symtab[i].st_value;
        }
    }
    return nullptr;
}

static void* FileOffToAnyVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib) || !strstr(line, ".so")) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'r') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        found = reinterpret_cast<void*>(start + (fileOff - off));
        break;
    }
    fclose(fp);
    return found;
}

static bool TryHookStageFns() {
    if (g_stageHooksInstalled.load()) return true;
    char line1[768] = {0};
    char line2[768] = {0};
    void* nativeInit = FileOffToVa(kTargetLib(), 0x279DFC, line1, sizeof(line1));
    void* bootStage = FileOffToVa(kTargetLib(), 0x279F28, line2, sizeof(line2));
    if (!nativeInit || !bootStage) return false;
    void* nativeRender = FileOffToVa(kTargetLib(), 0x27A030, nullptr, 0);
    if (DobbyHook(nativeInit, (dobby_dummy_func_t)hooked_nativeInit,
                  (dobby_dummy_func_t*)&g_origNativeInit) != 0) return false;
    if (DobbyHook(bootStage, (dobby_dummy_func_t)hooked_bootStage,
                  (dobby_dummy_func_t*)&g_origBootStage) != 0) return false;
    if (nativeRender && DobbyHook(nativeRender, (dobby_dummy_func_t)hooked_nativeRender,
                                  (dobby_dummy_func_t*)&g_origNativeRender) != 0) {
        LOGW("%s DobbyHook nativeRender failed", DWDLM_LOG);
    }
    g_stageHooksInstalled.store(true);
    LOGI("%s hooked nativeInit=%p bootStage=%p", DWDLM_LOG, nativeInit, bootStage);
    LOGI("%s hooked nativeRender=%p", DWDLM_LOG, nativeRender);
    return true;
}

static bool TryHookNativeRender() {
    if (g_nativeRenderHooked.load()) return true;
    void* nativeRender = FileOffToVa(kTargetLib(), 0x27A030, nullptr, 0);
    if (!nativeRender) return false;
    if (DobbyHook(nativeRender, (dobby_dummy_func_t)hooked_nativeRender,
                  (dobby_dummy_func_t*)&g_origNativeRender) != 0) {
        LOGW("%s DobbyHook nativeRender failed", DWDLM_LOG);
        return false;
    }
    g_nativeRenderHooked.store(true);
    LOGI("%s hooked nativeRender=%p", DWDLM_LOG, nativeRender);
    return true;
}

static bool TryHookRunScript() {
    if (g_runScriptHooked.load()) return true;
    char mapLine[768] = {0};
    void* target = FileOffToVa(kTargetLib(), 0x30CFD4, mapLine, sizeof(mapLine));
    if (!target) return false;
    if (DobbyHook(target, (dobby_dummy_func_t)hooked_seRunScript,
                  (dobby_dummy_func_t*)&g_origSeRunScript) != 0) {
        LOGW("%s DobbyHook runScript failed", DWDLM_LOG);
        return false;
    }
    g_runScriptHooked.store(true);
    LOGI("%s hooked runScript @%p map=%s", DWDLM_LOG, target,
         mapLine[0] ? mapLine : "(none)");
    return true;
}

typedef int (*IntBoolFn_t)(void);
typedef void (*VoidOneFn_t)(void*);
typedef void (*VoidTwoFn_t)(void*, void*);
typedef void (*VoidManyFn_t)(void*, void*, void*, void*, void*, void*, void*);
typedef void (*UpdateTaskEnqueueFn_t)(void*, int, void*, void*, void*, int, void*);
typedef long (*HttpResponseFn_t)(void*, void*, void*);
typedef void (*BuildHeadersFn_t)(void*, void*);
typedef void (*SignatureFn_t)(void*, void*);
static std::string StdStringToUtf8(void* p);
static IntBoolFn_t g_origIsFirstStartFn = nullptr;
static IntBoolFn_t g_origShellGateReadyFn = nullptr;
static VoidManyFn_t g_origResourceEntryFn = nullptr;
static VoidTwoFn_t g_origFirstStartBranchFn = nullptr;
static VoidOneFn_t g_origAfterFirstStartFn = nullptr;
static VoidTwoFn_t g_origMaybeStartUpdateFn = nullptr;
static VoidOneFn_t g_origUpdateTaskPumpFn = nullptr;
static UpdateTaskEnqueueFn_t g_origUpdateTaskEnqueueFn = nullptr;
static VoidTwoFn_t g_origUpdateErrorFn = nullptr;
static VoidOneFn_t g_origLoadAppInfoFn = nullptr;
static HttpResponseFn_t g_origHttpResponseFn = nullptr;
static BuildHeadersFn_t g_origBuildHeadersFn = nullptr;
#ifndef DWDLM_SHELL_ONLY
extern "C" SignatureFn_t g_origSignatureFn = nullptr;
extern "C" void DwdlmLogSignatureInputs(void* key, void* message);
extern "C" void DwdlmLogSignatureOutput(void* key, void* message, void* output);
extern "C" __attribute__((naked)) void HookedSignatureEntry();
#endif
static std::atomic<bool> g_bootFlowHooksInstalled{false};

// Keep libsecret.so text untouched while testing whether APKSecure can
// establish a real ticket/fingerprint. Dobby patches there change the
// bytes covered by self-text-check.
constexpr bool kUseSecretTextHooks = true;
static std::atomic<uintptr_t> g_cocosGuardBase{0};
static void StartShellStateGuard();
static void PatchCocosShellInterface();

static int HookedIsFirstStart() {
    const int value = g_origIsFirstStartFn ? g_origIsFirstStartFn() : -1;
    LOGI("%s boot isFirstStart=%d", DWDLM_LOG, value);
    return value;
}

static int HookedSecShellRequireExport();
static int HookedSecShellIsReadyExport();
static int HookedSecShellIsDirtyExport();
static int HookedSecShellCheckedMaskExport();
static int HookedSecShellFailedMaskExport();
static void* HookedSecShellTicketExport();
static int HookedSecShellDeriveKeyExport(const void*, int, void*, int);

static int HookedShellGateReady() {
}

static int HookedShellGateReadyCompat() {
#ifdef DWDLM_SHELL_ONLY
    PatchCocosShellInterface();
#endif
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s boot shellGateReady -> 1 (compat interface)", DWDLM_LOG);
    }
    return 1;
}

#ifndef DWDLM_SHELL_ONLY
extern "C" void DwdlmLogSignatureInputs(void* key, void* message) {
    LOGI("%s sign key=%s message=%s", DWDLM_LOG,
         StdStringToUtf8(key).c_str(), StdStringToUtf8(message).c_str());
}

extern "C" void DwdlmLogSignatureOutput(void* key, void* message, void* output) {
    LOGI("%s sign output=%s", DWDLM_LOG, StdStringToUtf8(output).c_str());
}

extern "C" __attribute__((naked)) void HookedSignatureEntry() {
    __asm__ volatile(
        "stp x29, x30, [sp, #-0x40]!\n"
        "mov x29, sp\n"
        "stp x0, x1, [sp, #0x10]\n"
        "str x8, [sp, #0x20]\n"
        "bl DwdlmLogSignatureInputs\n"
        "ldp x0, x1, [sp, #0x10]\n"
        "ldr x8, [sp, #0x20]\n"
        "adrp x9, g_origSignatureFn\n"
        "add x9, x9, :lo12:g_origSignatureFn\n"
        "ldr x9, [x9]\n"
        "blr x9\n"
        "ldp x0, x1, [sp, #0x10]\n"
        "ldr x2, [sp, #0x20]\n"
        "bl DwdlmLogSignatureOutput\n"
        "ldp x29, x30, [sp], #0x40\n"
        "ret\n"
    );
}
#endif

static void PatchCocosShellInterface() {
    static std::atomic<bool> patched{false};
    const bool first = !patched.exchange(true);
    const uintptr_t base = g_cocosGuardBase.load(std::memory_order_acquire);
    if (!base) return;

    volatile uint32_t* shellStatus = reinterpret_cast<volatile uint32_t*>(
        base + 0x21C9058);
    if (shellStatus) {
        const uint32_t before = *shellStatus;
        *shellStatus = 0;
        if (first) {
            LOGI("%s cocos shell status %u -> %u @%p", DWDLM_LOG,
                 (unsigned)before, (unsigned)*shellStatus, (void*)shellStatus);
        }
    }

    struct Slot { uintptr_t off; const char* name; void* value; };
    Slot slots[] = {
        {0x221B820, "Require", (void*)HookedSecShellRequireExport},
        {0x221B800, "IsReady", (void*)HookedSecShellIsReadyExport},
        {0x221B808, "IsDirty", (void*)HookedSecShellIsDirtyExport},
        {0x221B810, "CheckedMask", (void*)HookedSecShellCheckedMaskExport},
        {0x221B818, "FailedMask", (void*)HookedSecShellFailedMaskExport},
        {0x221B830, "Ticket", (void*)HookedSecShellTicketExport},
        {0x221B838, "DeriveKey", (void*)HookedSecShellDeriveKeyExport},
    };
    for (auto& slot : slots) {
        uintptr_t* target = reinterpret_cast<uintptr_t*>(
            base + slot.off);
        if (!target) continue;
        __atomic_store_n(target, reinterpret_cast<uintptr_t>(slot.value), __ATOMIC_SEQ_CST);
        if (first) {
            LOGI("%s patched cocos shell slot %s @%p value=%p readback=%p", DWDLM_LOG,
                 slot.name, target, slot.value, (void*)*target);
        }
    }
}

#ifndef DWDLM_SHELL_ONLY
static void HookedResourceEntry(void* a0, void* a1, void* a2, void* a3,
                                void* a4, void* a5, void* a6) {
    LOGI("%s boot resourceEntry enter", DWDLM_LOG);
    if (g_origResourceEntryFn) g_origResourceEntryFn(a0, a1, a2, a3, a4, a5, a6);
    LOGI("%s boot resourceEntry exit", DWDLM_LOG);
}

static void HookedFirstStartBranch(void* a0, void* a1) {
    LOGI("%s boot firstStartBranch enter", DWDLM_LOG);
    if (g_origFirstStartBranchFn) g_origFirstStartBranchFn(a0, a1);
    LOGI("%s boot firstStartBranch exit", DWDLM_LOG);
}

static void HookedAfterFirstStart(void* a0) {
    LOGI("%s boot afterFirstStart enter", DWDLM_LOG);
    if (g_origAfterFirstStartFn) g_origAfterFirstStartFn(a0);
    LOGI("%s boot afterFirstStart exit", DWDLM_LOG);
}

static void HookedMaybeStartUpdate(void* a0, void* a1) {
    LOGI("%s boot maybeStartUpdate enter", DWDLM_LOG);
    if (g_origMaybeStartUpdateFn) g_origMaybeStartUpdateFn(a0, a1);
    LOGI("%s boot maybeStartUpdate exit", DWDLM_LOG);
}

static void HookedUpdateTaskPump(void* a0) {
    LOGI("%s boot updateTaskPump enter", DWDLM_LOG);
    PatchCocosShellInterface();
    if (g_origUpdateTaskPumpFn) g_origUpdateTaskPumpFn(a0);
    LOGI("%s boot updateTaskPump exit", DWDLM_LOG);
}

static void HookedUpdateTaskEnqueue(void* a0, int a1, void* a2, void* a3,
                                   void* a4, int a5, void* a6) {
    LOGI("%s boot updateTaskEnqueue enter", DWDLM_LOG);
    LOGI("%s updateTaskEnqueue args mode=%d s1=%s s2=%s base=%s flag=%d", DWDLM_LOG,
         a1, StdStringToUtf8(a2).c_str(), StdStringToUtf8(a3).c_str(),
         StdStringToUtf8(a6).c_str(), a5);
    LOGI("%s updateTaskEnqueue caller=%p", DWDLM_LOG, __builtin_return_address(0));
    {
        typedef void (*StdStringCtorFn_t)(void*, const char*);
        unsigned char cocosString[24] __attribute__((aligned(8))) = {0};
        char hit[768] = {0};
        void* ctor = FileOffToVa(kTargetLib(), 0x1FCC9C, hit, sizeof(hit));
        if (ctor) {
            reinterpret_cast<StdStringCtorFn_t>(ctor)(
                cocosString, "https://api-animal.xgamevip.com/");
            a6 = cocosString;
            LOGI("%s updateTaskEnqueue cocos string=%s raw=%02x%02x%02x%02x ctor=%p", DWDLM_LOG,
                 StdStringToUtf8(a6).c_str(), cocosString[0], cocosString[1],
                 cocosString[2], cocosString[3], ctor);
        } else {
            LOGW("%s updateTaskEnqueue cocos string ctor missing map=%s", DWDLM_LOG, hit);
        }
    }
    if (g_origUpdateTaskEnqueueFn) g_origUpdateTaskEnqueueFn(a0, a1, a2, a3, a4, a5, a6);
    LOGI("%s boot updateTaskEnqueue exit", DWDLM_LOG);
}

static void HookedUpdateError(void* a0, void* a1) {
    LOGI("%s boot updateError enter", DWDLM_LOG);
    LOGI("%s updateError message=%s", DWDLM_LOG, StdStringToUtf8(a1).c_str());
    if (g_origUpdateErrorFn) g_origUpdateErrorFn(a0, a1);
    LOGI("%s boot updateError exit", DWDLM_LOG);
}

static void HookedBuildHeaders(void* a0, void* a1) {
    const uintptr_t cocosBase = g_cocosGuardBase.load(std::memory_order_acquire);
    if (cocosBase) {
        unsigned char* firstFp = reinterpret_cast<unsigned char*>(
            cocosBase + 0x221B7A0);
        unsigned char* secondFp = reinterpret_cast<unsigned char*>(
            cocosBase + 0x221B7D0);
        bool fpReady = false;
        for (int i = 0; i < 32; ++i) fpReady |= firstFp[i] != 0;
        if (fpReady) {
            memcpy(secondFp, firstFp, 32);
            *reinterpret_cast<volatile uint8_t*>(cocosBase + 0x221B7C4) = 0;
            *reinterpret_cast<volatile uint8_t*>(cocosBase + 0x221B7F0) = 1;
        }
    }
    if (g_origBuildHeadersFn) g_origBuildHeadersFn(a0, a1);
    static std::atomic<int> logged{0};
    if (++logged > 4 || !a1) return;

    uintptr_t client = 0;
    memcpy(&client, reinterpret_cast<char*>(a1) + 160, sizeof(client));
    if (!client) return;
    const char* begin = nullptr;
    const char* end = nullptr;
    memcpy(&begin, reinterpret_cast<char*>(client) + 152, sizeof(begin));
    memcpy(&end, reinterpret_cast<char*>(client) + 160, sizeof(end));
    if (!begin || !end || end < begin || (end - begin) % 24 != 0) return;

    int index = 0;
    for (const char* p = begin; p < end; p += 24) {
        LOGI("%s http header[%d]=%s", DWDLM_LOG, index++,
             StdStringToUtf8((void*)p).c_str());
    }
}

static long HookedHttpResponse(void* a0, void* a1, void* a2) {
    if (a2) {
        const unsigned char* raw = reinterpret_cast<const unsigned char*>(a2);
        uint64_t status = 0;
        memcpy(&status, raw + 80, sizeof(status));
        char* begin = nullptr;
        char* end = nullptr;
        memcpy(&begin, raw + 32, sizeof(begin));
        memcpy(&end, raw + 40, sizeof(end));
        size_t length = (begin && end && end > begin) ? (size_t)(end - begin) : 0;
        if (begin && length) {
            std::string body(begin, length);
            if (body.find("\"code\":4030") != std::string::npos) {
                static constexpr const char kLocalSessionOk[] =
                    "{\"code\":0,\"msg\":\"success\",\"data\":{}}";
                size_t newLength = sizeof(kLocalSessionOk) - 1;
                if (length >= newLength) {
                    memcpy(begin, kLocalSessionOk, newLength);
                    if (newLength < length) begin[newLength] = 0;
                    char* newEnd = begin + newLength;
                    memcpy(const_cast<unsigned char*>(raw) + 40, &newEnd, sizeof(newEnd));
                    end = newEnd;
                    length = newLength;
                    if (a0) {
                        unsigned char* request = nullptr;
                        memcpy(&request, reinterpret_cast<unsigned char*>(a0) + 16,
                               sizeof(request));
                        if (request) {
                            request[104] = 0;
                            request[105] = 0;
                        }
                    }
                    LOGI("%s local session bypass applied", DWDLM_LOG);
                }
            }
        }
        if (length > 1024) length = 1024;
        LOGI("%s http response status=%llu bodyLen=%zu body=%.*s", DWDLM_LOG,
             (unsigned long long)status, length, (int)length,
             begin ? begin : "");
    }
    return g_origHttpResponseFn ? g_origHttpResponseFn(a0, a1, a2) : 0;
}

static void HookedLoadAppInfo(void* a0) {
    if (g_origLoadAppInfoFn) g_origLoadAppInfoFn(a0);
    static constexpr const char* kFallbackBaseUrl =
        "https://api-animal.xgamevip.com/";
    typedef void (*AssignStdStringFn_t)(void*, const char*, size_t);
    typedef void* (*GetConfigFn_t)();
    void* getConfig = FileOffToVa(kTargetLib(), 0x2560A4, nullptr, 0);
    if (!getConfig) return;
    void* config = reinterpret_cast<GetConfigFn_t>(getConfig)();
    if (!config) return;
    void* baseField = reinterpret_cast<char*>(config) + 48;
    const std::string before = StdStringToUtf8(baseField);
    std::string resolved = kFallbackBaseUrl;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        jclass cls = LoadAppClass(env, "com.bingo.sdklibs.utils.PluginHelper");
        if (!cls) cls = env->FindClass("com/bingo/sdklibs/utils/PluginHelper");
        if (cls) {
            jmethodID mid = env->GetStaticMethodID(
                cls, "getEffectiveBaseUrl", "()Ljava/lang/String;");
            if (mid) {
                jstring value = (jstring)env->CallStaticObjectMethod(cls, mid);
                if (value) {
                    const char* chars = env->GetStringUTFChars(value, nullptr);
                    if (chars) {
                        if (chars[0]) resolved.assign(chars);
                        env->ReleaseStringUTFChars(value, chars);
                    }
                    env->DeleteLocalRef(value);
                }
            }
            env->DeleteLocalRef(cls);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
        if (detach) g_jvm->DetachCurrentThread();
    }
    void* assign = FileOffToVa(kTargetLib(), 0x1FFB28, nullptr, 0);
    if (assign) {
        reinterpret_cast<AssignStdStringFn_t>(assign)(
            baseField, resolved.c_str(), resolved.size());
    }
    LOGI("%s config baseUrl before=%s resolved=%s after=%s", DWDLM_LOG,
         before.c_str(), resolved.c_str(), StdStringToUtf8(baseField).c_str());
}
#endif


#ifdef DWDLM_SHELL_STATE_ONLY
static bool TryHookBootFlowFns() { return false; }
#else
static bool TryHookBootFlowFns() {
    if (g_bootFlowHooksInstalled.load()) return true;
    struct Item { uintptr_t off; void* hook; void** orig; const char* name; };
#ifdef DWDLM_SHELL_ONLY
    Item items[] = {
        {0x221CC4, (void*)HookedShellGateReadyCompat, (void**)&g_origShellGateReadyFn, "shellGateReady"},
    };
#else
    Item items[] = {
        {0x22DBA8, (void*)HookedIsFirstStart, (void**)&g_origIsFirstStartFn, "isFirstStart"},
        {0x23EC48, (void*)HookedResourceEntry, (void**)&g_origResourceEntryFn, "resourceEntry"},
        {0x23F6DC, (void*)HookedFirstStartBranch, (void**)&g_origFirstStartBranchFn, "firstStartBranch"},
        {0x23FA90, (void*)HookedAfterFirstStart, (void**)&g_origAfterFirstStartFn, "afterFirstStart"},
        {0x264A94, (void*)HookedMaybeStartUpdate, (void**)&g_origMaybeStartUpdateFn, "maybeStartUpdate"},
        {0x221CC4, (void*)HookedShellGateReadyCompat, (void**)&g_origShellGateReadyFn, "shellGateReady"},
        {0x258188, (void*)HookedUpdateTaskPump, (void**)&g_origUpdateTaskPumpFn, "updateTaskPump"},
        {0x256628, (void*)HookedUpdateTaskEnqueue, (void**)&g_origUpdateTaskEnqueueFn, "updateTaskEnqueue"},
        {0x25A94C, (void*)HookedUpdateError, (void**)&g_origUpdateErrorFn, "updateError"},
        {0x238C6C, (void*)HookedLoadAppInfo, (void**)&g_origLoadAppInfoFn, "loadAppInfo"},
        {0x2578B8, (void*)HookedHttpResponse, (void**)&g_origHttpResponseFn, "httpResponse"},
        {0x2583BC, (void*)HookedBuildHeaders, (void**)&g_origBuildHeadersFn, "buildHeaders"},
        {0x21A338, (void*)HookedSignatureEntry, (void**)&g_origSignatureFn, "signature"},
    };
#endif
    bool any = false;
    for (auto& item : items) {
        void* target = FileOffToVa(kTargetLib(), item.off, nullptr, 0);
        if (!target) continue;
        if (DobbyHook(target, (dobby_dummy_func_t)item.hook,
                      (dobby_dummy_func_t*)item.orig) == 0) {
            uintptr_t expected = 0;
            g_cocosGuardBase.compare_exchange_strong(
                expected, reinterpret_cast<uintptr_t>(target) - item.off,
                std::memory_order_release, std::memory_order_relaxed);
            LOGI("%s boot hook %s @%p", DWDLM_LOG, item.name, target);
            any = true;
        } else {
            LOGW("%s boot hook %s failed", DWDLM_LOG, item.name);
        }
    }
    if (any) g_bootFlowHooksInstalled.store(true);
#ifdef DWDLM_SHELL_ONLY
    if (any) PatchCocosShellInterface();
#endif
    return any;
}
#endif

static uint32_t ReadInsn(const void* p) {
    uint32_t v = 0;
    memcpy(&v, p, sizeof(v));
    return v;
}

static void* FollowVeneer(void* p) {
    if (!p) return nullptr;
    if (ReadInsn(p) == 0x58000051u &&
        ReadInsn(reinterpret_cast<char*>(p) + 4) == 0xD61F0220u) {
        uint64_t literal = 0;
        memcpy(&literal, reinterpret_cast<char*>(p) + 8, sizeof(literal));
        return reinterpret_cast<void*>(literal);
    }
    return nullptr;
}

// Resolve the real ScriptEngine::evalString from the exported JNI bridge.
// This avoids a version-specific ELF RVA; the JNI bridge is stable.
static void* ResolveEvalFromBridge(void* p, int depth) {
    if (!p || depth > 3) return nullptr;
    if (void* veneer = FollowVeneer(p)) return ResolveEvalFromBridge(veneer, depth + 1);
    if (ReadInsn(p) == kEvalPrologue) return p;
    for (int i = 0; i < 96; i++) {
        uint32_t insn = ReadInsn(reinterpret_cast<char*>(p) + i * 4);
        if ((insn & 0xFC000000u) == 0x94000000u ||
            (insn & 0xFC000000u) == 0x14000000u) {
            int64_t imm = static_cast<int32_t>(insn << 6) >> 4;
            void* target = reinterpret_cast<char*>(p) + i * 4 + imm;
            if (target == p) continue;
            if (void* found = ResolveEvalFromBridge(target, depth + 1)) return found;
        }
        if ((insn & 0xFFFFFC1Fu) == 0xD65F0000u) break;
    }
    return nullptr;
}

typedef bool (*FileUtilsIsFileExist_t)(void*, void*);
static FileUtilsIsFileExist_t g_origIsFileExist = nullptr;

static std::string StdStringToUtf8(void* p) {
    if (!p) return std::string();
    const unsigned char* raw = reinterpret_cast<const unsigned char*>(p);
    if ((raw[0] & 0x1) == 0) {
        return std::string(reinterpret_cast<const char*>(raw + 1), raw[0] >> 1);
    }
    const size_t size = *reinterpret_cast<const size_t*>(raw + 8);
    const char* data = *reinterpret_cast<const char* const*>(raw + 16);
    return data ? std::string(data, size) : std::string();
}

static bool HookedIsFileExist(void* thiz, void* path) {
    bool result = g_origIsFileExist ? g_origIsFileExist(thiz, path) : false;
    std::string name = StdStringToUtf8(path);
    if (name.find(".js") != std::string::npos ||
        name.find(".jsc") != std::string::npos ||
        name.find("main") != std::string::npos ||
        name.find("config") != std::string::npos ||
        name.find("settings") != std::string::npos) {
        LOGI("%s isFileExist %s = %d", DWDLM_LOG, name.c_str(), result ? 1 : 0);
    }
    return result;
}

static void InstallFileUtilsProbe() {
    if (g_origIsFileExist) return;
    void* getInstance = FileOffToVa(kTargetLib(), 0x267020, nullptr, 0);
    if (!getInstance) {
        LOGW("%s FileUtils::getInstance address missing", DWDLM_LOG);
        return;
    }
    typedef void* (*GetInstance_t)();
    void* instance = reinterpret_cast<GetInstance_t>(getInstance)();
    if (!instance) {
        LOGW("%s FileUtils instance null", DWDLM_LOG);
        return;
    }
    void** vtable = *reinterpret_cast<void***>(instance);
    if (!vtable) return;
    void* isFileExist = vtable[0x100 / 8];
    if (!isFileExist) {
        LOGW("%s FileUtils::isFileExist slot missing", DWDLM_LOG);
        return;
    }
    if (DobbyHook(isFileExist, (dobby_dummy_func_t)HookedIsFileExist,
                  (dobby_dummy_func_t*)&g_origIsFileExist) == 0) {
        LOGI("%s hooked FileUtils::isFileExist @%p", DWDLM_LOG, isFileExist);
    } else {
        LOGW("%s DobbyHook FileUtils::isFileExist failed", DWDLM_LOG);
    }
}

typedef void (*UpdateCallback_t)(void*);
static UpdateCallback_t g_origOnCheckUpdate = nullptr;
static UpdateCallback_t g_origOnFileListComplete = nullptr;
static UpdateCallback_t g_origOnLoadingComplete = nullptr;
static UpdateCallback_t g_origOnLoadingProgress = nullptr;
static UpdateCallback_t g_origOnSetVersion = nullptr;

static void HookedOnCheckUpdate(void* a) {
    LOGI("%s update cb onCheckUpdate", DWDLM_LOG);
    if (g_origOnCheckUpdate) g_origOnCheckUpdate(a);
}
static void HookedOnFileListComplete(void* a) {
    LOGI("%s update cb onFileListDownloadComplete", DWDLM_LOG);
    if (g_origOnFileListComplete) g_origOnFileListComplete(a);
}
static void HookedOnLoadingComplete(void* a) {
    LOGI("%s update cb onLoadingComplete", DWDLM_LOG);
    if (g_origOnLoadingComplete) g_origOnLoadingComplete(a);
}
static void HookedOnLoadingProgress(void* a) {
    LOGI("%s update cb onLoadingProgress arg=%p", DWDLM_LOG, a);
    if (g_origOnLoadingProgress) g_origOnLoadingProgress(a);
}
static void HookedOnSetVersion(void* a) {
    LOGI("%s update cb onSetVersion arg=%p", DWDLM_LOG, a);
    if (g_origOnSetVersion) g_origOnSetVersion(a);
}

static void InstallUpdateCallbacksProbe() {
    struct Item { uintptr_t off; void* hook; void** orig; const char* name; };
    Item items[] = {
        {0x231710, (void*)HookedOnCheckUpdate, (void**)&g_origOnCheckUpdate, "onCheckUpdate"},
        {0x2318CC, (void*)HookedOnFileListComplete, (void**)&g_origOnFileListComplete, "onFileListComplete"},
        {0x2319A4, (void*)HookedOnLoadingComplete, (void**)&g_origOnLoadingComplete, "onLoadingComplete"},
        {0x231A7C, (void*)HookedOnLoadingProgress, (void**)&g_origOnLoadingProgress, "onLoadingProgress"},
        {0x231B5C, (void*)HookedOnSetVersion, (void**)&g_origOnSetVersion, "onSetVersion"},
    };
    for (auto& item : items) {
        if (*item.orig) continue;
        void* target = FileOffToVa(kTargetLib(), item.off, nullptr, 0);
        if (!target) {
            LOGW("%s update cb %s offset 0x%lX missing", DWDLM_LOG,
                 item.name, (unsigned long)item.off);
            continue;
        }
        if (DobbyHook(target, (dobby_dummy_func_t)item.hook,
                      (dobby_dummy_func_t*)item.orig) == 0) {
            LOGI("%s hooked update cb %s @%p", DWDLM_LOG, item.name, target);
        } else {
            LOGW("%s DobbyHook update cb %s failed", DWDLM_LOG, item.name);
        }
    }
}

static bool TryHookEvalString() {
    if (g_evalHooked.load()) return true;
    const char* lib = kTargetLib();
    if (!isLibraryLoaded(lib)) return false;
    void* jni = dlsym(RTLD_DEFAULT, kJniEvalSym());
    if (!jni) jni = FindDynSymbolFromLib(lib, kJniEvalSym());
    char mapLine[768] = {0};
    void* target = jni ? ResolveEvalFromBridge(jni, 0) : nullptr;
    if (target) {
        LOGI("%s evalString resolved from JNI bridge jni=%p target=%p", DWDLM_LOG,
             jni, target);
    } else {
        target = FileOffToVa(lib, kEvalFileOff, mapLine, sizeof(mapLine));
    }
    if (!target) {
        LOGW("%s evalString file offset 0x%lX not in RX map", DWDLM_LOG,
             (unsigned long)kEvalFileOff);
        return false;
    }
    uint32_t got = 0;
    memcpy(&got, target, sizeof(got));
    if (got != kEvalPrologue) {
        LOGW("%s evalString prologue drift got=0x%08X want=0x%08X jni=%p target=%p map=%s",
             DWDLM_LOG, got, kEvalPrologue, jni, target,
             mapLine[0] ? mapLine : "(none)");
        return false;
    }
    if (DobbyHook(target, (dobby_dummy_func_t)hooked_seEval,
                  (dobby_dummy_func_t*)&g_origSeEval) != 0) {
        LOGW("%s DobbyHook evalString failed target=%p map=%s", DWDLM_LOG, target,
             mapLine[0] ? mapLine : "(none)");
        return false;
    }
    g_evalHooked.store(true);
    LOGI("%s hooked evalString @%p file=0x%lX jni=%p map=%s", DWDLM_LOG, target,
         (unsigned long)kEvalFileOff, jni, mapLine[0] ? mapLine : "(none)");
    return true;
}

static void* EarlyHookThread(void* arg) {
#ifdef DWDLM_SHELL_STATE_ONLY
    (void)arg;
    LOGI("%s state-only: skip cocos boot hooks", DWDLM_LOG);
    return nullptr;
#else
    g_jvm = (JavaVM*)arg;
    for (int i = 0; i < 3000; i++) {
#ifdef DWDLM_SHELL_ONLY
        if (isLibraryLoaded(kTargetLib()) && TryHookBootFlowFns()) {
            g_ready.store(true);
            SyncJavaRets();
            LOGI("%s early shell-only hook ready iter=%d", DWDLM_LOG, i);
            return nullptr;
        }
#else
        if (isLibraryLoaded(kTargetLib()) && TryHookEvalString()) {
            TryHookBootFlowFns();
            g_ready.store(true);
            SyncJavaRets();
            LOGI("%s early hook ready iter=%d", DWDLM_LOG, i);
            return nullptr;
        }
#endif
        usleep(20 * 1000);
    }
    LOGW("%s early hook timeout", DWDLM_LOG);
    return nullptr;
#endif
}

void Dwdlm_StartEarlyHookThread(JavaVM* vm) {
    static std::atomic<bool> started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) pthread_detach(th);
    else started.store(false);
}

static bool IsReadableRange(uintptr_t addr, size_t size) {
    if (!addr || size == 0 || addr + size < addr) return false;
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    const uintptr_t endAddr = addr + size - 1;
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        unsigned long start = 0, end = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &start, &end, perm) != 3) continue;
        if (!strchr(perm, 'r') || end <= start) continue;
        if (addr >= start && endAddr < end) { ok = true; break; }
    }
    fclose(fp);
    return ok;
}

static void DumpDexAt(const void* candidate, int index) {
    const unsigned char magic[8] = {'d', 'e', 'x', '\n', '0', '3', '5', 0};
    if (!IsReadableRange(reinterpret_cast<uintptr_t>(candidate), 0x40) ||
        memcmp(candidate, magic, sizeof(magic)) != 0) return;
    uint32_t fileSize = 0;
    memcpy(&fileSize, reinterpret_cast<const char*>(candidate) + 0x20, sizeof(fileSize));
    if (fileSize < 0x70 || fileSize > 64u * 1024u * 1024u ||
        !IsReadableRange(reinterpret_cast<uintptr_t>(candidate), fileSize)) {
        LOGW("%s business dex candidate size invalid: %u", DWDLM_LOG, fileSize);
        return;
    }
    char path[256] = {0};
    snprintf(path, sizeof(path),
             "/data/user/0/com.fightingstars.games.animal/files/starry_business_%d_%u.dex",
             index, fileSize);
    FILE* out = fopen(path, "wb");
    if (!out) {
        LOGW("%s business dex fopen failed: %s", DWDLM_LOG, path);
        return;
    }
    size_t written = fwrite(candidate, 1, fileSize, out);
    fclose(out);
    LOGI("%s business dex dumped: %s bytes=%zu addr=%p", DWDLM_LOG, path,
         written, candidate);
}

static void DumpBusinessDexOnce() {
    static std::atomic<bool> attempted{false};
    bool expected = false;
    if (!attempted.compare_exchange_strong(expected, true)) return;

    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;

    jclass atCls = env->FindClass("android/app/ActivityThread");
    jmethodID current = atCls ? env->GetStaticMethodID(
        atCls, "currentApplication", "()Landroid/app/Application;") : nullptr;
    jobject app = current ? env->CallStaticObjectMethod(atCls, current) : nullptr;
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (!app) {
        LOGW("%s business dex dump skipped: application null", DWDLM_LOG);
        if (atCls) env->DeleteLocalRef(atCls);
        if (detach) g_jvm->DetachCurrentThread();
        return;
    }

    jclass appCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(appCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject loader = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (!loader) {
        LOGW("%s business dex dump skipped: class loader null", DWDLM_LOG);
        goto cleanup;
    }

    {
        jclass loaderCls = env->GetObjectClass(loader);
        jfieldID pathListField = env->GetFieldID(
            loaderCls, "pathList", "Ldalvik/system/DexPathList;");
        jobject pathList = pathListField ? env->GetObjectField(loader, pathListField) : nullptr;
        if (env->ExceptionCheck()) env->ExceptionClear();
        jclass pathCls = pathList ? env->GetObjectClass(pathList) : nullptr;
        jfieldID elementsField = pathCls ? env->GetFieldID(
            pathCls, "dexElements", "[Ldalvik/system/DexPathList$Element;") : nullptr;
        jobjectArray elements = elementsField ? (jobjectArray)env->GetObjectField(pathList, elementsField) : nullptr;
        if (env->ExceptionCheck()) env->ExceptionClear();
        jsize count = elements ? env->GetArrayLength(elements) : 0;
        int dumped = 0;
        for (jsize i = 0; i < count; i++) {
            jobject element = env->GetObjectArrayElement(elements, i);
            jclass elementCls = element ? env->GetObjectClass(element) : nullptr;
            jfieldID dexField = elementCls ? env->GetFieldID(
                elementCls, "dexFile", "Ldalvik/system/DexFile;") : nullptr;
            jobject dexFile = dexField ? env->GetObjectField(element, dexField) : nullptr;
            if (env->ExceptionCheck()) env->ExceptionClear();
            if (!dexFile) {
                if (elementCls) env->DeleteLocalRef(elementCls);
                if (element) env->DeleteLocalRef(element);
                continue;
            }

            jclass dexCls = env->FindClass("dalvik/system/DexFile");
            jfieldID cookieField = dexCls ? env->GetFieldID(dexCls, "mCookie", "[J") : nullptr;
            jlongArray cookie = cookieField ? (jlongArray)env->GetObjectField(dexFile, cookieField) : nullptr;
            if (env->ExceptionCheck()) env->ExceptionClear();
            if (cookie && env->GetArrayLength(cookie) >= 2) {
                jlong values[2] = {0, 0};
                env->GetLongArrayRegion(cookie, 0, 2, values);
                const uintptr_t cookiePtr = static_cast<uintptr_t>(values[1]);
                bool found = false;
                for (uintptr_t off = 8; off < 0x80 && !found; off += 8) {
                    if (!IsReadableRange(cookiePtr + off, sizeof(uintptr_t))) continue;
                    uintptr_t candidate = 0;
                    memcpy(&candidate, reinterpret_cast<const void*>(cookiePtr + off), sizeof(candidate));
                    if (!IsReadableRange(candidate, 0x40)) continue;
                    const unsigned char magic[8] = {'d', 'e', 'x', '\n', '0', '3', '5', 0};
                    if (memcmp(reinterpret_cast<const void*>(candidate), magic, sizeof(magic)) == 0) {
                        DumpDexAt(reinterpret_cast<const void*>(candidate), dumped++);
                        found = true;
                    }
                }
            }
            if (cookie) env->DeleteLocalRef(cookie);
            if (dexCls) env->DeleteLocalRef(dexCls);
            if (dexFile) env->DeleteLocalRef(dexFile);
            if (elementCls) env->DeleteLocalRef(elementCls);
            if (element) env->DeleteLocalRef(element);
        }
        LOGI("%s business dex dump complete: %d", DWDLM_LOG, dumped);
        if (elements) env->DeleteLocalRef(elements);
        if (pathCls) env->DeleteLocalRef(pathCls);
        if (pathList) env->DeleteLocalRef(pathList);
        if (loaderCls) env->DeleteLocalRef(loaderCls);
    }

cleanup:
    env->DeleteLocalRef(loader);
    env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (atCls) env->DeleteLocalRef(atCls);
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_jvm->DetachCurrentThread();
}

bool Dwdlm_TryInit(JavaVM* vm) {
    LOGI("%s secureHook=%d evalSeen=%d runScript=%d stages=%d native=%d boot=%d path=%s", DWDLM_LOG,
         g_secureHookInstalled.load() ? 1 : 0, g_evalSeen.load(),
         g_runScriptHooked.load() ? 1 : 0, g_stageHooksInstalled.load() ? 1 : 0,
         g_nativeInitSeen.load() ? 1 : 0, g_bootStageSeen.load() ? 1 : 0,
         g_lastScriptPath[0] ? g_lastScriptPath : "(none)");
    if (vm) g_jvm = vm;
    LOGW("%s probe mode: game hooks disabled", DWDLM_LOG);
    g_ready.store(true);
    SyncJavaRets();
    return true;
}

bool Dwdlm_IsReady() { return g_ready.load(); }

void Dwdlm_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum > 0 && featNum < DWDLM_FEAT_COUNT) {
        g_feat[featNum].store(enabled);
        LOGI("%s feature #%d=%s", DWDLM_LOG, featNum, enabled ? "ON" : "OFF");
    }
    SyncJavaRets();
}

void Dwdlm_OnValueChanged(int featNum, int value) {
    if (featNum == DWDLM_FEAT_ATK) {
        if (value < DWDLM_ATK_MIN) value = DWDLM_ATK_MIN;
        if (value > DWDLM_ATK_MAX) value = DWDLM_ATK_MAX;
        g_atkValue.store(value);
        LOGI("%s attack slider=%d", DWDLM_LOG, value);
    } else if (featNum == DWDLM_FEAT_SPEED) {
        if (value < DWDLM_SPEED_MIN) value = DWDLM_SPEED_MIN;
        if (value > DWDLM_SPEED_MAX) value = DWDLM_SPEED_MAX;
        g_speedValue.store(value);
        LOGI("%s speed slider=%d", DWDLM_LOG, value);
    } else {
        return;
    }
    SyncJavaRets();
}

void Dwdlm_Tick(const bool* features, const int* values) {
    if (features) {
        for (int i = 1; i < DWDLM_FEAT_COUNT; i++) g_feat[i].store(features[i]);
    }
    if (values) {
        int atk = values[DWDLM_FEAT_ATK];
        if (atk >= DWDLM_ATK_MIN && atk <= DWDLM_ATK_MAX) g_atkValue.store(atk);
        int speed = values[DWDLM_FEAT_SPEED];
        if (speed >= DWDLM_SPEED_MIN && speed <= DWDLM_SPEED_MAX) g_speedValue.store(speed);
    }
#ifndef DWDLM_SHELL_ONLY
    if (!g_evalHooked.load()) TryHookEvalString();
#endif
    SyncJavaRets();
}

void Dwdlm_ForceClear() {
    for (int i = 1; i < DWDLM_FEAT_COUNT; i++) g_feat[i].store(false);
    g_atkValue.store(DWDLM_ATK_DEFAULT);
    g_speedValue.store(DWDLM_SPEED_DEFAULT);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_clearAll) env->CallStaticVoidMethod(g_hooksCls, g_clearAll);
        if (g_hooksCls && g_setVal) {
            env->CallStaticVoidMethod(g_hooksCls, g_setVal, DWDLM_FEAT_ATK, DWDLM_ATK_DEFAULT);
            env->CallStaticVoidMethod(g_hooksCls, g_setVal, DWDLM_FEAT_SPEED, DWDLM_SPEED_DEFAULT);
        }
        if (detach) g_jvm->DetachCurrentThread();
    }
}

typedef void (*SecShellRequire_t)(const char*, const char*, const char*);
void Dwdlm_PrepareSecureShell();

typedef void (*OnShellCreate_t)(void*, void*, int);
static OnShellCreate_t g_origOnShellCreate = nullptr;

static void LogRuntimeApplication(JNIEnv* env, const char* phase) {
    if (!env) return;
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        LOGW("%s onShellCreate %s app lookup failed", DWDLM_LOG, phase);
        return;
    }
    jmethodID current = env->GetStaticMethodID(
        atCls, "currentApplication", "()Landroid/app/Application;");
    jobject app = current ? env->CallStaticObjectMethod(atCls, current) : nullptr;
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (!app) {
        LOGW("%s onShellCreate %s app=null", DWDLM_LOG, phase);
        env->DeleteLocalRef(atCls);
        return;
    }
    jclass appCls = env->GetObjectClass(app);
    jclass clsCls = env->FindClass("java/lang/Class");
    jmethodID getName = clsCls ? env->GetMethodID(clsCls, "getName", "()Ljava/lang/String;") : nullptr;
    jstring jname = (appCls && getName) ? (jstring)env->CallObjectMethod(appCls, getName) : nullptr;
    const char* name = jname ? env->GetStringUTFChars(jname, nullptr) : nullptr;
    LOGI("%s onShellCreate %s app=%s", DWDLM_LOG, phase, name ? name : "(unknown)");
    if (name) env->ReleaseStringUTFChars(jname, name);
    if (jname) env->DeleteLocalRef(jname);
    if (clsCls) env->DeleteLocalRef(clsCls);
    if (appCls) env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    env->DeleteLocalRef(atCls);
    if (env->ExceptionCheck()) env->ExceptionClear();
}

// The mod triggers APKSecure's self-text and manifest integrity checks, so the
// shell marks its persistent state dirty even though the checks are rewritten
// to pass at the reporting layer. onShellCreate runs the real game
// BingoApplication.onCreate only when dirty == 0. Clear only that derived
// flag immediately before the shell call; keep ready/checked/failed and all
// earlier decryption/extraction state untouched.

static void ClearShellDirty() {
    volatile uint32_t* dirty = reinterpret_cast<volatile uint32_t*>(
        ImageAddr("libsecret.so", 0x70318));
    if (dirty) *dirty = 0;

    volatile uint32_t* ready = reinterpret_cast<volatile uint32_t*>(
        ImageAddr("libsecret.so", 0x70358));
    volatile uint32_t* checked = reinterpret_cast<volatile uint32_t*>(
        ImageAddr("libsecret.so", 0x70328));
    volatile uint32_t* failed = reinterpret_cast<volatile uint32_t*>(
        ImageAddr("libsecret.so", 0x7032C));
    if (ready) *ready = 1;
    if (checked) *checked = 0x7EFu;
    if (failed) *failed = 0;

    const char* path =
        "/data/user/0/com.fightingstars.games.animal/no_backup/.sec_shell_state";
    int fd = open(path, O_RDWR);
    if (fd < 0) return;
    unsigned char buf[20] = {0};
    if (pread(fd, buf, sizeof(buf), 0) == (ssize_t)sizeof(buf) &&
        memcmp(buf, "SCES", 4) == 0) {
        uint32_t dirtyValue = 0;
        memcpy(buf + 8, &dirtyValue, 4);
        uint32_t readyValue = 1;
        uint32_t checkedValue = 0x7EFu;
        uint32_t failedValue = 0;
        memcpy(buf + 4, &readyValue, 4);
        memcpy(buf + 12, &checkedValue, 4);
        memcpy(buf + 16, &failedValue, 4);
        pwrite(fd, buf, sizeof(buf), 0);
    }
    close(fd);
    LOGI("%s shell state normalized ready=1 dirty=0 checked=0x%X failed=0", DWDLM_LOG, 0x7EFu);
}

static std::atomic<bool> g_shellStateGuardStarted{false};

static void* SecretExport(uintptr_t fileOff);

static void* ShellStateGuardThread(void*) {
    for (int i = 0; i < 120000; ++i) {
#ifdef DWDLM_DATA_GUARD_ONLY
        volatile uint32_t* dirty = reinterpret_cast<volatile uint32_t*>(
            ImageAddr("libsecret.so", 0x70318));
        volatile uint32_t* ready = reinterpret_cast<volatile uint32_t*>(
            ImageAddr("libsecret.so", 0x70358));
        volatile uint32_t* checked = reinterpret_cast<volatile uint32_t*>(
            ImageAddr("libsecret.so", 0x70328));
        volatile uint32_t* failed = reinterpret_cast<volatile uint32_t*>(
            ImageAddr("libsecret.so", 0x7032C));
        if (!dirty || !ready || !checked || !failed) {
            usleep(1000);
            continue;
        }
        static std::atomic<int> logged{0};
        if (++logged == 1) {
            LOGI("%s data guard pointers dirty=%p ready=%p checked=%p failed=%p", DWDLM_LOG,
                 (void*)dirty, (void*)ready, (void*)checked, (void*)failed);
        }
#else
        const uintptr_t cocosBase = g_cocosGuardBase.load(std::memory_order_acquire);
        if (!cocosBase) {
            usleep(1000);
            continue;
        }
        volatile uint32_t* dirty = reinterpret_cast<volatile uint32_t*>(
            SecretExport(0x70318));
        volatile uint32_t* ready = reinterpret_cast<volatile uint32_t*>(
            SecretExport(0x70358));
        volatile uint32_t* checked = reinterpret_cast<volatile uint32_t*>(
            SecretExport(0x70328));
        volatile uint32_t* failed = reinterpret_cast<volatile uint32_t*>(
            SecretExport(0x7032C));
#endif
        if (dirty) *dirty = 0;
        if (ready) *ready = 1;
        if (checked) *checked = 0x7EFu;
        if (failed) *failed = 0;
#ifndef DWDLM_DATA_GUARD_ONLY
        unsigned char* firstFp = reinterpret_cast<unsigned char*>(
            cocosBase + 0x221B7A0);
        unsigned char* secondFp = reinterpret_cast<unsigned char*>(
            cocosBase + 0x221B7D0);
        volatile uint8_t* tampered = reinterpret_cast<volatile uint8_t*>(
            cocosBase + 0x221B7C4);
        volatile uint8_t* secondFpReady = reinterpret_cast<volatile uint8_t*>(
            cocosBase + 0x221B7F0);
        if (firstFp && secondFp) memcpy(secondFp, firstFp, 32);
        if (tampered) *tampered = 0;
        if (secondFpReady) *secondFpReady = 1;
        PatchCocosShellInterface();
#endif
        usleep(500);
    }
    LOGI("%s shell state guard exit", DWDLM_LOG);
    return nullptr;
}

static void StartShellStateGuard() {
    if (g_shellStateGuardStarted.exchange(true)) return;
    pthread_t thread = 0;
    if (pthread_create(&thread, nullptr, ShellStateGuardThread, nullptr) == 0) {
        pthread_detach(thread);
        LOGI("%s shell state guard started", DWDLM_LOG);
    } else {
        g_shellStateGuardStarted.store(false);
        LOGW("%s shell state guard start failed", DWDLM_LOG);
    }
}

static void HookedOnShellCreate(void* env, void* thiz, int api) {
    LOGI("%s onShellCreate enter api=%d", DWDLM_LOG, api);
    ClearShellDirty();
    LogRuntimeApplication(reinterpret_cast<JNIEnv*>(env), "before");
    if (g_origOnShellCreate) g_origOnShellCreate(env, thiz, api);
    LogRuntimeApplication(reinterpret_cast<JNIEnv*>(env), "after");
    LOGI("%s onShellCreate exit", DWDLM_LOG);
}

static SecShellRequire_t g_origSecShellRequire = nullptr;
static volatile uint32_t* g_secCheckedMask = nullptr;
static volatile uint32_t* g_secFailedMask = nullptr;

static int g_secureOverrideHits = 0;
typedef int (*SecShellNoArgFn_t)(void);
typedef void* (*SecShellTicketFn_t)(void);
typedef int (*SecShellDeriveKeyFn_t)(const void*, int, void*, int);
static SecShellNoArgFn_t g_origSecShellRequireExport = nullptr;
static SecShellNoArgFn_t g_origSecShellIsReadyExport = nullptr;
static SecShellNoArgFn_t g_origSecShellIsDirtyExport = nullptr;
static SecShellNoArgFn_t g_origSecShellCheckedMaskExport = nullptr;
static SecShellNoArgFn_t g_origSecShellFailedMaskExport = nullptr;
static SecShellTicketFn_t g_origSecShellTicketExport = nullptr;
static SecShellDeriveKeyFn_t g_origSecShellDeriveKeyExport = nullptr;
static std::atomic<bool> g_shellGateHooksInstalled{false};

static void* SecretExport(uintptr_t fileOff) {
    static std::atomic<uintptr_t> cachedBase{0};
    uintptr_t base = cachedBase.load(std::memory_order_acquire);
    if (!base) {
        void* handle = dlopen("libsecret.so", RTLD_NOW | RTLD_NOLOAD);
        if (!handle) return nullptr;
        void* require = dlsym(handle, "SecShell_Require");
        if (!require) return nullptr;
        base = reinterpret_cast<uintptr_t>(require) - 0x3D59C;
        cachedBase.store(base, std::memory_order_release);
    }
    return reinterpret_cast<void*>(base + fileOff);
}

// The Cocos runtime resolves these exports from libsecret.so through dlsym.
// SecShell_Require() returns zero when the shell is ready; the update gate
// additionally requires SecShell_IsReady() to be non-zero.
static int HookedSecShellRequireExport() {
    typedef int (*Fn)(void);
    Fn originalFn = g_origSecShellRequireExport
        ? g_origSecShellRequireExport
        : reinterpret_cast<Fn>(SecretExport(0x3D59C));
    const int original = originalFn ? originalFn() : -1;
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s shell export Require()=%d -> 0", DWDLM_LOG, original);
    }
    return 0;
}

static int HookedSecShellIsReadyExport() {
    typedef int (*Fn)(void);
    Fn originalFn = g_origSecShellIsReadyExport
        ? g_origSecShellIsReadyExport
        : reinterpret_cast<Fn>(SecretExport(0x3D68C));
    const int original = originalFn ? originalFn() : 0;
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s shell export IsReady()=%d -> 1", DWDLM_LOG, original);
    }
    return 1;
}

static int HookedSecShellIsDirtyExport() {
    typedef int (*Fn)(void);
    Fn originalFn = g_origSecShellIsDirtyExport
        ? g_origSecShellIsDirtyExport
        : reinterpret_cast<Fn>(SecretExport(0x3D6FC));
    const int original = originalFn ? originalFn() : 0;
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s shell export IsDirty()=%d -> 0", DWDLM_LOG, original);
    }
    return 0;
}

static int HookedSecShellCheckedMaskExport() {
    typedef int (*Fn)(void);
    Fn originalFn = g_origSecShellCheckedMaskExport
        ? g_origSecShellCheckedMaskExport
        : reinterpret_cast<Fn>(SecretExport(0x3D76C));
    const int original = originalFn ? originalFn() : 0;
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s shell export CheckedMask()=0x%X", DWDLM_LOG, (unsigned)original);
    }
    return 0x7EF;
}

static int HookedSecShellFailedMaskExport() {
    typedef int (*Fn)(void);
    Fn originalFn = g_origSecShellFailedMaskExport
        ? g_origSecShellFailedMaskExport
        : reinterpret_cast<Fn>(SecretExport(0x3D7CC));
    const int original = originalFn ? originalFn() : 0;
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s shell export FailedMask()=0x%X", DWDLM_LOG, (unsigned)original);
    }
    return 0;
}

static void* HookedSecShellTicketExport() {
    typedef void* (*Fn)(void*);
    Fn originalFn = g_origSecShellTicketExport
        ? reinterpret_cast<Fn>(g_origSecShellTicketExport)
        : reinterpret_cast<Fn>(SecretExport(0x3DA18));
    void* original = originalFn ? originalFn(nullptr) : nullptr;
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s shell export Ticket()=%p", DWDLM_LOG, original);
    }
    return original;
}

static int HookedSecShellDeriveKeyExport(const void* data, int length,
                                         void* output, int outputLength) {
    typedef int (*Fn)(const void*, int, void*, int);
    Fn originalFn = g_origSecShellDeriveKeyExport
        ? g_origSecShellDeriveKeyExport
        : reinterpret_cast<Fn>(SecretExport(0x3DB74));
    const int original = originalFn ? originalFn(data, length, output, outputLength) : 0;
    static std::atomic<int> logged{0};
    if (++logged <= 8) {
        LOGI("%s shell export DeriveKey()=%d", DWDLM_LOG, original);
    }
    return original;
}

static const char* RewriteCheckResult(const char* name, const char* result) {
    if (!name) return result;
    // This build has no server endpoint. Turning the original fail into
    // "pass" makes the shell require a token it can never obtain; "skip"
    // keeps the optional server gate disabled while local checks still run.
    if (strcmp(name, "server-verify") == 0) return "skip";
    // Preserve the shell's own kill-switch state instead of re-arming it.
    if (strcmp(name, "kill-on-fail") == 0) return result;
    if (result && strcmp(result, "fail") == 0) return "pass";
    return result;
}

static void HookedSecShellRequireRefined(const char* name, const char* result, const char* detail) {
    g_secureOverrideHits++;
    if (!g_origSecShellRequire) return;
    const char* rewritten = RewriteCheckResult(name, result);
    if (name && rewritten) {
        LOGI("%s secure check %s result=%s->%s", DWDLM_LOG, name,
             result ? result : "(null)", rewritten);
    }
    g_origSecShellRequire(name, rewritten, detail);
}

static bool InstallSecretHook(uintptr_t fileOff, void* hook, void** orig, const char* name) {
    char mapLine[768] = {0};
    void* target = FileOffToVa("libsecret.so", fileOff, mapLine, sizeof(mapLine));
    if (!target) {
        LOGW("%s %s file offset 0x%lX not mapped", DWDLM_LOG, name,
             (unsigned long)fileOff);
        return false;
    }
    if (DobbyHook(target, (dobby_dummy_func_t)hook, (dobby_dummy_func_t*)orig) != 0) {
        LOGW("%s DobbyHook %s failed", DWDLM_LOG, name);
        return false;
    }
    LOGI("%s hooked %s @%p map=%s", DWDLM_LOG, name, target,
         mapLine[0] ? mapLine : "(none)");
    return true;
}

static bool PatchSecretExitNop(uintptr_t fileOff, const char* name) {
    char mapLine[768] = {0};
    void* target = FileOffToVa("libsecret.so", fileOff, mapLine, sizeof(mapLine));
    if (!target) {
        LOGW("%s %s file offset 0x%lX not mapped", DWDLM_LOG, name,
             (unsigned long)fileOff);
        return false;
    }
    uint32_t insn = 0;
    memcpy(&insn, target, sizeof(insn));
    if ((insn & 0xFC000000u) != 0x94000000u) {
        LOGW("%s %s not a BL: 0x%08X", DWDLM_LOG, name, insn);
        return false;
    }
    uint32_t nop = 0xD503201Fu;
    if (DobbyCodePatch(target, (uint8_t*)&nop, sizeof(nop)) != 0) {
        LOGW("%s DobbyCodePatch %s failed", DWDLM_LOG, name);
        return false;
    }
    LOGI("%s patched %s exit call @%p", DWDLM_LOG, name, target);
    return true;
}

void Dwdlm_InstallPathProbe() { InstallPathProbe(); }

void Dwdlm_PrepareSecureShell() {
#ifdef DWDLM_DATA_GUARD_ONLY
    LOGI("%s data-guard only: start shell state polling", DWDLM_LOG);
    StartShellStateGuard();
    g_secureHookInstalled.store(true);
    return;
#endif
    LOGI("%s prepareSecureShell enter installed=%d", DWDLM_LOG,
         g_origSecShellRequire ? 1 : 0);
    if (g_origSecShellRequire) return;
    if (!kUseSecretTextHooks) {
        LOGW("%s secure text hooks disabled; preserving libsecret checks", DWDLM_LOG);
        g_secureHookInstalled.store(true);
        return;
    }
    // libsecret is loaded with System.loadLibrary before this hook runs. Resolve
    // by its ELF file offset because bionic may keep its symbols in a local ns.
    if (!InstallSecretHook(0x20008, (void*)HookedSecShellRequireRefined,
                           (void**)&g_origSecShellRequire, "check-report")) {
        return;
    }
    // Only bit 0x20000 is the log gate. Writing the whole word flips loader
    // strategy bits too (memfd-only), which breaks System.loadLibrary later.
    void* dbgFlags = ImageAddr("libsecret.so", 0x667B4);
    if (dbgFlags) {
        uint32_t flags = 0;
        memcpy(&flags, dbgFlags, sizeof(flags));
        flags |= 0x20000u;
        memcpy(dbgFlags, &flags, sizeof(flags));
        LOGI("%s secure log gate enabled flags=0x%X @%p", DWDLM_LOG, flags, dbgFlags);
    } else {
        LOGW("%s secure debug flags address missing", DWDLM_LOG);
    }
    InstallSecretHook(0x1D2AC, (void*)HookedOnShellCreate,
                      (void**)&g_origOnShellCreate, "onShellCreate");
    g_secCheckedMask = reinterpret_cast<volatile uint32_t*>(
        ImageAddr("libsecret.so", 0x70328));
    g_secFailedMask = reinterpret_cast<volatile uint32_t*>(
        ImageAddr("libsecret.so", 0x7032C));
    volatile uint32_t* dirty = reinterpret_cast<volatile uint32_t*>(
        ImageAddr("libsecret.so", 0x70318));
    LOGI("%s secure mask state dirty=%p failed=%p checked=%p", DWDLM_LOG,
         (void*)dirty, (void*)g_secFailedMask, (void*)g_secCheckedMask);
    PatchSecretExitNop(0x1ED8C, "exit-1");
    PatchSecretExitNop(0x20000, "exit-2");
    PatchSecretExitNop(0x202DC, "exit-3");
    PatchSecretExitNop(0x3D688, "exit-4");
#ifndef DWDLM_SHELL_STATE_ONLY
    if (!g_shellGateHooksInstalled.exchange(true)) {
        InstallSecretHook(0x3D59C, (void*)HookedSecShellRequireExport,
                          (void**)&g_origSecShellRequireExport, "export Require");
        InstallSecretHook(0x3D68C, (void*)HookedSecShellIsReadyExport,
                          (void**)&g_origSecShellIsReadyExport, "export IsReady");
        InstallSecretHook(0x3D6FC, (void*)HookedSecShellIsDirtyExport,
                          (void**)&g_origSecShellIsDirtyExport, "export IsDirty");
        InstallSecretHook(0x3D76C, (void*)HookedSecShellCheckedMaskExport,
                          (void**)&g_origSecShellCheckedMaskExport, "export CheckedMask");
        InstallSecretHook(0x3D7CC, (void*)HookedSecShellFailedMaskExport,
                          (void**)&g_origSecShellFailedMaskExport, "export FailedMask");
        InstallSecretHook(0x3DA18, (void*)HookedSecShellTicketExport,
                          (void**)&g_origSecShellTicketExport, "export Ticket");
        InstallSecretHook(0x3DB74, (void*)HookedSecShellDeriveKeyExport,
                          (void**)&g_origSecShellDeriveKeyExport, "export DeriveKey");
    }
#endif
    g_secureHookInstalled.store(true);
    LOGI("%s secure override install complete", DWDLM_LOG);
}

extern "C" JNIEXPORT void JNICALL
Java_com_starry_magic_dwdlm_DwdlmApplication_prepareSecureShell(JNIEnv*, jclass) {
    Dwdlm_PrepareSecureShell();
}
