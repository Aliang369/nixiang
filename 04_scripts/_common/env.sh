# nixiang shared env — source from other _common scripts
# shellcheck shell=bash

NIXIANG_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
export NIXIANG_ROOT

# Prefer brew python3.11 for RE libs (pycryptodome/xxtea/keystone/yara)
if [[ -x /opt/homebrew/bin/python3.11 ]]; then
  export NIXIANG_PYTHON=/opt/homebrew/bin/python3.11
else
  export NIXIANG_PYTHON="$(command -v python3)"
fi

# Android SDK build-tools (zipalign / apksigner) — not always on PATH
_resolve_build_tools() {
  local sdk="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-$HOME/Library/Android/sdk}}"
  local preferred=(36.0.0 35.0.0 34.0.0)
  local v dir
  if [[ ! -d "$sdk/build-tools" ]]; then
    return 1
  fi
  for v in "${preferred[@]}"; do
    dir="$sdk/build-tools/$v"
    if [[ -x "$dir/zipalign" && -x "$dir/apksigner" ]]; then
      echo "$dir"
      return 0
    fi
  done
  # newest directory name
  dir="$(ls -1 "$sdk/build-tools" 2>/dev/null | sort -V | tail -1)"
  if [[ -n "$dir" && -x "$sdk/build-tools/$dir/zipalign" ]]; then
    echo "$sdk/build-tools/$dir"
    return 0
  fi
  return 1
}

NIXIANG_BUILD_TOOLS="$(_resolve_build_tools || true)"
export NIXIANG_BUILD_TOOLS

if [[ -n "${NIXIANG_BUILD_TOOLS:-}" ]]; then
  export NIXIANG_ZIPALIGN="$NIXIANG_BUILD_TOOLS/zipalign"
  export NIXIANG_APKSIGNER="$NIXIANG_BUILD_TOOLS/apksigner"
else
  export NIXIANG_ZIPALIGN="$(command -v zipalign || true)"
  export NIXIANG_APKSIGNER="$(command -v apksigner || true)"
fi

export NIXIANG_UBER_SIGNER="$NIXIANG_ROOT/01_tools/uber-apk-signer.jar"

# Mod-menu / Gradle builds need JDK 17
if [[ -z "${JAVA_HOME:-}" ]] || ! java -version 2>&1 | head -1 | grep -q '17\.'; then
  if JAVA17="$(/usr/libexec/java_home -v 17 2>/dev/null)"; then
    export NIXIANG_JAVA17="$JAVA17"
  fi
fi
