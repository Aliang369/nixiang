# 旷野征途 3.2 — C# 侧（AOT）反篡改反推

> 结论先行：3.2 启动时有且只有一个 C# 反篡改入口 `ET.AndroidPackageIntegrity.VerifyInstalledPackageAsync`，
> 它做**两件事**：① Java 签名比对（SHA-256(DER) 的 base64 == 官方证书指纹）；② APK 内 3 个核心 so 的 SHA-256 白名单比对（含 RSA 签名保护）。
> **它不枚举 `lib/` 目录**，所以「多一个 libstarry_magic.so」本身不失败；真正会失败的是「改了 libil2cpp.so / libunity.so / libmain.so 的字节」。
>
> 状态：纯静态反推，未上机、未改任何代码。文中每条结论标 `[实测]`（本机反汇编/哈希可复核）、`[推断]`（有证据链推理）、`[待验证]`（缺一条证据，已注明缺什么）。

---

## 0. 输入指纹与复现命令（AGENTS.md §5 要求）

| 输入 | 值 |
|---|---|
| `libil2cpp.so` | 66,268,112 B / sha256 `b249a13111e87c82ca595d374a10da5c4c972e3dd94bac2fd0540f07b65e332b` |
| `global-metadata.dat` | 11,782,304 B / sha256 `9a106895fdf32adb7a77528962adc3576c8124898fe829d3a4d0f31da8428ddd`（v31） |
| `dump.cs` | 25,786,132 B / sha256 `3bdecf5da5a3c213dc6ea6a23dfd167b8a9087e346b679981a2e9ed336c8000f` |
| 官方 APK | `02_apks/旷野征途/旷野征途_3.2_taptap_official.apk` / sha256 `6318a972a599cfa664c1db867b960fc1a5f9b878d9bad3c15b003d28da17df1d` |
| va − file | `0x4000`（`.text` 与 `il2cpp` 两段实测一致） |
| 分段 | `.text` va 0x1797714 / off 0x1793714 / 0x492A40；`il2cpp` va 0x1C2A154 / off 0x1C26154 / 0x1E64B44；`.data.rel.ro` 0x3A95220；`.data` 0x3D05E28；`.bss` 0x3F3E250 |

本轮自建脚本（全部落在 `13_scratch/kyzt32_antitamper/`，一次性分析脚手架，不是工具）：

```bash
# 1) RVA -> 方法名索引（流式扫 dump.cs，不整读）
python 13_scratch/kyzt32_antitamper/rvaidx.py            # 产出 rva_index.tsv，90,037 条（与 dump 统计一致）

# 2) 带注解反汇编：capstone(arm64) + RVA 索引反查 BL 目标 + RELA 相对重定位
python 13_scratch/kyzt32_antitamper/ana.py 0x2B6CA4C:1055          # VerifyInstalledPackageAsync
python 13_scratch/kyzt32_antitamper/ana.py 0x2B6DAD0:476           # VerifyCurrentSigner
python 13_scratch/kyzt32_antitamper/ana.py 0x2B6E290:933           # VerifyNativeLibraries（到下一个方法 0x2B6F124 为止）
python 13_scratch/kyzt32_antitamper/ana.py 0x2B6F124:198           # GetUniqueEntry
python 13_scratch/kyzt32_antitamper/ana.py 0x2B6F43C:0x50          # .cctor

# 3) 调用点扫描（扫两个可执行段的 BL/B 立即数）
python 13_scratch/kyzt32_antitamper/xrefs.py 0x2B6CA4C 0x2BA6AE8 0x2B6DAD0 0x2B6E290

# 4) 白名单密码学取证（官方 APK 内 480B blob 对 28 个 so 实算 SHA-256 比对）
python 13_scratch/kyzt32_antitamper/blob_probe.py
```

工具链：既有 `01_tools/il2cppdumper`（未改动 config，只出 dump.cs）+ 系统 python3.11 + capstone 5.0.7 + pyelftools。**未自造反汇编器**，脚本只是 capstone 的取值/注解壳。

---

## 1. 日志证据链（真机 96f0d883，已确认）

```
11-24 16:35:43.435 W Unity: [Startup] 0.20s: 检查安装包完整性
11-24 16:35:43.467 E Unity: InvalidDataException: 安装包签名校验失败。
  at ET.AndroidPackageIntegrity.VerifyCurrentSigner (UnityEngine.AndroidJavaObject info, System.Boolean modern)
  at ET.AndroidPackageIntegrity.VerifyInstalledPackageAsync ()
```

堆栈与反汇编**完全对得上**，并且给出了两条硬信息 `[实测+推断]`：

1. 栈里**没有** `<>c__DisplayClass5_0`/`Task.Run`/ThreadPool 帧 ⇒ 异常是在**调用线程同步抛出**的。
   反汇编证实：`VerifyCurrentSigner` 的调用点 0x2B6CF20 早于 `Task.Run` 的 0x2B6D0E0 ⇒ `VerifyCurrentSigner` **不在** Task 里跑，所以它抛的异常**不经过 Task**。
