#!/usr/bin/env bash
# script-policy: {"purpose": "群侠传：幸存者 TapTap 登录改包法（现行口径单轨：包内 TapSDK 可达 ⇒ 改包）：验签/初始化门闩恒放行 + 验签失败弹窗掐掉 + 登录入口恒 Web + TapSignKit.getSign 回灌官方证书 MD5（真机 1001 必需），产出可替换的 classes_mod dex", "inputs": "VERSION（默认 0.2.3.10）/ SMALI_DIR（默认 03_decompiled/群侠传：幸存者/<VERSION>/smali）/ OUT_DEX（默认 03_decompiled/群侠传：幸存者/<VERSION>/classes_mod_<VERSION>.dex）/ ORIG_APK（默认 02_apks/群侠传：幸存者/群侠传：幸存者_<VERSION>_official_from_device.apk，仅用于校验对照）", "outputs": "classes_mod_<VERSION>.dex（apktool smali 全树重编，6 处登录补丁）+ stdout 的 method_ids/class_defs/体积/差异类清单", "boundary": "只改 6 个登录相关方法体（InitializerServiceImpl.checkInitialize/checkLocalInitialize、GatekeeperRepository.isGatekeeperLoadedSuccessfully/showErrorToast、DefaultTapLoginFragmentProvider.provide、TapSignKit.getSign）；不改资源、不改其它类、不碰 SignSpoof 与 com/starry/** 框架、不装机。TapSignKit.getSign 只做「返回值回灌官方 cert.der MD5」，不改 com/starry/magic/sign/** 框架类", "prior_art": "04_scripts/不一样西游/patch_taptap_login_byyxy.sh（改包法骨架与自检风格）；skill/re-analyze/login/taptap-web-login.md 改包法四步；03_decompiled/群侠传：幸存者/classes_mod.dex（0.2.3.9 旧版改法复刻依据）", "difference": "锚点按 4.11.0 现取；门闩恒 true 是旧版没有的新增项（旧版靠回灌签名让服务端放行）；真机 16:31 实测 init_failed(code=1001=TapInitErrorCode.CONFIG_ERROR 0x3e9, msg=包名、签名错误) 证明 Gatekeeper 侧仍需官方签名 ⇒ 补回 TapSignKit.getSign 回灌（与 0.2.3.9 同法）；新增「零改动对照重编」差异门禁，把 smali 全树重编的结构性噪声与本次改动分离", "evidence": "10_docs/群侠传：幸存者/_state/0.2.3.10_登录改包.md"}
# ============================================================================
# 群侠传：幸存者 — TapTap 去签名校验 + 网页登录（改包法，单轨）
#
# 为什么是改包法：包内 Lcom/taptap/sdk/** 未加密可直接搜到（4.11.0，1853 个 smali）
# ⇒ 按 skill/re-analyze/login/taptap-web-login.md 顶部「现行口径」走静态改 dex，
#   不走运行时 SPI，不复活 SignSpoof。
#
# 改签后真正会挡登录的是 Gatekeeper 门闩（4.11.0 现取证据）：
#   GatekeeperRepository$initialize$1 里 ApiErr.INVALID_CLIENT ⇒
#     showErrorToast(...) + _initStateFlow.tryEmit(TapInitStateFailed(0x3e9, msg))
#   其余异常走 commitDegradedSuccess() ⇒ 反而是 Success（只有签名/包名不匹配才致命）
#   ⇒ InitializerServiceImpl.checkInitialize/checkLocalInitialize 恒 false ⇒
#     TapTapLogin.loginWithScopes 直接回调错误，连登录界面都进不去。
# 旧版（0.2.3.9）用 TapSignKit.getSign 回灌官方证书 MD5 让服务端校验通过。
# 2026-11-15 真机实测（pid 24448, 16:31）：只做门闩恒放行仍然 init_failed(code=1001,
# msg=包名、签名错误) —— 该 1001 = TapInitErrorCode.CONFIG_ERROR(0x3e9)，唯一产生点是
# GatekeeperRepository 把 GatekeeperApiService 的 INVALID_CLIENT 映射成 TapInitStateFailed。
# 而 GateKeeperRequest 的 signature 字段就是 TapSignKit.getSign(context)，故本脚本同时做：
#   门闩恒放行 + 弹窗掐掉 + 入口恒 Web + 签名回灌（getSign 返回官方 cert.der 的 MD5）。
#
# 6 个补丁点（锚点每次现取，缺失即 die）：
#   A1 com/taptap/sdk/initializer/service/InitializerServiceImpl.smali
#        .method public checkInitialize(Z)Z            -> const/4 p1, 0x1; return p1
#   A2 同文件 .method public checkLocalInitialize(Z)Z   -> const/4 p1, 0x1; return p1
#   B  com/taptap/sdk/initializer/repository/GatekeeperRepository.smali
#        .method public final isGatekeeperLoadedSuccessfully()Z -> const/4 p0, 0x1; return p0
#        .method private final showErrorToast(TapNetworkException)V -> return-void
#   C  com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali
#        provide() 内 "aget v2, v3, v2" 之后插 const/4 v2, 0x3（WhenMappings 里 3=WEB）
#   D  com/taptap/sdk/kit/internal/utils/TapSignKit.smali
#        getSign(Context)String 整体 → const-string "6b013f42541e222db7f2153a138ab301"; return-object v0
#        （= 官方 cert.der 的 MD5。真机实测：门闩恒放行不足以消掉 init_failed(1001)，
#         因为 GateKeeperRequest 把 getSign 的结果当签名上报，服务端比对不上就回 1001）
#
# 用法：
#   bash 04_scripts/群侠传：幸存者/patch_taptap_login_qxzxcz.sh
#   VERSION=0.2.3.10 SMALI_DIR=... OUT_DEX=... bash ...
#   SKIP_CONTROL=1 ...      # 跳过零改动对照重编（省一次 63MB 拷贝 + 6s 汇编）
#   KEEP_WORK=1 ...         # 保留临时目录（排错用）
#
# 幂等：输入不变时重复执行产出字节相同（汇编固定 -j 1，dexlib2 多线程会乱序）。
# ============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

