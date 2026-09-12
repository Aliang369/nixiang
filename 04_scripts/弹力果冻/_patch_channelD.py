import io

p = "mod菜单/starry-mod-menu/app/src/main/jni/Games/TlgdTapTapMod.cpp"
s = io.open(p, encoding="utf-8").read()

def rep(old, new):
    global s
    assert s.count(old) == 1, ("NO/MULTI: " + old[:70] + " n=%d" % s.count(old))
    s = s.replace(old, new)

# ---------- 1) 新增类/偏移（通道D：InfoCache.attribute） ----------
rep("""static size_t offRAHp = 0, offRAFullHp = 0, offRAShield = 0, offRAFullShield = 0, offRAMoveSpeed = 0;
static size_t offHcHp = 0, offHcFullHp = 0, offHcShield = 0, offHcFullShield = 0;""",
"""static void*  g_clsHeroInfoCache = nullptr; // BattleHeroInfoCache（含 attribute : BattleAttributeCache）
static void*  g_clsPetInfoCache  = nullptr; // BattlePetInfoCache
static size_t offHeroInfoAttr = 0, offPetInfoAttr = 0;   // InfoCache.attribute
static size_t offRAHp = 0, offRAFullHp = 0, offRAShield = 0, offRAFullShield = 0, offRAMoveSpeed = 0;
static size_t offHcHp = 0, offHcFullHp = 0, offHcShield = 0, offHcFullShield = 0;""")

# ---------- 2) 计数器改名：rc → 属性缓存命中 ac ----------
rep("static std::atomic<uint32_t> g_rcSeen{0}, g_hcSeen{0}, g_hcW{0};",
    "static std::atomic<uint32_t> g_acSeen{0}, g_hcSeen{0}, g_hcW{0};")

# ---------- 3) 缓存遍历重构：CacheDictOf / CacheValueByClass / UnitHpCacheOf / AttrCacheOf ----------
old_walk = """// 代理血量缓存（long 型）：Proxy.cacheSystem.cacheDic 中按"值的具体类"认出 UnitHpCache
// Dictionary<Type,Cache> 布局约定：entries@0x18 / count@0x20 / entry 24B / value@+0x10
static void* UnitHpCacheOf(void* unit) {
    if (!g_clsUnitHpCache || !offUnitProxy || !offProxyCacheSystem || !offCacheDic) return nullptr;
    void* px = ReadPtr(unit, offUnitProxy);
    if (!PtrOk(px)) return nullptr;
    void* cs = ReadPtr(px, offProxyCacheSystem);
    if (!PtrOk(cs)) return nullptr;
    void* dict = ReadPtr(cs, offCacheDic);
    if (!PtrOk(dict)) return nullptr;
    void* entries = ReadPtr(dict, 0x18);
    const int32_t cnt = ReadI32(dict, 0x20);
    const int32_t cap = entries ? ReadI32(entries, 0x18) : 0;   // 数组 max_length（此处早于 ArrLen 定义）
    if (!PtrOk(entries) || cnt <= 0 || cnt > 64 || cap < cnt) return nullptr;
    for (int32_t i = 0; i < cnt; i++) {
        void* e = (void*)((uintptr_t)entries + 0x20 + (size_t)i * 24);
        void* v = ReadPtr(e, 0x10);
        if (PtrOk(v) && ClassOf(v) == g_clsUnitHpCache) return v;
    }
    return nullptr;
}"""
new_walk = """// 走通道：BattleUnit.proxy → Proxy.cacheSystem → CacheSystem.cacheDic（Dictionary<Type,Cache>）
// 返回 entry 数（<0 = 链路不通），dict 由 outDict 带回。布局约定：entries@0x18 / count@0x20 / entry 24B / value@+0x10
static int32_t CacheDictOf(void* unit, void** outDict) {
    if (outDict) *outDict = nullptr;
    if (!offUnitProxy || !offProxyCacheSystem || !offCacheDic) return -1;
    void* px = ReadPtr(unit, offUnitProxy);
    if (!PtrOk(px)) return -1;
    void* cs = ReadPtr(px, offProxyCacheSystem);
    if (!PtrOk(cs)) return -1;
    void* dict = ReadPtr(cs, offCacheDic);
    if (!PtrOk(dict)) return -1;
    const int32_t cnt = ReadI32(dict, 0x20);
    void* entries = ReadPtr(dict, 0x18);
    const int32_t cap = entries ? ReadI32(entries, 0x18) : 0;
    if (!PtrOk(entries) || cnt < 0 || cnt > 64 || cap < cnt) return -1;
    if (outDict) *outDict = dict;
    return cnt;
}

// 按"值的具体类"从 cacheDic 取缓存对象（绕开 Type 键，不需要构造反射 Type）
static void* CacheValueByClass(void* unit, void* klass) {
    if (!klass) return nullptr;
    void* dict = nullptr;
    const int32_t cnt = CacheDictOf(unit, &dict);
    if (cnt <= 0 || !dict) return nullptr;
    void* entries = ReadPtr(dict, 0x18);
    for (int32_t i = 0; i < cnt; i++) {
        void* e = (void*)((uintptr_t)entries + 0x20 + (size_t)i * 24);
        void* v = ReadPtr(e, 0x10);
        if (PtrOk(v) && ClassOf(v) == klass) return v;
    }
    return nullptr;
}

// 通道 C：UnitHpCache（long Hp/FullHp/Shield/FullShield）
static void* UnitHpCacheOf(void* unit) { return CacheValueByClass(unit, g_clsUnitHpCache); }

// 通道 D：BattleHeroInfoCache/BattlePetInfoCache.attribute（float 属性快照，游戏用它重建本地属性）
static void* AttrCacheOf(void* unit) {
    if (g_clsHeroInfoCache && offHeroInfoAttr) {
        if (void* v = CacheValueByClass(unit, g_clsHeroInfoCache)) {
            void* a = ReadPtr(v, offHeroInfoAttr);
            if (PtrOk(a)) return a;
        }
    }
    if (g_clsPetInfoCache && offPetInfoAttr) {
        if (void* v = CacheValueByClass(unit, g_clsPetInfoCache)) {
            void* a = ReadPtr(v, offPetInfoAttr);
            if (PtrOk(a)) return a;
        }
    }
    return nullptr;
}"""
rep(old_walk, new_walk)

