#!/usr/bin/env bash
# 独奏骑士 1.1.768 — 菜单源码换新（只换 classes2.dex，底包 so 逐字节不动）
#
# 用途：以机身现装星光包为底包，用**工作区当前 HEAD 的菜单源码**重编 classes2.dex，
#       只替换 classes2.dex 一个 zip 条目，实现「dex 菜单源码换新」；
#       本轮目标是设备ID识别修复（v5 SSAID 优先 + v4 稳定指纹兜底）。
#
# 与 dzqs_payload_swap_1.1.768.sh 的区别：本脚本**不重编 native**，
# 不替换 libstarry_magic.so（用户要求「其他不用动」），并断言 so 逐字节不变。
#
# 用法：
#   # 底包从指定的机身/本地 APK 取（推荐：设备只读拉取的那份）
#   DZQS_BASE_APK=/tmp/dzqs_pull/base.apk bash scripts/dzqs_menu_latest_1.1.768.sh
#   # 未指定 DZQS_BASE_APK 时，从设备只读拉取现装 base.apk
#   bash scripts/dzqs_menu_latest_1.1.768.sh
#   # 装机（覆盖安装，不清数据、不卸载）
#   STARRY_INSTALL=1 DZQS_BASE_APK=... bash scripts/dzqs_menu_latest_1.1.768.sh
#   # 已出包，只补装机
#   STARRY_INSTALL_ONLY=1 bash scripts/dzqs_menu_latest_1.1.768.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT_DIR="$NIXIANG/08_output/独奏骑士"
OUT_APK="${DZQS_OUT_APK:-$OUT_DIR/独奏骑士_1.1.768_v7-releaseSigned.apk}"
PKG="com.Shimmer.SoloKnight.cn"
SERIAL="${STARRY_SERIAL:-f9a116a2}"
BASE_APK="${DZQS_BASE_APK:-}"

KS="$NIXIANG/01_tools/starry_release.jks"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
PRE_GATE="$NIXIANG/04_scripts/_common/pre_install_gate.sh"
VERIFY="$NIXIANG/04_scripts/_common/verify_installed_apk.sh"

# 装机后必须匹配的签名证书指纹（starry_release.jks / 机身现装包一致）
WANT_CERT_SHA256="83:C5:CC:0B:E2:18:D0:46:B3:B7:AF:0D:2A:9F:82:36:29:7F:E2:28:17:1A:F2:97:4B:71:BA:C1:30:73:6F:1D"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done

die() { echo "ERROR: $*" >&2; exit 1; }

[[ -x "$SMALI_BIN" ]] || die "missing $SMALI_BIN"
[[ -n "$BT" ]] || die "build-tools not found"
[[ -f "$PRE_GATE" ]] || die "missing $PRE_GATE"
[[ -f "$KS" ]] || die "missing $KS"

lock="/tmp/dzqs_menu_latest_1768.lockdir"
mkdir "$lock" 2>/dev/null || die "lock held: $lock"
work="$(mktemp -d /tmp/dzqs_menu_latest.XXXXXX)"
remote="/data/local/tmp/dzqs_menu_latest.apk"
cleanup() { rm -rf "$work" "$lock"; }
trap cleanup EXIT

# ---------------------------------------------------------------- 只补装机
if [[ "${STARRY_INSTALL_ONLY:-0}" == "1" ]]; then
  [[ -f "$OUT_APK" ]] || die "STARRY_INSTALL_ONLY=1 但产物不存在: $OUT_APK"
  echo "[install-only] $OUT_APK"
  echo "  md5=$(md5 -q "$OUT_APK")"
  adb -s "$SERIAL" push "$OUT_APK" "$remote"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$remote\"'"
  bash "$VERIFY" "$SERIAL" "$PKG" "$OUT_APK" --su
  echo "DONE(install-only): $OUT_APK"
  exit 0
fi

[[ ! -e "$OUT_APK" ]] || die "输出已存在，先移走或指定 DZQS_OUT_APK: $OUT_APK"
command -v apktool >/dev/null || die "apktool not found"

echo "[0/6] 取底包（只读）"
if [[ -z "$BASE_APK" ]]; then
  apk_path="$(adb -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
  [[ -n "$apk_path" ]] || die "无法从设备取 $PKG 的 base.apk 路径"
  echo "  device path: $apk_path"
  BASE_APK="$work/base.apk"
  adb -s "$SERIAL" exec-out su -c "cat '$apk_path'" > "$BASE_APK"
