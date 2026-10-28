#!/usr/bin/env bash
# 我在神界刷装备 157.0 正式包验证（默认路径可由 --apk 覆盖）。
#
# 覆盖：
#   1. APK/dex/so 指纹打印
#   2. classes3.dex 存在且包含星光入口类
#   3. 菜单模板四项与包内 so 字节一致
#   4. 包内 so == libs_sjzz 构建 so
#   5. apksigner/zipalign/包名核对
#   6. pre_install_gate.sh（JS 可选）
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
DEFAULT_APK="$ROOT/08_output/我在神界刷装备/我在神界刷装备_157.0_surgical-releaseSigned.apk"
SO_BUILD="$ROOT/mod菜单/starry-mod-menu/libs_sjzz/arm64-v8a/libstarry_magic.so"
MENU_JSON="$ROOT/10_docs/我在神界刷装备/wzsjszb_menu_template.json"
CPP_MAIN="$ROOT/mod菜单/starry-mod-menu/app/src/main/jni/MainModern.cpp"
CPP_MOD="$ROOT/mod菜单/starry-mod-menu/app/src/main/jni/Games/SjzzMod.cpp"
GATE="$ROOT/04_scripts/_common/pre_install_gate.sh"
PKG="com.windforce.rg.offical"

APK="$DEFAULT_APK"
RUN_GATE=1
JS_FILES=()

usage() {
  cat >&2 <<'EOF'
用法:
  bash 04_scripts/我在神界刷装备/sjzz_verify.sh [APK] [--js 注入JS] [--skip-gate]

默认 APK:
  08_output/我在神界刷装备/我在神界刷装备_157.0_surgical-releaseSigned.apk
EOF
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --apk)
      [[ $# -ge 2 ]] || usage
      APK="$2"
      shift 2
      ;;
    --js)
      [[ $# -ge 2 ]] || usage
      JS_FILES+=("$2")
      shift 2
      ;;
    --skip-gate)
      RUN_GATE=0
      shift
      ;;
    -h|--help)
      usage
      ;;
    -*)
      usage
      ;;
    *)
      APK="$1"
      shift
      ;;
  esac
done

fail() {
  echo "ERROR: $*" >&2
  exit 1
}

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  d="$HOME/Library/Android/sdk/build-tools/$v"
  if [[ -x "$d/apksigner" && -x "$d/zipalign" && -x "$d/aapt2" ]]; then
    BT="$d"
    break
  fi
done
[[ -n "$BT" ]] || fail "未找到可用的 Build Tools（需 apksigner/zipalign/aapt2）"
[[ -f "$APK" ]] || fail "APK 不存在: $APK"
[[ -f "$SO_BUILD" ]] || fail "构建 so 不存在: $SO_BUILD"
[[ -f "$MENU_JSON" ]] || fail "菜单模板不存在: $MENU_JSON"
[[ -f "$CPP_MAIN" && -f "$CPP_MOD" ]] || fail "门禁源码缺失"
[[ -f "$GATE" ]] || fail "门禁脚本不存在: $GATE"

TMP="$(mktemp -d /tmp/sjzz_verify.XXXXXX)"
trap 'rm -rf "$TMP"' EXIT

echo "[identity] APK"
echo "  path   = $APK"
echo "  size   = $(stat -f '%z' "$APK")"
echo "  md5    = $(md5 -q "$APK")"
echo "  sha256 = $(shasum -a 256 "$APK" | awk '{print $1}')"

unzip -Z1 "$APK" > "$TMP/entries.txt"
grep -qx 'classes3.dex' "$TMP/entries.txt" || fail "APK 缺少 classes3.dex"
grep -qx 'lib/arm64-v8a/libstarry_magic.so' "$TMP/entries.txt" \
  || fail "APK 缺少 lib/arm64-v8a/libstarry_magic.so"
unzip -p "$APK" classes3.dex > "$TMP/classes3.dex"
unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "$TMP/package.so"
[[ -s "$TMP/classes3.dex" && -s "$TMP/package.so" ]] || fail "包内 classes3.dex/so 为空"

echo "[identity] classes3.dex"
echo "  size   = $(stat -f '%z' "$TMP/classes3.dex")"
echo "  md5    = $(md5 -q "$TMP/classes3.dex")"
echo "  sha256 = $(shasum -a 256 "$TMP/classes3.dex" | awk '{print $1}')"
echo "[identity] libstarry_magic.so"
echo "  build  = $(md5 -q "$SO_BUILD")"
echo "  prompt = $(md5 -q "$TMP/package.so")"

for needle in \
  'SjzzHooks' \
  'SignSpoof' \
  'StarryAppComponentFactory' \
  'StarryInitProvider' \
  'MenuService'; do
  LC_ALL=C grep -aFq "$needle" "$TMP/classes3.dex" \
    || fail "classes3.dex 缺少字符串: $needle"
done
echo "[dex] 入口类字符串 5/5 命中"

python3.11 - "$MENU_JSON" > "$TMP/menu.tsv" <<'PY'
import json
import sys

data = json.load(open(sys.argv[1], encoding="utf-8"))
assert set(data) == {"template_id", "name", "package", "source", "features"}, sorted(data)
assert data["template_id"] == "wzsjszb"
assert data["package"] == "com.windforce.rg.offical"
expected = ["伤害倍率", "无敌", "免广告", "游戏加速"]
got = [(int(x["feat_num"]), x["name"]) for x in data["features"]]
assert got == [(i + 1, n) for i, n in enumerate(expected)], got
for _, name in got:
    print(name)
PY

menu_count=0
while IFS= read -r name; do
  [[ -n "$name" ]] || continue
  LC_ALL=C grep -aFq "$name" "$TMP/package.so" \
    || fail "包内 so 缺少菜单字符串: $name"
  menu_count=$((menu_count + 1))
done < "$TMP/menu.tsv"
[[ "$menu_count" -eq 4 ]] || fail "菜单数量不是 4: $menu_count"
echo "[menu] 模板/包内 so 四项一致"

build_md5="$(md5 -q "$SO_BUILD")"
pkg_md5="$(md5 -q "$TMP/package.so")"
[[ "$build_md5" == "$pkg_md5" ]] \
  || fail "包内 so != 构建 so ($build_md5 vs $pkg_md5)"
echo "[so] md5 闭环 = $build_md5"

"$BT/apksigner" verify --print-certs "$APK" > "$TMP/certs.txt"
grep -q 'CN=StarryMagic' "$TMP/certs.txt" || fail "签名 CN 不是 StarryMagic"
"$BT/zipalign" -c -p 4 "$APK" >/dev/null || fail "zipalign -c -p 4 失败"
"$BT/aapt2" dump badging "$APK" > "$TMP/badging.txt"
grep -q "package: name='$PKG'" "$TMP/badging.txt" || fail "包名不是 $PKG"
echo "[apk] 签名/对齐/包名通过"

if [[ "$RUN_GATE" == "1" ]]; then
  gate_args=(--cpp "$CPP_MAIN" --cpp "$CPP_MOD" --dex "$TMP/classes3.dex" \
    --so "$TMP/package.so" --menu-json "$MENU_JSON")
  if [[ ${#JS_FILES[@]} -gt 0 ]]; then
    for js in "${JS_FILES[@]}"; do
      gate_args+=(--js "$js")
    done
  fi
  bash "$GATE" "${gate_args[@]}"
fi

echo "[verify] PASS: $APK"
