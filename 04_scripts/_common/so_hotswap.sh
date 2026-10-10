#!/usr/bin/env bash
# ================================================================
#  so_hotswap.sh — 已装机包的 so 原地热替换（只改 so 的轮次：秒级迭代，免重打包/免重装）
#
#  背景（2026-10-10 明日方舟复盘 P0-1）：
#    上一轮的迭代成本是「改一处 → 重打 1.8 GB 包 → 装机 → 用户手测」，一轮十几分钟到半小时。
#    实测该包 `extractNativeLibs=true` ⇒ so 已解压到 `/data/app/~~xxx/<pkg>-yyy/lib/arm64/`，
#    该目录 root 可原地替换（chcon apk_data_file 后标签正确），**181 KB push + 重启即可换 so**。
#    于是「只改 so」的诊断/确认轮可以不走完整链路，把反馈周期压到 1 分钟内。
#
#  用法:
#    bash 04_scripts/_common/so_hotswap.sh --serial <序列号> --so <本机 so 路径> --pkg <包名> \
#         [--lib-name libstarry_magic.so] [--restart] [--dry-run] [--judge <判读脚本>]
#    bash 04_scripts/_common/so_hotswap.sh --serial S --pkg P --restore-from <正式APK> [--lib-name …]
#
#  行为:
#    ① `pm path` 解析已安装 APK 私有目录（同签名/同包名，不需要卸载）
#    ② 校验目标 so **已解压存在**（不存在 ⇒ 该包 extractNativeLibs=false，热替换不适用，直接 FAIL）
#    ③ push 到 /data/local/tmp/ → su -c 'cp + chmod 755 + chcon apk_data_file' → 回读 md5 比对
#    ④ --restart：force-stop + monkey 冷启（让新 so 生效；不改 APK 本体）
#    ⑤ --judge：跑指定判读脚本（默认不跑）
#    ⑥ 在设备 /data/local/tmp/starry_magic/hotswap_state.txt 留下热替换台账（谁换了、md5、时间）
#
#  ⚠️ 硬边界（写进 TOOLBOOK）：
#    · 热替换**不改 APK 本体** ⇒ 设备 base.apk 的 md5 仍等于交付包 md5；
#      但**运行中的 so 与包内 so 不再一致**，任何「设备形态」证据必须标注「热替换态」。
#    · 交付/确认轮、改 dex・清单・签名、验证新装（extractNativeLibs）、ACE 形态验证，
#      一律回到完整链路（`pipeline.sh <游戏> reset build_so` → `run` → 门禁 → 装机）。
#    · 收尾前必须 `--restore-from <正式APK>`（或重装正式包），否则设备形态与交付包不一致。
#
#  退出码: 0=替换并核验成功；1=FAIL（含 extractNativeLibs=false / root 不可用 / md5 不符）；2=用法错
#  bash 3.2 兼容（不用 mapfile/关联数组/local -n/${v^^}）
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SERIAL=""; PKG=""; SO=""; LIB_NAME="libstarry_magic.so"
RESTART=0; DRY_RUN=0; JUDGE=""; RESTORE_FROM=""

usage() {
  awk 'NR>1{ if (/^#/) { sub(/^# ?/, ""); print } else { exit } }' "$0"
}

while [ $# -gt 0 ]; do
  case "$1" in
    --serial)       SERIAL="${2:-}"; shift 2 ;;
    --pkg)          PKG="${2:-}"; shift 2 ;;
    --so)           SO="${2:-}"; shift 2 ;;
    --lib-name)     LIB_NAME="${2:-}"; shift 2 ;;
    --restart)      RESTART=1; shift ;;
    --dry-run)      DRY_RUN=1; shift ;;
    --judge)        JUDGE="${2:-}"; shift 2 ;;
    --restore-from) RESTORE_FROM="${2:-}"; shift 2 ;;
    -h|--help)      usage; exit 0 ;;
    *) echo "✗ 未知参数: $1（--help 看用法）" >&2; exit 2 ;;
  esac
done

