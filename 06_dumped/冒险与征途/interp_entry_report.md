# 保护壳 hook 目标：HybridCLR 解释器入口 —— 定位结论报告

目标：`libil2cpp.so`（`E:\nixiang\06_dumped\冒险与征途\apk_extract\lib\arm64-v8a\libil2cpp.so`，87,013,984 B）
方法：纯静态只读（自写 AArch64 解码器 + `llvm-objdump` 复核），无 adb、无联网。

---

## 一、结论（一句话）

壳 `libSDKloading.so` DobbyHook 的目标 **就是 HybridCLR 解释器入口函数**：

| 项 | 值 |
|----|----|
| **解释器入口 RVA** | **`0x207F798`** |
| **运行时 vaddr** | **`0x40002527F798`**（= 0x400023200000 + 0x207F798） |
| 文件偏移 | 0x207E798（`.text`，delta = 0x1000） |
| 函数规模 | 栈帧 `0xC40`，成千上万条指令的大型 dispatch 循环 |
| 直接调用者 | **5,221 处 `bl`**（RVA 0x207F640、0x208E018 两处常规调用 + 0x21C7950…0x220BA00 连续 5,219 个 AOT 解释器桩） |

**证据强度：强。** 三条独立证据同时成立（字符串调用链闭合、AOT 桩群、帧池结构），且与本游戏壳自身日志的中文诊断串（`定位 AllocFrame/AllocStackSlot` → `找调用 AllocFrame 的地方` → `★解释器入口已定位`）描述的三步算法**完全一致**。

---

## 二、壳的算法 vs 本次实测结构

壳的诊断串描述的是三步：

```
① 找引用了字符串 "AllocFrame"/"AllocStackSlot" 的指令 → 定位这两个函数入口
② 找调用 AllocFrame 的地方
③ 再找那个函数的调用者 = 解释器入口 → DobbyHook
```

**实测结果与这三步完全对得上，不需要「硬套」**（这是本次最关键的复核点）：

| 壳的步骤 | 实测 |
|----|----|
| ① 字符串 xref | 两串各 **恰好 1 处** `ADRP+ADD`：`0x207DD98/9C`→`"AllocStackSlot"`、`0x207DDF0/F4`→`"AllocFrame"`。各自所在的函数 = `0x207DD40`(=AllocStackSlot)、`0x207DDAC`(=AllocFrame) |
| ② 调用 AllocFrame 的地方 | `0x207DDAC` 有 **2 个** `bl` 调用点：`0x207DD0C`（在 `0x207DCCC`）、`0x207DE48`（在 `0x207DE04`）—— 两个几乎同构的「帧初始化」重载 |
| ③ 那个函数的调用者 | `0x207DCCC` 的 16 个 `bl` 全部来自 `0x207F798`；`0x207DE04` 的唯一 `bl`（`0x207F820`）也来自 `0x207F798` |

⇒ **无论壳在②里挑哪一个调用点，③都会收敛到同一个 `0x207F798`。定位无歧义，与日志「★解释器入口已定位」自洽。**

一处与题面描述的细微差异（不影响结论）：两个函数名串**不是**独立调试串，而是被塞在 `.rodata` 的大串池中（前后是 il2cpp/HybridCLR 的 C++ 报错文本与 C# 类型名），且**只出现在这两个函数自己的「冷路径」异常构造块里**——即 `AllocFrame` 这个名字是其**内部抛栈溢出异常时的消息串**，不是外部引用。

---

## 三、调用链（含每级证据）

```
HybridCLR 解释器入口  0x207F798                 <== 壳的 DobbyHook 目标
   ├─ bl 0x207DE04  @0x207F820   帧初始化 B
   └─ bl 0x207DCCC  @0x208AD70 等 16 处          帧初始化 A
        └─ bl 0x207DD40 (AllocStackSlot)  @0x207DD04 / @0x207DE38
        └─ bl 0x207DDAC (AllocFrame)      @0x207DD0C / @0x207DE48
                └─ 冷路径: adrp/add → "AllocFrame" 串 → bl 0x20C9E60 → __cxa_throw
        └─ 冷路径: adrp/add → "AllocStackSlot" 串 → bl 0x20C9E60 → __cxa_throw
```

### 3.1 入口函数头（前 16 条指令，llvm-objdump 复核）

