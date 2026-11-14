using Mono.Cecil;
using Mono.Cecil.Cil;

// 汉克旅店 1.1.0（com.chillyroom.hanksbar.yw / vc337）热更程序集 IL 补丁器
//
// 输入：06_dumped/汉克旅店/Assembly-CSharp.dll（sha256 d2e7edf0…3277，5,328,896 B）
// 输出：07_patched/汉克旅店/Assembly-CSharp_hkld.dll（由 native 在 Assembly.Load(byte[]) 边界替换）
//
// 六个功能（全部「入口前置门控」：配置缺失/关闭时原 IL 原样执行，不删改原指令，只插入）：
//   ① 无敌      PlayerManager.RefreshHP(bool,bool,bool)        入口: if(god) this.isPlayerHurt=false
//   ② 伤害倍率  MonsterControl.CustomMonsterHurt(float,bool,int,enum,int) 入口: finalDamageNum *= damage_x
//   ③ 无限技能  SkillUseManager.OnUseSkillEnd(int,int)          入口: if(inf_skill) mpConsume=0
//               GameManager.SetSkillCoolingTime(int)           入口: if(inf_skill) return（collingTimeDic 保持 0 = 真无 CD）
//               GameManager.SetCoolingTimeMax(int,int)         入口: if(inf_skill) cdNum=0（cdMaxList 归零，UI 遮罩不显示）
//   ④ 解锁角色  SaveDataManager.RepairSaveData()              末尾: Starry.HkldPatch.UnlockAll(this)
//   ⑤ 无限金币  PlayerManager.RefreshMoney(bool)               入口: if(inf_money) curMoney=lastMoney=9999
//   ⑥ 无限钥匙  PlayerManager.RefreshKeyCount()                入口: if(inf_key) keyCount=999
//   ⑦ 移动速度  PlayerManager.GetRealMoveSpeed()               每个 ret 前: MoveMul(float) × move_speed_x(100..300%)
//   ⑧ 无限蓝    PlayerManager.RefreshMP(bool)                   入口: MpFill(this) —— if(inf_mp) curMp=mp
//
// 铁律（skill/re-implement/hotfix-dll-replace-pattern.md §Cecil 检查清单）：
//   1) 参数下标：Parameters 不含 this ⇒ SlotToParamIndex(slot)=HasThis?slot-1:slot；写回前断言参数名+类型
//   2) 写盘期解析：任何成员类型解析不到都会中止 Write ⇒ TolerantRefs 垫片 + 宽容解析器 + 常量台账
//   3) 字节容器：产物大于原文件 ⇒ native 侧按新长度重新分配（本仓 HkldMod.cpp 已实现）
internal static class Patcher
{
    const string FILE_NAME = "starry_config.ini";
    // HybridCLR 解释器下 Application.persistentDataPath 实测常为空串 ⇒ 用包名私有目录绝对路径。
    // native 侧（Games/HkldMod.cpp 的 ConfigPath()）必须写同一路径。
    const string PKG = "com.chillyroom.hanksbar.yw";
    const string CFG_PATH = "/data/user/0/" + PKG + "/files/" + FILE_NAME;

    // 配置键 —— 与 Games/HkldMod.cpp 的 WriteConfigKey 逐字一致（改一处必须改两处）
    const string K_GOD    = "god";
    const string K_DMGX   = "damage_x";
    const string K_SKILL  = "inf_skill";
    const string K_UNLOCK = "unlock_char";
    const string K_MONEY  = "inf_money";
    const string K_KEY    = "inf_key";
    const string K_MOVESP = "move_speed_x";
    // 移速百分比口径：move_speed_x = 100 表示原速（默认值 = 菜单 min ⇒ 缺键/关菜单零副作用），
    // 200 = 2x、300 = 3x。结果再夹到 [MOVESP_MIN, MOVESP_MAX]，防属性叠加后高速穿透碰撞/触发体。
    const int MOVESP_DEF = 100;
    const float MOVESP_MIN = 1f;
    const float MOVESP_MAX = 999f;
    const string K_INFMP  = "inf_mp";

    static ModuleDefinition mod;
    static MethodReference mGetBool, mGetInt;
    static TypeReference tStr, tI32, tBool;
    static int nPatch;

    // 被补丁的方法清单（Program.cs 据此打印完整 IL）
    public static readonly (string Type, string Method)[] Targets =
    {
        ("PlayerManager",     "RefreshHP"),
        ("MonsterControl",    "CustomMonsterHurt"),
        ("SkillUseManager",   "OnUseSkillEnd"),
        ("GameManager",       "SetSkillCoolingTime"),
        ("GameManager",       "SetCoolingTimeMax"),
        ("PlayerChoicePanel", "UpdateCharacter"),
        ("PlayerManager",     "RefreshMoney"),
        ("PlayerManager",     "RefreshKeyCount"),
        ("PlayerManager",     "RefreshMP"),
        ("PlayerManager",     "GetRealMoveSpeed"),
        ("Starry.HkldPatch",  "ForceUnlock"),
        ("Starry.HkldPatch",  "MpFill"),
        ("Starry.HkldPatch",  "MoveMul"),
    };

