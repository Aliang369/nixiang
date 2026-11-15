
import frida, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
var MR = BASE.add(0x7ff3ea8);
function cstr(p){ try { var v=p.readCString(); if(!v||v.length>80) return null; for (var i=0;i<v.length;i++){var c=v.charCodeAt(i); if(c<32||c>126) return null;} return v; } catch(e){ return null; } }
rpc.exports = {
  probeLayout: function (want) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var out = [];
    var zero = 0, nonzero = 0;
    for (var i = 0; i < cnt && out.length < 6; i++) {
      var tp = arr.add(i*8).readPointer();
      if (tp.isNull()) { zero++; continue; }
      nonzero++;
      var data; try { data = tp.readPointer(); } catch(e) { continue; }
      if (data.isNull()) continue;
      var meta; try { meta = tp.add(8).readU32(); } catch(e) { continue; }
      var typ = (meta >>> 16) & 0xff;
      if (typ !== 0x12) continue;
      var rec = {i:i, typ:typ, data:data.toString()};
      for (var off = 0; off <= 0x20; off += 8) { var v = cstr(data.add(off).readPointer()); if (v) rec["s0x"+off.toString(16)] = v; }
      out.push(rec);
    }
    return {cnt: cnt, zero: zero, nonzero: nonzero, items: out};
  }
};
"""
script = s.create_script(AGENT)
script.load()
r = script.exports_sync.probeLayout("Unit")
print(json.dumps(r, ensure_ascii=False)[:1200])
s.detach()
