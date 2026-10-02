#!/usr/bin/env bash
# 境界-死神激斗 1.38.2（com.cmge.ssjd.x7sy，小七渠道）— 外科组装
#   官方 classes.dex..classes8.dex / resources.arsc / libil2cpp.so 字节级保留，只增不改。
#
# 产物：08_output/境界死神激斗/境界死神激斗_1.38.2_surgical-releaseSigned.apk
#
# 方案（骨架复刻 surgical_phigros_3.20.0.sh 的大包路径 + surgical_yntt_739.sh 的
# ContentProvider 注入路径）：
#   - 官方 Manifest 的 application@name = com.excelliance.open.LBApplication（乐变热更框架）
#     → 绝不改 application@name，走 ContentProvider 注入：
#         StarryInitProvider + android:initOrder="2147483647"（系统最先初始化，让
#         SignSpoof.install() 早于 LBApplication 之后的 SDK/provider 读签名）
#   - 官方包 8 个 dex 连续占用 classes.dex..classes8.dex → 星光 smali → classes9.dex
#     （ART 连续 dex 规则；新增永不写死下标，由 next_free_dex.py 复核）
#   - 官方已带 SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE → 只补 FOREGROUND_SERVICE_SPECIAL_USE
#     （MenuService 前台化 + Android 14 specialUse 类型前提）
#   - 二进制 Manifest：迷你 APK（流式剔除 META-INF/assets/lib/dex，2.47G 不整包 decode）
#     → apktool d -s → 改文本 → apktool b → aapt2 xmltree 官方节点零丢失门禁 →
#     缓存到 07_patched/境界死神激斗/surgical_1.38.2/AndroidManifest.xml
#   - zip 合并：python 流式逐条复制（zin.open → copyfileobj → zout.open，2.47G 不整体读内存）
#     + normalize_zip（python zipfile 的 ZIP64_LIMIT=2^31-1 会把 >2G 产物写成 zip64
#       EOCD/晚期条目 extra，zipalign 36.0.0 走 zip64 路径对 >2G 直接 "Unable to open"）
#     + 双自检：官方 dex/arsc/libil2cpp sha256 相等 + 全量 CRC/size/ctype/名字字节/utf8 flag 一致
#   - 原包仅 v1 签名（META-INF/X7GAME.{SF,RSA}，无 APK Signing Block）→ 重签即闭环
#
# 用法:
#   bash scripts/surgical_jjssjd_1.38.2.sh                 # 只出包，不装机
#   STARRY_INSTALL=1 bash scripts/surgical_jjssjd_1.38.2.sh
#
# 底包（仓内不留 2.47G 官包，用完即删）：
#   重取 = adb -s f9a116a2 pull /data/app/*/com.cmge.ssjd.x7sy-*/base.apk /tmp/ssjd/base.apk
#   md5=befa9c606746fe8fa5d084ece3547949 sha256=1249a01296005a460ce7d68f279151b488e84d4d3d28906e0d3fe43216b62a34
#   登记见 10_docs/_state/底包再获取台账.md
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"

GAME="境界死神激斗"
VER="1.38.2"
PKG="com.cmge.ssjd.x7sy"
TAG="Jjssjd"
GAME_ID="jjssjd"
NEW_DEX="classes9.dex"

ORIG_APK="${STARRY_ORIG_APK:-/tmp/ssjd/base.apk}"
ORIG_MD5="befa9c606746fe8fa5d084ece3547949"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical_$VER/AndroidManifest.xml"
OUT="$NIXIANG/08_output/$GAME"
SIGNED="$OUT/${GAME}_${VER}_surgical-releaseSigned.apk"
TMP_UNSIGNED="$OUT/.tmp_unsigned.apk"
TMP_ALIGNED="$OUT/.tmp_aligned.apk"
SO_PREBUILT="$STARRY_SRC/libs_$GAME_ID/arm64-v8a/libstarry_magic.so"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
ADB="${ADB:-adb}"
SERIAL="${ANDROID_SERIAL:-f9a116a2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools zipalign missing"; exit 1; }
AAPT2="$BT/aapt2"