2. 所以它走的是 `GameManagerMono.<Start>d__41.MoveNext` 的 `catch (Exception)` 分支，不是 `Task.IsFaulted` 分支（见 §2.1）。

---

## 2. 五个方法反推

### 2.1 `VerifyInstalledPackageAsync`（RVA 0x2B6CA4C / off 0x2B68A4C / 长 0x107C）

**签名与返回**：`internal static Task`，**不是 async**（没有 state machine，dump 里没有 `<VerifyInstalledPackageAsync>d__N`）⇒ 它就 `return Task.Run(lambda)`。
**没有 try/catch**：全函数只有 `throw`（0x1B11430 = NRE / 0x1B112F8 = throw），没有任何 handler 表入口。

反推出的等价 C#：

```csharp
internal static Task VerifyInstalledPackageAsync()
{
    var jcActivityThread = new AndroidJavaClass("<lit#0>");               // 0x2B6CC60
    var ctx   = jcActivityThread.GetStatic<AndroidJavaObject>("<lit#1>"); // 0x2B6CC88
    var jcPM  = new AndroidJavaClass("<lit#2>");                          // 0x2B6CCAC
    if (ctx.Call<string>("<lit#3>") != PackageName)                       // 0x2B6CD28 → op_Inequality 0x2B6CD3C
        throw new InvalidDataException(...);                              // 0x2B6D41C
    int sdkInt = jcBuildVersion.GetStatic<int>("SDK_INT");                // 0x2B6CD64（w25）
    bool modern = sdkInt > 27;                                            // 0x2B6CE70 cmp w25,#0x1b / 0x2B6CF18 cset gt
    int flag = jcPM.GetStatic<int>(modern ? "<GET_SIGNING_CERTIFICATES>"  // 字面量槽 0x3CADA58
                                         : "<GET_SIGNATURES>");           // 字面量槽 0x3CADA28
    var args = new object[2]; args[0] = <包名>; args[1] = (object)flag;    // 0x2B6CE0C array_new(string,2) + 0x2B6CEA4 装箱
    var info = pm.Call<AndroidJavaObject>("getPackageInfo", args);        // 0x2B6CEF4 (0x1D80200)
    VerifyCurrentSigner(info, modern);                                    // 0x2B6CF20  ← 异常从这里同步逃逸
    long vc = modern ? info.Call<long>("getLongVersionCode")              // 0x2B6CFA8 (0x1D801B0, sxtw)
                     : info.Call<int>("getVersionCode");                  // 0x2B6CFDC (0x1D81F18)
    var appInfo = info.Get<AndroidJavaObject>("applicationInfo");         // 0x2B6D008
    string apkPath = appInfo.Get<string>("sourceDir");                    // 0x2B6D030
    string abi = IntPtr.Size == 8 ? "arm64-v8a" : "armeabi-v7a";          // 0x2B6D04C IntPtr.get_Size + csel
    var dc = new <>c__DisplayClass5_0 { apkPath=…, versionCode=vc, abi=… }; // 0x2B6D0A0/.0x10/.0x18/.0x20
    if (vc < 1 || string.IsNullOrEmpty(apkPath)) throw new InvalidDataException(...); // 0x2B6D07C / 0x2B6D08C
    return Task.Run(new Action(dc.<VerifyInstalledPackageAsync>b__0));    // 0x2B6D0E0
}
```

关键切片（实测，`ana.py` 输出）：

```
0x02B6CE70  cmp   w25, #0x1b                       ; sdkInt vs 27
0x02B6CE7C  csel  x8, x9, x10, gt                  ; gt → 取 GET_SIGNING_CERTIFICATES 槽，否则 GET_SIGNATURES 槽
0x02B6CEA4  bl    #0x1b11310                       ; 装箱 flag(int → object)
0x02B6CEF4  bl    #0x1d80200                       ; pm.Call<AndroidJavaObject>("getPackageInfo", object[])
0x02B6CF18  cset  w1, gt                           ; modern 参数
0x02B6CF20  bl    #0x2b6dad0                       ; ET.AndroidPackageIntegrity::VerifyCurrentSigner(info, modern)
0x02B6D04C  bl    #0x2e01514                       ; System.IntPtr::get_Size()  → abi 由进程位宽硬编码
0x02B6D0E0  bl    #0x2bf3cfc                       ; System.Threading.Tasks.Task::Run(Action)
```

