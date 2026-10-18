/* ================================================================
 *  KdjxsfMod.cpp — 口袋觉醒（私服 com.zcdsg.xmgm 3.10）星光菜单模块
 *
 *  目标库：libcocos2dlua.so（12,550,352 B / md5 ba1746167a964004da8d20beb5b2cfda）
 *  引擎：Cocos2d-x + LuaJIT 2.1(GC64)；游戏脚本 = 热更目录里 "ccc" 加密的
 *        LuaJIT 自定义字节码，由 libnp_protect_res.so(xhook 壳) 落地。
 *
 *  注入路线（真机已验证）：
 *    ① lua_* 未导出（dynsym 仅 4 个 JNI 桥）⇒ 用固定 RVA + 序言 8 字节校验
 *    ② lua_State* 存于 so 内静态槽 base+0xc07d68（JNI 桥 0x180c60 直接取用）
 *    ③ Lua 必须在 GL 线程调用（离线程调用实测 SIGABRT in GLThread）
 *       ⇒ Dobby hook 导出符号 Java_..._Cocos2dxRenderer_nativeRender（0x17d40c）
 *    ④ 补丁 = loadstring + pcall(L,1,1,0) 取函数 + pcall(L,0,0,0) 执行
 *    ⑤ 开关通道 = 调 Lua 全局 __kdjx_set("inv,dmg,spd,skill")
 *
 *  RVA 铁律：全部带序言校验；任一不符 ⇒ 拒绝装钩并打日志（不猜、不硬上）。
 * ================================================================ */

#include "KdjxsfMod.h"
#include "KdjxsfPatch.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <mutex>
#include <string>

#include <pthread.h>
#include <sys/syscall.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"

#define KDJX_LOG "[Kdjx]"

static const char* kKdjxLib = "libcocos2dlua.so";
static const char* kKdjxRenderSym = "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeRender";

// ============================================================
// ① RVA / 序言指纹（so 版本 3.10 / md5 ba174616…；换包必须重取）
// ============================================================
#define KDJX_RVA_RENDER        0x17d40cUL
#define KDJX_RVA_LUA_GETTOP    0x31e054UL
#define KDJX_RVA_LUA_SETTOP    0x31e068UL
#define KDJX_RVA_LUA_TYPE      0x31e2acUL
#define KDJX_RVA_LUA_PUSHSTR   0x31f344UL
#define KDJX_RVA_LUA_GETFIELD  0x31faecUL
#define KDJX_RVA_LUA_PCALL     0x320764UL
#define KDJX_RVA_GLBCALL       0x180c60UL   // callLuaGlobalFunctionWithString(name, str)
#define KDJX_RVA_STATE_SLOT    0xc07d68UL   // *(void**) = lua_State*

#define KDJX_LUA_GLOBALSINDEX  (-10002)

struct KdjxSig {
    uintptr_t rva;
    const char* name;
    unsigned char sig[8];
};

static const KdjxSig kSigs[] = {
    {KDJX_RVA_LUA_GETTOP,   "lua_gettop",   {0x01,0x14,0x40,0xf9,0x00,0x10,0x40,0xf9}},
    {KDJX_RVA_LUA_SETTOP,   "lua_settop",   {0xff,0x83,0x00,0xd1,0xf3,0x53,0x00,0xa9}},
    {KDJX_RVA_LUA_TYPE,     "lua_type",     {0xff,0x43,0x00,0xd1,0xf3,0x7b,0x00,0xa9}},
    {KDJX_RVA_LUA_PUSHSTR,  "lua_pushstring",{0xff,0x83,0x00,0xd1,0xf3,0x7b,0x00,0xa9}},
    {KDJX_RVA_LUA_GETFIELD, "lua_getfield", {0xff,0xc3,0x00,0xd1,0xf5,0x7b,0x01,0xa9}},
    {KDJX_RVA_LUA_PCALL,    "lua_pcall",    {0xff,0x83,0x00,0xd1,0x7f,0x00,0x1f,0x6b}},
};

// ============================================================
// ② 状态
// ============================================================
static std::mutex       g_installMutex;
static std::atomic<bool> g_hookInstalled{false};
static std::atomic<bool> g_sigOk{false};
static std::atomic<bool> g_patchInstalled{false};
static std::atomic<uint32_t> g_installPatchedCount{0};
static std::atomic<bool> g_patchFailed{false};
static std::atomic<uint32_t> g_frames{0};
static std::atomic<uint32_t> g_installTries{0};

