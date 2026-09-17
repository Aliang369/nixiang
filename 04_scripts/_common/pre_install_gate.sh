#!/usr/bin/env bash
# ================================================================
#  pre_install_gate.sh — 装机前产物机器关（四查）
#
#  背景（2026-09-17 会话记录审计定案）：多游戏装机失败属于机器可拦截错误，
#  每次都浪费一整轮装机+用户实测。本脚本在 install 阶段前强制拦截四类：
#    查1 JS语法关   ：注入 JS（拼接产物）node --check，防截断缺尾/语法错
#    查2 JNI配套关  ：Mod 源码注册的 JNI 方法名 vs dex 字符串池，
#                     防"新 so 注册 19 个方法、旧 dex 只有 17 个声明"装机即闪退
#    查3 P/Invoke符号关：EntryPoint 清单 vs so 导出表（nm -D），
#                     防 DllImport 名与 so 导出不符（调用即 EntryPointNotFoundException）
#    查4 控件读取关 ：菜单 JSON 控件类型 vs getVal/getRet 调用，
#                     防滑块被当布尔读（getRet 读 Slider → 永远 0/1）
#
#  用法（各查独立可选，传了才查；至少传一项）：
#    bash 04_scripts/_common/pre_install_gate.sh \
#      --js <注入JS文件> [--js <更多JS>...] \
#      --cpp <Mod源码.cpp> --dex <dex文件> [--dex <更多dex>...] \
#      --so <so文件> --pinvoke-json <EntryPoint清单.json> \
#      --menu-json <菜单模板.json> --cpp <含getVal/getRet调用的源码>
#
#  退出码: 0=通过/WARN-only；1=存在 FAIL（禁止装机）
#  注意: 与 preflight.sh 职责不同——preflight 管"改完源文件"的语法，
#        本脚本管"装机前"的产物交叉核对。
# ================================================================
set -u

# macOS Bash 3.2 兼容：不用 mapfile/关联数组；不用 pipefail（防 SIGPIPE 假失败）

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
TMPDIR_PF="$(mktemp -d /tmp/pre_install_gate.XXXXXX)" || exit 1
trap 'rm -rf "$TMPDIR_PF"' EXIT

fail=0; pass=0; warn=0; skip=0

# ---------- 参数收集 ----------
js_files=""; cpp_files=""; dex_files=""; so_file=""; pinvoke_json=""; menu_json=""

while [ $# -gt 0 ]; do
  case "$1" in
    --js)          js_files="$js_files $2"; shift 2 ;;
    --cpp)         cpp_files="$cpp_files $2"; shift 2 ;;
    --dex)         dex_files="$dex_files $2"; shift 2 ;;
    --so)          so_file="$2"; shift 2 ;;
    --pinvoke-json) pinvoke_json="$2"; shift 2 ;;
    --menu-json)   menu_json="$2"; shift 2 ;;
    -h|--help)     sed -n '2,30p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1（--help 看用法）"; exit 1 ;;
  esac
done

# ---------- 查1：JS 语法关 ----------
for f in $js_files; do
  if [ ! -f "$f" ]; then
    echo "✗ FAIL  [JS缺失] $f"; fail=$((fail+1)); continue
  fi
  if ! command -v node >/dev/null 2>&1; then
    echo "- SKIP  [JS语法] ${f}（本机无 node）"; skip=$((skip+1)); continue
  fi
  # node --check 只认 .js 扩展名：非 .js 产物复制为临时 .js 再查（防拼接产物无扩展名）
  js_tmp="$TMPDIR_PF/js_check_$$.js"
  cp "$f" "$js_tmp" 2>/dev/null || { echo "✗ FAIL  [JS语法] 无法读取: $f"; fail=$((fail+1)); continue; }
  if node --check "$js_tmp" 2>"$TMPDIR_PF/js_err"; then
    echo "✓ PASS  [JS语法] $f"; pass=$((pass+1))
  else
    echo "✗ FAIL  [JS语法] ${f}（注入 JS 有语法错/被截断：先查拼接边界）"
    sed 's/^/       /' "$TMPDIR_PF/js_err"; fail=$((fail+1))
  fi
done

