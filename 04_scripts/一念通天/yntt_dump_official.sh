#!/usr/bin/env bash
# 一念通天 pipeline 阶段 dump_official —— 原版底包静态 dump（版本无关）。
#
# 输入：02_apks/一念通天/一念通天_<版本>_official_from_device.apk（pull_official 阶段产物）
# 输出：06_dumped/一念通天/official_<版本>/static/{libil2cpp.so,global-metadata.dat,...}
#       06_dumped/一念通天/official_<版本>/static/il2cpp_dump/{dump.cs,script.json,...}
#       06_dumped/一念通天/official_<版本>/static/summary.txt
#
# 版本号与底包 md5 全部取自 official_apk.json，脚本内不出现任何版本常量。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/yntt_common.sh"

OUT="${DUMP_DIR}/static"
DUMPER_SRC="${NIXIANG_ROOT}/01_tools/Il2CppDumper"
DUMPER_OUT="${OUT}/il2cpp_dump"
TOOL_DIR="${OUT}.tool"

yntt_need_file "${ORIG_APK}"
yntt_need_file "${DUMPER_SRC}/Il2CppDumper.dll"

ACTUAL_MD5="$(yntt_md5 "${ORIG_APK}")"
[ "${ACTUAL_MD5}" = "${ORIG_MD5}" ] || yntt_die "底包指纹不符：期望 ${ORIG_MD5}，实际 ${ACTUAL_MD5}（${ORIG_APK}）"

DOTNET="$(yntt_dotnet)" || yntt_die "找不到 dotnet（Il2CppDumper 运行依赖）；装好后重跑，或在 PATH 上提供 dotnet"
yntt_log "dotnet=${DOTNET}"
yntt_log "底包=${ORIG_APK}（${YNTT_VER}）"

mkdir -p "${OUT}" "${DUMPER_OUT}"
rm -rf "${TOOL_DIR}"
mkdir -p "${TOOL_DIR}"
cp "${DUMPER_SRC}/Il2CppDumper.dll" "${TOOL_DIR}/"
cp "${DUMPER_SRC}/Il2CppDumper.deps.json" "${TOOL_DIR}/"
cp "${DUMPER_SRC}/Il2CppDumper.runtimeconfig.json" "${TOOL_DIR}/"
cp "${DUMPER_SRC}"/Mono.Cecil*.dll "${TOOL_DIR}/"
cat > "${TOOL_DIR}/config.json" <<'JSON'
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

yntt_log "[1/3] 从底包提取 IL2CPP 核心文件"
unzip -p "${ORIG_APK}" 'lib/arm64-v8a/libil2cpp.so' > "${OUT}/libil2cpp.so"
unzip -p "${ORIG_APK}" 'assets/bin/Data/Managed/Metadata/global-metadata.dat' > "${OUT}/global-metadata.dat"
unzip -p "${ORIG_APK}" 'lib/arm64-v8a/libunity.so' > "${OUT}/libunity.so"
unzip -p "${ORIG_APK}" 'lib/arm64-v8a/libmain.so' > "${OUT}/libmain.so"
unzip -p "${ORIG_APK}" 'lib/arm64-v8a/libtapsdkcore.so' > "${OUT}/libtapsdkcore.so"
unzip -p "${ORIG_APK}" 'classes.dex' > "${OUT}/classes.dex"
unzip -p "${ORIG_APK}" 'classes2.dex' > "${OUT}/classes2.dex"
yntt_need_file "${OUT}/libil2cpp.so"
yntt_need_file "${OUT}/global-metadata.dat"

yntt_log "[2/3] Il2CppDumper 全量 dump（885K 行量级，耐心等）"
if [ ! -s "${DUMPER_OUT}/dump.cs" ] || [ ! -s "${DUMPER_OUT}/script.json" ]; then
  "${DOTNET}" "${TOOL_DIR}/Il2CppDumper.dll" \
    "${OUT}/libil2cpp.so" "${OUT}/global-metadata.dat" "${DUMPER_OUT}"
fi
yntt_need_file "${DUMPER_OUT}/dump.cs"
yntt_need_file "${DUMPER_OUT}/script.json"

yntt_log "[3/3] 写 summary.txt"
{
  file "${OUT}/libil2cpp.so"
  printf 'version=%s\n' "${YNTT_VER}"
  printf 'orig_apk_md5=%s\n' "${ORIG_MD5}"
  printf 'libil2cpp_sha256=%s\n' "$(yntt_sha256 "${OUT}/libil2cpp.so")"
  printf 'libil2cpp_size=%s\n' "$(yntt_size "${OUT}/libil2cpp.so")"
  printf 'metadata_sha256=%s\n' "$(yntt_sha256 "${OUT}/global-metadata.dat")"
  printf 'metadata_size=%s\n' "$(yntt_size "${OUT}/global-metadata.dat")"
  printf 'dump_cs_lines=%s\n' "$(wc -l < "${DUMPER_OUT}/dump.cs" | tr -d ' ')"
  printf 'dump_dir=%s\n' "${DUMPER_OUT}"
} > "${OUT}/summary.txt"
cat "${OUT}/summary.txt"
yntt_log "done"