# ---------- 4) Resolve()：解析 InfoCache 系列 ----------
rep("""    if (g_clsUnitHpCache) {
        offHcHp         = api.FieldOffset(g_clsUnitHpCache, "Hp");""",
"""    g_clsHeroInfoCache = FindClassAnyImage("BattleHeroInfoCache");
    g_clsPetInfoCache  = FindClassAnyImage("BattlePetInfoCache");
    if (g_clsHeroInfoCache) offHeroInfoAttr = api.FieldOffset(g_clsHeroInfoCache, "attribute");
    if (g_clsPetInfoCache)  offPetInfoAttr  = api.FieldOffset(g_clsPetInfoCache, "attribute");
    if (g_clsUnitHpCache) {
        offHcHp         = api.FieldOffset(g_clsUnitHpCache, "Hp");""")

# ---------- 5) 解析日志：报告 InfoCache ----------
rep("""    LOGI(TLGD_TAG "   远端缓存 RACache hp=0x%zx""",
"""    LOGI(TLGD_TAG "   属性缓存 HeroInfoCache=%p attrOff=0x%zx | PetInfoCache=%p attrOff=0x%zx",
         g_clsHeroInfoCache, offHeroInfoAttr, g_clsPetInfoCache, offPetInfoAttr);
    LOGI(TLGD_TAG "   远端缓存 RACache hp=0x%zx""")

# ---------- 6) ApplyEnemy：通道 D 写血/盾 ----------
rep("""    // 通道 B：远端 float 属性缓存（代理流读它）
    if (void* rc = RemoteAttrCacheOf(unit)) {
        g_rcSeen.fetch_add(1);
        if (offRAHp)        WriteF32(rc, offRAHp, kOneHp);
        if (aoe && offRAShield) WriteF32(rc, offRAShield, 0.f);
    }""",
"""    // 通道 D：InfoCache.attribute（float 属性快照）—— 游戏定期用它重建本地属性，是"被改回"的来源
    if (void* ac = AttrCacheOf(unit)) {
        g_acSeen.fetch_add(1);
        if (offRAHp)     WriteF32(ac, offRAHp, kOneHp);          // cache.Hp → 重建时赋给 Hp 与 FullHp
        if (aoe && offRAShield) WriteF32(ac, offRAShield, 0.f);  // cache.Shield → 重建时赋给 Shield/FullShield
    }""")

# ---------- 7) 无敌：通道 D 写盾/血 ----------
rep("""        // 通道 B/C：代理流的护盾真值（只写本地属性会被缓存覆盖）
        if (void* rc = RemoteAttrCacheOf(unit)) {
            g_rcSeen.fetch_add(1);
            if (offRAShield)     WriteF32(rc, offRAShield, kBigShield);
            if (offRAFullShield) WriteF32(rc, offRAFullShield, kBigShield);
            if (offRAHp && offRAFullHp) WriteF32(rc, offRAHp, ReadF32(rc, offRAFullHp));
        }""",
"""        // 通道 D：属性快照（重建源）—— Shield 写满即 FullShield 也满，Hp 写 cache 值
        if (void* ac = AttrCacheOf(unit)) {
            g_acSeen.fetch_add(1);
            if (offRAShield) WriteF32(ac, offRAShield, kBigShield);
            if (offRAHp)     WriteF32(ac, offRAHp, kBigHpCache);
        }""")

