import os
import traceback

import ida_auto
import ida_bytes
import ida_funcs
import ida_hexrays
import ida_name
import idautils
import idc


OUT = "/tmp/dwdlm_ida_startup.txt"
NEEDLES = [
    "applicationDidFinishLaunching",
    "hotUpdateOnLaunch",
    "getVersionUpdateInfo",
    "getVersionInfo",
    "onSetVersionCallback",
    "onCheckUpdateCallback",
    "checkFileMd5",
    "CResourcesManager",
    "main.js",
    "res/",
]


def string_text(ea):
    return idc.get_strlit_contents(ea, -1, idc.STRTYPE_C) or b""


def append(msg):
    with open(OUT, "a", encoding="utf-8") as f:
        f.write(msg + "\n")


def safe_decompile(fn_ea):
    try:
        cf = ida_hexrays.decompile(fn_ea)
        return str(cf) if cf else "(decompile returned None)"
    except Exception:
        return "DECOMPILE_ERROR\n" + traceback.format_exc()


def main():
    if os.path.exists(OUT):
        os.remove(OUT)
    ida_auto.auto_wait()
    targets = [0x228204, 0x22dc88, 0x22dfe4, 0x22abc0, 0x22bfd4,
               0x238c6c, 0x26496c, 0x264d98]
    for ea in targets:
        f = ida_funcs.get_func(ea)
        fstart = f.start_ea if f else idc.BADADDR
        fend = f.end_ea if f else idc.BADADDR
        append("\n=== TARGET %#x func=%#x-%#x name=%s ===" % (
            ea, fstart, fend,
            ida_funcs.get_func_name(fstart) if f else "(none)"))
        if f:
            append(safe_decompile(fstart)[:40000])
        refs = list(idautils.XrefsTo(ea, 0))
        append("xrefs=%d" % len(refs))
        for xr in refs[:100]:
            rf = ida_funcs.get_func(xr.frm)
            append("  ref %#x type=%d func=%#x name=%s" % (
                xr.frm, xr.type, rf.start_ea if rf else idc.BADADDR,
                ida_funcs.get_func_name(rf.start_ea) if rf else "(none)"))

    append("\ndone")


if __name__ == "__main__":
    main()
