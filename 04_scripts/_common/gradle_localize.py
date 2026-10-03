#!/usr/bin/env python3
"""把 starry-mod-menu 的临时构建副本"本地化"到本机 Android SDK + 国内镜像。

只改**临时副本**（surgical 的 $WORK/starry），不动仓库源文件。

三件事，全部幂等：

1) repositories{} 前置阿里云镜像
   背景：dl.google.com 在国内时通时不通——maven-metadata.xml 能 200，但下大 jar 会挂在
   socket read 上（Gradle daemon 75 线程全 Wait、CPU 不涨、日志停在 "Starting a Gradle Daemon"）。
   本机实测 dl.google.com 0.08~8.6s（抖动极大） vs maven.aliyun.com 0.08s。
   阿里云放最前，google()/mavenCentral() 留后面兜底。

2) compileSdkVersion / buildToolsVersion 回落到本机已装版本
   背景：项目写死 compileSdkVersion 35 / 不写 buildToolsVersion（AGP 8.6.0 默认 34.0.0），
   而本机 SDK 只有 platforms/android-36、build-tools/35.0.0+36.1.0，也没有 cmdline-tools
   （装不了新组件）⇒ AGP 会去 dl.google.com/android/repository 自动下载
   platforms;android-35 与 build-tools;34.0.0，那儿不通 ⇒ 卡死并反复打印
       Still waiting for package manifests to be fetched remotely.
   回落到本机已有的版本即可；star app 只编一个桥 smali，平台版本无所谓。
   并在 gradle.properties 写 android.suppressUnsupportedCompileSdk=<n>（AGP 8.6 官方最高认 35）。

3) gradle.properties 关掉 SDK 自动下载
   android.builder.sdkDownload=false ⇒ 真缺组件时立刻报错，而不是静默挂在网络上。

用法: gradle_localize.py <项目目录> [--sdk <AndroidSdk 根>]
"""
import os
import re
import sys

MIRRORS = [
    "https://maven.aliyun.com/repository/google",
    "https://maven.aliyun.com/repository/public",
    "https://maven.aliyun.com/repository/gradle-plugin",
]
MARK = "maven.aliyun.com/repository"


def mirror_block(indent):
    return "".join('%smaven { url "%s" }\n' % (indent, u) for u in MIRRORS)


def patch_repositories(text):
    if MARK in text:
        return text, 0
    out, n = [], 0
    for line in text.splitlines(keepends=True):
        out.append(line)
        m = re.match(r"^(\s*)repositories\s*\{\s*$", line.rstrip("\n") + "\n")
        if m:
            out.append(mirror_block(m.group(1) + "    "))
            n += 1
    return "".join(out), n


def installed_platforms(sdk):
    d = os.path.join(sdk, "platforms")
    out = []
    if os.path.isdir(d):
        for name in os.listdir(d):
            m = re.fullmatch(r"android-(\d+)", name)
            if m and os.path.isfile(os.path.join(d, name, "android.jar")):
                out.append(int(m.group(1)))
    return sorted(out)


def installed_build_tools(sdk):
    d = os.path.join(sdk, "build-tools")
    out = []
    if os.path.isdir(d):
        for name in os.listdir(d):
            # 只看真的有 aapt2/zipalign 的目录
            if os.path.isfile(os.path.join(d, name, "aapt2.exe")) or os.path.isfile(os.path.join(d, name, "aapt2")):
                out.append(name)
    return sorted(out, key=lambda s: [int(x) for x in re.findall(r"\d+", s)])


def prop_set(text, key, value):
    pat = re.compile(r"^\s*%s\s*=.*$" % re.escape(key), re.M)
    line = "%s=%s" % (key, value)
    if pat.search(text):
        return pat.sub(line, text)
    return text.rstrip("\n") + "\n" + line + "\n"


