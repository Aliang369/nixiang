/* ================================================================
 *  无限升级 · 星光版 21 槽 — 13.101 / TapTap 官方包
 *  （详见 Wxsjdz1NewMod.h；本文件 = 13.x 注入链 + 21 槽配置契约）
 *
 *  行为骨架**照抄 Games/WxsjMod.cpp（13.x 线）**：
 *    · waitLib = libil2cpp.so（入口表给）
 *    · 挂点 RVA = libil2cpp.so + 0x2b890d8
 *      （RuntimeAssemblyLoader::ExecuteRuntimeAssembly；vc1644 起同址，2026-09-23
 *       真机运行时映射重解实锤，13.101 沿用；序言 16 字节逐字节校验，漂移即拒 hook）
 *    · 变体表 kDllVariants + DIAG/PINNED 逻辑
 *    · 补丁 > 官方数组时走 mode=selfarray（malloc 自建同布局 byte[]）
 *    · hook 装上即无条件武装（与玩家开不开功能无关）
 *  与 WxsjMod.cpp 的差异（仅此四处，均为隔离/安全目的）：
 *    ① 符号前缀 Wxsj1New_*、日志前缀 [Wxsj1New]，避免与既有 Wxsjdz1* / Wxsj* 重名；
 *    ② 配置**只写内部路径** /data/user/0/<pkg>/files/starry_config.ini（与 13.x WxsjMod.cpp 同）。
 *       依据：13.101 补丁链的 StarryConfig helper（$Obfuz$ConstFieldHolder$0::Get/GetInt）
 *       读的就是这条内部路径，13.x 已交付版实测生效；dz1 当初双写 external 是因为**断网版**
 *       DLL 走 Unity persistentDataPath，属另一套底包，不得套到本包（会在外置存储留冗余副本）；
 *    ③ feat 7/8 由 Toggle 语义改为 **InputValue 传倍数**，并补 OnInputChanged（feat 7/8/12/15）；
 *    ④ PINNED 兜底加长度下限 kPinnedMinLen：装载到的 DLL 明显不是 13.x 系（<2.6MB）时
 *       不硬塞 2.76MB 补丁，改为透传官方版（宁可本次无功能，不留错版崩溃）。
 *
 *  载荷已定稿（2026-11-20，fix1：修 feat8 键名错配 item_double→item_double_x）：19 项 allfix
 *   official (2,748,416 B / fnv 0x2ab99f0abb45f1ea) → patched (2,764,288 B / fnv 0x768949eea17b309d
 *   / md5 a184d32e…)，超长 +15,872 ⇒ 运行时恒走 mode=selfarray。
 *   .a 来源：Games/Wxsjdz1New19FeatDll.a（objcopy 自 07_patched/无限升级/13101_new/
 *   targetfuntional_13101new_allfix_19feat.dll）。换载荷只改：本文件两个符号名 + kDllVariants 首行 + mk 的模块源。
 * ================================================================ */
#include "Wxsjdz1NewMod.h"

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
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节）

#define WXSJ1NEW_LOG "[Wxsj1New]"
#if defined(STARRY_TRACE) && STARRY_TRACE
#define WXSJ1NEW_TRACE(...) __android_log_print(ANDROID_LOG_INFO, "StarryTrace", WXSJ1NEW_LOG " " __VA_ARGS__)
#else
#define WXSJ1NEW_TRACE(...) ((void)0)
#endif

// ============================================================
//  挂点身份（13.x 线：与 WxsjMod.cpp 同一组常量，逐字节一致）
// ============================================================
static constexpr uintptr_t kExecRva = 0x2b890d8;
static const uint8_t kExecPrologue[16] = {
    0xfe, 0x0f, 0x1c, 0xf8, 0xf8, 0x5f, 0x01, 0xa9,
    0xf6, 0x57, 0x02, 0xa9, 0xf4, 0x4f, 0x03, 0xa9,
};
static const char kDllNameKey[] = "targetfuntional";

