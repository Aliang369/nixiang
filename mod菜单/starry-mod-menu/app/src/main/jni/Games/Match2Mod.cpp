/* ================================================================
 *  消消英雄2 — 在 LuaManager.Update 主线程注入 xLua 补丁
 *  不用 il2cpp DoString（会 SIGSEGV），改走 libxlua C API
 *
 *  功能（改包 AN_* 同款）：
 *    AN_1 十倍伤害 · AN_2 无敌 · AN_3 解锁自动战斗（游戏内自动战斗 + 3 倍速）
 *  过检测（「检测到战斗作弊」）+ 上报拦截：基线能力，常开（见 kAcScript / DNS 段）
 * ================================================================ */

#include "Match2Mod.h"

#include <android/log.h>
#include <atomic>
#include <cstring>
#include <dlfcn.h>
#include <mutex>
#include <netdb.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;
using Il2Cpp::IsValidHeapPtr;
using Il2Cpp::ReadPtr;
using Il2Cpp::ReadU8;
using Il2Cpp::SafeProbe;

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_wantSync{false};
static std::atomic<bool> g_wantInstall{false};
static std::atomic<bool> g_forceOff{false};
static std::atomic<bool> g_inHook{false};

static void* g_luaManagerClass = nullptr;
static size_t g_offLuaEnv = 0;
static size_t g_offInitialized = 0;

typedef void (*Update_t)(void* thiz);
static Update_t g_origUpdate = nullptr;

static bool g_match2_inv = false;
static bool g_match2_one = false;
static bool g_match2_unlock = false;
static bool g_patched = false;

// ==================== 上报拦截（基线）：DNS 黑名单 ====================
// 依据（1.2.28 静态枚举，证据 /tmp/xxh2_endpoints_raw.txt）：
//   ① cn.fly/FlySDK 的 cfgc.zztfly.com/pa/gcf 是「环境指纹上报」——检到 root/调试/
//      模拟器/frida 就打这个接口（cn/fly/commons/y.java:179），是「外部工具/小黑屋」
//      判定的输入源；devc.zztfly.com/dsign、/dinfo 是设备指纹+签名（反作弊核心）。
//      另外 tgc.zztfly.com/tcp/config/init 会下发 Mob 的 TCP 长连接 host:port，
//      ⇒ 黑名单必须含整个 .zztfly.com，否则长连接拿到纯 IP 就绕过 DNS。
//   ② Bugly（崩溃/统计）、腾讯灯塔 Atta、TapDB 埋点、TapSDK openlog 日志上报。
// 做法：inline hook libc 解析入口——native 侧走 getaddrinfo，Java 侧
//      （InetAddress/HttpURLConnection/OkHttp）走 android_getaddrinfofornet
//      （实证：libjavacore.so 的未定义符号只有 UND android_getaddrinfofornet），
//      命中后缀直接返回 EAI_NONAME。
// 边界（不做什么，以及为什么）：不拦 IP、不碰 TLS、不改 connect()；游戏本体与登录域
//      一律放行——*.alina-game.cn（本体 WebSocket + CDN）、*.taptap.cn / *.taptap.com /
//      *.xdrnd.cn / *.xdrnd.com（登录、实名、防沉迷门槛）、*.alipay.com 与 open.e.189.cn
//      （支付与登录协议页）。拦这些会直接打断登录与对局。
// 常开：过检测与上报拦截是基线，不跟菜单功能开关联动（同 WxsjMod 的硬约束）。
#ifndef RTLD_DEFAULT
#define RTLD_DEFAULT ((void*)0)
#endif

static const char* kDnsBlockSuffixes[] = {
    ".zztfly.com",          // Mob/FlySDK：/pa/gcf 环境上报、/dsign /dinfo 设备指纹、/tcp/config/init
    ".bugly.qq.com",        // Bugly 崩溃/统计
    ".bugly.qcloud.com",
    ".cros.wr.pvp.net",
    ".trace.qq.com",        // 腾讯灯塔 Atta
    ".tapdb.net",           // TapDB 埋点
    "openlog.tapapis.cn",   // TapSDK 日志上报（libtapsdkcore.so 明文）
    "openlog.tapapis.com",
    "openlog.xdrnd.cn",
    ".cmpassport.com",      // 运营商一键登录遥测
    "crbt.i139.cn",
    "opencloud.wostore.cn",
    "api-e189.21cn.com",
};

static std::mutex g_dnsLogMtx;
static const char* g_dnsSeen[256];
static int g_dnsSeenCount = 0;

// 首见才打一条日志，避免 SDK 轮询把 logcat 刷满
static void LogHostOnce(const char* host, bool blocked) {
    if (!host || !*host) return;
    std::lock_guard<std::mutex> lk(g_dnsLogMtx);
    for (int i = 0; i < g_dnsSeenCount; ++i) {
        if (g_dnsSeen[i] && strcmp(g_dnsSeen[i], host) == 0) return;
    }
    if (g_dnsSeenCount < 256) {
        g_dnsSeen[g_dnsSeenCount++] = strdup(host);
    }
    LOGI("[Match2] DNS %s %s", blocked ? "BLOCK" : "allow", host);
}

static bool HostBlocked(const char* host) {
    if (!host || !*host) return false;
    size_t n = strlen(host);
    if (n == 0 || n >= 256) return false;
    char buf[256];
    for (size_t i = 0; i < n; ++i) {
        char c = host[i];
        buf[i] = (c >= 'A' && c <= 'Z') ? (char)(c - 'A' + 'a') : c;
    }
    buf[n] = 0;
    // 纯数字/十六进制点分形态（IP 字面量）不拦：Java 侧有时把已解析地址再喂进来
    bool numish = true;
    for (size_t i = 0; i < n; ++i) {
        char c = buf[i];
        bool ok = (c >= '0' && c <= '9') || c == '.' || c == ':' || (c >= 'a' && c <= 'f');
        if (!ok) { numish = false; break; }
    }
    if (numish) return false;
    for (size_t i = 0; i < sizeof(kDnsBlockSuffixes) / sizeof(kDnsBlockSuffixes[0]); ++i) {
        const char* suf = kDnsBlockSuffixes[i];
        size_t sn = strlen(suf);
        if (sn <= n && strcmp(buf + (n - sn), suf) == 0) return true;
    }
    return false;
}

// 统一按 8 个指针宽度转发：arm64 下多余寄存器实参被被调方忽略，少传才会崩
typedef int (*GaiFn)(uintptr_t, uintptr_t, uintptr_t, uintptr_t,
                     uintptr_t, uintptr_t, uintptr_t, uintptr_t);
typedef uintptr_t (*HbnFn)(uintptr_t, uintptr_t, uintptr_t, uintptr_t,
                           uintptr_t, uintptr_t, uintptr_t, uintptr_t);

static GaiFn g_orig_gai = nullptr;
static GaiFn g_orig_gai_net = nullptr;
static HbnFn g_orig_ghbn = nullptr;
static HbnFn g_orig_ghbn2 = nullptr;

static int Hook_GetAddrInfo(uintptr_t a0, uintptr_t a1, uintptr_t a2, uintptr_t a3,
                            uintptr_t a4, uintptr_t a5, uintptr_t a6, uintptr_t a7) {
    const char* node = (const char*)a0;
    if (HostBlocked(node)) {
        LogHostOnce(node, true);
        if (a3) *(void**)a3 = nullptr;   // getaddrinfo 第 4 参是 res 出参
        return EAI_NONAME;
    }
    LogHostOnce(node, false);
    if (!g_orig_gai) return EAI_FAIL;
    return g_orig_gai(a0, a1, a2, a3, a4, a5, a6, a7);
}