```
 207f798: a9ba7bfd      stp  x29, x30, [sp, #-0x60]!     ; 标准 prologue -> 真入口
 207f79c: a9016ffc      stp  x28, x27, [sp, #0x10]
 207f7a0: a90267fa      stp  x26, x25, [sp, #0x20]
 207f7a4: a9035ff8      stp  x24, x23, [sp, #0x30]
 207f7a8: a90457f6      stp  x22, x21, [sp, #0x40]
 207f7ac: a9054ff4      stp  x20, x19, [sp, #0x50]
 207f7b0: 910003fd      mov  x29, sp
 207f7b4: d13103ff      sub  sp, sp, #0xc40                    ; 3.1KB 栈帧
 207f7b8: 910003f3      mov  x19, sp
 207f7bc: d53bd048      mrs  x8, TPIDR_EL0                       ; 栈保护（__stack_chk_guard）
 207f7c0: f9001e68      str  x8, [x19, #0x38]
 207f7c4: f9401508      ldr  x8, [x8, #0x28]
 207f7c8: aa0203f4      mov  x20, x2                            ; arg2 保存
 207f7cc: aa0103f5      mov  x21, x1                            ; arg1 保存
 207f7d0: aa0003f6      mov  x22, x0                            ; arg0 保存（MethodInfo*/InterpMethodInfo*）
 207f7d4: f81f03a8      stur x8, [x29, #-0x10]
```

随后 `bl 0x205FF04`（`pthread_getspecific` 取每线程 0x68 字节帧池对象）→ 读 `[T+0xC]/[T+0x20]` → `bl 0x207DE04(dst=&local, x22, x21)` 建帧 → 进入巨型 dispatch 循环（回边 `b 0x207F880`、`b 0x208BA9C`、`b 0x2086900`）。

### 3.2 为什么它是 HybridCLR 解释器（不是别的）

| 证据 | 内容 |
|----|----|
| **AOT 桩群（最强）** | 入口有 **5,221 个 `bl` 调用者**，其中 5,219 个在 `0x21C7950`–`0x220BA00` 连续成百上千个**极短桩函数**里。桩样例 `0x21C7950`：`sub sp,sp,#0x20; str x30,[sp,#0x10]; add x1,sp,#0x18; add x2,sp,#0xc; bl 0x207F798; ldrb w0,[sp,#0xc]; ret` —— 典型的「AOT 方法 → 交解释器执行并把返回值写回 `ret` 缓冲」胶水桩。这正是 HybridCLR「AOT + 热更解释执行」的标志 |
| **帧管理语义** | 入口直接调用两个帧初始化函数，后者再调 `AllocStackSlot`/`AllocFrame`；`AllocStackSlot` 以 **8 字节**步长 bump 分配（= `StackObject` 大小），`AllocFrame` 以 **0x40 字节**步长 bump 分配帧 |
| **异常类型** | 两处冷路径最终尾调到 `0x20C9E60`，它把 `x1 = 0xCD6F49("System")`、`x2 = 0xD17BD8("StackOverflowException")`、`x3 = 消息串` 组装后交给 `0x20C881C`（`__cxa_allocate_exception(8)` + `__cxa_throw`，noreturn）。⇒ **帧/槽位分配失败时抛 `System.StackOverflowException`，消息就是函数名 `"AllocFrame"` / `"AllocStackSlot"`** —— 这是 HybridCLR 解释器帧池的专有行为 |
| **栈池对象布局（实测）** | 每线程对象 `T`（`0x205FF04` 经 `pthread_getspecific` 取得，首次分配 0x68 B）：`T+0x00` = `StackObject*` 槽数组（元素 8 B）、`T+0x0C` = 已用槽数、`T+0x10` = 槽容量、`T+0x18` = 帧数组（元素 0x40 B）、`T+0x20` = 已用帧数、`T+0x24` = 帧容量 |
| **符号侧** | `.dynsym`（2,388 条）**没有任何** `hybridclr`/`Interpreter`/`InterpFrame`/`Execute` 导出；只有 `il2cpp_*` 与 libc++。⇒ 解释器入口只能特征定位，与壳日志「定位方式=特征自动定位」「`[MI-API] 方式=自解析导出表`」一致 |

---

## 四、AllocFrame / AllocStackSlot 函数入口

### AllocStackSlot — RVA `0x207DD40`（运行时 `0x40002527DD40`）

