#!/usr/bin/env bash
# Phigros 3.20.0 — TapTap 强制内嵌网页登录：重建 classes2.dex 的确定性入口
#
# 背景：游戏内「账号与统计 → TapTap 登录」被 TapTap 客户端以 signature not match 拒绝
#       （见 10_docs/phigros/复核_TapTap签名与网页登录_20260921.md §3.4）。
# 做法：baksmali 拆 mod 自有 classes2.dex → 打两处补丁 → smali 重新组装 → 交给
#       surgical_phigros_dex_patch.sh 换 dex/重签/装机。
#
#   补丁 1：TapLoginForceWeb.PKG_WHITELIST 增加 "com.PigeonGames.Phigros"
#   补丁 2：TapLoginForceWeb.onPreCreated 调 forceOldImplWebEarly，把 intent extra
#           "request" 用 Parcel 往返成 YnttLoginRequest（getRequestCode() 抛异常）
#           → LoginClient.sendLoginRequest 走 catch → 官方 WebViewHandler 内嵌网页登录
#   补丁 3：注入 YnttLoginRequest(.$1).smali（来源 yntt_smali/）
#
# 用法:
#   ./build_phigros_dex_patch.sh <base.apk> [out_dir]
#   base.apk = 已装的 Phigros 改签包（含 mod 的 classes2.dex）；out_dir 默认 /tmp/phbuild
set -euo pipefail

BASE="${1:?base apk}"
OUT_DIR="${2:-/tmp/phbuild}"
STARRY="/Users/aliang/Documents/nixiang/mod菜单/starry-mod-menu"
SMALI_BIN="/Users/aliang/Documents/nixiang/01_tools/smali/smali"
BAKSMALI_BIN="/Users/aliang/Documents/nixiang/01_tools/smali/baksmali"
YNTT="$STARRY/yntt_smali/com/starry/magic/sign"

[[ -f "$BASE" ]] || { echo "ERROR: 缺少 base apk" >&2; exit 1; }
mkdir -p "$OUT_DIR"
WORK="$OUT_DIR/dexwork"
rm -rf "$WORK"; mkdir -p "$WORK"

echo "==> [1/4] 取出 mod 的 classes2.dex"
unzip -o -q "$BASE" classes2.dex -d "$WORK"

echo "==> [2/4] baksmali 拆解"
"$BAKSMALI_BIN" d "$WORK/classes2.dex" -o "$WORK/sm" >/dev/null

echo "==> [3/4] 打补丁"
SIGN_DIR="$WORK/sm/com/starry/magic/sign"
[[ -f "$SIGN_DIR/TapLoginForceWeb.smali" ]] || { echo "ERROR: 该 base 不含 TapLoginForceWeb" >&2; exit 1; }
python3 - "$SIGN_DIR/TapLoginForceWeb.smali" <<'PY'
import sys, pathlib
p = pathlib.Path(sys.argv[1])
t = p.read_text(encoding="utf-8")

# 补丁 1：白名单（幂等）
anchor = '    const-string v3, "com.hypergryph.endfield"\n'
assert anchor in t, "whitelist anchor missing"
if "com.PigeonGames.Phigros" not in t:
    t = t.replace(anchor, anchor + '\n    const-string v3, "com.PigeonGames.Phigros"\n', 1)

# 补丁 2：onPreCreated 里加旧版强制网页钩子
old = """    if-lt v0, v1, :cond_9

    .line 6
    .line 7
    invoke-static {p0}, Lcom/starry/magic/sign/TapLoginForceWeb;->sanitize(Landroid/app/Activity;)V
"""
new = """    if-lt v0, v1, :cond_9

    .line 6
    invoke-static {p0}, Lcom/starry/magic/sign/TapLoginForceWeb;->forceOldImplWebEarly(Landroid/app/Activity;)V

    .line 7
    invoke-static {p0}, Lcom/starry/magic/sign/TapLoginForceWeb;->sanitize(Landroid/app/Activity;)V
"""
assert old in t, "onPreCreated anchor missing"
t = t.replace(old, new, 1)
p.write_text(t, encoding="utf-8")
print("    whitelist + onPreCreated hook patched")
PY

