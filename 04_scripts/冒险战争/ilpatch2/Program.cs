using Mono.Cecil;
using Mono.Cecil.Cil;

// 冒险战争 (mxzz) IL 补丁器 v2 —— 挂点 ClientLife::set_HP(System.Int64)
// 静态依据见 csproj 注释；本程序只做「读配置 → 改 value → 落回原方法体」

if (args.Length < 2) { Console.Error.WriteLine("用法: mxzz_clpatch <src.dll> <dst.dll>"); return 2; }
string src = args[0], dst = args[1];
const string CfgPath = "/data/user/0/com.mxzz.zbml/files/starry_config.ini";

var refDirs = new List<string> {
    Path.GetDirectoryName(Path.GetFullPath(src)),
    Path.GetDirectoryName(Path.GetFullPath(dst)),
    @"E:\nixiang\13_scratch\冒险战争\dll"
}.Where(d => !string.IsNullOrEmpty(d) && Directory.Exists(d)).Distinct().ToList();
var resolver = new TolerantAssemblyResolver();
foreach (var d in refDirs) { resolver.AddSearchDirectory(d); Console.WriteLine("[ref] " + d); }
var metaResolver = new TolerantMetadataResolver(resolver);
var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { AssemblyResolver = resolver, MetadataResolver = metaResolver, ReadSymbols = false });
var mod = asm.MainModule;
var intT = mod.TypeSystem.Int32;
var longT = mod.TypeSystem.Int64;
var strT = mod.TypeSystem.String;
var boolT = mod.TypeSystem.Boolean;
var voidT = mod.TypeSystem.Void;
var objT = mod.TypeSystem.Object;
var corlib = mod.TypeSystem.CoreLibrary;

