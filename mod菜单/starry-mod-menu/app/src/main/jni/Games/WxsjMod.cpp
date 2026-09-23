/* ================================================================
 *  无限升级 — 热更 dll 字节替换（详见 WxsjMod.h）
 *  hook 点：RuntimeAssemblyLoader::ExecuteRuntimeAssembly（AOT，
 *  Assembly.Load(byte[]) 的唯一热更装载路径，反汇编实锤：
 *    x0=this(未用) x1=byte[] data x2=out param x3=MethodInfo*
 *    检查 data 非空且 [data+0x18](len)>0 后直调 Assembly::Load）
 *  hook 内仅 memcmp/memcpy（memcmp 级操作，无 il2cpp API 调用）；
 *  替换发生在游戏 bootstrap 线程（非渲染线程），单次装载触发。
 * ================================================================ */
#include "WxsjMod.h"
#include "Includes/SecStrings.hpp"

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

#define WXSJ_LOG "[Wxsj]"
#if defined(STARRY_TRACE) && STARRY_TRACE
#define WXSJ_TRACE(...) __android_log_print(ANDROID_LOG_INFO, "StarryTrace", WXSJ_LOG " " __VA_ARGS__)
#else
#define WXSJ_TRACE(...) ((void)0)
#endif

// ---- 版本绑定常量（vc1637 / 11.266018，来源见 WxsjMod.h）----
static constexpr uintptr_t kExecRva = 0x2b890d8;  // 12.10701 / vc1644（2026-09-22 运行时映射重解；il2cpp_rx_2 +0x7460d8 prologue 实锤）
// 方法入口前 16 字节（运行时明文 .text 实测），装机即验，不符=版本漂移→拒绝 hook
static const uint8_t kExecPrologue[16] = {
    0xfe, 0x0f, 0x1c, 0xf8, 0xf8, 0x5f, 0x01, 0xa9,
    0xf6, 0x57, 0x02, 0xa9, 0xf4, 0x4f, 0x03, 0xa9,
};
static constexpr int32_t kOrigDllLen = 2030080;   // 官方热更 targetfuntional.dll 字节数（2026-09-23 二次热更，真机 DIAG dump 实测）
static constexpr int32_t kPrevDllLen = 1960960;   // 官方上一版热更 dll（2026-09-23 下午官方回滚/灰度回落，真机 mismatch 标记实测）
static const char kDllNameKey[] = "targetfuntional";

// ---- 内置 patched dll（llvm-objcopy 生成，Android.mk 链接）----
// 🩸 2026-09-23 血案：官方热更会**回滚/灰度切换**（同一天内 1960960 ⇄ 2030080 来回），
// 只内置一份补丁必然在切换后失配。⇒ 两个已知官方版本各内置一份补丁，按运行时数组长度选。
extern const uint8_t _binary_targetfuntional_dll_bin_start[];       // 对应 kOrigDllLen
extern const uint8_t _binary_targetfuntional_dll_bin_end[];
extern const uint8_t _binary_targetfuntional_dll_v711_bin_start[];  // 对应 kPrevDllLen
extern const uint8_t _binary_targetfuntional_dll_v711_bin_end[];

struct DllVariant { int32_t officialLen; const uint8_t* start; const uint8_t* end; const char* tag; };
static const DllVariant* PickDllVariant(int32_t len) {
    static const DllVariant kVars[] = {
        { kOrigDllLen, _binary_targetfuntional_dll_bin_start,      _binary_targetfuntional_dll_bin_end,      "hotfix2-2030080" },
        { kPrevDllLen, _binary_targetfuntional_dll_v711_bin_start, _binary_targetfuntional_dll_v711_bin_end, "v711-1960960" },
    };
    for (const DllVariant& v : kVars) if (v.officialLen == len) return &v;
    return nullptr;
}

typedef uint8_t (*Exec_t)(void* self, void* data, void* outP, void* mi);

static std::mutex g_mtx;
static Exec_t g_origExec = nullptr;
static std::atomic<bool> g_hooked{false};
static std::atomic<int> g_hits{0};
static std::atomic<bool> g_replaceArmed{false};  // 任一功能开过才武装替换（缺省=完全原版）

static bool ReadFileLines(const std::string& path, std::vector<std::string>* out);
static std::string PkgName();
static void WriteConfigKey(const char* key, const std::string& val);

