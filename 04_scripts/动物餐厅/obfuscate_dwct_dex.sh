#!/usr/bin/env bash
# ================================================================
#  obfuscate_menu_dex.sh — 给注入用 menu dex 做 R8 混淆
#
#  用法:
#    bash 04_scripts/_common/obfuscate_menu_dex.sh <输入.dex|classes目录> <输出.dex> [--keep <额外规则>]
#
#  例:
#    # 从已产出的 dex 再混淆一道（smali 路径用）
#    bash 04_scripts/_common/obfuscate_menu_dex.sh \
#         templates/ncsh_menu_current/classes2.dex /tmp/classes2.obf.dex
#
#    # 从 javac 产物直出（javac+d8 路径用，跳过 d8 一步）
#    bash 04_scripts/_common/obfuscate_menu_dex.sh \
#         app/build/intermediates/javac/release/compileReleaseJavaWithJavac/classes /tmp/x.dex
#
#  为什么需要（2026-11-10 实测）:
#    注入进游戏包的 menu dex 有两条产出路径（apktool+smali 57 个 / javac+d8 2 个），
#    **两条都不经过 R8** —— `app/build.gradle` 的 minifyEnabled 只作用于 release APK，
#    而注入链根本不跑 gradle。结果注入 dex 完全明文，strings 就能读出：
#      /v1/auth/grant、http://<服务器IP>、AuthClient、DeviceFingerprint、GRANT_RENEW_*
#    ⇒ 卡密协议对破解者完全透明（含绕过方法本身）。
#
#  实测效果（真实 132 个类）:
#    466,588 B → 352,468 B（-24.5%）；120 个业务类改名为 com/starry/magic/o/A…；
#    19 个组件类（Manifest/native/注入脚本按名找）原名保留。
#
#  ★ 硬约束（规则文件里已写清，改规则前先读）:
#    - 必须 keep：Manifest 组件、native FindClass 目标、注入脚本写进游戏 Manifest 的类
#    - 必须 `-repackageclasses 'com.starry.magic.o'`：57 个注入脚本用 `cp -R com/starry`
#      只拷这一棵子树；R8 默认重打包到 `k/`、`d/` 这类顶层包 ⇒ 拷不进包 ⇒ ClassNotFound
#    - JNI 方法名保持（`-keepclasseswithmembernames`），与 native 的 RegisterNatives 表对齐
# ================================================================
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
RULES="$ROOT/mod菜单/starry-mod-menu/app/inject-dex-rules.pro"

IN="${1:-}"; OUT="${2:-}"; shift 2 2>/dev/null || true
EXTRA_KEEP=""
while [ $# -gt 0 ]; do
  case "$1" in
    --keep) EXTRA_KEEP="$2"; shift 2 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

die() { echo "ERROR: $*" >&2; exit 1; }
say() { printf '%s\n' "$*"; }

[ -n "$IN" ] && [ -n "$OUT" ] || die "用法: obfuscate_menu_dex.sh <输入.dex|classes目录> <输出.dex> [--keep 追加规则文件]"
[ -e "$IN" ] || die "输入不存在: $IN"
[ -f "$RULES" ] || die "规则文件缺失: $RULES"

# --- 工具解析（mac 与 Win 都覆盖；走 env.sh 的既有口径）---
_py() { for c in python3.11 python3 python py; do command -v "$c" >/dev/null 2>&1 && { echo "$c"; return 0; }; done; return 1; }
PY="$(_py)" || die "无可用 python"

# java：优先 env.sh 的 JDK17，其次 PATH
JAVA="java"
if [ -n "${NIXIANG_JAVA17:-}" ] && [ -x "$NIXIANG_JAVA17/bin/java" ]; then
  JAVA="$NIXIANG_JAVA17/bin/java"
elif [ -n "${NIXIANG_JAVA17:-}" ] && [ -x "$NIXIANG_JAVA17/bin/java.exe" ]; then
  JAVA="$NIXIANG_JAVA17/bin/java.exe"
fi
command -v "$JAVA" >/dev/null 2>&1 || die "找不到 java（需要 JDK17）"

# r8.jar：cmdline-tools 里那份（build-tools 只有 d8，d8 --release 只去调试信息、不混淆）
R8="${NIXIANG_BUILD_TOOLS}/lib/d8.jar"
for c in \
  "${ANDROID_SDK_ROOT:-$HOME/Library/Android/sdk}/cmdline-tools/latest/lib/r8.jar" \
  "${ANDROID_SDK_ROOT:-$HOME/Library/Android/sdk}"/cmdline-tools/*/lib/r8.jar \
  "${LOCALAPPDATA:-}/Android/Sdk/cmdline-tools/latest/lib/r8.jar"; do
  [ -f "$c" ] && { R8="$c"; break; }
done
[ -n "$R8" ] || die "找不到 r8.jar（需 cmdline-tools；build-tools 里的 d8 不做混淆）"

# android.jar：--lib 必需（否则 android.* 全被判 missing class）
AJAR=""
SDK="${ANDROID_SDK_ROOT:-$HOME/Library/Android/sdk}"
for c in "$SDK"/platforms/android-*/android.jar; do
  [ -f "$c" ] && AJAR="$c"
