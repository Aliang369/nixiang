import io

p = "mod菜单/starry-mod-menu/app/src/main/jni/Games/TlgdTapTapMod.cpp"
s = io.open(p, encoding="utf-8").read()

def rep(old, new):
    global s
    assert s.count(old) == 1, ("NO/MULTI: " + old[:70] + " n=%d" % s.count(old))
    s = s.replace(old, new)

# ---------- 1) 新增 MovmentDriver 类/字段 + 深查找助手 ----------
rep("""static void*  g_clsHeroInfoCache = nullptr;""",
"""static void*  g_clsMovmentDriver = nullptr;  // MovmentDriver（每帧被 BattleDriver 驱动的移动组件）
static size_t offMvUnit = 0;                 // MovmentDriver.unit（可能继承自基类 → 深查找）
static void*  g_clsHeroInfoCache = nullptr;""")

rep("""// 类相等或为其子类（Skill 有几十个子类，相等比较会把真实技能全判掉）""",
"""// 字段偏移深查找：class_get_field_from_name 不搜基类，继承字段必须逐级往上找
static size_t FieldOffsetDeep(void* klass, const char* name) {
    auto& api = Il2Cpp::GetApi();
    for (int d = 0; d < 16 && klass; d++) {
        const size_t o = api.FieldOffset(klass, name);
        if (o) return o;
        if (!api.class_parent) break;
        klass = api.class_parent(klass);
    }
    return 0;
}

// 类相等或为其子类（Skill 有几十个子类，相等比较会把真实技能全判掉）""")

# ---------- 2) Resolve()：解析 MovmentDriver ----------
rep("""    g_clsHeroInfoCache = FindClassAnyImage("BattleHeroInfoCache");""",
"""    g_clsMovmentDriver  = FindClassAnyImage("MovmentDriver");
    if (g_clsMovmentDriver) offMvUnit = FieldOffsetDeep(g_clsMovmentDriver, "unit");
    g_clsHeroInfoCache = FindClassAnyImage("BattleHeroInfoCache");""")

rep("""    LOGI(TLGD_TAG "   属性缓存 HeroInfoCache=%p attrOff=0x%zx | PetInfoCache=%p attrOff=0x%zx",""",
"""    LOGI(TLGD_TAG "   移动驱动 MovmentDriver=%p unitOff=0x%zx（实体更新表的判据）",
         g_clsMovmentDriver, offMvUnit);
    LOGI(TLGD_TAG "   属性缓存 HeroInfoCache=%p attrOff=0x%zx | PetInfoCache=%p attrOff=0x%zx",""")

# ---------- 3) 活着单位的判据探针（扫 MovmentDriver 实例 → 读其 unit → 与 battleUnits 比指针） ----------
rep("""// 实时监视：本地属性 vs 两层缓存，判断"我们写的值是否被游戏改回\"""",
"""// 【一锤定音】扫 MovmentDriver 实例：它是每帧被 BattleDriver 驱动的东西，其 unit 就是"活着的单位"。
// 与 battleUnits 逐个比指针 —— 不一致即证明我们一直在写不参与运行的对象。
static void ProbeLiveDrivers(int32_t n, void* items) {
    if (!g_clsMovmentDriver || !offMvUnit) {
        LOGW(TLGDF " 探测跳过：MovmentDriver=%p unitOff=0x%zx", g_clsMovmentDriver, offMvUnit);
        return;
    }
    std::vector<void*> inst;
    Il2Cpp::SafeScan([&] { Il2Cpp::GetApi().ScanInstances(g_clsMovmentDriver, inst, 2048); });
    LOGI(TLGDF " 【判据】MovmentDriver 实例=%zu | battleUnits=%d", inst.size(), n);
    int32_t inList = 0, notInList = 0;
    for (size_t k = 0; k < inst.size() && k < 12; k++) {
        void* u = ReadPtr(inst[k], offMvUnit);
        bool found = false;
        for (int32_t i = 0; i < n; i++) if (ArrAt(items, i) == u) { found = true; break; }
        if (found) inList++; else notInList++;
        const char* cn = PtrOk(u) ? Il2Cpp::GetApi().ClassName(u) : "?";
        LOGI(TLGDF "   驱动[%zu] unit=%p %s team=%d eid=%lld 在battleUnits=%s",
             k, u, cn, PtrOk(u) ? ReadI32(u, offEntTeam) : -1,
             (long long)(PtrOk(u) ? ReadI64(u, offEntEid) : -1), found ? "是" : "否");
    }
    LOGI(TLGDF " 【判据】样本中 属于battleUnits=%d 不属于=%d", inList, notInList);
}

// 实时监视：本地属性 vs 两层缓存，判断"我们写的值是否被游戏改回\"""")

