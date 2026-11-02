#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""不一样的修仙宗门2 —— 直接改 assets/src/project.jsc（Cocos 2.x XXTEA 脚本）
原因：本包只有 arm64 so，雷电用 houdini 转译；native inline hook 与 Java evalString 注入
都会在转译层 SIGSEGV（2026-11-02 实测两次）。改脚本 = 唯一稳定路径。

流程：xxtea 解密 -> 解 zip -> 取 encrypt.js -> 末尾追加补丁 -> 重新 zip -> xxtea 加密
用法：python 04_scripts/不一样的修仙宗门2/byydxxzm2_patch_js.py
产物：13_scratch/byydxxzm2_js/project.jsc（由 surgical 脚本塞回 apk）
"""
import io, os, struct, zipfile, hashlib
from pathlib import Path

ROOT = Path(r"E:\nixiang")
ORIG_APK = ROOT / "02_apks" / "不一样的修仙宗门2" / "byydxxzm_tap_original.apk"
OUTDIR = ROOT / "13_scratch" / "byydxxzm2_js"
KEY = b"0ab97bfb-1bc5-4a"
DELTA = 0x9E3779B9
M = 0xFFFFFFFF
MARK = "/*starry_byydxxzm2*/"


def _k(endian):
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
    return struct.pack(endian + str(L) + "I", *v), n, v[n]


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


PATCH_JS = r"""
;/*starry_byydxxzm2*/
(function(){
  try{
    if(window.__sby) return;
    var S = window.__sby = {yu:true, ling:true, noad:true};
    S.patch = function(){
      try{
        var q = window.qingyunzhi;
        if(!q || !q.base) return;
        var b = q.base;
        if(b.__sbyBase) return;
        var g = b.gold, d = b.diamond;
        Object.defineProperty(b,'gold',{configurable:true,enumerable:true,
          get:function(){ return S.ling ? 999999999 : g; },
          set:function(v){ g = v; }});
        Object.defineProperty(b,'diamond',{configurable:true,enumerable:true,
          get:function(){ return S.yu ? 999999999 : d; },
          set:function(v){ d = v; }});
        b.__sbyBase = 1;
      }catch(e){}
    };
    S.readCfg = function(){
      try{
        var t = cc.sys.localStorage.getItem('starry_byydxxzm2');
        if(!t) return;
        var o = JSON.parse(t);
        if(o && typeof o === 'object'){
          S.yu = !!o.yu; S.ling = !!o.ling; S.noad = !!o.noad;
        }
      }catch(e){}
    };
    S.tick = function(){
      S.readCfg();
      S.patch();
      try{ window.donghuaFuhao = !S.noad; }catch(e){}
    };
    setTimeout(function(){ S.tick(); }, 3000);
    setTimeout(function(){
      try{
        cc.director.getScheduler().schedule(function(){ S.tick(); }, null, 2, cc.macro.REPEAT_FOREVER, 0, false);
      }catch(e){ setInterval(S.tick, 2000); }
    }, 6000);
  }catch(e){}
})();
"""


def main():
    OUTDIR.mkdir(parents=True, exist_ok=True)
    z = zipfile.ZipFile(ORIG_APK)
    orig = z.read("assets/src/project.jsc")
    print("orig project.jsc", len(orig), hashlib.sha256(orig).hexdigest()[:16])
    blob, n, olen = xxtea_decrypt(orig)
    body = blob[:olen]
    print("decrypted len", olen, "zip?", body[:2])
    # 往返自检
    again, _, _ = xxtea_decrypt(xxtea_encrypt(body))
    assert again[:olen] == body, "xxtea 往返失败"
    print("xxtea roundtrip ok")

    zin = zipfile.ZipFile(io.BytesIO(body))
    print("zip entries:", [(i.filename, i.file_size, i.compress_type) for i in zin.infolist()])
    name = zin.infolist()[0].filename
    js = zin.read(name).decode("utf-8", "surrogateescape")
    print("js chars", len(js), "already patched?", MARK in js)
    minimal = OUTDIR / "project__encrypt.minimal.js"
    if minimal.exists():
        js = minimal.read_text(encoding="utf-8", errors="surrogateescape")
        print("use minimal patch:", len(js), "chars")
    elif MARK not in js:
        js = js + PATCH_JS
    (OUTDIR / "project__encrypt.patched.js").write_text(js, encoding="utf-8", errors="surrogateescape")

    buf = io.BytesIO()
    with zipfile.ZipFile(buf, "w", zipfile.ZIP_DEFLATED) as zo:
        zi = zipfile.ZipInfo(name, (2026, 11, 2, 0, 0, 0))
        zi.compress_type = zipfile.ZIP_DEFLATED
        zi.external_attr = 0o644 << 16
        zo.writestr(zi, js.encode("utf-8", "surrogateescape"))
    newbody = buf.getvalue()
    print("new zip len", len(newbody))
    enc = xxtea_encrypt(newbody)
    (OUTDIR / "project.jsc").write_bytes(enc)
    print("new project.jsc", len(enc), hashlib.sha256(enc).hexdigest()[:16])
    # 再解一遍确认
    back, _, blen = xxtea_decrypt(enc)
    assert back[:blen] == newbody, "新 jsc 自解失败"
    print("verify: new jsc decrypts back ok, len", blen)


if __name__ == "__main__":
    main()
