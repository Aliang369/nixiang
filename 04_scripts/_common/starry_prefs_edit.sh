#!/usr/bin/env bash
# ================================================================
#  starry_prefs_edit.sh — 星光 mod 偏好文件（授权会话 / 功能开关）预置与体检
#
#  为什么有（2026-10-15 霓虹深渊2 r19 会话复盘 P0-1）：
#    为恢复「官方包卸载重装后丢掉的卡密会话」花了约 2 小时 + 几十次真机取证，
#    最终根因只是**一个 XML 属性写法**：Android 的 SharedPreferences 解析器对
#    <boolean>/<int>/<float>/<long> **只读 `value=` 属性**；写成文本体
#    `<boolean name="5">true</boolean>` ⇒ 解析抛异常 ⇒ App 拿到**空 map** ⇒
#    代码缺省重写 9 个键、auth_* 全丢。现象（`[验证] grant rejected` +
#    `StarryPrefs: 功能列表变化 → 已清功能偏好；prev=[]` + 预置内容整包消失）
#    与「被谁清空」一模一样 ⇒ 极易误诊成「有东西在删我的 prefs」。
#    离线生成整份 XML 同样会被判空 map（同轮实测）⇒ 只能「在 App 自己写的文件上拼接」。
#  本脚本把三条固化：① 写之前**先校验**（校验不过不写）；② 只做「拼接」，不生成整包；
#    ③ 写前 force-stop 并断言进程已退，写后可 `--launch --verify` 按日志判据复核。
#
#  用法（真机唯一设备时可省 --serial；SERIAL 环境变量亦可）:
#    bash 04_scripts/_common/starry_prefs_edit.sh --show
#    bash 04_scripts/_common/starry_prefs_edit.sh --validate <本地 xml>          # 纯离线体检
#    bash 04_scripts/_common/starry_prefs_edit.sh --seed-auth \
#         --grant '1|dev_xxx|nhsy2|1792080205|1|1|kid.hexsig' \
#         --token '<token>' --card 'AL-XXXX-XXXX-XXXX-XXXX' \
#         [--exp '2026-10-21T16:03:25+00:00'] [--last-ok 1791993849260] \
#         [--launch] [--verify] [--dry-run]
#    bash 04_scripts/_common/starry_prefs_edit.sh --seed-features 5,7,8,9,10,11,12 \
#         [--launch] [--verify]
#
#  退出码: 0=成功 / 2=用法或参数错 / 3=XML 校验失败 / 4=进程没停干净 / 5=写入后复核失败
# ================================================================
set -euo pipefail

SERIAL="${SERIAL:-}"
PKG="${PKG:-com.veewo.neonabyss2}"
PKG_PREFS="starry_magic_prefs.xml"
MODE=""
XMLFILE=""
GRANT=""
TOKEN=""
CARD=""
EXP=""
LASTOK=""
FEATURES=""
LAUNCH=0
VERIFY=0
DRY=0
WORK=""

log()  { printf '[prefs] %s\n' "$*"; }
warn() { printf '[prefs] ⚠ %s\n' "$*" >&2; }
die()  { printf '[prefs] ✗ %s\n' "$*" >&2; exit "${2:-1}"; }

usage() {
  sed -n '2,30p' "$0" | sed 's/^# \{0,1\}//'
  exit 2
}

while [ $# -gt 0 ]; do
  case "$1" in
    --serial)       SERIAL="${2:-}"; shift 2 ;;
    --pkg)          PKG="${2:-}"; shift 2 ;;
    --show)         MODE=show; shift ;;
    --validate)     MODE=validate; XMLFILE="${2:-}"; shift 2 ;;
    --seed-auth)    MODE=seed; shift ;;
    --seed-features) MODE=feat; FEATURES="${2:-}"; shift 2 ;;
    --grant)        GRANT="${2:-}"; shift 2 ;;
    --token)        TOKEN="${2:-}"; shift 2 ;;
    --card)         CARD="${2:-}"; shift 2 ;;
    --exp)          EXP="${2:-}"; shift 2 ;;
    --last-ok)      LASTOK="${2:-}"; shift 2 ;;
    --launch)       LAUNCH=1; shift ;;
    --verify)       VERIFY=1; LAUNCH=1; shift ;;
    --dry-run)      DRY=1; shift ;;
    -h|--help)      usage ;;
    *)              die "未知参数: $1" 2 ;;
  esac
done

[ -n "${MODE}" ] || usage

