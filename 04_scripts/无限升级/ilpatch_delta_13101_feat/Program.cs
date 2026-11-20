using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ============================================================================
// ilpatch_delta_13101_feat —— 无限升级 13.101（TapTap 原版 / wxsjdz1 21 槽）第 2 段增量补丁器
//
// 输入 = 13.x 已交付 allfix（v1 31 条 + delta 防封层 merged）:
//          07_patched/无限升级/13101_new/targetfuntional_13101new_allfix.dll
//          2,760,704 B（对官方 2,748,416 B）
// 输出 = 追加 8 项功能后的 19 项载荷 ..._allfix_19feat.dll（不覆盖 allfix）
//
// 本步只补 13.x 缺的 8 项（方案唯一来源 13_scratch/lead/feature_gap_analysis.md §2/§3.1）：
//   feat13 shop_ads_unlimited      AdsManager.ShowAdsNewFlow.Show + ShopWindow.Refresh 入口清零 DailyData 每日广告计数
//                                  （取自 ilpatch_vc1637:975-1027，改用断网版容错版 ZeroDailyAdCounters dz1_feat:897-933）
//   feat14 item_reverse            GameData::MinusItem 全部调用点取负（排除存档链 + 库存驱动循环）
//   feat15 item_reverse_x          MinusItem 本体认负额：负额跳过 item_keep/skin_unlock 并乘 item_reverse_x（缺省 1000）
//                                  （14/15 取自 ilpatch_vc1637:311-520 + :767-935）
//   feat16 zhuansheng_free         LunHuiWindow::WindowOpen 两处 TotalHours>=1 比较点常量换 -1.0
//   feat17 skill_projectile_x      SkillAssets::GetBulletAmount 末尾乘 N
//   feat18 skill_damage_x          SkillAssets::get_damage / GetSafeDamage 各 2 处 ret 前乘 N（LargeNumber::op_Multiply）
//   feat19 zhuan_sheng_free_point  LegacyCell 内每处 Calculater.GetLegacyCost* 串 StarryCostZero 归零
//   feat20 chenghao_lv             PlayerGo::ChangeClothes 写 hounerLevel / RefreshMainSkills 强制显示 / FixedUpdate 变化重刷
//                                  （17/18/19 取自 ilpatch_vc1637_feat3:100-270；20 取自 无限升级_断网版/ilpatch_ch_dz1 v5）
//
// 基线差异（相对 11 线 / 断网版）：
//   1) 配置宿主不是 Starry.StarryConfig，而是官方已存在的 $Obfuz$ConstFieldHolder$0（ilpatch BuildStarryConfig 口径）。
//   2) feat19 的 helper 不新增 Starry.StarryLegacy 类型，改为挂在同一宿主类型上（见 EnsureHelper 注释）。
//   3) kExpectLegacySites 由 13 改为实测值（13.x LegacyCell 布局不同）。
//
// 写盘前纪律（README:22-31 铁律）：
//   1) 每个被改方法 SimplifyMacros() + OptimizeMacros()；
//   2) BranchRelax.WidenOutOfRange 多轮循环（照 ilpatch_vc1637:227-240 / dz1_feat:143-152）；
//   3) 落点数写死自校验，不符即 throw 不写盘（照 feat3:340-360 / dz1_feat:170-198）。
//
// 用法：
//   WXSJ_SRC=<allfix.dll> WXSJ_DST=<out.dll> dotnet run -c Release --no-build
//   dotnet run -c Release --no-build -- --scan           只盘点落点，不写盘
//   dotnet run -c Release --no-build -- --verify <dll>   只读校验既有产物
// ============================================================================
internal static class Program
{
    const string kInDefault = "/Users/aliang/Documents/nixiang/07_patched/无限升级/13101_new/targetfuntional_13101new_allfix.dll";
    const string kOutDefault = "/Users/aliang/Documents/nixiang/07_patched/无限升级/13101_new/targetfuntional_13101new_allfix_19feat.dll";

    // 输入基线（allfix）实测长度：不符即拒绝（防止误把官方 / v1 当输入）
    const long kExpectSrcLen = 2760704;
    // 载荷槽余量参考：官方现网数组 2,748,416 B
    const long kOfficialArrayLen = 2748416;

    // ---------------- 写死落点数（自校验，不符 throw 不写盘） ----------------
    const int kExpectShopGates = 2;          // Show 1 + ShopWindow.Refresh 1
    const int kExpectShopFields = 10;        // 清零的 DailyData 计数字段数
    const int kExpectZsSites = 2;            // WindowOpen 两处 TotalHours>=1
    const int kExpectProjectileSites = 1;    // GetBulletAmount 1 处
    const int kExpectDamageRets = 4;         // get_damage 2 + GetSafeDamage 2
    // 实测（2026-11-20 --scan on allfix）：LegacyCell 13 处（PiLiangFuntion 2 / Show 4 / Upgrade 4 / Upgrade10 1 / Upgrade100 1 / ReturnOver5000 1）。
    // 与 analyst 报的 9、scout 报的 6 都不同：这两个数按 11 线的方法分布猜的，13.x 的 Show/Upgrade 各是 4 处而不是 2 处。
    const int kExpectLegacySites = 13;
    const int kExpectChenghaoSites = 3;      // A ChangeClothes 1 + B RefreshMainSkills 1 + C FixedUpdate 1
    // 实测（2026-11-20 --scan on allfix）：144 个 MinusItem 调用点，排除 8 处（存档链 2 + 库存驱动循环 6）⇒ 可打 136。
    const int kExpectReverseSites = 136;
    const int kExpectMinusItemBodyOps = 2;   // 负额序言 1 + 乘 item_reverse_x 取数块 1

    const int kItemReverseDefaultX = 1000;
    const int kItemReverseMinX = 1;
    const int kItemReverseMaxX = 1000000;
    const int kGateInstrCount = 8;

    static ModuleDefinition mod;
    static TypeDefinition tCfg;
    static MethodReference mGetBool;
    static MethodReference mGetInt;
    static TypeDefinition tGameData;
    static FieldReference fGameDataI;
    static FieldReference fDailyData;
    static FieldReference fHounerLevel;
    static TypeDefinition tDailyData;
    static TypeReference tLargeNumber;
    static MethodReference mOpMul;
    static readonly HashSet<MethodDefinition> touched = new HashSet<MethodDefinition>();

