#!/usr/bin/env bash
# 收官工具：裁剪唯一正式包（--prune）、打印文档同步片段（--sync-doc）、可选落一张原始 icon、
# 菜单模板字段自检 + 原文打印。**不创建任何 delivery 目录**（2026-10-28 用户指令：交付目录整套废止）。
# icon 可选：传 --icon-source 时按**原图像素**落 `08_output/{游戏}/icon_{游戏}_{边长}.png`
# （不放大/不缩小；源与已有文件相同就别重出）。旧 `_512.png`/`_144.png` 双档已废弃。
# 默认 dry-run；--apply 才写文件。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
GAME=""
APK=""
ICON_SOURCE=""
ICON_EDGE=""       # 源图边长（px），传给 --icon-source 时探测
ICON_NAME=""       # 落盘文件名 icon_{游戏}_{边长}.png
MENU_TEMPLATE=""
DATE="$(date +%Y%m%d)"
MODE="dry-run"
PRUNE=0
ALLOW_NO_HARDEN=0   # 加固门禁逃生门：必须同时给 --harden-reason（机器强制，2026-10-27 第六轮）
HARDEN_REASON="${HARDEN_REASON:-}"
SYNC_DOC=0
ALLOW_LEGACY_TEMPLATE=0
# 口令点名即必须交付（2026-10-12 霓虹深渊2 会话复盘 P0-4）：
# 用户在当轮口令里点名要 icon / 菜单模板时，必须显式传 --icon-source / --menu-template，
# 漏传一律 rc=1（以前只打一行 NOTE 就放过去了 —— 那次用户追问了两次「菜单模版 json 为什么
# 不发给我，让我复制？」「菜单模版 JSON 你有按照规范格式写的吗？」）。
EXPECT_ICON=0
EXPECT_MENU_TEMPLATE=0

usage() {
  cat >&2 <<'EOF'
用法:
  bash 04_scripts/_common/finalize_game_delivery.sh \
    --game 潮汐守望者 \
    [--apk 08_output/潮汐守望者/xxx.apk] \
    [--icon-source /path/app_icon.png] \
    [--menu-template 10_docs/潮汐守望者/cxswz_menu_template.json] \
    [--date YYYYMMDD] \
    [--prune] \
    [--sync-doc] \
    [--allow-legacy-template] \
    [--expect-icon] \
    [--expect-menu-template] \
    [--apply]

说明:
  - 默认 dry-run，只检查并打印将生成的交付路径。
  - --apply 才写文件（icon 落到 08_output/{游戏}/；裁剪仍只由 --prune 触发）。
  - --prune 才调用 prune_output_final.sh 裁剪 08_output/{游戏} 为唯一 APK。
  - --sync-doc 打印「粘到游戏摘要 + current.md」的文档同步片段（**只打印，不自动改文件**：
    `10_docs/_state/current.md` 与游戏摘要是多会话共享文件，须手工粘贴后按 AGENTS.md 内容级暂存）。
  - icon/菜单模板为**按需**项：显式传 --icon-source / --menu-template 才处理；
    都不传 ⇒ 本次不生成任何文件（也不再自动扫描 10_docs 下的模板）。
  - icon **只落一张原图** `icon_{游戏}_{边长}.png`（边长＝源图自身像素，2026-10-14 用户 m00631/m00668
    定案，旧 `_512.png`/`_144.png` 双档已废弃）；图标源与已有文件相同时**不要重出**。
  - 传 --menu-template 时：字段严格自检 + 原文打到终端（供直接复制）。
  - 菜单模板自检含分组行门禁（features 不得出现 Category_* 或纯分组名）+ feat_num 升序/去重/
    缺口提示，依据 `10_docs/_templates/菜单模板_规范与示例.md` §2。遗留模板确实带分组行时
    可加 --allow-legacy-template 把该门禁降级为 WARN。
  - --expect-icon / --expect-menu-template（2026-10-12 霓虹深渊2 会话复盘 P0-4）：
    **用户口令里点名要什么，就传哪个守卫**。传了守卫却没传对应内容 ⇒ rc=1（不再静默跳过）。
    传 --expect-menu-template 时会把模板 JSON **原文打到终端**，供直接复制发给用户；
    理由：那次会话用户两次追问「菜单模版 json 为什么不发给我，让我复制？」。
EOF
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --game) GAME="${2:-}"; shift 2 ;;
    --apk) APK="${2:-}"; shift 2 ;;
    --icon-source) ICON_SOURCE="${2:-}"; shift 2 ;;
    --menu-template) MENU_TEMPLATE="${2:-}"; shift 2 ;;
    --date) DATE="${2:-}"; shift 2 ;;
    --prune) PRUNE=1; shift ;;
    --sync-doc) SYNC_DOC=1; shift ;;
    --allow-legacy-template) ALLOW_LEGACY_TEMPLATE=1; shift ;;
    # 「口令点名即必须交付」：这两个开关是**守卫**不是产出 —— 传了它却没传对应内容 = 直接 FAIL
    --expect-icon) EXPECT_ICON=1; shift ;;
    --expect-menu-template) EXPECT_MENU_TEMPLATE=1; shift ;;
    --allow-no-harden) ALLOW_NO_HARDEN=1; shift ;;
    --harden-reason)   HARDEN_REASON="$2"; shift 2 ;;
    --apply) MODE="apply"; shift ;;
    -h|--help) usage ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage ;;
  esac
