/* ================================================================
 *  勇士之心 MOD - se::ScriptEngine::evalString splice
 *
 *  目标：TapTap com.fightingstars.games.warrior 1.0.3 / vc5
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
/* 模拟器侧引导脚本素材（由 04_scripts/勇士之心/ysxz_mainjs_patch.py 生成）：
   kYsxzBootPristine/_len = 底包 assets/main.js（加密态）
   kYsxzBootPatched/_len  = 同一明文 + 自举补丁后重新加密
   生成命令见 ysxz_build_so_103.sh；缺这个头文件就编不过（故意如此，防拿旧产物出包）。 */
#include "YsxzBootBlob.h"
/* 1.0.6（加密壳代）追加：
   YsxzSecShell —— 每 1ms 把壳的 dirty/failedMask 归零，避免重签+外科导致壳自检失败后
                   跳过 Application.onCreate ⇒ bingo SDK 未初始化 ⇒ AppInfo.parameters NPE。
                   在 1.0.3 明文包上找不到 libsecret.so 会自动退化为 idle，无副作用。
   YsxzExitTrace —— 退出点只观测（打印调用点），不改行为。 */
#if defined(__has_include)
#  if __has_include("Games/YsxzSecShell.h")
#    include "Games/YsxzSecShell.h"
#    define YSXZ_HAVE_SECSHELL 1
#  endif
#  if __has_include("Games/YsxzExitTrace.h")
#    include "Games/YsxzExitTrace.h"
#    define YSXZ_HAVE_EXITTRACE 1
#  endif
#else
#  include "Games/YsxzSecShell.h"
#  include "Games/YsxzExitTrace.h"
#  define YSXZ_HAVE_SECSHELL 1
#  define YSXZ_HAVE_EXITTRACE 1
#endif

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <string>
#include <sys/stat.h>
#include <pthread.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"

#define YSXZ_LOG "[Ysxz]"

static const char* kTargetLib() { return sec_lib_cocosjs(); }

static constexpr uintptr_t kEvalRva = 0x3138e4;
static constexpr uint32_t kEvalInsn = 0xD10543FFu;
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
    /* 2026-10-07 补 assets/main/index.：过检测闸门 ANTI_CHEAT_ENABLED 的宿主模块 $LZ
     * 在 main bundle 里(实测 main/index.js 19 处命中)，不放行则闸门层永远 arm 不上。
     * main 内容过滤已过关(window.__require / maxApDefRate / AUTO_BATTLE 齐备)。
     * JS 侧用 sealMethods(仅 main 独有)识别 main，走 gate-only，不跑功能扫描。 */
    return fileName && (strstr(fileName, "assets/battleLogic/index.") ||
                        strstr(fileName, "assets/game/index.") ||
                        strstr(fileName, "assets/main/index."));
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

static bool HookedEvalString(void* thiz, const char* script, long length, void* ret,
                             const char* fileName);

/* ======================================================================
 *  模拟器自举的两块地基（MuMu/Houdini 上原生 inline hook 装得上但不命中）
 *
 *  1) main.js 自愈：引擎真正加载的是 files/res/main.js。若它还是底包原样
 *     （或首启尚未解包、文件不存在），就把「尾附自举补丁」的那份写回去。
 *     只按字节比对已知 pristine/patched，不认识的版本一律不动。
 *  2) 生效证明：只有真的截获到 evalString 才写 files/ysxz_native_ok（内容=pid）。
 *     main.js 里的自举驱动启动时读它：同 pid ⇒ 原生路线已在工作，自举让位；
 *     读不到（含 MuMu 这种钩子不命中的情形）⇒ 自举接管。
 *  两条路线共享同一份补丁源，运行时判据选路，无机型/模拟器名单分叉。
 * ====================================================================== */
static void YsxzDataDir(char* out, size_t cap) {
    if (!out || cap < 32) return;
    out[0] = 0;
    FILE* fp = fopen("/proc/self/cmdline", "rb");
    if (!fp) return;
    char buf[256];
    memset(buf, 0, sizeof(buf));
    const size_t got = fread(buf, 1, sizeof(buf) - 1, fp);
    fclose(fp);
    if (got == 0) return;
    char* colon = strchr(buf, ':');   /* com.pkg:remote -> com.pkg */
    if (colon) *colon = 0;
    if (!buf[0]) return;
    snprintf(out, cap, "/data/data/%s", buf);
    if (access(out, F_OK) != 0) snprintf(out, cap, "/data/user/0/%s", buf);
}

static bool YsxzReadWholeFile(const char* path, std::string* out) {
    FILE* fp = fopen(path, "rb");
    if (!fp) return false;
    out->clear();
    char buf[4096];
    size_t n = 0;
    while ((n = fread(buf, 1, sizeof(buf), fp)) > 0) out->append(buf, n);
    fclose(fp);
    return true;
}

