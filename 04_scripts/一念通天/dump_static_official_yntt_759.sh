#!/usr/bin/env bash
# 一念通天 759：提取当前原版内容并生成独立 IL2CPP dump。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
APK="$NIXIANG_ROOT/02_apks/一念通天/一念通天_759_official_from_device.apk"
OUT="$NIXIANG_ROOT/06_dumped/一念通天/official_759/static"
DUMPER_DIR="$NIXIANG_ROOT/01_tools/Il2CppDumper"
DUMPER_OUT="$OUT/il2cpp_dump"
TOOL_DIR="$OUT.tool"

[[ -f "$APK" ]] || { echo "错误: 原版包不存在: $APK" >&2; exit 1; }
[[ -f "$DUMPER_DIR/Il2CppDumper.dll" ]] || { echo "错误: Il2CppDumper 不存在" >&2; exit 1; }

mkdir -p "$OUT" "$DUMPER_OUT"
rm -rf "$TOOL_DIR"
mkdir -p "$TOOL_DIR"
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
  "ForceDump": false,
  "NoRedirectedPointer": false
}
JSON

unzip -p "$APK" 'lib/arm64-v8a/libil2cpp.so' > "$OUT/libil2cpp.so"
unzip -p "$APK" 'assets/bin/Data/Managed/Metadata/global-metadata.dat' > "$OUT/global-metadata.dat"
unzip -p "$APK" 'lib/arm64-v8a/libunity.so' > "$OUT/libunity.so"
unzip -p "$APK" 'lib/arm64-v8a/libmain.so' > "$OUT/libmain.so"
unzip -p "$APK" 'classes.dex' > "$OUT/classes.dex"
unzip -p "$APK" 'classes2.dex' > "$OUT/classes2.dex"

if [[ ! -s "$DUMPER_OUT/dump.cs" || ! -s "$DUMPER_OUT/script.json" ]]; then
  dotnet "$TOOL_DIR/Il2CppDumper.dll" "$OUT/libil2cpp.so" "$OUT/global-metadata.dat" "$DUMPER_OUT"
fi

[[ -s "$DUMPER_OUT/dump.cs" ]] || { echo "错误: dump.cs 未生成" >&2; exit 1; }
[[ -s "$DUMPER_OUT/script.json" ]] || { echo "错误: script.json 未生成" >&2; exit 1; }

FILE "$OUT/libil2cpp.so" | tee "$OUT/summary.txt"
printf 'libil2cpp_sha256=%s\n' "$(shasum -a 256 "$OUT/libil2cpp.so" | awk '{print $1}')" >> "$OUT/summary.txt"
printf 'metadata_sha256=%s\n' "$(shasum -a 256 "$OUT/global-metadata.dat" | awk '{print $1}')" >> "$OUT/summary.txt"
printf 'dump_cs_lines=%s\n' "$(wc -l < "$DUMPER_OUT/dump.cs" | tr -d ' ')" >> "$OUT/summary.txt"
printf 'dump_dir=%s\n' "$DUMPER_OUT" >> "$OUT/summary.txt"
cat "$OUT/summary.txt"
