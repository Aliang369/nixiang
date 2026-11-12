#!/usr/bin/env bash
# script-policy: {"purpose": "追迹迷城 1.0.20261111 单轨外科组装：官方底包 + TapSDK 三处静态补丁（classes4）+ 新增 classes6（仅星光，无 killer）+ 私有 mk 重编 so + 最小 Manifest（不改 Factory），产出 releaseSigned APK", "inputs": "02_apks/追迹迷城/追迹迷城_1.0.20261111_official.apk、mod菜单/starry-mod-menu/libs_zjmc/arm64-v8a/libstarry_magic.so、07_patched/追迹迷城/surgical_1.0.20261111_singletrack/AndroidManifest.xml、04_scripts/追迹迷城/patch_zjmc_tapsdk_static.py、01_tools/starry_release.jks", "outputs": "08_output/追迹迷城/追迹迷城_1.0.20261111_starry-singletrack-final-releaseSigned.apk", "boundary": "只做 zip 外科组装；禁止 apktool b 整包、禁止改公共 Android.mk/Main.cpp/MainModern.cpp/GameConfig.h、禁止复活 SignSpoof、禁止注入 killer、禁止脚本内卸载或安装（除非 STARRY_INSTALL=1 且另有授权）", "prior_art": "mod菜单/starry-mod-menu/scripts/surgical_zjmc_1.0.20261030.sh（同族上一版，逐行沿用）；mod菜单/starry-mod-menu/scripts/surgical_zjmc_1.0.20261009.sh（更早同族）；mod菜单/starry-mod-menu/scripts/surgical_zjmc_1.0.20261111_dualtrack.sh（本版双轨历史，保留作对照包生产者，不用于出包）；04_scripts/追迹迷城/zjmc_manifest_minimal.sh", "difference": "单轨：不注入 killer、Manifest 不改 appComponentFactory、TapSDK 三处静态补丁落 classes4", "evidence": "10_docs/追迹迷城/路线锁定.md#单轨"}
# 追迹迷城 1.0.20261111（vc104502，TapTap）— 外科组装 · **单轨静态改包**
#
# 产物：08_output/追迹迷城/追迹迷城_1.0.20261111_starry-singletrack-final-releaseSigned.apk
#   （2026-11-11 修复轮：旧候选 ..._starry-singletrack-releaseSigned.apk 误删 85 条 META-INF 业务条目，
#    保留作对照不覆盖；本脚本自本轮起按显式旧签名清单清理，META-INF 业务条目全部原样保留）
#
# 用法:
#   export NIXIANG=/Users/aliang/Documents/nixiang
#   bash scripts/surgical_zjmc_1.0.20261111.sh
#   ZJMC_REBUILD_TPL=1 bash scripts/surgical_zjmc_1.0.20261111.sh   # 强制重跑 gradle（默认复用缓存的菜单模板 APK）
#
# 本版与「双轨制」历史版（surgical_zjmc_1.0.20261111_dualtrack.sh）的三处结构性差异：
#   ① **不注入 killer**：classes6.dex 只放星光类，不含 bin/mt/signature/KillerApplication193。
#      依据：skill/re-analyze/login/taptap-web-login.md「现行口径」（2026-11-11）——按包内 TapSDK
#      是否可达二选一，本包 TapSDK 明文全在 classes4 ⇒ 走**改包法**，运行时签名伪装/替换一律不用。
#   ② **Manifest 不改 appComponentFactory**：保持官方 androidx.core.app.CoreComponentFactory。
#      pre_install_gate 查12 ① 的实际判据是「manifest 仍挂 SignSpoofAppComponentFactory ⇒ FAIL」
#      （StarryAppComponentFactory 本身不在判据内，gate 仅把它列为「需早于 Application 时」的替代）。
#      Manifest 只做最小集：首位 <provider StarryInitProvider> + <service MenuService>(FGS specialUse)
#      + 两条权限 + resource map（由 zjmc_manifest_minimal.sh / v5 补丁器产出）。
#   ③ **TapSDK 三处静态补丁**（classes4.dex，改包法 1–3 步，见 patch_zjmc_tapsdk_static.py）：
#      GatekeeperRepository.isGatekeeperLoadedSuccessfully()Z → const/4 v0,0x1
#      GatekeeperRepository.showErrorToast(TapNetworkException)V → return-void
#      DefaultTapLoginFragmentProvider.provide → aget v2,v3,v2 之后 const/4 v2,0x3（LoginTypes.WEB）
#      TapSignKit 保持官方原样（同一链条禁双修）。
#
# 不变部分（与双轨版逐条相同）：so 走游戏私有 mk Android_zjmc.mk；星光类落点＝新增 classes6.dex
#   （官方 5 dex method_ids 余量 6/25/54/30/21 全部 ≤54，合并路线不可行）；入口走 ContentProvider；
#   官方 classes/2/3/5.dex 与 resources.arsc 必须逐字节不变（脚本内 sha256 断言）。
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
[ -f "$NIXIANG/04_scripts/_common/env.sh" ] && source "$NIXIANG/04_scripts/_common/env.sh"

