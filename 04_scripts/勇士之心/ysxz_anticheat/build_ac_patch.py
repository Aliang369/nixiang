#!/usr/bin/env python3.11
"""
把「星空过检测层」(ac_layer.js) 外科式并入出货基线 patch JS，并重新生成
YsxzPatchJs.h。四层复刻的"外科点"全部用精确字符串锚定，任一锚点未命中
或命中多次即中止(不产出任何文件)。

输入
  04_scripts/勇士之心/ysxz_anticheat/baseline_patch_from_shipped_so.js  (从出货 so 抽出的权威基线)
  04_scripts/勇士之心/ysxz_anticheat/ac_layer.js                        (卡密包四层复刻)
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
ACL = HERE / "ac_layer.js"
AUT = HERE / "auto_layer.js"
OUT_JS = HERE / "patch_v2_ac.js"
HEADER = ROOT / "04_scripts/勇士之心/ysxz_1.0.3_menu/YsxzPatchJs.h"
MARKER = "/*starry_ysxz_v1*/"
START = 'R"YSXZJS('
END = ')YSXZJS";'

# ---------------------------------------------------------------- 外科锚点 --
# 每项 = (名称, 旧串, 新串)。旧串必须恰好出现 1 次。
EDITS: list[tuple[str, str, str]] = [
    (
        "E1 伤害公式钩子: 记录未放大值并通知 AC 层",
        """                var _f1 = false, _dm = 1;
                try { _f1 = !!MOD.feature1; _dm = MOD.damage; } catch (readErr) {}
                if (_dm > 1) d = d * _dm;            /* 1 伤害倍率滑块 1..50, 1=原倍率 */
                if (_f1) d = Math.ceil(d * 100);     /* 2 秒杀: 玩家 x100 伤害 */
                if (d > 1e9) d = 1e9;                /* 上限, 防数值溢出 */
""",
        """                var _f1 = false, _dm = 1, _d0 = d;   /* _d0 = 未放大值(过检测还原用) */
                try { _f1 = !!MOD.feature1; _dm = MOD.damage; } catch (readErr) {}
                if (_dm > 1) d = d * _dm;            /* 1 伤害倍率滑块 1..50, 1=原倍率 */
                if (_f1) d = Math.ceil(d * 100);     /* 2 秒杀: 玩家 x100 伤害 */
                if (d > 1e9) d = 1e9;                /* 上限, 防数值溢出 */
                /* 过检测(卡密包 L2578 的等价物): 把这一笔"本该上报的伤害"记下来,
                   hY 扣血口用把 hw.finalDamage / hw.finalHurtValue 还原成原值。 */
                try { if (window.__starry_ac) window.__starry_ac.noteDamage(_d0, d); } catch (acErr) {}
