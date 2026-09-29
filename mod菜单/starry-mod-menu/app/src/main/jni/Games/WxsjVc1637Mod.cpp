/* ================================================================
 *  无限升级 11.266018 / vc1637 — 星空版 native 模块
 *
 *  与 12.x 的 WxsjMod 完全隔离：
 *    - RuntimeAssemblyLoader::ExecuteRuntimeAssembly RVA = 0x2b3f3c4
 *    - 内嵌 targetfuntional_dll_vc1637.bin，装载时强制覆写
 *    - 不匹配官方长度时也覆写（只要缓冲区容得下），实现版本钉死
 *
 *  功能开关由 DLL 内 StarryConfig 读取 starry_config.ini（2s 缓存）。
 * ================================================================ */
#include "WxsjVc1637Mod.h"
#include "Includes/SecStrings.hpp"

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

#define WXSJVC_LOG "[WxsjVC1637]"

// vc1637 / 11.266018 真机运行时映射实锤。
static constexpr uintptr_t kExecRva = 0x2b3f3c4;
static const uint8_t kExecPrologue[16] = {
    0xfe, 0x0f, 0x1c, 0xf8, 0xf8, 0x5f, 0x01, 0xa9,
    0xf6, 0x57, 0x02, 0xa9, 0xf4, 0x4f, 0x03, 0xa9,
};

static const char kDllNameKey[] = "targetfuntional";
static constexpr int32_t kOfficialDllLen = 1960960;  // 仅日志兜底用

// ---- 三版本变体：按运行时下发的官方 DLL 长度/哈希选对应补丁 ----
// 2026-09-29 真机实测：服务端给 vc1637 下发的是 hotfix2 2,030,080 B，不是分析期假设的
// 1,960,960 B。ildiff 实测两版差 52 类型 / 512 方法（整个 MissionTree 系统），
// 把 1960960 系补丁注入 2030080 官方包 = A 线当年「MissionTreeData 序列化异常 / 卡等待
// 响应」的复现。无匹配变体时一律 PASSTHROUGH 官方版（宁可不生效，不能错版）。
struct DllVariant {
    int32_t officialLen;
    uint64_t officialHash;
    const char* name;
    const uint8_t* start;
    const uint8_t* end;
};
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf2_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf2_bin_end[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf3_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_hf3_bin_end[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_bin_start[];
extern "C" const uint8_t _binary_targetfuntional_dll_vc1637_bin_end[];

static const DllVariant kDllVariants[] = {
    {2030080, 0x91f9acbd064fa875ULL, "hf2",  _binary_targetfuntional_dll_vc1637_hf2_bin_start, _binary_targetfuntional_dll_vc1637_hf2_bin_end},
    {2028544, 0x43df8d0d87bf08b2ULL, "hf3",  _binary_targetfuntional_dll_vc1637_hf3_bin_start, _binary_targetfuntional_dll_vc1637_hf3_bin_end},
    {1960960, 0x6f07494d17997a03ULL, "base", _binary_targetfuntional_dll_vc1637_bin_start,     _binary_targetfuntional_dll_vc1637_bin_end},
};

static const DllVariant* PickDllVariant(int32_t len, uint64_t h) {
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len && v.officialHash == h) return &v;
    }
    for (const auto& v : kDllVariants) {
        if (v.officialLen == len) return &v;
    }
    return nullptr;
}

typedef uint8_t (*Exec_t)(void* self, void* data, void* outP, void* mi);

static std::mutex g_mtx;
static Exec_t g_origExec = nullptr;
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_replaceArmed{false};
static std::atomic<int> g_hits{0};
static std::atomic<int> g_pinNotice{0};
static std::atomic<bool> g_retryStarted{false};
static const char* g_lastVariant = "none";

static bool ReadFileLines(const std::string& path, std::vector<std::string>* out);
static std::string PkgName();
static void WriteConfigKey(const char* key, const std::string& val);

static bool OkPtr(void* p) {
    uintptr_t v = (uintptr_t)p;
    return p && v >= 0x100000000ULL && v < 0x800000000000ULL;
}

