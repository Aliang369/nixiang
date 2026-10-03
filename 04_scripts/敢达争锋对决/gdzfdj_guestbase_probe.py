"""定位 guest 地址换算：哪些映射属于 libil2cpp，4 个抓点指令的真实地址在哪。

背景：以 guest_base=0x400025000000 + RVA 直接读，UpdateAI/CalcDamage 读到的字
与 expectInsn 不一致，get_deltaTime/set_timeScale 落在 `---` 保留区（读会 access
violation）⇒ 说明 base/RVA 关系需要实证，不能假设。
本脚本：① 打印 il2cpp 相关映射（含 file.offset）；② 在可读区间里扫 4 个 expectInsn
字节串，用扫描结果反推真实地址与 base。
"""
import json
import sys
import time

import frida

SITES = [
    ("UpdateAI", 0x3D9CF54, 0xF81D0FFE),
    ("CalcDamage", 0x3DB99CC, 0xD10383FF),
    ("get_deltaTime", 0x67BE180, 0xA9BF4FFE),
    ("set_timeScale", 0x67BE458, 0xFC1E0FE8),
]

AGENT = r"""
const sites = %s;
function hex(n) { return (n >>> 0).toString(16).padStart(8, '0'); }

const ranges = [];
for (const r of Process.enumerateRanges('r--')) {
  const path = (r.file && r.file.path) ? r.file.path : null;
  if (path && (path.indexOf('il2cpp') >= 0 || path.indexOf('libunity') >= 0)) {
    ranges.push({ base: r.base, size: r.size, prot: r.protection, path: path,
                  off: (r.file && typeof r.file.offset === 'number') ? r.file.offset : null });
  }
}
send({ kind: 'ranges', ranges: ranges.map(r => ({ base: r.base.toString(), size: r.size,
      prot: r.prot, path: r.path, off: r.off })) });

// 所有可读区间（含匿名），用于扫描
const readable = [];
for (const r of Process.enumerateRanges('r--')) readable.push(r);

for (const s of sites) {
  const pat = s[2];
  const bytes = [pat & 0xff, (pat >> 8) & 0xff, (pat >> 16) & 0xff, (pat >>> 24) & 0xff];
  const patStr = bytes.map(b => b.toString(16).padStart(2, '0')).join(' ');
  let hits = [];
  for (const r of readable) {
    if (r.size > 256 * 1024 * 1024) continue;
    try {
      const res = Memory.scanSync(r.base, r.size, patStr);
      for (const m of res) hits.push(m.address.toString());
      if (hits.length > 6) break;
    } catch (e) { }
  }
  send({ kind: 'scan', name: s[0], rva: s[1], expect: hex(pat), hits: hits.slice(0, 6) });
}
""" % json.dumps([[n, r, e] for n, r, e in SITES])


def main():
    pid = int(sys.argv[1]) if len(sys.argv) > 1 else 63814
    device = frida.get_usb_device(timeout=10)
    session = device.attach(pid)
    script = session.create_script(AGENT)
    msgs = []
    script.on("message", lambda m, data: msgs.append(m))
    script.load()
    time.sleep(6.0)
    for m in msgs:
        p = m.get("payload")
        if not isinstance(p, dict):
            print(m)
            continue
        if p["kind"] == "ranges":
            print(f"[映射] 命中 {len(p['ranges'])} 个 il2cpp/libunity 相关 r-- 区间")
            for r in p["ranges"]:
                print(f"   base={r['base']} size=0x{r['size']:X} prot={r['prot']} "
                      f"fileoff={r['off'] if r['off'] is None else hex(r['off'])} {r['path']}")
        else:
            print(f"[扫描] {p['name']:<14} RVA=0x{p['rva']:X} expect={p['expect']} "
                  f"命中 {len(p['hits'])}: {p['hits']}")
    session.detach()


if __name__ == "__main__":
    main()
