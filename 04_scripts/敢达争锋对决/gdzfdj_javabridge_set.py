#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""敢达争锋对决 · 用 Java 桥把功能值推给 native（等价于菜单拨开关），触发 4 个抓点挂钩。

为什么要 Java 桥
----------------
* prefs 预置法已死：`ModBridge` 构造函数第一件事就是 `Preferences.clearFeaturePrefs()`
  （`ModBridge.java:196`，功能列表随 so 切换，默认全关），且 SharedPreferences 进程内
  内存缓存，改盘上文件无效。
* frida 17 起内置 Java 桥移出核心，必须 `import Java from 'frida-java-bridge'` 并由
  frida-tools 的 Compiler 打包（node_modules 需先 `npm ci`，见 phantom-frida/src）。
* MuMu = x86_64 ART + houdini ARM .so；x64 agent 看不到 ARM 模块，无法直接 Hook ARM
  代码，但 Java 层调用 `ModBridge` 的 static native 会经 houdini 正常落到 ARM 实现。

用法
----
  D:\\Python313\\python.exe gdzfdj_javabridge_set.py                 # 伤害=20 / 定怪=on / 加速=3
  D:\\Python313\\python.exe gdzfdj_javabridge_set.py --dmg 2 --root
  D:\\Python313\\python.exe gdzfdj_javabridge_set.py --off           # 全部复位
  D:\\Python313\\python.exe gdzfdj_javabridge_set.py --pid 63000 --keep
"""
import argparse
import pathlib
import sys
import time

import frida

PACKAGE = "com.kingnet.gundam.mg.normal"
AGENT = pathlib.Path(__file__).with_name("gdzfdj_javabridge_agent.mjs")
BRIDGE_SRC = pathlib.Path(r"E:\nixiang\01_tools\phantom-frida\src")


def build_bundle():
    """frida-compile 要求 entrypoint 位于 project_root 内，而 node_modules（含
    frida-java-bridge）只装在 phantom-frida/src ⇒ 临时把 agent 复制进该目录打包，
    打完即删，仓库不留痕。"""
    if not (BRIDGE_SRC / "node_modules" / "frida-java-bridge").is_dir():
        raise SystemExit(f"缺少 frida-java-bridge：请在 {BRIDGE_SRC} 执行 `npm ci`")
    tmp_entry = BRIDGE_SRC / AGENT.name
    compiler = frida.Compiler()
    diag = []
    try:
        compiler.on("diagnostics", lambda d: diag.append(str(d)))
    except Exception:
        pass
    try:
        tmp_entry.write_bytes(AGENT.read_bytes())
        src = compiler.build(str(tmp_entry), project_root=str(BRIDGE_SRC))
    except Exception as e:
        raise SystemExit(f"打包失败: {e}\n诊断: {diag}")
    finally:
        try:
            tmp_entry.unlink()
        except OSError:
            pass
    if not src:
        raise SystemExit(f"打包结果为空；诊断: {diag}")
    return src


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pid", type=int, default=0)
    ap.add_argument("--dmg", type=int, default=20)
    ap.add_argument("--root", action="store_true", help="开启超级定怪")
    ap.add_argument("--spd", type=int, default=3)
    ap.add_argument("--off", action="store_true", help="全部复位（1/OFF/1）")
    ap.add_argument("--keep", action="store_true", help="注入后保持 agent 挂载（不 detach）")
    ap.add_argument("--wait", type=float, default=3.0, help="注入后等待秒数再读日志")
    a = ap.parse_args()

    device = frida.get_usb_device(timeout=10)
    pid = a.pid
    if not pid:
        pid = next((app.pid for app in device.enumerate_applications()
                    if app.identifier == PACKAGE), 0)
    if not pid:
        sys.exit(f"未找到 {PACKAGE} 进程；请先启动游戏或用 --pid 指定")

    print(f"[*] 打包 agent … ({AGENT.name})")
    bundle = build_bundle()
    print(f"[*] bundle {len(bundle)} B；attach pid={pid}")
    session = device.attach(pid)
    script = session.create_script(bundle)
    script.on("message", lambda m, data: print("[agent]", m.get("payload", m)))
    script.load()

    if a.off:
        res = script.exports_sync.off()
    else:
        res = script.exports_sync.run(a.dmg, bool(a.root), a.spd)
    print("[*] 注入结果：")
    for k, v in res.items():
        print(f"    {k} = {v}")

    if a.keep:
        print("[*] --keep：保持挂载，Ctrl+C 退出")
        try:
            while True:
                time.sleep(1)
        except KeyboardInterrupt:
            pass
    else:
        time.sleep(a.wait)
    session.detach()
    print("[*] done")


if __name__ == "__main__":
    main()
