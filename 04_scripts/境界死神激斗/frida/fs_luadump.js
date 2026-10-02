// jjssjd: libxlua.so probes (poll-until-loaded, spawn-safe)
'use strict';

var installed = false;
var names = 0;

function log(s) { send({ t: 'log', m: s }); }

function hookOne(mod, sym, tag) {
  var a = null;
  try { a = mod.getExportByName(sym); } catch (e) { return false; }
  Interceptor.attach(a, {
    onEnter: function (args) {
      var sz = args[2].toInt32();
      var nm = null;
      try { nm = args[3].isNull() ? null : args[3].readCString(); } catch (e) { nm = '<bad>'; }
      names++;
      if (names <= 300) send({ t: 'load', sym: tag, name: nm, sz: sz, L: args[0].toString() });
      if (args[1].isNull() || sz <= 0 || sz > (8 << 20)) return;
      if (names <= 2500) {
        try { send({ t: 'chunk', name: nm, sz: sz }, args[1].readByteArray(sz)); } catch (e) {}
      }
    }
  });
  log('hooked ' + sym + ' @ ' + a);
  return true;
}

function tryInstall() {
  if (installed) return;
  var mod = null;
  try { mod = Process.findModuleByName('libxlua.so'); } catch (e) {}
  if (!mod) return;
  installed = true;
  log('libxlua.so base=' + mod.base + ' size=' + mod.size + ' path=' + mod.path);
  hookOne(mod, 'luaL_loadbuffer', 'luaL_loadbuffer');
  hookOne(mod, 'luaL_loadbufferx', 'luaL_loadbufferx');
  hookOne(mod, 'luaL_loadfilex', 'luaL_loadfilex');
  hookOne(mod, 'lua_pcallk', 'lua_pcallk');
}

setInterval(tryInstall, 100);
log('probe armed');
