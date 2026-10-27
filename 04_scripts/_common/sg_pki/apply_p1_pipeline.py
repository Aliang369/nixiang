# -*- coding: utf-8 -*-
"""⛔ 停用件（2026-10-27 用户裁决）：加固门禁已下线，**不要再跑本脚本**——
它的第 1 项会把 harden 阶段重新写回 pipeline.json（与下线口径相反）。
保留仅为历史追溯；需要真跑加固时用 `HARDEN_GATE_ENABLED=1 python 04_scripts/_common/harden_apk.py check`。

补 P1-7 的两处收尾：
  1) pipeline.json 增加 harden 阶段（出包门禁进管线，而不只是藏在 assemble 里）
  2) 菜单模板补记真实装机包名（.tt1 变体）—— precheck 需要它，但不动后端已认的 package 字段
两项都幂等，重复跑不会写脏。
"""
import io
import json

PIPE = r"E:\nixiang\04_scripts\勇士之心\pipeline.json"
TPL = r"E:\nixiang\10_docs\勇士之心\yszzx_menu_template.json"

STAGE = {
    "id": "harden",
    "name": "加固出包门禁（P1-7）：公钥一致 / 清单一致 / 去路标 / 产物同一 / 红队可检测",
    "requires": ["sign"],
    "command": "python3.11 04_scripts/_common/harden_apk.py redteam",
    "preflight": ["04_scripts/_common/harden_apk.py"],
    "verify": [],
    "status": "pending",
    "note": ("2026-10-27 防御加固 P1-7 新增：五条断言任一 FAIL 即非 0 退出，拒绝交付。"
             "C1 客户端公钥==私钥 / C2 完整性清单与当前 dex+main.js+证书 相符 / "
             "C3 包内 so 无 [验证]·grant ok·grant rejected 明文 / C4 包内 so+dex md5 == 本地产物 / "
             "C5 改 1 字节后哈希必变（红队可检测）。assemble 脚本结尾也会自动跑一次（同一条门禁）。"),
}


def add_stage():
    d = json.load(io.open(PIPE, encoding="utf-8"))
    ids = [s.get("id") for s in d["stages"]]
    if "harden" in ids:
        print("pipeline.json: harden 已存在，跳过")
        return
    i = ids.index("sign") + 1
    d["stages"].insert(i, STAGE)
    io.open(PIPE, "w", encoding="utf-8", newline="\n").write(
        json.dumps(d, ensure_ascii=False, indent=2) + "\n")
    print("pipeline.json: 已在 sign 之后插入 harden 阶段（stages=%d）" % len(d["stages"]))


def add_tpl():
    d = json.load(io.open(TPL, encoding="utf-8"))
    if d.get("package_install"):
        print("menu_template: package_install 已存在，跳过")
        return
    # 只加说明字段，不动后端已认的 package
    out = {}
    for k, v in d.items():
        out[k] = v
        if k == "package":
            out["package_install"] = "com.fightingstars.games.warrior.tt1"
            out["package_note"] = (
                "1.0.3 TapTap 底包是 .tt1 变体：装机器/预检请用 package_install"
                "（post_install_precheck.sh --pkg com.fightingstars.games.warrior.tt1）；"
                "package 字段保持后端卡密/菜单匹配用的历史值。")
    io.open(TPL, "w", encoding="utf-8", newline="\n").write(
        json.dumps(out, ensure_ascii=False, indent=2) + "\n")
    print("menu_template: 已补 package_install=.tt1")


if __name__ == "__main__":
    add_stage()
    add_tpl()
