#!/usr/bin/env bash
# 境界守望者（com.ap.watcherofrealms 国际服）管线 build_so 阶段后端：私有 Android_wor.mk 直编
# 产物 = mod菜单/starry-mod-menu/libs_wor/arm64-v8a/libstarry_magic.so
#
# 母本：04_scripts/潮汐守望者/cxswz_build_so.sh
#
# 阶段（2026-10-03）：e2 功能阶段（WOR_ENV_STAGE=0）= 无敌 + 伤害倍率，真机验收要看到
#   `[Wor] lib ready (liblogic.so, feature stage) hooked=N tag=wor-e2-20261004b`。
# C1 热重载（2026-10-04 加）：wor_cfg.txt 每 cfg_poll_ms（默认 2000ms）轮询一次，mtime/size 变了
#   就整体重读 ⇒ 改配置连重启游戏都不用。重载日志锚点 `[Wor] cfg-reload`。
# 诊断开关（C2，2026-10-04 加）：STARRY_WOR_DIAG=1（默认）= 排障构建；=0 = 正式构建
#   （S1~S6 诊断与 S3 扫描整段编掉 ⇒ Android_wor.mk 翻成 -DWOR_DIAG_FULL=）。两种构建
#   so 里的 tier 串分别是 `diag-tier=FULL` / `diag-tier=RELEASE`，本脚本按 so 内容核对（见文末）。
# 历史：e1 环境阶段（WOR_ENV_STAGE=1）= kSlots 空表 = 0 hook，仅壳 + 菜单 + 去签名 + 官证回灌；
#   本脚本的阶段自检按 so 内容自动翻转（见文末），历史标签 wor-e1-20261003 已不在自检清单内。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC_REAL="$NIXIANG/mod菜单/starry-mod-menu"

# ---------- ASCII 路径垫片（Windows 上 ndk/make 处理非 ASCII 路径会乱码） ----------
# 本仓库的 mod菜单/ 含中文；ndk-build 的 make 在 Windows 下会把非 ASCII 路径打乱，
# 导致 clang 收到错路径或 make 报 "The system cannot find the file specified."。
# 解决：建一个纯 ASCII 的目录 junction（starry_link）指向真实项目，构建全程走 junction。
#
# ★ 2026-10-04 修（真机复现）：原来写的是 `MSYS2_ARG_CONV_EXCL='*' cmd //c mklink ...`。
#   `//c` 只有在**参数转换开启**时才会被 MSYS 折成 `/c`；而同一行又设了
#   MSYS2_ARG_CONV_EXCL='*'（关掉全部转换）⇒ cmd 收到字面 `//c`，不认识，
#   直接进交互式 shell 并立即退出、rc=0。加 `>/dev/null 2>&1 || true` 后
#   **失败被完全吞掉**，junction 从未创建，脚本继续用中文路径 ⇒ ndk-build 报
#   一堆 `error: unable to open output file 'E:/nixiang/mod<乱码>/...obj_wor/....o'`，
#   根因（junction 没建成）看起来毫不相关。修法：转换关掉时开关要写 `/c` 而不是 `//c`；
#   并且**创建失败必须致命退出**（继续跑只会得到误导性的 clang 错误）。
STARRY_SRC="$STARRY_SRC_REAL"
case "$STARRY_SRC_REAL" in
  *[!\ -~]*)
    LINK="$NIXIANG/starry_link"
    if [[ ! -d "$LINK" ]]; then
      # 注意：这一行刻意不加引号保护之外的任何转义；MSYS2_ARG_CONV_EXCL='*' 必须在
      # 命令前以「临时环境变量赋值」形式给出，且开关写成 /c、/J（单斜杠）。
      MSYS2_ARG_CONV_EXCL='*' cmd /c mklink /J "$(cygpath -w "$LINK")" "$(cygpath -w "$STARRY_SRC_REAL")" || true
    fi
    if [[ -d "$LINK" ]]; then
      STARRY_SRC="$LINK"
      echo "[ascii] 走 ASCII junction: $LINK -> $STARRY_SRC_REAL"
    else
      echo "ERROR: 无法创建 ASCII junction $LINK -> $STARRY_SRC_REAL" >&2
      echo "       本作项目路径含非 ASCII（mod菜单/），Windows 下 ndk-build 走中文路径必失败。" >&2
      echo "       手工修（Git Bash 里跑，注意 /c 与 /J 是单斜杠、前缀必须带 MSYS2_ARG_CONV_EXCL）：" >&2
      echo "         MSYS2_ARG_CONV_EXCL='*' cmd /c mklink /J 'E:\\\\nixiang\\\\starry_link' '<真项目路径>'" >&2
      exit 1
    fi
    ;;
