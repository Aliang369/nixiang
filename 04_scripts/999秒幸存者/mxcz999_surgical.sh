#!/usr/bin/env bash
# script-policy: {"purpose":"999秒!幸存者 V0.0866(vc866) 最小可用外科组装：官方底包 + 注入 classes4.dex + 补丁 AXML(StarryInitProvider/MenuService) + 替换 libstarry_magic.so + zipalign + apksigner","inputs":"02_apks/999秒幸存者/999秒幸存者_V0.0866_vc866_原版.apk、07_patched/999秒幸存者/surgical/{classes4.dex,AndroidManifest.xml}、mod菜单/starry-mod-menu/libs_mxcz999/arm64-v8a/libstarry_magic.so、01_tools/starry_release.jks","outputs":"08_output/999秒幸存者/999秒幸存者_V0.0866_vc866_starry-releaseSigned.apk","boundary":"只做 zip 外科组装；禁止 apktool b 整包；禁止改公共 Android.mk/Main.cpp/MainModern.cpp/GameConfig.h；禁止写 app/src/main/jniLibs；脚本内禁止 install/uninstall；本版不做 TapSDK 补丁","prior_art":"mod菜单/starry-mod-menu/scripts/surgical_zjmc_1.0.20261111.sh（同族：单 base.apk + 新增 classesN + 最小 Manifest + so 替换）","difference":"PATCHED_DEX_DIR 允许为空（TapSDK 改包法留待下一步）；只做最小集","evidence":"10_docs/999秒幸存者/实现_20261115.md; 10_docs/999秒幸存者/设备与底包_20261115.md"}
# ================================================================
#  999秒!幸存者 — 外科组装（最小可用包：不含 TapSDK 补丁）
#
#  形态事实：单 base.apk（无 split）/ ABI 仅 arm64-v8a / extractNativeLibs=true
#            官方 dex = classes/classes2/classes3 ⇒ 星光落 classes4.dex
#            官方签名 v2，DN "C=CN, O=halo games" ⇒ 星空包异签名，覆盖安装必失败
#
#  用法:
#    MXCZ999_SURGICAL_ALLOW=1 bash 04_scripts/999秒幸存者/mxcz999_surgical.sh
#    bash 04_scripts/999秒幸存者/mxcz999_surgical.sh --dry-run
#  可选环境变量：MXCZ999_ORIG_APK / MXCZ999_STAR_DEX / MXCZ999_MANIFEST /
#                MXCZ999_PATCHED_DEX_DIR（空=跳过 TapSDK）/ MXCZ999_OLD_SIG_LIST
# ================================================================
set -euo pipefail

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
[ -f "$NIXIANG/04_scripts/_common/env.sh" ] && source "$NIXIANG/04_scripts/_common/env.sh"

DRY_RUN=0
for a in "$@"; do
  case "$a" in
    --dry-run) DRY_RUN=1 ;;
    *) echo "ERROR: 未知参数 ${a}（只支持 --dry-run）" >&2; exit 2 ;;
  esac
done

GAME="999秒幸存者"
PKG="com.HaloGames.Survivors999"
VERSION="V0.0866"
VC="866"
TAG="Mxcz999"
MERGE_DEX="classes4.dex"
ORIG_MD5_EXPECT="9037f72d07e2e8f3f92289377ec979ef"

WORK="$NIXIANG/07_patched/$GAME/surgical"
ORIG_APK="${MXCZ999_ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_vc${VC}_原版.apk}"
SO="$NIXIANG/mod菜单/starry-mod-menu/libs_mxcz999/arm64-v8a/libstarry_magic.so"
CXX_SO="$NIXIANG/mod菜单/starry-mod-menu/libs_mxcz999/arm64-v8a/libc++_shared.so"
STAR_DEX_SRC="${MXCZ999_STAR_DEX:-$WORK/$MERGE_DEX}"
MANIFEST_PATH="${MXCZ999_MANIFEST:-$WORK/AndroidManifest.xml}"
PATCHED_DEX_DIR="${MXCZ999_PATCHED_DEX_DIR:-}"
OLD_SIG_LIST="${MXCZ999_OLD_SIG_LIST:-$WORK/old_sig_entries.txt}"
KS="$NIXIANG/01_tools/starry_release.jks"
OUT_DIR="$NIXIANG/08_output/$GAME"
SIGNED="$OUT_DIR/${GAME}_${VERSION}_vc${VC}_starry-releaseSigned.apk"
UNSIGNED="$WORK/unsigned.apk"
ALIGNED="$WORK/aligned.apk"
# aapt2：env.sh 不给 AAPT2_BIN，直接按 build-tools 定位（Windows 两份候选）
AAPT2="${NIXIANG_AAPT2:-}"
if [ -z "$AAPT2" ] && [ -n "${NIXIANG_BUILD_TOOLS:-}" ]; then
  for cand in "$NIXIANG_BUILD_TOOLS/aapt2.exe" "$NIXIANG_BUILD_TOOLS/aapt2"; do
    if [ -f "$cand" ]; then AAPT2="$cand"; break; fi
  done