var cl = mod.GetType("ClientLife") ?? mod.GetTypes().FirstOrDefault(x => x.Name == "ClientLife");
if (cl == null) { Console.Error.WriteLine("[err] 类型 ClientLife 未找到"); return 3; }
var setHp = cl.Methods.FirstOrDefault(m => m.Name == "set_HP" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Int64");
if (setHp == null) { Console.Error.WriteLine("[err] ClientLife::set_HP(Int64) 未找到"); return 3; }
var curHp = cl.Fields.FirstOrDefault(f => f.Name == "_curHp");
if (curHp == null || curHp.FieldType.FullName != "System.Int64") { Console.Error.WriteLine("[err] _curHp 不是 Int64"); return 3; }
var campF = cl.Fields.FirstOrDefault(f => f.Name == "campType");
if (campF == null) { Console.Error.WriteLine("[err] campType 未找到"); return 3; }
int campEnum = 0;
try { var campTd = campF.FieldType.Resolve(); if (campTd != null && campTd.HasFields) campEnum = (int)campTd.Fields.First(f => f.Name == "ENEMY_CAMP").Constant; } catch { }

Console.WriteLine("[map] ClientLife::set_HP 参数=" + setHp.Parameters[0].ParameterType.FullName + "  原 IL=" + setHp.Body.Instructions.Count + "  _curHp=" + curHp.FieldType.FullName + "  campType=" + campF.FieldType.Name);

// ---- 工具类型 Starry.MxzzPatch ----
var helper = new TypeDefinition("Starry", "MxzzPatch", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit, objT);
mod.Types.Add(helper);
FieldDefinition MkField(string n) { var f = new FieldDefinition(n, FieldAttributes.Private | FieldAttributes.Static, intT); helper.Fields.Add(f); return f; }
var fGod = MkField("s_god");
var fKill = MkField("s_kill");
var fTick = MkField("s_tick");
var fHit = MkField("s_hit");
var fCBtl = MkField("s_cBtl");
var fCFrame = MkField("s_cFrame");
var fCAdd = MkField("s_cAdd");
var fGodHit = MkField("s_godHit");
var fKillHit = MkField("s_killHit");
var fLastCamp = MkField("s_lastCamp");
var fLastCur = MkField("s_lastCur");
var fLastVal = MkField("s_lastVal");

var fileT = new TypeReference("System.IO", "File", mod, corlib);
var mReadAll = new MethodReference("ReadAllText", strT, fileT) { HasThis = false };
mReadAll.Parameters.Add(new ParameterDefinition(strT));
var mContains = new MethodReference("Contains", boolT, strT) { HasThis = true };
mContains.Parameters.Add(new ParameterDefinition(strT));
var envT = new TypeReference("System", "Environment", mod, corlib);
var mTick = new MethodReference("get_TickCount", intT, envT) { HasThis = false };
var exT = new TypeReference("System", "Exception", mod, corlib);

// ---- static void Refresh() ----
var refresh = new MethodDefinition("Refresh", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, voidT);
helper.Methods.Add(refresh);
{
    var b = refresh.Body; var p = b.GetILProcessor();
    b.InitLocals = true; b.MaxStackSize = 4;
    var vN = new VariableDefinition(intT); b.Variables.Add(vN);
    var vT = new VariableDefinition(strT); b.Variables.Add(vT);
    var iEnd = p.Create(OpCodes.Ret);
    var iGo = p.Create(OpCodes.Nop);
    var iL1 = p.Create(OpCodes.Nop);
    var iL2 = p.Create(OpCodes.Nop);
    var tryStart = p.Create(OpCodes.Call, mTick); p.Append(tryStart);
    p.Append(p.Create(OpCodes.Stloc, vN));
    p.Append(p.Create(OpCodes.Ldloc, vN));
    p.Append(p.Create(OpCodes.Ldsfld, fTick));
    p.Append(p.Create(OpCodes.Sub));
    p.Append(p.Create(OpCodes.Ldc_I4, 1000));
    p.Append(p.Create(OpCodes.Bge, iGo));       // n - tick >= 1000 ⇒ 继续读取
    p.Append(p.Create(OpCodes.Leave, iEnd));    // 否则沿用旧值（不可用 blt 跨 EH 边界）
    p.Append(iGo);
    p.Append(p.Create(OpCodes.Ldloc, vN));
    p.Append(p.Create(OpCodes.Stsfld, fTick));
    p.Append(p.Create(OpCodes.Ldc_I4_0)); p.Append(p.Create(OpCodes.Stsfld, fGod));
    p.Append(p.Create(OpCodes.Ldc_I4_0)); p.Append(p.Create(OpCodes.Stsfld, fKill));
    p.Append(p.Create(OpCodes.Ldstr, CfgPath));
    p.Append(p.Create(OpCodes.Call, mReadAll));
    p.Append(p.Create(OpCodes.Stloc, vT));
    p.Append(p.Create(OpCodes.Ldloc, vT));
    p.Append(p.Create(OpCodes.Ldstr, "god=1"));
    p.Append(p.Create(OpCodes.Callvirt, mContains));
    p.Append(p.Create(OpCodes.Brfalse, iL1));
    p.Append(p.Create(OpCodes.Ldc_I4_1)); p.Append(p.Create(OpCodes.Stsfld, fGod));
    p.Append(iL1);
    p.Append(p.Create(OpCodes.Ldloc, vT));
    p.Append(p.Create(OpCodes.Ldstr, "kill=1"));
    p.Append(p.Create(OpCodes.Callvirt, mContains));
    p.Append(p.Create(OpCodes.Brfalse, iL2));
    p.Append(p.Create(OpCodes.Ldc_I4_1)); p.Append(p.Create(OpCodes.Stsfld, fKill));
    p.Append(iL2);
    p.Append(p.Create(OpCodes.Leave, iEnd));
    var tryEnd = p.Create(OpCodes.Nop); p.Append(tryEnd);
    var hStart = p.Create(OpCodes.Pop); p.Append(hStart);
    p.Append(p.Create(OpCodes.Leave, iEnd));
    var hEnd = p.Create(OpCodes.Nop); p.Append(hEnd);
    p.Append(iEnd);
    b.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch) { CatchType = exT, TryStart = tryStart, TryEnd = tryEnd, HandlerStart = hStart, HandlerEnd = hEnd });
}

