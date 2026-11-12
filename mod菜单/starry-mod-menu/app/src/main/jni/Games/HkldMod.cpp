/* ================================================================
 *  汉克旅店 1.1.0 / vc337 — 星空版 native 模块
 *
 *  做法（R1：加载边界换程序集字节）：
 *    1) 按名解析 AOT 方法 System.Reflection.Assembly.Load(byte[])（不被 HybridCLR 解释，
 *       methodPointer 是真代码地址；禁止写死 RVA —— 热更/换版全部作废）
 *    2) Dobby inline hook；收到的 byte[] 若是官方 Assembly-CSharp.dll（MZ + 长度 ==
 *       kHkldOfficialLen）⇒ 用 il2cpp_array_new 在托管堆按**补丁产物真实长度**重新分配
 *       容器、拷入内嵌补丁字节，再交给原函数（hotfix-dll-replace-pattern §7）
 *    3) 补丁版 IL 从 starry_config.ini 读菜单键；本文件的 OnFeature* 负责写该文件
 *
 *  安全默认：长度不匹配（热更换了新版 DLL）⇒ PASSTHROUGH 官方版，不崩不闪退，只在
 *            logcat 留一行并弹一次 toast。
 * ================================================================ */
#include "HkldMod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <mutex>
#include <string>
#include <vector>
#include <pthread.h>
#include <unistd.h>
#include <sys/stat.h>

#include <android/log.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/StarPtr.h"
#include "Includes/sg_plain_hkld.h"
#include "Unity/Il2CppApi.h"

#define HKLD_LOG "[Hkld]"

/* objcopy 内嵌的明文补丁程序集（Games/HkldPatchedDll.a，符号名固定） */
extern "C" const uint8_t _binary_hkld_patched_dll_bin_start[];
extern "C" const uint8_t _binary_hkld_patched_dll_bin_end[];

/* 原生 toast（实现在 MainModern.cpp）；未知版本时给用户一句可读提示 */
extern "C" void StarryShowToast(const char* text);

/* ---- Il2CppArray 布局（Unity 2022 / IL2CPP，与 无限升级 vc1637 同一实测口径）----
 *   Il2CppObject{klass*,monitor*} = 16B；bounds* @0x10；max_length @0x18；元素数据 @0x20 */
static constexpr size_t kArrLenOff  = 0x18;
static constexpr size_t kArrDataOff = 0x20;

typedef void* (*ArrayNew_t)(void* elementClass, uintptr_t length);
typedef void* (*GchandleNew_t)(void* obj, bool pinned);
typedef void  (*GchandleFree_t)(void* handle);
typedef void* (*AssemblyLoadBytes_t)(void* rawAssembly, const void* method);
typedef int   (*TypeGetType_t)(void* type);   // Il2CppType* -> Il2CppTypeEnum

static AssemblyLoadBytes_t g_origLoad = nullptr;
static ArrayNew_t  g_arrayNew   = nullptr;
static GchandleNew_t g_gcNew    = nullptr;
static GchandleFree_t g_gcFree  = nullptr;
static void* g_byteClass = nullptr;
static TypeGetType_t g_typeGetType = nullptr;

static std::atomic<bool> g_hooked{false};
static std::atomic<int>  g_hits{0};
static std::atomic<int>  g_unknownNotice{0};
static std::atomic<bool> g_retryStarted{false};
static std::mutex g_mtx;

// Il2CppTypeEnum：SZARRAY = 0x1d（byte[]），STRING = 0x0e（string）
static constexpr int kTypeSzArray = 0x1d;
static const char* kAsmLoadClassNs = "System.Reflection";
static const char* kAsmLoadClass   = "Assembly";

// ============================================================
//  starry_config.ini 通道（与 ilpatch 的 Starry.StarryConfig 同路径）
// ============================================================
static std::string PkgName() {
    char b[160] = {0};
    FILE* f = fopen("/proc/self/cmdline", "rb");
    if (!f) return std::string();
    size_t n = fread(b, 1, sizeof(b) - 1, f);
    fclose(f);
    if (!n) return std::string();
    b[n] = 0;
    // 进程名可能带 ":xxx" 后缀（子进程），取冒号前
    for (size_t i = 0; i < n; i++) if (b[i] == ':') { b[i] = 0; break; }
    return std::string(b);
}

static std::string ConfigPath() {
    std::string pkg = PkgName();
    if (pkg.empty()) return std::string();
    return "/data/user/0/" + pkg + "/files/starry_config.ini";
}