fi
if [ -z "$AAPT2" ]; then
  for cand in "$NIXIANG/01_tools/android-sdk/build-tools"/*/aapt2.exe; do
    if [ -f "$cand" ]; then AAPT2="$cand"; break; fi
  done
fi

die() { echo "ERROR: $*" >&2; exit 1; }
md5f() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}' | tr -d '\\'; else md5 -q "$1" | tr -d '\\'; fi; }
sha256f() { if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}'; else shasum -a 256 "$1" | awk '{print $1}'; fi; }
sizeb() { stat -c %s "$1" 2>/dev/null || wc -c < "$1"; }

echo "[surgical] $GAME $VERSION vc$VC / $PKG / tag=$TAG"
echo "[surgical] ORIG     = $ORIG_APK"
echo "[surgical] SO       = $SO"
echo "[surgical] STAR_DEX = $STAR_DEX_SRC"
echo "[surgical] MANIFEST = $MANIFEST_PATH"
echo "[surgical] TAPSDK   = ${PATCHED_DEX_DIR:-<空：本版跳过>}"
echo "[surgical] OUT      = $SIGNED"
echo "[surgical] jniLibs  = 不读不写（属 abssnmxt 会话）"

if [ "$DRY_RUN" = "1" ]; then
  echo "[dry-run] 未组装、未签名、未写产物。"
  exit 0
fi

[ "${MXCZ999_SURGICAL_ALLOW:-0}" = "1" ] || die "需 MXCZ999_SURGICAL_ALLOW=1 才执行（防误跑）"

# ---- 0. 前置 ----
[ -f "$ORIG_APK" ] || die "缺底包: $ORIG_APK"
[ -f "$SO" ] || die "缺 so: ${SO}（先跑 mxcz999_build_so.sh）"
[ -f "$STAR_DEX_SRC" ] || die "缺星光菜单 dex: ${STAR_DEX_SRC}（先跑 mxcz999_build_dex_win.sh）"
[ -f "$MANIFEST_PATH" ] || die "缺补丁后 Manifest: ${MANIFEST_PATH}（先跑 patch_mxcz999_manifest.py）"
[ -f "$KS" ] || die "缺签名库: $KS"
[ -n "${NIXIANG_ZIPALIGN:-}" ] && [ -n "${NIXIANG_APKSIGNER:-}" ] || die "env.sh 未解析 zipalign/apksigner"
[ -n "$AAPT2" ] && [ -f "$AAPT2" ] || die "缺 aapt2（env.sh 的 AAPT2_BIN）"

mkdir -p "$OUT_DIR" "$WORK"

echo "[1/7] 底包身份核对"
GOT="$(md5f "$ORIG_APK")"
[ "$GOT" = "$ORIG_MD5_EXPECT" ] || die "底包 md5 不符: $GOT != $ORIG_MD5_EXPECT"
echo "      md5 OK ($GOT)"

echo "[2/7] 输入自检"
python3 - "$STAR_DEX_SRC" "$MERGE_DEX" <<'PY'
import sys
p, name = sys.argv[1], sys.argv[2]
b = open(p, 'rb').read()
need = [b'com/starry/magic/StarryInitProvider', b'com/starry/magic/menu/MenuService', b'com/starry/magic/bridge/ModBridge']
miss = [n.decode() for n in need if n not in b]
for n in need:
    print('      %-44s %s' % (n.decode(), 'OK' if n in b else 'MISSING'))
assert b'com/HaloGames' not in b, 'dex 含游戏类（应为纯框架类）'
assert b'Lcom/starry/magic/sign/SignSpoof;' not in b, 'dex 含已删除的 SignSpoof'
print('      size=%d (%s)' % (len(b), name))
if miss: sys.exit(1)
PY

if [ -n "$PATCHED_DEX_DIR" ]; then
  echo "[3/7] TapSDK 补丁 dex: $PATCHED_DEX_DIR"
  [ -f "$PATCHED_DEX_DIR/classes2.dex" ] || die "缺 $PATCHED_DEX_DIR/classes2.dex"
  [ -f "$PATCHED_DEX_DIR/classes3.dex" ] || die "缺 $PATCHED_DEX_DIR/classes3.dex"
else
  echo "[3/7] TapSDK 补丁：skipped（本版不做，MXCZ999_PATCHED_DEX_DIR 为空）"
fi

if [ ! -f "$OLD_SIG_LIST" ]; then
  : > "$OLD_SIG_LIST"
  echo "[4/7] 旧签名名单：底包无 META-INF/*.SF|*.RSA|*.MF（v2 签名在 APK Signing Block，非 zip 条目）⇒ 空名单"
else
  echo "[4/7] 旧签名名单：$OLD_SIG_LIST"
fi

echo "[5/7] zip 外科置换 → $UNSIGNED"
python3 - "$ORIG_APK" "$UNSIGNED" "$SO" "$MERGE_DEX" "$STAR_DEX_SRC" "$MANIFEST_PATH" "$OLD_SIG_LIST" "$PATCHED_DEX_DIR" "$CXX_SO" <<'PY'
import sys, zipfile
orig, dst, so_path, merge, star_dex, manifest, oldsig = sys.argv[1:8]
patch_dir = sys.argv[8] if len(sys.argv) > 8 else ""
repl = {
    "lib/arm64-v8a/libstarry_magic.so": open(so_path, "rb").read(),
    "lib/arm64-v8a/libc++_shared.so": open(sys.argv[9], "rb").read(),
    merge: open(star_dex, "rb").read(),
    "AndroidManifest.xml": open(manifest, "rb").read(),
}
if patch_dir:
    repl["classes2.dex"] = open(patch_dir + "/classes2.dex", "rb").read()
    repl["classes3.dex"] = open(patch_dir + "/classes3.dex", "rb").read()
drop = set(l.strip() for l in open(oldsig, encoding="utf-8") if l.strip())
z = zipfile.ZipFile(orig)
zo = zipfile.ZipFile(dst, "w", zipfile.ZIP_DEFLATED)
written = set()
for it in z.infolist():
    if it.filename in drop:
        continue
    data = repl.get(it.filename)
    if data is not None:
        zo.writestr(it, data); written.add(it.filename)
    else:
        zo.writestr(it, z.read(it.filename))
for name, data in repl.items():
    if name not in written and name not in z.namelist():
        zo.writestr(name, data); written.add(name)
zo.close()
print("      替换/新增 %d 项；产物条目 %d" % (len(written), len(zipfile.ZipFile(dst).namelist())))
PY

echo "[6/7] zipalign + apksigner → $SIGNED"
"$NIXIANG_ZIPALIGN" -f -p 4 "$UNSIGNED" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
# 显式开 v1(关)/v2(开)/v3(开)：底包是 v2（DN C=CN, O=halo games）；apksigner 默认值随 minSdk 变化，
# 实测不显式指定时只落 v3 ⇒ 与底包方案不一致（老设备/部分校验器只认 v2）。
"$NIXIANG_APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$ALIGNED"
"$NIXIANG_ZIPALIGN" -c -p 4 "$SIGNED"
"$NIXIANG_APKSIGNER" verify --print-certs "$SIGNED" 2>&1 | head -6

echo "[7/7] 产物复验"
SO_SRC_MD5="$(md5f "$SO")"
DEX_SRC_MD5="$(md5f "$STAR_DEX_SRC")"
python3 - "$SIGNED" "$MERGE_DEX" "$TAG" "$SO_SRC_MD5" "$DEX_SRC_MD5" <<'PY'
import sys, zipfile, hashlib
apk, merge, tag, so_md5, dex_md5 = sys.argv[1:6]
z = zipfile.ZipFile(apk)
names = z.namelist()
assert merge in names, "产物缺 " + merge
so = z.read("lib/arm64-v8a/libstarry_magic.so")
d = z.read(merge)
assert hashlib.md5(so).hexdigest() == so_md5, "包内 so md5 != 源 so"
assert hashlib.md5(d).hexdigest() == dex_md5, "包内 dex md5 != 源 dex"
assert ("[" + tag + "]").encode() in so, "so 缺本游戏标签"
assert b"[Abssnmxt]" not in so, "so 含别人会话标签"
print("      包内 so/dex md5 与源一致；so 标签 [%s] OK" % tag)
PY
"$AAPT2" dump xmltree "$SIGNED" --file AndroidManifest.xml 2>/dev/null | grep -q "StarryInitProvider" || die "aapt2 复读不到 StarryInitProvider"
echo "      aapt2: StarryInitProvider 在 Manifest 中"
"$AAPT2" dump xmltree "$SIGNED" --file AndroidManifest.xml 2>/dev/null | grep -q "StarryInitProvider\|MenuService" || die "aapt2 复读不到 MenuService"
echo "      aapt2: MenuService 在 Manifest 中"

rm -f "$UNSIGNED" "$ALIGNED"
echo "[surgical] size   = $(sizeb "$SIGNED") B"
echo "[surgical] md5    = $(md5f "$SIGNED")"
echo "[surgical] sha256 = $(sha256f "$SIGNED")"
echo "[surgical] DONE $SIGNED"
echo "[surgical] 提醒：异签名包，覆盖安装必失败；装机需授权卸载；本脚本内不 install。"
