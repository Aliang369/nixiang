#!/usr/bin/env bash
# 冒险与征途 1.5.9 / vc159 — 星光外壳组装（provider-only 注入路线，分支 B）
#
# 底包：02_apks/冒险与征途/冒险与征途_1.5.9_official_base.apk（官方 com.mxyzt.gzmy 1.5.9）
# 注入（五件）：
#   1) classes4.dex                        → templates/mxyzt_menu_current/classes4.dex（javac+d8 产出）
#   2) lib/arm64-v8a/libstarry_magic.so     → libs_mxyzt/arm64-v8a/（mxyzt_build_so.sh）
#   3) lib/arm64-v8a/libc++_shared.so       → 同上（官方包无此 so，必须补）
#   4) assets/certs/starlight.crt           → templates/wxsj_menu_*/starlight.crt（AuthClient 证书钉扎）
#   5) AndroidManifest.xml                  → 07_patched/冒险与征途/surgical/AndroidManifest.xml（二进制补丁器产出）
#
# ★注入路线铁律（本作写死）：**不改 application@name、不改 appComponentFactory**
#   官方 <application name="com.hdwj.afk.MainApplication"
#        appComponentFactory="androidx.core.app.CoreComponentFactory">，只新增
#   <provider com.starry.magic.StarryInitProvider> + <service com.starry.magic.menu.MenuService>
#   + FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE 权限。
#   脚本对「清单里 app 名与组件工厂是否仍是原值」做断言，防止误用换 Application 的文本清单变体。
#
# 签名：01_tools/starry_release.jks（CN=StarryMagic，alias starry，pass starry2026）。
#   本机（win）当前**没有**该密钥库 ⇒ 未提供时脚本在 [4/7] 停下并把合并包留在
#     08_output/冒险与征途/冒险与征途_1.5.9_starry-merged-unsigned.apk（可复现、可后补签）。
#     绝不能用其它证书出正式包：菜单 v5 设备指纹只用 SSAID，SSAID 按签名证书隔离，
#     换证书 = 换设备 → 卡密 403「已绑定其他设备」。
#
# 用法:
#   bash scripts/surgical_mxyzt_1.5.9.sh
#   STARRY_INSTALL=1 SERIAL=127.0.0.1:16384 bash scripts/surgical_mxyzt_1.5.9.sh
#
# 产物：08_output/冒险与征途/冒险与征途_1.5.9_starry-final-releaseSigned.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
# shellcheck source=/dev/null
source "${STARRY_SRC}/../../04_scripts/_common/env.sh"

NIXIANG="${NIXIANG_ROOT}"
PKG="com.mxyzt.gzmy"
SERIAL="${SERIAL:-127.0.0.1:16384}"

ORIG_APK="${NIXIANG}/02_apks/冒险与征途/冒险与征途_1.5.9_official_base.apk"
ORIG_SIZE=883770021

# 菜单模板新鲜度门禁（2026-10-26 起）：模板必须与当前 Java 源码 HEAD 同步，
# 否则复制的就是旧菜单 dex（无限升级 vc1637 / 镇邪人 7f94233e 同一根因）。
bash "$NIXIANG/04_scripts/_common/menu_tpl_gate.sh" --tpl "${STARRY_SRC}/templates/mxyzt_menu_current" || {
  echo "ERROR: 菜单模板过期 ⇒ 先重跑: bash 04_scripts/冒险与征途/mxyzt_build_dex.sh" >&2
  exit 1
}

