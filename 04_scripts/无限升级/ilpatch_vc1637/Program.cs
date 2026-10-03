using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// 无限升级 vc1637 / 11.266018 专用 IL 补丁器。
//
// 输入不是 12.x 的当前 DLL，而是从 commit 7a51174 取回的 v7.11 补丁 DLL：
//   1,955,328 B / SHA-256 e5ef1bf51c32a1ccd2c8e303438cecdcb1853b26e46b1a3f1e57c429e40de885
// 该 DLL 已包含 v1+v7.11 的功能与静默防封，但缺少：
//   exp_multi / shop_ads_unlimited / 版本钉死 / BanGame 清封禁标志 / 存档救命链恢复
//
// 本工具只做上述增量，不碰 12.x 链路。
// ---------------------------------------------------------------------------
// 变体（2026-09-29 补）：真机实测服务端下发的运行时 DLL 是 2,030,080 B（hotfix2），
// 而不是分析期假设的 1,960,960 B。ildiff 实测两版差 52 类型 / 512 方法（整个 MissionTree
// 系统），把 1960960 系补丁注入 2030080 官方包 = A 线当年「MissionTreeData 序列化异常 /
// 卡等待响应」的复现。故必须按运行时长度选对应版本的补丁。
//
//   v1960960  基线 v7.11（1960960 系）        打 6 项
//   hf2       基线 12.x v7.19 hotfix2（2030080 系） 只打 4 项
//   hf3       基线 12.x v7.19 hotfix3（2028544 系） 只打 4 项
//
// hf2/hf3 基线已含 exp_multi 与「存档救命链恢复」，只缺 shop_ads_unlimited /
// 版本钉死 / BanGame；重复打 exp_multi 会二次放大、重复删 block_report 门会抛错，故必须区分。
// ---------------------------------------------------------------------------
internal sealed class VariantSpec
{
    public string Name = "";
    public string Src = "";
    public string Dst = "";
    public long ExpectedSrcLen;
    public long OfficialArrayLen;
    public bool FullPatchSet;
}

internal static class Program
{
    const string PatchDir = "/Users/aliang/Documents/nixiang/07_patched/无限升级";

    static readonly VariantSpec[] Variants =
    {
        new VariantSpec
        {
            Name = "v1960960",
            Src = PatchDir + "/targetfuntional_vc1637_v711.bin",
            Dst = PatchDir + "/targetfuntional_vc1637_patched.dll",
            ExpectedSrcLen = 1955328,
            OfficialArrayLen = 1960960,
            FullPatchSet = true,
        },
        new VariantSpec
        {
            Name = "hf2",
            Src = PatchDir + "/targetfuntional_v719_hotfix2_patched.dll",
            Dst = PatchDir + "/targetfuntional_vc1637_hf2_patched.dll",
            ExpectedSrcLen = 2024960,
            OfficialArrayLen = 2030080,
            FullPatchSet = false,
        },
        new VariantSpec
        {
            Name = "hf3",
            Src = PatchDir + "/targetfuntional_v719_hotfix3_patched.dll",
            Dst = PatchDir + "/targetfuntional_vc1637_hf3_patched.dll",
            ExpectedSrcLen = 2022912,
            OfficialArrayLen = 2028544,
            FullPatchSet = false,
        },
    };

    static VariantSpec FindVariant(string name)
    {
        var v = Variants.FirstOrDefault(x => x.Name == name);
        if (v == null)
            throw new ArgumentException($"未知变体 {name}（可选：{string.Join(", ", Variants.Select(x => x.Name))}）");
        return v;
    }

    static ModuleDefinition mod;
    static TypeDefinition tStarryConfig;
    static MethodReference mGetBool;
    static MethodReference mGetInt;
    static TypeDefinition tGameData;
    static FieldReference fGameDataI;
    static FieldReference fDailyData;
    static FieldReference fHounerLevel;
    static FieldReference fBan;
    static TypeDefinition tDailyData;

    static int Main(string[] args)
    {
        string variantName = "v1960960";
        bool scanOnly = false;
        bool scanAll = false;
        bool incremental = false;
        bool noReverse = false;  // 复现性自检用：跳过 item_reverse
        bool dumpMinus = false;
        bool dumpKeys = false;
        bool dumpMethods = false;
        bool dumpRpc = false;
        bool scanNoop = false;
        var dumpSpecs = new List<string>();
        var rest = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--variant" && i + 1 < args.Length) variantName = args[++i];
            else if (args[i] == "--scan") scanOnly = true;
            else if (args[i] == "--scan2") scanAll = true;
            else if (args[i] == "--dump-minus") dumpMinus = true;
            else if (args[i] == "--dump-keys") dumpKeys = true;
            else if (args[i] == "--dump-rpc") dumpRpc = true;
            else if (args[i] == "--scan-noop") scanNoop = true;
            else if (args[i] == "--dump-methods") dumpMethods = true;
            else if (dumpMethods && !args[i].StartsWith("--")) dumpSpecs.Add(args[i]);
            else if (args[i] == "--incremental") incremental = true;
            else if (args[i] == "--no-item-reverse") noReverse = true;
            else rest.Add(args[i]);
        }

        var spec = FindVariant(variantName);
        var src = rest.Count >= 1 ? rest[0] : spec.Src;
        var dst = rest.Count >= 2 ? rest[1] : spec.Dst;

        if (!File.Exists(src)) throw new FileNotFoundException($"vc1637 [{spec.Name}] 基线 DLL 不存在", src);
        var srcLen = new FileInfo(src).Length;

        // 盘点模式：只列出所有 MinusItem 调用点（类型::方法 + IL 偏移），不改文件。
        if (scanOnly || scanAll || dumpMinus || dumpKeys || dumpMethods || dumpRpc || scanNoop)
        {
            var sc = AssemblyDefinition.ReadAssembly(src);
            mod = sc.MainModule;
            Console.WriteLine($"[scan] {src} ({srcLen} B)");
            if (scanNoop) ScanEntryNoOps();
            else if (dumpRpc) DumpRpcGates();
            else if (dumpMethods) { foreach (var s in dumpSpecs) DumpMethodBySpec(s); }
            else if (dumpKeys) DumpConfigKeys();
            else if (dumpMinus) { ResolveStarryOnly(); DumpMinusItemBody(); }
            else if (scanAll) ScanAllMinusItemCalls();
            else ScanMinusItemCalls();
            sc.Dispose();
            return 0;
        }

        // 增量模式：入口 DLL 已经是补丁产物（hf2/hf3 的 12.x 基线已丢失，无法全量重打），
        // 只追加 item_reverse 这一项，其余补丁一律不碰。
        if (incremental)
        {
            if (rest.Count < 2)
                throw new ArgumentException("--incremental 必须显式给出 <src> <dst>");
            if (Path.GetFullPath(src) == Path.GetFullPath(dst))
                throw new ArgumentException("--incremental 禁止原地覆写（src == dst）");

            var asmI = AssemblyDefinition.ReadAssembly(src);
            mod = asmI.MainModule;
            Console.WriteLine($"[load] {src} ({srcLen} B)");
            Console.WriteLine($"[mode] incremental：只追加 item_reverse，其余补丁不动");
            ResolveStarryOnly();
            int newSites = PatchItemReverse(allowExisting: true);
            int bodyFix = PatchMinusItemBody(allowExisting: true);
            int anti = PatchAntiBanToggle();
            bool banChanged = PatchBanGame();
            bool expChanged = PatchExpMulti();   // 12.x 基线缺省是 1，这里原地升到 1000
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            if (newSites == 0 && bodyFix == 0 && anti == 0 && !banChanged && !expChanged)
            {
                // 幂等：产物已经带门，原样复制（不重写，避免字节漂移）。
                asmI.Dispose();
                File.Copy(src, dst, overwrite: true);
                Console.WriteLine("[done] item_reverse / anti-ban / exp_multi 均已是目标形态，原样复制（幂等跳过）");
            }
            else
            {
                asmI.Write(dst);
                asmI.Dispose();
            }
            var outLenI = new FileInfo(dst).Length;
            Console.WriteLine($"[done] {dst} ({outLenI} B, official={spec.OfficialArrayLen}, margin={spec.OfficialArrayLen - outLenI})");
            if (outLenI > spec.OfficialArrayLen)
                throw new InvalidDataException($"补丁 DLL {outLenI} B 超过官方数组 {spec.OfficialArrayLen} B");
            VerifyItemReverse(dst);
            VerifyMinusItemReverseBodyAt(dst);
            VerifyAntiBan(dst);
            if (!ExpMultiDefaultIs(kExpMultiDefault))
                throw new Exception($"[verify] {dst}: exp_multi 缺省不是 {kExpMultiDefault}");
            Console.WriteLine($"[verify] exp_multi 缺省 ×{kExpMultiDefault}(默认 1=不放大)");
            return 0;
        }

        if (srcLen != spec.ExpectedSrcLen)
            throw new InvalidDataException($"输入 DLL 长度不是 [{spec.Name}] 基线的 {spec.ExpectedSrcLen}：{srcLen}");

        var asm = AssemblyDefinition.ReadAssembly(src);
        mod = asm.MainModule;
        Console.WriteLine($"[load] {src}");
        Console.WriteLine($"[load] types={mod.Types.Count}");
        Console.WriteLine($"[variant] {spec.Name} (full={spec.FullPatchSet}, official={spec.OfficialArrayLen})");

