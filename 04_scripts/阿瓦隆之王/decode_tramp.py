#!/usr/bin/env python3
"""Decode AVALON king hinge fixed trampolines (adrp+add+br x17) from memory dump bytes.

Usage: python3 decode_tramp.py <mem_hex_bytes...>
Each arg is a hex string of >=12 bytes read from the patched prologue.
"""
import re
import sys


def u32(b: bytes) -> int:
    return int.from_bytes(b, "little")


def decode(hexstr: str, label: str):
    b = bytes.fromhex(hexstr)
    if len(b) < 12:
        print(f"{label}: too short")
        return
    w0 = u32(b[0:4])
    w1 = u32(b[4:8])
    w2 = u32(b[8:12])
    # adrp x17 : bits [31:24]=0xD0? -> op=1, immlo in [30:29], immhi [23:5]
    # ADRP encoding: sf=1, op=1(0110000) ... check manually via masks
    def decode_adrp(w, rd):
        # ADRP: 1 0 0 1 1 0 0 0 | immhi[20:2] (19 bits) | 0 0 | rd(5)  -- for 64-bit
        # actual layout: bits31-24 = 10010000 (0x90) for ADRP? No padding.
        # ADRP = (0x90000000 | ((immhi&0x7ffff)<<5) | rd); plus immlo in bits30-29? No: ADRP uses two fields
        # Standard: ADRP Xd, label: 1 [30:29 immlo] [0 0 1 1 1] 1 0 x [immhi 19b] [00] [rd]
        if (w >> 24) & 0xFF != 0x90:
            return None  # not ADRP (0x90 = 10010000)
        immlo = (w >> 29) & 0x3
        immhi = (w >> 5) & 0x7FFFF
        imm = (immhi << 2) | immlo
        if imm & (1 << 20):
            imm -= 1 << 21
        return imm * 4096
    # Use precise decode:
    def adrp_target(w):
        # ADRP: bits[31:28]=1001, bit[27:24]=0000? -> check (w>>24)&0xF0 == 0x90
        if ((w >> 24) & 0xF0) != 0x90:
            return None
        immlo = (w >> 29) & 0x3
        immhi = (w >> 5) & 0x7FFFF
        imm = (immhi << 2) | immlo
        return imm  # pages, sign-extended 21 bits
    def add_imm12(w):
        # ADD xd, xn, #imm : 1 0 0 1 0 0 0 1 | shift(2) | imm12 | rn | rd ; xd=xn=x17 -> rn bits[9:5], rd bits[4:0]
        if ((w >> 24) & 0xFF) != 0x91:
            return None
        imm12 = (w >> 10) & 0xFFF
        sh = (w >> 22) & 0x3
        return imm12 << (12 * sh)

    # decode w0 as ADRP x17 (rd=17)
    tgt_page = None
    if adrp_target(w0) is not None and (w0 & 0x1F) == 17:
        pg = adrp_target(w0)
        base = None
        if (w1 >> 24) == 0x91 and (w1 & 0x1F) == 17 and ((w1 >> 5) & 0x1F) == 17:
            off = add_imm12(w1)
            tgt_page = (pg << 12) + off
    br = None
    if (w2 & 0xFFFFFC1F) == 0xD61F0000 and ((w2 >> 5) & 0x1F) == 17:
        br = "br x17"
    print(f"{label}: target_page_imm={pg:#x if pg is not None else None} "
          f"target_abs={tgt_page:#x if tgt_page is not None else None} tail={br}")
    return tgt_page


if __name__ == "__main__":
    for i, h in enumerate(sys.argv[1:]):
        decode(h, f"patch{i}")