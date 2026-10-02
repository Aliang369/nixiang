# libil2cpp.so ELF 段/节映射表（实测）

文件: `E:\nixiang\06_dumped\冒险与征途\apk_extract\lib\arm64-v8a\libil2cpp.so`
大小: 87,013,984 bytes (0x52FBA60)
格式: ELF64 little-endian / AArch64 / ET_DYN
`e_entry = 0x1D7408C`、`e_phoff = 0x40`、`e_phnum = 10`、`e_shoff = 0x52FB320`、`e_shnum = 29`、`e_shstrndx = 27`

---

## 1. 程序头（PT_LOAD）与 **delta 实测**

> **核心结论：本游戏 delta 不是常数，必须按段取。**
> `vaddr = file_offset + delta`，delta 由该地址落在哪个 PT_LOAD 决定。

| idx | type | flags | file_off | vaddr | filesz | memsz | **delta = vaddr-off** |
|----|----|----|----|----|----|----|----|
| 1 | LOAD | R-- | 0x0 | 0x0 | 0x1D7308C | 0x1D7308C | **0x0** |
| 2 | LOAD | R-X | 0x1D7308C | 0x1D7408C | 0x2F8B204 | 0x2F8B204 | **0x1000** |
| 3 | LOAD | RW- | 0x4CFE290 | 0x4D00290 | 0x347388 | 0x347388 | **0x2000** |
| 4 | LOAD | RW- | 0x5045618 | 0x5048618 | 0x2B5AB0 | 0x4E6528 | **0x3000** |

其余段：`PHDR`(0x40)、`DYNAMIC`(idx5, delta 0x2000)、`GNU_PROPERTY`(idx6)、`GNU_STACK`(idx7)、`GNU_RELRO`(idx8, 空)、`NOTE`(idx9)。

注意第 4 段 memsz(0x4E6528) > filesz(0x2B5AB0)，尾部 0x22D A78 为 `.bss` 零页。

---

## 2. 节头（全部 29 节）与 RV↔offset 换算

