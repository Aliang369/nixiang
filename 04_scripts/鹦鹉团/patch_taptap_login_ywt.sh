#!/usr/bin/env bash
# script-policy: {"purpose": "鹦鹉团 1.0.7 TapTap 去签名校验 + 登录改网页登录：按 smali 行级锚点打 7 处补丁（P1 门闩恒放行 / P2 弹窗掐掉 / P3 登录入口恒 Web / P4 TapSignKit.getSign 回灌官方证书 MD5 / P5a QGSdkUtils.getApkSign / P5b quicksdk k.a(Context) / P6 Check.getSignInfo 由 native 改 Java 常量实现），默认只改工作副本并打印 diff，不汇编不打包不签名不装机", "inputs": "SMALI_ROOT（默认 03_decompiled/鹦鹉团/apktool_1.0.7）；--dry-run 只打印 diff；BUILD_DEX=1 才在末尾汇编受影响 dex（默认关）", "outputs": "WORK 目录内 3 个 smali 子树副本的改动 + stdout 的 7 处 before/after 片段与自检结论；BUILD_DEX=1 时额外产出 classes.dex / classes7.dex / classes11.dex", "boundary": "只改 7 个方法（GatekeeperRepository.isGatekeeperLoadedSuccessfully / showErrorToast、DefaultTapLoginFragmentProvider.provide、TapSignKit.getSign、QGSdkUtils.getApkSign、com.quicksdk.utility.k.a(Context)、com.game.apkverify.Check.getSignInfo）；不改资源、不改 other 类、不碰 SignSpoof、不复活运行时签名伪装、不做 zipalign/apksigner/adb", "prior_art": "04_scripts/群侠传：幸存者/patch_taptap_login_qxzxcz.sh（骨架与自检风格）；skill/re-analyze/login/taptap-web-login.md 改包法四步；10_docs/鹦鹉团/鹦鹉团_登录改包方案.md；10_docs/鹦鹉团/鹦鹉团_反篡改校验排查_20261119.md", "difference": "多 dex（锚点跨 classes11 / classes7 / classes 三个 dex，不是单一 smali 树）；本包 InitializerServiceImpl 无 checkLocalInitialize，故门闩只改 GatekeeperRepository.isGatekeeperLoadedSuccessfully（不照抄群侠传的 A1/A2）；新增 P5a/P5b 两处 QGSDK 签名值回灌与新增大写/小写双格式（TapTap 小写、QGSDK 大写）；新增 P6 把 native 的 Check.getSignInfo 改成 Java 常量实现（一次覆盖 subject/issuer/md5/sha1，兜住 net/a:6107 的 sign_sha1 直取路径）；新增 --dry-run 与幂等跳过；默认不汇编", "evidence": "10_docs/鹦鹉团/鹦鹉团_登录改包方案.md §2；10_docs/鹦鹉团/鹦鹉团_登录改包脚本_dryrun_20261119.md"}

# ============================================================================
# 鹦鹉团 1.0.7 - TapTap 去签名校验 + 登录改网页登录（改包法 A 轨）
#
# 为什么是 A 轨：包内 Lcom/taptap/sdk/** 完全可达（classes11 1788 个 smali、classes2 1142 个），
# 无壳（13 dex 全明文）⇒ 按 skill/re-analyze/login/taptap-web-login.md 顶部现行口径走静态改 dex，
# 不走运行时 SPI，不复活 SignSpoof。
#
# 7 处补丁（锚点全部按本包 1.0.7 现取，禁止跨版本抄行号）：
#   P1 smali_classes11 com/taptap/sdk/initializer/repository/GatekeeperRepository.smali
#        isGatekeeperLoadedSuccessfully()Z -> const/4 v0,0x1; return v0
#        （链路：TapTapLogin.loginWithScopes -> TapSdkChecker.checkInitialize(默认 true)
#          -> InitializerServiceImpl.checkInitialize -> 判 isGatekeeperLoadedSuccessfully；
#          false 时 loginWithScopes 直接 return-void，登录界面都不弹）
#   P2 同文件 showErrorToast(TapNetworkException)V -> 方法体首 return-void（保留原 .locals 3）
#   P3 smali_classes11 .../login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali
#        provide() 内 "aget v2, v3, v2" 之后插 const/4 v2,0x3（$WhenMappings: 1=CLIENT 2=CLOUD_GAME 3=WEB）
#   P4 smali_classes11 com/taptap/sdk/kit/internal/utils/TapSignKit.smali
#        getSign(Context)String 整段替换为 const-string 官方 cert.der 的 MD5（真机 1001 必需）
#   P5a smali_classes7 com/quickgamesdk/utils/QGSdkUtils.smali
#        getApkSign(Context)String -> 同常量（QGParameter 每个请求都带 signMd5）
#   P5b smali com/quicksdk/utility/k.smali
#        a(Context)String -> 同常量（net/a:3319 与 utility/b:268 的 sign_md5）
#
# 用法：
#   bash 04_scripts/鹦鹉团/patch_taptap_login_ywt.sh                # 打补丁到工作副本并自检（不汇编）
#   bash 04_scripts/鹦鹉团/patch_taptap_login_ywt.sh --dry-run      # 只打印 diff，不写任何文件
#   BUILD_DEX=1 bash 04_scripts/鹦鹉团/patch_taptap_login_ywt.sh    # 额外汇编受影响 dex（仍不打包/不签名）
#   KEEP_WORK=1 ...                                                # 保留工作副本
#
# 幂等：已改过的文件再次运行会被识别并跳过（不会二次插入）。
# 边界：不跑 apktool b / zipalign / apksigner / adb，出包与装机由管线统一执行。
# ============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

