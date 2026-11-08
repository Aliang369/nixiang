#!/usr/bin/env bash
# 动物大联盟 1.0.25 · 无壳包 + 星光菜单（只出包，不装机）
#
# 基座＝已实测能进 Cocos 的无壳包 shellless2（64,888,873B / 7f1b1ad2…cd18），本脚本只加一个变量：
#   ★ dex 连续到 classes5.dex（classes5 = 菜单 dex，com/starry/magic/** + com/starry/magic/dwdlm/**）
#   ★ lib/arm64-v8a 加 libstarry_magic.so（env 档）+ 其 NEEDED 依赖 libc++_shared.so（官方包/无壳包都无）
#   ★ Manifest 加 ContentProvider 注入点 com.starry.magic.StarryInitProvider + MenuService + 3 权限
#     **不加 appComponentFactory**（无壳包 Application=BingoApplication，挂工厂会断启动链）
#   其余 zip 条目逐字节透传（由 shellless_assemble_1.0.25.py 断言）。
#
# 纪律：本轮只构建 —— 无 adb / 无 install / 无 force-stop / 不截图。
#
# 用法：
#   bash 04_scripts/动物大联盟/shellless_menu_build_1.0.25.sh --dry-run
#   bash 04_scripts/动物大联盟/shellless_menu_build_1.0.25.sh
set -euo pipefail

MODE="build"
for a in "$@"; do
  case "$a" in
    --dry-run) MODE="dry" ;;
    -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $a" >&2; exit 2 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="动物大联盟"
VER="1.0.25"
PKG="com.fightingstars.games.animal"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
APKTOOL_JAR="$ROOT/01_tools/apktool/apktool.jar"
KS="$ROOT/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
OFFICIAL="$ROOT/02_apks/$GAME/${GAME}_${VER}_official.apk"
BASE="$ROOT/08_output/$GAME/${GAME}_${VER}_shellless2-releaseSigned.apk"
MENU_DEX="$SCRIPT_DIR/build/menu_dex/classes5.dex"
SO_PROFILE="${DWDLM_SO_PROFILE:-env}"
SO="$ROOT/mod菜单/starry-mod-menu/libs_dwdlm/$SO_PROFILE/arm64-v8a/libstarry_magic.so"
OUT_DIR="$ROOT/08_output/$GAME"
TAG="${GAME}_${VER}_shellless-menu"
UNSIGNED="$OUT_DIR/${TAG}-unsigned.apk"
SIGNED="$OUT_DIR/${TAG}-releaseSigned.apk"
REPORT="$SCRIPT_DIR/shellless_menu_build.json"
EXPECT_OFFICIAL_SHA="2c9e85451b41b3bcb38148e488e28c85bcfba005db9c37743c926c0363c4f539"
EXPECT_BASE_SHA="7f1b1ad2fa9c002e068b32dce30e6f57b9e678ba1901edc4aa10e2d73337cd18"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
NDK_CXX=""
for cand in "$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so" \
            "$NDK/toolchains/llvm/prebuilt/darwin-aarch64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"; do
  if [[ -f "$cand" ]]; then NDK_CXX="$cand"; break; fi
done

say() { printf "[menu_build] %s\n" "$*"; }
die() { printf "ERROR: %s\n" "$*" >&2; exit 1; }

# ---- 0. 静态预检 -----------------------------------------------------------
[[ -f "$OFFICIAL" ]] || die "缺底包 $OFFICIAL"
[[ "$(shasum -a 256 "$OFFICIAL" | awk '{print $1}')" == "$EXPECT_OFFICIAL_SHA" ]] || die "底包 sha256 与登记值不符"
[[ -f "$BASE" ]] || die "缺无壳基座 ${BASE}（先跑 shellless_assemble_1.0.25.py 那条链）"
BASE_SHA="$(shasum -a 256 "$BASE" | awk '{print $1}')"
[[ "$BASE_SHA" == "$EXPECT_BASE_SHA" ]] || die "无壳基座 sha256=$BASE_SHA 与登记值不符（基座被替换？）"
[[ -f "$MENU_DEX" ]] || die "缺菜单 dex ${MENU_DEX}（先跑 build_menu_dex_dwdlm.sh）"
LC_ALL=C grep -aq "Lcom/starry/magic/StarryInitProvider;" "$MENU_DEX" || die "菜单 dex 缺 StarryInitProvider"
if LC_ALL=C grep -aq "SignSpoof" "$MENU_DEX"; then die "菜单 dex 含 SignSpoof（禁止复活）"; fi
[[ -f "$SO" ]] || die "缺 so ${SO}（先跑 dwdlm_build_so.sh profile=${SO_PROFILE}）"
LC_ALL=C grep -aq "dwdlm" "$SO" || die "so 无 dwdlm 标签"
if [[ "$SO_PROFILE" == "env" ]]; then
  if LC_ALL=C grep -aq "starry_dwdlm_v1" "$SO"; then die "env so 含玩法补丁标记（应编译期剔除）"; fi
  LC_ALL=C grep -aq "DWDLM_ENV_ONLY" "$SO" || die "env so 缺 DWDLM_ENV_ONLY 横幅"
