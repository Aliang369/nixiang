#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
xhzm_cdn_server.py —— 伪造热更 CDN 应答服务器（免 root 投递的落地端）。

作用：监听 :80，把 install2.hotune.com 的**三个清单/bundle 请求**换成我们伪造的
文件（目录 ./overrides/），其余任何请求原样反代到真实 CDN。
配合「VPN 只劫持 install2.hotune.com 的 DNS → 本机 IP」，客户端就会自己把补丁
bundle 写进它自己的 Android/data，全程不需要 root、不需要电脑。

覆盖规则：按**文件名**匹配（basename），因此 /bundle3/Android/x 与
/bundle3/2.6.53.13/Android/x 都能命中，无需关心客户端用哪种 URL 形态。

用法：
    python3 xhzm_cdn_server.py --root /opt/xhzm_cdn --port 80
"""
import argparse
import http.client
import os
import socket
import socketserver
import sys
import threading
from http.server import BaseHTTPRequestHandler

ROOT = "/opt/xhzm_cdn"
OVERRIDE_DIR = "overrides"
UPSTREAM_HOST = "install2.hotune.com"
UPSTREAM_PORT = 80
LOG_LOCK = threading.Lock()

# 真实 CDN 用系统 DNS 解析（本服务不劫持自己），结果缓存
_upstream_ip = None


def upstream_ip(force=False):
    global _upstream_ip
    if _upstream_ip is None or force:
        ip = socket.gethostbyname(UPSTREAM_HOST)
        _upstream_ip = ip
    return _upstream_ip


def log(msg):
    with LOG_LOCK:
        sys.stdout.write(msg + "\n")
        sys.stdout.flush()


def override_path(basename):
    p = os.path.join(ROOT, OVERRIDE_DIR, basename)
    return p if os.path.isfile(p) else None


class Handler(BaseHTTPRequestHandler):
    protocol_version = "HTTP/1.1"
    server_version = "starry-cdn/1.0"

    def log_message(self, fmt, *args):  # 静音默认日志，用我们自己的
        pass

    def _host(self):
        return (self.headers.get("Host") or "").split(":")[0].lower()

    def _dispatch(self, method):
        path = self.path
        basename = path.split("?")[0].rstrip("/").split("/")[-1]
        ov = override_path(basename)
        if ov:
            data = open(ov, "rb").read()
            ctype = "application/json; charset=utf-8" if basename.endswith(".json") else "application/octet-stream"
            self.send_response(200)
            self.send_header("Content-Type", ctype)
            self.send_header("Content-Length", str(len(data)))
            self.send_header("Accept-Ranges", "bytes")
            self.end_headers()
            if method != "HEAD":
                self.wfile.write(data)
            log(f"[FORGED] {method} {self._host()}{path} -> 200 {len(data)}B ({basename})")
            return

        # 其余一律反代真实 CDN
        try:
            conn = http.client.HTTPConnection(upstream_ip(), UPSTREAM_PORT, timeout=30)
            hdrs = {k: v for k, v in self.headers.items()}
            hdrs["Host"] = UPSTREAM_HOST
            conn.request(method, path, body=None, headers=hdrs)
            resp = conn.getresponse()
            body = resp.read()
            self.send_response(resp.status)
            for k, v in resp.getheaders():
                if k.lower() in ("connection", "transfer-encoding", "keep-alive"):
                    continue
                self.send_header(k, v)
            self.send_header("Content-Length", str(len(body)))
            self.end_headers()
            if method != "HEAD":
                self.wfile.write(body)
            log(f"[proxy ] {method} {self._host()}{path} -> {resp.status} {len(body)}B")
        except Exception as e:
            log(f"[ERROR ] {method} {self._host()}{path} -> {e!r}")
            try:
                self.send_error(502, "upstream error")
            except Exception:
                pass

    def do_GET(self):
        self._dispatch("GET")

    def do_HEAD(self):
        self._dispatch("HEAD")

    def do_POST(self):
        self._dispatch("POST")


class Server(socketserver.ThreadingMixIn, socketserver.TCPServer):
    allow_reuse_address = True
    daemon_threads = True


def main():
    global ROOT
    ap = argparse.ArgumentParser()
    ap.add_argument("--root", default=ROOT)
    ap.add_argument("--port", type=int, default=80)
    a = ap.parse_args()
    ROOT = a.root
    ovd = os.path.join(ROOT, OVERRIDE_DIR)
    if not os.path.isdir(ovd):
        sys.exit(f"缺少覆盖目录 {ovd}")
    names = sorted(os.listdir(ovd))
    log(f"[boot  ] root={ROOT} port={a.port} upstream={UPSTREAM_HOST} -> {upstream_ip()}")
    log(f"[boot  ] 覆盖文件({len(names)}): " + ", ".join(names))
    Server(("0.0.0.0", a.port), Handler).serve_forever()


if __name__ == "__main__":
    main()
