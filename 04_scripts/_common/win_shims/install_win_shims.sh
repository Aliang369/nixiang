#!/usr/bin/env bash
# ==============================================================================
#  win_shims/install_win_shims.sh — 把 python3.11 垫片装到机器 PATH 上（仅 Windows/Git Bash）
# ==============================================================================
#  为什么还需要这一步：
#    env.sh 只在脚本 source 它时才把垫片目录前置进 PATH。本仓 43 个含
#    `python3.11` 的脚本里，只有 1 个（env.sh 自己）source 了 env.sh —— 其余
#    全都不 source。所以必须再往机器 PATH 上的一个目录放一份。
#
#  装法：目标目录里放一个「转发壳」（不是复制，也不是符号链接）：
#      #!/usr/bin/env bash
#      exec "<仓库>/04_scripts/_common/win_shims/python3.11" "$@"
#  这样以后改仓库里的垫片立刻生效，不用重装；也避免 Windows 上 ln -s 需要
#  开发者模式的问题。
#
#  用法：
#    bash 04_scripts/_common/win_shims/install_win_shims.sh            # 安装/刷新
#    bash 04_scripts/_common/win_shims/install_win_shims.sh --check    # 只体检
#    NIXIANG_WIN_BIN=/c/some/bin bash .../install_win_shims.sh         # 指定目录
#
#  mac 上跑会直接拒绝（本脚本只服务 Windows）。
# ==============================================================================
set -uo pipefail

_here="$(cd "$(dirname "$0")" && pwd)"
REPO="$(cd "$_here/../../.." && pwd)"
SHIM="$_here/python3.11"
MODE="${1:-install}"

if [ ! -f "$SHIM" ]; then
  echo "ERROR: 找不到垫片本体: $SHIM" >&2
  exit 1
fi

# ---- 目标目录：优先 $NIXIANG_WIN_BIN，其次 PATH 上已存在的 $HOME/.local/bin ----
_pick_bin() {
  if [ -n "${NIXIANG_WIN_BIN:-}" ]; then printf '%s\n' "$NIXIANG_WIN_BIN"; return 0; fi
  for d in "$HOME/.local/bin" "$HOME/bin" "$HOME/AppData/Local/Microsoft/WindowsApps"; do
    [ -d "$d" ] || continue
    case ":$PATH:" in *":$d:"*) printf '%s\n' "$d"; return 0 ;; esac
  done
  printf '%s\n' "$HOME/.local/bin"
}
BIN="$(_pick_bin)"

echo "[shims] 仓库     : $REPO"
echo "[shims] 垫片本体 : $SHIM"
echo "[shims] 安装目标 : $BIN"

# ---- 体检 ----
echo
echo "===== --check: 当前状态 ====="
_cmd="$(command -v python3.11 2>/dev/null || true)"
if [ -n "$_cmd" ]; then echo "  command -v python3.11 -> $_cmd"; else echo "  command -v python3.11 -> (none) ← 垫片还没上 PATH"; fi
case ":$PATH:" in
  *":$BIN:"*) echo "  $BIN 在 PATH 上 ✅" ;;
  *)          echo "  $BIN 不在 PATH 上 ⚠（装进去也不会被解析到，需要手动加 PATH）" ;;
esac

if [ "$MODE" = "--check" ] || [ "$MODE" = "check" ]; then
  echo
  echo "[shims] --check 结束，未改动任何文件。"
  exit 0
fi

# ---- 拒 mac ----
case "$(uname -s)" in
  Darwin)
    echo
    echo "本脚本只服务 Windows。mac 上 python3.11 由 brew 提供，不需要垫片。" >&2
    exit 1 ;;
esac

# ---- 安装（转发壳）----
mkdir -p "$BIN" || { echo "ERROR: 无法创建 $BIN" >&2; exit 1; }
cat > "$BIN/python3.11" <<EOF
#!/usr/bin/env bash
# 自动生成 by 04_scripts/_common/win_shims/install_win_shims.sh —— 不要手改。
# 真正的逻辑在仓库里，这里只转发，所以改垫片立刻生效。
exec "$SHIM" "\$@"
EOF
chmod +x "$BIN/python3.11" 2>/dev/null || true
echo
echo "[shims] 已写入: $BIN/python3.11"

# ---- 自证 ----
echo
echo "===== 自证 ====="
hash -r 2>/dev/null || true
echo "  command -v python3.11 -> $(command -v python3.11 2>/dev/null || echo '(仍为 none)')"
echo "  NIXIANG_SHIM_INFO=1 python3.11 :"
NIXIANG_SHIM_INFO=1 "$BIN/python3.11" 2>&1 | sed 's/^/    /'
echo
echo "  真实调用冒烟（stdin 必须透传）："
printf 'import sys\nprint("stdin-ok", sys.version_info[:3])\n' | python3.11 - 2>&1 | sed 's/^/    /'
echo "  argv 转发冒烟："
python3.11 -c 'import sys;print("argv-ok", sys.argv[1:])' a b 2>&1 | sed 's/^/    /'
echo "  退出码转发冒烟（期望 3）："
python3.11 -c 'import sys;sys.exit(3)' 2>/dev/null; echo "    rc=$?"
echo
echo "[shims] 完成。若上面 command -v 仍为 none，说明 $BIN 不在 PATH 上。"