# ---------- 查2：JNI 配套关（cpp 注册方法 vs dex 字符串池） ----------
if [ -n "$cpp_files" ] && [ -n "$dex_files" ]; then
  # 2a. 从 cpp 提取动态注册方法名：regOne(env, clazz, "name", "sig", fn)
  #     与静态注册符号声明文本：Java_com_xxx_yyy（源码里 JNIEXPORT 函数名）
  jni_names="$TMPDIR_PF/jni_names.txt"; : > "$jni_names"
  for f in $cpp_files; do
    [ -f "$f" ] || { echo "✗ FAIL  [JNI] cpp 缺失: $f"; fail=$((fail+1)); }
    grep -oE 'regOne[[:space:]]*\([^,]+,[^,]+,[[:space:]]*"[A-Za-z0-9_]+"' "$f" 2>/dev/null \
      | grep -oE '"[A-Za-z0-9_]+"' | tr -d '"' >> "$jni_names"
    # 静态注册符号拆解：Java_com_x_y → 类路径 com/x + 方法名 y（末段），分别核对 dex
    grep -oE 'Java_[A-Za-z0-9_]+' "$f" 2>/dev/null | while read -r sym; do
      base="${sym#Java_}"
      cls="$(printf '%s' "$base" | sed 's/_[^_]*$//; s/_/\//g')"
      mth="${base##*_}"
      printf '%s\n%s\n' "$cls" "$mth" >> "$jni_names"
    done
  done
  # 去重
  if [ -s "$jni_names" ]; then
    sort -u "$jni_names" -o "$jni_names"
    # 2b. 每个方法名必须出现在至少一个 dex 的字符串池里（dex 是二进制，用 grep -a）
    miss=0; total=0
    while read -r name; do
      total=$((total+1))
      found=0
      for d in $dex_files; do
        if [ -f "$d" ] && grep -aqF "$name" "$d" 2>/dev/null; then found=1; break; fi
      done
      if [ "$found" -eq 0 ]; then
        echo "✗ FAIL  [JNI配套] 方法『${name}』在所有 dex 中都找不到声明（so 注册了、dex 没有 → 装机即 UnsatisfiedLinkError/SIGABRT）"
        miss=$((miss+1))
      fi
    done < "$jni_names"
    if [ "$miss" -eq 0 ]; then
      echo "✓ PASS  [JNI配套] $total 个 JNI 方法名在 dex 中全部有声明"; pass=$((pass+1))
    else
      echo "       （若是故意保留的兼容空实现，需在装机报告中显式说明并经用户确认后才可放行）"
      fail=$((fail+1))
    fi
  else
    echo "- SKIP  [JNI配套] cpp 中未发现 JNI 注册（regOne/Java_*），无消耗"
  fi
fi

# ---------- 查3：P/Invoke 符号关（EntryPoint vs so 导出表） ----------
if [ -n "$pinvoke_json" ] && [ -n "$so_file" ]; then
  if [ ! -f "$pinvoke_json" ] || [ ! -f "$so_file" ]; then
    echo "✗ FAIL  [PInvoke] 文件缺失: pinvoke=$pinvoke_json so=$so_file"; fail=$((fail+1))
  else
    # 3a. 递归提取 json 中所有 EntryPoint 值
    entries="$TMPDIR_PF/entries.txt"
    python3 - "$pinvoke_json" > "$entries" <<'PYEOF'
import json, sys
def walk(o):
    if isinstance(o, dict):
        for k, v in o.items():
            if k == "EntryPoint" and isinstance(v, str):
                print(v)
            else:
                walk(v)
    elif isinstance(o, list):
        for i in o: walk(i)
try:
    with open(sys.argv[1], encoding="utf-8") as f:
        walk(json.load(f))
except Exception as e:
    sys.stderr.write(f"pinvoke json 解析失败: {e}\n"); sys.exit(1)
PYEOF
    if [ $? -ne 0 ] || [ ! -s "$entries" ]; then
      echo "✗ FAIL  [PInvoke] $pinvoke_json 解析失败或无 EntryPoint 字段"; fail=$((fail+1))
    else
      sort -u "$entries" -o "$entries"
      # 3b. nm 工具选择（llvm-nm 优先，ndk 自带）
      NM=""
      for cand in llvm-nm nm; do
        command -v "$cand" >/dev/null 2>&1 && { NM="$cand"; break; }
      done
      if [ -z "$NM" ]; then
        echo "- SKIP  [PInvoke] 本机无 nm/llvm-nm"; skip=$((skip+1))
      else
        "$NM" -D "$so_file" 2>/dev/null | grep -oE '[A-Za-z_][A-Za-z0-9_]*$' | sort -u > "$TMPDIR_PF/so_exports.txt"
        miss=0; total=0
        while read -r e; do
          total=$((total+1))
          if ! grep -qxF "$e" "$TMPDIR_PF/so_exports.txt"; then
            echo "✗ FAIL  [PInvoke] EntryPoint『${e}』不在 so 导出表中（调用即抛异常，战斗协程被打断）"
            miss=$((miss+1))
          fi
        done < "$entries"
        if [ "$miss" -eq 0 ]; then
          echo "✓ PASS  [PInvoke] $total 个 EntryPoint 全部在 so 导出表中"; pass=$((pass+1))
        else
          fail=$((fail+1))
        fi
      fi
    fi
  fi