// android_getaddrinfofornet(node, service, hints, netid, mark, res)：
// 出参位置各版本有差异，**一律不写出参**，只回错误码（调用方先判返回码，异常走 UnknownHost）
static int Hook_GetAddrInfoForNet(uintptr_t a0, uintptr_t a1, uintptr_t a2, uintptr_t a3,
                                  uintptr_t a4, uintptr_t a5, uintptr_t a6, uintptr_t a7) {
    const char* node = (const char*)a0;
    if (HostBlocked(node)) {
        LogHostOnce(node, true);
        return EAI_NONAME;
    }
    LogHostOnce(node, false);
    if (!g_orig_gai_net) return EAI_FAIL;
    return g_orig_gai_net(a0, a1, a2, a3, a4, a5, a6, a7);
}

static uintptr_t Hook_GetHostByName(uintptr_t a0, uintptr_t a1, uintptr_t a2, uintptr_t a3,
                                    uintptr_t a4, uintptr_t a5, uintptr_t a6, uintptr_t a7) {
    const char* name = (const char*)a0;
    if (HostBlocked(name)) {
        LogHostOnce(name, true);
        return 0;
    }
    LogHostOnce(name, false);
    if (!g_orig_ghbn) return 0;
    return g_orig_ghbn(a0, a1, a2, a3, a4, a5, a6, a7);
}

static uintptr_t Hook_GetHostByName2(uintptr_t a0, uintptr_t a1, uintptr_t a2, uintptr_t a3,
                                     uintptr_t a4, uintptr_t a5, uintptr_t a6, uintptr_t a7) {
    const char* name = (const char*)a0;
    if (HostBlocked(name)) {
        LogHostOnce(name, true);
        return 0;
    }
    LogHostOnce(name, false);
    if (!g_orig_ghbn2) return 0;
    return g_orig_ghbn2(a0, a1, a2, a3, a4, a5, a6, a7);
}

static void InstallDnsBlockHooks() {
    static std::atomic<bool> s_done{false};
    if (s_done.exchange(true)) return;
    int n = 0;
    void* p = dlsym(RTLD_DEFAULT, "getaddrinfo");
    if (p && DobbyHook(p, (void*)Hook_GetAddrInfo, (void**)&g_orig_gai) == 0) ++n;
    p = dlsym(RTLD_DEFAULT, "android_getaddrinfofornet");
    if (p && DobbyHook(p, (void*)Hook_GetAddrInfoForNet, (void**)&g_orig_gai_net) == 0) ++n;
    p = dlsym(RTLD_DEFAULT, "gethostbyname");
    if (p && DobbyHook(p, (void*)Hook_GetHostByName, (void**)&g_orig_ghbn) == 0) ++n;
    p = dlsym(RTLD_DEFAULT, "gethostbyname2");
    if (p && DobbyHook(p, (void*)Hook_GetHostByName2, (void**)&g_orig_ghbn2) == 0) ++n;
    LOGI("[Match2] 上报拦截武装完毕：DNS 黑名单 %d 个入口（后缀数=%d）",
         n, (int)(sizeof(kDnsBlockSuffixes) / sizeof(kDnsBlockSuffixes[0])));
}

// so 在 StarryInitProvider 里被 System.loadLibrary 加载（早于 cn.fly.FlyApplication.onCreate
// → FlySDK.init），构造器里装 hook 正好赶在 Mob 初始化之前
__attribute__((constructor)) static void Match2_ReportBlockCtor() {
    InstallDnsBlockHooks();
}

// ---- libxlua ----
struct lua_State;
typedef int (*luaL_loadstring_t)(lua_State*, const char*);
typedef int (*lua_pcall_t)(lua_State*, int, int, int);
typedef void (*lua_pushboolean_t)(lua_State*, int);
typedef void (*lua_setglobal_t)(lua_State*, const char*);
typedef void (*lua_getglobal_t)(lua_State*, const char*);
typedef int (*lua_type_t)(lua_State*, int);
typedef void (*lua_settop_t)(lua_State*, int);
typedef const char* (*lua_tolstring_t)(lua_State*, int, size_t*);

static luaL_loadstring_t p_luaL_loadstring = nullptr;
static lua_pcall_t p_lua_pcall = nullptr;
static lua_pushboolean_t p_lua_pushboolean = nullptr;
static lua_setglobal_t p_lua_setglobal = nullptr;
static lua_getglobal_t p_lua_getglobal = nullptr;
static lua_type_t p_lua_type = nullptr;
static lua_settop_t p_lua_settop = nullptr;
static lua_tolstring_t p_lua_tolstring = nullptr;

enum { LUA_TNIL = 0, LUA_TSTRING = 4, LUA_TTABLE = 5 };

#ifndef MATCH2_LUA_DUMP_TRACE
#define MATCH2_LUA_DUMP_TRACE 0
#endif

#if MATCH2_LUA_DUMP_TRACE
#include <cstdio>
#include <sys/stat.h>
#include <thread>

bool Match2_TryInitTraceEarly();

#define MATCH2_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Match2] " __VA_ARGS__)

static constexpr const char* kLuaDumpDir =
    "/sdcard/Android/data/com.blina.match2.tt/files/starry_lua_dump";

static bool ReadIl2CppString(void* strObj, char* out, size_t outCap) {
    if (!out || outCap < 2 || !IsValidHeapPtr(strObj)) return false;
    int32_t len = 0;
    if (!SafeProbe([&]() -> bool {
            len = Il2Cpp::ReadI32(strObj, 0x10);
            return len > 0 && len < 4096;
        })) {
        return false;
    }
    size_t n = 0;
    if (!SafeProbe([&]() -> bool {
            const uint16_t* chars = (const uint16_t*)((uintptr_t)strObj + 0x14);
            for (int32_t i = 0; i < len && n + 1 < outCap; i++) {
                uint16_t c = chars[i];
                out[n++] = (c < 0x80) ? (char)c : '_';
            }
            out[n] = '\0';
            return n > 0;
        })) {
        return false;
    }
    return true;
}

static bool ReadByteArray(void* arr, const uint8_t** data, size_t* len) {
    if (!data || !len || !IsValidHeapPtr(arr)) return false;
    return SafeProbe([&]() -> bool {
        int32_t maxLen = Il2Cpp::ReadI32(arr, 0x18);
        if (maxLen <= 0 || maxLen > (8 << 20)) return false;
        *data = (const uint8_t*)((uintptr_t)arr + 0x20);
        *len = (size_t)maxLen;
        return true;
    });
}

// 诊断期开关：把**所有**已解密 Lua 模块落盘（只影响 -DMATCH2_LUA_DUMP_TRACE=1 的诊断包），
// 用于定位菜单功能对应的 Lua 门控（例如「通关主线 4-10 后解锁自动战斗」）。
#define MATCH2_DUMP_ALL 1