        ResolveCommonRefs();
        if (!noReverse) PatchItemReverse(allowExisting: false);
        if (!noReverse) PatchMinusItemBody(allowExisting: false);
        PatchShopAdsUnlimited();
        PatchVersionPin();
        PatchBanGame();
        PatchAntiBanToggle();
        if (spec.FullPatchSet)
        {
            PatchExpMulti();
            PatchSaveGuardChain();
        }
        else
        {
            // hf2/hf3 基线（12.x v7.19）已含这两项，重复打会二次放大 / 抛错。
            // exp_multi 走 PatchExpMulti()：它会识别既有补丁并只改写缺省操作数（旧的 1 → 1000）。
            PatchExpMulti();
            RequireBlockReportGateAbsent("NetWorkPlayerMgr", "CheckCodeLocked", 1);
            RequireBlockReportGateAbsent("NetWorkPlayerMgr", "OnSaveGuardFailed", 1);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
        asm.Write(dst);
        asm.Dispose();

        var outLen = new FileInfo(dst).Length;
        Console.WriteLine($"[done] {dst} ({outLen} B, official={spec.OfficialArrayLen}, margin={spec.OfficialArrayLen - outLen})");
        if (outLen > spec.OfficialArrayLen)
            throw new InvalidDataException($"补丁 DLL {outLen} B 超过官方数组 {spec.OfficialArrayLen} B");

        if (!noReverse) VerifyItemReverse(dst);
        if (!noReverse) VerifyMinusItemReverseBodyAt(dst);
        VerifyAntiBan(dst);
        Verify(dst);
        return 0;
    }

    static void ResolveStarryOnly()
    {
        tStarryConfig = FindType("StarryConfig");
        mGetBool = tStarryConfig.Methods.Single(x => x.Name == "GetBool" && x.Parameters.Count == 1);
        mGetInt = tStarryConfig.Methods.Single(x => x.Name == "GetInt" && x.Parameters.Count == 2);
    }

    // =====================================================================
    // 物品/材料/货币使用反增（item_reverse）
    //
    // 作用域：DLL 内**所有** GameData::MinusItem(string,int64) 调用点都会反增
    // （物品使用、材料消耗、货币扣减全覆盖），但下面两类强制排除：
    //
    //   1) 「库存驱动循环」内的调用点 —— 例如
    //        while (GameData.I.Enough("纯净之灵", cost)) { GameData.I.MinusItem("纯净之灵", cost); ...升级... }
    //      循环体里再查库存 ⇒ 反增后数量只增不减 ⇒ 永不退出 ⇒ 真机卡死。
    //      判据：调用点所在循环体内出现 GameData::Enough / GameData::GetAmount 等「查库存」调用。
    //      （ItemData::get_ItemAmount 之类「静态配置数量」的取值器不算，那些是有界 for/foreach。）
    //   2) 存档链（GameDatas.FixSave / DataManager::Load）：反增会写坏存档修复与读档逻辑。
    //
    // 倍率可配置：读 starry_config.ini 的 item_reverse_x（int，缺省 1000，菜单输入框写入）。
    //
    // 实现分两半（体积最小化：base 变体的官方数组只有 1,960,960 B，DLL 必须装得下）：
    //   ① 调用点（112 处）只插 8 条：把 amount 取负，表示「这次是反增请求」
    //        ldstr "item_reverse"            ┐ 门关着就原样走原逻辑
    //        call  StarryConfig.GetBool      │
    //        brfalse -> callvirt             ┘
    //        dup / ldc.i4.0 / conv.i8 / ble -> callvirt   ← amount <= 0 不反增（「返还」语义不能翻成扣减）
    //        neg                             ⇒ MinusItem(id, -amount)
    //   ② MinusItem 本体（1 处）认负额：target < 0 且 item_reverse 开 ⇒ target *= item_reverse_x
    //      然后按原逻辑 `count -= target` ⇒ 数量 +|target|×倍率。
    //      同时这一段兼作 fix D：负额跳过 skin_unlock / item_keep 两道门（否则 item_keep=1 时整方法 no-op）。
    //
    // 为什么倍率放本体：调用点 ×倍率 要 12 条指令（≈54 B/处 × 112 = 6 KB ⇒ 撑爆官方数组），
    // 放本体只写一次，每个调用点省下 ≈36 B。
    // =====================================================================
    const int kItemReverseDefaultX = 1000;
    const int kGateInstrCount = 8;   // 调用点之前的门指令数（不含 callvirt 本身）

    // 强制排除的类型（沿 DeclaringType 链上溯）
    static readonly string[] kReverseSkipTypes = { "FixSave", "DataManager" };

    sealed class ReverseSite
    {
        public MethodDefinition Method;
        public Instruction Call;
        public string Skip;   // null = 可打
    }

