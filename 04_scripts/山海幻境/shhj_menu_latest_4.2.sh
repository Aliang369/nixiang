#!/usr/bin/env bash
# 山海幻境 4.2 (vc68) — 菜单源码换新（只换 classes4.dex，底包 so 逐字节不动）
#
# 用途：以 `02_apks/山海幻境_4.2.apk`（用户声明的现装星光包）为底包，
#       用**工作区当前 HEAD 的菜单源码**重编 classes4.dex，只替换 classes4.dex
#       一个 zip 条目，实现「dex 菜单源码换新」；本轮目标是旧 dex 的
#       DeviceFingerprint v5-SSAID 单路径（菜单整块不可用）等已知缺陷。
#
# 与 so-only 脚本（shhj_so_only_*.sh）的区别：本脚本**不重编 native**、
# 不替换 lib/arm64-v8a/libstarry_magic.so，并断言 so 与其余条目逐字节不变。
#
# 战斗面专项断言（本游戏要求「确保战斗代码相关功能无误」）：
#   A. `com/starry/magic/cocos/ShhjHooks`（战斗 hook 的 Java 面）新旧 dex 归一化后
#      逐指令相同（寄存器重编号 + label 重命名不影响判定）。
#   B. so 内 `[Shhj]` splice 标签齐全、无其它游戏标签。
#   C. so 的 `RegisterNatives` 条目（Main.cpp regOne 20 条）与新 dex 的 native
#      声明名/签名逐一配对（装机前门禁 查2）。
#
# 用法：
#   bash 04_scripts/山海幻境/shhj_menu_latest_4.2.sh
#   SHHJ_BASE_APK=/path/to/base.apk bash 04_scripts/山海幻境/shhj_menu_latest_4.2.sh
#   STARRY_INSTALL=1 SERIAL=f9a116a2 bash 04_scripts/山海幻境/shhj_menu_latest_4.2.sh
#   STARRY_INSTALL_ONLY=1 bash 04_scripts/山海幻境/shhj_menu_latest_4.2.sh   # 已出包只补装机
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
OUT_DIR="$NIXIANG/08_output/山海幻境"
OUT_APK="${SHHJ_OUT_APK:-$OUT_DIR/山海幻境_4.2_menu-latest-releaseSigned.apk}"
PKG="com.chx.shhj"
SERIAL="${STARRY_SERIAL:-f9a116a2}"
BASE_APK="${SHHJ_BASE_APK:-$NIXIANG/02_apks/山海幻境_4.2.apk}"
DEX_ENTRY="classes4.dex"

KS="$NIXIANG/01_tools/starry_release.jks"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
PRE_GATE="$NIXIANG/04_scripts/_common/pre_install_gate.sh"
VERIFY="$NIXIANG/04_scripts/_common/verify_installed_apk.sh"

# 装机后必须匹配的签名证书指纹（starry_release.jks / 机身现装包一致）
WANT_CERT_SHA256="83:C5:CC:0B:E2:18:D0:46:B3:B7:AF:0D:2A:9F:82:36:29:7F:E2:28:17:1A:F2:97:4B:71:BA:C1:30:73:6F:1D"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done

die() { echo "ERROR: $*" >&2; exit 1; }

[[ -x "$SMALI_BIN" ]] || die "missing $SMALI_BIN"
[[ -x "$BAKSMALI_BIN" ]] || die "missing $BAKSMALI_BIN"
[[ -n "$BT" ]] || die "build-tools not found"
[[ -f "$PRE_GATE" ]] || die "missing $PRE_GATE"
[[ -f "$KS" ]] || die "missing $KS"
[[ -d "$STARRY_SRC" ]] || die "missing starry source $STARRY_SRC"

lock="/tmp/shhj_menu_latest_42.lockdir"
mkdir "$lock" 2>/dev/null || die "lock held: $lock"
work="$(mktemp -d /tmp/shhj_menu_latest.XXXXXX)"
remote="/data/local/tmp/shhj_menu_latest.apk"
cleanup() { rm -rf "$work" "$lock"; }
trap cleanup EXIT

