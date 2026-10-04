#!/usr/bin/env bash
# 我嘎嘎乱杀 1.3.6 — 官方原包外科组装（禁止 apktool b 整包游戏）
#
# - classes4.dex = com/starry（原生菜单）
# - classes3.dex = TapSignKit + ClientLoginUtils + UnityPlayerActivity 生命周期
# - classes2.dex = ConsentLauncher 生命周期
# - 保留 classes.dex / resources.arsc 字节（Unity 不在此）
#
# 产物：08_output/我嘎嘎乱杀/我嘎嘎乱杀_1.3.6_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_wggls_1.3.6.sh
#   STARRY_INSTALL=1 bash scripts/surgical_wggls_1.3.6.sh
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
PKG="com.sixag.gaga"
ORIG_APK="${WGGLS_BASE_APK:-$NIXIANG/02_apks/我嘎嘎乱杀/official_v1.3.6.apk}"
OUT="$NIXIANG/08_output/我嘎嘎乱杀"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
SIGNED="$OUT/我嘎嘎乱杀_1.3.6_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/我嘎嘎乱杀_1.3.6_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/我嘎嘎乱杀/surgical_1.3.6/AndroidManifest.xml"
# 官方 Signer MD5（apksigner verify --print-certs）
ORIG_CERT_MD5="ec6d336b738a1adf899494fc5ce6918b"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools zipalign missing"; exit 1; }
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_WGGLS' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_WGGLS"; exit 1; }
grep -E 'Games/WgglsMod\.cpp' "$MK" \
  || { echo "ERROR: Android.mk must compile WgglsMod.cpp"; exit 1; }
echo "[check] Android.mk target=wggls"

LOCK_DIR="/tmp/wggls_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wggls_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Wggls\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 WGGLS"
  strings "$SO_BUILT" | grep -E 'Wggls|wggls|Match2|\[Match2\]|xxh2' | sort -u | head -20
  exit 1
