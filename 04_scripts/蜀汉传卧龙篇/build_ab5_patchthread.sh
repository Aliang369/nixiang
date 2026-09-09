#!/usr/bin/env bash
# AB-5 = AB-1(SDK跳过+签名桥patch,已在 smali_out) + libpatcher.so 进程内自补丁线程
# 产物: 蜀汉传卧龙篇_100.22.1_ab5_patchthread_signed.apk
set -euo pipefail

N=/Users/aliang/Documents/nixiang
W="$N/07_patched/蜀汉传卧龙篇/ab1_20260905"
ORIG="$N/02_apks/蜀汉传卧龙篇/蜀汉传卧龙篇_100.22.1_from_device_20260905.apk"
BT="$HOME/Library/Android/sdk/build-tools/36.0.0"
NDK="$HOME/Library/Android/sdk/ndk/27.0.12077973"
CC=$(ls "$NDK"/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android3[45]-clang | head -1)
KS="$N/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
OUT="$W/蜀汉传卧龙篇_100.22.1_ab5_patchthread_signed.apk"
T="$W/smali_out/com/bantu/trgame/TRGame.smali"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17)}"

# 0) AB-1 补丁留存校验
grep -q '0769ed9f9aab50cc4732774d74f56ff4' "$T" || { echo "AB-1 补丁丢失"; exit 1; }

# 1) TRGame.onCreate 顶部注入 loadLibrary("patcher")
python3 - "$T" <<'PY'
import sys
p = sys.argv[1]
s = open(p, encoding='utf-8').read()
if 'loadLibrary' in s and '"patcher"' in s:
    print('已注入过，跳过'); sys.exit(0)
old = '.method protected onCreate(Landroid/os/Bundle;)V\n    .registers 3\n'
new = ('.method protected onCreate(Landroid/os/Bundle;)V\n    .registers 3\n\n'
       '    const-string v0, "patcher"\n'
       '    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V\n')
assert old in s, 'onCreate 头不匹配'
open(p, 'w', encoding='utf-8').write(s.replace(old, new, 1))
print('loadLibrary 注入 OK')
PY

# 2) 编译 libpatcher.so（SHC_SECC_TRAP=exit/exit_group SIGSYS 取证；直接产出到 staging 路径）
mkdir -p "$W/staging/lib/arm64-v8a"
"$CC" -shared -O2 ${SHC_FLAGS:--DSHC_SECC_TRAP} -o "$W/staging/lib/arm64-v8a/libpatcher.so" \
  "$N/04_scripts/蜀汉传卧龙篇/libpatcher.c" -llog

# 3) smali 回编 + 外科 zip 更新（classes.dex 替换 + libpatcher.so 新增）
"$N/01_tools/smali/smali" a "$W/smali_out" -o "$W/staging/classes.dex"
cp -f "$ORIG" "$W/ab5_unsigned.apk"
( cd "$W/staging" && zip -q "$W/ab5_unsigned.apk" classes.dex lib/arm64-v8a/libpatcher.so )

# 4) zipalign + 签名
"$BT/zipalign" -f 4 "$W/ab5_unsigned.apk" "$W/ab5_aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$OUT" "$W/ab5_aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" 2>/dev/null | head -2
ls -lh "$OUT"