GAME="群侠传：幸存者"
VERSION="${VERSION:-0.2.3.10}"
SMALI_DIR="${SMALI_DIR:-$NIXIANG/03_decompiled/$GAME/$VERSION/smali}"
OUT_DEX="${OUT_DEX:-$NIXIANG/03_decompiled/$GAME/$VERSION/classes_mod_${VERSION}.dex}"
ORIG_APK="${ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_official_from_device.apk}"
ORIG_APK_MD5="7677c9c2fd301b06cc3ab245bb0cced0"
API_LEVEL="${API_LEVEL:-25}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
SKIP_CONTROL="${SKIP_CONTROL:-0}"
KEEP_WORK="${KEEP_WORK:-0}"

REL_SVC="com/taptap/sdk/initializer/service/InitializerServiceImpl.smali"
REL_GK="com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
REL_PROV="com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali"
REL_SIGN="com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
OFFICIAL_CERT_MD5="6b013f42541e222db7f2153a138ab301"   # 03_decompiled/群侠传：幸存者/cert/cert.der 的 md5

die() { echo "FAIL: $*" >&2; exit 1; }

[[ -d "$SMALI_DIR" ]] || die "smali 目录不存在：$SMALI_DIR"
SMALI_DIR="$(cd "$SMALI_DIR" && pwd -P)"   # 0.2.3.10/smali 是指向 apktool_0.2.3.10 的软链，cp -R 不解引用 ⇒ 必须取物理路径
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || die "缺少 01_tools/smali/{smali,baksmali}"
for r in "$REL_SVC" "$REL_GK" "$REL_PROV" "$REL_SIGN"; do
  [[ -f "$SMALI_DIR/$r" ]] || die "锚点类缺失（SDK 版本变了？重取锚点）：$r"
done