static bool YsxzWriteWholeFile(const char* path, const unsigned char* data, size_t len) {
    std::string tmp = std::string(path) + ".starrytmp";
    FILE* fp = fopen(tmp.c_str(), "wb");
    if (!fp) return false;
    const size_t wrote = fwrite(data, 1, len, fp);
    fclose(fp);
    if (wrote != len) { unlink(tmp.c_str()); return false; }
    chmod(tmp.c_str(), 0600);
    if (rename(tmp.c_str(), path) != 0) { unlink(tmp.c_str()); return false; }
    return true;
}

/* 状态: 0=未判 1=已是补丁版 2=本轮刚写 3=陌生产物（不动） */
static void YsxzBootSelfHealOnce(const char* dataDir, int* state) {
    if (!dataDir || !dataDir[0]) return;
    char path[512];
    snprintf(path, sizeof(path), "%s/files/res/main.js", dataDir);
    std::string cur;
    if (!YsxzReadWholeFile(path, &cur)) {
        if (YsxzWriteWholeFile(path, kYsxzBootPatched, kYsxzBootPatched_len)) {
            LOGI("%s boot self-heal: main.js missing -> wrote patched (%u B)", YSXZ_LOG,
                 kYsxzBootPatched_len);
            *state = 1;
        }
        return;
    }
    if (cur.size() == kYsxzBootPatched_len &&
        memcmp(cur.data(), kYsxzBootPatched, kYsxzBootPatched_len) == 0) {
        if (*state != 1) LOGI("%s boot self-heal: main.js already patched", YSXZ_LOG);
        *state = 1;
        return;
    }
    if (cur.size() == kYsxzBootPristine_len &&
        memcmp(cur.data(), kYsxzBootPristine, kYsxzBootPristine_len) == 0) {
        if (YsxzWriteWholeFile(path, kYsxzBootPatched, kYsxzBootPatched_len)) {
            LOGI("%s boot self-heal: pristine -> patched (%u B)", YSXZ_LOG,
                 kYsxzBootPatched_len);
            *state = 2;
        }
        return;
    }
    /* 覆盖安装（保数据）后残留的是【上一版】自举补丁 main.js：既不等于本版 pristine
       也不等于本版 patched，旧 driver 会继续跑（2026-10-06 MuMu 实测：旧自举仍锁
       .bJV 假伤害公式 -> 秒杀/伤害倍率失效）。main.js 不在服务端热更名单（只有 4 个
       index.js bundle），运行时只可能是官方原样或本 MOD 写过的补丁 ⇒ 任何 *ze* 且非
       pristine 的形态都按“上一版自举”重写为本版 patched。
       注：main.js 是加密态，明文里的 boot 标记搜不到，只能用 *ze* + 非 pristine 判。 */
    if (cur.size() > 5 && memcmp(cur.data(), "*ze*", 4) == 0) {
        if (YsxzWriteWholeFile(path, kYsxzBootPatched, kYsxzBootPatched_len)) {
            LOGI("%s boot self-heal: stale boot patch %zu B -> patched (%u B)", YSXZ_LOG,
                 cur.size(), kYsxzBootPatched_len);
            *state = 2;
        }
        return;
    }
    if (*state != 3) {
        LOGW("%s boot self-heal: main.js unknown version (%zu B), left untouched", YSXZ_LOG,
             cur.size());
    }
    *state = 3;
}

static std::atomic<bool> g_proofWritten{false};

static void YsxzWriteNativeProof() {
    char dir[384] = {0};
    YsxzDataDir(dir, sizeof(dir));
    if (!dir[0]) return;
    char path[512];
    snprintf(path, sizeof(path), "%s/files/ysxz_native_ok", dir);
    FILE* fp = fopen(path, "wb");
    if (!fp) return;
    fprintf(fp, "%d\n", (int)getpid());
    fclose(fp);
}

static void* BootSelfHealThread(void* arg) {
    (void)arg;
    char dir[384] = {0};
    YsxzDataDir(dir, sizeof(dir));
    /* 先清上一进程残留的「原生已生效」证明，避免误导 main.js 里的自举判据 */
    if (dir[0]) {
        const std::string proof = std::string(dir) + "/files/ysxz_native_ok";
        unlink(proof.c_str());
    }
    int state = 0;
    bool loggedDir = false;
    for (int i = 0; i < 90; i++) {   /* 90s 看门狗：覆盖首启解包/热更覆盖 */
        if (dir[0] && access(dir, F_OK) == 0) {
            if (!loggedDir) {
                LOGI("%s boot self-heal: dir=%s", YSXZ_LOG, dir);
                loggedDir = true;
            }
            YsxzBootSelfHealOnce(dir, &state);
        }
        usleep(1000 * 1000);
    }
    return nullptr;
}