# ---------- 8) 移速：通道 D 写 MoveSpeed ----------
rep("""        // 通道 B：远端缓存里的移速（RigidMovement 读 unitAttr.MoveSpeed，属性可能由该缓存重建）
        if (void* rc = RemoteAttrCacheOf(unit)) {
            g_rcSeen.fetch_add(1);
            if (offRAMoveSpeed) WriteF32(rc, offRAMoveSpeed, want);
        }""",
"""        // 通道 D：属性快照里的移速（游戏重建本地属性时会从这里覆盖回来 —— 位移计数暴涨即此现象）
        if (void* ac = AttrCacheOf(unit)) {
            g_acSeen.fetch_add(1);
            if (offRAMoveSpeed) WriteF32(ac, offRAMoveSpeed, want);
        }""")

# ---------- 9) 删掉旧的 RemoteAttrCacheOf（宿主类找错了，通道B作废）----------
old_remote = """// 远端属性缓存（float 型）：代理流真值之一
static void* RemoteAttrCacheOf(void* unit) {
    if (!offBUARemoteCache) return nullptr;
    void* ua = ReadPtr(unit, offUnitAttr);
    if (!PtrOk(ua)) return nullptr;
    void* rc = ReadPtr(ua, offBUARemoteCache);
    return PtrOk(rc) ? rc : nullptr;
}

"""
rep(old_remote, "")

# ---------- 10) 常量：无敌时给 cache 的 HP 用一个大值 ----------
rep("""static constexpr float kBigShield = 1.0e9f;  // 无敌：护盾量级（伤害先扣盾）""",
"""static constexpr float kBigShield = 1.0e9f;  // 无敌：护盾量级（伤害先扣盾）
static constexpr float kBigHpCache = 1.0e9f; // 无敌：属性快照里的 Hp（重建时同时成为 FullHp）""")

# ---------- 11) 探针 + 实时监视（进战斗后取样，含 cache 类名） ----------
rep("""// il2cpp 集合布局约定：List<T> _items@0x10 / _size@0x18；数组 data@0x20 / max_length@0x18""",
"""// 逐单位通道探针：把 proxy→cacheSystem→cacheDic 的内容（含每个缓存的类名）打出来
static void ProbeUnit(int32_t idx, void* u) {
    const char* cn = Il2Cpp::GetApi().ClassName(u);
    void* px = offUnitProxy ? ReadPtr(u, offUnitProxy) : nullptr;
    void* cs = (PtrOk(px) && offProxyCacheSystem) ? ReadPtr(px, offProxyCacheSystem) : nullptr;
    void* dict = nullptr;
    const int32_t cnt = CacheDictOf(u, &dict);
    LOGI(TLGDF " 探针[%d] %s proxy=%p cs=%p dict=%p cnt=%d",
         idx, cn ? cn : "?", px, cs, dict, cnt);
    if (cnt <= 0 || !dict) return;
    void* entries = ReadPtr(dict, 0x18);
    for (int32_t i = 0; i < cnt && i < 8; i++) {
        void* e = (void*)((uintptr_t)entries + 0x20 + (size_t)i * 24);
        void* v = ReadPtr(e, 0x10);
        if (!PtrOk(v)) continue;
        const char* vc = Il2Cpp::GetApi().ClassName(v);
        void* attr = nullptr;
        if (g_clsHeroInfoCache && ClassOf(v) == g_clsHeroInfoCache && offHeroInfoAttr) attr = ReadPtr(v, offHeroInfoAttr);
        else if (g_clsPetInfoCache && ClassOf(v) == g_clsPetInfoCache && offPetInfoAttr) attr = ReadPtr(v, offPetInfoAttr);
        const bool isHp = g_clsUnitHpCache && ClassOf(v) == g_clsUnitHpCache;
        LOGI(TLGDF "   cache[%d] %-22s%s%s HpCache(hp=%lld shield=%lld) AttrCache(hp=%.1f spd=%.2f shield=%.1f)",
             i, vc ? vc : "?", isHp ? " [HpCache]" : "", attr ? " [AttrCache]" : "",
             (long long)(isHp && offHcHp ? ReadI64(v, offHcHp) : -1),
             (long long)(isHp && offHcShield ? ReadI64(v, offHcShield) : -1),
             attr && offRAHp ? ReadF32(attr, offRAHp) : -1.f,
             attr && offRAMoveSpeed ? ReadF32(attr, offRAMoveSpeed) : -1.f,
             attr && offRAShield ? ReadF32(attr, offRAShield) : -1.f);
    }
}

// 实时监视：本地属性 vs 两层缓存，判断"我们写的值是否被游戏改回"
static void MonitorUnit(const char* who, void* u) {
    if (!PtrOk(u)) return;
    AttrView av;
    if (!ResolveAttr(u, av)) { LOGI(TLGDF " 监视 %s 属性不可用", who); return; }
    void* ac = AttrCacheOf(u);
    void* hc = UnitHpCacheOf(u);
    LOGI(TLGDF " 监视 %s 本地(hp=%.1f/%.1f shield=%.1f spd=%.2f) | AttrCache(hp=%.1f shield=%.1f spd=%.2f) | "
               "HpCache(hp=%lld/%lld shield=%lld)",
         who,
         ObscRead(av.obj, av.hp), av.fullHp ? ObscRead(av.obj, av.fullHp) : -1.f,
         av.shield ? ObscRead(av.obj, av.shield) : -1.f,
         av.moveSpeed ? ObscRead(av.obj, av.moveSpeed) : -1.f,
         ac && offRAHp ? ReadF32(ac, offRAHp) : -1.f,
         ac && offRAShield ? ReadF32(ac, offRAShield) : -1.f,
         ac && offRAMoveSpeed ? ReadF32(ac, offRAMoveSpeed) : -1.f,
         (long long)(hc && offHcHp ? ReadI64(hc, offHcHp) : -1),
         (long long)(hc && offHcFullHp ? ReadI64(hc, offHcFullHp) : -1),
         (long long)(hc && offHcShield ? ReadI64(hc, offHcShield) : -1));
}

// il2cpp 集合布局约定：List<T> _items@0x10 / _size@0x18；数组 data@0x20 / max_length@0x18""")