GAME="鹦鹉团"
VERSION="1.0.7"
SMALI_ROOT="${SMALI_ROOT:-$NIXIANG/03_decompiled/$GAME/apktool_1.0.7}"
# 证书四字段：全部从 META-INF/AIAUTION.RSA 的 DER(720 B) 现取（命令见 10_docs/鹦鹉团/鹦鹉团_登录改包脚本_dryrun_20261119.md §9）
# TapTap 侧：CommonUtils.getHashString 走 Integer.toHexString ⇒ **小写**（与 apksigner 输出一致）
OFFICIAL_CERT_MD5="27c2d8ca4e5703d9ad2a1ee39d2a859d"
# QGSDK 侧：native get_digest 用 .rodata 的 "0123456789ABCDEF" 表手工转十六进制 ⇒ **大写**
CERT_MD5_UPPER="27C2D8CA4E5703D9AD2A1EE39D2A859D"
CERT_SHA1_UPPER="35A8E7B1B9FFE25F3413BE2955C9A06B022992C9"
# X509Certificate.getSubjectDN()/getIssuerDN().toString() 的 RFC2253 形态（openssl -nameopt RFC2253 同形）
CERT_SUBJECT="C=CN,L=SH,CN=Meta48"
CERT_ISSUER="C=CN,L=SH,CN=Meta48"
SMALI_BIN="${SMALI_BIN:-$NIXIANG/01_tools/smali/smali}"
API_LEVEL="${API_LEVEL:-25}"
BUILD_DEX="${BUILD_DEX:-0}"
KEEP_WORK="${KEEP_WORK:-0}"
DRY_RUN=0

for a in "$@"; do
  case "$a" in
    --dry-run) DRY_RUN=1 ;;
    -h|--help) sed -n "2,45p" "$0"; exit 0 ;;
    *) echo "FAIL: 未知参数 $a" >&2; exit 2 ;;
  esac
done

# dex 目录 -> 锚点类相对路径。三组必须都在，缺一个就说明版本/布局变了。
DEX_A="smali_classes11"
DEX_B="smali_classes7"
DEX_C="smali"
# 2026-11-19 builder 补：GatekeeperRepository / TapSignKit 在本包里**同时**定义于
# smali_classes2 与 smali_classes11（两份 smali 逐字节相同）。ART 的 DexPathList 按 dex
# 顺序解析 ⇒ classes2 的副本先命中，只改 classes11 等于没改（pre_install_gate 查12 实测
# 就报「classes2.dex 与底包逐字节相同 ⇒ 未改包」）。故 classes2 的这两处也必须一起打。
DEX_D="smali_classes2"
REL_GK="com/taptap/sdk/initializer/repository/GatekeeperRepository.smali"
REL_PROV="com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali"
REL_SIGN="com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
REL_QGUTIL="com/quickgamesdk/utils/QGSdkUtils.smali"
REL_K="com/quicksdk/utility/k.smali"
REL_APKVERIFY="com/game/apkverify/Check.smali"

die() { echo "FAIL: $*" >&2; exit 1; }
say() { echo "  $*"; }

[[ -d "$SMALI_ROOT" ]] || die "解包目录不存在：${SMALI_ROOT}（先跑 decode_apk.sh）"
SMALI_ROOT="$(cd "$SMALI_ROOT" && pwd -P)"
for r in "$DEX_A/$REL_GK" "$DEX_A/$REL_PROV" "$DEX_A/$REL_SIGN" "$DEX_B/$REL_QGUTIL" "$DEX_C/$REL_K" "$DEX_C/$REL_APKVERIFY" "$DEX_D/$REL_GK" "$DEX_D/$REL_SIGN"; do
  [[ -f "$SMALI_ROOT/$r" ]] || die "锚点类缺失（SDK/包结构变了？重取锚点）：$r"