ADB="adb"
[ -n "${SERIAL}" ] && ADB="adb -s ${SERIAL}"
REMOTE_DIR="/data/data/${PKG}/shared_prefs"
REMOTE="${REMOTE_DIR}/${PKG_PREFS}"
TMPDIR_LOCAL="$(mktemp -d)"
cleanup() { rm -rf "${TMPDIR_LOCAL}"; }
trap cleanup EXIT

# ---------------------------------------------------------------- XML 体检
# Android SharedPreferences 解析规则（XmlUtils.readThisValueXml）：
#   string  → 值在**文本体**；int/long/float/boolean → 值在 **value= 属性**；
#   set     → 子元素必须是 <string>；根必须是 <map>；每个子元素必须有 name。
validate_xml() { # $1 = 文件；返回 0/3，错误打到 stderr
  awk -v F="$1" '
    function attr(tag, key,   i, s, j) {
      i = index(tag, key "=\"")
      if (i == 0) return ""
      s = substr(tag, i + length(key) + 2)
      j = index(s, "\"")
      if (j == 0) return ""
      return substr(s, 1, j - 1)
    }
    function tagname(tag,   s, j) {
      s = tag
      sub(/^[ \t<]+/, "", s)
      j = index(s, " ")
      if (j > 0) s = substr(s, 1, j - 1)
      sub(/>$/, "", s)
      sub(/\/$/, "", s)
      return s
    }
    function trim(s) { gsub(/^[ \t\r\n]+/, "", s); gsub(/[ \t\r\n]+$/, "", s); return s }
    function errm(msg) { printf("[prefs] ✗ %s\n", msg) > "/dev/stderr"; errors++ }
    function flushbody(   t) {
      if (cur == "") { body = ""; return }
      if (cur != "string" && cur != "set") {
        t = trim(body)
        if (t != "")
          errm(sprintf("元素 <%s> 的值必须写在 value= 属性里（收到文本体 %s）", cur, substr(t, 1, 40)))
      }
      body = ""
    }
    BEGIN { errors = 0; keys = 0; root = 0; cur = ""; body = ""; buf = ""; lineno = 1; in_set = 0 }
    { buf = buf $0 "\n" }
    END {
      n = length(buf); i = 1
      while (i <= n) {
        c = substr(buf, i, 1)
        if (c != "<") {
          body = body c
          if (c == "\n") lineno++
          i++
          continue
        }
        j = index(substr(buf, i + 1), ">")
        if (j == 0) { errm(sprintf("第 %d 行附近：标签缺闭合的 >（文件被截断？）", lineno)); break }
        inner = substr(buf, i + 1, j - 1)
        tmp = substr(buf, i, j + 1); gsub(/[^\n]/, "", tmp); lineno += length(tmp)
        i = i + j + 1
        if (inner ~ /^\?/ || inner ~ /^!/) { body = ""; continue }   # 声明 / 注释
        if (inner ~ /^\//) {                                          # 闭合标签
          ct = substr(inner, 2)
          if (ct == "map" || ct == "set") { flushbody(); cur = ""; if (ct == "set") in_set = 0; continue }
          if (cur != "" && ct != cur) errm(sprintf("第 %d 行：</%s> 与 <%s> 不匹配", lineno, ct, cur))
          flushbody(); cur = ""; continue
        }
        selfclose = (substr(inner, length(inner), 1) == "/")
        tg = tagname(inner)
        if (tg == "map") { root = 1; cur = selfclose ? "" : "map"; body = ""; continue }
        if (tg != "string" && tg != "int" && tg != "long" && tg != "float" && tg != "boolean" && tg != "set") {
          errm(sprintf("第 %d 行：未知元素 <%s>（合法：string/int/long/float/boolean/set）", lineno, tg))
          body = ""; continue
        }
        nm = attr(inner, "name")
        if (tg == "set") {
          if (nm == "") errm(sprintf("第 %d 行：<set> 缺 name 属性", lineno))
          else { if (seen[nm]++) errm(sprintf("第 %d 行：键名重复 name=\"%s\"", lineno, nm)); keys++ }
          cur = selfclose ? "" : "set"; in_set = !selfclose; body = ""
          continue
        }
        if (nm == "") {
          if (in_set && tg == "string") { cur = ""; body = ""; continue }   # <set> 成员 <string> 无 name 属正常
          errm(sprintf("第 %d 行：<%s> 缺 name 属性", lineno, tg)); body = ""; continue
        }
        if (seen[nm]++) errm(sprintf("第 %d 行：键名重复 name=\"%s\"", lineno, nm))
        keys++
        if (tg == "string") {
          if (attr(inner, "value") != "") {
            errm(sprintf("第 %d 行：<string name=\"%s\" value=\"…\"/> —— Android 读的是文本体，属性值会被静默丢弃", lineno, nm))
            printf("[prefs]    修法：<string name=\"%s\">值</string>\n", nm) > "/dev/stderr"
          }
        } else if (attr(inner, "value") == "") {
          errm(sprintf("第 %d 行：<%s name=\"%s\"> 缺 value= 属性 ⇒ Android 解析抛异常 ⇒ App 得到空 map", lineno, tg, nm))
          printf("[prefs]    修法：<%s name=\"%s\" value=\"…\" />（布尔/整数/长整/浮点一律写属性，不写文本体）\n", tg, nm) > "/dev/stderr"
        }
        cur = selfclose ? "" : tg; body = ""
      }
      flushbody()
      if (cur != "") errm(sprintf("元素 <%s> 没有闭合标签", cur))
      if (!root) errm("根元素不是 <map>（Android 只认 <map>）")
      if (errors == 0) printf("[prefs] ✓ XML 体检通过（%d 个键）\n", keys)
      exit(errors == 0 ? 0 : 3)
    }
  ' "$1"
}

