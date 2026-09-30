#pragma once
// ============================================================
//  ModDiagLua.h — Lua 侧诊断 chunk 模板（编译期隔离 + 运行时上限）
//  见 ModDiag.h 的说明（2026-09-30 伊莫复盘 P0-1/P0-2）
//
//  用法（复制即可编译）：
//    #include "ModDiag.h"
//    #include "ModDiagLua.h"
//    ...
//    snprintf(buf, sizeof(buf), kMyPatch, ..., MOD_DIAG_LUA_TEXT);   // %s 位置
//  正式包（不带 STARRY_DIAG）⇒ MOD_DIAG_LUA_TEXT 为空串，Lua 侧**零诊断代码**。
//
//  运行时上限（P0-2）：设备上 `/sdcard/.starry_magic/logcfg`，一行一条 `tag=上限`
//    __default=20    # 未单列的 tag 用这个
//    tp=200          # 单列某个 tag
//    tp=-1           # 负数 = 不限量
//  改完重启游戏即生效，**不需要重建重装**（伊莫曾为把 5 改成 24 重建过一轮）。
//
//  设计要点（都是踩过的坑）：
//    · 一律 rawget/rawset（xLua 的全局表是白名单式：裸读未声明全局返回 nil 且记
//      error，不抛异常 ⇒ 裸读会让整套诊断静默失效）
//    · 写文件通道（游戏自带 Logger 常常不落 logcat）
//    · 每条打 `[diag][tag]#n` 序号：**字段恒为常数 = 诊断本身可能错**，序号能一眼
//      看出「根本没命中」还是「命中了但值不对」
//    · 全局只挂一个 MOD_DIAG 表，避免污染游戏命名空间
// ============================================================

#ifdef STARRY_DIAG

static const char kModDiagLua[] = R"LUA(
-- ===== ModDiagLua：诊断 chunk（仅 STARRY_DIAG=1 时注入）=====
do
  local G = _G
  local rg, rs = rawget, rawset            -- 禁止裸读全局（白名单式 __index）
  local DIR = rg(G, "MOD_DIAG_DIR")
  if type(DIR) ~= "string" then DIR = "/sdcard/.starry_magic" end
  local PATH = DIR .. "/moddiag.log"

  local cfg = { __default = 5 }
  local f = io.open(DIR .. "/logcfg", "r")
  if f then
    for line in f:lines() do
      local k, v = line:match("^%s*([%w_]+)%s*=%s*(-?%d+)")
      if k and v then cfg[k] = tonumber(v) end
    end
    f:close()
  end

  local N = rg(G, "__MOD_DIAG_N")
  if type(N) ~= "table" then N = {}; rs(G, "__MOD_DIAG_N", N) end

  local function emit(s)
    local h = io.open(PATH, "a")
    if h then h:write(s, "\n"); h:close() end
  end

  local D = function(tag, fmt, ...)
    local n = (N[tag] or 0) + 1; N[tag] = n
    local lim = cfg[tag] or cfg.__default
    if lim >= 0 and n > lim then return end
    local ok, s = pcall(string.format, fmt, ...)
    local line = "[diag][" .. tostring(tag) .. "]#" .. n .. " " .. (ok and s or tostring(fmt))
    emit(line)
    local p = rg(G, "print")
    if p then p(line) end
  end

  -- 自检行：诊断装上了没有，一眼可判（"没命中" vs "没装上"）
  local top = 0
  if type(rg(G, "pg")) == "table" then top = 1 end
  D("boot", "moddiag ready path=%s top=%d default=%d", PATH, top, cfg.__default)

  rs(G, "MOD_DIAG", {
    D        = D,                                   -- MOD_DIAG.D("tp", "%.1f", x)
    emit     = emit,
    limit    = function(tag, lim) cfg[tag] = lim end, -- 调试期内热改上限
    path     = PATH,
    counters = N,
  })
end
)LUA";

// 拼进 patch 的文本：正式包为空串
#define MOD_DIAG_LUA_TEXT (kModDiagLua)
#define MOD_DIAG_LUA_LEN  ((int)(sizeof(kModDiagLua) - 1))

#else

#define MOD_DIAG_LUA_TEXT ""
#define MOD_DIAG_LUA_LEN 0

#endif
