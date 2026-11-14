#!/usr/bin/env bash
# 汉克旅店 1.1.0 — 星光底包 so-only 叠加出包（新增 ⑦移动速度 ⑧无限蓝）
#
# 背景（与 hkld_surgical_1.1.0.sh 的区别，别混用）：
#   hkld_surgical_1.1.0.sh 打的是**官方底包**（要合并 dex + 打 manifest 补丁 + 换 so）；
#   本脚本打的是**已带星光注入的真机底包**（manifest 的 StarryInitProvider、classes4.dex 的
#   com/starry 类都已就位）⇒ 只重建并替换 lib/arm64-v8a/libstarry_magic.so（内含新的 Cecil
#   补丁 DLL），dex 与 AndroidManifest.xml 必须逐字节不变，不新增 dex、不重复打 manifest 补丁。
#
# 底包：02_apks/汉克旅店/汉克旅店_1.1.0_starry_真机拉取.apk
#       md5 03d3ec2ad30f2472a43bb9bdc19d2dba（= 设备 /data/app 下 base.apk，传输无损）
# 产物：08_output/汉克旅店/汉克旅店_1.1.0_surgical-releaseSigned.apk
#
# 用法:
#   bash 04_scripts/汉克旅店/hkld_overlay_1.1.0.sh
#   HKLD_SKIP_SO_BUILD=1 bash ...        # 复用已构建的 libs_hkld/arm64-v8a/libstarry_magic.so
set -euo pipefail

VERSION="1.1.0"
PKG="com.chillyroom.hanksbar.yw"
BASE_MD5_EXPECT="03d3ec2ad30f2472a43bb9bdc19d2dba"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
BASE_APK="$NIXIANG/02_apks/汉克旅店/汉克旅店_${VERSION}_starry_真机拉取.apk"
OUT="$NIXIANG/08_output/汉克旅店"
SIGNED="$OUT/汉克旅店_${VERSION}_surgical-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
SO_BUILT="$STARRY_SRC/libs_hkld/arm64-v8a/libstarry_magic.so"
SO_REL="lib/arm64-v8a/libstarry_magic.so"

[[ -f "$BASE_APK" ]] || { echo "ERROR: 缺星光底包 $BASE_APK"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: 缺签名库 $KS"; exit 1; }

# shellcheck source=/dev/null
[[ -f "$NIXIANG/04_scripts/_common/env.sh" ]] && source "$NIXIANG/04_scripts/_common/env.sh"
ZIPALIGN="${NIXIANG_ZIPALIGN:-}"
APKSIGNER="${NIXIANG_APKSIGNER:-}"
if [[ -z "$ZIPALIGN" || ! -f "$ZIPALIGN" ]]; then
  for v in 36.0.0 35.0.0 34.0.0; do
    d="$HOME/Library/Android/sdk/build-tools/$v"
    if [[ -f "$d/zipalign" ]]; then ZIPALIGN="$d/zipalign"; APKSIGNER="$d/apksigner"; break; fi
  done
fi
[[ -f "$ZIPALIGN" ]] || { echo "ERROR: zipalign 不存在"; exit 1; }
[[ -f "$APKSIGNER" ]] || { echo "ERROR: apksigner 不存在"; exit 1; }

LOCK_DIR="/tmp/hkld_overlay.lockdir"
mkdir "$LOCK_DIR" 2>/dev/null || { echo "ERROR: lock $LOCK_DIR held"; exit 1; }
WORK="$(mktemp -d /tmp/hkld_overlay.XXXXXX)"
trap 'rm -rf "$WORK"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT"

echo "[0/5] 底包身份断言（md5 == 设备 base.apk，禁换错底包）"
BASE_MD5="$(md5 -q "$BASE_APK")"
echo "  base: $BASE_APK"
echo "  md5 = $BASE_MD5  size=$(stat -f%z "$BASE_APK") B"
[[ "$BASE_MD5" == "$BASE_MD5_EXPECT" ]] || { echo "ERROR: 底包 md5=$BASE_MD5 ≠ 期望 $BASE_MD5_EXPECT"; exit 1; }

echo "[1/5] so 自检（星光注入后重新构建，含 8 个菜单串）"
if [[ "${HKLD_SKIP_SO_BUILD:-0}" != "1" ]]; then
  bash "$SCRIPT_DIR/build_hkld_so.sh" | tail -3
fi
[[ -f "$SO_BUILT" ]] || { echo "ERROR: 缺 ${SO_BUILT}（先跑 build_hkld_so.sh）"; exit 1; }
LC_ALL=C grep -a -q '\[Hkld\]' "$SO_BUILT" || { echo "ERROR: so 无 [Hkld] 标签"; exit 1; }
for s in 无敌 伤害倍率 无限技能 解锁角色 无限金币 无限钥匙 移动速度 无限蓝; do
  LC_ALL=C grep -a -q "$s" "$SO_BUILT" || { echo "ERROR: so 缺菜单串 $s"; exit 1; }
done
for s in move_speed_x inf_mp; do
  LC_ALL=C grep -a -q "$s" "$SO_BUILT" || { echo "ERROR: so 缺配置键串 $s"; exit 1; }
done
echo "  so ok: md5=$(md5 -q "$SO_BUILT") size=$(stat -f%z "$SO_BUILT") B（8 个菜单串 + move_speed_x/inf_mp 键齐备）"

echo "[2/5] 外科 zip 组装（只替换 ${SO_REL}，其余条目原样搬运）"
python3.11 - "$BASE_APK" "$SO_BUILT" "$WORK/unsigned.apk" "$SO_REL" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
base, so, dst, so_rel = sys.argv[1], Path(sys.argv[2]), sys.argv[3], sys.argv[4]
so_data = so.read_bytes()

with zipfile.ZipFile(base, "r") as zin:
    names = zin.namelist()
    assert so_rel in names, "底包里没有 " + so_rel
    old_so = zin.read(so_rel)
    assert b"[Zjmc]" not in old_so and b"[Wxsj" not in old_so, "底包 so 不是 Hkld"
    dexes = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))
    print("  底包 dex 布局:", dexes, "（本次不新增、不修改）")
    keep_sha = {}
    for n in names:
        if n.startswith("META-INF/") or n == so_rel:
            continue
        keep_sha[n] = hashlib.sha256(zin.read(n)).hexdigest()
    man_sha = keep_sha.get("AndroidManifest.xml")

    with zipfile.ZipFile(dst, "w") as zout:
        for item in zin.infolist():
            name = item.filename
            if name.startswith("META-INF/"):
                continue          # 旧签名由 apksigner 重做
            data = so_data if name == so_rel else zin.read(name)
            info = zipfile.ZipInfo(name, item.date_time)
            if name == "resources.arsc":
                info.compress_type = zipfile.ZIP_STORED
            elif name == so_rel:
                info.compress_type = zipfile.ZIP_DEFLATED
            else:
                info.compress_type = item.compress_type
            info.external_attr = item.external_attr
            info.create_system = 3
            zout.writestr(info, data)

