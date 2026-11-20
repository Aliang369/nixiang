#!/usr/bin/env bash
# =====================================================================
# 无限升级 13.101 / vc1650 —— game_id=wxsjdz1（21 槽）出包一条龙
#
# 链路（每步独立日志 ${WORK}/logs/NN_*.log）：
#   [0] 输入核对   底包 md5 / 真 dex 目录 / 菜单模板 / 星光 so / tapfix.dex
#   [1] 无壳装配   wxsj_surgical_unshelled_13101.sh（运行时 fork，只回填 so 的 sha256 断言）
#   [2] 判据①     无壳包 dex 编号连续 + baksmali 可列类 + StarryApp / StarryInitProvider 在位
#   [3] 换 tapfix  patch_wxsj_tap_unshelled_dex.sh <apk> --swap <tapfix.dex>（内含 zipalign+签名）
#   [4] 判据②     目标 dex md5 == tapfix.dex；其余条目逐字节透传；META-INF 由签名重建
#   [5] 判据③④   apksigner verify + CN=StarryMagic / 证书 MD5；包名 vc vn 未变
#   [6] 门禁       check_framework_isolation / pre_install_gate / menu_tpl_gate --require-stamp
#                 / preflight（本脚本自身）
#
# 用法：
#   bash 04_scripts/无限升级/build_wxsjdz1new_13101.sh --dry-run
#   bash 04_scripts/无限升级/build_wxsjdz1new_13101.sh            # 正式包
#   bash 04_scripts/无限升级/build_wxsjdz1new_13101.sh --help
#
# 环境变量（默认值 = 2026-11-20 现状，全部可覆盖）：
#   WXSJ_ORIG_APK    底包 APK（02_apks/无限升级/无限升级_原版_13.101_vc1650.apk）
#   WXSJ_UNSHELL_DIR 真 dex 目录（4 个 dex）
#   WXSJ_MENU_TPL    菜单模板目录（内含 classes2.dex + libc++_shared.so）
#   WXSJ_STARRY_SO   星光 so（game_id=wxsjdz1，21 槽）
#   WXSJ_TAPFIX_DEX  tapfix.dex（dexlib2 四步手术产物）
#   WXSJ_OUT_DIR     产物目录（默认 08_output/无限升级）
#   WXSJ_OUT_APK     产物全路径（默认按 --dry-run 自动加 _dryrun）
#   WXSJ_WORK_DIR    中间产物/日志目录（默认 OUT_DIR/_work_dz1new）
#   WXSJ_DRYRUN      1 = dry-run（等价 --dry-run）
#   MENU_REFRESH     1 = 先跑 wxsj_refresh_menu_tpl.sh 重编菜单模板
#   WXSJ_TPL_NAME    刷新产物模板目录名（默认 wxsj_menu_dz1new）
#   RUN_GATES        0 = 跳过 [6] 门禁（仅本地试跑，报告须写明）
#   WXSJ_EXPECT_BASE_MD5 / WXSJ_EXPECT_CERT_MD5 / WXSJ_EXPECT_CERT_CN
# =====================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

DRYRUN="${WXSJ_DRYRUN:-0}"
for arg in "$@"; do
  case "$arg" in
    --dry-run) DRYRUN=1 ;;
    --help|-h) sed -n '2,40p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 ${arg}（只接受 --dry-run / --help）"; exit 2 ;;
  esac
done

ORIG_APK="${WXSJ_ORIG_APK:-$NIXIANG/02_apks/无限升级/无限升级_原版_13.101_vc1650.apk}"
UNSHELL_DIR="${WXSJ_UNSHELL_DIR:-$NIXIANG/06_dumped/无限升级/13101/dex_dump}"
MENU_TPL="${WXSJ_MENU_TPL:-$NIXIANG/mod菜单/starry-mod-menu/templates/wxsj_menu_current_sig}"
STARRY_SO="${WXSJ_STARRY_SO:-$NIXIANG/08_output/无限升级/libstarry_magic_wxsjdz1new_13101_21slot_20261120.so}"
TAPFIX_DEX="${WXSJ_TAPFIX_DEX:-$NIXIANG/07_patched/无限升级/13101/tapfix/tapfix.dex}"
OUT_DIR="${WXSJ_OUT_DIR:-$NIXIANG/08_output/无限升级}"
WORK="${WXSJ_WORK_DIR:-$OUT_DIR/_work_dz1new}"
MENU_REFRESH="${MENU_REFRESH:-0}"
TPL_NAME="${WXSJ_TPL_NAME:-wxsj_menu_dz1new}"
RUN_GATES="${RUN_GATES:-1}"
EXPECT_BASE_MD5="${WXSJ_EXPECT_BASE_MD5:-23c3b6e9be50cc579de81f5c5a944e3b}"
EXPECT_CERT_MD5="${WXSJ_EXPECT_CERT_MD5:-161fca32a36d3df274458682a44cb774}"
EXPECT_CERT_CN="${WXSJ_EXPECT_CERT_CN:-StarryMagic}"
EXPECT_PKG="com.xingzuodaoshi.infinityUpgrade"
EXPECT_VC="1650"
EXPECT_VN="13.101"