static int PhdrCb(struct dl_phdr_info* info, size_t, void* data) {
    uintptr_t* out = (uintptr_t*)data;
    const char* n = info->dlpi_name;
    if (n && strlen(n) >= 12 && strcmp(n + strlen(n) - 12, sec_lib_il2cpp()) == 0) {
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

extern "C" void StarryShowToast(const char* text);

static bool ContainsDllNameKey(const uint8_t* bytes, size_t len) {
    const size_t klen = sizeof(kDllNameKey) - 1;
    for (size_t o = 0; o + klen <= len; o++) {
        if (bytes[o] == 't' && memcmp(bytes + o, kDllNameKey, klen) == 0) return true;
    }
    return false;
}

static uint64_t Fnv1a64(const uint8_t* p, size_t n) {
    uint64_t h = 0xcbf29ce484222325ULL;
    for (size_t i = 0; i < n; i++) { h ^= (uint64_t)p[i]; h *= 0x100000001b3ULL; }
    return h;
}

static void WritePinMarker(int32_t loadedLen, size_t patchedLen) {
    std::string pkg = PkgName();
    if (pkg.empty()) return;
    std::string path = "/data/user/0/" + pkg + "/files/starry_hotfix_pinned";
    FILE* f = fopen(path.c_str(), "wb");
    if (!f) return;
    fprintf(f, "vc1637 loaded=%d patched=%zu variant=%s action=forced-patch\n",
            loadedLen, patchedLen, g_lastVariant);
    fclose(f);
}

static uint8_t HookExec(void* self, void* data, void* outP, void* mi) {
    Exec_t orig = g_origExec;
    if (orig && OkPtr(data)) {
        int32_t len = 0;
        memcpy(&len, (const uint8_t*)data + 0x18, sizeof(len));

        char head[16] = {0};
        const uint8_t* bytes = (const uint8_t*)data + 0x20;
        if (len >= 4) {
            snprintf(head, sizeof(head), "%02x%02x%02x%02x", bytes[0], bytes[1], bytes[2], bytes[3]);
        }
        LOGI(WXSJVC_LOG " Exec call len=%d head=%s armed=%d", len, head,
             g_replaceArmed.load(std::memory_order_relaxed) ? 1 : 0);

        const bool isDll = len >= 2 && bytes[0] == 'M' && bytes[1] == 'Z';
        if (g_replaceArmed.load(std::memory_order_relaxed) && isDll
            && ContainsDllNameKey(bytes, (size_t)len)) {
            const uint64_t h = Fnv1a64(bytes, (size_t)len);
            const DllVariant* v = PickDllVariant(len, h);
            if (!v) {
                if (g_pinNotice.fetch_add(1) == 0) {
                    StarryShowToast("[无限升级] 游戏热更版本未识别，本次以官方版本运行（存档安全）。"
                                    "新功能请等人工更新安装包");
                }
                LOGE(WXSJVC_LOG " no variant for len=%d fnv=%016llx : PASSTHROUGH official",
                     len, (unsigned long long)h);
                return orig(self, data, outP, mi);
            }
            const size_t pLen = (size_t)(v->end - v->start);
            if (pLen == 0 || pLen > (size_t)len) {
                LOGE(WXSJVC_LOG " variant %s patched %zuB > loaded %dB: PASSTHROUGH official",
                     v->name, pLen, len);
                return orig(self, data, outP, mi);
            }
            // 幂等：同一装载缓冲区被二次执行（换号 / 重登录 / 程序集重装载）时，
            // 缓冲区里已经是我们这一版补丁，直接放行，不重复覆写。
            if (memcmp(bytes, v->start, pLen) == 0) {
                LOGI(WXSJVC_LOG " buffer already carries %s patch (hit#%d fnv=%016llx) -> passthrough",
                     v->name, g_hits.load(), (unsigned long long)h);
                return orig(self, data, outP, mi);
            }
            memcpy((void*)bytes, v->start, pLen);
            memset((void*)(bytes + pLen), 0, (size_t)len - pLen);
            g_lastVariant = v->name;
            int n = g_hits.fetch_add(1) + 1;
            LOGI(WXSJVC_LOG " patched dll injected (vc1637 %s, hit#%d loaded=%d fnv=%016llx patched=%zu pad=%d)",
                 v->name, n, len, (unsigned long long)h, pLen, (int)((size_t)len - pLen));
            WritePinMarker(len, pLen);
            return orig(self, data, outP, mi);
        }
    }
    return orig(self, data, outP, mi);
}

static bool TryHookOnce() {
    uintptr_t base = Il2CppBase();
    if (!base) return false;

    uint8_t* target = (uint8_t*)(base + kExecRva);
    uint8_t cur[16];
    memcpy(cur, target, sizeof(cur));
    if (memcmp(cur, kExecPrologue, sizeof(cur)) != 0) return false;

    void* orig = nullptr;
    if (DobbyHook(target, (void*)HookExec, &orig) != 0 || !orig) {
        LOGE(WXSJVC_LOG " DobbyHook failed");
        return false;
    }

    g_origExec = (Exec_t)orig;
    g_hooked.store(true);
    g_replaceArmed.store(true);

    size_t pLen = (size_t)(_binary_targetfuntional_dll_vc1637_bin_end
                         - _binary_targetfuntional_dll_vc1637_bin_start);
    (void)pLen;
    LOGI(WXSJVC_LOG " hook installed @base=%p +0x%zx orig=%p variants=hf2:%zuB+hf3:%zuB+base:%zuB",
         (void*)base, kExecRva, orig,
         (size_t)(kDllVariants[0].end - kDllVariants[0].start),
         (size_t)(kDllVariants[1].end - kDllVariants[1].start),
         (size_t)(kDllVariants[2].end - kDllVariants[2].start));

    std::string pkg = PkgName();
    if (!pkg.empty()) {
        std::vector<std::string> lines;
        const std::string cfgPath = "/data/user/0/" + pkg + "/files/starry_config.ini";
        ReadFileLines(cfgPath, &lines);
        bool hasBr = false;
        for (auto& l : lines) {
            if (l.compare(0, 13, "block_report") == 0) hasBr = true;
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
        LOGE(WXSJVC_LOG " fast-retry 60s 超时放弃（允许下一轮 tryInit 重启线程）");
        g_retryStarted.store(false, std::memory_order_release);
    }
    return nullptr;
}

bool WxsjVc1637_TryInit() {
    if (g_hooked.load()) return true;
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return true;
    // MainModern 每 initRetryIntervalMs 回调一次；只允许存在一个 fast-retry 线程。
    if (!g_retryStarted.exchange(true, std::memory_order_acq_rel)) {
        pthread_t tid;
        if (pthread_create(&tid, nullptr, FastRetryThread, nullptr) == 0) {
            pthread_detach(tid);
            LOGI(WXSJVC_LOG " fast-retry thread started");
        } else {
            g_retryStarted.store(false, std::memory_order_release);
            LOGE(WXSJVC_LOG " pthread_create failed");
        }
    }
    return false;
}

bool WxsjVc1637_IsReady() { return g_hooked.load(); }

void WxsjVc1637_ForceClear() {
    LOGI(WXSJVC_LOG " ForceClear (hook kept, config-driven)");
}

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

static void WriteConfigKey(const char* key, const std::string& val) {
    std::string pkg = PkgName();
    if (pkg.empty()) {
        LOGE(WXSJVC_LOG " pkg name empty, config write skipped (%s)", key);
        return;
    }
    std::string dir = "/data/user/0/" + pkg + "/files";
    std::string path = dir + "/starry_config.ini";
    mkdir(dir.c_str(), 0755);
    std::vector<std::string> lines;
    bool exists = ReadFileLines(path, &lines);
    std::string tmp = path + ".tmp";
    FILE* f = fopen(tmp.c_str(), "wb");
    if (!f) {
        LOGE(WXSJVC_LOG " cannot write %s", tmp.c_str());
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
    LOGI(WXSJVC_LOG " config %s=%s (exists=%d)", key, val.c_str(), exists ? 1 : 0);
}

void WxsjVc1637_OnFeatureChanged(int featNum, bool enabled) {
    const char* on = enabled ? "1" : "0";
    switch (featNum) {
    case WXSJVC_FEAT_GOD:        WriteConfigKey("god", on); break;
    case WXSJVC_FEAT_ONESHOT:    WriteConfigKey("oneshot", on); break;
    case WXSJVC_FEAT_FREEADS:    WriteConfigKey("free_ads", on); break;
    case WXSJVC_FEAT_ITEMKEEP:   WriteConfigKey("item_keep", on); break;
    case WXSJVC_FEAT_SKILLFREE:  WriteConfigKey("skill_free", on); break;
    case WXSJVC_FEAT_ADSREWARD:  WriteConfigKey("ads_reward_x", enabled ? "2" : "1"); break;
    case WXSJVC_FEAT_ITEMDOUBLE: WriteConfigKey("item_double", on); break;
    case WXSJVC_FEAT_MIJINGFREE: WriteConfigKey("mijing_free", on); break;
    case WXSJVC_FEAT_SKINUNLOCK: WriteConfigKey("skin_unlock", on); break;
    case WXSJVC_FEAT_FUNCUNLOCK: WriteConfigKey("func_unlock", on); break;
    case WXSJVC_FEAT_SHOPADS:    WriteConfigKey("shop_ads_unlimited", on); break;
    default: break;
    }
}

void WxsjVc1637_OnValueChanged(int featNum, int value) {
    switch (featNum) {
    case WXSJVC_FEAT_EXPMULTI: {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        char buf[8];
        snprintf(buf, sizeof(buf), "%d", value);
        WriteConfigKey("exp_multi", buf);
        break;
    }
    default: break;
    }
}
