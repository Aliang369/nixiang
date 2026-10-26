#!/usr/bin/env bash
# 无限升级 wxsjdz2（12.10701 / vc1644 定制号）— 专用 so 构建（双载荷变体）
#
# 载荷形态：P2-2 加密（密文进包 + 运行时用票里的 kx 解封；无票落防封基线）
#
# 载荷链（两套变体，各自都带三个新功能）：
#   live = 现网 2,028,544 / fnv 0x1bf6c08f1ba38809
#          newdump_20261026/patched_new.dll（v1 31 patches + δ 的 A 线交付口径）
#          → ilpatch_wxsjdz2（投掷物/技能伤害/转生免轮回点）
#   hf2  = 回退 2,030,080 / fnv 0x91f9acbd064fa875
#          官方 targetfuntional_OFFICIAL_12.10701_hotfix2_2030080.dll
#          → ilpatch（v1）→ ilpatch_delta（δ）→ ilpatch_wxsjdz2
#
# 产物：app/src/libs_wxsjdz2/arm64-v8a/libstarry_magic.so
# 不覆盖 12 线 wxsj 的 app/src/libs/ 与 11 线 wxsj_vc1637 的 libs_wxsj_vc1637/。
set -euo pipefail

ROOT="${ROOT:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
STARRY="$ROOT/mod菜单/starry-mod-menu"
JNI="$STARRY/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OBJCOPY="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-objcopy"
NM="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
OBJ="$STARRY/app/src/obj_wxsjdz2"
LIBS="$STARRY/app/src/libs_wxsjdz2"
OUT="$LIBS/arm64-v8a/libstarry_magic.so"
PDIR="$ROOT/07_patched/无限升级"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
[[ -x "$OBJCOPY" ]] || { echo "ERROR: llvm-objcopy missing at $OBJCOPY"; exit 1; }

echo "[wxsjdz2-so 1/6] IL 补丁链（live + hf2 两套变体）"
# ---- live：A 线 v1+δ 产物 → 三个新功能 ----
LIVE_SRC="$PDIR/newdump_20261026/patched_new.dll"
LIVE_DLL="$PDIR/targetfuntional_wxsjdz2_patched.dll"
[[ -f "$LIVE_SRC" ]] || { echo "ERROR: 缺 live 基线 $LIVE_SRC"; exit 1; }
WXSJ_SRC="$LIVE_SRC" WXSJ_DST="$LIVE_DLL" \
  dotnet run --project "$ROOT/04_scripts/无限升级/ilpatch_wxsjdz2" --configuration Release >/tmp/wxsjdz2_live_patch.log 2>&1 || {
  echo "ERROR: live 变体补丁失败"; tail -20 /tmp/wxsjdz2_live_patch.log; exit 1; }
grep -E "^\[out\]|skip" /tmp/wxsjdz2_live_patch.log || true

# ---- hf2：官方 2030080 全链重放 ----
HF2_OFF="$PDIR/targetfuntional_OFFICIAL_12.10701_hotfix2_2030080.dll"
HF2_V1="/tmp/wxsjdz2_v1_hf2.dll"
HF2_DELTA="/tmp/wxsjdz2_delta_hf2.dll"
HF2_DLL="$PDIR/targetfuntional_wxsjdz2_hf2_patched.dll"
[[ -f "$HF2_OFF" ]] || { echo "ERROR: 缺官方 hf2 底包 $HF2_OFF"; exit 1; }
WXSJ_SRC="$HF2_OFF" WXSJ_DST="$HF2_V1" \
  dotnet run --project "$ROOT/04_scripts/无限升级/ilpatch" --configuration Release >/tmp/wxsjdz2_hf2_v1.log 2>&1 || {
  echo "ERROR: hf2 v1 阶段失败"; tail -20 /tmp/wxsjdz2_hf2_v1.log; exit 1; }
WXSJ_SRC="$HF2_V1" WXSJ_DST="$HF2_DELTA" \
  dotnet run --project "$ROOT/04_scripts/无限升级/ilpatch_delta" --configuration Release >/tmp/wxsjdz2_hf2_delta.log 2>&1 || {
  echo "ERROR: hf2 δ 阶段失败"; tail -20 /tmp/wxsjdz2_hf2_delta.log; exit 1; }
