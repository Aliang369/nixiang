#!/usr/bin/env python3
"""动物大联盟 1.0.25 · libsecret.so 签名路径开关补丁（DWDLM_SEC_JNI_PATH）

照 1.0.23 的链条：壳原本从「包内 APK 的 v2 签名块」取证书派生 apksecurefile 的 KEY
（0x55a78，重签必崩）；它另有一条 PackageManager 路径（0x563ec），由 .data 的
SCFGBLOB01 记录里 +0xbd 的 1 字节开关控制：1=v2 块，0=PackageManager。

改成 0 后，配合 DwdlmAppComponentFactory 里的运行时签名回灌（官方证书），
壳就能派生出与官方一致的 KEY —— 等价于 1.0.23 的 SignSpoof 那一步。

用法：
  python3 patch_secret_jnipath_dwdlm.py <in.so> <out.so> [--expect 0|1]
退出码：0 成功；1 定位/校验失败（不写输出）
"""
import hashlib
import sys
from pathlib import Path

MAGIC = b"SCFGBLOB01"
REC_BACK = 4          # 记录基址 = magic 前 4 字节（0x66010 = 0x66015-5? 见下）
SWITCH_DELTA = 0xBD   # 基址 +0xbd = 开关字节（0x66010+0xbd = 0x660cd，实测）


def main(argv):
    if len(argv) < 3:
        print("usage: patch_secret_jnipath_dwdlm.py <in.so> <out.so> [--expect 0|1]")
        return 2
    src, dst = Path(argv[1]), Path(argv[2])
    expect = None
    if "--expect" in argv:
        expect = int(argv[argv.index("--expect") + 1])
    data = bytearray(src.read_bytes())
    print("in  : %s  %d B  sha256=%s" % (src, len(data), hashlib.sha256(data).hexdigest()[:16]))

    hits = []
    start = 0
    while True:
        i = data.find(MAGIC, start)
        if i < 0:
            break
        hits.append(i)
        start = i + 1
    if len(hits) != 1:
        print("FAIL: SCFGBLOB01 命中 %d 次（要求唯一）: %s" % (len(hits), [hex(h) for h in hits]))
        return 1
    magic_at = hits[0]
    base = magic_at - REC_BACK
    sw = base + SWITCH_DELTA
    if sw >= len(data):
        print("FAIL: 开关偏移 0x%x 越界" % sw)
        return 1
    old = data[sw]
    print("locate: magic@0x%x base=0x%x switch@0x%x 当前值=%d" % (magic_at, base, sw, old))
    if old not in (0, 1):
        print("FAIL: 开关字节不是 0/1（=0x%02x），不做猜测" % old)
        return 1
    if expect is not None and old == expect:
        print("SKIP: 已是期望值 %d" % expect)
    data[sw] = 0
    dst.write_bytes(bytes(data))
    after = bytearray(dst.read_bytes())
    assert after[sw] == 0, "写回校验失败"
    print("out : %s  sha256=%s" % (dst, hashlib.sha256(bytes(data)).hexdigest()))
    print("OK: switch %d -> 0（壳将走 PackageManager 取签名）" % old)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
