#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""勇士之心 · 诱饵层契约门禁（2026-10-27「卸载重装卡加载页」事故后新增）。

背景（一句话）：原生 splice 会把主 bundle 的注册表调用改写成
    }(window.__starry_capture({ … }), {}, [ …
所以**任何被注入的 JS（真载荷或诱饵）都必须定义 `window.__starry_capture` 并原样
返回注册表**。诱饵当初漏了这条，无票路径直接把 `assets/main/index.js` 打死 ⇒
`ScriptEngine::evalString … failed` ⇒ 引擎永远停在「正在为您加载游戏资源」。

本门禁检查三件事（任一不过即 rc=1，构建应当中止）：
  1. `YsxzDecoyJs.h` 里的诱饵 JS 能被 `node --check` 通过（语法合法）；
  2. 诱饵定义了 `window.__starry_capture` 且 `return reg`（透传，不返回 undefined）；
  3. 用 node 跑一遍「splice 契约」仿真：诱饵 + 改写后的注册表调用必须把注册表
     原样传进 bundle 工厂；并做**负向对照**（把透传那几行抠掉必须 FAIL），
     证明这条检查真的抓得住当初的 bug，而不是一条永远 PASS 的假门禁。
另加一条静态断言：`YsxzMod.cpp` 里必须保留修法 B（`sgpjs::ready()` + skip splice），
防止有人把「解不开就不改写」这层保险删掉。

用法：
  python3 04_scripts/勇士之心/check_decoy_js.py
  python3 04_scripts/勇士之心/check_decoy_js.py --quiet
"""
from __future__ import annotations

import argparse
import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent.parent
JNI = ROOT / "mod菜单" / "starry-mod-menu" / "app" / "src" / "main" / "jni"
DECOY = JNI / "Games" / "YsxzDecoyJs.h"
MOD_CPP = HERE / "ysxz_1.0.3_menu" / "YsxzMod.cpp"

RAW_RE = re.compile(r'R"YSXZDECOY\((.*?)\)YSXZDECOY"', re.S)

# splice 契约仿真：模拟 cocos bundle 工厂 + YsxzMod.cpp::PatchRegistryCall 的改写结果
HARNESS = r"""
// ---- 宿主仿真：cocos 的 JS 引擎里 window 恒存在；node 里要自己铺 ----
globalThis.window = globalThis;
// ---- 注入体（诱饵 / 真载荷）----
%(inject)s
// ---- splice 改写后的注册表调用（与 PatchRegistryCall 的 beginNew/endNew 等价）----
var seen = null;
function bundleFactory(reg, a, b) { seen = reg; return reg && reg.k; }
var out = bundleFactory(window.__starry_capture({ k: 'OK' }), {}, [1, 2]);
if (out !== 'OK') { console.error('FAIL 注册表未被原样传入 bundle'); process.exit(3); }
if (!seen || seen.k !== 'OK') { console.error('FAIL 注册表被改写/清空'); process.exit(4); }
if (typeof window.__starry_capture !== 'function') { console.error('FAIL 透传未安装'); process.exit(5); }
console.log('HARNESS_PASS');
"""


def log(msg: str, quiet: bool = False) -> None:
    if not quiet:
        print(msg)


def extract_decoy(path: Path) -> str:
    text = path.read_text(encoding="utf-8")
    m = RAW_RE.search(text)
    if not m:
        raise SystemExit(f"FAIL 在 {path} 里找不到 R\"YSXZDECOY(...)YSXZDECOY\" 原文块")
    return m.group(1)


def node_check(node: str, js: str, tmp: Path, name: str) -> tuple[bool, str]:
    p = tmp / name
    p.write_text(js, encoding="utf-8")
    r = subprocess.run([node, "--check", str(p)], capture_output=True, text=True)
    return r.returncode == 0, (r.stdout + r.stderr).strip()


def run_harness(node: str, inject: str, tmp: Path, name: str) -> tuple[int, str]:
    p = tmp / name
    p.write_text(HARNESS % {"inject": inject}, encoding="utf-8")
    r = subprocess.run([node, str(p)], capture_output=True, text=True)
    return r.returncode, (r.stdout + r.stderr).strip()


def strip_capture(js: str) -> str:
    """负向对照用：把透传那几行抠掉（模拟事故版诱饵）。"""
    out = re.sub(
        r"window\.__starry_capture\s*=\s*function\s*\([^)]*\)\s*\{.*?\}\s*;",
        "/* capture removed for negative control */",
        js,
        flags=re.S,
    )
    return out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--quiet", action="store_true")
    ap.add_argument("--decoy", default=str(DECOY))
    ap.add_argument("--mod-cpp", default=str(MOD_CPP))
    ap.add_argument("--so", nargs="*", default=[], help="构建后校验：这些 .so 里必须含诱饵透传明文")
    args = ap.parse_args()
    quiet = args.quiet

    node = shutil.which("node")
    if not node:
        print("FAIL 找不到 node（诱饵语法与契约仿真都靠它）", file=sys.stderr)
        return 1

    decoy_path = Path(args.decoy)
    if not decoy_path.is_file():
        print(f"FAIL 诱饵文件不存在: {decoy_path}", file=sys.stderr)
        return 1
    decoy = extract_decoy(decoy_path)
    log(f"[decoy] {decoy_path}  提取 {len(decoy)} 字符", quiet)

    fails = []
    with tempfile.TemporaryDirectory(prefix="ysxz_decoy_") as td:
        tmp = Path(td)

        # 1. 语法合法
        ok, msg = node_check(node, decoy, tmp, "decoy.js")
        log(f"[1/4] node --check 诱饵 .................. {'OK' if ok else 'FAIL'}", quiet)
        if not ok:
            fails.append(("node --check", msg))

        # 2. 透传存在且原样返回
        has_fn = "window.__starry_capture" in decoy
        returns_reg = bool(re.search(r"window\.__starry_capture\s*=\s*function\s*\(\s*reg\s*\)\s*\{[^}]*return\s+reg\s*;", decoy, re.S))
        has_mark = "starry_ysxz_v1" in decoy
        log(f"[2/4] capture 定义/return reg/标记 ....... "
            f"{'OK' if (has_fn and returns_reg and has_mark) else 'FAIL'} "
            f"(fn={has_fn} return_reg={returns_reg} mark={has_mark})", quiet)
        if not (has_fn and returns_reg and has_mark):
            fails.append(("decoy contract", "缺 window.__starry_capture / return reg / starry_ysxz_v1"))

        # 3. 契约仿真（正向）
        rc, msg = run_harness(node, decoy, tmp, "harness_pos.js")
        log(f"[3/4] splice 契约仿真（诱饵） ........... {'OK' if rc == 0 else 'FAIL'}", quiet)
        if rc != 0:
            fails.append(("decoy harness", msg))

        # 4. 负向对照：抠掉透传必须 FAIL（证明这门禁抓得住事故版）
        rc_neg, _ = run_harness(node, strip_capture(decoy), tmp, "harness_neg.js")
        log(f"[4/4] 负向对照（去掉透传必须失败） ...... {'OK' if rc_neg != 0 else 'FAIL'}", quiet)
        if rc_neg == 0:
            fails.append(("negative control", "去掉 __starry_capture 后仍 PASS ⇒ 门禁是假的"))

    # 附加静态断言：修法 B 不能被删
    mod_text = Path(args.mod_cpp).read_text(encoding="utf-8")
    has_b = ("sgpjs::ready()" in mod_text) and ("skip splice" in mod_text)
    log(f"[+ ] 修法 B（解不开就不 splice） ......... {'OK' if has_b else 'FAIL'}", quiet)
    if not has_b:
        fails.append(("mod guard", f"{args.mod_cpp} 里缺 sgpjs::ready() / skip splice"))

    if fails:
        print("\n=== FAIL 明细 ===", file=sys.stderr)
        for name, msg in fails:
            print(f"- {name}: {msg}", file=sys.stderr)
        print("VERDICT FAIL —— 诱饵层会打断游戏启动，禁止出包", file=sys.stderr)
        return 1

    # 构建后追加校验：so 里必须有诱饵透传的**明文**（真载荷是密文，只有诱饵是明文）
    if args.so:
        for so in args.so:
            p = Path(so)
            if not p.is_file():
                print(f"FAIL .so 不存在: {p}", file=sys.stderr)
                return 1
            blob = p.read_bytes()
            ok = (b"window.__starry_capture" in blob) and (b"return reg" in blob)
            log(f"[so] {p.name:24s} {p.stat().st_size:>9d} B  诱饵透传明文 "
                f"{'OK' if ok else 'FAIL'}", quiet)
            if not ok:
                print(f"FAIL {p} 里没有诱饵透传明文 ⇒ 诱饵层没编进这份 so", file=sys.stderr)
                return 1

    print("VERDICT PASS —— 诱饵自包含（合法 JS + 透传 __starry_capture）、修法 B 在位")
    return 0


if __name__ == "__main__":
    sys.exit(main())
