#!/usr/bin/env bash
# 全游戏卡密：统一 / 补齐 device_id 种子到全局可读位置。
# 宿主常无存储权限读不了 /sdcard；必须有 /data/local/tmp/starry_magic/device_id。
# 冲突时：sdcard > tmp > 其它包 prefs；装机侧把副本统一成 canonical。
# 兼容 macOS Bash 3（勿用 mapfile/关联数组）。
set -euo pipefail

TMP_DIR=/data/local/tmp/starry_magic

ROWS=$(adb shell "su -c '
  for p in \
    /sdcard/Android/starry_magic/device_id \
    /sdcard/.starry_magic/device_id \
    /sdcard/Download/.starry_magic/device_id \
    /data/local/tmp/starry_magic/device_id
  do
    v=\$(cat \"\$p\" 2>/dev/null | tr -d \"\\r\\n\")
    case \"\$v\" in dev_*) echo \"\$p|\$v\";; esac
  done
  grep -h auth_device_id /data/data/*/shared_prefs/starry_magic_prefs.xml 2>/dev/null \
    | sed -n \"s/.*\\(dev_[a-fA-F0-9_]\\{8,\\}\\).*/prefs|\\1/p\"
'" | tr -d '\r')

if [[ -z "${ROWS}" ]]; then
  echo "[seed] no existing device_id — skip (first bind will create)"
  exit 0
fi

score_of() {
  case "$1" in
    */Android/starry_magic/*) echo 100 ;;
    */.starry_magic/*) echo 90 ;;
    */Download/.starry_magic/*) echo 70 ;;
    */tmp/starry_magic/*) echo 40 ;;
    prefs) echo 30 ;;
    *) echo 10 ;;
  esac
}

ID=""
SRC=""
BEST=-1
ALL_IDS=""
while IFS= read -r row; do
  [[ -z "$row" || "$row" != *"|"* ]] && continue
  p=${row%%|*}
  v=${row#*|}
  case "$v" in dev_*) ;; *) continue ;; esac
  case " $ALL_IDS " in
    *" $v "*) ;;
    *) ALL_IDS="$ALL_IDS $v" ;;
  esac
  s=$(score_of "$p")
  if [[ "$s" -gt "$BEST" ]]; then
    BEST=$s
    ID=$v
    SRC=$p
  fi
done <<EOF
$ROWS
EOF

if [[ -z "$ID" ]]; then
  echo "[seed] parse failed"
  exit 0
fi

COUNT=$(echo "$ALL_IDS" | wc -w | tr -d ' ')
if [[ "$COUNT" -gt 1 ]]; then
  echo "[seed] CONFLICT ids=$ALL_IDS → keep $ID (from $SRC)"
fi
echo "[seed] canonical $ID @$SRC"

adb shell "su -c '
  ID=\"$ID\"
  write_one() {
    f=\"\$1\"
    d=\$(dirname \"\$f\")
    mkdir -p \"\$d\" && chmod 755 \"\$d\"
    cur=\$(cat \"\$f\" 2>/dev/null | tr -d \"\\r\\n\")
    if [ -n \"\$cur\" ] && [ \"\$cur\" != \"\$ID\" ]; then
      echo \"[seed] heal \$f : \$cur -> \$ID\"
    fi
    printf %s \"\$ID\" > \"\$f\"
    chmod 666 \"\$f\"
  }
  write_one /data/local/tmp/starry_magic/device_id
  write_one /sdcard/Android/starry_magic/device_id
  write_one /sdcard/.starry_magic/device_id
  write_one /sdcard/Download/.starry_magic/device_id
  echo -n tmp=; cat /data/local/tmp/starry_magic/device_id
'"
echo
echo "[seed] ok → all copies = $ID"
