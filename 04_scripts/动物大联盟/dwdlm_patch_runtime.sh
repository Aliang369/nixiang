#!/usr/bin/env bash
# 动物大联盟 1.0.23 — 运行态 main bundle 补丁。
# 解密 files/res/assets/main/index.jsc，注入 loadBundle 包装 + 六功能补丁，
# 重新加密后覆盖回设备（不改 APK，不触发重装）。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
SERIAL="${DWDLM_SERIAL:-$(cat "$SCRIPT_DIR/.device_serial" 2>/dev/null || true)}"
PKG="com.fightingstars.games.animal"
MAIN_DEC="$NIXIANG/06_dumped/动物大联盟/runtime_1.0.23/decrypted_js/assets/main/index.js"
BOOT_SRC="$NIXIANG/06_dumped/动物大联盟/runtime_1.0.23/files_res/main.js"
VERSION_SRC="$NIXIANG/06_dumped/动物大联盟/runtime_1.0.23/files_res/version"
PATCH_JS="$SCRIPT_DIR/patch_dwdlm.js"
OUT_DIR="$NIXIANG/06_dumped/动物大联盟/runtime_1.0.23/patched"
OUT_JSC="$OUT_DIR/assets/main/index.jsc"
OUT_BOOT="$OUT_DIR/main.js"
STATE_JSON="$SCRIPT_DIR/runtime_patch.json"
DEVICE_JSC="/data/user/0/$PKG/files/res/assets/main/index.jsc"
DEVICE_BOOT="/data/user/0/$PKG/files/res/main.js"
DEVICE_VERSION="/data/user/0/$PKG/files/res/version"
TOOL="$SCRIPT_DIR/dwdlm_jsc_tool.py"

[[ -n "$SERIAL" ]] || { echo "ERROR: missing device serial" >&2; exit 1; }
[[ -f "$MAIN_DEC" ]] || { echo "ERROR: missing $MAIN_DEC" >&2; exit 1; }
[[ -f "$BOOT_SRC" ]] || { echo "ERROR: missing $BOOT_SRC" >&2; exit 1; }
[[ -f "$VERSION_SRC" ]] || { echo "ERROR: missing $VERSION_SRC" >&2; exit 1; }
[[ -f "$PATCH_JS" ]] || { echo "ERROR: missing $PATCH_JS" >&2; exit 1; }
[[ -f "$TOOL" ]] || { echo "ERROR: missing $TOOL" >&2; exit 1; }

WORK="$(mktemp -d /tmp/dwdlm_runtime_patch.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT_DIR/assets/main"

python3 - "$MAIN_DEC" "$PATCH_JS" "$WORK/main.patched.js" "$WORK/wrapper.js" <<'PY'
import json
import sys
from pathlib import Path

main_src = Path(sys.argv[1]).read_text(encoding="utf-8")
patch_src = Path(sys.argv[2]).read_text(encoding="utf-8")
out = Path(sys.argv[3])
wrapper_out = Path(sys.argv[4])

wrapper = r'''
;(function () {
  function fileLog(m) {
    try {
      var p = jsb.fileUtils.getWritablePath() + 'dwdlm_boot.log';
      var old = jsb.fileUtils.getStringFromFile(p) || '';
      jsb.fileUtils.writeStringToFile(old + '[dwdlm] ' + m + '\n', p);
    } catch (e) {}
  }
  function log(m) {
    fileLog(m);
    try { console.log('[dwdlm] ' + m); } catch (e) {}
    try { jsb.reflection.callStaticMethod('android/util/Log', 'i', '(Ljava/lang/String;Ljava/lang/String;)I', 'dwdlm', String(m)); } catch (e) {}
  }
  var PATCH = __PATCH__;
  function install() {
    if (!window.cc || !cc.assetManager || cc.assetManager.__dwdlmWrapped) return false;
    var am = cc.assetManager;
    am.__dwdlmWrapped = true;
    var orig = am.loadBundle;
    am.loadBundle = function (name, options, cb) {
      if (name !== 'scripts') return orig.apply(am, arguments);
      log('scripts bundle request');
      var bases = [
        '/data/user/0/com.fightingstars.games.animal/files/res/assets',
        '/data/user/0/com.fightingstars.games.animal/files/res',
        ''
      ];
      var done = function (err, bundle) {
        log('scripts bundle result ' + (err ? (err.message || err) : 'ok'));
        if (!err) {
          try { (0, eval)(PATCH); log('feature patch applied'); }
          catch (e) { log('feature patch error ' + e); }
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
  log('main bundle wrapper loaded');
  if (!install()) {
    var t = setInterval(function () { if (install()) clearInterval(t); }, 50);
  }
})();
'''
wrapper_text = wrapper.replace("__PATCH__", json.dumps(patch_src))
out.write_text(main_src + wrapper_text, encoding="utf-8")
wrapper_out.write_text(wrapper_text, encoding="utf-8")
PY