VERSION="1.0.20261111"
VC="104502"
META="$NIXIANG/04_scripts/追迹迷城/official_apk.json"
if [[ -n "${ZJMC_ORIG_APK:-}" ]]; then
  ORIG_APK="$ZJMC_ORIG_APK"
else
  ORIG_APK="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1],encoding="utf-8"))["apk_path"])' "$META")"
fi
OUT="$NIXIANG/08_output/追迹迷城"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
if [ -n "${ANDROID_NDK_HOME:-}" ]; then
  NDK="$ANDROID_NDK_HOME"
elif [ "$NIXIANG_HOST" = "win" ]; then
  NDK="$(ls -d "$NIXIANG/01_tools/android-sdk/ndk/"* 2>/dev/null | sort -V | tail -1 || true)"
else
  NDK="$HOME/Library/Android/sdk/ndk/27.0.12077973"
fi
case "$NIXIANG_HOST" in
  win) NDK_PREBUILT="windows-x86_64" ;;
  *)   NDK_PREBUILT="darwin-x86_64" ;;
esac
NDK_CXX="$NDK/toolchains/llvm/prebuilt/$NDK_PREBUILT/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.TeaMelo.DungeonChasers"
SIGNED="$OUT/追迹迷城_${VERSION}_starry-singletrack-final-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/追迹迷城_${VERSION}_starry-singletrack-final-debugSigned.apk"
WORK_CACHE="$NIXIANG/07_patched/追迹迷城/surgical_${VERSION}_singletrack"
MF_CACHE="$WORK_CACHE/AndroidManifest.xml"
TPL_APK="$WORK_CACHE/starry-app-$BUILD_TYPE.apk"
TAPSDK_PATCHER="$NIXIANG/04_scripts/追迹迷城/patch_zjmc_tapsdk_static.py"
MERGE_DEX="classes6.dex"
SO_MD5_EXPECT="${ZJMC_SO_MD5:-}"

export JAVA_HOME="${JAVA_HOME:-${NIXIANG_JAVA17:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT="${NIXIANG_BUILD_TOOLS:-}"
if [ -z "$BT" ]; then
  for v in 36.0.0 35.0.0 34.0.0; do
    if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
      BT="$HOME/Library/Android/sdk/build-tools/$v"; break
    fi
  done
fi
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}（底包见 10_docs/_state/底包再获取台账.md）"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
[[ -f "$TAPSDK_PATCHER" ]] || { echo "ERROR: missing ${TAPSDK_PATCHER}"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android_zjmc.mk"
[[ -f "$MK" ]] || { echo "ERROR: missing ${MK}（游戏私有 mk）"; exit 1; }
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ZJMC' "$MK" \
  || { echo "ERROR: Android_zjmc.mk must enable STARRY_GAME_ZJMC"; exit 1; }
echo "[check] Android_zjmc.mk target=zjmc"

# 底包身份核对（防拿错版本的包出包）
python3 - "$ORIG_APK" "$VERSION" "$VC" <<'PY'
import sys, zipfile, struct
apk, ver, vc = sys.argv[1], sys.argv[2], sys.argv[3]
z = zipfile.ZipFile(apk)
names = z.namelist()
dexes = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))
print("  底包 dex 布局:", dexes)
assert dexes == ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex"], dexes
assert "classes6.dex" not in names, "底包已存在 classes6.dex，新 dex 编号需顺延"
def hdr(n):
    d = z.read(n)
    return struct.unpack_from("<20I", d, 0x20)[14]