# ---------- 4) WorkTick：探针改为"进战斗后"触发（+20 全量 / +60 +150 仅通道） ----------
rep("""    // 探针：进战斗后取样三次（此时属性已就绪），把 proxy/缓存结构打实
    {
        const uint32_t t = g_tick.load();
        // 全堆对比：battleUnits 之外还有多少 BattleUnit 实例（若明显更多 → 我们写的不是战场那一批）
        if (t == 40) {
            std::vector<void*> inst;
            Il2Cpp::SafeScan([&] { Il2Cpp::GetApi().ScanInstances(g_clsUnit, inst, 2048); });
            LOGI(TLGDF " 全堆 BattleUnit 实例=%zu（battleUnits=%d）—— 相等才说明写的就是战场对象",
                 inst.size(), n);
            for (size_t k = 0; k < inst.size() && k < 8; k++) {
                const char* cn = Il2Cpp::GetApi().ClassName(inst[k]);
                LOGI(TLGDF "   堆实例[%zu] %s team=%d eid=%lld", k, cn ? cn : "?",
                     ReadI32(inst[k], offEntTeam), (long long)ReadI64(inst[k], offEntEid));
            }
        }
        if ((t == 40 || t == 140 || t == 340) && players + enemies > 0) {
            LOGI(TLGDF " 通道探针（tick=%u units=%d 我方=%u 敌方=%u）：", t, n, players, enemies);""",
"""    // 探针：以"进战斗那一刻"为基准触发（按启动 tick 触发会错过进战斗时机）
    {
        const uint32_t t = g_tick.load();
        if (players + enemies > 0) {
            if (g_battleTick == 0) g_battleTick = t + 20;               // 进战斗后 20 拍（约 4 秒）首探
            const uint32_t off = (t >= g_battleTick) ? (t - g_battleTick) : 0xFFFFFFFFu;
            if (off == 0) {                                             // ① 全量探针（含两次堆扫，只做一次）
                LOGI(TLGDF " 通道探针（进战斗 tick=%u units=%d 我方=%u 敌方=%u）：", t, n, players, enemies);
                for (int32_t i = 0; i < n && i < 3; i++) {
                    void* u = ArrAt(items, i);
                    if (!PtrOk(u)) continue;
                    const int32_t tm = ReadI32(u, offEntTeam);
                    if (tm != kTeamPlayer && tm != kTeamEnemy) continue;
                    ProbeUnit(i, u);
                }
                std::vector<void*> inst;
                Il2Cpp::SafeScan([&] { Il2Cpp::GetApi().ScanInstances(g_clsUnit, inst, 2048); });
                LOGI(TLGDF " 【判据】全堆 BattleUnit 实例=%zu | battleUnits=%d", inst.size(), n);
                for (size_t k = 0; k < inst.size() && k < 10; k++) {
                    const char* cn = Il2Cpp::GetApi().ClassName(inst[k]);
                    bool found = false;
                    for (int32_t i = 0; i < n; i++) if (ArrAt(items, i) == inst[k]) { found = true; break; }
                    LOGI(TLGDF "   堆实例[%zu] %p %s team=%d eid=%lld 在battleUnits=%s", k, inst[k],
                         cn ? cn : "?", ReadI32(inst[k], offEntTeam),
                         (long long)ReadI64(inst[k], offEntEid), found ? "是" : "否");
                }
                ProbeLiveDrivers(n, items);
            } else if (off == 60 || off == 150) {                        // ② 后续只探通道（不扫堆，避免卡顿）
                LOGI(TLGDF " 通道探针复测（进战斗+%u 拍 units=%d）：", off, n);
                for (int32_t i = 0; i < n && i < 3; i++) {
                    void* u = ArrAt(items, i);
                    if (!PtrOk(u)) continue;
                    const int32_t tm = ReadI32(u, offEntTeam);
                    if (tm != kTeamPlayer && tm != kTeamEnemy) continue;
                    ProbeUnit(i, u);
                }
            }
        } else {
            g_battleTick = 0;                                            // 离开战斗 → 重新武装
        }
        if (false) {
            LOGI(TLGDF " 通道探针（tick=%u units=%d 我方=%u 敌方=%u）：", t, n, players, enemies);""")

# ---------- 5) 新增 g_battleTick ----------
rep("""static std::atomic<uint32_t> g_tick{0};""",
"""static std::atomic<uint32_t> g_tick{0};
static uint32_t g_battleTick = 0;   // 进战斗基准 tick（0 = 不在战斗）""")

io.open(p, "w", encoding="utf-8").write(s)
print("✅ 已加入：进战斗触发探针 + MovmentDriver 活体判据 + FieldOffsetDeep")
print("   花括号平衡:", s.count("{") - s.count("}"))
