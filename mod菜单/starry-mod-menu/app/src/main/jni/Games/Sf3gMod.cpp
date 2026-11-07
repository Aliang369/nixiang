/* ================================================================
 *  Sf3gMod.cpp — 暗影格斗3 国际服（com.nekki.shadowfight3 / Google Play 1.46.1）
 *
 *  第一版：**只做 il2cpp 清单探针**（不装任何玩法 hook）
 *    - 运行时遍历 domain/assemblies/classes/fields/methods
 *    - 落盘 /sdcard/Android/data/com.nekki.shadowfight3/files/sf3g_dump.txt
 *    - 每条方法给 RVA（相对 libil2cpp 基址）+ 参数类型 + static 标志
 *  用途：国际服与国服不同源（metadata v39 / 混淆名全换），必须重新定位玩法入口。
 *
 *  设计铁律（沿用国服 Sf3Mod 骨架）：
 *    - 只在 api.ready 后动；dump 失败不重试风暴
 *    - 不装 hook、不写游戏内存（本版纯只读）
 *    - 异常兜底：文件打不开就放弃，不崩游戏
 * ================================================================ */

#include "Sf3gMod.h"

#include <atomic>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <dlfcn.h>
#include <link.h>
#include <mutex>
#include <unistd.h>
#include <string>

#include "Dobby/dobby.h"
#include "xDL/xdl.h"
#include <unwind.h>
#include <cstdarg>
#include <fcntl.h>
#include <algorithm>
#include <string>
#include <vector>
#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

#define SF3G_LOG "[sf3g]"
#define SF3G_DUMP_PATH "/sdcard/Android/data/com.nekki.shadowfight3/files/sf3g_dump.txt"

using Il2Cpp::GetApi;

static std::atomic<bool> g_apiReady{false};
static std::atomic<bool> g_dumpTried{false};
static std::atomic<bool> g_dumped{false};
static std::atomic<int>  g_ticks{0};
static std::atomic<int>  g_dmgMult{1};   // 验证期关闭
static std::atomic<bool> g_god{false}; // 验证期关闭
static uintptr_t g_il2cppBase = 0;
static void sf3gWriteFile(const char* name, const char* text) {
    char path[256];
    snprintf(path, sizeof(path), "/sdcard/Android/data/com.nekki.shadowfight3/files/%s", name);
    FILE* f = fopen(path, "a");
    if (!f) return;
    fprintf(f, "%s\n", text);
    fclose(f);
}

extern "C" void Sf3g_StartEarlyHooks();
static void sf3gInstallHideSelf();
static void sf3gInstallHidePhdr();

__attribute__((constructor)) static void sf3gCtor() {
    sf3gWriteFile("sf3g_ctor.txt", "ctor: libstarry_magic loaded");
    Sf3g_StartEarlyHooks();   // wrap/LD_PRELOAD 模式没人调 JNI_OnLoad，这里自己起
    sf3gInstallHideSelf();    // 先把自重隐藏挂上，早于游戏任何校验
    sf3gInstallHidePhdr();    // 兼堵 dl_iterate_phdr 枚举路径
}


static int findIl2cppCb(struct dl_phdr_info* info, size_t, void*) {
    if (info && info->dlpi_name && strstr(info->dlpi_name, "libil2cpp.so")) {
        g_il2cppBase = (uintptr_t)info->dlpi_addr;
        return 1;
    }
    return 0;
}

static bool nsSkipped(const char* ns) {
    static const char* kSkip[] = {
        "System", "UnityEngine", "Unity", "Mono", "Microsoft", "JetBrains",
        "Newtonsoft", "Firebase", "Google", "Facebook", "Amazon", "Tapjoy",
        "AppLovin", "Java", "Org.", "com.", "Apple", "Nekki.Firebase",
    };
    if (!ns || !*ns) return false;
    for (size_t i = 0; i < sizeof(kSkip) / sizeof(kSkip[0]); i++) {
        size_t n = strlen(kSkip[i]);
        if (strncmp(ns, kSkip[i], n) == 0) return true;
    }
    return false;
}

