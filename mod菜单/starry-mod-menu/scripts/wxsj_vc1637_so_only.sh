#!/usr/bin/env bash
# 无限升级 11.266018 / vc1637 — 专用 so 构建（四版本变体）
#
# 输入：07_patched/无限升级/targetfuntional_vc1637_v711.bin（base 基线）
#       07_patched/无限升级/targetfuntional_v719_hotfix{2,3}_patched.dll（hf2/hf3 基线）
#       07_patched/无限升级/newdump_20261026/patched_new.dll（hf4 基线 = 2026-10-26 现网热更的 A 线补丁产物）
# 产物：app/src/libs_wxsj_vc1637/arm64-v8a/libstarry_magic.so
#
# 2026-09-29 真机实测服务端下发 2,030,080（hotfix2），不是分析期假设的 1,960,960。
# ildiff 实测两版差 52 类型 / 512 方法（整个 MissionTree 系统）⇒ 必须按运行时长度/哈希选变体；
# 无匹配则 PASSTHROUGH 官方版（宁可不生效，不能错版）。
# 2026-10-26：现网热更换成 2,028,544 B / fnv 0x1bf6c08f1ba38809（与 hf3 同长），新增 hf4 变体。
#
# 不覆盖 12.x 的 app/src/libs/arm64-v8a/libstarry_magic.so。
set -euo pipefail

ROOT="${ROOT:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
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
# baseline = 该变体的「未打补丁入口 DLL」。hf2/hf3 的 12.x v7.19 基线已随工作区瘦身丢失
# （git 里从来没有过），只能对 07_patched 里既有的补丁产物做增量补丁。
# baseline 写成 INCR:<path> = 该基线本身已是补丁产物（A 线 v1+δ 输出），必须走增量补丁。
#
# 已停用 base（1,960,960）：那份官方 DLL 早于 2026-09-23 二次热更，没有 AddItem 4 参形态，
# 物品倍数/转生无限两项物理装不进去；未知长度/哈希由 so 侧 PASSTHROUGH 兜底。
VARIANTS=(
  "hf2|2030080|$PDIR/targetfuntional_vc1637_hf2_patched.dll|targetfuntional_dll_vc1637_hf2.enc.bin|Games/WxsjVc1637Hf2Dll.a|$PDIR/targetfuntional_v719_hotfix2_patched.dll"
  "hf4|2028544|$PDIR/targetfuntional_vc1637_hf4_patched.dll|targetfuntional_dll_vc1637_hf4.enc.bin|Games/WxsjVc1637Hf4Dll.a|INCR:$PDIR/newdump_20261026/patched_new.dll"
  "hf3|2028544|$PDIR/targetfuntional_vc1637_hf3_patched.dll|targetfuntional_dll_vc1637_hf3.enc.bin|Games/WxsjVc1637Hf3Dll.a|$PDIR/targetfuntional_v719_hotfix3_patched.dll"
)

echo "[vc1637-so 1/5] IL patch（三变体：hf2 / hf4 / hf3）"
for row in "${VARIANTS[@]}"; do
  IFS='|' read -r name off dll bin lib base_in <<< "$row"
  if [[ "$base_in" == INCR:* ]]; then
    # 基线本身已是补丁产物（A 线 v1+δ）→ 只做 vc1637 增量（幂等，已带门时原样保留）
    base_src="${base_in#INCR:}"
    [[ -f "$base_src" ]] || { echo "ERROR: missing baseline $base_src"; exit 1; }
    echo "[vc1637-so]   ${name} 增量补丁（基线 ${base_src}）"
    INCR="$(mktemp -t wxsj_vc1637_incr)"
    dotnet run --project "$PATCHER" --configuration Release -- --variant "$name" --incremental "$base_src" "$INCR" >/dev/null
    cp -f "$INCR" "$dll"
    rm -f "$INCR"
  elif [[ -f "$base_in" ]]; then
    dotnet run --project "$PATCHER" --configuration Release -- --variant "$name" >/dev/null
  else
    # 基线缺失（hf2/hf3）：对既有补丁产物做增量补丁（幂等，已带门时原样保留）。
    [[ -f "$dll" ]] || { echo "ERROR: missing $dll"; exit 1; }
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
  echo "[vc1637-so]   $name official=$off patched=$PLEN margin=$((off - PLEN))"
done

echo "[vc1637-so 2/5] 载荷加密（P2-2）+ rebuild embedded static libraries"
# 明文补丁**不再进 jni**：先由共享生成器把三份明文加密成 <name>.enc.bin（密文 + 32B MAC），
# 再 objcopy 成静态库内嵌。运行时用 v3 票里的 kx 还原 K_build 才解得开，解不开落诱饵。
# 文件名必须与 04_scripts/_common/sg_games.py 的 wxsj_vc1637 登记一致。
python3 "$ROOT/04_scripts/_common/gen_payload_enc.py" --game wxsj_vc1637 >/tmp/wxsj_vc1637_payload.log 2>&1 || {
  echo "ERROR: 载荷加密失败（gen_payload_enc.py --game wxsj_vc1637）" >&2
  tail -20 /tmp/wxsj_vc1637_payload.log >&2; exit 1; }
