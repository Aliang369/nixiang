// 监控 UserinfoRecord 存档写入 + 存盘瞬间全内存扫描明文特征 (S 路线)
'use strict';
const PATS = ['Gold', 'RoleId', 'Unlock', 'DLC', 'BookMode', 'UserInfo', 'HeroId'];
const fds = {};   // fd -> path
let scanned = false;   // 首次 UI 写入后全内存扫一次
let cipherSeq = 0, hitSeq = 0, hitTotal = 0;
const HIT_CAP = 240;

function toHex(s, utf16) {
  let out = '';
  for (let i = 0; i < s.length; i++) {
    const c = s.charCodeAt(i) & 0xff;
    out += ('0' + c.toString(16)).slice(-2);
    if (utf16) out += '00';
  }
  return out;
}

function baseName(p) {
  const i = p.lastIndexOf('/');
  return i >= 0 ? p.slice(i + 1) : p;
}

const LIBC = Process.getModuleByName('libc.so');
const ex = (n) => LIBC.findExportByName(n);

function hookOpen(name) {
  try {
    const fn = ex(name);
    Interceptor.attach(fn, {
      onEnter(args) {
        try { this.p = args[name === 'openat' ? 1 : 0].readCString(); } catch (e) { this.p = null; }
      },
      onLeave(retval) {
        const fd = retval.toInt32();
        if (fd >= 0 && this.p && this.p.indexOf('UserinfoRecord') !== -1) {
          fds[fd] = this.p;
          send({ type: 'log', msg: 'open fd=' + fd + ' path=' + this.p });
        }
      }
    });
  } catch (e) { send({ type: 'log', msg: 'hook ' + name + ' 失败: ' + e }); }
}

function scanAll() {
  const t0 = Date.now();
  let ranges;
  try { ranges = Process.enumerateRanges({ protection: 'rw-', coalesce: true }); }
  catch (e) { send({ type: 'log', msg: 'enumerate 失败: ' + e }); return; }
  send({ type: 'log', msg: '全内存扫描开始: ' + ranges.length + ' 个 rw- 区' });
  for (const r of ranges) {
    if (r.size > 512 * 1024 * 1024) continue;
    for (const p of PATS) {
      for (const utf16 of [false, true]) {
        const hex = toHex(p, utf16);
        let hits = [];
        try { hits = Memory.scanSync(r.base, r.size, hex); } catch (e) { continue; }
        for (const h of hits.slice(0, 6)) {
          if (hitTotal >= HIT_CAP) { send({ type: 'log', msg: '命中达到上限 ' + HIT_CAP }); return; }
          hitTotal++;
          let ctx = null;
          try { ctx = h.address.sub(384).readByteArray(1152); } catch (e) {
            try { ctx = h.address.readByteArray(512); } catch (e2) { continue; }
          }
          send({ type: 'hit', pat: p, enc: utf16 ? 'u16' : 'u8', addr: h.address.toString(),
                 range: r.base.toString(), seq: hitSeq++ }, ctx);
        }
      }
    }
  }
  scanned = true;
  send({ type: 'log', msg: '全内存扫描完成 ' + (Date.now() - t0) + 'ms, 命中 ' + hitTotal + ' 处' });
}

hookOpen('openat');
hookOpen('open');

const writeFn = ex('write');
Interceptor.attach(writeFn, {
  onEnter(args) {
    const fd = args[0].toInt32();
    const path = fds[fd];
    if (!path) return;
    const len = args[2].toInt32();
    if (len <= 0 || len > 8 * 1024 * 1024) return;
    let data = null;
    try { data = args[1].readByteArray(len); } catch (e) { return; }
    send({ type: 'cipher', path: path, len: len, seq: cipherSeq++ }, data);
    const b = baseName(path);
    if (!scanned && (b === 'UI' || b.indexOf('UI') === 0)) {
      setTimeout(scanAll, 50);
    }
  }
});

try {
  const closeFn = ex('close');
  Interceptor.attach(closeFn, {
    onEnter(args) { const fd = args[0].toInt32(); if (fds[fd]) delete fds[fd]; }
  });
} catch (e) {}

send({ type: 'log', msg: 'save_watch 已加载: 监控 UserinfoRecord 写入 + 特征扫描就绪' });