for n in dexes:
    print("  %-14s method_ids=%6d spare=%d" % (n, hdr(n), 65536 - hdr(n)))
print("  目标版本 %s / vc%s 与底包 dex 布局断言通过" % (ver, vc))
PY

LOCK_DIR="/tmp/zjmc_surgical_singletrack.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zjmc_singletrack.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$WORK_CACHE"

echo "[1/8] verify build_so -> jniLibs（本版不重编 native）"
SO_BUILT="$STARRY_SRC/libs_zjmc/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT; run 04_scripts/追迹迷城/zjmc_build_so.sh first"; exit 1; }
LC_ALL=C grep -a -q '\[Zjmc\]' "$SO_BUILT" || { echo "ERROR: so 不是 ZJMC"; exit 1; }
for lbl in 秒杀 无敌 全屏秒杀 无限技能 免广告; do
  LC_ALL=C grep -a -q "Toggle_${lbl}" "$SO_BUILT" || { echo "ERROR: so 缺菜单项 Toggle_${lbl}"; exit 1; }
done
LC_ALL=C grep -a -q 'AT.GetFreeSkipAds' "$SO_BUILT" || { echo "ERROR: so 缺少免广告 Hook 标签"; exit 1; }
if LC_ALL=C grep -a -q '\[Cstjjs\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款 [Cstjjs]"; exit 1
fi
SO_MD5="$(md5 -q "$SO_BUILT")"
if [[ -n "$SO_MD5_EXPECT" && "$SO_MD5" != "$SO_MD5_EXPECT" ]]; then
  echo "ERROR: so md5=$SO_MD5 与期望 $SO_MD5_EXPECT 不符"; exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok（[Zjmc] + 五项菜单串 + 免广告标签；md5=${SO_MD5}）"

echo "[2/8] starry menu template（默认复用缓存 APK，ZJMC_REBUILD_TPL=1 强制重跑 gradle）"
if [[ -f "$TPL_APK" && "${ZJMC_REBUILD_TPL:-0}" != "1" ]]; then
  echo "  reuse cached template: $TPL_APK"
else
  rsync -a --delete \
    --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
    --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
    "$STARRY_SRC/" "$WORK/starry/"
  (cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
  cp -f "$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME" "$TPL_APK"
  echo "  built template -> $TPL_APK"
fi
STARRY_APK="$TPL_APK"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing starry template APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q '\[Zjmc\]' "$WORK/check.so" || { echo "ERROR: 模板 APK 内 so 不是 ZJMC"; exit 1; }

echo "[3/8] apktool d starry template + collect starry smali（不含 killer）"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing"; exit 1; }
if [[ -d "$SMALI_ROOT/bin" ]]; then
  echo "ERROR: 星光 smali 树里出现 bin/（应为空——本版禁止 killer）"; exit 1
fi
echo "  starry smali collected（$(find "$SMALI_ROOT" -name '*.smali' | wc -l | tr -d ' ') files）"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/8] classes4.dex：TapSDK 改包法三处静态补丁"
python3 - "$ORIG_APK" "$WORK" "$BAKSMALI_BIN" "$SMALI_BIN" "$TAPSDK_PATCHER" "classes4.dex" <<'PY'
import shutil, subprocess, sys, zipfile
from pathlib import Path
apk, work, baksmali, smali, patcher, dex_name = (Path(sys.argv[1]), Path(sys.argv[2]),
                                                 sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6])
with zipfile.ZipFile(apk) as z:
    src = work / ("orig_" + dex_name)
    src.write_bytes(z.read(dex_name))
unpacked = work / "tap_smali"
if unpacked.exists():
    shutil.rmtree(unpacked)