| idx | name | type | flags | addr(VA) | off | size | delta |
|----|----|----|----|----|----|----|----|
| 1 | .note.android.ident | NOTE | A | 0x270 | 0x270 | 0x98 | 0x0 |
| 2 | .note.gnu.build-id | NOTE | A | 0x308 | 0x308 | 0x18 | 0x0 |
| 3 | .dynsym | DYNSYM | A | 0x320 | 0x320 | 0xFC78 | 0x0 |
| 4 | .gnu.version | VERSYM | A | 0xFF98 | 0xFF98 | 0x150A | 0x0 |
| 5 | .gnu.version_r | VERNEED | A | 0x114A4 | 0x114A4 | 0x60 | 0x0 |
| 6 | .gnu.hash | GNU_HASH | A | 0x11508 | 0x11508 | 0x3EB4 | 0x0 |
| 7 | .hash | HASH | A | 0x153BC | 0x153BC | 0x5430 | 0x0 |
| 8 | .dynstr | STRTAB | A | 0x1A7EC | 0x1A7EC | 0x1B67B | 0x0 |
| 9 | .rela.dyn | RELA | A | 0x35E68 | 0x35E68 | 0xC9A398 | 0x0 |
| 10 | .rela.plt | RELA | AI | 0xCD0200 | 0xCD0200 | 0x3840 | 0x0 |
| 11 | **.rodata** | PROGBITS | A | **0xCD3A40** | **0xCD3A40** | 0x57ACEB | **0x0** |
| 12 | .gcc_except_table | PROGBITS | A | 0x124E72C | 0x124E72C | 0xB82A4 | 0x0 |
| 13 | .eh_frame_hdr | PROGBITS | A | 0x13069D0 | 0x13069D0 | 0x23F404 | 0x0 |
| 14 | .eh_frame | PROGBITS | A | 0x1545DD8 | 0x1545DD8 | 0x82D2B4 | 0x0 |
| 15 | **.text** | PROGBITS | AX | **0x1D7408C** | **0x1D7308C** | 0x55267C | **0x1000** |
| 16 | **il2cpp**（AOT 段） | PROGBITS | AX | **0x22C6708** | **0x22C5708** | 0x2A365E0 | **0x1000** |
| 17 | .plt | PROGBITS | AX | 0x4CFCCF0 | 0x4CFBCF0 | 0x25A0 | 0x1000 |
| 18 | .data.rel.ro | PROGBITS | WA | 0x4D00290 | 0x4CFE290 | 0x2DE9E8 | 0x2000 |
| 19 | .fini_array | FINI_ARRAY | WA | 0x4FDEC78 | 0x4FDCC78 | 0x10 | 0x2000 |
| 20 | .init_array | INIT_ARRAY | WA | 0x4FDEC88 | 0x4FDCC88 | 0xE8 | 0x2000 |
| 21 | .dynamic | DYNAMIC | WA | 0x4FDED70 | 0x4FDCD70 | 0x1D0 | 0x2000 |
| 22 | .got | PROGBITS | WA | 0x4FDEF40 | 0x4FDCF40 | 0x67400 | 0x2000 |
| 23 | .got.plt | PROGBITS | WA | 0x5046340 | 0x5044340 | 0x12D8 | 0x2000 |
| 24 | **.data** | PROGBITS | WA | **0x5048618** | **0x5045618** | 0x2B5AB0 | **0x3000** |
| 25 | .bss | NOBITS | WA | 0x52FE0D0 | 0x52FB0C8 | 0x230A70 | （NOBITS，无文件映射） |
| 26 | .comment | PROGBITS | MS | 0x0 | 0x52FB0C8 | 0x125 | 不映射 |
| 27 | .shstrtab | STRTAB | — | 0x0 | 0x52FB1ED | 0x11A | 不映射 |
| 28 | .gnu_debuglink | PROGBITS | — | 0x0 | 0x52FB308 | 0x18 | 不映射 |

### 换算公式（本文件唯一正确姿势）

| 目标段 | RVA ↔ file offset |
|----|----|
| 前 8 节 + `.rodata`/`.gcc_except_table`/`.eh_frame*`（**只读段**） | `file_off = RVA`（delta = 0） |
| `.text` / `il2cpp`(AOT) / `.plt` | `file_off = RVA - 0x1000`；`RVA = file_off + 0x1000` |
| `.data.rel.ro` / `.got*` / `.dynamic` / `.init_array` | `file_off = RVA - 0x2000` |
| `.data` | `file_off = RVA - 0x3000` |

> 反汇编器注意事项：`llvm-objdump` 用 **RVA（ELF vaddr）** 寻址，不要自己减 0x1000；
> 只有手工读文件时（mmap/struct）才需要按段减 delta。

---

## 3. `.text` + `il2cpp` AOT 段布局

| 区间 | RVA 起 | RVA 止 | 说明 |
|----|----|----|----|
| `.text` | 0x1D7408C（= `e_entry`） | 0x22C6707 | 主 C++ 代码（il2cpp runtime + HybridCLR 解释器） |
| `il2cpp` | 0x22C6708 | 0x4CFCCE7 | AOT 生成的 C# 方法体 |
| `.plt` | 0x4CFCCF0 | 0x4CFF28F | PLT |

实测：解释器相关代码全部落在 `.text` 的 0x207Dxxx–0x208Cxxx 区段。

---

## 4. 关键字符串精确地址（实测）

| 字符串 | 长度 | VA = file_off | 所在节 | 段 | delta | 是否唯一 |
|----|----|----|----|----|----|----|
| `AllocFrame` | 10 | **0x00D1CA18** | `.rodata` | PT_LOAD1 (R--) | 0x0 | **全文件唯一命中** |
| `AllocStackSlot` | 14 | **0x00CF2929** | `.rodata` | PT_LOAD1 (R--) | 0x0 | **全文件唯一命中** |

