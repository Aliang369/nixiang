'use strict';
/*
 * dwdlm(动物大联盟 1.0.25) libsecret.so 签名串探针 v2
 *
 * v1 失败原因（2026-11-05 复现）：dev.attach(pid) 直接抛 TransportError
 *   "agent connection closed unexpectedly" —— SELinux(enforcing) 拒绝 ptrace，
 *   脚本在 attach 阶段就退出，probe.log 只有头、probe_raw.jsonl 0 字节。
 *   与映像选择无关；但映像选择确实也是隐患（见下）。
 *
 * v2 修复：
 *  1) 不再用 Process.findModuleByName（双加载时只返回一个映像）。改为解析
 *     /proc/self/maps（enumerateRanges('r-x') 优先，File 读 maps 兜底），
 *     取 **所有** path 含 libsecret.so 且 offset==00000000 的行，逐个算 base，
 *     全部 attach；每处 attach 成功/失败都单独上报，不静默。
 *  2) 0x55a78 静态确认为 sret(std::string)：调用点 0x56390 前 add x0, sp, #0x70；
 *     函数内 0x55f60 str x8,[x19,#0x10] / 0x55f78 返回，布局 = +0 data ptr /
 *     +8 len / +16 cap。onLeave 按此布局读，另无条件 dump 返回结构 48 字节 raw。
 *  3) 追加 0x563a4（bl 0x55a78 之后，cbz x23 处）hook，读 sp+0x70 交叉验证。
 *  4) .bss 采样加入 0x78858（0x55fbc..0x55ffc 用 0x12114 的 "0123456789abcdef"
 *     表把 32 字节转 64 字符 hex 后写入的缓存）与 0x788a0(KEY)/0x787a0(IV)。
 */
