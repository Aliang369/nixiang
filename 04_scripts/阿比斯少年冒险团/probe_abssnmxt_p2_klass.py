
import frida, json, time
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
var MR = BASE.add(0x7ff3ea8);
function cstr(p){ try { var v=p.readCString(); if(!v||v.length>80) return null; for (var i=0;i<v.length;i++){var c=v.charCodeAt(i); if(c<32||c>126) return null;} return v; } catch(e){ return null; } }
rpc.exports = {
  findByTypes: function (want) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var out = [];
    var scanned = 0;
    for (var i = 0; i < cnt; i++) {
      var tp = arr.add(i*16);                  // Il2CppType, 16 bytes
      var data;
      try { data = tp.readPointer(); } catch(e) { continue; }
      if (data.isNull()) continue;
      var meta;
      try { meta = tp.add(8).readU32(); } catch(e) { continue; }
      var typ = (meta >> 16) & 0xff;           // attrs:16, type:8
      if (typ !== 0x11 && typ !== 0x12) continue;
      scanned++;
      var nm = cstr(data.add(0x10).readPointer());
      if (nm === want) {
        var ns = cstr(data.add(0x18).readPointer());
        out.push({i:i, klass:data.toString(), ns:ns, typ:typ});
        if (out.length >= 5) break;
      }
    }
    return {typesCount: cnt, classLike: scanned, hits: out};
  }
};
"""
script = s.create_script(AGENT)
script.load()
t0 = time.time()
r = script.exports_sync.find_by_types("Unit")
print("elapsed", round(time.time()-t0,1), "s")
print(json.dumps(r)[:800])
s.detach()
