#!/usr/bin/env bash
# framework_entry_audit.sh — 出包前「入口点 + 渠道接入」静态审计（复盘 20261010 §4 P0-1）
#
# 用法:
#   bash 04_scripts/_common/framework_entry_audit.sh --game <游戏> --pkg <包名> [--apk <apk>] [--macro STARRY_GAME_XXX]
#
# 为什么需要它（2026-10-10 追迹迷城 1.0.20261009 会话复盘 §4 P0-1）：
#   下面这些坑都不会让构建报错，只会让装机后菜单不出现 / 登录不走指定渠道，且几乎零日志：
#     a) TapLoginForceWeb.PKG_WHITELIST 漏配包名 ⇒ 5 处 `if (!PKG_WHITELIST.contains(...)) return;`
#        静默 return（实测教训：漏配时 TapTap 被强制走网页登录，全程无日志）。
#     b) 运行时签名伪装残留：SignSpoof 已于 2026-10-28 整体删除 ⇒ 成品包不得再含该类
#        （含则属历史产物：重打 dex 即消失；manifest 若仍挂已删除的 SignSpoofAppComponentFactory，
#          装上必 ClassNotFoundException —— [G] 判 ✗）。
#     c) GameConfig.h 漏映射 ⇒ STARRY_AUTH_GAME_ID 空/串号 ⇒ 授权校验失败。
#     d) Android.mk 激活分支还停在上一个游戏 ⇒ 源码全对但编出别人的 so。
#     e) 入口形态选错：1.0.20261009 实测 <application android:name> 平台不采用
#        （aapt2 复读合法、StarryApp 从未实例化、且无 "Unable to instantiate application" 崩溃）
#        ⇒ 推荐 ContentProvider 入口（见 skill/starry-mod/SKILL.md「入口点决策表」）。
#     f) 交付物层面：APK 的 dex 里根本没有入口类 / AXML 里没有 provider ⇒ 平台启动即异常。
#
# 退出码: 0=全部 ✓（可含 ⚠ WARN）；1=存在 ✗（禁止出包）；2=用法错误
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=env.sh
[ -f "$SCRIPT_DIR/env.sh" ] && . "$SCRIPT_DIR/env.sh"
root="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

GAME="" PKG="" APK="" MACRO_ARG="" ROOT_ARG="" SELFCHECK=0
fail=0 warn=0
macro=""
SELF="$SCRIPT_DIR/$(basename "$0")"

usage() {
  cat <<'EOF'
用法:
  bash 04_scripts/_common/framework_entry_audit.sh --game <游戏> --pkg <包名> [--apk <apk>] [--macro STARRY_GAME_XXX]
  bash 04_scripts/_common/framework_entry_audit.sh --selfcheck

参数:
  --game   游戏中文名（与 04_scripts/<游戏>/、Android.mk 注释里的写法一致，例：追迹迷城）
  --pkg    目标渠道包名（例：com.TeaMelo.DungeonChasers）
  --apk    可选。给了才做交付物断言（dex/Manifest 里的入口类与服务）
  --macro  可选。直接指定 STARRY_GAME_* 宏名（推断失败时用）
  --root   可选。覆盖仓库根（自检夹具用）
  --selfcheck  本检查器自检：正控（该抓的一律抓出）+ 负控（干净夹具零 ✗），全绿才说明它的「通过」可信

退出码: 0=全部 ✓（可含 ⚠ WARN）；1=存在 ✗（禁止出包）；2=用法错误
EOF
}

while [ $# -gt 0 ]; do
  case "$1" in
    --game)  [ $# -ge 2 ] || { echo "--game 缺参数" >&2; exit 2; }; GAME="$2"; shift 2 ;;
    --pkg)   [ $# -ge 2 ] || { echo "--pkg 缺参数" >&2; exit 2; }; PKG="$2"; shift 2 ;;
    --apk)   [ $# -ge 2 ] || { echo "--apk 缺参数" >&2; exit 2; }; APK="$2"; shift 2 ;;
    --macro) [ $# -ge 2 ] || { echo "--macro 缺参数" >&2; exit 2; }; MACRO_ARG="$2"; shift 2 ;;
    --root)  [ $# -ge 2 ] || { echo "--root 缺参数" >&2; exit 2; }; ROOT_ARG="$2"; shift 2 ;;
    --selfcheck) SELFCHECK=1; shift ;;
    -h|--help) usage; exit 0 ;;
    *) echo "未知/多余参数: $1" >&2; usage; exit 2 ;;
  esac
