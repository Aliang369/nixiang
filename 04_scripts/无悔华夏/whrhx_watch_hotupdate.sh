#!/usr/bin/env bash
# 无悔华夏 · 盯热更负载回填（清数据重装后游戏需要重新下载热更资源）
# 用途：轮询等待 /data/data/<pkg>/lebian/<pkg>-1/lib/arm64/libil2cpp.so 回填，
#       回填后核对 sha 是否仍等于校订 RVA 时用的那份；同时报告当前进程实际映射的是哪份 libil2cpp.so。
# 只读，不改设备任何状态。
set -uo pipefail

SERIAL="${WHRHX_SERIAL:-f9a116a2}"
PKG="${WHRHX_PKG:-com.yhhy.whrhx.aligames}"
LEBIAN_LIB="/data/data/$PKG/lebian/$PKG-1/lib/arm64/libil2cpp.so"
LEBIAN_DIR="/data/data/$PKG/lebian"
EXPECT_SO_SHA="79adda770c528633213efe26bd084d14839c58a3a5382007de19cfc3d897aec9"
ROUNDS="${WHRHX_ROUNDS:-60}"      # 60 × 30s = 30 分钟上限
INTERVAL="${WHRHX_INTERVAL:-30}"

# 设备侧执行（去掉 \r；stderr 由远端合并进 stdout，故下方一律做格式校验）
dev() { adb -s "$SERIAL" shell "$1" 2>/dev/null | tr -d '\r'; }

show_maps() {
  local pid; pid="$(dev "pidof $PKG" | awk '{print $1}')"
  if [[ -z "$pid" ]]; then echo "  游戏进程: 未运行"; return; fi
  echo "  游戏进程: pid=$pid"
  dev "su -c \"grep -E 'libil2cpp|libstarry_magic' /proc/$pid/maps\"" \
    | awk '{print $6}' | sort -u | sed 's/^/    maps: /'
}

echo "[watch] 轮询 ${LEBIAN_LIB}（每 ${INTERVAL}s，上限 $((ROUNDS*INTERVAL/60)) 分钟）"
for i in $(seq 1 "$ROUNDS"); do
  EXISTS="$(dev "su -c 'test -f $LEBIAN_LIB && echo YES || echo NO'" | tail -1)"
  SIZE="$(dev "su -c 'du -sh $LEBIAN_DIR 2>/dev/null'" | awk '{print $1}')"
  if [[ "$EXISTS" == "YES" ]]; then
    SHA="$(dev "su -c 'sha256sum $LEBIAN_LIB'" | awk '{print $1}')"
    if [[ "$SHA" =~ ^[0-9a-f]{64}$ ]]; then
      echo "[watch] 热更运行时 so 已回填（第 $((i*INTERVAL)) 秒）"
      echo "  size=$(dev "su -c 'stat -c %s $LEBIAN_LIB'")  sha256=$SHA"
      if [[ "$SHA" == "$EXPECT_SO_SHA" ]]; then
        echo "  ✓ 与校订 RVA 所用版本一致 ⇒ 本轮 RVA 仍然有效"
      else
        echo "  ! 与校订版本（79adda77…）不同 ⇒ RVA 可能再次漂移，需要重新 dump 校订"
      fi
      show_maps
      echo "=== [Whrhx] 最近日志 ==="
      adb -s "$SERIAL" logcat -d 2>/dev/null | LC_ALL=C grep -F "[Whrhx]" | tail -15
      exit 0
    fi
    echo "[watch] 文件已出现但 sha 读取异常，继续等…"
  fi
  echo "[watch] #$i lebian=$SIZE  so=未回填"
  sleep "$INTERVAL"
done

echo "[watch] 到期仍未回填。当前状态："
dev "su -c 'ls -la $LEBIAN_DIR'" | head -15
show_maps
echo "⇒ 游戏可能仍需你在界面上确认下载，或下载尚未完成。"
exit 2
