#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
zzz_frida_tool.py — 绝区零 3.3.0 运行期定位工具（frida 侧，只读优先）

用途（本轮 3.3.0 迁移：四个功能函数已被重写，地址类方法全失效 ⇒ 改按数据/行为定位）：
  scan   按数值扫描进程内存，得到候选地址（可多轮收敛：scan → 变化后再 scan --narrow）
  read   批量读取地址当前值（用于确认哪个候选在变）
  write  写值并回读校验（危险操作，需 --yes）
  watch  对给定 RVA 装**只读观察钩子**（frida Interceptor，自动调原函数，不改行为），统计调用次数
  ranges 列出大块可读区域（扫描前先看规模）
  watchmem 对给定**数据地址**做 MemoryAccessMonitor 读写监视（找「谁写了这个字段」= 伤害结算写点），
        逐条记录 operation / address / 访问指令 RVA / backtrace，且每次命中后自动
        disable→enable 重新 arm（frida 的监视器命中后不会自动续命，旧版只记 from 的教训）

设计约束：
  - 不扫全进程：默认只扫 16MB~512MB 的可读区域，且限制总扫描字节数（--max-mb）
  - watch 只允许函数入口（frida Interceptor 的语义要求），装前先打印字节，异常不影响游戏
  - 所有输出保持「地址 + 当前值」两列，便于人工判定

用法示例：
  python3 zzz_frida_tool.py ranges
  python3 zzz_frida_tool.py scan --value 1234 --type int --max-mb 3000 --state hp.json
  python3 zzz_frida_tool.py read --state hp.json --limit 40
  python3 zzz_frida_tool.py scan --narrow --value 987 --state hp.json
  python3 zzz_frida_tool.py watch --rva 0x1aae4d08,0x16070928 --seconds 30
  python3 zzz_frida_tool.py watchmem --addr 0x780c1c5630 --seconds 30
  python3 zzz_frida_tool.py watchmem --state hp.json --limit 1 --seconds 30

watchmem 设计约束：
  - MemoryAccessMonitor 只能按**页**（4KB）监视，命中里过滤出目标 4 字节；同页热点对象也会触发回调，
    因此必须短时监视（20~40 秒），命中/访问数超限自动收尾，避免长时间卡帧
  - 回调内部只做「记录 + send + 延时重新 arm」，绝不读目标内存（页在 PROT_NONE 下，读会二次触发）
