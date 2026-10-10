#!/usr/bin/env bash
# 安全隔离清理：默认只预演；--apply 才把显式路径移动到 macOS Trash 下的隔离目录。
# 不处理 02_apks/ 与 08_output/，避免误触用户手动管理的输入和正式包。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MODE="dry-run"
TAG="cleanup"
MANIFEST=""
PATHS=()
SUGGEST=0
GAME=""
GAME_ID=""
ALLOW_TRACKED=0

usage() {
  cat >&2 <<'EOF'
用法:
  bash 04_scripts/_common/trash_paths.sh [--apply] [--tag 名称] [--manifest 文件] \
    --path 03_decompiled/{游戏} [--path ...]

说明:
  - 默认 dry-run，只列出路径、大小和目标隔离位置。
  - --apply 后移动到 ~/.Trash/nixiang_<tag>_<时间>/<仓库相对路径>。
  - 强制拒绝仓库根、.git、01_tools、02_apks、08_output。
  - 例外：01_tools 下工具自身的构建/运行输出按 TOOLS_REGEN_ALLOW 白名单放行（规则见 TOOLBOOK.md）。
  - --manifest 可指定 JSON 清单；apply 且未指定时写入隔离目录内的 manifest.json。
  - apply 前会打印同样的 dry-run 清单并要求路径显式传入。
  - --suggest --game <游戏> [--id <游戏id>]：只打印该游戏可清理路径的草案（--path 行，可直接拼进命令行），
    自动排除仓库根/.git/01_tools/02_apks/08_output，并自动排除「含已入库(tracked)文件」的路径。
  - 硬门禁（P0-4，2026-10-10）：任何路径若含 tracked 文件，dry-run 即拒绝（历史事故：清理清单压到
    tracked 的 rva_targets.json，整目录被移走）；确需清理时用 --allow-tracked 显式放行并在汇报里写明。
EOF
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apply)
      MODE="apply"
      shift
      ;;
    --dry-run)
      MODE="dry-run"
      shift
      ;;
    --tag)
      [[ $# -ge 2 ]] || usage
      TAG="$2"
      shift 2
      ;;
    --manifest)
      [[ $# -ge 2 ]] || usage
      MANIFEST="$2"
      shift 2
      ;;
    --path)
      [[ $# -ge 2 ]] || usage
      PATHS+=("$2")
      shift 2
      ;;
    --suggest)
      SUGGEST=1
      shift
      ;;
    --game)
      [[ $# -ge 2 ]] || usage
      GAME="$2"
      shift 2
      ;;
    --id)
      [[ $# -ge 2 ]] || usage
      GAME_ID="$2"
      shift 2
      ;;
    --allow-tracked)
      ALLOW_TRACKED=1
      shift
      ;;
    -h|--help)
      usage
      ;;
    *)
      usage
      ;;
  esac
done

# --suggest 模式不需要 --path（真正的分派在函数定义之后，见文件下半部）
[[ "$SUGGEST" == "1" || ${#PATHS[@]} -gt 0 ]] || usage
[[ -n "$TAG" ]] || TAG="cleanup"
TAG="$(printf '%s' "$TAG" | tr -c 'A-Za-z0-9._-' '_' | sed 's/_*$//')"
[[ -n "$TAG" ]] || TAG="cleanup"

# P0-4：tracked 门禁。清单里的路径若包含已入库文件，一律拒绝（--allow-tracked 可显式放行）。
# 判据用 `git ls-files -- <rel>`（目录会列出其下全部入库文件），不用 basename 猜测。
tracked_under() {   # $1 = 仓库相对路径；stdout = 命中的 tracked 文件（最多 5 条）
  git -C "$ROOT" ls-files -- "$1" 2>/dev/null | head -5 || true
}

# P0-4：--suggest 草案生成（只打印，不动文件）
suggest_paths() {
  local game="$1" rel child t ct any skipped id
  [[ -n "$game" ]] || { echo "ERROR: --suggest 需要 --game <游戏>" >&2; exit 2; }
  # 游戏 id（zjmc 之类）：优先 --id，其次取 04_scripts/<游戏>/pipeline.json 的 params.game_id
  id="${GAME_ID:-}"
  if [[ -z "$id" && -f "$ROOT/04_scripts/$game/pipeline.json" ]]; then
    id="$(grep -o '"game_id"[[:space:]]*:[[:space:]]*"[^"]*"' "$ROOT/04_scripts/$game/pipeline.json" 2>/dev/null | head -1 | sed 's/.*"\([^"]*\)"$/\1/')"
  fi
  echo "[trash_paths] --suggest game=${game} id=${id:-<未知>}（已排除含 tracked 文件的路径与禁清目录）"
  for parent in 03_decompiled 06_dumped 07_patched 09_test; do
    for rel in "$parent/$game" "$parent/${game}"_*; do
      [[ -e "$ROOT/$rel" ]] || continue
      is_banned_rel "$rel" && ! is_tools_regen_allow "$rel" && continue
      t="$(tracked_under "$rel")"
      if [[ -z "$t" ]]; then
        printf -- '--path %s\n' "$rel"
        continue
      fi
      any=0
      skipped=""
      while IFS= read -r child; do
        [[ -n "$child" && -d "$ROOT/$child" ]] || continue
        ct="$(git -C "$ROOT" ls-files -- "$child" 2>/dev/null | head -1 || true)"
        if [[ -z "$ct" ]]; then
          printf -- '--path %s\n' "$child"
          any=1
        else
          skipped="${skipped}${child} "
        fi
      done < <(cd "$ROOT" && find "$rel" -mindepth 1 -maxdepth 1 -type d 2>/dev/null | sort)
      [[ -n "$skipped" ]] && printf '[note] 已排除（含 tracked 文件）：%s\n' "$skipped" >&2
      [[ "$any" == "0" ]] && printf '[note] %s 整体未列入（含 tracked 文件，例：%s）\n' "$rel" "$(printf '%s' "$t" | head -1)" >&2
    done
  done
  for rel in "mod菜单/starry-mod-menu/app/build" "mod菜单/starry-mod-menu/build" "mod菜单/starry-mod-menu/.gradle" "mod菜单/starry-mod-menu/.gradle-home"; do
    [[ -e "$ROOT/$rel" ]] || continue
    [[ -z "$(tracked_under "$rel")" ]] && printf -- '--path %s\n' "$rel"
  done
  if [[ -n "$id" ]]; then
    while IFS= read -r rel; do
      [[ -n "$rel" ]] && printf -- '--path %s\n' "$rel"
    done < <(cd "$ROOT" && find mod菜单 -maxdepth 6 -type d \( -name "obj_${id}" -o -name "libs_${id}" \) 2>/dev/null | sort)
  else
    printf '[note] 未识别 <游戏id> ⇒ 未列出 mod菜单 的 obj_<id>/libs_<id> 编译产物（可用 --id 指定）\n' >&2
  fi
  exit 0
}

STAMP="$(date '+%Y%m%d_%H%M%S')"
TRASH_PARENT="${NIXIANG_TRASH_DIR:-$HOME/.Trash}"
QUARANTINE="$TRASH_PARENT/nixiang_${TAG}_${STAMP}"

is_banned_rel() {
  case "$1" in
    ""|.|.git|.git/*|01_tools|01_tools/*|02_apks|02_apks/*|08_output|08_output/*)
      return 0
      ;;
  esac
  return 1
}

# 01_tools 可再生输出白名单（2026-09-30 定；规则见 TOOLBOOK.md「01_tools 可再生输出」）。
# 只放行工具自身的「构建产物」与「运行输出」，其余 01_tools 内容仍硬拒绝；不接受通配，新增须显式列入。
TOOLS_REGEN_ALLOW=(
  "01_tools/Il2CppDumper/Il2CppDumper/bin"
  "01_tools/Il2CppDumper/Il2CppDumper/obj"
  "01_tools/Il2CppDumper/dump.cs"
  "01_tools/Il2CppDumper/script.json"
  "01_tools/Il2CppDumper/il2cpp.h"
  "01_tools/Il2CppDumper/stringliteral.json"
  "01_tools/Il2CppDumper/DummyDll"
  # Cpp2IL 源码 clone 内的上游测试素材（未被本地改动、可由 clone 自身 git 恢复）
  "01_tools/Cpp2IL_src/TestFiles"
)
is_tools_regen_allow() {
  local p="$1" a
  for a in "${TOOLS_REGEN_ALLOW[@]}"; do
    [[ "$p" == "$a" ]] && return 0
  done
  return 1
}

[[ "$SUGGEST" == "1" ]] && suggest_paths "$GAME"
TMP="$(mktemp /tmp/trash_paths.XXXXXX)"
trap 'rm -f "$TMP"' EXIT

for input in "${PATHS[@]}"; do
  if [[ "$input" == /* ]]; then
    abs="$input"
  else
    abs="$ROOT/$input"
  fi
  [[ -e "$abs" || -L "$abs" ]] || { echo "ERROR: 路径不存在: $abs" >&2; exit 1; }

  parent="$(cd "$(dirname "$abs")" && pwd -P)"
  abs="$parent/$(basename "$abs")"
  case "$abs" in
    "$ROOT"/*) ;;
    *) echo "ERROR: 路径不在工作区: $abs" >&2; exit 1 ;;
  esac
  rel="${abs#"$ROOT"/}"
  if is_banned_rel "$rel" && ! is_tools_regen_allow "$rel"; then
    echo "ERROR: 安全禁止清理: $rel" >&2; exit 1
  fi

  tracked_hit="$(tracked_under "$rel")"
  if [[ -n "$tracked_hit" ]]; then
    if [[ "$ALLOW_TRACKED" == "1" ]]; then
      echo "[trash_paths] WARNING: --allow-tracked 放行含 tracked 文件的路径: $rel" >&2
      printf '%s\n' "$tracked_hit" | sed 's/^/        tracked: /' >&2
    else
      echo "ERROR: 路径含已入库(tracked)文件，禁止清理: $rel" >&2
      printf '%s\n' "$tracked_hit" | sed 's/^/        tracked: /' >&2
      echo "        正解：从清单里剔除该文件/该目录；确需清理用 --allow-tracked 并在汇报里写明。" >&2
      exit 1
    fi
  fi

  if [[ -d "$abs" && ! -L "$abs" ]]; then
    size_kib="$(du -sk "$abs" | awk '{print $1}')"
    kind="dir"
  else
    size_kib="$(du -k "$abs" | awk '{print $1}')"
    kind="file"
  fi
  dest="$QUARANTINE/$rel"
  printf '%s\t%s\t%s\t%s\t%s\n' "$rel" "$kind" "$size_kib" "$abs" "$dest" >> "$TMP"
done

echo "[trash_paths] mode=$MODE quarantine=$QUARANTINE"
while IFS=$'\t' read -r rel kind size_kib abs dest; do
  echo "  $rel  type=$kind size=${size_kib}KiB  -> $dest"
done < "$TMP"

if [[ "$MODE" != "apply" ]]; then
  echo "[trash_paths] dry-run，未移动任何文件；确认后加 --apply。"
  exit 0
fi

mkdir -p "$QUARANTINE"
[[ -n "$MANIFEST" ]] || MANIFEST="$QUARANTINE/manifest.json"
if [[ "$MANIFEST" != /* ]]; then
  MANIFEST="$ROOT/$MANIFEST"
fi

while IFS=$'\t' read -r rel kind size_kib abs dest; do
  mkdir -p "$(dirname "$dest")"
  mv -- "$abs" "$dest"
  echo "[trash_paths] moved: $rel"
done < "$TMP"

# Windows 上没有 python3.11 这个名字（只有 python.exe / py），逐个探测；与
# 04_scripts/_common/finalize_game_delivery.sh 的 PYTHON 探测保持同一姿势。
PY="${PYTHON:-}"
if [[ -z "$PY" ]]; then
  for c in python3.11 python3 python py; do
    command -v "$c" >/dev/null 2>&1 && { PY="$c"; break; }
  done
fi
[[ -n "$PY" ]] || { echo "ERROR: 找不到 python，无法写 manifest（可设 PYTHON=/path/to/python）" >&2; exit 1; }

# ⚠️ 原生 Windows python 认不得 POSIX 路径：`/tmp/trash_paths.XXXXXX` 会被当成 `\tmp\…`（当前盘根），
#    直接抛 FileNotFoundError: No such file or directory: '\tmp\trash_paths.XXXXXX'；manifest 同理。
#    故只对**真正做文件 I/O 的两个参数**做 cygpath 转换，JSON 里记录的 root/quarantine 仍保留
#    POSIX 形式（与历史清单一致，便于人读）。同一姿势见 finalize_game_delivery.sh 的 win_path()。
win_path() {
  if command -v cygpath >/dev/null 2>&1; then
    cygpath -m "$1" 2>/dev/null || printf '%s' "$1"
  else
    printf '%s' "$1"
  fi
}
MANIFEST_WIN="$(win_path "$MANIFEST")"
TMP_WIN="$(win_path "$TMP")"

"$PY" - "$MANIFEST_WIN" "$ROOT" "$QUARANTINE" "$STAMP" "$TMP_WIN" <<'PY'
import json
import sys
from pathlib import Path

manifest, root, quarantine, stamp, listing = sys.argv[1:]
items = []
for line in Path(listing).read_text(encoding="utf-8").splitlines():
    if not line.strip():
        continue
    rel, kind, size_kib, source, destination = line.split("\t")
    items.append({
        "path": rel,
        "type": kind,
        "size_kib": int(size_kib),
        "source": source,
        "quarantine": destination,
    })
data = {
    "root": root,
    "quarantine": quarantine,
    "timestamp": stamp,
    "mode": "apply",
    "items": items,
}
Path(manifest).parent.mkdir(parents=True, exist_ok=True)
Path(manifest).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(f"[trash_paths] manifest={manifest}")
PY

echo "[trash_paths] apply 完成；恢复时将 quarantine 内对应路径移回原位置。"