done
if [ -n "$ROOT_ARG" ]; then root="$ROOT_ARG"; fi
if [ "$SELFCHECK" != 1 ]; then
  [ -n "$GAME" ] || { echo "缺 --game" >&2; usage; exit 2; }
  [ -n "$PKG" ]  || { echo "缺 --pkg" >&2; usage; exit 2; }
fi

JAVA_ROOT="$root/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic"
JNI="$root/mod菜单/starry-mod-menu/app/src/main/jni"
TAP="$JAVA_ROOT/sign/TapLoginForceWeb.java"
# 2026-10-28：sign/SignSpoof.java 已整体删除（用户定案：TapTap 验签/登录一律改包法）。
# 下面 TAP_MODE 用「成品包 manifest 是否仍挂已删除的 SignSpoofAppComponentFactory」判老包。
CFG="$JNI/GameConfig.h"
MK="$JNI/Android.mk"

ok()    { echo "  ✓ $*"; }
bad()   { echo "  ✗ $*"; fail=$((fail + 1)); }
note()  { echo "  ⚠ $*"; warn=$((warn + 1)); }
sub()   { echo "      · $*"; }
hdr()   { echo ""; echo "---- $*"; }

# 自检（TOOLBOOK 二-b #45：检查器自身必须带正控 + 负控）
selfcheck() {
  sc_bad=0
  sc_tmp="$(mktemp -d "${TMPDIR:-/tmp}/fea_selfcheck.XXXXXX" 2>/dev/null)"
  if [ -z "$sc_tmp" ] || [ ! -d "$sc_tmp" ]; then
    echo "✗ 自检：无法创建临时夹具目录" >&2
    return 1
  fi
  sc_j="$sc_tmp/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic"
  sc_n="$sc_tmp/mod菜单/starry-mod-menu/app/src/main/jni"
  if ! mkdir -p "$sc_j/sign" "$sc_n/Games"; then
    echo "✗ 自检：夹具目录创建失败" >&2
    return 1
  fi
  cat > "$sc_j/sign/TapLoginForceWeb.java" <<'EOF'
public class TapLoginForceWeb {
    private static final java.util.Set<String> PKG_WHITELIST = new java.util.HashSet<>(java.util.Arrays.asList(
            "com.other.game",
            "com.fixture.good"));
    void hook(android.app.Activity activity) {
        if (!PKG_WHITELIST.contains(activity.getPackageName())) return;
    }
}
EOF
  cat > "$sc_j/sign/StarryAppComponentFactory.java" <<'EOF'
package com.starry.magic.sign;
public class StarryAppComponentFactory extends android.app.AppComponentFactory {
}
EOF
  cat > "$sc_j/StarryInitProvider.java" <<'EOF'
package com.starry.magic;
public class StarryInitProvider extends android.content.ContentProvider {
}
EOF
  cat > "$sc_j/StarryApp.java" <<'EOF'
package com.starry.magic;
public class StarryApp extends android.app.Application {
}
EOF
  cat > "$sc_n/GameConfig.h" <<'EOF'
#pragma once
#ifdef STARRY_AUTH_GAME_ID
#error "fixture: 不要在 Android.mk 写 STARRY_AUTH_GAME_ID"
#endif
#if defined(STARRY_GAME_ZJMC)
#define STARRY_AUTH_GAME_ID "zjmc"
#elif defined(STARRY_GAME_WDYZ)
#define STARRY_AUTH_GAME_ID "wdyz"
#else
#error "fixture: 须启用且仅启用一个 STARRY_GAME_*"
#endif
#ifdef ENABLE_UNITY_HOOK
namespace GameUnity {
#if defined(STARRY_GAME_ZJMC)
    constexpr const char* PACKAGE    = "com.fixture.good";
    constexpr const char* GAME_NAME  = "追迹迷城";
#elif defined(STARRY_GAME_WDYZ)
    constexpr const char* PACKAGE    = "com.other.game";
    constexpr const char* GAME_NAME  = "无尽探险队";
#endif
}
#endif
EOF
  cat > "$sc_n/Android.mk" <<'EOF'
LOCAL_PATH := $(call my-dir)
# 追迹迷城（selfcheck fixture）
LOCAL_CFLAGS := -w -s
LOCAL_CFLAGS += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ZJMC
LOCAL_SRC_FILES := Main.cpp Games/ZjmcMod.cpp
EOF
  echo '// 追迹迷城（selfcheck fixture）' > "$sc_n/Games/ZjmcMod.cpp"
  echo '// 无尽探险队（selfcheck fixture）' > "$sc_n/Games/WdyzMod.cpp"

  echo "== framework_entry_audit.sh --selfcheck =="
  # 负控：干净夹具 ⇒ rc=0 且零 ✗（该放行的放行）
  sc_out="$(bash "$SELF" --root "$sc_tmp" --game 追迹迷城 --pkg com.fixture.good 2>&1)"; sc_rc=$?
  sc_cnt="$(printf '%s\n' "$sc_out" | grep -c '✗')"
  if [ "$sc_rc" = 0 ] && [ "$sc_cnt" = 0 ]; then
    echo "  ✓ 负控：干净夹具 rc=0、✗ 0 项"
  else
    echo "  ✗ 负控失败：rc=${sc_rc}、✗ ${sc_cnt} 项（期望 rc=0 且 0 项）"
    printf '%s\n' "$sc_out" | grep '✗' | sed 's/^/      /'
    sc_bad=$((sc_bad + 1))
  fi
  # 正控①：包名不在 TapLoginForceWeb 白名单 ⇒ 必须抓出 ≥1 项 ✗
  # （2026-10-28 起 [B] 不再查 SignSpoof 证书表——该类已删除，正控改为只压 [A]）
  sc_out="$(bash "$SELF" --root "$sc_tmp" --game 追迹迷城 --pkg com.fixture.absent 2>&1)"; sc_rc=$?
  sc_cnt="$(printf '%s\n' "$sc_out" | grep -c '✗')"
  if [ "$sc_rc" = 1 ] && [ "$sc_cnt" -ge 1 ]; then
    echo "  ✓ 正控①：漏配包名被判 ✗ ${sc_cnt} 项"
  else
    echo "  ✗ 正控①失败：rc=${sc_rc}、✗ ${sc_cnt} 项（期望 rc=1 且 ≥1）"
    printf '%s\n' "$sc_out" | grep '✗' | sed 's/^/      /'
    sc_bad=$((sc_bad + 1))
  fi
  # 正控②：游戏名与激活分支错配 ⇒ 必须出现「不一致」判据
  sc_out="$(bash "$SELF" --root "$sc_tmp" --game 无尽探险队 --pkg com.fixture.good 2>&1)"; sc_rc=$?
  if [ "$sc_rc" = 1 ] && printf '%s\n' "$sc_out" | grep -q '不一致'; then
    echo "  ✓ 正控②：游戏/激活分支错配被判 ✗"
  else
    echo "  ✗ 正控②失败：rc=${sc_rc}，未出现「不一致」判据"
    printf '%s\n' "$sc_out" | grep '✗' | sed 's/^/      /'
    sc_bad=$((sc_bad + 1))
  fi
  case "$sc_tmp" in
    "${TMPDIR:-/tmp}"/*|/tmp/*) rm -rf "$sc_tmp" ;;
    *) echo "  ⚠ 夹具未清理（路径不在临时目录）：${sc_tmp}" ;;
  esac
  if [ "$sc_bad" = 0 ]; then
    echo "== 自检结论：✓ 全绿（负控 1 + 正控 2）=="
    return 0
  fi
  echo "== 自检结论：✗ ${sc_bad} 个控制点失败 ⇒ 该检查器的「通过」不可信 =="
  return 1
}

infer_macro() {
  if [ -n "$MACRO_ARG" ]; then echo "${MACRO_ARG#STARRY_GAME_}"; return 0; fi
  local m="" f b t
  # 1) Android.mk 里与游戏名同现（含注释行）的 -DSTARRY_GAME_*
  if [ -f "$MK" ] && grep -qF "$GAME" "$MK" 2>/dev/null; then
    m="$(grep -F "$GAME" "$MK" 2>/dev/null | sed -n 's/.*-DSTARRY_GAME_\([A-Z0-9_]*\).*/\1/p' | head -1)"
  fi
  # 2) Games/<Tag>Mod.cpp / <Tag>Entry.cpp 内容含该游戏名 ⇒ 文件名转宏名
  if [ -z "$m" ] && [ -d "$JNI/Games" ]; then
    for f in "$JNI"/Games/*; do
      [ -f "$f" ] || continue
      case "$f" in *.cpp|*.h) ;; *) continue ;; esac
      if grep -qF "$GAME" "$f" 2>/dev/null; then
        b="$(basename "$f")"
        b="${b%.cpp}"; b="${b%.h}"; b="${b%Mod}"; b="${b%Entry}"
        t="$(echo "$b" | tr 'a-z' 'A-Z' | tr -cd 'A-Z0-9_')"
        [ -n "$t" ] && m="$t"
        break
      fi
    done
  fi
  # 3) 04_scripts/<游戏>/ 下脚本文件名里的英文 tag（如 patch_zjmc_binary_manifest_v3.py / zjmc_manifest.sh）
  if [ -z "$m" ] && [ -d "$root/04_scripts/$GAME" ] && [ -f "$MK" ]; then
    for t in "$(ls "$root/04_scripts/$GAME" 2>/dev/null | sed -n 's/.*[^a-zA-Z0-9]\([a-z][a-z0-9_]\{2,7\}\)[^a-zA-Z0-9].*/\1/p' | tr 'a-z' 'A-Z' | sort -u)"; do
      if grep -q "STARRY_GAME_$t" "$MK" 2>/dev/null; then m="$t"; break; fi
    done
  fi
  echo "$m"
}

