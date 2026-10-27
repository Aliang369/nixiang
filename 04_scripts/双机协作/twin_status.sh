#!/usr/bin/env bash
# ============================================================================
# twin_status.sh - 双机协作一键体检 (mac / win 通用, bash 3.2 兼容)
#
# 用法:  bash 04_scripts/双机协作/twin_status.sh
# 退出码: 0 = 全绿 / 1 = 有 FAIL / 2 = 仅 WARN
#
# 覆盖: 仓库定位与兼容路径 / git 状态与远端 / 换行符白名单 / mac 命令垫片
#       env.sh 六项 / python / adb 设备 / SMB 共享 / Syncthing 进程与 GUI
#       AI 入口文件规范一致性 (防 CLAUDE.md 类入口复制正文后静默脱队)
#
# 说明: 本脚本是唯一被允许做"整仓 git status"的地方(体检用途, 只读)。
#       手工排查时仍按 AGENTS.md 纪律带 pathspec。
# ============================================================================
set -u

PASS_N=0; WARN_N=0; FAIL_N=0
pass() { PASS_N=$((PASS_N + 1)); printf '[PASS] %s\n' "$*"; }
warn() { WARN_N=$((WARN_N + 1)); printf '[WARN] %s\n' "$*"; }
fail() { FAIL_N=$((FAIL_N + 1)); printf '[FAIL] %s\n' "$*"; }
info() { printf '       %s\n' "$*"; }
sect() { printf '\n== %s ==\n' "$*"; }

if [ "${1:-}" = "-h" ] || [ "${1:-}" = "--help" ]; then
  sed -n '2,12p' "$0"
  exit 0
fi

MAC_ROOT=/Users/aliang/Documents/nixiang
SHA_X=2d711642b726b04401627ca9fbac32f5c8530fb1903cc4db02258717921a4881
MD5_X=9dd4e461268c8034f5c8564e155c67a6
TMPD="${TMPDIR:-/tmp}"
TMPD="${TMPD%/}"

case "$(uname -s)" in
  Darwin) HOST=mac ;;
  MINGW*|MSYS*|CYGWIN*) HOST=win ;;
  *) HOST=unknown ;;
esac

ROOT="${NIXIANG_ROOT:-}"
if [ -z "$ROOT" ]; then
  ROOT="$(cd "$(dirname "$0")/../.." 2>/dev/null && pwd -P)"
fi

printf 'nixiang 双机体检  host=%s  root=%s\n' "$HOST" "$ROOT"

# --------------------------------------------------------------------------
sect "1. 仓库定位"
if [ -n "$ROOT" ] && [ -d "$ROOT/.git" ]; then
  pass "repo root: $ROOT"
else
  fail "不是 git 仓库: ${ROOT:-<empty>}"
fi
if [ "$HOST" = win ]; then
  if (cd "$MAC_ROOT" 2>/dev/null); then
    pass "兼容路径可进入: $MAC_ROOT"
  else
    fail "兼容路径失效: $MAC_ROOT (junction 丢了? 重跑 win_bootstrap.ps1 步骤 6)"
  fi
fi

# --------------------------------------------------------------------------
sect "2. git 状态与远端"
if [ -d "$ROOT/.git" ]; then
  cd "$ROOT" || fail "cd $ROOT 失败"
  head_short="$(git rev-parse --short HEAD 2>/dev/null)"
  head_full="$(git rev-parse HEAD 2>/dev/null)"
  branch_line="$(git status -sb 2>/dev/null | head -1)"
  info "HEAD    : $head_short"
  info "branch  : $branch_line"
  porcelain="$(git status --porcelain --untracked-files=no 2>/dev/null)"
  # 心跳自指豁免: 心跳一写树就脏, 下一轮体检把自己记成 WARN 会污染心跳结果误导对方 (Win 端 2026-10-04 实测发现)
  dirty="$(printf '%s\n' "$porcelain" | grep -v '10_docs/_state/机器心跳/' | grep -c . || true)"
  hb_dirty="$(printf '%s\n' "$porcelain" | grep -c '10_docs/_state/机器心跳/' || true)"
  if [ "$dirty" = "0" ]; then
    pass "已跟踪文件无改动 (心跳文件豁免)"
  else
    warn "已跟踪文件有 $dirty 处未提交改动 (收尾前必须处理)"
  fi
  if [ "${hb_dirty:-0}" != "0" ]; then
    info "心跳未提交 $hb_dirty 处 (收工时随 git 提交, 不计 WARN)"
  fi
  if git rev-parse --verify -q origin/main >/dev/null 2>&1; then
    behind="$(git rev-list --left-right --count origin/main...HEAD 2>/dev/null | awk '{print $1}')"
    ahead="$(git rev-list --left-right --count origin/main...HEAD 2>/dev/null | awk '{print $2}')"
    info "origin/main: $(git rev-parse --short origin/main 2>/dev/null)  behind=$behind ahead=$ahead"
    if [ "${behind:-0}" != "0" ]; then
      warn "落后 origin/main $behind 个提交 - 开工前先 git pull --rebase --autostash"
    else
      pass "不落后远端"
    fi
    if [ "${ahead:-0}" != "0" ]; then
      warn "本地领先 $ahead 个提交未 push"
    fi
  else
    warn "本地没有 origin/main 引用 (先 git fetch)"
  fi