static bool ShouldDumpModule(const char* path) {
#if MATCH2_DUMP_ALL
    (void)path;
    return true;
#endif
    if (!path || !path[0]) return false;
    if (path[0] == 'r' && strncmp(path, "return", 6) == 0) return false;
    if (strncmp(path, "@Modules.", 9) == 0) {
        if (strstr(path, "EnvTask.Tasks.") || strstr(path, "EnvTask.")) return true;
        if (strstr(path, "Input.") || strstr(path, "Replay.")) return true;
        return false;
    }
    if (strncmp(path, "Modules.", 8) == 0) {
        if (strstr(path, "EnvTask.") || strstr(path, "Input.") || strstr(path, "Replay.")) return true;
        return false;
    }
    static const char* kAppNeedles[] = {
        "AntiCheat", "ServerEvent", "DeviceInfo", "LoginManager",
        "Network.", "Replay", "Cheat", "ExternalPlayer", nullptr};
    if (strncmp(path, "@Application.", 13) == 0) {
        for (int i = 0; kAppNeedles[i]; i++) {
            if (strstr(path, kAppNeedles[i])) return true;
        }
    }
    static const char* kNeedles[] = {
        "ReplayInput", "ReplayRecord", "CodeUpdater", "AppInput",
        "ServerEvent", "anticheat", "EnvTask", "BattleRecovery", "CFightSystem", nullptr};
    for (int i = 0; kNeedles[i]; i++) {
        if (strstr(path, kNeedles[i])) return true;
    }
    return false;
}

static uint32_t PathHash32(const char* s) {
    uint32_t h = 2166136261u;
    if (!s) return h;
    for (; *s; s++) {
        h ^= (uint8_t)*s;
        h *= 16777619u;
    }
    return h;
}

static bool WriteDumpFile(const char* path, const uint8_t* data, size_t len) {
    if (!path || !data || !len || len > (4 << 20)) return false;
    mkdir(kLuaDumpDir, 0777);
    char filePath[512];
    snprintf(filePath, sizeof(filePath), "%s/m_%08x_%zu.lua.bin",
             kLuaDumpDir, PathHash32(path), len);
    if (access(filePath, F_OK) == 0) return true;

    FILE* fp = fopen(filePath, "wb");
    if (!fp) {
        MATCH2_TRACE("dump open fail %s", filePath);
        return false;
    }
    size_t w = fwrite(data, 1, len, fp);
    fclose(fp);
    if (w != len) return false;

    char metaPath[512];
    snprintf(metaPath, sizeof(metaPath), "%s/m_%08x.path", kLuaDumpDir, PathHash32(path));
    FILE* meta = fopen(metaPath, "w");
    if (meta) {
        fprintf(meta, "%s\n%zu\n", path, len);
        fclose(meta);
    }
    MATCH2_TRACE("dumped %s (%zu bytes) -> %s", path, len, filePath);
    return true;
}

static void HandleVerifiedModule(const char* path, const uint8_t* bytes, size_t len) {
    if (!bytes || !len || !path || !path[0]) return;
    if (len < 16 || len > (2 << 20)) return;
    if (!ShouldDumpModule(path)) return;
    WriteDumpFile(path, bytes, len);
}

typedef int (*luaL_loadbuffer_t)(lua_State*, const char*, size_t, const char*);
typedef int (*luaL_loadbufferx_t)(lua_State*, const char*, size_t, const char*, const char*);
static luaL_loadbuffer_t g_origLoadBuffer = nullptr;
static luaL_loadbufferx_t g_origLoadBufferX = nullptr;

static void TraceLuaLoad(const char* tag, const char* name, size_t sz) {
    static std::atomic<uint32_t> s_log{0};
    uint32_t n = ++s_log;
    if (n <= 400 || (name && ShouldDumpModule(name))) {   // MATCH2_DUMP_ALL 下 cap 住日志量
        MATCH2_TRACE("%s #%u name=%s sz=%zu", tag, n, name ? name : "?", sz);
    }
}

static int Hooked_luaL_loadbuffer(lua_State* L, const char* buff, size_t sz, const char* name) {
    TraceLuaLoad("xlua load", name, sz);
    if (name && buff && sz >= 16 && sz <= (300 << 10) && ShouldDumpModule(name)) {
        HandleVerifiedModule(name, (const uint8_t*)buff, sz);
    }
    return g_origLoadBuffer ? g_origLoadBuffer(L, buff, sz, name) : -1;
}

static int Hooked_luaL_loadbufferx(lua_State* L, const char* buff, size_t sz, const char* name,
                                   const char* mode) {
    TraceLuaLoad("xlua loadx", name, sz);
    if (name && buff && sz >= 16 && sz <= (300 << 10) && ShouldDumpModule(name)) {
        HandleVerifiedModule(name, (const uint8_t*)buff, sz);
    }
    return g_origLoadBufferX ? g_origLoadBufferX(L, buff, sz, name, mode) : -1;
}

static bool InstallXluaLoadHook() {
    static std::atomic<bool> installed{false};
    if (installed.load()) return g_origLoadBuffer != nullptr || g_origLoadBufferX != nullptr;

    void* h = dlopen("libxlua.so", RTLD_NOW);
    if (!h) h = dlopen("libxlua.so", RTLD_NOLOAD | RTLD_NOW);
    if (!h) return false;

    bool ok = false;
    void* sym = dlsym(h, "luaL_loadbuffer");
    if (sym && !g_origLoadBuffer) {
        if (DobbyHook(sym, (void*)Hooked_luaL_loadbuffer, (void**)&g_origLoadBuffer) == 0) {
            LOGI("[Match2] hooked luaL_loadbuffer @ %p", sym);
            ok = true;
        }
    }
    void* symx = dlsym(h, "luaL_loadbufferx");
    if (symx && !g_origLoadBufferX) {
        if (DobbyHook(symx, (void*)Hooked_luaL_loadbufferx, (void**)&g_origLoadBufferX) == 0) {
            LOGI("[Match2] hooked luaL_loadbufferx @ %p", symx);
            ok = true;
        }
    }
    if (ok) installed.store(true);
    return ok;
}

typedef void* (*LoadAndVerify_t)(void* thiz, void** filepathRef, void* method);
static LoadAndVerify_t g_origLoadAndVerify = nullptr;

static void* Hooked_LoadAndVerify(void* thiz, void** filepathRef, void* method) {
    if (!g_origLoadAndVerify) return nullptr;
    void* ret = g_origLoadAndVerify(thiz, filepathRef, method);

    static std::atomic<uint32_t> s_calls{0};
    uint32_t n = ++s_calls;

    char path[512];
    path[0] = '\0';
    if (ret && thiz && filepathRef) {
        SafeProbe([&]() -> bool {
            void* strObj = *filepathRef;
            if (!strObj) return false;
            return ReadIl2CppString(strObj, path, sizeof(path));
        });
    }

    if (n <= 400 || (n % 500) == 1) {
        MATCH2_TRACE("load_and_verify #%u ret=%p path=%s", n, ret, path[0] ? path : "?");
    }
    if (!ret || !thiz || !filepathRef || !path[0]) return ret;

    const uint8_t* bytes = nullptr;
    size_t len = 0;
    if (!ReadByteArray(ret, &bytes, &len)) return ret;
    HandleVerifiedModule(path, bytes, len);
    return ret;
}

static bool InstallSignatureLoaderHook() {
    static std::atomic<bool> installed{false};
    if (installed.load()) return g_origLoadAndVerify != nullptr;

    auto& api = GetApi();
    if (!api.ready) return false;

    void* klass = api.FindClassByShortName("SignatureLoader");
    if (!klass) {
        LOGW("[Match2] SignatureLoader not found (lua dump)");
        return false;
    }
    void* method = api.GetMethod(klass, "load_and_verify", 1);
    if (!method) {
        LOGW("[Match2] load_and_verify not found");
        return false;
    }
    void* ptr = *(void**)method;
    if (!ptr) {
        LOGE("[Match2] load_and_verify code ptr missing");
        return false;
    }
    if (g_origLoadAndVerify) {
        installed.store(true);
        return true;
    }
    if (DobbyHook(ptr, (void*)Hooked_LoadAndVerify, (void**)&g_origLoadAndVerify) != 0) {
        LOGE("[Match2] DobbyHook load_and_verify failed @ %p", ptr);
        return false;
    }
    installed.store(true);
    LOGI("[Match2] hooked SignatureLoader.load_and_verify @ %p", ptr);
    MATCH2_TRACE("lua dump dir: %s", kLuaDumpDir);
    return true;
}

