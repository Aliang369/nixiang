#!/usr/bin/env bash
# 勇士之心 1.0.3 专用：用「1.0.3 源码 + 1.0.4 菜单序号」重建 libstarry_magic.so。
#
# 为什么需要这个脚本：
#   1.0.3 的出包脚本 scripts/surgical_ysxz_1.0.3.sh 只组装 dex，.so 是**预置产物**
#   （$STARRY_SRC/libs_ysxz/arm64-v8a/libstarry_magic.so）。libs_*/ 被 .gitignore 忽略，
#   所以换机器后这份 so 不存在（或残留旧菜单那份），1.0.3 就会带上旧菜单。
#   本脚本从 git 里的 1.0.3 源码提交重建，并覆盖 04_scripts/勇士之心/ysxz_1.0.3_menu/
#   里那份「1.0.3 源码 + 新菜单表」的三个文件，产物直接落回 libs_ysxz。
#   2026-10-05 追加：MuMu/Houdini 下 cocos so 无 x 段 ⇒ FileOffToVa 只认 x 会恒
#   not mapped；覆盖的 YsxzMod.cpp 带「x 段优先 → 可读段兜底」运行时探测（禁机型分叉）。
#
# 用法：
#   bash 04_scripts/勇士之心/ysxz_build_so_103.sh
#   环境变量：ANDROID_NDK_HOME / YSXZ_NDK 指定 NDK；YSXZ_103_BASE 换基线提交（默认 81c70279）；
#            YSXZ_103_WORK 换临时构建目录；YSXZ_103_NO_COPY=1 只构建不落 libs_ysxz。
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$HERE/../.." && pwd)}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI_REL="mod菜单/starry-mod-menu/app/src/main/jni"
MENU_DIR="$NIXIANG/04_scripts/勇士之心/ysxz_1.0.3_menu"
BASE_COMMIT="${YSXZ_103_BASE:-81c70279}"
WORK="${YSXZ_103_WORK:-$NIXIANG/.ysxz_103_build}"
OUT_LIBS="$STARRY_SRC/libs_ysxz"
JNIW="$WORK/jni"
SRC_APK="${ORIG_APK:-$NIXIANG/02_apks/勇士之心_1.0.3.apk}"
export PATH="$HOME/.local/bin:$PATH"   # Win 端 python3.11 包装器常在这里
PY="${YSXZ_PY:-$(command -v python3.11 || command -v python3 || true)}"