/* 一次性 dump：只导出游戏自身命名空间的类/字段/方法 */
static bool dumpIl2Cpp() {
    auto& api = GetApi();
    if (!api.ready) { sf3gWriteFile("sf3g_prog.txt", "not ready"); return false; }
    if (!api.domain_get || !api.domain_assemblies) { sf3gWriteFile("sf3g_prog.txt", "no domain api"); return false; }
    void* dom = api.domain_get();
    if (!dom) { sf3gWriteFile("sf3g_prog.txt", "domain null"); return false; }
    if (api.thread_attach) api.thread_attach(dom);
    sf3gWriteFile("sf3g_prog.txt", "domain ok, start dump");

    void* domain = api.domain_get ? api.domain_get() : nullptr;
    if (api.thread_attach && domain) api.thread_attach(domain);
    dl_iterate_phdr(findIl2cppCb, nullptr);

    FILE* f = fopen(SF3G_DUMP_PATH, "w");
    if (!f) {
        LOGE("%s dump: open %s failed", SF3G_LOG, SF3G_DUMP_PATH);
        return false;
    }

    size_t asmCount = 0;
    sf3gWriteFile("sf3g_prog.txt", "before Assemblies");
    void** asms = api.Assemblies(&asmCount);
    {
        char b[96];
        snprintf(b, sizeof(b), "assemblies=%zu ptr=%p", asmCount, (void*)asms);
        sf3gWriteFile("sf3g_prog.txt", b);
    }
    fprintf(f, "# il2cpp_base=0x%llx assemblies=%zu\n", (unsigned long long)g_il2cppBase, asmCount);

    int classes = 0, methods = 0, fields = 0;
    for (size_t i = 0; i < asmCount; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img || !api.image_class_count || !api.image_class) continue;
        uint32_t cc = api.image_class_count(img);
        if (i < 3) {
            char b[96];
            snprintf(b, sizeof(b), "asm[%zu] classes=%u", i, cc);
            sf3gWriteFile("sf3g_prog.txt", b);
        }
        for (uint32_t j = 0; j < cc; j++) {
            void* k = api.image_class(img, j);
            if (!k || !api.class_name) continue;
            const char* ns = api.class_namespace ? api.class_namespace(k) : "";
            if (nsSkipped(ns)) continue;
            const char* cn = api.class_name(k);
            if (!cn) continue;
            if (classes < 3) {
                char b[160];
                snprintf(b, sizeof(b), "class[%d] %s::%s", classes, (ns && *ns) ? ns : "", cn);
                sf3gWriteFile("sf3g_prog.txt", b);
            }

            fprintf(f, "\nC %s::%s", (ns && *ns) ? ns : "", cn);
            if (api.class_parent) {
                void* p = api.class_parent(k);
                const char* pn = p && api.class_name ? api.class_name(p) : nullptr;
                const char* pns = (p && api.class_namespace) ? api.class_namespace(p) : "";
                if (pn) fprintf(f, " parent=%s::%s", (pns && *pns) ? pns : "", pn);
            }
            fprintf(f, "\n");
            classes++;
            if (classes <= 5) {
                char b[96]; snprintf(b, sizeof(b), "emit class %d", classes);
                sf3gWriteFile("sf3g_prog.txt", b);
            }
            fflush(f);

            if (classes <= 5) sf3gWriteFile("sf3g_prog.txt", "  fields begin");
            if (api.class_fields && api.field_name) {
                void* it = nullptr;
                while (void* fld = api.class_fields(k, &it)) {
                    size_t off = api.field_offset ? api.field_offset(fld) : 0;
                    const char* fn = api.field_name(fld);
                    char* tn = nullptr;
                    if (api.field_type && api.type_name) {
                        void* t = api.field_type(fld);
                        if (t) tn = api.type_name(t);
                    }
                    fprintf(f, "F %s off=%zu %s\n", fn ? fn : "?", off, tn ? tn : "?");
                    /* 不 free：il2cpp_type_get_name 部分版本返回内部缓冲，释放会损坏堆 */
                    fields++;
                }
            }

            if (api.class_methods && api.method_get_name) {
                void* it = nullptr;
                while (void* m = api.class_methods(k, &it)) {
                    const char* mn = api.method_get_name(m);
                    uint32_t pc = api.method_param_count ? api.method_param_count(m) : 0;
                    uint32_t iflags = 0;
                    uint32_t flags = api.method_flags ? api.method_flags(m, &iflags) : 0;
                    void* ptr = *(void**)m;
                    long long rva = (long long)((uintptr_t)ptr - g_il2cppBase);
                    fprintf(f, "M %s/%u rva=0x%llx%s", mn ? mn : "?", pc, rva,
                            (flags & 0x10) ? " static" : "");
                    for (uint32_t q = 0; q < pc; q++) {
                        char* tn = nullptr;
                        if (api.method_param && api.type_name) {
                            void* t = api.method_param(m, q);
                            if (t) tn = api.type_name(t);
                        }
                        fprintf(f, " (%s)", tn ? tn : "?");
                        /* 不 free：il2cpp_type_get_name 部分版本返回内部缓冲，释放会损坏堆 */
                    }
                    if (api.method_ret_type && api.type_name) {
                        void* rt = api.method_ret_type(m);
                        char* rn = rt ? api.type_name(rt) : nullptr;
                        fprintf(f, " -> %s", rn ? rn : "?");

                    }
                    fprintf(f, "\n");
                    methods++;
                }
            }
        }
    }
    fprintf(f, "\n# end classes=%d methods=%d fields=%d\n", classes, methods, fields);
    fclose(f);
    LOGI("%s dump done: classes=%d methods=%d fields=%d -> %s", SF3G_LOG, classes, methods, fields, SF3G_DUMP_PATH);
    char buf[128];
    snprintf(buf, sizeof(buf), "dump done classes=%d methods=%d fields=%d", classes, methods, fields);
    sf3gWriteFile("sf3g_prog.txt", buf);
    return true;
}

extern "C" void Sf3g_StartEarlyHooks();

void Sf3g_OnLoad(JavaVM* vm) {
    (void)vm;
    sf3gWriteFile("sf3g_onload.txt", "onLoad called");
    Sf3g_StartEarlyHooks();
    LOGI("%s onLoad", SF3G_LOG);
}

static void sf3gInstallHooks();
static void sf3gInstallCodeHashHook();
static void sf3gInstallActkProbe();
static void sf3gInstallSumHook();
static void sf3gDumpBt(const char* tag);
static void sf3gInstallHideSelf();
static JavaVM* g_sf3gVm = nullptr;

static std::atomic<int> g_tryCount{0};

