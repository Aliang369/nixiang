#!/usr/bin/env bash
# 蜀汉传卧龙篇 AB-1 测试包构建（2026-09-05，配方见 10_docs/蜀汉传卧龙篇/突破口重析_20260905.md §二 P0-A/P0-B）
# 变量隔离：仅 SDK跳过(InitEx/HTProtect.init 删除) + getPackageSign 签名桥 patch
# 不带：seccomp / SignSpoof / Dobby / 星空 so / 新 dex / Manifest 改动
set -euo pipefail

N=/Users/aliang/Documents/nixiang
W="$N/07_patched/蜀汉传卧龙篇/ab1_20260905"
ORIG="$N/02_apks/蜀汉传卧龙篇/蜀汉传卧龙篇_100.22.1_from_device_20260905.apk"
BT="$HOME/Library/Android/sdk/build-tools/36.0.0"
KS="$N/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
OUT="$W/蜀汉传卧龙篇_100.22.1_ab1_signpatch_signed.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17)}"

[[ -d "$W/smali_out" ]] || { echo "缺 smali_out，先 baksmali"; exit 1; }
T="$W/smali_out/com/bantu/trgame/TRGame.smali"

# ① 签名桥：getPackageSign 恒返官方证书 MD5（apksigner 实测 2026-09-05）
python3 - "$T" <<'PY'
import sys, re
p = sys.argv[1]
s = open(p, encoding='utf-8').read()
pat = re.compile(r'\.method public static getPackageSign\(\)Ljava/lang/String;.*?\.end method\n', re.S)
repl = ('.method public static getPackageSign()Ljava/lang/String;\n'
        '    .locals 1\n\n'
        '    const-string v0, "0769ed9f9aab50cc4732774d74f56ff4"\n\n'
        '    return-object v0\n'
        '.end method\n')
s, n1 = pat.subn(repl, s, count=1)

# ② 保护 SDK 跳过（仅删调用行，void 返回无 move-result）
s2, n2 = re.subn(r'    invoke-static \{\}, Lcom/swpprotect/sdk/SWProtectSDK;->InitEx\(\)V\n',
                 '    # [shc-ab1] SWProtectSDK.InitEx removed\n', s, count=1)
s3, n3 = re.subn(r'    invoke-static \{v1, p0, p1, p2, v0\}, Lcom/netease/htprotect/HTProtect;->init\(Landroid/content/Context;Ljava/lang/String;Ljava/lang/String;ILcom/netease/htprotect/HTPCallback;\)V\n',
                 '    # [shc-ab1] HTProtect.init removed\n', s2, count=1)
open(p, 'w', encoding='utf-8').write(s3)
print(f'patched: getPackageSign={n1} InitEx={n2} HTProtect.init={n3}')
sys.exit(0 if (n1 == 1 and n2 == 1 and n3 == 1) else 2)
PY
[[ $? -eq 0 ]] || { echo 'PATCH 计数异常，中止'; exit 2; }

# ③ smali 回编
"$N/01_tools/smali/smali" a "$W/smali_out" -o "$W/staging/classes.dex"

# ④ 外科 zip 更新（仅 ADD/REPLACE classes.dex 单条目）→ zipalign → 签名
cp -f "$ORIG" "$W/ab1_unsigned.apk"
( cd "$W/staging" && zip -q "$W/ab1_unsigned.apk" classes.dex )
"$BT/zipalign" -f 4 "$W/ab1_unsigned.apk" "$W/ab1_aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$OUT" "$W/ab1_aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" 2>/dev/null | head -3
ls -lh "$OUT"
