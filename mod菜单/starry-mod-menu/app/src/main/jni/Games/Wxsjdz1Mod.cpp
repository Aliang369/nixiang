/* ================================================================
 *  无限升级 · 断网版 wxsjdz1 — 12.10701 / vc1647（第三方断网定制包）
 *
 *  ★ 这不是 wxsj / wxsjdz2 ★（三套的载荷 DLL、挂点、菜单口径互不通用）
 *
 *  本轮（2026-11-15）形态：
 *    · 挂点 = libil2cpp.so +0x2b890d8（本包 so 运行时实测：探针包 hook installed 实锤）
 *    · 载荷 = 明文内嵌补丁 DLL（无卡密 ⇒ 不走票控加密层，不定义 SG_PAYLOAD_ENC）
 *    · 注入 = **自建数组**：官方数组 1,995,776 B，补丁 2,008,576 B（Cecil 重写固有膨胀
 *      7,680 B + 补丁 5,120 B），memcpy 会越界 ⇒ 用 il2cpp_array_new_specific 建同类型
 *      字节数组，拷补丁进去后把**新数组**当 data 传给原函数；数组永久 pinned
 *      （解释器按指针惰性读 IL，不可回收/移动）。放得下时仍走原地覆写（省一次分配）。
 *    · 变体表：主行 {1995776, 0xaca5988be25f5119}；官方 12 线两行（2028544/2030080）
 *      保留为**识别但透传**（本包里出现即说明换了底包，绝不拿 dz1 载荷去顶别人的数组）。
 *
 *  功能开关契约（三套一致）：native 只写 starry_config.ini，
 *  效果由补丁 DLL 内的 StarryConfig（GetBool/GetInt，2s 缓存）读取 —— native 侧禁碰 IL2CPP/UI。
 * ================================================================ */
#include "Wxsjdz1Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <mutex>
#include <string>
#include <vector>
#include <pthread.h>
#include <unistd.h>
#include <sys/stat.h>

#include <android/log.h>
#include <dlfcn.h>
#include <link.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/StarPtr.h"            // 设备无关指针判据（清 TBI/MTE 顶字节）

// 明文载荷（无加固）：长度/sha 由 04_scripts/无限升级_断网版/gen_plain_header_dz1.py 生成
#include "Includes/sg_plain_wxsjdz1.h"

#define WXSJ1_LOG "[Wxsjdz1]"

// ---- 挂点身份 ----
// vc1647（本断网包 libil2cpp.so md5 917c706ace5672d5fa2f81bf7ba58891 / 115,579,184 B）
// 运行时实锤：探针包日志 "hook installed @base=0x771797f000 +0x2b890d8 orig=0x7942e04000"。
// 序言与本包 so 明文 .text 逐字节一致（WxsjMod.cpp 同一组常量）；不符 = 版本漂移 ⇒ 拒绝 hook。
static constexpr uintptr_t kWxsj1ExecRva = 0x2b890d8;
static const uint8_t kWxsj1ExecPrologue[16] = {
    0xfe, 0x0f, 0x1c, 0xf8, 0xf8, 0x5f, 0x01, 0xa9,
    0xf6, 0x57, 0x02, 0xa9, 0xf4, 0x4f, 0x03, 0xa9,
};
static const char kWxsj1DllNameKey[] = "targetfuntional";

// ---- 载荷变体表 ----
// 每行 = 上游 DLL 身份（官方长度 + 整包 fnv1a64）+ 内嵌补丁（明文 .bin，objcopy 成 .a）。
// patchedHash = 内嵌补丁整包 fnv1a64（写进日志/文档，供人工对账；0 = 该行无载荷 ⇒ 透传）。
struct DllVariant {
    int32_t officialLen;
    uint64_t officialHash;
    uint64_t patchedHash;
    const char* name;
    const uint8_t* data;
    uint32_t len;
};
extern "C" const uint8_t _binary_targetfuntional_dll_wxsjdz1_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_wxsjdz1_bin_end[];

