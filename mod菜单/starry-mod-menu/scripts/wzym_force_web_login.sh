#!/usr/bin/env bash
# 物质与魔法 6.03 星光包 — 仅补 TapTap 强制网页登录（外科换 classes8.dex）
#
# 底包默认：真机拉出的星光包 02_apks/物质与魔法/物质与魔法_6.03_starry_from_device.apk
# 旧版 Tap SDK：ActivityHandler.authorize 抛 ActivityNotFoundException
#   → LoginClient catch → WebViewHandler（与迷雾大陆同锚点 starry_force_web_login）
#
# 用法:
#   bash scripts/wzym_force_web_login.sh
#   STARRY_INSTALL=1 bash scripts/wzym_force_web_login.sh
#   WZYM_BASE_APK=/path/to.apk STARRY_INSTALL=1 bash scripts/wzym_force_web_login.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
OUT="$NIXIANG/08_output/物质与魔法"
PKG="com.huanshits.materialMagic"
BASE_APK="${WZYM_BASE_APK:-$NIXIANG/02_apks/物质与魔法/物质与魔法_6.03_starry_from_device.apk}"
SIGNED="$OUT/物质与魔法_6.03_starry-forceWebLogin-releaseSigned.apk"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
[[ -f "$SIGNER" ]] || { echo "ERROR: missing $SIGNER"; exit 1; }
mkdir -p "$OUT"

WORK="$(mktemp -d /tmp/wzym_force_web.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/4] baksmali classes8.dex from $(basename "$BASE_APK")"
unzip -p "$BASE_APK" classes8.dex > "$WORK/classes8_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c8smali" "$WORK/classes8_orig.dex"

echo "[2/4] patch ActivityHandler.authorize → force WebLogin"
python3.11 - "$WORK/c8smali" <<'PY'
from pathlib import Path
import re, sys

root = Path(sys.argv[1])
ah = root / "com/taptap/sdk/ui/ActivityHandler.smali"
if not ah.is_file():
    raise SystemExit(f"missing {ah}")
ah_t = ah.read_text(encoding="utf-8")
if "starry_force_web_login" in ah_t:
    print("  already patched")
else:
    FORCE_WEB = """.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .registers 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO
    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string p1, "starry_force_web_login"

    invoke-direct {v0, p1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method"""
    ah2, n = re.subn(
        r"\.method public authorize\(Lcom/taptap/sdk/LoginRequest;\)V[\s\S]*?\.end method",
        FORCE_WEB,
        ah_t,
        count=1,
    )
    if n != 1:
        raise SystemExit("ActivityHandler.authorize patch failed")
    ah.write_text(ah2, encoding="utf-8")
    print("  ActivityHandler.authorize → throw starry_force_web_login")
PY

"$SMALI_BIN" assemble -o "$WORK/classes8.dex" "$WORK/c8smali"
ls -la "$WORK/classes8.dex"

echo "[3/4] zip replace classes8.dex (keep rest byte-identical)"
python3.11 - "$BASE_APK" "$WORK/classes8.dex" "$WORK/unsigned.apk" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, new_dex, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
dex = new_dex.read_bytes()
zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    data = dex if name == "classes8.dex" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = item.create_system
    zout.writestr(info, data)
    n += 1
zin.close()
zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in ("resources.arsc", "classes.dex", "classes2.dex", "lib/arm64-v8a/libstarry_magic.so"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert hashlib.sha256(zout2.read("classes8.dex")).hexdigest() == hashlib.sha256(dex).hexdigest()
assert b"starry_force_web_login" in zout2.read("classes8.dex")
print(f"  merged ok; entries={n}; classes8={len(dex)}")
zin2.close()
zout2.close()
PY

echo "[4/4] zipalign + uber sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
mkdir -p "$WORK/signed"
UBER_ARGS=(-a "$ALIGNED" --allowResign --out "$WORK/signed")
[[ -n "$BT" ]] && UBER_ARGS+=(--zipAlignPath "$BT/zipalign")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
SIGNED_SRC="$(ls -t "$WORK/signed"/*Signed*.apk 2>/dev/null | head -1 || true)"
[[ -n "${SIGNED_SRC:-}" ]] || SIGNED_SRC="$(ls -t "$WORK/signed"/*.apk 2>/dev/null | head -1)"
[[ -f "${SIGNED_SRC:-}" ]] || { echo "ERROR: signed apk missing"; ls -la "$WORK/signed"; exit 1; }
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"
if [[ -n "$BT" ]]; then
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
fi

# 自检：dex 含锚点
unzip -p "$SIGNED" classes8.dex | grep -aq starry_force_web_login && echo "  verify: starry_force_web_login OK"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/wzym_force_web.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover-install failed — 不自动卸载；请人工处理签名冲突后重试"
    exit 1
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "installed: $PKG"
fi

echo "DONE: $SIGNED"
