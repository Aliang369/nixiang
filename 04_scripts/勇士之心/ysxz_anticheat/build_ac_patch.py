#!/usr/bin/env python3.11
"""
把「战斗加速层」(speed_layer.js) + 「自动化层」(auto_layer.js) 外科式并入出货基线
patch JS，并重新生成 YsxzPatchJs.h。外科点全部用精确字符串锚定，任一锚点未命中
或命中多次即中止(不产出任何文件)。

2026-11-23 变更：**过检测层(ac_layer.js)已从合成链移除**（用户本轮指令）。
  * ac_layer.js 里的第 7 节「战斗加速」是菜单功能层第 6 项的实现体，已抽出为
    speed_layer.js 并继续参与合成；其余闸门/出口挂钩/结算补偿/伤害还原全部不再进包。
  * 原 E1..E8 八个锚点随之删除，基线回归出厂原样；只保留 E9(功能开关 getter) 与
    E10(自动战斗序号右移)。
  * 反向断言：生成物里 __starry_ac / ANTI_CHEAT_ENABLED 等 AC 标记计数必须为 0。

输入
  04_scripts/勇士之心/ysxz_anticheat/baseline_patch_from_shipped_so.js  (从出货 so 抽出的权威基线)
  04_scripts/勇士之心/ysxz_anticheat/speed_layer.js                     (战斗加速，从 ac_layer 抽出)
  04_scripts/勇士之心/ysxz_anticheat/auto_layer.js                      (自动化层)
输出
  04_scripts/勇士之心/ysxz_anticheat/patch_v2_ac.js                     (合成后的 patch JS)
  04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzPatchJs.h                     (构建用头文件, 原文件先备份 .bak)

用法
  python3.11 build_ac_patch.py [--dry]
"""
from __future__ import annotations

import hashlib
import shutil
import subprocess
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = HERE / "baseline_patch_from_shipped_so.js"
SPL = HERE / "speed_layer.js"      # 战斗加速(从 ac_layer.js 第 7 节抽出, 2026-11-23)
AUT = HERE / "auto_layer.js"
OUT_JS = HERE / "patch_v2_ac.js"
HEADER = ROOT / "04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzPatchJs.h"
MARKER = "/*starry_ysxz_v1*/"
START = 'R"YSXZJS('
END = ')YSXZJS";'

# ---------------------------------------------------------------- 外科锚点 --
# 每项 = (名称, 旧串, 新串)。旧串必须恰好出现 1 次。
#
# 2026-11-23 移除过检测层：原 E1/E6(伤害记账/配对纪律) / E2(AC 扫描) / E3(闸门目标判定) /
# E4(update 墙钟) / E5(capture 接入) / E7(gate-only 识别) / E8(adoptHit) 八个锚点全部删除。
# 它们只服务于 ac_layer.js；删掉后基线 body 回到出货原样，功能层菜单绑定一项不动
# (伤害倍率/秒杀 走 baseline 伤害公式咽喉, 无敌/无限技能/移速 走 baseline tryLogic/tryEntity/
#  trySystem, 自动战斗 走 baseline tryAuto + feature5=R(7))。
EDITS: list[tuple[str, str, str]] = [
    (
        "E10 菜单重排: 自动战斗 6 -> 7（战斗加速占第 6 位，基础功能内）",
        '''  Object.defineProperty(M, "feature5", { get: function () { return R(6); } });   /* 自动战斗 */''',
        '''  Object.defineProperty(M, "feature5", { get: function () { return R(7); } });   /* 自动战斗 */''',
    ),
    (
        "E9 新增功能开关: 追加 feature6..9 的菜单 getter (自动化层读这些)",
        """  Object.defineProperty(M, "speed", { get: function () { return V(5); } });      /* 移动速度 1..10 */
  Object.defineProperty(M, "damage", { get: function () { return V(1); } });     /* 伤害倍率 1..50 */
""",
        """  Object.defineProperty(M, "speed", { get: function () { return V(5); } });      /* 移动速度 1..10 */
  Object.defineProperty(M, "damage", { get: function () { return V(1); } });     /* 伤害倍率 1..50 */
  /* 2026-10-07 新增功能(卡密包同名功能复刻)。序号=菜单位置, 必须与 kYsxz_Features 严格一致:
       8  解锁金卡(已并入「基础功能」分类)  /  9  Category_自动化(分组占位)
       10 自动重复挑战 / 11 自动出售 / 12 自动分解
     （2026-11-23 用户要求删除原 13 号功能 => 菜单槽位 14 -> 13, feature10/R(13) 一并删除）
     全部由 automation 层(auto_layer.js)消费, 加速层(speed_layer.js)不读这些。 */
  Object.defineProperty(M, "feature6",  { get: function () { return R(8); } });  /* 解锁金卡 */
  Object.defineProperty(M, "feature7",  { get: function () { return R(10); } }); /* 自动重复挑战 */
  Object.defineProperty(M, "feature8",  { get: function () { return R(11); } }); /* 自动出售 */
  Object.defineProperty(M, "feature9",  { get: function () { return R(12); } }); /* 自动分解 */
  /* 2026-10-24 新增：战斗加速（1=关，2..5=倍率），放在「基础功能」里、紧跟移动速度（序号 6）。
     消费方是 speed 层(speed_layer.js)：引擎 calculateDeltaTime 的 _deltaTime ×k（真·全局变速）。 */
  Object.defineProperty(M, "bspeed",    { get: function () { return V(6); } }); /* 战斗加速 */
""",
    ),
]

