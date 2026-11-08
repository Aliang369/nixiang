#!/usr/bin/env python3
# 动物大联盟 1.0.25 — 私有源指纹（构建与核验共用的**唯一**口径）
#
# 为什么单独一个脚本：构建报告和事后核验必须用同一套"源集合 + 哈希算法"，
# 否则两边各写一遍必然对不上（并且对不上时会被当成"漂移"误报）。
#
# 口径：固定 9 个文件（本作 source 侧交付面），按相对路径排序，拼 "relpath:sha256" 逐行后取 sha256。
# 用途：
#   * dwdlm_build_so.sh 把它写进 so_build_<profile>.json 的 source_fingerprint；
#   * verify_artifact_dwdlm.py 重算并与报告比对 —— 不一致即"源码在构建后变更，候选过期"，FAIL。
import hashlib
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
JNI = ROOT / "mod菜单" / "starry-mod-menu" / "app" / "src" / "main" / "jni"
JAVA = ROOT / "mod菜单" / "starry-mod-menu" / "app" / "src" / "main" / "java" / "com" / "starry" / "magic" / "dwdlm"
FILES = [
    JNI / "Games" / "DwdlmEntry.cpp",
    JNI / "Games" / "DwdlmMod.cpp",
    JNI / "Games" / "DwdlmMod.h",
    JNI / "Games" / "DwdlmSecShell.cpp",
    JNI / "Games" / "DwdlmSecShell.h",
    JNI / "Android_dwdlm.mk",
    JAVA / "DwdlmAppComponentFactory.java",
    JAVA / "DwdlmApplication.java",
]

def sha(p):
    return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def main():
    rows = []
    missing = []
    for f in FILES:
        rel = str(Path(f).resolve().relative_to(ROOT))
        if not Path(f).is_file():
            missing.append(rel)
            continue
        rows.append(rel + ":" + sha(f))
    if missing:
        print("MISSING " + ",".join(missing), file=sys.stderr)
        return 2
    rows.sort()
    digest = hashlib.sha256(chr(10).join(rows).encode("utf-8")).hexdigest()
    if "--json" in sys.argv:
        import json
        print(json.dumps({"source_fingerprint": digest, "files": dict(r.split(":", 1) for r in rows)},
                         ensure_ascii=False, indent=1))
    else:
        print(digest)
    return 0

if __name__ == "__main__":
    sys.exit(main())
