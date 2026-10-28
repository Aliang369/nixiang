#!/usr/bin/env bash
# 收官步骤 1：把 09_test 里的决定性日志行抽成 evidence/README.md（删日志前先落证据）
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
EV="10_docs/长生天机降世/evidence_20261028"
mkdir -p "$EV"
# 日志已按收官清理隔离到 99_archive/_trash_20261028/；脚本优先读隔离区，回退原位置
TROOT=$(ls -d 99_archive/_trash_20261028/nixiang_cstjjs_*/09_test/长生天机降世 2>/dev/null | head -1)
BASE="${TROOT:-09_test/长生天机降世}"
SRC1="$BASE/hotupdate_20261028_2149/logcat_feat.txt"
SRC2="$BASE/hotupdate_20261028_2149/logcat_run.txt"
echo "log base = $BASE"

{
  echo "# 长生天机降世 1.0.4 装机实测证据（2026-10-28，雷电 emulator-5554）"
  echo
  echo "> 目的：原文日志（1.2 M / 14 k 行）不入库，这里只留**决定性行** + 最短复现命令。"
  echo "> 采集：\`adb -s emulator-5554 logcat -d\`（进程 pid 24269，包 com.netkuu.changsheng.tjjs，1.0.4 / vc1040）"
  echo
  echo "## 1. 菜单框架与挂点落地"
  echo '```text'
  grep -a -m1 'StarryNativeLoad: loaded apk lib-dir so' "$SRC2" 2>/dev/null
  grep -a -m1 'MenuService: auth game_id' "$SRC2" 2>/dev/null
  grep -a -m1 'NativeMenu: attached ACTIVITY' "$SRC2" 2>/dev/null
  grep -a -m1 'Cstjjs\] enter-interp maps' "$SRC2" 2>/dev/null
  grep -a -m1 'Cstjjs\] hooked enter-interp' "$SRC2" 2>/dev/null
  grep -a -m1 'Cstjjs\] 初始化成功' "$SRC2" 2>/dev/null
  grep -a -m1 'Cstjjs\] MI ready' "$SRC2" 2>/dev/null
  echo '```'
  echo
  echo "## 2. 两项功能命中真实扣血路径（feat1 十倍伤害 / feat2 无敌）"
  echo '```text'
  grep -a 'Cstjjs\] patch Health' "$SRC1" 2>/dev/null | head -12
  echo '```'
  echo
  echo "## 3. 开关切换（21:48:48 两次 feat 变更；切换者无法区分人工/自动，按规定不当作 L3 回执）"
  echo '```text'
  grep -a 'Cstjjs\] feat' "$SRC1" 2>/dev/null | tail -6
  echo '```'
  echo
  echo "## 4. 登录链路（TapTap 内嵌登录页被拉起，登录后进战斗）"
  echo '```text'
  grep -a -m2 'START u0 {cmp=com.netkuu.changsheng.tjjs/com.taptap.sdk.login' "$SRC2" 2>/dev/null | cut -c1-200
  grep -a -m2 'DlgNetkuuLoginSystem' "$SRC2" 2>/dev/null | cut -c1-200
  echo '```'
  echo
  echo "## 5. 最短复现"
  echo '```bash'
  echo 'adb -s emulator-5554 logcat -c'
  echo 'adb -s emulator-5554 shell am start -n com.netkuu.changsheng.tjjs/com.unity3d.player.UnityPlayerActivity'
  echo 'adb -s emulator-5554 logcat -d | grep -aE "Cstjjs|StarryNativeLoad|MenuService"'
  echo '```'
  echo
  echo "## 6. 「非法登录」判据的取证（离线，不需跑游戏）"
  echo '```bash'
  echo '# 1) 从设备热更缓存拉运行时程序集（root）：'
  echo '#   /sdcard/Android/data/com.netkuu.changsheng.tjjs/files/yoo/DefaultPackage/CacheFiles/28/2881b5583742ca26faabf74502d508ae/__data'
  echo '# 2) UnityPy 解包 → Code.dll；3) ilspycmd -t ET.DlgNetkuuLoginSystem 反编译：'
  echo '#   见 06_dumped/长生天机降世/1.0.4/decomp/decomp_hot_login.cs:460'
  echo 'python3 04_scripts/长生天机降世/decomp_hot.sh'
  echo '```'
} > "$EV/README.md"

echo "evidence -> $EV/README.md ($(wc -l < "$EV/README.md") 行)"
sed -n '1,30p' "$EV/README.md"
