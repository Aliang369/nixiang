#!/usr/bin/env bash
# 无悔华夏 4399：从运行中进程的 /proc/<pid>/mem 精确导出业务 DEX（无壳重建的输入）。
#
# 只读：不 ptrace attach、不注入、不改目标进程。以 root 读 /proc/<pid>/mem。
# 编译要点（2026-11-05 实测）：whrhx_mem_dex.c 必须**动态链接**（不带 -static）——
#   NDK 静态链接产出的 PT_TLS 对齐只有 8，bionic 要求 >= 64，装上就报
#   "executable's TLS segment is underaligned ... Aborted"。动态版无 PT_TLS，直接可跑。
# 业务 DEX 判定：version 035 + class_defs 前缀命中（com/tpf、com/excelliance、cn.m4399、com/yhhy、com/unity3d）；
#   version 039 的都是 Android 16 boot dex，必须排除（4399 的壳不像九游那样把业务拆成 13 份，只有 6 份）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PKG="${PKG:-com.yhhy.whrhx.m4399}"
SERIAL="${SERIAL:-f9a116a2}"
RUN_TAG="${RUN_TAG:-$(date +%Y%m%d)}"
RUN_DIR="${RUN_DIR:-$NIXIANG_ROOT/06_dumped/无悔华夏4399/runtime_$RUN_TAG}"
OUT="$RUN_DIR/realdex"
ADB_BIN="${ADB_BIN:-adb}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
CC="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android21-clang"
READELF="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-readelf"
SRC="$NIXIANG_ROOT/04_scripts/无悔华夏/whrhx_mem_dex.c"
BIN_DIR="$SCRIPT_DIR/bin"
BIN="$BIN_DIR/whrhx_mem_dex"
REMOTE_BIN="/data/local/tmp/whrhx_mem_dex"
REMOTE_OUT="/data/local/tmp/whrhx4399_dex_$RUN_TAG"

[ -x "$CC" ] || { echo "ERROR: clang 不存在: $CC" >&2; exit 1; }
[ -f "$SRC" ] || { echo "ERROR: 缺源码: $SRC" >&2; exit 1; }

echo "[1/5] 编译 whrhx_mem_dex（动态链接，规避 TLS 对齐）"
mkdir -p "$BIN_DIR"
"$CC" -O2 -o "$BIN" "$SRC"
if "$READELF" -l "$BIN" | grep -q 'TLS'; then
  echo "ERROR: 产物含 PT_TLS（bionic 会拒绝执行），检查编译参数" >&2; exit 1
fi
echo "  built $(basename "$BIN") $(stat -f %z "$BIN")B"

echo "[2/5] 确保游戏在运行"
PID="$("$ADB_BIN" -s "$SERIAL" shell pidof "$PKG" 2>/dev/null | tr -d '\r')"
if [ -z "$PID" ]; then
  echo "  未运行 → monkey 拉起并等 60s"
  "$ADB_BIN" -s "$SERIAL" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
  sleep 60
  PID="$("$ADB_BIN" -s "$SERIAL" shell pidof "$PKG" | tr -d '\r')"
fi
[ -n "$PID" ] || { echo "ERROR: 游戏进程不存在" >&2; exit 1; }
echo "  pid=$PID"

echo "[3/5] 推送到设备并以 root 扫描（只读 /proc/$PID/mem）"
"$ADB_BIN" -s "$SERIAL" push "$BIN" "$REMOTE_BIN" >/dev/null
"$ADB_BIN" -s "$SERIAL" shell "su -c 'chmod 755 $REMOTE_BIN'"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'mkdir -p $REMOTE_OUT'"
"$ADB_BIN" -s "$SERIAL" shell "su -c '$REMOTE_BIN $PID $REMOTE_OUT > $REMOTE_OUT/scan.log 2>&1'"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'tail -1 $REMOTE_OUT/scan.log'"

echo "[4/5] 打包拉回"
mkdir -p "$OUT"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'tar cf $REMOTE_OUT.tar -C /data/local/tmp $(basename "$REMOTE_OUT")'"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'chmod 644 $REMOTE_OUT.tar'"
TAR="$(mktemp /tmp/whrhx4399_dex.XXXXXX.tar)"
"$ADB_BIN" -s "$SERIAL" pull "$REMOTE_OUT.tar" "$TAR" >/dev/null
tar xf "$TAR" -C "$OUT" --strip-components=1
rm -f "$TAR"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -rf $REMOTE_OUT $REMOTE_OUT.tar'" >/dev/null 2>&1 || true

echo "[5/5] 筛业务 DEX（version 035 + 类前缀）"
python3.11 - "$OUT" <<'PY'
import struct, sys, hashlib, collections, json
from pathlib import Path

root = Path(sys.argv[1])
BIZ = ('com/tpf', 'com/excelliance', 'cn/m4399', 'com/yhhy', 'com/unity3d', 'com/whrhx', 'com/netease/nis')

def uleb(b, off):
    r = s = 0
    while True:
        x = b[off]; off += 1
        r |= (x & 0x7f) << s
        if not x & 0x80: break
        s += 7
    return r, off

def parse(p):
    b = p.read_bytes()
    if b[:4] != b'dex\n': return None
    ver = b[4:7].decode()
    fsz = struct.unpack_from('<I', b, 0x20)[0]
    sid_n, sid_off = struct.unpack_from('<II', b, 0x38)
    tid_n, tid_off = struct.unpack_from('<II', b, 0x40)
    cd_n, cd_off = struct.unpack_from('<II', b, 0x60)
    pref = collections.Counter()
    for i in range(cd_n):
        cidx = struct.unpack_from('<I', b, cd_off + i * 32)[0]
        if cidx >= tid_n: continue
        didx = struct.unpack_from('<I', b, tid_off + cidx * 4)[0]
        if didx >= sid_n: continue
        soff = struct.unpack_from('<I', b, sid_off + didx * 4)[0]
        n, o = uleb(b, soff); end = b.index(b'\x00', o)
        desc = b[o:end].decode('utf-8', 'replace').lstrip('L').rstrip(';')
        pref['/'.join(desc.split('/')[:2])] += 1
    return {'ver': ver, 'size': len(b), 'header_size': fsz, 'complete': len(b) == fsz,
            'classes': cd_n, 'sha256': hashlib.sha256(b).hexdigest(),
            'biz_hits': sum(v for k, v in pref.items() if k in BIZ), 'pref': dict(pref.most_common(5))}

rows, seen, biz = [], {}, []
for p in sorted(root.glob('*.dex')):
    info = parse(p)
    if not info: continue
    if info['sha256'] in seen: continue
    seen[info['sha256']] = p.name
    info['name'] = p.name
    rows.append(info)
    # 极小的 035 dex（<=10 类，如 9,340B 的 com.google 附属件）壳也加载了，放宽一并纳入
    if info['ver'] == '035' and info['complete'] and (info['biz_hits'] > 0 or info['classes'] <= 10):
        biz.append(info)

biz.sort(key=lambda r: -r['size'])
print('  唯一 dex = %d，业务 dex = %d，合计 %d B' % (len(rows), len(biz), sum(r['size'] for r in biz)))
for r in biz:
    print('    %-32s %10d B  cls=%-5d biz=%d' % (r['name'], r['size'], r['classes'], r['biz_hits']))
root.joinpath('biz_dex.json').write_text(
    json.dumps([{k: r[k] for k in ('name', 'size', 'ver', 'classes', 'sha256', 'biz_hits')} for r in biz],
               ensure_ascii=False, indent=2), encoding='utf-8')
print('  → %s' % (root / 'biz_dex.json'))
PY

echo "DONE $OUT"
