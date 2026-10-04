#!/usr/bin/env bash
# 收官交付物打包：唯一正式 APK、icon 512/144、菜单模板副本、可选裁剪清单。
# 默认 dry-run；--apply 才写入 delivery 目录和裁剪 08_output。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
GAME=""
APK=""
ICON_SOURCE=""
MENU_TEMPLATE=""
DATE="$(date +%Y%m%d)"
MODE="dry-run"
PRUNE=0
SYNC_DOC=0
ALLOW_LEGACY_TEMPLATE=0

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
    [--apply]

说明:
  - 默认 dry-run，只检查并打印将生成的交付路径。
  - --apply 才创建 08_output/{游戏}/delivery_{日期}/。
  - --prune 才调用 prune_output_final.sh 裁剪 08_output/{游戏} 为唯一 APK。
  - --sync-doc 打印「粘到游戏摘要 + current.md」的文档同步片段（**只打印，不自动改文件**：
    `10_docs/_state/current.md` 与游戏摘要是多会话共享文件，须手工粘贴后按 AGENTS.md 内容级暂存）。
  - icon 源图存在时生成 512/144 PNG；菜单模板会做字段严格自检并复制一份。
  - 菜单模板自检含分组行门禁（features 不得出现 Category_* 或纯分组名）+ feat_num 升序/去重/
    缺口提示，依据 `10_docs/_templates/菜单模板_规范与示例.md` §2。遗留模板确实带分组行时
    可加 --allow-legacy-template 把该门禁降级为 WARN。
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
    --apply) MODE="apply"; shift ;;
    -h|--help) usage ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage ;;
  esac
done

[[ -n "$GAME" ]] || usage
[[ "$DATE" =~ ^[0-9]{8}$ ]] || { echo "ERROR: --date 需为 YYYYMMDD" >&2; exit 2; }
GAME_DIR="$ROOT/08_output/$GAME"
[[ -d "$GAME_DIR" ]] || { echo "ERROR: 输出目录不存在: $GAME_DIR" >&2; exit 1; }

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

if [[ -z "$MENU_TEMPLATE" ]]; then
  menus=()
  while IFS= read -r -d '' f; do menus+=("$f"); done < <(find "$ROOT/10_docs/$GAME" -maxdepth 1 -type f -name '*_menu_template.json' -print0 2>/dev/null)
  [[ "${#menus[@]}" -eq 1 ]] || {
    echo "ERROR: 未找到唯一菜单模板，请显式传 --menu-template（当前命中 ${#menus[@]} 个）" >&2
    exit 1
  }
  MENU_TEMPLATE="${menus[0]}"
fi
MENU_TEMPLATE="$(abs_path "$MENU_TEMPLATE")"
[[ -f "$MENU_TEMPLATE" ]] || { echo "ERROR: 菜单模板不存在: $MENU_TEMPLATE" >&2; exit 1; }

if [[ -n "$ICON_SOURCE" ]]; then
  ICON_SOURCE="$(abs_path "$ICON_SOURCE")"
  [[ -f "$ICON_SOURCE" ]] || { echo "ERROR: icon 源图不存在: $ICON_SOURCE" >&2; exit 1; }
  # 生成器：macOS 用 sips；其它平台走 Pillow（apply 阶段真正调用，此处只做可用性预检）
  if ! command -v sips >/dev/null 2>&1; then
    pick_py_with_pil >/dev/null \
      || { echo "ERROR: 无 sips 且无可用的 python+Pillow 生成 icon（可设 PYTHON=/path/to/python）" >&2; exit 1; }
  fi
fi

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

DELIVERY="$GAME_DIR/delivery_$DATE"
echo "[finalize] game=$GAME"
echo "[finalize] apk=$APK"
echo "[finalize] apk_md5=$(md5_of "$APK")"
echo "[finalize] apk_sha256=$(sha256_of "$APK")"
echo "[finalize] menu_template=$MENU_TEMPLATE"
echo "[finalize] delivery=$DELIVERY"
if [[ -n "$ICON_SOURCE" ]]; then
  echo "[finalize] icon_source=$ICON_SOURCE"
  echo "[finalize] icon_out=icon_${GAME}_512.png, icon_${GAME}_144.png"
else
  echo "[finalize] icon_source=(未提供，跳过 icon 生成)"
fi

