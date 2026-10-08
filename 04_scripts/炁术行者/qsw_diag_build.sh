#!/usr/bin/env bash
# 炁术行者 诊断包构建（只换 assets/qsw/main.js，不动唯一正式包）
#
# 用途：把主 JS 补丁里的「静默取证」打开 —— __QSW_LOG 从空实现改成转发到
#       window.StarryH5.log()（→ logcat tag StarryJs_qsxz），并在伤害钩子上加计数，
#       用来回答「两功能为何没效果」里唯一还没证伪的两个问题：
#         (a) 运行期战斗到底有没有走到被包住的 getBaseReduceHp / getRealReduceHp / getExtraRealReduceHp？
#         (b) 走到时 this.camp 是什么值、当时的 __QSWK / __QSW_DR 是多少？
#
# 用法:
#   bash 04_scripts/炁术行者/qsw_diag_build.sh            # 只出包 /tmp/qsw_diag/炁术行者_1.0_diag.apk
#   bash 04_scripts/炁术行者/qsw_diag_build.sh --install  # 出包并覆盖安装（同签，保留数据）
#   bash 04_scripts/炁术行者/qsw_diag_build.sh --restore  # 把唯一正式包装回设备（诊断结束收尾）
#
# ⚠️ 本脚本永不写 08_output/，唯一正式包不会被改动。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=../_common/env.sh
source "$SCRIPT_DIR/../_common/env.sh"

GAME="炁术行者"
PKG="com.h5wrapper.app"
NIXIANG="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
PY_BIN="${PY_BIN:-python3.11}"
SERIAL="${ANDROID_SERIAL:-f9a116a2}"

OFFICIAL="$NIXIANG/08_output/$GAME/${GAME}_1.0_星光版.apk"
SRC_JS="${QSW_WORK:-/tmp/qsw_build}/main_index.js"
DIAG_DIR="/tmp/qsw_diag"
DIAG_APK="$DIAG_DIR/${GAME}_1.0_diag.apk"

KS="$NIXIANG/01_tools/qsw_release.jks"
KS_ALIAS="qsw"
KS_PASS="${QSW_KS_PASS:-qsw123456}"
CERT_SHA256_WANT="42:6F:A3:FB:6B:14:09:E0:B2:E8:4E:9C:DB:EF:A8:63:A0:87:34:6D:87:88:46:E6:32:80:DB:3C:15:79:2B:88"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "找不到 build-tools/zipalign" >&2; exit 1; }

say() { printf '\n\033[1m==> %s\033[0m\n' "$*"; }

MODE="build"
for a in "$@"; do
  case "$a" in
    --install) MODE="install" ;;
    --restore) MODE="restore" ;;
    *) echo "未知参数: $a" >&2; exit 1 ;;
  esac
done

if [[ "$MODE" == "restore" ]]; then
  say "装回唯一正式包"
  [[ "$(md5 -q "$OFFICIAL")" == "ca2f3551420db63ac571936fadde6ba3" ]] || {
    echo "!! 正式包 md5 不是预期的 ca2f3551…（$(md5 -q "$OFFICIAL")）" >&2; exit 1; }
  adb -s "$SERIAL" install -r "$OFFICIAL"
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$OFFICIAL" --su
  exit 0
fi

[[ -f "$OFFICIAL" ]] || { echo "缺正式包 $OFFICIAL" >&2; exit 1; }
[[ -f "$SRC_JS" ]]   || { echo "缺服务端原始 JS ${SRC_JS}（先跑一次 qsw_build_star.sh）" >&2; exit 1; }
[[ -f "$KS" ]]       || { echo "缺签名密钥 $KS" >&2; exit 1; }
mkdir -p "$DIAG_DIR"

say "1. 生成补丁版 main.js（原样跑生产补丁链）"
"$PY_BIN" "$SCRIPT_DIR/qsw_patch_js_star.py" --src "$SRC_JS" --dst "$DIAG_DIR/main.prod.js"

say "2. 打开取证：__QSW_LOG 转发 + 伤害钩子计数"
"$PY_BIN" - "$DIAG_DIR/main.prod.js" "$DIAG_DIR/main.js" <<'PY'
import sys, hashlib, re
from pathlib import Path

src_p, dst_p = Path(sys.argv[1]), Path(sys.argv[2])
s = src_p.read_text(encoding="utf-8")


def sub_once(t, old, new, tag):
    n = t.count(old)
    assert n == 1, "%s 命中 %d 次（期望 1）" % (tag, n)
    print("   %-26s OK (1)" % tag)
    return t.replace(old, new)


# (1) 日志出口：空实现 → 转发到框架 JS 桥（→ logcat StarryJs_qsxz）
s = sub_once(s,
    "w.__QSW_LOG=function(){};",
    'w.__QSW_LOG=function(o){try{window.StarryH5&&window.StarryH5.log("QSWDIAG "+'
    'JSON.stringify(o).slice(0,320))}catch(e){}};',
    "log→StarryH5.log")

# (2) 装载确认：证明页面真的执行了我们这份 main.js
s = sub_once(s,
    'try{console.log("QSW_MOD_LOADED v10s")}catch(e){}',
    'try{console.log("QSW_MOD_LOADED v10s")}catch(e){}'
    'try{w.__QSW_LOG({ev:"loaded",tag:"v10s_diag",k:w.__QSWK,dr:w.__QSW_DR,'
    'bridge:!!window.StarryH5,ua:navigator.userAgent.slice(0,40)})}catch(e){}',
    "loaded 装载信标")

# (3) 投值确认：__QSW_APPLY 每次调用都留痕
s = sub_once(s,
    "w.__QSWK=k; w.__QSW_DR=drPct/100;",
    "w.__QSWK=k; w.__QSW_DR=drPct/100; w.__QSW_LOG({ev:\"apply\",k:k,dr:w.__QSW_DR});",
    "apply 投值信标")