static const DllVariant kDllVariants[] = {
    // 主行：本断网包实际装载的官方热更 DLL（1995776 / fnv 0xaca5988be25f5119）
    //   → 补丁 dz1_v2（31 项补丁链：ilpatch 30 + delta 1，含 feat 1/2/4–12）
    {1995776, 0xaca5988be25f5119ULL, kWXSJDZ1_PATCHED_FNV, "dz1",
     _binary_targetfuntional_dll_wxsjdz1_bin_start, kWxsjdz1PayloadLen},
    // 兜底两行（官方 12 线旧底包）：**只识别、不带载荷** ⇒ 命中即 PASSTHROUGH 官方版。
    // 断网包里出现这两长度说明底包被人换过，拿 dz1 载荷顶上去 = 错版（宁可无功能，不可错版）。
    {2028544, 0x1ce9b97aae2abca3ULL, 0, "hf3-official-passthrough", nullptr, 0},
    {2030080, 0x91f9acbd064fa875ULL, 0, "hf2-official-passthrough", nullptr, 0},
};
static constexpr int kDllVariantCount = (int)(sizeof(kDllVariants) / sizeof(kDllVariants[0]));

static const DllVariant* PickDllVariant(int32_t len, uint64_t h) {
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len && v.officialHash == h) return &v;
    }
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len) return &v;
    }
    return nullptr;
}

// 数值输入框夹取范围（与 11 线 ilpatch_vc1637 的 kItemReverseMinX/MaxX 口径一致：1..1000000）
static const long long kWxsj1ValueMin = 1;
static const long long kWxsj1ValueMax = 1000000;
static const int kWxsj1ItemReverseDefault = 1000;

typedef uint8_t (*Exec_t)(void* self, void* data, void* outP, void* mi);

static std::mutex g_mtx;
static Exec_t g_origExec = nullptr;
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_replaceArmed{false};
static std::atomic<int> g_hits{0};
static std::atomic<bool> g_retryStarted{false};

// 自建数组缓存（同一 (len,hash) 只建一次；数组内容不变，可复用）
static void* g_swapArr = nullptr;
static uint64_t g_swapFor = 0;
static int32_t g_swapLen = 0;

// il2cpp API（运行时从 libil2cpp.so 取；缺任一 ⇒ 超长路径放弃，落 PASSTHROUGH）
typedef void* (*pfn_array_new_specific)(void* arrayKlass, uintptr_t length);
typedef void* (*pfn_gchandle_new)(void* obj, int pinned);
typedef const char* (*pfn_class_get_name)(void* klass);
static pfn_array_new_specific g_arrayNewSpecific = nullptr;
static pfn_gchandle_new g_gcHandleNew = nullptr;
static pfn_class_get_name g_classGetName = nullptr;

static std::string PkgName();
static bool ReadFileLines(const std::string& path, std::vector<std::string>* out);
static std::string ConfigPath();
static void WriteConfigKey(const char* key, const std::string& val);

// ============================================================
//  小工具
// ============================================================
static uint64_t Fnv1a64(const uint8_t* p, size_t n) {
    uint64_t h = 0xcbf29ce484222325ULL;
    for (size_t i = 0; i < n; i++) { h ^= (uint64_t)p[i]; h *= 0x100000001b3ULL; }
    return h;
}

static bool ContainsDllNameKey(const uint8_t* bytes, size_t len) {
    const size_t klen = sizeof(kWxsj1DllNameKey) - 1;
    for (size_t o = 0; o + klen <= len; o++) {
        if (bytes[o] == 't' && memcmp(bytes + o, kWxsj1DllNameKey, klen) == 0) return true;
    }
    return false;
}

static int PhdrCb(struct dl_phdr_info* info, size_t, void* data) {
    uintptr_t* out = (uintptr_t*)data;
    const char* n = info->dlpi_name;
    if (n && strlen(n) >= 12 && strcmp(n + strlen(n) - 12, "libil2cpp.so") == 0) {
        *out = info->dlpi_addr;
        return 1;
    }
    return 0;
}

static uintptr_t Il2CppBase() {
    uintptr_t base = 0;
    dl_iterate_phdr(PhdrCb, &base);
    return base;
}