# ---- 锚点断言（缺失即打印上下文并退出）------------------------------------
assert_anchor() { # $1=文件(相对) $2=固定字符串 $3=期望出现次数
  local f="$SMALI_DIR/$1" s="$2" want="$3" got
  got="$(grep -F -c -- "$s" "$f" 2>/dev/null || true)"
  if [ "${got:-0}" != "$want" ]; then
    echo "FAIL: 锚点不符 [$1] want=$want got=${got:-0}" >&2
    echo "锚点原文: $s" >&2
    echo "---- 上下文 ----" >&2
    grep -F -n -m 1 -- "$s" "$f" >&2 || true
    exit 1
  fi
}
assert_anchor "$REL_SVC"  ".method public checkInitialize(Z)Z" 1
assert_anchor "$REL_SVC"  ".method public checkLocalInitialize(Z)Z" 1
assert_anchor "$REL_GK"   ".method public final isGatekeeperLoadedSuccessfully()Z" 1
assert_anchor "$REL_GK"   ".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V" 1
assert_anchor "$REL_PROV" "    aget v2, v3, v2" 1
assert_anchor "$REL_SIGN" ".method public final getSign(Landroid/content/Context;)Ljava/lang/String;" 1
assert_anchor "$REL_SIGN" "invoke-virtual {v1, p1, v2}, Landroid/content/pm/PackageManager;->getPackageInfo(Ljava/lang/String;I)Landroid/content/pm/PackageInfo;" 1

# ---- 工作区 ---------------------------------------------------------------
WORK="$(mktemp -d /tmp/qxzxcz_loginpatch.XXXXXX)"
cleanup() { if [ "$KEEP_WORK" = "1" ]; then echo "保留工作区：$WORK"; else rm -rf "$WORK"; fi; }
trap cleanup EXIT

echo "[1/5] 拷贝 smali 树（$(du -sm "$SMALI_DIR" | cut -f1) MB）"
mkdir -p "$WORK"
cp -R "$SMALI_DIR" "$WORK/smali"

echo "[2/5] 打补丁（6 处）"
python3 - "$WORK/smali" "$REL_SVC" "$REL_GK" "$REL_PROV" "$REL_SIGN" "$OFFICIAL_CERT_MD5" <<'PYEOF'
import re, sys

root, rel_svc, rel_gk, rel_prov, rel_sign, cert_md5 = sys.argv[1:7]

def read(p):
    with open(p, encoding='utf-8') as f:
        return f.read().split('\n')

def write(p, lines):
    with open(p, 'w', encoding='utf-8') as f:
        f.write('\n'.join(lines))

def locate(lines, decl):
    for i, l in enumerate(lines):
        if l.strip() == decl:
            k = i
            while k < len(lines) and not re.match(r'\s*\.(locals|registers)\s', lines[k]):
                k += 1
            if k >= len(lines):
                raise SystemExit('FAIL: %s 缺 .locals/.registers' % decl)
            return i, k
    raise SystemExit('FAIL: 找不到方法声明 %s' % decl)

def body_span(lines, decl):
    i, k = locate(lines, decl)
    m = k + 1
    while m < len(lines):
        t = lines[m].strip()
        if t.startswith('.annotation') or t.startswith('.param'):
            closed = '.end annotation' if t.startswith('.annotation') else '.end param'
            while m < len(lines) and lines[m].strip() != closed:
                m += 1
            m += 1
            continue
        if t == '':
            m += 1
            continue
        break
    e = m
    while e < len(lines) and lines[e].strip() != '.end method':
        e += 1
    if e >= len(lines):
        raise SystemExit('FAIL: %s 缺 .end method' % decl)
    return i, k, m, e

def norm(seq):
    return [s.strip() for s in seq if s.strip() and not s.strip().startswith('#')]

def patch_method(path, decl, body, tag):
    lines = read(path)
    i, k, m, e = body_span(lines, decl)
    if norm(lines[m:e]) == norm(body):
        print('  = %-52s 已是目标形态，跳过' % tag)
        return False
    new = lines[:m] + [''] + list(body) + lines[e:]
    write(path, new)
    print('  * %-52s 已替换（原体 %d 行 -> %d 行）' % (tag, e - m, len(body)))
    return True

