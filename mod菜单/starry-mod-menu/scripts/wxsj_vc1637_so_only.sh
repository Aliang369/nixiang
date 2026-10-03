#!/usr/bin/env bash
# 无限升级 11.266018 / vc1637 — 专用 so 构建（三版本变体）
#
# 输入：07_patched/无限升级/targetfuntional_vc1637_v711.bin（base 基线）
#       07_patched/无限升级/targetfuntional_v719_hotfix{2,3}_patched.dll（hf2/hf3 基线）
# 产物：app/src/libs_wxsj_vc1637/arm64-v8a/libstarry_magic.so
#
# 2026-09-29 真机实测服务端下发 2,030,080（hotfix2），不是分析期假设的 1,960,960。
# ildiff 实测两版差 52 类型 / 512 方法（整个 MissionTree 系统）⇒ 必须三版本都内置，
# 按运行时长度/哈希选；无匹配则 PASSTHROUGH 官方版（宁可不生效，不能错版）。
#
# 不覆盖 12.x 的 app/src/libs/arm64-v8a/libstarry_magic.so。
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
STARRY="$ROOT/mod菜单/starry-mod-menu"
JNI="$STARRY/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OBJCOPY="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-objcopy"
NM="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
OBJ="$STARRY/app/src/obj_wxsj_vc1637"
LIBS="$STARRY/app/src/libs_wxsj_vc1637"
OUT="$LIBS/arm64-v8a/libstarry_magic.so"
PATCHER="$ROOT/04_scripts/无限升级/ilpatch_vc1637/ilpatch_vc1637.csproj"
PDIR="$ROOT/07_patched/无限升级"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
[[ -x "$OBJCOPY" ]] || { echo "ERROR: llvm-objcopy missing at $OBJCOPY"; exit 1; }

# 变体表：name|official_len|patched_dll|bin_name|static_lib|baseline
# baseline = 该变体的「未打补丁入口 DLL」。base 有（v7.11）；hf2/hf3 的 12.x v7.19 基线
# 已随工作区瘦身丢失（git 里从来没有过），只能对 07_patched 里既有的补丁产物做增量补丁。
VARIANTS=(
  "v1960960|1960960|$PDIR/targetfuntional_vc1637_patched.dll|targetfuntional_dll_vc1637.bin|Games/WxsjVc1637PatchedDll.a|$PDIR/targetfuntional_vc1637_v711.bin"
  "hf2|2030080|$PDIR/targetfuntional_vc1637_hf2_patched.dll|targetfuntional_dll_vc1637_hf2.bin|Games/WxsjVc1637Hf2Dll.a|$PDIR/targetfuntional_v719_hotfix2_patched.dll"
  "hf3|2028544|$PDIR/targetfuntional_vc1637_hf3_patched.dll|targetfuntional_dll_vc1637_hf3.bin|Games/WxsjVc1637Hf3Dll.a|$PDIR/targetfuntional_v719_hotfix3_patched.dll"
)

echo "[vc1637-so 1/5] IL patch（三变体）"
for row in "${VARIANTS[@]}"; do
  IFS='|' read -r name off dll bin lib base_in <<< "$row"
  [[ -f "$dll" ]] || { echo "ERROR: missing $dll"; exit 1; }
  if [[ -f "$base_in" ]]; then
    dotnet run --project "$PATCHER" --configuration Release -- --variant "$name" >/dev/null
  else
    # 基线缺失（hf2/hf3）：对既有补丁产物做增量补丁（幂等，已带门时原样保留）。
    echo "[vc1637-so]   ${name} 基线缺失（${base_in}）→ 增量补丁既有产物"
    INCR="$(mktemp -t wxsj_vc1637_incr)"
    cp -f "$dll" "$INCR"
    dotnet run --project "$PATCHER" --configuration Release -- --variant "$name" --incremental "$INCR" "$dll" >/dev/null
    rm -f "$INCR"
  fi
  PLEN="$(stat -f%z "$dll")"
  if (( PLEN > off )); then
    echo "ERROR: [$name] patched DLL $PLEN > official $off"; exit 1
  fi
  cp -f "$dll" "$JNI/$bin"
  echo "[vc1637-so]   $name official=$off patched=$PLEN margin=$((off - PLEN))"
done

echo "[vc1637-so 2/5] rebuild embedded static libraries"
# 必须在 $JNI 下用相对文件名，符号名才是 _binary_<bin>_start
for row in "${VARIANTS[@]}"; do
  IFS='|' read -r name off dll bin lib base_in <<< "$row"
  ( cd "$JNI" && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 "$bin" "$lib" )
  sym="_binary_$(echo "$bin" | tr '.' '_')_start"
  if ! "$NM" "$JNI/$lib" 2>/dev/null | grep -q "$sym"; then
    echo "ERROR: $lib 符号名不是 ${sym}（objcopy 输入名不对）"; exit 1
  fi
  TMP_CHECK="$(mktemp -t wxsj_vc1637_a_check)"
  if ! "$OBJCOPY" -O binary --only-section=.data "$JNI/$lib" "$TMP_CHECK" 2>/dev/null \
      || ! cmp -s "$TMP_CHECK" "$JNI/$bin"; then
    echo "ERROR: $lib .data != $bin"
    rm -f "$TMP_CHECK"; exit 1
  fi
  rm -f "$TMP_CHECK"
  echo "[vc1637-so]   $lib data check ok"
done

echo "[vc1637-so 3/5] ndk-build"
rm -rf "$OBJ" "$LIBS"
mkdir -p "$OBJ" "$LIBS"
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="$STARRY/app" \
  APP_BUILD_SCRIPT="$JNI/Android_wxsj_vc1637.mk" \
  NDK_APPLICATION_MK="$JNI/Application.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$LIBS" \
  -j8

[[ -f "$OUT" ]] || { echo "ERROR: missing $OUT"; exit 1; }

echo "[vc1637-so 4/5] so assertions"
LC_ALL=C grep -aq '\[WxsjVC1637\]' "$OUT" || { echo "ERROR: so missing [WxsjVC1637]"; exit 1; }
LC_ALL=C grep -aq 'Toggle_商店广告无限' "$OUT" || { echo "ERROR: so missing Toggle_商店广告无限"; exit 1; }
LC_ALL=C grep -aq 'InputValue_1000:物品反增倍率' "$OUT" || { echo "ERROR: so missing InputValue_1000:物品反增倍率"; exit 1; }
LC_ALL=C grep -aq 'item_reverse_x' "$OUT" || { echo "ERROR: so missing item_reverse_x"; exit 1; }
LC_ALL=C grep -aq 'targetfuntional' "$OUT" || { echo "ERROR: so missing targetfuntional"; exit 1; }

python3 - "$OUT" "${VARIANTS[@]}" <<'PY'
import sys
so = open(sys.argv[1], 'rb').read()
for row in sys.argv[2:]:
    name, off, dll, binname, lib = row.split('|')[:5]
    dll_bytes = open(dll, 'rb').read()
    pos = so.find(dll_bytes)
    if pos < 0:
        raise SystemExit("ERROR: embedded %s bytes not found in so" % name)
    print("in-so OK: %-4s %s (%d B) offset=%d" % (name, dll, len(dll_bytes), pos))
PY

echo "[vc1637-so 5/5] done"
ls -lh "$OUT"
shasum -a 256 "$OUT"
for row in "${VARIANTS[@]}"; do
  IFS='|' read -r name off dll bin lib base_in <<< "$row"
  shasum -a 256 "$dll"
done