command -v python3.11 >/dev/null || { echo "ERROR: python3.11 missing"; exit 1; }
command -v apktool   >/dev/null || { echo "ERROR: apktool missing"; exit 1; }
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}（底包重取见脚本头注释）"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
TAG_CLASS="$(python3.11 -c 'import sys; print(sys.argv[1][:1].upper()+sys.argv[1][1:])' "$TAG")"   # Jjssjd
[[ -f "$STARRY_SRC/app/src/main/jni/Android_$GAME_ID.mk" ]] || { echo "ERROR: missing Android_$GAME_ID.mk"; exit 1; }
grep -q "Games/${TAG_CLASS}Mod.cpp" "$STARRY_SRC/app/src/main/jni/Android_$GAME_ID.mk" \
  || { echo "ERROR: Android_$GAME_ID.mk 未编入 Games/${TAG_CLASS}Mod.cpp"; exit 1; }

# 底包指纹复核（只取 md5，2.47G 约 8s；命中即证明是同版本官包）
GOT_MD5="$(md5 -q "$ORIG_APK")"
[[ "$GOT_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 底包 md5 不符 got=$GOT_MD5 want=$ORIG_MD5"; exit 1; }
echo "[check] orig apk md5 ok ($ORIG_MD5)"

# 新增 dex 下标不写死：最小空闲下标必须等于 NEW_DEX
FREE_DEX="classes$(python3.11 "$NIXIANG/04_scripts/_common/next_free_dex.py" "$ORIG_APK").dex"
[[ "$FREE_DEX" == "$NEW_DEX" ]] || { echo "ERROR: 最小空闲 dex=${FREE_DEX}，与脚本假定 $NEW_DEX 不符"; exit 1; }
echo "[check] free dex index = $NEW_DEX"

# 磁盘预检：峰值 = unsigned + aligned 两份全包并存 ≈5GiB
AVAIL_GB="$(df -g "$NIXIANG" | awk 'NR==2{print $4}')"
[[ "${AVAIL_GB:-0}" -ge 6 ]] || { echo "ERROR: 磁盘可用 ${AVAIL_GB:-0}G < 6G（峰值需两份全包并存）"; exit 1; }
echo "[check] disk avail=${AVAIL_GB}G ok"

LOCK_DIR="/tmp/jjssjd1382_surgical.lockdir"
mkdir "$LOCK_DIR" 2>/dev/null || { echo "ERROR: lock $LOCK_DIR held"; exit 1; }
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/jjssjd1382_surgical.XXXXXX)"
cleanup() {
  rm -rf "$WORK" "$LOCK_DIR" 2>/dev/null || true
  [[ -n "${KEEP_TMP:-}" ]] || rm -f "$TMP_UNSIGNED" "$TMP_ALIGNED" 2>/dev/null || true
}
trap cleanup EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build → libs_${GAME_ID}（so 必须是本游戏，防旧 so 污染）"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT="./Android_$GAME_ID.mk" \
    NDK_APPLICATION_MK=./Application.mk \
    NDK_OUT="$STARRY_SRC/obj_$GAME_ID" NDK_LIBS_OUT="$STARRY_SRC/libs_$GAME_ID" \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)" >"$WORK/ndk.log" 2>&1) \
  || { echo "ERROR: ndk-build 失败"; tail -30 "$WORK/ndk.log"; exit 1; }
