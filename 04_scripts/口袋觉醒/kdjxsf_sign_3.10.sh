#!/usr/bin/env bash
# ============================================================
# 口袋觉醒（私服 com.zcdsg.xmgm 3.10）— 星光签名阶段
#   输入：08_output/口袋觉醒/口袋觉醒_私服_3.10_starry-unsigned.apk
#   输出：08_output/口袋觉醒/口袋觉醒_私服_3.10_starry-releaseSigned.apk
#   签名：01_tools/starry_release.jks（CN=StarryMagic）
# ============================================================
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
GAME="口袋觉醒"
VERSION="3.10"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_私服_${VERSION}_starry-unsigned.apk"
SIGNED="$OUT/${GAME}_私服_${VERSION}_starry-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILD="$NIXIANG/mod菜单/starry-mod-menu/libs_kdjx/arm64-v8a/libstarry_magic.so"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$UNSIGNED" ]] || { echo "ERROR: missing $UNSIGNED （先跑 surgical）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: zipalign 未找到（build-tools）" >&2; exit 1; }

WORK="$(mktemp -d /tmp/kdjxsf_sign.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

echo "[1/3] zipalign -p 4（1.9GB，耐心）"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"

echo "[2/3] apksigner（CN=StarryMagic）"
rm -f "$SIGNED"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -3

echo "[3/3] 产物自检"
python3 - "$SIGNED" "$SO_BUILD" <<'PY'
import sys, zipfile, hashlib
apk, sop = sys.argv[1:3]
with zipfile.ZipFile(apk) as z:
    names = z.namelist()
    for need in ('classes.dex', 'classes2.dex', 'AndroidManifest.xml',
                 'lib/arm64-v8a/libcocos2dlua.so',
                 'lib/arm64-v8a/libstarry_magic.so'):
        if need not in names:
            raise SystemExit(f'缺条目 {need}')
    so = z.read('lib/arm64-v8a/libstarry_magic.so')
    if so != open(sop, 'rb').read():
        raise SystemExit('包内 starry so != 构建产物（so 没换成功）')
    if b'Kdjx' not in so:
        raise SystemExit('包内 so 不含本游戏标签 [Kdjx]')
print('  ok: 包内 so 与构建产物一致且带 [Kdjx] 标签')
PY
echo "[signed] $SIGNED"
echo "[signed] size = $(stat -f%z "$SIGNED") B  md5 = $(md5 -q "$SIGNED")"