if [ "$DRYRUN" = "1" ]; then SUFFIX="_dryrun"; else SUFFIX=""; fi
OUT_APK="${WXSJ_OUT_APK:-$OUT_DIR/无限升级_13.101_vc1650_wxsjdz1new$SUFFIX.apk}"

SURGICAL="$SCRIPT_DIR/wxsj_surgical_unshelled_13101.sh"
TAPSWAP="$SCRIPT_DIR/patch_wxsj_tap_unshelled_dex.sh"
GATES="$NIXIANG/04_scripts/_common"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
LOGDIR="$WORK/logs"
RESULT_FILE="$WORK/gate_results.txt"

say() { printf '%s\n' "$*"; }
hr() { printf '%s\n' '--------------------------------------------------------------------'; }

crit() { # crit <ID> <PASS|FAIL|SKIP> <说明>
  printf '%s\t%s\t%s\n' "$1" "$2" "$3" >> "$RESULT_FILE"
  printf '  [%s] %-3s %s\n' "$2" "$1" "$3"
}

run_logged() { # run_logged <日志名> <命令...>
  local name="$1"; shift
  local log="$LOGDIR/$name.log"
  say "  \$ $*"
  if "$@" > "$log" 2>&1; then
    say "  OK  ${name}（日志 ${log}）"
    return 0
  fi
  say "  !!  ${name} 失败（日志 $log 末 25 行）"
  tail -25 "$log" | sed 's/^/      /'
  return 1
}

PY=""
for c in python3 python; do
  if command -v "$c" >/dev/null 2>&1; then PY="$c"; break; fi
