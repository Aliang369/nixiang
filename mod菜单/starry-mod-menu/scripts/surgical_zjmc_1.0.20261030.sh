#!/usr/bin/env bash
# 追迹迷城 1.0.20261030（vc103301，TapTap）— 外科组装（禁止对游戏 apktool b 整包）
#
# 产物：08_output/追迹迷城/追迹迷城_1.0.20261030_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_zjmc_1.0.20261030.sh
#   STARRY_INSTALL=1 bash scripts/surgical_zjmc_1.0.20261030.sh
#
# 2026-11-02「双轨制」版（依据 AGENTS §3 TapTap 条目 + skill/re-analyze/login/taptap-web-login.md「现行口径」
# + 10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md「2026-10-29 增补」）：
#   · B 轨 签名/完整性＝运行时签名替换：MT ApkSignatureKillerEx 形态的
#     bin.mt.signature.KillerApplication193（已烘焙本包名 + 官方证书 DER）与星光 smali 一起装进
#     新增的 classes6.dex；挂载点＝Manifest appComponentFactory →
#     com.starry.magic.sign.StarryAppComponentFactory.instantiateApplication（早于 Application 实例化）。
#   · A 轨 登录方式＝改包法：classes4.dex 的
#     com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.provide 里
#     aget 之后强制 v2=3（LoginTypes.WEB）⇒ 恒走内嵌网页登录，绕开客户端 SSO 的签名互验。
#     （同一链条禁双修：TapSignKit.getSign 保持官方原样，签名只由 B 轨 killer 处理。）
#   · 与 1009 版的结构差异：星光类改为新增 classes6.dex（官方 5 个 dex 余量全部 ≤54，无法合并）；
#   ① 依据（2026-11-02 实测）：本版官方 5 个 dex 的 method_ids 余量全部 ≤54
#      （classes.dex 65530 余6 / classes2 65511 余25 / classes3 65482 余54 /
#        classes4 65506 余30 / classes5 65515 余21）⇒ 1009 的「合并进 classes4.dex」
#      路线整体不可行（星光 smali 需 2k+ method）。唯一可行落点＝新增 classes6.dex，
#      编号紧接最后一个官方 dex（ART 按 classesN 连续加载；跳号才有「自动发现失效」风险）。
#   ② 不变部分（与 1009 逐条相同、无需重做试验）：入口仍走 ContentProvider
#      （com.starry.magic.StarryInitProvider，Provider 先于 Activity 实例化，内部做
#      SignSpoof.install() + StarryNativeLoad.ensureLoaded() + 菜单拉起）；Manifest 仍走
#      v3 补丁器（新增 MenuService + provider + FGS specialUse/悬浮窗权限，resource map
#      追加基址已修正）；官方 dex 与 resources.arsc **必须逐字节不变**（脚本内 sha256 断言）。
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

VERSION="1.0.20261030"
VC="103301"
META="$NIXIANG/04_scripts/追迹迷城/official_apk.json"
# 底包路径以 official_apk.json 的 apk_path 为准（AGENTS：APK 不留本地，底包常驻 /tmp 临时副本）
if [[ -n "${ZJMC_ORIG_APK:-}" ]]; then
  ORIG_APK="$ZJMC_ORIG_APK"
else
  ORIG_APK="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1],encoding="utf-8"))["apk_path"])' "$META")"
