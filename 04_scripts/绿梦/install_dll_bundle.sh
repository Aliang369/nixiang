#!/usr/bin/env bash
# ============================================================================
#  install_dll_bundle.sh — 绿梦：时空之声 热更 Dll bundle 落地/回滚（root）
#
#  背景：游戏的业务代码在 HybridCLR 热更程序集 Assembly-ACT.dll 里，该 dll 装在
#        YooAsset `Dll` 包的 bundle 内。投递 = 覆盖设备上
#          files/yoo/Dll/BundleFiles/02/<guid>/__data      （重打包后的 bundle）
#          files/yoo/Dll/BundleFiles/02/<guid>/__info      （新大小 + 新 CRC，18 B）
#          files/yoo/Dll/ManifestFiles/CacheRecords.snapshot（可选，等长同步 CRC/大小）
#        YooAsset 启动校验是 Middle 级 = 只比文件大小，而大小取自 __info 本身；
#        加载走 AssetBundle.LoadFromFileAsync(path) 不带 CRC ⇒ 上述两件即可自洽。
#        依据：decompiled/YooAsset/YooAsset/{DCFSInitializeOperation,DefaultCacheFileSystem,
#              FileVerifyHelper}.cs（见 10_docs/绿梦/侦察笔记.md §4）
#
#  用法:
#    bash 04_scripts/绿梦/install_dll_bundle.sh --bundle <__data> --info <__info> \
#         [--snapshot <CacheRecords.snapshot>] [--serial <S>] [--dry-run] [--no-backup]
#    bash 04_scripts/绿梦/install_dll_bundle.sh --rollback [--serial <S>] [--list]
#
#  安全闸门:
#    ① 本地先验 bundle 结构（能解出 224 块、node size 一致、Assembly-ACT 仍在
#       解压流偏移 649120 长度 14529536）—— 任一不符直接退出，不碰设备；
#    ② 校验 --info 与 bundle 自洽（UTF8 CRC 字符串 + i64 大小都要对得上）；
#    ③ 落地前把设备上现有三个文件备份到 /data/local/tmp/lvmeng_backup_<ts>/；
#    ④ 落地后回读 md5 复核 == 本地文件。
# ============================================================================
set -u

SELF_DIR="$(cd "$(dirname "$0")" && pwd -P)"
ROOT="${NIXIANG_ROOT:-$(cd "$SELF_DIR/../.." && pwd -P)}"
SERIAL="${ANDROID_SERIAL:-}"
PKG="com.zhuziplay.lvmengnew.op"
GUID="02341ed99641ed1b023420439a78fc9d"
GUID_PREFIX="02"
ACT_OFF=649120
ACT_SIZE=14529536

PY="${PYTHON:-/opt/homebrew/bin/python3.11}"
REPACK="$SELF_DIR/bundle_repack.py"
BUNDLE=""; INFO=""; SNAPSHOT=""; DRY=0; NOBACKUP=0; ROLLBACK=0; LIST=0
BACKUP_ROOT="/data/local/tmp"

while [ $# -gt 0 ]; do
  case "$1" in
    --bundle)   BUNDLE="${2:-}"; shift 2 ;;
    --info)     INFO="${2:-}"; shift 2 ;;
    --snapshot) SNAPSHOT="${2:-}"; shift 2 ;;
    --serial)   SERIAL="${2:-}"; shift 2 ;;
    --dry-run)  DRY=1; shift ;;
    --no-backup) NOBACKUP=1; shift ;;
    --rollback) ROLLBACK=1; shift ;;
    --list)     LIST=1; shift ;;
    -h|--help)  sed -n '2,30p' "$0"; exit 0 ;;
    *) echo "未知参数: $1" >&2; exit 2 ;;
  esac
done

ADB=(adb)
[ -n "$SERIAL" ] && ADB=(adb -s "$SERIAL")
DEV_BASE="/sdcard/Android/data/$PKG/files/yoo/Dll"
DEV_BDIR="$DEV_BASE/BundleFiles/$GUID_PREFIX/$GUID"
DEV_MDIR="$DEV_BASE/ManifestFiles"
APP_UID="$( ("${ADB[@]}" shell "su -c 'stat -c %u /data/user/0/$PKG/files'" 2>/dev/null || true) | tr -d '\r')"
[ -z "$APP_UID" ] && APP_UID="u0_a1061"
APP_GID="ext_data_rw"
echo "[cfg] serial=${SERIAL:-<auto>} pkg=$PKG app_uid=$APP_UID:$APP_GID"

sh_su() { "${ADB[@]}" shell "su -c \"$1\"" 2>&1 | tr -d '\r'; }

