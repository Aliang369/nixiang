#!/usr/bin/env bash
# 镇邪人 1.0.3 — 0d1ae 热更修复版 + **最新菜单源码**（2026-10-01）
#
# 与 surgical_zxr_ca0d4_menu_latest.sh 的差别（对应本轮两件事）：
#   1) 战斗热更：服务器 src 已从 e4278 漂到 0d1ae，引擎「包内命中优先」落空后
#      会走缓存下载**未修补**的 0d1ae → 四项战斗功能静默失效。
#      本脚本把修补好的 index.0d1ae.jsc / config.0d1ae.json 钉进包内，
#      并保留底包里已有的 e4278/ca0d4/d5507 三份 pin。
#   2) 菜单：不再沿用底包里的旧 classes8.dex / so，而是用当前 mod菜单 源码重新：
#        ndk-build → gradle assembleRelease → apktool d → smali assemble → classes8.dex
#      其中 native Games/ZxrMod.cpp 的热更兜底已从「文件名白名单」改为「内容指纹」。
#   底包的 assets/jsb-adapter/jsb-engine.js（包内命中优先）与 assets/main.js 原样保留。
#
# 用法：ZXR_BASE_APK=/path/base.apk bash mod菜单/starry-mod-menu/scripts/surgical_zxr_0d1ae_menu_latest.sh
#   必须显式给 ZXR_BASE_APK：底包=设备上当前安装的星光包（本轮为 09-29 的
#   e4278 正式包，110,316,865B / md5 0cde8985655c30007b7bd65275f7742f）。
#   APK 不留本地（2026-09-30 定）⇒ 从设备只读拉取、用完即删：
#     adb -s $SERIAL shell pm path com.zxr.dlxmz.zxrwz37zgx10.gg9b
#     adb -s $SERIAL exec-out su -c 'cat <上面返回的 base.apk 路径>' > /tmp/base.apk
#   身份与再获取路径见 10_docs/_state/底包再获取台账.md。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
BASE_APK="${ZXR_BASE_APK:-}"
if [[ -z "$BASE_APK" ]]; then
  echo "ERROR: 必须指定 ZXR_BASE_APK（底包不留本地；身份/再获取见 10_docs/_state/底包再获取台账.md）" >&2
  exit 1
fi
OUT="$NIXIANG/08_output/镇邪人"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
# 签名：默认 uber-debug（与真机旧 inject 同证，可 -r 覆盖安装不丢数据）；
# 正式出货必须 ZXR_USE_RELEASE_KS=1 切 01_tools/starry_release.jks
#   （CN=StarryMagic, SHA-256 83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d）
KS="$NIXIANG/01_tools/uber-debug.keystore"
KS_ALIAS="androiddebugkey"; KS_PASS="android"
if [[ "${ZXR_USE_RELEASE_KS:-0}" == "1" ]]; then
  KS="$NIXIANG/01_tools/starry_release.jks"
  KS_ALIAS="${STARRY_KS_ALIAS:-starry}"; KS_PASS="${STARRY_KS_PASS:-starry2026}"
fi
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
OUT_NAME="${ZXR_OUTPUT_NAME:-镇邪人_1.0.3_0d1ae_menu-latest-releaseSigned.apk}"
SIGNED="$OUT/$OUT_NAME"
MENU_KEEP="$NIXIANG/07_patched/镇邪人/menu_latest"   # 留档新菜单产物，便于复用/比对

PATCHED_SRC_JSC="${ZXR_PATCHED_SRC_JSC:-$NIXIANG/07_patched/镇邪人/0d1ae/index.0d1ae.jsc}"
PATCHED_SRC_CONFIG="${ZXR_PATCHED_SRC_CONFIG:-$NIXIANG/07_patched/镇邪人/0d1ae/config.0d1ae.json}"
PIN_VER="${ZXR_PIN_VER:-0d1ae}"
SKIP_MENU_BUILD="${ZXR_SKIP_MENU_BUILD:-0}"   # =1 时复用 $MENU_KEEP 里已构建好的产物

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools with zipalign not found"; exit 1; }

for f in "$BASE_APK" "$PATCHED_SRC_JSC" "$PATCHED_SRC_CONFIG" "$KS" "$SMALI_BIN"; do
  [[ -f "$f" ]] || { echo "ERROR: missing input $f"; exit 1; }