grep -E 'error|undefined symbol' "$WORK/ndk.log" | head -20 || true
grep -qE 'BUILD|Install' "$WORK/ndk.log" || { echo "WARN: ndk.log 无构建完成标记"; tail -20 "$WORK/ndk.log"; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing $SO_PREBUILT"; exit 1; }
LC_ALL=C grep -aq "[$TAG]" "$SO_PREBUILT" || { echo "ERROR: so 无 [$TAG] 标签（可能仍是上一款游戏）"; exit 1; }
LC_ALL=C grep -aq "$GAME_ID" "$SO_PREBUILT" || { echo "ERROR: so 无 game_id=$GAME_ID"; exit 1; }
LC_ALL=C grep -aq '无敌' "$SO_PREBUILT" || { echo "ERROR: so 无菜单串（无敌）"; exit 1; }
for dest in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" "$STARRY_SRC/app/src/main/libs/arm64-v8a" "$STARRY_SRC/app/libs/arm64-v8a"; do
  mkdir -p "$dest"; cp -f "$SO_PREBUILT" "$dest/libstarry_magic.so"
done
echo "  so ok ($(ls -l "$SO_PREBUILT" | awk '{print $5}') B, [$TAG] verified)"

echo "[2/7] gradle assembleRelease（starry 小模板；externalNativeBuild 关闭，so 取自 jniLibs）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain 2>&1 | tail -3)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
LC_ALL=C grep -aq "[$TAG]" "$WORK/check.so" || { echo "ERROR: gradle APK 内 so 不是 $TAG"; exit 1; }

echo "[3/7] apktool d starry → com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for f in com/starry/magic/StarryInitProvider.smali com/starry/magic/menu/MenuService.smali; do
  [[ -f "$SMALI_ROOT/$f" ]] || { echo "ERROR: $f missing"; exit 1; }
done
grep -q "$PKG" "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" \
  || { echo "ERROR: SignSpoof.smali 内无 ${PKG}（证书未落进 smali）"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

echo "[3b/7] collect lib + certs from starry decode"
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
LC_ALL=C grep -aq "[$TAG]" "$SO_SRC" || { echo "ERROR: gradle 解包 so 无 [$TAG] 标签"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"; cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] binary AndroidManifest（ContentProvider 注入：保留 LBApplication，绝不改 application@name）"
if [[ -n "${STARRY_BINARY_MANIFEST:-}" && -f "${STARRY_BINARY_MANIFEST:-}" ]]; then
  cp "$STARRY_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using STARRY_BINARY_MANIFEST=$STARRY_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  mini apk（剔除 META-INF/assets/lib/dex，只留 res+arsc+manifest）→ apktool 往返"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
zin, zout = zipfile.ZipFile(src, "r"), zipfile.ZipFile(dst, "w")
kept = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith(("META-INF/", "assets/", "lib/")): continue
    if name.startswith("classes") and name.endswith(".dex"): continue
    info = zipfile.ZipInfo(item.orig_filename, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, zin.read(item))
    kept += 1
zout.close(); zin.close()
print(f"  mini apk: {kept} entries")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
if 'android:name="com.excelliance.open.LBApplication"' not in t:
    raise SystemExit("ERROR: 官方 application@name 不是 LBApplication，注入路线需重判（见 skill/re-analyze/environment-min-path.md §2）")
perms = []
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if 'android:name="android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0: raise SystemExit("ERROR: no <uses-permission> anchor")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
    print("  manifest: + " + ", ".join(x.split('"')[1] for x in perms))
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="2147483647" />
''' % pkg
    t = t.replace("</application>", prov + "\n    </application>", 1)
    print("  manifest: StarryInitProvider added (initOrder=2147483647)")
if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
    print("  manifest: MenuService added")
p.write_text(t, encoding="utf-8")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

fi
ls -la "$WORK/AndroidManifest.xml"

gate_manifest() {
  # 结构化门禁（aapt2 xmltree）：官方节点零丢失 + 三处新增齐备 + LBApplication 保留
  if [[ -x "$AAPT2" ]]; then
    "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt" 2>/dev/null
    python3.11 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
    "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null \
      || { echo "ERROR: aapt2 无法解析新 manifest"; exit 1; }
    grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing"; exit 1; }
    grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" || { echo "ERROR: service missing"; exit 1; }
    grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" || { echo "ERROR: FGS specialUse permission missing"; exit 1; }
    grep -q 'android:initOrder(0x0101001a)=2147483647' "$WORK/tree_new.txt" \
      || { echo "ERROR: StarryInitProvider 缺 initOrder=2147483647（签名伪装将晚于后续 provider/SDK init）"; exit 1; }
    grep -q 'com.excelliance.open.LBApplication' "$WORK/tree_new.txt" \
      || { echo "ERROR: 官方 LBApplication 丢失（注入路线铁律：绝不改 application@name）"; exit 1; }
    sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
    sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
    LOST="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
    [[ "$LOST" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $LOST 行"; exit 1; }
    echo "  aapt2 门禁: 官方节点零丢失 (lost=0), 新增 $(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^>' || true) 行"
  else
    echo "  WARN: aapt2 不可用，跳过结构化门禁"
  fi
}
gate_manifest
if [[ ! -f "$MF_CACHE" ]]; then
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi

echo "[5/7] zip surgical merge（流式逐条复制，官方 dex/arsc/libil2cpp 字节级保留）"
python3.11 - "$ORIG_APK" "$WORK" "$TMP_UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib, shutil, struct
from pathlib import Path

src, work, dst, new_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
extras = {}
libdir = work / "lib"
if libdir.is_dir():
    for p in sorted(libdir.rglob("*")):
        if p.is_file():
            extras["lib/" + str(p.relative_to(libdir)).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in sorted(certs.rglob("*")):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
assert "lib/arm64-v8a/libstarry_magic.so" in extras, "libstarry_magic.so not staged"

class RawNameZipInfo(zipfile.ZipInfo):
    """按原始字节写条目名（flag 不置 0x800）。
    Unity APK 里部分 AssetBundle 名是「UTF-8 字节但不带 UTF-8 flag」；python zipfile
    重写时会把名字重编码并强设 0x800 → Unity/Android zip 读取器路径对不上 →
    "Unable to open archive file" → 加载条卡死。"""
    _raw_name = b""
    _out_flags = 0x800   # python 在 _open_to_write 里会把 flag_bits 清零，必须另存
    def _encodeFilenameFlags(self):
        if self._raw_name:
            return self._raw_name, self._out_flags
        name, flags = super()._encodeFilenameFlags()
        return name, flags | self._out_flags

def make_info(item):
    raw = item.orig_filename.encode("cp437", "replace")
    info = RawNameZipInfo(item.orig_filename, item.date_time)
    if (item.flag_bits & 0x800) == 0 and raw != item.orig_filename.encode("ascii", "replace"):
        info._raw_name = raw
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    # 原包（Unity 产物）3025 条 flag 全部带 0x800(UTF-8)，其中 2380 条另带 0x08(data descriptor)。
    # 我侧一律写真实 local header（seekable，不写数据描述符）⇒ 显式保留 0x800、丢弃 0x08，
    # 否则 CD 声明了不存在的数据描述符，严格读取器会按描述符解析而错位。
    info._out_flags = item.flag_bits & ~0x08
    return info

zin = zipfile.ZipFile(src, "r")
orig_names = set(zin.namelist())
dup = (orig_names & set(extras)) | ({new_dex} & orig_names)
assert not dup, f"版本假设失效（原包已含注入项）: {sorted(dup)}"

zout = zipfile.ZipFile(dst, "w")
seen = set()
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):      # 官方 v1 签名（MANIFEST.MF / X7GAME.SF / X7GAME.RSA）整组丢弃
        continue
    if name in extras:
        continue
    info = make_info(item)
    if name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(info, mf)
    elif name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED   # Android 11+ 要求 arsc 未压缩
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    seen.add(item.orig_filename)
    n += 1

info = RawNameZipInfo(new_dex)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
info._out_flags = 0x800
zout.writestr(info, cnew)
seen.add(new_dex)

for name, data in sorted(extras.items()):
    einfo = RawNameZipInfo(name)
    einfo.compress_type = zipfile.ZIP_DEFLATED   # extractNativeLibs=true，DEFLATED 可安装
    einfo.create_system = 3
    einfo._out_flags = 0x800
    zout.writestr(einfo, data)
    seen.add(name)

zout.close(); zin.close()

def normalize_zip(path):
    """剥离 python zipfile 写入的 zip64 残留（ZIP64_LIMIT=2^31-1 会在 >2G 产物上触发）：
    还原逐条目真实 hoff（uint32 合法域）、vneed 回填本地头版本（消 BadPack header mismatch）、
    尾部改写纯经典 EOCD。不做这步 zipalign 36.0.0 走 zip64 路径会 "Unable to open"。"""
    f = open(path, "r+b")
    f.seek(0, 2); size = f.tell()
    f.seek(max(0, size - 65536)); tail = f.read()
    e = tail.rfind(b"PK\x05\x06")
    assert e >= 0, "EOCD not found"
    _, _, _, _, n2, cdsize, cdoff, clen = struct.unpack("<IHHHHIIH", tail[e:e+22])
    assert clen == 0, "unexpected EOCD comment"
    if cdoff == 0xFFFFFFFF or cdsize == 0xFFFFFFFF or n2 == 0xFFFF:
        e64 = e - 76
        assert e64 >= 0 and tail[e64:e64+4] == b"PK\x06\x06", "expected EOCD64 before EOCD"
        count64, = struct.unpack("<Q", tail[e64+32:e64+40])
        cdsize64, cdoff64 = struct.unpack("<QQ", tail[e64+40:e64+56])
        n2, cdsize, cdoff = count64, cdsize64, cdoff64
    f.seek(cdoff); cd = f.read(cdsize)
    out = bytearray(); pos = fixed = 0
    while pos < len(cd):
        assert cd[pos:pos+4] == b"PK\x01\x02", f"bad CD sig @{pos}"
        nlen, elen, clen2 = struct.unpack_from("<HHH", cd, pos+28)
        hoff = struct.unpack_from("<I", cd, pos+42)[0]
        vneed = struct.unpack_from("<H", cd, pos+6)[0]
        total = 46 + nlen + elen + clen2
        rec = bytearray(cd[pos:pos+total])
        if hoff == 0xFFFFFFFF:
            name = cd[pos+46:pos+46+nlen]
            extra = cd[pos+46+nlen:pos+46+nlen+elen]
            comment = cd[pos+46+nlen+elen:pos+total]
            real = None; blocks = []; q = 0
            while q + 4 <= len(extra):
                bid, bsz = struct.unpack_from("<HH", extra, q)
                payload = extra[q+4:q+4+bsz]
                if bid == 1: real = payload
                else: blocks.append((bid, payload))
                q += 4 + bsz
            assert real is not None and len(real) == 8, "unexpected zip64 payload"
            real_off = struct.unpack("<Q", real)[0]
            assert real_off < 0xFFFFFFFF, "entry offset exceeds classic uint32"
            f.seek(real_off + 4); lver = struct.unpack("<H", f.read(2))[0]
            new_extra = b"".join(struct.pack("<HH", bid, len(pl)) + pl for bid, pl in blocks)
            rec = bytearray(cd[pos:pos+46])
            struct.pack_into("<H", rec, 6, lver)
            struct.pack_into("<H", rec, 30, len(new_extra))
            struct.pack_into("<I", rec, 42, real_off)
            out += rec + name + new_extra + comment
            fixed += 1
        elif vneed == 45:
            f.seek(hoff + 4); lver = struct.unpack("<H", f.read(2))[0]
            struct.pack_into("<H", rec, 6, lver)
            out += rec
            fixed += 1
        else:
            out += rec
        pos += total
    new_cdsize = len(out)
    f.seek(cdoff); f.write(out)
    f.seek(cdoff + new_cdsize)
    f.write(struct.pack("<IHHHHIIH", 0x06054b50, 0, 0, n2, n2, new_cdsize, cdoff, 0))
    f.truncate(); f.close()
    print(f"  normalized: zip64 extras stripped/vneed fixed={fixed}, classic EOCD ok")

normalize_zip(dst)

# 自检 1：官方受保护条目 sha256（外科铁律：dex / arsc / libil2cpp 字节不动）
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")

def stream_sha(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

protected = [f"classes{i}.dex" if i > 1 else "classes.dex" for i in range(1, 9)]
protected += ["resources.arsc", "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libxlua.so"]
for nm in protected:
    assert nm in orig_names, f"missing protected entry {nm}"
    h1, h2 = stream_sha(zin2, nm), stream_sha(zout2, nm)
    assert h1 == h2, f"PROTECTED MUTATED: {nm}"
print(f"  sha256 ok: {len(protected)} protected entries unchanged")

# 自检 2：全量条目 meta 对比 + 名字字节与 utf8 flag
meta_mismatch = raw_mismatch = flag_mismatch = 0
for item in zin2.infolist():
    name = item.orig_filename
    if name.startswith("META-INF/") or name == "AndroidManifest.xml":
        continue
    if name in extras:
        continue
    m = zout2.getinfo(name)
    if item.CRC != m.CRC or item.file_size != m.file_size or item.compress_type != m.compress_type:
        meta_mismatch += 1
    if item.orig_filename.encode("cp437", "replace") != m.orig_filename.encode("cp437", "replace"):
        raw_mismatch += 1
    # 0x08(data descriptor) 是本次有意归一化掉的位；除此之外 flag 必须逐位一致
    if (item.flag_bits & ~0x08) != (m.flag_bits & ~0x08) or (m.flag_bits & 0x08):
        flag_mismatch += 1
        if flag_mismatch <= 3:
            print(f"    flagmismatch {name!r}: orig={item.flag_bits:#06x} new={m.flag_bits:#06x}")
assert meta_mismatch == 0 and raw_mismatch == 0 and flag_mismatch == 0, \
    f"meta={meta_mismatch} rawname={raw_mismatch} flag={flag_mismatch}"
missing = [x for x in (new_dex, "lib/arm64-v8a/libstarry_magic.so") if x not in seen]
assert not missing, f"missing injectables: {missing}"
assert "AndroidManifest.xml" in zout2.namelist()
print(f"  merged ok; official entries={n}; {new_dex}={len(cnew)}B; +{len(extras)} extras")
print("  全量 CRC/size/ctype 一致; name-bytes & utf8-flag 全量一致（mismatch=0）")
zin2.close(); zout2.close()
PY

echo "[6/7] zipalign + sign → $SIGNED"
"$BT/zipalign" -f -p 4 "$TMP_UNSIGNED" "$TMP_ALIGNED"
"$BT/zipalign" -c 4 "$TMP_ALIGNED" || { echo "ERROR: aligned APK failed zipalign -c"; exit 1; }
rm -f "$TMP_UNSIGNED"   # 对齐完成即删，压低磁盘峰值
if [[ -x "$BT/apksigner" && -f "$KS" ]]; then
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --v4-signing-enabled false --out "$SIGNED" "$TMP_ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$TMP_ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND="$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | grep -v '\.tmp_' | head -1 || true)"
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
rm -f "$TMP_ALIGNED"
[[ -f "$SIGNED" ]] || { echo "ERROR: signing produced no $SIGNED"; exit 1; }

echo "[7/7] 产物自检"
python3.11 - "$SIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile
apk, new_dex = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
names = set(z.namelist())
assert new_dex in names, f"{new_dex} missing"
assert "lib/arm64-v8a/libstarry_magic.so" in names, "so missing"
assert "resources.arsc" in names and z.getinfo("resources.arsc").compress_type == zipfile.ZIP_STORED, "arsc not stored"
sig = sorted(n for n in names if n.startswith("META-INF/"))
assert not any(n.endswith(("X7GAME.SF", "X7GAME.RSA", "X7GAME.DSA", "X7GAME.EC")) for n in sig), \
    f"官方旧签名残留: {sig}"
assert any(n.endswith(".SF") for n in sig) and any(n.endswith(".RSA") for n in sig), \
    f"缺重签后的 v1 签名: {sig}"
dexes = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))
print(f"  dex: {dexes}")
print(f"  entries={len(names)} arsc=STORED v1sig={sig}")
PY
"$BT/zipalign" -c 4 "$SIGNED" >/dev/null && echo "  zipalign -c 4 ok"
echo "  size  : $(ls -lh "$SIGNED" | awk '{print $5}')"
echo "  sha256: $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "  md5   : $(md5 -q "$SIGNED")"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  # 覆盖安装（禁卸载：清数据 = Mod + 存档双杀）。装机后必须对设备包 md5 == 本地 APK
  REMOTE="/data/local/tmp/jjssjd1382_surgical.apk"
  "$ADB" -s "$SERIAL" push "$SIGNED" "$REMOTE"
  "$ADB" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  "$ADB" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
    || "$ADB" -s "$SERIAL" install -r "$SIGNED"
  echo "log: $ADB -s $SERIAL logcat -s Mod_Menu:V StarrySignSpoof:V MeatSdk:V"
fi

echo "DONE: $SIGNED"
