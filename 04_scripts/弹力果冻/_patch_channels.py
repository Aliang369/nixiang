import io, sys

p = "mod菜单/starry-mod-menu/app/src/main/jni/Games/TlgdTapTapMod.cpp"
s = io.open(p, encoding="utf-8").read()

def rep(old, new):
    global s
    assert s.count(old) == 1, ("NO/MULTI MATCH: " + old[:80] + " count=%d" % s.count(old))
    s = s.replace(old, new)

# ---------- 1) WriteI64 助手 ----------
rep("""static inline int64_t ReadI64(void* p, size_t o) {
    int64_t v = 0; memcpy(&v, (const void*)((uintptr_t)p + o), 8); return v;
}""",
"""static inline int64_t ReadI64(void* p, size_t o) {
    int64_t v = 0; memcpy(&v, (const void*)((uintptr_t)p + o), 8); return v;
}
static inline void WriteI64(void* p, size_t o, int64_t v) {
    memcpy((void*)((uintptr_t)p + o), &v, 8);
}""")

# ---------- 2) 新增类/偏移 ----------
rep("""static size_t offSdRunner = 0, offScCurSkill = 0, offScSkills = 0;""",
"""static size_t offSdRunner = 0, offScCurSkill = 0, offScSkills = 0;
// ---- 联机(代理)流的三条真值通道 ----
// 标准战斗走 NetableBattleManager（它把 MovmentDriver.useAi 置 false），血/盾/移速由代理缓存驱动；
// 只写本地属性字段会被缓存覆盖 ⇒ 必须同时写这两层缓存（都是普通字段，仍是零 hook/零 invoke）。
static void*  g_clsRemoteCache  = nullptr;  // BattleAttributeCache（float 型远端属性缓存）
static void*  g_clsUnitHpCache  = nullptr;  // UnitHpCache（long 型代理血量缓存）
static void*  g_clsProxy        = nullptr;  // Proxy
static void*  g_clsCacheSystem  = nullptr;  // CacheSystem
static size_t offBUARemoteCache = 0;        // BattleUnitAttribute.remoteAttributeCache
static size_t offUnitProxy      = 0;        // BattleUnit.proxy
static size_t offProxyCacheSystem = 0;      // Proxy.cacheSystem
static size_t offCacheDic       = 0;        // CacheSystem.cacheDic : Dictionary<Type,Cache>
static size_t offRAHp = 0, offRAFullHp = 0, offRAShield = 0, offRAFullShield = 0, offRAMoveSpeed = 0;
static size_t offHcHp = 0, offHcFullHp = 0, offHcShield = 0, offHcFullShield = 0;""")

# ---------- 3) 计数器 ----------
rep("""static std::atomic<uint32_t> g_wKill{0}, g_wAoe{0}, g_wEs{0},""",
"""static std::atomic<uint32_t> g_rcSeen{0}, g_hcSeen{0}, g_hcW{0};   // 远端缓存命中/代理缓存命中/缓存写入
static std::atomic<uint32_t> g_wKill{0}, g_wAoe{0}, g_wEs{0},""")

# ---------- 4) 通道取用助手 ----------
rep("""// il2cpp 集合布局约定：List<T> _items@0x10 / _size@0x18；数组 data@0x20 / max_length@0x18""",
"""// 远端属性缓存（float 型）：代理流真值之一
static void* RemoteAttrCacheOf(void* unit) {
    if (!offBUARemoteCache) return nullptr;
    void* ua = ReadPtr(unit, offUnitAttr);
    if (!PtrOk(ua)) return nullptr;
    void* rc = ReadPtr(ua, offBUARemoteCache);
    return PtrOk(rc) ? rc : nullptr;
}

// 代理血量缓存（long 型）：Proxy.cacheSystem.cacheDic 中按"值的具体类"认出 UnitHpCache
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
    const int32_t cap = ArrLen(entries);
    if (!PtrOk(entries) || cnt <= 0 || cnt > 64 || cap < cnt) return nullptr;
    for (int32_t i = 0; i < cnt; i++) {
        void* e = (void*)((uintptr_t)entries + 0x20 + (size_t)i * 24);
        void* v = ReadPtr(e, 0x10);
        if (PtrOk(v) && ClassOf(v) == g_clsUnitHpCache) return v;
    }
    return nullptr;
}

// il2cpp 集合布局约定：List<T> _items@0x10 / _size@0x18；数组 data@0x20 / max_length@0x18""")