# (4) 伤害钩子：计数 + 前 10 次 / 每 100 次打点（含 camp、k、dr、返回前后值）
i = s.index("w.__QSW_DMG=function(v,c){")
j = s.index("\n};", i) + 3
old_block = s[i:j]
assert "w.__QSW_DN++" in old_block and "1===c" in old_block, "钩子块形状异常:\n" + old_block
new_block = (
    "w.__QSW_DMG=function(v,c){\n"
    "  w.__QSW_DN++; var _b,_r;\n"
    "  if(1===c){ if(w.__QSW_DR>0){_b=\"def\";_r=Math.max(1,Math.round(v*(1-w.__QSW_DR)));}"
    "else{_b=\"def0\";_r=v;} }\n"
    "  else { _b=\"atk\"; _r=v*w.__QSWK; }\n"
    "  if(w.__QSW_DN<=10||0===w.__QSW_DN%100)w.__QSW_LOG({ev:\"dmg\",n:w.__QSW_DN,"
    "v:Math.round(v),c:c,k:w.__QSWK,dr:w.__QSW_DR,r:Math.round(_r),b:_b});\n"
    "  return _r;\n"
    "};\n"
)
s = s[:i] + new_block + s[j:]
print("   %-26s OK (1)" % "dmg 钩子插桩")

dst_p.write_text(s, encoding="utf-8")
print("   写出 %s  (%d B, md5 %s)" % (dst_p, len(s.encode("utf-8")),
                                    hashlib.md5(s.encode("utf-8")).hexdigest()))
PY

node --check "$DIAG_DIR/main.js" && echo "   node --check OK"
for needle in 'QSWDIAG' 'ev:"loaded"' 'ev:"apply"' 'ev:"dmg"' 'window.__QSW_DMG(Math.max(0,s)+Math.max(0,r),this.camp)' 'QSW_MOD_LOADED v10s'; do
  c=$(LC_ALL=C grep -acF -- "$needle" "$DIAG_DIR/main.js" || true)
  [[ "$c" -ge 1 ]] || { echo "!! 插桩缺件: $needle" >&2; exit 1; }
  printf '   %-56s x%s\n' "$needle" "$c"
done
# 注意：游戏 JS 压缩成极少几行，grep -c 数的是「行」不是「次数」⇒ 必须 grep -o 再数
DMG_CALLS=$(LC_ALL=C grep -oF -- 'window.__QSW_DMG(' "$DIAG_DIR/main.js" | wc -l | tr -d ' ')
[[ "$DMG_CALLS" == "3" ]] || { echo "!! __QSW_DMG 调用点 $DMG_CALLS 处（期望 3）" >&2; exit 1; }

say "3. zip 外科合并（只换 assets/qsw/main.js，剔除旧签名）"
"$PY_BIN" - "$OFFICIAL" "$DIAG_DIR/main.js" "$DIAG_DIR/unsigned.apk" <<'PY'
import sys, zipfile, shutil, hashlib
from pathlib import Path

src, new_js, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
data = new_js.read_bytes()
TARGET = "assets/qsw/main.js"

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
hit = False
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    info = zipfile.ZipInfo(name, item.date_time)
    info.external_attr = item.external_attr
    info.create_system = 3
    if name == "resources.arsc" or name.endswith(".so"):
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    if name == TARGET:
        info.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(info, data)
        hit = True
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    n += 1
zin.close(); zout.close()
assert hit, "APK 内没有 " + TARGET

a = zipfile.ZipFile(src); b = zipfile.ZipFile(dst)
na, nb = set(a.namelist()) - set(b.namelist()), set(b.namelist()) - set(a.namelist())
assert not (nb - {"assets/qsw/main.js"}), "多出条目: %s" % nb
print("   条目 %d；只换 %s（新 %d B, md5 %s）" % (n, TARGET, len(data),
                                              hashlib.md5(data).hexdigest()))
for k in a.namelist():
    if k.startswith("META-INF/") or k == TARGET:
        continue
    assert hashlib.sha256(a.read(k)).hexdigest() == hashlib.sha256(b.read(k)).hexdigest(), \
        "被误改: " + k
print("   其余条目逐字节一致 ✓")
a.close(); b.close()
PY

say "4. zipalign + sign"
"$BT/zipalign" -f -p 4 "$DIAG_DIR/unsigned.apk" "$DIAG_DIR/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$DIAG_APK" "$DIAG_DIR/aligned.apk"
"$BT/apksigner" verify --print-certs "$DIAG_APK" >/dev/null
GOT_SHA=$("$BT/apksigner" verify --print-certs "$DIAG_APK" | grep -i 'SHA-256' | head -1 | awk '{print $NF}' | tr -d ':' | tr 'A-Z' 'a-z')
WANT_SHA=$(echo "$CERT_SHA256_WANT" | tr -d ':' | tr 'A-Z' 'a-z')
echo "   signer SHA-256: $GOT_SHA"
[[ "$GOT_SHA" == "$WANT_SHA" ]] || { echo "!! 签名证书不符，无法覆盖安装" >&2; exit 1; }
echo "   md5  : $(md5 -q "$DIAG_APK")"
echo "   体积 : $(ls -lh "$DIAG_APK" | awk '{print $5}')"
echo "   正式包未改动: $OFFICIAL md5 $(md5 -q "$OFFICIAL")"

if [[ "$MODE" == "install" ]]; then
  say "5. 覆盖安装诊断包"
  adb -s "$SERIAL" install -r "$DIAG_APK"
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$DIAG_APK" --su
fi
