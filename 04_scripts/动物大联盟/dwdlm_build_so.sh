#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 私有 so 构建（环境候选两档 profile）
#
# 生成链（自 f9e8f36b^ 恢复，最小必要集）：
#   patch_dwdlm.js（本目录，构建输入）
#     → 内联 python 生成 DwdlmPatchJs.h（R"DWDLMJS(...)DWDLMJS" 原样嵌入）
#     → 经 STARRY_DWDLM_GEN 加 -I；DwdlmMod.cpp 仅在「非 DWDLM_ENV_ONLY」时 #include "DwdlmPatchJs.h"
#
# 环境阶段口径：
#   * profile=env       : DWDLM_ENV_ONLY + DWDLM_SHELL_ONLY（玩法 JS 补丁与 1.0.23 签名/证书回灌 hook 均不编译）
#   * profile=env_shell : DWDLM_ENV_ONLY（只剔玩法，保留 1.0.23 全壳链，用于对照）
#   * 两档都「生成」patch JS 头作为输入并纳入缓存键，但 env 档 so 内必须不含 starry_dwdlm_v1
#     （＝证明玩法补丁未被编译进去，而不是靠运行期开关兜）。
#   * 旧壳 offset（libsecret 0x20008/0x1D2AC/0x70318…、cocos 0x221B7A0）为 1.0.23 实测值，
#     1.0.25 未核 ⇒ 编译成功 ≠ 可装（见 10_docs/动物大联盟/环境阶段构建与组装_1.0.25_20261105.md）。
#
# 缓存键覆盖：源码树（mk 引用源 + 相关头 + 预编译库，**含 Games/DwdlmSecShell.cpp/.h**
#            与其 include 头 Includes/Logger.h）+ 生成 JS 头 + 全部环境宏 + ABI + NDK 路径与版本
#            + STARRY_BUILD_EPOCH（Application.mk 把它塞进 -D，变了必须重编）。
#            profile=env 显式向 ndk-build 传 STARRY_DWDLM_ENV_ONLY=1（mk 里 → -DDWDLM_ENV_ONLY=1），
#            该开关同时进入 KEY/MACROS，改宏必落新 obj 目录，绝不复用旧 .o。
# 隔离策略：obj 按 <profile>-<key8> 建独立目录，不删任何目录（无 rm -rf / 无通配删除）；
#          切换 SHELL_ONLY 等宏只会落到新目录，不会复用旧 .o。
#
# 用法：
#   bash 04_scripts/动物大联盟/dwdlm_build_so.sh                 # 默认 profile=env
#   DWDLM_BUILD_PROFILE=env_shell bash 04_scripts/动物大联盟/dwdlm_build_so.sh
#   DWDLM_BUILD_FORCE=1 ...                                      # 忽略缓存戳强制重编（仍不删旧 obj）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
PATCH_JS="$SCRIPT_DIR/patch_dwdlm.js"
PROFILE="${DWDLM_BUILD_PROFILE:-env}"
OBJ_ROOT="$STARRY_SRC/obj_dwdlm"
LIBS_ROOT="$STARRY_SRC/libs_dwdlm"
REPORT="$SCRIPT_DIR/so_build_${PROFILE}.json"

case "$PROFILE" in
  env)       M_ENV_ONLY=1; M_SHELL_ONLY=1; M_SHELL_STATE_ONLY=0; M_DATA_GUARD_ONLY=0; M_TRACE=0 ;;
  env_shell) M_ENV_ONLY=1; M_SHELL_ONLY=0; M_SHELL_STATE_ONLY=0; M_DATA_GUARD_ONLY=0; M_TRACE=0 ;;
  full)      echo "ERROR: profile=full（六项玩法）属第二阶段，本轮未授权；先过环境再开" >&2; exit 2 ;;
  *)         echo "ERROR: 未知 profile=${PROFILE}（支持 env / env_shell）" >&2; exit 2 ;;
esac