/* 2026-10-26 1.0.3（vc3 / TapTap .tt1 底包）实测：
 *   本包 libcocos2djs.so **导出**了 _ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_
 *   （.dynsym -> .text，1488 B）；而静态 RVA 0x3138e4 上放的是普通字符串
 *   （"internal40Load_G"）⇒ 老守卫必然失配、原生 splice 永不挂上（伤害/技能/无敌全灭）。
 *   按 hook-rules §24「不写死会被重编挪走的定位」改成
 *   **动态符号候选优先 + RVA/序言兜底**：符号命中就用符号（跨版本最稳），
 *   命不中再走老 RVA 路径，两条都不通才放弃（此时只能靠 main.js 自举路线）。
 *   1.0.6（加密壳代）符号被 strip，自然落到 RVA 分支，行为与原来一致。 */
static const char* kEvalSyms[] = {
    "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_",   // (const char*, long, Value*, Value*)
    "_ZN2se12ScriptEngine10evalStringEPKcmPNS_5ValueES2_",   // (const char*, unsigned long, Value*, Value*)
    "_ZN2se12ScriptEngine10evalStringEPKcPNS_5ValueES3_",    // (const char*, Value*, Value*)
    "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_b",  // 尾参 bool 变体
};
static const char* kEvalSymJni =
    "Java_org_cocos2dx_lib_Cocos2dxJavascriptJavaBridge_evalString";  // 仅佐证，不作 splice 点

static void* ResolveEvalString(const char* lib, const char** via, bool* viaReadable) {
    if (via) *via = "rva";
    if (viaReadable) *viaReadable = false;
    for (const char* sym : kEvalSyms) {
        void* p = getSymAddress(lib, sym, false);
        if (!p) continue;
        if (via) *via = "dynsym";
        LOGI("%s evalString resolved by dynsym: %s", YSXZ_LOG, sym);
        return p;
    }
    if (getSymAddress(lib, kEvalSymJni, false)) {
        LOGI("%s note: JNI evalString bridge present (不用作 splice 点)", YSXZ_LOG);
    }
    void* va = FileOffToVa(lib, kEvalRva, viaReadable);
    if (!va) return nullptr;
    uint32_t got = 0;
    memcpy(&got, va, sizeof(got));
    if (got != kEvalInsn) {
        // 重编后 se::evalString 已不在 kEvalRva 上 ⇒ 原生 splice 不挂，改由 JS 自举路线
        // （files/res/main.js 尾部补丁）生效。重试循环每秒走很多次，日志必须限速。
        static std::atomic<int> s_drift{0};
        const int n = ++s_drift;
        if (n <= 3 || (n % 200) == 0) {
            LOGW("%s evalString prologue drift got=0x%08X want=0x%08X (#%d, 本版走 JS 自举路线)",
                 YSXZ_LOG, got, kEvalInsn, n);
        }
        return nullptr;
    }
    if (via) *via = (viaReadable && *viaReadable) ? "readable" : "rx";
    return va;
}

static bool HookEvalString() {
    if (g_hooked.load()) return true;
    const char* lib = kTargetLib();
    if (!isLibraryLoaded(lib)) return false;

    const char* via = "rva";
    bool viaReadable = false;
    void* va = ResolveEvalString(lib, &via, &viaReadable);
    if (!va) {
        LOGW("%s evalString not found (dynsym + rva both miss, rva=0x%lX)", YSXZ_LOG,
             (unsigned long)kEvalRva);
        return false;
    }
    if (DobbyHook(va, (dobby_dummy_func_t)HookedEvalString, (dobby_dummy_func_t*)&g_origSeEval) != 0) {
        LOGW("%s DobbyHook evalString failed @%p", YSXZ_LOG, va);
        return false;
    }
    g_hooked.store(true);
    LOGI("%s SELFCHECK evalString va=%p via=%s rd=1 hook=1", YSXZ_LOG, va, via);
    LOGI("%s hooked se::evalString @%p via=%s", YSXZ_LOG, va, via);
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
    /* 真的截获到了 ⇒ 原生路线在这台机器上有效；写 pid 证明给 main.js 里的自举驱动看 */
    if (!g_proofWritten.exchange(true)) YsxzWriteNativeProof();
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
    // 1.0.6 加密壳：退出点取证 + 壳状态净化。
    // 正常路径由 com.starry.magic.ysxz.YsxzAppComponentFactory 在 ~365ms（早于壳自检 413ms）
    // 先起一次；这里兜底，保证任何入口（含 OTA 热更 so / 直接 dlopen）都能起。
#ifdef YSXZ_HAVE_EXITTRACE
    YsxzExitTrace_Start(vm);
#endif
#ifdef YSXZ_HAVE_SECSHELL
    YsxzSecShell_Start();
#endif
    static std::atomic<bool> started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
    }
    pthread_t bh;
    if (pthread_create(&bh, nullptr, BootSelfHealThread, nullptr) == 0) pthread_detach(bh);
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
