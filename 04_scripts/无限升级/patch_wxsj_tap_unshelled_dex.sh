#!/usr/bin/env bash
# ============================================================
#  无限升级 13.101 / vc1650 —— TapTap「无壳 dex」静态改包法四步（2026-11-18）
#
#  前提：底包已脱壳（06_dumped/无限升级/13101/dex_dump/），真实 dex 可静态改。
#  本 dex 实测 TapSDK 路由＝3.x handler 数组（native 报 4.8.1-beta.7）：
#    com/taptap/sdk/login/internal/LoginActivityImpl.<init>
#      new-array[2] LoginHandler -> [0]=CloudLoginHandler、[1]=ClientLoginHandler
#    选择逻辑 instance-of Cloud/Client -> LoginEntrance.CLOUD/CLIENT，else -> WEB
#
#  四步（锚点按本 dex 现取）：
#    (1) GatekeeperRepository.isGatekeeperLoadedSuccessfully()Z -> const/4 v0,0x1; return v0
#    (2) GatekeeperRepository.showErrorToast(TapNetworkException)V -> 首行 return-void（保留 .registers）
#    (3) LoginActivityImpl.<init> 的 ClientLoginHandler 实例化 -> WebHandler（WebHandler.canHandle 恒 true）
#    (4) TapSignKit.getSign(Context)String -> const-string 官方证书 MD5（小写无冒号）
#
#  实现方式：dexlib2 级手术（04_scripts/无限升级/dexpatch/PatchWxsjTap.java），**不走 smali 往返**。
#    原因（实测）：smali 2.5.2 assemble 非字节确定 —— 同一 smali 树连两次产出 md5 不同
#    （/tmp/dt -> f200f5d7… / b3a8e90b…），无法满足「同输入两遍 md5 一致」；
#    dexlib2 DexFileFactory.writeDexFile 实测确定（同 DexFile 两次 d5508880…）。
#
#  用法：
#    bash 04_scripts/无限升级/patch_wxsj_tap_unshelled_dex.sh --dex <in.dex> [--out <out.dex>] [--twice]
#        默认 --out = 07_patched/无限升级/13101/tapfix/tapfix.dex
#    bash 04_scripts/无限升级/patch_wxsj_tap_unshelled_dex.sh <in.apk> [--out <out.apk>] [--twice]
#        加 --swap <已打补丁的 dex> = 不重新打补丁，直接把该 dex 换进包（保证与 dex 产物逐字节一致）
#        默认 --out = <同名>_tapfix-releaseSigned.apk
#
#  自验（硬性）：baksmali 回读，逐 .smali 文件 md5 比对 + 语义归一化：
#    允许 = ① 四步目标文件（3 个文件覆盖 4 步）② 语义等价噪声（.line/.local/.param、
#    静态字段默认值 = false/0/0x0/null、try 区间重编码）—— 噪声逐条列出，不计入目标。
#    出现其它差异或新增/删除文件 -> FAIL。
# ============================================================
set -o pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
SMALI_DIR="$NIXIANG/01_tools/smali"
BAKSMALI="$SMALI_DIR/baksmali"
PATCH_SRC="$NIXIANG/04_scripts/无限升级/dexpatch/PatchWxsjTap.java"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="starry"
KS_PASS="starry2026"
CERT_MD5="40fb0d3074290c0fd60a0dd5303b1757"

G_REPO="com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
G_LOGIN="com/taptap/sdk/login/internal/LoginActivityImpl.smali"
G_SIGN="com/taptap/sdk/kit/internal/utils/TapSignKit.smali"

IN=""; IN_DEX=""; OUT=""; DO_TWICE=0; SWAP=""
if [ -n "$STARRY_KS" ]; then KS="$STARRY_KS"; fi
if [ -n "$WXSJ_CERT_MD5" ]; then CERT_MD5="$WXSJ_CERT_MD5"; fi

