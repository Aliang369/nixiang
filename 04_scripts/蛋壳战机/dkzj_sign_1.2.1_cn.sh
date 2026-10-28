#!/usr/bin/env bash
# 蛋壳战机 国服 1.2.1 (vc44, taptap) — 星光签名阶段。
#
# zipalign + apksigner（01_tools/starry_release.jks / alias starry）；
# 签完必须复核：包结构、包内 so == 构建产物、原包游戏本体逐字节不变、
# classes5.dex 含本包名映射、签名 CN=StarryMagic。
# 2026-10-28 起走 TapTap「改包法」：classes4.dex 是**故意修补**的（验签门闩恒 true / 登录恒 Web），
# 因此该条从「零改动」断言改为「== 补丁产物」；并发伪装判据反向为「manifest 不得含 StarryAppComponentFactory」。
#
# 跨机：优先用 build-tools 的 apksigner.jar（java -jar，mac/Win 同命令）。
# 用法：bash 04_scripts/蛋壳战机/dkzj_sign_1.2.1_cn.sh
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
GAME="蛋壳战机"
VERSION="1.2.1"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_starry-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_starry-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_BUILD="$NIXIANG/mod菜单/starry-mod-menu/libs_dkzj121/arm64-v8a/libstarry_magic.so"
PATCHED_DEX4="${DKZJ_PATCHED_DEX4:-$NIXIANG/07_patched/$GAME/tapsdk_patch/classes4.dex}"
ORIG_APK="${DKZJ_BASE_APK:-$NIXIANG/02_apks/$GAME/蛋壳战机_1.2.1_official_vc44_from_emulator.apk}"

md5_of() { if md5 -q /dev/null >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | cut -d' ' -f1; fi; }
sha256_of() { if shasum -a 256 /dev/null >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'; else sha256sum "$1" | cut -d' ' -f1; fi; }
PY="$(command -v python3 || command -v python)"

[ -f "$UNSIGNED" ] || { echo "ERROR: missing ${UNSIGNED}（先跑 surgical_dkzj_1.2.1_cn.sh）" >&2; exit 1; }
[ -f "$KS" ] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[ -f "$ORIG_APK" ] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }

BT=""
for c in "$HOME/Library/Android/sdk/build-tools/36.0.0" \
         "${LOCALAPPDATA:-}/Android/Sdk/build-tools/36.1.0" \
         "/c/Users/Administrator/AppData/Local/Android/Sdk/build-tools/36.1.0" \
         "${LOCALAPPDATA:-}/Android/Sdk/build-tools/35.0.0"; do
  if [ -x "$c/zipalign" ] || [ -x "$c/zipalign.exe" ]; then BT="$c"; break; fi
done
[ -n "$BT" ] || { echo "ERROR: zipalign not found (build-tools)" >&2; exit 1; }
ZIPALIGN="$BT/zipalign"; [ -x "$ZIPALIGN" ] || ZIPALIGN="$BT/zipalign.exe"
APKSIGNER_JAR="$BT/lib/apksigner.jar"
[ -f "$APKSIGNER_JAR" ] || { echo "ERROR: apksigner.jar missing in $BT/lib" >&2; exit 1; }
JAVA_BIN="${JAVA_HOME:+$JAVA_HOME/bin/java}"; [ -x "${JAVA_BIN:-}" ] || JAVA_BIN="$(command -v java)"

WORK="$(mktemp -d "${TMPDIR:-/tmp}/dkzj_sign_cn.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT
ALIGNED="$WORK/aligned.apk"

echo "[sign] zipalign"
"$ZIPALIGN" -f -p 4 "$UNSIGNED" "$ALIGNED"
rm -f "$SIGNED"
echo "[sign] apksigner"
"$JAVA_BIN" -jar "$APKSIGNER_JAR" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$JAVA_BIN" -jar "$APKSIGNER_JAR" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
"$JAVA_BIN" -jar "$APKSIGNER_JAR" verify "$SIGNED" >/dev/null \
  || { echo "ERROR: apksigner verify failed" >&2; exit 1; }

