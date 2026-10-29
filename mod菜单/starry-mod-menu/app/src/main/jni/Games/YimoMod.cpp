/* ================================================================
 *  伊莫（com.x.yimoo.official 1.0.1 / vc4）— 星光菜单游戏模块
 *
 *  引擎：Unity IL2CPP + xLua。玩法逻辑 100% 在 Lua（C# 侧只是宿主），
 *        所以【不碰 IL2CPP】，全部改动落在 Lua 层。
 *
 *  注入路线（与 JjdmMod / Match2Mod 同型，真机已验证）：
 *    ① dlopen("libxlua.so") + dlsym 取 Lua C API
 *       —— 不抄斌哥壳的硬编码偏移（base+0x010a41a8 等）。本作 libxlua.so
 *          未 strip，dynsym 齐全，dlsym 更稳、换版本不会静默错位。
 *    ② Dobby hook lua_pcall / lua_gettop → 拿到 lua_State*，
 *       且天然运行在【调用 Lua 的那个线程】= 游戏主线程（不用碰 IL2CPP 找 LuaManager）。
 *    ③ 在该 hook 内用 luaL_loadstring + lua_pcall 注入补丁，
 *       并用同一通道把菜单开关/滑块值写进 Lua 全局 YM。
 *
 *  ☠ Dobby 在 arm64 上 inline patch 宽 16 字节 ⇒ 只 hook 本体 >= 16 字节的入口。
 *     实测 libxlua.so（nm -D -S）：
 *       lua_gettop  0x1090be0 size=16  ✓（恰好 16，下一符号 lua_settop 在 +16，不越界）
 *       lua_pcall   0x1096dac size=204 ✓
 *       lua_setglobal 0x9a8ed4 size=12 ✗ —— 12 字节，绝不能 hook（Jjdm 踩过 SIGILL）
 *     本文件只 hook lua_pcall + lua_gettop；lua_setglobal / lua_pushboolean 只调用。
 *
 *  抓点依据：10_docs/伊莫/斌哥hook锚点_20260930.json
 *           10_docs/伊莫/抓点总结_斌哥mod_20260930.md
 * ================================================================ */

#include "YimoMod.h"

#include <atomic>
#include <thread>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "ModDiag.h"      // 诊断脚手架（正式包编译期清空，见文件头说明 P0-1）
#include "ModDiagLua.h"   // Lua 侧诊断 chunk 模板（正式包 MOD_DIAG_LUA_TEXT == ""）

#define YIMO_LOG "[Yimo]"

// 目标 Lua 库（本作 xLua 的 C 实现；不是 libil2cpp.so）
#define YIMO_LUA_LIB "libxlua.so"

#define LUA_TTABLE 5

// ============================================================
// ① 开关 / 状态
// ============================================================
static std::atomic<bool> g_luaReady{false};   // libxlua.so 符号已解析
static std::atomic<bool> g_ready{false};      // Lua VM 入口已 hook
static std::atomic<bool> g_wantSync{true};    // 需要同步一次开关
static std::atomic<bool> g_forceOff{false};   // 强制关闭（鉴权失败清理）
static std::atomic<bool> g_inHook{false};     // 防重入

/* 唯一同步标志：菜单/滑块/Tick 置 true，Lua 侧写成功后置 false。
 * （Jjdm 的教训：曾经分裂成 g_wantSync / g_needSync 两个变量，
 *   首次同步后读侧永远 false，菜单开关再也传不到 Lua。） */
/* ① 精灵攻击秒杀：命中上报的【重发段数】。
 *   斌哥 MOD.dmgMul = 100（他自己的默认档，实机验证有效）；1 = 关闭。
 *   ☠上限受 Lua 侧 MAX_RESEND_PER_SEC = 500（每秒重发预算）约束，
 *     别再无脑加大：挤爆预算反而会饿死精灵那条真正有效的 DoActOnActorActions。 */
static constexpr int kKillResend = 100;
static std::atomic<int>  g_dmgMult{1};        // 重发段数（1 = 关闭；默认必须为 1）
static std::atomic<int>  g_spdMult{1};        // 移速倍率（1 = 关闭）
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_jump{false};
static std::atomic<bool> g_stam{false};
static std::atomic<bool> g_tp{false};

static bool g_patched = false;                // Lua 补丁已装

// ============================================================
// ② Lua C API（从 libxlua.so 取符号）
// ============================================================
struct lua_State;
typedef int (*luaL_loadstring_t)(lua_State*, const char*);
typedef int (*lua_pcall_t)(lua_State*, int, int, int);
typedef const char* (*lua_tolstring_t)(lua_State*, int, size_t*);
typedef void (*lua_settop_t)(lua_State*, int);
typedef void (*lua_getglobal_t)(lua_State*, const char*);
typedef void (*lua_setglobal_t)(lua_State*, const char*);
typedef void (*lua_pushboolean_t)(lua_State*, int);
typedef int (*lua_type_t)(lua_State*, int);
typedef int (*lua_gettop_t)(lua_State*);

static luaL_loadstring_t p_luaL_loadstring = nullptr;
static lua_pcall_t       p_lua_pcall = nullptr;
static lua_tolstring_t   p_lua_tolstring = nullptr;
static lua_settop_t      p_lua_settop = nullptr;
static lua_getglobal_t   p_lua_getglobal = nullptr;
static lua_setglobal_t   p_lua_setglobal = nullptr;
static lua_pushboolean_t p_lua_pushboolean = nullptr;
static lua_type_t        p_lua_type = nullptr;
static lua_gettop_t      p_lua_gettop = nullptr;

/* Dobby 蹦床：hook 之后必须走这里，否则无限递归 */
static lua_pcall_t       g_origPcall = nullptr;
static lua_gettop_t      g_origGettop = nullptr;
static bool              g_hooksInstalled = false;
static std::atomic<uint32_t> g_hookCalls{0};

/* ---------------------------------------------------------------
 * ★多 lua_State 跟踪（2026-10-29 伊莫 1.1.0 实测：菜单 ON 但游戏里不生效 /
 *   无敌一直开着、菜单关不掉）
 *
 * 老实现只有一个**全局一次性**标志 g_wantSync：谁先触发 hook 谁把它消费掉，
 * 于是「写开关」只在那个 state 上跑一次。游戏若有多个 lua_State（加载 VM ≠ 战斗 VM），
 * 战斗 VM 里的 YM 就永远停在首次写入的旧值 —— 菜单看着能点、游戏里不变。
 *
 * 现在：每个 state 记「已应用序号」，开关一变 g_syncSeq++，各 state 下次进 hook
 * 时各自补写一遍，与有几个 VM、谁先触发无关。
 * --------------------------------------------------------------- */
static constexpr int kMaxStates = 8;
static lua_State* g_states[kMaxStates] = {nullptr};
static uint32_t   g_stateApplied[kMaxStates] = {0};
static std::atomic<int> g_stateCount{0};
static std::atomic<uint32_t> g_syncSeq{1};

static int StateSlot(lua_State* L) {
    const int n = g_stateCount.load(std::memory_order_acquire);
    for (int i = 0; i < n; ++i) {
        if (g_states[i] == L) return i;
    }
    if (n >= kMaxStates) return -1;
    g_states[n] = L;
    g_stateApplied[n] = 0;
    g_stateCount.store(n + 1, std::memory_order_release);
    DLOG("state registered slot=%d state=%p", n, (void*)L);
    return n;
}

/* 开关/数值变了：序号 +1，并要求各 state 补写 */
static inline void BumpSync() {
    g_syncSeq.fetch_add(1, std::memory_order_relaxed);
    g_wantSync.store(true, std::memory_order_relaxed);
}

/* ----------------------------------------------------------------
 * 序言指纹（本作 libxlua.so 实测；只在 HookAt 前做一次校验，
 * 不匹配 = 换了包/换了版本 → 拒绝挂，宁可没有功能也不挂错地址闪退）
 *   lua_gettop 0x1090be0: a9422009 cb090108 d343fd00 d65f03c0
 *   lua_pcall  0x1096dac: f81e0ffe a9014ff4 f9400813 34000143
 * ---------------------------------------------------------------- */
static const uint32_t kGetTopFp[4] = {0xa9422009u, 0xcb090108u, 0xd343fd00u, 0xd65f03c0u};
static const uint32_t kPcallFp[4]  = {0xf81e0ffeu, 0xa9014ff4u, 0xf9400813u, 0x34000143u};

static bool FingerprintOk(void* fn, const uint32_t fp[4]) {
    if (!fn) return false;
    uint32_t got[4] = {0, 0, 0, 0};
    memcpy(got, fn, sizeof(got));
    return got[0] == fp[0] && got[1] == fp[1] && got[2] == fp[2] && got[3] == fp[3];
}

static bool LoadLuaApi() {
    if (p_luaL_loadstring && p_lua_pcall) return true;
    void* h = dlopen(YIMO_LUA_LIB, RTLD_NOW);
    if (!h) h = dlopen(YIMO_LUA_LIB, RTLD_NOLOAD | RTLD_NOW);
    if (!h) {
        /* 正式包静默（DLOG 编译期清空）；诊断包才出日志。
         * ★这里原本完全静默，2026-10-29 伊莫 1.1.0 排查时「什么都没打」无法区分
         *   「dlopen 失败」与「pg 未就绪」，故加诊断通道（release 零影响）。 */
        DLOG("dlopen(%s) failed", YIMO_LUA_LIB);
        static int s_dlFail = 0;
        if (++s_dlFail == 1) LOGW(YIMO_LOG " dlopen libxlua 失败（hooks 未装）");
        return false;
    }
    DLOG("dlopen(%s) ok handle=%p", YIMO_LUA_LIB, h);
    p_luaL_loadstring = (luaL_loadstring_t)dlsym(h, "luaL_loadstring");
    p_lua_pcall       = (lua_pcall_t)dlsym(h, "lua_pcall");
    p_lua_tolstring   = (lua_tolstring_t)dlsym(h, "lua_tolstring");
    p_lua_settop      = (lua_settop_t)dlsym(h, "lua_settop");
    p_lua_getglobal   = (lua_getglobal_t)dlsym(h, "lua_getglobal");
    p_lua_setglobal   = (lua_setglobal_t)dlsym(h, "lua_setglobal");
    p_lua_pushboolean = (lua_pushboolean_t)dlsym(h, "lua_pushboolean");
    p_lua_type        = (lua_type_t)dlsym(h, "lua_type");
    p_lua_gettop      = (lua_gettop_t)dlsym(h, "lua_gettop");

    if (!p_luaL_loadstring || !p_lua_pcall || !p_lua_tolstring || !p_lua_settop) {
        LOGE(YIMO_LOG " libxlua 符号缺失 loadstring=%p pcall=%p tolstring=%p settop=%p",
             (void*)p_luaL_loadstring, (void*)p_lua_pcall,
             (void*)p_lua_tolstring, (void*)p_lua_settop);
        return false;
    }
    DLOG("lua api ok loadstring=%p pcall=%p gettop=%p getglobal=%p type=%p",
         (void*)p_luaL_loadstring, (void*)p_lua_pcall, (void*)p_lua_gettop,
         (void*)p_lua_getglobal, (void*)p_lua_type);
    g_luaReady.store(true);
    return true;
}

/* 统一走 orig 蹦床；拿不到才退回被 hook 的导出符号（此时 g_inHook 会挡住递归） */
static inline int CallPcall(lua_State* L) {
    if (g_origPcall) return g_origPcall(L, 0, 0, 0);
    return p_lua_pcall(L, 0, 0, 0);
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
        LOGE(YIMO_LOG " %s: %s (+%u 条同类已抑制)", what, err ? err : "?", s_suppressed);
        s_suppressed = 0;
    } else {
        LOGE(YIMO_LOG " %s: %s", what, err ? err : "?");
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

// ============================================================
// ③ Lua 补丁
// ============================================================
/* 说明（抓点来自斌哥包逆向，见 10_docs/伊莫/抓点总结_斌哥mod_20260930.md）：
 *   开关表 YM 由 native 侧写入（只改字段，绝不替换表 —— 本脚本只捕获一次引用）。
 *   ① 精灵攻击秒杀（原「伤害倍率」滑块，2026-10 改开关直实现）：包
 *      ClientMainPlayer/ClientPet/ClientPuppet/ClientCreation 的
 *      serverMsg，命中 DMG_OUT 白名单时把上报重发 (N-1) 次（开 = 重发 100 段）。
 *      依据：伤害值 100% 服务器算（客户端改本地数值是死码），
 *            而命中判定 100% 客户端算并上报 ⇒ 多报一次 = 服务器多算一次。
 *      ☠ 对人物（ClientMainPlayer）几乎无效：它只发 RPC_CS_ProjectileHitTarget，
 *         首参是投射物实例 id、命中后紧接 ProjectileDestroy ⇒「同颗弹只认第一次」；
 *         真正吃到倍率的是精灵（ClientPet 的 RPC_CS_DoActOnActorActions）。
 *         所以这一项的正名就是「精灵攻击秒杀」。
 *      ☠ 只包这 4 个类；其余实体类不上包装（斌哥全量包装导致过「技能百倍失效」）。
 *      ☠ 安全阀：每秒最多 500 个重发包（否则 64 段投射物 × 100 倍 = 上万包打死连接）。
 *   ② 移速：ActorCombatAttribute:getAttribRatioValue，attributeId ==
 *      speed_ratio_load_adjustment_v 时乘 N。
 *      依据：该 id 在 11 个 *SpeedRatio + 6 个绝对速度 getter 的公式里各出现恰好一次
 *            ⇒ 乘一次 = 精确 xN，覆盖走/跑/冲刺/游泳/滑翔/闪避/蹲伏。
 *      ☠ 不要按 run_v/dash_v 做白名单（宠物冲刺会叠成 xN²）。
 *   ③ 无敌：5 条独立通道（不是一个咽喉能包住的）——
 *      a Utils.checkValidTarget → false；b dialoguePauseInvincibleEndTs = 4e12；
 *      c ClientSpace.checkImmuneFallDamage → true；d HURT_IN 按受害者 id 丢上报；
 *      e VICTIM_REPORT 按发送者丢上报（陷阱/荆棘/导电是第三条独立自伤通道）。
 *      ☠ RPC_CS_OnPuppetThornsCollision 只能放 HURT_IN（结构反过来，按名丢会让怪免疫荆棘）；
 *      ☠ RPC_CS_OnCSProjectileHit 不能整条丢（会静默废掉抓宠）。
 *   ④ 无限跳跃：PawnController:performJump（全库唯一跳跃出口）+ PlayerController 同名方法。
 *      ☠ 不硬改 canJump（会吃掉 StopFly）；☠ 不用 forceUngrounded（锁存标志，会永久不着地）。
 *   ⑤ 无限体力：ActorCombatAttribute:changeStamina，value < 0 时拦截（放行恢复）。
 *      本作唯一零 RPC 的纯客户端数值 ⇒ 五个功能里唯一零服务器风险。
 *   ⑥ 瞬移采集：在 performJump 内、无限跳之前接管；走游戏自己的
 *      forbidPositionCheck → forceSetPosRot 合法传送流程。
 *      ★距离闸门：服务器对强搬人有真实距离上限（超限位置校验失败、人被弹回原点），
 *        所以超限不瞬移、落回普通跳跃，并由 1 秒 tick 复查落点自适应调上限。
 */
static const char kYimoPatch[] = R"LUA(
-- ================= 星空·伊莫 补丁 v1 =================
local _rg = rawget
if _rg(_G, "__YM_V") then return end

local function L(n) return package.loaded[n] end

local pg = _rg(_G, "pg")
if type(pg) ~= "table" or pg.me == nil or pg.global == nil then return end

local ACA   = L("Common.Ability.Attribute.ActorCombatAttribute")
local ATTR  = L("Common.Const.AttributeConst")
local CONST = L("Common.Const.Const")
local PC    = L("GameApp.Controller.PawnController")
local IC    = L("GameApp.Input.InputCommand")
local CMP   = L("Entities.SpaceEntities.ClientMainPlayer")
if not (ACA and ATTR and CONST and PC and IC and CMP) then return end

-- ---------- 开关表（native 只改字段，不替换表）----------
local YM = _rg(_G, "YM")
if type(YM) ~= "table" then YM = {}; rawset(_G, "YM", YM) end
if type(YM.dmg) ~= "number" then YM.dmg = 1 end
if type(YM.spd) ~= "number" then YM.spd = 1 end
YM.inv  = YM.inv  == true
YM.jump = YM.jump == true
YM.stam = YM.stam == true
YM.tp   = YM.tp   == true

local function isMine(e)
  if type(e) ~= "table" then return false end
  if e.isMainPlayer == true or e.isMainPet == true then return true end
  if e == pg.me or e == pg.pawn then return true end
  return false
end

local function isMyActorId(id)
  if type(id) ~= "number" then return false end
  if pg.me and pg.me.actorId == id then return true end
  if pg.pawn and pg.pawn.actorId == id then return true end
  if pg.me and pg.me.getCurPetEntity then
    local ok, pet = pcall(pg.me.getCurPetEntity, pg.me)
    if ok and type(pet) == "table" and pet.actorId == id then return true end
  end
  return false
end

-- 发送者判定：投射物/子弹的 self 不是实体本体，要顺着 owner/srcEntity 找
local function senderIsMine(s)
  if isMine(s) then return true end
  if type(s) ~= "table" then return false end
  local o = _rg(s, "owner") or _rg(s, "ownerEntity") or _rg(s, "caster")
              or _rg(s, "srcEntity") or _rg(s, "entity")
  if isMine(o) then return true end
  local oid = _rg(s, "ownerActorId") or _rg(s, "srcActorId")
  if oid ~= nil and isMyActorId(oid) then return true end
  return false
end

-- LiteClass / 普通模块表：直接赋值
local function patchLite(cls, key, wrap)
  if type(cls) ~= "table" then return nil end
  local orig = cls[key]
  if type(orig) ~= "function" then return nil end
  local wrapped = wrap(orig)
  local ok = pcall(function() cls[key] = wrapped end)
  return ok and wrapped or nil
end

-- Class.Class 实体类：优先写 vtbl；☠类表 rawget 可能被 Config 的 RPC 参数表遮蔽
local function patchEntityClass(cls, key, wrap)
  if type(cls) ~= "table" then return false end
  local orig = cls[key]
  if type(orig) ~= "function" and type(cls.getVtbl) == "function" then
    local okV, vt = pcall(cls.getVtbl)
    if okV and type(vt) == "table" and type(vt[key]) == "function" then orig = vt[key] end
  end
  if type(orig) ~= "function" then return false end
  local wrapped = wrap(orig)
  local ok
  if type(cls.getVtbl) == "function" then
    ok = pcall(function() cls.getVtbl()[key] = wrapped end)
  end
  if not ok then
    if type(cls.forceAttrRepeat) == "function" then pcall(cls.forceAttrRepeat, cls, key) end
    ok = pcall(function() cls[key] = wrapped end)
  end
  return ok
end

---------------------------------------------------------------------
-- ① 精灵攻击秒杀：重发命中上报（菜单名；原「伤害倍率」滑块）
---------------------------------------------------------------------
local DMG_OUT = {
  RPC_CS_DoActOnActorActions = true,
  RPC_CS_DoActOnSweepTargetsActions = true,
  RPC_CS_OnAbilityCollisionHitTarget = true,
  -- RPC_CS_ProjectileHitTarget：斌哥 dead_end 已实测首参是投射物实例 id、命中后
  --   紧接 ProjectileDestroy（同 id）⇒「同颗弹重发只认第一次」，对人物基本无效。
  --   正式包仍保留在表内（＝回到斌哥原始设计）：它对**精灵**的弹幕仍有实际贡献，
  --   且 ①-b 已移除，不存在「多段命中 × 重发」把 MAX_RESEND_PER_SEC 挤爆的问题。
  RPC_CS_ProjectileHitTarget = true,
}

-- 按【被打者 actorId】过滤（value = 从变参取受害者 id 的函数；false = 无条件丢）
local HURT_IN = {
  RPC_CS_NotifyActOnActor = function(a4) return a4 and a4.actorId end,
  RPC_CS_NotifyActOnSweepTargets = function(a4) return a4 and (a4.actorId or a4.hitActorId) end,
  RPC_CS_OnCSProjectileHit = function(a3) return a3 end,
  RPC_CS_NotifyProjHit = function(a5) return a5 and a5.hitActorId end,
  RPC_CS_SpecialDamage = false,
  -- ☠只放 HURT_IN：结构反过来，发送者是 pg.me、受害者是 a1 对应的怪
  RPC_CS_OnPuppetThornsCollision = function(a1) return a1 end,
}

-- 按【发送者】丢：陷阱/荆棘/导电是第三条完全独立的自伤通道，不过 checkValidTarget
local VICTIM_REPORT = {
  RPC_CS_SetHitActionTimeline = true,
  RPC_CS_OnThornsCollision = true,
  RPC_CS_OnConductHit = true,
}

local MAX_RESEND_PER_SEC = 500
local _castResend = 0

local function wrapServerMsg(orig)
  return function(self, name, ...)
    if type(name) ~= "string" then return orig(self, name, ...) end

    if name == "RPC_CS_CastAbilityOnTarget" and isMine(self) then _castResend = 0 end

    if YM.inv then
      if VICTIM_REPORT[name] and isMine(self) then return end
      local pick = HURT_IN[name]
      if pick == false then return end
      if type(pick) == "function" then
        local ok, victim = pcall(pick, ...)
        if ok and isMyActorId(victim) then return end
      end
    end

    -- 诊断计数（仅 YM.diag 为真；正式包该键恒 nil ⇒ 不执行、无副作用）
    if YM.diag then
      YM.calls = (YM.calls or 0) + 1
      if DMG_OUT[name] then YM.callsOut = (YM.callsOut or 0) + 1 end
      if senderIsMine(self) then YM.callsMine = (YM.callsMine or 0) + 1 end
      local nm = YM.names
      if type(nm) ~= "table" then nm = {}; YM.names = nm end
      local k = tostring(name)
      nm[k] = (nm[k] or 0) + 1
    end

    local m = YM.dmg
    if type(m) == "number" and m > 1 and DMG_OUT[name] and senderIsMine(self) then
      local n = math.floor(m) - 1
      for _ = 1, n do
        if _castResend >= MAX_RESEND_PER_SEC then break end
        _castResend = _castResend + 1
        pcall(orig, self, name, ...)
      end
    end

    return orig(self, name, ...)
  end
end

local _wrapMark = {}
local function fullWrap(cls)
  if type(cls) ~= "table" then return end
  local vt = cls
  if type(cls.getVtbl) == "function" then
    local ok, t = pcall(cls.getVtbl)
    if ok and type(t) == "table" then vt = t end
  end
  -- ★必须退回 __index 拿继承的那份，否则漏掉 ClientPet（而宠物才是本作主输出）
  -- ★1.1.0：弹道命中上报（RPC_CS_ProjectileHitTarget / RPC_CS_NotifyProjHit）走
  --   serverMsgNoGC 而不是 serverMsg（dump: @GameApp/Ability/ClientProjectile）
  --   ⇒ 两个入口都要包，只包 serverMsg 会「装上但一封命中包都看不到」。
  for _, key in ipairs({ "serverMsg", "serverMsgNoGC" }) do
    local target = rawget(vt, key) or cls[key]
    if type(target) == "function" and not _wrapMark[target] then
      local wrapped = wrapServerMsg(target)
      _wrapMark[wrapped] = true
      pcall(function() vt[key] = wrapped end)
    end
  end
end

fullWrap(L("Entities.SpaceEntities.ClientMainPlayer"))
fullWrap(L("Entities.SpaceEntities.ClientPet"))
fullWrap(L("Entities.SpaceEntities.ClientPuppet"))
fullWrap(L("Entities.SpaceEntities.ClientCreation"))

---------------------------------------------------------------------
-- ①-b 投射物多段命中 —— ★正式包已移除（2026-09-30 21:5x 决策）
---------------------------------------------------------------------
-- 背景（实机取证，当时滑块值 56）：人物 ME 只发 RPC_CS_ProjectileHitTarget，
--   精灵 PET 发 RPC_CS_DoActOnActorActions；人物唯一通道的弹「同颗重发只认
--   第一次」⇒ 重发式倍率对人物无效、只对精灵有效。曾据此实现 ①-b
--   （destroyOnHit=false / maxHitNum / checkCanHitTarget 恒真）。
--
-- ★移除原因（为什么不进正式包）：
--   1) 用户明确「人物不用做，精灵就够了」——精灵那条 DoActOnActorActions
--      重发通道已实测有效，倍率功能不需要 ①-b；
--   2) ☠ 致命副作用：checkCanHitTarget 是**类级**方法，patchProjectileClass
--      写的 vt.destroyOnHit / vt.maxHitNum 也是**类级**字段 —— 它作用于该类的
--      **所有**投射物实例，**包括敌方的**。敌方弹从此「命中不销毁 + 可重复
--      命中」，属未估清的玩法副作用，不能进正式包；
--   3) 若要给人物加伤，正确做法是按**实例**判定 owner 后再改实例字段，
--      且必须同时保证敌方弹走原逻辑 —— 本轮不做（用户已点名不做）。
--
-- ⇒ 正式包回到斌哥原始设计：按来源身份重发上报包（见 DMG_OUT；菜单名「精灵攻击秒杀」）。
--   投射物相关字段名与 ①-b 实现思路保留在此备查：
--     destroyOnHit / maxHitNum / checkCanHitTarget / hitTargetSet / owner / srcActorId
--     （源路径 XAndPublish/.../LuaScripts/Common/Ability/Projectile/Projectile.lua）
--   类：Common.Ability.Projectile.Projectile / GameApp.Ability.ClientProjectile /
--       GameApp.Ability.EditorProjectile


