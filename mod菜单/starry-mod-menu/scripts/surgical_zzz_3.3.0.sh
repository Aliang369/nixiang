#!/usr/bin/env bash
# 绝区零 3.3.0 (com.miHoYo.Nap) — 星光外科组装【骨架探针版】（provider-only 注入路线）
#
# 底包：/tmp/nx_zzz_330/base_330.apk（真机 f9a116a2 只读拉取 3.3.0 vc438，md5 校验）
# 本包只做「装机链自证」：so 为 ZzzProbeMod（只读探针，不装 hook、不写代码段）
# 注入（三件，其余条目字节保真）：
#   1) classes6.dex                        → templates/zzz_menu_current/classes6.dex（javac+d8）
#   2) lib/arm64-v8a/libstarry_magic.so    → libs_zzz/arm64-v8a/（zzz_build_so.sh）
#   3) AndroidManifest.xml                 → 07_patched/绝区零/surgical/AndroidManifest.xml
#
# ★注入路线铁律（本作写死）：**不改 application@name、不改 appComponentFactory**
#   官方 <application name="com.combosdk.openapi.ComboApplication"
#        appComponentFactory="androidx.core.app.CoreComponentFactory">，只新增
#   <provider com.starry.magic.StarryInitProvider> + <service com.starry.magic.menu.MenuService>
#   + SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE_SPECIAL_USE（其余权限官包已有）。
#
# ★libc++_shared.so 官方包已自带（Unity IL2CPP）⇒ **绝不注入/覆盖**，
#   我们只加 libstarry_magic.so，让它解析到官包那一份。
#
# 签名：01_tools/starry_release.jks（CN=StarryMagic，alias starry，pass starry2026）
#   ★不要换证书：菜单 v5 设备指纹只用 SSAID，SSAID 按签名证书隔离，换证书 = 换设备 → 卡密 403。
#
# 用法:
#   bash mod菜单/starry-mod-menu/scripts/surgical_zzz_3.2.0.sh
#   ZZZ_BASE_APK=/path/to/official.apk bash ...（覆盖底包）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
# shellcheck source=/dev/null
source "${STARRY_SRC}/../../04_scripts/_common/env.sh"

NIXIANG="${NIXIANG_ROOT}"
GAME="绝区零"
VERSION="3.3.0"
PKG="com.miHoYo.Nap"
AUTHORITY="${PKG}.starryinit"

BASE_DIR_DEFAULT="/tmp/nx_zzz_330"
ORIG_APK="${ZZZ_BASE_APK:-${BASE_DIR_DEFAULT}/base_330.apk}"
# 3.3.0 (vc438) 真机只读拉取；见 10_docs/绝区零/版本重定位_3.3.0_20261022.md §1
ORIG_MD5="${ZZZ_BASE_MD5:-5cd97ff5fba40d9767f129eb8e887497}"

DEX_NAME="classes6.dex"

# 菜单模板新鲜度门禁（2026-10-26 起）：模板必须与当前 Java 源码 HEAD 同步，
# 否则复制的就是旧菜单 dex（无限升级 vc1637 / 镇邪人 7f94233e 同一根因）。
bash "$NIXIANG/04_scripts/_common/menu_tpl_gate.sh" --tpl "${STARRY_SRC}/templates/zzz_menu_current" || {
  echo "ERROR: 菜单模板过期 ⇒ 先重跑: bash 04_scripts/绝区零/zzz_build_dex.sh" >&2
  exit 1
}

DEX_SRC="${STARRY_SRC}/templates/zzz_menu_current/${DEX_NAME}"
SO_SRC="${ZZZ_SO_SRC:-${STARRY_SRC}/libs_zzz_probe/arm64-v8a/libstarry_magic.so}"
# 池鸳同款保护层手术：libtersafe2.so 置零 12B（file 0xa24bc），可选件
TERSAFE_SRC="${ZZZ_TERSAFE_SO:-}"   # 3.3.0 未核对该偏移，默认不替换
MF_CACHE="${NIXIANG}/07_patched/${GAME}/surgical/AndroidManifest_3.3.0.xml"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-probe-releaseSigned.apk"
MERGED="${OUT_DIR}/${GAME}_${VERSION}_starry-probe-merged-unsigned.apk"
# 构建暂存一律走 ASCII 目录：apktool/aapt2 在中文路径下会 GBK 乱码报 "failed to open directory"
ASCII_ROOT="${TEMP:-/tmp}"
if command -v cygpath >/dev/null 2>&1; then
  ASCII_ROOT="$(cygpath -u "${ASCII_ROOT}" 2>/dev/null || printf '%s' "${ASCII_ROOT}")"
