#!/usr/bin/env bash
# ================================================================
#  pre_install_gate.sh — 装机前产物机器关（六查）
#
#  背景（2026-09-17 会话记录审计定案）：多游戏装机失败属于机器可拦截错误，
#  每次都浪费一整轮装机+用户实测。本脚本在 install 阶段前强制拦截五类：
#    查1 JS语法关   ：注入 JS（拼接产物）node --check，防截断缺尾/语法错
#    查2 JNI配套关  ：Mod 源码动态注册的方法名（含 OBFUSCATE("…") 写法）vs dex 字符串池，
#                     防"新 so 注册 19 个方法、旧 dex 只有 17 个声明"装机即闪退；
#                     静态导出 Java_xxx 在 dex 无声明只 WARN（无 native 声明即无人查找，不崩）
#    查3 P/Invoke符号关：EntryPoint 清单 vs so 导出表（nm -D），
#                     防 DllImport 名与 so 导出不符（调用即 EntryPointNotFoundException）
#    查4 控件读取关 ：菜单 JSON 控件类型 vs getVal/getRet 调用，
#                     防滑块被当布尔读（getRet 读 Slider → 永远 0/1）
#    查5 门禁串字面量存在性关（2026-10-02 起，P2-1）：声明的门禁串（类名/方法名/
#                     const-string）逐个在**已构建产物**里做二进制安全存在性检查
#                     （-F 固定串 + -a 二进制当文本 + -c 计数，APK 走 unzip -p classes*.dex），
#                     计数 0 即 FAIL ⇒ 防"门禁串被写成 smali `#` 注释、汇编器丢弃后
#                     产物里根本没有该字面量、门禁却显示通过"（本轮因此失败 2 轮）
#
#  用法（各查独立可选，传了才查；至少传一项）：
#    bash 04_scripts/_common/pre_install_gate.sh \
#      --js <注入JS文件> [--js <更多JS>...] \
#      --cpp <Mod源码.cpp> --dex <dex文件> [--dex <更多dex>...] \
#      --so <so文件> --pinvoke-json <EntryPoint清单.json> \
#      --menu-json <菜单模板.json> --cpp <含getVal/getRet调用的源码>
#      --runtime-menu-json <运行时控件清单.json>  # 可选；允许 Slider_/Toggle_ 等运行时名称，只用于控件类型检查
#      --apk <目标APK> --needle '<门禁串>' [--needle '...'...]  # 查5；也可 --dex 当目标
#      --needle-file <门禁串清单.txt>            # 查5；每行一条，'#' 开头的整行注释被跳过
#      --game <游戏> [--rounds-threshold N] [--allow-rounds]   # 查6；读该游戏轮次台账做根因闸门
#
#  ⚠️ 查5 门禁串必须是**产物中真实存在的字面量**（类名/方法名/const-string）。
#     **不要用 smali 的 `#` 注释当门禁串**——汇编器会把注释整行丢弃，产物里根本
#     没有该字面量，只有"二进制字面量存在性机检"才能抓到这类假通过。
#
#    查6 轮次根因关   ：（2026-10-10 复盘 P1-3）传 `--game <游戏>` 时，读 10_docs/<游戏>/轮次台账.tsv，
#                     同一功能**连续 ≥2 轮「无效」**则必须有「根因定案」（10_docs/<游戏>/根因定案.tsv，
#                     rootcause_note.sh 登记，定案轮次 ≥ 最后一次无效轮次），否则 FAIL。
#                     防的是「同一个功能换个自变量再装一次包、白耗一整轮 + 用户一次实测」。
#                     显式强过：`STAGE_NEXT_FORCE=1` 或 `--allow-rounds`（须在台账写明理由）。
#    查7 包内 Manifest 形态关（2026-10-15 补）：`--apk` 时校验包内 AndroidManifest.xml 头 4 字节
#                     = 03 00 08 00（真 AXML）；命中文本 Manifest 一律 FAIL。
#                     防「dump 的文本 Manifest 与外科 MF_CACHE 同名 ⇒ 文本进包、构建全绿」。
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
runtime_menu_json=""; apk_file=""
game=""; rounds_threshold=2; allow_rounds=""
NEEDLES="$TMPDIR_PF/needles.txt"; : > "$NEEDLES"