fi
if grep -a -q '\[Match2\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款 Match2 标签"; exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Wggls])"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q '\[Wggls\]' "$WORK/check.so" || { echo "ERROR: gradle APK so 不是 WGGLS"; exit 1; }

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] classes4.dex = com/starry"
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] || { echo "ERROR: StarryApp.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] || { echo "ERROR: MenuService.smali missing"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] patch classes3: TapSignKit + ClientLoginUtils + Unity lifecycle"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
python3.11 - "$WORK/c3smali" "$ORIG_CERT_MD5" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5 = sys.argv[2]

tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not tap.is_file():
    raise SystemExit(f"missing {tap}")
ts = tap.read_text(encoding="utf-8")
GETSIGN = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;

    # starry: official APK cert MD5
    const-string v0, "{md5}"

    return-object v0
.end method"""
ts2, n = re.subn(
    r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
    GETSIGN, ts, count=1,
)
if n != 1:
    # try without final
    GETSIGN2 = GETSIGN.replace("public final getSign", "public getSign")
    ts2, n = re.subn(
        r"\.method public[^\n]*getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        GETSIGN2, ts, count=1,
    )
if n != 1:
    raise SystemExit("TapSignKit.getSign patch failed")
tap.write_text(ts2, encoding="utf-8")
print(f"  TapSignKit.getSign → {md5}")

clu = root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
if not clu.is_file():
    raise SystemExit(f"missing {clu}")
ct = clu.read_text(encoding="utf-8")
if "force WebLogin" not in ct:
    FORCE = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1
    .param p1    # Landroid/app/Activity;
    .param p2    # Lcom/taptap/sdk/login/impl/data/model/Actions;

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
    ct2, n = re.subn(
        r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?\.end method",
        FORCE, ct, count=1,
    )
    if n != 1:
        raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
    clu.write_text(ct2, encoding="utf-8")
    print("  ClientLoginUtils.isClientLogin → false (force WebLogin)")
else:
    print("  ClientLoginUtils already forced")

# UnityPlayerActivity 在本包 classes3.dex（非 classes2）
unity = root / "com/unity3d/player/UnityPlayerActivity.smali"
if not unity.is_file():
    raise SystemExit("UnityPlayerActivity.smali missing in classes3")
us = unity.read_text(encoding="utf-8")
uchanged = False
if "MenuService;->notifyHostLeaving" not in us:
    us += """
.method protected onUserLeaveHint()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V
    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
"""
    uchanged = True
    print("  +UnityPlayerActivity.onUserLeaveHint (classes3)")
if "MenuService;->notifyHostResumed" not in us:
    m2 = re.search(r"(\.method[^\n]*\bonResume\(\)V\n)([\s\S]*?)(\n\.end method)", us)
    if m2 and "notifyHostResumed" not in m2.group(0):
        head2, body_r, end2 = m2.group(1), m2.group(2), m2.group(3)
        rets = list(re.finditer(r"(\n\s*)return-void\b", body_r))
        if not rets:
            raise SystemExit("UnityPlayerActivity.onResume: no return-void")
        mm = rets[-1]
        body_r = (
            body_r[: mm.start()]
            + mm.group(1)
            + "invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V"
            + mm.group(0)
            + body_r[mm.end() :]
        )
        us = us[: m2.start()] + head2 + body_r + end2 + us[m2.end() :]
        uchanged = True
        print("  UnityPlayerActivity.onResume + notifyHostResumed (classes3)")
    elif not m2:
        us += """
.method protected onResume()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onResume()V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
    return-void
.end method
"""
        uchanged = True
        print("  +UnityPlayerActivity.onResume (classes3)")
if uchanged:
    unity.write_text(us, encoding="utf-8")
else:
    print("  UnityPlayerActivity lifecycle already present")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"

echo "[5/7] patch classes2: ConsentLauncher + SecurityBridge signing spoof"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
# 官方证书 SHA-256（大写，与 SecurityBridge.bytesToHexSha256 一致）
ORIG_CERT_SHA256="27B92798FCE9D962FB5DA25234D7EE60596B6FDE2BBAE25CA47BA3B6F5BAAB69"
python3.11 - "$WORK/c2smali" "$ORIG_CERT_SHA256" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
sha = sys.argv[2]

# API36+ SigningInfo 返回拷贝，SignSpoof 改数组无效 → 直接钉死官方 SHA256
sb = root / "com/game/security/SecurityBridge.smali"
if not sb.is_file():
    raise SystemExit("SecurityBridge.smali missing in classes2")
st = sb.read_text(encoding="utf-8")
FORCE = f""".method public static getSigningCertSha256()Ljava/lang/String;
    .locals 1

    # starry: force official signing cert sha256 (SEC-GATE-01)
    const-string v0, "{sha}"

    return-object v0
.end method"""
st2, n = re.subn(
    r"\.method public static getSigningCertSha256\(\)Ljava/lang/String;[\s\S]*?\.end method",
    FORCE, st, count=1,
)
if n != 1:
    raise SystemExit("SecurityBridge.getSigningCertSha256 patch failed")
sb.write_text(st2, encoding="utf-8")
print(f"  SecurityBridge.getSigningCertSha256 → {sha}")

def strip_legacy(body: str) -> str:
    body = re.sub(
        r"\n\s*const-string v\d+, \"starry_magic\"\s*\n\s*invoke-static \{v\d+\}, Ljava/lang/System;->loadLibrary\(Ljava/lang/String;\)V\s*\n",
        "\n", body,
    )
    body = re.sub(
        r"\n\s*invoke-static \{p0\}, Lcom/starry/magic/menu/MenuService;->start\(Landroid/content/Context;\)V\s*\n",
        "\n", body,
    )
    return body

def inject(rel: str, tag: str, super_onleave="Landroid/app/Activity;", super_onresume="Landroid/app/Activity;"):
    path = root / rel
    if not path.is_file():
        print(f"  skip {tag} missing")
        return
    s = path.read_text(encoding="utf-8")
    changed = False
    m = re.search(r"(\.method[^\n]*\bonCreate\(Landroid/os/Bundle;\)V\n)([\s\S]*?)(\n\.end method)", s)
    if m:
        head, body, end = m.group(1), m.group(2), m.group(3)
        body2 = strip_legacy(body)
        if body2 != body:
            s = s[: m.start()] + head + body2 + end + s[m.end() :]
            changed = True
            print(f"  {tag}.onCreate strip legacy")
    if "MenuService;->notifyHostLeaving" not in s:
        s += f"""
.method protected onUserLeaveHint()V
    .locals 0
    invoke-super {{p0}}, {super_onleave}->onUserLeaveHint()V
    invoke-static {{}}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
"""
        changed = True
        print(f"  +{tag}.onUserLeaveHint")
    if "MenuService;->notifyHostResumed" not in s:
        if not re.search(r"\.method[^\n]*\bonResume\(\)V", s):
            s += f"""
.method protected onResume()V
    .locals 0
    invoke-super {{p0}}, {super_onresume}->onResume()V
    invoke-static {{p0}}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
    return-void
.end method
"""
            changed = True
            print(f"  +{tag}.onResume")
        else:
            m2 = re.search(r"(\.method[^\n]*\bonResume\(\)V\n)([\s\S]*?)(\n\.end method)", s)
            if m2 and "notifyHostResumed" not in m2.group(0):
                head2, body_r, end2 = m2.group(1), m2.group(2), m2.group(3)
                rets = list(re.finditer(r"(\n\s*)return-void\b", body_r))
                if rets:
                    mm = rets[-1]
                    body_r = (
                        body_r[: mm.start()]
                        + mm.group(1)
                        + "invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V"
                        + mm.group(0)
                        + body_r[mm.end() :]
                    )
                    s = s[: m2.start()] + head2 + body_r + end2 + s[m2.end() :]
                    changed = True
                    print(f"  {tag}.onResume + notifyHostResumed")
    if changed:
        path.write_text(s, encoding="utf-8")

inject("com/game/consent/ConsentLauncherActivity.smali", "ConsentLauncher")
inject("com/unity3d/player/UnityPlayerActivity.smali", "UnityPlayerActivity")
print("  classes2 lifecycle done")
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"

# UnityPlayerActivity often in classes.dex — patch if needed
if ! unzip -l "$ORIG_APK" | grep -q 'classes.dex'; then
  :
fi
echo "[5b/7] optional UnityPlayerActivity in classes.dex"
unzip -p "$ORIG_APK" classes.dex > "$WORK/classes_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c1smali" "$WORK/classes_orig.dex"
if [[ -f "$WORK/c1smali/com/unity3d/player/UnityPlayerActivity.smali" ]]; then
  python3.11 - "$WORK/c1smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
path = root / "com/unity3d/player/UnityPlayerActivity.smali"
s = path.read_text(encoding="utf-8")
changed = False
if "MenuService;->notifyHostLeaving" not in s:
    s += """
.method protected onUserLeaveHint()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V
    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
"""
    changed = True
if "MenuService;->notifyHostResumed" not in s:
    if not re.search(r"\.method[^\n]*\bonResume\(\)V", s):
        s += """
.method protected onResume()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onResume()V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
    return-void
.end method
"""
        changed = True
    else:
        m2 = re.search(r"(\.method[^\n]*\bonResume\(\)V\n)([\s\S]*?)(\n\.end method)", s)
        if m2 and "notifyHostResumed" not in m2.group(0):
            head2, body_r, end2 = m2.group(1), m2.group(2), m2.group(3)
            rets = list(re.finditer(r"(\n\s*)return-void\b", body_r))
            if rets:
                mm = rets[-1]
                body_r = (
                    body_r[: mm.start()]
                    + mm.group(1)
                    + "invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V"
                    + mm.group(0)
                    + body_r[mm.end() :]
                )
                s = s[: m2.start()] + head2 + body_r + end2 + s[m2.end() :]
                changed = True
if changed:
    path.write_text(s, encoding="utf-8")
    print("  classes.dex UnityPlayerActivity patched")
else:
    print("  classes.dex UnityPlayerActivity unchanged/skip")
PY
  "$SMALI_BIN" assemble -o "$WORK/classes.dex" "$WORK/c1smali"
  PATCH_CLASSES1=1
else
  echo "  UnityPlayerActivity not in classes.dex"
  PATCH_CLASSES1=0
fi

echo "[6/7] AndroidManifest (StarryApp + MenuService)"
if [[ -f "$MF_CACHE" ]]; then
  cp -f "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached Manifest"
else
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if "<application" in t and 'android:name="com.starry.magic.StarryApp"' not in t:
    t = t.replace("<application", '<application android:name="com.starry.magic.StarryApp"', 1)
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
print("  text Manifest patched")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi

echo "[7/7] zip merge + sign"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null 2>&1 || true
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$WORK/classes2.dex" "$WORK/classes3.dex" "$WORK/classes4.dex" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml classes2.dex classes3.dex classes4.dex lib/arm64-v8a/libstarry_magic.so)
[[ -f "$STAGE/lib/arm64-v8a/libc++_shared.so" ]] && ZIP_ARGS+=(lib/arm64-v8a/libc++_shared.so)
if [[ "${PATCH_CLASSES1:-0}" == "1" ]]; then
  cp -f "$WORK/classes.dex" "$STAGE/"
  ZIP_ARGS+=(classes.dex)
fi
if [[ -d "$WORK/assets/certs" ]]; then
  mkdir -p "$STAGE/assets/certs"
  cp -R "$WORK/assets/certs/." "$STAGE/assets/certs/"
  while IFS= read -r -d '' f; do
    ZIP_ARGS+=("${f#"$STAGE/"}")
  done < <(find "$STAGE/assets/certs" -type f -print0)
fi
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" "${ZIP_ARGS[@]}")

python3.11 - "$WORK/unsigned.apk" "$ORIG_APK" "${PATCH_CLASSES1:-0}" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
dst, src = Path(sys.argv[1]), Path(sys.argv[2])
patched1 = sys.argv[3] == "1"
z = zipfile.ZipFile(dst)
names = set(z.namelist())
assert "lib/arm64-v8a/libstarry_magic.so" in names
assert "classes4.dex" in names
zo = zipfile.ZipFile(src)
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
if not patched1:
    assert hashlib.sha256(zo.read("classes.dex")).hexdigest() == hashlib.sha256(z.read("classes.dex")).hexdigest()
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Wggls]" in so
c3 = z.read("classes3.dex")
assert b"ec6d336b738a1adf899494fc5ce6918b" in c3
print("  merged ok; arsc intact; [Wggls] so; TapSign MD5 ok")
PY

ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -8 || true
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] 尝试覆盖安装（签名与官方不同时会失败，需用户同意卸载）"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  REMOTE=/data/local/tmp/starry_wggls_surgical.apk
  adb push "$SIGNED" "$REMOTE"
  ok=0
  if adb install -r -d "$SIGNED" 2>/dev/null; then
    echo "[install] adb install -r -d ok"; ok=1
  elif adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] su pm install ok"; ok=1
  fi
  adb shell "rm -f '$REMOTE'" 2>/dev/null || true
  if [[ "$ok" != "1" ]]; then
    echo "ERROR: 覆盖安装失败（官方签 ↔ 星光签不一致）。请在聊天确认「可以卸载」后再装。"
    exit 1
  fi
  _NIXIANG="$(cd "$SCRIPT_DIR/../../.." && pwd)"
  # shellcheck source=/dev/null
  source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
  starry_post_install "$PKG"
  echo "log: adb logcat -s StarryApp:V StarryMagic:V StarryTrace:V Mod_Menu:V"
fi

echo "DONE: $SIGNED"
