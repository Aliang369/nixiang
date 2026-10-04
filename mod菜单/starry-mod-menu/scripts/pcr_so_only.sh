#!/usr/bin/env bash
# 公主连结：只重编 so，并 zip 进「外科包」（禁止 apktool 整包重建）
#
# 教训 2026-08-14：pcr_so_only 用 apktool b 会改 arsc/受保护 dex，TSS ~1.5s 杀进程；
# 误判成属性 hook。正确路径：以外科包为底，仅替换 libstarry_magic.so 后重签。
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/pcr_so_only.sh
#   STARRY_BASE_APK=... bash scripts/pcr_so_only.sh   # 可选覆盖底包
#
# 禁止默认卸载本包（热更约 10GB）；只 cover install。
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT="$NIXIANG/08_output/公主连结"
PKG="com.bilibili.priconne"
BASE="${STARRY_BASE_APK:-$OUT/公主连结_11.7.2_surgical-releaseSigned.apk}"
SIGNED="$OUT/公主连结_11.7.2_surgical-soOnly-releaseSigned.apk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_PCR' "$MK" || { echo "ERROR: enable STARRY_GAME_PCR in Android.mk"; exit 1; }
[[ -f "$BASE" ]] || { echo "ERROR: missing base surgical apk: $BASE"; exit 1; }

APP="$STARRY_SRC/app"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8

SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: ndk-build did not produce $SO"; exit 1; }

# jniLibs 强制刷新（gradle 不编 native；漏拷 = 下次打错游戏）
mkdir -p "$APP/src/main/jniLibs/arm64-v8a" "$APP/src/main/libs/arm64-v8a" \
         "$APP/src/libs/arm64-v8a" "$APP/libs/arm64-v8a"
cp -f "$SO" "$APP/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$APP/src/main/libs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$APP/src/libs/arm64-v8a/libstarry_magic.so"
cp -f "$SO" "$APP/libs/arm64-v8a/libstarry_magic.so"

# 装机前自检：必须是 PCR，禁止残留上一款标签
if ! strings "$SO" | grep -q '\[Pcr\]'; then
  echo "ERROR: so missing [Pcr] tag"; exit 1
fi
if strings "$SO" | grep -qE '\[Gjc2\]|\[Dwct\]|\[Ltzj\]'; then
  echo "ERROR: so still contains other-game tag"; exit 1
fi

WORK="$(mktemp -d /tmp/pcr_so_only.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
cp -f "$BASE" "$WORK/in.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -u in.apk lib/arm64-v8a/libstarry_magic.so)

KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
java -jar "$SIGNER" -a "$WORK/in.apk" --overwrite --allowResign \
  --ks "$KS" --ksAlias starry --ksPass starry2026 --ksKeyPass starry2026

mkdir -p "$OUT"
if [[ -f "$WORK/in-aligned-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-aligned-releaseSigned.apk" "$SIGNED"
elif [[ -f "$WORK/in-releaseSigned.apk" ]]; then
  cp -f "$WORK/in-releaseSigned.apk" "$SIGNED"
else
  cp -f "$WORK/in.apk" "$SIGNED"
fi
# also refresh canonical surgical-soFix name used in docs
cp -f "$SIGNED" "$OUT/公主连结_11.7.2_surgical-soFix-releaseSigned.apk"
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb push "$SIGNED" /data/local/tmp/starry_pcr_so.apk
  adb shell "su -c \"pm install -r -d --user 0 /data/local/tmp/starry_pcr_so.apk\""
  if [[ -f "$NIXIANG/04_scripts/_common/starry_post_install.sh" ]]; then
    # shellcheck disable=SC1091
    source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
    starry_post_install "$PKG" || true
  fi
fi
echo "done → $SIGNED (base=$(basename "$BASE"); NO apktool)"
