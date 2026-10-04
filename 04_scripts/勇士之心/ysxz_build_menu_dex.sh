#!/usr/bin/env bash
# 重建 1.0.3 的星空菜单 dex（classes4.dex）——「菜单源码」的部分。
#
# 背景：菜单的**外观**（星空面板 / 星光紫配色 / 布局）在 Java 侧
#   app/src/main/java/com/starry/magic/menu/MagicRes.java
#   app/src/main/java/com/starry/magic/menu/NativeMenuController.java
# 打包成包内 classes4.dex；而菜单的**项目清单**（名称/序号/类型）在 so 里
# （app/src/main/jni/Games/YsxzEntry.cpp::kYsxz_Features[]，见 ysxz_build_so_103.sh）。
# 两者必须同时是新版，否则会出现「菜单项是新的、外观是旧的」或序号错位。
#
# 本脚本从仓库当前菜单源码重编 classes4.dex，并断言：
#   1) 含 MagicRes$PanelBgDrawable（2026-09-22 星光紫新外观）
#   2) 不含 1.0.4 的过检测类 YsxzAppComponentFactory（1.0.3 不需要）
# 产物：mod菜单/starry-mod-menu/libs_ysxz/classes4.dex
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${NIXIANG:-$(cd "$HERE/../.." && pwd)}"
SRC="$ROOT/mod菜单/starry-mod-menu"
OUT="$SRC/libs_ysxz"
WORK="${TMPDIR:-/tmp}/ysxz_menu_dex.$$"
APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"

say() { printf '%s\n' "$*" >&2; }
die() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }

[[ -d "$SRC/app/src/main/java/com/starry/magic/menu" ]] || die "找不到菜单源码目录（${SRC}）"
[[ -f "$ROOT/01_tools/apktool/apktool.jar" ]] || die "缺 $ROOT/01_tools/apktool/apktool.jar"

# ---- JDK ----
if [[ -z "${JAVA_HOME:-}" ]]; then
  if [[ -x /usr/libexec/java_home ]]; then
    JAVA_HOME="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
  fi
fi
command -v java >/dev/null 2>&1 || die "找不到 java（装 JDK 17 或设 JAVA_HOME）"
say "JDK: $(java -version 2>&1 | head -1)"

# ---- Android SDK（不写死机器路径）----
SDK=""
for cand in "${ANDROID_SDK_ROOT:-}" "${ANDROID_HOME:-}" \
            "${LOCALAPPDATA:+$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")/Android/Sdk}" \
            "$HOME/Android/Sdk" "$HOME/Library/Android/sdk"; do
  [[ -n "$cand" && -d "$cand" ]] || continue
  [[ -d "$cand/platforms" || -d "$cand/build-tools" ]] && { SDK="$cand"; break; }
done
[[ -n "$SDK" ]] || die "找不到 Android SDK（设 ANDROID_HOME=/path/to/Android/Sdk）"
export ANDROID_HOME="$SDK" ANDROID_SDK_ROOT="$SDK"
say "SDK: $SDK"

trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/starry"

say "[1/4] 复制 starry 源码（排除构建产物）"
tar -cf - -C "$SRC" \
  --exclude='.gradle' --exclude='app/build' --exclude='build' \
  --exclude='obj_*' --exclude='libs_*' --exclude='auth-server/.venv' \
  --exclude='auth-server/data' --exclude='.idea' . | tar -xf - -C "$WORK/starry"
# gradle 优先读 local.properties 的 sdk.dir，写成本机路径
printf 'sdk.dir=%s\n' "$(cygpath -m "$SDK" 2>/dev/null || echo "$SDK")" > "$WORK/starry/local.properties"

say "[2/4] gradle assembleRelease"
( cd "$WORK/starry" && chmod +x ./gradlew 2>/dev/null || true
  ./gradlew assembleRelease --console=plain )
[[ -f "$APK" ]] || die "gradle 没产出 $APK"

say "[3/4] starry com/starry smali → classes4.dex"
java -jar "$ROOT/01_tools/apktool/apktool.jar" d -f -o "$WORK/dec" "$APK" >"$WORK/apktool.log" 2>&1 \
  || { tail -20 "$WORK/apktool.log" >&2; die "apktool 解包失败"; }
SM="$WORK/smali_in"
mkdir -p "$SM"
shopt -s nullglob
for d in "$WORK/dec"/smali "$WORK/dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  cp -a "$d/com/starry" "$SM/"
done
shopt -u nullglob
# 1.0.4 的过检测类不搬（1.0.3 的 Manifest 用 androidx.core.app.CoreComponentFactory）
find "$SM" -name 'YsxzAppComponentFactory*.smali' -delete 2>/dev/null || true
find "$SM" -type d -empty -delete 2>/dev/null || true
java -jar "$ROOT/01_tools/smali/smali-fat.jar" assemble "$SM" -o "$WORK/classes4.dex" >/dev/null

DEX="$WORK/classes4.dex"
[[ -f "$DEX" ]] || die "smali 组装失败"

say "[4/4] 断言 + 落盘"
LC_ALL=C grep -aq -- 'MagicRes$PanelBgDrawable' "$DEX" \
  || die "新 dex 里没有 MagicRes\$PanelBgDrawable —— 菜单源码是美化前的旧外观"
if LC_ALL=C grep -aq -- 'YsxzAppComponentFactory' "$DEX"; then
  die "新 dex 里混进了 1.0.4 的过检测类 YsxzAppComponentFactory"
fi
SZ="$(wc -c < "$DEX")"
[[ "$SZ" -ge 400000 && "$SZ" -le 550000 ]] || die "classes4.dex 体积异常：$SZ"
mkdir -p "$OUT"
cp -f "$DEX" "$OUT/classes4.dex"
say "OK: $OUT/classes4.dex  ($SZ B, md5 $(md5sum "$OUT/classes4.dex" | cut -d' ' -f1))"
say "    下一步：重打包时同时替换 classes4.dex 与 libstarry_magic.so（两者为一组，缺一不可）"
