#!/usr/bin/env bash
# 发布玩法 OTA（覆盖该 game_id 的 latest）
# 用法:
#   ./publish_play_ota.sh <game_id> <ver> [note] [patch.json] [libstarry_magic.so]
# 环境:
#   AUTH_BASE   默认 https://127.0.0.1:8443
#   ADMIN_TOKEN 管理端 JWT（必填）
set -euo pipefail

GAME_ID="${1:-}"
VER="${2:-}"
NOTE="${3:-}"
PATCH_JSON="${4:-}"
SO_FILE="${5:-}"
AUTH_BASE="${AUTH_BASE:-https://127.0.0.1:8443}"
ADMIN_TOKEN="${ADMIN_TOKEN:-}"

if [[ -z "$GAME_ID" || -z "$VER" ]]; then
  echo "用法: $0 <game_id> <ver> [note] [patch.json] [libstarry_magic.so]" >&2
  exit 1
fi
if [[ -z "$ADMIN_TOKEN" ]]; then
  echo "请设置 ADMIN_TOKEN=管理端 JWT" >&2
  exit 1
fi
if [[ -z "$PATCH_JSON" && -z "$SO_FILE" ]]; then
  echo "至少提供 patch.json 或 so 之一" >&2
  exit 1
fi

ARGS=(-X POST "${AUTH_BASE%/}/v1/admin/ota/${GAME_ID}/publish"
  -H "Authorization: Bearer ${ADMIN_TOKEN}"
  -F "ver=${VER}"
  -F "note=${NOTE}")
if [[ -n "$PATCH_JSON" ]]; then
  ARGS+=(-F "patch_json=@${PATCH_JSON};type=application/json")
fi
if [[ -n "$SO_FILE" ]]; then
  ARGS+=(-F "so_file=@${SO_FILE};type=application/octet-stream")
fi

curl -k -sS "${ARGS[@]}"
echo