static void Match2_StartXluaHookThread() {
    static std::atomic<bool> started{false};
    if (started.exchange(true)) return;
    std::thread([]() {
        for (int i = 0; i < 6000; i++) {
            if (InstallXluaLoadHook()) {
                MATCH2_TRACE("early xlua hook ready (iter=%d)", i);
                return;
            }
            usleep(10000);
        }
        LOGW("[Match2] early xlua hook timeout");
    }).detach();
}

__attribute__((constructor)) static void Match2_XluaHookCtor() {
    Match2_StartXluaHookThread();
}
#endif // MATCH2_LUA_DUMP_TRACE

// 幂等：CFightSystem / UserInfoManager 重载后标记丢失时再挂一次；已挂则 rawget 直接 return
// 完成标记由 Lua 写回 __starry_fight / __starry_unlock（字符串 "1"），C++ 侧据此判定是否装齐
static const char* kInstallScript = R"LUA(
AN_1 = AN_1 or false
AN_2 = AN_2 or false
AN_3 = AN_3 or false

-- [[1]] AN_3 解锁自动战斗 + 3 倍速（唯一门控 UserInfoManager.HasUnlockSpeed3）
--   GameViewUpperUI:OnAutoPlayButtonClicked 未解锁时弹 auto_play_open_tip
--   （"通关主线4-10后解锁自动战斗功能和3倍速功能"）并 return；
--   GameViewUpperUI:OnSpeedUpButtonClicked 首句同样查它 → 自动战斗与 3 倍速同一门控。
--   原实现：MainLevel.Create(GameConfig.GetAutoPlayOpenChapter(), GetAutoPlayOpenSection())
--           <= UserInfoManager._accountMaxMainLevel
--   覆盖成每次调用读 AN_3 的包装：关开关即回退原判定，不用重载模块。
if type(UserInfoManager) == "table" then
	if not rawget(UserInfoManager, "__starry_unlock") then
		local _HasUnlockSpeed3 = UserInfoManager.HasUnlockSpeed3
		if type(_HasUnlockSpeed3) == "function" then
			UserInfoManager.HasUnlockSpeed3 = function(...)
				if AN_3 then return true end
				return _HasUnlockSpeed3(...)
			end
			UserInfoManager.__starry_unlock = true
		end
	end
	if rawget(UserInfoManager, "__starry_unlock") then __starry_unlock = "1" end
end

-- [[2]] 战斗数值补丁（AN_1 十倍伤害 / AN_2 无敌）
if type(CFightSystem) ~= "table" then return end
if rawget(CFightSystem, "__starry_patched") then __starry_fight = "1" return end

local _DoAfterTaxDamage = CFightSystem.DoAfterTaxDamage
if type(_DoAfterTaxDamage) ~= "function" then return end
function CFightSystem:DoAfterTaxDamage(args)
	local skill = args.Skill
	local points = args.Points
	local isSide1Caster = skill:GetCaster():GetPlayer():GetSide() == CSide.Side1
	if AN_1 and isSide1Caster then
		points = points * 10
		args.Points = points
	end
	if AN_2 and not isSide1Caster then
		args.Points = 0
	end
	return _DoAfterTaxDamage(self, args)
end

CFightSystem.__starry_patched = true
__starry_fight = "1"
)LUA";

// ============================================================
// 反作弊压制 + 上报拦截（基线能力，常开，不跟菜单开关联动）
//   依据 1.2.28 静态还原 + 真机 recon 取证
//   （10_docs/消消英雄2/过检测与上报拦截_1.2.28.md）：
//   ① 过检测
//      「检测到战斗作弊」＝ AntiCheatManager.CheckPlay（仅在
//      ShouldCheckPlay()=观察期内 + 上局 IsWin 时触发）→
//      EnvTaskManager.RunTask("EnvTaskReplay", …) 本地重放校验失败 →
//      UIManager.OpenWindow("ForcedWindow", GameText.GetText("cheat_play_tip"))。
//      观察期由服端事件 anticheat.warning → OnCheatWarning → 计数置 30、
//      CheatWarningWindow（cheat_warning_tip）开启。
//      ⇒ 压掉入口（ShouldCheckPlay=false / CheckPlay 空实现）+ 弹窗兜底。
//   ② 上报拦截
//      BI 单一收口 BINetworkManager.CustomSend（Send 只是转发、CachedSend
//      只写本地缓存），上游 BIRequest 8 个出口 + C# 侧入口
//      LuaManager.LuaBISend；渠道 TapDB（ChannelTapReport._ReportEvent）；
//      Bugly 场景埋点（GameConfig.UpdateBuglyReportSceneData 等）。
//      BI 走游戏自家域名（NetworkUtility.BIHttpUrl + /bi/<ver>/<cat>），
//      DNS 层拦会连坐登录/进度 ⇒ 只能在 Lua 漏斗层拦（见上方 DNS 段）。
//      【重要】服端判定伤害异常的输入正是 BI：SideQuestApi._BIEndQuest 用
//      AntiCheatManager.Judge（按战力估每回合伤害）与实际 DamagePerRound
//      比对后置 Cheated 字段（倍数阈值），紧接着 BIRequest.EndQuest 发出；
//      该字段全仓只有 SideQuestApi 引用 ⇒ BI 一死，伤害异常信号不出设备。
//   刻意不动（避免影响正常游戏）：
//     · ApiRequest / ApiClient（登录、进度、主关卡结算、回放字段）
//     · ReplayRecord / ReplayInput（对局、断线恢复、PvP 回放播发）
//     · LogModel / NetworkLogManager（本地日志，不上报）
//     · UserStatisticsManager（本地统计，喂 UI 与 BI；BI 已死，留原样）
//     · AntiCheatManager.SetCheatCheckCount（保持原语义：观察期计数照常
//       递减并回传，避免「有观察期却不递减」本身成为特征）
// ============================================================
static std::atomic<bool> g_acDone{false};

static const char* kAcScript = R"LUA(
local ST = rawget(_G, "__starry_ac_state")
if type(ST) ~= "table" then
	ST = { acSkip = 0, acWin = 0, biBlock = 0, tapBlock = 0, buglyBlock = 0 }
	_G.__starry_ac_state = ST
end
local function bump(k) ST[k] = (ST[k] or 0) + 1 end

local okAC, okUI, okBI, okBINet, okTap, okBugly, okBIAct =
	false, false, false, false, false, false, false

-- [[1]] 过检测：本地重放校验 + 服端警告弹窗全部压掉
if type(AntiCheatManager) == "table" and not rawget(AntiCheatManager, "__starry_ac") then
	if type(AntiCheatManager.ShouldCheckPlay) == "function" then
		AntiCheatManager.ShouldCheckPlay = function() return false end
	end
	AntiCheatManager.CheckPlay = function(self)
		bump("acSkip")
		return
	end
	AntiCheatManager.TipCheatWarning = function(self)
		bump("acWin")
		pcall(function()
			self.CheatWarningTimeLeft = 0
			if type(LocalStorage) == "table" and type(LocalStorage.Save) == "function" then
				LocalStorage.Save(self.CheatWarningTimeLeftKey or "CheatWarningTimeLeftKey", 0)
			end
		end)
		return
	end
	AntiCheatManager.OnCheatWarning = function(self)
		bump("acWin")
		pcall(function()
			if type(LocalStorage) == "table" and type(LocalStorage.Save) == "function" then
				LocalStorage.Save(self.CheatWarningTimeLeftKey or "CheatWarningTimeLeftKey", 0)
			end
		end)
		return
	end
	AntiCheatManager.__starry_ac = true
	okAC = true