""",
    ),
    (
        "E6 伤害公式钩子入口: 每笔伤害先清空配对记录(防跨次误配)",
        """        var oFV = K[fv];
        K[fv] = function (ctx, hit, ratio, canCrit) {
            var d = oFV.apply(this, arguments);""",
        """        var oFV = K[fv];
        K[fv] = function (ctx, hit, ratio, canCrit) {
            /* 过检测配对纪律: 先清空, 只有本笔伤害被我们放大过才在下面重新置位 */
            try { if (window.__starry_ac) window.__starry_ac.clearPending(); } catch (acErr) {}
            var d = oFV.apply(this, arguments);""",
    ),
    (
        "E2 目标模块包装: 把 exports 同时交给 AC 层(main 包只走闸门, 不跑功能扫描)",
        """            entry[0] = function (req, mod, exp) {
                orig.apply(this, arguments);
                try { scan(mod && mod.exports ? mod.exports : exp); } catch (e) { log("scan ERR: " + e); }
            };""",
        """            entry[0] = function (req, mod, exp) {
                orig.apply(this, arguments);
                var _ex = mod && mod.exports ? mod.exports : exp;
                /* main 包(gate-only): 只把 exports 交给闸门层, 不跑 tryLogic/tryEntity/trySystem/tryAuto,
                   否则 main 里的 $VN(maxApDefRate + 18 处 AUTO_BATTLE) 会被误钩。 */
                if (!_gateOnly) { try { scan(_ex); } catch (e) { log("scan ERR: " + e); } }
                try { if (window.__starry_ac) window.__starry_ac.scan(_ex, mid); } catch (e) { log("ac.scan ERR: " + e); }
            };""",
    ),
    (
        "E3 目标模块判定: 追加 ANTI_CHEAT_ENABLED(闸门层要拿到那个模块); main 包只认闸门相关模块",
        """        if (s.indexOf("maxApDefRate") < 0 && s.indexOf("lockMinHP") < 0 && s.indexOf("hitPauseTimeScale") < 0 && s.indexOf("AUTO_BATTLE") < 0) continue;""",
        """        if (_gateOnly) {
            /* main 包: 只包装含闸门明文(sealMethods 是 main 独有标记)的模块 */
            if (s.indexOf("sealMethods") < 0 && s.indexOf("ANTI_CHEAT_ENABLED") < 0) continue;
        } else if (s.indexOf("maxApDefRate") < 0 && s.indexOf("lockMinHP") < 0 && s.indexOf("hitPauseTimeScale") < 0 && s.indexOf("AUTO_BATTLE") < 0 &&
            s.indexOf("ANTI_CHEAT_ENABLED") < 0 && s.indexOf("finalHurtValue") < 0) continue;""",
    ),
    (
        "E4 移动系统 update 钩子: 累计未缩放墙钟(供结算一致性观测)",
        """        var oUp = up;
        Cls.prototype.update = function (list) {
            try {""",
        """        var oUp = up;
        Cls.prototype.update = function (list) {
            try { if (window.__starry_ac) window.__starry_ac.tick(this.dt); } catch (acErr) {}
            try {""",
    ),
    (
        "E5 capture 入口: 把模块注册表交给 AC 层做兜底安装",
        """window.__starry_capture = function (reg) {
  try { MOD_boot(reg); } catch (e) {
    try { if (window.__starry_nlog) window.__starry_nlog("[MODMOD] boot ERR: " + e); } catch (ignored) {}
  }
  return reg;
};""",
        """window.__starry_capture = function (reg) {
  try { MOD_boot(reg); } catch (e) {
    try { if (window.__starry_nlog) window.__starry_nlog("[MODMOD] boot ERR: " + e); } catch (ignored) {}
  }
  try { if (window.__starry_ac) window.__starry_ac.capture(reg); } catch (e) {
    try { if (window.__starry_nlog) window.__starry_nlog("[AC] capture ERR: " + e); } catch (ignored) {}
  }
  return reg;
};""",
    ),
    (
        "E7 main 包识别: sealMethods 只出现在 main bundle, 命中则切 gate-only",
        """    var targets = [];""",
        """    /* 过检测: main bundle 独有明文 sealMethods(实测仅 main/index.js 命中, 宿主模块 $LZ),
       用它判断"入口表是 main" —— 此时只跑闸门层(见 E2/E3), 不跑伤害/移速/自动战斗功能扫描。
       原生 IsTargetBundle 已放行 assets/main/index.(2026-10-07), 否则这段永远进不来。 */
    var _gateOnly = false;
    for (id in srcs) { if (srcs[id] && srcs[id].indexOf("sealMethods") >= 0) { _gateOnly = true; break; } }
    if (_gateOnly) log("bundle=MAIN (sealMethods) -> gate-only scan");
    var targets = [];""",
    ),
    (
        "E8 伤害还原层接入: baseline 仲裁出 hY 后把它交给 AC 层包装",
        """        N.entDone = 1;
        N.hX = hx;""",
        """        N.entDone = 1;
        N.hX = hx;
        /* 过检测(E8): baseline 用 RE_APPLY 仲裁出的 hY 才是真扣血口, 把它交给 AC 层。
        AC 必须包在最内层(此处早于下面 var oHX = Cls.prototype[hx];):
          feature2 关: baseline wrapper -> AC wrapper -> 原 hY  ⇒ AC 正常记账/还原
          feature2 开: baseline 提前 return ⇒ AC 不执行 ⇒ 不会凭空写累加器
        (若反序, feature2 开时 AC 先加 pd.orig 再被 baseline 清零, 累加器会被污染。) */
        try { if (window.__starry_ac) window.__starry_ac.adoptHit(Cls, hx); } catch (acErr) {}""",
    ),
    (
        "E9 新增功能开关: 追加 feature6..10 的菜单 getter (自动化层读这些)",
        """  Object.defineProperty(M, "speed", { get: function () { return V(5); } });      /* 移动速度 1..10 */
  Object.defineProperty(M, "damage", { get: function () { return V(1); } });     /* 伤害倍率 1..50 */
""",
        """  Object.defineProperty(M, "speed", { get: function () { return V(5); } });      /* 移动速度 1..10 */
  Object.defineProperty(M, "damage", { get: function () { return V(1); } });     /* 伤害倍率 1..50 */
  /* 2026-10-07 新增功能(卡密包同名功能复刻)。序号=菜单位置, 必须与 kYsxz_Features 严格一致:
       7  解锁金卡(已并入「基础功能」分类)  /  8  Category_自动化(分组占位)
       9  自动重复挑战 / 10 自动出售 / 11 自动分解 / 12 自动穿戴
     全部由 automation 层(auto_layer.js)消费, 过检测层不读这些。 */
  Object.defineProperty(M, "feature6",  { get: function () { return R(7); } });  /* 解锁金卡 */
  Object.defineProperty(M, "feature7",  { get: function () { return R(9); } });  /* 自动重复挑战 */
  Object.defineProperty(M, "feature8",  { get: function () { return R(10); } }); /* 自动出售 */
  Object.defineProperty(M, "feature9",  { get: function () { return R(11); } }); /* 自动分解 */
  Object.defineProperty(M, "feature10", { get: function () { return R(12); } }); /* 自动穿戴 */
""",
    ),
]


def sha(s: str) -> str:
    return hashlib.sha256(s.encode("utf-8")).hexdigest()


def main() -> int:
    dry = "--dry" in sys.argv
    base = BASE.read_text(encoding="utf-8")
    acl = ACL.read_text(encoding="utf-8")
    aut = AUT.read_text(encoding="utf-8")

    # ---- 前置断言: 基线与两层 ---------------------------------------------
    if base.count(MARKER) != 1:
        raise SystemExit(f"baseline 里 {MARKER} 出现 {base.count(MARKER)} 次, 期望 1 次")
    if base.count(")YSXZJS\"") or acl.count(")YSXZJS\"") or aut.count(")YSXZJS\""):
        raise SystemExit("源码里出现了 raw string 结束符 )YSXZJS\", 会截断头文件")
    for nm, txt in (("baseline", base), ("ac_layer", acl), ("auto_layer", aut)):
        if "\x00" in txt:
            raise SystemExit(f"{nm} 含 NUL 字节 (ysxz 用 strlen 注入, 会被截断)")
    if "window.__starry_ac" not in acl:
        raise SystemExit("ac_layer.js 未定义 window.__starry_ac")
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

    # 顺序: 标记行 -> 过检测层(ac) -> 自动化层(auto) -> 基线 IIFE。
    # ac 在前是为了保持过检测层已验证的日志顺序与 diff 稳定; auto 里引用
    # window.__starry_ac / window.MOD 都发生在【运行时】(1s 后的定时器), 不依赖加载顺序。
    combined = head + acl.rstrip("\n") + "\n" + aut.rstrip("\n") + "\n" + body

    # ---- 合成后自检 --------------------------------------------------------
    checks = [
        (combined.count(MARKER) == 1, f"{MARKER} 恰好 1 次"),
        (combined.count("window.__starry_ac") >= 6, "AC 层引用点齐全"),
        ("noteDamage(_d0, d)" in combined, "E1 生效"),
        ("__starry_ac.scan(_ex, mid)" in combined, "E2 生效"),
        ("(function (mid, entry) {" in combined, "E2 的 mid 在作用域内(IIFE 形参)"),
        ("__starry_ac.clearPending()" in combined, "E6 生效"),
        ('s.indexOf("ANTI_CHEAT_ENABLED") < 0' in combined, "E3 生效"),
        ('s.indexOf("finalHurtValue") < 0' in combined, "E3 生效(finalHurtValue)"),
        ("__starry_ac.tick(this.dt)" in combined, "E4 生效"),
        ("__starry_ac.capture(reg)" in combined, "E5 生效"),
        ("__starry_ac.adoptHit(Cls, hx)" in combined, "E8 生效(伤害还原层接入)"),
        ("var _gateOnly = false;" in combined, "E7 生效(_gateOnly 声明)"),
        ('srcs[id].indexOf("sealMethods") >= 0' in combined, "E7 生效(main 包识别)"),
        ("if (_gateOnly) {" in combined, "E3 生效(gate-only 目标判定)"),
        ("if (!_gateOnly) {" in combined, "E2 生效(gate-only 跳过功能扫描)"),
        (combined.count("MOD.feature4") == 1, "feature4 出现次数未变(1)"),
        (combined.count("MOD.feature1") == 2, "feature1 出现次数未变(2)"),
        ("if (MOD.speed > 1) {" in combined, "移速滑条判定保留"),
        ('indexOf("bVs(")' not in combined, "自动战斗指纹未写死 bVs"),
        ("function tryAuto(Cls)" in combined, "tryAuto 保留"),
        ("if (_dm > 1) d = d * _dm;" in combined, "伤害倍率咽喉保留"),
        ("if (_f1) d = Math.ceil(d * 100);" in combined, "秒杀咽喉保留"),
        ('Object.defineProperty(M, "damage"' in combined, "damage 滑块绑定保留"),
        ("dmg boosted #" in combined, "AC 证据链⓪(伤害被放大)保留"),
        ("dmg hit #" in combined, "AC 证据链①(hY 被调用)保留"),
        ("dmg fix #" in combined, "AC 证据链②(累加器还原写回)保留"),
        ("AC.evHit < 3" in combined, "AC 证据链①有上限(3 条,不刷屏)"),
        ("AC.evFix < 3" in combined, "AC 证据链②有上限(3 条,不刷屏)"),
        ("dmgEvNote" in combined and "dmgEvFix" in combined, "AC 证据计数已进 __AC_STATE__"),
        ("report #" in combined and "AC.evReport < 3" in combined, "AC 证据链③(结算出口)保留且封顶"),
        ("repEv" in combined, "AC 结算证据计数已进 __AC_STATE__"),
        # ---- 自动化层(auto_layer.js) 与 E9 新开关 ----
        (combined.count("window.__starry_auto") >= 2, "automation 层已接入且定义 window.__starry_auto"),
        ("window.__AUTO_STATE__" in combined, "automation 可观测口 __AUTO_STATE__ 存在"),
        ('Object.defineProperty(M, "feature6"' in combined, "E9 生效(feature6 解锁金卡)"),
        ('Object.defineProperty(M, "feature10"' in combined, "E9 生效(feature10 自动穿戴)"),
        ("return R(7); } });" in combined, "E9 序号: feature6 -> R(7)"),
        ("return R(9); } });" in combined, "E9 序号: feature7 -> R(9)"),
        ("return R(10); } });" in combined, "E9 序号: feature8 -> R(10)"),
        ("return R(11); } });" in combined, "E9 序号: feature9 -> R(11)"),
        ("return R(12); } });" in combined, "E9 序号: feature10 -> R(12)"),
        ("proto[hit.name] = w;" in combined, "解锁金卡: 按发现结果包装原型方法(不写死短名)"),
        ("goldCandidates" in combined and 'src.indexOf("$eZ.buff")' in combined,
         "解锁金卡: 结构锚点(silver/Premium/buffId/$eZ.buff)仍在"),
        ("goldAcceptBool" in combined and 'typeof r === "boolean"' in combined,
         "解锁金卡: 行为甄别(只认返回 boolean 的持有判定)"),
        ('cardType === "gold"' in combined, "解锁金卡: 只放行 gold 卡类型"),
        ("AdventureFinishPanel" in combined and "cur.efi();" in combined, "自动重复挑战: 点结算面板 efi()"),
        ("A.repeatToken" in combined, "自动重复挑战: token 去重(防重复点击)"),
        ("autoBattleFlag" in combined, "自动重复挑战: 游戏内挑战位同步"),
        ("bag.bYK(sellQ)" in combined, "自动出售: bag.bYK"),
        ("bag.bYJ(decoQ)" in combined, "自动分解: bag.bYJ"),
        ("lm.cnp : lm.cnq" in combined, "出售/分解品质取自游戏内勾选"),
        ("sellQ = [1, 2, 3];" in combined and "decoQ = [1, 2, 3];" in combined, "品质空兜底 [1,2,3] 保留"),
        ("eq.fvH.call(eq, list)" in combined, "自动穿戴: equip.fvH(list)"),
        ("eq.ftA()" in combined, "自动穿戴: equip.ftA() 取待替换列表"),
        ("A.inBattleLatch" in combined, "回城才触发(战斗->城镇 latch)"),
        ("now - A.lastCleanup < 5000" in combined, "两次清理间隔 >=5s"),
        ("}, 900);" in combined and "}, 2100);" in combined, "错峰 900ms 出售分解 / 2100ms 穿戴"),
        ("A.tickN === 15 || A.tickN === 60" in combined, "automation 两张一次性状态快照保留"),
        ("AUTO_VER = 2" in combined, "automation 版本仲裁 AUTO_VER = 2"),
        ("FEAT(6)" in combined and "FEAT(10)" in combined, "automation 开关走 FEAT(6)..FEAT(10)"),
    ]
    bad = [msg for ok, msg in checks if not ok]
    if bad:
        raise SystemExit("合成自检失败:\n  - " + "\n  - ".join(bad))

    if dry:
        print("\n".join(report))
        print(f"[dry] 未写文件。合成后 {len(combined)} B / {combined.count(chr(10))+1} 行")
        return 0

    OUT_JS.write_text(combined, encoding="utf-8")
    proc = subprocess.run(["node", "--check", str(OUT_JS)], capture_output=True, text=True)
    if proc.returncode != 0:
        raise SystemExit("node --check 失败:\n" + proc.stderr)

    header = (
        "/* Auto-generated by 04_scripts/勇士之心/ysxz_anticheat/build_ac_patch.py\n"
        " * 来源: baseline_patch_from_shipped_so.js (从出货 so 抽出的权威基线)\n"
        " *     + ac_layer.js (卡密包 v66 过检测四层复刻)\n"
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
    HEADER.write_text(header, encoding="utf-8")

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