# ---------- 12) status 日志：rc → ac ----------
rep("""              "敌盾清=%u 锁血=%u 护盾=%u 技能=%u 位移=%u | 缓存 rc=%u hc=%u hc写=%u | 无属性单位=%u | \"""",
"""              "敌盾清=%u 锁血=%u 护盾=%u 技能=%u 位移=%u | 缓存 ac=%u hc=%u hc写=%u | 无属性单位=%u | \"""")
rep("         g_rcSeen.load(), g_hcSeen.load(), g_hcW.load(),",
    "         g_acSeen.load(), g_hcSeen.load(), g_hcW.load(),")

# ---------- 13) WorkTick：探针（tick 40/140/340）+ 监视（每 25 拍） ----------
rep("""    g_seenUnits.store((uint32_t)n);
    g_seenPlayers.store(players);
    g_seenEnemies.store(enemies);""",
"""    g_seenUnits.store((uint32_t)n);
    g_seenPlayers.store(players);
    g_seenEnemies.store(enemies);

    // 探针：进战斗后取样三次（此时属性已就绪），把 proxy/缓存结构打实
    {
        const uint32_t t = g_tick.load();
        if ((t == 40 || t == 140 || t == 340) && players + enemies > 0) {
            LOGI(TLGDF " 通道探针（tick=%u units=%d 我方=%u 敌方=%u）：", t, n, players, enemies);
            int32_t shown = 0;
            for (int32_t i = 0; i < n && shown < 3; i++) {
                void* u = ArrAt(items, i);
                if (!PtrOk(u)) continue;
                const int32_t tm = ReadI32(u, offEntTeam);
                if (tm != kTeamPlayer && tm != kTeamEnemy) continue;
                ProbeUnit(i, u);
                shown++;
            }
        }
        // 实时监视：每 25 拍打印"第一个我方 + 第一个敌方"的本地值 vs 缓存值
        if ((t % 25) == 0) {
            void* pu = nullptr; void* eu = nullptr;
            for (int32_t i = 0; i < n; i++) {
                void* u = ArrAt(items, i);
                if (!PtrOk(u)) continue;
                const int32_t tm = ReadI32(u, offEntTeam);
                if (!pu && tm == kTeamPlayer) pu = u;
                if (!eu && tm == kTeamEnemy) eu = u;
                if (pu && eu) break;
            }
            if (pu) MonitorUnit("我方0", pu);
            if (eu) MonitorUnit("敌方0", eu);
        }
    }""")

io.open(p, "w", encoding="utf-8").write(s)
print("✅ 通道D(InfoCache.attribute) + 探针 + 实时监视 已写入")
print("   rc→ac 改名残留:", s.count("g_rcSeen"), " RemoteAttrCacheOf 残留:", s.count("RemoteAttrCacheOf"))
