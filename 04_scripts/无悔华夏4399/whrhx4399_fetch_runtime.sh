#!/usr/bin/env bash
# 无悔华夏 4399：从真机只读提取乐变运行时底包（base.odex.apk，含明文 libil2cpp.so + 热更资源）。
# 只读：不安装、不启动、不清数据。bash 3.2 兼容。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PKG="${PKG:-com.yhhy.whrhx.m4399}"
SERIAL="${SERIAL:-f9a116a2}"
RUN_TAG="${RUN_TAG:-$(date +%Y%m%d)}"
RUN_DIR="${RUN_DIR:-$NIXIANG_ROOT/06_dumped/无悔华夏4399/runtime_$RUN_TAG}"
ADB_BIN="${ADB_BIN:-adb}"
REMOTE_LEB="/data/data/$PKG/lebian/$PKG-1/base.odex.apk"
OUT_DIR="$RUN_DIR/pkg"
OUT="$OUT_DIR/base.odex.apk"
REMOTE_TMP="/data/local/tmp/whrhx4399_base_odex.apk"

"$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1 || { echo "错误: 设备不可用: $SERIAL" >&2; exit 1; }

REMOTE_INFO="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'ls -l $REMOTE_LEB'" | tr -d '\r')"
[[ -n "$REMOTE_INFO" ]] || { echo "错误: 乐变负载不存在: ${REMOTE_LEB}（先启动游戏让乐变重建）" >&2; exit 1; }
REMOTE_SIZE="$(printf '%s\n' "$REMOTE_INFO" | awk '{print $5}')"
REMOTE_SHA="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'sha256sum $REMOTE_LEB'" | tr -d '\r' | awk '{print $1}')"
echo "remote: size=$REMOTE_SIZE sha256=$REMOTE_SHA"

mkdir -p "$OUT_DIR"
if [ -f "$OUT" ]; then
  LOCAL_SHA="$(shasum -a 256 "$OUT" | awk '{print $1}')"
  if [ "$LOCAL_SHA" = "$REMOTE_SHA" ]; then
    echo "已存在且一致，跳过拉取: $OUT"
  else
    echo "本地底包与设备不一致，重新拉取"
    rm -f "$OUT"
  fi
fi

if [ ! -f "$OUT" ]; then
  TMP="$OUT.part"
  rm -f "$TMP"
  echo "==> root 复制到 /data/local/tmp"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp $REMOTE_LEB $REMOTE_TMP'"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'chmod 644 $REMOTE_TMP'"
  echo "==> adb pull（约 1.6G）"
  if ! "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_TMP" "$TMP"; then
    rm -f "$TMP"
    echo "错误: adb pull 失败" >&2
    "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f $REMOTE_TMP'" >/dev/null 2>&1 || true
    exit 1
  fi
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f $REMOTE_TMP'" >/dev/null 2>&1 || true
  mv "$TMP" "$OUT"
fi

LOCAL_SIZE="$(stat -f '%z' "$OUT")"
LOCAL_SHA="$(shasum -a 256 "$OUT" | awk '{print $1}')"
[ "$LOCAL_SIZE" = "$REMOTE_SIZE" ] || { echo "错误: 大小不符 $LOCAL_SIZE != $REMOTE_SIZE" >&2; exit 1; }
[ "$LOCAL_SHA" = "$REMOTE_SHA" ] || { echo "错误: sha256 不符 $LOCAL_SHA != $REMOTE_SHA" >&2; exit 1; }

SO_SHA="$(unzip -p "$OUT" 'lib/arm64-v8a/libil2cpp.so' | shasum -a 256 | awk '{print $1}')"
SO_SIZE="$(unzip -l "$OUT" 'lib/arm64-v8a/libil2cpp.so' | awk '$4=="lib/arm64-v8a/libil2cpp.so"{print $1; exit}')"
echo "runtime libil2cpp.so size=$SO_SIZE sha256=$SO_SHA"
python3.11 - "$OUT_DIR/base_odex.json" "$PKG" "$OUT" "$LOCAL_SIZE" "$LOCAL_SHA" "$SO_SIZE" "$SO_SHA" <<'PY'
import json, sys
out, pkg, path, size, sha, so_size, so_sha = sys.argv[1:]
json.dump({"package": pkg, "base_odex": path, "size": int(size), "sha256": sha,
           "runtime_so": {"size": int(so_size), "sha256": so_sha}},
          open(out, "w", encoding="utf-8"), ensure_ascii=False, indent=2)
print("meta ->", out)
PY
echo "DONE $OUT"