fi
mkdir -p "${ASCII_ROOT}"

KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
CERT_SHA1="821c92e404ab5fdac8dbf157c2e467e71440917d"
CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

PY="${NIXIANG_PYTHON:-python}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-${NIXIANG_BUILD_TOOLS}/zipalign}"
APKSIGNER="${NIXIANG_APKSIGNER:-${NIXIANG_BUILD_TOOLS}/apksigner}"
APKTOOL="${APKTOOL:-${NIXIANG}/01_tools/apktool/apktool}"
AAPT2="${AAPT2:-${NIXIANG_BUILD_TOOLS}/aapt2.exe}"
[ -f "$AAPT2" ] || AAPT2="${NIXIANG_BUILD_TOOLS}/aapt2"
if [[ -n "${NIXIANG_JAVA17:-}" ]]; then
  export JAVA_HOME="${NIXIANG_JAVA17}"
  export PATH="${NIXIANG_JAVA17}/bin:${PATH}"
fi

die() { echo "ERROR: $*" >&2; exit 1; }

[[ -f "${ORIG_APK}" ]] || die "missing ${ORIG_APK}（用 ZZZ_BASE_APK=... 指定底包）"
[[ -f "${DEX_SRC}" ]]  || die "missing ${DEX_SRC}（先跑 04_scripts/绝区零/zzz_build_dex.sh）"
[[ -f "${SO_SRC}" ]]   || die "missing ${SO_SRC}（先跑 04_scripts/绝区零/zzz_build_so.sh）"
[[ -x "${ZIPALIGN}" ]] || die "missing zipalign: ${ZIPALIGN}"
[[ -x "${APKSIGNER}" || -f "${APKSIGNER}" ]] || die "missing apksigner: ${APKSIGNER}"
[[ -f "${APKTOOL}" || -x "${APKTOOL}" ]] || die "missing apktool: ${APKTOOL}"
[[ -n "${PY}" ]] || die "python not resolved (NIXIANG_PYTHON empty)"

LOCK_DIR="/tmp/zzz_surgical.lockdir"
if ! mkdir "${LOCK_DIR}" 2>/dev/null; then
  die "lock ${LOCK_DIR} held（另一个外科进程在跑；本阶段禁止并发）"
fi
echo $$ >"${LOCK_DIR}/pid"
WORK=""
cleanup() {
  if [[ -n "${WORK}" && -d "${WORK}" ]]; then rm -rf "${WORK}"; fi
  rm -rf "${LOCK_DIR}"
}
trap cleanup EXIT
mkdir -p "${OUT_DIR}"
WORK="$(mktemp -d "${ASCII_ROOT}/zzz_surgical.XXXXXX")"
MERGED="${WORK}/${GAME}_${VERSION}_starry-probe-merged-unsigned.apk"
FINAL_BUILD="${WORK}/${GAME}_${VERSION}_starry-probe-releaseSigned.apk"
mkdir -p "$(dirname "${MF_CACHE}")"

echo "[1/6] input checks"
osize="$(wc -c <"${ORIG_APK}" | tr -d ' ')"
omsg="$("${PY}" -c "
import hashlib,sys
print(hashlib.md5(open(sys.argv[1],'rb').read()).hexdigest())
" "${ORIG_APK}")"
[[ "${omsg}" == "${ORIG_MD5}" ]] || die "官方底包指纹不符 ${omsg}（期望 ${ORIG_MD5}）"
ssize="$(wc -c <"${SO_SRC}" | tr -d ' ')"
for s in 'Toggle_无敌' 'Toggle_无限技能' 'Slider_伤害倍率' 'Toggle_定怪'; do
  c="$(LC_ALL=C grep -ac "${s}" "${SO_SRC}" || true)"
  [[ "${c}" == "1" ]] || die "so 缺少菜单串 ${s}（grep -c=${c}）"
