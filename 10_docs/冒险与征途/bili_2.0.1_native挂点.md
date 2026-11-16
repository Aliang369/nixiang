# bili 2.0.1 libil2cpp native 挂点（解释器入口 / push-frame / 共享桩）

> 目标：`com.my.mxyzt.bilibili` 2.0.1，`lib/arm64-v8a/libil2cpp.so`
> 文件：`13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so`，87,013,984 B（0x52FBA60），md5 `2e088d84e71273516de0403f490140ed`
> metadata：`13_scratch/bili_201/assets/bin/Data/Managed/Metadata/global-metadata.dat`，13,324,172 B，sanity `0xFAB11BAF` / version **29**
> 方法：纯静态（自写 AArch64 解码器 + `llvm-objdump` 22.1.8 复核 + `.eh_frame_hdr` FDE 区间 + `.rela.dyn` 直方图 + Il2CppDumper），无 adb、无装机、无 git 提交
> 证据分级：**PROVEN（实测）** / **推断** / **待验证**

---

## 一、结论（先行）

| 项 | 新 RVA | 与旧（1.5.9）差值 | 运行时 vaddr（base 0x400023200000） | 可信度 |
|---|---|---|---|---|
| **解释器入口**（MethodInfo 分发主入口） | **0x207F798** | **0** | 0x40002527F798 | **PROVEN** |
| **push-frame**（帧初始化 A，调 AllocStackSlot+AllocFrame） | **0x207DCCC** | **0** | 0x40002527DCCC | **PROVEN** |
| **共享桩**（NotSupportNative2Managed 抛异常桩） | **0x207F04C** | **0** | 0x40002527F04C | **PROVEN**（桩身份）；「热更方法 methodPointer 全指向它」＝**待验证**（见 §5） |
| 附带：帧初始化 B | 0x207DE04 | 0 | 0x40002527DE04 | PROVEN |
| 附带：AllocStackSlot / AllocFrame | 0x207DD40 / 0x207DDAC | 0 | 0x40002527DD40 / …DDAC | PROVEN |

**「旧 RVA 能否沿用」＝可以，三个地址原样沿用。** 依据：段表逐字节同构 + 旧记录反汇编样例 234/236 条逐字节一致（另 2 条只是指向 `.rodata` 串的 ADD 立即数）+ 调用点地址/数量与旧记录完全重合 + FDE 区间把 16 个 push-frame 调用点全包在入口函数体内。

---

## 二、段表同构（第一层证据）

新版 ELF 与旧记录 `il2cpp_sections.md` 的 PT_LOAD / 节头**地址、大小全部相同**（section 名、addr、off、size 逐项比对，无一处 DIFF）：

| PT_LOAD | file_off | vaddr | filesz | delta |
|---|---|---|---|---|
| R--  | 0x0 | 0x0 | 0x1D7308C | 0x0 |
| R-X  | 0x1D7308C | 0x1D7408C | 0x2F8B204 | 0x1000 |
| RW-  | 0x4CFE290 | 0x4D00290 | 0x347388 | 0x2000 |
| RW-  | 0x5045618 | 0x5048618 | 0x2B5AB0 | 0x3000 |

`.text` = vaddr 0x1D7408C / off 0x1D7308C / size 0x55267C（与旧完全一致）；`e_entry` 0x1D7408C 亦一致。
⇒ 换算姿势沿用旧结论：**`.text` 内 file_off = RVA − 0x1000**；llvm-objdump 直接吃 RVA。

---

## 三、逐字节比对（第二层证据：量化差异）

旧 so 已不在本机（仅存 md5 `ab8935f6…`），无法整段 diff。改用**旧记录里的逐条反汇编样例**（`interp_entry_disasm.txt`：入口头、帧初始化 A/B、AllocFrame、AllocStackSlot、异常链、AOT 桩）与新 so 同 RVA 的字节比对：

- 可定位指令 **236 条，逐字节一致 234 条（99.2%）**；
- 唯 2 条不一致，且都是**指向 `.rodata` 字符串的 ADD 立即数**：
  - RVA 0x207DD9C：`add x0,x0,#0x929` → `#0x94D`（`AllocStackSlot` 串由 0xCF2929 移到 **0xCF294D**，+0x24）
  - RVA 0x20C9E78：`add x2,x2,#0xBD8` → `#0xFD0`（另一处串位移 +0x3F8）

**判读**：`.text` 代码布局（函数位置、函数体指令、调用目标编码）**完全等价**；本版差异集中在 `.rodata` 串池内容（若干字符串长度变化导致后续串整体平移，但 `.rodata` 总 size 与旧相同 0x57ACEB）。
⇒ 差异字节比例（可测样本内）**≈0.8%**，差异**全部集中**在「指向字符串的立即数字段」，**无一落在解释器相关函数体逻辑**。

