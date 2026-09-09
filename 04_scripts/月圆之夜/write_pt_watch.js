// 写时抓明文: hook write(), 对 UIA/UI/XML 缓冲区附近内存探测 25B 周期 XOR 密钥
'use strict';
const LIBC = Process.getModuleByName('libc.so');
const ex = (n) => LIBC.findExportByName(n);
const TARGET_BASE = 'UserinfoRecord';   // UIA/UI 文件
const TARGET_PREFS = 'playerprefs';     // XML 提交
const fds = {};
const done = {};   // 去重: path -> count

function baseName(p) { const i = p.lastIndexOf('/'); return i >= 0 ? p.slice(i + 1) : p; }

function hookOpen(name) {
  const fn = ex(name);
  if (!fn) return;
  Interceptor.attach(fn, {
    onEnter(args) {
      try { this.p = args[name === 'openat' ? 1 : 0].readCString(); } catch (e) { this.p = null; }
    },
    onLeave(retval) {
      const fd = retval.toInt32();
      if (fd >= 0 && this.p) {
        const b = baseName(this.p);
        if (this.p.indexOf(TARGET_BASE) !== -1 || b.indexOf(TARGET_PREFS) !== -1) {
          fds[fd] = this.p;
        }
      }
    }
  });
}
hookOpen('openat'); hookOpen('open');

// 周期探测: cand 与 ct XOR 后是否呈 L 周期
function testPeriodicArr(ct, cand, off, L) {
  const n = Math.min(64, ct.length, cand.length - off);
  if (n < 2 * L) return null;
  const K = new Uint8Array(L);
  for (let j = 0; j < L; j++) K[j] = ct[j] ^ cand[off + j];
  let distinct = 0, nz = 0;
  for (let j = 0; j < L; j++) {
    if (K[j] !== 0) nz++;
    for (let q = j + 1; q < L; q++) if (K[j] !== K[q]) { distinct = 1; break; }
    if (distinct) break;
  }
  if (!nz || !distinct) return null;
  for (let j = 0; j < L; j++) {
    for (let r = L; r + j < n; r += L) {
      if ((ct[r + j] ^ cand[off + r + j]) !== K[j]) return null;
    }
  }
  return K;
}

function probeAround(ctBuf, ctLen, label, path) {
  const R = 262144; // ±256KB
  const L = 25;
  const ctLenC = Math.min(64, ctLen);
  let ct;
  try { ct = new Uint8Array(ctBuf.readByteArray(ctLenC)); } catch (e) { return false; }
  for (const sign of [-1, 1]) {
    for (const size of [R, 65536, 16384]) {
      const base = sign < 0 ? ctBuf.sub(size) : ctBuf;
      let win;
      try { win = new Uint8Array(base.readByteArray(size)); } catch (e) { continue; }
      let zeros = 0, same = true, first = win[0];
      for (let i = 0; i < 256; i++) { if (win[i] !== 0) zeros++; if (win[i] !== first) same = false; }
      if (zeros < 16 && !same) { /* 有内容才扫 */ }
      const lim = win.length - 128;
      for (let i = 0; i < lim; i += 4) {
        // 快速预筛: 候选区不能全等/全零
        if (win[i] === win[i + 25] && win[i + 1] === win[i + 26] && win[i] === win[i + 1] && win[i] === 0) continue;
        const K = testPeriodicArr(ct, win, i, L);
        if (K) {
          const addrStr = base.add(i).toString();
          send({ type: 'hit', label: label, path: path, addr: addrStr, ct: ctBuf.toString(),
                 len: ctLen, keyhex: Array.from(K).map(x => ('0' + x.toString(16)).slice(-2)).join('') },
               base.add(i).readByteArray(1024));
          return true;
        }
      }
      if (sign < 0) break; // 低窗命中即止, 未中再试高窗大窗
    }
  }
  return false;
}

const writeFn = ex('write');
Interceptor.attach(writeFn, {
  onEnter(args) {
    const fd = args[0].toInt32();
    const path = fds[fd];
    if (!path) return;
    const len = args[2].toInt32();
    if (len <= 0 || len > 8 * 1024 * 1024) return;
    const buf = args[1];
    let data;
    try { data = buf.readByteArray(len); } catch (e) { return; }
    const n = (done[baseName(path)] || 0) + 1;
    done[baseName(path)] = n;
    send({ type: 'cipher', path: path, seq: n, len: len }, data);
    const b = baseName(path);
    if (b === 'UIA' || b === 'UIA_Backup' || b === 'UI' || b.indexOf('playerprefs') !== -1) {
      try {
        const ok = probeAround(buf, len, b, path);
        send({ type: 'log', msg: b + ' 写入#' + n + ' len=' + len + ' 明文探测: ' + (ok ? '命中!' : '未中') });
      } catch (e) {
        send({ type: 'log', msg: '探测异常: ' + e + ' | ' + (e.stack || '').slice(0, 200) });
      }
    }
  }
});

try {
  const closeFn = ex('close');
  Interceptor.attach(closeFn, {
    onEnter(args) { const fd = args[0].toInt32(); if (fds[fd]) delete fds[fd]; }
  });
} catch (e) {}

send({ type: 'log', msg: 'write_plaintext_watch 就绪' });
