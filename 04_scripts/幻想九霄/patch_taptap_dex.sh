#!/bin/bash
# TapTap 改包法 · 静态补丁器（幻想九霄 1.0.212 / TapSDK 4.10.1 / classes2.dex）
# 用法: patch_taptap_dex.sh <底包.apk> <输出 classes2.dex>
# 4 处补丁：门闩恒 true / showErrorToast->return-void / 登录选择点恒 Web / getSign 回灌官方证书 MD5
# 校验：① 控制组（未打补丁的同源 smali 重新组装）与补丁组回编译后 diff，只允许 4 个类不同；
#       ② 与原始反编译树的其余差异只允许「静态字段默认初值行」；③ 逐条断言 4 处指令；④ dex 头/方法数不越界。
# 失败即 exit 非 0。
#
# 非确定性说明（实测）：dexlib2(smali 2.5.2) 组装同一份 smali 目录，两次产出的 dex 字节不同
#   （差异分布在各 section 的条目顺序，各 section 大小完全相同；两次产物重新 baksmali 后
#    diff -rq 完全相同 ⇒ 语义等价）。因此 sha256 不能当复现判据，复现判据是本脚本的断言全过。
set -euo pipefail

if [ $# -ne 2 ]; then
  echo "用法: $0 <底包.apk> <输出 classes2.dex>" >&2
  exit 2
fi
APK="$1"
OUT="$2"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
WORK="$ROOT/03_decompiled/幻想九霄/taptap"
BAKSMALI="$ROOT/01_tools/smali/baksmali"
SMALI="$ROOT/01_tools/smali/smali"
APILVL=24                      # classes2.dex magic = dex.037
CERT_MD5="e2f5dfec1023f11a8f8247bfc84c245b"

D_IMPL="com/taptap/sdk/initializer/service/InitializerServiceImpl.smali"
D_GATE="com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
D_PROV="com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali"
D_SIGN="com/taptap/sdk/kit/internal/utils/TapSignKit.smali"

[ -f "$APK" ] || { echo "FAIL: 底包不存在 $APK" >&2; exit 1; }

echo "== [1/7] 解出 classes2.dex"
mkdir -p "$WORK/dex_orig" "$WORK/dex_out" "$WORK/log"
rm -rf "$WORK/smali" "$WORK/smali_patched" "$WORK/smali_check" "$WORK/smali_control"
unzip -o -q "$APK" 'classes2.dex' -d "$WORK/dex_orig"
[ -f "$WORK/dex_orig/classes2.dex" ] || { echo "FAIL: 底包内无 classes2.dex" >&2; exit 1; }
ORIG="$WORK/dex_orig/classes2.dex"

echo "== [2/7] baksmali 反编译 -> $WORK/smali"
"$BAKSMALI" d -a $APILVL "$ORIG" -o "$WORK/smali"
cp -R "$WORK/smali" "$WORK/smali_patched"

echo "== [3/7] 打 4 处补丁"
python3 - "$WORK/smali_patched" "$CERT_MD5" "$D_IMPL" "$D_GATE" "$D_PROV" "$D_SIGN" <<'PY'
import sys, os
root, cert, d_impl, d_gate, d_prov, d_sign = sys.argv[1:7]

def rd(p):
    with open(os.path.join(root,p),'rb') as f: return f.read()

def wr(p,b):
    with open(os.path.join(root,p),'wb') as f: f.write(b)

def replace_method(buf, header, new_body):
    """把 header 起始的整个方法（到同层的 .end method）替换成 new_body"""
    h = header.encode()
    assert buf.count(h) == 1, "method header not unique: %r (count=%d)" % (header, buf.count(h))
    i = buf.index(h)
    j = buf.index(b'\n.end method', i) + len(b'\n.end method')
    return buf[:i] + new_body.encode() + buf[j:]

# 补丁 1: 门闩恒 true（保留原 .registers 6）
p1 = rd(d_impl)
p1 = replace_method(p1,
    '.method public checkInitialize(Z)Z\n    .registers 6',
    '''.method public checkInitialize(Z)Z
    .registers 6

    const/4 v0, 0x1

    return v0
.end method''')
wr(d_impl, p1)

# 补丁 2: 验签失败弹窗 return-void（保留原 .registers 5）
p2 = rd(d_gate)
p2 = replace_method(p2,
    '.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V\n    .registers 5',
    '''.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V
    .registers 5

    return-void
.end method''')
wr(d_gate, p2)

# 补丁 3: 登录入口恒 Web（aget v2,v3,v2 之后把 switch 值钉成 3 == LoginTypes.WEB）
p3 = rd(d_prov)
anchor = b'    aget v2, v3, v2\n'
assert p3.count(anchor) == 1, "login switch anchor not unique: count=%d" % p3.count(anchor)
p3 = p3.replace(anchor, anchor + b'\n    const/4 v2, 0x3\n')
assert b'const/4 v2, 0x3' in p3
wr(d_prov, p3)

# 补丁 4: 签名回灌（保留原 .registers 5，实例方法签名不变）
p4 = rd(d_sign)
p4 = replace_method(p4,
    '.method public final getSign(Landroid/content/Context;)Ljava/lang/String;\n    .registers 5',
    '''.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 5

    const-string v0, "%s"

    return-object v0
.end method''' % cert)
wr(d_sign, p4)

print("  patched: " + ", ".join([d_impl, d_gate, d_prov, d_sign]))
PY

echo "== [4/7] smali 重新组装（补丁组 + 控制组）"
"$SMALI" a -a $APILVL "$WORK/smali_patched" -o "$WORK/dex_out/classes2.dex"
"$SMALI" a -a $APILVL "$WORK/smali"         -o "$WORK/dex_out/classes2_control.dex"
PATCHED="$WORK/dex_out/classes2.dex"
CONTROL="$WORK/dex_out/classes2_control.dex"
[ -f "$PATCHED" ] || { echo "FAIL: 未生成 patched dex" >&2; exit 1; }
[ -f "$CONTROL" ] || { echo "FAIL: 未生成 control dex" >&2; exit 1; }

echo "== [5/7] 回编译校验（重新 baksmali 后逐字节 diff）"
"$BAKSMALI" d -a $APILVL "$PATCHED" -o "$WORK/smali_check"
"$BAKSMALI" d -a $APILVL "$CONTROL"  -o "$WORK/smali_control"

# 5a 控制组 vs 补丁组：只允许 4 个目标类不同
diff -rq "$WORK/smali_control" "$WORK/smali_check" > "$WORK/log/diff_control.txt" 2>&1 || true
python3 - "$WORK/log/diff_control.txt" "$D_IMPL" "$D_GATE" "$D_PROV" "$D_SIGN" "$WORK/smali_control" <<'PY'
import sys
logp = sys.argv[1]
allowed = set(sys.argv[2:6]); base = sys.argv[6]
lines = [l.strip() for l in open(logp, encoding='utf-8').read().splitlines() if l.strip()]
diffs = set()
for l in lines:
    if l.startswith('Files ') and l.endswith(' differ'):
        rel = l[len('Files '):-len(' differ')]
        a, b = rel.split(' and ')
        if not a.startswith(base + '/'): sys.exit('FAIL: 路径不在工作区: ' + l)
        diffs.add(a[len(base)+1:])
    elif l.startswith('Only in'):
        sys.exit('FAIL: 出现结构差异（类增删）: ' + l)
    else:
        sys.exit('FAIL: 其他 diff 输出: ' + l)
missing = allowed - diffs; extra = diffs - allowed
if missing: sys.exit('FAIL: 预期改动的类未发生变化: ' + ', '.join(sorted(missing)))
if extra:   sys.exit('FAIL: 控制组对比出现计划外改动: ' + ', '.join(sorted(extra)))
print('  [控制组] 差异类 %d 个，与允许清单完全一致' % len(diffs))
for d in sorted(diffs): print('    - ' + d)
PY

# 5b 原始反编译树 vs 补丁组：非目标类的差异只允许「静态字段默认初值被 round-trip 丢弃」
diff -rq "$WORK/smali" "$WORK/smali_check" > "$WORK/log/diff_raw.txt" 2>&1 || true
python3 - "$WORK/log/diff_raw.txt" "$WORK/smali" "$WORK/smali_check" "$D_IMPL" "$D_GATE" "$D_PROV" "$D_SIGN" <<'PY'
import sys, difflib
logp, base_o, base_c = sys.argv[1:4]
allowed = set(sys.argv[4:8])
lines = [l.strip() for l in open(logp, encoding='utf-8').read().splitlines() if l.strip()]
other = []
for l in lines:
    if l.startswith('Files ') and l.endswith(' differ'):
        rel = l[len('Files '):-len(' differ')]
        a, b = rel.split(' and ')
        if not a.startswith(base_o + '/'): sys.exit('FAIL: 路径不在工作区: ' + l)
        relp = a[len(base_o)+1:]
        if relp in allowed: continue
        other.append(relp)
    elif l.startswith('Only in'):
        sys.exit('FAIL: 出现结构差异（类增删）: ' + l)
    else:
        sys.exit('FAIL: 其他 diff 输出: ' + l)

def is_default_field_drop(old, new):
    # 只允许: ".field ... :T = <默认初值>" -> ".field ... :T"
    if not old.startswith('.field ') or ' = ' not in old: return False
    if not new.startswith('.field '): return False
    left, val = old.rsplit(' = ', 1)
    if new != left: return False
    t = left.rsplit(':', 1)[-1]
    v = val.strip()
    return (t == 'Z' and v in ('false','true')) or (t in ('B','S','I','J') and (v.endswith('L') or v.endswith('t') or v.endswith('s'))) or v == 'null'

for relp in other:
    # 空行忽略：round-trip 会重排空行，只比较有内容的行
    o = [x for x in open(base_o + '/' + relp, encoding='utf-8').read().splitlines() if x.strip()]
    n = [x for x in open(base_c + '/' + relp, encoding='utf-8').read().splitlines() if x.strip()]
    sm = difflib.SequenceMatcher(None, o, n)
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag == 'equal': continue
        if tag != 'replace' or (i2-i1) != (j2-j1):
            sys.exit('FAIL: %s 存在非字段行的结构差异 (%s)' % (relp, tag))
        for k in range(i2-i1):
            if not is_default_field_drop(o[i1+k], n[j1+k]):
                sys.exit('FAIL: %s 出现非默认初值的改动: %r -> %r' % (relp, o[i1+k], n[j1+k]))
if other:
    print('  [原始树] 非目标类差异 %d 个，全部为 smali round-trip 丢弃默认静态初值（语义等价）:' % len(other))
    for d in sorted(other): print('    ~ ' + d)
else:
    print('  [原始树] 非目标类零差异')
PY

echo "== [6/7] 4 处补丁指令断言 + dex 头断言"
python3 - "$WORK/smali_check" "$WORK/smali_control" "$ORIG" "$PATCHED" "$CERT_MD5" "$D_IMPL" "$D_GATE" "$D_PROV" "$D_SIGN" <<'PY'
import sys, os, struct
check, control, orig, patched, cert, d_impl, d_gate, d_prov, d_sign = sys.argv[1:10]

def body(root, rel, header):
    t = open(os.path.join(root, rel), encoding='utf-8').read()
    i = t.index(header)
    j = t.index('\n.end method', i)
    return t[i:j]

def must(cond, msg):
    if not cond: sys.exit('FAIL: ' + msg)

H1 = '.method public checkInitialize(Z)Z'
H2 = '.method private final showErrorToast('
H3 = '.method public provide('
H4 = '.method public final getSign(Landroid/content/Context;)Ljava/lang/String;'

# 控制组里这 4 个方法必须保持原样（证明差异确实来自补丁而非 round-trip）
c1 = body(control, d_impl, H1)
must('invoke-virtual {v0}' in c1, '控制组 checkInitialize 已被意外改动')
c2 = body(control, d_gate, H2)
must('showToast' in c2, '控制组 showErrorToast 已被意外改动')
c3 = body(control, d_prov, H3)
must('const/4 v2, 0x3' not in c3, '控制组 provide 已被意外改动')
c4 = body(control, d_sign, H4)
must('PackageManager' in c4, '控制组 getSign 已被意外改动')

b1 = body(check, d_impl, H1)
must('.registers 6' in b1, 'checkInitialize 未保留 .registers 6')
must('const/4 v0, 0x1' in b1 and 'return v0' in b1, 'checkInitialize 未恒 true')
must('invoke' not in b1, 'checkInitialize 仍含 invoke 调用')

b2 = body(check, d_gate, H2)
must('.registers 5' in b2, 'showErrorToast 未保留 .registers 5')
must(b2.rstrip().endswith('return-void'), 'showErrorToast 未以 return-void 结尾')
must('showToast' not in b2, 'showErrorToast 仍调用 showToast')

b3 = body(check, d_prov, H3)
idx = b3.index('aget v2, v3, v2')
must('const/4 v2, 0x3' in b3[idx:idx+120], '登录选择点未钉 Web(3)')
must('WebLoginFragment$Companion' in b3 and 'ClientLoginFragment$Companion' in b3, 'provide 分支结构被破坏')
must(b3.count('const/4 v2, 0x3') == 1, 'provide 中出现多余的对齐常量')

b4 = body(check, d_sign, H4)
must('.registers 5' in b4, 'getSign 未保留 .registers 5')
must('"%s"' % cert in b4, 'getSign 未写入官方证书 MD5')
must('return-object v0' in b4, 'getSign 未 return-object')
must('PackageManager' not in b4, 'getSign 仍读 PackageManager')

def hdr(p):
    b = open(p,'rb').read()
    return (b, struct.unpack_from('<I', b, 0x20)[0], struct.unpack_from('<I', b, 0x58)[0],
            struct.unpack_from('<I', b, 0x60)[0])

bo, so, mo, co = hdr(orig)
bp, sp, mp, cp = hdr(patched)
must(bo[:7] == b'dex\n037', '原 dex magic 异常')
must(bp[:7] == b'dex\n037', 'patched dex magic != dex.037')
must(mo - 64 <= mp <= mo, '方法引用数异常变动: %d -> %d' % (mo, mp))
must(mp < 65536, '方法引用数越界: %d' % mp)
must(cp == co, '类数变化: %d -> %d' % (co, cp))
must(sp <= int(so * 1.25), 'dex 体积异常膨胀: %d -> %d (+%.1f%%)' % (so, sp, (sp-so)*100.0/so))
must(sp <= 0x40000000, 'dex 体积越界')
print('  类定义 %d（不变）  方法引用 %d -> %d（被删的只是死引用，仍 <65536）  体积 %d -> %d B（Δ %+d）' % (co, mo, mp, so, sp, sp-so))
PY

echo "== [7/7] 输出"
mkdir -p "$(dirname "$OUT")"
cp "$PATCHED" "$OUT"
echo "  patched dex: $OUT"
if command -v sha256sum >/dev/null 2>&1; then sha256sum "$OUT"; else shasum -a 256 "$OUT"; fi
echo "ALL ASSERTIONS PASSED"