bool Sf3g_TryInit(JavaVM* vm) {
    if (vm) g_sf3gVm = vm;
    int n = ++g_tryCount;
    auto& api = GetApi();
    if (!api.ready && !api.Load()) {
        if (n <= 5) {
            char buf[160];
            snprintf(buf, sizeof(buf), "try#%d Load=false cn=%d cf=%d fo=%d dg=%d da=%d",
                     n, api.class_name != nullptr, api.class_get_field != nullptr,
                     api.field_offset != nullptr, api.domain_get != nullptr,
                     api.domain_assemblies != nullptr);
            sf3gWriteFile("sf3g_try.txt", buf);
        }
        return false;
    }
    g_apiReady = true;
    sf3gWriteFile("sf3g_init.txt", "tryInit: il2cpp api ready");
    sf3gInstallHooks();
    // sf3gInstallCodeHashHook();  // 2026-11-07: 探针会干扰 ACTk 校验，正式版关闭
    // sf3gInstallActkProbe();  // 2026-11-07: 探针会干扰 ACTk 校验，正式版关闭
    // sf3gInstallSumHook();  // 2026-11-07: 探针会干扰 ACTk 校验，正式版关闭
    LOGI("%s il2cpp api ready", SF3G_LOG);
    return true;
}

bool Sf3g_IsReady() { return g_apiReady.load(); }

void Sf3g_OnFeatureChanged(int featNum, bool enabled) {
    LOGI("%s feat %d -> %d", SF3G_LOG, featNum, (int)enabled);
}

void Sf3g_OnValueChanged(int featNum, int value) {
    LOGI("%s feat %d value %d", SF3G_LOG, featNum, value);
}

void Sf3g_Tick(const bool* features, const int* values) {
    (void)features; (void)values;
    int t = ++g_ticks;
    /* 等 15 秒（75 tick）再 dump，避开启动期半初始化 */
    if (!g_apiReady.load() || g_dumpTried.load() || t < 75) return;
    g_dumpTried = true;
    dumpIl2Cpp();
    g_dumped = true;
}

void Sf3g_ForceClear() {
    // g_dmgMult / g_god 保持编译期默认（wrap 模式无菜单 UI）
}

// ===== 探针通道：由 ProbeProvider 直接调用，不依赖菜单框架 =====
static void* sf3gDumpThread(void*) {
    for (int i = 0; i < 60; i++) {
        sleep(2);
        auto& api = GetApi();
        if (!api.ready) api.Load();
        if (!api.ready) continue;
        if (dumpIl2Cpp()) break;
    }
    return nullptr;
}

extern "C" JNIEXPORT void JNICALL
Java_com_starry_probe_ProbeProvider_startDump(JNIEnv* env, jclass, jobject ctx) {
    (void)env; (void)ctx;
    sf3gWriteFile("sf3g_probe.txt", "startDump called");
    pthread_t tid;
    pthread_create(&tid, nullptr, sf3gDumpThread, nullptr);
    pthread_detach(tid);
}

// ============================================================
//  战斗入口探针：FJEKNKHKIMM 的三个 5 参同签名方法
//    签名 (CJDJNHOONFB p1, PMGOJMDNEFD unit, ABDBPCCGAJP value, string, Action<...>)
//    self+0x68(104) = 玩家 unit，self+0x70(112) = 敌人 unit（与国服同构）
//  本版只记录，不改值。
// ============================================================
typedef void (*Sf3gChangeLife_t)(void* self, void* a1, void* a2, void* unit, void* value,
                                 void* a5, void* a6, void* a7);
static Sf3gChangeLife_t g_origA = nullptr;
static Sf3gChangeLife_t g_origB = nullptr;
static Sf3gChangeLife_t g_origC = nullptr;
static std::atomic<int> g_hitA{0}, g_hitB{0}, g_hitC{0};

enum : size_t { kOffPlayerUnit = 104, kOffEnemyUnit = 112 };

static void sf3gLogHit(const char* tag, std::atomic<int>& counter,
                       void* self, void* unit, void* value) {
    int n = ++counter;
    if (n > 8) return;
    char buf[320];
    void* player = nullptr;
    void* enemy = nullptr;
    if (self) {
        player = *(void**)((char*)self + kOffPlayerUnit);
        enemy = *(void**)((char*)self + kOffEnemyUnit);
    }
    snprintf(buf, sizeof(buf), "%s#%d self=%p player=%p enemy=%p unit=%p value=%p isPlayer=%d isEnemy=%d",
             tag, n, self, player, enemy, unit, value,
             (player && unit == player) ? 1 : 0,
             (enemy && unit == enemy) ? 1 : 0);
    sf3gWriteFile("sf3g_hits.txt", buf);
}


// ===== 伤害应用：value 是 ABDBPCCGAJP 容器，用 get/set（rva 0x67686e0 / 0x6768754）=====
typedef float (*Sf3gGetDmg_t)(void*);
typedef void  (*Sf3gSetDmg_t)(void*, float);
static Sf3gGetDmg_t g_getDmg = nullptr;
static Sf3gSetDmg_t g_setDmg = nullptr;

static void sf3gEnsureDmgFns() {
    if (g_getDmg || !g_il2cppBase) return;
    g_getDmg = (Sf3gGetDmg_t)(g_il2cppBase + 0x67686e0);
    g_setDmg = (Sf3gSetDmg_t)(g_il2cppBase + 0x6768754);
}