done

echo "== 鹦鹉团 $VERSION TapTap 登录补丁（改包法 7 处） =="
echo "   smali 根：$SMALI_ROOT"
echo "   官方 cert MD5：$OFFICIAL_CERT_MD5"
echo "   模式：$([[ "$DRY_RUN" = "1" ]] && echo "dry-run（源树只读；改动只落 /tmp 副本；打印 diff 后自检）" || echo "打补丁到 /tmp 工作副本")"

# ---- 锚点断言：字符串必须精确出现 N 次，否则说明版本漂移 --------------------
assert_anchor() { # $1=dex目录 $2=相对路径 $3=固定串 $4=期望次数
  local f="$SMALI_ROOT/$1/$2" s="$3" want="$4" got
  got="$(grep -F -c -- "$s" "$f" 2>/dev/null || true)"
  if [ "${got:-0}" != "$want" ]; then
    echo "FAIL: 锚点不符 [$1/$2] want=$want got=${got:-0}" >&2
    echo "锚点原文: $s" >&2
    echo "---- 上下文 ----" >&2
    grep -F -n -m 1 -- "$s" "$f" >&2 || true
    exit 1
  fi
}
# 该锚点在打补丁后会消失（方法体被整体替换）⇒ 0 次时允许通过，但必须能看到回灌常量
assert_anchor_or_patched() { # $1=dex目录 $2=相对路径 $3=固定串
  local f="$SMALI_ROOT/$1/$2" s="$3" got
  got="$(grep -F -c -- "$s" "$f" 2>/dev/null || true)"
  if [ "${got:-0}" = "1" ]; then return 0; fi
  if [ "${got:-0}" = "0" ] && { grep -q "$OFFICIAL_CERT_MD5" "$f" 2>/dev/null || grep -q "$CERT_MD5_UPPER" "$f" 2>/dev/null; }; then
    echo "  ! 锚点已消失但文件内已有回灌常量，视为已打过补丁：[$1/$2]" >&2
    return 0
  fi
  echo "FAIL: 锚点不符 [$1/$2] got=${got:-0}（期望 1，或已打补丁时 0）" >&2
  echo "锚点原文: $s" >&2
  exit 1
}

say "[锚点] P1/P2 GatekeeperRepository"
assert_anchor "$DEX_A" "$REL_GK" ".method public final isGatekeeperLoadedSuccessfully()Z" 1
assert_anchor "$DEX_A" "$REL_GK" ".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V" 1
# 语义锚点：门闩方法体里读该字段的那一条 iget（唯一）。已打过补丁的源树上它会消失，故允许 0 次并 WARN。
GATE_IGET="iget-boolean v0, p0, Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;->loadGatekeeperSuccess:Z"
GATE_CNT="$(grep -F -c -- "$GATE_IGET" "$SMALI_ROOT/$DEX_A/$REL_GK" 2>/dev/null || true)"
if [ "${GATE_CNT:-0}" = "0" ]; then
  echo "  ! 门闩 iget 锚点已消失（源树可能已打过补丁）——若下方自检报「恒 true OK」属正常" >&2
elif [ "$GATE_CNT" != "1" ]; then
  die "门闩 iget 锚点出现 $GATE_CNT 次（期望 1）"
fi
say "[锚点] P3 DefaultTapLoginFragmentProvider"
assert_anchor "$DEX_A" "$REL_PROV" ".method public provide(Lcom/taptap/sdk/login/impl/data/model/LoginRequest;)Landroid/app/Fragment;" 1
assert_anchor "$DEX_A" "$REL_PROV" "    aget v2, v3, v2" 1
say "[锚点] P4 TapSignKit"
assert_anchor "$DEX_A" "$REL_SIGN" ".method public final getSign(Landroid/content/Context;)Ljava/lang/String;" 1
# 寄存器无关的取签名锚点（正文实为 {v0, p1, v1}）：必须唯一
assert_anchor_or_patched "$DEX_A" "$REL_SIGN" "Landroid/content/pm/PackageManager;->getPackageInfo(Ljava/lang/String;I)Landroid/content/pm/PackageInfo;"
assert_anchor_or_patched "$DEX_A" "$REL_SIGN" "CommonUtils;->getMD5([B)Ljava/lang/String;"
say "[锚点] P5a QGSdkUtils.getApkSign"
assert_anchor "$DEX_B" "$REL_QGUTIL" ".method public static getApkSign(Landroid/content/Context;)Ljava/lang/String;" 1
say "[锚点] P5b quicksdk k.a(Context)"
assert_anchor "$DEX_C" "$REL_K" ".method public static a(Landroid/content/Context;)Ljava/lang/String;" 1
say "[锚点] P6 com.game.apkverify.Check.getSignInfo（native 声明，打好后会变成 Java 实现）"
assert_anchor_or_patched "$DEX_C" "$REL_APKVERIFY" ".method public static native getSignInfo(Ljava/lang/Object;)[Ljava/lang/String;"
say "[锚点] 全部命中"

