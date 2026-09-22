#!/usr/bin/env bash
# 无限升级 · 菜单开关状态「备份 / 恢复」工具（v1，2026-09-23）
#
# 背景（为什么需要它）：
#   菜单开关状态同时存在两处，任意一处丢失都会表现成「我的开关全没了 / 功能没效果」——
#     ① /data/data/<pkg>/files/starry_config.ini              ← 补丁 DLL 的实时数据源
#        （DLL 侧 StarryConfig.GetBool/GetInt，2 秒缓存读文件 ⇒ 改完 ~2s 即生效，**不用重启**）
#     ② /data/data/<pkg>/shared_prefs/starry_magic_prefs.xml  ← 菜单 UI 的状态源
#        （游戏启动时 Java 侧 ModBridge.reapplySavedFeaturesToNative() 按 `loadPrefBool(i,false)`
#         逐个推回 native；**菜单上按钮显示什么，以它为准**）
#   ⇒ 重装/清 prefs 后 ② 缺键 → 启动推回全 0 → ① 被覆盖成全关（本次事故即此机制）。
#   本脚本：可一键全开、可备份/恢复快照，无需重装、无需重刷 so。
#
# 用法：
#   bash restore_wxsj_state.sh --all-on              # 全部功能开满（写 ①+②；①立即生效，②重启后菜单同步）
#   bash restore_wxsj_state.sh --all-on --restart    # 同上，并自动重启游戏使菜单显示立即一致
#   bash restore_wxsj_state.sh --backup DIR          # 只备份当前 ①+② 到 DIR
#   bash restore_wxsj_state.sh --restore DIR [--restart]   # 用 DIR 里的备份覆盖回去
#   bash restore_wxsj_state.sh --show                # 打印当前 ①+② 状态
# 说明：① 写 ② 需要游戏停止，脚本默认先 force-stop（--no-stop 跳过）；
#       ② 默认允许在直写被 SELinux 拒时临时 setenforce 0（写完立刻还原；--no-permissive 禁用）。
set -uo pipefail

PKG="${WXSJ_PKG:-com.xingzuodaoshi.infinityUpgrade}"
DD="/data/data/$PKG"
INI="$DD/files/starry_config.ini"
PREF="$DD/shared_prefs/starry_magic_prefs.xml"
TMP="/data/local/tmp/wxsj_state_tool"
ADB="${ADB:-adb}"

MODE=""; DIR=""; RESTART=0; NO_STOP=0; PERMISSIVE=1
while [ $# -gt 0 ]; do
  case "$1" in
    --all-on)  MODE="allon" ;;
    --backup)  MODE="backup"; DIR="${2:-}"; shift ;;
    --restore) MODE="restore"; DIR="${2:-}"; shift ;;
    --show)    MODE="show" ;;
    --restart) RESTART=1 ;;
    --no-stop) NO_STOP=1 ;;
    --no-permissive) PERMISSIVE=0 ;;
    *) echo "未知参数：$1"; exit 2 ;;
  esac
  shift
done
[ -n "$MODE" ] || { sed -n '1,28p' "$0"; exit 2; }

sh_su()  { "$ADB" shell su -c "$1"; }
read_f() { sh_su "cat '$1'"; }
# 就地覆盖写（> 重定向保留 inode/属主/SELinux 上下文；`sed -i`/`cp` 会因 app_data_file 目录禁写而失败）
write_f() { "$ADB" push "$1" "$TMP/$2" >/dev/null && sh_su "cat '$TMP/$2' > '$3'"; }

local_backup() {   # $1 = host dir
  mkdir -p "$1"
  read_f "$INI"  > "$1/starry_config.ini" 2>/dev/null || true
  read_f "$PREF" > "$1/starry_magic_prefs.xml" 2>/dev/null || true
  echo "[backup] $1/starry_config.ini ($(wc -c < "$1/starry_config.ini" | tr -d ' ') B)"
  echo "[backup] $1/starry_magic_prefs.xml ($(wc -c < "$1/starry_magic_prefs.xml" | tr -d ' ') B)"
}

# ① 全开 ini（键名 ←→ 菜单号：1 无敌/2 秒杀/4 免广告/5 物品不减/6 技能免费升级/
#    7 广告奖励翻倍(ads_reward_x=2)/8 物品翻倍/9 无限挑战秘境/10 皮肤全解锁/11 功能全解锁）
gen_ini() {
  cat > "$1" <<'EOF'
block_report=1
god=1
oneshot=1
free_ads=1
item_keep=1
skill_free=1
ads_reward_x=2
item_double=1
mijing_free=1
skin_unlock=1
func_unlock=1
EOF
}

