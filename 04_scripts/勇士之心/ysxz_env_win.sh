#!/usr/bin/env bash
# =============================================================================
#  勇士之心 1.0.6 —— Windows(MSYS/Git Bash) 出包环境入口
#
#  用途：`source 04_scripts/勇士之心/ysxz_env_win.sh` 之后再跑出包链，命令与 mac 端一致：
#      python3.11 04_scripts/勇士之心/ysxz_anticheat/build_ac_patch.py
#      python3.11 04_scripts/勇士之心/gen_ysxz_boot_js.py --patch-js 04_scripts/勇士之心/ysxz_anticheat/patch_v2_ac.js
#      python3.11 04_scripts/勇士之心/ysxz_mainjs_patch.py --apk 09_test/勇士之心/base_1.0.6/base_official_1.0.6.apk \
#          --boot-js mod菜单/starry-mod-menu/libs_ysxz/ysxz_boot.js \
#          --out mod菜单/starry-mod-menu/libs_ysxz/main.patched.enc.js \
#          --header mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzBootBlob.h
#      bash 04_scripts/勇士之心/ysxz_build_so.sh
#      bash mod菜单/starry-mod-menu/scripts/surgical_ysxz_1.0.6.sh
#      VERSION=1.0.6 bash 04_scripts/勇士之心/ysxz_sign.sh
#      ALLOW_UNINSTALL=1 VERSION=1.0.6 SERIAL=emulator-5554 bash 04_scripts/勇士之心/install_ysxz.sh
#
#  说明（2026-10-25 雷电模拟器轮实测打通；原先这些变量散在会话临时脚本里，收尾时收进本仓）：
#   - `md5` 垫片：本仓脚本按 mac 习惯调 `md5 -q`，Windows 只有 `md5sum` ⇒ 运行时生成一个
#     临时 shim 目录塞进 PATH，不改脚本。
#   - NDK：直接用 SDK 里的 27.0.12077973；`ysxz_build_so.sh` 读 `ANDROID_NDK_HOME`。
#   - 中文路径：`ysxz_build_so.sh` 的 Windows 分支会把 jni 拷到 `$YSXZ_WIN_STAGE`（默认
#     `C:/ysxz_build_stage`）再调 `ndk-build.cmd`，避开 cmd.exe 的中文路径问题。
#   - `YSXZ_SKIP_GEN=1`：构建阶段不再自动重生成 JS 头（本脚本上方三步已显式生成）。
#   - 路径若与本机不同（JDK/SDK 版本），改下面 3 个默认值即可，不要改脚本。
# =============================================================================
export NIXIANG="${NIXIANG:-/e/nixiang}"
export JAVA_HOME="${JAVA_HOME:-/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot}"
export ANDROID_HOME="${ANDROID_HOME:-/c/Users/Administrator/AppData/Local/Android/Sdk}"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
# NDK shim：SDK 的 NDK 没有顶层 `ndk-build`（只有 build/ndk-build 与 build/ndk-build.cmd），
# 而 `ysxz_build_so.sh` 用 `[[ -x $ANDROID_NDK_HOME/ndk-build ]]` 做存在性判据 ⇒ 在临时目录
# 里拼一个「顶层 ndk-build + 各子目录软链」的 shim NDK（不动 SDK，也不依赖会话临时目录）。
_ysxz_ndk_sdk="${YSXZ_NDK_SDK:-$ANDROID_HOME/ndk/27.0.12077973}"
_ysxz_ndk="${TMPDIR:-/tmp}/ysxz_ndk"
mkdir -p "$_ysxz_ndk" 2>/dev/null
for _e in build meta prebuilt python-packages shader-tools simpleperf sources toolchains wrap.sh; do
  if [ -e "$_ysxz_ndk_sdk/$_e" ] && [ ! -e "$_ysxz_ndk/$_e" ]; then
    ln -s "$_ysxz_ndk_sdk/$_e" "$_ysxz_ndk/$_e" 2>/dev/null || true
  fi
done
if [ ! -x "$_ysxz_ndk/ndk-build" ]; then
  {
    echo '#!/usr/bin/env bash'
    echo "export GNUMAKE=\"$_ysxz_ndk_sdk/prebuilt/windows-x86_64/bin/make.exe\""
    echo "exec \"$_ysxz_ndk_sdk/build/ndk-build\" \"\$@\""
  } > "$_ysxz_ndk/ndk-build"
  chmod +x "$_ysxz_ndk/ndk-build" 2>/dev/null || true
fi
export ANDROID_NDK_HOME="$_ysxz_ndk"
unset _ysxz_ndk_sdk _ysxz_ndk _e
export AAPT2="${AAPT2:-$ANDROID_HOME/build-tools/35.0.0/aapt2.exe}"
export YSXZ_MENU_SRC="$NIXIANG/04_scripts/勇士之心/ysxz_1.0.3_menu"
export YSXZ_SKIP_GEN=1
export YSXZ_SO="$NIXIANG/mod菜单/starry-mod-menu/libs_ysxz/arm64-v8a/libstarry_magic.so"
export ANDROID_SERIAL="${ANDROID_SERIAL:-emulator-5554}"
export PYTHONUTF8=1

# md5 垫片（mac 的 `md5 -q` ↔ Windows 的 `md5sum`）
_ysxz_shim="${TMPDIR:-/tmp}/ysxz_bin"
mkdir -p "$_ysxz_shim" 2>/dev/null
if [ ! -x "$_ysxz_shim/md5" ]; then
  printf '%s\n' '#!/usr/bin/env bash' \
    'if [ "$1" = "-q" ]; then shift; exec md5sum "$@" | awk "{print \$1}"; fi' \
    'exec md5sum "$@"' > "$_ysxz_shim/md5"
  chmod +x "$_ysxz_shim/md5"
fi

export PATH="$_ysxz_shim:$NIXIANG/01_tools/apktool:$NIXIANG/01_tools/smali:$ANDROID_HOME/platform-tools:$PATH"
unset _ysxz_shim
echo "[ysxz_env_win] NIXIANG=$NIXIANG"
echo "[ysxz_env_win] NDK=$ANDROID_NDK_HOME JAVA_HOME=$JAVA_HOME"
