#!/usr/bin/env bash
# ================================================================
#  install_guard.sh — 装机策略闸门（P1-⑥，2026-10-11 无悔华夏会话复盘）
#
#  为什么要有这个脚本
#  ----------------------------------------------------------------
#  无悔华夏 3.8.280 的**装机策略是特例**：乐变（lebian）派生负载目录
#  （/data/data/<pkg>/lebian/<pkg>-1/lib/arm64/libil2cpp.so）只在「全新安装」时重建。
#  用 `adb install -r` 覆盖安装时，乐变保留 resourceInfo.cfg 里的 currVer 而不重建负载，
#  游戏于是去映射 APK 内那份**加密** libil2cpp.so ⇒ Il2Cpp API 拿不到、RVA/按名钩全失效。
#  用户原话（m00888）：「无悔华夏每次安装游戏都需要卸载重装，不要直接覆盖安装，
#  不然资源会加载出错，这个问题已经说了十几次」。
#
#  但仓库里同时存在一条**相反**的通用纪律（AGENTS.md:66 / TOOLBOOK.md 规则 77 /
#  skill/starry-mod/SKILL.md:282：优先覆盖安装、不要卸载）——那条对**大多数游戏**成立，
#  对无悔华夏**不适用**。两条规则打架时靠记忆 = 必然出事，所以这里做成脚本闸门：
#  install_policy 存在 04_scripts/<游戏>/pipeline.json 里，本脚本读它做裁决。
#
#  用法
#  ----------------------------------------------------------------
#    bash 04_scripts/_common/install_guard.sh <游戏> --policy
#        只打印该游戏的 install_policy 与说明（exit 0）
#
#    bash 04_scripts/_common/install_guard.sh <游戏> --cmd "adb -s X install -r app.apk"
#        检查一段**待执行命令字符串**（装机脚本里最常用）
#
#    bash 04_scripts/_common/install_guard.sh <游戏> --adb-args -s X install -r app.apk
#        检查一段参数向量（$* 形式）
#
#    bash 04_scripts/_common/install_guard.sh --pkg com.yhhy.whrhx.aligames --cmd "…"
#        用包名反查游戏（扫 04_scripts/*/pipeline.json 的 params.package）
#
#  退出码
#  ----------------------------------------------------------------
#    0 = 允许（策略不是 uninstall_only，或命令里没有覆盖安装语义）
#   65 = 拒绝（uninstall_only 游戏 + 命令含 -r/--replace/-d/--downgrade 等覆盖安装）  ← EX_DATAERR
#    2 = 用法错（缺参数/未知参数）
#    3 = 策略未知（找不到 pipeline.json 或没有 install_policy 字段）
#
#  典型接法（装机脚本开头）
#  ----------------------------------------------------------------
#    bash 04_scripts/_common/install_guard.sh 无悔华夏 --cmd "adb -s $SERIAL install -r $APK" || exit $?
#    # ⇒ 打印 exit 65 与整改指令（改走 whrhx_clean_reinstall.sh --uninstall）
#
#  刻意不做「环境变量强行放行」：这条规则的失败代价是整轮功能验证白做
#  （2026-10-11 实测：装错一次 = 乐变负载目录消失 + 全仓钩子失效 + 63s 卸载重装返工）。
#  真有正当理由要覆盖安装时，请改的是 pipeline.json 的 install_policy，并在台账留一行。
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="${NIXIANG_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
GAMES_DIR="$NIXIANG_ROOT/04_scripts"

GAME=""; PKG=""; MODE=""; CMDSTR=""
while [ $# -gt 0 ]; do
  case "$1" in
    --policy)   MODE=policy; shift ;;
    --cmd)      MODE=cmd;    CMDSTR="${2:-}"; shift 2 ;;
    --adb-args) MODE=args;   shift; CMDSTR="$*"; break ;;
    --pkg)      PKG="${2:-}"; shift 2 ;;
    --game)     GAME="${2:-}"; shift 2 ;;
    -h|--help)  sed -n '1,60p' "$0"; exit 0 ;;
    -*)         echo "未知参数: $1" >&2; echo "见: bash $0 --help" >&2; exit 2 ;;
    *)          GAME="$1"; shift ;;
  esac
done

# ---- pip 省略：定位游戏 ----
if [ -z "$GAME" ] && [ -n "$PKG" ]; then
  for f in "$GAMES_DIR"/*/pipeline.json; do
    [ -f "$f" ] || continue
    if grep -q "\"package\"[[:space:]]*:[[:space:]]*\"$PKG\"" "$f" 2>/dev/null; then
      GAME="$(basename "$(dirname "$f")")"
      break
    fi
  done
  if [ -z "$GAME" ]; then
    echo "INSTALL_GUARD: 无法由包名 $PKG 反查游戏（扫 $GAMES_DIR/*/pipeline.json 未命中）" >&2
    exit 3
  fi
