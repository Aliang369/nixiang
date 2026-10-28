#!/usr/bin/env bash
# ================================================================
#  ⛔ DEPRECATED / 已废弃（2026-09-27 晚）—— 禁止用于交付
#  本脚本产出 base-resign.apk + split-starry.apk 两个包，与用户硬要求
#  「只能交付一个 APK」相违，已停用。
#  正确路径 = hxwj_build_single.sh，产出唯一单包。
#  保留本文件仅作历史对照；下方「整体完整性校验」结论已被推翻：
#  SecShell 实际校验非 META-INF 条目名集合 + CRC 字段，不重算内容 CRC。
# ================================================================
# ================================================================
#  幻想武将模拟器 1.0.8 (com.qxjc.hxwjmlq) — SecShell 兼容分体交付
#
#  ★背景（2026-09-27 真机实测，tombstone/AB 证据）：
#    SecShell（libSecShell.so，梆梆系）对 base.apk 的**非 META-INF 载荷**
#    做整体完整性校验，任何改动都会让校验线程跳飞：
#      signal 7 (SIGBUS) BUS_ADRALN, fault addr 0xa01, pc=0xa01, sp=0, lr=0
#      （崩溃线程名 = 进程名，地址落在 libSecShell.so 的运行时解密段）
#    实测矩阵（每次只改一处，安装后启动）：
#      · 纯重签（只动 META-INF）                     → 通过（游戏正常进 Unity）
#      · 新增 assets/zzz_test.txt                    → SIGBUS
#      · 新增 classes2.dex                          → SIGBUS
#      · 新增 lib/arm64-v8a/libstarry_magic.so      → SIGBUS
#      · 追加 16 字节到 assets/classes0.jar         → SIGBUS
#      · 追加 12 字节到 lib/arm64-v8a/libSecShell.so→ SIGBUS（它连自己都校验）
#    ⇒ 常规「外科 zip 改包」路线在本作被否决。
#
#  ★解法 = 分体（base + split）：
#    base.apk 保持**逐字节原始内容**（只重签），SecShell 校验通过；
#    星光 dex / so / 组件清单放进 split_starry.apk，与 base 同一会话安装。
#    split 的 dex 进同一 PathClassLoader、so 进同一 native lib 路径、
#    provider/service 进合并清单 ⇒ 星光菜单照常启动。
#    旧分体组合实测：RegisterNatives 19/19、BridgeTest PASS=15、菜单悬浮球可拉起。
#
#  ★签名语义：base 与 split 必须同证书。base 由官方 CN=sy 重签为
#    CN=StarryMagic ⇒ 游戏内 xxgame.GlobalGameFun::CheckSignature()
#    会失败并 UnityPlayer.finish()，由星光 dex 内 SignSpoof 回灌 CN=sy
#    证书修复（实测 CheckSignature 调用后进程继续、进入登录页）。
#
#  产出：
#    08_output/幻想武将模拟器/幻想武将模拟器_1.0.8_base-resign.apk
#    08_output/幻想武将模拟器/幻想武将模拟器_1.0.8_split-starry.apk
#
#  用法: bash 04_scripts/幻想武将模拟器/hxwj_build_split.sh
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="幻想武将模拟器"
VERSION="1.0.8"
PKG="com.qxjc.hxwjmlq"

ORIG_APK="${ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_1.0.8_taptap_original.apk}"
ORIG_MD5="${ORIG_MD5:-68ed2632579329c3700559447aba77f0}"

# 星光载荷来源：外科未签名包（含 classes2.dex + so + cert）；缺则退回已签名包
PAYLOAD_UNSIGNED="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-unsigned.apk"
PAYLOAD_SIGNED="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
PAYLOAD="${HXWJ_PAYLOAD:-}"
if [[ -z "$PAYLOAD" ]]; then
  if [[ -f "$PAYLOAD_UNSIGNED" ]]; then PAYLOAD="$PAYLOAD_UNSIGNED"
  elif [[ -f "$PAYLOAD_SIGNED" ]]; then PAYLOAD="$PAYLOAD_SIGNED"
  else echo "ERROR: 缺外科包（先跑 surgical_hxwj_1.0.8.sh）" >&2; exit 1; fi
fi

OUT="$NIXIANG/08_output/$GAME"
BASE_OUT="$OUT/${GAME}_${VERSION}_base-resign.apk"
SPLIT_OUT="$OUT/${GAME}_${VERSION}_split-starry.apk"

KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BT="${BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"
AAPT2="$BT/aapt2"
AJ="${ANDROID_JAR:-$HOME/Library/Android/sdk/platforms/android-35/android.jar}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -f "$PAYLOAD" ]]  || { echo "ERROR: missing $PAYLOAD" >&2; exit 1; }
[[ -x "$AAPT2" ]]    || { echo "ERROR: missing $AAPT2" >&2; exit 1; }
[[ -f "$AJ" ]]       || { echo "ERROR: missing $AJ" >&2; exit 1; }

