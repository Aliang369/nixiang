#!/usr/bin/env python3
# -*- coding: utf-8 -*-
r"""gdzfdj_frida_probe.py — 诊断 MuMu 上 arm64 应用能否被 Frida(x86_64 server) 仪表化

用法: python 04_scripts/敢达争锋对决/gdzfdj_frida_probe.py [pid]
输出: 进程 arch / 模块总数 / 关键模块 / Java 运行时可用性 / Frida 运行时列表
"""
import sys
import time

import frida

JS = r"""
(function () {
  function keys(mods) {
    return mods.filter(function (m) {
      var n = (m.name || '').toLowerCase();
      return n.indexOf('il2cpp') >= 0 || n.indexOf('starry') >= 0 ||
             n.indexOf('unity') >= 0 || n.indexOf('art') >= 0 ||
             n.indexOf('houdini') >= 0 || n.indexOf('nemu') >= 0 ||
             n.indexOf('mono') >= 0;
    }).map(function (m) { return m.name + ' @' + m.base + ' +' + m.size; });
  }
  var mods = Process.enumerateModules();
  send({ t: 'arch', m: Process.arch + ' ptr=' + Process.pointerSize });
  send({ t: 'count', m: String(mods.length) });
  send({ t: 'key', m: keys(mods) });
  send({ t: 'first', m: mods.slice(0, 40).map(function (x) { return x.name; }) });
  send({ t: 'java', m: (typeof Java === 'undefined') ? 'NO-Java-global' : 'Java-available' });
  try { send({ t: 'rt', m: JSON.stringify(Frida.enumerateRuntimes()) }); }
  catch (e) { send({ t: 'rt', m: 'err ' + e }); }
})();
"""


def main():
    pid = int(sys.argv[1]) if len(sys.argv) > 1 else 63000
    dev = frida.get_usb_device(timeout=10)
    print("[*] attach pid=%d" % pid)
    sess = dev.attach(pid)
    sc = sess.create_script(JS)
    sc.on("message", lambda m, d: print("[probe] %s" % (m.get("payload") or m)))
    sc.load()
    time.sleep(3.0)
    sess.detach()
    print("[*] done")


if __name__ == "__main__":
    main()