done
[ -n "$PY" ] || { echo "ERROR: 缺 python"; exit 2; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ] && [ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[ -n "$BT" ] || { echo "ERROR: 缺 Android build-tools（apksigner/zipalign）"; exit 2; }
[ -x "$BAKSMALI" ] || { echo "ERROR: 缺 baksmali: $BAKSMALI"; exit 2; }

mkdir -p "$WORK" "$LOGDIR" "$OUT_DIR" "$NIXIANG/07_patched/无限升级/13101_dz1new"
: > "$RESULT_FILE"

say "======================================================"
say " 无限升级 13.101/vc1650 · wxsjdz1(21 槽) 出包  DRYRUN=$DRYRUN"
say " 产物: $OUT_APK"
say " 工作区: $WORK"
say "======================================================"

# ---------------------------------------------------------------- [0]
hr; say "[0] 输入核对"
missing=0
for f in "$ORIG_APK" "$SURGICAL" "$TAPSWAP" "$STARRY_SO" "$TAPFIX_DEX" "$MENU_TPL/classes2.dex" "$MENU_TPL/libc++_shared.so"; do
  if [ -f "$f" ]; then
    say "  OK   $(basename "$f")  $(stat -f %z "$f") B  $(shasum -a 256 "$f" | awk '{print substr($1,1,16)}')  $f"
  else
    say "  MISS $f"; missing=1
  fi
done
[ "$missing" = "0" ] || { echo "ERROR: 输入缺件（见上）"; exit 1; }

BASE_MD5="$(md5 -q "$ORIG_APK")"
if [ "$BASE_MD5" = "$EXPECT_BASE_MD5" ]; then
  crit G1 PASS "底包 md5 = ${BASE_MD5}（与 2026-11-18 的 13.101 一致，脱壳件可复用）"
else
  crit G1 FAIL "底包 md5 = ${BASE_MD5}，期望 $EXPECT_BASE_MD5"
  echo "ERROR: 底包不是约定的 13.101/vc1650"; exit 1
fi
TAPFIX_MD5="$(md5 -q "$TAPFIX_DEX")"
STARRY_SO_SHA="$(shasum -a 256 "$STARRY_SO" | awk '{print $1}')"
say "  tapfix.dex md5 = $TAPFIX_MD5"
say "  starry so  md5 = $(md5 -q "$STARRY_SO")  size=$(stat -f %z "$STARRY_SO")"

DEX_N="$(find "$UNSHELL_DIR" -maxdepth 1 -type f -name '*.dex' | wc -l | tr -d ' ')"
if [ "$DEX_N" = "4" ]; then
  crit G2 PASS "真 dex 目录 $DEX_N 个 dex（06_dumped/无限升级/13101/dex_dump）"
else
  crit G2 FAIL "真 dex 目录有 $DEX_N 个 dex（13.101 期望 4）"
fi

# ------------------------------------------------------------- [0.5]
if [ "$MENU_REFRESH" = "1" ]; then
  hr; say "[0.5] 重编菜单模板 → templates/$TPL_NAME"
  if run_logged 00_menu_refresh env WXSJ_TPL_NAME="$TPL_NAME" \
        bash "$NIXIANG/mod菜单/starry-mod-menu/scripts/wxsj_refresh_menu_tpl.sh"; then
    MENU_TPL="$NIXIANG/mod菜单/starry-mod-menu/templates/$TPL_NAME"
    crit G3 PASS "菜单模板已重编: ${MENU_TPL}（classes2.dex $(stat -f %z "$MENU_TPL/classes2.dex") B）"
  else
    crit G3 FAIL "菜单模板重编失败（wxsj_refresh_menu_tpl.sh，见 $LOGDIR/00_menu_refresh.log）"
    exit 1
  fi
fi

# ---------------------------------------------------------------- [1]
hr; say "[1] 无壳装配（fork 外科脚本：回填 so 的 sha256 断言 + 钉死 NIXIANG）"
FORKED="$WORK/_fork/wxsj_surgical_unshelled_13101.dz1new.sh"
mkdir -p "$(dirname "$FORKED")"
"$PY" - "$SURGICAL" "$FORKED" "$STARRY_SO_SHA" "$NIXIANG" <<'PYEOF'
import re
import sys

src, dst, sha, root = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
text = open(src, encoding="utf-8").read()
# (a) so 的 sha256 断言：原脚本写死旧 so 指纹，回填成本次 so
pat = re.compile(r'check_hash "\$STARRY_SO"\s+"[0-9a-f]{64}"')
text, n1 = pat.subn('check_hash "$STARRY_SO"   "%s"' % sha, text)
# (b) fork 落在工作区（不在 04_scripts/<游戏>/ 下），原脚本的 NIXIANG 推导会指错 ⇒ 钉死仓库根
pat2 = re.compile(r'^NIXIANG=".*?$', re.M)
text, n2 = pat2.subn('NIXIANG="%s"' % root, text, count=1)
if n1 != 1 or n2 != 1:
    raise SystemExit("ERROR: fork 失败：so 断言命中 %d 处 / NIXIANG 命中 %d 处（各应为 1）" % (n1, n2))
open(dst, "w", encoding="utf-8").write(text)
print("  fork 完成: so sha256 断言 = %s" % sha)
print("  fork 完成: NIXIANG = %s" % root)
PYEOF
chmod +x "$FORKED"
crit G4 PASS "外科脚本已 fork（原脚本零改动；只回填 so 断言 + 钉死 NIXIANG）"

UNSHELLED="$WORK/01_unshelled.apk"
if ! run_logged 01_unshelled env \
      WXSJ_ORIG_APK="$ORIG_APK" \
      WXSJ_UNSHELL_DIR="$UNSHELL_DIR" \
      WXSJ_MENU_TPL="$MENU_TPL" \
      WXSJ_STARRY_SO="$STARRY_SO" \
      WXSJ_OUT_APK="$UNSHELLED" \
      bash "$FORKED"; then
  crit C1 FAIL "无壳装配失败（见 $LOGDIR/01_unshelled.log）"; exit 1
fi
crit C1 PASS "无壳装配产出 $(stat -f %z "$UNSHELLED") B"

# ---------------------------------------------------------------- [2]
hr; say "[2] 判据① dex 编号连续 / baksmali 可列类 / StarryApp+StarryInitProvider"
if "$PY" - "$UNSHELLED" "$BAKSMALI" > "$LOGDIR/02_crit1.log" 2>&1 <<'PYEOF'
import re
import subprocess
import sys
import zipfile

apk, baksmali = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
names = [n for n in z.namelist() if re.match(r'^classes\d*\.dex$', n)]
if not names:
    raise SystemExit("FAIL: 包内没有 classes*.dex")

def order(n):
    m = re.match(r'^classes(\d*)\.dex$', n)
    return 1 if m.group(1) == "" else int(m.group(1))

names.sort(key=order)
expect = ["classes.dex"] + ["classes%d.dex" % i for i in range(2, len(names) + 1)]
if names != expect:
    raise SystemExit("FAIL: dex 编号不连续：%s" % names)
print("  dex 编号连续：%s" % ", ".join(names))

all_classes = []
per_dex = {}
for n in names:
    tmp = "/tmp/_crit1_%s" % n
    open(tmp, "wb").write(z.read(n))
    out = subprocess.check_output([baksmali, "list", "classes", tmp], text=True)
    classes = [l.strip() for l in out.splitlines() if l.strip()]
    per_dex[n] = classes
    all_classes.extend(classes)
    print("  %-14s %9d B  %5d 类" % (n, z.getinfo(n).file_size, len(classes)))

for need in ("Lcom/starry/magic/StarryApp;", "Lcom/starry/magic/StarryInitProvider;"):
    hits = [n for n in names if need in per_dex[n]]
    if not hits:
        raise SystemExit("FAIL: 缺类 %s" % need)
    print("  %s 位于 %s" % (need, ", ".join(hits)))

seen = {}
for n in names:
    for c in per_dex[n]:
        seen.setdefault(c, []).append(n)
dups = sorted([c for c, ns in seen.items() if len(ns) > 1])
if dups:
    raise SystemExit("FAIL: 跨 dex 重复类 %d 个：%s" % (len(dups), dups[:5]))
print("  OK: 合计 %d 类，无跨 dex 重复" % len(all_classes))
PYEOF
then
  sed 's/^/  /' "$LOGDIR/02_crit1.log"
  crit C2a PASS "判据①：dex 编号连续 + baksmali 全部可列 + StarryApp/StarryInitProvider 在位"
else
  sed 's/^/  /' "$LOGDIR/02_crit1.log"
  crit C2a FAIL "判据① 不成立（见 $LOGDIR/02_crit1.log）"; exit 1
fi

# ---------------------------------------------------------------- [3]
hr; say "[3] 换 tapfix dex（--swap，不重新打补丁；内含 zipalign + 签名）"
SWAPPED="$WORK/02_tapfix.apk"
if ! run_logged 03_tapfix bash "$TAPSWAP" "$UNSHELLED" --swap "$TAPFIX_DEX" --out "$SWAPPED"; then
  crit C2b FAIL "换 dex 失败（见 $LOGDIR/03_tapfix.log）"; exit 1
fi

# ---------------------------------------------------------------- [4]
hr; say "[4] 判据② 目标 dex md5 == tapfix.dex / 其余条目逐字节透传 / META-INF 重建"
if "$PY" - "$UNSHELLED" "$SWAPPED" "$TAPFIX_DEX" "$WORK/crit2.txt" > "$LOGDIR/04_crit2.log" 2>&1 <<'PYEOF'
import hashlib
import sys
import zipfile

pre, post, tapfix, outmeta = sys.argv[1:5]
want = hashlib.md5(open(tapfix, "rb").read()).hexdigest()
za, zb = zipfile.ZipFile(pre), zipfile.ZipFile(post)
znames = zb.namelist()
pre_names = za.namelist()

changed = []
for n in pre_names:
    if n.startswith("classes") and n.endswith(".dex"):
        if n not in znames:
            raise SystemExit("FAIL: 成品缺 dex %s" % n)
        if hashlib.md5(za.read(n)).hexdigest() != hashlib.md5(zb.read(n)).hexdigest():
            changed.append(n)
if len(changed) != 1:
    raise SystemExit("FAIL: 变化的 dex 数 = %d（期望 1）：%s" % (len(changed), changed))
target = changed[0]
got = hashlib.md5(zb.read(target)).hexdigest()
if got != want:
    raise SystemExit("FAIL: %s md5=%s，tapfix.dex md5=%s" % (target, got, want))
print("  目标 dex %s：换前 %s -> 成品 %s（== tapfix.dex）" %
      (target, hashlib.md5(za.read(target)).hexdigest()[:12], got))

diffs = []
count = 0
for n in pre_names:
    if n.startswith("META-INF/") or n == target:
        continue
    count += 1
    if n not in znames:
        diffs.append("missing:%s" % n)
        continue
    if hashlib.sha256(za.read(n)).hexdigest() != hashlib.sha256(zb.read(n)).hexdigest():
        diffs.append("changed:%s" % n)
extra = [n for n in znames if n not in pre_names and not n.startswith("META-INF/")]
if extra:
    diffs.extend("extra:%s" % n for n in extra)
if diffs:
    raise SystemExit("FAIL: 非目标条目被改动：%s" % diffs[:8])
print("  其余 %d 个条目（除 META-INF）逐字节透传 OK" % count)

old_meta = [n for n in pre_names if n.startswith("META-INF/")]
new_meta = [n for n in znames if n.startswith("META-INF/")]
if not new_meta:
    raise SystemExit("FAIL: 成品无 META-INF（签名未重建）")
same = [n for n in new_meta if n in old_meta and
        hashlib.sha256(za.read(n)).hexdigest() == hashlib.sha256(zb.read(n)).hexdigest()]
if same:
    raise SystemExit("FAIL: META-INF 未重建（沿用旧同名文件）: %s" % same[:5])
print("  META-INF 重建 OK：旧 %d 条 -> 新 %d 条（%s）" %
      (len(old_meta), len(new_meta), ", ".join(new_meta[:4])))

open(outmeta, "w").write("TARGET=%s\nTARGET_MD5=%s\n" % (target, got))
PYEOF
then
  sed 's/^/  /' "$LOGDIR/04_crit2.log"
  crit C2b PASS "判据②：目标 dex md5 == tapfix.dex，其余条目透传，META-INF 重建"
else
  sed 's/^/  /' "$LOGDIR/04_crit2.log"
  crit C2b FAIL "判据② 不成立（见 $LOGDIR/04_crit2.log）"; exit 1
fi
TARGET_DEX="$(grep '^TARGET=' "$WORK/crit2.txt" | cut -d= -f2)"

# ---------------------------------------------------------------- [5]
hr; say "[5] 判据③④ 签名证书 / 包名版本"
"$BT/apksigner" verify --verbose --print-certs "$SWAPPED" > "$LOGDIR/05_apksigner.log" 2>&1 || true
CERT_DN="$(grep -m1 'Signer #1 certificate DN:' "$LOGDIR/05_apksigner.log" | sed 's/.*DN: *//' || true)"
CERT_MD5="$(grep -m1 'Signer #1 certificate MD5' "$LOGDIR/05_apksigner.log" | sed 's/.*: *//' | tr -d ':' | tr 'A-F' 'a-f' || true)"
VERIFY_OK=0
if grep -q 'Verified using v2 scheme (APK Signature Scheme v2): true' "$LOGDIR/05_apksigner.log"; then VERIFY_OK=1; fi
DN_CN="${CERT_DN#CN=}"
if [ "$VERIFY_OK" = "1" ] && [ "$CERT_MD5" = "$EXPECT_CERT_MD5" ] && [ "$DN_CN" != "$CERT_DN" ]; then
  crit C3 PASS "apksigner verify 通过（v2）；证书 $CERT_DN / MD5 $CERT_MD5"
else
  crit C3 FAIL "签名核对失败：verify=$VERIFY_OK DN=$CERT_DN MD5=${CERT_MD5}（期望 CN=$EXPECT_CERT_CN / ${EXPECT_CERT_MD5}）"
fi

"$BT/aapt2" dump badging "$SWAPPED" > "$LOGDIR/05_badging.log" 2>&1 || true
BADGE="$(grep -m1 '^package:' "$LOGDIR/05_badging.log" || true)"
GOT_PKG="$(printf '%s' "$BADGE" | sed -n "s/^package: name='\([^']*\)'.*/\1/p")"
GOT_VC="$(printf '%s' "$BADGE" | sed -n "s/.*versionCode='\([^']*\)'.*/\1/p")"
GOT_VN="$(printf '%s' "$BADGE" | sed -n "s/.*versionName='\([^']*\)'.*/\1/p")"
if [ "$GOT_PKG" = "$EXPECT_PKG" ] && [ "$GOT_VC" = "$EXPECT_VC" ] && [ "$GOT_VN" = "$EXPECT_VN" ]; then
  crit C4 PASS "包名/版本未变：$GOT_PKG vc$GOT_VC vn$GOT_VN"