static bool ReadWholeFile(const std::string& path, std::vector<std::string>* out) {
    FILE* f = fopen(path.c_str(), "rb");
    if (!f) return false;
    std::string all;
    char buf[4096];
    size_t n;
    while ((n = fread(buf, 1, sizeof(buf), f)) > 0) all.append(buf, n);
    fclose(f);
    size_t pos = 0;
    while (pos <= all.size()) {
        size_t e = all.find('\n', pos);
        if (e == std::string::npos) {
            if (pos < all.size()) out->push_back(all.substr(pos));
            break;
        }
        std::string line = all.substr(pos, e - pos);
        while (!line.empty() && (line.back() == '\r' || line.back() == ' ')) line.pop_back();
        if (!line.empty()) out->push_back(line);
        pos = e + 1;
    }
    return true;
}

static void WriteConfigKey(const char* key, const std::string& val) {
    std::string path = ConfigPath();
    if (path.empty()) { LOGE(HKLD_LOG " pkg name empty, skip %s", key); return; }
    std::string dir = path.substr(0, path.rfind('/'));
    if (!dir.empty()) mkdir(dir.c_str(), 0755);

    std::vector<std::string> lines;
    ReadWholeFile(path, &lines);
    std::string tmp = path + ".tmp";
    FILE* f = fopen(tmp.c_str(), "wb");
    if (!f) { LOGE(HKLD_LOG " cannot write %s", tmp.c_str()); return; }
    const size_t klen = strlen(key);
    bool replaced = false;
    for (auto& l : lines) {
        if (l.compare(0, klen, key) == 0 && l.size() > klen && l[klen] == '=') {
            fprintf(f, "%s=%s\n", key, val.c_str());
            replaced = true;
        } else {
            fprintf(f, "%s\n", l.c_str());
        }
    }
    if (!replaced) fprintf(f, "%s=%s\n", key, val.c_str());
    fclose(f);
    if (rename(tmp.c_str(), path.c_str()) != 0) LOGE(HKLD_LOG " rename failed %s", path.c_str());
    else LOGI(HKLD_LOG " config %s=%s", key, val.c_str());
}

// 一次性把「运行时实际装载的程序集」落到私有目录（版本辨识用）
// 用途：确认热更下发的 Assembly-CSharp.dll 与包内 dump 是否同一版（长度 5,328,384 vs 5,328,896 差 512 B）。
static void DumpSeenOnce(const uint8_t* data, size_t len) {
    std::string pkg = PkgName();
    if (pkg.empty()) return;
    std::string dir = "/data/user/0/" + pkg + "/files";
    mkdir(dir.c_str(), 0755);
    char name[96];
    snprintf(name, sizeof(name), "%s/hkld_seen_%zu.dll", dir.c_str(), len);
    if (access(name, F_OK) == 0) return;   // 已落过盘，不覆盖
    FILE* f = fopen(name, "wb");
    if (!f) { LOGE(HKLD_LOG " cannot dump seen dll %s", name); return; }
    size_t n = fwrite(data, 1, len, f);
    fclose(f);
    LOGI(HKLD_LOG " 运行时程序集已落盘诊断: %s (%zu/%zu B)", name, n, len);
}

// ============================================================
//  按名解析 System.Reflection.Assembly.Load(byte[])
// ============================================================
/* 在 klass 的方法表里找「名字 == name、参数个数 == argc、第 p0 个参数类型名含 wantTypeSuffix」
 * 的方法。为什么不能直接用 GetMethod(name, argc)：Load 有两个 argc==1 的重载
 * （Load(string) / Load(byte[])），先命中的可能是 string 版 ⇒ 只解析不报错、运行时全哑。 */
static void* FindMethodByParamType(void* klass, const char* name, int argc, int wantTypeEnum) {
    auto& api = Il2Cpp::GetApi();
    if (!klass || !api.class_methods || !api.method_get_name) return nullptr;
    if (!g_typeGetType || !api.method_param) return nullptr;
    void* iter = nullptr;
    while (true) {
        void* m = api.class_methods(klass, &iter);
        if (!m) break;
        const char* mn = api.method_get_name(m);
        if (!mn || strcmp(mn, name) != 0) continue;
        if (api.method_param_count && (int)api.method_param_count(m) != argc) continue;
        if (argc == 0) return m;
        void* pt = api.method_param(m, 0);
        if (!pt) continue;
        // 用 Il2CppTypeEnum 判别参数类型（byte[] = SZARRAY 0x1d / string = 0x0e），
        // 不用 il2cpp_type_get_name：其返回值需 free，而该指针在真机（OPPO/Android 15）
        // 上带 tag，传给 free 会 SIGABRT「Pointer tag ... was truncated」（2026-11-13 实测）。
        if (g_typeGetType(pt) == wantTypeEnum) return m;
    }
    return nullptr;
}

static bool ResolveByteClass() {
    auto& api = Il2Cpp::GetApi();
    void* corlib = api.GetCorlib();
    if (!corlib) return false;
    g_byteClass = api.ClassFromName(corlib, "System", "Byte");
    return g_byteClass != nullptr;
}

