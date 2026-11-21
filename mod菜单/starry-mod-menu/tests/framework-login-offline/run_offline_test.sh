#!/usr/bin/env bash
# ================================================================
#  run_offline_test.sh — 星光「TapTap 强制网页登录 + 单游戏 SPI」离线回归
#
#  为什么有它：784 轮踩过的坑是「改了 Java 但没真编过 / 只 grep 过就算验证」。
#  本脚本用 JDK 17 把**真实源码** core/TapTapWebLogin.java、core/GameHook.java、
#  core/GameHooks.java、games/yntt/GameHookImpl.java 与 fixtures/ 下的 Android/TapSDK
#  假件一起 javac 编出来，再跑 offlinetest.OfflineLoginTest 的全部断言。
#
#  不碰设备、不联网、不写仓库内产物（中间/输出目录走临时目录）。
#
#  位置（2026-11-11 迁移）：测试源从 09_test/ 迁到 tests/ —— 09_test/ 被 .gitignore 整条忽略
#  （.gitignore:41），脚本被隐藏则 preflight --sweep 与 git 都看不见它；tests/ 可正常跟踪。
#  生成物（latest-app-compile.log 等）仍留在被忽略的 09_test/framework-login-offline/，不进仓库。
#
#  用法：
#    bash tests/framework-login-offline/run_offline_test.sh             # 跑回归
#    bash tests/framework-login-offline/run_offline_test.sh --selftest  # 验回归本身有效
#  环境：
#    STARRY_MENU_ROOT=<dir>   被测菜单根（自检用变异树时覆盖；默认本脚本所属菜单）
#  退出码：0=全绿；1=有断言失败；2=环境问题（缺 JDK17 / 缺源文件）
# ================================================================
set -uo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEFAULT_MENU_ROOT="$(cd "$HERE/../.." && pwd)"
MENU_ROOT="${STARRY_MENU_ROOT:-$DEFAULT_MENU_ROOT}"
JAVA_SRC="$MENU_ROOT/app/src/main/java/com/starry/magic"

