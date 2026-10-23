#!/usr/bin/env bash
# ================================================================
#  device_profile.sh — 设备画像 / 能力自检（一次探测，全设备复用）
#
#  要解决的问题（2026-10-04 用户提出）：
#    「为适配模拟器和真机好像每次都要改两次代码」。**代码不该按设备分叉**，
#    设备差异必须由运行时判据吸收；本脚本负责把差异**提前探测、落盘复用**，
#    于是同一份 so 在任何设备上都是同一条路径，出错时也能一眼看出是哪一层。
#
#  它回答三个问题：
#    ① 这台设备的 libil2cpp 是单映射还是多映射？（真机常见 dual；模拟器单份）
#       —— 直接按与源码 LibBaseResolve() 相同的分组规则算出它会选哪个基址，
#          并给出选错时会看到的现象。不用装机就能预判基址层。
#    ② 关键 RVA 落在哪个段、内存字节与文件字节是否一致
#       —— 判定「基址判据选中的是执行副本还是只读影子副本」。
#    ③ 装钩/注入/出包的环境条件：页大小（16K 页机型影响对齐）、fs-verity 提示
#       （影响「原地改已装 apk」这类快路）、root 形态、ABI、模拟器标志位。
#
#  ★ 本脚本对设备**完全只读**（唯一的非只读动作是 --launch 时 monkey 冷启，
#    不 force-stop、不清数据、不写任何文件）。结论是**静态预判**，运行时真值
#    仍以 §30.3 三行自检 + 菜单 SELFCHECK 行为准（本脚本会在末尾打印该提示）。
#
#  用法:
#    bash 04_scripts/_common/device_profile.sh --serial <SERIAL> [--pkg <包名>] \
#         [--rva 名称=0x十六进制]... [--launch] [--no-su] [--quiet] \
#         [--out <文件>] [--stdout-only]
#
#  例（冒险与征途 1.5.9，真机）:
#    bash 04_scripts/_common/device_profile.sh --serial f9a116a2 \
#         --pkg com.mxyzt.gzmy --launch \
#         --rva interp=0x207F798 --rva pushframe=0x207DCCC
#
#  产出: 10_docs/_state/设备画像/<serial>.md（换设备/换机器直接读，不重新探索）
#        + stdout 紧凑摘要（--quiet 只打摘要）
#
#  退出码: 0=画像已生成且判据正常 | 1=参数/设备/环境错误
#          2=画像已生成但**判据异常**（选了影子副本/找不到映射/RVA 不在可执行段），
#            管线可用它做「出包前静态门」
#
#  维护约束（同 env.sh）: macOS Bash 3.2 兼容 —— 不用 mapfile / readarray /
#  declare -A / local -n / ${v^^} / ;;& / &>>；重活交 python3。
# ================================================================
set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

SERIAL=""; PKG=""; RVAS=""; LAUNCH=0; USE_SU=1; QUIET=0; OUT=""; STDOUT_ONLY=0; IDLE_WAIT=6

while [ $# -gt 0 ]; do
  case "$1" in
    --serial)      SERIAL="$2"; shift 2 ;;
    --pkg)         PKG="$2"; shift 2 ;;
    --rva)         RVAS="${RVAS}${RVAS:+ }$2"; shift 2 ;;
    --launch)      LAUNCH=1; shift ;;
    --no-su)       USE_SU=0; shift ;;
    --quiet)       QUIET=1; shift ;;
    --out)         OUT="$2"; shift 2 ;;
    --stdout-only) STDOUT_ONLY=1; shift ;;
    --idle-wait)   IDLE_WAIT="$2"; shift 2 ;;
    -h|--help)     sed -n '2,/^set -u$/p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1（--help 看用法）" >&2; exit 1 ;;
  esac
done
[ -n "$SERIAL" ] || { echo "✗ 必填 --serial <SERIAL>（多设备时不给会歧义）" >&2; exit 1; }
case "$SERIAL" in
  *:*) SAFE_SERIAL="$(printf '%s' "$SERIAL" | tr ':' '_')" ;;
  *)   SAFE_SERIAL="$SERIAL" ;;
esac
[ -n "$OUT" ] || OUT="$NIXIANG_ROOT/10_docs/_state/设备画像/${SAFE_SERIAL}.md"

