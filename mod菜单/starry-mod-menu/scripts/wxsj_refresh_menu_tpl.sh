#!/usr/bin/env bash
# 重建「星空菜单模板 dex」——外科 zip 组装路线（不对菜单包做 apktool b 整包重构）
#
# 用途：app/src/main/java/com/starry/** 改动后，把当前源码编译出的 com/starry 装进
#       templates/wxsj_menu_<name>/classes2.dex，供 build_wxsj_12.10701.sh 直接 zip 追加。
#
# 为什么必须有这一步：
#   so 侧 Main.cpp registerBridgeNatives() 会 RegisterNatives 20 个 native 方法。
#   若菜单 dex 落后于 so/Java（dex 里没声明某个 native），ART 抛 NoSuchMethodError、
#   JNI_OnLoad 返回 JNI_ERR → System.loadLibrary 把异常抛给 Java →
#   StarryInitProvider.onCreate 中断 → MenuService / nativeInit / mod_thread / Hook 全链不启动。
#   2026-09-26 实测事故即此（dex 缺 nativeGetBuildStamp）。
#   （Main.cpp 的 regOne 已改成容错注册，但保持 dex 与 so 同步才是正解。）
#
# 用法：
#   bash scripts/wxsj_refresh_menu_tpl.sh                    # → templates/wxsj_menu_$(date +%Y%m%d)
#   WXSJ_TPL_NAME=wxsj_menu_x bash scripts/wxsj_refresh_menu_tpl.sh
#   WXSJ_TPL_PREV=templates/wxsj_menu_20260922 bash scripts/...   # 指定对照旧模板
#
# 断言：
#   - StarryApp.smali / StarryInitProvider.smali 必须存在，且 StarryApp.super 改易盾壳类
#   - 新 dex 内必含 StarryInitProvider
#   - 相对对照旧模板 **零删除**（严格超集）——防止又出现「绘制类丢失 → 菜单外观变样」
#   - libc++_shared.so / starlight.crt 与对照旧模板逐字节比对（不一致只 WARN，不拦截）
set -euo pipefail

NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
APP="$NIXIANG/mod菜单/starry-mod-menu"
# 2026-10-26：默认改 **release**。原因：MainActivity 的自检入口由 `if (BuildConfig.DEBUG && …)`
# 守卫，javac 在 release 做常量折叠后 `starry_selftest` 串消失；debug 包会带上它，被
# check_release_apk 判「调试脚手架」拦下（实测 debug dex 命中 1 次 → FAIL）。
# 想要 debug 版做诊断：WXSJ_GRADLE_TASK=assembleDebug（出来只能给取证用，不能出正式包）。
GRADLE_TASK="${WXSJ_GRADLE_TASK:-assembleRelease}"
case "$GRADLE_TASK" in
  assembleRelease) APK_REL="app/build/outputs/apk/release/app-release.apk" ;;
  assembleDebug)   APK_REL="app/build/outputs/apk/debug/app-debug.apk" ;;
  *) echo "ERROR: 不支持的 WXSJ_GRADLE_TASK=${GRADLE_TASK}（assembleRelease / assembleDebug）"; exit 1 ;;
esac
STARRY_APK="${WXSJ_STARRY_APK:-$APP/$APK_REL}"
SMALI="$NIXIANG/01_tools/smali/smali"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
APP_CLASS="Lcom/netease/nis/wrapper/MyApplication;"

PREV="${WXSJ_TPL_PREV:-$APP/templates/wxsj_menu_20260922}"
# 2026-10-26：默认目录**去日期化**（固定 wxsj_menu_current），刷新即覆盖。
# 历史教训：默认原名 wxsj_menu_$(date) 每次刷新都产新目录，而出包脚本写死旧目录名
# ⇒ 刷新产物成孤儿、出包继续用老 dex（无限升级 vc1637 / 镇邪人 7f94233e 同一根因）。
NAME="${WXSJ_TPL_NAME:-wxsj_menu_current}"
OUT="$APP/templates/$NAME"
WORK="$(mktemp -d /tmp/wxsj_tpl.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