svc = root + '/' + rel_svc
gk  = root + '/' + rel_gk
prov = root + '/' + rel_prov
sign = root + '/' + rel_sign

# A1/A2：门闩恒放行（4.11.0：checkInitialize/checkLocalInitialize 只有 TapInitStateSuccess 才 true）
latch = ['    const/4 p1, 0x1', '', '    return p1']
patch_method(svc, '.method public checkInitialize(Z)Z', latch, 'A1 InitializerServiceImpl.checkInitialize')
patch_method(svc, '.method public checkLocalInitialize(Z)Z', latch, 'A2 InitializerServiceImpl.checkLocalInitialize')

# B1：旧入口（@Deprecated，兼容/日志路径）恒 true
patch_method(gk, '.method public final isGatekeeperLoadedSuccessfully()Z',
             ['    const/4 p0, 0x1', '', '    return p0'],
             'B1 GatekeeperRepository.isGatekeeperLoadedSuccessfully')
# B2：验签失败弹窗 + 错误日志全部掐掉
patch_method(gk,
             '.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V',
             ['    return-void'],
             'B2 GatekeeperRepository.showErrorToast')

# D：签名回灌（真机 1001 修复）：GateKeeperRequest(platform, packageName, signature) 的 signature
#    来自 TapSignKit.getSign，服务端比对不上就回「包名、签名错误」⇒ TapInitStateFailed(0x3e9=1001)。
#    改签包里真签名必然不匹配，故返回官方 cert.der 的 MD5（与 0.2.3.9 旧版同法）。
sign_body = ['    const-string v0, "%s"' % cert_md5, '', '    return-object v0']
patch_method(sign, '.method public final getSign(Landroid/content/Context;)Ljava/lang/String;',
             sign_body,
             'D TapSignKit.getSign（回灌官方 cert MD5）')

# C：登录入口恒 Web（provide 内 switch 的 WhenMappings 下标：1=CLIENT 2=CLOUD_GAME 3=WEB）
lines = read(prov)
decl = [l for l in lines if l.startswith('.method') and ' provide(' in l][0].strip()
i, k = locate(lines, decl)
e = i
while e < len(lines) and lines[e].strip() != '.end method':
    e += 1
hit = None
for j in range(i, e):
    if lines[j].strip() == 'aget v2, v3, v2':
        hit = j
        break
if hit is None:
    raise SystemExit('FAIL: provide 内找不到 aget v2, v3, v2')
if 'const/4 v2, 0x3' in lines[hit + 1]:
    print('  = %-52s 已插入 const/4 v2, 0x3，跳过' % 'C DefaultTapLoginFragmentProvider.provide')
else:
    lines.insert(hit + 1, '')
    lines.insert(hit + 2, '    const/4 v2, 0x3')
    write(prov, lines)
    print('  * %-52s 已在 aget 后插入 const/4 v2, 0x3' % 'C DefaultTapLoginFragmentProvider.provide')
PYEOF

echo "[3/5] 汇编（--api $API_LEVEL -j 1，保证幂等字节一致）"
mkdir -p "$(dirname "$OUT_DEX")"
"$SMALI_BIN" assemble --api "$API_LEVEL" -j 1 -o "$OUT_DEX" "$WORK/smali"

echo "[4/5] 结构校验（method_ids / class_defs / 无 SignSpoof / dexdump 可解析）"
[[ -f "$ORIG_APK" ]] || die "缺少对照底包（仅用于校验）：$ORIG_APK"
ACT_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACT_MD5" == "$ORIG_APK_MD5" ]] || die "底包指纹不符：${ACT_MD5}（期望 ${ORIG_APK_MD5}）"
unzip -p "$ORIG_APK" classes.dex > "$WORK/classes.dex"

python3 - "$WORK/classes.dex" "$OUT_DEX" <<'PYEOF'
import struct, sys
o, m = sys.argv[1:3]
def hdr(p):
    d = open(p, 'rb').read()
    g = lambda off: struct.unpack_from('<I', d, off)[0]
    return {'file_size': g(0x20), 'string_ids': g(0x38), 'type_ids': g(0x40),
            'proto_ids': g(0x48), 'field_ids': g(0x50), 'method_ids': g(0x58),
            'class_defs': g(0x60)}, d[:8]