end

-- [[2]] 弹窗兜底：别的链路开窗也拦掉（只认作弊相关，不误伤普通窗）
if type(UIManager) == "table" and not rawget(UIManager, "__starry_ac") then
	local _OpenWindow = UIManager.OpenWindow
	if type(_OpenWindow) == "function" then
		local function isCheatWindow(name, text)
			if name == "CheatWarningWindow" or name == "TestQuestCheatWindow" then return true end
			-- ForcedWindow 实为多选窗 windows/multi_choice_window，只在文案命中作弊时拦
			if name == "ForcedWindow" and type(text) == "string" then
				if string.find(string.lower(text), "cheat", 1, true) then return true end
				if string.find(text, "作弊", 1, true) then return true end
			end
			return false
		end
		UIManager.OpenWindow = function(a, b, c, d, e, f, g)
			if type(a) == "string" then
				-- 点号调用：UIManager.OpenWindow(name, text, …)（AntiCheatManager 用的就是这种）
				if isCheatWindow(a, b) then bump("acWin") return end
				return _OpenWindow(a, b, c, d, e, f, g)
			end
			-- 冒号调用：UIManager:OpenWindow(name, text, …)
			if isCheatWindow(b, c) then bump("acWin") return end
			return _OpenWindow(a, b, c, d, e, f, g)
		end
		UIManager.__starry_ac = true
		okUI = true
	end
end

-- [[3]] BI 出口（上游 8 个函数全换 no-op；EndQuest 就是 BIPlay 结算上报）
if type(BIRequest) == "table" and not rawget(BIRequest, "__starry_ac") then
	local fns = { "GeneralSend", "CommonAction", "ReportDeviceInfo", "EndQuest",
	              "Tutorial", "SkipTimeline", "NetLibRecords", "Question" }
	local n = 0
	for i = 1, #fns do
		local k = fns[i]
		if type(BIRequest[k]) == "function" then
			BIRequest[k] = function() bump("biBlock") return end
			n = n + 1
		end
	end
	BIRequest.__starry_ac = true
	okBI = n > 0
end

-- [[4]] BI 收口（CustomSend 是唯一真正发送器）
if type(BINetworkManager) == "table" and not rawget(BINetworkManager, "__starry_ac") then
	local fns = { "CustomSend", "Send", "CachedSend", "StartSendingCachedData" }
	local n = 0
	for i = 1, #fns do
		local k = fns[i]
		if type(BINetworkManager[k]) == "function" then
			BINetworkManager[k] = function() bump("biBlock") return end
			n = n + 1
		end
	end
	BINetworkManager.__starry_ac = true
	okBINet = n > 0
end

-- [[5]] C# 侧 BI 入口重绑（BIRequest 加载时会把 LuaBISend 设成自家
--   CommonAction；上面换过表字段后必须重指一次，热重载后每次跑都重指）
pcall(function()
	local lm = CS and CS.LuaManager and CS.LuaManager.Instance
	if lm ~= nil and type(BIRequest) == "table" and type(BIRequest.CommonAction) == "function" then
		lm.LuaBISend = BIRequest.CommonAction
	end
end)

-- [[6]] 渠道上报（TapDB）：_ReportEvent 是真正发送器，TrackEvent 只入队
if type(ChannelTapReport) == "table" and not rawget(ChannelTapReport, "__starry_ac") then
	local fns = { "_ReportEvent", "TrackEvent" }
	local n = 0
	for i = 1, #fns do
		local k = fns[i]
		if type(ChannelTapReport[k]) == "function" then
			ChannelTapReport[k] = function() bump("tapBlock") return end
			n = n + 1
		end
	end
	ChannelTapReport.__starry_ac = true
	okTap = n > 0
end

-- [[7]] Bugly 场景埋点（崩溃上报的上下文，非玩法必需）
if type(GameConfig) == "table" and not rawget(GameConfig, "__starry_ac") then
	local fns = { "UpdateBuglyReportSceneData", "_buglyReportSceneDataUpdate",
	              "StartBuglyReportSceneDataUpdater" }
	local n = 0
	for i = 1, #fns do
		local k = fns[i]
		if type(GameConfig[k]) == "function" then
			GameConfig[k] = function() bump("buglyBlock") return end
			n = n + 1
		end
	end
	GameConfig.__starry_ac = true
	okBugly = n > 0
end

-- [[8]] BI 行为队列：BI 已死，入队只会白涨内存
if type(BIActionManager) == "table" and not rawget(BIActionManager, "__starry_ac") then
	if type(BIActionManager.OnEvent) == "function" then
		BIActionManager.OnEvent = function() return end
	end
	BIActionManager.__starry_ac = true
	okBIAct = true
end

-- 完成标记：四个关键面都包上才算齐（渠道/Bugly/BIAction 表可能还没加载，不作门槛）
if okAC and okUI and okBI and okBINet then __starry_ac = "1" end
if not rawget(_G, "__starry_ac_logged") then
	_G.__starry_ac_logged = true
	print(string.format(
		"[StarryACG] 过检测=%s 弹窗兜底=%s BI出口=%s BI收口=%s 渠道=%s Bugly=%s BIAction=%s",
		tostring(okAC), tostring(okUI), tostring(okBI), tostring(okBINet),
		tostring(okTap), tostring(okBugly), tostring(okBIAct)))
end
)LUA";

static bool LoadXLuaApi() {
    if (p_lua_pcall) return true;
    void* h = dlopen("libxlua.so", RTLD_NOW);
    if (!h) h = dlopen("libxlua.so", RTLD_NOLOAD | RTLD_NOW);
    if (!h) {
        LOGW("[Match2] dlopen libxlua.so failed");
        return false;
    }
    p_luaL_loadstring = (luaL_loadstring_t)dlsym(h, "luaL_loadstring");
    p_lua_pcall = (lua_pcall_t)dlsym(h, "lua_pcall");
    p_lua_pushboolean = (lua_pushboolean_t)dlsym(h, "lua_pushboolean");
    p_lua_setglobal = (lua_setglobal_t)dlsym(h, "lua_setglobal");
    p_lua_getglobal = (lua_getglobal_t)dlsym(h, "lua_getglobal");
    p_lua_type = (lua_type_t)dlsym(h, "lua_type");
    p_lua_settop = (lua_settop_t)dlsym(h, "lua_settop");
    p_lua_tolstring = (lua_tolstring_t)dlsym(h, "lua_tolstring");
    if (!p_luaL_loadstring || !p_lua_pcall || !p_lua_pushboolean || !p_lua_setglobal ||
        !p_lua_getglobal || !p_lua_type || !p_lua_settop) {
        LOGE("[Match2] libxlua symbols missing");
        return false;
    }
    return true;
}

static void* MethodPointer(void* methodInfo) {
    if (!methodInfo) return nullptr;
    return *(void**)methodInfo;
}