fi

# ---------- 查4：控件读取一致性关（菜单 JSON vs getVal/getRet） ----------
if [ -n "$menu_json" ] && [ -n "$cpp_files" ]; then
  if [ ! -f "$menu_json" ]; then
    echo "✗ FAIL  [控件] 菜单 JSON 缺失: $menu_json"; fail=$((fail+1))
  else
    # 4a. 菜单控件表：feat_num + type（无 type 默认 toggle）→ 临时文件 "num type"
    ctrl="$TMPDIR_PF/controls.txt"; : > "$ctrl"
    python3 - "$menu_json" >> "$ctrl" <<'PYEOF2'
import json, sys
try:
    with open(sys.argv[1], encoding="utf-8") as f:
        data = json.load(f)
    feats = data.get("features", data if isinstance(data, list) else [])
    for it in feats:
        n = it.get("feat_num"); t = it.get("type", "toggle")
        if n is not None:
            print(f"{n} {t}")
except Exception as e:
    sys.stderr.write(f"menu json 解析失败: {e}\n"); sys.exit(1)
PYEOF2
    if [ ! -s "$ctrl" ]; then
      echo "- SKIP  [控件] 菜单 JSON 无 features/feat_num（格式不匹配，人工核对）"; skip=$((skip+1))
    else
      # 4b. 从 cpp/头文件抓读取调用（仅字面量索引；变量传参形式机器无法核对，靠人工）：
      #     callStaticMethod(H, "getRet", "(I)Z", 4) / 裸 getRet(4) / getVal(4)
      reads="$TMPDIR_PF/reads.txt"; : > "$reads"
      for f in $cpp_files; do
        [ -f "$f" ] || continue
        grep -oE '"(getVal|getRet)",[[:space:]]*"[^"]*",[[:space:]]*[0-9]+' "$f" 2>/dev/null \
          | sed -E 's/"(getVal|getRet)".* ([0-9]+)$/\2 \1/' >> "$reads"
        grep -oE '(getVal|getRet)[[:space:]]*\([[:space:]]*[0-9]+[[:space:]]*\)' "$f" 2>/dev/null \
          | sed -E 's/(getVal|getRet)[[:space:]]*\([[:space:]]*([0-9]+)[[:space:]]*\)/\2 \1/' >> "$reads"
      done
      sort -u "$reads" -o "$reads" 2>/dev/null
      bad=0; w=0
      while read -r num mode; do
        [ -n "$num" ] || continue
        ctype="$(awk -v n="$num" '$1==n {print $2}' "$ctrl")"
        [ -n "$ctype" ] || continue   # 菜单里没有该编号，跳过（可能是内部通道）
        if [ "$mode" = "getRet" ] && [ "$ctype" != "toggle" ] && [ "$ctype" != "button" ]; then
          echo "✗ FAIL  [控件] feat $num 是 $ctype 但被 getRet 布尔读取（滑块/输入框读 getRet 只会得到 0/1）"
          bad=$((bad+1))
        elif [ "$mode" = "getVal" ] && [ "$ctype" = "toggle" ]; then
          echo "! WARN  [控件] feat $num 是 toggle 但被 getVal 数值读取（若非有意 0/1 语义请核对）"
          w=$((w+1))
        fi
      done < "$reads"
      # 4c. slider/spinner 必须被 getVal 读过（漏实现提醒）
      while read -r num ctype; do
        case "$ctype" in
          slider|spinner|input)
            if ! awk -v n="$num" '$1==n {found=1} END{exit !found}' "$reads" 2>/dev/null \
               && ! grep -q "^$num " "$reads" 2>/dev/null; then
              echo "! WARN  [控件] feat ${num}（${ctype}）在源码中未见 getVal/getRet 读取（可能漏实现或经 JS 侧变量传递）"
              w=$((w+1))
            fi ;;
        esac
      done < "$ctrl"
      if [ "$bad" -eq 0 ]; then
        echo "✓ PASS  [控件] 控件类型与读取方式无硬冲突（WARN=${w}）"; pass=$((pass+1)); warn=$((warn+w))
      else
        fail=$((fail+1))
      fi
    fi
  fi
fi

# ---------- 汇总 ----------
echo "────────────────────────────────"
echo "汇总: PASS=$pass  FAIL=$fail  WARN=$warn  SKIP=$skip"
if [ "$fail" -gt 0 ]; then
  echo "❌ 存在 FAIL：装机被拦截。修复产物/源码后重跑，禁止跳过本门禁直接装机。"
  exit 1
fi
echo "✅ 装机前门禁通过（WARN 属提醒项，装机报告需向用户说明）。"
exit 0