    static List<ReverseSite> CollectReverseSites()
    {
        var list = new List<ReverseSite>();
        foreach (var t in AllTypes(mod.Types))
        {
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                var il = m.Body.Instructions.ToList();
                var loops = LoopRanges(il);
                for (int i = 0; i < il.Count; i++)
                {
                    var ins = il[i];
                    if (ins.OpCode.Code != Code.Call && ins.OpCode.Code != Code.Callvirt) continue;
                    if (ins.Operand is not MethodReference mr || mr.Name != "MinusItem") continue;
                    if (mr.DeclaringType.Name != "GameData") continue;
                    if (mr.Parameters.Count != 2 || mr.Parameters[1].ParameterType.FullName != "System.Int64")
                        throw new Exception($"{t.FullName}::{m.Name} 的 MinusItem 重载不是 (string,int64)：{mr.FullName}");
                    list.Add(new ReverseSite { Method = m, Call = ins, Skip = SkipReason(t, il, loops, i) });
                }
            }
        }
        return list;
    }

    static string SkipReason(TypeDefinition t, List<Instruction> il, List<int[]> loops, int i)
    {
        for (var cur = t; cur != null; cur = cur.DeclaringType)
            if (Array.IndexOf(kReverseSkipTypes, cur.Name) >= 0)
                return "存档链(" + cur.Name + ")";
        foreach (var l in loops)
        {
            if (l[0] > i || i > l[1]) continue;
            for (int k = l[0]; k <= l[1]; k++)
            {
                if (il[k].OpCode.Code != Code.Call && il[k].OpCode.Code != Code.Callvirt) continue;
                if (il[k].Operand is MethodReference cr
                    && cr.DeclaringType.Name == "GameData"
                    && Array.IndexOf(kStockQueryNames, cr.Name) >= 0)
                    return $"库存驱动循环({cr.Name})";
            }
        }
        return null;
    }

    static List<Instruction> TakeBack(Instruction call, int n, bool includeCall)
    {
        var res = new LinkedList<Instruction>();
        for (var p = call.Previous; p != null && res.Count < n; p = p.Previous) res.AddFirst(p);
        if (includeCall) res.AddLast(call);
        return res.ToList();
    }

    // 0 = 没门；1 = 新形态（只把 amount 取负）；2 = 旧形态（调用点自带 ×1000，可原地升级）
    static int GateState(Instruction call)
    {
        var seq = TakeBack(call, kGateInstrCount, includeCall: true);
        if (seq.Count == kGateInstrCount + 1
            && seq[0].OpCode.Code == Code.Ldstr && (string)seq[0].Operand == "item_reverse"
            && seq[1].OpCode.Code == Code.Call && ((MethodReference)seq[1].Operand).Name == "GetBool"
            && seq[2].OpCode.Code == Code.Brfalse && ReferenceEquals(seq[2].Operand, call)
            && seq[3].OpCode.Code == Code.Dup
            && seq[4].OpCode.Code == Code.Ldc_I4_0
            && seq[5].OpCode.Code == Code.Conv_I8
            && seq[6].OpCode.Code == Code.Ble && ReferenceEquals(seq[6].Operand, call)
            && seq[7].OpCode.Code == Code.Neg)
            return 1;

        // 旧形态（2026-10-03 首版）：ldstr/GetBool/brfalse/ldc.i8 1000/mul/neg
        var old = TakeBack(call, 6, includeCall: false);
        if (old.Count == 6
            && old[0].OpCode.Code == Code.Ldstr && (string)old[0].Operand == "item_reverse"
            && old[1].OpCode.Code == Code.Call && ((MethodReference)old[1].Operand).Name == "GetBool"
            && old[2].OpCode.Code == Code.Brfalse
            && old[3].OpCode.Code == Code.Ldc_I8 && (long)old[3].Operand == 1000L
            && old[4].OpCode.Code == Code.Mul
            && old[5].OpCode.Code == Code.Neg)
            return 2;
        return 0;
    }

    static void EmitGate(MethodDefinition m, Instruction call)
    {
        var proc = m.Body.GetILProcessor();
        proc.InsertBefore(call, Instruction.Create(OpCodes.Ldstr, "item_reverse"));
        proc.InsertBefore(call, Instruction.Create(OpCodes.Call, mGetBool));
        proc.InsertBefore(call, Instruction.Create(OpCodes.Brfalse, call));
        proc.InsertBefore(call, Instruction.Create(OpCodes.Dup));
        proc.InsertBefore(call, Instruction.Create(OpCodes.Ldc_I4_0));
        proc.InsertBefore(call, Instruction.Create(OpCodes.Conv_I8));
        proc.InsertBefore(call, Instruction.Create(OpCodes.Ble, call));
        proc.InsertBefore(call, Instruction.Create(OpCodes.Neg));
        m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 4);
    }

    // 旧形态 → 新形态：调用点自带 ldc.i8 1000 / mul（倍率已挪进本体），删掉并补上正额判定。
    //   …/brfalse/ [ldc.i8 1000] [mul] neg  ⇒  …/brfalse/ dup/ldc.i4.0/conv.i8/ble neg
    static void UpgradeGate(MethodDefinition m, Instruction call)
    {
        var seq = TakeBack(call, 6, includeCall: false);
        var neg = seq[5];
        var proc = m.Body.GetILProcessor();
        proc.Remove(seq[3]);   // ldc.i8 1000
        proc.Remove(seq[4]);   // mul
        proc.InsertBefore(neg, Instruction.Create(OpCodes.Dup));
        proc.InsertBefore(neg, Instruction.Create(OpCodes.Ldc_I4_0));
        proc.InsertBefore(neg, Instruction.Create(OpCodes.Conv_I8));
        proc.InsertBefore(neg, Instruction.Create(OpCodes.Ble, call));
        m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 4);
    }

    static int PatchItemReverse(bool allowExisting)
    {
        var sites = CollectReverseSites();
        int patched = 0, upgraded = 0, existing = 0, skipped = 0;
        foreach (var s in sites)
        {
            var where = $"{s.Method.DeclaringType.FullName}::{s.Method.Name} @IL_{s.Call.Offset:X4}";
            if (s.Skip != null)
            {
                skipped++;
                Console.WriteLine($"[skip] item_reverse <- {where}  ({s.Skip})");
                continue;
            }
            int st = GateState(s.Call);
            if (st == 1)
            {
                if (!allowExisting)
                    throw new Exception($"{where} 已带 item_reverse 门（基线不该是已打过补丁的 DLL）");
                existing++;
                continue;
            }
            if (st == 2)
            {
                if (!allowExisting)
                    throw new Exception($"{where} 是旧形态 item_reverse 门（×1000 挂在调用点），必须从干净基线重打或走 --incremental");
                UpgradeGate(s.Method, s.Call);
                upgraded++;
                continue;
            }
            EmitGate(s.Method, s.Call);
            patched++;
        }
        if (patched + existing == 0) throw new Exception("未找到任何可反增的 MinusItem 调用点（基线变了？）");
        Console.WriteLine($"[patch] item_reverse -> 负额门(倍率在本体 item_reverse_x，缺省 {kItemReverseDefaultX})，新注入 {patched} 处 / 旧形态升级 {upgraded} 处 / 已存在 {existing} 处 / 排除 {skipped} 处（共 {sites.Count} 处调用点）");
        return patched;
    }

    static void VerifyItemReverse(string path)
    {
        var asm = AssemblyDefinition.ReadAssembly(path);
        mod = asm.MainModule;
        ResolveStarryOnly();
        var sites = CollectReverseSites();
        int ok = 0, skip = 0, bad = 0;
        foreach (var s in sites)
        {
            if (s.Skip != null) { skip++; continue; }
            if (GateState(s.Call) == 1) ok++;
            else bad++;
        }
        if (ok == 0) throw new Exception("verify failed: item_reverse 无有效注入点");
        if (bad != 0) throw new Exception($"verify failed: {bad} 处可反增调用点缺门");
        VerifyMinusItemReverseBody();
        Console.WriteLine($"[verify] item_reverse 门（缺省 ×{kItemReverseDefaultX}，item_reverse_x 可调）@ {ok} 处；排除 {skip} 处");
        asm.Dispose();
    }


    // 「使用物品」三族类型：仅供 --scan 打 [USE-PATH] 标记参考（真实的打点范围见 CollectReverseSites）。
    static readonly string[] kItemUseTypeNames = { "ItemDetialWindow", "SelectLegacyPetItemUse", "SelectMiFaItemUse" };

    // 调试用：列出所有「入口 ret + 后面还留着原方法体」的方法 —— 这就是 δ 层「无条件 no-op」的指纹。
    // 用来把防封族里「无开关」的那批全部找出来（改造成可切换前必须知道全量）。
    static void ScanEntryNoOps()
    {
        int n = 0;
        foreach (var t in mod.GetTypes())
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                var il = m.Body.Instructions;
                if (il.Count < 2 || il[0].OpCode.Code != Code.Ret) continue;
                n++;
                var second = il[1];
                string tag = second.OpCode.Code == Code.Ldstr ? $"ldstr \"{second.Operand}\"" : second.OpCode.ToString();
                Console.WriteLine($"[noop] {t.FullName}::{m.Name}/{m.Parameters.Count}  il={il.Count}  第二条={tag}");
            }
        Console.WriteLine($"[noop] 共 {n} 个入口-ret 无条件 no-op");
    }

    static string Fmt(object o) => o switch
    {
        null => "",
        ParameterDefinition pd => $"param#{pd.Index}({pd.Name})",
        VariableDefinition vd => $"local#{vd.Index}({vd.VariableType.Name})",
        Instruction t => $"IL_{t.Offset:X4}",
        _ => o?.ToString() ?? ""
    };

    // 调试用：找出所有调用 StarryConfig::StrictBlockedRpc 的通道，打印门周围 IL。
    static void DumpRpcGates()
    {
        int n = 0;
        foreach (var t in mod.GetTypes())
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                var il = m.Body.Instructions;
                int hit = -1;
                for (int i = 0; i < il.Count; i++)
                {
                    if (il[i].OpCode.Code != Code.Call) continue;
                    if (!(il[i].Operand is MethodReference mr)) continue;
                    if (mr.Name == "StrictBlockedRpc") { hit = i; break; }
                }
                if (hit < 0) continue;
                n++;
                int lo = Math.Max(0, hit - 6), hi = Math.Min(il.Count - 1, hit + 10);
                Console.WriteLine($"[rpc] #{n} {t.FullName}::{m.Name}  (调用点 idx={hit})");
                for (int i = lo; i <= hi; i++)
                    Console.WriteLine($"[rpc]   {i,4} IL_{il[i].Offset:X4} {il[i].OpCode.Code,-14} {Fmt(il[i].Operand)}");
            }
        Console.WriteLine($"[rpc] 共 {n} 个 StrictBlockedRpc 门");
    }

    // 调试用：按 "Type::Method"（可带参数个数后缀 "Type::Method/1"）打印方法 IL。
    static void DumpMethodBySpec(string spec)
    {
        int argc = -1;
        int slash = spec.LastIndexOf('/');
        if (slash > 0 && int.TryParse(spec.Substring(slash + 1), out var n)) { argc = n; spec = spec.Substring(0, slash); }
        int sep = spec.IndexOf("::", StringComparison.Ordinal);
        var typeName = spec.Substring(0, sep);
        var methName = spec.Substring(sep + 2);
        var cands = new List<MethodDefinition>();
        foreach (var t in mod.GetTypes())
        {
            if (t.Name != typeName && t.FullName != typeName) continue;
            foreach (var m in t.Methods)
                if (m.Name == methName && (argc < 0 || m.Parameters.Count == argc)) cands.Add(m);
        }
        if (cands.Count == 0) { Console.WriteLine($"[dump] !! 找不到 {spec}"); return; }
        foreach (var m in cands)
        {
            Console.WriteLine($"[dump] === {m.FullName}  HasThis={m.HasThis} params={m.Parameters.Count} locals={(m.HasBody ? m.Body.Variables.Count : -1)} maxstack={(m.HasBody ? m.Body.MaxStackSize : -1)}");
            if (!m.HasBody) { Console.WriteLine("[dump]   (无方法体)"); continue; }
            var il = m.Body.Instructions;
            for (int i = 0; i < il.Count; i++)
                Console.WriteLine($"[dump]   {i,4} IL_{il[i].Offset:X4} {il[i].OpCode}  {Fmt(il[i].Operand)}");
        }
    }

    // 调试用：盘点整个 DLL 里所有 StarryConfig.GetBool/GetInt/GetString("key") 读取点。
    // 用来确认「哪些补丁已经是运行时可切换的」，以及防封族的实际门控键分布。
    static void DumpConfigKeys()
    {
        var seen = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var t in mod.GetTypes())
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                var il = m.Body.Instructions;
                for (int i = 0; i + 1 < il.Count; i++)
                {
                    if (il[i].OpCode.Code != Code.Ldstr) continue;
                    var nx = il[i + 1];
                    if (nx.OpCode.Code != Code.Call && nx.OpCode.Code != Code.Callvirt) continue;
                    var mr = nx.Operand as MethodReference;
                    if (mr == null || mr.DeclaringType?.Name != "StarryConfig") continue;
                    if (mr.Name != "GetBool" && mr.Name != "GetInt" && mr.Name != "GetString") continue;
                    var key = (string)il[i].Operand;
                    if (!seen.TryGetValue(key, out var l)) { l = new List<string>(); seen[key] = l; }
                    l.Add($"{mr.Name} @ {t.FullName}::{m.Name}");
                }
            }
        Console.WriteLine($"[keys] 共 {seen.Count} 个配置键");
        foreach (var kv in seen)
        {
            Console.WriteLine($"[key] {kv.Key}  ({kv.Value.Count} 处)");
            foreach (var s in kv.Value) Console.WriteLine($"        {s}");
        }
    }

    // 调试用：打印 GameData::MinusItem(string,int64) 的完整 IL（参数/局部变量索引一目了然）。
    static void DumpMinusItemBody()
    {
        var m = FindMinusItemBody();
        Console.WriteLine($"[dump] {m.FullName}  HasThis={m.HasThis} params={m.Parameters.Count} locals={m.Body.Variables.Count} maxstack={m.Body.MaxStackSize}");
        for (int i = 0; i < m.Parameters.Count; i++)
            Console.WriteLine($"[dump]   param[{i}] index={m.Parameters[i].Index} {m.Parameters[i].ParameterType.FullName} {m.Parameters[i].Name}");
        for (int i = 0; i < m.Body.Variables.Count; i++)
            Console.WriteLine($"[dump]   local[{i}] {m.Body.Variables[i].VariableType.FullName}");
        var il = m.Body.Instructions;
        for (int i = 0; i < il.Count; i++)
        {
            var x = il[i];
            string op = x.Operand switch
            {
                null => "",
                ParameterDefinition pd => $"param#{pd.Index}({pd.Name})",
                VariableDefinition vd => $"local#{vd.Index}({vd.VariableType.Name})",
                Instruction t => $"IL_{t.Offset:X4}",
                _ => x.Operand.ToString()
            };
            Console.WriteLine($"[dump]   {i,4} IL_{x.Offset:X4} {x.OpCode.Code,-14} {op}");
        }
    }

    static bool InUseFamily(TypeDefinition t)
    {
        for (var cur = t; cur != null; cur = cur.DeclaringType)
            if (Array.IndexOf(kItemUseTypeNames, cur.Name) >= 0) return true;
        return false;
    }

    // 循环区间检测：每条「向后跳转」的回边 (tgtIdx -> srcIdx) 构成一个循环体 [tgtIdx, srcIdx]。
    // 用途：判定某个 MinusItem 调用点是否落在循环里（循环里消耗→反增会永不退出）。
    static List<int[]> LoopRanges(ICollection<Instruction> il)
    {
        var list = il.ToList();
        var idx = new Dictionary<Instruction, int>();
        for (int i = 0; i < list.Count; i++) idx[list[i]] = i;
        var res = new List<int[]>();
        for (int j = 0; j < list.Count; j++)
        {
            if (list[j].Operand is Instruction tgt)
            {
                if (idx.TryGetValue(tgt, out int t) && t < j) res.Add(new[] { t, j });
            }
            else if (list[j].Operand is Instruction[] tgs)
            {
                foreach (var tt in tgs)
                    if (idx.TryGetValue(tt, out int t2) && t2 < j) res.Add(new[] { t2, j });
            }
        }
        return res;
    }

    // 「再查库存」类调用：循环体里若出现这些，说明该循环由库存驱动 ⇒ 反增会让它永不退出。
    static readonly string[] kStockQueryNames =
    {
        "Enough", "GetAmount", "GetItemAmount", "GetItemNum", "GetNum", "GetCount",
        "GetItemCount", "HaveItem", "IsEnough", "GetItemSum",
    };

    static void ScanAllMinusItemCalls()
    {
        int n = 0, inLoop = 0, danger = 0;
        foreach (var t in AllTypes(mod.Types))
        {
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                var il = m.Body.Instructions.ToList();
                var loops = LoopRanges(il);
                for (int i = 0; i < il.Count; i++)
                {
                    var ins = il[i];
                    if (ins.OpCode.Code != Code.Call && ins.OpCode.Code != Code.Callvirt) continue;
                    if (ins.Operand is not MethodReference mr || mr.Name != "MinusItem") continue;
                    n++;
                    var marks = new List<string>();
                    if (InUseFamily(t)) marks.Add("USE-PATH");
                    var enc = loops.Where(l => l[0] <= i && i <= l[1]).ToList();
                    if (enc.Count > 0) { marks.Add($"LOOPx{enc.Count}"); inLoop++; }
                    var skip = SkipReason(t, il, loops, i);
                    if (skip != null) { danger++; marks.Add("SKIP(" + skip + ")"); }
                    else marks.Add("PATCH");
                    Console.WriteLine($"[scan2] {t.FullName}::{m.Name} @IL_{ins.Offset:X4}  {(marks.Count > 0 ? string.Join(" ", marks) : "-")}");
                    if (enc.Count > 0)
                    {
                        var names = new SortedSet<string>();
                        foreach (var l in enc)
                            for (int k = l[0]; k <= l[1]; k++)
                            {
                                if (il[k].OpCode.Code != Code.Call && il[k].OpCode.Code != Code.Callvirt) continue;
                                if (il[k].Operand is MethodReference cr) names.Add(cr.DeclaringType.Name + "::" + cr.Name);
                            }
                        Console.WriteLine($"[scan2-loop]   calls = {string.Join(", ", names)}");
                    }
                }
            }
        }
        Console.WriteLine($"[scan2] total={n} inLoop={inLoop} stockDriven={danger}");
    }

    static void ScanMinusItemCalls()
    {
        int n = 0;
        foreach (var t in AllTypes(mod.Types))
        {
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                foreach (var ins in m.Body.Instructions)
                {
                    if (ins.OpCode.Code != Code.Call && ins.OpCode.Code != Code.Callvirt) continue;
                    if (ins.Operand is not MethodReference mr || mr.Name != "MinusItem") continue;
                    n++;
                    Console.WriteLine($"[scan] {mr.DeclaringType.FullName}::{mr.Name}({mr.Parameters.Count}p)  <- {t.FullName}::{m.Name} @IL_{ins.Offset:X4}{(InUseFamily(t) ? "   [USE-PATH]" : "")}");
                }
            }
        }
        Console.WriteLine($"[scan] total MinusItem call sites = {n}");
    }

    // ------------------------------------------------------------------
    // item_reverse 的本体端：GameData::MinusItem(string targetItem, long target)
    //
    // 调用点侧只把 amount 取负（-target 表示「这次是反增请求」），倍率与守卫绕过都在这里：
    //   target >= 0  ⇒ 完全走原逻辑（含 skin_unlock / item_keep 两道门）——正向扣减零影响，
    //                  消耗驱动的 while 循环（如「纯净之灵」）行为不变，不会卡死。
    //   target <  0  ⇒ 跳过 skin_unlock / item_keep（这两道门开着时整个方法是 no-op，真机
    //                  item_keep=1 ⇒ 反增注入的负额扣减什么也不发生 = 首轮「没效果」的根因）；
    //                  再乘 item_reverse_x（缺省 1000）⇒ 原逻辑 count -= target ⇒ 数量 +|target|×倍率。
    //                  ContainsKey 检查保留 ⇒ 不在 _allItemCode 里的特殊物品不会被写坏。
    //
    // 这里不引入任何新的外部方法引用（System.Math.Max 之类可能被 AOT 裁剪），夹取下限用自家分支。
    // ------------------------------------------------------------------
    const int kItemReverseMinX = 1;
    const int kItemReverseMaxX = 1000000;   // 与菜单侧 WxsjVc1637Mod.cpp 的夹取保持一致

    static MethodDefinition FindMinusItemBody()
    {
        return FindType("GameData").Methods.SingleOrDefault(x => x.Name == "MinusItem"
                && x.Parameters.Count == 2
                && x.Parameters[1].ParameterType.FullName == "System.Int64"
                && x.HasBody)
            ?? throw new Exception("GameData::MinusItem(string,int64) 未找到（基线变了？）");
    }

    // 本体状态：head = 「负额跳过 skin_unlock/item_keep 守卫链」的序言；scale = 「target<0 时乘 item_reverse_x」的取数块。
    //   都没有       → 干净基线（还没打过 item_reverse）
    //   只有 head    → 旧形态（首版：倍率硬编码在调用点的 ldc.i8 1000 / mul）
    //   head + scale → 新形态（本版：调用点只取负，倍率在本体里读 item_reverse_x）
    static (bool head, bool scale) MinusItemBodyState(MethodDefinition m)
    {
        var il = m.Body.Instructions;
        bool head = il.Count >= 4
            && il[0].OpCode.Code == Code.Ldarg_2
            && il[1].OpCode.Code == Code.Ldc_I8 && (long)il[1].Operand == 0L
            && il[2].OpCode.Code == Code.Clt
            && (il[3].OpCode.Code == Code.Brtrue || il[3].OpCode.Code == Code.Brfalse);
        bool scale = false;
        foreach (var x in il)
            if (x.OpCode.Code == Code.Ldstr && (string)x.Operand == "item_reverse_x") { scale = true; break; }
        return (head, scale);
    }

    // 读 target 参数（IL 参数槽 2 = 第 2 个形参，因为 HasThis）的指令。
    static bool IsTargetRead(Instruction x)
    {
        if (x.OpCode.Code == Code.Ldarg_2) return true;
        if ((x.OpCode.Code == Code.Ldarg_S || x.OpCode.Code == Code.Ldarg)
            && x.Operand is ParameterDefinition pd && pd.Index == 1) return true;
        return false;
    }

    // 方法体里真正「读 target」的位置，跳过开头 4 条负额序言（那里也有一条 ldarg.2）。
    static List<Instruction> TargetReads(MethodDefinition m)
    {
        var il = m.Body.Instructions.ToList();
        int skip = 0;
        if (il.Count >= 4
            && il[0].OpCode.Code == Code.Ldarg_2
            && il[1].OpCode.Code == Code.Ldc_I8 && (long)il[1].Operand == 0L
            && il[2].OpCode.Code == Code.Clt
            && (il[3].OpCode.Code == Code.Brtrue || il[3].OpCode.Code == Code.Brfalse)) skip = 4;
        var res = new List<Instruction>();
        for (int i = skip; i < il.Count; i++)
            if (IsTargetRead(il[i])) res.Add(il[i]);
        return res;
    }

    // 守卫链出口 = 最后一条 `call GetBool` 之后那个条件跳转的落点（即守卫链之后的第一条指令）。
    static Instruction FindGuardExit(ICollection<Instruction> il)
    {
        var a = il.ToList();
        Instruction exit = null;
        for (int i = 0; i + 1 < a.Count; i++)
        {
            if (a[i].OpCode.Code != Code.Call) continue;
            if (a[i].Operand is not MethodReference gb || gb.Name != "GetBool") continue;
            var nx = a[i + 1];
            if (nx.OpCode.Code != Code.Brfalse && nx.OpCode.Code != Code.Brfalse_S) continue;
            if (nx.Operand is Instruction tgt) exit = tgt;
        }
        return exit;
    }

    static int PatchMinusItemBody(bool allowExisting)
    {
        var m = FindMinusItemBody();
        var (hasHead, hasScale) = MinusItemBodyState(m);

        if (hasHead && hasScale)
        {
            if (!allowExisting)
                throw new Exception($"{m.FullName} 已带 item_reverse 本体补丁（基线不该是已打过补丁的 DLL）");
            Console.WriteLine("[patch] minus_item_reverse_body 已存在，跳过");
            return 0;
        }
        if (hasHead && !hasScale && !allowExisting)
            throw new Exception($"{m.FullName} 是旧形态 fix D 序言（负额不乘倍率），必须走 --incremental 原地升级");

        int ops = 0;
        // 顺序要紧：先在「干净的」方法体上定位守卫链出口，再插取数块。
        // 反过来会把取数块里的 `call GetBool / brfalse` 误当成守卫链的最后一跳，
        // 负额序言就会直接跳到 `Sub` 上（空栈 ⇒ 非法 IL）。
        Instruction exit = null;
        if (!hasHead)
        {
            exit = FindGuardExit(m.Body.Instructions)
                ?? throw new Exception("未找到 GameData::MinusItem 的 skin_unlock/item_keep 守卫链（基线变了？）");
            var first = m.Body.Instructions.First();
            var proc = m.Body.GetILProcessor();
            proc.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_2));
            proc.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I8, 0L));
            proc.InsertBefore(first, Instruction.Create(OpCodes.Clt));
            proc.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, exit));   // target<0 ⇒ 跳过 skin_unlock/item_keep
            m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 4);
            Console.WriteLine($"[patch] minus_item_neg_bypass -> {m.FullName} 负额跳过 item_keep/skin_unlock，落点 IL_{exit.Offset:X4}");
            ops++;
        }
        if (!hasScale) ops += EmitScaleBlocks(m);
        return ops;
    }

    // 在每处「读 target 参数」的指令之后，把刚压栈的 target 就地换成「需要时 ×item_reverse_x」的版本：
    //   ldarg.2                                   ← 原指令，栈: [.., t]
    //   dup / ldc.i8 0 / clt / brfalse_S done     ← 正额：原样透传（原逻辑完全不受影响）
    //   ldstr "item_reverse" / call GetBool / brfalse_S done
    //   ldstr "item_reverse_x" / ldc.i4 1000 / call GetInt(string,int)
    //   dup / ldc.i4.1 / bge ok / pop / ldc.i4.1                    ← x < 1 时夹到 1
    //   dup / ldc.i4.1000000 / ble ok2 / pop / ldc.i4.1000000        ← x > 1e6 时夹到 1e6（防溢出成负）
    //   ok: conv.i8 / mul                         ← [.., t×X]
    //   done:                                     ← 调用方原逻辑 `_allItemCode[item] = 原值 - 这个值`
    // 不引入任何新的外部方法引用（System.Math.Max 之类可能被 AOT 裁剪），夹取下限用自家分支。
    static int EmitScaleBlocks(MethodDefinition m)
    {
        var reads = TargetReads(m);
        if (reads.Count == 0)
            throw new Exception("GameData::MinusItem 里找不到 target 参数的读取（基线变了？）");
        foreach (var rd in reads)
        {
            var anchor = rd.Next
                ?? throw new Exception("GameData::MinusItem: target 读取之后没有指令（基线变了？）");
            var proc = m.Body.GetILProcessor();
            var done = Instruction.Create(OpCodes.Nop);
            var conv = Instruction.Create(OpCodes.Conv_I8);
            var bge = Instruction.Create(OpCodes.Bge, conv);    // 占位
            var ble = Instruction.Create(OpCodes.Ble, conv);    // 占位
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Dup));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldc_I8, 0L));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Clt));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Brfalse_S, done));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldstr, "item_reverse"));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Call, mGetBool));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Brfalse_S, done));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldstr, "item_reverse_x"));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldc_I4, kItemReverseDefaultX));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Call, mGetInt));
            var maxHead = Instruction.Create(OpCodes.Dup);   // 高限段的起头（也是低限段的落点）
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Dup));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldc_I4, kItemReverseMinX));
            proc.InsertBefore(anchor, bge);
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Pop));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldc_I4, kItemReverseMinX));
            proc.InsertBefore(anchor, maxHead);
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldc_I4, kItemReverseMaxX));
            proc.InsertBefore(anchor, ble);
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Pop));
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Ldc_I4, kItemReverseMaxX));
            proc.InsertBefore(anchor, conv);
            proc.InsertBefore(anchor, Instruction.Create(OpCodes.Mul));
            proc.InsertBefore(anchor, done);
            bge.Operand = maxHead;
            ble.Operand = conv;
        }
        m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 6);
        Console.WriteLine($"[patch] minus_item_reverse_x -> {m.FullName} 负额 ×item_reverse_x(缺省 {kItemReverseDefaultX}) @ {reads.Count} 处取数点");
        return reads.Count;
    }

    static void VerifyMinusItemReverseBody()
    {
        var m = FindMinusItemBody();
        var (hasHead, hasScale) = MinusItemBodyState(m);
        if (!hasHead) throw new Exception("verify failed: minus_item_reverse_body 负额跳过守卫链的序言不在");
        if (!hasScale) throw new Exception("verify failed: minus_item_reverse_body 缺 ×item_reverse_x 取数块（旧形态？）");
        var reads = TargetReads(m);
        if (reads.Count == 0) throw new Exception("verify failed: minus_item_reverse_body 找不到 target 取数点");
        int ok = 0;
        foreach (var rd in reads)
            if (rd.Next != null && rd.Next.OpCode.Code == Code.Dup) ok++;
        if (ok != reads.Count)
            throw new Exception($"verify failed: target 取数点 {ok}/{reads.Count} 包上了 ×item_reverse_x 块");
        Console.WriteLine($"[verify] minus_item_reverse_body @ {m.FullName}（负额 ×item_reverse_x，正额走原逻辑）@ {ok} 处取数点");
    }

    static void VerifyMinusItemReverseBodyAt(string path)
    {
        var asm = AssemblyDefinition.ReadAssembly(path);
        mod = asm.MainModule;
        VerifyMinusItemReverseBody();
        asm.Dispose();
    }

    static void ResolveCommonRefs()
    {
        tStarryConfig = FindType("StarryConfig");
        mGetBool = tStarryConfig.Methods.Single(x => x.Name == "GetBool" && x.Parameters.Count == 1);
        mGetInt = tStarryConfig.Methods.Single(x => x.Name == "GetInt" && x.Parameters.Count == 2);

        tGameData = FindType("GameData");
        fGameDataI = tGameData.Fields.Single(x => x.Name == "I" && x.IsStatic);
        fDailyData = tGameData.Fields.Single(x => x.Name == "dailyData");
        fHounerLevel = tGameData.Fields.Single(x => x.Name == "hounerLevel");
        fBan = tGameData.Fields.Single(x => x.Name == "ban");
        tDailyData = fDailyData.FieldType.Resolve()
            ?? throw new Exception("GameData.dailyData 类型无法解析");
    }

    static void PatchShopAdsUnlimited()
    {
        var show = FindM(FindType("AdsManager/ShowAdsNewFlow"), "Show");
        InsertConfigGate(show, "shop_ads_unlimited", ZeroDailyAdCounters());
        Console.WriteLine("[patch] shop_ads_unlimited -> AdsManager.ShowAdsNewFlow.Show 清零每日广告计数");

        var refresh = FindM(FindType("ShopWindow"), "Refresh", 0);
        InsertConfigGate(refresh, "shop_ads_unlimited", ZeroDailyAdCounters());
        Console.WriteLine("[patch] shop_ads_unlimited -> ShopWindow.Refresh 清零每日广告计数");
    }

    static List<Instruction> ZeroDailyAdCounters()
    {
        var names = new[]
        {
            "adsAmount",
            "miFaLibao",
            "equipLiBao",
            "yiWangShui",
            "baoShiLimit",
            "yuangulimit",
            "yunXingSuiPian",
            "mowudan",
            "aomiYao",
            "shuangbeiyao",
        };
        var fields = names.Select(n =>
        {
            var f = tDailyData.Fields.FirstOrDefault(x => x.Name == n);
            if (f == null) throw new Exception($"DailyData.{n} 不存在");
            return (FieldReference)f;
        }).ToArray();

        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldsfld, fGameDataI),
            Instruction.Create(OpCodes.Ldfld, fDailyData),
        };
        foreach (var f in fields)
        {
            seq.Add(Instruction.Create(OpCodes.Dup));
            seq.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            seq.Add(Instruction.Create(OpCodes.Stfld, f));
        }
        seq.Add(Instruction.Create(OpCodes.Pop));
        return seq;
    }

    // 经验倍率缺省值（菜单 feat 12 输入框同为 1000；DLL 侧 GetInt 缺键时取它）
    const int kExpMultiDefault = 1;   // 默认 1 = 不放大（官方行为）；倍率由菜单滑块 1..1000 下发

    // 目标形态：DataManager::AddExp(LargeNumber) 入口
    //     int v = StarryConfig.GetInt("exp_multi", 1); if (v > 1) exp *= (double)v;
    // 幂等 + 可升级：产物里已存在该补丁（12.x 时代默认写的是 1）时只改默认操作数，
    // 绝不重复插入（重复插入会二次放大）。返回是否发生了改动（给 --incremental 判幂等）。
    static bool PatchExpMulti()
    {
        var m = FindM(FindType("DataManager"), "AddExp", 1);

        var exist = m.Body.Instructions.FirstOrDefault(
            x => x.OpCode == OpCodes.Ldstr && (string)x.Operand == "exp_multi");
        if (exist != null)
        {
            var il = m.Body.Instructions;
            var ni = il[il.IndexOf(exist) + 1];
            if (!ni.OpCode.Name.StartsWith("ldc.i4"))
                throw new Exception($"DataManager::AddExp 的 exp_multi 补丁形态异常：ldstr 后不是 ldc.i4，而是 {ni.OpCode.Name}");
            if (ni.OpCode == OpCodes.Ldc_I4 && ni.Operand is int cur && cur == kExpMultiDefault)
            {
                Console.WriteLine($"[patch] exp_multi -> 已是最新形态（缺省 ×{kExpMultiDefault}(默认 1=不放大)），原样保留");
                return false;
            }
            ni.OpCode = OpCodes.Ldc_I4;
            ni.Operand = kExpMultiDefault;
            Console.WriteLine($"[patch] exp_multi -> 既有补丁原地升级：缺省倍率改写为 ×{kExpMultiDefault}");
            return true;
        }

        var pExp = m.Parameters[0];
        var scLn = mod.AssemblyReferences.FirstOrDefault(r => r.Name == "com.votrubec.largenumbers")
            ?? throw new Exception("com.votrubec.largenumbers 程序集引用不存在");
        var tLn = new TypeReference(pExp.ParameterType.Namespace, pExp.ParameterType.Name, mod, scLn);

        var mMul = new MethodReference("op_Multiply", tLn, tLn) { HasThis = false };
        mMul.Parameters.Add(new ParameterDefinition(tLn));
        mMul.Parameters.Add(new ParameterDefinition(mod.TypeSystem.Double));

        var vMult = new VariableDefinition(mod.TypeSystem.Int32);
        m.Body.Variables.Add(vMult);
        m.Body.InitLocals = true;

        var first = m.Body.Instructions[0];
        var orig = Instruction.Create(OpCodes.Nop);
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "exp_multi"),
            Instruction.Create(OpCodes.Ldc_I4, kExpMultiDefault),
            Instruction.Create(OpCodes.Call, mGetInt),
            Instruction.Create(OpCodes.Stloc, vMult),
            Instruction.Create(OpCodes.Ldloc, vMult),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble, orig),
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Ldloc, vMult),
            Instruction.Create(OpCodes.Conv_R8),
            Instruction.Create(OpCodes.Call, mMul),
            Instruction.Create(OpCodes.Starg_S, pExp),
            orig,
            Instruction.Create(OpCodes.Br, first),
        };
        InsertAtStart(m, seq);
        Console.WriteLine($"[patch] exp_multi -> DataManager.AddExp(LargeNumber) 入口 ×N（缺省 ×{kExpMultiDefault}(默认 1=不放大)）");
        return true;
    }

    // 校验 DataManager::AddExp 的 exp_multi 缺省操作数（供 Verify 用）
    static bool ExpMultiDefaultIs(int want)
    {
        var m = FindM(FindType("DataManager"), "AddExp", 1);
        var il = m.Body.Instructions;
        var i = il.IndexOf(il.FirstOrDefault(
            x => x.OpCode == OpCodes.Ldstr && (string)x.Operand == "exp_multi"));
        if (i < 0) return false;
        var ni = il[i + 1];
        return ni.OpCode == OpCodes.Ldc_I4 && ni.Operand is int v && v == want;
    }

    static void PatchVersionPin()
    {
        NoOpAlways(FindM(FindType("NetWorkPlayerMgr"), "Vch", 0), "NetWorkPlayerMgr.Vch");
        NoOpAlways(FindM(FindType("NetWorkPlayerMgr"), "BlockVersion", 0), "NetWorkPlayerMgr.BlockVersion");
        NoOpAlways(FindM(FindType("InfinityCore.NetDataCalculater"), "CurrentVersion", 1), "InfinityCore.NetDataCalculater.CurrentVersion");
        NoOpAlways(FindM(FindType("MainUIFuntions"), "ShowNewVersion", 0), "MainUIFuntions.ShowNewVersion");
        NoOpAlways(FindM(FindType("MainUIFuntions"), "OnNewVersionClick", 0), "MainUIFuntions.OnNewVersionClick");
        NoOpAlways(FindM(FindType("LowVersionMgr"), "NeedUpdate", 0), "LowVersionMgr.NeedUpdate");
        NoOpAlways(FindM(FindType("LowVersionMgr"), "OpenUrl", 1), "LowVersionMgr.OpenUrl");
        NoOpAlways(FindM(FindType("LowVersionMgr_InitScene"), "ShowOnShow", 0), "LowVersionMgr_InitScene.ShowOnShow");
        NoOpAlways(FindM(FindType("LowVersionMgr_InitScene"), "OnDownLoadClick", 0), "LowVersionMgr_InitScene.OnDownLoadClick");

        ReturnLongMax(FindM(FindType("ResVersionApi"), "GetVersion", 0));
        ReplaceApplicationVersion(FindM(FindType("InfinityCore.TransferHandler"), "CfgCallBack", 1));
        ReplaceApplicationVersion(FindM(FindType("NetWorkPlayerMgr"), "UpDatePlayerData", 7));
        ReplaceApplicationVersion(FindM(FindType("NetWorkPlayerMgr"), "UpDatePlayerMainData", 1));
        Console.WriteLine("[patch] version pin -> 屏蔽强制更新/版本封锁/推荐更新链路");
    }

    // =====================================================================
    // 防封开关（菜单 feat 16「防封」→ starry_config.ini 的 block_report）
    //
    // 基线里「防封」其实是三块，本方法把三块统一挂到同一个键上：
    //   A) 37 处 `if (StarryConfig.GetBool("block_report")) return/throw;` 门
    //      （上报 / 云存档 / COS 直传 / 腾讯 CLS / 排行与数据同步 / 作弊监控 /
    //       执法 RPC / ItemAddSourceAnomalyMonitor）—— 天生跟键走，一条 IL 都不用改。
    //   B) 6 处 δ 层「入口 ret + 原方法体还在」的无条件封号拦截（见 kAntiBanEntryRets，
    //      第 6 处 NetWorkPlayerMgr::BanGame 由 PatchBanGame 单独处理）；
    //      删掉入口那条 ret，就回落到它们体内自带的 block_report 门 ⇒ 随开关。
    //   C) StarryConfig::StrictBlockedRpc（13 项「纯上报」名单，19 个 SendRT/Send/SendBA 通道共用）
    //      —— 前插 `if (!block_report) return false;` ⇒ 关掉防封后出站过滤整体失效。
    //
    // 恒常开、**不**纳入开关（用户 m02113 点名「拦截更新的不适合做进去」）：
    //   · 版本拦截族：PatchVersionPin 的 9 个 NoOpAlways + ResVersionApi.GetVersion→Int64.Max
    //     + 3 处 ReplaceApplicationVersion。关掉它 ⇒ 游戏接受官方热更 DLL（长度变了）
    //     ⇒ 内嵌补丁 DLL 与官方数组长度不匹配 ⇒ WxsjMod 的 `pLen <= len` 不成立 ⇒ 静默 skip
    //     （跑官方 DLL、补丁全失效且菜单不报错）。
    //   · 存档救命链：CheckCodeLocked（唯一回滚链）/ OnSaveGuardFailed（冻结气囊）
    //     —— PatchSaveGuardChain 已删门（v7.18 丢档教训）。与防封正交，绝不能随开关。
    //
    // 语义：block_report 缺键 = false = 关（StarryConfig.GetBool 实现：Get() 返回 null → false）。
    static readonly (string Type, string Method)[] kAntiBanEntryRets =
    {
        ("NetWorkPlayerMgr", "BanGameAciton"),
        ("InfinityCoreNormal.NetConver", "CheckGamePlayerAction"),
        ("InfinityCoreNormal.NetConver", "ForceBanGameAciton"),
        ("InfinityCoreNormal.NetConver", "UpDateBanStatu"),
        ("InfinityCore.NetDataCalculater", "PlayerOnLine"),
    };

    static int PatchAntiBanToggle()
    {
        int n = 0;
        foreach (var spec in kAntiBanEntryRets)
        {
            var m = FindM(FindType(spec.Type), spec.Method);
            var il = m.Body.Instructions;
            if (il[0].OpCode == OpCodes.Ret)
            {
                il.RemoveAt(0);
                RequireBlockReportGate(m, $"{spec.Type}::{spec.Method}");
                n++;
                Console.WriteLine($"[patch] anti-ban -> 解除 {spec.Type}::{spec.Method} 的无条件 no-op（回落到 block_report 门）");
            }
            else
            {
                RequireBlockReportGate(m, $"{spec.Type}::{spec.Method}");
                Console.WriteLine($"[patch] anti-ban -> {spec.Type}::{spec.Method} 已是可切换形态（幂等跳过）");
            }
        }
        PatchStrictBlockedRpcGate();
        Console.WriteLine($"[patch] anti-ban -> 防封按钮就绪：本轮解除无条件 no-op {n} 处；键 block_report（缺键=false=关）");
        return n;
    }

    // 删掉入口 ret 后必须真的能回落到体内的 block_report 门，否则防封关掉就没人挡了。
    static void RequireBlockReportGate(MethodDefinition m, string tag)
    {
        bool found = m.Body.Instructions.Any(x => x.OpCode.Code == Code.Ldstr && (string)x.Operand == "block_report");
        if (!found) throw new Exception($"{tag} 体内没有 block_report 门（防封关掉后无法回落，基线变了？）");
    }

    // δ v7.11 的 19 处出站通道共用这个判定（true=拦）：前插「防封关就一律不拦」。
    static void PatchStrictBlockedRpcGate()
    {
        var m = FindType("StarryConfig").Methods.Single(x => x.Name == "StrictBlockedRpc" && x.Parameters.Count == 1);
        var il = m.Body.Instructions;
        if (il[0].OpCode.Code == Code.Ldstr && (string)il[0].Operand == "block_report")
        {
            Console.WriteLine("[patch] anti-ban -> StrictBlockedRpc 已带门（幂等跳过）");
            return;
        }
        InsertAtStart(m, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "block_report"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brtrue, il[0]),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Ret),
        });
        m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 2);
        Console.WriteLine("[patch] anti-ban -> StrictBlockedRpc 前插 `if(!block_report) return false`（19 通道纯上报名单随开关）");
    }

    // PatchBanGame：防封开 ⇒ 清 GameData.ban 并返回（不被锁死）；防封关 ⇒ 回落到原体
    // （BanGameAciton → 官方封号表现：上传 ban 存档 + banMask + timeScale=0 + 提示）。
    // vc1637 第2轮之前是无条件清 ban；升级时先拆掉旧插的那 4 条再重插门控版。
    // 返回是否发生了改动（给 --incremental 判断幂等用）。
    static bool PatchBanGame()
    {
        var m = FindM(FindType("NetWorkPlayerMgr"), "BanGame", 0);
        var il = m.Body.Instructions;

        if (il[0].OpCode.Code == Code.Ldstr && (string)il[0].Operand == "block_report")
        {
            Console.WriteLine("[patch] ban -> BanGame 已是门控形态（幂等跳过）");
            return false;
        }
        if (il[0].OpCode.Code == Code.Ldstr)
            throw new Exception("BanGame 入口既不是 δ ret 也不是旧清 ban，基线变了？");

        if (il[0].OpCode == OpCodes.Ldsfld && il.Count > 3 && il[3].OpCode == OpCodes.Ret)
        {
            for (int k = 0; k < 4; k++) il.RemoveAt(0);
            Console.WriteLine("[patch] ban -> 拆除旧的无条件清 ban（4 条）");
        }
        if (il[0].OpCode == OpCodes.Ret)
        {
            il.RemoveAt(0);
            Console.WriteLine("[patch] ban -> 解除 BanGame 的 δ 无条件 no-op");
        }

        var fI = FindType("GameData").Fields.Single(x => x.Name == "I");
        var fB = FindType("GameData").Fields.Single(x => x.Name == "ban");
        InsertAtStart(m, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "block_report"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, il[0]),
            Instruction.Create(OpCodes.Ldsfld, fI),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stfld, fB),
            Instruction.Create(OpCodes.Ret),
        });
        Console.WriteLine("[patch] ban -> BanGame 清 GameData.ban=false（挂在 block_report 门下）");
        return true;
    }

    static void VerifyAntiBan(string path)
    {
        var asm = AssemblyDefinition.ReadAssembly(path);
        mod = asm.MainModule;
        try
        {
            ResolveStarryOnly();
            foreach (var spec in kAntiBanEntryRets)
            {
                var m = FindM(FindType(spec.Type), spec.Method);
                if (m.Body.Instructions[0].OpCode == OpCodes.Ret)
                    throw new Exception($"[verify] {spec.Type}::{spec.Method} 仍是入口 ret 的无条件 no-op");
                RequireBlockReportGate(m, $"{spec.Type}::{spec.Method}");
            }
            var bgl = FindM(FindType("NetWorkPlayerMgr"), "BanGame", 0).Body.Instructions;
            if (!(bgl[0].OpCode.Code == Code.Ldstr && (string)bgl[0].Operand == "block_report" && bgl[1].OpCode.Code == Code.Call))
                throw new Exception("[verify] BanGame 入口不是门控形态");
            var sr = FindType("StarryConfig").Methods.Single(x => x.Name == "StrictBlockedRpc" && x.Parameters.Count == 1);
            if (!(sr.Body.Instructions[0].OpCode.Code == Code.Ldstr && (string)sr.Body.Instructions[0].Operand == "block_report"))
                throw new Exception("[verify] StrictBlockedRpc 未带 block_report 门");

            foreach (var vt in new[] { ("NetWorkPlayerMgr", "BlockVersion", 0), ("NetWorkPlayerMgr", "Vch", 0), ("LowVersionMgr", "NeedUpdate", 0), ("LowVersionMgr", "OpenUrl", 1) })
                if (FindM(FindType(vt.Item1), vt.Item2, vt.Item3).Body.Instructions[0].OpCode != OpCodes.Ret)
                    throw new Exception($"[verify] {vt.Item1}::{vt.Item2} 版本拦截不是常开（入口 ret 丢了）");
            foreach (var mm in new[] { "CheckCodeLocked", "OnSaveGuardFailed" })
            {
                var sm = FindM(FindType("NetWorkPlayerMgr"), mm, 1);
                if (sm.Body.Instructions.Any(x => x.OpCode.Code == Code.Ldstr && (string)x.Operand == "block_report"))
                    throw new Exception($"[verify] NetWorkPlayerMgr::{mm} 又出现 block_report 门（救档链会被防封开关关掉）");
            }

            int sites = 0;
            foreach (var t in mod.GetTypes())
                foreach (var m in t.Methods)
                {
                    if (!m.HasBody) continue;
                    var q = m.Body.Instructions;
                    for (int i = 0; i + 1 < q.Count; i++)
                        if (q[i].OpCode.Code == Code.Ldstr && (string)q[i].Operand == "block_report") sites++;
                }
            Console.WriteLine($"[verify] anti-ban 门（block_report，缺键=false=关）@ {sites} 处；封号拦截 5 处已可切换；版本拦截族/救档链恒常开");
        }
        finally { asm.Dispose(); }
    }


    // v7.18 教训（12.x 丢档事故根因之一）：v7.11 基线把两条「救命链」挂在 block_report 门下。
    // block_report 默认 = 1 ⇒ 整段被跳过：
    //   ① NetWorkPlayerMgr.CheckCodeLocked：服务器验证码不通过时【唯一的存档回滚链】不再执行
    //      （不会回滚到 last save）⇒ 校验失败后新档原地覆盖主档；
    //   ② NetWorkPlayerMgr.OnSaveGuardFailed：存档安全校验失败的【冻结安全气囊】失效。
    // vc1637 必须恢复为无条件执行，与 12.x v7.19 一致。
    static void PatchSaveGuardChain()
    {
        var locked = FindM(FindType("NetWorkPlayerMgr"), "CheckCodeLocked", 1);
        RemoveBlockReportGate(locked, "NetWorkPlayerMgr.CheckCodeLocked");
        var guard = FindM(FindType("NetWorkPlayerMgr"), "OnSaveGuardFailed", 1);
        RemoveBlockReportGate(guard, "NetWorkPlayerMgr.OnSaveGuardFailed");
        Console.WriteLine("[patch] save-guard -> 恢复存档回滚链 / 安全气囊（删 block_report 门）");
    }

    static void RemoveBlockReportGate(MethodDefinition m, string tag)
    {
        var il = m.Body.Instructions;
        for (int i = 0; i + 2 < il.Count; i++)
        {
            if (il[i].OpCode != OpCodes.Ldstr || (string)il[i].Operand != "block_report") continue;
            if (!(il[i + 1].OpCode == OpCodes.Call && il[i + 1].Operand is MethodReference mr && mr.Name == "GetBool")) continue;
            var br = il[i + 2];
            if (br.OpCode != OpCodes.Brfalse && br.OpCode != OpCodes.Brfalse_S &&
                br.OpCode != OpCodes.Brtrue && br.OpCode != OpCodes.Brtrue_S)
                continue;

            // 形式 A（早退门）：ldstr; call GetBool; brfalse body; ret; body:
            // 形式 B（包裹门）：ldstr; call GetBool; brtrue end; body...; end:
            int n = (i + 3 < il.Count && il[i + 3].OpCode == OpCodes.Ret) ? 4 : 3;
            for (int k = 0; k < n; k++) il.RemoveAt(i);
            Console.WriteLine($"[patch] save-guard -> {tag} 删除 block_report 门（{n} 条）");
            return;
        }
        throw new Exception($"{tag} 未找到 block_report 门（基线变了？）");
    }

    // hf2/hf3 变体：这两项在 12.x v7.19 基线里已经做过，本工具必须确认它们确实存在，
    // 否则会出现「以为已恢复、实际仍被 block_report 门挡住」的静默丢档风险。
    static void RequireAlreadyPresent(string type, string method, int? pc, string key)
    {
        var m = FindM(FindType(type), method, pc);
        bool found = m.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == key);
        if (!found)
            throw new Exception($"{type}::{method} 未找到既有 {key} 门（基线不是 v7.19？）");
        Console.WriteLine($"[variant] 基线已含 {type}::{method} -> {key}");
    }

    static void RequireBlockReportGateAbsent(string type, string method, int? pc)
    {
        var m = FindM(FindType(type), method, pc);
        bool hasGate = false;
        var il = m.Body.Instructions;
        for (int i = 0; i + 2 < il.Count; i++)
        {
            if (il[i].OpCode != OpCodes.Ldstr || (string)il[i].Operand != "block_report") continue;
            if (il[i + 1].OpCode == OpCodes.Call && il[i + 1].Operand is MethodReference mr && mr.Name == "GetBool")
                hasGate = true;
        }
        if (hasGate)
            throw new Exception($"{type}::{method} 仍带 block_report 门（基线不是 v7.19？）");
        Console.WriteLine($"[variant] 基线已恢复 {type}::{method}（无 block_report 门）");
    }

    static void InsertConfigGate(MethodDefinition m, string key, List<Instruction> body)
    {
        var first = m.Body.Instructions[0];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, key),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
        };
        seq.AddRange(body);
        InsertAtStart(m, seq);
    }

    static void InsertAtStart(MethodDefinition m, List<Instruction> seq)
    {
        for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    }

    static void NoOpAlways(MethodDefinition m, string tag)
    {
        if (m.ReturnType.FullName != "System.Void")
            throw new Exception($"{tag} 不是 void，不能 no-op");
        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ret));
        Console.WriteLine($"[patch] version pin -> {tag} no-op");
    }

    static void ReturnLongMax(MethodDefinition m)
    {
        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ret));
        m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I8, long.MaxValue));
        Console.WriteLine("[patch] version pin -> ResVersionApi.GetVersion() = long.MaxValue");
    }

    static void ReplaceApplicationVersion(MethodDefinition m)
    {
        int n = 0;
        foreach (var ins in m.Body.Instructions)
        {
            if ((ins.OpCode.Code == Code.Call || ins.OpCode.Code == Code.Callvirt)
                && ins.Operand is MethodReference mr
                && mr.Name == "get_version"
                && mr.DeclaringType.FullName == "UnityEngine.Application")
            {
                ins.OpCode = OpCodes.Ldstr;
                ins.Operand = "999999";
                n++;
            }
        }
        if (n == 0) throw new Exception($"{m.FullName} 未找到 Application.get_version");
        Console.WriteLine($"[patch] version pin -> {m.DeclaringType.Name}.{m.Name} Application.version -> 999999 ({n})");
    }

    static TypeDefinition FindType(string name)
    {
        var direct = mod.GetType(name);
        if (direct != null) return direct;

        var simple = name.Contains('/') ? name[(name.LastIndexOf('/') + 1)..] : name;
        var matches = AllTypes(mod.Types).Where(x => x.Name == simple).ToList();
        if (matches.Count == 1) return matches[0];
        if (matches.Count == 0) throw new Exception($"类型不存在: {name}");
        throw new Exception($"类型名不唯一: {name} -> {matches.Count}");
    }

    static MethodDefinition FindM(TypeDefinition t, string name, int? paramCount = null)
    {
        var list = t.Methods.Where(x => x.Name == name &&
            (paramCount == null || x.Parameters.Count == paramCount)).ToList();
        if (list.Count != 1) throw new Exception($"{t.FullName}::{name} 命中 {list.Count} 个重载");
        if (!list[0].HasBody) throw new Exception($"{t.FullName}::{name} 无方法体");
        return list[0];
    }

    static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (var t in types)
        {
            yield return t;
            foreach (var n in AllTypes(t.NestedTypes)) yield return n;
        }
    }

    static void Verify(string path)
    {
        var asm = AssemblyDefinition.ReadAssembly(path);
        var m = asm.MainModule;

        bool FirstRet(string type, string method, int? pc = null)
        {
            var mm = FindM(FindTypeIn(m, type), method, pc);
            return mm.Body.Instructions.Count > 0 && mm.Body.Instructions[0].OpCode.Code == Code.Ret;
        }

        bool HasString(string type, string method, string s, int? pc = null)
        {
            var mm = FindM(FindTypeIn(m, type), method, pc);
            return mm.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == s);
        }

        bool HasBanStore(string type, string method)
        {
            var mm = FindM(FindTypeIn(m, type), method, 0);
            return mm.Body.Instructions.Any(i => i.OpCode.Code == Code.Stfld
                && i.Operand is FieldReference f && f.Name == "ban");
        }

        bool HasStringPart(string type, string method, string part, int? pc = null)
        {
            var mm = FindM(FindTypeIn(m, type), method, pc);
            return mm.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr
                && i.Operand is string s && s.Contains(part, StringComparison.Ordinal));
        }

        bool HasAppVersion(string type, string method, int? pc = null)
        {
            var mm = FindM(FindTypeIn(m, type), method, pc);
            return mm.Body.Instructions.Any(i => i.Operand is MethodReference mr
                && mr.Name == "get_version"
                && mr.DeclaringType.FullName == "UnityEngine.Application");
        }

        void Check(bool ok, string what)
        {
            if (!ok) throw new Exception($"verify failed: {what}");
            Console.WriteLine($"[verify] {what}");
        }

        Check(FirstRet("NetWorkPlayerMgr", "Vch", 0), "Vch no-op");
        Check(FirstRet("NetWorkPlayerMgr", "BlockVersion", 0), "BlockVersion no-op");
        Check(FirstRet("InfinityCore.NetDataCalculater", "CurrentVersion", 1), "CurrentVersion no-op");
        Check(FirstRet("MainUIFuntions", "ShowNewVersion", 0), "ShowNewVersion no-op");
        Check(FirstRet("MainUIFuntions", "OnNewVersionClick", 0), "OnNewVersionClick no-op");
        Check(FirstRet("LowVersionMgr", "NeedUpdate", 0), "LowVersionMgr.NeedUpdate no-op");
        Check(FirstRet("LowVersionMgr_InitScene", "ShowOnShow", 0), "LowVersionMgr_InitScene.ShowOnShow no-op");
        Check(FirstRet("LowVersionMgr_InitScene", "OnDownLoadClick", 0), "LowVersionMgr_InitScene.OnDownLoadClick no-op");

        Check(HasString("AdsManager/ShowAdsNewFlow", "Show", "shop_ads_unlimited"), "shop_ads_unlimited show gate");
        Check(HasString("ShopWindow", "Refresh", "shop_ads_unlimited", 0), "shop_ads_unlimited refresh gate");
        Check(HasString("DataManager", "AddExp", "exp_multi", 1), "exp_multi gate");
        Check(ExpMultiDefaultIs(kExpMultiDefault), $"exp_multi 缺省 ×{kExpMultiDefault}(默认 1=不放大)");
        Check(HasBanStore("NetWorkPlayerMgr", "BanGame"), "BanGame clears ban");
        Check(!HasString("NetWorkPlayerMgr", "CheckCodeLocked", "block_report", 1),
              "CheckCodeLocked 存档回滚链已恢复（无 block_report 门）");
        Check(HasStringPart("NetWorkPlayerMgr", "CheckCodeLocked", "存档文件验证码不通过", 1),
              "CheckCodeLocked 方法体完整（回滚提示仍在）");
        Check(!HasString("NetWorkPlayerMgr", "OnSaveGuardFailed", "block_report", 1),
              "OnSaveGuardFailed 安全气囊已恢复（无 block_report 门）");
        Check(HasStringPart("NetWorkPlayerMgr", "OnSaveGuardFailed", "存档安全校验不通过", 1),
              "OnSaveGuardFailed 方法体完整（冻结提示仍在）");
        Check(!HasAppVersion("InfinityCore.TransferHandler", "CfgCallBack", 1), "CfgCallBack version compare bypassed");
        Check(!HasAppVersion("NetWorkPlayerMgr", "UpDatePlayerData", 7), "UpDatePlayerData version compare bypassed");
        Check(!HasAppVersion("NetWorkPlayerMgr", "UpDatePlayerMainData", 1), "UpDatePlayerMainData version compare bypassed");

        var getVersion = FindM(FindTypeIn(m, "ResVersionApi"), "GetVersion", 0);
        Check(getVersion.Body.Instructions.Count >= 2
              && getVersion.Body.Instructions[0].OpCode.Code == Code.Ldc_I8
              && getVersion.Body.Instructions[1].OpCode.Code == Code.Ret,
              "ResVersionApi.GetVersion returns long.MaxValue");

        asm.Dispose();
    }

    static TypeDefinition FindTypeIn(ModuleDefinition module, string name)
    {
        var direct = module.GetType(name);
        if (direct != null) return direct;
        var simple = name.Contains('/') ? name[(name.LastIndexOf('/') + 1)..] : name;
        var matches = AllTypes(module.Types).Where(x => x.Name == simple).ToList();
        if (matches.Count == 1) return matches[0];
        if (matches.Count == 0) throw new Exception($"verify: 类型不存在 {name}");
        throw new Exception($"verify: 类型不唯一 {name}");
    }
}