---------------------------------------------------------------------
-- ② 移速 xN
---------------------------------------------------------------------
local LOAD_ADJ = ATTR.speed_ratio_load_adjustment_v
patchLite(ACA, "getAttribRatioValue", function(orig)
  return function(self, attributeId)
    local v = orig(self, attributeId)
    -- 诊断：游戏到底向这个函数要了哪些 attributeId（仅 YM.diag）
    if YM.diag then
      local h = YM.gvH
      if type(h) ~= "table" then h = {}; YM.gvH = h end
      local k = tostring(attributeId)
      h[k] = (h[k] or 0) + 1
      if attributeId == LOAD_ADJ then
        if isMine(self and self.entity) then
          YM.laMine = (YM.laMine or 0) + 1
        else
          YM.laOther = (YM.laOther or 0) + 1
        end
      end
    end
    local m = YM.spd
    -- ★2026-10-29 1.1.0 实测：这条乘法**确实生效**（A/B 同一 id 取到 1 vs 6），但游戏里
    --   速度不变；连「所有属性比率都乘」也不变 ⇒ 1.1.0 主角色移动速度不由 Lua 属性
    --   getter 驱动（pawn 上取不到 motion 组件，onMotionAttrChange 调了也不重算）。
    --   移速在 1.1.0 暂未生效；保留此乘法（无害）供后续接原生层时复用。
    if type(m) == "number" and m ~= 1 and attributeId == LOAD_ADJ
       and isMine(self and self.entity) then
      return v * m
    end
    return v
  end
end)

---------------------------------------------------------------------
-- ⑤ 无限体力（拦截消耗，放行恢复）
---------------------------------------------------------------------
-- ② 移速 xN（1.1.0 版：两个属性入口都乘）
-- ★2026-10-29 取证：1.1.0 里 getAttribValue(351) 是最高频读法（avH: 351x54），
--   而 getAttribRatioValue(351) 次数少得多 ⇒ 1.1.0 的运动数学改从 getAttribValue 取比率，
--   只挂 getAttribRatioValue（1.0.1 的做法）就「乘法生效但没人用」。
--   两个入口都乘（同一属性不会被同一次计算读两遍，实测取 1→6 线性，无双乘现象）。
patchLite(ACA, "getAttribValue", function(orig)
  return function(self, attributeId)
    local v = orig(self, attributeId)
    if YM.diag then
      local h = YM.avH
      if type(h) ~= "table" then h = {}; YM.avH = h end
      local k = tostring(attributeId)
      h[k] = (h[k] or 0) + 1
    end
    local m = YM.spd
    if type(m) == "number" and m ~= 1 and attributeId == LOAD_ADJ
       and isMine(self and self.entity) then
      if YM.diag then YM.avMul = (YM.avMul or 0) + 1 end
      return v * m
    end
    return v
  end
end)


-- 诊断：getRawAttribValue 被要了哪些 id（移速真实读数很可能走这里）
patchLite(ACA, "getRawAttribValue", function(orig)
  return function(self, attributeId)
    local v = orig(self, attributeId)
    if YM.diag then
      local h = YM.rawH
      if type(h) ~= "table" then h = {}; YM.rawH = h end
      local k = tostring(attributeId)
      h[k] = (h[k] or 0) + 1
    end
    return v
  end
end)

patchLite(ACA, "changeStamina", function(orig)
  return function(self, value)
    local ent = self and self.entity
    if YM.stam and type(value) == "number" and value < 0 and isMine(ent)
       and not (ent and ent.restoringLockedStamina == true) then
      return
    end
    return orig(self, value)
  end
end)

---------------------------------------------------------------------
-- ③ 无敌
---------------------------------------------------------------------
local function hookCheckValidTarget(U)
  if type(U) ~= "table" or type(U.checkValidTarget) ~= "function" then return end
  local orig = U.checkValidTarget
  if YM.diag then YM.cvtWrap = (YM.cvtWrap or 0) + 1 end
  U.checkValidTarget = function(target, owner)
    if YM.diag then
      YM.cvt = (YM.cvt or 0) + 1
      if YM.inv then YM.cvtInv = (YM.cvtInv or 0) + 1 end
    end
    if YM.inv and owner ~= nil and isMine(target) and not isMine(owner)
       and type(U.isEnemy) == "function" then
      local ok, en = pcall(U.isEnemy, owner, target)
      if ok and en == true then
        if YM.diag then YM.cvtBlock = (YM.cvtBlock or 0) + 1 end
        return false
      end
    end
    return orig(target, owner)
  end
end

-- ☠ Common.Utils.Utils 与 Utils.Utils 是同一份源码的两个 package.loaded 实例，两个都要补
local U1 = L("Common.Utils.Utils")
local U2 = L("Utils.Utils")
hookCheckValidTarget(U1)
if U2 and U2 ~= U1 then hookCheckValidTarget(U2) end

patchEntityClass(L("Entities.SpaceEntities.ClientSpace"), "checkImmuneFallDamage", function(orig)
  return function(self, ...)
    if YM.inv then return true end
    return orig(self, ...)
  end
end)

-- ③-b 1.1.0 新通道：AbilityUtils 用 invalidTargetMap/setAbilityInvalidTarget 记录
--     「技能不可选中」实体（dump: @Common/Utils/AbilityUtils）。
--     1.0.1 用的 Utils.checkValidTarget 在 1.1.0 战斗路径已不被调用（实测 cvt=nil）。
local AU = L("Common.Utils.AbilityUtils")
local function markInvalidTarget(on)
  if type(AU) ~= "table" or type(AU.setAbilityInvalidTarget) ~= "function" then return end
  local tgt = pg.pawn or pg.me
  if type(tgt) ~= "table" then return end
  local ok = pcall(AU.setAbilityInvalidTarget, tgt, on ~= false)
  if YM.diag then
    if ok then YM.ait = (YM.ait or 0) + 1 else YM.aitErr = (YM.aitErr or 0) + 1 end
  end
end

-- 关掉时要「真的清掉」：实测（2026-10-29）setAbilityInvalidTarget(t,true) 生效后，
-- 只传 false 清不干净 ⇒ 菜单关掉仍然一直无敌。这里把三条存储都清一遍并兜底删 map 键。
local function clearInvalidTarget()
  local tgt = pg.pawn or pg.me
  if type(tgt) ~= "table" then return end
  if type(AU) == "table" then
    if type(AU.setAbilityInvalidTarget) == "function" then pcall(AU.setAbilityInvalidTarget, tgt, false) end
    if type(AU.setAbilityInvalidLock) == "function" then pcall(AU.setAbilityInvalidLock, tgt, false) end
    local aid = _rg(tgt, "actorId")
    for _, mapName in ipairs({ "invalidTargetMap", "invalidLockMap" }) do
      local m = _rg(AU, mapName)
      if type(m) == "table" then
        pcall(function() m[tgt] = nil end)
        if aid ~= nil then pcall(function() m[aid] = nil end) end
      end
    end
  end
  pcall(function() tgt.dialoguePauseInvincibleEndTs = 0 end)
  local m2 = pg.me
  if type(m2) == "table" then pcall(function() m2.dialoguePauseInvincibleEndTs = 0 end) end
  if YM.diag then YM.clr = (YM.clr or 0) + 1 end
end

