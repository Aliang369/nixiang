#!/usr/bin/env bash
# ============================================================================
#  shlt_push_hotfix_cache.sh — 山河旅探 1.4.38：把补丁后的热更 dll bundle 注入
#  设备侧 YooAsset 缓存（HotfixDll 包），让游戏启动时直接加载我们的 DLL。
#
#  设备目标（Android 11+ 的 /sdcard 走 FUSE，shell 写不进 app 私有目录，
#  故一律先 push 到 /data/local/tmp 再 su cp）：
#    /sdcard/Android/data/com.itaotuo.shanhelvtan/files/yoo/HotfixDll/
#      CacheFiles/<md5 前 2 位>/<md5>/__data    新 bundle（目录名 = bundle md5）
#      CacheFiles/<md5 前 2 位>/<md5>/__info    18 B = u16LE(8) + CRC32(LE 十六进制 8 字) + i64LE(字节数)
#      ManifestFiles/PackageManifest_HotfixDll_2025-02-14-1015.bytes   新清单
#      ManifestFiles/PackageManifest_HotfixDll_2025-02-14-1015.hash    md5(新清单) 的 ASCII
#      ManifestFiles/PackageManifest_HotfixDll.version                 **不动**
#
#  依据（YooAsset 2.0.x 源码 CacheSystem / ResourcePackage，详见
#  10_docs/山河旅探/热更缓存注入_1.4.38.md §3）：
#    · CacheHelper.VerifyingCacheFile：任何等级下 __info 缺失 => InfoFileNotExisted；
#      校验失败 => VerifyCacheFilesOperation 回调 DeleteFiles() **整目录删除**（然后重下）；
#    · CacheBootVerifyLevel 默认 Middle：size 取自 __info；High 额外比 CRC；
#    · LoadCacheManifestOperation：<pkg>.version 定位 .bytes，.hash 内容比 md5(.bytes)，
#      不符则删 .bytes/.hash 回落内置清单 => .hash 必须同步更新。
#
#  安全闸门：
#    ① 本地先验（不碰设备）：bundle 头 = UnityFS；manifest 里该 bundle 的
#       md5 / CRC 串 / 字节数 == 实算值；.hash == md5(.bytes)；__info 自洽；
#    ② 默认 dry-run 只打印命令，--apply 才真跑；
#    ③ 默认备份设备上将被覆盖的文件到 /data/local/tmp/shlt_hotfix_backup_<ts>/；
#    ④ 落地后回读 md5 复核；.version 只读打印、不改。
#
#  用法：
#    bash mod菜单/starry-mod-menu/scripts/shlt_push_hotfix_cache.sh                # dry-run
#    bash mod菜单/starry-mod-menu/scripts/shlt_push_hotfix_cache.sh --apply
#    bash mod菜单/starry-mod-menu/scripts/shlt_push_hotfix_cache.sh --apply --serial emulator-5554
#    bash .../shlt_push_hotfix_cache.sh --restore-latest --apply                   # 回滚清单
#
#  约束：不 pm install / 不卸载 / 不删设备缓存目录；--info 缺省时按 bundle 现算。
# ============================================================================
set -u

SELF_DIR="$(cd "$(dirname "$0")" && pwd -P)"
NIXIANG="${NIXIANG_ROOT:-$(cd "$SELF_DIR/../../.." && pwd -P)}"
PKG="com.itaotuo.shanhelvtan"
VER="2025-02-14-1015"
MAN_BASE="PackageManifest_HotfixDll_${VER}"
BUNDLE_NAME="hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle"
PATCH_DIR="$NIXIANG/07_patched/山河旅探/hotfix_${VER}"

SERIAL="${ANDROID_SERIAL:-emulator-5554}"
BUNDLE="$PATCH_DIR/$BUNDLE_NAME"
MANIFEST="$PATCH_DIR/$MAN_BASE.bytes"
INFO="$PATCH_DIR/__info"
APPLY=0
BACKUP=1
FORCE_STOP=0
RESTORE=0

