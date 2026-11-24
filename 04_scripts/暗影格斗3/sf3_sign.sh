#!/usr/bin/env bash
# 暗影格斗3 1.45.3 — 签名阶段（pipeline sign）
set -euo pipefail

# ============================================================================
# 平台垫层（2026-11-24 builder：Windows Git Bash / macOS 双跑；只改路径与工具探测）
# ============================================================================
NIXIANG="${NIXIANG_ROOT:-/Users/aliang/Documents/nixiang}"
# 非登录 shell（脚本直跑）不会加载 ~/.bashrc 的 NIXIANG_ROOT ⇒ 缺 workspace 时以脚本位置为准（双机同解）
if [ ! -d "$NIXIANG/01_tools" ]; then
  NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
fi
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) HOST_OS=win ;;
  *)                    HOST_OS=unix ;;
esac
if command -v md5sum >/dev/null 2>&1; then
  md5() { if [ "${1:-}" = "-q" ]; then shift; md5sum "$@" | awk '{print $1}'; else md5sum "$@"; fi; }
fi
fsize() { stat -c '%s' "$1" 2>/dev/null || stat -f '%z' "$1"; }

GAME="暗影格斗3"
VERSION="1.45.3"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

# JDK 17：NIXIANG_JAVA17（_common/env.sh）→ JAVA_HOME → mac /usr/libexec/java_home
if [ -z "${JAVA_HOME:-}" ] && [ -n "${NIXIANG_JAVA17:-}" ]; then
  JAVA_HOME="$NIXIANG_JAVA17"
fi
if [ -z "${JAVA_HOME:-}" ] && [ "$HOST_OS" = "unix" ]; then
  JAVA_HOME="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
fi
export JAVA_HOME="${JAVA_HOME:-}"
if [ -n "$JAVA_HOME" ]; then export PATH="$JAVA_HOME/bin:$PATH"; fi

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing ${UNSIGNED}（先跑 surgical）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

# build-tools 解析：本仓 01_tools/android-sdk → mac 旧路径 → %LOCALAPPDATA%/Android/Sdk
# （取第一个带 zipalign 的；Win 侧真实文件是 zipalign.exe / apksigner.bat）
try_bt() {
  local root="$1" v d
  [ -n "$root" ] && [ -d "$root" ] || return 1
  for v in 36.0.0 35.0.0 34.0.0; do
    d="$root/$v"
    if [ -x "$d/zipalign" ] || [ -f "$d/zipalign.exe" ]; then printf '%s' "$d"; return 0; fi
  done
  d="$(ls -1 "$root" 2>/dev/null | sort -V | tail -1)"
  if [ -n "$d" ]; then
    d="$root/$d"
    if [ -x "$d/zipalign" ] || [ -f "$d/zipalign.exe" ]; then printf '%s' "$d"; return 0; fi
  fi
  return 1
}
BT="$(try_bt "$NIXIANG/01_tools/android-sdk/build-tools" || true)"
if [ -z "$BT" ] && [ -n "${LOCALAPPDATA:-}" ]; then
  BT="$(try_bt "$(cygpath -u "$LOCALAPPDATA" 2>/dev/null)/Android/Sdk/build-tools" || true)"
fi
if [ -z "$BT" ]; then
  BT="$(try_bt "$HOME/Library/Android/sdk/build-tools" || true)"
fi
ZA_BIN=""; AS_BIN=""; AS_JAR=""
if [ -n "$BT" ]; then
  if [ -x "$BT/zipalign" ]; then ZA_BIN="$BT/zipalign"; elif [ -f "$BT/zipalign.exe" ]; then ZA_BIN="$BT/zipalign.exe"; fi
  # Win 侧优先 java -jar：apksigner.bat 经 cmd.exe 转发时会把含空格的 JAVA_HOME 拆开（实测 'C:\Program' 不是内部或外部命令）
  if [ -x "$BT/apksigner" ]; then AS_BIN="$BT/apksigner"; elif [ -f "$BT/lib/apksigner.jar" ]; then AS_JAR="$BT/lib/apksigner.jar"; elif [ -f "$BT/apksigner.bat" ]; then AS_BIN="$BT/apksigner.bat"; fi
fi
apksigner_cmd() { if [ -n "$AS_BIN" ]; then "$AS_BIN" "$@"; else java -jar "$AS_JAR" "$@"; fi; }
echo "[sign] build-tools = ${BT:-none} / zipalign=${ZA_BIN:-none} / apksigner=${AS_BIN:-${AS_JAR:-none}}"

WORK="$(mktemp -d "${TMPDIR:-/tmp}/sf3_sign.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

