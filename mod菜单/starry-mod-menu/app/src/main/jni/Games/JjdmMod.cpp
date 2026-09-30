/* ================================================================
 *  境界刀鸣（com.blade.bd）— 星光菜单游戏模块
 *
 *  注入路线（真机已验证，见 10_docs/境界刀鸣/Lua玩法层注入方案_真机验证_20260922.md）：
 *    玩法逻辑在 Lua（libgame.so = Lua 5.4.6 + xLua），不在 C#。
 *    ① Dobby hook libgame.so 的 Lua VM 公开入口 lua_gettop / lua_pcall
 *       → 直接拿到 lua_State*，且天然运行在调用 Lua 的线程（游戏逻辑=主线程）
 *    ② 在该 hook 内用 libgame.so 的 C API（luaL_loadstring + lua_pcall）
 *       注入 monkey-patch，并写开关
 *    ※ 放弃 il2cpp 路线：本游戏 libil2cpp.so 的 dynsym 名↔址被 TP 打乱
 *      （readelf 标 il2cpp_class_from_name 的 0x257b278 实际是
 *        il2cpp_domain_get_assemblies），按名取 LuaManager 不可靠
 *
 *  补丁（实测生效）：
 *    伤害倍率 → 包 CombatFunc.DoSubHp，攻击方是 HERO/AID_HERO 时 damage * N
 *    无敌     → 受击方是 HERO/AID_HERO 时直接 return（不调原函数）
 *    无限技能 → 包 CombatFunc.EnterSkillCd，英雄 no-op（不进入冷却）
 *    实体查表：worldImpl:GetEntityImplByUid(uid) → ei.m_ObjData:GetEntityType()
 *    CTXDefine.ENTITY_TYPE：HERO=2 / AID_HERO=11 / MONSTER=3
 * ================================================================ */

#include "JjdmMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;
using Il2Cpp::IsValidHeapPtr;
using Il2Cpp::ReadPtr;
using Il2Cpp::SafeProbe;

#define JJDM_LOG "[Jjdm]"

// 目标库：il2cpp（取元数据/类型）与游戏 Lua 库（取 Lua C API）
#define JJDM_IL2CPP_LIB sec_lib_il2cpp()
#define JJDM_LUA_LIB "libgame.so"

// XLua.LuaEnv.rawL 在对象内的固定偏移（真机实测；前两个字段是 static）
#define JJDM_RAW_L_OFF 0x10

#define LUA_TTABLE 5

// ============================================================
// ① 开关 / 状态
// ============================================================
static std::atomic<bool> g_libReady{false};   // libil2cpp 已映射
static std::atomic<bool> g_luaReady{false};   // libgame.so 符号已解析
static std::atomic<bool> g_ready{false};      // LuaManager 已 hook
static std::atomic<bool> g_wantSync{true};    // 需要同步一次开关
static std::atomic<bool> g_forceOff{false};   // 强制关闭（清理）
static std::atomic<bool> g_inHook{false};     // 防重入

/* 唯一同步标志：菜单/滑块/Tick 置 true，Lua 侧写成功后置 false。
 * 曾经这里分裂成 g_wantSync(写) 与 g_needSync(读) 两个变量 ⇒ 首次同步后
 * g_needSync 永远为 false，菜单开关再也传不到 Lua（表现为三功能全部无效）。 */

static std::atomic<int>  g_dmgMult{1};        // 伤害倍率（1 = 关；默认必须为 1）
static std::atomic<bool> g_inv{false};        // 无敌
static std::atomic<bool> g_nocd{false};       // 无限技能

static bool   g_patched = false;              // Lua 补丁已装

// ============================================================
// ② Lua C API（从 libgame.so 取符号）
// ============================================================
struct lua_State;
typedef int (*luaL_loadstring_t)(lua_State*, const char*);
typedef int (*lua_pcall_t)(lua_State*, int, int, int);
typedef int (*lua_pcallk_t)(lua_State*, int, int, int, intptr_t, void*);
typedef const char* (*lua_tolstring_t)(lua_State*, int, size_t*);
typedef void (*lua_settop_t)(lua_State*, int);
typedef void (*lua_getglobal_t)(lua_State*, const char*);
typedef int (*lua_type_t)(lua_State*, int);
typedef int (*lua_gettop_t)(lua_State*);