else
  fail "跳过 git 检查"
fi

# --------------------------------------------------------------------------
sect "3. 换行符白名单"
if [ -d "$ROOT/.git" ]; then
  crlf_all="$(git ls-files --eol 2>/dev/null | grep 'i/crlf' || true)"
  n_crlf="$(printf '%s\n' "$crlf_all" | grep -c . || true)"
  bad="$(printf '%s\n' "$crlf_all" | grep -v -E '06_dumped/.*/apk_assets/|06_dumped/.*/il2cpp_dump_|10_docs/掌门下山/lua_dump/' | grep . || true)"
  n_bad="$(printf '%s\n' "$bad" | grep -c . || true)"
  info "index 里 i/crlf 共 ${n_crlf} 行 (白名单上限 15)"
  if [ "${n_bad:-0}" = "0" ] && [ "${n_crlf:-0}" -le 15 ]; then
    pass "白名单外零 CRLF"
  else
    fail "白名单外出现 CRLF:"
    printf '%s\n' "$bad" | head -20 | while read -r l; do info "$l"; done
  fi
fi

# --------------------------------------------------------------------------
sect "4. mac 命令垫片 (md5 / stat -f / shasum)"
probe="$TMPD/.nx_twin_probe.$$"
rm -f "$probe"
if printf x > "$probe" 2>/dev/null; then
  m="$(md5 -q "$probe" 2>/dev/null || true)"
  sz="$(stat -f %z "$probe" 2>/dev/null || true)"
  sh="$(shasum -a 256 "$probe" 2>/dev/null | awk '{print $1}')"
  cm="$(bash -c "md5 -q $probe" 2>/dev/null || true)"
  info "md5 -q = ${m:-<空>}"; info "stat -f %z = ${sz:-<空>}"; info "shasum -a 256 = ${sh:-<空>}"
  if [ "$m" = "$MD5_X" ]; then pass "md5 垫片正确"; else fail "md5 -q 不符合预期 (期望 $MD5_X)"; fi
  if [ "$sz" = "1" ]; then pass "stat -f %z 垫片正确"; else fail "stat -f %z 不是 1 (GNU stat 没被垫住?)"; fi
  if [ "$sh" = "$SHA_X" ]; then pass "shasum -a 256 正确"; else fail "shasum -a 256 不符合预期"; fi
  if [ -n "$cm" ] && [ "$cm" = "$m" ]; then pass "垫片可被非交互子 bash 继承 (bash -c)"; else fail "bash -c 里垫片没继承 (检查 export -f / BASH_ENV)"; fi
  if [ "$HOST" = win ]; then info "BASH_ENV=${BASH_ENV:-<未设置>}"; fi
  rm -f "$probe"
else
  fail "无法在 $TMPD 写探针文件"
fi

# --------------------------------------------------------------------------
sect "5. env.sh 六项"
tmp_env="$TMPD/.nx_twin_env.$$"
cat > "$tmp_env" <<'NXEOF'
cd "$1" || exit 9
. 04_scripts/_common/env.sh
printf '%s|%s|%s|%s|%s|%s\n' \
  "${NIXIANG_HOST:-}" "${NIXIANG_PYTHON:-}" "${NIXIANG_BUILD_TOOLS:-}" \
  "${NIXIANG_ZIPALIGN:-}" "${NIXIANG_APKSIGNER:-}" "${NIXIANG_JAVA17:-}"