static bool ResolveIl2CppApi() {
    static std::once_flag once;
    static bool ok = false;
    std::call_once(once, []() {
        void* h = dlopen("libil2cpp.so", RTLD_NOW | RTLD_NOLOAD);
        if (!h) h = dlopen("libil2cpp.so", RTLD_NOW);
        if (!h) { LOGE(WXSJ1_LOG " dlopen libil2cpp.so failed"); return; }
        // 本包 libil2cpp.so 的 dynsym 被易盾重写（静态 readelf 看不到任何 il2cpp_*），
        // 只能在运行时逐个探名（dlsym=RTLD_DEFAULT 兜底）。探测结果一次性落日志，供定位用。
        static const char* kArrNewNames[] = {
            "il2cpp_array_new_specific", "il2cpp_array_new", "il2cpp_array_new_full",
            "il2cpp_array_class_get", "il2cpp_array_length", "il2cpp_array_get_byte_length",
        };
        static const char* kMiscNames[] = {
            "il2cpp_gchandle_new", "il2cpp_gchandle_new_weakref", "il2cpp_gchandle_free",
            "il2cpp_class_get_name", "il2cpp_class_get_element_class", "il2cpp_object_new",
            "il2cpp_gc_alloc", "il2cpp_gc_malloc", "il2cpp_domain_get", "il2cpp_class_from_name",
            "il2cpp_runtime_invoke", "il2cpp_string_new",
        };
        for (const char* n : kArrNewNames) {
            void* s1 = dlsym(h, n);
            void* s2 = s1 ? s1 : dlsym(RTLD_DEFAULT, n);
            LOGI(WXSJ1_LOG " sym %-32s = %p%s", n, s2, s2 ? (s1 ? "" : " (dflt)") : " MISSING");
            if (!g_arrayNewSpecific && s2) {
                if (strcmp(n, "il2cpp_array_new_specific") == 0) g_arrayNewSpecific = (pfn_array_new_specific)s2;
            }
        }
        for (const char* n : kMiscNames) {
            void* s2 = dlsym(h, n);
            if (!s2) s2 = dlsym(RTLD_DEFAULT, n);
            LOGI(WXSJ1_LOG " sym %-32s = %p", n, s2);
            if (!g_gcHandleNew && strcmp(n, "il2cpp_gchandle_new") == 0) g_gcHandleNew = (pfn_gchandle_new)s2;
            if (!g_classGetName && strcmp(n, "il2cpp_class_get_name") == 0) g_classGetName = (pfn_class_get_name)s2;
        }
        ok = (g_arrayNewSpecific && g_gcHandleNew);
        LOGI(WXSJ1_LOG " il2cpp api: array_new_specific=%p gchandle_new=%p class_get_name=%p",
             (void*)g_arrayNewSpecific, (void*)g_gcHandleNew, (void*)g_classGetName);
    });
    return ok;
}

