// 称号等级覆盖 v5（wxsjdz1 断网版）
//  A) PlayerGo::ChangeClothes 入口：写 GameData.hounerLevel（开局/换装时应用）
//  B) PlayerGo::RefreshMainSkills：brfalse.s -> pop，强制 angleSign.SetActive(true)
//  C) PlayerGo::FixedUpdate 入口：发现 chenghao_lv 与当前 hounerLevel 不同 ⇒ 写入并调 GameData.OnChangeCloth() 立即重刷
//     只用程序集里已存在的令牌（GameData.OnChangeCloth / MonoSingleton<PlayerGo>.get_Instance / 既有字段），
//     不再新增任何 UnityEngine API 引用（v3 崩在 Component::get_gameObject 这个不存在的令牌上）。
using Mono.Cecil;
using Mono.Cecil.Cil;

var src = Environment.GetEnvironmentVariable("WXSJ_SRC");
var dst = Environment.GetEnvironmentVariable("WXSJ_DST");
if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dst)) { Console.Error.WriteLine("need WXSJ_SRC/WXSJ_DST"); return 2; }

var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { ReadSymbols = false });
var mod = asm.MainModule;

var mGetInt = mod.GetType("Starry.StarryConfig").Methods.First(m => m.Name == "GetInt");
var gd = mod.GetType("GameData");
var fI = gd.Fields.First(f => f.Name == "I");
var fHon = gd.Fields.First(f => f.Name == "hounerLevel");
var mOcc = gd.Methods.First(m => m.Name == "OnChangeCloth" && m.Parameters.Count == 0);
var pm = mod.GetType("PlayerGo");

// ---------------- A) ChangeClothes ----------------
var cc = pm.Methods.First(m => m.Name == "ChangeClothes" && m.Parameters.Count == 1);
{
    var body = cc.Body;
    var il = body.GetILProcessor();
    var first = body.Instructions[0];
    var v = new VariableDefinition(mod.TypeSystem.Int32);
    body.Variables.Add(v);
    var skip = il.Create(OpCodes.Nop);
    void I(Instruction x) => il.InsertBefore(first, x);
    I(il.Create(OpCodes.Ldstr, "chenghao_lv"));
    I(il.Create(OpCodes.Ldc_I4_0));
    I(il.Create(OpCodes.Call, mGetInt));
    I(il.Create(OpCodes.Stloc, v));
    I(il.Create(OpCodes.Ldloc, v));
    I(il.Create(OpCodes.Ldc_I4_0));
    I(il.Create(OpCodes.Ble, skip));
    I(il.Create(OpCodes.Ldsfld, fI));
    I(il.Create(OpCodes.Brfalse, skip));
    I(il.Create(OpCodes.Ldsfld, fI));
    I(il.Create(OpCodes.Ldloc, v));
    I(il.Create(OpCodes.Stfld, fHon));
    il.InsertBefore(first, skip);
    Console.WriteLine("[A] ChangeClothes 写入 hounerLevel");
}

// ---------------- B) RefreshMainSkills ----------------
var rms = pm.Methods.First(m => m.Name == "RefreshMainSkills" && m.Parameters.Count == 0);
{
    var il = rms.Body.GetILProcessor();
    Instruction target = null;
    foreach (var ins in rms.Body.Instructions)
        if ((ins.OpCode == OpCodes.Brfalse || ins.OpCode == OpCodes.Brfalse_S) && ins.Operand is Instruction t && t.Offset > ins.Offset) { target = ins; break; }
    if (target == null) throw new Exception("RefreshMainSkills 未找到 brfalse");
    il.Replace(target, il.Create(OpCodes.Pop));
    Console.WriteLine("[B] RefreshMainSkills 强制显示");
}

// ---------------- C) FixedUpdate ----------------
var fu = pm.Methods.First(m => m.Name == "FixedUpdate" && m.Parameters.Count == 0);
{
    var body = fu.Body;
    var il = body.GetILProcessor();
    var first = body.Instructions[0];
    var v = new VariableDefinition(mod.TypeSystem.Int32);
    body.Variables.Add(v);
    var skip = il.Create(OpCodes.Nop);
    void I(Instruction x) => il.InsertBefore(first, x);
    I(il.Create(OpCodes.Ldsfld, fI));
    I(il.Create(OpCodes.Brfalse, skip));
    I(il.Create(OpCodes.Ldstr, "chenghao_lv"));
    I(il.Create(OpCodes.Ldc_I4_0));
    I(il.Create(OpCodes.Call, mGetInt));
    I(il.Create(OpCodes.Stloc, v));
    I(il.Create(OpCodes.Ldloc, v));
    I(il.Create(OpCodes.Ldc_I4_0));
    I(il.Create(OpCodes.Ble, skip));
    I(il.Create(OpCodes.Ldloc, v));
    I(il.Create(OpCodes.Ldsfld, fI));
    I(il.Create(OpCodes.Ldfld, fHon));
    I(il.Create(OpCodes.Beq, skip));
    I(il.Create(OpCodes.Ldsfld, fI));
    I(il.Create(OpCodes.Ldloc, v));
    I(il.Create(OpCodes.Stfld, fHon));
    I(il.Create(OpCodes.Ldsfld, fI));
    I(il.Create(OpCodes.Callvirt, mOcc));
    il.InsertBefore(first, skip);
    Console.WriteLine("[C] FixedUpdate 变化即重刷");
}

asm.Write(dst);
Console.WriteLine("[done] chenghao v5 -> " + dst);
return 0;
