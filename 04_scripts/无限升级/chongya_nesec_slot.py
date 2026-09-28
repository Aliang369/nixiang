#!/usr/bin/env python3.11
"""Attach the nesec dispatch-slot probe to every Chongya :stub* process.

Unlike the earlier driver this never spawns or child-gates Chongya itself:
the host app is only ever attached at :stub* level, so we do not trip the
host's own libmsaoaidsec startup crash.

Usage:
    python3.11 chongya_nesec_slot.py --patch-slot --duration 180
"""

import argparse
import json
import os
import sys
import time
from datetime import datetime

import frida


SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROBE = os.path.join(SCRIPT_DIR, "chongya_nesec_slot.js")


def make_handler(label, out_file, verbose):
    def on_message(message, data):
        with open(out_file, "a", encoding="utf-8") as f:
            f.write(json.dumps({"label": label, "message": message}, ensure_ascii=False) + "\n")
        if message.get("type") != "send":
            print(f"[{label}] {message}", flush=True)
            return
        payload = message.get("payload")
        if not isinstance(payload, dict):
            print(f"[{label}] send {payload}", flush=True)
            return
        kind = payload.get("kind", "send")
        if kind == "probe-ready":
            print(f"[{label}] ready pid={payload.get('pid')} patchSlot={payload.get('patchSlot')}", flush=True)
        elif kind == "nesec-loaded":
            slot = payload.get("slot", {})
            print(
                f"[{label}] NESEC LOADED pid={payload.get('pid')} base={payload.get('module', {}).get('base')} "
                f"slot={slot.get('addr')} value={slot.get('value')} err={slot.get('error')} "
                f"prot={slot.get('range', {}).get('prot')}",
                flush=True,
            )
            print(f"[{label}]   slot bytes: {slot.get('bytes')}", flush=True)
            print(f"[{label}]   fn bytes  : {payload.get('fnBytes')}", flush=True)
            print(f"[{label}]   anon r-x  : {payload.get('anonExec')}", flush=True)
        elif kind == "slot-patch":
            print(
                f"[{label}] SLOT PATCH before={payload.get('before')} after={payload.get('after')} "
                f"attempts={payload.get('attempts')}",
                flush=True,
            )
        elif kind == "sigill":
            print(
                f"[{label}] SIGILL pid={payload.get('pid')} pc={payload.get('pc')} lr={payload.get('lr')} "
                f"lrOff={payload.get('lrOffset')} isNesec={payload.get('isNesec')}",
                flush=True,
            )
            print(f"[{label}]   regs: {payload.get('regs')}", flush=True)
            print(f"[{label}]   pcRange: {payload.get('pcRange')}", flush=True)
        elif kind in ("sigill-skip", "sigill-skip-error"):
            print(f"[{label}] {kind} {payload}", flush=True)
        elif verbose:
            print(f"[{label}] {kind} {json.dumps(payload, ensure_ascii=False)[:400]}", flush=True)

    return on_message


def load_probe(session, label, out_file, config, verbose):
    with open(PROBE, "r", encoding="utf-8") as f:
        source = f.read()
    source = "var PROBE_CONFIG = " + json.dumps(config) + ";\n" + source
    script = session.create_script(source, name="chongya_nesec_slot")
    script.on("message", make_handler(label, out_file, verbose))
    script.load()
    return script


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--device", default="f9a116a2")
    ap.add_argument("--package", default="com.joke.chongya")
    ap.add_argument("--guest", default="com.xingzuodaoshi.infinityUpgrade")
    ap.add_argument("--skip-pid", type=int, action="append", default=[], help="pid to never attach (e.g. the direct-launch copy)")
    ap.add_argument("--duration", type=int, default=180)
    ap.add_argument("--outdir", default="/tmp/wxsj_chongya_probe")
    ap.add_argument("--patch-slot", action="store_true", help="write 0 into libnesec+0x105ff0")
    ap.add_argument("--skip-sigill", action="store_true", help="redirect nesec SIGILL to the fallback")
    ap.add_argument("--verbose", action="store_true")
    args = ap.parse_args()

    os.makedirs(args.outdir, exist_ok=True)
    stamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    events_file = os.path.join(args.outdir, f"slot_{stamp}.jsonl")
    config = {"patchSlot": args.patch_slot, "skipSigill": args.skip_sigill}

    device = frida.get_device(args.device, timeout=10)
    print(f"[driver] events={events_file} config={config}", flush=True)

    sessions = {}
    skip = set(args.skip_pid)
    seen = {}
    attempts = {}
    MAX_ATTEMPTS = 60
    deadline = time.time() + args.duration

    while time.time() < deadline:
        try:
            processes = device.enumerate_processes()
        except Exception as e:
            print(f"[driver] enumerate error: {e}", flush=True)
            time.sleep(0.5)
            continue

        for process in processes:
            name = process.name or ""
            if process.pid not in seen:
                seen[process.pid] = name
                if name.startswith(args.package) or name == args.guest:
                    print(f"[driver] seen pid={process.pid} name={name}", flush=True)
            interesting = (
                name.startswith(args.package + ":") or name == args.guest
            )
            if not interesting:
                continue
            if process.pid in skip:
                continue
            if process.pid in sessions:
                continue
            if attempts.get(process.pid, 0) >= MAX_ATTEMPTS:
                continue
            attempts[process.pid] = attempts.get(process.pid, 0) + 1
            try:
                session = device.attach(process.pid, timeout=1)
                script = load_probe(session, name + "-" + str(process.pid), events_file, config, args.verbose)
                sessions[process.pid] = (session, script)
                print(f"[driver] attached {name} pid={process.pid} (attempt {attempts[process.pid]})", flush=True)
            except Exception as e:
                n = attempts[process.pid]
                if n == 1 or n % 10 == 0:
                    print(f"[driver] attach {name} pid={process.pid} failed (attempt {n}): {e}", flush=True)

        time.sleep(0.02)

    print(f"[driver] duration reached; attached={sorted(sessions)} attempted={sorted(attempts)}", flush=True)
    for pid, (session, _script) in sessions.items():
        try:
            session.detach()
        except Exception:
            pass
    return 0


if __name__ == "__main__":
    sys.exit(main())