# ---------------------------------------------------------------- 拼接（绝不全量生成）
xml_escape() { printf '%s' "$1" | sed -e 's/&/\&amp;/g' -e 's/</\&lt;/g' -e 's/>/\&gt;/g'; }

splice_in() { # $1=原 xml  $2=要插入的行文件  $3=输出 xml
  awk -v insfile="$2" '
    BEGIN { n = 0; while ((getline l < insfile) > 0) { n++; ins[n] = l } }
    /<\/map>/ { for (i = 1; i <= n; i++) print ins[i] }
    { print }
  ' "$1" > "$3"
}

build_insert_lines() { # $1 = 输出行文件
  : > "$1"
  if [ -n "${GRANT}" ];  then printf '    <string name="auth_grant">%s</string>\n'  "$(xml_escape "${GRANT}")"  >> "$1"; fi
  if [ -n "${TOKEN}" ];  then printf '    <string name="auth_token">%s</string>\n'  "$(xml_escape "${TOKEN}")"  >> "$1"; fi
  if [ -n "${CARD}" ];   then printf '    <string name="auth_card_key">%s</string>\n' "$(xml_escape "${CARD}")" >> "$1"; fi
  if [ -n "${EXP}" ];    then printf '    <string name="auth_expire_at">%s</string>\n' "$(xml_escape "${EXP}")" >> "$1"; fi
  if [ -n "${LASTOK}" ]; then printf '    <long name="auth_last_ok_at" value="%s" />\n' "$(xml_escape "${LASTOK}")" >> "$1"; fi
  if [ -n "${FEATURES}" ]; then
    IFS=','
    for f in ${FEATURES}; do
      f="$(printf '%s' "${f}" | tr -d ' ')"
      case "${f}" in
        ''|*[!0-9]*) die "功能槽位必须是数字，收到：${f}" 2 ;;
      esac
      printf '    <boolean name="%s" value="true" />\n' "${f}" >> "$1"
    done
    unset IFS
  fi
}

# ---------------------------------------------------------------- 设备侧
need_device() {
  $ADB get-state >/dev/null 2>&1 || die "设备未就绪（adb get-state 失败；用 --serial 指定）"
}

pid_of() { $ADB shell pidof "${PKG}" 2>/dev/null | tr -d '\r\n' ; }

pull_prefs() { # $1 = 目标本地文件
  $ADB exec-out su 0 cat "${REMOTE}" > "$1" 2>/dev/null || return 1
  [ -s "$1" ] || return 1
}

show_keys() {
  need_device
  local f="${TMPDIR_LOCAL}/show.xml"
  pull_prefs "${f}" || die "拉取 ${REMOTE} 失败（包/路径不对或缺 root）"
  log "文件：${REMOTE}（$(wc -c < "${f}" | tr -d ' ') B）"
  grep -o 'name="[A-Za-z0-9_]*"' "${f}" | sed 's/name="//; s/"//' | sort | awk '
    {
      if ($0 == "auth_token" || $0 == "auth_grant" || $0 == "auth_card_key" || $0 ~ /^auth_backup_(token|grant|card)/) {
        if ($0 == "auth_grant") print "  " $0 " = <已脱敏，长度由文件决定>";
        else print "  " $0 " = <已脱敏>";
      } else print "  " $0
    }'
  local n
  n=$(grep -c 'name="' "${f}" || true)
  log "共 ${n} 个键；布尔/整数必须是 value= 属性形式"
}