fi

if [ -z "$GAME" ]; then
  echo "用法: bash $0 <游戏> --policy | --cmd \"<命令>\" | --adb-args <args…> [--pkg <包名>]" >&2
  exit 2
fi

PIPE="$GAMES_DIR/$GAME/pipeline.json"
if [ ! -f "$PIPE" ]; then
  echo "INSTALL_GUARD[$GAME]: 无 $PIPE ⇒ 装机策略未知（补 pipeline.json 的 install_policy 字段）" >&2
  exit 3
fi

# ---- 读策略（避开 jq 依赖：仓库里 pipeline.json 都是扁平 ASCII 字段）----
POLICY="$(LC_ALL=C grep -o '"install_policy"[[:space:]]*:[[:space:]]*"[^"]*"' "$PIPE" 2>/dev/null | head -1 | sed 's/.*"\([^"]*\)"$/\1/')"
if [ -z "$POLICY" ]; then
  echo "INSTALL_GUARD[$GAME]: pipeline.json 无 install_policy 字段 ⇒ 策略未知" >&2
  echo "  默认按通用纪律处理（允许覆盖安装）；本游戏若属卸载重装类，请补字段：\"install_policy\": \"uninstall_only\"" >&2
  exit 3
fi

echo "INSTALL_GUARD game=$GAME policy=$POLICY"

if [ "$MODE" = "policy" ] || [ -z "$MODE" ]; then
  case "$POLICY" in
    uninstall_only)
      echo "  ⇒ 禁止覆盖安装（adb install -r / --replace / -d）。装机唯一入口："
      echo "     WHRHX_APK=<apk> bash 04_scripts/$GAME/whrhx_clean_reinstall.sh --uninstall"
      echo "     理由见 10_docs/$GAME/硬约束.md 第 4 条（乐变负载不重建 ⇒ 钩子全失效）"
      ;;
    *) echo "  ⇒ 通用纪律（允许覆盖安装；卸载/清数据前先确认不用保档）" ;;
  esac
  exit 0
fi

# ---- 命令检查 ----
# 覆盖安装语义：install 子命令后带 -r / --replace / -d / --downgrade，
# 或 pm install 带 -r，或 install-multiple / install-multi-package（同为替换语义）。
low="$(printf '%s' "$CMDSTR" | tr 'A-Z' 'a-z')"
HIT=""
case "$low" in
  *"install -r "*|*"install -r"|*"--replace"*)                       HIT="adb install -r / --replace" ;;
  *"install -d "*|*"install -d"|*"--downgrade"*)                     HIT="adb install -d / --downgrade" ;;
  *"install-multiple"*|*"install-multi-package"*)                    HIT="install-multiple（替换语义）" ;;
esac
# `pm install -r`（走 adb shell）单列，便于打印更准的整改提示
case "$low" in *"pm install -r"*) HIT="pm install -r" ;; esac

if [ -n "$HIT" ]; then
  if [ "$POLICY" = "uninstall_only" ]; then
    cat >&2 <<EOF
INSTALL_GUARD: ✗ 拒绝执行（exit 65）
  游戏    : $GAME
  策略    : install_policy=${POLICY}（只能卸载重装）
  命中    : $HIT
  命令    : $CMDSTR
  为什么  : 覆盖安装不会让乐变重建派生负载目录，游戏会映射 APK 内加密 libil2cpp.so，
            所有 RVA/按名钩失效（用户 m00888 已强调十几次的本游戏特例）。
  怎么办  : 改走唯一入口
            WHRHX_APK=<apk> bash 04_scripts/$GAME/whrhx_clean_reinstall.sh --uninstall
            （等价于 pm uninstall <pkg> && adb install <apk>，装机后等乐变回填 ~60s）
  依据    : 10_docs/$GAME/硬约束.md 第 4 条 + 04_scripts/$GAME/pipeline.json install_policy
EOF
    exit 65
  fi
  echo "INSTALL_GUARD: ✓ 允许（策略=${POLICY}，命中 $HIT 但通用纪律允许覆盖安装）"
  exit 0
fi

# 没命中覆盖安装：若是 install 且策略为 uninstall_only，给一条正向提醒（不算失败）
case "$low" in
  *" install "*|*"install "*)
    if [ "$POLICY" = "uninstall_only" ]; then
      echo "  ⚠ 本游戏策略=uninstall_only：全新安装前必须先 pm uninstall（脚本已按此实现？）"
      echo "    否则 adb install 会 INSTALL_FAILED_ALREADY_EXISTS，或落到乐变负载不一致的状态。"
    fi
    ;;
esac
echo "INSTALL_GUARD: ✓ 允许"
exit 0