# 自检分支放这里：selfcheck() 必须先定义（bash 顺序执行）
if [ "$SELFCHECK" = 1 ]; then selfcheck; exit $?; fi

echo "================================================================"
echo " 出包前审计：入口点 + 渠道接入（复盘 20261010 §4 P0-1）"
echo "   游戏 = ${GAME}"
echo "   包名 = ${PKG}"
echo "   APK  = ${APK:-（未提供 ⇒ 跳过 f 交付物断言）}"
echo "================================================================"

# ---------------- 现行 TapTap 方案判定（2026-10-28）----------------
# 用户 2026-10-28 定案：TapTap 验签/网页登录一律「改包法」，运行时签名伪装（SignSpoof）已整体删除。
# 老包判据：manifest 仍挂已删除的 com.starry.magic.sign.SignSpoofAppComponentFactory。
# legacy（老包）时 [A] 照旧查 PKG_WHITELIST；new 时 [A] 跳过（TapLoginForceWeb 不参与执行）。
TAP_MODE="unknown"
if [ -n "$APK" ] && [ -f "$APK" ]; then
  _PY="$(command -v python3 || command -v python || true)"
  if [ -n "$_PY" ] && PYTHONUTF8=1 PYTHONIOENCODING=utf-8 "$_PY" - "$APK" 2>/dev/null <<'PY'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as z:
    man = z.read('AndroidManifest.xml')