done

[[ -n "$GAME" ]] || usage
[[ "$DATE" =~ ^[0-9]{8}$ ]] || { echo "ERROR: --date 需为 YYYYMMDD" >&2; exit 2; }
GAME_DIR="$ROOT/08_output/$GAME"
# ── 加固门禁：**2026-10-27 已下线**（用户裁决撤销加固强制；脚本与凭证体系保留为停用件）──
# 原行为：交付前必须过 harden_apk.py check（C1–C9），FAIL 即 exit 4，逃生门 --allow-no-harden + --harden-reason。
# 现在：不再调用、不再拦截；参数仍解析（兼容旧命令）。见 AGENTS.md §6 与 10_docs/_state/规范变更日志.md。


# 2026-10-28 修：下方 echo 行用到 md5_of/sha256_of，而函数原先定义在其后 ⇒
# 每次运行都打印 `sha256_of: command not found` 且 apk_sha256 为空（裁剪清单/文档片段跟着缺指纹）。
# 定义必须在使用点之前。
abs_path() {
  local p="$1"
  if [[ "$p" == /* ]]; then printf '%s\n' "$p"; else printf '%s\n' "$ROOT/$p"; fi
}

md5_of() {
  if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi
}
sha256_of() {
  if command -v shasum >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'; else sha256sum "$1" | awk '{print $1}'; fi
}

echo "[finalize] game=$GAME"
echo "[finalize] apk=$APK"
echo "[finalize] apk_md5=$(md5_of "$APK")"
echo "[finalize] apk_sha256=$(sha256_of "$APK")"
echo "[finalize] menu_template=${MENU_TEMPLATE:-（未提供，跳过菜单模板交付）}"

if [[ -z "$APK" ]]; then
  apks=()
  while IFS= read -r -d '' f; do apks+=("$f"); done < <(find "$GAME_DIR" -maxdepth 1 -type f -name '*.apk' -print0)
  [[ "${#apks[@]}" -eq 1 ]] || {
    echo "ERROR: $GAME_DIR 顶层 APK 数=${#apks[@]}，请显式传 --apk" >&2
    exit 1
  }
  APK="${apks[0]}"
fi
APK="$(abs_path "$APK")"
[[ -f "$APK" ]] || { echo "ERROR: APK 不存在: $APK" >&2; exit 1; }
case "$APK" in
  "$GAME_DIR"/*) ;;
  *) echo "ERROR: 正式包必须在 $GAME_DIR 下: $APK" >&2; exit 1 ;;
esac
[[ "$APK" == *.apk ]] || { echo "ERROR: 正式包必须是 .apk: $APK" >&2; exit 1; }

APKSIGNER="${APKSIGNER:-}"
if [[ -z "$APKSIGNER" ]]; then
  # Windows Git Bash 的默认位置（与 04_scripts/*/**_sign.sh 的 BUILD_TOOLS 探测保持一致）
  for v in 36.0.0 36.1.0 35.0.0 34.0.0; do
    for c in \
      "$HOME/Library/Android/sdk/build-tools/$v/apksigner" \
      "${ANDROID_SDK_ROOT:-${LOCALAPPDATA:-}/Android/Sdk}/build-tools/$v/apksigner" \
      "${ANDROID_SDK_ROOT:-${LOCALAPPDATA:-}/Android/Sdk}/build-tools/$v/apksigner.bat"; do
      if [[ -f "$c" ]]; then APKSIGNER="$c"; break 2; fi
    done
  done
fi
# ⚠️ Windows 上 apksigner.bat 没有 x 位（-rw-r--r--），判可用性必须用 -f 不能用 -x ——
#    用 -x 会误报「未找到 apksigner」。（同 04_scripts/境界守望者/wor_sign.sh:55-57 的教训）
[[ -f "$APKSIGNER" ]] || { echo "ERROR: 未找到 apksigner，拒绝交付: ${APKSIGNER:-未检测到}" >&2; exit 1; }
command -v cygpath >/dev/null 2>&1 && APKSIGNER="$(cygpath -m "$APKSIGNER" 2>/dev/null || printf '%s' "$APKSIGNER")"
# ⚠️ apksigner(.bat) 是 Windows 程序（java 起家），**argv 里的文件路径也必须转 Windows 形式**。
#    只转 APKSIGNER 自己不够：POSIX 路径 `/e/nixiang/…` 会让 java 抛
#    FileNotFoundException: \e\nixiang\…（且经 .bat 转发后 CJK 还会乱码）。
#    另注：若调用方 `export MSYS_NO_PATHCONV=1`，Git Bash 的自动转换会被禁掉，
#    此时这里必须显式 cygpath。路径含空格/CJK，务必保留引号。
win_path() {
  if command -v cygpath >/dev/null 2>&1; then
    cygpath -m "$1" 2>/dev/null || printf '%s' "$1"
  else
    printf '%s' "$1"
  fi
}
APK_WIN="$(win_path "$APK")"
"$APKSIGNER" verify "$APK_WIN" >/dev/null || { echo "ERROR: 正式包签名校验失败: $APK" >&2; exit 1; }

# 选一个**真能 `from PIL import Image`** 的 python。
# ⚠️ 本机实况（2026-10-03 逐个实测）：
#    python3 / python → …\Programs\Python\Python311 ：没有 PIL（ModuleNotFoundError）
#    py (C:\windows\py.exe) → D:\Python313 ：`import PIL` **能过**，但 `from PIL import Image` 抛
#        ImportError: cannot import name '_imaging' from 'PIL'（装坏了）—— 所以探测必须用真导入，
#        只测 `import PIL` 会挑中它然后在 icon 生成处炸。
#    ✅ 可用的：$HOME/.dsh/dsh-runtimes/*/dependencies/python/python.exe 与 D:\Python313\python.exe
#        （都带 Pillow 12.3.0；注意 `py` 启动器和直接跑 D:\Python313\python.exe 的结果不一样）。
pick_py_with_pil() {
  local c
  local -a cands=()
  [[ -n "${PYTHON:-}" ]] && cands+=("$PYTHON")
  cands+=(python3 python py)
  # 绝对路径回退（PATH 上没有能用的 Pillow 时兜底）
  cands+=(
    "$HOME"/.dsh/dsh-runtimes/*/dependencies/python/python.exe
    /d/Python313/python.exe
    "$HOME"/AppData/Local/Programs/Python/Python3*/python.exe
  )
  for c in "${cands[@]}"; do
    [[ -n "$c" ]] || continue
    if [[ -e "$c" ]]; then :; else command -v "$c" >/dev/null 2>&1 || continue; fi
    "$c" -c 'from PIL import Image' >/dev/null 2>&1 || continue
    printf '%s' "$c"
    return 0
  done
  return 1
}

# 菜单模板**不再自动扫描**：不传就完全跳过（2026-10-05 用户定「按需交付」）。
if [[ -n "$MENU_TEMPLATE" ]]; then
  MENU_TEMPLATE="$(abs_path "$MENU_TEMPLATE")"
  [[ -f "$MENU_TEMPLATE" ]] || { echo "ERROR: 菜单模板不存在: $MENU_TEMPLATE" >&2; exit 1; }
fi

if [[ -n "$ICON_SOURCE" ]]; then
  ICON_SOURCE="$(abs_path "$ICON_SOURCE")"
  [[ -f "$ICON_SOURCE" ]] || { echo "ERROR: icon 源图不存在: $ICON_SOURCE" >&2; exit 1; }
  # 生成器：macOS 用 sips；其它平台走 Pillow（apply 阶段真正调用，此处只做可用性预检）
  if ! command -v sips >/dev/null 2>&1; then
    pick_py_with_pil >/dev/null \
      || { echo "ERROR: 无 sips 且无可用的 python+Pillow 生成 icon（可设 PYTHON=/path/to/python）" >&2; exit 1; }
  fi
  # 命名口径（2026-10-14 用户 m00631/m00668）：只落一张原图 icon_{游戏}_{边长}.png，边长＝源图自身像素。
  if command -v sips >/dev/null 2>&1; then
    ICON_EDGE="$(sips -g pixelWidth "$ICON_SOURCE" 2>/dev/null | awk '/pixelWidth/{print $2}')"
  fi
  if ! [[ "${ICON_EDGE:-}" =~ ^[0-9]+$ ]]; then
    _ICON_PY="$(pick_py_with_pil)" \
      || { echo "ERROR: 无法探测 icon 源图边长（无 sips 且无可用的 python+Pillow）" >&2; exit 1; }
    ICON_EDGE="$("$_ICON_PY" -c 'import sys;from PIL import Image;print(Image.open(sys.argv[1]).size[0])' "$(win_path "$ICON_SOURCE")" 2>/dev/null || true)"
  fi
  [[ "${ICON_EDGE:-}" =~ ^[0-9]+$ ]] \
    || { echo "ERROR: 无法读取 icon 源图边长（该文件不是可解析的图片？）: $ICON_SOURCE" >&2; exit 1; }
  ICON_NAME="icon_${GAME}_${ICON_EDGE}.png"
  # 「已有则不再生成」提示（历史事故：2026-10-14 底包迁移时把 r17 已交付的同一张图标又生成了一遍）
  _prev_icon="$(ls -1 "$ROOT/08_output/$GAME"/icon_${GAME}_*.png 2>/dev/null | head -1 || true)"
  if [[ -n "$_prev_icon" ]]; then
    echo "[finalize] NOTE: $GAME 已有交付图标 ${_prev_icon}；图标源未变时按「已有则不再生成」**不要重出**（不要传 --icon-source）。"
  fi
  if [[ "$ICON_EDGE" -lt 512 ]]; then
    echo "[finalize] NOTE: 源图仅 ${ICON_EDGE}×${ICON_EDGE}；本口径不做放大，将按原图像素落盘（如需更大尺寸请改从更高密度资源取原图）。" >&2
  fi
fi

if [[ -n "$MENU_TEMPLATE" ]]; then
# ⚠️ 校验器用的 python 也吃 Windows 路径（原生 Windows python 认不得 /e/… ）。
_VALIDATOR_PY="${PYTHON:-$(command -v python3 || command -v python || command -v py || true)}"
[[ -n "$_VALIDATOR_PY" ]] || { echo "ERROR: 未找到 python，无法校验菜单模板（可设 PYTHON=/path/to/python）" >&2; exit 1; }
"$_VALIDATOR_PY" - "$(win_path "$MENU_TEMPLATE")" "$ALLOW_LEGACY_TEMPLATE" <<'PY'
import json, sys
p, allow_legacy = sys.argv[1], sys.argv[2] == '1'
d = json.load(open(p, encoding='utf-8'))
assert set(d) == {'template_id', 'name', 'package', 'source', 'features'}, sorted(d)
assert all(set(x) == {'feat_num', 'name'} for x in d['features'])
feats = d['features']
# feat_num：整数、严格升序、无重复（规范 §2：feat_num = C++ featureList[] 真实下标，不得重编号）
nums = [x['feat_num'] for x in feats]
assert all(isinstance(n, int) and not isinstance(n, bool) for n in nums), nums
assert nums == sorted(nums), f'feat_num 未升序: {nums}'
assert len(set(nums)) == len(nums), f'feat_num 重复: {nums}'
# 分组行门禁（规范 §2：Category_* 是运行时占位，不得写进模板 features）
GROUP_NAMES = {'战斗', '解锁', '基础功能', '功能', '其他', '通用', '玩法', '数值',
               '战斗功能', '解锁功能', '功能开关', '修改功能', '游戏功能'}
bad = [x for x in feats if x['name'].startswith('Category_') or x['name'] in GROUP_NAMES]
if bad:
    msg = 'features 含分组行（规范 §2 禁止）: ' + str([(x['feat_num'], x['name']) for x in bad])
    if allow_legacy:
        print('menu_template WARN ' + msg, file=sys.stderr)
    else:
        print('ERROR: ' + msg, file=sys.stderr)
        print('ERROR: 分组行只是菜单里的 Category_ 占位，不属于模板内容；请从 features 删除。'
              '若是历史遗留模板暂不修，显式加 --allow-legacy-template 降级为 WARN。', file=sys.stderr)
        print('ERROR: 规范见 10_docs/_templates/菜单模板_规范与示例.md §2', file=sys.stderr)
        sys.exit(1)
# 缺口提示：feat_num 断档处 = Category_ 占用的下标
gaps = [n for n in range(min(nums), max(nums) + 1) if n not in set(nums)]
if gaps:
    print(f'NOTE: 检测到 feat_num 缺口 {gaps} —— 确认这是 Category_ 分组下标；'
          '录入后台时必须带缺口（从 0 连续填会把后面所有开关串位）', file=sys.stderr)
print('menu_template OK', d['template_id'], len(feats), '项', [x['name'] for x in feats])
PY
fi

ICON_OUT="$GAME_DIR/$ICON_NAME"
OUT_ASSETS=""
OUT_ASSETS_DESC=""
if [[ -n "$ICON_SOURCE" ]]; then
  OUT_ASSETS="icon ${ICON_EDGE}×${ICON_EDGE}（原图）"
  OUT_ASSETS_DESC="$ICON_NAME"
fi
if [[ -n "$MENU_TEMPLATE" ]]; then
  if [[ -n "$OUT_ASSETS" ]]; then
    OUT_ASSETS="${OUT_ASSETS} + 菜单模板自检/原文打印"
  else
    OUT_ASSETS="菜单模板自检/原文打印"
  fi
fi



[[ -d "$GAME_DIR" ]] || { echo "ERROR: 输出目录不存在: $GAME_DIR" >&2; exit 1; }

echo "[finalize] out_dir=$GAME_DIR"
if [[ -n "$ICON_SOURCE" ]]; then
  echo "[finalize] icon_source=$ICON_SOURCE"
  echo "[finalize] icon_out=${ICON_NAME}（原图 ${ICON_EDGE}×${ICON_EDGE}，不放大/不缩小；旧 512/144 双档已废弃）"
else
  echo "[finalize] icon_source=(未提供，跳过 icon 生成)"
fi
if [[ -n "$OUT_ASSETS" ]]; then
  echo "[finalize] on_demand=$OUT_ASSETS"
else
  echo "[finalize] on_demand=(无：本次不生成 icon / 不处理菜单模板)"
fi

# ---------- 「口令点名即必须交付」门禁（2026-10-12 霓虹深渊2 会话复盘 P0-4）----------
# 为什么要有这一段：那次会话用户两次追问菜单模板 JSON，根因是"用户当轮点名要什么"这件事
# 在脚本里没有机械判据 —— 不传参数就安静跳过（只打一行 NOTE，rc 仍是 0）。
# 现在：口令里点名了 icon / 菜单模板，就必须显式传 --icon-source / --menu-template，否则 rc=1。
EXPECT_FAIL=0
if [[ "$EXPECT_ICON" == "1" && -z "$ICON_SOURCE" ]]; then
  echo "ERROR: --expect-icon：本轮点名要交付 icon，但没有 --icon-source <app_icon.png>" >&2
  echo "       动作：从底包取官方最高密度资源那张原图（png），补传后重跑；本口径保留原图像素，不做放大。" >&2
  EXPECT_FAIL=1
fi
if [[ "$EXPECT_MENU_TEMPLATE" == "1" && -z "$MENU_TEMPLATE" ]]; then
  echo "ERROR: --expect-menu-template：本轮点名要交付菜单模板，但没有 --menu-template <…_menu_template.json>" >&2
  echo "       动作：按 10_docs/_templates/菜单模板_规范与示例.md 生成/自检模板后，补传后重跑。" >&2
  echo "       （模板必须无 Category_* 分组行；不确定就用 --allow-legacy-template 前先问用户）" >&2
  EXPECT_FAIL=1
fi
if [[ "$EXPECT_MENU_TEMPLATE" == "1" && -n "$MENU_TEMPLATE" ]]; then
  echo
  echo "================ 菜单模板 JSON 原文（直接复制粘贴给用户，别让用户自己找文件）================"
  cat "$MENU_TEMPLATE"
  echo "================ 菜单模板 JSON 原文结束（同时已自检见上）================"
  echo
fi
[[ "$EXPECT_FAIL" == "0" ]] || { echo "[finalize] 因本轮点名的交付物缺失而中止（未写任何文件）" >&2; exit 1; }

if [[ "$SYNC_DOC" == "1" ]]; then
  APK_BASE="$(basename "$APK")"
  APK_SHA="$(sha256_of "$APK")"
  APK_SIZE="$(stat -f%z "$APK" 2>/dev/null || stat -c%s "$APK")"
  echo "[finalize] ─── 文档同步片段（--sync-doc，仅打印；共享文件请手工粘贴后按 AGENTS.md 内容级暂存）───"
  echo "# ① 追加到 10_docs/_state/games/${GAME}.md 的「本地产物与关键输入」表："
  if [[ -n "$OUT_ASSETS_DESC" ]]; then
    echo "| \`08_output/${GAME}/${OUT_ASSETS_DESC}\` | icon（原始像素，未放大） | 按需交付 |"
  fi
  if [[ "$PRUNE" == "1" ]]; then
    echo "| \`10_docs/${GAME}/正式包裁剪清单_${DATE}.json\` | 裁剪清单（保留 ${APK_BASE}） | 最终交付 |"
  fi
  echo "# ② 追加到 10_docs/_state/current.md 该游戏行末尾（整行 ≤400 字符）："
  echo "收尾已交付：唯一正式包 ${APK_BASE}（${APK_SIZE} B，sha256 ${APK_SHA:0:12}…）"
  echo "# ③ 回扫提示（--apply 会重写下列产物，请回头核对已写过的摘要行）："
  if [[ -n "$ICON_SOURCE" ]]; then
    echo "#   - 08_output/${GAME}/${ICON_NAME}（原图 ${ICON_EDGE}×${ICON_EDGE}）"
  fi
  if [[ -n "$MENU_TEMPLATE" ]]; then
    echo "#   - 菜单模板源：10_docs/${GAME}/$(basename "$MENU_TEMPLATE")（已自检并打印原文）"
    echo "#     ★ 若本次修正了源模板（增删功能项 / feat_num 变化 / source 改写），10_docs/_state/games/${GAME}.md 的"
    echo "#       「本地产物与关键输入」表里该模板行的「N 项」与指纹摘要已过期，必须同步改写。"
    echo "#       （历史事故：模板 17 项→15 项后 games/疾风骑士.md:39 仍写 17 项）"
  fi
  if [[ "$PRUNE" == "1" ]]; then
    echo "#   - 10_docs/${GAME}/正式包裁剪清单_${DATE}.json"
  fi
  echo "[finalize] ─── 片段结束（脚本不改任何文档文件）───"
fi

if [[ "$MODE" != "apply" ]]; then
  echo "[finalize] dry-run：未写文件。确认后加 --apply。"
  exit 0
fi

if [[ -n "$ICON_SOURCE" ]]; then
  # 单张原图落盘：PNG 源直接逐字节复制（不重编码、不改像素）；其它格式转成 PNG 但保持原像素。
  if [[ "$ICON_SOURCE" == "$ICON_OUT" ]]; then
    echo "[finalize] icon 已在位（源==目标，跳过复制）: $ICON_OUT"
  elif [[ "$ICON_SOURCE" == *.png || "$ICON_SOURCE" == *.PNG ]]; then
    cp -f "$ICON_SOURCE" "$ICON_OUT" || { echo "ERROR: icon 落盘失败: $ICON_OUT" >&2; exit 1; }
    echo "[finalize] + ${ICON_NAME}（${ICON_EDGE}×${ICON_EDGE}，原图逐字节复制，未重编码）"
  elif command -v sips >/dev/null 2>&1; then
    sips -s format png "$ICON_SOURCE" --out "$ICON_OUT" >/dev/null \
      || { echo "ERROR: icon 转 PNG 失败: $ICON_SOURCE" >&2; exit 1; }
    echo "[finalize] + ${ICON_NAME}（${ICON_EDGE}×${ICON_EDGE}，转 PNG，原像素未改）"
  else
    # Windows / Linux 无 sips ⇒ 走 Pillow（跨平台等价实现；不 resize）
    PYTHON="$(pick_py_with_pil)" \
      || { echo "ERROR: 无 sips 且无可用的 python+Pillow，无法生成 icon（可设 PYTHON=/path/to/python）" >&2; exit 1; }
    [[ "$PYTHON" == */* ]] && PYTHON="$(win_path "$PYTHON")"
    "$PYTHON" - "$(win_path "$ICON_SOURCE")" "$(win_path "$ICON_OUT")" <<'PYEOF' || { echo "ERROR: icon 生成失败" >&2; exit 1; }
import sys
from PIL import Image
src, dst = sys.argv[1], sys.argv[2]
Image.open(src).convert("RGBA").save(dst)   # 原像素，不 resize
PYEOF
    echo "[finalize] + ${ICON_NAME}（${ICON_EDGE}×${ICON_EDGE}，原像素未改）"
  fi
fi

if [[ "$PRUNE" == "1" ]]; then
  MANIFEST="$ROOT/10_docs/$GAME/正式包裁剪清单_${DATE}.json"
  bash "$ROOT/04_scripts/_common/prune_output_final.sh" "$GAME_DIR" "$(basename "$APK")" --apply --manifest "$MANIFEST"
fi

echo "[finalize] done"
if [[ -n "$ICON_SOURCE" && -f "$ICON_OUT" ]]; then
  echo "$ICON_OUT"
fi
