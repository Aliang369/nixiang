#!/usr/bin/env python3
# -*- coding: utf-8 -*-
r"""gdzfdj_frida_verify.py — 无 UI 验证 4 个钩子是否按 RVA 挂载（敢达争锋对决星空 mod）

背景：ModBridge 构造函数每次进程启动都会 Preferences.clearFeaturePrefs() 清掉 featNum 偏好
     （ModBridge.java:196），因此**无法**用「预置 shared_prefs + 重启」的办法驱动功能开关；
     而 SharedPreferences 在进程内是内存缓存，运行中改盘上文件也读不到。
     故改用 Frida 直调 ModBridge 的静态 native 桥（ModBridge.java:130-156 均为 static native），
     等价于用户在菜单里拨动开关，从而触发 GdzfdjMod.cpp SyncHooks() 装机钩子。

前置：adb root + 设备侧 frida-server 已起（exec: /data/local/tmp/fs-x86_64 -D）
用法：python 04_scripts/敢达争锋对决/gdzfdj_frida_verify.py [--pid N] [--dmg 20] [--root on] [--spd 3]
验收：adb -s emulator-5554 logcat -d | grep '\[gdzfdj\]' 应出现
      [gdzfdj] xRole$$CalcDamage hooked @rva 0x3DB99CC 等 4 条
"""
import argparse
import sys
import time

try:
    import frida
except ImportError:
    sys.exit("frida python 模块缺失：pip install frida")

PKG = "com.kingnet.gundam.mg.normal"
SPEC_DMG = "Slider_伤害倍率:1:100:1"
SPEC_ROOT = "Toggle_超级定怪"
SPEC_SPD = "Slider_全局加速:1:10:1"

JS = r"""
Java.perform(function () {
  var MB = Java.use("com.starry.magic.bridge.ModBridge");
  function log(s) { send({ t: "log", m: String(s) }); }
  try {
    log("live=" + MB.nativeHasLiveSession() +
        " gameId=" + MB.nativeGetAuthGameId() +
        " libLoaded=" + MB.nativeIsGameLibLoaded());
  } catch (e) { log("probe err: " + e); }
  try {
    var feats = MB.nativeGetFeatureList();
    for (var i = 0; i < feats.length; i++) log("feat[" + i + "]=" + feats[i]);
  } catch (e) { log("list err: " + e); }
  var specDmg = SPEC_DMG, specRoot = SPEC_ROOT, specSpd = SPEC_SPD;
  try { MB.nativeSetValue(1, specDmg, DMG); log("nativeSetValue(1,dmg)=" + DMG); }
  catch (e) { log("setValue#1 err: " + e); }
  try { MB.nativeSetFeature(2, specRoot, ROOT); log("nativeSetFeature(2,root)=" + ROOT); }
  catch (e) { log("setFeature#2 err: " + e); }
  try { MB.nativeSetValue(3, specSpd, SPD); log("nativeSetValue(3,spd)=" + SPD); }
  catch (e) { log("setValue#3 err: " + e); }
  log("done");
});
"""


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pid", type=int, default=0, help="目标 pid，0=按包名查找")
    ap.add_argument("--dmg", type=int, default=20)
    ap.add_argument("--root", choices=["on", "off"], default="on")
    ap.add_argument("--spd", type=int, default=3)
    ap.add_argument("--wait", type=float, default=6.0, help="脚本加载后等待秒数（看日志）")
    a = ap.parse_args()

    dev = frida.get_usb_device(timeout=10)
    if a.pid:
        pid = a.pid
    else:
        pid = next((p.pid for p in dev.enumerate_processes() if p.name == PKG), 0)
        if not pid:
            sys.exit("未找到进程 %s" % PKG)
    print("[*] attach pid=%d (%s)" % (pid, PKG))
    session = dev.attach(pid)

    js = (JS.replace("SPEC_DMG", '"%s"' % SPEC_DMG)
            .replace("SPEC_ROOT", '"%s"' % SPEC_ROOT)
            .replace("SPEC_SPD", '"%s"' % SPEC_SPD)
            .replace("DMG", str(a.dmg))
            .replace("ROOT", "true" if a.root == "on" else "false")
            .replace("SPD", str(a.spd)))
    script = session.create_script(js)
    script.on("message", lambda m, d: print("[frida] %s" % (m.get("payload") or m)))
    script.load()
    time.sleep(a.wait)
    try:
        script.unload()
    except Exception:
        pass
    session.detach()
    print("[*] detached — 现在查 logcat: adb -s emulator-5554 logcat -d | grep '\\[gdzfdj\\]'")


if __name__ == "__main__":
    main()
