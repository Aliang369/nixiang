#!/usr/bin/env bash
# 无悔华夏 4399：收官清理（只删本项目可再生大件，不碰 02_apks/08_output）。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME_DIR="$NIXIANG/06_dumped/无悔华夏4399"
RUNTIME="$GAME_DIR/runtime_20260914"

delete_tree() {
  local p="$1"
  if [[ -e "$p" ]]; then
    find "$p" -depth -delete
  fi
}

delete_file() {
  local p="$1"
  if [[ -e "$p" ]]; then
    unlink "$p"
  fi
}

# 可再生的拆包/静态分析大件
delete_tree "$NIXIANG/03_decompiled/无悔华夏4399"
delete_tree "$GAME_DIR/3.8.280"
delete_tree "$RUNTIME/frida_dexdump"
delete_tree "$RUNTIME/il2cpp_dump"
delete_file "$RUNTIME/base.odex.apk"
delete_file "$RUNTIME/global-metadata.runtime_hdd.dat"
delete_file "$RUNTIME/libil2cpp_hot.so"
delete_file "$RUNTIME/libunity_hot.so"

# 只保留精确 InMemoryDexFile 三件套与小型日志，供后续快速复现。
mkdir -p "$RUNTIME/inmemory_dex"
# L7 与 L4 的真实 DEX 字节完全相同，只保留 L4 三件套。
delete_file "$RUNTIME/inmemory_dex/inmemory_L7_e1_8621488.dex"
delete_file "$RUNTIME/inmemory_dex/inmemory_L7_e2_1902428.dex"
delete_file "$RUNTIME/inmemory_dex/inmemory_L7_e3_1108844.dex"
find "$RUNTIME/inmemory_dex" -maxdepth 1 -type f -name '*.dex' -print | sort

# 中间构建对象可删；保留最终 so 供 OTA/复打包。
delete_tree "$NIXIANG/mod菜单/starry-mod-menu/obj_whrhx4399"

echo "CLEANUP_DONE"
du -sh "$GAME_DIR" "$NIXIANG/03_decompiled/无悔华夏4399" 2>/dev/null || true
