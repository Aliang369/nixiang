// jjssjd: 在游戏 Lua 状态里枚举 _G（拿战斗 API 名字）
'use strict';
function log(s) { send({ t: 'log', m: s }); }

var L = null, done = false, hits = 0, armed = false;
setTimeout(function () { armed = true; log('armed'); }, 4000);

var MOD = null, f = {};

function bind() {
  if (MOD) return true;
  try { MOD = Process.findModuleByName('libxlua.so'); } catch (e) { MOD = null; }
  if (!MOD) return false;
  function E(n) { try { return MOD.getExportByName(n); } catch (e) { return null; } }
  f.loadstring = new NativeFunction(E('luaL_loadstring'), 'int', ['pointer', 'pointer']);
  f.pcall = new NativeFunction(E('lua_pcall'), 'int', ['pointer', 'int', 'int', 'int']);
  f.tolstring = new NativeFunction(E('lua_tolstring'), 'pointer', ['pointer', 'int', 'pointer']);
  f.gettop = new NativeFunction(E('lua_gettop'), 'int', ['pointer']);
  f.settop = new NativeFunction(E('lua_settop'), 'void', ['pointer', 'int']);
  f.pcallk = E('lua_pcallk');
  f.gettopE = E('lua_gettop');
  f.callk = E('lua_callk');
  log('bound libxlua ' + MOD.base);
  return true;
}

var PROBE = [
  'local G=_G',
  'local t={}',
  'local n=0',
  'for k,v in next,G do n=n+1 if n<=6000 then t[#t+1]=tostring(k).."|"..type(v) end end',
  'local cand={"CFightSystem","CombatFunc","CTXDefine","FightSystem","BattleMgr","SkillMgr","LogicMgr","EntityMgr","FightMgr"}',
  'local c={}',
  'for i=1,#cand do local v=rawget(G,cand[i]) c[#c+1]=cand[i].."="..type(v) end',
  'return "N="..n.."\\n"..table.concat(c,",").."\\n"..table.concat(t,",")'
].join('\n');

function inject() {
  if (done || !L) return;
  done = true;
  try {
    var top = f.gettop(L);
    var src = Memory.allocUtf8String(PROBE);
    var r = f.loadstring(L, src);
    if (r !== 0) {
      var len = Memory.alloc(8);
      var p = f.tolstring(L, -1, len);
      log('loadstring err ' + (p.isNull() ? '?' : p.readUtf8String(len.readU64().toNumber())));
      f.settop(L, top); return;
    }
    r = f.pcall(L, 0, 1, 0);
    if (r !== 0) {
      var len2 = Memory.alloc(8);
      var p2 = f.tolstring(L, -1, len2);
      log('pcall err ' + (p2.isNull() ? '?' : p2.readUtf8String(len2.readU64().toNumber())));
      f.settop(L, top); return;
    }
    var len3 = Memory.alloc(8);
    var p3 = f.tolstring(L, -1, len3);
    if (p3.isNull()) { log('nil result'); f.settop(L, top); return; }
    var s = p3.readUtf8String(len3.readU64().toNumber());
    send({ t: 'globals', s: s });
    f.settop(L, top);
  } catch (e) { log('inject throw ' + e); }
}

function attachEnt(p, tag) {
  try {
    Interceptor.attach(p, {
      onEnter: function (args) {
        hits++;
        if (!L) { L = args[0]; log('got L via ' + tag + ' @' + args[0]); }
        if (armed && !done && hits > 20) inject();
      }
    });
    log('hook ' + tag);
  } catch (e) { log('attach fail ' + tag + ' ' + e); }
}

setInterval(function () {
  if (!bind()) return;
  if (!f.hooked) {
    f.hooked = true;
    if (f.gettopE) attachEnt(f.gettopE, 'lua_gettop');
    if (f.pcallk) attachEnt(f.pcallk, 'lua_pcallk');
    if (f.callk) attachEnt(f.callk, 'lua_callk');
  }
  log('hits=' + hits + ' L=' + L + ' done=' + done);
}, 5000);

log('probe armed');