fi
OUT="$NIXIANG/08_output/追迹迷城"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.TeaMelo.DungeonChasers"
SIGNED="$OUT/追迹迷城_${VERSION}_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/追迹迷城_${VERSION}_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/追迹迷城/surgical_$VERSION/AndroidManifest.xml"
MERGE_DEX=""            # 本版固定 classes6.dex（官方 5 dex 余量均 ≤54，见文件头 ①），在第 97 行赋值
SO_MD5_EXPECT="${ZJMC_SO_MD5:-}"
# B 轨签名杀手 dex：由 04_scripts/追迹迷城/build_killer_dex_zjmc.sh 从参数化 smali + 官方证书烘焙
KILLER_ASSET="$NIXIANG/04_scripts/追迹迷城/assets/zjmc_signature_killer.dex"
KILLER_MD5="${ZJMC_KILLER_MD5:-a41a5681f7e1c2f8f8a7473c57f54014}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}（底包是 /tmp 临时副本，用完即删；见 10_docs/_state/底包再获取台账.md）"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
[[ -f "$KILLER_ASSET" ]] || { echo "ERROR: missing killer dex ${KILLER_ASSET}（先跑 04_scripts/追迹迷城/build_killer_dex_zjmc.sh）"; exit 1; }
[[ "$(md5 -q "$KILLER_ASSET")" == "$KILLER_MD5" ]] || { echo "ERROR: killer dex md5 与记录不符（证书/包名变更请重烘并更新 KILLER_MD5）"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ZJMC' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_ZJMC"; exit 1; }
echo "[check] Android.mk target=zjmc"

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
    f = struct.unpack_from("<20I", d, 0x20)
    return f[14]  # method_ids_size
for n in dexes:
    print(f"  {n:14s} method_ids={hdr(n):6d} spare={65536-hdr(n)}")
print(f"  目标版本 {ver} / vc{vc} 与底包 dex 布局断言通过")
PY

# 星光类落点：新增 classes6.dex（本版官方 5 dex 余量均 ≤54，合并路线不可行）
MERGE_DEX="classes6.dex"
echo "[dex] 星光类落点 = 新增 ${MERGE_DEX}（官方 classes.dex..classes5.dex 一个不动；ART 连续加载）"

LOCK_DIR="/tmp/zjmc_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zjmc_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] verify build_so -> jniLibs"
SO_BUILT="$STARRY_SRC/libs_zjmc/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT; run 04_scripts/追迹迷城/zjmc_build_so.sh first"; exit 1; }
LC_ALL=C grep -a -q '\[Zjmc\]' "$SO_BUILT" || { echo "ERROR: so 不是 ZJMC"; exit 1; }
LC_ALL=C grep -a -q 'Toggle_免广告' "$SO_BUILT" || { echo "ERROR: so 缺少免广告菜单项"; exit 1; }
LC_ALL=C grep -a -q 'AT.GetFreeSkipAds' "$SO_BUILT" || { echo "ERROR: so 缺少免广告 Hook 标签"; exit 1; }
if LC_ALL=C grep -a -q '\[Cstjjs\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款 [Cstjjs]"
  exit 1
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
echo "  so -> jniLibs ok ([Zjmc] + 免广告 verified, md5=$SO_MD5)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Zjmc\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 ZJMC"; exit 1
fi

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] collect starry smali (将合并进 ${MERGE_DEX})"
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
# B 轨：killer smali 一起装进同一个新 dex（killer 类必须能在 instantiateApplication 时 Class.forName 到）
"$BAKSMALI_BIN" d -o "$WORK/killer_smali" "$KILLER_ASSET" >/dev/null
rsync -a "$WORK/killer_smali/bin" "$SMALI_ROOT/"
[[ -f "$SMALI_ROOT/bin/mt/signature/KillerApplication193.smali" ]] \
  || { echo "ERROR: killer smali missing after unpack"; exit 1; }
echo "  starry smali collected ($(find "$SMALI_ROOT" -name '*.smali' | wc -l | tr -d ' ') files, 含 killer)"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3b/7] A 轨改包法：classes4.dex 的 DefaultTapLoginFragmentProvider.provide → 恒 WEB 登录"
python3 - "$ORIG_APK" "$WORK" "$BAKSMALI_BIN" "$SMALI_BIN" "classes4.dex" <<'PY'
import shutil
import subprocess
import sys
import zipfile
from pathlib import Path

apk, work, baksmali, smali, dex_name = (Path(sys.argv[1]), Path(sys.argv[2]),
                                        sys.argv[3], sys.argv[4], sys.argv[5])
with zipfile.ZipFile(apk) as z:
    src = work / ("orig_" + dex_name)
    src.write_bytes(z.read(dex_name))
unpacked = work / "tap_smali"
if unpacked.exists():
    shutil.rmtree(unpacked)
subprocess.run([baksmali, "d", "-o", str(unpacked), str(src)], check=True)

target = unpacked / "com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali"
if not target.is_file():
    raise SystemExit("missing " + str(target))
text = target.read_text(encoding="utf-8")
anchor = "    aget v2, v3, v2\n"
if anchor not in text:
    raise SystemExit("provide() 的 aget 锚点缺失 —— 底包 TapSDK 结构变了，禁按旧锚点硬改")
_after = text.split(anchor, 1)[1][:200]
if "const/4 v2, 0x3" in _after:
    raise SystemExit("classes4.dex 已被 A 轨补丁过（勿二次组装）")
text = text.replace(anchor, anchor + "\n    # forced loginType=WEB by zjmc surgical (A 轨改包法)\n    const/4 v2, 0x3\n", 1)
target.write_text(text, encoding="utf-8")

