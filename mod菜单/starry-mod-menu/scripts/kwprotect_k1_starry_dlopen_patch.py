#!/usr/bin/env python3
"""Patch libKwProtectSDK K1: deferred pthread dlopen(libstarry_magic.so) after JNI_OnLoad."""
from __future__ import annotations

import argparse
import struct
from pathlib import Path

JNI_ONLOAD_VA = 0x24e024
JNI_ONLOAD_CONT_VA = 0x24e034
DLOPEN_PLT_VA = 0x2e120
PTHREAD_CREATE_PLT_VA = 0x2e050
USLEEP_PLT_VA = 0x2dc50
STUB_FILE_OFF = 0x479880
STUB_VA = 0x479880
ORIG_BYTES = bytes.fromhex("ff0302d1fd7b06a9f44f07a9fd830191")
USLEEP_USEC = 8_000_000
LIB_PATH = b"libstarry_magic.so\x00"


def b_insn(pc_va: int, to_va: int) -> bytes:
    off = (to_va - pc_va) // 4
    return struct.pack("<I", 0x14000000 | (off & 0x03ffffff))


def bl_insn(pc_va: int, to_va: int) -> bytes:
    off = (to_va - pc_va) // 4
    return struct.pack("<I", 0x94000000 | (off & 0x03ffffff))


def adr(rd: int, pc_va: int, to_va: int) -> bytes:
    off = to_va - pc_va
    imm = off & ~3
    immlo = imm & 3
    immhi = (imm >> 2) & 0x7ffff
    return struct.pack("<I", 0x10000000 | (immlo << 29) | (immhi << 5) | rd)


def movz(rd: int, imm16: int = 0) -> bytes:
    return struct.pack("<I", 0xd2800000 | ((imm16 & 0xffff) << 5) | (rd & 0x1f))


def sub_sp(imm: int) -> bytes:
    return struct.pack("<I", 0xd1000000 | ((imm & 0xfff) << 10) | (31 << 5) | 31)


def add_sp(imm: int) -> bytes:
    return struct.pack("<I", 0x91000000 | ((imm & 0xfff) << 10) | (31 << 5) | 31)


def add_rd_sp(rd: int, imm: int = 0) -> bytes:
    return struct.pack("<I", 0x91000000 | ((imm & 0xfff) << 10) | (31 << 5) | (rd & 0x1f))


def mov_u32(rd: int, value: int) -> bytes:
    out = []
    first = True
    for shift in (0, 16, 32, 48):
        chunk = (value >> shift) & 0xffff
        if chunk == 0 and not first:
            continue
        if first:
            out.append(movz(rd, chunk))
            first = False
        else:
            out.append(struct.pack("<I", 0xf2a00000 | (chunk << 5) | rd | ((shift // 16) << 21)))
    return b"".join(out)


def build_worker(worker_va: int, lib_va: int) -> bytes:
    w = b""
    w += mov_u32(0, USLEEP_USEC)
    pc = worker_va + len(w)
    w += bl_insn(pc, USLEEP_PLT_VA)
    pc = worker_va + len(w)
    w += adr(0, pc, lib_va)
    w += struct.pack("<I", 0x52800041)
    pc = worker_va + len(w)
    w += bl_insn(pc, DLOPEN_PLT_VA)
    w += movz(0, 0)
    w += struct.pack("<I", 0xd65f03c0)
    return w


def build_spawn(spawn_va: int, worker_va: int) -> bytes:
    s = b""
    s += sub_sp(0x30)
    s += add_rd_sp(0, 0)
    s += movz(1, 0)
    pc = spawn_va + len(s)
    s += adr(2, pc, worker_va)
    s += movz(3, 0)
    pc = spawn_va + len(s)
    s += bl_insn(pc, PTHREAD_CREATE_PLT_VA)
    s += add_sp(0x30)
    s += struct.pack("<I", 0xd65f03c0)
    return s


def build_stub() -> bytes:
    resume_va = STUB_VA + 4
    cont_b_va = resume_va + len(ORIG_BYTES)
    spawn_va = cont_b_va + 4
    spawn_len = len(build_spawn(spawn_va, spawn_va + 256))
    worker_va = spawn_va + spawn_len
    lib_va = worker_va + 64
    worker = build_worker(worker_va, lib_va)
    lib_va = worker_va + len(worker)
    worker = build_worker(worker_va, lib_va)
    spawn = build_spawn(spawn_va, worker_va)
    head = bl_insn(STUB_VA, spawn_va)
    mid = ORIG_BYTES + b_insn(cont_b_va, JNI_ONLOAD_CONT_VA)
    return head + mid + spawn + worker + LIB_PATH


def patch(src: Path, dst: Path) -> None:
    data = bytearray(src.read_bytes())
    orig = data[JNI_ONLOAD_VA:JNI_ONLOAD_VA + len(ORIG_BYTES)]
    stub = build_stub()
    hook = b_insn(JNI_ONLOAD_VA, STUB_VA)
    end_stub = STUB_FILE_OFF + len(stub)
    if end_stub > len(data):
        raise SystemExit(f"stub overflows file: need {end_stub}, have {len(data)}")
    data[STUB_FILE_OFF:STUB_FILE_OFF + len(stub)] = stub
    data[JNI_ONLOAD_VA:JNI_ONLOAD_VA + len(hook)] = hook
    data[JNI_ONLOAD_VA + len(hook):JNI_ONLOAD_VA + len(ORIG_BYTES)] = orig[len(hook):]
    dst.write_bytes(data)
    print(f"patched {src} -> {dst} ({len(stub)} B stub, dlopen only)")


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("input", type=Path)
    ap.add_argument("output", type=Path, nargs="?", default=None)
    args = ap.parse_args()
    out = args.output or args.input.with_name(args.input.stem + "_dlopen.so")
    patch(args.input, out)


if __name__ == "__main__":
    main()