for f in "$SMALI" "$BAKSMALI"; do
  [[ -x "$f" ]] || { echo "ERROR: missing $f"; exit 1; }
done

echo "[1/6] gradle 离线构建 ${GRADLE_TASK}"
(cd "$APP" && ./gradlew --offline "$GRADLE_TASK" -q)
[[ -f "$STARRY_APK" ]] || { echo "ERROR: $STARRY_APK 未产出"; exit 1; }
echo "  apk: $(stat -f %z "$STARRY_APK") B  mtime=$(stat -f %Sm "$STARRY_APK")  task=${GRADLE_TASK}"

echo "[2/6] apktool d（-f，只读拆分，不重构整包）"
apktool d -f -o "$WORK/dec" "$STARRY_APK" >/dev/null 2>&1

echo "[3/6] 收集 com/starry（只装这一类树，易盾壳主 dex 不动）"
SMALI_IN="$WORK/smali_in"
mkdir -p "$SMALI_IN/com/starry"
shopt -s nullglob
pulled=0
for d in "$WORK/dec"/smali "$WORK/dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry/" "$SMALI_IN/com/starry/"
  pulled=$((pulled + 1))
done
shopt -u nullglob
[[ "$pulled" -ge 1 ]] || { echo "ERROR: apk 内没有 com/starry"; exit 1; }
echo "  从 $pulled 个 smali 目录汇总 com/starry"
for f in StarryApp StarryInitProvider; do
  [[ -f "$SMALI_IN/com/starry/magic/$f.smali" ]] || { echo "ERROR: $f.smali 缺失"; exit 1; }
done

echo "[4/6] StarryApp.super → 易盾壳类（禁 replace Application，见 build 脚本头注释）"
python3 - "$SMALI_IN/com/starry/magic/StarryApp.smali" "$APP_CLASS" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); APP = sys.argv[2]
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", f".super {APP}"),
    ("invoke-direct {p0}, Landroid/app/Application;-><init>()V",
     f"invoke-direct {{p0}}, {APP}-><init>()V"),
    ("invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
     f"invoke-super {{p0, p1}}, {APP}->attachBaseContext(Landroid/content/Context;)V"),
    ("invoke-super {p0}, Landroid/app/Application;->onCreate()V",
     f"invoke-super {{p0}}, {APP}->onCreate()V"),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b); changed += 1
if APP not in t:
    raise SystemExit(f"ERROR: StarryApp.smali 缺 {APP}")
if ".super Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali 仍是 .super Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  ok ({changed} edits)")
PY

echo "[5b/6] 注入菜单构建戳 starry-menu:<commit12>（供 menu_tpl_gate.sh 判新旧）"
JAVA_SRC_REL="mod菜单/starry-mod-menu/app/src/main/java"
SRC_COMMIT="$(git -C "$NIXIANG" log -1 --format=%H -- "$JAVA_SRC_REL" 2>/dev/null || true)"
[[ -n "$SRC_COMMIT" ]] || { echo "ERROR: 取不到 $JAVA_SRC_REL 的 git HEAD"; exit 1; }
SRC12="${SRC_COMMIT:0:12}"
STAMP="starry-menu:${SRC12}"
BC_SMALI="$SMALI_IN/com/starry/magic/BuildConfig.smali"
[[ -f "$BC_SMALI" ]] || { echo "ERROR: 缺 ${BC_SMALI}（无法注入构建戳）"; exit 1; }
python3 - "$BC_SMALI" "$STAMP" <<'PY'
from pathlib import Path
import re, sys
p = Path(sys.argv[1]); stamp = sys.argv[2]
t = p.read_text(encoding="utf-8")
line = '.field public static final MENU_STAMP:Ljava/lang/String; = "%s"\n' % stamp
t2, n = re.subn(r'^\.field public static final MENU_STAMP:Ljava/lang/String; = "[^"]*"\n',
                line, t, flags=re.M)
