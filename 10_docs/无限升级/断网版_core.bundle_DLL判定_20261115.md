# 断网版 `core.bundle` 热更 DLL 判定（2026-11-15）

> 范围：`02_apks/无限升级_12.10701_断网版.apk!assets/offline/core.bundle` 的单节点载荷。
> 方法：自实现标准 LZ4 block 解码（未用仓库 `unityfs_bundle.py`，未用 lz4 库），逐块校验尺寸。
> 产物目录：`06_dumped/无限升级_断网版/`

## 0. 结论速览

| 问题 | 结论 | 置信度 |
|---|---|---|
| 能否完整解出 1,998,680 B？ | **不能**。block00 压缩流非法，前 131,072 B 永久缺失；其余 1,867,608 B 完整可解 | 高（穷举 51,675 个起点全部失败） |
| 解出的是什么？ | **.NET 托管的 `targetfuntional.dll` 图像，但 PE 头 + 前 128 KB 已丢失**（不是 HybridCLR 裸 IL 流，不是加密） | 高（BSJB/元数据表/导入表/版本资源俱在） |
| 是不是官方 12.10701/vc1644 那一份？ | **不是**。MVID、表行数、类型集合均不同，且内含断网破解注入类 | 高 |
| 是不是 20261110 那份？ | **也不是**。是介于两者之间的第三个构建 | 高 |
| 已拍好的 2,023,424 B 补丁 DLL 能否直接塞回 | **不能**（见 §5，4 条独立理由） | 高 |

## 1. 解码过程与逐块校验

头解析（与 lead 的实测一致，已复核）：

```
UnityFS / version=8 / unity=5.x.x / rev=2022.3.62f2c1
size=1,077,185  flags=0x242 (LZ4 + blocksInfoCombined + blocksInfo 16 对齐)
cbis=151  ubis=241  blocksInfo@0x40   解出 241 B（sha256 6cf4210aa4babf28…）
uncompressedDataHash = 0000…0000（全零，说明打包端未写该校验）
nodes = 1: CAB-cb132cbbb0bfe5d34dd5fa57a781dd1a  size=1,998,680  flags=0x4(SerializedFile)
数据区：raw 0xd7 → 16 对齐 0xe0，其后剩 1,076,961 B = Σcomp，精确到 EOF（无尾部冗余）
```

16 个 block 的解码结果：

| # | unc | comp | 结果 |
|---|---|---|---|
| 00 | 131,072 | 51,675 | **失败**：流偏移 63 处 token=0x55 要求 offset=57,593，而当时只产出 81 B |
| 01–15 | 各 131,072（末块 32,600） | 见清单 | 全部 OK，每块产出长度与声明 unc **完全相等** |

对 block00 做了穷举：把 51,675 B 的每一个起点（0…51,674）都当作流头重试，**没有任何一个**能得到完整 131,072 且恰好耗尽输入的合法流。→ 该 block 的压缩字节在包内就已损坏/被改写，不是解码器的问题。

**上一轮 recon 的两个文件为什么不可信**：
- `core_bundle_serialized_1998680.bin` 的前 131,072 B 全 0x00：是**填充占位**，不是解压结果。block00 的前 6 个 token 合法，解出 76 B 且形态是"稀疏零 + 零星字节"（`…16 00…09 f1…1e 7f 58…0a 30…`），**本身就不是全零**。
- `core_bundle_payload_from_0x20000.bin`（1,867,608 B）= 直接丢弃 block00。尺寸对，但当时没有校验块边界，也没发现 block00 是坏的。

## 2. 节点解剖：它到底是什么

可读区段（1,867,608 B，对应节点偏移 131,072…1,998,679）解剖：

1. **0x0 起就是合法 CIL 字节码**（`00 00 0a 2a 4a 02 7b 21 0e 00 04 6f 31 00 00 0a …` = nop nop stloc.0 ret / ldind.i8 ldarg.0 ldfld call …），说明头部被切掉的位置正好落在一个方法体边界附近。
2. **BSJB 位于段内 0x0DEEB8（= 913,080；对应全节点 0xFEEB8 = 1,044,152）**：`BSJB 01 00 01 00 … v4.0.30319` —— 标准 ECMA-335 元数据根，5 个流：

   ```
   #~        size 553,076     #Strings 172,104
   #US       size 138,372     #GUID 16        #Blob 88,860
   ```

