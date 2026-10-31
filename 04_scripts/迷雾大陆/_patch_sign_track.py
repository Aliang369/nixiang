#!/usr/bin/env python3
"""一次性补丁：给 surgical_mwdl_15.0.0.sh 加「签名轨」（MT killer）两步。

2026-10-31：登录返回 10011/10045「指纹签名无效」，SignSpoof 已删且禁止复活，
按双轨制改走运行时签名替换（AppComponentFactory 挂 killer）。
"""
from pathlib import Path

P = Path("mod菜单/starry-mod-menu/scripts/surgical_mwdl_15.0.0.sh")
s = P.read_text(encoding="utf-8")

if "2b/6] 官方签名证书 DER" in s:
    print("already patched")
    raise SystemExit(0)

CERT_STEP = r'''echo "[2b/6] 官方签名证书 DER（v2 签名块）→ 喂 killer"
[[ -d "$KILLER_SMALI/bin/mt/signature" ]] || { echo "ERROR: missing killer smali $KILLER_SMALI"; exit 1; }
python3.11 - "$ORIG_APK" "$WORK/official.der" <<'PYCERT'
import struct, sys, hashlib
apk, out = sys.argv[1], sys.argv[2]
d = open(apk, 'rb').read()
i = d.rfind(b'PK\x05\x06')
assert i > 0, 'EOCD not found'
cd_off = struct.unpack('<I', d[i + 16:i + 20])[0]
blk_size = struct.unpack('<Q', d[cd_off - 24:cd_off - 16])[0]
start = cd_off - blk_size - 8
assert d[start:start + 8] == struct.pack('<Q', blk_size), 'APK Signing Block magic mismatch'
pairs = d[start + 8:cd_off - 24]
off, v2 = 0, None
while off < len(pairs):
    plen = struct.unpack('<Q', pairs[off:off + 8])[0]
    pid = struct.unpack('<I', pairs[off + 8:off + 12])[0]
    val = pairs[off + 12:off + 8 + plen]
    if pid == 0x7109871a:
        v2 = val
    off += 8 + plen
assert v2, 'v2 signature block (0x7109871a) not found'
o = 4                                                    # signers seq len
o += 4                                                   # signer len
o += 4                                                   # signed-data len
dg = struct.unpack('<I', v2[o:o + 4])[0]; o += 4 + dg     # digests
o += 4                                                   # certificates seq len
ln = struct.unpack('<I', v2[o:o + 4])[0]; o += 4
der = v2[o:o + ln]
assert der[:1] == b'\x30', 'certificate DER malformed'
open(out, 'wb').write(der)
print('  cert DER %d B md5 %s' % (len(der), hashlib.md5(der).hexdigest()))
PYCERT
SIGNER_MD5="$("$APKSIGNER" verify --print-certs "$ORIG_APK" 2>/dev/null | sed -n 's/^Signer #1 certificate MD5 digest: //p' | head -1 | tr -d '\r')"
DER_MD5="$(md5 -q "$WORK/official.der" 2>/dev/null || md5sum "$WORK/official.der" | awk '{print $1}')"
echo "  证书 DER md5=$DER_MD5（apksigner 报 ${SIGNER_MD5:-无}）"
[[ -z "$SIGNER_MD5" || "$DER_MD5" == "$SIGNER_MD5" ]] || { echo "ERROR: 证书 DER 与 apksigner 不一致 —— killer 喂错证书会静默不生效"; exit 1; }

'''

KILLER_ASSEMBLE = r'''# 签名轨：killer 并入同一个 classes5（包名/证书按本底包现改，不写死）
rsync -a "$KILLER_SMALI/bin" "$SMALI_ROOT/"
[[ -f "$SMALI_ROOT/bin/mt/signature/KillerApplication193.smali" ]] || { echo "ERROR: killer smali copy failed"; exit 1; }
python3.11 - "$SMALI_ROOT" "$WORK/official.der" "$PKG" <<'PYKILLER'
import base64, io, os, re, sys
root, der_path, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
b64 = base64.b64encode(open(der_path, 'rb').read()).decode()
wrapped = '\\n'.join(b64[i:i + 76] for i in range(0, len(b64), 76))
p = os.path.join(root, 'bin/mt/signature/KillerApplication193.smali')
t = io.open(p, encoding='utf-8').read()
assert '.super Landroid/app/Application;' in t, 'killer 父类不是 android.app.Application'
t, n = re.subn(r'const-string v1, "[^"]*"', 'const-string v1, "%s"' % pkg, t, count=1)
assert n == 1, 'killer 包名锚点未命中'
j = t.find('const-string v0, "MI')
assert j >= 0, 'killer 证书锚点未命中'
k = t.find('"', j + len('const-string v0, '))
k2 = t.find('"', k + 1)
t = t[:j] + 'const-string v0, "%s"' % wrapped + t[k2 + 1:]
io.open(p, 'w', encoding='utf-8', newline='\n').write(t)
print('  killer: pkg=%s cert_b64=%d chars' % (pkg, len(b64)))
PYKILLER
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"'''

A = 'echo "[3/6] collect com/starry smali → classes5.dex"'
assert A in s
s = s.replace(A, CERT_STEP + A, 1)

B = '"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"'
assert s.count(B) == 1
s = s.replace(B, KILLER_ASSEMBLE, 1)

P.write_text(s, encoding="utf-8")
print("patched:", len(s))