// ============================================================
//  载荷（明文内嵌，objcopy 成 ELF 可重定位目标）
//  ⚠ 换载荷时**只改这两个符号名 + 下面 kDllVariants 首行**（其余行共用同一份载荷）
// ============================================================
extern "C" const uint8_t _binary_wxsjdz1new_19feat_start[];
extern "C" const uint8_t _binary_wxsjdz1new_19feat_end[];
static const uint8_t* const kPayloadStart = _binary_wxsjdz1new_19feat_start;
static const uint8_t* const kPayloadEnd   = _binary_wxsjdz1new_19feat_end;

struct DllVariant {
    int32_t officialLen;    // 官方运行时 byte[] 长度
    uint64_t officialHash;  // 官方整包 fnv1a64
    uint64_t patchedBufHash;// 内嵌补丁整包 fnv1a64（二次装载幂等判据）
};
// 13.x 系官方长度（2.73–2.75MB）：全部钉到同一份内嵌补丁；不在表内且 >= kPinnedMinLen 的
// 新版本走 PINNED（同样钉到本补丁 = 「不让游戏热更生效，等人工换包」，13.x 既定策略）。
static const DllVariant kDllVariants[] = {
    // 【首行 = 本轮定稿载荷】官方 2026-11-19 14:23 版（DIAG 运行时落盘实锤：
    //   2,748,416 B / fnv 0x2ab99f0abb45f1ea）→ 19 项 allfix 补丁 fix1
    //   2,764,288 B / fnv 0x768949eea17b309d（超长 +15,872 ⇒ selfarray；修 feat8 键名错配）
    {2748416, 0x2ab99f0abb45f1eaULL, 0x768949eea17b309dULL},
    // 兜底：2026-11-19 更早一版（2,746,368 / 0xd303f1e9c7154db9）——同一份载荷钉死
    {2746368, 0xd303f1e9c7154db9ULL, 0x768949eea17b309dULL},
    // 兜底：2026-11-18 13.101 初版（2,738,688 / 0xd31dc94b29a8bb0d）——同一份载荷钉死
    {2738688, 0xd31dc94b29a8bb0dULL, 0x768949eea17b309dULL},
};
// 低于此长度说明装载的不是 13.x 系 DLL（例如 12.x 的 2.02MB）⇒ 不钉，透传官方
static constexpr int32_t kPinnedMinLen = 2600000;

static const DllVariant* PickDllVariant(int32_t len, uint64_t h) {
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len && v.officialHash == h) return &v;
    }
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len) return &v;
    }
    return nullptr;
}

// ---- 小工具 ----
static uint64_t Fnv1a64(const uint8_t* p, size_t n) {
    uint64_t h = 0xcbf29ce484222325ULL;
    for (size_t i = 0; i < n; i++) { h ^= (uint64_t)p[i]; h *= 0x100000001b3ULL; }
    return h;
}

static bool ContainsDllNameKey(const uint8_t* bytes, size_t len) {
    const size_t klen = sizeof(kDllNameKey) - 1;
    for (size_t o = 0; o + klen <= len; o++) {
        if (bytes[o] == 't' && memcmp(bytes + o, kDllNameKey, klen) == 0) return true;
    }
    return false;
}

typedef uint8_t (*Exec_t)(void* self, void* data, void* outP, void* mi);

static std::mutex g_mtx;
static Exec_t g_origExec = nullptr;
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_replaceArmed{false};
static std::atomic<int> g_hits{0};
static std::atomic<bool> g_retryStarted{false};
static std::atomic<int> g_forced{0};      // 「非已知底包但已钉死注入」计数（只用于首次提示）

// selfarray 缓存：同一 (len, hash) 只建一次，指针活到进程结束（不 free）
static void* g_swapArr = nullptr;
static uint64_t g_swapFor = 0;
static int32_t g_swapLen = 0;

static std::string PkgName();
static bool ReadFileLines(const std::string& path, std::vector<std::string>* out);
static void WriteConfigKey(const char* key, const std::string& val);

// ---- libil2cpp.so 基址（dlpi_addr = 首映射 = VA 基准）----
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

extern "C" void StarryShowToast(const char* text);  // MainModern.cpp 导出（:194）