# 同一链条禁双修：签名只由 B 轨 killer 处理，TapSignKit 必须保持官方原样
sk = unpacked / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not sk.is_file():
    raise SystemExit("missing TapSignKit.smali")
sk_text = sk.read_text(encoding="utf-8")
for needle in ("4684a2b2249e978c74230f8895635894", "b9c07db608080d40b04254d32ac3fc5c"):
    if needle in sk_text:
        raise SystemExit("TapSignKit.getSign 被静态改成官方摘要 —— 违反双轨制同一链条禁双修")
subprocess.run([smali, "assemble", "-o", str(work / "tap_patched.dex"), str(unpacked)], check=True)
print("  patched provide() -> WebLoginFragment；TapSignKit 保持官方原样")
PY
[[ -f "$WORK/tap_patched.dex" ]] || { echo "ERROR: A 轨改包失败"; exit 1; }

echo "[4/7] assemble starry smali + killer smali → 新增 ${MERGE_DEX}"
# 为什么本版必须新增 dex：官方 5 个 dex 的 method_ids 余量全部 ≤54（见文件头 ①），
# 合并路线不可行；classes6.dex 紧接 classes5.dex ⇒ ART 连续加载成立。
"$SMALI_BIN" assemble -o "$WORK/$MERGE_DEX" "$SMALI_ROOT"
python3 - "$WORK/$MERGE_DEX" <<'PY'
import struct, sys
p = sys.argv[1]
d = open(p, "rb").read()
assert d[:8] in (b"dex\n035\x00", b"dex\n037\x00", b"dex\n039\x00"), d[:8]
f = struct.unpack_from("<20I", d, 0x20)
print(f"  新 dex size={len(d)} method_ids={f[14]}/65536 class_defs={f[16]}")
assert f[14] < 65536, f[14]
assert f[16] >= 190, f[16]   # 星光 199 个类（1009 版合并时同为 187→5685 的增量口径）
need = [b"com/starry/magic/StarryApp", b"com/starry/magic/menu/MenuService",
        b"com/starry/magic/StarryInitProvider",
        b"com/starry/magic/sign/StarryAppComponentFactory",   # B 轨挂载点
        b"com/starry/magic/ota/StarryNativeLoad",
        b"bin/mt/signature/KillerApplication193"]             # B 轨 killer
for s in need:
    assert s in d, s
# 双轨制红线：已删除且禁止复活的类不得出现在产物里
for banned in (b"Lcom/starry/magic/sign/SignSpoof;",
               b"Lcom/starry/magic/sign/SignSpoofAppComponentFactory;"):
    assert banned not in d, banned
print("  新 dex 自检通过（星光入口 + killer 齐备；无 SignSpoof；官方 dex 未被改动）")
PY

echo "[5/7] binary AndroidManifest (StarryInitProvider + MenuService, v3/v4 补丁产物)"
# 硬规则：禁止对游戏包 apktool d/b（见 apk-build-surgical.mdc）。
if [[ -n "${ZJMC_BINARY_MANIFEST:-}" && -f "$ZJMC_BINARY_MANIFEST" ]]; then
  cp "$ZJMC_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using ZJMC_BINARY_MANIFEST=$ZJMC_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "ERROR: missing $MF_CACHE"
  echo "  禁止在本脚本内对游戏包 apktool d/b。请先生成并缓存："
  echo "    bash 04_scripts/追迹迷城/zjmc_manifest.sh"
  exit 1
fi
python3 - "$WORK/AndroidManifest.xml" "$MERGE_DEX" <<'PY'
import sys
d = open(sys.argv[1], "rb").read()
new_dex = sys.argv[2].encode()
for s in ("com.starry.magic.sign.StarryAppComponentFactory",   # B 轨挂载点
          "com.starry.magic.StarryInitProvider",
          "com.starry.magic.menu.MenuService",
          "com.TeaMelo.DungeonChasers.starryinit",
          "android.permission.SYSTEM_ALERT_WINDOW",
          "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
          "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"):
    assert s.encode("utf-16-le") in d, s
# 双轨制红线：不得挂已删除的 SignSpoof 工厂，也不得注入 android:name=StarryApp
assert "SignSpoofAppComponentFactory".encode("utf-16-le") not in d
assert "com.starry.magic.StarryApp\x00".encode("utf-16-le") not in d
assert new_dex not in d  # Manifest 不需要声明 dex 名
print("  manifest 自检通过（appComponentFactory→StarryAppComponentFactory + Provider + MenuService + 权限）")
PY

