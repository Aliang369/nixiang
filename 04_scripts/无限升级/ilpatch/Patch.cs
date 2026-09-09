using Mono.Cecil;
using Mono.Cecil.Cil;

// 无限升级 targetfuntional.dll IL 注入（全量重建路径，输入=原始 dll）
// 原则（铁律）：有过滤（全部经 StarryConfig 开关，配置缺失=原版行为）；调原函数（关→原 IL 原样执行）
// 产物：07_patched/无限升级/targetfuntional.dll
//
// ⚠️ 版本演进（2026-09-06）：本文件只覆盖 v1+v2（22 patches）。v3（全离线：断连三通道+
//    8 个远程执法开关 no-op+COS 直传杀+免广告离线化）在 04_scripts/无限升级/ilpatch_delta/
//    以增量链维护（v1备份→v2→v3，Cecil 模式化）。原始 dll 字节已随 03_decompiled 清理，
//    07_patched/无限升级/targetfuntional_v1_20260906.dll 是全链复现的必需输入，勿删。
//    若未来取回原始 dll 全量重建：先跑本文件得 v1，再依次 ilpatch_delta v2、v3。
internal static class Patcher
{
    const string FILE_NAME = "starry_config.ini";

    static ModuleDefinition mod;
    static MethodReference mGetBool, mGetInt;
    static Dictionary<string, MethodDefinition> cfgMethods = new();
    static TypeReference tStr, tI32, tBool;