# ---- 工作副本 --------------------------------------------------------------
if [ "$DRY_RUN" = "1" ]; then
  WORK="$(mktemp -d /tmp/ywt_loginpatch_dry.XXXXXX)"
else
  WORK="$(mktemp -d /tmp/ywt_loginpatch.XXXXXX)"
fi
cleanup() { if [ "$KEEP_WORK" = "1" ]; then echo "保留工作副本：$WORK"; else rm -rf "$WORK"; fi; }
trap cleanup EXIT

echo "[1/4] 拷贝受影响的 4 个 dex 的 smali 子树"
mkdir -p "$WORK"
for d in "$DEX_A" "$DEX_B" "$DEX_C" "$DEX_D"; do
  say "${d}（$(du -sm "$SMALI_ROOT/$d" | cut -f1) MB）"
  cp -R "$SMALI_ROOT/$d" "$WORK/$d"
done

echo "[2/4] 打补丁（7 处，含幂等判定与 before/after）"
python3 - "$WORK" "$DEX_A" "$DEX_B" "$DEX_C" "$REL_GK" "$REL_PROV" "$REL_SIGN" "$REL_QGUTIL" "$REL_K" "$OFFICIAL_CERT_MD5" "$DRY_RUN" "$REL_APKVERIFY" "$CERT_MD5_UPPER" "$CERT_SHA1_UPPER" "$CERT_SUBJECT" "$CERT_ISSUER" <<'PYEOF'
import difflib, re, sys

work, dex_a, dex_b, dex_c, rel_gk, rel_prov, rel_sign, rel_qgutil, rel_k, cert_md5, dry, rel_chk, md5_up, sha1_up, cert_subj, cert_iss = sys.argv[1:17]
dry = (dry == "1")
changed = []      # (tag, path, before_text, after_text)
skipped = []      # tag


def read(p):
    with open(p, encoding="utf-8") as f:
        return f.read().split("\n")


def write(p, lines):
    with open(p, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))


def locate(lines, decl):
    for i, l in enumerate(lines):
        if l.strip() == decl:
            k = i
            while k < len(lines) and not re.match(r"\s*\.(locals|registers)\s", lines[k]):
                k += 1
            if k >= len(lines):
                raise SystemExit("FAIL: %s 缺 .locals/.registers" % decl)
            return i, k
    raise SystemExit("FAIL: 找不到方法声明 %s" % decl)


def body_span(lines, decl):
    i, k = locate(lines, decl)
    m = k + 1
    while m < len(lines):
        t = lines[m].strip()
        if t.startswith(".annotation"):
            while m < len(lines) and lines[m].strip() != ".end annotation":
                m += 1
            m += 1
            continue
        if t.startswith(".param"):
            # ★2026-11-19 builder 修（P5b VerifyError 根因）：单行 `.param p0, "context"` 没有配套
            # `.end param`（只有带注解块的 param 才有）。原来无脑向后找 `.end param`，会一路吃到
            # 后面某个 4 参方法的 `.end param`，于是 body_span 算出「下一个方法的 body」，
            # 把 const-string + return-object 写进了 a(Context,String,String,String)V
            # ⇒ 装机后 attachBaseContext 阶段 java.lang.VerifyError: return-object not expected。
            # 新判据：先看下一个非空行是不是 .annotation（param 注解块），是才吃到 .end param。
            j = m + 1
            while j < len(lines) and lines[j].strip() == "":
                j += 1
            if j < len(lines) and lines[j].strip().startswith(".annotation"):
                while m < len(lines) and lines[m].strip() != ".end param":
                    m += 1
                m += 1
            else:
                m += 1
            continue
        if t == "":
            m += 1
            continue
        break
    e = m
    while e < len(lines) and lines[e].strip() != ".end method":
        e += 1
    if e >= len(lines):
        raise SystemExit("FAIL: %s 缺 .end method" % decl)
    return i, k, m, e