echo "[6/7] zip surgical merge（Manifest + A 轨 classes4.dex + 新增 ${MERGE_DEX} + so/assets）"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
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
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" "${ZIP_ARGS[@]}")

python3.11 - "$WORK/unsigned.apk" "$ORIG_APK" "$MERGE_DEX" "$BAKSMALI_BIN" <<'PY'
import sys, zipfile, hashlib, os, subprocess, tempfile
from pathlib import Path
dst, src, merge_dex, baksmali = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3], sys.argv[4]
z = zipfile.ZipFile(dst); zo = zipfile.ZipFile(src)
names = set(z.namelist())
assert "lib/arm64-v8a/libstarry_magic.so" in names
d4 = z.read(merge_dex)
assert b"com/starry/magic/StarryApp" in d4 and b"com/starry/magic/menu/MenuService" in d4
assert b"com/starry/magic/StarryInitProvider" in d4
# 外科铁证（双轨制版）：允许改 = Manifest + classes4.dex（A 轨登录改包）+ 新增 classes6.dex + so/assets；
# 其余官方条目（classes/2/3/5.dex、resources.arsc）必须逐字节未变。
for n in ["classes.dex", "classes2.dex", "classes3.dex", "classes5.dex", "resources.arsc"]:
    a, b = hashlib.sha256(zo.read(n)).hexdigest(), hashlib.sha256(z.read(n)).hexdigest()
    assert a == b, f"{n} 被改动！"
assert hashlib.sha256(zo.read("classes4.dex")).hexdigest() != \
       hashlib.sha256(z.read("classes4.dex")).hexdigest(), "classes4.dex 未变 —— A 轨登录补丁没落上"
# A 轨锚点是 smali 注释 + 指令，汇编后注释不保留 ⇒ 回读反汇编判定
_tmp = Path(tempfile.mkdtemp())
(_tmp / "c4.dex").write_bytes(z.read("classes4.dex"))
subprocess.run([baksmali, "d", "-o", str(_tmp / "out"), str(_tmp / "c4.dex")], check=True)
_txt = (_tmp / "out/com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali").read_text(encoding="utf-8")
assert "const/4 v2, 0x3" in _txt.split("aget v2, v3, v2")[1][:200], \
       "classes4.dex 缺 A 轨锚点（provide 未强制 WEB）"
assert b"GatekeeperRepository" in zo.read("classes4.dex"), "底包 classes4.dex 无 GatekeeperRepository（门禁查12 预期不符）"
assert merge_dex not in zo.namelist(), "底包不应已有 classes6.dex"
assert merge_dex in names, "未写入 classes6.dex"
d6 = z.read(merge_dex)
assert b"bin/mt/signature/KillerApplication193" in d6, "classes6.dex 缺 killer（B 轨签名替换未注入）"
assert b"com/starry/magic/sign/StarryAppComponentFactory" in d6, "classes6.dex 缺 StarryAppComponentFactory"
assert b"Lcom/starry/magic/sign/SignSpoof;" not in d6, "classes6.dex 含已删除的 SignSpoof（禁止复活）"
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zjmc]" in so and b"[Cstjjs]" not in so
print(f"  merged ok; 官方 classes/2/3/5.dex + arsc 逐字节一致; classes4.dex 已打 A 轨登录补丁; "
      f"新增 {merge_dex}（星光+killer）; [Zjmc] so")
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
rm -f "$WORK/unsigned.apk"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -8

python3.11 - "$SIGNED" "$MERGE_DEX" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
d4 = z.read(sys.argv[2])
assert b"com/starry/magic/StarryApp" in d4
assert b"com/starry/magic/StarryInitProvider" in d4
assert b"com/starry/magic/menu/MenuService" in d4
assert b"[Zjmc]" in z.read("lib/arm64-v8a/libstarry_magic.so")
print("  signed ok; 星光入口在 %s，菜单 so 为 [Zjmc]" % sys.argv[2])
PY
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install"
  REMOTE=/data/local/tmp/starry_zjmc.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""; then
    echo "ERROR: pm install failed（多半是异签名 INSTALL_FAILED_UPDATE_INCOMPATIBLE）"
    echo "       官方包签名为 O=深圳缇米乐科技有限公司,CN=蒋帅，本包为 CN=StarryMagic；"
    echo "       覆盖安装不可能成功，需用户明确同意后 uninstall 再装。"
    exit 1
  fi
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
fi
echo "DONE $SIGNED"
