
import frida, json, time
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
var MR = BASE.add(0x7ff3ea8);
function cstr(p){ try { var v=p.readCString(); if(!v||v.length>80) return null; for (var i=0;i<v.length;i++){var c=v.charCodeAt(i); if(c<32||c>126) return null;} return v; } catch(e){ return null; } }
rpc.exports = {
  sample: function (n) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var out = [];
    for (var i = 0; i < n; i++) {
      var tp = arr.add(i*8).readPointer();
      var data = tp.readPointer();
      var meta = tp.add(8).readU32();
      var typ = (meta >>> 16) & 0xff;
      var nm = cstr(data.add(0x10).readPointer());
      out.push({i:i, tp: tp.toString(), data: data.toString(), typ: typ, name: nm});
    }
    return {cnt: cnt, items: out};
  },
  findUnit: function () {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var hits = [];
    for (var i = 0; i < cnt; i++) {
      var tp = arr.add(i*8).readPointer();
      if (tp.isNull()) continue;
      var data;
      try { data = tp.readPointer(); } catch(e) { continue; }
      if (data.isNull()) continue;
      var meta; try { meta = tp.add(8).readU32(); } catch(e) { continue; }
      var typ = (meta >>> 16) & 0xff;
      if (typ !== 0x11 && typ !== 0x12) continue;
      var nm = cstr(data.add(0x10).readPointer());
      if (nm === "Unit") {
        hits.push({i:i, klass:data.toString(), ns:cstr(data.add(0x18).readPointer())});
        if (hits.length >= 4) break;
      }
    }
    return {cnt: cnt, hits: hits};
  }
};
"""
script = s.create_script(AGENT)
script.load()
print("sample:", json.dumps(script.exports_sync.sample(4))[:500])
t0 = time.time()
r = script.exports_sync.find_unit()
print("findUnit elapsed", round(time.time()-t0,1), "s ->", json.dumps(r)[:500])
s.detach()
