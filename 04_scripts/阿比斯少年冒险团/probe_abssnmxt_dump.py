
import frida, json
PID = 18425
dev = frida.get_usb_device(timeout=10)
s = dev.attach(PID)
AGENT = r"""
rpc.exports = {
  dumpHex: function (addr, n) {
    var p = ptr(addr);
    var b = p.readByteArray(n);
    var u8 = new Uint8Array(b);
    var out = [];
    for (var i = 0; i < u8.length; i++) out.push(("0" + u8[i].toString(16)).slice(-2));
    var qwords = [];
    for (var o = 0; o + 8 <= n; o += 8) qwords.push({off: o, val: "0x" + p.add(o).readU64().toString(16)});
    return { hex: out.join(" "), qwords: qwords };
  }
};
"""
script = s.create_script(AGENT)
script.load()
for addr, n in [("0x4002a3f3ea8", 0x90), ("0x4000a30e5d8", 0x60)]:
    try:
        r = script.exports_sync.dump_hex(addr, n)
        print("=== %s ===" % addr)
        print("hex:", r["hex"][:400])
        for q in r["qwords"]:
            print("   +0x%02X %s" % (q["off"], q["val"]))
    except Exception as e:
        print("ERR", addr, e)
s.detach()