WXSJ_SRC="$HF2_DELTA" WXSJ_DST="$HF2_DLL" \
  dotnet run --project "$ROOT/04_scripts/无限升级/ilpatch_wxsjdz2" --configuration Release >/tmp/wxsjdz2_hf2_patch.log 2>&1 || {
  echo "ERROR: hf2 变体补丁失败"; tail -20 /tmp/wxsjdz2_hf2_patch.log; exit 1; }
grep -E "^\[out\]|skip" /tmp/wxsjdz2_hf2_patch.log || true
rm -f "$HF2_V1" "$HF2_DELTA"

echo "[wxsjdz2-so 2/6] 尺寸校验 + 载荷头新鲜度（C6 同口径）"
LIVE_LEN="$(stat -f%z "$LIVE_DLL")"
HF2_LEN="$(stat -f%z "$HF2_DLL")"
(( LIVE_LEN <= 2028544 )) || { echo "ERROR: live 补丁 $LIVE_LEN > 官方数组 2028544"; exit 1; }
(( HF2_LEN  <= 2030080 )) || { echo "ERROR: hf2 补丁 $HF2_LEN > 官方数组 2030080"; exit 1; }
echo "[wxsjdz2-so]   live official=2028544 patched=$LIVE_LEN margin=$((2028544-LIVE_LEN))"
echo "[wxsjdz2-so]   hf2  official=2030080 patched=$HF2_LEN margin=$((2030080-HF2_LEN))"
# 加固版不用 patchedBufHash 做幂等（改为运行时 memcmp 比对解封后的明文），
# 但载荷头必须与当前明文一致，否则包里是旧密文 ⇒ 运行期解封比对错版。
python3 "$ROOT/04_scripts/_common/gen_payload_enc.py" --game wxsjdz2 --check >/tmp/wxsjdz2_payload_check.log 2>&1 || {
  echo "ERROR: 载荷头不新鲜（gen_payload_enc.py --check 失败）"; tail -10 /tmp/wxsjdz2_payload_check.log; exit 1; }
tail -2 /tmp/wxsjdz2_payload_check.log || true

echo "[wxsjdz2-so 3/6] 载荷加密（P2-2）+ 密文静态库"
# 明文补丁**不再进 jni/so**：先由共享生成器把 live/hf2/防封基线加密成 *.enc.bin（密文 + 32B MAC），
# 再 objcopy 内嵌。运行时用 v3 票里的 kx 还原 K_build 才解得开，解不开落诱饵/防封基线。
python3 "$ROOT/04_scripts/_common/gen_payload_enc.py" --game wxsjdz2 >/tmp/wxsjdz2_payload.log 2>&1 || {
  echo "ERROR: 载荷加密失败（gen_payload_enc.py --game wxsjdz2）" >&2
  tail -20 /tmp/wxsjdz2_payload.log >&2; exit 1; }
grep -E "载荷 sha256|密文头已写|新 sha" /tmp/wxsjdz2_payload.log || true
for row in "targetfuntional_dll_wxsjdz2.enc.bin|Games/Wxsjdz2PayloadDll.a" \
           "targetfuntional_dll_wxsjdz2_hf2.enc.bin|Games/Wxsjdz2Hf2Dll.a" \
           "targetfuntional_dll_wxsjdz2_antiban.enc.bin|Games/Wxsjdz2AntibanDll.a"; do
  IFS='|' read -r bin lib <<< "$row"
  [[ -f "$JNI/$bin" ]] || { echo "ERROR: 缺密文段 $JNI/$bin"; exit 1; }
  ( cd "$JNI" && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 "$bin" "$lib" )
  sym="_binary_$(echo "$bin" | tr '.' '_')_start"
  "$NM" "$JNI/$lib" 2>/dev/null | grep -q "$sym" || { echo "ERROR: $lib 符号名不是 $sym"; exit 1; }
  TMP_CHECK="$(mktemp -t wxsjdz2_a_check)"
  "$OBJCOPY" -O binary --only-section=.data "$JNI/$lib" "$TMP_CHECK" 2>/dev/null || { rm -f "$TMP_CHECK"; echo "ERROR: $lib 抽 .data 失败"; exit 1; }
  cmp -s "$TMP_CHECK" "$JNI/$bin" || { rm -f "$TMP_CHECK"; echo "ERROR: $lib .data ≠ $bin"; exit 1; }
  rm -f "$TMP_CHECK"
  echo "[wxsjdz2-so]   $lib data check ok"