"""
import argparse, json, os, sys, time

try:
    import frida
except ImportError:
    sys.exit("需要 frida python 模块（/opt/homebrew/bin/python3.11 下可用）")

PKG = "com.miHoYo.Nap"


# ---------------------------------------------------------------- 设备/进程
def get_pid(serial):
    dev = frida.get_usb_device(timeout=15) if serial is None else \
          frida.get_device_manager().get_device(serial, timeout=15)
    try:
        return dev, int(_adb_pid())
    except Exception:
        procs = [p for p in dev.enumerate_processes() if p.name == PKG]
        if not procs:
            sys.exit("找不到进程 %s（游戏是否在前台/存活？）" % PKG)
        return dev, procs[0].pid


def _adb_pid():
    import subprocess
    out = subprocess.run(["adb", "shell", "su", "-c", "pidof %s" % PKG],
                         capture_output=True, text=True, timeout=10).stdout.strip()
    return out.split()[0]


# ---------------------------------------------------------------- JS 片段
JS_COMMON = r"""
function rangesFiltered(minMB, maxMB){
  const out=[];
  for (const perm of ['rw-','r--']) {
    let rs=[]; try { rs = Process.enumerateRanges(perm); } catch(e) { continue; }
    for (const r of rs){
      const mb = r.size/1048576;
      if (mb < minMB || mb > maxMB) continue;
      out.push({base:r.base, size:r.size, mb:mb, perm:r.protection});
    }
  }
  out.sort((a,b)=>b.size-a.size);
  return out;
}
"""

JS_RANGES = JS_COMMON + r"""
const rs = rangesFiltered(16, 4096);
send({ranges: rs.map(r=>({base:r.base.toString(), mb:Math.round(r.mb), perm:r.perm}))});
"""

JS_SCAN = JS_COMMON + r"""
const {value, type, maxMB, limit, prev} = __ARGS__;
function pat(v, type){
  const b = new Uint8Array(8);
  if (type==='int'){ for(let i=0;i<4;i++) b[i]=(v>>(8*i))&0xff; return Array.from(b.slice(0,4)).map(x=>('0'+x.toString(16)).slice(-2)).join(' '); }
  const dv=new DataView(b.buffer); dv.setFloat32(0, v, true);
  return Array.from(b.slice(0,4)).map(x=>('0'+x.toString(16)).slice(-2)).join(' ');
}
const hex = pat(value, type);
const rs = rangesFiltered(4, 400);
let scannedMB=0; const hits=[]; let idx=0;
for (const r of rs){
  if (scannedMB > maxMB) break;
  idx++;
  let res=[];
  try { res = Memory.scanSync(r.base, r.size, hex); } catch(e) { continue; }
  scannedMB += r.size/1048576;
  send({phase:'region', idx:idx, base:r.base.toString(), mb:Math.round(r.size/1048576), scannedMB:Math.round(scannedMB), hits:hits.length+res.length});
  for (const h of res){ hits.push(h.address.toString()); if (hits.length>=limit) break; }
  if (hits.length>=limit) break;
}
let out = hits;
if (prev && prev.length){
  const keep = new Set(prev);
  out = hits.filter(a=>keep.has(a));
}
send({scannedMB: Math.round(scannedMB), total: hits.length, hits: out.slice(0, limit), filtered: out.length});
"""

JS_READ = r"""
const addrs = __ADDRS__;
const type = __TYPE__;
const res = [];
for (const a of addrs){
  try {
    const p = ptr(a);
    let v;
    if (type==='int') v = p.readS32();
    else if (type==='u32') v = p.readU32();
    else v = p.readFloat();
    res.push({addr:a, value:v});
  } catch(e) { res.push({addr:a, value:null}); }
}
send({values:res});
"""

JS_WRITE = r"""
const {addr, value, type} = __ARGS__;
let ok=false, before=null, after=null;
try{
  const p=ptr(addr);
  if (type==='int'){ before=p.readS32(); p.writeS32(value); after=p.readS32(); }
  else if (type==='u32'){ before=p.readU32(); p.writeU32(value); after=p.readU32(); }
  else { before=p.readFloat(); p.writeFloat(value); after=p.readFloat(); }
  ok=true;
}catch(e){ send({error:String(e)}); }
send({ok:ok, before:before, after:after});
"""

JS_WATCH = r"""
const {rvAs, seconds} = __ARGS__;
const mod = Process.getModuleByName('libil2cpp.so');
const base = mod.base;
const recs = [];
for (const rva of rvAs){
  const addr = base.add(rva);
  const rec = {rva:rva, addr:addr.toString(), calls:0, ok:false};
  recs.push(rec);
  try {
    const w = addr.readU32();
    rec.word = w >>> 0;
    Interceptor.attach(addr, {
      onEnter(args){
        rec.calls++;
        if (rec.calls <= 3) {
          const a=[];
          for (let i=0;i<4;i++){ try{ a.push(args[i].toString()); }catch(e){ a.push('?'); } }
          send({hit:rec.rva, n:rec.calls, args:a});
        }
      }
    });
    rec.ok = true;
  } catch(e) { rec.err = String(e); }
  send({installed:rec.rva, ok:rec.ok, word:rec.word, err:rec.err||''});
}
setTimeout(()=>{ send({summary: recs.map(r=>({rva:r.rva, ok:r.ok, calls:r.calls}))}); }, seconds*1000);
"""


JS_WATCHVALS = r"""
const {addrs, type, seconds} = __ARGS__;
const st = {};
for (const a of addrs){ st[a] = null; }
const changes = [];
function readOne(a){
  try{
    const p = ptr(a);
    if (type==='int') return p.readS32();
    if (type==='u32') return p.readU32();
    return p.readFloat();
  }catch(e){ return null; }
}
for (const a of addrs){ st[a] = readOne(a); }
const start = Date.now();
const iv = setInterval(()=>{
  for (const a of addrs){
    const v = readOne(a);
    if (v !== st[a]) {
      changes.push({t:Date.now()-start, addr:a, from:st[a], to:v});
      st[a] = v;
      if (changes.length > 4000) changes.length = 4000;
    }
  }
  if (Date.now()-start > seconds*1000){
    clearInterval(iv);
    send({changes: changes.slice(0,800), total: changes.length, seconds: seconds});
  }
}, 100);
"""


# 数据地址读写监视：MemoryAccessMonitor（页粒度，命中后过滤到目标 word）
# 关键点（上一轮踩过的坑）：
#   1) 必须记录 details.operation + details.address（只记 from 的版本线索不可用）
#   2) 命中后监视器不会自动续命 → 每次命中都 setTimeout 后 disable→enable 重新 arm
#   3) 回调运行在目标线程的信号处理里，禁止在里面读被监视页（PROT_NONE 会二次触发），
#      目标值统一在收尾（已 disable）后读取
JS_WATCHMEM = r"""
const {words, seconds, maxHits, maxAccesses, rearmDelay} = __ARGS__;
const PAGE = (typeof Process.pageSize === 'number' && Process.pageSize > 0) ? Process.pageSize : 0x1000;
const stats = {accesses:0, reads:0, writes:0, execute:0, other:0, hits:0, ignored:0, rearms:0, rearmErrors:0};
const targets = [];
for (const w of words) { targets.push(ptr(w)); }
const rmap = {};
for (const t of targets) {
  const b = t.and(ptr(PAGE - 1).not());
  rmap[b.toString()] = b;
}
const ranges = Object.keys(rmap).map(function(k){ return {base: rmap[k], size: PAGE}; });
const start = Date.now();
const records = [];
let finished = false, lastAccess = 0, rearmPending = false;