static void sf3gApplyDamage(const char* tag, void* self, void* unit, void* value) {
    if (!self || !unit || !value) return;
    int mult = g_dmgMult.load();
    bool god = g_god.load();
    if (mult <= 1 && !god) return;
    sf3gEnsureDmgFns();
    if (!g_getDmg || !g_setDmg) return;
    void* player = *(void**)((char*)self + 104);
    void* enemy  = *(void**)((char*)self + 112);
    bool isP = (player && unit == player);
    bool isE = (enemy && unit == enemy);
    if (!isP && !isE) return;
    float cur = g_getDmg(value);
    char b[128];
    if (isE && mult > 1) {
        g_setDmg(value, cur * (float)mult);
        snprintf(b, sizeof(b), "%s ENEMY %.3f -> %.3f", tag, cur, cur * mult);
        sf3gWriteFile("sf3g_dmg.txt", b);
    } else if (isP && god) {
        g_setDmg(value, 0.0f);
        snprintf(b, sizeof(b), "%s PLAYER %.3f -> 0", tag, cur);
        sf3gWriteFile("sf3g_dmg.txt", b);
    }
}

static void sf3gHookA(void* self, void* a1, void* a2, void* unit, void* value, void* a5, void* a6, void* a7) {
    sf3gLogHit("A", g_hitA, self, unit, value);
    sf3gApplyDamage("A", self, unit, value);
    if (g_origA) g_origA(self, a1, a2, unit, value, a5, a6, a7);
}
static void sf3gHookB(void* self, void* a1, void* a2, void* unit, void* value, void* a5, void* a6, void* a7) {
    sf3gLogHit("B", g_hitB, self, unit, value);
    sf3gApplyDamage("B", self, unit, value);
    if (g_origB) g_origB(self, a1, a2, unit, value, a5, a6, a7);
}
static void sf3gHookC(void* self, void* a1, void* a2, void* unit, void* value, void* a5, void* a6, void* a7) {
    sf3gLogHit("C", g_hitC, self, unit, value);
    sf3gApplyDamage("C", self, unit, value);
    if (g_origC) g_origC(self, a1, a2, unit, value, a5, a6, a7);
}

static std::atomic<bool> g_hooksInstalled{false};

static void sf3gInstallHooks() {
    if (g_hooksInstalled.load()) return;
    if (!g_il2cppBase) dl_iterate_phdr(findIl2cppCb, nullptr);
    if (!g_il2cppBase) { sf3gWriteFile("sf3g_hooks.txt", "no il2cpp base"); return; }
    struct { uintptr_t rva; void* fn; Sf3gChangeLife_t* orig; const char* name; } tbl[] = {
        {0x677f184, (void*)sf3gHookA, &g_origA, "A:677f184"},
        {0x6780150, (void*)sf3gHookB, &g_origB, "B:6780150"},
        {0x677f314, (void*)sf3gHookC, &g_origC, "C:677f314"},
    };
    for (auto& e : tbl) {
        void* target = (void*)(g_il2cppBase + e.rva);
        int rc = DobbyHook(target, e.fn, (void**)e.orig);
        char buf[96];
        snprintf(buf, sizeof(buf), "hook %s rc=%d orig=%p", e.name, rc, (void*)*e.orig);
        sf3gWriteFile("sf3g_hooks.txt", buf);
    }
    g_hooksInstalled = true;
}

// ============================================================
//  ACTk CodeHash 修正：把「我们改过的东西」还原成官方值再算 summary
//    - 跳过 libstarry_magic.so / libc++_shared.so（官方没有）
//    - classes9.dex 的 hash 换成官方原版（我们注入过菜单）
//  算法（逆向自 net.codestage.actk.androidnative.CodeHashGenerator）：
//    每文件 SHA-1 → 列表排序 → 拼接 → 再 SHA-1（大写 hex）
// ============================================================
static const char* kOrigClasses9Sha1 = "8B6FF438CB8009C901E21B5851AC14247C11B37E";

static std::string sf3gJStr(JNIEnv* env, jstring s) {
    if (!s) return std::string();
    const char* c = env->GetStringUTFChars(s, nullptr);
    std::string r = c ? c : "";
    if (c) env->ReleaseStringUTFChars(s, c);
    return r;
}

static std::string sf3gSha1HexUpper(JNIEnv* env, const std::string& data) {
    std::string out;
    jclass mdCls = env->FindClass("java/security/MessageDigest");
    if (!mdCls) return out;
    jmethodID getInst = env->GetStaticMethodID(mdCls, "getInstance",
                                               "(Ljava/lang/String;)Ljava/security/MessageDigest;");
    jmethodID update = env->GetMethodID(mdCls, "update", "([B)V");
    jmethodID digest = env->GetMethodID(mdCls, "digest", "()[B");
    if (!getInst || !update || !digest) return out;
    jstring alg = env->NewStringUTF("SHA-1");
    jobject inst = env->CallStaticObjectMethod(mdCls, getInst, alg);
    env->DeleteLocalRef(alg);
    if (!inst || env->ExceptionCheck()) { env->ExceptionClear(); return out; }
    jbyteArray arr = env->NewByteArray((jsize)data.size());
    env->SetByteArrayRegion(arr, 0, (jsize)data.size(), (const jbyte*)data.data());
    env->CallVoidMethod(inst, update, arr);
    env->DeleteLocalRef(arr);
    jbyteArray dg = (jbyteArray)env->CallObjectMethod(inst, digest);
    env->DeleteLocalRef(inst);
    if (!dg || env->ExceptionCheck()) { env->ExceptionClear(); return out; }
    jsize n = env->GetArrayLength(dg);
    std::vector<jbyte> buf(n > 0 ? n : 0);
    if (n > 0) env->GetByteArrayRegion(dg, 0, n, buf.data());
    env->DeleteLocalRef(dg);
    static const char* HEX = "0123456789ABCDEF";
    for (jsize i = 0; i < n; i++) {
        unsigned char b = (unsigned char)buf[i];
        out.push_back(HEX[b >> 4]);
        out.push_back(HEX[b & 0xF]);
    }
    return out;
}