    static int Main(string[] args)
    {
        string src = Env("WXSJ_SRC") ?? kInDefault;
        string dst = Env("WXSJ_DST") ?? kOutDefault;
        bool scanOnly = false, verifyOnly = false;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--src": src = args[++i]; break;
                case "--dst": dst = args[++i]; break;
                case "--scan": scanOnly = true; break;
                case "--verify": verifyOnly = true; break;
                default: Console.WriteLine("[warn] 未知参数 " + args[i]); break;
            }
        }
        if (!File.Exists(src)) { Console.WriteLine("[ERR] 输入不存在: " + src); return 2; }
        long srcLen = new FileInfo(src).Length;
        if (!scanOnly && !verifyOnly && Path.GetFullPath(src) == Path.GetFullPath(dst))
            throw new ArgumentException("禁止原地覆写（src == dst）");

        var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { ReadSymbols = false });
        mod = asm.MainModule;
        Console.WriteLine("[load] " + src + "  (" + srcLen + " B, types=" + mod.Types.Count + ")");
        ResolveRefs();

        if (scanOnly) { Scan(); asm.Dispose(); return 0; }
        if (verifyOnly) { bool vok = Verify(false); asm.Dispose(); Console.WriteLine(vok ? "[verify] PASS" : "[verify] FAIL"); return vok ? 0 : 1; }

        if (srcLen != kExpectSrcLen)
            throw new InvalidDataException("输入 " + srcLen + " B != allfix 基线 " + kExpectSrcLen + " B（应为 13.x allfix 产物）");

        // ---------------- 依次打 8 项 ----------------
        int shopGates = PatchShopAdsUnlimited();
        var rev = PatchItemReverse();
        int bodyOps = PatchMinusItemBody();
        bool zs = PatchZhuanShengFree();
        int proj = PatchSkillProjectile();
        int dmg = PatchSkillDamage();
        int legacy = PatchLegacyPointFree();
        int ch = PatchChenghaoLv();

        Console.WriteLine("[patch] shop_ads=" + shopGates + " / item_reverse 新增=" + rev.patched + " 已存在=" + rev.existing
            + " 排除=" + rev.skipped + " 共=" + rev.total + " / minusBody=" + bodyOps + " / zhuansheng=" + (zs ? 1 : 0)
            + " / projectile=" + proj + " / damageRets=" + dmg + " / legacy=" + legacy + " / chenghao=" + ch);

        // ---------------- 落点数自校验（不符 throw，不写盘） ----------------
        if (!Verify(false)) throw new Exception("[ERR] 落点数自校验失败，不写盘");

        // ---------------- 写盘前：宏简化 + 短跳放宽 ----------------
        int opt = 0;
        foreach (var m in touched)
        {
            if (m == null || !m.HasBody) continue;
            m.Body.SimplifyMacros();
            m.Body.OptimizeMacros();
            opt++;
        }
        Console.WriteLine("[finalize] " + opt + " 个被改方法已 SimplifyMacros+OptimizeMacros");

        int relaxed = 0;
        for (int pass = 0; pass < 8; pass++)
        {
            using (var ms = new MemoryStream()) asm.Write(ms);   // 干写一次，让 Cecil 算出真实 Offset
            int n = BranchRelax.WidenOutOfRange(mod);
            if (n == 0) break;
            relaxed += n;
        }
        if (relaxed > 0) Console.WriteLine("[relax] 短跳转越界升级为长跳转：" + relaxed + " 处");

        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        asm.Write(dst);
        asm.Dispose();

        long outLen = new FileInfo(dst).Length;
        Console.WriteLine("[done] " + dst + "  " + outLen + " B  (official=" + kOfficialArrayLen + ", margin=" + (kOfficialArrayLen - outLen) + ")");

        // ---------------- 落盘后复读复验 ----------------
        var re = AssemblyDefinition.ReadAssembly(dst, new ReaderParameters { ReadSymbols = false });
        mod = re.MainModule;
        ResolveRefs();
        bool ok2 = Verify(false);
        re.Dispose();
        if (!ok2) { Console.WriteLine("[ERR] 落盘后复读校验失败"); return 6; }
        Console.WriteLine("[verify] PASS（落盘后复读）");
        return 0;
    }

    static string Env(string k) => Environment.GetEnvironmentVariable(k);

    // ===================================================================
    // 引用解析
    // ===================================================================
    static void ResolveRefs()
    {
        // 13.x 的配置宿主：ilpatch 的 BuildStarryConfig 把 helper 挂在官方已存在的
        // $Obfuz$ConstFieldHolder$0 上（MethodDef 尾），不是 Starry.StarryConfig。
        tCfg = mod.GetType("Starry.StarryConfig") ?? mod.GetType("$Obfuz$ConstFieldHolder$0");
        if (tCfg == null)
            tCfg = AllTypes().FirstOrDefault(t => t.Methods.Any(mm => mm.Name == "GetBool" && mm.Parameters.Count == 1)
                                               && t.Methods.Any(mm => mm.Name == "GetInt" && mm.Parameters.Count == 2));
        if (tCfg == null) throw new Exception("找不到配置宿主（GetBool/GetInt）—— 基线不是 ilpatch 产物？");
        mGetBool = tCfg.Methods.First(mm => mm.Name == "GetBool" && mm.Parameters.Count == 1);
        mGetInt = tCfg.Methods.First(mm => mm.Name == "GetInt" && mm.Parameters.Count == 2);

        tGameData = FindType("GameData");
        fGameDataI = tGameData.Fields.First(f => f.Name == "I" && f.IsStatic);
        fDailyData = tGameData.Fields.First(f => f.Name == "dailyData");
        fHounerLevel = tGameData.Fields.First(f => f.Name == "hounerLevel");
        tDailyData = fDailyData.FieldType.Resolve() ?? throw new Exception("GameData.dailyData 类型无法解析");

        var gd = FindType("SkillAssets").Methods.FirstOrDefault(mm => mm.Name == "get_damage");
        if (gd == null || !gd.HasBody) throw new Exception("SkillAssets::get_damage 未找到");
        tLargeNumber = gd.ReturnType;
        mOpMul = null;
        foreach (var ins in gd.Body.Instructions)
            if (ins.Operand is MethodReference mr && mr.Name == "op_Multiply"
                && mr.DeclaringType.Name == "LargeNumber" && mr.Parameters.Count == 2
                && mr.Parameters[1].ParameterType.MetadataType == MetadataType.Double)
            { mOpMul = mr; break; }
        if (mOpMul == null) throw new Exception("get_damage 内未找到 LargeNumber::op_Multiply(LargeNumber,double)");

        Console.WriteLine("[refs] cfg=" + tCfg.FullName + " / GameData.I=" + fGameDataI.Name
            + " / dailyData=" + tDailyData.FullName + " / LargeNumber=" + tLargeNumber.Name);
    }

    // ===================================================================
    // feat13 商店广告无限：Show + ShopWindow.Refresh 入口清零 DailyData 每日广告计数
    //   （vc1637:976-1034 实现 + dz1_feat:897-933 容错：字段存在才清零）
    // ===================================================================
    static int PatchShopAdsUnlimited()
    {
        var show = FindM(FindType("AdsManager/ShowAdsNewFlow"), "Show");
        var refresh = FindM(FindType("ShopWindow"), "Refresh", 0);
        int n = 0;
        if (InsertConfigGateOnce(show, "shop_ads_unlimited", ZeroDailyAdCounters, "AdsManager.ShowAdsNewFlow.Show 清零每日广告计数")) n++;
        if (InsertConfigGateOnce(refresh, "shop_ads_unlimited", ZeroDailyAdCounters, "ShopWindow.Refresh 清零每日广告计数")) n++;
        if (n == 0) Console.WriteLine("[skip] shop_ads_unlimited 两处门均已存在（幂等）");
        return n;
    }

    static bool InsertConfigGateOnce(MethodDefinition m, string key, Func<List<Instruction>> body, string tag)
    {
        if (m.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == key))
        {
            Console.WriteLine("[skip] " + key + " -> " + tag + "（已有门）");
            return false;
        }
        var first = m.Body.Instructions[0];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, key),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
        };
        seq.AddRange(body());
        for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
        m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 4);
        touched.Add(m);
        Console.WriteLine("[patch] " + key + " -> " + tag);
        return true;
    }

    static readonly string[] kAdCounterFields =
    {
        "adsAmount", "miFaLibao", "equipLiBao", "yiWangShui", "baoShiLimit",
        "yuangulimit", "yunXingSuiPian", "mowudan", "aomiYao", "shuangbeiyao",
    };

    static List<Instruction> ZeroDailyAdCounters()
    {
        var fields = kAdCounterFields.Select(n => tDailyData.Fields.FirstOrDefault(x => x.Name == n))
                                     .Where(x => x != null).Select(x => (FieldReference)x).ToArray();
        var absent = kAdCounterFields.Where(n => tDailyData.Fields.All(x => x.Name != n)).ToArray();
        if (absent.Length > 0)
            Console.WriteLine("[warn] DailyData 缺 " + absent.Length + " 个计数域（" + string.Join(",", absent) + "），跳过");
        if (fields.Length == 0) throw new Exception("DailyData 一个广告计数域都没有（基线变了？）");

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

    // ===================================================================
    // feat14 物品使用反增：所有 GameData::MinusItem(string,int64) 调用点把 amount 取负
    //   排除 1) 存档链（FixSave / DataManager 及其嵌套）2) 库存驱动循环（循环体内再查库存）
    // ===================================================================
    static readonly string[] kReverseSkipTypes = { "FixSave", "DataManager" };
    static readonly string[] kStockQueryNames =
    {
        "Enough", "GetAmount", "GetItemAmount", "GetItemNum", "GetNum", "GetCount",
        "GetItemCount", "HaveItem", "IsEnough", "GetItemSum",
    };

    sealed class ReverseSite
    {
        public MethodDefinition Method;
        public Instruction Call;
        public string Skip;
    }

    static List<ReverseSite> CollectReverseSites()
    {
        var list = new List<ReverseSite>();
        foreach (var t in AllTypes())
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
                    if (!(ins.Operand is MethodReference mr) || mr.Name != "MinusItem") continue;
                    if (mr.DeclaringType.Name != "GameData") continue;
                    if (mr.Parameters.Count != 2 || mr.Parameters[1].ParameterType.FullName != "System.Int64")
                        throw new Exception(t.FullName + "::" + m.Name + " 的 MinusItem 重载不是 (string,int64): " + mr.FullName);
                    list.Add(new ReverseSite { Method = m, Call = ins, Skip = SkipReason(t, il, loops, i) });
                }
            }
        }
        return list;
    }

    static string SkipReason(TypeDefinition t, List<Instruction> il, List<int[]> loops, int i)
    {
        for (var cur = t; cur != null; cur = cur.DeclaringType)
            if (Array.IndexOf(kReverseSkipTypes, cur.Name) >= 0) return "存档链(" + cur.Name + ")";
        foreach (var l in loops)
        {
            if (l[0] > i || i > l[1]) continue;
            for (int k = l[0]; k <= l[1]; k++)
            {
                if (il[k].OpCode.Code != Code.Call && il[k].OpCode.Code != Code.Callvirt) continue;
                if (il[k].Operand is MethodReference cr
                    && cr.DeclaringType.Name == "GameData"
                    && Array.IndexOf(kStockQueryNames, cr.Name) >= 0)
                    return "库存驱动循环(" + cr.Name + ")";
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

    // 0 = 没门；1 = 新形态（只把 amount 取负）；2 = 旧形态（调用点自带乘 1000）
    static int GateState(Instruction call)
    {
        var seq = TakeBack(call, kGateInstrCount, includeCall: true);
        if (seq.Count == kGateInstrCount + 1
            && seq[0].OpCode.Code == Code.Ldstr && (string)seq[0].Operand == "item_reverse"
            && seq[1].OpCode.Code == Code.Call && ((MethodReference)seq[1].Operand).Name == "GetBool"
            && IsBrfalse(seq[2]) && ReferenceEquals(seq[2].Operand, call)
            && seq[3].OpCode.Code == Code.Dup
            && seq[4].OpCode.Code == Code.Ldc_I4_0
            && seq[5].OpCode.Code == Code.Conv_I8
            && IsBle(seq[6]) && ReferenceEquals(seq[6].Operand, call)
            && seq[7].OpCode.Code == Code.Neg)
            return 1;
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

    // 落盘后 OptimizeMacros 会把长跳转收成短跳转形式（Code 变 Brfalse_S / Ble_S），判定必须两种都认
    static bool IsBrfalse(Instruction x) => x.OpCode.Code == Code.Brfalse || x.OpCode.Code == Code.Brfalse_S;
    static bool IsBle(Instruction x) => x.OpCode.Code == Code.Ble || x.OpCode.Code == Code.Ble_S;

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

    static (int patched, int existing, int skipped, int total) PatchItemReverse()
    {
        var sites = CollectReverseSites();
        int patched = 0, existing = 0, skipped = 0;
        foreach (var s in sites)
        {
            if (s.Skip != null) { skipped++; continue; }
            int st = GateState(s.Call);
            if (st == 1) { existing++; continue; }
            if (st == 2) throw new Exception(s.Method.FullName + " 是旧形态 item_reverse 门（调用点自带乘 1000）——基线不该是这种形态");
            EmitGate(s.Method, s.Call);
            touched.Add(s.Method);
            patched++;
        }
        return (patched, existing, skipped, sites.Count);
    }

    // ===================================================================
    // feat15 物品反增倍率：MinusItem 本体认负额
    //   负额 => 跳过 item_keep / skin_unlock 守卫链，并在每处 target 取数后乘 item_reverse_x
    // ===================================================================
    static MethodDefinition FindMinusItemBody()
    {
        var m = FindType("GameData").Methods.SingleOrDefault(x => x.Name == "MinusItem"
                && x.Parameters.Count == 2
                && x.Parameters[1].ParameterType.FullName == "System.Int64"
                && x.HasBody);
        if (m == null) throw new Exception("GameData::MinusItem(string,int64) 未找到（基线变了？）");
        return m;
    }

    static bool IsTargetRead(Instruction x)
    {
        if (x.OpCode.Code == Code.Ldarg_2) return true;
        if ((x.OpCode.Code == Code.Ldarg_S || x.OpCode.Code == Code.Ldarg)
            && x.Operand is ParameterDefinition pd && pd.Index == 1) return true;
        return false;
    }

    static bool IsCondBranch(Instruction x)
        => x.OpCode.Code == Code.Brtrue || x.OpCode.Code == Code.Brtrue_S
        || x.OpCode.Code == Code.Brfalse || x.OpCode.Code == Code.Brfalse_S;

    // 开头 4 条 = 负额序言（ldarg.2 / ldc.i8 0 / clt / brtrue|brfalse）
    static int PrologueLen(MethodDefinition m)
    {
        var il = m.Body.Instructions;
        if (il.Count >= 4
            && il[0].OpCode.Code == Code.Ldarg_2
            && il[1].OpCode.Code == Code.Ldc_I8 && (long)il[1].Operand == 0L
            && il[2].OpCode.Code == Code.Clt
            && IsCondBranch(il[3])) return 4;
        return 0;
    }

    static (bool head, bool scale) MinusItemBodyState(MethodDefinition m)
    {
        bool head = PrologueLen(m) == 4;
        bool scale = m.Body.Instructions.Any(x => x.OpCode.Code == Code.Ldstr && (string)x.Operand == "item_reverse_x");
        return (head, scale);
    }

    static List<Instruction> TargetReads(MethodDefinition m)
    {
        var il = m.Body.Instructions.ToList();
        int skip = PrologueLen(m);
        var res = new List<Instruction>();
        for (int i = skip; i < il.Count; i++) if (IsTargetRead(il[i])) res.Add(il[i]);
        return res;
    }

    // 守卫链出口 = 最后一条 call GetBool 之后那个条件跳转的落点
    static Instruction FindGuardExit(ICollection<Instruction> il)
    {
        var a = il.ToList();
        Instruction exit = null;
        for (int i = 0; i + 1 < a.Count; i++)
        {
            if (a[i].OpCode.Code != Code.Call) continue;
            if (!(a[i].Operand is MethodReference gb) || gb.Name != "GetBool") continue;
            if (!IsCondBranch(a[i + 1])) continue;
            if (a[i + 1].Operand is Instruction tgt) exit = tgt;
        }
        return exit;
    }

    static int PatchMinusItemBody()
    {
        var m = FindMinusItemBody();
        var st = MinusItemBodyState(m);
        if (st.head && st.scale) { Console.WriteLine("[skip] minus_item_reverse_body 已存在"); return 0; }
        if (st.head && !st.scale) throw new Exception(m.FullName + " 是旧形态 fix D 序言（负额不乘倍率），不能增量重打");

        int ops = 0;
        // 顺序要紧：先在干净方法体上定位守卫链出口，再插取数块
        if (!st.head)
        {
            var exit = FindGuardExit(m.Body.Instructions);
            if (exit == null) throw new Exception("未找到 MinusItem 的 skin_unlock/item_keep 守卫链（基线变了？）");
            var first = m.Body.Instructions.First();
            var proc = m.Body.GetILProcessor();
            proc.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_2));
            proc.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I8, 0L));
            proc.InsertBefore(first, Instruction.Create(OpCodes.Clt));
            proc.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, exit));
            m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 4);
            Console.WriteLine("[patch] minus_item_neg_bypass -> " + m.FullName + " 负额跳过 item_keep/skin_unlock，落点 IL_" + exit.Offset.ToString("X4"));
            ops++;
        }
        ops += EmitScaleBlocks(m);
        touched.Add(m);
        return ops;
    }

    static int EmitScaleBlocks(MethodDefinition m)
    {
        var reads = TargetReads(m);
        if (reads.Count == 0) throw new Exception("MinusItem 里找不到 target 参数的读取（基线变了？）");
        foreach (var rd in reads)
        {
            var anchor = rd.Next;
            if (anchor == null) throw new Exception("MinusItem: target 读取之后没有指令（基线变了？）");
            var proc = m.Body.GetILProcessor();
            var done = Instruction.Create(OpCodes.Nop);
            var conv = Instruction.Create(OpCodes.Conv_I8);
            var bge = Instruction.Create(OpCodes.Bge, conv);    // 占位，稍后改指向 maxHead
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
            var maxHead = Instruction.Create(OpCodes.Dup);
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
        Console.WriteLine("[patch] minus_item_reverse_x -> " + m.FullName + " 负额乘 item_reverse_x(缺省 " + kItemReverseDefaultX + ") @ " + reads.Count + " 处取数点");
        return reads.Count;
    }

    // ===================================================================
    // feat16 转生时间无限制：LunHuiWindow::WindowOpen 两处 TotalHours>=1 常量换 -1.0
    // ===================================================================
    static bool PatchZhuanShengFree()
    {
        var m = FindM(FindType("LunHuiWindow"), "WindowOpen", 0);
        var il = m.Body.Instructions;
        if (il.Any(x => x.OpCode.Code == Code.Ldstr && (string)x.Operand == "zhuansheng_free"))
        { Console.WriteLine("[skip] zhuansheng_free 已存在"); return false; }

        var sites = new List<Instruction>();
        for (int i = 1; i < il.Count; i++)
        {
            if (il[i].OpCode != OpCodes.Ldc_R8) continue;
            if (!(il[i].Operand is double d) || d != 1.0) continue;
            if (!(il[i - 1].OpCode.Code == Code.Call && il[i - 1].Operand is MethodReference mr && mr.Name == "get_TotalHours")) continue;
            sites.Add(il[i]);
        }
        if (sites.Count != kExpectZsSites)
            throw new Exception("LunHuiWindow::WindowOpen 的 TotalHours>=1 比较点命中 " + sites.Count + " 处（应为 " + kExpectZsSites + "，基线变了？）");

        foreach (var site in sites)
        {
            int idx = il.IndexOf(site);
            var neg = Instruction.Create(OpCodes.Ldc_R8, -1.0);
            var cont = Instruction.Create(OpCodes.Nop);
            il.Insert(idx, Instruction.Create(OpCodes.Ldstr, "zhuansheng_free"));
            il.Insert(idx + 1, Instruction.Create(OpCodes.Call, mGetBool));
            il.Insert(idx + 2, Instruction.Create(OpCodes.Brtrue, neg));
            // idx+3 = 原 ldc.r8 1（关态路径）
            il.Insert(idx + 4, Instruction.Create(OpCodes.Br, cont));
            il.Insert(idx + 5, neg);
            il.Insert(idx + 6, cont);
        }
        m.Body.MaxStackSize = Math.Max(m.Body.MaxStackSize, 3);
        touched.Add(m);
        Console.WriteLine("[patch] zhuansheng_free -> LunHuiWindow.WindowOpen 取消 1 小时转生冷却（" + sites.Count + " 处比较点）");
        return true;
    }

    // ===================================================================
    // feat17 技能投掷物倍数：SkillAssets::GetBulletAmount 末尾乘 skill_projectile_x
    // ===================================================================
    static int PatchSkillProjectile()
    {
        var m = FindType("SkillAssets").Methods.FirstOrDefault(x => x.Name == "GetBulletAmount");
        if (m == null || !m.HasBody) throw new Exception("SkillAssets::GetBulletAmount 未找到");
        if (HasLdstr(m, "skill_projectile_x")) { Console.WriteLine("[skip] skill_projectile_x 已存在"); return 1; }

        var il = m.Body.GetILProcessor();
        var vRes = new VariableDefinition(mod.TypeSystem.Int32);
        var vX = new VariableDefinition(mod.TypeSystem.Int32);
        m.Body.Variables.Add(vRes);
        m.Body.Variables.Add(vX);
        m.Body.InitLocals = true;

        var ret = m.Body.Instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);
        if (ret == null) throw new Exception("GetBulletAmount 无 ret");

        var skip = Instruction.Create(OpCodes.Ldloc, vRes);
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Stloc, vRes),
            Instruction.Create(OpCodes.Ldstr, "skill_projectile_x"),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Call, mGetInt),
            Instruction.Create(OpCodes.Stloc, vX),
            Instruction.Create(OpCodes.Ldloc, vX),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble, skip),
            Instruction.Create(OpCodes.Ldloc, vRes),
            Instruction.Create(OpCodes.Ldloc, vX),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Stloc, vRes),
            skip,
            Instruction.Create(OpCodes.Ret),
        };
        foreach (var i in seq) il.InsertBefore(ret, i);
        il.Remove(ret);
        if (m.Body.MaxStackSize < 4) m.Body.MaxStackSize = 4;
        touched.Add(m);
        Console.WriteLine("[patch] GetBulletAmount 乘 skill_projectile_x");
        return 1;
    }

    // ===================================================================
    // feat18 技能伤害倍数：get_damage / GetSafeDamage 每个 ret 前乘 skill_damage_x
    // ===================================================================
    static int PatchSkillDamage()
    {
        var sa = FindType("SkillAssets");
        int patched = 0;
        foreach (var name in new[] { "get_damage", "GetSafeDamage" })
        {
            var m = sa.Methods.FirstOrDefault(x => x.Name == name);
            if (m == null || !m.HasBody) throw new Exception("SkillAssets::" + name + " 未找到");
            if (HasLdstr(m, "skill_damage_x")) { Console.WriteLine("[skip] " + name + " 已有 skill_damage_x"); continue; }

            var il = m.Body.GetILProcessor();
            var vL = new VariableDefinition(tLargeNumber);
            var vX = new VariableDefinition(mod.TypeSystem.Int32);
            m.Body.Variables.Add(vL);
            m.Body.Variables.Add(vX);
            m.Body.InitLocals = true;

            var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
            foreach (var ret in rets)
            {
                var skip = Instruction.Create(OpCodes.Ldloc, vL);
                var block = new List<Instruction>
                {
                    Instruction.Create(OpCodes.Stloc, vL),
                    Instruction.Create(OpCodes.Ldstr, "skill_damage_x"),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Call, mGetInt),
                    Instruction.Create(OpCodes.Stloc, vX),
                    Instruction.Create(OpCodes.Ldloc, vX),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Ble, skip),
                    Instruction.Create(OpCodes.Ldloc, vL),
                    Instruction.Create(OpCodes.Ldloc, vX),
                    Instruction.Create(OpCodes.Conv_R8),
                    Instruction.Create(OpCodes.Call, mOpMul),
                    Instruction.Create(OpCodes.Stloc, vL),
                    skip,
                    Instruction.Create(OpCodes.Ret),
                };
                foreach (var i in block) il.InsertBefore(ret, i);
                il.Remove(ret);
                patched++;
            }
            if (m.Body.MaxStackSize < 5) m.Body.MaxStackSize = 5;
            touched.Add(m);
            Console.WriteLine("[patch] " + name + " 乘 skill_damage_x @ " + rets.Count + " 个 ret");
        }
        return patched;
    }

    // ===================================================================
    // feat19 转生无条件升级：LegacyCell 内每处 Calculater.GetLegacyCost* 之后串 StarryCostZero
    // ===================================================================
    static int PatchLegacyPointFree()
    {
        var cell = FindType("LegacySystem.LegacyCell");
        foreach (var mm in cell.Methods)
            if (mm.HasBody && mm.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"))
            {
                int cur = CountCostZeroSites(cell);
                Console.WriteLine("[skip] LegacyCell 已有 StarryCostZero，count=" + cur);
                return cur;
            }

        var costM = EnsureHelper();

        int sites = 0;
        foreach (var mm in cell.Methods)
        {
            if (!mm.HasBody) continue;
            var il = mm.Body.GetILProcessor();
            var calls = mm.Body.Instructions
                .Where(i => i.Operand is MethodReference r
                            && r.DeclaringType.Name == "Calculater"
                            && r.Name.StartsWith("GetLegacyCost", StringComparison.Ordinal))
                .ToList();
            foreach (var c in calls) { il.InsertAfter(c, Instruction.Create(OpCodes.Call, costM)); sites++; }
            if (calls.Count > 0) { Console.WriteLine("[patch] LegacyCell::" + mm.Name + " 成本点 " + calls.Count + " 处"); touched.Add(mm); }
        }
        if (sites != kExpectLegacySites)
            throw new Exception("LegacyCell 成本点 " + sites + " != 写死基线 " + kExpectLegacySites + "（布局变了？必须人工复核后改常量）");
        Console.WriteLine("[patch] zhuan_sheng_free_point -> " + sites + " 处成本点归零");
        return sites;
    }

    // helper 宿主：不新增类型（13.x 的 MethodDef 尾就是现有 cfg 宿主）。
    // feat3 的写法是新建 Starry.StarryLegacy；13.x 增 TypeDef 行会引入「热更集新增类型」风险面，
    // 且与已交付 allfix「TypeDef 无新增」的 RID 口径不一致 => 改挂同一宿主（MethodDef 尾追加，零位移）。
    static MethodReference EnsureHelper()
    {
        var exist = tCfg.Methods.FirstOrDefault(x => x.Name == "StarryCostZero" && x.Parameters.Count == 1);
        if (exist != null) return exist;

        var costM = new MethodDefinition("StarryCostZero",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, tLargeNumber);
        costM.Parameters.Add(new ParameterDefinition("cost", ParameterAttributes.None, tLargeNumber));
        tCfg.Methods.Add(costM);

        var body = costM.Body;
        body.InitLocals = true;
        body.MaxStackSize = 2;
        var vZero = new VariableDefinition(tLargeNumber);
        body.Variables.Add(vZero);
        var il = body.GetILProcessor();
        var retOrig = Instruction.Create(OpCodes.Ldarg_0);
        il.Append(Instruction.Create(OpCodes.Ldstr, "zhuan_sheng_free_point"));
        il.Append(Instruction.Create(OpCodes.Call, mGetBool));
        il.Append(Instruction.Create(OpCodes.Brfalse, retOrig));
        il.Append(Instruction.Create(OpCodes.Ldloca, vZero));
        il.Append(Instruction.Create(OpCodes.Initobj, tLargeNumber));
        il.Append(Instruction.Create(OpCodes.Ldloc, vZero));
        il.Append(Instruction.Create(OpCodes.Ret));
        il.Append(retOrig);
        il.Append(Instruction.Create(OpCodes.Ret));
        touched.Add(costM);
        Console.WriteLine("[patch] helper StarryCostZero 挂在 " + tCfg.FullName + "（不新增类型）");
        return costM;
    }

    static int CountCostZeroSites(TypeDefinition cell)
    {
        int n = 0;
        foreach (var mm in cell.Methods)
            if (mm.HasBody) n += mm.Body.Instructions.Count(i => i.Operand is MethodReference r && r.Name == "StarryCostZero");
        return n;
    }

    // ===================================================================
    // feat20 称号等级（v5，照 断网版 ilpatch_ch_dz1:24-92）
    //   A) PlayerGo::ChangeClothes 入口写 GameData.hounerLevel
    //   B) PlayerGo::RefreshMainSkills：首个向前跳的 brfalse(.s) 换 pop（强制 angleSign.SetActive(true)）
    //   C) PlayerGo::FixedUpdate 入口变化检测 + GameData::OnChangeCloth()
    // ===================================================================
    static int PatchChenghaoLv()
    {
        var pm = FindType("PlayerGo");
        if (pm.Methods.Any(x => x.HasBody && HasLdstr(x, "chenghao_lv")))
        { Console.WriteLine("[skip] chenghao_lv 已存在"); return kExpectChenghaoSites; }

        var mOcc = tGameData.Methods.FirstOrDefault(m => m.Name == "OnChangeCloth" && m.Parameters.Count == 0);
        if (mOcc == null) throw new Exception("GameData::OnChangeCloth() 未找到");
        int n = 0;

        // A) ChangeClothes
        var cc = pm.Methods.FirstOrDefault(m => m.Name == "ChangeClothes" && m.Parameters.Count == 1);
        if (cc == null || !cc.HasBody) throw new Exception("PlayerGo::ChangeClothes(Sprite) 未找到/无体");
        {
            var body = cc.Body;
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            var v = new VariableDefinition(mod.TypeSystem.Int32);
            body.Variables.Add(v);
            var skip = il.Create(OpCodes.Nop);
            var seq = new List<Instruction>
            {
                il.Create(OpCodes.Ldstr, "chenghao_lv"),
                il.Create(OpCodes.Ldc_I4_0),
                il.Create(OpCodes.Call, mGetInt),
                il.Create(OpCodes.Stloc, v),
                il.Create(OpCodes.Ldloc, v),
                il.Create(OpCodes.Ldc_I4_0),
                il.Create(OpCodes.Ble, skip),
                il.Create(OpCodes.Ldsfld, fGameDataI),
                il.Create(OpCodes.Brfalse, skip),
                il.Create(OpCodes.Ldsfld, fGameDataI),
                il.Create(OpCodes.Ldloc, v),
                il.Create(OpCodes.Stfld, fHounerLevel),
                skip,
            };
            foreach (var x in seq) il.InsertBefore(first, x);
            body.MaxStackSize = Math.Max(body.MaxStackSize, 2);
            touched.Add(cc);
            n++;
            Console.WriteLine("[patch] feat20/A ChangeClothes 写入 hounerLevel");
        }
        // B) RefreshMainSkills
        var rms = pm.Methods.FirstOrDefault(m => m.Name == "RefreshMainSkills" && m.Parameters.Count == 0);
        if (rms == null || !rms.HasBody) throw new Exception("PlayerGo::RefreshMainSkills() 未找到/无体");
        {
            var il = rms.Body.GetILProcessor();
            Instruction target = null;
            foreach (var ins in rms.Body.Instructions)
                if ((ins.OpCode == OpCodes.Brfalse || ins.OpCode == OpCodes.Brfalse_S)
                    && ins.Operand is Instruction t && t.Offset > ins.Offset)
                { target = ins; break; }
            if (target == null) throw new Exception("RefreshMainSkills 未找到向前的 brfalse（基线变了？）");
            il.Replace(target, il.Create(OpCodes.Pop));
            touched.Add(rms);
            n++;
            Console.WriteLine("[patch] feat20/B RefreshMainSkills 强制显示");
        }
        // C) FixedUpdate
        var fu = pm.Methods.FirstOrDefault(m => m.Name == "FixedUpdate" && m.Parameters.Count == 0);
        if (fu == null || !fu.HasBody) throw new Exception("PlayerGo::FixedUpdate() 未找到/无体");
        {
            var body = fu.Body;
            var il = body.GetILProcessor();
            var first = body.Instructions[0];
            var v = new VariableDefinition(mod.TypeSystem.Int32);
            body.Variables.Add(v);
            var skip = il.Create(OpCodes.Nop);
            var seq = new List<Instruction>
            {
                il.Create(OpCodes.Ldsfld, fGameDataI),
                il.Create(OpCodes.Brfalse, skip),
                il.Create(OpCodes.Ldstr, "chenghao_lv"),
                il.Create(OpCodes.Ldc_I4_0),
                il.Create(OpCodes.Call, mGetInt),
                il.Create(OpCodes.Stloc, v),
                il.Create(OpCodes.Ldloc, v),
                il.Create(OpCodes.Ldc_I4_0),
                il.Create(OpCodes.Ble, skip),
                il.Create(OpCodes.Ldloc, v),
                il.Create(OpCodes.Ldsfld, fGameDataI),
                il.Create(OpCodes.Ldfld, fHounerLevel),
                il.Create(OpCodes.Beq, skip),
                il.Create(OpCodes.Ldsfld, fGameDataI),
                il.Create(OpCodes.Ldloc, v),
                il.Create(OpCodes.Stfld, fHounerLevel),
                il.Create(OpCodes.Ldsfld, fGameDataI),
                il.Create(OpCodes.Callvirt, mOcc),
                skip,
            };
            foreach (var x in seq) il.InsertBefore(first, x);
            body.MaxStackSize = Math.Max(body.MaxStackSize, 2);
            touched.Add(fu);
            n++;
            Console.WriteLine("[patch] feat20/C FixedUpdate 变化即重刷");
        }
        return n;
    }

    // ===================================================================
    // 落点数自校验（写盘前 + 落盘后各一次）
    // ===================================================================
    static bool Verify(bool quiet)
    {
        bool ok = true;
        void Chk(bool cond, string msg, string detail)
        {
            if (!cond) { ok = false; Console.WriteLine("[verify][FAIL] " + msg + "  " + detail); }
            else if (!quiet) Console.WriteLine("[verify][ok] " + msg + "  " + detail);
        }
        int Ld(MethodDefinition m, string s) => m.HasBody ? m.Body.Instructions.Count(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == s) : -1;

        // ---- feat13 ----
        var show = FindM(FindType("AdsManager/ShowAdsNewFlow"), "Show");
        var refresh = FindM(FindType("ShopWindow"), "Refresh", 0);
        Chk(Ld(show, "shop_ads_unlimited") == 1, "feat13 Show 门 x1", "实际 " + Ld(show, "shop_ads_unlimited"));
        Chk(Ld(refresh, "shop_ads_unlimited") == 1, "feat13 Refresh 门 x1", "实际 " + Ld(refresh, "shop_ads_unlimited"));
        foreach (var pair in new[] { (mm: refresh, tag: "Refresh"), (mm: show, tag: "Show") })
        {
            int zeroed = 0;
            var il = pair.mm.Body.Instructions;
            for (int i = 0; i + 1 < il.Count; i++)
                if (il[i].OpCode.Code == Code.Ldc_I4_0 && il[i + 1].OpCode.Code == Code.Stfld
                    && il[i + 1].Operand is FieldReference f && f.DeclaringType.Name == "DailyData")
                    zeroed++;
            Chk(zeroed == kExpectShopFields, "feat13 " + pair.tag + " 清零 DailyData 计数域 " + kExpectShopFields + " 个", "实际 " + zeroed);
        }

        // ---- feat14 ----
        var sites = CollectReverseSites();
        int gated = 0, skippedSites = 0, ungated = 0;
        foreach (var s in sites)
        {
            if (s.Skip != null) { skippedSites++; continue; }
            if (GateState(s.Call) == 1) gated++; else ungated++;
        }
        Chk(gated == kExpectReverseSites, "feat14 item_reverse 门 " + kExpectReverseSites + " 处（可打点全命中）",
            "实际 " + gated + " / 缺 " + ungated + " / 排除 " + skippedSites + " / 共 " + sites.Count);
        Chk(skippedSites == sites.Count - kExpectReverseSites, "feat14 排除点与写死值一致", "排除 " + skippedSites);

        // ---- feat15 ----
        var mi = FindMinusItemBody();
        var st = MinusItemBodyState(mi);
        Chk(st.head, "feat15 MinusItem 负额序言已插（跳过 item_keep/skin_unlock）", "");
        Chk(st.scale, "feat15 MinusItem 含 item_reverse_x 取数块", "");
        Chk(Ld(mi, "item_reverse_x") == 1, "feat15 item_reverse_x 读取 x1", "实际 " + Ld(mi, "item_reverse_x"));
        var reads = TargetReads(mi);
        Chk(reads.Count > 0 && reads.All(r => r.Next != null && r.Next.OpCode.Code == Code.Dup),
            "feat15 target 取数点 " + reads.Count + " 处全部包上倍率块", "");

        // ---- feat16 ----
        var w = FindM(FindType("LunHuiWindow"), "WindowOpen", 0);
        Chk(Ld(w, "zhuansheng_free") == kExpectZsSites, "feat16 zhuansheng_free 门 " + kExpectZsSites + " 处", "实际 " + Ld(w, "zhuansheng_free"));
        int nNeg = w.Body.Instructions.Count(i => i.OpCode.Code == Code.Ldc_R8 && i.Operand is double d && d == -1.0);
        int nTH = w.Body.Instructions.Count(i => i.OpCode.Code == Code.Call && i.Operand is MethodReference mr && mr.Name == "get_TotalHours");
        Chk(nNeg == kExpectZsSites && nTH == kExpectZsSites, "feat16 ldc.r8 -1 x" + kExpectZsSites + " / TotalHours x" + kExpectZsSites, "实际 " + nNeg + "/" + nTH);

        // ---- feat17 ----
        var gba = FindType("SkillAssets").Methods.First(m => m.Name == "GetBulletAmount");
        Chk(Ld(gba, "skill_projectile_x") == kExpectProjectileSites, "feat17 GetBulletAmount 乘 skill_projectile_x", "实际 " + Ld(gba, "skill_projectile_x"));

        // ---- feat18 ----
        var sa = FindType("SkillAssets");
        int dmg = 0;
        foreach (var nm in new[] { "get_damage", "GetSafeDamage" })
        {
            var m2 = sa.Methods.First(x => x.Name == nm);
            int c = Ld(m2, "skill_damage_x");
            Chk(c == 2, "feat18 " + nm + " 两处 ret 均含 skill_damage_x", "实际 " + c);
            dmg += c;
        }
        Chk(dmg == kExpectDamageRets, "feat18 伤害 ret 落点 " + kExpectDamageRets + " 处", "实际 " + dmg);

        // ---- feat19 ----
        var cell = FindType("LegacySystem.LegacyCell");
        int costSites = CountCostZeroSites(cell);
        Chk(costSites == kExpectLegacySites, "feat19 LegacyCell 成本点 " + kExpectLegacySites + " 处归零", "实际 " + costSites);
        Chk(tCfg.Methods.Any(m => m.Name == "StarryCostZero" && m.HasBody), "feat19 helper StarryCostZero 存在于 " + tCfg.Name, "");

        // ---- feat20 ----
        var pm = FindType("PlayerGo");
        int ccCnt = Ld(pm.Methods.First(m => m.Name == "ChangeClothes" && m.Parameters.Count == 1), "chenghao_lv");
        int fvCnt = Ld(pm.Methods.First(m => m.Name == "FixedUpdate" && m.Parameters.Count == 0), "chenghao_lv");
        Chk(ccCnt == 1 && fvCnt == 1, "feat20 chenghao_lv 门 ChangeClothes x1 / FixedUpdate x1", "实际 " + ccCnt + "/" + fvCnt);
        var rms = pm.Methods.First(m => m.Name == "RefreshMainSkills" && m.Parameters.Count == 0);
        bool popAfterAngle = false;
        var rils = rms.Body.Instructions;
        for (int i = 0; i + 1 < rils.Count; i++)
            if (rils[i].OpCode.Code == Code.Callvirt && rils[i].Operand is MethodReference r2 && r2.Name == "get_Angle"
                && rils[i + 1].OpCode.Code == Code.Pop) popAfterAngle = true;
        Chk(popAfterAngle, "feat20 RefreshMainSkills 的 get_Angle 后 brfalse 已换 pop（强制显示）", "");

        return ok;
    }

    // ===================================================================
    // 盘点模式：只读，打印落点与基线事实（用于标定写死常量）
    // ===================================================================
    static void Scan()
    {
        Console.WriteLine("[scan] ---- feat13 DailyData 计数域 ----");
        foreach (var n in kAdCounterFields)
            Console.WriteLine("[scan]   " + (tDailyData.Fields.Any(f => f.Name == n) ? "有" : "缺") + " " + tDailyData.FullName + "." + n);
        Console.WriteLine("[scan] DailyData 字段总数 " + tDailyData.Fields.Count);

        Console.WriteLine("[scan] ---- feat14/15 item_reverse 调用点 ----");
        var sites = CollectReverseSites();
        int gated = 0;
        foreach (var s in sites)
        {
            string stt = s.Skip != null ? "SKIP(" + s.Skip + ")" : (GateState(s.Call) == 1 ? "已有门" : "PATCH");
            Console.WriteLine("[scan]   " + s.Method.DeclaringType.FullName + "::" + s.Method.Name + "  " + stt);
            if (stt == "已有门") gated++;
        }
        int patchable = sites.Count(s => s.Skip == null) - gated;
        Console.WriteLine("[scan] 总调用点 " + sites.Count + " / 可打 " + patchable + " / 已有门 " + gated + " / 排除 " + sites.Count(s => s.Skip != null));

        Console.WriteLine("[scan] ---- feat15 MinusItem ----");
        var mi = FindMinusItemBody();
        var mst = MinusItemBodyState(mi);
        var guard = FindGuardExit(mi.Body.Instructions);
        Console.WriteLine("[scan]   head=" + mst.head + " scale=" + mst.scale + " targetReads=" + TargetReads(mi).Count
            + " guardExit=" + (guard == null ? "null" : guard.Offset.ToString("X4")));

        Console.WriteLine("[scan] ---- feat16 WindowOpen ----");
        var w = FindM(FindType("LunHuiWindow"), "WindowOpen", 0);
        int zc = 0;
        for (int i = 1; i < w.Body.Instructions.Count; i++)
            if (w.Body.Instructions[i].OpCode == OpCodes.Ldc_R8 && w.Body.Instructions[i].Operand is double d && d == 1.0
                && w.Body.Instructions[i - 1].OpCode.Code == Code.Call
                && w.Body.Instructions[i - 1].Operand is MethodReference mr && mr.Name == "get_TotalHours") zc++;
        Console.WriteLine("[scan]   TotalHours>=1 比较点 " + zc);

        Console.WriteLine("[scan] ---- feat17/18 ----");
        var gba = FindType("SkillAssets").Methods.First(m => m.Name == "GetBulletAmount");
        Console.WriteLine("[scan]   GetBulletAmount ins=" + gba.Body.Instructions.Count + " ret=" + gba.Body.Instructions.Count(i => i.OpCode == OpCodes.Ret));
        foreach (var nm in new[] { "get_damage", "GetSafeDamage" })
            Console.WriteLine("[scan]   " + nm + " ret=" + FindType("SkillAssets").Methods.First(m => m.Name == nm).Body.Instructions.Count(i => i.OpCode == OpCodes.Ret));

        Console.WriteLine("[scan] ---- feat19 LegacyCell 成本点 ----");
        var cell = FindType("LegacySystem.LegacyCell");
        int tot = 0;
        foreach (var mm in cell.Methods)
        {
            if (!mm.HasBody) continue;
            var c = mm.Body.Instructions.Count(i => i.Operand is MethodReference r && r.DeclaringType.Name == "Calculater" && r.Name.StartsWith("GetLegacyCost"));
            if (c > 0) Console.WriteLine("[scan]   LegacyCell::" + mm.Name + " = " + c);
            tot += c;
        }
        Console.WriteLine("[scan]   LegacyCell 成本点合计 " + tot
            + "；CalculateAllLegacyCost=" + CntCost("CalculateAllLegacyCost")
            + " CalculateAllLegacyCostWithCurrent=" + CntCost("CalculateAllLegacyCostWithCurrent"));

        Console.WriteLine("[scan] ---- feat20 PlayerGo ----");
        var pm = FindType("PlayerGo");
        foreach (var nm in new[] { "ChangeClothes", "RefreshMainSkills", "FixedUpdate" })
            Console.WriteLine("[scan]   PlayerGo::" + nm + " ins=" + pm.Methods.First(m => m.Name == nm).Body.Instructions.Count);
        Console.WriteLine("[scan]   GameData.hounerLevel 类型=" + fHounerLevel.FieldType.FullName);

        Console.WriteLine("[scan] ---- 宿主位置 ----");
        Console.WriteLine("[scan]   cfg=" + tCfg.FullName + " 是否最后一个 top-level 类型=" + ReferenceEquals(mod.Types.Last(), tCfg));
        Console.WriteLine("[scan]   mod.Types 末三个=" + string.Join(", ", mod.Types.Skip(Math.Max(0, mod.Types.Count - 3)).Select(t => t.Name)));
    }

    static int CntCost(string name)
    {
        var m = FindType("Calculater").Methods.FirstOrDefault(x => x.Name == name && x.HasBody);
        if (m == null) return -1;
        return m.Body.Instructions.Count(i => i.Operand is MethodReference r && r.Name.StartsWith("GetLegacyCost"));
    }

    // ===================================================================
    // 通用工具
    // ===================================================================
    static MethodDefinition FindM(TypeDefinition t, string name, int? paramCount = null)
    {
        var list = t.Methods.Where(x => x.Name == name && (paramCount == null || x.Parameters.Count == paramCount)).ToList();
        if (list.Count == 0) throw new Exception(t.FullName + "::" + name + " 未找到");
        if (list.Count > 1) throw new Exception(t.FullName + "::" + name + " 命中 " + list.Count + " 个重载");
        if (!list[0].HasBody) throw new Exception(t.FullName + "::" + name + " 无方法体");
        return list[0];
    }

    static TypeDefinition FindType(string name)
    {
        var direct = mod.GetType(name);
        if (direct != null) return direct;
        var simple = name.Contains('/') ? name.Substring(name.LastIndexOf('/') + 1) : name;
        var matches = AllTypes().Where(x => x.Name == simple).ToList();
        if (matches.Count == 1) return matches[0];
        if (matches.Count == 0) throw new Exception("类型不存在: " + name);
        throw new Exception("类型名不唯一: " + name + " -> " + matches.Count);
    }

    static IEnumerable<TypeDefinition> AllTypes() => AllTypes(mod.Types);

    static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (var t in types)
        {
            yield return t;
            foreach (var n in AllTypes(t.NestedTypes)) yield return n;
        }
    }

    static bool HasLdstr(MethodDefinition m, string s)
        => m.HasBody && m.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == s);

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
                int t;
                if (idx.TryGetValue(tgt, out t) && t < j) res.Add(new[] { t, j });
            }
            else if (list[j].Operand is Instruction[] tgs)
            {
                foreach (var tt in tgs)
                {
                    int t2;
                    if (idx.TryGetValue(tt, out t2) && t2 < j) res.Add(new[] { t2, j });
                }
            }
        }
        return res;
    }
}

