#!/usr/bin/env bash
# 勇士之心 出包可移植性自检（Mac 出包 → 模拟器可用 规约 §3）
#
# 作用：出包前静态扫「本次踩过的 7 类地雷」，输出 OK/WARN/FAIL 清单。
# 用法：
#   bash 04_scripts/勇士之心/ysxz_mac_portability_check.sh            # 扫默认范围
#   ROOT=/path/to/repo bash .../ysxz_mac_portability_check.sh          # 指定仓库根
#
# 退出码：0 = 无 FAIL（可能有 WARN）；1 = 至少一条 FAIL。
# 规约文档：10_docs/勇士之心/Mac出包避免模拟器问题_规约_20261004.md

set -uo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${ROOT:-$(cd "$HERE/../.." && pwd)}"

MOD_DIR="$ROOT/mod菜单/starry-mod-menu"
JNI_DIR="$MOD_DIR/app/src/main/jni"
SCRIPTS_DIR="$MOD_DIR/scripts"
SURGICAL_DIR="$ROOT/04_scripts/勇士之心"
GAMES_DIR="$JNI_DIR/Games"

FAILS=0
WARNS=0

c_ok()   { printf '  \033[32mOK\033[0m   %s\n' "$1"; }
c_warn() { printf '  \033[33mWARN\033[0m %s\n' "$1"; WARNS=$((WARNS + 1)); }
c_fail() { printf '  \033[31mFAIL\033[0m %s\n' "$1"; FAILS=$((FAILS + 1)); }
c_info() { printf '       %s\n' "$1"; }
sec()    { printf '\n[%s] %s\n' "$1" "$2"; }

# 在文件集合里 grep（文件不存在则静默跳过）
g_files() {
  local pattern="$1"; shift
  local f
  for f in "$@"; do
    [ -e "$f" ] || continue
    LC_ALL=C grep -nH -- "$pattern" "$f" 2>/dev/null
  done
}
g_r() { # 递归 grep
  local pattern="$1"; shift
  local d
  for d in "$@"; do
    [ -d "$d" ] || continue
    LC_ALL=C grep -rn --exclude-dir=.git --exclude-dir=build --exclude-dir=obj \
      --exclude='*.so' --exclude='*.o' --exclude='*.bak' --exclude='*.orig' -- "$pattern" "$d" 2>/dev/null
  done
}

printf '=== 勇士之心 出包可移植性自检 ===\n'
printf '仓库根: %s\n' "$ROOT"

if [ ! -d "$MOD_DIR" ]; then
  printf '\n[!] 找不到 mod 目录 %s（用 ROOT=<仓库根> 指定）\n' "$MOD_DIR"
  exit 1
fi

# ---------------------------------------------------------------- 1. 硬编码 RVA
sec 1/7 "硬编码 RVA（必须版本无关）"
RVA_HITS="$(g_r "kEvalRvaFallbacks\|kEvalRva\|rva 0x[0-9A-Fa-f]\+" "$JNI_DIR" | head -20)"
DISC_HITS="$(g_r "kEvalInsn\|dynsym\|LocateEvalString\|ResolveEvalViaJniBridge\|imm26" "$JNI_DIR" | head -10)"
if [ -z "$RVA_HITS" ]; then
  c_ok "未发现硬编码 evalString RVA"
elif [ -n "$DISC_HITS" ]; then
  c_warn "仍有硬编码 RVA 常量，但已存在特征发现路径（可接受，需多候选逐条校验）"
  printf '%s\n' "$RVA_HITS" | sed 's/^/       /'
else
  c_fail "只有硬编码 RVA、没有特征发现路径：换版本/换 ABI 必失效"
  printf '%s\n' "$RVA_HITS" | sed 's/^/       /'
fi

