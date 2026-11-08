'use strict';
/*
 * dwdlm(动物大联盟 1.0.25) APKSecure 壳 libsecret.so 探针
 * 目的：dump 0x55a78（解析内存 APK v2 签名块、返回「签名串」）的真实返回结构，
 *       以及 key-unwrap 之后的 .bss 三元组 flag/IV/KEY。
 * 只读：不 patch 任何指令、不写设备文件、不改进程内存。
 *
 * 约定（来自 D 报告，实测）：0x55a78 用 sret，调用者 x8 传返回对象地址，
 * 调用点 0x56390 bl 0x55a78，sret = sp+0x70。
 */
(function () {
  var SO_NAME = 'libsecret.so';
  var OFF_SIG   = 0x55a78;   // 签名串解析函数入口
  var OFF_FLAG  = 0x786b0;   // u32 LE, ==1 表示 KEY/IV 已算完
  var OFF_IV    = 0x787a0;   // 16B
  var OFF_KEY   = 0x788a0;   // 32B

  var base = null;
  var hooked = false;
  var sigCalls = 0;
  var pollTimer = null;
  var pollN = 0;
  var pollMax = 800;         // 25ms * 800 = 20s
  var lastSnapKey = '';
  var keyCapturedAt = -1;
  var t0 = Date.now();

  function nowMs() { return Date.now() - t0; }

  function hexstr(ab) {
    var u = new Uint8Array(ab), s = '', i, h;
    for (i = 0; i < u.length; i++) { h = u[i].toString(16); if (h.length < 2) { h = '0' + h; } s += h; }
    return s;
  }
  function asciiOf(ab, max) {
    var u = new Uint8Array(ab), n = Math.min(u.length, max || 256), s = '', i, c;
    for (i = 0; i < n; i++) { c = u[i]; s += (c >= 0x20 && c < 0x7f) ? String.fromCharCode(c) : '.'; }
    return s;
  }
  function b64(ab) {
    try {
      var u = new Uint8Array(ab), s = '', i, chunk;
      for (i = 0; i < u.length; i += 3) {
        chunk = [u[i], i + 1 < u.length ? u[i + 1] : -1, i + 2 < u.length ? u[i + 2] : -1];
        var n = (chunk[0] << 16) | ((chunk[1] < 0 ? 0 : chunk[1]) << 8) | (chunk[2] < 0 ? 0 : chunk[2]);
        s += 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'[(n >> 18) & 63];
        s += 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'[(n >> 12) & 63];
        s += chunk[1] < 0 ? '=' : 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'[(n >> 6) & 63];
        s += chunk[2] < 0 ? '=' : 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'[n & 63];
      }
      return s;
    } catch (e) { return ''; }
  }
  function readSafe(addr, n) {
    try { if (addr === null || addr.isNull()) { return null; } return addr.readByteArray(n); } catch (e) { return null; }
  }
  function u32le(ab) {
    var u = new Uint8Array(ab);
    return (u[0] | (u[1] << 8) | (u[2] << 16) | (u[3] << 24)) >>> 0;
  }
  function isZeroHex(h) { return /^0*$/.test(h || ''); }

  /* ---------- 候选字符串分类 ---------- */
  function classify(ab) {
    var u = new Uint8Array(ab), i, c;
    var der = u.length >= 4 && u[0] === 0x30 && (u[1] === 0x82 || u[1] === 0x81 || u[1] === 0x30);
    var hexish = u.length > 0, b64ish = u.length > 0, printable = true;
    for (i = 0; i < u.length; i++) {
      c = u[i];
      var isHexCh = (c >= 0x30 && c <= 0x39) || (c >= 0x61 && c <= 0x66) || (c >= 0x41 && c <= 0x46);
      var isB64Ch = isHexCh || c === 0x2b || c === 0x2f || c === 0x3d;
      if (!isHexCh) { hexish = false; }
      if (!isB64Ch) { b64ish = false; }
      if (c < 0x20 || c >= 0x7f) { printable = false; }
    }
    return { der: der, hexish: hexish, b64ish: b64ish, printable: printable, len: u.length,
             head4: hexstr(u.subarray(0, Math.min(4, u.length))) };
  }

  /* ---------- 把 sret 指向的对象整体摊开 ---------- */
  function dumpObject(o, tag) {
    var out = { t: 'sig_ret_obj', tag: tag, addr: o.toString(), at_ms: nowMs() };
    var raw = readSafe(o, 0x40);
    out.raw64_hex = raw ? hexstr(raw) : null;
    if (!raw) { out.err = 'sret 不可读'; send(out); return; }

    /* 1) 逐 8 字节偏移尝试 libc++ std::string {ptr@0, size@8} 形态（含结构体前导字段的情况） */
    out.slots = [];
    var off, i;
    for (off = 0; off <= 0x38; off += 8) {
      var slot = { off: off };
      var p = null, sz = null;
      try { p = o.add(off).readPointer(); slot.ptr = p.toString(); } catch (e) { slot.ptr = null; }
      try { sz = o.add(off + 8).readU64().toString(); slot.size = sz; } catch (e) { slot.size = null; }
      /* 也记录该偏移的 u64 数值，便于识别长度字段 */
      try { slot.u64 = o.add(off).readU64().toString(); } catch (e) {}
      if (p !== null && !p.isNull() && sz !== null) {
        var len = parseInt(sz, 10);
        if (len > 0 && len < 65536) {
          var ab = readSafe(p, len);
          if (ab) {
            var c = classify(ab);
            slot.str = { len: len, hex_head: hexstr(ab.subarray(0, Math.min(256, len))),
                         ascii_head: asciiOf(ab, 256), cls: c, b64_full: len <= 8192 ? b64(ab) : null,
                         b64_truncated: len > 8192 };
          } else { slot.str = { len: len, read_fail: true }; }
        }
      }
      out.slots.push(slot);
    }

    /* 2) libc++ 短串(SSO)形态：inline 数据在 offset 1，长度在 offset 16，低位标记 */
    try {
      var b16 = o.add(16).readU8();
      var ssoLen = b16 >> 1;
      if ((b16 & 1) === 1 && ssoLen > 0 && ssoLen < 23) {
        var sab = readSafe(o.add(1), ssoLen);
        if (sab) { out.sso = { flagByte: b16, len: ssoLen, hex: hexstr(sab), ascii: asciiOf(sab, ssoLen), cls: classify(sab) }; }
      } else { out.sso = { flagByte: b16, note: 'not short-string layout' }; }
    } catch (e) {}

    /* 3) 直接把前 0x40 当可打印串试一下 */
    out.raw_ascii = asciiOf(raw, 0x40);
    send(out);
  }

  /* ---------- 0x55a78 hook ---------- */
  function attachSig() {
    try {
      Interceptor.attach(base.add(OFF_SIG), {
        onEnter: function (args) {
          sigCalls++;
          this.sret = this.context.x8;
          this.args = [this.context.x0, this.context.x1, this.context.x2, this.context.x3];
          this.lr = this.context.lr;
          this.sp = this.context.sp;
          send({ t: 'sig_enter', n: sigCalls, at_ms: nowMs(), sret: this.sret.toString(),
                 x0: this.args[0].toString(), x1: this.args[1].toString(),
                 x2: this.args[2].toString(), x3: this.args[3].toString(),
                 sp: this.sp.toString(), lr: this.lr.toString(), base: base.toString() });
          /* 若 x0..x3 中有可读指针，顺手记录前 32 字节，便于判断入参形态 */
          var regs = ['x0', 'x1', 'x2', 'x3'], i;
          for (i = 0; i < 4; i++) {
            var ab = readSafe(this.args[i], 32);
            if (ab) { send({ t: 'sig_arg_head', n: sigCalls, reg: regs[i], hex: hexstr(ab), ascii: asciiOf(ab, 32) }); }
          }
        },
        onLeave: function (ret) {
          send({ t: 'sig_leave', n: sigCalls, at_ms: nowMs(), x0: ret.toString() });
          try { dumpObject(this.sret, 'sret'); } catch (e) {
            send({ t: 'error', where: 'dumpObject', msg: String(e) });
          }
        }
      });
      send({ t: 'note', msg: 'attach 0x55a78 ok', base: base.toString(), at_ms: nowMs() });
    } catch (e) {
      send({ t: 'error', where: 'attachSig', msg: String(e) });
    }
  }

  /* ---------- .bss 轮询（比定时快照更保险：崩溃前也能抓到） ---------- */
  function readBss() {
    var fl = readSafe(base.add(OFF_FLAG), 4);
    var iv = readSafe(base.add(OFF_IV), 16);
    var key = readSafe(base.add(OFF_KEY), 32);
    if (!fl || !iv || !key) { return { ok: false, err: 'bss 读取失败(可能段未映射)' }; }
    return { ok: true, flag_hex: hexstr(fl), flag_u32: u32le(fl),
             iv_hex: hexstr(iv), key_hex: hexstr(key),
             flag_zero: isZeroHex(hexstr(fl)), key_zero: isZeroHex(hexstr(key)), iv_zero: isZeroHex(hexstr(iv)) };
  }
  function pollOnce() {
    pollN++;
    var b = readBss();
    if (!b.ok) {
      if (pollN % 200 === 0) { send({ t: 'bss', at_ms: nowMs(), poll: pollN, err: b.err }); }
      return;
    }
    var sig = b.flag_hex + '|' + b.key_hex + '|' + b.iv_hex;
    if (sig !== lastSnapKey) {
      lastSnapKey = sig;
      if (b.key_zero === false && keyCapturedAt < 0) { keyCapturedAt = nowMs(); }
      send({ t: 'bss', at_ms: nowMs(), poll: pollN, why: 'changed', flag_hex: b.flag_hex, flag_u32: b.flag_u32,
             iv_hex: b.iv_hex, key_hex: b.key_hex, key_nonzero: b.key_zero === false,
             first_key_at_ms: keyCapturedAt });
    }
  }
  function startPolling() {
    if (pollTimer) { return; }
    pollOnce();
    pollTimer = setInterval(function () {
      pollOnce();
      if (pollN >= pollMax) { clearInterval(pollTimer); pollTimer = null; send({ t: 'note', msg: 'bss 轮询结束', poll: pollN }); }
    }, 25);
    send({ t: 'note', msg: 'bss 轮询已启动(25ms)', at_ms: nowMs() });
  }

  function onModule(m) {
    if (hooked) { return; }
    hooked = true;
    base = m.base;
    send({ t: 'module', name: m.name, path: m.path, base: base.toString(), size: m.size, at_ms: nowMs() });
    attachSig();
    startPolling();
  }

  /* ---------- 模块加载捕获：三种手段并用，先到先算 ---------- */
  (function () {
    var m = null;
    try { m = Process.findModuleByName(SO_NAME); } catch (e) {}
    if (m) { onModule(m); return; }

    try {
      if (Process.attachModuleObserver) {
        Process.attachModuleObserver({
          onAdded: function (mod) {
            if (mod && mod.name === SO_NAME) { try { onModule(mod); } catch (e) { send({ t: 'error', where: 'observer', msg: String(e) }); } }
          }
        });
        send({ t: 'note', msg: 'module observer installed' });
      }
    } catch (e) { send({ t: 'error', where: 'attachModuleObserver', msg: String(e) }); }

    function findGlobal(n) {
      try { if (Module.findGlobalExportByName) { var a = Module.findGlobalExportByName(n); if (a) { return a; } } } catch (e) {}
      try { if (Module.findExportByName) { var b = Module.findExportByName(null, n); if (b) { return b; } } } catch (e) {}
      try { var d = Process.getModuleByName('libdl.so'); var c = d.findExportByName(n); if (c) { return c; } } catch (e) {}
      return null;
    }
    function hookDlopen(name, isExt) {
      var a = findGlobal(name);
      if (!a) { return; }
      try {
        Interceptor.attach(a, {
          onEnter: function (args) {
            try { this.path = isExt ? args[1].readUtf8String() : args[0].readUtf8String(); } catch (e) { this.path = null; }
          },
          onLeave: function () {
            if (hooked) { return; }
            if (this.path && this.path.indexOf('libsecret.so') >= 0) {
              var mod = null;
              try { mod = Process.findModuleByName(SO_NAME); } catch (e) {}
              if (mod) { onModule(mod); }
            }
          }
        });
        send({ t: 'note', msg: 'dlopen hook: ' + name });
      } catch (e) { send({ t: 'error', where: 'hookDlopen:' + name, msg: String(e) }); }
    }
    hookDlopen('android_dlopen_ext', true);
    hookDlopen('dlopen', false);

    var iv = setInterval(function () {
      if (hooked) { clearInterval(iv); return; }
      var mod = null;
      try { mod = Process.findModuleByName(SO_NAME); } catch (e) {}
      if (mod) { clearInterval(iv); onModule(mod); }
    }, 10);

    send({ t: 'note', msg: 'waiting for ' + SO_NAME });
  })();

  /* ---------- RPC：外部可强制要一次快照 ---------- */
  rpc.exports = {
    snapshot: function () {
      var b = base ? readBss() : { ok: false, err: 'base 未获取' };
      return { base: base ? base.toString() : null, hooked: hooked, sig_calls: sigCalls,
               at_ms: nowMs(), bss: b, last_key_hex: lastSnapKey };
    },
    sigcalls: function () { return { sig_calls: sigCalls, hooked: hooked, base: base ? base.toString() : null }; }
  };
})();
