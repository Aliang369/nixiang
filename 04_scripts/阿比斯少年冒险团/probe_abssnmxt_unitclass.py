
import frida, sys, time, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
print("attached pid", PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
function cstr(p) { try { return p.readCString(); } catch (e) { return null; } }
rpc.exports = {
  info: function () {
    var m = Process.findModuleByName("libil2cpp.so");
    return { arch: Process.arch, mod: m ? m.base.toString() : null, size: m ? m.size : 0, nmods: Process.enumerateModules().length };
  },
  findUnitClass: function (metaRegRva, typesOff, typesPtrOff, nameOff) {
    var mr = BASE.add(metaRegRva);
    var count = mr.add(typesOff).readS32();
    var arr = mr.add(typesPtrOff).readPointer();
    var hits = [];
    for (var i = 0; i < count; i++) {
      var klass = arr.add(i * 8).readPointer();
      if (klass.isNull()) continue;
      var np = klass.add(nameOff).readPointer();
      if (np.isNull()) continue;
      var nm = cstr(np);
      if (nm === "Unit") hits.push({ i: i, klass: klass.toString() });
      if (hits.length >= 4) break;
    }
    return { count: count, hits: hits };
  }
};
"""
script = s.create_script(AGENT)
script.load()
print("info:", json.dumps(script.exports_sync.info()))
for (mr, toff, poff, noff) in [(0x7ff3ea8, 0x18, 0x20, 0x10), (0x7ff3ea8, 0x18, 0x20, 0x18)]:
    try:
        r = script.exports_sync.find_unit_class(mr, toff, poff, noff)
        print("namescan metaReg=0x%X nameOff=0x%X ->" % (mr, noff), json.dumps(r)[:400])
    except Exception as e:
        print("ERR", e)
s.detach()
