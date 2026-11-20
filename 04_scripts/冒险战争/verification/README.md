# 冒险战争（com.mxzz.zbml / mxzz）静态反推与成品验证工具

只读工具。不写任何 dll，不碰设备。移植自 `04_scripts/霸海争锋/verification/`（MemberRef 解析 + IL 后缀对照），
按冒险战争需要新增 `baseline` / `xref` 两个子命令，并把游戏路径、DummyDll、目标方法/字段、前缀长度全部参数化。

## 构建

本机**没有系统 .NET SDK**（`dotnet --list-sdks` 为空），用仓库内便携 SDK：

```powershell
pwsh -NoProfile -File 04_scripts/冒险战争/verification/build.ps1
# 产物: 04_scripts/冒险战争/verification/bin/Release/net8.0/advwar_static.dll
# 换 Mono.Cecil: -pwsh ... build.ps1 -Cecil <Mono.Cecil.dll>
```

运行时统一用便携 SDK 的 dotnet：
```powershell
$exe = '04_scripts\冒险战争\verification\bin\Release\net8.0\advwar_static.dll'
& .\01_tools\dotnet-sdk8\dotnet.exe $exe <子命令> ...
```

## 子命令

### baseline —— 方法基线反推
```
baseline <dll> [--method Type::Name] [--out il.txt] [--report sum.txt] [--dummy <DummyDll>] [--field Type::Field]
```
输出：文件大小/sha256/mtime、程序集名+版本+MVID、AssemblyRef 全集、
目标方法 token（0x0601xxxx）、完整签名（含参数名）、IL 数、locals 数（含类型）、EH 数、MaxStack、
InitLocals、CodeSize、完整 IL 文本（`--out`）、目标字段 token/属性/序号/声明类型布局、
GetPlayer / GetCamp 的全部重载、所有 Camp 相关枚举值。
默认 `--method Battle.Life::AddHP`、`--field Battle.Life::_curHP`。

### xref —— 调用点/字段站点交叉引用
```
xref <dll> [--method Type::Name] [--field Type::Name] [--window 8] [--out x.txt]
```
- 目标方法的全部 call/callvirt/newobj/ldftn 站点，带每个站点前 N 条 IL 上下文；
- 每个调用方方法内「阵营相关引用」计数（`*Camp*` 方法名 / `E_CAMP` 字段）与首个位置；
- 线索标记：`NEG_CONST`（负常数）、`NEG_OP`（neg）、`SUB/NEG_METHOD`（op_Subtraction/op_UnaryNegation）、`COND_GUARD`（近邻条件跳转越过该调用）；
- 目标字段全部 ldfld/stfld/ldflda 站点（按种类计数），并单列**目标方法内部的写入点**。

### verify —— 成员引用全解析（成品门禁 1）
```
verify <dll> [--type <regex>] [--method Type::Name] [--verbose] [--max-print 50] [--dummy <dir>]
```
每条 call/callvirt/newobj 的 MemberRef 解析到真实程序集，核对 scope / HasThis / 返回类型 / 参数类型。
分层计数：`ok / missing / sigbad / skipped(generic/typespec 未校验) / unresolved(程序集解析失败)`，
另对 `System.Private.CoreLib` / `netstandard` / `System.Runtime` 引用单独告警（旧事故：误引 CoreLib 导致运行期炸）。
`missing + sigbad + unresolved == 0 且 suspicious == 0` 才 exit 0。

### compare —— 成品反推（成品门禁 2）
```
compare <orig.dll> <patched.dll> [--method Type::Name] [--prefix auto|<int>] [--out cmp.txt]
```
1. 方法元数据对照（token / 签名 / IL 数 / locals / EH / MaxStack / InitLocals）；
2. **前缀长度 auto 穷举**：对所有 k 检查 patched[k..] 与原指令序列逐条一致（op / operand / 分支目标按所在列表相对索引归一）；
3. 前缀 IL 全文 + 可达性（`ret/throw` 早退 = 早退式补丁；`br` 越过后缀 = 需人工确认）；
4. **INLINE-DIFF（LCS）**：无纯前缀候选时判断是否「纯插入」——原指令序列是否被改写；
5. locals 前缀一致 + 新增 locals 列表；EH 按前缀平移后逐项一致；
6. AssemblyRef 集合增删；types/methods 数量差（防顺手重编译）。
判定：`PREFIX-IDENTICAL` / `INSERTION-ONLY` / `DIFFERENCES-FOUND`。exit 0 = 前两者。

## 退出码
| code | 含义 |
|---|---|
| 0 | baseline 找到目标 / xref 有站点 / verify 通过 / compare 通过 |
| 1 | 未找到 / 有缺陷 / 有差异 |
| 2 | 用法错误 |
| 3 | 致命异常 |

## 默认路径解析
DummyDll 默认取 `$NIXIANG_ROOT/06_dumped/冒险战争/1.1.036_il2cpp/dump/DummyDll`；
`NIXIANG_ROOT` 未设置时从可执行文件所在目录向上找同时含 `04_scripts` 与 `06_dumped` 的目录。
换游戏/换版本一律用 `--dummy`、`--refdir` 覆盖，不改代码。

## 门禁脚本（2026-11-19 新增，起因：b2 漏打 libc++_shared.so）

### libclosure.ps1 —— 原生库依赖闭包
```powershell
pwsh -NoProfile -File 04_scripts/冒险战争/verification/libclosure.ps1 -Path '<apk>' [-Ignore libA,libB] [-Quiet]
```
对 `lib/<abi>/` 下每个 `.so` 用 `llvm-readelf -d` 读**真实 DT_NEEDED**（不是字符串命中），
逐条要求在「同 ABI 目录内 app 库」或「系统库白名单」里存在，缺一即 FAIL，exit 1。
`-Ignore` 用于已知动态 dlopen 的既有依赖（例：无限升级的 libti-monitor.so → libplt-unwind.so），命中记 WARN 不 FAIL。
实测对照：冒险战争 b2 = FAIL（libstarry_magic.so 缺 libc++_shared.so）；无限升级 13.101 = PASS（含 -Ignore）。

### nativeload_check.ps1 —— native 真加载校验（吃 logcat）
```powershell
pwsh -NoProfile -File 04_scripts/冒险战争/verification/nativeload_check.ps1 -Log '<logcat.txt>' [-SoName libstarry_magic.so]
```
FAIL 特征：`No implementation found`、`is the library loaded`、`dlopen failed`、`UnsatisfiedLinkError`、
`暂无功能列表`、`native ensureLoaded=false`。
PASS 需出现装载成功证据（`loaded ... .so` / `System.load ...` / `nativeInit ok` / featureList ready）。
**只有 MenuService/overlay 行 ⇒ exit 3 INCONCLUSIVE，不算通过**（这正是 b2 事故的判据缺口）。
