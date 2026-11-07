#!/usr/bin/env bash
# 境界刀鸣 国际服 1.48.211534（com.bladetw.bd，GooglePlay 渠道，vc48）— hhld kill-switch NOP 外科补丁
#
# 背景（实测）：非官方签名启动 ~20s 后崩，tombstone：
#   signal 11 (SIGSEGV), code 2 (SEGV_ACCERR), fault addr 0x18a44, pc=lr=0x18a44
#   backtrace 1 帧 [anon:dalvik-free list large object space]
# 与国服 com.blade.bd 1.39.211507 是同一个 kill-switch（同 fault addr）。
#
# 定位：libhhld-rt.so（split_config.arm64_v8a.apk 内）0x7938c 起为 TLS 析构链：
#   7939c  bl  0x12262c                 ; 取 pthread key
#   793a0  ldr x1,[x19]                 ; node->arg
#   793ac  bl  pthread_setspecific      ; 清 TLS
#   793b0  ldr x8,[x19,#0x8]            ; node->func  <- 与国服同构的 [x19+8]
#   793b8  blr x8                       ; * 补丁点 0x793b8  00 01 3f d6 -> 1f 20 03 d5 (nop)
# 国服对应点：0x5c270（旧版 1199664 B）；本版 1970168 B。
#
# 用法:
#   bash 04_scripts/境界刀鸣国际/patch_hhld_nop_intl.sh            # 只出包
#   bash 04_scripts/境界刀鸣国际/patch_hhld_nop_intl.sh --install  # 出包 + install-multiple 覆盖
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "$0")/../.." && pwd)}"
VER="1.48.211534"
SRC_DIR="$NIXIANG/02_apks/境界刀鸣国际"
OUT_DIR="$NIXIANG/08_output/境界刀鸣国际"
SRC_CFG="$SRC_DIR/境界刀鸣国际_${VER}_official_from_device_split_config.arm64_v8a.apk"
PATCHED_CFG="$OUT_DIR/境界刀鸣国际_${VER}_hhldnop_split_config.arm64_v8a.apk"
BASE_APK="$OUT_DIR/境界刀鸣国际_${VER}_resignsmoke_base.apk"
IT_APK="$OUT_DIR/境界刀鸣国际_${VER}_resignsmoke_split_installtime.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_ENTRY="lib/arm64-v8a/libhhld-rt.so"
HH_OFF=793b8
HH_OLD="00013fd6"
HH_NEW="1f2003d5"

for f in "$SRC_CFG" "$BASE_APK" "$IT_APK" "$KS"; do
  [ -f "$f" ] || { echo "ERROR: missing $f"; exit 1; }
done

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[ -n "$BT" ] || { echo "ERROR: 找不到 build-tools"; exit 1; }

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"

echo "[1/3] 外科打补丁 -> $PATCHED_CFG"
/opt/homebrew/bin/python3.11 - "$SRC_CFG" "$PATCHED_CFG" "$SO_ENTRY" "$HH_OFF" "$HH_OLD" "$HH_NEW" <<'PY'
import sys, zipfile
src, dst, entry, off, old, new = sys.argv[1], sys.argv[2], sys.argv[3], int(sys.argv[4], 16), bytes.fromhex(sys.argv[5]), bytes.fromhex(sys.argv[6])
zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n_hit = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    data = zin.read(item)
    if name == entry:
        if data[off:off + len(old)] != old:
            raise SystemExit("ERROR: 补丁点不符 %s@0x%x 实际=%s" % (name, off, data[off:off+len(old)].hex()))
        data = data[:off] + new + data[off+len(new):]
        n_hit += 1
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()
if n_hit != 1:
    raise SystemExit("ERROR: 命中数=%d（应为 1）" % n_hit)
z = zipfile.ZipFile(dst)
so = z.read(entry)
chk = so[off:off + len(new)]
assert chk == new, "ERROR: 成品校验失败 %s" % chk.hex()
print("  hhld NOP @0x%x %s -> %s  OK（so %d B）" % (off, old.hex(), new.hex(), len(so)))
PY

echo "[2/3] zipalign + apksigner"
ALIGNED="$(mktemp /tmp/jjdmintl_cfg.XXXXXX.apk)"
"$BT/zipalign" -f -p 4 "$PATCHED_CFG" "$ALIGNED"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$PATCHED_CFG" "$ALIGNED" 2>&1 | grep -v WARNING || true
rm -f "$ALIGNED"
"$BT/apksigner" verify --print-certs "$PATCHED_CFG" 2>&1 | grep -E "certificate DN|certificate SHA-256" | head -2

echo "[3/3] 结果"
ls -l "$PATCHED_CFG"
shasum -a 256 "$PATCHED_CFG"

if [ "${1:-}" = "--install" ]; then
  ADB="${ADB:-/opt/homebrew/bin/adb}"
  DEV="${DEV:-}"
  echo "[install] install-multiple -r（同签名覆盖，保留数据）"
  "$ADB" ${DEV:+-s "$DEV"} install-multiple -r -d "$BASE_APK" "$PATCHED_CFG" "$IT_APK"
fi
echo "[done] $PATCHED_CFG"