sys.exit(0 if b'SignSpoofAppComponentFactory' in man else 1)
PY
  then TAP_MODE="legacy"; else TAP_MODE="new"; fi
fi

# ---------------- A. TapTap 强制网页登录白名单 ----------------
hdr "[A] TapTap 强制网页登录白名单 PKG_WHITELIST（历史机制）"
if [ "$TAP_MODE" = "new" ]; then
  ok "跳过：成品包已是「改包法」形态（TapLoginForceWeb 不参与执行，白名单无需登记）"
elif [ ! -f "$TAP" ]; then
  bad "缺文件 mod菜单/.../sign/TapLoginForceWeb.java"
else
  wl_start="$(grep -n 'PKG_WHITELIST' "$TAP" | head -1 | cut -d: -f1)"
  wl_end=""
  [ -n "$wl_start" ] && wl_end="$(awk -v s="$wl_start" 'NR>=s && /\)\);/ {print NR; exit}' "$TAP")"
  hit="$(grep -nF "\"$PKG\"" "$TAP" | head -1 | cut -d: -f1)"
  guards="$(grep -c 'PKG_WHITELIST.contains' "$TAP")"
  if [ -z "$wl_start" ]; then
    bad "TapLoginForceWeb.java 里找不到 PKG_WHITELIST 声明（文件被改写？）"
  elif [ -z "$hit" ]; then
    bad "PKG_WHITELIST 未含包名 \"${PKG}\"（列表 :${wl_start}-${wl_end:-?}）⇒ ${guards} 处 contains 判据全部静默 return，零日志"
  elif [ -n "$wl_end" ] && [ "$hit" -gt "$wl_end" ]; then
    bad "包名 \"${PKG}\" 出现在 :${hit}，落在 PKG_WHITELIST 列表（:${wl_start}-${wl_end}）之外 ⇒ 不在白名单"
  else
    ok "PKG_WHITELIST 含包名 \"${PKG}\"（:${hit}，列表 :${wl_start}-${wl_end}，contains 判据 ${guards} 处）"
  fi