3. **元数据表可完整解析**：Module 行 name = `targetfuntional.dll`；TypeDef 1422、MethodDef 10,099、AssemblyRef 45、CustomAttribute 7,229、MemberRef 4,521。
4. **元数据结束于段内 0x1C7790（= 1,865,616；全节点 1,996,688）**，其后 1,992 B 是 PE 尾结构，含：
   - .NET 导入描述符 + `_CorDllMain` + `mscoree.dll` + IAT 桩 `ff 25 00 20 00 10`（ImageBase 0x10000000 / IAT RVA 0x2000，标准托管 DLL）；
   - `.rsrc` 资源目录 + `VS_VERSION_INFO`（`InternalName`/`OriginalFilename` = targetfuntional.dll，FileVersion/Assembly Version 0.0.0.0）。

**判据结论**：
- ❌ 不是标准 .NET PE 文件：可读 1.87 MB 内 `MZ\x90\x00`、`PE\0\0`、`.text/.rsrc/.reloc` 节名**均为 0 次命中**；而 `assets/offline/mscorlib.dll` 等明文 DLL 都是正常 MZ+PE。
- ❌ 不是 HybridCLR 的"IL+metadata 裸流"（这种封装本身不存在；HybridCLR 加载的就是标准 PE）。
- ❌ 不是加密：IL 可读、元数据表可解析、字符串堆可读。
- ✅ 是 **一个真实托管程序集图像，PE 头 + 前 131,072 B 被剥离（或被置零）后剩下的主体**；尾部 PE 结构完整保留。与 lead 的"第三方 loader 运行时补头"假设方向一致，但**包内并没有可用的零块**，block00 是损坏数据而非全零。

## 3. 与三份基线的比对

基线（注意 lead 给的尺寸有一处记错）：`targetfuntional_OFFICIAL_12.10701.dll` 实为 **1,960,960 B**（2,028,544 B 是 `newdump_20261110/targetfuntional_NEW_20261110.dll`）；已拍补丁 `targetfuntional_hotfix3_patched_20261110.dll` = 2,023,424 B。

| 指标 | 本包 node | OFFICIAL 12.10701 | NEW 20261110 | PATCH(hotfix3) |
|---|---|---|---|---|
| 文件/节点大小 | 1,998,680（缺头 131,072） | 1,960,960 | 2,028,544 | 2,023,424 |
| Module 名 | targetfuntional.dll | 同 | 同 | 同 |
| **MVID** | **f84e1b18-953f-964e-8f60-c22e4694a565** | 960af7ce-f3e3-0a40-87bb-964d9c67c91a | 2e2dcb96-972d-cb43-ba48-330373ac4d21 | 同 NEW |
| TypeDef | 1,422 | 1,378 | 1,426 | 1,427 |
| MethodDef | 10,099 | 9,795 | 10,087 | 10,093 |
| AssemblyRef | 45 | 46 | 48 | 48 |
| `#Strings` 条数 | 11,640 | 11,360 | 11,705 | — |

相似度（同一 codebase 的对照标定非常关键）：

| 比对 | 4 KB 块哈希交集（任意对齐） | 4 KB 块对齐网格 | 256 B 窗口"任意位置命中" | `#Strings` Jaccard |
|---|---|---|---|---|
| node ↔ OFFICIAL 12.10701 | 0 / 456 | 0.0% | **12.2%** | 0.963 |
| node ↔ NEW 20261110 | 0 / 456 | 0.0% | **13.2%** | 0.978 |
| node ↔ PATCH hotfix3 | 0 / 456 | 0.0% | 13.4% | — |
| *标定：OFFICIAL ↔ NEW* | *0 / 456* | *0.0%* | ***17.2%*** | — |
| *标定：NEW ↔ PATCH* | *0 / 456* | *0.0%* | *18.6%* | — |

读法：4 KB 对齐块交集对本类对象**没有分辨力**（任意一份真实不同构建都是 0，因为元数据表插入会让其后全部错位）；抗错位指标的读法是——本包与两份基线的重合度（12–13%）**低于**两份已知不同构建之间的重合度（15–19%），说明差异不小于"换了一个版本"的量级。

本包独有、两份基线都没有的东西：

- **断网破解注入类**：`CodexOfflineBridge`、`CodexOfflineRunner`（字节级 grep：node=有，OFFICIAL/NEW/PATCH 全部=无）；
- 新玩法/新流程符号：`MissionTreeStageTable`、`MissionTreeTaskTable`、`DeserializeMissionTreeData`、`InitialReward`、`LocalShopReady`、`LoadSupplementalMetadata`、`CreatePackage`、`InitializationOperation`。

## 4. 判定：是不是官方 12.10701/vc1644 同一份？

**不是。** 四条独立判据同时成立：