static luaL_loadstring_t p_luaL_loadstring = nullptr;
static lua_pcall_t       p_lua_pcall = nullptr;
static lua_pcallk_t      p_lua_pcallk = nullptr;
static lua_tolstring_t   p_lua_tolstring = nullptr;
static lua_settop_t      p_lua_settop = nullptr;
static lua_getglobal_t   p_lua_getglobal = nullptr;
static lua_type_t        p_lua_type = nullptr;
static lua_gettop_t      p_lua_gettop = nullptr;

/* Dobby 蹦床：hook 之后必须走这里，否则无限递归 */
static lua_pcall_t       g_origPcall = nullptr;
static lua_pcallk_t      g_origPcallk = nullptr;
static lua_gettop_t      g_origGettop = nullptr;
static bool              g_hooksInstalled = false;
// (已删除) g_needSync —— 与 g_wantSync 重复，统一到 g_wantSync
static std::atomic<uint32_t> g_hookCalls{0};

/* ----------------------------------------------------------------
 * Dobby 在 arm64 上的 inline patch 宽 16 字节（ldr x17,#8 / br x17 / .quad）。
 * 若被 hook 的函数本体 < 16 字节，patch 会越过函数尾、破坏紧随其后的下一个
 * 函数入口。实测证据（libgame.so）：
 *     lua_pcall   @0x512920 size=12
 *     lua_tonumber@0x51292c size=8     <-- 前 4 字节被 patch 尾部覆盖
 * 结果：游戏每次调用 lua_tonumber 都执行到 patch 残留 → SIGILL(signal 4)，
 *       启动约 10s 静默死亡（被 SignalQuarantine 屏蔽了致命信号，logcat 无痕）。
 * 铁律：只 hook 已验证 size >= 16 的入口。
 *     lua_gettop @0x515e38 size=28   ✓
 *     lua_pcallk @0x518fec size=332  ✓   （Lua 5.4 里 lua_pcall 是宏，
 *                                          真正被调用的是 lua_pcallk）
 * ---------------------------------------------------------------- */
#define DOBBY_PATCH_BYTES 16

static bool LoadLuaApi() {
    if (p_luaL_loadstring && (p_lua_pcallk || p_lua_pcall)) return true;
    void* h = dlopen(JJDM_LUA_LIB, RTLD_NOW);
    if (!h) h = dlopen(JJDM_LUA_LIB, RTLD_NOLOAD | RTLD_NOW);
    if (!h) {
        static int s_dl = 0;
        if ((++s_dl % 20) == 1) {
            const char* e = dlerror();
            LOGI(JJDM_LOG " dlopen(%s) 未成功（第 %d 次）：%s", JJDM_LUA_LIB, s_dl, e ? e : "?");
        }
        return false;
    }
    p_luaL_loadstring = (luaL_loadstring_t)dlsym(h, "luaL_loadstring");
    p_lua_pcall       = (lua_pcall_t)dlsym(h, "lua_pcall");
    p_lua_pcallk      = (lua_pcallk_t)dlsym(h, "lua_pcallk");
    p_lua_tolstring   = (lua_tolstring_t)dlsym(h, "lua_tolstring");
    p_lua_settop      = (lua_settop_t)dlsym(h, "lua_settop");
    p_lua_getglobal   = (lua_getglobal_t)dlsym(h, "lua_getglobal");
    p_lua_type        = (lua_type_t)dlsym(h, "lua_type");
    p_lua_gettop      = (lua_gettop_t)dlsym(h, "lua_gettop");
    if (!p_luaL_loadstring || (!p_lua_pcallk && !p_lua_pcall) ||
        !p_lua_tolstring || !p_lua_settop) {
        LOGE(JJDM_LOG " libgame lua symbols missing");
        return false;
    }
    g_luaReady.store(true);
    LOGI(JJDM_LOG " lua C api ready (%s) pcallk=%p gettop=%p",
         JJDM_LUA_LIB, (void*)p_lua_pcallk, (void*)p_lua_gettop);
    return true;
}

