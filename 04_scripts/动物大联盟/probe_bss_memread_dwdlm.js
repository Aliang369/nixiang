'use strict';
/*
 * dwdlm(动物大联盟 1.0.25) — 通过 frida 挂在 su/ksu 域进程上，
 * 用 File API 直读目标 app 进程的 /proc/<pid>/mem，快照壳 libsecret.so 的 .bss 三元组。
 *
 * 为什么：直接 attach app 进程被 SELinux 阻断（agent 在 untrusted_app 域起不来，
 * frida-server 也装不上自己的 selinux policy：Unable to save SELinux policy ... Out of memory），
 * 但 attach 同域进程可行，且 root 可 open /proc/<app>/mem（实测 300 次 528B 读 6ms）。
 *
 * Frida 17 已移除 Process.enumerateProcesses，pid 由 runner 侧 pidof 经 rpc.watch(pid) 传入。
 * 本脚本只读：不写目标内存、不 patch 指令。
 */
(function () {
  var SO = 'libsecret.so';
  var OFF_SWITCH  = 0x660cd;  // .data 首字节应为 0x01（校验 base）
  var OFF_CONST32 = 0x660de;  // 32B 静态常量（校验 base）
  var OFF_FLAG = 0x786b0, OFF_IV = 0x787a0, OFF_KEY = 0x788a0;
  var WIN_START = 0x786b0, WIN_LEN = 0x210;   // flag .. KEY+32
  var POLL_MS = 2, MAX_MS = 90000;

  var t0 = Date.now();
  function now() { return Date.now() - t0; }
  function hex(ab, max) {
    if (!ab) { return null; }
    var u = new Uint8Array(ab), n = max ? Math.min(max, u.length) : u.length, s = '', i, h;
    for (i = 0; i < n; i++) { h = u[i].toString(16); if (h.length < 2) { h = '0' + h; } s += h; }
    return s;
  }
  function toText(ab) {
    var u = new Uint8Array(ab), s = '', i, CH = 4096;
    for (i = 0; i < u.length; i += CH) { s += String.fromCharCode.apply(null, u.subarray(i, Math.min(i + CH, u.length))); }
    return s;
  }
  function readFileBytes(path, n) {
    var f = null;
    try { f = new File(path, 'rb'); var b = f.readBytes(n); f.close(); return b; }
    catch (e) { try { if (f) { f.close(); } } catch (_) {} return null; }
  }
  function u32le(u, off) { return ((u[off] | (u[off + 1] << 8) | (u[off + 2] << 16) | (u[off + 3] << 24)) >>> 0); }

  function findBase(pid) {
    var ab = readFileBytes('/proc/' + pid + '/maps', 2 * 1024 * 1024);
    if (!ab) { return { err: 'maps 读取失败' }; }
    var lines = toText(ab).split('\n'), best = null, hits = [], i;
    for (i = 0; i < lines.length; i++) {
      var L = lines[i];
      if (L.indexOf(SO) < 0) { continue; }
      hits.push(L.trim());
      var m = L.match(/^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+([0-9a-f]+)/);
      if (!m) { continue; }
      var b = parseInt(m[1], 16) - parseInt(m[4], 16);
      if (best === null || b < best) { best = b; }
    }
    if (best === null) { return { err: 'no-so' }; }
    return { base: best, maps: hits };
  }

  var files = {}, watch = {}, lastNote = 0;
  function memFile(pid) {
    if (files[pid]) { return files[pid]; }
    var f = new File('/proc/' + pid + '/mem', 'rb');
    files[pid] = f;
    return f;
  }
  function dropMem(pid) { try { if (files[pid]) { files[pid].close(); } } catch (e) {} delete files[pid]; }

  function register(pid, name) {
    var w = watch[pid] || { pid: pid, name: name, base: null, lastSig: '', tried: 0, dead: false };
    watch[pid] = w;
    if (w.base) { return; }
    var r = findBase(pid);
    if (r.err === 'no-so') { w.tried++; if (w.tried === 1 || w.tried % 500 === 0) { send({ t: 'note', pid: pid, msg: 'maps 里还没有 ' + SO + '（第 ' + w.tried + ' 次）' }); } return; }
    if (r.err) { send({ t: 'error', where: 'findBase', pid: pid, msg: r.err }); return; }
    w.base = r.base;
    var sw = null, c32 = null;
    try {
      var f = memFile(pid);
      f.seek(r.base + OFF_SWITCH); sw = hex(f.readBytes(1));
      f.seek(r.base + OFF_CONST32); c32 = hex(f.readBytes(32));
    } catch (e) { send({ t: 'error', where: 'verifyBase', pid: pid, msg: String(e) }); }
    send({ t: 'found', pid: pid, name: name, base: r.base.toString(), switch_byte: sw,
           const32_hex: c32, maps: r.maps, at_ms: now() });
  }

  function poll() {
    if (now() > MAX_MS) { send({ t: 'note', msg: '达到最大监听时长，停止' }); clearInterval(timer); return; }
    var pids = Object.keys(watch), i;
    for (i = 0; i < pids.length; i++) {
      var pid = parseInt(pids[i], 10), w = watch[pid];
      if (w.dead) { continue; }
      if (!w.base) { register(pid, w.name); if (!w.base) { continue; } }
      var ab = null, ok = false;
      try {
        var f = memFile(pid);
        f.seek(w.base + WIN_START);
        ab = f.readBytes(WIN_LEN);
        ok = true;
      } catch (e) {
        dropMem(pid);
        try { var f2 = memFile(pid); f2.seek(w.base + WIN_START); ab = f2.readBytes(WIN_LEN); ok = true; }
        catch (e2) {
          w.fail = (w.fail || 0) + 1;
          if (w.fail === 20) { w.dead = true; send({ t: 'dead', pid: pid, msg: String(e2), at_ms: now() }); }
          continue;
        }
      }
      if (!ok || !ab) { continue; }
      var u = new Uint8Array(ab);
      var h = hex(ab);
      var kOff = (OFF_KEY - WIN_START) * 2, iOff = (OFF_IV - WIN_START) * 2;
      var keyHex = h.substring(kOff, kOff + 64), ivHex = h.substring(iOff, iOff + 32), flHex = h.substring(0, 8);
      var sig = flHex + ivHex + keyHex;
      if (sig !== w.lastSig) {
        var keyNZ = /[1-9a-f]/.test(keyHex);
        var firstKey = keyNZ && !w.keySeen;
        if (keyNZ) { w.keySeen = true; }
        w.lastSig = sig;
        send({ t: 'bss', pid: pid, at_ms: now(), flag_hex: flHex, flag_u32: u32le(u, 0),
               iv_hex: ivHex, key_hex: keyHex, key_nonzero: keyNZ, first_key: firstKey,
               base: w.base.toString(), window_hex: h });
      }
      w.samples = (w.samples || 0) + 1;
    }
  }

  var timer = setInterval(poll, POLL_MS);
  send({ t: 'ready', msg: 'watcher 已启动', self: Process.id, poll_ms: POLL_MS, at_ms: now() });

  rpc.exports = {
    watch: function (pid) { watch[pid] = watch[pid] || { pid: pid, name: '', base: null, lastSig: '', tried: 0, dead: false }; register(pid, ''); return true; },
    unwatch: function (pid) { if (watch[pid]) { watch[pid].dead = true; } return true; },
    status: function () {
      return { at_ms: now(), self: Process.id, pids: Object.keys(watch).map(function (k) {
        var w = watch[k];
        return { pid: parseInt(k, 10), base: w.base ? w.base.toString() : null, key_seen: !!w.keySeen,
                 samples: w.samples || 0, dead: !!w.dead, last: w.lastSig };
      }) };
    },
    stop: function () { clearInterval(timer); return 'stopped'; }
  };
})();
