// ============================================================================
//  ywtMod.cpp — 鹦鹉团（com.jinsaina.auditionsgo.gw 1.0.7）Lua 注入模块
//
//  通道（方案 b，运行时注入）：hook libxlua.so 的 lua_pcall 拿 lua_State*，
//  首次命中后 luaL_loadstring + lua_pcall 执行 monkey-patch 脚本；返回 0 表示
//  「目标还没 require」，1s 后重试（上限 600 次）。设计见
//  10_docs/鹦鹉团/鹦鹉团_Lua注入落地设计_20261119.md §3/§6/§9。
//
//  本文件刻意不引入 Il2CPP 挂点骨架（template 的 RVA/类名/信号保护区都不需要）：
//    ① 没有 thread_local（模板用 thread_local 会在 x86_64 + -fno-emulated-tls 下
//       链出 undefined symbol: __tls_get_addr；本文件改用 pthread_self 判重入）
//    ② 没有 RVA：全部 dlsym 按名取符号，换版本不会挂错地址
//
//  功能（只两项，禁止自增）：
//    ① 自动跳舞 Toggle → DanceVM.SetCheatMode(2=Prefect) / 关闭回 None(1)
//       走游戏自带的 Auto 家族（AutoNote/AutoHitTap/...），不自造按键循环
//    ② 判定概率 Toggle + 六档权重 SeekBar（默认 60/25/10/3/1/1）+ 保留真实 Miss
//       双路径包装：RhythmMasterVM/DanceRhythmVM/DanceKeyFallingVM.GetJudgeResult
//                   + UI_DanceRhythmMasterMode_Ctrl.OnJudgeResult
//  内部参数不进 UI：per_frame=2、skip=0（本版自动跳舞交给官方 Auto，二者不发往 Lua
//  热路径，仍写进 cfg 表以便后续启用）。
// ============================================================================

#include "ywtMod.h"

#include <atomic>
#include <string>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>
#include <pthread.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"

#define YWT_LUA_LIB "libxlua.so"

// ============================================================================
//  ① Lua C API 原型（全部实测导出，见 06_dumped/鹦鹉团/base/libxlua.so）
//     lua_pcall 0x64fa4 / luaL_loadstring 0x2c524 / lua_createtable 0x28ab0 ...
//     ☠ lua_tointeger / lua_pushcfunction 是宏，未导出 —— 用 lua_tointegerx /
//       lua_pushcclosure(f, 0)
// ============================================================================
struct lua_State;

typedef int         (*luaL_loadstring_t)(lua_State*, const char*);
typedef int         (*lua_pcall_t)(lua_State*, int, int, int);
typedef const char* (*lua_tolstring_t)(lua_State*, int, size_t*);
typedef void        (*lua_settop_t)(lua_State*, int);
typedef int         (*lua_gettop_t)(lua_State*);
typedef void        (*lua_setglobal_t)(lua_State*, const char*);
typedef void        (*lua_pushboolean_t)(lua_State*, int);
typedef void        (*lua_pushinteger_t)(lua_State*, long long);
typedef void        (*lua_pushnumber_t)(lua_State*, double);
typedef int         (*lua_CFunction_t)(lua_State*);
typedef void        (*lua_pushcclosure_t)(lua_State*, lua_CFunction_t, int);
typedef void        (*lua_createtable_t)(lua_State*, int, int);
typedef void        (*lua_setfield_t)(lua_State*, int, const char*);
typedef long long   (*lua_tointegerx_t)(lua_State*, int, int*);
typedef int         (*lua_getglobal_t)(lua_State*, const char*);
typedef int         (*lua_toboolean_t)(lua_State*, int);
// 2026-11-20：模块类表挂在 chunk 的 _ENV upvalue 上（不在 _G），必须从 upvalue 拿
typedef const char* (*lua_getupvalue_t)(lua_State*, int, int);
typedef int         (*lua_getfield_t)(lua_State*, int, const char*);
typedef int         (*lua_istable_t)(lua_State*, int);
typedef int         (*lua_isfunction_t)(lua_State*, int);
typedef void        (*lua_pushvalue_t)(lua_State*, int);

static luaL_loadstring_t   p_luaL_loadstring   = nullptr;
static lua_pcall_t         p_lua_pcall         = nullptr;
static lua_tolstring_t     p_lua_tolstring     = nullptr;
static lua_settop_t        p_lua_settop        = nullptr;
static lua_gettop_t        p_lua_gettop        = nullptr;
static lua_setglobal_t     p_lua_setglobal     = nullptr;
static lua_getglobal_t     p_lua_getglobal     = nullptr;
static lua_toboolean_t     p_lua_toboolean     = nullptr;
static lua_getupvalue_t    p_lua_getupvalue    = nullptr;
static lua_getfield_t      p_lua_getfield      = nullptr;
static lua_istable_t       p_lua_istable       = nullptr;
static lua_isfunction_t    p_lua_isfunction    = nullptr;
static lua_pushvalue_t     p_lua_pushvalue     = nullptr;
static lua_pushboolean_t   p_lua_pushboolean   = nullptr;
static lua_pushinteger_t   p_lua_pushinteger   = nullptr;
static lua_pushnumber_t    p_lua_pushnumber    = nullptr;
static lua_pushcclosure_t  p_lua_pushcclosure  = nullptr;
static lua_createtable_t   p_lua_createtable   = nullptr;
static lua_setfield_t      p_lua_setfield      = nullptr;
static lua_tointegerx_t    p_lua_tointegerx    = nullptr;

static lua_pcall_t  g_origPcall  = nullptr;   // Dobby 蹦床
static std::atomic<bool> g_apiReady{false};
static std::atomic<bool> g_hooked{false};