static void* HookAssemblyLoad(void* rawAssembly, const void* method);

static bool TryHookOnce() {
    auto& api = Il2Cpp::GetApi();
    if (!api.ready) {
        if (!api.Load(sec_lib_il2cpp())) return false;
    }
    if (api.thread_attach) {
        void* dom = api.Domain();
        if (dom) api.thread_attach(dom);   // 后台线程调 il2cpp API 前必须先 attach
    }
    void* corlib = api.GetCorlib();
    if (!corlib) return false;
    // ⚠ 必须在 FindMethodByParamType 之前解析（首版把这段放在 mi 解析之后 ⇒
    //   首轮 g_typeGetType 为空、按签名匹配恒失败，静默退化成 GetMethod 首个 argc=1
    //   （可能命中 Load(string)）——2026-11-13 真机日志实锤 "[Hkld] Load(byte[]) 未按签名命中"）。
    if (!g_typeGetType) g_typeGetType = (TypeGetType_t)Il2Cpp::ResolveExport("il2cpp_type_get_type");
    if (!g_arrayNew) g_arrayNew = (ArrayNew_t)Il2Cpp::ResolveExport("il2cpp_array_new");
    if (!g_gcNew)    g_gcNew    = (GchandleNew_t)Il2Cpp::ResolveExport("il2cpp_gchandle_new");
    if (!g_gcFree)   g_gcFree   = (GchandleFree_t)Il2Cpp::ResolveExport("il2cpp_gchandle_free");
    if (!g_arrayNew || !g_gcNew || !g_gcFree) {
        LOGE(HKLD_LOG " il2cpp_array_new/gchandle 未解析到，禁用替换");
        return false;
    }

    void* klass = api.ClassFromName(corlib, kAsmLoadClassNs, kAsmLoadClass);
    if (!klass) { LOGE(HKLD_LOG " System.Reflection.Assembly 未解析到"); return false; }

    void* mi = FindMethodByParamType(klass, "Load", 1, kTypeSzArray);
    if (!mi) {
        mi = api.GetMethod(klass, "Load", 1);   // 兜底（并用下面的可执行段校验兜住挂错）
        if (mi) LOGW(HKLD_LOG " Load(byte[]) 未按签名命中，退化为 GetMethod 首个 argc=1（需日志复核）");
    }
    if (!mi) { LOGE(HKLD_LOG " Assembly.Load 未解析到"); return false; }
    if (g_typeGetType && api.method_param && api.method_param_count && api.method_param_count(mi) == 1) {
        void* pt = api.method_param(mi, 0);
        LOGI(HKLD_LOG " Assembly.Load 命中 mi=%p paramType=%d（期望 %d=SZARRAY；%d/... 即 string）",
             mi, pt ? g_typeGetType(pt) : -1, kTypeSzArray, 0x0e);
    }

    void* target = *(void**)mi;   // MethodInfo.methodPointer @ 0x0
    if (!target || !Il2Cpp::IsExecutableRange((uintptr_t)target, 16)) {
        LOGE(HKLD_LOG " methodPointer 不可执行 (%p) —— 运行时未就绪，稍后重试", target);
        return false;
    }

    if (!ResolveByteClass()) { LOGE(HKLD_LOG " System.Byte 未解析到"); return false; }

    void* orig = nullptr;
    if (DobbyHook(target, (void*)HookAssemblyLoad, &orig) != 0 || !orig) {
        LOGE(HKLD_LOG " DobbyHook 失败 @%p", target);
        return false;
    }
    g_origLoad = (AssemblyLoadBytes_t)orig;
    g_hooked.store(true);
    LOGI(HKLD_LOG " hook 已安装: Assembly.Load(byte[]) mi=%p target=%p orig=%p", mi, target, orig);
    LOGI(HKLD_LOG " 载荷策略=%s 官方=%uB 补丁=%uB sha=%s",
         kHkldPayloadPolicy, (unsigned)kHkldOfficialLen, (unsigned)kHkldPatchedLen, kHkldPatchedSha);
    return true;
}