subprocess.run([baksmali, "d", "-o", str(unpacked), str(src)], check=True)
subprocess.run([sys.executable, patcher, str(unpacked)], check=True)
subprocess.run([smali, "assemble", "-o", str(work / "tap_patched.dex"), str(unpacked)], check=True)
print("  classes4 三处补丁已汇编 -> tap_patched.dex")
PY
[[ -f "$WORK/tap_patched.dex" ]] || { echo "ERROR: classes4 改包失败"; exit 1; }

echo "[5/8] assemble starry smali -> 新增 ${MERGE_DEX}（仅星光，无 killer）"
"$SMALI_BIN" assemble -o "$WORK/$MERGE_DEX" "$SMALI_ROOT"
python3 - "$WORK/$MERGE_DEX" <<'PY'
import struct, sys
d = open(sys.argv[1], "rb").read()
assert d[:8] in (b"dex\n035\x00", b"dex\n037\x00", b"dex\n039\x00"), d[:8]
f = struct.unpack_from("<20I", d, 0x20)
print("  新 dex size=%d method_ids=%d/65536 class_defs=%d" % (len(d), f[14], f[16]))
assert f[14] < 65536, f[14]
assert f[16] >= 190, f[16]
need = [b"com/starry/magic/StarryApp", b"com/starry/magic/menu/MenuService",
        b"com/starry/magic/StarryInitProvider"]
for s in need:
    assert s in d, s
assert any(s in d for s in (b"starry_magic", b"libstarry_magic", b"SecShell")), \
       "新 dex 未见 so 加载串"
# 单轨红线：不得含 killer 类定义，也不得含已删除的 SignSpoof
for banned in (b"Lbin/mt/", b"bin/mt/signature/KillerApplication193;",
               b"Lcom/starry/magic/sign/SignSpoof;"):
    assert banned not in d, banned
print("  新 dex 自检通过（星光入口齐备；无 bin/mt killer；无 SignSpoof）")
PY

echo "[6/8] 最小 Manifest（不改 appComponentFactory）"
if [[ -n "${ZJMC_BINARY_MANIFEST:-}" && -f "${ZJMC_BINARY_MANIFEST:-}" ]]; then
  cp "$ZJMC_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using ZJMC_BINARY_MANIFEST=$ZJMC_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "ERROR: missing ${MF_CACHE}（先跑 bash 04_scripts/追迹迷城/zjmc_manifest_minimal.sh）"
  exit 1
fi
python3 - "$WORK/AndroidManifest.xml" <<'PY'
import sys
d = open(sys.argv[1], "rb").read()
for s in ("com.starry.magic.StarryInitProvider",
          "com.TeaMelo.DungeonChasers.starryinit",
          "com.starry.magic.menu.MenuService",
          "androidx.core.app.CoreComponentFactory",
          "android.permission.SYSTEM_ALERT_WINDOW",
          "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
          "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"):
    assert s.encode("utf-16-le") in d, s
# 单轨红线：不改 Factory、不挂 killer 工厂、不注入 android:name
assert "StarryAppComponentFactory".encode("utf-16-le") not in d, "manifest 不得挂 killer 工厂"
assert "SignSpoofAppComponentFactory".encode("utf-16-le") not in d
assert "com.starry.magic.StarryApp\x00".encode("utf-16-le") not in d, "不得注入 android:name=StarryApp"
print("  manifest 自检通过（Factory 保持 CoreComponentFactory + Provider + MenuService + 两权限）")
PY

echo "[7/8] zip surgical merge（Manifest + classes4 补丁 + 新增 ${MERGE_DEX} + so/assets）"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
# ── 旧 v1 签名清理（2026-11-11 修复：原 'META-INF/*' 会误删官方 85 条业务条目）─────────
# 只删**顶层旧签名元数据**：META-INF/MANIFEST.MF、META-INF/<名>.SF|.RSA|.DSA|.EC、META-INF/SIG-*。
# 不匹配任何子目录 ⇒ META-INF/services/*（ServiceLoader 注册）、META-INF/kotlinx_*、
# META-INF/androidx.*、META-INF/com/unity3d/ads-mediation/**/verification.properties 全部保留。
# 官方 Gradle 包签名在 APK Signing Block（无 META-INF 签名文件）⇒ 本包清单为空，此处直接跳过删除。
OLD_SIG_LIST="$WORK/meta_inf_old_signers.txt"
python3 - "$ORIG_APK" "$OLD_SIG_LIST" <<'PY'
import re, sys, zipfile
apk, out = sys.argv[1], sys.argv[2]
SIG = re.compile(r"^META-INF/(?:MANIFEST\.MF|[^/]+\.(?:SF|RSA|DSA|EC)|SIG-[^/]+)$")
with zipfile.ZipFile(apk) as z:
    names = z.namelist()
