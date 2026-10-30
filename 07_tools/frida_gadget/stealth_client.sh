#!/bin/bash
# 切换本机 frida CLI 的「隐身对偶」：与 gadget_stealth.so 配套。
# 协议标记 frida:rpc → froda:rpc（等长替换），CLI 与 agent 必须一致才能握手。
# 用法: bash stealth_client.sh on|off|status
set -euo pipefail
SO="$HOME/Library/Python/3.11/lib/python/site-packages/frida/_frida.abi3.so"
BAK="$SO.orig"
[[ -f "$SO" ]] || { echo "ERROR: 找不到 $SO"; exit 1; }
case "${1:-status}" in
  on)
    [[ -f "$BAK" ]] || cp -p "$SO" "$BAK"
    python3 - "$BAK" "$SO" <<'PY'
import sys
d = open(sys.argv[1],'rb').read()
n = d.count(b'frida:rpc')
d = d.replace(b'frida:rpc', b'froda:rpc')
open(sys.argv[2],'wb').write(d)
print('patched %d occurrences' % n)
PY
    echo "stealth client: ON（配套 gadget_stealth.so）";;
  off)
    [[ -f "$BAK" ]] && cp -p "$BAK" "$SO" && echo "stealth client: OFF（已还原原版）" || echo "无需还原";;
  status)
    if [[ -f "$BAK" ]]; then echo "备份存在 ⇒ 当前可能是 ON"; else echo "无备份 ⇒ 当前是原版"; fi
    grep -c "froda:rpc" "$SO" 2>/dev/null | sed 's/^/froda:rpc 出现次数=/';;
  *) echo "用法: $0 on|off|status"; exit 2;;
esac
