#!/usr/bin/env bash
# 冒险战争 (mxzz) 菜单 Java 桥 dex 构建（gradle-free：javac + jar + R8）
#
# 照 04_scripts/魂坠深境/hzsj_build_dex.sh（2026-10-05 绝区零 zzz 路线，win 实测）：
#   - 本机无 gradle 发行版；javac + R8 全离线（JDK17 + build-tools/cmdline-tools）
#   - 源码 = mod菜单/starry-mod-menu/app/src/main/java（全量通用菜单）
#   - 补 R / BuildConfig 两个**构建期桩**（按源码 grep 自动枚举，不手抄）
#   - **编译期 stub**：HEAD(946f44f4) 缺 com.starry.magic.core.TapTapWebLogin（框架级缺陷，
#     games/wxsj/GameHookImpl.java 引用而全仓不存在）⇒ 用 04_scripts/冒险战争/menu_stub/
#     单独编译成 classpath，**不并入 jar/dex**（Lead 2026-11-19 裁决 A）。
#
# 产物：07_patched/冒险战争/menu_current/classes6.dex（底包 dex = classes..classes5，取下一空位）
#      + build_info.txt（source_commit / sha256，供 menu_tpl_gate.sh 与 build_manifest 用）
#
# 用法：bash 04_scripts/冒险战争/mxzz_build_dex.sh
#      MXZZ_DEX_NAME=classes7.dex bash ...   # 覆盖产出名（surgical 的 DEX_ADD_NAME 必须一致）
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=../_common/env.sh
. "${HERE}/../_common/env.sh"

STAR="${NIXIANG_ROOT}/mod菜单/starry-mod-menu"
SRC="${STAR}/app/src/main/java"
STUB="${HERE}/menu_stub"
TPL_DIR="${NIXIANG_ROOT}/07_patched/冒险战争/menu_current"
DEX_NAME="${MXZZ_DEX_NAME:-classes6.dex}"

die() { echo "ERROR: $*" >&2; exit 1; }

[ -n "${NIXIANG_JAVA17:-}" ] || die "NIXIANG_JAVA17 未解析（env.sh）"
JAVAC="${NIXIANG_JAVA17}/bin/javac.exe"; [ -f "${JAVAC}" ] || JAVAC="${NIXIANG_JAVA17}/bin/javac"
[ -f "${JAVAC}" ] || die "javac 不存在: ${JAVAC}"
JAR_BIN="${NIXIANG_JAVA17}/bin/jar.exe"; [ -f "${JAR_BIN}" ] || JAR_BIN="${NIXIANG_JAVA17}/bin/jar"
[ -f "${JAR_BIN}" ] || die "jar 不存在"

SDK_WIN="${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk"
AJAR=""
for cand in "${SDK_WIN}/platforms/android-35/android.jar" "${SDK_WIN}/platforms/android-36/android.jar"; do
  [ -f "${cand}" ] && AJAR="${cand}" && break
done
if [ -z "${AJAR}" ]; then
  cand="$(ls -1d "${SDK_WIN}"/platforms/android-*/android.jar 2>/dev/null | sort -V | tail -1)"
  [ -n "${cand}" ] && AJAR="${cand}"
fi
[ -n "${AJAR}" ] && [ -f "${AJAR}" ] || die "android.jar 未找到（${SDK_WIN}/platforms）"

[ -d "${SRC}" ] || die "Java 源目录不存在: ${SRC}"
[ -d "${STUB}" ] || die "编译期 stub 目录不存在: ${STUB}"

# --- ASCII 暂存（中文路径会打死 Windows 侧工具）---
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) HOST=win; STAGE_ROOT="$(cygpath -u "${LOCALAPPDATA:-$HOME/AppData/Local}")/Temp/mxzz_build/dex" ;;
  *) HOST=unix; STAGE_ROOT="${TMPDIR:-/tmp}/mxzz_build/dex" ;;
