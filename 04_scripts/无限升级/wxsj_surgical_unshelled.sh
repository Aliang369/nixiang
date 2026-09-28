#!/usr/bin/env bash
# WXSJ 12.10701 unshelled surgical rebuild.
#
# Inputs:
#   - official 12.10701 base APK (still contains the NIS wrapper)
#   - every runtime DEX in RUNTIME_DIR except the menu DEX
#   - Starry menu DEX and native library copied from the current Starry package
#
# Output:
#   unshelled APK with Manifest Application -> com.starry.magic.StarryApp
#   and the NIS wrapper/application/native loader removed.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

ORIG_APK="${WXSJ_ORIG_APK:-$NIXIANG/07_patched/无限升级/installed_orig_20260922/base.apk}"
RUNTIME_DIR="${WXSJ_UNSHELL_DIR:-$NIXIANG/06_dumped/无限升级/runtime_20260928_unshelled}"
OUT_DIR="${WXSJ_OUT_DIR:-$NIXIANG/08_output/无限升级}"
OUT_APK="${WXSJ_OUT_APK:-$OUT_DIR/无限升级_12.10701_unshelled-menu-releaseSigned.apk}"

GAME_DEX="$RUNTIME_DIR/game_main.dex"
UNITY_DEX="$RUNTIME_DIR/unity_runtime.dex"
ANYTHINK_DEX="$RUNTIME_DIR/anythink.dex"
STARRY_DEX="$RUNTIME_DIR/starry_menu.dex"
STARRY_SO="$RUNTIME_DIR/libstarry_magic.so"
LIBCXX_SO="$RUNTIME_DIR/libc++_shared.so"
KOTLIN_DEX="$RUNTIME_DIR/kotlin_missing.dex"
SUPPORT_DEX="$RUNTIME_DIR/runtime_support.dex"
JOOR_DEX="$RUNTIME_DIR/joor.dex"

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
APKTOOL_BIN="${APKTOOL_BIN:-/opt/homebrew/bin/apktool}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"