SO_OUT_DIR="$LIBS_ROOT/$PROFILE"
SO="$SO_OUT_DIR/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_dwdlm.mk" ]] || { echo "ERROR: missing Android_dwdlm.mk" >&2; exit 1; }
[[ -f "$JNI/Application.mk" ]] || { echo "ERROR: missing Application.mk" >&2; exit 1; }
[[ -f "$PATCH_JS" ]] || { echo "ERROR: missing ${PATCH_JS}（生成链输入）" >&2; exit 1; }
[[ -f "$JNI/Games/DwdlmMod.cpp" ]] || { echo "ERROR: missing Games/DwdlmMod.cpp" >&2; exit 1; }
[[ -f "$JNI/Games/NoAceStubs.cpp" ]] || { echo "ERROR: missing Games/NoAceStubs.cpp（mk 引用，缺则 ld 报 undefined）" >&2; exit 1; }

ABI="$(sed -n 's/^APP_ABI[[:space:]]*:=[[:space:]]*//p' "$JNI/Application.mk" | head -1)"
[[ -n "$ABI" ]] || { echo "ERROR: 无法从 Application.mk 解析 APP_ABI" >&2; exit 1; }
NDK_REV="$(sed -n 's/^Pkg.Revision[[:space:]]*=[[:space:]]*//p' "$NDK/source.properties" 2>/dev/null | head -1)"
BUILD_EPOCH="$(cd "$NIXIANG" && git log -1 --format=%ct 2>/dev/null | tr -cd '0-9' || true)"
[[ -n "$BUILD_EPOCH" ]] || BUILD_EPOCH="1790184000"

MACROS="ENV_ONLY=$M_ENV_ONLY SHELL_ONLY=$M_SHELL_ONLY SHELL_STATE_ONLY=$M_SHELL_STATE_ONLY DATA_GUARD_ONLY=$M_DATA_GUARD_ONLY TRACE=$M_TRACE"

# ---- 1. 源码与输入指纹（缓存键原料）----------------------------------------
SRC_LIST="$(mktemp)"
trap 'rm -f "$SRC_LIST"' EXIT
{
  printf '%s\n' "$JNI/MainModern.cpp" "$JNI/Android_dwdlm.mk" "$JNI/Application.mk" "$PATCH_JS" \
                "$JNI/Games/DwdlmMod.cpp" "$JNI/Games/DwdlmMod.h" "$JNI/Games/DwdlmEntry.cpp" \
                "$JNI/Games/DwdlmSecShell.cpp" "$JNI/Games/DwdlmSecShell.h" \
                "$JNI/Games/NoAceStubs.cpp" "$JNI/Games/ModEntry.h" \
                "$JNI/Dobby/$ABI/libdobby.a" "$JNI/Games/WxsjPatchedDll.a"
  find "$JNI/Includes" "$JNI/xDL" -type f \( -name '*.cpp' -o -name '*.c' -o -name '*.h' -o -name '*.hpp' \) 2>/dev/null
} | while IFS= read -r f; do
  if [[ -f "$f" ]]; then shasum -a 256 "$f"; else echo "MISSING $f"; fi
done > "$SRC_LIST"
if grep -q '^MISSING ' "$SRC_LIST"; then
  echo "ERROR: 缓存键输入缺失：" >&2; grep '^MISSING ' "$SRC_LIST" >&2; exit 1
fi

KEY="$( {
  cat "$SRC_LIST"
  printf 'PROFILE=${PROFILE}\nMACROS=${MACROS}\nABI=${ABI}\nNDK=${NDK}\nNDK_REV=${NDK_REV}\nEPOCH=${BUILD_EPOCH}\n'
} | shasum -a 256 | awk '{print $1}')"
KEY8="$(printf '%s' "$KEY" | cut -c1-8)"
SRC_SHA="$(python3 "$SCRIPT_DIR/source_fingerprint_dwdlm.py")"

OBJ="$OBJ_ROOT/${PROFILE}-${KEY8}"
GEN="$OBJ/generated"
STAMP="$OBJ/.build_stamp"

echo "[dwdlm_build_so] profile=$PROFILE abi=$ABI ndk_rev=${NDK_REV:-unknown} key=$KEY8"
echo "[dwdlm_build_so] 生效宏: $MACROS"
echo "[dwdlm_build_so] obj=$OBJ"
echo "[dwdlm_build_so] libs=$SO_OUT_DIR"

mkdir -p "$GEN" "$SO_OUT_DIR/arm64-v8a"

