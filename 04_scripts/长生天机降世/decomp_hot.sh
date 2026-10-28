#!/usr/bin/env bash
# 反编译运行时（热更）Code.dll 的指定类型
set -uo pipefail
cd /e/nixiang/_cs104/tmp || exit 1
DLL=$(find ilspycmd_pkg -name 'ilspycmd.dll' | head -1)
TYPE="${1:-ET.DlgNetkuuLoginSystem}"
ASM="${2:-/e/nixiang/_cs104/tmp/code_hot/Code.dll}"
OUT="${3:-decomp_hot_login.cs}"
"/c/Program Files/dotnet/dotnet.exe" "$DLL" -t "$TYPE" "$ASM" > "$OUT" 2>"$OUT.err"
echo "rc=$? out=$OUT lines=$(wc -l < "$OUT")"
head -3 "$OUT.err"