static bool OkPtr(void* p) {
    uintptr_t v = (uintptr_t)p;
    return p && v >= 0x100000000ULL && v < 0x800000000000ULL;
}

// ---- libil2cpp.so 基址（dlpi_addr = 首映射 = VA 基准）----
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

extern "C" void StarryShowToast(const char* text);  // Main.cpp 导出，崩前提示

static std::atomic<bool> g_mismatchBlocked{false};  // 官方热更失配：只处置一次（v7.13 起改为「禁止进入」而非崩溃）

// ---- 测试钩子：files/starry_force_mismatch 存在 ⇒ 强制走失配分支 ----
// 用途：不等官方真热更也能验证「禁止进入游戏」的行为（正式逻辑不受影响；删文件即恢复）
static bool ForceMismatch() {
    static std::atomic<int> cached{-1};  // -1 未查 / 0 无 / 1 有
    int c = cached.load(std::memory_order_relaxed);
    if (c < 0) {
        std::string pkg = PkgName();
        c = 0;
        if (!pkg.empty()) {
            std::string p = "/data/user/0/" + pkg + "/files/starry_force_mismatch";
            if (access(p.c_str(), F_OK) == 0) c = 1;
        }
        cached.store(c, std::memory_order_relaxed);
    }
    return c == 1;
}

// ---- DIAG 取证模式（-DWXSJ_DIAG_DUMP=1）：只落盘官方运行时 DLL，不替换/不崩 ----
#if defined(WXSJ_DIAG_DUMP) && WXSJ_DIAG_DUMP
static std::atomic<int> g_diagCount{0};
static void DiagDumpDll(const uint8_t* bytes, int32_t len, const char* head) {
    if (len < 400000) return;                 // 小框架 dll（mscorlib 等）略过
    int n = g_diagCount.load();
    if (n >= 8) return;
    g_diagCount.store(n + 1);
    std::string pkg = PkgName();
    if (pkg.empty()) { LOGE(WXSJ_LOG " DIAG pkg empty"); return; }
    char name[128];
    snprintf(name, sizeof(name), "/data/user/0/%s/files/wxsj_dump_%d_%s.bin", pkg.c_str(), len, head);
    FILE* f = fopen(name, "wb");
    if (!f) { LOGE(WXSJ_LOG " DIAG cannot write %s", name); return; }
    size_t w = fwrite(bytes, 1, (size_t)len, f);
    fclose(f);
    LOGI(WXSJ_LOG " DIAG dumped %zu/%d bytes -> %s", w, len, name);
}
#endif

