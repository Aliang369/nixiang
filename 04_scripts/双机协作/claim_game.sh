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
  ROOT="${NIXIANG_CLAIM_ROOT:-$(cd "$(dirname "$0")/../.." 2>/dev/null && pwd -P)}"
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
    --audit) MODE=audit ;;
    -h|--help) usage ;;
    --*) usage ;;
    *) GAME="$arg" ;;
  esac
done

if [ "$MODE" = "audit" ]; then
  # 只读审计：重复行 / 多机持有 / 字段格式 / 疑似过时（>7 天，仅提示）
  if [ ! -f "$TSV" ] || [ ! -s "$TSV" ]; then
    echo "✓ 占用表为空或不存在，无冲突"
    exit 0
  fi
  problems=0
  echo "== 占用表审计 ($TSV) =="
  echo "-- 字段格式（期望 4 列：游戏<TAB>机器<TAB>时间<TAB>HEAD）--"
  bad_fmt="$(awk -F'\t' 'NF!=4 {printf "  行%d: %d 列: %s\n", NR, NF, $0}' "$TSV")"
  if [ -n "$bad_fmt" ]; then printf '%s\n' "$bad_fmt"; problems=$((problems+1)); else echo "  ✓ 全部为 4 列"; fi
  echo "-- 同一游戏多行（重复登记）--"
  dup="$(awk -F'\t' '{c[$1]++} END{for (g in c) if (c[g]>1) printf "  %s: %d 行\n", g, c[g]}' "$TSV")"
  if [ -n "$dup" ]; then printf '%s\n' "$dup"; problems=$((problems+1)); else echo "  ✓ 无重复"; fi
  echo "-- 同一游戏多机持有（真冲突）--"
  multi=0
  awk -F'\t' '
    { hosts[$1] = ($1 in hosts) ? hosts[$1] "|" $2 : $2 }
    END {
      for (g in hosts) {
        n = split(hosts[g], a, "|"); delete seen; u = 0
        for (i = 1; i <= n; i++) { if (!(a[i] in seen)) { seen[a[i]] = 1; u++ } }
        if (u > 1) printf "  %s: 由 %d 台机器持有 (%s)\n", g, u, hosts[g]
      }
    }' "$TSV" > /tmp/claim_multi.$$
  if [ -s /tmp/claim_multi.$$ ]; then cat /tmp/claim_multi.$$; multi=1; problems=$((problems+1)); else echo "  ✓ 无多机持有"; fi
  rm -f /tmp/claim_multi.$$
  echo "-- 疑似过时（>7 天，仅提示，不代表对方已收工）--"
  now="$(date '+%s')"
  stale_tmp="/tmp/claim_stale.$$"
  bad_ts_tmp="/tmp/claim_badts.$$"
  : > "$stale_tmp"; : > "$bad_ts_tmp"
  awk -F'\t' 'NF==4 {print $1"\t"$2"\t"$3}' "$TSV" | while IFS="$(printf '\t')" read -r g m t; do
    e="$(date -j -f '%Y-%m-%d %H:%M' "$t" '+%s' 2>/dev/null || date -d "$t" '+%s' 2>/dev/null || echo '')"
    if [ -n "$e" ]; then
      d=$(( (now - e) / 86400 ))
      if [ "$d" -gt 7 ]; then echo "  ${g} (${m}) 已 ${d} 天: 需人工确认对方是否结束, 不得直接删他人行" >> "$stale_tmp"; fi
    else
      echo "  ${g} (${m}) 时间无法解析: '${t}'" >> "$bad_ts_tmp"
    fi
  done
  if [ -s "$stale_tmp" ]; then cat "$stale_tmp"; fi
  # 2026-10-27 第七轮：时间戳无法解析属实质异常，不得仍报「无冲突」并返回成功
  if [ -s "$bad_ts_tmp" ]; then
    echo "-- 时间戳异常（必须人工修正，属实质问题）--"
    cat "$bad_ts_tmp"
    problems=$((problems+1))
  fi
  rm -f "$stale_tmp" "$bad_ts_tmp"
  if [ "$problems" -gt 0 ]; then
    echo "== 结论: 发现 ${problems} 类问题（只读审计, 未修改任何行）=="
    exit 1
  fi
  echo "== 结论: 无冲突 =="
  exit 0
fi

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
    # 2026-10-27 第六轮：检查该游戏的**全部**行（原来只取第一行，会漏掉多机持有/重复行）
    all_rows="$(row_of "$GAME" "")" || true
    if [ -n "$all_rows" ]; then
      # 行数 >1 ⇒ 重复登记；任一行的机器不是本机 ⇒ 真冲突
      row_n="$(printf '%s\n' "$all_rows" | grep -c . || true)"
      other="$(printf '%s\n' "$all_rows" | awk -F'\t' -v h="$HOST" '$2 != h {print $2}' | sort -u | tr '\n' ' ')"
      bad_fmt="$(printf '%s\n' "$all_rows" | awk -F'\t' 'NF!=4')"
      if [ -n "$bad_fmt" ]; then
        echo "✗ [${GAME}] 占用行格式错误（应为 4 列），先修表再认领:"
        printf '%s\n' "$bad_fmt" | sed 's/^/       /'
        echo "  → 用 bash 04_scripts/双机协作/claim_game.sh --audit 看完整诊断；不要手删他人行"
        exit 3
      fi
      if [ -n "$other" ]; then
        echo "✗ [${GAME}] 被【${other% }】占用未释放, 本机(${HOST})禁止写（该游戏共 ${row_n} 行）:"
        printf '%s\n' "$all_rows" | sed 's/^/       /'
        echo "  → 先协调 (等对方 --done 或用户拍板); 确认要强占需用户当轮明确授权"
        exit 3
      fi
      if [ "$row_n" -gt 1 ]; then
        echo "⚠ [${GAME}] 有 ${row_n} 行本机重复登记, 认领时将收敛为 1 行"
      fi
      # 全是本机持有 → 幂等刷新 (先删本机旧行, 下面重新 append)
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