// ---- static bool AdjustValue(ClientLife self, long value, out long adjusted) ----
var adjust = new MethodDefinition("AdjustValue", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, boolT);
adjust.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, cl));
adjust.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, longT));
adjust.Parameters.Add(new ParameterDefinition("adjusted", ParameterAttributes.Out, new ByReferenceType(longT)));
helper.Methods.Add(adjust);
{
    var b = adjust.Body; var p = b.GetILProcessor();
    b.InitLocals = true; b.MaxStackSize = 4;
    var vCur = new VariableDefinition(longT); b.Variables.Add(vCur);
    var vRet = new VariableDefinition(boolT); b.Variables.Add(vRet);
    var iEnd = p.Create(OpCodes.Ldloc, vRet);
    var iFalse = p.Create(OpCodes.Ldc_I4_0);
    var iGod = p.Create(OpCodes.Nop);
    var iKill = p.Create(OpCodes.Nop);
    var tryStart = p.Create(OpCodes.Call, refresh); p.Append(tryStart);
    p.Append(p.Create(OpCodes.Ldarg_2)); p.Append(p.Create(OpCodes.Ldarg_1)); p.Append(p.Create(OpCodes.Stind_I8));  // adjusted = value
    p.Append(p.Create(OpCodes.Ldsfld, fGod));
    p.Append(p.Create(OpCodes.Brtrue, iGod));
    p.Append(p.Create(OpCodes.Ldsfld, fKill));
    p.Append(p.Create(OpCodes.Brfalse, iFalse));
    p.Append(iGod);
    p.Append(p.Create(OpCodes.Ldarg_0));
    p.Append(p.Create(OpCodes.Brfalse, iFalse));
    p.Append(p.Create(OpCodes.Ldarg_0)); p.Append(p.Create(OpCodes.Ldfld, curHp)); p.Append(p.Create(OpCodes.Stloc, vCur));
    p.Append(p.Create(OpCodes.Ldarg_1)); p.Append(p.Create(OpCodes.Ldloc, vCur));
    p.Append(p.Create(OpCodes.Bge, iFalse));     // value >= cur ⇒ 不是掉血 ⇒ 不处理
    // 无敌：camp == FRIEND_CAMP(1)
    p.Append(p.Create(OpCodes.Ldsfld, fGod));
    p.Append(p.Create(OpCodes.Brfalse, iKill));
    p.Append(p.Create(OpCodes.Ldarg_0)); p.Append(p.Create(OpCodes.Ldfld, campF)); p.Append(p.Create(OpCodes.Conv_I4));
    p.Append(p.Create(OpCodes.Ldc_I4_1));
    p.Append(p.Create(OpCodes.Bne_Un, iKill));
    p.Append(p.Create(OpCodes.Ldarg_2)); p.Append(p.Create(OpCodes.Ldloc, vCur)); p.Append(p.Create(OpCodes.Stind_I8));
    p.Append(p.Create(OpCodes.Ldc_I4_1)); p.Append(p.Create(OpCodes.Stloc, vRet));
    p.Append(p.Create(OpCodes.Leave, iEnd));
    p.Append(iKill);
    // 秒杀：camp == ENEMY_CAMP(0)
    p.Append(p.Create(OpCodes.Ldsfld, fKill));
    p.Append(p.Create(OpCodes.Brfalse, iFalse));
    p.Append(p.Create(OpCodes.Ldarg_0)); p.Append(p.Create(OpCodes.Ldfld, campF)); p.Append(p.Create(OpCodes.Conv_I4));
    p.Append(p.Create(OpCodes.Ldc_I4, campEnum));
    p.Append(p.Create(OpCodes.Bne_Un, iFalse));
    p.Append(p.Create(OpCodes.Ldarg_2)); p.Append(p.Create(OpCodes.Ldc_I4_0)); p.Append(p.Create(OpCodes.Conv_I8)); p.Append(p.Create(OpCodes.Stind_I8));
    p.Append(p.Create(OpCodes.Ldc_I4_1)); p.Append(p.Create(OpCodes.Stloc, vRet));
    p.Append(p.Create(OpCodes.Leave, iEnd));
    p.Append(iFalse);
    p.Append(p.Create(OpCodes.Stloc, vRet));
    p.Append(p.Create(OpCodes.Leave, iEnd));
    var tryEnd = p.Create(OpCodes.Nop); p.Append(tryEnd);
    var hStart = p.Create(OpCodes.Pop); p.Append(hStart);
    p.Append(p.Create(OpCodes.Ldc_I4_0)); p.Append(p.Create(OpCodes.Stloc, vRet));
    p.Append(p.Create(OpCodes.Leave, iEnd));
    var hEnd = p.Create(OpCodes.Nop); p.Append(hEnd);
    p.Append(iEnd);
    p.Append(p.Create(OpCodes.Ret));
    b.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch) { CatchType = exT, TryStart = tryStart, TryEnd = tryEnd, HandlerStart = hStart, HandlerEnd = hEnd });
}