// ---- hook 函数：替换 targetfuntional 字节后调原函数 ----
static uint8_t HookExec(void* self, void* data, void* outP, void* mi) {
    Exec_t orig = g_origExec;
    if (orig && OkPtr(data)) {
        int32_t len = 0;
        memcpy(&len, (const uint8_t*)data + 0x18, sizeof(len));  // il2cpp 数组 max_length@0x18
        char head[16] = {0};
        if (len >= 4) {
            const uint8_t* b = (const uint8_t*)data + 0x20;
            snprintf(head, sizeof(head), "%02x%02x%02x%02x", b[0], b[1], b[2], b[3]);
        }
        LOGI(WXSJ_LOG " Exec call len=%d head=%s armed=%d", len, head,
             g_replaceArmed.load(std::memory_order_relaxed) ? 1 : 0);
        const uint8_t* bytes = (const uint8_t*)data + 0x20;  // 数组数据@0x20
        const bool isDll = len >= 2 && bytes[0] == 'M' && bytes[1] == 'Z';
#if defined(WXSJ_DIAG_DUMP) && WXSJ_DIAG_DUMP
        // DIAG：只落盘官方运行时 dll（大数组），不替换、不崩
        if (isDll) DiagDumpDll(bytes, len, head);
#else
        const DllVariant* var = PickDllVariant(len);
        const bool forced = ForceMismatch();
        if (g_replaceArmed.load(std::memory_order_relaxed) && var && isDll && !forced) {
            // 命中某个已知官方版长度：替换为该版本对应的内置 patched dll
            // 深过滤：全数组内必须出现程序集名（实测名字在 BSJB 元数据 ~0x19 万字节处，
            // 原先 256KB 窗口永远扫不到 → 从未替换。一次性装载，1.9MB memmem 开销可忽略）
                size_t scan = (size_t)len;
                size_t klen = sizeof(kDllNameKey) - 1;
                bool found = false;
                for (size_t o = 0; o + klen <= scan; o += 1) {
                    if (bytes[o] == 't' && memcmp(bytes + o, kDllNameKey, klen) == 0) { found = true; break; }
                }
                if (found) {
                    const uint8_t* pStart = var->start;
                    size_t pLen = (size_t)(var->end - var->start);
                    if (pLen > 0 && pLen <= (size_t)len) {
                        memcpy((void*)bytes, pStart, pLen);
                        memset((void*)(bytes + pLen), 0, (size_t)len - pLen);  // 原长度不变，尾零填充
                        int h = g_hits.fetch_add(1) + 1;
                        LOGI(WXSJ_LOG " patched dll injected (hit #%d, variant=%s patched=%zu pad=%d)",
                             h, var->tag, pLen, (int)(len - (int32_t)pLen));
                        WXSJ_TRACE("dll replaced");
                    } else {
                        LOGE(WXSJ_LOG " embedded dll size %zu > array %d (%s), skip", pLen, len, var->tag);
                    }
                }
            }
        else if (g_replaceArmed.load(std::memory_order_relaxed) && isDll && (!var || forced) && !g_mismatchBlocked.load()) {
            // 官方已热更（装载 dll 长度未适配，补丁打不上）⇒ v7.13：**不崩**，改为「禁止进入游戏」。
            // 做法：不调 orig ⇒ 热更程序集根本不执行 ⇒ 游戏进不了世界、官方逻辑也不会跑起来上传脏数据；
            // 菜单与 App 仍存活，玩家可直接到菜单「游戏补丁 → 检查更新」拉新 so，重开即恢复。
            if (g_mismatchBlocked.exchange(true)) return 0;  // 已处置：后续调用继续拦
            LOGE(WXSJ_LOG " HOTFIX MISMATCH: loaded dll len=%d known={%d,%d} forced=%d -> BLOCK ENTRY",
                 len, kOrigDllLen, kPrevDllLen, forced ? 1 : 0);
            StarryShowToast("[无限升级] 游戏已热更，当前补丁不匹配，已禁止进入游戏。请到菜单「游戏补丁 → 检查更新」下载新补丁后重开");
            std::string pkg = PkgName();
            if (!pkg.empty()) {
                std::string dp = "/data/user/0/" + pkg + "/files/starry_hotfix_mismatch";
                FILE* f = fopen(dp.c_str(), "wb");
                if (f) {
                    fprintf(f, "blocked len=%d known=%d,%d forced=%d\n", len, kOrigDllLen, kPrevDllLen, forced ? 1 : 0);
                    fclose(f);
                }
            }
            return 0;  // 不调 orig：热更程序集不执行
        }
#endif  // WXSJ_DIAG_DUMP
        if (g_mismatchBlocked.load()) return 0;  // 已处置（竞态窗口）
    }
    return orig(self, data, outP, mi);
}

static bool TryHookOnce() {
    uintptr_t base = Il2CppBase();
    if (!base) return false;
    uint8_t* target = (uint8_t*)(base + kExecRva);
    // 序言三查（hook-rules §18 精神）：版本漂移/壳未解密 → 拒绝 hook，绝不盲打
    uint8_t cur[16];
    memcpy(cur, target, sizeof(cur));
    if (memcmp(cur, kExecPrologue, sizeof(cur)) != 0) return false;  // 壳未解密/漂移：静默重试
    void* orig = nullptr;
    if (DobbyHook(target, (void*)HookExec, &orig) != 0 || !orig) {
        LOGE(WXSJ_LOG " DobbyHook failed");
        return false;
    }
    g_origExec = (Exec_t)orig;
    g_hooked.store(true);
    size_t pLen = (size_t)(_binary_targetfuntional_dll_bin_end - _binary_targetfuntional_dll_bin_start);
    size_t pLen711 = (size_t)(_binary_targetfuntional_dll_v711_bin_end - _binary_targetfuntional_dll_v711_bin_start);
    LOGI(WXSJ_LOG " hook installed @base=%p +0x%zx orig=%p embedded=%zuB(%d) %zuB(%d)",
         (void*)base, kExecRva, orig, pLen, kOrigDllLen, pLen711, kPrevDllLen);
    // v7.2：屏蔽封号执法为基线功能——无条件武装字节替换（清数据/首装同样生效）。
    // 旧逻辑「配置里有功能开启才武装」在清数据后导致原版 dll 运行，封号弹窗+COS上传全链复活。
    g_replaceArmed.store(true);
    // block_report 默认开：键缺失时补 1（玩家菜单可关，关闭后保持关）
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
        // 静默防封（出站 RPC 白名单）不再走配置键：δ 层无条件插门，加载即生效。
    }
    return true;
}

