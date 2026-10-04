#!/usr/bin/env bash
# 勇士之心 / 星光包 —— 登录态与账号数据 备份 / 还原 / 体检（跨设备通用版）
#
# 背景（问题③）：
#   云机 / 模拟器上"登录成功后，第二次打开又要重新登录"。
#   根因不是游戏不保存，而是【重装导致数据目录被清空】：
#   官方包与星光包签名不同（官方 CN=shan，星光 CN=StarryMagic），
#   `adb install -r` 会直接 INSTALL_FAILED_UPDATE_INCOMPATIBLE，
#   于是只能先 uninstall 再 install —— /data/data/<pkg> 连同
#   shared_prefs（登录票据）、databases/jsb.sqlite（本地存档）、
#   files/res（831MB 热更资源）一起被删除。
#
#   修法（首选，零成本）：固定签名出包 + `adb install -r` 原地覆盖，
#   数据目录原样保留，登录态与存档都不会丢。
#   本脚本是本修法之外的【兜底】：
#     备份 -> 换签名/换包重装 -> 还原，用于已经丢过一次数据的场景。
#
# 与老脚本的关系：
#   04_scripts/勇士之心/ysxz_backup_data.sh / ysxz_restore_data.sh 是
#   真机版（依赖 `su` + .device_serial）。本脚本是通用版：同时支持
#   `su`（真机 root）与 `adb root`（MuMu 等模拟器，无 su 二进制）。
#
# 用法：
#   bash ysxz_login_state.sh inspect                       # 只读体检（推荐先跑）
#   bash ysxz_login_state.sh backup  TAG=20261004          # 备份到 09_test/勇士之心/backup_<TAG>/
#   ALLOW_RESTORE=1 bash ysxz_login_state.sh restore TAG=20261004   # 还原（危险操作，需显式开关）
#
# 环境变量：
#   SERIAL   目标设备序列号，默认 127.0.0.1:16384（MuMu 必须用 IP:端口，emulator-5554 会卡死）
#   PKG      包名，默认 com.fightingstars.games.warrior
#   TAG      备份标签，默认当天日期
#   ROOT     强制指定 root 方式：su | adbroot | auto（默认 auto）
set -euo pipefail

PKG="${PKG:-com.fightingstars.games.warrior}"
SERIAL="${SERIAL:-127.0.0.1:16384}"
ADB_BIN="${ADB_BIN:-adb}"
ROOT="${ROOT:-auto}"
MODE="${1:-inspect}"
TAG="${TAG:-$(date +%Y%m%d)}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
OUT="$NIXIANG/09_test/勇士之心/backup_$TAG"
REMOTE_DIR="/data/data/$PKG"
REMOTE_TAR="/data/local/tmp/ysxz_state_$TAG.tar"

A=("$ADB_BIN" -s "$SERIAL")

die() { echo "错误: $*" >&2; exit 1; }
say() { echo "[ysxz_login_state] $*"; }

# ---------------------------------------------------------------------------
# root 方式自动判定：MuMu/模拟器有 `adb root`，真机有 `su`
# ---------------------------------------------------------------------------
detect_root() {
  if [[ "$ROOT" != "auto" ]]; then echo "$ROOT"; return; fi
  if "${A[@]}" shell 'su -c id' 2>/dev/null | grep -q 'uid=0'; then echo su; return; fi
  if "${A[@]}" shell id 2>/dev/null | grep -q 'uid=0'; then echo adbroot; return; fi
  if "${A[@]}" root >/dev/null 2>&1; then
    sleep 2
    if "${A[@]}" shell id 2>/dev/null | grep -q 'uid=0'; then echo adbroot; return; fi
  fi
  echo none
}

ROOTMODE=""
# 以 root 执行一段远端 shell：RSH 'cmd'
RSH() {
  case "$ROOTMODE" in
    su)      "${A[@]}" shell "su -c $(python3 -c 'import sys,json;print(json.dumps(sys.argv[1]))' "$1")" ;;
    adbroot) "${A[@]}" shell "$1" ;;
    *)       die "设备无 root（su 与 adb root 都不可用），$MODE 无法进行" ;;
  esac
}