echo "[sign] 结构 + 原包零改动复核"
"$PY" - "$ORIG_APK" "$SIGNED" "$SO_BUILD" "$PATCHED_DEX4" <<'PY'
import hashlib, sys, zipfile
orig, signed, so_build = sys.argv[1], sys.argv[2], sys.argv[3]
patched4 = sys.argv[4] if len(sys.argv) > 4 else ''
# TapTap「改包法」：classes4.dex 是**故意修补**的（验签门闩/网页登录），不参与「零改动」断言；
# 改为断言包内 classes4.dex == 补丁产物。
GUARD = [
    'classes.dex', 'classes2.dex', 'classes3.dex',
    'resources.arsc', 'lib/arm64-v8a/libil2cpp.so',
    'lib/arm64-v8a/libtapsdkcore.so', 'lib/arm64-v8a/libunity.so',
    'lib/arm64-v8a/libmain.so',
    'assets/bin/Data/Managed/Metadata/global-metadata.dat',
]
with zipfile.ZipFile(orig) as a, zipfile.ZipFile(signed) as b:
    names = set(b.namelist())
    for need in ['classes.dex', 'classes2.dex', 'classes3.dex', 'classes4.dex', 'classes5.dex',
                 'resources.arsc', 'lib/arm64-v8a/libstarry_magic.so',
                 'lib/arm64-v8a/libc++_shared.so']:
        if need not in names:
            raise SystemExit(f'missing entry: {need}')
    for n in GUARD:
        ha = hashlib.sha256(a.read(n)).hexdigest()
        hb = hashlib.sha256(b.read(n)).hexdigest()
        if ha != hb:
            raise SystemExit(f'protected entry changed: {n}')
        print(f'  {n} sha256={hb[:16]}… ok')
    dex4_in = hashlib.md5(b.read('classes4.dex')).hexdigest()
    so_in = hashlib.md5(b.read('lib/arm64-v8a/libstarry_magic.so')).hexdigest()
so_build_md5 = hashlib.md5(open(so_build, 'rb').read()).hexdigest()
if so_in != so_build_md5:
    raise SystemExit(f'packaged so {so_in} != build_so {so_build_md5}')
print(f'  包内 so == 构建产物 ({so_in}) ok')
if patched4:
    p4 = hashlib.md5(open(patched4, 'rb').read()).hexdigest()
    if dex4_in != p4:
        raise SystemExit(f'packaged classes4.dex {dex4_in} != 改包法补丁 {p4}')
    print(f'  包内 classes4.dex == 改包法补丁 ({dex4_in}) ok')
print('  其余游戏本体条目零改动校验通过（classes4.dex 按改包法有意修改）')

with zipfile.ZipFile(signed) as b:
    dex = b.read('classes5.dex')
    man = b.read('AndroidManifest.xml')
if b'com.survivor.acecn' not in dex:
    raise SystemExit('classes5.dex 缺 包名签名映射 com.survivor.acecn')
if patched4:
    # 改包法（2026-10-28 起）：运行时签名伪装已停用 ⇒ 不再要求 gorilla 证书串生效，
    # 反而必须断言 manifest 没挂 StarryAppComponentFactory（旧法判据反向）。
    if b'StarryAppComponentFactory' in man:
        raise SystemExit('manifest 仍挂 StarryAppComponentFactory —— 改包法包不允许（旧运行时伪装已停用）')
    print('  classes5.dex 包名映射 ok；manifest 未挂 StarryAppComponentFactory（改包法）')
else:
    if b'MIICojCCAYqgAwIBAgIEbs5L8zAN' not in dex:
        raise SystemExit('classes5.dex 缺 gorilla 官方证书（旧签名伪装路线下必需）')
    print('  classes5.dex SignSpoof 串 ok（旧路线：含 gorilla 证书）')
PY

echo "[sign] done → $SIGNED"
echo "[sign] md5    = $(md5_of "$SIGNED")"
echo "[sign] sha256 = $(sha256_of "$SIGNED")"
ls -lh "$SIGNED"