esac
w() { if [ "${HOST}" = "win" ]; then cygpath -m "$1"; else printf '%s' "$1"; fi; }
rm -rf "${STAGE_ROOT}"
mkdir -p "${STAGE_ROOT}/src" "${STAGE_ROOT}/gen/com/starry/magic" "${STAGE_ROOT}/stub_classes" "${STAGE_ROOT}/classes" "${STAGE_ROOT}/out"

echo "[1/6] 源拷贝 → ${STAGE_ROOT}/src（ASCII 路径）"
cp -a "${SRC}/." "${STAGE_ROOT}/src/"

echo "[2/6] 自动生成 R / BuildConfig 桩（grep 枚举，不手抄）"
python - "${STAGE_ROOT}/src" "${STAGE_ROOT}/gen/com/starry/magic" <<'PY' || die "R/BuildConfig 桩生成失败"
import os, re, sys
src, out = sys.argv[1], sys.argv[2]
rrefs, bcrefs = {}, set()
for root, _d, files in os.walk(src):
    for fn in files:
        if not fn.endswith('.java'):
            continue
        txt = open(os.path.join(root, fn), encoding='utf-8', errors='ignore').read()
        for kind, name in re.findall(r'\bR\.(id|string|layout|drawable|mipmap|color|style|anim|xml|array|dimen|raw|attr)\.([A-Za-z0-9_]+)', txt):
            rrefs.setdefault(kind, set()).add(name)
        for name in re.findall(r'\bBuildConfig\.([A-Za-z0-9_]+)', txt):
            bcrefs.add(name)
order = ['layout', 'drawable', 'string', 'id', 'mipmap', 'color', 'style', 'anim', 'xml', 'array', 'dimen', 'raw', 'attr']
lines = ['package com.starry.magic;', '',
         '/** 构建期 R 桩：本机无 aapt2 资源编译。仅供编译期符号解析。 */',
         'public final class R {', '    private R() {}']
n = 0
for k in order + [k for k in rrefs if k not in order]:
    if k not in rrefs:
        continue
    lines.append('    public static final class %s {' % k)
    lines.append('        private %s() {}' % k)
    for i, nm in enumerate(sorted(rrefs[k]), start=1):
        lines.append('        public static final int %s = 0x7f%02x%04x;' % (nm, order.index(k) + 1 if k in order else 90, i))
        n += 1
    lines.append('    }')
lines.append('}')
open(os.path.join(out, 'R.java'), 'w', encoding='utf-8', newline='\n').write('\n'.join(lines) + '\n')

bclines = ['package com.starry.magic;', '',
           '/** 构建期 BuildConfig 桩：本机无 gradle。 */',
           'public final class BuildConfig {', '    private BuildConfig() {}']
for nm in sorted(bcrefs):
    if nm == 'DEBUG':
        bclines.append('    public static final boolean DEBUG = false;')
    elif nm == 'VERSION_CODE':
        bclines.append('    public static final int VERSION_CODE = 7;')
    elif nm.startswith('VERSION_NAME') or nm == 'APPLICATION_ID' or nm == 'BUILD_TYPE' or nm == 'FLAVOR':
        bclines.append('    public static final String %s = "1.1.036";' % nm)
    else:
        bclines.append('    public static final String %s = "mxzz";' % nm)
bclines.append('}')
open(os.path.join(out, 'BuildConfig.java'), 'w', encoding='utf-8', newline='\n').write('\n'.join(bclines) + '\n')
print('   R 资源符号 %d 个（%s）；BuildConfig 字段 %s' % (n, ','.join(sorted(rrefs)), sorted(bcrefs)))
PY

