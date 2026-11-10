#!/usr/bin/env bash
# 一念通天 pipeline 阶段 surgical —— 官方底包外科组装（版本无关）。
#
# 原包零改动：classes.dex / classes2.dex / resources.arsc / lib/**.so / global-metadata.dat 字节不变。
# 改动面：
#   * AndroidManifest.xml：保留官方 <application> 属性集，仅新增 StarryInitProvider / MenuService
#     与两个前台服务权限（官方 779 起不再声明自定义 Application 类，故断言改为属性集逐条相等）
#   * 新增 classes3.dex（星光 smali：com/starry/** + 本作 yntt_smali）
#   * 新增 lib/arm64-v8a/libstarry_magic.so（构建产物）+ libc++_shared.so
#   * 保留官方 META-INF 非签名条目，移除旧签名条目
#
# 产物：08_output/一念通天/一念通天_<版本>_surgical-unsigned.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/yntt_common.sh"

STARRY_SRC="${NIXIANG_ROOT}/mod菜单/starry-mod-menu"
UNSIGNED="${OUT_DIR}/${GAME}_${YNTT_VER}_surgical-unsigned.apk"
SMALI_BIN="${NIXIANG_ROOT}/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-${HOME}/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="${NDK}/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
NEW_DEX="classes3.dex"
AAPT2="${AAPT2:-${HOME}/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+${JAVA_HOME}/bin:}${PATH}"

yntt_need_file "${ORIG_APK}"
yntt_need_exec "${SMALI_BIN}"
SO_PREBUILT="${YNTT_SO:-${STARRY_SRC}/libs_yntt/arm64-v8a/libstarry_magic.so}"
yntt_need_file "${SO_PREBUILT}"
yntt_need_file "${AAPT2}"

ACTUAL_MD5="$(yntt_md5 "${ORIG_APK}")"
[ "${ACTUAL_MD5}" = "${ORIG_MD5}" ] || yntt_die "原包指纹不符：期望 ${ORIG_MD5}，实际 ${ACTUAL_MD5}"
yntt_log "底包=${ORIG_APK}（${YNTT_VER}，md5 ${ORIG_MD5}）"

LOCK_DIR="/tmp/yntt_surgical.lockdir"
if ! mkdir "${LOCK_DIR}" 2>/dev/null; then
  yntt_die "锁被占用：${LOCK_DIR}（另一条外科链在跑？确认无进程后 rmdir）"
fi
printf '%s' "$$" > "${LOCK_DIR}/pid"
WORK="$(mktemp -d /tmp/yntt_surgical.XXXXXX)"
trap 'rm -rf "${WORK}"; rm -f "${LOCK_DIR}/pid"; rmdir "${LOCK_DIR}" 2>/dev/null || true' EXIT
mkdir -p "${OUT_DIR}" "${WORK}"

yntt_log "[1/6] gradle assembleRelease（starry → 星光 dex 源）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "${STARRY_SRC}/" "${WORK}/starry/"
( cd "${WORK}/starry" && ./gradlew assembleRelease --console=plain )
STARRY_APK="${WORK}/starry/app/build/outputs/apk/release/app-release.apk"
yntt_need_file "${STARRY_APK}"

yntt_log "[2/6] apktool d → com/starry smali → ${NEW_DEX}"
apktool d -f -o "${WORK}/starry_dec" "${STARRY_APK}" >/dev/null
SMALI_ROOT="${WORK}/smali_in"
mkdir -p "${SMALI_ROOT}"
for d in "${WORK}/starry_dec"/smali "${WORK}/starry_dec"/smali_classes*; do
  [ -d "${d}/com/starry" ] || continue
  rsync -a "${d}/com/starry" "${SMALI_ROOT}/com/"
done
YNTT_SMALI="${STARRY_SRC}/yntt_smali"
[ -d "${YNTT_SMALI}" ] || yntt_die "缺少 ${YNTT_SMALI}"
rsync -a "${YNTT_SMALI}/" "${SMALI_ROOT}/"
for smali in \
  "com/starry/magic/sign/YnttLoginRequest.smali" \
  "com/starry/magic/sign/YnttLoginRequest\$1.smali" \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali"; do
  yntt_need_file "${SMALI_ROOT}/${smali}"
done
"${SMALI_BIN}" assemble -o "${WORK}/${NEW_DEX}" "${SMALI_ROOT}"
ls -la "${WORK}/${NEW_DEX}"

mkdir -p "${WORK}/lib/arm64-v8a"
cp "${SO_PREBUILT}" "${WORK}/lib/arm64-v8a/"
if [ -f "${NDK_CXX}" ]; then cp "${NDK_CXX}" "${WORK}/lib/arm64-v8a/libc++_shared.so"; fi
if [ -d "${WORK}/starry_dec/assets/certs" ]; then
  mkdir -p "${WORK}/assets/certs"
  cp -R "${WORK}/starry_dec/assets/certs/." "${WORK}/assets/certs/"
fi

