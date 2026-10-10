#!/usr/bin/env bash
# git_push_main.sh —— 推送到 origin/main 的**传输自适应**实现（P0-5，2026-10-10 追迹迷城复盘落地）
#
# 为什么单独成文件：2026-10-10 实测本机 `git@github.com:22` 与 `ssh.github.com:443` 都超时，
# 而 HTTPS 可达 —— 当时连试 3 次 `git push`、每次盲等 60 s 以上才发现是传输层问题。
# 本函数把「先判传输可达性、不可达立刻改走 HTTPS + gh token」固化成唯一路径。
#
# 用法（由其它脚本 source 后调用）：
#   source "$(dirname "$0")/git_push_main.sh"
#   push_main main          # 参数缺省为 main
#
# 行为与退出码：
#   0   = 已推送（含 HTTPS 绕行；`git push` 报 Everything up-to-date 也算成功）
#   非 0 = **未推送**（本地提交保留；绝不强推，调用方须在汇报里写明 ahead 数量）
#
# 步骤：① `ssh -T git@github.com`（BatchMode + ConnectTimeout=5）判 SSH 是否真能认证，
# 不靠盲等 push 失败；② SSH 可用 → `git push origin <branch>`（不改 remote 配置）；
# ③ SSH 不可用 → 有 `gh auth token` 就一次性 `-c credential.helper=…` 走 HTTPS 推送，
# 成功后 `git fetch <url> <branch>:refs/remotes/origin/<branch>` 刷新跟踪引用
# （URL 推送**不会**更新 refs/remotes/origin/*，不刷新会导致 `git status` 误报 ahead）；
# ④ 无 token → 打印「保留本地提交、未推送」并返回非 0。
push_main() {
  local branch="${1:-main}" rem url tok rc
  if ssh -o BatchMode=yes -o ConnectTimeout=5 -T git@github.com 2>&1 \
       | grep -q "successfully authenticated"; then
    git push origin "$branch"
    return $?
  fi
  echo "[push] git@github.com:22 不可达或不可认证 ⇒ 改走 HTTPS + gh token（不改 remote 配置）"
  tok=""
  if command -v gh >/dev/null 2>&1; then
    tok="$(gh auth token 2>/dev/null || true)"
  fi
  if [[ -z "$tok" ]]; then
    echo "[push] 无可用 gh token ⇒ 保留本地提交、未推送；汇报里写明 ahead 数量，禁止强推"
    return 1
  fi
  rem="$(git remote get-url origin 2>/dev/null || echo 'git@github.com:Aliang369/nixiang.git')"
  url="$(printf '%s' "$rem" | sed -e 's#^git@github.com:#https://github.com/#')"
  git -c credential.helper= -c credential.helper="!f(){ echo username=x-access-token; echo \"password=$tok\"; };f" \
      push "$url" "$branch"
  rc=$?
  if [[ "$rc" -eq 0 ]]; then
    git fetch --quiet "$url" "$branch:refs/remotes/origin/$branch" 2>/dev/null || true
    echo "[push] 已用 HTTPS 绕行推送成功；refs/remotes/origin/$branch 已刷新"
  else
    echo "[push] HTTPS 推送失败（rc=${rc}）；本地提交保留，未强推"
  fi
  return "$rc"
}
