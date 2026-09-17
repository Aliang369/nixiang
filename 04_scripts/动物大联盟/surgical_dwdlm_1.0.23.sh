#!/usr/bin/env bash
# 动物大联盟 1.0.23 — 官方原包外科组装。
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
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
ORIG_APK="${DWDLM_BASE_APK:-$NIXIANG/02_apks/动物大联盟/动物大联盟_1.0.23_official.apk}"
ORIG_MD5="${DWDLM_BASE_MD5:-211cfebb2d273360e22ea3593fdaedd6}"
OUT_DIR="$NIXIANG/07_patched/动物大联盟"
UNSIGNED="$OUT_DIR/动物大联盟_1.0.23_starry-unsigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.fightingstars.games.animal"
SO_PREBUILT="${DWDLM_SO:-$STARRY_SRC/libs_dwdlm/arm64-v8a/libstarry_magic.so}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing starry so $SO_PREBUILT" >&2; exit 1; }
[[ -f "$NDK_CXX" ]] || { echo "ERROR: missing libc++_shared at $NDK_CXX" >&2; exit 1; }
ACTUAL_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: base APK fingerprint mismatch: $ACTUAL_MD5" >&2; exit 1; }

LOCK="/tmp/dwdlm_surgical.lockdir"
if ! mkdir "$LOCK" 2>/dev/null; then echo "ERROR: lock held: $LOCK" >&2; exit 1; fi
echo $$ > "$LOCK/pid"
WORK="$(mktemp -d /tmp/dwdlm_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK"' EXIT
mkdir -p "$OUT_DIR" "$WORK"

echo "[1/6] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_dwdlm/' --exclude 'libs_dwdlm/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: starry apk missing" >&2; exit 1; }

