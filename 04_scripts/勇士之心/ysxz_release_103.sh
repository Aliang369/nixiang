#!/usr/bin/env bash
# 勇士之心 1.0.3 星光版「正式包」流水线（可移植：Windows Git-Bash / macOS / Linux）。
#
# 组成（三个输入缺一不可，且必须同源同一轮构建）：
#   底包  02_apks/勇士之心_1.0.3.apk                  （官方资源 + 官方 dex，不动）
#   菜单清单+功能  mod菜单/starry-mod-menu/libs_ysxz/arm64-v8a/libstarry_magic.so
#   菜单外观+授权  mod菜单/starry-mod-menu/libs_ysxz/classes4.dex
#
# 流程：断言 → 外科替换（其余条目逐字节保留）→ zipalign → 同钥匙重签 → 校验 → 归集交付
#
# 用法：
#   bash 04_scripts/勇士之心/ysxz_release_103.sh              # 用现成产物出包
#   bash 04_scripts/勇士之心/ysxz_release_103.sh --build      # 先从源码重建 .so 与 classes4.dex
#   bash 04_scripts/勇士之心/ysxz_release_103.sh --out DIR    # 指定输出目录（默认 08_output/勇士之心）
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${NIXIANG:-$(cd "$HERE/../.." && pwd)}"
GAME="勇士之心"
VERSION="1.0.3"
PKG="com.fightingstars.games.warrior"
STAGE="${STAGE:-20261004}"

SRC_APK="${ORIG_APK:-$ROOT/02_apks/${GAME}_1.0.3.apk}"
SO="${YSXZ_SO:-$ROOT/mod菜单/starry-mod-menu/libs_ysxz/arm64-v8a/libstarry_magic.so}"
DEX="${YSXZ_DEX:-$ROOT/mod菜单/starry-mod-menu/libs_ysxz/classes4.dex}"
KS="${KS:-$ROOT/01_tools/starry_release.jks}"
KS_PASS="${KS_PASS:-starry2026}"
KS_ALIAS="${KS_ALIAS:-starry}"
WANT_CERT="83C5CC0BE218D046B3B7AF0D2A9F8236297FE228171AF2974B71BAC130736F1D"
DO_BUILD=0
OUT=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --build) DO_BUILD=1; shift ;;
    --out) OUT="${2:?--out 需要目录}"; shift 2 ;;
    -h|--help) sed -n '2,18p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $1" >&2; exit 2 ;;
  esac
done
OUT="${OUT:-$ROOT/08_output/$GAME}"
DELIVER="$OUT/delivery_$STAGE"

say() { printf '%s\n' "$*"; }
die() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }
h()   { printf '\n=== %s ===\n' "$*"; }

# ---------------- 0. 工具链（不写死机器路径） ----------------
h "0/7 工具链"
if [[ -z "${JAVA_HOME:-}" && -x /usr/libexec/java_home ]]; then
  JAVA_HOME="$(/usr/libexec/java_home -v 17 2>/dev/null || true)"
fi
if [[ -z "${JAVA_HOME:-}" || ! -x "$JAVA_HOME/bin/java" ]]; then
  for c in "/c/Program Files/Microsoft/jdk-17.0.20.101-hotspot" "/c/Program Files/Java/jdk-17" "$HOME/.jdks"; do
    [[ -x "$c/bin/java.exe" || -x "$c/bin/java" ]] && { JAVA_HOME="$c"; break; }
  done
fi
JAVA="$(command -v java 2>/dev/null || true)"
[[ -n "${JAVA_HOME:-}" && -x "$JAVA_HOME/bin/java" ]] && JAVA="$JAVA_HOME/bin/java"
[[ -x "${JAVA:-/nonexistent}" || -n "${JAVA:-}" ]] || die "找不到 java（装 JDK 17 或设 JAVA_HOME）"
say "  JDK : $("$JAVA" -version 2>&1 | head -1)"

