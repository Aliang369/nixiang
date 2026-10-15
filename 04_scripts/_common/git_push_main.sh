#!/usr/bin/env bash
# git_push_main.sh —— 推送到 origin/<branch> 的**四通道自适应 + 自愈重试**实现
#
# 版本：v2.2（2026-10-15 修 HTTPS 通道退化 + 有界低速阈值）；v2.1（2026-10-11 重写并实测）；
#       v1 = 2026-10-10「单次探测 SSH，失败即永久改走 HTTPS」。
#
# ── v2.2 修的是什么（2026-10-15 实测，用户报「推送总失败」）───────────────────────
#   `origin` 是 **SSH 形态**（`git@github.com:owner/repo.git`）且仓库/全局都没有 `url.insteadOf`
#   映射时，https-proxy / https-direct 两条通道虽然传了 `-c http.proxy=` 与 `-c credential.helper=`，
#   git 仍按 SSH 传输 ⇒ 这两个参数**完全无效**，两条通道退化成「再试一次 SSH」，
#   「四通道自愈」实际只剩 ssh22/ssh443 两条。实测证据：
#     · `push_main --selftest` 四通道全 FAIL，且**错误文本全是 SSH 的**
#       「Please make sure you have the correct access rights and the repository exists.」；
#     · 进程树里 https 通道的子进程仍是 `/usr/bin/ssh … git-upload-pack`；
#     · 后果：三条 SSH 路线都不通时整轮必然失败，且 `archive_scoped_git.sh` 卡在推送阶段 17 分钟。
#   修法＝HTTPS 通道先按 `_pg_https_insteadof_cfg` 把 SSH 形态 origin 改写成 HTTPS（见 ⑧）。
#   同批把 `lowSpeedLimit` 从 0 改为非 0：0 表示**不做低速判定**，`lowSpeedTime` 形同虚设，
#   单条通道可挂 10 分钟；现在默认 1000 B/s × 60 s 即判死，可用 PUSH_HTTP_LOWSPEED_* 覆盖。
#
# ── 为什么要重写：v1 的真实失效模式（2026-10-11 用户已多次报「推送总失败」）────────────
#   · 本机到 github 的通道是**分钟级抖动**：同一次调用里 `ssh -T` 成功、下一分钟 `:22 Operation timed out`；
#     HTTPS 必须过 `HTTPS_PROXY=http://127.0.0.1:7897`，代理节点一挂就是 `SSL_ERROR_SYSCALL`（rc=35，
#     5 秒内秒失败）或 `curl 28 Operation too slow`（被 lowSpeedLimit=1000/lowSpeedTime=60 掐断）；
#     直连 HTTPS 曾经是 `Connection reset by peer`。
#   · v1 只在**开头探一次** SSH，失败就把整轮永久交给 HTTPS+代理 ⇒ 代理一挂必然全败；
#     重试只能靠人工临时写 /tmp/retry_push*.sh（本会话写了 3 个，仍反复失败）。
#
# ── v2.1 的做法 ───────────────────────────────────────────────────────────────
#   ① **四通道按序试**：ssh22 | ssh443(`-p 443 -o Hostname=ssh.github.com`) | https-proxy | https-direct
#   ② **探测 == 真实操作**：每条通道用 `git ls-remote --heads origin <branch>` 做探测，
#      它与 `git push` 走**完全相同的认证与 DNS/代理路径** ⇒ 不会再出现「探测通过但 push 失败」的错配
#      （v2.0 用 `ssh -T` 探测就踩过这个坑）。探测失败会把**真实错误尾行**打出来。
#   ③ **一次调用内自带重试**：`--attempts`（默认 6）× `--wait`（默认 20s），抖动期内不必人工再喊。
#   ④ 一律 `git -c … push origin <branch>`（**不再 URL 推**）⇒ 跟踪引用天然刷新；
#      传输层耐慢参数 HTTP/1.1 + 大 postBuffer + **有界**低速阈值（`lowSpeedLimit` 必须非 0，
#      否则 `lowSpeedTime` 形同虚设）；SSH 侧 `ConnectTimeout=8 / ServerAliveInterval=10 /
#      ServerAliveCountMax=3`。
#   ⑧ **HTTPS 通道先把 SSH 形态的 origin 改写成 HTTPS**（`_pg_https_insteadof_cfg`）：
#      少了这一步，`-c http.proxy` / `-c credential.helper` 对 SSH 传输完全无效，
#      https-proxy / https-direct 会退化成「再试一次 SSH」⇒ 四通道实际只剩两条（见头部 v2.2 说明）。
#   ⑤ **分叉短路**：被 `Updates were rejected` / `non-fast-forward` 时立刻停手、打印两端 tip、
#      返回 3 —— 这不是网络问题，必须先 fetch/rebase，**绝不 force**（本会话 amend 已推提交踩过）。
#   ⑥ **无需联网也不会误报**：本地 ahead=0 时直接 `PUSH_RESULT=NOTHING_TO_PUSH` rc=0。
#   ⑦ 可选**本地镜像**（离线第二落点）：`PUSH_MIRROR_DIR=/path/to/bare.git` 时同轮把提交推到本地裸仓
#      （放 Syncthing/Resilio 同步目录即可实现双机搬运，网络全挂也不丢提交）。
#
# 用法（其它脚本 source 后调用）：
#   source "$(dirname "$0")/git_push_main.sh"
#   push_main main                      # 缺省 main
#   push_main main --attempts 8 --wait 15
#   push_main --selftest                # 只体检四条通道并逐条打印真实错误，不做推送
#
# 环境变量：PUSH_MAX_ATTEMPTS(6) / PUSH_WAIT_SECS(20) / PUSH_ROUTES(通道白名单) /
#           PUSH_MIRROR_DIR(本地裸仓) / PUSH_QUIET(1=少说话)
#
# 退出码：0 = 已推送（含 up-to-date / 仅落本地镜像）   1 = 未推送（本地提交保留，绝不强推）
#         2 = 用法/参数错误                            3 = 远端分叉（需 fetch/rebase，不是网络问题）
_pg_log() { [ "${PUSH_QUIET:-0}" = "1" ] || echo "[push] $*"; }