echo "[3/6] 编译期 stub（只做 classpath，不进 jar）"
# stub 也要拷进 ASCII 暂存：javac 的 @argfile 在 Windows 下按本地代码页读参数，
# 非 ASCII 路径（04_scripts/冒险战争/...）会被读成乱码 ⇒ "找不到文件"。
cp -a "${STUB}/." "${STAGE_ROOT}/stub_src/"
find "${STAGE_ROOT}/stub_src" -name '*.java' > "${STAGE_ROOT}/stub_srcs.txt"
: > "${STAGE_ROOT}/stub_argv.txt"
while IFS= read -r f; do printf '"%s"\n' "$(w "${f}")" >> "${STAGE_ROOT}/stub_argv.txt"; done < "${STAGE_ROOT}/stub_srcs.txt"
"${JAVAC}" -nowarn -encoding UTF-8 --release 11 \
  -classpath "$(w "${AJAR}")" -d "$(w "${STAGE_ROOT}/stub_classes")" \
  "@${STAGE_ROOT}/stub_argv.txt" > "${STAGE_ROOT}/stub_javac.log" 2>&1 \
  || { tail -20 "${STAGE_ROOT}/stub_javac.log" >&2; die "stub 编译失败"; }
echo "      stub class: $(find "${STAGE_ROOT}/stub_classes" -name '*.class' | wc -l | tr -d ' ')（不进 jar）"

echo "[4/6] javac 编译全量菜单源码"
find "${STAGE_ROOT}/src" -name '*.java' > "${STAGE_ROOT}/srcs.txt"
printf '%s\n' "${STAGE_ROOT}/gen/com/starry/magic/R.java" "${STAGE_ROOT}/gen/com/starry/magic/BuildConfig.java" >> "${STAGE_ROOT}/srcs.txt"
: > "${STAGE_ROOT}/argv.txt"
while IFS= read -r f; do printf '"%s"\n' "$(w "${f}")" >> "${STAGE_ROOT}/argv.txt"; done < "${STAGE_ROOT}/srcs.txt"

# 注意：不能用 -bootclasspath android.jar（android.jar 的 LambdaMetafactory 是阉割版）
CP_MAIN="$(w "${AJAR}")$(python -c "import os;print(os.pathsep)")$(w "${STAGE_ROOT}/stub_classes")"
"${JAVAC}" -nowarn -encoding UTF-8 --release 11 \
  -classpath "${CP_MAIN}" -d "$(w "${STAGE_ROOT}/classes")" \
  "@${STAGE_ROOT}/argv.txt" > "${STAGE_ROOT}/javac.log" 2>&1 \
  || { echo "--- javac 失败（尾部 40 行）---" >&2; tail -40 "${STAGE_ROOT}/javac.log" >&2; die "javac rc!=0"; }
echo "      class 文件 $(find "${STAGE_ROOT}/classes" -name '*.class' | wc -l | tr -d ' ') 个"
# stub 类不得出现在产物里（Lead 约束）
if find "${STAGE_ROOT}/classes" -path '*com/starry/magic/core/TapTapWebLogin.class' | grep -q .; then
  die "stub 类混进 classes（必须只作 classpath）"
fi

echo "[5/6] 打包 class → jar → R8 混淆出 dex"
"${JAR_BIN}" cf "$(w "${STAGE_ROOT}/classes.jar")" -C "$(w "${STAGE_ROOT}/classes")" . \
  > "${STAGE_ROOT}/jar.log" 2>&1 || { tail -20 "${STAGE_ROOT}/jar.log" >&2; die "jar 失败"; }
OBF="${NIXIANG_ROOT}/04_scripts/_common/obfuscate_menu_dex.sh"
[ -f "${OBF}" ] || die "缺混淆脚本: ${OBF}"
if ! ANDROID_SDK_ROOT="${SDK_WIN}" bash "${OBF}" "$(w "${STAGE_ROOT}/classes.jar")" "$(w "${STAGE_ROOT}/out/classes.dex")" \
      --keep "${STUB}/mxzz_stub_dontwarn.pro" \
      > "${STAGE_ROOT}/r8.log" 2>&1; then
  echo "--- R8 失败（尾部 40 行）---" >&2; tail -40 "${STAGE_ROOT}/r8.log" >&2; die "R8 混淆失败"
fi
[ -f "${STAGE_ROOT}/out/classes.dex" ] || die "R8 未产出 dex"

