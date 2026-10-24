#!/usr/bin/env bash
# 镇邪人：用指定源码树重编「原生菜单」（classes8.dex + libstarry_magic.so + certs）并 zip 级换进现有包。
#
# 场景（2026-10-25）：上一轮交付包的 classes8.dex 是 2026-10-01 的构建产物，早于
# 2026-10-07「ModBridge 无条件清功能偏好」根因修复（commit 70f02c4f）⇒ 每次启动都把
# 玩家开关清回全关。为不把工作区里**他人未提交的 Java 改动**带进包，本脚本要求源码树
# 干净：默认从 HEAD 建临时 git worktree 再编（ZXR_MENU_SRC 可指向任意干净树）。
#
# 用法：
#   ZXR_BASE_APK=08_output/镇邪人/<现行包>.apk \
#   ZXR_OUTPUT_NAME=镇邪人_1.0.3_cb9dc_menu20261025-releaseSigned.apk \
#     bash 04_scripts/镇邪人/menu_rebuild_zxr.sh
#   ZXR_INSTALL=1 …… 编完同签覆盖安装
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
BASE_APK="${ZXR_BASE_APK:-$NIXIANG/08_output/镇邪人/镇邪人_1.0.3_cb9dc_so_only_20261025-releaseSigned.apk}"
OUT="$NIXIANG/08_output/镇邪人"
OUT_NAME="${ZXR_OUTPUT_NAME:-镇邪人_1.0.3_cb9dc_menu_$(date +%Y%m%d)-releaseSigned.apk}"
SIGNED="$OUT/$OUT_NAME"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
PY="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"
[[ -x "$PY" ]] || PY="$(command -v python3)"
SERIAL="${SERIAL:-f9a116a2}"

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
[[ -n "$BT" ]] || { echo "ERROR: build-tools not found" >&2; exit 1; }
[[ -f "$BASE_APK" ]] || { echo "ERROR: base apk missing: $BASE_APK" >&2; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing" >&2; exit 1; }
command -v apktool >/dev/null || { echo "ERROR: apktool not in PATH" >&2; exit 1; }

LOCK_DIR="/tmp/zxr_menu_rebuild.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held" >&2; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zxr_menu_rebuild.XXXXXX)"
WT_CREATED=0
cleanup() {
  rm -rf "$WORK"; rm -rf "$LOCK_DIR"
  if [[ "$WT_CREATED" == "1" && -n "${WT_DIR:-}" ]]; then
    git -C "$NIXIANG" worktree remove --force "$WT_DIR" >/dev/null 2>&1 || rm -rf "$WT_DIR"
  fi
}
trap cleanup EXIT
mkdir -p "$OUT"

echo "[1/8] 准备干净源码树"
SRC="${ZXR_MENU_SRC:-}"
if [[ -z "$SRC" ]]; then
  WT_DIR="/tmp/zxr_head_wt.$$"
  git -C "$NIXIANG" worktree add "$WT_DIR" HEAD --detach >/dev/null
  WT_CREATED=1
  SRC="$WT_DIR/mod菜单/starry-mod-menu"
  echo "  临时 worktree: $WT_DIR (HEAD=$(git -C "$NIXIANG" rev-parse --short HEAD))"
fi
[[ -f "$SRC/app/src/main/jni/Android.mk" ]] || { echo "ERROR: 源码树不像 starry-mod-menu: $SRC" >&2; exit 1; }
if [[ -d "$SRC/../.git" || -f "$SRC/.git" ]]; then
  if [[ -n "$(git -C "$SRC" status --porcelain 2>/dev/null | head -1)" ]]; then
    echo "ERROR: 源码树有未提交改动，拒绝编（会把他人在做的改动带进包）" >&2
    exit 1
  fi
fi
cp -R "$SRC" "$WORK/starry"
# 构建工具链文件在模板 .gitignore 里（`*.jar` / local.properties）⇒ 干净 worktree 里没有，
# 从工作区补齐（只补工具文件，不碰任何 java/cpp 源码，避免他人在做的改动混入）。
for tool in gradle/wrapper/gradle-wrapper.jar local.properties gradle.properties gradlew; do
  if [[ -f "$NIXIANG/mod菜单/starry-mod-menu/$tool" && ! -f "$WORK/starry/$tool" ]]; then
    mkdir -p "$(dirname "$WORK/starry/$tool")"
    cp -f "$NIXIANG/mod菜单/starry-mod-menu/$tool" "$WORK/starry/$tool"
    echo "  补齐构建工具文件: $tool"
  fi
done
chmod +x "$WORK/starry/gradlew" 2>/dev/null || true
# 复核关键修复存在
grep -q "clearFeaturePrefsIfListChanged" "$WORK/starry/app/src/main/java/com/starry/magic/bridge/ModBridge.java" \
  || { echo "ERROR: 源码树没有 2026-10-07 偏好修复（clearFeaturePrefsIfListChanged）" >&2; exit 1; }

