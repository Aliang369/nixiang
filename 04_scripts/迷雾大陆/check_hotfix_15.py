#!/usr/bin/env python3
"""Verify the live 15.0.0 code bundle and the exact Mwdl splice points.

This is a pre-build gate. It fetches the current native_manifest/15.0.0.json,
downloads the code bundle named by that manifest, verifies unique anchors, then
applies the same text replacements as MwdlMod.cpp and runs `node --check`.

Bundle drift history (same game version 15.0.0, bundle token changes):
  a1e47  unit-enum `ow`  attr `RS.MOV_SPEED`  damage tail `i.target.damage=E}`
  bffa2  unit-enum `gA`  attr `PS.MOV_SPEED`  damage tail `i.target.damage=P}`

The damage tail anchor ends in the minified *local* variable name, so it is not
interchangeable across tokens: a stale hook would emit JS referencing an
undeclared variable. `check_scope_reachability` additionally proves the unit
enum is declared at top level, so referencing it inside the damage function
cannot raise ReferenceError (the 15.0.0 `jA` incident class).
"""
from __future__ import annotations

import hashlib
import json
import re
import subprocess
import sys
import urllib.request
from pathlib import Path

ROOT = Path("/Users/aliang/Documents/nixiang")
OUT_DIR = ROOT / "06_dumped/迷雾大陆"
VERSION = "15.0.0"
CDN = "https://poyun-mwdl-res-v1.hortorgames.com/taptap"
ACTIVITY = "org.cocos2dx.javascript.AppActivity"

UNIT_ENUM = "gA"
DECL_SUFFIX = f"||({UNIT_ENUM}={{}}))"
DECL_CONSTRUCT = f"}}({UNIT_ENUM}{DECL_SUFFIX}"  # }(gA||(gA={}))

DAMAGE_ANCHOR = "i.target.damage=P}"
SPEED_ANCHOR = "this.AttributeComponent.getAttr(PS.MOV_SPEED)*t}"
TICK_ANCHOR = "this.isDormant||(this.SkillComponent.tick(t)"
CHEAT_ANCHOR = "t.prototype.onStageCheat=function(t){"

HERO_UNIT_TYPES = (
    f"{UNIT_ENUM}.Hero||"
    f"_su.unitType==={UNIT_ENUM}.Summon||"
    f"_su.unitType==={UNIT_ENUM}.Guardian||"
    f"_su.unitType==={UNIT_ENUM}.Player||"
    f"_su.unitType==={UNIT_ENUM}.ParasyteGuardian"
)
SELF_UNIT_TYPES = (
    f"this.unitType==={UNIT_ENUM}.Hero||"
    f"this.unitType==={UNIT_ENUM}.Summon||"
    f"this.unitType==={UNIT_ENUM}.Guardian||"
    f"this.unitType==={UNIT_ENUM}.Player||"
    f"this.unitType==={UNIT_ENUM}.ParasyteGuardian"
)


def fetch(url: str) -> bytes:
    req = urllib.request.Request(url, headers={"User-Agent": "nixiang-mwdl-anchor-check/1"})
    with urllib.request.urlopen(req, timeout=30) as response:
        return response.read()


def call(method: str, signature: str, index: int, ret: str) -> str:
    return (
        f'jsb.reflection.callStaticMethod("{ACTIVITY}","{method}",'
        f'"{signature}",{index}){ret}'
    )


def brace_depths(text: str, offsets) -> dict:
    """Brace depth at each offset, skipping strings/comments/regex literals.

    Depth 0 = outermost (bundle-global) scope.
    """
    want = set(offsets)
    found: dict[int, int] = {}
    depth = 0
    prev = None
    i, n = 0, len(text)
    while i < n:
        if i in want:
            found[i] = depth
        c = text[i]
        if c == "/" and i + 1 < n and text[i + 1] == "/":
            j = text.find("\n", i)
            i = n if j < 0 else j
            continue
        if c == "/" and i + 1 < n and text[i + 1] == "*":
            j = text.find("*/", i + 2)
            i = n if j < 0 else j + 2
            continue
        if c in "\"'`":
            quote = c
            i += 1
            while i < n:
                if text[i] == "\\":
                    i += 2
                    continue
                if text[i] == quote:
                    i += 1
                    break
                i += 1
            prev = "x"
            continue
        if c == "/" and prev in "(,=:[!&|?{};+-*%~^<>":
            i += 1
            while i < n:
                ch = text[i]
                if ch == "\\":
                    i += 2
                    continue
                if ch == "[":
                    while i < n and text[i] != "]":
                        if text[i] == "\\":
                            i += 2
                            continue
                        i += 1
                if ch == "/":
                    i += 1
                    break
                i += 1
            prev = "x"
            continue
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
        if not c.isspace():
            prev = c
        i += 1
    return found