// ---- 测试钩子：files/starry_pin_test 存在 ⇒ 强制走「未知版本」路径（真机验证钉死注入）----
static bool PinTest() {
    std::string pkg = PkgName();
    if (pkg.empty()) return false;
    return access(("/data/user/0/" + pkg + "/files/starry_pin_test").c_str(), F_OK) == 0;
}

// ---- DIAG 取证模式（-DWXSJ1NEW_DIAG_DUMP=1）：只落盘官方运行时 DLL，不替换 ----
#if defined(WXSJ1NEW_DIAG_DUMP) && WXSJ1NEW_DIAG_DUMP
static std::atomic<int> g_diagCount{0};
static void DiagDumpDll(const uint8_t* bytes, int32_t len, const char* head) {
    if (len < 400000) return;
    int n = g_diagCount.load();
    if (n >= 8) return;
    g_diagCount.store(n + 1);
    std::string pkg = PkgName();
    if (pkg.empty()) { LOGE(WXSJ1NEW_LOG " DIAG pkg empty"); return; }
    char name[160];
    snprintf(name, sizeof(name), "/data/user/0/%s/files/wxsjdz1new_dump_%d_%s.bin",
             pkg.c_str(), len, head);
    FILE* f = fopen(name, "wb");
    if (!f) { LOGE(WXSJ1NEW_LOG " DIAG cannot write %s", name); return; }
    size_t w = fwrite(bytes, 1, (size_t)len, f);
    fclose(f);
    LOGI(WXSJ1NEW_LOG " DIAG dumped %zu/%d bytes -> %s", w, len, name);
}
#endif

