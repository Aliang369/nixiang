#!/usr/bin/env bash
# 重建 classes11.dex：StarryApp→JMYBTApplication（外科必需）+ 最新 Sc33Hooks（含 getAtkMul）
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NX="$(cd "$ROOT/../.." && pwd)"
OUT_DEX="${1:?output dex path required}"
SMALI_BIN="$NX/01_tools/smali/smali"
BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
APK_NAME="app-release.apk"
[[ "$BUILD_TYPE" == "debug" ]] && APK_NAME="app-debug.apk"

WORKDIR=$(mktemp -d)
trap 'rm -rf "$WORKDIR"' EXIT

echo "[1] gradle assembleRelease"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
(cd "$ROOT" && ./gradlew assembleRelease -q --no-daemon)
STARRY_APK="$ROOT/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2] apktool d + StarryApp→JMYBTApplication（与 surgical_sc33 一致）"
apktool d -f -o "$WORKDIR/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORKDIR/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORKDIR/starry_dec"/smali "$WORKDIR/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing" >&2; exit 1; }
grep -q getAtkMul "$SMALI_ROOT/com/starry/magic/cocos/Sc33Hooks.smali" \
  || { echo "ERROR: Sc33Hooks 无 getAtkMul" >&2; exit 1; }

python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/sanqi/jmybt/game37/JMYBTApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/sanqi/jmybt/game37/JMYBTApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->onCreate()V",
    ),
]
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
if "Lcom/sanqi/jmybt/game37/JMYBTApplication;" not in t:
    raise SystemExit("ERROR: StarryApp missing JMYBTApplication super")
if ".method protected attachBaseContext" not in t:
    t = t.rstrip() + """

.method protected attachBaseContext(Landroid/content/Context;)V
    .locals 1
    .param p1, "base"    # Landroid/content/Context;

    invoke-super {p0, p1}, Lcom/sanqi/jmybt/game37/JMYBTApplication;->attachBaseContext(Landroid/content/Context;)V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    return-void
.end method
"""
p.write_text(t, encoding="utf-8")
print("  StarryApp → JMYBTApplication ok")
PY

echo "[3] assemble classes11.dex"
"$SMALI_BIN" assemble -o "$OUT_DEX" "$SMALI_ROOT"
strings "$OUT_DEX" | grep -q JMYBTApplication
strings "$OUT_DEX" | grep -q getAtkMul
strings "$OUT_DEX" | grep -q isCocosEvalSafe
echo "[ok] classes11 $(wc -c < "$OUT_DEX") bytes"
