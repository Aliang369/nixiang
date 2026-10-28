import subprocess, os
blob = open(r"E:\nixiang\06_dumped\绿梦_20261027\fg_inflated.bin","rb").read()
mc = r"C:\Users\Administrator\AppData\Local\Android\Sdk\ndk\27.0.12077973\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-mc.exe"
for va in (0x2783a8, 0x27de88, 0x35c408, 0x2a3d4):
    start = va - 0x50
    off = start - 0x458
    if off < 0: start = 0x458; off = 0
    data = blob[off:off + 0x50 + 0x10]
    # 对齐到 4 字节
    pad = (4 - (len(data) % 4)) % 4
    data += b"\x00" * pad
    hexs = " ".join("0x%02x" % b for b in data)
    p = subprocess.run([mc, "-triple=aarch64", "-disassemble"], input=hexs, capture_output=True, text=True)
    print("=== ret va 0x%x (first insn at va 0x%x) ===" % (va, start))
    lines = [l for l in p.stdout.splitlines() if l.strip()]
    for i, l in enumerate(lines):
        addr = start + i * 4
        mark = "  <== RET" if addr == va else ""
        print("  %#08x  %s%s" % (addr, l.strip(), mark))
    print()
