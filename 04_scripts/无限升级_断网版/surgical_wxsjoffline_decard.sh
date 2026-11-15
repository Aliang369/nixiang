#!/usr/bin/env bash
# 无限升级 断网版 12.10701 (com.xingzuodaoshi.infinityOffline) —— 去第三方卡密闸门（外科式出包）
#
# 底包：02_apks/无限升级_12.10701_断网版.apk（他人改包，只读；不原地改）
# 方案：B —— 只重写第三方 classes3.dex 里 GateActivity.onCreate 为「直通 UnityPlayerActivity」。
#   * AndroidManifest.xml 不动（launcher 仍是 GateActivity，exported/主题/intent-filter 全不变）
#   * 官方 classes.dex / resources.arsc / lib/arm64-v8a/libil2cpp.so 逐字节透传
#   * GateClient 与 infinity-activated:// 方案原样保留（外部触发仍能拉起 Unity）
# 选型与风险见 10_docs/无限升级/去卡密_断网版_20261115.md
#
# 用法：bash 04_scripts/无限升级_断网版/surgical_wxsjoffline_decard.sh
# 产物：07_patched/无限升级_断网版/无限升级_12.10701_断网版_去卡密.apk
# 可重复执行：每次重建中间目录并整份重写产物。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "${SCRIPT_DIR}/../.." && pwd)"

BASE_APK="${NIXIANG}/02_apks/无限升级_12.10701_断网版.apk"
WORK="${NIXIANG}/06_dumped/无限升级_断网版/decard"
DEC_DIR="${WORK}/apktool_dec"
SMALI3="${DEC_DIR}/smali_classes3"
GATE_SMALI="${SMALI3}/local/privategame/infinity/GateActivity.smali"
CLIENT_SMALI="${SMALI3}/local/privategame/infinity/GateClient.smali"
TOOLS_SRC="${SCRIPT_DIR}/AsmDex.java"
TOOLS_OUT="${WORK}/tools"
NEW_DEX="${WORK}/classes3_decard.dex"
OUT_DIR="${NIXIANG}/07_patched/无限升级_断网版"
OUT_APK="${OUT_DIR}/无限升级_12.10701_断网版_去卡密.apk"
UNSIGNED_APK="${WORK}/assembled_unsigned.apk"
ALIGNED_APK="${WORK}/assembled_aligned.apk"

# 底包指纹（2026-11-15 实测；不匹配即停，禁止换包硬跑）
BASE_MD5="0e2fe95ccd6f27e98a417ad1cb47228c"
BASE_SHA256="f9f580b081d63d095867d22657ce1836e8fb601744bb7d52d4ab8b11ea173bf8"
# 底包内第三方 classes3.dex 指纹（GateActivity + GateClient）
SRC_CLASSES3_MD5="ac2393ba68cd6260b7231ccf284bbc76"

APKTOOL_BIN="${APKTOOL_BIN:-/opt/homebrew/bin/apktool}"
KS="${NIXIANG}/01_tools/starry_release.jks"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"

log() { printf '%s\n' "$*"; }
die() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }
sha256_of() { shasum -a 256 "$1" | awk '{print $1}'; }

# ---------- 0. 工具定位 ----------
[[ -f "${BASE_APK}" ]] || die "missing base APK: ${BASE_APK}"
[[ -f "${TOOLS_SRC}" ]] || die "missing assembler source: ${TOOLS_SRC}"
[[ -x "${APKTOOL_BIN}" ]] || die "missing apktool: ${APKTOOL_BIN}"
[[ -f "${KS}" ]] || die "missing signing key: ${KS}"

APKTOOL_JAR="${APKTOOL_JAR_PATH:-}"
if [[ -z "${APKTOOL_JAR}" ]]; then
  for cand in /opt/homebrew/Cellar/apktool/*/libexec/apktool_*.jar \
              /opt/homebrew/share/apktool/apktool_*.jar \
              /usr/local/Cellar/apktool/*/libexec/apktool_*.jar; do
    if [[ -f "${cand}" ]]; then APKTOOL_JAR="${cand}"; fi
  done
fi
[[ -n "${APKTOOL_JAR}" ]] || die "cannot locate apktool jar (set APKTOOL_JAR_PATH)"
JAVA_BIN="java"
if [[ -n "${JAVA_HOME:-}" ]]; then JAVA_BIN="${JAVA_HOME}/bin/java"; fi
command -v "${JAVA_BIN}" >/dev/null 2>&1 || die "java not found"
JAVAC_BIN="javac"
if [[ -n "${JAVA_HOME:-}" ]]; then JAVAC_BIN="${JAVA_HOME}/bin/javac"; fi
command -v "${JAVAC_BIN}" >/dev/null 2>&1 || die "javac not found"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "${HOME}/Library/Android/sdk/build-tools/${v}/zipalign" ]]; then
    BT="${HOME}/Library/Android/sdk/build-tools/${v}"
    break
  fi
