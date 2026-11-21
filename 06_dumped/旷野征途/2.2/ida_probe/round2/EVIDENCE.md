# HybridCLR 解释器候选 · 第2轮语义证据（2026-11-14 18:24）

映射：.text file 0x1748604 → va 0x174c604（va−file=0x4000）。下列 VA 与 file 均按此换算。
工具：capstone 5.0.7 直读 libil2cpp.so；函数边界取自 idat headless 结果 funcs.txt（63 个边界）。

## 候选 ① 0x1af5338 / file 0x1af1338（size 0x208，1 caller 0x1af52fc）
- prologue `sub sp,sp,#0x90` + 保存 x19..x27；`mov x20,x0` / `mov x19,x1` / `mov x23,x2` —— **三参分别留存**
- `ldr x8,[x20,#0x100]`、`add x0,x0,#0x38`、`bl 0x19b9bc8`（外部 helper）
- `str wzr,[x20,#0x3ec]` / 后续 `str w1,[x20,#0x3ec]`：向 arg0（组）写计数
- `ldr w8,[x23,#0xc]`、`ldr w9,[x23,#4]`、`sub w22,w8,w9`：arg2 当「栈/参数包」做指针差
- 调用：0x1af5068（AllocSlot 形态）×4、0x1af5540（opcode 执行体）、0x1af5868（引用释放）×2
结论：**与 1.8 的 EnterFrameFromInterpreter(group, methodInfo, argsPkt) 形参语义逐项吻合**，为当前最强候选。
待补证据：arg2 是否按 **16 字节槽位** 迭代（1.8 实测 argsPkt：+0 this / +8 arg0 / +16 arg1 / +24 arg2 / +32 arg3）。

## 候选 ② 0x1af5540 / file 0x1af1540（size 0x328，1 caller 0x1af5488）
- `ldr w8,[x2,#0xc]; add w10,w8,#2; str w10,[x2,#0xc]` —— arg2 计数器 **+2**（推进 2 个槽）
- `str wzr,[x0,#0x3fc]`、`str wzr,[x20,#0x40c]`、`str wzr,[x20,#0x41c]` —— 清帧字段
- `ldrb w8,[x1,#0x52]` —— 读 arg1（方法）标志位；`ldrb w25,[x8,w9,uxtw]` 取 opcode 字节，`cmp w25,#0x53` 分派
- 直接 `bl 0x1af5b3c` ⇒ 它是**上层推进/执行**，调用 ③
结论：不是入口；挂它会错层（argsPkt 的 x2 语义在此是栈指针而非参数包）。

## 候选 ③ 0x1af5b3c / file 0x1af1b3c（size 0x4c8，6 callers）
- 大帧 `sub sp,sp,#0x4c0` + 栈保护 `mrs tpidr_el0; str x8,[sp,#0x4b8]`（canary）
- 5 参：`w23=w4`、`x22=x3`、`x20=x2`、`x19=x1`、`x21=x0`
- `ldr w8,[x3,#8]; ubfx w9,w8,#0x10,#8; cmp w9,#0x4f` 后接 **跳转表**（table 0xe06000+0xf8c；`br x11`，值域 2..0x1d / >0x4f 分支）
- 自递归 ×3、AllocSlot ×4、错误路径 snprintf/abort 成对出现
结论：**按类型标签分派的执行体**，不是帧入口。

## 同期排除（避免重复劳动）
- 0x1af59a4（0x198）：读 `[x1]`/`[x1,#0xc]` 游标 + `ldrb` 取字节 + 按 2..0xf 分支 ⇒ **操作数读取器**。
- 0x1af5958（0x4c，14 callers）：`[x0,#8]/[x0,#0xc]` 容量+长度，必要时扩容 0x1afc4f0，末尾 `strb` 写一字节 ⇒ **单字节追加器**，非帧压栈。
- 0x1af5868（0x2c）：`[x8,#0x60] -= 1` 后调 0x1afcb8c ⇒ **引用计数释放**。
- 0x1af4e24（0x1d0）：`br x11` 跳转表 + ldc 类型分支 ⇒ **参数转换（convert_args）**。

