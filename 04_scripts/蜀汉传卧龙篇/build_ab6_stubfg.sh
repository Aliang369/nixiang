#!/usr/bin/env bash
# AB-6 = AB-5(SDK跳过+签名桥patch+libpatcher) + libFairGuard.so 换空壳桩（DT_NEEDED 依赖测试）
# 假说：FairGuard 为完整性检测源头；桩掉它则检测器族整体失活
set -euo pipefail

N=/Users/aliang/Documents/nixiang
W="$N/07_patched/蜀汉传卧龙篇/ab1_20260905"
ORIG="$N/02_apks/蜀汉传卧龙篇/蜀汉传卧龙篇_100.22.1_from_device_20260905.apk"
BT="$HOME/Library/Android/sdk/build-tools/36.0.0"
KS="$N/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
OUT="$W/蜀汉传卧龙篇_100.22.1_ab6_stubfg_signed.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17)}"

grep -q '0769ed9f9aab50cc4732774d74f56ff4' "$W/smali_out/com/bantu/trgame/TRGame.smali" || { echo "AB-1 补丁丢失"; exit 1; }
[[ -f "$W/staging/lib/arm64-v8a/libpatcher.so" ]] || { echo "先跑 build_ab5_patchthread.sh"; exit 1; }

# ① smali 回编（AB-1+loadLibrary 已在 smali_out）
"$N/01_tools/smali/smali" a "$W/smali_out" -o "$W/staging/classes.dex"

# ② 外科 zip：classes.dex 更新 + libpatcher.so 新增 + libFairGuard.so 替换为桩
cp -f "$ORIG" "$W/ab6_unsigned.apk"
mkdir -p "$W/staging_fg/lib/arm64-v8a"
cp -f /tmp/libFairGuard_stub.so "$W/staging_fg/lib/arm64-v8a/libFairGuard.so"
( cd "$W/staging"    && zip -q "$W/ab6_unsigned.apk" classes.dex lib/arm64-v8a/libpatcher.so )
( cd "$W/staging_fg" && zip -q "$W/ab6_unsigned.apk" lib/arm64-v8a/libFairGuard.so )

# ③ 验证包内 libFairGuard 确为桩（体积 ~8KB 而非 4.5MB）
python3 - "$W/ab6_unsigned.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
for n in ('lib/arm64-v8a/libFairGuard.so', 'lib/arm64-v8a/libpatcher.so'):
    print(n, z.getinfo(n).file_size)
sz = z.getinfo('lib/arm64-v8a/libFairGuard.so').file_size
assert sz < 100000, f'libFairGuard 未被替换 size={sz}'
print('STUB VERIFIED')
PY

# ④ zipalign + 签名
"$BT/zipalign" -f 4 "$W/ab6_unsigned.apk" "$W/ab6_aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$OUT" "$W/ab6_aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" 2>/dev/null | head -2
ls -lh "$OUT"
