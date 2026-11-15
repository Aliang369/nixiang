
import frida, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
rpc.exports = {
  elf: function () { return BASE.readU32().toString(16); },
  qwords: function (rva, n) {
    var p = BASE.add(rva); var out = [];
    for (var o = 0; o < n; o += 8) {
      try { out.push({off: o, val: "0x" + p.add(o).readU64().toString(16)}); }
      catch (e) { out.push({off: o, err: String(e).slice(0, 40)}); }
    }
    return out;
  }
};
"""
script = s.create_script(AGENT)
script.load()
print("elf magic:", script.exports_sync.elf())
print("=== MetadataRegistration +0x0..0x60 (rva 0x7ff3ea8) ===")
for q in script.exports_sync.qwords(0x7ff3ea8, 0x60):
    print("  +0x%02X %s" % (q["off"], q.get("val", q.get("err"))))
print("=== CodeRegistration +0x0..0x40 (rva 0x7d4a5d8) ===")
for q in script.exports_sync.qwords(0x7d4a5d8, 0x40):
    print("  +0x%02X %s" % (q["off"], q.get("val", q.get("err"))))
s.detach()
