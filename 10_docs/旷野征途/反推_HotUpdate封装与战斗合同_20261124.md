# 反推：HotUpdate.dll 封装解密与战斗/经济 IL 合同（3.2，2026-11-24）

> 子代理：hotupdate-crack　｜　目标：从 AOT C# 的 IL/反汇编反推 3.2 新的 v2 热更包解密链路并取回密钥，再核对 2.4 的六项合同。
> 结论先行：**密钥已拿到，HotUpdate.dll 已解出并验证为合法 PE**；六项合同中 1 项变化（ApplyHp）、5 项保持。

## 0. 输入指纹与复现

| 输入 | 大小 | sha256 |
|---|---|---|
| `13_scratch/kyzt32_base/lib/arm64-v8a/libil2cpp.so` | 66,268,112 | `b249a13111e87c82ca595d374a10da5c4c972e3dd94bac2fd0540f07b65e332b` |
| `13_scratch/kyzt32_base/assets/bin/Data/Managed/Metadata/global-metadata.dat`（v31） | 11,782,304 | `9a106895fdf32adb7a77528962adc3576c8124898fe829d3a4d0f31da8428ddd` |
| `06_dumped/旷野征途/3.2/out/dump.cs` | 25,786,132 | `3bdecf5d…600f` |
| `13_scratch/kyzt32_native/hotupdate/HotUpdate.dll`（封装字节） | 3,583,905 | `728f1fc340d3b7b8977818d48b5f44d9157b32437ea42c680f107b70ab19cb29` |

**产物**：`13_scratch/kyzt32_native/hotupdate/HotUpdate.decrypted.dll`　3,583,488 B　sha256 `812e6563c61c19e4dac4a667a5e7f6c399ad981813b4ea83f4221ca4a7d551b0`

复现（全部离线，只读 .so/.dat/封装字节）：
```bash
# 1) 取密钥候选（按 metadata 字符串字面量表，不用正则扫 blob）
python 13_scratch/kyzt32_native/hd_brute_lit.py
# 2) 解包
python 13_scratch/kyzt32_native/hd_decrypt.py
# 3) 合同核对
python 13_scratch/kyzt32_native/hd_verify2.py
```

> **RVA→文件偏移不是一律 −0x4000**（本 .so 实测 4 个 LOAD 段）：
> `off=0x0 va=0x0 delta=0x0 (R)` / `off=0x1793714 va=0x1797714 delta=0x4000 (RX)` / `off=0x3a8d220 va=0x3a95220 delta=0x8000 (RW)` / `off=0x3cf9e28 va=0x3d05e28 delta=0xc000 (RW, BSS 更大)`。
> 代码段用 −0x4000 正确；**数据段（元数据槽、静态字段）用错 delta 会读到别的字节**。

## 1. 包格式（AOT 侧 `ET.HotUpdateCodePackage`，TypeDefIndex 10299）

```
dump.cs L672279  Namespace: ET
public static class HotUpdateCodePackage
  const int   FormatVersion   = 2
  const int   IvLength        = 16
  const int   SignatureLength = 384
  const int   HeaderLength    = 17          // = 1 + 16
  static readonly byte[] EncryptionKey;     // 静态字段 off 0x0
  const string PublicModulus = "vwZcqjBYeIt4…"   // RSA-3072, base64
  const string PublicExponent = "AQAB"

  RVA 0x2BA74F4  static long GetVerifiedAndroidVersionCode()
  RVA 0x2BA7A64  static byte[] ReadCodeAsset(byte[] package, string assetName, long versionCode)
  RVA 0x2BA7FA8  static Assembly LoadAssembly(Dictionary<string,byte[]> files, long versionCode)
  RVA 0x2BA84EC  static byte[] ComputeSignatureHash(byte[] package, int signedLength, string assetName, long versionCode)
  RVA 0x2BA88C8  static bool   VerifyReleaseSignature(byte[] hash, byte[] signature)
  RVA 0x2BA8BD0  static void   .cctor()
```

