#!/usr/bin/env bash
# 炁术行者（com.h5wrapper.app）——「星光版」增量重打包（底包缺失 / 只改功能的降级路径）
#
# 与 qsw_build_star.sh 的关系：
#   · qsw_build_star.sh 是「全量重建」：从 1.4MB 原始壳出发，重编 classes.dex 注入 +
#     classes2.dex 框架 + Manifest 三声明 + so + JS，再重签。它硬校验底包 md5
#     （BASE_APK_MD5=f528339a…，即 02_apks/炁术行者9.15.apk）。
#   · 本脚本是「增量重打包」：当原始壳不在、只剩已装星光的正式包（如
#     02_apks/炁术行者_1.0.apk）时使用。正式包已经完成 dex 注入 / 框架 / Manifest /
#     签名，故本脚本只换三件——so、assets/starry/qsxz_patch.js、assets/qsw/main.js，
#     然后剔除旧签名重签。**不重编 dex、不重跑 gradle、不改 Manifest**。
#
# 适用：本次「三功能上限放宽（伤害倍率 1-100 / 减伤 1-100 / 战斗加速 1-10）」。
# 用法：
#   bash 04_scripts/炁术行者/qsw_repackage.sh            # 出包（不装机）
#   bash 04_scripts/炁术行者/qsw_repackage.sh --install  # 出包并覆盖安装
#   环境变量：QSW_BASE  已装星光的正式包（默认 02_apks/炁术行者_1.0.apk）
#             QSW_INDEX 已拉取的服务端 main/index.js（默认自动 curl；指定文件可确定性出包）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="炁术行者"
PKG="com.h5wrapper.app"
SERVER="http://43.248.140.201:8888"
BASE="${QSW_BASE:-$NIXIANG/02_apks/炁术行者_1.0.apk}"
SRC_DIR="$NIXIANG/04_scripts/$GAME"
SO_SRC="$NIXIANG/mod菜单/starry-mod-menu/libs_qsxz/arm64-v8a/libstarry_magic.so"
PATCH_JS="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/assets/starry/qsxz_patch.js"
WORK="${QSW_WORK:-/tmp/qsw_repackage}"
OUT_DIR="$NIXIANG/08_output/$GAME"
OUT="$OUT_DIR/${GAME}_1.0_星光版.apk"
KS="$NIXIANG/01_tools/qsw_release.jks"
KS_ALIAS="qsw"
KS_PASS="${QSW_KS_PASS:-qsw123456}"
# 设备上已装包的签名证书 SHA-256：换签名会导致覆盖安装失败
CERT_SHA256_WANT="42:6F:A3:FB:6B:14:09:E0:B2:E8:4E:9C:DB:EF:A8:63:A0:87:34:6D:87:88:46:E6:32:80:DB:3C:15:79:2B:88"

DO_INSTALL=0
[[ "${1:-}" == "--install" ]] && DO_INSTALL=1

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

say() { printf '\n== %s ==\n' "$1"; }
die() { echo "ERROR: $*" >&2; exit 1; }

PY_BIN="python3.11"; command -v "$PY_BIN" >/dev/null 2>&1 || PY_BIN="python3"
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]] && { BT="$HOME/Library/Android/sdk/build-tools/$v"; break; }
done
[[ -n "$BT" ]] || die "找不到 build-tools/zipalign"

say "0. 输入检查"
[[ -f "$BASE" ]] || die "缺正式包 ${BASE}（QSW_BASE 指定）"
[[ -f "$SO_SRC" ]] || die "缺 so ${SO_SRC}（先跑 qsxz_build_so.sh）"
[[ -f "$PATCH_JS" ]] || die "缺 ${PATCH_JS}"
[[ -f "$KS" ]] || die "缺签名密钥 ${KS}"
mkdir -p "$WORK/assets/qsw" "$WORK/assets/starry" "$OUT_DIR"

say "1. 取服务端 JS + 生成 main.js"
if [[ -n "${QSW_INDEX:-}" && -f "$QSW_INDEX" ]]; then
  cp "$QSW_INDEX" "$WORK/main_index.js"
  echo "  复用已拉取 index.js：$(md5 -q "$WORK/main_index.js")"
else
  curl -fsS "$SERVER/assets/main/index.js" -o "$WORK/main_index.js" || die "拉 main/index.js 失败"
fi
echo "  main_index.js $(wc -c <"$WORK/main_index.js" | tr -d ' ') B"
"$PY_BIN" "$SRC_DIR/qsw_patch_js_star.py" --src "$WORK/main_index.js" --dst "$WORK/assets/qsw/main.js"
cp "$SO_SRC" "$WORK/libstarry_magic.so"
cp "$PATCH_JS" "$WORK/assets/starry/qsxz_patch.js"

