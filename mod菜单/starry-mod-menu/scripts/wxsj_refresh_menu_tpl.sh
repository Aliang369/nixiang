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
STARRY_APK="$APP/app/build/outputs/apk/debug/app-debug.apk"
SMALI="$NIXIANG/01_tools/smali/smali"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
APP_CLASS="Lcom/netease/nis/wrapper/MyApplication;"

PREV="${WXSJ_TPL_PREV:-$APP/templates/wxsj_menu_20260922}"
NAME="${WXSJ_TPL_NAME:-wxsj_menu_$(date +%Y%m%d)}"
OUT="$APP/templates/$NAME"
WORK="$(mktemp -d /tmp/wxsj_tpl.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

for f in "$SMALI" "$BAKSMALI"; do
  [[ -x "$f" ]] || { echo "ERROR: missing $f"; exit 1; }
done

echo "[1/6] gradle 离线构建 debug apk"
(cd "$APP" && ./gradlew --offline assembleDebug -q)
[[ -f "$STARRY_APK" ]] || { echo "ERROR: $STARRY_APK 未产出"; exit 1; }
echo "  apk: $(stat -f %z "$STARRY_APK") B  mtime=$(stat -f %Sm "$STARRY_APK")"

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

echo "[5/6] smali assemble → classes2.dex"
"$SMALI" assemble -o "$WORK/classes2.dex" "$SMALI_IN" >/dev/null
[[ -s "$WORK/classes2.dex" ]] || { echo "ERROR: 组装失败"; exit 1; }
LC_ALL=C grep -aq 'com/starry/magic/StarryInitProvider' "$WORK/classes2.dex" \
  || { echo "ERROR: 新 dex 内缺 StarryInitProvider"; exit 1; }
NEWN=$("$BAKSMALI" list classes "$WORK/classes2.dex" | wc -l | tr -d ' ')
echo "  classes2.dex = $(stat -f %z "$WORK/classes2.dex") B / $NEWN 类"

echo "[6/6] 与旧模板比对（零删除为硬性要求）"
if [[ -s "$PREV/classes2.dex" ]]; then
  "$BAKSMALI" list classes "$PREV/classes2.dex" | sort > "$WORK/prev_cls.txt"
  "$BAKSMALI" list classes "$WORK/classes2.dex" | sort > "$WORK/new_cls.txt"
  comm -23 "$WORK/prev_cls.txt" "$WORK/new_cls.txt" > "$WORK/lost.txt" || true
  if [[ -s "$WORK/lost.txt" ]]; then
    echo "ERROR: 新 dex 相对 $PREV 丢类（会导致菜单缺控件/外观变样）："
    sed 's/^/    /' "$WORK/lost.txt"
    exit 1
  fi
  echo "  旧模板 $(wc -l < "$WORK/prev_cls.txt" | tr -d ' ') 类 → 新 $(wc -l < "$WORK/new_cls.txt" | tr -d ' ') 类，零删除 ✓"
  if [[ -s "$WORK/lost.txt" ]]; then :; fi
  added="$(comm -13 "$WORK/prev_cls.txt" "$WORK/new_cls.txt" || true)"
  [[ -n "$added" ]] && { echo "  新增:"; printf '    %s\n' $added; }
else
  echo "  WARN: 对照模板 $PREV 不可用，跳过零删除断言（首次生成？）"
fi

mkdir -p "$OUT"
cp -f "$WORK/classes2.dex" "$OUT/classes2.dex"
unzip -p "$STARRY_APK" lib/arm64-v8a/libc++_shared.so > "$OUT/libc++_shared.so"
unzip -p "$STARRY_APK" assets/certs/starlight.crt > "$OUT/starlight.crt"
if [[ -s "$PREV/libc++_shared.so" ]]; then
  a=$(md5 -q "$PREV/libc++_shared.so"); b=$(md5 -q "$OUT/libc++_shared.so")
  [[ "$a" == "$b" ]] || echo "  WARN: libc++_shared.so 与旧模板不同（$a → ${b}）"
  a=$(md5 -q "$PREV/starlight.crt"); b=$(md5 -q "$OUT/starlight.crt")
  [[ "$a" == "$b" ]] || echo "  WARN: starlight.crt 与旧模板不同（$a → ${b}）"
fi

echo
echo "模板已生成: $OUT"
ls -l "$OUT"
shasum -a 256 "$OUT/classes2.dex"
echo
echo "出包（已默认指向新模板，无需额外环境变量）:"
echo "  WXSJ_PHASE=release bash scripts/build_wxsj_12.10701.sh"