fi
[[ -f "$BASE_APK" ]] || die "底包不存在: $BASE_APK"
base_md5="$(md5 -q "$BASE_APK")"
base_size="$(stat -f %z "$BASE_APK")"
echo "  base=$BASE_APK  size=$base_size  md5=$base_md5"

python3.11 - "$BASE_APK" <<'PY'
import sys, zipfile
from pathlib import Path
base = Path(sys.argv[1])
with zipfile.ZipFile(base) as z:
    names = set(z.namelist())
    for need in ("classes.dex", "classes2.dex", "lib/arm64-v8a/libstarry_magic.so"):
        if need not in names:
            raise SystemExit(f"底包缺少条目: {need}")
    c2 = z.read("classes2.dex")
    so = z.read("lib/arm64-v8a/libstarry_magic.so")
if b"com/starry/magic/StarryApp" not in c2 and b"StarryApp" not in c2:
    raise SystemExit("底包 classes2.dex 不含星菜单 StarryApp，底包选错")
print(f"  base classes2.dex={len(c2)}B  so={len(so)}B")
PY

echo "[1/6] 从工作区 HEAD 构建菜单 release APK（只取 dex，不取 so）"
echo "  src=$STARRY_SRC"
echo "  git HEAD=$(git -C "$STARRY_SRC" rev-parse --short HEAD 2>/dev/null || echo unknown)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$work/starry/"
(cd "$work/starry" && ./gradlew assembleRelease --offline --console=plain >/dev/null)
starry_apk="$work/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$starry_apk" ]] || die "missing $starry_apk"

echo "[2/6] 反编译取 com/starry smali + StarryApp 宿主改写"
apktool d -f -o "$work/starry_dec" "$starry_apk" >/dev/null
smali_root="$work/smali_in"
mkdir -p "$smali_root"
shopt -s nullglob
for d in "$work/starry_dec"/smali "$work/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$smali_root/com/"
done
shopt -u nullglob
[[ -f "$smali_root/com/starry/magic/StarryApp.smali" ]] || die "StarryApp.smali missing"

python3.11 - "$smali_root/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/sgsdk/core/SGGlobalApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/sgsdk/core/SGGlobalApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/sgsdk/core/SGGlobalApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/sgsdk/core/SGGlobalApplication;->onCreate()V",
    ),
]
for a, b in repls:
    t = t.replace(a, b)
if "Lcom/sgsdk/core/SGGlobalApplication;" not in t:
    raise SystemExit("ERROR: StarryApp missing SGGlobalApplication")
bad = [x for x in (
    ".super Landroid/app/Application;",
    "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
    "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
    "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
) if x in t]
if bad:
    raise SystemExit("ERROR: Application super dispatch remains: " + "; ".join(bad))
p.write_text(t, encoding="utf-8")
PY

echo "[3/6] smali assemble → 新 classes2.dex"
new_dex="$work/classes2.dex"
"$SMALI_BIN" assemble -o "$new_dex" "$smali_root"
# 用字节级断言（不依赖 locale 的 grep，中文串在 C locale 下 grep 会假失败）
python3.11 - "$new_dex" "$smali_root" <<'PY'
import sys
from pathlib import Path
dex = Path(sys.argv[1]).read_bytes()
src = Path(sys.argv[2])
need = {
    b"ver=v5": "v5 指纹标记",
    b"auth_legacy_device_ids": "legacy 设备ID键",
    "设备标识不可用：v5/v4 均缺失".encode(): "新设备标识降级提示",
    b"VERSION_LEGACY_V4": "v5->v4 兜底版本常量",
    b"getV4": "getV4 入口",
}
for k, label in need.items():
    if k not in dex:
        raise SystemExit(f"ERROR: 新 dex 缺 {label} ({k!r})")
# 旧 dex 专属提示必须消失，证明不是旧菜单源码
if "系统 ANDROID_ID 缺失或无效".encode() in dex:
    raise SystemExit("ERROR: 新 dex 仍含旧版纯 SSAID 报错串（菜单源码未换新）")
dp = (src / "com/starry/magic/auth/DeviceFingerprint.smali").read_text(encoding="utf-8")
if "VERSION_LEGACY_V4" not in dp or "getV4" not in dp:
    raise SystemExit("ERROR: DeviceFingerprint.smali 缺 v4 兜底")
