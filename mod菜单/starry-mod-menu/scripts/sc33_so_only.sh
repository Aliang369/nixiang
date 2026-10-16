#!/usr/bin/env bash
# 生存33天 — so-only 重出包（底包须已是可用外科成品，只换 libstarry_magic.so）
#
# 2026-10-17 重写（原 1.0.23 版脚本保留过 dex 更新步骤，本作 dex 未变故去掉）：
#   - 构建改走私有 mk Android_sc33.mk（modern 注册表入口），
#     不再临时切换公共 Android.mk（Main.cpp 用 __LINE__ 做 OBFUSCATE 种子，改不得）
#   - zip 外科改 python 逐条零漂移断言（旧 `zip -u` 无法证明其余条目未变）
#   - 官方 1.0.24 底包与旧 08_output 均已按「APK 不留本地」清理，
#     故本脚本的「底包」= 既有已签星光包；设备侧留档
#     /sdcard/Download/123云盘/生存33天_1.0.24.apk（MD5 5042c1eb…，会自动拉取）
#
# 用法:
#   bash scripts/sc33_so_only.sh                    # 构建 + 组包 + 签名
#   STARRY_INSTALL=1 bash scripts/sc33_so_only.sh   # 再覆盖安装
#   STARRY_TRACE=1 bash scripts/sc33_so_only.sh     # 锚点 Trace（只 dump，不拼接）
#   SC33_BASE_APK=/path/base.apk bash scripts/sc33_so_only.sh
#   SKIP_BUILD=1 bash scripts/sc33_so_only.sh       # 复用已有 so
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NX="$(cd "$ROOT/../.." && pwd)"
PKG=com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive
OUT_DIR="$NX/08_output/生存33天"
JNI="$ROOT/app/src/main/jni"
SO="$ROOT/app/src/libs_sc33/arm64-v8a/libstarry_magic.so"
DEV_BASE="/sdcard/Download/123云盘/生存33天_1.0.24.apk"
BASE_MD5_EXPECT="5042c1eb7eb3d888524b590d8083e0ba"
NDK="${NDK:-$(ls -d "$HOME/Library/Android/sdk/ndk/"* 2>/dev/null | sort -V | tail -1)}"
TRACE="${STARRY_TRACE:-0}"
TAG="release"
[[ "$TRACE" == "1" ]] && TAG="trace-release"
SIGNED="$OUT_DIR/生存33天_1.0.24_hotfix1_surgical-${TAG}Signed.apk"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools (zipalign/apksigner)" >&2; exit 1; }

WORKDIR="$(mktemp -d /tmp/sc33_so_only.XXXXXX)"
trap 'rm -rf "$WORKDIR"' EXIT

# ---- 底包 ----
BASE_APK="${SC33_BASE_APK:-$OUT_DIR/生存33天_1.0.24_surgical-releaseSigned.apk}"
if [[ ! -f "$BASE_APK" ]]; then
  echo "[0] 本地无底包 → 从设备拉 $DEV_BASE"
  adb exec-out "su -c \"cat '$DEV_BASE'\"" > "$WORKDIR/base.apk"
  BASE_APK="$WORKDIR/base.apk"
fi
BASE_MD5="$(md5 -q "$BASE_APK")"
echo "[0] 底包 $BASE_APK"
echo "    MD5=$BASE_MD5 size=$(stat -f%z "$BASE_APK")"
if [[ "$BASE_MD5" != "$BASE_MD5_EXPECT" && "${STARRY_ALLOW_ANY_BASE:-0}" != "1" ]]; then
  echo "ERROR: 底包 MD5 与历史正式包不符（期望 ${BASE_MD5_EXPECT}）；确认渠道/版本无误再设 STARRY_ALLOW_ANY_BASE=1" >&2
  exit 1
fi

# ---- so ----
if [[ "${SKIP_BUILD:-0}" == "1" ]]; then
  echo "[1] SKIP_BUILD=1，复用 $SO"
else
  echo "[1] ndk-build Android_sc33.mk (STARRY_TRACE=$TRACE)"
  [[ -x "$NDK/ndk-build" ]] || { echo "ERROR: 缺少 $NDK/ndk-build" >&2; exit 1; }
  NDK_FLAGS=(NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_sc33.mk
             NDK_APPLICATION_MK=./Application.mk
             NDK_OUT=../../obj_sc33 NDK_LIBS_OUT=../../libs_sc33 -j8)
  if [[ "$TRACE" == "1" ]]; then
    NDK_FLAGS+=(STARRY_TRACE=1)
  else
    NDK_FLAGS+=(SC33_SPLICE_ENABLED=1)
  fi
  (cd "$JNI" && "$NDK/ndk-build" "${NDK_FLAGS[@]}")
