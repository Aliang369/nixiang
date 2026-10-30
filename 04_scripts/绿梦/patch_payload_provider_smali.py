# -*- coding: utf-8 -*-
"""绿梦：时空之声 —— 热更后改 LvmengPayloadProvider 的 bundle GUID + snapshot 强制重写（smali 层）

背景
----
`LvmengPayloadProvider` 把 `assets/lvmeng/{__data,__info}` 每次启动写进**写死的**
`<ext>/yoo/Dll/BundleFiles/<前2位>/<GUID>/`。游戏热更若换了 Dll bundle 哈希，
补丁就落到游戏不看的目录 ⇒ 五项功能全失效（2026-10-30 实测，见
`10_docs/绿梦/热更20261030_修复.md`）。

另外 `writeSnapshotIfAbsent` 的「snapshot 已存在就 return」必须改成无条件写：
否则游戏用自己下载的官方 crc/size 记录判定我们补丁 bundle 大小不符 → 删档重下。

用法
----
  # 1) baksmali 出 classes4.dex
  java -jar 01_tools/smali/baksmali-fat.jar d classes4.dex -o smali4
  # 2) 改 smali
  python 04_scripts/绿梦/patch_payload_provider_smali.py \
      --smali smali4/com/starry/magic/lvmeng/LvmengPayloadProvider.smali \
      --old-guid f8124a60540376c3583f7e7d15eaf9e0 \
      --new-guid 17d93118365e63ab20bf267703295779
  # 3) 重汇编
  java -jar 01_tools/smali/smali-fat.jar a smali4 -o classes4_new.dex

退出码：0=成功；2=参数/结构不符（找不到待改的锚点）。
"""
import argparse
import io
import sys


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--smali", required=True, help="LvmengPayloadProvider.smali 路径")
    ap.add_argument("--old-guid", required=True, help="现役（要替换掉的）32 位 GUID")
    ap.add_argument("--new-guid", required=True, help="新的 32 位 GUID")
    a = ap.parse_args()
    if len(a.old_guid) != 32 or len(a.new_guid) != 32:
        sys.stderr.write("ERROR: GUID 必须是 32 位十六进制\n")
        return 2

    with io.open(a.smali, encoding="utf-8") as f:
        s = f.read()

    n = s.count(a.old_guid)
    if n == 0:
        sys.stderr.write("ERROR: smali 里找不到旧 GUID %s\n" % a.old_guid)
        return 2
    s = s.replace(a.old_guid, a.new_guid)

    # GUID_DIR 常量 "<前2位>/<guid>" 与建目录用的完整路径串 "yoo/Dll/BundleFiles/<前2位>/<guid>"
    # 前缀一律从 --old-guid 推导（2026-10-30b 热更：17→c9，写死 f8 会整轮改不动）
    old_pfx = a.old_guid[:2]
    new_pfx = a.new_guid[:2]
    for pat, rep in (
        ('"%s/' % old_pfx, '"%s/' % new_pfx),
        ("BundleFiles/%s/" % old_pfx, "BundleFiles/%s/" % new_pfx),
    ):
        s = s.replace(pat, rep)
    print("GUID 替换 %d 处；目录前缀 %s → %s" % (n, old_pfx, new_pfx))

    if "goto :cond_28" in s:
        print("snapshot 已是 goto 形态，跳过")
    elif "if-eqz v1, :cond_28" in s:
        s = s.replace(
            "if-eqz v1, :cond_28",
            "goto :cond_28  # hotfix: snapshot 每次重写（见 10_docs/绿梦/热更20261030_修复.md）",
        )
        print("snapshot 早退 if-eqz → goto（每次重写）")
    else:
        # r4 起（su 修复轮）LvmengPayloadProvider 由源码重编，源码本身已是无条件写，
        # 部署 dex 里根本不存在「snapshot 已存在就 return」的早退分支 ⇒ 无锚点属正常形态。
        print("未见 snapshot 早退分支（源码已无条件写）⇒ 无需改，跳过")

    with io.open(a.smali, "w", encoding="utf-8", newline="\n") as f:
        f.write(s)
    print("written", a.smali)
    return 0


if __name__ == "__main__":
    sys.exit(main())
