#!/usr/bin/env bash
# 异世界勇者 8.1.1 — 外科注入（增量置换模式，pipeline 阶段 surgical_so_swap）
#
# 背景：AGENTS.md 里的 02_apks/异世界勇者/ 与 08_output/异世界勇者/ 已由用户手动清空，
#       scripts/surgical_ysjyz_8.1.1.sh 硬依赖官包 md5 710d1cfe…，无法直接复跑。
#       因此以「设备上现装的 v5 正式包」为底做**增量置换**：
#         · v6：只换 lib/arm64-v8a/libstarry_magic.so（伤害修复全在这个 so 的 JS 补丁里）
#         · v7：再换 classes3.dex（mod 菜单框架换新构建，用户要求）
#       其余条目（classes.dex / classes2.dex / Manifest / certs / libc++_shared.so /
#       index.jsc…）一律逐条目 size+CRC 与底包全等 —— 差异比全量外科更小、可逐条核验。
#
# 输入：07_patched/异世界勇者/base_v5_from_device.apk（设备 /data/app/.../base.apk，v5 指纹）
#       mod菜单/starry-mod-menu/libs_ysjyz/arm64-v8a/libstarry_magic.so（build_so 构建）
#       07_patched/异世界勇者/classes3_menu.dex（build_menu_dex 构建；设 YSJYZ_SKIP_CLASSES3=1 可跳过）
# 输出：08_output/异世界勇者/异世界勇者_8.1.1_surgical-unsigned.apk（交给 ysjyz_sign.sh）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
OUT="$NIXIANG/08_output/异世界勇者"
BASE="${YSJYZ_BASE_APK:-$NIXIANG/07_patched/异世界勇者/base_v5_from_device.apk}"
SO="$NIXIANG/mod菜单/starry-mod-menu/libs_ysjyz/arm64-v8a/libstarry_magic.so"
C3="${YSJYZ_CLASSES3_DEX:-$NIXIANG/07_patched/异世界勇者/classes3_menu.dex}"
UNSIGNED="$OUT/异世界勇者_8.1.1_surgical-unsigned.apk"
# v5 内容相同、只是外科重跑过两次 ⇒ 两个合法 md5（MuMu 装机版 / 小米装机版，见
# 10_docs/异世界勇者/星空落地_ysjyz_8.1.1_20260911.md:221-222,336）。本机真机装的是 680a3d2c…。
V5_MD5S="680a3d2c0e67175149c2ecdc8a8df1a9 94bdd89714236311496c09bd352f0bf6"
V5_SO_MD5="54cc06236f7d04bbf070131f60979b22"       # 底包内 so 必须仍是 v5 构建
V5_C3_MD5="916bdd497364a90f26d0d42fc99532a0"       # 底包内 classes3.dex 必须仍是 v5 框架
SO_PATH="lib/arm64-v8a/libstarry_magic.so"
C3_PATH="classes3.dex"

[[ -f "$BASE" ]] || { echo "ERROR: 缺底包 ${BASE}（先 adb exec-out su -c 'cat <base.apk>' 拉回并核对 md5）" >&2; exit 1; }
[[ -f "$SO" ]]   || { echo "ERROR: 缺 ${SO}（先跑 build_so 阶段）" >&2; exit 1; }

BMD5="$(md5 -q "$BASE")"
grep -q "$BMD5" <<<"${V5_MD5S// /$'\n'}" || { echo "ERROR: 底包 md5=$BMD5 不属于已知 v5 内容（${V5_MD5S}）" >&2; exit 1; }
# 内容判据（比整包 md5 更硬）：classes3.dex 在 且 包内 so 仍是 v5 构建
# 注意：禁止 `unzip -l | grep -q`（grep 提前退出发 SIGPIPE，pipefail 下整条管道返回 141 误报缺失）
BASE_LIST="$(mktemp /tmp/ysjyz_baselist.XXXXXX)"
unzip -l "$BASE" > "$BASE_LIST"
grep -c "classes3.dex" "$BASE_LIST" >/dev/null || { rm -f "$BASE_LIST"; echo "ERROR: 底包无 classes3.dex —— 这不是星空包（增量模式要求已注入的 v5 包）" >&2; exit 1; }
rm -f "$BASE_LIST"
INNER_SO_MD5="$(unzip -p "$BASE" "$SO_PATH" | md5 -q)"
[[ "$INNER_SO_MD5" == "$V5_SO_MD5" ]] || { echo "ERROR: 底包内 so md5=$INNER_SO_MD5 ≠ v5 构建 ${V5_SO_MD5}（底包不是 v5）" >&2; exit 1; }
INNER_C3_MD5="$(unzip -p "$BASE" "$C3_PATH" | md5 -q)"
[[ "$INNER_C3_MD5" == "$V5_C3_MD5" ]] || { echo "ERROR: 底包内 classes3.dex md5=$INNER_C3_MD5 ≠ v5 框架 ${V5_C3_MD5}（底包不是 v5）" >&2; exit 1; }
SMD5="$(md5 -q "$SO")"
echo "[swap] base=$BASE";  echo "[swap] base md5 = $BMD5"
echo "[swap] so   md5 = $SMD5  ($(stat -f%z "$SO") B)"

