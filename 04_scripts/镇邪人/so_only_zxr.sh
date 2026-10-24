#!/usr/bin/env bash
# 镇邪人 so-only 轮：只重编并替换 lib/arm64-v8a/libstarry_magic.so（不动 classes8.dex / assets / Manifest）。
#
# 用途：2026-10-25 把 native 热更兜底从「字面锚点」改成「结构派生 + 变体表 + 显式告警」，
# 只涉及 Games/ZxrMod.cpp / Games/ZxrDerive.h / Games/ZxrPatchJs.h ⇒ 不必碰 Java/dex。
# 不修改工作区 Android.mk：在 $WORK 副本里解开 ZXR 的 cflags+src 相邻对再编。
#
# 用法：
#   ZXR_BASE_APK=08_output/镇邪人/镇邪人_1.0.3_cb9dc_star-releaseSigned.apk \
#   ZXR_OUTPUT_NAME=镇邪人_1.0.3_cb9dc_so_only_20261025-releaseSigned.apk \
#     bash 04_scripts/镇邪人/so_only_zxr.sh
#   ZXR_INSTALL=1 …… 编完直接同签覆盖安装（-r，不清数据）
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
STARRY="$NIXIANG/mod菜单/starry-mod-menu"
BASE_APK="${ZXR_BASE_APK:-$NIXIANG/08_output/镇邪人/镇邪人_1.0.3_cb9dc_star-releaseSigned.apk}"
OUT="$NIXIANG/08_output/镇邪人"
OUT_NAME="${ZXR_OUTPUT_NAME:-镇邪人_1.0.3_cb9dc_so_only_$(date +%Y%m%d)-releaseSigned.apk}"
SIGNED="$OUT/$OUT_NAME"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
PY="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"
[[ -x "$PY" ]] || PY="$(command -v python3)"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"

KS="$NIXIANG/01_tools/starry_release.jks"; KS_ALIAS="${STARRY_KS_ALIAS:-starry}"; KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ "${ZXR_USE_RELEASE_KS:-1}" != "1" ]]; then
  KS="$NIXIANG/01_tools/uber-debug.keystore"; KS_ALIAS="androiddebugkey"; KS_PASS="android"
fi
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools with zipalign not found" >&2; exit 1; }
[[ -f "$BASE_APK" ]] || { echo "ERROR: base apk missing: $BASE_APK" >&2; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing: $NDK" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: keystore missing: $KS" >&2; exit 1; }

LOCK_DIR="/tmp/zxr_so_only.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held" >&2; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zxr_so_only.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT"

echo "[1/5] 复制源码到 ASCII 工作区并启用 ZXR 编译对（不动工作区 Android.mk）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY/" "$WORK/starry/"
"$PY" - "$WORK/starry/app/src/main/jni/Android.mk" <<'PY'
import re, sys
p = sys.argv[1]
lines = open(p, encoding="utf-8").read().split("\n")
out, deact = [], 0
pat_cflags_active = re.compile(r'^(LOCAL_CFLAGS\s*(?::=|\+=)\s*[^\n]*STARRY_GAME_\w+)\s*$')
pat_src_active    = re.compile(r'^(LOCAL_SRC_FILES\s*(?::=|\+=)\s*Main\.cpp[^\n]*)\s*$')
for ln in lines:
    if pat_cflags_active.match(ln) or pat_src_active.match(ln):
        out.append("# " + ln); deact += 1
    else:
        out.append(ln)
pat_cflags_off = re.compile(r'^#\s*(LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_ZXR)\s*$')
pat_src_off    = re.compile(r'^#\s*(LOCAL_SRC_FILES\s*:=\s*Main\.cpp\s+Games/ZxrMod\.cpp)\s*$')
pair_at = None
for i in range(len(out) - 1):
    if pat_cflags_off.match(out[i]) and pat_src_off.match(out[i + 1]):
        pair_at = i; break
assert pair_at is not None, "Android.mk 找不到相邻配对的 ZXR cflags+src 两行"
out[pair_at]     = pat_cflags_off.match(out[pair_at]).group(1)
out[pair_at + 1] = pat_src_off.match(out[pair_at + 1]).group(1)
s = "\n".join(out)
open(p, "w", encoding="utf-8").write(s)
assert re.findall(r'^LOCAL_CFLAGS\s*(?::=|\+=)\s*[^\n]*STARRY_GAME_(\w+)', s, re.M) == ["ZXR"]
assert re.findall(r'^LOCAL_SRC_FILES\s*(?::=|\+=)\s*Main\.cpp\s+Games/(\w+Mod)\.cpp', s, re.M) == ["ZxrMod"]
print(f"  Android.mk: 关掉 {deact} 行、L{pair_at + 1} 启用 ZXR 相邻对")
PY