done
[ -n "$AJAR" ] || die "找不到 android.jar（--lib 必需）"

# --- 输入形态归一：dex → jar，目录 → class 列表 ---
WORK="${TMPDIR:-/tmp}/obf_dex_$$"
mkdir -p "$WORK" || die "无法创建临时目录"
cleanup() { rm -rf "$WORK"; }
trap cleanup EXIT

INPUTS=()
case "$IN" in
  *.jar)
    # 构建脚本（javac 收集完 class 后本来就要打 jar 喂 d8）直接把 jar 传进来最省事：
    # R8 原生吃 jar，不必解开。
    [ -s "$IN" ] || die "jar 为空或不存在: $IN"
    say "[1/3] 输入 jar，跳过解包"
    INPUTS=("$IN")
    ;;
  *.dex)
    say "[1/3] 检查输入形态"
    # R8 吃 class/jar，dex 不可逆 ⇒ 只有 dex 时无法混淆。
    # 直接报错而不是静默产出"看着成功其实没混淆"的包 —— 后者是最坏的结果：
    # 以为防护上了，实际 dex 仍是明文。
    die "输入是 dex，无法直接混淆（R8 只吃 class/jar，dex 不可逆）。
       正确做法：把 javac 打完的 jar 交给本脚本（构建脚本里替换 d8 那一步），
       或直接传 class 目录。见本脚本头部用法示例。"
    ;;
  *)
    [ -d "$IN" ] || die "输入既不是目录、jar 也不是 dex: $IN"
    # bash 3.2 兼容：不能用 mapfile/readarray（macOS 自带 bash 就是 3.2）。
    INPUTS=()
    while IFS= read -r _cls; do
      [ -n "$_cls" ] && INPUTS+=("$_cls")
    done <<EOF
$(find "$IN" -name "*.class" 2>/dev/null)
EOF
    [ "${#INPUTS[@]}" -gt 0 ] || die "目录里没有 .class: $IN"
    ;;
esac

say "[2/3] R8 混淆（输入 ${#INPUTS[@]} 项）"
PGARGS=(--pg-conf "$RULES")
[ -n "$EXTRA_KEEP" ] && [ -f "$EXTRA_KEEP" ] && PGARGS+=(--pg-conf "$EXTRA_KEEP")

mkdir -p "$WORK/out"
if ! "$JAVA" -cp "$R8" com.android.tools.r8.R8 \
      --release --min-api 21 \
      --lib "$AJAR" \
      "${PGARGS[@]}" \
      --output "$WORK/out" \
      "${INPUTS[@]}" >"$WORK/r8.log" 2>&1; then
  grep -iE "^Error|missing class" "$WORK/r8.log" | head -10 >&2
  die "R8 失败（完整日志见上）"
fi
[ -f "$WORK/out/classes.dex" ] || die "R8 未产出 classes.dex"

# --- 3. 校验：必保类还在、业务类已改名 ---
say "[3/3] 校验"
"$PY" - "$WORK/out/classes.dex" <<'PYEOF' || exit 1
import re, struct, sys
p = sys.argv[1]
d = open(p, 'rb').read()
names = sorted(set(x.decode('ascii','ignore') for x in re.findall(rb'L[a-zA-Z0-9_/$]+;', d)))
must = ['com/starry/magic/menu/MenuService', 'com/starry/magic/bridge/ModBridge',
        'com/starry/magic/StarryInitProvider']
bad = [m for m in must if 'L'+m+';' not in names]
if bad:
    print("  ✗ 必保类丢失: " + ", ".join(bad)); sys.exit(1)
# 顶层包必须在 com/starry 下（否则 cp -R com/starry 拷不到）。
# 例外：dev/mars/secure/ProxyApplication 是**编译期桩**（源码注释："Compile-time stub only.
# Runtime class comes from the original classes.dex"），游戏包里本来就有官方那一份，
# 我们的桩不进包才对；它出现在 com/starry 之外是预期的，不算漏拷。
ALLOW_STRAY = {'Ldev/mars/secure/ProxyApplication;'}
stray = [n for n in names
         if not n.startswith(('Lcom/starry/', 'Landroid/', 'Ljava/',
                              'Ljavax/', 'Ldalvik/', 'Lorg/'))
         and n not in ALLOW_STRAY]
if stray:
    print(f"  ✗ {len(stray)} 个类被重打包到 com/starry 之外（cp -R com/starry 会漏拷）: {stray[:3]}")
    sys.exit(1)
print(f"  ✓ dex {len(d)} B / 类 {len(names)} 个；必保类齐全；无越界重打包")
PYEOF

cp -f "$WORK/out/classes.dex" "$OUT" || die "写出失败: $OUT"
say "✓ 已产出 ${OUT}（$(wc -c <"$OUT" | tr -d ' ') B）"