字节布局（由 `ReadCodeAsset` 的边界检查反推，实测吻合）：

```
 0       1B   FormatVersion = 0x02                pkg[0x20+0] == 2 否则抛异常
 1      16B   IV                                    (AES-CBC 初向量)
17       N*16 密文   N = (len - 17 - 384)/16          实测 3,583,504 B = 16×223969
17+N*16 384B  RSA 签名                              pkg[signedLength .. +384]
```

长度校验（`0x2ba7adc..0x2ba7af0`）：
```
0x2ba7adc  ldr   w24,[x20,#0x18]      ; w24 = pkg.Length
0x2ba7ae0  sub   w22,w24,#0x191       ; w22 = len-401 (= 密文长)
0x2ba7ae4  cmp   w22,#0x10            ; b.lt  失败  -> 至少 16B
0x2ba7aec  and   w8,w22,#0xf          ; cbnz  失败  -> 密文必须 16 对齐
0x2ba7af4  ldrb  w8,[x20,#0x20]       ; 数组元素0
0x2ba7af8  cmp   w8,#2                ; b.ne  失败  -> FormatVersion==2
```

## 2. 解密链路（RVA + 反汇编切片）

### 2.1 `ReadCodeAsset`（0x2BA7A64）

```
0x2ba7b38  bl    #0x2df554c            ; Buffer.BlockCopy(pkg, signedLength, sig384, 0, 384)
0x2ba7b64  bl    #0x2ba84ec            ; ComputeSignatureHash(pkg, signedLength, assetName, versionCode)
0x2ba7b6c  bl    #0x2ba88c8            ; VerifyReleaseSignature(hash, sig)   ↓失败即抛
0x2ba7b90  bl    #0x2c2cbac            ; Aes.Create()      (x19 = aes 实例)
0x2ba7bac  ldr   x1,[x8]               ; x8 = class->static_fields -> EncryptionKey (byte[])
0x2ba7bcc  blr   x10                   ; aes.Key = EncryptionKey        (vtable+0x208)
0x2ba7bd0  bl    #0x1b11278            ; new byte[16]
0x2ba7be0  mov   w1,#1                 ; srcOffset = 1
0x2ba7be4  mov   w4,#0x10              ; count = 16
0x2ba7bf8  bl    #0x2df554c            ; Buffer.BlockCopy(pkg, 1, iv, 0, 16)   <== IV = pkg[1:17]
0x2ba7c0c  blr   x9                    ; aes.IV = iv                    (vtable+0x1e8)
0x2ba7c20  blr   x9                    ; aes.CreateDecryptor()          (vtable+0x2b8)
0x2ba7c84  ldp   x8,x4,[x0]
0x2ba7c88  mov   w2,#0x11              ; inputOffset = 17
0x2ba7c94  mov   w3,w22                ; inputCount  = len-401
0x2ba7c98  blr   x8                    ; ICryptoTransform.TransformFinalBlock(pkg,17,len-401)
```

要点：**一次 TransformFinalBlock 解完整段**，Aes 未改 Mode/Padding ⇒ AES-CBC + PKCS7；明文 = 单个 asset 内容（不是容器）。

### 2.2 `.cctor`（0x2BA8BD0）——密钥来源

```
0x2ba8bd8  adrp x20,#0x3f40000 ; ldrb w8,[x20,#0xd23]   ; 静态初始化 flag
0x2ba8bf4  bl   #0x1b11190      ; 初始化元数据槽 [0x3ca1478]
0x2ba8c00  bl   #0x1b11190      ; 初始化元数据槽 [0x3cadb20]  (HotUpdateCodePackage class*)
0x2ba8c0c  bl   #0x1b11190      ; 初始化元数据槽 [0x3caf560]  (字符串字面量)
0x2ba8c48  mov  x1,xzr
0x2ba8c40  bl   #0x2d9779c      ; System.Convert.FromBase64String(string)   <== 唯一的密钥派生
0x2ba8c4c  ldr  x8,[x8,#0xb8]   ; static_fields
0x2ba8c50  str  x0,[x8]         ; EncryptionKey = FromBase64String(字面量)
```