def check_scope_reachability(text: str, anchor_pos: int) -> dict:
    """Prove the unit enum is reachable from the damage splice site.

    The bundle is one top-level IIFE (`!function(){"use strict";...}()`), so
    depth 1 is the bundle-global scope and every nested function can see it.
    Calibrated A/B against the user-verified token a1e47 (unit enum `ow`):
    decl depth 1 / anchor depth 2 / nearest enum use 2199 B, vs bffa2 `gA`:
    decl depth 1 / anchor depth 2 / nearest use 2162 B.
    """
    decl_pos = text.find(DECL_CONSTRUCT)
    if decl_pos < 0:
        raise RuntimeError(f"unit enum declaration {DECL_CONSTRUCT} not found")
    decl_end = decl_pos + len(DECL_CONSTRUCT)
    depths = brace_depths(text, (decl_end, anchor_pos))
    decl_depth = depths.get(decl_end)
    anchor_depth = depths.get(anchor_pos)
    if decl_depth is None or anchor_depth is None:
        raise RuntimeError("scope scan failed to reach target offsets")
    if decl_depth > 1:
        raise RuntimeError(
            f"unit enum {UNIT_ENUM} declared at depth {decl_depth} (expected <=1, the "
            "bundle-global scope of the top-level IIFE): splicing it into a nested "
            "function may raise ReferenceError"
        )
    if anchor_depth <= decl_depth:
        raise RuntimeError(
            f"damage anchor depth {anchor_depth} is not nested below enum depth {decl_depth}"
        )
    near = text.find(f"{UNIT_ENUM}.", anchor_pos)
    return {
        "enum_depth": decl_depth,
        "anchor_depth": anchor_depth,
        "next_enum_use_delta": (near - anchor_pos) if near >= 0 else None,
    }


def damage_hook() -> str:
    return (
        f"/*starry_mwdl_kh*/var _su=i.sourceUnit;if(_su&&(_su.unitType==={HERO_UNIT_TYPES})){{"
        + call("get_ret", "(I)Z", 1, "&&(P*=100)")
        + f"}}if(_su&&_su.unitType==={UNIT_ENUM}.Monster&&"
        + call("get_ret", "(I)Z", 2, "){P=0}")
        + "i.target.damage=P}"
    )


def speed_hook() -> str:
    return (
        "/*starry_mwdl_spd*/function(n){try{var m="
        + call("get_val", "(I)F", 3, ";")
        + f"m>1&&({SELF_UNIT_TYPES})&&(n*=m)}}catch(_e){{}}return "
        "n}.call(this,this.AttributeComponent.getAttr(PS.MOV_SPEED)*t)}"
    )


def tick_hook() -> str:
    return (
        "/*starry_mwdl_uls*/ "
        + call("get_ret", "(I)Z", 4, "")
        + f"&&({SELF_UNIT_TYPES})&&"
        "(this.SkillComponent.reduceAllSkillColdDown(1e9),"
        "this.ActiveSkillComponent&&this.ActiveSkillComponent.reduceAllSkillColdDown(1e9)), "
    )


CPP_PATH = ROOT / "mod菜单/starry-mod-menu/app/src/main/jni/Games/MwdlMod.cpp"


def cpp_hook_literals(cpp_text: str, buffer: str) -> str:
    """Concatenate the adjacent C string literals passed to snprintf for `buffer`."""
    marker = f"{buffer}, sizeof({buffer}),"
    start = cpp_text.find(marker)
    if start < 0:
        raise RuntimeError(f"{buffer} snprintf not found in {CPP_PATH}")
    end = cpp_text.find("kAct", start)
    if end < 0:
        raise RuntimeError(f"{buffer} snprintf args not found")
    body = cpp_text[start:end]
    parts = []
    for raw in re.findall(r'"((?:[^"\\]|\\.)*)"', body):
        parts.append(raw.replace('\\"', '"').replace("\\\\", "\\").replace("\\n", "\n"))
    return "".join(parts)