// ============================================================================
//  ② 菜单配置（原子快照；只在 Lua 线程写进 _G.__ywt_cfg）
//     ★默认值：judge 关 / 自动跳舞关 / 权重 70-15-8-4-2-1（2026-11-19 用户口径：偏最高档）
// ============================================================================
static std::atomic<bool>     g_autoOn{false};
static std::atomic<int>      g_autoMode{1};    // 0=关 1=Prefect 2=Random（默认 Prefect）
static std::atomic<bool>     g_judgeOn{false};
static std::atomic<int>      g_w[6];           // 六档权重，顺序 SPP/SP/P/G/Good/Miss
static std::atomic<uint32_t> g_cfgSeq{1};      // 配置版本；各 state 按它补写

static const int   kAutoPerFrame = 2;          // 内部参数，不进 UI
static const float kAutoSkip     = 0.0f;       // 内部参数，不进 UI

static struct WInit {
    WInit() {
        // 2026-11-20：滑条保持 0-100 百分比，默认 70/15/8/4/2/1
        const int d[6] = {70, 15, 8, 4, 2, 1};
        for (int i = 0; i < 6; ++i) g_w[i].store(d[i]);
    }
} g_wInit;

// ============================================================================
//  ③ 多 lua_State 跟踪（每个 state 记「已应用序号」，开关一变各自补写）
// ============================================================================
static constexpr int kMaxStates = 16;   // 2026-11-20：实测本作至少 4 个 state，留足余量
static lua_State*  g_states[kMaxStates] = {nullptr};
static uint32_t    g_stateApplied[kMaxStates] = {0};
static std::atomic<int> g_stateCount{0};

static int StateSlot(lua_State* L) {
    const int n = g_stateCount.load(std::memory_order_acquire);
    for (int i = 0; i < n; ++i) {
        if (g_states[i] == L) return i;
    }
    if (n >= kMaxStates) return -1;
    g_states[n] = L;
    g_stateApplied[n] = 0;
    g_stateCount.store(n + 1, std::memory_order_release);
    LOGI("[Ywt] state registered slot=%d L=%p", n, (void*)L);
    return n;
}

// ============================================================================
//  ④ Lua 侧注入脚本（§9 定稿 + 自动跳舞改为 SetCheatMode）
//     约定：native 先写 _G.__ywt_cfg / _G.__ywt_log，脚本只读现读不缓存
// ============================================================================
static const char kYwtPatch[] = R"YWT(
-- @starry_ywt_patch (鹦鹉团 1.0.7) v7 —— 整理版（2026-11-20）
-- 结构：1 cfg  2 判定位  3 env 获取  4 包装 OnJudgeResult  5 自动跳舞  6 统计
-- 判定档采用「累积分布（互斥区间）」：每一档的实际概率 == 设定值，互不稀释。
if rawget(_G, "__ywt_ver") == 7 then return 1 end

local G, rg, rs = _G, rawget, rawset
local type, pcall, math, tostring, string = type, pcall, math, tostring, string
local pairs, table = pairs, table

-- ============ 1. cfg ============
local function cfg() local c = rg(G, "__ywt_cfg"); if type(c) == "table" then return c end end
local function log(m) local f = rg(G, "__ywt_log"); if type(f) == "function" then pcall(f, tostring(m)) end end

local function idx(t, k)
  if type(t) ~= "table" then return nil end
  local ok, v = pcall(function() return t[k] end)
  if ok then return v end
  return nil
end

