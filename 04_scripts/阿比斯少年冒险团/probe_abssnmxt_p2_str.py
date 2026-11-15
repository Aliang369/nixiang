
import frida, json, re, sys, time

PID = 18425
MAPS = r"E:\nixiang\06_dumped\阿比斯少年冒险团\_maps.txt"
ranges = []
for line in open(MAPS, encoding="utf-8", errors="replace"):
    m = re.match(r'^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s', line)
    if not m: continue
    a, b, perms = int(m.group(1), 16), int(m.group(2), 16), m.group(3)
    if "r" not in perms: continue
    if not (a >= 0x400000000000 or a >= 0x730000000000): continue
    size = b - a
    if size <= 0 or size > 0x20000000: continue
    ranges.append([a, size])
# dedupe/merge nothing; cap total
total = sum(r[1] for r in ranges)
print("scan ranges:", len(ranges), "total MB:", round(total/1048576,1))

dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
rpc.exports = {
  scanStr: function (ranges, hexpat, cap) {
    var hits = [];
    for (var i = 0; i < ranges.length && hits.length < cap; i++) {
      try {
        Memory.scanSync(ptr(ranges[i][0]), ranges[i][1], hexpat, {
          onMatch: function (a) { if (hits.length < cap) hits.push(a.toString()); },
          onComplete: function () {}
        });
      } catch (e) {}
    }
    return hits;
  },
  scanPtr: function (ranges, addrs, capPer) {
    var out = [];
    for (var k = 0; k < addrs.length; k++) {
      var a = ptr(addrs[k]);
      var q = a.toString(16);
      while (q.length < 16) q = "0" + q;
      var bytes = [];
      for (var i = 14; i >= 0; i -= 2) bytes.push(q.substr(i, 2));
      var pat = bytes.join(" ");
      var cnt = 0;
      for (var i = 0; i < ranges.length && cnt < capPer; i++) {
        try {
          Memory.scanSync(ptr(ranges[i][0]), ranges[i][1], pat, {
            onMatch: function (addr) {
              if (cnt < capPer) { out.push({target: a.toString(), at: addr.toString()}); cnt++; }
            },
            onComplete: function () {}
          });
        } catch (e) {}
      }
    }
    return out;
  },
  probe: function (addrs) {
    var res = [];
    for (var i = 0; i < addrs.length; i++) {
      var p = ptr(addrs[i]);
      var rec = {at: addrs[i]};
      try { rec.q0 = p.readPointer().toString(); } catch (e) { rec.q0 = "ERR"; }
      for (var off = 0; off <= 0x28; off += 8) {
        try {
          var v = p.add(off).readPointer();
          var s2 = v.readCString();
          if (s2 !== null && s2.length < 40) rec["s0x" + off.toString(16)] = s2;
        } catch (e) {}
      }
      res.push(rec);
    }
    return res;
  }
};
"""
script = s.create_script(AGENT)
script.load()
t0 = time.time()
hits = script.exports_sync.scanStr(ranges, "55 6e 69 74 00", 300)
print("step1 'Unit' hits:", len(hits), "elapsed", round(time.time()-t0,1), "s")
print("sample:", hits[:6])
json.dump({"ranges": ranges, "unit_str_hits": hits}, open(r"E:\nixiang\06_dumped\阿比斯少年冒险团\_p2_step1.json", "w"))
s.detach()