done

echo "[wxsjdz2-so 4/6] ndk-build（Android_wxsjdz2.mk）"
rm -rf "$OBJ" "$LIBS"
mkdir -p "$OBJ" "$LIBS"
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="$STARRY/app" \
  APP_BUILD_SCRIPT="$JNI/Android_wxsjdz2.mk" \
  NDK_APPLICATION_MK="$JNI/Application.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$LIBS" \
  -j8
BUILT="$LIBS/arm64-v8a/libstarry_magic.so"
[[ -f "$BUILT" ]] || { echo "ERROR: missing $BUILT"; exit 1; }

echo "[wxsjdz2-so 5/6] 断言（必须有 / 必须没有）"
must_have=(
  "[Wxsjdz2]"
  "Toggle_无敌"
  "Toggle_秒杀"
  "Slider_经验倍率:1:10"
  "Slider_技能投掷物倍数:1:100"
  "Slider_技能伤害倍数:1:1000"
  "Toggle_转生无条件升级"
  "skill_projectile_x"
  "skill_damage_x"
  "zhuan_sheng_free_point"
  "targetfuntional"
  "wxsjdz2"
  "payload decrypted"
  "-> decoy"
)
must_not=(
  "[WxsjVC1637]"
  "Toggle_物品使用反增"
  "Toggle_物品反增倍率"
  "Toggle_转生时间无限制"
  "Toggle_商店广告无限"
  "Toggle_防封"
  "DIAG dumped"
  "[Cwjh]"
)
# 注意：必须 grep -F（固定串）。用正则时 "[WxsjVC1637]" 会被当成字符集，几乎什么都命中/漏判。
for s in "${must_have[@]}"; do
  LC_ALL=C grep -aFq -- "$s" "$BUILT" || { echo "ERROR: so 缺必需串 [$s]"; exit 1; }
done
for s in "${must_not[@]}"; do
  if LC_ALL=C grep -aFq -- "$s" "$BUILT"; then echo "ERROR: so 含禁用串 [$s]（越界/串线）"; exit 1; fi
done
echo "[wxsjdz2-so]   必需串 ${#must_have[@]} 条齐全 / 禁用串 ${#must_not[@]} 条 0 命中"

# 终局断言（加固版）：so 内不得出现任何一份载荷明文（只有密文可进包）；
# 且必须含 C7 的载荷层标记（payload decrypted / -> decoy），否则说明载荷层没编进来。
python3 - "$BUILT" "$JNI/targetfuntional_dll_wxsjdz2.enc.bin" "$LIVE_DLL" "$HF2_DLL" "$PDIR/targetfuntional_wxsjdz2_antiban.dll" <<'PYCHK'
import sys
so = open(sys.argv[1], "rb").read()
enc = open(sys.argv[2], "rb").read()
if so.find(enc) < 0:
    print("MISSING in so: 密文段 %s (%d B)" % (sys.argv[2].split("/")[-1], len(enc))); sys.exit(1)
print("  in-so OK: 密文 %s (%d B)" % (sys.argv[2].split("/")[-1], len(enc)))
for plain in sys.argv[3:]:
    d = open(plain, "rb").read()
    if so.find(d[:4096]) >= 0:
        print("LEAK in so: 明文载荷片段出现在包里 %s" % plain.split("/")[-1]); sys.exit(1)
    print("  no-leak OK: %s 明文不在 so 内" % plain.split("/")[-1])
PYCHK

echo "[wxsjdz2-so 6/6] 发布 → $OUT"
mkdir -p "$(dirname "$OUT")"
# NDK_LIBS_OUT 就是 libs_wxsjdz2 ⇒ BUILT 与 OUT 常为同一文件，cp 自比会返回 1（macOS）触发 set -e
if [[ "$BUILT" != "$OUT" ]]; then cp -f "$BUILT" "$OUT"; fi
ls -lh "$OUT"
shasum -a 256 "$OUT"
echo "done"