// ============================================================
//  hook 函数：把 targetfuntional 的装载缓冲区换成内嵌补丁
//  data = Il2CppArray（byte[]）：+0x18 = max_length(int32)，+0x20 = 内联数据起点
// ============================================================
static uint8_t HookExec(void* self, void* data, void* outP, void* mi) {
    Exec_t orig = g_origExec;
    if (orig && starptr::LiveFast(data)) {
        int32_t len = 0;
        memcpy(&len, (const uint8_t*)data + 0x18, sizeof(len));
        const uint8_t* bytes = (const uint8_t*)data + 0x20;
        const bool isDll = (len >= 2 && bytes[0] == 'M' && bytes[1] == 'Z');

#if defined(WXSJ1NEW_DIAG_DUMP) && WXSJ1NEW_DIAG_DUMP
        if (isDll) {
            char head[16] = {0};
            snprintf(head, sizeof(head), "%02x%02x%02x%02x", bytes[0], bytes[1], bytes[2], bytes[3]);
            DiagDumpDll(bytes, len, head);
        }
#else
        // 深过滤：全数组内必须出现程序集名（把 targetfuntional 与 mscorlib 等热更程序集分开）
        if (g_replaceArmed.load(std::memory_order_relaxed) && isDll
            && ContainsDllNameKey(bytes, (size_t)len)) {
            const uint64_t h = Fnv1a64(bytes, (size_t)len);
            const DllVariant* v = PickDllVariant(len, h);
            const size_t pLen = (size_t)(kPayloadEnd - kPayloadStart);

            // 幂等：装载缓冲区里已经是我们的补丁（原地覆写过）⇒ 放行
            bool matchedPatchedBuf = false;
            if (len == (int32_t)pLen && memcmp(bytes, kPayloadStart, pLen) == 0) {
                matchedPatchedBuf = true;
            } else {
                for (const auto& cv : kDllVariants) {
                    if (cv.officialLen == len && cv.patchedBufHash == h) { matchedPatchedBuf = true; break; }
                }
            }
            if (matchedPatchedBuf) {
                LOGI(WXSJ1NEW_LOG " Exec re-enter: buffer already carries our patch"
                                  " (hit#%d fnv=%016llx) -> passthrough",
                     g_hits.load(), (unsigned long long)h);
                return orig(self, data, outP, mi);
            }

            // PINNED：已知表外的新版本（13.x 系长度）一律钉到内嵌补丁 = 热更不生效
            bool pinned = false;
            if (!v || PinTest()) {
                if (len >= kPinnedMinLen || PinTest()) {
                    v = &kDllVariants[0];
                    pinned = true;
                    LOGE(WXSJ1NEW_LOG " PINNED unknown official len=%d fnv=%016llx -> patched=%zu"
                                      " (hot-update blocked)", len, (unsigned long long)h, pLen);
                } else {
                    // 明显不是 13.x 系底包：透传官方版（宁可本次无功能，不留错版崩溃）
                    g_forced.fetch_add(1);
                    LOGE(WXSJ1NEW_LOG " embedded %zuB vs array %d : PASSTHROUGH official"
                                      " (foreign base dll, rebuild APK to restore features)", pLen, len);
                    return orig(self, data, outP, mi);
                }
            }
            (void)pinned;

            if (pLen == 0 || kPayloadStart >= kPayloadEnd) {
                LOGE(WXSJ1NEW_LOG " empty embedded payload : PASSTHROUGH official");
                return orig(self, data, outP, mi);
            }

            if (pLen <= (size_t)len) {
                // 放得下：原地覆写（官方数组长度不变，尾零填充）
                memcpy((void*)bytes, kPayloadStart, pLen);
                memset((void*)(bytes + pLen), 0, (size_t)len - pLen);
                int n = g_hits.fetch_add(1) + 1;
                LOGI(WXSJ1NEW_LOG " patched dll injected (hit#%d mode=inplace loaded=%d"
                                  " fnv=%016llx patched=%zu pad=%d)",
                     n, len, (unsigned long long)h, pLen, (int)((size_t)len - pLen));
                WXSJ1NEW_TRACE("dll replaced (inplace)");
                return orig(self, data, outP, mi);
            }

            // 超长：自建「同布局字节数组」（++0x00 klass / +0x08 monitor / +0x10 bounds /
            //   +0x18 max_length(int32) / +0x20 内联数据）。放 malloc 区（非 GC 堆）：
            //   Boehm 保守 GC 不扫描/不回收；解释器按指针惰性读 IL ⇒ 必须活到进程结束。
            if (g_swapArr && g_swapFor == h && g_swapLen == len) {
                LOGI(WXSJ1NEW_LOG " patched dll injected (hit#%d mode=selfarray cached buf=%p)",
                     g_hits.load(), g_swapArr);
                return orig(self, g_swapArr, outP, mi);
            }
            const int32_t pLen32 = (int32_t)pLen;
            const size_t fakeSz = 0x20 + pLen + 8;
            uint8_t* fake = (uint8_t*)malloc(fakeSz);
            if (fake) {
                memcpy(fake, data, 0x20);            // klass/monitor/bounds 原样
                memcpy(fake + 0x18, &pLen32, 4);     // max_length = 补丁长度
                memcpy(fake + 0x20, kPayloadStart, pLen);
                g_swapArr = fake;
                g_swapFor = h;
                g_swapLen = len;
                int n = g_hits.fetch_add(1) + 1;
                LOGI(WXSJ1NEW_LOG " patched dll injected (hit#%d mode=selfarray loaded=%d"
                                  " fnv=%016llx patched=%zu pad=%d buf=%p)",
                     n, len, (unsigned long long)h, pLen, (int)((int64_t)pLen - (int64_t)len), (void*)fake);
                WXSJ1NEW_TRACE("dll replaced (selfarray)");
                return orig(self, fake, outP, mi);
            }
            LOGE(WXSJ1NEW_LOG " malloc(%zu) for self-array failed : PASSTHROUGH official", fakeSz);
            g_forced.fetch_add(1);
        }
#endif  // WXSJ1NEW_DIAG_DUMP
    }
    return orig(self, data, outP, mi);
}