NXEOF
six="$(bash "$tmp_env" "$ROOT" 2>/dev/null || true)"
rm -f "$tmp_env"
IFS='|' read -r f_host f_py f_bt f_za f_ap f_j17 <<NXEOF
$six
NXEOF
info "HOST=$f_host"
info "PYTHON=$f_py"
info "BUILD_TOOLS=$f_bt"
info "ZIPALIGN=$f_za"
info "APKSIGNER=$f_ap"
info "JAVA17=$f_j17"
missing=""
[ -n "${f_host:-}" ] || missing="$missing HOST"
[ -n "${f_py:-}" ] || missing="$missing PYTHON"
[ -n "${f_bt:-}" ] || missing="$missing BUILD_TOOLS"
[ -n "${f_za:-}" ] || missing="$missing ZIPALIGN"
[ -n "${f_ap:-}" ] || missing="$missing APKSIGNER"
[ -n "${f_j17:-}" ] || missing="$missing JAVA17"
if [ -z "$missing" ]; then pass "六项全部非空"; else fail "env.sh 解析为空:$missing"; fi
if [ "${f_host:-}" = "$HOST" ]; then pass "NIXIANG_HOST 与本机一致 ($HOST)"; else warn "NIXIANG_HOST=$f_host 与本机 $HOST 不一致"; fi
if [ "${f_py:-}" != "" ] && [ -x "$f_py" ]; then pass "python 可执行: $f_py"; else warn "python 不可执行: ${f_py:-<空>}"; fi
if [ "${f_j17:-}" != "" ] && [ -x "$f_j17/bin/javac" -o -x "$f_j17/bin/javac.exe" ]; then
  pass "javac 存在: $f_j17/bin/javac"
else
  warn "javac 不存在于 ${f_j17:-<空>}/bin"
fi

# --------------------------------------------------------------------------
sect "6. 设备 (adb)"
if command -v adb >/dev/null 2>&1; then
  info "ANDROID_SERIAL=${ANDROID_SERIAL:-<未设置>}"
  devs="$(adb devices 2>/dev/null | awk 'NR>1 && NF {printf "%s(%s) ", $1, $2}')"
  if [ -n "$devs" ]; then
    pass "adb 设备: $devs"
    if [ -n "${ANDROID_SERIAL:-}" ]; then
      if (adb -s "$ANDROID_SERIAL" get-state >/dev/null 2>&1); then
        pass "ANDROID_SERIAL=$ANDROID_SERIAL 可达"
      else
        fail "ANDROID_SERIAL=$ANDROID_SERIAL 不可达 (模拟器/adb server 重启后要先 adb connect)"
      fi
    fi
  else
    warn "adb 没有列出设备 (模拟器没开 / 手机没插 / 要先 adb connect 127.0.0.1:16384)"
  fi
else
  warn "adb 不在 PATH"
fi

# --------------------------------------------------------------------------
sect "7. SMB 互传 (E:\\nixiang 共享)"
if (exec 3<>/dev/tcp/127.0.0.1/445) 2>/dev/null; then
  exec 3>&- 2>/dev/null || true
  pass "SMB 服务在监听 (445)"
else
  warn "本机 445 未监听 (LanmanServer 没起?)"
fi
if [ "$HOST" = win ]; then
  if net share nixiang >/dev/null 2>&1; then pass "共享 nixiang 存在"; else warn "共享 nixiang 不存在 (New-SmbShare -Name nixiang -Path E:\\nixiang)"; fi
else
  if smbutil statshares -a 2>/dev/null | grep -q nixiang; then pass "已挂载 nixiang"; else warn "未挂载 nixiang (访达 cmd+K -> smb://192.168.10.9/nixiang)"; fi
fi

# --------------------------------------------------------------------------
sect "8. Syncthing 大文件同步"
if [ "$HOST" = win ]; then
  proc="$(ps -W 2>/dev/null | grep -c '[s]yncthing.exe' || true)"
else
  proc="$(pgrep -f '[s]yncthing' 2>/dev/null | wc -l | tr -d ' ')"
fi
if [ "${proc:-0}" != "0" ]; then pass "syncthing 进程数 $proc"; else fail "syncthing 没在跑 (带出门回来/重启后最容易静默挂掉的就是它)"; fi
if command -v curl >/dev/null 2>&1; then
  code="$(curl -s -m 4 -o /dev/null -w '%{http_code}' http://127.0.0.1:8384/rest/noauth/health 2>/dev/null || true)"
  if [ "$code" = "200" ]; then pass "GUI 健康检查 200 (http://127.0.0.1:8384)"; else warn "GUI 健康检查返回 ${code:-<空>}"; fi
else
  warn "没有 curl, 跳过 GUI 检查"