typedef void (*Sf3gOnSuccess_t)(void* self, void* summary, void* hashes, void* paths, void* err, void* mi);
static Sf3gOnSuccess_t g_origOnSuccess = nullptr;
static std::atomic<int> g_hashCalls{0};

static void sf3gFixCodeHash(void* self, void* summary, void* hashes, void* paths, void* err, void* mi) {
    JNIEnv* env = nullptr;
    if (g_sf3gVm) g_sf3gVm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (!env) { if (g_origOnSuccess) g_origOnSuccess(self, summary, hashes, paths, err, mi); return; }

    jobjectArray jh = (jobjectArray)hashes;
    jobjectArray jp = (jobjectArray)paths;
    if (!jp || !jh) { if (g_origOnSuccess) g_origOnSuccess(self, summary, hashes, paths, err, mi); return; }

    jsize n = env->GetArrayLength(jp);
    jsize nh = env->GetArrayLength(jh);
    std::vector<std::string> keep;
    std::string dump;
    for (jsize i = 0; i < n && i < nh; i++) {
        jstring jpItem = (jstring)env->GetObjectArrayElement(jp, i);
        jstring jhItem = (jstring)env->GetObjectArrayElement(jh, i);
        std::string p = sf3gJStr(env, jpItem);
        std::string h = sf3gJStr(env, jhItem);
        env->DeleteLocalRef(jpItem);
        env->DeleteLocalRef(jhItem);
        dump += p + " " + h + "\n";
        if (p.find("libstarry_magic.so") != std::string::npos ||
            p.find("libc++_shared.so") != std::string::npos) {
            continue;
        }
        if (p.find("classes9.dex") != std::string::npos) h = kOrigClasses9Sha1;
        keep.push_back(h);
    }
    if (g_hashCalls.fetch_add(1) < 3) sf3gWriteFile("sf3g_codepaths.txt", dump.c_str());

    std::sort(keep.begin(), keep.end());
    std::string joined;
    for (auto& s : keep) joined += s;
    std::string fixed = sf3gSha1HexUpper(env, joined);
    std::string old = sf3gJStr(env, (jstring)summary);
    char info[192];
    snprintf(info, sizeof(info), "n=%d old=%s fixed=%s", (int)keep.size(), old.c_str(), fixed.c_str());
    sf3gWriteFile("sf3g_codesum.txt", info);

    jstring newSum = fixed.empty() ? nullptr : env->NewStringUTF(fixed.c_str());
    if (g_origOnSuccess) g_origOnSuccess(self, newSum ? (void*)newSum : summary, hashes, paths, err, mi);
    if (newSum) env->DeleteLocalRef(newSum);
}

static void sf3gInstallCodeHashHook() {
    if (!g_il2cppBase) dl_iterate_phdr(findIl2cppCb, nullptr);
    if (!g_il2cppBase) return;
    int rc = DobbyHook((void*)(g_il2cppBase + 0x3773dc8), (void*)sf3gFixCodeHash, (void**)&g_origOnSuccess);
    char buf[96];
    snprintf(buf, sizeof(buf), "codehash hook rc=%d orig=%p", rc, (void*)g_origOnSuccess);
    sf3gWriteFile("sf3g_hooks.txt", buf);
}

// ===== ACTk 取结果探针：确认 sum 是否来自 CodeHashGenerator =====
typedef void* (*Sf3gLastResult_t)(void* self, void* mi);
static Sf3gLastResult_t g_origLastResult = nullptr;
static std::atomic<int> g_lastCalls{0};

static void* sf3gHookLastResult(void* self, void* mi) {
    void* r = g_origLastResult ? g_origLastResult(self, mi) : nullptr;
    int n = ++g_lastCalls;
    if (n <= 5) {
        char buf[96];
        snprintf(buf, sizeof(buf), "get_LastResult#%d ret=%p", n, r);
        sf3gWriteFile("sf3g_actk.txt", buf);
    }
    return r;
}

static void sf3gInstallActkProbe() {
    if (!g_il2cppBase) dl_iterate_phdr(findIl2cppCb, nullptr);
    if (!g_il2cppBase) return;
    int rc = DobbyHook((void*)(g_il2cppBase + 0x3772094), (void*)sf3gHookLastResult, (void**)&g_origLastResult);
    char buf[96];
    snprintf(buf, sizeof(buf), "lastResult hook rc=%d orig=%p", rc, (void*)g_origLastResult);
    sf3gWriteFile("sf3g_actk.txt", buf);
}

// ===== ACTk Generate 探针 + 早期安装线程 =====
typedef void* (*Sf3gGen_t)(void* mi);
static Sf3gGen_t g_origGen = nullptr;
static std::atomic<int> g_genCalls{0};

static void* sf3gHookGen(void* mi) {
    int n = ++g_genCalls;
    if (n <= 5) sf3gWriteFile("sf3g_actk.txt", "Generate called");
    return g_origGen ? g_origGen(mi) : nullptr;
}