assert len(names) == len(set(names)), "底包存在重复 ZIP 路径"
meta = [n for n in names if n.startswith("META-INF/")]
old = [n for n in meta if SIG.match(n)]
keep = [n for n in meta if n not in set(old)]
print("  官方底包 META-INF: %d 条（旧签名 %d / 业务条目 %d）" % (len(meta), len(old), len(keep)))
for n in old:
    print("    旧签名待删: " + n)
open(out, "w", encoding="utf-8").write("".join(n + "\n" for n in old))
assert not any("/" in n[len("META-INF/"):] for n in old), "旧签名清单不得含子目录条目"
assert len(old) + len(keep) == len(meta)
PY
OLD_SIG_ARGS=()
while IFS= read -r e; do
  [[ -n "$e" ]] && OLD_SIG_ARGS+=("$e")
done < "$OLD_SIG_LIST"
if [[ ${#OLD_SIG_ARGS[@]} -gt 0 ]]; then
  # 删除前二次守卫：逐条复核形态，任何非顶层签名条目立即中止
  for e in "${OLD_SIG_ARGS[@]}"; do
    case "$e" in
      META-INF/MANIFEST.MF) ;;
      META-INF/*.SF|META-INF/*.RSA|META-INF/*.DSA|META-INF/*.EC) ;;
      META-INF/SIG-*) ;;
      *) echo "ERROR: 旧签名清单出现非签名条目: $e"; exit 1 ;;
    esac
    case "${e#META-INF/}" in */*) echo "ERROR: 旧签名条目含子目录: $e"; exit 1 ;; esac
  done
  zip -d "$WORK/unsigned.apk" "${OLD_SIG_ARGS[@]}" >/dev/null
  echo "  已删旧 v1 签名元数据 ${#OLD_SIG_ARGS[@]} 条（显式清单，未触碰 META-INF 子目录）"
else
  echo "  无旧 v1 签名条目（官方包签名在 APK Signing Block）⇒ 不执行删除，META-INF 业务条目全保留"
fi
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$WORK/$MERGE_DEX" "$STAGE/"
cp -f "$WORK/tap_patched.dex" "$STAGE/classes4.dex"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml "$MERGE_DEX" classes4.dex lib/arm64-v8a/libstarry_magic.so)
[[ -f "$STAGE/lib/arm64-v8a/libc++_shared.so" ]] && ZIP_ARGS+=(lib/arm64-v8a/libc++_shared.so)
if [[ -d "$WORK/assets/certs" ]]; then
  mkdir -p "$STAGE/assets/certs"
  cp -R "$WORK/assets/certs/." "$STAGE/assets/certs/"
  while IFS= read -r -d '' f; do
    ZIP_ARGS+=("${f#"$STAGE/"}")
  done < <(find "$STAGE/assets/certs" -type f -print0)
fi
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" ${ZIP_ARGS[@]+"${ZIP_ARGS[@]}"})

python3 - "$WORK/unsigned.apk" "$ORIG_APK" "$MERGE_DEX" "$BAKSMALI_BIN" "$TAPSDK_PATCHER" <<'PY'
import hashlib, subprocess, sys, tempfile, zipfile
from pathlib import Path
dst, src, merge_dex, baksmali, patcher = (Path(sys.argv[1]), Path(sys.argv[2]),
                                          sys.argv[3], sys.argv[4], sys.argv[5])
