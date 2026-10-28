# -*- coding: utf-8 -*-
"""收官文档同步：轮次台账追加 1.0.4 轮、台账补收官行、游戏摘要补收官状态。"""
import io

# ---------- 1) 轮次台账.tsv ----------
P = "10_docs/长生天机降世/轮次台账.tsv"
row = ("2026-10-28 21:41\t全部(1-2)\t3\t"
       "1.0.4 复刻（改动）：底包 1.0.4/vc1040 2,277,631,016 B 入 02_apks → "
       "挂点重新取证 file 0x2657F4C（签名扫描唯一命中）→ 沿用 1.0.3 外科包结构 → "
       "雷电 emulator-5554 纯安装（无原版、未卸载）\t"
       "见下行结果行\t见下行结果行")
res = ("2026-10-28 21:48\t全部(1-2)\t3\t"
       "1.0.4 复刻（结果）：engine logcat 命中 `[Cstjjs] hooked enter-interp @0x400025a5bf4c`、"
       "`MI ready`、`patch Health n=1..4 side=1 -50→0`（无敌）、`n=5/6 side=2 -5→-50`、"
       "`n=7 side=2 -24→-240`（十倍伤害）、`feat 1/2 → 1`；pid 24269 存活无崩溃\t"
       "`pre_install_gate.sh` PASS=3 FAIL=0 WARN=0 SKIP=1；`adb install -r -d` Success（23.8s）；"
       "登录经 TapTap 内嵌登录页完成、未弹「非法登录」（成品 2,275,713,348 B < 红线 2,277,634,048 B）\t"
       "有效（引擎层实测命中真实扣血路径；用户当轮回执「游戏没问题了」）")
s = io.open(P, encoding="utf-8").read()
if "1.0.4 复刻（结果）" not in s:
    if not s.endswith("\n"):
        s += "\n"
    s += row + "\n" + res + "\n"
    io.open(P, "w", encoding="utf-8").write(s)
    print("轮次台账：已追加 1.0.4 轮（2 行）")
else:
    print("轮次台账：已存在 1.0.4 轮，跳过")

# ---------- 2) 台账.md 收官行 ----------
P2 = "10_docs/长生天机降世/台账.md"
anchor = "\n## 已否证（禁止重试区）"
add = (
    "| 1.0.4 收官（2026-10-28）：正式包裁剪为唯一 APK + 滚动物清理 | "
    "`prune_output_final.sh` 删除 `…starry-unsigned.apk`（2,278,932,553 B，越体积红线 1.3 MB ⇒ 正是上一轮「非法登录」的成因），"
    "清单 `正式包裁剪清单_20261028.json`；`trash_paths.sh` 隔离清理 `07_patched/长生天机降世`(237 M)、"
    "`06_dumped/1.0.4/{static 111 M, dump 38 M, code_apk 15 M, code_hot 22 M, manifests, classes*.dex}`、"
    "`09_test/长生天机降世`(6 M)，隔离目录 `99_archive/_trash_20261028/`，清单 `滚动物清理清单_20261028.json`；"
    "`06_dumped/1.0.4` 只留 `decomp/` 两份反编译原文 + `README.md`（再生命令） | "
    "成立（2026-10-28） | 本轮收官 |\n"
)
s2 = io.open(P2, encoding="utf-8").read()
if "1.0.4 收官（2026-10-28）" not in s2:
    s2 = s2.replace(anchor, "\n" + add.rstrip("\n") + anchor, 1)
    io.open(P2, "w", encoding="utf-8").write(s2)
    print("台账：已补收官行")
else:
    print("台账：已有收官行，跳过")

# ---------- 3) 游戏摘要补收官行 ----------
P3 = "10_docs/_state/games/长生天机降世.md"
s3 = io.open(P3, encoding="utf-8").read()
if "| 收官（2026-10-28）" not in s3:
    old = "| 未决 | ① 本包 TapTap 侧仍是「TapSignKit MD5 回灌 + isClientLogin=false」老办法（与 2026-10-28 定案的「改包法」不同源）；② 出包脚本仍是 mac 专用 `surgical_cstjjs_1.0.3.sh`，Windows 侧尚无 1.0.4 版脚本 |"
    new = ("| 收官（2026-10-28） | 用户口令「收尾+正式包+清理+git」：`08_output/长生天机降世` 已裁为唯一正式包（删 `…starry-unsigned.apk` 2,278,932,553 B，清单 `正式包裁剪清单_20261028.json`）；"
           "滚动物已隔离清理（`07_patched` 237 M、`06_dumped/1.0.4` 大二进制、`09_test`，清单 `滚动物清理清单_20261028.json`，隔离在 `99_archive/_trash_20261028/`）；"
           "`06_dumped/1.0.4` 只留 `decomp/` 两份反编译原文＋`README.md`；`pipeline.json` 快照已升 1.0.4（version/底包/so/挂点 0x2657F4C） |\n"
           "| 未决 | ① 本包 TapTap 侧仍是「TapSignKit MD5 回灌 + isClientLogin=false」老办法（与 2026-10-28 定案的「改包法」不同源）；② 出包脚本仍是 mac 专用 `surgical_cstjjs_1.0.3.sh`，Windows 侧尚无 1.0.4 版脚本 |")
    s3 = s3.replace(old, new, 1)
    io.open(P3, "w", encoding="utf-8").write(s3)
    print("游戏摘要：已补收官行")
else:
    print("游戏摘要：已有收官行，跳过")