```
207dd40: f81e0ffe  str  x30, [sp, #-0x20]!
207dd44: a9014ff4  stp  x20, x19, [sp, #0x10]
207dd48: 2941a808  ldp  w8, w10, [x0, #0xc]     ; w8=usedCount, w10=capacity
207dd4c: aa0003f3  mov  x19, x0
207dd50: 0b010109  add  w9, w8, w1              ; used + n
207dd54: 6b0a013f  cmp  w9, w10
207dd58: 5400014d  b.le 0x207dd80
207dd5c: f940026b  ldr  x11, [x19]              ; slot 数组指针
207dd60: b50000cb  cbnz x11, 0x207dd78
207dd64: aa1303e0  mov  x0, x19
207dd68: 2a0103f4  mov  w20, w1
207dd6c: 9400452c  bl   0x208f21c              ; 扩容 slot 数组
207dd70: 2941aa68  ldp  w8, w10, [x19, #0xc]
207dd74: 0b140109  add  w9, w8, w20
207dd78: 6b0a013f  cmp  w9, w10
207dd7c: 540000ec  b.gt 0x207dd98              ; 还是不够 -> 冷路径抛异常
207dd80: f940026a  ldr  x10, [x19]
207dd84: b9000e69  str  w9, [x19, #0xc]         ; usedCount += n
207dd88: a9414ff4  ldp  x20, x19, [sp, #0x10]
207dd8c: 8b28cd40  add  x0, x10, w8, sxtw #3    ; 返回 &slots[oldUsed]（步长 8）
207dd90: f84207fe  ldr  x30, [sp], #0x20
207dd94: d65f03c0  ret
207dd98: b0ff63a0  adrp x0, 0xcf2000            ; <<< 冷路径：字符串 "AllocStackSlot"
207dd9c: 9124a400  add  x0, x0, #0x929
207dda0: 94013030  bl   0x20c9e60
207dda4: aa1f03e1  mov  x1, xzr
207dda8: 94012a9d  bl   0x20c881c              ; __cxa_throw (noreturn)
```

### AllocFrame — RVA `0x207DDAC`（运行时 `0x40002527DDAC`）

```
207ddac: a9bf4ffe  stp  x30, x19, [sp, #-0x10]!
207ddb0: 29442408  ldp  w8, w9, [x0, #0x20]     ; w8=frameCount, w9=frameCapacity
207ddb4: aa0003f3  mov  x19, x0
207ddb8: 6b09011f  cmp  w8, w9
207ddbc: 540000cb  b.lt 0x207ddd4
207ddc0: f9400e68  ldr  x8, [x19, #0x18]        ; 帧数组指针
207ddc4: b5000168  cbnz x8, 0x207ddf0           ; 非空仍不够 -> 冷路径抛异常
207ddc8: aa1303e0  mov  x0, x19
207ddcc: 94004530  bl   0x208f28c              ; 分配帧数组
207ddd0: b9402268  ldr  w8, [x19, #0x20]
207ddd4: f9400e69  ldr  x9, [x19, #0x18]
207ddd8: 1100050a  add  w10, w8, #0x1
207dddc: 93407d08  sxtw x8, w8
207dde0: b900226a  str  w10, [x19, #0x20]       ; frameCount = w8+1
207dde4: 8b081920  add  x0, x9, x8, lsl #6      ; 返回 &frames[idx]（步长 0x40）
207dde8: a8c14ffe  ldp  x30, x19, [sp], #0x10
207ddec: d65f03c0  ret
207ddf0: f0ff64e0  adrp x0, 0xd1c000            ; <<< 冷路径：字符串 "AllocFrame"
207ddf4: 91286000  add  x0, x0, #0xa18
207ddf8: 9401301a  bl   0x20c9e60
207ddfc: aa1f03e1  mov  x1, xzr
207de00: 94012a87  bl   0x20c881c              ; __cxa_throw (noreturn)
```

> 注意：两个串的 xref **在函数 `ret` 之后的独立冷路径块里**，靠函数体内 `b.gt 0x207dd98` / `cbnz x8, 0x207ddf0` 跳入。
> 壳要从 xref 反推「函数入口」，必须**向前扫 prologue**；若壳按「xref 所在连续块」判函数，会把函数入口误判成 0x207DD98/0x207DDF0（错）。实测壳日志显示它成功，说明它用的是向前扫 prologue 的写法。

---

## 五、帧初始化中间函数（调 AllocFrame 的那两个）

| 函数 | RVA | 运行时 vaddr | 调用 AllocStackSlot | 调用 AllocFrame | 被谁调用 |
|----|----|----|----|----|----|
| 帧初始化 A | `0x207DCCC` | 0x40002527DCCC | `bl 0x207DD40 @0x207DD04` | `bl 0x207DDAC @0x207DD0C` | 16 处，全在入口 `0x207F798` |
| 帧初始化 B | `0x207DE04` | 0x40002527DE04 | `bl 0x207DD40 @0x207DE38` | `bl 0x207DDAC @0x207DE48` | 1 处：`@0x207F820`（入口） |