fi

# ---------------- B. 运行时签名伪装必须不存在（2026-10-28 起 SignSpoof 已整体删除）----------------
hdr "[B] 运行时签名伪装（SignSpoof）必须不存在"
if [ -z "$APK" ] || [ ! -f "$APK" ]; then
  note "未传 --apk ⇒ 跳过（本关看成品包 dex）"
elif [ -n "${_PY:-}" ] && PYTHONUTF8=1 PYTHONIOENCODING=utf-8 "$_PY" - "$APK" 2>/dev/null <<'PY'
import sys, zipfile
NEEDLE = b'Lcom/starry/magic/sign/SignSpoof;'
with zipfile.ZipFile(sys.argv[1]) as z:
    hit = [n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex') and NEEDLE in z.read(n)]
print('   ' + ('命中: ' + ', '.join(hit) if hit else ''))
sys.exit(0 if hit else 1)
PY
then
  note "包内仍有 Lcom/starry/magic/sign/SignSpoof;（历史产物）：源码已于 2026-10-28 删除，重打 dex 即消失；不得再新增依赖"
else
  ok "包内无 Lcom/starry/magic/sign/SignSpoof;（签名伪装类已彻底不存在）"
fi

# ---------------- C. GameConfig.h 鉴权 / 包名 / 游戏名 ----------------
hdr "[C] GameConfig.h 鉴权 game_id / PACKAGE / GAME_NAME 映射"
if [ ! -f "$CFG" ]; then
  bad "缺文件 mod菜单/.../jni/GameConfig.h"
else
  macro="$(infer_macro)"
  if [ -z "$macro" ]; then
    note "推断不出 STARRY_GAME_* 宏名（Android.mk / Games/*.cpp / 04_scripts/${GAME}/ 均无线索）⇒ 跳过分支断言；可显式加 --macro STARRY_GAME_XXX"
  else
    sub "推断宏名：STARRY_GAME_${macro}"
    al="$(grep -n "defined(STARRY_GAME_${macro})" "$CFG" | head -1)"
    aln="${al%%:*}"
    if [ -z "$aln" ]; then
      bad "GameConfig.h 无 defined(STARRY_GAME_${macro}) 分支 ⇒ STARRY_AUTH_GAME_ID 未定义，:24 的 #error 会拦住构建"
    else
      auth="$(sed -n "${aln},$((aln + 6))p" "$CFG" | grep -m1 'STARRY_AUTH_GAME_ID' | sed 's/^[[:space:]]*//')"
      if [ -n "$auth" ]; then
        ok "鉴权映射：:${aln} $(echo "$auth" | sed 's/[[:space:]]\{1,\}/ /g')"
      else
        bad "STARRY_GAME_${macro} 分支（:${aln}）后 6 行内无 STARRY_AUTH_GAME_ID define ⇒ 鉴权 game_id 缺失"
      fi
    fi
    pln="$(grep -n 'const char\* PACKAGE' "$CFG" | grep -F "\"$PKG\"" | head -1 | cut -d: -f1)"
    gln="$(grep -n 'const char\* GAME_NAME' "$CFG" | grep -F "\"$GAME\"" | head -1 | cut -d: -f1)"
    pkgcnt="$(grep -c 'const char\* PACKAGE' "$CFG")"
    if [ -n "$pln" ]; then
      gline="$(awk -v ln="$pln" 'NR<=ln && /defined\(STARRY_GAME_[A-Z0-9_]+\)/ {m=$0} END {print m}' "$CFG")"
      gm="$(echo "$gline" | sed -n 's/.*STARRY_GAME_\([A-Z0-9_]*\).*/\1/p')"
      if [ "$gm" = "$macro" ]; then
        ok "PACKAGE=\"${PKG}\"（:${pln}，隶属 STARRY_GAME_${gm} 分支）"
      else
        bad "PACKAGE=\"${PKG}\"（:${pln}）挂在 STARRY_GAME_${gm:-?} 分支，与推断的 STARRY_GAME_${macro} 不符 ⇒ 串号"
      fi
    else
      note "GameConfig.h 内无 PACKAGE=\"${PKG}\"（全表 ${pkgcnt} 条）⇒ 该游戏可能是 modern 注册表，人工确认包名映射位置"
    fi
    if [ -n "$gln" ]; then
      ok "GAME_NAME=\"${GAME}\"（:${gln}）"
    else
      note "GameConfig.h 内无 GAME_NAME=\"${GAME}\" ⇒ 同上，可能走 modern 注册表"
    fi
  fi