def norm(seq):
    return [s.strip() for s in seq if s.strip() and not s.strip().startswith("#")]


def patch_method(path, decl, body, tag):
    lines = read(path)
    i, k, m, e = body_span(lines, decl)
    before = "\n".join(lines[m:e])
    if norm(lines[m:e]) == norm(body):
        skipped.append(tag)
        print("  = %-56s 已是目标形态，跳过" % tag)
        return
    new_lines = lines[:m] + [""] + list(body) + lines[e:]
    after = "\n".join(list(body))
    write(path, new_lines)          # 只在 /tmp 工作副本上写；源树始终只读
    changed.append((tag, path, before, after))
    print("  * %-56s 已替换（原体 %d 行 -> %d 行）" % (tag, e - m, len(body)))


def insert_after(path, decl, needle, insert_line, tag):
    lines = read(path)
    i, k = locate(lines, decl)
    e = i
    while e < len(lines) and lines[e].strip() != ".end method":
        e += 1
    hit = None
    for j in range(i, e):
        if lines[j].strip() == needle:
            hit = j
            break
    if hit is None:
        raise SystemExit("FAIL: %s 内找不到锚点指令 %r" % (decl, needle))
    nxt = [x.strip() for x in lines[hit + 1:hit + 4]]
    if insert_line.strip() in nxt:
        skipped.append(tag)
        print("  = %-56s 已插入，跳过" % tag)
        return
    before = "\n".join(lines[hit - 1:hit + 3])
    new_lines = lines[:hit + 1] + ["", insert_line] + lines[hit + 1:]
    after = "\n".join(new_lines[hit - 1:hit + 5])
    write(path, new_lines)          # 只在 /tmp 工作副本上写；源树始终只读
    changed.append((tag, path, before, after))
    print("  * %-56s 已在锚点后插入 %s" % (tag, insert_line.strip()))


def patch_native_to_java(path, decl_native, decl_java, body, tag):
    """把 native 声明改成同签名同返回类型的 Java 实现（整段替换 声明行 + 空方法体）。"""
    lines = read(path)
    if decl_java in lines:
        # 已经是 Java 实现：整段（声明之后到 .end method 之前）与目标体比较
        i = lines.index(decl_java)
        e = i
        while e < len(lines) and lines[e].strip() != ".end method":
            e += 1
        if norm(lines[i + 1:e]) == norm(body):
            skipped.append(tag)
            print("  = %-56s 已是 Java 常量实现，跳过" % tag)
            return
        raise SystemExit("FAIL: %s 已是 Java 实现但方法体不是目标形态（人工介入）" % decl_java)
    if decl_native not in lines:
        raise SystemExit("FAIL: 找不到 native 声明 %s" % decl_native)
    i = lines.index(decl_native)
    e = i
    while e < len(lines) and lines[e].strip() != ".end method":
        e += 1
    if e >= len(lines):
        raise SystemExit("FAIL: %s 缺 .end method" % decl_native)
    before = "\n".join(lines[i:e + 1])
    new_lines = lines[:i] + [decl_java] + [""] + list(body) + [".end method"] + lines[e + 1:]
    write(path, new_lines)
    after = "\n".join([decl_java, ""] + list(body) + [".end method"])
    changed.append((tag, path, before, after))
    print("  * %-56s native -> Java（原声明 %d 行）" % (tag, e - i + 1))


P_GK = work + "/" + dex_a + "/" + rel_gk
P_CHK = work + "/" + dex_c + "/" + rel_chk
P_PROV = work + "/" + dex_a + "/" + rel_prov
P_SIGN = work + "/" + dex_a + "/" + rel_sign
P_QGU = work + "/" + dex_b + "/" + rel_qgutil
P_K = work + "/" + dex_c + "/" + rel_k

# P1 门闩恒放行（本包无 checkLocalInitialize，只改这一处；不改 checkInitialize 以免绕过未初始化保护）
patch_method(P_GK,
             ".method public final isGatekeeperLoadedSuccessfully()Z",
             ["    const/4 v0, 0x1", "", "    return v0"],
             "P1 GatekeeperRepository.isGatekeeperLoadedSuccessfully")

# P2 验签失败弹窗掐掉（保留原 .locals 3：改小 smali 汇编会报寄存器不足）
patch_method(P_GK,
             ".method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V",
             ["    return-void"],
             "P2 GatekeeperRepository.showErrorToast")

