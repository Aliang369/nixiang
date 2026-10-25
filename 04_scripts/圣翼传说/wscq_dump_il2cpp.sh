#!/usr/bin/env bash
# 圣翼传说新版本：提取 libil2cpp/metadata 并生成 Il2CppDumper 索引。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
BASE="${WSCQ_BASE_APK:-$NIXIANG/09_test/圣翼传说/base_20261026/base.apk}"
VERSION="${WSCQ_VERSION:-7.6.06}"
OUT_ROOT="${WSCQ_IL2CPP_OUT:-$NIXIANG/06_dumped/圣翼传说/il2cppdump_${VERSION}}"
INPUT="$OUT_ROOT/input"
OUT="$OUT_ROOT/out"
SO="$INPUT/libil2cpp.so"
META="$INPUT/global-metadata.dat"
DUMPER="$NIXIANG/01_tools/Il2CppDumper/Il2CppDumper.dll"
EXTRACT="$NIXIANG/04_scripts/_common/extract_il2cpp_preambles.py"

die() { echo "ERROR: $*" >&2; exit 1; }
[[ -f "$BASE" ]] || die "缺少底包: $BASE"
[[ -f "$DUMPER" ]] || die "缺少 Il2CppDumper: $DUMPER"
[[ -f "$EXTRACT" ]] || die "缺少索引工具: $EXTRACT"
command -v dotnet >/dev/null 2>&1 || die "dotnet 不可用"

rm -rf "$OUT_ROOT"
mkdir -p "$INPUT" "$OUT"
unzip -p "$BASE" lib/arm64-v8a/libil2cpp.so > "$SO"
unzip -p "$BASE" assets/bin/Data/Managed/Metadata/global-metadata.dat > "$META"
[[ -s "$SO" && -s "$META" ]] || die "提取 so/metadata 失败"
echo "[dump] so     $(stat -f %z "$SO") B md5=$(md5 -q "$SO")"
echo "[dump] meta   $(stat -f %z "$META") B md5=$(md5 -q "$META")"

dotnet "$DUMPER" "$SO" "$META" "$OUT"
[[ -f "$OUT/dump.cs" ]] || die "Il2CppDumper 未产出 dump.cs"

python3 "$EXTRACT" --dump "$OUT/dump.cs" --index-out "$OUT"
python3 "$EXTRACT" --index-verify "$OUT/methods.txt" > "$OUT/index_verify.txt"

{
  echo "version=$VERSION"
  echo "libil2cpp_md5=$(md5 -q "$SO")"
  echo "global_metadata_md5=$(md5 -q "$META")"
  echo "dump_cs=$OUT/dump.cs"
  echo "methods_txt=$OUT/methods.txt"
  echo "classes_txt=$OUT/classes.txt"
} | tee "$OUT/dump_info.txt"

echo "[dump] done → $OUT"