static void* FastRetryThread(void*) {
    // 热更装载在 libil2cpp 解密后 ~1.5s 内就开始（15:25 会话实测），必须 100ms 级重试
    for (int i = 0; i < 600 && !g_hooked.load(); i++) {
        if (TryHookOnce()) return nullptr;
        usleep(100 * 1000);
    }
    if (!g_hooked.load()) LOGE(WXSJ_LOG " fast-retry 60s 超时放弃");
    return nullptr;
}

bool Wxsj_TryInit() {
    if (g_hooked.load()) return true;
    std::lock_guard<std::mutex> lk(g_mtx);
    if (g_hooked.load()) return true;
    pthread_t tid;
    if (pthread_create(&tid, nullptr, FastRetryThread, nullptr) == 0) {
        pthread_detach(tid);
        LOGI(WXSJ_LOG " fast-retry thread started");
    }
    return false;  // 由 fast-retry 线程完成安装；Main.cpp 轮询 IsReady
}

bool Wxsj_IsReady() { return g_hooked.load(); }

void Wxsj_ForceClear() {
    // hook 本身不改变行为（配置缺省=全关=原版），授权失效保留 hook 无副作用
    LOGI(WXSJ_LOG " ForceClear (hook kept, features config-driven)");
}

// ---- 配置文件（dll 侧 StarryConfig 消费）----
// 路径 = /data/user/0/<pkg>/files/starry_config.ini（= Application.persistentDataPath）
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
        LOGE(WXSJ_LOG " pkg name empty, config write skipped (%s)", key);
        return;
    }
    std::string dir = "/data/user/0/" + pkg + "/files";
    std::string path = dir + "/starry_config.ini";
    mkdir(dir.c_str(), 0755);  // files 目录已存在，兜底
    std::vector<std::string> lines;
    bool exists = ReadFileLines(path, &lines);
    std::string tmp = path + ".tmp";
    FILE* f = fopen(tmp.c_str(), "wb");
    if (!f) {
        LOGE(WXSJ_LOG " cannot write %s", tmp.c_str());
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
    LOGI(WXSJ_LOG " config %s=%s (exists=%d)", key, val.c_str(), exists ? 1 : 0);
}

void Wxsj_OnFeatureChanged(int featNum, bool enabled) {
    // 只写配置文件（dll 2s 内生效），禁碰 IL2CPP/UI
    const char* on = enabled ? "1" : "0";
    switch (featNum) {
    case WXSJ_FEAT_GOD:       WriteConfigKey("god", on); break;
    case WXSJ_FEAT_ONESHOT:   WriteConfigKey("oneshot", on); break;
    case WXSJ_FEAT_FREEADS:   WriteConfigKey("free_ads", on); break;
    case WXSJ_FEAT_ITEMKEEP:  WriteConfigKey("item_keep", on); break;
    case WXSJ_FEAT_SKILLFREE: WriteConfigKey("skill_free", on); break;
    // 广告奖励翻倍：dll 侧 GetInt("ads_reward_x",1)，<=1 走原逻辑 → 开=2，关=1（不能写 0）
    case WXSJ_FEAT_ADSREWARD:  WriteConfigKey("ads_reward_x", enabled ? "2" : "1"); break;
    case WXSJ_FEAT_ITEMDOUBLE: WriteConfigKey("item_double", on); break;
    case WXSJ_FEAT_MIJINGFREE: WriteConfigKey("mijing_free", on); break;
    case WXSJ_FEAT_SKINUNLOCK: WriteConfigKey("skin_unlock", on); break;
    case WXSJ_FEAT_FUNCUNLOCK: WriteConfigKey("func_unlock", on); break;
    default: break;
    }
    // 任一功能开启 → 武装字节替换（未开过 = 纯原版基线）
    if (enabled) g_replaceArmed.store(true);
}