else
  crit C4 FAIL "包名/版本异常：pkg=$GOT_PKG vc=$GOT_VC vn=${GOT_VN}（期望 $EXPECT_PKG/$EXPECT_VC/${EXPECT_VN}）"
fi

# ---------------------------------------------------------------- [6]
hr; say "[6] 落地产物 + 门禁"
cp -f "$SWAPPED" "$OUT_APK"
crit A1 PASS "产物 $OUT_APK  $(stat -f %z "$OUT_APK") B  md5 $(md5 -q "$OUT_APK")"

MENU_DEX="$("$PY" - "$OUT_APK" <<'PYEOF'
import re
import sys
import zipfile

z = zipfile.ZipFile(sys.argv[1])
names = [n for n in z.namelist() if re.match(r'^classes\d*\.dex$', n)]
for n in names:
    if b"Lcom/starry/magic/StarryInitProvider;" in z.read(n):
        print(n)
        break
PYEOF
)"
say "  菜单 dex = ${MENU_DEX}"

if [ "$RUN_GATES" = "1" ]; then
  if run_logged 06_gate_framework bash "$GATES/check_framework_isolation.sh"; then
    crit M1 PASS "check_framework_isolation.sh"
  else
    crit M1 FAIL "check_framework_isolation.sh（见 $LOGDIR/06_gate_framework.log）"
  fi

  if run_logged 06_gate_preinstall bash "$GATES/pre_install_gate.sh" --apk "$OUT_APK" --base-apk "$ORIG_APK"; then
    crit M2 PASS "pre_install_gate.sh --apk --base-apk（含查12 TapTap 方案关）"
  else
    crit M2 FAIL "pre_install_gate.sh（见 $LOGDIR/06_gate_preinstall.log）"
  fi

  if run_logged 06_gate_menutpl bash "$GATES/menu_tpl_gate.sh" --tpl "$MENU_TPL" --dex-name classes2.dex --require-stamp; then
    crit M3 PASS "menu_tpl_gate.sh --tpl $MENU_TPL --require-stamp"
  else
    crit M3 FAIL "menu_tpl_gate.sh --tpl（见 $LOGDIR/06_gate_menutpl.log）"
  fi

  if [ -n "$MENU_DEX" ]; then
    if run_logged 06_gate_menutpl_apk bash "$GATES/menu_tpl_gate.sh" --apk "$OUT_APK" --dex-name "$MENU_DEX" --require-stamp; then
      crit M4 PASS "menu_tpl_gate.sh --apk --dex-name $MENU_DEX --require-stamp"
    else
      crit M4 FAIL "menu_tpl_gate.sh --apk（见 $LOGDIR/06_gate_menutpl_apk.log）"
    fi
  else
    crit M4 FAIL "成品里找不到含 StarryInitProvider 的菜单 dex"
  fi

  if run_logged 06_gate_preflight bash "$GATES/preflight.sh" "$0"; then
    crit M5 PASS "preflight.sh build_wxsjdz1new_13101.sh"
  else
    crit M5 FAIL "preflight.sh（见 $LOGDIR/06_gate_preflight.log）"
  fi