while [ $# -gt 0 ]; do
  case "$1" in
    --dex) IN_DEX="$2"; shift 2 ;;
    --out) OUT="$2"; shift 2 ;;
    --twice) DO_TWICE=1; shift ;;
    --cert-md5) CERT_MD5="$2"; shift 2 ;;
    --swap) SWAP="$2"; shift 2 ;;
    -h|--help) sed -n '2,42p' "$0"; exit 0 ;;
    -*) echo "ERROR: 未知参数 $1"; exit 2 ;;
    *) if [ -z "$IN" ]; then IN="$1"; else echo "ERROR: 多余参数 $1"; exit 2; fi; shift ;;
  esac
done

if [ -z "$IN" ] && [ -z "$IN_DEX" ]; then echo "ERROR: 需要 APK 或 --dex"; exit 2; fi
[ -x "$BAKSMALI" ] || { echo "ERROR: 缺 baksmali"; exit 2; }
[ -f "$PATCH_SRC" ] || { echo "ERROR: 缺 $PATCH_SRC"; exit 2; }

WORK="$(mktemp -d /tmp/wxsj_usdex.XXXXXX)" || exit 1
trap 'rm -rf "$WORK"' EXIT

PY=""
for c in python3.11 python3 python; do
  if command -v "$c" >/dev/null 2>&1; then PY="$c"; break; fi
done
[ -n "$PY" ] || { echo "ERROR: 缺 python3"; exit 2; }

# JDK
if [ -n "$JAVA_HOME" ] && [ -x "$JAVA_HOME/bin/javac" ]; then
  JAVAC="$JAVA_HOME/bin/javac"; JAVA="$JAVA_HOME/bin/java"
elif [ -x /usr/libexec/java_home ]; then
  JH="$(/usr/libexec/java_home -v 17 2>/dev/null || /usr/libexec/java_home 2>/dev/null || true)"
  JAVAC="$JH/bin/javac"; JAVA="$JH/bin/java"
else
  JAVAC="$(command -v javac || true)"; JAVA="$(command -v java || true)"
fi
[ -x "$JAVAC" ] && [ -x "$JAVA" ] || { echo "ERROR: 缺 javac/java"; exit 2; }

DEXLIB2_CP="$SMALI_DIR/dexlib2-2.5.2.jar:$SMALI_DIR/guava-30.1.1-jre.jar:$SMALI_DIR/jcommander-1.64.jar:$SMALI_DIR/antlr-runtime-3.5.2.jar:$SMALI_DIR/jsr305-3.0.2.jar:$SMALI_DIR/failureaccess-1.0.1.jar:$SMALI_DIR/util-2.5.2.jar"
CLASSES="$WORK/classes"
mkdir -p "$CLASSES"
"$JAVAC" -nowarn -encoding UTF-8 -cp "$DEXLIB2_CP" -d "$CLASSES" "$PATCH_SRC" >/dev/null 2>&1   || { echo "ERROR: PatchWxsjTap.java 编译失败"; exit 1; }

