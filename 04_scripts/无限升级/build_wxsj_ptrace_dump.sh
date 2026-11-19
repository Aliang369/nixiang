#!/usr/bin/env bash
# 构建 wxsj_ptrace_dump（arm64 静态）—— 2026-11-19
# 为什么需要本脚本：NDK27 `-static` 产物的 PT_TLS p_align=8，bionic 会拒绝执行
#   （"executable's TLS segment is underaligned: alignment is 8, needs to be at least 64"）。
#   加 64 对齐的 TLS 变量顶不上去（MemSiz 仍 8），故构建后把 PT_TLS 的 p_align 改成 64。
# 用法: bash 04_scripts/无限升级/build_wxsj_ptrace_dump.sh [输出路径]
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SRC="$ROOT/04_scripts/无限升级/wxsj_ptrace_dump.c"
OUT="${1:-$ROOT/08_output/无限升级/_diag/wxsj_ptrace_dump}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
CC="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android24-clang"
RE="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-readelf"
[[ -x "$CC" ]] || { echo "ERROR: clang missing: $CC"; exit 1; }
mkdir -p "$(dirname "$OUT")"
echo "[1/3] compile -static -O2 -s"
"$CC" -static -O2 -s -Wall -Wextra -o "$OUT" "$SRC"
echo "[2/3] patch PT_TLS p_align -> 64（bionic 静态可执行文件的对齐要求）"
python3 - "$OUT" <<'PY'
import struct, sys
p = sys.argv[1]
d = bytearray(open(p, "rb").read())
assert d[:4] == b"\x7fELF" and d[4] == 2, "not ELF64"
phoff = struct.unpack_from("<Q", d, 0x20)[0]
phentsize = struct.unpack_from("<H", d, 0x36)[0]
phnum = struct.unpack_from("<H", d, 0x38)[0]
n = 0
for i in range(phnum):
    o = phoff + i * phentsize
    ptype = struct.unpack_from("<I", d, o)[0]
    if ptype == 7:  # PT_TLS
        align = struct.unpack_from("<Q", d, o + 8 + 0x28)[0]
        if align < 64:
            struct.pack_into("<Q", d, o + 8 + 0x28, 64)
            n += 1
print("PT_TLS p_align patched:", n)
open(p, "wb").write(d)
PY
echo "[3/3] verify"
"$RE" -l "$OUT" | grep TLS || echo "WARN: 无 PT_TLS 段（本程序 8 字节 errno TLS，正常应有）"
file "$OUT"
ls -l "$OUT"
shasum -a 256 "$OUT" | awk '{print "sha256", $1}'
echo "md5 $(md5 -q "$OUT" 2>/dev/null || md5sum "$OUT" | cut -d' ' -f1)"
echo "done: $OUT"
