/* fg_input_probe.js v4 — 捕获 FairGuard 打完 open 之后如何取字节（read/pread/mmap），不动 FairGuard */
'use strict';
const T0 = Date.now();
function ts() { return ((Date.now() - T0) / 1000).toFixed(2) + 's'; }
function LOGG(s) { console.log('[' + ts() + '] ' + s); }
const APKRE = /base\.apk|META-INF|\.RSA|\.SF$|idsig/i;
const g_fd = {};
function modOf(a) {
  try { const m = Process.findModuleByAddress(a); return m ? (m.name + '+0x' + a.sub(m.base).toString(16)) : a.toString(); }
  catch (e) { return '?'; }
}
function btN(ctx, n) {
  const o = [];
  try { const f = Thread.backtrace(ctx, Backtracer.ACCURATE); for (let i = 0; i < Math.min(f.length, n); i++) o.push(modOf(f[i])); }
  catch (e) { o.push('BT_ERR'); }
  return o.join(' < ');
}
function rs(p) { try { return p.readCString(200); } catch (e) { return null; } }
function isFg(ctx) { try { return btN(ctx, 6).indexOf('libFairGuard') >= 0; } catch (e) { return false; } }
function sym(n) {
  try { const a = Module.getGlobalExportByName(n); if (a) return a; } catch (e) {}
  try { return Module.findExportByName(null, n); } catch (e) { return null; }
}
function hook(n, cb) { const a = sym(n); if (!a) { LOGG('sym_missing ' + n); return; } try { Interceptor.attach(a, cb); } catch (e) { LOGG('hook_fail ' + n); } }

hook('open', { onEnter(a) { this.p = rs(a[0]); }, onLeave(r) { const fd = r.toInt32(); if (this.p && fd >= 0) { g_fd[fd] = this.p; if (APKRE.test(this.p) && isFg(this.context)) LOGG('FG open fd=' + fd + ' ' + this.p.slice(0, 50) + ' bt=' + btN(this.context, 5)); } } });
hook('openat', { onEnter(a) { this.p = rs(a[1]); }, onLeave(r) { const fd = r.toInt32(); if (this.p && fd >= 0) { g_fd[fd] = this.p; if (APKRE.test(this.p) && isFg(this.context)) LOGG('FG openat fd=' + fd + ' ' + this.p.slice(0, 50) + ' bt=' + btN(this.context, 5)); } } });
hook('read', { onEnter(a) { this.fd = a[0].toInt32(); }, onLeave(r) { const n = r.toInt32(); if (n > 0 && isFg(this.context)) LOGG('FG read fd=' + this.fd + ' n=' + n + ' path=' + (g_fd[this.fd] || '?').slice(0, 40)); } });
hook('pread64', { onEnter(a) { this.fd = a[0].toInt32(); this.off = a[3].toString(); }, onLeave(r) { const n = r.toInt32(); if (n > 0 && isFg(this.context)) LOGG('FG pread fd=' + this.fd + ' off=' + this.off + ' n=' + n + ' path=' + (g_fd[this.fd] || '?').slice(0, 40)); } });
hook('mmap', { onEnter(a) { this.fd = a[4].toInt32(); this.len = a[1].toString(); this.prot = a[2].toInt32(); this.off = a[5].toString(); }, onLeave(r) { if (isFg(this.context)) LOGG('FG mmap fd=' + this.fd + ' len=' + this.len + ' prot=0x' + this.prot.toString(16) + ' off=' + this.off + ' => ' + r + ' path=' + (g_fd[this.fd] || '?').slice(0, 40)); } });
hook('fstat', { onEnter(a) { this.fd = a[0].toInt32(); }, onLeave(r) { if (isFg(this.context)) LOGG('FG fstat fd=' + this.fd + ' path=' + (g_fd[this.fd] || '?').slice(0, 40)); } });

let k = 0;
setInterval(function () { k++; if (k % 5 === 0) LOGG('ALIVE t=' + k); }, 1000);
LOGG('fg_input_probe v4 armed pid=' + Process.id);