PKG="com.xingzuodaoshi.infinityUpgrade"
APP_CLASS="com.starry.magic.StarryApp"
PROVIDER_CLASS="com.starry.magic.StarryInitProvider"
PROVIDER_AUTH="$PKG.starryinit"
MENU_SERVICE="com.starry.magic.menu.MenuService"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK: $ORIG_APK"; exit 1; }
[[ -f "$GAME_DEX" ]] || { echo "ERROR: missing game DEX: $GAME_DEX"; exit 1; }
[[ -f "$UNITY_DEX" ]] || { echo "ERROR: missing Unity DEX: $UNITY_DEX"; exit 1; }
[[ -f "$ANYTHINK_DEX" ]] || { echo "ERROR: missing AnyThink DEX: $ANYTHINK_DEX"; exit 1; }
[[ -f "$STARRY_DEX" ]] || { echo "ERROR: missing Starry DEX: $STARRY_DEX"; exit 1; }
[[ -f "$STARRY_SO" ]] || { echo "ERROR: missing Starry SO: $STARRY_SO"; exit 1; }
[[ -f "$LIBCXX_SO" ]] || { echo "ERROR: missing libc++_shared.so: $LIBCXX_SO"; exit 1; }
[[ -f "$KOTLIN_DEX" ]] || { echo "ERROR: missing Kotlin DEX: $KOTLIN_DEX"; exit 1; }
[[ -f "$SUPPORT_DEX" ]] || { echo "ERROR: missing runtime support DEX: $SUPPORT_DEX"; exit 1; }
[[ -f "$JOOR_DEX" ]] || { echo "ERROR: missing jOOR DEX: $JOOR_DEX"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing smali/baksmali"; exit 1; }
[[ -x "$APKTOOL_BIN" ]] || { echo "ERROR: missing apktool: $APKTOOL_BIN"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing signing key: $KS"; exit 1; }

check_hash() {
  local file="$1" want="$2" got
  got="$(shasum -a 256 "$file" | awk '{print $1}')"
  [[ "$got" == "$want" ]] || {
    echo "ERROR: hash mismatch for $file"
    echo "  want=$want"
    echo "  got =$got"
    exit 1
  }
}

check_hash "$GAME_DEX" "4af40c7651e8e6c75a90be8f848d6d7346ddfd4f5f2627bd49168972de6dcdc1"
check_hash "$UNITY_DEX" "096680fa7091080b6898b12ab5136f50029377062e0fd70ffee280aed1dde5db"
check_hash "$ANYTHINK_DEX" "891c5ef541cc8ca45fbdb458defc4ba479ee2dbb2ff18b6d8e6b3b68b349ef90"
check_hash "$STARRY_DEX" "c5e5dc08d8f62a789c891f744da630c6cd714f23af564edad155cc88c0a4548c"
check_hash "$STARRY_SO" "3b358a66deb6bbf7616605d6289fd7cc0e758bf7a39178de2ebbcf11598fa6e1"
check_hash "$LIBCXX_SO" "92ee8da641b969254cc8723f64aa115d36c8ffa87115e38bc9cb89e3eefee839"
check_hash "$KOTLIN_DEX" "f350944b4a936469db4ca4506d9dd9dd3c05157a4d870af6e962b48656ca0371"
check_hash "$SUPPORT_DEX" "66197d8bb929e69d8662553d005742a46a1224a33d7ef10adaf7d7ba7757aa4e"
check_hash "$JOOR_DEX" "cafb2f539b65f19da71d81b917f4fca2376f023aa194dfd2bee812bb70119d52"

RUNTIME_DEXES=()
add_runtime_dex() {
  local file="$1"
  [[ -f "$file" ]] || return 0
  local absolute
  absolute="$(cd "$(dirname "$file")" && pwd)/$(basename "$file")"
  local existing
  if [[ ${#RUNTIME_DEXES[@]} -gt 0 ]]; then
    for existing in "${RUNTIME_DEXES[@]}"; do
      if [[ "$existing" == "$absolute" ]]; then
        return 0
      fi
    done
  fi
  RUNTIME_DEXES+=("$absolute")
}

add_runtime_dex "$GAME_DEX"
add_runtime_dex "$UNITY_DEX"
add_runtime_dex "$ANYTHINK_DEX"
add_runtime_dex "$KOTLIN_DEX"
add_runtime_dex "$SUPPORT_DEX"
add_runtime_dex "$JOOR_DEX"
while IFS= read -r extra_dex; do
  if [[ "$(basename "$extra_dex")" == "starry_menu.dex" ]]; then
    continue
  fi
  add_runtime_dex "$extra_dex"
done < <(find "$RUNTIME_DIR" -maxdepth 1 -type f -name '*.dex' -print | LC_ALL=C sort)

printf '[input] runtime DEX order:\n'
for dex in "${RUNTIME_DEXES[@]}"; do
  printf '  %s  %s\n' "$(basename "$dex")" "$(shasum -a 256 "$dex" | awk '{print $1}')"
done

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: no Android build-tools with zipalign"; exit 1; }

WORK="$(mktemp -d /tmp/wxsj_unshelled.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK" "$OUT_DIR"

echo "[1/5] Patch StarryApp: MyApplication -> android.app.Application"
"$BAKSMALI_BIN" d "$STARRY_DEX" -o "$WORK/starry_smali"
python3 - "$WORK/starry_smali/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Lcom/netease/nis/wrapper/MyApplication;",
     ".super Landroid/app/Application;"),
    ("invoke-direct {p0}, Lcom/netease/nis/wrapper/MyApplication;-><init>()V",
     "invoke-direct {p0}, Landroid/app/Application;-><init>()V"),
    ("invoke-super {p0, p1}, Lcom/netease/nis/wrapper/MyApplication;->attachBaseContext(Landroid/content/Context;)V",
     "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V"),
    ("invoke-super {p0}, Lcom/netease/nis/wrapper/MyApplication;->onCreate()V",
     "invoke-super {p0}, Landroid/app/Application;->onCreate()V"),
]
for old, new in repls:
    if old not in t:
        raise SystemExit(f"missing expected smali pattern: {old}")
    t = t.replace(old, new)
if "Lcom/netease/nis/wrapper/MyApplication;" in t:
    raise SystemExit("shell Application reference remains in StarryApp")
p.write_text(t, encoding="utf-8")
PY
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$WORK/starry_smali"
ls -lh "$WORK/classes4.dex"

echo "[2/5] Patch binary AndroidManifest.xml"
python3 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path

src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
with zipfile.ZipFile(src) as zin, zipfile.ZipFile(dst, "w") as zout:
    for name in keep:
        item = zin.getinfo(name)
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
        zout.writestr(info, zin.read(item))
PY
"$APKTOOL_BIN" d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
python3 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")

for perm in (
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
):
    if perm not in t:
        t = t.replace(
            "<application",
            f'    <uses-permission android:name="{perm}"/>\n    <application',
            1,
        )

old_apps = (
    'android:name="com.netease.nis.wrapper.MyApplication"',
    'android:name="com.starry.magic.StarryApp"',
)
if old_apps[0] in t:
    t = t.replace(old_apps[0], 'android:name="com.starry.magic.StarryApp"', 1)
elif old_apps[1] not in t:
    t = t.replace("<application ", '<application android:name="com.starry.magic.StarryApp" ', 1)

if 'com.starry.magic.StarryInitProvider' not in t:
    provider = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:exported="false"
            android:authorities="com.xingzuodaoshi.infinityUpgrade.starryinit"/>
'''
    t = t.replace("</application>", provider + "\n    </application>", 1)

if 'com.starry.magic.menu.MenuService' not in t:
    service = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:stopWithTask="true"
            android:foregroundServiceType="specialUse">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay service"/>
        </service>
'''
    t = t.replace("</application>", service + "\n    </application>", 1)

p.write_text(t, encoding="utf-8")
PY
"$APKTOOL_BIN" b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
ls -lh "$WORK/AndroidManifest.xml"

echo "[3/5] Surgical ZIP merge"
python3 - "$ORIG_APK" "$WORK" "$WORK/classes4.dex" "$STARRY_SO" "$LIBCXX_SO" "$WORK/unsigned.apk" "${RUNTIME_DEXES[@]}" <<'PY'
import hashlib
import sys
import zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
starry = Path(sys.argv[3]).read_bytes()
so = Path(sys.argv[4]).read_bytes()
libcxx = Path(sys.argv[5]).read_bytes()
dst = Path(sys.argv[6])
runtime_paths = [Path(p) for p in sys.argv[7:]]
manifest = (work / "AndroidManifest.xml").read_bytes()

runtime_data = []
seen_hashes = set()
for path in runtime_paths:
    data = path.read_bytes()
    digest = hashlib.sha256(data).digest()
    if digest in seen_hashes:
        print(f"  deduplicated runtime DEX: {path.name}")
        continue
    seen_hashes.add(digest)
    runtime_data.append((path.name, data))

extras = {}
for index, (name, data) in enumerate(runtime_data, start=1):
    dex_name = "classes.dex" if index == 1 else f"classes{index}.dex"
    extras[dex_name] = data
    print(f"  {dex_name} <- {name}")

menu_dex_name = f"classes{len(runtime_data) + 1}.dex"
extras[menu_dex_name] = starry
extras["lib/arm64-v8a/libstarry_magic.so"] = so
extras["lib/arm64-v8a/libc++_shared.so"] = libcxx
print(f"  {menu_dex_name} <- patched Starry menu")
shell_entries = {
    "assets/shellinfo",
    "assets/na.czl",
    "assets/nedata.db",
    "lib/arm64-v8a/libnesec.so",
    "lib/arm64-v8a/libnesec-x86.so",
}

with zipfile.ZipFile(src, "r") as zin, zipfile.ZipFile(dst, "w") as zout:
    for item in zin.infolist():
        name = item.filename
        if name.startswith("META-INF/"):
            continue
        if name in shell_entries or name in extras:
            continue
        data = manifest if name == "AndroidManifest.xml" else zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
        if name == "AndroidManifest.xml":
            info.compress_type = zipfile.ZIP_DEFLATED
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)

    for name, data in extras.items():
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)

with zipfile.ZipFile(src, "r") as a, zipfile.ZipFile(dst, "r") as b:
    for name in (
        "resources.arsc",
        "lib/arm64-v8a/libil2cpp.so",
        "lib/arm64-v8a/libunity.so",
        "assets/bin/Data/Managed/Metadata/global-metadata.dat",
        "assets/gdt_plugin/gdtadv2.jar",
    ):
        if name in a.namelist() and name in b.namelist():
            ah = hashlib.sha256(a.read(name)).hexdigest()
            bh = hashlib.sha256(b.read(name)).hexdigest()
            if ah != bh:
                raise SystemExit(f"preserved entry changed: {name}")
    for name, data in extras.items():
        if name not in b.namelist():
            raise SystemExit(f"missing extra entry: {name}")
        if hashlib.sha256(b.read(name)).hexdigest() != hashlib.sha256(data).hexdigest():
            raise SystemExit(f"extra entry hash mismatch: {name}")
    for name in shell_entries:
        if name in b.namelist():
            raise SystemExit(f"shell entry still present: {name}")
    print(f"  merged entries={len(b.namelist())}")
PY

echo "[3.5/5] Runtime DEX class audit"
python3 - "$BAKSMALI_BIN" "$WORK/classes4.dex" "${RUNTIME_DEXES[@]}" <<'PY'
import collections
import subprocess
import sys
from pathlib import Path

baksmali = sys.argv[1]
dex_paths = [Path(p) for p in sys.argv[2:]]
required = {
    "Lkotlin/text/StringsKt;",
    "Lkotlinx/serialization/json/JsonKt;",
    "Lkotlinx/coroutines/Dispatchers;",
    "Lkotlinx/coroutines/android/AndroidDispatcherFactory;",
    "Lcom/taptap/sdk/kit/internal/json/TapJson;",
    "Lorg/joor/Reflect;",
    "Lcom/starry/magic/StarryApp;",
}
all_classes = set()
seen = collections.defaultdict(list)
for dex_path in dex_paths:
    output = subprocess.check_output(
        [baksmali, "list", "classes", str(dex_path)],
        text=True,
    )
    classes = [line.strip() for line in output.splitlines() if line.strip()]
    print(f"  {dex_path.name}: {len(classes)} classes")
    for class_name in classes:
        all_classes.add(class_name)
        seen[class_name].append(dex_path.name)

missing = sorted(required - all_classes)
if missing:
    raise SystemExit("missing required runtime classes: " + ", ".join(missing))

duplicates = {name: paths for name, paths in seen.items() if len(paths) > 1}
if duplicates:
    detail = "; ".join(f"{name} in {paths}" for name, paths in sorted(duplicates.items())[:20])
    raise SystemExit("duplicate runtime classes: " + detail)

print(f"  audit ok: {len(all_classes)} unique classes, no duplicate class definitions")
PY

echo "[4/5] zipalign + apksigner"
"$BT/zipalign" -p -f 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
rm -f "$OUT_APK" "$OUT_APK.idsig"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$OUT_APK" "$WORK/aligned.apk"
"$BT/apksigner" verify --verbose --print-certs "$OUT_APK"

echo "[5/5] Static checks"
"$BT/aapt2" dump xmltree "$OUT_APK" --file AndroidManifest.xml | rg -n 'android:name\(0x01010003\)="com\.starry\.magic\.(StarryApp|StarryInitProvider|menu\.MenuService)"|com\.netease\.nis'
shasum -a 256 "$OUT_APK"
ls -lh "$OUT_APK"