/* 统一走 pcallk（Lua 5.4 实际入口）；只有拿不到 pcallk 时才退回 12 字节的 pcall 薄壳 */
static inline int CallPcall(lua_State* L) {
    if (g_origPcallk) return g_origPcallk(L, 0, 0, 0, 0, nullptr);
    if (p_lua_pcallk) return p_lua_pcallk(L, 0, 0, 0, 0, nullptr);
    return (g_origPcall ? g_origPcall : p_lua_pcall)(L, 0, 0, 0);
}

/* 出错时打日志要限流：hook 每秒触发上千次，失败会瞬间刷爆 logcat
 * （chatty 会把我们真正关心的行挤掉） */
static void LogErrThrottled(const char* what, const char* err) {
    static uint64_t s_lastMs = 0;
    static unsigned s_suppressed = 0;
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    const uint64_t nowMs = (uint64_t)ts.tv_sec * 1000ull + (uint64_t)ts.tv_nsec / 1000000ull;
    if (s_lastMs != 0 && (nowMs - s_lastMs) < 2000) { ++s_suppressed; return; }
    if (s_suppressed) {
        LOGE(JJDM_LOG " %s: %s (+%u 条同类已抑制)", what, err ? err : "?", s_suppressed);
        s_suppressed = 0;
    } else {
        LOGE(JJDM_LOG " %s: %s", what, err ? err : "?");
    }
    s_lastMs = nowMs;
}

static bool RunLua(lua_State* L, const char* chunk) {
    if (p_luaL_loadstring(L, chunk) != 0) {
        const char* err = p_lua_tolstring ? p_lua_tolstring(L, -1, nullptr) : "load err";
        LogErrThrottled("loadstring", err);
        if (p_lua_settop) p_lua_settop(L, -2);
        return false;
    }
    if (CallPcall(L) != 0) {
        const char* err = p_lua_tolstring ? p_lua_tolstring(L, -1, nullptr) : "pcall err";
        LogErrThrottled("pcall", err);
        if (p_lua_settop) p_lua_settop(L, -2);
        return false;
    }
    return true;
}

static bool IsTableGlobal(lua_State* L, const char* name) {
    if (!p_lua_getglobal || !p_lua_type || !p_lua_settop) return false;
    p_lua_getglobal(L, name);
    int t = p_lua_type(L, -1);
    p_lua_settop(L, -2);
    return t == LUA_TTABLE;
}

// ============================================================
// ③ Lua 补丁（真机验证版本；幂等）
// ============================================================
static const char kInstallScript[] = R"LUA(
if type(CombatFunc) ~= 'table' or type(CTXDefine) ~= 'table' then return end
local CF, ET = CombatFunc, CTXDefine.ENTITY_TYPE
local W = __jjdm_wrap
if not W then W = {}; __jjdm_wrap = W end
-- Lua 的函数不能挂字段（写 CF.DoSubHp.__jjdm 会 "attempt to index a function value"），
-- 所以「包装是否还在」只能靠身份比对：W.sub 记住我们装上去的那个函数
if type(CF.DoSubHp) == 'function' and W.cf == CF and W.sub == CF.DoSubHp then return end
local pcall, type = pcall, type
JJDM = JJDM or {}
JJDM.v, JJDM.ok = 2, nil
JJDM.re = (JJDM.re or 0) + 1
JJDM.dmg = JJDM.dmg or 1
JJDM.inv = JJDM.inv or false
JJDM.nocd = JJDM.nocd or false
JJDM.hit, JJDM.block, JJDM.cd = JJDM.hit or 0, JJDM.block or 0, JJDM.cd or 0
local hero = {}
if ET.HERO then hero[ET.HERO] = true end
if ET.AID_HERO then hero[ET.AID_HERO] = true end
local function etype(w, uid)
  if uid == nil then return nil end
  local ok, ei = pcall(function() return w:GetEntityImplByUid(uid) end)
  if not ok or ei == nil then return nil end
  local ok2, od = pcall(function() return ei.m_ObjData end)
  if not ok2 or od == nil then return nil end
  local ok3, et = pcall(function() return od:GetEntityType() end)
  if not ok3 then return nil end
  return et
