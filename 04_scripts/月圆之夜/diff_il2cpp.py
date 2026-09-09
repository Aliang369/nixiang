#!/usr/bin/env python3.11
# il2cpp 全 r-x/r-- 区哈希比对驱动: 内存 vs APK 文件 → 差异块回传
# 用法: /tmp/fridaenv/bin/python3.11 diff_il2cpp.py
import frida, subprocess, sys, time, os, struct, json

PKG = "com.ztgame.yyzy"
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜"
SO = "/tmp/yyzy_il2cpp/lib/arm64-v8a/libil2cpp.so"
os.makedirs(OUT, exist_ok=True)
CH = 1024 * 1024

def sh(cmd):
    return subprocess.run(["adb", "shell", cmd], capture_output=True, text=True, timeout=30).stdout.strip()

# ---- 文件侧: PHDR + 双 FNV ----
fdata = open(SO, "rb").read()
e_phoff = struct.unpack_from("<Q", fdata, 0x20)[0]
e_phnum = struct.unpack_from("<H", fdata, 0x38)[0]
segs = []
for i in range(e_phnum):
    off = e_phoff + i * 56
    p_type, p_flags = struct.unpack_from("<II", fdata, off)
    p_offset, p_vaddr, _pa, p_filesz = struct.unpack_from("<QQQQ", fdata, off + 8)
    if p_type == 1:
        segs.append((p_offset, p_vaddr, p_filesz))

def va2off(v):
    for off, va, sz in segs:
        if va <= v < va + sz:
            return off + (v - va)
    return None

PRIME1 = 16777619
PRIME2 = 2246822519
MASK = 0xFFFFFFFF

def fnv2_host(b):
    h1 = 0x811c9dc5
    h2 = 0x811c9dc5
    for x in b:
        h1 = ((h1 ^ x) * PRIME1) & MASK
        h2 = ((h2 + x) & MASK)
        h2 = (h2 * PRIME2) & MASK
    return [h1, h2]

log = []

def on_message(msg, data):
    if msg.get("type") != "send":
        if msg.get("type") == "error":
            print("[script-error]", str(msg.get("description"))[:200], flush=True)
        return
    p = msg["payload"]
    t = p.get("type")
    if t == "diffdump":
        rel = p["rel"]
        with open(f"{OUT}/il2cpp_diff.bin", "r+b" if os.path.exists(f"{OUT}/il2cpp_diff.bin") else "wb") as f:
            f.seek(rel); f.write(data)
        return
    log.append(p)
    print(f"[{t}] " + str({k: v for k, v in p.items()})[:300], flush=True)

def main():
    sh("su -c 'setenforce 0'")
    sh(f"am force-stop {PKG}")
    time.sleep(1)
    session = None
    for attempt in range(3):
        sh(f"am force-stop {PKG}")
        time.sleep(1.5)
        sh(f"monkey -p {PKG} -c android.intent.category.LAUNCHER 1")
        time.sleep(6)
        pid = int(sh(f"pidof {PKG}").split()[0])
        print(f"attach 尝试 {attempt+1}, pid={pid}")
        try:
            session = frida.get_usb_device(timeout=5).attach(pid)
            break
        except Exception as e:
            print("  失败:", e)
            session = None
    if session is None:
        print("attach 3 次均失败")
        sh("su -c 'setenforce 1'")
        sys.exit(1)
    script = session.create_script(open(os.path.join(HERE, "diff_il2cpp.js")).read())
    script.on("message", on_message)
    script.load()

    t0 = time.time()
    hashes = script.exports_sync.hash() if hasattr(script, "exports_sync") else script.exports.hash()
    print(f"哈希完成: {len(hashes)} 块, 耗时 {time.time()-t0:.1f}s")

    diffs = []
    unread = 0
    for item in hashes:
        rel, ln = item["rel"], item["len"]
        off = va2off(rel)
        if off is None:
            continue
        if item["h"] == "unreadable":
            unread += 1
            continue
        exp = fnv2_host(fdata[off:off + ln])
        if exp != item["h"]:
            diffs.append({"rel": rel, "len": ln})
    print(f"差异块: {len(diffs)}, 不可读: {unread}")
    # 去重 (同 rel 只 dump 一次)
    seen = set()
    dd = []
    for d in diffs:
        if d["rel"] in seen: continue
        seen.add(d["rel"]); dd.append(d)
    diffs = dd
    print(f"去重后: {len(diffs)}")
    json.dump(diffs, open(f"{OUT}/il2cpp_diff_list.json", "w"), indent=1)
    if diffs:
        script.exports.dumpdiff(diffs) if not hasattr(script, "exports_sync") else script.exports_sync.dumpdiff(diffs)
    script.exports.anonScan() if not hasattr(script, "exports_sync") else script.exports_sync.anonScan()
    time.sleep(8)
    try: session.detach()
    except Exception: pass
    sh(f"am force-stop {PKG}")
    sh("su -c 'setenforce 1'")
    print("getenforce =", sh("su -c 'getenforce'"))

if __name__ == "__main__":
    main()