static void sf3gInstallGenProbe() {
    if (!g_il2cppBase) return;
    int rc = DobbyHook((void*)(g_il2cppBase + 0x37720ec), (void*)sf3gHookGen, (void**)&g_origGen);
    char buf[96];
    snprintf(buf, sizeof(buf), "Generate hook rc=%d orig=%p", rc, (void*)g_origGen);
    sf3gWriteFile("sf3g_actk.txt", buf);
}

static void* sf3gEarlyHookThread(void*) {
    for (int i = 0; i < 1200; i++) {
        if (!g_il2cppBase) dl_iterate_phdr(findIl2cppCb, nullptr);
        if (g_il2cppBase) {
            sf3gInstallHooks();
            // sf3gInstallCodeHashHook();  // 2026-11-07: 探针会干扰 ACTk 校验，正式版关闭
            // sf3gInstallActkProbe();  // 2026-11-07: 探针会干扰 ACTk 校验，正式版关闭
    // sf3gInstallSumHook();  // 2026-11-07: 探针会干扰 ACTk 校验，正式版关闭
            sf3gInstallGenProbe();
            sf3gWriteFile("sf3g_actk.txt", "early hooks installed");
            return nullptr;
        }
        usleep(50000);
    }
    sf3gWriteFile("sf3g_actk.txt", "early thread timeout: no il2cpp");
    return nullptr;
}

extern "C" void Sf3g_StartEarlyHooks() {
    pthread_t tid;
    if (pthread_create(&tid, nullptr, sf3gEarlyHookThread, nullptr) == 0) pthread_detach(tid);
}

// ============================================================
//  Hashes.sum 拦截：CreatePlayerRequest.hashes_ 是 MapField<string,string>
//  在 Add("sum", <hash>) 时记录原值并替换成官方内容算出的值。
// ============================================================
static const char* kOfficialSum = "7C24E70D50D52886E4A18976F9D1FFD44E05D2A4";

typedef void (*Sf3gMapAdd_t)(void* self, void* key, void* value, void* mi);
static Sf3gMapAdd_t g_mapAddOrig = nullptr;
typedef void (*Sf3gRtClassInit_t)(void*);
static Sf3gRtClassInit_t g_rtClassInit = nullptr;
typedef void* (*Sf3gClassFromType_t)(const void*);
static Sf3gClassFromType_t g_classFromType = nullptr;
static std::atomic<int> g_sumHits{0};
static std::atomic<bool> g_dropSum{false};
static std::atomic<bool> g_patchSum{false};   // 2026-11-06：先验证「原值直发」
static std::atomic<bool> g_mapAddHooked{false};

static std::string sf3gIl2CppStr(void* s) {
    std::string r;
    if (!s) return r;
    int32_t len = *(int32_t*)((char*)s + 16);
    if (len <= 0 || len > 4096) return r;
    const uint16_t* c = (const uint16_t*)((char*)s + 20);
    for (int32_t i = 0; i < len; i++) r.push_back((char)(c[i] & 0xFF));
    return r;
}

static void sf3gMapAdd(void* self, void* key, void* value, void* mi) {
    auto& api = GetApi();
    bool isSum = false;
    if (key) {
        int32_t len = *(int32_t*)((char*)key + 16);
        if (len == 3) {
            const uint16_t* c = (const uint16_t*)((char*)key + 20);
            isSum = (c[0] == 's' && c[1] == 'u' && c[2] == 'm');
        }
    }
    if (isSum) {
        std::string oldVal = sf3gIl2CppStr(value);
        char buf[320];
        void* ra0 = __builtin_return_address(0);
        void* ra1 = __builtin_return_address(1);
        void* ra2 = __builtin_return_address(2);
        uintptr_t b = (uintptr_t)g_il2cppBase;
        snprintf(buf, sizeof(buf), "sum old=%s\n  caller0=0x%lx caller1=0x%lx caller2=0x%lx",
                 oldVal.c_str(),
                 b ? (uintptr_t)ra0 - b : 0,
                 b ? (uintptr_t)ra1 - b : 0,
                 b ? (uintptr_t)ra2 - b : 0);
        sf3gWriteFile("sf3g_sum.txt", buf);
        sf3gDumpBt("sum");
        if (g_patchSum.load() && api.string_new) {
            void* nv = api.string_new(kOfficialSum);
            if (nv) value = nv;
        }
        if (g_dropSum.load()) return;   // 实验：完全不往 map 里放 sum
    }
    if (g_mapAddOrig) g_mapAddOrig(self, key, value, mi);
}