- 0x2D9779C 在 dump.cs 中定位为 `System.Convert.FromBase64String(string s)`（同块内 0x2D96FEC `ConvertToBase64Array`）。
- 因此 `EncryptionKey` = **某个 C# 字符串字面量的 base64 解码**，长度取 16/24/32 三者之一才可能 `set_Key` 成功。
- 没有任何设备指纹 / PlayerPrefs / URL / 服务器下发参与：整段 .cctor 只有一次 FromBase64String。

### 2.3 定位那条字面量（关键，也是前序 0 命中的原因）

字符串字面量表在 global-metadata.dat 头部：`stringLiteralOffset=256, size=128408` ⇒ **16,051 条** `{int32 length, int32 dataIndex}`，字符在 `stringLiteralDataOffset=128664`。

- 前序用「正则扫 blob 取 base64 串」为什么 0 命中：字面量在 blob 里**首尾相连无分隔**，贪婪正则会把相邻字面量一起吞掉，得到的串长度非 4 的倍数、解码成错误字节。
- 正确做法：按 (offset,length) 表逐条取。918 条候选中（base64→16/24/32 或 raw 16/24/32）只有 2 条通过 PKCS7 校验，其中 1 条直接给出 `MZ`：

| 字面量序号 | 字面量 | 解码 key |
|---|---|---|
| **6155** | `MUTuHPa4moJngx5gmKtcxz+6hqvmmKoSt4qpjwsDAL8=` | `3144ee1cf6b89a8267831e6098ab5cc73fba86abe698aa12b78aa98f0b0300bf`（AES-256, 32B） |

**判定：密钥来源 = 静态字符串字面量（AES-256）**，与设备/服务器无关，可静态解包；服务器下发假设不成立。

## 3. 解包结果指纹

| 项 | 值 |
|---|---|
| 密文 | 3,583,504 B，IV `4313c04f37593d8efcb32fd2711c7f61` |
| 明文 | 3,583,488 B（PKCS7 填充 16，尾块全 `0x10`） |
| sha256 | `812e6563c61c19e4dac4a667a5e7f6c399ad981813b4ea83f4221ca4a7d551b0` |
| PE | MZ / PE32（machine 0x14c）/ 3 节 |
| Assembly | `HotUpdate` v0.0.0.0，Module `HotUpdate.dll`，MVID `17d9582b-9634-4fdc-a86b-b948bcda2cad` |
| 输出 | `13_scratch/kyzt32_native/hotupdate/HotUpdate.decrypted.dll` |

## 4. 六项合同逐条核对（3.2 vs 2.4 前序记录）

### ① `Battle.BattleRoleActor.ApplyHp` —— **变化**

| 项 | 2.4 | 3.2 实测 |
|---|---|---|
| argc | 4 | **5**（`void(BattleRoleActor atker, int atkSeatNum, BattleDamageResult dmgInfo, Action<BattleRoleActor> OnDie)`） |
| body | 1265 B | **1594 B**（fat，RVA 0xFF20C） |
| IL_0173 get_isGod | ✔ | ✘：该偏移现在是 `ldc.i4.1`；**get_isGod 移到 IL_021C** |
| IL_0187 ret | ✔ | ✘：IL_0187 现在是 `beq.s`；近处 ret 在 IL_0205 / IL_0230 |
| IL_0197 add→IL_01A0 set_curHp | ✔ | ✘：首次 `set_curHp` 在 **IL_0249 / IL_0263** |

