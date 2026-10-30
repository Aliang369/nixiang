#!/usr/bin/env python3
"""detect_abis.py — 读 APK **中央目录**里的 lib/<abi>/ 列表（不整包解压，1.8G 包也是秒级）。

为什么存在（2026-10-31 公主连结雷电轮）：
    官方包自带哪些 ABI 决定「这个包在雷电模拟器上能不能跑原生 x86_64」——
    PCR 官方包同时带 arm64-v8a / armeabi-v7a / x86_64，雷电 primaryCpuAbi=x86_64
    只解出 lib/x86_64/，而星光包历史上只注入 arm64 的 libstarry_magic.so ⇒
    dlopen 恒失败、菜单显示「暂无功能列表（native 未就绪）」。
    出包脚本因此不该写死 `APP_ABI=arm64-v8a`，应**按底包实况**出 so。

用法:
    python3 detect_abis.py <base.apk>            # 打印「框架可产出 且 底包确有」的 ABI
    python3 detect_abis.py <base.apk> --all      # 打印底包内全部 ABI
    python3 detect_abis.py <base.apk> --check x86_64   # 底包是否带该 ABI（rc=0/1）

约定：BUILDABLE = 本框架能产出、且两种执行形态都成立的 ABI。
    arm64-v8a —— 真机原生；雷电上由 houdini 转译执行（可用但 hooks 稳定性另说）
    x86_64    —— 雷电/模拟器**原生**执行（Dobby/Keystone 都有 x86_64 预编译，2026-10-31 PCR 实测通过）
    不产 armeabi-v7a / x86：32 位在主流设备与模拟器上都已不是默认 ABI，出了也没人解。
"""
import sys
import zipfile

BUILDABLE = ["arm64-v8a", "x86_64"]


def abis_in_apk(apk):
    out = []
    with zipfile.ZipFile(apk) as z:
        for name in z.namelist():
            if not name.startswith("lib/"):
                continue
            parts = name.split("/")
            if len(parts) >= 3 and parts[1] and parts[1] not in out:
                out.append(parts[1])
    return out


def main(argv):
    if len(argv) < 2 or argv[1] in ("-h", "--help"):
        sys.stdout.write(__doc__)
        return 2
    apk = argv[1]
    try:
        found = abis_in_apk(apk)
    except Exception as exc:  # noqa: BLE001
        sys.stderr.write("ERROR: 读不了的 APK: %s (%s)\n" % (apk, exc))
        return 2

    if "--all" in argv:
        print(" ".join(sorted(found)))
        return 0
    if "--check" in argv:
        i = argv.index("--check")
        want = argv[i + 1] if i + 1 < len(argv) else ""
        if not want:
            sys.stderr.write("ERROR: --check 需要 ABI 名\n")
            return 2
        ok = want in found
        sys.stderr.write("%s: 底包%s %s（底包 ABI=%s）\n"
                         % (apk, "带" if ok else "不带", want, " ".join(sorted(found))))
        return 0 if ok else 1

    sel = [a for a in BUILDABLE if a in found]
    if not sel:
        sys.stderr.write("WARN: 底包内没有框架可产出的 ABI（底包 ABI=%s）\n"
                         % " ".join(sorted(found)))
    print(" ".join(sel))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