command -v adb >/dev/null 2>&1 || { echo "✗ 找不到 adb（先过 env_deps.sh）" >&2; exit 1; }
ST="$(adb -s "$SERIAL" get-state 2>/dev/null | tr -d '\r')"
[ "$ST" = "device" ] || { echo "✗ 设备不可用（get-state=${ST:-空}）: $SERIAL" >&2; exit 1; }

TMP="$(mktemp "${TMPDIR:-/tmp}/nx_devprof.XXXXXX")"
trap 'rm -f "$TMP"' EXIT

# ---------- 设备命令封装 ----------
adbx() { adb -s "$SERIAL" "$@" < /dev/null; }   # < /dev/null：否则 adb shell 会吞掉调用方 while-read 循环的 stdin
# 设备侧单条命令（自动去 \r）
dq() { adbx shell "$1" 2>/dev/null | tr -d '\r'; }
# root 执行（命令内**不得含单引号**；未探测到 su 时原样执行）
ROOT_MODE=""
ROOT_NOTE=""
probe_root() {
  [ "$USE_SU" = "1" ] || { ROOT_MODE=""; ROOT_NOTE="--no-su 指定不用 root"; return; }
  local uid
  uid="$(adbx shell "su -c 'id -u'" 2>/dev/null | tr -d '\r' | head -1)"
  case "$uid" in
    *0*) ROOT_MODE="su"; ROOT_NOTE="su -c 可用（KSU/Magisk 等）" ;;
    *)   ROOT_MODE="";   ROOT_NOTE="su -c 不可用 ⇒ /proc/<pid>/mem 与 /data/app 读取会失败" ;;
  esac
}
shroot() {
  if [ "$ROOT_MODE" = "su" ]; then adbx shell "su -c '$1'" 2>/dev/null | tr -d '\r'
  else adbx shell "$1" 2>/dev/null | tr -d '\r'; fi
}
getp() { dq "getprop $1" | head -1; }

# 读设备上某地址的 hex（bs=4096 跳页 + od；找不到 od 退 hexdump）
dev_hex() {   # $1=路径 $2=绝对偏移 $3=字节数
  local src="$1" addr="$2" cnt="${3:-16}"
  local page=$(( addr / 4096 )) off=$(( addr % 4096 )) hex=""
  # 注意：su -c '<cmd>' 的命令里**不能出现单引号**，所以 tr/hexdump 一律用双引号
  hex="$(shroot "dd if=$src bs=4096 skip=$page count=1 2>/dev/null | od -An -v -tx1 | tr -d \" \\n\"" | tr -d '\r\n ')"
  case "$hex" in ""|*[!0-9a-fA-F]*) hex="" ;; esac
  if [ -z "$hex" ]; then
    hex="$(shroot "dd if=$src bs=4096 skip=$page count=1 2>/dev/null | hexdump -v -e \"1/1 \\\"%02x\\\"\"" | tr -d '\r\n ')"
    case "$hex" in ""|*[!0-9a-fA-F]*) hex="" ;; esac
  fi
  [ -n "$hex" ] && printf '%s' "${hex:$(( off * 2 )):$(( cnt * 2 ))}"
}

emit() { printf '%s\n' "$*" >> "$TMP"; }

# ================= 1. 设备与系统 =================
probe_root
PAGE_SIZE="$(dq "getconf PAGE_SIZE" | head -1)"
[ -n "$PAGE_SIZE" ] || PAGE_SIZE="$(getp ro.boot.hardware.pagesize)"
ABI="$(getp ro.product.cpu.abi)"
ABILIST="$(getp ro.product.cpu.abilist)"
REL="$(getp ro.build.version.release)"
SDK="$(getp ro.build.version.sdk)"
QEMU="$(getp ro.kernel.qemu)"
KERNEL="$(dq "uname -r")"
MEMTOTAL="$(dq "head -1 /proc/meminfo")"

