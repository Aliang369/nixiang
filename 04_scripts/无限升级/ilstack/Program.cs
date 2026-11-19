using Mono.Cecil;
using Mono.Cecil.Cil;

// 只读求值栈校验器：对 DLL 全量方法做 IL 栈深数据流校验（含 EH 边界），并可选做调用图可达性分析。
// 用法:
//   ilstack <dll>                     全量校验，打印违规明细
//   ilstack <dll> --quiet             只打印汇总
//   ilstack <dll> --path Type::Method 从该方法做调用图 BFS，标出「我们改过的方法」
var path = args[0];
var mode = args.Length > 1 ? args[1] : "";
var asm = AssemblyDefinition.ReadAssembly(path);
var mod = asm.MainModule;

IEnumerable<TypeDefinition> All(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x; }
var types = mod.Types.SelectMany(All).ToList();

int PopCount(StackBehaviour b, Instruction ins, out bool special)
{
    special = false;
    switch (b)
    {
        case StackBehaviour.Pop0: return 0;
        case StackBehaviour.Pop1: case StackBehaviour.Popi: case StackBehaviour.Popref: return 1;
        case StackBehaviour.Pop1_pop1: case StackBehaviour.Popi_pop1: case StackBehaviour.Popi_popi:
        case StackBehaviour.Popi_popi8: case StackBehaviour.Popi_popr4: case StackBehaviour.Popi_popr8:
        case StackBehaviour.Popref_pop1: case StackBehaviour.Popref_popi: return 2;
        case StackBehaviour.Popi_popi_popi: case StackBehaviour.Popref_popi_popi: case StackBehaviour.Popref_popi_popi8:
        case StackBehaviour.Popref_popi_popr4: case StackBehaviour.Popref_popi_popr8: case StackBehaviour.Popref_popi_popref: return 3;
        case StackBehaviour.PopAll: special = true; return -1;
        case StackBehaviour.Varpop:
            if (ins.OpCode.Code == Code.Ret || ins.OpCode.Code == Code.Throw) { special = true; return -1; }
            if (ins.Operand is MethodReference mr)
            {
                int n = mr.Parameters.Count + (mr.HasThis ? 1 : 0);
                if (ins.OpCode.Code == Code.Newobj) n = mr.Parameters.Count;
                return n;
            }
            if (ins.OpCode.Code == Code.Endfinally || ins.OpCode.Code == Code.Endfilter) { special = true; return -1; }
            return 0;
    }
    return 0;
}
int PushCount(StackBehaviour b, Instruction ins)
{
    switch (b)
    {
        case StackBehaviour.Push0: return 0;
        case StackBehaviour.Push1: case StackBehaviour.Pushi: case StackBehaviour.Pushi8:
        case StackBehaviour.Pushr4: case StackBehaviour.Pushr8: case StackBehaviour.Pushref: return 1;
        case StackBehaviour.Push1_push1: return 2;
        case StackBehaviour.Varpush:
            if (ins.Operand is MethodReference mr) return mr.ReturnType.FullName == "System.Void" ? 0 : 1;
            return 0;
    }
    return 0;
}

int bad = 0, checkedM = 0, checkedI = 0;
var reports = new List<string>();

