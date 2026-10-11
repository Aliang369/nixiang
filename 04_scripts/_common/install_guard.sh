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
#  装机策略取值（pipeline.json 的 install_policy）
#  ----------------------------------------------------------------
#    uninstall_only ：只能「先卸载再全新安装」（无悔华夏：乐变负载目录只在新装时重建）
#    overlay_only   ：只许**覆盖安装**（adb install -r -d），**禁止卸载/清数据**（霓虹深渊2：
#                     官方包 1.3GB、同签名覆盖即可；卸载会丢官方包与账号态）
#                     确需卸载时加 --allow-uninstall 或 INSTALL_GUARD_ALLOW_UNINSTALL=1，
#                     并在轮次台账留一行理由。
#                     （2026-10-12 霓虹深渊2 会话复盘 P1-9：该会话出现 7 次 uninstall，
#                       招致用户质问「同签名游戏你为什么还要卸载重装？」）
#
#    bash 04_scripts/_common/install_guard.sh <游戏> --cmd "adb uninstall <pkg>"   # overlay_only ⇒ 66
#
#  退出码
#  ----------------------------------------------------------------
#    0 = 允许（策略不是 uninstall_only，或命令里没有覆盖安装语义）
#   65 = 拒绝（uninstall_only 游戏 + 命令含 -r/--replace/-d/--downgrade 等覆盖安装）  ← EX_DATAERR
#   66 = 拒绝（overlay_only 游戏 + 命令含卸载/清数据，且未显式 --allow-uninstall）    ← EX_NOINPUT
#    2 = 用法错（缺参数/未知参数）
#    3 = 策略未知（找不到 pipeline.json 或没有 install_policy 字段）
#       ⚠ exit 3 **不是**「可以随便装」的许可：这正是 2026-10-12 复盘查到的黑洞 ——
#       霓虹深渊2 当时没有 pipeline.json ⇒ 本闸门形同虚设、全程 0 次调用。新游戏建目录时先把 pipeline.json 建好。
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
ALLOW_UNINSTALL="${INSTALL_GUARD_ALLOW_UNINSTALL:-0}"
while [ $# -gt 0 ]; do
  case "$1" in
    --policy)   MODE=policy; shift ;;
    --cmd)      MODE=cmd;    CMDSTR="${2:-}"; shift 2 ;;
    --adb-args) MODE=args;   shift; CMDSTR="$*"; break ;;
    --pkg)      PKG="${2:-}"; shift 2 ;;
    --game)     GAME="${2:-}"; shift 2 ;;
    --allow-uninstall) ALLOW_UNINSTALL=1; shift ;;
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
  echo "  若是「只许覆盖、禁止卸载」类（如霓虹深渊2），请补：\"install_policy\": \"overlay_only\"" >&2
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
    overlay_only)
      echo "  ⇒ 只许覆盖安装（adb install -r -d），**禁止卸载/清数据**（uninstall / pm clear / pm uninstall）"
      echo "     理由：官方包 1.3GB + 同签名覆盖即可；卸载会丢官方包与账号态、还要重下 1.3GB"
      echo "     确需卸载时（换签名/修损坏安装）：--allow-uninstall 或 INSTALL_GUARD_ALLOW_UNINSTALL=1，并在轮次台账留一行"
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

# ---- 卸载 / 清数据语义（overlay_only 策略下必须显式放行）----
UNHIT=""
case "$low" in
  *"uninstall"*)                              UNHIT="uninstall" ;;
  *"pm clear"*|*"pm  clear"*)                 UNHIT="pm clear（清应用数据）" ;;
  *"rm -rf /data/data"*|*"rm -rf /data/user"*) UNHIT="删数据目录" ;;
esac

if [ -n "$UNHIT" ] && [ "$POLICY" = "overlay_only" ] && [ "$ALLOW_UNINSTALL" != "1" ]; then
  cat >&2 <<EOF
INSTALL_GUARD: ✗ 拒绝执行（exit 66）
  游戏    : $GAME
  策略    : install_policy=${POLICY}（只许覆盖安装，禁止卸载/清数据）
  命中    : $UNHIT
  命令    : $CMDSTR
  为什么  : 本游戏同签名覆盖安装即可（adb install -r -d）。卸载会丢官方包与账号态，还要重下 1.3GB；
            2026-10-12 霓虹深渊2 会话复盘：该会话出现 7 次 uninstall，招致用户质问
            「同签名游戏你为什么还要卸载重装？」——闸门存在却被旁路（当时没有 pipeline.json）。
  怎么办  : ① 改覆盖安装：    adb -s <serial> install -r -d <已签名 apk>
            ② 确需卸载/清数据：加 --allow-uninstall （或 INSTALL_GUARD_ALLOW_UNINSTALL=1），
               并在 10_docs/$GAME/轮次台账.tsv 留一行理由
  依据    : 04_scripts/$GAME/pipeline.json install_policy + 10_docs/$GAME/硬约束.md
EOF
  exit 66
fi
if [ -n "$UNHIT" ] && [ "$POLICY" = "overlay_only" ] && [ "$ALLOW_UNINSTALL" = "1" ]; then
  echo "INSTALL_GUARD: ⚠ 放行卸载（策略=${POLICY}，命中 ${UNHIT}，已显式 --allow-uninstall）⇒ 请在轮次台账留一行理由"
fi

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
    elif [ "$POLICY" = "overlay_only" ] && [ -z "$HIT" ]; then
      echo "  ⚠ 本游戏策略=overlay_only：覆盖安装要带 -r（adb install -r -d），否则会 INSTALL_FAILED_ALREADY_EXISTS"
    fi
    ;;
esac
echo "INSTALL_GUARD: ✓ 允许"
exit 0
