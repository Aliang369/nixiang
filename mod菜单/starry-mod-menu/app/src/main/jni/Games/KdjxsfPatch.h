/* ================================================================
 *  KdjxsfPatch.h — 口袋觉醒（私服 com.zcdsg.xmgm 3.10）Lua 侧补丁
 *
 *  运行环境：Cocos2d-x + LuaJIT 2.1(GC64)，游戏逻辑全在 Lua（无 il2cpp）。
 *  载入方式：KdjxsfMod.cpp 在 GL 线程（nativeRender hook）用
 *            loadstring + pcall 执行本 chunk；幂等标记 __kdjx_v。
 *
 *  v1 = 诊断版：包裹战斗模型方法写 kd_diag.log（真机取证用），
 *       功能仅接「战斗加速」（不需要战斗语义即可安全落地）。
 * ================================================================ */
#ifndef STARRY_KDJXSF_PATCH_H
#define STARRY_KDJXSF_PATCH_H

/* 诊断日志落点：应用自己的外部私有目录，无需任何权限 */
#define KDJX_LOG_PATH "/data/data/com.zcdsg.xmgm/files/kd_diag.log"

static const char* kKdjxsfLuaPatch = R"LUA(
-- 口袋觉醒 星光补丁 v1（诊断 + 战斗加速）
if __kdjx_v then return end
local __ok, __err = pcall(function()

local LOGP = "/data/data/com.zcdsg.xmgm/files/kd_diag.log"
local ERRP = "/data/data/com.zcdsg.xmgm/files/kd_patch_err.txt"
local MAXLINES = 4000
local S = {inv = 0, dmg = 1, spd = 1, skill = 0}
local fh = nil
local nlines = 0

local function log(s)
  if nlines > MAXLINES then return end
  if not fh then fh = io.open(LOGP, "a") end
  if fh then
    fh:write(s, "\n")
    fh:flush()
    nlines = nlines + 1
  end
end

local function pack(...) return {n = select('#', ...), ...} end

local function sv(v)
  local t = type(v)
  if t == "number" then
    if v == math.floor(v) and math.abs(v) < 1e9 then return string.format("%d", v) end
    return string.format("%.2f", v)
  elseif t == "string" then
    return v:sub(1, 24)
  elseif t == "boolean" then
    return tostring(v)
  elseif t == "table" then
    return "T"
  elseif t == "function" then
    return "F"
  elseif t == "nil" then
    return "nil"
  end
  return t
end

local function argstr(...)
  local n = select('#', ...)
  local r = {}
  for i = 1, math.min(n, 8) do r[i] = sv((select(i, ...))) end
  return n .. "[" .. table.concat(r, ",") .. "]"
end

local function dumpobj(o)
  if type(o) ~= "table" then return sv(o) end
  local r = {}
  local c = 0
  for k, v in pairs(o) do
    local t = type(v)
    if t ~= "table" and t ~= "function" and t ~= "userdata" then
      r[#r + 1] = tostring(k) .. "=" .. sv(v)
    end
    c = c + 1
    if c > 300 then break end
  end
  table.sort(r)
  return table.concat(r, " ")
end

local function dumptbl(name, t)
  if type(t) ~= "table" then return end
  local r = {}
  for k, v in pairs(t) do r[#r + 1] = tostring(k) .. "=" .. sv(v) end
  table.sort(r)
  log(name .. "| " .. table.concat(r, " "))
end

-- ============ 状态入口（C++ 侧每帧检测变化后调用） ============
function __kdjx_set(s)
  local a, b, c, d = string.match(tostring(s), "(%d+),(%d+),(%d+),(%d+)")
  S.inv = tonumber(a) or 0
  S.dmg = tonumber(b) or 1
  S.spd = tonumber(c) or 1
  S.skill = tonumber(d) or 0
  if S.spd < 1 then S.spd = 1 end
  if S.dmg < 1 then S.dmg = 1 end
  local ok = pcall(function()
    local sch = cc.Director:getInstance():getScheduler()
    if sch then sch:setTimeScale(S.spd) end
  end)
  log(string.format("state inv=%d dmg=%d spd=%d skill=%d ts=%s",
      S.inv, S.dmg, S.spd, S.skill, tostring(ok)))
end

-- ============ 诊断：战斗模型方法包裹 ============
local OM = ObjectModel
if type(OM) == "table" then
  local specs = {
    {"beAttack", 14}, {"strike", 8}, {"calcInternalDamage", 10},
    {"correctHp", 6}, {"setHP", 6}, {"addHp", 6}, {"cure", 6},
    {"mp1", 3}, {"mp2", 3}, {"addMP1", 6},
    {"getCurSkill", 3}, {"getSkillType", 3},
    {"onNewRound", 3}, {"onNewBattleTurn", 3},
  }
  for i = 1, #specs do
    local name = specs[i][1]
    local limit = specs[i][2]
    local orig = OM[name]
    if type(orig) == "function" then
      local cnt = 0
      OM[name] = function(self, ...)
        cnt = cnt + 1
        local trace = cnt <= limit
        if trace then
          log("OM." .. name .. "#" .. cnt .. " self{" .. dumpobj(self) .. "} args=" .. argstr(...))
        end
        local r = pack(orig(self, ...))
        if trace then
          log("OM." .. name .. " -> " .. argstr(unpack(r, 1, r.n)))
        end
        return unpack(r, 1, r.n)
      end
    end
  end
end

local G = battlePlay and battlePlay.Gate
if type(G) == "table" then
  local names = {"runBattleTurn", "beginBattleTurn", "endBattleTurn",
                 "speedRankSort", "recordDamageStats", "refreshUIHp"}
  for i = 1, #names do
    local name = names[i]
    local orig = G[name]
    if type(orig) == "function" then
      local cnt = 0
      G[name] = function(self, ...)
        cnt = cnt + 1
        if cnt <= 4 then
          log("Gate." .. name .. "#" .. cnt .. " args=" .. argstr(...))
        end
        return orig(self, ...)
      end
    end
  end
end

if type(battle) == "table" then
  dumptbl("battle.ObjectType", battle.ObjectType)
  dumptbl("battle.ObjectClass", battle.ObjectClass)
  dumptbl("battle.DamageFrom", battle.DamageFrom)
  dumptbl("battle.SkillType", battle.SkillType)
end

log("patch v1 loaded, OM=" .. tostring(type(OM)) .. " Gate=" .. tostring(type(G)))
end)
if __ok then
  __kdjx_v = 1
else
  local f = io.open(ERRP, "wb")
  if f then f:write(tostring(__err)); f:close() end
end
)LUA";

#endif  // STARRY_KDJXSF_PATCH_H
