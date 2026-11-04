#!/usr/bin/env bash
# 无限升级 11.266018 / vc1637 —— Windows 端「无加固」出包（2026-11-04）
#
# 口径（用户 2026-11-04）：加固与载荷 sha 全部去掉 ⇒ so 内嵌**明文**补丁 DLL；
# 现有功能全留 + 新增三项（投掷物倍数 / 技能伤害倍数 / 转生无条件升级）；两形态都出。
# 底包 = 云端已装 11 定制包（只换 lib/arm64-v8a/libstarry_magic.so，其余逐字节透传）。
#
# 依赖（本机已装）：
#   01_tools/dotnet-sdk8          补丁器（Mono.Cecil）
#   01_tools/android-sdk/ndk/android-ndk-r27c   ndk-build / llvm-objcopy
#   01_tools/android-sdk/build-tools/android-15 zipalign / apksigner
#   C:/Program Files/Microsoft/jdk-17.0.20.101-hotspot  (apksigner 用)
#   python3（gen_plain_header / pack）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_REAL="$NIXIANG/mod菜单/starry-mod-menu"

# ---------- ASCII junction（mod菜单/ 含中文，ndk-build 在 Windows 下必乱码） ----------
LINK="$NIXIANG/starry_link"
if [[ ! -d "$LINK/app/src/main/jni" ]]; then
  MSYS2_ARG_CONV_EXCL='*' cmd /c mklink /J "$(cygpath -w "$LINK")" "$(cygpath -w "$STARRY_REAL")" || true
fi
[[ -d "$LINK/app/src/main/jni" ]] || { echo "ERROR: 缺 ASCII junction $LINK" >&2; exit 1; }
JNI="$LINK/app/src/main/jni"

PDIR="$NIXIANG/07_patched/无限升级"
DOTNET="$NIXIANG/01_tools/dotnet-sdk8/dotnet.exe"
NDK="${ANDROID_NDK_HOME:-${WXSJ_NDK:-$NIXIANG/01_tools/android-sdk/ndk/android-ndk-r27c}}"
BT="${WXSJ_BUILD_TOOLS:-$NIXIANG/01_tools/android-sdk/build-tools/android-15}"
JAVA_HOME="${JAVA_HOME:-C:/Program Files/Microsoft/jdk-17.0.20.101-hotspot}"
KS="$NIXIANG/01_tools/starry_release.jks"; KS_PASS="${STARRY_KS_PASS:-starry2026}"; KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
BASE_APK="${WXSJ_BASE_APK:-$NIXIANG/.local/wxsj_vc1637_base_20261104/base.apk}"
OUTDIR="$NIXIANG/08_output/无限升级"
BUILD="$NIXIANG/.wxsj_vc1637_build"
PRE="$NDK/toolchains/llvm/prebuilt/windows-x86_64/bin"
OBJCOPY="$PRE/llvm-objcopy.exe"; NMTOOL="$PRE/llvm-nm.exe"

for f in "$DOTNET" "$NDK/ndk-build.cmd" "$OBJCOPY" "$NMTOOL" "$BT/zipalign.exe" "$BT/apksigner.bat" "$KS" "$BASE_APK"; do
  [[ -f "$f" ]] || { echo "ERROR: 缺 $f" >&2; exit 1; }
done
mkdir -p "$OUTDIR" "$BUILD"

NCPU="${NUMBER_OF_PROCESSORS:-8}"

# dotnet/NuGet 在 msys 下拿到 unix 风格 HOME(/c/Users/…) 会报
# "Value cannot be null (Parameter 'path1')" ⇒ 显式喂 Windows 风格用户目录。
WUSERHOME="${WXSJ_WINHOME:-C:/Users/Administrator}"
export HOME="$WUSERHOME" USERPROFILE="$(cygpath -w "$WUSERHOME" 2>/dev/null || echo 'C:\\Users\\Administrator')"
EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null || true)"
[[ "$EPOCH" =~ ^[0-9]+$ ]] || EPOCH=1790184000

size_of() { stat -c%s "$1" 2>/dev/null || stat -f%z "$1"; }

# ---------- 0) 菜单 feat 号 / 枚举 / 回调 一致性（防重排错位） ----------
echo "[0/6] feat 顺序校验"
python "$NIXIANG/04_scripts/无限升级/check_feat_order.py" || { echo "ERROR: feat 映射不一致，禁止出包" >&2; exit 1; }

# ---------- 1) 三变体叠三功能 ----------
echo "[1/6] IL 补丁（hf2/hf4/hf3 + 三功能）"
patch_variant() { # $1=tag $2=official_len
  local tag="$1" off="$2"
  "$DOTNET" run --project "$NIXIANG/04_scripts/无限升级/ilpatch_vc1637_feat3" -c Release -- \
    --src "$PDIR/targetfuntional_vc1637_${tag}_patched.dll" \
    --dst "$PDIR/targetfuntional_vc1637_${tag}_feat3.dll" --maxlen "$off" \
    | grep -E '^\[(src|patch|out|verify)]|warn|ERR' || true
  [[ -f "$PDIR/targetfuntional_vc1637_${tag}_feat3.dll" ]] || { echo "ERROR: ${tag} 补丁失败" >&2; exit 1; }
}
patch_variant hf2 2030080
patch_variant hf4 2028544
patch_variant hf3 2028544