static lua_State* GetLuaState(void* luaManager) {
    void* env = nullptr;
    if (!SafeProbe([&]() -> bool {
            if (g_offInitialized && !ReadU8(luaManager, g_offInitialized)) return false;
            env = ReadPtr(luaManager, g_offLuaEnv);
            return IsValidHeapPtr(env);
        })) {
        return nullptr;
    }
    // XLua.LuaEnv.rawL @ 0x10
    void* rawL = nullptr;
    if (!SafeProbe([&]() -> bool {
            rawL = ReadPtr(env, 0x10);
            return rawL != nullptr;
        })) {
        return nullptr;
    }
    return (lua_State*)rawL;
}

static void SetGlobalBool(lua_State* L, const char* name, bool v) {
    p_lua_pushboolean(L, v ? 1 : 0);
    p_lua_setglobal(L, name);
}

static bool GlobalIsTable(lua_State* L, const char* name) {
    p_lua_getglobal(L, name);
    int t = p_lua_type(L, -1);
    p_lua_settop(L, -2);
    return t == LUA_TTABLE;
}

// Lua 写回的字符串完成标记（"1"）；nil / 非串（表未就绪）均视为未完成
static bool GlobalStrFlag(lua_State* L, const char* name) {
    if (!p_lua_tolstring) return false;
    p_lua_getglobal(L, name);
    bool ok = false;
    if (p_lua_type(L, -1) == LUA_TSTRING) {
        const char* v = p_lua_tolstring(L, -1, nullptr);
        ok = (v != nullptr && v[0] == '1');
    }
    p_lua_settop(L, -2);
    return ok;
}

static bool RunLua(lua_State* L, const char* chunk) {
    if (p_luaL_loadstring(L, chunk) != 0) {
        const char* err = p_lua_tolstring ? p_lua_tolstring(L, -1, nullptr) : "load err";
        LOGE("[Match2] loadstring: %s", err ? err : "?");
        p_lua_settop(L, -2);
        return false;
    }
    if (p_lua_pcall(L, 0, 0, 0) != 0) {
        const char* err = p_lua_tolstring ? p_lua_tolstring(L, -1, nullptr) : "pcall err";
        LOGE("[Match2] pcall: %s", err ? err : "?");
        p_lua_settop(L, -2);
        return false;
    }
    return true;
}

#if MATCH2_LUA_DUMP_TRACE
/* ----------------------------------------------------------------
 * Starry Recon —— 运行期 Lua 面枚举（只读、pass-through，仅诊断包）
 *
 * 为什么需要它：反作弊 / 重放校验 / 上报出口这几个模块走
 * PersistentLoader（EnvTask 运行期），**不经过 luaL_loadbuffer***，
 * 所以字节码 dump 抓不到本体，也就无从静态定位。这里改为在真机运行时
 * 直接枚举 Lua 表名·方法名·函数源码路径·调用记录（参数 + 返回值），
 * 并把「长度 >=512 的字符串实参」当字节码/上报载荷存下来回传，
 * 供离线反汇编与字段账本使用。
 *
 * 安全约束：所有包装都是 pass-through（原参数原样转发、返回值原样返回），
 *           每段都套 pcall，出错只写 #ERRORS 不改游戏行为（run 全关也照跑）。
 * ---------------------------------------------------------------- */
static std::atomic<bool> g_reconPending{true};
static std::atomic<int> g_reconRound{0};

static const char* kReconScript = R"LUA(
local S = rawget(_G, "__STARRY_RECON")
local FIRST = (S == nil)
if FIRST then
	S = { log = {}, drops = 0, wrapped = {}, counters = {}, blobs = {}, snap = 0, errs = {}, seen = {} }
	_G.__STARRY_RECON = S