static std::atomic<bool> g_inv{false};
static std::atomic<int>  g_dmg{1};
static std::atomic<int>  g_spd{1};
static std::atomic<bool> g_skill{false};

static int g_lastSync[4] = {-1, -1, -1, -1};

static uintptr_t g_base = 0;

// lua C API（RVA 解析后使用）
typedef int  (*fn_gettop)(void*);
typedef void (*fn_settop)(void*, int);
typedef int  (*fn_type)(void*, int);
typedef void (*fn_pushstring)(void*, const char*);
typedef void (*fn_getfield)(void*, int, const char*);
typedef int  (*fn_pcall)(void*, int, int, int);
typedef int  (*fn_glbcall)(const char*, const char*);

static fn_gettop     p_gettop = nullptr;
static fn_settop     p_settop = nullptr;
static fn_type       p_type = nullptr;
static fn_pushstring p_pushstring = nullptr;
static fn_getfield   p_getfield = nullptr;
static fn_pcall      p_pcall = nullptr;
static fn_glbcall    p_glbcall = nullptr;

static void (*g_origRender)(JNIEnv*, jclass) = nullptr;

/* lua_State 发现：不依赖单一静态槽（实测游戏在登录前的主 VM 里没有 ObjectModel）。
 * 改 hook lua_gettop 抓真实调用者的第一实参，作为候选集合，OnFrame 里逐个验 ObjectModel。 */
static int (*g_origGetTop)(void*) = nullptr;
static void* g_candL[16] = {nullptr};
static bool  g_candPatched[16] = {false};
static pid_t g_candTid[16] = {0};
static std::atomic<int> g_candN{0};
static std::atomic<pid_t> g_renderTid{0};   // nativeRender 首次进入的线程（= 主 Lua 线程）

static pid_t CurTid() {
    return (pid_t)syscall(SYS_gettid, 0, 0, 0, 0, 0, 0);
}

static int HookGetTop(void* L) {
    int n = g_candN.load();
    if (L && ((uintptr_t)L > 0x1000)) {
        bool found = false;
        for (int i = 0; i < n; ++i) { if (g_candL[i] == L) { found = true; break; } }
        if (!found && n < 16) { g_candL[n] = L; g_candTid[n] = CurTid(); g_candN.store(n + 1); }
    }
    return g_origGetTop ? g_origGetTop(L) : 0;
}

// ============================================================
// ③ 工具
// ============================================================
static uint64_t NowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (uint64_t)ts.tv_sec * 1000ull + (uint64_t)ts.tv_nsec / 1000000ull;
}

static bool VerifySigs(uintptr_t base) {
    for (size_t i = 0; i < sizeof(kSigs) / sizeof(kSigs[0]); ++i) {
        const unsigned char* p = (const unsigned char*)(base + kSigs[i].rva);
        if (memcmp(p, kSigs[i].sig, 8) != 0) {
            LOGE(KDJX_LOG " 序言不符: %s @0x%lx", kSigs[i].name, (unsigned long)kSigs[i].rva);
            return false;
        }
    }
    return true;
}

static void* GetLuaState() {
    if (!g_base) return nullptr;
    void** slot = (void**)(g_base + KDJX_RVA_STATE_SLOT);
    void* L = *slot;
    if (!L || ((uintptr_t)L) < 0x1000) return nullptr;
    return L;
}

/* 在 GL 线程上执行一段 Lua 源码；返回 false 表示语法/运行错误 */
static bool RunLuaSource(void* L, const char* src) {
    if (!L || !p_getfield || !p_pushstring || !p_pcall || !p_settop) return false;
    const int top = p_gettop(L);
    p_getfield(L, KDJX_LUA_GLOBALSINDEX, "loadstring");
    p_pushstring(L, src);
    int rc = p_pcall(L, 1, 1, 0);
    if (rc != 0) {
        p_settop(L, top);
        LOGE(KDJX_LOG " loadstring rc=%d", rc);
        return false;
    }
    rc = p_pcall(L, 0, 0, 0);
    p_settop(L, top);
    if (rc != 0) {
        LOGE(KDJX_LOG " patch pcall rc=%d", rc);
        return false;
    }
    return true;
}