preflight() {
  command -v "$ADB_BIN" >/dev/null 2>&1 || die "找不到 adb（可用 ADB_BIN=... 指定）"
  "${A[@]}" get-state >/dev/null 2>&1 || die "设备 $SERIAL 未连接（MuMu 请用 IP:端口，emulator-5554 传输层会卡死）"
  ROOTMODE="$(detect_root)"
  say "device=$SERIAL package=$PKG root=$ROOTMODE mode=$MODE"
  "${A[@]}" shell "ls -d $REMOTE_DIR" >/dev/null 2>&1 \
    || die "$REMOTE_DIR 不存在：游戏未安装，或包名不对"
}

# ---------------------------------------------------------------------------
# inspect —— 只读体检：登录票据在不在、存档在不在、热更资源大不大
# ---------------------------------------------------------------------------
cmd_inspect() {
  local tmp="$OUT"; mkdir -p "$tmp"
  say "[1/4] 包信息"
  "${A[@]}" shell "dumpsys package $PKG | grep -E 'versionName|versionCode|lastUpdateTime|codePath'" \
    | tr -d '\r' | tee "$tmp/inspect_package.txt"
  say "[2/4] 关键登录态文件（shared_prefs）"
  RSH "ls -l $REMOTE_DIR/shared_prefs 2>/dev/null" | tr -d '\r' | tee "$tmp/inspect_prefs.txt"
  say "[3/4] 登录票据关键字段（只打印是否命中，不打印明文 token）"
  RSH "for f in ymeng_sdk_pref.xml joyo_login_info_preference.xml game_device_id.xml; do
         p=$REMOTE_DIR/shared_prefs/\$f
         if [ -f \$p ]; then
           echo \"HIT \$f  bytes=\$(wc -c < \$p)\";
           grep -o -E 'can_auto_login|session_code|user_name|user_id|roleName|roleLevel|serverName|user_token|gKey' \$p | sort -u | tr '\n' ' '; echo;
         else echo \"MISS \$f\"; fi
       done" | tr -d '\r' | tee "$tmp/inspect_login_keys.txt"
  say "[4/4] 数据目录体积与存档"
  RSH "du -sh $REMOTE_DIR 2>/dev/null | cut -f1" | tr -d '\r' | sed 's/^/data_dir_size=/'
  RSH "ls -l $REMOTE_DIR/databases 2>/dev/null" | tr -d '\r'
  RSH "ls $REMOTE_DIR/files/res/version 2>/dev/null && cat $REMOTE_DIR/files/res/version" | tr -d '\r'
  say "体检完成，明细见 $tmp/inspect_*.txt"
  say "判定：若 inspect_login_keys.txt 里 3 个文件都是 HIT 且能看到 can_auto_login/session_code，"
  say "      说明【登录态本身是保存的】——再次要求登录只可能是重装把数据目录清了（见文件头说明）。"
}

