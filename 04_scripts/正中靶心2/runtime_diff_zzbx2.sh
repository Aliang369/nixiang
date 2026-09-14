#!/usr/bin/env bash
# 正中靶心2：比较 APK 内 so 与运行中进程的 RX 段，定位斌哥 native inline patch。
# 只读目标进程内存；临时文件仅落在游戏作用域 /data/local/tmp/zzbx2_diff。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
META_FILE="$SCRIPT_DIR/binge_apk.json"
OUT_DIR="$NIXIANG_ROOT/06_dumped/正中靶心2/binge_1.6/runtime_diff"
PKG="${PKG:-com.openew.monster}"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 $STATE_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }
[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }

APK="$(python3 - "$META_FILE" <<'PY'
import json, sys
print(json.load(open(sys.argv[1], encoding='utf-8'))['apk_path'])
PY
)"
[[ -f "$APK" ]] || { echo "错误: APK 不存在: $APK" >&2; exit 1; }

rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR/segments"

python3 - "$SERIAL" "$PKG" "$APK" "$OUT_DIR" "$ADB_BIN" <<'PY'
import json
import os
import re
import shutil
import struct
import subprocess
import sys
import zipfile
from pathlib import Path
import shlex

serial, pkg, apk, out_dir, adb = sys.argv[1:]
out_dir = Path(out_dir)
adb = shutil.which(adb) or adb

def run(args, check=True):
    return subprocess.run(args, check=check, text=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)

def shell(cmd, check=True):
    return run([adb, '-s', serial, 'shell', f'su -c {shlex.quote(cmd)}'], check=check)

pid = shell(f'pidof {pkg}').stdout.strip().split()[0]
maps = shell(f'cat /proc/{pid}/maps').stdout
(out_dir / 'maps.txt').write_text(maps, encoding='utf-8')

line_re = re.compile(r'^([0-9a-f]+)-([0-9a-f]+)\s+(\S+)\s+([0-9a-f]+)\s+\S+\s+\d+\s+(.*)$')
wanted = {}
for line in maps.splitlines():
    m = line_re.match(line)
    if not m:
        continue
    start, end, perm, foff, path = m.groups()
    if perm != 'r-xp':
        continue
    if path.endswith('/libcocos2djs.so') or path.endswith('/libSDKloading.so'):
        wanted.setdefault(path, []).append((int(start, 16), int(end, 16), int(foff, 16), path))

remote_root = '/data/local/tmp/zzbx2_diff'
shell(f'rm -rf {remote_root} && mkdir -p {remote_root}')

records = []
idx = 0
for path in sorted(wanted):
    module = Path(path).name
    with zipfile.ZipFile(apk) as zf:
        src_name = next((n for n in zf.namelist() if n.endswith('/arm64-v8a/' + module)), None)
        if not src_name:
            raise SystemExit(f'APK 内未找到 {module}')
        disk = zf.read(src_name)

    for start, end, foff, _ in wanted[path]:
        idx += 1
        size = end - start
        remote = f'{remote_root}/{idx:02d}_{module}.bin'
        local = out_dir / 'segments' / f'{idx:02d}_{module}_{start:x}_{size:x}.bin'
        dd = (
            f"dd if=/proc/{pid}/mem of={remote} bs=1048576 "
            f"iflag=skip_bytes,count_bytes skip={start} count={size} status=none && chmod 644 {remote}"
        )
        shell(dd)
        run([adb, '-s', serial, 'pull', remote, str(local)])
        runtime = local.read_bytes()
        disk_view = disk[foff:foff + size]
        if len(disk_view) != len(runtime):
            records.append({
                'module': module,
                'path': path,
                'runtime_start': hex(start),
                'runtime_end': hex(end),
                'file_offset': hex(foff),
                'size': size,
                'disk_available': len(disk_view),
                'error': 'disk_view_size_mismatch',
            })
            continue
        diff_offsets = [i for i, (a, b) in enumerate(zip(disk_view, runtime)) if a != b]
        pages = sorted({off // 4096 for off in diff_offsets})
        groups = []
        if diff_offsets:
            s = p = diff_offsets[0]
            for off in diff_offsets[1:]:
                if off == p + 1:
                    p = off
                else:
                    groups.append([s, p])
                    s = p = off
            groups.append([s, p])
        records.append({
            'module': module,
            'path': path,
            'runtime_start': hex(start),
            'runtime_end': hex(end),
            'file_offset': hex(foff),
            'size': size,
            'diff_bytes': len(diff_offsets),
            'diff_page_count': len(pages),
            'diff_pages': [hex(foff + p * 4096) for p in pages],
            'diff_ranges': [{'offset': off, 'size': end_off - off + 1,
                             'file_off': hex(foff + off),
                             'runtime_off': hex(start + off)} for off, end_off in groups[:200]],
        })

shell(f'rm -rf {remote_root}')
report = {'package': pkg, 'pid': int(pid), 'modules': records}
(out_dir / 'runtime_diff.json').write_text(
    json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8'
)
print(json.dumps(report, ensure_ascii=False, indent=2))
PY

cat "$OUT_DIR/runtime_diff.json"
