import io

p = "mod菜单/starry-mod-menu/app/src/main/jni/Games/TlgdTapTapMod.cpp"
s = io.open(p, encoding="utf-8").read()

def rep(old, new):
    global s
    assert s.count(old) == 1, ("NO/MULTI: " + old[:70] + " n=%d" % s.count(old))
    s = s.replace(old, new)

# ========== 1) 删掉全堆扫兜底（闪退根因：静态 _Stuff 为空时在活动堆上连扫） ==========
rep("""    // ② 兜底：全堆扫 BattleEntityManager 实例（只读，SafeScan 内执行）
    std::vector<void*> hits;
    Il2Cpp::SetScanThread(pthread_self());   // SEGV longjmp 仅对登记线程有效
    Il2Cpp::SafeScan([&] { api.ScanInstances(g_clsMgr, hits, 2048); });
    const uint32_t seq = g_acquire.fetch_add(1) + 1;
    for (void* m : hits) {
        if (ValidateMgr(m)) {
            g_mgr = m; g_mgrTick = g_tick.load();
            LOGI(TLGDF " manager 扫得单例=%p（候选%zu 第%u次扫描）", m, hits.size(), seq);
            return true;
        }
    }
    LOGW(TLGDF " manager 未取到（候选%zu 第%u次扫描）", hits.size(), seq);
    return false;
}""",
"""    // ② 静态为空 = 当前不在战斗（或战斗未初始化）→ **直接放弃本次**，等下一轮重试。
    // 【教训｜2026-09-12 闪退】此处原本兜底"全堆扫 BattleEntityManager 实例"，实测在
    // 主菜单（_Stuff 为空）时触发，该扫描要在活动堆上遍历约 2GB，且每 10 秒连扫，
    // 最终在 mod 线程 SIGSEGV（fault addr 0x7a000012a1，非堆指针）闪退。
    // 静态 _Stuff 在战斗内必然可取（实战验证），因此兜底扫描全面移除。
    g_acquire.fetch_add(1);
    return false;
}""")

# ========== 2) 删掉两个堆扫描探针（同类风险） ==========
start = s.index("// 【一锤定音】扫 MovmentDriver 实例")
end = s.index("// 实时监视：本地属性 vs 两层缓存")
s = s[:start] + s[end:]

rep("""                std::vector<void*> inst;
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
""", "")

# ========== 3) 无扫描的存活判据：Unity 原生对象指针 m_CachedPtr + _movmentDriver 反向指认 ==========
rep("""static void*  g_clsMovmentDriver = nullptr;  // MovmentDriver（每帧被 BattleDriver 驱动的移动组件）
static size_t offMvUnit = 0;                 // MovmentDriver.unit（可能继承自基类 → 深查找）""",
"""static void*  g_clsMovmentDriver = nullptr;  // MovmentDriver（每帧被 BattleDriver 驱动的移动组件）
static size_t offMvUnit = 0;                 // MovmentDriver.unit（可能继承自基类 → 深查找）
static size_t offObjCachedPtr = 0;           // UnityEngine.Object.m_CachedPtr（0 = 原生对象已销毁 ⇒ 死对象）
static size_t offBUMovDriver = 0;            // BattleUnit._movmentDriver（私有字段，按名可取）""")

rep("""    g_clsMovmentDriver  = FindClassAnyImage("MovmentDriver");
    if (g_clsMovmentDriver) offMvUnit = FieldOffsetDeep(g_clsMovmentDriver, "unit");""",
"""    g_clsMovmentDriver  = FindClassAnyImage("MovmentDriver");
    if (g_clsMovmentDriver) offMvUnit = FieldOffsetDeep(g_clsMovmentDriver, "unit");
    offObjCachedPtr = FieldOffsetDeep(g_clsUnit, "m_CachedPtr");   // 继承自 UnityEngine.Object
    offBUMovDriver  = FieldOffsetDeep(g_clsUnit, "_movmentDriver");""")

rep("""    LOGI(TLGD_TAG "   移动驱动 MovmentDriver=%p unitOff=0x%zx（实体更新表的判据）",
         g_clsMovmentDriver, offMvUnit);""",
"""    LOGI(TLGD_TAG "   移动驱动 MovmentDriver=%p unitOff=0x%zx | 存活判据 m_CachedPtr=0x%zx _movmentDriver=0x%zx",
         g_clsMovmentDriver, offMvUnit, offObjCachedPtr, offBUMovDriver);""")

# 探针里打印这两个判据（放在 ProbeUnit 的类名行后面）
rep("""    LOGI(TLGDF " 探针[%d] %s proxy=%p cs=%p dict=%p cnt=%d",
         idx, cn ? cn : "?", px, cs, dict, cnt);""",
"""    // 存活判据：① 原生对象指针（0 = 已销毁）② 自身移动驱动是否反向指认自己
    const uintptr_t nptr = offObjCachedPtr ? (uintptr_t)ReadPtr(u, offObjCachedPtr) : 0;
    void* mvd = offBUMovDriver ? ReadPtr(u, offBUMovDriver) : nullptr;
    void* mvdUnit = (PtrOk(mvd) && offMvUnit) ? ReadPtr(mvd, offMvUnit) : nullptr;
    LOGI(TLGDF " 探针[%d] %s proxy=%p cs=%p dict=%p cnt=%d | 原生对象=%p 移动驱动=%p 驱动指向自己=%s",
         idx, cn ? cn : "?", px, cs, dict, cnt, (void*)nptr, mvd,
         (mvdUnit == u) ? "是" : (PtrOk(mvd) ? "否" : "无驱动"));""")

io.open(p, "w", encoding="utf-8").write(s)
print("✅ 已修：移除全堆扫兜底 + 移除堆扫探针 + 加入无扫描存活判据")
print("   ScanInstances 残留:", s.count("ScanInstances"))
print("   SafeScan 残留:", s.count("SafeScan"))
print("   花括号平衡:", s.count("{") - s.count("}"))