echo "[6/6] 字节自检 + 发布"
mkdir -p "${TPL_DIR}"
cp -f "${STAGE_ROOT}/out/classes.dex" "${TPL_DIR}/${DEX_NAME}"
"${NIXIANG_PYTHON:-python}" - "${TPL_DIR}/${DEX_NAME}" "${DEX_NAME}" <<'PY' || die "dex 字节自检失败"
import hashlib, sys, re
p, name = sys.argv[1], sys.argv[2]
b = open(p, 'rb').read()
need = [b'com/starry/magic/StarryInitProvider', b'com/starry/magic/menu/MenuService',
        b'com/starry/magic/ota/StarryNativeLoad', b'com/starry/magic/StarryApp']
miss = [n.decode() for n in need if n not in b]
print('file    : %s' % name); print('size    : %d B' % len(b))
print('sha256  : %s' % hashlib.sha256(b).hexdigest()); print('md5     : %s' % hashlib.md5(b).hexdigest())
print('magic   : %s' % b[:8].hex())
for n in need:
    print('  %-42s %s' % (n.decode(), 'OK' if n in b else 'MISSING'))
if b'com/starry/magic/core/TapTapWebLogin' in b:
    print('note: dex 内仍留存该类型的**引用字符串**（GameHookImpl 的字段/调用点），属预期；'
          '是否被定义由下一步 baksmali 类清单判定')
if miss:
    print('FAIL missing: %s' % ', '.join(miss)); sys.exit(1)
print('OK')
PY

# 决定性判据：编译期 stub 不得作为**类定义**进入 dex（引用字符串允许存在）
BAKSMALI="${NIXIANG_ROOT}/01_tools/smali/baksmali-fat.jar"   # 纯 jar 无 Main-Class，必须用 fat
if [ -f "${BAKSMALI}" ]; then
  CLSLIST="${STAGE_ROOT}/classes_list.txt"
  "@${NIXIANG_JAVA17}/bin/java.exe" -jar "${BAKSMALI}" list classes "${TPL_DIR}/${DEX_NAME}" > "${CLSLIST}" 2>/dev/null     || "${NIXIANG_JAVA17}/bin/java" -jar "${BAKSMALI}" list classes "${TPL_DIR}/${DEX_NAME}" > "${CLSLIST}" 2>/dev/null     || die "baksmali 类清单失败"
  echo "      dex 类定义数: $(wc -l < "${CLSLIST}" | tr -d ' ')"
  if grep -q '^Lcom/starry/magic/core/TapTapWebLogin;' "${CLSLIST}"; then
    die "编译期 stub 作为类定义进了 dex（违反 Lead 约束）"
  fi
  echo "      [check] stub 未作为类定义进入 dex OK"
else
  echo "      [warn] 缺 baksmali.jar，跳过「stub 类定义」判定"
fi

{
  echo "game        : 冒险战争 (mxzz)"
  echo "dex         : ${DEX_NAME}"
  echo "built_at    : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "javac       : ${JAVAC}"
  echo "androidjar  : ${AJAR}"
  echo "source      : ${SRC}"
  echo "compile_stub: ${STUB}（仅 classpath，不进 dex；原因见实现记录 §六）"
  echo "sha256      : $( (sha256sum "${TPL_DIR}/${DEX_NAME}" 2>/dev/null || shasum -a 256 "${TPL_DIR}/${DEX_NAME}") | cut -d' ' -f1)"
  echo "source_commit: $(git -C "${NIXIANG_ROOT}" log -1 --format=%H -- "mod菜单/starry-mod-menu/app/src/main/java" 2>/dev/null)"
} > "${TPL_DIR}/build_info.txt"

bash "${NIXIANG_ROOT}/04_scripts/_common/menu_tpl_gate.sh" --tpl "${TPL_DIR}" --dex-name "${DEX_NAME}" || {
  echo "ERROR: 模板新鲜度门禁未过" >&2; exit 1
}
echo "OK → ${TPL_DIR}/${DEX_NAME}"
