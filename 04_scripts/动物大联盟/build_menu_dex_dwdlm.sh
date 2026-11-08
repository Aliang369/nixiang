#!/usr/bin/env bash
# 动物大联盟 1.0.25 · 菜单 dex 单产物构建（只产 build/menu_dex/classes5.dex）
#
# 为什么单独一条：surgical_dwdlm_1.0.25.sh 的主链是「菜单 smali + 私有工厂(+killer) → 一份注入 dex
# → 立刻整包组装 + 改 Manifest 换 Application」。本阶段外层已换成无壳包（BingoApplication 直装），
# 注入点改走 ContentProvider，因此只需要「菜单 dex」这一件产物，先独立落盘、可复用、可复现。
#
# 生成链（与 surgical 第 2/3 步同源，去掉 killer / Manifest / zip / 签名）：
#   gradle assembleRelease（/tmp 隔离副本）→ apktool d → 抽 smali*/com/starry
#     → smali assemble → <OUT>/classes5.dex
#
# 纪律：不打包、不签名、不装机、不 adb；共享 jniLibs 与共享 app/build 全程不写（只写 /tmp 副本与
#       本游戏私有 build/）。
#
# 用法：
#   bash 04_scripts/动物大联盟/build_menu_dex_dwdlm.sh --dry-run   # 静态预检，零写
#   bash 04_scripts/动物大联盟/build_menu_dex_dwdlm.sh
#   DWDLM_SO_PROFILE=env_shell bash 04_scripts/动物大联盟/build_menu_dex_dwdlm.sh   # 换 so 档（仅校验用）
set -euo pipefail

MODE="build"
for a in "$@"; do
  case "$a" in
    --dry-run) MODE="dry" ;;
    -h|--help) sed -n '2,22p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $a" >&2; exit 2 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
SMALI_BAK="$NIXIANG/01_tools/smali/baksmali"
APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
SO_PROFILE="${DWDLM_SO_PROFILE:-env}"
SO_PREBUILT="$STARRY_SRC/libs_dwdlm/$SO_PROFILE/arm64-v8a/libstarry_magic.so"
OUT_DIR="$SCRIPT_DIR/build/menu_dex"
DEX_NAME="${DWDLM_MENU_DEX_NAME:-classes5.dex}"
REPORT="$SCRIPT_DIR/menu_dex_build.json"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/dexdump" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
ANDROID_JAR=""
for api in 36 35 34 33; do
  if [[ -f "$HOME/Library/Android/sdk/platforms/android-$api/android.jar" ]]; then ANDROID_JAR="$HOME/Library/Android/sdk/platforms/android-$api/android.jar"; break; fi
done
NDK_CXX=""
for cand in "$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so" \
            "$NDK/toolchains/llvm/prebuilt/darwin-aarch64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"; do
  if [[ -f "$cand" ]]; then NDK_CXX="$cand"; break; fi
done

say() { printf "[menu_dex] %s\n" "$*"; }
die() { printf "ERROR: %s\n" "$*" >&2; exit 1; }

# ---- 0. 静态预检 -----------------------------------------------------------
[[ -x "$SMALI_BIN" ]] || die "缺 smali: $SMALI_BIN"
[[ -x "$SMALI_BAK" ]] || die "缺 baksmali: $SMALI_BAK"
[[ -f "$APKTOOL_JAR" ]] || die "缺 apktool: $APKTOOL_JAR"
[[ -n "$BT" ]] || die "缺 build-tools（dexdump）"
[[ -n "$ANDROID_JAR" ]] || die "缺 platforms/android-<api>/android.jar"
[[ -n "$NDK_CXX" ]] || die "缺 NDK libc++_shared.so"
[[ -d "$STARRY_SRC/app/src/main/java/com/starry/magic" ]] || die "缺菜单源: $STARRY_SRC"
[[ -f "$STARRY_SRC/app/src/main/java/com/starry/magic/StarryInitProvider.java" ]] || die "缺 StarryInitProvider.java（注入点不存在）"
[[ -f "$STARRY_SRC/app/src/main/java/com/starry/magic/dwdlm/DwdlmAppComponentFactory.java" ]] || die "缺 dwdlm 私有工厂源"
[[ -f "$SO_PREBUILT" ]] || die "缺 ${SO_PREBUILT}（先跑 dwdlm_build_so.sh profile=${SO_PROFILE}）"
command -v javac >/dev/null 2>&1 || die "缺 javac（JAVA_HOME=${JAVA_HOME}）"
SO_MD5="$(md5 -q "$SO_PREBUILT")"
SO_SHA="$(shasum -a 256 "$SO_PREBUILT" | awk '{print $1}')"
LC_ALL=C grep -aq "dwdlm" "$SO_PREBUILT" || die "so 无 dwdlm 标签（不是本游戏产物）"
if [[ "$SO_PROFILE" == "env" ]]; then
  if LC_ALL=C grep -aq "starry_dwdlm_v1" "$SO_PREBUILT"; then die "env so 内含玩法补丁标记（应编译期剔除）"; fi
  LC_ALL=C grep -aq "DWDLM_ENV_ONLY" "$SO_PREBUILT" || die "env so 缺 DWDLM_ENV_ONLY 横幅"