**abi 不是从 APK 读的**，是 `IntPtr.Size==8 ? "arm64-v8a" : "armeabi-v7a"`（0x2B6D050–0x2B6D070 两个字面量槽 0x3CADA30/0x3CADA38 二选一）`[实测]`。
⇒ 32 位进程会去找 `assets/NativeIntegrity/armeabi-v7a.bin`，而官方包里**只有** arm64-v8a 那份（见 §3），所以官方包在 32 位下走 VerifyNativeLibraries 必挂 —— 反过来说明 96f0d883 是 64 位进程。

**谁接住**（`[实测]`，xrefs 扫描）：
- `VerifyInstalledPackageAsync` 只有 **1 个调用点**：`GameManagerMono.<Start>d__41.MoveNext+0x5C4`（0x2BA66F4）。
- `ShowPackageIntegrityError` 有 **2 个调用点**，都在同一个 MoveNext 里：`+0x648`（0x2BA6778，Task.IsFaulted 分支）与 `+0x90C`（0x2BA6A3C，`catch (Exception)` 分支）。

```
0x02BA66C0  bl  #0x2ba5f58   ; UIStartupController::SetStage("检查安装包完整性")  ← 日志那行
0x02BA66F4  bl  #0x2b6ca4c   ; VerifyInstalledPackageAsync()
0x02BA6728  bl  #0x2be9c20   ; Task::get_IsCompleted()
0x02BA673C  bl  #0x2bede14   ; Task::get_IsFaulted()
0x02BA6758  bl  #0x2beddd8   ; Task::get_Exception()  → AggregateException
0x02BA6764  ldp x9,x1,[x8,#0x1a8] ; 虚调用 AggregateException.get_InnerException
0x02BA6778  bl  #0x2ba6ae8   ; UIStartupController::ShowPackageIntegrityError(inner)
0x02BA69E0  bl  #0x1b11438 / 0x1b11454 / 0x1b112f8   ; catch 块：取托管异常
0x02BA6A3C  bl  #0x2ba6ae8   ; 同一个 ShowPackageIntegrityError
```

**弹窗终点**（`[实测]`）：`ShowPackageIntegrityError` 是个 **1 条指令的尾调用**：
```
0x02BA6B74  b  #0x2bae284   ; UIStartupController::ShowBlockingError(stage, error, title, message)
                            ; 三个字面量槽：0x3CAF458 / 0x3CAF478 / 0x3CAF480
```
三个字面量 = 「检查安装包完整性」/ 标题 / 「请从官方渠道获取完整包后重试」`[推断：三槽全是字面量字符串，文案由用户在真机截图确认]`。

**失败后不会自己恢复**：`+0x648` 分支处理完直接 `b 0x2BA69C4` = `mov w0,wzr; ret` ⇒ MoveNext 返回 false ⇒ 协程结束 ⇒ `GameManagerMono.Init()` 永不执行。这就是"弹窗后卡死"。`[实测]`

### 2.2 `VerifyCurrentSigner`（RVA 0x2B6DAD0 / off 0x2B69AD0）

**失败方式是 `throw`，不是 `Log`**（全函数 4 处 `new InvalidDataException(msg)` + `throw`，见下表）。

```csharp
private static void VerifyCurrentSigner(AndroidJavaObject info, bool modern)
{
    AndroidJavaObject sig;
    if (modern) {                                              // 0x2B6DBD0 tbz w20,#0 → 非 modern 分支
        var signingInfo = info.Call<AndroidJavaObject>("<lit 0x3CADAD0>");       // 0x2B6DBF4 → 判空 throw @0x2B6DF5C
        var signerArr   = signingInfo.Call<AndroidJavaObject[]>("<lit 0x3CADAF0>"); // 0x2B6DC6C → 判空 throw
        if (signerArr.Length != 1) throw new InvalidDataException(...);           // 0x2B6DCAC cmp w8,#1 → 0x2B6DFAC
        sig = signerArr[0];
    } else {
        var arr = info.Call<AndroidJavaObject[]>("<lit 0x3CADAD8>");              // 0x2B6DC98 → 判空 throw @0x2B6E058
        if (arr.Length != 1) throw new InvalidDataException(...);
        sig = arr[0];
    }
    byte[] der = sig.Call<byte[]>("<lit 0x3CADAE0>");           // 0x2B6DD28 / 0x2B6DD28 汇合点
    byte[] h   = SHA256.Create().ComputeHash(der);              // 0x2B6DDD8 / 0x2B6DDF0
    if (Convert.ToBase64String(h) != SignerSha256)              // 0x2B6DE18 / 0x2B6DE2C op_Inequality
        throw new InvalidDataException("安装包签名校验失败。");  // 0x2B6E030 ← 日志里那句
}
```

逐条回答提问：