fi
[[ -f "$KS" ]] || die "缺签名库 $KS"
[[ -f "$APKTOOL_JAR" ]] || die "缺 apktool $APKTOOL_JAR"
[[ -n "$BT" ]] || die "缺 build-tools（zipalign/apksigner）"
[[ -n "$NDK_CXX" ]] || die "缺 NDK libc++_shared.so（菜单 so 的 NEEDED，包内必须自带）"

SO_SHA="$(shasum -a 256 "$SO" | awk '{print $1}')"
SO_MD5="$(md5 -q "$SO")"
CXX_SHA="$(shasum -a 256 "$NDK_CXX" | awk '{print $1}')"
MENU_SHA="$(shasum -a 256 "$MENU_DEX" | awk '{print $1}')"
say "official $(printf %s "$EXPECT_OFFICIAL_SHA" | cut -c1-16)…  base(shellless2) $(printf %s "$BASE_SHA" | cut -c1-16)…"
say "menu dex $(basename "$MENU_DEX") $(stat -f '%z' "$MENU_DEX")B sha256=$(printf %s "$MENU_SHA" | cut -c1-16)…"
say "so profile=$SO_PROFILE md5=$SO_MD5 sha256=$(printf %s "$SO_SHA" | cut -c1-16)…"
say "cxx $(basename "$NDK_CXX") sha256=$(printf %s "$CXX_SHA" | cut -c1-16)…"

if [[ "$MODE" == "dry" ]]; then
  say "[dry-run] 无写入。若执行将产出："
  say "  unsigned : $UNSIGNED"
  say "  signed   : $SIGNED"
  say "  dex      : classes.dex..classes4.dex（透传 shellless2 的 4 份）+ classes5.dex（菜单）"
  say "  so       : libstarry_magic.so + libc++_shared.so → lib/arm64-v8a/"
  say "  manifest : provider com.starry.magic.StarryInitProvider（${PKG}.starryinit, initOrder=2147483647）"
  say "             + service com.starry.magic.menu.MenuService + 3 权限；**无 appComponentFactory**"
  say "[dry-run] 静态预检全过"
  exit 0
fi

LOCK="/tmp/dwdlm_shellless_menu.lockdir"
mkdir "$LOCK" 2>/dev/null || die "lock held: $LOCK"
echo $$ > "$LOCK/pid"
WORK="$(mktemp -d /tmp/dwdlm_shellless_menu.XXXXXX)"
cleanup() { rm -rf "$WORK"; rm -rf "$LOCK"; }
trap cleanup EXIT
mkdir -p "$OUT_DIR"

# ---- 1. 从无壳基座取 4 份 dex（字节透传，不重编）---------------------------
say "[1/7] 从无壳基座提取 classes.dex..classes4.dex（逐字节）"
for n in classes.dex classes2.dex classes3.dex classes4.dex; do
  unzip -p "$BASE" "$n" > "$WORK/$n" || die "基座取不到 $n"
  [[ -s "$WORK/$n" ]] || die "基座 $n 为空"
  say "  $n $(stat -f '%z' "$WORK/$n")B sha256=$(shasum -a 256 "$WORK/$n" | cut -c1-16)…"
done

# ---- 2. mini.apk（只在 Manifest 改造时需要，arsc 用官方原件）--------------
say "[2/7] 造 mini.apk（AndroidManifest.xml + resources.arsc + classes.dex）"
python3 - "$OFFICIAL" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in keep:
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, zin.read(name))
zin.close(); zout.close()
print("  mini apk %d B" % dst.stat().st_size)
PY