SDK=""
for cand in "${ANDROID_SDK_ROOT:-}" "${ANDROID_HOME:-}" \
            "${LOCALAPPDATA:+$(cygpath -u "$LOCALAPPDATA" 2>/dev/null || echo "$LOCALAPPDATA")/Android/Sdk}" \
            "$HOME/Android/Sdk" "$HOME/Library/Android/sdk"; do
  [[ -n "$cand" && -d "$cand" ]] || continue
  [[ -d "$cand/build-tools" || -d "$cand/platforms" ]] && { SDK="$cand"; break; }
done
[[ -n "$SDK" ]] || die "找不到 Android SDK（设 ANDROID_HOME=/path/to/Android/Sdk）"
BT="$(ls -d "$SDK"/build-tools/*/ 2>/dev/null | sort -V | tail -1 || true)"
BT="${BT%/}"
[[ -n "$BT" ]] || die "找不到 build-tools（$SDK/build-tools）"
ZL="$BT/zipalign"; [[ -x "$ZL.exe" ]] && ZL="$ZL.exe"
AS="$BT/lib/apksigner.jar"
[[ -f "$AS" ]] || die "缺 $AS"
say "  SDK : $SDK"
say "  BT  : $BT"
say "  sign: $("$JAVA" -jar "$AS" version 2>&1 | head -1)"

# ---------------- 1. 输入 ----------------
h "1/7 输入与前置断言"
if [[ "$DO_BUILD" == "1" ]]; then
  say "  --build：先从源码重建 so 与 classes4.dex"
  bash "$HERE/ysxz_build_so_103.sh"
  bash "$HERE/ysxz_build_menu_dex.sh"
fi
[[ -f "$SRC_APK" ]] || die "缺底包 $SRC_APK"
[[ -f "$SO" ]]      || die "缺菜单 so ${SO}（跑 --build 或先执行 ysxz_build_so_103.sh）"
[[ -f "$DEX" ]]     || die "缺菜单 dex ${DEX}（跑 --build 或先执行 ysxz_build_menu_dex.sh）"
say "  底包: $SRC_APK ($(wc -c <"$SRC_APK") B, md5 $(md5sum "$SRC_APK" | cut -d' ' -f1))"
say "  so  : $SO ($(wc -c <"$SO") B, md5 $(md5sum "$SO" | cut -d' ' -f1))"
say "  dex : $DEX ($(wc -c <"$DEX") B, md5 $(md5sum "$DEX" | cut -d' ' -f1))"

# so 必须是新版 7 行菜单（1 号位伤害倍率滑块），否则菜单序号会与 JS 补丁错位
# 说明：菜单串/日志串里含 `[`（如 [auth]），必须用 grep -F 固定串匹配，
#       否则 `[auth]` 会被当成正则字符类 ⇒ 假阴性（2026-10-04 踩过）。
for tok in "Slider_伤害倍率:1:50:1" "Slider_移动速度:1:10:1" "Toggle_自动战斗"; do
  LC_ALL=C grep -aqF -- "$tok" "$SO" || die "so 缺新菜单串 [$tok] —— 这是旧菜单 so，先跑 --build"
done
LC_ALL=C grep -aqF -- "Slider_移动速度:1:5:1" "$SO" && die "so 仍是旧菜单（含 Slider_移动速度:1:5:1）"
say "  ✓ so 菜单表 = 新版 7 行"

# 自动战斗指纹回归门禁（2026-10-05 事故：本脚本打包过未同步 09-21 修法的旧指纹 so，
# 热更 res 1.1.4.0 改名后 tryAuto 静默跳过 → 自动战斗失效且无任何日志）。
LC_ALL=C grep -aqF -- 'BuffTableEnum' "$SO" || die "so 自动战斗指纹是旧版（缺 BuffTableEnum）—— 检查 ysxz_1.0.3_menu/YsxzPatchJs.h 后 --build"
LC_ALL=C grep -aqF -- 'rejected by fingerprint' "$SO" || die "so 缺自动战斗拒绝诊断日志（仍是旧指纹 so）"
LC_ALL=C grep -aqF -- 'indexOf("bVs(")' "$SO" && die "so 仍写死旧混淆名 bVs( —— 自动战斗在热更后必失效"
say "  ✓ so 自动战斗 = 两级指纹 + 拒绝诊断"