// ============================================================
//  hook 安装（100ms 级快速重试：热更装载在 libil2cpp 解密后 ~1.5s 内开始）
// ============================================================
static bool TryHookOnce() {
    uintptr_t base = Il2CppBase();
    if (!base) return false;
    uint8_t* target = (uint8_t*)(base + kExecRva);
    uint8_t cur[16];
    memcpy(cur, target, sizeof(cur));
    if (memcmp(cur, kExecPrologue, sizeof(cur)) != 0) return false;  // 壳未解密/版本漂移：静默重试

    void* orig = nullptr;
    if (DobbyHook(target, (void*)HookExec, &orig) != 0 || !orig) {
        LOGE(WXSJ1NEW_LOG " DobbyHook failed");
        return false;
    }
    g_origExec = (Exec_t)orig;
    g_hooked.store(true);
    g_replaceArmed.store(true);   // 13.x 口径：钩子装上即武装，与菜单开关无关
    const size_t pLen = (size_t)(kPayloadEnd - kPayloadStart);
    LOGI(WXSJ1NEW_LOG " hook installed @base=%p +0x%zx orig=%p variants=%zu payload=%zuB",
         (void*)base, kExecRva, orig,
         sizeof(kDllVariants) / sizeof(kDllVariants[0]), pLen);
    // 13.x 防封基线：config 里没有 block_report 时补 1（dll 侧 19 通道门用它，菜单不外露）
    std::string pkg = PkgName();
    if (!pkg.empty()) {
        std::vector<std::string> lines;
        ReadFileLines("/data/user/0/" + pkg + "/files/starry_config.ini", &lines);
        bool hasBr = false;
        for (auto& l : lines) {
            if (l.compare(0, 12, "block_report") == 0 && l.size() > 12 && l[12] == '=')
                hasBr = true;
        }
        if (!hasBr) WriteConfigKey("block_report", "1");
    }
    return true;
}

static void* FastRetryThread(void*) {
    for (int i = 0; i < 600 && !g_hooked.load(); i++) {
        if (TryHookOnce()) return nullptr;
        usleep(100 * 1000);
    }
    if (!g_hooked.load()) {
        LOGE(WXSJ1NEW_LOG " fast-retry 60s 超时放弃（允许下一轮 tryInit 重启线程）");
        g_retryStarted.store(false, std::memory_order_release);
    }
    return nullptr;
}

bool Wxsj1New_TryInit() {
    if (g_hooked.load()) return true;
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return true;
    if (!g_retryStarted.exchange(true, std::memory_order_acq_rel)) {
        pthread_t tid;
        if (pthread_create(&tid, nullptr, FastRetryThread, nullptr) == 0) {
            pthread_detach(tid);
            LOGI(WXSJ1NEW_LOG " fast-retry thread started");
        } else {
            g_retryStarted.store(false, std::memory_order_release);
            LOGE(WXSJ1NEW_LOG " pthread_create failed");
        }
    }
    return g_hooked.load();
}

bool Wxsj1New_IsReady() { return g_hooked.load(); }

void Wxsj1New_ForceClear() {
    // hook 本身不改变行为（配置缺省=全关=原版），授权失效保留 hook 无副作用
    LOGI(WXSJ1NEW_LOG " ForceClear (hook kept, features config-driven)");
}

// ============================================================
//  配置写盘（双写：内部 + 外部 Unity persistentDataPath）
//  DLL 侧 StarryConfig 读 GetBool/GetInt（2s 缓存）；native 只写文件，禁碰 IL2CPP/UI
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

static std::string InternalConfigPath() {
    std::string pkg = PkgName();
    if (pkg.empty()) return std::string();
    return "/data/user/0/" + pkg + "/files/starry_config.ini";
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
        LOGE(WXSJ1NEW_LOG " cannot write %s", tmp.c_str());
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
    LOGI(WXSJ1NEW_LOG " config %s=%s (exists=%d)", key, val.c_str(), exists ? 1 : 0);
}

// 单写内部路径（理由见文件头 ②）：13.x 补丁链读 /data/user/0/<pkg>/files/starry_config.ini；
// 包名从 /proc/self/cmdline 取 ⇒ 官方包与断网版天然各写各的，不串档。
static void WriteConfigKey(const char* key, const std::string& val) {
    std::string inner = InternalConfigPath();
    if (inner.empty()) {
        LOGE(WXSJ1NEW_LOG " pkg name empty, config write skipped (%s)", key);
        return;
    }
    WriteOneConfigFile(inner, key, val);
}