两者同构，差异在 A 用 `w1 = w9 - w8`（`[method+0x2C] - [method+0x20]`）算槽数、B 直接用 `w1 = [method+0x2C]`。典型是同一 `InterpFrame::Init` 的两个重载/内联变体。
A 的填充：`frame[0x00]=MethodInfo*`、`frame[0x08]=arg`、`frame[0x10]=[[pool]+0xC]`、`frame[0x18..0x37]=0`、`frame[0x38]=[method+0x20]` → 帧大小 0x40，与 `AllocFrame` 的 `lsl #6` 完全吻合。

---

## 六、`llvm-objdump` 的正确调用姿势（实测）

```powershell
$od = "C:\Users\Administrator\AppData\Local\Android\Sdk\ndk\27.0.12077973\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-objdump.exe"
$so = "E:\nixiang\06_dumped\冒险与征途\apk_extract\lib\arm64-v8a\libil2cpp.so"

# 正确：--start-address 直接吃 ELF 虚拟地址（= RVA = 节 sh_addr）
& $od -d --start-address=0x207F798 --stop-address=0x207F820 $so
```

- **本 so 的 `.text` delta = 0x1000，但 objdump 内部已按程序头处理，传 RVA 即可，绝对不要自己减 0x1000。**
- **不需要 `--adjust-vma`。** 加了反而会整体位移、输出伪指令。
- 自检：输出中 `207dd98:` 必须为 `adrp x0, 0xcf2000` 且下一行 `add x0, x0, #0x929`；`207ddf0:` 必须为 `adrp x0, 0xd1c000` 且下一行 `add x0, x0, #0xa18`。两条对上，寻址模型即正确。
- 该 so **无 `.symtab`**（已 strip），objdump 只能借用 `.dynsym` 里不相干的最近符号（如 `_ZNSt6__ndk117bad_function_callD0Ev+0x16528`、`DllCanUnloadNow+0x1afc`），**这些符号名全是误导，请忽略，只看地址**。

---

## 七、不确定点（NOT PROVEN / 待验证）

1. **「壳实际 hook 的就是 0x207F798」属强推断，非直接观测。** 依据是与壳自身三步诊断串完全自洽 + 入口唯一性（②的两个候选都收敛到它）。没有设备侧 DobbyHook 目标地址日志做交叉验证；如需铁证，应在设备上 `hook DobbyHook` 或读 frida 的 hook 记录，打印其传入的目标地址，比对是否为 `0x40002527F798`。
2. **入口函数的 C++ 精确签名未定。** 观测到 3 个入参 `(x0, x1, x2)`：AOT 桩把 `x1` 当返回缓冲、`x2` 当辅助字节缓冲；`x0` 被当作 `MethodInfo*` 使用（取 `[x0+0x58]`、`[x0+0x20]`，并 `bl 0x208E3A8` 做懒加载转换）。倾向 `Interpreter::Execute(MethodInfo*, void* args, void* ret)` 一类，但**没有**字符串/符号直接标注函数名，标「待验证」。
3. **`0x205FF04` 返回的每线程 0x68 字节对象**只确定了字段偏移与用途（槽池 + 帧池），未确定其类名；标「待验证」。
4. **`.rodata` 大串池的性质**未完全判定：既含 C# 类型/方法名（`<Module>`、`System.IO.Compression`、`UnityEngine.Vector4`、`MeshFusionPro...`），又含 il2cpp/HybridCLR 的 C++ 报错串。已确认**不是**内嵌 `global-metadata.dat`（魔数 `0xFAB11BAF` 全文件 0 命中）。
5. 本报告只做静态定位；**未**做任何 hook 可行性/时序验证，也**未**触及设备。

---

## 八、产物

| 文件 | 内容 |
|----|----|
| `il2cpp_sections.md` | ELF 程序头/节头 RV↔offset 映射表、**逐段 delta**（0 / 0x1000 / 0x2000 / 0x3000）、关键字符串地址、运行时换算 |
| `interp_entry_xrefs.txt` | 两串的唯一性与全部 xref 命中点（RVA + 文件偏移 + 反汇编行）、重定位/指针表负结果 |
| `interp_entry_disasm.txt` | 入口 / 两个帧初始化 / AllocFrame / AllocStackSlot / 异常链 / AOT 桩 的完整反汇编 + **逐条 objdump 命令原文** |
| `interp_entry_report.md` | 本文件 |

生成脚本（均 `_` 开头，只读）：`_elf_sections.py`、`_find_strings.py`、`_region_char.py`、`_scan_xrefs.py`、`_trace_xrefs.py`、`_callers.py`、`_gen_disasm.py`