结论：2.4 的 ApplyHp patch 模板对 3.2 **不可用**（偏移全错 + 参数多了 `OnDie` 回调）。若阶段3 要改 ApplyHp，需按新签名重定位。

交叉验证（ilspycmd 独立还原，非本仓 dnfile 一家之言）：
```csharp
private void ApplyHp(BattleRoleActor atker, int atkSeatNum, BattleDamageResult dmgInfo, Action<BattleRoleActor> OnDie)
```

### ② `MemoryValueEncoding.RotateKey` —— **一致**

两重载均 `static`，参数 1：

```
Int32  rva 0x3DEEC  body 26B
  ldarg.0; ldc.i4.s 13; shl; xor; ldc.i4.s 17; shr.un; xor; ldc.i4.5; shl; xor; ret
  ⇒ x ^= x<<13; x ^= x>>>17; x ^= x<<5

Int64  rva 0x3DF07  body 37B
  ldarg.0; ldc.i4.s 13; shl; xor; ldc.i4.7; shr.un; xor; ldc.i4.s 17; shl; xor; ret
  ⇒ x ^= x<<13; x ^= x>>>7; x ^= x<<17
```

（`op61=xor / op62=shl / op64=shr.un`；常量 13/17/5 与 13/7/17 与前序记录逐字一致。）

交叉验证（ilspycmd `-t MemoryValueEncoding`）：
```csharp
int num2 = num ^ (num << 13); int num3 = num2 ^ (num2 >>> 17); return num3 ^ (num3 << 5);
long num2 = num ^ (num << 13); long num3 = num2 ^ (num2 >>> 7); return num3 ^ (num3 << 17);
```

附带发现：`MemoryValueEncoding..cctor`（rva 0x3DF78）用 `Guid.NewGuid().GetHashCode()` 播种静态字段 `sequence`，`NextKey()` 用 `Interlocked.Add(seq, 0x9e3779b9)` + 混合。**每次进程启动的密钥流不同**，静态预测 encodedPoints 数值不可行；但 RotateKey 本身对给定 key 确定。

### ③ `BattleRoleActor` 字段名 —— **一致（3 个为自动属性 backing field）**

| 2.4 名 | 3.2 实测 |
|---|---|
| `_curHp` | ✔ 直接字段 |
| `_baseAttributes` | ✔ 存在，但真名 `<_baseAttributes>k__BackingField` |
| `_battleAttributes` | ✔ 存在，真名 `<_battleAttributes>k__BackingField` |
| `hpCount` | ✔ 真名 `<hpCount>k__BackingField`（访问器 `get_hpCount/set_hpCount` 在） |

字段总数 464（3.2 大量新增职业/宠物状态字段）。按名反射要写 backing field 名或走属性访问器。

### ④ `BattleRoleMgr` / 多人运行时 —— **一致**

- `BattleRoleMgr.Instance` ✔ 但 `Instance` 是 **public static 字段**（`get_Instance` 不存在，别按属性取）。
- `<player>k__BackingField` ✔（`get_player/set_player` 在，rva 0x2E30B / 0x2E313）
- `BattleRoleActor.get_LocalSeatNum` ✔ rva 0x101EE3
- `PartyBattleRuntime.get_LocalPlayer` ✔ rva 0x280E7（body 26B）

### ⑤ 经济三项 —— **一致，且有新增面**

| 2.4 依赖 | 3.2 实测 |
|---|---|
| `AccountInfo.encodedPoints` / `pointsKey` | ✔ 字段均在 |
| `ItemSaveData.encodedCount` / `countKey` / `baseItemid` | ✔ 字段均在 |
| `PlayerInfo.ChangeGold(Int64,bool)` | ✔ `bool ChangeGold(i8, bool)` rva 0x535F8 body 120B；`CanChangeGold(i8)` 也在 |
| 新增 | `AccountInfo.encodedGold` / `goldKey` / `get_sharedGold`（金币与点数分离加密）；`ItemManager.ChangeGold(i8)` |

