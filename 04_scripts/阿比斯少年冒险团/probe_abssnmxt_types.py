
import frida, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
var BASE = ptr("0x400022c00000");
var MR = BASE.add(0x7ff3ea8);
function cstr(p){ try { var s=p.readCString(); if(!s) return null; for (var i=0;i<s.length;i++){ var c=s.charCodeAt(i); if (c<32||c>126) return null; } return s; } catch(e){ return null; } }
rpc.exports = {
  derefTypes: function (n) {
    var cnt = MR.add(0x30).readS32();
    var arr = MR.add(0x38).readPointer();
    var out = [];
    for (var i = 0; i < n; i++) {
      var k = arr.add(i*8).readPointer();
      var rec = {i:i, entry:k.toString()};
      try { rec.e0 = k.readPointer().toString(); } catch(e) { rec.e0 = "ERR"; }
      for (var off = 0; off < 0x20; off += 8) {
        try {
          var nm = cstr(k.add(off).readPointer());
          if (nm && nm.length) rec["s"+off.toString(16)] = nm;
        } catch(e) {}
      }
      // try entry->data->klass
      try {
        var d = k.readPointer();
        var nm2 = cstr(d.add(0x10).readPointer());
        if (nm2) rec["data_name10"] = nm2;
      } catch(e) {}
      out.push(rec);
    }
    return {cnt: cnt, out: out};
  }
};
"""
script = s.create_script(AGENT)
script.load()
r = script.exports_sync.deref_types(6)
print("cnt", r["cnt"])
for it in r["out"]:
    print(json.dumps(it, ensure_ascii=False)[:300])
s.detach()
