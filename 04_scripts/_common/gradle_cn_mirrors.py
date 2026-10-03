#!/usr/bin/env python3
"""在 build.gradle 的每个 repositories{} 块最前面插入国内镜像仓库。

背景：dl.google.com 在国内时通时不通——maven-metadata.xml 能 200，但下大 jar 会挂在
socket read 上（Gradle daemon 75 个线程全 Wait，CPU 不涨，日志停在 "Starting a Gradle Daemon"）。
本机实测：
  dl.google.com/.../maven-metadata.xml        200   1.59s
  maven.aliyun.com/repository/google/...      200   0.23s   ← 快一个数量级
  repo.maven.apache.org/maven2/               200   2.72s

把阿里云放在最前面、google()/mavenCentral() 留在后面兜底：
命中镜像就走镜像，镜像没有的仍然回官方源。幂等——已插过就跳过。

用法: gradle_cn_mirrors.py <build.gradle> [--check]
"""
import re
import sys

MIRRORS = [
    "https://maven.aliyun.com/repository/google",
    "https://maven.aliyun.com/repository/public",
    "https://maven.aliyun.com/repository/gradle-plugin",
]
MARK = "maven.aliyun.com/repository"


def mirror_block(indent):
    return "\n".join(
        '%smaven { url "%s" }' % (indent, u) for u in MIRRORS
    ) + "\n"


def patch(text):
    if MARK in text:
        return text, 0
    out = []
    count = 0
    for line in text.splitlines(keepends=True):
        out.append(line)
        m = re.match(r"^(\s*)repositories\s*\{\s*$", line.rstrip("\n") + "\n")
        if m:
            out.append(mirror_block(m.group(1) + "    "))
            count += 1
    return "".join(out), count


def main():
    if len(sys.argv) < 2:
        print(__doc__, file=sys.stderr)
        raise SystemExit(2)
    path = sys.argv[1]
    text = open(path, encoding="utf-8").read()
    new, n = patch(text)
    if "--check" in sys.argv:
        print("%s: %s" % (path, "already mirrored" if n == 0 else "needs %d mirror block(s)" % n))
        return
    if n == 0:
        print("  cn-mirrors: 已存在，跳过 (%s)" % path)
        return
    open(path, "w", encoding="utf-8", newline="").write(new)
    print("  cn-mirrors: 已插入 %d 个 repositories{} 块 ← 阿里云优先，google()/mavenCentral() 兜底 (%s)" % (n, path))


if __name__ == "__main__":
    main()
