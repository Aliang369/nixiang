#!/usr/bin/env python3.11
# 月圆之夜: 从本地 YooAsset bundle 提取全部 TextAsset (Config CSV 等) — 纯离线
# 用法: extract_configs.py [package ...]  默认 CommonPackage 先跑
import os, sys, json, io
import UnityPy

YOO = "/Users/aliang/Documents/nixiang/03_decompiled/月圆之夜/apktool_1.6.30.1/assets/yoo"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/configs"

def sniff_ext(data: bytes, name: str) -> str:
    n = name.lower()
    for e in (".csv", ".json", ".txt", ".xml", ".bytes", ".tsv"):
        if n.endswith(e):
            return e
    head = data[:64].lstrip()
    if b"," in data[:256] and b"\n" in data[:4096]:
        return ".csv"
    if head.startswith(b"{") or head.startswith(b"["):
        return ".json"
    if head.startswith(b"<?xml"):
        return ".xml"
    return ".bytes"

def extract(pkg: str):
    src = os.path.join(YOO, pkg)
    files = [f for f in os.listdir(src) if f.endswith((".bundle", ".rawfile"))]
    index = []
    n_text = 0
    for i, fn in enumerate(sorted(files)):
        path = os.path.join(src, fn)
        try:
            data = open(path, "rb").read()
            # YooAsset 加密头: bundle 前 0x20 字节零填充, UnityFS 魔数在 0x20
            if fn.endswith(".bundle"):
                if data[:0x20] == b"\0" * 0x20 and data[0x20:0x28] == b"UnityFS\0":
                    data = data[0x20:]
                elif data[:8] != b"UnityFS\0":
                    print(f"[skip] {fn}: 非标准头", flush=True)
                    continue
            env = UnityPy.load(io.BytesIO(data))
        except Exception as e:
            print(f"[skip] {fn}: {type(e).__name__} {str(e)[:80]}", flush=True)
            continue
        # container 路径映射 (供命名参考; UnityPy container 偶发内部迭代错误, 失败即跳过)
        cont = {}
        try:
            for cpath, obj in list(env.container.items()):
                cont[obj.path_id] = cpath
        except Exception:
            cont = {}
        for obj in env.objects:
            if obj.type.name not in ("TextAsset",):
                continue
            try:
                d = obj.read()
                name = d.m_Name
                raw = d.m_Script
                if isinstance(raw, str):
                    raw = raw.encode("utf-8", "surrogateescape")
            except Exception as e:
                print(f"[err ] {fn} pid={obj.path_id}: {e}", flush=True)
                continue
            n_text += 1
            cpath = cont.get(obj.path_id, "")
            ext = sniff_ext(raw, name)
            # 若名字无扩展名, 优先用 container 路径里的真实文件名
            base_from_path = ""
            if cpath:
                base_from_path = os.path.basename(cpath)
            final_name = base_from_path if base_from_path and base_from_path != name else name + ext
            outdir = os.path.join(OUT, pkg)
            os.makedirs(outdir, exist_ok=True)
            outp = os.path.join(outdir, final_name)
            # 同名冲突: 记录不覆盖
            if os.path.exists(outp) and open(outp, "rb").read() != raw:
                stem, ext2 = os.path.splitext(final_name)
                k = 2
                while os.path.exists(os.path.join(outdir, f"{stem}_{k}{ext2}")):
                    k += 1
                outp = os.path.join(outdir, f"{stem}_{k}{ext2}")
            with open(outp, "wb") as f:
                f.write(raw)
            index.append({"pkg": pkg, "bundle": fn, "container": cpath,
                          "name": name, "file": os.path.basename(outp), "size": len(raw)})
        if (i + 1) % 100 == 0:
            print(f"  .. {i+1}/{len(files)} textassets={n_text}", flush=True)
    return index

def main():
    pkgs = sys.argv[1:] or ["CommonPackage"]
    all_idx = []
    for p in pkgs:
        print(f"== {p} ==", flush=True)
        all_idx += extract(p)
    os.makedirs(OUT, exist_ok=True)
    with open(os.path.join(OUT, "index.json"), "a", encoding="utf-8") as f:
        json.dump(all_idx, f, ensure_ascii=False, indent=1)
    print(f"done: {len(all_idx)} textassets → {OUT}")

if __name__ == "__main__":
    main()