while [ $# -gt 0 ]; do
  case "$1" in
    --bundle)         BUNDLE="${2:-}"; shift 2 ;;
    --manifest)       MANIFEST="${2:-}"; shift 2 ;;
    --info)           INFO="${2:-}"; shift 2 ;;
    --serial)         SERIAL="${2:-}"; shift 2 ;;
    --apply)          APPLY=1; shift ;;
    --dry-run)        APPLY=0; shift ;;
    --no-backup)      BACKUP=0; shift ;;
    --force-stop)     FORCE_STOP=1; shift ;;
    --restore-latest) RESTORE=1; shift ;;
    -h|--help)        sed -n '2,40p' "$0"; exit 0 ;;
    *) echo "未知参数: $1（见 --help）" >&2; exit 2 ;;
  esac
done

ADB=(adb -s "$SERIAL")
DEV_BASE="/sdcard/Android/data/$PKG/files/yoo/HotfixDll"
DEV_MDIR="$DEV_BASE/ManifestFiles"
BK_ROOT="/data/local/tmp"
TS="$(date +%Y%m%d_%H%M%S)"
BK="$BK_ROOT/shlt_hotfix_backup_$TS"
STAGE="$BK_ROOT/shlt_hotfix_stage_$TS"
WORK="$(mktemp -d "${TMPDIR:-/tmp}/shlt_push.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT

md5of() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | cut -d' ' -f1; else md5 -q "$1"; fi; }
say()  { echo "[shlt] $*"; }
warn() { echo "[shlt][WARN] $*" >&2; }
die()  { echo "[shlt][FAIL] $*" >&2; exit 3; }
sh_su() { adb -s "$SERIAL" shell "su -c \"$1\"" 2>&1 | tr -d '\r'; }

PY="${PYTHON:-$(command -v python3.11 || command -v python3 || command -v python || true)}"
[ -n "$PY" ] || die "找不到 python（本地自检用）；可设 PYTHON=..."

adbpush() { adb -s "$SERIAL" push "$1" "$2"; }

# ---------------------------------------------------------------- 回滚分支
if [ "$RESTORE" = 1 ]; then
  LATEST="$(sh_su "ls -1d $BK_ROOT/shlt_hotfix_backup_* 2>/dev/null | sort | tail -1")"
  [ -n "$LATEST" ] || die "没有可用备份（$BK_ROOT/shlt_hotfix_backup_*）"
  say "回滚来源 = $LATEST"
  if [ "$APPLY" = 0 ]; then
    say "[dry-run] 将把 $LATEST 下已有的 $MAN_BASE.bytes/.hash 复制回 $DEV_MDIR 并 chown/chmod"
    exit 0
  fi
  UID_GID="$(sh_su "stat -c %u:%g /data/user/0/$PKG/files" || true)"
  sh_su "ls -l '$LATEST'"
  sh_su "for f in $MAN_BASE.bytes $MAN_BASE.hash; do \
      [ -f \"$LATEST/\$f\" ] && cp -f \"$LATEST/\$f\" \"$DEV_MDIR/\$f\"; done; \
    [ -n '$UID_GID' ] && chown $UID_GID \"$DEV_MDIR/$MAN_BASE.bytes\" \"$DEV_MDIR/$MAN_BASE.hash\"; \
    chmod 664 \"$DEV_MDIR/$MAN_BASE.bytes\" \"$DEV_MDIR/$MAN_BASE.hash\" 2>/dev/null; \
    md5sum \"$DEV_MDIR/$MAN_BASE.bytes\" \"$DEV_MDIR/$MAN_BASE.hash\"; \
    echo -n 'version='; cat '$DEV_MDIR/PackageManifest_HotfixDll.version'"
  say "回滚完成（新 md5 缓存目录保留但不再被清单引用）。"
  exit 0
fi