# ---------- 5) Resolve() 解析 ----------
rep("""    if (g_clsSkillDriver)    offSdRunner = api.FieldOffset(g_clsSkillDriver, "runner");""",
"""    // 代理流通道：BattleUnit.proxy → Proxy.cacheSystem → CacheSystem.cacheDic
    g_clsProxy       = FindClassAnyImage("Proxy");
    g_clsCacheSystem = FindClassAnyImage("CacheSystem");
    g_clsUnitHpCache = FindClassAnyImage("UnitHpCache");
    g_clsRemoteCache = FindClassAnyImage("BattleAttributeCache");
    offBUARemoteCache = api.FieldOffset(g_clsBUAttr, "remoteAttributeCache");
    if (g_clsUnit)        offUnitProxy        = api.FieldOffset(g_clsUnit, "proxy");
    if (g_clsProxy)       offProxyCacheSystem = api.FieldOffset(g_clsProxy, "cacheSystem");
    if (g_clsCacheSystem) offCacheDic         = api.FieldOffset(g_clsCacheSystem, "cacheDic");
    if (g_clsRemoteCache) {
        offRAHp         = api.FieldOffset(g_clsRemoteCache, "Hp");
        offRAFullHp     = api.FieldOffset(g_clsRemoteCache, "FullHp");
        offRAShield     = api.FieldOffset(g_clsRemoteCache, "Shield");
        offRAFullShield = api.FieldOffset(g_clsRemoteCache, "FullShield");
        offRAMoveSpeed  = api.FieldOffset(g_clsRemoteCache, "MoveSpeed");
    }
    if (g_clsUnitHpCache) {
        offHcHp         = api.FieldOffset(g_clsUnitHpCache, "Hp");
        offHcFullHp     = api.FieldOffset(g_clsUnitHpCache, "FullHp");
        offHcShield     = api.FieldOffset(g_clsUnitHpCache, "Shield");
        offHcFullShield = api.FieldOffset(g_clsUnitHpCache, "FullShield");
    }
    if (g_clsSkillDriver)    offSdRunner = api.FieldOffset(g_clsSkillDriver, "runner");""")

# ---------- 6) 解析日志 ----------
rep("""    LOGI(TLGD_TAG "   技能链 sdriver=0x%zx""",
"""    LOGI(TLGD_TAG "   代理通道 proxy=0x%zx cacheSystem=0x%zx cacheDic=0x%zx remoteCache=0x%zx "
                  "(cls proxy=%p cachesys=%p hp=%p ra=%p)",
         offUnitProxy, offProxyCacheSystem, offCacheDic, offBUARemoteCache,
         g_clsProxy, g_clsCacheSystem, g_clsUnitHpCache, g_clsRemoteCache);
    LOGI(TLGD_TAG "   远端缓存 RACache hp=0x%zx full=0x%zx shield=0x%zx fshield=0x%zx spd=0x%zx | "
                  "代理缓存 HpCache hp=0x%zx full=0x%zx shield=0x%zx fshield=0x%zx",
         offRAHp, offRAFullHp, offRAShield, offRAFullShield, offRAMoveSpeed,
         offHcHp, offHcFullHp, offHcShield, offHcFullShield);
    LOGI(TLGD_TAG "   技能链 sdriver=0x%zx""")

# ---------- 7) ApplyEnemy：追加两条缓存通道 ----------
rep("""    const float hp = ObscRead(av.obj, av.hp);
    if (hp <= kOneHp) return;                   // 已是 1 血（或异常值），不重复写
    ObscWrite(av.obj, av.hp, kOneHp);            // → 下一次伤害结算后 isDead → onDeadEvent → OnDead
    if (aoe) g_wAoe.fetch_add(1); else g_wKill.fetch_add(1);
}""",
"""    const float hp = ObscRead(av.obj, av.hp);
    if (hp > kOneHp) {
        ObscWrite(av.obj, av.hp, kOneHp);        // 通道 A：本地属性 → 伤害结算后 isDead → onDeadEvent → OnDead
        if (aoe) g_wAoe.fetch_add(1); else g_wKill.fetch_add(1);
    }

    // 通道 B：远端 float 属性缓存（代理流读它）
    if (void* rc = RemoteAttrCacheOf(unit)) {
        g_rcSeen.fetch_add(1);
        if (offRAHp)        WriteF32(rc, offRAHp, kOneHp);
        if (aoe && offRAShield) WriteF32(rc, offRAShield, 0.f);
    }
    // 通道 C：代理 long 血量缓存
    if (void* hc = UnitHpCacheOf(unit)) {
        g_hcSeen.fetch_add(1);
        if (offHcHp)            { WriteI64(hc, offHcHp, 1); g_hcW.fetch_add(1); }
        if (aoe && offHcShield) { WriteI64(hc, offHcShield, 0); g_hcW.fetch_add(1); }
    }
}""")