done
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: missing $NDK/ndk-build"; exit 1; }
command -v apktool >/dev/null || { echo "ERROR: apktool not in PATH"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
# 当前 HEAD 的 Android.mk 停在别的游戏（本轮是 CSTJJS 一组），且注释里有多处
# STARRY_GAME_ZXR 散行（第 65-80 行是「可用游戏清单」，没有配对的 SRC_FILES）。
# 所以下面必须锁定 **相邻的 cflags+src 配对**，否则会解错行、编出空壳 so。
# 我们**不改工作区源码**：只在 $WORK 的副本里解开那一对再编译。
grep -qE '^\s*#\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_ZXR\s*$' "$MK" \
  || { echo "ERROR: Android.mk 里找不到注释状态的 STARRY_GAME_ZXR 行"; exit 1; }

LOCK_DIR="/tmp/zxr_0d1ae_menu.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held"; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zxr_0d1ae_menu.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$MENU_KEEP"

echo "[1/6] 校验底包（设备在装星光包，已钉 e4278/ca0d4/d5507）"
python3.11 - "$BASE_APK" "$PIN_VER" <<'PY'
import hashlib
import sys
import zipfile

p, pin = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(p); names = set(z.namelist())
need = ["classes8.dex", "lib/arm64-v8a/libstarry_magic.so", "assets/jsb-adapter/jsb-engine.js",
        "assets/main.js", "AndroidManifest.xml", "assets/certs/starlight.crt",
        "assets/assets/src/index.d5507.jsc", "assets/assets/src/index.ca0d4.jsc",
        "assets/assets/src/index.e4278.jsc", "assets/assets/src/config.e4278.json"]
miss = [n for n in need if n not in names]
if miss: raise SystemExit(f"ERROR: base missing {miss}")
want_pin = f"assets/assets/src/index.{pin}.jsc"
if want_pin in names:
    raise SystemExit(f"ERROR: base 已含 {want_pin}（底包拿错了，这是上一轮的产物）")
eng = z.read("assets/jsb-adapter/jsb-engine.js")
if b"var packagePath = getPackagePathFromRemoteUrl(url);" not in eng:
    raise SystemExit("ERROR: 底包引擎没有「包内命中优先」补丁")
print(f"  base md5={hashlib.md5(open(p,'rb').read()).hexdigest()} entries={len(names)} pin={pin} 待钉")
PY

if [[ "$SKIP_MENU_BUILD" == "1" ]]; then
  echo "[2/6] 复用已构建菜单产物 $MENU_KEEP"
  for f in classes8.dex libstarry_magic.so libc++_shared.so; do
    [[ -f "$MENU_KEEP/$f" ]] || { echo "ERROR: missing $MENU_KEEP/$f"; exit 1; }
  done
  cp -f "$MENU_KEEP/classes8.dex" "$WORK/classes8.dex"
  mkdir -p "$WORK/lib/arm64-v8a"
  cp -f "$MENU_KEEP/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
  cp -f "$MENU_KEEP/libc++_shared.so" "$WORK/lib/arm64-v8a/"
  if [[ -d "$MENU_KEEP/certs" ]]; then mkdir -p "$WORK/assets/certs"; cp -R "$MENU_KEEP/certs/." "$WORK/assets/certs/"; fi
else
  echo "[2/6] 复制源码到 $WORK/starry 并开启 ZXR 开关（不动工作区）"
  rsync -a --delete \
    --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
    --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
    "$STARRY_SRC/" "$WORK/starry/"
  python3.11 - "$WORK/starry/app/src/main/jni/Android.mk" <<'PY'
import re, sys
p = sys.argv[1]
lines = open(p, encoding="utf-8").read().split("\n")
out, deact = [], 0
# 1) 关掉任何「已启用」的游戏选择行（当前 HEAD 停在 CSTJJS 一组；286-290 行
#    还有一段 ifdef STARRY_GAME_DZQS 的 := 形式覆盖，同样要关）。
#    注意赋值号要写 `(?::=|\+=)`：写成 `(?::|\+=)` 只匹配单个冒号（漏掉 :=），
#    写成 `\+?=` 则漏掉冒号等号 —— 两种写法都会让「已启用」的行残留，编出多游戏 so。
pat_cflags_active = re.compile(r'^(LOCAL_CFLAGS\s*(?::=|\+=)\s*[^\n]*STARRY_GAME_\w+)\s*$')
pat_src_active    = re.compile(r'^(LOCAL_SRC_FILES\s*(?::=|\+=)\s*Main\.cpp[^\n]*)\s*$')
for ln in lines:
    if pat_cflags_active.match(ln) or pat_src_active.match(ln):
        out.append("# " + ln); deact += 1
    else:
        out.append(ln)
# 2) 只启用 **相邻配对** 的 ZXR 组（上面 65-80 行那种散行 cflags 不配对，必须跳过）
pat_cflags_off = re.compile(r'^#\s*(LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_ZXR)\s*$')
pat_src_off    = re.compile(r'^#\s*(LOCAL_SRC_FILES\s*:=\s*Main\.cpp\s+Games/ZxrMod\.cpp)\s*$')
pair_at = None
for i in range(len(out) - 1):
    if pat_cflags_off.match(out[i]) and pat_src_off.match(out[i + 1]):
        pair_at = i; break
assert pair_at is not None, "Android.mk 里找不到相邻配对的 ZXR cflags+src 两行"
out[pair_at]     = pat_cflags_off.match(out[pair_at]).group(1)
out[pair_at + 1] = pat_src_off.match(out[pair_at + 1]).group(1)
s = "\n".join(out)
open(p, "w", encoding="utf-8").write(s)
# 3) 复检：唯一启用的游戏选择，且就是 ZXR 相邻那一对
act_c = re.findall(r'^LOCAL_CFLAGS\s*(?::=|\+=)\s*[^\n]*STARRY_GAME_(\w+)', s, re.M)
act_s = re.findall(r'^LOCAL_SRC_FILES\s*(?::=|\+=)\s*Main\.cpp\s+Games/(\w+Mod)\.cpp', s, re.M)
assert act_c == ["ZXR"], f"启用的游戏 CFLAGS 不唯一: {act_c}"
assert act_s == ["ZxrMod"], f"启用的 Games/*Mod 不唯一: {act_s}"
print(f"  Android.mk: 关掉 {deact} 行、在 L{pair_at + 1} 启用 ZXR 相邻对 → CFLAGS={act_c} SRC={act_s}")
PY

  echo "[2b/6] ndk-build（最新菜单源码，WORK 副本内）"
  (cd "$WORK/starry/app/src/main/jni" && \
    "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk \
      NDK_APPLICATION_MK=./Application.mk -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
  SO_BUILT="$WORK/starry/app/src/libs/arm64-v8a/libstarry_magic.so"
  [[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
  python3.11 - "$SO_BUILT" <<'PY'
from pathlib import Path
import sys
b = Path(sys.argv[1]).read_bytes()
if b"[Zxr]" not in b or b"zxr\0" not in b:
    raise SystemExit("ERROR: so 不是 ZXR（缺 [Zxr] 或 game_id zxr）")
bad = [t for t in (b"[Tlgd]", b"[Sc33]", b"[Dwct]", b"[Gyws]", b"[Hzsj]", b"[Pcr]", b"[Wxsj]") if t in b]
if bad: raise SystemExit(f"ERROR: so 仍含其它游戏标签: {bad}")
# 热更兜底必须已是「内容指纹」版：旧的文件名白名单整块消失、新增失败回退分支
if b"assets/remote/src/index" in b:
    raise SystemExit("ERROR: so 仍含旧的 bundle 文件名白名单（ZxrMod.cpp 没编进去？）")
if b"hotfallback splice eval failed, retry original script" not in b:
    raise SystemExit("ERROR: so 缺内容指纹版失败回退分支")
print(f"  so tags ok ([Zxr]+zxr+内容指纹兜底), size={len(b)}")
PY
  for dest in "$WORK/starry/app/src/main/jniLibs/arm64-v8a" \
              "$WORK/starry/app/src/main/libs/arm64-v8a" \
              "$WORK/starry/app/libs/arm64-v8a"; do
    mkdir -p "$dest"; cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
  done

  echo "[3/6] gradle assembleRelease（starry 模板）"
  (cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
  STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
  [[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

  echo "[4/6] apktool d → smali assemble classes8.dex"
  apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
  SMALI_ROOT="$WORK/smali_starry"; mkdir -p "$SMALI_ROOT"
  shopt -s nullglob
  for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
    [[ -d "$d/com/starry" ]] || continue
    rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
  done
  shopt -u nullglob
  for f in com/starry/magic/menu/NativeMenuController.smali com/starry/magic/cocos/ZxrHooks.smali; do
    [[ -f "$SMALI_ROOT/$f" ]] || { echo "ERROR: $f missing"; exit 1; }
  done
  "$SMALI_BIN" assemble -o "$WORK/classes8.dex" "$SMALI_ROOT"
  mkdir -p "$WORK/lib/arm64-v8a"
  cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
  [[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
  if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
    mkdir -p "$WORK/assets/certs"; cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
  fi
  # 留档
  cp -f "$WORK/classes8.dex" "$MENU_KEEP/classes8.dex"
  cp -f "$WORK/lib/arm64-v8a/libstarry_magic.so" "$MENU_KEEP/libstarry_magic.so"
  [[ -f "$WORK/lib/arm64-v8a/libc++_shared.so" ]] && cp -f "$WORK/lib/arm64-v8a/libc++_shared.so" "$MENU_KEEP/libc++_shared.so"
  if [[ -d "$WORK/assets/certs" ]]; then rm -rf "$MENU_KEEP/certs"; cp -R "$WORK/assets/certs" "$MENU_KEEP/certs"; fi
  ls -l "$MENU_KEEP"
fi

echo "[5/6] surgical merge（新菜单 dex/so/certs + 钉入 index.$PIN_VER + 保留底包引擎/main.js）"
python3.11 - "$BASE_APK" "$WORK" "$WORK/unsigned.apk" "$PATCHED_SRC_JSC" "$PATCHED_SRC_CONFIG" "$PIN_VER" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, work, dst, jsc, cfg, pin = (Path(a) for a in sys.argv[1:7])
c8 = (work / "classes8.dex").read_bytes()
extras = {
    f"assets/assets/src/index.{pin}.jsc": jsc.read_bytes(),
    f"assets/assets/src/config.{pin}.json": cfg.read_bytes(),
}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

# 底包（设备在装星光包）已经没有 assets/web / 旧相对路径路由 / b5278，
# 这里的 DROP_* 只是防呆：底包若来自其它来源也不会把历史垃圾带进新包。
DROP_PREFIX = ("assets/web/", "assets/assets/remote/src/", "assets/remote/src/",
               "assets/assets/src/index.b5278.", "assets/src/index.b5278.")
DROP_EXACT = {"assets/web", "classes8.dex",
              "lib/arm64-v8a/libstarry_magic.so", "lib/arm64-v8a/libc++_shared.so"}

KEEP_VERIFY = ["assets/jsb-adapter/jsb-engine.js", "assets/main.js", "resources.arsc",
               "classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
               "classes5.dex", "classes6.dex", "classes7.dex"]
KEEP_SHA = {}

zin = zipfile.ZipFile(src, "r")
for n in KEEP_VERIFY:
    KEEP_SHA[n] = hashlib.sha256(zin.read(n)).hexdigest()
base_names = set(zin.namelist())
zout = zipfile.ZipFile(dst, "w")
kept = dropped = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"): continue
    if name in DROP_EXACT or name.startswith(DROP_PREFIX):
        dropped += 1; continue
    if name in extras:
        continue
    data = zin.read(name)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr; info.create_system = 3
    zout.writestr(info, data); kept += 1
info = zipfile.ZipInfo("classes8.dex"); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
zout.writestr(info, c8)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
out_names = set(zout2.namelist())
for n, h in KEEP_SHA.items():
    assert hashlib.sha256(zout2.read(n)).hexdigest() == h, f"{n} drifted"
nc8 = zout2.read("classes8.dex")
assert nc8 == c8, "classes8 mismatch"
assert b"NativeMenuController" in nc8 and b"ZxrHooks" in nc8 and b"mod-menu-v2.html" not in nc8
assert nc8 != zin2.read("classes8.dex"), "classes8 未更新（仍是旧菜单）"
# 三份历史 pin 必须原样还在，新 pin 必须已就位
for ver in ("d5507", "ca0d4", "e4278", pin):
    for tail in (f"index.{ver}.jsc", f"config.{ver}.json"):
        assert f"assets/assets/src/{tail}" in out_names, tail
        if ver != pin:
            assert f"assets/assets/src/{tail}" in base_names, tail
assert len(extras[f"assets/assets/src/index.{pin}.jsc"]) > 4_000_000, "pin jsc 太小，拿错文件了"
assert not [n for n in out_names if n.startswith("assets/web")]
assert not [n for n in out_names if "index.b5278" in n]
print(f"  merge ok; kept={kept} dropped={dropped}; classes8={len(nc8)}B(新菜单) "
      f"so={len(extras['lib/arm64-v8a/libstarry_magic.so'])}B pin={pin}"
      f"({len(extras[f'assets/assets/src/index.{pin}.jsc'])}B)")
zin2.close(); zout2.close()
PY

echo "[6/6] zipalign + sign + 后检"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | grep -E "Signer #1 certificate SHA-256"
python3.11 - "$SIGNED" "$BASE_APK" "$PIN_VER" <<'PY'
import sys, zipfile, hashlib
new = zipfile.ZipFile(sys.argv[1]); base = zipfile.ZipFile(sys.argv[2]); pin = sys.argv[3]
assert new.read("classes8.dex") != base.read("classes8.dex"), "classes8 未更新（仍是旧菜单）"
assert new.read("lib/arm64-v8a/libstarry_magic.so") != base.read("lib/arm64-v8a/libstarry_magic.so"), "so 未更新"
n = new.namelist()
print("  entries=", len(n))
print("  classes8 sha256=", hashlib.sha256(new.read("classes8.dex")).hexdigest())
print("  so sha256=", hashlib.sha256(new.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest())
print(f"  {pin} jsc sha256=", hashlib.sha256(new.read(f"assets/assets/src/index.{pin}.jsc")).hexdigest())
assert f"assets/assets/src/config.{pin}.json" in n
assert not any(x.startswith("assets/web") for x in n)
PY
ls -lh "$SIGNED"
echo "done → $SIGNED"