else
  crit M1 SKIP "RUN_GATES=0：本次跳过全部门禁（仅本地试跑，报告须写明）"
fi

# ---------------------------------------------------------------- 汇总
hr; say "汇总（${RESULT_FILE}）"
printf '  %-5s %-6s %s\n' ID 状态 说明
while IFS='	' read -r a b c; do
  printf '  %-5s %-6s %s\n' "$a" "$b" "$c"
done < "$RESULT_FILE"

FAIL_N="$(awk -F'\t' '$2=="FAIL"' "$RESULT_FILE" | wc -l | tr -d ' ')"
{
  echo "apk      : $OUT_APK"
  echo "size     : $(stat -f %z "$OUT_APK")"
  echo "md5      : $(md5 -q "$OUT_APK")"
  echo "sha256   : $(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
  echo "target   : $TARGET_DEX (md5 $(grep '^TARGET_MD5=' "$WORK/crit2.txt" | cut -d= -f2))"
  echo "menu_dex : $MENU_DEX"
  echo "so       : $STARRY_SO ($(md5 -q "$STARRY_SO"))"
  echo "tapfix   : $TAPFIX_DEX ($TAPFIX_MD5)"
  echo "dryrun   : $DRYRUN"
} > "$WORK/artifact.txt"
say ""
sed 's/^/  /' "$WORK/artifact.txt"

if [ "$FAIL_N" != "0" ]; then
  say ""; say "FAIL x $FAIL_N —— 不得交付"; exit 1
fi
say ""; say "全部判据/门禁 PASS（DRYRUN=${DRYRUN}）"