while [ $# -gt 0 ]; do
  case "$1" in
    --js)          js_files="$js_files $2"; shift 2 ;;
    --cpp)         cpp_files="$cpp_files $2"; shift 2 ;;
    --dex)         dex_files="$dex_files $2"; shift 2 ;;
    --so)          so_file="$2"; shift 2 ;;
    --pinvoke-json) pinvoke_json="$2"; shift 2 ;;
    --menu-json)   menu_json="$2"; shift 2 ;;
    --runtime-menu-json) runtime_menu_json="$2"; shift 2 ;;
    --apk)         apk_file="$2"; shift 2 ;;
    --game)        game="$2"; shift 2 ;;
    --rounds-threshold) rounds_threshold="$2"; shift 2 ;;
    --allow-rounds) allow_rounds="1"; shift ;;
    --needle)      printf '%s\n' "$2" >> "$NEEDLES"; shift 2 ;;
    --needle-file)
      if [ ! -f "$2" ]; then echo "✗ 门禁串清单缺失: $2"; exit 1; fi
      grep -v '^[[:space:]]*#' "$2" 2>/dev/null >> "$NEEDLES" || true
      shift 2 ;;
    -h|--help)     sed -n '2,/^set -u$/p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1（--help 看用法）"; exit 1 ;;
  esac
done

# ---------- 共用：二进制安全「字面量存在性」检查（查2 / 查5 共用，勿另起一套） ----------
# 用法：dex_literal_count <needle> <目标...>   → stdout 打印命中总次数
#   目标可为 APK（自动 unzip -p 'classes*.dex' 后统计）或裸 dex 文件。
#   选项固定 -F（固定串，不当正则） -a（把二进制当文本） -c（计数）；
#   BSD grep 兼容：不用 -P / -oP（本仓 macOS 无 rg、无 grep -P）。
dex_literal_count() {
  needle="$1"; shift
  total=0
  for src in "$@"; do
    [ -f "$src" ] || continue
    case "$src" in
      *.apk|*.zip) n="$(unzip -p "$src" 'classes*.dex' 2>/dev/null | grep -c -F -a -- "$needle" 2>/dev/null)" ;;
      *)           n="$(grep -c -F -a -- "$needle" "$src" 2>/dev/null)" ;;
    esac
    case "$n" in ''|*[!0-9]*) n=0 ;; esac
    total=$((total + n))
  done
  echo "$total"
}

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
# 方向定案（2026-09-20 我的勇者假 FAIL）：只有两个方向之一会崩——
#   动态注册 regOne 的方法名 dex 里没有 → RegisterNatives 抛 NoSuchMethodError/SIGABRT → FAIL
#   静态导出 Java_xxx 的符号 dex 里没有 → dex 无 native 声明即无人查找该符号，不崩 → WARN
# 覆盖修正：regOne 第三个参数普遍写作 OBFUSCATE("name")（编译期混淆、运行时还原明文），
#   旧正则只认裸字符串 → 19 条注册被整批漏抽、该查形同虚设（只剩静态导出在报错）。
if [ -n "$cpp_files" ] && [ -n "$dex_files" ]; then
  reg_names="$TMPDIR_PF/reg_names.txt"; : > "$reg_names"
  stat_syms="$TMPDIR_PF/stat_syms.txt"; : > "$stat_syms"
  for f in $cpp_files; do
    [ -f "$f" ] || { echo "✗ FAIL  [JNI] cpp 缺失: $f"; fail=$((fail+1)); }
    # 2a. 动态注册名：regOne(env, clazz, "name", …) 或 regOne(env, clazz, OBFUSCATE("name"), …)
    grep -oE 'regOne[[:space:]]*\([^,]*,[^,]*,[[:space:]]*(OBFUSCATE[[:space:]]*\([[:space:]]*)?"[A-Za-z0-9_]+"' "$f" 2>/dev/null \
      | grep -oE '"[A-Za-z0-9_]+"' | tr -d '"' >> "$reg_names"
    # 2b. 静态注册符号 Java_com_x_y（仅用于 WARN 提示，不构成装机阻断）
    grep -oE 'Java_[A-Za-z0-9_]+' "$f" 2>/dev/null >> "$stat_syms"
  done
  if [ -s "$reg_names" ]; then
    sort -u "$reg_names" -o "$reg_names"
    miss=0; total=0
    while read -r name; do
      [ -n "$name" ] || continue
      total=$((total+1))
      # 二进制安全字面量存在性（走共用 helper，勿另写一套 grep）
      if [ "$(dex_literal_count "$name" $dex_files)" -gt 0 ]; then found=1; else found=0; fi
      if [ "$found" -eq 0 ]; then
        echo "✗ FAIL  [JNI配套] regOne 注册的方法『${name}』在所有 dex 中都找不到（RegisterNatives 即抛 NoSuchMethodError/SIGABRT）"
        miss=$((miss+1))
      fi
    done < "$reg_names"
    if [ "$miss" -eq 0 ]; then
      echo "✓ PASS  [JNI配套] $total 个动态注册方法名在 dex 中全部有声明"; pass=$((pass+1))
    else
      echo "       （若是故意保留的兼容空实现，需在装机报告中显式说明并经用户确认后才可放行）"
      fail=$((fail+1))
    fi
  else
    echo "- SKIP  [JNI配套] cpp 中未发现 regOne 动态注册（含 OBFUSCATE 形式），无消耗"; skip=$((skip+1))
  fi
  # 2c. 静态导出符号：dex 无同名声明只提示、不拦截（死导出/遗留符号不构成崩溃）
  if [ -s "$stat_syms" ]; then
    sort -u "$stat_syms" -o "$stat_syms"
    dead=""
    while read -r sym; do
      [ -n "$sym" ] || continue
      mth="${sym##*_}"
      [ -n "$mth" ] || continue
      # 二进制安全字面量存在性（走共用 helper，勿另写一套 grep）
      if [ "$(dex_literal_count "$mth" $dex_files)" -gt 0 ]; then found=1; else found=0; fi
      if [ "$found" -eq 0 ]; then dead="$dead $mth"; fi
    done < "$stat_syms"
    if [ -n "$dead" ]; then
      echo "! WARN  [JNI配套] 静态导出符号在 dex 中无同名声明（无害遗留，不阻断装机）:$dead"
      warn=$((warn+1))
    else
      echo "✓ PASS  [JNI配套] 静态导出符号在 dex 中均有同名声明"; pass=$((pass+1))
    fi
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
control_json="${runtime_menu_json:-$menu_json}"
if [ -n "$control_json" ] && [ -n "$cpp_files" ]; then
  if [ ! -f "$control_json" ]; then
    echo "✗ FAIL  [控件] 控件清单缺失: $control_json"; fail=$((fail+1))
  else
    # 4a. 控件表：优先读 --runtime-menu-json；无 type 时按 Slider_/Toggle_ 名称前缀推断
    ctrl="$TMPDIR_PF/controls.txt"; : > "$ctrl"
    python3 - "$control_json" >> "$ctrl" <<'PYEOF2'