with zipfile.ZipFile(dst, "r") as z2:
    out_names = z2.namelist()
    assert not any(n.startswith("META-INF/") and n.upper().endswith((".RSA", ".SF", ".MF")) for n in out_names), "残留旧签名"
    diff = []
    for n, h in keep_sha.items():
        if n not in out_names:
            diff.append(n + " (丢失)"); continue
        if hashlib.sha256(z2.read(n)).hexdigest() != h:
            diff.append(n + " (内容变了)")
    extra = [n for n in out_names if n not in keep_sha and n != so_rel]
    assert not diff, "以下条目与底包不一致：" + ", ".join(diff[:5])
    assert not extra, "出现底包没有的新条目：" + ", ".join(extra[:5])
    assert sorted(n for n in out_names if n.startswith("classes") and n.endswith(".dex")) == dexes, "dex 布局变化"
    assert hashlib.sha256(z2.read("AndroidManifest.xml")).hexdigest() == man_sha, "AndroidManifest.xml 被改动"
    new_so = z2.read(so_rel)
    assert new_so == so_data, "so 未按预期替换"
    print("  OK 除 %s 外 %d 个条目 sha256 与底包逐字节一致（含 4 个 dex + AndroidManifest.xml + resources.arsc）"
          % (so_rel, len(keep_sha)))
    print("  OK 无新条目、无新 dex；新 so %d B md5=%s" % (len(new_so), hashlib.md5(new_so).hexdigest()))
PY

echo "[3/5] zipalign + apksigner（v1/v2/v3，覆盖安装同签名）"
"$ZIPALIGN" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
rm -f "$WORK/unsigned.apk"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
rm -f "$WORK/aligned.apk" 2>/dev/null || true
rm -f "$OUT"/*.idsig 2>/dev/null || true

echo "[4/5] 产物自检（签名 + so + 星光入口 + dex 不变）"
"$APKSIGNER" verify --print-certs "$SIGNED" 2>&1 | head -4
python3.11 - "$SIGNED" "$BASE_APK" "$SO_REL" <<'PY'
import hashlib, sys, zipfile
signed, base, so_rel = sys.argv[1], sys.argv[2], sys.argv[3]
with zipfile.ZipFile(signed) as z, zipfile.ZipFile(base) as zb:
    so = z.read(so_rel)
    assert b"[Hkld]" in so, "so 不是 Hkld"
    for s in ("无敌", "移动速度", "无限蓝"):
        assert s.encode() in so, "so 缺菜单串 " + s
    mf = z.read("AndroidManifest.xml")
    for s in ("com.starry.magic.StarryInitProvider", "com.chillyroom.hanksbar.yw.starryinit"):
        assert s.encode("utf-16-le") in mf, "Manifest 缺 " + s
    for n in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex"):
        assert hashlib.sha256(z.read(n)).hexdigest() == hashlib.sha256(zb.read(n)).hexdigest(), n + " 被改动"
    print("  signed ok：so 为 [Hkld]（含 移动速度/无限蓝）；Manifest 星光入口齐；4 个 dex 与底包逐字节一致")
PY

echo "[5/5] 产物指纹"
python3.11 - "$SIGNED" "$BASE_APK" "$SO_REL" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
signed, base, so_rel = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3]
d = signed.read_bytes()
with zipfile.ZipFile(signed) as z, zipfile.ZipFile(base) as zb:
    so, sob = z.read(so_rel), zb.read(so_rel)
print("  APK   : %s" % signed)
print("          size=%d B  md5=%s  sha256=%s" % (len(d), hashlib.md5(d).hexdigest(), hashlib.sha256(d).hexdigest()))
print("  so    : size=%d B  md5=%s（底包 so md5=%s）" % (len(so), hashlib.md5(so).hexdigest(), hashlib.md5(sob).hexdigest()))
print("  底包  : md5=%s size=%d B" % (hashlib.md5(base.read_bytes()).hexdigest(), base.stat().st_size))
PY
ls -lh "$SIGNED"
echo "DONE $SIGNED"