local function markInvincible()
  local v = YM.inv and 4e12 or 0
  local list = { pg.me, pg.pawn }
  if pg.me and pg.me.getCurPetEntity then
    local ok, pet = pcall(pg.me.getCurPetEntity, pg.me)
    if ok and type(pet) == "table" then list[#list + 1] = pet end
  end
  for i = 1, #list do
    local e = list[i]
    if type(e) == "table" and e.dialoguePauseInvincibleEndTs ~= v then
      pcall(function() e.dialoguePauseInvincibleEndTs = v end)
    end
  end
end

-- 速度重算：属性 getter 被乘不会自动生效，必须让运动组件重算缓存
-- （dump: @Entities/SpaceEntities/CommonComponent/ClientMotionComponent 有
--   onMotionAttrChange / refreshWalkSpeed / refreshRunSpeed / refreshSprintSpeed…）
local MOTION_KEY = (CONST and CONST.COMPONENT_MOTION) or "COMPONENT_MOTION"
local function motionComp()
  local p = pg.pawn or pg.me
  if type(p) ~= "table" then return nil end
  local c = _rg(p, "motionComponent")
  if type(c) == "table" then return c end
  if type(p.getComponent) == "function" then
    local ok, v = pcall(p.getComponent, p, MOTION_KEY)
    if ok and type(v) == "table" then return v end
  end
  local comps = _rg(p, "components")
  if type(comps) == "table" then
    local v = comps[MOTION_KEY] or comps.motion
    if type(v) == "table" then return v end
  end
  return nil
end

local function refreshSpeed()
  local c = motionComp()
  if type(c) == "table" then
    if YM.diag then YM.motComp = (YM.motComp or 0) + 1 end
    if type(c.onMotionAttrChange) == "function" then pcall(c.onMotionAttrChange, c) end
    for _, fn in ipairs({ "refreshWalkSpeed", "refreshRunSpeed", "refreshSprintSpeed",
                          "refreshSwimSpeed", "refreshGlideSpeed" }) do
      local f = c[fn]
      if type(f) == "function" then pcall(f, c) end
    end
    return
  end
  local p = pg.pawn or pg.me
  if type(p) == "table" and type(p.onMotionAttrChange) == "function" then
    if YM.diag then YM.motPawn = (YM.motPawn or 0) + 1 end
    pcall(p.onMotionAttrChange, p)
  end
end

local function topUpStamina()
  local a = pg.me and pg.me.actorCombatAttribute
  if type(a) ~= "table" then return end
  if not a.getRawAttribValue or not a.changeStamina then return end
  local ok1, cur = pcall(a.getRawAttribValue, a, ATTR.stamina_cur)
  local ok2, mx  = pcall(a.getRawAttribValue, a, ATTR.stamina_max_cur)
  if ok1 and ok2 and type(cur) == "number" and type(mx) == "number" and mx > cur then
    pcall(a.changeStamina, a, mx - cur)
  end
end

---------------------------------------------------------------------
-- ⑥ 瞬移采集（走游戏自己的合法传送流程）
--   ★「瞬移完被弹回原地」的根因是【距离】：服务器对客户端强行搬人有真实的
--     距离上限，超限就位置校验失败(1002)把人弹回原点。斌哥实测 6.5m 成功 /
--     15.1m 被弹回；我们实机 10.4m 成功。⇒ 本实现三重处理：
--       ① 选中目标后先量距离，超限【不瞬移】、落回普通跳跃（打 tpwhy 说明原因，
--          不再白吃掉这一次跳跃，也不再产生「飞过去又弹回来」的体验）；
--       ② 每次瞬移后由 1 秒 tick 复查落点（tpcheck，斌哥判据：记出发点，
--          remainH<3 到位 / movedH<2 弹回 / 否则滑走）；
--       ③ 自适应上限：到位且贴着上限就往上试（×1.3），被弹回就收窄（×0.6），
--          「只有成功才涨」⇒ 永不主动冒险。硬顶 25 米＝官方 EntityPlayerSetPos
--          的搬运上限（@GameApp_DialogueGraph_DialogueGraphRuntime_Node_
--          DialogueGraphEntitySetPosAndRotNode.lua:24-39）。
---------------------------------------------------------------------
local U = U1 or U2
local STAND = 1.0   -- 落点半径：1.5 已验证可用；正上方(0,+2.5) 已证伪(服务器 1002 弹回)

local TP_MAX_DIST      = 10.0   -- 10 米 = 我们自己实测 10.4m 成功的安全侧
local TP_MAX_DIST_HARD = 25.0   -- 官方搬运硬上限，自适应不越过
local TP_MAX_DIST_MIN  = 3.0
local tpMaxDist = TP_MAX_DIST
local tpWatch   = nil           -- {mx,mz,tx,tz,plan}：出发点 / 落点 / 计划水平距离

-- 返回 (ok, kind)：kind = "chest" | "collect" | "env"
--   ★"env" = EnvObject（门/机关/采集点，Utils.isEnvObj）。当前菜单叫「瞬移采集」，
--     所以采集点也要能当目标；但我们【没有】自动开箱/自动拉机关，只是把人挪过去，
--     因此瞬到门口没有副作用。为了不让门抢走宝箱，env 只在【没有】宝箱/掉落物时用
--     （见 nearestChest 的两级选择）。
local function chestEntOk(ent)
  if type(ent) ~= "table" then return false, nil end
  if type(ent.needItem) == "table" and (ent.needItem[1] or 0) > 0 then return false, nil end
  local isChest, isCollect, isEnv = false, false, false
  if U then
    if type(U.isChest) == "function" then
      local ok, r = pcall(U.isChest, ent); isChest = ok and r == true
    end
    if type(U.isCollectItem) == "function" then
      local ok, r = pcall(U.isCollectItem, ent); isCollect = ok and r == true
    end
    if type(U.isEnvObj) == "function" then
      local ok, r = pcall(U.isEnvObj, ent); isEnv = ok and r == true
    end
  end
  if not isChest and not isCollect then
    isChest, isCollect = ent.isChest == true, ent.isCollectItem == true
  end
  if isChest or isCollect then
    if isChest and type(ent.isChestOpened) == "function" then
      local ok, o = pcall(ent.isChestOpened, ent); if ok and o == true then return false, nil end
    end
    if isCollect and type(ent.isCollectItemOpened) == "function" then
      local ok, o = pcall(ent.isCollectItemOpened, ent); if ok and o == true then return false, nil end
    end
    if U and pg.me then
      if isChest and type(U.isChestVisible) == "function" then
        local ok, v = pcall(U.isChestVisible, pg.me, ent); if ok and v == false then return false, nil end
      end
      if isCollect and type(U.isCollectItemVisible) == "function" then
        local ok, v = pcall(U.isCollectItemVisible, pg.me, ent); if ok and v == false then return false, nil end
      end
    end
    return true, (isChest and "chest" or "collect")
  end
  if isEnv then return true, "env" end
  return false, nil
end

local function nearestChest(pawn)
  if type(pawn) ~= "table" or type(pawn.getPosition) ~= "function" then return nil end
  local V3 = _rg(_G, "Vector3")
  if type(V3) ~= "table" or type(V3.SqrDistance) ~= "function" then return nil end
  local okMe, myPos = pcall(pawn.getPosition, pawn)
  if not okMe or type(myPos) ~= "table" then return nil end

  local seen, cand = {}, {}
  if pg.me and type(pg.me.GetRangeEntsChest) == "function" then
    local ok, list = pcall(pg.me.GetRangeEntsChest, pg.me)
    if ok and type(list) == "table" then
      for i = 1, #list do cand[#cand + 1] = list[i] end
    end
  end
  if type(pawn.entitiesInRange) == "function" and CONST.SEARCH_USR_TYPE_INTERACTOR then
    local ok, list = pcall(pawn.entitiesInRange, pawn, 110, CONST.SEARCH_USR_TYPE_INTERACTOR, 0)
    if ok and type(list) == "table" then
      for i = 1, #list do
        local v = list[i]
        local e = v
        if type(v) == "number" and type(pg.getEntityByActorId) == "function" then
          local ok2, r = pcall(pg.getEntityByActorId, v)
          if ok2 then e = r end
        end
        if type(e) == "table" then cand[#cand + 1] = e end
      end
    end
  end

  local best, bestD2, bestKind = nil, nil, nil
  local envBest, envD2 = nil, nil
  for i = 1, #cand do
    local ent = cand[i]
    if type(ent) == "table" and not seen[ent] then
      seen[ent] = true
      local okK, kind = chestEntOk(ent)
      if okK and type(ent.getPosition) == "function" then
        local ok, pos = pcall(ent.getPosition, ent)
        if ok and type(pos) == "table" then
          local okD, d = pcall(V3.SqrDistance, pos, myPos)
          if okD and type(d) == "number" then
            if kind == "env" then
              if not envD2 or d < envD2 then envBest, envD2 = ent, d end
            elseif not bestD2 or d < bestD2 then
              best, bestD2, bestKind = ent, d, kind
            end
          end
        end
      end
    end
  end

  -- ★距离闸门：超限就【不瞬移】（服务器会位置校验失败把人弹回）。
  --   宁愿这一次老老实实普通跳，也不要「飞过去 → 被弹回原点」。
  if best then
    local dist = math.sqrt(bestD2)
    if dist <= tpMaxDist then return best, dist, bestKind end
    if envBest then
      local d2 = math.sqrt(envD2)
      if d2 <= tpMaxDist then return envBest, d2, "env" end
    end
    return nil
  end
  if envBest then
    local d2 = math.sqrt(envD2)
    if d2 <= tpMaxDist then return envBest, d2, "env" end
  end
  return nil
end

local function tpToChest(pawn)
  if type(pawn) ~= "table" then return false end
  if type(CONST.FORBID_POSITION_REASON) ~= "table" then return false end
  local V3 = _rg(_G, "Vector3")
  if type(V3) ~= "function" and type(V3) ~= "table" then return false end
  local ent, dist, kind = nearestChest(pawn)
  if not ent then return false end

  local okE, ePos = pcall(ent.getPosition, ent)
  local okM, myPos = pcall(pawn.getPosition, pawn)
  if not (okE and okM and type(ePos) == "table" and type(myPos) == "table") then return false end

  -- ☠☠getPosition 返回【共享引用】self.posRef（@...ClientPosRotComponent.lua:112）：
  --   传送成功后就地改写 ⇒ 传送【之后】再读 myPos.x/z 拿到的是【落点】而不是出发点。
  --   实测后果：plan 恒为 0.0m、tpcheck 的「弹回原地」判据退化成死代码、自适应上限永远不动。
  --   ⇒ 必须在这里（传送之前）立刻取标量快照 mx/mz/ex/ez。
  local mx, mz = myPos.x, myPos.z
  local ex, ez = ePos.x, ePos.z
  local dx, dz = mx - ex, mz - ez
  local len = math.sqrt(dx * dx + dz * dz)
  if len < 0.0001 then dx, dz, len = 1, 0, 1 end
  local ux, uz = dx / len, dz / len

  local okT, target = pcall(V3, ex + ux * STAND, ePos.y, ez + uz * STAND)
  if not okT or type(target) ~= "table" then return false end

  local rot
  local Q = _rg(_G, "Quaternion")
  if type(Q) == "table" and type(Q.LookRotation) == "function" then
    local okD, dirV = pcall(V3, -ux, 0, -uz)
    if okD then
      local okR, r = pcall(Q.LookRotation, dirV, V3.up)
      if okR then rot = r end
    end
  end

  -- 游戏自己的合法传送流程（照抄 GameApp.Sandbox.LevelItem）
  if type(pg.me.forbidPositionCheck) == "function" then
    pcall(pg.me.forbidPositionCheck, pg.me, {
      CONST.FORBID_POSITION_REASON.LEVEL_ITEM_INTERACT, ent.id, target,
    })
  end

  local moved = false
  if rot and type(pawn.forceSetPosRot) == "function" then
    local ok, r = pcall(pawn.forceSetPosRot, pawn, target, rot, true)
    moved = ok and r ~= false
  end
  if not moved and type(pawn.forceSetPos) == "function" then
    local ok = pcall(pawn.forceSetPos, pawn, target, false)
    moved = ok
  end
  if not moved then return false end

  if pg.world and type(pg.world.setIsSyncPosThisFrame) == "function" and pawn.actorId then
    pcall(pg.world.setIsSyncPosThisFrame, pawn.actorId, true)
  end

  -- ★记【出发点 / 落点 / 计划水平距离】交给下一拍 tick 复查
  --   （斌哥教训：必须记出发点，否则「落地后滑了两步」会被误判成弹回）
  local tx, tz = ex + ux * STAND, ez + uz * STAND
  local pdx, pdz = tx - mx, tz - mz
  local plan = math.sqrt(pdx * pdx + pdz * pdz)
  tpWatch = { mx, mz, tx, tz, plan }

  return true
end

-- 落点复查（每秒 tick 调）：判 kept / SNAPPED-BACK / DRIFTED，并自适应调整距离上限
local function tpVerify()
  local w = tpWatch
  if not w then return end
  tpWatch = nil
  if not (pg.me and type(pg.me.getPosition) == "function") then return end
  local ok, pos = pcall(pg.me.getPosition, pg.me)
  if not ok or type(pos) ~= "table" or type(pos.x) ~= "number" then return end
  local mdx, mdz = pos.x - w[1], pos.z - w[2]
  local movedH = math.sqrt(mdx * mdx + mdz * mdz)
  local rdx, rdz = pos.x - w[3], pos.z - w[4]
  local remainH = math.sqrt(rdx * rdx + rdz * rdz)
  local plan = w[5]
  if remainH < 3.0 then
    -- 到位：贴着上限就往上试一档（「只有成功才涨」⇒ 永不主动冒险）
    if plan > tpMaxDist * 0.8 then
      local nxt = math.min(TP_MAX_DIST_HARD, plan * 1.3)
      if nxt > tpMaxDist then tpMaxDist = nxt end
    end
  elseif movedH < 2.0 then
    -- ☠被弹回原点：收窄上限（这一次失败正是服务器在告诉我们真实边界在哪）
    tpMaxDist = math.max(TP_MAX_DIST_MIN, plan * 0.6)
  end
end

---------------------------------------------------------------------
-- ④ 无限跳跃（+ ⑥ 瞬移接管）
---------------------------------------------------------------------
local COMP_CC = CONST.COMPONENT_INDEX_CHARACETER_CONTROLLER
local CSC = L("Common.Const.CharacterStateConst")

local function airborne(pawn)
  local ok, g = pcall(function() return pawn.eModel.IsOnGround end)
  if ok and type(g) == "boolean" then return not g end
  if CSC and CSC.isChildOfState and CSC.AIRING then
    local ok2, r = pcall(CSC.isChildOfState, pawn.characterState, CSC.AIRING)
    if ok2 then return r == true end
  end
  return false
end

local jumpWrapper = patchLite(PC, "performJump", function(orig)
  return function(self, canJump, inFly)
    local pawn = self and self.pawn

    -- ⑥ 必须在无限跳【之前】接管，否则空中按跳会被无限跳吃掉；inFly 时不接管（会抢 StopFly）
    if YM.tp and not inFly then
      local ok, done = pcall(tpToChest, pawn)
      if ok and done then return end
    end

    -- ④ 空中自己抬：用游戏自带原语 playRaiseUp
    if YM.jump and not inFly then
      local ok = pcall(function()
        if pawn.resetMotorTempState then pawn:resetMotorTempState() end
        if pawn.playRaiseUp then pawn:playRaiseUp(3.2, 0.26, 0.55) end
        local ctrl = pg.game and pg.game.controller
        if ctrl and ctrl.nextSkillAction then ctrl.nextSkillAction:clear() end
        pawn.eModel:SetInputCommand(COMP_CC, IC.Jump)
      end)
      if ok then return end
    end

    return orig(self, canJump, inFly)
  end
end)

-- ☠LiteClass 继承会把父类方法 rawset 缓存进子类 ⇒ PlayerController 也要写一遍
local PLC = L("GameApp.Controller.PlayerController")
if jumpWrapper and type(PLC) == "table" and rawget(PLC, "performJump") ~= jumpWrapper then
  pcall(function() PLC.performJump = jumpWrapper end)
end

---------------------------------------------------------------------
-- 定时维护（由 native 侧每秒/每 0.3 秒调一次 Yimo.luaTick）
---------------------------------------------------------------------
-- ☠☠ 游戏全局表装了「白名单式 __index / __newindex」：
--    裸读未声明全局会走 __logGlobalError（每 200ms 刷一条 Unity 错误日志）并返回 nil，
--    裸写会被 __newindex 拦掉 ⇒ 我们所有自建全局一律 rawget/rawset，与 YM 走同一条路。
local Yimo = _rg(_G, "Yimo")
if type(Yimo) ~= "table" then Yimo = {}; rawset(_G, "Yimo", Yimo) end
Yimo.v = 1
Yimo.tick = function()
  if _rg(_G, "__YM_V") == nil then return end
  _castResend = 0            -- 安全阀：每秒清一次重发预算
  if YM.inv then
    markInvincible()
    markInvalidTarget(true)
  else
    markInvalidTarget(false)
    clearInvalidTarget()
  end
  if YM.spd ~= 1 then refreshSpeed() end
  if YM.stam then topUpStamina() end
  tpVerify()                 -- ⑥ 瞬移落点复查 + 自适应距离上限
end

rawset(_G, "__YM_V", 1)
)LUA";

// 每秒调一次的 Lua 维护入口（★必须 rawget：游戏全局表是白名单式 __index，裸读会刷错误日志）
static const char kYimoTickChunk[] =
    "local G=_G local Y=rawget(G,\"Yimo\") "
    "if type(Y)==\"table\" and type(Y.tick)==\"function\" then Y.tick() end";

// ============================================================
// ④ 同步：把 native 状态写进 Lua 全局 YM
// ============================================================
static bool IsTableGlobal(lua_State* L, const char* name) {
    if (!p_lua_getglobal || !p_lua_type || !p_lua_settop) return false;
    p_lua_getglobal(L, name);
    int t = p_lua_type(L, -1);
    p_lua_settop(L, -2);
    return t == LUA_TTABLE;
}

/* 功能补丁文本（正式包与诊断包完全一致，不被诊断影响）。
 * 诊断 chunk 走独立的 RunDiagChunk（下方）—— ★必须是独立 chunk：
 * 拼进同一段会把「诊断语法错」放大成「整个补丁失效」（Jjdm 踩过同型坑）。 */
static const char* YimoPatchText() { return kYimoPatch; }

#if MOD_DIAG_LUA
/* ============================================================
 * 诊断：从**进程内存**里扒出已解密的 global-metadata.dat
 *
 * 背景（2026-10-29 伊莫 1.1.0）：APK 里的 global-metadata.dat 被易盾加密
 * （Il2CppDumper 直接报 "Metadata file not found or encrypted"），且这个
 * libil2cpp.so 的动态符号里**一个 il2cpp_* 都没有导出**（dynsym 3155 条、含
 * il2cpp 0 条），所以既不能静态 dump、也不能 dlsym IL2CPP API。
 * 唯一可行路线：等游戏把 metadata 解密进内存后，按魔数 0xFAB11BAF 扫出来落盘，
 * 再喂 Il2CppDumper 拿 RVA，最后按 RVA + 序言指纹挂 Dobby。
 * ============================================================ */
#include <sys/mman.h>
#include <cerrno>
#include <cstdlib>
#include <strings.h>
#include <fcntl.h>
#include <unistd.h>

/* ★2026-10-29 第二轮改造（采集器「可知化」，只影响 STARRY_DIAG 诊断包）：
 *   前两版的失败都是「没有任何输出」——分不清线程没起来 / 扫到一半卡死 / 区块被过滤，
 *   所以这一版把三件事补齐：
 *     ① 入口无条件心跳（线程刚起来就打，成功失败都打）；
 *     ② 每次 fopen 失败都带 errno 落日志，不再静默 return；
 *     ③ 把「被过滤掉的区块」按原因分类统计（非可读 / 太小 / 超大 / 超扫描预算）。
 *   另外新增**命令文件** meta_cmd.txt：不用重编译重装，就能让进程按需把任意地址区间
 *   落盘（dump / findstr / big / maps / scan）。离线迭代走它，装机成本降到 0。 */
static const char* kMetaDir =
    "/sdcard/Android/data/com.x.yimoo.official/files";
static const char* kMetaDumpPath =
    "/sdcard/Android/data/com.x.yimoo.official/files/meta_dump.bin";
static const char* kMetaCmdPath =
    "/sdcard/Android/data/com.x.yimoo.official/files/meta_cmd.txt";
static const char* kMetaMapsPath =
    "/sdcard/Android/data/com.x.yimoo.official/files/meta_maps.txt";

static bool YimoWriteBuf(const char* path, const void* data, size_t len) {
    FILE* f = fopen(path, "wb");
    if (!f) { DLOG("meta write FAIL open %s errno=%d", path, errno); return false; }
    const size_t w = fwrite(data, 1, len, f);
    fclose(f);
    if (w != len) {
        DLOG("meta write SHORT %s w=%zu want=%zu", path, w, len);
        return false;
    }
    return true;
}

/* ---- /proc/self/maps 解析 ---- */
struct YimoRegion {
    uintptr_t a;
    uintptr_t b;
    char perms[8];
    char name[256];   /* ★256：80 字节会把 /data/app/~~.../lib/arm64/libil2cpp.so
                       * 截断成「...com.x.yimoo.official-1vcyqI9btT9MidyWEca」，
                       * 导致 strstr(name,"libil2cpp") 永远不命中（实测踩过） */
};

/* maps 第 6 个字段起是映射名（路径或 [anon:...]），可能为空 */
static void YimoRegionName(const char* line, char* dst, size_t cap) {
    dst[0] = 0;
    int field = 0;
    const char* p = line;
    while (*p) {
        while (*p == ' ' || *p == '\t') ++p;
        if (!*p || *p == '\n') break;
        const char* s = p;
        while (*p && *p != ' ' && *p != '\t' && *p != '\n') ++p;
        if (field >= 5) {
            size_t len = (size_t)(p - s);
            if (len >= cap) len = cap - 1;
            memcpy(dst, s, len);
            dst[len] = 0;
            return;
        }
        ++field;
    }
}

static int YimoReadMaps(YimoRegion* out, int cap) {
    FILE* m = fopen("/proc/self/maps", "r");
    if (!m) { DLOG("meta maps FAIL open /proc/self/maps errno=%d", errno); return -1; }
    char line[600];
    int n = 0;
    while (fgets(line, sizeof(line), m)) {
        if (n >= cap) break;
        YimoRegion r{};
        if (sscanf(line, "%lx-%lx %7s", &r.a, &r.b, r.perms) != 3) continue;
        YimoRegionName(line, r.name, sizeof(r.name));
        out[n++] = r;
    }
    fclose(m);
    return n;
}

/* 把 /proc/self/maps 原样落盘（几 KB，离线看布局用） */
static void YimoMetaDumpMaps() {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) { DLOG("meta mapsnap FAIL open /proc/self/maps errno=%d", errno); return; }
    FILE* o = fopen(kMetaMapsPath, "wb");
    if (!o) { DLOG("meta mapsnap FAIL open %s errno=%d", kMetaMapsPath, errno); fclose(f); return; }
    char buf[4096];
    size_t n = 0, tot = 0;
    while ((n = fread(buf, 1, sizeof(buf), f)) > 0) tot += fwrite(buf, 1, n, o);
    fclose(f);
    fclose(o);
    DLOG("meta mapsnap -> %s (%zu B)", kMetaMapsPath, tot);
}

/* ---- 魔数 / 头部启发式扫描 ---- */
static const uint32_t kMetaMagic = 0xFAB11BAFu;

/* ★安全读：直接解引用整个区域会在「maps 快照过期 / 页被 munmap」时吃 SIGSEGV，
 * 上一版 ptrscan 就是这么把采集线程打死的（logcat: CRASH Forwarding signal 11）。
 * 改走 pread(/proc/self/mem)：坏页只返回 EIO/short read，不崩进程。 */
static int YimoSelfMemFd() {
    static int s_fd = -1;
    if (s_fd < 0) s_fd = open("/proc/self/mem", O_RDONLY | O_CLOEXEC);
    return s_fd;
}

/* 读 [addr, addr+len) 到 dst；返回实际读到的字节数（坏页处会 short） */
static size_t YimoSafeRead(uintptr_t addr, void* dst, size_t len) {
    const int fd = YimoSelfMemFd();
    if (fd < 0) return 0;
    uint8_t* out = (uint8_t*)dst;
    size_t done = 0;
    while (done < len) {
        const ssize_t r = pread(fd, out + done, len - done, (off_t)(addr + done));
        if (r <= 0) break;
        done += (size_t)r;
    }
    return done;
}

/* ★2026-10-29 实测：Android 14 + 易盾进程里 pread(/proc/self/mem) 可能整体失败
 * （现象：ptrscan 10 s 就「扫完」7 GB 且 0 命中，连 metadata 指针都找不到）。
 * 所以先自检：拿一个「必定可读」的地址试读 16 B；不行就退回直读。 */
static bool s_directRead = false;   /* true = 直读（有 SIGSEGV 风险，仅诊断包） */
static const char* s_readerName = "?";

static void YimoReaderProbe(uintptr_t knownGood) {
    if (s_readerName[0] != '?') return;
    const int fd = YimoSelfMemFd();
    uint8_t t[16] = {0};
    const size_t got = YimoSafeRead(knownGood, t, sizeof(t));
    if (fd >= 0 && got == sizeof(t)) {
        s_directRead = false;
        s_readerName = "pread(/proc/self/mem)";
        DLOG("meta reader: pread 可用 fd=%d 试读 %p -> %zu B", fd, (void*)knownGood, got);
    } else {
        s_directRead = true;
        s_readerName = "直接解引用";
        DLOG("meta reader: pread 不可用（fd=%d got=%zu errno=%d）→ 退回直接解引用",
             fd, got, errno);
        errno = 0;
    }
}

/* 统一的按块读取：按 s_directRead 选择 pread 或直接解引用 */
static size_t YimoReadChunk(uintptr_t addr, void* dst, size_t len) {
    if (!s_directRead) {
        const size_t got = YimoSafeRead(addr, dst, len);
        if (got) return got;
        s_directRead = true;   /* 中途失效：之后的块改直读 */
        DLOG("meta reader: pread 中途失效 -> 转直读");
        errno = 0;
    }
    memcpy(dst, (const void*)addr, len);
    return len;
}

/* metadata 体量约 29.85 MB；用「落盘一个候选」的公共出口，带完整日志 */
static void YimoMetaSave(const char* kind, int idx, const uint8_t* p, size_t len,
                         size_t capBytes) {
    if (len > capBytes) len = capBytes;
    char path[256];
    snprintf(path, sizeof(path), "%s/meta_%s_%d.bin", kMetaDir, kind, idx);
    const bool ok = YimoWriteBuf(path, p, len);
    DLOG("meta %s #%d @%p len=%zu ok=%d", kind, idx, (const void*)p, len, (int)ok);
}