local function getmod(name)
  local v = idx(G, name)
  if type(v) == "table" then return v end
  local pl = idx(G, "package")
  local L = (type(pl) == "table") and idx(pl, "loaded") or nil
  if type(L) ~= "table" then return nil end
  v = idx(L, name); if type(v) == "table" then return v end
  local tail = "." .. name
  for k, vv in pairs(L) do
    if type(k) == "string" and type(vv) == "table" and string.sub(k, -#tail) == tail then return vv end
  end
  return nil
end

-- ============ 2. 判定档位：**每条滑条显式绑定一个枚举名**（不按下标猜）============
-- 实测 JudgeResultType：Miss=1 Good=2 Great=3 Perfect=4 SuperPerfect=5 SPerfectPlus=6 SPlusAdd=7
local ROWS = {
  { key = "w1", enum = "SuperPerfectPlusPlusAdd", fb = 7, short = "SPP" },
  { key = "w2", enum = "SuperPerfectPlus",        fb = 6, short = "SP+" },
  { key = "w3", enum = "SuperPerfect",            fb = 5, short = "SP"  },
  { key = "w4", enum = "Perfect",                 fb = 4, short = "Pf"  },
  { key = "w5", enum = "Great",                   fb = 3, short = "Gr"  },
  { key = "w6", enum = "Good",                    fb = 2, short = "Gd"  },
}

local function judgeVals()
  local VM = getmod("RhythmMasterVM")
  local JT = VM and idx(VM, "JudgeResultType") or nil
  local vals = {}
  for i = 1, #ROWS do
    local v = (type(JT) == "table") and idx(JT, ROWS[i].enum) or nil
    if type(v) ~= "number" then v = ROWS[i].fb end
    vals[i] = v
  end
  return vals
end

-- 累积分布：r ∈ [1,100]，落在哪个区间就用哪一档；全 0 或 r > sum ⇒ nil（不改写）
-- 6 档全覆盖（含 Good），每档按 ROWS[i].key 读 cfg，按 ROWS[i].enum 取实数
local function pickJudge(c, vals)
  local w, sum = {}, 0
  for i = 1, #ROWS do
    local v = c and c[ROWS[i].key] or 0
    if type(v) ~= "number" or v < 0 then v = 0 end
    if v > 100 then v = 100 end
    w[i] = v
    sum = sum + v
  end
  if sum <= 0 then return nil, 0 end
  local r = math.random(1, 100)
  local acc = 0
  for i = 1, #ROWS do
    acc = acc + w[i]
    if r <= acc then return vals[i], i end
  end
  return nil, 0
end

-- ============ 3. env 获取（唯一有效路径：已知函数的 _ENV upvalue）============
-- ★2026-11-20 回归：光靠函数名不可靠（同名方法可能来自基类/共享模块，_ENV 不是玩法模块的）。
-- 恢复「广撒网」：收集大量函数 → 各自取 _ENV → 逐个检查目标表/目标方法；找不到就把 env 的
-- 前 8 个 key 打出来（限 10 个 env），供人工定位。
local function countKeys(t)
  local ok, n = pcall(function()
    local c, k = 0, nil
    repeat
      k = next(t, k)
      if k ~= nil then c = c + 1 end
    until k == nil or c > 500
    return c
  end)
  if ok then return n end
  return -1
end

local function firstKeys(t, lim)
  local out = {}
  pcall(function()
    local k = nil
    while #out < lim do
      k = next(t, k)
      if k == nil then break end
      if type(k) == "string" then out[#out + 1] = k end
    end
  end)
  return table.concat(out, ",")
end

-- ★2026-11-20 定案：**不要按名字筛 _ENV**。
-- 实测 Page_DancePlay_Ctrl.OnUpdate 的名字 _ENV 指向的是「当前 state 的全局表」（495 keys），
-- 而目标模块环境是另一个表（有保护元表、pairs 会失败）。上一版能命中，正是因为它在遍历所有
-- table 型 upvalue 逐个试。这里返回全部 table 型 upvalue（不筛名字）。
local function envsOf(fn)
  local out = {}
  if type(fn) ~= "function" then return out end
  for i = 1, 12 do
    local nm, v = debug.getupvalue(fn, i)
    if not nm then break end
    if type(v) == "table" then out[#out + 1] = { name = nm, tab = v } end
  end
  return out
end

local function envCands()
  local list = {}
  local function addTable(t, tag)
    if type(t) ~= "table" then return end
    local n = 0
    for k, v in pairs(t) do
      if type(v) == "function" and n < 6 then
        n = n + 1
        list[#list + 1] = { fn = v, name = tag .. "." .. tostring(k) }
      end
    end
  end
  addTable(G, "_G")
  for k, v in pairs(G) do
    if type(k) == "string" and type(v) == "table"
       and (string.find(k, "Dance", 1, true) or string.find(k, "Page_", 1, true)
            or string.find(k, "Rhythm", 1, true)) then
      addTable(v, k)
    end
  end
  return list
end

-- ============ 4. 包装 OnJudgeResult（6 参）============
local CNT, SKIP_N, FIRST = 0, 0, false
local PICK_N = {}

local function wrapCtrl(ctrl, tag)
  if type(ctrl) ~= "table" or rg(ctrl, "__ywt_hooked") then return false end
  local orig = idx(ctrl, "OnJudgeResult")
  if type(orig) ~= "function" then return false end
  local vals = judgeVals()
  ctrl.OnJudgeResult = function(self, x, judgeType, noteType, a4, nid)
    local c = cfg()
    if c and c.judge_on then
      local v, i = pickJudge(c, vals)
      if v ~= nil then
        CNT = CNT + 1
        PICK_N[i] = (PICK_N[i] or 0) + 1
        if not FIRST then
          FIRST = true
          log(string.format("[Ywt] OJR in=%s out=%s w=%d src=%s",
              tostring(judgeType), tostring(v), i, tag))
        end
        judgeType = v
      else
        SKIP_N = SKIP_N + 1
      end
      local n = CNT + SKIP_N
      if n % 50 == 0 then
        log(string.format("[Ywt] OJR count=%d pick=SPP:%d,SP+:%d,SP:%d,Pf:%d,Gr:%d,Gd:%d changed=%d skip=%d",
            n, PICK_N[1] or 0, PICK_N[2] or 0, PICK_N[3] or 0, PICK_N[4] or 0,
            PICK_N[5] or 0, PICK_N[6] or 0, CNT, SKIP_N))
      end
    end
    return orig(self, x, judgeType, noteType, a4, nid)
  end
  pcall(rs, ctrl, "__ywt_hooked", 1)
  rs(G, "__ywt_ojr", tag)
  log("[Ywt] ctrl wrapped OnJudgeResult src=" .. tag)
  return true
end

-- 不同 state 的 env 可能不同 ⇒ 每个候选都试一遍；已包过的类表用 __ywt_hooked 跳过。
-- 返回本轮新包住的数量（>0 即视为本 state 就绪）。
local ENV_LOGGED, ENV_SUM = 0, 0
local APPLIED = {}

-- ★2026-11-20：类表在 chunk 执行**前**就由框架建好（空占位表），
-- 所以「表存在」≠「模块已加载」。判据必须是**方法可用**（OnJudgeResult/JudgeModule 是 function），
-- 且只有真正包上才允许置 done；表在但方法空 ⇒ 打一条 PENDING 并继续每 tick 重试。
local PENDING = {}

-- 在某个 env 里把候选判定表全部找出来（①索引 ②rawget ③next 遍历）
local function findCtrlIn(env)
  local out, seenT = {}, {}
  local t = nil
  pcall(function() t = env.UI_DanceRhythmMasterMode_Ctrl end)
  if type(t) ~= "table" then pcall(function() t = rg(env, "UI_DanceRhythmMasterMode_Ctrl") end) end
  if type(t) == "table" and not seenT[t] then seenT[t] = 1; out[#out + 1] = { t = t, kind = 1 } end
  pcall(function()
    local k, n = nil, 0
    while true do
      k = next(env, k)
      if k == nil or n > 600 then break end
      n = n + 1
      local okv, vv = pcall(function() return env[k] end)
      if okv and type(vv) == "table" and not seenT[vv] then
        if idx(vv, "OnJudgeResult") ~= nil or idx(vv, "JudgeModule") ~= nil then
          seenT[vv] = 1
          out[#out + 1] = { t = vv, kind = 2 }
        end
      end
    end
  end)
  return out
end

local function fnOf(t)
  if type(idx(t, "OnJudgeResult")) == "function" then return "OnJudgeResult" end
  if type(idx(t, "JudgeModule")) == "function" then return "JudgeModule" end
  return nil
end

local function tryWrapIn(env, src)
  local list = findCtrlIn(env)
  for i = 1, #list do
    local fname = fnOf(list[i].t)
    if fname ~= nil then
      log("[Ywt] env HIT src=" .. src .. " keys=" .. countKeys(env) .. " hasCtrl=" .. list[i].kind .. " fn=" .. fname)
      if wrapCtrl(list[i].t, "env:" .. src) then return true end
    end
  end
  if not PENDING[src] then
    PENDING[src] = 1
    local t = list[1] and list[1].t or nil
    log("[Ywt] env PENDING src=" .. src .. " keys=" .. countKeys(env) ..
        " ctrlType=" .. type(t) .. " ojr=" .. (t and type(idx(t, "OnJudgeResult")) or "nil"))
  end
  -- 记进待重试列表：update_fn/OnUpdate 每帧只重查这几个 env（便宜）
  APPLIED.pend = APPLIED.pend or {}
  for i = 1, #APPLIED.pend do
    if APPLIED.pend[i].tab == env then return false end
  end
  APPLIED.pend[#APPLIED.pend + 1] = { tab = env, src = src }
  return false
end

-- 每帧快查：只查「表在但方法还没填充」的那几个 env
local function tryCachedEnvs()
  if not APPLIED.pend then return false end
  for i = 1, #APPLIED.pend do
    if tryWrapIn(APPLIED.pend[i].tab, APPLIED.pend[i].src) then return true end
  end
  return false
end

-- ============ ★2026-11-20 真落点：typeMode.AdjustJudge ============
-- 反汇编 proto#50：OnJudgeResult 里 R2 先被 self.typeMode:AdjustJudge(R2) 的返回值覆盖
-- （[3] CALL / [4] MOVE R2 R6），随机模式再覆盖一次 ⇒ 包 OnJudgeResult 的入参一定会被冲掉。
-- typeMode 是 Mode 实例字段，我们从 OnUpdate(self) 就能拿到，不依赖找类表。
local AJ = { cnt = 0, skip = 0, picks = {}, first = false, obs = 0, norm = 0 }

local function armAdjust(self)
  if type(self) ~= "table" then return false end
  local tm = idx(self, "typeMode")
  if type(tm) ~= "table" then return false end
  if rg(tm, "__ywt_aj") then return true end
  local aj = idx(tm, "AdjustJudge")
  if type(aj) ~= "function" then
    if not APPLIED.ajno then
      APPLIED.ajno = true
      log("[Ywt] AdjustJudge not found on typeMode (keys=" .. countKeys(tm) .. ")")
    end
    return false
  end
  local vals = judgeVals()
  tm.AdjustJudge = function(tm2, judgeType, ...)
    local c = cfg()
    local inType = judgeType
    -- ★2026-11-20 task-5 ② 自动档归一：
    --   117 UI_DanceRhythmMasterMode_Ctrl.AutoHitTap 把判定档硬编码成 7
    --   (SuperPerfectPlusPlusAdd, __117:4237-4240)，而 016/020 的 calcReduceHp 用
    --   judgeType 当 Judge_Basehp 的行键、缺行回退 Miss 行 ⇒ delta≈0 ⇒「自动跳舞不加血」。
    --   自动跳舞开着时把 >=7 的入参降到 6 (SuperPerfectPlus)：与 015 AdjustJudge 对 1..5
    --   的归一结果一致(__015:741-752)，也是手动能打到的最高有效档。
    --   只改「送进原函数」的档位；judge_on 的伪档返回值不动（判定显示仍可高档）。
    if c and c.auto_on and type(judgeType) == "number" and judgeType >= 7 then
      judgeType = 6
      AJ.norm = AJ.norm + 1
    end
    local v = nil
    if c and c.judge_on then
      local vv, i = pickJudge(c, vals)
      v = vv
      if v ~= nil then
        AJ.cnt = AJ.cnt + 1
        AJ.picks[i] = (AJ.picks[i] or 0) + 1
        if not AJ.first then
          AJ.first = true
          log(string.format("[Ywt] AJ pick=%s w=%d in=%s", ROWS[i].enum, i, tostring(inType)))
        end
      else
        AJ.skip = AJ.skip + 1
      end
    end
    -- ★2026-11-20 task-5 ③ 低频观测日志（每 50 次一条；真机验证 auto/judge 与归一结果）
    AJ.obs = AJ.obs + 1
    if AJ.obs % 50 == 0 then
      log(string.format("[Ywt] AJ obs=%d auto=%s judge=%s in=%s use=%s norm=%d pick=%d skip=%d",
          AJ.obs, tostring(c and c.auto_on or false), tostring(c and c.judge_on or false),
          tostring(inType), tostring(judgeType), AJ.norm, AJ.cnt, AJ.skip))
    end
    -- ★2026-11-20 task-5 ① 先调原函数、再返回伪档：
    --   原版在 judge_on 分支「pickJudge 后直接 return v」，把 aj() 整段跳过了；而
    --   OnAdjustJudge（加血/护盾/OnAttrSyncMyBlood）与 AddMyEnergy（能量）全在这个原函数里
    --   (__015:741-752) ⇒ 判定概率一开就「一定不加血、不加能量」。
    local r = aj(tm2, judgeType, ...)
    if v ~= nil then return v end
    return r
  end
  pcall(rs, tm, "__ywt_aj", 1)
  log("[Ywt] AdjustJudge hooked src=typeMode")
  return true
end

-- 每帧入口：RhythmMasterMode.OnUpdate 里顺手重查待定 env（进玩法第一帧就能挂上，不必等 2s）
local function armUpdateHook()
  if rg(G, "__ywt_upd") then return end
  local M = getmod("RhythmMasterMode")
  local up = M and idx(M, "OnUpdate")
  if type(up) ~= "function" then return end
  rs(G, "__ywt_upd", 1)
  M.OnUpdate = function(self, ...)
    local r = up(self, ...)
    pcall(armAdjust, self)                                   -- ★主落点：typeMode.AdjustJudge
    if not rg(G, "__ywt_env_done") and APPLIED.pend then pcall(tryCachedEnvs) end
    return r
  end
  log("[Ywt] OnUpdate armed (per-frame env check)")
end

local function tryEnvWrap()
  local list = envCands()
  if not APPLIED.candLogged then
    APPLIED.candLogged = true
    log("[Ywt] env cands=" .. #list)
  end
  local seen, wrapped = {}, 0
  for i = 1, #list do
    local ups = envsOf(list[i].fn)
    for j = 1, #ups do
      local tab = ups[j].tab
      if not seen[tab] then
        seen[tab] = 1
        if tryWrapIn(tab, list[i].name .. "@" .. tostring(ups[j].name)) then
          wrapped = wrapped + 1
        end
      end
    end
  end
  return wrapped
end

-- ============ 5. 自动跳舞（按 state 扇出，每 state 各设一遍）============
local function applyAuto()
  local c = cfg(); if not c then return end
  local DM = getmod("DanceVM")
  if type(DM) ~= "table" then return end
  local set = idx(DM, "SetCheatMode")
  if type(set) ~= "function" then return end
  local want = c.auto_on and ((c.auto_mode == 2) and 3 or 2) or 1
  if APPLIED.set and APPLIED.mode == want then return end
  local ok = pcall(set, DM, want)
  if not ok then ok = pcall(set, want) end
  if ok then
    APPLIED.mode = want; APPLIED.set = true
  else
    APPLIED.fail = (APPLIED.fail or 0) + 1
    if APPLIED.fail == 1 then log("[Ywt] SetCheatMode(" .. tostring(want) .. ") failed") end
  end
end
rs(G, "__ywt_apply", applyAuto)

local function late()
  applyAuto()                     -- ★先跑自动跳舞：env 扫描怎么失败都不能影响它
  armUpdateHook()                 -- 每帧快查入口（方法填充后立刻包，不等 2s）
  if rg(G, "__ywt_env_done") then return true end
  if tryCachedEnvs() then
    rs(G, "__ywt_env_done", 1)
    log("[Ywt] late ok ojr=" .. tostring(rg(G, "__ywt_ojr")) .. " (cached)")
    return true
  end
  local n = tryEnvWrap()
  if n > 0 then
    rs(G, "__ywt_env_done", 1)
    log("[Ywt] late ok ojr=" .. tostring(rg(G, "__ywt_ojr")) .. " wrapped=" .. n)
    return true
  end
  return type(rg(G, "__ywt_ojr")) == "string"
end
rs(G, "__ywt_late", late)

-- ============ 6. 收尾 ============
local wrapped = tryEnvWrap()
applyAuto()
if wrapped > 0 then
  rs(G, "__ywt_ver", 7)
  return 1
end
if getmod("RhythmMasterVM") ~= nil then
  rs(G, "__ywt_ver", 7)
  log("[Ywt] patch ok ojr=no (env 未出现，late fix armed)")
  return 1
end
if not APPLIED.pending then
  APPLIED.pending = true
  log("[Ywt] patch pending miss=RhythmMasterVM")
end
return 0
)YWT";

// ============================================================================
//  ⑤ 时间工具
// ============================================================================
static uint64_t NowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (uint64_t)ts.tv_sec * 1000ull + (uint64_t)ts.tv_nsec / 1000000ull;
}

// ============================================================================
//  ⑥ 配置下发（只在 Lua 线程调用：hook 内）
// ============================================================================
static int LuaYwtLog(lua_State* L) {
    const char* msg = p_lua_tolstring ? p_lua_tolstring(L, 1, nullptr) : nullptr;
    if (msg) LOGI("%s", msg);
    return 0;
}

static void PushCfg(lua_State* L) {
    if (!p_lua_createtable || !p_lua_setfield || !p_lua_setglobal || !p_lua_gettop || !p_lua_settop) return;
    const int top = p_lua_gettop(L);
    p_lua_createtable(L, 0, 16);
    p_lua_pushboolean(L, g_autoOn.load() ? 1 : 0);  p_lua_setfield(L, -2, "auto_on");
    p_lua_pushinteger(L, g_autoMode.load());        p_lua_setfield(L, -2, "auto_mode");
    p_lua_pushboolean(L, g_judgeOn.load() ? 1 : 0); p_lua_setfield(L, -2, "judge_on");
    for (int i = 0; i < 6; ++i) {
        char name[6];
        snprintf(name, sizeof(name), "w%d", i + 1);
        p_lua_pushinteger(L, g_w[i].load());
        p_lua_setfield(L, -2, name);
    }
    p_lua_pushinteger(L, kAutoPerFrame);            p_lua_setfield(L, -2, "per_frame");
    if (p_lua_pushnumber) {
        p_lua_pushnumber(L, (double)kAutoSkip);     p_lua_setfield(L, -2, "skip");
    }
    p_lua_setglobal(L, "__ywt_cfg");

    // 日志回调（每 state 一份；幂等）
    if (p_lua_pushcclosure) {
        p_lua_pushcclosure(L, (lua_CFunction_t)LuaYwtLog, 0);
        p_lua_setglobal(L, "__ywt_log");
    }
    p_lua_settop(L, top);
}

// 调 _G.__ywt_late()：模块可能在 patch 成功后才 require（Ctrl 就是），每 2s 补挂一次
static void RunLate(lua_State* L) {
    if (!p_luaL_loadstring || !g_origPcall) return;
    const int top = p_lua_gettop ? p_lua_gettop(L) : 0;
    if (p_luaL_loadstring(L, "if type(_G.__ywt_late)=='function' then _G.__ywt_late() end") == 0) {
        g_origPcall(L, 0, 0, 0);
    }
    if (p_lua_settop) p_lua_settop(L, top);
}

// 调 _G.__ywt_apply()（开关刚变时让 SetCheatMode 立刻跟上）
static void RunApply(lua_State* L) {
    if (!p_luaL_loadstring || !g_origPcall) return;
    const int top = p_lua_gettop ? p_lua_gettop(L) : 0;
    if (p_luaL_loadstring(L, "if type(_G.__ywt_apply)=='function' then _G.__ywt_apply() end") == 0) {
        if (g_origPcall(L, 0, 0, 0) != 0 && p_lua_tolstring) {
            const char* err = p_lua_tolstring(L, -1, nullptr);
            LOGW("[Ywt] apply err: %s", err ? err : "?");
        }
    }
    if (p_lua_settop) p_lua_settop(L, top);
}

// ============================================================================
//  ⑦ 注入状态机
// ============================================================================
static std::atomic<uint64_t> g_lastTryMs{0};
static std::atomic<int>      g_tryCount{0};
// ★2026-11-20 根因：本作有多个独立 lua_State，_G 完全隔离；
//   Ctrl 只挂在「玩法 state」的 _G 上 ⇒ 必须**按 state 分别 patch**，
//   用一个全局 bool 会让 patch ok 之后再也不管其它 state（自动跳舞/判定都失效）。
static std::atomic<bool>     g_patched{false};              // 仅表示「至少有一个 state 装好」（IsReady 用）
static std::atomic<uint8_t>  g_slotPatched[kMaxStates];     // 每个 state 自己的 patch 状态
static std::atomic<uint64_t> g_slotLastTry[kMaxStates];     // 每 state 上次尝试时间
static std::atomic<int>      g_slotTry[kMaxStates];         // 每 state 尝试次数
static std::atomic<uint64_t> g_slotLastLate[kMaxStates];    // 每 state 上次 late tick
static std::atomic<uint64_t> g_slotLastLateLog[kMaxStates]; // 每 state 上次 late 日志（5s 一条）
static std::atomic<uint64_t> g_injectTid{0};
static std::atomic<uint64_t> g_pcallCalls{0};
static std::atomic<uint64_t> g_lastLateMs{0};

static const int kMaxTry = 600;      // 600 次 × 1s ≈ 10 分钟
static const uint64_t kRetryMs = 1000;

// 跑补丁脚本；返回 true = 装好
//
// ★栈纪律（2026-11-19 真实事故修正，必读）
//   lua_settop(L, -1) 在 Lua 5.3 里是**空操作**：实现是 L->top += idx+1，idx=-1 ⇒ +0。
//   老代码用它「弹掉返回值」等于没弹 ⇒ 我们的 0/1 留在栈顶；紧接着业务 lua_pcall 按
//   nargs 反推函数位置（func = top - nargs - 1）时把我们的 number 当成了函数 ⇒
//   xLua DoString 报 "attempt to call a number value"，登录协程 _StepEnterLogin 中断
//   （实测 21:15:17，注入执行 2ms 后）。
//   现规则：入口 lua_gettop 存绝对高度，出口 lua_settop(top) 恢复；异常信息用
//   xpcall + debug.traceback 包一层，失败时能拿到 Lua 侧行号。
static bool RunPatch(lua_State* L) {
    if (!p_luaL_loadstring || !p_lua_gettop || !p_lua_settop) return false;
    const int top = p_lua_gettop(L);
    static const char kWrapHead[] = "local __ok,__r = xpcall(function()\n";
    static const char kWrapTail[] = "\nend, debug.traceback)\nif not __ok then error(__r, 0) end\nreturn __r\n";

    std::string src;
    src.reserve(sizeof(kWrapHead) + strlen(kYwtPatch) + sizeof(kWrapTail));
    src += kWrapHead;
    src += kYwtPatch;
    src += kWrapTail;

    bool ok = false;
    if (p_luaL_loadstring(L, src.c_str()) != 0) {
        if (p_lua_tolstring) {
            const char* err = p_lua_tolstring(L, -1, nullptr);
            LOGE("[Ywt] loadstring err: %s", err ? err : "?");
        }
    } else {
        const int rc = g_origPcall ? g_origPcall(L, 0, 1, 0) : p_lua_pcall(L, 0, 1, 0);
        if (rc != 0) {
            if (p_lua_tolstring) {
                const char* err = p_lua_tolstring(L, -1, nullptr);
                LOGE("[Ywt] patch err: %s", err ? err : "?");
            }
        } else {
            const long long done = p_lua_tointegerx ? p_lua_tointegerx(L, -1, nullptr) : 0;
            ok = (done == 1);
        }
    }
    p_lua_settop(L, top);            // ★绝对高度，不是 -1
    return ok;
}

// ============================================================================
//  ⑧ lua_pcall 钩子（唯一挂点）
// ============================================================================
static int Hook_lua_pcall(lua_State* L, int nargs, int nresults, int errfunc) {
    if (!g_origPcall) return -1;
    const uint64_t calls = g_pcallCalls.fetch_add(1, std::memory_order_relaxed) + 1;
    if (calls == 1) LOGI("[Ywt] lua_pcall hit L=%p", (void*)L);

    // 重入闸：注入过程中自己调的 pcall 直接走蹦床（不用 thread_local，x86_64 可链）
    const uint64_t self = (uint64_t)pthread_self();
    if (g_injectTid.load(std::memory_order_acquire) == self) {
        return g_origPcall(L, nargs, nresults, errfunc);
    }

    // 入口栈高度：注入全程必须回到这里，否则业务 pcall 的 nargs/nresults 语义会被我们改写
    const int topEntry = p_lua_gettop ? p_lua_gettop(L) : 0;

    const int slot = StateSlot(L);
    if (slot >= 0) {
        // 配置变更 → 各 state 各自补写（开关一变立刻生效；Lua 侧每次现读）
        const uint32_t seq = g_cfgSeq.load(std::memory_order_acquire);
        if (g_stateApplied[slot] != seq) {
            g_injectTid.store(self, std::memory_order_release);
            PushCfg(L);
            RunApply(L);
            g_injectTid.store(0, std::memory_order_release);
            g_stateApplied[slot] = seq;
        }
    }

    // 2026-11-20 整理：native 的 _ENV 探针（ProbeEnvAndHook）已删除，
    // 改由 Lua 侧从已知函数的 _ENV upvalue 直接取模块环境（唯一有效路径）。

    // ===== 按 state 扇出：只在「当前这个 L」上做（跨 state 调用不安全，也没必要——
    //       玩法 state 活跃时它的 lua_pcall 会不断命中，自然会轮到它）=====
    if (slot >= 0) {
        const bool patched = g_slotPatched[slot].load(std::memory_order_relaxed) != 0;
        if (!patched) {
            const int tries = g_slotTry[slot].load(std::memory_order_relaxed);
            const uint64_t now = NowMs();
            if (tries < kMaxTry && (now - g_slotLastTry[slot].load(std::memory_order_relaxed)) >= kRetryMs) {
                g_slotLastTry[slot].store(now, std::memory_order_relaxed);
                g_slotTry[slot].store(tries + 1, std::memory_order_relaxed);
                g_injectTid.store(self, std::memory_order_release);
                const bool ok = RunPatch(L);
                int ctrl = 0;
                if (p_lua_getglobal && p_lua_toboolean && p_lua_settop) {
                    p_lua_getglobal(L, "__ywt_ojr");
                    ctrl = p_lua_toboolean(L, -1);
                    p_lua_settop(L, topEntry);      // 立刻复平，别把值留在栈上
                }
                g_injectTid.store(0, std::memory_order_release);
                // ctrl 位来自 C API 的 lua_getglobal（registry globals），xLua 的 _G 未必是同一个表
                // ⇒ 只作参考，真正判据是 Lua 打的 "[Ywt] lua ctrl=... ojr=..."
                LOGI("[Ywt] patch try slot=%d ok=%d ctrl=%d (native view) tries=%d", slot, ok ? 1 : 0, ctrl, tries + 1);
                if (ok) {
                    g_slotPatched[slot].store(1, std::memory_order_release);
                    g_patched.store(true, std::memory_order_release);
                } else if (tries + 1 == kMaxTry) {
                    LOGE("[Ywt] patch give up slot=%d tries=%d", slot, tries + 1);
                }
            }
        } else {
            const uint64_t now2 = NowMs();
            if (now2 - g_slotLastLate[slot].load(std::memory_order_relaxed) >= 2000) {
                g_slotLastLate[slot].store(now2, std::memory_order_relaxed);
                g_injectTid.store(self, std::memory_order_release);
                RunLate(L);      // = applyAuto + wrapCtrl，两个都跑，autodance 不缓存 state
                g_injectTid.store(0, std::memory_order_release);
                // 每个 slot 每 5s 一条心跳：否则玩法 state(slot=3) 完全不可见，等于盲修
                if (now2 - g_slotLastLateLog[slot].load(std::memory_order_relaxed) >= 5000) {
                    g_slotLastLateLog[slot].store(now2, std::memory_order_relaxed);
                    LOGI("[Ywt] late slot=%d", slot);
                }
            }
        }
    }

    // 兜底：无论注入走了哪条分支，栈必须与进入时完全一致（多一个值就会让业务
    // lua_pcall 把别的值当函数调 —— 2026-11-19 "attempt to call a number value" 事故）
    if (p_lua_settop && p_lua_gettop) {
        const int now = p_lua_gettop(L);
        if (now != topEntry) {
            LOGE("[Ywt] stack drift %d -> %d, restoring", topEntry, now);
            p_lua_settop(L, topEntry);
        }
    }
    return g_origPcall(L, nargs, nresults, errfunc);
}

// ============================================================================
//  ⑨ 符号解析 + 装钩
// ============================================================================
static bool LoadLuaApi() {
    if (g_apiReady.load(std::memory_order_acquire)) return true;
    // 只认已加载的实例：自己 dlopen 会把 libxlua 提前拉起来，干扰游戏初始化节奏
    void* h = dlopen(YWT_LUA_LIB, RTLD_NOLOAD | RTLD_NOW);
    if (!h) return false;
    p_luaL_loadstring  = (luaL_loadstring_t)dlsym(h, "luaL_loadstring");
    p_lua_pcall        = (lua_pcall_t)dlsym(h, "lua_pcall");
    p_lua_tolstring    = (lua_tolstring_t)dlsym(h, "lua_tolstring");
    p_lua_settop       = (lua_settop_t)dlsym(h, "lua_settop");
    p_lua_gettop       = (lua_gettop_t)dlsym(h, "lua_gettop");
    p_lua_setglobal    = (lua_setglobal_t)dlsym(h, "lua_setglobal");
    p_lua_getglobal    = (lua_getglobal_t)dlsym(h, "lua_getglobal");
    p_lua_toboolean    = (lua_toboolean_t)dlsym(h, "lua_toboolean");
    p_lua_getupvalue   = (lua_getupvalue_t)dlsym(h, "lua_getupvalue");
    p_lua_getfield     = (lua_getfield_t)dlsym(h, "lua_getfield");
    p_lua_istable      = (lua_istable_t)dlsym(h, "lua_istable");
    p_lua_isfunction   = (lua_isfunction_t)dlsym(h, "lua_isfunction");
    p_lua_pushvalue    = (lua_pushvalue_t)dlsym(h, "lua_pushvalue");
    p_lua_pushboolean  = (lua_pushboolean_t)dlsym(h, "lua_pushboolean");
    p_lua_pushinteger  = (lua_pushinteger_t)dlsym(h, "lua_pushinteger");
    p_lua_pushnumber   = (lua_pushnumber_t)dlsym(h, "lua_pushnumber");
    p_lua_pushcclosure = (lua_pushcclosure_t)dlsym(h, "lua_pushcclosure");
    p_lua_createtable  = (lua_createtable_t)dlsym(h, "lua_createtable");
    p_lua_setfield     = (lua_setfield_t)dlsym(h, "lua_setfield");
    p_lua_tointegerx   = (lua_tointegerx_t)dlsym(h, "lua_tointegerx");

    if (!p_luaL_loadstring || !p_lua_pcall || !p_lua_tolstring || !p_lua_settop ||
        !p_lua_gettop || !p_lua_createtable || !p_lua_setfield || !p_lua_setglobal ||
        !p_lua_pushboolean || !p_lua_pushinteger || !p_lua_pushcclosure) {
        LOGE("[Ywt] libxlua 符号缺失 loadstring=%p pcall=%p table=%p field=%p global=%p cclosure=%p",
             (void*)p_luaL_loadstring, (void*)p_lua_pcall, (void*)p_lua_createtable,
             (void*)p_lua_setfield, (void*)p_lua_setglobal, (void*)p_lua_pushcclosure);
        return false;
    }
    g_apiReady.store(true, std::memory_order_release);
    LOGI("[Ywt] lua api ok loadstring=%p pcall=%p tolstring=%p", (void*)p_luaL_loadstring,
         (void*)p_lua_pcall, (void*)p_lua_tolstring);
    return true;
}

static void InstallHook() {
    if (g_hooked.load(std::memory_order_acquire)) return;
    void* target = (void*)p_lua_pcall;
    if (!target) return;

#if defined(__aarch64__)
    // 序言实测 d100c3ff a9027bfd 910083fd f81f83a0（16 B 干净，Dobby 安全）；
    // 不匹配只告警不拒绝：符号由 dlsym 保证是 lua_pcall，换版本≠挂错地址
    static const uint32_t kFp[4] = {0xd100c3ffu, 0xa9027bfdu, 0x910083fdu, 0xf81f83a0u};
    uint32_t got[4] = {0, 0, 0, 0};
    memcpy(got, target, sizeof(got));
    if (got[0] != kFp[0] || got[1] != kFp[1] || got[2] != kFp[2] || got[3] != kFp[3]) {
        LOGW("[Ywt] lua_pcall 序言不同于实测（%08x %08x %08x %08x），仍按符号挂钩",
             got[0], got[1], got[2], got[3]);
    }
#endif

    const int rc = DobbyHook(target, (void*)Hook_lua_pcall, (void**)&g_origPcall);
    if (rc == 0 && g_origPcall) {
        g_hooked.store(true, std::memory_order_release);
        LOGI("[Ywt] hooked lua_pcall @ %p orig=%p", target, (void*)g_origPcall);
    } else {
        LOGE("[Ywt] DobbyHook lua_pcall rc=%d orig=%p", rc, (void*)g_origPcall);
    }
}

// ============================================================================
//  ⑩ 注册表回调
// ============================================================================
bool ywt_TryInit(JavaVM* /*vm*/) {
    if (!LoadLuaApi()) return false;   // libxlua 还没加载 → 下轮再试
    InstallHook();
    return g_hooked.load();
}

bool ywt_IsReady() { return g_patched.load(); }

void ywt_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case YWT_FEAT_AUTO_DANCE:
            g_autoOn.store(enabled);
            g_autoMode.store(enabled ? 1 : 0);
            break;
        case YWT_FEAT_JUDGE_ON:
            g_judgeOn.store(enabled);
            break;
        default:
            break;
    }
    g_cfgSeq.fetch_add(1, std::memory_order_relaxed);
    LOGI("[Ywt] feat %d -> %d", featNum, enabled ? 1 : 0);
}

void ywt_OnValueChanged(int featNum, int value) {
    int idx = -1;
    switch (featNum) {
        case YWT_FEAT_W_SPP:  idx = 0; break;
        case YWT_FEAT_W_SP:   idx = 1; break;
        case YWT_FEAT_W_P:    idx = 2; break;
        case YWT_FEAT_W_G:    idx = 3; break;
        case YWT_FEAT_W_GRT:  idx = 4; break;
        case YWT_FEAT_W_GOOD: idx = 5; break;
        default: break;
    }
    if (idx < 0) return;
    // 2026-11-20：滑条 0-100 百分比（用户最终口径），clamp 到 100
    if (value < 0) value = 0;
    if (value > 100) value = 100;
    g_w[idx].store(value);
    g_cfgSeq.fetch_add(1, std::memory_order_relaxed);
    LOGI("[Ywt] weight[%d] -> %d", idx, value);
}

void ywt_Tick(const bool* /*features*/, const int* /*values*/) {
    // 兜底：启动期 tryInit 用尽后 libxlua 才加载的情况（每 tick ≈200ms，廉价）
    if (!g_hooked.load()) {
        if (LoadLuaApi()) InstallHook();
    }
}

void ywt_ForceClear() {
    g_autoOn.store(false);
    g_autoMode.store(0);
    g_judgeOn.store(false);
    const int d[6] = {70, 15, 8, 4, 2, 1};
    for (int i = 0; i < 6; ++i) g_w[i].store(d[i]);
    g_cfgSeq.fetch_add(1, std::memory_order_relaxed);
    LOGI("[Ywt] force clear");
}