/* 开发期：优先加载 /sdcard/kd_patch.lua（免重编 so 迭代补丁）；无文件则用内嵌补丁。
 * 发布前删除本分支即可（见 10_docs/口袋觉醒/ 的清理清单）。 */
static bool ReadDevPatch(std::string* out) {
    const char* paths[3] = {"/sdcard/kd_patch.lua",
                            "/storage/emulated/0/kd_patch.lua",
                            "/data/data/com.zcdsg.xmgm/files/kd_patch.lua"};
    for (int i = 0; i < 3; ++i) {
        FILE* f = fopen(paths[i], "rb");
        if (!f) continue;
        std::string buf;
        char tmp[4096];
        size_t n;
        while ((n = fread(tmp, 1, sizeof(tmp), f)) > 0) buf.append(tmp, n);
        fclose(f);
        if (!buf.empty()) { *out = buf; return true; }
    }
    return false;
}

static bool TryInstallPatchOn(void* L);

static bool TryInstallPatchLocked() {
    /* 游戏可能在登录/进关时重建 Lua VM ⇒ 对**每个新出现的 state** 都装一遍，
     * 不能"装成功一次就收工"（2026-10-19 实测：首轮补丁装进登录 VM，战斗内有 0 条埋点）。 */
    int n = g_candN.load();
    bool anyPatched = false;
    const pid_t me = g_renderTid.load();
    for (int i = 0; i < n; ++i) {
        if (g_candPatched[i]) { anyPatched = true; continue; }
        if (me != 0 && g_candTid[i] != me) continue;   // 别的线程的 VM 不碰（跨线程注入=堆损坏）
        if (TryInstallPatchOn(g_candL[i])) {
            g_candPatched[i] = true;
            g_installPatchedCount.fetch_add(1);
            anyPatched = true;
            LOGI(KDJX_LOG " 补丁装进 state#%d（累计 %u）", i, g_installPatchedCount.load());
        }
    }
    void* slotL = GetLuaState();
    if (slotL) {
        bool known = false;
        for (int i = 0; i < n; ++i) { if (g_candL[i] == slotL) { known = true; break; } }
        if (!known && n < 16) {
            g_candL[n] = slotL;
            g_candTid[n] = CurTid();
            g_candN.store(n + 1);
            if (TryInstallPatchOn(slotL)) {
                g_candPatched[n] = true;
                g_installPatchedCount.fetch_add(1);
                anyPatched = true;
                LOGI(KDJX_LOG " 补丁装进 slot state#%d", n);
            }
        }
    }
    if (!anyPatched) {
        uint32_t k = g_installTries.load();
        if ((k % 40) == 1) {
            LOGI(KDJX_LOG " 等待 Lua 就绪：候选 state=%d tries=%u", n, k);
        }
    }
    return anyPatched;
}

static bool TryInstallPatchOn(void* L) {
    if (!L) {
        LOGE(KDJX_LOG " lua_State 为空（slot=%p）", (void*)(g_base + KDJX_RVA_STATE_SLOT));
        return false;
    }
    // 合法性自检：gettop 必须返回合理值
    int top = p_gettop(L);
    if (top < 0 || top > 10000) {
        LOGE(KDJX_LOG " lua_State 自检失败 top=%d", top);
        return false;
    }
    if (top != 0) return false;   // 栈非空 ⇒ VM 正在执行 Lua，别在这里注入
    // ObjectModel 已注册才说明游戏 Lua 侧就绪
    p_getfield(L, KDJX_LUA_GLOBALSINDEX, "ObjectModel");
    int t = p_type(L, -1);
    p_settop(L, p_gettop(L) - 1);
    if (t != 5) return false;

    std::string devPatch;
    const char* src = kKdjxsfLuaPatch;
    if (ReadDevPatch(&devPatch)) {
        src = devPatch.c_str();
        LOGI(KDJX_LOG " 使用 /sdcard/kd_patch.lua（%zu B）", devPatch.size());
    }
    if (!RunLuaSource(L, src)) return false;
    g_lastSync[0] = -1;  // 强制首次写状态文件
    LOGI(KDJX_LOG " Lua 补丁已安装 top=%d", top);
    return true;
}