fi

# ---------------- D. Android.mk 激活分支 ----------------
hdr "[D] Android.mk 当前激活的 STARRY_GAME_* 分支"
if [ ! -f "$MK" ]; then
  bad "缺文件 mod菜单/.../jni/Android.mk"
else
  act=""
  tmpmk="/tmp/fea_mk_$$"
  grep -nE '^[[:space:]]*LOCAL_CFLAGS.*-DSTARRY_GAME_' "$MK" 2>/dev/null | grep -vE '^[0-9]+:[[:space:]]*#' > "$tmpmk"
  while read -r l; do
    [ -n "$l" ] || continue
    ln="${l%%:*}"
    g="$((ln > 3 ? ln - 3 : 1))"
    guard="$(sed -n "${g},$((ln - 1))p" "$MK" | grep -E '^[[:space:]]*(ifdef|ifeq|ifneq)' | head -1 | sed 's/^[[:space:]]*//')"
    mac="$(echo "$l" | sed -n 's/.*-DSTARRY_GAME_\([A-Z0-9_]*\).*/\1/p')"
    if [ -n "$guard" ]; then
      sub ":${ln} -DSTARRY_GAME_${mac}（条件分支 ${guard}，未设该环境变量时不生效）"
    else
      act="${act} ${mac}"
      sub ":${ln} 激活 -DSTARRY_GAME_${mac}"
    fi
  done < "$tmpmk"
  rm -f "$tmpmk"
  hist="$(grep -cE '^[[:space:]]*#.*-DSTARRY_GAME_' "$MK")"
  if [ -z "$act" ]; then
    bad "Android.mk 无未注释的 LOCAL_CFLAGS -DSTARRY_GAME_* ⇒ 没选游戏（或只剩被注释的历史分支 ${hist} 行）"
  elif [ -z "$macro" ]; then
    note "已激活：${act}；宏名推断失败，无法与 --game ${GAME} 比对"
  else
    case " ${act} " in
      *" ${macro} "*) ok "激活分支含 STARRY_GAME_${macro}，与 --game ${GAME} 一致" ;;
      *) bad "激活分支 =${act} ，与 --game ${GAME}（推断 STARRY_GAME_${macro}）不一致 ⇒ 会编出别的游戏的 so" ;;
    esac
  fi
  sub "注释掉的历史分支 ${hist} 行（切游戏残留，仅供对照）"
fi

# ---------------- E. 入口形态清单 ----------------
hdr "[E] 入口形态清单（推荐 = ContentProvider）"
if [ ! -d "$JAVA_ROOT" ]; then
  bad "缺目录 mod菜单/.../java/com/starry/magic/"