/* 扫描：魔数命中优先；magic 被抹掉（易盾反 dump 常见）时靠头部字段形状兜底 */
static size_t YimoMetaScanMagic(const YimoRegion* R, int n, size_t budgetBytes,
                                int* pScanned, int* pSkippedBudget, int* pCand) {
    size_t used = 0;
    int scanned = 0, skipped = 0, cand = 0, heur = 0;
    for (int i = 0; i < n; ++i) {
        const YimoRegion& r = R[i];
        if (r.perms[0] != 'r') continue;                  /* 已在统计里计过 */
        const size_t len = (size_t)(r.b - r.a);
        if (len < (256u << 10)) continue;                 /* 同上：已计过 */
        if (used >= budgetBytes) { ++skipped; continue; }
        size_t lim = len;
        if (lim > budgetBytes - used) lim = budgetBytes - used;
        used += lim;
        ++scanned;
        const uint8_t* p = (const uint8_t*)r.a;
        bool tookThis = false;
        for (size_t off = 0; off + 64 <= lim; off += 4) {
            uint32_t v = 0;
            memcpy(&v, p + off, 4);
            if (v == kMetaMagic) {
                int32_t ver = 0;
                memcpy(&ver, p + off + 4, 4);
                int32_t slOff = 0;
                memcpy(&slOff, p + off + 8, 4);
                DLOG("meta MAGIC @%p off=%zu region=%p+%zu ver=%d sl=%d name=%s",
                     (const void*)(p + off), off, (const void*)r.a, off, ver, slOff,
                     r.name);
                YimoMetaSave("dump", cand, p + off, len - off, 48u << 20);
                ++cand;
                tookThis = true;
                break;
            }
            /* 头部启发式：v 是 sanity 被清零/改写后的样子，看 version + 偏移链 */
            if (v <= 1u && heur < 4) {
                int32_t w2 = 0, w3 = 0, w4 = 0, w6 = 0;
                memcpy(&w2, p + off + 8, 4);
                memcpy(&w3, p + off + 12, 4);
                memcpy(&w4, p + off + 16, 4);
                memcpy(&w6, p + off + 24, 4);
                int32_t ver = 0;
                memcpy(&ver, p + off + 4, 4);
                if (ver >= 19 && ver <= 32 &&
                    w2 >= 0x40 && w2 <= 0x2000 &&
                    w3 > 0 && (w3 % 4) == 0 && w3 <= (8 << 20) &&
                    w4 > w2 && w4 <= (48 << 20) &&
                    w6 > w4 && w6 <= (48 << 20)) {
                    DLOG("meta HEUR @%p off=%zu region=%p+%zu ver=%d w=[%u,%d,%d,%d,%d] name=%s",
                         (const void*)(p + off), off, (const void*)r.a, off, ver,
                         v, w2, w3, w4, w6, r.name);
                    YimoMetaSave("heur", heur, p + off, len - off, 48u << 20);
                    ++heur;
                    if (heur >= 4) break;
                }
            }
        }
        if (tookThis && cand >= 4) break;
    }
    if (pScanned) *pScanned = scanned;
    if (pSkippedBudget) *pSkippedBudget = skipped;
    if (pCand) *pCand = cand + heur;
    return used;
}

/* 按字符串在可读区里定位（判断「明文 metadata 到底在不在内存里」最直接的证据） */
static void YimoMetaFindStr(const char* pat, const YimoRegion* R, int n, int maxHits) {
    const size_t plen = strlen(pat);
    if (plen < 4) { DLOG("meta findstr: pattern too short"); return; }
    int hits = 0;
    size_t scannedMB = 0;
    for (int i = 0; i < n && hits < maxHits; ++i) {
        const YimoRegion& r = R[i];
        if (r.perms[0] != 'r') continue;
        const size_t len = (size_t)(r.b - r.a);
        if (len < plen) continue;
        scannedMB += len >> 20;
        const uint8_t* h = (const uint8_t*)r.a;
        const uint8_t* end = h + (len - plen);
        const uint8_t c0 = (uint8_t)pat[0];
        for (const uint8_t* p = h; p <= end;) {
            const uint8_t* q = (const uint8_t*)memchr(p, c0, (size_t)(end - p) + 1);
            if (!q) break;
            if (memcmp(q, pat, plen) == 0) {
                DLOG("meta findstr '%s' @%p region=%p+%zu name=%s",
                     pat, (const void*)q, (const void*)r.a, (size_t)(q - h), r.name);
                ++hits;
                if (hits >= maxHits) break;
                p = q + 1;
            } else {
                p = q + 1;
            }
        }
    }
    if (!hits) DLOG("meta findstr '%s': 0 hits (scanned %zu MB)", pat, scannedMB);
}

/* 把指定尺寸窗口内的 rw 匿名块整体落盘（metadata 解密缓冲大概率就在这个尺寸段） */
static void YimoMetaDumpBig(const YimoRegion* R, int n, size_t minB, size_t maxB,
                            int maxCount, size_t capBytes) {
    int done = 0;
    for (int i = 0; i < n && done < maxCount; ++i) {
        const YimoRegion& r = R[i];
        if (r.perms[0] != 'r' || r.perms[1] != 'w') continue;
        const size_t len = (size_t)(r.b - r.a);
        if (len < minB || len > maxB) continue;
        DLOG("meta big cand #%d region=%p..%p len=%zu name=%s",
             done, (void*)r.a, (void*)r.b, len, r.name);
        YimoMetaSave("big", done, (const uint8_t*)r.a, len, capBytes);
        ++done;
    }
    DLOG("meta big: done=%d (窗口 %zu..%zu B)", done, minB, maxB);
}

/* ---- 指针扫描（安全读版）：找「一串指向 .text 的指针」= methodPointers 数组 ----
 * 快筛：方法指针高 32 位恒为 so 基址高 32 位（0x00004000），
 * 先比 u32 再验全值，比逐槽 64 位比较快一倍多。 */
static void YimoMetaPtrScan(const YimoRegion* R, int n, uintptr_t lo, uintptr_t hi,
                            size_t minBytes) {
    const uint32_t kHi32 = (uint32_t)(lo >> 32);
    const size_t kChunk = 1u << 18;                 /* 256KB/次 pread */
    uint8_t* buf = (uint8_t*)malloc(kChunk + 8);
    if (!buf) { DLOG("meta ptrscan: malloc 失败"); return; }
    YimoReaderProbe(lo);                            /* ★先自检读取方式 */
    DLOG("meta ptrscan [%p,%p) 开始（min=%zuB, reader=%s）", (void*)lo, (void*)hi, minBytes,
         s_readerName);
    int regHits = 0, regScanned = 0, regSkip = 0;
    size_t total = 0, bytesRead = 0;
    uintptr_t bestRunAddr = 0;
    size_t bestRunLen = 0;
    for (int i = 0; i < n; ++i) {
        const YimoRegion& r = R[i];
        if (r.perms[0] != 'r') continue;
        const size_t len = (size_t)(r.b - r.a);
        if (len < minBytes) { ++regSkip; continue; }
        ++regScanned;
        size_t hits = 0, run = 0;
        uintptr_t runStart = 0;
        for (size_t base = 0; base < len; base += kChunk) {
            size_t want = len - base;
            if (want > kChunk) want = kChunk;
            const size_t got = YimoReadChunk(r.a + base, buf, want);
            bytesRead += got;
            if (got < 8) continue;
            for (size_t off = 0; off + 8 <= got; off += 8) {
                uint32_t hi32 = 0;
                memcpy(&hi32, buf + off + 4, 4);
                if (hi32 != kHi32) { run = 0; continue; }
                uintptr_t v = 0;
                memcpy(&v, buf + off, 8);
                if (v >= lo && v < hi) {
                    ++hits;
                    if (run == 0) runStart = r.a + base + off;
                    ++run;
                    if (run > bestRunLen) { bestRunLen = run; bestRunAddr = runStart; }
                } else {
                    run = 0;
                }
            }
        }
        if (hits) {
            ++regHits;
            total += hits;
            DLOG("meta ptrscan region=%p..%p len=%zu hits=%zu name=%s",
                 (void*)r.a, (void*)r.b, len, hits, r.name);
        }
    }
    free(buf);
    DLOG("meta ptrscan 完成: scanned=%d skip(minB)=%d 有命中区=%d hits=%zu read=%zuMB "
         "bestRun=[%p x %zu]",
         regScanned, regSkip, regHits, total, bytesRead >> 20, (void*)bestRunAddr, bestRunLen);
}

/* 反查「哪个 8 字节槽等于这个值」（安全读版） */
static void YimoMetaFindVal(const YimoRegion* R, int n, uintptr_t want, int maxHits) {
    const size_t kChunk = 1u << 18;
    uint8_t* buf = (uint8_t*)malloc(kChunk);
    if (!buf) { DLOG("meta findval: malloc 失败"); return; }
    YimoReaderProbe(want);                          /* 自检（want 本身不一定可读，仅用于日志） */
    int hits = 0;
    size_t bytesRead = 0;
    for (int i = 0; i < n && hits < maxHits; ++i) {
        const YimoRegion& r = R[i];
        if (r.perms[0] != 'r') continue;
        const size_t len = (size_t)(r.b - r.a);
        if (len < 8) continue;
        for (size_t base = 0; base < len && hits < maxHits; base += kChunk) {
            size_t wantLen = len - base;
            if (wantLen > kChunk) wantLen = kChunk;
            const size_t got = YimoReadChunk(r.a + base, buf, wantLen);
            bytesRead += got;
            for (size_t off = 0; off + 8 <= got; off += 8) {
                uintptr_t v = 0;
                memcpy(&v, buf + off, 8);
                if (v == want) {
                    DLOG("meta findval %p -> slot=%p region=%p name=%s",
                         (void*)want, (void*)(r.a + base + off), (void*)r.a, r.name);
                    if (++hits >= maxHits) break;
                }
            }
        }
    }
    free(buf);
    if (!hits) DLOG("meta findval %p: 0 hits (read %zu MB, reader=%s)", (void*)want,
                    bytesRead >> 20, s_readerName);
}

/* ---- 字符串索引表搜索 ----
 * 背景：这块 metadata 的「串数据块」是完整明文（6 MB ASCII），但**索引表不在
 * metadata 文件里**——易盾把它拆到别处，只在运行时存在。没有索引表就没法把
 * method/type 的 nameIndex 还原成名字，所以必须在内存里把它找出来。
 * 判据：一张 u32 表，每个元素都 < 串块大小，且指向的位置恰好是一个「串起点」
 * （前一字节是 NUL、首字节可打印）。 */
static void YimoMetaStrTab(const YimoRegion* R, int n, uintptr_t dataVa, size_t limit,
                           int minRun) {
    if (limit == 0 || limit > (64u << 20)) { DLOG("meta strtab: limit %zu 不合理", limit); return; }
    uint8_t* data = (uint8_t*)malloc(limit);
    uint8_t* valid = (uint8_t*)calloc(limit, 1);
    uint8_t* buf = (uint8_t*)malloc(1u << 18);
    if (!data || !valid || !buf) {
        DLOG("meta strtab: malloc 失败");
        free(data); free(valid); free(buf);
        return;
    }
    size_t gotAll = 0;
    for (size_t base = 0; base < limit; base += (1u << 18)) {
        size_t want = limit - base;
        if (want > (1u << 18)) want = (1u << 18);
        const size_t got = YimoReadChunk(dataVa + base, data + base, want);
        gotAll += got;
        if (got < want) break;
    }
    if (gotAll != limit) {
        DLOG("meta strtab: 串数据块只读到 %zu/%zu B，放弃", gotAll, limit);
        free(data); free(valid); free(buf);
        return;
    }
    for (size_t i = 0; i < limit; ++i) {
        const uint8_t b = data[i];
        if (b >= 32 && b < 127 && (i == 0 || data[i - 1] == 0)) valid[i] = 1;
    }
    YimoReaderProbe(dataVa);
    DLOG("meta strtab: 串块 %zu B @%p 合法串起点读完，开始扫表（reader=%s）",
         limit, (void*)dataVa, s_readerName);
    int regHits = 0, bestRegIdx = -1;
    size_t totalSlots = 0;
    uintptr_t bestAddr = 0;
    size_t bestRun = 0;
    for (int i = 0; i < n; ++i) {
        const YimoRegion& r = R[i];
        if (r.perms[0] != 'r') continue;
        const size_t len = (size_t)(r.b - r.a);
        if (len < 4096) continue;
        size_t hits = 0, run = 0;
        uintptr_t runStart = 0;
        for (size_t base = 0; base < len; base += (1u << 18)) {
            size_t want = len - base;
            if (want > (1u << 18)) want = (1u << 18);
            const size_t got = YimoReadChunk(r.a + base, buf, want);
            if (got < 4) continue;
            for (size_t off = 0; off + 4 <= got; off += 4) {
                uint32_t v = 0;
                memcpy(&v, buf + off, 4);
                if (v < limit && valid[v]) {
                    ++hits;
                    if (run == 0) runStart = r.a + base + off;
                    ++run;
                    if (run > bestRun) { bestRun = run; bestAddr = runStart; bestRegIdx = i; }
                } else {
                    run = 0;
                }
            }
        }
        if (hits) {
            ++regHits;
            totalSlots += hits;
            DLOG("meta strtab region=%p..%p len=%zu 命中槽=%zu name=%s",
                 (void*)r.a, (void*)r.b, len, hits, r.name);
        }
    }
    DLOG("meta strtab 完成: 命中区=%d 命中槽=%zu bestRun=[%p x %zu] 区#%d",
         regHits, totalSlots, (void*)bestAddr, bestRun, bestRegIdx);
    if (bestRun >= (size_t)minRun) {
        /* 把「最长连续表」整段落盘（向后多给 4 倍余量，防止表被截断） */
        const size_t dumpLen = bestRun * 4 + (1u << 20);
        char path[256];
        snprintf(path, sizeof(path), "%s/meta_strtab.bin", kMetaDir);
        DLOG("meta strtab: 落盘 %p len=%zu -> %s", (void*)bestAddr, dumpLen, path);
        YimoWriteBuf(path, (const void*)bestAddr, dumpLen);
    }
    free(data); free(valid); free(buf);
}

/* ---- 命令文件：meta_cmd.txt（离线下发，避免每次重装） ---- */
static void YimoMetaExecCmd(const YimoRegion* R, int n) {
    FILE* f = fopen(kMetaCmdPath, "r");
    if (!f) {
        static bool s_logged = false;
        if (!s_logged) { s_logged = true; DLOG("meta cmd: 无 %s（首装正常态）", kMetaCmdPath); }
        return;
    }
    char buf[8192];
    size_t got = fread(buf, 1, sizeof(buf) - 1, f);
    fclose(f);
    buf[got] = 0;
    uint32_t h = 2166136261u;
    for (size_t i = 0; i < got; ++i) { h ^= (uint8_t)buf[i]; h *= 16777619u; }
    static uint32_t s_last = 0;
    if (h == s_last) return;                  /* 内容没变：不重复执行 */
    s_last = h;
    DLOG("meta cmd: 执行新命令 hash=%08x (%zu B)", h, got);

    char* save = nullptr;
    for (char* ln = strtok_r(buf, "\n", &save); ln; ln = strtok_r(nullptr, "\n", &save)) {
        while (*ln == ' ' || *ln == '\t') ++ln;
        if (!*ln || *ln == '#') continue;
        char verb[32] = {0};
        char a1[128] = {0}, a2[64] = {0}, a3[128] = {0};
        const int nf = sscanf(ln, "%31s %127s %63s %127s", verb, a1, a2, a3);
        if (nf < 1) continue;
        if (strcmp(verb, "maps") == 0) {
            YimoMetaDumpMaps();
        } else if (strcmp(verb, "scan") == 0) {
            size_t mb = nf >= 2 ? (size_t)strtoul(a1, nullptr, 0) : 256;
            int sc = 0, sk = 0, cand = 0;
            const size_t used = YimoMetaScanMagic(R, n, mb << 20, &sc, &sk, &cand);
            DLOG("meta cmd scan: budget=%zuMB used=%zuMB regions=%d skipped=%d cand=%d",
                 mb, used >> 20, sc, sk, cand);
        } else if (strcmp(verb, "dump") == 0 && nf >= 4) {
            const uintptr_t a = (uintptr_t)strtoull(a1, nullptr, 16);
            const size_t len = (size_t)strtoull(a2, nullptr, 16);
            char path[256];
            snprintf(path, sizeof(path), "%s/meta_%s.bin", kMetaDir, a3);
            DLOG("meta cmd dump: %p len=%zu -> %s", (void*)a, len, path);
            if (len && len <= (256u << 20)) {
                YimoWriteBuf(path, (const void*)a, len);
            } else {
                DLOG("meta cmd dump: 长度 %zu 超限（<=256MB）", len);
            }
        } else if (strcmp(verb, "findstr") == 0) {
            const int mx = nf >= 3 ? atoi(a2) : 20;
            YimoMetaFindStr(a1, R, n, mx > 0 ? mx : 20);
        } else if (strcmp(verb, "big") == 0) {
            const size_t minB = (size_t)((nf >= 2 ? strtoul(a1, nullptr, 0) : 24) << 20);
            const size_t maxB = (size_t)((nf >= 3 ? strtoul(a2, nullptr, 0) : 48) << 20);
            const int cnt = nf >= 4 ? atoi(a3) : 4;
            YimoMetaDumpBig(R, n, minB, maxB, cnt > 0 ? cnt : 4, maxB);
        } else if (strcmp(verb, "ptrscan") == 0 && nf >= 3) {
            /* 扫可读区：8 字节槽里有多少值落在 [lo,hi)，并报最长连续指针串。
             * 用途：找 il2cpp 的 methodPointers 数组（一长串指向 .text 的指针）。
             * 第 3 个参数可选：最小区域字节数（默认 64KB）。 */
            const uintptr_t lo = (uintptr_t)strtoull(a1, nullptr, 16);
            const uintptr_t hi = (uintptr_t)strtoull(a2, nullptr, 16);
            const size_t minB = nf >= 4 ? (size_t)strtoull(a3, nullptr, 0) : (64u << 10);
            YimoMetaPtrScan(R, n, lo, hi, minB);
        } else if (strcmp(verb, "findval") == 0 && nf >= 2) {
            /* 找「哪个 8 字节槽等于这个值」（反查谁引用了某地址） */
            const uintptr_t want = (uintptr_t)strtoull(a1, nullptr, 16);
            int mx = nf >= 3 ? atoi(a2) : 20;
            if (mx <= 0) mx = 20;
            YimoMetaFindVal(R, n, want, mx);
        } else if (strcmp(verb, "strtab") == 0 && nf >= 3) {
            /* strtab <串数据块运行时地址> <串块大小> [最小连续表长度] */
            const uintptr_t dataVa = (uintptr_t)strtoull(a1, nullptr, 16);
            const size_t limit = (size_t)strtoull(a2, nullptr, 0);
            const int minRun = nf >= 4 ? atoi(a3) : 1000;
            YimoMetaStrTab(R, n, dataVa, limit, minRun > 0 ? minRun : 1000);
        } else {
            DLOG("meta cmd: 未知动作 '%s'", verb);
        }
    }
}

/* ---- 每一轮：心跳 + 统计 + 现场 ---- */
static void YimoMetaRound(int round) {
    DLOG("meta round #%d begin (pid up, tid tick)", round);
    const int kCap = 4096;
    YimoRegion* R = (YimoRegion*)calloc((size_t)kCap, sizeof(YimoRegion));
    if (!R) { DLOG("meta round #%d: calloc 失败", round); return; }
    const int n = YimoReadMaps(R, kCap);
    if (n <= 0) {
        DLOG("meta round #%d: maps 读取失败 n=%d", round, n);
        free(R);
        return;
    }
    /* 分类统计（这一步就是上一版缺的「可知性」：被过滤掉多少、为什么） */
    int nonRead = 0, small = 0, huge = 0, ok = 0, rwBig = 0;
    size_t bytesReadable = 0;
    uintptr_t imgLo = ~(uintptr_t)0, imgHi = 0;
    for (int i = 0; i < n; ++i) {
        const YimoRegion& r = R[i];
        if (strstr(r.name, "libil2cpp")) {
            if (r.a < imgLo) imgLo = r.a;
            if (r.b > imgHi) imgHi = r.b;
        }
        if (r.perms[0] != 'r') { ++nonRead; continue; }
        const size_t len = (size_t)(r.b - r.a);
        bytesReadable += len;
        if (r.perms[1] == 'w' && len >= (8u << 20)) ++rwBig;
        if (len < (256u << 10)) { ++small; continue; }
        if (len > (512ull << 20)) { ++huge; continue; }
        ++ok;
    }
    DLOG("meta round #%d: maps=%d readable=%d/%d (%.0f MB) eligible=%d "
         "skip{nonRead=%d,small=%d,huge=%d} rw>=8MB=%d img=%p..%p",
         round, n, n - nonRead, n, (double)bytesReadable / (1024.0 * 1024.0), ok,
         nonRead, small, huge, rwBig, (void*)imgLo, (void*)imgHi);

    if (round == 1) {
        YimoMetaDumpMaps();
        int sc = 0, sk = 0, cand = 0;
        const size_t used = YimoMetaScanMagic(R, n, 256u << 20, &sc, &sk, &cand);
        DLOG("meta round #1 scan: used=%zuMB regions=%d skippedBudget=%d cand=%d",
             used >> 20, sc, sk, cand);
        /* metadata 量级（≈29.85 MB）的 rw 块直接落盘：一次拿全，离线慢慢裁 */
        YimoMetaDumpBig(R, n, 24u << 20, 48u << 20, 4, 48u << 20);
    }
    YimoMetaExecCmd(R, n);
    free(R);
    DLOG("meta round #%d end", round);
}