import json, sys
try:
    with open(sys.argv[1], encoding="utf-8") as f:
        data = json.load(f)
    feats = data.get("features", data if isinstance(data, list) else [])
    for it in feats:
        n = it.get("feat_num")
        t = it.get("type")
        name = it.get("name", "")
        if not t:
            low = name.lower()
            if name.startswith("Slider_") or low.startswith("slider_"): t = "slider"
            elif name.startswith("Spinner_") or low.startswith("spinner_"): t = "spinner"
            elif name.startswith("Input_") or low.startswith("input_"): t = "input"
            elif name.startswith("Button_") or low.startswith("button_"): t = "button"
            else: t = "toggle"
        if n is not None:
            print(f"{n} {t}")
except Exception as e:
    sys.stderr.write(f"控件清单解析失败: {e}\n"); sys.exit(1)
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

# ---------- 查5：门禁串字面量存在性关（2026-10-02 起，P2-1） ----------
# 背景：外科出包时曾把「门禁串」（要求产物里必须出现的类名/方法名/const-string）写成
#   smali 的 "#" 注释行 —— 汇编器把注释整行丢弃，产物里根本没有该字面量，而旧门禁只做
#   「文本包含」不做「二进制字面量存在性」机检，于是门禁显示通过（本轮失败 2 轮）。
# ⚠️ 门禁串必须是**产物中真实存在的字面量**（类名/方法名/const-string）；
#    **不要用 smali 的 "#" 注释当门禁串**——汇编器会丢弃，本查必然计数 0。
# 目标可为 APK（unzip -p 'classes*.dex'）或裸 dex；计数 0 ⇒ FAIL ⇒ 脚本末尾 exit 1 拦装机。
gate_targets=""
if [ -n "$apk_file" ]; then gate_targets="$gate_targets $apk_file"; fi
if [ -n "$dex_files" ]; then gate_targets="$gate_targets $dex_files"; fi
if [ -s "$NEEDLES" ]; then
  if [ -z "$gate_targets" ]; then
    echo "✗ FAIL  [门禁串] 传了 --needle/--needle-file 但没有检查目标（需 --apk 或 --dex）"
    fail=$((fail+1))
  else
    sort -u "$NEEDLES" -o "$NEEDLES"
    miss=0; total=0
    while IFS= read -r needle; do
      [ -n "$needle" ] || continue
      total=$((total+1))
      cnt="$(dex_literal_count "$needle" $gate_targets)"
      if [ "$cnt" -eq 0 ]; then
        # 2026-10-15（山海幻境 4.3 实测踩坑）：门禁串若写成**点分 Java 名**（com.x.Y），
        # dex 字符串池里只有斜杠形式（Lcom/x/Y; / com/x/Y），会假 FAIL 白耗一轮。
        # 归一：点分名自动再试斜杠形式；命中则 WARN 放过并提示改写。
        alt=""
        case "$needle" in
          *.*) alt="$(printf '%s' "$needle" | tr '.' '/')" ;;
        esac
        if [ -n "$alt" ] && [ "$alt" != "$needle" ]; then
          cnt_alt="$(dex_literal_count "$alt" $gate_targets)"
          if [ "$cnt_alt" -gt 0 ]; then
            echo "⚠ WARN  [门禁串] 『${needle}』是点分 Java 名；dex 里只有斜杠形式『${alt}』（命中 ${cnt_alt}）"
            echo "        ⇒ 本次按命中放过；请把门禁串改写成斜杠形式（类名用 com/x/Y），避免下次假 FAIL"
            warn=$((warn+1))
            continue
          fi
        fi
        echo "✗ FAIL  [门禁串] 字面量『${needle}』在产物中不存在（检查目标:${gate_targets}）"
        echo "        ⇒ 若该串写在 smali 里，先确认它不是被写成了 # 注释行（汇编器会丢弃，二进制里根本不存在该字面量）"
        miss=$((miss+1))
      fi
    done < "$NEEDLES"
    if [ "$miss" -eq 0 ]; then
      echo "✓ PASS  [门禁串] $total 个门禁串在产物中均为真实存在的字面量"; pass=$((pass+1))
    else
      fail=$((fail+1))
    fi
  fi