fi
if [ "$HOST" = win ]; then
  # 注意: MSYS 会把 /query 当成路径改写掉, 必须写 //query
  if schtasks //query //tn nixiang-syncthing >/dev/null 2>&1; then pass "自启任务 nixiang-syncthing 已注册"; else warn "自启任务 nixiang-syncthing 不存在"; fi
fi

# --------------------------------------------------------------------------
sect "9. AI 入口文件规范一致性 (防入口漂移)"
# AGENTS.md「工具专属文件」节: 入口只放指针, 不复制政策正文。
# 指纹: 文件含 "git pull --rebase" 或 "git status -sb" 即视为复制了开工正文;
#       复制正文的入口必须带齐现行开工关键口令 (口令清单随 AGENTS.md 升级维护)。
# 背景: 2026-10-04 发现 CLAUDE.md 停在 10-02 老三步开工, 架空了本体检与心跳。
if [ -f "$ROOT/AGENTS.md" ]; then
  if grep -q 'twin_status' "$ROOT/AGENTS.md" 2>/dev/null; then
    pass "AGENTS.md 源头含 twin_status 开工口令"
  else
    fail "AGENTS.md 缺 twin_status 开工口令 (源头被改坏? 本节检查失效)"
  fi
else
  fail "AGENTS.md 不存在"
