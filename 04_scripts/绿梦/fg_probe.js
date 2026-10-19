/*
 * fg_probe.js — 绿梦 libFairGuard.so 运行期取证（只读，不改任何字节）
 *
 * 已确认事实（Stage 2 实测，见 10_docs/绿梦/FairGuard运行期取证.md）：
 *   - libFairGuard.so 首段运行期映射为 rwxp（base..base+0x413000），内存镜像与磁盘不同 ⇒ 载荷原地解包；
 *   - 解包后 image 里有两处 syscall 桩表：磁盘布局 0x42E000 一套（openat 0x42EB00 / exit_group 0x42F100），
 *     另有一套 image 内联表（openat 0x35B100 / read 0x35B200 / mprotect 0x35B900 / exit_group 0x35BE00 / ptrace 0x35B500 / prctl 0x35BF00）；
 *   - 进程其实并不经过以上任何桩：0x42F100/0x35BE00/libc exit/_exit/abort/AndroidRuntime::exit 全部 hook 过，零命中；
 *   - 30s 前后的 logcat 先出现 ro.secure / ro.debuggable / adb_enabled 探测，紧接着 Zygote: exited cleanly (0)。
 *
 * 用法: frida -U -p <pid> -l fg_probe.js   或   python fg_run.py attach <秒> <pid>
 * 门控: HOOK_FG / HOOK_LIBC / HOOK_BYTES / SNAP_MAPS / DUMP / HOOK_JAVA
 */
'use strict';
const T0 = Date.now();
const HOOK_FG = true, HOOK_LIBC = true, HOOK_BYTES = true, SNAP_MAPS = true, DUMP = false, HOOK_JAVA = true;
function ts() { return ((Date.now() - T0) / 1000).toFixed(2) + 's'; }
function LOGG(s) { console.log('[' + ts() + '] ' + s); }
const FG = 'libFairGuard.so';
const FG_STUBS = [ [0x42f100, 'disk_exit_group'], [0x42eb00, 'disk_openat'], [0x35be00, 'img_exit_group'], [0x35b100, 'img_openat'] ];
let fgBase = null, fgSize = 0;
function modOf(a) { try { const m = Process.findModuleByAddress(a); if (m) return m.name + '+0x' + a.sub(m.base).toString(16); } catch (e) {} try { return a.toString(); } catch (e) { return '?'; } }
function bt(c, n) { const o = []; try { const f = Thread.backtrace(c, Backtracer.ACCURATE); for (let i = 0; i < Math.min(f.length, n || 22); i++) o.push(modOf(f[i])); } catch (e) { o.push('BT_ERR'); } return o; }
function num(p) { try { return p.toInt32(); } catch (e) { return '?'; } }
function sstr(p) { try { return p.readUtf8String(256); } catch (e) { return '<x>'; } }
function hexb(a, n) { try { const b = new Uint8Array(a.readByteArray(n)); let s = ''; for (let i = 0; i < b.length; i++) s += ('0' + b[i].toString(16)).slice(-2); return s; } catch (e) { return 'UNREADABLE'; } }

function hookAddr(a, tag, fn) {
  try { Interceptor.attach(a, { onEnter: function (args) { try { fn.call(this, args); } catch (e) { LOGG('CB_ERR ' + tag + ': ' + e.message); } } }); LOGG('hooked ' + tag + ' @' + modOf(a)); return true; }
  catch (e) { LOGG('HOOK_FAIL ' + tag + ': ' + e.message); return false; }
}

function installFg() {
  const m = Process.findModuleByName(FG); if (!m || installFg.done) return;
  installFg.done = true; fgBase = m.base; fgSize = m.size;
  LOGG('MODULE ' + FG + ' base=' + fgBase + ' size=0x' + m.size.toString(16));
  if (HOOK_FG) FG_STUBS.forEach(function (s) {
    hookAddr(fgBase.add(s[0]), s[1], function (a) {
      LOGG('HIT ' + s[1] + ' x0=' + num(this.context.x0) + ' path=' + (s[1].indexOf('openat') >= 0 ? sstr(a[1]) : '-') + ' bt=' + JSON.stringify(bt(this.context, 20)));
    });
  });
  if (HOOK_BYTES) [2000, 12000].forEach(function (d) {
    setTimeout(function () { LOGG('BYTES@' + d + 'ms fg+0x42f100=' + hexb(fgBase.add(0x42f100), 16)); }, d);
  });
  if (SNAP_MAPS) setTimeout(function () {
    try {
      const L = File.readAllText('/proc/self/maps').split('\n').filter(function (l) { return /libFairGuard|rwxp/.test(l); });
      LOGG('MAPS ' + L.length + '\n' + L.join('\n'));
    } catch (e) { LOGG('MAPS_FAIL ' + e.message); }
  }, 900);
  if (DUMP) setTimeout(function () {
    const CH = 256 * 1024;
    for (let off = 0; off < fgSize; off += CH) {
      const n = Math.min(CH, fgSize - off);
      try { send({ t: 'chunk', off: off, n: n }, fgBase.add(off).readByteArray(n)); } catch (e) { LOGG('DUMP_ERR 0x' + off.toString(16)); }
    }
    send({ t: 'chunk_done', size: fgSize });
  }, 1500);
}

if (HOOK_LIBC) setImmediate(function () {
  ['exit', '_exit', 'abort'].forEach(function (nm) {
    let a = null; try { a = Module.getGlobalExportByName(nm); } catch (e) {}
    if (!a) { try { a = Module.findExportByName(null, nm); } catch (e) {} }
    if (a) hookAddr(a, 'libc:' + nm, function () { LOGG('HIT libc ' + nm + ' bt=' + JSON.stringify(bt(this.context, 22))); });
  });
  let art = null; try { art = Module.getGlobalExportByName('_ZN7android14AndroidRuntime4exitEi'); } catch (e) {}
  if (art) hookAddr(art, 'AndroidRuntime::exit', function () { LOGG('HIT AndroidRuntime::exit bt=' + JSON.stringify(bt(this.context, 22))); });
});

if (HOOK_JAVA) setTimeout(function () {
  try {
    if (typeof Java === 'undefined') { LOGG('java_bridge_unavailable (本机 frida-server 无 java bridge)'); return; }
    Java.perform(function () {
      const S = Java.use('java.lang.System');
      S.exit.overload('int').implementation = function (c) { LOGG('JAVA System.exit(' + c + ')'); return this.exit(c); };
      const R = Java.use('java.lang.Runtime');
      R.exit.overload('int').implementation = function (c) { LOGG('JAVA Runtime.exit(' + c + ')'); return this.exit(c); };
      LOGG('java hooks installed');
    });
  } catch (e) { LOGG('java_hook_fail ' + e.message); }
}, 300);

setInterval(installFg, 250);
let ticks = 0;
setInterval(function () { ticks++; if (ticks % 5 === 0) LOGG('ALIVE t=' + ticks + 's threads=' + Process.enumerateThreads().length); }, 1000);
LOGG('fg_probe armed pid=' + Process.id + ' frida=' + Frida.version);