/* 采集线程主体：入口无条件心跳，之后每 20 s 一轮（最多 120 轮 ≈ 40 分钟） */
static void YimoSpdProbeInstall();      /* 定义在下方「移速挂点命中验证」段 */
static void YimoSpdMulWatcher();
static void YimoMetaWorker() {
    DLOG("meta worker ENTER —— 采集线程已启动（改造版，可观测）");
    struct timespec s0{8, 0};
    nanosleep(&s0, nullptr);
    /* 诊断期：装移速挂点探针（只计数）+ 起 spd_mul 只读监视线程 */
    YimoSpdProbeInstall();
    std::thread([] { YimoSpdMulWatcher(); }).detach();
    for (int round = 1; round <= 120; ++round) {
        YimoMetaRound(round);
        struct timespec ts{20, 0};
        nanosleep(&ts, nullptr);
    }
    DLOG("meta worker EXIT after 120 rounds");
}

/* ============================================================
 * 诊断：移速挂点「命中验证」（★只计数 + 只打印，绝不修改返回值/入参）
 *
 * 依据（2026-10-29 离线定位，证据链见
 *   10_docs/伊莫/移速_原生路线_侦察_20261029.md）：
 *   · 类 FunPlus.WorldX.Entities.Components.MotionComponent（游戏主程序集
 *     FunPlus.WorldX.Scripts.dll，非 Lua 绑定层）
 *   · GetAttributeSpeedRatio  RVA 0x0435378c  —— 速度倍率入口候选
 *       str x30,[sp,#-0x10]! ; ldr x8,[x0,#0x18] ; cbz ; ldr x0,[x8,#0xa8] ...
 *       尾巴调用（b）到泛型实例，返回值经 s0 透传
 *   · ChangeMoveStateSpeed    RVA 0x043533fc  —— 按移动状态改速度
 *       str d8,[sp,#-0x40]! ; stp x30,x23 ; cbz x1 ; ... 一连串标志位→速度常量
 *   · set_GmMoveSpeed 里 `STR S0,[X19,#0xB0]` ⇒ 该组件 this+0xB0 是 float 速度字段
 *
 * 挂点铁律：**先验序言指纹，不匹配就拒挂**（宁可没数据也不挂错地址）。
 * ============================================================ */
#define YIMO_RVA_ABSR 0x0435378cu
#define YIMO_RVA_CMSS 0x043533fcu
static const char* kSpdMulPath =
    "/sdcard/Android/data/com.x.yimoo.official/files/spd_mul.txt";

static float (*g_origAbsr)(void*, void*) = nullptr;
static void  (*g_origCmss)(void*, void*) = nullptr;
static std::atomic<uint64_t> g_absrHits{0};
static std::atomic<uint64_t> g_cmssHits{0};
static std::atomic<int> g_spdMul{6};   /* ★本轮固定 6（诊断包硬编码默认；1=关闭） */
static std::atomic<int> g_spdMode{0};  /* 0=GM 倍率位(+0xB0)，1=缩放速度向量(arg1) */
static std::atomic<bool> g_spdCycle{false};

struct YimoSpdSelfRec { void* p; uint64_t hits; };
static YimoSpdSelfRec g_spdSelves[3];
static std::atomic<int> g_spdSelfN{0};

static float YimoReadF32(const void* base, size_t off) {
    float v = 0.0f;
    memcpy(&v, (const uint8_t*)base + off, 4);
    return v;
}

/* 记录最多 3 个不同的 this（第一次出现就打一条，含 this+0xB0） */
static void YimoSpdNoteSelf(void* self, const char* tag, uint64_t hits) {
    if (!self) return;
    const int n = g_spdSelfN.load(std::memory_order_relaxed);
    for (int i = 0; i < n; ++i) {
        if (g_spdSelves[i].p == self) { g_spdSelves[i].hits = hits; return; }
    }
    if (n >= 3) return;
    g_spdSelves[n].p = self;
    g_spdSelves[n].hits = hits;
    g_spdSelfN.store(n + 1, std::memory_order_relaxed);
    DLOG("spd %s NEW this=%p hits=%llu this+0xB0=%.3f", tag, self,
         (unsigned long long)hits, YimoReadF32(self, 0xB0));
}

static float Hook_Absr(void* self, void* a1) {
    const uint64_t n = g_absrHits.fetch_add(1, std::memory_order_relaxed) + 1;
    const float ret = g_origAbsr ? g_origAbsr(self, a1) : 0.0f;
    if (n <= 3 || (n % 60) == 0) {
        DLOG("spd ABSR #%llu this=%p ret=%.4f this+0xB0=%.3f mul=%d",
             (unsigned long long)n, self, ret, self ? YimoReadF32(self, 0xB0) : 0.0f,
             g_spdMul.load());
    }
    YimoSpdNoteSelf(self, "ABSR", n);
    return ret;   /* ★原样返回，不改 */
}

static void Hook_Cmss(void* self, void* a1) {
    const uint64_t n = g_cmssHits.fetch_add(1, std::memory_order_relaxed) + 1;
    if (g_origCmss) g_origCmss(self, a1);
    if (n <= 5 || (n % 20) == 0) {
        DLOG("spd CMSS #%llu this=%p arg1=%p this+0xB0=%.3f mul=%d",
             (unsigned long long)n, self, a1, self ? YimoReadF32(self, 0xB0) : 0.0f,
             g_spdMul.load());
    }
    YimoSpdNoteSelf(self, "CMSS", n);
}

/* libil2cpp 基址：maps 里 offset==0 的那条 libil2cpp 映射 */
static uintptr_t YimoSoBase() {
    FILE* m = fopen("/proc/self/maps", "r");
    if (!m) { DLOG("spd probe: maps 打不开 errno=%d", errno); return 0; }
    char line[600];
    uintptr_t base = 0;
    while (fgets(line, sizeof(line), m)) {
        if (!strstr(line, "libil2cpp.so")) continue;
        uintptr_t a = 0, b = 0, off = 0;
        if (sscanf(line, "%lx-%lx %*s %lx", &a, &b, &off) == 3 && off == 0) { base = a; break; }
    }
    fclose(m);
    return base;
}

static bool YimoFpOk(uintptr_t addr, const uint32_t* fp) {
    if (!addr) return false;
    uint32_t w[4] = {0, 0, 0, 0};
    memcpy(w, (const void*)addr, sizeof(w));
    const bool ok = (w[0] == fp[0] && w[1] == fp[1] && w[2] == fp[2] && w[3] == fp[3]);
    DLOG("spd probe: fp@%p = %08X %08X %08X %08X · 期望 %08X %08X %08X %08X ⇒ %s",
         (void*)addr, w[0], w[1], w[2], w[3], fp[0], fp[1], fp[2], fp[3],
         ok ? "匹配" : "不符");
    return ok;
}

/* ---- 第三个点：MotionComponent.UpdateVelocity(this, arg1, float dt) ----
 * 反汇编（0x0435b884）：sub sp,#0x110 ; fmov s8,s0 ; mov x20,x1 ; mov x19,x0
 * ⇒ 带一个 float 参数（dt），几乎必然是每帧速度更新入口。
 * 只计数 + 打印 this 附近若干 float，供离线判断"实时速度字段"在哪。 */
static void (*g_origUV)(void*, void*, float) = nullptr;
static std::atomic<uint64_t> g_uvHits{0};

/* ★2026-10-29 第三轮：认主 + 生效。
 * 硬证据（UpdateVelocity 内部，rva 0x0435bb8c 起）：
 *     ldr  s0, [x19, #0xb0]        ; s0 = this->GmMoveSpeed
 *     fcmp s0, #0.0
 *     b.le skip                    ; <=0 不生效（默认 -1）
 *     ldr  d1, [x20]  / ldr s2, [x20, #8]      ; 速度向量 xy / z
 *     fmul v1.2s, v1.2s, v0.s[0]   ; xy *= 该值
 *     fmul s0, s0, s2              ; z  *= 该值
 *     str  d1, [x20] / str s0, [x20, #8]       ; 写回速度
 * ⇒ this+0xB0 是**逐帧速度乘法倍率**（>0 生效）。不需要改任何指令，
 *   只要对主角的组件写这个 float 就能改移速；改回 -1 即恢复。
 *
 * 认主：CharacterControllerComponent.get_InputMove 只有本地玩家会调；
 * 链路（见 SetVelocity 反汇编 `ldr x8,[x0,#0x18] ; ldr x0,[x8,#0xd0]`）：
 *   ccc(CCC 组件) -+0x18-> entity  -+0xD0-> MotionComponent
 */
#define YIMO_RVA_INPUTMOVE 0x04305828u
static const uint32_t kFpInputMove[4] = {0xA9BE57FEu, 0xA9014FF4u, 0xD00332F5u, 0xD00332F4u};
static uint64_t (*g_origInputMove)(void*) = nullptr;
static std::atomic<uint64_t> g_imHits{0};
static void* volatile g_playerCcc = nullptr;
static void* volatile g_playerEntity = nullptr;
static void* volatile g_playerMotion = nullptr;
static std::atomic<int> g_spdApplied{0};

/* 判断 [addr, addr+len) 是否完整落在一个可读映射里（用 /proc/self/maps，2 s 缓存）。
 * 比"猜地址范围"靠谱：arg1 这种 Vector3* 常常是调用方栈上的临时量。 */
static bool YimoRangeReadable(const void* addr, size_t len, char* nameOut, size_t nameLen) {
    static char s_maps[768 * 1024];   /* ★实测该进程 maps 约 380 KB，96 KB 会截断导致查不到 */
    static uint64_t s_ts = 0;
    struct timespec t{};
    clock_gettime(CLOCK_MONOTONIC, &t);
    const uint64_t now = (uint64_t)t.tv_sec;
    if (now - s_ts >= 2 || s_maps[0] == '\0') {
        FILE* f = fopen("/proc/self/maps", "r");
        size_t got = 0;
        if (f) {
            got = fread(s_maps, 1, sizeof(s_maps) - 1, f);
            fclose(f);
        }
        s_maps[got] = '\0';
        s_ts = now;
    }
    const uintptr_t a = (uintptr_t)addr;
    const uintptr_t b = a + len;
    const char* p = s_maps;
    while (*p) {
        uintptr_t lo = 0, hi = 0;
        char perms[8] = {0};
        int off = 0;
        if (sscanf(p, "%lx-%lx %7s %n", &lo, &hi, perms, &off) >= 3) {
            if (a >= lo && b <= hi) {
                const char* nl = strchr(p, '\n');
                if (nameOut && nameLen) {
                    const char* nm = p + off;
                    size_t k = 0;
                    while (nm < (nl ? nl : p + strlen(p)) && k + 1 < nameLen &&
                           *nm != '\n') {
                        nameOut[k++] = *nm++;
                    }
                    nameOut[k] = '\0';
                }
                return perms[0] == 'r';
            }
        }
        p = strchr(p, '\n');
        if (!p) break;
        ++p;
    }
    return false;
}

static bool YimoPtrSane(const void* p) {
    const uintptr_t v = (uintptr_t)p;
    /* 注意两点（都踩过）：
     *  · Vector3* 之类常常只有 4 字节对齐，不能要求 8 字节对齐；
     *  · 这台设备的堆地址形如 0x4000_xxxx_xxxx（≈7.0e13），比 0x8000000000 大得多，
     *    早期上限写成 0x8000000000 导致所有实体指针都被误判非法。 */
    return v > 0x10000ull && (v & 3) == 0 && v < 0x7fffffffffffULL;
}

static uint64_t Hook_InputMove(void* self) {
    const uint64_t r = g_origInputMove ? g_origInputMove(self) : 0;
    const uint64_t n = g_imHits.fetch_add(1, std::memory_order_relaxed) + 1;
    if (self && !g_playerEntity) {
        void* ent = nullptr;
        memcpy(&ent, (const uint8_t*)self + 0x18, sizeof(ent));
        if (YimoPtrSane(ent)) {
            g_playerCcc = self;
            g_playerEntity = ent;          /* 只要 entity，MotionComponent 由 UV 侧比对认领 */
            DLOG("spd OWNER 候选 ccc=%p entity=%p（inputMove 第 %llu 次，等 MotionComponent 比对）",
                 self, ent, (unsigned long long)n);
        } else {
            DLOG("spd OWNER inputMove this=%p 的 +0x18 不像实体指针，忽略", self);
        }
    }
    return r;
}

/* ---- 认主点 B：CharacterControllerComponent.CheckSprint ----
 * 冲刺键必然来自真人输入，AI 不会按冲刺键 ⇒ 它的 this 就是玩家。
 * 反汇编（0x0430b0d4）：stp x30,x23 / stp x20,x19 / cmp w0,#0x14 / mov x19,x0
 *   签名按 (void* self, int key, void* extra) 保守转发（w0=按键值）。 */
#define YIMO_RVA_CHECKSPRINT 0x0430b0d4u
static const uint32_t kFpCheckSprint[4] = {0xA9BE57FEu, 0xA9014FF4u, 0x7100503Fu, 0xAA0003F3u};
static uint64_t (*g_origCheckSprint)(void*, int, void*) = nullptr;
static std::atomic<uint64_t> g_csHits{0};

static void YimoSpdLockOwner(void* ccc, const char* tag, uint64_t n) {
    /* 只做记录，不直接锁主：锁定统一由 UV 侧「entity 指针比对」完成，
     * 避免把"会冲刺的 NPC"误当玩家。 */
    if (!ccc || g_playerMotion) return;
    void* ent = nullptr;
    memcpy(&ent, (const uint8_t*)ccc + 0x18, sizeof(ent));
    DLOG("spd OWNER(%s#%llu) ccc=%p entity=%p（仅供参考，锁定由 UV 侧比对决定）", tag,
         (unsigned long long)n, ccc, ent);
}

static uint64_t Hook_CheckSprint(void* self, int key, void* extra) {
    const uint64_t n = g_csHits.fetch_add(1, std::memory_order_relaxed) + 1;
    const uint64_t r = g_origCheckSprint ? g_origCheckSprint(self, key, extra) : 0;
    if (n <= 20) DLOG("spd CS #%llu this=%p key=%d ret=%llu", (unsigned long long)n, self,
                      key, (unsigned long long)r);
    YimoSpdLockOwner(self, "CS", n);
    return r;   /* ★原样透传返回值（可能是 bool） */
}

/* ---- 认主点 C/D：CCC.OnHandleMove / CCC.get_MoveAxis3D ----
 * 这两个点承载"按键输入"，只有本地玩家才有非零输入 ⇒ 用来拿 entity。
 * 结构校验：只要求首指令像 AArch64 序言（stp/sub/str 压栈），并把真实 4 字打进
 * 日志，便于后续固化成精确指纹（本轮先用结构校验，因为要在装机后才能读到运行时代码）。 */
#define YIMO_RVA_ONHANDLEMOVE 0x043079e0u
#define YIMO_RVA_MOVEAXIS3D   0x0430598cu
struct YimoVec3 { float x, y, z; };
static YimoVec3 (*g_origMoveAxis3D)(void*, void*, void*) = nullptr;
static std::atomic<uint64_t> g_ohmHits{0};
static std::atomic<uint64_t> g_maHits{0};

static bool YimoPrologueSane(uintptr_t addr, const char* tag) {
    uint32_t w[4] = {0, 0, 0, 0};
    memcpy(w, (const void*)addr, sizeof(w));
    /* 判据（实测校准过两次）：
     *  · 叶子存取器不一定有栈帧 —— get_MoveAxis3D 是 `movi d1,#0 ; ldp s0,s1,[x0,#0x48] ; ret`
     *    （3 条指令、无 prologue）；OnHandleMove 是 `stp d13,d12,[sp,#-0x60]!`（FP 压栈）；
     *  · 文件里 .text 是密文，填充模式为 `a1 00 00 00` 重复 ⇒ 用它当"未解密/挂错"的判据；
     *  · 这 4 个字一律打进日志，供事后固化成精确指纹。 */
    const bool filler = (w[0] == 0x000000A1u || w[0] == 0xA1000000u) &&
                        (w[1] == 0x000000A1u || w[1] == 0xA1000000u);
    const bool nonzero = (w[0] | w[1] | w[2] | w[3]) != 0;
    const bool ok = nonzero && !filler;
    DLOG("spd probe: %s @%p 首4字 = %08X %08X %08X %08X（filler=%d ⇒ %s）", tag, (void*)addr,
         w[0], w[1], w[2], w[3], (int)filler, ok ? "已解密代码，允许挂" : "拒绝挂");
    return ok;
}

/* 从 CCC 组件拿 entity，交给 UV 侧用 MotionComponent+0x18 比对锁定 */
static void YimoSpdOwnerFromCcc(void* ccc, const char* tag, uint64_t n) {
    if (!ccc || g_playerMotion) return;
    void* ent = nullptr;
    memcpy(&ent, (const uint8_t*)ccc + 0x18, sizeof(ent));
    if (!YimoPtrSane(ent)) {
        DLOG("spd OWNER2(%s#%llu) ccc=%p 的 +0x18=%p 不像实体，忽略", tag,
             (unsigned long long)n, ccc, ent);
        return;
    }
    if (!g_playerEntity) {
        g_playerEntity = ent;
        g_playerCcc = ccc;
        DLOG("spd OWNER2(%s#%llu) 记录玩家 entity=%p（来自 ccc=%p），等 MotionComponent 比对",
             tag, (unsigned long long)n, ent, ccc);
    }
}


static YimoVec3 Hook_MoveAxis3D(void* self, void* a1, void* a2) {
    YimoVec3 r{0.0f, 0.0f, 0.0f};
    if (g_origMoveAxis3D) r = g_origMoveAxis3D(self, a1, a2);
    const uint64_t n = g_maHits.fetch_add(1, std::memory_order_relaxed) + 1;
    const float m = r.x * r.x + r.y * r.y + r.z * r.z;
    if (n <= 8 || m > 0.01f) {
        DLOG("spd MAXIS #%llu this=%p axis=(%.3f,%.3f,%.3f) |a|=%.3f", (unsigned long long)n,
             self, r.x, r.y, r.z, sqrtf(m));
    }
    /* 只有"有按键输入"（轴非零）才认主：AI 的输入通常为 0，能排除 NPC */
    if (m > 0.01f) YimoSpdOwnerFromCcc(self, "MAXIS", n);
    return r;
}


#define YIMO_SPD_ENTS 64
#define YIMO_SPD_W0   0x60      /* 扫描窗口起点 */
#define YIMO_SPD_WORDS 64       /* 0x60..0x160，共 64 个 32 位字 */

struct YimoSpdEnt { void* p; uint64_t hits; uint32_t snap[YIMO_SPD_WORDS]; bool inited; };
static YimoSpdEnt g_spdEnts[YIMO_SPD_ENTS];
static std::atomic<int> g_spdEntN{0};