a, ma = hdr(o)
b, mb = hdr(m)
print('  | 项 | 原 dex | 产物 | 差 |')
print('  |---|---|---|---|')
for k in a:
    print('  | %s | %d | %d | %+d |' % (k, a[k], b[k], b[k] - a[k]))
print('  magic: %r -> %r' % (ma, mb))
assert b['method_ids'] <= 65536, 'method_ids 超 65536'
assert b['class_defs'] == a['class_defs'], 'class_defs 数量变化'
assert b['method_ids'] - a['method_ids'] <= 0, 'method_ids 增长'
PYEOF

if grep -a -q 'com/starry/magic/sign/SignSpoof' "$OUT_DEX"; then
  die "产物 dex 出现 SignSpoof（该框架类已于 2026-10-28 删除，禁止复活）"
fi
if ! grep -a -q "${OFFICIAL_CERT_MD5}" "$OUT_DEX"; then
  die "产物 dex 内找不到官方证书 MD5（TapSignKit.getSign 回灌未生效）"
fi
echo "  · TapSignKit.getSign 已回灌官方 cert MD5（${OFFICIAL_CERT_MD5}）"
DD="${ANDROID_BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}/dexdump"
if [[ -x "$DD" ]]; then
  "$DD" -f "$OUT_DEX" > "$WORK/dexdump.txt" 2>"$WORK/dexdump.err" || die "dexdump 解析失败（见 $WORK/dexdump.err）"
  echo "  · dexdump OK：$(grep -m1 -o "DEX version '[^']*'" "$WORK/dexdump.txt")，method_ids=$(grep -m1 'method_ids_size' "$WORK/dexdump.txt" | tr -dc 0-9)"
else
  echo "  ! 未找到 dexdump（${DD}），跳过该步"
fi

echo "[5/5] 差异校验（baksmali 级：改动必须只落在 4 个锚点类）"
"$BAKSMALI_BIN" d -o "$WORK/v_orig" "$WORK/classes.dex"
"$BAKSMALI_BIN" d -o "$WORK/v_mod"  "$OUT_DEX"
diff -rq "$WORK/v_orig" "$WORK/v_mod" 2>/dev/null | sed "s|$WORK/v_orig/||; s| and .*||; s|^Files ||" | sort > "$WORK/diff_vs_orig.txt" || true
echo "  原 dex 与产物的差异文件（含 smali 全树重编的结构性噪声）："
sed 's/^/    /' "$WORK/diff_vs_orig.txt"

if [ "$SKIP_CONTROL" != "1" ]; then
  cp -R "$SMALI_DIR" "$WORK/smali_ctl"
  "$SMALI_BIN" assemble --api "$API_LEVEL" -j 1 -o "$WORK/control.dex" "$WORK/smali_ctl"
  "$BAKSMALI_BIN" d -o "$WORK/v_ctl" "$WORK/control.dex"
  diff -rq "$WORK/v_ctl" "$WORK/v_mod" 2>/dev/null | sed "s|$WORK/v_ctl/||; s| and .*||; s|^Files ||" | sort > "$WORK/diff_vs_ctl.txt" || true
  echo "  零改动对照重编与产物的差异文件（= 本次真实改动）："
  sed 's/^/    /' "$WORK/diff_vs_ctl.txt"
  printf '%s\n' "$REL_SVC" "$REL_GK" "$REL_PROV" "$REL_SIGN" | sort > "$WORK/expect.txt"
  if ! diff -q "$WORK/expect.txt" "$WORK/diff_vs_ctl.txt" >/dev/null 2>&1; then
    die "改动范围越界：期望仅 4 个锚点类，实际见上表"
  fi
  echo "  ✓ 改动范围 = 4 个锚点类（无越界）"
fi

echo
echo "OK  $OUT_DEX"
echo "    体积 $(stat -f%z "$OUT_DEX") B  md5 $(md5 -q "$OUT_DEX")"