// ---- 记录层：LogicRecord_HPValue::PerformDoRecord 前置改写 hp ----
//   静态依据：PerformDoRecord IL@0016 ldfld hp → IL@001B ClientLife::set_HP，随后用同一个 hp 算 HPPercent
//   ⇒ 只改 set_HP 会造成「血被保住但血条按记录值掉」；必须在记录层把 hp 改掉。
var lrT = mod.GetTypes().FirstOrDefault(x => x.Name == "LogicRecord_HPValue");
var doRec = lrT?.Methods.FirstOrDefault(m => m.Name == "PerformDoRecord");
var recHp = lrT?.Fields.FirstOrDefault(f => f.Name == "hp");
var recHero = lrT?.BaseType?.Resolve()?.Fields.FirstOrDefault(f => f.Name == "Hero");
var getHp = cl.Methods.FirstOrDefault(m => m.Name == "get_HP" && m.Parameters.Count == 0);
if (doRec == null || recHp == null || recHero == null || getHp == null) { Console.Error.WriteLine("[err] 记录层静态前提不满足（PerformDoRecord/hp/Hero/get_HP）"); return 3; }
Console.WriteLine("[map] LogicRecord_HPValue::PerformDoRecord 原 IL=" + doRec.Body.Instructions.Count + "  hp=" + recHp.FieldType.Name + "  Hero=" + recHero.FieldType.Name);
var fixRec = new MethodDefinition("FixRecord", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, voidT);
fixRec.Parameters.Add(new ParameterDefinition("rec", ParameterAttributes.None, lrT));
helper.Methods.Add(fixRec);
{
    var b = fixRec.Body; var p2 = b.GetILProcessor();
    b.InitLocals = true; b.MaxStackSize = 4;
    var vHero = new VariableDefinition(cl); b.Variables.Add(vHero);
    var vCur = new VariableDefinition(longT); b.Variables.Add(vCur);
    var vHp = new VariableDefinition(longT); b.Variables.Add(vHp);
    var vCamp = new VariableDefinition(intT); b.Variables.Add(vCamp);
    var iEnd = p2.Create(OpCodes.Ret);
    var iL1 = p2.Create(OpCodes.Nop);
    var iL2 = p2.Create(OpCodes.Nop);
    var iKill = p2.Create(OpCodes.Nop);
    var tryStart = p2.Create(OpCodes.Call, refresh); p2.Append(tryStart);
    p2.Append(p2.Create(OpCodes.Ldarg_0)); p2.Append(p2.Create(OpCodes.Brtrue, iL1)); p2.Append(p2.Create(OpCodes.Leave, iEnd));
    p2.Append(iL1);
    p2.Append(p2.Create(OpCodes.Ldarg_0)); p2.Append(p2.Create(OpCodes.Ldfld, recHero)); p2.Append(p2.Create(OpCodes.Stloc, vHero));
    p2.Append(p2.Create(OpCodes.Ldloc, vHero)); p2.Append(p2.Create(OpCodes.Brtrue, iL2)); p2.Append(p2.Create(OpCodes.Leave, iEnd));
    p2.Append(iL2);
    p2.Append(p2.Create(OpCodes.Ldloc, vHero)); p2.Append(p2.Create(OpCodes.Callvirt, getHp)); p2.Append(p2.Create(OpCodes.Stloc, vCur));
    p2.Append(p2.Create(OpCodes.Ldarg_0)); p2.Append(p2.Create(OpCodes.Ldfld, recHp)); p2.Append(p2.Create(OpCodes.Stloc, vHp));
    p2.Append(p2.Create(OpCodes.Ldloc, vHero)); p2.Append(p2.Create(OpCodes.Ldfld, campF)); p2.Append(p2.Create(OpCodes.Conv_I4)); p2.Append(p2.Create(OpCodes.Stloc, vCamp));
    p2.Append(p2.Create(OpCodes.Ldloc, vCamp)); p2.Append(p2.Create(OpCodes.Stsfld, fLastCamp));
    p2.Append(p2.Create(OpCodes.Ldloc, vCur)); p2.Append(p2.Create(OpCodes.Conv_I4)); p2.Append(p2.Create(OpCodes.Stsfld, fLastCur));
    p2.Append(p2.Create(OpCodes.Ldloc, vHp)); p2.Append(p2.Create(OpCodes.Conv_I4)); p2.Append(p2.Create(OpCodes.Stsfld, fLastVal));
    p2.Append(p2.Create(OpCodes.Ldloc, vHp)); p2.Append(p2.Create(OpCodes.Ldloc, vCur));
    p2.Append(p2.Create(OpCodes.Bge, iEnd));   // 记录值 >= 当前血 ⇒ 不是掉血
    p2.Append(p2.Create(OpCodes.Ldsfld, fGod));
    p2.Append(p2.Create(OpCodes.Brfalse, iKill));
    p2.Append(p2.Create(OpCodes.Ldloc, vCamp)); p2.Append(p2.Create(OpCodes.Ldc_I4_1)); p2.Append(p2.Create(OpCodes.Bne_Un, iKill));
    p2.Append(p2.Create(OpCodes.Ldarg_0)); p2.Append(p2.Create(OpCodes.Ldloc, vCur)); p2.Append(p2.Create(OpCodes.Stfld, recHp));
    p2.Append(p2.Create(OpCodes.Ldsfld, fGodHit)); p2.Append(p2.Create(OpCodes.Ldc_I4_1)); p2.Append(p2.Create(OpCodes.Add)); p2.Append(p2.Create(OpCodes.Stsfld, fGodHit));
    p2.Append(p2.Create(OpCodes.Leave, iEnd));
    p2.Append(iKill);
    p2.Append(p2.Create(OpCodes.Ldsfld, fKill));
    p2.Append(p2.Create(OpCodes.Brfalse, iEnd));
    p2.Append(p2.Create(OpCodes.Ldloc, vCamp)); p2.Append(p2.Create(OpCodes.Ldc_I4, campEnum)); p2.Append(p2.Create(OpCodes.Bne_Un, iEnd));
    p2.Append(p2.Create(OpCodes.Ldarg_0)); p2.Append(p2.Create(OpCodes.Ldc_I4_0)); p2.Append(p2.Create(OpCodes.Conv_I8)); p2.Append(p2.Create(OpCodes.Stfld, recHp));
    p2.Append(p2.Create(OpCodes.Ldsfld, fKillHit)); p2.Append(p2.Create(OpCodes.Ldc_I4_1)); p2.Append(p2.Create(OpCodes.Add)); p2.Append(p2.Create(OpCodes.Stsfld, fKillHit));
    p2.Append(p2.Create(OpCodes.Leave, iEnd));
    var tryEnd = p2.Create(OpCodes.Nop); p2.Append(tryEnd);
    var hStart = p2.Create(OpCodes.Pop); p2.Append(hStart);
    p2.Append(p2.Create(OpCodes.Leave, iEnd));
    var hEnd = p2.Create(OpCodes.Nop); p2.Append(hEnd);
    p2.Append(iEnd);
    b.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch) { CatchType = exT, TryStart = tryStart, TryEnd = tryEnd, HandlerStart = hStart, HandlerEnd = hEnd });
}
int recOrig = doRec.Body.Instructions.Count;
var recFirst = doRec.Body.Instructions[0];
if (doRec.Body.MaxStackSize < 4) doRec.Body.MaxStackSize = 4;
var ril = doRec.Body.GetILProcessor();
ril.InsertBefore(recFirst, ril.Create(OpCodes.Ldarg_0));
ril.InsertBefore(recFirst, ril.Create(OpCodes.Call, fixRec));