end
local function isHero(w, uid)
  local et = etype(w, uid)
  return et ~= nil and hero[et] == true
end
local o_sub = CF.DoSubHp
function CF:DoSubHp(worldImpl, uid, damage, attackUid, skillId, damageId)
  local J = JJDM
  if J.inv and isHero(worldImpl, uid) then J.block = J.block + 1 return end
  local m = J.dmg
  if m and m ~= 1 and isHero(worldImpl, attackUid) then
    damage = damage * m
    J.hit = J.hit + 1
  end
  return o_sub(self, worldImpl, uid, damage, attackUid, skillId, damageId)
end
W.cf, W.sub = CF, CF.DoSubHp
local o_enter = CF.EnterSkillCd
if type(o_enter) == 'function' then
  function CF:EnterSkillCd(worldImpl, uid, ...)
    if JJDM.nocd and isHero(worldImpl, uid) then JJDM.cd = JJDM.cd + 1 return end
    return o_enter(self, worldImpl, uid, ...)
  end
  W.enter = CF.EnterSkillCd
end
JJDM.ok = true
)LUA";

// ============================================================
// ⑤ 主线程同步：直接 hook libgame.so 的 Lua VM 公开入口
//    本游戏 libil2cpp.so 的 dynsym 名↔址被 TP 打乱（实测 0x257b278 槽位
//    readelf 标 il2cpp_class_from_name，实际是 il2cpp_domain_get_assemblies，
//    (domain, size_t*) -> const Il2CppAssembly**）⇒ 按名取
//    GameFramework.LuaManager 不可靠。改为在 Lua VM 入口上抓 lua_State*：
//    既拿到 state，又天然运行在调用 Lua 的那个线程（游戏逻辑 = 主线程）。
// ============================================================

/* 回读 JJDM 表的真实状态 + 补丁是否仍然挂在 CombatFunc 上（限流打日志）。
 * wrap=no 表示游戏把 CombatFunc（或其 DoSubHp）换掉了 ⇒ 我们的包装被顶掉。
 * hit/block/cd 是包装内部计数器：全 0 = 游戏根本没走 CombatFunc.DoSubHp。 */
static void VerifyState(lua_State* L) {
    static const char kProbe[] =
        "if JJDM then"
        " local w='no'"
        " if __jjdm_wrap and type(CombatFunc)=='table'"
        " and __jjdm_wrap.sub == CombatFunc.DoSubHp then w='yes' end"
        " _G.__jjdm_state = string.format('v=%s ok=%s re=%s dmg=%s inv=%s nocd=%s wrap=%s hit=%s block=%s cd=%s',"
        "tostring(JJDM.v),tostring(JJDM.ok),tostring(JJDM.re),tostring(JJDM.dmg),tostring(JJDM.inv),"
        "tostring(JJDM.nocd),w,tostring(JJDM.hit),tostring(JJDM.block),tostring(JJDM.cd)) end";
    if (!RunLua(L, kProbe)) return;
    if (!p_lua_getglobal || !p_lua_tolstring) return;
    p_lua_getglobal(L, "__jjdm_state");
    const char* s = p_lua_tolstring(L, -1, nullptr);
    static char s_last[160] = {0};
    static uint64_t s_lastMs = 0;
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    const uint64_t nowMs = (uint64_t)ts.tv_sec * 1000ull + (uint64_t)ts.tv_nsec / 1000000ull;
    if (s && strcmp(s, s_last) != 0 && (nowMs - s_lastMs) >= 400) {
        snprintf(s_last, sizeof(s_last), "%s", s);
        s_lastMs = nowMs;
        LOGI(JJDM_LOG " state[%p] %s", (void*)L, s);
    }
    if (p_lua_settop) p_lua_settop(L, -2);
}

