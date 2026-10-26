#!/usr/bin/env bash
# 已废弃（2026-10-26）：由 python 版取代，本文件只留一行指针，避免旧链接失效。
#
# 弃用原因：底包/仓库路径含中文，bash 侧 cygpath + 引号 + GBK 控制台反复出错；
# 且新版要一次做完「加固包自动脱壳 + 自动算 classesN.dex + 三 ABI + 逐条回读校验」。
#
# 现用入口：
#   python3 04_scripts/勇士之心/ysxz_assemble_103.py --abis "arm64-v8a x86 armeabi-v7a"
#
# 历史与实测见 10_docs/勇士之心/路线锁定.md §4.1。
echo "DEPRECATED: 请用 python3 04_scripts/勇士之心/ysxz_assemble_103.py" >&2
exit 1