static void YimoSpdTrackAndDiff() {
    /* 每 1 s 一次；对每个已知组件读窗口并和上次快照比对，只打变化 */
    static uint64_t s_lastMs = 0;
    struct timespec ts{};
    clock_gettime(CLOCK_MONOTONIC, &ts);
    const uint64_t nowMs = (uint64_t)ts.tv_sec * 1000ull + ts.tv_nsec / 1000000ull;
    if (nowMs - s_lastMs < 1000ull) return;
    s_lastMs = nowMs;

    const int n = g_spdEntN.load(std::memory_order_relaxed);
    int changedEnts = 0;
    for (int i = 0; i < n; ++i) {
        YimoSpdEnt& e = g_spdEnts[i];
        if (!e.p) continue;
        uint32_t cur[YIMO_SPD_WORDS];
        memcpy(cur, (const uint8_t*)e.p + YIMO_SPD_W0, sizeof(cur));
        if (!e.inited) {
            memcpy(e.snap, cur, sizeof(cur));
            e.inited = true;
            continue;
        }
        int diffs = 0;
        char line[700];
        int o = 0;
        for (int k = 0; k < YIMO_SPD_WORDS && diffs < 8; ++k) {
            if (cur[k] == e.snap[k]) continue;
            float fo, fn;
            memcpy(&fo, &e.snap[k], 4);
            memcpy(&fn, &cur[k], 4);
            o += snprintf(line + o, sizeof(line) - o, " +%#04x:%.3f->%.3f",
                          YIMO_SPD_W0 + k * 4, fo, fn);
            ++diffs;
        }
        if (diffs) {
            ++changedEnts;
            if (changedEnts <= 3) {
                DLOG("spd CHG this=%p%s", e.p, line);
            }
            memcpy(e.snap, cur, sizeof(cur));
        }
    }
    static int s_tick = 0;
    if (++s_tick % 5 == 0) {
        DLOG("spd TICK tracked=%d changed=%d uvHits=%llu owner=%p entity=%p mul=%d",
             n, changedEnts, (unsigned long long)g_uvHits.load(), (void*)g_playerMotion,
             (void*)g_playerEntity, g_spdMul.load());
    }
}

static void Hook_UV(void* self, void* a1, float dt) {
    const uint64_t n = g_uvHits.fetch_add(1, std::memory_order_relaxed) + 1;
    const int mul = g_spdMul.load(std::memory_order_relaxed);

    /* ---- 认主（实体指针比对）：MotionComponent+0x18 → entity ----
     * 依据：UpdateVelocity 里 `ldr x9,[x19,#0x18] ; ldr x21,[x9,#0xc8]`、
     *       GetAttributeSpeedRatio 里 `ldr x8,[x0,#0x18] ; ldr x0,[x8,#0xa8]`
     *       ⇒ 组件 +0x18 就是 entity。get_InputMove 已经给出玩家 entity，
     *       哪个 MotionComponent 的 +0x18 等于它，哪个就是主角。 */
    if (!g_playerMotion && g_playerEntity && self) {
        void* myEnt = nullptr;
        memcpy(&myEnt, (const uint8_t*)self + 0x18, sizeof(myEnt));
        if (myEnt && myEnt == g_playerEntity) {
            g_playerMotion = self;
            DLOG("spd OWNER 锁定主角 MotionComponent=%p（entity=%p 指针比对一致）"
                 " this+0xB0=%.3f", self, myEnt, YimoReadF32(self, 0xB0));
        }
    }

    /* ★模式 0（默认）：走游戏自带的 GM 倍率位 this+0xB0 —— 原函数在 +0308 处读它，
     *   用它乘速度向量。这是游戏自己的 cheat 通路，语义最可靠，且在调用【之前】写。 */
    const bool isPlayerPre = (self && (void*)self == g_playerMotion);
    if (g_spdMode.load() == 0 && isPlayerPre) {
        const float want = (mul == 1) ? -1.0f : (float)mul;
        float cur = 0.0f;
        memcpy(&cur, (const uint8_t*)self + 0xB0, 4);
        if (cur != want) {
            memcpy((uint8_t*)self + 0xB0, &want, 4);
            DLOG("spd APPLY 主角 this=%p +0xB0: %.3f -> %.3f (mul=%d)", self, cur, want, mul);
        }
    }

    /* ★判据日志：pre（调用原函数前）/ post（调用原函数后）三轴数值 + 当场倍率位。
     * 机制若成立，post 的幅值应≈ pre 的 6 倍（游戏自己在 +0314..+0328 乘这个位）。 */
    const bool a1ok = (a1 && YimoRangeReadable(a1, 12, nullptr, 0));
    float pre3[3] = {0.0f, 0.0f, 0.0f};
    float gmBefore = 0.0f;
    if (a1ok) memcpy(pre3, a1, sizeof(pre3));
    if (self) gmBefore = YimoReadF32(self, 0xB0);

    if (g_origUV) g_origUV(self, a1, dt);

    if (a1ok) {
        float post3[3] = {0.0f, 0.0f, 0.0f};
        memcpy(post3, a1, sizeof(post3));
        const bool isPlayerNow = (self && (void*)self == g_playerMotion);
        static uint64_t s_velMs = 0;
        static std::atomic<int> s_velN{0};
        struct timespec tv{};
        clock_gettime(CLOCK_MONOTONIC, &tv);
        const uint64_t nowMs = (uint64_t)tv.tv_sec * 1000ull + tv.tv_nsec / 1000000ull;
        if (isPlayerNow ? (nowMs - s_velMs >= 500ull) : (s_velN.load() < 4)) {
            if (isPlayerNow) s_velMs = nowMs; else s_velN.fetch_add(1);
            const float lp = sqrtf(pre3[0] * pre3[0] + pre3[1] * pre3[1] + pre3[2] * pre3[2]);
            const float lo = sqrtf(post3[0] * post3[0] + post3[1] * post3[1] +
                                   post3[2] * post3[2]);
            DLOG("spd VEL%s this=%p mul=%d pre=(%.3f,%.3f,%.3f) gm=%.3f "
                 "post=(%.3f,%.3f,%.3f) |pre|=%.3f |post|=%.3f post/pre=%.2f",
                 isPlayerNow ? " 主角" : " 采样", self, mul, pre3[0], pre3[1], pre3[2],
                 gmBefore, post3[0], post3[1], post3[2], lp, lo,
                 (lp > 1e-4f ? lo / lp : 0.0f));
        }
    }

    /* ★模式 1（a6 这种写法）：post-call 直接把速度向量 xyz 各乘 mul。
     * 为什么不用 +0x1B0..+0x1BC：反汇编显示那 4 个 float 是与 [+0x44]/[+0x4C] 点乘后
     * 取 fmin(fabs,1) 再和阈值比较 —— 是**法线/方向**（坡度限制），不是速度。
     * 真正的速度向量 = 本函数第二参 arg1（`mov x20,x1` 之后
     *   `ldp s0,s1,[x20] ; ldr s2,[x20,#8]` 读、`str d1,[x20]/str s0,[x20,#8]` 写），
     *   游戏自己的 GM 倍率分支也是拿 +0xB0 乘这个向量。 */
    if (g_spdMode.load() == 1 && mul != 1 && a1 && YimoRangeReadable(a1, 12, nullptr, 0)) {
        float v[3];
        memcpy(v, a1, sizeof(v));
        const bool finite = (v[0] == v[0] && v[1] == v[1] && v[2] == v[2]) &&
                            (v[0] < 1e5f && v[0] > -1e5f) && (v[1] < 1e5f && v[1] > -1e5f) &&
                            (v[2] < 1e5f && v[2] > -1e5f);
        const float mag = v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
        const bool isPlayer = (self && (void*)self == g_playerMotion);
        /* 未认主时做最多 5 次「对照实验」：对正在动的单位缩放一次，证明写入真的生效 */
        static std::atomic<int> s_ctrl{0};
        const bool control = !isPlayer && !g_playerMotion && mag > 0.04f &&
                             s_ctrl.load() < 5;
        if (finite && (isPlayer || control)) {
            float out[3] = {v[0] * (float)mul, v[1] * (float)mul, v[2] * (float)mul};
            memcpy(a1, out, sizeof(out));
            static uint64_t s_lastMs = 0;
            struct timespec ts{};
            clock_gettime(CLOCK_MONOTONIC, &ts);
            const uint64_t nowMs = (uint64_t)ts.tv_sec * 1000ull + ts.tv_nsec / 1000000ull;
            if (control || nowMs - s_lastMs >= 1000ull) {
                s_lastMs = nowMs;
                if (control) s_ctrl.fetch_add(1);
                DLOG("spd APPLY%s this=%p mul=%d arg1=%p 速度(前) %.3f,%.3f,%.3f -> (后) "
                     "%.3f,%.3f,%.3f | |v| %.3f -> %.3f",
                     isPlayer ? " 主角" : " 对照", self, mul, a1, v[0], v[1], v[2],
                     out[0], out[1], out[2], sqrtf(mag),
                     sqrtf(out[0] * out[0] + out[1] * out[1] + out[2] * out[2]));
            }
        }
    }
    if (!self) return;
    /* 注册新组件（最多 64 个；满了就挤掉命中次数最少的那个，保证"活着的"单位都在） */
    const int en = g_spdEntN.load(std::memory_order_relaxed);
    int idx = -1;
    for (int i = 0; i < en; ++i) if (g_spdEnts[i].p == self) { idx = i; break; }
    if (idx >= 0) {
        ++g_spdEnts[idx].hits;
    } else {
        int slot = (en < YIMO_SPD_ENTS) ? en : 0;
        if (en < YIMO_SPD_ENTS) {
            g_spdEntN.store(en + 1, std::memory_order_relaxed);
        } else {
            uint64_t minHits = ~0ull;
            for (int i = 0; i < en; ++i) {
                if (g_spdEnts[i].hits < minHits) { minHits = g_spdEnts[i].hits; slot = i; }
            }
        }
        g_spdEnts[slot].p = self;
        g_spdEnts[slot].hits = 1;
        g_spdEnts[slot].inited = false;
        DLOG("spd REG #%d this=%p dt=%.4f (累计命中 %llu)", slot, self, dt,
             (unsigned long long)n);
    }
    YimoSpdTrackAndDiff();
}

static void YimoSpdProbeInstall() {
    const uintptr_t base = YimoSoBase();
    DLOG("spd probe: so_base=%p（ABSR=%p CMSS=%p）", (void*)base,
         (void*)(base + YIMO_RVA_ABSR), (void*)(base + YIMO_RVA_CMSS));
    if (!base) { DLOG("spd probe: 找不到 libil2cpp 基址，放弃挂点"); return; }

    static const uint32_t kFpAbsr[4] = {0xF81F0FFEu, 0xF9400C08u, 0xB4000148u, 0xF9405500u};
    static const uint32_t kFpCmss[4] = {0xFC1C0FE8u, 0xA9015FFEu, 0xA90257F6u, 0xA9034FF4u};
    static const uint32_t kFpUV[4]   = {0xD10443FFu, 0x6D083BEFu, 0x6D0933EDu, 0x6D0A2BEBu};
    const uintptr_t aUV = base + 0x0435b884u;

    if (YimoFpOk(base + YIMO_RVA_ABSR, kFpAbsr)) {
        const int rc = DobbyHook((void*)(base + YIMO_RVA_ABSR), (void*)Hook_Absr,
                                 (void**)&g_origAbsr);
        DLOG("spd probe: DobbyHook ABSR rc=%d orig=%p", rc, (void*)g_origAbsr);
    } else {
        DLOG("spd probe: ABSR 指纹不符 ⇒ 拒挂（宁可不改也不挂错）");
    }
    if (YimoFpOk(base + YIMO_RVA_CMSS, kFpCmss)) {
        const int rc = DobbyHook((void*)(base + YIMO_RVA_CMSS), (void*)Hook_Cmss,
                                 (void**)&g_origCmss);
        DLOG("spd probe: DobbyHook CMSS rc=%d orig=%p", rc, (void*)g_origCmss);
    } else {
        DLOG("spd probe: CMSS 指纹不符 ⇒ 拒挂");
    }
    if (YimoFpOk(aUV, kFpUV)) {
        const int rc = DobbyHook((void*)aUV, (void*)Hook_UV, (void**)&g_origUV);
        DLOG("spd probe: DobbyHook UV rc=%d orig=%p", rc, (void*)g_origUV);
    } else {
        DLOG("spd probe: UV 指纹不符 ⇒ 拒挂");
    }
    /* 认主点：get_InputMove（只有本地玩家调） */
    if (YimoFpOk(base + YIMO_RVA_INPUTMOVE, kFpInputMove)) {
        const int rc = DobbyHook((void*)(base + YIMO_RVA_INPUTMOVE), (void*)Hook_InputMove,
                                 (void**)&g_origInputMove);
        DLOG("spd probe: DobbyHook InputMove rc=%d orig=%p", rc, (void*)g_origInputMove);
    } else {
        DLOG("spd probe: InputMove 指纹不符 ⇒ 拒挂（主角将无法认出，移速不会生效）");
    }
    /* 认主点 B：CheckSprint（真人按冲刺键才会走这里） */
    if (YimoFpOk(base + YIMO_RVA_CHECKSPRINT, kFpCheckSprint)) {
        const int rc = DobbyHook((void*)(base + YIMO_RVA_CHECKSPRINT),
                                 (void*)Hook_CheckSprint, (void**)&g_origCheckSprint);
        DLOG("spd probe: DobbyHook CheckSprint rc=%d orig=%p", rc, (void*)g_origCheckSprint);
    } else {
        DLOG("spd probe: CheckSprint 指纹不符 ⇒ 拒挂（主角认不出来）");
    }
    /* 认主点 C/D：OnHandleMove（记录用）/ get_MoveAxis3D（有按键输入时认主） */
    if (YimoPrologueSane(base + YIMO_RVA_MOVEAXIS3D, "get_MoveAxis3D")) {
        const int rc = DobbyHook((void*)(base + YIMO_RVA_MOVEAXIS3D),
                                 (void*)Hook_MoveAxis3D, (void**)&g_origMoveAxis3D);
        DLOG("spd probe: DobbyHook MoveAxis3D rc=%d orig=%p", rc, (void*)g_origMoveAxis3D);
    }
    DLOG("spd probe: 装点完成 absr=%p cmss=%p uv=%p inputmove=%p checksprint=%p maxis=%p",
         (void*)g_origAbsr, (void*)g_origCmss, (void*)g_origUV,
         (void*)g_origInputMove, (void*)g_origCheckSprint,
         (void*)g_origMoveAxis3D);
}

/* spd_mul.txt：每 1 s 读一次。支持三种写法：
 *   纯数字 N        → 走"游戏自带 GM 倍率位"：给主角 this+0xB0 写 N（N=1 ⇒ 写 -1 关闭）
 *   a<N>（如 a6）   → 走"直接缩放速度向量"：post-call 把 arg1 的 xyz 各乘 N
 *   cycle           → 自动翻牌：每 10 s 在 6 与 1 之间切换（免对时的 A/B）
 * 无论哪种模式，都只在【主角】上生效；未认主时宁可不写。 */
static void YimoSpdMulWatcher() {
    DLOG("spd mul watcher ENTER（数字=GM倍率位 / a6=缩放向量 / cycle=自动翻牌）");
    int last = -1;
    bool lastCycle = false;
    int cycleIdx = 0;
    uint64_t cycleStartMs = 0;
    for (int i = 0; i < 3600; ++i) {          /* ≈1 小时 */
        FILE* f = fopen(kSpdMulPath, "r");
        char buf[64] = {0};
        bool got = false;
        if (f) {
            if (fgets(buf, sizeof(buf), f)) got = true;
            fclose(f);
        }
        if (got) {
            if (strstr(buf, "cycle")) {
                struct timespec ts{};
                clock_gettime(CLOCK_MONOTONIC, &ts);
                const uint64_t nowMs = (uint64_t)ts.tv_sec * 1000ull + ts.tv_nsec / 1000000ull;
                if (!lastCycle) {
                    lastCycle = true;
                    g_spdCycle.store(true);
                    g_spdMode.store(0);
                    cycleIdx = 0;
                    cycleStartMs = nowMs;
                    DLOG("spd mul -> cycle 模式开始（每 10 s 翻 6 / 1）");
                }
                if (nowMs - cycleStartMs >= 10000ull) {
                    cycleStartMs = nowMs;
                    ++cycleIdx;
                }
                const int want = (cycleIdx % 2 == 0) ? 6 : 1;
                if (want != last) {
                    last = want;
                    g_spdMul.store(want);
                    g_spdMult.store(want);   /* 诊断覆盖值 -> 同一实现的倍率源 */
                    DLOG("spd mul -> %d（cycle 第 %d 段）", want, cycleIdx + 1);
                }
            } else {
                if (lastCycle) {
                    lastCycle = false;
                    g_spdCycle.store(false);
                    DLOG("spd mul -> 退出 cycle 模式");
                }
                int mode = 0;
                const char* p = buf;
                while (*p == ' ' || *p == '\t') ++p;
                if (*p == 'a' || *p == 'A') { mode = 1; ++p; }
                const int v = atoi(p);
                if (v >= 0 && v <= 100) {
                    if (mode != g_spdMode.load()) {
                        g_spdMode.store(mode);
                        DLOG("spd mul 模式 -> %s", mode ? "缩放速度向量" : "GM 倍率位");
                    }
                    if (v != last) {
                        last = v;
                        g_spdMul.store(v);
                        g_spdMult.store(v);   /* 诊断覆盖值 -> 同一实现的倍率源 */
                        DLOG("spd mul -> %d（只作用于主角；1 = 关闭）", v);
                    }
                }
            }
        }
        struct timespec ts{1, 0};
        nanosleep(&ts, nullptr);
    }
    DLOG("spd mul watcher EXIT");
}


/* ============================================================
 * 诊断期 Lua 模块转储（照 口袋觉醒/Match2Mod.cpp 的成熟做法搬过来）
 *
 * 目的：1.1.0 里「移速/无敌/精灵攻击秒杀」三项失效，靠猜挂点已反复落空；
 *       把游戏实际 loadbuffer 进来的 Lua 模块全量落盘，直接读代码定挂点。
 * 路径：/sdcard/Android/data/<pkg>/files/starry_lua_dump（App 自有目录，无需存储权限）
 * 只在 STARRY_DIAG=1 的诊断包里存在，正式包零代码。
 * ============================================================ */
#include <sys/stat.h>
#include <unistd.h>

static const char* kYimoDumpDir =
    "/sdcard/Android/data/com.x.yimoo.official/files/starry_lua_dump";

static uint32_t YimoPathHash32(const char* s) {
    uint32_t h = 2166136261u;
    if (!s) return h;
    for (; *s; s++) { h ^= (uint8_t)*s; h *= 16777619u; }
    return h;
}

static bool YimoWriteDump(const char* path, const uint8_t* data, size_t len) {
    if (!path || !path[0] || !data || !len || len > (2 << 20)) return false;
    mkdir(kYimoDumpDir, 0777);
    char fp[384];
    snprintf(fp, sizeof(fp), "%s/m_%08x.bin", kYimoDumpDir, YimoPathHash32(path));
    if (access(fp, F_OK) == 0) return true;
    FILE* f = fopen(fp, "wb");
    if (!f) return false;
    const size_t w = fwrite(data, 1, len, f);
    fclose(f);
    if (w != len) return false;
    char mp[384];
    snprintf(mp, sizeof(mp), "%s/m_%08x.path", kYimoDumpDir, YimoPathHash32(path));
    FILE* m = fopen(mp, "w");
    if (m) { fprintf(m, "%s\n%zu\n", path, len); fclose(m); }
    static std::atomic<uint32_t> s_cnt{0};
    const uint32_t n = ++s_cnt;
    if (n <= 30 || (n % 500) == 0) DLOG("dump #%u %s (%zu B)", n, path, len);
    return true;
}

typedef int (*luaL_loadbuffer_t2)(lua_State*, const char*, size_t, const char*);
typedef int (*luaL_loadbufferx_t2)(lua_State*, const char*, size_t, const char*, const char*);
static luaL_loadbuffer_t2  g_origLoadBuf  = nullptr;
static luaL_loadbufferx_t2 g_origLoadBufx = nullptr;

static int Hooked_luaL_loadbuffer2(lua_State* L, const char* buff, size_t sz, const char* name) {
    if (name && buff && sz >= 16) YimoWriteDump(name, (const uint8_t*)buff, sz);
    return g_origLoadBuf ? g_origLoadBuf(L, buff, sz, name) : -1;
}

static int Hooked_luaL_loadbufferx2(lua_State* L, const char* buff, size_t sz,
                                   const char* name, const char* mode) {
    if (name && buff && sz >= 16) YimoWriteDump(name, (const uint8_t*)buff, sz);
    return g_origLoadBufx ? g_origLoadBufx(L, buff, sz, name, mode) : -1;
}