die() { echo "✗ FAIL  $*" >&2; exit 1; }
[ -n "$SERIAL" ] || { usage; exit 2; }
[ -n "$PKG" ] || { usage; exit 2; }

ABI_DIR="lib/arm64"
DEVDIR="/data/local/tmp/starry_magic"

# ---------- 0. 设备与 root ----------
adb -s "$SERIAL" get-state >/dev/null 2>&1 || die "设备 $SERIAL 不可达（adb devices 看一眼）"
if [ -z "$RESTORE_FROM" ]; then
  [ -n "$SO" ] || { usage; exit 2; }
  [ -f "$SO" ] || die "本机 so 不存在: $SO"
fi
if [ -n "$RESTORE_FROM" ]; then
  [ -f "$RESTORE_FROM" ] || die "--restore-from 的 APK 不存在: $RESTORE_FROM"
fi
ROOT_OK="$(adb -s "$SERIAL" shell "su -c 'id' 2>/dev/null | grep -c 'uid=0' || true" | tr -d '\r' | tr -d ' ')"
case "$ROOT_OK" in ''|*[!0-9]*) ROOT_OK=0 ;; esac
[ "$ROOT_OK" -ge 1 ] || die "root 不可用（su -c 'id' 拿不到 uid=0；热替换必须 root）"

# ---------- 1. 解析已安装 APK 私有目录 ----------
PM_PATH="$(adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' | grep -m1 '^package:' | sed 's|^package:||')"
[ -n "$PM_PATH" ] || die "包未安装或 pm path 解析失败: $PKG"
APKDIR="${PM_PATH%/base.apk}"
[ "$APKDIR" != "$PM_PATH" ] || die "pm path 不是期望的 base.apk 形态: $PM_PATH"
TARGET="${APKDIR}/${ABI_DIR}/${LIB_NAME}"
APK_MD5_DEV="$(adb -s "$SERIAL" shell "md5sum $PM_PATH 2>/dev/null" | tr -d '\r' | awk '{print $1}')"

echo "== 目标 =="
echo "  设备        : $SERIAL"
echo "  包          : $PKG"
echo "  已安装 APK  : $PM_PATH"
echo "  设备 base.apk md5: ${APK_MD5_DEV:-未知}"
echo "  替换目标    : $TARGET"

# ---------- 2. 目标必须已解压存在（否则 extractNativeLibs=false，不适用） ----------
EXIST="$(adb -s "$SERIAL" shell "ls -l $TARGET 2>/dev/null | wc -l" | tr -d '\r' | tr -d ' ')"
case "$EXIST" in ''|*[!0-9]*) EXIST=0 ;; esac
if [ "$EXIST" -lt 1 ]; then
  cat >&2 <<EOF
✗ FAIL  $TARGET 不存在 ⇒ 该包不是 extractNativeLibs=true（so 直接从 APK mmap，无独立文件可替换）。
        本包不适用 so 热替换，只改 so 的轮次也得走完整链路（build_so → 出包 → 装机）。
        判定方法：aapt2 dump xmltree --file AndroidManifest.xml <apk> | grep extractNativeLibs
EOF
  exit 1
fi

# ---------- 3. 取证：替换前 so 指纹 ----------
OLD_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $TARGET'" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
echo "  替换前 so md5: ${OLD_MD5:-未知}"

# ---------- 4. --restore-from：从正式 APK 里抽同名 so 还原 ----------
if [ -n "$RESTORE_FROM" ]; then
  TMP_SO="$(mktemp -t hotswap_restore.XXXXXX)"
  echo "  还原来源    : $RESTORE_FROM"
  unzip -p "$RESTORE_FROM" "lib/arm64-v8a/$LIB_NAME" > "$TMP_SO" 2>/dev/null || {
    rm -f "$TMP_SO"; die "APK 内找不到 lib/arm64-v8a/$LIB_NAME"; }
  [ -s "$TMP_SO" ] || { rm -f "$TMP_SO"; die "抽出的 so 为空"; }
  SO="$TMP_SO"
  echo "  还原 so md5 : $(md5 -q "$SO" 2>/dev/null || md5sum "$SO" | awk '{print $1}')"
