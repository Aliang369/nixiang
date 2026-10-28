#!/usr/bin/env bash
# 圣翼传说 7.6.06 装机前 L1 静态门禁（只读本地产物 + 设备画像）。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="圣翼传说"
VERSION="7.6.06"
VC="7606"
PKG="com.aiyue.wscq"
SERIAL="f9a116a2"

BASE_APK="$NIXIANG/09_test/$GAME/base_20261026/base.apk"
BASE_MD5="a39c4219859570a489450bcee5a50dec"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
SO="$NIXIANG/mod菜单/starry-mod-menu/libs_wscq/arm64-v8a/libstarry_magic.so"
DEX="$NIXIANG/07_patched/$GAME/surgical_7.6.06/classes2.dex"
MENU_JSON="$NIXIANG/10_docs/$GAME/sycs_menu_template.json"
STARRY_CPP="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni/Games/WscqMod.cpp"
MAIN_CPP="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni/MainModern.cpp"
REPORT_DIR="$NIXIANG/09_test/$GAME/base_20261026/preinstall"
REPORT="$REPORT_DIR/preinstall_${VERSION}_20261026.txt"
IL2CPP_SHA256="69eea5d129e1568ea12cf94647e346fee0fa0c3feac8fdb6029030914f4b532c"
META_SHA256="0f7115c7c8bf3f93815aa0fd078052d5a2a8db7be632be4a222c0ed4270b0c42"

for f in "$BASE_APK" "$APK" "$SO" "$DEX" "$MENU_JSON" "$STARRY_CPP" "$MAIN_CPP"; do
  [[ -f "$f" ]] || { echo "ERROR: missing $f" >&2; exit 1; }
done
[[ "$(md5 -q "$BASE_APK")" == "$BASE_MD5" ]] \
  || { echo "ERROR: base APK md5 mismatch" >&2; exit 1; }

mkdir -p "$REPORT_DIR"
: > "$REPORT"
exec > >(tee -a "$REPORT") 2>&1

echo "[preinstall] game=$GAME version=$VERSION vc=$VC package=$PKG"
echo "[preinstall] apk=$(md5 -q "$APK") size=$(stat -f '%z' "$APK")"
echo "[preinstall] base=$(md5 -q "$BASE_APK") so=$(md5 -q "$SO")"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/aapt2" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: Android build-tools not found" >&2; exit 1; }

BADGING="$("$BT/aapt2" dump badging "$APK")"
printf '%s\n' "$BADGING" | grep -q "package: name='$PKG' versionCode='$VC' versionName='$VERSION'" \
  || { echo "ERROR: package/version mismatch" >&2; exit 1; }
printf '%s\n' "$BADGING" | grep -q 'application-debuggable' \
  && { echo "ERROR: debuggable package" >&2; exit 1; }
"$BT/apksigner" verify --print-certs "$APK" | grep -q 'CN=StarryMagic' \
  || { echo "ERROR: signer is not StarryMagic" >&2; exit 1; }

ENTRIES="$(unzip -l "$APK")"
for e in classes.dex classes2.dex AndroidManifest.xml \
         lib/arm64-v8a/libstarry_magic.so lib/arm64-v8a/libc++_shared.so \
         lib/arm64-v8a/libil2cpp.so \
         assets/bin/Data/Managed/Metadata/global-metadata.dat; do
  printf '%s\n' "$ENTRIES" | grep -q " $e$" \
    || { echo "ERROR: missing package entry $e" >&2; exit 1; }
done

SO_BUILT_MD5="$(md5 -q "$SO")"
SO_PKG_MD5="$(unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_BUILT_MD5" == "$SO_PKG_MD5" ]] \
  || { echo "ERROR: packaged starry so != build output" >&2; exit 1; }
SO_SHA256="$(unzip -p "$APK" lib/arm64-v8a/libil2cpp.so | shasum -a 256 | awk '{print $1}')"
META_SHA256_ACTUAL="$(unzip -p "$APK" assets/bin/Data/Managed/Metadata/global-metadata.dat | shasum -a 256 | awk '{print $1}')"
[[ "$SO_SHA256" == "$IL2CPP_SHA256" ]] \
  || { echo "ERROR: libil2cpp sha256 mismatch ($SO_SHA256)" >&2; exit 1; }
[[ "$META_SHA256_ACTUAL" == "$META_SHA256" ]] \
  || { echo "ERROR: metadata sha256 mismatch ($META_SHA256_ACTUAL)" >&2; exit 1; }

echo "[1/4] clean release"
bash "$NIXIANG/04_scripts/_common/check_release_clean.sh" \
  --src "$STARRY_CPP" --src "$MAIN_CPP" --so "$SO"
bash "$NIXIANG/04_scripts/_common/check_release_apk.sh" \
  --apk "$APK" --game "$GAME" --expect-vc "$VC" --expect-vn "$VERSION"

echo "[2/4] package cross-check"
bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" \
  --apk "$APK" --base-apk "$BASE_APK" \
  --expect-so-sha256 "$IL2CPP_SHA256" \
  --expect-metadata-sha256 "$META_SHA256" \
  --cpp "$STARRY_CPP" --cpp "$MAIN_CPP" --dex "$DEX" --so "$SO" \
  --menu-json "$MENU_JSON" \
  --needle 'com/starry/magic/StarryInitProvider' \
  --needle 'com/starry/magic/menu/MenuService' \
  --needle 'com/starry/magic/sign/StarryAppComponentFactory' \
  --needle 'com/starry/magic/sign/TapLoginForceWeb' \
  --needle 'com/starry/magic/sign/YnttLoginRequest' \
  --needle 'com/taptap/sdk/LoginRequest' \
  --needle 'com/starry/magic/bridge/ModBridge' \
  --game "$GAME" --rounds-threshold 3

echo "[3/4] menu fidelity"
python3 - "$MENU_JSON" <<'PY'
import json
import sys

want = [
    (0, "战斗", "category"),
    (1, "伤害倍率", "slider"),
    (2, "玩家无敌", "toggle"),
    (3, "全屏攻击", "toggle"),
]
with open(sys.argv[1], encoding="utf-8") as f:
    data = json.load(f)
feats = data.get("features", [])
got = []
for item in feats:
    name = item.get("name", "")
    typ = item.get("type", "toggle" if not name.startswith("Category_") and name != "战斗" else "category")
    got.append((item.get("feat_num"), name, typ))
if got != want:
    raise SystemExit(f"menu mismatch: {got!r}")
print("menu OK: feature set is exactly the three requested capabilities + category")
PY
for s in 'SeekBar_伤害倍率_1_1000_1' 'Toggle_玩家无敌' \
         'Toggle_全屏攻击'; do
  n="$(unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -acF "$s" || true)"
  [[ "${n:-0}" -ge 1 ]] \
    || { echo "ERROR: missing menu literal $s" >&2; exit 1; }
done

echo "[4/4] device profile"
bash "$NIXIANG/04_scripts/_common/device_profile.sh" \
  --serial "$SERIAL" --pkg "$PKG" --launch --idle-wait 8 \
  --rva EnemyGetHurt=0x13280B4 \
  --rva PlayerGetHurt=0x13CC2C8 \
  --rva PlayerFixed=0x13CE180 \
  --rva EnemyCtr2Fixed=0x138E280

bash "$NIXIANG/04_scripts/_common/install_guard.sh" "$GAME" --policy

echo "[preinstall] PASS repo=$REPORT"
