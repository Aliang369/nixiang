#!/usr/bin/env bash
# 明日方舟 - 装机阶段（pipeline install）【版本无关，版本取自 official_apk.json】
#
# 默认只做同签名覆盖安装。TapTap 官方包为 HyperGryph 签名，星光包为
# StarryMagic 签名，异签覆盖会失败；卸载/清数据需 MRFZZ_ALLOW_UNINSTALL=1。
#
# 双机口径（2026-11-27 win/雷电适配）：
#   - adb 解析：ADB_BIN > 脚本目录 .device_adb > PATH 上的 adb。
#     Win 上 PATH 里常是 C:\windows\system32\adb.exe（33.x），与雷电自带 34.x
#     client/server 混用会报版本不一致，且 1.9 GB 大包推送更需要 34 的稳定性
#     ⇒ 解析到的 adb 所在目录前置进 PATH，下游脚本里的裸 adb 用同一个二进制。
#   - 设备序列号：脚本目录 .device_serial；并 export ANDROID_SERIAL，让不带 -s 的
#     下游脚本（ensure_starry_device_id_seed.sh）也打对设备。
#   - 大包超时：adb install / pm install 可用 MRFZZ_PM_TIMEOUT（秒，默认 1800）封顶。
#   - 中文路径：adb.exe 是 ANSI argv，含中文的 APK 路径可能被 mangle ⇒ 装机前把
#     APK 硬链接（失败则拷贝）到纯 ASCII 暂存路径，装完删除。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG/04_scripts/_common/env.sh"
GAME="明日方舟"
PKG="com.hypergryph.arknights"
META_FILE="$SCRIPT_DIR/official_apk.json"
[[ -f "$META_FILE" ]] || { echo "错误: 缺少 ${META_FILE}，请先跑 fetch_official 阶段" >&2; exit 1; }
VERSION="${MRFZZ_VERSION:-$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1],encoding="utf-8"))["version_name"])' "$META_FILE")}"
SIGNED="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_HINT_FILE="$SCRIPT_DIR/.device_adb"
OK_STAMP="$SCRIPT_DIR/.install_ok"
PM_TIMEOUT="${MRFZZ_PM_TIMEOUT:-1800}"

# ---------- adb 解析 ----------
ADB_BIN="${ADB_BIN:-}"
if [ -z "$ADB_BIN" ] && [ -f "$ADB_HINT_FILE" ]; then
  ADB_BIN="$(tr -d '[:space:]' < "$ADB_HINT_FILE")"
fi
if [ -z "$ADB_BIN" ]; then
  ADB_BIN="$(command -v adb || true)"
fi
if [ -z "$ADB_BIN" ] && [ "${NIXIANG_HOST:-}" = "win" ]; then
  for _c in "/e/leidian/LDPlayer14/adb.exe" "/c/leidian/LDPlayer14/adb.exe"; do
    [ -f "$_c" ] && { ADB_BIN="$_c"; break; }
  done
  unset _c
