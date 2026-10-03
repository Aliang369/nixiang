#!/usr/bin/env bash
# ============================================================================
# claim_game.sh - 一机一游戏归属登记 (双机协作, bash 3.2 兼容)
#
# 用法:
#   bash 04_scripts/双机协作/claim_game.sh <游戏名>          # 认领 (本机已持有则幂等刷新)
#   bash 04_scripts/双机协作/claim_game.sh --done <游戏名>   # 收工释放 (删本机的占用行)
#   bash 04_scripts/双机协作/claim_game.sh --list            # 查看当前占用
#
# 台账: 10_docs/双机协作/本轮归属.tsv
#       (tab 分隔: 游戏 / 机器 / 时间 / HEAD; 只存"占用中"的行, --done 即删行, 历史在 git)
#
# 规则: 对方已认领未释放的游戏 → 本机禁止写 (退出码 3, 打印占用行)。
#       认领与释放都要随收工 git 提交, 否则对方看不到。
# ============================================================================
set -u

ROOT="${NIXIANG_ROOT:-}"
if [ -z "$ROOT" ]; then
  ROOT="$(cd "$(dirname "$0")/../.." 2>/dev/null && pwd -P)"
fi

case "$(uname -s)" in
  Darwin) HOST=mac ;;
  MINGW*|MSYS*|CYGWIN*) HOST=win ;;
  *) echo "✗ 无法判定本机类型 ($(uname -s)), 拒绝登记"; exit 2 ;;
esac

TSV="$ROOT/10_docs/双机协作/本轮归属.tsv"

usage() { sed -n '2,14p' "$0"; exit 2; }

# row_of <游戏> [机器] : 打印匹配的占用行 (无匹配时退出码非 0)
# 用 awk 字符串等值比较, 避开游戏名里的正则元字符
row_of() {
  [ -f "$TSV" ] || return 1
  if [ -n "${2:-}" ]; then
    awk -F'\t' -v g="$1" -v h="$2" '$1==g && $2==h {print; found=1} END{exit(found?0:1)}' "$TSV"
  else
    awk -F'\t' -v g="$1" '$1==g {print; found=1} END{exit(found?0:1)}' "$TSV"
  fi
}

# remove_row <游戏> <机器> : 从台账删本机对该游戏的占用行 (原子: 临时文件 + mv)
remove_row() {
  [ -f "$TSV" ] || return 0
  tmp="$TSV.tmp.$$"
  awk -F'\t' -v g="$1" -v h="$2" '!($1==g && $2==h)' "$TSV" > "$tmp" && mv "$tmp" "$TSV"
}

MODE=claim
GAME=""
for arg in "$@"; do
  case "$arg" in
    --done) MODE=done ;;
    --list) MODE=list ;;
    -h|--help) usage ;;
    --*) usage ;;
    *) GAME="$arg" ;;
  esac
done

if [ "$MODE" = "list" ]; then
  if [ -f "$TSV" ] && [ -s "$TSV" ]; then
    echo "当前占用 (10_docs/双机协作/本轮归属.tsv):"
    sed 's/^/  /' "$TSV"
  else
    echo "当前无任何游戏被占用"
  fi
  exit 0
fi

[ -n "$GAME" ] || usage

case "$MODE" in
  claim)
    holder_row="$(row_of "$GAME" "")" || true
    if [ -n "$holder_row" ]; then
      holder="$(printf '%s\n' "$holder_row" | awk -F'\t' 'NR==1{print $2}')"
      if [ "$holder" != "$HOST" ]; then
        echo "✗ [${GAME}] 已被【${holder}】占用未释放, 本机(${HOST})禁止写:"
        printf '%s\n' "$holder_row" | sed 's/^/       /'
        echo "  → 先协调 (等对方 --done 或用户拍板); 确认要强占需用户当轮明确授权"
        exit 3
      fi
      # 本机已持有 → 幂等刷新 (先删旧行, 下面重新 append)
    fi
    head_short="$(git -C "$ROOT" rev-parse --short HEAD 2>/dev/null || echo unknown)"
    mkdir -p "$(dirname "$TSV")"
    remove_row "$GAME" "$HOST"
    printf '%s\t%s\t%s\t%s\n' "$GAME" "$HOST" "$(date '+%Y-%m-%d %H:%M')" "$head_short" >> "$TSV"
    echo "✓ 已登记占用: [${GAME}] → ${HOST} (HEAD ${head_short})"
    echo "  收工时记得: bash 04_scripts/双机协作/claim_game.sh --done ${GAME}"
    ;;
  done)
    if row_of "$GAME" "$HOST" >/dev/null 2>&1; then
      remove_row "$GAME" "$HOST"
      echo "✓ 已释放: [${GAME}] (${HOST})"
    else
      echo "ℹ 本机没有 [${GAME}] 的占用行 (可能已释放或从未认领), 无操作"
    fi
    ;;
esac
exit 0
