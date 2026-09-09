#!/usr/bin/env bash
# 重新生成自签证书（改 IP 时用）
set -euo pipefail
IP="${1:-47.100.216.101}"
DIR="$(cd "$(dirname "$0")" && pwd)"
openssl req -x509 -newkey rsa:2048 -nodes -days 825 \
  -keyout "$DIR/starlight.key" -out "$DIR/starlight.crt" \
  -subj "/CN=${IP}" \
  -addext "subjectAltName=IP:${IP}"
echo "Generated for IP=${IP}"
openssl x509 -in "$DIR/starlight.crt" -noout -fingerprint -sha256
echo "Copy public cert to Android assets:"
echo "  cp \"$DIR/starlight.crt\" \"../../app/src/main/assets/certs/starlight.crt\""