say "2. 静态自检"
"$PY_BIN" - "$WORK/assets/qsw/main.js" "$WORK/assets/starry/qsxz_patch.js" <<'PY'
import sys
mj = open(sys.argv[1], encoding='utf-8').read()
pj = open(sys.argv[2], encoding='utf-8').read()
for m in ('QSW-MOD v15s', '__QSW_APPLY', 'getBaseReduceHp=function',
          '__QSW_SPD', 'Object.defineProperty(Q,"speed1"', 'window.__QSW_SCHED=r.getScheduler()',
          '__QSW_ALERTSKIP', '__QSW_NETREQ', '__QSW_NETERR'):
    assert m in mj, 'main.js 缺 ' + m
assert mj.count('window.__QSW_DMG(') == 3, 'QSW_DMG 调用点不是 3 处'
# 减伤上限放宽后 dr 仍保留 0 作原版哨兵，且 100% 减伤折算除零已保护
assert 'if(drPct<0)drPct=0; else if(drPct>100)drPct=100' in mj, 'main.js dr 钳制口径不对'
assert '_d>0&&_d<1' in mj, 'main.js 减伤折算除零保护缺失'
for ch, nm in (('%', '百分号'), ('#', '井号')):
    assert ch not in pj, 'qsxz_patch.js 含' + nm
for m in ('getState', '__QSW_APPLY', 'state.live'):
    assert m in pj, 'qsxz_patch.js 缺 ' + m
print('  ok: main.js 标记齐全、dr 钳制/除零保护到位；qsxz_patch.js 无百分号/井号')
PY
if command -v node >/dev/null 2>&1; then
  node --check "$WORK/assets/qsw/main.js" && node --check "$WORK/assets/starry/qsxz_patch.js" \
    && echo "  ok: node --check 两份 JS 语法通过"
fi

say "3. zip 外科替换（只换 so + qsxz_patch.js + main.js，剔除旧签名）"
"$PY_BIN" - "$BASE" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, shutil
from pathlib import Path
src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
new_so = (work / "libstarry_magic.so").read_bytes()
new_mj = (work / "assets/qsw/main.js").read_bytes()
new_pj = (work / "assets/starry/qsxz_patch.js").read_bytes()
replace = {
    "lib/arm64-v8a/libstarry_magic.so": new_so,
    "assets/qsw/main.js": new_mj,
    "assets/starry/qsxz_patch.js": new_pj,
}
zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue                       # 旧签名必须剔除
    info = zipfile.ZipInfo(name, item.date_time)
    info.external_attr = item.external_attr
    info.create_system = 3
    # resources.arsc 与 .so 必须不压缩（targetSdk>=30 + extractNativeLibs 兼容）
    if name == "resources.arsc" or name.endswith(".so"):
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    if name in replace:
        zout.writestr(info, replace[name])
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    n += 1
zin.close(); zout.close()
print('  merged ok; 条目≈%d' % n)
PY

say "4. zipalign + sign + 闭环校验"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" >/dev/null
GOT_SHA=$("$BT/apksigner" verify --print-certs "$OUT" | grep -i 'SHA-256' | head -1 | awk '{print $NF}' | tr -d ':' | tr 'A-Z' 'a-z')
WANT_SHA=$(echo "$CERT_SHA256_WANT" | tr -d ':' | tr 'A-Z' 'a-z')
echo "  signer SHA-256: $GOT_SHA"
[[ "$GOT_SHA" == "$WANT_SHA" ]] || die "签名证书与设备已装包不一致（${GOT_SHA}）⇒ 无法覆盖安装"
unzip -p "$OUT" lib/arm64-v8a/libstarry_magic.so > "$WORK/pkg.so"
[[ "$(md5 -q "$SO_SRC")" == "$(md5 -q "$WORK/pkg.so")" ]] || die "包内 so != 构建产物（旧 so 顶包）"
echo "  包内 so md5          = $(md5 -q "$WORK/pkg.so")"
echo "  包内 main.js md5      = $(unzip -p "$OUT" assets/qsw/main.js | md5 -q)"
echo "  包内 qsxz_patch.js md5 = $(unzip -p "$OUT" assets/starry/qsxz_patch.js | md5 -q)"
LC_ALL=C grep -ac 'SeekBar_' "$WORK/pkg.so" >/dev/null
echo "  体积                 = $(ls -lh "$OUT" | awk '{print $5}')"
echo "  md5                  = $(md5 -q "$OUT")"

if [[ "$DO_INSTALL" == "1" ]]; then
  say "5. 装机（覆盖安装，保留数据）"
  adb install -r "$OUT"
fi

say "DONE"
echo "$OUT"