# ================= 2. 包与进程 =================
PKG_PATH=""; PKG_VER=""; PID=""
if [ -n "$PKG" ]; then
  PKG_PATH="$(dq "pm path $PKG" | grep '^package:' | grep 'base\.apk' | head -1 | sed 's/^package://')"
  PKG_VER="$(dq "dumpsys package $PKG | grep -m1 versionName" | awk -F= '{print $2}' | tr -d ' ')"
  PID="$(dq "pidof $PKG" | awk '{print $1}')"
  if [ -z "$PID" ] && [ "$LAUNCH" = "1" ]; then
    adbx shell "monkey -p $PKG -c android.intent.category.LAUNCHER 1" >/dev/null 2>&1
    sleep "$IDLE_WAIT"
    PID="$(dq "pidof $PKG" | awk '{print $1}')"
  fi
fi

# ================= 3. libil2cpp 映射分组（★核心判据） =================
MAPS_LIB=""; BASE=""; GRPINFO=""; RVAINFO=""
if [ -n "$PID" ]; then
  MAPS_LIB="$(shroot "cat /proc/$PID/maps" | grep 'libil2cpp')"
fi

PY_OUT=""
if [ -n "$MAPS_LIB" ]; then
  PY_OUT="$(printf '%s\n' "$MAPS_LIB" | python3 -c '
import sys
rvas=[]
for a in sys.argv[1:]:
    if "=" in a:
        n,v=a.split("=",1)
        try: rvas.append((n,int(v,16)))
        except ValueError: pass
groups=[]; cur=None
for line in sys.stdin:
    p=line.split()
    if len(p)<5: continue
    try: st,en=[int(x,16) for x in p[0].split("-")]
    except ValueError: continue
    perm,off,dev,ino=p[1],p[2],p[3],p[4]
    path=p[5] if len(p)>5 else ""
    key=(dev,ino)
    if cur is not None and cur["key"]==key and st==cur["end"]:
        cur["segs"].append((st,en,perm,off,path)); cur["end"]=en
    else:
        if cur is not None: groups.append(cur)
        cur={"key":key,"start":st,"end":en,"segs":[(st,en,perm,off,path)]}
if cur is not None: groups.append(cur)
print("GROUPCOUNT %d" % len(groups))
def xspan(g):
    return sum(e-s for s,e,p,o,pa in g["segs"] if "x" in p)
for i,g in enumerate(groups):
    rw=sum(e-s for s,e,p,o,pa in g["segs"] if p.startswith("rw"))
    off0=[s for s,e,p,o,pa in g["segs"] if o=="00000000"]
    g["rw"]=rw; g["off0"]=min(off0) if off0 else None
    print("GROUP %d segs=%d total=0x%x rw=0x%x xs=0x%x off0=%s path=%s" % (
        i,len(g["segs"]),g["end"]-g["start"],rw,xspan(g),
        ("0x%x"%g["off0"]) if g["off0"] is not None else "-",
        g["segs"][0][4] or "(anon)"))
# 与源码 LibBaseResolve() 同规则：rw-p 总量最大的那组；并列/全零退化为总长最大
best=max(range(len(groups)), key=lambda i:(groups[i]["rw"], groups[i]["end"]-groups[i]["start"]))
rule="rw-p总量最大组的off==0起点"
if not groups[best]["rw"]:
    rule="无rw-p段⇒退化为总长最大组起点"
base=groups[best]["off0"] if groups[best]["off0"] is not None else groups[best]["start"]
# ★后备判据（2026-10-23，我的勇者 7.6.4 实测）：壳加载时段间留 0x3000 空隙 ⇒ 同一 so 被拆成多组，
#   「最大 rw-p 组」可能落到末段 bss（组内无可执行段），base 错、所有 RVA 落空。
#   此时按主程序 FileOffToVa 的语义改取「含可执行段」的组：rw-p 最大者优先，
#   base = 该组最大 x 段的 start-off（即该映射副本的 off==0 起点）。
#   选中组本就可执行（旧行为正确）时**完全不变**——Mxytz 等既有游戏不受影响。
if xspan(groups[best])==0:
    xg=[i for i in range(len(groups)) if xspan(groups[i])>0]
    if xg:
        best=max(xg, key=lambda i:(groups[i]["rw"], groups[i]["end"]-groups[i]["start"]))
        seg=max([(e-s,s,int(o,16)) for s,e,p,o,pa in groups[best]["segs"] if "x" in p])
        xstart=seg[1]
        # 基址 = 同一 so 里 off==00000000 且起始地址 <= 该 x 段的最近一段起点
        #   （start-off 会差一个 ELF 首段页偏移 0x4000，不能用）
        cand=[s for gg in groups for s,e,p,o,pa in gg["segs"] if o=="00000000" and s<=xstart]
        base=max(cand) if cand else xstart-seg[2]
        rule="选中组无可执行段⇒改取含 r-xp 的组（FileOffToVa 语义：取 r-xp 段 + 同副本 off==0 起点为基址）"