// 数值输入框夹取范围（与 11 线 / dz1 口径一致：1..1,000,000）
static const long long kValueMin = 1;
static const long long kValueMax = 1000000;

// ============================================================
//  开关回调：12 个 Toggle（feat 7/8/12/15 是 InputValue，不在此处）
// ============================================================
void Wxsj1New_OnFeatureChanged(int featNum, bool enabled) {
    const char* on = enabled ? "1" : "0";
    switch (featNum) {
    case WXSJ1NEW_FEAT_GOD:         WriteConfigKey("god", on); break;
    case WXSJ1NEW_FEAT_ONESHOT:     WriteConfigKey("oneshot", on); break;
    case WXSJ1NEW_FEAT_FREEADS:     WriteConfigKey("free_ads", on); break;
    case WXSJ1NEW_FEAT_ITEMKEEP:    WriteConfigKey("item_keep", on); break;
    case WXSJ1NEW_FEAT_SKILLFREE:   WriteConfigKey("skill_free", on); break;
    case WXSJ1NEW_FEAT_MIJINGFREE:  WriteConfigKey("mijing_free", on); break;
    case WXSJ1NEW_FEAT_SKINUNLOCK:  WriteConfigKey("skin_unlock", on); break;
    case WXSJ1NEW_FEAT_FUNCUNLOCK:  WriteConfigKey("func_unlock", on); break;
    // ---- 21 槽新增 4 个 Toggle ----
    case WXSJ1NEW_FEAT_SHOPADS:     WriteConfigKey("shop_ads_unlimited", on); break;
    case WXSJ1NEW_FEAT_ITEMREVERSE: WriteConfigKey("item_reverse", on); break;
    case WXSJ1NEW_FEAT_ZHUANSHENG:  WriteConfigKey("zhuansheng_free", on); break;
    case WXSJ1NEW_FEAT_LEGACYFREE:  WriteConfigKey("zhuan_sheng_free_point", on); break;
    default: break;   // 0/3 = Category_ 分组行，无功能体
    }
}

// ============================================================
//  滑块回调：feat 17/18/20（1..100 / 1..1000 / 1..20）
// ============================================================
void Wxsj1New_OnValueChanged(int featNum, int value) {
    const char* key = nullptr;
    int lo = 1, hi = 100;
    switch (featNum) {
    case WXSJ1NEW_FEAT_PROJECTILE: key = "skill_projectile_x"; lo = 1; hi = 100;  break;
    case WXSJ1NEW_FEAT_SKILLDMG:   key = "skill_damage_x";    lo = 1; hi = 1000; break;
    case WXSJ1NEW_FEAT_CHENGHAO:   key = "chenghao_lv";       lo = 1; hi = 20;   break;
    default: return;
    }
    if (value < lo) value = lo;
    if (value > hi) value = hi;
    char buf[16];
    snprintf(buf, sizeof(buf), "%d", value);
    WriteConfigKey(key, buf);
}

// ============================================================
//  输入框回调：feat 7/8/12/15（1=官方原样；0/空 = 不落盘，保留上一轮值）
// ============================================================
void Wxsj1New_OnInputChanged(int featNum, const char* text) {
    const char* key = nullptr;
    switch (featNum) {
    case WXSJ1NEW_FEAT_ADSREWARD:  key = "ads_reward_x";    break;
    case WXSJ1NEW_FEAT_ITEMDOUBLE: key = "item_double_x";   break;
    case WXSJ1NEW_FEAT_EXPMULTI:   key = "exp_multi";       break;
    case WXSJ1NEW_FEAT_ITEMREVX:   key = "item_reverse_x";  break;
    default: return;
    }
    if (text == nullptr || *text == '\0') return;   // 冷启动空串不落盘
    for (const char* p = text; *p != '\0'; ++p) {
        if (*p < '0' || *p > '9') return;             // 非纯数字：丢弃，保持上次值
    }
    long long v = strtoll(text, nullptr, 10);
    if (v < kValueMin) v = kValueMin;
    if (v > kValueMax) v = kValueMax;
    char buf[24];
    snprintf(buf, sizeof(buf), "%lld", v);
    WriteConfigKey(key, buf);
}
