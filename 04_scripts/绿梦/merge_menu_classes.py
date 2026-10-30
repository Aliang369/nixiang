# -*- coding: utf-8 -*-
"""绿梦：把「r3 su 修复 + 新 GUID/snapshot」编译出的类，外科移植进已部署的 classes4.dex 反编译树。

为什么这样做
------------
已部署的 classes4.dex（v32 血统）是用 AGP/smali 那一套工具链产出的，dex 里带
`$$ExternalSyntheticApiModelOutline*` 等 desugaring 产物；用 javac+d8 整份重建会换掉
整包 desugaring 形态（老 Android 上风险不可控）。所以**只替换本次真正改动的 3 个类
及其合成类**，其余类逐字节保持部署版。

用法
----
  python 04_scripts/绿梦/merge_menu_classes.py \
      --deployed-smali  <部署版 baksmali 目录> \
      --built-smali     <新建 dex 的 baksmali 目录> \
      --out-smali       <输出 smali 目录（会先清空）> \
      --classes com/starry/magic/auth/DeviceFingerprint \
                com/starry/magic/menu/Preferences \
                com/starry/magic/lvmeng/LvmengPayloadProvider

退出码：0=成功；1=缺类/重复/自检失败；2=用法错。
"""
import argparse
import os
import shutil
import sys


def class_files(root, cls):
    """返回 root 下属于 cls 的所有 smali 文件相对路径（含 `Cls$…` / `Cls$$…` 合成类）。"""
    d = os.path.join(root, os.path.dirname(cls))
    base = os.path.basename(cls)
    hits = []
    if not os.path.isdir(d):
        return hits
    for fn in os.listdir(d):
        if fn == base + ".smali" or fn.startswith(base + "$"):
            hits.append(os.path.join(os.path.dirname(cls), fn))
    return sorted(hits)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--deployed-smali", required=True)
    ap.add_argument("--built-smali", required=True)
    ap.add_argument("--out-smali", required=True)
    ap.add_argument("--classes", nargs="+", required=True)
    a = ap.parse_args()

    if os.path.abspath(a.out_smali) in (os.path.abspath(a.deployed_smali), os.path.abspath(a.built_smali)):
        sys.stderr.write("ERROR: --out-smali 不能与输入目录相同\n")
        return 2

    if os.path.isdir(a.out_smali):
        shutil.rmtree(a.out_smali)
    shutil.copytree(a.deployed_smali, a.out_smali)

    total_del = total_add = 0
    for cls in a.classes:
        dep = class_files(a.deployed_smali, cls)
        bld = class_files(a.built_smali, cls)
        if not bld:
            sys.stderr.write("ERROR: 新建树里没有 %s\n" % cls)
            return 1
        # 删掉部署版里属于这些类的全部文件（本体 + 合成类）
        for rel in dep:
            tgt = os.path.join(a.out_smali, rel)
            if os.path.isfile(tgt):
                os.remove(tgt)
                total_del += 1
        # 拷入新建版
        for rel in bld:
            src = os.path.join(a.built_smali, rel)
            tgt = os.path.join(a.out_smali, rel)
            os.makedirs(os.path.dirname(tgt), exist_ok=True)
            shutil.copyfile(src, tgt)
            total_add += 1
        print("  %-52s 部署版 %2d 个 → 新建 %2d 个" % (cls, len(dep), len(bld)))

    print("合并完成：删除 %d 个、写入 %d 个 → %s" % (total_del, total_add, a.out_smali))
    return 0


if __name__ == "__main__":
    sys.exit(main())