# ---------- NDK / SDK 探测（macOS / Windows 都能跑，不写死机器路径） ----------
# ANDROID_NDK_HOME / YSXZ_NDK 优先；否则在 ANDROID_SDK_ROOT|ANDROID_HOME 和系统默认 SDK 目录下找 27.0.12077973
LOCALAPPDATA_U=""
[[ -n "${LOCALAPPDATA:-}" ]] && LOCALAPPDATA_U="$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")"
NDK=""
for cand in "${YSXZ_NDK:-}" "${ANDROID_NDK_HOME:-}" \
            "${ANDROID_SDK_ROOT:-}/ndk/27.0.12077973" "${ANDROID_HOME:-}/ndk/27.0.12077973" \
            "$HOME/Library/Android/sdk/ndk/27.0.12077973" \
            "$LOCALAPPDATA_U/Android/Sdk/ndk/27.0.12077973"; do
  [[ -n "$cand" && "$cand" == /* ]] || continue
  if [[ -x "$cand/ndk-build" || -f "$cand/ndk-build.cmd" ]]; then NDK="$cand"; break; fi
done

[[ -n "$NDK" ]] || { echo "ERROR: 找不到 NDK，请设 ANDROID_NDK_HOME（需要含 ndk-build 或 ndk-build.cmd）" >&2; exit 1; }
if [[ -x "$NDK/ndk-build" ]]; then NDKBIN="$NDK/ndk-build"; else NDKBIN="$NDK/ndk-build.cmd"; fi

# 2026-10-26：Windows 上必须用 build/ndk-build.cmd 驱动（NDK 给 make 发的是 cmd 批处理：
# rem/del/copy/md，MSYS 的 /usr/bin/sh 不认 ⇒ `rem: command not found` + Error 127）。
_IS_WIN=0
case "$(uname -s 2>/dev/null || echo unknown)" in
  MINGW*|MSYS*|CYGWIN*) _IS_WIN=1 ;;
esac
if [[ "$_IS_WIN" == "1" && -f "$NDK/build/ndk-build.cmd" ]]; then
  NDKBIN="$NDK/build/ndk-build.cmd"
fi

JOBS="$(nproc 2>/dev/null || sysctl -n hw.ncpu 2>/dev/null || echo 8)"
# ndk-build.cmd 是 Windows 原生程序，路径必须用 Windows 形式
W_WIN="$(cygpath -m "$WORK" 2>/dev/null || echo "$WORK")"

echo "[103] NIXIANG   = $NIXIANG"
echo "[103] NDK       = $NDK"
echo "[103] base      = $BASE_COMMIT"
echo "[103] work      = $W_WIN"

for f in YsxzEntry.cpp YsxzMod.h YsxzPatchJs.h YsxzMod.cpp; do
  [[ -f "$MENU_DIR/$f" ]] || { echo "ERROR: missing $MENU_DIR/$f" >&2; exit 1; }
done

[[ -f "$SRC_APK" ]] || { echo "ERROR: 缺底包 ${SRC_APK}（用于提取 pristine main.js）" >&2; exit 1; }
[[ -n "$PY" ]] || { echo "ERROR: 找不到 python3.11/python3（自举素材生成需要）" >&2; exit 1; }
[[ -f "$MENU_DIR/boot_ids.json" ]] || { echo "ERROR: 缺 $MENU_DIR/boot_ids.json" >&2; exit 1; }

# ---------- 0. 诱饵层契约门禁（2026-10-27「卸载重装卡加载页」事故后新增）----------
# splice 会把 bundle 改写成 }(window.__starry_capture({…}))，任何注入体（真载荷/诱饵）
# 都必须定义它并 return reg。诱饵曾经漏了 ⇒ 无票路径直接把主 bundle 打死。
# 这里在编译前跑：语法合法 + 透传在位 + node 契约仿真 + 负向对照 + 修法 B 在位。
"$PY" "$NIXIANG/04_scripts/勇士之心/check_decoy_js.py" || {
  echo "ERROR: 诱饵层契约门禁 FAIL（禁止出包）" >&2; exit 1; }

# ---------- 1. 从 1.0.3 源码提交导出 jni（落到 ASCII 路径，避免中文路径进 clang） ----------
rm -rf "$WORK"
mkdir -p "$WORK"
git -C "$NIXIANG" archive "$BASE_COMMIT" "$JNI_REL" | tar -x -C "$WORK" --strip-components=5
[[ -f "$JNIW/Android_ysxz.mk" ]] || { echo "ERROR: 导出失败，缺 $JNIW/Android_ysxz.mk" >&2; exit 1; }

# ---------- 2. 覆盖「新菜单 + 可读段兜底」四个文件 ----------
cp -f "$MENU_DIR/YsxzEntry.cpp" "$JNIW/Games/YsxzEntry.cpp"
cp -f "$MENU_DIR/YsxzMod.h"     "$JNIW/Games/YsxzMod.h"
cp -f "$MENU_DIR/YsxzPatchJs.h" "$JNIW/Games/YsxzPatchJs.h"
cp -f "$MENU_DIR/YsxzMod.cpp"   "$JNIW/Games/YsxzMod.cpp"

# ---------- 2.1 框架层用**工作区现行**覆盖（2026-10-26 防御加固 P1）----------
# 上面第 1 步是从 1.0.3 基线提交导出的「游戏菜单基线」，但框架层（授权链 / JNI 入口）
# 现在由 Includes/SgAuth.h 统一承载、并且要随每轮加固演进。若仍用基线提交的那份，
# 任何加固改动都得先提交才能编译，且容易把旧授权链又编回包里。
# ⇒ 框架层文件一律用工作区现行版本覆盖（缺一个就报错，不静默降级）。
# ---------- 2.05 P2-2：载荷密文必须与**刚拷进来的这份**一致 ----------
# 2026-10-27 踩过：mod菜单 与 ysxz_1.0.3_menu 下各有一份同名 YsxzPatchJs.h（内容不同，
# 分属 1.0.4/1.0.6 线与 1.0.3 复刻线），加密时取错了源头 ⇒ 注入旧载荷、部分功能失效。
# ⇒ 这里在编译前**强制**按本脚本刚拷贝的那份重新生成密文，物理上不可能再错源头。
# C9：把"该游戏 + 构建机 key"交给出包门禁，让 harden 直接问服务端
# 「这个游戏的载荷 sha 登记了吗」——不一致就 FAIL 并打出后台操作步骤。
export SG_GAME_ID=yszzx
export SG_API_BASE="${SG_API_BASE:-https://47.100.216.101:8443}"
if [ -z "${SG_BUILD_KEY:-}" ] && [ -f "$HOME/.starry-keys/build_key.txt" ]; then
  export SG_BUILD_KEY="$(cat "$HOME/.starry-keys/build_key.txt")"
fi

python3 "$NIXIANG/04_scripts/勇士之心/gen_payload_enc.py" >/tmp/payload_gen.log 2>&1 || {
  echo "ERROR: 载荷加密失败（gen_payload_enc.py）" >&2; tail -5 /tmp/payload_gen.log >&2; exit 1; }
echo "[103] payload = $(wc -c < "$JNIW/Games/YsxzPatchJs.h") B（加密源 == 编译源，已强制对齐）"

FW_OVERLAY="${YSXZ_FW_OVERLAY:-1}"
if [[ "$FW_OVERLAY" == "1" ]]; then
  for f in MainModern.cpp Main.cpp Includes/SgAuth.h Includes/sg_rsa_verify.h Includes/sg_pubkey.h \
           Includes/SgIntegrity.h Includes/sg_integ.h Includes/SgZip.h Includes/SgBuildManifest.h \
           Includes/SgPayload.h Includes/SgPayloadJs.h Includes/sg_payload_enc.h Games/YsxzDecoyJs.h \
           Games/SignalQuarantine.h Games/NoAceStubs.cpp \
           Includes/Utils.cpp Includes/Utils.hpp Includes/StarryStamp.h \
           Includes/obfuscate.h; do
    src="$STARRY_SRC/app/src/main/jni/$f"
    [[ -f "$src" ]] || { echo "ERROR: 工作区缺框架文件 ${f}（P1 加固必需）" >&2; exit 1; }
    mkdir -p "$(dirname "$JNIW/$f")"
    cp -f "$src" "$JNIW/$f"
  done
  # 1.0.3 基线 mk 不认后来新增的框架源文件 ⇒ 补进 LOCAL_SRC_FILES（幂等）
  # 同时补 P1-6 需要的编译宏与链接库（**必须加在这里**：真正编译的是 $JNIW 里这份
  # 基线导出的 mk，改工作区那份没用 —— 2026-10-26 已踩过：宏没进包 ⇒ 完整性校验整体
  # 没编进去，日志里看不到 [V2] g=1）。
  python3 - "$JNIW/Android_ysxz.mk" <<'PY'
import io, re, sys
p = sys.argv[1]
s = io.open(p, encoding="utf-8").read()

# (a) 补源文件
if "Games/NoAceStubs.cpp" in s:
    print("[103] mk 已含 NoAceStubs.cpp")
else:
    out, done = [], False
    for line in s.splitlines(True):
        if not done and line.strip().startswith("include $(BUILD_SHARED_LIBRARY)"):
            out.append("LOCAL_SRC_FILES += Games/NoAceStubs.cpp\n")
            done = True
        out.append(line)
    if not done:
        raise SystemExit("ERROR: Android_ysxz.mk 里找不到 include $(BUILD_SHARED_LIBRARY)")
    s = "".join(out)
    print("[103] mk += Games/NoAceStubs.cpp")

# (b) P1-6 宏 + zlib
if "SG_HAVE_ZLIB" not in s:
    def addflag(m):
        return m.group(0) + " -DSG_HAVE_ZLIB=1 -DSG_REQUIRE_INTEGRITY=1 -DSG_PAYLOAD_ENC=1"
    s = re.sub(r"^LOCAL_C(PP)?FLAGS\s*:=.*$", addflag, s, flags=re.M)
    s = re.sub(r"^(LOCAL_LDLIBS\s*:=.*)$", r"\1 -lz", s, flags=re.M)
    print("[103] mk += -DSG_HAVE_ZLIB -DSG_REQUIRE_INTEGRITY -DSG_PAYLOAD_ENC / -lz")

else:
    print("[103] mk 已含 SG_HAVE_ZLIB")

# 构建戳保留（装机闸门「查8」）：modern 入口零引用 kStarryBuildStamp，
# 不加 --undefined 会被 --gc-sections 回收 ⇒ so 里没有 STARRYSTAMP1:EPOCH=
# ⇒ 覆盖安装后不清玩家旧 OTA so（2026-10-27 勇士之心实测缺戳，本补丁修）
if "kStarryBuildStamp" not in s:
    s2 = re.sub(r"^(LOCAL_LDFLAGS\s*:=.*)$",
                r"\1 -Wl,--undefined=kStarryBuildStamp", s, count=1, flags=re.M)
    if s2 != s:
        s = s2
        print("[103] mk += -Wl,--undefined=kStarryBuildStamp（OTA 守卫构建戳）")
    else:
        # 兜底：没有 LOCAL_LDFLAGS:= 行就自己加一行
        s = re.sub(r"^(LOCAL_LDLIBS\s*:=.*)$",
                   r"\1\nLOCAL_LDFLAGS += -Wl,--undefined=kStarryBuildStamp", s, count=1, flags=re.M)
        print("[103] mk 新增 LOCAL_LDFLAGS 行（OTA 守卫构建戳）")
else:
    print("[103] mk 已含构建戳链接标志")

io.open(p, "w", encoding="utf-8", newline="\n").write(s)
PY
  # 断言：编进包的必须是委托版（不是基线的复制版），否则加固等于没做
  if ! grep -q 'sgauth::apply' "$JNIW/MainModern.cpp"; then
    echo "ERROR: 覆盖后 MainModern.cpp 里没有 sgauth::apply 委托 —— 拒绝用旧授权链出包" >&2
    exit 1
  fi
  echo "[103] 框架层覆盖：Main{Modern,}.cpp + SgAuth/sg_rsa_verify/sg_pubkey + SignalQuarantine（已断言 sgauth::apply）"
fi

# ---------- 2.4 多 ABI（2026-10-26 雷电轮）----------
# 默认沿用底包的 Application.mk（APP_ABI := arm64-v8a）。用户要求以雷电模拟器为验证环境，
# 该实例是 x86_64 + Houdini，但游戏自带 lib/x86 原生库 ⇒ 只有把 so 也编成 x86，应用才会
# 走原生 x86 路径（不走 Houdini 翻译）。用 YSXZ_ABIS="arm64-v8a x86" 覆盖。
if [[ -n "${YSXZ_ABIS:-}" ]]; then
  python3 - "$JNIW/Application.mk" "$YSXZ_ABIS" <<'PY'
import io, re, sys
p, abis = sys.argv[1], sys.argv[2]
s = io.open(p, encoding="utf-8").read()
if re.search(r"^APP_ABI\s*:?=", s, re.M):
    s = re.sub(r"^APP_ABI\s*:?=.*$", "APP_ABI := %s" % abis, s, count=1, flags=re.M)
else:
    s += "\nAPP_ABI := %s\n" % abis
io.open(p, "w", encoding="utf-8", newline="\n").write(s)
print("[103] APP_ABI := " + abis)
PY
fi

# ---------- 2.44 构建戳：确保暂存 Application.mk 会注入 STARRY_BUILD_EPOCH ----------
# 1.0.3 基线 Application.mk 没有 APP_CPPFLAGS 那行 ⇒ STARRY_BUILD_EPOCH 永远走兜底值
# 1790184000 ⇒ 装机闸门「查8 构建戳」FAIL（2026-10-27 实测：戳印出来是兜底值）。
python3 - "$JNIW/Application.mk" <<'PY'
import io, sys
p = sys.argv[1]
s = io.open(p, encoding="utf-8").read()
if "STARRY_BUILD_EPOCH" in s and "APP_CPPFLAGS" in s:
    print("[103] Application.mk 已注入 STARRY_BUILD_EPOCH")
else:
    add = []
    if "STARRY_BUILD_EPOCH ?=" not in s:
        add.append('STARRY_BUILD_EPOCH ?= $(shell git log -1 --format=%ct 2>/dev/null | tr -cd "0-9")')
        add.append('ifeq ($(strip $(STARRY_BUILD_EPOCH)),)')
        add.append('STARRY_BUILD_EPOCH := 1790184000')
        add.append('endif')
    if "-DSTARRY_BUILD_EPOCH" not in s:
        add.append('APP_CPPFLAGS += -DSTARRY_BUILD_EPOCH=$(STARRY_BUILD_EPOCH)')
    s = s.rstrip("\n") + "\n" + "\n".join(add) + "\n"
    io.open(p, "w", encoding="utf-8", newline="\n").write(s)
    print("[103] Application.mk += APP_CPPFLAGS -DSTARRY_BUILD_EPOCH（构建戳必需）")
PY

# ---------- 2.45 去掉误链的 arm-only 预编译库（2026-10-26 多 ABI 必踩）----------
# 1.0.3 基线 Android_ysxz.mk 里 `LOCAL_STATIC_LIBRARIES := Dobby wxsj_dll`，而
# Games/WxsjPatchedDll.a 是别的游戏的 arm 专用产物；编 x86 时链接器直接报
#   ld.lld: error: ./Games/WxsjPatchedDll.a(...) is incompatible with elf_i386
# 勇士之心根本不用它（1.0.6 的 Android_ysxz.mk 早已去掉）⇒ 统一摘掉。
python3 - "$JNIW/Android_ysxz.mk" <<'PY'
import io, re, sys
p = sys.argv[1]
s = io.open(p, encoding="utf-8").read()
s2 = re.sub(r"^LOCAL_STATIC_LIBRARIES\s*:?=.*$", "LOCAL_STATIC_LIBRARIES := Dobby", s, count=1, flags=re.M)
if s2 != s:
    io.open(p, "w", encoding="utf-8", newline="\n").write(s2)
    print("[103] Android_ysxz.mk: LOCAL_STATIC_LIBRARIES := Dobby（摘掉 arm-only wxsj_dll）")
else:
    print("[103] Android_ysxz.mk: 已是纯 Dobby 静态库")
PY

# ---------- 2.5 生成「模拟器自举」素材（JS 驱动 + 加密 main.js + so 内嵌 C 头） ----------
# 背景（2026-10-05 实测）：MuMu(Houdini) 下原生 inline hook 装得上但从不命中，
# 自举 JS 在运行时接管 files/res/main.js；两条路线靠 files/ysxz_native_ok 判据互斥，
# 禁机型/模拟器名单分叉（硬约束#6）。头文件故意硬依赖：缺它就地报错，防拿旧物件出包。
BOOT_JS="$OUT_LIBS/ysxz_boot.js"
BOOT_ENC="$OUT_LIBS/main.patched.enc.js"
BOOT_HDR="$JNIW/Games/YsxzBootBlob.h"
mkdir -p "$OUT_LIBS"
"$PY" "$HERE/gen_ysxz_boot_js.py" --out "$BOOT_JS"
"$PY" "$HERE/ysxz_mainjs_patch.py" --apk "$SRC_APK" --boot-js "$BOOT_JS" \
      --out "$BOOT_ENC" --header "$BOOT_HDR"
[[ -f "$BOOT_HDR" ]] || { echo "ERROR: 自举头未生成 $BOOT_HDR" >&2; exit 1; }

# ---------- 3. 构建 ----------
# 构建戳（OTA 守卫）取**仓库**的 git commit 时间：暂存目录 .ysxz_103_build 不是 git 仓库，
# Application.mk 里的 `git log` 会失败并回落兜底值 1790184000（装机闸门查8 会拦）。
if [[ -z "${STARRY_BUILD_EPOCH:-}" ]]; then
  STARRY_BUILD_EPOCH="$(git -C "$NIXIANG" log -1 --format=%ct 2>/dev/null | tr -cd '0-9')"
fi
[[ -n "${STARRY_BUILD_EPOCH:-}" ]] || STARRY_BUILD_EPOCH=1790184000
export STARRY_BUILD_EPOCH
echo "[103] STARRY_BUILD_EPOCH = ${STARRY_BUILD_EPOCH}（构建戳）"

if [[ "$_IS_WIN" == "1" ]]; then
  echo "[103] win 分支: cmd //c ndk-build.cmd -C $W_WIN/jni"
  ( cd "$W_WIN/jni" && cmd //c "$(cygpath -w "$NDKBIN")" -C "$W_WIN/jni" \
      NDK_PROJECT_PATH="$W_WIN" \
      APP_BUILD_SCRIPT="./Android_ysxz.mk" \
      NDK_APPLICATION_MK="./Application.mk" \
      NDK_OUT="$W_WIN/obj" \
      NDK_LIBS_OUT="$W_WIN/libs" \
      STARRY_BUILD_EPOCH="$STARRY_BUILD_EPOCH" \
      -j"$JOBS" )
else
  "$NDKBIN" -C "$W_WIN/jni" \
    NDK_PROJECT_PATH="$W_WIN" \
    APP_BUILD_SCRIPT="./Android_ysxz.mk" \
    NDK_APPLICATION_MK="./Application.mk" \
    NDK_OUT="$W_WIN/obj" \
    NDK_LIBS_OUT="$W_WIN/libs" \
    STARRY_BUILD_EPOCH="$STARRY_BUILD_EPOCH" \
    -j"$JOBS"
fi

ABIS_BUILT=""
for _abi_dir in "$WORK"/libs/*/; do
  [[ -f "$_abi_dir/libstarry_magic.so" ]] && ABIS_BUILT="$ABIS_BUILT $(basename "$_abi_dir")"
done
[[ -n "$ABIS_BUILT" ]] || { echo "ERROR: 构建产物缺失（$WORK/libs/*/libstarry_magic.so）" >&2; exit 1; }
echo "[103] abis =$ABIS_BUILT"
SO="$WORK/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || SO="$(ls "$WORK"/libs/*/libstarry_magic.so | head -1)"

# ---------- 4. 断言：必须带新菜单，绝不能带旧菜单 ----------
for token in "Slider_伤害倍率:1:50:1" "Slider_移动速度:1:10:1" "Toggle_秒杀" "Toggle_无敌" "Toggle_无限技能" "Toggle_自动战斗"; do
  HITS="$(LC_ALL=C grep -ac -- "$token" "$SO" || true)"
  [[ "${HITS:-0}" -ge 1 ]] || { echo "ERROR: $SO 不含菜单串 $token" >&2; exit 1; }
done
if LC_ALL=C grep -aq -- "Slider_移动速度:1:5:1" "$SO"; then
  echo "ERROR: $SO 仍含旧菜单串 Slider_移动速度:1:5:1 —— 菜单表没换成功" >&2
  exit 1
fi

# 必须带模拟器可读段兜底（2026-10-05）：缺它时 MuMu 上 evalString 恒 not mapped。
LC_ALL=C grep -aqF -- "readable-map fallback" "$SO" || {
  echo "ERROR: $SO 不含 readable-map fallback —— 地址解析没带可读段兜底，MuMu 会失效" >&2
  exit 1
}

# 必须带模拟器自举两件套（2026-10-05 第二轮）：main.js 自愈 + 原生生效证明。
for tok in "boot self-heal" "ysxz_native_ok"; do
  LC_ALL=C grep -aqF -- "$tok" "$SO" || {
    echo "ERROR: $SO 不含 [$tok] —— 自举/证明代码没编进去" >&2
    exit 1
  }
done

SZ="$(wc -c <"$SO" | tr -d ' ')"
# 上限放宽到 400000：注入的 patch JS 会被【嵌两次】——一次是 Games/YsxzPatchJs.h
# (kYsxzPatchJs)，一次是自举 blob libs_ysxz/main.patched.enc.js（XXTEA 后仍近似线性）。
# 所以 patch JS 每长 1 B，so 大约长 2 B。2026-10-07 加自动化层后 patch 51.6K->60.8K，
# so 199024 -> 226680，属预期。只防"编错目标/明显损坏"这种量级错误。
if [[ "$SZ" -lt 120000 || "$SZ" -gt 400000 ]]; then
  echo "ERROR: $SO 体积异常（$SZ B），疑似编错目标" >&2; exit 1
fi
MD5="$(md5sum "$SO" 2>/dev/null | awk '{print $1}' || md5 -q "$SO")"
SHA="$(sha256sum "$SO" 2>/dev/null | awk '{print $1}' || shasum -a 256 "$SO" | awk '{print $1}')"
echo "[103] so     = $SO"
echo "[103] size   = $SZ B"
echo "[103] md5    = $MD5"
echo "[103] sha256 = $SHA"

# ---------- 5. 落回 1.0.3 出包脚本读取的预置 so 位置 ----------
if [[ "${YSXZ_103_NO_COPY:-0}" != "1" ]]; then
  for _abi_dir in "$WORK"/libs/*/; do
    _abi="$(basename "$_abi_dir")"
    [[ -f "$_abi_dir/libstarry_magic.so" ]] || continue
    mkdir -p "$OUT_LIBS/$_abi"
    cp -f "$_abi_dir/libstarry_magic.so" "$OUT_LIBS/$_abi/libstarry_magic.so"
    echo "[103] 已落 $OUT_LIBS/$_abi/libstarry_magic.so"
  done
  # ---------- 5.1 产物级门禁：每份 so 里必须有诱饵透传**明文** ----------
  # 真载荷是密文（C8 查的是"没有明文"），诱饵是明文 ⇒ 用反转的指纹证明
  # 「诱饵层这次真的编进这份 so 了」，不再靠"包能出"推断。
  "$PY" "$NIXIANG/04_scripts/勇士之心/check_decoy_js.py" --quiet \
    --so "$OUT_LIBS/arm64-v8a/libstarry_magic.so" \
         "$OUT_LIBS/x86/libstarry_magic.so" \
         "$OUT_LIBS/armeabi-v7a/libstarry_magic.so" || {
    echo "ERROR: so 里没有诱饵透传明文（诱饵层未编进包）" >&2; exit 1; }
fi
echo "[103] done"