fi
[[ -f "$SO" ]] || { echo "ERROR: 缺少 $SO" >&2; exit 1; }
# 注：勿写 `strings "$SO" | grep -q` —— grep -q 命中即退出会让 strings 吃 SIGPIPE(141)，
# 在 `set -o pipefail` 下整条管道判失败（本脚本首跑即踩）。
strings "$SO" > "$WORKDIR/so_strings.txt"
NM="$(ls "$NDK"/toolchains/llvm/prebuilt/*/bin/llvm-nm 2>/dev/null | head -1)"
if [[ -n "$NM" ]]; then
  "$NM" -D --defined-only "$SO" > "$WORKDIR/so_dynsym.txt" 2>/dev/null || true
  "$NM" -D --undefined-only "$SO" > "$WORKDIR/so_undef.txt" 2>/dev/null || true
else
  : > "$WORKDIR/so_dynsym.txt"; : > "$WORKDIR/so_undef.txt"
  echo "WARN: 找不到 llvm-nm，跳过符号自检" >&2
fi
grep -q '\[Sc33\]' "$WORKDIR/so_strings.txt" \
  || { echo "ERROR: so 自检失败，未见 [Sc33]" >&2; exit 1; }
if grep -q 'emutls' "$WORKDIR/so_undef.txt"; then
  echo "ERROR: so 含 __emutls_* 未定义符号（-fno-emulated-tls 失效，dlopen 会失败）" >&2; exit 1
fi
if ! grep -q ' T JNI_OnLoad' "$WORKDIR/so_dynsym.txt"; then
  echo "ERROR: so 未导出 JNI_OnLoad（--exclude-libs/visibility 配置有误）" >&2; exit 1
fi
echo "    so=$SO ($(stat -f%z "$SO") B)"

# ---- zip 外科：只换 .so，逐条零漂移 ----
echo "[2] zip surgical：仅替换 lib/arm64-v8a/libstarry_magic.so"
python3.11 - "$BASE_APK" "$SO" "$WORKDIR/unsigned.apk" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
base, so, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
TARGET = "lib/arm64-v8a/libstarry_magic.so"
new_so = so.read_bytes()
zin = zipfile.ZipFile(base, "r")
zout = zipfile.ZipFile(dst, "w")
orig, replaced = {}, False
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    data = zin.read(name)
    orig[name] = hashlib.sha256(data).hexdigest()
    if name == TARGET:
        data = new_so
        replaced = True
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()
assert replaced, "base apk 缺 " + TARGET
z2 = zipfile.ZipFile(dst, "r")
drift = []
for name, h in orig.items():
    h2 = hashlib.sha256(z2.read(name)).hexdigest()
    if name == TARGET:
        assert h2 != h, "so 未变化（底包是不是已经是本轮成品？）"
    elif h2 != h:
        drift.append(name)
z2.close()
if drift:
    raise SystemExit("ERROR: 非目标条目字节漂移: %s" % drift[:5])
print("    merge ok: 仅 %s 变化，其余 %d 条目零漂移" % (TARGET, len(orig) - 1))
PY

# ---- align + sign ----
echo "[3] zipalign + apksigner"
KS="$NX/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
mkdir -p "$OUT_DIR"
"$BT/zipalign" -f -p 4 "$WORKDIR/unsigned.apk" "$WORKDIR/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORKDIR/aligned.apk"
rm -f "$OUT_DIR/"*.idsig 2>/dev/null || true

# ---- 产物自检 ----
echo "[4] 自检"
unzip -p "$SIGNED" "lib/arm64-v8a/libstarry_magic.so" > "$WORKDIR/so_in_apk.so"
[[ "$(shasum -a 256 "$WORKDIR/so_in_apk.so" | cut -d' ' -f1)" == "$(shasum -a 256 "$SO" | cut -d' ' -f1)" ]] \
  || { echo "ERROR: 包内 so 与本地 so 不一致" >&2; exit 1; }
"$BT/apksigner" verify --print-certs "$SIGNED" | grep -i "SHA-256 digest" | head -2 || true
echo "    SOLD=$(shasum -a 256 "$SO" | cut -d' ' -f1)"
echo "    APK_MD5=$(md5 -q "$SIGNED") size=$(stat -f%z "$SIGNED")"
ls -lh "$SIGNED"

# ---- 装机（可选）----
if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[5] 覆盖安装"
  adb push "$SIGNED" /data/local/tmp/sc33_so.apk
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 /data/local/tmp/sc33_so.apk'"; then
    echo "    [install] cover ok"
  else
    echo "ERROR: 覆盖安装失败（异签名先报告，不自动卸载）" >&2
    exit 2
  fi
  adb shell rm -f /data/local/tmp/sc33_so.apk 2>/dev/null || true
  echo "    log: adb logcat -s StarryApp:V StarryMagic:V MenuService:V | grep -aE 'Sc33|MOD'"
else
  echo "[5] 未装机（STARRY_INSTALL=1 才装）"
fi
echo "DONE: $SIGNED"