## 结论与边界
- 当前唯一满足「三参语义 + 组内计数写 + MethodInfo 字段读」的候选是 **0x1af5338**；仍缺第 2 条交叉证据（16 字节槽迭代 / 运行期 MethodInfo 名称比对）。
- 因此：**不 hook、不写死地址**，战斗两项继续「未适配」。下一轮只需验证 0x1af5338 的 arg2 槽位迭代即可定论。

## 判据降级（2026-11-14 18:35，按 lead 更正）
- `str x8,[x19,#0x10]` 里的 **x19 = arg1**（在 0x1af5338 里 `mov x19,x1`），**不是 arg2**；不能当作 argsPkt 槽位步进证据。
- 0x1af5338 里 x2 被当作「有 +4 / +0xc 字段的结构」做差（`ldr w8,[x23,#0xc]; ldr w9,[x23,#4]; sub w22,w8,w9`），与「raw argsPkt 指针数组」不一致。
- ⇒ **0x1af5338 降级为「未证实」**，不得据此写 hook；未知 ABI 下禁止把 x1 当 MethodInfo 解析、禁止读 x2 前 5 槽做观测。

## 调用者反向追踪（0x1af52c0，0x1af5338 的唯一 caller）
```
0x1af52c0  sub sp,sp,#0x30; stp x30,x21/x20,x19
0x1af52cc  mov x19,x1          ; arg1 留存
0x1af52d0  adrp x1,#0x3c4f000; ldr x1,[x1,#0x170]   ; 取一个全局对象（互斥量/元数据注册）
0x1af52e4  bl 0x1991928        ; 加锁（x0=&sp+8）
0x1af52e8  ldrb w8,[x19,#4]; cbnz w8,#0x1af530c     ; arg1+4 是「已完成」标志位
0x1af52fc  bl 0x1af5338        ; 真正计算：传 (x0=arg0, x1=arg1, x2=arg2)
0x1af5304  mov w8,#1; strb w8,[x19,#4]              ; 回写完成标志（记忆化）
0x1af5310  bl 0x1972ea0        ; 解锁
```
判读：这是**双检锁 + 一次性初始化**外壳（对象上 +4 标志位 + 全局锁 + 结果记忆化）。
⇒ 0x1af5338 的语义更可能是「为某个 method/type 惰性创建内部描述（InterpMethodInfo / 类型元数据）」，**不像** EnterFrame（EnterFrame 每调用都会压帧，不应有一次性标志）。
⇒ 结论：**0x1af5338 从「最强候选」下调为「疑似初始化器」，仍非 EnterFrame**；EnterFrame 仍**未定位**。

## 观测 hook 方案：**撤回**
- 原计划（入口只读打印 method 名 + argsPkt 前 4 槽）在 ABI 未证前属「边猜边装」，按 lead 口径**作废**，不实现。

## 下一步（有界）
1. 待 review 用官方 HybridCLR 源码交叉（EnterFrameFromInterpreter 真实参数、AllocStackSlot 语义、InterpMethodInfo 创建路径）。
2. 若确认 0x1af5338 = CreateInterpMethodInfo，则转而在 **其唯一 caller 的上层**（0x1af52c0 的调用者）或 InterpMethodInfo 使用点找 EnterFrame。

