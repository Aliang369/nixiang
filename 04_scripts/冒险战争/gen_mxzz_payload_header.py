#!/usr/bin/env python3
"""冒险战争 (mxzz) —— 生成 jni/Games/MxzzPayload.h（载荷表：官方 sha -> 内嵌补丁体）

为什么要有这一步：
  · native 侧只认「官方程序集 sha256 白名单」，白名单与补丁体长度/sha 必须由**实际产物**写入，
    不许手填、不许猜（用户 2026-11-19 硬要求：静态结论 + 反推验证后才动手）。
  · 生成物包含补丁体长度，ndk-build 前必须存在；缺失或与 dll 不符即中止。

用法：
  python 04_scripts/冒险战争/gen_mxzz_payload_header.py            # 用默认 spec
  python 04_scripts/冒险战争/gen_mxzz_payload_header.py --spec <json> --out <h>
  python 04_scripts/冒险战争/gen_mxzz_payload_header.py --check    # 只校验，不写盘（退出码即结论）

纪律：本脚本不修改任何 dll，只读 + 写头文件 + 写报告。
"""
import argparse
import hashlib
import json
import os
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DEFAULT_SPEC = os.path.join(ROOT, "04_scripts", "冒险战争", "mxzz_payload_spec.json")
DEFAULT_OUT = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni", "Games", "MxzzPayload.h")
REPORT = os.path.join(ROOT, "07_patched", "冒险战争", "payload_report.json")


def sha256_of(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for blk in iter(lambda: f.read(1 << 20), b""):
            h.update(blk)
    return h.hexdigest()


def fail(msg):
    sys.stderr.write("ERROR: " + msg + "\n")
    sys.exit(1)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--spec", default=DEFAULT_SPEC)
    ap.add_argument("--out", default=DEFAULT_OUT)
    ap.add_argument("--check", action="store_true", help="只校验不写盘")
    args = ap.parse_args()

    if not os.path.isfile(args.spec):
        fail("缺 spec: " + args.spec)
    spec = json.load(open(args.spec, "r", encoding="utf-8"))
    entries = spec.get("entries") or []
    if not entries:
        fail("spec.entries 为空 —— 载荷表不得为空（空表 = 只会 PASSTHROUGH）")

    resolved = []
    seen_official = set()
    for e in entries:
        role = e.get("role") or "?"
        off_sha = (e.get("official_sha256") or "").lower()
        if len(off_sha) != 64:
            fail("%s: official_sha256 不是 64 位 hex" % role)
        if off_sha in seen_official:
            fail("%s: official_sha256 重复" % role)
        seen_official.add(off_sha)
        pdl = e.get("patched_dll")
        path = pdl if os.path.isabs(pdl) else os.path.join(ROOT, pdl)
        if not os.path.isfile(path):
            fail("%s: 补丁体不存在 %s（先跑 ilpatch）" % (role, path))
        size = os.path.getsize(path)
        psha = sha256_of(path)
        if psha == off_sha:
            fail("%s: 补丁体与官方逐字节相同（未打补丁），拒绝生成载荷表" % role)
        if size < 1024:
            fail("%s: 补丁体过小 %d B" % (role, size))
        blob = e.get("blob") or ("mxzz_%s.bin" % role)
        if not blob.endswith(".bin"):
            fail("%s: blob 名必须以 .bin 结尾（objcopy 符号名由文件名决定）" % role)
        resolved.append({
            "role": role, "official_sha256": off_sha,
            "official_size": e.get("official_size"),
            "patched_dll": os.path.relpath(path, ROOT).replace("\\", "/"),
            "patched_size": size, "patched_sha256": psha,
            # objcopy 的符号名 = 输入文件名里非字母数字字符替换为下划线（mxzz_dev.bin → _binary_mxzz_dev_bin_start）
            "symbol": "_binary_%s_start" % blob.replace(".", "_").replace("-", "_"),
            "blob": blob,
        })
        print("[ok] %-5s 官方=%s.. %s B  →  补丁=%s.. %d B  符号=%s"
              % (role, off_sha[:12], e.get("official_size"), psha[:12], size,
                 "_binary_%s_start" % blob.replace(".", "_").replace("-", "_")))

    report = {"game": spec.get("game"), "game_id": spec.get("game_id"),
              "package": spec.get("package"), "policy": spec.get("policy"),
              "entries": resolved}
    if args.check:
        print("[check] 载荷表可生成（未写盘）")
        return 0

    lines = []
    lines.append("/* ================================================================")
    lines.append(" *  MxzzPayload.h —— 【生成物·勿手改】")
    lines.append(" *  生成者：04_scripts/冒险战争/gen_mxzz_payload_header.py")
    lines.append(" *  生成依据：%s" % os.path.relpath(args.spec, ROOT).replace("\\", "/"))
    lines.append(" *")
    lines.append(" *  载荷表语义：官方 sha256 → 同版本的 Cecil 补丁体（dev 优先 / APK 基线兜底）。")
    lines.append(" *  运行期不在表里的 sha 一律 PASSTHROUGH（不崩、不闪退）。")
    lines.append(" * ================================================================ */")
    lines.append("#ifndef STARRY_MXZZ_PAYLOAD_H")
    lines.append("#define STARRY_MXZZ_PAYLOAD_H")
    lines.append("")
    for r in resolved:
        lines.append("extern \"C\" const unsigned char %s[];" % r["symbol"])
    lines.append("")
    lines.append("struct MxzzPayloadEntry {")
    lines.append("    const char* sha;            // 官方程序集 sha256（小写 hex）")
    lines.append("    const unsigned char* data;  // 补丁体（objcopy 内嵌）")
    lines.append("    unsigned len;               // 补丁体长度（托管堆按此重新分配容器）")
    lines.append("};")
    lines.append("")
    lines.append("#define MXZZ_PAYLOAD_GENERATED 1")
    lines.append("#define MXZZ_PAYLOAD_POLICY \"%s\"" % (spec.get("policy") or "custom"))
    lines.append("#define MXZZ_PAYLOAD_COUNT %d" % len(resolved))
    lines.append("static const MxzzPayloadEntry kMxzzPayloads[] = {")
    for r in resolved:
        lines.append("    { \"%s\", %s, %du },   // %s"
                     % (r["official_sha256"], r["symbol"], r["patched_size"], r["role"]))
    lines.append("};")
    lines.append("")
    lines.append("// 补丁体自身的 sha256：缓冲区已是补丁体时静默放行（否则会误报「未知版本」并误弹 toast）")
    lines.append("#define MXZZ_SELF_SHA_COUNT %d" % len(resolved))
    lines.append("static const char* const kMxzzSelfShas[] = {")
    for r in resolved:
        lines.append("    \"%s\",   // %s" % (r["patched_sha256"], r["role"]))
    lines.append("};")
    lines.append("")
    lines.append("#endif  // STARRY_MXZZ_PAYLOAD_H")
    lines.append("")

    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    with open(args.out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))
    os.makedirs(os.path.dirname(REPORT), exist_ok=True)
    with open(REPORT, "w", encoding="utf-8", newline="\n") as f:
        json.dump(report, f, ensure_ascii=False, indent=2)
        f.write("\n")
    print("[out] %s" % os.path.relpath(args.out, ROOT))
    print("[out] %s" % os.path.relpath(REPORT, ROOT))
    return 0


if __name__ == "__main__":
    sys.exit(main())
