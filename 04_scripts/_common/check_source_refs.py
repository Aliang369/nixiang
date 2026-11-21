#!/usr/bin/env python3
"""check_source_refs.py — 跨目录源码引用完整性门禁（2026-11-21 建）

事故背景（本脚本存在的唯一理由）
  ① Java（2026-11-11 起，2026-11-21 才发现）：core/TapTapWebLogin.java 只落在本机磁盘、
     从未 `git add`；之后三次收尾按 `games/<game_id>` 目录白名单提交，目录外的这个新类
     一路没人补，而这三次入库的 GameHookImpl.java 都 import 它。Mac 本机有文件所以编译永远绿，
     Win clone 后 Gradle 全量 javac 直接 `cannot find symbol`。补入库：4d6ac207。
  ② Python 后台（同一天暴露）：auth-server 的 deviceid.py / services/presence.py 未入库，
     而 6 个已入库文件 `from ..deviceid import …`、`from ..services.presence import …`。
     同样的结构：改动与它依赖的新模块不在同一笔提交里。补入库：a36e2836。
  共同病根：收尾按目录白名单 add，「本次暂存了什么」永远是绿的，
  「已入库代码引用的东西是否也在库里」从来没人问。本脚本只回答后者。

判据
  1) Java：已入库 .java 里 `import com.starry.magic.X;` 必须解析到已入库源文件。
     BuildConfig / R 为 Gradle 生成，按定义排除。
  2) Python：auth-server 下已入库 .py 的 `from .x import a` / `from ..y.z import b` /
     `from app.q import c` 必须解析到已入库 .py 或包 `__init__.py`。
  3) app/src 与 auth-server 下存在未入库源码（.java/.cpp/.h/.hpp/.c/.mk/.py）→ REMIND 列清单
     但不 FAIL（避免误伤正在写的半成品）；收尾前须清零。
  一律以 **git 索引** 为基准（`git ls-files`），不看磁盘 —— 磁盘上有、库里没有正是要抓的情形。

用法
  python3 check_source_refs.py [--root <仓库根>]
退出码
  0 = 无悬空引用；1 = 存在悬空引用；2 = 环境问题（非 git 仓）
"""

import argparse
import re
import subprocess
import sys
from pathlib import Path

JAVA_ROOT = "mod菜单/starry-mod-menu/app/src/main/java"
SRC_ROOT = "mod菜单/starry-mod-menu/app/src"
PY_ROOT = "mod菜单/starry-mod-menu/auth-server"
GRADLE_GENERATED = {"com.starry.magic.BuildConfig", "com.starry.magic.R"}
SOURCE_EXTS = (".java", ".cpp", ".cc", ".c", ".h", ".hpp", ".mk", ".py")
JAVA_IMPORT_RE = re.compile(r"^\s*import\s+(com\.starry\.magic\.[\w.]+)\s*;", re.M)
PY_IMPORT_RE = re.compile(r"^\s*from\s+(\.*)([\w.]*)\s+import\s+(.+)$", re.M)


def git(root: Path, *args: str) -> str:
    # core.quotepath=off：否则非 ASCII 路径被 git 转义成 \346\234\210，中文目录全部误判
    proc = subprocess.run(["git", "-c", "core.quotepath=off", *args], cwd=root,
                          capture_output=True, text=True, encoding="utf-8", errors="replace")
    if proc.returncode != 0:
        raise SystemExit(f"git {' '.join(args)} 失败: {proc.stderr.strip()}")
    return proc.stdout


def check_java(root: Path) -> dict[str, list[str]]:
    tracked = [p for p in git(root, "ls-files", "--", JAVA_ROOT).splitlines() if p.endswith(".java")]
    if not tracked:
        return {}
    prefix = JAVA_ROOT + "/"
    known = {p[len(prefix):-len(".java")].replace("/", ".") for p in tracked}
    dangling: dict[str, list[str]] = {}
    for path in tracked:
        src = (root / path).read_text(encoding="utf-8", errors="ignore")
        for cls in JAVA_IMPORT_RE.findall(src):
            if cls in GRADLE_GENERATED or cls in known:
                continue
            dangling.setdefault(cls, []).append(path.split("/")[-1])
    return dangling