static void sf3gInstallSumHook() {
    if (g_mapAddHooked.load()) return;
    auto& api = GetApi();
    if (!api.ready) return;
    sf3gWriteFile("sf3g_sum.txt", "s1 ready");
    void* req = nullptr;
    if (api.domain_get && api.domain_assemblies && api.assembly_image) {
        void* dom = api.domain_get();
        sf3gWriteFile("sf3g_sum.txt", "s2 has domain api");
        if (!dom) { sf3gWriteFile("sf3g_sum.txt", "no domain"); return; }
        size_t an = 0;
        void** asms = api.domain_assemblies(dom, &an);
        { char b[64]; snprintf(b,sizeof(b),"s3 asmCount=%zu", an); sf3gWriteFile("sf3g_sum.txt", b); }
        if (!asms || an == 0) { sf3gWriteFile("sf3g_sum.txt", "no assemblies"); return; }
        for (size_t i = 0; i < an && !req; i++) {
            if (!asms[i]) continue;
            const void* img = api.assembly_image(asms[i]);
            if (img) req = api.ClassFromName((void*)img, "sf3DTO", "CreatePlayerRequest");
        }
    }
    sf3gWriteFile("sf3g_sum.txt", req ? "s4 class found" : "s4 class NOT found");
    if (!req) return;
    void* fld = api.class_get_field ? api.class_get_field(req, "hashes_") : nullptr;
    sf3gWriteFile("sf3g_sum.txt", fld ? "s5 field ok" : "s5 field null");
    if (!fld) return;
    void* ftype = api.field_type ? api.field_type(fld) : nullptr;
    sf3gWriteFile("sf3g_sum.txt", ftype ? "s6 type ok" : "s6 type null");
    if (!ftype) return;
    if (!g_classFromType) {
        void* h0 = xdl_open("libil2cpp.so", XDL_DEFAULT);
        if (h0) {
            g_classFromType = (Sf3gClassFromType_t)xdl_sym(h0, "il2cpp_class_from_il2cpp_type", nullptr);
            if (!g_classFromType) g_classFromType = (Sf3gClassFromType_t)xdl_sym(h0, "il2cpp_class_from_type", nullptr);
            xdl_close(h0);
        }
    }
    void* mfClass = g_classFromType ? g_classFromType(ftype) : nullptr;
    sf3gWriteFile("sf3g_sum.txt", mfClass ? "s7 mapclass ok" : "s7 mapclass null");
    if (!mfClass) return;
    if (!g_rtClassInit) {
        void* h = xdl_open("libil2cpp.so", XDL_DEFAULT);
        if (h) {
            g_rtClassInit = (Sf3gRtClassInit_t)xdl_sym(h, "il2cpp_runtime_class_init", nullptr);
            xdl_close(h);
        }
        char b[96];
        snprintf(b, sizeof(b), "s7b rtClassInit=%p", (void*)g_rtClassInit);
        sf3gWriteFile("sf3g_sum.txt", b);
    }
    void* mi = nullptr;
    if (api.class_methods && api.method_get_name) {
        void* it = nullptr;
        while (void* m = api.class_methods(mfClass, &it)) {
            const char* nm = api.method_get_name(m);
            if (nm && strcmp(nm, "Add") == 0) {
                int pc = api.method_param_count ? api.method_param_count(m) : -1;
                if (pc == 2) { mi = m; break; }
            }
        }
        char b[80];
        snprintf(b, sizeof(b), "s7c iter done mi=%p", mi);
        sf3gWriteFile("sf3g_sum.txt", b);
    }
    sf3gWriteFile("sf3g_sum.txt", mi ? "s8 method ok" : "s8 method null");
    if (!mi) return;
    void* fn = *(void**)mi;
    if (!fn) { sf3gWriteFile("sf3g_sum.txt", "no Add code"); return; }
    int rc = DobbyHook(fn, (void*)sf3gMapAdd, (void**)&g_mapAddOrig);
    char b[160];
    snprintf(b, sizeof(b), "mapAdd hook rc=%d fn=%p orig=%p", rc, fn, (void*)g_mapAddOrig);
    sf3gWriteFile("sf3g_sum.txt", b);
    if (rc == 0) g_mapAddHooked = true;
}

// ===== 调用栈回溯（定位 sum 的调用链）=====
struct Sf3gBtState { uintptr_t pc[40]; int n; };
static _Unwind_Reason_Code sf3gBtCb(struct _Unwind_Context* ctx, void* arg) {
    Sf3gBtState* s = (Sf3gBtState*)arg;
    uintptr_t pc = _Unwind_GetIP(ctx);
    if (pc && s->n < 40) s->pc[s->n++] = pc;
    return _URC_NO_REASON;
}
static void sf3gDumpBt(const char* tag) {
    Sf3gBtState st; st.n = 0;
    _Unwind_Backtrace(sf3gBtCb, &st);
    uintptr_t base = (uintptr_t)g_il2cppBase;
    char buf[128];
    for (int i = 0; i < st.n && i < 12; i++) {
        if (base && st.pc[i] > base && st.pc[i] - base < 0x9000000)
            snprintf(buf, sizeof(buf), "%s bt[%d] rva=0x%lx", tag, i, (unsigned long)(st.pc[i] - base));
        else
            snprintf(buf, sizeof(buf), "%s bt[%d] abs=0x%lx", tag, i, (unsigned long)st.pc[i]);
        sf3gWriteFile("sf3g_bt.txt", buf);
    }
}

// ============================================================
//  自隐藏：把 libsf3g 从游戏读到的 /proc/self/maps 里抹掉
//  ACTk 扫到「非本包模块」就会上报错误 hashes（实测封号诱因）
// ============================================================
static int   (*g_oOpenat)(int, const char*, int, ...) = nullptr;
static int   (*g_oOpen)(const char*, int, ...) = nullptr;
static FILE* (*g_oFopen)(const char*, const char*) = nullptr;
static char  g_cleanMaps[192] = {0};
static std::atomic<bool> g_hiding{false};

static bool sf3gIsMapsPath(const char* p) {
    return p && strstr(p, "/proc/self/maps");
}

