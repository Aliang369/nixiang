#!/usr/bin/env bash
# 反编译指定类型（默认 DlgNetkuuLoginSystem）
set -uo pipefail
cd /e/nixiang/_cs104/tmp || exit 1
DLL=$(find ilspycmd_pkg -name 'ilspycmd.dll' | head -1)
TYPE="${1:-ET.DlgNetkuuLoginSystem}"
OUT="${2:-decomp_$(echo "$TYPE" | tr '.' '_').cs}"
"/c/Program Files/dotnet/dotnet.exe" "$DLL" -t "$TYPE" "/e/nixiang/_cs104/tmp/code/Code.dll" > "$OUT" 2>"$OUT.err"
echo "rc=$? out=$OUT lines=$(wc -l < "$OUT")"
head -5 "$OUT.err"