## 有限调用图（2026-11-14 18:36）：初始化器 → 字段填充
```
0x1b2297c  add x1,x22,#0x18 ; mov x0,x20 ; mov x2,x21
0x1b22988  bl 0x1af52c0            ; 双检锁 + 惰性创建（0x1af5338 在里面）
0x1b2298c  ldr x1,[x20,#0x100]     ; 组内指针
0x1b22990  ldp x2,x3,[x22,#0x20]   ; 从 method 侧取两字段
0x1b22998  bl 0x19c6954            ; ★ 结构填充
```
`0x19c6954` 体（0x2c，唯一 caller 0x1b22998）：
```
stp x30,x19,[sp,#-0x10]!
mov x19,x0
str x1,[x0]          ; 字段0 = x1（上面传的 [x20,#0x100]）
str x2,[x0,#8]!      ; 字段1 = x2（method+0x20）
str x3,[x19,#0x10]   ; 字段2 = x3（method+0x28）
cbz x2 → 0 else bl 0x1b57608（计数/长度换算）
str w0,[x19,#0x18]   ; 字段3 = 换算结果
```
判读：这是**把 method 侧信息灌进一个描述结构**（method/argBase/argCount 三元 + 计算值），
与「0x1af5338 = 惰性创建 InterpMethodInfo」相互印证；二者都**不是**帧分配器。

## 帧分配器仍未定位（现有排除）
- 调 AllocSlot(0x1af5068) 的函数共 20 个点、5 个函数：0x1af5540(opcode 推进，9 次)、0x1af5338(初始化，4)、0x1af5b3c(类型分派，4)、0x1af4e24(convert_args，2)、0x1af6004(1)。
- 其中没有一个出现「以 group 为 x0、把 MethodInfo 与 argBase 成对写入帧槽」的 stp 形态 ⇒ 帧分配器可能在区外（0x1af4xxx 之外）或内联进上层。

## 口径更正（2026-11-14 18:42，按 lead）
- `0x1af5068` **仅形态上**像槽位分配（读写 `[x0,#8]/[x0,#0xc]` 容量+长度、满则调 `0x1afc4f0` 扩容、末尾 `strb` 写一字节）；**未由源码确证**为 AllocStackSlot。周围若全是参数/类型元数据构造，也可能是其它容器的 slot 分配。
- 原文「帧分配器在区外或已内联」**降级为假设**，不作为结论。
- 「未见 `stp` 成对写」**不排除**编译器把同样的两字段写成两条 `str`（或 `stp` 到别的基址寄存器）。
- 先前用「argBase / argCount」描述字段属**命名联想**，改为只写偏移：`[method+0x20]`、`[method+0x28]`、`[obj+0x18]`、`[obj+0x20]`。

## 再上一层（0x1b2293c，直接 caller 层）
```
0x1b2293c  sub sp,sp,#0x40; stp x30,x1,[sp,#0x10]; stp x22,x21 / x20,x19
0x1b2294c  adrp x2,#0xe27000; add x2,x2,#0x142      ; 常量指针（该址落在字面量串池中段，非可读 C 串）
0x1b22950  mov x20,x0                                ; 保存 arg0
0x1b22954  add x0,x0,#0x3a0                          ; arg0+0x3a0
0x1b22968  mov x19,x8                                ; ★ x8 = 结构返回缓冲（sret），本函数按值返回结构
0x1b2296c  bl 0x1b01404                              ; 以 (x0=arg0+0x3a0, x2=常量, x3=&sp+0x18, x4=&sp+8) 查询
0x1b22970  ldrb w8,[x0,#0x1c]                        ; 返回对象 +0x1c 是完成标志
0x1b2297c..0x1b22998                                 ; 惰性创建 → 用 [x20,#0x100] 与对象 +0x20/+0x28 填结构
```
- `BL` 扫描：**0 个直接调用者** ⇒ 经间接/虚调用进入，不能靠调用者列表继续上行；需要走 xrefs（IDA 已有数据库才有的间接引用）或运行期。
- 判读（只到形态）：该函数**按值返回一个描述结构**（sret=x8），内容取自 `arg0+0x3a0` 的查询结果与 `[arg0+0x100]`；仍属「获取/构造描述」，**不构成帧入口证据**。
- 未名函数不给肯定身份：0x1b01404、0x1b57608 均只记为「被调用」，不猜其名。