// ---- 死亡拦截：无敌时友方不得进入死亡流程 ----
//   静态依据：死亡可见链路 = LogicRecord_Dead::PerformDoRecord → ClientLife::set_LogicDead / ClientLife::DoDead
//   ⇒ 只保住 HP 不够：血满仍会「演死」，必须同时拦住 set_LogicDead(true) 与 DoDead()。
var setLogicDead = cl.Methods.FirstOrDefault(m => m.Name == "set_LogicDead" && m.Parameters.Count == 1);
var doDeadM = cl.Methods.FirstOrDefault(m => m.Name == "DoDead" && m.Parameters.Count == 0);
if (setLogicDead == null || doDeadM == null) { Console.Error.WriteLine("[err] 死亡方法未找到（set_LogicDead / DoDead）"); return 3; }
var blockM = new MethodDefinition("BlockDeath", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, boolT);
blockM.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, cl));
helper.Methods.Add(blockM);
{
    var bb = blockM.Body; var pb = bb.GetILProcessor();
    bb.InitLocals = true; bb.MaxStackSize = 2;
    var vR = new VariableDefinition(boolT); bb.Variables.Add(vR);
    var iEnd = pb.Create(OpCodes.Ldloc, vR);
    var tryStart = pb.Create(OpCodes.Call, refresh); pb.Append(tryStart);
    pb.Append(pb.Create(OpCodes.Ldc_I4_0)); pb.Append(pb.Create(OpCodes.Stloc, vR));
    pb.Append(pb.Create(OpCodes.Ldsfld, fGod)); pb.Append(pb.Create(OpCodes.Brfalse, iEnd));
    pb.Append(pb.Create(OpCodes.Ldarg_0)); pb.Append(pb.Create(OpCodes.Brfalse, iEnd));
    pb.Append(pb.Create(OpCodes.Ldarg_0)); pb.Append(pb.Create(OpCodes.Ldfld, campF)); pb.Append(pb.Create(OpCodes.Conv_I4));
    pb.Append(pb.Create(OpCodes.Ldc_I4_1)); pb.Append(pb.Create(OpCodes.Bne_Un, iEnd));
    pb.Append(pb.Create(OpCodes.Ldc_I4_1)); pb.Append(pb.Create(OpCodes.Stloc, vR));
    pb.Append(pb.Create(OpCodes.Leave, iEnd));
    var tryEnd = pb.Create(OpCodes.Nop); pb.Append(tryEnd);
    var hStart = pb.Create(OpCodes.Pop); pb.Append(hStart);
    pb.Append(pb.Create(OpCodes.Ldc_I4_0)); pb.Append(pb.Create(OpCodes.Stloc, vR));
    pb.Append(pb.Create(OpCodes.Leave, iEnd));
    var hEnd = pb.Create(OpCodes.Nop); pb.Append(hEnd);
    pb.Append(iEnd);
    pb.Append(pb.Create(OpCodes.Ret));
    bb.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch) { CatchType = exT, TryStart = tryStart, TryEnd = tryEnd, HandlerStart = hStart, HandlerEnd = hEnd });
}
// DoDead()：无敌友方直接返回
{
    var first = doDeadM.Body.Instructions[0];
    var di = doDeadM.Body.GetILProcessor();
    di.InsertBefore(first, di.Create(OpCodes.Ldarg_0));
    di.InsertBefore(first, di.Create(OpCodes.Call, blockM));
    di.InsertBefore(first, di.Create(OpCodes.Brfalse, first));
    di.InsertBefore(first, di.Create(OpCodes.Ret));
    if (doDeadM.Body.MaxStackSize < 4) doDeadM.Body.MaxStackSize = 4;
}
// set_LogicDead(bool)：无敌友方禁止置 true
{
    var first = setLogicDead.Body.Instructions[0];
    var si = setLogicDead.Body.GetILProcessor();
    si.InsertBefore(first, si.Create(OpCodes.Ldarg_1));
    si.InsertBefore(first, si.Create(OpCodes.Brfalse, first));
    si.InsertBefore(first, si.Create(OpCodes.Ldarg_0));
    si.InsertBefore(first, si.Create(OpCodes.Call, blockM));
    si.InsertBefore(first, si.Create(OpCodes.Brfalse, first));
    si.InsertBefore(first, si.Create(OpCodes.Ret));
    if (setLogicDead.Body.MaxStackSize < 4) setLogicDead.Body.MaxStackSize = 4;
}
// ---- 死亡表现与死亡记录拦截（b15）----
//   静态依据：
//     ClientLife::Update            → PlayDeadperform() / DoDead()
//     ClientLife::PlayDeadperform   → ToDissover()（溶解消失）
//     ToDissover / ToFloor / LifeDead / ImmediatelyLifeDead → RealDead = true
//     LogicRecord_Dead::PerformDoRecord → set_LogicDead(true) + set_Killer + 事件派发
//   ⇒ b14 只拦了 DoDead / set_LogicDead，仍可由 PlayDeadperform→ToDissover 或
//     LogicRecord_Dead 直接走到「消失/死亡」，必须一并拦住。
var toDissover = cl.Methods.FirstOrDefault(m => m.Name == "ToDissover" && m.Parameters.Count == 0);
var playDead = cl.Methods.FirstOrDefault(m => m.Name == "PlayDeadperform" && m.Parameters.Count == 0);
var deadRecT = mod.GetTypes().FirstOrDefault(x => x.Name == "LogicRecord_Dead");
var deadDoRec = deadRecT?.Methods.FirstOrDefault(m => m.Name == "PerformDoRecord");
var deadRecHero = deadRecT?.BaseType?.Resolve()?.Fields.FirstOrDefault(f => f.Name == "Hero");
if (deadRecT == null || deadRecHero == null || toDissover == null || playDead == null) { Console.Error.WriteLine("[err] 死亡链静态前提缺失（LogicRecord_Dead/Hero/ToDissover/PlayDeadperform）"); return 3; }
// 记录级判定：入参为 LogicRecord_Dead
var blockRec = new MethodDefinition("BlockDeathRecord", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, boolT);
blockRec.Parameters.Add(new ParameterDefinition("rec", ParameterAttributes.None, deadRecT));
helper.Methods.Add(blockRec);
{
    var bb = blockRec.Body; var pb = bb.GetILProcessor();
    bb.InitLocals = true; bb.MaxStackSize = 2;
    var vR = new VariableDefinition(boolT); bb.Variables.Add(vR);
    var vH = new VariableDefinition(cl); bb.Variables.Add(vH);
    var iEnd = pb.Create(OpCodes.Ldloc, vR);
    var tryStart = pb.Create(OpCodes.Call, refresh); pb.Append(tryStart);
    pb.Append(pb.Create(OpCodes.Ldc_I4_0)); pb.Append(pb.Create(OpCodes.Stloc, vR));
    pb.Append(pb.Create(OpCodes.Ldsfld, fGod)); pb.Append(pb.Create(OpCodes.Brfalse, iEnd));
    pb.Append(pb.Create(OpCodes.Ldarg_0)); pb.Append(pb.Create(OpCodes.Brfalse, iEnd));
    pb.Append(pb.Create(OpCodes.Ldarg_0)); pb.Append(pb.Create(OpCodes.Ldfld, deadRecHero)); pb.Append(pb.Create(OpCodes.Stloc, vH));
    pb.Append(pb.Create(OpCodes.Ldloc, vH)); pb.Append(pb.Create(OpCodes.Brfalse, iEnd));
    pb.Append(pb.Create(OpCodes.Ldloc, vH)); pb.Append(pb.Create(OpCodes.Ldfld, campF)); pb.Append(pb.Create(OpCodes.Conv_I4));
    pb.Append(pb.Create(OpCodes.Ldc_I4_1)); pb.Append(pb.Create(OpCodes.Bne_Un, iEnd));
    pb.Append(pb.Create(OpCodes.Ldc_I4_1)); pb.Append(pb.Create(OpCodes.Stloc, vR));
    pb.Append(pb.Create(OpCodes.Leave, iEnd));
    var tryEnd = pb.Create(OpCodes.Nop); pb.Append(tryEnd);
    var hStart = pb.Create(OpCodes.Pop); pb.Append(hStart);
    pb.Append(pb.Create(OpCodes.Ldc_I4_0)); pb.Append(pb.Create(OpCodes.Stloc, vR));
    pb.Append(pb.Create(OpCodes.Leave, iEnd));
    var hEnd = pb.Create(OpCodes.Nop); pb.Append(hEnd);
    pb.Append(iEnd); pb.Append(pb.Create(OpCodes.Ret));
    bb.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch) { CatchType = exT, TryStart = tryStart, TryEnd = tryEnd, HandlerStart = hStart, HandlerEnd = hEnd });
}
// 记录层：无敌时友方死亡记录直接跳过
if (deadDoRec != null && deadRecHero != null) {
    var first = deadDoRec.Body.Instructions[0];
    var di = deadDoRec.Body.GetILProcessor();
    di.InsertBefore(first, di.Create(OpCodes.Ldarg_0));
    di.InsertBefore(first, di.Create(OpCodes.Call, blockRec));
    di.InsertBefore(first, di.Create(OpCodes.Brfalse, first));
    di.InsertBefore(first, di.Create(OpCodes.Ret));
    if (deadDoRec.Body.MaxStackSize < 4) deadDoRec.Body.MaxStackSize = 4;
    Console.WriteLine("[map] LogicRecord_Dead::PerformDoRecord 已装拦截，IL→" + deadDoRec.Body.Instructions.Count);
} else Console.WriteLine("[warn] LogicRecord_Dead 静态前提缺失，跳过记录层拦截");
// 表现层：无敌时友方不播死亡/不溶解
foreach (var mm in new[] { toDissover, playDead }) {
    if (mm == null) { Console.WriteLine("[warn] 缺少 ToDissover/PlayDeadperform"); continue; }
    var first = mm.Body.Instructions[0];
    var di = mm.Body.GetILProcessor();
    di.InsertBefore(first, di.Create(OpCodes.Ldarg_0));
    di.InsertBefore(first, di.Create(OpCodes.Call, blockM));
    di.InsertBefore(first, di.Create(OpCodes.Brfalse, first));
    di.InsertBefore(first, di.Create(OpCodes.Ret));
    if (mm.Body.MaxStackSize < 4) mm.Body.MaxStackSize = 4;
    Console.WriteLine("[map] " + mm.Name + " 已装拦截，IL→" + mm.Body.Instructions.Count);
}
Console.WriteLine("[map] 死亡拦截已装：DoDead IL " + doDeadM.Body.Instructions.Count + " 条，set_LogicDead IL " + setLogicDead.Body.Instructions.Count + " 条");