DEX_SRC="${STARRY_SRC}/templates/mxyzt_menu_current/classes4.dex"
# ★2026-10-27（加固轮）：dex 指纹不再手抄（同一坑：手抄值每轮都会过期 ⇒ 要么在校验处 FAIL，
#   要么被人绕过断言把旧 dex 拼进新包）。改为**现算 + 打印**；要锁死某版时设
#   `DEX_SHA256_EXPECT=<sha256>`。模板新鲜度另由 menu_tpl_gate.sh 对 Java 源码 HEAD 把关。
DEX_SHA256="$("${NIXIANG_PYTHON:-python}" -c 'import hashlib,sys;print(hashlib.sha256(open(sys.argv[1],"rb").read()).hexdigest())' "${DEX_SRC}" 2>/dev/null || true)"
DEX_SHA256_EXPECT="${DEX_SHA256_EXPECT:-}"
SO_SRC="${STARRY_SRC}/libs_mxyzt/arm64-v8a/libstarry_magic.so"
# ★FW-16（2026-10-16 优化会话）：so 的 size/sha256 **不再手抄** —— 手抄值每次「只换 so」交付后都会过期
#   （v46b 的 2223040/f08deb3b… 被 v52 的 2264760/560b562f… 取代后无人更新 ⇒ 要么在校验处 FAIL，
#    要么有人绕过断言把**旧 so** 拼进新包，跑起来是新包旧逻辑）。改为**从 SO_SRC 现算**。
#   需要锁定某一版 so 时，调用前设 `SO_SHA256_EXPECT=<sha256>`（不设＝只现算并打印供人工核对）。
#   注意：此处 `die()` 尚未定义（在第 74 行），故用内联错误处理。
if [[ ! -f "${SO_SRC}" ]]; then
  echo "ERROR: so 不存在：${SO_SRC}（先跑 04_scripts/冒险与征途/mxyzt_build_so.sh）" >&2
  exit 1
fi
SO_SIZE="$(wc -c <"${SO_SRC}" | tr -d ' ')"
SO_SHA256="$("${NIXIANG_PYTHON:-python}" -c 'import hashlib,sys;print(hashlib.sha256(open(sys.argv[1],"rb").read()).hexdigest())' "${SO_SRC}")"
SO_SHA256_EXPECT="${SO_SHA256_EXPECT:-}"
CXX_SRC="${STARRY_SRC}/libs_mxyzt/arm64-v8a/libc++_shared.so"
MF_SRC="${NIXIANG}/07_patched/冒险与征途/surgical/AndroidManifest.xml"
MF_SIZE=39148
OUT_DIR="${NIXIANG}/08_output/冒险与征途"
# 2026-10-27（加固轮）：允许另存，别覆盖上一版已验证的正式包（留可回滚）：
#   MXYZT_FINAL_APK="$OUT_DIR/冒险与征途_1.5.9_harden-releaseSigned.apk" bash ...
FINAL="${MXYZT_FINAL_APK:-${OUT_DIR}/冒险与征途_1.5.9_starry-final-releaseSigned.apk}"
MERGED="${OUT_DIR}/冒险与征途_1.5.9_starry-merged-unsigned.apk"
KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
CERT_SHA1="821c92e404ab5fdac8dbf157c2e467e71440917d"
CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

PY="${NIXIANG_PYTHON:-python}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-${NIXIANG_BUILD_TOOLS}/zipalign}"
APKSIGNER="${NIXIANG_APKSIGNER:-${NIXIANG_BUILD_TOOLS}/apksigner}"
if [[ -n "${NIXIANG_JAVA17:-}" ]]; then
  export JAVA_HOME="${NIXIANG_JAVA17}"
  export PATH="${NIXIANG_JAVA17}/bin:${PATH}"
fi

# starlight.crt（任一模板目录即可，内容同源）
CRT_SRC=""
for d in "${STARRY_SRC}"/templates/*/; do
  if [[ -f "${d}starlight.crt" ]]; then CRT_SRC="${d}starlight.crt"; break; fi
done

die() { echo "ERROR: $*" >&2; exit 1; }

[[ -f "${ORIG_APK}" ]] || die "missing ${ORIG_APK}"
[[ -f "${DEX_SRC}" ]]  || die "missing ${DEX_SRC}"
[[ -f "${SO_SRC}" ]]   || die "missing ${SO_SRC}"
[[ -f "${CXX_SRC}" ]]  || die "missing ${CXX_SRC} (rebuild via 04_scripts/冒险与征途/mxyzt_build_so.sh)"
[[ -f "${MF_SRC}" ]]   || die "missing ${MF_SRC} (patched binary manifest)"
[[ -n "${CRT_SRC}" ]]  || die "missing assets/certs/starlight.crt source under ${STARRY_SRC}/templates/*/"
[[ -x "${ZIPALIGN}" ]] || die "missing zipalign: ${ZIPALIGN}"
# Win 下 apksigner 是 apksigner.bat，MSYS 对 .bat 的 [ -x ] 恒为假 ⇒ 用 [ -f ] 判定
[[ -x "${APKSIGNER}" || -f "${APKSIGNER}" ]] || die "missing apksigner: ${APKSIGNER}"
[[ -n "${PY}" ]] || die "python not resolved (NIXIANG_PYTHON empty)"