# ---------------------------------------------------------------- rollback / list
if [ "$LIST" = 1 ] || [ "$ROLLBACK" = 1 ]; then
  echo "== 设备上现有备份 =="
  sh_su "ls -1d $BACKUP_ROOT/lvmeng_backup_* 2>/dev/null"
  if [ "$ROLLBACK" = 1 ]; then
    LATEST="$(sh_su "ls -1d $BACKUP_ROOT/lvmeng_backup_* 2>/dev/null | sort | tail -1" | tr -d '\r' | head -1)"
    if [ -z "$LATEST" ]; then echo "✗ 没有可用备份，拒绝回滚" >&2; exit 3; fi
    echo "[rollback] 从 $LATEST 恢复"
    if [ "$DRY" = 1 ]; then echo "[dry-run] 将执行: cp $LATEST/{__data,__info} → $DEV_BDIR/"; exit 0; fi
    sh_su "am force-stop $PKG; sleep 1; \
      cp '$LATEST/__data' '$DEV_BDIR/__data' && \
      cp '$LATEST/__info' '$DEV_BDIR/__info' && \
      chown $APP_UID:$APP_GID '$DEV_BDIR/__data' '$DEV_BDIR/__info' && \
      chmod 660 '$DEV_BDIR/__data' '$DEV_BDIR/__info'"
    if sh_su "test -f '$LATEST/CacheRecords.snapshot' && echo YES" | grep -q YES; then
      sh_su "cp '$LATEST/CacheRecords.snapshot' '$DEV_MDIR/CacheRecords.snapshot' && \
        chown $APP_UID:$APP_GID '$DEV_MDIR/CacheRecords.snapshot' && \
        chmod 660 '$DEV_MDIR/CacheRecords.snapshot'"
    fi
    echo "== 回读 =="
    sh_su "md5sum '$DEV_BDIR/__data' '$DEV_BDIR/__info'; ls -l '$DEV_BDIR/'"
    echo "✓ 回滚完成（请重启游戏）"
  fi
  exit 0
fi

# ---------------------------------------------------------------- 入参检查
if [ -z "$BUNDLE" ] || [ -z "$INFO" ]; then
  echo "缺少 --bundle / --info（见 --help）" >&2; exit 2
fi
[ -f "$BUNDLE" ] || { echo "✗ 找不到 $BUNDLE" >&2; exit 2; }
[ -f "$INFO" ]   || { echo "✗ 找不到 $INFO" >&2; exit 2; }
[ -f "$REPACK" ] || { echo "✗ 找不到 $REPACK" >&2; exit 2; }

# ---------------------------------------------------------------- 闸门 ①：bundle 结构
echo "== 闸门 ① bundle 结构自检 =="
INSP="$("$PY" "$REPACK" inspect "$BUNDLE" 2>&1)" || { echo "$INSP"; echo "✗ inspect 失败" >&2; exit 4; }
echo "$INSP" | grep -E "header:|node:|bioff|block   0:" | head -6
if echo "$INSP" | grep -q "解压失败块数 = 0"; then
  echo "✗ 应当恰有 1 块（索引 0）不可解，实际 0 —— 说明拿到的是别的 bundle" >&2; exit 4
fi
echo "$INSP" | grep -q "解压失败块数 = 1" || { echo "✗ 不可解块数 != 1" >&2; exit 4; }
echo "$INSP" | grep -q "29274972" || { echo "✗ node size 不是 29274972 —— 拒绝落地" >&2; exit 4; }

echo "== 闸门 ①b Assembly-ACT 落位 =="
PE="$("$PY" "$REPACK" locate-pe "$BUNDLE" 2>&1 | grep -E "Assembly-ACT")"
echo "   $PE"
echo "$PE" | grep -qE "off= *$ACT_OFF size= *$ACT_SIZE Assembly-ACT" \
  || { echo "✗ Assembly-ACT 不在 off=$ACT_OFF size=$ACT_SIZE —— 拒绝落地" >&2; exit 4; }

# ---------------------------------------------------------------- 闸门 ②：__info 自洽
echo "== 闸门 ② __info 与 bundle 自洽 =="
"$PY" - "$BUNDLE" "$INFO" <<'PYEOF' || exit 5
import struct, sys, zlib
b = open(sys.argv[1], 'rb').read()
info = open(sys.argv[2], 'rb').read()
if len(info) != 18:
    print("✗ __info 应为 18 B，实际 %d" % len(info)); sys.exit(5)
n = struct.unpack('<H', info[:2])[0]
crc = info[2:2+n].decode('ascii', 'replace')
size = struct.unpack('<q', info[2+n:2+n+8])[0]
want_crc = struct.pack('<I', zlib.crc32(b) & 0xFFFFFFFF).hex()
if size != len(b):
    print("✗ __info size=%d != bundle %d" % (size, len(b))); sys.exit(5)
if crc != want_crc:
    print("✗ __info crc=%s != 实算 %s" % (crc, want_crc)); sys.exit(5)
print("   ✓ size=%d crc=%s 自洽" % (size, crc))
PYEOF

