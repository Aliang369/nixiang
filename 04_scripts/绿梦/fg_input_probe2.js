/*
 * fg_input_probe2.js — Stage 7 判定探针（frida 版；实机用 so 内等价实现跑，更抗反调试）
 * 目的：不替换 fd，只记录 FairGuard 源（回址在 libFairGuard 区间）的
 *       open / read / pread64 / mmap / fstat / readlink，判断它读数据走 libc 还是自有 svc。
 * 用法：frida -U -p <pid> -l fg_input_probe2.js
 */
'use strict';
const T0 = Date.now();
function ts() { return ((Date.now() - T0) / 1000).toFixed(2) + 's'; }
function LOGG(s) { console.log('[' + ts() + '] ' + s); }
function modOf(a) {
  try { const m = Process.findModuleByAddress(a); return m ? (m.name + '+0x' + a.sub(m.base).toString(16)) : a.toString(); }
  catch (e) { return '?'; }
}
function firstFg(ctx) {
  try {
    const f = Thread.backtrace(ctx, Backtracer.FUZZY);
    for (let i = 0; i < Math.min(f.length, 8); i++) {
      const m = Process.findModuleByAddress(f[i]);
      if (m && m.name.indexOf('libFairGuard') >= 0) return m.name + '+0x' + f[i].sub(m.base).toString(16);
    }
  } catch (e) {}
  return null;
}
function sym(n) {
  try { const a = Module.getGlobalExportByName(n); if (a) return a; } catch (e) {}
  try { return Module.findExportByName(null, n); } catch (e) { return null; }
}
function hook(n, cb, isPtr) {
  const a = sym(n);
  if (!a) { LOGG('sym_missing ' + n); return; }
  try { Interceptor.attach(a, cb); } catch (e) { LOGG('hook_fail ' + n); }
}
hook('open', {
  onEnter(a) { this.p = null; try { this.p = a[0].readCString(160); } catch (e) {} },
  onLeave(r) { const fg = firstFg(this.context); if (fg && this.p && this.p.indexOf('base.apk') >= 0) LOGG('FG open ' + this.p + ' fd=' + r.toInt32() + ' @' + fg); }
});
hook('read', {
  onEnter(a) { this.fd = a[0].toInt32(); this.n = a[2].toInt32(); },
  onLeave(r) { const fg = firstFg(this.context); if (fg && r.toInt32() > 0) LOGG('FG read fd=' + this.fd + ' n=' + r.toInt32() + ' @' + fg); }
});
hook('pread64', {
  onEnter(a) { this.fd = a[0].toInt32(); this.off = a[3].toString(); },
  onLeave(r) { const fg = firstFg(this.context); if (fg && r.toInt32() > 0) LOGG('FG pread fd=' + this.fd + ' off=' + this.off + ' n=' + r.toInt32() + ' @' + fg); }
});
hook('mmap', {
  onEnter(a) { this.fd = a[4].toInt32(); this.len = a[1].toString(); this.off = a[5].toString(); },
  onLeave(r) { const fg = firstFg(this.context); if (fg) LOGG('FG mmap fd=' + this.fd + ' len=' + this.len + ' off=' + this.off + ' @' + fg); }
});
hook('fstat', {
  onEnter(a) { this.fd = a[0].toInt32(); },
  onLeave(r) { const fg = firstFg(this.context); if (fg) LOGG('FG fstat fd=' + this.fd + ' rc=' + r.toInt32() + ' @' + fg); }
});
hook('readlink', {
  onEnter(a) { this.p = null; try { this.p = a[0].readCString(160); } catch (e) {} },
  onLeave(r) { const fg = firstFg(this.context); if (fg) LOGG('FG readlink ' + this.p + ' rc=' + r.toInt32() + ' @' + fg); }
});
let k = 0;
setInterval(function () { k++; if (k % 5 === 0) LOGG('ALIVE t=' + k); }, 1000);
LOGG('fg_input_probe2 armed pid=' + Process.id);