# P3 登录入口恒 Web（1=CLIENT 2=CLOUD_GAME 3=WEB）
insert_after(P_PROV,
             ".method public provide(Lcom/taptap/sdk/login/impl/data/model/LoginRequest;)Landroid/app/Fragment;",
             "aget v2, v3, v2",
             "    const/4 v2, 0x3",
             "P3 DefaultTapLoginFragmentProvider.provide")

# P4 签名回灌（必需）：GateKeeperRequest(platform, packageName, signature) 的 signature 来源
patch_method(P_SIGN,
             ".method public final getSign(Landroid/content/Context;)Ljava/lang/String;",
             ["    const-string v0, \"%s\"" % cert_md5, "", "    return-object v0"],
             "P4 TapSignKit.getSign（回灌官方 cert MD5）")

# P5a QGSDK 每请求必带的 signMd5
patch_method(P_QGU,
             ".method public static getApkSign(Landroid/content/Context;)Ljava/lang/String;",
             ["    const-string v0, \"%s\"" % md5_up, "", "    return-object v0"],
             "P5a QGSdkUtils.getApkSign")

# P5b 另一条上报路径的 sign_md5
patch_method(P_K,
             ".method public static a(Landroid/content/Context;)Ljava/lang/String;",
             ["    const-string v0, \"%s\"" % md5_up, "", "    return-object v0"],
             "P5b com.quicksdk.utility.k.a(Context)")

# P6 单点兜底：把 Check.getSignInfo 由 native 改成 Java 常量实现（同签名同返回类型）。
#    为什么必须改这里：com/quicksdk/net/a.smali:6107 **直接**调 getSignInfo 并取 [3]=sign_sha1，
#    不经过 QGSdkUtils.getApkSign ⇒ P5a 覆盖不到；此处一次覆盖全部消费者（含 [0]subject [1]issuer）。
#    顺序与大小写照抄 native：getCertificateInfo 先 get_digest("MD5") 后 get_digest("SHA1")，
#    且 get_digest 用 .rodata "0123456789ABCDEF" 手工转十六进制 ⇒ 大写。
P6_DECL_NATIVE = ".method public static native getSignInfo(Ljava/lang/Object;)[Ljava/lang/String;"
P6_DECL_JAVA = ".method public static getSignInfo(Ljava/lang/Object;)[Ljava/lang/String;"
p6_body = [
    "    .locals 3",
    "",
    "    const/4 v0, 0x4",
    "",
    "    new-array v0, v0, [Ljava/lang/String;",
    "",
    "    const/4 v1, 0x0",
    "",
    '    const-string v2, "%s"' % cert_subj,
    "",
    "    aput-object v2, v0, v1",
    "",
    "    const/4 v1, 0x1",
    "",
    '    const-string v2, "%s"' % cert_iss,
    "",
    "    aput-object v2, v0, v1",
    "",
    "    const/4 v1, 0x2",
    "",
    '    const-string v2, "%s"' % md5_up,
    "",
    "    aput-object v2, v0, v1",
    "",
    "    const/4 v1, 0x3",
    "",
    '    const-string v2, "%s"' % sha1_up,
    "",
    "    aput-object v2, v0, v1",
    "",
    "    return-object v0",
]
patch_native_to_java(P_CHK, P6_DECL_NATIVE, P6_DECL_JAVA, p6_body,
                     "P6 Check.getSignInfo（native -> Java 常量实现）")

print("")
print("  ---- 7 处汇总：改动 %d，已就位 %d ----" % (len(changed), len(skipped)))

if changed:
    print("")
    print("  ======== before / after（逐处）========")
    for tag, path, before, after in changed:
        print("")
        print("  ### " + tag + "  ->  " + path.replace(work + "/", ""))
        print("  --- before ---")
        for l in before.split("\n")[:14]:
            print("  | " + l)
        print("  --- after ----")
        for l in after.split("\n")[:14]:
            print("  | " + l)

if dry and changed:
    print("")
    print("  ======== unified diff（dry-run：源树未改，改动只在 /tmp 副本）========")
    for tag, path, before, after in changed:
        print("")
        print("  --- " + path.replace(work + "/", "") + "   [" + tag + "]")
        print("  --- " + path.replace(work + "/", ""))
        for l in difflib.unified_diff(before.split("\n"), after.split("\n"),
                                      fromfile="before", tofile="after", lineterm="", n=2):
            print("  " + l)

if len(changed) + len(skipped) != 7:
    raise SystemExit("FAIL: 7 处补丁未全部处理（改动 %d + 就位 %d）" % (len(changed), len(skipped)))
PYEOF

