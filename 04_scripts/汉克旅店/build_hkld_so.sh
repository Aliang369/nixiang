#!/usr/bin/env bash
# 汉克旅店 — so 构建后端（四件套：绝对路径 obj / obj stamp / 链接活动保险 / 菜单串字节校验）
#
# 步骤：①生成载荷头（长度+sha） ②objcopy 内嵌补丁 DLL 为 .a ③ndk-build ④同步 jniLibs ⑤自检
# 用法: bash 04_scripts/汉克旅店/build_hkld_so.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
DLL="$NIXIANG/07_patched/汉克旅店/Assembly-CSharp_hkld.dll"
OBJCOPY="$(ls "$NDK"/toolchains/llvm/prebuilt/*/bin/llvm-objcopy | head -1)"
NM="$(ls "$NDK"/toolchains/llvm/prebuilt/*/bin/llvm-nm | head -1)"
JOBS="$(sysctl -n hw.ncpu 2>/dev/null || echo 4)"

[[ -f "$DLL" ]] || { echo "ERROR: 缺补丁 DLL ${DLL}（先跑 04_scripts/汉克旅店/ilpatch）"; exit 1; }
[[ -x "$OBJCOPY" ]] || { echo "ERROR: 缺 llvm-objcopy"; exit 1; }

echo "[1/5] 载荷头（长度 + sha256）"
python3 "$SCRIPT_DIR/gen_hkld_payload_header.py" --dll "$DLL"

echo "[2/5] objcopy 内嵌 .a"
TMP="$(mktemp -d)"; trap 'rm -rf "$TMP"' EXIT
cp "$DLL" "$TMP/hkld_patched_dll.bin"
( cd "$TMP" && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 hkld_patched_dll.bin hkld.a )
mv -f "$TMP/hkld.a" "$JNI/Games/HkldPatchedDll.a"
"$NM" "$JNI/Games/HkldPatchedDll.a" | grep -q '_binary_hkld_patched_dll_bin_start' \
  || { echo "ERROR: .a 符号名不对（应 _binary_hkld_patched_dll_bin_start）"; exit 1; }
echo "  Games/HkldPatchedDll.a <- $DLL ($(wc -c < "$DLL" | tr -d ' ') B)"

echo "[3/5] ndk-build（绝对路径 obj，禁止相对路径假成功）"
rm -rf "$STARRY/obj_hkld"\\/"stamp" 2>/dev/null || true
( cd "$JNI" && "$NDK/ndk-build" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_hkld.mk \
    NDK_APPLICATION_MK=./Application.mk APP_ABI="arm64-v8a" \
    NDK_OUT="$STARRY/obj_hkld" NDK_LIBS_OUT="$STARRY/libs_hkld" -j"$JOBS" )
SO="$STARRY/libs_hkld/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: 缺 $SO"; exit 1; }

echo "[4/5] 同步 jniLibs（Gradle 只打包 jniLibs）"
for dest in "$STARRY/app/src/main/jniLibs/arm64-v8a" "$STARRY/app/src/main/libs/arm64-v8a" "$STARRY/app/libs/arm64-v8a"; do
  mkdir -p "$dest"; cp -f "$SO" "$dest/libstarry_magic.so"
done

echo "[5/5] 产物自检"
LC_ALL=C grep -a -q '\[Hkld\]' "$SO" || { echo "ERROR: so 无 [Hkld] 标签"; exit 1; }
for s in 无敌 伤害倍率 无限技能 解锁角色 无限金币 无限钥匙; do
  LC_ALL=C grep -a -q "$s" "$SO" || { echo "ERROR: so 缺菜单串 $s"; exit 1; }
done
"$NM" -D --undefined-only "$SO" | grep -q emutls && { echo "ERROR: so 含 __emutls_* 未定义符号"; exit 1; }
"$NM" -D --defined-only "$SO" | grep -q ' T JNI_OnLoad' || { echo "ERROR: so 未导出 JNI_OnLoad"; exit 1; }
echo "  OK  $SO  size=$(wc -c < "$SO" | tr -d ' ') B  md5=$(md5 -q "$SO")"
