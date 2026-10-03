#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""敢达争锋对决 · 钩子落点内存探针（无需 Java 桥、无需 UI 操作）

原理
----
星云/starry 框架的钩子 = 在 guest COW 页里把目标函数头 4 字节改写成 `B <imm26>`
（内联 veneer），磁盘上的 libil2cpp.so 不变。所以比较「进程内 guest 虚拟地址处的
4 字节」与「原始指令字」即可判定钩子是否已挂：

  * 内存 == 原始指令  -> 该抓点未挂钩（功能全关时的预期状态）
  * 内存 != 原始指令  -> 已被改写；解码 B 的 imm26 可得到跳板地址

guest 地址换算：guest_base(0x400023600000) + RVA，其中 RVA 取自
`07_patched/敢达争锋对决/dump_out/dump.cs`（Il2CppDumper 的 RVA 列 = ELF p_vaddr）。

base 的实证来源（gdzfdj_guestbase_probe.py，libil2cpp.so 的 guest 映射）：
  r-- 0x400023600000 +0x26E5000 fileoff 0x0        -> vaddr 0        => base = 0x400023600000
  r-- 0x400025ce8000 +0x43C3000 fileoff 0x26e4000  -> vaddr 0x26E8000 （= file_off + 0x4000）
  r-- 0x40002a0ae000 +0x463000  fileoff 0x6aa6000  -> vaddr 0x6AAE000
  rw- 0x40002a514000 +0x4B4000  fileoff 0x6f08000  -> vaddr 0x6F0C000
三处一致给出 base = 0x400023600000（旧记录 0x400025000000 是错的，会让 0x67BE1xx
落到 `---` 保留区而读失败）。

用法
----
  D:\\Python313\\python.exe gdzfdj_hookprobe.py --pid 63814
  D:\\Python313\\python.exe gdzfdj_hookprobe.py --base 0x400023600000

判据
----
功能全关（伤害倍率=1 / 超级定怪=OFF / 全局加速=1）时四点应全部 == 原始指令；
把任一功能拨开后重跑，对应抓点应变为 != 原始指令（且能解出 B 目标）。
"""
import argparse
import json
import sys

import frida

# (名字, RVA, 原始 4 字节小端字, 说明)
SITES = [
    ("xRole$$UpdateAI",      0x3D9CF54, 0xF81D0FFE, "定怪：str x30,[sp,#-0x30]!"),
    ("xRole$$CalcDamage",    0x3DB99CC, 0xD10383FF, "伤害：sub sp,sp,#0xe0"),
    ("Time$$get_deltaTime",  0x67BE180, 0xA9BF4FFE, "加速：stp x30,x19,[sp,#-0x10]!"),
    ("Time$$set_timeScale",  0x67BE458, 0xFC1E0FE8, "加速：str d8,[sp,#-0x20]!"),
]
DEFAULT_BASE = 0x400023600000

AGENT = r"""
const req = %s;
rpc.exports = {
  probe() {
    const out = [];
    for (const s of req.sites) {
      const addr = ptr(s.base).add(s.rva);
      const row = { name: s.name, rva: s.rva, addr: addr.toString(), expect: s.expect,
                    word: null, err: null, range: null };
      try {
        const r = Process.findRangeByAddress(addr);
        row.range = r ? (r.base + "+0x" + r.size.toString(16) + " " + r.protection +
                         (r.file ? " " + r.file.path : "")) : "no-range";
      } catch (e) { row.range = "rangeErr:" + e; }
      try { row.word = addr.readU32() >>> 0; } catch (e) { row.err = String(e); }
      out.push(row);
    }
    return out;
  }
};
"""


def decode_branch(word, pc):
    """B 的 opcode=0b000101，imm26 为 PC 相对字偏移。返回跳转目标或 None。"""
    if (word >> 26) != 0b000101:
        return None
    imm26 = word & 0x03FFFFFF
    if imm26 & (1 << 25):
        imm26 -= 1 << 26
    return pc + imm26 * 4


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--pid", type=int, default=0)
    ap.add_argument("--base", default=hex(DEFAULT_BASE))
    a = ap.parse_args()

    base = int(a.base, 16)
    pid = a.pid
    device = frida.get_usb_device(timeout=10)
    if not pid:
        pid = next((app.pid for app in device.enumerate_applications()
                    if app.identifier == "com.kingnet.gundam.mg.normal"), 0)
        if not pid:
            sys.exit("未找到 com.kingnet.gundam.mg.normal 进程；请用 --pid 指定")

    session = device.attach(pid)
    payload = {"sites": [{"name": n, "rva": r, "expect": e, "base": hex(base)}
                         for n, r, e, _ in SITES]}
    script = session.create_script(AGENT % json.dumps(payload))
    script.load()
    rows = script.exports_sync.probe()

    print(f"[*] pid={pid} guest_base={hex(base)}")
    print(f"{'抓点':<22}{'guest_addr':<18}{'内存字':<12}{'原始字':<12}判定")
    hooked = clean = errs = 0
    for r in rows:
        if r["err"]:
            print(f"{r['name']:<22}{r['addr']:<18}{'--':<12}{r['expect']:08X}    读取失败: {r['err']}")
            print(f"{'':<22}range={r['range']}")
            errs += 1
            continue
        w = r["word"]
        tgt = decode_branch(w, int(r["addr"], 16))
        if w == r["expect"]:
            verdict = "未挂钩(原始指令)"
            clean += 1
        else:
            verdict = "★已挂钩" + (f" -> B {hex(tgt)}" if tgt else f" (非 B：{w:08X})")
            hooked += 1
        print(f"{r['name']:<22}{r['addr']:<18}{w:08X}    {r['expect']:08X}    {verdict}")

    print(f"[*] 汇总：已挂钩 {hooked} / 原始 {clean} / 读取失败 {errs}"
          f"（功能全关时应为 已挂钩 0）")
    session.detach()


if __name__ == "__main__":
    main()
