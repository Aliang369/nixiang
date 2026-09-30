#!/usr/bin/env python3
# ============================================================
#  lua_struct_check.py — xLua 内嵌 Lua chunk 结构体检（非完整语法检查）
#
#  来源：2026-09-30 伊莫 1.0.1 会话复盘（P0-1 附带工具）
#  用途：游戏模块里以 R"LUA( ... )LUA" 内嵌的 Lua 补丁/chunk，编译期不受任何
#        检查——写错一个 end，真机表现为「补丁静默不生效」，排查代价极高。
#        本工具做**结构配平**体检，可在装机前抓到绝大多数低级错误。
#
#  用法:
#    python3 04_scripts/_common/lua_struct_check.py <文件.cpp|.h> ...
#    python3 04_scripts/_common/lua_struct_check.py            # 默认扫 jni/Games/*.{cpp,h}
#    python3 04_scripts/_common/lua_struct_check.py --all      # 扫 jni 下全部 .cpp/.h
#
#  检查项（计数法，见下）:
#    · end 数量 == function + if + do（for/while 各带一个 do，其 end 已计入 do；
#      elseif/else 不改变计数）
#    · repeat == until；圆括号/花括号配平；字符串与长字符串闭合
#  ★ 注意：这是**结构**体检，不是语法检查（不校验语义/字段名）。
#    自检方法：拿一个已知能跑通的 chunk（如伊莫 kYimoPatch）当对照，必须报 OK。
# ============================================================
import re, sys, glob, os
from pathlib import Path

def strip_lua(src):
    out = []; i = 0; n = len(src)
    while i < n:
        if src.startswith('--[[', i):
            j = src.find(']]', i + 4); i = n if j < 0 else j + 2; continue
        if src.startswith('--', i):
            j = src.find('\n', i); i = n if j < 0 else j; continue
        if src.startswith('[[', i):
            j = src.find(']]', i + 2)
            if j < 0: sys.exit("长字符串未闭合")
            i = j + 2; out.append(' "" '); continue
        c = src[i]
        if c in '"\'':
            j = i + 1
            while j < n:
                if src[j] == '\\': j += 2; continue
                if src[j] == c: break
                j += 1
            if j >= n: sys.exit("引号未闭合 @%d" % i)
            i = j + 1; out.append(' "" '); continue
        out.append(c); i += 1
    return ''.join(out)

def extract(path, start='R"LUA(', end=')LUA"'):
    s = Path(path).read_text(errors='replace')
    res = []; i = s.find(start)
    while i >= 0:
        j = s.find(end, i + len(start))
        if j < 0: break
        res.append(s[i + len(start):j]); i = s.find(start, j + len(end))
    return res

def check(name, chunk):
    code = strip_lua(chunk)
    toks = re.findall(r'[A-Za-z_][A-Za-z_0-9]*', code)
    c = lambda k: toks.count(k)
    exp_end = c('function') + c('if') + c('do')
    errs = []
    if exp_end != c('end'):
        errs.append("end 不配平 期望%d 实际%d（function=%d if=%d do=%d）"
                    % (exp_end, c('end'), c('function'), c('if'), c('do')))
    if c('repeat') != c('until'):
        errs.append("repeat/until 不配平 %d/%d" % (c('repeat'), c('until')))
    if code.count('(') != code.count(')'):
        errs.append("圆括号不配平 %+d" % (code.count('(') - code.count(')')))
    if code.count('{') != code.count('}'):
        errs.append("花括号不配平 %+d" % (code.count('{') - code.count('}')))
    print(("  OK   " if not errs else "  FAIL ") + "%s (%d B)" % (name, len(chunk))
          + ("" if not errs else "\n         -> " + "; ".join(errs)))
    return not errs

if __name__ == '__main__':
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    if not args:
        jni = Path(__file__).resolve().parents[2] / 'mod菜单/starry-mod-menu/app/src/main/jni'
        exts = ['**/*.cpp', '**/*.h'] if '--all' in sys.argv else ['Games/*.cpp', 'Games/*.h']
        args = sorted({p for pat in exts for p in glob.glob(str(jni / pat), recursive=True)})
    ok, total = True, 0
    for path in args:
        chunks = extract(path)
        if not chunks:
            continue
        for ch in chunks:
            total += 1
            ok &= check(os.path.basename(path), ch)
    if total == 0:
        print("  没有发现 R\"LUA( 内嵌 chunk")
    print("RESULT:", "ALL OK (%d chunk)" % total if ok else "HAS ERRORS")
    sys.exit(0 if ok else 1)
