#!/usr/bin/env bash
# ================================================================
#  冒险与征途 (com.mxyzt.gzmy 1.5.9 / vc159) — 仅换 libstarry_magic.so
# ----------------------------------------------------------------
#  用途：底包（真机上已是已验证的 v44 星光成品）**只替换一个 so**，
#        同签重打后 `pm install -r` 覆盖安装 —— 不动数据、不动卡米绑定、
#        不重跑外科注入（dex / 清单 / 证书 / libc++_shared 全保持原样）。
#
#  ★ 为什么必须同签：菜单 v5 的设备指纹只取 SSAID，SSAID 按**签名证书**隔离；
#    换证书 = 换设备 → 卡密 403「已绑定其他设备」。底包签名 = starry_release.jks。
#    脚本在打签后对「新包签名摘要 == 底包签名摘要」做断言，不同就直接 FAIL。
#
#  底包解析顺序：
#    1) $MXYTZ_BASE_APK
#    2) 02_apks/冒险与征途/冒险与征途_1.5.9_official_base.apk（win 侧常驻）
#    3) 02_apks/冒险与征途/冒险与征途_1.5.9_starry-final-releaseSigned.apk（v44 交付包）
#    4) 本机缓存 ~/.cache/nixiang/mxyzt_1.5.9_starry.apk
#       三个 sha256 都按 74147b36...（= v44 / 真机在装包，已实测同一文件）校验。
#
#  用法:
#    bash 04_scripts/冒险与征途/mxyzt_so_only_1.5.9.sh              # 只打包
#    STARRY_INSTALL=1 SERIAL=f9a116a2 bash .../mxyzt_so_only_1.5.9.sh   # 打包+覆盖安装
#
#  产物：08_output/冒险与征途/冒险与征途_1.5.9_soonly-<md5前8>-releaseSigned.apk
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SELF_DIR/../.." && pwd)"
STARRY="$NIXIANG/mod菜单/starry-mod-menu"
PKG="com.mxyzt.gzmy"
SERIAL="${SERIAL:-}"
APK_SHA="74147b363f9687139f14f73bb6c99ba82ccfe34aca5e11a6b2ea7bd9ece767ea"
SO="$STARRY/libs_mxyzt/arm64-v8a/libstarry_magic.so"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
OUT_DIR="$NIXIANG/08_output/冒险与征途"
CACHE="$HOME/.cache/nixiang/mxyzt_1.5.9_starry.apk"