grep -E "载荷 sha256|密文头已写" /tmp/wxsj_vc1637_payload.log || true
# 必须在 $JNI 下用相对文件名，符号名才是 _binary_<encbin>_start
for row in "${VARIANTS[@]}"; do
  IFS='|' read -r name off dll bin lib base_in <<< "$row"
  [[ -f "$JNI/$bin" ]] || { echo "ERROR: 缺密文段 $JNI/$bin"; exit 1; }
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

# 防封基线（无票也生效的那份）：同样只以密文进包
AB_BIN="targetfuntional_dll_vc1637_antiban.enc.bin"
AB_LIB="Games/WxsjVc1637AntibanDll.a"
[[ -f "$JNI/$AB_BIN" ]] || { echo "ERROR: 缺防封基线密文 $JNI/$AB_BIN"; exit 1; }
( cd "$JNI" && "$OBJCOPY" -I binary -O elf64-littleaarch64 -B aarch64 "$AB_BIN" "$AB_LIB" )
if ! "$NM" "$JNI/$AB_LIB" 2>/dev/null | grep -q "_binary_targetfuntional_dll_vc1637_antiban_enc_bin_start"; then
  echo "ERROR: 防封基线静态库符号名不对"; exit 1
fi
echo "[vc1637-so]   $AB_LIB data check ok（防封基线）"

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
# 必须有
REQ_STRS=(
  '[WxsjVC1637]'
  'Category_战斗'
  'Toggle_无敌'
  'Toggle_秒杀'
  'Toggle_免广告'
  'Toggle_物品不减'
  'Toggle_技能免费升级'
  'InputValue_1:广告奖励倍数'
  'InputValue_1:物品倍数'
  'Toggle_无限挑战秘境'
  'Toggle_皮肤全解锁'
  'Toggle_功能全解锁'
  'InputValue_1:经验倍数'
  'Toggle_商店广告无限'
  'Toggle_物品使用反增'
  'InputValue_1000:物品反增倍率'
  'Toggle_转生时间无限制'          # 防封已撤出菜单（2026-10-26），改由 so 侧基线无条件生效
  'antiban baseline unsealed'
  'starry-antiban-v1|'
  'ads_reward_x'
  'item_double_x'
  'exp_multi'
  'item_reverse_x'
  'zhuansheng_free'
  'shop_ads_unlimited'
  'targetfuntional'
)
for s in "${REQ_STRS[@]}"; do
  LC_ALL=C grep -aq -- "$s" "$OUT" || { echo "ERROR: so missing [$s]"; exit 1; }
done
# 必须没有（旧控件/旧硬编码键；残留即说明菜单或 DLL 没换过来）
BAN_STRS=(
  'Slider_经验倍率'
  'Toggle_物品翻倍'
  'Toggle_广告奖励翻倍'
  'Toggle_防封'      # 旧菜单项不许再出现（防封不在菜单里）
)
for s in "${BAN_STRS[@]}"; do
  if LC_ALL=C grep -aq -- "$s" "$OUT"; then echo "ERROR: so still carries old [$s]"; exit 1; fi
done
echo "[vc1637-so]   assertions ok: ${#REQ_STRS[@]} required / ${#BAN_STRS[@]} forbidden"

python3 - "$OUT" "$JNI" "${VARIANTS[@]}" <<'PY'
import sys
so = open(sys.argv[1], 'rb').read()
jni = sys.argv[2]
bad = 0
for row in sys.argv[3:]:
    name, off, dll, binname, lib = row.split('|')[:5]
    ct = open(jni + '/' + binname, 'rb').read()
    plain = open(dll, 'rb').read()
    pos = so.find(ct)
    if pos < 0:
        raise SystemExit("ERROR: 密文段 %s（%s）未内嵌进 so" % (name, binname))
    leak = so.find(plain)
    if leak >= 0:
        bad += 1
        print("LEAK %-4s 补丁明文出现在 so 偏移 %d（载荷没加密！）" % (name, leak))
    print("in-so OK: %-4s cipher=%d B offset=%d | plaintext_absent=%s"
          % (name, len(ct), pos, "no" if leak >= 0 else "yes"))
if bad:
    raise SystemExit("ERROR: %d 段补丁明文泄漏进 so（P2-2 失败）" % bad)
PY

echo "[vc1637-so 5/5] done"
ls -lh "$OUT"
shasum -a 256 "$OUT"
for row in "${VARIANTS[@]}"; do
  IFS='|' read -r name off dll bin lib base_in <<< "$row"
  shasum -a 256 "$dll"
done
