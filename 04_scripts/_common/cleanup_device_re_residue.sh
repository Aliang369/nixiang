#!/usr/bin/env bash
# 真机逆向残留一键清理（每次 Frida/补丁/注入/试装结束后必跑）
#
# 背景：雷霆战机 ACE 实验中，/data/local/tmp 的 frida/ptrace、KSU 全局 umount、
# 应用目录内改过的 libtersafe* 等残留会导致「腾讯游戏全家异常 / 误判 ACE 自毁」。
# 详见：10_docs/雷霆战机/官方原包_ACE崩溃与过检候选_20260810.md §13
#
# Usage:
#   ./04_scripts/_common/cleanup_device_re_residue.sh           # 标准清理
#   ./04_scripts/_common/cleanup_device_re_residue.sh --dry-run # 只列不删
#   ./04_scripts/_common/cleanup_device_re_residue.sh --deep    # 额外清 tmp 大 dump
#   PKG=com.xxx.app ./... --restore-so                         # 可选：从备份恢复该包 so
#
# 不做：卸载 SukiSU/Root、删用户游戏数据、动非本仓备份以外的个人文件

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
# shellcheck source=/dev/null
source "$ROOT/04_scripts/_common/env.sh" 2>/dev/null || true

DRY=0
DEEP=0
RESTORE_SO=0
BACKUP_DIR="/sdcard/MT2/backup/re_cleanup_$(date +%Y%m%d)"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run) DRY=1; shift ;;
    --deep) DEEP=1; shift ;;
    --restore-so) RESTORE_SO=1; shift ;;
    -h|--help)
      sed -n '2,20p' "$0" | sed 's/^# \?//'
      exit 0
      ;;
    *) echo "unknown: $1" >&2; exit 2 ;;
  esac
done

die() { echo "[!] $*" >&2; exit 1; }
log() { echo "[*] $*"; }

adb get-state >/dev/null 2>&1 || die "无 adb 设备。请 USB 连接并授权后再跑。"

SERIAL="$(adb get-serialno | tr -d '\r')"
MODEL="$(adb shell getprop ro.product.model | tr -d '\r')"
log "device=$MODEL serial=$SERIAL dry=$DRY deep=$DEEP"

run_su() {
  adb shell "su -c $(printf '%q' "$*")" 2>/dev/null || adb shell "su -c '$*'"
}

# ---------- 1) 停注入进程 ----------
log "kill injectors / helpers"
# 2026-09-10 起 server 换 phantom-frida：推机名 fs-arm64 / 原名 ipcmodule-server；线程名已随机化扫不到，靠进程名
INJECT_PAT='frida-server|frida-helper|gum-js-loop|ltzj_ptrace|ltzj_patch|ltzj_sigbus|re\.frida|fs-arm64|ipcmodule'
if [[ "$DRY" == "1" ]]; then
  adb shell "ps -A 2>/dev/null | grep -iE '$INJECT_PAT' || true" | tr -d '\r' || true
else
  adb shell "su -c 'killall frida-server frida-helper fs-arm64 ipcmodule-server ltzj_ptrace_bus2 ltzj_ptrace_bus ltzj_ptrace_cont 2>/dev/null; pkill -f frida-server 2>/dev/null; pkill -f fs-arm64 2>/dev/null; pkill -f ipcmodule 2>/dev/null; pkill -f ltzj_ptrace 2>/dev/null; true'" || true
fi

# ---------- 2) KSU 全局 umount（试验常加的路径） ----------
log "KSU umount list (remove RE trial paths if present)"
if adb shell "su -c 'command -v ksud'" 2>/dev/null | grep -q ksud; then
  adb shell "su -c 'ksud umount list 2>/dev/null || true'" | tr -d '\r' || true
  if [[ "$DRY" == "0" ]]; then
    # 雷霆实验加过的路径；重复 remove 无害
    for p in /data/adb /debug_ramdisk /sbin /.magisk; do
      adb shell "su -c 'ksud umount remove $p 2>/dev/null || ksud umount del $p 2>/dev/null || true'" || true
    done
    adb shell "su -c 'ksud umount list 2>/dev/null || true'" | tr -d '\r' || true
  fi
else
  log "ksud not found — skip umount cleanup"
fi

# ---------- 3) /data/local/tmp 逆向垃圾 ----------
# 【硬禁】绝不删除卡密设备指纹种子（与逆向残留无关）：
#   /data/local/tmp/starry_magic/
#   /sdcard/Android/starry_magic/  /sdcard/.starry_magic/  /sdcard/Download/.starry_magic/
# 删了会导致「卡密已绑定其他设备」。只清 frida/ptrace/试装 so/临时 apk 等。

log "scan /data/local/tmp RE residue"
adb shell "su -c 'ls -la /data/local/tmp 2>/dev/null | head -60'" | tr -d '\r' || true

# 明确模式：注入器、本仓试验 so/工具、星光临时 apk（保留用户其它文件）
TMP_GLOBS=(
  'frida-server*'
  'frida-helper*'
  're.frida*'
  'fs-arm64*'
  'ipcmodule*'
  'ltzj_*'
  'libltzj_*'
  'libtersafe2_ace*.so'
  'libtersafe2_root*.so'
  'libtersafe2_*patch*.so'
  'libtprt_*patch*.so'
  'libtprt_exit*.so'
  '*sigbus*'
  '*ptrace_bus*'
  'apply_tersafe*.sh'
  'starry_*.apk'
  'hscan*'
  '*_mempatch*'
)

