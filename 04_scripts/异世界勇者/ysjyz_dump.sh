#!/usr/bin/env bash
# ================================================================
#  异世界勇者 管线 dump 阶段后端
#
#  jsc 解密已在分析阶段完成（06_dumped/异世界勇者/jsc_decrypted），
#  本脚本负责把"外科注入需要的 dex/Manifest 事实"落盘并出摘要：
#    - AndroidManifest 明文（aapt2 xmltree）
#    - classes2.dex baksmali（AppActivity / LocalApplication 所在 dex）
#    - dump_summary_8.1.1.md（类名/dex 归属/壳组件盘点）
#
#  产物：03_decompiled/异世界勇者/{AndroidManifest.xmltree.txt,smali2/,dump_summary_8.1.1.md}
#  幂等：已存在则重建（baksmali 目录先清）
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=../_common/env.sh
source "$SCRIPT_DIR/../_common/env.sh"

GAME="异世界勇者"
APK="$NIXIANG_ROOT/02_apks/${GAME}/${GAME}_8.1.1_official.apk"
DEC="$NIXIANG_ROOT/03_decompiled/${GAME}"
DUMP="$NIXIANG_ROOT/06_dumped/${GAME}"
AAPT2="${NIXIANG_BUILD_TOOLS:-}/aapt2"
BAKSMALI="$NIXIANG_ROOT/01_tools/smali/baksmali"

[[ -f "$APK" ]] || { echo "ERROR: 官包缺失 $APK" >&2; exit 1; }
[[ -x "$AAPT2" ]] || { echo "ERROR: aapt2 缺失 ($AAPT2)" >&2; exit 1; }
[[ -x "$BAKSMALI" ]] || { echo "ERROR: baksmali 缺失 ($BAKSMALI)" >&2; exit 1; }
[[ -f "$DUMP/jsc_decrypted/main.index.js" ]] \
  || { echo "ERROR: jsc 解密产物缺失 $DUMP/jsc_decrypted/main.index.js" >&2; exit 1; }

mkdir -p "$DEC"

echo "[1/4] AndroidManifest 明文"
"$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" > "$DEC/AndroidManifest.xmltree.txt"
grep -c . "$DEC/AndroidManifest.xmltree.txt" >/dev/null

echo "[2/4] 组件/权限摘要"
# 注意：本脚本 set -o pipefail，管道末端禁止用 head（会 SIGPIPE 使整脚本 rc=141）
"$AAPT2" dump badging "$APK" > "$DEC/badging.txt"
APP_CLS="$(grep -m1 -A12 'E: application' "$DEC/AndroidManifest.xmltree.txt" \
  | sed -n 's/.*android:name(0x01010003)="\([^"]*\)".*/\1/p' | sed -n '1p')"
LAUNCH_CLS="$(sed -n "s/^launchable-activity: name='\([^']*\)'.*/\1/p" "$DEC/badging.txt" | sed -n '1p')"
[[ -n "$APP_CLS" ]] || { echo "ERROR: 解析不到 application android:name" >&2; exit 1; }
[[ -n "$LAUNCH_CLS" ]] || { echo "ERROR: 解析不到 launchable-activity" >&2; exit 1; }
echo "  Application=$APP_CLS"
echo "  Activity=$LAUNCH_CLS"

echo "[3/4] classes2.dex baksmali（AppActivity 所在 dex）"
WORK="$(mktemp -d /tmp/ysjyz_dump.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
unzip -o -q "$APK" classes.dex classes2.dex -d "$WORK"
for d in classes.dex classes2.dex; do
  [[ -f "$WORK/$d" ]] || { echo "ERROR: $d 不在包内" >&2; exit 1; }
done
rm -rf "$DEC/smali2"
"$BAKSMALI" d -o "$DEC/smali2" "$WORK/classes2.dex"

ACT_SMALI="$DEC/smali2/${LAUNCH_CLS//.//}.smali"
APP_SMALI="$DEC/smali2/${APP_CLS//.//}.smali"
[[ -f "$ACT_SMALI" ]] || { echo "ERROR: $LAUNCH_CLS 不在 classes2.dex（dex 归属假设失效，需重新定位）" >&2; exit 1; }
[[ -f "$APP_SMALI" ]] || { echo "ERROR: $APP_CLS 不在 classes2.dex" >&2; exit 1; }
echo "  $ACT_SMALI ✓"
echo "  $APP_SMALI ✓"

# 壳/加固组件盘点（只记录事实，不下结论）
SUSPECTS="$(grep -o 'Lcom/baidu/xenv[^;]*;\|Lcom/tencent/StubShell[^;]*;\|Lcom/stub/[^;]*;\|Llibcore/io/Memory[^;]*;' \
  "$DEC/AndroidManifest.xmltree.txt" 2>/dev/null | sort -u | head -10 || true)"

echo "[4/4] 摘要 → $DEC/dump_summary_8.1.1.md"
cat > "$DEC/dump_summary_8.1.1.md" <<EOF
# 异世界勇者 8.1.1 拆包摘要

- 官包：\`02_apks/异世界勇者/异世界勇者_8.1.1_official.apk\`（545,635,354 B，md5 \`710d1cfe9a86f837d5c697b376e73767\`）
- 包名：\`com.mudamuda.idlewow\`，versionCode 811 / versionName 8.1.1，minSdk 26 / targetSdk 30
- Application：\`${APP_CLS}\`（classes2.dex）
- 启动 Activity：\`${LAUNCH_CLS}\`（classes2.dex，继承 \`org.cocos2dx.lib.Cocos2dxActivity\`）
- dex 布局：\`classes.dex\`(7,972,576 B) + \`classes2.dex\`(7,052,188 B) → 外科新增用 \`classes3.dex\`
- 引擎：Cocos Creator 2.4.2（\`libcocos2djs.so\` 27,320,344 B；ABI arm64-v8a + armeabi-v7a）
- 脚本：\`.jsc\` = XXTEA(btea, key \`fa6d338a-c7b8-4d\`) + gzip；解密产物 \`06_dumped/异世界勇者/jsc_decrypted/main.index.js\`（96,334 行）
- 第三方组件（广告/统计）：穿山甲 openadsdk、腾讯 GDT、快手 kwad、百度 xenv（广告 SDK 环境服务，非壳）
- 壳/加固：分析阶段未见 tup/StubShell/ACE 类壳；\`libthemis.so\` = TapTap themis-lite 环境组件（行为未验证）

## 外科注入锚点（本摘要登记的事实）

| 用途 | 目标 | 证据 |
|------|------|------|
| 加载 so + 菜单 | \`${APP_CLS}.attachBaseContext\` | 需含 super 调用 |
| 菜单生命周期 | \`${LAUNCH_CLS}.onCreate/onResume/onPause\` | 继承 Cocos2dxActivity |
| JS→Java 桥 | \`${LAUNCH_CLS}.get_ret(I)Z\` / \`get_val(I)I\` | 由外科脚本注入，转 \`com.starry.magic.cocos.YsjYzHooks\` |

${SUSPECTS:+## 可疑壳组件（仅记录）\n\n\`\`\`\n${SUSPECTS}\n\`\`\`\n}
EOF

echo "[dump] done"