done
if LC_ALL=C grep -aq 'Slider_移动速度' "${SO_SRC}"; then
  die "so 仍含已删除的移动速度菜单串（2026-10-07 用户要求删除）"
fi
LC_ALL=C grep -aq '\[jql\]' "${SO_SRC}" || die "so 缺少 [jql] 标签"
echo "[check] 底包 ${osize} B (md5 ${omsg}) / so ${ssize} B / dex $(wc -c <"${DEX_SRC}" | tr -d ' ') B / 四条菜单串 ok（移速已删）"

echo "[2/6] binary AndroidManifest（保留官方 Application 与 ComponentFactory）"
if [[ -n "${ZZZ_BINARY_MANIFEST:-}" && -f "${ZZZ_BINARY_MANIFEST:-}" ]]; then
  cp "$ZZZ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $ZZZ_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once（apktool d -s --no-assets → 文本补丁 → apktool b）"
  "${APKTOOL}" d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  "${PY}" - "$WORK/orig_dec/AndroidManifest.xml" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
authority = sys.argv[2]
t = p.read_text(encoding="utf-8")

# 官方 Application / ComponentFactory 必须原样保留（断言而不是替换）
for must in ('android:name="com.combosdk.openapi.ComboApplication"',
             'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'):
    if must not in t:
        raise SystemExit("manifest assertion failed: missing %s" % must)

perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % authority
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
print("  text Manifest patched (Application/Factory preserved): +%d perms +provider +service" % len(perms))
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  "${APKTOOL}" b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
echo "  manifest $(wc -c <"$WORK/AndroidManifest.xml" | tr -d ' ') B"

echo "[3/6] binary Manifest 回读校验（aapt2 + 官方节点不丢失）"
"$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
"${PY}" - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
"$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
  || die "aapt2 无法解析新 manifest"
for need in \
  "com.starry.magic.StarryInitProvider" \
  "com.starry.magic.menu.MenuService" \
  "com.combosdk.openapi.ComboApplication" \
  "androidx.core.app.CoreComponentFactory" \
  "android.permission.SYSTEM_ALERT_WINDOW" \
  "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"; do
  grep -q "$need" "$WORK/tree_new.txt" || die "新 manifest 缺 $need"
done
sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
"${PY}" - "$WORK/o.norm" "$WORK/n.norm" <<'PY'
import sys
from collections import Counter
from pathlib import Path