# ---------------------------------------------------------------- 入参
[ -f "$BUNDLE" ]   || die "找不到 bundle: $BUNDLE"
[ -f "$MANIFEST" ] || die "找不到 manifest: $MANIFEST"
BSIZE="$(wc -c < "$BUNDLE" | tr -d ' ')"
[ "$BSIZE" -gt 100000 ] || die "bundle 才 $BSIZE B，不像热更 dll bundle"
head -c 7 "$BUNDLE" | grep -q "UnityFS" || die "bundle 头不是 UnityFS"
if [ ! -f "$INFO" ]; then INFO="$WORK/__info"; warn "未提供 --info，按 bundle 现算到 $INFO"; fi

BUNDLE_MD5="$(md5of "$BUNDLE")"
DEV_CDIR="$DEV_BASE/CacheFiles/${BUNDLE_MD5:0:2}/$BUNDLE_MD5"
MAN_MD5="$(md5of "$MANIFEST")"

say "serial      = $SERIAL"
say "bundle      = ${BUNDLE}（$BSIZE B, md5=${BUNDLE_MD5}）"
say "manifest    = ${MANIFEST}（md5=${MAN_MD5}）"
say "info        = $INFO"
say "device cache= $DEV_CDIR"

# ---------------------------------------------------------------- 闸门 ①：本地自检
say "== 闸门 ① 本地自检（bundle / manifest / __info / .hash 互相自洽）=="
"$PY" - "$BUNDLE" "$MANIFEST" "$INFO" "$BUNDLE_NAME" <<'PYEOF' || die "本地自检不通过，未触碰设备"
import hashlib, struct, sys, zlib
from pathlib import Path

bundle_p, man_p, info_p, bname = sys.argv[1:5]
b = Path(bundle_p).read_bytes()
man = Path(man_p).read_bytes()

def entries(buf, needle):
    out, i = [], 0
    while True:
        i = buf.find(needle, i)
        if i < 0:
            return out
        end = i + len(needle)
        crc_u, n = struct.unpack_from("<IH", buf, end)
        if n != 32:
            i = end
            continue
        md5s = buf[end + 6:end + 38].decode("ascii", "replace")
        m = struct.unpack_from("<H", buf, end + 38)[0]
        crcs = buf[end + 40:end + 40 + m].decode("ascii", "replace")
        size = struct.unpack_from("<q", buf, end + 40 + m)[0]
        out.append(dict(unitycrc=crc_u, md5=md5s, crc=crcs, size=size))
        i = end

es = entries(man, bname.encode())
if not es:
    print("XX manifest 里找不到 %s 的条目" % bname); sys.exit(1)
if len(es) > 1:
    print("XX manifest 里 %s 出现 %d 次（预期 1）" % (bname, len(es))); sys.exit(1)
e = es[0]
want_md5 = hashlib.md5(b).hexdigest()
want_crc = struct.pack("<I", zlib.crc32(b) & 0xFFFFFFFF).hex()
want_size = len(b)
ok = True
for k, got, want in (("md5", e["md5"], want_md5), ("crc", e["crc"], want_crc), ("size", e["size"], want_size)):
    if got != want:
        ok = False
    print("  %s manifest.%-5s = %-34s 实算 = %s" % ("OK" if got == want else "XX", k, got, want))
print("  manifest.unitycrc = 0x%08X（改包后置 0，LoadFromFile 跳过 Unity CRC）" % e["unitycrc"])
if not ok:
    print("XX manifest 与 bundle 不一致（清单不是这份 bundle 的）"); sys.exit(1)

hp = Path(man_p).with_suffix(".hash")
if hp.exists():
    h = hp.read_text(encoding="ascii").strip()
    if h != hashlib.md5(man).hexdigest():
        print("XX %s 内容 %s != md5(manifest) %s" % (hp.name, h, hashlib.md5(man).hexdigest())); sys.exit(1)
    print("  OK .hash = md5(.bytes) = %s" % h)
else:
    print("  !! 未找到 %s：--apply 时不会推送 .hash，游戏将回落内置清单" % hp.name)

ip = Path(info_p)
want_info = struct.pack("<H", len(want_crc)) + want_crc.encode() + struct.pack("<q", want_size)
if not ip.exists() or ip.stat().st_size == 0:
    ip.write_bytes(want_info)
    print("  -- 已生成 __info = %s" % want_info.hex())