# ---------------------------------------------------------------- 只补装机
if [[ "${STARRY_INSTALL_ONLY:-0}" == "1" ]]; then
  [[ -f "$OUT_APK" ]] || die "STARRY_INSTALL_ONLY=1 但产物不存在: $OUT_APK"
  echo "[install-only] $OUT_APK"
  echo "  md5=$(md5 -q "$OUT_APK")"
  adb -s "$SERIAL" push "$OUT_APK" "$remote"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$remote\"'"
  bash "$VERIFY" "$SERIAL" "$PKG" "$OUT_APK" --su
  echo "DONE(install-only): $OUT_APK"
  exit 0
fi

[[ ! -e "$OUT_APK" ]] || die "输出已存在，先移走或指定 SHHJ_OUT_APK: $OUT_APK"
command -v apktool >/dev/null || die "apktool not found"

echo "[0/7] 校验底包（本地 02_apks，只读）"
[[ -f "$BASE_APK" ]] || die "底包不存在: $BASE_APK"
echo "  base=$BASE_APK"
echo "  size=$(stat -f %z "$BASE_APK")  md5=$(md5 -q "$BASE_APK")"

python3.11 - "$BASE_APK" "$DEX_ENTRY" "$work/base_so.so" "$work/classes_old.dex" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
base, dex_entry, so_out, old_dex_out = sys.argv[1], sys.argv[2], Path(sys.argv[3]), Path(sys.argv[4])
with zipfile.ZipFile(base) as z:
    names = set(z.namelist())
    for need in ("classes.dex", dex_entry, "lib/arm64-v8a/libstarry_magic.so"):
        if need not in names:
            raise SystemExit(f"底包缺少条目: {need}")
    for n in sorted(x for x in names if x.startswith("classes") and x.endswith(".dex")):
        print(f"    {n}: {z.getinfo(n).file_size}B")
    c = z.read(dex_entry)
    so = z.read("lib/arm64-v8a/libstarry_magic.so")
if b"com/starry/magic/StarryApp" not in c:
    raise SystemExit(f"底包 {dex_entry} 不含星菜单 StarryApp，底包选错")
so_out.write_bytes(so)
old_dex_out.write_bytes(c)
print(f"  base {dex_entry}={len(c)}B md5={hashlib.md5(c).hexdigest()}")
print(f"  base so={len(so)}B md5={hashlib.md5(so).hexdigest()}")
# so 战斗面标签断言（确保底包 so 就是山海幻境那支）
needles = [b"[Shhj] spliced GXR/GXD/GX5 preamble", b"[Shhj] spliced curHp (compact)",
           b"[Shhj] spliced tHurt inv (spaced)", b"[Shhj] spliced getHurt dmg5 (spaced)"]
missing = [n.decode() for n in needles if n not in so]
if missing:
    raise SystemExit("底包 so 缺 Shhj splice 标签: " + "; ".join(missing))
for other in (b"[Gyws]", b"[Zmxy]", b"[Dwct]", b"[Dzqs]"):
    if other in so:
        raise SystemExit(f"底包 so 含其它游戏标签 {other!r}，底包选错")
print("  so 战斗面标签：4/4 齐全，无其它游戏标签")
PY

echo "[1/7] 从工作区 HEAD 构建菜单 release APK（只取 dex，不取 so）"
echo "  src=$STARRY_SRC"
echo "  git HEAD=$(git -C "$STARRY_SRC" rev-parse --short HEAD 2>/dev/null || echo unknown)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$work/starry/"
(cd "$work/starry" && ./gradlew assembleRelease --offline --console=plain >/dev/null)
starry_apk="$work/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$starry_apk" ]] || die "missing $starry_apk"

