const lib = Process.findModuleByName("libsecret.so");
if (!lib) {
    console.log("libsecret not found");
} else {
    console.log("libsecret base=" + lib.base + " size=" + lib.size);
    const base = lib.base;
    const offsets = [0x70310, 0x70318, 0x70328, 0x7032c, 0x70358, 0x7035c, 0x70360, 0x667b4];
    for (const off of offsets) {
        try {
            console.log("u32[" + off.toString(16) + "]=" + base.add(off).readU32().toString(16));
        } catch (e) {
            console.log("read " + off.toString(16) + " failed: " + e);
        }
    }
    try {
        console.log(hexdump(base.add(0x70000), { offset: 0, length: 0x900, header: true, ansi: false }));
    } catch (e) {
        console.log("dump failed: " + e);
    }
}
