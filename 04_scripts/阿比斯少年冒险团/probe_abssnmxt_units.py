
import frida, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
var MR = BASE.add(0x7ff3ea8);
function cstr(p){ try { return p.readCString(); } catch(e){ return null; } }
rpc.exports = {
  findClass: function (name) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var res = [];
    for (var i = 0; i < cnt; i++) {
      var k = arr.add(i*8).readPointer();
      if (k.isNull()) continue;
      var nm = cstr(k.add(0x10).readPointer());
      if (nm === name) {
        var ns = cstr(k.add(0x18).readPointer());
        var parent = k.add(0x38).readPointer();
        res.push({i:i, klass:k.toString(), ns:ns, parent:parent.isNull()?null:cstr(parent.add(0x10).readPointer())});
      }
    }
    return {count: cnt, res: res};
  },
  scanInstances: function (klassHex, maxHits) {
    var kp = ptr(klassHex);
    var needle = kp.toByteArray ? null : null;
    var pat = [];
    var b = new Uint8Array(kp.toString ? [] : []);
    // build pattern from pointer bytes
    var q = kp.toString().replace("0x","");
    while (q.length < 16) q = "0" + q;
    var bytes = [];
    for (var i = 14; i >= 0; i -= 2) bytes.push(q.substr(i,2));
    var pattern = bytes.join(" ");
    var hits = [];
    var ranges = Process.enumerateRanges("rw-");
    var scanned = 0;
    for (var r = 0; r < ranges.length && hits.length < maxHits; r++) {
      var rg = ranges[r];
      if (rg.size < 0x1000 || rg.size > 0x40000000) continue;
      if (scanned > 400 * 1024 * 1024) break;
      scanned += rg.size;
      try {
        Memory.scanSync(rg.base, rg.size, pattern, {
          onMatch: function (addr) {
            if (hits.length < maxHits) hits.push(addr.toString());
          },
          onComplete: function () {}
        });
      } catch (e) {}
    }
    return {pattern: pattern, scanned: scanned, hits: hits};
  },
  readFields: function (addrHex) {
    var p = ptr(addrHex);
    function rd(o, kind) {
      try { return kind === "p" ? p.add(o).readPointer().toString() : p.add(o).readU64().toString(); }
      catch (e) { return "ERR"; }
    }
    function rd32(o) { try { return p.add(o).readS32(); } catch(e){ return "ERR"; } }
    return { addr: addrHex, k150: rd32(0x150), f190: rd(0x190,"u"), f1a0: rd(0x1a0,"u"),
             f120: rd(0x120,"p"), f20: rd32(0x20), f188: rd32(0x188), f198: rd32(0x198),
             f240: (function(){ try { return p.add(0x240).readFloat().toFixed(3); } catch(e){ return "ERR"; } })() };
  }
};
"""
script = s.create_script(AGENT)
script.load()
r = script.exports_sync.find_class("Unit")
print("typesCount", r["count"], "hits", json.dumps(r["res"])[:500])
if r["res"]:
    klass = r["res"][0]["klass"]
    sc = script.exports_sync.scan_instances(klass, 40)
    print("scan pattern", sc["pattern"], "scanned(MB)", round(sc["scanned"]/1048576,1), "hits", len(sc["hits"]))
    for h in sc["hits"][:20]:
        print("  ", json.dumps(script.exports_sync.read_fields(h)))
s.detach()
