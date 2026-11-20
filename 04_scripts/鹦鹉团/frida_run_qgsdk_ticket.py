#!/usr/bin/env python3
# script-policy: {"purpose": "鹦鹉团 QGSDK 换票失败定位：attach 运行中的游戏进程，装载只读 hook（onFailed 失败码 / thridLogin 参数 / QGParameter.create 的 signMd5 / jkhttp 请求响应 / Check.getSignInfo 四元组），把消息落日志文件", "inputs": "设备 serial（默认 f9a116a2）、包名 com.jinsaina.auditionsgo.gw、frida_qgsdk_ticket.js", "outputs": "13_scratch/ywt_qgsdk/<时间>.log（每条一行，[YWT] 前缀）+ stdout 回显", "boundary": "只 attach 运行中的 pid，不 spawn、不 kill、不改逻辑、不写设备文件；超时自动 detach，游戏继续运行", "prior_art": "04_scripts/_common/start_frida_server.sh（phantom-frida 17.16.4 server）", "difference": "首次用于本包的 QGSDK 中台换票定位；只读 implementation 包装（原方法照常调用）", "evidence": "10_docs/鹦鹉团/鹦鹉团_QGSDK换票失败定位_20261119.md"}
import argparse
import os
import sys
import time

import frida

PKG = "com.jinsaina.auditionsgo.gw"
HERE = os.path.dirname(os.path.abspath(__file__))
JS = os.path.join(HERE, "frida_qgsdk_ticket.js")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--serial", default=os.environ.get("SERIAL", "f9a116a2"))
    ap.add_argument("--seconds", type=int, default=180)
    ap.add_argument("--wait", type=int, default=0,
                    help="进程不在跑时最多等这么多秒（用户手动启动游戏后自动 attach）")
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    out = args.out or os.path.join(
        os.path.dirname(HERE), "..", "13_scratch", "ywt_qgsdk",
        time.strftime("%Y%m%d_%H%M%S") + ".log")
    out = os.path.abspath(out)
    os.makedirs(os.path.dirname(out), exist_ok=True)

    dev = frida.get_device(args.serial, timeout=10)
    pid = None
    deadline = time.time() + max(0, args.wait)
    while True:
        try:
            pid = dev.get_process(PKG).pid
            break
        except Exception:
            if time.time() >= deadline:
                print("ERROR: 进程不在跑（%s）；本脚本不 spawn，请先手动启动游戏"
                      "（或加 --wait N 等它起来）" % PKG, file=sys.stderr)
                return 2
            time.sleep(1.0)
    print("[run] attach pid=%d pkg=%s -> %s" % (pid, PKG, out))

    with open(JS, encoding="utf-8") as f:
        source = f.read()

    log = open(out, "a", encoding="utf-8")

    def on_message(msg, data):
        if msg.get("type") == "log":
            line = "[YWT] " + msg.get("payload", "")
        elif msg.get("type") == "error":
            line = "[YWT][ERR] " + str(msg.get("stack") or msg.get("description"))
        else:
            line = "[YWT][?] " + str(msg)
        print(line, flush=True)
        log.write(line + "\n")
        log.flush()

    session = dev.attach(pid)
    script = session.create_script(source, runtime="v8")
    script.on("message", on_message)
    script.load()
    print("[run] hook 已装载，抓 %d 秒（Ctrl-C 提前结束；不影响游戏运行）" % args.seconds, flush=True)

    t0 = time.time()
    try:
        while time.time() - t0 < args.seconds:
            time.sleep(0.5)
    except KeyboardInterrupt:
        print("[run] 手动中断", flush=True)
    finally:
        try:
            script.unload()
        except Exception:
            pass
        try:
            session.detach()
        except Exception:
            pass
        log.close()
    print("[run] 结束，日志：%s" % out)
    return 0


if __name__ == "__main__":
    sys.exit(main())