patch_one_dex() {
  local src="$1" dst="$2"
  local w="$WORK/one"
  rm -rf "$w"; mkdir -p "$w"

  echo "   [dexlib2] 四步手术"
  "$JAVA" -cp "$DEXLIB2_CP:$CLASSES" PatchWxsjTap "$src" "$dst" "$CERT_MD5" || { echo "ERROR: 补丁执行失败"; return 1; }

  LC_ALL=C grep -aq -- "$CERT_MD5" "$dst" || { echo "ERROR: 产物缺官方 cert MD5"; return 1; }
  LC_ALL=C grep -aq 'Lcom/taptap/sdk/login/internal/handlers/web/WebHandler;' "$dst" || { echo "ERROR: 产物缺 WebHandler 引用"; return 1; }

  "$BAKSMALI" d -o "$w/before" "$src" >/dev/null 2>&1 || { echo "ERROR: baksmali(原) 失败"; return 1; }
  "$BAKSMALI" d -o "$w/after" "$dst" >/dev/null 2>&1 || { echo "ERROR: baksmali(产物) 失败"; return 1; }

  "$PY" - "$w/before" "$w/after" "$G_REPO" "$G_LOGIN" "$G_SIGN" <<'PYEOF'
import hashlib, os, sys, re
before, after, g_repo, g_login, g_sign = sys.argv[1:6]
expected = set([g_repo, g_login, g_sign])

NOISE = re.compile(r'^\s*\.(line|local|end local|param|prologue|restart local)\b')
DEFAULT_VAL = re.compile(r'^(\s*\.field\s[^=]+?)\s*=\s*(false|0|0x0|0x0L|0x0s|0x0f|0\.0|0\.0f|0L|0s|null)\s*$')
TRYNOISE = re.compile(r'^\s*(:(try_start|try_end)_[0-9]+|\.catch(\s|all\s))')

def snap(root, normalize):
    d = {}
    for dirpath, _, files in os.walk(root):
        for fn in files:
            if not fn.endswith('.smali'):
                continue
            p = os.path.join(dirpath, fn)
            data = open(p, 'rb').read()
            if normalize:
                txt = data.decode('utf-8', 'replace')
                kept = []
                for l in txt.split(chr(10)):
                    if NOISE.match(l) or TRYNOISE.match(l):
                        continue
                    l = l.rstrip()
                    m = DEFAULT_VAL.match(l)
                    if m:
                        l = m.group(1)
                    kept.append(l)
                data = chr(10).join(kept).encode('utf-8')
            d[os.path.relpath(p, root)] = hashlib.md5(data).hexdigest()
    return d

b, a = snap(before, False), snap(after, False)
added = sorted(set(a) - set(b)); removed = sorted(set(b) - set(a))
changed = sorted([k for k in set(a) & set(b) if a[k] != b[k]])
print('  回读：%d -> %d 文件；新增 %d / 删除 %d / 改动 %d' % (len(b), len(a), len(added), len(removed), len(changed)))
if added or removed:
    print('   ! 新增:', added[:5], '删除:', removed[:5])
    raise SystemExit('ERROR: 自验失败：出现新增/删除 smali 文件')
unexpected = [c for c in changed if c not in expected]
bn, an = snap(before, True), snap(after, True)
noise = [c for c in unexpected if bn.get(c) == an.get(c)]
real = [c for c in unexpected if bn.get(c) != an.get(c)]
if noise:
    print('   ~ 语义等价噪声 %d 个（.line/.local/静态字段默认值/try 区间重编码），前 8 条：' % len(noise))
    for c in noise[:8]:
        print('       - ' + c)
if real:
    print('   ! 非噪声改动:', real[:5])
    raise SystemExit('ERROR: 自验失败：改动范围超出四步目标')
targets = [c for c in changed if c in expected]
print('  自验 OK：目标改动 %d 个 = %s' % (len(targets), ', '.join(targets)))
print('  噪声 %d 个（语义等价，已单列）' % len(noise))
PYEOF
  [ $? -eq 0 ] || return 1

  echo "   --- 语义断言 ---"
  local f="$w/after/$G_LOGIN"
  grep -n 'new-array.*LoginHandler' "$f" | sed 's/^/       /'
  grep -n 'handlers/cloud/CloudLoginHandler;' "$f" | sed 's/^/       /'
  grep -n 'handlers/web/WebHandler;-><init>' "$f" | sed 's/^/       /'
  grep -n 'LoginEntrance;->WEB' "$f" | sed 's/^/       /'
  awk '/\.method public final isGatekeeperLoadedSuccessfully/,/\.end method/' "$w/after/$G_REPO" | sed 's/^/       /'
  awk '/\.method private final showErrorToast/,/^\.end method/' "$w/after/$G_REPO" | head -6 | sed 's/^/       /'
  return 0
}