function modInfo(addr){
  try {
    const m = Process.findModuleByAddress(addr);
    if (m) return {name: m.name, rva: '0x' + addr.sub(m.base).toString(16), base: m.base.toString()};
  } catch (e) {}
  return null;
}

function matchWord(addr){
  if (!addr) return null;
  try {
    for (const t of targets) { if (addr.equals(t)) return t; }
  } catch (e) {}
  return null;
}

function finish(reason){
  if (finished) return;
  finished = true;
  setTimeout(function(){
    try { MemoryAccessMonitor.disable(); } catch (e) {}
    const vals = [];
    for (const t of targets) {
      const o = {addr: t.toString(), u32: null, s32: null};
      try { o.u32 = t.readU32(); } catch (e) {}
      try { o.s32 = t.readS32(); } catch (e) {}
      vals.push(o);
    }
    send({done: true, reason: reason, summary: stats, total: records.length,
          seconds: Math.round((Date.now() - start) / 1000),
          records: records.slice(0, 200), currentValues: vals,
          ranges: ranges.map(function(r){ return r.base.toString() + '+' + r.size; })});
  }, 80);
}

function arm(){
  if (finished) return;
  try {
    MemoryAccessMonitor.enable(ranges, {onAccess: onAccess});
    stats.rearms++;
  } catch (e) {
    stats.rearmErrors++;
    send({rearm_error: String(e), n: stats.rearmErrors});
  }
}

function rearmLater(){
  // 合并突发访问：信号风暴下也最多每 rearmDelay ms 重装一次，避免 mprotect 抖动卡帧
  if (finished || rearmPending) return;
  rearmPending = true;
  setTimeout(function(){
    rearmPending = false;
    if (finished) return;
    try { MemoryAccessMonitor.disable(); } catch (e) {}
    arm();
  }, rearmDelay);
}

