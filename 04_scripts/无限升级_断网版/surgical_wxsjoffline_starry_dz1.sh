#!/usr/bin/env bash
# 无限升级 断网版 12.10701 (com.xingzuodaoshi.infinityOffline / vc1647) —— wxsjdz1 星空层外科组装
#
# 与 surgical_wxsjoffline_starry_probe.sh 的区别（探针包只取证，本脚本出正式包）：
#   ① so 换成 wxsjdz1 版（Android_wxsjdz1.mk 编出，内嵌 wxsjdz1 补丁 DLL 2,008,576 B）
#   ② 断言换成 wxsjdz1 特征（tag=wxsjdz1 + 内嵌载荷特征串 CodexOfflineBridge）
#   ③ 产物名带 _星空v1（探针包保留不动）
#
# 底包（只读）：07_patched/无限升级_断网版/无限升级_12.10701_断网版_去卡密.apk（md5 18de96…）
# 方案：外科 zip 追加 classes4.dex + libstarry_magic.so + libc++_shared.so + starlight.crt
#       + 二进制 AXML 补丁后的 AndroidManifest.xml；官方文件逐字节透传（脚本内有断言）
# 禁止：apktool b 整包重建（10_docs/无限升级/硬约束.md A#10）
#
# 用法：bash 04_scripts/无限升级_断网版/surgical_wxsjoffline_starry_dz1.sh [so路径]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "${SCRIPT_DIR}/../.." && pwd)"
BASE_APK="${NIXIANG}/07_patched/无限升级_断网版/无限升级_12.10701_断网版_去卡密.apk"
BASE_MD5="18de96004169843fa0774f446e314566"

WORK="${NIXIANG}/07_patched/无限升级_断网版/dz1"
SO="${1:-${WORK}/libstarry_magic.so}"
TPL_DIR="${NIXIANG}/mod菜单/starry-mod-menu/templates/wxsj_menu_current"
OUT_DIR="${NIXIANG}/07_patched/无限升级_断网版"
OUT_APK="${OUT_DIR}/无限升级_12.10701_断网版_星空v1.apk"
UNSIGNED="${WORK}/dz1_unsigned.apk"
ALIGNED="${WORK}/dz1_aligned.apk"
KS="${NIXIANG}/01_tools/starry_release.jks"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"

log(){ printf '%s\n' "$*"; }
die(){ printf 'ERROR: %s\n' "$*" >&2; exit 1; }

[[ -f "${BASE_APK}" ]] || die "missing base apk"
[[ "$(md5 -q "${BASE_APK}")" == "${BASE_MD5}" ]] || die "底包 md5 不符（禁止换包硬跑）"
[[ -f "${SO}" ]] || die "missing so: ${SO}"
[[ -f "${TPL_DIR}/classes2.dex" ]] || die "missing menu template dex"
[[ -f "${TPL_DIR}/libc++_shared.so" ]] || die "missing libc++_shared.so"
[[ -f "${TPL_DIR}/starlight.crt" ]] || die "missing starlight.crt"
[[ -f "${KS}" ]] || die "missing keystore"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "${BT}" ]] || die "Android build-tools not found"

mkdir -p "${WORK}/stage/lib/arm64-v8a" "${WORK}/stage/assets/certs"

log "[1/6] 二进制 AXML 补丁（provider 末位 + initOrder=MAX + MenuService + 3 权限）"
unzip -p "${BASE_APK}" AndroidManifest.xml > "${WORK}/AndroidManifest.base.xml"
python3 "${SCRIPT_DIR}/patch_wxsjoffline_starry_manifest.py" \
  "${WORK}/AndroidManifest.base.xml" "${WORK}/AndroidManifest.patched.xml"

log "[2/6] 暂存星空层四件套（dex 必须 classes4.dex：底包已占 classes/2/3）"
STARRY_DEX="classes4.dex"
unzip -Z1 "${BASE_APK}" > "${WORK}/base_list.txt"
for n in classes.dex classes2.dex classes3.dex; do
  grep -qx "${n}" "${WORK}/base_list.txt" || die "底包缺 ${n}（dex 布局变了，先复核编号）"
