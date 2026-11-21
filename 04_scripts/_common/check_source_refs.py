#!/usr/bin/env python3
"""check_source_refs.py — 跨目录源码引用完整性门禁（2026-11-21 建）

事故背景（这就是本脚本存在的唯一理由）
  2026-11-11 02:04 写了 core/TapTapWebLogin.java，只落在本机磁盘、从未 `git add`。
  11-15 / 11-18 / 11-20 三次收尾都按 `games/<game_id>` 目录白名单提交，目录外的这个新类
  一路没人补；而这三次入库的 GameHookImpl.java 都 `import com.starry.magic.core.TapTapWebLogin`。
  结果：Mac 本机因为磁盘上有文件，编译永远绿；Win clone 下来 Gradle 全量 javac
  直接 `cannot find symbol`，连编译都进不去。直到 2026-11-21（提交 4d6ac207）才补入库。
  当时的机器关只校验「本次暂存了哪些文件」，从不校验「已入库代码引用的东西是否也在库里」，
  所以三次提交全 PASS —— 这是门禁的盲区，不是谁手滑。

判据
  1) 已入库 .java 中的 `import com.starry.magic.X;` 必须解析到**已入库**的源文件，否则 FAIL。
     BuildConfig / R 由 Gradle 构建期生成，按定义排除。
     实现走 `git ls-files`（索引），不看磁盘 —— 磁盘上有、库里没有，正是要抓的情形。
  2) app/src 下存在未入库源码（.java/.cpp/.h/.hpp/.c/.mk）→ REMIND 打印清单但不 FAIL，
     避免误伤正在写的半成品会话；但收尾前必须清零（remind 会一直跟着）。

用法
  python3 check_source_refs.py [--root <仓库根>]
退出码
  0 = 无悬空引用；1 = 存在悬空引用（必须修）；2 = 环境问题（非 git 仓）
"""

import argparse
import re
import subprocess
import sys
from pathlib import Path

JAVA_ROOT = "mod菜单/starry-mod-menu/app/src/main/java"
SRC_ROOT = "mod菜单/starry-mod-menu/app/src"
GRADLE_GENERATED = {"com.starry.magic.BuildConfig", "com.starry.magic.R"}
SOURCE_EXTS = (".java", ".cpp", ".cc", ".c", ".h", ".hpp", ".mk")
IMPORT_RE = re.compile(r"^\s*import\s+(com\.starry\.magic\.[\w.]+)\s*;", re.M)


def git(root: Path, *args: str) -> str:
    # core.quotepath=off：否则非 ASCII 路径被 git 转义成 \346\234\210，中文目录全部误判
    proc = subprocess.run(["git", "-c", "core.quotepath=off", *args], cwd=root,
                          capture_output=True, text=True, encoding="utf-8", errors="replace")
    if proc.returncode != 0:
        raise SystemExit(f"git {' '.join(args)} 失败: {proc.stderr.strip()}")
    return proc.stdout


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--root", default=".")
    ns = ap.parse_args()
    root = Path(ns.root).resolve()

    if subprocess.run(["git", "rev-parse", "--git-dir"], cwd=root,
                      capture_output=True).returncode != 0:
        print("✗ 不是 git 仓库，无法校验索引", file=sys.stderr)
        return 2

    tracked = [p for p in git(root, "ls-files", "--", JAVA_ROOT).splitlines() if p.endswith(".java")]
    if not tracked:
        print(f"⚠ 索引里没有 {JAVA_ROOT} 下的 .java，跳过（路径可能已迁移）")
        return 0

    prefix = JAVA_ROOT + "/"
    tracked_classes = {p[len(prefix):-len(".java")].replace("/", ".") for p in tracked}

    dangling: dict[str, list[str]] = {}
    for path in tracked:
        src = (root / path).read_text(encoding="utf-8", errors="ignore")
        for cls in IMPORT_RE.findall(src):
            if cls in GRADLE_GENERATED or cls in tracked_classes:
                continue
            dangling.setdefault(cls, []).append(path.split("/")[-1])

    untracked: list[str] = []
    porcelain = git(root, "status", "--porcelain", "-uall", "--", SRC_ROOT)
    for line in porcelain.splitlines():
        if not line.startswith("?? "):
            continue
        rel = line[3:].strip()
        if rel.endswith(SOURCE_EXTS):
            untracked.append(rel)

    if untracked:
        print(f"⚠ REMIND [未入库源码] {len(untracked)} 个文件只在磁盘、不在库里（收尾前须清零）:")
        for rel in sorted(untracked):
            print(f"    - {rel}")
        print("    处置: bash 04_scripts/_common/archive_scoped_git.sh --message '<说明>' --path '<上述路径>'")

    if dangling:
        print(f"✗ FAIL [悬空引用] {len(dangling)} 个类被已入库代码 import，但索引里没有对应源文件")
        print("   ⇒ Win/Linux clone 后 Gradle 全量 javac 必然 cannot find symbol")
        for cls, users in sorted(dangling.items()):
            rel = cls.replace(".", "/") + ".java"
            print(f"    - {cls}")
            print(f"        期望路径: {JAVA_ROOT}/{rel}")
            print(f"        引用方: {', '.join(sorted(set(users))[:5])}")
        return 1

    if not untracked:
        print(f"✓ 已检查 {len(tracked)} 个已入库 .java：跨目录 import 全部可解析，无未入库源码")
    else:
        print(f"✓ 已检查 {len(tracked)} 个已入库 .java：跨目录 import 全部可解析")
    return 0


if __name__ == "__main__":
    sys.exit(main())