def sha(s: str) -> str:
    return hashlib.sha256(s.encode("utf-8")).hexdigest()


def main() -> int:
    dry = "--dry" in sys.argv
    base = BASE.read_text(encoding="utf-8")
    spl = SPL.read_text(encoding="utf-8")
    aut = AUT.read_text(encoding="utf-8")

    # ---- 前置断言: 基线与两层 ---------------------------------------------
    if base.count(MARKER) != 1:
        raise SystemExit(f"baseline 里 {MARKER} 出现 {base.count(MARKER)} 次, 期望 1 次")
    if base.count(")YSXZJS\"") or spl.count(")YSXZJS\"") or aut.count(")YSXZJS\""):
        raise SystemExit("源码里出现了 raw string 结束符 )YSXZJS\", 会截断头文件")
    for nm, txt in (("baseline", base), ("speed_layer", spl), ("auto_layer", aut)):
        if "\x00" in txt:
            raise SystemExit(f"{nm} 含 NUL 字节 (ysxz 用 strlen 注入, 会被截断)")
    if "window.__starry_speed" not in spl:
        raise SystemExit("speed_layer.js 未定义 window.__starry_speed")
    # 反向断言: 加速层自己不得夹带任何过检测痕迹(本轮用户点名的移除目标)
    for bad in ("__starry_ac", "ANTI_CHEAT_ENABLED", "noteDamage", "clearPending",
                "adoptHit", "_gateOnly", "finalHurtValue"):
        if bad in spl:
            raise SystemExit(f"speed_layer.js 里出现了过检测残留 {bad!r} —— 拒绝合成")
    if "window.__starry_auto" not in aut or "window.__AUTO_STATE__" not in aut:
        raise SystemExit("auto_layer.js 未导出 window.__starry_auto / __AUTO_STATE__")

    lines = base.splitlines(keepends=True)
    try:
        idx = next(i for i, l in enumerate(lines) if l.strip() == MARKER)
    except StopIteration:
        raise SystemExit(f"baseline 里找不到标记行 {MARKER}")
    if any(l.strip() == MARKER for l in lines[idx + 1:]):
        raise SystemExit(f"baseline 里标记行 {MARKER} 出现多次")
    head = "".join(lines[:idx + 1])                 # 前导空行 + 标记行
    body = "".join(lines[idx + 1:])                 # 基线 IIFE
    if ";(" not in body.split("\n", 1)[0]:
        raise SystemExit(f"标记行后第一行不是 IIFE 开头: {body.split(chr(10))[0]!r}")

    # ---- 外科编辑 ----------------------------------------------------------
    report: list[str] = []
    for name, old, new in EDITS:
        c = body.count(old)
        if c != 1:
            raise SystemExit(f"锚点未命中: {name} (出现 {c} 次, 期望 1 次)")
        body = body.replace(old, new, 1)
        report.append(f"  ok  {name}")

    # 顺序: 标记行 -> 战斗加速层(speed) -> 自动化层(auto) -> 基线 IIFE。
    # 过检测层(ac_layer.js)自 2026-11-23 起不再参与合成。
    # speed 在前保持「加速先于自动化」的加载顺序; auto 引用 window.__starry_speed / window.MOD
    # 都发生在【运行时】(1s 后的定时器), 本身不依赖加载顺序。
    combined = head + spl.rstrip("\n") + "\n" + aut.rstrip("\n") + "\n" + body

    # ---- 合成后自检 --------------------------------------------------------
    checks = [
        (combined.count(MARKER) == 1, f"{MARKER} 恰好 1 次"),
        (combined.count("window.__starry_speed") >= 2, "speed 层引用点齐全(定义 + 自动化层次通道)"),
        ("var sp = window.__starry_speed;" in combined, "自动化层 battleSignal 次通道改读 window.__starry_speed"),
        ("sp.tickSeen" in combined, "自动化层次通道读 sp.tickSeen(帧计数)"),
        # ---- 反向断言: 过检测层必须彻底不在生成物里 ----
        (combined.count("__starry_ac") == 0, "反向断言: __starry_ac 计数 = 0"),
        (combined.count("ANTI_CHEAT_ENABLED") == 0, "反向断言: ANTI_CHEAT_ENABLED 计数 = 0"),
        (combined.count("noteDamage") == 0, "反向断言: noteDamage 计数 = 0"),
        (combined.count("clearPending") == 0, "反向断言: clearPending 计数 = 0"),
        (combined.count("adoptHit") == 0, "反向断言: adoptHit 计数 = 0"),
        (combined.count("_gateOnly") == 0, "反向断言: _gateOnly 计数 = 0"),
        # finalHurtValue 不能断言 0：baseline 功能层(无敌/唯一扣血口)本来就拿它当明文指纹，
        # 共 5 处(实测)。这里断言"只剩基线原值"，即 E3 追加的闸门判据确实没了。
        (combined.count("finalHurtValue") == base.count("finalHurtValue"),
         "反向断言: finalHurtValue 只剩基线原值(功能层指纹 5 处)"),
        (combined.count("MOD.feature4") == 1, "feature4 出现次数未变(1)"),
        (combined.count("MOD.feature1") == 2, "feature1 出现次数未变(2)"),
        ("if (MOD.speed > 1) {" in combined, "移速滑条判定保留"),
        ('indexOf("bVs(")' not in combined, "自动战斗指纹未写死 bVs"),
        ("function tryAuto(Cls)" in combined, "tryAuto 保留"),
        ("if (_dm > 1) d = d * _dm;" in combined, "伤害倍率咽喉保留"),
        ("if (_f1) d = Math.ceil(d * 100);" in combined, "秒杀咽喉保留"),
        ('Object.defineProperty(M, "damage"' in combined, "damage 滑块绑定保留"),
        # 2026-11-23: 原 AC 证据链⓪①②③(dmg boosted/hit/fix/report #)断言随过检测层一并删除。
        # ---- 自动化层(auto_layer.js) 与 E9 新开关 ----
        (combined.count("window.__starry_auto") >= 2, "automation 层已接入且定义 window.__starry_auto"),
        ("window.__AUTO_STATE__" in combined, "automation 可观测口 __AUTO_STATE__ 存在"),
        ('Object.defineProperty(M, "feature6"' in combined, "E9 生效(feature6 解锁金卡)"),
        ("return R(7); } });" in combined, "E10 序号: feature5(自动战斗) -> R(7)"),
        ("return R(8); } });" in combined, "E9 序号: feature6(金卡) -> R(8)"),
        ("return R(10); } });" in combined, "E9 序号: feature7(重复挑战) -> R(10)"),
        ("return R(11); } });" in combined, "E9 序号: feature8(出售) -> R(11)"),
        ("return R(12); } });" in combined, "E9 序号: feature9(分解) -> R(12)"),
        ('Object.defineProperty(M, "bspeed"' in combined, "战斗加速: 滑块绑定存在"),
        ("return V(6); } });" in combined, "战斗加速: 序号 6 -> V(6)"),
        ("SPEED.installDeltaScale" in combined, "战斗加速: 引擎 dt 放大安装器存在(真·全局)"),
        ("this._deltaTime = dt * k;" in combined, "战斗加速: 每帧 _deltaTime ×k"),
        ("SPEED.dtScaled" in combined, "战斗加速: dt 路线标志(与兜底互斥)"),
        ("SPEED.applySpeed = function ()" in combined, "战斗加速: applySpeed 实现存在"),
        ("worldTimeScale" in combined, "战斗加速: 走 worldTimeScale(明文名, 不写死短名)"),
        ("SPEED.speedMul" in combined, "战斗加速: 倍率记账 speedMul（只记账，不碰上报字段）"),
        ("speedLoop" in combined, "战斗加速: 载入即启动的低频施加链存在"),
        ("SPEED.tickSeen" in combined, "战斗加速: 帧计数 tickSeen 由 calculateDeltaTime 包装器维护"),
        ("proto[hit.name] = w;" in combined, "解锁金卡: 按发现结果包装原型方法(不写死短名)"),
        ("goldCandidates" in combined and 'src.indexOf("$eZ.buff")' in combined,
         "解锁金卡: 结构锚点(silver/Premium/buffId/$eZ.buff)仍在"),
        ("goldAcceptBool" in combined and 'typeof r === "boolean"' in combined,
         "解锁金卡: 行为甄别(只认返回 boolean 的持有判定)"),
        ('cardType === "gold"' in combined, "解锁金卡: 只放行 gold 卡类型"),
        ("sellQ = QUAL_DEF.slice();" in combined and "decoQ = QUAL_DEF.slice();" in combined, "品质空兜底 QUAL_DEF [1,2,3] 保留"),
        ("A.tickN === 15 || A.tickN === 60" in combined, "automation 两张一次性状态快照保留"),
        ("lm[api.repWrite](REPEAT_IDX, on)" in combined, "自动重复挑战: 按 autoBattleFlag 指纹发现的写入口置位"),
        ("autoBattleFlag" in combined, "自动重复挑战: 游戏内挑战位同步"),
        ("window.__starry_fin" in combined and "againKey" in combined, "结算面板时机: 改吃 baseline 的 window.__starry_fin 事件"),
        # 2026-11-10：兜底语义改为「先判面板存活，再调游戏自带的 3 秒倒计时」，
        # 且禁止再出现 inst[againKey]()（那是点按钮/打开方法的老路，会重开面板死循环）。
        ("panelAlive(inst)" in combined, "自动重复挑战: 兜底前先判结算面板是否还活着"),
        ("inst[startKey]();" in combined, "自动重复挑战: 兜底只调游戏自带的倒计时启动方法"),
        ("findPanelStart" in combined, "结算面板钩: 启动倒计时方法按 1e3/-1 字面量发现(不写死短名)"),
        ("inst[againKey](true)" in combined, "自动重复挑战: 无启动方法时退化为驱动刷新方法(仍不代点按钮)"),
        ("inst[againKey]()" not in combined, "自动重复挑战: 不再直接调 againKey(防重开面板死循环)"),
        ('["auto", "btn", "again"].join("_")' in combined, "结算面板钩: 指纹字面量运行期拼接(防包装器自污染)"),
        ("bag[api.sell](sellQ)" in combined, "自动出售: bag 按品质批量出售口(指纹发现)"),
        ("bag[api.deco](decoQ)" in combined, "自动分解: bag 按品质批量分解口(指纹发现)"),
        ("autoSellEquipFlag" in combined and "autoDecomposeEquipFlag" in combined, "出售/分解品质取自存档字段 autoSellEquipFlag/autoDecomposeEquipFlag"),
        # 2026-11-23：自动穿戴整项删除 ⇒ 反向断言（12 项菜单口径）
        ('Object.defineProperty(M, "feature10"' not in combined, "反向断言: feature10 getter 已删除"),
        ("自动穿戴" not in combined, "反向断言: 菜单/日志里再无「自动穿戴」"),
        ("WORE_EQUIP" not in combined and "wearAll" not in combined and "wearBest" not in combined,
         "反向断言: 穿戴实现(wearAll/wearBest/WORE_EQUIP)全清"),
        ("}, 400);" in combined and "}, 6500);" in combined and "}, 1600);" not in combined,
         "错峰只剩 400ms 出售分解 / 6500ms 重复兜底"),
        ("try { autoBattleTick(); }" in combined, "自动战斗解锁: 驱动里显式调用 autoBattleTick()(防再漏)"),
        ("AUTO_VER = 6" in combined, "automation 版本仲裁 AUTO_VER = 6"),
        ("FEAT(6)" in combined and "FEAT(9)" in combined and "FEAT(10)" not in combined,
         "automation 开关走 FEAT(6)..FEAT(9)（FEAT(10) 已随穿戴删除）"),
        ("btn_toNext" in combined and "tryPanel" in combined, "结算面板模块已纳入扫描(tryPanel + btn_toNext 过滤)"),
        ("findFnOn(bag, 1" in combined and "findFnOn(lm, 1" in combined, "自动化发现层: 明文锚点 + 结构指纹(不写死短名)"),
    ]
    bad = [msg for ok, msg in checks if not ok]
    if bad:
        raise SystemExit("合成自检失败:\n  - " + "\n  - ".join(bad))

    if dry:
        print("\n".join(report))
        print(f"[dry] 未写文件。合成后 {len(combined)} B / {combined.count(chr(10))+1} 行")
        return 0

    # newline="\n"：产物必须与 mac 端旧 header 同为 LF（2026-11-23 统一，防 Windows 写成 CRLF），
    # 也让下面 f-string 里打印的 sha256 就是文件的 sha256。
    OUT_JS.write_text(combined, encoding="utf-8", newline="\n")
    proc = subprocess.run(["node", "--check", str(OUT_JS)], capture_output=True, text=True)
    if proc.returncode != 0:
        raise SystemExit("node --check 失败:\n" + proc.stderr)

    header = (
        "/* Auto-generated by 04_scripts/勇士之心/ysxz_anticheat/build_ac_patch.py\n"
        " * 来源: baseline_patch_from_shipped_so.js (从出货 so 抽出的权威基线)\n"
        " *     + speed_layer.js (战斗加速，2026-11-23 从 ac_layer.js 抽出；过检测层已移除)\n"
        " *     + auto_layer.js (自动化层)\n"
        f" * patch js sha256 = {sha(combined)}\n"
        " */\n"
        "#pragma once\n\n"
        f"static const char* kYsxzPatchJs = {START}\n\n"
        + combined
        + "\n\n"
        + END
        + "\n"
    )
    if HEADER.exists():
        shutil.copy2(HEADER, HEADER.with_suffix(".h.bak"))
    HEADER.write_text(header, encoding="utf-8", newline="\n")

    # 头文件回读校验: 从 raw string 里取回的 JS 必须与 patch js 完全一致
    ht = HEADER.read_text(encoding="utf-8")
    s = ht.find(START)
    e = ht.rfind(END)
    if s < 0 or e <= s:
        raise SystemExit("头文件 raw string 定位失败")
    back = ht[s + len(START):e]
    if back.strip("\n") != combined.strip("\n"):
        raise SystemExit("头文件回读内容与 patch js 不一致")
    if "\x00" in ht:
        raise SystemExit("头文件含 NUL")

    print("\n".join(report))
    print(f"patch js : {OUT_JS}  {len(combined)} B  sha256={sha(combined)}")
    print(f"header   : {HEADER}  {len(ht)} B  (原文件已备份 {HEADER.name}.bak)")
    print(f"回读校验 : OK ({len(back)} B)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
