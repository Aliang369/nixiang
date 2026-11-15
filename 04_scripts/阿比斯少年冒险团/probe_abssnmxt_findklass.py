
import frida, json, time
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
function cstr(p){ try { var s=p.readCString(); if(!s) return null; if (s.length>64) return null; for (var i=0;i<s.length;i++){ var c=s.charCodeAt(i); if (c<32||c>126) return null; } return s; } catch(e){ return null; } }
rpc.exports = {
  step1FindString: function () {
    var hits = [];
    var ranges = Process.enumerateRanges("rw-");
    var t0 = Date.now();
    for (var i = 0; i < ranges.length; i++) {
      var r = ranges[i];
      if (r.size < 0x1000 || r.size > 0x20000000) continue;
      try {
        Memory.scanSync(r.base, r.size, "55 6e 69 74 00", {
          onMatch: function (a) { if (hits.length < 400) hits.push(a.toString()); },
          onComplete: function () {}
        });
      } catch (e) {}
      if (Date.now() - t0 > 60000) break;
    }
    return {ranges: ranges.length, hits: hits.length, sample: hits.slice(0, 8), ms: Date.now()-t0};
  },
  step2PtrTo: function (targets, maxHits) {
    var out = [];
    for (var t = 0; t < targets.length; t++) {
      var a = ptr(targets[t]);
      var q = a.toString().replace("0x","");
      while (q.length < 16) q = "0" + q;
      var bytes = [];
      for (var i = 14; i >= 0; i -= 2) bytes.push(q.substr(i,2));
      var pat = bytes.join(" ");
      var ranges = Process.enumerateRanges("rw-");
      for (var r = 0; r < ranges.length; r++) {
        var rg = ranges[r];
        if (rg.size < 0x1000 || rg.size > 0x20000000) continue;
        try {
          Memory.scanSync(rg.base, rg.size, pat, {
            onMatch: function (addr) {
              if (out.length < maxHits) {
                var klass = addr.sub(0x10);
                var nm = cstr(klass.add(0x10).readPointer());
                var ns = cstr(klass.add(0x18).readPointer());
                if (nm === "Unit") out.push({str: targets, klass: klass.toString(), ns: ns});
              }
            },
            onComplete: function () {}
          });
        } catch (e) {}
      }
      if (out.length >= maxHits) break;
    }
    return {found: out.length, items: out.slice(0, 6)};
  }
};
"""
script = s.create_script(AGENT)
script.load()
t0 = time.time()
r1 = script.exports_sync.step1FindString()
print("step1 string 'Unit' hits:", r1["hits"], "ms", r1["ms"], "sample", r1["sample"][:5])
if r1["hits"]:
    r2 = script.exports_sync.step2PtrTo(r1["sample"][:8], 4)
    print("step2:", json.dumps(r2)[:600])
print("elapsed", round(time.time()-t0,1))
s.detach()
