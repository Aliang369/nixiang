#!/usr/bin/env bash
# 光阴之外 1.0.3 TapTap 原版 → 星空魔法外科组装（Cocos Creator 3.x / libcocos2djs.so）
#
# 产出：08_output/光阴之外/光阴之外_1.0.3_surgical-releaseSigned.apk
#
# 注入内容
#   classes5.dex                    星空菜单（com.starry.magic.**，含 GyzwHooks）
#   lib/arm64-v8a/libstarry_magic.so   native：hook se::ScriptEngine::evalString，splice boot JS
#   lib/arm64-v8a/libc++_shared.so
#   assets/certs/starlight.crt
#   AndroidManifest.xml             分支 B（StarryInitProvider，**不替换官方 Application**）
#                                   + MenuService + SYSTEM_ALERT_WINDOW/FOREGROUND_SERVICE*
#   classes3.dex                    ★唯一被改写的官方 dex：TapAdModImpl.getRewardVideo 前置免广告门控
#
# 字节不变（脚本内断言 sha256）：classes.dex / classes2.dex / classes4.dex /
#   resources.arsc / lib/arm64-v8a/libcocos2djs.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"

GAME="光阴之外"
VERSION="1.0.3"
PKG="com.shbq.gyzw.ppyx"
TARGET_DEX="classes3.dex"          # com/sp/mod/tapad/TapAdModImpl 所在（28 个 com.sp.mod.tapad.* 类）
TARGET_DEX2="classes4.dex"         # ★com/sp/common/SPBaseController 所在（真正的广告分发入口）
NEW_DEX="classes5.dex"             # 官方已有 classes.dex/2/3/4

ORIG_APK="${ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_1.0.3_taptap_original.apk}"
ORIG_MD5="${ORIG_MD5:-363ac0e6926b3b7a1ea71f804500d1fd}"
ORIG_TARGET_DEX_MD5="${ORIG_TARGET_DEX_MD5:-b2cbeeffba6a9003d417c5284df8f150}"
ORIG_TARGET_DEX2_MD5="${ORIG_TARGET_DEX2_MD5:-6c9ab10baa1e34a7930206813d17a4de}"

OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SIGNED="$OUT/${GAME}_${VERSION}_surgical-releaseSigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SO_PREBUILT="${GYZW_SO:-$STARRY_SRC/libs_gyzw/arm64-v8a/libstarry_magic.so}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]]      || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]]     || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]]  || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]]   || { echo "ERROR: missing $SO_PREBUILT (先跑 build_so)" >&2; exit 1; }