LC_ALL=C grep -aqF -- 'MagicRes$PanelBgDrawable' "$DEX" || die "dex 无 MagicRes\$PanelBgDrawable —— 菜单外观是美化前的旧版"
LC_ALL=C grep -aqF -- 'YsxzAppComponentFactory' "$DEX" && die "dex 混进 1.0.4 过检测类 YsxzAppComponentFactory"
for tok in "[auth] store verdict=" "cold-start self-heal" "auth_backup_"; do
  LC_ALL=C grep -aqF -- "$tok" "$DEX" || die "dex 缺授权修法标记 [$tok] —— 这是修法前的 dex"
done
say "  ✓ dex = 星空外观 + 二次打开授权修法"

# ---------------- 2. 外科替换 ----------------
h "2/7 外科替换（只动 classes4.dex 与 libstarry_magic.so）"
WORK="$(mktemp -d "${TMPDIR:-/tmp}/ysxz_release.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT
UNSIGNED="$WORK/${GAME}_${VERSION}_starry-final-unsigned.apk"
python3 "$HERE/ysxz_repack_103.py" "$SRC_APK" "$SO" "$UNSIGNED" "$DEX"

# ---------------- 3. 对齐 ----------------
h "3/7 zipalign"
ALIGNED="$WORK/aligned.apk"
"$ZL" -p -f 4 "$UNSIGNED" "$ALIGNED" 2>/dev/null || { say "  -p 失败 → 退回普通 4 字节对齐"; "$ZL" -f 4 "$UNSIGNED" "$ALIGNED" >/dev/null; }
"$ZL" -c -v 4 "$ALIGNED" >/dev/null 2>&1 && say "  ✓ 4 字节对齐校验通过" || die "zipalign 校验失败"

# ---------------- 4. 签名 ----------------
h "4/7 重签名（与线上同一把星光钥匙，install -r 可保数据）"
[[ -f "$KS" ]] || die "缺钥匙 $KS"
SIGNED="$WORK/${GAME}_${VERSION}_starry-final-releaseSigned.apk"
"$JAVA" -jar "$AS" sign \
  --ks "$KS" --ks-pass "pass:$KS_PASS" --ks-key-alias "$KS_ALIAS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$ALIGNED"
say "  ✓ 已签名"

# ---------------- 5. 校验 ----------------
h "5/7 校验"
# 注意：apksigner 的 SHA-256 digest 输出**不带冒号**（64 位十六进制）。
# 用 [0-9a-f:]{64,95} 同时兼容带/不带冒号两种写法；再加 || true 避免 set -e 静默退出。
CERT_OUT="$("$JAVA" -jar "$AS" verify --print-certs "$SIGNED" 2>&1 || true)"
GOT="$(printf '%s\n' "$CERT_OUT" | grep -i 'SHA-256 digest' | head -1 | grep -oiE '[0-9a-f:]{64,95}' | tr -d ':' | tr 'a-f' 'A-F' || true)"
[[ "$GOT" == "$WANT_CERT" ]] || die "证书指纹不一致（got=[$GOT] ≠ want=${WANT_CERT}）⇒ install -r 会丢数据"
say "  ✓ 证书 SHA-256 ${GOT}（与线上包一致）"
"$JAVA" -jar "$AS" verify -v "$SIGNED" 2>&1 | grep -iE 'Verified using|Number of signers' | sed 's/^/    /'

python3 - "$SIGNED" "$SO" "$DEX" <<'PY'
# 注意：Windows 下 python3 的 stdout 默认 GBK，本段输出只用 ASCII，
#       避免 UnicodeEncodeError（'gbk' codec can't encode character '\u2713'）。
import hashlib, sys, zipfile
apk, so, dex = sys.argv[1], sys.argv[2], sys.argv[3]
z = zipfile.ZipFile(apk)
def md5of(name):
    return hashlib.md5(z.read(name)).hexdigest()
