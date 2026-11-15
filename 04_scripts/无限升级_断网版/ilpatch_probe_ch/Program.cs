// 称号(skin/clothes) 诊断探针
//  ① ClothesIdHelper::GetSpriteKey(AttClothes) 入口：打印 每个称号的 NumberIndex/Id/IconIdFix/IsHide
//  ② GameData::OnChangeCloth 入口：打印 当前穿戴 currentClothId / currentCloth / currentCloth_onlyView / hounerLevel
using Mono.Cecil;
using Mono.Cecil.Cil;

var src = Environment.GetEnvironmentVariable("WXSJ_SRC");
var dst = Environment.GetEnvironmentVariable("WXSJ_DST");
if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dst)) { Console.Error.WriteLine("need WXSJ_SRC/WXSJ_DST"); return 2; }

var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { ReadSymbols = false });
var mod = asm.MainModule;
var core = mod.AssemblyReferences.First(r => r.Name == "UnityEngine.CoreModule");
var tDebug = new TypeReference("UnityEngine", "Debug", mod, core);
var mLog = new MethodReference("Log", mod.TypeSystem.Void, tDebug) { HasThis = false };
mLog.Parameters.Add(new ParameterDefinition(mod.TypeSystem.Object));
var mConcatSO = mod.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(object) }));
var mConcatSS = mod.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));

// ---------- ① GetSpriteKey(AttClothes) ----------
var helper = mod.GetType("ClothesSpriteHelper");
var gsk = helper.Methods.First(m => m.Name == "GetSpriteKey" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "AttClothes");
{
    var il = gsk.Body.GetILProcessor();
    var first = gsk.Body.Instructions[0];
    void B(Instruction i) => il.InsertBefore(first, i);
    void Str(string s) => B(il.Create(OpCodes.Ldstr, s));
    void CatSO() => B(il.Create(OpCodes.Call, mConcatSO));
    void CatSS() => B(il.Create(OpCodes.Call, mConcatSS));

    Str("CHKEY n=");
    B(il.Create(OpCodes.Ldarg_0));
    B(il.Create(OpCodes.Callvirt, mod.ImportReference(gsk.Module.GetType("cfg.MyTable.AttClothes").Properties.First(p => p.Name == "NumberIndex").GetMethod)));
    B(il.Create(OpCodes.Box, mod.TypeSystem.Int16));
    CatSO();
    Str(" id=");
    CatSS();
    B(il.Create(OpCodes.Ldarg_0));
    B(il.Create(OpCodes.Callvirt, mod.ImportReference(gsk.Module.GetType("cfg.MyTable.AttClothes").Properties.First(p => p.Name == "Id").GetMethod)));
    CatSS();
    Str(" fix=");
    CatSS();
    B(il.Create(OpCodes.Ldarg_0));
    B(il.Create(OpCodes.Callvirt, mod.ImportReference(gsk.Module.GetType("cfg.MyTable.AttClothes").Properties.First(p => p.Name == "IconIdFix").GetMethod)));
    B(il.Create(OpCodes.Box, mod.TypeSystem.Boolean));
    CatSO();
    B(il.Create(OpCodes.Call, mLog));
    Console.WriteLine("[probe] GetSpriteKey ok");
}

// ---------- ② GameData::OnChangeCloth ----------
var gd = mod.GetType("GameData");
var occ = gd.Methods.First(m => m.Name == "OnChangeCloth" && m.Parameters.Count == 0);
{
    var il = occ.Body.GetILProcessor();
    var first = occ.Body.Instructions[0];
    void B(Instruction i) => il.InsertBefore(first, i);
    void Str(string s) => B(il.Create(OpCodes.Ldstr, s));
    void CatSO() => B(il.Create(OpCodes.Call, mConcatSO));
    void CatSS() => B(il.Create(OpCodes.Call, mConcatSS));

    var fI = gd.Fields.First(f => f.Name == "I");
    var fCurId = gd.Fields.First(f => f.Name == "currentClothId");
    var fCur = gd.Fields.First(f => f.Name == "currentCloth");
    var fOnly = gd.Fields.First(f => f.Name == "currentCloth_onlyView");
    var fHon = gd.Fields.First(f => f.Name == "hounerLevel");

    Str("CHCUR id=");
    B(il.Create(OpCodes.Ldsfld, fI));
    B(il.Create(OpCodes.Ldfld, fCurId));
    B(il.Create(OpCodes.Box, mod.TypeSystem.Int16));
    CatSO();
    Str(" cur=");
    CatSS();
    B(il.Create(OpCodes.Ldsfld, fI));
    B(il.Create(OpCodes.Ldfld, fCur));
    CatSS();
    Str(" only=");
    CatSS();
    B(il.Create(OpCodes.Ldsfld, fI));
    B(il.Create(OpCodes.Ldfld, fOnly));
    CatSS();
    Str(" hon=");
    CatSS();
    B(il.Create(OpCodes.Ldsfld, fI));
    B(il.Create(OpCodes.Ldfld, fHon));
    B(il.Create(OpCodes.Box, mod.TypeSystem.Int32));
    CatSO();
    B(il.Create(OpCodes.Call, mLog));
    Console.WriteLine("[probe] OnChangeCloth ok");
}

asm.Write(dst);
Console.WriteLine("[done] " + dst);
return 0;