echo "[2/8] Android.mk 启用 ZXR 编译对（只改副本）"
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
print(f"  关掉 {deact} 行；L{pair_at + 1} 启用 ZXR 相邻对")
PY

echo "[3/8] ndk-build"
if [[ -z "${STARRY_BUILD_EPOCH:-}" ]]; then export STARRY_BUILD_EPOCH="$(date +%s)"; fi
echo "  STARRY_BUILD_EPOCH=${STARRY_BUILD_EPOCH}"
(cd "$WORK/starry/app/src/main/jni" && \
  STARRY_BUILD_EPOCH="$STARRY_BUILD_EPOCH" \
  "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk \
    NDK_APPLICATION_MK=./Application.mk -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$WORK/starry/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing so" >&2; exit 1; }
mkdir -p "$WORK/starry/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO_BUILT" "$WORK/starry/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

echo "[4/8] gradle assembleRelease（菜单模板）"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain -q)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[5/8] apktool d → smali assemble classes8.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_starry"; mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for f in com/starry/magic/menu/NativeMenuController.smali com/starry/magic/cocos/ZxrHooks.smali \
         com/starry/magic/menu/Preferences.smali com/starry/magic/bridge/ModBridge.smali; do
  [[ -f "$SMALI_ROOT/$f" ]] || { echo "ERROR: $f missing" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/classes8.dex" "$SMALI_ROOT"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then mkdir -p "$WORK/assets/certs"; cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"; fi

echo "[6/8] 产物复核（偏好修复 + Zxr 挂钩 + 兜底标记必须在）"
"$PY" - "$WORK/classes8.dex" "$WORK/lib/arm64-v8a/libstarry_magic.so" <<'PY'
import sys
from pathlib import Path
dex = Path(sys.argv[1]).read_bytes()
so = Path(sys.argv[2]).read_bytes()
for probe in (b"NativeMenuController", b"ZxrHooks", b"clearFeaturePrefsIfListChanged",
              b"feature_list_sig", b"persist_features"):
    if probe not in dex:
        raise SystemExit(f"ERROR: classes8 缺 {probe!r}（源码树不是修复后的版本？）")
if b"clearFeaturePrefs" in dex and b"clearFeaturePrefsIfListChanged" not in dex:
    raise SystemExit("ERROR: classes8 仍是旧的「无条件清偏好」版本")
for probe in (b"[Zxr]", b"zxr\x00", b"hotfallback plan ok", b"hot-patch MISS", b"hot=%d"):
    if probe not in so:
        raise SystemExit(f"ERROR: so 缺 {probe!r}")
print(f"  classes8={len(dex)}B so={len(so)}B ok")
PY

echo "[7/8] zip 级换菜单（classes8.dex + so + libc++ + certs）+ zipalign + 签名"
"$PY" - "$BASE_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
base, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
extras = {}
extras["classes8.dex"] = (work / "classes8.dex").read_bytes()
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib"))] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in sorted(certs.rglob("*")):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs))] = p.read_bytes()

zin = zipfile.ZipFile(base)
zout = zipfile.ZipFile(dst, "w")
kept = replaced = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        replaced += 1
        continue
    data = zin.read(name)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data); kept += 1
for name in sorted(extras):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, extras[name])
zin.close(); zout.close()
z2 = zipfile.ZipFile(dst)
assert z2.read("classes8.dex") == extras["classes8.dex"]
print(f"  repack ok kept={kept} replaced={replaced} classes8={len(extras['classes8.dex'])}B")
PY
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | grep -E "Signer #1 certificate SHA-256 digest"

echo "[8/8] 后检：只有菜单相关条目变化"
"$PY" - "$SIGNED" "$BASE_APK" <<'PY'
import hashlib, sys, zipfile
new, base = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
nn = {n: new.read(n) for n in new.namelist() if not n.startswith("META-INF/")}
bn = {n: base.read(n) for n in base.namelist() if not n.startswith("META-INF/")}
if set(nn) != set(bn):
    raise SystemExit(f"ERROR: 条目集合变了 +{sorted(set(nn)-set(bn))[:5]} -{sorted(set(bn)-set(nn))[:5]}")
diff = [n for n in nn if nn[n] != bn[n]]
allowed = ("classes8.dex", "lib/arm64-v8a/libstarry_magic.so",
           "lib/arm64-v8a/libc++_shared.so", "assets/certs/")
bad = [n for n in diff if not n.startswith(allowed)]
if bad:
    raise SystemExit(f"ERROR: 意外变动条目 {bad}")
data = open(sys.argv[1], "rb").read()
print(f"  changed={diff}")
print(f"  final size={len(data)} md5={hashlib.md5(data).hexdigest()} sha256={hashlib.sha256(data).hexdigest()}")
PY
ls -lh "$SIGNED"

if [[ "${ZXR_INSTALL:-0}" == "1" ]]; then
  echo "[install] 同签覆盖安装（-r，不清数据/不动 prefs）"
  SERIAL="$SERIAL" bash "$NIXIANG/04_scripts/镇邪人/install_zxr.sh" "$SIGNED"
fi
echo "done → ${SIGNED}"