print("  dex 断言通过：新设备ID逻辑（v5 优先 + v4 兜底）已在产物内")
PY
new_dex_md5="$(md5 -q "$new_dex")"
new_dex_size="$(stat -f %z "$new_dex")"
echo "  new classes2.dex size=$new_dex_size md5=$new_dex_md5"

echo "[4/6] 外科置换：只换 classes2.dex，so 逐字节不动"
python3.11 - "$BASE_APK" "$work/unsigned.apk" "$new_dex" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

base, dst, new_dex = map(Path, sys.argv[1:4])
with zipfile.ZipFile(base) as z:
    base_so = z.read("lib/arm64-v8a/libstarry_magic.so")
targets = {"classes2.dex"}

with zipfile.ZipFile(base, "r") as zin, zipfile.ZipFile(dst, "w", allowZip64=True) as zout:
    for item in zin.infolist():
        if item.filename.startswith("META-INF/"):
            continue
        if item.filename == "classes2.dex":
            data = new_dex.read_bytes()
        else:
            data = zin.read(item)
        info = zipfile.ZipInfo(item.filename, item.date_time)
        info.compress_type = item.compress_type
        info.comment = item.comment
        info.extra = item.extra
        info.internal_attr = item.internal_attr
        info.external_attr = item.external_attr
        info.create_system = item.create_system
        zout.writestr(info, data)

def manifest(p):
    out = {}
    with zipfile.ZipFile(p) as z:
        for i in z.infolist():
            if i.filename.startswith("META-INF/"):
                continue
            out[i.filename] = hashlib.sha256(z.read(i)).hexdigest()
    return out

ma, mb = manifest(base), manifest(dst)
if set(ma) != set(mb):
    raise SystemExit("entry set changed unexpectedly")
changed = [n for n in ma if ma[n] != mb[n]]
if changed != ["classes2.dex"]:
    raise SystemExit(f"非预期改动条目: {changed}")
with zipfile.ZipFile(dst) as z:
    if z.read("classes2.dex") != new_dex.read_bytes():
        raise SystemExit("classes2.dex copy mismatch")
    if z.read("lib/arm64-v8a/libstarry_magic.so") != base_so:
        raise SystemExit("starry so 被改动（本轮要求逐字节不动）")
print("  zip audit ok: 仅 classes2.dex 变更，so 与其余条目逐字节一致")
print("  base so md5 =", hashlib.md5(base_so).hexdigest())
PY

echo "[5/6] zipalign + release 签名"
aligned="$work/aligned.apk"
"$BT/zipalign" -f -p 4 "$work/unsigned.apk" "$aligned"
mkdir -p "$OUT_DIR"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" \
  --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --key-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --out "$OUT_APK" "$aligned"
"$BT/apksigner" verify --print-certs "$OUT_APK" >/dev/null
cert="$("$BT/apksigner" verify --print-certs "$OUT_APK" | sed -n 's/^Signer #1 certificate SHA-256 digest: //p' | head -1)"
# apksigner 输出小写无冒号，keytool 输出大写带冒号 → 归一化后再比
cert_norm="$(printf '%s' "$cert" | tr -d ':' | tr 'A-Z' 'a-z')"
want_norm="$(printf '%s' "$WANT_CERT_SHA256" | tr -d ':' | tr 'A-Z' 'a-z')"
[[ "$cert_norm" == "$want_norm" ]] || die "签名证书指纹不符: $cert (期望 $WANT_CERT_SHA256)"
echo "  output=$OUT_APK"
echo "  size=$(stat -f %z "$OUT_APK") md5=$(md5 -q "$OUT_APK")"
echo "  cert=$cert"

echo "[6/6] 装机前机器关（JNI 配套：so 注册方法 vs dex 声明）"
python3.11 - "$BASE_APK" "$work/base_so.so" <<'PY'
import sys, zipfile
from pathlib import Path
with zipfile.ZipFile(sys.argv[1]) as z:
    Path(sys.argv[2]).write_bytes(z.read("lib/arm64-v8a/libstarry_magic.so"))
PY
bash "$PRE_GATE" \
  --cpp "$STARRY_SRC/app/src/main/jni/Games/DzqsMod.cpp" \
  --dex "$new_dex" \
  --so "$work/base_so.so"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] 覆盖安装（不清数据、不卸载）"
  adb -s "$SERIAL" push "$OUT_APK" "$remote"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$remote\"'"
  bash "$VERIFY" "$SERIAL" "$PKG" "$OUT_APK" --su
fi

echo "DONE: $OUT_APK"