# ---------- 8) 无敌：追加两条缓存通道 ----------
rep("""        if (av.fullHp) {
            const float hp = ObscRead(av.obj, av.hp);
            const float full = ObscRead(av.obj, av.fullHp);
            if (full > 0.f && hp < full) { ObscWrite(av.obj, av.hp, full); g_wInvHp.fetch_add(1); }
        }
    }""",
"""        if (av.fullHp) {
            const float hp = ObscRead(av.obj, av.hp);
            const float full = ObscRead(av.obj, av.fullHp);
            if (full > 0.f && hp < full) { ObscWrite(av.obj, av.hp, full); g_wInvHp.fetch_add(1); }
        }
        // 通道 B/C：代理流的护盾真值（只写本地属性会被缓存覆盖）
        if (void* rc = RemoteAttrCacheOf(unit)) {
            g_rcSeen.fetch_add(1);
            if (offRAShield)     WriteF32(rc, offRAShield, kBigShield);
            if (offRAFullShield) WriteF32(rc, offRAFullShield, kBigShield);
            if (offRAHp && offRAFullHp) WriteF32(rc, offRAHp, ReadF32(rc, offRAFullHp));
        }
        if (void* hc = UnitHpCacheOf(unit)) {
            g_hcSeen.fetch_add(1);
            if (offHcShield)     WriteI64(hc, offHcShield, (int64_t)kBigShield);
            if (offHcFullShield) WriteI64(hc, offHcFullShield, (int64_t)kBigShield);
            if (offHcHp && offHcFullHp) WriteI64(hc, offHcHp, ReadI64(hc, offHcFullHp));
            g_hcW.fetch_add(1);
        }
    }""")

# ---------- 9) 移速：追加远端缓存通道 ----------
rep("""        if (cur > 0.f && std::fabs(cur - want) > 0.001f) {
            ObscWrite(av.obj, av.moveSpeed, want);
            g_spdLast = want;
            g_wSpd.fetch_add(1);
        }
        g_speedHeroAttr = av.obj;""",
"""        if (cur > 0.f && std::fabs(cur - want) > 0.001f) {
            ObscWrite(av.obj, av.moveSpeed, want);
            g_spdLast = want;
            g_wSpd.fetch_add(1);
        }
        g_speedHeroAttr = av.obj;
        // 通道 B：远端缓存里的移速（RigidMovement 读 unitAttr.MoveSpeed，属性可能由该缓存重建）
        if (void* rc = RemoteAttrCacheOf(unit)) {
            g_rcSeen.fetch_add(1);
            if (offRAMoveSpeed) WriteF32(rc, offRAMoveSpeed, want);
        }""")

# ---------- 10) 自检：逐单位通道对比 ----------
rep("""            LOGI(TLGDF "   单位[%d] %s team=%d eid=%lld attrType=%d(%s) hp=%.1f/%.1f shield=%.1f move=%.2f",""",
"""            {
                void* rc = RemoteAttrCacheOf(u);
                void* hc = UnitHpCacheOf(u);
                LOGI(TLGDF "      通道[%d] proxy=%p 远端缓存hp=%.1f/%.1f 代理缓存hp=%lld/%lld shield=%lld",
                     i, ReadPtr(u, offUnitProxy),
                     rc && offRAHp ? ReadF32(rc, offRAHp) : -1.f,
                     rc && offRAFullHp ? ReadF32(rc, offRAFullHp) : -1.f,
                     (long long)(hc && offHcHp ? ReadI64(hc, offHcHp) : -1),
                     (long long)(hc && offHcFullHp ? ReadI64(hc, offHcFullHp) : -1),
                     (long long)(hc && offHcShield ? ReadI64(hc, offHcShield) : -1));
            }
            LOGI(TLGDF "   单位[%d] %s team=%d eid=%lld attrType=%d(%s) hp=%.1f/%.1f shield=%.1f move=%.2f",""")

# ---------- 11) 状态日志 ----------
rep("""              "敌盾清=%u 锁血=%u 护盾=%u 技能=%u 位移=%u | 无属性单位=%u | 战场 units=%u 我方=%u 敌方=%u",""",
"""              "敌盾清=%u 锁血=%u 护盾=%u 技能=%u 位移=%u | 缓存 rc=%u hc=%u hc写=%u | 无属性单位=%u | "
              "战场 units=%u 我方=%u 敌方=%u",""")
rep("""         g_wInvHp.load(), g_wInvSh.load(), g_wCd.load(), g_wSpd.load(),
         g_noAttr.load(),""",
"""         g_wInvHp.load(), g_wInvSh.load(), g_wCd.load(), g_wSpd.load(),
         g_rcSeen.load(), g_hcSeen.load(), g_hcW.load(),
         g_noAttr.load(),""")

io.open(p, "w", encoding="utf-8").write(s)
print("✅ 三条真值通道已接入：通道A 本地属性 / 通道B 远端 float 缓存 / 通道C 代理 long 缓存")
print("   计数器：rc(远端缓存命中) hc(代理缓存命中) hc写")