if [ -n "$SNAPSHOT" ]; then
  [ -f "$SNAPSHOT" ] || { echo "✗ 找不到 $SNAPSHOT" >&2; exit 2; }
  echo "== 闸门 ②b snapshot 与 __info 一致 =="
  "$PY" - "$BUNDLE" "$SNAPSHOT" <<'PYEOF' || exit 5
import struct, sys, zlib
b = open(sys.argv[1], 'rb').read()
snap = open(sys.argv[2], 'rb').read()
if snap[:4] != b'DCER':
    print("✗ snapshot magic 不是 DCER"); sys.exit(5)
want_crc = struct.pack('<I', zlib.crc32(b) & 0xFFFFFFFF).hex().encode()
if want_crc not in snap:
    print("✗ snapshot 里找不到 %s" % want_crc.decode()); sys.exit(5)
if struct.pack('<q', len(b)) not in snap:
    print("✗ snapshot 里找不到 size=%d" % len(b)); sys.exit(5)
print("   ✓ snapshot 含 crc=%s size=%d" % (want_crc.decode(), len(b)))
PYEOF
fi

if [ "$DRY" = 1 ]; then
  echo "[dry-run] 闸门全过，未触碰设备。将执行："
  echo "   am force-stop $PKG → 备份现有 → 覆盖 $DEV_BDIR/__data 与 __info$([ -n "$SNAPSHOT" ] && echo ' 与 CacheRecords.snapshot') → chown/chmod → 回读 md5"
  exit 0
fi

# ---------------------------------------------------------------- 闸门 ③：备份
STAMP="$(date +%Y%m%d_%H%M%S)"
BK="$BACKUP_ROOT/lvmeng_backup_$STAMP"
# 暂存目录必须 shell 可写（先建 root 目录再改属主，否则 adb push 静默失败）
STAGE="$BACKUP_ROOT/lvmeng_stage_$STAMP"
sh_su "mkdir -p '$STAGE' '$BK'"
"${ADB[@]}" shell "su -c 'chown shell:shell $STAGE && chmod 775 $STAGE'" >/dev/null 2>&1
"${ADB[@]}" push "$BUNDLE" "$STAGE/__data" 2>&1 | tail -1
"${ADB[@]}" push "$INFO"   "$STAGE/__info" 2>&1 | tail -1
if [ -n "$SNAPSHOT" ]; then
  "${ADB[@]}" push "$SNAPSHOT" "$STAGE/CacheRecords.snapshot" 2>&1 | tail -1
fi
echo "== 落地前复核暂存 md5 =="
L_B="$(md5 -q "$BUNDLE" 2>/dev/null || md5sum "$BUNDLE" | cut -d' ' -f1)"
L_I="$(md5 -q "$INFO" 2>/dev/null || md5sum "$INFO" | cut -d' ' -f1)"
sh_su "md5sum '$STAGE/__data' '$STAGE/__info'"
sh_su "echo want_data=$L_B; echo want_info=$L_I"

if [ "$NOBACKUP" = 0 ]; then
  echo "== 闸门 ③ 备份现有设备文件 → $BK =="
  sh_su "cp '$DEV_BDIR/__data' '$BK/__data' && cp '$DEV_BDIR/__info' '$BK/__info' && \
    cp '$DEV_MDIR/CacheRecords.snapshot' '$BK/CacheRecords.snapshot' && ls -l '$BK'"
fi

# ---------------------------------------------------------------- 应用
echo "== 应用（force-stop 游戏 → 覆盖 → chown/chmod）=="
sh_su "am force-stop $PKG; sleep 1; \
  cp '$STAGE/__data' '$DEV_BDIR/__data' && \
  cp '$STAGE/__info' '$DEV_BDIR/__info' && \
  chown $APP_UID:$APP_GID '$DEV_BDIR/__data' '$DEV_BDIR/__info' && \
  chmod 660 '$DEV_BDIR/__data' '$DEV_BDIR/__info' && echo APPLIED"
if [ -n "$SNAPSHOT" ]; then
  sh_su "cp '$STAGE/CacheRecords.snapshot' '$DEV_MDIR/CacheRecords.snapshot' && \
    chown $APP_UID:$APP_GID '$DEV_MDIR/CacheRecords.snapshot' && \
    chmod 660 '$DEV_MDIR/CacheRecords.snapshot' && echo SNAPSHOT_APPLIED"
fi

echo "== 闸门 ④ 回读复核 =="
sh_su "ls -lZ '$DEV_BDIR/'"
DEV_MD5="$(sh_su "md5sum '$DEV_BDIR/__data'" | awk '{print $1}')"
echo "   device __data md5 = $DEV_MD5"
echo "   local  __data md5 = $L_B"
if [ "$DEV_MD5" = "$L_B" ]; then echo "✓ 落地成功（回滚: --rollback）"; else echo "✗ md5 不一致！" >&2; exit 6; fi