yntt_log "[3/6] AndroidManifest（保留官方 application 属性集，只加 provider/service/权限）"
apktool d -f -s --no-assets -o "${WORK}/orig_dec" "${ORIG_APK}" >/dev/null
python3 - "${WORK}/orig_dec/AndroidManifest.xml" "${PKG}" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
if "FOREGROUND_SERVICE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("ERROR: manifest 里没有 <uses-permission> 锚点")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg
    t = t.replace("</application>", prov + "\n    </application>", 1)
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
p.write_text(t, encoding="utf-8")
print("  manifest patched: provider + service + foreground permissions")
PY
apktool b -f "${WORK}/orig_dec" -o "${WORK}/mf_only.apk" >/dev/null
unzip -p "${WORK}/mf_only.apk" AndroidManifest.xml > "${WORK}/AndroidManifest.xml"

"${AAPT2}" dump xmltree "${ORIG_APK}" --file AndroidManifest.xml > "${WORK}/tree_orig.txt"
python3 - "${WORK}/AndroidManifest.xml" "${WORK}/mf_check.apk" <<'PY'
import sys
import zipfile

z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
"${AAPT2}" dump xmltree "${WORK}/mf_check.apk" --file AndroidManifest.xml > "${WORK}/tree_new.txt" 2>/dev/null \
  || yntt_die "aapt2 无法解析新 manifest"
grep -q "com.starry.magic.StarryInitProvider" "${WORK}/tree_new.txt" || yntt_die "新 manifest 缺 StarryInitProvider"
grep -q "com.starry.magic.menu.MenuService" "${WORK}/tree_new.txt" || yntt_die "新 manifest 缺 MenuService"
grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "${WORK}/tree_new.txt" || yntt_die "新 manifest 缺 FOREGROUND_SERVICE_SPECIAL_USE"
python3 - "${WORK}/tree_orig.txt" "${WORK}/tree_new.txt" <<'PY'
import sys


def app_attrs(path):
    lines = open(path, encoding="utf-8", errors="replace").read().splitlines()
    for i, ln in enumerate(lines):
        if ln.strip().startswith("E: application"):
            indent = len(ln) - len(ln.lstrip())
            out = []
            for nxt in lines[i + 1:]:
                if not nxt.strip():
                    continue
                if len(nxt) - len(nxt.lstrip()) <= indent:
                    break
                t = nxt.strip()
                if t.startswith("E: "):
                    break
                out.append(t)
            return out
    return None


orig, new = app_attrs(sys.argv[1]), app_attrs(sys.argv[2])
if orig is None or new is None:
    raise SystemExit("ERROR: manifest 中找不到 <application> 节点")
if orig != new:
    raise SystemExit("ERROR: <application> 属性集被改动\n  orig=%r\n  new=%r" % (orig, new))
print("  application attrs identical (%d attrs)" % len(orig))
PY
sed -E 's/\(line=[0-9]+\)//' "${WORK}/tree_orig.txt" > "${WORK}/o.norm"
sed -E 's/\(line=[0-9]+\)//' "${WORK}/tree_new.txt" > "${WORK}/n.norm"
DIFF_D="$(diff "${WORK}/o.norm" "${WORK}/n.norm" | grep -c '^<' || true)"
[ "${DIFF_D}" = "0" ] || yntt_die "manifest 丢失官方节点 ${DIFF_D} 行"

yntt_log "[4/6] zip 外科合并 → ${UNSIGNED}"
python3 - "${ORIG_APK}" "${WORK}" "${UNSIGNED}" "${NEW_DEX}" <<'PY'
import hashlib
import sys
import zipfile
from pathlib import Path

src, work, dst, new_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
untouched = ["classes.dex", "classes2.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
             "lib/arm64-v8a/libmain.so", "lib/arm64-v8a/libtapsdkcore.so",
             "assets/bin/Data/Managed/Metadata/global-metadata.dat"]


def is_sig(name):
    if name == "META-INF/MANIFEST.MF":
        return True
    if not name.startswith("META-INF/"):
        return False
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))


zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names, "orig apk missing AndroidManifest.xml"
assert new_dex not in names, "orig apk already contains " + new_dex
for n in untouched:
    assert n in names, "orig apk missing " + n
kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
cnt = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    cnt += 1
info = zipfile.ZipInfo(new_dex)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, cnew)
cnt += 1
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    cnt += 1
zin.close()
zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, "changed: " + name
out_names = set(zout2.namelist())
for name in kept_meta:
    assert name in out_names, "lost " + name
assert new_dex in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert not any(x.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
print("  merged ok; entries~%d; %s=%d; untouched %d/%d sha256 verified"
      % (cnt, new_dex, len(cnew), len(untouched), len(untouched)))
zin2.close()
zout2.close()
PY

yntt_log "[5/6] 闭环校验（包内 so == 构建产物；so 标签存在）"
IN_SO_MD5="$(unzip -p "${UNSIGNED}" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(yntt_md5 "${SO_PREBUILT}")"
[ "${IN_SO_MD5}" = "${SRC_SO_MD5}" ] || yntt_die "包内 so md5 与构建产物不一致"
IN_SO_TAG="$(unzip -p "${UNSIGNED}" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -ac 'yntt' || true)"
[ "${IN_SO_TAG}" -ge 1 ] || yntt_die "包内 so 缺 yntt 标签"

yntt_log "[6/6] 完成（未签名）"
ls -lh "${UNSIGNED}"
yntt_log "unsigned=${UNSIGNED}"
yntt_log "so_md5=${IN_SO_MD5}"
yntt_log "由 04_scripts/一念通天/yntt_sign_release.sh 签名"
