import sys
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM

SO = "/Users/aliang/Documents/nixiang/06_dumped/旷野征途/2.2/libil2cpp.so"
TEXT_OFF = 0x1748604; TEXT_VA = 0x174c604
OUT = "/Users/aliang/Documents/nixiang/06_dumped/旷野征途/2.2/ida_probe/round2"

CANDS = [
    ("0x1af5b3c", 0x1af5b3c, 0x1af6004),
    ("0x1af5338", 0x1af5338, 0x1af5540),
    ("0x1af5540", 0x1af5540, 0x1af5868),
]

def main():
    so = open(SO, "rb").read()
    md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
    for name, start, end in CANDS:
        off = TEXT_OFF + (start - TEXT_VA)
        code = so[off:off + (end - start)]
        lines = []
        bls = []
        stps = []
        for ins in md.disasm(code, start):
            lines.append("0x%x  %-8s %s" % (ins.address, ins.mnemonic, ins.op_str))
            if ins.mnemonic == "bl":
                bls.append(ins.op_str)
            if ins.mnemonic in ("stp", "str") and "[x0" in ins.op_str:
                stps.append("0x%x %s %s" % (ins.address, ins.mnemonic, ins.op_str))
        path = OUT + "/" + name + ".asm"
        with open(path, "w") as fp:
            fp.write("\n".join(lines) + "\n")
        print("== %s size=0x%x lines=%d" % (name, end - start, len(lines)))
        print("   bl targets:", bls)
        print("   [x0*]-stores:", stps[:8])

main()
