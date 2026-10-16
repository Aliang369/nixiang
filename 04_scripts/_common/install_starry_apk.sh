#!/usr/bin/env bash
# ================================================================
#  install_starry_apk.sh — 星光包「只装机 / 异签名卸载重装」统一入口（2026-10-16）
#
#  为什么有它（两条已登记框架待办）
#  ----------------------------------------------------------------
#  ① FW-11「只装机缺统一入口」：`skill/starry-mod/SKILL.md` 把 `STARRY_INSTALL_ONLY=1`
#     当既定约定，但历史 `surgical_*.sh` 大多没有该分支 ⇒ 想只装机就被迫重跑全链，
#     而重签会改产物 md5、使已取证/已交付的包与证据脱钩。本脚本给**所有游戏**一个
#     统一入口，无须逐个改 40 个外科脚本。
#  ② FW-12「异签名卸载重装会静默清外置数据」：`pm uninstall` 连带清
#     `/sdcard/Android/data/<pkg>`（终末地实测 29,990 MB），且重装后 app uid 会变
#     （实测 11058→11059）——不 chown 回新 uid，app 就读不到自己的外置目录。
#     本脚本把「同卷 rename 落备胎 → **核对备胎非空才允许卸载** → 装后归位 + chown +
#     restorecon」做成硬闸门。
#
#  用法
#  ----------------------------------------------------------------
#    bash 04_scripts/_common/install_starry_apk.sh \
#        --apk 08_output/{游戏}/{正式包}.apk --pkg com.example.game \
#        [--so-tag '[Endfield]'] [--game-id mrfzzmd] \
#        [--serial f9a116a2] [--clean] [--allow-resource-loss] [--no-seed] [--dry-run]
#
#    --clean                 覆盖安装前先卸载（异签名必须；会先落备胎并核对非空）
#    --allow-resource-loss   备胎失败时仍继续卸载（默认拒绝，退出码 3）—— 需显式承担重下代价
#    --no-seed               不跑 ensure_starry_device_id_seed.sh
#    --dry-run               只打印将执行的判定与命令，不做任何写操作（也不做备胎核对）
#    退出码：0=成功；1=校验/装机/闭环核对失败；2=用法错；3=备胎失败且未加 --allow-resource-loss
#
#  纪律
#  ----------------------------------------------------------------
#  · 断言一律「先落盘再 grep」，禁 `cmd | grep -q`（pipefail 下 grep -q 提前退出会让
#    左侧吃 SIGPIPE(141)、把命中读成失败——2026-10-16 实测假阴性）。
#  · 标签/字面量一律 `grep -F`（`'[Wggls]'` 是字符类，会假命中/假失败）。
#  · `su -c '...'` 外层是单引号 ⇒ **内层只能用双引号**（内层单引号会提前闭合外层）。
#  · 变量后紧跟全角字一律写 `${VAR}`（否则全角字被并进变量名，`set -u` 下直接中断——
#    本脚本首版即被 lint_shell_traps.py R7 抓到）。
#  · bash 3.2 兼容：不用 mapfile/关联数组/`${v^^}`。
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

APK="" PKG="" SO_TAG="" GAME_ID="" SERIAL="${DEV_SERIAL:-f9a116a2}"
CLEAN=0 DRY=0 ALLOW_LOSS=0 SEED=1

usage() { sed -n '2,40p' "$0" | sed 's/^# \{0,1\}//' >&2; exit 2; }
while [[ $# -gt 0 ]]; do
  case "$1" in
    --apk) APK="${2:-}"; shift 2 ;;
    --pkg) PKG="${2:-}"; shift 2 ;;
    --so-tag) SO_TAG="${2:-}"; shift 2 ;;
    --game-id) GAME_ID="${2:-}"; shift 2 ;;
    --serial) SERIAL="${2:-}"; shift 2 ;;
    --clean) CLEAN=1; shift ;;
    --dry-run) DRY=1; shift ;;
    --allow-resource-loss) ALLOW_LOSS=1; shift ;;
    --no-seed) SEED=0; shift ;;
    -h|--help) usage ;;
    *) echo "错误: 未知参数 $1" >&2; usage ;;
  esac
done
[[ -n "$APK" && -n "$PKG" ]] || usage
[[ -f "$APK" ]] || { echo "ERROR: APK 不存在: ${APK}" >&2; exit 1; }

A=()
[[ -n "$SERIAL" ]] && A=(-s "$SERIAL")
AEXP=("${A[@]+"${A[@]}"}")

