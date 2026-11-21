# 2.2 EnterFrame 定位（2026-11-14 19:0x）— 三特征 + 两独立交叉证据

映射：.text file 0x1748604 → va 0x174c604（va−file=0x4000）。

## 结论（候选，待 review 独立复核）
| 角色 | VA | file |
| EnterFrameFromInterpreter | **0x19a9004** | 0x19a5004 |
| EnterFrameFromNative | 0x19a913c | 0x19a513c |
| AllocStackSlot | 0x19a9078 | 0x19a5078 |
| PushFrame | 0x19a90e4 | 0x19a50e4 |

## 证据链
### A. 字符串 xref（唯一）
- `AllocStackSlot` @va 0xa0bcc0 → 唯一引用 0x19a90d0（raise 冷路径，紧邻 AllocStackSlot 本体 0x19a9078 之后）
- `AllocFrame` @va 0xa2e8b0 → 唯一引用 0x19a9128（raise 冷路径，紧邻 PushFrame 本体 0x19a90e4 之后）
- `AllocLocalloc` @va 0xa5f595 → 唯一引用 0x19bf308（AllocLocalloc 本体 0x19bf31c 之前）

### B. AllocStackSlot 本体（0x19a9078）
```
ldp w8,w10,[x0,#0xc]      ; w8 = stackTopIdx, w10 = localPoolBottomIdx
add w9,w8,w1 ; cmp w9,w10 ; b.le ok
... 不足则 bl 0x19ba90c 扩容，重读 [x0,#0xc]
ok: ldr x10,[x19] ; str w9,[x19,#0xc]   ; 写回 top += slotNum
    add x0,x10, w8, sxtw #3              ; 返回 base + oldTop*8  ⇒ **槽宽 8 字节**
```
与公开源码 `AllocStackSlot`（返回 data=stack+oldTop，stride=StackObject=8B）完全一致 ⇒ 第二证据。

### C. PushFrame 本体（0x19a90e4）
```
ldp w8,w9,[x0,#0x20]      ; w8 = frameTopIdx, w9 = frameCapacity
cmp w8,w9 ; 满则跳 0x19a9128 raise "AllocFrame"
ldr x9,[x19,#0x18] ; add w10,w8,#1 ; str w10,[x19,#0x20]
add x0, x9, x8, lsl #6    ; 返回 frames + idx*0x40 ⇒ **InterpFrame = 64 字节**
```

### D. EnterFrameFromInterpreter（0x19a9004）——三特征全中
```
mov x21,x0 ; mov x0,x1 ; mov x19,x2 ; mov x20,x1   ; (this, method, argBase)
bl 0x1abde50                 ; RuntimeInitClassCCtor 形态
ldr x8,[x20,#0x58]           ; imi = method->interpData            ← 特征①
ldr w9,[x8,#0x2c]            ; imi->maxStackSize
ldr w8,[x8,#0x20]            ; imi->argStackObjectSize
ldr w22,[x0,#0xc]            ; oldStackTop = state->stackTopIdx
sub w1,w9,w8                 ; maxStackSize − argStackObjectSize   ← 特征①（Native 版无此减法）
bl 0x19a9078                 ; AllocStackSlot                     ← 特征②
bl 0x19a90e4                 ; PushFrame                          ← 特征②
stp x20,x19,[x0]             ; frame->method = method, frame->argBase = argBase
str w22,[x0,#0x10]           ; frame->oldStackTop
stur q0,[x0,#0x18] ; stur q0,[x0,#0x28]   ; 3×nullptr 区
str w8,[x0,#0x38]            ; localPoolBottomIdx                 ← 特征③（9 字段一次写完）
ret                          ; 返回 newFrame
```

### E. 与 FromNative 的区分（0x19a913c）
```
ldr x8,[x22,#0x58] ; ldr w1,[x24,#0x2c]   ; 直接用 maxStackSize，**无减法**
bl 0x19a9078 → bl 0x19a90e4 → stp x22,x21,[x0]（argBase 处填 stackBasePtr）
```
⇒ 两函数结构同构、唯一差别就是「减不减 argStackObjectSize」，与公开源码两版实现一致。

## 与 1.8 记录的关系
- 1.8 的 0x19876a8（file 0x19836a8）+ 序言 0xF81D0FFE 在 2.2 已换位；2.2 对应实现改为 `str x30,[sp,#-0x30]!` + 上述差值形态。
- 1.8 记录的 `ldr x8,[x20,#0x58]`、`ldr w9,[x8,#0x2c]` 在 2.2 **依然成立**（0x19a9024/0x19a902c），此前「0 命中」是因为前置指令不同，不是字段变了。
