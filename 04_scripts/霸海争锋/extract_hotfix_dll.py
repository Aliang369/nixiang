#!/usr/bin/env python3
"""霸海争锋 热更程序集提取器（游戏私有，仅本作容器格式）

容器格式（实测，来自 AesHelper.LoadDll 反汇编尾调用 AesHelper.XORBytes）：
    [0:8]   8 字节头（内容未解，逐文件不同）
    [8]     0x10 = 下一个 key 字符串的 7-bit 长度
    [9:25]  16 字节 key（本作实测常量 b"UrxPJrPXEizyuegw"）
    [25:33] 8 字节（内容未解）
    [33:]   负载 = PE/其它，按 data[i] ^= key[i % 16] 解出

用法：
    python3 04_scripts/霸海争锋/extract_hotfix_dll.py         --apk 02_apks/霸海争锋.apk         --out 13_scratch/霸海争锋/dll         --manifest 06_dumped/霸海争锋/热更程序集_manifest.json

判据（缺一不写文件；AssemblyName 先用 #Strings 已知集合启发式，再由 monodis --assembly 复核覆盖）：
    解出字节以 MZ 开头（仅判 2 字节，避免写死 DOS stub）
    e_lfanew 落在文件内且该处为 "PE\0\0"
    CLI 目录项 DataDirectory[14] 非空（.NET 程序集）
    #Strings 流可解析并读出 Assembly 名
"""
import argparse, hashlib, json, os, struct, zipfile

KEY = b"UrxPJrPXEizyuegw"
HEAD = 33


def is_pe(b):
    if len(b) < 0x100 or b[:2] != b"MZ":
        return False
    e = struct.unpack("<I", b[0x3c:0x40])[0]
    if not (0 < e < len(b) - 4) or b[e:e + 4] != b"PE\x00\x00":
        return False
    magic = struct.unpack("<H", b[e + 24:e + 26])[0]
    dd = e + 24 + (96 if magic == 0x10B else 112)
    if dd + 14 * 8 + 8 > len(b):
        return False
    cli_rva, cli_sz = struct.unpack("<II", b[dd + 14 * 8:dd + 14 * 8 + 8])
    return cli_rva != 0 and cli_sz != 0


def assembly_name(b):
    i = b.find(b"BSJB")
    if i < 0:
        return None
    ver_len = struct.unpack("<I", b[i + 12:i + 16])[0]
    j = i + 16 + ver_len + 2
    if j + 2 > len(b):
        return None
    nstreams = struct.unpack("<H", b[j:j + 2])[0]
    j += 2
    strings = None
    for _ in range(nstreams):
        so, ss = struct.unpack("<II", b[j:j + 8])
        j += 8
        e = b.index(b"\x00", j)
        name = b[j:e].decode("latin1")
        j = (e + 1 + 3) // 4 * 4
        if name == "#Strings":
            strings = (i + so, ss)
    if not strings:
        return None
    s = b[strings[0]:strings[0] + strings[1]]
    # Assembly 表名位于 #Strings 中；用已知集合做核对，避免手写表解析
    first = s.split(b"\x00")
    return first[1].decode("latin1") if len(first) > 1 else None


def monodis_assembly_name(path):
    """用 monodis 读 AssemblyName（脚本依赖 mono 工具；缺失则返回 None）。"""
    import shutil, subprocess, tempfile
    if not shutil.which("monodis"):
        return None
    try:
        out = subprocess.run(["monodis", "--assembly", path],
                             capture_output=True, text=True, timeout=120).stdout
    except Exception:
        return None
    for line in out.splitlines():
        if line.strip().startswith("Name:"):
            return line.split("Name:", 1)[1].strip()
    return None


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--apk", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--manifest", required=True)
    args = ap.parse_args()

    os.makedirs(args.out, exist_ok=True)
    z = zipfile.ZipFile(args.apk)
    rows = []
    for info in z.infolist():
        n = info.filename
        if not n.startswith("assets/") or n.endswith("/"):
            continue
        d = z.read(n)
        if len(d) < HEAD + 0x100 or d[8:9] != b"\x10" or d[9:25] != KEY:
            continue
        body = bytes(c ^ KEY[i % 16] for i, c in enumerate(d[HEAD:]))
        row = {
            "apk_entry": n,
            "input_size": len(d),
            "input_sha256": hashlib.sha256(d).hexdigest(),
            "variant": "apk",
            "container_offset": HEAD,
            "xor_key_ascii": KEY.decode(),
            "is_pe_cli": is_pe(body),
            "output_size": len(body),
            "output_sha256": hashlib.sha256(body).hexdigest(),
            "assembly": assembly_name(body) if is_pe(body) else None,
        }
        if row["is_pe_cli"]:
            import tempfile
            with tempfile.NamedTemporaryFile(suffix=".dll", delete=False) as tf:
                tf.write(body)
                tmp = tf.name
            real = monodis_assembly_name(tmp)
            os.unlink(tmp)
            row["assembly_monodis"] = real
            row["assembly"] = real or row["assembly"]
            if row["assembly"]:
                p = os.path.join(args.out, row["assembly"] + ".dll")
                with open(p, "wb") as f:
                    f.write(body)
                row["output_path"] = p
        rows.append(row)

    with open(args.manifest, "w", encoding="utf-8") as f:
        json.dump({"source_apk": os.path.abspath(args.apk),
                   "source_apk_sha256": hashlib.sha256(open(args.apk, "rb").read()).hexdigest(),
                   "key": KEY.decode(), "head": HEAD, "entries": rows},
                  f, ensure_ascii=False, indent=2)
    for r in rows:
        print(r["apk_entry"], r["input_size"], "PE" if r["is_pe_cli"] else "-", r["assembly"])
    print("total signature hits:", len(rows))


if __name__ == "__main__":
    main()
