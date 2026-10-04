#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把出包脚本里硬编码的 /Users/aliang/... 换成可移植写法。

背景
----
`04_scripts/勇士之心/ysxz_mac_portability_check.sh` 的 [3/7] 检查硬编码 Mac 主目录。
仓库里 140+ 个出包脚本的第一段都是:

    NIXIANG="/Users/aliang/Documents/nixiang"

换机器 / 换人就跑不起来。本脚本把这类赋值改成"按脚本自身位置推导仓库根":

    NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"

转换规则
--------
A) 整行 shell 赋值  VAR="/Users/aliang/Documents/nixiang"   ->  按脚本位置回溯
B) 行内 shell 代码/heredoc 里的 /Users/aliang/Documents/nixiang -> ${NIXIANG}
C) 其它 /Users/<user>/...                -> $HOME/...
D) Python  Path("/Users/aliang/Documents/nixiang/xx") -> Path(__file__).resolve().parents[N] / "xx"
   Python  Path("/Users/aliang/Library/...")          -> Path.home() / "Library" / ...
E) JS 字符串 '...' 里的路径 -> (process.env.NIXIANG || require('os').homedir() + '/...')

默认 dry-run；`--apply` 才落盘（git 里已有历史，不再堆 .bak）。

用法
----
    python3 04_scripts/_common/portable_users_path_migrate.py
    python3 04_scripts/_common/portable_users_path_migrate.py --apply
    python3 04_scripts/_common/portable_users_path_migrate.py --list-residual
