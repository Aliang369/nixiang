#!/usr/bin/env bash
# 下载 ilspycmd nupkg 并直接用 dotnet 运行 DLL（本机只有 runtime，无 SDK）
set -uo pipefail
cd /e/nixiang/_cs104/tmp || exit 1
V="${1:-8.2.0.7535}"
URL="https://api.nuget.org/v3-flatcontainer/ilspycmd/${V}/ilspycmd.${V}.nupkg"
echo "== download $URL"
curl -sSL -o ilspycmd.nupkg "$URL" && ls -l ilspycmd.nupkg
rm -rf ilspycmd_pkg && mkdir -p ilspycmd_pkg
unzip -q -o ilspycmd.nupkg -d ilspycmd_pkg
find ilspycmd_pkg -name 'ilspycmd.dll' | head -3
echo "== 试运行"
DLL=$(find ilspycmd_pkg -name 'ilspycmd.dll' | head -1)
"/c/Program Files/dotnet/dotnet.exe" "$DLL" --version 2>&1 | head -5