def main():
    if len(sys.argv) < 2:
        print(__doc__, file=sys.stderr)
        raise SystemExit(2)
    proj = sys.argv[1]
    sdk = None
    if "--sdk" in sys.argv:
        sdk = sys.argv[sys.argv.index("--sdk") + 1]
    sdk = sdk or os.environ.get("ANDROID_SDK_ROOT") or os.environ.get("ANDROID_HOME") or ""
    sdk = sdk.replace("\\", "/")
    if not sdk or not os.path.isdir(sdk):
        raise SystemExit("ERROR: 找不到 Android SDK（--sdk 或 ANDROID_SDK_ROOT）：%r" % sdk)

    plats = installed_platforms(sdk)
    bts = installed_build_tools(sdk)
    if not plats:
        raise SystemExit("ERROR: %s/platforms 下没有可用 platform" % sdk)
    if not bts:
        raise SystemExit("ERROR: %s/build-tools 下没有可用 build-tools" % sdk)
    print("  本机 platform: %s" % ", ".join("android-%d" % p for p in plats))
    print("  本机 build-tools: %s" % ", ".join(bts))

    # ---- 1) 镜像：根 build.gradle + 各模块 build.gradle ----
    gradles = []
    for dirpath, dirnames, filenames in os.walk(proj):
        dirnames[:] = [d for d in dirnames if d not in (".gradle", "build", ".git")]
        for fn in filenames:
            if fn in ("build.gradle", "build.gradle.kts"):
                gradles.append(os.path.join(dirpath, fn))
    total = 0
    for g in sorted(gradles):
        t = open(g, encoding="utf-8").read()
        new, n = patch_repositories(t)
        if n:
            open(g, "w", encoding="utf-8", newline="").write(new)
            total += n
            print("  镜像 ← %d 块  %s" % (n, os.path.relpath(g, proj)))
    if not total:
        print("  镜像：已存在，跳过")

    # ---- 2) compileSdk / buildTools 回落 ----
    compile_sdk = None
    bt = bts[-1]
    for g in sorted(gradles):
        t = open(g, encoding="utf-8").read()
        orig = t
        m = re.search(r"^\s*compileSdk(?:Version)?\s+(\d+)\s*$", t, re.M)
        if m:
            want = int(m.group(1))
            use = want if want in plats else plats[-1]
            if use != want:
                t = re.sub(r"^(\s*compileSdk(?:Version)?\s+)\d+\s*$",
                           lambda mm: "%s%d" % (mm.group(1), use), t, flags=re.M)
                print("  compileSdkVersion %d → %d（%d 本机没装）" % (want, use, want))
            compile_sdk = use
        elif re.search(r"^\s*android\s*\{", t, re.M) and compile_sdk:
            pass
        # buildToolsVersion：显式写死，避免 AGP 默认的 34.0.0 触发自动下载
        if re.search(r"^\s*buildToolsVersion\s", t, re.M):
            t = re.sub(r"^(\s*buildToolsVersion\s+).*$",
                       lambda mm: "%s'%s'" % (mm.group(1), bt), t, flags=re.M)
        elif "com.android.application" in t or "com.android.library" in t:
            t = re.sub(r"^(\s*compileSdk(?:Version)?\s+\d+\s*)$",
                       lambda mm: mm.group(1) + "\n    buildToolsVersion '%s'" % bt, t, flags=re.M, count=1)
        if t != orig:
            open(g, "w", encoding="utf-8", newline="").write(t)
            print("  buildToolsVersion ← %s  %s" % (bt, os.path.relpath(g, proj)))

    # ---- 3) gradle.properties ----
    gp = os.path.join(proj, "gradle.properties")
    t = open(gp, encoding="utf-8").read() if os.path.isfile(gp) else ""
    if compile_sdk:
        t = prop_set(t, "android.suppressUnsupportedCompileSdk", str(compile_sdk))
    t = prop_set(t, "android.builder.sdkDownload", "false")
    open(gp, "w", encoding="utf-8", newline="").write(t)
    print("  gradle.properties: suppressUnsupportedCompileSdk=%s, sdkDownload=false" % compile_sdk)


if __name__ == "__main__":
    main()