# ② 全开 prefs：保留鉴权条目，只补/改 boolean 键（3 = Category_功能 非开关，跳过）
gen_pref() {   # $1 = 输入 xml, $2 = 输出 xml
  python3 - "$1" "$2" <<'PY'
import re, sys
src = open(sys.argv[1], encoding='utf-8').read()
if '</map>' not in src:
    sys.exit('[pref] 输入不是合法的 SharedPreferences XML')
want = ['1', '2', '4', '5', '6', '7', '8', '9', '10', '11']
for k in want:                       # 先删同名旧条目，再统一插入，避免重复键
    src = re.sub(r'[ \t]*<boolean name="%s" value="(?:true|false)"\s*/>\n' % k, '', src)
ins = ''.join('    <boolean name="%s" value="true" />\n' % k for k in want)
src = src.replace('</map>', ins + '</map>')
open(sys.argv[2], 'w', encoding='utf-8').write(src)
print('[pref] 已写入开关键：%s（鉴权条目原样保留：%s）'
      % (','.join(want), all(s in src for s in ('auth_token', 'auth_card_key', 'auth_grant'))))
PY
}

case "$MODE" in
  show)
    echo "==== ① $INI ===="; read_f "$INI"
    echo "==== ② ${PREF}（仅 boolean） ===="; read_f "$PREF" | grep -o '<boolean name="[^"]*" value="[^"]*"' ;;
  backup)
    [ -n "$DIR" ] || { echo "--backup 需要目录参数"; exit 2; }
    local_backup "$DIR" ;;
  allon|restore)
    if [ "$MODE" = "restore" ]; then
      [ -n "$DIR" ] || { echo "--restore 需要目录参数"; exit 2; }
      [ -f "$DIR/starry_config.ini" ] && [ -f "$DIR/starry_magic_prefs.xml" ] || { echo "$DIR 里缺少两个文件"; exit 2; }
      SRC_INI="$DIR/starry_config.ini"; SRC_PREF="$DIR/starry_magic_prefs.xml"
    else
      gen_ini /tmp/wxsj_allon.ini;        SRC_INI=/tmp/wxsj_allon.ini
      read_f "$PREF" > /tmp/wxsj_cur_prefs.xml 2>/dev/null || true
      [ -s /tmp/wxsj_cur_prefs.xml ] || { echo "读不到当前 prefs，改用最小 XML（可能导致鉴权丢失，已中止）"; exit 3; }
      gen_pref /tmp/wxsj_cur_prefs.xml /tmp/wxsj_allon_prefs.xml; SRC_PREF=/tmp/wxsj_allon_prefs.xml
    fi
    BK="/tmp/wxsj_state_backup_$(date +%Y%m%d_%H%M%S)"; local_backup "$BK" >/dev/null
    echo "[safe] 已备份现状 → $BK"
    if [ "$NO_STOP" = "0" ]; then
      "$ADB" shell am force-stop "$PKG" >/dev/null 2>&1
      echo "[step] 已 force-stop 游戏（防止 prefs 被进程内存覆盖）"
    fi
    echo "[note] SELinux 为 Enforcing 时，su 域对 app_data_file 只有读权限（写/建文件被拒）——"
    echo "       脚本会先试直写，被拒则临时 setenforce 0 写完立刻 setenforce 1（毫秒级窗口，KernelSU root）。"
    do_write() { write_f "$SRC_INI" ini "$INI"; write_f "$SRC_PREF" prefs "$PREF"; }
    do_verify() {
      OK=1
      for kv in god=1 oneshot=1 free_ads=1 item_keep=1 skill_free=1 ads_reward_x=2 item_double=1 mijing_free=1 skin_unlock=1 func_unlock=1 block_report=1; do
        read_f "$INI" | grep -qx "$kv" || { echo "❌ ini 校验失败：$kv"; OK=0; }
      done
      for k in 1 2 4 5 6 7 8 9 10 11; do
        read_f "$PREF" | grep -q "<boolean name=\"$k\" value=\"true\"" || { echo "❌ prefs 校验失败：键 $k"; OK=0; }
      done
      read_f "$PREF" | grep -q 'auth_token' || { echo "❌ 鉴权条目丢失！立刻用 $BK 回滚"; OK=0; }
      [ "$OK" = "1" ]
    }
    do_write
    if ! do_verify; then
      if [ "$PERMISSIVE" = "1" ]; then
        ORIG_SE="$(sh_su 'getenforce' | tr -d '\r')"
        echo "[step] 直写被 SELinux 拒绝 → 临时 permissive（原状态 ${ORIG_SE:-?}）"
        sh_su 'setenforce 0' >/dev/null 2>&1
        trap 'sh_su "setenforce 1" >/dev/null 2>&1' EXIT INT TERM
        do_write
      fi
    fi
    if do_verify; then
      if [ "${PERMISSIVE:-0}" = "1" ] && sh_su 'getenforce' | grep -q Permissive; then
        sh_su 'setenforce 1' >/dev/null 2>&1; trap - EXIT INT TERM
        echo "[step] SELinux 已还原为 $(sh_su 'getenforce' | tr -d '\r')"
      fi
      echo "✅ 恢复完成：① ini 全开（DLL ~2s 内生效，无需重启）；② prefs 全开（重启后菜单按钮显示为开）"
    else
      sh_su 'setenforce 1' >/dev/null 2>&1
      echo "❌ 恢复失败（上面是未通过的校验项）——现状已备份在 ${BK}，可先 --no-stop 重试或改菜单手点"
      exit 4
    fi
    if [ "$RESTART" = "1" ]; then
      "$ADB" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
      echo "[step] 已重启游戏（菜单显示应与 ini 一致）"
    fi
    ;;
esac