# ---------- 2) 明码头 + 内嵌静态库 ----------
echo "[2/6] 明文头 + objcopy 内嵌"
python "$NIXIANG/04_scripts/无限升级/gen_plain_header.py" --dir "$PDIR"
embed() { # $1=tag $2=lib名
  local tag="$1"
  local lib="$2"
  local bin="targetfuntional_dll_vc1637_${tag}.bin"
  cp -f "$PDIR/targetfuntional_vc1637_${tag}_feat3.dll" "$JNI/$bin"
  ( cd "$JNI" && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 "$bin" "Games/${lib}.a" )
  "$NMTOOL" "$JNI/Games/${lib}.a" 2>/dev/null | grep -q "_binary_targetfuntional_dll_vc1637_${tag}_bin_start" \
    || { echo "ERROR: ${lib}.a 符号名不对" >&2; exit 1; }
  echo "  ${lib}.a <- ${bin} ($(size_of "$JNI/$bin") B)"
}
embed hf2 WxsjVc1637Hf2Dll
embed hf4 WxsjVc1637Hf4Dll
embed hf3 WxsjVc1637Hf3Dll

# ---------- 3) ndk-build（两形态，各自独立 NDK_OUT 防串档） ----------
echo "[3/6] ndk-build"
build_flavor() { # $1=flavor $2=antiban_menu(1/空)
  local fl="$1" ab="$2"
  rm -rf "$BUILD/obj_$fl" "$BUILD/libs_$fl"
  "$NDK/ndk-build.cmd" -C "$JNI" \
    NDK_PROJECT_PATH="../.." APP_BUILD_SCRIPT="./Android_wxsj_vc1637.mk" \
    NDK_APPLICATION_MK="./Application.mk" APP_ABI=arm64-v8a \
    NDK_OUT="$BUILD/obj_$fl" NDK_LIBS_OUT="$BUILD/libs_$fl" \
    WXSJ_ANTIBAN_MENU="$ab" STARRY_BUILD_EPOCH="$EPOCH" -j"$NCPU" >"$BUILD/build_$fl.log" 2>&1 \
    || { echo "ERROR: ndk-build($fl) 失败，日志：$BUILD/build_$fl.log" >&2; tail -30 "$BUILD/build_$fl.log" >&2; exit 1; }
  local so="$BUILD/libs_$fl/arm64-v8a/libstarry_magic.so"
  [[ -f "$so" ]] || { echo "ERROR: 未产出 $so" >&2; exit 1; }
  echo "  $fl: $(size_of "$so") B"
}
build_flavor screen ""
build_flavor normal "1"

# ---------- 4) so 断言 ----------
echo "[4/6] so 断言"
assert_so() { # $1=so $2=flavor
  local so="$1" fl="$2" s
  local req=('[WxsjVC1637]' 'Category_战斗' 'Toggle_无敌' 'Toggle_商店广告无限' 'Toggle_物品使用反增' \
             'Toggle_转生时间无限制' 'Slider_技能投掷物倍数:1:100' 'Slider_技能伤害倍数:1:1000' \
             'Toggle_转生无条件升级' 'skill_projectile_x' 'skill_damage_x' 'zhuan_sheng_free_point' \
             'Toggle_广告奖励倍数生效' 'Toggle_物品倍数生效' 'Toggle_经验倍数生效' \
             'ads_reward_on' 'item_double_on' 'exp_multi_on' \
             'plain-inline' 'targetfuntional')
  local ban=('Slider_经验倍率' 'Toggle_物品翻倍' 'Toggle_广告奖励翻倍' 'antiban baseline unsealed')
  if [[ "$fl" == "normal" ]]; then req+=('Toggle_防封'); else ban+=('Toggle_防封'); fi
  for s in "${req[@]}"; do
    LC_ALL=C grep -aqF -- "$s" "$so" || { echo "ERROR: $so 缺 [$s]" >&2; exit 1; }
  done
  for s in "${ban[@]}"; do
    if LC_ALL=C grep -aqF -- "$s" "$so"; then echo "ERROR: $so 含禁用串 [$s]" >&2; exit 1; fi
  done
  echo "  $fl 断言通过（必须项 ${#req[@]} / 禁止项 ${#ban[@]}）"
}
assert_so "$BUILD/libs_screen/arm64-v8a/libstarry_magic.so" screen
assert_so "$BUILD/libs_normal/arm64-v8a/libstarry_magic.so" normal

# ---------- 5) 外科重打 ----------
echo "[5/6] 重打 APK"
pack() { # $1=flavor $2=outname
  local fl="$1" name="$2"
  local so="$BUILD/libs_$fl/arm64-v8a/libstarry_magic.so"
  local raw="$OUTDIR/${name}.unsigned.apk" aligned="$OUTDIR/${name}.aligned.apk" apk="$OUTDIR/${name}.apk"
  python "$NIXIANG/04_scripts/无限升级/pack_wxsj_vc1637_apk.py" --base "$BASE_APK" --so "$so" --out "$raw"
  "$BT/zipalign.exe" -p -f 4 "$raw" "$aligned"
  JAVA_HOME="$JAVA_HOME" "$BT/apksigner.bat" sign --ks "$KS" --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --ks-key-alias "$KS_ALIAS" --v1-signing-enabled false --v2-signing-enabled true --v3-signing-enabled true \
    --out "$apk" "$aligned" >/dev/null
  rm -f "$raw" "$aligned"
  JAVA_HOME="$JAVA_HOME" "$BT/apksigner.bat" verify --print-certs "$apk" | head -3
  echo "  -> $apk  $(size_of "$apk") B"
}
pack screen "无限升级_屏蔽版_11.266018_vc1637_feat21_20261104"
pack normal "无限升级_正常版_11.266018_vc1637_feat22_20261104"

# ---------- 6) 指纹 ----------
echo "[6/6] 指纹"
for f in "$OUTDIR"/*feat2*.apk; do
  echo "  $(basename "$f")"
  echo "    size=$(size_of "$f")  md5=$(md5sum "$f" | awk '{print $1}')"
done
echo "完成。"