python3 "$TOOL" decrypt-ze "$BOOT_SRC" "$WORK/main.bootstrap.js"
python3 - "$WORK/main.bootstrap.js" "$WORK/wrapper.js" "$WORK/main.bootstrap.patched.js" <<'PY'
import sys
from pathlib import Path
boot = Path(sys.argv[1]).read_text(encoding="utf-8")
wrapper = Path(sys.argv[2]).read_text(encoding="utf-8")
Path(sys.argv[3]).write_text(boot + "\n" + wrapper, encoding="utf-8")
PY
python3 "$TOOL" encrypt-ze "$WORK/main.bootstrap.patched.js" "$OUT_BOOT"

python3 "$TOOL" encrypt "$WORK/main.patched.js" "$OUT_JSC"

adb -s "$SERIAL" push "$OUT_JSC" /data/local/tmp/dwdlm_main_index.jsc >/dev/null
adb -s "$SERIAL" shell "su -c 'cp /data/local/tmp/dwdlm_main_index.jsc $DEVICE_JSC && chown u0_a723:u0_a723 $DEVICE_JSC && chmod 600 $DEVICE_JSC'"
adb -s "$SERIAL" shell "rm -f /data/local/tmp/dwdlm_main_index.jsc" >/dev/null

adb -s "$SERIAL" push "$OUT_BOOT" /data/local/tmp/dwdlm_main_boot.js >/dev/null
adb -s "$SERIAL" shell "su -c 'cp /data/local/tmp/dwdlm_main_boot.js $DEVICE_BOOT && chown u0_a723:u0_a723 $DEVICE_BOOT && chmod 600 $DEVICE_BOOT'"
adb -s "$SERIAL" shell "rm -f /data/local/tmp/dwdlm_main_boot.js" >/dev/null

adb -s "$SERIAL" push "$VERSION_SRC" /data/local/tmp/dwdlm_version.json >/dev/null
adb -s "$SERIAL" shell "su -c 'cp /data/local/tmp/dwdlm_version.json $DEVICE_VERSION && chown u0_a723:u0_a723 $DEVICE_VERSION && chmod 700 $DEVICE_VERSION'"
adb -s "$SERIAL" shell "rm -f /data/local/tmp/dwdlm_version.json" >/dev/null

LOCAL_MD5="$(md5 -q "$OUT_JSC")"
DEVICE_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEVICE_JSC'" | awk '{print $1}' | tr -d '\r')"
[[ "$LOCAL_MD5" == "$DEVICE_MD5" ]] || {
  echo "ERROR: device jsc md5 mismatch: local=$LOCAL_MD5 device=$DEVICE_MD5" >&2
  exit 1
}

LOCAL_BOOT_MD5="$(md5 -q "$OUT_BOOT")"
DEVICE_BOOT_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEVICE_BOOT'" | awk '{print $1}' | tr -d '\r')"
[[ "$LOCAL_BOOT_MD5" == "$DEVICE_BOOT_MD5" ]] || {
  echo "ERROR: device boot md5 mismatch: local=$LOCAL_BOOT_MD5 device=$DEVICE_BOOT_MD5" >&2
  exit 1
}
LOCAL_VERSION_MD5="$(md5 -q "$VERSION_SRC")"
DEVICE_VERSION_MD5="$(adb -s "$SERIAL" shell "su -c 'md5sum $DEVICE_VERSION'" | awk '{print $1}' | tr -d '\r')"
[[ "$LOCAL_VERSION_MD5" == "$DEVICE_VERSION_MD5" ]] || {
  echo "ERROR: device version md5 mismatch: local=$LOCAL_VERSION_MD5 device=$DEVICE_VERSION_MD5" >&2
  exit 1
}
python3 - "$STATE_JSON" "$SERIAL" "$OUT_JSC" "$LOCAL_MD5" "$DEVICE_JSC" "$OUT_BOOT" "$LOCAL_BOOT_MD5" "$DEVICE_BOOT" <<'PY'

import json, sys
from pathlib import Path
Path(sys.argv[1]).write_text(json.dumps({
    "serial": sys.argv[2],
    "local_jsc": sys.argv[3],
    "md5": sys.argv[4],
    "device_jsc": sys.argv[5],
    "local_boot": sys.argv[6],
    "boot_md5": sys.argv[7],
    "device_boot": sys.argv[8],
}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

echo "runtime main bundle patched: $OUT_JSC"
echo "device: $DEVICE_JSC md5=$DEVICE_MD5"
echo "device boot: $DEVICE_BOOT md5=$DEVICE_BOOT_MD5"