static void SyncOnLua(lua_State* L) {
    if (!g_luaReady.load() || !L) return;
    if (!IsTableGlobal(L, "CombatFunc")) return;   // Lua 环境未就绪，下次再试
    /* ① 守卫式安装：每次同步都跑。游戏换场景/换战斗时若重建了 CombatFunc 表，
     *    我们的包装会随之丢失，这里自动重新包装（一次性安装的写法会静默失效）。
     *    ★ 必须独立成 chunk：kInstallScript 命中守卫时会提前 return，
     *      把它和「写开关」拼在同一个 chunk 里会把写开关整段吃掉（真机踩过：
     *      patched=1 / wrap=yes，但开关值永远传不到 Lua）。 */
    if (!RunLua(L, kInstallScript)) return;
    /* ② 写开关（独立 chunk，绝不能被 ① 的 return 吃掉） */
    const bool off = g_forceOff.load();
    const int dmg = off ? 1 : g_dmgMult.load();
    const bool inv = !off && g_inv.load();
    const bool nocd = !off && g_nocd.load();
    char buf[128];
    snprintf(buf, sizeof(buf),
             "if JJDM then JJDM.dmg=%d JJDM.inv=%s JJDM.nocd=%s end",
             dmg, inv ? "true" : "false", nocd ? "true" : "false");
    if (!RunLua(L, buf)) return;
    if (!g_patched) {
        g_patched = true;
        LOGI(JJDM_LOG " lua patch installed (state=%p)", (void*)L);
    }
    /* 限流证明「写开关」这段真的在执行（否则只能靠 state 变化间接推断） */
    {
        static uint64_t s_lastMs = 0;
        struct timespec ts;
        clock_gettime(CLOCK_MONOTONIC, &ts);
        const uint64_t nowMs = (uint64_t)ts.tv_sec * 1000ull + (uint64_t)ts.tv_nsec / 1000000ull;
        if ((nowMs - s_lastMs) >= 2000) {
            s_lastMs = nowMs;
            LOGI(JJDM_LOG " sync dmg=%d inv=%d nocd=%d", dmg, (int)inv, (int)nocd);
        }
    }
    g_wantSync.store(false, std::memory_order_relaxed);
    VerifyState(L);
}

/* 所有 Lua VM 入口共用：进入 hook 时 VM 处于稳定边界，可安全嵌套一次 pcall */
static inline void SyncFromLuaEntry(lua_State* L) {
    if (g_inHook.exchange(true, std::memory_order_acq_rel)) return;
    /* 记录见过的 lua_State：游戏可能有多套 VM（战斗 VM ≠ 加载 VM），
     * 那样补丁装在 A 上、战斗跑在 B 上，表现为「补丁装上了但没效果」 */
    static void* s_states[8] = {0};
    static int s_nstates = 0;
    bool known = false;
    for (int i = 0; i < s_nstates; ++i) if (s_states[i] == (void*)L) { known = true; break; }
    if (!known && s_nstates < 8) {
        s_states[s_nstates++] = (void*)L;
        LOGI(JJDM_LOG " lua state #%d = %p", s_nstates, (void*)L);
    }
    const uint32_t c = g_hookCalls.fetch_add(1, std::memory_order_relaxed);
    if (c == 0) LOGI(JJDM_LOG " lua hook 首次触发 state=%p", (void*)L);
    else if ((c % 2000) == 0) LOGI(JJDM_LOG " lua hook calls=%u patched=%d nstates=%d", c, (int)g_patched, s_nstates);
    bool want = g_wantSync.load(std::memory_order_relaxed);
    if (!g_patched && c >= 20) want = true;   // 预热 20 次后再尝试装补丁
    if (want) SyncOnLua(L);
    g_inHook.store(false, std::memory_order_release);
}

static int Hook_lua_gettop(lua_State* L) {
    SyncFromLuaEntry(L);
    return g_origGettop ? g_origGettop(L) : 0;
}

