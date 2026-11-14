#!/usr/bin/env bash
# 动物餐厅12.42私有外科入口；默认离线，禁止此入口安装。
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
source "$HERE/../_common/env.sh"
export NIXIANG="$NIXIANG_ROOT"
export SERIAL=emulator-5554
[[ "${STARRY_INSTALL:-0}" == 0 ]] || { echo 'ERROR: build-only entry'; exit 2; }
BASE="$NIXIANG_ROOT/02_apks/动物餐厅/动物餐厅_12.42_taptap_official_from_device.apk"
WORK="$NIXIANG_ROOT/07_patched/动物餐厅/dwct1242"
mkdir -p "$WORK"
[[ "$(sha256sum "$BASE" | cut -d' ' -f1)" == b759601c5c9197901269a0b8ac8d31171b578fa08207ef02b565b52245a4f705 ]] || exit 3
bash "$NIXIANG_ROOT/mod菜单/starry-mod-menu/scripts/_lib/dex_target.sh" --apk "$BASE" --need-methods 2048
python3.11 - "$BASE" "$WORK" <<'PY'
import sys,zipfile,shutil
from pathlib import Path
with zipfile.ZipFile(sys.argv[1]) as z:
 p=Path(sys.argv[2])
 for n in ['classes_smali','classes2_smali','classes3_smali','menu_smali']:
  if (p/n).exists():shutil.rmtree(p/n)
 (p/'index.original.jsc').write_bytes(z.read('assets/assets/Script/index.jsc'))
 for n in z.namelist():
  if n.startswith('classes') and n.endswith('.dex'):(p/n).write_bytes(z.read(n))
 (p/'AndroidManifest.original.xml').write_bytes(z.read('AndroidManifest.xml'))
PY
node "$HERE/inspect_1242.js" "$(nixiang_win_path "$WORK/index.original.jsc")" "$(nixiang_win_path "$WORK/index.original.js")"
JAVA="$NIXIANG_JAVA17/bin/java"
for DEX in "$WORK"/classes*.dex; do
 "$JAVA" -jar "$(nixiang_win_path "$NIXIANG_ROOT/01_tools/smali/baksmali-fat.jar")" d "$(nixiang_win_path "$DEX")" -o "$(nixiang_win_path "$WORK/$(basename "$DEX" .dex)_smali")"
done
python3.11 "$HERE/inspect_main.py"
node "$HERE/inspect_1242.js" "$(nixiang_win_path "$WORK/main.original.jsc")" "$(nixiang_win_path "$WORK/main.original.js")"
node "$HERE/patch_1242.js" "$(nixiang_win_path "$WORK")"
node "$HERE/patch_hotupdate.js" "$(nixiang_win_path "$WORK")"
python3.11 "$HERE/patch_dwct_smali.py" "$(nixiang_win_path "$WORK")"
export ANDROID_SDK_ROOT="$(cd "$NIXIANG_BUILD_TOOLS/../.." && pwd)"
bash "$HERE/dwct_build_dex.sh"
bash "$HERE/dwct_build_so.sh"
MERGE_DEX="$(bash "$NIXIANG_ROOT/mod菜单/starry-mod-menu/scripts/_lib/dex_target.sh" --apk "$BASE" --need-methods 4096 --print-entry)"
MENU="$NIXIANG_ROOT/mod菜单/starry-mod-menu/templates/dwct1242_menu_current/menu.dex"
"$JAVA" -jar "$(nixiang_win_path "$NIXIANG_ROOT/01_tools/smali/baksmali-fat.jar")" d "$(nixiang_win_path "$MENU")" -o "$(nixiang_win_path "$WORK/menu_smali")"
MERGE_ROOT="$WORK/${MERGE_DEX%.dex}_smali"
[[ ! -d "$MERGE_ROOT/com/starry" ]] || { echo ERROR_duplicate_starry; exit 1; }
cp -a "$WORK/menu_smali/com/starry" "$MERGE_ROOT/com/"
python3.11 "$HERE/patch_dwct_binary_manifest.py" "$(nixiang_win_path "$WORK/AndroidManifest.original.xml")" "$(nixiang_win_path "$WORK/AndroidManifest.xml")"
python3.11 "$HERE/assemble_dwct.py" "$(nixiang_win_path "$WORK")" "$(nixiang_win_path "$NIXIANG_ROOT")" "$MERGE_DEX"
SIGNED="${DWCT_SIGNED_OUT:-$NIXIANG_ROOT/08_output/动物餐厅/动物餐厅_12.42_starry-local-releaseSigned.apk}"
mkdir -p "$(dirname "$SIGNED")"
"$NIXIANG_ZIPALIGN" -f -p 4 "$(nixiang_win_path "$WORK/unsigned.apk")" "$(nixiang_win_path "$WORK/aligned.apk")"
"$NIXIANG_APKSIGNER" sign --ks "$(nixiang_win_path "$NIXIANG_ROOT/01_tools/starry_release.jks")" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" --key-pass "pass:${STARRY_KS_PASS:-starry2026}" --out "$(nixiang_win_path "$SIGNED")" "$(nixiang_win_path "$WORK/aligned.apk")"
"$NIXIANG_APKSIGNER" verify --print-certs "$(nixiang_win_path "$SIGNED")" > "$WORK/final_certificate.txt"
bash "$NIXIANG_ROOT/04_scripts/_common/menu_tpl_gate.sh" --tpl "$NIXIANG_ROOT/mod菜单/starry-mod-menu/templates/dwct1242_menu_current" --dex-name menu.dex --require-stamp
SO_META_CHECK=0 bash "$NIXIANG_ROOT/04_scripts/_common/pre_install_gate.sh" --apk "$SIGNED" --base-apk "$BASE" --js "$WORK/Script.patched.js" --js "$WORK/main.patched.js" --cpp "$NIXIANG_ROOT/mod菜单/starry-mod-menu/app/src/main/jni/MainModern.cpp" --cpp "$NIXIANG_ROOT/mod菜单/starry-mod-menu/app/src/main/jni/Games/DwctMod.cpp" --dex "$WORK/$MERGE_DEX" --menu-json "$HERE/dwct_menu_template.json" --menu-dex "$SIGNED" --menu-dex-name "$MERGE_DEX" > "$WORK/pre_install_gate.log" 2>&1
sha256sum "$SIGNED"
echo "BUILD_ONLY_COMPLETE $SIGNED"
