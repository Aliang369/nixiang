#!/usr/bin/env bash
# ================================================================
#  check_framework_isolation.sh — 框架层「不许出现单游戏逻辑」的机器关
#
#  背景（2026-11-11 框架收口）：
#    共用 Java（StarryApp / StarryInitProvider / MenuService / ModBridge …）里长期混着
#    单游戏逻辑，形态有三种：
#      a) 无条件调某游戏的补丁类   StarryApp 曾直接 DwctIapGate.apply(this)
#      b) 写死单游戏的临时路径     /data/local/tmp/ltzj_st/...
#      c) 写死单游戏的包名/判断    ModTarget.GAME_PKG = "com.nekki.shadowfight3"
#    后果是「一款游戏为过检测改的东西」永久留在框架里，后面每款游戏都继承。
#
#  现行口径（唯一权威＝mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/core/GameHook.java）：
#    · 单游戏差异只允许写在 com/starry/magic/games/<game_id>/GameHookImpl.java
#    · 框架层通过 GameHooks.get() 反射取得，禁止 import 任何 games.* 具体类
#
#  检查项（任一命中即 FAIL）：
#    1. 框架层（core/ 之外，排除 games/ 与 legacy 清单）出现 `games.<id>.` 的**具体类引用**
#    2. 框架 .java 里出现 /data/local/tmp/<某个游戏 tag> 形式的历史临时目录
#    3. 框架 .java 里出现 TmpXxx 式写死的游戏包名（除 ModTarget.HELPER_PKG 与包名常量表）
#    4. LegacyTapTapGames 清单条目数 > 基线（只减不增）
#    5. 新游戏 GameHookImpl 缺 targetPackage()（独立助手形态无法定位目标）
#
#  用法：bash 04_scripts/_common/check_framework_isolation.sh [--verbose]
#  退出码：0=PASS；1=FAIL；2=用法/环境错
# ================================================================
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
JAVA="$ROOT/mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic"
VERBOSE=0
[[ "${1:-}" == "--verbose" ]] && VERBOSE=1

[[ -d "$JAVA" ]] || { echo "ERROR: 缺目录 $JAVA" >&2; exit 2; }

fail=0
hit() { echo "  ✗ $*" >&2; fail=1; }
ok()  { [[ $VERBOSE -eq 1 ]] && echo "  ✓ $*"; return 0; }

echo "[1/5] 框架层不得 import 具体游戏实现类"
# 框架文件 = 除 core/games 之外的所有 .java；core 里也只查「具体类」引用
# bash 3.2（macOS 自带）无 mapfile/readarray —— 用 while read 收集（AGENTS.md bash32 纪律）
FRAMEWORK_FILES=()
while IFS= read -r _f; do
  [[ -n "$_f" ]] && FRAMEWORK_FILES+=("$_f")
done < <(find "$JAVA" -name '*.java' \
  -not -path "$JAVA/games/*" -not -path "$JAVA/core/*" 2>/dev/null)