# ---- 2. 生成 DwdlmPatchJs.h（内联 python，自 f9e8f36b^ 恢复）----------------
python3 - "$PATCH_JS" "$GEN/DwdlmPatchJs.h" <<'PY'
import hashlib
import sys
from pathlib import Path

src, dst = Path(sys.argv[1]), Path(sys.argv[2])
text = src.read_text(encoding="utf-8")
if ')DWDLMJS"' in text:
    raise SystemExit("错误: patch JS 内含原生字符串终止符，无法原样嵌入")
new = 'static const char kPatchJs[] = R"DWDLMJS(' + text + ')DWDLMJS";\n'
old = dst.read_text(encoding="utf-8") if dst.exists() else None
changed = old != new
if changed:
    dst.write_text(new, encoding="utf-8")
js_sha = hashlib.sha256(text.encode("utf-8")).hexdigest()
hdr_sha = hashlib.sha256(new.encode("utf-8")).hexdigest()
print("  js=%s %dB sha256=%s..." % (src.name, len(text.encode("utf-8")), js_sha[:16]))
print("  header=%s %dB sha256=%s... %s" % (dst.name, len(new.encode("utf-8")), hdr_sha[:16], "(rewritten)" if changed else "(unchanged)"))
assert new.startswith('static const char kPatchJs[] = R"DWDLMJS('), 'header 前缀不符'
assert new.endswith(')DWDLMJS";\n'), 'header 后缀不符'
assert text in new, 'header 未原样包含 patch JS'
assert 'starry_dwdlm_v1' in new, 'patch JS 缺 starry_dwdlm_v1 标记'
print("  header 一致性自检 ok（前缀/后缀/原样包含/标记齐全）")
PY

# ---- 3. 缓存命判（命中不编、不删旧 obj）-------------------------------------
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
if [[ "${DWDLM_BUILD_FORCE:-0}" != "1" && -f "$STAMP" && -f "$SO" ]]; then
  if [[ "$(cat "$STAMP" 2>/dev/null)" == "$KEY" ]]; then
    echo "[dwdlm_build_so] 缓存命中（源码/JS/宏/ABI/NDK 全部一致）→ 不重编"
    echo "[dwdlm_build_so] so=$SO md5=$(md5 -q "$SO") size=$(stat -f '%z' "$SO")"
    echo "[dwdlm_build_so] header=$GEN/DwdlmPatchJs.h"
    # 缓存命中 = native 输入逐字节未变（KEY 由 native 源 + 宏 + ABI + NDK 算出），所以 so 字节可信；
    # 但 source_fingerprint 的口径含 java/dwdlm/*.java（交付面 8 文件）。只改 Java 时 so 键不变、
    # 报告不刷新 ⇒ verify_artifact 的 source 关会永久报"源在构建后变更"（假阳性，2026-11-05 实测）。
    # 因此命中时只把"当前源指纹"补记进报告，并显式标注 cache_hit 与上一版指纹，不伪称重编。
    if [[ -f "$REPORT" ]]; then
      python3 - "$REPORT" "$SRC_SHA" "${BUILD_EPOCH}" <<'PY'
import json, sys, time
from pathlib import Path
path, now_sha = sys.argv[1], sys.argv[2]
p = Path(path)
try:
    d = json.loads(p.read_text(encoding="utf-8"))
except Exception:
    raise SystemExit(0)
prev = d.get("source_fingerprint")
if prev == now_sha:
    raise SystemExit(0)
d["source_fingerprint_prev"] = prev
d["source_fingerprint"] = now_sha
d["cache_hit"] = True
d["fingerprint_refreshed_at"] = time.strftime("%Y-%m-%d %H:%M:%S")
d["fingerprint_refresh_note"] = ("缓存命中（native 键未变，so 字节未变）；"
                                 "仅因 java/dwdlm/*.java 变更而补记当前交付面源指纹，未重编 so")