fi

# ---------- 查7：包内 Manifest 二进制形态关（2026-10-15 补，山海幻境 4.3 复盘） ----------
# 背景：dump 脚本把**文本** AndroidManifest.xml 落在与外科脚本 MF_CACHE **同名**的路径上，
#   外科会把文本 Manifest 原样塞进 zip —— 构建不报错、装机后 Manifest 坏（黑屏/装不上）。
#   判据：真 AXML 头 4 字节 = 03 00 08 00（其余一律 FAIL）。
if [ -n "$apk_file" ]; then
  axml_head="$(unzip -p "$apk_file" AndroidManifest.xml 2>/dev/null | head -c 4 | od -An -tx1 | tr -d ' \n')"
  case "$axml_head" in
    03000800) echo "✓ PASS  [Manifest形态] 包内 AndroidManifest.xml 是二进制 AXML" ;;
    "")       echo "✗ FAIL  [Manifest形态] 包内读不到 AndroidManifest.xml"; fail=$((fail+1)) ;;
    *)        echo "✗ FAIL  [Manifest形态] 包内 AndroidManifest.xml 头=${axml_head}（期望 03000800）⇒ 多半把文本 Manifest 打进包了"; fail=$((fail+1)) ;;
  esac
fi

# ---------- 查6：轮次根因关（2026-10-10 复盘 P1-3） ----------
# 背景：同一功能连续多轮「无效」时最容易发生的是「换个自变量再装一次包」——本轮明日方舟
#       因此白耗 2 轮 + 用户 2 次实测。规则：连续 >=2 轮无效必须留下根因定案（含证据行）。
if [ -n "$game" ]; then
  if [ -n "$allow_rounds" ] || [ "${STAGE_NEXT_FORCE:-}" = "1" ]; then
    echo "- WARN  [轮次根因] 已显式放行（${allow_rounds:+--allow-rounds }${STAGE_NEXT_FORCE:+STAGE_NEXT_FORCE=1}）：本轮交班/台账必须写明为何绕过轮次根因闸门"
    warn=$((warn+1))
  elif [ -f "$SCRIPT_DIR/rootcause_note.sh" ]; then
    if bash "$SCRIPT_DIR/rootcause_note.sh" --check --game "$game" --threshold "$rounds_threshold"; then
      pass=$((pass+1))
    else
      echo "        ⇒ 登记定案: bash 04_scripts/_common/rootcause_note.sh --game ${game} --feature <N> --round <最后一次无效轮次+1> --cause '<一句话根因，指向具体判据/代码行>' --evidence '<文件:行 或 logcat 行>' --verify '<验证命令>'"
      echo "        ⇒ 或显式强过（须写明理由）: STAGE_NEXT_FORCE=1（或本脚本 --allow-rounds）"
      fail=$((fail+1))
    fi
  else
    echo "- SKIP  [轮次根因] 未找到 $SCRIPT_DIR/rootcause_note.sh（无法做根因闸门）"
    skip=$((skip+1))
  fi
