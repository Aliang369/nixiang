
import frida, json, re
# 1) 本地 metadata 中找 "Unit\0" 的偏移（取 type name 表附近的那个）
data = open(r"E:\nixiang\06_dumped\阿比斯少年冒险团\global-metadata.dat", "rb").read()
offs = []
i = data.find(b"Unit\x00")
while i != -1 and len(offs) < 30:
    offs.append(i); i = data.find(b"Unit\x00", i + 1)
print("meta 'Unit\\0' offsets:", offs[:10])
META_BASE = 0x73f3b6fc7000
cands = [META_BASE + o for o in offs[:10]]
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
rpc.exports = {
  cstrAt: function (addrs) {
    var out = [];
    for (var i = 0; i < addrs.length; i++) {
      try { out.push({addr: addrs[i], s: ptr(addrs[i]).readCString()}); } catch (e) { out.push({addr: addrs[i], s: "ERR"}); }
    }
    return out;
  },
  scanPtrIn: function (base, size, val) {
    var q = ptr(val).toString(16); while (q.length < 16) q = "0" + q;
    var bytes = []; for (var i = 14; i >= 0; i -= 2) bytes.push(q.substr(i,2));
    var pat = bytes.join(" ");
    var hits = []; var err = null;
    try { Memory.scanSync(ptr(base), size, pat, { onMatch: function (a) { hits.push(a.toString()); }, onComplete: function () {} }); }
    catch (e) { err = String(e).slice(0, 100); }
    return {hits: hits.length, sample: hits.slice(0, 5), err: err};
  }
};
"""
script = s.create_script(AGENT)
script.load()
print("cstr check:", json.dumps(script.exports_sync.cstr_at([hex(c) for c in cands[:5]]))[:400])
# 在 host 大堆段试 scanSync
for base, size in [(0x73f390000000, 0x10000000), (0x73f2a0000000, 0x8400000)]:
    r = script.exports_sync.scan_ptr_in__(hex(base), size, hex(cands[0])) if False else script.exports_sync.scanPtrIn(hex(base), size, hex(cands[0]))
    print("scan %x..%x ->" % (base, base+size), json.dumps(r)[:200])
s.detach()