# ------------------------------------------------------- 2. 只认可执行段
sec 2/7 "映射扫描是否只认 x 权限段（Houdini 下 ARM 代码是 r--p）"
X_ONLY="$(g_r "strchr(perm, *'x')" "$JNI_DIR" | head -10)"
R_FALLBACK="$(g_r "strchr(perm, *'r')\|readable-map fallback\|可读映射" "$JNI_DIR" | head -10)"
if [ -z "$X_ONLY" ]; then
  c_ok "未发现只认 'x' 的映射筛选（或实现方式不同）"
elif [ -n "$R_FALLBACK" ]; then
  c_ok "存在 'x' 快路径 + 可读段兜底（模拟器可用）"
else
  c_fail "只接受含 'x' 的映射行 ⇒ 模拟器下恒报 not mapped"
  printf '%s\n' "$X_ONLY" | sed 's/^/       /'
fi

# ------------------------------------------------------ 3. Mac 绝对路径
sec 3/7 "硬编码 Mac 绝对路径（/Users/...）"
MAC_RAW="$(g_r "/Users/[A-Za-z0-9_.-]\+" "$SCRIPTS_DIR" "$SURGICAL_DIR" "$JNI_DIR" "$MOD_DIR/app/build.gradle" "$MOD_DIR/build.gradle" "$MOD_DIR/gradle.properties" "$MOD_DIR/settings.gradle" \
          | grep -v 'ysxz_mac_portability_check\.sh:' || true)"
# 只看会真正执行的代码文件；.json/.txt 是"当时机器"的记录快照，单列成 WARN
MAC_PATH="$(printf '%s\n' "$MAC_RAW" | grep -vE '\.(json|txt|md):' | head -20)"
MAC_DATA="$(printf '%s\n' "$MAC_RAW" | grep -E '\.(json|txt|md):' | head -10)"
if [ -z "$MAC_PATH" ]; then
  c_ok "代码文件未发现 /Users/ 硬编码路径"
else
  c_fail "硬编码 Mac 主目录：换机器/换人必跑不起来（改为按 \${BASH_SOURCE[0]} 推仓库根）"
  printf '%s\n' "$MAC_PATH" | sed 's/^/       /'
fi
if [ -n "$MAC_DATA" ]; then
  c_warn "记录类文件(.json/.txt)里仍留有当时机器的绝对路径：属数据快照，换机需重跑生成"
  printf '%s\n' "$MAC_DATA" | sed 's/^/       /'
fi

# ------------------------------------------------------ 4. mac-only 命令
sec 4/7 "仅限 macOS 的命令 / 三元组"
MAC_CMD="$(g_r "md5 -q\|/usr/libexec/java_home\|darwin-x86_64\|darwin-arm64\|sysctl -n hw.ncpu\|shasum -a 256\|xcrun" "$SCRIPTS_DIR" "$SURGICAL_DIR" "$MOD_DIR/app/src/main/jni/Application.mk" | head -20)"
if [ -z "$MAC_CMD" ]; then
  c_ok "未发现 mac-only 命令依赖"
else
  c_warn "存在 mac-only 命令：建议探测式取用（md5sum/shasum/md5 -q 三选一、command -v java + readlink、uname -s 决定三元组）"
  printf '%s\n' "$MAC_CMD" | sed 's/^/       /'
fi

# ------------------------------------------------------ 5. 非 ASCII 构建戳
sec 5/7 "构建元数据（git log 结果直接进 -D 宏会带中文/空格）"
EPOCH_RAW="$(g_r "git log -1 --format=%ct" "$JNI_DIR" "$SCRIPTS_DIR" | head -10)"
if [ -z "$EPOCH_RAW" ]; then
  c_ok "未在 NDK 构建里直接用 git log 结果"