else:
    got = ip.read_bytes()
    print("  __info = %s" % got.hex())
    if got != want_info:
        print("XX __info 与 bundle 不一致；期望 %s" % want_info.hex()); sys.exit(1)
    print("  OK __info 自洽（u16LE 8 + CRC 串 + i64LE 大小）")
PYEOF

# .hash 缺失则本地补一份（内容 = md5(.bytes)），不猜
if [ ! -f "${MANIFEST%.bytes}.hash" ]; then
  warn "$MAN_BASE.hash 不存在，用 md5($MANIFEST) 现生成"
  MANS="$WORK/$MAN_BASE.bytes"; MANH="$WORK/$MAN_BASE.hash"
  cp -f "$MANIFEST" "$MANS"; printf '%s' "$MAN_MD5" > "$MANH"
else
  MANS="$MANIFEST"; MANH="${MANIFEST%.bytes}.hash"
fi

# ---------------------------------------------------------------- 计划
say "== 计划 =="
say "1) su mkdir -p ${DEV_CDIR}；push __data / __info 到 $STAGE 后 su cp 落地"
say "2) su cp $MAN_BASE.bytes / .hash -> ${DEV_MDIR}（覆盖；.version 保持 ${VER}）"
say "3) chown 到 app uid（su stat 取，退化到 dumpsys package）、目录 chmod 2775、文件 chmod 664"
say "4) 回读 md5 复核；.version 只打印"
[ "$BACKUP" = 1 ] && say "   （落地前备份现有文件到 ${BK}）"
[ "$FORCE_STOP" = 1 ] && say "   （落地前 am force-stop ${PKG}）"
say "   注意：YooAsset 只在启动时校验缓存；--apply 前请先手动退出游戏。"

if [ "$APPLY" = 0 ]; then
  say "[dry-run] 未触碰设备。将执行："
  echo "    adb -s $SERIAL push \"$BUNDLE\" $STAGE/__data"
  echo "    adb -s $SERIAL push \"$INFO\"   $STAGE/__info"
  echo "    adb -s $SERIAL push \"$MANS\"   $STAGE/manifest.bytes"
  echo "    adb -s $SERIAL push \"$MANH\"   $STAGE/manifest.hash"
  echo "    adb -s $SERIAL shell \"su -c 'mkdir -p $DEV_CDIR && cp -f $STAGE/__data $DEV_CDIR/__data && ...'\""
  echo "    adb -s $SERIAL shell \"su -c 'chown <uid>:<gid> ... && chmod 2775 <dir> && chmod 664 <file>'\""
  say "加 --apply 才真跑。"
  exit 0
fi

# ---------------------------------------------------------------- 设备：取 uid
UID_GID="$(sh_su "stat -c %u:%g /data/user/0/$PKG/files" || true)"
if [ -z "$UID_GID" ] || [ "$UID_GID" = ":" ]; then
  APP_UID="$( (adb -s "$SERIAL" shell "dumpsys package $PKG" | tr -d '\r' | grep -m1 'userId=' | sed 's/.*userId=\([0-9][0-9]*\).*/\1/') || true)"
  [ -n "$APP_UID" ] || die "拿不到 app uid（su stat / dumpsys package 都失败），拒绝猜 chown"
  UID_GID="$APP_UID:$APP_UID"
fi
say "app uid:gid = $UID_GID"

# ---------------------------------------------------------------- 暂存 + 备份
sh_su "mkdir -p '$STAGE' '$BK' && chown shell:shell '$STAGE' '$BK' && chmod 775 '$STAGE' '$BK'" >/dev/null \
  || die "无法创建暂存目录 $STAGE"