fi
say "profile=$SO_PROFILE so md5=$SO_MD5 size=$(stat -f '%z' "$SO_PREBUILT")"
say "android.jar=$ANDROID_JAR build-tools=$BT"

if [[ "$MODE" == "dry" ]]; then
  say "[dry-run] 无写入。若执行将产出：$OUT_DIR/$DEX_NAME"
  say "[dry-run] 静态预检全过"
  exit 0
fi

# ---- 1. /tmp 隔离副本 ------------------------------------------------------
LOCK="/tmp/dwdlm_menu_dex.lockdir"
mkdir "$LOCK" 2>/dev/null || die "lock held: ${LOCK}（另一进程在跑？）"
echo $$ > "$LOCK/pid"
WORK="$(mktemp -d /tmp/dwdlm_menu_dex.XXXXXX)"
cleanup() { rm -rf "$WORK"; rm -rf "$LOCK"; }
trap cleanup EXIT
say "[1/6] rsync 隔离副本 → $WORK/starry（共享 jniLibs / app/build 不写）"
rsync -a --delete \
  --exclude ".gradle/" --exclude "app/build/" --exclude "build/" \
  --exclude "auth-server/.venv/" --exclude "auth-server/data/" --exclude ".idea/" \
  --exclude "obj_*/" --exclude "libs_*/" \
  "$STARRY_SRC/" "$WORK/starry/"
mkdir -p "$WORK/starry/app/src/main/jniLibs/arm64-v8a"
cp -f "$SO_PREBUILT" "$WORK/starry/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
cp -f "$NDK_CXX" "$WORK/starry/app/src/main/jniLibs/arm64-v8a/libc++_shared.so"

# ---- 2. gradle assembleRelease（release 变体带 R8，但 -keep com.starry.magic.** 保类名）----
say "[2/6] gradle assembleRelease --offline（release/R8，proguard -keep com.starry.magic.**）"
( cd "$WORK/starry" && ./gradlew assembleRelease --offline --console=plain -q ) \
  || die "gradle assembleRelease 失败（先看上面的 gradle 输出；离线缓存缺失可去掉 --offline 重跑）"
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || die "gradle 未产出 $STARRY_APK"
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
[[ "$(md5 -q "$WORK/check.so")" == "$SO_MD5" ]] || die "gradle APK 内 so 与 libs_$SO_PROFILE 产物 md5 不一致（旧 so 混包）"
say "  APK=$(basename "$STARRY_APK") $(stat -f '%z' "$STARRY_APK")B so 校验 ok"

# ---- 3. apktool d ----------------------------------------------------------
say "[3/6] apktool d（只取 smali，不碰 resources）"
java -jar "$APKTOOL_JAR" d -f -o "$WORK/dec" "$STARRY_APK" >/dev/null

# ---- 4. 抽 com/starry smali ------------------------------------------------
say "[4/6] 抽 smali*/com/starry → 合并树"
mkdir -p "$WORK/smali"
for d in "$WORK/dec"/smali "$WORK"/dec/smali_classes*; do
  if [[ -d "$d/com/starry" ]]; then rsync -a "$d/com/starry" "$WORK/smali/com/"; fi