fi
n_ptr=0
for ef in "$ROOT/CLAUDE.md" "$ROOT/.trae/rules/project_rules.md" "$ROOT"/.cursor/rules/*.mdc; do
  [ -f "$ef" ] || continue
  if ! grep -q -E 'git pull --rebase|git status -sb' "$ef" 2>/dev/null; then
    n_ptr=$((n_ptr + 1))
    continue
  fi
  miss_t=""
  for tok in twin_status claim_game; do
    grep -q "$tok" "$ef" 2>/dev/null || miss_t="$miss_t $tok"
  done
  if [ -n "$miss_t" ]; then
    warn "入口复制开工正文但缺口令:$miss_t -> ${ef#"$ROOT"/} (照 AGENTS.md「双机协作」节更新)"
  else
    pass "入口正文口令齐全: ${ef#"$ROOT"/}"
  fi
done
pass "纯指针入口 $n_ptr 个, 不承载正文不漂移"

# 入口文件边界（2026-10-15 用户 m00006 定，机检版）:
#   AGENTS.md 只放通用规则与路由 —— 日志正文与单游戏例外正文不得回流。
#   判据 1: 入口内不得出现「日期开头的日志条目」（日志全在 10_docs/_state/规范变更日志.md）
#   判据 2: 入口体量仍在 D6 闸门内（≤32768 B）
if [ -f "$ROOT/AGENTS.md" ]; then
  n_log="$(grep -c -E '^[[:space:]]*-[[:space:]]*[0-9]{4}-[0-9]{2}-[0-9]{2}[（(]' "$ROOT/AGENTS.md" 2>/dev/null || true)"
  if [ "${n_log:-0}" -gt 0 ]; then
    fail "AGENTS.md 含 $n_log 条日志正文（入口不放日志）-> 挪到 10_docs/_state/规范变更日志.md，入口只留一行指针"
  else
    pass "AGENTS.md 无日志条目（日志统一在 规范变更日志.md）"
  fi
  sz="$(wc -c < "$ROOT/AGENTS.md" | tr -d ' ')"
  if [ "$sz" -le 32768 ]; then
    pass "AGENTS.md 体量 $sz B <= 32768（D6 闸门）"
  else
    fail "AGENTS.md 体量 $sz B > 32768（D6 闸门）-> 按「入口文件边界」把单游戏/日志内容移出"
  fi
fi

# 技能层待办可见性（策略 B「触碰即清」，2026-10-15 用户 m00008）:
#   不为清理单开一轮，但要求每个会话开工就看得到剩余量，改到哪个清哪个。
SKILL_TODO="$ROOT/10_docs/_state/技能层待办.md"
if [ -f "$SKILL_TODO" ]; then
  todo_line="$(grep -m1 '^> 剩余待清' "$SKILL_TODO" 2>/dev/null || true)"
  if [ -n "$todo_line" ]; then
    info "技能层待办（策略 B：触碰即清）: ${todo_line#> }"
  else
    warn "技能层待办缺「> 剩余待清：N 个」行（twin_status 用这一行显示进度）"
  fi
fi

# --------------------------------------------------------------------------
# 心跳: 重写本机心跳文件 (只写本机那台, 收工随 git 提交; 对方 pull 后可见本机最近体检)
if [ "$HOST" = mac ] || [ "$HOST" = win ]; then
  HB_DIR="$ROOT/10_docs/_state/机器心跳"
  if mkdir -p "$HB_DIR" 2>/dev/null; then
    hb_head="$(git -C "$ROOT" rev-parse --short HEAD 2>/dev/null || echo unknown)"
    cat > "$HB_DIR/$HOST.md" <<HBEOF
# 机器心跳: ${HOST}（twin_status.sh 每次运行自动重写，勿手改）
- 体检时间: $(date '+%Y-%m-%d %H:%M')
- HEAD: $hb_head
- 结果: PASS=$PASS_N WARN=$WARN_N FAIL=$FAIL_N
- 对方机器看这里: 超 7 天未更新或含 FAIL → 先提醒本机复检，勿默认本机环境正常
HBEOF
    printf '心跳已更新: 10_docs/_state/机器心跳/%s.md (收工时记得随 git 提交)\n' "$HOST"
  fi
fi

# --------------------------------------------------------------------------
# 对方心跳检查 (2026-10-27 第六轮: 真正读取对方心跳; 缺失/过期/含 FAIL 都要判)
peer=""
[ "$HOST" = mac ] && peer=win
[ "$HOST" = win ] && peer=mac
if [ -n "$peer" ]; then
  PEOF="$ROOT/10_docs/_state/机器心跳/${peer}.md"
  if [ ! -f "$PEOF" ]; then
    warn "对方(${peer})心跳缺失: 10_docs/_state/机器心跳/${peer}.md 不存在 ⇒ 不能假定对方环境正常, 先让对方跑 twin_status.sh"
  else
    peer_time="$(grep -m1 '体检时间' "$PEOF" 2>/dev/null | sed 's/.*: //')"
    peer_res="$(grep -m1 '结果' "$PEOF" 2>/dev/null | sed 's/.*: //')"
    if ! printf '%s' "$peer_time" | grep -Eq '^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9]$'; then
      fail "对方(${peer})心跳时间戳无效: '${peer_time}' (格式应为 YYYY-MM-DD HH:MM; 文件可能被手改或格式老化)"
    else
      peer_epoch="$(date -j -f '%Y-%m-%d %H:%M' "$peer_time" '+%s' 2>/dev/null || date -d "$peer_time" '+%s' 2>/dev/null || echo '')"
      if [ -z "$peer_epoch" ]; then
        warn "对方(${peer})心跳时间戳无法转换为时间: '${peer_time}' (本机 date 不支持该格式, 需人工核对)"
      else
        now_epoch="$(date '+%s')"
        age_days=$(( (now_epoch - peer_epoch) / 86400 ))
        if [ "$age_days" -gt 7 ]; then
          warn "对方(${peer})心跳已 ${age_days} 天未更新 (>7 天) ⇒ 先提醒对方复检, 勿默认其环境正常"
        else
          pass "对方(${peer})心跳 ${age_days} 天前, 未过期"
        fi
      fi
    fi
    if [ -z "$peer_res" ]; then
      warn "对方(${peer})心跳缺少「结果」行 ⇒ 无法判断对方上次是否全过"
    else
      peer_fail_n="$(printf '%s' "$peer_res" | sed -n 's/.*FAIL=\([0-9][0-9]*\).*/\1/p')"
      peer_warn_n="$(printf '%s' "$peer_res" | sed -n 's/.*WARN=\([0-9][0-9]*\).*/\1/p')"
      if [ -n "$peer_fail_n" ] && [ "$peer_fail_n" -gt 0 ]; then
        fail "对方(${peer})最近一次体检有 ${peer_fail_n} 项 FAIL: ${peer_res} ⇒ 先让对方修完再开工"
      elif [ -n "$peer_warn_n" ] && [ "$peer_warn_n" -gt 0 ]; then
        warn "对方(${peer})最近一次体检有 ${peer_warn_n} 项 WARN (非 FAIL): ${peer_res}"
      else
        pass "对方(${peer})最近一次体检无 FAIL: ${peer_res}"
      fi
    fi
  fi
fi

# --------------------------------------------------------------------------
printf '\n== 汇总 ==\n'
printf 'PASS=%s  WARN=%s  FAIL=%s\n' "$PASS_N" "$WARN_N" "$FAIL_N"
if [ "$FAIL_N" -gt 0 ]; then
  printf '结论: 有 FAIL, 先修它们\n'
  exit 1
fi
if [ "$WARN_N" -gt 0 ]; then
  printf '结论: 无 FAIL, 有 WARN\n'
  exit 2
fi
printf '结论: 全绿\n'
exit 0