echo "[2/7] 反编译取 com/starry smali + StarryApp 宿主改写（→ Lcom/chx/shhj/GameApplication;）"
apktool d -f -o "$work/starry_dec" "$starry_apk" >/dev/null
smali_root="$work/smali_in"
mkdir -p "$smali_root"
shopt -s nullglob
for d in "$work/starry_dec"/smali "$work/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$smali_root/com/"
done
shopt -u nullglob
[[ -f "$smali_root/com/starry/magic/StarryApp.smali" ]] || die "StarryApp.smali missing"

python3.11 - "$smali_root/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
HOST = "Lcom/chx/shhj/GameApplication;"
repls = [
    (".super Landroid/app/Application;", f".super {HOST}"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        f"invoke-direct {{p0}}, {HOST}-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        f"invoke-super {{p0, p1}}, {HOST}->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        f"invoke-super {{p0}}, {HOST}->onCreate()V",
    ),
]
for a, b in repls:
    t = t.replace(a, b)
if HOST not in t:
    raise SystemExit("ERROR: StarryApp 缺宿主 GameApplication 改写")
bad = [x for x in (
    ".super Landroid/app/Application;",
    "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
    "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
    "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
) if x in t]
if bad:
    raise SystemExit("ERROR: Application super 分派未改净: " + "; ".join(bad))
print("  宿主改写 4/4 完成，旧 Application 分派残留 0")
p.write_text(t, encoding="utf-8")
PY

echo "[3/7] smali assemble → 新 $DEX_ENTRY"
new_dex="$work/$DEX_ENTRY"
"$SMALI_BIN" assemble -o "$new_dex" "$smali_root"
# 字节级断言（不依赖 locale 的 grep，中文串在 C locale 下 grep 会假失败）
python3.11 - "$new_dex" "$smali_root" <<'PY'
import sys
from pathlib import Path
dex = Path(sys.argv[1]).read_bytes()
src = Path(sys.argv[2])
need = {
    b"VERSION_LEGACY_V4": "v5->v4 兜底版本常量",
    b"getV4": "getV4 入口",
    b"writeSeedFile": "seed 落盘入口",
    b"featureListSignature": "功能列表指纹（偏好不再被无条件清空）",
    b"clearFeaturePrefsIfListChanged": "偏好按指纹清理",
    b"com/starry/magic/cocos/ShhjHooks": "战斗 hook Java 面",
    b"setRet": "ShhjHooks.setRet 入口",
    b"clearAll": "ShhjHooks.clearAll 入口",
    b"com/starry/magic/StarryApp": "星菜单 Application",
}
for k, label in need.items():
    if k not in dex:
        raise SystemExit(f"ERROR: 新 dex 缺 {label} ({k!r})")
# 旧 dex 专属提示必须消失，证明不是旧菜单源码
if "系统 ANDROID_ID 缺失或无效".encode() in dex:
    raise SystemExit("ERROR: 新 dex 仍含旧版纯 SSAID 报错串（菜单源码未换新）")
dp = (src / "com/starry/magic/auth/DeviceFingerprint.smali").read_text(encoding="utf-8")
if "VERSION_LEGACY_V4" not in dp or "getV4" not in dp:
    raise SystemExit("ERROR: DeviceFingerprint.smali 缺 v4 兜底")
sh = (src / "com/starry/magic/cocos/ShhjHooks.smali").read_text(encoding="utf-8")
for sig in ("setRet(IZ)V", "getRet(I)Z", "clearAll()V"):
    if sig not in sh:
        raise SystemExit(f"ERROR: ShhjHooks.smali 缺 {sig}")
print("  新 dex 断言通过：设备ID兜底 + 偏好指纹 + ShhjHooks 三方法齐全")
PY
echo "  new $DEX_ENTRY size=$(stat -f %z "$new_dex") md5=$(md5 -q "$new_dex")"

echo "[4/7] 战斗面语义比对：ShhjHooks 新旧 dex 归一化逐指令 diff"
"$BAKSMALI_BIN" d -o "$work/smali_old" "$work/classes_old.dex" >/dev/null
"$BAKSMALI_BIN" d -o "$work/smali_new" "$new_dex" >/dev/null
python3.11 - "$work/smali_old" "$work/smali_new" <<'PY'
import re, sys
from pathlib import Path