def md5f(p):
    return hashlib.md5(open(p, "rb").read()).hexdigest()
exp_so, got_so = md5f(so), md5of("lib/arm64-v8a/libstarry_magic.so")
exp_dex, got_dex = md5f(dex), md5of("classes4.dex")
ok = True
def rep(label, got, exp=None):
    global ok
    if exp is None:
        print("    %-26s %s" % (label, got))
        return
    good = got == exp
    ok = ok and good
    print("    %-26s %s  %s" % (label, got, "OK" if good else "MISMATCH expected " + exp))
rep("classes4.dex", got_dex, exp_dex)
rep("libstarry_magic.so", got_so, exp_so)
for n in ("classes.dex", "classes2.dex", "classes3.dex", "resources.arsc", "lib/arm64-v8a/libcocos2djs.so"):
    rep(n, md5of(n))
sys.exit(0 if ok else 1)
PY

# ---------------- 6. 归集交付 ----------------
h "6/7 归集到 $OUT"
mkdir -p "$OUT" "$DELIVER"
APKNAME="${GAME}_${VERSION}_starry-final-releaseSigned.apk"
# 只把**唯一正式包**放进 08_output；unsigned 属构建中间物，留在工作目录（收尾由
# _common/finalize_game_delivery.sh --prune 裁成唯一 APK，见 AGENTS.md「收尾」节）。
cp -f "$SIGNED" "$OUT/$APKNAME"
# 清单里 apk 写相对路径 ../xxx.apk，这样在 delivery 目录里 sha256sum -c 可直接校验
{
  echo "# ${GAME} ${VERSION} 星光正式包 ${STAGE} —— 交付清单 (sha256)"
  sha256sum "$OUT/$APKNAME" | sed "s#$OUT/#../#"
  # 两个被替换条目不再复制副本进 delivery（构建产物不留本地，且可 --build 复现）；
  # 只登记源文件哈希供核对 —— 条目内容已在上一步逐条与包内实测 md5 比对通过。
  echo "# 被替换条目的源文件哈希（不入库、不复制副本）"
  echo "#   $(basename "$SO")  sha256 $(sha256sum "$SO" | cut -d' ' -f1)"
  echo "#   $(basename "$DEX") sha256 $(sha256sum "$DEX" | cut -d' ' -f1)"
} > "$DELIVER/SHA256SUMS.txt"
say "  清单："
sed 's/^/    /' "$DELIVER/SHA256SUMS.txt"
( cd "$DELIVER" && sha256sum -c SHA256SUMS.txt 2>&1 | sed 's/^/    /' ) || true

# 交付说明（随包生成，绝对路径/摘要值每次都刷新）
APK_MD5="$(md5sum "$OUT/$APKNAME" | cut -d' ' -f1)"
APK_SHA="$(sha256sum "$OUT/$APKNAME" | cut -d' ' -f1)"
APK_SZ="$(wc -c <"$OUT/$APKNAME")"
SO_MD5="$(md5sum "$SO" | cut -d' ' -f1)"
DEX_MD5="$(md5sum "$DEX" | cut -d' ' -f1)"
cat > "$DELIVER/说明.md" <<EOF
# ${GAME} ${VERSION} 星光正式包（${STAGE}）

## 这是什么

1.0.3 的星光补丁包：官方资源与官方 dex 全部保持原样，只替换两个条目 ——

| 条目 | 内容 | 本轮值 |
|---|---|---|
| classes4.dex | 菜单外观（星空面板/星光紫）+ 菜单授权链路 | ${DEX_MD5} |
| lib/arm64-v8a/libstarry_magic.so | 菜单清单（7 行）+ 各功能原生实现 | ${SO_MD5} |

同目录另有 icon_${GAME}_512.png / icon_${GAME}_144.png（由 finalize_game_delivery.sh 从
10_docs/${GAME}/yszzx_icon.png 生成）、menu_template.json（菜单模板副本），以及
SHA256SUMS.txt（在 delivery 目录里执行 sha256sum -c SHA256SUMS.txt 可校验主包）。
两个被替换条目本身不复制副本（构建产物，可 --build 复现），哈希见 SHA256SUMS.txt 注释。