// 跳转放宽：把插入指令后越界的短跳转升级为长跳转（照 ilpatch_vc1637:1717-1775）
static class BranchRelax
{
    static readonly Dictionary<Code, OpCode> Long = new Dictionary<Code, OpCode>
    {
        { Code.Br_S, OpCodes.Br }, { Code.Brfalse_S, OpCodes.Brfalse }, { Code.Brtrue_S, OpCodes.Brtrue },
        { Code.Beq_S, OpCodes.Beq }, { Code.Bge_S, OpCodes.Bge }, { Code.Bgt_S, OpCodes.Bgt },
        { Code.Ble_S, OpCodes.Ble }, { Code.Blt_S, OpCodes.Blt }, { Code.Bne_Un_S, OpCodes.Bne_Un },
        { Code.Bge_Un_S, OpCodes.Bge_Un }, { Code.Bgt_Un_S, OpCodes.Bgt_Un },
        { Code.Ble_Un_S, OpCodes.Ble_Un }, { Code.Blt_Un_S, OpCodes.Blt_Un },
        { Code.Leave_S, OpCodes.Leave },
    };

    public static int WidenOutOfRange(ModuleDefinition mod)
    {
        int total = 0;
        foreach (var t in All(mod.Types))
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                var ins = m.Body.Instructions;
                if (ins.Count < 2) continue;
                var repl = new Dictionary<Instruction, Instruction>();
                foreach (var i in ins)
                {
                    if (!Long.ContainsKey(i.OpCode.Code)) continue;
                    Instruction tgt = i.Operand as Instruction;
                    if (tgt == null) continue;
                    int next = i.Offset + i.GetSize();
                    int delta = tgt.Offset - next;
                    if (delta >= -128 && delta <= 127) continue;
                    repl[i] = Instruction.Create(Long[i.OpCode.Code], tgt);
                }
                if (repl.Count == 0) continue;
                for (int k = 0; k < ins.Count; k++)
                {
                    var i = ins[k];
                    Instruction o = i.Operand as Instruction;
                    if (o != null)
                    {
                        Instruction no;
                        if (repl.TryGetValue(o, out no)) i.Operand = no;
                    }
                    var oa = i.Operand as Instruction[];
                    if (oa != null)
                        for (int j = 0; j < oa.Length; j++)
                        {
                            Instruction no2;
                            if (repl.TryGetValue(oa[j], out no2)) oa[j] = no2;
                        }
                }
                for (int k = 0; k < ins.Count; k++)
                {
                    Instruction ni;
                    if (repl.TryGetValue(ins[k], out ni)) ins[k] = ni;
                }
                total += repl.Count;
            }
        return total;
    }

    static IEnumerable<TypeDefinition> All(IEnumerable<TypeDefinition> types)
    {
        foreach (var t in types)
        {
            yield return t;
            foreach (var n in All(t.NestedTypes)) yield return n;
        }
    }
}