fi
[ -n "$ADB_BIN" ] || { echo "ERROR: adb 不可用（export ADB_BIN=/path/to/adb，或把路径写进 ${ADB_HINT_FILE}）" >&2; exit 1; }
case "$ADB_BIN" in
  */*) ;;
  *)   ADB_BIN="$(command -v "$ADB_BIN" || true)" ;;
esac
[ -n "$ADB_BIN" ] || { echo "ERROR: adb 解析失败" >&2; exit 1; }
ADB_DIR="$(cd "$(dirname "$ADB_BIN")" && pwd)"
case ":$PATH:" in
  *":$ADB_DIR:"*) : ;;
  *) PATH="$ADB_DIR:$PATH" ;;
esac
export PATH

# ---------- 小工具（mac bash3 兼容；win 用 md5sum，mac 用 md5 -q） ----------
file_md5() {
  if command -v md5sum >/dev/null 2>&1; then
    md5sum "$1" | awk '{print $1}'
  elif command -v md5 >/dev/null 2>&1; then
    md5 -q "$1"
  else
    echo ""
  fi
}
# 交给原生 adb.exe 的路径：win 转成 E:/… 形式（cygpath -m），其余原样
adb_path() {
  if [ "${NIXIANG_HOST:-}" = "win" ]; then nixiang_win_path "$1"; else printf '%s' "$1"; fi
}
run_timed() {   # run_timed <秒> <cmd...>；无 timeout 命令时直接跑
  _t="$1"; shift
  if command -v timeout >/dev/null 2>&1; then timeout "$_t" "$@"; else "$@"; fi
}

[[ -f "$SIGNED" ]] || { echo "ERROR: missing signed apk $SIGNED" >&2; exit 1; }
[[ -f "$STATE_FILE" ]] || { echo "ERROR: missing $STATE_FILE; run device_ready first" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty device serial" >&2; exit 1; }
export ANDROID_SERIAL="$SERIAL"
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1 \
  || { echo "ERROR: device $SERIAL offline（${ADB_BIN}）" >&2; exit 1; }
echo "==> adb=$ADB_BIN serial=$SERIAL"

REMOTE="/data/local/tmp/mrfzz_install.apk"

# ---------- 中文路径规避：ASCII 暂存（硬链接优先，失败拷贝） ----------
STAGE_DIR="$NIXIANG/13_scratch"
mkdir -p "$STAGE_DIR"
STAGE_APK="$STAGE_DIR/mrfzz_install_${VERSION}.apk"
if [ "$(adb_path "$SIGNED")" != "$SIGNED" ]; then
  rm -f "$STAGE_APK"
  if ! ln "$SIGNED" "$STAGE_APK" 2>/dev/null; then
    echo "==> 硬链接不可用，拷贝到 ASCII 暂存路径（约 1.8 GB）"
    cp "$SIGNED" "$STAGE_APK"
  fi
  INSTALL_SRC="$STAGE_APK"
else
  INSTALL_SRC="$SIGNED"
fi
cleanup_stage() { rm -f "$INSTALL_SRC" 2>/dev/null || true; }
trap cleanup_stage EXIT

# ---------- 卸载（用户已授权：MRFZZ_ALLOW_UNINSTALL=1 + MRFZZ_UNINSTALL_FIRST=1） ----------
if [ "${MRFZZ_UNINSTALL_FIRST:-0}" = "1" ]; then
  [ "${MRFZZ_ALLOW_UNINSTALL:-0}" = "1" ] || {
    echo "ERROR: MRFZZ_UNINSTALL_FIRST=1 必须同时给 MRFZZ_ALLOW_UNINSTALL=1（会清掉本地数据）" >&2; exit 2; }
  echo "==> 卸载官方包（清本地数据：已下载资源 / 本地设置；账号进度在服务端）"
  "$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  # pm path 在「未安装」时 rc=1：set -e 下裸赋值会直接中断脚本（2026-11-27 实测踩到：
  # 卸载成功却没继续装）。故设备侧探测一律 || true 兜住，卸载成功与否以 pm path 复核为准。
  BEFORE_PATH="$("$ADB_BIN" -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' || true)"
  if [ -z "$BEFORE_PATH" ]; then
    echo "  ! $PKG 本就不在，跳过卸载"
  else
    UN_OUT="$("$ADB_BIN" -s "$SERIAL" uninstall "$PKG" 2>&1 | tr -d '\r' || true)"
    printf '%s\n' "$UN_OUT"
    case "$UN_OUT" in
      *Success*) echo "  卸载 Success" ;;
      *) echo "  ! 卸载未返回 Success（部分 ROM 对已删包报 DELETE_FAILED_INTERNAL_ERROR），以 pm path 复核" ;;
    esac
    LEFT="$("$ADB_BIN" -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | tr -d '\r' || true)"
    [ -z "$LEFT" ] || { echo "ERROR: 卸载后 pm path 仍有返回：$LEFT" >&2; exit 3; }
    echo "  复核: pm path 为空（包已移除）"
  fi
fi

echo "==> install $INSTALL_SRC"
"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true

install_via_adb() {   # adb install 直装（内部自动 push 到 /data/local/tmp）
  run_timed "$PM_TIMEOUT" "$ADB_BIN" -s "$SERIAL" install -r -d --user 0 "$(adb_path "$INSTALL_SRC")" 2>&1 | tr -d '\r' || true
}
install_via_push() {  # 退回 push + root pm install
  echo "  → 退回 push + pm install"
  _rmd5="$("$ADB_BIN" -s "$SERIAL" shell "md5sum $REMOTE 2>/dev/null | cut -d' ' -f1" | tr -d '\r')"
  if [ -n "$_rmd5" ] && [ "$_rmd5" = "$(file_md5 "$INSTALL_SRC")" ]; then
    echo "  /data/local/tmp 已有同 md5 副本，跳过 push"
  else
    "$ADB_BIN" -s "$SERIAL" push "$(adb_path "$INSTALL_SRC")" "$REMOTE" 2>&1 | tail -2
  fi
  run_timed "$PM_TIMEOUT" "$ADB_BIN" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>&1 | tr -d '\r' || true
}

INSTALL_OUT="$(install_via_adb || true)"
printf '%s\n' "$INSTALL_OUT" | tail -5

if ! printf '%s\n' "$INSTALL_OUT" | grep -q "Success"; then
  if printf '%s\n' "$INSTALL_OUT" | grep -qE "INSTALL_FAILED_UPDATE_INCOMPATIBLE|signatures do not match"; then
    if [ "${MRFZZ_ALLOW_UNINSTALL:-0}" != "1" ]; then
      echo "ERROR: 官方包签名不同，覆盖安装被拒；卸载会清本地数据，须显式批准：" >&2
      echo "  MRFZZ_ALLOW_UNINSTALL=1 MRFZZ_UNINSTALL_FIRST=1 bash 04_scripts/明日方舟/mrfzz_install.sh" >&2
      exit 2
    fi
    echo "==> 异签：按用户授权卸载后重装"
    "$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
    "$ADB_BIN" -s "$SERIAL" uninstall "$PKG" 2>&1 | tr -d '\r' || true
    INSTALL_OUT="$(install_via_adb || true)"
    printf '%s\n' "$INSTALL_OUT" | tail -5
  fi
fi

if ! printf '%s\n' "$INSTALL_OUT" | grep -q "Success"; then
  echo "  adb install 未成功，走 push + pm install 兜底"
  INSTALL_OUT="$(install_via_push || true)"
  printf '%s\n' "$INSTALL_OUT" | tail -5
fi
printf '%s\n' "$INSTALL_OUT" | grep -q "Success" \
  || { echo "ERROR: 安装失败（见上）" >&2; exit 1; }
echo "  install ok"

"$ADB_BIN" -s "$SERIAL" shell "rm -f $REMOTE" 2>/dev/null || true

echo "==> verify installed apk"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" \
  "$SERIAL" "$PKG" "$SIGNED" --su \
  || { echo "ERROR: installed package does not match local apk" >&2; exit 1; }

SEED_SH="$NIXIANG/04_scripts/_common/ensure_starry_device_id_seed.sh"
[[ -f "$SEED_SH" ]] && { bash "$SEED_SH" 2>/dev/null || true; }

{
  echo "installed_at=$(date '+%Y-%m-%d %H:%M:%S')"
  echo "serial=$SERIAL"
  echo "apk=$SIGNED"
  echo "apk_md5=$(file_md5 "$SIGNED")"
} > "$OK_STAMP"

echo "install complete"
echo "  device=$SERIAL"
echo "  package=$PKG"
echo "  version=$VERSION"
echo "  apk_md5=$(file_md5 "$SIGNED")"