LOCK_DIR="/tmp/mxyzt_surgical.lockdir"
if ! mkdir "${LOCK_DIR}" 2>/dev/null; then
  die "lock ${LOCK_DIR} held（另一个外科进程在跑；本阶段禁止并发，见 10_docs/_state/games/光阴之外.md 坑 5）"
fi
echo $$ >"${LOCK_DIR}/pid"
WORK=""
cleanup() {
  if [[ -n "${WORK}" && -d "${WORK}" ]]; then rm -rf "${WORK}"; fi
  rm -rf "${LOCK_DIR}"
}
trap cleanup EXIT
mkdir -p "${OUT_DIR}"
WORK="$(cd "${OUT_DIR}" && mktemp -d "$(pwd)/.mxyzt_surgical.XXXXXX")"
mkdir -p "${WORK}"

echo "[1/7] input checks"
osize="$(wc -c <"${ORIG_APK}" | tr -d ' ')"
[[ "${osize}" == "${ORIG_SIZE}" ]] || die "官方底包体积不符：期望 ${ORIG_SIZE} 实得 ${osize}（底包被换过？禁止继续）"
ssize="$(wc -c <"${SO_SRC}" | tr -d ' ')"
[[ "${ssize}" == "${SO_SIZE}" ]] || die "so 体积自检失败：现算 ${SO_SIZE} 实得 ${ssize}（同一次运行内不可能不符，说明文件被并发改写）"
(( ssize > 1000000 && ssize < 8000000 )) || die "so 体积异常：${ssize} B（预期 1–8 MB 量级的菜单 so；是不是拼到了别的 .so？）"
msize="$(wc -c <"${MF_SRC}" | tr -d ' ')"
[[ "${msize}" == "${MF_SIZE}" ]] || die "补丁清单体积不符：期望 ${MF_SIZE} 实得 ${msize}"
if [[ -n "${DEX_SHA256_EXPECT}" ]]; then
  "${PY}" -c "
import hashlib,sys
h=hashlib.sha256(open(sys.argv[1],'rb').read()).hexdigest()
if h!=sys.argv[2]:
    print('dex sha256 mismatch: %s'%h); sys.exit(1)
print('dex sha256 ok（强校验：== DEX_SHA256_EXPECT）')
" "${DEX_SRC}" "${DEX_SHA256_EXPECT}" || die "菜单 dex 指纹不符：期望 ${DEX_SHA256_EXPECT}（重跑 mxyzt_build_dex.sh）"
else
  echo "[check] 菜单 dex sha256（现算）= ${DEX_SHA256}  ← 需要锁定时设 DEX_SHA256_EXPECT=<sha256>"
fi
if [[ -n "${SO_SHA256_EXPECT}" ]]; then
  "${PY}" -c "
import hashlib,sys
h=hashlib.sha256(open(sys.argv[1],'rb').read()).hexdigest()
if h!=sys.argv[2]:
    print('so sha256 mismatch: %s'%h); sys.exit(1)
print('so sha256 ok（强校验：== SO_SHA256_EXPECT）')
" "${SO_SRC}" "${SO_SHA256_EXPECT}" || die "so 指纹不符：期望 ${SO_SHA256_EXPECT}（重跑 mxyzt_build_so.sh）"
else
  echo "[check] so sha256（现算）= ${SO_SHA256}  ← 需要锁定时设 SO_SHA256_EXPECT=<sha256>"