---

## 四、解释器入口 0x207F798（PROVEN）

### 4.1 函数区间（`.eh_frame_hdr` FDE 表，294,526 条 FDE 二分查找）

入口 FDE 起点 **0x207F798**，下一条 FDE 起点 **0x208D9B8** ⇒ 函数体 **0x207F798..0x208D9B8（长 0xE220 = 56.5 KB，14,472 条指令）**，全库唯一、无内嵌子函数边界。

### 4.2 序言（与旧记录逐字节一致）

```
207f798: a9ba7bfd  stp x29, x30, [sp, #-0x60]!
207f79c: a9016ffc  stp x28, x27, [sp, #0x10]
207f7a0: a90267fa  stp x26, x25, [sp, #0x20]
207f7a4: a9035ff8  stp x24, x23, [sp, #0x30]
207f7a8: a90457f6  stp x22, x21, [sp, #0x40]
207f7ac: a9054ff4  stp x20, x19, [sp, #0x50]
207f7b0: 910003fd  mov x29, sp
207f7b4: d13103ff  sub sp, sp, #0xc40          ; 3.1KB 栈帧（解释器帧池专用）
207f7b8: 910003f3  mov x19, sp
207f7bc: d53bd048  mrs x8, TPIDR_EL0           ; __stack_chk_guard
207f7c0: f9001e68  str x8, [x19, #0x38]
207f7c8: aa0203f4  mov x20, x2                  ; ret 缓冲
207f7cc: aa0103f5  mov x21, x1                  ; args 槽区
207f7d0: aa0003f6  mov x22, x0                  ; MethodInfo*
207f7d8: 97ff81cb  bl 0x205ff04                ; pthread_getspecific 取每线程帧池对象
207f7ec: f9402ec8  ldr x8, [x22, #0x58]         ; 读 MethodInfo+0x58
207f7fc: 94003aeb  bl 0x208e3a8                ; 懒加载 / 方法信息补齐
207f820: 97fff979  bl 0x207de04                ; 帧初始化 B
```

### 4.3 它是「按 MethodInfo 分发的解释器主入口」的三条判据

1. **AOT 解释器桩群（最强）**：全库 `bl 0x207F798` 共 **5,221 处**，其中 **5,219 处**密集落在 **0x21C7960..0x220BA00** 的短桩函数群；样例 0x21C7950：
   `sub sp,sp,#0x20; str x30,[sp,#0x10]; add x1,sp,#0x18; add x2,sp,#0xC; bl 0x207F798; ldrb w0,[sp,#0xC]; ret` —— 典型「AOT 方法 → 交解释器执行 → 把返回值写回 ret 缓冲」胶水桩（HybridCLR 热更解释执行标志）。
2. **帧管理语义**：入口在 0x207F7D8 取每线程帧池对象 → 0x207F820 调帧初始化 B（0x207DE04），另有 **16 处** `bl 0x207DCCC` 帧初始化 A（见 §6）；两者再调 `AllocStackSlot`(0x207DD40，8 B 步长) / `AllocFrame`(0x207DDAC，0x40 B 步长)，失败抛 `System.StackOverflowException`，消息串就是函数名。
3. **分发循环**：函数体内 **9 处 `br xN` 间接跳转**（0x207F8A0 / 0x208530C / 0x20857C8 / 0x2089FC4 / 0x208A048 / 0x208A95C / 0x208B0C0 / 0x208B194 / 0x208B98C）= IL opcode switch 分发；回边 `b 0x207F880` 等构成主循环。

### 4.4 直接调用者（全部）

| 调用点 | 所在函数（FDE） | 性质 |
|---|---|---|
| 0x207F640 | 0x207F5B8..0x207F798（长 0x1E0） | 入口紧邻前的 C++ 包装（x1=槽区、x2=ret） |
| 0x208E018 | 0x208DF54..0x208E04C（长 0xF8） | 入口后的 C++ 包装（调 0x208EBEC 后交解释器） |
| 0x21C7960..0x220BA00（5,219 处） | AOT 桩群 | 热更/AOT 方法胶水桩 |

---

## 五、共享桩 0x207F04C（PROVEN 身份，运行期性质待验证）

### 5.1 静态事实