static void sf3gBuildCleanMaps() {
    if (g_cleanMaps[0]) return;
    const char* base = getenv("HOME");
    char dst[192];
    snprintf(dst, sizeof(dst), "/data/data/com.nekki.shadowfight3/cache/.cm");
    int in = g_oOpenat ? g_oOpenat(AT_FDCWD, "/proc/self/maps", O_RDONLY) : -1;
    if (in < 0) return;
    int out = g_oOpenat ? g_oOpenat(AT_FDCWD, dst, O_WRONLY | O_CREAT | O_TRUNC, 0600) : -1;
    if (out < 0) { close(in); return; }
    char buf[8192];
    ssize_t n;
    std::string line;
    while ((n = read(in, buf, sizeof(buf))) > 0) {
        for (ssize_t i = 0; i < n; i++) {
            line.push_back(buf[i]);
            if (buf[i] == '\n') {
                if (line.find("libsf3g") == std::string::npos) write(out, line.data(), line.size());
                line.clear();
            }
        }
    }
    if (!line.empty() && line.find("libsf3g") == std::string::npos) write(out, line.data(), line.size());
    close(in);
    close(out);
    strncpy(g_cleanMaps, dst, sizeof(g_cleanMaps) - 1);
    (void)base;
}

static int sf3gOpenat(int dirfd, const char* path, int flags, ...) {
    va_list ap; va_start(ap, flags); mode_t m = va_arg(ap, mode_t); va_end(ap);
    if (sf3gIsMapsPath(path) && !g_hiding.exchange(true)) {
        sf3gBuildCleanMaps();
        g_hiding = false;
        if (g_cleanMaps[0]) return g_oOpenat(AT_FDCWD, g_cleanMaps, flags, m);
    }
    return g_oOpenat(dirfd, path, flags, m);
}

static int sf3gOpen(const char* path, int flags, ...) {
    va_list ap; va_start(ap, flags); mode_t m = va_arg(ap, mode_t); va_end(ap);
    if (sf3gIsMapsPath(path) && !g_hiding.exchange(true)) {
        sf3gBuildCleanMaps();
        g_hiding = false;
        if (g_cleanMaps[0]) return g_oOpenat(AT_FDCWD, g_cleanMaps, flags, m);
    }
    return g_oOpenat(AT_FDCWD, path, flags, m);
}

static FILE* sf3gFopen(const char* path, const char* mode) {
    if (sf3gIsMapsPath(path) && !g_hiding.exchange(true)) {
        sf3gBuildCleanMaps();
        g_hiding = false;
        if (g_cleanMaps[0]) return g_oFopen(g_cleanMaps, mode);
    }
    return g_oFopen(path, mode);
}

static void sf3gInstallHideSelf() {
    void* h = xdl_open("libc.so", XDL_DEFAULT);
    if (h) {
        g_oOpenat = (int(*)(int,const char*,int,...))xdl_sym(h, "openat", nullptr);
        g_oOpen    = (int(*)(const char*,int,...))xdl_sym(h, "open", nullptr);
        g_oFopen   = (FILE*(*)(const char*,const char*))xdl_sym(h, "fopen", nullptr);
        xdl_close(h);
    }
    int r1 = g_oOpenat ? DobbyHook((void*)g_oOpenat, (void*)sf3gOpenat, (void**)&g_oOpenat) : -1;
    int r2 = g_oFopen ? DobbyHook((void*)g_oFopen, (void*)sf3gFopen, (void**)&g_oFopen) : -1;
    char b[128];
    snprintf(b, sizeof(b), "hideSelf openat=%d fopen=%d", r1, r2);
    sf3gWriteFile("sf3g_hide.txt", b);
}

// ===== 隐藏第二路：dl_iterate_phdr（ACTk 遍历已加载模块）=====
#include <link.h>
typedef int (*Sf3gDlIterCb)(struct dl_phdr_info*, size_t, void*);
static int (*g_oDlIterate)(Sf3gDlIterCb, void*) = nullptr;

struct Sf3gDlWrap { Sf3gDlIterCb cb; void* data; };
static int sf3gDlWrapCb(struct dl_phdr_info* info, size_t sz, void* data) {
    Sf3gDlWrap* w = (Sf3gDlWrap*)data;
    if (info && info->dlpi_name && strstr(info->dlpi_name, "libsf3g")) return 0;  // 跳过自己
    if (info && info->dlpi_name && strstr(info->dlpi_name, "starry_magic")) return 0;
    return w->cb(info, sz, w->data);
}
static int sf3gDlIterate(Sf3gDlIterCb cb, void* data) {
    if (!g_oDlIterate) return 0;
    Sf3gDlWrap w{cb, data};
    return g_oDlIterate(sf3gDlWrapCb, &w);
}
static void sf3gInstallHidePhdr() {
    void* h = xdl_open("libdl.so", XDL_DEFAULT);
    if (!h) h = xdl_open("libc.so", XDL_DEFAULT);
    if (h) {
        g_oDlIterate = (int(*)(Sf3gDlIterCb,void*))xdl_sym(h, "dl_iterate_phdr", nullptr);
        xdl_close(h);
    }
    if (!g_oDlIterate) { sf3gWriteFile("sf3g_hide2.txt", "dl_iterate_phdr not found"); return; }
    int rc = DobbyHook((void*)g_oDlIterate, (void*)sf3gDlIterate, (void**)&g_oDlIterate);
    char b[96];
    snprintf(b, sizeof(b), "hidePhdr rc=%d", rc);
    sf3gWriteFile("sf3g_hide2.txt", b);
}