# ---------------- 自检：变异树必须让本回归变红（否则「全绿」不可信） ----------------
stage_tree() {
  # 只拷被测/被读的文件：4 个被测源 + 2 个 R8 规则 + 2 个生命周期入口 + yntt smali
  local dst="$1" dstj f
  dstj="$dst/app/src/main/java/com/starry/magic"
  mkdir -p "$dst/app" "$dstj/core" "$dstj/games/yntt" "$dst/yntt_smali/com/starry/magic/sign"
  cp "$MENU_ROOT/app/proguard-rules.pro" "$dst/app/" 2>/dev/null
  cp "$MENU_ROOT/app/inject-dex-rules.pro" "$dst/app/" 2>/dev/null
  for f in core/TapTapWebLogin.java core/GameHook.java core/GameHooks.java games/yntt/GameHookImpl.java StarryApp.java StarryInitProvider.java; do
    cp "$JAVA_SRC/$f" "$dstj/$f" 2>/dev/null || { echo "ERROR: stage 时缺 $f" >&2; return 1; }
  done
  # T20（native/Java 一致性）读这些：两个搬迁的 Mod.cpp、games/dwct|dwdlm 的 Java 声明、
  # 以及 Mod.cpp 里仍在用的 cocos/ 类（必须存在，否则 T20 报「引用了不存在的 Java 类」）。
  mkdir -p "$dst/app/src/main/jni/Games" "$dstj/cocos" "$dstj/games/dwct" "$dstj/games/dwdlm"
  for f in Games/DwctMod.cpp Games/DwdlmMod.cpp; do
    cp "$MENU_ROOT/app/src/main/jni/$f" "$dst/app/src/main/jni/$f" 2>/dev/null || { echo "ERROR: stage 时缺 $f" >&2; return 1; }
  done
  for f in games/dwct/DwctIapGate.java games/dwdlm/DwdlmHooks.java; do
    cp "$JAVA_SRC/$f" "$dstj/$f" 2>/dev/null || { echo "ERROR: stage 时缺 $f" >&2; return 1; }
  done
  cp "$JAVA_SRC"/cocos/*.java "$dstj/cocos/" 2>/dev/null || { echo "ERROR: stage 时缺 cocos/*.java" >&2; return 1; }

  cp "$MENU_ROOT/yntt_smali/com/starry/magic/sign/YnttLoginRequest.smali" "$dst/yntt_smali/com/starry/magic/sign/" 2>/dev/null
  return 0
}

selftest() {
  local sc_bad=0 tmp base nm rc out
  tmp="$(mktemp -d "${TMPDIR:-/tmp}/starry_offline_sc.XXXXXX")" || return 2

  # 负控：未变异的拷贝必须 PASS（证明拷贝本身没坏）
  base="$tmp/clean/menu"
  if ! stage_tree "$base"; then rm -rf "$tmp"; return 2; fi
  out="$(STARRY_MENU_ROOT="$base" bash "$HERE/run_offline_test.sh" 2>&1)"; rc=$?
  if [ "$rc" = 0 ]; then
    echo "  ok 负控：未变异拷贝 rc=0（回归在纯净树上可通过）"
  else
    echo "  XX 负控失败：未变异拷贝 rc=$rc"; printf '%s\n' "$out" | tail -6 | sed 's/^/      /'; sc_bad=$((sc_bad+1))
  fi

  # 正控 1：yntt 传错 request 类名 ⇒ 反射可达性必须红（T18，且注入路 T04/T16 也该红）
  nm="$tmp/m1/menu"
  stage_tree "$nm" >/dev/null
  perl -0pi -e 's/com\.starry\.magic\.sign\.YnttLoginRequest/com.starry.magic.sign.NotYnttRequest/' \
    "$nm/app/src/main/java/com/starry/magic/games/yntt/GameHookImpl.java"
  out="$(STARRY_MENU_ROOT="$nm" bash "$HERE/run_offline_test.sh" 2>&1)"; rc=$?
  if [ "$rc" != 0 ] && printf '%s' "$out" | grep -q 'T18'; then
    echo "  ok 正控1：request 类名传错 ⇒ 变红并命中 T18（smali 可达性）"
  else
    echo "  XX 正控1失败：rc=${rc}（期望非 0 且含 T18）"; printf '%s\n' "$out" | tail -4 | sed 's/^/      /'; sc_bad=$((sc_bad+1))
  fi

  # 正控 2：只删 proguard-rules.pro 的 games.** keepnames ⇒ R8 一致性必须红（T17）
  nm="$tmp/m2/menu"
  stage_tree "$nm" >/dev/null
  grep -v 'keepnames class com.starry.magic.games' "$nm/app/proguard-rules.pro" > "$tmp/p.tmp"
  mv "$tmp/p.tmp" "$nm/app/proguard-rules.pro"
  out="$(STARRY_MENU_ROOT="$nm" bash "$HERE/run_offline_test.sh" 2>&1)"; rc=$?
  if [ "$rc" != 0 ] && printf '%s' "$out" | grep -q 'T17'; then
    echo "  ok 正控2：两 R8 规则漂移 ⇒ 变红并命中 T17"
  else
    echo "  XX 正控2失败：rc=${rc}（期望非 0 且含 T17）"; printf '%s\n' "$out" | tail -4 | sed 's/^/      /'; sc_bad=$((sc_bad+1))
  fi

  # 正控 3：StarryApp 漏调一次 onActivityCreated ⇒ 生命周期入口必须红（T19）
  nm="$tmp/m3/menu"
  stage_tree "$nm" >/dev/null
  perl -0pi -e 's/h\.onActivityCreated\(a\);/\/*removed*\//' \
    "$nm/app/src/main/java/com/starry/magic/StarryApp.java"
  out="$(STARRY_MENU_ROOT="$nm" bash "$HERE/run_offline_test.sh" 2>&1)"; rc=$?
  if [ "$rc" != 0 ] && printf '%s' "$out" | grep -q 'T19'; then
    echo "  ok 正控3：生命周期入口漏调 onActivityCreated ⇒ 变红并命中 T19"
  else
    echo "  XX 正控3失败：rc=${rc}（期望非 0 且含 T19）"; printf '%s\n' "$out" | tail -4 | sed 's/^/      /'; sc_bad=$((sc_bad+1))
  fi

  case "$tmp" in
    "${TMPDIR:-/tmp}"/*|/tmp/*) rm -rf "$tmp" ;;
    *) echo "  ⚠ 未清理：$tmp" ;;
  esac
  if [ "$sc_bad" = 0 ]; then
    echo "== 自检：ok 全绿（负控 1 + 正控 3）=="
    return 0
  fi
  echo "== 自检：XX $sc_bad 个控制点失败 ⇒ 本回归的「全绿」不可信 =="
  return 1
}

if [ "${1:-}" = "--selftest" ]; then selftest; exit $?; fi
[ -d "$MENU_ROOT/app" ] || { echo "ERROR: 菜单根无效：$MENU_ROOT" >&2; exit 2; }


# ---------------- JDK 17 ----------------
if [ -n "${JAVA_HOME:-}" ] && [ -x "${JAVA_HOME}/bin/javac" ]; then
  JH="$JAVA_HOME"
elif [ -x "/Users/aliang/Library/Java/JavaVirtualMachines/jdk-17.0.2.jdk/Contents/Home/bin/javac" ]; then
  JH="/Users/aliang/Library/Java/JavaVirtualMachines/jdk-17.0.2.jdk/Contents/Home"
elif command -v /usr/libexec/java_home >/dev/null 2>&1 && /usr/libexec/java_home -v 17 >/dev/null 2>&1; then
  JH="$(/usr/libexec/java_home -v 17)"
elif command -v javac >/dev/null 2>&1; then
  JH="$(cd "$(dirname "$(command -v javac)")/.." && pwd)"
else
  echo "ERROR: 找不到 JDK（需要 17）" >&2; exit 2
fi

UNDER_TEST=(
  "$JAVA_SRC/core/TapTapWebLogin.java"
  "$JAVA_SRC/core/GameHook.java"
  "$JAVA_SRC/core/GameHooks.java"
  "$JAVA_SRC/games/yntt/GameHookImpl.java"
)
for f in "${UNDER_TEST[@]}"; do
  [ -f "$f" ] || { echo "ERROR: 缺被测源文件 $f" >&2; exit 2; }
done

OUT="$(mktemp -d "${TMPDIR:-/tmp}/starry_offline_login.XXXXXX")" || { echo "ERROR: mktemp 失败" >&2; exit 2; }
trap 'rm -rf "$OUT"' EXIT

FIXTURES=()
while IFS= read -r f; do FIXTURES+=("$f"); done < <(find "$HERE/fixtures" -name '*.java' | sort)
TESTS=()
while IFS= read -r f; do TESTS+=("$f"); done < <(find "$HERE/src" -name '*.java' | sort)

echo "== compile (javac $("$JH/bin/javac" -version 2>&1 | sed 's/^javac //')) =="
"$JH/bin/javac" -encoding UTF-8 -d "$OUT/classes" \
  "${FIXTURES[@]}" "${TESTS[@]}" "${UNDER_TEST[@]}" 2>&1 | sed 's/^/   /'
rc=${PIPESTATUS[0]}
if [ "$rc" != 0 ]; then
  echo "FAIL: javac exit=$rc ⇒ 登录重构在真实编译器下不成立（菜单根 ${MENU_ROOT}）" >&2
  exit 1
fi
echo "   ok javac exit=0（$(( ${#FIXTURES[@]} + ${#TESTS[@]} + ${#UNDER_TEST[@]} )) 个源文件）"
echo

"$JH/bin/java" -Dstarry.menu.root="$MENU_ROOT" -Dstarry.repo.root="$MENU_ROOT" \
  -cp "$OUT/classes" offlinetest.OfflineLoginTest
rc=$?
echo
if [ "$rc" = 0 ]; then
  echo "[run_offline_test] PASS"
else
  echo "[run_offline_test] FAIL exit=$rc" >&2
fi
exit "$rc"