function onAccess(d){
  if (finished) return;
  lastAccess = Date.now();
  stats.accesses++;
  if (stats.accesses > maxAccesses) { finish('max-accesses'); return; }
  const op = d.operation ? String(d.operation) : 'unknown';
  const t = matchWord(d.address);
  if (t === null) { stats.ignored++; rearmLater(); return; }
  if (op === 'write') stats.writes++;
  else if (op === 'read') stats.reads++;
  else if (op === 'execute') stats.execute++;
  else stats.other++;
  let instr = null, instrRva = null, modName = null, il2 = null;
  try {
    if (d.from && !d.from.isNull()) {
      instr = d.from.toString();
      const mi = modInfo(d.from);
      if (mi) { instrRva = mi.rva; modName = mi.name; }
      const il = Process.getModuleByName('libil2cpp.so');
      il2 = '0x' + d.from.sub(il.base).toString(16);
    }
  } catch (e) {}
  let bt = [];
  try { bt = Thread.backtrace(null, Backtracer.FUZZY); }
  catch (e) { try { bt = Thread.backtrace(); } catch (e2) { bt = []; } }
  const bts = [];
  for (let i = 0; i < bt.length && i < 12; i++) {
    const mm = modInfo(bt[i]);
    bts.push(mm ? (mm.name + '!' + mm.rva) : bt[i].toString());
  }
  const rec = {n: ++stats.hits, ms: Date.now() - start, op: op,
               target: t.toString(), address: d.address ? d.address.toString() : null,
               from: instr, module: modName, instr_rva: instrRva, il2cpp_rva: il2, bt: bts};
  records.push(rec);
  send({access: rec});
  if (stats.hits >= maxHits) { finish('max-hits'); return; }
  rearmLater();
}

