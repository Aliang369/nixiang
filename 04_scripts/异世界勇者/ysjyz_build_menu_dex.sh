#!/usr/bin/env bash
# 异世界勇者 — 重建「mod 菜单框架」dex（包内 classes3.dex），pipeline 阶段 build_menu_dex
#
# 背景：用户要求 mod 菜单框架 dex 一起换新构建（口述「5.dex」，本包实际是 classes3.dex）。
#       框架 dex = starry-mod-menu 全量 com/starry/**（与其它游戏共用同一份源码，含各自
#       cocos/*Hooks）。本包只用到 com.starry.magic.cocos.YsjYzHooks + MenuService +
#       SignSpoof，其余是他游代码，属正常冗余（与 v5 一致）。
#
# 依赖：gradle 能跑（JDK 17）+ apktool（/opt/homebrew/bin/apktool）+ 01_tools/smali
# 输出：07_patched/异世界勇者/classes3_menu.dex（供 ysjyz_so_swap.sh 置换进包）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
SRC="$NIXIANG/mod菜单/starry-mod-menu"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
APK="$SRC/app/build/outputs/apk/release/app-release.apk"
OUT="$NIXIANG/07_patched/异世界勇者/classes3_menu.dex"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -x "$SMALI_BIN" ]] || { echo "ERROR: 缺 $SMALI_BIN" >&2; exit 1; }
mkdir -p "$(dirname "$OUT")"

echo "[1/3] gradle assembleRelease"
(cd "$SRC" && ./gradlew assembleRelease --console=plain >/dev/null)
[[ -f "$APK" ]] || { echo "ERROR: gradle 没产出 $APK" >&2; exit 1; }
echo "[1/3] app-release.apk = $(stat -f%z "$APK") B  md5=$(md5 -q "$APK")"

WORK="$(mktemp -d /tmp/ysjyz_menu.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[2/3] apktool d → 合并 com/starry smali"
apktool d -f -o "$WORK/dec" "$APK" >/dev/null
mkdir -p "$WORK/smali_in"
shopt -s nullglob
for d in "$WORK/dec"/smali "$WORK/dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$WORK/smali_in/com/"
done
shopt -u nullglob
for f in com/starry/magic/StarryApp.smali \
         com/starry/magic/cocos/YsjYzHooks.smali \
         com/starry/magic/sign/SignSpoof.smali \
         com/starry/magic/menu/MenuService.smali; do
  [[ -f "$WORK/smali_in/$f" ]] || { echo "ERROR: 框架缺 $f（Java 桥没编进 starry app）" >&2; exit 1; }
done

echo "[3/3] smali assemble → $OUT"
"$SMALI_BIN" assemble -o "$OUT" "$WORK/smali_in"

# 硬依赖校验：classes2.dex 的 patch 会调这些方法，签名必须一字不差（否则运行期 NoSuchMethodError）
"$BAKSMALI_BIN" d -o "$WORK/check" "$OUT"
chkcls() {
  local f="$1" sig="$2"
  grep -q -F "$sig" "$WORK/check/com/starry/$f" \
    || { echo "ERROR: classes3 缺 $f -> $sig" >&2; exit 1; }
}
chkcls magic/sign/SignSpoof.smali      '.method public static install()V'
chkcls magic/menu/MenuService.smali    '.method public static start(Landroid/content/Context;)V'
chkcls magic/menu/MenuService.smali    '.method public static notifyHostResumed(Landroid/app/Activity;)V'
chkcls magic/menu/MenuService.smali    '.method public static notifyHostLeaving()V'
chkcls magic/cocos/YsjYzHooks.smali    '.method public static getRet(I)Z'
chkcls magic/cocos/YsjYzHooks.smali    '.method public static getVal(I)I'
chkcls magic/cocos/YsjYzHooks.smali    '.method public static setRet(IZ)V'
chkcls magic/cocos/YsjYzHooks.smali    '.method public static setVal(II)V'

echo "[build_menu_dex] done → $OUT"
echo "[build_menu_dex] size = $(stat -f%z "$OUT") B"
echo "[build_menu_dex] md5  = $(md5 -q "$OUT")"
