#!/usr/bin/env python3
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "_common"))
from gen_util import write_if_changed  # noqa: E402

ROOT = Path('/Users/aliang/Documents/nixiang')
JS = ROOT / 'mod菜单/starry-mod-menu/app/src/main/jni/Games/HalyPatch.js'
HEADER = ROOT / 'mod菜单/starry-mod-menu/app/src/main/jni/Games/HalyPatchJs.h'

text = JS.read_text(encoding='utf-8')
if ')JS"' in text:
    raise SystemExit('HalyPatch.js contains the C++ raw-string terminator')
header = '#pragma once\n\nstatic const char* kHalyInstallJs = R"JS(\n' + text + '\n)JS";\n'
# 幂等写（见 _common/gen_util.py）：内容未变不落盘，避免刷新 mtime 触发无谓重建
write_if_changed(HEADER, header)