(function () {
  var SO = 'libsecret.so';
  var OFF_SIG = 0x55a78;
  var OFF_CALL = 0x563a4;
  var OFF_FLAG = 0x786a4;   // u32 ==1 表示 0x78858 缓存有效
  var OFF_FLAG2 = 0x786a8;  // u32 非零 = 有值
  var OFF_P3 = 0x786b4;     // u32，作为 w3 传入 0x55a78
  var OFF_HEX = 0x78858;    // 64 字符小写 hex + NUL
  var OFF_KEY = 0x788a0;    // 32B
  var OFF_IV = 0x787a0;     // 16B
  var MAX_DETAIL = 40;

  var t0 = Date.now();
  function nowMs() { return Date.now() - t0; }

  function hexstr(ab) {
    var u = new Uint8Array(ab), s = '', i, h;
    for (i = 0; i < u.length; i++) { h = u[i].toString(16); if (h.length < 2) { h = '0' + h; } s += h; }
    return s;
  }
  function asciiOf(ab, max) {
    var u = new Uint8Array(ab), n = Math.min(u.length, max || u.length), s = '', i, c;
    for (i = 0; i < n; i++) { c = u[i]; s += (c >= 0x20 && c < 0x7f) ? String.fromCharCode(c) : '.'; }
    return s;
  }
  function readSafe(addr, n) {
    try { if (!addr || addr.isNull()) { return null; } return addr.readByteArray(n); } catch (e) { return null; }
  }
  function u32At(addr) { try { return addr.readU32(); } catch (e) { return null; } }
  function u64At(addr) { try { return parseInt(addr.readU64().toString(), 10); } catch (e) { return null; } }

  function b64(ab) {
    var T = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
    try {
      var u = new Uint8Array(ab), s = '', i, a, b, c, n;
      for (i = 0; i < u.length; i += 3) {
        a = u[i]; b = (i + 1 < u.length) ? u[i + 1] : -1; c = (i + 2 < u.length) ? u[i + 2] : -1;
        n = (a << 16) | ((b < 0 ? 0 : b) << 8) | (c < 0 ? 0 : c);
        s += T[(n >> 18) & 63] + T[(n >> 12) & 63] +
             (b < 0 ? '=' : T[(n >> 6) & 63]) + (c < 0 ? '=' : T[n & 63]);
      }
      return s;
    } catch (e) { return ''; }
  }

  function classify(ab) {
    var u = new Uint8Array(ab), i, c;
    var isHex64 = false, hexish = u.length > 0, b64ish = u.length > 0, printable = true;
    for (i = 0; i < u.length; i++) {
      c = u[i];
      var isHexCh = (c >= 0x30 && c <= 0x39) || (c >= 0x61 && c <= 0x66) || (c >= 0x41 && c <= 0x46);
      var isB64Ch = isHexCh || c === 0x2b || c === 0x2f || c === 0x3d;
      if (!isHexCh) { hexish = false; }
      if (!isB64Ch) { b64ish = false; }
      if (c < 0x20 || c >= 0x7f) { printable = false; }
    }
    if (u.length === 64 && hexish) { isHex64 = true; }
    return {
      len: u.length, is_hex64: isHex64, hexish: hexish, b64ish: b64ish, printable: printable,
      der_3082: u.length >= 4 && u[0] === 0x30 && u[1] === 0x82,
      der_30: u.length >= 2 && u[0] === 0x30,
      head4: hexstr(u.subarray(0, Math.min(4, u.length)))
    };
  }

  /* 读一个 std::string(24B) 结构，主路径 + SSO 兜底 */
  function emitStringObj(objp, tag, kind, n) {
    var raw = readSafe(objp, 0x30);
    var o = { t: 'strobj', tag: tag, kind: kind, n: n, addr: objp.toString(), at_ms: nowMs() };
    if (!raw) { o.err = 'structure unreadable'; send(o); return; }
    o.raw48_hex = hexstr(raw);
    var u = new Uint8Array(raw);
    var p = null;
    try { p = objp.readPointer(); o.ptr = p.toString(); } catch (e) { o.ptr = null; }
    o.len = u64At(objp.add(8));
    o.cap = u64At(objp.add(16));
    o.cap_bit0 = (o.cap === null) ? null : (o.cap & 1);
    o.inline_ascii = asciiOf(u.subarray(0, 23), 23);
    if (p && !p.isNull() && o.len !== null && o.len > 8 && o.len < 4096) {
      var s = readSafe(p, o.len);
      if (s) {
        o.data_hex = hexstr(s);
        o.data_ascii = asciiOf(s, Math.min(o.len, 400));
        o.data_b64 = o.len <= 2048 ? b64(s) : null;
        o.data_b64_truncated = o.len > 2048;
        o.cls = classify(s);
      } else { o.data_err = 'data ptr unreadable'; }
    } else if (o.len !== null && o.len > 0 && o.len < 24) {
      var s2 = readSafe(objp, Math.min(o.len, 23));
      if (s2) { o.sso_hex = hexstr(s2); o.sso_ascii = asciiOf(s2, s2.byteLength); }
    }
    send(o);
  }

  /* ---------- base 发现：解析 maps 全部 offset==0 的 r-xp libsecret ---------- */
  var bases = [];   // [{base, path, src, size}]
  var attached = {};
  var tries = {};
  var stats = {};

  function addBase(base, src, path, size) {
    var k = base.toString();
    for (var i = 0; i < bases.length; i++) { if (bases[i].base.toString() === k) { return null; } }
    var b = { base: base, src: src, path: path || '', size: size || 0 };
    bases.push(b);
    return b;
  }

  function readText(path) {
    try {
      var f = new File(path, 'r');
      var parts = [], chunk;
      while (true) {
        chunk = f.readBytes(32768);
        if (chunk === null || chunk.byteLength === 0) { break; }
        var u = new Uint8Array(chunk), s = '', i, CH = 8192;
        for (i = 0; i < u.length; i += CH) {
          s += String.fromCharCode.apply(null, u.subarray(i, Math.min(i + CH, u.length)));
        }
        parts.push(s);
      }
      f.close();
      return parts.join('');
    } catch (e) { return ''; }
  }

  function findBases() {
    var found = 0;
    try {
      var rs = Process.enumerateRanges('r-x');
      for (var i = 0; i < rs.length; i++) {
        var r = rs[i], f = r.file;
        if (!f || !f.path) { continue; }
        if (f.path.indexOf(SO) < 0) { continue; }
        var off = (f.offset === undefined) ? 0 : f.offset;
        if (off !== 0) { continue; }
        if (addBase(r.base, 'enumerateRanges', f.path, r.size)) { found++; }
      }
    } catch (e) { send({ t: 'error', where: 'enumerateRanges', msg: String(e) }); }

    if (found === 0) {
      var txt = readText('/proc/self/maps');
      if (txt.length === 0) { send({ t: 'error', where: 'readMaps', msg: '/proc/self/maps 读不到或为空' }); }
      var lines = txt.split('\n');
      for (var j = 0; j < lines.length; j++) {
        var m = lines[j].match(/^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+([0-9a-f]+)\s+\S+\s+\d+\s+(.*)$/);
        if (!m) { continue; }
        if (m[3].indexOf('x') < 0) { continue; }
        if (m[4] !== '00000000') { continue; }
        if (m[5].indexOf(SO) < 0) { continue; }
        var st = parseInt(m[1], 16), en = parseInt(m[2], 16);
        if (addBase(ptr(st), 'proc-maps', m[5], en - st)) { found++; }
      }
    }
    return found;
  }

  function attachSig(b, tag) {
    var key = tag + ':sig';
    if (attached[key] === 1) { return; }
    tries[key] = (tries[key] || 0) + 1;
    if (tries[key] > 6) { return; }
    var addr = b.base.add(OFF_SIG);
    try {
      Interceptor.attach(addr, {
        onEnter: function () {
          var s = stats[tag] || (stats[tag] = { sig: 0, call: 0 });
          s.sig++;
          this.sret = this.context.x8;
          this.sp = this.context.sp;
          this.lr = this.context.lr;
          this.x0 = this.context.x0; this.x1 = this.context.x1;
          this.x2 = this.context.x2; this.x3 = this.context.x3;
          if (s.sig <= MAX_DETAIL) {
            send({ t: 'sig_enter', tag: tag, n: s.sig, at_ms: nowMs(), base: b.base.toString(),
                   sret: this.sret.toString(), sp: this.sp.toString(), lr: this.lr.toString(),
                   x0: this.x0.toString(), x1: this.x1.toString(), x2: this.x2.toString(),
                   x3: this.x3.toString(), x3_u32: this.x3.toUInt32() });
            var h1 = readSafe(this.x1, 32);
            if (h1) { send({ t: 'sig_arg', tag: tag, n: s.sig, reg: 'x1', hex: hexstr(h1), ascii: asciiOf(h1, 32) }); }
            var h2 = readSafe(this.x2, 32);
            if (h2) { send({ t: 'sig_arg', tag: tag, n: s.sig, reg: 'x2', hex: hexstr(h2), ascii: asciiOf(h2, 32) }); }
          }
        },
        onLeave: function (ret) {
          var s = stats[tag];
          if (s && s.sig > MAX_DETAIL) { return; }
          send({ t: 'sig_leave', tag: tag, n: s ? s.sig : -1, at_ms: nowMs(), x0: ret.toString() });
          try { emitStringObj(this.sret, tag, 'sret', s ? s.sig : -1); }
          catch (e) { send({ t: 'error', where: 'emitStringObj/sret', msg: String(e) }); }
        }
      });
      attached[key] = 1;
      send({ t: 'attach_ok', what: 'sig', tag: tag, base: b.base.toString(), addr: addr.toString() });
    } catch (e) {
      send({ t: 'attach_fail', what: 'sig', tag: tag, base: b.base.toString(), addr: addr.toString(), msg: String(e) });
    }
  }

  function attachCall(b, tag) {
    var key = tag + ':call';
    if (attached[key] === 1) { return; }
    tries[key] = (tries[key] || 0) + 1;
    if (tries[key] > 6) { return; }
    var addr = b.base.add(OFF_CALL);
    try {
      Interceptor.attach(addr, {
        onEnter: function () {
          var s = stats[tag] || (stats[tag] = { sig: 0, call: 0 });
          s.call++;
          if (s.call > MAX_DETAIL) { return; }
          var objp = this.context.sp.add(0x70);
          send({ t: 'call_enter', tag: tag, n: s.call, at_ms: nowMs(), base: b.base.toString(),
                 sp: this.context.sp.toString(), obj: objp.toString(), x23: this.context.x23.toString() });
          try { emitStringObj(objp, tag, 'sp70', s.call); }
          catch (e) { send({ t: 'error', where: 'emitStringObj/sp70', msg: String(e) }); }
        }
      });
      attached[key] = 1;
      send({ t: 'attach_ok', what: 'call', tag: tag, base: b.base.toString(), addr: addr.toString() });
    } catch (e) {
      send({ t: 'attach_fail', what: 'call', tag: tag, base: b.base.toString(), addr: addr.toString(), msg: String(e) });
    }
  }

  function attachAll() {
    for (var i = 0; i < bases.length; i++) {
      var b = bases[i], tag = 'b' + i;
      if (!b.tag) { b.tag = tag; send({ t: 'base', tag: tag, base: b.base.toString(), src: b.src, path: b.path, size: b.size, at_ms: nowMs() }); }
      attachSig(b, tag);
      attachCall(b, tag);
    }
  }

  /* ---------- .bss 采样 ---------- */
  var lastSig = {};
  function snapBss(force) {
    for (var i = 0; i < bases.length; i++) {
      var b = bases[i], tag = b.tag || ('b' + i);
      var fl = readSafe(b.base.add(OFF_FLAG), 4);
      var f2 = readSafe(b.base.add(OFF_FLAG2), 4);
      var p3 = readSafe(b.base.add(OFF_P3), 4);
      var hx = readSafe(b.base.add(OFF_HEX), 96);
      var key = readSafe(b.base.add(OFF_KEY), 32);
      var iv = readSafe(b.base.add(OFF_IV), 16);
      if (!fl && !hx && !key) { continue; }
      var rec = {
        t: 'bss', tag: tag, at_ms: nowMs(), base: b.base.toString(),
        flag_u32: fl ? u32At(b.base.add(OFF_FLAG)) : null,
        flag2_u32: f2 ? u32At(b.base.add(OFF_FLAG2)) : null,
        p3_u32: p3 ? u32At(b.base.add(OFF_P3)) : null,
        hex_str: hx ? asciiOf(hx, 96).replace(/\.+$/, '') : null,
        hex_raw: hx ? hexstr(hx) : null,
        key_hex: key ? hexstr(key) : null,
        iv_hex: iv ? hexstr(iv) : null
      };
      var sig = (rec.hex_str || '') + '|' + (rec.key_hex || '') + '|' + (rec.iv_hex || '') + '|' + rec.flag_u32;
      if (!force && lastSig[tag] === sig) { continue; }
      lastSig[tag] = sig;
      rec.why = force ? 'force' : 'changed';
      send(rec);
    }
  }

  /* ---------- 主循环 ---------- */
  var bootTries = 0;
  var timer = null;
  function tick() {
    bootTries++;
    var before = bases.length;
    var n = findBases();
    if (bases.length > before) { attachAll(); snapBss(true); }
    else if (bases.length > 0) { attachAll(); snapBss(false); }
    else if (bootTries % 20 === 0) {
      send({ t: 'note', msg: 'maps 里还没出现 ' + SO, tries: bootTries, at_ms: nowMs() });
    }
  }

  send({ t: 'ready', msg: 'probe v2 启动', pid: Process.id, arch: Process.arch, at_ms: nowMs() });

  /* dlopen 提示（辅助确认 libsecret 何时被加载） */
  try {
    var names = ['android_dlopen_ext', 'dlopen'];
    for (var i = 0; i < names.length; i++) {
      var a = null;
      try { a = Module.findGlobalExportByName ? Module.findGlobalExportByName(names[i]) : null; } catch (e) { a = null; }
      if (!a) { try { a = Module.findExportByName(null, names[i]); } catch (e) { a = null; } }
      if (!a) { continue; }
      (function (nm) {
        try {
          Interceptor.attach(a, {
            onEnter: function (args) {
              try {
                var p = (nm === 'dlopen') ? args[0].readUtf8String() : args[1].readUtf8String();
                if (p && p.indexOf(SO) >= 0) { send({ t: 'note', msg: nm + ' -> ' + p, at_ms: nowMs() }); }
              } catch (e) {}
            }
          });
          send({ t: 'note', msg: 'dlopen 提示已挂: ' + nm });
        } catch (e) { send({ t: 'error', where: 'hookDlopen:' + nm, msg: String(e) }); }
      })(names[i]);
    }
  } catch (e) { send({ t: 'error', where: 'dlopen-setup', msg: String(e) }); }

  timer = setInterval(tick, 50);
  tick();

  rpc.exports = {
    snap: function () { snapBss(true); return { bases: bases.length, at_ms: nowMs() }; },
    stats: function () {
      var o = {};
      for (var k in stats) { o[k] = stats[k]; }
      return { bases: bases.length, stats: o, at_ms: nowMs() };
    },
    addrtab: function () {
      var out = [];
      for (var i = 0; i < bases.length; i++) {
        out.push({ tag: bases[i].tag || ('b' + i), base: bases[i].base.toString(), src: bases[i].src,
                   sig: bases[i].base.add(OFF_SIG).toString(), call: bases[i].base.add(OFF_CALL).toString(),
                   hex: bases[i].base.add(OFF_HEX).toString(), key: bases[i].base.add(OFF_KEY).toString() });
      }
      return out;
    }
  };
})();