def check_python(root: Path) -> dict[str, list[str]]:
    tracked = [p for p in git(root, "ls-files", "--", PY_ROOT).splitlines() if p.endswith(".py")]
    if not tracked:
        return {}
    known = set(tracked)
    dangling: dict[str, list[str]] = {}
    for path in tracked:
        src = (root / path).read_text(encoding="utf-8", errors="ignore")
        pkg = Path(path).parent
        for dots, mod, names in PY_IMPORT_RE.findall(src):
            base = pkg
            for _ in range(max(len(dots) - 1, 0)):
                base = base.parent
            if dots:
                target = base / mod.replace(".", "/") if mod else base
            elif mod == "app" or mod.startswith("app."):
                target = Path(PY_ROOT) / mod.replace(".", "/")  # 项目绝对 import
            else:
                continue  # 标准库/第三方（datetime / fastapi / sqlalchemy …）不在本门禁范围
            cands = [str(target) + ".py", str(target / "__init__.py")]
            if any(c in known for c in cands):
                continue
            # `from . import x` / `from pkg import module` 形式：名字本身可能就是子模块
            names_list = [n.strip().split(" as ")[0].strip("() ") for n in names.split(",")]
            if any(f"{target}/{n}.py" in known for n in names_list if n.isidentifier()):
                continue
            dangling.setdefault(f"{Path(path).name} → {dots}{mod} import {names[:50].strip()}",
                                []).append(path.replace(PY_ROOT + "/", ""))
    return dangling


def untracked_sources(root: Path) -> list[str]:
    out, seen = [], set()
    for scope in (SRC_ROOT, PY_ROOT):
        for line in git(root, "status", "--porcelain", "-uall", "--", scope).splitlines():
            if not line.startswith("?? "):
                continue
            rel = line[3:].strip()
            if rel.endswith(SOURCE_EXTS) and rel not in seen:
                seen.add(rel)
                out.append(rel)
    return sorted(out)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--root", default=".")
    ns = ap.parse_args()
    root = Path(ns.root).resolve()

    if subprocess.run(["git", "rev-parse", "--git-dir"], cwd=root,
                      capture_output=True).returncode != 0:
        print("✗ 不是 git 仓库，无法校验索引", file=sys.stderr)
        return 2

    java_bad = check_java(root)
    py_bad = check_python(root)
    loose = untracked_sources(root)

    if loose:
        print(f"⚠ REMIND [未入库源码] {len(loose)} 个文件只在磁盘、不在库里（收尾前须清零）:")
        for rel in loose:
            print(f"    - {rel}")
        print("    处置: bash 04_scripts/_common/archive_scoped_git.sh --message '<说明>' --path '<上述路径>'")

    rc = 0
    if java_bad:
        print(f"✗ FAIL [悬空引用·Java] {len(java_bad)} 个类被已入库代码 import，但索引里没有对应源文件")
        print("   ⇒ Win/Linux clone 后 Gradle 全量 javac 必然 cannot find symbol")
        for cls, users in sorted(java_bad.items()):
            print(f"    - {cls}")
            print(f"        期望路径: {JAVA_ROOT}/{cls.replace('.', '/')}.java")
            print(f"        引用方: {', '.join(sorted(set(users))[:5])}")
        rc = 1

    if py_bad:
        print(f"✗ FAIL [悬空引用·Python] {len(py_bad)} 条 import 指向索引里不存在的模块")
        print("   ⇒ 后台 clone 后 import 期直接 ModuleNotFoundError，服务起不来")
        for desc, users in sorted(py_bad.items()):
            print(f"    - {desc}")
            print(f"        引用方: {', '.join(sorted(set(users))[:5])}")
        rc = 1

    if rc == 0:
        print(f"✓ 源码引用完整：Java {len(git(root, 'ls-files', '--', JAVA_ROOT).splitlines())} 个、"
              f"Python {len([p for p in git(root, 'ls-files', '--', PY_ROOT).splitlines() if p.endswith('.py')])} 个已入库源文件，"
              f"跨目录 import 全部可解析" + ("" if loose else "，无未入库源码"))
    return rc


if __name__ == "__main__":
    sys.exit(main())