done
[[ -f "$WORK/smali/com/starry/magic/StarryInitProvider.smali" ]] || die "smali 缺 StarryInitProvider（注入点不存在）"
[[ -f "$WORK/smali/com/starry/magic/menu/MenuService.smali" ]] || die "smali 缺 MenuService（菜单服务没编进去）"
[[ -f "$WORK/smali/com/starry/magic/dwdlm/DwdlmAppComponentFactory.smali" ]] || die "smali 缺 dwdlm 私有工厂"
[[ -f "$WORK/smali/com/starry/magic/cocos/DwdlmHooks.smali" ]] || die "smali 缺 DwdlmHooks（本游戏 Java 桥没进包）"
if [[ -e "$WORK/smali/com/starry/magic/sign/SignSpoof.smali" ]]; then die "smali 含 SignSpoof（2026-10-28 已整体删除，禁止复活）"; fi
if [[ -d "$WORK/smali/bin" ]]; then die "smali 含 bin/（签名杀手不得进本 dex）"; fi
NSMALI="$(find "$WORK/smali" -name "*.smali" | wc -l | tr -d " ")"
say "  smali 文件数=$NSMALI"

# ---- 5. smali assemble → 单 dex -------------------------------------------
say "[5/6] smali assemble → $DEX_NAME"
"$SMALI_BIN" assemble -o "$WORK/$DEX_NAME" "$WORK/smali"
[[ -f "$WORK/$DEX_NAME" ]] || die "smali assemble 未产出 $DEX_NAME"

# ---- 6. 落盘 + 关键类断言 ---------------------------------------------------
say "[6/6] 落盘 $OUT_DIR/$DEX_NAME + 关键类断言"
mkdir -p "$OUT_DIR"
cp -f "$WORK/$DEX_NAME" "$OUT_DIR/$DEX_NAME"
for k in "Lcom/starry/magic/StarryInitProvider;" "Lcom/starry/magic/menu/MenuService;" \
         "Lcom/starry/magic/dwdlm/DwdlmAppComponentFactory;" "Lcom/starry/magic/cocos/DwdlmHooks;"; do
  LC_ALL=C grep -aq "$k" "$OUT_DIR/$DEX_NAME" || die "$DEX_NAME 缺 $k"
done
if LC_ALL=C grep -aq "SignSpoof" "$OUT_DIR/$DEX_NAME"; then die "$DEX_NAME 含 SignSpoof（禁止复活）"; fi
if LC_ALL=C grep -aq "bin/mt/signature" "$OUT_DIR/$DEX_NAME"; then die "$DEX_NAME 含 bin/mt 签名杀手类"; fi
if LC_ALL=C grep -aq "starry_dwdlm_v1" "$OUT_DIR/$DEX_NAME"; then die "$DEX_NAME 含 starry_dwdlm_v1（玩法标记不该进 dex）"; fi
"$BT/dexdump" -c "$OUT_DIR/$DEX_NAME" >/dev/null || die "dexdump -c 校验失败"
DEX_SIZE="$(stat -f '%z' "$OUT_DIR/$DEX_NAME")"
DEX_SHA="$(shasum -a 256 "$OUT_DIR/$DEX_NAME" | awk '{print $1}')"
CDEFS="$("$BT/dexdump" -f "$OUT_DIR/$DEX_NAME" | sed -n 's/^ *class_defs_size *: *//p' | head -1)"

python3 - "$REPORT" "$OUT_DIR/$DEX_NAME" "$DEX_SIZE" "$DEX_SHA" "$CDEFS" "$NSMALI" "$SO_SHA" "$SO_MD5" "$SO_PROFILE" <<'PY'
import json, sys
report, path, size, sha, cdefs, ns, so_sha, so_md5, prof = sys.argv[1:10]
json.dump({
  "dex_path": path, "dex_size": int(size), "dex_sha256": sha,
  "class_defs_size": int((cdefs or "0").strip() or 0), "smali_files": int(ns),
  "so_profile": prof, "so_sha256": so_sha, "so_md5": so_md5,
  "contains": ["com.starry.magic.StarryInitProvider", "com.starry.magic.menu.MenuService",
               "com.starry.magic.dwdlm.DwdlmAppComponentFactory", "com.starry.magic.cocos.DwdlmHooks"],
  "excludes": ["bin/mt/**（签名杀手）", "com.starry.magic.sign.SignSpoof"],
}, open(report, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
PY

say "OK  $OUT_DIR/$DEX_NAME size=$DEX_SIZE sha256=$DEX_SHA class_defs=$CDEFS so(${SO_PROFILE})=$SO_SHA"
