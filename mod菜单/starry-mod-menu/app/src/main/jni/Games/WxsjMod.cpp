/* ================================================================
 *  无限升级 — 热更 dll 字节替换（详见 WxsjMod.h）
 *  hook 点：RuntimeAssemblyLoader::ExecuteRuntimeAssembly（AOT，
 *  Assembly.Load(byte[]) 的唯一热更装载路径，反汇编实锤：
 *    x0=this(未用) x1=byte[] data x2=out param x3=MethodInfo*
 *    检查 data 非空且 [data+0x18](len)>0 后直调 Assembly::Load）
 *  hook 内仅 memcmp/memcpy（memcmp 级操作，无 il2cpp API 调用）；
 *  替换发生在游戏 bootstrap 线程（非渲染线程），单次装载触发。
 * ================================================================ */
/* v7.14 (2026-09-26)：单一版本重做。
 * 废弃 v7.13 的「双版本内置 + PickDllVariant」，只绑定现网官方 hotfix2 /
 * 2,030,080 一个版本，且用整包 FNV-1a 64 逐字节校验底包（同长度换版也拦得住）。
 * ❗失配仍「禁止进入游戏」（v7.15 恢复，见下）：这是**防封号基线**，不是可选项——
 * 失配意味着补丁一个字节都没打上（上报拦截/执法 RPC no-op/封号弹窗压制全失效），
 * 放行 = 玩家裸奔进游戏 ⇒ 真被对账封号（2026-09-26 实测反馈：外人安装后弹封号窗）。
 * 处置：不调 orig ⇒ 热更程序集不执行 ⇒ 进不了世界、官方上报逻辑也跑不起来；
 * 菜单/App 存活，玩家可去「游戏补丁 → 检查更新」拉新 so，重开即恢复。 */
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
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节）

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
// ---- v7.19 双版本：hotfix2 2030080 + hotfix3 2028544，按运行时长度/哈希自动选 ----
static const char kDllNameKey[] = "targetfuntional";
struct DllVariant {
    int32_t officialLen;
    uint64_t officialHash;
    uint64_t patchedBufHash;
    const uint8_t* dataStart;
    const uint8_t* dataEnd;
};
extern "C" const uint8_t _binary_wxsj_hf2_start[];
extern "C" const uint8_t _binary_wxsj_hf2_end[];
extern "C" const uint8_t _binary_wxsj_hf3_start[];
extern "C" const uint8_t _binary_wxsj_hf3_end[];
static constexpr DllVariant kDllVariants[] = {
    {2030080, 0x91f9acbd064fa875ULL, 0x839daa17e05c50ceULL, _binary_wxsj_hf2_start, _binary_wxsj_hf2_end},
    {2028544, 0x43df8d0d87bf08b2ULL, 0x8104d06951efb074ULL, _binary_wxsj_hf3_start, _binary_wxsj_hf3_end},
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
static std::atomic<int> g_hits{0};
// v7.2 起：钩子装上即无条件武装（TryHookOnce 末尾 store(true)，本文件 :308 附近），与「玩家开不开功能」无关。
// 玩家装包后打开游戏就生效，无需任何菜单操作 —— 不要照旧注释以为要先开个功能。
static std::atomic<bool> g_replaceArmed{false};

static bool ReadFileLines(const std::string& path, std::vector<std::string>* out);
static std::string PkgName();
static void WriteConfigKey(const char* key, const std::string& val);

// ★设备无关修正：真机（arm64 + Android 12+）堆指针带 TBI 标签（形如 0xb4000076…），
//   原先对原始指针判 <0x800000000000 会把合法对象判否 ⇒ 真机功能静默失效。
static bool OkPtr(void* p) { return starptr::LiveFast(p); }

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

// 【v7.18】官方热更失配计数（替代 v7.15/v7.16 的 g_mismatchBlocked 硬拦）。
// 失配不再阻断进入游戏，而是把装载缓冲区强制覆写成内嵌补丁版 ⇒ 版本钉死、存档安全；
// 计数只用于「首次提示一次」，绝不用于拦截。
static std::atomic<int> g_hotfixForced{0};

// ---- 测试钩子：files/starry_force_mismatch ----
// 【v7.18 起语义变化】失配分支现在是「强制换成内嵌补丁版」而非「禁止进入游戏」，
// 所以本钩子验证的是「失配路径也被正确覆写 + 只提示一次 + 存档不丢」，而非「被拦」。
// 真实失配（换长度/换哈希的官方新包）无法在无网环境复现，本钩子是最接近的替代。
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

// ---- 应急开关：files/starry_skip_dll_hash ----
// 【v7.18 起空转】旧语义是「跳过哈希门、只比长度」；新逻辑无条件覆写为嵌补丁版，
// 不再有需要跳过的门。保留函数以免调用点孤儿化，v7.19 可整体删除。
static bool SkipHashGate() {
    std::string pkg = PkgName();
    if (pkg.empty()) return false;
    return access(("/data/user/0/" + pkg + "/files/starry_skip_dll_hash").c_str(), F_OK) == 0;
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
        // 深过滤：全数组内必须出现程序集名（实测名字在 BSJB 元数据 ~0x19 万字节处；
        // 一次性装载，2MB 扫描开销可忽略）。用它把 targetfuntional 与其它热更程序集（mscorlib 等）分开。
        if (g_replaceArmed.load(std::memory_order_relaxed) && isDll && ContainsDllNameKey(bytes, (size_t)len)) {
            const uint64_t h = Fnv1a64(bytes, (size_t)len);
            const DllVariant* v = PickDllVariant(len, h);
            if (!v) {
                LOGE(WXSJ_LOG " no variant for len=%d fnv=%016llx : PASSTHROUGH official", len, (unsigned long long)h);
                if (g_hotfixForced.fetch_add(1) == 0) {
                    StarryShowToast("[无限升级] 游戏热更版本未识别，本次以官方版本运行（存档安全）。新功能请等人工更新安装包");
                }
                return orig(self, data, outP, mi);
            }
            const size_t pLen = (size_t)(v->dataEnd - v->dataStart);
            // 【v7.16】同一装载缓冲区被二次执行：内容已经是我们的补丁（长度仍是官方 2030080，
            // 但整包哈希 = kPatchedBufHash）。不认这一条 ⇒ 第二次装载（换号/重登录/程序集重执行）
            // 会被当成「官方换版」误封入口——现场表现正是「第一次登录能进、第二次禁止进入」。
            // 放行是安全的：缓冲区里本就是官方底包 + 我们的补丁，与第一次注入后的内容逐字节相同。
            bool matchedPatchedBuf = false;
            for (const auto& cv : kDllVariants) {
                if (cv.officialLen == len && cv.patchedBufHash == h) { matchedPatchedBuf = true; break; }
            }
            if (matchedPatchedBuf) {
                LOGI(WXSJ_LOG " Exec call re-enter: buffer already carries our patch (hit#%d fnv=%016llx) -> passthrough",
                     g_hits.load(), (unsigned long long)h);
                return orig(self, data, outP, mi);
            }
            // 【v7.18】注入路径统一：只要缓冲区「不是我们已经注入过的内容」，就一律覆写为内嵌补丁版。
            // 不再区分「官方底包匹配 / 官方已热更」——两者结果是同一件事：跑我们的补丁版本。
            // 这正是用户定策「不让游戏热更生效，等人工换安装包」的落地：装载什么进来都无所谓。
            // （kPatchedBufHash 已在上面放行，避免二次装载时重复覆写同一缓冲区。）
            if (pLen > 0 && pLen <= (size_t)len) {
                memcpy((void*)bytes, v->dataStart, pLen);
                memset((void*)(bytes + pLen), 0, (size_t)len - pLen);  // 原长度不变，尾零填充
                int n = g_hits.fetch_add(1) + 1;
                // 【v7.18 修复】v7.18 重写时把 ForceMismatch() 的调用点删掉了，导致
                //   ① 测试钩子 files/starry_force_mismatch 变成死代码（放不放都走正常路径）
                //   ② 真机上无法复现「官方热更到新版」的失配分支，只能靠等官方真热更
                // 现接回：钩子存在 ⇒ 强制认定失配，用于复现「非已知底包」场景。
                const bool wasMismatch = !(len == v->officialLen && h == v->officialHash)
                                         || SkipHashGate() || ForceMismatch();
                if (wasMismatch) g_hotfixForced.fetch_add(1);
                LOGI(WXSJ_LOG " patched dll injected (hit #%d, loaded=official-%d fnv=%016llx patched=%zu pad=%d%s)",
                     n, len, (unsigned long long)h, pLen, (int)(len - (int32_t)pLen),
                     wasMismatch ? " [HOTFIX MISMATCH -> FORCED PATCH: hot-update neutralized, save safe]" : "");
                WXSJ_TRACE("dll replaced");
                if (wasMismatch && g_hotfixForced.load() == 1) {
                    char tb[320];
                    snprintf(tb, sizeof(tb),
                             "[无限升级] 已忽略本次游戏热更（len=%d fnv=%016llx），继续使用当前补丁版本，"
                             "你的存档不受影响。新功能请等人工更新安装包",
                             len, (unsigned long long)h);
                    StarryShowToast(tb);
                }
                if (wasMismatch) {
                    std::string pkg0 = PkgName();
                    if (!pkg0.empty()) {
                        std::string dp = "/data/user/0/" + pkg0 + "/files/starry_hotfix_mismatch";
                        FILE* f = fopen(dp.c_str(), "wb");
                        if (f) {
                            fprintf(f, "forced len=%d fnv=%016llx expect_len=%d expect_fnv=%016llx"
                                       " embedded=%zu action=forced-patch\n",
                                    len, (unsigned long long)h, v->officialLen, (unsigned long long)v->officialHash, pLen);
                            fclose(f);
                        }
                    }
                }
            } else {
                // 极端：官方 dll 比我们的补丁还小 —— memcpy 会越界，绝不覆盖；也绝不 BLOCK（丢档）。
                // 放行官方版：玩家能玩、存档不丢；mod 功能本次失效，等人工出新包。
                g_hotfixForced.fetch_add(1);
                LOGE(WXSJ_LOG " embedded %zuB > array %d : PASSTHROUGH official"
                               " (save-loss guard; rebuild APK to restore mod features)",
                     pLen, len);
                if (g_hotfixForced.load() == 1) {
                    StarryShowToast("[无限升级] 游戏已热更且体积小于补丁，本次以官方版本运行（存档安全）。"
                                    "新功能请等人工更新安装包");
                }
            }
        }
#endif  // WXSJ_DIAG_DUMP
        // 【v7.18】不再有任何「拦死」出口：加载什么进来都会被换成嵌补丁版（或极端下放行官方）。
        // 旧 v7.15/v7.16 此处是 `if (g_mismatchBlocked) return 0;` —— 正是丢档元凶，已删除。
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
    size_t hf2Len = (size_t)(_binary_wxsj_hf2_end - _binary_wxsj_hf2_start);
    size_t hf3Len = (size_t)(_binary_wxsj_hf3_end - _binary_wxsj_hf3_start);
    LOGI(WXSJ_LOG " hook installed @base=%p +0x%zx orig=%p dual=hf2:%zuB+hf3:%zuB",
         (void*)base, kExecRva, orig, hf2Len, hf3Len);
    // v7.2：屏蔽封号执法为基线功能——无条件武装字节替换（清数据/首装同样生效）。
    // 旧逻辑「配置里有功能开启才武装」在清数据后导致原版 dll 运行，封号弹窗+COS上传全链复活。
    // ★玩家不需要任何操作：钩子装上即武装。触发链与菜单无关：
    //   StarryInitProvider.onCreate(应用进程启动) → mb_nativeInit → mod_thread
    //   → Wxsj_TryInit → FastRetryThread(每 100ms 重试, 上限 60s) → 本函数 → 此处武装。
    //   开功能按钮只写 starry_config.ini，不再参与武装判定（旧注释已删，勿再引入「要先开功能」）。
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
    // 【v7.19】删掉冗余的 `if (enabled) g_replaceArmed.store(true);`
    // 旧注释「任一功能开启 → 武装字节替换（未开过 = 纯原版基线）」是 v7.2 前的口径，早已不成立：
    // v7.2 起 TryHookOnce 末尾（本文件 :308 附近）已经无条件 store(true)，故此处是焊死在 true 上的空操作。
    // 留着只会让读代码的人以为「得先开个功能才生效」——实测玩家无需任何菜单操作。
}

void Wxsj_OnValueChanged(int featNum, int value) {
    // 滑块通道：只写配置文件（dll 2s 内生效），禁碰 IL2CPP/UI
    switch (featNum) {
    // 经验倍率：dll 侧 GetInt("exp_multi",1)，<=1 走原逻辑
    case WXSJ_FEAT_EXPMULTI: {
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