"""
from __future__ import annotations

import argparse
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

# 与检查器 [3/7] 的扫描范围保持一致
TARGETS = [
    os.path.join(ROOT, "mod菜单", "starry-mod-menu", "scripts"),
    os.path.join(ROOT, "04_scripts", "勇士之心"),
    os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "jni"),
    os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "build.gradle"),
    os.path.join(ROOT, "mod菜单", "starry-mod-menu", "build.gradle"),
]

SKIP_DIRS = {".git", "build", "obj", ".gradle", ".idea", "node_modules"}
TEXT_EXT = {".sh", ".bash", ".py", ".js", ".mk", ".gradle", ".txt", ".properties", ".json", ".pro"}

RE_ANY = re.compile(r"/Users/[A-Za-z0-9_.-]+/[^\s\"'`;)|&<>]*")
RE_ASSIGN = re.compile(
    r"^(?P<indent>[ \t]*)"
    r"(?P<var>[A-Za-z_][A-Za-z0-9_]*)="
    r"(?P<q>[\"']?)"
    r"(?P<path>/Users/[A-Za-z0-9_.-]+/[^\"'\s]*)"
    r"(?P=q)"
    r"(?P<tail>[ \t]*)$"
)
RE_PY_PATH = re.compile(r"Path\(\s*(?P<q>[\"'])(?P<p>/Users/[^\"']*)(?P=q)\s*\)")
RE_JS_STR = re.compile(r"(?P<q>[\"'])(?P<p>/Users/[^\"']*)(?P=q)")

REPO_UNDER_HOME = "/Users/aliang/Documents/nixiang"


def iter_files():
    for t in TARGETS:
        if os.path.isfile(t):
            yield t
            continue
        if not os.path.isdir(t):
            continue
        for base, dirs, files in os.walk(t):
            dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
            for f in files:
                if os.path.splitext(f)[1].lower() in TEXT_EXT:
                    yield os.path.join(base, f)


def read_text(path):
    with open(path, "rb") as fh:
        raw = fh.read()
    if b"\x00" in raw[:4096]:
        return None, None
    for enc in ("utf-8", "gbk"):
        try:
            return raw.decode(enc), enc
        except UnicodeDecodeError:
            continue
    return None, None


def write_text(path, text, enc):
    tmp = path + ".porttmp"
    with open(tmp, "w", encoding=enc, newline="") as fh:
        fh.write(text)
    os.replace(tmp, path)


def parents_to_root(path):
    """文件所在目录 -> 仓库根，返回 parents[N] 里的 N。"""
    d = os.path.dirname(os.path.abspath(path))
    rel = os.path.relpath(ROOT, d).replace("\\", "/")
    if rel == ".":
        return 0, ""
    return len(rel.split("/")), rel


def shell_expr(path, var, value):
    n, rel = parents_to_root(path)
    if rel == "":
        base = '$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)'
    elif rel.startswith(".."):
        base = '$(cd "$(dirname "${BASH_SOURCE[0]}")/%s" && pwd)' % rel
    else:
        return None
    return '"${%s:-%s}"' % (var, base)


def py_expr(path, val):
    n, _rel = parents_to_root(path)
    if val.startswith(REPO_UNDER_HOME):
        rest = val[len(REPO_UNDER_HOME):].strip("/")
        expr = "Path(__file__).resolve().parents[%d]" % n
    elif val.startswith("/Users/aliang/"):
        rest = val[len("/Users/aliang/"):].strip("/")
        expr = "Path.home()"
    else:
        return None
    for part in [p for p in rest.split("/") if p]:
        expr += ' / "%s"' % part
    return expr


def js_expr(val):
    if val.startswith(REPO_UNDER_HOME):
        rest = val[len(REPO_UNDER_HOME):].strip("/")
        expr = "(process.env.NIXIANG || require('os').homedir() + '/Documents/nixiang')"
    elif val.startswith("/Users/aliang/"):
        rest = val[len("/Users/aliang/"):].strip("/")
        expr = "require('os').homedir()"
    else:
        return None
    for part in [p for p in rest.split("/") if p]:
        expr += " + '/%s'" % part
    return expr


def migrate(path, apply_changes):
    text, enc = read_text(path)
    if text is None:
        return 0, ["[skip] 非文本: %s" % path]
    ext = os.path.splitext(path)[1].lower()
    changed = 0
    out = []
    eol = "\r\n" if "\r\n" in text else "\n"
    for i, line in enumerate(text.splitlines()):
        m = RE_ASSIGN.match(line)
        if m:
            expr = shell_expr(path, m.group("var"), m.group("path"))
            if expr:
                out.append("%s%s=%s%s" % (m.group("indent"), m.group("var"), expr, m.group("tail")))
                changed += 1
                continue
        out.append(line)
    text = eol.join(out) + (eol if text.endswith(("\n", "\r")) else "")

    # --- B/C/D/E: 行内形式 ---
    if ext in (".sh", ".bash", ".mk", ".pro"):
        new = text.replace(REPO_UNDER_HOME, "${NIXIANG}")
        new = re.sub(r"/Users/[A-Za-z0-9_.-]+/", "$HOME/", new)
        # 用了 ${NIXIANG} 但整个文件没有它的定义 => 补一段按脚本位置回溯的定义
        if "${NIXIANG}" in new and not re.search(r"^\s*(export\s+)?NIXIANG=", new, re.M):
            n, rel = parents_to_root(path)
            body = '$(cd "$(dirname "${BASH_SOURCE[0]}")%s" && pwd)' % (
                ("/" + rel) if rel.startswith("..") else "")
            decl = 'NIXIANG="${NIXIANG:-%s}"' % body
            lines = new.splitlines(keepends=False)
            if lines and lines[0].startswith("#!"):
                lines.insert(1, decl)
            else:
                lines.insert(0, decl)
            new = eol.join(lines) + eol
    elif ext == ".py":
        new = RE_PY_PATH.sub(lambda m: py_expr(path, m.group("p")) or m.group(0), text)
    elif ext == ".js":
        new = RE_JS_STR.sub(lambda m: js_expr(m.group("p")) or m.group(0), text)
    else:
        new = text
    changed += sum(1 for a, b in zip(text.splitlines(), new.splitlines()) if a != b)
    text = new

    residual = []
    for i, line in enumerate(text.splitlines()):
        if "/Users/" in line:
            residual.append("%s:%d  %s" % (path, i + 1, line.strip()))
    if changed and apply_changes:
        write_text(path, text, enc)
    return changed, residual


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--apply", action="store_true", help="真正写盘(默认 dry-run)")
    ap.add_argument("--list-residual", action="store_true", help="只打印残留，不改")
    args = ap.parse_args()

    n_files = 0
    n_changed = 0
    all_res = []
    for f in sorted(iter_files()):
        ch, res = migrate(f, args.apply)
        all_res.extend(res)
        if res:
            n_files += 1
        if ch:
            n_changed += ch
            print("[改] %s  (%d 行)" % (os.path.relpath(f, ROOT), ch))
    print("-" * 60)
    print("自动改写行数: %d" % n_changed)
    print("仍有 /Users/ 残留的文件数: %d，残留行数: %d" % (n_files, len(all_res)))
    if all_res:
        print("-" * 60)
        for r in all_res:
            print(os.path.relpath(r, ROOT) if r.startswith(ROOT) else r)
    if not args.apply:
        print("-" * 60)
        print("DRY-RUN：加 --apply 才会落盘")
    return 0


if __name__ == "__main__":
    sys.exit(main())