# 补丁 2b/3：钩子方法与 YnttLoginRequest
python3 - "$SIGN_DIR/TapLoginForceWeb.smali" "$YNTT" <<'PY'
import sys, pathlib, shutil
target = pathlib.Path(sys.argv[1]); yntt = pathlib.Path(sys.argv[2])
HOOK = r'''
# Phigros 等 TapSDK 3.30.x 旧版路径：onCreate 之前把 intent extra "request"
# 换成 YnttLoginRequest；其 getRequestCode() 抛异常，被 LoginClient.sendLoginRequest
# 的 catch 捕获 → 回落官方 WebViewHandler（内嵌网页登录），不进 TapTap 客户端。
.method private static forceOldImplWebEarly(Landroid/app/Activity;)V
    .registers 6

    :try_start_0
    if-eqz p0, :cond_ret

    invoke-virtual {p0}, Landroid/app/Activity;->getClass()Ljava/lang/Class;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v0

    const-string v1, "com.taptap.sdk.ui.TapTapActivity"

    invoke-virtual {v1, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_ret

    invoke-virtual {p0}, Landroid/app/Activity;->getPackageName()Ljava/lang/String;

    move-result-object v0

    sget-object v1, Lcom/starry/magic/sign/TapLoginForceWeb;->PKG_WHITELIST:Ljava/util/Set;

    invoke-interface {v1, v0}, Ljava/util/Set;->contains(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_go

    :cond_ret
    return-void

    :cond_go
    invoke-virtual {p0}, Landroid/app/Activity;->getIntent()Landroid/content/Intent;

    move-result-object v0

    if-nez v0, :cond_intent_ok

    return-void

    :cond_intent_ok
    const-string v1, "request"

    invoke-virtual {v0, v1}, Landroid/content/Intent;->getParcelableExtra(Ljava/lang/String;)Landroid/os/Parcelable;

    move-result-object v2

    if-nez v2, :cond_req_ok

    return-void

    :cond_req_ok
    invoke-virtual {v2}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v3

    const-string v4, "com.starry.magic.sign.YnttLoginRequest"

    invoke-virtual {v4, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_build

    return-void

    :cond_build
    invoke-static {v2}, Lcom/starry/magic/sign/TapLoginForceWeb;->asYnttRequest(Landroid/os/Parcelable;)Landroid/os/Parcelable;

    move-result-object v4

    if-nez v4, :cond_put

    return-void

    :cond_put
    invoke-virtual {v0, v1, v4}, Landroid/content/Intent;->putExtra(Ljava/lang/String;Landroid/os/Parcelable;)Landroid/content/Intent;
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    return-void

    :catch_0
    move-exception v0

    return-void
.end method

# 把原始 LoginRequest 用 Parcel 往返成 YnttLoginRequest（同名同字段的 Parcelable 子类，
# 其 getRequestCode() 抛异常）。刻意不用泛型 API：Class.asSubclass 会让跨 dex 校验器
# 判「expected 2 argument registers, method signature has 1」而 VerifyError。
.method private static asYnttRequest(Landroid/os/Parcelable;)Landroid/os/Parcelable;
    .registers 4

    const/4 v2, 0x0

    :try_start_0
    invoke-static {}, Landroid/os/Parcel;->obtain()Landroid/os/Parcel;

    move-result-object v0

    const/4 v1, 0x0

    invoke-interface {p0, v0, v1}, Landroid/os/Parcelable;->writeToParcel(Landroid/os/Parcel;I)V

    invoke-virtual {v0, v1}, Landroid/os/Parcel;->setDataPosition(I)V

    sget-object v1, Lcom/starry/magic/sign/YnttLoginRequest;->CREATOR:Landroid/os/Parcelable$Creator;

    invoke-interface {v1, v0}, Landroid/os/Parcelable$Creator;->createFromParcel(Landroid/os/Parcel;)Ljava/lang/Object;

    move-result-object v1

    invoke-virtual {v0}, Landroid/os/Parcel;->recycle()V

    move-object v2, v1

    check-cast v2, Landroid/os/Parcelable;
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :catch_0
    return-object v2
.end method
'''
t = target.read_text(encoding="utf-8")
if ".method private static forceOldImplWebEarly" in t:
    print("    hook already present, skip")
else:
    t = t.replace(".method private static sanitize(", HOOK.strip() + "\n\n.method private static sanitize(", 1)
    target.write_text(t, encoding="utf-8")
for f in ("YnttLoginRequest.smali", "YnttLoginRequest$1.smali"):
    shutil.copy(yntt / f, target.parent / f)
print("    hook + YnttLoginRequest injected")
PY

echo "==> [4/4] 重新组装 dex"
"$SMALI_BIN" a -a 35 -o "$OUT_DIR/classes2_new.dex" "$WORK/sm"
echo "    $OUT_DIR/classes2_new.dex"
echo
echo "下一步："
echo "  $STARRY/scripts/surgical_phigros_dex_patch.sh $BASE $OUT_DIR/classes2_new.dex <out.apk> [--install]"