fi
# so 字节自检：四条菜单串 + 标签 + 目标库名
for s in 'Toggle_秒杀小怪' 'Toggle_无敌' 'Toggle_全模式技能伤害增加' 'Toggle_免广告'; do
  c="$(LC_ALL=C grep -ac "${s}" "${SO_SRC}" || true)"
  [[ "${c}" == "1" ]] || die "so 缺少菜单串 ${s}（grep -c=${c}）"
done
LC_ALL=C grep -aq '\[mxyzt\]' "${SO_SRC}" || die "so 缺少 [mxyzt] 标签"
LC_ALL=C grep -aq 'libil2cpp.so' "${SO_SRC}" || die "so 缺少 libil2cpp.so 等待库名"
echo "[check] 底包 ${osize} B / so ${ssize} B / 清单 ${msize} B / dex 指纹 ok / so 现算（见上） / 四条菜单串 ok"

echo "[2/7] merge (python zipfile, 官方非注入条目字节保真)"
"${PY}" - "${ORIG_APK}" "${WORK}/merged.apk" "${MF_SRC}" "${DEX_SRC}" "${SO_SRC}" "${CXX_SRC}" "${CRT_SRC}" <<'PYEOF'
import sys, zipfile, os

orig, out, mf, dex, so, cxx, crt = sys.argv[1:8]
REPL = {
    'AndroidManifest.xml': mf,
    'classes4.dex': dex,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
    'assets/certs/starlight.crt': crt,
}

def is_sig(name):
    u = name.upper()
    if u == 'META-INF/MANIFEST.MF':
        return True
    return name.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

# ---- 清单断言（UTF-16LE 字符串池）----
mfb = open(mf, 'rb').read()
def has(s):
    return s.encode('utf-16-le') in mfb
for s in ('com.hdwj.afk.MainApplication', 'androidx.core.app.CoreComponentFactory'):
    if not has(s):
        sys.exit('manifest assertion failed: missing %s' % s)
for s in ('com.starry.magic.StarryInitProvider', 'com.starry.magic.menu.MenuService'):
    if not has(s):
        sys.exit('manifest assertion failed: missing injected %s' % s)
print("[merge] manifest ok: app=%s factory=CoreComponentFactory + provider + service" % 'com.hdwj.afk.MainApplication')

zin = zipfile.ZipFile(orig, 'r')
names = zin.namelist()
for k in REPL:
    if k not in names and k.startswith('lib/') is False and k != 'classes4.dex' and k != 'assets/certs/starlight.crt':
        sys.exit('unexpected replace key %s' % k)
n_sig = 0
copied = []
replaced = []
added = []
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
        if n in REPL:
            zi.compress_type = item.compress_type  # 继承原条目的存储方式（arsc/manifest 必须 STORED）
            zout.writestr(zi, open(REPL[n], 'rb').read())
            replaced.append(n)
            continue
        zi.compress_type = item.compress_type
        zout.writestr(zi, zin.read(n))
        copied.append(n)
    # 新增条目（官方包不存在）
    for n, p in sorted(REPL.items()):
        if n in names or n in replaced:
            continue
        zi = zipfile.ZipInfo(n, date_time=(2026, 10, 2, 0, 0, 0))
        zi.external_attr = 0o100644 << 16
        zi.create_system = 3
        zi.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(zi, open(p, 'rb').read())
        added.append(n)

# ---- 保真复核：被透传的条目 CRC/体积必须与原包一致（只读中央目录，不解压）----
z2 = zipfile.ZipFile(out, 'r')
bad = []
for n in copied:
    a, b = zin.getinfo(n), z2.getinfo(n)
    if a.CRC != b.CRC or a.file_size != b.file_size:
        bad.append(n)
if bad:
    sys.exit('passthrough mismatch (%d entries), first=%s' % (len(bad), bad[0]))
print("[merge] entries: orig=%d copied=%d replaced=%d added=%d dropped_sig=%d"
      % (len(names), len(copied), len(replaced), len(added), n_sig))