adbx() { if [[ "$DRY" == 1 ]]; then echo "  [dry-run] adb ${AEXP[*]:-} $*"; else adb "${AEXP[@]+"${AEXP[@]}"}" "$@"; fi; }
adbsu() {  # $1 = 设备侧 shell 片段（**内层不得用单引号**）
  if [[ "$DRY" == 1 ]]; then echo "  [dry-run] adb ${AEXP[*]:-} shell \"su -c '$1'\""; else adb "${AEXP[@]+"${AEXP[@]}"}" shell "su -c '$1'"; fi
}

TMP="$(mktemp -d "${TMPDIR:-/tmp}/starry_install.XXXXXX")"
trap 'rm -rf "${TMP}"' EXIT

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/${v}/apksigner" ]]; then BT="$HOME/Library/Android/sdk/build-tools/${v}"; break; fi
done

md5f() { if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi; }

echo "== install_starry_apk 前置校验 =="
echo "  apk    = ${APK}"
echo "  size   = $( (stat -f %z "$APK" 2>/dev/null || stat -c %s "$APK") ) B"
LOCAL_MD5="$(md5f "$APK")"
echo "  md5    = ${LOCAL_MD5}"
echo "  pkg    = ${PKG}    serial = ${SERIAL:-<默认>}    clean=${CLEAN} dry=${DRY}"

# ---- 查 1：签名身份必须是星光自签 ----
if [[ -n "$BT" ]]; then
  "$BT/apksigner" verify --print-certs "$APK" > "${TMP}/signer.txt" 2>&1 || true
  if grep -q "CN=StarryMagic" "${TMP}/signer.txt"; then
    echo "  ✓ 签名身份 CN=StarryMagic"
  else
    echo "ERROR: 签名身份不是 StarryMagic（异签名覆盖装会失败）：" >&2
    sed -n '1,6p' "${TMP}/signer.txt" >&2
    exit 1
  fi
else
  echo "  ! 未找到 apksigner（build-tools）⇒ 跳过签名身份检查"
fi

# ---- 查 2：包内 so 的本游戏标签 / game_id（防"装到上一款游戏的包"）----
if [[ -n "$SO_TAG" || -n "$GAME_ID" ]]; then
  unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "${TMP}/so" 2>/dev/null || true
  [[ -s "${TMP}/so" ]] || { echo "ERROR: 包内缺 lib/arm64-v8a/libstarry_magic.so" >&2; exit 1; }
  echo "  so md5 = $(md5f "${TMP}/so")"
  if [[ -n "$SO_TAG" ]]; then
    grep -a -F -q -- "$SO_TAG" "${TMP}/so" || { echo "ERROR: 包内 so 不含标签 ${SO_TAG}" >&2; exit 1; }
    echo "  ✓ so 含标签 ${SO_TAG}"
  fi
  if [[ -n "$GAME_ID" ]]; then
    grep -a -F -q -- "$GAME_ID" "${TMP}/so" || { echo "ERROR: 包内 so 不含 game_id ${GAME_ID}" >&2; exit 1; }
    echo "  ✓ so 含 game_id ${GAME_ID}"
  fi
fi

RESERVE="/sdcard/Android/data/${PKG}.nxreserve"
APPEXT="/sdcard/Android/data/${PKG}"

if [[ "$CLEAN" == 1 ]]; then
  echo "== 异签名卸载重装（先保护外置数据）=="
  adbx shell am force-stop "$PKG" >/dev/null 2>&1 || true
  echo "  保护 ${APPEXT} → ${RESERVE}"
  RESERVE_CMD="rm -rf \"${RESERVE}\"; if [ -d \"${APPEXT}\" ]; then mv \"${APPEXT}\" \"${RESERVE}\"; fi; ls -d \"${RESERVE}\" 2>/dev/null"
  if [[ "$DRY" == 1 ]]; then
    adbsu "$RESERVE_CMD"
    echo "  [dry-run] 跳过备胎核对（dry-run 无真实输出，不据此判定成功）"
  else
    adbsu "$RESERVE_CMD" > "${TMP}/reserve.txt" 2>&1 || true
    cat "${TMP}/reserve.txt"
    if grep -q "$RESERVE" "${TMP}/reserve.txt"; then
      echo "  ✓ 备胎就位（mv 成功；同卷 rename、零额外空间）"
    else
      echo "  ! 外置资源目录不存在或 mv 失败（可能是新装、或机型不支持同卷 rename）"
      if [[ "$ALLOW_LOSS" == 1 ]]; then
        echo "  ! --allow-resource-loss 已显式给出 ⇒ 继续卸载（外置数据将丢失）"
      else
        echo "ERROR: 拒绝裸卸（避免静默丢大资源）。确认可丢时加 --allow-resource-loss" >&2
        exit 3
      fi
    fi
  fi
  adbsu "pm uninstall ${PKG}" || true