static void InstallLuaDumpHook() {
    if (g_origLoadBuf || g_origLoadBufx) return;
    void* h = dlopen(YIMO_LUA_LIB, RTLD_NOW);
    if (!h) h = dlopen(YIMO_LUA_LIB, RTLD_NOLOAD | RTLD_NOW);
    if (!h) return;
    void* s1 = dlsym(h, "luaL_loadbuffer");
    if (s1 && DobbyHook(s1, (void*)Hooked_luaL_loadbuffer2, (void**)&g_origLoadBuf) == 0)
        DLOG("lua dump hook: luaL_loadbuffer @ %p", s1);
    void* s2 = dlsym(h, "luaL_loadbufferx");
    if (s2 && DobbyHook(s2, (void*)Hooked_luaL_loadbufferx2, (void**)&g_origLoadBufx) == 0)
        DLOG("lua dump hook: luaL_loadbufferx @ %p", s2);
}

/* 诊断复读：让 Lua 把「这个 lua_State 眼里」的关键对象/开关值格式化成**字符串返回**，
 * 由 native 用 DLOG 打到 logcat（Lua 侧 io.open 写 /sdcard 在 Android 14 无权限，2026-10-29 实测写不进去）。
 * 判据用途：
 *   ① YM 值没写进来（写开关落到别的 state / 写失败）
 *   ② 值对但挂点变了（ACA / ATTR / loadadj 类型异常）
 *   ③ 值与类都对，但游戏不走这个入口 */
static const char kYimoStateProbe[] = R"LUA(
local G, rg = _G, rawget
local Y  = rg(G, "YM") or {}
Y.diag = true
local PM = rg(G, "pg")
local me = (type(PM) == "table") and rg(PM, "me") or nil
local pw = (type(PM) == "table") and rg(PM, "pawn") or nil
local ACA  = package.loaded["Common.Ability.Attribute.ActorCombatAttribute"]
local ATTR = package.loaded["Common.Const.AttributeConst"]
local gv  = (type(ACA) == "table") and rg(ACA, "getAttribRatioValue") or nil
local la  = (type(ATTR) == "table") and rg(ATTR, "speed_ratio_load_adjustment_v") or nil
local parts = {}
parts[#parts+1] = string.format("sw dmg=%s spd=%s inv=%s jump=%s stam=%s tp=%s seq=%s",
    tostring(Y.dmg), tostring(Y.spd), tostring(Y.inv), tostring(Y.jump),
    tostring(Y.stam), tostring(Y.tp), tostring(rg(G, "__YM_SEQ")))
if type(me) == "table" and type(gv) == "function" and la ~= nil then
  local a = rg(me, "actorCombatAttribute") or me
  local keep = Y.spd
  Y.spd = 1; local _, v1 = pcall(gv, a, la)
  Y.spd = keep; local _, v2 = pcall(gv, a, la)
  parts[#parts+1] = string.format("spdAB spd1=%s spd%s=%s laMine=%s laOther=%s",
      tostring(v1), tostring(keep), tostring(v2), tostring(Y.laMine), tostring(Y.laOther))