// ============================================================
//  注入钩子
//  data = Il2CppArray（byte[]）：+0x18 = max_length(int32)，+0x20 = 内联数据起点。
//  实测：+0x20 直接就是 "MZ"，说明数据是**内联**在该数组对象里的，不能改指针（改了就覆盖 DOS 头），
//  所以超长时只能**换一个更大的同类型数组**，把新数组当形参传给原函数。
// ============================================================
static uint8_t HookExec(void* self, void* data, void* outP, void* mi) {
    Exec_t orig = g_origExec;
    if (!orig) return 0;
    if (!starptr::LiveFast(data)) return orig(self, data, outP, mi);

    int32_t len = 0;
    memcpy(&len, (const uint8_t*)data + 0x18, sizeof(len));
    const uint8_t* bytes = (const uint8_t*)data + 0x20;
    const bool isDll = (len >= 2 && bytes[0] == 'M' && bytes[1] == 'Z');

    if (!(g_replaceArmed.load(std::memory_order_relaxed) && isDll
          && ContainsDllNameKey(bytes, (size_t)len))) {
        return orig(self, data, outP, mi);
    }

    const uint64_t h = Fnv1a64(bytes, (size_t)len);
    const DllVariant* v = PickDllVariant(len, h);
    if (!v) {
        LOGE(WXSJ1_LOG " unknown dll len=%d fnv=%016llx : PASSTHROUGH official", len,
             (unsigned long long)h);
        return orig(self, data, outP, mi);
    }
    if (!v->data || v->len == 0) {
        LOGW(WXSJ1_LOG " variant %s recognized but ship no payload -> PASSTHROUGH official (len=%d fnv=%016llx)",
             v->name, len, (unsigned long long)h);
        return orig(self, data, outP, mi);
    }

    const size_t pLen = (size_t)v->len;
    const int32_t pLen32 = (int32_t)pLen;

    // 幂等：缓冲区里已经是我们的补丁（原地覆写过），直接放行
    if ((size_t)len == pLen && memcmp(bytes, v->data, pLen) == 0) {
        LOGI(WXSJ1_LOG " buffer already carries %s patch (hit#%d fnv=%016llx) -> passthrough",
             v->name, g_hits.load(), (unsigned long long)h);
        return orig(self, data, outP, mi);
    }

    if (pLen <= (size_t)len) {
        // 放得下：原地覆写（官方数组不变长，尾零填充）
        memcpy((void*)bytes, v->data, pLen);
        memset((void*)(bytes + pLen), 0, (size_t)len - pLen);
        int n = g_hits.fetch_add(1) + 1;
        LOGI(WXSJ1_LOG " patched dll injected (wxsjdz1 %s mode=inplace hit#%d loaded=%d fnv=%016llx patched=%zu pad=%d)",
             v->name, n, len, (unsigned long long)h, pLen, (int)((size_t)len - pLen));
        return orig(self, data, outP, mi);
    }

    // 超长：自建「同布局字节数组」对象，传给原函数（原地覆写物理上无解，见下）
    // 为什么不用 il2cpp_array_new_specific：本包 libil2cpp.so 的 dynsym 被易盾重写，
    //   实测只导出 gchandle_new/weakref/free + domain_get + runtime_invoke，
    //   il2cpp_array_new*/object_new/class_from_name **全部 MISSING**（见 hook 安装时的符号探针日志）。
    // 故按运行时实测布局手工构造对象（64 位 il2cpp）：
    //   +0x00 klass / +0x08 monitor / +0x10 bounds / +0x18 max_length(int32) / +0x20 内联数据
    //   头部 0x20 字节直接从官方数组拷，只把 max_length 改成补丁长度 —— 类指针、bounds 全原样。
    // 安置在 malloc 区（不在 GC 堆内）：Boehm 保守 GC 只处理堆内指针，堆外对象不扫描/不回收；
    //   且解释器按指针惰性读 IL ⇒ 这块内存必须活到进程结束（不 free、指针留在静态变量里）。
    const size_t fakeSz = 0x20 + pLen + 8;
    uint8_t* fake = (uint8_t*)malloc(fakeSz);
    if (!fake) {
        LOGE(WXSJ1_LOG " malloc(%zu) for self-array failed : PASSTHROUGH official", fakeSz);
        return orig(self, data, outP, mi);
    }
    memcpy(fake, data, 0x20);                       // klass/monitor/bounds 原样
    memcpy(fake + 0x18, &pLen32, 4);                // max_length = 补丁长度
    memcpy(fake + 0x20, v->data, pLen);
    g_swapArr = fake;
    g_swapFor = h;
    g_swapLen = len;
    int n = g_hits.fetch_add(1) + 1;
    LOGI(WXSJ1_LOG " patched dll injected (wxsjdz1 %s mode=selfarray hit#%d loaded=%d fnv=%016llx patched=%zu pad=%d klass=%p buf=%p)",
         v->name, n, len, (unsigned long long)h, pLen, (int)((int64_t)len - (int64_t)pLen),
         *(void**)data, (void*)fake);
    return orig(self, fake, outP, mi);
}

static bool TryHookOnce() {
    uintptr_t base = Il2CppBase();
    if (!base) return false;
    uint8_t* target = (uint8_t*)(base + kWxsj1ExecRva);
    uint8_t cur[16];
    memcpy(cur, target, sizeof(cur));
    if (memcmp(cur, kWxsj1ExecPrologue, sizeof(cur)) != 0) return false;   // 壳未解密/漂移：静默重试

    void* orig = nullptr;
    if (DobbyHook(target, (void*)HookExec, &orig) != 0 || !orig) {
        LOGE(WXSJ1_LOG " DobbyHook failed");
        return false;
    }
    g_origExec = (Exec_t)orig;
    g_hooked.store(true);
    g_replaceArmed.store(true);
    LOGI(WXSJ1_LOG " hook installed @base=%p +0x%zx orig=%p variants=%d(payload %uB)",
         (void*)base, kWxsj1ExecRva, orig, kDllVariantCount, (unsigned)kWxsjdz1PayloadLen);
    LOGI(WXSJ1_LOG " payload policy=%s -> patch injected on next assembly load", kWxsjdz1PlainPolicy);
    // 2026-11-15 用户口径：本断网版**不要过检测** ⇒ 不再写 block_report，防封相关补丁也已整段摘除。
    return true;
}

