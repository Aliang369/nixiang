#!/usr/bin/env bash
# 一念通天 pipeline 阶段 sign —— zipalign + StarryMagic 签名（版本无关）。
#
# 签名身份：CN=StarryMagic（与官方 CN=demo 异签 ⇒ 覆盖安装必然 INSTALL_FAILED_UPDATE_INCOMPATIBLE，
# 装机必须由用户当轮授权卸载重装，见 10_docs/一念通天/硬约束.md A#4）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/yntt_common.sh"

UNSIGNED="${OUT_DIR}/${GAME}_${YNTT_VER}_surgical-unsigned.apk"
SIGNED="${OUT_DIR}/${GAME}_${YNTT_VER}_surgical-releaseSigned.apk"
KS="${NIXIANG_ROOT}/01_tools/starry_release.jks"
UBER="${NIXIANG_ROOT}/01_tools/uber-apk-signer.jar"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+${JAVA_HOME}/bin:}${PATH}"

yntt_need_file "${UNSIGNED}"
yntt_need_file "${KS}"
[ -n "${NIXIANG_APKSIGNER:-}" ] && yntt_need_file "${NIXIANG_APKSIGNER}"
[ -n "${NIXIANG_ZIPALIGN:-}" ] && yntt_need_file "${NIXIANG_ZIPALIGN}"

WORK="$(mktemp -d /tmp/yntt_sign.XXXXXX)"
trap 'rm -rf "${WORK}"' EXIT
ALIGNED="${WORK}/aligned.apk"

if [ -n "${NIXIANG_APKSIGNER:-}" ] && [ -n "${NIXIANG_ZIPALIGN:-}" ]; then
  yntt_log "签名链路：build-tools zipalign + apksigner"
  "${NIXIANG_ZIPALIGN}" -f -p 4 "${UNSIGNED}" "${ALIGNED}"
  "${NIXIANG_APKSIGNER}" sign \
    --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
    --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
    --out "${SIGNED}" "${ALIGNED}"
  "${NIXIANG_APKSIGNER}" verify --print-certs "${SIGNED}" 2>&1 | sed -n '1,6p' || true
else
  yntt_log "签名链路：uber-apk-signer 兜底（build-tools 未解析到）"
  yntt_need_file "${UBER}"
  cp "${UNSIGNED}" "${ALIGNED}"
  java -jar "${UBER}" -a "${ALIGNED}" --out "${WORK}/signed" --allowResign \
    --ks "${KS}" --ksAlias "${KS_ALIAS}" --ksPass "${KS_PASS}" --ksKeyPass "${KS_PASS}" >/dev/null
  CAND="$(ls -t "${WORK}"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [ -n "${CAND}" ] || yntt_die "uber-apk-signer 未产出签名包"
  cp -f "${CAND}" "${SIGNED}"
fi

yntt_need_file "${SIGNED}"
if [ -n "${NIXIANG_APKSIGNER:-}" ]; then
  "${NIXIANG_APKSIGNER}" verify "${SIGNED}" >/dev/null || yntt_die "apksigner verify 失败"
fi

unzip -l "${SIGNED}" > "${WORK}/entries.txt"
for e in classes.dex classes2.dex classes3.dex lib/arm64-v8a/libstarry_magic.so resources.arsc; do
  grep -q " ${e}\$" "${WORK}/entries.txt" || yntt_die "签名包缺条目 ${e}"
done

SO_MD5="$(yntt_md5 "${NIXIANG_ROOT}/mod菜单/starry-mod-menu/libs_yntt/arm64-v8a/libstarry_magic.so")"
IN_SO_MD5="$(unzip -p "${SIGNED}" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[ "${SO_MD5}" = "${IN_SO_MD5}" ] || yntt_die "签名包内 so 与构建产物不一致"

yntt_log "signed=${SIGNED}"
yntt_log "size=$(yntt_size "${SIGNED}")"
yntt_log "md5=$(yntt_md5 "${SIGNED}")"
yntt_log "sha256=$(yntt_sha256 "${SIGNED}")"
yntt_log "so_md5=${IN_SO_MD5}"
yntt_log "done"