def canon(path):
    stack, out = [], []
    for ln in Path(path).read_text(encoding='utf-8', errors='replace').splitlines():
        if not ln.strip():
            continue
        ind = len(ln) - len(ln.lstrip(' '))
        core = ln.strip()
        if core.startswith('N:'):
            continue
        if core.startswith('E:'):
            lv = (ind + 2) // 4
            stack = stack[:lv - 1]
            stack.append(core[2:].strip().split(' ')[0])
            out.append('/'.join(stack))
        elif core.startswith('A:') and stack:
            out.append('/'.join(stack[:ind // 4]) + '|' + core)
    return Counter(out)

o, n = canon(sys.argv[1]), canon(sys.argv[2])
dropped = o - n
added = sum((n - o).values())
if dropped:
    for k, v in list(dropped.items())[:10]:
        print('  LOST x%d: %s' % (v, k[:160]), file=sys.stderr)
    raise SystemExit('manifest 重建丢失官方节点（%d 项）' % sum(dropped.values()))
print('  manifest ok（新增 %d 项 / 官方节点零丢失）' % added)
PY

echo "[4/6] surgical zip merge（原 dex/arsc/assets/lib 字节保真）"
cp -f "${DEX_SRC}" "${WORK}/${DEX_NAME}"
"${PY}" - "$ORIG_APK" "$WORK" "$MERGED" "$SO_SRC" "$DEX_NAME" <<'PY'
import os, sys, zipfile

orig, work, out, so, dex_name = sys.argv[1:6]
REPL = {
    'AndroidManifest.xml': os.path.join(work, 'AndroidManifest.xml'),
    dex_name: os.path.join(work, dex_name),
    'lib/arm64-v8a/libstarry_magic.so': so,
}
_ts = os.environ.get('TERSAFE_SO') or ''
if _ts and os.path.exists(_ts):
    REPL['lib/arm64-v8a/libtersafe2.so'] = _ts
    print('  [tersafe] replacing libtersafe2.so from %s' % _ts)

def is_sig(name):
    u = name.upper()
    if u == 'META-INF/MANIFEST.MF':
        return True
    return name.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

zin = zipfile.ZipFile(orig, 'r')
names = set(zin.namelist())
for k in REPL:
    if k == dex_name or k.startswith('lib/'):
        continue
    if k not in names:
        sys.exit('unexpected replace key %s' % k)
# 严禁覆盖官方 libc++_shared.so
assert 'lib/arm64-v8a/libc++_shared.so' not in REPL

n_sig = copied = replaced = added = 0
added_names, replaced_names = [], []
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, allowZip64=True) as zout:
    for item in zin.infolist():
        n = item.filename
        if is_sig(n):
            n_sig += 1
            continue
        zi = zipfile.ZipInfo(n, date_time=item.date_time)
        zi.external_attr = item.external_attr
        zi.internal_attr = item.internal_attr
        zi.create_system = item.create_system
        zi.comment = item.comment
        zi.compress_type = item.compress_type
        if n in REPL:
            zout.writestr(zi, open(REPL[n], 'rb').read())
            replaced += 1
            replaced_names.append(n)
            continue
        zout.writestr(zi, zin.read(n))
        copied += 1
    for n, p in sorted(REPL.items()):
        if n in names:
            continue
        zi = zipfile.ZipInfo(n, date_time=(2026, 10, 5, 0, 0, 0))
        zi.external_attr = 0o100644 << 16
        zi.create_system = 3
        zi.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(zi, open(p, 'rb').read())
        added += 1
        added_names.append(n)

z2 = zipfile.ZipFile(out, 'r')
bad = []
for n in names:
    if n in REPL or is_sig(n):
        continue
    a, b = zin.getinfo(n), z2.getinfo(n)
    if a.CRC != b.CRC or a.file_size != b.file_size:
        bad.append(n)
if bad:
    sys.exit('passthrough mismatch (%d entries), first=%s' % (len(bad), bad[0]))
print('  entries: orig=%d copied=%d replaced=%d added=%d dropped_sig=%d'
      % (len(names), copied, replaced, added, n_sig))
print('  replaced: %s' % ','.join(replaced_names))
print('  added   : %s' % ','.join(added_names))
print('  passthrough CRC verified: %d entries' % copied)
print('  out size = %d B' % os.path.getsize(out))
PY

echo "[5/6] zipalign -f -p 4 + apksigner"
"${ZIPALIGN}" -f -p 4 "${MERGED}" "${WORK}/aligned.apk"
[[ -f "${WORK}/aligned.apk" ]] || die "zipalign failed"

if [[ ! -f "${KS}" ]]; then
  echo "WARN: 未找到签名密钥 ${KS} → 只留未签名合并包"
  exit 3
fi

rm -f "${FINAL}"
"${APKSIGNER}" sign \
  --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "${FINAL_BUILD}" "${WORK}/aligned.apk"
[[ -f "${FINAL_BUILD}" ]] || die "apksigner produced nothing"
cp -f "${FINAL_BUILD}" "${FINAL}"
[[ -f "${FINAL}" ]] || die "copy to ${FINAL} failed"

echo "[6/6] verify"
CERTINFO="$("${APKSIGNER}" verify --print-certs "${FINAL}" | head -6)"
echo "${CERTINFO}"
echo "${CERTINFO}" | LC_ALL=C grep -qi "${CERT_SHA256}" || die "签发证书 SHA-256 不符"
echo "${CERTINFO}" | LC_ALL=C grep -qi "${CERT_SHA1}" || die "签发证书 SHA-1 不符"
rm -f "${MERGED}"
fsize="$(wc -c <"${FINAL}" | tr -d ' ')"
echo "[done] ${FINAL} (${fsize} B)"
"${PY}" -c "
import hashlib,sys
print('sha256 %s' % hashlib.sha256(open(sys.argv[1],'rb').read()).hexdigest())
" "${FINAL}"