if n == 0:
    t2 = t.rstrip("\n") + "\n\n" + line
p.write_text(t2, encoding="utf-8")
print("  ok: %s" % stamp)
PY

echo "[5/6] smali assemble → classes2.dex"
"$SMALI" assemble -o "$WORK/classes2.dex" "$SMALI_IN" >/dev/null
[[ -s "$WORK/classes2.dex" ]] || { echo "ERROR: 组装失败"; exit 1; }
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$WORK/classes2.dex" \
  || { echo "ERROR: 新 dex 内缺 StarryInitProvider"; exit 1; }
LC_ALL=C grep -aq -- "$STAMP" "$WORK/classes2.dex" \
  || { echo "ERROR: 新 dex 内找不到构建戳 ${STAMP}"; exit 1; }
if LC_ALL=C grep -aq 'starry_selftest' "$WORK/classes2.dex"; then
  echo "ERROR: dex 带调试脚手架 starry_selftest（${GRADLE_TASK} 包不该有；正式包必被 check_release_apk 拦）"
  echo "       ⇒ 用 WXSJ_GRADLE_TASK=assembleRelease（默认）重跑"
  exit 1
fi
NEWN=$("$BAKSMALI" list classes "$WORK/classes2.dex" | wc -l | tr -d ' ')
echo "  classes2.dex = $(stat -f %z "$WORK/classes2.dex") B / $NEWN 类"

echo "[6/6] 与旧模板比对（真类零删除为硬性要求；合成 lambda 增减只提示）"
if [[ -s "$PREV/classes2.dex" ]]; then
  "$BAKSMALI" list classes "$PREV/classes2.dex" | sort > "$WORK/prev_cls.txt"
  "$BAKSMALI" list classes "$WORK/classes2.dex" | sort > "$WORK/new_cls.txt"
  comm -23 "$WORK/prev_cls.txt" "$WORK/new_cls.txt" > "$WORK/lost.txt" || true
  # 2026-10-26：`$$ExternalSyntheticLambda*` 与 `-IA;`（接口伴随类）是 D8/R8 的脱糖
  # 产物，重构后数量/编号会变（release 的 R8 还会合并），不算「丢类」。
  # 真类（控件/绘制/Service/Bridge…）丢失仍然硬拦——那才是「菜单外观变样」的来源。
  grep -E '\$\$ExternalSyntheticLambda[0-9]+;|-IA;$' "$WORK/lost.txt" > "$WORK/lost_synth.txt" 2>/dev/null || true
  grep -vE '\$\$ExternalSyntheticLambda[0-9]+;|-IA;$' "$WORK/lost.txt" > "$WORK/lost_real.txt" 2>/dev/null || true
  if [[ -s "$WORK/lost_real.txt" ]]; then
    echo "ERROR: 新 dex 相对 $PREV 丢真类（会导致菜单缺控件/外观变样）："
    sed 's/^/    /' "$WORK/lost_real.txt"
    exit 1
  fi
  if [[ -s "$WORK/lost_synth.txt" ]]; then
    echo "  合成类减少 $(wc -l < "$WORK/lost_synth.txt" | tr -d ' ') 个（脱糖/R8 合并所致，放行）"
  fi
  echo "  旧模板 $(wc -l < "$WORK/prev_cls.txt" | tr -d ' ') 类 → 新 $(wc -l < "$WORK/new_cls.txt" | tr -d ' ') 类，真类零删除 ✓"
  added="$(comm -13 "$WORK/prev_cls.txt" "$WORK/new_cls.txt" || true)"
  [[ -n "$added" ]] && { echo "  新增 $(printf '%s\n' "$added" | wc -l | tr -d ' ') 个（含合成类）"; }
else
  echo "  WARN: 对照模板 $PREV 不可用，跳过零删除断言（首次生成？）"
fi