# 所有 git 调用共用的传输层参数（耐慢 + HTTP/1.1 抗代理抖动）。
# 低速阈值必须**非 0**：`lowSpeedLimit=0` = 不做低速判定（`lowSpeedTime` 随之失效），
# 代理一挂就会把单条通道挂满（v2.1 曾设 0/600 ⇒ 实测单轮卡 17 分钟）。默认 1000 B/s × 60 s。
_PG_LOWSPEED_LIMIT="${PUSH_HTTP_LOWSPEED_LIMIT:-1000}"
_PG_LOWSPEED_TIME="${PUSH_HTTP_LOWSPEED_TIME:-60}"
_PG_GIT_OPTS=( -c http.version=HTTP/1.1 \
               -c "http.lowSpeedLimit=${_PG_LOWSPEED_LIMIT}" \
               -c "http.lowSpeedTime=${_PG_LOWSPEED_TIME}" \
               -c http.postBuffer=524288000 )
_PG_SSH_OPTS="-o BatchMode=yes -o ConnectTimeout=8 -o ServerAliveInterval=10 -o ServerAliveCountMax=3"
_PG_SSH22=""
_PG_SSH443="-p 443 -o Hostname=ssh.github.com"

# gh token（HTTPS 通道用；只取一次并缓存）
_pg_token() {
  if [ -z "${_PG_TOKEN:-}" ]; then
    if command -v gh >/dev/null 2>&1; then _PG_TOKEN="$(gh auth token 2>/dev/null || true)"; fi
  fi
  printf '%s' "${_PG_TOKEN:-}"
}