GOT_MD5="$(md5 -q "$ORIG_APK")"
[[ "$GOT_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 原包 md5=$GOT_MD5 != $ORIG_MD5" >&2; exit 1; }
echo "[split] 底包 = ${ORIG_APK}（md5 ok）"
echo "[split] 载荷 = $PAYLOAD"

WORK="$(mktemp -d /tmp/hxwj_split.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/p/lib/arm64-v8a" "$WORK/p/assets/certs" "$OUT"

# ---------------------------------------------------------------- 1/4 取载荷
echo "[1/4] 从外科包取星光载荷"
unzip -p "$PAYLOAD" classes2.dex > "$WORK/p/classes.dex"
unzip -p "$PAYLOAD" lib/arm64-v8a/libstarry_magic.so > "$WORK/p/lib/arm64-v8a/libstarry_magic.so"
unzip -p "$PAYLOAD" lib/arm64-v8a/libc++_shared.so   > "$WORK/p/lib/arm64-v8a/libc++_shared.so"
unzip -p "$PAYLOAD" assets/certs/starlight.crt       > "$WORK/p/assets/certs/starlight.crt"

for t in "com/starry/magic/StarryInitProvider" "com/starry/magic/menu/MenuService" \
         "com/starry/magic/sign/StarryAppComponentFactory" "com/starry/magic/sign/TapLoginForceWeb"; do
  LC_ALL=C grep -aqF "$t" "$WORK/p/classes.dex" || { echo "ERROR: dex 缺 $t" >&2; exit 1; }
done
LC_ALL=C grep -aqF -- '[hxwj]' "$WORK/p/lib/arm64-v8a/libstarry_magic.so" \
  || { echo "ERROR: so 不是 Hxwj 版" >&2; exit 1; }
if LC_ALL=C grep -aqF -- '[Gyzw]' "$WORK/p/lib/arm64-v8a/libstarry_magic.so"; then
  echo "ERROR: so 含 [Gyzw]（别家顶包）" >&2; exit 1
fi
echo "  classes.dex=$(stat -f%z "$WORK/p/classes.dex") so=$(stat -f%z "$WORK/p/lib/arm64-v8a/libstarry_magic.so")"

# ---------------------------------------------------------------- 2/4 split
echo "[2/4] 组装 split_starry.apk（dex + so + provider/service）"
cat > "$WORK/AndroidManifest.xml" <<'XML'
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.qxjc.hxwjmlq"
    split="starry"
    android:versionCode="8"
    android:versionName="1.0.8">
    <uses-sdk android:minSdkVersion="22" android:targetSdkVersion="35" />
    <uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE" />
    <application android:hasCode="true" android:extractNativeLibs="true">
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.qxjc.hxwjmlq.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
    </application>
</manifest>
XML
"$AAPT2" link -o "$WORK/split-unsigned.apk" --manifest "$WORK/AndroidManifest.xml" \
  -I "$AJ" --min-sdk-version 22 --target-sdk-version 35
(cd "$WORK/p" && zip -X -q "$WORK/split-unsigned.apk" \
  classes.dex lib/arm64-v8a/libstarry_magic.so lib/arm64-v8a/libc++_shared.so assets/certs/starlight.crt)
"$BT/zipalign" -f -p 4 "$WORK/split-unsigned.apk" "$WORK/split-aligned.apk"
rm -f "$SPLIT_OUT" "$SPLIT_OUT.idsig"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SPLIT_OUT" "$WORK/split-aligned.apk"
"$BT/apksigner" verify "$SPLIT_OUT" >/dev/null || { echo "ERROR: split 签名校验失败" >&2; exit 1; }

# ---------------------------------------------------------------- 3/4 base
echo "[3/4] 重签 base（★内容逐字节原始）"
"$BT/zipalign" -f -p 4 "$ORIG_APK" "$WORK/base-aligned.apk"
rm -f "$BASE_OUT" "$BASE_OUT.idsig"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$BASE_OUT" "$WORK/base-aligned.apk"
"$BT/apksigner" verify "$BASE_OUT" >/dev/null || { echo "ERROR: base 签名校验失败" >&2; exit 1; }

# ---------------------------------------------------------------- 4/4 硬校验
echo "[4/4] 硬校验：base 非 META-INF 载荷必须逐字节 == 原包"
python3.11 - "$ORIG_APK" "$BASE_OUT" <<'PY'
import sys, zipfile, hashlib
orig, base = sys.argv[1], sys.argv[2]
zo, zb = zipfile.ZipFile(orig), zipfile.ZipFile(base)

def payload(z):
    out = {}
    for i in z.infolist():
        n = i.filename
        if n.startswith("META-INF/"):
            continue
        out[n] = hashlib.sha256(z.read(n)).hexdigest()
    return out

po, pb = payload(zo), payload(zb)
missing = sorted(set(po) - set(pb))
added   = sorted(set(pb) - set(po))
changed = sorted(n for n in set(po) & set(pb) if po[n] != pb[n])
print(f"  payload entries orig={len(po)} base={len(pb)}")
if missing or added or changed:
    for label, items in (("MISSING", missing), ("ADDED", added), ("CHANGED", changed)):
        for n in items[:10]:
            print(f"  {label}: {n}")
    raise SystemExit("ERROR: base 载荷与原包不一致（SecShell 会 SIGBUS）")
print("  OK: base 非 META-INF 载荷与原包逐字节一致")
PY

echo
echo "[split] done"
echo "  base  = $BASE_OUT  md5=$(md5 -q "$BASE_OUT")  size=$(stat -f%z "$BASE_OUT")"
echo "  split = $SPLIT_OUT md5=$(md5 -q "$SPLIT_OUT") size=$(stat -f%z "$SPLIT_OUT")"
echo "  下一步: bash 04_scripts/幻想武将模拟器/hxwj_install.sh"