g=groups[best]
print("PICK %d base=0x%x xs=0x%x rule=%s" % (best, base, xspan(g), rule))
print("BASEMSG 组数=%d 选中组=#%d 段数=%d 总长=0x%x rw-p=0x%x xs=0x%x（其它组：%s）" % (
    len(groups), best, len(g["segs"]), g["end"]-g["start"], g["rw"], xspan(g),
    ", ".join("#%d segs=%d rw=0x%x xs=0x%x"%(i,len(x["segs"]),x["rw"],xspan(x)) for i,x in enumerate(groups) if i!=best) or "无"))
for n,rv in rvas:
    addr=base+rv
    hit=None
    for i,gg in enumerate(groups):
        for s,e,p,o,pa in gg["segs"]:
            if s<=addr<e:
                fo=int(o,16)+(addr-s)
                hit=(i,p,fo,pa or "(anon)",s)
    if hit is None:
        print("RVA %s rva=0x%x addr=0x%x seg=- perm=- fileoff=- path=- inbest=-" % (n,rv,addr))
    else:
        print("RVA %s rva=0x%x addr=0x%x seg=0x%x perm=%s fileoff=0x%x path=%s inbest=%s" % (
            n,rv,addr,hit[4],hit[1],hit[2],hit[3],"yes" if hit[0]==best else "no"))
' $RVAS)"
fi

# ---------- 衍生量（一次 root 调用拿齐菜单 so 目录，其余为属性读取） ----------
RVA_CMD=""
for _w in $RVAS; do RVA_CMD="${RVA_CMD} --rva ${_w}"; done

PTRACE_VAL="$(dq "cat /proc/sys/kernel/yama/ptrace_scope 2>/dev/null" | head -1 | tr -d ' ')"
[ -n "$PTRACE_VAL" ] || PTRACE_VAL="（空）"

VERITY_VAL="$(getp ro.apk_verity.mode | tr -d ' ')"
[ -n "$VERITY_VAL" ] || VERITY_VAL="$(getp ro.boot.veritymode | tr -d ' ')"
[ -n "$VERITY_VAL" ] || VERITY_VAL="（空）"

