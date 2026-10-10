#!/usr/bin/env bash
# ================================================================
# 无悔华夏 3.8.280 九游（aligames）— 「新版菜单 + 本轮 so」外科合并
#
# 语义（stages.md「调试内循环」的 menu_latest 变体）：
#   底包 = 真机现装的星空包（用户授权从设备拉取；Manifest 已是
#          Application=StarryApp + MenuService 的注入态，本轮不动 Manifest）
#   只改两个 zip 条目：
#     classes2.dex                     ← 07_patched/无悔华夏/menu_latest_20260923/classes2.dex
#                                        （仓库当前菜单源码 R8 release 重建 + StarryApp
#                                          .super 回改 Lcom/netease/nis/wrapper/MyApplication;）
#     lib/arm64-v8a/libstarry_magic.so ← 本轮 build_so 产物（科研真门 GetPolicyItemShowState）
#   其余条目（classes.dex / resources.arsc / libc++_shared.so / certs / Manifest …）逐字节不动。
#   → zipalign → 星空证书重签 → 四查审计。不安装、不卸载、不清数据。
#
# 注意：同名产物若已存在请先删除（磁盘峰值 ≈ 底包 ×3）。
# P1-⑦（2026-10-11）：出包后默认**删除同批（同一 8 位日期）旧产物与其 .idsig**，
# 只留本轮包（本轮曾手工堆出 R1/R2/正式三个 1.5 GB 包 ≈3 GB）。关掉用 WHRHX_PRUNE_SAME_BATCH=0。
# ================================================================
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${WHRHX_BASE_APK:-$NIXIANG/06_dumped/无悔华夏/current_20260923/pkg/base_installed_20260923.apk}"
BASE_SHA_EXPECT="${WHRHX_BASE_SHA:-0b6c4772e64a6753b3a3169c9aea3a2e9af435c6361746c83b13516ba3f03167}"
SO="${WHRHX_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_whrhx/arm64-v8a/libstarry_magic.so}"
DEX="${WHRHX_MENU_DEX:-$NIXIANG/07_patched/无悔华夏/menu_latest_20260923/classes2.dex}"
DEX_SHA_EXPECT="${WHRHX_MENU_DEX_SHA:-ef84725fc724c7027775d7876ed0fc94058244cf7bfe737a1eaf08117fd54579}"
OUT_DIR="$NIXIANG/08_output/无悔华夏"
OUT_APK="${WHRHX_OUT_APK:-$OUT_DIR/无悔华夏_3.8.280_新版菜单_20260923-releaseSigned.apk}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_ENTRY="lib/arm64-v8a/libstarry_magic.so"
DEX_ENTRY="classes2.dex"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing mod so: $SO" >&2; exit 1; }
[[ -f "$DEX" ]] || { echo "ERROR: missing menu dex: $DEX" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing signing key: $KS" >&2; exit 1; }
[[ ! -e "$OUT_APK" ]] || { echo "ERROR: output exists, remove first (disk): $OUT_APK" >&2; exit 1; }

BASE_SHA="$(shasum -a 256 "$BASE_APK" | awk '{print $1}')"
[[ "$BASE_SHA" == "$BASE_SHA_EXPECT" ]] || {
  echo "ERROR: unexpected base apk sha256=$BASE_SHA (want $BASE_SHA_EXPECT)" >&2
  exit 1
}

# --- 构建戳门禁：本轮 OTA 守卫修复要求 so 与 dex 都带 STARRYSTAMP1 戳 ---
# （守卫改成比对 so 内戳，无戳的 so 会被判为「比包内旧」而每次冷启动清掉 OTA；
#   旧包复现可 WHRHX_REQUIRE_STAMP=0 关掉）
if [[ "${WHRHX_REQUIRE_STAMP:-1}" == "1" ]]; then
  LC_ALL=C grep -a -q "STARRYSTAMP1:EPOCH=" "$SO" || {
    echo "ERROR: mod so missing STARRYSTAMP1 build stamp（OTA 守卫会反复清补丁）" >&2; exit 1; }
  LC_ALL=C grep -a -q "STARRYSTAMP1:EPOCH=" "$DEX" || {
    echo "ERROR: menu dex missing OtaSoStamp stamp literal" >&2; exit 1; }
  echo "[menu_latest] build stamp = $(LC_ALL=C grep -a -o 'STARRYSTAMP1:EPOCH=[0-9]*;' "$SO" | head -1)"
fi

# --- so 标签门禁：本轮科研真门 hook 标记必须存在 ---
# 门禁只许要求「代码真的会发出的字面量」：类名与方法名在 il2cpp 里是两条独立字面量，
# 拼接串「PolicyDataMgr.GetPolicyItemShowState」只在已删除的手写 kExpect tag 里存在过
# （曾导致 p0 出包被误判失败，见 10_docs/无悔华夏/台账.md 20261011 追加节）。
for tag in "[Whrhx]" "policy panel gate ready" "PolicyDataMgr" "GetPolicyItemShowState" "科研立即完成"; do
  LC_ALL=C grep -a -q "$tag" "$SO" || { echo "ERROR: mod so missing tag: $tag" >&2; exit 1; }
done

# --- 菜单 dex 门禁：指纹 + 外科补丁 + 入口类 ---
DEX_SHA="$(shasum -a 256 "$DEX" | awk '{print $1}')"
[[ "$DEX_SHA" == "$DEX_SHA_EXPECT" ]] || {
  echo "ERROR: unexpected menu dex sha256=$DEX_SHA (want $DEX_SHA_EXPECT)" >&2
  exit 1
}
LC_ALL=C grep -a -q "Lcom/netease/nis/wrapper/MyApplication;" "$DEX" || {
  echo "ERROR: menu dex missing NIS-shell StarryApp super patch" >&2; exit 1; }
for cls in "Lcom/starry/magic/StarryApp;" "Lcom/starry/magic/menu/MenuService;" \
           "Lcom/starry/magic/menu/NativeMenuController;" "Lcom/starry/magic/menu/MagicRes;" \
           "Lcom/starry/magic/bridge/ModBridge;"; do
  LC_ALL=C grep -a -q "$cls" "$DEX" || { echo "ERROR: menu dex missing class: $cls" >&2; exit 1; }
done

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: apksigner not found" >&2; exit 1; }

WORK="$(mktemp -d /tmp/whrhx_menulatest.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[menu_latest] base   sha256 = $BASE_SHA"
echo "[menu_latest] so     sha256 = $(shasum -a 256 "$SO" | awk '{print $1}')  md5=$(md5 -q "$SO")"
echo "[menu_latest] menudex sha256 = $DEX_SHA  size=$(stat -f %z "$DEX")"
echo "[menu_latest] base 内 旧 dex sha256 = $(unzip -p "$BASE_APK" "$DEX_ENTRY" | shasum -a 256 | awk '{print $1}')"
echo "[menu_latest] base 内 旧 so  sha256 = $(unzip -p "$BASE_APK" "$SO_ENTRY" | shasum -a 256 | awk '{print $1}')"

cp -f "$BASE_APK" "$WORK/base.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
cp -f "$DEX" "$WORK/classes2.dex"
(
  cd "$WORK"
  zip -q -u base.apk "$SO_ENTRY" "$DEX_ENTRY"
)

python3.11 - "$WORK/base.apk" <<'PY'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as b:
    infos = b.infolist()
    names = [i.filename for i in infos]
    if len(names) != len(set(names)):
        raise SystemExit("ERROR: duplicate zip entries after update")
    bad = [i.filename for i in infos if i.compress_type not in (0, 8)]
    if bad:
        raise SystemExit(f"ERROR: unexpected compress methods: {bad[:5]}")
print(f"zip entries after update: {len(names)}")
PY

python3.11 - "$BASE_APK" "$WORK/base.apk" <<'PY'
import sys, zipfile
src, dst = sys.argv[1], sys.argv[2]
targets = {"lib/arm64-v8a/libstarry_magic.so", "classes2.dex"}
with zipfile.ZipFile(src) as a, zipfile.ZipFile(dst) as b:
    names_a, names_b = a.namelist(), b.namelist()
    if names_a != names_b:
        raise SystemExit("ERROR: zip entry list/order changed")
    ma = {i.filename: i.compress_type for i in a.infolist()}
    mb = {i.filename: i.compress_type for i in b.infolist()}
    changed, meta = [], []
    for name in names_a:
        ia, ib = a.getinfo(name), b.getinfo(name)
        if ia.CRC != ib.CRC or ia.file_size != ib.file_size:
            changed.append(name)
        if ia.compress_type != ib.compress_type:
            meta.append(name)
    if set(changed) != targets:
        raise SystemExit(f"ERROR: changed entries unexpected: {changed}")
    if meta:
        raise SystemExit(f"ERROR: compress method drifted: {meta}")
print("zip entry audit: only classes2.dex + lib/arm64-v8a/libstarry_magic.so changed")
PY

"$BT/zipalign" -f -p 4 "$WORK/base.apk" "$WORK/aligned.apk"
rm -f "$WORK/base.apk"

mkdir -p "$OUT_DIR"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT_APK" "$WORK/aligned.apk"
rm -f "$WORK/aligned.apk"

"$BT/apksigner" verify --verbose --print-certs "$OUT_APK" > "$WORK/verify.txt"
LC_ALL=C grep -q "CN=StarryMagic" "$WORK/verify.txt" || {
  echo "ERROR: signed apk does not use StarryMagic certificate" >&2; cat "$WORK/verify.txt" >&2; exit 1; }
LC_ALL=C grep -q "SHA-1 digest: 821c92e404ab5fdac8dbf157c2e467e71440917d" "$WORK/verify.txt" || {
  echo "ERROR: unexpected signer SHA-1" >&2; cat "$WORK/verify.txt" >&2; exit 1; }

# --- 产物复核：两个目标条目 == 输入；其余关键条目与底包逐字节一致 ---
unzip -p "$OUT_APK" "$SO_ENTRY" > "$WORK/final.so"
cmp -s "$SO" "$WORK/final.so" || { echo "ERROR: packaged so differs from build output" >&2; exit 1; }
unzip -p "$OUT_APK" "$DEX_ENTRY" > "$WORK/final.dex"
cmp -s "$DEX" "$WORK/final.dex" || { echo "ERROR: packaged dex differs from menu dex" >&2; exit 1; }
for d in classes.dex resources.arsc AndroidManifest.xml lib/arm64-v8a/libc++_shared.so assets/certs/starlight.crt; do
  unzip -p "$BASE_APK" "$d" > "$WORK/base_only" 2>/dev/null || { echo "ERROR: base missing $d" >&2; exit 1; }
  unzip -p "$OUT_APK"  "$d" > "$WORK/out_only"  2>/dev/null || { echo "ERROR: out missing $d" >&2; exit 1; }
  cmp -s "$WORK/base_only" "$WORK/out_only" || { echo "ERROR: $d changed" >&2; exit 1; }
done

python3.11 - "$BASE_APK" "$OUT_APK" <<'PY'
import sys, zipfile
def sig(n):
    u = n.upper()
    return n.startswith("META-INF/") and u.endswith((".SF", ".RSA", ".DSA", ".MF"))
with zipfile.ZipFile(sys.argv[1]) as a, zipfile.ZipFile(sys.argv[2]) as b:
    na, nb = a.namelist(), b.namelist()
    if [n for n in na if not sig(n)] != [n for n in nb if not sig(n)]:
        raise SystemExit("ERROR: final apk non-signature entry list changed")
    delta = sorted(set(nb) - set(na)), sorted(set(na) - set(nb))
    print(f"final apk entries ok; META-INF signature delta: +{delta[0]} -{delta[1]}")
PY

# --- dex 内嵌复核（baksmali 侧确认补丁真的进了包）---
if [[ -x "$NIXIANG/01_tools/smali/baksmali" ]]; then
  "$NIXIANG/01_tools/smali/baksmali" list classes "$WORK/final.dex" > "$WORK/final_classes.txt"
  echo "[menu_latest] 包内 dex 类数 = $(wc -l < "$WORK/final_classes.txt" | tr -d ' ')"
  LC_ALL=C grep -q "Lcom/starry/magic/menu/MagicRes;" "$WORK/final_classes.txt" || {
    echo "ERROR: packaged dex lost MagicRes" >&2; exit 1; }
fi

# --- P1-⑦ 同批产物清理（WHRHX_PRUNE_SAME_BATCH，默认 1=开）：只留本轮正式包 ---
# 依据（2026-10-11 复盘账）：收尾时 08_output/无悔华夏/ 里堆着 R1/R2/正式三个 1.5 GB 包
# （≈3 GB）纯粹是人工忘删 —— 脚本既然是唯一产出点，就该由它保证「同批只留一个」。
# 安全边界三条缺一不可：
#   ① 只在 $OUT_DIR 内操作；② 只删「同一 8 位日期 + -releaseSigned.apk[.idsig]」；
#   ③ 永不删本轮 $OUT_APK 自己（显式排除，删完再断言它还在）。
if [[ "${WHRHX_PRUNE_SAME_BATCH:-1}" = "1" ]]; then
  OUT_NAME="$(basename "$OUT_APK")"
  # 注意：BSD sed 的 BRE 不支持 \{8\}（实测静默不匹配 ⇒ 日期取空 ⇒ 一路跳过）⇒ 一律用 -E
  BATCH_DATE="$(printf '%s' "$OUT_NAME" | sed -nE 's/.*_([0-9]{8})-releaseSigned\.apk$/\1/p')"
  if [[ -z "$BATCH_DATE" ]]; then
    echo "[menu_latest] prune 跳过：产物名里没有 8 位日期（${OUT_NAME}）"
  else
    shopt -s nullglob
    pruned=0; freed=0
    for f in "$OUT_DIR"/无悔华夏_*_"$BATCH_DATE"-releaseSigned.apk \
             "$OUT_DIR"/无悔华夏_*_"$BATCH_DATE"-releaseSigned.apk.idsig; do
      [[ "$f" = "$OUT_APK" ]] && continue
      [[ "$f" = "$OUT_APK.idsig" ]] && continue
      sz="$(stat -f %z "$f")"
      rm -f -- "$f"
      pruned=$((pruned + 1)); freed=$((freed + sz))
      echo "[menu_latest] prune 删除同批旧产物：$(basename "$f")（${sz} B）"
    done
    shopt -u nullglob
    [[ -f "$OUT_APK" ]] || { echo "ERROR: prune 把本轮产物删没了" >&2; exit 1; }
    echo "[menu_latest] prune 同批日期=${BATCH_DATE} 删除 ${pruned} 个文件，回收 $((freed / 1048576)) MiB（当前目录只剩本轮产物）"
    ls -la "$OUT_DIR" | sed 's/^/    /'
  fi
fi

echo "[menu_latest] done -> $OUT_APK"
echo "[menu_latest] apk size   = $(stat -f %z "$OUT_APK")"
echo "[menu_latest] apk sha256 = $(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
echo "[menu_latest] apk md5    = $(md5 -q "$OUT_APK")"