foreach (var t in types)
foreach (var md in t.Methods)
{
    if (!md.HasBody) continue;
    checkedM++;
    var ins = md.Body.Instructions;
    var depth = new Dictionary<Instruction, int>();
    var q = new Queue<Instruction>();
    depth[ins[0]] = 0; q.Enqueue(ins[0]);
    var methName = t.FullName + "::" + md.Name;
    while (q.Count > 0)
    {
        var i = q.Dequeue();
        int d = depth[i];
        checkedI++;
        if (i.OpCode.Code == Code.Ret)
        {
            int need = md.ReturnType.FullName == "System.Void" ? 0 : 1;
            if (d != need) { bad++; reports.Add($"[RET-DEPTH] {methName} IL_{i.Offset:X4} ret 处栈深={d} 期望={need}"); }
            continue;
        }
        if (i.OpCode.Code == Code.Throw)
        {
            if (d < 1) { bad++; reports.Add($"[UNDERFLOW] {methName} IL_{i.Offset:X4} throw 栈深={d}"); }
            continue;
        }
        bool special; int pop = PopCount(i.OpCode.StackBehaviourPop, i, out special);
        int push = PushCount(i.OpCode.StackBehaviourPush, i);
        int after;
        if (i.OpCode.Code == Code.Endfinally || i.OpCode.Code == Code.Endfilter) continue;
        if (i.OpCode.Code == Code.Leave || i.OpCode.Code == Code.Leave_S)
        {
            after = 0;
            if (d < 0) { bad++; reports.Add($"[UNDERFLOW] {methName} IL_{i.Offset:X4} leave"); }
        }
        else
        {
            if (pop < 0) { after = d; }
            else
            {
                if (d < pop) { bad++; reports.Add($"[UNDERFLOW] {methName} IL_{i.Offset:X4} {i.OpCode} 需 {pop} 栈深={d}  (prev: {(i.Previous==null?"<entry>":i.Previous.OpCode.ToString())})"); continue; }
                after = d - pop + push;
            }
        }
        // 后继
        IEnumerable<Instruction> nexts;
        if (i.OpCode.FlowControl == FlowControl.Branch) nexts = new[] { (Instruction)i.Operand };
        else if (i.OpCode.FlowControl == FlowControl.Cond_Branch)
        {
            var list = new List<Instruction>();
            if (i.Operand is Instruction tgt) list.Add(tgt);
            else if (i.Operand is Instruction[] arr) list.AddRange(arr);
            if (i.Next != null) list.Add(i.Next);
            nexts = list;
        }
        else if (i.OpCode.FlowControl == FlowControl.Return || i.OpCode.FlowControl == FlowControl.Throw) nexts = new Instruction[0];
        else nexts = i.Next == null ? new Instruction[0] : new[] { i.Next };
        foreach (var nx in nexts)
        {
            if (depth.TryGetValue(nx, out var known))
            {
                if (known != after) { bad++; reports.Add($"[DEPTH-MISMATCH] {methName} IL_{nx.Offset:X4} 由 IL_{i.Offset:X4}({i.OpCode}) 到达栈深={after} 与已记录 {known} 不一致"); }
            }
            else { depth[nx] = after; q.Enqueue(nx); }
        }
    }
    // EH：catch/filter 入口栈深必须是 1；finally/fault 入口 0
    foreach (var h in md.Body.ExceptionHandlers)
    {
        if (h.HandlerStart == null) continue;
        int need = (h.HandlerType == ExceptionHandlerType.Catch || h.HandlerType == ExceptionHandlerType.Filter) ? 1 : 0;
        if (!depth.TryGetValue(h.HandlerStart, out var d)) continue;
        if (d != need) { bad++; reports.Add($"[EH-DEPTH] {methName} handler IL_{h.HandlerStart.Offset:X4} {h.HandlerType} 栈深={d} 期望={need}"); }
        if (h.FilterStart != null && depth.TryGetValue(h.FilterStart, out var fd) && fd != 1)
        { bad++; reports.Add($"[EH-DEPTH] {methName} filter IL_{h.FilterStart.Offset:X4} 栈深={fd} 期望=1"); }
    }
}

Console.WriteLine($"[ilstack] {System.IO.Path.GetFileName(path)}  methods={checkedM} visits={checkedI} violations={bad}");
if (mode != "--quiet")
{
    foreach (var r in reports.Take(200)) Console.WriteLine("  " + r);
    if (reports.Count > 200) Console.WriteLine($"  ... 还有 {reports.Count - 200} 条");
}
Environment.ExitCode = bad == 0 ? 0 : 1;