done
[[ -n "${BT}" ]] || die "no Android build-tools with zipalign"

mkdir -p "${OUT_DIR}"

# ---------- 1. 底包指纹 ----------
log "[1/7] verify base APK fingerprints"
got_md5="$(md5 -q "${BASE_APK}")"
got_sha="$(sha256_of "${BASE_APK}")"
[[ "${got_md5}" == "${BASE_MD5}" ]] || die "base APK md5 mismatch: ${got_md5} != ${BASE_MD5}"
[[ "${got_sha}" == "${BASE_SHA256}" ]] || die "base APK sha256 mismatch"
log "  md5    = ${got_md5}"
log "  sha256 = ${got_sha}"

# ---------- 2. 解码（只要 smali） ----------
log "[2/7] decode sources (apktool d -r --no-assets)"
rm -rf "${DEC_DIR}"
"${APKTOOL_BIN}" d -r --no-assets -f -o "${DEC_DIR}" "${BASE_APK}" >/dev/null
[[ -f "${GATE_SMALI}" ]] || die "GateActivity.smali not found after decode"
[[ -f "${CLIENT_SMALI}" ]] || die "GateClient.smali not found after decode"

src_dex_md5="$(unzip -p "${BASE_APK}" classes3.dex | md5 -q)"
[[ "${src_dex_md5}" == "${SRC_CLASSES3_MD5}" ]] || die "classes3.dex md5 mismatch: ${src_dex_md5}"
log "  classes3.dex md5 = ${src_dex_md5} (third-party gate dex, will be replaced)"

# ---------- 3. GateClient 必须保持原样 ----------
log "[3/7] assert GateClient untouched (scheme router stays as-is)"
grep -q 'infinity-activated://launch' "${CLIENT_SMALI}" || die "GateClient lost infinity-activated scheme"
grep -q 'Lcom/unity3d/player/UnityPlayerActivity;' "${CLIENT_SMALI}" || die "GateClient lost Unity target"
log "  scheme + Unity target present"

# ---------- 4. 重写 GateActivity.onCreate 为直通 ----------
log "[4/7] patch GateActivity.onCreate -> direct launch"
cat > "${GATE_SMALI}" <<'SMALI'
.class public Llocal/privategame/infinity/GateActivity;
.super Landroid/app/Activity;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Landroid/app/Activity;-><init>()V

    return-void
.end method


# virtual methods
# 去卡密版：不创建 WebView、不加载激活页，onCreate 直接拉起 Unity，
# 等价于原版激活成功回调（GateClient.shouldOverrideUrlLoading 里的 startActivity + finish）。
.method protected onCreate(Landroid/os/Bundle;)V
    .locals 3

    invoke-super {p0, p1}, Landroid/app/Activity;->onCreate(Landroid/os/Bundle;)V

    new-instance v0, Landroid/content/Intent;

    const-class v1, Lcom/unity3d/player/UnityPlayerActivity;

    invoke-direct {v0, p0, v1}, Landroid/content/Intent;-><init>(Landroid/content/Context;Ljava/lang/Class;)V

    invoke-virtual {p0, v0}, Landroid/app/Activity;->startActivity(Landroid/content/Intent;)V

    invoke-virtual {p0}, Landroid/app/Activity;->finish()V

    return-void
.end method
SMALI
grep -q 'UnityPlayerActivity' "${GATE_SMALI}" || die "patched smali missing Unity target"

# ---------- 5. 组装 classes3.dex（dex 040） ----------
log "[5/7] assemble classes3.dex (api 34 -> dex 040)"
rm -rf "${TOOLS_OUT}"
mkdir -p "${TOOLS_OUT}"
"${JAVAC_BIN}" -cp "${APKTOOL_JAR}" -d "${TOOLS_OUT}" "${TOOLS_SRC}"
rm -f "${NEW_DEX}"
"${JAVA_BIN}" -cp "${APKTOOL_JAR}:${TOOLS_OUT}" AsmDex "${SMALI3}" "${NEW_DEX}" 34

dex_magic="$(xxd -p -l 8 "${NEW_DEX}")"
[[ "${dex_magic}" == "6465780a30343000" ]] || die "unexpected dex header: ${dex_magic} (want dex 040)"
new_dex_classes="$("${BT}/dexdump" -f "${NEW_DEX}" 2>/dev/null | grep -c 'Class descriptor' || true)"
[[ "${new_dex_classes}" == "2" ]] || die "unexpected class count: ${new_dex_classes} (want 2)"
if strings -a "${NEW_DEX}" | grep -q 'infinity_gate'; then die "patched dex still references gate asset"; fi
strings -a "${NEW_DEX}" | grep -q 'Lcom/unity3d/player/UnityPlayerActivity;' || die "patched dex missing Unity class ref"
log "  dex 040, classes=${new_dex_classes}, no gate asset reference"
log "  new classes3.dex md5 = $(md5 -q "${NEW_DEX}")"

