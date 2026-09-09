/* ================================================================
 *  消消英雄2 — 在 LuaManager.Update 主线程注入 xLua 补丁
 *  不用 il2cpp DoString（会 SIGSEGV），改走 libxlua C API
 *
 *  功能（改包 AN_* 同款）：
 *    AN_1 十倍伤害 · AN_2 无敌 · AN_3 瞬间结束战斗
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
static bool g_match2_fast = false;
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

enum { LUA_TNIL = 0, LUA_TTABLE = 5 };

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

static bool ShouldDumpModule(const char* path) {
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
    if (n <= 40 || (name && ShouldDumpModule(name))) {
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

    if (n <= 30 || (n % 100) == 1 || (path[0] && ShouldDumpModule(path))) {
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

// 幂等：CFightSystem 重载后 __starry_patched 丢失时再挂一次；已挂则 rawget 直接 return
static const char* kInstallScript = R"LUA(
AN_1 = AN_1 or false
AN_2 = AN_2 or false
AN_3 = AN_3 or false
if type(CFightSystem) ~= "table" then return end
if rawget(CFightSystem, "__starry_patched") then return end

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

local _DispatchGemMessage = CFightSystem.DispatchGemMessage
if type(_DispatchGemMessage) == "function" then
	function CFightSystem:DispatchGemMessage(type, ...)
		if type == CGemMessageType.FightBegan and AN_3 then
			if TimeScaleManager ~= nil and InputManager ~= nil
				and InputManager.GetPlayerMode() == PlayerMode.SinglePlayer
				and (TutorialManager == nil or TutorialManager.GetExecuter() == nil) then
				TimeScaleManager.SetTimeScale(TimeScaleType.TestFastPlay, 5)
				if GameConfig ~= nil then GameConfig.SetFastPlayMode(true) end
				if InputManager.GetCurrentMode() == InputMode.Human then
					InputManager.SetCurrentMode(InputMode.Autoplay)
					InputManager.SetAutoCastSkill(true)
				end
			end
		end
		return _DispatchGemMessage(self, type, ...)
	end
end

CFightSystem.__starry_patched = true
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

static bool IsFightSystemTable(lua_State* L) {
    p_lua_getglobal(L, "CFightSystem");
    int t = p_lua_type(L, -1);
    p_lua_settop(L, -2);
    return t == LUA_TTABLE;
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
        fast = g_match2_fast;
    }

    // 仅在需要功能或已打补丁时写全局，避免启动期乱动 Lua
    if (g_wantInstall.load() || g_patched || inv || one || fast || g_forceOff.load()) {
        SetGlobalBool(L, "AN_1", one);
        SetGlobalBool(L, "AN_2", inv);
        SetGlobalBool(L, "AN_3", fast);
    }

    // 幂等 install：不依赖易丢的全局 flag；CFightSystem 重载后可再挂
    if (g_wantInstall.load() || inv || one || fast) {
        if (IsFightSystemTable(L)) {
            if (RunLua(L, kInstallScript)) {
                if (!g_patched) {
                    g_patched = true;
                    LOGI("[Match2] fight patch installed");
                }
                g_wantInstall.store(false);
            }
        }
        // CFightSystem 未就绪：保留 wantInstall，下次 Update 再试
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
    g_match2_fast = false;
    g_forceOff.store(true);
    g_wantSync.store(true);
}

void Match2_Tick(const bool* features) {
    if (!features || !g_ready.load()) return;
    bool inv = features[MATCH2_FEAT_INVINCIBLE];
    bool one = features[MATCH2_FEAT_ONESHOT];
    bool fast = features[MATCH2_FEAT_FASTEND];
    if (inv != g_match2_inv || one != g_match2_one || fast != g_match2_fast || g_forceOff.load()) {
        g_match2_inv = inv;
        g_match2_one = one;
        g_match2_fast = fast;
        if (inv || one || fast) g_wantInstall.store(true);
        g_wantSync.store(true);
    }
    // CFightSystem 可能晚于菜单开关才就绪；低频脉冲直到装上补丁
    if (!g_patched && g_wantInstall.load()) {
        static int s_pulse = 0;
        if ((++s_pulse % 30) == 0) g_wantSync.store(true);
    }
}
