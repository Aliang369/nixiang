#!/usr/bin/env bash
# 无悔华夏 3.8.280（九游 aligames）：从当前 Gradle 源码重建菜单 classes2.dex
#
# 语义：只重建菜单 dex（不含 so）。so 走 04_scripts/无悔华夏/whrhx_build_so.sh。
# 关键坑：壳 App 的 .super 必须在注入时改掉 —— 游戏内在用 dex 里 StarryApp 父类是
#   Lcom/netease/nis/wrapper/MyApplication;（网易易盾 NIS 壳，定义在游戏 classes.dex），
#   而 gradle 构建出来的是 Landroid/app/Application;（详见
#   10_docs/无悔华夏/热更失效修复_20260923.md §12.4）。
#
# 产物：07_patched/无悔华夏/menu_latest_<date>/classes2.dex + MENU_SOURCE.txt
#
# 用法：bash 04_scripts/无悔华夏/whrhx_build_latest_dex.sh
set -euo pipefail

trap 'rc=$?; echo "ERROR: whrhx_build_latest_dex.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
DATE_TAG="${WHRHX_DEX_DATE:-$(date +%Y%m%d)}"
OUT_DIR="${WHRHX_DEX_OUT_DIR:-$NIXIANG/07_patched/无悔华夏/menu_latest_$DATE_TAG}"
OUT_DEX="$OUT_DIR/${WHRHX_DEX_NAME:-classes2.dex}"
# 壳父类：九游/快爆=易盾 NIS MyApplication（留在包内做壳）；B 站/4399=无壳重建，父类改成乐变 LBApplication
SUPER_CLASS="${WHRHX_SUPER_CLASS:-Lcom/netease/nis/wrapper/MyApplication;}"
SMALI="$NIXIANG/01_tools/smali/smali"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
BT_DEXDUMP="$HOME/Library/Android/sdk/build-tools/36.0.0/dexdump"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -x "$SMALI" ]] || { echo "ERROR: $SMALI missing" >&2; exit 1; }
[[ -n "$JAVA_HOME" ]] || { echo "ERROR: JDK 17 not found" >&2; exit 1; }
command -v apktool >/dev/null || { echo "ERROR: apktool missing" >&2; exit 1; }
[[ -f "$STARRY_SRC/app/src/main/java/com/starry/magic/ota/OtaSoStamp.java" ]] \
  || { echo "ERROR: OtaSoStamp.java missing（本轮 OTA 守卫修复未在工作区）" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx_latest_dex.XXXXXX)"
trap 'rm -rf "$WORK"; rc=$?; [ $rc -eq 0 ] || echo "ERROR: whrhx_build_latest_dex.sh rc=$rc" >&2' EXIT

echo "[1/5] rsync 隔离副本 + gradle assembleRelease"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'libs_*/' --exclude 'obj_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
APP_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$APP_APK" ]] || { echo "ERROR: gradle app-release.apk missing" >&2; exit 1; }
echo "  app-release.apk $(stat -f %z "$APP_APK")B sha256 $(shasum -a 256 "$APP_APK" | awk '{print $1}')"

echo "[2/5] apktool d → 汇总 com/starry smali"
apktool d -f -o "$WORK/starry_dec" "$APP_APK" >/dev/null
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
SMALI_COUNT="$(find "$SMALI_ROOT" -name '*.smali' | wc -l | tr -d ' ')"
echo "  smali 文件数 = $SMALI_COUNT"

echo "[3/5] StarryApp 父类外科补丁（${SUPER_CLASS}）"
APP_SMALI="$SMALI_ROOT/com/starry/magic/StarryApp.smali"
[[ -f "$APP_SMALI" ]] || { echo "ERROR: StarryApp.smali missing" >&2; exit 1; }
SUPER_CLASS="$SUPER_CLASS" python3.11 - "$APP_SMALI" <<'PY'
from pathlib import Path
import os, sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
SUPER = os.environ["SUPER_CLASS"]
repls = [
    (".super Landroid/app/Application;", ".super " + SUPER),
    ("invoke-direct {p0}, Landroid/app/Application;-><init>()V",
     "invoke-direct {p0}, " + SUPER + "-><init>()V"),
    ("invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
     "invoke-super {p0, p1}, " + SUPER + "->attachBaseContext(Landroid/content/Context;)V"),
    ("invoke-super {p0}, Landroid/app/Application;->onCreate()V",
     "invoke-super {p0}, " + SUPER + "->onCreate()V"),
]
hits = []
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        hits.append(a.split("->")[-1].split("(")[0])
if SUPER not in t:
    raise SystemExit("ERROR: StarryApp missing MyApplication super")
