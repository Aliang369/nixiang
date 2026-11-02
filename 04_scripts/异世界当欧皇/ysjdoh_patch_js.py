#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""异世界当欧皇 (com.cxgame.ysjdoh 1.0.104) —— Cocos Creator 2.x 脚本补丁
流程：XXTEA(key=kesdugQNl6HuX2wv, padding=False) -> gunzip -> 最小改动 -> gzip -> XXTEA
锚点来源：10_docs/_batch_binge6/六款斌哥改包_抓点总表_20261102.md §二.1 + 本地反编译实测
功能：伤害倍率(2-100) / 超级无敌 / 战斗加速(5x) / 免广告(激励视频直接发奖 + 不弹插屏)

用法：python3 04_scripts/异世界当欧皇/ysjdoh_patch_js.py [--verify-only]
产物：13_scratch/ysjdoh_js/index.patched.jsc
"""
import gzip, io, struct, sys, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
APK = ROOT / "02_apks" / "异世界当欧皇_官方_com.cxgame.ysjdoh.apk"
ENTRY = "assets/assets/scripts/index.jsc"
WORK = ROOT / "13_scratch" / "ysjdoh_js"
OUT_JSC = WORK / "index.patched.jsc"
KEY = b"kesdugQNl6HuX2wv"
DELTA = 0x9E3779B9
M = 0xFFFFFFFF
MARK = "/*starry_ysjdoh*/"
MULT_MAX = 100

# ---- 锚点（各唯一命中）----
ANCHOR_ENEMY = ("t.prototype.Hurt = function(t) {\n"
                "if (!this.IsDie && null != t) if (t.Damage.gt(0) && this.IsHasBuff(p.BuffEffectType.Invincible)) {")
ANCHOR_HERO = ("t.prototype.Hurt = function(t) {\n"
               "if (!this.IsDie && null != t) if (t.Damage.gt(0) && this.IsHasBuff(l.BuffEffectType.Invincible)) {")
ANCHOR_SPD = "this.SimulationSpeed = 1;"
ANCHOR_VIDEO = "t.prototype.ShowVideo = function(e, t, n, i, a) {\nvar o = this;"
ANCHOR_INTER = "t.prototype.ShowInter = function(e, t, n) {\nvoid 0 === t && (t = 0);"

# ---- 补丁片段 ----
P_MULT = ("try{if(window.__sby&&window.__sby.mult>1&&t&&t.Damage&&t.Damage.gt(0)&&t.Damage.mul){"
          "t.Damage=t.Damage.mul(d.default.value(window.__sby.mult));"
          "window.__sby.mn=(window.__sby.mn||0)+1;}}catch(_sbyE){}")
P_INV = ("try{if(window.__sby&&window.__sby.inv&&t&&t.Damage&&t.Damage.gt(0)){"
         "t.Damage=f.default.value(0);window.__sby.iv=(window.__sby.iv||0)+1;}}catch(_sbyE){}")
P_SPD = ("this.SimulationSpeed = 1;try{var _sbyV=1;Object.defineProperty(this,\"SimulationSpeed\","
         "{configurable:!0,enumerable:!0,get:function(){var o=window.__sby;"
         "return o&&o.spd?5:_sbyV},set:function(v){_sbyV=v}});}catch(_sbyE){}")
# 免广告：复刻游戏自带 FreeADActive 分支（直接发奖，不拉广告）
P_NOAD = ("try{if(window.__sby&&window.__sby.noad){this.OnVideoAdEvent_OnClose(!0,e);"
          "n&&n(!0);window.__sby.an=(window.__sby.an||0)+1;return;}}catch(_sbyE){}")
P_INTER = "try{if(window.__sby&&window.__sby.noad)return;}catch(_sbyE){}"

BOOT = ("\n;" + MARK + "\n(function(){try{if(window.__sbyBoot)return;window.__sbyBoot=1;"
        "var S=window.__sby={mult:0,inv:0,spd:0,noad:0},K=\"starry_ysjdoh\";"
        "function rd(){try{if(\"undefined\"==typeof cc||!cc.sys||!cc.sys.localStorage)return;"
        "var s=cc.sys.localStorage.getItem(K);if(!s)return;var o=JSON.parse(s);"
        "var m=parseInt(o.mult,10)||0;if(m<0)m=0;if(m>%d)m=%d;"
        "S.mult=m;S.inv=o.inv?1:0;S.spd=o.spd?1:0;S.noad=o.noad?1:0;"
        "try{cc.sys.localStorage.setItem('starry_alive',''+Date.now()+'|'+S.mult+'|'+S.inv+'|'+S.spd+'|'+S.noad);}catch(_sbyE){}"
        "}catch(_sbyE){}}setInterval(rd,3000);rd();}catch(_sbyE){}})();\n") % (MULT_MAX, MULT_MAX)


def _k(endian="<"):
    return list(struct.unpack(endian + "4I", KEY))


def xxtea_decrypt(data: bytes, endian="<"):
    L = len(data) // 4
    v = list(struct.unpack(endian + str(L) + "I", data))
    k = _k(endian)
    n = L - 1
    rounds = 6 + 52 // (n + 1)
    s = (rounds * DELTA) & M
    y = v[0]
    while s:
        e = (s >> 2) & 3
        for p in range(n, 0, -1):
            z = v[p - 1]
            mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) & M) ^ (((s ^ y) + (k[(p & 3) ^ e] ^ z)) & M)
            v[p] = (v[p] - mx) & M
            y = v[p]
        z = v[n]
        mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) & M) ^ (((s ^ y) + (k[e] ^ z)) & M)
        v[0] = (v[0] - mx) & M
        y = v[0]
        s = (s - DELTA) & M
    return struct.pack(endian + str(L) + "I", *v), v[n]


def xxtea_encrypt(payload: bytes, endian="<"):
    n = (len(payload) + 3) // 4
    pad = payload + b"\x00" * (n * 4 - len(payload))
    v = list(struct.unpack(endian + str(n) + "I", pad)) + [len(payload)]
    k = _k(endian)
    rounds = 6 + 52 // (n + 1)
    s = 0
    z = v[n]
    while rounds:
        s = (s + DELTA) & M
        e = (s >> 2) & 3
        for p in range(n):
            y = v[p + 1]
            mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) & M) ^ (((s ^ y) + (k[(p & 3) ^ e] ^ z)) & M)
            z = v[p] = (v[p] + mx) & M
        y = v[0]
        p = n
        mx = ((((z >> 5) ^ ((y << 2) & M)) + ((y >> 3) ^ ((z << 4) & M))) & M) ^ (((s ^ y) + (k[(p & 3) ^ e] ^ z)) & M)
        z = v[n] = (v[n] + mx) & M
        rounds -= 1
    return struct.pack(endian + str(n + 1) + "I", *v)


def dec_jsc(enc: bytes) -> str:
    packed, real_len = xxtea_decrypt(enc)
    return gzip.decompress(packed[:real_len]).decode("utf-8")


def enc_jsc(js: str) -> bytes:
    buf = io.BytesIO()
    with gzip.GzipFile(fileobj=buf, mode="wb", compresslevel=9, mtime=0) as gz:
        gz.write(js.encode("utf-8"))
    return xxtea_encrypt(buf.getvalue())


def _at_head(anchor, code):
    """把补丁插在函数第一行之后、原逻辑之前。
    注意：ANCHOR_* 覆盖到了 if 语句的 {，直接拼会插进 if 分支里（曾经踩过）。"""
    head = "t.prototype.Hurt = function(t) {" + chr(10)
    if not anchor.startswith(head):
        raise SystemExit("锚点不以函数头开始: %r" % anchor[:40])
    return head + code + chr(10) + anchor[len(head):]


def main():
    if not APK.exists():
        raise SystemExit("缺官方包: %s" % APK)
    with zipfile.ZipFile(APK) as z:
        enc = z.read(ENTRY)
    js = dec_jsc(enc)
    print("解密 OK: %d 字符" % len(js))

    if dec_jsc(enc_jsc(js)) != js:
        raise SystemExit("回转自检失败（解密/加密不可逆）")
    print("回转自检 OK")

    anchors = [("enemy", ANCHOR_ENEMY), ("hero", ANCHOR_HERO), ("spd", ANCHOR_SPD),
               ("video", ANCHOR_VIDEO), ("inter", ANCHOR_INTER)]
    for name, a in anchors:
        c = js.count(a)
        print("  锚点 %-6s 命中 %d" % (name, c))
        if c != 1:
            raise SystemExit("锚点 %s 命中数 %d != 1" % (name, c))
    if "--verify-only" in sys.argv:
        return

    patched = js.replace(ANCHOR_ENEMY, _at_head(ANCHOR_ENEMY, P_MULT), 1)
    patched = patched.replace(ANCHOR_HERO, _at_head(ANCHOR_HERO, P_INV), 1)
    patched = patched.replace(ANCHOR_SPD, P_SPD, 1)
    patched = patched.replace(ANCHOR_VIDEO, ANCHOR_VIDEO + P_NOAD, 1)
    patched = patched.replace(ANCHOR_INTER, ANCHOR_INTER + P_INTER, 1)
    if MARK not in patched:
        patched = patched + BOOT

    for frag in ("window.__sby.noad", "OnVideoAdEvent_OnClose(!0,e)", "starry_ysjdoh",
                 "S.noad=o.noad?1:0", "o&&o.spd?5:_sbyV"):
        if frag not in patched:
            raise SystemExit("补丁片段缺失: %s" % frag)

    out = enc_jsc(patched)
    WORK.mkdir(parents=True, exist_ok=True)
    OUT_JSC.write_bytes(out)
    if dec_jsc(out) != patched:
        raise SystemExit("产物自检失败")
    print("产物 OK: %s (%d B, 明文 %d 字符)" % (OUT_JSC.relative_to(ROOT), len(out), len(patched)))


if __name__ == "__main__":
    main()