if [[ "$DRY" == "1" ]]; then
  for g in "${TMP_GLOBS[@]}"; do
    adb shell "su -c 'ls -la /data/local/tmp/$g 2>/dev/null'" | tr -d '\r' || true
  done
else
  adb shell "su -c 'mkdir -p $BACKUP_DIR'" || true
  # 官方 so 备份若存在则挪到 MT2 backup，避免误删无可恢复
  adb shell "su -c 'for f in /data/local/tmp/libtersafe2_official_orig.so /data/local/tmp/libtprt_official_orig.so; do [ -f \"\$f\" ] && mv -f \"\$f\" $BACKUP_DIR/; done; true'" || true
  for g in "${TMP_GLOBS[@]}"; do
    adb shell "su -c 'rm -f /data/local/tmp/$g 2>/dev/null; true'" || true
  done
  # 常见临时试装包名
  adb shell "su -c 'rm -f /data/local/tmp/ltzj_*.apk /data/local/tmp/*_starry*.apk /data/local/tmp/*kill*.apk 2>/dev/null; true'" || true
  log "moved official so backups (if any) -> $BACKUP_DIR"
fi

if [[ "$DEEP" == "1" && "$DRY" == "0" ]]; then
  log "deep: remove large dump leftovers under tmp (wx/jsb/etc. older scans)"
  adb shell "su -c 'rm -rf /data/local/tmp/wx_* /data/local/tmp/jsb_* /data/local/tmp/dump_* /data/local/tmp/*_dump* 2>/dev/null; true'" || true
fi

# ---------- 4) 可选：恢复当前前台/指定包的官方 so（需备份） ----------
if [[ "$RESTORE_SO" == "1" ]]; then
  PKG="${PKG:-}"
  if [[ -z "$PKG" ]]; then
    log "--restore-so needs PKG=com.xxx"
  else
    APK_PATH="$(adb shell pm path "$PKG" | tr -d '\r' | head -1 | sed 's/^package://')"
    LIBDIR="${APK_PATH%/base.apk}/lib/arm64"
    log "restore so for $PKG libdir=$LIBDIR (from $BACKUP_DIR if present)"
    if [[ "$DRY" == "0" ]]; then
      adb shell "su -c 'if [ -f $BACKUP_DIR/libtersafe2_official_orig.so ]; then cp -f $BACKUP_DIR/libtersafe2_official_orig.so $LIBDIR/libtersafe2.so; chmod 755 $LIBDIR/libtersafe2.so; chown system:system $LIBDIR/libtersafe2.so; fi'"
      adb shell "su -c 'if [ -f $BACKUP_DIR/libtprt_official_orig.so ]; then cp -f $BACKUP_DIR/libtprt_official_orig.so $LIBDIR/libtprt.so; chmod 755 $LIBDIR/libtprt.so; chown system:system $LIBDIR/libtprt.so; fi'"
      adb shell "su -c 'md5sum $LIBDIR/libtersafe2.so $LIBDIR/libtprt.so 2>/dev/null'" | tr -d '\r' || true
    fi
  fi
fi

# ---------- 5) 自检 ----------
log "post-check"
echo "--- processes ---"
adb shell "ps -A 2>/dev/null | grep -iE '$INJECT_PAT' || echo '(none)'" | tr -d '\r'
echo "--- tmp frida/ltzj ---"
adb shell "su -c 'ls /data/local/tmp/frida-server /data/local/tmp/fs-arm64* /data/local/tmp/ltzj_* /data/local/tmp/libltzj_* 2>/dev/null || echo clean'" | tr -d '\r'
echo "--- ksud umount ---"
adb shell "su -c 'ksud umount list 2>/dev/null || echo no-ksud'" | tr -d '\r'
echo "--- frida port ---"
# 27042=官方/Florida 时代；39571=phantom run39.1 本包端口（随机构建，换包以 build-info.json 为准）
adb shell "su -c 'ss -ltnp 2>/dev/null | grep -E \"27042|39571\" || netstat -ltnp 2>/dev/null | grep -E \"27042|39571\" || echo no-frida-port'" | tr -d '\r'
echo "--- starry device_id seed (MUST keep) ---"
SEED_NOW="$(adb shell "su -c 'cat /data/local/tmp/starry_magic/device_id 2>/dev/null'" | tr -d '\r' || true)"
if [[ "$SEED_NOW" == dev_* ]]; then
  echo "ok $SEED_NOW"
else
  echo "MISSING — restoring via ensure_starry_device_id_seed.sh"
  if [[ "$DRY" == "0" ]]; then
    bash "$ROOT/04_scripts/_common/ensure_starry_device_id_seed.sh" || log "WARN: seed restore failed"
  fi
fi

log "done. 勿卸载 SukiSU；勿删 starry_magic 种子。若腾讯游戏仍异常：重启手机后再试；仍挂则可能是云端风控/网络。"
if [[ "$DRY" == "0" ]]; then
  log "backup dir on device: $BACKUP_DIR"
fi