// ---- b16 逻辑层探针（只加计数，不改语义）----
//   静态依据：LogicBattle::CreateBattle ← ClientRealtimeComputing / ImitateBattleRun / BattleEditor…
//             LogicBattle::FixedUpdate ← Game::FixedUpdate（逐帧推进逻辑战斗）
//             Battle.Life::AddHP ← SkillFunc::AppendDamage / BuffActionData::* / Life::ReceiveDamage
//   目的：确认「客户端是否真的在本地跑逻辑战斗、并走到 AddHP」。
void Count(MethodDefinition mm, FieldDefinition f, string tag, int expectMin)
{
    if (mm == null) { Console.WriteLine("[warn] 未找到 " + tag); return; }
    var first = mm.Body.Instructions[0];
    var ci = mm.Body.GetILProcessor();
    ci.InsertBefore(first, ci.Create(OpCodes.Ldsfld, f));
    ci.InsertBefore(first, ci.Create(OpCodes.Ldc_I4_1));
    ci.InsertBefore(first, ci.Create(OpCodes.Add));
    ci.InsertBefore(first, ci.Create(OpCodes.Stsfld, f));
    if (mm.Body.MaxStackSize < 8) mm.Body.MaxStackSize = 8;
    Console.WriteLine("[map] 计数已装 " + tag + " → IL " + mm.Body.Instructions.Count);
}
var logicBattleT = mod.GetTypes().FirstOrDefault(x => x.Name == "LogicBattle");
var lifeT = mod.GetTypes().FirstOrDefault(x => x.Name == "Life" && x.Namespace == "Battle");
Count(logicBattleT?.Methods.FirstOrDefault(x => x.Name == "CreateBattle"), fCBtl, "LogicBattle::CreateBattle", 1);
Count(logicBattleT?.Methods.FirstOrDefault(x => x.Name == "FixedUpdate"), fCFrame, "LogicBattle::FixedUpdate", 1);
Count(logicBattleT?.Methods.FirstOrDefault(x => x.Name == "Update" && x.Parameters.Count == 0), fCFrame, "LogicBattle::Update", 1);
var addHpM = lifeT?.Methods.FirstOrDefault(x => x.Name == "AddHP" && x.Parameters.Count == 3);
Count(addHpM, fCAdd, "Battle.Life::AddHP", 0);

