// ilpatch_probe4 —— 双探针：
//  A) Starry.StarryConfig.GetBool 入口：Debug.Log("WXSJDBG pdp=<persistentDataPath> key=<key>")
//  B) PlayerMgr.OnHit 入口：Debug.Log("WXSJDBG onhit")   ← 判定伤害主链是否真的走这个方法
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
var tApp = new TypeReference("UnityEngine", "Application", mod, core);
var mPdp = new MethodReference("get_persistentDataPath", mod.TypeSystem.String, tApp) { HasThis = false };
var mConcat = mod.ImportReference(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));

var cfg = mod.GetType("Starry.StarryConfig");
var getBool = cfg?.Methods.FirstOrDefault(m => m.Name == "GetBool" && m.Parameters.Count == 1);
if (getBool != null) {
    var il = getBool.Body.GetILProcessor();
    var first = getBool.Body.Instructions[0];
    void B(Instruction i) => il.InsertBefore(first, i);
    B(il.Create(OpCodes.Ldstr, "WXSJDBG pdp="));
    B(il.Create(OpCodes.Call, mPdp));
    B(il.Create(OpCodes.Call, mConcat));
    B(il.Create(OpCodes.Ldstr, " key="));
    B(il.Create(OpCodes.Call, mConcat));
    B(il.Create(OpCodes.Ldarg_0));
    B(il.Create(OpCodes.Call, mConcat));
    B(il.Create(OpCodes.Call, mLog));
    Console.WriteLine("[probe] GetBool ok");
}

var pm = mod.GetType("PlayerMgr");
var onHit = pm?.Methods.FirstOrDefault(m => m.Name == "OnHit");
if (onHit != null) {
    var il2 = onHit.Body.GetILProcessor();
    var f2 = onHit.Body.Instructions[0];
    il2.InsertBefore(f2, il2.Create(OpCodes.Ldstr, "WXSJDBG onhit"));
    il2.InsertBefore(f2, il2.Create(OpCodes.Call, mLog));
    Console.WriteLine("[probe] PlayerMgr.OnHit ok");
}

asm.Write(dst);
Console.WriteLine("[done] probe4 -> " + dst);
return 0;