# origin 是 SSH 形态时，HTTPS 通道必须把它改写成 HTTPS —— 否则 `-c http.proxy` /
# `-c credential.helper` 对 SSH 传输完全无效，本通道退化成「再试一次 SSH」（见头部 v2.2）。
# $1 = 远端 URL（显式传入以便单测，不传则空 ⇒ 输出空）；stdout = 单个 git `-c` 值，无需改写时为空。
_pg_https_insteadof_cfg() {
  local url="${1:-}" host path
  case "$url" in
    git@*:*)     host="${url#git@}"; host="${host%%:*}"; path="${url#*:}" ;;
    ssh://git@*) path="${url#ssh://git@}"; host="${path%%/*}"; path="${path#*/}" ;;
    *) return 0 ;;
  esac
  [ -n "$host" ] && [ -n "$path" ] || return 0
  printf 'url.https://%s/%s.insteadOf=%s' "$host" "$path" "$url"
}

# 当前 origin 的改写配置（origin 读不到 / 已是 HTTPS 时输出空）。
_pg_origin_insteadof_cfg() {
  _pg_https_insteadof_cfg "$(git remote get-url origin 2>/dev/null || true)"
}

# 统一执行器：跑一条命令，**只把 stderr 收进 $_PG_LAST_ERR**（push/ls-remote 的正常进度都走 stderr，
# 故 err 非空 ≠ 失败；成败一律看返回码），并原样把返回码传出去。
_pg_exec() {
  local out rc
  out="$("$@" 2>&1 >/dev/null)"; rc=$?
  _PG_LAST_ERR="$(printf '%s' "$out" | grep -v '^$' | tail -2 | tr '\n' ' ')"
  return "$rc"
}

# 统一的「跑一条通道」：$1=route $2=branch $3=probe|push
# probe 用 `git ls-remote --heads origin <branch>` —— 与 push 走**完全相同**的认证/DNS/代理路径。
_pg_run_route() {
  local route="$1" branch="$2" mode="$3" proxy helper
  _PG_LAST_ERR=""
  case "$route" in
    ssh22|ssh443)
      local ssho="$_PG_SSH22"
      [ "$route" = "ssh443" ] && ssho="$_PG_SSH443"
      if [ "$mode" = "probe" ]; then
        _pg_exec git "${_PG_GIT_OPTS[@]}" -c core.sshCommand="ssh $ssho $_PG_SSH_OPTS" \
          ls-remote --heads origin "$branch"
      else
        _pg_exec git "${_PG_GIT_OPTS[@]}" -c core.sshCommand="ssh $ssho $_PG_SSH_OPTS" \
          push origin "$branch"
      fi
      return $?;;
    https-proxy|https-direct)
      proxy="${HTTPS_PROXY:-${https_proxy:-http://127.0.0.1:7897}}"
      [ "$route" = "https-direct" ] && proxy=""
      helper=""
      if [ -n "$(_pg_token)" ]; then
        helper='!f(){ echo username=x-access-token; echo \"password='"$(_pg_token)"'\"; };f'
      fi
      # SSH 形态的 origin 必须改写成 HTTPS，否则本通道的代理/凭证参数对 SSH 传输无效（见头部 v2.2）。
      # `rw` 可能为空数组 ⇒ 一律用 ${rw[@]+"${rw[@]}"} 展开（bash 3.2 + set -u 下裸展开会报错）。
      local rw=() cfg
      cfg="$(_pg_origin_insteadof_cfg)"
      [ -n "$cfg" ] && rw=( -c "$cfg" )
      if [ -n "$proxy" ]; then
        if [ "$mode" = "probe" ]; then
          _pg_exec git "${_PG_GIT_OPTS[@]}" ${rw[@]+"${rw[@]}"} -c http.proxy="$proxy" \
            -c credential.helper= -c credential.helper="$helper" ls-remote --heads origin "$branch"
        else
          _pg_exec git "${_PG_GIT_OPTS[@]}" ${rw[@]+"${rw[@]}"} -c http.proxy="$proxy" \
            -c credential.helper= -c credential.helper="$helper" push origin "$branch"
        fi
      else
        if [ "$mode" = "probe" ]; then
          _pg_exec env -u HTTPS_PROXY -u https_proxy -u HTTP_PROXY -u http_proxy -u ALL_PROXY -u all_proxy \
            git "${_PG_GIT_OPTS[@]}" ${rw[@]+"${rw[@]}"} -c http.proxy= \
            -c credential.helper= -c credential.helper="$helper" \
            ls-remote --heads origin "$branch"
        else
          _pg_exec env -u HTTPS_PROXY -u https_proxy -u HTTP_PROXY -u http_proxy -u ALL_PROXY -u all_proxy \
            git "${_PG_GIT_OPTS[@]}" ${rw[@]+"${rw[@]}"} -c http.proxy= \
            -c credential.helper= -c credential.helper="$helper" \
            push origin "$branch"
        fi
      fi
      return $?;;
    *) _PG_LAST_ERR="未知通道 $route"; return 1;;
  esac
}