| 问 | 答 | 证据 |
|---|---|---|
| 调哪个 Java API | SDK>27 走 `getSigningInfo` → `getApkContentsSigners` → `[0].toByteArray()`；否则 `getSignatures` → `[0].toByteArray()` | 两条路径各用不同字面量槽，最后汇合到同一槽 0x3CADAE0；`[推断]` 方法名文本未解（见 §7） |
| getPackageInfo 的 flag | 由 SDK 分叉：**`modern = sdkInt > 27`** 时取 `GET_SIGNING_CERTIFICATES`，否则 `GET_SIGNATURES`；flag 值不是硬编码常量，而是 `PackageManager.GetStatic<int>(字段名)` | 0x2B6CE70 `cmp w25,#0x1b` + 0x2B6CE7C `csel` + 0x2B6CEA4 装箱 `[实测]；`[推断]` 是"取静态字段"而不是"硬编码 0x40/0x8000000"` |
| 比对 SHA-256 还是 DER | **SHA-256(DER) 再 base64**，与 `SignerSha256` 常量做字符串比较（不是直接比 DER，也不是比 hex） | `SHA256::Create` 0x2C3B8FC + `ComputeHash(byte[])` 0x2C2B1C8 + `Convert::ToBase64String` 0x2D96A14 + `String::op_Inequality` 0x2CC3C74 `[实测]` |
| modern 分叉在哪 | 0x2B6DBD0 `tbz w20,#0, 0x2B6DC78`；w20 来自 0x2B6DAEC `mov w20,w1`（第 2 参数） | `[实测]` |
| 失败是 throw 还是 Log | **throw**（4 个 InvalidDataException 构造点：0x2B6DF88 / 0x2B6DFD8 / 0x2B6E030 + 三处 `throw` 0x1B112F8） | `[实测]` |
| 只允许 1 个签名者 | 是，`Length != 1` 直接 throw | 0x2B6DCAC / 0x2B6E0A8 `cmp w8,#1` `[实测]` |

### 2.3 `SignerSha256` 常量 ⇔ 官方证书指纹（自己算的）

```
base64("Cjt4JZHHiVQ3RypHaKR1TmSH/SfL4Yh2l+vDjeQaBI8=")  解码 32 B
  = 0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f
official_apk.json:signer_sha256
  = 0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f
  → EQUAL   [实测]
```
⇒ 校验的就是**官方证书的 SHA-256**，不是 DER，也不是 DN。改包后签名不同 → 必挂。

### 2.4 `.cctor`（RVA 0x2B6F43C / off 0x2B6B43C / 长 0x118）

`Libraries` 的长度是 **3**（不是 16、不是 28）：

```
0x02B6F458..0x02B6F490  5 次 il2cpp_codegen_initialize_method  （5 个字面量/类型槽）
0x02B6F49C  ldr  x0,[x20]         ; x20 = *(0x3CA0C08) = String 类的 TypeInfo 槽
0x02B6F4A0  mov  w1, #3           ; ← new string[3]
0x02B6F4A4  bl   #0x1b11278       ; il2cpp_array_new(String[], 3)
0x02B6F4DC  cmp  w8, #1  / b.ls   ; 写 [1] 前边界检查
0x02B6F4F8  (str + stelemref)     ; [0] = 字面量槽 0x3CADBC0
0x02B6F504  cmp  w8, #2  / b.ls   ; [1] = 字面量槽 0x3CADBC8
0x02B6F51C  ...                   ; [2] = 字面量槽 0x3CADBB8
0x02B6F534  str  x19,[x8]         ; AndroidPackageIntegrity.static_fields[0] = Libraries
```

**数组顺序 = {槽0x3CADBC0, 槽0x3CADBC8, 槽0x3CADBB8}**，与 §3 的 blob 哈希顺序一一对应。

### 2.5 `GetUniqueEntry`（RVA 0x2B6F124 / off 0x2B6B124）

```csharp
private static ZipArchiveEntry GetUniqueEntry(ZipArchive apk, string name)
{
    ZipArchiveEntry found = null;
    foreach (var e in apk.Entries)                 // 0x31104DC ZipArchive::get_Entries()
        if (e.FullName != name) continue;          // e+0x98 = FullName；0x2CC3C74 op_Inequality
        else if (found != null)                    // 0x2B6F29C cbz x20
            throw new InvalidDataException("..." + name);  // 0x33B0AF0 InvalidDataException(string)
        else found = e;
    return found;                                   // 可能返回 null（调用方自己判）
}
```

要点：**精确 `FullName` 全等**匹配（不是前缀/不是目录扫描）；同名条目出现 2 次 → 直接抛 `InvalidDataException`。`[实测]`

### 2.6 `VerifyNativeLibraries`（RVA 0x2B6E290 / off 0x2B6A290 / 长 0xE94）——本次最关键