esac
JNI="$STARRY_SRC/app/src/main/jni"

# ---------- NDK 探测（macOS / Linux / Windows Git Bash 三平台） ----------
# 判定标准：该目录下必须有 ndk-build 或 ndk-build.cmd
_ndk_has_build() { [[ -x "$1/ndk-build" || -f "$1/ndk-build.cmd" ]]; }
_ndk_resolve() {  # $1 = 可能是 sdk 根 / ndk 目录 / 具体版本目录
  local base="$1"
  _ndk_has_build "$base" && { echo "$base"; return; }
  # 具体版本优先 27.0.12077973（与母本一致），否则取第一个可用的
  local v
  for v in 27.0.12077973; do
    _ndk_has_build "$base/$v" && { echo "$base/$v"; return; }
    _ndk_has_build "$base/ndk/$v" && { echo "$base/ndk/$v"; return; }
  done
  local c
  for c in "$base"/ndk/* "$base"/*; do
    _ndk_has_build "$c" && { echo "$c"; return; }
  done
}

WUSER="${USER:-${USERNAME:-Administrator}}"
NDK=""
for cand in "${ANDROID_NDK_HOME:-}" "${ANDROID_NDK_ROOT:-}" \
            "${ANDROID_SDK_ROOT:-}" "${ANDROID_HOME:-}" \
            "$HOME/Library/Android/sdk" "$HOME/Android/Sdk" \
            "/c/Users/$WUSER/AppData/Local/Android/Sdk" \
            "/mnt/c/Users/$WUSER/AppData/Local/Android/Sdk"; do
  [[ -n "$cand" && -d "$cand" ]] || continue
  NDK="$(_ndk_resolve "$cand")"
  [[ -n "$NDK" ]] && break
done

if _ndk_has_build "${NDK:-/nonexistent}"; then
  if [[ -x "$NDK/ndk-build" ]]; then NDK_BUILD="$NDK/ndk-build"
  else NDK_BUILD="$NDK/ndk-build.cmd"; fi
else
  echo "ERROR: ndk-build 未找到（设 ANDROID_NDK_HOME=/path/to/ndk）" >&2
  exit 1
fi
echo "[ndk] $NDK_BUILD"
[[ -f "$JNI/Android_wor.mk" ]] || { echo "ERROR: missing $JNI/Android_wor.mk" >&2; exit 1; }

# ---------- 构建输出目录：必须纯 ASCII，且**不能挂在 junction 下**（2026-10-04 修） ----------
# 真机复现：NDK_OUT 原为 $STARRY_SRC/obj_wor（= /e/nixiang/starry_link/obj_wor）。
# 但 cygpath 会把 junction **解析回真实中文路径**（实测：
#   cygpath -w /e/nixiang/starry_link/obj_wor → E:\nixiang\mod菜单\starry-mod-menu\obj_wor），
# ndk-build 的 make（cmd.exe 子壳）与 clang 对该非 ASCII 路径的编码处理不一致 ⇒
# 一边在 `mod菜单/…` 建目录、另一边去 `mod<乱码>/…` 写 .o，报一堆
#   error: unable to open output file 'E:/nixiang/mod<乱码>/…/xdl.o': 'No such file or directory'
# （两条日志其实是同一路径的两种乱码形态）。
# ⇒ 结论：junction 只能救 `-C`（进 jni 目录），**救不了输出路径**。
#   故 obj/libs 全部落到仓库根下的纯 ASCII 目录 .wor_build/（已 gitignore），
#   编完再由 bash `cp` 回填契约路径（bash 处理 UTF-8 路径正常，只是原生 win 程序不行）。
BUILD_TMP="$NIXIANG/.wor_build"
OBJ="$BUILD_TMP/obj"
OUT_LIBS="$BUILD_TMP/libs"
SO_BUILD="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
# 下游契约路径（**不变**，surgical/sign/install/gradle 都按这个取 so）
SO="$STARRY_SRC_REAL/libs_wor/arm64-v8a/libstarry_magic.so"

# ---------- 可移植垫片 ----------
md5_of() {
  if command -v md5 >/dev/null 2>&1; then md5 -q "$1"
  else md5sum "$1" | awk '{print $1}'; fi
}
size_of() {
  if stat -c%s "$1" >/dev/null 2>&1; then stat -c%s "$1"
  else stat -f%z "$1"; fi
}
sha256_of_stdin() {
  if command -v sha256sum >/dev/null 2>&1; then sha256sum | awk '{print $1}'
  else shasum -a 256 | awk '{print $1}'; fi
}
ncpu() {
  if command -v nproc >/dev/null 2>&1; then nproc
  elif command -v sysctl >/dev/null 2>&1; then sysctl -n hw.ncpu 2>/dev/null || echo 8
  else echo "${NUMBER_OF_PROCESSORS:-8}"; fi
}

# 注意：OBJ / OUT_LIBS / SO 已在上面「构建输出目录」段定义（纯 ASCII .wor_build/）。
# 这里**不要**再赋值 —— 曾经留过一行 `OBJ="$STARRY_SRC/obj_wor"` 把 ASCII 目录覆盖回中文路径，
# 结果编译报的仍然是 mod<乱码>/obj_wor 的错，看起来像"改了没用"。
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / trace 开关变化 → 清 obj 强制全量重编 ----
CFG_HASH="$( (cat "$JNI/Android_wor.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}"; \
  printf 'WOR_DIAG=%s\n' "${STARRY_WOR_DIAG:-1}") \
  | sha256_of_stdin)"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 .wor_build/obj 强制全量重编"
  rm -rf "$OBJ"
fi

# NDK_OUT/NDK_LIBS_OUT 必须绝对路径：相对路径会被 -C 的 cwd 解析到 app/ 下（假成功教训）
SO_MD5_BEFORE="$(md5_of "$SO" 2>/dev/null || echo none)"
# ---- STARRY_BUILD_EPOCH 必须在命令行强制传入纯数字 ----
# Application.mk 里 `STARRY_BUILD_EPOCH := $(shell git log -1 --format=%ct 2>/dev/null || date +%s ...)`
# 在 Windows（ndk-build 用 cmd.exe 当 shell）会退化成整个 git commit 文本（含空格 / <email> / 中文），
# 使 -DSTARRY_BUILD_EPOCH=... 裂成多参，其中 `...@users.noreply.github.com>` 里的 > 被 cmd 当重定向，
# 编译直接报 "The system cannot find the file specified."。macOS 上 sh 正常所以母本没暴露。
EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null || true)"
[[ "$EPOCH" =~ ^[0-9]+$ ]] || EPOCH="$(date +%s 2>/dev/null || echo 1790184000)"
[[ "$EPOCH" =~ ^[0-9]+$ ]] || EPOCH=1790184000
echo "[epoch] STARRY_BUILD_EPOCH=$EPOCH"

# ---- C2 诊断档位：只接受 0 / 1，其它值一律回落到 1（宁可出排障构建，也不要混档） ----
DIAG="${STARRY_WOR_DIAG:-1}"
case "$DIAG" in
  0|1) ;;
  *) echo "WARN: STARRY_WOR_DIAG='$DIAG' 非法（只接受 0/1）→ 按 1 排障构建处理" >&2; DIAG=1 ;;
esac
if [[ "$DIAG" == "0" ]]; then
  echo "[diag] STARRY_WOR_DIAG=0 → 正式构建（S1~S6 诊断 + S3 扫描整段编掉）"
else
  echo "[diag] STARRY_WOR_DIAG=$DIAG → 排障构建（诊断全编入）"
fi
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK_BUILD" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_wor.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  STARRY_BUILD_EPOCH="$EPOCH" \
  STARRY_WOR_DIAG="$DIAG" \
  -j"$(ncpu)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO_BUILD" ]] || { echo "ERROR: build produced no $SO_BUILD" >&2; exit 1; }

# ---- 回填契约路径（ASCII 构建目录 → mod菜单/…/libs_wor） ----
# 用 cp 而不是 mv：保留 .wor_build/obj 供下次增量编译。
mkdir -p "$(dirname "$SO")" || { echo "ERROR: 无法创建 $(dirname "$SO")" >&2; exit 1; }
cp -f "$SO_BUILD" "$SO" || { echo "ERROR: 无法回填 $SO" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: 回填后 $SO 不存在" >&2; exit 1; }
echo "[so] $SO_BUILD → $SO ($(size_of "$SO") B)"

# ---- 链接活动保险：有新 .o 但 so 字节没变 → 旧 so 顶包嫌疑 ----
if [[ -n "$NEW_O" && "$(md5_of "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 归属防呆：本作串必须在、别家标签必须不在（跨会话共享目录）----
# 中文 UTF-8 多字节，strings 默认 ASCII 会漏报 ⇒ LC_ALL=C grep -a 按字节搜
for s in "Category_战斗" "Toggle_无敌" "Slider_伤害倍率:1:10:1" "[Wor]" "liblogic.so" "wor-e2-20261004"; do
  n="$(LC_ALL=C grep -acF -- "$s" "$SO" || true)"
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含本作串 ${s}（旧 so 混入或源码未编译）" >&2; exit 1; }
done
for s in "[Cxswz]" "[Jjdm]" "[Gyzw]" "[Hxwj]" "[Blfx]" "[Sjzz]" "[WbjY]" "[Wjtxd]"; do
  # -F：标签里的 [] 是字面字符，否则会被 grep 当字符类（曾假报含别家标签）
  if LC_ALL=C grep -aqF -- "$s" "$SO"; then
    echo "ERROR: $SO 含别家标签 ${s}（libs_wor 被并行会话覆盖）" >&2
    exit 1
  fi
done

# ---- 阶段自检（环境阶段 / 功能阶段二选一，按源码实际状态自动判定）----
# 判据用 "ENV stage e1"（TryInit 里一定会执行到的日志）。
# 不要用 InstallBattleHooks 里那句 "env stage 0-hook: kSlots disabled"：
#   环境阶段该函数从不被调用，-ffunction-sections + --gc-sections 会把它整段连同
#   它的字符串字面量一起回收，so 里查不到（实测 confirmed）。
SO_ENV_N="$(LC_ALL=C grep -acF -- "ENV stage e1" "$SO" || true)"
SO_FEAT_N="$(LC_ALL=C grep -acF -- "feature stage" "$SO" || true)"
if [[ "$SO_ENV_N" -ge 1 ]]; then
  echo "[stage] 环境阶段 so（WOR_ENV_STAGE=1，0 hook）"
  [[ "$SO_FEAT_N" == "0" ]] \
    || { echo "ERROR: 环境阶段 so 里混进了功能阶段标记（feature stage）" >&2; exit 1; }
  for s in "DMG x%d" "INV e=%p"; do
    LC_ALL=C grep -aqF -- "$s" "$SO" \
      && { echo "ERROR: 环境阶段 so 里混进了功能阶段日志（${s}）" >&2; exit 1; }
  done
  if LC_ALL=C grep -aqF -- "Slider_伤害倍率:1:100:1" "$SO"; then
    echo "ERROR: $SO 仍含旧 1:100 滑条" >&2; exit 1
  fi
else
  echo "[stage] 功能阶段 so"
  LC_ALL=C grep -aqF -- "feature stage" "$SO" \
    || { echo "ERROR: $SO 未见功能阶段标记（源码未切到功能阶段）" >&2; exit 1; }
  LC_ALL=C grep -aqF -- "DMG x%d" "$SO" \
    || { echo "ERROR: $SO 未见伤害倍率改写日志（功能未编译进去）" >&2; exit 1; }
  LC_ALL=C grep -aqF -- "INV e=%p" "$SO" \
    || { echo "ERROR: $SO 未见无敌改写日志（功能未编译进去）" >&2; exit 1; }
  LC_ALL=C grep -aqF -- "Slider_伤害倍率:1:10:1" "$SO" \
    || { echo "ERROR: $SO 伤害倍率滑条不是 1:10（菜单串与需求不一致）" >&2; exit 1; }
fi

# ---- C2 诊断档位自检（so 里的档位串必须与构建请求一致，杜绝混档 so 出厂）----
# 排障独有串（WOR_DIAG_FULL=0 时整段编掉 ⇒ so 里必须一个都搜不到）：
#   `===== SELFTEST =====` / `===== SECTION 1: images =====` / `===== SECTION 3: structural-scan =====`
# 不要用 "HpChangeEntitySystem" 当判据：功能代码 FindSysClass 也引用它，两种档位都在。
if [[ "$DIAG" == "0" ]]; then
  LC_ALL=C grep -aqF -- "diag-tier=RELEASE" "$SO" \
    || { echo "ERROR: $SO 未见 diag-tier=RELEASE（正式构建标记缺失）" >&2; exit 1; }
  if LC_ALL=C grep -aqF -- "diag-tier=FULL" "$SO"; then
    echo "ERROR: $SO 同时含 diag-tier=FULL —— 诊断未编掉（混档 so）" >&2; exit 1
  fi
  for s in "===== SELFTEST =====" "===== SECTION 1: images =====" "===== SECTION 3: structural-scan ====="; do
    if LC_ALL=C grep -aqF -- "$s" "$SO"; then
      echo "ERROR: 正式构建 so 里仍有诊断串 ${s}（C2 开关未生效）" >&2; exit 1
    fi
  done
  echo "[diag] 自检通过：RELEASE 档（诊断串已全部编掉）"
else
  LC_ALL=C grep -aqF -- "diag-tier=FULL" "$SO" \
    || { echo "ERROR: $SO 未见 diag-tier=FULL（排障构建标记缺失）" >&2; exit 1; }
  if LC_ALL=C grep -aqF -- "diag-tier=RELEASE" "$SO"; then
    echo "ERROR: $SO 含 diag-tier=RELEASE —— 档位与请求不符" >&2; exit 1
  fi
  LC_ALL=C grep -aqF -- "===== SELFTEST =====" "$SO" \
    || { echo "ERROR: 排障构建 so 里无 SELFTEST 串（诊断未编入）" >&2; exit 1; }
  echo "[diag] 自检通过：FULL 档（诊断已编入）"
fi

# ---- 同步到 gradle 实际取用的 jniLibs（共享路径，必须字节一致）----
SO_MD5="$(md5_of "$SO")"
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -d "$d" ]] || continue
  cp -f "$SO" "$d/libstarry_magic.so"
done
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -f "$d/libstarry_magic.so" ]] || continue
  m="$(md5_of "$d/libstarry_magic.so")"
  [[ "$m" == "$SO_MD5" ]] || { echo "ERROR: $d 同步失败（$m != ${SO_MD5}）" >&2; exit 1; }
done

echo "[build_so] done → $SO ($SO_MD5, $(size_of "$SO") B)；jniLibs 已同步"