adbpush "$BUNDLE" "$STAGE/__data"         >/dev/null || die "push __data 失败"
adbpush "$INFO"   "$STAGE/__info"         >/dev/null || die "push __info 失败"
adbpush "$MANS"   "$STAGE/manifest.bytes" >/dev/null || die "push manifest 失败"
adbpush "$MANH"   "$STAGE/manifest.hash"  >/dev/null || die "push hash 失败"
say "暂存 md5 复核："
sh_su "md5sum '$STAGE/__data' '$STAGE/__info'"
say "本地  bundle md5=$BUNDLE_MD5  manifest md5=$MAN_MD5"

if [ "$BACKUP" = 1 ]; then
  sh_su "for f in '$DEV_CDIR/__data' '$DEV_CDIR/__info' '$DEV_MDIR/$MAN_BASE.bytes' '$DEV_MDIR/$MAN_BASE.hash'; do \
      [ -f \"\$f\" ] && cp -f \"\$f\" '$BK'/; done; \
    echo '$DEV_CDIR' > '$BK/cache_dir.txt'; ls -l '$BK'" || warn "备份未完全成功（继续）"
fi

# ---------------------------------------------------------------- 落地
[ "$FORCE_STOP" = 1 ] && sh_su "am force-stop $PKG; sleep 1"
say "== 落地 =="
sh_su "mkdir -p '$DEV_CDIR' && cp -f '$STAGE/__data' '$DEV_CDIR/__data' && cp -f '$STAGE/__info' '$DEV_CDIR/__info' && \
      cp -f '$STAGE/manifest.bytes' '$DEV_MDIR/$MAN_BASE.bytes' && cp -f '$STAGE/manifest.hash' '$DEV_MDIR/$MAN_BASE.hash' && echo COPIED" \
  || die "复制失败"
sh_su "chown $UID_GID '$DEV_BASE/CacheFiles/${BUNDLE_MD5:0:2}' '$DEV_CDIR' 2>/dev/null; \
      chown $UID_GID '$DEV_CDIR/__data' '$DEV_CDIR/__info' '$DEV_MDIR/$MAN_BASE.bytes' '$DEV_MDIR/$MAN_BASE.hash' 2>/dev/null; \
      chmod 2775 '$DEV_BASE/CacheFiles/${BUNDLE_MD5:0:2}' '$DEV_CDIR' 2>/dev/null; \
      chmod 664 '$DEV_CDIR/__data' '$DEV_CDIR/__info' '$DEV_MDIR/$MAN_BASE.bytes' '$DEV_MDIR/$MAN_BASE.hash' 2>/dev/null; echo PERM_DONE"
warn "chmod/chown 在 /sdcard(FUSE) 上可能被忽略；以回读的 ls -ln 为准。属主不对 => YooAsset 写缓存失败（黑屏）"

# ---------------------------------------------------------------- 回读
say "== 回读复核 =="
sh_su "ls -ln '$DEV_CDIR'; ls -l '$DEV_MDIR'; md5sum '$DEV_CDIR/__data' '$DEV_CDIR/__info' '$DEV_MDIR/$MAN_BASE.bytes' '$DEV_MDIR/$MAN_BASE.hash'; echo -n 'version='; cat '$DEV_MDIR/PackageManifest_HotfixDll.version'"
DEV_BMD5="$(sh_su "md5sum '$DEV_CDIR/__data'" | awk '{print $1}')"
DEV_MMD5="$(sh_su "md5sum '$DEV_MDIR/$MAN_BASE.bytes'" | awk '{print $1}')"
sh_su "rm -rf '$STAGE'" >/dev/null 2>&1 || true
[ "$DEV_BMD5" = "$BUNDLE_MD5" ] || die "设备 __data md5=$DEV_BMD5 != 本地 $BUNDLE_MD5"
[ "$DEV_MMD5" = "$MAN_MD5" ]    || die "设备 manifest md5=$DEV_MMD5 != 本地 $MAN_MD5"
say "OK 注入完成：__data md5=${BUNDLE_MD5}，manifest md5=${MAN_MD5}，.version 未改（${VER}）"
say "  备份: ${BK}（回滚: $0 --restore-latest --apply）"
say "  验收由用户执行：冷启游戏看购买/关卡是否放开；logcat 查 'Failed verify file' / 'Failed to load assetBundle'"