// ---- set_HP 前缀 ----
int origCount = setHp.Body.Instructions.Count;
var origFirst = setHp.Body.Instructions[0];
setHp.Body.InitLocals = true;
var vAdj = new VariableDefinition(longT);
setHp.Body.Variables.Add(vAdj);
if (setHp.Body.MaxStackSize < 8) setHp.Body.MaxStackSize = 8;
var il = setHp.Body.GetILProcessor();
il.InsertBefore(origFirst, il.Create(OpCodes.Ldsfld, fHit));
il.InsertBefore(origFirst, il.Create(OpCodes.Ldc_I4_1));
il.InsertBefore(origFirst, il.Create(OpCodes.Add));
il.InsertBefore(origFirst, il.Create(OpCodes.Stsfld, fHit));
il.InsertBefore(origFirst, il.Create(OpCodes.Ldarg_0));
il.InsertBefore(origFirst, il.Create(OpCodes.Ldarg_1));
il.InsertBefore(origFirst, il.Create(OpCodes.Ldloca, vAdj));
il.InsertBefore(origFirst, il.Create(OpCodes.Call, adjust));
il.InsertBefore(origFirst, il.Create(OpCodes.Brfalse, origFirst));
il.InsertBefore(origFirst, il.Create(OpCodes.Ldloc, vAdj));
il.InsertBefore(origFirst, il.Create(OpCodes.Starg, setHp.Parameters[0]));