```csharp
private static void VerifyNativeLibraries(string apkPath, long versionCode, string abi)
{
    using var fs  = File.OpenRead(apkPath);                        // 0x2D088CC
    using var apk = new ZipArchive(fs, ZipArchiveMode.Read);       // 0x310FC28 (mode=0)

    // ① 取「清单 blob」：名字 = "…" + abi + "…"，长度必须恰好 384 + 32*Libraries.Length（=480）
    var blobEntry = GetUniqueEntry(apk, string.Concat(<lit>, abi, <lit>));   // 0x2CC4450 Concat(str,str,str)
    if (blobEntry == null) throw new InvalidDataException(...);              // 0x2B6EC04
    if (blobEntry.Length != 0x180 + 32 * Libraries.Length) throw ...;        // 0x2B6E444/0x2B6E458  (0x180=384)

    using var br = new BinaryReader(blobEntry.Open());
    byte[] hashes = br.ReadBytes(32 * Libraries.Length);   // 0x2B6E4A8  (w1 = 32*3)
    byte[] sig    = br.ReadBytes(384);                     // 0x2B6E4C4  (w1 = 0x180)

    // ② 校验 blob 自身的 RSA 签名：SHA256( payload_utf8 || hashes ) == Verify(sig)
    string payload = string.Concat(new string[]{
        <lit 0x3CADB58>,                                   // 0x2B6E59C
        versionCode.ToString(CultureInfo.InvariantCulture),// 0x2DBDE04
        <lit 0x3CA18C0>,                                   // 0x2B6E608
        abi,                                               // 0x2B6E62C
        <lit 0x3CA18C8> });                                // 0x2B6E648
    byte[] pb = Encoding.UTF8.GetBytes(payload);            // 0x2B6E678 虚调用 0x2e8
    byte[] cat = new byte[pb.Length + hashes.Length];       // 0x2B6E6A0
    Buffer.BlockCopy(pb,0,cat,0,pb.Length);                 // 0x2DF554C
    Buffer.BlockCopy(hashes,0,cat,pb.Length,hashes.Length); // 0x2DF554C
    if (!HotUpdateCodePackage.VerifyReleaseSignature(SHA256.Create().ComputeHash(cat), sig))
        throw new InvalidDataException(...);                // 0x2B6E728 / 0x2B6E72C → 0x2B6ECEC

    // ③ 逐个比对白名单里的 so（← 只遍历 Libraries，不遍历 APK）
    for (int i = 0; i < Libraries.Length; i++) {
        var e = GetUniqueEntry(apk, string.Concat(<lit>, abi, <lit>, Libraries[i]));  // 0x2CC4C24 Concat(4)
        if (e == null) throw new InvalidDataException(...);                            // 0x2B6EB58
        byte[] actual = SHA256.Create().ComputeHash(e.Open());                         // 0x2C2B5E8 ComputeHash(Stream)
        if (!BytesEqual(actual, hashes, i*32, 32)) throw new InvalidDataException(...); // 0x2B6E8B0 → 0x2B6E93C
    }
}
```

模块边界（`[实测]`）：循环条件 0x2B6E774 `ldrsw x9,[x8,#0x18]; cmp x29,x9; b.ge 0x2B6E9D8`，x8 = `Libraries`；下标 → 名字 0x2B6E7BC `ldr x25,[x8, x29, lsl #3, #0x20]`。</br>
⇒ **判据是"白名单每个成员都必须在包里且字节一致"，不是"包里的 lib/ 目录必须恰好等于白名单"。**

---

## 3. 白名单完整内容（密码学取证，不靠猜字面量）

推导链：`VerifyNativeLibraries` 要求 blob 长度 = `384 + 32*3 = 480` ⇒ 官方 APK 里找 `file_size == 480` 的条目 ⇒ 唯一命中 `assets/NativeIntegrity/arm64-v8a.bin` ⇒ 前 96 B 按 32 B 切成 3 段，与 28 个 so 的实算 SHA-256 对撞：

```
assets/NativeIntegrity/arm64-v8a.bin   480 B  = 32*3 + 384
  hash[0] 98660436f78418667bf865646428b61666d10cfc72642d42ea11454ffddeae76
          → lib/arm64-v8a/libmain.so
  hash[1] 25c511a3f769b875de870a6baaf789fa894f90079fc48446e0bfc3ce488c6e8a
          → lib/arm64-v8a/libunity.so
  hash[2] b249a13111e87c82ca595d374a10da5c4c972e3dd94bac2fd0540f07b65e332b
          → lib/arm64-v8a/libil2cpp.so        （= 我们手上这份 so 的指纹，闭环）
  sig(384) 起始 213a7b6a5b7546967499314197664299…
```

⇒ **`static readonly string[] Libraries = { "libmain.so", "libunity.so", "libil2cpp.so" }`**（顺序如上）`[实测，等价于密码学证明]`。