else
  ent="$(grep -rl --include='*.java' -E 'class [A-Za-z0-9_]+ extends ([A-Za-z0-9_.]+\.)?(Application|ContentProvider|AppComponentFactory)' "$JAVA_ROOT" 2>/dev/null | sort)"
  if [ -z "$ent" ]; then
    bad "magic/ 下未找到任何 Application/ContentProvider/AppComponentFactory 入口类"
  else
    for f in $ent; do
      cls="$(grep -m1 -E 'class [A-Za-z0-9_]+ extends ([A-Za-z0-9_.]+\.)?(Application|ContentProvider|AppComponentFactory)' "$f" | sed 's/^[[:space:]]*//')"
      kind="$(echo "$cls" | sed -n 's/.*extends \([A-Za-z0-9_.]*\).*/\1/p' | awk -F. '{print $NF}')"
      case "$kind" in
        Application)         tag="Application 入口（<application android:name>，已被 1.0.20261009 实测否决）" ;;
        ContentProvider)     tag="ContentProvider 入口（<provider>，推荐）" ;;
        AppComponentFactory) tag="appComponentFactory 入口（等长类名替换 CoreComponentFactory）" ;;
        *)                   tag="$kind" ;;
      esac
      sub "${f#$JAVA_ROOT/} — ${tag}"
    done
  fi
  if [ -f "$JAVA_ROOT/StarryInitProvider.java" ]; then
    ok "推荐入口类可用 StarryInitProvider.java（ContentProvider.onCreate 早于 Activity.onCreate，且晚于 ACE initialize 完成＝安全窗口）"
  else
    note "缺 StarryInitProvider.java ⇒ 只能退回 Application/appComponentFactory 入口（ACE/TSS 类壳有毒丸挂起风险）"
  fi
  dlist=""
  for d in ysxz conch h5box dwdlm ltzj; do
    [ -d "$JAVA_ROOT/$d" ] && dlist="${dlist} ${d}/"
  done
  [ -n "$dlist" ] && sub "壳/宿主 Activity 注入分支：${dlist}"
  sub "推荐入口＝ContentProvider：<application android:name> 在部分壳不被平台采用（StarryApp 从未实例化），替换 Application 还会触发 AceApplication.initialize native 信号量毒丸挂起；判据见 skill/starry-mod/SKILL.md「入口点决策表」"
fi

# ---------------- F. 交付物断言（--apk） ----------------
hdr "[F] 交付物断言（dex 落地 / AXML 入口）"
if [ -z "$APK" ]; then
  sub "未提供 --apk ⇒ 跳过；出包后请务必带 --apk 复跑"
elif [ ! -f "$APK" ]; then
  bad "APK 不存在：${APK}"
else
  dexes="$(unzip -Z1 -- "$APK" 'classes*.dex' 2>/dev/null | tr -d '\r')"
  if [ -z "$dexes" ]; then
    bad "APK 内未找到 classes*.dex（不是 APK 或 APK 损坏）"
  else
    fp=0; fs=0; pdex=""; sdex=""
    for d in $dexes; do
      if [ "$fp" = 0 ] || [ "$fs" = 0 ]; then
        hits="$(unzip -p -- "$APK" "$d" 2>/dev/null | LC_ALL=C grep -ao -e 'Lcom/starry/magic/StarryInitProvider;' -e 'Lcom/starry/magic/menu/MenuService;' | sort -u)"
        if [ "$fp" = 0 ]; then
          case "$hits" in *'Lcom/starry/magic/StarryInitProvider;'*) fp=1; pdex="$d" ;; esac
        fi
        if [ "$fs" = 0 ]; then
          case "$hits" in *'Lcom/starry/magic/menu/MenuService;'*) fs=1; sdex="$d" ;; esac
        fi
      fi
    done
    if [ "$fp" = 1 ]; then
      ok "dex 含 Lcom/starry/magic/StarryInitProvider;（${pdex}）"
    else
      bad "dex 未含 Lcom/starry/magic/StarryInitProvider; ⇒ Manifest 的 provider 指向不存在的类，启动即 ClassNotFoundException"
    fi
    if [ "$fs" = 1 ]; then
      ok "dex 含 Lcom/starry/magic/menu/MenuService;（${sdex}）"
    else
      bad "dex 未含 Lcom/starry/magic/menu/MenuService; ⇒ 菜单前台服务起不来（装机后无菜单）"
    fi
  fi
  AAPT2=""
  for c in "$(command -v aapt2 2>/dev/null || true)" "${NIXIANG_BUILD_TOOLS:-}/aapt2" "$root/01_tools/aapt2"; do
    if [ -n "$c" ] && [ -x "$c" ]; then AAPT2="$c"; break; fi
  done
  if [ -z "$AAPT2" ]; then
    note "未找到 aapt2（01_tools / PATH / NIXIANG_BUILD_TOOLS 均无）⇒ 跳过 AXML 断言"
  else
    man="$("$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" 2>/dev/null)"
    if [ -z "$man" ]; then
      bad "aapt2 dump xmltree 失败：${APK}"
    else
      # ⚠ 禁写成 `printf '%s\n' "$man" | grep -qF …`：grep -q 命中即退出 ⇒ printf 吃 SIGPIPE(141)，
      #   与 :20 的 pipefail 相乘会把「命中」判成失败（2026-10-16 终末地 1.6.4 实测假阴性：
      #   同一 dump 里**靠前**的 StarryInitProvider 判 MISS、**靠后**的 MenuService 判 HIT）。
      #   here-string 不建管道、无 SIGPIPE，匹配语义仍是「$man 含字面量 ="$cls"」。
      for cls in com.starry.magic.StarryInitProvider com.starry.magic.menu.MenuService; do
        if grep -qF "=\"${cls}\"" <<<"$man"; then
          ok "AXML 含 ${cls}（aapt2：${AAPT2}）"
        else
          bad "AXML 未含 ${cls} ⇒ 入口/服务没写进 Manifest（dex 里有类也白搭）"
        fi
      done
    fi
  fi