LBL = re.compile(r':[A-Za-z_][A-Za-z0-9_]*')
REG = re.compile(r'\b[vp]\d+\b')

def norm(path):
    out = []
    lbl_map, lbl_n = {}, 0
    reg_map, reg_n = {}, 0
    for line in Path(path).read_text(encoding="utf-8", errors="replace").splitlines():
        s = line.strip()
        if s.startswith((".registers", ".locals", ".line", "#")):
            continue
        def sub_lbl(m):
            nonlocal lbl_n
            k = m.group(0)
            if k not in lbl_map:
                lbl_map[k] = f":L{lbl_n}"; lbl_n += 1
            return lbl_map[k]
        def sub_reg(m):
            nonlocal reg_n
            k = m.group(0)
            if k not in reg_map:
                reg_map[k] = f"r{reg_n}"; reg_n += 1
            return reg_map[k]
        s = LBL.sub(sub_lbl, s)
        s = REG.sub(sub_reg, s)
        out.append(s)
    return out

old = norm(sys.argv[1] + "/com/starry/magic/cocos/ShhjHooks.smali")
new = norm(sys.argv[2] + "/com/starry/magic/cocos/ShhjHooks.smali")
if old == new:
    print(f"  ShhjHooks 归一化逐指令相同（{len(new)} 行）")
else:
    import difflib
    d = list(difflib.unified_diff(old, new, "old", "new", lineterm="", n=1))
    print("\n".join(d[:60]))
    raise SystemExit(f"ERROR: ShhjHooks 归一化后仍有 {sum(1 for x in d if x[:1] in '+-' and x[:3] not in ('+++','---'))} 行差异（战斗 hook Java 面变了）")

# 其余 native 面 / 登录体类：只做方法·字段·const-string 集合比对，允许新增
def sets(path):
    t = Path(path).read_text(encoding="utf-8", errors="replace")
    m = set(re.findall(r'^\.method[^\n]*?([A-Za-z0-9_$<>]+\([^\n]*?\)[^\s]+)', t, re.M))
    f = set(re.findall(r'^\.field[^\n]*?([A-Za-z0-9_$]+):', t, re.M))
    s = set(re.findall(r'const-string [vp]\d+, "((?:[^"\\]|\\.)*)"', t))
    return m, f, s

for cls in ("bridge/ModBridge", "menu/NativeMenuController", "auth/DeviceFingerprint",
            "menu/MenuService", "ota/StarryNativeLoad"):
    o = Path(sys.argv[1], "com/starry/magic", cls + ".smali")
    n = Path(sys.argv[2], "com/starry/magic", cls + ".smali")
    if not o.exists() or not n.exists():
        print(f"  {cls}: 跳过（一侧缺失 old={o.exists()} new={n.exists()}）")
        continue
    mo, fo, so = sets(o); mn, fn, sn = sets(n)
    print(f"  {cls}: 方法 {len(mo)}→{len(mn)} 字段 {len(fo)}→{len(fn)} 串 {len(so)}→{len(sn)}"
          f" | 仅旧有 方法{sorted(mo-mn)} 串{sorted(so-sn)[:3]}")
PY

echo "[5/7] 外科置换：只换 ${DEX_ENTRY}，so 逐字节不动"
python3.11 - "$BASE_APK" "$work/unsigned.apk" "$new_dex" "$DEX_ENTRY" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

base, dst, new_dex, dex_entry = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
with zipfile.ZipFile(base) as z:
    base_so = z.read("lib/arm64-v8a/libstarry_magic.so")
    names = set(z.namelist())
dropped = sorted(n for n in names if n.startswith("META-INF/"))