// ============================================================
//  Hook 本体：官方 Assembly-CSharp.dll -> 补丁版
// ============================================================
static void* HookAssemblyLoad(void* rawAssembly, const void* method) {
    AssemblyLoadBytes_t orig = g_origLoad;
    if (!orig) return nullptr;
    if (!rawAssembly || !starptr::LiveFast(rawAssembly)) return orig(rawAssembly, method);

    const int64_t len = *(const int64_t*)((const char*)rawAssembly + kArrLenOff);
    const uint8_t* data = (const uint8_t*)rawAssembly + kArrDataOff;
    if (len < 2 || data[0] != 'M' || data[1] != 'Z') return orig(rawAssembly, method);   // 非 PE

    if ((uint32_t)len == kHkldPatchedLen) {
        LOGI(HKLD_LOG " 缓冲区已是补丁版（幂等放行）");
        return orig(rawAssembly, method);
    }
    // 两个可接受基线：运行时热更下发（kHkldOfficialLen，补丁按此制作）/ 包内 bundle（kHkldBundledLen）
    const bool isOfficial = ((uint32_t)len == kHkldOfficialLen);
    const bool isBundled  = ((uint32_t)len == kHkldBundledLen);
    if (!isOfficial && !isBundled) {
        DumpSeenOnce(data, (size_t)len);
        if (g_unknownNotice.fetch_add(1) == 0) {
            LOGW(HKLD_LOG " 未知程序集长度 %d（期望 %u）→ PASSTHROUGH 官方版",
                 (int)len, (unsigned)kHkldOfficialLen);
            StarryShowToast("[汉克旅店] 游戏热更版本与补丁不符，本次按官方版本运行");
        }
        return orig(rawAssembly, method);
    }

    if (isBundled && !isOfficial) {
        LOGW(HKLD_LOG " 命中包内 bundle 版本(%uB)，补丁按运行时基线(%uB)制作 —— 仍需真机复核",
             (unsigned)len, (unsigned)kHkldOfficialLen);
    }
    void* newArr = g_arrayNew(g_byteClass, (uintptr_t)kHkldPatchedLen);
    if (!newArr) {
        LOGE(HKLD_LOG " il2cpp_array_new 失败 → PASSTHROUGH");
        return orig(rawAssembly, method);
    }
    memcpy((char*)newArr + kArrDataOff, _binary_hkld_patched_dll_bin_start, kHkldPatchedLen);

    // 分配后到调用前万一有 GC，本地 C 指针不构成根 ⇒ 用 gchandle 钉住
    void* h = g_gcNew(newArr, false);
    int n = g_hits.fetch_add(1) + 1;
    LOGI(HKLD_LOG " 装载边界替换成功 (#%d) 官方=%d 补丁=%u B", n, (int)len, (unsigned)kHkldPatchedLen);
    void* ret = orig(newArr, method);
    if (h) g_gcFree(h);
    return ret;
}

static void* RetryThread(void*) {
    for (int i = 0; i < 600 && !g_hooked.load(); i++) {
        if (TryHookOnce()) return nullptr;
        usleep(100 * 1000);
    }
    if (!g_hooked.load()) {
        LOGE(HKLD_LOG " fast-retry 60s 超时放弃（下一轮 tryInit 会再起线程）");
        g_retryStarted.store(false);
    }
    return nullptr;
}

bool Hkld_TryInit(JavaVM* /*vm*/) {
    if (g_hooked.load()) return true;
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return true;
    if (!g_retryStarted.exchange(true)) {
        pthread_t tid;
        if (pthread_create(&tid, nullptr, RetryThread, nullptr) == 0) {
            pthread_detach(tid);
            LOGI(HKLD_LOG " fast-retry 线程已启动（100ms x 600）");
        } else {
            g_retryStarted.store(false);
            LOGE(HKLD_LOG " pthread_create 失败");
        }
    }
    return false;
}

bool Hkld_IsReady() { return g_hooked.load(); }

void Hkld_Tick(const bool* /*features*/, const int* /*values*/) {
    // 开关全走 ini 文件通道，Tick 不做任何事（热路径零开销）
}

void Hkld_OnFeatureChanged(int featNum, bool enabled) {
    const char* on = enabled ? "1" : "0";
    switch (featNum) {
    case HKLD_FEAT_GOD:    WriteConfigKey("god", on); break;
    case HKLD_FEAT_SKILL:  WriteConfigKey("inf_skill", on); break;
    case HKLD_FEAT_UNLOCK: WriteConfigKey("unlock_char", on); break;
    case HKLD_FEAT_MONEY:  WriteConfigKey("inf_money", on); break;
    case HKLD_FEAT_KEY:    WriteConfigKey("inf_key", on); break;
    default: break;
    }
}

void Hkld_OnValueChanged(int featNum, int value) {
    if (featNum != HKLD_FEAT_DMGX) return;
    if (value < 1) value = 1;
    if (value > 100) value = 100;
    char buf[16];
    snprintf(buf, sizeof(buf), "%d", value);
    WriteConfigKey("damage_x", buf);
}

void Hkld_ForceClear() {
    // 授权失效时把「生效开关」全部落回关（slider 值保留：它只是倍数，不生效即无副作用）
    LOGI(HKLD_LOG " ForceClear: 关闭所有生效开关");
    WriteConfigKey("god", "0");
    WriteConfigKey("inf_skill", "0");
    WriteConfigKey("unlock_char", "0");
    WriteConfigKey("inf_money", "0");
    WriteConfigKey("inf_key", "0");
}