# ---- 3. Manifest：无壳改造 → 菜单注入点 -----------------------------------
say "[3/7] apktool d -s + shellless_manifest_edit + shellless_menu_manifest_edit"
java -jar "$APKTOOL_JAR" d -f -s -o "$WORK/dec" "$WORK/mini.apk" >/dev/null
python3 "$SCRIPT_DIR/shellless_manifest_edit_1.0.25.py" "$WORK/dec/AndroidManifest.xml" > "$WORK/mf_step1.json"
python3 "$SCRIPT_DIR/shellless_menu_manifest_edit_1.0.25.py" "$WORK/dec/AndroidManifest.xml" "$PKG" > "$WORK/mf_step2.json"
cat "$WORK/mf_step1.json" "$WORK/mf_step2.json"

# ---- 4. apktool b → 只取回 AndroidManifest.xml + AXML 复核 -----------------
say "[4/7] apktool b → 只取 AndroidManifest.xml（arsc 用官方原件）"
java -jar "$APKTOOL_JAR" b -f "$WORK/dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
[[ -s "$WORK/AndroidManifest.xml" ]] || die "Manifest 重建失败"
python3 - "$WORK/AndroidManifest.xml" "$PKG" <<'PY'
import sys
from pathlib import Path
data = Path(sys.argv[1]).read_bytes(); pkg = sys.argv[2]
assert data[:4] == b"\x03\x00\x08\x00", "不是真 AXML（头应为 03000800）"
text = data.decode("utf-16-le", "ignore")
need = [("com.starry.magic.StarryInitProvider", "注入 Provider"),
        ("com.starry.magic.menu.MenuService", "菜单 Service"),
        ("com.bingo.cocos.bridge.BingoApplication", "无壳 Application"),
        ("org.cocos2dx.javascript.AppActivity", "启动 Activity"),
        (pkg + ".starryinit", "provider authority"),
        ("android.intent.category.LAUNCHER", "LAUNCHER category")]
for needle, what in need:
    assert needle in text, "二进制 Manifest 缺 " + what + "（" + needle + "）"
for bad, what in [("dev.mars.secure", "壳类"), ("appComponentFactory", "工厂属性"), ("SignSpoof", "签名伪装")]:
    assert bad not in text, "二进制 Manifest 含 " + what
print("  AXML 复核 ok：关键字符串齐全，无 dev.mars.secure / appComponentFactory / SignSpoof")
PY

# ---- 5. 外科 zip 组装（官方条目逐字节透传 + dex 连续 + 两个 so）-----------
say "[5/7] shellless_assemble（透传断言内置）"
SHELLLESS_EXTRA_SO="$SO:$NDK_CXX" python3 "$SCRIPT_DIR/shellless_assemble_1.0.25.py" \
  "$OFFICIAL" "$WORK/AndroidManifest.xml" "$WORK/classes.dex" "$WORK/classes2.dex" "$WORK/classes3.dex" \
  "$ROOT/06_dumped/$GAME/env_1.0.25/code/raw/libcocos2djs.so" "$UNSIGNED" \
  "$WORK/classes4.dex" "$MENU_DEX" > "$WORK/assemble.json" || die "组装失败"
cat "$WORK/assemble.json"

# ---- 6. zipalign -p 4 + apksigner -----------------------------------------
say "[6/7] zipalign -f -p 4 + apksigner（starry_release / ${KS_ALIAS}）"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -6

# ---- 7. 摘要 ---------------------------------------------------------------
FINAL_SHA="$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
FINAL_SIZE="$(stat -f '%z' "$SIGNED")"
say "[7/7] OK $SIGNED"
say "        size=$FINAL_SIZE sha256=$FINAL_SHA"
python3 - "$REPORT" "$SIGNED" "$FINAL_SHA" "$FINAL_SIZE" "$BASE_SHA" "$MENU_SHA" "$SO_SHA" "$SO_MD5" "$CXX_SHA" "$SO_PROFILE" <<'PY'
import json, sys
report, apk, sha, size, base_sha, menu_sha, so_sha, so_md5, cxx_sha, prof = sys.argv[1:11]
json.dump({"apk": apk, "apk_sha256": sha, "apk_size": int(size), "base_shellless2_sha256": base_sha,
           "menu_dex_sha256": menu_sha, "so_profile": prof, "so_sha256": so_sha, "so_md5": so_md5,
           "libcxx_shared_sha256": cxx_sha},
          open(report, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
print("  report:", report)
PY
say "下一步（不装机）：自检 python3 04_scripts/动物大联盟/shellless_menu_verify_1.0.25.py <signed.apk>"