end
local O = {}
local function L(s) O[#O + 1] = s end
local function ERR(where, err)
	if #S.errs < 40 then S.errs[#S.errs + 1] = where .. ": " .. tostring(err) end
end
local function hexhead(s, n)
	n = n or 16
	local t = {}
	for i = 1, math.min(#s, n) do t[i] = string.format("%02x", s:byte(i)) end
	return table.concat(t)
end
local function short(v)
	local t = type(v)
	if t == "string" then
		if #v > 200 then return string.format("%q..(len=%d)", v:sub(1, 200), #v) end
		return string.format("%q", v)
	elseif t == "number" or t == "boolean" or t == "nil" then
		return tostring(v)
	elseif t == "table" then return "<table>"
	elseif t == "function" then return "<fn>"
	else return "<" .. t .. ">" end
end
local function dump(v, depth, maxk)
	depth = depth or 0
	maxk = maxk or 12
	if type(v) ~= "table" then return short(v) end
	if depth >= 2 then return "<tbl>" end
	local parts, n = {}, 0
	local ok = pcall(function()
		for k, vv in pairs(v) do
			n = n + 1
			if n > maxk then parts[#parts + 1] = "..." break end
			local kk = tostring(k)
			if type(k) ~= "string" and type(k) ~= "number" then kk = "?" end
			parts[#parts + 1] = kk .. "=" .. dump(vv, depth + 1, 6)
		end
	end)
	if not ok then return "<tbl!>" end
	local s = table.concat(parts, ",")
	if #s > 500 then s = s:sub(1, 500) .. "..." end
	return "{" .. s .. "}"
end
local function srcof(f)
	if type(debug) ~= "table" or type(debug.getinfo) ~= "function" then return "" end
	local ok, info = pcall(debug.getinfo, f, "S")
	if not ok or type(info) ~= "table" then return "" end
	return "  @" .. tostring(info.short_src or info.source or "?") .. ":" .. tostring(info.linedefined or -1)
end
local function addlog(s)
	if #S.log >= 1500 then S.drops = S.drops + 1 return end
	if #s > 600 then s = s:sub(1, 600) .. "..." end
	S.log[#S.log + 1] = s
end
local WRAPN, WRAP_MAX = 0, 160
local function wrap(tname, key)
	local t = _G[tname]
	if type(t) ~= "table" then return false end
	local f = rawget(t, key)
	if type(f) ~= "function" then return false end
	local tag = tname .. "." .. key
	if S.wrapped[tag] == f then return false end
	if WRAPN >= WRAP_MAX then return false end
	WRAPN = WRAPN + 1
	local nf = function(...)
		local n = select("#", ...)
		local c = (S.counters[tag] or 0) + 1
		S.counters[tag] = c
		if c <= 20 or (c % 50) == 0 then
			local parts = {}
			for i = 1, (n > 4 and 4 or n) do parts[i] = dump((select(i, ...)), 0, 10) end
			addlog(tag .. "(" .. table.concat(parts, " ; ") .. ")")
		end
		if c <= 200 then
			for i = 1, n do
				local a = select(i, ...)
				if type(a) == "string" and #a >= 512 and #a <= (2 << 20) then
					S.blobs[#S.blobs + 1] = a
					if #S.blobs > 16 then table.remove(S.blobs, 1) end
					addlog("BLOB " .. tag .. " arg" .. i .. " len=" .. #a .. " head=" .. hexhead(a, 16))
				end
			end
		end
		local ret = table.pack(f(...))
		if ret.n > 0 then
			S.counters[tag .. " #ret"] = (S.counters[tag .. " #ret"] or 0) + 1
			if c <= 20 or (c % 50) == 0 then addlog(tag .. " -> " .. dump(ret[1], 0, 8)) end
		end
		return table.unpack(ret, 1, ret.n)
	end
	S.wrapped[tag] = nf
	t[key] = nf
	S.counters[tag .. " #wrap"] = (S.counters[tag .. " #wrap"] or 0) + 1
	return true
end
local TARGETS = {
	"AntiCheatManager", "AntiCheat", "CheatManager", "EnvTaskManager", "EnvTask",
	"ReplayInput", "ReplayRecord", "ReplayUtility", "ReplayManager", "Replay",
	"ServerEventManager", "ServerEvent", "NetworkManager", "NetManager", "Network",
	"SocketManager", "Socket", "HttpManager", "Http", "ReportManager",
	"DeviceInfo", "UIManager", "GameText", "LocalStorage", "LuaManager",
	"PersistentLoader", "SignatureLoader", "GameConfig", "UserInfoManager",
	"LoginManager", "CodeUpdater", "ExternalPlayer", "Analytics", "TrackManager",
	"Statistics", "SDKManager", "Report", "MsgManager", "MessageManager",
	"EventManager", "AEventManager", "GlobalEvent", "TimerManager",
}
local KEYPATS = { "report", "upload", "track", "stat", "analytics", "log", "send", "post",
	"request", "submit", "push", "msg", "message", "cheat", "anti", "network", "net",
	"http", "socket", "server", "device", "replay", "risk", "black", "ban", "verify" }
local function isreport(k)
	if type(k) ~= "string" then return false end
	local lk = k:lower()
	for i = 1, #KEYPATS do
		if lk:find(KEYPATS[i], 1, true) then return true end
	end
	return false
end
local function dump_table(tname, t, out)
	local ks = {}
	for k in pairs(t) do ks[#ks + 1] = k end
	table.sort(ks, function(a, b) return tostring(a) < tostring(b) end)
	out[#out + 1] = "# TABLE " .. tname .. "  keys=" .. #ks
	local n = 0
	for i = 1, #ks do
		n = n + 1
		if n > 120 then out[#out + 1] = "  ..." break end
		local k = ks[i]
		local v = t[k]
		local tv = type(v)
		out[#out + 1] = "  " .. tostring(k) .. " : " .. tv .. (tv == "function" and srcof(v) or "")
	end
end

-- PHASE 1：确保包装在位（幂等；模块热重载后自动重挂）
local okw, ew = pcall(function()
	local cand = {}
	for i = 1, #TARGETS do cand[#cand + 1] = TARGETS[i] end
	for k, v in pairs(_G) do
		if type(k) == "string" and type(v) == "table" and not S.seen["G:" .. k] then
			if isreport(k) then
				cand[#cand + 1] = k
				S.seen["G:" .. k] = 1
			end
		end
	end
	for i = 1, #cand do
		local t = _G[cand[i]]
		if type(t) == "table" then
			local ks = {}
			for k, v in pairs(t) do
				if type(v) == "function" and isreport(k) then ks[#ks + 1] = k end
			end
			for j = 1, #ks do wrap(cand[i], ks[j]) end
		end
	end
	local pair = {
		{ "UIManager", "OpenWindow" }, { "UIManager", "ShowWindow" }, { "UIManager", "CloseWindow" },
		{ "LuaManager", "CreateEnvTaskRuntime" }, { "LuaManager", "LoadModule" },
		{ "EnvTaskManager", "RunTask" }, { "PersistentLoader", "Load" },
		{ "GameText", "Get" }, { "LocalStorage", "SetString" }, { "LocalStorage", "GetString" },
	}
	for i = 1, #pair do wrap(pair[i][1], pair[i][2]) end
end)
if not okw then ERR("phase1", ew) end

-- PHASE 2：组装快照
S.snap = S.snap + 1
L("# STARRY RECON SNAP " .. S.snap)
L("# clock=" .. tostring((type(os) == "table" and os.time and os.time()) or -1))
if S.snap == 1 or S.snap == 6 or S.snap == 12 then
	local gk = {}
	for k in pairs(_G) do
		if type(k) == "string" then gk[#gk + 1] = k end
	end
	table.sort(gk)
	L("# G_KEYS " .. #gk)
	L("# G " .. table.concat(gk, " "))
end
L("# TABLES")
for i = 1, #TARGETS do
	local t = _G[TARGETS[i]]
	if t ~= nil then
		if type(t) == "table" then pcall(dump_table, TARGETS[i], t, O)
		else L("# TABLE " .. TARGETS[i] .. "  type=" .. type(t) .. " (非 table，无法在 Lua 层包装)") end
	end
end
if S.snap == 1 then
	for k, v in pairs(_G) do
		if type(k) == "string" and type(v) == "table" and S.seen["G:" .. k] then
			pcall(dump_table, k, v, O)
		end
	end
end
L("# WRAPPED " .. WRAPN .. " drops=" .. S.drops)
local ck = {}
for k, v in pairs(S.counters) do ck[#ck + 1] = string.format("%-52s %d", k, v) end
table.sort(ck)
L("# COUNTERS " .. #ck)
for i = 1, #ck do L("  " .. ck[i]) end
L("# LOG " .. #S.log)
for i = 1, #S.log do L("  " .. S.log[i]) end
if #S.errs > 0 then
	L("# ERRORS")
	for i = 1, #S.errs do L("  " .. S.errs[i]) end
end
_G.__starry_recon = table.concat(O, "\n")
if _G.__starry_blob_taken ~= nil then _G.__starry_blob_taken = nil end
_G.__starry_blob = table.remove(S.blobs, 1)
)LUA";

// 读回 Lua 全局字符串并落盘（返回是否写出）
static bool WriteGlobalStringLua(lua_State* L, const char* gname, const char* file) {
    if (!p_lua_tolstring) return false;
    p_lua_getglobal(L, gname);
    bool done = false;
    if (p_lua_type(L, -1) == LUA_TSTRING) {
        size_t n = 0;
        const char* s = p_lua_tolstring(L, -1, &n);
        if (s && n > 0) {
            mkdir(kLuaDumpDir, 0777);
            FILE* fp = fopen(file, "wb");
            if (fp) {
                size_t w = fwrite(s, 1, n, fp);
                fclose(fp);
                done = (w == n);
            }
        }
    }
    p_lua_settop(L, -2);
    return done;
}

static void MaybeRunRecon(lua_State* L) {
    if (!RunLua(L, kReconScript)) return;
    int round = g_reconRound.load() + 1;
    char file[512];
    snprintf(file, sizeof(file), "%s/recon_%02d.txt", kLuaDumpDir, round);
    if (WriteGlobalStringLua(L, "__starry_recon", file)) {
        g_reconRound.store(round);
        g_reconPending.store(false);
        MATCH2_TRACE("recon snapshot %d -> %s", round, file);
    }
    // 回传一个「大字符串实参」样本（EnvTask 字节码 / 上报载荷）
    char blob[512];
    snprintf(blob, sizeof(blob), "%s/blob_%02d.bin", kLuaDumpDir, round);
    if (WriteGlobalStringLua(L, "__starry_blob", blob)) {
        SetGlobalBool(L, "__starry_blob_taken", true);
        MATCH2_TRACE("blob %d -> %s", round, blob);
    }
}
#endif // MATCH2_LUA_DUMP_TRACE

static void SyncOnMain(void* luaManager) {
    if (!LoadXLuaApi()) return;
    lua_State* L = GetLuaState(luaManager);
    if (!L) return;

#if MATCH2_LUA_DUMP_TRACE
    if (g_reconPending.load()) MaybeRunRecon(L);
#endif

    bool inv = false, one = false, fast = false;
    if (!g_forceOff.load()) {
        inv = g_match2_inv;
        one = g_match2_one;
        fast = g_match2_unlock;
    }

    // 仅在需要功能或已打补丁时写全局，避免启动期乱动 Lua
    if (g_wantInstall.load() || g_patched || inv || one || fast || g_forceOff.load()) {
        SetGlobalBool(L, "AN_1", one);
        SetGlobalBool(L, "AN_2", inv);
        SetGlobalBool(L, "AN_3", fast);
    }

    // 幂等 install：不依赖易丢的全局 flag；表重载后可再挂
    // 完成判定读 Lua 写回的 __starry_fight / __starry_unlock：
    // 只有两项都装齐才清 wantInstall，避免 UserInfoManager 晚于 CFightSystem 就绪时提前放弃
    if (g_wantInstall.load() || inv || one || fast) {
        if (GlobalIsTable(L, "CFightSystem") || GlobalIsTable(L, "UserInfoManager")) {
            if (RunLua(L, kInstallScript)) {
                bool fightDone = GlobalStrFlag(L, "__starry_fight");
                bool unlockDone = GlobalStrFlag(L, "__starry_unlock");
                if (fightDone && unlockDone) {
                    g_wantInstall.store(false);
                    if (!g_patched) {
                        g_patched = true;
                        LOGI("[Match2] patch installed (fight=1 unlock=1)");
                    }
                }
            }
        }
        // 两张表都未就绪：保留 wantInstall，下次 Update 再试
    }

    // 基线：反作弊压制 + 上报拦截。与菜单开关无关，常开。
    // 模块热重载/切场景会把包好的表换成新表，故这里每次都重跑一遍
    // （Lua 侧逐表 rawget("__starry_ac") 自幂等：装齐过就是零成本空转）。
    if (GlobalIsTable(L, "AntiCheatManager") || GlobalIsTable(L, "BIRequest") ||
        GlobalIsTable(L, "UIManager")) {
        if (RunLua(L, kAcScript) && GlobalStrFlag(L, "__starry_ac")) {
            if (!g_acDone.load()) {
                g_acDone.store(true);
                LOGI("[Match2] 反作弊压制/上报拦截武装完成 (__starry_ac=1)");
            }
        }
    }

    g_forceOff.store(false);
}

static void Hooked_Update(void* thiz) {
    if (g_origUpdate) g_origUpdate(thiz);
    if (!g_ready.load()) return;
#if MATCH2_LUA_DUMP_TRACE
    // 诊断：约每 15 秒（900 帧）做一次 recon 快照，最多 12 次（≈3 分钟）；
    // 首次在钩子装上后即时做一次，便于对比「早期 / 玩过一局之后」的差异。
    {
        static int s_tick = 0;
        if (g_reconRound.load() < 12 && (++s_tick % 900) == 1) {
            g_reconPending.store(true);
            g_wantSync.store(true);
        }
    }
#endif
    // 基线脉冲：过检测/上报拦截不依赖菜单开关，所以不能等玩家开功能才挂。
    // 常开：约每 15 秒（900 帧）主动跑一次 sync，Lua 热重载后可自动补挂（幂等）。
    {
        static int s_acTick = 0;
        if ((++s_acTick % 900) == 1) g_wantSync.store(true);
    }
    if (!g_wantSync.load() && !g_wantInstall.load()) return;
    if (g_inHook.exchange(true)) return;
    if (IsValidHeapPtr(thiz)) {
        SyncOnMain(thiz);
    }
    g_wantSync.store(false);
    g_inHook.store(false);
}

bool Match2_TryInit() {
    if (g_ready.load()) return true;
    auto& api = GetApi();
    if (!api.Load(GameUnity::TARGET_LIB)) return false;

    // 勿对 domain/assemblies 套 SafeProbe：装 SIGSEGV handler 后在后台线程调
    // il2cpp_domain_get_assemblies 可能卡死（本机已复现）。domain 未起时 Domain()
    // 通常返回 nullptr；等非空再 FindClass。
    void* domain = api.Domain();
    if (!domain) {
        static int s_domMiss = 0;
        if ((++s_domMiss % 8) == 1) LOGI("[Match2] il2cpp domain not ready yet");
        return false;
    }
    size_t asmCount = 0;
    void** assemblies = api.Assemblies(&asmCount);
    if (!assemblies || asmCount == 0) {
        static int s_asmMiss = 0;
        if ((++s_asmMiss % 8) == 1) LOGI("[Match2] domain assemblies empty");
        return false;
    }

#if MATCH2_LUA_DUMP_TRACE
    InstallXluaLoadHook();
    InstallSignatureLoaderHook();
#endif
    // DNS 层上报拦截兜底重试：构造器已装过一次，这里防 so 加载时序异常
    InstallDnsBlockHooks();
    if (!LoadXLuaApi()) return false;

    void* klass = api.FindClassByShortName("LuaManager");
    if (!klass) {
        static int s_clsMiss = 0;
        if ((++s_clsMiss % 8) == 1)
            LOGI("[Match2] LuaManager not found (asmCount=%zu)", asmCount);
        return false;
    }
    g_luaManagerClass = klass;

    g_offLuaEnv = api.FieldOffset(g_luaManagerClass, "_luaEnvironment", 0);
    g_offInitialized = api.FieldOffset(g_luaManagerClass, "<Initialized>k__BackingField", 0);
    if (g_offLuaEnv == 0) g_offLuaEnv = 0x28;
    if (g_offInitialized == 0) g_offInitialized = 0x30;

    void* updateMethod = api.GetMethod(g_luaManagerClass, "Update", 0);
    void* updatePtr = MethodPointer(updateMethod);
    if (!updatePtr) {
        LOGE("[Match2] LuaManager.Update pointer missing");
        return false;
    }

    if (!g_origUpdate) {
        if (DobbyHook(updatePtr, (void*)Hooked_Update, (void**)&g_origUpdate) != 0) {
            LOGE("[Match2] DobbyHook Update failed");
            return false;
        }
        LOGI("[Match2] hooked LuaManager.Update @ %p", updatePtr);
    }

    g_ready.store(true);
    g_wantSync.store(true);
    LOGI("[Match2] init ok (xlua C API) envOff=0x%zx", g_offLuaEnv);
    return true;
}

#if MATCH2_LUA_DUMP_TRACE
bool Match2_TryInitTraceEarly() {
    return InstallXluaLoadHook();
}
#else
bool Match2_TryInitTraceEarly() { return false; }
#endif

bool Match2_IsReady() { return g_ready.load(); }

void Match2_OnFeatureChanged(int featNum, bool enabled) {
    (void)featNum;
    if (enabled) g_wantInstall.store(true);
    g_wantSync.store(true);
}

void Match2_ForceClear() {
    g_match2_inv = false;
    g_match2_one = false;
    g_match2_unlock = false;
    g_forceOff.store(true);
    g_wantSync.store(true);
}

void Match2_Tick(const bool* features) {
    if (!features || !g_ready.load()) return;
    bool inv = features[MATCH2_FEAT_INVINCIBLE];
    bool one = features[MATCH2_FEAT_ONESHOT];
    bool fast = features[MATCH2_FEAT_AUTOUNLOCK];
    if (inv != g_match2_inv || one != g_match2_one || fast != g_match2_unlock || g_forceOff.load()) {
        g_match2_inv = inv;
        g_match2_one = one;
        g_match2_unlock = fast;
        if (inv || one || fast) g_wantInstall.store(true);
        g_wantSync.store(true);
    }
    // CFightSystem 可能晚于菜单开关才就绪；低频脉冲直到装上补丁
    if (!g_patched && g_wantInstall.load()) {
        static int s_pulse = 0;
        if ((++s_pulse % 30) == 0) g_wantSync.store(true);
    }
}