## 文件

- 主包：../${APKNAME}
- 体积：${APK_SZ} B
- md5：${APK_MD5}
- sha256：${APK_SHA}
- 包名：${PKG}
- 签名：CN=StarryMagic，SHA-256 ${WANT_CERT}
  （与线上已装的星光包同一把钥匙 ⇒ install -r 不会清数据）

## 本轮改了什么

1. 菜单授权「第二次打开变未授权」修复（classes4.dex）：
   - 设备号存了就必须认（不再每次重算）
   - 云端设备类失败先静默自愈重登，可自愈类失败永不清档
   - 心跳带上历史设备号，服务端可把卡密绑定迁到新设备
   - 新增 [auth] store verdict= 一行日志，云机排障不用猜
2. 菜单清单为 7 行版（伤害倍率 / 秒杀 / 无敌 / 无限技能 / 移动速度 / 自动战斗）。

## 装机与验收

    adb install -r "${APKNAME}"

1. 打开游戏，菜单应为「授权有效」，不必重输卡密；
2. 彻底杀掉再打开 2~3 次，仍应为「授权有效」；
3. 取日志：

    adb logcat -d | grep -a '\\[auth\\] store verdict='

判读：

| 日志 | 含义 |
|---|---|
| verdict=ok ... source=cached ... grant_match=true | 存档设备号被信任，票据匹配（最常见，已修） |
| cold-start self-heal ... relogin 且随后 verdict=ok | 设备号漂移过一次，已静默迁移（已修） |
| verdict=heal-deferred ... keep archive | 暂时救不动，但卡密与存档保留，下次冷启动重试 |
| verdict=kick ... auth_reason=kick:xxx | 权威致命（封禁/过期/用尽/不存在），卡密本身有问题 |

只要不出现 kick，存档就不会被清。

## 复现这条流水线

    bash 04_scripts/勇士之心/ysxz_release_103.sh          # 用现成 so/dex
    bash 04_scripts/勇士之心/ysxz_release_103.sh --build  # 从源码重编 so + dex 再出包

前置：02_apks/${GAME}_1.0.3.apk（底包）、01_tools/starry_release.jks（同一把钥匙）、JDK 17、Android SDK build-tools。
EOF
say "  说明：$DELIVER/说明.md"

# 标准交付物（icon 512/144 + 菜单模板副本）+ 裁成唯一正式包。
# 收尾一律走一键脚本、禁手工重造交付结构（AGENTS.md「收尾」节）；
# --icon-source 缺失时 finalize 只跳过 icon，不报错。
FINALIZE="$ROOT/04_scripts/_common/finalize_game_delivery.sh"
if [[ -f "$FINALIZE" ]]; then
  bash "$FINALIZE" --game "$GAME" --apk "$OUT/$APKNAME" \
    --icon-source "$ROOT/10_docs/${GAME}/yszzx_icon.png" \
    --menu-template "$ROOT/10_docs/${GAME}/yszzx_menu_template.json" \
    --prune --apply \
    || say "  ⚠ finalize 未全绿（见上）：正式包已就位 $OUT/${APKNAME}，请按提示补 delivery/裁剪"
else
  say "  ⚠ 缺 ${FINALIZE}：跳过标准归集（icon/菜单模板/裁剪）"
fi

# ---------------- 7. 摘要 ----------------
h "7/7 完成"
FINAL="$OUT/$APKNAME"
say "  交付包 : $FINAL"
say "  包名   : $PKG"
say "  体积   : $(wc -c <"$FINAL") B"
say "  md5    : $(md5sum "$FINAL" | cut -d' ' -f1)"
say "  sha256 : $(sha256sum "$FINAL" | cut -d' ' -f1)"
say "  清单   : $DELIVER/SHA256SUMS.txt"
say ""
say "  装机（保数据）：adb install -r \"$FINAL\""
say "  冷启动判授权：adb logcat -d | grep -a '\\[auth\\] store verdict='"
