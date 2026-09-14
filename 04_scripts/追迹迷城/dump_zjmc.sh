#!/usr/bin/env bash
# 追迹迷城：拆包并运行 Il2CppDumper，产出可复现的静态分析输入。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
GAME="追迹迷城"
DUMPER="$NIXIANG_ROOT/01_tools/Il2CppDumper/Il2CppDumper.dll"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }

while IFS=$'\t' read -r key value; do
  case "$key" in
    apk_path) APK="$value" ;;
    version_name) VERSION="$value" ;;
  esac
done < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
for k in ("apk_path", "version_name"):
    print(f"{k}\t{d[k]}")
PY
)

[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }
command -v jadx >/dev/null || { echo "错误: jadx 不可用" >&2; exit 1; }
command -v apktool >/dev/null || { echo "错误: apktool 不可用" >&2; exit 1; }
command -v dotnet >/dev/null || { echo "错误: dotnet 不可用" >&2; exit 1; }
[[ -f "$DUMPER" ]] || { echo "错误: Il2CppDumper.dll 不存在" >&2; exit 1; }

echo "==> [1/3] jadx + apktool"
bash "$NIXIANG_ROOT/04_scripts/_common/decode_apk.sh" "$GAME" "$APK" "$VERSION"

DEC_ROOT="$NIXIANG_ROOT/03_decompiled/$GAME"
APKTOOL_DIR="$DEC_ROOT/apktool_$VERSION"
SO="$APKTOOL_DIR/lib/arm64-v8a/libil2cpp.so"
META="$APKTOOL_DIR/assets/bin/Data/Managed/Metadata/global-metadata.dat"
[[ -f "$SO" ]] || { echo "错误: 缺少 $SO" >&2; exit 1; }
[[ -f "$META" ]] || { echo "错误: 缺少 $META" >&2; exit 1; }

DUMP_ROOT="$NIXIANG_ROOT/06_dumped/$GAME"
OUT="$DUMP_ROOT/${VERSION}_il2cpp"
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

echo "==> [2/3] Il2CppDumper"
dotnet "$TOOL_DIR/Il2CppDumper.dll" "$SO" "$META" "$OUT"

[[ -s "$OUT/dump.cs" ]] || { echo "错误: Il2CppDumper 未产出 dump.cs" >&2; exit 1; }
[[ -s "$OUT/script.json" ]] || { echo "错误: Il2CppDumper 未产出 script.json" >&2; exit 1; }

echo "==> [3/3] 固化 current 指针与输入指纹"
ln -sfn "$APKTOOL_DIR" "$DEC_ROOT/current"
ln -sfn "$OUT" "$DUMP_ROOT/current"
mkdir -p "$DUMP_ROOT/inputs"
cp -f "$SO" "$DUMP_ROOT/inputs/libil2cpp.so"
cp -f "$META" "$DUMP_ROOT/inputs/global-metadata.dat"
{
  echo "apk=$APK"
  echo "apk_md5=$(md5 -q "$APK")"
  echo "libil2cpp_md5=$(md5 -q "$SO")"
  echo "libil2cpp_sha256=$(shasum -a 256 "$SO" | awk '{print $1}')"
  echo "global_metadata_md5=$(md5 -q "$META")"
  echo "global_metadata_sha256=$(shasum -a 256 "$META" | awk '{print $1}')"
  echo "dump_cs_sha256=$(shasum -a 256 "$OUT/dump.cs" | awk '{print $1}')"
  echo "script_json_sha256=$(shasum -a 256 "$OUT/script.json" | awk '{print $1}')"
} > "$OUT/inputs.sha256"

wc -l "$OUT/dump.cs"
ls -lh "$OUT/script.json" "$OUT/dump.cs"
echo "完成: $OUT"