echo "[3/4] 语法与范围自检（.method/.end method 配对、方法体可定位、常量已落位）"
python3 - "$WORK" "$DEX_A" "$DEX_B" "$DEX_C" "$REL_GK" "$REL_PROV" "$REL_SIGN" "$REL_QGUTIL" "$REL_K" "$OFFICIAL_CERT_MD5" "$REL_APKVERIFY" "$CERT_MD5_UPPER" "$CERT_SHA1_UPPER" "$CERT_SUBJECT" <<'PYEOF'
import re, sys

work, dex_a, dex_b, dex_c, rel_gk, rel_prov, rel_sign, rel_qgutil, rel_k, md5, rel_chk, md5_up, sha1_up, subj = sys.argv[1:15]
targets = [
    (dex_a + "/" + rel_gk, ["isGatekeeperLoadedSuccessfully()Z", "showErrorToast("]),
    (dex_a + "/" + rel_prov, ["provide("]),
    (dex_a + "/" + rel_sign, ["getSign("]),
    (dex_b + "/" + rel_qgutil, ["getApkSign("]),
    (dex_c + "/" + rel_k, ["a(Landroid/content/Context;)"]),
    (dex_c + "/" + rel_chk, ["getSignInfo("]),
]
fail = 0
for rel, decls in targets:
    p = work + "/" + rel
    txt = open(p, encoding="utf-8").read()
    n_decl = len(re.findall(r"(?m)^\.method ", txt))
    n_end = len(re.findall(r"(?m)^\.end method$", txt))
    ok_pair = (n_decl == n_end)
    ok_decl = all(d in txt for d in decls)
    print("  %-58s .method=%d .end=%d %s" % (rel.split("/")[-1], n_decl, n_end,
          "OK" if ok_pair and ok_decl else "FAIL"))
    if not (ok_pair and ok_decl):
        fail = 1

# 常量落位：P4 小写（TapTap CommonUtils → Integer.toHexString）；P5a/P5b 大写（native 十六进制表）；P6 大写 md5+sha1
if md5 not in open(work + "/" + dex_a + "/" + rel_sign, encoding="utf-8").read():
    print("  FAIL: TapSignKit 内没有小写回灌常量（P4）")
    fail = 1
for rel in (dex_b + "/" + rel_qgutil, dex_c + "/" + rel_k):
    if md5_up not in open(work + "/" + rel, encoding="utf-8").read():
        print("  FAIL: %s 内没有大写回灌常量（P5）" % rel)
        fail = 1
chk_txt = open(work + "/" + dex_c + "/" + rel_chk, encoding="utf-8").read()
if "native getSignInfo" in chk_txt:
    print("  FAIL: Check.getSignInfo 仍是 native 声明（P6 未生效）")
    fail = 1
for probe, name in ((md5_up, "MD5"), (sha1_up, "SHA1"), (subj, "subject")):
    if probe not in chk_txt:
        print("  FAIL: Check.getSignInfo 缺少 %s 常量" % name)
        fail = 1
if chk_txt.count("aput-object v2, v0, v1") != 4:
    print("  FAIL: Check.getSignInfo 的 aput 次数 = %d（期望 4）" % chk_txt.count("aput-object v2, v0, v1"))
    fail = 1
if "return-object v0" not in chk_txt:
    print("  FAIL: Check.getSignInfo 缺 return-object")
    fail = 1
gate_txt = open(work + "/" + dex_a + "/" + rel_gk, encoding="utf-8").read()
m = re.search(r"(?ms)^\.method public final isGatekeeperLoadedSuccessfully\(\)Z(.*?)^\.end method", gate_txt)
if not m or "const/4 v0, 0x1" not in m.group(1):
    print("  FAIL: 门闩方法体未改成恒 true")
    fail = 1
else:
    print("  门闩方法体：恒 true OK")

prov_txt = open(work + "/" + dex_a + "/" + rel_prov, encoding="utf-8").read()
if prov_txt.count("const/4 v2, 0x3") != 1:
    print("  FAIL: provide 内 const/4 v2, 0x3 出现次数 = %d（期望 1）" % prov_txt.count("const/4 v2, 0x3"))
    fail = 1
else:
    print("  provide 强制 Web 指令：出现 1 次 OK")

# ★2026-11-19 builder 加：VerifyError 回归闸
#   任何返回 void 的方法体里出现 return-object，ART 校验期直接拒类
#   （实测事故：P5b 把 const-string+return-object 写进 a(Context,String,String,String)V，
#    装机后 QuickSdkApplication.attachBaseContext 阶段 VerifyError: return-object not expected）。
#   这里把本轮动过的 6 个类全量扫一遍：void 方法体内不许出现 return-object。
_VOIDGUARD = [work + "/" + dex_c + "/" + rel_k,
              work + "/" + dex_c + "/" + rel_chk,
              work + "/" + dex_b + "/" + rel_qgutil,
              work + "/" + dex_a + "/" + rel_gk,
              work + "/" + dex_a + "/" + rel_sign,
              work + "/" + dex_a + "/" + rel_prov]
