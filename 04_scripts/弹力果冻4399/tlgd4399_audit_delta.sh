#!/usr/bin/env bash
# 弹力果冻 **4399 渠道** — 产物外科增量审计（L1 静态验收）
#
# 目的：逐条目比对「底包 vs 最终产物」，证明**只有预期的条目变了**，其余字节逐一相同。
#   预期变化：
#     classes2.dex                       —— 换成新构建菜单 dex
#     lib/arm64-v8a/libstarry_magic.so   —— 换成新构建稳定整改 so
#     lib/arm64-v8a/libc++_shared.so     —— 以新构建为准（可能相同）
#     assets/certs/*                     —— 以新构建为准（可能相同）
#     META-INF/*                         —— 重签名必然变（名字相同）
#   预期不变：AndroidManifest.xml 必须逐字节保留（除非本轮明确改 Manifest）
#   硬约束：classes.dex / resources.arsc 逐字节相同；dex 布局恰为 classes.dex + classes2.dex
#   任何其它条目的增删/变更 = 审计失败。
#
# 速度：先用 zip 中央目录的 (size, CRC-32) 快筛；仅当二者不同才真正解压算 sha256。
#
# 用法: bash 04_scripts/弹力果冻4399/tlgd4399_audit_delta.sh [底包] [产物]
set -euo pipefail

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
BASE="${1:-$NIXIANG/02_apks/弹力果冻4399/弹力果冻4399_2.4.0_vc4412_device_base.apk}"
OUT="${2:-$NIXIANG/08_output/弹力果冻4399/弹力果冻4399_2.4.0_稳定整改_20261007-releaseSigned.apk}"
[[ -f "$BASE" ]] || { echo "ERROR: 底包不存在 $BASE"; exit 1; }
[[ -f "$OUT" ]] || { echo "ERROR: 产物不存在 $OUT"; exit 1; }

python3.11 - "$BASE" "$OUT" <<'PY'
import sys, zipfile, hashlib

base_p, out_p = sys.argv[1], sys.argv[2]
zb, zo = zipfile.ZipFile(base_p), zipfile.ZipFile(out_p)
nb = {i.filename: i for i in zb.infolist()}
no = {i.filename: i for i in zo.infolist()}

ALLOW_DIFF = {
    "classes2.dex",
    "lib/arm64-v8a/libstarry_magic.so",
    "lib/arm64-v8a/libc++_shared.so",
}
# META-INF/* 是签名块：底包（上一轮星光签名）与产物（本轮重签）必然不同，名字恰好相同
# （都是 META-INF/STARRY.RSA + STARRY.SF + MANIFEST.MF）→ 计入预期变更。
ALLOW_DIFF_PREFIX = ("assets/certs/", "META-INF/")

def h(x, n=16):
    return x[:n] + "…"

# 1) 消失的条目
gone = sorted(set(nb) - set(no))
bad_gone = [g for g in gone if not g.startswith("META-INF/")]
print(f"[gone] {len(gone)} 条：{gone}")
if bad_gone:
    print(f"  ✗ 非预期消失：{bad_gone}")
    sys.exit(1)

# 2) 新增的条目
added = sorted(set(no) - set(nb))
bad_added = [a for a in added if not a.startswith("META-INF/")]
print(f"[added] {len(added)} 条：{added}")
if bad_added:
    print(f"  ✗ 非预期新增：{bad_added}")
    sys.exit(1)

# 3) 共同条目比对：先 (size, CRC) 快筛，不同才解压算 sha256
SAME = DIFF = 0
unexpected = []
expected = []
for name in sorted(set(nb) & set(no)):
    ib, io = nb[name], no[name]
    same_quick = (ib.file_size == io.file_size and ib.CRC == io.CRC)
    if same_quick:
        SAME += 1
        continue
    DIFF += 1
    hb = hashlib.sha256(zb.read(name)).hexdigest()
    ho = hashlib.sha256(zo.read(name)).hexdigest()
    if hb == ho:
        # size/CRC 标记不同但内容其实相同（极少见，例如 STORED 与 DEFLATE 差异）
        SAME += 1
        DIFF -= 1
        continue
    allowed = name in ALLOW_DIFF or any(name.startswith(p) for p in ALLOW_DIFF_PREFIX)
    if allowed:
        expected.append(f"{name}: {h(hb)} -> {h(ho)}  [{ib.file_size} B -> {io.file_size} B]")
    else:
        unexpected.append(f"{name}: {h(hb)} -> {h(ho)}")

for line in expected:
    print(f"  [预期变更] {line}")
print(f"[same] 相同 {SAME} 条；[diff] 变更 {DIFF} 条（非预期 {len(unexpected)} 条）")

# 4) 硬约束
for prot in ("classes.dex", "resources.arsc"):
    assert prot in nb and prot in no, f"缺受保护条目 {prot}"
    hb = hashlib.sha256(zb.read(prot)).hexdigest()
    ho = hashlib.sha256(zo.read(prot)).hexdigest()
    assert hb == ho, f"✗ 受保护条目被改动：{prot}"
    print(f"[protected] {prot} 逐字节相同 ✓ ({h(hb)}, {nb[prot].file_size} B)")

# AndroidManifest.xml 默认必须逐字节保留（本轮不改 Manifest）
if "AndroidManifest.xml" in nb and "AndroidManifest.xml" in no:
    hb = hashlib.sha256(zb.read("AndroidManifest.xml")).hexdigest()
    ho = hashlib.sha256(zo.read("AndroidManifest.xml")).hexdigest()
    if hb != ho:
        unexpected.append("AndroidManifest.xml: 本轮声明不改 Manifest，但字节变了")
    else:
        print(f"[protected] AndroidManifest.xml 逐字节相同 ✓ ({h(hb)}, {nb['AndroidManifest.xml'].file_size} B)")

names_o = set(no)
assert "classes2.dex" in names_o, "缺 classes2.dex"
dexs = sorted(n for n in names_o if n.endswith(".dex"))
assert dexs == ["classes.dex", "classes2.dex"], f"dex 布局异常 {dexs}"
assert "lib/arm64-v8a/libstarry_magic.so" in names_o
# 产物必须是「已重签」状态：META-INF 里要有签名块
sig = sorted(n for n in names_o if n.startswith("META-INF/") and n.endswith((".RSA", ".DSA", ".EC", ".SF")))
assert sig, "✗ 产物 META-INF 里没有签名块（未签名？）"
print(f"[signed] META-INF 签名块：{sig}")

if unexpected:
    print("✗ 存在非预期差异：")
    for line in unexpected:
        print(f"   {line}")
    sys.exit(1)

print(f"✓ 审计通过：共 {len(names_o)} 条目，仅 classes2.dex / libstarry_magic.so / libc++_shared.so "
      f"(+assets/certs) 按预期变更；classes.dex 与 resources.arsc 逐字节保留")
PY
