// jjssjd: 扫描进程内存找 Lua 源文本注册表 / 热更 DLL
'use strict';
function log(s) { send({ t: 'log', m: s }); }

var PATS = [
  { n: 'GameSystem', p: '47 61 6d 65 53 79 73 74 65 6d' },
  { n: 'AutoGenerate', p: '41 75 74 6f 20 47 65 6e 65 72 61 74 65' },
  { n: 'MZPE', p: '4d 5a' }
];

function doScan() {
  var ranges = Process.enumerateRanges('r--');
  log('ranges=' + ranges.length);
  for (var k = 0; k < PATS.length; k++) {
    var pat = PATS[k];
    var total = 0;
    var hits = [];
    for (var i = 0; i < ranges.length; i++) {
      var r = ranges[i];
      if (r.size > (512 << 20)) { log('skip huge ' + r.base + ' ' + r.size); continue; }
      var res = null;
      try { res = Memory.scanSync(r.base, r.size, pat.p); } catch (e) { continue; }
      if (res && res.length) {
        total += res.length;
        if (hits.length < 40) {
          hits.push({ base: r.base.toString(), size: r.size, prot: r.protection, file: r.file ? r.file.path : null, n: res.length, first: res[0].address.toString() });
        }
      }
    }
    log('[' + pat.n + '] total=' + total);
    send({ t: 'scan', pat: pat.n, total: total, hits: hits });
  }
}

// DLL 候选：MZ + PE\0\0
function scanPE() {
  var out = [];
  var ranges = Process.enumerateRanges('r--');
  for (var i = 0; i < ranges.length; i++) {
    var r = ranges[i];
    if (r.size > (512 << 20) || r.size < 4096) continue;
    var res = null;
    try { res = Memory.scanSync(r.base, r.size, '4d 5a'); } catch (e) { continue; }
    for (var j = 0; j < res.length; j++) {
      try {
        var a = res[j].address;
        var e_lfanew = a.add(0x3c).readU32();
        if (e_lfanew < 0x40 || e_lfanew > 0x400) continue;
        var sig = a.add(e_lfanew).readU32();
        if (sig !== 0x00004550) continue;   // "PE\0\0"
        out.push(a.toString());
        if (out.length >= 30) break;
      } catch (e) {}
    }
    if (out.length >= 30) break;
  }
  log('PE candidates=' + out.length);
  send({ t: 'pe', list: out });
}

setTimeout(function () { try { doScan(); } catch (e) { log('scan err ' + e); } }, 500);
setTimeout(function () { try { scanPE(); } catch (e) { log('pe err ' + e); } }, 15000);
