#!/usr/bin/env bash
# 无悔华夏 RVA 自动重标（P0-② 前端）
#
# 背景：手写 RVA 表在乐变热更后整体漂移（实测 按名 46 : 按 RVA 2，`rva miss` 221 条），
# 每次都要人肉对 dump。本脚本把整条链自动化：
#   1) 从 WhrhxMod.cpp 的 HookRvaOrIl2Cpp 站点提取 (类, 方法, 参数个数)
#   2) 从本机运行时 dump.cs 查实测 RVA
#   3) 从运行时 so 在同一 RVA 读首指令（本 so p_offset==p_vaddr==0 ⇒ 文件偏移=RVA）
#   4) 产出 WhrhxRvaGen.h（WHRHX_GEN_RVA_* + kExpectGen[]），并报告与手写枚举的漂移
#
# 解析内核在 04_scripts/无悔华夏/whrhx_rebaseline.py（本脚本负责默认值/校验/落盘/汇总）。
#
# 用法：
#   bash 04_scripts/无悔华夏/whrhx_rebaseline.sh              # 生成头 + 打印漂移报告
#   bash 04_scripts/无悔华夏/whrhx_rebaseline.sh --check      # 只看报告，不写文件
#   bash 04_scripts/无悔华夏/whrhx_rebaseline.sh --apply      # 额外把漂移值写回 cpp 枚举（留 .bak）
#   ... --dump <dump.cs> --so <so> --cpp <WhrhxMod.cpp> --out <WhrhxRvaGen.h>
#
# 退出码：0 = 全部站点解析成功；1 = 有未解析站点（先修站点/名字，再谈重标）；2 = 用法/环境错误。
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
GAMES="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni/Games"
DUMP="$NIXIANG/06_dumped/无悔华夏/current_20261011/runtime/il2cpp_dump/dump.cs"
SO="$NIXIANG/06_dumped/无悔华夏/current_20261011/runtime/libil2cpp_runtime_20261011.so"
CPP="$GAMES/WhrhxMod.cpp"
OUT="$GAMES/WhrhxRvaGen.h"
EXPECT_SO_SHA="79adda770c528633213efe26bd084d14839c58a3a5382007de19cfc3d897aec9"
MODE_EMIT=1
MODE_APPLY=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --check) MODE_EMIT=0; shift ;;
    --apply) MODE_APPLY=1; shift ;;
    --no-emit) MODE_EMIT=0; shift ;;
    --emit) MODE_EMIT=1; shift ;;
    --dump) DUMP="$2"; shift 2 ;;
    --so) SO="$2"; shift 2 ;;
    --cpp) CPP="$2"; shift 2 ;;
    --out) OUT="$2"; shift 2 ;;
    --expect-sha) EXPECT_SO_SHA="$2"; shift 2 ;;
    -h|--help) sed -n '2,26p' "$0"; exit 0 ;;
    *) echo "ERROR: unknown arg $1" >&2; exit 2 ;;
  esac
done

for f in "$DUMP" "$SO" "$CPP"; do
  [[ -f "$f" ]] || { echo "ERROR: missing $f" >&2; exit 2; }
done

SO_SHA="$(shasum -a 256 "$SO" | awk '{print $1}')"
echo "[rebase] so   = $SO"
echo "[rebase] sha  = $SO_SHA"
if [[ -n "$EXPECT_SO_SHA" && "$SO_SHA" != "$EXPECT_SO_SHA" ]]; then
  echo "[rebase] WARN: so sha 与预期不符（预期 ${EXPECT_SO_SHA}）—— 重标结果只对这份 so 有效！" >&2
fi
echo "[rebase] dump = $DUMP"

ARGS=(--dump "$DUMP" --so "$SO" --cpp "$CPP")
[[ $MODE_EMIT -eq 1 ]] && ARGS+=(--emit "$OUT")
[[ $MODE_EMIT -eq 1 ]] || ARGS+=(--check)
[[ $MODE_APPLY -eq 1 ]] && ARGS+=(--apply)

RC=0
python3 "$NIXIANG/04_scripts/无悔华夏/whrhx_rebaseline.py" "${ARGS[@]}" || RC=$?

if [[ $MODE_EMIT -eq 1 && $RC -eq 0 ]]; then
  echo "[rebase] 头文件: $OUT"
  echo "[rebase] 下一步："
  echo "  (1) 确认 WhrhxMod.cpp 已 #include \"WhrhxRvaGen.h\"（ResolveRva 会用 kExpectGen 校验序言）"
  echo "  (2) bash 04_scripts/无悔华夏/whrhx_build_so.sh"
  echo "  (3) bash 04_scripts/_common/dev_swap_so.sh libs_whrhx/arm64-v8a/libstarry_magic.so"
  echo "  (4) 冷启后 bash 04_scripts/无悔华夏/whrhx_verify.sh --log <log>"
elif [[ $MODE_EMIT -eq 1 ]]; then
  echo "[rebase] 有未解析站点 ⇒ 未生成头文件（先把手写站点改成 dump 里的真名，或补 argc 数组）" >&2
fi
exit $RC