MENU_SO_NOTE="（未判定）"
LIBDIR_LISTED=0
if [ -n "$PKG_PATH" ]; then
  _libdir="$(shroot "ls ${PKG_PATH%/*}/lib/arm64 2>/dev/null")"
  _total="$(printf '%s\n' "$_libdir" | grep -c .)"
  _starry="$(printf '%s\n' "$_libdir" | grep -c starry)"
  _il2="$(printf '%s\n' "$_libdir" | grep -c il2cpp)"
  if [ "$_total" != "0" ]; then
    LIBDIR_LISTED=1
    MENU_SO_NOTE="该目录 ${_total} 个 .so（starry*=${_starry} il2cpp*=${_il2}）⇒ **lib 已展开到磁盘**，extractNativeLibs 应为 true"
  else
    MENU_SO_NOTE="该目录为空 ⇒ **lib 仍在 apk 内**（extractNativeLibs=false）"
  fi
fi

GROUPCOUNT="$(printf '%s\n' "$PY_OUT" | awk '/^GROUPCOUNT/{print $2}')"
BASEMSG="$(printf '%s\n' "$PY_OUT" | sed -n 's/^BASEMSG //p')"
PICKLINE="$(printf '%s\n' "$PY_OUT" | sed -n 's/^PICK //p')"
BASE="$(printf '%s' "$PICKLINE" | sed -n 's/.*base=0x\([0-9a-fA-F]*\).*/\1/p')"
# 选中组自身可执行段总长（只看选中组，不能从 BASEMSG 里 grep —— 那里还列了「其它组」的同名字段）
SELX="$(printf '%s' "$PICKLINE" | sed -n 's/.*xs=0x\([0-9a-fA-F]*\).*/\1/p')"
GRPINFO="$(printf '%s\n' "$PY_OUT" | sed -n '/^GROUP /p')"
RVAINFO="$(printf '%s\n' "$PY_OUT" | sed -n '/^RVA /p')"

# ================= 4. 组装报告 =================
VERDICT="判据正常"; RC=0
[ -n "$PID" ] || { VERDICT="进程未运行 ⇒ 映射层未探测（加 --launch 冷启后再跑）"; RC=2; }
if [ -n "$PID" ] && [ -z "$MAPS_LIB" ]; then VERDICT="进程里【没有 libil2cpp.so】⇒ 未进入 IL2CPP 层"; RC=2; fi
if [ -n "$MAPS_LIB" ] && [ -z "$BASE" ]; then VERDICT="有映射但选不出基址 ⇒ 判据需修（§30.2 逐级降级）"; RC=2; fi
if [ -n "$BASE" ]; then
  # 选了影子副本的典型特征：**选中组自己**没有任何可执行段（SELX=0，只有 r--p/rw-p）
  if [ -n "$SELX" ] && [ "$((16#$SELX))" -eq 0 ]; then
    VERDICT="★疑似选中只读影子副本（选中组无可执行段 xs=0）⇒ 装钩会静默失败，须按 §30.2 加显式 RVA 交叉校验"; RC=2
  fi
fi
NVUMS="$(printf '%s\n' "$RVAINFO" | grep -c 'perm=-' || true)"
if [ -n "$RVAINFO" ] && [ "${NVUMS:-0}" != "0" ]; then
  VERDICT="${VERDICT}；有 ${NVUMS} 个 RVA 未落在任何段"; RC=2
fi

NOW="$(date '+%Y-%m-%d %H:%M:%S')"
{
  # ---------- 报告头 ----------
  emit "# 设备画像 — ${SERIAL}"
  emit ""
  emit "- 生成: ${NOW}（主机 $(uname -s)）/ 脚本 \`04_scripts/_common/device_profile.sh\`"
  emit "- **静态预判: ${VERDICT}**（运行时真值仍以三行自检 + 菜单 SELFCHECK 行为准，见文末）"
  emit "- 复现命令: \`bash 04_scripts/_common/device_profile.sh --serial ${SERIAL}$([ -n "$PKG" ] && printf ' --pkg %s' "$PKG")${RVA_CMD}\`"
  emit ""
  emit "## 1. 设备与系统"
  emit ""
  emit "| 项 | 值 |"
  emit "|---|---|"
  emit "| 型号/厂商 | $(getp ro.product.model) / $(getp ro.product.manufacturer) |"
  emit "| Android / SDK | ${REL} / ${SDK} |"
  emit "| ABI / abilist | ${ABI} / ${ABILIST} |"
  emit "| 内核 | ${KERNEL} |"
  emit "| 页大小 | ${PAGE_SIZE} $([ "${PAGE_SIZE:-4096}" = "16384" ] && printf '（★16K 页机型：so 需 16K 对齐）') |"
  emit "| ro.kernel.qemu | ${QEMU:-（空=不是 qemu/模拟器）} |"
  emit "| 物理内存 | ${MEMTOTAL} |"
  emit "| fingerprint | $(getp ro.build.fingerprint) |"
  emit "| 容器/虚拟化线索 | ro.boot.hardware=$(getp ro.boot.hardware) ro.product.board=$(getp ro.product.board) |"
  emit ""
  emit "## 2. root 与注入条件"
  emit ""
  emit "| 项 | 值 |"
  emit "|---|---|"
  emit "| root 形态 | ${ROOT_NOTE} |"
  emit "| adb 身份 | $(dq id | head -1) |"
  emit "| ptrace_scope | ${PTRACE_VAL}（空=内核无 yama/未启用；1/2 会影响 frida 附加） |"
  emit ""
  emit "## 3. 包与安装形态"
  emit ""
  if [ -n "$PKG" ]; then
    emit "| 项 | 值 |"
    emit "|---|---|"
    emit "| 包名 / 版本 | ${PKG} / ${PKG_VER} |"
    emit "| base.apk | ${PKG_PATH} |"
    emit "| 进程 pid | ${PID:-（未运行）} |"
    emit "| apk 大小 | $(shroot "stat -c %s ${PKG_PATH}" | head -1) |"
    emit "| 已装菜单 so | ${MENU_SO_NOTE} |"
    emit ""
  else
    emit "（未传 --pkg，跳过）"
    emit ""
  fi
  emit "## 4. libil2cpp 映射分组（★设备差异核心判据）"
  emit ""
  if [ -n "$MAPS_LIB" ]; then
    emit "分组规则与源码 \`LibBaseResolve()\` 一致：按 dev:inode 分组 → 组内地址首尾相接 → 取 **rw-p 总量最大那组**的 \`off==00000000\` 起点为基址；若该组**无可执行段**（壳加载时段间空隙把 \`r-xp\` 拆成独立组），回退取含 \`r-xp\` 的组，并以该副本 \`off==00000000\` 段（start <= x 段起点者）的起点为基址（与主程序 \`FileOffToVa\` 语义一致）。"
    emit ""
    emit "\`\`\`"
    printf '%s\n' "$GRPINFO" >> "$TMP"
    printf '%s\n' "$PICKLINE" | sed 's/^/PICK /' >> "$TMP"
    emit "\`\`\`"
    emit ""
    emit "- ${BASEMSG}"
    emit "- **本机将选中的基址 = 0x${BASE}**（若进程重启地址会变，此处只用于判「选的是哪一组」）"
    emit ""
    case "$BASEMSG" in
      *"组数=1"*) emit "- 解读：**单映射**（模拟器/部分机型）⇒ 无所谓选哪组，判据天然正确。" ;;
      *"组数="*) emit "- 解读：**多映射**（真机 dual：只读影子 + 执行真身）⇒ 选中组必须是带 r-xp/rw-p 的那个组，否则会重演 2026-10-04 v45 事故（基址取到影子 ⇒ hook 打在没人执行的副本上 ⇒ 四项功能全灭）。" ;;
      *) emit "- 解读：无法判定（映射为空）。" ;;
    esac
    emit ""
  else
    emit "（进程未运行或没有 libil2cpp 映射，跳过；加 \`--launch\` 或先手启动游戏）"
    emit ""
  fi
  emit "## 5. 关键 RVA 可达性（内存字节 vs 文件字节）"
  emit ""
  if [ -n "$RVAINFO" ]; then
    emit "| 名称 | RVA | 绝对地址 | 所在段权限 | 是否在选中组 | 内存 16 字节 | 文件 16 字节 | 一致性 |"
    emit "|---|---|---|---|---|---|---|---|"
    printf '%s\n' "$RVAINFO" | while read -r _l; do
      # 行格式：RVA <名> rva=0x.. addr=0x.. seg=0x.. perm=.. fileoff=0x.. path=.. inbest=..
      rn="$(printf '%s' "$_l" | awk '{print $2}')"
      rv="$(printf '%s' "$_l" | awk '{print $3}' | cut -d= -f2)"
      ad="$(printf '%s' "$_l" | awk '{print $4}' | cut -d= -f2)"
      pm="$(printf '%s' "$_l" | awk '{print $6}' | cut -d= -f2)"
      fo="$(printf '%s' "$_l" | awk '{print $7}' | cut -d= -f2)"
      pa="$(printf '%s' "$_l" | awk '{print $8}' | sed 's/^path=//')"
      ib="$(printf '%s' "$_l" | awk '{print $9}' | cut -d= -f2)"
      memhex="-"; filehex="-"; same="-"
      if [ "$pm" != "-" ] && [ -n "$PID" ]; then
        memhex="$(dev_hex "/proc/$PID/mem" "$((ad))" 16)"
        [ -n "$memhex" ] || memhex="(读不到)"
        if [ "$fo" != "-" ] && [ "$pa" != "-" ] && [ -n "$pa" ]; then
          filehex="$(dev_hex "$pa" "$((fo))" 16)"
          [ -n "$filehex" ] || filehex="(读不到)"
        fi
        if [ "$memhex" != "(读不到)" ] && [ "$filehex" != "(读不到)" ]; then
          if [ "$memhex" = "$filehex" ]; then same="一致（原样）"; else same="**不一致（内存已解密/改写）**"; fi
        fi
      fi
      emit "| ${rn} | rva=${rv} | addr=${ad} | perm=${pm} | inbest=${ib} | \`${memhex}\` | \`${filehex}\` | ${same} |"
    done
    emit ""
    emit "解读：RVA 必须落在 **r-xp** 段且 \`是否在选中组=yes\`；内存与文件字节不一致是正常的（IL2CPP 的代码段可能在装载后被解密/改写），一致则说明读到的是原样副本。两者都不影响结论，**关键是段权限与所属组**。"
  else
    emit "（未传 --rva，跳过；例：\`--rva interp=0x207F798 --rva pushframe=0x207DCCC\`）"
  fi
  emit ""
  emit "## 6. 迭代快路（原地改已装 apk）可行性提示"
  emit ""
  emit "| 项 | 值 |"
  emit "|---|---|"
  emit "| ro.apk_verity.mode | ${VERITY_VAL}（空=该属性不存在；非空即说明参与 APK 校验，原地改包风险↑） |"
  emit "| 页大小 | ${PAGE_SIZE} |"
  emit "| extractNativeLibs | $([ "$LIBDIR_LISTED" = "1" ] && echo 静态推断=true（lib/arm64 目录已展开到磁盘） || echo 静态推断=false（lib 仍在 apk 内；以 manifest 为准）) |"
  emit ""
  emit "- **D3（原地改已装 apk 内的 so）判定**：$([ "$VERITY_VAL" = "enforcing" ] && echo "**本机不可用** —— ro.apk_verity.mode=enforcing ⇒ apk 受 fs-verity 保护，改写盘上字节会在读时 EIO；只能走「同签重打 + 全量安装」。" || echo "待实测：先备份原包，做一次单条目原地写 + 冷启自检，读通才允许用于自测快路。")$([ "$LIBDIR_LISTED" = "1" ] && echo "（另：lib 已展开到磁盘，改 so 其实只需替换 /data/app/.../lib/arm64/ 下的文件，但仍会破坏 apk 签名校验，需实测确认进程是否重校验。）" || echo "") 