for _f in _VOIDGUARD:
    _txt = open(_f, encoding="utf-8").read()
    for _blk in re.findall(r"(?ms)^\.method [^\n]*\)V\s*$(.*?)^\.end method", _txt):
        if "return-object" in _blk:
            print("  FAIL: %s 的 void 方法体里出现 return-object（ART VerifyError 必崩）" % _f.split("/")[-1])
            fail = 1
if not fail:
    print("  void/return-object 交叉检查：6 个类全部通过 OK")

if fail:
    raise SystemExit("FAIL: 语法/范围自检未通过")
print("  ✓ 7 处全部命中且结构无损")
PYEOF

# [3.5/4] 重复类同步：classes2 的副本先被 ART 命中，必须与 classes11 保持一致
echo "[3.5/4] 同步重复类到 ${DEX_D}（classes2 优先命中，必须一起打）"
for r in "$REL_GK" "$REL_SIGN"; do
  cp -f "$WORK/$DEX_A/$r" "$WORK/$DEX_D/$r" || die "同步失败：$r"
  cmp -s "$WORK/$DEX_A/$r" "$WORK/$DEX_D/$r" || die "同步后两份不一致：$r"
  say "同步 $r"
done
# 同步后仍必须带补丁痕迹（防「同步成了未打补丁的原文件」）：
#   TapSignKit = 回灌的证书常量；GatekeeperRepository = 门闩恒 true（常量不在这个类里）
if ! grep -q -F "$OFFICIAL_CERT_MD5" "$WORK/$DEX_D/$REL_SIGN" && ! grep -q -F "$CERT_MD5_UPPER" "$WORK/$DEX_D/$REL_SIGN"; then
  die "$DEX_D/$REL_SIGN 同步后找不到回灌常量"
fi
if ! grep -q -F "const/4 v0, 0x1" "$WORK/$DEX_D/$REL_GK"; then
  die "$DEX_D/$REL_GK 同步后看不到门闩恒 true"
fi
say "✓ $DEX_D 两个重复类已带补丁痕迹（cert 常量 + 门闩恒 true）"

if [ "$BUILD_DEX" = "1" ]; then
  echo "[4/4] 汇编受影响 dex（BUILD_DEX=1；不含打包/签名/装机）"
  [[ -x "$SMALI_BIN" ]] || die "缺少 01_tools/smali/smali"
  OUT_DIR="${OUT_DIR:-$WORK/out}"
  mkdir -p "$OUT_DIR"
  for d in "$DEX_A" "$DEX_B" "$DEX_C" "$DEX_D"; do
    case "$d" in
      smali) out="classes.dex" ;;
      *) out="$(echo "$d" | sed "s/^smali_classes/classes/").dex" ;;
    esac
    say "assemble $d -> $OUT_DIR/$out"
    "$SMALI_BIN" assemble --api "$API_LEVEL" -j 1 -o "$OUT_DIR/$out" "$WORK/$d"
    say "  md5 $(md5 -q "$OUT_DIR/$out")  size $(stat -f%z "$OUT_DIR/$out")"
  done
  for f in "$OUT_DIR"/*.dex; do
    # classes11 用 TapTap 侧小写常量；classes7/classes 用 QGSDK 侧大写常量 ⇒ 两种都接受
    if ! grep -a -q "$OFFICIAL_CERT_MD5" "$f" && ! grep -a -q "$CERT_MD5_UPPER" "$f"; then
      die "产物 $(basename "$f") 内找不到回灌常量"
    fi
    if grep -a -q "com/starry/magic/sign/SignSpoof" "$f"; then
      die "产物出现 SignSpoof（该类已于 2026-10-28 删除，禁止复活）"
    fi
  done
  say "✓ 四个 dex 均含回灌常量且无 SignSpoof"
else
  echo "[4/4] 跳过汇编（默认 BUILD_DEX=0；出包由管线统一执行）"
fi

echo
echo "OK  7 处补丁已就绪$([[ "$DRY_RUN" = "1" ]] && echo "（dry-run：源树未改、未汇编）")"
echo "    下一步：由管线执行「回灌 dex -> 外科替换 -> zipalign/apksigner -> 装机前门禁」"