echo "== 无限升级 13.101 · TapTap 无壳 dex 四步（dexlib2 手术） =="
echo "   官方 cert MD5: ${CERT_MD5}（小写无冒号；CommonUtils.getHashString 走 Integer.toHexString，dex 内无 toUpperCase）"

if [ -n "$IN_DEX" ]; then
  [ -f "$IN_DEX" ] || { echo "ERROR: $IN_DEX 不存在"; exit 2; }
  if [ -z "$OUT" ]; then
    OUTDIR="$NIXIANG/07_patched/无限升级/13101/tapfix"
    mkdir -p "$OUTDIR"
    OUT="$OUTDIR/tapfix.dex"
  fi
  echo "   输入 dex : $IN_DEX ($(stat -f %z "$IN_DEX") B / md5 $(md5 -q "$IN_DEX"))"
  echo "[1/2] 四步 + 回读自验"
  patch_one_dex "$IN_DEX" "$OUT" || exit 1
  echo "[2/2] 完成"
  echo "   输出 dex : $OUT ($(stat -f %z "$OUT") B / md5 $(md5 -q "$OUT") / sha256 $(shasum -a 256 "$OUT" | awk '{print $1}'))"
  if [ "$DO_TWICE" = "1" ]; then
    patch_one_dex "$IN_DEX" "$WORK/second.dex" >/dev/null 2>&1 || { echo "ERROR: 第二遍失败"; exit 1; }
    A=$(md5 -q "$OUT"); B=$(md5 -q "$WORK/second.dex")
    echo "   幂等: 第1遍 $A / 第2遍 $B"
    [ "$A" = "$B" ] || { echo "ERROR: 两遍 dex md5 不一致（不幂等）"; exit 1; }
    echo "   幂等 OK（同输入两遍字节一致）"
    patch_one_dex "$OUT" "$WORK/third.dex" >/dev/null 2>&1 || { echo "ERROR: 已打补丁的再补一遍失败"; exit 1; }
    C=$(md5 -q "$WORK/third.dex")
    echo "   重入幂等: 对产物再补一次 = $C"
    [ "$A" = "$C" ] || { echo "WARN: 对产物再补一次字节不同（语义相同，dexlib2 重写布局所致）"; }
  fi
  exit 0
fi

[ -f "$IN" ] || { echo "ERROR: $IN 不存在"; exit 2; }
BT_DIR=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]; then BT_DIR="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[ -n "$BT_DIR" ] || { echo "ERROR: 缺 build-tools"; exit 1; }

if [ -z "$OUT" ]; then
  d=$(dirname "$IN"); b=$(basename "$IN" .apk)
  OUT="$d/$b""_tapfix-releaseSigned.apk"
fi
echo "   输入 APK : $IN"
echo "   输出 APK : $OUT"

TARGET="$WORK/target.dex"
"$PY" - "$IN" "$TARGET" "$WORK/found.txt" <<'PYEOF'
import sys, zipfile
apk, out, found = sys.argv[1], sys.argv[2], sys.argv[3]
needles = [b'Lcom/taptap/sdk/kit/internal/utils/TapSignKit;', b'Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;']
z = zipfile.ZipFile(apk)
hits = []
for n in z.namelist():
    if n.startswith('classes') and n.endswith('.dex'):
        d = z.read(n)
        if all(x in d for x in needles):
            hits.append(n)
if not hits:
    raise SystemExit('ERROR: 包内没有 dex 同时含 TapSignKit + GatekeeperRepository（无壳没做到？）')
if len(hits) > 1:
    raise SystemExit('ERROR: 多个 dex 命中: ' + ','.join(hits))
data = z.read(hits[0])
open(out, 'wb').write(data)
open(found, 'w').write(hits[0])
print('   目标 dex : %s (%d B / md5 %s)' % (hits[0], len(data), __import__('hashlib').md5(data).hexdigest()))
PYEOF
[ $? -eq 0 ] || exit 1
NAME=$(cat "$WORK/found.txt")