    public static void Run(string srcPath, string dstPath)
    {
        var resolver = new TolerantAssemblyResolver();
        foreach (var d in RefDirs(srcPath))
            if (Directory.Exists(d)) { resolver.AddSearchDirectory(d); Console.WriteLine("[ref] " + d); }
        var metaResolver = new TolerantMetadataResolver(resolver);
        var asm = AssemblyDefinition.ReadAssembly(srcPath, new ReaderParameters
        {
            AssemblyResolver = resolver,
            MetadataResolver = metaResolver,
            ReadSymbols = false,
        });
        mod = asm.MainModule;
        Console.WriteLine("[in ] " + srcPath + "  (" + new FileInfo(srcPath).Length + " B)");
        Console.WriteLine("[in ] assembly=" + asm.Name.Name + "  types=" + mod.Types.Count);

        var realDirs = RefDirs(srcPath).Where(Directory.Exists).ToList();
        var needs = ShimFactory.Discover(mod, realDirs);
        var shimDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcPath)), "refs_shim");
        var nShim = ShimFactory.Materialize(shimDir, needs);
        if (nShim > 0) { resolver.AddSearchDirectory(shimDir); Console.WriteLine("[shim] " + nShim + " 个垫片程序集 -> " + shimDir); }

        BuildStarryConfig();

        P(PatchGod,          "PlayerManager.RefreshHP            [god]");
        P(PatchDamageMul,    "MonsterControl.CustomMonsterHurt   [damage_x]");
        P(PatchSkillNoCost,  "SkillUseManager.OnUseSkillEnd      [inf_skill: mp]");
        P(PatchSkillNoCd,    "GameManager.SetSkillCoolingTime    [inf_skill: cd]");
        P(PatchCoolingMax,   "GameManager.SetCoolingTimeMax      [inf_skill: cdMaxList]");
        P(PatchForceUnlock,  "Save/MainCharacterInfo.isUnlock 全部读取点 [unlock_char]");
        P(PatchMoney,        "PlayerManager.RefreshMoney         [inf_money]");
        P(PatchKey,          "PlayerManager.RefreshKeyCount      [inf_key]");
        P(PatchInfMp,        "PlayerManager.RefreshMP            [inf_mp]");
        P(PatchMoveSpeed,    "PlayerManager.GetRealMoveSpeed     [move_speed_x]");

        var constantsBefore = ConstantAudit.Snapshot(mod);
        asm.Write(dstPath, new WriterParameters { WriteSymbols = false });
        asm.Dispose();
        var len = new FileInfo(dstPath).Length;
        Console.WriteLine("[out] " + dstPath + "  (" + len + " B)");
        Console.WriteLine("[ok ] patches=" + nPatch);

        using (var back = AssemblyDefinition.ReadAssembly(dstPath, new ReaderParameters
        {
            AssemblyResolver = resolver, MetadataResolver = metaResolver, ReadSymbols = false,
        }))
        {
            var lost = ConstantAudit.Diff(constantsBefore, back.MainModule);
            Console.WriteLine(lost.Count == 0
                ? "[gate] 常量台账 PASS（" + constantsBefore.Count + " 条真实默认值全部保留）"
                : "[gate] 常量台账 FAIL（丢失 " + lost.Count + " 条）：\n  " + string.Join("\n  ", lost));
            if (lost.Count != 0) Environment.Exit(1);
        }
    }

    // =====================================================================
    //  ① 无敌：RefreshHP 入口把 isPlayerHurt 置 false ⇒ 复用游戏自带「免疫」分支
    //     （PlayerManager.cs:1404，原分支会飘「免疫」字并把 curHp 还原为 preHp）
    // =====================================================================
    static void PatchGod()
    {
        var t = NeedType("PlayerManager");
        var m = NeedMethod(t, "RefreshHP", 3);
        AssertParams(m, new[] { "isAnim", "isIgnoreArmor", "isLoadSave" }, new[] { "Boolean", "Boolean", "Boolean" });
        if (HasLdstr(m, K_GOD)) { Console.WriteLine("[skip] RefreshHP 已含 " + K_GOD); return; }
        var f = t.Fields.FirstOrDefault(x => x.Name == "isPlayerHurt");
        if (f == null) throw new Exception("PlayerManager.isPlayerHurt 字段不存在");

        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var skip = Instruction.Create(OpCodes.Nop);
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, K_GOD));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, mGetBool));
        il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, skip));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Stfld, f));
        il.InsertBefore(first, skip);
    }

    // =====================================================================
    //  ② 伤害倍率：CustomMonsterHurt 入口 finalDamageNum *= GetInt("damage_x",1)
    //     唯一汇聚点（普攻 DamageManager / 法术 SkillUseManager 69 处都收敛到它）
    // =====================================================================
    static void PatchDamageMul()
    {
        var t = NeedType("MonsterControl");
        var m = NeedMethod(t, "CustomMonsterHurt", 5);
        AssertParams(m,
            new[] { "finalDamageNum", "isCritical", "critRate", "monsterBeHarmedType", "magicId" },
            new[] { "Single", "Boolean", "Int32", "MonsterBeHarmedType", "Int32" });
        if (HasLdstr(m, K_DMGX)) { Console.WriteLine("[skip] CustomMonsterHurt 已含 " + K_DMGX); return; }

        var vX = new VariableDefinition(mod.TypeSystem.Int32);
        m.Body.Variables.Add(vX);
        m.Body.InitLocals = true;
        if (m.Body.MaxStackSize < 4) m.Body.MaxStackSize = 4;

        var pDmg = m.Parameters[0];       // arg slot 1（HasThis）
        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var skip = Instruction.Create(OpCodes.Nop);
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, K_DMGX));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_1));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, mGetInt));
        il.InsertBefore(first, Instruction.Create(OpCodes.Stloc, vX));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldloc, vX));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_1));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ble, skip));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));         // finalDamageNum
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldloc, vX));
        il.InsertBefore(first, Instruction.Create(OpCodes.Conv_R4));
        il.InsertBefore(first, Instruction.Create(OpCodes.Mul));
        il.InsertBefore(first, Instruction.Create(OpCodes.Starg, pDmg));     // 写回 slot 1 = Parameters[0]
        il.InsertBefore(first, skip);
    }

    // =====================================================================
    //  ③a 无限技能（免蓝）：OnUseSkillEnd 入口 mpConsume = 0
    // =====================================================================
    static void PatchSkillNoCost()
    {
        var t = NeedType("SkillUseManager");
        var m = NeedMethod(t, "OnUseSkillEnd", 2);
        AssertParams(m, new[] { "mpConsume", "magicId" }, new[] { "Int32", "Int32" });
        if (HasLdstr(m, K_SKILL)) { Console.WriteLine("[skip] OnUseSkillEnd 已含 " + K_SKILL); return; }

        var pMp = m.Parameters[0];
        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var skip = Instruction.Create(OpCodes.Nop);
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, K_SKILL));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, mGetBool));
        il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, skip));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Starg, pMp));
        il.InsertBefore(first, skip);
    }

    // =====================================================================
    //  ③b 无限技能（无 CD）：SetSkillCoolingTime 入口直接 return
    //     为什么必须在**这里**拦：本函数内 Mathf.Clamp(baseCdNum,1f,999f) 会给最小 1 回合，
    //     且 collingTimeDic[skill] 被写成 1；而 SkillUseManager.cs:4446 / GameManager.cs:390
    //     两个「能不能放技能」的门禁判的是 collingTimeDic != 0。提前返回 ⇒ 字典保持 0 = 真无 CD。
    // =====================================================================
    static void PatchSkillNoCd()
    {
        var t = NeedType("GameManager");
        var m = NeedMethod(t, "SetSkillCoolingTime", 1);
        AssertParams(m, new[] { "skillNum" }, new[] { "Int32" });
        if (HasLdstr(m, K_SKILL)) { Console.WriteLine("[skip] SetSkillCoolingTime 已含 " + K_SKILL); return; }

        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var cont = Instruction.Create(OpCodes.Nop);
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, K_SKILL));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, mGetBool));
        il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, cont));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        il.InsertBefore(first, cont);
    }

    // =====================================================================
    //  ③c 冷却上限归零：SetCoolingTimeMax 入口 cdNum = 0 ⇒ cdMaxList[skillNum]=0
    //     （RefreshCoolingTime 第 314 行 cdMaxList[i]<=0 直接跳过，UI 遮罩不显示）
    // =====================================================================
    static void PatchCoolingMax()
    {
        var t = NeedType("GameManager");
        var m = NeedMethod(t, "SetCoolingTimeMax", 2);
        AssertParams(m, new[] { "skillNum", "cdNum" }, new[] { "Int32", "Int32" });
        if (HasLdstr(m, K_SKILL)) { Console.WriteLine("[skip] SetCoolingTimeMax 已含 " + K_SKILL); return; }

        var pCd = m.Parameters[1];
        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var skip = Instruction.Create(OpCodes.Nop);
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, K_SKILL));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, mGetBool));
        il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, skip));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Starg, pCd));
        il.InsertBefore(first, skip);
    }

    // =====================================================================
    //  ④ 解锁所有角色（本地）：所有 Save/MainCharacterInfo.isUnlock 的读取点统一过一道闸
    //
    //  为什么不用「RepairSaveData 末尾循环 UnlockNewCharacter」：
    //   · RepairSaveData 确实被 LoadByJson 调用（runtime 基线 SaveDataManager.cs:224/242/254），
    //     但它在存档加载那一刻执行；菜单开关是之后才被用户打开的 ⇒ 读到的还是 0，白跑；
    //   · UnlockNewCharacter(id) 要求 id 已在 characterInfoDic（键是字符串 id），用 0..N 猜 id
    //     属臆造（角色 id 可能稀疏）。
    //
    //  现方案：在每个 ldfld Save/MainCharacterInfo::isUnlock 之后统一接
    //    Starry.HkldPatch.ForceUnlock(bool v) = GetBool("unlock_char") ? true : v
    //  覆盖全部判锁点（PlayerChoicePanel.UpdateCharacter/EnterEvent 的进关与按钮态、
    //  MainMenuShopHelper 的已拥有判定、SaveDataManager 的统计/重建），不猜任何 id、
    //  不写档也立刻生效；开关关掉即恢复原值。
    // =====================================================================
    static void PatchForceUnlock()
    {
        var helper = BuildForceUnlock();
        int sites = 0;
        foreach (var t in mod.Types.ToList())
        {
            if (!t.HasMethods) continue;
            foreach (var m in t.Methods.ToList())
            {
                if (!m.HasBody) continue;
                var snapshot = m.Body.Instructions.ToList();
                var il = m.Body.GetILProcessor();
                int n = 0;
                foreach (var ins in snapshot)
                {
                    if (ins.OpCode != OpCodes.Ldfld) continue;
                    if (!(ins.Operand is FieldReference fr)) continue;
                    if (fr.Name != "isUnlock") continue;
                    if (!fr.DeclaringType.FullName.EndsWith("MainCharacterInfo")) continue;
                    il.InsertAfter(ins, Instruction.Create(OpCodes.Call, helper));
                    n++;
                }
                if (n > 0) { sites += n; Console.WriteLine("[patch] " + t.FullName + "::" + m.Name + " isUnlock 读取点 " + n + " 处"); }
            }
        }
        if (sites == 0) throw new Exception("没有找到任何 isUnlock 读取点（字段名/类型名变了？）");
        Console.WriteLine("[patch] ForceUnlock 覆盖 " + sites + " 处判锁读取点");
    }

    static MethodDefinition BuildForceUnlock()
    {
        var t = GetOrCreateHkldType();
        var exist = t.Methods.FirstOrDefault(x => x.Name == "ForceUnlock");
        if (exist != null) return exist;

        var m = new MethodDefinition("ForceUnlock",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, mod.TypeSystem.Boolean);
        m.Parameters.Add(new ParameterDefinition("v", ParameterAttributes.None, mod.TypeSystem.Boolean));
        t.Methods.Add(m);
        m.Body.InitLocals = true;
        m.Body.MaxStackSize = 2;
        var il = m.Body.GetILProcessor();
        var keepV = Instruction.Create(OpCodes.Ldarg_0);
        il.Append(Instruction.Create(OpCodes.Ldstr, K_UNLOCK));
        il.Append(Instruction.Create(OpCodes.Call, mGetBool));
        il.Append(Instruction.Create(OpCodes.Brfalse, keepV));
        il.Append(Instruction.Create(OpCodes.Ldc_I4_1));
        il.Append(Instruction.Create(OpCodes.Ret));
        il.Append(keepV);
        il.Append(Instruction.Create(OpCodes.Ret));
        Console.WriteLine("[patch] Starry.HkldPatch.ForceUnlock 已注入");
        return m;
    }

    // =====================================================================
    //  ⑤ 无限金币：RefreshMoney 入口 curMoney = lastMoney = 9999
    //     同时写 lastMoney：游戏用 curMoney-lastMoney 差值触发「邪神7」扣血（PlayerManager.cs:1985）
    //     9999 = 本作硬上限（第 1984 行 Mathf.Clamp(curMoney,0,9999)），写更大也会被夹回
    // =====================================================================
    static void PatchMoney()
    {
        var t = NeedType("PlayerManager");
        var m = NeedMethod(t, "RefreshMoney", 1);
        AssertParams(m, new[] { "isLoadSave" }, new[] { "Boolean" });
        if (HasLdstr(m, K_MONEY)) { Console.WriteLine("[skip] RefreshMoney 已含 " + K_MONEY); return; }
        var fCur = t.Fields.FirstOrDefault(x => x.Name == "curMoney");
        var fLast = t.Fields.FirstOrDefault(x => x.Name == "lastMoney");
        if (fCur == null || fLast == null) throw new Exception("PlayerManager.curMoney/lastMoney 字段缺失");

        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var skip = Instruction.Create(OpCodes.Nop);
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, K_MONEY));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, mGetBool));
        il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, skip));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4, 9999));
        il.InsertBefore(first, Instruction.Create(OpCodes.Stfld, fCur));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4, 9999));
        il.InsertBefore(first, Instruction.Create(OpCodes.Stfld, fLast));
        il.InsertBefore(first, skip);
    }

    // =====================================================================
    //  ⑥ 无限钥匙：RefreshKeyCount 入口 keyCount = 999（该字段无 clamp，函数只刷 UI）
    // =====================================================================
    static void PatchKey()
    {
        var t = NeedType("PlayerManager");
        var m = NeedMethod(t, "RefreshKeyCount", 0);
        if (HasLdstr(m, K_KEY)) { Console.WriteLine("[skip] RefreshKeyCount 已含 " + K_KEY); return; }
        var f = t.Fields.FirstOrDefault(x => x.Name == "keyCount");
        if (f == null) throw new Exception("PlayerManager.keyCount 字段缺失");

        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var skip = Instruction.Create(OpCodes.Nop);
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldstr, K_KEY));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, mGetBool));
        il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, skip));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldc_I4, 999));
        il.InsertBefore(first, Instruction.Create(OpCodes.Stfld, f));
        il.InsertBefore(first, skip);
    }

    // =====================================================================
    //  ⑧ 无限蓝：RefreshMP 入口 MpFill(this) —— if(inf_mp) curMp = mp
    //
    //  为什么挂这里：全部扣蓝点都是 `PlayerManager.curMp -= X; PlayerManager.RefreshMP();`
    //  （ItemControl.cs:494 / SkillUseManager.cs:7481 / AoeSkillControl.cs:587 /
    //    JingAbilityControl.cs:1699 等 20+ 处，已抽样确认）。RefreshMP 内部原本就是
    //  「curMp>mp ⇒ curMp=mp；curMp<0 ⇒ 0；conv.i4 取整；刷 mpText + mpImgObjList」，
    //  所以入口把 curMp 抬到上限即可，蓝条会显示满、不需要另外写 UI。
    //
    //  副作用（已写进 10_docs/汉克旅店/路线锁定.md）：只保证蓝条不掉；
    //  「有魔法宝石时技能消耗改扣血」是另一条 curHp 路径，不受本补丁影响。
    //  入口插入即使命中 RefreshMP 开头的 curGameState==0 提前 return 也无害（只多写一次 curMp）。
    // =====================================================================
    static void PatchInfMp()
    {
        var t = NeedType("PlayerManager");
        var m = NeedMethod(t, "RefreshMP", 1);
        AssertParams(m, new[] { "isLoadSave" }, new[] { "Boolean" });
        if (HasLdstr(m, K_INFMP)) { Console.WriteLine("[skip] RefreshMP 已含 " + K_INFMP); return; }
        NeedField(t, "curMp", "Single");
        NeedField(t, "mp", "Single");
        var helper = BuildMpFill();

        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
        il.InsertBefore(first, Instruction.Create(OpCodes.Call, helper));
        Console.WriteLine("[patch] RefreshMP 入口已接 MpFill（" + m.Body.Instructions.Count + " 条 IL）");
    }

    // static void MpFill(PlayerManager pm) { if (GetBool("inf_mp")) pm.curMp = pm.mp; }
    static MethodDefinition BuildMpFill()
    {
        var t = GetOrCreateHkldType();
        var exist = t.Methods.FirstOrDefault(x => x.Name == "MpFill");
        if (exist != null) return exist;
        var pmType = NeedType("PlayerManager");
        var fCur = NeedField(pmType, "curMp", "Single");
        var fMax = NeedField(pmType, "mp", "Single");

        var m = new MethodDefinition("MpFill",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, mod.TypeSystem.Void);
        m.Parameters.Add(new ParameterDefinition("pm", ParameterAttributes.None, pmType));
        t.Methods.Add(m);
        m.Body.InitLocals = true;
        m.Body.MaxStackSize = 2;
        var il = m.Body.GetILProcessor();
        var ret = Instruction.Create(OpCodes.Ret);
        il.Append(Instruction.Create(OpCodes.Ldstr, K_INFMP));
        il.Append(Instruction.Create(OpCodes.Call, mGetBool));
        il.Append(Instruction.Create(OpCodes.Brfalse, ret));
        il.Append(Instruction.Create(OpCodes.Ldarg_0));
        il.Append(Instruction.Create(OpCodes.Ldarg_0));
        il.Append(Instruction.Create(OpCodes.Ldfld, fMax));
        il.Append(Instruction.Create(OpCodes.Stfld, fCur));
        il.Append(ret);
        Console.WriteLine("[patch] Starry.HkldPatch.MpFill 已注入（if " + K_INFMP + " ⇒ pm.curMp = pm.mp）");
        return m;
    }

    // =====================================================================
    //  ⑦ 移动速度：GetRealMoveSpeed 的每个 ret 前接 MoveMul(float) —— v * move_speed_x/100（100..300%）
    //
    //  为什么挂这里：PlayerManager.GetRealMoveSpeed() 只有两条 return（GAME 态读
    //  totalAttributes.moveSpeed/100f，非 GAME 态读 PlayerData.MoveSpeed/100f），
    //  消费方全部收敛到它：PlayerSwipe.GetRealMoveSpeed()（PlayerJoystickControl.cs:227/236
    //  摇杆位移）、PlayerAnimatorManager.cs:100（动画 MoveSpeed）。一个挂点同时覆盖位移与动画。
    //  不改属性数值、不写存档 ⇒ 滑块拨回 1 立即恢复原速。
    //  已知未覆盖（有意为之，写进路线锁定.md）：HuJiaoRollAbiltiyControl.cs:518/815 直接读
    //  totalAttributes.moveSpeed 算翻滚系数，属技能内部公式，不受本补丁影响。
    // =====================================================================
    static void PatchMoveSpeed()
    {
        var t = NeedType("PlayerManager");
        var m = NeedMethod(t, "GetRealMoveSpeed", 0);
        if (m.ReturnType.Name != "Single") throw new Exception("GetRealMoveSpeed 返回类型=" + m.ReturnType.Name + " 期望 Single");
        int already = m.Body.Instructions.Count(i => i.Operand is MethodReference r && r.Name == "MoveMul");
        if (already > 0) { Console.WriteLine("[skip] GetRealMoveSpeed 已含 " + already + " 处 MoveMul"); return; }
        var helper = BuildMoveMul();

        var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        if (rets.Count == 0) throw new Exception("GetRealMoveSpeed 没有 ret（方法体异常？）");
        var il = m.Body.GetILProcessor();
        foreach (var ret in rets) il.InsertBefore(ret, Instruction.Create(OpCodes.Call, helper));
        if (m.Body.MaxStackSize < 2) m.Body.MaxStackSize = 2;
        Console.WriteLine("[patch] GetRealMoveSpeed " + rets.Count + " 个 ret 前已接 MoveMul");
    }

    // static float MoveMul(float v)
    // {
    //     int x = GetInt("move_speed_x", 100);          // 100 = 原速（菜单 min = 默认值）
    //     if (x <= 100) return v;                       // 缺键/关菜单/最小值 ⇒ 零副作用
    //     float r = v * x / 100f;                       // 100..300 ⇒ 1x..3x
    //     if (r > 999f) r = 999f;                       // 上限夹（防属性叠加后高速穿透）
    //     if (r < 1f) return 1f;                        // 下界兜底
    //     return r;
    // }
    static MethodDefinition BuildMoveMul()
    {
        var t = GetOrCreateHkldType();
        var exist = t.Methods.FirstOrDefault(x => x.Name == "MoveMul");
        if (exist != null) return exist;

        var m = new MethodDefinition("MoveMul",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, mod.TypeSystem.Single);
        m.Parameters.Add(new ParameterDefinition("v", ParameterAttributes.None, mod.TypeSystem.Single));
        t.Methods.Add(m);
        var vM = new VariableDefinition(mod.TypeSystem.Int32);
        var vR = new VariableDefinition(mod.TypeSystem.Single);
        m.Body.Variables.Add(vM);
        m.Body.Variables.Add(vR);
        m.Body.InitLocals = true;
        m.Body.MaxStackSize = 2;
        var il = m.Body.GetILProcessor();
        var keep = Instruction.Create(OpCodes.Ldarg_0);
        var retF = Instruction.Create(OpCodes.Ldloc, vR);
        // 下界夹的**比较操作数必须是 r（vR）**，不是入参 v。2026-11-15 Lead 审查发现：
        // 首版把跳转标签直接做成 Ldarg_0（栈合法、Gate 也过），于是判的是 v >= 1 而不是 r >= 1 ⇒
        // 基础速度 <1 的角色（MoveSpeed 90 ⇒ v=0.9）拖到 150% 时 r=1.35 被压回 1.0，倍率不对。
        var clampLow = Instruction.Create(OpCodes.Ldloc, vR);
        il.Append(Instruction.Create(OpCodes.Ldstr, K_MOVESP));
        il.Append(Instruction.Create(OpCodes.Ldc_I4, MOVESP_DEF));
        il.Append(Instruction.Create(OpCodes.Call, mGetInt));
        il.Append(Instruction.Create(OpCodes.Stloc, vM));
        il.Append(Instruction.Create(OpCodes.Ldloc, vM));
        il.Append(Instruction.Create(OpCodes.Ldc_I4, MOVESP_DEF));
        il.Append(Instruction.Create(OpCodes.Ble, keep));    // x <= 100 ⇒ 原值返回（缺键/默认/关）
        il.Append(Instruction.Create(OpCodes.Ldarg_0));
        il.Append(Instruction.Create(OpCodes.Ldloc, vM));
        il.Append(Instruction.Create(OpCodes.Conv_R4));
        il.Append(Instruction.Create(OpCodes.Mul));
        il.Append(Instruction.Create(OpCodes.Ldc_R4, 100f));
        il.Append(Instruction.Create(OpCodes.Div));          // r = v * x / 100
        il.Append(Instruction.Create(OpCodes.Stloc, vR));
        il.Append(Instruction.Create(OpCodes.Ldloc, vR));
        il.Append(Instruction.Create(OpCodes.Ldc_R4, MOVESP_MAX));
        il.Append(Instruction.Create(OpCodes.Ble, clampLow)); // r <= 999 ⇒ 再查下界
        il.Append(Instruction.Create(OpCodes.Ldc_R4, MOVESP_MAX));
        il.Append(Instruction.Create(OpCodes.Stloc, vR));     // r > 999 ⇒ 夹到 999（防高速穿透）
        il.Append(clampLow);
        il.Append(Instruction.Create(OpCodes.Ldc_R4, MOVESP_MIN));
        il.Append(Instruction.Create(OpCodes.Bge, retF));     // r >= 1 ⇒ 直接返回
        il.Append(Instruction.Create(OpCodes.Ldc_R4, MOVESP_MIN));
        il.Append(Instruction.Create(OpCodes.Stloc, vR));     // r < 1 ⇒ 兜底 1（防 0/负速卡死）
        il.Append(retF);
        il.Append(Instruction.Create(OpCodes.Ret));
        il.Append(keep);
        il.Append(Instruction.Create(OpCodes.Ret));
        Console.WriteLine("[patch] Starry.HkldPatch.MoveMul 已注入（v * " + K_MOVESP + "/100，默认 " + MOVESP_DEF + " = 原速）");
        return m;
    }

    // =====================================================================
    //  配置读取类型 Starry.StarryConfig（读 starry_config.ini，2s 缓存）
    //  与 native 侧 starry_config.ini 写入通道配套（HybridCLR 下解释器调 native 不可靠，
    //  文件镜像是本仓 HybridCLR 游戏既有口径：无限升级 / 境界死神激斗）
    // =====================================================================
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

        var cctor = new MethodDefinition(".cctor",
            MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            mod.TypeSystem.Void);
        cfgType.Methods.Add(cctor);
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fLines));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        var mGet = new MethodDefinition("Get", MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tStr);
        mGet.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        cfgType.Methods.Add(mGet);
        var vLine = new VariableDefinition(tStr);
        var vI = new VariableDefinition(tI32);
        mGet.Body.Variables.Add(vLine);
        mGet.Body.Variables.Add(vI);
        mGet.Body.InitLocals = true;
        var g = mGet.Body.Instructions;
        var loopExit = Instruction.Create(OpCodes.Ldnull);
        var skipReload = Instruction.Create(OpCodes.Nop);
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fNext));
        g.Add(Instruction.Create(OpCodes.Ble, skipReload));
        g.Add(Instruction.Create(OpCodes.Ldc_I4, 2000));
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        var t0 = Instruction.Create(OpCodes.Ldstr, CFG_PATH);
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

        var mGetBoolDef = new MethodDefinition("GetBool", MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tBool);
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

        // static int GetInt(string key, int def) —— 用 TryParse，不带异常处理器
        // （首版用 Parse + try/catch，IL 落盘后反编译成「返回值被丢弃、恒返回 def」，
        //   见 2026-11-12 复读：Unreachable code IL_002b ⇒ 伤害倍率会静默恒为 1。改成 TryParse。）
        var mTryParse = new MethodReference("TryParse", tBool, mod.TypeSystem.Int32);
        mTryParse.Parameters.Add(new ParameterDefinition(tStr));
        mTryParse.Parameters.Add(new ParameterDefinition(new ByReferenceType(tI32)));

        var mGetDef = new MethodDefinition("GetInt", MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tI32);
        mGetDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        mGetDef.Parameters.Add(new ParameterDefinition("def", ParameterAttributes.None, tI32));
        cfgType.Methods.Add(mGetDef);
        var vV = new VariableDefinition(tStr);
        var vR = new VariableDefinition(tI32);
        mGetDef.Body.Variables.Add(vV);
        mGetDef.Body.Variables.Add(vR);
        mGetDef.Body.InitLocals = true;
        mGetDef.Body.MaxStackSize = 3;
        var mi = mGetDef.Body.Instructions;
        var defPath = Instruction.Create(OpCodes.Ldarg_1);
        mi.Add(Instruction.Create(OpCodes.Ldarg_0));
        mi.Add(Instruction.Create(OpCodes.Call, mGet));
        mi.Add(Instruction.Create(OpCodes.Stloc, vV));
        mi.Add(Instruction.Create(OpCodes.Ldloc, vV));
        mi.Add(Instruction.Create(OpCodes.Brfalse, defPath));
        mi.Add(Instruction.Create(OpCodes.Ldloc, vV));
        mi.Add(Instruction.Create(OpCodes.Ldloca, vR));
        mi.Add(Instruction.Create(OpCodes.Call, mTryParse));
        mi.Add(Instruction.Create(OpCodes.Brfalse, defPath));
        mi.Add(Instruction.Create(OpCodes.Ldloc, vR));
        mi.Add(Instruction.Create(OpCodes.Ret));
        mi.Add(defPath);
        mi.Add(Instruction.Create(OpCodes.Ret));
        mGetInt = mGetDef;

        Console.WriteLine("[cfg] Starry.StarryConfig 已注入（2s 缓存, " + CFG_PATH + "）");
    }

    // =====================================================================
    //  门禁：逐补丁意图断言（不是「跑通没报错」，而是「注入的语义对不对」）
    // =====================================================================
    public static void Gate(ModuleDefinition mm, List<string> fails)
    {
        void A(string name, bool ok, string detail)
        {
            Console.WriteLine("[assert] " + (ok ? "OK  " : "FAIL") + " " + name + ": " + detail);
            if (!ok) fails.Add(name + ": " + detail);
        }
        MethodDefinition Need(string tn, string mn)
        {
            var t = mm.GetType(tn);
            var m = t?.Methods.FirstOrDefault(x => x.Name == mn);
            if (m == null) fails.Add(tn + "::" + mn + " NOT FOUND（补丁未落到该方法）");
            return m;
        }
        int Cnt(MethodDefinition m, string s) => m == null || !m.HasBody ? 0
            : m.Body.Instructions.Count(i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == s);

        var rh = Need("PlayerManager", "RefreshHP");
        A("无敌 god 门落在 RefreshHP", Cnt(rh, K_GOD) == 1, "god 出现 " + Cnt(rh, K_GOD) + " 次（期望 1）");
        var cm = Need("MonsterControl", "CustomMonsterHurt");
        A("伤害倍率门落在 CustomMonsterHurt", Cnt(cm, K_DMGX) == 1, K_DMGX + " 出现 " + Cnt(cm, K_DMGX) + " 次（期望 1）");
        var ou = Need("SkillUseManager", "OnUseSkillEnd");
        A("免蓝门落在 OnUseSkillEnd", Cnt(ou, K_SKILL) == 1, K_SKILL + " 出现 " + Cnt(ou, K_SKILL) + " 次（期望 1）");
        var sc = Need("GameManager", "SetSkillCoolingTime");
        A("无 CD 门落在 SetSkillCoolingTime", Cnt(sc, K_SKILL) == 1, K_SKILL + " 出现 " + Cnt(sc, K_SKILL) + " 次（期望 1）");
        A("SetSkillCoolingTime 入口确有 ret（提前返回）",
            sc != null && sc.Body.Instructions.Count(i => i.OpCode == OpCodes.Ret) >= 2,
            "ret 总数 " + (sc == null ? 0 : sc.Body.Instructions.Count(i => i.OpCode == OpCodes.Ret)) + "（期望 >=2）");
        var sm = Need("GameManager", "SetCoolingTimeMax");
        A("cdMaxList 归零门落在 SetCoolingTimeMax", Cnt(sm, K_SKILL) == 1, K_SKILL + " 出现 " + Cnt(sm, K_SKILL) + " 次（期望 1）");
        int fu = 0;
        foreach (var ty in mm.Types)
            if (ty.HasMethods)
                foreach (var meth in ty.Methods)
                    if (meth.HasBody)
                        fu += meth.Body.Instructions.Count(i => i.Operand is MethodReference r && r.Name == "ForceUnlock");
        A("解锁角色：ForceUnlock 覆盖全部 isUnlock 读取点", fu >= 4, "调用点 " + fu + " 处（期望 >=4）");
        var hp = mm.GetType("Starry.HkldPatch");
        A("Starry.HkldPatch.ForceUnlock 已注入",
            hp != null && hp.Methods.Any(x => x.Name == "ForceUnlock" && x.Parameters.Count == 1), "类型/方法存在");
        var pcp = mm.GetType("PlayerChoicePanel");
        A("PlayerChoicePanel.UpdateCharacter 已接闸",
            pcp != null && pcp.Methods.Any(x => x.Name == "UpdateCharacter"
                && x.Body.Instructions.Any(i => i.Operand is MethodReference r2 && r2.Name == "ForceUnlock")),
            "UpdateCharacter 内含 ForceUnlock 调用");
        var rm = Need("PlayerManager", "RefreshMoney");
        A("金币门落在 RefreshMoney", Cnt(rm, K_MONEY) == 1, K_MONEY + " 出现 " + Cnt(rm, K_MONEY) + " 次（期望 1）");
        A("金币同时写 curMoney+lastMoney（防邪神7 差值扣血）",
            rm != null && rm.Body.Instructions.Count(i => i.OpCode == OpCodes.Stfld) >= 2,
            "新块 stfld 数 " + (rm == null ? 0 : rm.Body.Instructions.Take(12).Count(i => i.OpCode == OpCodes.Stfld)));
        var rk = Need("PlayerManager", "RefreshKeyCount");
        A("钥匙门落在 RefreshKeyCount", Cnt(rk, K_KEY) == 1, K_KEY + " 出现 " + Cnt(rk, K_KEY) + " 次（期望 1）");

        // 无限蓝：调用点计数 + 落点 + helper 语义复读
        var rmp = Need("PlayerManager", "RefreshMP");
        int mpSites = 0;
        MethodDefinition mpHost = null;
        foreach (var ty in mm.Types)
            if (ty.HasMethods)
                foreach (var meth in ty.Methods)
                    if (meth.HasBody)
                    {
                        int c = meth.Body.Instructions.Count(i => i.Operand is MethodReference r0 && r0.Name == "MpFill");
                        if (c > 0) { mpSites += c; mpHost = meth; }
                    }
        A("无限蓝：MpFill 调用点 == 1 处", mpSites == 1, "调用点 " + mpSites + " 处（期望 1）");
        A("无限蓝：调用落在 PlayerManager.RefreshMP 入口处",
            mpHost == rmp && rmp != null
              && rmp.Body.Instructions.Take(2).Any(i => i.Operand is MethodReference r1 && r1.Name == "MpFill"),
            "宿主 " + (mpHost == null ? "(无)" : mpHost.FullName) + "；RefreshMP 前 2 条 IL 内含 MpFill");
        var mpFill = hp?.Methods.FirstOrDefault(x => x.Name == "MpFill");
        A("Starry.HkldPatch.MpFill 签名 (PlayerManager)->Void",
            mpFill != null && mpFill.Parameters.Count == 1
              && mpFill.Parameters[0].ParameterType.Name == "PlayerManager"
              && mpFill.ReturnType.Name == "Void",
            "helper 存在且签名正确");
        A("MpFill 语义 = if(inf_mp) pm.curMp = pm.mp",
            mpFill != null && Cnt(mpFill, K_INFMP) == 1
              && mpFill.Body.Instructions.Any(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference g && g.Name == "GetBool")
              && mpFill.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference fa && fa.Name == "mp")
              && mpFill.Body.Instructions.Any(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference fb && fb.Name == "curMp"),
            "ldstr " + K_INFMP + " + GetBool + ldfld mp + stfld curMp 齐备");

        // 移动速度：调用点计数 == ret 数（实测打印）+ 逐 ret 前置校验 + helper 语义复读
        var gm = Need("PlayerManager", "GetRealMoveSpeed");
        var gins = gm == null ? new List<Instruction>() : gm.Body.Instructions.ToList();
        int gmRets = gins.Count(i => i.OpCode == OpCodes.Ret);
        int gmCalls = gins.Count(i => i.Operand is MethodReference r5 && r5.Name == "MoveMul");
        A("移动速度：MoveMul 调用点 == GetRealMoveSpeed 的 ret 数",
            gmRets > 0 && gmCalls == gmRets, "调用点 " + gmCalls + " / 实测 ret " + gmRets);
        bool everyRetCovered = gmRets > 0;
        for (int i = 0; i < gins.Count; i++)
        {
            if (gins[i].OpCode != OpCodes.Ret) continue;
            if (i == 0 || !(gins[i - 1].Operand is MethodReference r6) || r6.Name != "MoveMul") everyRetCovered = false;
        }
        A("移动速度：每个 ret 紧前一条都是 MoveMul（两条 return 都接）", everyRetCovered,
            "逐 ret 前置校验 " + (everyRetCovered ? "全通过" : "有 ret 未接 MoveMul"));
        var moveMul = hp?.Methods.FirstOrDefault(x => x.Name == "MoveMul");
        A("Starry.HkldPatch.MoveMul 签名 (Single)->Single",
            moveMul != null && moveMul.Parameters.Count == 1
              && moveMul.Parameters[0].ParameterType.Name == "Single"
              && moveMul.ReturnType.Name == "Single",
            "helper 存在且签名正确");
        A("MoveMul 语义 = x<=100 原值 : v * move_speed_x / 100（含上下界夹取）",
            moveMul != null && Cnt(moveMul, K_MOVESP) == 1
              && moveMul.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4 && (int)i.Operand == MOVESP_DEF)
              && moveMul.Body.Instructions.Any(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference g2 && g2.Name == "GetInt")
              && moveMul.Body.Instructions.Any(i => i.OpCode == OpCodes.Ble)
              && moveMul.Body.Instructions.Any(i => i.OpCode == OpCodes.Bge)
              && moveMul.Body.Instructions.Any(i => i.OpCode == OpCodes.Conv_R4)
              && moveMul.Body.Instructions.Any(i => i.OpCode == OpCodes.Mul)
              && moveMul.Body.Instructions.Any(i => i.OpCode == OpCodes.Div),
            "GetInt(def=" + MOVESP_DEF + ") + ble/bge + conv.r4 + mul + div 齐备");
        // 防回归：这类「栈合法但操作数选错」的 bug 前几条断言抓不到 ⇒ 直接查「ldc.r4 1 前一条」
        var moveIns = moveMul == null ? new List<Instruction>() : moveMul.Body.Instructions.ToList();
        int iMin = moveIns.FindIndex(x => x.OpCode == OpCodes.Ldc_R4 && x.Operand is float f1 && f1 == MOVESP_MIN);
        A("移动速度：MoveMul 下界夹比较的是 r（ldc.r4 1 前一条必须是 ldloc）",
            iMin > 0 && moveIns[iMin - 1].OpCode == OpCodes.Ldloc,
            iMin <= 0 ? "未找到 ldc.r4 " + MOVESP_MIN
                      : "前一条 = " + moveIns[iMin - 1].OpCode + "（期望 ldloc）");
        A("移动速度：GetRealMoveSpeed 方法体内无 stfld（未改成写属性字段）",
            gm != null && !gins.Any(i => i.OpCode == OpCodes.Stfld),
            "stfld 数 " + gins.Count(i => i.OpCode == OpCodes.Stfld) + "（期望 0）");

        // 反例断言：绝不能落到「Clamp 之后的 SetSkillCoolingTime 内」以外的错误点上
        A("SetSkillCoolingTime 的原 Clamp 指令仍在（未删改原 IL）",
            sc != null && sc.Body.Instructions.Any(i => i.Operand is MethodReference mr && mr.Name == "Clamp"),
            "Mathf.Clamp 调用仍在");
        A("SaveDataManager.UnlockNewCharacter 本体未被改动",
            mm.GetType("SaveDataManager")?.Methods.FirstOrDefault(x => x.Name == "UnlockNewCharacter")?.Body
              .Instructions.Any(i => i.OpCode == OpCodes.Ldstr) != true,
            "UnlockNewCharacter 内无新增 ldstr");
    }

    // ============================== 工具 ==============================
    static void P(Action f, string name)
    {
        f();
        nPatch++;
        Console.WriteLine("[patch] " + name);
    }

    static TypeDefinition NeedType(string full)
    {
        var t = mod.GetType(full);
        if (t == null) throw new Exception("类型缺失: " + full);
        return t;
    }

    static MethodDefinition NeedMethod(TypeDefinition t, string name, int argc)
    {
        var m = t.Methods.FirstOrDefault(x => x.Name == name && x.Parameters.Count == argc);
        if (m == null) throw new Exception("方法缺失: " + t.FullName + "::" + name + "/" + argc);
        if (!m.HasBody) throw new Exception("方法无 IL 体: " + t.FullName + "::" + name);
        return m;
    }

    // 取或建 Starry.HkldPatch 辅助类型（所有新 helper 都落这里）
    static TypeDefinition GetOrCreateHkldType()
    {
        var t = mod.GetType("Starry.HkldPatch");
        if (t != null) return t;
        t = new TypeDefinition("Starry", "HkldPatch",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            mod.TypeSystem.Object);
        mod.Types.Add(t);
        return t;
    }

    // 字段名 + 类型双断言（跨类读写字段时字段丢了会静默出 nullref，必须先断）
    static FieldDefinition NeedField(TypeDefinition t, string name, string typeName)
    {
        var f = t.Fields.FirstOrDefault(x => x.Name == name);
        if (f == null) throw new Exception("字段缺失: " + t.FullName + "::" + name);
        if (f.FieldType.Name != typeName)
            throw new Exception(t.FullName + "::" + name + " 类型=" + f.FieldType.Name + " 期望 " + typeName);
        return f;
    }

    // 参数名 + 参数类型双断言（hotfix 清单第 5 项：错位是静默的，签名合法/IL 合法/运行时才炸）
    static void AssertParams(MethodDefinition m, string[] names, string[] typeNames)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (i >= m.Parameters.Count) throw new Exception(m.FullName + " 参数个数不足");
            var p = m.Parameters[i];
            if (p.Name != names[i])
                throw new Exception(m.FullName + " Parameters[" + i + "].Name=" + p.Name + " 期望 " + names[i]);
            if (p.ParameterType.Name != typeNames[i])
                throw new Exception(m.FullName + " Parameters[" + i + "] 类型=" + p.ParameterType.Name + " 期望 " + typeNames[i]);
        }
    }

    static bool HasLdstr(MethodDefinition m, string s)
        => m.HasBody && m.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == s);

    static IEnumerable<string> RefDirs(string srcPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(srcPath));
        yield return Path.Combine(dir, "refs");
        yield return "/Users/aliang/Documents/nixiang/06_dumped/汉克旅店/il2cpp_out/DummyDll";
        var env = Environment.GetEnvironmentVariable("HKLD_REF_DIR");
        if (!string.IsNullOrEmpty(env)) yield return env;
    }

    static AssemblyNameReference Corlib()
    {
        foreach (var want in new[] { "mscorlib", "System.Runtime", "netstandard", "System.Private.CoreLib" })
        {
            var r = mod.AssemblyReferences.FirstOrDefault(x => x.Name == want);
            if (r != null) return r;
        }
        throw new Exception("找不到 corlib 引用: " + string.Join(",", mod.AssemblyReferences.Select(x => x.Name)));
    }
}