if ".method protected attachBaseContext" not in t:
    t = t.rstrip() + """

.method protected attachBaseContext(Landroid/content/Context;)V
    .locals 1
    .param p1, "base"    # Landroid/content/Context;

    invoke-super {p0, p1}, """ + SUPER + """->attachBaseContext(Landroid/content/Context;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    return-void
.end method
"""
    print("  StarryApp +attachBaseContext loadLibrary")
p.write_text(t, encoding="utf-8")
print("  StarryApp patched -> MyApplication, 替换命中:", hits)
PY

echo "[4/5] smali assemble → classes2.dex"
mkdir -p "$OUT_DIR"
"$SMALI" assemble -o "$OUT_DEX" "$SMALI_ROOT"
echo "  classes2.dex $(stat -f %z "$OUT_DEX")B"

echo "[5/5] 产物校验"
SUPER_CLASS="$SUPER_CLASS" python3.11 - "$OUT_DEX" "$SMALI_ROOT" <<'PY'
from pathlib import Path
import os, sys
dex = Path(sys.argv[1]).read_bytes()
smali_root = Path(sys.argv[2])
checks = [
    (b'STARRYSTAMP1:EPOCH=', 'OtaSoStamp 戳字面量'),
    (b'keep OTA: ', 'OTA 守卫保留日志'),  # 2026-10-21：源码改为 "keep OTA: " + why，格式串不再是字面量
    (b'nativeGetBuildStamp', '新桥接声明'),
    (b'com/starry/magic/ota/OtaSoStamp', 'OtaSoStamp 类描述符'),
    (os.environ.get('SUPER_CLASS', 'Lcom/netease/nis/wrapper/MyApplication;').encode(), '壳父类'),
]
for needle, label in checks:
    if needle not in dex:
        raise SystemExit(f'ERROR: dex 缺 {label} ({needle!r})')
    print(f'  ok {label}')
if b'ota_apk_last_update_time' in dex:
    print('  note: dex 仍含 ota_apk_last_update_time 字符串（Preferences 遗留常量，已无调用方）')
if not (smali_root / 'com/starry/magic/ota/OtaSoStamp.smali').is_file():
    raise SystemExit('ERROR: OtaSoStamp.smali 未生成')
print('  class files =', len(list(smali_root.rglob('*.smali'))))
PY

if [[ -x "$BT_DEXDUMP" ]]; then
  "$BT_DEXDUMP" -f "$OUT_DEX" > "$WORK/dexdump.txt" 2>/dev/null || true
  for name in nativeGetBuildStamp nativeVerifyOtaSig nativeApplyPlayPatch nativeGetPinnedCert; do
    grep -Fq "$name" "$WORK/dexdump.txt" || { echo "ERROR: dexdump 缺 $name" >&2; exit 1; }
  done
  echo "  dexdump native 声明校验通过"
fi

SUPER_CLASS="$SUPER_CLASS" python3.11 - "$OUT_DEX" "$OUT_DIR/MENU_SOURCE.txt" "$SMALI_COUNT" <<'PY'
from pathlib import Path
import hashlib, os, sys
dex, out, count = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3]
b = dex.read_bytes()
SUPER_CLASS = os.environ.get('SUPER_CLASS', 'Lcom/netease/nis/wrapper/MyApplication;')
out.write_text(f"""无悔华夏 3.8.280（九游 aligames）菜单 dex — 重建于 {__import__('datetime').date.today()}
================================================================
来源：mod菜单/starry-mod-menu 工作区（含 OTA 守卫 build-stamp 修复：
  OtaSoStamp.java / PlayOtaManager.invalidateOtaIfApkUpdated 重写 / ModBridge.nativeGetBuildStamp）
构建：{dex.parent} （rsync 隔离副本，不动工作区）
  JAVA_HOME=jdk-17 ./gradlew assembleRelease --console=plain
  apktool d → 汇总 smali*/com/starry → {count} 个 .smali
外科补丁：StarryApp.smali 4 处 Landroid/app/Application; → {SUPER_CLASS}
产物：classes2.dex  {len(b)}B
  sha256 {hashlib.sha256(b).hexdigest()}
  md5    {hashlib.md5(b).hexdigest()}
校验：STARRYSTAMP1:EPOCH= 字面量 / keep OTA: apkSoStamp= 新守卫日志 /
  nativeGetBuildStamp 桥接声明 / com.starry.magic.ota.OtaSoStamp 类 / NIS 壳父类 —— 全部命中
""", encoding='utf-8')
print(f"  MENU_SOURCE.txt -> {out}")
PY

printf '[latest_dex] output=%s\n[latest_dex] md5=%s\n[latest_dex] sha256=%s\n' \
  "$OUT_DEX" "$(md5 -q "$OUT_DEX")" "$(shasum -a 256 "$OUT_DEX" | awk '{print $1}')"
