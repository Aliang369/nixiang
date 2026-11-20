#!/usr/bin/env python3
# script-policy: {"purpose": "解析 frida hook 日志（frida_run_qgsdk_ticket.py 产出），把 signMd5/sign_sha1/失败码/中台响应体抽成一张对照表，并与我们回灌的常量逐字符比对", "inputs": "13_scratch/ywt_qgsdk/*.log", "outputs": "stdout 的对照表（不写文件）", "boundary": "只读日志；不连设备、不改任何包", "prior_art": "无", "difference": "首次；针对 QGSDK 换票定位的输出格式", "evidence": "10_docs/鹦鹉团/鹦鹉团_QGSDK换票失败定位_20261119.md"}
import json
import re
import sys

EXPECT_UP = "27C2D8CA4E5703D9AD2A1EE39D2A859D"
EXPECT_LOW = "27c2d8ca4e5703d9ad2a1ee39d2a859d"
EXPECT_SHA1_UP = "35A8E7B1B9FFE25F3413BE2955C9A06B022992C9"


def main():
    path = sys.argv[1]
    txt = open(path, encoding="utf-8", errors="replace").read().splitlines()
    sign_info, params, fails, oks, reqs, resps = [], [], [], [], [], []
    for line in txt:
        if "[SIGN]" in line and "getSignInfo" in line:
            sign_info.append(line.split("getSignInfo() = ", 1)[-1].strip())
        elif "[PARAM]" in line:
            params.append(line.split("create() = ", 1)[-1].strip())
        elif "[FAIL]" in line:
            fails.append(line.split("[FAIL]", 1)[-1].strip())
        elif "[OK]" in line:
            oks.append(line.split("[OK]", 1)[-1].strip())
        elif "[HTTP-REQ]" in line:
            reqs.append(line.split("[HTTP-REQ]", 1)[-1].strip())
        elif "[HTTP-RESP]" in line:
            resps.append(line.split("[HTTP-RESP]", 1)[-1].strip())

    print("=== 1) Check.getSignInfo 返回值（%d 次）===" % len(sign_info))
    for s in sign_info[:5]:
        print("  " + s)
        try:
            arr = json.loads(s)
        except Exception:
            continue
        if len(arr) >= 4:
            print("     subject=%r" % arr[0])
            print("     issuer =%r" % arr[1])
            print("     md5    =%r  %s" % (arr[2], "== 期望(大写)" if arr[2] == EXPECT_UP
                                          else ("== 期望(小写)" if arr[2] == EXPECT_LOW else "!! 与期望都不同")))
            print("     sha1   =%r  %s" % (arr[3], "== 期望" if arr[3] == EXPECT_SHA1_UP else "!! 与期望不同"))

    print()
    print("=== 2) QGParameter.create() 里的签名参数（%d 条请求）===" % len(params))
    for p in params[:6]:
        m1 = re.search(r"(?:^|&)signMd5=([^&]*)", p)
        m2 = re.search(r"(?:^|&)sign_md5=([^&]*)", p)
        m3 = re.search(r"(?:^|&)sign_sha1=([^&]*)", p)
        print("  signMd5=%s  sign_md5=%s  sign_sha1=%s" % (
            m1.group(1) if m1 else "-", m2.group(1) if m2 else "-", m3.group(1) if m3 else "-"))
        for tag, v in (("signMd5", m1), ("sign_md5", m2)):
            if v:
                val = v.group(1)
                verdict = ("== EXPECT_UP" if val == EXPECT_UP else
                           "== EXPECT_LOW" if val == EXPECT_LOW else "!! 不等于我们的常量")
                print("     %s 逐字符比对：%s" % (tag, verdict))
        if m3:
            print("     sign_sha1 比对：%s" % ("== EXPECT" if m3.group(1) == EXPECT_SHA1_UP else "!! 不同"))

    print()
    print("=== 3) 失败/成功回调 ===")
    for f in fails:
        print("  FAIL " + f)
    for o in oks[:3]:
        print("  OK   " + o)
    if not fails and not oks:
        print("  （无）")

    print()
    print("=== 4) HTTP 请求 ===")
    for q in reqs[:12]:
        print("  " + q)
    print()
    print("=== 5) HTTP 响应（含中台 JSON）===")
    for r in resps[:12]:
        print("  " + r[:600])
    if not resps:
        print("  （无：jkhttp 可能没走 HttpClient，见文档「后续」）")
    return 0


if __name__ == "__main__":
    sys.exit(main())