static void* FastRetryThread(void*) {
    for (int i = 0; i < 600 && !g_hooked.load(); i++) {
        if (TryHookOnce()) return nullptr;
        usleep(100 * 1000);
    }
    if (!g_hooked.load()) {
        LOGE(WXSJ1_LOG " fast-retry 60s 超时放弃（允许下一轮 tryInit 重启线程）");
        g_retryStarted.store(false, std::memory_order_release);
    }
    return nullptr;
}

bool Wxsjdz1_TryInit() {
    if (g_hooked.load()) return true;
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return true;
    if (!g_retryStarted.exchange(true, std::memory_order_acq_rel)) {
        pthread_t tid;
        if (pthread_create(&tid, nullptr, FastRetryThread, nullptr) == 0) {
            pthread_detach(tid);
            LOGI(WXSJ1_LOG " fast-retry thread started");
        } else {
            g_retryStarted.store(false, std::memory_order_release);
            LOGE(WXSJ1_LOG " pthread_create failed");
        }
    }
    return g_hooked.load();
}

bool Wxsjdz1_IsReady() { return g_hooked.load(); }

void Wxsjdz1_ForceClear() {
    LOGI(WXSJ1_LOG " ForceClear (hook kept, features config-driven)");
}

// ============================================================
//  配置写盘（与 Wxsjdz2Mod / WxsjVc1637Mod 同一实现，逐字节等价）
//  路径：/data/user/0/<本包名>/files/starry_config.ini
//  包名从 /proc/self/cmdline 取 ⇒ 断网版（infinityOffline）天然独立，不与官方包串档
// ============================================================
static std::string PkgName() {
    char b[160] = {0};
    FILE* f = fopen("/proc/self/cmdline", "rb");
    if (!f) return "";
    size_t n = fread(b, 1, sizeof(b) - 1, f);
    fclose(f);
    if (!n) return "";
    b[n] = 0;
    return std::string(b);
}

static bool ReadFileLines(const std::string& path, std::vector<std::string>* out) {
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
        out->push_back(all.substr(pos, e - pos));
        pos = e + 1;
    }
    return true;
}

static std::string ConfigPath() {
    std::string pkg = PkgName();
    if (pkg.empty()) return std::string();
    return "/data/user/0/" + pkg + "/files/starry_config.ini";
}

static std::string ExternalConfigPath() {
    std::string pkg = PkgName();
    if (pkg.empty()) return std::string();
    return "/storage/emulated/0/Android/data/" + pkg + "/files/starry_config.ini";
}

// 2026-11-15 修正（用户实测「所有功能都没效果」根因）：
//   补丁 DLL 里的 StarryConfig 读的是 UnityEngine.Application.persistentDataPath
//   = /storage/emulated/0/Android/data/<pkg>/files/starry_config.ini（**外部**目录），
//   而本层原来只写 /data/user/0/<pkg>/files（**内部**目录）⇒ DLL 永远读不到 ⇒ 全默认关。
//   改为**双写**：内部 + 外部都写一份，DLL 用哪个路径都能读到。
static void WriteOneConfigFile(const std::string& path, const char* key, const std::string& val) {
    if (path.empty()) return;
    std::string dir = path.substr(0, path.rfind('/'));
    if (dir.empty()) return;
    mkdir(dir.c_str(), 0755);
    std::vector<std::string> lines;
    bool exists = ReadFileLines(path, &lines);
    std::string tmp = path + ".tmp";
    FILE* f = fopen(tmp.c_str(), "wb");
    if (!f) {
        LOGE(WXSJ1_LOG " cannot write %s", tmp.c_str());
        return;
    }
    bool replaced = false;
    for (auto& l : lines) {
        if (l.compare(0, strlen(key), key) == 0 && l.size() > strlen(key) && l[strlen(key)] == '=') {
            fprintf(f, "%s=%s\n", key, val.c_str());
            replaced = true;
        } else if (!l.empty()) {
            fprintf(f, "%s\n", l.c_str());
        }
    }
    if (!replaced) fprintf(f, "%s=%s\n", key, val.c_str());
    fclose(f);
    rename(tmp.c_str(), path.c_str());
    (void)exists;
    LOGI(WXSJ1_LOG " config %s=%s (exists=%d)", key, val.c_str(), exists ? 1 : 0);
}