fi

LOCAL_MD5="$(md5 -q "$SO" 2>/dev/null || md5sum "$SO" | awk '{print $1}')"
LOCAL_SIZE="$(stat -f '%z' "$SO" 2>/dev/null || stat -c '%s' "$SO")"
echo "  待替换 so   : $SO (${LOCAL_SIZE} B, md5 ${LOCAL_MD5})"
if [ "$LOCAL_MD5" = "$OLD_MD5" ]; then
  echo "  ⚠ 待替换 so 与设备当前 so md5 相同 —— 内容无变化（热替换后行为不会变）"
fi

if [ "$DRY_RUN" = "1" ]; then
  echo "== --dry-run：只做适用性核验，不写设备 =="
  echo "✅ 该包适用 so 热替换（目标存在且 root 可用）。"
  exit 0
fi

# ---------- 5. 落地：push → su cp → chcon → 回读 md5 ----------
STAGE_DEV="/data/local/tmp/_hs_$$.so"
adb -s "$SERIAL" push "$SO" "$STAGE_DEV" >/dev/null 2>&1 || die "push 到 $STAGE_DEV 失败（/data/local/tmp 应可写）"
OUT="$(adb -s "$SERIAL" shell "su -c 'cp $STAGE_DEV $TARGET && chmod 755 $TARGET && chcon u:object_r:apk_data_file:s0 $TARGET' 2>&1" | tr -d '\r')"
[ -z "$OUT" ] || echo "  su 输出: $OUT"
NEW_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $TARGET'" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
adb -s "$SERIAL" shell "rm -f $STAGE_DEV" >/dev/null 2>&1 || true

if [ "$NEW_MD5" != "$LOCAL_MD5" ]; then
  die "替换后设备 so md5 (${NEW_MD5:-空}) != 本机 so md5 (${LOCAL_MD5})"
fi
echo "  ✓ 替换成功：$TARGET md5=$NEW_MD5"

# ---------- 6. 热替换台账（设备侧留痕，避免把热替换态当交付态） ----------
STAMP="$(date '+%Y-%m-%d %H:%M:%S')"
adb -s "$SERIAL" shell "su -c 'mkdir -p $DEVDIR && printf \"%s\\t%s\\t%s\\t%s\\n\" \"$STAMP\" \"$PKG\" \"$LIB_NAME\" \"$LOCAL_MD5\" >> $DEVDIR/hotswap_state.txt'" >/dev/null 2>&1 || true
echo "  热替换台账: $DEVDIR/hotswap_state.txt（收尾前须 --restore-from 正式包还原）"

# ---------- 7. 重启与判读 ----------
if [ "$RESTART" = "1" ]; then
  echo "  重启 $PKG …"
  adb -s "$SERIAL" shell "am force-stop $PKG" >/dev/null 2>&1 || true
  sleep 1
  adb -s "$SERIAL" shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1 || true
  echo "  已冷启（请等游戏起来；热替换态下勿做交付性证据）"
fi
if [ -n "$JUDGE" ]; then
  echo "== 判读 =="
  bash "$JUDGE" || echo "  ⚠ 判读脚本退出码非 0（看它自己的输出定性质）"
fi

cat <<EOF

== 提醒（硬边界）==
  ① 本次**未改 APK 本体** ⇒ 设备 base.apk md5 仍是 ${APK_MD5_DEV}（与交付包比对仍通过）。
  ② 运行中的 so 与包内 so **已不一致** ⇒ 本轮任何结论都要标注「热替换态（so md5=${LOCAL_MD5}）」。
  ③ 交付轮/改 dex・清单・签名/验证新装 ⇒ 回完整链路：pipeline.sh <游戏> reset build_so → run → 门禁。
  ④ 收尾前还原：bash 04_scripts/_common/so_hotswap.sh --serial $SERIAL --pkg $PKG --restore-from <正式APK>
EOF
exit 0