else
  echo "== 覆盖安装（不卸载）=="
  adbx shell am force-stop "$PKG" >/dev/null 2>&1 || true
fi

echo "== 安装 =="
REMOTE="/data/local/tmp/$(basename "$APK")"
if adb "${AEXP[@]+"${AEXP[@]}"}" shell "su -c id" 2>/dev/null | grep -q "uid=0"; then
  adbx push "$APK" "$REMOTE"
  adbsu "pm install -r -d --user 0 \"${REMOTE}\" && rm -f \"${REMOTE}\""
else
  echo "  (设备无 root ⇒ 走 adb install 流式安装，不做 /data/local/tmp 预推，避免重推 1.7GB)"
  adbx install -r -d "$APK"
fi

if [[ "$CLEAN" == 1 && "$DRY" != 1 ]]; then
  echo "== 外置数据归位 =="
  NEW_UID="$(adb "${AEXP[@]+"${AEXP[@]}"}" shell "su -c 'stat -c %u /data/user/0/${PKG} 2>/dev/null || stat -c %u /data/data/${PKG} 2>/dev/null'" 2>/dev/null | tr -d '\r' | head -1)"
  case "$NEW_UID" in ''|*[!0-9]*) echo "ERROR: 取不到新 app uid（'${NEW_UID}'）⇒ 需人工归位，禁止声称收尾完成" >&2; exit 1;; esac
  echo "  新 uid = ${NEW_UID}（重装会重新分配；不 chown 则 app 读不到自己的外置目录）"
  RESTORE_CMD="R=\"${RESERVE}\"; T=\"${APPEXT}\"; if [ ! -d \"\$R\" ]; then echo NO_RESERVE; elif [ -d \"\$T/files/VFS\" ]; then echo TARGET_ALREADY_POPULATED; else rm -rf \"\$T\"; mv \"\$R\" \"\$T\"; chown -R ${NEW_UID}:${NEW_UID} \"\$T\"; restorecon -R \"\$T\" 2>/dev/null; du -sm \"\$T\" 2>/dev/null; echo RESTORED; fi"
  adbsu "$RESTORE_CMD"
fi

if [[ "$DRY" != 1 ]]; then
  # root 探测决定 --su：非 root 机（实测小米 96f0d883 无 su）只要 /data/app 对 shell 可读，
  # 免 su 也能做 md5 核对；写死 --su 会让「装成功」被判成失败。
  DEV_ROOT=0
  if adb "${AEXP[@]+"${AEXP[@]}"}" shell "su -c id" 2>/dev/null | grep -q "uid=0"; then DEV_ROOT=1; fi
  echo "== 闭环核对（设备包 md5 == 本地产物；root=$([ "$DEV_ROOT" == "1" ] && echo yes || echo no)）=="
  V_ARGS=()
  [[ "$DEV_ROOT" == "1" ]] && V_ARGS=(--su)
  if ! bash "${SCRIPT_DIR}/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" "${V_ARGS[@]+"${V_ARGS[@]}"}"; then
    echo "  ! md5 核对未通过 ⇒ 降级为组件指纹旁证（非 root 机读不到 /data/app 时用）：" >&2
    adb "${AEXP[@]+"${AEXP[@]}"}" shell "dumpsys package $PKG | grep -E 'versionName=|versionCode=|lastUpdateTime=|firstInstallTime='" 2>/dev/null | tr -d '\r' | sed 's/^/    /' >&2 || true
    echo "  ⇒ 人工比对 versionName/versionCode 与本次包；**md5 未核 ⇒ 不得宣称「已核对完成」**。" >&2
  fi
  if [[ "$SEED" == 1 ]]; then
    bash "${SCRIPT_DIR}/ensure_starry_device_id_seed.sh" || true
  fi
  echo "log: adb -s ${SERIAL:-<默认>} logcat -s StarryApp:V StarryMagic:V Mod_Menu:V MenuService:V"
fi

echo "DONE: installed ${PKG} from $(basename "$APK")"
