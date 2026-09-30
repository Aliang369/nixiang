#!/usr/bin/env bash
# 失落城堡：从运行进程内存导出 libil2cpp.so 的全部映射段，用于与磁盘文件逐字节比对。
#
# 目的：判定易盾（NIS）对 libil2cpp.so 的加密形态 ——
#   (a) 内存 == 磁盘  ⇒ 静态 Il2CppDumper 可直接用磁盘文件；
#   (b) 内存 != 磁盘  ⇒ 壳在装载时解密，必须用内存映像做 il2cpp dump。
#
# 用法：bash 04_scripts/失落城堡/slcb_dump_il2cpp_mem.sh [模块名]
# 默认模块名 libil2cpp.so。读 /proc/<pid>/mem 需要 root（设备 f9a116a2 已 root）。
set -euo pipefail

SERIAL="${SERIAL:-f9a116a2}"
MOD="${1:-libil2cpp.so}"
PKG=com.leiting.lostcastle.leiting
DEV_DIR=/data/local/tmp/slcb_il2cpp_mem
OUT_DIR="09_test/失落城堡/mem"
ORIG="09_test/失落城堡/orig/elfcheck/lib/arm64-v8a/$MOD"

cd "$(dirname "$0")/../.."

PID=$(adb -s "$SERIAL" shell "pidof $PKG" | tr -d '\r' | awk '{print $1}')
[[ -n "$PID" ]] || { echo "ERROR: $PKG 未运行"; exit 1; }
echo "[mem] pid=$PID mod=$MOD"

adb -s "$SERIAL" shell "su -c 'rm -rf $DEV_DIR; mkdir -p $DEV_DIR; chmod 777 $DEV_DIR'" </dev/null

MAPS=$(adb -s "$SERIAL" shell "su -c 'cat /proc/$PID/maps'" </dev/null | tr -d '\r' | grep -F "$MOD" || true)
[[ -n "$MAPS" ]] || { echo "ERROR: maps 里找不到 $MOD"; exit 1; }
echo "[mem] maps:"
echo "$MAPS" | sed 's/^/    /'

i=0
while read -r line; do
  [[ -z "$line" ]] && continue
  range=$(echo "$line" | awk '{print $1}')
  perm=$(echo "$line" | awk '{print $2}')
  off=$(echo "$line"  | awk '{print $3}')
  start_hex=${range%%-*}
  end_hex=${range##*-}
  start=$((16#$start_hex))
  end=$((16#$end_hex))
  size=$((end - start))
  pages=$((size / 4096))
  [[ "$pages" -gt 0 ]] || { echo "[mem] skip zero-len $range"; continue; }
  name=$(printf "seg_%02d_%s_%s_%s.bin" "$i" "$perm" "$off" "$start_hex")
  echo "[mem] dump #$i $range $perm off=$off pages=$pages -> $name"
  # ★ 内层 adb 必须 </dev/null，否则它会吃掉外层 while 的 stdin（首轮只 dump 到 #0 即此因）
  adb -s "$SERIAL" shell "su -c 'dd if=/proc/$PID/mem of=$DEV_DIR/$name bs=4096 skip=$((start / 4096)) count=$pages 2>/dev/null'" </dev/null
  i=$((i + 1))
done <<< "$MAPS"

mkdir -p "$OUT_DIR"
adb -s "$SERIAL" shell "su -c 'chmod 666 $DEV_DIR/*'" </dev/null
adb -s "$SERIAL" pull "$DEV_DIR" "$OUT_DIR/" >/dev/null
echo "[mem] pulled -> $OUT_DIR/slcb_il2cpp_mem/"

# 逐段与磁盘文件对应偏移比对 + 按文件偏移拼回完整 .so
python3 - "$OUT_DIR/slcb_il2cpp_mem" "$ORIG" "$OUT_DIR/slcb_libil2cpp_memresolved.so" <<'PY'
import os, sys, hashlib
d, orig, out_path = sys.argv[1], sys.argv[2], sys.argv[3]
if not os.path.isfile(orig):
    print(f"[cmp] 磁盘原文件不在: {orig}（跳过比对）"); sys.exit(0)
blob = open(orig, 'rb').read()
print(f"[cmp] 磁盘 {orig} size={len(blob)}")

segs = []
for fn in sorted(os.listdir(d)):
    if not fn.endswith('.bin'):
        continue
    parts = fn[:-4].split('_')          # seg_NN_perm_off_start.bin
    off = int(parts[3], 16)
    data = open(os.path.join(d, fn), 'rb').read()
    segs.append((off, fn, data))
    ref = blob[off:off + len(data)]
    if len(ref) != len(data):
        print(f"[cmp] {fn} size={len(data)} 超出文件尾 {len(ref)}B → 属 memsz 匿名尾（无文件对应）")
        ref = ref + b'\x00' * (len(data) - len(ref))
    same = data == ref
    diff = sum(1 for a, b in zip(data, ref) if a != b)
    nz = sum(1 for b in data if b != 0)
    print(f"[cmp] {fn} size={len(data)} 同偏移 {'一致 ✅' if same else f'不同 ❌ 差异={diff}'}"
          f" | 非零={nz} ({nz/len(data):.1%}) md5={hashlib.md5(data).hexdigest()}")

# 拼回：同一 file offset 只取第一段（后者是 PT_LOAD memsz 匿名尾，无文件内容）
segs.sort(key=lambda t: t[0])
buf = bytearray(blob)          # 以磁盘内容为底，用内存覆盖
placed, seen = [], set()
for off, fn, data in segs:
    if off in seen:
        print(f"[asm] skip {fn}（file off={off:#x} 已有内存映像）")
        continue
    seen.add(off)
    end = min(off + len(data), len(buf))
    buf[off:end] = data[:end - off]
    placed.append((off, end, fn))
print("[asm] 覆盖区间:")
for off, end, fn in placed:
    print(f"    {off:#010x}..{end:#010x}  ({end-off} B)  <- {fn}")
open(out_path, 'wb').write(buf)
print(f"[asm] 输出 {out_path} size={len(buf)} md5={hashlib.md5(bytes(buf)).hexdigest()}")
changed = sum(1 for a, b in zip(buf, blob) if a != b)
print(f"[asm] 与磁盘差异字节 = {changed} ({changed/len(blob):.1%})")
PY

echo "[done]"