sha256f() { if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}'; else shasum -a 256 "$1" | awk '{print $1}'; fi; }
md5f()    { if command -v md5sum   >/dev/null 2>&1; then md5sum   "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }

# ---- 底包 ----
pick_base() {
  local c
  for c in \
    "${MXYTZ_BASE_APK:-}" \
    "$NIXIANG/02_apks/冒险与征途/冒险与征途_1.5.9_starry-final-releaseSigned.apk" \
    "$NIXIANG/02_apks/冒险与征途/冒险与征途_1.5.9_official_base.apk" \
    "$CACHE"; do
    [ -n "$c" ] && [ -f "$c" ] && { printf '%s' "$c"; return 0; }
  done
  return 1
}
BASE_APK="$(pick_base)" || {
  echo "ERROR: 找不到底包。可显式指定 MXYTZ_BASE_APK=<v44 成品或官方底包>" >&2
  exit 1
}
echo "[0] BASE = $BASE_APK"
[ -f "$SO" ] || { echo "ERROR: 缺 ${SO}（先跑 04_scripts/冒险与征途/mxyzt_build_so.sh）" >&2; exit 1; }
SO_MD5="$(md5f "$SO")"; SO_TAG="${SO_MD5:0:8}"
echo "[0] SO   = $SO  md5=$SO_MD5"

# ---- 工具 ----
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[ -n "$BT" ] || { echo "ERROR: 找不到 Android build-tools（需要 apksigner/zipalign）" >&2; exit 1; }
[ -f "$KS" ] || { echo "ERROR: 缺签名库 $KS" >&2; exit 1; }
echo "[0] BT   = $BT"

# ---- 工作区 ----
WORK="$(mktemp -d /tmp/mxyzt_soonly.XXXXXX)"
cleanup() { rm -rf "$WORK"; }
trap cleanup EXIT
mkdir -p "$OUT_DIR"

echo "[1/5] 取底包签名摘要（用于同签断言）"
BASE_CERT="$("$BT/apksigner" verify --print-certs "$BASE_APK" 2>/dev/null | grep -m1 'certificate SHA-256 digest' | awk '{print $NF}')"
echo "      base SHA-256 = $BASE_CERT"
[ -n "$BASE_CERT" ] || { echo "ERROR: 无法读取底包证书摘要" >&2; exit 1; }

echo "[2/5] 复刻底包 → 仅替换 lib/arm64-v8a/libstarry_magic.so"
cp -f "$BASE_APK" "$WORK/out.apk"
( cd "$WORK" && zip -qu out.apk ) || true
cp -f "$SO" "$WORK/libstarry_magic.so"

python3 - "$WORK/out.apk" "$WORK/libstarry_magic.so" <<'PY'
import sys, zipfile, shutil, zlib
apk, so = sys.argv[1], sys.argv[2]
tmp = apk + ".tmp"
TARGET = 'lib/arm64-v8a/libstarry_magic.so'
# 原 APK 里 lib/*.so 一律 ZIP_STORED（未压缩）⇒ 其余条目**原样搬压缩流**，
# 不解压不重压：885MB 的包也能在几十秒内完成，且不动任何非目标 entry 的字节。
with open(so, 'rb') as f: newso = f.read()
with zipfile.ZipFile(apk, 'r') as zin, zipfile.ZipFile(tmp, 'w', zipfile.ZIP_STORED) as zout:
    for it in zin.infolist():
        zi = zipfile.ZipInfo(it.filename, date_time=it.date_time)
        zi.compress_type = it.compress_type
        zi.external_attr = it.external_attr
        zi.internal_attr = it.internal_attr
        zi.create_system = it.create_system
        zi.flag_bits = it.flag_bits
        if it.filename == TARGET:
            zi.compress_type = zipfile.ZIP_STORED
            zout.writestr(zi, newso)
        else:
            with zin.open(it) as src:
                zout.writestr(zi, src.read())
shutil.move(tmp, apk)
print("      replaced libstarry_magic.so inside apk")
PY

echo "[3/5] zipalign -p 4 + apksigner sign"
"$BT/zipalign" -f -p 4 "$WORK/out.apk" "$WORK/aligned.apk" >/dev/null
SIGNED="$OUT_DIR/冒险与征途_1.5.9_soonly-${SO_TAG}-releaseSigned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -3

echo "[4/5] 同签断言（换证书 = 换设备 = 卡密 403，必须一致）"
NEW_CERT="$("$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | grep -m1 'certificate SHA-256 digest' | awk '{print $NF}')"
if [ "$NEW_CERT" != "$BASE_CERT" ]; then
  echo "ERROR: 签名摘要不一致！base=$BASE_CERT new=$NEW_CERT" >&2
  exit 1
fi
echo "      OK 同签：$NEW_CERT"

echo "[5/5] 包内 so 摘要校验"
python3 - "$SIGNED" "$SO_MD5" <<'PY'
import sys, zipfile, hashlib
apk, want = sys.argv[1], sys.argv[2]
with zipfile.ZipFile(apk) as z:
    d = z.read('lib/arm64-v8a/libstarry_magic.so')
got = hashlib.md5(d).hexdigest()
assert got == want, f"so md5 mismatch: {got} != {want}"
print(f"      OK libstarry_magic.so md5 = {got}")
PY
ls -lh "$SIGNED"

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  : "${SERIAL:?STARRY_INSTALL=1 时必须给 SERIAL=<设备序列号>}"
  echo "[install] push + pm install -r -d（保数据、不降级）"
  adb -s "$SERIAL" push "$SIGNED" /data/local/tmp/mxyzt_soonly.apk
  adb -s "$SERIAL" shell am force-stop "$PKG" || true
  adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 /data/local/tmp/mxyzt_soonly.apk'"
  adb -s "$SERIAL" shell rm -f /data/local/tmp/mxyzt_soonly.apk
  echo "[install] done. 装机后自检："
  echo "  adb -s $SERIAL logcat -c && adb -s $SERIAL shell monkey -p $PKG -c android.intent.category.LAUNCHER 1"
  echo "  adb -s $SERIAL logcat -s Mod_Menu:V | grep -E 'maps\\[|interp base=|interp DobbyHook'"
fi
