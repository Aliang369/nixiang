#!/usr/bin/env bash
# 无悔华夏 · 清数据重装 / 装机后状态校验
# 目的：数据被清或热更资源缺失导致游戏卡在资源加载时，做一次干净的「清数据 → 覆盖安装 → 拉起 → 校验」，
#       并在游戏重新下载完热更资源后校验运行时 libil2cpp.so 是否仍是本轮校订 RVA 时用的那一份。
# 用法：
#   bash whrhx_clean_reinstall.sh --check      # 只读：报告当前装机/数据/热更 so 状态，不动任何东西
#   bash whrhx_clean_reinstall.sh --wipe       # 破坏性：pm clear（清档）→ adb install -r → 拉起游戏
#   bash whrhx_clean_reinstall.sh --uninstall  # 破坏性（最彻底）：pm uninstall（连数据目录/外置数据/热更负载一起没了）→ adb install → 拉起游戏
# --wipe / --uninstall 都会丢失游戏存档与本地缓存，必须由用户明确授权后运行。
set -uo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
SERIAL="${WHRHX_SERIAL:-f9a116a2}"
PKG="${WHRHX_PKG:-com.yhhy.whrhx.aligames}"
APK="${WHRHX_APK:-$NIXIANG/08_output/无悔华夏/无悔华夏_3.8.280_科研立即完成热更修复_20260923-releaseSigned.apk}"
RUNTIME_SO="$NIXIANG/06_dumped/无悔华夏/current_20260923/runtime/libil2cpp_runtime_20260923.so"
EXPECT_SO_SHA="c04afff475683c0d0550ba3443d28f6e3dcc53fcf661351aa5692be5e364d3ca"
LEBIAN_LIB="/data/data/$PKG/lebian/$PKG-1/lib/arm64/libil2cpp.so"

MODE="${1:-}"
case "$MODE" in
  --check|--wipe|--uninstall) ;;
  *) echo "用法: $0 --check | --wipe | --uninstall" >&2; exit 64 ;;
esac

echo "== 1. 装机状态 =="
DEV_APK="$(adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | sed -n 's/^package://p' | head -1 | tr -d '\r')"
[[ -n "$DEV_APK" ]] || { echo "ERROR: $PKG 未安装" >&2; exit 1; }
echo "设备路径: $DEV_APK"
DEV_SHA="$(adb -s "$SERIAL" exec-out "su -c 'sha256sum $DEV_APK'" 2>/dev/null | awk '{print $1}')"
echo "设备包 sha256: $DEV_SHA"
LOCAL_SHA="$(shasum -a 256 "$APK" | awk '{print $1}')"
echo "本地产物 sha256: $LOCAL_SHA"
[[ "$DEV_SHA" == "$LOCAL_SHA" ]] && echo "✓ 设备装的就是本轮产物" || echo "! 设备包与本轮产物不一致（可能是你手动装的别版）"

echo
echo "== 2. 数据/热更资源状态 =="
if adb -s "$SERIAL" shell "su -c 'test -d /data/data/$PKG/lebian'" 2>/dev/null; then
  echo "lebian 热更目录: 存在"
  RSO_SHA="$(adb -s "$SERIAL" exec-out "su -c 'sha256sum $LEBIAN_LIB 2>/dev/null'" 2>/dev/null | awk '{print $1}')"
  if [[ -z "$RSO_SHA" ]]; then
    echo "运行时 libil2cpp.so: 缺失 ⇒ 尚未下载热更资源（游戏首启需联网重新下载）"
  else
    echo "运行时 libil2cpp.so sha256: $RSO_SHA"
    if [[ "$RSO_SHA" == "$EXPECT_SO_SHA" ]]; then
      echo "✓ 与本轮校订 RVA 时所用版本一致 ⇒ 科研 RVA 仍然有效"
    else
      echo "! 运行时 so 已变 ⇒ RVA 可能再次漂移，需重新校订（把这份 so 拉回来重跑 Il2CppDumper）"
    fi
  fi
else
  echo "lebian 热更目录: 不存在 ⇒ 数据已被清 / 从未热更过（游戏首启需联网重新下载）"
fi

if [[ "$MODE" == "--check" ]]; then
  echo
  echo "[--check] 只读检查完成，未改动任何设备状态。"
  exit 0
fi

if [[ "$MODE" == "--uninstall" ]]; then
  echo
  echo "== 3. 卸载（破坏性·最彻底）=="
  echo "即将执行: force-stop → pm uninstall $PKG  →  应用数据目录 / 外置数据 / 热更负载全部消失"
  adb -s "$SERIAL" shell "su -c 'am force-stop $PKG'" >/dev/null 2>&1 || true
  adb -s "$SERIAL" shell "su -c 'pm uninstall $PKG'" 2>/dev/null | tr -d '\r'
  if adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | grep -q '^package:'; then
    echo "pm uninstall 未生效，改用 adb uninstall…"
    adb -s "$SERIAL" uninstall "$PKG" || { echo "ERROR: 卸载失败" >&2; exit 1; }
  fi
  if adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | grep -q '^package:'; then
    echo "ERROR: $PKG 仍未卸载" >&2; exit 1
  fi
  echo "✓ 已卸载（$PKG 不再安装）"
  adb -s "$SERIAL" shell "su -c 'test -d /data/data/$PKG && echo 数据目录仍在 || echo 数据目录已清除'" 2>/dev/null | tr -d '\r'

  echo
  echo "== 4. 全新安装本轮产物 =="
  adb -s "$SERIAL" install "$APK" || { echo "ERROR: install 失败" >&2; exit 1; }
else
  echo
  echo "== 3. 清数据（破坏性）=="
  echo "即将执行: pm clear $PKG  →  存档/本地缓存全部丢失（APK 与 mod so 不受影响）"
  adb -s "$SERIAL" shell "su -c 'pm clear $PKG'" || { echo "ERROR: pm clear 失败" >&2; exit 1; }

  echo
  echo "== 4. 覆盖安装本轮产物 =="
  adb -s "$SERIAL" install -r "$APK" || { echo "ERROR: install -r 失败" >&2; exit 1; }
fi
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo
echo "== 5. 拉起游戏（不点屏）=="
adb -s "$SERIAL" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
echo "已拉起。请让游戏联网把热更资源重新下载完（可能要几分钟），然后："
echo "  1) 打开游戏内星空菜单，开启第 9 项「科研立即完成」；"
echo "  2) 做一次国家政策/科技研发 + 一次名臣政策研发；"
echo "  3) 回来跑: bash 04_scripts/无悔华夏/whrhx_research_log_check.sh"
echo "另建议热更下载完成后复跑本脚本 --check，确认运行时 so sha 仍为 ${EXPECT_SO_SHA:0:8}…。"