if [ -n "$ZA_BIN" ]; then
  "$ZA_BIN" -f -p 4 "$UNSIGNED" "$ALIGNED"
  rm -f "$SIGNED"
  if [ -n "$AS_BIN" ] || [ -n "$AS_JAR" ]; then
    apksigner_cmd sign \
      --ks "$KS" --ks-key-alias "$KS_ALIAS" \
      --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
      --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
      --out "$SIGNED" "$ALIGNED"
    apksigner_cmd verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
  fi
fi

if [[ ! -f "$SIGNED" ]]; then
  [[ -n "$BT" ]] || cp "$UNSIGNED" "$ALIGNED"
  java -jar "$SIGNER" -a "$ALIGNED" --out "$WORK/signed" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS" >/dev/null
  CAND="$(ls -t "$WORK"/signed/*Signed*.apk 2>/dev/null | sed -n '1p' || true)"
  [[ -n "${CAND:-}" ]] || { echo "ERROR: uber-apk-signer produced nothing" >&2; exit 1; }
  cp -f "$CAND" "$SIGNED"
fi

[[ -f "$SIGNED" ]] || { echo "ERROR: sign failed" >&2; exit 1; }

if [ -n "$AS_BIN" ] || [ -n "$AS_JAR" ]; then
  apksigner_cmd verify "$SIGNED" >/dev/null || { echo "ERROR: apksigner verify failed" >&2; exit 1; }
fi

unzip -l "$SIGNED" > "$WORK/entries.txt"
for n in classes.dex classes7.dex classes8.dex; do
  grep -q " $n$" "$WORK/entries.txt" || { echo "ERROR: $n missing" >&2; exit 1; }
done
grep -q "lib/arm64-v8a/libstarry_magic.so" "$WORK/entries.txt" \
  || { echo "ERROR: starry so missing" >&2; exit 1; }

SO_MD5="$(md5 -q "$NIXIANG/mod菜单/starry-mod-menu/libs_sf3/arm64-v8a/libstarry_magic.so" 2>/dev/null || echo none)"
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] \
  || { echo "ERROR: signed so ($IN_SO_MD5) != build_so ($SO_MD5)" >&2; exit 1; }

# 官包游戏本体零改动硬校验（5 个库：libil2cpp / global-metadata / libaccess / libcpp / libcore）
# 期望值为官方底包实测（02_apks/暗影格斗3/…_taptap_official_from_device.apk，2026-11-24 重算）；
# 硬约束 A-3「游戏本体二进制保持官方零改动」——任一不等即 FAIL，禁止放宽。
IL2CPP_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libil2cpp.so | md5 -q)"
[[ "$IL2CPP_MD5" == "53345cdb4d31dff336fdd575fb87b667" ]] \
  || { echo "ERROR: libil2cpp.so 被改动（${IL2CPP_MD5}）" >&2; exit 1; }
META_MD5="$(unzip -p "$SIGNED" assets/bin/Data/Managed/Metadata/global-metadata.dat | md5 -q)"
[[ "$META_MD5" == "809ec525f0875c2092dc7f21e73fb288" ]] \
  || { echo "ERROR: global-metadata.dat 被改动（${META_MD5}）" >&2; exit 1; }
ACCESS_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libaccess.so | md5 -q)"
[[ "$ACCESS_MD5" == "82ee8142fa3a5996cb5eb7bd4a43677e" ]] \
  || { echo "ERROR: libaccess.so 被改动（${ACCESS_MD5}）" >&2; exit 1; }
LIBCPP_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libcpp.so | md5 -q)"
[[ "$LIBCPP_MD5" == "7dad26fcca3bfe85df53a45a046959e9" ]] \
  || { echo "ERROR: libcpp.so 被改动（${LIBCPP_MD5}）" >&2; exit 1; }
LIBCORE_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libcore.so | md5 -q)"
[[ "$LIBCORE_MD5" == "4b0b4e792b2670e8f0f5bab8d47aa08b" ]] \
  || { echo "ERROR: libcore.so 被改动（${LIBCORE_MD5}）" >&2; exit 1; }
echo "[sign] 5 库 md5 断言 OK：il2cpp=$IL2CPP_MD5 metadata=$META_MD5 access=$ACCESS_MD5 cpp=$LIBCPP_MD5 core=$LIBCORE_MD5"

echo "[sign] done → $SIGNED"
echo "[sign] md5 = $(md5 -q "$SIGNED")"
echo "[sign] sha256 = $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "[sign] size = $(fsize "$SIGNED")"
ls -lh "$SIGNED"