# 未配置镜像时必须 return 1（不能 0）：否则调用方把「没配镜像」当成「镜像已落盘」。
_pg_mirror_push() {  # $1 = 分支
  [ -n "${PUSH_MIRROR_DIR:-}" ] || return 1
  if git push "$PUSH_MIRROR_DIR" "$1" >/dev/null 2>&1; then
    _pg_log "MIRROR_OK dir=${PUSH_MIRROR_DIR}（不依赖网络，提交已落盘）"
    return 0
  fi
  _pg_log "MIRROR_FAIL dir=${PUSH_MIRROR_DIR}（检查该路径是否裸仓且可写）"
  return 1
}

# 本地相对跟踪引用的待推提交数（0 = 没东西要推；-1 = 算不出来）
_pg_pending_count() {  # $1 = 分支
  git rev-list --count "origin/$1..$1" 2>/dev/null || echo "-1"
}

_pg_usage() {
  echo "[push] 用法：push_main [<branch>] [--attempts N] [--wait S] [--quiet] [--selftest]"
  echo "[push] 环境：PUSH_MAX_ATTEMPTS(6) PUSH_WAIT_SECS(20) PUSH_ROUTES(白名单) PUSH_MIRROR_DIR(本地裸仓)"
  echo "[push]       PUSH_HTTP_LOWSPEED_LIMIT(1000) PUSH_HTTP_LOWSPEED_TIME(60)"
}

