#!/usr/bin/env bash
# 盘外产物普查（C/D/E，工作区 E:\nixiang 之外）：按关键词与扩展名找可疑产物，只列不删
# 兼容 bash 3.2 / macOS（无 GNU timeout ⇒ 不用 timeout，靠 maxdepth + head 限流）
set -uo pipefail
OUT="/e/nixiang/09_test/盘外产物普查_20261028.txt"
: > "$OUT"

log() { printf '%s\n' "$*" >> "$OUT"; }

scan_root() {
  local root="$1" depth="$2"
  log ""
  log "---- ROOT $root (maxdepth=$depth) ----"
  if [ ! -d "$root" ]; then
    log "  (不存在)"
    return
  fi
  find "$root" -maxdepth "$depth" \( -path '/e/nixiang' -prune \) -o \
    \( -newermt '2026-06-01' \( \
        -iname '*cstjjs*' -o -iname '*长生*' -o -iname '*天机*' -o -iname '*netkuu*' \
        -o -iname '*starry*' -o -iname '*nixiang*' -o -iname '*星空*' \
        -o -iname 'menu_cstjjs*' -o -iname '*.idsig' \) \) -print 2>/dev/null | head -400 >> "$OUT"
}

log "==== 盘外产物普查 $(date '+%Y-%m-%d %H:%M')（只读枚举，不含删除）===="
log "工作区=E:\\nixiang（本表已排除其内部）"

log ""
log "==== 1) 用户目录（桌面/下载/文档/临时/漫游）===="
for d in /c/Users/Administrator/Desktop /c/Users/Administrator/Downloads /c/Users/Administrator/Documents; do
  scan_root "$d" 6
done
scan_root /c/Users/Administrator/AppData/Local/Temp 6
scan_root /c/Users/Administrator/AppData/Roaming 6

log ""
log "==== 2) C 盘根层目录（depth 2）===="
scan_root /c 2

log ""
log "==== 3) D 盘（depth 4）===="
scan_root /d 4

log ""
log "==== 4) E 盘（depth 4，排除 /e/nixiang）===="
scan_root /e 4

log ""
log "==== 5) 大体积 APK（>50MB）在用户目录与 D/E 根 ===="
find /c/Users/Administrator/Downloads /c/Users/Administrator/Desktop /d /e -maxdepth 4 \
  \( -path '/e/nixiang' -prune \) -o \( -iname '*.apk' -size +50M \) -print 2>/dev/null | head -60 >> "$OUT"

log ""
log "==== 6) 与逆向/打包直接相关的名字 ===="
find /c /d /e -maxdepth 4 \( -path '/e/nixiang' -prune \) -o \
  \( -iname '*搜包*' -o -iname '*拆包*' -o -iname '*dump*' -o -iname '*il2cpp*' \
     -o -iname '*frida*' -o -iname '*smali*' \) -print 2>/dev/null \
  | grep -v '^/e/nixiang' | head -80 >> "$OUT"

echo "done -> $OUT"
wc -l "$OUT"