p.write_text(json.dumps(d, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print("[dwdlm_build_so] 源指纹已补记（so 未重编）：%s → %s" % (str(prev)[:16], now_sha[:16]))
PY
    fi
    exit 0
  fi
  echo "[dwdlm_build_so] 缓存键变化 → 落到新 obj 目录 ${OBJ}（旧目录保留，不做清理）"
fi

# ---- 4. ndk-build（绝对路径 NDK_OUT / NDK_LIBS_OUT）-------------------------
NDK_ARGS=""
[[ "$M_ENV_ONLY" == "1" ]] && NDK_ARGS="$NDK_ARGS STARRY_DWDLM_ENV_ONLY=1"
[[ "$M_SHELL_ONLY" == "1" ]] && NDK_ARGS="$NDK_ARGS STARRY_SHELL_ONLY=1"
[[ "$M_TRACE" == "1" ]] && NDK_ARGS="$NDK_ARGS STARRY_TRACE=1"

T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
# shellcheck disable=SC2086
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_dwdlm.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$SO_OUT_DIR" \
  STARRY_DWDLM_GEN="$GEN" \
  $NDK_ARGS \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=${BUILD_RC}（编译失败；不删 obj，可直接复现）" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }
if [[ -n "$NEW_O" && "$SO_MD5_BEFORE" != "none" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次产生新 .o 但 so 未变化——旧 so 顶包嫌疑" >&2; exit 1
fi

# ---- 5. 产物自检（标签/菜单/玩法补丁是否真的没进去）------------------------
check_str() { local n; n="$(LC_ALL=C grep -ac -- "$1" "$SO" || true)"; [[ "${n:-0}" -ge 1 ]] || { echo "ERROR: so 不含 $2" >&2; exit 1; }; }
reject_str() { local n; n="$(LC_ALL=C grep -ac -- "$1" "$SO" || true)"; [[ "${n:-0}" -eq 0 ]] || { echo "ERROR: so 含 $2（本 profile 必须不含）" >&2; exit 1; }; }
check_str "dwdlm"                   "游戏标签 dwdlm"
check_str "Slider_攻击倍率:1:100:1" "菜单串 Slider_攻击倍率:1:100:1"
check_str "Toggle_上帝模式"          "菜单串 Toggle_上帝模式"
check_str "Toggle_无限技能"          "菜单串 Toggle_无限技能"
check_str "Toggle_自动战斗"          "菜单串 Toggle_自动战斗"
check_str "Slider_移速:1:5:1"        "菜单串 Slider_移速:1:5:1"
check_str "Toggle_免广告"            "菜单串 Toggle_免广告"
check_str "libcocos2djs.so"          "waitLib libcocos2djs.so"
if [[ "$M_ENV_ONLY" == "1" ]]; then
  check_str "DWDLM_ENV_ONLY"         "环境构建横幅"
  reject_str "starry_dwdlm_v1"       "玩法 JS 补丁标记（环境档必须剔除）"
  # 旧 1.0.23 壳链/日志通道必须编译期消失（不是"运行期没走到"）：
  #   check-report = PrepareSecureShell(0x20008) 的 hook 标签；exit-1..4 = PatchSecretExitNop 标签；
  #   secure text hooks = 旧 kUseSecretTextHooks 分支；ENV 下整段在 #else 里 ⇒ 字符串应 0。
  reject_str "check-report"          "旧 1.0.23 壳 hook 标签（ENV 必须编译期剔除）"
  reject_str "secure text hooks"     "旧 1.0.23 壳 hook 分支（ENV 必须编译期剔除）"
  reject_str "exit-1"                "旧 PatchSecretExitNop 标签 exit-1"
  reject_str "prepareSecureShell enter" "旧壳 hook 入口日志（ENV 必须编译期剔除）"
fi

# 新壳模块（Games/DwdlmSecShell.cpp）身份 + JNI 导出：不只看字符串，还要看 .dynsym。
#   * 模块身份串："[DwdlmSec] " 日志前缀 + "watchdog requested (state=RESOLVING)"
#   * 状态字来源证据：dlsym 名 "SecShell_IsDirty" / "SecShell_FailedMask"（序言反推的锚点）
#   * 导出：Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvStart（新工厂入口，须为
#     .dynsym 里的**定义**，Type=FUNC、Bind=GLOBAL）
#   * 兼容面：Java_com_starry_magic_dwdlm_DwdlmApplication_prepareSecureShell 仍导出（ENV 下体为空实现）
if [[ "$M_ENV_ONLY" == "1" ]]; then
  check_str "[DwdlmSec] "                   "新壳模块日志前缀"
  check_str "watchdog requested (state=RESOLVING)" "新壳模块启动日志"
  check_str "SecShell_IsDirty"              "新壳模块状态字锚点 SecShell_IsDirty"
  check_str "SecShell_FailedMask"           "新壳模块状态字锚点 SecShell_FailedMask"

  LLVM_NM=""
  for cand in "$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm" \
              "$NDK/toolchains/llvm/prebuilt/darwin-aarch64/bin/llvm-nm"; do
    if [[ -x "$cand" ]]; then LLVM_NM="$cand"; break; fi
  done
  [[ -n "$LLVM_NM" ]] || { echo "ERROR: 缺 llvm-nm（无法校验 JNI 导出符号）" >&2; exit 1; }
  DYNSYM="$("$LLVM_NM" -D --defined-only "$SO" 2>/dev/null || true)"
  [[ -n "$DYNSYM" ]] || { echo "ERROR: llvm-nm 读不到 .dynsym" >&2; exit 1; }
  check_sym() {
    printf '%s\n' "$DYNSYM" | LC_ALL=C grep -q " T $1$" \
      || { echo "ERROR: .dynsym 缺导出定义 $1（$2）" >&2; exit 1; }
  }
  check_sym "Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvStart" "新工厂 nativeEnvStart"
  check_sym "Java_com_starry_magic_dwdlm_DwdlmApplication_prepareSecureShell" "旧入口兼容符号"
  check_sym "JNI_OnLoad" "JNI 入口"
  # 旧壳链的 JNI 面必须**只剩**上面那一个兼容符号，不得再出现别的 prepare* / install* 导出
  if printf '%s\n' "$DYNSYM" | LC_ALL=C grep -q "Java_.*Dwdlm.*_prepareSecureShellImpl"; then
    echo "ERROR: .dynsym 出现旧壳链内部导出" >&2; exit 1
  fi
fi
if [[ "$M_SHELL_ONLY" == "0" ]]; then
  check_str "libsecret"              "壳链（env_shell 档保留 1.0.23 壳 hook）"
fi

SO_MD5="$(md5 -q "$SO")"
SO_SIZE="$(stat -f '%z' "$SO")"
printf '%s' "$KEY" > "$STAMP"

cat > "$REPORT" <<JSON
{
  "game": "动物大联盟",
  "version": "1.0.25",
  "profile": "$PROFILE",
  "macros": "$MACROS",
  "abi": "$ABI",
  "ndk": "$NDK",
  "ndk_revision": "${NDK_REV:-unknown}",
  "build_epoch": "$BUILD_EPOCH",
  "cache_key": "$KEY",
  "source_fingerprint": "$SRC_SHA",
  "source_fingerprint_scope": "source_fingerprint_dwdlm.py（8 个文件：Games/Dwdlm{Entry,Mod,SecShell}.* + Android_dwdlm.mk + java/dwdlm/*.java）",
  "obj_dir": "$OBJ",
  "generated_header": "$GEN/DwdlmPatchJs.h",
  "so_path": "$SO",
  "so_md5": "$SO_MD5",
  "so_size": $SO_SIZE,
  "play_patch_compiled_in": false,
  "env_only_macro": "STARRY_DWDLM_ENV_ONLY=$M_ENV_ONLY",
  "sec_shell_module": "Games/DwdlmSecShell.cpp（独立模块；JNI 导出 nativeEnvStart 已校验 .dynsym）",
  "rejected_strings": "starry_dwdlm_v1 / check-report / secure text hooks / exit-1 / prepareSecureShell enter（ENV 下均为 0）",
  "installable_verified": false,
  "built_at": "$(date '+%Y-%m-%d %H:%M:%S')",
  "note": "编译成功 != 可装：1.0.25 壳 offset 未核，库内 1.0.23 偏移不可外推"
}
JSON

echo "[dwdlm_build_so] done → $SO"
echo "[dwdlm_build_so] md5=$SO_MD5 size=$SO_SIZE"
echo "[dwdlm_build_so] report=$REPORT"
