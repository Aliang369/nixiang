
import frida, json, re
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
rpc.exports = {
  tryscan: function (base, size, pat) {
    var out = {hits: 0, err: null, read: null, ms: 0};
    var t0 = Date.now();
    try { out.read = ptr(base).readByteArray(8); } catch (e) { out.read = "readERR:" + e; }
    try {
      var n = 0;
      Memory.scanSync(ptr(base), size, pat, {
        onMatch: function (a) { n++; },
        onComplete: function () {}
      });
      out.hits = n;
    } catch (e) { out.err = String(e).slice(0, 120); }
    out.ms = Date.now() - t0;
    return out;
  },
  readstr: function (addr) {
    try { return ptr(addr).readCString(); } catch (e) { return "ERR:" + e; }
  }
};
"""
script = s.create_script(AGENT)
script.load()
base = 0x400022c00000
print("A) scan ELF magic in first mapping (size 0x2bc4000):")
print("  ", json.dumps(script.exports_sync.tryscan(base, 0x2bc4000, "7f 45 4c 46")))
print("B) scan 'Unit' in same:")
print("  ", json.dumps(script.exports_sync.tryscan(base, 0x2bc4000, "55 6e 69 74 00")))
print("C) scan 'Monobehaviour'/'MonoBehaviour' in same:")
print("  ", json.dumps(script.exports_sync.tryscan(base, 0x2bc4000, "4d 6f 6e 6f 42 65 68 61 76 69 6f 75 72")))
print("D) scan in second mapping (r-- offset 0x2bc4000):")
print("  ", json.dumps(script.exports_sync.tryscan(0x4000257c8000, 0x517ef000 & 0x1000000, "55 6e 69 74 00")))
print("E) read a string at base+0x3000000:", script.exports_sync.readstr(hex(base + 0x3000000)))
s.detach()