// 双写：内部（native 原路径，向后兼容）+ 外部（Unity persistentDataPath，DLL 真正读的那份）
static void WriteConfigKey(const char* key, const std::string& val) {
    std::string inner = ConfigPath();
    std::string outer = ExternalConfigPath();
    if (inner.empty() && outer.empty()) {
        LOGE(WXSJ1_LOG " pkg name empty, config write skipped (%s)", key);
        return;
    }
    if (!inner.empty()) WriteOneConfigFile(inner, key, val);
    if (!outer.empty() && outer != inner) WriteOneConfigFile(outer, key, val);
}

// ============================================================
//  开关回调：feat 号 -> 配置键（键名与 12 线补丁链同名，避免双套命名）
// ============================================================
void Wxsjdz1_OnFeatureChanged(int featNum, bool enabled) {
    const char* on = enabled ? "1" : "0";
    switch (featNum) {
    case WXSJ1_FEAT_GOD:         WriteConfigKey("god", on); break;
    case WXSJ1_FEAT_ONESHOT:     WriteConfigKey("oneshot", on); break;
    case WXSJ1_FEAT_FREEADS:     WriteConfigKey("free_ads", on); break;
    case WXSJ1_FEAT_ITEMKEEP:    WriteConfigKey("item_keep", on); break;
    case WXSJ1_FEAT_SKILLFREE:   WriteConfigKey("skill_free", on); break;
    case WXSJ1_FEAT_MIJINGFREE:  WriteConfigKey("mijing_free", on); break;
    case WXSJ1_FEAT_SKINUNLOCK:  WriteConfigKey("skin_unlock", on); break;
    case WXSJ1_FEAT_FUNCUNLOCK:  WriteConfigKey("func_unlock", on); break;
    // ---- wxsjdz1 独有 4 项（阶段 B 生效）----
    case WXSJ1_FEAT_SHOPADS:     WriteConfigKey("shop_ads_unlimited", on); break;
    case WXSJ1_FEAT_ITEMREVERSE: WriteConfigKey("item_reverse", on); break;
    case WXSJ1_FEAT_ZHUANSHENG:  WriteConfigKey("zhuansheng_free", on); break;
    case WXSJ1_FEAT_LEGACYFREE:  WriteConfigKey("zhuan_sheng_free_point", on); break;
    default: break;
    }
}

// ============================================================
//  输入框回调（feat 7/8/12/15）：只收纯数字，1..1,000,000 夹取
// ============================================================
// 滑块回调（feat 18/19）：DLL 侧读 skill_projectile_x / skill_damage_x
void Wxsjdz1_OnValueChanged(int featNum, int value) {
    const char* key = nullptr;
    int lo = 1, hi = 100;
    switch (featNum) {
    case WXSJ1_FEAT_PROJECTILE: key = "skill_projectile_x"; lo = 1; hi = 100;  break;
    case WXSJ1_FEAT_SKILLDMG:   key = "skill_damage_x";    lo = 1; hi = 1000; break;
    case WXSJ1_FEAT_CHENGHAO:   key = "chenghao_lv";       lo = 1; hi = 20;   break;
    default: return;
    }
    if (value < lo) value = lo;
    if (value > hi) value = hi;
    char buf[16];
    snprintf(buf, sizeof(buf), "%d", value);
    WriteConfigKey(key, buf);
}

void Wxsjdz1_OnInputChanged(int featNum, const char* text) {
    const char* key = nullptr;
    switch (featNum) {
    case WXSJ1_FEAT_ADSREWARD:  key = "ads_reward_x";  break;
    case WXSJ1_FEAT_ITEMDOUBLE: key = "item_double_x"; break;
    case WXSJ1_FEAT_EXPMULTI:   key = "exp_multi";     break;
    case WXSJ1_FEAT_ITEMREVX:   key = "item_reverse_x"; break;
    default:
        return;
    }
    if (text == nullptr || *text == '\0') return;   // 冷启动空串不落盘（保留上一轮值/官方缺省）
    for (const char* p = text; *p != '\0'; ++p) {
        if (*p < '0' || *p > '9') return;
    }
    long long v = strtoll(text, nullptr, 10);
    if (v < kWxsj1ValueMin) v = kWxsj1ValueMin;
    if (v > kWxsj1ValueMax) v = kWxsj1ValueMax;
    char buf[24];
    snprintf(buf, sizeof(buf), "%lld", v);
    WriteConfigKey(key, buf);
}