# ---------------------------------------------------------------------------
# backup —— 停进程 -> 设备端 tar -> exec-out 拉回 -> 清单
# ---------------------------------------------------------------------------
cmd_backup() {
  mkdir -p "$OUT"
  say "[1/4] 记录安装指纹"
  "${A[@]}" shell "dumpsys package $PKG | grep -E 'versionName|versionCode|codePath|lastUpdateTime'" \
    | tr -d '\r' > "$OUT/device_package.txt"
  say "[2/4] 停进程并在设备端打包（排除 cache/code_cache/app_webview）"
  "${A[@]}" shell "am force-stop $PKG"
  RSH "cd $REMOTE_DIR && tar -cf $REMOTE_TAR --exclude=cache --exclude=code_cache --exclude=app_webview . && ls -l $REMOTE_TAR" \
    | tr -d '\r' > "$OUT/remote_tar.txt"
  say "[3/4] 拉回本地"
  "${A[@]}" exec-out "su -c 'cat $REMOTE_TAR' 2>/dev/null" > "$OUT/data_$TAG.tar" 2>/dev/null || true
  if [[ ! -s "$OUT/data_$TAG.tar" ]]; then
    # 无 su 的模拟器：adb root 下直接 cat 即可
    "${A[@]}" exec-out "cat $REMOTE_TAR" > "$OUT/data_$TAG.tar"
  fi
  [[ -s "$OUT/data_$TAG.tar" ]] || die "拉回的 tar 为空，备份失败"
  local size md5
  size=$(wc -c < "$OUT/data_$TAG.tar" | tr -d ' ')
  md5=$(md5sum "$OUT/data_$TAG.tar" | awk '{print $1}')
  say "      local tar bytes=$size md5=$md5"
  say "[4/4] 生成清单"
  tar -tf "$OUT/data_$TAG.tar" > "$OUT/tar_listing.txt"
  printf '%s\n' "$md5" > "$OUT/data_$TAG.tar.md5"
  python3 - "$OUT" "$PKG" "$SERIAL" "$TAG" "$size" "$md5" <<'PY'
import json, sys, pathlib
out, pkg, serial, tag, size, md5 = sys.argv[1:7]
d = pathlib.Path(out)
lines = (d / "tar_listing.txt").read_text(errors="replace").splitlines()
keys = [l for l in lines if ("shared_prefs" in l or "jsb.sqlite" in l or "bingo_guest_id" in l)]
json.dump({
    "package": pkg, "serial": serial, "tag": tag,
    "remote_tar": f"/data/local/tmp/ysxz_state_{tag}.tar",
    "local_tar": f"data_{tag}.tar",
    "size": int(size), "md5": md5, "entries": len(lines),
    "key_entries": keys[:40],
}, open(d / "backup_manifest.json", "w"), ensure_ascii=False, indent=2)
PY
  say "backup_ok=$OUT/data_$TAG.tar"
  say "设备端副本保留在 ${REMOTE_TAR}（还原时免 push）"
}

# ---------------------------------------------------------------------------
# restore —— 必须显式 ALLOW_RESTORE=1
# ---------------------------------------------------------------------------
cmd_restore() {
  [[ "${ALLOW_RESTORE:-0}" == "1" ]] || die "未设置 ALLOW_RESTORE=1（还原会覆盖现有数据，必须显式确认）"
  local tar="$OUT/data_$TAG.tar"
  [[ -s "$tar" ]] || die "找不到备份 ${tar}（先跑 backup）"
  mkdir -p "$OUT"
  say "[1/5] 包路径"
  "${A[@]}" shell "pm path $PKG" | tr -d '\r' | tee "$OUT/restore_pm_path.txt"
  say "[2/5] 停进程"
  "${A[@]}" shell "am force-stop $PKG"
  say "[3/5] push 备份到设备"
  "${A[@]}" push "$tar" "$REMOTE_TAR" | tail -1
  say "[4/5] 解包 + 修正属主 + 修正 SELinux 标签"
  RSH "UG=\$(stat -c %u:%g $REMOTE_DIR); cd $REMOTE_DIR && tar -xf $REMOTE_TAR; \
       chown -R \$UG $REMOTE_DIR/files $REMOTE_DIR/shared_prefs $REMOTE_DIR/databases 2>/dev/null || true; \
       restorecon -R $REMOTE_DIR 2>/dev/null || true; \
       echo uid_gid=\$UG; du -sh $REMOTE_DIR; echo shared_prefs_count=\$(ls $REMOTE_DIR/shared_prefs | wc -l)" \
    | tr -d '\r' | tee "$OUT/restore_result.txt"
  say "[5/5] restore_ok=true"
  say "请冷启动游戏验证：① 是否需要重新登录；② 热更资源是否还在"
}

case "$MODE" in
  inspect) preflight; cmd_inspect ;;
  backup)  preflight; cmd_backup ;;
  restore) preflight; cmd_restore ;;
  *) die "未知模式：${MODE}（可用：inspect | backup | restore）" ;;
esac