SWAP_C3=1
if [[ "${YSJYZ_SKIP_CLASSES3:-0}" == "1" ]]; then
  SWAP_C3=0
  echo "[swap] classes3.dex 跳过（YSJYZ_SKIP_CLASSES3=1）"
else
  [[ -f "$C3" ]] || { echo "ERROR: 缺 ${C3}（先跑 build_menu_dex 阶段，或设 YSJYZ_SKIP_CLASSES3=1）" >&2; exit 1; }
  CMD5="$(md5 -q "$C3")"
  [[ "$CMD5" != "$V5_C3_MD5" ]] || { echo "ERROR: classes3 构建产物与 v5 框架相同（${CMD5}），没有换新" >&2; exit 1; }
  echo "[swap] c3   md5 = $CMD5  ($(stat -f%z "$C3") B)"
fi

mkdir -p "$OUT"
WORK="$(mktemp -d /tmp/ysjyz_swap.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

# 1) 底包 → unsigned 工作副本，剥掉旧签名（v1 JAR 签名条目会让 apksigner 报 stale）
cp "$BASE" "$UNSIGNED"
zip -q -d "$UNSIGNED" 'META-INF/*.RSA' 'META-INF/*.SF' 'META-INF/*.MF' 2>/dev/null || true

# 2) 逐条替换（保留原压缩方式，别把 STORED 变成 DEFLATE）
swap_in() {
  local src="$1" path="$2"
  local method
  method="$(unzip -v "$BASE" "$path" | awk -v p="$path" '$NF==p {print $2}' | sed -n '1p')"
  local zipopt; [[ "$method" == "Stored" ]] && zipopt="-0" || zipopt="-9"
  echo "[swap] $path 压缩方式=${method:-Defl} → zip $zipopt"
  mkdir -p "$WORK/payload/$(dirname "$path")"
  cp "$src" "$WORK/payload/$path"
  # -D：不写目录条目（否则会多出 lib/arm64-v8a/libstarry_magic.so/ 这样的目录项，破坏"逐条目全等"核验）
  ( cd "$WORK/payload" && zip -q -D $zipopt "$UNSIGNED" "$path" )
  rm -f "$WORK/payload/$path"
}
swap_in "$SO" "$SO_PATH"
[[ "$SWAP_C3" == "1" ]] && swap_in "$C3" "$C3_PATH"

# 3) 逐条目核验：除替换过的条目与剥掉的旧签名外，**任何条目都不允许变**（size/crc 全等）
python3 - "$BASE" "$UNSIGNED" "$SO_PATH" "$SO" "$C3" "$SWAP_C3" <<'PY'
import sys, zipfile, hashlib
base, out, so_path, so_file, c3_file, swap_c3 = sys.argv[1:7]
allow = {so_path} | ({"classes3.dex"} if swap_c3 == "1" else set())
def table(p):
    with zipfile.ZipFile(p) as z:
        return {i.filename: (i.file_size, i.CRC, i.compress_type) for i in z.infolist()}
b, o = table(base), table(out)
gone = sorted(set(b) - set(o))
new = sorted(set(o) - set(b))
diff = [n for n in sorted(set(b) & set(o)) if b[n] != o[n]]
print(f"[swap] 消失条目 {len(gone)}: {gone}")
print(f"[swap] 新增条目 {len(new)}: {new}")
print(f"[swap] 内容变化条目 {len(diff)}: {diff}")
assert new == [], f"不该有新增条目: {new}"
assert set(diff) == allow, f"只允许 {sorted(allow)} 变化，实际: {diff}"
assert all(n.startswith("META-INF/") and n.upper().endswith((".RSA", ".SF", ".MF")) for n in gone), gone
with zipfile.ZipFile(out) as z:
    got = hashlib.md5(z.read(so_path)).hexdigest()
    want = hashlib.md5(open(so_file, "rb").read()).hexdigest()
    assert got == want, f"包内 so md5 {got} ≠ 构建产物 {want}"
    print(f"[swap] 包内 so md5 = {got} ✔")
    if swap_c3 == "1":
        g3 = hashlib.md5(z.read("classes3.dex")).hexdigest()
        w3 = hashlib.md5(open(c3_file, "rb").read()).hexdigest()
        assert g3 == w3, f"包内 classes3.dex md5 {g3} ≠ 构建产物 {w3}"
        print(f"[swap] 包内 classes3.dex md5 = {g3} ✔")
    print(f"[swap] 其余 {len(o) - len(allow)} 个条目逐一条目 size/CRC 与底包全等")
PY

# 4) 关键条目在（classes3.dex / classes2.dex / libc++_shared.so / certs 一个不能少）
unzip -l "$UNSIGNED" > "$WORK/entries.txt"
for e in classes.dex classes2.dex classes3.dex resources.arsc AndroidManifest.xml \
         lib/arm64-v8a/libstarry_magic.so lib/arm64-v8a/libc++_shared.so \
         assets/assets/main/index.jsc assets/src/settings.jsc; do
  grep -q " $e\$" "$WORK/entries.txt" || { echo "ERROR: 缺条目 $e" >&2; exit 1; }
done

echo "[swap] done → $UNSIGNED"
echo "[swap] md5 = $(md5 -q "$UNSIGNED")"
ls -lh "$UNSIGNED"
