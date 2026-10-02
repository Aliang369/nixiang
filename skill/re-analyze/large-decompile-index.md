# 大体积反编译产物：先建索引，再按锚点切片读

> 路径：`re-analyze/large-decompile-index.md`
> 适用：单文件 >10MB 的反编译/导出产物 —— `dump.cs`、整包反编译的 `Assembly-CSharp.decompiled.cs`、
> `Il2CppDumper` 输出、Decompiler 全量导出等。
> 目标：把「一次 grep 全文件 + 反复读窗口」换成「一次性建索引 → 之后只按行号切片」，既省 context 又不再重复劳动。

## 为什么值得单独建流程

一次 grep 命中几十上百行、`read` 一次吞掉几千行，是这类文件最典型的 token 泄漏点：
真正的信息往往只有 3–5 行（一个类的声明、一个方法体的门控条件），其余全是同族成员的噪音。
**索引是廉价的（一次性），重复搜索是昂贵的（每次都要重新付）**。

## 标准流程

### 1. 先量体

```bash
wc -l big.decompiled.cs          # 行数
ls -l  big.decompiled.cs         # 字节数
shasum -a 256 big.decompiled.cs  # 记哈希：行号锚点必须钉在确定的版本上
```

### 2. 建类型索引（一次性，落盘到 `/tmp` 或 `09_test/{游戏}/`）

```bash
# 类型声明 + 行号（去掉 `{`/基类细节，只留「行号 全限定名」）
grep -nE '^\s*(public|internal|private|protected)?\s*(sealed |abstract |static |partial |unsafe )*(class|struct|enum|interface)\s' big.decompiled.cs \
  | sed -E 's/\{.*$//; s/:.*$//' > /tmp/big_types.txt
wc -l /tmp/big_types.txt
```

- 大类（>1000 行）先 `grep -c` 再 `grep -n`，别直接打印。
- **同名类会重复出现**（不同命名空间、嵌套类型）⇒ 索引用「行号 + 名称」双列，靠行号区分。
- 需要按命名空间分组时补一列 `awk`，不要重跑反编译。

### 3. 建「锚点表」并写进游戏文档（关键一步）

把这次确定要反复引用的位置，连同**文件哈希 + 总行数**一起落到 `10_docs/{游戏}/`：

```text
文件: /tmp/ssjd2/cs_src/Assembly-CSharp.decompiled.cs  (26,840,204 B, sha256 ab12…, 行数 N)
  976335  class BleachBattle.ActorDataControl
  977657  ActorDataControl.get_IsInvincible
 979884  ActorDataControl._finalAttrImpl
 980070  ActorDataControl.Hurt
 980429  ActorDataControl.ModifyHealth
 1023907 BleachBattle.SkillInstance.get_iSkillState
```

好处：后续所有步骤只做 `sed -n '976335,976420p'`，不再 grep；跨会话/跨压缩块也能直接续上。
**文件一旦重新生成，锚点表作废** —— 所以哈希要写在同一行。

### 4. 按锚点切片读

```bash
sed -n '976335,976420p' big.decompiled.cs      # 只读要用的那一段
```

- 单次窗口控制在 ±80 行；不够就第二刀，不要「一次读 3000 行保险」。
- 需要「这个类有哪些方法」时，用 `grep -n` 限定在类的行区间内：`sed -n 'A,Bp' big.cs | grep -n 'public'`。

### 5. 能用工具按类型取，就不要 grep 大文件

已有 DLL 时，单类型反编译是 O(该类型) 而不是 O(全包)：

```bash
~/.dotnet/tools/ilspycmd -l c          target.dll          # 类型清单（比 grep 大 .cs 更准）
~/.dotnet/tools/ilspycmd -t 'Ns.Type'  target.dll          # 只要这一个类型
```

同理，IL/字段/参数列表用自定义 dumper（本工作区：`04_scripts/*/ilpatch` 的 `--dump <类型> <方法|*>`）比在大 `.cs` 里搜更可靠。

### 6. 产物归档纪律

- 大反编译产物（>10MB）**不入库**，只在文档里登记「怎么再生成」（DLL 路径 + 命令），和 APK 底包一个口径。
- 中间索引文件（`*_types.txt`）放 `/tmp` 或 `09_test/{游戏}/`，可丢弃、可重生成。
- 索引/锚点表的**结论**（哪一层、哪个类型、哪个方法）写进游戏文档；**过程**不入库。

## 反面清单（本工作区已踩过）

- ✗ `cat` 大文件、或不带 `-m` 的 grep 打到几十上百行。
- ✗ 用 `read` 直接翻大文件找类定义（索引一次就能省掉后面所有翻页）。
- ✗ 反复重新反编译同一 DLL（产物落盘 + 哈希登记一次即可）。
- ✗ 把行号写在文档里却不记文件哈希（下次文件变了，锚点静默指错位置）。
- ✗ 在「按层定位」还没做完前就一头扎进大文件搜索（先走 [SKILL.md](SKILL.md) 的玩法层定位决策树）。

## 快速检查清单

```text
□ 这份产物多大？哈希和行数记了吗？
□ 类型索引建了吗（落盘）？还是每次都在原地 grep？
□ 要复用的位置进锚点表了吗（带哈希）？
□ 本次读的窗口是 ±80 行级别，还是「保险起见读一大段」？
□ 有 DLL 时是不是该用 ilspycmd/dumper 按类型取，而不是 grep .cs？
□ 大产物入库了吗（不该）？再生成命令写进文档了吗？
```
