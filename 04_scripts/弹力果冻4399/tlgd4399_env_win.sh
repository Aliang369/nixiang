#!/usr/bin/env bash
# =============================================================================
#  弹力果冻 4399（com.ginicat.sweet.m4399）—— Windows(MSYS/Git Bash) 出包环境入口
#
#  用法：source 04_scripts/弹力果冻4399/tlgd4399_env_win.sh
#        bash 04_scripts/弹力果冻4399/tlgd4399_build_so.sh
#        bash 04_scripts/弹力果冻4399/tlgd4399_surgical.sh
#        bash 04_scripts/弹力果冻4399/tlgd4399_install.sh
#
#  为什么需要（2026-10-26 雷电模拟器轮）：
#   本游戏脚本按 mac 习惯写成（`md5 -q` / `stat -f%z` / `~/Library/Android/sdk` / `sysctl`），
#   Windows 侧没有这些；且 jni 目录含中文（`mod菜单`），ndk-build 直接 `-C` 到中文路径会失败。
#   本文件只做三件事（不改脚本语义，参照 04_scripts/勇士之心/ysxz_env_win.sh）：
#     ① macOS 命令垫片（md5 -q / stat -f%z / stat -f%m）
#     ② NDK shim（SDK 的 NDK 无顶层 ndk-build；并把 NDK 暴露成 /tmp 下 ASCII 路径）
#     ③ 工具 PATH（apktool / smali / platform-tools / build-tools）+ JAVA_HOME + PYTHONUTF8
#  路径若与本机不同，改下面 3 个默认值即可，不要改脚本。
# =============================================================================
export NIXIANG="${NIXIANG:-/e/nixiang}"
export JAVA_HOME="${JAVA_HOME:-/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot}"
export ANDROID_HOME="${ANDROID_HOME:-/c/Users/Administrator/AppData/Local/Android/Sdk}"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export ANDROID_SERIAL="${ANDROID_SERIAL:-emulator-5554}"
export PYTHONUTF8=1
export LC_ALL="${LC_ALL:-C.UTF-8}"

# ---- 底包 / 构建参数（本轮：雷电模拟器上已装 4399 官方原版）----
export TLGD4399_BASE="${TLGD4399_BASE:-$NIXIANG/02_apks/弹力果冻4399/弹力果冻4399_2.4.0_vc4412_雷电原版_20261026.apk}"
export TLGD4399_SERIAL="${TLGD4399_SERIAL:-emulator-5554}"

# ---- NDK shim：顶层 ndk-build + ASCII 别名路径 ----
_tlgd_ndk_sdk="${TLGD_NDK_SDK:-$ANDROID_HOME/ndk/27.0.12077973}"
_tlgd_ndk="${TLGD_NDK:-/tmp/tlgd_ndk}"
mkdir -p "$_tlgd_ndk" 2>/dev/null
for _e in build meta prebuilt python-packages shader-tools simpleperf sources toolchains; do
  if [ -e "$_tlgd_ndk_sdk/$_e" ] && [ ! -e "$_tlgd_ndk/$_e" ]; then
    ln -s "$_tlgd_ndk_sdk/$_e" "$_tlgd_ndk/$_e" 2>/dev/null || true
  fi
done
if [ ! -x "$_tlgd_ndk/ndk-build" ]; then
  {
    echo '#!/usr/bin/env bash'
    echo "export GNUMAKE=\"$_tlgd_ndk_sdk/prebuilt/windows-x86_64/bin/make.exe\""
    echo "exec \"$_tlgd_ndk_sdk/build/ndk-build\" \"\$@\""
  } > "$_tlgd_ndk/ndk-build"
  chmod +x "$_tlgd_ndk/ndk-build" 2>/dev/null || true
fi
export ANDROID_NDK_HOME="$_tlgd_ndk"
unset _tlgd_ndk_sdk _tlgd_ndk _e

# ---- build-tools ----
_tlgd_bt=""
for v in 36.1.0 36.0.0 35.0.0 34.0.0; do
  if [ -e "$ANDROID_HOME/build-tools/$v/zipalign.exe" ] || [ -e "$ANDROID_HOME/build-tools/$v/zipalign" ]; then
    _tlgd_bt="$ANDROID_HOME/build-tools/$v"; break
  fi
done
export TLGD_BT="$_tlgd_bt"
export AAPT2="${AAPT2:-$_tlgd_bt/aapt2.exe}"
unset _tlgd_bt

# ---- macOS 命令垫片：md5 / stat -f ----
_tlgd_shim="${TLGD_SHIM:-/tmp/tlgd_bin}"
mkdir -p "$_tlgd_shim" 2>/dev/null
cat > "$_tlgd_shim/md5" <<'SH'
#!/usr/bin/env bash
if [ "${1:-}" = "-q" ]; then shift; md5sum "$@" | awk '{print $1}'; exit 0; fi
exec md5sum "$@"
SH
cat > "$_tlgd_shim/stat" <<'SH'
#!/usr/bin/env bash
case "${1:-}" in
  -f%z) shift; for f in "$@"; do printf '%s\n' "$(wc -c < "$f" | tr -d ' ')"; done; exit 0 ;;
  -f%m) shift; for f in "$@"; do printf '%s\n' "$(date -r "$f" +%s 2>/dev/null || echo 0)"; done; exit 0 ;;
esac
exec /usr/bin/stat "$@"
SH
chmod +x "$_tlgd_shim/md5" "$_tlgd_shim/stat" 2>/dev/null || true
export PATH="$_tlgd_shim:$NIXIANG/01_tools/apktool:$NIXIANG/01_tools/smali:$ANDROID_HOME/platform-tools:${TLGD_BT:-}:$JAVA_HOME/bin:$PATH"
unset _tlgd_shim

echo "[tlgd4399_env_win] NIXIANG=$NIXIANG"
echo "[tlgd4399_env_win] NDK=$ANDROID_NDK_HOME"
echo "[tlgd4399_env_win] BT=${TLGD_BT:-<none>} JAVA_HOME=$JAVA_HOME SERIAL=$ANDROID_SERIAL"
echo "[tlgd4399_env_win] BASE=$TLGD4399_BASE"