echo "[2/6] collect starry smali -> classes2.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"; mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] || { echo "ERROR: StarryApp missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/cocos/DwdlmHooks.smali" ]] || { echo "ERROR: DwdlmHooks missing from Gradle build" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -lh "$WORK/classes2.dex"

echo "[3/6] create Manifest patch"
apktool d -f -s -o "$WORK/base_dec" "$ORIG_APK" >/dev/null
python3 - "$WORK/base_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding='utf-8')
if 'com.starry.magic.dwdlm.DwdlmApplication' not in t:
    t = t.replace('android:name="dev.mars.secure.ProxyApplication"', 'android:name="com.starry.magic.dwdlm.DwdlmApplication"', 1)
perms = []
for perm in [
    'android.permission.SYSTEM_ALERT_WINDOW',
    'android.permission.FOREGROUND_SERVICE',
    'android.permission.FOREGROUND_SERVICE_SPECIAL_USE',
]:
    if f'android:name="{perm}"' not in t:
        perms.append(f'    <uses-permission android:name="{perm}"/>')
if perms:
    i = t.find('<uses-permission')
    if i < 0:
        raise SystemExit('no uses-permission node')
    t = t[:i] + '\n'.join(perms) + '\n' + t[i:]
if 'com.starry.magic.menu.MenuService' not in t:
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
    t = t.replace('</application>', svc + '\n    </application>', 1)
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.fightingstars.games.animal.starryinit"
            android:exported="false"
            android:initOrder="100" />
'''
    t = t.replace('</application>', prov + '\n    </application>', 1)
p.write_text(t, encoding='utf-8')
print('manifest patched: provider + MenuService')
PY
rm -rf "$WORK/base_dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/base_dec" -o "$WORK/manifest_only.apk" >/dev/null
unzip -p "$WORK/manifest_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[3.5/6] patch assets/main.js bootstrap"
python3 - "$ORIG_APK" "$WORK" "$SCRIPT_DIR/patch_dwdlm.js" <<'PY'
import json, os, sys, zipfile
from pathlib import Path
orig, work, patch_path = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
with zipfile.ZipFile(orig, 'r') as z:
    main_js = z.read('assets/main.js').decode('utf-8')
patch_src = patch_path.read_text(encoding='utf-8')
wrapper = r'''

;(function () {
  function log(m) {
    fileLog(m);
    try { console.log('[dwdlm] ' + m); } catch (e) {}
    try { jsb.reflection.callStaticMethod('android/util/Log', 'i', '(Ljava/lang/String;Ljava/lang/String;)I', 'dwdlm', String(m)); } catch (e) {}
    try { jsb.reflection.callStaticMethod('com.starry.magic.cocos.DwdlmHooks', 'logStatus', '(Ljava/lang/String;)V', String(m)); } catch (e) {}
  }
  function fileLog(m) {
    try {
      var p = jsb.fileUtils.getWritablePath() + 'dwdlm_boot.log';
      var old = jsb.fileUtils.getStringFromFile(p) || '';
      jsb.fileUtils.writeStringToFile(old + '[dwdlm] ' + m + '\n', p);
    } catch (e) {}
  }
  var PATCH = __PATCH__;
  log('bootstrap wrapper loaded');
  function install() {
    fileLog('install attempt');
    if (!window.cc || !cc.assetManager || cc.assetManager.__dwdlmWrapped) return false;
    var am = cc.assetManager;
    am.__dwdlmWrapped = true;
    var orig = am.loadBundle;
    am.loadBundle = function (name, options, cb) {
      if (name !== 'scripts') return orig.apply(am, arguments);
      log('scripts bundle request');
      var bases = ['/data/user/0/com.fightingstars.games.animal/files/res/assets', '/data/user/0/com.fightingstars.games.animal/files/res', ''];
      var done = function (err, bundle) {
        log('scripts bundle result ' + (err ? (err.message || err) : 'ok'));
        if (!err) {
          try { (0, eval)(PATCH); log('feature patch applied'); } catch (e) { log('feature patch error ' + e); }
        }
        if (typeof cb === 'function') cb(err, bundle);
      };
      var attempt = function (i) {
        if (i >= bases.length) return done(new Error('all bundle bases failed'));
        var opts = bases[i] ? { base: bases[i] } : undefined;
        try {
          return orig.call(am, name, opts, function (err, bundle) {
            if (err && i + 1 < bases.length) {
              log('bundle base failed: ' + bases[i] + ' -> ' + (err.message || err));
              return attempt(i + 1);
            }
            done(err, bundle);
          });
        } catch (e) {
          log('bundle call failed: ' + e);
          return attempt(i + 1);
        }
      };
      return attempt(0);
    };
    log('loadBundle wrapper installed');
    return true;
  }
  if (!install()) {
    var t = setInterval(function () { if (install()) clearInterval(t); }, 50);
  }
})();
'''
wrapper = wrapper.replace('__PATCH__', json.dumps(patch_src))
if os.environ.get('DWDLM_SKIP_MAIN_JS_PATCH') == '1':
    (work / 'main.js.patched').write_text(main_js, encoding='utf-8')
    print('assets/main.js pristine (skip patch)')
else:
    wrapper = wrapper.replace('__PATCH__', json.dumps(patch_src))
    (work / 'main.js.patched').write_text(main_js + wrapper, encoding='utf-8')
    print('assets/main.js bootstrap wrapper prepared')
PY

echo "[4/6] surgical zip merge"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
src = Path(sys.argv[1]); work = Path(sys.argv[2]); dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes(); cxx = Path(sys.argv[5]).read_bytes()
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes2 = (work / 'classes2.dex').read_bytes()
main_js = (work / 'main.js.patched').read_bytes()
replace = {'AndroidManifest.xml': manifest, 'assets/main.js': main_js}
extras = {
    'classes2.dex': classes2,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name == 'META-INF/MANIFEST.MF' or (name.startswith('META-INF/') and name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if name in extras:
            continue
        data = replace.get(name)
        if data is None:
            data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == 'resources.arsc' else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
    for name, data in extras.items():
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name.startswith('lib/') and name.endswith('.so') else zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    for n in original_names:
        if n == 'META-INF/MANIFEST.MF' or (n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))) or n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
    for n in extras:
        if n not in out_names:
            raise SystemExit(f'missing extra: {n}')
    if hashlib.sha256(zout.read('lib/arm64-v8a/libstarry_magic.so')).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print('surgical zip verified: official entries unchanged + classes2 + starry so')
PY

echo "[5/6] unsigned output -> $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"

echo "[6/6] done"
