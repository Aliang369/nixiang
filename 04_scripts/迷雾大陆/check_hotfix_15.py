#!/usr/bin/env python3
"""Verify the live 15.0.0 code bundle and the exact Mwdl splice points.

This is a pre-build gate. It fetches the current native_manifest/15.0.0.json,
downloads the code bundle named by that manifest, verifies unique anchors, then
applies the same text replacements as MwdlMod.cpp and runs `node --check`.
"""
from __future__ import annotations

import hashlib
import json
import subprocess
import sys
import urllib.request
from pathlib import Path

ROOT = Path("/Users/aliang/Documents/nixiang")
OUT_DIR = ROOT / "06_dumped/迷雾大陆"
VERSION = "15.0.0"
CDN = "https://poyun-mwdl-res-v1.hortorgames.com/taptap"
ACTIVITY = "org.cocos2dx.javascript.AppActivity"

DAMAGE_ANCHOR = "i.target.damage=E}"
SPEED_ANCHOR = "this.AttributeComponent.getAttr(RS.MOV_SPEED)*t}"
TICK_ANCHOR = "this.isDormant||(this.SkillComponent.tick(t)"
CHEAT_ANCHOR = "t.prototype.onStageCheat=function(t){"


def fetch(url: str) -> bytes:
    req = urllib.request.Request(url, headers={"User-Agent": "nixiang-mwdl-anchor-check/1"})
    with urllib.request.urlopen(req, timeout=30) as response:
        return response.read()


def call(method: str, signature: str, index: int, ret: str) -> str:
    return (
        f'jsb.reflection.callStaticMethod("{ACTIVITY}","{method}",'
        f'"{signature}",{index}){ret}'
    )


def patch_code(text: str) -> str:
    if text.count(DAMAGE_ANCHOR) != 1:
        raise RuntimeError(f"damage anchor count={text.count(DAMAGE_ANCHOR)}")
    damage = (
        "/*starry_mwdl_kh*/var _su=i.sourceUnit;if(_su&&(_su.unitType===ow.Hero||"
        "_su.unitType===ow.Summon||_su.unitType===ow.Guardian||"
        "_su.unitType===ow.Player||_su.unitType===ow.ParasyteGuardian)){"
        + call("get_ret", "(I)Z", 1, "&&(E*=100)")
        + "}if(_su&&_su.unitType===ow.Monster&&"
        + call("get_ret", "(I)Z", 2, "){E=0}")
        + "i.target.damage=E}"
    )
    text = text.replace(DAMAGE_ANCHOR, damage, 1)

    if text.count(SPEED_ANCHOR) != 1:
        raise RuntimeError(f"speed anchor count={text.count(SPEED_ANCHOR)}")
    speed = (
        "/*starry_mwdl_spd*/function(n){try{var m="
        + call("get_val", "(I)F", 3, ";")
        + "m>1&&(this.unitType===ow.Hero||this.unitType===ow.Summon||"
        "this.unitType===ow.Guardian||this.unitType===ow.Player||"
        "this.unitType===ow.ParasyteGuardian)&&(n*=m)}catch(_e){}return "
        "n}.call(this,this.AttributeComponent.getAttr(RS.MOV_SPEED)*t)}"
    )
    text = text.replace(SPEED_ANCHOR, speed, 1)

    if text.count(TICK_ANCHOR) != 2:
        raise RuntimeError(f"tick anchor count={text.count(TICK_ANCHOR)}")
    tick_anchor = TICK_ANCHOR
    tick_pos = text.find(tick_anchor)
    insert_at = text.find("(", tick_pos)
    if insert_at < 0:
        raise RuntimeError("tick insertion point missing")
    tick = (
        "/*starry_mwdl_uls*/ "
        + call("get_ret", "(I)Z", 4, "")
        + "&&(this.unitType===ow.Hero||this.unitType===ow.Summon||"
        "this.unitType===ow.Guardian||this.unitType===ow.Player||"
        "this.unitType===ow.ParasyteGuardian)&&"
        "(this.SkillComponent.reduceAllSkillColdDown(1e9),"
        "this.ActiveSkillComponent&&this.ActiveSkillComponent.reduceAllSkillColdDown(1e9)), "
    )
    text = text[: insert_at + 1] + tick + text[insert_at + 1 :]

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
        "enum_ow": code_text.count("}(ow||(ow={}))"),
        "attr_RS": code_text.count("RS.MOV_SPEED"),
        "sourceUnit": code_text.count("this.sourceUnit=t"),
    }
    expected = {
        "damage_anchor": 1,
        "speed_anchor": 1,
        "tick_anchor": 2,
        "cheat_anchor": 1,
        "enum_ow": 1,
        "attr_RS": 5,
        "sourceUnit": 1,
    }
    if checks != expected:
        raise RuntimeError(f"anchor mismatch: got={checks} expected={expected}")

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