if [[ "$SYNC_DOC" == "1" ]]; then
  APK_BASE="$(basename "$APK")"
  APK_SHA="$(sha256_of "$APK")"
  APK_SIZE="$(stat -f%z "$APK" 2>/dev/null || stat -c%s "$APK")"
  echo "[finalize] ─── 文档同步片段（--sync-doc，仅打印；共享文件请手工粘贴后按 AGENTS.md 内容级暂存）───"
  echo "# ① 追加到 10_docs/_state/games/${GAME}.md 的「本地产物与关键输入」表："
  echo "| \`08_output/${GAME}/delivery_${DATE}/\` | 交付物：icon_${GAME}_512.png / icon_${GAME}_144.png + menu_template.json | 最终交付 |"
  if [[ "$PRUNE" == "1" ]]; then
    echo "| \`10_docs/${GAME}/正式包裁剪清单_${DATE}.json\` | 裁剪清单（保留 ${APK_BASE}） | 最终交付 |"
  fi
  echo "# ② 追加到 10_docs/_state/current.md 该游戏行末尾（整行 ≤400 字符）："
  echo "收尾已交付：唯一正式包 ${APK_BASE}（${APK_SIZE} B，sha256 ${APK_SHA:0:12}…）+ delivery_${DATE}/（icon 512/144 + 菜单模板）"
  echo "# ③ 回扫提示（--apply 会重写下列产物，请回头核对已写过的摘要行）："
  echo "#   - 08_output/${GAME}/delivery_${DATE}/icon_${GAME}_512.png、icon_${GAME}_144.png"
  echo "#   - 08_output/${GAME}/delivery_${DATE}/menu_template.json（内容来自 10_docs/${GAME}/$(basename "$MENU_TEMPLATE")）"
  echo "#     ★ 若本次修正了源模板（增删功能项 / feat_num 变化 / source 改写），10_docs/_state/games/${GAME}.md 的"
  echo "#       「本地产物与关键输入」表里该模板行的「N 项」与指纹摘要已过期，必须同步改写。"
  echo "#       （历史事故：模板 17 项→15 项后 games/疾风骑士.md:39 仍写 17 项）"
  if [[ "$PRUNE" == "1" ]]; then
    echo "#   - 10_docs/${GAME}/正式包裁剪清单_${DATE}.json"
  fi
  echo "[finalize] ─── 片段结束（脚本不改任何文档文件）───"
fi

if [[ "$MODE" != "apply" ]]; then
  echo "[finalize] dry-run：未写文件。确认后加 --apply。"
  exit 0
fi

mkdir -p "$DELIVERY"
if [[ -n "$ICON_SOURCE" ]]; then
  if command -v sips >/dev/null 2>&1; then
    sips -z 512 512 "$ICON_SOURCE" --out "$DELIVERY/icon_${GAME}_512.png" >/dev/null
    sips -z 144 144 "$ICON_SOURCE" --out "$DELIVERY/icon_${GAME}_144.png" >/dev/null
  else
    # Windows / Linux 无 sips ⇒ 走 Pillow（跨平台等价实现）
    PYTHON="$(pick_py_with_pil)" \
      || { echo "ERROR: 无 sips 且无可用的 python+Pillow，无法生成 icon（可设 PYTHON=/path/to/python）" >&2; exit 1; }
    [[ "$PYTHON" == */* ]] && PYTHON="$(win_path "$PYTHON")"
    "$PYTHON" - "$(win_path "$ICON_SOURCE")" "$(win_path "$DELIVERY")" "$GAME" <<'PYEOF' || { echo "ERROR: icon 生成失败" >&2; exit 1; }
import sys
from PIL import Image
src, out_dir, game = sys.argv[1], sys.argv[2], sys.argv[3]
im = Image.open(src).convert("RGBA")
for edge in (512, 144):
    im.resize((edge, edge), Image.LANCZOS).save(f"{out_dir}/icon_{game}_{edge}.png")
PYEOF
  fi
fi
cp -f "$MENU_TEMPLATE" "$DELIVERY/menu_template.json"

if [[ "$PRUNE" == "1" ]]; then
  MANIFEST="$ROOT/10_docs/$GAME/正式包裁剪清单_${DATE}.json"
  bash "$ROOT/04_scripts/_common/prune_output_final.sh" "$GAME_DIR" "$(basename "$APK")" --apply --manifest "$MANIFEST"
fi

echo "[finalize] done"
find "$DELIVERY" -maxdepth 1 -type f -print | sort
