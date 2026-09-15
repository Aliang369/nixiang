#!/usr/bin/env python3
from pathlib import Path

ROOT = Path('/Users/aliang/Documents/nixiang')
JS = ROOT / 'mod菜单/starry-mod-menu/app/src/main/jni/Games/HalyPatch.js'
HEADER = ROOT / 'mod菜单/starry-mod-menu/app/src/main/jni/Games/HalyPatchJs.h'

text = JS.read_text(encoding='utf-8')
if ')JS"' in text:
    raise SystemExit('HalyPatch.js contains the C++ raw-string terminator')
header = '#pragma once\n\nstatic const char* kHalyInstallJs = R"JS(\n' + text + '\n)JS";\n'
HEADER.write_text(header, encoding='utf-8')
print(f'generated {HEADER} ({len(text)} bytes JS)')
