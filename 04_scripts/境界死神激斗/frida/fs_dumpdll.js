// jjssjd: dump raw PE images (HybridCLR assemblies) from anon regions
'use strict';
function log(s) { send({ t: 'log', m: s }); }

var CANDS = ['0x725d400000', '0x72a1c00000', '0x72a2363540', '0x72a24d0c00', '0x72a5200000', '0x74314784b4'];
var MAX = 80 * 1024 * 1024;

function analyze(baseStr) {
  var p = ptr(baseStr);
  var hdr = null;
  try { hdr = p.readByteArray(0x2000); } catch (e) { return { base: baseStr, bad: 'hdr ' + e }; }
  var d = new DataView(hdr);
  var r = { base: baseStr };
  var e_lfanew = d.getUint32(0x3c, true);
  r.e_lfanew = e_lfanew;
  if (d.getUint32(e_lfanew, true) !== 0x00004550) { r.bad = 'sig'; return r; }
  var nsec = d.getUint16(e_lfanew + 6, true);
  var optSize = d.getUint16(e_lfanew + 20, true);
  var magic = d.getUint16(e_lfanew + 24, true);
  r.nsec = nsec; r.magic = magic;
  var optOff = e_lfanew + 24;
  var ddOff = optOff + (magic === 0x20b ? 112 : 96);
  var corRva = d.getUint32(ddOff + 14 * 8, true);
  r.corRva = corRva;
  var secOff = optOff + optSize;
  var secs = [], fileEnd = 0;
  for (var i = 0; i < nsec && i < 32; i++) {
    var o = secOff + i * 40;
    var va = d.getUint32(o + 12, true);
    var vsz = d.getUint32(o + 8, true);
    var praw = d.getUint32(o + 20, true);
    var sraw = d.getUint32(o + 16, true);
    secs.push({ va: va, vsz: vsz, praw: praw, sraw: sraw });
    if (praw + sraw > fileEnd) fileEnd = praw + sraw;
  }
  r.secs = secs; r.fileEnd = fileEnd;
  function rva2off(rva) {
    for (var j = 0; j < secs.length; j++) {
      var s = secs[j];
      if (rva >= s.va && rva < s.va + Math.max(s.vsz, s.sraw)) return s.praw + (rva - s.va);
    }
    return -1;
  }
  var corOff = rva2off(corRva);
  r.corOff = corOff;
  if (corOff >= 0x2000) { r.note = 'corOff beyond hdr read'; return r; }
  if (corOff > 0) {
    var mdRva = d.getUint32(corOff + 8, true);
    var mdSz = d.getUint32(corOff + 12, true);
    r.cb = d.getUint32(corOff, true);
    r.mdRva = mdRva; r.mdSz = mdSz;
    var mdOff = rva2off(mdRva);
    r.mdOff = mdOff;
    if (mdOff >= 0 && mdOff < 0x1ffc) {
      r.metaSig = d.getUint32(mdOff, true).toString(16);  // 424a5342 == BSJB
      var vlen = d.getUint32(mdOff + 12, true);
      var vb = [];
      for (var k = 0; k < Math.min(vlen, 32); k++) vb.push(d.getUint8(mdOff + 16 + k));
      r.ver = String.fromCharCode.apply(null, vb);
    }
  }
  return r;
}

setTimeout(function () {
  CANDS.forEach(function (c, i) {
    var info = analyze(c);
    log('cand[' + i + '] ' + JSON.stringify(info));
    if (info.bad || !info.fileEnd || info.fileEnd > MAX) {
      log('skip ' + i);
      return;
    }
    try {
      var buf = ptr(c).readByteArray(info.fileEnd);
      send({ t: 'dll', idx: i, base: c, size: info.fileEnd }, buf);
      log('sent ' + i + ' ' + info.fileEnd);
    } catch (e) { log('read fail ' + i + ' ' + e); }
  });
  log('done');
}, 1200);