z, zo = zipfile.ZipFile(dst), zipfile.ZipFile(src)
import re
all_names = z.namelist()
assert len(all_names) == len(set(all_names)), "产物出现重复 ZIP 路径"
names = set(all_names)
# META-INF 业务条目必须原样保留（services / kotlinx / androidx / Unity Ads 等；仅顶层签名文件可不同）
SIGRE = re.compile(r"^META-INF/(?:MANIFEST\.MF|[^/]+\.(?:SF|RSA|DSA|EC)|SIG-[^/]+)$")
src_meta = [n for n in zo.namelist() if n.startswith("META-INF/") and not SIGRE.match(n)]
new_meta = set(n for n in names if n.startswith("META-INF/") and not SIGRE.match(n))
_lost = sorted(set(src_meta) - new_meta)
assert not _lost, "META-INF 业务条目被删: %s" % _lost
for n in src_meta:
    assert hashlib.sha256(zo.read(n)).hexdigest() == hashlib.sha256(z.read(n)).hexdigest(), \
        "META-INF 条目被改: " + n
print("  META-INF 业务条目 %d 条全部保留且 sha256 相等" % len(src_meta))
assert "lib/arm64-v8a/libstarry_magic.so" in names
for n in ["classes.dex", "classes2.dex", "classes3.dex", "classes5.dex", "resources.arsc"]:
    a = hashlib.sha256(zo.read(n)).hexdigest()
    b = hashlib.sha256(z.read(n)).hexdigest()
    assert a == b, "%s 被改动！" % n
assert hashlib.sha256(zo.read("classes4.dex")).hexdigest() != \
       hashlib.sha256(z.read("classes4.dex")).hexdigest(), "classes4.dex 未变 —— 改包法补丁没落上"
_tmp = Path(tempfile.mkdtemp())
(_tmp / "c4.dex").write_bytes(z.read("classes4.dex"))
subprocess.run([baksmali, "d", "-o", str(_tmp / "out"), str(_tmp / "c4.dex")], check=True)
# 三处补丁的形态断言复用同一份判据（单一真源；字段 setter 仍在本类，不能按「字段名消失」判门闩）
subprocess.run([sys.executable, str(patcher), "--verify", str(_tmp / "out")], check=True)
assert merge_dex not in zo.namelist(), "底包不应已有 classes6.dex"
assert merge_dex in names, "未写入 classes6.dex"
d6 = z.read(merge_dex)
assert b"com/starry/magic/menu/MenuService" in d6
for banned in (b"Lbin/mt/", b"Lcom/starry/magic/sign/SignSpoof;"):
    assert banned not in d6, banned
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zjmc]" in so and b"[Cstjjs]" not in so
print("  merged ok; 官方 classes/2/3/5.dex + arsc 逐字节一致; classes4 三处补丁在; "
      "新增 %s 仅星光（无 killer）; [Zjmc] so" % merge_dex)
PY

echo "[8/8] zipalign + sign -> $SIGNED"
ALIGNED="$WORK/aligned.apk"
"${NIXIANG_ZIPALIGN:-$BT/zipalign}" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
rm -f "$WORK/unsigned.apk"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"${NIXIANG_APKSIGNER:-$BT/apksigner}" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"${NIXIANG_ZIPALIGN:-$BT/zipalign}" -c -p 4 "$SIGNED" && echo "  zipalign -c 4 PASS"
"${NIXIANG_APKSIGNER:-$BT/apksigner}" verify --print-certs "$SIGNED" 2>&1 | head -8

python3 - "$SIGNED" "$MERGE_DEX" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
d6 = z.read(sys.argv[2])
for s in (b"com/starry/magic/StarryApp", b"com/starry/magic/StarryInitProvider",
          b"com/starry/magic/menu/MenuService"):
    assert s in d6, s
assert b"[Zjmc]" in z.read("lib/arm64-v8a/libstarry_magic.so")
print("  signed ok; 星光入口在 %s，菜单 so 为 [Zjmc]" % sys.argv[2])
PY
md5 -q "$SIGNED"
shasum -a 256 "$SIGNED" | awk '{print $1}'
ls -l "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "ERROR: 本轮未获装机/卸载授权（异签名包需卸载官方包）。禁止在脚本内 install。"
  echo "       如需装机须用户明确授权后走 04_scripts/pipeline.sh 的 install 阶段。"
  exit 1
fi
echo "DONE $SIGNED"