echo "[1/4] 四步 + 回读自验"
PATCHED="$WORK/patched.dex"
if [ -n "$SWAP" ]; then
  [ -f "$SWAP" ] || { echo "ERROR: --swap 文件不存在: $SWAP"; exit 2; }
  cp -f "$SWAP" "$PATCHED"
  echo "   [swap] 直接用给定 dex（不重新打补丁）: $SWAP ($(stat -f %z "$SWAP") B / md5 $(md5 -q "$SWAP"))"
  LC_ALL=C grep -aq -- "$CERT_MD5" "$PATCHED" || { echo "ERROR: swap dex 缺官方 cert MD5"; exit 1; }
  LC_ALL=C grep -aq 'Lcom/taptap/sdk/login/internal/handlers/web/WebHandler;' "$PATCHED" || { echo "ERROR: swap dex 缺 WebHandler"; exit 1; }
else
  patch_one_dex "$TARGET" "$PATCHED" || exit 1
fi

echo "[2/4] 组包（替换 ${NAME}；其余条目逐字节透传；META-INF 剔除）"
STAGE="$WORK/stage.apk"
"$PY" - "$IN" "$STAGE" "$NAME" "$PATCHED" <<'PYEOF'
import sys, zipfile
src, dst, name, newdex = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
data = open(newdex, 'rb').read()
zin = zipfile.ZipFile(src)
n_out = 0
with zipfile.ZipFile(dst, 'w') as zout:
    for info in zin.infolist():
        n = info.filename
        if n.startswith('META-INF/'):
            continue
        payload = data if n == name else zin.read(n)
        zi = zipfile.ZipInfo(n, date_time=(1981, 1, 1, 1, 1, 0))
        zi.compress_type = info.compress_type
        zi.external_attr = info.external_attr
        zout.writestr(zi, payload)
        n_out += 1
print('   写出条目数:', n_out)
PYEOF
[ $? -eq 0 ] || exit 1

echo "[3/4] zipalign + sign"
"$BT_DIR/zipalign" -f -p 4 "$STAGE" "$WORK/aligned.apk" || { echo "ERROR: zipalign 失败"; exit 1; }
"$BT_DIR/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$OUT" "$WORK/aligned.apk" || { echo "ERROR: 签名失败"; exit 1; }

echo "[4/4] 校验"
"$BT_DIR/apksigner" verify --print-certs "$OUT" > "$WORK/verify.txt" 2>&1
grep -E 'Signer #1 certificate (DN|MD5)' "$WORK/verify.txt" | sed 's/^/   /'
"$PY" - "$IN" "$OUT" "$NAME" "$PATCHED" <<'PYEOF'
import sys, zipfile, hashlib
a, b, name, patched = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
za, zb = zipfile.ZipFile(a), zipfile.ZipFile(b)
want = hashlib.md5(open(patched, 'rb').read()).hexdigest()
got = hashlib.md5(zb.read(name)).hexdigest()
assert want == got, 'dex 段与补丁产物不一致'
print('   / 目标 dex md5: 底包 %s -> 成品 %s' % (hashlib.md5(za.read(name)).hexdigest(), got))
diff = 0
for n in za.namelist():
    if n.startswith('META-INF/') or n == name:
        continue
    if n not in zb.namelist():
        print('   ! 成品缺条目', n); diff += 1; continue
    if hashlib.sha256(za.read(n)).hexdigest() != hashlib.sha256(zb.read(n)).hexdigest():
        print('   ! 条目被改动', n); diff += 1
assert diff == 0, '存在非目标条目被改动'
print('   / 其余条目逐字节透传 OK（META-INF 已重建）')
PYEOF
[ $? -eq 0 ] || { echo "ERROR: 透传断言失败"; exit 1; }

echo "   产物: $OUT"
echo "   大小: $(stat -f %z "$OUT") B"
echo "   md5 : $(md5 -q "$OUT")"
echo "   sha256: $(shasum -a 256 "$OUT" | awk '{print $1}')"