/* 开关下发走**文件**（游戏自己的 Lua 每秒轮询 kd_state.txt）：
 * 不做 C++→Lua 的 pcall 注入——实测该路径会在 LuaJIT 里把表节点写坏导致 SIGSEGV。 */
static void WriteStateFile(void) {
    const int cur[4] = {g_inv.load() ? 1 : 0, g_dmg.load(), g_spd.load(), g_skill.load() ? 1 : 0};
    if (cur[0] == g_lastSync[0] && cur[1] == g_lastSync[1] &&
        cur[2] == g_lastSync[2] && cur[3] == g_lastSync[3]) {
        return;
    }
    const char* path = "/data/data/com.zcdsg.xmgm/files/kd_state.txt";
    char tmp[128];
    snprintf(tmp, sizeof(tmp), "%s.tmp", path);
    FILE* f = fopen(tmp, "wb");
    if (!f) return;
    fprintf(f, "%d,%d,%d,%d\n", cur[0], cur[1], cur[2], cur[3]);
    fflush(f);
    fclose(f);
    if (rename(tmp, path) == 0) {
        g_lastSync[0] = cur[0]; g_lastSync[1] = cur[1];
        g_lastSync[2] = cur[2]; g_lastSync[3] = cur[3];
        LOGI(KDJX_LOG " state=%d,%d,%d,%d", cur[0], cur[1], cur[2], cur[3]);
    }
}

static void SyncState(void* L) {
    (void)L;
    WriteStateFile();
}

/* 每帧（GL 线程）调用：懒装补丁 + 同步开关 */
static void SyncAllStates(void) {
    int n = g_candN.load();
    const pid_t me = g_renderTid.load();
    for (int i = 0; i < n; ++i) {
        if (g_candPatched[i] && g_candTid[i] == me) SyncState(g_candL[i]);
    }
    void* slotL = GetLuaState();
    if (slotL) {
        for (int i = 0; i < n; ++i) { if (g_candL[i] == slotL) return; }
        SyncState(slotL);
    }
}

static void Kdjxsf_OnFrame() {
    const uint32_t n = g_frames.fetch_add(1) + 1;
    if ((n % 30) == 0 && !g_patchFailed.load()) {
        std::lock_guard<std::mutex> lk(g_installMutex);
        g_installTries.fetch_add(1);
        if (TryInstallPatchLocked()) {
            g_patchInstalled.store(true);
        } else if (!g_patchInstalled.load() && g_installTries.load() > 600) {
            g_patchFailed.store(true);
            LOGE(KDJX_LOG " 补丁安装超上限，放弃");
        }
    }
    if ((n % 6) != 0) return;
    SyncAllStates();
}

// ============================================================
// ④ nativeRender hook
// ============================================================
static void HookRender(JNIEnv* env, jclass clazz) {
    if (g_renderTid.load() == 0) g_renderTid.store(CurTid());
    if (g_origRender) g_origRender(env, clazz);
    Kdjxsf_OnFrame();
}

