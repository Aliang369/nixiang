/* ================================================================
 *  消消英雄2 — 在 LuaManager.Update 主线程注入 xLua 补丁
 *  不用 il2cpp DoString（会 SIGSEGV），改走 libxlua C API
 *
 *  功能（改包 AN_* 同款）：
 *    AN_1 十倍伤害 · AN_2 无敌 · AN_3 解锁自动战斗（游戏内自动战斗 + 3 倍速）
 *  过检测 / ReplayInput / AntiCheat 补丁：本版不写（有问题，暂关）
 * ================================================================ */

#include "Match2Mod.h"

#include <android/log.h>
#include <atomic>
#include <cstring>
#include <dlfcn.h>
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

static void SyncOnMain(void* luaManager) {
    if (!LoadXLuaApi()) return;
    lua_State* L = GetLuaState(luaManager);
    if (!L) return;

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

    g_forceOff.store(false);
}

static void Hooked_Update(void* thiz) {
    if (g_origUpdate) g_origUpdate(thiz);
    if (!g_ready.load()) return;
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