seed() {
  need_device
  local cur="${TMPDIR_LOCAL}/cur.xml"
  local new="${TMPDIR_LOCAL}/new.xml"
  local ins="${TMPDIR_LOCAL}/ins.txt"

  if [ -z "${GRANT}${TOKEN}${CARD}${EXP}${LASTOK}${FEATURES}" ]; then
    die "没有要写的内容（给 --grant/--token/--card/--exp/--last-ok 或 --seed-features）" 2
  fi
  build_insert_lines "${ins}"
  log "计划写入 $(wc -l < "${ins}" | tr -d ' ') 个键："
  sed 's/^/    +/' "${ins}"

  if [ "${DRY}" = 1 ]; then
    log "--dry-run：不 force-stop、不拉取、不写入"
    return 0
  fi

  # ① 先停进程（App 运行中被写会被它自己的缓存覆盖）
  $ADB shell am force-stop "${PKG}" >/dev/null 2>&1 || true
  sleep 2
  local p
  p="$(pid_of)"
  [ -z "${p}" ] || die "进程还在（pid=${p}）；再 force-stop 一次后重试" 4
  log "进程已停（pid 空）"

  # ② 拉 App 自己写的文件并体检（原地拼接的前提）
  pull_prefs "${cur}" || die "拉取 ${REMOTE} 失败（App 必须先跑过一次生成该文件）"
  cp "${cur}" "${TMPDIR_LOCAL}/orig.copy"
  validate_xml "${cur}" || die "设备上的原文件本身就不合法（见上），先按提示修它再预置" 3

  # ③ 拼接 + 写前复检（这一步就是本轮 2 小时的教训）
  splice_in "${cur}" "${ins}" "${new}"
  validate_xml "${new}" || die "拼接后的 XML 不合法 ⇒ 拒绝写入" 3

  # ④ 推回去并还原属主/权限
  local owner mode
  owner="$($ADB shell su 0 stat -c '%u:%g' "${REMOTE}" | tr -d '\r\n')"
  mode="$($ADB shell su 0 stat -c '%a' "${REMOTE}" | tr -d '\r\n')"
  $ADB push "${new}" /data/local/tmp/starry_prefs_new.xml >/dev/null || die "push 失败"
  $ADB shell su 0 cp /data/local/tmp/starry_prefs_new.xml "${REMOTE}" || die "写入失败"
  $ADB shell su 0 chown "${owner}" "${REMOTE}" || warn "chown 失败（owner=${owner}）"
  $ADB shell su 0 chmod "${mode}" "${REMOTE}" || warn "chmod 失败（mode=${mode}）"
  $ADB shell su 0 rm -f /data/local/tmp/starry_prefs_new.xml >/dev/null 2>&1 || true
  log "已写入（owner=${owner} mode=${mode}）"

  # ⑤ 复核：键在不在 + 长度
  local n_keys now_keys
  n_keys=$(wc -c < "${new}" | tr -d ' ')
  now_keys=$($ADB shell su 0 stat -c '%s' "${REMOTE}" | tr -d '\r\n')
  [ "${now_keys}" = "${n_keys}" ] || die "写入后大小不符（本地 ${n_keys} vs 设备 ${now_keys}）" 5
  if [ -n "${GRANT}" ]; then
    $ADB shell su 0 grep -q auth_grant "${REMOTE}" || die "写入后找不到 auth_grant" 5
  fi
  log "写入后大小一致（${now_keys} B）"

  if [ "${LAUNCH}" = 1 ]; then
    [ "${VERIFY}" = 1 ] && $ADB logcat -c >/dev/null 2>&1 || true
    $ADB shell monkey -p "${PKG}" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
    log "已冷启，等 45 s 后按日志判据复核…"
    sleep 45
    if [ "${VERIFY}" = 1 ]; then
      local out="${TMPDIR_LOCAL}/verify.log"
      $ADB logcat -d -v time > "${out}" 2>/dev/null || true
      log "判据（--verify）:"
      grep -a -E '\[验证\] grant ok|\[验证\] grant rejected|Toggle: #|install pass|install skip|core api missing|Fatal signal|FATAL EXCEPTION' "${out}" \
        | cut -c1-140 | tail -20 | sed 's/^/    /' || true
      if grep -aq '\[验证\] grant rejected' "${out}"; then
        warn "仍见到 grant rejected ⇒ 会话未生效（查 auth_grant/device_id/game_id 三者是否与卡密绑定一致）"
        exit 5
      fi
      log "未见到 grant rejected ✓"
    fi
  fi
}

case "${MODE}" in
  validate)
    [ -n "${XMLFILE}" ] || die "--validate 需要文件路径" 2
    [ -f "${XMLFILE}" ] || die "文件不存在：${XMLFILE}" 2
    validate_xml "${XMLFILE}"
    ;;
  show) show_keys ;;
  seed|feat) seed ;;
  *) usage ;;
esac