// ============================================================
// ⑤ 对外接口
// ============================================================
bool Kdjxsf_TryInit(JavaVM* vm) {
    (void)vm;
    if (g_hookInstalled.load()) return true;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_hookInstalled.load()) return true;

    uintptr_t base = getLibraryAddress(kKdjxLib);
    if (!base) return false;
    g_base = base;

    if (!g_sigOk.load()) {
        if (!VerifySigs(base)) {
            LOGE(KDJX_LOG " libcocos2dlua.so 序言校验失败（版本漂移？）base=0x%lx", (unsigned long)base);
            return false;
        }
        g_sigOk.store(true);
        p_gettop     = (fn_gettop)(base + KDJX_RVA_LUA_GETTOP);
        p_settop     = (fn_settop)(base + KDJX_RVA_LUA_SETTOP);
        p_type       = (fn_type)(base + KDJX_RVA_LUA_TYPE);
        p_pushstring = (fn_pushstring)(base + KDJX_RVA_LUA_PUSHSTR);
        p_getfield   = (fn_getfield)(base + KDJX_RVA_LUA_GETFIELD);
        p_pcall      = (fn_pcall)(base + KDJX_RVA_LUA_PCALL);
        p_glbcall    = (fn_glbcall)(base + KDJX_RVA_GLBCALL);
        LOGI(KDJX_LOG " lua api ready base=0x%lx", (unsigned long)base);
    }

    // lua_gettop hook（抓真实 lua_State；lua_gettop 实测 20 字节 ≥ Dobby 16 字节补丁）
    if (!g_origGetTop) {
        void* gt = (void*)(base + KDJX_RVA_LUA_GETTOP);
        int rc0 = DobbyHook(gt, (void*)HookGetTop, (void**)&g_origGetTop);
        if (rc0 == 0 && g_origGetTop) {
            p_gettop = (fn_gettop)g_origGetTop;   // 自调用必须走蹦床，否则递归
            LOGI(KDJX_LOG " lua_gettop hook ok（state 捕获）");
        } else {
            LOGW(KDJX_LOG " lua_gettop hook 失败 rc=%d（退回静态槽）", rc0);
        }
    }

    void* render = DobbySymbolResolver(kKdjxLib, kKdjxRenderSym);
    uintptr_t expect = base + KDJX_RVA_RENDER;
    if (!render) {
        LOGW(KDJX_LOG " DobbySymbolResolver miss %s", kKdjxRenderSym);
        return false;
    }
    if ((uintptr_t)render != expect) {
        LOGW(KDJX_LOG " nativeRender 地址漂移 sym=0x%lx rva=0x%lx",
             (unsigned long)(uintptr_t)render, (unsigned long)expect);
        // 以符号为准，但序言仍须匹配（防止导出了别的函数）
        const unsigned char* p = (const unsigned char*)render;
        const unsigned char want[8] = {0xfd,0x7b,0xbf,0xa9,0xfd,0x03,0x00,0x91};
        if (memcmp(p, want, 8) != 0) {
            LOGE(KDJX_LOG " nativeRender 序言不符，放弃");
            return false;
        }
    }

    int rc = DobbyHook(render, (void*)HookRender, (void**)&g_origRender);
    if (rc != 0 || !g_origRender) {
        LOGE(KDJX_LOG " DobbyHook(nativeRender) rc=%d", rc);
        return false;
    }
    g_hookInstalled.store(true);
    LOGI(KDJX_LOG " nativeRender hook ok @0x%lx", (unsigned long)(uintptr_t)render);
    return true;
}

bool Kdjxsf_IsReady() {
    return g_patchInstalled.load();
}

void Kdjxsf_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case KDJXSF_FEAT_INV:   g_inv.store(enabled); break;
        case KDJXSF_FEAT_SKILL: g_skill.store(enabled); break;
        case KDJXSF_FEAT_DMG:   g_dmg.store(enabled ? (g_dmg.load() > 1 ? g_dmg.load() : 1) : 1); break;
        case KDJXSF_FEAT_SPD:   g_spd.store(enabled ? (g_spd.load() > 1 ? g_spd.load() : 1) : 1); break;
        default: break;
    }
    LOGI(KDJX_LOG " feat[%d]=%d", featNum, enabled ? 1 : 0);
}

void Kdjxsf_OnValueChanged(int featNum, int value) {
    switch (featNum) {
        case KDJXSF_FEAT_DMG:
            if (value < 1) value = 1;
            if (value > 100) value = 100;
            g_dmg.store(value);
            break;
        case KDJXSF_FEAT_SPD:
            if (value < 1) value = 1;
            if (value > 5) value = 5;
            g_spd.store(value);
            break;
        default: break;
    }
    LOGI(KDJX_LOG " val[%d]=%d", featNum, value);
}

void Kdjxsf_Tick(const bool* features, const int* values) {
    if (features) {
        g_inv.store(features[KDJXSF_FEAT_INV] ? true : false);
        g_skill.store(features[KDJXSF_FEAT_SKILL] ? true : false);
    }
    if (values) {
        int d = values[KDJXSF_FEAT_DMG];
        int s = values[KDJXSF_FEAT_SPD];
        if (d >= 1 && d <= 100) g_dmg.store(d);
        if (s >= 1 && s <= 5) g_spd.store(s);
    }
}

void Kdjxsf_ForceClear() {
    g_inv.store(false);
    g_skill.store(false);
    g_dmg.store(1);
    g_spd.store(1);
    g_lastSync[0] = -1;
    LOGI(KDJX_LOG " forceClear");
}
