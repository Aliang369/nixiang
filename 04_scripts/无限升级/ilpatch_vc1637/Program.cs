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
        bool incremental = false;
        bool noReverse = false;  // 复现性自检用：跳过 item_reverse
        var rest = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--variant" && i + 1 < args.Length) variantName = args[++i];
            else if (args[i] == "--scan") scanOnly = true;
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
        if (scanOnly)
        {
            var sc = AssemblyDefinition.ReadAssembly(src);
            mod = sc.MainModule;
            Console.WriteLine($"[scan] {src} ({srcLen} B)");
            ScanMinusItemCalls();
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
            int newSites = PatchItemUseReverse(allowExisting: true);
            int negFix = PatchMinusItemNegativeBypass(allowExisting: true);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            if (newSites == 0 && negFix == 0)
            {
                // 幂等：产物已经带门，原样复制（不重写，避免字节漂移）。
                asmI.Dispose();
                File.Copy(src, dst, overwrite: true);
                Console.WriteLine("[done] item_reverse 已存在，原样复制（幂等跳过）");
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
            VerifyMinusItemNegativeBypass(dst);
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
        if (!noReverse) PatchItemUseReverse(allowExisting: false);
        if (!noReverse) PatchMinusItemNegativeBypass(allowExisting: false);
        PatchShopAdsUnlimited();
        PatchVersionPin();
        PatchBanGame();
        if (spec.FullPatchSet)
        {
            PatchExpMulti();
            PatchSaveGuardChain();
        }
        else
        {
            // hf2/hf3 基线（12.x v7.19）已含这两项，重复打会二次放大 / 抛错。
            RequireAlreadyPresent("DataManager", "AddExp", 1, "exp_multi");
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
        if (!noReverse) VerifyMinusItemNegativeBypass(dst);
        Verify(dst);
        return 0;
    }

    static void ResolveStarryOnly()
    {
        tStarryConfig = FindType("StarryConfig");
        mGetBool = tStarryConfig.Methods.Single(x => x.Name == "GetBool" && x.Parameters.Count == 1);
    }

    // =====================================================================
    // 物品使用反增（item_reverse）：使用物品时不是扣掉，而是 ×1000 加上去。
    //
    // 只打「使用物品」链路（ItemDetialWindow 的 OnUse/OnUseAll/OnUseMulti 及其里
    // 的编译器闭包方法、SelectLegacyPetItemUse.Use、SelectMiFaItemUse.Use）。
    // 绝不能打 GameData::MinusItem 本体：DLL 里存在
    //   while (GameData.I.Enough("纯净之灵", upgradeChunJingZhiLing)) { GameData.I.MinusItem("纯净之灵", ...); ... }
    // 这类「消耗驱动循环」，整体反转会永不退出 ⇒ 真机卡死。
    //
    // 注入位置 = 每个 callvirt MinusItem(string,int64) 之前，栈上此刻正好是 (id, amount)：
    //   ldstr "item_reverse"        ┐
    //   call  StarryConfig.GetBool  │ 门关着就原样走原逻辑
    //   brfalse -> callvirt         ┘
    //   ldc.i8 1000 / mul / neg     ⇒ MinusItem(id, -1000*amount) ⇒ 数量 +1000*amount
    // =====================================================================
    static readonly string[] kItemUseTypeNames = { "ItemDetialWindow", "SelectLegacyPetItemUse", "SelectMiFaItemUse" };
    const long kItemReverseMul = 1000L;

    static bool InUseFamily(TypeDefinition t)
    {
        for (var cur = t; cur != null; cur = cur.DeclaringType)
            if (Array.IndexOf(kItemUseTypeNames, cur.Name) >= 0) return true;
        return false;
    }

    static IEnumerable<MethodDefinition> UseFamilyMethods()
    {
        foreach (var t in AllTypes(mod.Types))
        {
            if (!InUseFamily(t)) continue;
            foreach (var m in t.Methods) yield return m;
        }
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

    static int PatchItemUseReverse(bool allowExisting)
    {
        int sites = 0;
        int existing = 0;
        foreach (var m in UseFamilyMethods())
        {
            if (!m.HasBody) continue;
            foreach (var ins in m.Body.Instructions.ToArray())
            {
                if (ins.OpCode.Code != Code.Call && ins.OpCode.Code != Code.Callvirt) continue;
                if (ins.Operand is not MethodReference mr || mr.Name != "MinusItem") continue;
                if (mr.DeclaringType.Name != "GameData") continue;
                if (mr.Parameters.Count != 2 || mr.Parameters[1].ParameterType.FullName != "System.Int64")
                    throw new Exception($"{m.FullName} 的 MinusItem 重载不是 (string,int64)：{mr.FullName}");

                if (AlreadyGated(ins))
                {
                    if (!allowExisting)
                        throw new Exception($"{m.FullName} @IL_{ins.Offset:X4} 已带 item_reverse 门（基线不该是已打过补丁的 DLL）");
                    existing++;
                    continue;
                }

                var proc = m.Body.GetILProcessor();
                proc.InsertBefore(ins, Instruction.Create(OpCodes.Ldstr, "item_reverse"));
                proc.InsertBefore(ins, Instruction.Create(OpCodes.Call, mGetBool));
                proc.InsertBefore(ins, Instruction.Create(OpCodes.Brfalse, ins));
                proc.InsertBefore(ins, Instruction.Create(OpCodes.Ldc_I8, kItemReverseMul));
                proc.InsertBefore(ins, Instruction.Create(OpCodes.Mul));
                proc.InsertBefore(ins, Instruction.Create(OpCodes.Neg));
                sites++;
                Console.WriteLine($"[patch] item_reverse -> {m.DeclaringType.FullName}::{m.Name} @IL_{ins.Offset:X4}");
            }
        }
        if (sites + existing == 0) throw new Exception("未找到任何使用路径的 MinusItem 调用点（基线变了？）");
        Console.WriteLine($"[patch] item_reverse -> 使用反增 ×{kItemReverseMul}，新注入 {sites} 处 / 已存在 {existing} 处");
        return sites;
    }

    static bool AlreadyGated(Instruction call)
    {
        for (var p = call.Previous; p != null; p = p.Previous)
        {
            if (p.OpCode.Code == Code.Ldstr && (string)p.Operand == "item_reverse") return true;
            if (p.OpCode.Code == Code.Ret || p.OpCode.Code == Code.Br || p.OpCode.Code == Code.Br_S) break;
        }
        return false;
    }

    static void VerifyItemReverse(string path)
    {
        var asm = AssemblyDefinition.ReadAssembly(path);
        mod = asm.MainModule;
        int sites = 0, mul = 0;
        foreach (var m in UseFamilyMethods())
        {
            if (!m.HasBody) continue;
            var il = m.Body.Instructions;
            for (int i = 0; i < il.Count; i++)
            {
                if (il[i].OpCode.Code != Code.Call && il[i].OpCode.Code != Code.Callvirt) continue;
                if (il[i].Operand is not MethodReference mr || mr.Name != "MinusItem" || mr.DeclaringType.Name != "GameData") continue;
                if (i < 6) throw new Exception($"verify: {m.FullName} @IL_{il[i].Offset:X4} 前导指令不足 6 条");
                if (!(il[i - 6].OpCode.Code == Code.Ldstr && (string)il[i - 6].Operand == "item_reverse")) continue;
                if (!(il[i - 5].OpCode.Code == Code.Call && ((MethodReference)il[i - 5].Operand).Name == "GetBool")) continue;
                if (il[i - 4].OpCode.Code != Code.Brfalse) continue;
                if (il[i - 3].OpCode.Code != Code.Ldc_I8 || (long)il[i - 3].Operand != kItemReverseMul) continue;
                if (il[i - 2].OpCode.Code != Code.Mul) continue;
                if (il[i - 1].OpCode.Code != Code.Neg) continue;
                sites++;
                mul += 1;
            }
        }
        if (sites == 0) throw new Exception("verify failed: item_reverse 无有效注入点");
        Console.WriteLine($"[verify] item_reverse gate ×{kItemReverseMul} @ {sites} 处使用路径");
        asm.Dispose();
    }

    // ------------------------------------------------------------------
    // fix D：item_keep（物品不减）与 skin_unlock（皮肤全解锁）会让 GameData::MinusItem
    // 在方法入口直接 ret（整个方法变成 no-op），于是 item_reverse 注入的「负额扣减」
    // 什么也不会发生（真机实测 item_keep=1 ⇒ 使用物品毫无变化）。
    //
    // 这里只给「负额」开后门：amount < 0 时跳过 skin_unlock / item_keep 两道门，
    // 直接落到原本的 ContainsKey 检查 + 扣减（减去负数 = 数量增加）。
    //   · 正向扣减完全不受影响 ⇒ 消耗驱动的 while 循环（如「纯净之灵」）行为不变，不会卡死。
    //   · ContainsKey 检查保留 ⇒ 不在 _allItemCode 里的特殊物品不会被写坏（只是不生效）。
    //   · 只有 item_reverse 注入的那 7 处使用路径才会传负额，其余调用点绝不触发。
    // ------------------------------------------------------------------
    static int PatchMinusItemNegativeBypass(bool allowExisting)
    {
        var t = FindType("GameData");
        var m = t.Methods.SingleOrDefault(x => x.Name == "MinusItem"
                && x.Parameters.Count == 2
                && x.Parameters[1].ParameterType.FullName == "System.Int64"
                && x.HasBody)
            ?? throw new Exception("GameData::MinusItem(string,int64) 未找到（基线变了？）");
        var il = m.Body.Instructions;

        // 幂等：方法开头已是 ldarg.2 / ldc.i8 0 / clt 即为已打过。
        bool patched = il.Count >= 3
            && il[0].OpCode.Code == Code.Ldarg_2
            && il[1].OpCode.Code == Code.Ldc_I8 && (long)il[1].Operand == 0L
            && il[2].OpCode.Code == Code.Clt;
        if (patched)
        {
            if (!allowExisting)
                throw new Exception($"{m.FullName} 已带负额绕过（基线不该是已打过补丁的 DLL）");
            Console.WriteLine("[patch] minus_item_neg_bypass 已存在，跳过");
            return 0;
        }

        // 守卫链出口 = 最后一条 `call GetBool` 之后那个条件跳转的落点（即守卫链之后的第一条指令）。
        Instruction exit = null;
        for (int i = 0; i + 1 < il.Count; i++)
        {
            if (il[i].OpCode.Code != Code.Call) continue;
            if (il[i].Operand is not MethodReference gb || gb.Name != "GetBool") continue;
            var nx = il[i + 1];
            if (nx.OpCode.Code != Code.Brfalse && nx.OpCode.Code != Code.Brfalse_S) continue;
            if (nx.Operand is Instruction tgt) exit = tgt;
        }
        if (exit == null)
            throw new Exception("未找到 GameData::MinusItem 的 skin_unlock/item_keep 守卫链（基线变了？）");

        var first = il[0];
        var proc = m.Body.GetILProcessor();
        proc.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_2));
        proc.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I8, 0L));
        proc.InsertBefore(first, Instruction.Create(OpCodes.Clt));
        proc.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, exit));
        m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 4);
        Console.WriteLine($"[patch] minus_item_neg_bypass -> {m.FullName} 负额绕过 item_keep/skin_unlock，落点 IL_{exit.Offset:X4}");
        return 1;
    }

    static void VerifyMinusItemNegativeBypass(string path)
    {
        var asm = AssemblyDefinition.ReadAssembly(path);
        mod = asm.MainModule;
        var t = FindType("GameData");
        var m = t.Methods.SingleOrDefault(x => x.Name == "MinusItem"
                && x.Parameters.Count == 2
                && x.Parameters[1].ParameterType.FullName == "System.Int64"
                && x.HasBody)
            ?? throw new Exception("verify: GameData::MinusItem(string,int64) 未找到");
        var il = m.Body.Instructions;
        if (il.Count < 4
            || il[0].OpCode.Code != Code.Ldarg_2
            || il[1].OpCode.Code != Code.Ldc_I8 || (long)il[1].Operand != 0L
            || il[2].OpCode.Code != Code.Clt
            || il[3].OpCode.Code != Code.Brtrue)
            throw new Exception("verify failed: minus_item_neg_bypass 未生效");
        Console.WriteLine($"[verify] minus_item_neg_bypass @ {m.FullName}（负额跳过 item_keep/skin_unlock）");
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

    static void PatchExpMulti()
    {
        var m = FindM(FindType("DataManager"), "AddExp", 1);
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
            Instruction.Create(OpCodes.Ldc_I4_1),
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
        Console.WriteLine("[patch] exp_multi -> DataManager.AddExp(LargeNumber) 入口 ×N");
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

    static void PatchBanGame()
    {
        var m = FindM(FindType("NetWorkPlayerMgr"), "BanGame", 0);
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldsfld, fGameDataI),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stfld, fBan),
            Instruction.Create(OpCodes.Ret),
        };
        InsertAtStart(m, seq);
        Console.WriteLine("[patch] ban -> BanGame 清 GameData.ban=false");
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
