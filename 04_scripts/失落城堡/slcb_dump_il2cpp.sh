#!/usr/bin/env bash
# 失落城堡：对「内存解析版」libil2cpp.so 跑 Il2CppDumper。
#
# 为什么不用磁盘原包里的 libil2cpp.so：
#   易盾（NIS）在装载时把 [0, 0x3993000) 整段解密，实测内存与磁盘
#   差异 51,583,232 B（69.9%）⇒ 磁盘件是密文，静态 dump 拿不到东西。
#   内存解析版由 slcb_dump_il2cpp_mem.sh 拼接，sha/md5 见 summary.txt。
#
# 用法：bash 04_scripts/失落城堡/slcb_dump_il2cpp.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SO="$ROOT/09_test/失落城堡/mem/libil2cpp_memresolved.so"
META="$ROOT/09_test/失落城堡/mem/global-metadata.dat"  # 运行时解密版（易盾原地解密，见 slcb_dump_metadata_mem.sh）
OUT="$ROOT/06_dumped/失落城堡/memresolved"
DUMPER_DIR="$ROOT/01_tools/Il2CppDumper"
DUMPER_OUT="$OUT/il2cpp_dump"
TOOL_DIR="$OUT.tool"

[[ -f "$SO"   ]] || { echo "错误: 内存解析版 so 不存在: $SO（先跑 slcb_dump_il2cpp_mem.sh）" >&2; exit 1; }
[[ -f "$META" ]] || { echo "错误: global-metadata.dat 不存在: $META" >&2; exit 1; }

mkdir -p "$OUT" "$DUMPER_OUT"
rm -rf "$TOOL_DIR"; mkdir -p "$TOOL_DIR"
cp "$DUMPER_DIR/Il2CppDumper.dll" "$TOOL_DIR/"
cp "$DUMPER_DIR/Il2CppDumper.deps.json" "$TOOL_DIR/"
cp "$DUMPER_DIR/Il2CppDumper.runtimeconfig.json" "$TOOL_DIR/"
cp "$DUMPER_DIR"/Mono.Cecil*.dll "$TOOL_DIR/"
cat > "$TOOL_DIR/config.json" <<'JSON'
{
  "DumpMethod": true,
  "DumpField": true,
  "DumpProperty": true,
  "DumpAttribute": false,
  "DumpFieldOffset": true,
  "DumpMethodOffset": true,
  "DumpTypeDefIndex": true,
  "GenerateDummyDll": true,
  "GenerateStruct": true,
  "DummyDllAddToken": true,
  "RequireAnyKey": false,
  "ForceIl2CppVersion": false,
  "ForceVersion": 16,
  "ForceDump": true,
  "NoRedirectedPointer": false
}
JSON

if [[ ! -s "$DUMPER_OUT/dump.cs" ]]; then
  dotnet "$TOOL_DIR/Il2CppDumper.dll" "$SO" "$META" "$DUMPER_OUT" 2>&1 | tail -40
fi

: > "$OUT/summary.txt"
printf 'so_md5=%s\n'      "$(md5 -q "$SO")"                              >> "$OUT/summary.txt"
printf 'so_sha256=%s\n'   "$(shasum -a 256 "$SO" | awk '{print $1}')"   >> "$OUT/summary.txt"
printf 'meta_sha256=%s\n' "$(shasum -a 256 "$META" | awk '{print $1}')" >> "$OUT/summary.txt"
for f in dump.cs script.json stringliteral.json il2cpp.h; do
  [[ -s "$DUMPER_OUT/$f" ]] && printf '%s_lines=%s\n' "${f%.*}" "$(wc -l < "$DUMPER_OUT/$f" | tr -d ' ')" >> "$OUT/summary.txt"
done
cat "$OUT/summary.txt"