else
  SANITIZED=1
  echo "$EPOCH_RAW" | LC_ALL=C grep -q "tr -cd\|tr -dc" || SANITIZED=0
  g_r "STARRY_BUILD_EPOCH *?=" "$JNI_DIR" | grep -q . || SANITIZED=0
  if [ "$SANITIZED" = "1" ]; then
    c_ok "git log 结果已过滤/可覆盖（STARRY_BUILD_EPOCH ?= + tr -cd '0-9'）"
  else
    c_fail "构建戳未过滤：含中文的提交信息会让 clang++ 报 The system cannot find the file specified."
    printf '%s\n' "$EPOCH_RAW" | sed 's/^/       /'
  fi
fi

# ------------------------------------------------------ 6. 中文路径内联
sec 6/7 "非 ASCII 路径是否内联进命令串（PowerShell→bash 引号层会被拆词）"
NB="$(LC_ALL=C grep -rn "[^ -~]" "$SURGICAL_DIR" --include='*.sh' 2>/dev/null | head -8)"
INLINE="$(LC_ALL=C grep -rn "bash .*-lc\|-lc \"" "$SURGICAL_DIR" --include='*.sh' 2>/dev/null | head -8)"
if [ -z "$NB" ]; then
  c_ok "出包脚本内无非 ASCII 内容"
elif [ -n "$INLINE" ]; then
  c_warn "脚本含非 ASCII 且出现内联 -lc 调用：中文路径/正则不要走内联命令，写成脚本文件执行"
  printf '%s\n' "$INLINE" | sed 's/^/       /'
else
  c_ok "含非 ASCII（目录名本身），但未见内联命令串风险"
fi
c_info "提示：Windows 侧用 ASCII junction（如 E:\\nixiang\\starry_link）指向中文目录，不要 cygpath。"
c_info "提示：经 pwsh 调 git-bash 时用 * 通配代替中文路径：bash -n /e/nixiang/04_scripts/*/xxx.sh"

# ------------------------------------------------------ 7. 签名与 ABI
sec 7/7 "签名固定 + ABI 策略"
KS="$ROOT/01_tools/starry_release.jks"
if [ -f "$KS" ]; then
  c_ok "签名库存在（alias starry，证书 CN=StarryMagic）"
else
  c_warn "未找到 $KS：确认出包是否用了固定签名库（换签名 ⇒ install -r 被拒 ⇒ 卸载 ⇒ 丢登录态与 831MB 热更）"
fi
g_r "INSTALL_FAILED_UPDATE_INCOMPATIBLE\|install -r\|adb install" "$SURGICAL_DIR" | grep -q . \
  && c_ok "出包/安装流程使用 install -r 覆盖安装" \
  || c_warn "未见 install -r 覆盖安装流程：禁止 uninstall"
if [ -d "$MOD_DIR/app/src/main/jniLibs/arm64-v8a" ]; then
  c_ok "存在 arm64-v8a 产物目录（MuMu/Houdini 实际执行的那份）"
else
  c_warn "缺 arm64-v8a 产物目录"
fi
if [ -d "$MOD_DIR/app/src/main/jniLibs/x86" ] || [ -d "$MOD_DIR/app/src/main/jniLibs/x86_64" ]; then
  c_info "存在 x86 副本：Houdini 翻译层下不执行，属死重量（可留，但别把它当作模拟器验证依据）"
fi
g_r "assets/battleLogic/index.js\|assets/game/index.js" "$SURGICAL_DIR" "$SCRIPTS_DIR" | grep -q . \
  && c_warn "出包流程提到服务端校验名单里的 4 个文件：确认未被改写（改必被 md5 杀 ⇒ exitProgram）" \
  || c_ok "未触碰服务端校验名单的 4 个 bundle"

# ---------------------------------------------------------------- 汇总
printf '\n=== 汇总 ===\n'
printf 'FAIL=%d  WARN=%d\n' "$FAILS" "$WARNS"
if [ "$FAILS" -gt 0 ]; then
  printf '结论：存在阻断项，先修 FAIL 再出包。\n'
  exit 1
fi
printf '结论：无阻断项，可以出包（WARN 项建议排期清理）。\n'
exit 0