1. **MVID 不同**（f84e1b18… vs 960af7ce…）——MVID 是编译产物指纹，不同即不是同一个二进制（哪怕只改一行代码也会变）。
2. **元数据表行数不同**（TypeDef +44 / MethodDef +304 / AssemblyRef −1）。
3. **内容重合度低于"两个已知不同构建"之间的水平**（12.2% vs 标定 17.2%）。
4. **内含官方基线没有的注入类**（`CodexOfflineBridge`/`CodexOfflineRunner`）——即使原来是官方 DLL，也已经是被断网改造重编译过的版本。

同时**也不是 20261110 那份**（MVID 不同、TypeDef 1422 vs 1426）。按表行数与字符串集丰富度排序（1,378 → 1,422 → 1,426），本包是**介于 vc1644 基线与 20261110 构建之间的第三个构建**，再叠加断网注入。

> 补充：`assets/offline/` 下另有 16 个明文 DLL（mscorlib / System / YooAsset / DOTween / ACTk / Obfuz / UniTask …），全部是标准 MZ+PE；只有 `core.bundle` 里的主程序集是"缺头"形态。这与"断网改造只重打了主程序集"的判断一致。

## 5. 已拍好的 2,023,424 B 补丁 DLL 能不能直接塞回去？

**不能直接塞。** 四条理由，任一条都足以否决：

1. **构建不同**：补丁基于 20261110 构建（MVID 2e2dcb96…、TypeDef 1427），本包是 1422 的另一个构建。字节级替换 = 换了一个程序集，不是打补丁。
2. **会丢断网桥**：`CodexOfflineBridge`/`CodexOfflineRunner` 只存在于本包这份程序集里，我们的 DLL 里没有（已字节级验证）。换进去等于删掉离线桥，断网启动链直接断。
3. **头部缺失，无法做等价替换与校验**：本包节点前 131,072 B 不可恢复，既拿不到原始头部来做"同尺寸替换"，也无法验证替换后的字节布局与 loader 预期一致。
4. **RVA 级 hook 偏移不通用**：已拍补丁的改写位置是按 20261110 构建的 RVA 定位的，在本构建里不成立（方法体整体位移，且 MethodDef 数差 12）。

**要在这份 bundle 上做功能，正确路线**：先拿到本构建的**带 PE 头的原样 DLL**（最直接：找非断网版 12.10701 原包的同名 bundle，或找该构建的 hot-update 原始 DLL），在它上面重做补丁；而不是复用 2,023,424 那份。

## 6. 产物与哈希

目录 `06_dumped/无限升级_断网版/`：

| 文件 | 大小 | sha256 |
|---|---|---|
| `core_bundle_node_verified_blocks01-15.bin` | 1,867,608 | `74e956cc575eff55fa18fb9f3537545e57df0f63702e3026cf40dd801dd1ac8e` |
| `core_bundle_node_FULL.bin` | 1,998,680 | `91800e268a3a3035b6710e9b686f06fb42f9d865a20e44fd3e21b413e0bc6044` |
| `core_bundle_block00_corrupt_51675.bin` | 51,675 | `8c26bf3aeeb54bc406a442396965a5af5a13e993d251ef32f4752692a66c3b0f` |
| `core_bundle_analysis_20261115.json` | 5,270 | `02ab0c7b132114f3ad94be5f70e2f30e29ed47b658d10c7884df8d9330fc9e60` |

- `core_bundle` 本体：1,077,185 B，sha256 `b4857494aeb16d937c1332a73d20217734c9157fe03c21f784aeb25b70a1c62d`（`07_patched/无限升级_断网版/…_去卡密.apk` 内同 hash，两份 APK 无差异，故**无法从别处取回 block00**）。
- ⚠️ `core_bundle_node_FULL.bin` 的 **0…131,071 是 0x00 占位**（因为 block00 不可解），131,072…1,998,679 是实测内容。任何"整文件哈希/整文件当 DLL 用"的用法都必须改用 `…verified_blocks01-15.bin`。

## 7. 未完成 / 下一步（需 lead 决策）

1. 想拿到**可用**的该构建 DLL，只有补回 block00 一条路：需要非断网版同名 bundle 或该构建原始 hot-update DLL。仓库内目前没有（已搜 `core.bundle`、全部 `*.apk`、`assets/offline`）。
2. 若目标是功能补丁：建议改为"以本包 1,867,608 B 有效区为基线重做补丁"，但前提仍是解决头部（loader 侧头部由谁生成、能否旁路）。
3. 本次未做：反编译 IL、定位具体功能挂点、判定 `CodexOfflineBridge` 的加载时序（需 lead 明确"开始做功能"后再启动）。