    public static void Run(string srcPath, string dstPath)
    {
        var asm = AssemblyDefinition.ReadAssembly(srcPath);
        mod = asm.MainModule;
        Console.WriteLine($"[load] {mod.Types.Count} types");

        BuildStarryConfig();

        int patched = 0;
        void P(Action f, string name) { f(); Console.WriteLine($"[patch] {name}"); patched++; }

        // ===== 六、上报屏蔽（key=block_report）=====
        P(() => EarlyReturnVoid(FindM(mod.GetType("InfinityCore.ItemUseAnomalyMonitor"), "CheckAmount"), "block_report"), "上报 ItemUseAnomalyMonitor.CheckAmount");
        P(() => EarlyReturnVoid(FindM(mod.GetType("DamageCheatRecordData"), "Record"), "block_report"), "上报 DamageCheatRecordData.Record");
        var npm = mod.GetType("NetWorkPlayerMgr");
        P(() => EarlyReturnVoid(FindM(npm, "CheckBan"), "block_report"), "上报 CheckBan");
        P(() => EarlyReturnVoid(FindM(npm, "CheckBanReport"), "block_report"), "上报 CheckBanReport");
        P(() => EarlyReturnVoid(FindM(npm, "CheckBan2"), "block_report"), "上报 CheckBan2");
        P(() => EarlyReturnVoid(FindM(npm, "BanGameAciton"), "block_report"), "上报 BanGameAciton(本地封号总闸)");
        P(() => EarlyReturnVoid(FindM(npm, "Report"), "block_report"), "上报 Report(AddPlayerReport)");
        P(() => SendRTBlacklist(FindM(mod.GetType("NetBehaviour"), "SendRT")), "上报 SendRT 黑名单(10 fun)");
        P(() => SendRTBlacklist(FindM(mod.GetType("AccountMgr.LoginScene.AccountMgrClient"), "SendRT", 3)), "上报 AccountMgrClient.SendRT(byte,string,object[]) 黑名单(10 fun)");
        P(() => EarlyReturnVoid(FindM(mod.GetType("InfinityCoreNormal.NetConver"), "ForceBanGameAciton"), "block_report"), "上报 NetConver.ForceBanGameAciton");

        // ===== 五、七项功能 =====
        P(() => EarlyReturnVoid(FindM(mod.GetType("PlayerMgr"), "OnHit"), "god"), "无敌 PlayerMgr.OnHit 入口返回");
        P(() => Oneshot(FindM(mod.GetType("Enemy"), "OnHit")), "秒杀 Enemy.OnHit Hp-=Hp");
        P(() => Oneshot(FindM(mod.GetType("BossEnemy"), "OnHit")), "秒杀 BossEnemy.OnHit Hp-=Hp");
        P(() => EarlyReturnI4(FindM(mod.GetType("GameData"), "GetSkillUseAmount"), "no_skill_limit", 99), "无限技能 GetSkillUseAmount→99");
        P(() => EarlyReturnI8(FindM(mod.GetType("Calculater"), "CalculateSkillUpgrad"), "skill_free", 1), "技能免费 CalculateSkillUpgrad→1");
        P(() => EarlyReturnVoid(FindM(mod.GetType("SkillDetialWindow"), "QuickUpgrade"), "skill_free"), "技能免费 禁用一键升级(原生路径tagged-pointer崩溃,规避)");
        P(() => EarlyReturnBool(FindM(mod.GetType("SkillDetialWindow"), "CanLevelUp"), "skill_free", true), "技能合成 CanLevelUp→true");
        P(() => EarlyReturnVoid(FindM(mod.GetType("GameData"), "MinusItem"), "item_keep"), "物品不减 MinusItem 入口返回");
        P(() => AddItemDouble(FindM(mod.GetType("GameData"), "AddItem", 2)), "物品翻倍 AddItem 数量×2");
        P(() => AdsRewardMultiply(FindM(npm, "GetAdsRewardCallBack")), "广告奖励倍数 GetAdsRewardCallBack×N");
        P(() => ShowAdsFree(), "免广告 ShowAdsNewFlow.Show→AdsFree 原路径");

        // ===== 诊断模式2（WXSJ_DIAG=1）：配置读取链路插桩；正式包不带此变量重出 =====
        if (Environment.GetEnvironmentVariable("WXSJ_DIAG") == "1")
        {
            var dbg = new TypeReference("UnityEngine", "Debug", mod, AsmRef("UnityEngine.CoreModule"));
            var mlog = new MethodReference("Log", mod.TypeSystem.Void, dbg);
            mlog.Parameters.Add(new ParameterDefinition(mod.TypeSystem.Object));
            var tStrD = new TypeReference("System", "String", mod, Corlib());
            var cat2 = new MethodReference("Concat", tStrD, tStrD) { HasThis = false };
            cat2.Parameters.Add(new ParameterDefinition(tStrD));
            cat2.Parameters.Add(new ParameterDefinition(tStrD));

            // ① Get(): 打印实际读取路径 + 成败
            var mGet = (MethodDefinition)cfgMethods["Get"];
            var g = mGet.Body.Instructions;
            int pdpIdx = -1;
            for (int i = 0; i < g.Count; i++)
                if (g[i].OpCode.Code == Mono.Cecil.Cil.Code.Call && g[i].Operand is MethodReference mm && mm.Name == "ReadAllLines")
                { pdpIdx = i; break; }
            if (pdpIdx > 0)
            {
                g.Insert(pdpIdx, Instruction.Create(OpCodes.Call, mlog));      // 先插后段
                g.Insert(pdpIdx, Instruction.Create(OpCodes.Dup));             // 再插 dup（位于 call 前）
                Console.WriteLine("[diag2] Get: path log inserted");
            }
            // 成功路径日志：stsfld lines 之后
            for (int i = 0; i < g.Count; i++)
                if (g[i].OpCode.Code == Mono.Cecil.Cil.Code.Stsfld && g[i].Operand is FieldReference ff && ff.Name == "lines")
                {
                    var okLog = new List<Instruction>
                    {
                        Instruction.Create(OpCodes.Ldstr, "[WxsjDiag2] cfg loaded OK"),
                        Instruction.Create(OpCodes.Call, mlog),
                    };
                    for (int k = 0; k < okLog.Count; k++) g.Insert(i + 1 + k, okLog[k]);
                    Console.WriteLine("[diag2] Get: success log inserted");
                    break;
                }
            // 失败路径日志：catch 的 pop 之后
            for (int i = 0; i < g.Count; i++)
                if (g[i].OpCode.Code == Mono.Cecil.Cil.Code.Pop)
                {
                    var failLog = new List<Instruction>
                    {
                        Instruction.Create(OpCodes.Ldstr, "[WxsjDiag2] cfg load FAILED(exception)"),
                        Instruction.Create(OpCodes.Call, mlog),
                    };
                    for (int k = 0; k < failLog.Count; k++) g.Insert(i + 1 + k, failLog[k]);
                    Console.WriteLine("[diag2] Get: catch log inserted");
                    break;
                }

            // ② GetBool(): 打印 key + 结果
            var mgb = (MethodDefinition)mGetBool;
            var b = mgb.Body.Instructions;
            var vB = mgb.Body.Variables[0];
            var lT = Instruction.Create(OpCodes.Nop);
            var lF = Instruction.Create(OpCodes.Nop);
            var lC = Instruction.Create(OpCodes.Nop);
            var logT = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldstr, "[WxsjDiag2] GetBool "),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, cat2),
                Instruction.Create(OpCodes.Ldstr, "=T"),
                Instruction.Create(OpCodes.Call, cat2),
                Instruction.Create(OpCodes.Call, mlog),
                Instruction.Create(OpCodes.Br, lC),
            };
            var logF = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldstr, "[WxsjDiag2] GetBool "),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, cat2),
                Instruction.Create(OpCodes.Ldstr, "=F"),
                Instruction.Create(OpCodes.Call, cat2),
                Instruction.Create(OpCodes.Call, mlog),
            };
            // 找到 stloc vB（索引2）之后插入：ldloc vB; brfalse lF; [logT]; br lC? 顺序整理：
            // 在 index3 位置插入：ldloc vB, brfalse lF, logT(…br lC), lF: logF, lC:
            var ins = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldloc, vB),
                Instruction.Create(OpCodes.Brfalse, lF),
            };
            ins.AddRange(logT);
            ins.Insert(ins.Count - 0, lT);  // logT 末尾已是 br lC；lT 占位放 logT 前？直接顺序即可
            ins.RemoveAt(ins.Count - 1);    // 移除占位（logT 已含 br lC）
            ins.Add(lF);
            ins.AddRange(logF);
            ins.Add(lC);
            for (int k = 0; k < ins.Count; k++) b.Insert(3 + k, ins[k]);
            Console.WriteLine("[diag2] GetBool: key+result log inserted");
        }

        var dir = Path.GetDirectoryName(dstPath)!;
        Directory.CreateDirectory(dir);
        asm.Write(dstPath);
        Console.WriteLine($"[done] {patched} patches → {dstPath}");
    }

    // ---------- 查找 ----------
    static MethodDefinition FindM(TypeDefinition t, string name, int? paramCount = null)
    {
        var list = t.Methods.Where(m => m.Name == name &&
            (paramCount == null || m.Parameters.Count == paramCount)).ToList();
        if (list.Count == 0) throw new Exception($"{t.FullName}::{name} not found");
        if (list.Count > 1) throw new Exception($"{t.FullName}::{name} has {list.Count} overloads");
        if (!list[0].HasBody) throw new Exception($"{t.FullName}::{name} no body");
        return list[0];
    }

    static AssemblyNameReference Corlib() => mod.AssemblyReferences.First(r => r.Name == "mscorlib");

    // ---------- StarryConfig ----------
    // 文件：<Application.persistentDataPath>/starry_config.ini，每行 key=value
    // 文件不存在/解析异常 → 全默认（关）→ 与原版行为一致；2 秒缓存
    static void BuildStarryConfig()
    {
        tStr = mod.TypeSystem.String;
        tI32 = mod.TypeSystem.Int32;
        tBool = mod.TypeSystem.Boolean;
        var tObj = mod.TypeSystem.Object;
        var tStrArr = new ArrayType(tStr);
        var tExc = new TypeReference("System", "Exception", mod, Corlib());

        var cfgType = new TypeDefinition("Starry", "StarryConfig",
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit, tObj);
        mod.Types.Add(cfgType);
        var fLines = new FieldDefinition("lines", FieldAttributes.Static | FieldAttributes.Private, tStrArr);
        var fNext = new FieldDefinition("nextCheck", FieldAttributes.Static | FieldAttributes.Private, tI32);
        cfgType.Fields.Add(fLines);
        cfgType.Fields.Add(fNext);

        var env = new TypeReference("System", "Environment", mod, Corlib());
        var mTick = new MethodReference("get_TickCount", tI32, env);
        var app = new TypeReference("UnityEngine", "Application", mod, AsmRef("UnityEngine.CoreModule"));
        var mPdp = new MethodReference("get_persistentDataPath", tStr, app);
        var file = new TypeReference("System.IO", "File", mod, Corlib());
        var mReadAll = new MethodReference("ReadAllLines", tStrArr, file);
        mReadAll.Parameters.Add(new ParameterDefinition(tStr));
        var mConcat = new MethodReference("Concat", tStr, tStr) { HasThis = false };
        mConcat.Parameters.Add(new ParameterDefinition(tStr));
        mConcat.Parameters.Add(new ParameterDefinition(tStr));
        var mStartsWith = new MethodReference("StartsWith", tBool, tStr) { HasThis = true };
        mStartsWith.Parameters.Add(new ParameterDefinition(tStr));
        var mSubstr = new MethodReference("Substring", tStr, tStr) { HasThis = true };
        mSubstr.Parameters.Add(new ParameterDefinition(tI32));
        var mLen = new MethodReference("get_Length", tI32, tStr) { HasThis = true };
        var mEq = new MethodReference("op_Equality", tBool, tStr) { HasThis = false };
        mEq.Parameters.Add(new ParameterDefinition(tStr));
        mEq.Parameters.Add(new ParameterDefinition(tStr));
        var mParse = new MethodReference("Parse", tI32, mod.TypeSystem.Int32);
        mParse.Parameters.Add(new ParameterDefinition(tStr));

        // .cctor
        var cctor = new MethodDefinition(".cctor",
            MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            mod.TypeSystem.Void);
        cfgType.Methods.Add(cctor);
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fLines));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        // static string Get(string key)
        var mGet = new MethodDefinition("Get",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tStr);
        mGet.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        cfgType.Methods.Add(mGet);
        cfgMethods["Get"] = mGet;
        var vLine = new VariableDefinition(tStr);
        var vI = new VariableDefinition(tI32);
        mGet.Body.Variables.Add(vLine);
        mGet.Body.Variables.Add(vI);
        mGet.Body.InitLocals = true;
        var g = mGet.Body.Instructions;
        var loopExit = Instruction.Create(OpCodes.Ldnull);        // lines==null → return null
        var skipReload = Instruction.Create(OpCodes.Nop);
        // if (Environment.TickCount > nextCheck)
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fNext));
        g.Add(Instruction.Create(OpCodes.Ble, skipReload));
        // { nextCheck = TickCount + 2000; try { lines = File.ReadAllLines(pdp+"/"+FILE_NAME); } catch { lines = null; } }
        g.Add(Instruction.Create(OpCodes.Ldc_I4, 2000));
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        // HybridCLR 解释器下 Application.persistentDataPath 返回空（实测 2026-09-05），
        // 弃用 Unity API，直接硬编码包内绝对路径（本构建为 WXSJ 专用）
        var t0 = Instruction.Create(OpCodes.Ldstr, "/data/user/0/com.xingzuodaoshi.infinityUpgrade/files/" + FILE_NAME);
        g.Add(t0);
        g.Add(Instruction.Create(OpCodes.Call, mReadAll));
        g.Add(Instruction.Create(OpCodes.Stsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Leave, skipReload));
        var h0 = Instruction.Create(OpCodes.Pop);
        g.Add(h0);
        g.Add(Instruction.Create(OpCodes.Ldnull));
        g.Add(Instruction.Create(OpCodes.Stsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Leave, skipReload));
        mGet.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = t0, TryEnd = h0, HandlerStart = h0, HandlerEnd = skipReload, CatchType = tExc,
        });
        g.Add(skipReload);
        // foreach (line in lines): if (line.StartsWith(key+"=")) return line.Substring(key.Length+1)
        g.Add(Instruction.Create(OpCodes.Ldsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Brfalse, loopExit));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        g.Add(Instruction.Create(OpCodes.Stloc, vI));
        var loopTop = Instruction.Create(OpCodes.Nop);
        g.Add(loopTop);
        g.Add(Instruction.Create(OpCodes.Ldloc, vI));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Ldlen));
        g.Add(Instruction.Create(OpCodes.Conv_I4));
        g.Add(Instruction.Create(OpCodes.Bge, loopExit));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Ldloc, vI));
        g.Add(Instruction.Create(OpCodes.Ldelem_Ref));
        g.Add(Instruction.Create(OpCodes.Stloc, vLine));
        g.Add(Instruction.Create(OpCodes.Ldloc, vLine));
        g.Add(Instruction.Create(OpCodes.Ldarg_0));
        g.Add(Instruction.Create(OpCodes.Ldstr, "="));
        g.Add(Instruction.Create(OpCodes.Call, mConcat));
        g.Add(Instruction.Create(OpCodes.Callvirt, mStartsWith));
        var nextIter = Instruction.Create(OpCodes.Nop);
        g.Add(Instruction.Create(OpCodes.Brfalse, nextIter));
        g.Add(Instruction.Create(OpCodes.Ldloc, vLine));
        g.Add(Instruction.Create(OpCodes.Ldarg_0));
        g.Add(Instruction.Create(OpCodes.Callvirt, mLen));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Callvirt, mSubstr));
        g.Add(Instruction.Create(OpCodes.Ret));
        g.Add(nextIter);
        g.Add(Instruction.Create(OpCodes.Ldloc, vI));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Stloc, vI));
        g.Add(Instruction.Create(OpCodes.Br, loopTop));
        g.Add(loopExit);
        g.Add(Instruction.Create(OpCodes.Ret));

        // static bool GetBool(string key)  =>  v=Get(key); v=="1"||v=="true"
        var mGetBoolDef = new MethodDefinition("GetBool",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tBool);
        mGetBoolDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        cfgType.Methods.Add(mGetBoolDef);
        var vB = new VariableDefinition(tStr);
        mGetBoolDef.Body.Variables.Add(vB);
        mGetBoolDef.Body.InitLocals = true;
        var b = mGetBoolDef.Body.Instructions;
        var falsePath = Instruction.Create(OpCodes.Ldc_I4_0);
        b.Add(Instruction.Create(OpCodes.Ldarg_0));
        b.Add(Instruction.Create(OpCodes.Call, mGet));
        b.Add(Instruction.Create(OpCodes.Stloc, vB));
        b.Add(Instruction.Create(OpCodes.Ldloc, vB));
        b.Add(Instruction.Create(OpCodes.Brfalse, falsePath));
        b.Add(Instruction.Create(OpCodes.Ldloc, vB));
        b.Add(Instruction.Create(OpCodes.Ldstr, "1"));
        b.Add(Instruction.Create(OpCodes.Call, mEq));
        var notOne = Instruction.Create(OpCodes.Nop);
        b.Add(Instruction.Create(OpCodes.Brfalse, notOne));
        b.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        b.Add(Instruction.Create(OpCodes.Ret));
        b.Add(notOne);
        b.Add(Instruction.Create(OpCodes.Ldloc, vB));
        b.Add(Instruction.Create(OpCodes.Ldstr, "true"));
        b.Add(Instruction.Create(OpCodes.Call, mEq));
        b.Add(Instruction.Create(OpCodes.Ret));
        b.Add(falsePath);
        b.Add(Instruction.Create(OpCodes.Ret));
        mGetBool = mGetBoolDef;

        // static int GetInt(string key, int def)  =>  v=Get(key); try Parse catch def
        var mGetDef = new MethodDefinition("GetInt",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tI32);
        mGetDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        mGetDef.Parameters.Add(new ParameterDefinition("def", ParameterAttributes.None, tI32));
        cfgType.Methods.Add(mGetDef);
        var vV = new VariableDefinition(tStr);
        var vR = new VariableDefinition(tI32);
        mGetDef.Body.Variables.Add(vV);
        mGetDef.Body.Variables.Add(vR);
        mGetDef.Body.InitLocals = true;
        var mi = mGetDef.Body.Instructions;
        var defPath = Instruction.Create(OpCodes.Ldarg_1);
        mi.Add(Instruction.Create(OpCodes.Ldarg_0));
        mi.Add(Instruction.Create(OpCodes.Call, mGet));
        mi.Add(Instruction.Create(OpCodes.Stloc, vV));
        mi.Add(Instruction.Create(OpCodes.Ldloc, vV));
        mi.Add(Instruction.Create(OpCodes.Brfalse, defPath));
        var tP = Instruction.Create(OpCodes.Ldloc, vV);
        mi.Add(tP);
        mi.Add(Instruction.Create(OpCodes.Call, mParse));
        mi.Add(Instruction.Create(OpCodes.Stloc, vR));
        var afterTry = Instruction.Create(OpCodes.Ldloc, vR);
        mi.Add(Instruction.Create(OpCodes.Leave, afterTry));
        var hP = Instruction.Create(OpCodes.Pop);
        mi.Add(hP);
        mi.Add(Instruction.Create(OpCodes.Leave, defPath));
        mi.Add(afterTry);
        mi.Add(Instruction.Create(OpCodes.Ret));
        mi.Add(defPath);
        mi.Add(Instruction.Create(OpCodes.Ret));
        mGetDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = tP, TryEnd = hP, HandlerStart = hP, HandlerEnd = defPath, CatchType = tExc,
        });
        mGetInt = mGetDef;
        Console.WriteLine("[cfg] StarryConfig created (2s cache, persistentDataPath/starry_config.ini)");
    }

    static AssemblyNameReference AsmRef(string name)
    {
        var r = mod.AssemblyReferences.FirstOrDefault(x => x.Name == name);
        if (r != null) return r;
        r = new AssemblyNameReference(name, new Version(0, 0, 0, 0));
        mod.AssemblyReferences.Add(r);
        return r;
    }

    // ---------- 通用注入 ----------
    static void InsertAt(MethodDefinition m, int index, IEnumerable<Instruction> seq)
    {
        var ins = m.Body.Instructions;
        int i = index;
        foreach (var x in seq) ins.Insert(i++, x);
    }

    /// void 方法入口：开关开 → 直接 return；关 → 原逻辑
    static void EarlyReturnVoid(MethodDefinition m, string key)
    {
        var first = m.Body.Instructions[0];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, key),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
            Instruction.Create(OpCodes.Ret),
        };
        InsertAt(m, 0, seq);
    }

    static void EarlyReturnBool(MethodDefinition m, string key, bool val)
    {
        var first = m.Body.Instructions[0];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, key),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
            Instruction.Create(val ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Ret),
        };
        InsertAt(m, 0, seq);
    }

    static void EarlyReturnI4(MethodDefinition m, string key, int val)
    {
        var first = m.Body.Instructions[0];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, key),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
            Instruction.Create(OpCodes.Ldc_I4, val),
            Instruction.Create(OpCodes.Ret),
        };
        InsertAt(m, 0, seq);
    }

    static void EarlyReturnI8(MethodDefinition m, string key, long val)
    {
        var first = m.Body.Instructions[0];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, key),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
            Instruction.Create(OpCodes.Ldc_I4, (int)val),
            Instruction.Create(OpCodes.Conv_I8),
            Instruction.Create(OpCodes.Ret),
        };
        InsertAt(m, 0, seq);
    }

    /// SendRT 黑名单：func ∈ 黑名单 → return；否则原逻辑
    /// v2（2026-09-06）新增 "CheckMMM"（登录时 本地免广告计时>0 而服务器 adsFree=false 的上报）；
    /// 并同步用于 AccountMgrClient.SendRT（该类不继承 NetBehaviour，v1 过滤器管不到，
    /// 实锤泄漏 AccountMgrHandler.UploadLog=全量 Error/Exception 日志+堆栈）
    static void SendRTBlacklist(MethodDefinition m)
    {
        string[] block = {
            "CheckBan", "CheckBanReport", "AddPlayerReport", "RecordItemUseAnomaly",
            "AddRecord", "ShowLogs", "ShowMessage", "UploadLog", "SendLocalLogToServer",
            "CheckMMM",
        };
        var first = m.Body.Instructions[0];
        var retI = Instruction.Create(OpCodes.Ret);
        var seq = new List<Instruction>();
        foreach (var fun in block)
        {
            seq.Add(Instruction.Create(OpCodes.Ldarg_2));           // func
            seq.Add(Instruction.Create(OpCodes.Ldstr, fun));
            seq.Add(Instruction.Create(OpCodes.Call, StringEq()));
            seq.Add(Instruction.Create(OpCodes.Brtrue, retI));
        }
        seq.Add(Instruction.Create(OpCodes.Br, first));
        seq.Add(retI);
        InsertAt(m, 0, seq);
    }

    static MethodReference _eq;
    static MethodReference StringEq()
    {
        if (_eq != null) return _eq;
        _eq = new MethodReference("op_Equality", tBool, tStr) { HasThis = false };
        _eq.Parameters.Add(new ParameterDefinition(tStr));
        _eq.Parameters.Add(new ParameterDefinition(tStr));
        return _eq;
    }

    /// 秒杀：定位 [ldarg.0, ldarg.0, call get_Hp, ldloc targetD, call op_Subtraction, call set_Hp]
    /// 开 → Hp -= Hp（原 OnDead 链照走）；关 → 原序列
    static void Oneshot(MethodDefinition m)
    {
        var ins = m.Body.Instructions;
        int idx = -1;
        MethodReference getHp = null, setHp = null, subOp = null;
        for (int i = 3; i < ins.Count - 2; i++)
        {
            if (ins[i].Operand is not MethodReference mr || mr.Name != "op_Subtraction") continue;
            if (ins[i + 1].Operand is MethodReference s && s.Name == "set_Hp" &&
                ins[i - 2].Operand is MethodReference g && g.Name == "get_Hp")
            {
                idx = i - 3; getHp = g; setHp = s; subOp = mr; break;
            }
        }
        if (idx < 0) throw new Exception($"{m.FullName}: 未找到 get_Hp/op_Subtraction/set_Hp 序列");
        var seqStart = ins[idx];
        var after = ins[idx + 5 + 1]; // set_Hp 之后
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "oneshot"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, seqStart),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, getHp),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, getHp),
            Instruction.Create(OpCodes.Call, subOp),
            Instruction.Create(OpCodes.Call, setHp),
            Instruction.Create(OpCodes.Br, after),
        };
        InsertAt(m, idx, seq);
    }

    /// 物品翻倍：入口 target(arg2, long) ×2 后落入原逻辑
    static void AddItemDouble(MethodDefinition m)
    {
        var first = m.Body.Instructions[0];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "item_double"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
            Instruction.Create(OpCodes.Ldarg_2),
            Instruction.Create(OpCodes.Ldc_I8, 2L),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Starg_S, m.Parameters[1]),
        };
        InsertAt(m, 0, seq);
    }

    /// 广告奖励倍数：入口 amount(arg2, int) ×N（N=GetInt("ads_reward_x")，<=1 走原逻辑）
    static void AdsRewardMultiply(MethodDefinition m)
    {
        var first = m.Body.Instructions[0];
        var vMult = new VariableDefinition(tI32);
        m.Body.Variables.Add(vMult);
        m.Body.InitLocals = true;
        var orig = Instruction.Create(OpCodes.Nop);
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "ads_reward_x"),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Call, mGetInt),
            Instruction.Create(OpCodes.Stloc, vMult),
            Instruction.Create(OpCodes.Ldloc, vMult),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble, orig),
            Instruction.Create(OpCodes.Ldarg_2),
            Instruction.Create(OpCodes.Ldloc, vMult),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Starg_S, m.Parameters[1]),
            orig,
        };
        InsertAt(m, 0, seq);
    }

    /// 免广告：在 action2 委托构造完成后注入分支（确保奖励回调随 AdsFree 执行）
    /// 开 → 跳过登录/网络/每日上限/条件判断，直接 AdsFree(action2, countAds)；关 → 原逻辑
    static void ShowAdsFree()
    {
        var show = FindM(mod.GetType("AdsManager/ShowAdsNewFlow"), "Show");
        var ins = show.Body.Instructions;
        int call = -1;
        for (int i = 0; i < ins.Count; i++)
            if (ins[i].Operand is MethodReference mr && mr.Name == "AdsFree") { call = i; break; }
        if (call < 0) throw new Exception("Show: AdsFree call not found");
        // 回溯到 AdsFree 压栈块首（4 条 load：ldarg.0/ldfld owner/ldloc.1 action2/ldarg.2）
        int push = call;
        int loads = 0;
        while (push > 0 && loads < 4)
        {
            var c = ins[push - 1].OpCode.Code;
            bool isLoad = c == Code.Ldarg || c == Code.Ldarg_S || (c >= Code.Ldarg_0 && c <= Code.Ldarg_3) ||
                          c == Code.Ldloc || c == Code.Ldloc_S || (c >= Code.Ldloc_0 && c <= Code.Ldloc_3) ||
                          c == Code.Ldfld;
            if (!isLoad) break;
            push--; loads++;
        }
        if (loads != 4) throw new Exception($"Show: AdsFree 前压栈指令数 {loads} != 4，IL 变了，需人工复核");
        // 找 newobj Action(object, native int)（委托构造）→ 跳过其后的 stloc → 注入点=条件判断起点
        int ctorIdx = -1;
        for (int i = 0; i < ins.Count; i++)
            if (ins[i].Operand is MethodReference mr2 && mr2.Name == ".ctor" &&
                mr2.DeclaringType.Name == "Action" && ins[i].OpCode.Code == Code.Newobj)
            { ctorIdx = i; break; }
        if (ctorIdx < 0) throw new Exception("Show: Action 委托构造 newobj not found");
        int inject = ctorIdx + 1;
        if (ins[inject].OpCode.Code == Code.Stloc || ins[inject].OpCode.Code == Code.Stloc_S ||
            (ins[inject].OpCode.Code >= Code.Stloc_0 && ins[inject].OpCode.Code <= Code.Stloc_3))
            inject++;
        if (inject > push) throw new Exception("Show: 注入点位于 AdsFree 压栈块之后，IL 布局异常");
        var first = ins[inject];
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "free_ads"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, first),
            Instruction.Create(OpCodes.Br, ins[push]),
        };
        InsertAt(show, inject, seq);
    }
    /// QuickUpgrade 防死循环：skill_free 时单价=0 → while(灵魂>=0) 永真 → 卡死。
    /// 回边前查 skill_free，开 → 跳出循环（一键升级 = 每点一次 +1 级，免费）
    static void QuickUpgradeAntiLoop()
    {
        var m = FindM(mod.GetType("SkillDetialWindow"), "QuickUpgrade");
        var ins = m.Body.Instructions;
        int calcCount = 0, loopbr = -1;
        for (int i = 0; i < ins.Count; i++)
        {
            if (ins[i].Operand is MethodReference mr && mr.Name == "CalculateSkillUpgrad" &&
                ins[i].OpCode.Code == Mono.Cecil.Cil.Code.Call)
            {
                calcCount++;
                if (calcCount == 2)
                {
                    for (int j = i + 1; j < Math.Min(i + 12, ins.Count); j++)
                        if (ins[j].OpCode.Code == Mono.Cecil.Cil.Code.Bge_S || ins[j].OpCode.Code == Mono.Cecil.Cil.Code.Bge)
                        { loopbr = j; break; }
                    break;
                }
            }
        }
        if (loopbr < 0) throw new Exception("QuickUpgrade: 未找到循环回边 bge");
        var loopbrIns = ins[loopbr];
        var exitIns = ins[loopbr + 1];
        // 回边处栈上残留 [soul.r8, num.r8]（原 bge.s 消费），提前跳出必须补 pop 平衡栈
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "skill_free"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, loopbrIns),
            Instruction.Create(OpCodes.Pop),
            Instruction.Create(OpCodes.Pop),
            Instruction.Create(OpCodes.Br, exitIns),
        };
        InsertAt(m, loopbr, seq);
    }
}