print("[merge] passthrough CRC verified: %d entries" % len(copied))
print("[merge] replaced: %s" % ','.join(replaced))
print("[merge] added: %s" % ','.join(added))
print("[merge] out size = %d B" % os.path.getsize(out))
PYEOF

echo "[3/7] zipalign -f -p 4"
"${ZIPALIGN}" -f -p 4 "${WORK}/merged.apk" "${WORK}/aligned.apk"
[[ -f "${WORK}/aligned.apk" ]] || die "zipalign failed"

if [[ ! -f "${KS}" ]]; then
  echo ""
  echo "────────────────────────────────────────────────────────────"
  echo "⚠️  未找到签名密钥：${KS}"
  echo "    合并包已就绪（未签名）：${MERGED}"
  echo "    请把工作区 release 密钥库放到该路径后重跑本脚本即可（内容不变，只补签）："
  echo "        ${NIXIANG}/01_tools/starry_release.jks     （alias starry / pass starry2026）"
  echo "    ★不要用其它证书出正式包：SSAID 按签名证书隔离，换证书 = 换设备 → 卡密 403。"
  echo "────────────────────────────────────────────────────────────"
  cp -f "${WORK}/aligned.apk" "${MERGED}"
  ls -l "${MERGED}" | awk '{print "[4/7] unsigned merged -> "$NF" ("$5" B)"}'
  exit 3
fi

echo "[4/7] apksigner sign (CN=StarryMagic)"
rm -f "${FINAL}"
"${APKSIGNER}" sign \
  --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "${FINAL}" "${WORK}/aligned.apk"
[[ -f "${FINAL}" ]] || die "apksigner produced nothing"

echo "[5/7] verify"
rm -f "${MERGED}"
CERTINFO="$("${APKSIGNER}" verify --print-certs "${FINAL}" | head -6)"
echo "${CERTINFO}"
echo "${CERTINFO}" | LC_ALL=C grep -qi "${CERT_SHA256}" || die "签发证书 SHA-256 与工作区 release 证书不符"
echo "${CERTINFO}" | LC_ALL=C grep -qi "${CERT_SHA1}" || die "签发证书 SHA-1 不符"
fsize="$(wc -c <"${FINAL}" | tr -d ' ')"
echo "[done] ${FINAL} (${fsize} B)"
"${PY}" -c "
import hashlib,sys
h=hashlib.sha256(open(sys.argv[1],'rb').read())
print('sha256 %s' % h.hexdigest())
" "${FINAL}"

echo "[6/7] 加固门禁（所有游戏默认强制，见 AGENTS.md §6）"
bash "${NIXIANG}/04_scripts/_common/harden_gate.sh" "${FINAL}" --game 冒险与征途 || die "加固门禁未过 ⇒ 拒绝交付该包"
echo "      ↑ 凭证：09_test/冒险与征途/harden/<apk_md5>.stamp（装机闸门查11 认这个）"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[7/7] install -> ${SERIAL}（同签 -r 覆盖保数据；官方签必须先卸载并会清数据）"
  ADB="${ADB:-adb}"
  INSTALL_SRC="${FINAL}"
  if [[ "${NIXIANG_HOST}" == "win" ]] && command -v cygpath >/dev/null 2>&1; then
    ASC="$(cygpath -u "${LOCALAPPDATA}")/Temp/mxyzt_install.apk"
    cp -f "${FINAL}" "${ASC}" && INSTALL_SRC="${ASC}"
  fi
  "${ADB}" -s "${SERIAL}" install -r -d --user 0 "${INSTALL_SRC}"
  "${ADB}" -s "${SERIAL}" shell "dumpsys package ${PKG} | grep -E 'signatures|versionName|lastUpdateTime'"
  echo "[install] 签名哈希应为星光 [6f4dda0b]（若显示官方 [948ce3aa] 说明仍是官方包）"
else
  echo "[7/7] skip install (STARRY_INSTALL=1 才装)"
fi
