#!/usr/bin/env python3.11
"""Attach the nesec SIGILL probe to a Chongya guest process.

The driver keeps the parent process under Frida child gating, so a newly
spawned :stub* child is paused until the probe script is loaded.
"""

import argparse
import json
import os
import sys
import time
from datetime import datetime

import frida


SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROBE = os.path.join(SCRIPT_DIR, "chongya_nesec_probe.js")


def find_parent_pid(device, package):
    for process in device.enumerate_processes():
        if process.name == package:
            return process.pid
    return None


def make_message_handler(label, out_file):
    def on_message(message, data):
        record = {"label": label, "message": message}
        if data is not None:
            record["data_len"] = len(data)
        with open(out_file, "a", encoding="utf-8") as f:
            f.write(json.dumps(record, ensure_ascii=False) + "\n")

        if message.get("type") == "send":
            payload = message.get("payload")
            if isinstance(payload, dict):
                kind = payload.get("kind", "send")
                if kind == "probe-ready":
                    print(f"[{label}] ready pid={payload.get('pid')} modules={len(payload.get('modules', []))}", flush=True)
                elif kind == "nesec-sigill":
                    print(
                        f"[{label}] SIGILL hit={payload.get('hit')} pc={payload.get('pc')} "
                        f"lr={payload.get('lr')} lrOff={payload.get('lrOffset')}",
                        flush=True,
                    )
                elif kind in ("nesec-skip", "nesec-skip-error"):
                    print(f"[{label}] {kind} {payload}", flush=True)
                else:
                    print(f"[{label}] send {payload}", flush=True)
            else:
                print(f"[{label}] send {payload}", flush=True)
        elif message.get("type") == "error":
            print(f"[{label}] script error: {message}", flush=True)
        else:
            print(f"[{label}] {message}", flush=True)

    return on_message


def load_probe(session, label, out_file):
    with open(PROBE, "r", encoding="utf-8") as f:
        source = f.read()
    script = session.create_script(source, name="chongya_nesec_probe")
    script.on("message", make_message_handler(label, out_file))
    script.load()
    return script


def child_description(child):
    parts = []
    for name in ("pid", "path", "argv"):
        try:
            parts.append(f"{name}={getattr(child, name)}")
        except Exception:
            pass
    return " ".join(parts)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--device", default="f9a116a2")
    parser.add_argument("--package", default="com.joke.chongya")
    parser.add_argument("--attach-pid", type=int)
    parser.add_argument("--spawn", action="store_true")
    parser.add_argument("--duration", type=int, default=300)
    parser.add_argument("--outdir", default="/tmp/wxsj_chongya_probe")
    args = parser.parse_args()

    os.makedirs(args.outdir, exist_ok=True)
    stamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    events_file = os.path.join(args.outdir, f"events_{stamp}.jsonl")

    device = frida.get_device(args.device, timeout=10)
    system = device.query_system_parameters()
    print(f"[driver] device={device.id} access={system.get('access')}", flush=True)
    print(f"[driver] events={events_file}", flush=True)

    parent_pid = args.attach_pid
    spawned = False
    if parent_pid is None and not args.spawn:
        parent_pid = find_parent_pid(device, args.package)
        if parent_pid is not None:
            print(f"[driver] found existing parent pid={parent_pid}", flush=True)

    if parent_pid is None:
        parent_pid = device.spawn([args.package])
        spawned = True
        print(f"[driver] spawned parent pid={parent_pid}", flush=True)

    parent_session = device.attach(parent_pid)
    parent_session.enable_child_gating()
    parent_script = load_probe(parent_session, "parent", events_file)
    print("[driver] child gating enabled", flush=True)

    if spawned:
        device.resume(parent_pid)
        print("[driver] parent resumed", flush=True)

    pending_seen = set()
    children = {}
    deadline = time.time() + args.duration

    while time.time() < deadline:
        candidates = []
        try:
            candidates.extend(device.enumerate_pending_children())
        except Exception as e:
            print(f"[driver] enumerate_pending_children error: {e}", flush=True)
        try:
            for process in device.enumerate_processes():
                if process.name.startswith(args.package + ":stub"):
                    candidates.append(process)
        except Exception as e:
            print(f"[driver] enumerate_processes error: {e}", flush=True)

        for child in candidates:
            pid = getattr(child, "pid", None)
            if pid is None or pid in pending_seen:
                continue
            pending_seen.add(pid)
            print(f"[driver] child candidate {child_description(child)}", flush=True)
            try:
                session = device.attach(pid)
                session.enable_child_gating()
                script = load_probe(session, f"child-{pid}", events_file)
                children[pid] = (session, script)
                print(f"[driver] child {pid} probe loaded", flush=True)
            except Exception as e:
                print(f"[driver] child {pid} probe error: {e}", flush=True)
            finally:
                try:
                    device.resume(pid)
                    print(f"[driver] child {pid} resumed", flush=True)
                except Exception as e:
                    print(f"[driver] child {pid} resume error: {e}", flush=True)

        time.sleep(0.1)

    print(f"[driver] duration reached; seen={sorted(pending_seen)}", flush=True)
    for pid, (session, _script) in children.items():
        try:
            session.detach()
        except Exception:
            pass
    try:
        parent_session.detach()
    except Exception:
        pass
    return 0


if __name__ == "__main__":
    sys.exit(main())