echo "[2/5] ndk-build（ZxrMod.cpp + ZxrDerive.h + ZxrPatchJs.h 变体表）"
# OTA stamp：源码副本在 /tmp 不是 git 仓库，Application.mk 会回退到远古 epoch，
# 导致「包内 so 比设备已 OTA 的 so 旧」⇒ 玩家继续跑旧 so。这里显式给当前时间。
if [[ -z "${STARRY_BUILD_EPOCH:-}" ]]; then
  export STARRY_BUILD_EPOCH="$(date +%s)"
fi
echo "  STARRY_BUILD_EPOCH=${STARRY_BUILD_EPOCH}"
(cd "$WORK/starry/app/src/main/jni" && \
  STARRY_BUILD_EPOCH="$STARRY_BUILD_EPOCH" \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk \
    NDK_APPLICATION_MK=./Application.mk -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$WORK/starry/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT" >&2; exit 1; }

echo "[3/5] so 标签复核（新兜底形态）"
"$PY" - "$SO_BUILT" "$BASE_APK" <<'PY'
import sys, zipfile
from pathlib import Path
so = Path(sys.argv[1]).read_bytes()
need = [b"[Zxr]", b"zxr\x00", b"hotfallback plan ok", b"hot-patch MISS",
        b"hotfallback variant hit", b"hotfallback splice eval failed, retry original script",
        b"[Zxr] feature #%d=%s noAd=%d splice=%d hot=%d"]
for t in need:
    if t not in so:
        raise SystemExit(f"ERROR: so 缺标记 {t!r}（改动没编进去？）")
bad = [b"assets/remote/src/index", b"index.b5278", b"index.d5507"]
for t in bad:
    if t in so:
        raise SystemExit(f"ERROR: so 仍含旧文件名白名单 {t!r}")
old = zipfile.ZipFile(sys.argv[2]).read("lib/arm64-v8a/libstarry_magic.so")
print(f"  so ok size={len(so)} (base={len(old)})")
PY

echo "[4/5] zip 级只换 so（保留 dex/assets/Manifest 逐字节不变）+ zipalign + 签名"
"$PY" - "$BASE_APK" "$WORK/aligned_in.apk" "$SO_BUILT" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
base, dst, so = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
newso = so.read_bytes()
zin = zipfile.ZipFile(base)
zout = zipfile.ZipFile(dst, "w")
replaced = 0
for item in zin.infolist():
    data = zin.read(item.filename)
    if item.filename == "lib/arm64-v8a/libstarry_magic.so":
        data = newso
        replaced += 1
    info = zipfile.ZipInfo(item.filename, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if item.filename == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()
if replaced != 1:
    raise SystemExit(f"ERROR: so 条目替换次数={replaced}")
z2 = zipfile.ZipFile(dst)
assert z2.read("lib/arm64-v8a/libstarry_magic.so") == newso
print(f"  repack ok: so sha256={hashlib.sha256(newso).hexdigest()}")
PY
"$BT/zipalign" -f -p 4 "$WORK/aligned_in.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | grep -E "Signer #1 certificate SHA-256 digest"

echo "[5/5] 后检：包身份 + 只差一个 so"
"$PY" - "$SIGNED" "$BASE_APK" <<'PY'
import hashlib, sys, zipfile
new, base = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
nn, bn = set(new.namelist()), set(base.namelist())
nn = {n for n in nn if not n.startswith("META-INF/")}
bn = {n for n in bn if not n.startswith("META-INF/")}
if nn != bn:
    raise SystemExit(f"ERROR: 条目集合变了: +{sorted(nn-bn)[:5]} -{sorted(bn-nn)[:5]}")
diff = [n for n in nn if new.read(n) != base.read(n)]
if diff != ["lib/arm64-v8a/libstarry_magic.so"]:
    raise SystemExit(f"ERROR: 变动条目不止 so: {diff}")
data = open(sys.argv[1], "rb").read()
print(f"  final size={len(data)} md5={hashlib.md5(data).hexdigest()} sha256={hashlib.sha256(data).hexdigest()}")
print("  only-so-diff: ok")
PY
ls -lh "$SIGNED"

if [[ "${ZXR_INSTALL:-0}" == "1" ]]; then
  echo "[install] 同签覆盖安装（-r，不清数据）"
  SERIAL="$SERIAL" bash "$NIXIANG/04_scripts/镇邪人/install_zxr.sh" "$SIGNED"
fi
echo "done → ${SIGNED}"
