#!/usr/bin/env python3
"""把 gradle 发行版里的分片 jar 合并成项目用的 gradle-wrapper.jar。

发行版自带:
  lib/plugins/gradle-wrapper-main-<v>.jar  -> org.gradle.wrapper.GradleWrapperMain（MANIFEST 无 Main-Class）
  lib/gradle-wrapper-shared-<v>.jar        -> Install / Download / BootstrapMainStarter 等
  lib/gradle-cli-<v>.jar                   -> org.gradle.cli.CommandLineParser（GradleWrapperMain 依赖）

自己合成的 MANIFEST 必须补 `Main-Class: org.gradle.wrapper.GradleWrapperMain`，
否则 `java -jar gradle-wrapper.jar` 报「没有主清单属性」。

用法:
  merge_gradle_wrapper.py <dist>/lib/plugins/gradle-wrapper-main-<v>.jar \
                          <dist>/lib/gradle-wrapper-shared-<v>.jar \
                          <dist>/lib/gradle-cli-<v>.jar \
                          <out>/gradle-wrapper.jar
"""
import os
import re
import sys
import zipfile

MANIFEST = "META-INF/MANIFEST.MF"

MANIFEST_BODY = (
    "Manifest-Version: 1.0\r\n"
    "Main-Class: org.gradle.wrapper.GradleWrapperMain\r\n"
    "Implementation-Title: Gradle\r\n"
    "Implementation-Version: {version}\r\n"
    "\r\n"
)

# 只从 gradle-cli 里挑 wrapper 需要的包，避免把整个 cli 依赖树拖进来
EXTRA_PREFIXES = ("org/gradle/cli/",)


def _version_of(main_jar):
    m = re.search(r"gradle-wrapper-main-([0-9][0-9.]*)\.jar$", os.path.basename(main_jar))
    return m.group(1) if m else "8.13"


def _collect(jar, prefixes=None):
    out = []
    with zipfile.ZipFile(jar) as z:
        for info in z.infolist():
            n = info.filename
            if n == MANIFEST or n.endswith("/"):
                continue
            if prefixes is not None and not n.startswith(prefixes):
                continue
            out.append((n, z.read(n)))
    return out


def merge(wrapper_main, wrapper_shared, gradle_cli, out_jar):
    entries = []
    seen = set()
    for jar, prefixes in ((wrapper_shared, None), (gradle_cli, EXTRA_PREFIXES), (wrapper_main, None)):
        for name, data in _collect(jar, prefixes):
            if name in seen:
                continue
            seen.add(name)
            entries.append((name, data))

    manifest = MANIFEST_BODY.format(version=_version_of(wrapper_main)).encode("utf-8")

    os.makedirs(os.path.dirname(os.path.abspath(out_jar)), exist_ok=True)
    with zipfile.ZipFile(out_jar, "w", zipfile.ZIP_DEFLATED) as z:
        zi = zipfile.ZipInfo(MANIFEST, date_time=(2024, 1, 1, 0, 0, 0))
        zi.compress_type = zipfile.ZIP_STORED
        z.writestr(zi, manifest)
        for name, data in entries:
            zi = zipfile.ZipInfo(name, date_time=(2024, 1, 1, 0, 0, 0))
            zi.compress_type = zipfile.ZIP_DEFLATED
            z.writestr(zi, data)

    with zipfile.ZipFile(out_jar) as z:
        bad = z.testzip()
        names = z.namelist()
    print("out    : %s (%d B, %d entries)" % (out_jar, os.path.getsize(out_jar), len(names)))
    print("first  : %s" % names[0])
    print("testzip: %s" % (bad or "OK"))
    for want in ("org/gradle/wrapper/GradleWrapperMain.class",
                 "org/gradle/wrapper/Install.class",
                 "org/gradle/wrapper/BootstrapMainStarter.class",
                 "org/gradle/cli/CommandLineParser.class"):
        print("  %-52s %s" % (want, "OK" if want in names else "MISSING"))


if __name__ == "__main__":
    if len(sys.argv) != 5:
        print(__doc__, file=sys.stderr)
        raise SystemExit(2)
    merge(sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4])