两者均为 NUL 结尾的独立 C 串，嵌在 `.rodata` 的大串池里（前后是别的串）：

- `AllocFrame` 邻接（±128B）：
  `...compare r4.` `not support array element type.` `Object.` `unused table type.` `<Module>.` `image can't be inited again.!.` **`AllocFrame`** `.bad delegate.u.` `JNI_OnLoad.` `Enum.` `System.IO.Compression.` ...
- `AllocStackSlot` 邻接（±128B）：
  `...Could not load type '.` `reloading placeholder assembly is not supported!.` `GetManaged2NativeMethodPointer.` ` sinature:%s not support..` **`AllocStackSlot`** `.StackTraceUtility.v.i.` `%s.` `ThreadStaticAttribute.` `Image::ReadTypeFromResolutionScope ...`

> 判读：这一片 `.rodata` 是 **il2cpp runtime + HybridCLR 源码里的常量串池**（混着 C++ 异常文本、C# 类型/方法名、HybridCLR 自己的 `MetadataModule.cpp`/`Image.cpp` 报错串）。
> `AllocFrame` / `AllocStackSlot` 正是 **HybridCLR 解释器 C++ 函数名被当作异常消息串**留在这里（见 `interp_entry_report.md` §3 的异常构造证据）。
>
> 已验证：**没有**任何 `.rela.dyn` 重定位、**没有** `.data.rel.ro` 里的裸指针指向这两个地址
> （`.rela.dyn` 共 550,609 条，addend 命中两者 = 0 条）。
> 即：它们只被**代码里的 ADRP+ADD**引用（共 2 处，见 `interp_entry_xrefs.txt`）。

---

## 5. 运行时地址换算

运行时基址（设备实测）：`base = 0x400023200000`

```
runtime_vaddr = base + RVA = 0x400023200000 + RVA
```

| 对象 | RVA | file_off | **运行时 vaddr** |
|----|----|----|----|
| HybridCLR 解释器入口 | **0x207F798** | 0x207E798 | **0x40002527F798** |
| AllocFrame 入口 | 0x207DDAC | 0x207CDAC | 0x40002527DDAC |
| AllocStackSlot 入口 | 0x207DD40 | 0x207CD40 | 0x40002527DD40 |
| 帧初始化 A（调 AllocStackSlot+AllocFrame） | 0x207DCCC | 0x207CCCC | 0x40002527DCCC |
| 帧初始化 B（同上，另一重载） | 0x207DE04 | 0x207CE04 | 0x40002527DE04 |
| 串 `AllocFrame` | 0xD1CA18 | 0xD1CA18 | 0x4000232D1CA18 |
| 串 `AllocStackSlot` | 0xCF2929 | 0xCF2929 | 0x4000232CF2929 |

---

## 6. 导出符号（`.dynsym`，共 2,388 条，无 `.symtab`）

- `il2cpp_*` 全套 API 存在：`il2cpp_method_get_name`、`il2cpp_method_get_class`、`il2cpp_class_get_name`、`il2cpp_alloc`、`il2cpp_class_from_name` 等（→ 壳日志里「自解析导出表 + `捕获IL2CPP函数：%s`」的来源）。
- **`.dynsym` 里没有任何 `hybridclr` / `InterpFrame` / `Interpreter` / `Execute` 相关导出**（只有 libc++ 的 `_ZNSt6__ndk1...` 与 `il2cpp_*`）。
  ⇒ 解释器入口**只能靠特征定位**，无法靠导出名定位 —— 与壳日志「定位方式=特征自动定位」一致。

---

## 7. 复现命令

```powershell
# 段/节表（本文件由它生成）
C:\Users\Administrator\AppData\Local\Programs\Python\Python311\python.exe .\_elf_sections.py
# 字符串定位
C:\Users\Administrator\AppData\Local\Programs\Python\Python311\python.exe .\_find_strings.py
# 重定位/段性质核查
C:\Users\Administrator\AppData\Local\Programs\Python\Python311\python.exe .\_region_char.py
```
