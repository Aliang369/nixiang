#!/usr/bin/env bash
# 大话蜀山 2.5 — Il2CppDumper 静态符号 dump。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=../_common/env.sh
source "$SCRIPT_DIR/../_common/env.sh"

GAME="大话蜀山"
VERSION="2.5"
APK="${1:-$NIXIANG_ROOT/02_apks/$GAME/${GAME}_${VERSION}_binge_from_device.apk}"
DEC="$NIXIANG_ROOT/03_decompiled/$GAME/apktool_$VERSION"
OUT="$NIXIANG_ROOT/06_dumped/$GAME/il2cpp_dump_$VERSION"
DUMPER="$NIXIANG_ROOT/01_tools/Il2CppDumper/Il2CppDumper.dll"
SO="$DEC/lib/arm64-v8a/libil2cpp.so"
META="$DEC/assets/bin/Data/Managed/Metadata/global-metadata.dat"

[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "错误: libil2cpp.so 不存在: $SO" >&2; exit 1; }
[[ -f "$META" ]] || { echo "错误: global-metadata.dat 不存在: $META" >&2; exit 1; }
[[ -f "$DUMPER" ]] || { echo "错误: Il2CppDumper.dll 不存在: $DUMPER" >&2; exit 1; }
command -v dotnet >/dev/null || { echo "错误: dotnet 不可用" >&2; exit 1; }

TOOL_DIR="$OUT.tool"
rm -rf "$OUT" "$TOOL_DIR"
mkdir -p "$OUT" "$TOOL_DIR"

cp "$DUMPER" "$TOOL_DIR/"
cp "$NIXIANG_ROOT/01_tools/Il2CppDumper/Il2CppDumper.deps.json" "$TOOL_DIR/"
cp "$NIXIANG_ROOT/01_tools/Il2CppDumper/Il2CppDumper.runtimeconfig.json" "$TOOL_DIR/"
cp "$NIXIANG_ROOT/01_tools/Il2CppDumper"/Mono.Cecil*.dll "$TOOL_DIR/"
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

echo "==> Il2CppDumper"
dotnet "$TOOL_DIR/Il2CppDumper.dll" "$SO" "$META" "$OUT"

[[ -s "$OUT/dump.cs" ]] || { echo "错误: Il2CppDumper 未产出 dump.cs" >&2; exit 1; }
[[ -s "$OUT/script.json" ]] || { echo "错误: Il2CppDumper 未产出 script.json" >&2; exit 1; }

{
  echo "apk=$APK"
  echo "apk_md5=$(md5 -q "$APK")"
  echo "libil2cpp_sha256=$(shasum -a 256 "$SO" | awk '{print $1}')"
  echo "metadata_sha256=$(shasum -a 256 "$META" | awk '{print $1}')"
  echo "dump_cs_sha256=$(shasum -a 256 "$OUT/dump.cs" | awk '{print $1}')"
  echo "script_json_sha256=$(shasum -a 256 "$OUT/script.json" | awk '{print $1}')"
} > "$OUT/inputs.sha256"

wc -l "$OUT/dump.cs"
ls -lh "$OUT/script.json" "$OUT/dump.cs"
echo "完成: $OUT"
