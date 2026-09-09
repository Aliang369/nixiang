#!/usr/bin/env python3.11
"""Compare dumped libil2cpp.so r-xp memory vs APK lib/arm64-v8a/libil2cpp.so.

Reports changed prologues / ARM64 B/BL stubs typical of Dobby inline hooks.
"""
from __future__ import annotations

import argparse
import struct
import sys
import zipfile
from pathlib import Path


def load_elf_pt_loads(so: bytes):
    if so[:4] != b"\x7fELF":
        raise ValueError("not ELF")
    e_phoff = struct.unpack_from("<Q", so, 32)[0]
    e_phentsize = struct.unpack_from("<H", so, 54)[0]
    e_phnum = struct.unpack_from("<H", so, 56)[0]
    loads = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        if struct.unpack_from("<I", so, off)[0] != 1:  # PT_LOAD
            continue
        p_offset, p_vaddr = struct.unpack_from("<QQ", so, off + 8)
        p_filesz = struct.unpack_from("<Q", so, off + 32)[0]
        p_memsz = struct.unpack_from("<Q", so, off + 40)[0]
        p_flags = struct.unpack_from("<I", so, off + 4)[0]
        loads.append((p_offset, p_vaddr, p_filesz, p_memsz, p_flags))
    return loads


def is_b_or_bl(w: int) -> bool:
    opc = (w >> 26) & 0x3F
    return opc in (0x05, 0x25)  # B / BL


def decode_b(w: int) -> int:
    imm = w & 0x3FFFFFF
    if imm & (1 << 25):
        imm -= 1 << 26
    return imm * 4


def read_apk_il2cpp(apk: Path) -> bytes:
    with zipfile.ZipFile(apk) as z:
        return z.read("lib/arm64-v8a/libil2cpp.so")


def build_mem_image(outdir: Path, so: bytes) -> tuple[bytearray, list[tuple[int, int, int]]]:
    """Build a file-offset-aligned image of r-xp from il2cpp_rx_*.bin + segs txt or maps."""
    loads = load_elf_pt_loads(so)
    # executable PT_LOAD (PF_X = 1)
    exec_loads = [L for L in loads if L[4] & 1]
    if not exec_loads:
        exec_loads = loads[:1]

    segs_path = outdir / "il2cpp_rx_segs.txt"
    maps_path = outdir / "maps.txt"
    segs: list[tuple[int, int, int]] = []  # start,end,file_off
    if segs_path.exists():
        for line in segs_path.read_text().splitlines():
            parts = line.split()
            if len(parts) >= 3:
                segs.append((int(parts[0], 16), int(parts[1], 16), int(parts[2], 16)))
    elif maps_path.exists():
        import re
        for line in maps_path.read_text().splitlines():
            if "libil2cpp.so" not in line or "r-xp" not in line:
                continue
            m = re.match(r"([0-9a-f]+)-([0-9a-f]+)\s+r-xp\s+([0-9a-f]+)\s+", line)
            if m:
                segs.append((int(m.group(1), 16), int(m.group(2), 16), int(m.group(3), 16)))

    bins = sorted(outdir.glob("il2cpp_rx_*.bin"))
    if not bins and (outdir / "raw").is_dir():
        bins = sorted((outdir / "raw").glob("il2cpp_rx_*.bin"))
    if not bins:
        raise FileNotFoundError("no il2cpp_rx_*.bin in outdir")

    # Map each dumped segment onto file offsets using maps file_off
    mem = bytearray(so)  # start as file copy; overlay dumped RX
    overlay_count = 0
    if segs and len(segs) == len(bins):
        pairs = list(zip(bins, segs))
    else:
        # fallback: assume single contiguous RX dump at first exec load file offset
        pairs = []
        off0 = exec_loads[0][0]
        for i, b in enumerate(bins):
            data = b.read_bytes()
            pairs.append((b, (0, len(data), off0)))
            off0 += len(data)

    for b, (start, end, foff) in pairs:
        data = b.read_bytes()
        n = min(len(data), len(mem) - foff) if foff < len(mem) else 0
        if n <= 0:
            print(f"WARN skip {b.name}: foff=0x{foff:x} out of range", file=sys.stderr)
            continue
        mem[foff : foff + n] = data[:n]
        overlay_count += n
        print(f"overlay {b.name}: file_off=0x{foff:x} n={n} va_range=0x{start:x}-0x{end:x}")

    print(f"overlay_total_bytes={overlay_count}")
    return mem, segs


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("outdir", help="memdiff OUTDIR with il2cpp_rx_*.bin + maps.txt")
    ap.add_argument("--apk", required=True, help="APK containing lib/arm64-v8a/libil2cpp.so")
    ap.add_argument("--max-report", type=int, default=200)
    args = ap.parse_args()
    outdir = Path(args.outdir)
    apk = Path(args.apk)

    so = read_apk_il2cpp(apk)
    mem, segs = build_mem_image(outdir, so)
    loads = load_elf_pt_loads(so)

    def file_to_va(foff: int):
        for o, v, fsz, msz, fl in loads:
            if o <= foff < o + fsz:
                return v + (foff - o)
        return None

    # Compare in 4-byte steps; cluster contiguous diffs
    patches = []
    i = 0
    n = min(len(so), len(mem))
    while i + 4 <= n:
        if so[i : i + 4] != mem[i : i + 4]:
            j = i
            while j + 4 <= n and so[j : j + 4] != mem[j : j + 4]:
                j += 4
                if j - i >= 64:  # cap single patch window
                    break
            patches.append((i, j))
            i = j
        else:
            i += 4

    print(f"\npatch_regions={len(patches)}")
    b_stubs = 0
    lines = []
    for idx, (a, b) in enumerate(patches[: args.max_report]):
        va = file_to_va(a)
        w_file = struct.unpack_from("<I", so, a)[0]
        w_mem = struct.unpack_from("<I", mem, a)[0]
        kind = []
        if is_b_or_bl(w_mem):
            b_stubs += 1
            kind.append("B/BL" if ((w_mem >> 26) & 0x3F) == 0x05 else "BL")
            kind.append(f"imm={decode_b(w_mem):+#x}")
        # LDR/BR patterns etc.
        if (w_mem & 0xFFE0001F) == 0xD61F0000:
            kind.append("BR")
        detail = " ".join(kind) if kind else "bytes"
        hex_f = so[a:a+min(16, b-a)].hex()
        hex_m = mem[a:a+min(16, b-a)].hex()
        line = (
            f"[{idx}] file_off=0x{a:x} va={hex(va) if va is not None else '?'} "
            f"len={b-a} {detail} file={hex_f} mem={hex_m}"
        )
        print(line)
        lines.append(line)

    summary = (
        f"file_size={len(so)} patch_regions={len(patches)} "
        f"b_or_bl_prologues≈{b_stubs} reported={min(len(patches), args.max_report)}"
    )
    print("\n" + summary)
    report = outdir / "il2cpp_memdiff_report.txt"
    report.write_text(summary + "\n" + "\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {report}")
    # machine-readable count
    (outdir / "il2cpp_patch_count.txt").write_text(str(len(patches)) + "\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