补充实测：
- 官方包 `lib/` 下 28 个 so，**全在 arm64-v8a**，没有 `armeabi-v7a` 目录 ⇒ `GetStatic("primaryCpuAbi")` 类路径不存在，abi 完全靠 `IntPtr.Size` 硬编码。
- 生成 `blob` 的路径字面量 = 3 段 Concat，实测包内条目名就是 `assets/NativeIntegrity/` + `arm64-v8a` + `.bin`。
- 循环里的 4 段 Concat = `lib/` + abi + `/` + Libraries[i]（与 28 个条目名对得上）。

**白名单只有 3 个，且不含任何 SDK 的 so** —— 插件类 so（libtapsdkcore/libthemis/libpanglearmor… 共 25 个）**完全不受保护**。

---

## 4. 全 dump 同类校验扫描（启动路径内/外）

扫描方式：`rva_index.tsv` 按 `Integrity|AntiCheat|Tamper|llegality|NativeLib|PackageInfo|Signer|Crack` 过一遍 + 过滤掉 BCL/Mono 命名空间，再用 xrefs 判断是否在启动路径；另从 `GameManagerMono.<Start>d__41.MoveNext` 里把全部调用目标倒出来做交叉验证。

| 类 | 方法 | RVA | 在启动路径？ |
|---|---|---|---|
| **ET.AndroidPackageIntegrity** | VerifyInstalledPackageAsync | 0x2B6CA4C | **是**（MoveNext+0x5C4，唯一入口） |
| | VerifyCurrentSigner | 0x2B6DAD0 | 是（唯一调用者：上面这个） |
| | VerifyNativeLibraries | 0x2B6E290 | 是（经 b__0 / Task.Run） |
| | GetUniqueEntry / .cctor | 0x2B6F124 / 0x2B6F43C | 是 |
| ET.HotUpdateCodePackage | VerifyReleaseSignature(byte[],byte[]) | 0x2BA88C8 | **否**（被上面调用；另 1 个调用者 `ReadCodeAsset+0x108` 走热更） |
| ET.UIStartupController | ShowPackageIntegrityError | 0x2BA6AE8 | 是（失败弹窗） |
| | ShowBlockingError(4 参) | 0x2BAE284 | 是（弹窗落点） |
| AstarPath | VerifyIntegrity | 0x1C2EB00 | **否**（A* 插件内部刷新图引用，与篡改无关，可排除） |
| Mono.Security.Cryptography.PKCS1 / RSAManaged / DSAManaged / System.Security.Cryptography.* | Verify_v15 / VerifySignature / VerifyHash / VerifyData | 0x2BCC27C 起 | 否（BCL，被上述代码间接用） |
| PKCS7.SignedData / SignerInfo | VerifySignature | 0x2B815xx 区 | 否（Mono BCL） |
| System.IO.FileAttributes | IntegrityStream | 常量 | 否 |

**结论**：3.2 启动路径上参与反篡改的类**只有 `ET.AndroidPackageIntegrity` 一个**；`HotUpdateCodePackage.VerifyReleaseSignature` 只是被复用为 RSA 验签工具，热更路径另外用它（属于阶段3 热更功能范围，不阻塞启动）。`[实测]`

---

## 5. 绕过方案对比

前提约束（三条，缺一即挂）：
1. `assets/NativeIntegrity/arm64-v8a.bin` 必须**原样保留**（480 B 一字不改，且**不能重复**，重复触发 GetUniqueEntry 的 InvalidDataException）；
2. `lib/arm64-v8a/{libmain,libunity,libil2cpp}.so` 三个必须**字节不变**（签名密钥拿不到，blob 无法重签）；
3. `versionCode` 必须仍是 **32**（它被写进被签名 payload）。

| 方案 | 做法 | 成本 | 可行性 | 风险 |
|---|---|---|---|---|
| **A** | inline hook `VerifyInstalledPackageAsync` → 直接 `return Task.CompletedTask` | 1 个 hook | **高**。一处封死全部 3 关；不碰 Java、不碰 APK 内容 | 返回值必须是真托管 `Task*`：用 `System.Threading.Tasks.Task::get_CompletedTask`（RVA **0x2BEE36C** / off 0x2BEA36C）取其单例，不能返回 null（0x2BA670C `cbnz` 会走 NRE 抛 0x2BA6A44）。函数非 async、无 EH 表 ⇒ 直接覆盖入口即可 |
| **B** | inline hook `VerifyCurrentSigner` + `VerifyNativeLibraries`，两个都直接 `ret`（`private static void`，无需返回值） | 2 个 hook | **高**，且比 A 更"最小伤"：保留 PackageInfo 采集，只砍比对 | 仍要保证 hook 覆盖到 b__0 那条**线程池线程**上的调用；`VerifyNativeLibraries` 若不 hook，会 SHA-256 66 MB 的 libil2cpp（耗时+耗电，且必然失败） |
| **C** | Java 侧伪装签名（改 `ActivityThread.mPackages` → `LoadedApk.mSignatures` / v28+ `mSigningDetails`） | 高 | **单靠 C 在"只加 so"前提下能过**（依据见下），但条件苛刻 | ① API 28+ 走 `getSigningInfo()`，要改的是 `PackageInfo.signingInfo`/`SigningDetails`，隐藏 API 需 HiddenApiBypass；② `VerifyNativeLibraries` 仍会跑，只要三个核心 so 没被改就能过，**但一旦阶段3 采用"直接改 libil2cpp.so"的静态补丁，C 必挂且不可救**（签名私钥不在手上）；③ 与 AGENTS.md「SignSpoof 系列禁止复活」冲突，需用户当轮明确授权 |
| **D（推荐备选）** | 直接改判定字节：`Task.get_IsFaulted` 结果或 MoveNext 里 0x2BA6740 的 `tbz` 分支 | 低 | **不足**：只改 IsFaulted 分支挡不住**同步抛**的那条 catch 路径（0x2BA69E0），且该分支处理完仍 `b 0x2BA69C4` → MoveNext 返回 false → 协程结束。要改成"错误吞掉并继续 Init()"必须同时改两处，反而比 A 更脆 |
| **E（更优点）** | **A + 一条防御**：A 里返回 CompletedTask，同时在 ContentProvider 阶段记录一次性日志 | 1 hook + 1 行日志 | **最优** | 唯一风险：hook 时机。见 §6 |