fi

# ---------- 查8：包内 so 构建戳关（2026-10-16 FW-13；终末地/小米 byron 事故） ----------
# so 内必须带 OTA 守卫用的构建戳 STARRYSTAMP1:EPOCH=<秒>。缺戳 ⇒ Java OtaSoStamp.apkStamp()=0
# ⇒ PlayOtaManager 的 `if (apkStamp > 0L && …)` 恒假 ⇒ **覆盖安装后永不清** starry_ota 旧 so
# ⇒ 玩家继续跑旧 so（症状：新包装上了、跑的却是旧 so，一开就崩）。根因（modern 入口零引用
# 被 --gc-sections 回收）已在共享 Includes/Utils.cpp 用 __attribute__((used, retain)) 修掉；
# 本关防的是「改动回退 / 新 mk 漏带」。逃生门：STAMP_CHECK=0（须在装机报告写明理由）。
if [ -n "$apk_file" ] && [ "${STAMP_CHECK:-1}" = "1" ]; then
  # 先落盘再 grep（禁 `unzip -l | grep -q`：pipefail 下 SIGPIPE(141) 会让命中被判成未命中，
  # 大 APK 的 listing 超管道缓冲时必现 ⇒ 本关会被静默跳过 = 假 PASS）
  apk_entries="$TMPDIR_PF/apk_entries.txt"
  unzip -l "$apk_file" > "$apk_entries" 2>/dev/null || true
  if grep -q 'lib/arm64-v8a/libstarry_magic.so' "$apk_entries"; then
    stamp="$(unzip -p "$apk_file" lib/arm64-v8a/libstarry_magic.so 2>/dev/null | grep -a -o 'STARRYSTAMP1:EPOCH=[0-9]*' | head -1)"
    if [ -n "$stamp" ]; then
      epoch="${stamp#STARRYSTAMP1:EPOCH=}"
      if [ "$epoch" = "1790184000" ]; then
        echo "- FAIL  [构建戳] 包内 so 的戳是**兜底值** ${stamp}（构建时未取到 git commit 时间）"
        echo "        ⇒ OTA 守卫判新旧会失准：兜底值恒小于之后任何真戳 ⇒ 覆盖安装**不清** OTA so ⇒ 跑旧 so"
        echo "        修：在能访问 git 的终端构建（Windows/Git Bash 需 git 在 PATH），或显式传 STARRY_BUILD_EPOCH=<秒>"
        echo "        逃生门：STAMP_CHECK=0（须在装机报告写明理由）"
        fail=$((fail+1))
      else
        echo "- PASS  [构建戳] 包内 so 含 ${stamp}（OTA 守卫可判新旧）"
        pass=$((pass+1))
      fi
    else
      echo "- FAIL  [构建戳] 包内 so 缺 STARRYSTAMP1:EPOCH=（OTA 守卫失效 ⇒ 覆盖安装后仍会优先跑玩家旧 OTA so）"
      echo "        修：重跑 so 构建（mk 需带 -Wl,--undefined=kStarryBuildStamp；共享 Includes/Utils.cpp 已有 retain）"
      echo "        逃生门：STAMP_CHECK=0（须在装机报告写明理由）"
      fail=$((fail+1))
    fi
  else
    echo "- SKIP  [构建戳] 包内无 lib/arm64-v8a/libstarry_magic.so（非星光包）"
    skip=$((skip+1))
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