def check_cpp_hook_parity() -> dict:
    """The gate and the shipped .so must splice byte-identical JavaScript."""
    cpp_text = CPP_PATH.read_text(encoding="utf-8")
    act = 'org.cocos2dx.javascript.AppActivity'
    pairs = (
        ("kKhHook", damage_hook(), 2),
        ("kSpdRepl", speed_hook(), 1),
        ("kTickHook", tick_hook(), 1),
    )
    report = {}
    for buffer, expected, arg_count in pairs:
        literal = cpp_hook_literals(cpp_text, buffer)
        if literal.count("%s") != arg_count:
            raise RuntimeError(
                f"{buffer}: expected {arg_count} %s placeholder(s), got {literal.count('%s')}"
            )
        rendered = literal.replace("%s", act)
        if rendered != expected:
            raise RuntimeError(
                f"{buffer} diverged from check_hotfix_15.py — update both together.\n"
                f"  cpp: {rendered[:200]}\n  py : {expected[:200]}"
            )
        report[buffer] = len(rendered)
    return report


def patch_code(text: str) -> str:
    if text.count(DAMAGE_ANCHOR) != 1:
        raise RuntimeError(f"damage anchor count={text.count(DAMAGE_ANCHOR)}")
    text = text.replace(DAMAGE_ANCHOR, damage_hook(), 1)

    if text.count(SPEED_ANCHOR) != 1:
        raise RuntimeError(f"speed anchor count={text.count(SPEED_ANCHOR)}")
    text = text.replace(SPEED_ANCHOR, speed_hook(), 1)

    if text.count(TICK_ANCHOR) != 2:
        raise RuntimeError(f"tick anchor count={text.count(TICK_ANCHOR)}")
    tick_pos = text.find(TICK_ANCHOR)
    insert_at = text.find("(", tick_pos)
    if insert_at < 0:
        raise RuntimeError("tick insertion point missing")
    text = text[: insert_at + 1] + tick_hook() + text[insert_at + 1 :]

    if text.count(CHEAT_ANCHOR) != 1:
        raise RuntimeError(f"cheat anchor count={text.count(CHEAT_ANCHOR)}")
    cheat_pos = text.find(CHEAT_ANCHOR)
    cheat_end = text.find("}", cheat_pos + len(CHEAT_ANCHOR))
    if cheat_end < 0:
        raise RuntimeError("cheat method end missing")
    text = (
        text[:cheat_pos]
        + "/*starry_mwdl_nocheat_stage*/t.prototype.onStageCheat=function(t){}"
        + text[cheat_end + 1 :]
    )
    return text


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    manifest_bytes = fetch(f"{CDN}/remote/native_manifest/{VERSION}.json")
    manifest = json.loads(manifest_bytes)
    code_version = manifest["bundleVers"]["code"]
    code_bytes = fetch(f"{CDN}/remote/code/index.{code_version}.js")
    code_text = code_bytes.decode("utf-8")

    checks = {
        "damage_anchor": code_text.count(DAMAGE_ANCHOR),
        "speed_anchor": code_text.count(SPEED_ANCHOR),
        "tick_anchor": code_text.count(TICK_ANCHOR),
        "cheat_anchor": code_text.count(CHEAT_ANCHOR),
        "enum_gA": code_text.count(DECL_CONSTRUCT),
        "attr_PS": code_text.count("PS.MOV_SPEED"),
        "sourceUnit": code_text.count("this.sourceUnit=t"),
    }
    expected = {
        "damage_anchor": 1,
        "speed_anchor": 1,
        "tick_anchor": 2,
        "cheat_anchor": 1,
        "enum_gA": 1,
        "attr_PS": 5,
        "sourceUnit": 1,
    }
    if checks != expected:
        raise RuntimeError(f"anchor mismatch: got={checks} expected={expected}")

    scope = check_scope_reachability(code_text, code_text.find(DAMAGE_ANCHOR))
    parity = check_cpp_hook_parity()

    patched = patch_code(code_text)
    src_md5 = hashlib.md5(code_bytes).hexdigest()
    code_path = OUT_DIR / f"code_index.{code_version}.js"
    patched_path = OUT_DIR / f"code_index.{code_version}.patched.check.js"
    result_path = OUT_DIR / "anchor_check_15.0.0.json"
    code_path.write_bytes(code_bytes)
    patched_path.write_text(patched, encoding="utf-8")
    subprocess.run(["node", "--check", str(patched_path)], check=True)

    result = {
        "ok": True,
        "version": VERSION,
        "code_version": code_version,
        "code_size": len(code_bytes),
        "code_md5": src_md5,
        "checks": checks,
        "scope": scope,
        "cpp_hook_parity": parity,
        "patched_size": len(patched.encode("utf-8")),
        "node_check": "ok",
    }
    result_path.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        raise SystemExit(1)