fi

# ---------------- G. TapTap 现行方案关（改包法，2026-10-28） ----------------
hdr "[G] TapTap 现行方案关（改包法，2026-10-28）"
if [ -z "$APK" ] || [ ! -f "$APK" ]; then
  note "未传 --apk ⇒ 跳过（本关需成品包）"
elif [ "$TAP_MODE" = "legacy" ]; then
  bad "成品包 manifest 仍挂 SignSpoofAppComponentFactory ⇒ 该类已于 2026-10-28 删除，装上会启动即 ClassNotFoundException"
  echo "      改法：manifest 恢复官方 androidx.core.app.CoreComponentFactory，入口只留 StarryInitProvider（或 StarryAppComponentFactory，若需早于 Application），"
  echo "            验签/登录改用改包法锚点：InitializerServiceImpl.checkInitialize/checkLocalInitialize 恒 true、"
  echo "            GatekeeperRepository.isGatekeeperLoadedSuccessfully 恒 true + showErrorToast no-op、登录入口恒 Web。"
  echo "            配方：10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md"
else
  ok "manifest 未挂已删除的 SignSpoofAppComponentFactory"
  if [ -n "${_PY:-}" ] && PYTHONUTF8=1 PYTHONIOENCODING=utf-8 "$_PY" - "$APK" 2>/dev/null <<'PY'
import sys, zipfile
GATE = b'com/taptap/sdk/initializer/repository/GatekeeperRepository'
with zipfile.ZipFile(sys.argv[1]) as z:
    hit = any(GATE in z.read(n) for n in z.namelist()
              if n.startswith('classes') and n.endswith('.dex'))
sys.exit(0 if hit else 1)
PY
  then
    note "包内含 TapSDK 门闩类 GatekeeperRepository ⇒ 还需用 pre_install_gate.sh --apk <成品> --base-apk <底包> 复核「查12 已改包」（本脚本无底包参数，不做字节比对）"
  else
    ok "包内无 TapSDK 门闩类 ⇒ 非 TapTap/TapSDK 包，改包法不适用"
  fi
fi

echo ""
echo "================================================================"
echo "建议：bash 04_scripts/_common/framework_entry_audit.sh --game <游戏> --pkg <包名> --apk <apk>"
echo "入口取舍与已否决判据见 skill/starry-mod/SKILL.md「入口点决策表」；TapTap 逐项见「TapTap 接入检查表」"
if [ "$fail" -gt 0 ]; then
  echo "结论：✗ FAIL ${fail} 项 / ⚠ WARN ${warn} 项 ⇒ 禁止出包（先修上面带 ✗ 的项）"
  echo "================================================================"
  exit 1
fi
echo "结论：✓ 通过（WARN ${warn} 项）"
echo "================================================================"
exit 0