### 「只做 C（签名伪装）能不能过？」——明确回答

**能过，但仅限"只加 so、不动三个核心 so"这一前提；且 C 不是充分的通用方案。**

依据（`VerifyNativeLibraries` 判据，全部 `[实测]`）：
1. 循环上界是 `Libraries.Length == 3`（0x2B6E774 `cmp x29,x9; b.ge`），**遍历白名单，不遍历 `apk.Entries`**；
2. 每次查找用 `GetUniqueEntry(apk, "lib/"+abi+"/"+Libraries[i])`，**精确名全等**，不做通配、不做计数；
3. 全函数**没有任何**对 `lib/` 目录列表、条目总数、目录 entry 的检查（`get_Entries()` 只在 GetUniqueEntry 里用于精确匹配）；
4. ⇒ 多出 `lib/arm64-v8a/libstarry_magic.so` **不会**被看到，不会失败。

反过来，只要满足 §5 的三条前提，C 就能同时过 ①签名（Java 返回官方 DER）②原生库（3 个哈希天然对上）。**所以"多一个 so"不是 C 的风险点，"改核心 so"才是。**

---

## 6. 要 hook 的函数清单 + 时机

### 6.1 清单（A 方案 1 条；B 方案 2 条）

| # | 目标 | RVA（va） | **file offset** | 序言 4 条指令（小端字） | hook 后动作 |
|---|---|---|---|---|---|
| 1 | `ET.AndroidPackageIntegrity::VerifyInstalledPackageAsync` | 0x2B6CA4C | **0x2B68A4C** | `0xD101C3FF` `0xA9017BFD` `0xA9026FFC` `0xA90367FA`<br>=`ff c3 01 d1 / fd 7b 01 a9 / fc 6f 02 a9 / fa 67 03 a9`<br>（`sub sp,sp,#0x70` `stp x29,x30,[sp,#0x10]!` `stp x28,x27,[sp,#0x20]!` `stp x26,x25,[sp,#0x30]!`） | 返回 `Task::get_CompletedTask()`（RVA 0x2BEE36C） |
| 2 | `VerifyCurrentSigner(AndroidJavaObject,bool)` | 0x2B6DAD0 | **0x2B69AD0** | `0xF81B0FFE` `0xA90167FA` `0xA9025FF8` `0xA90357F6`<br>=`fe 0f 1b f8 / fa 67 01 a9 / f8 5f 02 a9 / f6 57 03 a9`<br>（`str x30,[sp,#-0x50]!` `stp x26,x25,[sp,#0x10]` …） | 直接 `ret`（void） |
| 3 | `VerifyNativeLibraries(string,long,string)` | 0x2B6E290 | **0x2B6A290** | `0xD10203FF` `0xA9027BFD` `0xA9036FFC` `0xA90467FA`<br>=`ff 03 02 d1 / fd 7b 02 a9 / fc 6f 03 a9 / fa 67 04 a9`<br>（`sub sp,sp,#0x80` …） | 直接 `ret`（void） |

（备用参考：`GetUniqueEntry` off 0x2B6B124 序言 `fe 67 bc a9 / f8 5f 01 a9 / f6 57 02 a9 / f4 4f 03 a9`；一般不需要 hook 它。）