if (typeof MemoryAccessMonitor === 'undefined') {
  send({error: 'MemoryAccessMonitor 不可用（当前 frida 版本不含该 API）'});
} else if (!targets.length) {
  send({error: '没有目标地址'});
} else {
  send({start: {targets: targets.map(function(t){ return t.toString(); }),
                ranges: ranges.map(function(r){ return r.base.toString() + '+' + r.size; }),
                seconds: seconds, maxHits: maxHits, maxAccesses: maxAccesses}});
  arm();
  setTimeout(function(){ finish('timeout'); }, seconds * 1000);
  setInterval(function(){
    if (finished) return;
    send({progress: stats, alive_ms: Date.now() - start,
          lastAccessAgo: lastAccess ? Date.now() - lastAccess : -1});
  }, 5000);
}
"""

# ---------------------------------------------------------------- 执行器
def run_script(pid, src, wait_ms=60000, print_progress=True):
    dev, _ = None, None
    # 复用外部已算好的 dev/pid
    return src  # placeholder (未使用)


def attach_and_run(dev, pid, src, wait_ms=120000, label="", on_payload=None):
    done = {"v": False}
    msgs = []

    def on_msg(m, data):
        if m["type"] == "send":
            p = m["payload"]
            msgs.append(p)
            if on_payload is not None:
                try:
                    on_payload(p)
                except Exception as e:
                    print("  [payload-print-error] %s" % e, file=sys.stderr)
            if isinstance(p, dict) and p.get("phase") == "region":
                print("  [region %d] %s %dMB 累计%.0fMB 命中%d" %
                      (p.get("idx", 0), p.get("base"), p.get("mb", 0), p.get("scannedMB", 0), p.get("hits", 0)),
                      flush=True)
            if isinstance(p, dict) and (("done" in p) or ("summary" in p) or ("values" in p) or ("ranges" in p) or ("hits" in p and "total" in p)):
                done["v"] = True
        else:
            print("  [script-error] %s" % m.get("description"), file=sys.stderr)
            done["v"] = True

    s = dev.attach(pid)
    sc = s.create_script(src)
    sc.on("message", on_msg)
    t0 = time.time()
    sc.load()
    while not done["v"] and (time.time() - t0) * 1000 < wait_ms:
        time.sleep(0.2)
    return msgs


def resolve_watchmem_targets(a):
    """watchmem 的地址解析与合法性校验（在 attach 之前完成，坏地址不碰设备）。"""
    addrs = []
    if a.state:
        if not os.path.exists(a.state):
            sys.exit("state 文件不存在：%s（先跑 scan --state ...）" % a.state)
        addrs = json.load(open(a.state)).get("hits", [])
    if a.addr:
        addrs += [x for x in a.addr.split(",") if x.strip()]
    addrs = addrs[: max(1, a.limit)]
    if not addrs:
        sys.exit("没有目标地址：用 --addr 0x... 或 --state <scan.json>")
    words = []
    for x in addrs:
        try:
            v = int(x, 16)
        except ValueError:
            sys.exit("地址格式非法：%r（要十六进制，如 0x780c1c5630）" % x)
        if v <= 0x10000 or v >= (1 << 48):
            sys.exit("地址不像用户态内存：%#x（拒绝对明显非法地址 arm）" % v)
        words.append(hex(v))
    return words


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--serial", default=os.environ.get("ANDROID_SERIAL"))
    sub = ap.add_subparsers(dest="cmd", required=True)

    p = sub.add_parser("ranges")
    p = sub.add_parser("scan")
    p.add_argument("--value", required=True)
    p.add_argument("--type", default="int", choices=["int", "float"])
    p.add_argument("--max-mb", type=int, default=2000)
    p.add_argument("--limit", type=int, default=400)
    p.add_argument("--narrow", action="store_true", help="只保留上次结果里的地址")
    p.add_argument("--state", default="zzz_scan.json")

    p = sub.add_parser("read")
    p.add_argument("--state", default="zzz_scan.json")
    p.add_argument("--limit", type=int, default=40)
    p.add_argument("--type", default="int", choices=["int", "float", "u32"])

    p = sub.add_parser("write")
    p.add_argument("--addr", required=True)
    p.add_argument("--value", required=True)
    p.add_argument("--type", default="int", choices=["int", "float", "u32"])
    p.add_argument("--yes", action="store_true")

    p = sub.add_parser("watchvals")
    p.add_argument("--state", default=None, help="scan 产生的 state 文件（取其 hits）")
    p.add_argument("--addr", default=None, help="或直接给逗号分隔地址")
    p.add_argument("--type", default="int", choices=["int", "float", "u32"])
    p.add_argument("--limit", type=int, default=200)
    p.add_argument("--seconds", type=int, default=60)

    p = sub.add_parser("watch")
    p.add_argument("--rva", required=True, help="逗号分隔的 RVA 列表（十六进制）")
    p.add_argument("--seconds", type=int, default=30)

    p = sub.add_parser("watchmem", help="监视数据地址的读写，找写点（伤害结算函数）")
    p.add_argument("--addr", default=None, help="逗号分隔的目标地址（十六进制），如 0x780c1c5630")
    p.add_argument("--state", default=None, help="或从 scan 的 state 文件取 hits（配合 --limit）")
    p.add_argument("--limit", type=int, default=4, help="从 state 取前 N 个地址（默认 4）")
    p.add_argument("--seconds", type=int, default=30, help="监视时长，建议 20~40")
    p.add_argument("--max-hits", type=int, default=200, help="命中上限，超过即收尾")
    p.add_argument("--max-accesses", type=int, default=20000,
                   help="同页总访问上限（每页任意地址访问都算），防止页太热卡帧")
    p.add_argument("--rearm-delay", type=int, default=10, help="命中后重新 arm 的延时 ms")

    a = ap.parse_args()
    if a.cmd == "watchmem":
        a.words = resolve_watchmem_targets(a)
    dev, pid = get_pid(a.serial)

    if a.cmd == "ranges":
        msgs = attach_and_run(dev, pid, JS_RANGES, 30000)
        for m in msgs:
            if "ranges" in m:
                print("%-14s %6s MB" % ("base", "size"))
                for r in m["ranges"][:30]:
                    print("%-14s %6s  %s" % (r["base"], r["mb"], r["perm"]))
        return

    if a.cmd == "scan":
        prev = []
        if a.narrow and os.path.exists(a.state):
            prev = json.load(open(a.state)).get("hits", [])
        args = {"value": int(a.value) if a.type == "int" else float(a.value),
                "type": a.type, "maxMB": a.max_mb, "limit": a.limit, "prev": prev}
        src = JS_SCAN.replace("__ARGS__", json.dumps(args))
        msgs = attach_and_run(dev, pid, src, 600000)
        for m in msgs:
            if "hits" in m and "total" in m:
                print("扫描 %.0f MB，原始命中 %d，收敛后 %d" % (m["scannedMB"], m["total"], m["filtered"]))
                for h in m["hits"][:60]:
                    print("  %s" % h)
                json.dump({"value": args["value"], "type": a.type, "hits": m["hits"]},
                          open(a.state, "w"))
                print("状态已存 %s（%d 条）" % (a.state, len(m["hits"])))
        return

    if a.cmd == "read":
        st = json.load(open(a.state))
        addrs = st["hits"][: a.limit]
        ty = a.type if a.type else st.get("type", "int")
        src = JS_READ.replace("__ADDRS__", json.dumps(addrs)).replace("__TYPE__", json.dumps(ty))
        msgs = attach_and_run(dev, pid, src, 60000)
        for m in msgs:
            if "values" in m:
                for r in m["values"][:80]:
                    print("  %-16s %s" % (r["addr"], r["value"]))
        return

    if a.cmd == "write":
        if not a.yes:
            sys.exit("写内存属危险操作：确认无误后加 --yes")
        args = {"addr": a.addr, "value": int(a.value) if a.type != "float" else float(a.value), "type": a.type}
        src = JS_WRITE.replace("__ARGS__", json.dumps(args))
        msgs = attach_and_run(dev, pid, src, 30000)
        for m in msgs:
            print(m)
        return

    if a.cmd == "watchvals":
        addrs = []
        if a.state:
            addrs = json.load(open(a.state)).get("hits", [])
        if a.addr:
            addrs += [x for x in a.addr.split(",") if x.strip()]
        addrs = addrs[: a.limit]
        if not addrs:
            sys.exit("没有候选地址（先跑 scan --state ...）")
        args = {"addrs": addrs, "type": a.type, "seconds": a.seconds}
        src = JS_WATCHVALS.replace("__ARGS__", json.dumps(args))
        print("监视 %d 个地址 %d 秒（游戏内请制造变化）..." % (len(addrs), a.seconds), flush=True)
        msgs = attach_and_run(dev, pid, src, (a.seconds + 90) * 1000)
        for m in msgs:
            if "changes" in m:
                print("变化条目 %d（显示前 %d）" % (m["total"], len(m["changes"])))
                seen = {}
                for c in m["changes"]:
                    seen.setdefault(c["addr"], []).append(c)
                for addr, cs in sorted(seen.items(), key=lambda kv: -len(kv[1]))[:25]:
                    seq = " -> ".join("%s" % (c["to"],) for c in cs[:6])
                    print("  %-16s 变化%3d次  %s" % (addr, len(cs), seq))
        return

    if a.cmd == "watchmem":
        words = a.words
        args = {"words": words, "seconds": a.seconds, "maxHits": a.max_hits,
                "maxAccesses": a.max_accesses, "rearmDelay": a.rearm_delay}
        src = JS_WATCHMEM.replace("__ARGS__", json.dumps(args))

        def _p(p):
            if not isinstance(p, dict):
                return
            if "start" in p:
                print("  已 arm：目标 %s，页 %s，%d 秒（maxHits=%s maxAccesses=%s）" %
                      (",".join(p["start"].get("targets", [])),
                       ",".join(p["start"].get("ranges", [])), p["start"].get("seconds", 0),
                       p["start"].get("maxHits"), p["start"].get("maxAccesses")), flush=True)
            elif "access" in p:
                r = p["access"]
                print("  [%7.2fs] op=%-7s addr=%s  by %s!%s  il2cpp_rva=%s" %
                      ((r.get("ms") or 0) / 1000.0, r.get("op"), r.get("address"),
                       r.get("module"), r.get("instr_rva"), r.get("il2cpp_rva")), flush=True)
                for i, f in enumerate((r.get("bt") or [])[:8]):
                    print("        #%-2d %s" % (i, f), flush=True)
            elif "progress" in p:
                st = p.get("progress") or {}
                print("  .. %4.0fs 访问%d（读%d/写%d）命中%d 忽略%d rearm%d 最后访问 %sms 前" %
                      ((p.get("alive_ms") or 0) / 1000.0, st.get("accesses", 0), st.get("reads", 0),
                       st.get("writes", 0), st.get("hits", 0), st.get("ignored", 0),
                       st.get("rearms", 0), p.get("lastAccessAgo")), flush=True)
            elif "rearm_error" in p:
                print("  [rearm失败] %s (x%s)" % (p.get("rearm_error"), p.get("n")), flush=True)
            elif "error" in p:
                print("  [error] %s" % p.get("error"), flush=True)

        print("监视 %d 个 word（%s），%d 秒 —— 现在请在游戏内**挨一下**（触发 HP 写入）" %
              (len(words), ",".join(words), a.seconds), flush=True)
        msgs = attach_and_run(dev, pid, src, (a.seconds + 120) * 1000, on_payload=_p)
        fin = None
        for m in msgs:
            if isinstance(m, dict) and m.get("done"):
                fin = m
        if fin is None:
            print("没有收到收尾消息（脚本报错/被杀/未 attach 成功），以上为已有输出", file=sys.stderr)
            return
        st = fin.get("summary") or {}
        print("\n收尾 reason=%s：访问 %d（读 %d / 写 %d / exec %d / 其他 %d）命中 %d 忽略 %d "
              "rearm %d 次（失败 %d）" %
              (fin.get("reason"), st.get("accesses", 0), st.get("reads", 0), st.get("writes", 0),
               st.get("execute", 0), st.get("other", 0), st.get("hits", 0),
               st.get("ignored", 0), st.get("rearms", 0), st.get("rearmErrors", 0)))
        for cv in fin.get("currentValues") or []:
            print("  当前值 %s  u32=%s s32=%s" % (cv.get("addr"), cv.get("u32"), cv.get("s32")))
        writes = [r for r in (fin.get("records") or []) if r.get("op") == "write"]
        if not writes:
            print("  **未捕获 write**：① 这段期间没有发生伤害结算；② 该地址只是血条副本、"
                  "真结算写的是另一份字段；③ 同页太热已提前收尾（看 reason=max-accesses）\n"
                  "  下一步换法：hook 读点 0x15075418（onEnter 记录 x0/x1 与对象字段）反推真实字段，"
                  "或缩短到 10 秒、只留一次伤害再抓。")
        for r in writes[:20]:
            print("  WRITE %s  由 %s!%s（il2cpp_rva=%s）" %
                  (r.get("target"), r.get("module"), r.get("instr_rva"), r.get("il2cpp_rva")))
            for i, f in enumerate((r.get("bt") or [])[:8]):
                print("        #%-2d %s" % (i, f))
        return

    if a.cmd == "watch":
        rvAs = [int(x, 16) for x in a.rva.split(",") if x.strip()]
        args = {"rvAs": rvAs, "seconds": a.seconds}
        src = JS_WATCH.replace("__ARGS__", json.dumps(args))
        msgs = attach_and_run(dev, pid, src, (a.seconds + 60) * 1000)
        for m in msgs:
            if "installed" in m:
                print("install rva=%#x ok=%s word=%s %s" % (m["installed"], m["ok"], m.get("word", ""), m.get("err", "")))
            elif "hit" in m:
                print("  HIT rva=%#x n=%d args=%s" % (m["hit"], m["n"], m.get("args")))
            elif "summary" in m:
                print("汇总：", ", ".join("rva=%#x calls=%d" % (r["rva"], r["calls"]) for r in m["summary"]))
        return


if __name__ == "__main__":
    main()