end
local gh = Y.gvH
if type(gh) == "table" then
  local arr = {}
  for k, n in pairs(gh) do arr[#arr+1] = { k = k, n = n } end
  table.sort(arr, function(x, y) return x.n > y.n end)
  local top = {}
  for i = 1, math.min(6, #arr) do top[#top+1] = arr[i].k .. "x" .. arr[i].n end
  parts[#parts+1] = "gvH[" .. table.concat(top, ",") .. "]"
end
local rh = Y.rawH
if type(rh) == "table" then
  local arr = {}
  for k, n in pairs(rh) do arr[#arr+1] = { k = k, n = n } end
  table.sort(arr, function(x, y) return x.n > y.n end)
  local top = {}
  for i = 1, math.min(6, #arr) do top[#top+1] = arr[i].k .. "x" .. arr[i].n end
  parts[#parts+1] = "rawH[" .. table.concat(top, ",") .. "]"
end
-- 运动组件持有者勘察：1.0.1 的 ClientMotionComponent 在 1.1.0 挂在谁身上
do
  local probe = {}
  local cands = { pawn = pw, me = me,
                  emodel = (type(pw) == "table") and rg(pw, "eModel") or nil,
                  ctrl = (type(pw) == "table") and rg(pw, "controller") or nil }
  for name, obj in pairs(cands) do
    if type(obj) == "table" then
      local ks = {}
      for k, v in pairs(obj) do
        if type(k) == "string" then
          local lk = k:lower()
          if lk:find("motion") or lk:find("speed") or lk:find("component") then
            ks[#ks+1] = k .. ":" .. type(v)
            if #ks >= 6 then break end
          end
        end
      end
      table.sort(ks)
      probe[#probe+1] = name .. "{" .. table.concat(ks, ",") .. "}"
    else
      probe[#probe+1] = name .. "=" .. type(obj)
    end
  end
  parts[#parts+1] = "OWNER[" .. table.concat(probe, " ") .. "]"
end
if type(pw) == "table" then
  local CC = package.loaded["Common.Const.Const"]
  parts[#parts+1] = string.format("API getComponent=%s COMPONENT_MOTION=%s",
      type(rg(pw, "getComponent")),
      tostring(type(CC) == "table" and rg(CC, "COMPONENT_MOTION") or nil))
end
local ah = Y.avH
if type(ah) == "table" then
  local arr = {}
  for k, n in pairs(ah) do arr[#arr+1] = { k = k, n = n } end
  table.sort(arr, function(x, y) return x.n > y.n end)
  local top = {}
  for i = 1, math.min(6, #arr) do top[#top+1] = arr[i].k .. "x" .. arr[i].n end
  parts[#parts+1] = "avH[" .. table.concat(top, ",") .. "]"
end
if type(pw) == "table" then
  local out = {}
  for k, v in pairs(pw) do
    if type(k) == "string" then
      local lk = k:lower()
      if lk:find("speed") or lk:find("motion") then
        out[#out+1] = k .. "=" .. tostring(v)
        if #out >= 12 then break end
      end
    end
  end
  parts[#parts+1] = "PAWN[" .. table.concat(out, ",") .. "]"
end
local au = package.loaded["Common.Utils.AbilityUtils"]
local mtm = (type(au) == "table") and rg(au, "invalidTargetMap") or nil
local ntm = 0
if type(mtm) == "table" then for _ in pairs(mtm) do ntm = ntm + 1 end end
local tgt = pw or me
local isinv = "n/a"
if type(au) == "table" and type(rg(au, "isAbilityInvalidTarget")) == "function" and type(tgt) == "table" then
  local ok, v = pcall(rg(au, "isAbilityInvalidTarget"), tgt)
  isinv = tostring(ok and v or "err")
end
parts[#parts+1] = string.format("INV mapN=%s isInv=%s ts_pawn=%s ts_me=%s ait=%s clr=%s",
    tostring(ntm), isinv,
    tostring(type(pw) == "table" and rg(pw, "dialoguePauseInvincibleEndTs") or nil),
    tostring(type(me) == "table" and rg(me, "dialoguePauseInvincibleEndTs") or nil),
    tostring(Y.ait), tostring(Y.clr))
parts[#parts+1] = string.format("CMC hits=%s mine=%s scaled=%s", tostring(Y.cmcHits),
    tostring(Y.cmcMine), tostring(Y.cmcScaled))
parts[#parts+1] = string.format("inv cvt=%s inv=%s block=%s wrap=%s ait=%s aitErr=%s motComp=%s motPawn=%s",
    tostring(Y.cvt), tostring(Y.cvtInv), tostring(Y.cvtBlock), tostring(Y.cvtWrap),
    tostring(Y.ait), tostring(Y.aitErr), tostring(Y.motComp), tostring(Y.motPawn))
parts[#parts+1] = string.format("dmg calls=%s out=%s mine=%s", tostring(Y.calls),
    tostring(Y.callsOut), tostring(Y.callsMine))
return table.concat(parts, " || ")
)LUA";

/* 主题探测：把类/实例上「像目标过滤、像伤害上报、像速度刷新」的键名列出来，
 * 供精确定位 1.1.0 的真实挂点（每行 < 1 KB，避免 liblog 截断）。 */
static const char kYimoSurveyMe[] = R"LUA(
local G, rg = _G, rawget
local PM = rg(G, "pg")
local me = (type(PM) == "table") and rg(PM, "me") or nil
if type(me) ~= "table" then return "me=nil" end
local KEEP = { "target", "immune", "invinc", "damage", "hurt", "hit", "attack" }
local out = {}
local n = 0
for k, v in pairs(me) do
  if type(k) == "string" then
    local lk = k:lower()
    for _, w in ipairs(KEEP) do
      if lk:find(w, 1, true) then
        out[#out+1] = k .. ":" .. type(v); n = n + 1; break
      end
    end
  end
  if n > 28 then break end
end
return "ME[" .. table.concat(out, ",") .. "]"
)LUA";

static const char kYimoSurveyCls[] = R"LUA(
local G, rg = _G, rawget
local PM = rg(G, "pg")
local me = (type(PM) == "table") and rg(PM, "me") or nil
if type(me) ~= "table" then return "me=nil" end
local cls = nil
if type(rg(me, "getVtbl")) == "function" then
  local ok, v = pcall(rg(me, "getVtbl"), me)
  if ok then cls = v end
end
if type(cls) ~= "table" then cls = me end
local KEEP = { "target", "immune", "invinc", "damage", "hurt", "msg", "send", "report", "rpc" }
local out, n = {}, 0
for k, v in pairs(cls) do
  if type(k) == "string" then
    local lk = k:lower()
    for _, w in ipairs(KEEP) do
      if lk:find(w, 1, true) then out[#out+1] = k .. ":" .. type(v); n = n + 1; break end
    end
  end
  if n > 34 then break end
end
return "CLS[" .. table.concat(out, ",") .. "]"
)LUA";

static const char kYimoSurveyNet[] = R"LUA(
local out = {}
for _, mn in ipairs({ "Utils.LuaMsgUtils", "Core.Net.Client" }) do
  local m = package.loaded[mn]
  if type(m) == "table" then
    local ks = {}
    for k, v in pairs(m) do
      if type(v) == "function" then ks[#ks+1] = k end
      if #ks > 14 then break end
    end
    table.sort(ks)
    out[#out+1] = mn .. "{" .. table.concat(ks, ",") .. "}"
  else
    out[#out+1] = mn .. "=" .. type(m)
  end
end
return "NET[" .. table.concat(out, " ") .. "]"
)LUA";

static const char kYimoSurveyUtil[] = R"LUA(
local out = {}
for _, mn in ipairs({ "Common.Utils.Utils", "Utils.Utils" }) do
  local m = package.loaded[mn]
  if type(m) == "table" then
    local ks = {}
    for k, v in pairs(m) do
      if type(k) == "string" then
        local lk = k:lower()
        if lk:find("target", 1, true) or lk:find("invalid", 1, true)
           or lk:find("enemy", 1, true) or lk:find("immune", 1, true) then
          ks[#ks+1] = k .. ":" .. type(v)
          if #ks > 12 then break end
        end
      end
    end
    table.sort(ks)
    out[#out+1] = mn .. "{" .. table.concat(ks, ",") .. "}"
  else
    out[#out+1] = mn .. "=" .. type(m)
  end
end
return "UTIL[" .. table.concat(out, " ") .. "]"
)LUA";

static const char kYimoSurveyMod[] = R"LUA(
local G, rg = _G, rawget
local out, n = {}, 0
for name, v in pairs(package.loaded) do
  if type(name) == "string" and type(v) == "table" then
    local ln = name:lower()
    if ln:find("rpc", 1, true) or ln:find("msg", 1, true) or ln:find("hub", 1, true)
       or ln:find("net", 1, true) then
      out[#out+1] = name; n = n + 1
      if n > 12 then break end
    end
  end
end
return "MOD[" .. table.concat(out, ",") .. "]"
)LUA";

/* 诊断 chunk（占位保持顺序）：独立、一次性、失败绝不影响功能补丁。 */

/* 诊断：跑一段返回字符串的 Lua，把结果拷进 out（失败返回 false，绝不影响功能） */
static bool RunLuaStrRet(lua_State* L, const char* code, char* out, int cap) {
    if (out && cap > 0) out[0] = 0;
    if (!p_luaL_loadstring || !p_lua_pcall || !p_lua_tolstring || !p_lua_settop) return false;
    if (p_luaL_loadstring(L, code) != 0) { p_lua_settop(L, -2); return false; }
    if (p_lua_pcall(L, 0, 1, 0) != 0) { p_lua_settop(L, -2); return false; }
    const char* s = p_lua_tolstring(L, -1, nullptr);
    if (s && out && cap > 0) snprintf(out, (size_t)cap, "%s", s);
    p_lua_settop(L, -2);
    return s != nullptr;
}

/* 诊断 chunk：独立、一次性、失败绝不影响功能补丁。仅 STARRY_DIAG=1 构建存在。 */
static void RunDiagChunk(lua_State* L) {
    static bool s_diagDone = false;
    if (s_diagDone) return;
    if (!RunLua(L, MOD_DIAG_LUA_TEXT)) return;   // 失败下次同步再试（不抛错）
    s_diagDone = true;
}
#endif

/* ============================================================
 * 移速（原生 / il2cpp 路线）—— 正式包实现
 *
 * 原理（2026-10-30 日志证明，见 10_docs/伊莫/移速_原生路线_侦察_20261029.md §13/§14）：
 *   MotionComponent::SetVelocity(this, int kind, Vector3 v) 会把 v 写进
 *   "目标速度表" this+0x1E8[kind*12 + 0x20]（反汇编 +0054 stp s0,s1,[x10,#0x20]
 *   / +0058 str s2,[x10,#0x28]）。把 v 乘 N 再调原函数 ⇒ 玩家移动真的 N 倍
 *   （实测 in|4.014| → out|24.087|，下一帧 UpdateVelocity 的 arg1 幅值升到 11.9~16）。
 *
 * 只挂 2 个点，先验序言指纹；不匹配就静默退化（移速无效，绝不崩）：
 *   · MotionComponent.SetVelocity               RVA 0x04351a88  ← 功能
 *   · CharacterControllerComponent.OnHandleMove RVA 0x043079e0  ← 认主
 * 认主链：OnHandleMove 的 this(CCC) +0x18 → entity；与 MotionComponent +0x18 比对，
 *         命中即锁定（先到先得，之后不再改）。
 * ============================================================ */
#define YIMO_SPD_RVA_SETVEL   0x04351a88u
#define YIMO_SPD_RVA_ONHANDLE 0x043079e0u
static const uint32_t kYimoSpdFpSetVel[4]   = {0xD10283FFu, 0xFD002BEAu, 0x6D0623E9u,
                                               0xA9075FFEu};
static const uint32_t kYimoSpdFpOnHandle[4] = {0x6DBA33EDu, 0x6D012BEBu, 0x6D0223E9u,
                                               0xF9001BFEu};

struct YimoSpdVec3 { float x, y, z; };
static uint64_t (*g_yimoOrigSetVel)(void*, int, YimoSpdVec3) = nullptr;
static void*    (*g_yimoOrigOnHandle)(void*, void*, void*) = nullptr;
static std::atomic<void*>     g_yimoPlayerMotion{nullptr};
static std::atomic<void*>     g_yimoPlayerEntity{nullptr};
static std::atomic<uint32_t>  g_yimoSpdHookMask{0};   /* bit0=SetVelocity bit1=OnHandleMove */
static std::atomic<bool>      g_yimoSpdAnchor{false};

static bool YimoSpdPtrOk(const void* p) {
    const uintptr_t v = (uintptr_t)p;
    /* 8 字节对齐 + 落在用户态范围（本机堆地址形如 0x4000_xxxx_xxxx） */
    return v > 0x10000ull && (v & 7) == 0 && v < 0x7fffffffffffULL;
}

static void* YimoSpdEntOf(const void* comp) {
    void* e = nullptr;
    if (comp) memcpy(&e, (const uint8_t*)comp + 0x18, sizeof(e));
    return YimoSpdPtrOk(e) ? e : nullptr;
}

/* 功能点：只对已锁定主角、只对移动档（kind==1），把目标速度三轴乘 mul */
static uint64_t YimoSpdHookSetVel(void* self, int kind, YimoSpdVec3 v) {
    if (!g_yimoOrigSetVel) return 0;
    const int mul = g_forceOff.load(std::memory_order_relaxed)
                        ? 1
                        : g_spdMult.load(std::memory_order_relaxed);
    void* pm = g_yimoPlayerMotion.load(std::memory_order_acquire);
    if (!pm) {
        /* entity 已知但 MotionComponent 未锁定 ⇒ 用 +0x18 对撞认领 */
        const void* want = g_yimoPlayerEntity.load(std::memory_order_acquire);
        if (want && self && YimoSpdEntOf(self) == want) {
            g_yimoPlayerMotion.store(self, std::memory_order_release);
            pm = self;
#if MOD_DIAG_LUA
            g_playerMotion = self;      /* 诊断侧共用同一份认主结果 */
            DLOG("spd OWNER 锁定主角 MotionComponent=%p（entity=%p 比对一致）", self, want);
#endif
        }
    }
    if (!pm || self != pm || kind != 1 || mul <= 1) {
#if MOD_DIAG_LUA
        /* 诊断：把"为什么没放大"打清楚（前 3 条 + 每 2 s 一条） */
        static uint64_t s_skipMs = 0;
        static std::atomic<int> s_skipN{0};
        struct timespec ts0{};
        clock_gettime(CLOCK_MONOTONIC, &ts0);
        const uint64_t nowMs0 = (uint64_t)ts0.tv_sec * 1000ull + ts0.tv_nsec / 1000000ull;
        if (s_skipN.load() < 3 || nowMs0 - s_skipMs >= 2000ull) {
            s_skipMs = nowMs0;
            s_skipN.fetch_add(1);
            DLOG("spd SETV 跳过 this=%p locked=%p kind=%d mul=%d in=(%.3f,%.3f,%.3f)",
                 self, pm, kind, mul, v.x, v.y, v.z);
        }
#endif
        return g_yimoOrigSetVel(self, kind, v);        /* 完全原样透传 */
    }
    /* 反野指针：每次校验 +0x18 仍是那个 entity，不一致就解锁并停止写入 */
    const void* want = g_yimoPlayerEntity.load(std::memory_order_acquire);
    if (want && YimoSpdEntOf(self) != want) {
        g_yimoPlayerMotion.store(nullptr, std::memory_order_release);
        return g_yimoOrigSetVel(self, kind, v);
    }
    const YimoSpdVec3 out{v.x * (float)mul, v.y * (float)mul, v.z * (float)mul};
    const uint64_t r = g_yimoOrigSetVel(self, kind, out);
    if (!g_yimoSpdAnchor.exchange(true, std::memory_order_relaxed)) {
        LOGI(YIMO_LOG " speed native ok (mul=%d)", mul);      /* 唯一成功锚点 */
    }
#if MOD_DIAG_LUA
    {   /* 诊断：in/out/落库回读（0.5 s 节流） */
        static uint64_t s_lastMs = 0;
        struct timespec ts{};
        clock_gettime(CLOCK_MONOTONIC, &ts);
        const uint64_t nowMs = (uint64_t)ts.tv_sec * 1000ull + ts.tv_nsec / 1000000ull;
        if (nowMs - s_lastMs >= 500ull) {
            s_lastMs = nowMs;
            const float li = sqrtf(v.x * v.x + v.y * v.y + v.z * v.z);
            const float lo = sqrtf(out.x * out.x + out.y * out.y + out.z * out.z);
            float st[3] = {0.0f, 0.0f, 0.0f};
            bool stOk = false;
            void* tab = nullptr;
            memcpy(&tab, (const uint8_t*)self + 0x1E8, sizeof(tab));
            if (YimoSpdPtrOk(tab)) {
                int cnt = 0;
                memcpy(&cnt, (const uint8_t*)tab + 0x18, sizeof(cnt));
                if (kind >= 0 && kind < cnt) {
                    memcpy(st, (const uint8_t*)tab + kind * 12 + 0x20, sizeof(st));
                    stOk = true;
                }
            }
            DLOG("spd SETV 主角 this=%p kind=%d mul=%d in=(%.3f,%.3f,%.3f) |in|=%.3f "
                 "out=(%.3f,%.3f,%.3f) |out|=%.3f stored=%s(%.3f,%.3f,%.3f)",
                 self, kind, mul, v.x, v.y, v.z, li, out.x, out.y, out.z, lo,
                 stOk ? "Y" : "N", st[0], st[1], st[2]);
        }
    }
#endif
    return r;
}

/* 认主点：只有本地玩家会处理"按键移动"，取它的 entity */
static void* YimoSpdHookOnHandle(void* self, void* a1, void* a2) {
    if (!g_yimoOrigOnHandle) return nullptr;
    if (self && !g_yimoPlayerEntity.load(std::memory_order_acquire)) {
        void* ent = YimoSpdEntOf(self);
        if (ent) {
            g_yimoPlayerEntity.store(ent, std::memory_order_release);
#if MOD_DIAG_LUA
            g_playerEntity = ent;   /* 诊断侧共用 */
            DLOG("spd OWNER2(OHM) 记录玩家 entity=%p（来自 ccc=%p）", ent, self);
#endif
        }
    }
    return g_yimoOrigOnHandle(self, a1, a2);
}

/* libil2cpp 基址（maps 里 offset==0 的那条映射）
 * ★坑：必须**逐行**判断文件名。早先用 strstr(p,"libil2cpp.so") 跨行匹配，
 *   某条 offset==0 的其它映射（APK/houdini 等）也会命中 ⇒ 返回错误基址 ⇒
 *   指纹永远不符 ⇒ 静默不挂（正式包"完全没效果"就是这个原因）。 */
static uintptr_t YimoSpdSoBase() {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return 0;
    static char buf[768 * 1024];        /* 实测该进程 maps ≈380 KB，别再用 96 KB */
    const size_t got = fread(buf, 1, sizeof(buf) - 1, f);
    fclose(f);
    buf[got] = '\0';
    const char* p = buf;
    while (p && *p) {
        const char* nl = strchr(p, '\n');
        const size_t len = nl ? (size_t)(nl - p) : strlen(p);
        bool hit = false;
        for (size_t i = 0; i + 12 <= len; ++i) {
            if (memcmp(p + i, "libil2cpp.so", 12) == 0) { hit = true; break; }
        }
        if (hit) {
            uintptr_t a = 0, off = 0;
            if (sscanf(p, "%lx-%*lx %*s %lx", &a, &off) == 2 && off == 0) return a;
        }
        if (!nl) break;
        p = nl + 1;
    }
    return 0;
}

/* 后台安装：il2cpp 代码页可能尚未解密 ⇒ 指纹不符就退避重试（最多约 2 分钟） */
static void YimoSpdInstallWorker() {
    for (int i = 0; i < 60; ++i) {
        const uintptr_t base = YimoSpdSoBase();
        if (base) {
            if (!(g_yimoSpdHookMask.load(std::memory_order_relaxed) & 1)) {
                uint32_t w[4] = {0, 0, 0, 0};
                memcpy(w, (const void*)(base + YIMO_SPD_RVA_SETVEL), sizeof(w));
                if (w[0] == kYimoSpdFpSetVel[0] && w[1] == kYimoSpdFpSetVel[1] &&
                    w[2] == kYimoSpdFpSetVel[2] && w[3] == kYimoSpdFpSetVel[3]) {
                    if (DobbyHook((void*)(base + YIMO_SPD_RVA_SETVEL),
                                  (void*)YimoSpdHookSetVel, (void**)&g_yimoOrigSetVel) == 0) {
                        g_yimoSpdHookMask.fetch_or(1, std::memory_order_relaxed);
                        DLOG("speed hook SetVelocity ok orig=%p", (void*)g_yimoOrigSetVel);
                    }
                }
            }
            if (!(g_yimoSpdHookMask.load(std::memory_order_relaxed) & 2)) {
                uint32_t w[4] = {0, 0, 0, 0};
                memcpy(w, (const void*)(base + YIMO_SPD_RVA_ONHANDLE), sizeof(w));
                if (w[0] == kYimoSpdFpOnHandle[0] && w[1] == kYimoSpdFpOnHandle[1] &&
                    w[2] == kYimoSpdFpOnHandle[2] && w[3] == kYimoSpdFpOnHandle[3]) {
                    if (DobbyHook((void*)(base + YIMO_SPD_RVA_ONHANDLE),
                                  (void*)YimoSpdHookOnHandle,
                                  (void**)&g_yimoOrigOnHandle) == 0) {
                        g_yimoSpdHookMask.fetch_or(2, std::memory_order_relaxed);
                        DLOG("speed hook OnHandleMove ok orig=%p", (void*)g_yimoOrigOnHandle);
                    }
                }
            }
            if (g_yimoSpdHookMask.load() == 3) return;
        }
        struct timespec ts{2, 0};
        nanosleep(&ts, nullptr);
    }
    DLOG("speed hooks 未全部装上（指纹不符或代码页未解密）⇒ 移速退化为无效");
}

static void SyncOnLua(lua_State* L) {
    if (!g_luaReady.load() || !L) return;
    if (!IsTableGlobal(L, "pg")) {
        /* 诊断：pg 不是 table 时原本完全静默（正式包也一样），
         * 排查「补丁没装上」必须能区分「hook 没触发/没装上」与「pg 名变了」。 */
        static uint64_t s_lastPgLog = 0;
        struct timespec ts0;
        clock_gettime(CLOCK_MONOTONIC, &ts0);
        const uint64_t nowMs0 = (uint64_t)ts0.tv_sec * 1000ull + (uint64_t)ts0.tv_nsec / 1000000ull;
        if (s_lastPgLog == 0 || (nowMs0 - s_lastPgLog) >= 5000) {
            s_lastPgLog = nowMs0;
            DLOG("pg gate miss state=%p hookCalls=%u", (void*)L,
                 (unsigned)g_hookCalls.load(std::memory_order_relaxed));
        }
        return;   // 游戏环境未就绪，下次再试
    }

    /* ① 守卫式安装：每次同步都跑。游戏换场景/换战斗时若重建了实体类表，
     *    我们的包装会随之丢失，这里自动重新包装（一次性安装会静默失效）。
     *    ★ 必须独立成 chunk：补丁命中守卫时会提前 return，
     *      把它和「写开关」拼在同一个 chunk 里会把写开关整段吃掉（Jjdm 真机踩过）。 */
    if (!RunLua(L, YimoPatchText())) return;

    /* ② 写开关（独立 chunk，绝不能被 ① 的 return 吃掉） */
    const bool off = g_forceOff.load();
    const int dmg = off ? 1 : g_dmgMult.load();
    const int spd = off ? 1 : g_spdMult.load();
    const bool inv  = !off && g_inv.load();
    const bool jump = !off && g_jump.load();
    const bool stam = !off && g_stam.load();
    const bool tp   = !off && g_tp.load();
    char buf[512];
    snprintf(buf, sizeof(buf),
             "local G=_G local Y=rawget(G,\"YM\") "
             "if type(Y)~=\"table\" then Y={} rawset(G,\"YM\",Y) end "
             "Y.dmg=%d Y.spd=%d Y.inv=%s Y.jump=%s Y.stam=%s Y.tp=%s",
             dmg, spd, inv ? "true" : "false", jump ? "true" : "false",
             stam ? "true" : "false", tp ? "true" : "false");
    if (!RunLua(L, buf)) return;

    const int slot = StateSlot(L);
    const uint32_t seqNow = g_syncSeq.load(std::memory_order_relaxed);
    if (slot >= 0) g_stateApplied[slot] = seqNow;

#if MOD_DIAG_LUA
    RunDiagChunk(L);   /* 诊断 chunk（仅诊断构建存在；失败不影响上面两步功能） */
    {
        static uint64_t s_lastProbe = 0;
        struct timespec tsp;
        clock_gettime(CLOCK_MONOTONIC, &tsp);
        const uint64_t nowP = (uint64_t)tsp.tv_sec * 1000ull + (uint64_t)tsp.tv_nsec / 1000000ull;
        if (s_lastProbe == 0 || (nowP - s_lastProbe) >= 3000) {
            s_lastProbe = nowP;
            char got[3000];
            {
                static uint64_t s_lastSurvey = 0;
                if (s_lastSurvey == 0 || (nowP - s_lastSurvey) >= 5000) {
                    s_lastSurvey = nowP;
                    char sv[1200];
                    if (RunLuaStrRet(L, kYimoSurveyMe, sv, (int)sizeof(sv)))  DLOG("survey %s", sv);
                    if (RunLuaStrRet(L, kYimoSurveyCls, sv, (int)sizeof(sv))) DLOG("survey %s", sv);
                    if (RunLuaStrRet(L, kYimoSurveyMod, sv, (int)sizeof(sv))) DLOG("survey %s", sv);
                    if (RunLuaStrRet(L, kYimoSurveyNet, sv, (int)sizeof(sv)))  DLOG("survey %s", sv);
                    if (RunLuaStrRet(L, kYimoSurveyUtil, sv, (int)sizeof(sv))) DLOG("survey %s", sv);
                }
            }
            if (RunLuaStrRet(L, kYimoStateProbe, got, (int)sizeof(got))) {
                DLOG("probe slot=%d st=%p calls=%u seq=%u | %s", slot, (void*)L,
                     (unsigned)g_hookCalls.load(std::memory_order_relaxed),
                     (unsigned)seqNow, got);
            } else {
                DLOG("probe failed slot=%d st=%p", slot, (void*)L);
            }
        }
    }
#endif

    if (!g_patched) {
        g_patched = true;
        /* 全包唯一一条成功日志：既是装机验证锚点，也便于用户反馈问题时确认补丁已生效 */
        LOGI(YIMO_LOG " patch installed (state=%p)", (void*)L);
        DLOG("yimo patch installed state=%p patchLen=%d", (void*)L,
             (int)strlen(YimoPatchText()));
    }
    g_wantSync.store(false, std::memory_order_relaxed);

    /* ③ 每秒一次的 Lua 侧维护（清重发预算 / 无敌时间戳 / 速度刷新 / 补体力） */
    static uint64_t s_tickMs = 0;
    struct timespec ts2;
    clock_gettime(CLOCK_MONOTONIC, &ts2);
    const uint64_t nowMs2 = (uint64_t)ts2.tv_sec * 1000ull + (uint64_t)ts2.tv_nsec / 1000000ull;
    if ((nowMs2 - s_tickMs) >= 1000) {
        s_tickMs = nowMs2;
        RunLua(L, kYimoTickChunk);
    }
}

/* 所有 Lua VM 入口共用：进入 hook 时 VM 处于稳定边界，可安全嵌套一次 pcall */
static inline uint64_t YimoNowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (uint64_t)ts.tv_sec * 1000ull + (uint64_t)ts.tv_nsec / 1000000ull;
}

static inline void SyncFromLuaEntry(lua_State* L) {
    if (g_inHook.exchange(true, std::memory_order_acq_rel)) return;
    /* ★游戏可能有多套 lua_State（战斗 VM ≠ 加载 VM）：补丁装在 A 上、战斗跑在 B 上
     *  会表现为「装上了但没效果」。这里只靠 g_wantSync 驱动，每个 state 各自同步。 */
    const uint32_t c = g_hookCalls.fetch_add(1, std::memory_order_relaxed);

    bool want = g_wantSync.load(std::memory_order_relaxed);
    if (!g_patched) {
        /* ★性能红线：补丁没装上之前，绝不能每次 Lua C 调用都跑一遍补丁 chunk
         *   —— kYimoPatch 有 ~19 KB，luaL_loadstring 每次都要完整重新解析。
         *   实测 lua_gettop/lua_pcall 约 470 次/秒，限流前吃掉约 16% 单核
         *   （登录界面空转 64.6% 单核，主要就是这里）。
         *   改为每 500 ms 只尝试一次；`pg.me` 一出现照样在 0.5 s 内装上。 */
        static uint64_t s_lastTryMs = 0;
        const uint64_t nowMs = YimoNowMs();
        if (c >= 20 && (nowMs - s_lastTryMs) >= 500) { s_lastTryMs = nowMs; want = true; }
    }
#if MOD_DIAG_LUA
    /* 诊断构建：补丁装上后 g_wantSync 会被清掉、SyncOnLua 不再被调用，
     * 探针就哑了 ⇒ 每 3 s 强制同步一次（仅诊断包）。 */
    if (!want) {
        static uint64_t s_lastForce = 0;
        const uint64_t nowMsF = YimoNowMs();
        if (s_lastForce == 0 || (nowMsF - s_lastForce) >= 3000) { s_lastForce = nowMsF; want = true; }
    }
#endif
    if (want) {
        /* ★整体同步限流：最多每 100 ms 一次（Yimo_Tick 每个框架 tick 都会把
         *   g_wantSync 置真，不限流就是每 tick 一次 19 KB loadstring）。 */
        static uint64_t s_lastSyncMs = 0;
        const uint64_t nowMs2 = YimoNowMs();
        if ((nowMs2 - s_lastSyncMs) >= 100) {
            s_lastSyncMs = nowMs2;
            SyncOnLua(L);
        }
    }
    g_inHook.store(false, std::memory_order_release);
}

static int Hook_lua_gettop(lua_State* L) {
    SyncFromLuaEntry(L);
    return g_origGettop ? g_origGettop(L) : 0;
}

static int Hook_lua_pcall(lua_State* L, int nargs, int nres, int errfunc) {
    SyncFromLuaEntry(L);
    return (g_origPcall ? g_origPcall : p_lua_pcall)(L, nargs, nres, errfunc);
}

static bool InstallLuaHooks() {
    if (g_hooksInstalled) return true;

    /* 只 hook 本体 >= 16 字节的入口；并做序言指纹校验（漂移即拒挂） */
    if (!(p_lua_pcall && FingerprintOk((void*)p_lua_pcall, kPcallFp) &&
          DobbyHook((void*)p_lua_pcall, (void*)Hook_lua_pcall, (void**)&g_origPcall) == 0)) {
        LOGE(YIMO_LOG " hook lua_pcall 失败（fp=%d）", (int)FingerprintOk((void*)p_lua_pcall, kPcallFp));
    }

    if (!(p_lua_gettop && FingerprintOk((void*)p_lua_gettop, kGetTopFp) &&
          DobbyHook((void*)p_lua_gettop, (void*)Hook_lua_gettop, (void**)&g_origGettop) == 0)) {
        LOGW(YIMO_LOG " hook lua_gettop 未成功（fp=%d）—— lua_pcall 仍可用",
             (int)FingerprintOk((void*)p_lua_gettop, kGetTopFp));
    }

    if (!g_origPcall && !g_origGettop) {
        LOGE(YIMO_LOG " lua hook 全部失败");
        return false;
    }
    DLOG("hooks installed pcall_orig=%p gettop_orig=%p",
         (void*)g_origPcall, (void*)g_origGettop);
#if MOD_DIAG_LUA
    InstallLuaDumpHook();   /* 诊断包：把游戏加载的 Lua 模块落盘，用于定挂点 */
    /* metadata 解密发生在 libil2cpp 起来之后：等 8 s 再扒一次（只诊断包） */
    static std::atomic<bool> s_metaTried{false};
    if (!s_metaTried.exchange(true)) {
        /* 2026-10-29 改造：由「6 次 × 30 s 静默重试」换成带心跳/统计/命令文件的
         * 采集线程（见 YimoMetaWorker）。它自己会打入口心跳 + 每轮统计，
         * 所以这里不再需要 access() 探针判断跑没跑。 */
        std::thread([] { YimoMetaWorker(); }).detach();
    }
#endif
    /* 移速（原生）后台安装线程：正式包与诊断包同一套代码（两点：SetVelocity + OnHandleMove），
     * 指纹不符/代码页未解密就静默退化（最多重试约 2 分钟）。 */
    static std::atomic<bool> s_spdTried{false};
    if (!s_spdTried.exchange(true)) {
        std::thread([] { YimoSpdInstallWorker(); }).detach();
    }
    g_hooksInstalled = true;
    g_ready.store(true);
    BumpSync();
    return true;
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约）
// ============================================================
bool Yimo_TryInit(JavaVM* /*vm*/) {
    if (g_ready.load()) return true;
    if (!LoadLuaApi()) return false;   // libxlua.so 未起：交给重试 / Tick 懒加载
    return InstallLuaHooks();
}

bool Yimo_IsReady() { return g_ready.load(); }

void Yimo_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case YIMO_FEAT_INV:  g_inv.store(enabled);  break;
        case YIMO_FEAT_JUMP: g_jump.store(enabled); break;
        case YIMO_FEAT_STAM: g_stam.store(enabled); break;
        case YIMO_FEAT_TP:   g_tp.store(enabled);   break;
        case YIMO_FEAT_DMG:  g_dmgMult.store(enabled ? kKillResend : 1); break;  // 秒杀：开关直实现
        case YIMO_FEAT_SPD:  break;
        default: break;
    }
    BumpSync();
}

void Yimo_OnValueChanged(int featNum, int value) {
    if (featNum == YIMO_FEAT_DMG) {
        // ① 精灵攻击秒杀已由 Toggle 直实现（见 OnFeatureChanged）；
        //    旧菜单缓存若还按滑块回调带值过来，一律忽略，避免把开关状态改回去。
    } else if (featNum == YIMO_FEAT_SPD) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        g_spdMult.store(value);
        BumpSync();
    }
}

void Yimo_Tick(const bool* features, const int* values) {
    /* ★2026-10-29 实测补丁：菜单改了开关/数值，Lua 侧却一直是旧值
     *   （logcat: 菜单 #3 玩家无敌 ON / 移速 10，而 Lua 里 YM.inv=false、YM.spd=6）。
     *   原因：写开关只发生在 SyncOnLua 里，而 SyncOnLua 由 g_wantSync 驱动；
     *   实时点菜单走的是「每 tick 传 features[]/values[]」这条路，过去不改 wantSync
     *   ⇒ 只有「初次装载 via OnFeatureChanged」那一次会写，之后菜单怎么动都不同步。
     *   修法：Tick 里对比上一轮的开关/数值，有变化就 BumpSync()（序号+1，各 state 补写）。 */
    {
        static bool sLastF[16] = {false};
        static int  sLastV[16] = {0};
        static bool sInit = false;
        if (features) {
            bool changed = !sInit;
            for (int i = 0; i < 16; ++i) {
                if (features[i] != sLastF[i]) { sLastF[i] = features[i]; changed = true; }
            }
            if (values) {
                for (int i = 0; i < 16; ++i) {
                    if (values[i] != sLastV[i]) { sLastV[i] = values[i]; changed = true; }
                }
            }
            if (changed) {
                sInit = true;
                DLOG("tick detected feature change → BumpSync");
                BumpSync();
            }
        }
    }
    // initRetryMax=0 → 框架不会调 tryInit（"Tick 懒加载"合同），必须由 Tick 自己补
    if (!g_ready.load()) {
        static int s_lazy = 0;
        if ((s_lazy++ % 2) == 0) Yimo_TryInit(nullptr);  // 每 ~400ms 试一次
    }
    // 只做开关同步；禁止扫堆/遍历对象
    if (features) {
        // ① 是 Toggle：以 Tick 的 features[] 为准（自愈，防漏一次回调就永久不同步）
        g_dmgMult.store(features[YIMO_FEAT_DMG] ? kKillResend : 1);
        g_inv.store(features[YIMO_FEAT_INV]);
        g_jump.store(features[YIMO_FEAT_JUMP]);
        g_stam.store(features[YIMO_FEAT_STAM]);
        g_tp.store(features[YIMO_FEAT_TP]);
    }
    if (values) {
        // ☠不读 values[YIMO_FEAT_DMG]：① 是 Toggle 不是滑块，值由 OnFeatureChanged 直接给
        const int s = values[YIMO_FEAT_SPD];
        if (s >= 1 && s <= 10) g_spdMult.store(s);
    }
    g_wantSync.store(true);
}

void Yimo_ForceClear() {
    g_forceOff.store(true);
    g_dmgMult.store(1);
    g_spdMult.store(1);
    g_inv.store(false);
    g_jump.store(false);
    g_stam.store(false);
    g_tp.store(false);
    g_wantSync.store(true);
}
