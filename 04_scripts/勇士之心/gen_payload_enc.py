# -*- coding: utf-8 -*-
"""兼容壳（保留文件名）——正式实现已抽成跨游戏共享件 `04_scripts/_common/gen_payload_enc.py`。

为什么留这一层：`04_scripts/勇士之心/ysxz_build_so_103.sh` 与出包门禁 C6 都按这个路径调用，
改名会动到已交付游戏的构建链。内容只有一行转发（`--game yszxz`），算法与产物逐字节不变
（2026-10-27 抽取后实测：重生成 `Includes/sg_payload_enc.h` 与旧文件 md5 一致）。

用法（与旧脚本完全一致）：
  python 04_scripts/勇士之心/gen_payload_enc.py           # 生成密文头 + 钥
  python 04_scripts/勇士之心/gen_payload_enc.py --check    # 新鲜度校验
"""
import hashlib
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "_common"))
import gen_payload_enc as G  # noqa: E402

# ---- 旧模块级 API（**兼容壳契约**）----
# 有外部脚本把本文件当模块 import 用（`04_scripts/双机协作/fetch_starry_keys.sh`
# 的自证 B 就是 `m.extract_payload() / m.load_master() / m.build_key()`）。
# 抽实现时这里必须保留同名函数，否则跨机自证脚本会 AttributeError。


def extract_payload():
    """勇士之心当前明文载荷（等价于旧实现的 R"YSXZJS(...)" 段）。"""
    payload, _ = G.load_unit(G.cfg_of("ysxz"))
    return payload


def load_master(create=True):
    return G.load_master(create=create)


def build_key(master, payload):
    """旧签名：payload 进 → (K_build, sha256(payload))。"""
    digest = hashlib.sha256(payload).digest()
    return G.build_key(master, digest), digest


def enc(key, plain):
    return G.enc(key, plain)


if __name__ == "__main__":
    sys.exit(G.main(["--game", "ysxz"] + sys.argv[1:]))