- FDE 显示 0x207F04C 是**独立小函数**（下一条 FDE 起点 0x207F064，长度 **0x18 = 24 B**）：
  ```
  207f04c: f81f0ffe  str x30, [sp, #-0x10]!
  207f050: d0ff6740  adrp x0, 0xd69000
  207f054: 9126b000  add  x0, x0, #0x9ac      ; = 0xD699AC  "NotSupportNative2Managed"
  207f058: 94012b52  bl   0x20c9da0           ; 构造托管异常
  207f05c: aa1f03e1  mov  x1, xzr
  207f060: 940125ef  bl   0x20c881c           ; __cxa_throw（noreturn）
  ```
  紧随其后的 0x207F064 同构，串是 `"NotSupportAdjustorThunk"`（0xD0DE4A）。
- 两个串在全库**各只有 1 处代码 xref**：`adrp+add @0x207F050`、`adrp+add @0x207F068`（自写解码器扫 12,463,233 条可执行段指令，唯一命中）。
- 两个桩的地址在 `.got` 里有指针槽（`.rela.dyn` R_AARCH64_RELATIVE addend=0x207F04C 命中 1 条，r_offset=0x5045DC0；0x207F064 → 0x5045DC8）⇒ 属「可被取地址的全局函数」，不是内联冷路径。
- 全库**没有任何 `bl` 指向 0x207F04C**（0 处调用点），符合「只作为函数指针/方法指针目标」的用法。

⇒ **0x207F04C 就是旧版文档记录的同一个桩**（旧值 0x207F04C，差 0；旧版动态实测 `PREFLIGHT Hurt5 fn=0x40002527f04c`）。

### 5.2 metadata 交叉验证：结论是「该方法不适用」，不是「失败」

Il2CppDumper 跑通后（见 §7）从 `dump.cs` 做 RVA 直方图：

- 带 RVA 的方法 **206,005 个，RVA==0 的 0 个，最频繁 RVA 的 count = 1** ⇒ **所有 AOT 方法各有独立代码地址，不存在「多方法同址」**；
- 0x207F04C / 0x207F798 / 0x207DCCC 在 dump.cs 中命中 **0** 次；
- `dump.cs` 的 image 表共 **88 个**，只有 `Assembly-CSharp-firstpass.dll`，**没有 `Assembly-CSharp.dll`**；四项功能相关的 6 个标识符（`DS_Advertising`/`PlayADVideo`/`CtagHurtHPPara`/`StarsCollider`/`ActionCommand_AddProjectile`/`DoAttack`）在 dump.cs 与 global-metadata.dat 里**全部 0 命中**。

⇒ 热更程序集根本不在 `global-metadata.dat` 中（HybridCLR 形态），其 `methodPointer` 是**运行期**由解释器填成 NotSupportNative2Managed 桩，**静态 metadata 无法交叉验证**。
**因此「逐方法 hook 不可行」在本版＝「推断」（依据：旧版动态实测 + 本版 so 结构同构 + 桩身份一致），需真机读 `MethodInfo->methodPointer` 才能升 PROVEN（缺设备侧证据）。**

---

## 六、push-frame 0x207DCCC 的调用点（PROVEN）

- FDE：0x207DCCC 是函数起点，长度到 0x207DD40（下一条 FDE）⇒ 0x74 B，序言 `f81d0ffe str x30,[sp,#-0x30]!`，与旧记录逐字节一致。
- 全库 `bl 0x207DCCC`：**16 处**，地址 **0x208AD70 / 0x208B02C / 0x208B060 / 0x208B130 / 0x208B1F8 / 0x208B630 / 0x208B660 / 0x208B690 / 0x208B794 / 0x208B8C0 / 0x208B9DC / 0x208BA98 / 0x208BB34 / 0x208BCD4 / 0x208BD24 / 0x208BD78**。
- 16 处**全部落在入口 FDE 区间 0x207F798..0x208D9B8 内**（最远 0x208BD78 ≪ 0x208D9B8）⇒ **全部在解释器体内**，单簇（最大间隔 < 0x1000），与旧记录的 16 处地址**逐一相同**。
- 另：旧版内存 dump 对照（`exec_diff_regions.txt`）显示 `.text` 内**唯一**被壳运行期改写的位置是 **file 0x0207CCCC（= RVA 0x207DCCC）前 4 字节**（`b` 指令级 inline hook）——独立佐证 0x207DCCC 是壳实际下钩的 push-frame。

---

## 七、Il2CppDumper 运行结果（成功）

```
cd 01_tools/Il2CppDumper && ~/.dotnet/dotnet Il2CppDumper.dll <libil2cpp.so> <global-metadata.dat> 06_dumped/冒险与征途/bili_2.0.1_il2cpp
```

输出（关键行）：

```
Metadata Version: 29
Initializing il2cpp file... Applying relocations...
WARNING: find JNI_OnLoad
ERROR: This file may be protected.        <-- 仅告警，继续
Il2Cpp Version: 29 ; Change il2cpp version to: 29.1
CodeRegistration : 4d004f8
MetadataRegistration : 4e836e0
Dumping... Done! ; Generate dummy dll... Done!
```