`AccountInfo` 本身没有 `ChangeGold`（2.4 记录里可能是把 PlayerInfo 的写在一起），它是 109 个字段的存档根，金币走 `sharedGold` 属性。

### ⑥ 汇总表

| # | 合同 | 结论 |
|---|---|---|
| 1 | ApplyHp argc/body/IL_0173/0187/0197 | **变化**：5 / 1594B / 021C / 0205-0230 / 0249 |
| 2 | RotateKey Int32(13,>>>17,<<5) / Int64(13,>>>7,<<17) | 一致 |
| 3 | `_curHp`/`_baseAttributes`/`_battleAttributes`/`hpCount` | 一致（后三者是 k__BackingField） |
| 4 | `Instance`/`<player>k__BackingField`/`get_LocalSeatNum`/`get_LocalPlayer` | 一致（Instance 是字段） |
| 5 | `ChangeGold(i8,bool)`/`encodedPoints`/`pointsKey` | 一致（+encodedGold/goldKey） |
| 6 | `encodedCount`/`countKey`/`baseItemid` | 一致 |

## 5. 未证项

1. `ComputeSignatureHash` 的具体构造（是否 SHA-384、是否拼 assetName/versionCode）**未反推**；本轮用 PKCS7 完整性 + PE 头 + 元数据自洽代替 RSA 验签。若需要「内容与官方发布完全等价」的强证据，还应补 RSA-3072 PKCS1 验签。
2. `assetName` 实参（谁调用 `ReadCodeAsset`、传入什么名字）未追到；不影响解密（解密不依赖 assetName）。
3. `ApplyHp` 第 4 参已定位为 `OnDie` 回调（ilspy 还原），但其在新流程中的调用时机与是否可空未细究。

## 6. 给阶段3 的结论

- **HotUpdate.dll 已可静态解出**（密钥 `3144ee1c…0300bf`，AES-256-CBC，IV=pkg[1:17]，PKCS7）。产出 `HotUpdate.decrypted.dll` 可直接用 dnfile/ilspy 分析或 IL patch。
- **只有 ApplyHp 的挂点失效**：所有针对 ApplyHp 的 2.4 patch 偏移（IL_0173/0187/0197/01A0）在 3.2 全错位，且签名多了 `Action<BattleRoleActor>` 参数。阶段3 若做伤害/血量 hook，必须按新签名 + 新偏移（isGod@IL_021C、set_curHp@IL_0249）重定位，或改走属性访问器 `get_curHp/set_curHp`（rva 0x102442/0x10244A）这种稳定锚点。
- **经济与多人合同全部可用**：`MemoryValueEncoding.RotateKey` 常量未变、`encodedPoints/pointsKey/encodedCount/countKey/baseItemid/ChangeGold` 全部在位。
- 解包流程可复现且完全离线：`hd_brute_lit.py` → `hd_decrypt.py` → `hd_verify2.py`（脚本在 `13_scratch/kyzt32_native/`）。

## 7. 本轮改动文件

- 新增 `10_docs/旷野征途/反推_HotUpdate封装与战斗合同_20261124.md`（本文件）
- 新增产物 `13_scratch/kyzt32_native/hotupdate/HotUpdate.decrypted.dll`
- 新增脚本（scratch，一次性复现用）：`13_scratch/kyzt32_native/{hd_cfg.json,hd_key_brute.py,hd_key_brute2.py,hd_key_brute3.py,hd_brute_lit.py,hd_decrypt.py,hd_elf.py,hd_reloc.py,hd_dis2.py,hd_dis3.py,hd_verify.py,hd_verify2.py,hd_names.py,hd_sig2.py,hd_applyhp.py,hd_meta_hdr.py}`
- 反汇编切片缓存：`13_scratch/kyzt32_native/{dis_readcodeasset.txt,dis_ApplyHp.txt,verify_contracts.txt,verify_names.txt}`