# 通道体检：逐条真实探测并打印错误（诊断用，read-only，不推送）
push_selftest() {
  local branch="main" route ok=0 cfg
  [ $# -ge 1 ] && branch="$1"
  echo "[push] === 四通道体检（ls-remote，read-only）==="
  echo "[push] origin=$(git remote get-url origin 2>/dev/null || echo '<未知>')"
  cfg="$(_pg_origin_insteadof_cfg)"
  if [ -n "$cfg" ]; then
    echo "[push] HTTPS 通道改写：${cfg}"
    echo "[push]   （origin 是 SSH 形态；没这一行时 https-* 通道会退化成「再试一次 SSH」）"
  else
    echo "[push] HTTPS 通道改写：无需（origin 已是 HTTPS 形态）"
  fi
  for route in $(printf '%s' "${PUSH_ROUTES:-ssh22,ssh443,https-proxy,https-direct}" | tr ',' ' '); do
    if _pg_run_route "$route" "$branch" probe; then
      echo "[push]   $route  OK"; ok=$((ok + 1))
    else
      echo "[push]   $route  FAIL  ${_PG_LAST_ERR:-（无错误输出）}"
    fi
  done
  echo "[push] 可用通道 $ok/4"
  [ "$ok" -gt 0 ] || return 1
  return 0
}

push_main() {
  local branch="main" attempts="${PUSH_MAX_ATTEMPTS:-6}" wait_s="${PUSH_WAIT_SECS:-20}" i=0
  local routes route tried first_err ahead pending mirror_ok=0 selftest=0
  while [ $# -gt 0 ]; do
    case "$1" in
      --attempts) [ $# -ge 2 ] || { echo "[push] --attempts 缺少数值"; _pg_usage; return 2; }
                  attempts="$2"; shift 2;;
      --wait)     [ $# -ge 2 ] || { echo "[push] --wait 缺少秒数"; _pg_usage; return 2; }
                  wait_s="$2"; shift 2;;
      --quiet)    PUSH_QUIET=1; shift;;
      --selftest) selftest=1; shift;;
      -*)         echo "[push] 未知参数：$1"; _pg_usage; return 2;;
      *)          branch="$1"; shift;;
    esac
  done
  routes="${PUSH_ROUTES:-ssh22,ssh443,https-proxy,https-direct}"
  if [ "$selftest" = "1" ]; then push_selftest "$branch" && return 0 || return 1; fi

  _pg_mirror_push "$branch" && mirror_ok=1

  # 没东西要推时不要因为「网络全挂」报 FAIL：ahead=0 即目标已达成。
  pending="$(_pg_pending_count "$branch")"
  if [ "$pending" = "0" ]; then
    _pg_log "PUSH_RESULT=NOTHING_TO_PUSH（本地无新提交，ahead 0，无需联网）"
    return 0
  fi

  while [ "$i" -lt "$attempts" ]; do
    i=$((i + 1)); tried=""
    for route in $(printf '%s' "$routes" | tr ',' ' '); do
      if _pg_run_route "$route" "$branch" probe; then
        _pg_log "第 $i/$attempts 轮 route=$route 探测通过 ⇒ 推送"
        if _pg_run_route "$route" "$branch" push; then
          _pg_log "PUSH_RESULT=OK route=$route attempts=$i"
          return 0
        fi
        _pg_log "第 $i/$attempts 轮 route=$route push 失败：${_PG_LAST_ERR:-（无错误输出）}"
      else
        _pg_log "第 $i/$attempts 轮 route=$route 探测失败：${_PG_LAST_ERR:-（无错误输出）}"
      fi
      # 分叉短路：不是网络问题，换通道只会重复被拒
      case "${_PG_LAST_ERR:-}" in
        *"Updates were rejected"*|*"non-fast-forward"*)
          echo "[push] ✗ 远端与本地**历史分叉**（非网络问题）⇒ 先 fetch/rebase，禁止强推"
          echo "[push] 远端 $branch tip = $(git ls-remote origin "$branch" 2>/dev/null | awk '{print $1}')"
          echo "[push] 本地 $branch tip = $(git rev-parse --short HEAD)"
          echo "[push] PUSH_RESULT=DIVERGED"; return 3;;
      esac
      tried="${tried}${tried:+,}$route"
      if [ -z "$first_err" ]; then first_err="${_PG_LAST_ERR:-$route 无错误输出}"; fi
    done
    ahead="$(_pg_pending_count "$branch")"
    if [ "$ahead" = "0" ]; then _pg_log "PUSH_RESULT=OK（提交已在远端，ahead 0）"; return 0; fi
    if [ "$i" -lt "$attempts" ]; then
      _pg_log "第 $i/$attempts 轮四通道未成功（首错：${first_err}）⇒ ${wait_s}s 后重试"
      sleep "$wait_s"
    fi
  done
  echo "[push] ✗ 未推送：本地 ahead ${ahead}，已试通道=${tried}，首错=${first_err}"
  if [ "$mirror_ok" = "1" ]; then echo "[push] PUSH_RESULT=FAIL_REMOTE_MIRROR_OK（远端未推，但本地镜像已落盘）"
  else echo "[push] PUSH_RESULT=FAIL"; fi
  return 1
}