# ---------- 6. 外科式 zip 组装 ----------
log "[6/7] surgical zip assembly (pass-through everything else)"
rm -f "${UNSIGNED_APK}"
python3 - "${BASE_APK}" "${UNSIGNED_APK}" "${NEW_DEX}" <<'PY'
import hashlib
import sys
import zipfile

src, dst, new_dex_path = sys.argv[1], sys.argv[2], sys.argv[3]
new_dex = open(new_dex_path, "rb").read()

PRESERVE = (
    "AndroidManifest.xml",
    "classes.dex",
    "classes2.dex",
    "resources.arsc",
    "lib/arm64-v8a/libil2cpp.so",
    "lib/arm64-v8a/libunity.so",
    "lib/arm64-v8a/libthemis.so",
    "assets/nedata.db",
    "assets/na.czl",
    "assets/shellinfo",
    "assets/infinity_gate.html",
    "assets/infinity_gate.js",
    "assets/bin/Data/Managed/Metadata/global-metadata.dat",
)

dropped_sig = []
replaced = []

with zipfile.ZipFile(src, "r") as zin, zipfile.ZipFile(dst, "w", zipfile.ZIP_DEFLATED) as zout:
    for item in zin.infolist():
        name = item.filename
        if name.startswith("META-INF/") and name.upper().endswith((".SF", ".RSA", ".DSA", ".MF", ".EC")):
            dropped_sig.append(name)
            continue
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        if name == "classes3.dex":
            zout.writestr(info, new_dex)
            replaced.append(name)
            continue
        zout.writestr(info, zin.read(item))

with zipfile.ZipFile(src, "r") as a, zipfile.ZipFile(dst, "r") as b:
    for name in PRESERVE:
        if name not in a.namelist():
            raise SystemExit("base APK missing expected entry: " + name)
        if name not in b.namelist():
            raise SystemExit("output APK missing entry: " + name)
        ha = hashlib.sha256(a.read(name)).hexdigest()
        hb = hashlib.sha256(b.read(name)).hexdigest()
        if ha != hb:
            raise SystemExit("pass-through entry changed: " + name)
    if hashlib.sha256(b.read("classes3.dex")).hexdigest() == hashlib.sha256(a.read("classes3.dex")).hexdigest():
        raise SystemExit("classes3.dex was not replaced")
    print("  pass-through verified: %d entries byte-identical" % len(PRESERVE))
    print("  replaced: " + ", ".join(replaced))
    print("  dropped old v1 signature files: %d" % len(dropped_sig))
    print("  output entries: %d (base %d)" % (len(b.namelist()), len(a.namelist())))
PY

# ---------- 7. zipalign + 签名 ----------
log "[7/7] zipalign + apksigner"
rm -f "${ALIGNED_APK}" "${OUT_APK}" "${OUT_APK}.idsig"
"${BT}/zipalign" -p -f 4 "${UNSIGNED_APK}" "${ALIGNED_APK}"
"${BT}/apksigner" sign \
  --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "${OUT_APK}" "${ALIGNED_APK}"
"${BT}/apksigner" verify --verbose --print-certs "${OUT_APK}" | head -20

log ""
log "================ 产物 ================"
log "apk    : ${OUT_APK}"
log "size   : $(ls -l "${OUT_APK}" | awk '{print $5}') bytes"
log "md5    : $(md5 -q "${OUT_APK}")"
log "sha256 : $(sha256_of "${OUT_APK}")"
log ""
log "改动清单："
log "  替换: classes3.dex  (第三方卡密闸门 dex -> 直通版)"
log "  新增: 无"
log "  删除: META-INF/*.SF|*.RSA|*.DSA|*.EC|*.MF（旧签名，重签后由 apksigner 重新生成）"
log "  透传: AndroidManifest.xml / classes.dex / classes2.dex / resources.arsc"
log "        lib/arm64-v8a/libil2cpp.so / libunity.so / libthemis.so"
log "        assets/nedata.db / na.czl / shellinfo / infinity_gate.html / infinity_gate.js"
log "        assets/bin/Data/Managed/Metadata/global-metadata.dat 及全部其它条目"
log ""
log "manifest 断言（应与底包一致，launcher 仍是 GateActivity）："
"${BT}/aapt2" dump xmltree "${OUT_APK}" --file AndroidManifest.xml \
  | grep -nE 'privategame.infinity.GateActivity|unity3d.player.UnityPlayerActivity' || true