echo "[check] 底包 = $ORIG_APK"
GOT_MD5="$(md5 -q "$ORIG_APK")"
[[ "$GOT_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 原包 md5=$GOT_MD5 != $ORIG_MD5" >&2; exit 1; }
GOT_C3="$(unzip -p "$ORIG_APK" "$TARGET_DEX" | md5 -q)"
[[ "$GOT_C3" == "$ORIG_TARGET_DEX_MD5" ]] || { echo "ERROR: $TARGET_DEX md5=$GOT_C3 != $ORIG_TARGET_DEX_MD5" >&2; exit 1; }
GOT_C4="$(unzip -p "$ORIG_APK" "$TARGET_DEX2" | md5 -q)"
[[ "$GOT_C4" == "$ORIG_TARGET_DEX2_MD5" ]] || { echo "ERROR: $TARGET_DEX2 md5=$GOT_C4 != $ORIG_TARGET_DEX2_MD5" >&2; exit 1; }
echo "[check] 原包 md5 ok / $TARGET_DEX md5 ok / $TARGET_DEX2 md5 ok"

# so 归属校验（防串包：上一款游戏的 so 顶包）
if ! LC_ALL=C grep -a -q '\[Gyzw\]' "$SO_PREBUILT" \
   || ! LC_ALL=C grep -a -q 'Category_\u6218\u6597\|Category_' "$SO_PREBUILT"; then
  echo "ERROR: $SO_PREBUILT 不是 Gyzw 版（缺 [Gyzw] / Category_）" >&2
  strings "$SO_PREBUILT" | grep -aE '^\[(Gyzw|Dwct|Tlgd|Ysxz)' | sort -u | head -20 || true
  exit 1
fi
if LC_ALL=C grep -a -q '\[Ysxz\]' "$SO_PREBUILT"; then
  echo "ERROR: $SO_PREBUILT 含 [Ysxz]，libs_gyzw 被覆盖" >&2; exit 1
fi
SO_MD5="$(md5 -q "$SO_PREBUILT")"
echo "[check] so = $SO_PREBUILT md5=$SO_MD5"

LOCK_DIR="/tmp/gyzw_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/gyzw_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ---------------------------------------------------------------- 1/6
echo "[1/6] gradle assembleRelease（星空 APK → smali）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }
# 星空 APK 内 so 必须是 Gyzw 版（gradle 打进 jniLibs 的那份）
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/starry_inner.so"
if ! LC_ALL=C grep -a -q '\[Gyzw\]' "$WORK/starry_inner.so"; then
  echo "ERROR: 星空 APK 内 so 不是 Gyzw 版（jniLibs 未刷新）" >&2
  strings "$WORK/starry_inner.so" | grep -aE '^\[(Gyzw|Dwct|Ysxz|Tlgd)' | sort -u | head || true
  exit 1
fi
echo "  星空 APK ok（内嵌 so = Gyzw 版）"

# ---------------------------------------------------------------- 2/6
echo "[2/6] com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for cls in StarryInitProvider MenuService ModBridge sign/SignSpoof sign/TapLoginForceWeb cocos/GyzwHooks; do
  find "$SMALI_ROOT/com" -type f -name "$(basename "$cls").smali" | grep -q . || {
    echo "ERROR: starry smali 缺 $cls" >&2; exit 1; }
done
# 两张表必须同步（SignSpoof 证书表 + TapLoginForceWeb 白名单）
grep -q 'com[./]shbq[./]gyzw[./]ppyx' "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" \
  || { echo "ERROR: SignSpoof.smali 未含 com.shbq.gyzw.ppyx" >&2; exit 1; }
grep -q 'com[./]shbq[./]gyzw[./]ppyx' "$SMALI_ROOT/com/starry/magic/sign/TapLoginForceWeb.smali" \
  || { echo "ERROR: TapLoginForceWeb.smali 白名单未含 com.shbq.gyzw.ppyx" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi
[[ -f "$WORK/assets/certs/starlight.crt" ]] || { echo "ERROR: starlight.crt missing" >&2; exit 1; }

# ---------------------------------------------------------------- 3/6
echo "[3/6] $TARGET_DEX 免广告门控（两层）"
unzip -p "$ORIG_APK" "$TARGET_DEX" > "$WORK/$TARGET_DEX"
"$BAKSMALI_BIN" d -o "$WORK/smali_c3" "$WORK/$TARGET_DEX"
python3.11 - \
  "$WORK/smali_c3/com/sp/mod/tapad/manager/TapAdRewardManager.smali" \
  "$WORK/smali_c3/com/sp/mod/tapad/TapAdModImpl.smali" <<'PY'
# 免广告门控打在两层（缺一不可，见 2026-09-26 首轮实测）：
#   A) TapAdRewardManager.showRewardVideoAutoAd —— ★真正被游戏调用的展示入口
#      （TapAdRewardVideo.getRewardVideo:315 调它）。首轮只打了 TapAdModImpl
#      （一个 append-only 空壳），门控被调用但广告照弹，日志「激励视频展示」
#      证明真 SDK 仍被走到 ⇒ 必须打这一层才拦得住。拦截后直接伪造 listener
#      回调，完全绕开 TapSDK 与网络。
#   B) TapAdModImpl.getRewardVideo —— 上游包装层，保留拦截以覆盖其它调用方
#      （用 RewardVideoCallback 同形假回调）。
import sys
from pathlib import Path

GATE_MGR = '''
    # ---- 星空免广告门控（TapAdRewardManager，真展示入口）----
    # 本方法 .registers 已从 5 抬到 10；参数占 p0=5 p1=6 p2=7，
    # 本门控只用 v0..v4（局部区），与参数区完全不重叠。
    invoke-static {}, Lcom/starry/magic/cocos/GyzwHooks;->shouldSkipAd()Z

    move-result v0

    # ★极性铁律：if-eqz = 「noAd 为 false 时跳回原逻辑」。
    #   写成 if-nez 会把语义整个反过来——开启免广告反而走真广告路径。
    #   对照已验证先例 surgical_dwct_12.39.sh:193 `if-eqz v0, :cond_starry_ad`。
    if-eqz v0, :gyzw_mgr_orig

    const-string v0, "Gyzw"

    const-string v1, "no-ad gate: TapAdRewardManager.showRewardVideoAutoAd intercepted"

    invoke-static {v0, v1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    # 完全绕开 TapSDK：直接伪造 RewardVideoAutoAdListener 生命周期。
    # onRewardVerify(Z I String I String) —— rewardVerify=true / amount=123(0x7b,与 request 一致)
    # / name="\\u91d1\\u5e01"(金币) / code=0 / msg="\\u89c6\\u9891\\u89c2\\u770b\\u5b8c\\u6210"(视频观看完成)
    # ★寄存器纪律（第二轮实测踩坑）：/range 要求区间按寄存器号**递增**且 receiver 在首位。
    #   原方法 .registers=5 时 p2 位于寄存器 4，{p2 .. v4} 是 4..4 且 v4 不是参数 → 报
    #   "A register range must have the lower register listed first" / "Invalid register count: -4"。
    #   正确做法：把 listener 挪进 v0，5 个实参依次放 v1..v5，用 {v0 .. v5}（0→5 递增）。
    move-object v0, p2

    const/4 v1, 0x1

    const/16 v2, 0x7b

    const-string v3, "\\u91d1\\u5e01"

    const/4 v4, 0x0

    const-string v5, "\\u89c6\\u9891\\u89c2\\u770b\\u5b8c\\u6210"

    invoke-interface/range {v0 .. v5}, Lcom/tapsdk/tapad/group/DirichletAdNative$RewardVideoAutoAdListener;->onRewardVerify(ZILjava/lang/String;ILjava/lang/String;)V

    # 关卡收尾顺序照抄真实链路：onAdShow → onAdClose（TapAdRewardVideo$2 的语义）
    invoke-interface {v0}, Lcom/tapsdk/tapad/group/DirichletAdNative$RewardVideoAutoAdListener;->onAdShow()V

    invoke-interface {v0}, Lcom/tapsdk/tapad/group/DirichletAdNative$RewardVideoAutoAdListener;->onAdClose()V

    return-void

    :gyzw_mgr_orig
'''

GATE_IMPL = '''
    # ---- 星空免广告门控（TapAdModImpl，上游包装层）----
    # 本方法 .registers 已从 4 抬到 8；参数 p0=4 p1=5，门控只用 v0/v1。
    invoke-static {}, Lcom/starry/magic/cocos/GyzwHooks;->shouldSkipAd()Z

    move-result v0

    # ★极性铁律：if-eqz = 「noAd 为 false 时跳回原逻辑」（对照 surgical_dwct_12.39.sh:193）。
    if-eqz v0, :gyzw_impl_orig

    const-string v0, "Gyzw"

    const-string v1, "no-ad gate: TapAdModImpl.getRewardVideo intercepted"

    invoke-static {v0, v1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    # 同形假回调：先 onResult(0,"视频观看完成") 再 onAdClose("广告页面已关闭")
    const/4 v0, 0x0

    const-string v1, "\\u89c6\\u9891\\u89c2\\u770b\\u5b8c\\u6210"

    invoke-virtual {p1, v0, v1}, Lcom/sp/sdk/core/callback/RewardVideoCallback;->onResult(ILjava/lang/String;)V

    const-string v1, "\\u5e7f\\u544a\\u9875\\u9762\\u5df2\\u5173\\u95ed"

    invoke-virtual {p1, v1}, Lcom/sp/sdk/core/callback/RewardVideoCallback;->onAdClose(Ljava/lang/String;)V

    return-void

    :gyzw_impl_orig
'''


def inject(path, sig, gate, label, need_words):
    """在方法体最前面插入门控。

    ★寄存器纪律（2026-09-26 第二轮实测踩坑）：
    必须在插入前把 `.registers N` 抬到至少 `N + need_words`，并把门控用的 v 寄存器
    全部约束在 [0, N) 区间内（即原方法自己的局部寄存器区），绝不能碰到参数寄存器
    p0..pN。第一版直接把 v0..v4 当参数用，且没改 .registers，导致
        dex2oat64: outs_size (6d) > registers_size (5d)
    装机后 ART 校验失败、进程起不来（logcat 只有 dexopt FAILED，没有 Gyzw 日志）。
    """
    p = Path(path)
    s = p.read_text(encoding="utf-8")
    start = s.index(sig)
    end = s.index(".end method", start)
    body = s[start:end]

    # 已经打过就跳过（幂等）
    if "GyzwHooks" in body:
        print("  %s already patched, skip" % label)
        return False

    # 1) 抬 .registers
    reg_line_start = body.index(".registers ")
    reg_line_end = body.index("\n", reg_line_start)
    cur = int(body[reg_line_start + len(".registers "):reg_line_end].strip())
    if cur < need_words:
        raise SystemExit(
            "ERROR: %s .registers=%d 不足以容纳 %d 个门控寄存器" % (label, cur, need_words)
        )
    body = (
        body[:reg_line_start]
        + ".registers %d" % (cur + need_words)
        + body[reg_line_end:]
    )

    # 2) 插入门控
    ann = body.index(".end annotation")
    ins = body.index("\n", ann) + 1
    body = body[:ins] + gate + body[ins:]

    s = s[:start] + body + s[end:]
    p.write_text(s, encoding="utf-8")
    print("  %s patched (registers %d -> %d, %s)" % (label, cur, cur + need_words, path.split("/")[-1]))
    return True


mgr = sys.argv[1]
impl = sys.argv[2]

inject(
    mgr,
    ".method public showRewardVideoAutoAd(Lcom/tapsdk/tapad/group/DirichletAdRequest;Lcom/tapsdk/tapad/group/DirichletAdNative$RewardVideoAutoAdListener;)V",
    GATE_MGR,
    "TapAdRewardManager.showRewardVideoAutoAd",
    5,
)

inject(
    impl,
    ".method public getRewardVideo(Lcom/sp/sdk/core/callback/RewardVideoCallback;)V",
    GATE_IMPL,
    "TapAdModImpl.getRewardVideo",
    4,
)
PY
# 幂等 + 门控自检（两层都要）
MGR_SMALI="$WORK/smali_c3/com/sp/mod/tapad/manager/TapAdRewardManager.smali"
IMPL_SMALI="$WORK/smali_c3/com/sp/mod/tapad/TapAdModImpl.smali"
grep -q 'GyzwHooks;->shouldSkipAd' "$MGR_SMALI"  || { echo "ERROR: 门控未写入 TapAdRewardManager" >&2; exit 1; }
grep -q ':gyzw_mgr_orig'          "$MGR_SMALI"  || { echo "ERROR: TapAdRewardManager label 缺失" >&2; exit 1; }
grep -q 'RewardVideoAutoAdListener;->onRewardVerify' "$MGR_SMALI" \
  || { echo "ERROR: 假 onRewardVerify 回调缺失" >&2; exit 1; }
grep -q 'GyzwHooks;->shouldSkipAd' "$IMPL_SMALI" || { echo "ERROR: 门控未写入 TapAdModImpl" >&2; exit 1; }
grep -q ':gyzw_impl_orig'         "$IMPL_SMALI" || { echo "ERROR: TapAdModImpl label 缺失" >&2; exit 1; }
echo "  两层门控已写入并自检通过"
# 只允许这一个类被改：其余 smali 文本必须与原 dex 反编译结果一致（防误伤）
"$SMALI_BIN" assemble -o "$WORK/${TARGET_DEX%.dex}.patched.dex" "$WORK/smali_c3"
set +e
"$BAKSMALI_BIN" list classes "$WORK/${TARGET_DEX%.dex}.patched.dex" | sed 's/^L//;s/;$//' | tr '/' '.' | sort > "$WORK/c3_new.txt"
unzip -p "$ORIG_APK" "$TARGET_DEX" > "$WORK/c3_orig.dex"
"$BAKSMALI_BIN" list classes "$WORK/c3_orig.dex" | sed 's/^L//;s/;$//' | tr '/' '.' | sort > "$WORK/c3_old.txt"
set -e
diff -q "$WORK/c3_old.txt" "$WORK/c3_new.txt" >/dev/null \
  || { echo "ERROR: $TARGET_DEX 类集合在回编译后发生变化" >&2; diff "$WORK/c3_old.txt" "$WORK/c3_new.txt" | head -20; exit 1; }
echo "  $TARGET_DEX 类集合一致（$(wc -l < "$WORK/c3_new.txt" | tr -d ' ') 类），已注入门控"
PATCHED_DEX="$WORK/${TARGET_DEX%.dex}.patched.dex"
ls -la "$PATCHED_DEX"

# ---------------------------------------------------------------- 3b/6
# ★classes4.dex：com/sp/common/SPBaseController.getRewardVideo —— 真正的广告分发入口
#   第二轮实测：只在 classes3 两层设门控，no-ad gate 命中 0 次，广告照弹。
#   日志「SpBaseController 获取广告----」证明游戏走的是 SPBaseController → ChannelManager
#   → TapSDK 这条链，而 SPBaseController 在 classes4（classes3 里根本没有这个类）。
echo "[3b/6] $TARGET_DEX2 免广告门控 → SPBaseController.getRewardVideo"
unzip -p "$ORIG_APK" "$TARGET_DEX2" > "$WORK/$TARGET_DEX2"
"$BAKSMALI_BIN" d -o "$WORK/smali_c4" "$WORK/$TARGET_DEX2" 2>/dev/null \
  || "$BAKSMALI_BIN" d -o "$WORK/smali_c4" "$WORK/$TARGET_DEX2"
python3.11 - "$WORK/smali_c4/com/sp/common/SPBaseController.smali" <<'PY'
import sys
from pathlib import Path

GATE = '''
    # ---- 星空免广告门控（SPBaseController，真分发入口）----
    invoke-static {}, Lcom/starry/magic/cocos/GyzwHooks;->shouldSkipAd()Z

    move-result v0

    # ★极性铁律：if-eqz = 「noAd 为 false 时跳回原逻辑」（对照 surgical_dwct_12.39.sh:193）。
    #   此前误写 if-nez，导致开启免广告反而走进真广告路径（实测 logcat 无 gate 行）。
    if-eqz v0, :gyzw_sp_orig

    const-string v0, "Gyzw"

    const-string v1, "no-ad gate: SPBaseController.getRewardVideo intercepted"

    invoke-static {v0, v1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    # 同形假回调：先 onResult(0,"视频观看完成") 再 onAdClose("广告页面已关闭")。
    # 这是唯一能让游戏侧判定「看完并发奖」的时序（对照 TapAdRewardVideo$2.onRewardVerify）。
    const/4 v0, 0x0

    const-string v1, "\\u89c6\\u9891\\u89c2\\u770b\\u5b8c\\u6210"

    invoke-virtual {p1, v0, v1}, Lcom/sp/sdk/core/callback/RewardVideoCallback;->onResult(ILjava/lang/String;)V

    const-string v1, "\\u5e7f\\u544a\\u9875\\u9762\\u5df2\\u5173\\u95ed"

    invoke-virtual {p1, v1}, Lcom/sp/sdk/core/callback/RewardVideoCallback;->onAdClose(Ljava/lang/String;)V

    return-void

    :gyzw_sp_orig
'''

p = Path(sys.argv[1])
s = p.read_text(encoding="utf-8")
sig = ".method public getRewardVideo(Lcom/sp/sdk/core/callback/RewardVideoCallback;)V"
start = s.index(sig)
end = s.index(".end method", start)
body = s[start:end]

if "GyzwHooks" in body:
    print("  SPBaseController already patched, skip")
    sys.exit(0)

# 抬 .registers（本方法原为 3；门控只用 v0/v1，与参数 p0=1,p1=2 不冲突）
rs = body.index(".registers ")
re_ = body.index("\n", rs)
cur = int(body[rs + len(".registers "):re_].strip())
body = body[:rs] + ".registers %d" % (cur + 2) + body[re_:]

ann = body.index(".end annotation")
ins = body.index("\n", ann) + 1
body = body[:ins] + GATE + body[ins:]

s = s[:start] + body + s[end:]
p.write_text(s, encoding="utf-8")
print("  SPBaseController.getRewardVideo patched (registers %d -> %d)" % (cur, cur + 2))
PY
SP_SMALI="$WORK/smali_c4/com/sp/common/SPBaseController.smali"
grep -q 'GyzwHooks;->shouldSkipAd' "$SP_SMALI" || { echo "ERROR: 门控未写入 SPBaseController" >&2; exit 1; }
grep -q ':gyzw_sp_orig'          "$SP_SMALI" || { echo "ERROR: SPBaseController label 缺失" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/${TARGET_DEX2%.dex}.patched.dex" "$WORK/smali_c4"
set +e
"$BAKSMALI_BIN" list classes "$WORK/${TARGET_DEX2%.dex}.patched.dex" | sed 's/^L//;s/;$//' | tr '/' '.' | sort > "$WORK/c4_new.txt"
"$BAKSMALI_BIN" list classes "$WORK/$TARGET_DEX2" | sed 's/^L//;s/;$//' | tr '/' '.' | sort > "$WORK/c4_old.txt"
set -e
diff -q "$WORK/c4_old.txt" "$WORK/c4_new.txt" >/dev/null \
  || { echo "ERROR: $TARGET_DEX2 类集合在回编译后发生变化" >&2; diff "$WORK/c4_old.txt" "$WORK/c4_new.txt" | head -20; exit 1; }
echo "  $TARGET_DEX2 类集合一致（$(wc -l < "$WORK/c4_new.txt" | tr -d ' ') 类），已注入门控"
PATCHED_DEX2="$WORK/${TARGET_DEX2%.dex}.patched.dex"
ls -la "$PATCHED_DEX2"

# ---------------------------------------------------------------- 4/6
echo "[4/6] AndroidManifest：分支 B（StarryInitProvider）+ MenuService + FGS 权限"
if [[ -n "${GYZW_BINARY_MANIFEST:-}" && -f "${GYZW_BINARY_MANIFEST:-}" ]]; then
  cp "$GYZW_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  from GYZW_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  # 586MB 原包直接 apktool 太慢：用只含 Manifest+arsc+classes.dex 的迷你 zip 做一次
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in keep:
    info = zin.getinfo(name); data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    full = "android.permission." + perm
    # FOREGROUND_SERVICE 是 FOREGROUND_SERVICE_SPECIAL_USE 的前缀，必须精确匹配
    if ('android:name="%s"' % full) not in t:
        perms.append('    <uses-permission android:name="%s"/>' % full)
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if "com.starry.magic.StarryInitProvider" not in t:
    prov = ('''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg)
    t = t.replace("</application>", prov + "\n    </application>", 1)
if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched (provider + service + perms)")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt"
  python3.11 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService"     "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "SYSTEM_ALERT_WINDOW"                   "$WORK/tree_new.txt" || { echo "ERROR: overlay perm missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE"        "$WORK/tree_new.txt" || { echo "ERROR: FGS perm missing" >&2; exit 1; }
  grep -q "com.sp.sdk.core.GameApplication"       "$WORK/tree_new.txt" || { echo "ERROR: 官方 Application 丢失" >&2; exit 1; }
  grep -q "androidx.core.app.CoreComponentFactory" "$WORK/tree_new.txt" || { echo "ERROR: appComponentFactory 丢失" >&2; exit 1; }
  grep -q "$PKG" "$WORK/tree_new.txt" || { echo "ERROR: 包名丢失" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt"  > "$WORK/n.norm"
  LOST="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$LOST" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $LOST 行" >&2; diff "$WORK/o.norm" "$WORK/n.norm" | grep '^<' | head -10; exit 1; }
  echo "  aapt2 校验通过（官方节点零丢失）"
fi

# ---------------------------------------------------------------- 5/6
echo "[5/6] zip surgical merge → $UNSIGNED"
python3.11 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" "$TARGET_DEX" "$TARGET_DEX2" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
new_dex, target_dex, target_dex2 = sys.argv[4], sys.argv[5], sys.argv[6]

mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
cpatched = (work / (target_dex[:-4] + ".patched.dex")).read_bytes()
cpatched2 = (work / (target_dex2[:-4] + ".patched.dex")).read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

UNTOUCHED = [
    "classes.dex", "classes2.dex", "resources.arsc",
    "lib/arm64-v8a/libcocos2djs.so",
]
SIG_EXT = (".SF", ".RSA", ".DSA", ".EC")
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names
assert new_dex not in names, f"{new_dex} already in orig"
assert target_dex in names
assert target_dex2 in names, f"orig apk missing {target_dex2}"
for n in UNTOUCHED:
    assert n in names, f"orig apk missing {n}"

def is_sig(name):
    if name == "META-INF/MANIFEST.MF": return True
    if not name.startswith("META-INF/"): return False
    return name.upper().endswith(SIG_EXT)

kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
n_written = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras:
        continue
    if name == "AndroidManifest.xml":
        data = mf
        ct = zipfile.ZIP_DEFLATED
    elif name == target_dex:
        data = cpatched
        ct = item.compress_type
    elif name == target_dex2:
        data = cpatched2
        ct = item.compress_type
    else:
        data = zin.read(item)
        ct = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = ct
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
for name, data in [(new_dex, cnew)] + sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
zin.close(); zout.close()

zi2, zo2 = zipfile.ZipFile(src, "r"), zipfile.ZipFile(dst, "r")
for name in UNTOUCHED:
    a = hashlib.sha256(zi2.read(name)).hexdigest()
    b = hashlib.sha256(zo2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
out_names = set(zo2.namelist())
for name in kept_meta:
    assert name in out_names, f"lost {name}"
assert new_dex in out_names and target_dex in out_names and target_dex2 in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert "assets/certs/starlight.crt" in out_names
assert not any(x.upper().endswith(SIG_EXT) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
print(f"  merged ok; entries={n_written}; {new_dex}={len(cnew)}; {target_dex}={len(cpatched)}; {target_dex2}={len(cpatched2)}; untouched {len(UNTOUCHED)} verified")
zi2.close(); zo2.close()
PY

PKG_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$PKG_SO_MD5" == "$SO_MD5" ]] || { echo "ERROR: 包内 so md5=$PKG_SO_MD5 != $SO_MD5" >&2; exit 1; }
unzip -Z1 "$UNSIGNED" > "$WORK/entries.txt"
for e in "$NEW_DEX" "$TARGET_DEX" "lib/arm64-v8a/libstarry_magic.so" "assets/certs/starlight.crt" "classes.dex" "classes2.dex" "classes4.dex"; do
  grep -q "^$e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done
echo "  包内 so md5 ok；$TARGET_DEX + $NEW_DEX 到位"

# ---------------------------------------------------------------- 6/6
echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
  KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
  KS_PASS="${STARRY_KS_PASS:-starry2026}"
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
else
  java -jar "$SIGNER" -a "$UNSIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "${STARRY_KS_ALIAS:-starry}" \
    --ksPass "${STARRY_KS_PASS:-starry2026}" --ksKeyPass "${STARRY_KS_PASS:-starry2026}"
  CAND="$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)"
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
[[ -f "$SIGNED" ]] || { echo "ERROR: signing failed" >&2; exit 1; }
ls -lh "$SIGNED"
echo "signed_md5=$(md5 -q "$SIGNED")"
echo "signed_sha256=$(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"

echo "DONE: $SIGNED"