done
if grep -qx "${STARRY_DEX}" "${WORK}/base_list.txt"; then die "底包已存在 ${STARRY_DEX}，编号需重算"; fi
cp -f "${WORK}/AndroidManifest.patched.xml" "${WORK}/stage/AndroidManifest.xml"
cp -f "${SO}"                              "${WORK}/stage/lib/arm64-v8a/libstarry_magic.so"
cp -f "${TPL_DIR}/libc++_shared.so"         "${WORK}/stage/lib/arm64-v8a/libc++_shared.so"
cp -f "${TPL_DIR}/starlight.crt"           "${WORK}/stage/assets/certs/starlight.crt"
cp -f "${TPL_DIR}/classes2.dex"            "${WORK}/stage/${STARRY_DEX}"

# so 必须是 wxsjdz1 版（探针包 DIAG 版禁止出正式包）
LC_ALL=C grep -aq 'wxsjdz1' "${WORK}/stage/lib/arm64-v8a/libstarry_magic.so"      || die "so 缺 wxsjdz1 标识（拿错 so？）"
LC_ALL=C grep -aq 'patched dll injected' "${WORK}/stage/lib/arm64-v8a/libstarry_magic.so" || die "so 缺注入日志串"
LC_ALL=C grep -aq 'CodexOfflineBridge'   "${WORK}/stage/lib/arm64-v8a/libstarry_magic.so" || die "so 未内嵌本号补丁载荷"
if LC_ALL=C grep -aq 'DIAG dumped' "${WORK}/stage/lib/arm64-v8a/libstarry_magic.so"; then die "so 是 DIAG 探针版（禁出正式包）"; fi
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "${WORK}/stage/${STARRY_DEX}" || die "菜单 dex 缺 StarryInitProvider"

log "[3/6] 外科 zip 追加（底包为基底，只删 META-INF 重签）"
cp -f "${BASE_APK}" "${UNSIGNED}"
zip -d "${UNSIGNED}" 'META-INF/*' >/dev/null 2>&1 || true
( cd "${WORK}/stage" && zip -u "${UNSIGNED}" \
    AndroidManifest.xml \
    lib/arm64-v8a/libstarry_magic.so \
    lib/arm64-v8a/libc++_shared.so \
    assets/certs/starlight.crt \
    "${STARRY_DEX}" ) >/dev/null

log "[4/6] 断言（官方文件逐字节 + manifest 形态 + dex/so 落位）"
"${BT}/aapt2" dump xmltree --file AndroidManifest.xml "${UNSIGNED}" > "${WORK}/mf_tree.txt" 2>&1
grep -q 'com.starry.magic.StarryInitProvider' "${WORK}/mf_tree.txt" || die "StarryInitProvider missing"
grep -q 'android:initOrder(0x0101001a)=2147483647' "${WORK}/mf_tree.txt" || die "缺 initOrder=2147483647"
grep -q 'com.starry.magic.menu.MenuService' "${WORK}/mf_tree.txt" || die "MenuService missing"
grep -q 'com.netease.nis.wrapper.MyApplication' "${WORK}/mf_tree.txt" || die "易盾 Application 丢失（禁）"
grep -q 'infinityOffline.starryinit' "${WORK}/mf_tree.txt" || die "provider authorities 不符"

python3 - "${UNSIGNED}" "${BASE_APK}" "${STARRY_DEX}" <<'PY'
import hashlib, sys, zipfile
new, old = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
dex = sys.argv[3]
checked = [n for n in old.namelist() if n in ("lib/arm64-v8a/libil2cpp.so", "resources.arsc")
           or (n.startswith("classes") and n.endswith(".dex"))]
for name in sorted(checked):
    assert hashlib.sha256(new.read(name)).digest() == hashlib.sha256(old.read(name)).digest(), name
assert "lib/arm64-v8a/libil2cpp.so" in checked
so = new.read("lib/arm64-v8a/libstarry_magic.so")
assert b"wxsjdz1" in so and b"patched dll injected" in so and b"CodexOfflineBridge" in so
assert dex in new.namelist()
print("  official %d files byte-identical; %s + wxsjdz1 so staged" % (len(checked), dex))
PY

log "[5/6] zipalign + 签名（01_tools/starry_release.jks）"
"${BT}/zipalign" -f -p 4 "${UNSIGNED}" "${ALIGNED}"
mkdir -p "${OUT_DIR}"
"${BT}/apksigner" sign --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --out "${OUT_APK}" "${ALIGNED}"
"${BT}/apksigner" verify --print-certs "${OUT_APK}" | head -3

log "[6/6] done"
ls -l "${OUT_APK}"
shasum -a 256 "${OUT_APK}"