> 未验证前**禁止**用作交付路径；要启用必须先做「带备份的一次原地写实测」，且**交付永远走同签重打 + 全量安装**。"
  emit ""
  emit "## 7. 运行时真值怎么拿（本文件只是静态预判）"
  emit ""
  emit "1. 装机日志三行自检（缺一行 = 没装上，见 \`skill/re-implement/hook-rules.md\` §30.3）："
  emit "   \`interp base=0x… entry=0x… rd=1 rva=0x…\` / \`DobbyHook rc=0 errno=0 orig=0x…\` / \`armed\`"
  emit "2. 菜单 SELFCHECK 行（§31 规范）：一行给出 base / rd / hook / interp / path / maps 组数；"
  emit "3. 先跑 \`bash 04_scripts/_common/post_install_precheck.sh --game <游戏> --serial ${SERIAL}\`，预检全过才请用户上手。"
  emit ""
  emit "## 8. 原始证据"
  emit ""
  emit "\`\`\`"
  emit "(pidof ${PKG:-<pkg>}) = ${PID:-空}"
  printf '%s\n' "$MAPS_LIB" | head -12 >> "$TMP"
  emit "\`\`\`"
} 2>/dev/null

if [ "$STDOUT_ONLY" = "1" ]; then
  cat "$TMP"
else
  mkdir -p "$(dirname "$OUT")"
  cp "$TMP" "$OUT"
fi

# ---------- stdout 紧凑摘要 ----------
echo "[devprof] serial=${SERIAL} abi=${ABI} page=${PAGE_SIZE:-?} android=${REL:-?}"
[ -n "$PKG" ] && echo "[devprof] pkg=${PKG} ver=${PKG_VER:-?} pid=${PID:-未运行} apk=${PKG_PATH:-?}"
echo "[devprof] libil2cpp 组数=${GROUPCOUNT:-0} 基址=0x${BASE:-?} 判定=${VERDICT}"
[ -n "$BASEMSG" ] && echo "[devprof] ${BASEMSG}"
[ -n "$RVAINFO" ] && printf '%s\n' "$RVAINFO" | sed 's/^RVA /[devprof] RVA /'
[ "$QUIET" = "1" ] || [ "$STDOUT_ONLY" = "1" ] || echo "[devprof] 完整画像 → ${OUT}"
exit "$RC"
