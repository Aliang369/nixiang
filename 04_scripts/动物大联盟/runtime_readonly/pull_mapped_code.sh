#!/usr/bin/env bash
# ================================================================
#  pull_mapped_code.sh — 只拉「maps 已证明是代码」的映射文件（复用现成 root 拉取包装）
#
#  输入：probe_runtime_readonly.sh 产出的 <probe>/code_paths.tsv（maps 派生）
#  行为：按**允许清单**逐项从设备取到本机 disjoint 目录，并记录
#        path / size / sha256 / magic / 映射段。
#
#  ★ 允许清单（硬编码，非参数）—— 只有这些能拉：
#    ① 路径含本包名 com.fightingstars.games.animal
#    ② 扩展名 ∈ {.so,.dex,.vdex,.odex,.oat}
#    ③ 不含 shared_prefs / databases / cache / /files/  /app_webview 等个人内容目录
#  其余一律跳过并打印 SKIP 原因。
#
#  ★ 非目标：不启动应用、不 force-stop、不装机、不注入、不改应用数据。
#    拉取一律经 04_scripts/_common/dev_pull_root.sh（现成包装，自带尺寸校验）。
#
#  用法: bash 04_scripts/动物大联盟/runtime_readonly/pull_mapped_code.sh [--probe DIR] [--dest DIR]
#  退出码: 0=全部 PULL_OK | 4=部分失败 | 5=无可拉项 | 2=用法错
#  bash 3.2 兼容
# ================================================================
set -euo pipefail

SERIAL="${DEV_SERIAL:-f9a116a2}"
PKG="com.fightingstars.games.animal"
PROBE=""
DEST=""

die() { echo "✗ FAIL  $*" >&2; exit 1; }
bad() { echo "✗ 用法  $*" >&2; exit 2; }

while [ $# -gt 0 ]; do
  case "$1" in
    --probe) [ $# -ge 2 ] || bad "--probe 缺值"; PROBE="$2"; shift 2 ;;
    --dest)  [ $# -ge 2 ] || bad "--dest 缺值";  DEST="$2";  shift 2 ;;
    -h|--help) sed -n "2,24p" "$0"; exit 0 ;;
    *) bad "未知参数 $1" ;;
  esac
done

SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SELF_DIR/../../.." && pwd)"
[ -n "$PROBE" ] || PROBE="$ROOT_DIR/06_dumped/动物大联盟/env_1.0.25/probe"
[ -n "$DEST" ] || DEST="$ROOT_DIR/06_dumped/动物大联盟/env_1.0.25/code"
PULLER="$ROOT_DIR/04_scripts/_common/dev_pull_root.sh"
[ -f "$PULLER" ] || die "缺现成拉取包装 $PULLER"
[ -f "$PROBE/code_paths.tsv" ] || die "缺 $PROBE/code_paths.tsv（先跑 probe_runtime_readonly.sh）"
mkdir -p "$DEST/raw"

ALLOW_RE="/com\.fightingstars\.games\.animal"
EXT_RE="\.(so|dex|vdex|odex|oat)$"
DENY_RE="/shared_prefs/|/databases/|/cache/|/files/|/app_webview/|/no_backup/prefs|/code_cache/"

MAN="$DEST/pull_manifest.tsv"
printf "device_path\tlocal_path\tsize_dev\tsize_local\tsha256\tmagic\tsegments\tresult\n" > "$MAN"

OK_N=0; FAIL_N=0; SKIP_N=0

while IFS=$(printf '\t') read -r dpath nseg perms dsize _rest; do
  [ -n "$dpath" ] || continue
  [ "$dpath" = "path" ] && continue
  case "$dpath" in
    *"$PKG"*) : ;;
    *) SKIP_N=$((SKIP_N+1)); continue ;;
  esac
  echo "$dpath" | grep -Eq "$EXT_RE" || { echo "SKIP 非代码扩展: $dpath"; SKIP_N=$((SKIP_N+1)); continue; }
  if echo "$dpath" | grep -Eq "$DENY_RE"; then echo "SKIP 个人内容目录: $dpath"; SKIP_N=$((SKIP_N+1)); continue; fi
  base="$(basename "$dpath")"
  loc="$DEST/raw/$base"
  echo "PULL $dpath -> $loc"
  if bash "$PULLER" "$dpath" "$loc" --serial "$SERIAL" </dev/null >"$DEST/raw/.pull.$base.log" 2>&1; then
    res=PULL_OK; OK_N=$((OK_N+1))
  else
    res=PULL_FAIL; FAIL_N=$((FAIL_N+1))
    echo "  ✗ 拉取失败，日志见 $DEST/raw/.pull.$base.log"
    printf "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n" "$dpath" "-" "$dsize" "-" "-" "-" "$nseg" "$res" >> "$MAN"
    continue
  fi
  lsize=$(wc -c < "$loc" | tr -d " ")
  sha=$(shasum -a 256 "$loc" | awk "{print \$1}")
  magic=$(head -c 4 "$loc" | od -An -tx1 | tr -d " \n")
  printf "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n" "$dpath" "$loc" "$dsize" "$lsize" "$sha" "$magic" "$nseg" "$res" >> "$MAN"
done < "$PROBE/code_paths.tsv"

echo "---- pull_manifest.tsv ----"
cut -f1,3,4,6,8 "$MAN" | sed "s|/data/app/~~[^/]*/|/data/app/<hash>/|; s|/data/data/"$PKG"/|/data/data/<pkg>/|" | head -25
echo "OK=$OK_N FAIL=$FAIL_N SKIP=$SKIP_N"
if [ "$OK_N" -eq 0 ] && [ "$FAIL_N" -eq 0 ]; then exit 5; fi
if [ "$FAIL_N" -gt 0 ]; then exit 4; fi
exit 0