mkdir -p "$OUT"
cp -f "$WORK/classes2.dex" "$OUT/classes2.dex"
# libc++_shared.so / starlight.crt：release/debug 包里不一定带这两个文件（实测都取不到），
# 取不到就沿用对照模板的（框架配套件，与菜单代码无关）。
unzip -p "$STARRY_APK" lib/arm64-v8a/libc++_shared.so > "$WORK/libcxx.so" 2>/dev/null || true
if [[ -s "$WORK/libcxx.so" ]]; then
  cp -f "$WORK/libcxx.so" "$OUT/libc++_shared.so"
elif [[ -s "$PREV/libc++_shared.so" ]]; then
  cp -f "$PREV/libc++_shared.so" "$OUT/libc++_shared.so"
else
  echo "ERROR: 取不到 libc++_shared.so（apk 与对照模板都没有）"; exit 1
fi
unzip -p "$STARRY_APK" assets/certs/starlight.crt > "$WORK/starlight.crt" 2>/dev/null || true
if [[ -s "$WORK/starlight.crt" ]]; then
  cp -f "$WORK/starlight.crt" "$OUT/starlight.crt"
elif [[ -s "$PREV/starlight.crt" ]]; then
  cp -f "$PREV/starlight.crt" "$OUT/starlight.crt"
else
  echo "ERROR: 取不到 starlight.crt（apk 与对照模板都没有）"; exit 1
fi
if [[ -s "$PREV/libc++_shared.so" ]]; then
  a=$(md5 -q "$PREV/libc++_shared.so"); b=$(md5 -q "$OUT/libc++_shared.so")
  [[ "$a" == "$b" ]] || echo "  WARN: libc++_shared.so 与旧模板不同（$a → ${b}）"
  a=$(md5 -q "$PREV/starlight.crt"); b=$(md5 -q "$OUT/starlight.crt")
  [[ "$a" == "$b" ]] || echo "  WARN: starlight.crt 与旧模板不同（$a → ${b}）"
fi

# ---- build_info.txt：模板身份（menu_tpl_gate.sh 的唯一判据来源）----
SRC_SHA="$({ find "$APP/app/src/main/java" -type f -name '*.java' | LC_ALL=C sort | xargs shasum -a 256 2>/dev/null || true; } | shasum -a 256 | awk '{print $1}')"
DEX_SHA="$(shasum -a 256 "$OUT/classes2.dex" | awk '{print $1}')"
{
  echo "game        : wxsj (无限升级)"
  echo "dex         : classes2.dex"
  echo "built_at    : $(date '+%Y-%m-%d %H:%M:%S')"
  echo "gradle_task : ${GRADLE_TASK}"
  echo "apk         : ${STARRY_APK}"
  echo "source      : ${APP}/app/src/main/java"
  echo "source_commit: ${SRC_COMMIT}"
  echo "source_sha256: ${SRC_SHA}"
  echo "dex_sha256  : ${DEX_SHA}"
  echo "stamp       : ${STAMP}"
  echo "derived     : 由 refresh 直接从 gradle 产物 com/starry 组装（StarryApp.super→易盾壳类）"
} > "$OUT/build_info.txt"
echo "  build_info.txt 已写（source_commit ${SRC12} / dex ${DEX_SHA:0:16}…）"
if [[ -n "$(git -C "$NIXIANG" status --porcelain -- "$JAVA_SRC_REL" 2>/dev/null || true)" ]]; then
  echo "  WARN: app/src/main/java 有未提交改动 —— 本 dex 含未提交代码，跨机复现可能不一致（提交后再刷一次更稳）"
fi

echo
echo "模板已生成: $OUT"
ls -l "$OUT"
shasum -a 256 "$OUT/classes2.dex"
echo
echo "新鲜度自检:"
bash "$NIXIANG/04_scripts/_common/menu_tpl_gate.sh" --tpl "$OUT" --dex-name classes2.dex --require-stamp
echo
echo "出包（12.x 线；vc1637 线再跑 scripts/wxsj_vc1637_menu_tpl.sh 派生）："
echo "  WXSJ_PHASE=release bash scripts/build_wxsj_12.10701.sh"
