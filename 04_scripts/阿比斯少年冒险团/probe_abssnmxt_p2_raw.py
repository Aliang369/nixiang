
import frida, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
var MR = BASE.add(0x7ff3ea8);
rpc.exports = {
  raw: function (n) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var out = [];
    for (var i = 0; i < n; i++) {
      var tp = arr.add(i*16);
      var bytes = [];
      for (var k = 0; k < 16; k++) { try { bytes.push(tp.add(k).readU8()); } catch(e) { bytes.push(-1); } }
      out.push({i:i, hex: bytes.map(function(b){return ("0"+b.toString(16)).slice(-2);}).join(" ")});
    }
    return {cnt: cnt, arr: arr.toString(), items: out};
  }
};
"""
script = s.create_script(AGENT)
script.load()
r = script.exports_sync.raw(6)
print("cnt", r["cnt"], "arr", r["arr"])
for it in r["items"]: print("  i=%d %s" % (it["i"], it["hex"]))
s.detach()