// ---- 自检 ----
var ins = setHp.Body.Instructions;
int bad = 0;
void A(string name, bool ok, string msg) { Console.WriteLine((ok ? "[assert] OK   " : "[assert] FAIL ") + name + ": " + msg); if (!ok) bad++; }
A("前缀条数", ins.Count == origCount + 11, "IL 总数=" + ins.Count + "（期望 " + origCount + "+11=" + (origCount + 11) + "）");
A("命中计数", ins[3].OpCode == OpCodes.Stsfld && ((FieldReference)ins[3].Operand).Name == "s_hit", "ins[3]=stsfld s_hit");
A("调用判定", ins[7].OpCode == OpCodes.Call && ((MethodReference)ins[7].Operand).Name == "AdjustValue", "ins[7]=AdjustValue");
A("条件分支", ins[8].OpCode == OpCodes.Brfalse && (Instruction)ins[8].Operand == origFirst, "ins[8]=brfalse → 原首指令");
A("starg 落点", ins[10].OpCode == OpCodes.Starg && ((ParameterDefinition)ins[10].Operand).Index == 0, "ins[10]=starg Parameters[0]=value");
A("原体首条", ins[11].OpCode == OpCodes.Ldarg_0, "ins[11]=" + ins[11].OpCode.Name);
A("记录层前缀", doRec.Body.Instructions.Count == recOrig + 2 && doRec.Body.Instructions[0].OpCode == OpCodes.Ldarg_0, "PerformDoRecord " + recOrig + "→" + doRec.Body.Instructions.Count);
if (bad > 0) { Console.Error.WriteLine("[gate] FAIL " + bad + " 项，禁止出包"); return 1; }

var needs = ShimFactory.Discover(mod, refDirs);
var shimDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(src)), "refs_shim");
var nShim = ShimFactory.Materialize(shimDir, needs);
if (nShim > 0) { resolver.AddSearchDirectory(shimDir); Console.WriteLine("[shim] " + nShim + " 个垫片程序集 → " + shimDir); }
var constantsBefore = ConstantAudit.Snapshot(mod);
asm.Write(dst, new WriterParameters { WriteSymbols = false });
Console.WriteLine("[out] 已写出 " + dst + "  " + new FileInfo(dst).Length + " B");
Console.WriteLine("[gate] PASS —— set_HP 前缀 " + ins.Count + " 条（" + origCount + "+11），ENEMY_CAMP=" + campEnum + "，FRIEND_CAMP=1");
return 0;
