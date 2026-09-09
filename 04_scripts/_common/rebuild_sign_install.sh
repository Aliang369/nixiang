#!/usr/bin/env bash
# apktool 重建 → 对齐/签名 → 可选 adb 安装
# 优先 uber-apk-signer（内含 zipalign）；否则用 SDK build-tools 的 zipalign+apksigner
#
# 用法:
#   ./rebuild_sign_install.sh <游戏名> <07_patched下的目录> [版本标签] [--install] [--reinstall]
# 示例:
#   ./rebuild_sign_install.sh 战魂铭人 "$NIXIANG_ROOT/07_patched/战魂铭人/apktool_3.4.0_modmenu" 3.4.0_modmenu --install
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
source "$SCRIPT_DIR/env.sh"

INSTALL=0
REINSTALL=0
ARGS=()
for a in "$@"; do
  case "$a" in
    --install) INSTALL=1 ;;
    --reinstall) REINSTALL=1; INSTALL=1 ;;
    *) ARGS+=("$a") ;;
  esac
done

[[ ${#ARGS[@]} -ge 2 ]] || {
  echo "用法: $0 <游戏名> <patched目录> [版本标签] [--install] [--reinstall]"
  exit 1
}

GAME="${ARGS[0]}"
PROJECT="${ARGS[1]}"
TAG="${ARGS[2]:-modified}"

[[ -d "$PROJECT" ]] || { echo "目录不存在: $PROJECT" >&2; exit 1; }
PROJECT="$(cd "$PROJECT" && pwd)"

need() { command -v "$1" >/dev/null || { echo "缺少工具: $1"; exit 1; }; }
need apktool
need java

OUT_DIR="$NIXIANG_ROOT/08_output/$GAME"
mkdir -p "$OUT_DIR"
UNSIGNED="$OUT_DIR/${GAME}_${TAG}_unsigned.apk"

echo "==> apktool b → $UNSIGNED"
apktool b -f "$PROJECT" -o "$UNSIGNED"

SIGNED=""
METHOD=""

if [[ -f "$NIXIANG_UBER_SIGNER" ]]; then
  METHOD="uber-apk-signer"
  echo "==> uber-apk-signer → $OUT_DIR"
  # uber 自己会 zipalign（若本机有 zipalign 更好；没有则可能 --skipZipAlign）
  UBER_ARGS=(-a "$UNSIGNED" -o "$OUT_DIR" --allowResign)
  if [[ -n "${NIXIANG_ZIPALIGN:-}" && -x "${NIXIANG_ZIPALIGN}" ]]; then
    UBER_ARGS+=(--zipAlignPath "$NIXIANG_ZIPALIGN")
  else
    echo "提示: 未找到 zipalign，uber 将尽量处理；建议把 SDK build-tools 加入 PATH" >&2
  fi
  java -jar "$NIXIANG_UBER_SIGNER" "${UBER_ARGS[@]}"
  # 常见产物名: *_aligned-debugSigned.apk
  SIGNED="$(ls -t "$OUT_DIR"/*-debugSigned.apk "$OUT_DIR"/*_signed.apk 2>/dev/null | head -1 || true)"
  if [[ -z "$SIGNED" ]]; then
    # 回退：找最新 apk（排除 unsigned）
    SIGNED="$(ls -t "$OUT_DIR"/*.apk 2>/dev/null | grep -v unsigned | head -1 || true)"
  fi
elif [[ -n "${NIXIANG_ZIPALIGN:-}" && -x "${NIXIANG_ZIPALIGN}" && -n "${NIXIANG_APKSIGNER:-}" && -x "${NIXIANG_APKSIGNER}" ]]; then
  METHOD="zipalign+apksigner"
  ALIGNED="$OUT_DIR/${GAME}_${TAG}_aligned.apk"
  SIGNED="$OUT_DIR/${GAME}_${TAG}_signed.apk"
  KS="${NIXIANG_DEBUG_KS:-$OUT_DIR/.nixiang-debug.jks}"
  if [[ ! -f "$KS" ]]; then
    echo "==> 生成调试 keystore: $KS"
    keytool -genkeypair -v -keystore "$KS" -storepass android -keypass android \
      -alias androiddebugkey -keyalg RSA -keysize 2048 -validity 10000 \
      -dname "CN=nixiang,O=Android,C=US" >/dev/null
  fi
  echo "==> zipalign"
  "$NIXIANG_ZIPALIGN" -f -p 4 "$UNSIGNED" "$ALIGNED"
  echo "==> apksigner"
  "$NIXIANG_APKSIGNER" sign --ks "$KS" --ks-pass pass:android --key-pass pass:android \
    --out "$SIGNED" "$ALIGNED"
  "$NIXIANG_APKSIGNER" verify --verbose "$SIGNED" || true
else
  echo "错误: 找不到签名工具。" >&2
  echo "  需要其一: $NIXIANG_UBER_SIGNER" >&2
  echo "  或 SDK build-tools 中的 zipalign+apksigner（当前 BUILD_TOOLS=${NIXIANG_BUILD_TOOLS:-无}）" >&2
  exit 1
fi

echo
echo "签名方式: $METHOD"
echo "未签名:   $UNSIGNED"
echo "已签名:   ${SIGNED:-（未定位到产物，请到 08_output/$GAME/ 查看）}"

if [[ "$INSTALL" -eq 1 ]]; then
  need adb
  [[ -n "$SIGNED" && -f "$SIGNED" ]] || { echo "没有可安装的签名包"; exit 1; }
  echo "==> adb install"
  if [[ "$REINSTALL" -eq 1 ]]; then
    adb install -r "$SIGNED"
  else
    adb install "$SIGNED"
  fi
fi
