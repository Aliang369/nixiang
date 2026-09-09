#!/usr/bin/env bash
# 星光菜单：装机后公共收尾（悬浮窗 + 全游戏卡密种子）
# 在 inject_*.sh 的 adb install 成功后调用：
#   _NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"   # scripts/ → nixiang
#   # shellcheck source=/dev/null
#   source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
#   starry_post_install "$PKG"

starry_post_install() {
  local PKG="${1:?package}"
  local HERE SEED
  HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  SEED="$HERE/ensure_starry_device_id_seed.sh"

  adb shell su -c "appops set '$PKG' SYSTEM_ALERT_WINDOW allow" 2>/dev/null || true
  if [[ -f "$SEED" ]]; then
    bash "$SEED" || true
  else
    echo "[starry_post_install] WARN missing $SEED"
  fi
  adb shell am force-stop "$PKG" 2>/dev/null || true
}