static int Hook_lua_pcallk(lua_State* L, int nargs, int nres, int errfunc,
                           intptr_t ctx, void* k) {
    SyncFromLuaEntry(L);
    return (g_origPcallk ? g_origPcallk : p_lua_pcallk)(L, nargs, nres, errfunc, ctx, k);
}

static bool InstallLuaHooks() {
    if (g_hooksInstalled) return true;

    /* 只 hook 本体 >= DOBBY_PATCH_BYTES 的入口（见文件头注释：
     * 12 字节的 lua_pcall 被 patch 会踩坏紧随其后的 lua_tonumber → SIGILL） */
    if (p_lua_gettop &&
        DobbyHook((void*)p_lua_gettop, (void*)Hook_lua_gettop, (void**)&g_origGettop) == 0)
        LOGI(JJDM_LOG " hooked lua_gettop @ %p", (void*)p_lua_gettop);
    else
        LOGE(JJDM_LOG " hook lua_gettop 失败");

    if (p_lua_pcallk &&
        DobbyHook((void*)p_lua_pcallk, (void*)Hook_lua_pcallk, (void**)&g_origPcallk) == 0)
        LOGI(JJDM_LOG " hooked lua_pcallk @ %p", (void*)p_lua_pcallk);
    else
        LOGE(JJDM_LOG " hook lua_pcallk 失败");

    if (!g_origPcallk && !g_origGettop) {
        LOGE(JJDM_LOG " lua hook 全部失败");
        return false;
    }
    g_hooksInstalled = true;
    g_ready.store(true);
    g_wantSync.store(true);
    LOGI(JJDM_LOG " init ok (lua hook 路线，零 il2cpp 依赖)");
    return true;
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约）
// ============================================================
bool Jjdm_TryInit(JavaVM* /*vm*/) {
    if (g_ready.load()) return true;
    if (!LoadLuaApi()) return false;   // libgame.so 未起：交给重试 / Tick 懒加载
    return InstallLuaHooks();
}

bool Jjdm_IsReady() { return g_ready.load(); }

void Jjdm_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case JJDM_FEAT_INV:  g_inv.store(enabled);  break;
        case JJDM_FEAT_NOCD: g_nocd.store(enabled); break;
        case JJDM_FEAT_DMG:  break;  // 滑块：值只由 OnValueChanged/Tick 提供，禁止在此改写
        default: break;
    }
    LOGI(JJDM_LOG " feature #%d -> %d", featNum, (int)enabled);
    g_wantSync.store(true);
}

void Jjdm_OnValueChanged(int featNum, int value) {
    if (featNum == JJDM_FEAT_DMG) {
        if (value < 1) value = 1;
        if (value > 100) value = 100;
        g_dmgMult.store(value);
        LOGI(JJDM_LOG " dmg mult = %d", value);
        g_wantSync.store(true);
    }
}

void Jjdm_Tick(const bool* features, const int* values) {
    // initRetryMax=0 → 框架不会调 tryInit（"Tick 懒加载"合同），必须由 Tick 自己补
    if (!g_ready.load()) {
        static int s_lazy = 0;
        if ((s_lazy++ % 2) == 0) Jjdm_TryInit(nullptr);  // 每 ~400ms 试一次
    }
    // 只做开关同步；禁止扫堆/遍历对象
    if (features) {
        g_inv.store(features[JJDM_FEAT_INV]);
        g_nocd.store(features[JJDM_FEAT_NOCD]);
    }
    if (values) {
        int v = values[JJDM_FEAT_DMG];
        if (v >= 1 && v <= 100) g_dmgMult.store(v);
    }
    if (!g_libReady.load() && isLibraryLoaded(JJDM_IL2CPP_LIB)) g_libReady.store(true);
    g_wantSync.store(true);
}

void Jjdm_ForceClear() {
    g_forceOff.store(true);
    g_dmgMult.store(1);
    g_inv.store(false);
    g_nocd.store(false);
    g_wantSync.store(true);
}