with zipfile.ZipFile(base, "r") as zin, zipfile.ZipFile(dst, "w", allowZip64=True) as zout:
    for item in zin.infolist():
        if item.filename.startswith("META-INF/"):
            continue
        if item.filename == dex_entry:
            data = new_dex.read_bytes()
        else:
            data = zin.read(item)
        info = zipfile.ZipInfo(item.filename, item.date_time)
        info.compress_type = item.compress_type
        info.comment = item.comment
        info.extra = item.extra
        info.internal_attr = item.internal_attr
        info.external_attr = item.external_attr
        info.create_system = item.create_system
        zout.writestr(info, data)

def manifest(p):
    out = {}
    with zipfile.ZipFile(p) as z:
        for i in z.infolist():
            if i.filename.startswith("META-INF/"):
                continue
            out[i.filename] = hashlib.sha256(z.read(i)).hexdigest()
    return out

ma, mb = manifest(base), manifest(dst)
if set(ma) != set(mb):
    only_a = sorted(set(ma) - set(mb))[:10]
    only_b = sorted(set(mb) - set(ma))[:10]
    raise SystemExit(f"条目集合变化 unexpected: -{only_a} +{only_b}")
changed = [n for n in ma if ma[n] != mb[n]]
if changed != [dex_entry]:
    raise SystemExit(f"非预期改动条目: {changed}")
with zipfile.ZipFile(dst) as z:
    if z.read(dex_entry) != new_dex.read_bytes():
        raise SystemExit(f"{dex_entry} copy mismatch")
    if z.read("lib/arm64-v8a/libstarry_magic.so") != base_so:
        raise SystemExit("starry so 被改动（本轮要求逐字节不动）")
print(f"  zip audit ok: 仅 {dex_entry} 变更，so 与其余 {len(ma)-1} 个条目逐字节一致")
print(f"  丢弃旧签名条目 {len(dropped)} 个（重签补回）")
print("  base so md5 =", hashlib.md5(base_so).hexdigest())
PY

echo "[6/7] zipalign + release 签名 + 证书钉"
aligned="$work/aligned.apk"
"$BT/zipalign" -f -p 4 "$work/unsigned.apk" "$aligned"
mkdir -p "$OUT_DIR"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" \
  --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --key-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --out "$OUT_APK" "$aligned"
"$BT/apksigner" verify --print-certs "$OUT_APK" >/dev/null
cert="$("$BT/apksigner" verify --print-certs "$OUT_APK" | sed -n 's/^Signer #1 certificate SHA-256 digest: //p' | head -1)"
# apksigner 输出小写无冒号，keytool 输出大写带冒号 → 归一化后再比
cert_norm="$(printf '%s' "$cert" | tr -d ':' | tr 'A-Z' 'a-z')"
want_norm="$(printf '%s' "$WANT_CERT_SHA256" | tr -d ':' | tr 'A-Z' 'a-z')"
[[ "$cert_norm" == "$want_norm" ]] || die "签名证书指纹不符: $cert (期望 $WANT_CERT_SHA256)"
echo "  output=$OUT_APK"
echo "  size=$(stat -f %z "$OUT_APK") md5=$(md5 -q "$OUT_APK")"
echo "  cert=$cert"

echo "[7/7] 装机前机器关（JNI 配套：Main.cpp regOne vs 新 dex native 声明）"
# 门禁串只收 dex 字面量（查5 对 APK 走 unzip -p 'classes*.dex'，不扫 so）
bash "$PRE_GATE" \
  --cpp "$STARRY_SRC/app/src/main/jni/Main.cpp" \
  --dex "$new_dex" \
  --apk "$OUT_APK" \
  --needle "com/starry/magic/cocos/ShhjHooks" \
  --needle "com/starry/magic/bridge/ModBridge" \
  --needle "com/chx/shhj/GameApplication" \
  || die "pre_install_gate FAIL（详见上方输出）"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] 覆盖安装（不清数据、不卸载）"
  adb -s "$SERIAL" push "$OUT_APK" "$remote"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$remote\"'"
  bash "$VERIFY" "$SERIAL" "$PKG" "$OUT_APK" --su
fi

echo "DONE: $OUT_APK"