产物（`06_dumped/冒险与征途/bili_2.0.1_il2cpp/`）：`dump.cs` 31,135,612 B、`DummyDll/`（91 项）、`script.json`（`script.json`/`il2cpp.h`/`stringliteral.json` 由 `.gitignore` 忽略，均**未入库**）。
本机 `dotnet` 不在 PATH，实际路径 `/Users/aliang/.dotnet/dotnet`（Microsoft.NETCore.App 6.0.36 / 10.0.10）。

---

## 八、与旧 RVA 的差值表

| 项 | 旧 1.5.9 | 新 2.0.1 | Δ |
|---|---|---|---|
| 解释器入口 | 0x207F798 | 0x207F798 | **0** |
| push-frame A | 0x207DCCC | 0x207DCCC | **0** |
| 帧初始化 B | 0x207DE04 | 0x207DE04 | 0 |
| AllocStackSlot | 0x207DD40 | 0x207DD40 | 0 |
| AllocFrame | 0x207DDAC | 0x207DDAC | 0 |
| 共享桩（N2M 抛异常） | 0x207F04C | 0x207F04C | 0 |
| 串 `AllocFrame` | 0xD1CA18 | 0xD1CA18 | 0 |
| 串 `AllocStackSlot` | 0xCF2929 | **0xCF294D** | **+0x24**（唯一实测差异，且在 .rodata） |

---

## 九、不确定点 / 缺的证据

1. **「热更方法 methodPointer 全指向 0x207F04C」为待验证**：静态只能证明桩身份与地址未变；运行期性质需真机读 `MethodInfo->methodPointer`（或 logcat 打点）确认。缺：设备侧一次取值，或 `pipeline/pre_install_gate` 授权后的实测。
2. **入口 C++ 精确签名未定**（旧版同样未定）：观测 3 入参 `(MethodInfo* a0, void* args a1, void* ret a2)`，调用者约定一致（包装与 AOT 桩都把 ret 缓冲放 x2），但无符号/字符串直接标注函数名 —— 与旧报告结论相同，标「推断」。
3. **`.text` 全段逐字节 diff 无法做**（旧 so 不在本机，只有 md5）。已用「旧记录反汇编样例 236 条 + 调用点地址集合 + FDE 边界」三重间接比对替代；如需铁证，需重新取得 1.5.9 原包 `libil2cpp.so`（md5 `ab8935f68619ff4715d0b3664888e356`）做整段 sha1 分块比对。
4. 本报告**未**做任何 hook 可行性/时序验证，**未**装机、**未**提交 git。

---

## 十、产物与复现

| 文件 | 内容 |
|---|---|
| `10_docs/冒险与征途/bili_2.0.1_native挂点.md` | 本报告 |
| `06_dumped/冒险与征途/bili_2.0.1_il2cpp/dump.cs` | Il2CppDumper 全量 dump（31 MB，gitignore） |
| `06_dumped/冒险与征途/bili_2.0.1_il2cpp/DummyDll/` | 重建托管程序集 |
| `06_dumped/冒险与征途/bili_2.0.1_il2cpp/_scripts/_scan_new.py` | ELF 段表 / 字符串定位 |
| `…/_scripts/_xref_new.py` | ADRP+ADD/LDR 数据引用扫描（自写解码器） |
| `…/_scripts/_blscan.py` | 全库 BL/B/B.cond 调用点统计 → `_callers.json` |
| `…/_scripts/_fde_starts.py` | `.eh_frame_hdr` FDE 起点表 → 函数区间判定 |
| `…/_scripts/_reloc_hist.py` | `.rela.dyn` RELATIVE addend 直方图（找共享桩指针槽） |
| `…/_scripts/_rva_hist.py` | dump.cs RVA 直方图（流式） |
| `…/_scripts/_verify.py` | 旧记录样例 vs 新 so 逐字节比对 + NotSupport* 串 xref |

```bash
OD=/opt/homebrew/opt/llvm/bin/llvm-objdump ; SO=13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so
$OD -d --start-address=0x207f798 --stop-address=0x207f860 "$SO"     # 入口序言
$OD -d --start-address=0x207dca8 --stop-address=0x207de60 "$SO"     # push-frame + AllocFrame/AllocStackSlot
$OD -d --start-address=0x207f04c --stop-address=0x207f07c "$SO"     # 共享桩（NotSupportNative2Managed）
cd 06_dumped/冒险与征途/bili_2.0.1_il2cpp/_scripts && python3 _fde_starts.py   # 函数区间
```
