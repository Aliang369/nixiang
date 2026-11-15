
import frida, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
var MR = BASE.add(0x7ff3ea8);
function cstr(p){ try { return p.readCString(); } catch(e){ return "<err>"; } }
rpc.exports = {
  probe: function (limit) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var out = [];
    for (var i = 0; i < limit; i++) {
      var k = arr.add(i*8).readPointer();
      var np = k.isNull() ? null : k.add(0x10).readPointer();
      out.push({i:i, k:k.toString(), np: np ? np.toString() : null, name: np ? cstr(np) : null});
    }
    return {cnt: cnt, arr: arr.toString(), items: out};
  },
  findSub: function (sub, limit) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var res = [];
    for (var i = 0; i < cnt; i++) {
      var k = arr.add(i*8).readPointer();
      if (k.isNull()) continue;
      var nm = cstr(k.add(0x10).readPointer());
      if (nm && nm.indexOf(sub) >= 0) { res.push({i:i, k:k.toString(), name:nm}); if (res.length >= limit) break; }
    }
    return res;
  }
};
"""
script = s.create_script(AGENT)
script.load()
p = script.exports_sync.probe(8)
print("typesCount", p["cnt"], "arr", p["arr"])
for it in p["items"]:
    print("  i=%d k=%s name=%s" % (it["i"], it["k"], it["name"]))
print("--- substring Unit ---")
print(json.dumps(script.exports_sync.find_sub("Unit", 12))[:600])
s.detach()