**取址方式建议**：优先走 il2cpp API 而不是硬编码 RVA —— `il2cpp_domain_get` → `il2cpp_domain_assembly_open`/`il2cpp_class_from_name(image,"ET","AndroidPackageIntegrity")` → `il2cpp_class_get_method_from_name(klass,"VerifyInstalledPackageAsync",0)` → `method->methodPointer`。本版本关键导出实测：`il2cpp_class_from_name` 0x19F3FB4、`il2cpp_class_get_method_from_name` 0x19F3FDC、`il2cpp_domain_get` 0x19F44AC、`il2cpp_domain_get_assemblies` 0x19F44B8、`il2cpp_assembly_get_image` 0x19F3F7C、`il2cpp_thread_attach` 0x19F4908。

### 6.2 时机

- 校验发生在 `GameManagerMono.Start()` 协程的**第一帧**（`Time.realtimeSinceStartup` 0.20 s 打点，见 MoveNext 里 `Time::get_realtimeSinceStartup` 0x2BA64E0/0x2BA6524/0x2BA65EC）。`[实测+真机日志]`
- 星光侧 so 由 `StarryInitProvider`（ContentProvider）在 `ActivityThread.handleBindApplication` 内、**Application.onCreate 之前**加载 ⇒ **早于** Unity 引擎初始化 ⇒ 天然早于 GameManagerMono.Start 至少 0.1–0.2 s。
- 硬性要求：**hook 必须在 `Application.onCreate` 返回前装完**（保守写法：在 `StarryInitProvider.onCreate` 里、`dlopen` 成功后的第一时间装）。理由：Unity 的 `Start` 可能在任何一帧被调到，一旦赶不上就是既成事实的弹窗。
- 额外约束：**不能只在主线程装 hook 后就不管** —— B 方案的 `VerifyNativeLibraries` 跑在 `Task.Run` 的线程池线程上；inline hook 是进程级改写代码段，与线程无关，装一次就够，无需按线程重装。`[推断]`

---

## 7. 未证项（缺什么、怎么补）

| 项 | 状态 | 缺的证据 / 补法 |
|---|---|---|
| 各 Java 方法名字面量（`getPackageInfo`/`getSignatures`/`getSigningInfo`/`getApkContentsSigners`/`toByteArray`/`sourceDir`/`applicationInfo`/`getLongVersionCode`） | `[推断]`：由 API 形状 + SDK 分叉 + `Length==1` 断言推出，文本未读 | 字面量存在 `global-metadata.dat` 的 stringLiteral 表，需把 usage 槽（如 0x3CADAD0/0x3CADAD8/0x3CADAE0/0x3CADAF0）解到 stringLiteralIndex。本轮试过 RELA 重定位 + MetadataRegistration 两条路，未打通（GOT 槽指向 0x3D9E710 类 BSS 缓存槽，属运行时填充）。替代法：真机上 hook `il2cpp_codegen_initialize_method`/`MetadataCache::GetStringLiteral` 打印，或用支持 stringliteral 导出的 Il2CppDumper 版本 |
| `Libraries` 三个元素"文本" | `[实测·等价]`：由 480B blob 的三个哈希与 28 个 so 对撞得出，不是读出来的字符串 | 若要字面证据，同上 |
| blob 名字的两段字面量、签名 payload 的 3 段字面量 | `[推断]`：名字由 3 段 Concat + 包内条目名 `assets/NativeIntegrity/arm64-v8a.bin` 反证；payload 文本未知 | 同上（payload 文本对绕过无意义：签名私钥拿不到，只能跳过） |
| `BytesEqual` 的具体实现（0x2B82904 无符号） | `[推断]`：返回 bool、比较两段 span/数组 | 该地址不在 dump.cs 符号表内（泛型实例/共享代码），反汇编该处可确认 |
| 96f0d883 是 64 位进程 | `[推断]`：官方包只有 `assets/NativeIntegrity/arm64-v8a.bin`，32 位会去找 `armeabi-v7a.bin` 而必挂 | 真机 `getprop ro.product.cpu.abilist` / 进程位宽（本轮禁上机，未取） |

---

## 8. 给阶段3 的必改项（不是建议，是硬前提）

1. **直装包里三个核心 so 一律不许改字节**：`libmain.so` / `libunity.so` / `libil2cpp.so`。任何 native 补丁必须走 **运行时 hook**，不许用 `patch_libil2cpp_inplace` 那类静态改 so 的做法。
2. **`assets/NativeIntegrity/arm64-v8a.bin` 原样保留**，重打包时按 store/deflate 无所谓（校验的是解压后流），但**体积必须仍为 480 B 且只能有一条同名 entry**。
3. **`versionCode` 保持 32**。
4. **hook 装点固定在 ContentProvider（`StarryInitProvider`）阶段**，且在 `Application.onCreate` 前完成；`libstarry_magic.so` 允许随包新增（白名单不看它）。
5. 菜单/配置里若要"校验通过但保留证据"，用 A 方案返回 `Task.CompletedTask` 并自己打一行 log，勿去伪装签名（C 方案与「SignSpoof 禁止复活」冲突，需用户单独授权）。