if [[ ${#FRAMEWORK_FILES[@]} -eq 0 ]]; then
  hit "找不到框架源文件（路径变了？）"
else
  # 引用形态：com.starry.magic.games.<id>.<Class> —— 排除注释里提到的
  bad="$(grep -rn 'com\.starry\.magic\.games\.[a-z0-9_]*\.[A-Z]' "${FRAMEWORK_FILES[@]}" 2>/dev/null \
          | grep -v '^[^:]*:[0-9]*: *\*' | grep -v '://' || true)"
  if [[ -n "$bad" ]]; then
    while IFS= read -r l; do hit "框架引用了具体游戏类: $l"; done <<< "$bad"
  else
    ok "框架层无 games.<id>.<Class> 引用"
  fi
fi

echo "[2/5] 框架层不得出现单游戏的历史临时目录"
bad="$(grep -rn '/data/local/tmp/ltzj_st\|/data/local/tmp/[a-z]\{3,8\}_st' ${FRAMEWORK_FILES[@]+"${FRAMEWORK_FILES[@]}"} 2>/dev/null \
        | grep -v 'core/NativeLoader.java' || true)"
if [[ -n "$bad" ]]; then
  while IFS= read -r l; do hit "写死的单游戏临时目录: $l"; done <<< "$bad"
else
  ok "无单游戏临时目录"
fi

echo "[3/5] 框架层不得写死具体游戏包名"
# 允许的：com.starry.magic（自身/助手包名）、android 系统包、以及 core/legacy 清单
bad="$(grep -rnoE '"(com|cn|net|inland|game)\.[A-Za-z0-9_]+\.[A-Za-z0-9_.]+"' ${FRAMEWORK_FILES[@]+"${FRAMEWORK_FILES[@]}"} 2>/dev/null \
        | grep -vE 'com\.starry\.magic|com\.android|com\.google\.android|android\.' \
        | grep -vE 'com\.(cocos|taptap|anythink|chaunmi|bingo|unity|bytedance|tencent|umeng|alipay|gdt|kuaishou|twgame)\.' \
        | grep -v 'sign/TapLoginForceWeb.java' \
        | grep -v 'cocos/DwctIapGate.java' \
        | grep -vE 'StarryNativeLoad\.java:[0-9]+:"com\.qxjc\.hxwjmlq"' \
        | grep -vE '^[^:]+:[0-9]+:[[:space:]]*\*' || true)"
if [[ -n "$bad" ]]; then
  while IFS= read -r l; do hit "框架写死外部包名: $l"; done <<< "$bad"
else
  ok "无写死的外部游戏包名"
fi

echo "[4/5] LegacyTapTapGames 清单只减不增"
LEGACY="$JAVA/core/legacy/LegacyTapTapGames.java"
BASELINE=8   # 2026-11-11 建立基线时的条目数；迁移一个游戏就减一，并同步改这里
if [[ -f "$LEGACY" ]]; then
  n="$(sed -n '/GAME_IDS = new java.util.HashSet/,/));/p' "$LEGACY" | grep -cE '^\s*"' || true)"
  n="${n:-0}"
  if [[ "$n" -gt "$BASELINE" ]]; then
    hit "LegacyTapTapGames 条目 $n > 基线 ${BASELINE}（只减不增：新游戏请在自己 GameHookImpl 里调）"
  else
    ok "LegacyTapTapGames 条目 $n ≤ 基线 $BASELINE"
  fi
else
  ok "LegacyTapTapGames 已删除（全部迁移完成）"
fi

echo "[5/5] 每个 games/<id>/GameHookImpl 必须声明 targetPackage()"
missing=0
while IFS= read -r f; do
  [[ -z "$f" ]] && continue
  if ! grep -q 'targetPackage' "$f"; then
    hit "缺 targetPackage(): ${f#$ROOT/}"
    missing=1
  fi
done < <(find "$JAVA/games" -name 'GameHookImpl.java' 2>/dev/null)
[[ $missing -eq 0 ]] && ok "全部 GameHookImpl 都有 targetPackage()"

echo "[6/6] TapLoginForceWeb 包名白名单只减不增"
TLW="$JAVA/sign/TapLoginForceWeb.java"
TLW_BASELINE=47   # 2026-11-11 建立基线时的条目数（迁移一个游戏就减一并改这里）
if [[ -f "$TLW" ]]; then
  n="$(sed -n '/PKG_WHITELIST =/,/));/p' "$TLW" | grep -cE '^\s*"(com|cn|net|inland)\.' || true)"
  n="${n:-0}"
  if [[ "$n" -gt "$TLW_BASELINE" ]]; then
    hit "TapLoginForceWeb 白名单 $n > 基线 ${TLW_BASELINE}（只减不增：新游戏写进自己的 GameHookImpl）"
  else
    ok "TapLoginForceWeb 白名单 $n ≤ 基线 $TLW_BASELINE"
  fi
else
  ok "TapLoginForceWeb 已删除（全部迁移完成）"
fi

echo
if [[ $fail -eq 0 ]]; then
  echo "[check_framework_isolation] PASS"
else
  echo "[check_framework_isolation] FAIL — 框架层混入单游戏逻辑，见上方 ✗" >&2
fi
exit "$fail"
