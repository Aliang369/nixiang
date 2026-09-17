#!/system/bin/sh
# Device-side helper: dump a page from the last mapped libsecret data region.
set -eu

PKG="${1:-com.fightingstars.games.animal}"
OUT="${2:-/data/local/tmp/dwdlm_secret_state.bin}"
PID="$(pidof "$PKG")"
[ -n "$PID" ] || { echo "pid not found"; exit 1; }

LINE="$(grep 'libsecret.so' "/proc/$PID/maps" | tail -1)"
[ -n "$LINE" ] || { echo "libsecret mapping not found"; exit 1; }
START="${LINE%%-*}"
BASE=$((0x$START - 0x64000))
ADDR=$((BASE + 0x70000))
SKIP=$((ADDR / 4096))
dd if="/proc/$PID/mem" of="$OUT" bs=4096 skip="$SKIP" count=1
echo "pid=$PID base=$BASE addr=$ADDR out=$OUT"
chmod 0644 "$OUT"
