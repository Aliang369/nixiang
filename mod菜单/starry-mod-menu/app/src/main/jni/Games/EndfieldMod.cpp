/* ================================================================
 *  明日方舟：终末地 — 百倍伤害 / 无敌 / 无限跳跃 / 无限技能
 *  挂点语义：10_docs/终末地/抓点_四功能_20260902.md（斌哥壳按名实锤）
 *  metadata 加密 → 只走按名；默认懒装（hook-rules §16）
 * ================================================================ */

#include "EndfieldMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define EF_LOG "[Endfield]"

#if defined(STARRY_TRACE) && STARRY_TRACE
#define EF_TRACE(...) __android_log_print(ANDROID_LOG_INFO, "StarryTrace", EF_LOG " " __VA_ARGS__)
#include <android/log.h>
#else
#define EF_TRACE(...) ((void)0)
#endif

#include <mutex>
#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <pthread.h>
#include <unistd.h>

static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_jump{false};
static std::atomic<bool> g_skill{false};
static std::atomic<bool> g_reenter{false};
static std::atomic<uint32_t> g_hitsDmg{0};
static std::atomic<uint32_t> g_hitsInv{0};
static std::atomic<uint32_t> g_hitsJump{0};
static std::atomic<uint32_t> g_hitsSkill{0};

// ---- typed origs (装上哪个用哪个) ----
typedef float (*F5_t)(void*, void*, void*, void*, void*, void*);
typedef float (*F4_t)(void*, void*, void*, void*, void*);
typedef float (*F3_t)(void*, void*, void*, void*);
typedef float (*F2_t)(void*, void*, void*);
typedef float (*F1_t)(void*, void*);
typedef void (*V4_t)(void*, void*, void*, void*, void*);
typedef void (*V3_t)(void*, void*, void*, void*);
typedef void (*V2_t)(void*, void*, void*);
typedef void (*V1_t)(void*, void*);
typedef uint8_t (*B0_t)(void*, void*);   // instance bool(), + MethodInfo*
typedef uint8_t (*B1_t)(void*, void*, void*);
typedef uint8_t (*B2_t)(void*, void*, void*, void*);

static void* g_origCalc = nullptr;
static int g_calcArgc = -1;
static bool g_calcIsFloat = true;

static void* g_origApplyMod = nullptr;
static int g_applyArgc = -1;

// v14：敌我判定用原生 get_factionIndex 直调（同 klass 按名解析，纯字段读）
typedef int (*Faction_t)(void*, void*);
static Faction_t g_facNative = nullptr;
static void* g_miFaction = nullptr;

static B0_t g_origJump = nullptr;
static B0_t g_origCheckSkillCd = nullptr;
static B0_t g_origCheckCd = nullptr;
static B0_t g_origCheckCost = nullptr;
static V1_t g_origOnTick = nullptr;

// ---- v10 闪退复盘：Endfield_Tick(浮窗线程) 与 TryInit(主线程) 都跑
// SyncHooks，g_orig* 非原子 → 浮窗线程读到 null → 对同一函数重复 DobbyHook
// → fail 每 300ms 刷屏（14:21:55 起）→ 反复 patch .text → 内存损坏/触发
// TP → 毒杀（14:22:09 died）。修复：① SyncHooks 全局互斥（Install 串行化）
// ② 连续 3 次 dobby fail 熔断放弃该 hook（不再挑衅）。----
static std::mutex g_hookMtx;
static int g_failApply = 0, g_failCalc = 0;

static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

// ---- 敌我异步学习（v9）----
// ⚠️ 铁律（v6/v7/v8 闪退复盘）：主线程 hook 路径绝对禁止任何可能 SEGV 的
// 操作（解引用 hook 参数/调用 class_name/SafeProbe）——ScanSegvHandler 对
// 非登记线程直接 SIG_DFL+raise 秒杀进程。学习一律在专用线程 SafeProbe。

static std::mutex g_learnMtx;
static std::unordered_map<void*, uint8_t> g_selfKind;  // 0=pending 1=player 2=other
static std::vector<void*> g_learnQ;
static std::unordered_set<void*> g_dmgSeen;
static std::vector<void*> g_dmgQ;

// 主线程入口（零解引用）：self 敌我快查；-1=未见（放行+学习），1=玩家（拦），2=其他
static int SelfKind(void* p) {
    if (!OkPtr(p)) return 2;
    std::lock_guard<std::mutex> lk(g_learnMtx);
    auto it = g_selfKind.find(p);
    return it == g_selfKind.end() ? -1 : it->second;
}
static void EnqueueLearn(void* p) {
    std::lock_guard<std::mutex> lk(g_learnMtx);
    if (g_selfKind.size() > 512) {
        for (auto it = g_selfKind.begin(); it != g_selfKind.end();)
            it = (it->second == 2) ? g_selfKind.erase(it) : std::next(it);
    }
    if (g_selfKind.emplace(p, (uint8_t)0).second && g_learnQ.size() < 64)
        g_learnQ.push_back(p);
}
static void EnqueueDmgLearn(void* p) {
    std::lock_guard<std::mutex> lk(g_learnMtx);
    if (g_dmgSeen.size() > 256) g_dmgSeen.clear();
    if (g_dmgSeen.insert(p).second && g_dmgQ.size() < 64) g_dmgQ.push_back(p);
}
// 学习线程：读 self 自身类名 + 扫指针槽找宿主对象，按宿主类名判敌我
static const char* kPlayerKeys[] = {"Character", "Avatar", "Player"};
static const char* KlassNameAt(void* obj) {
    void* k = nullptr;
    memcpy(&k, obj, 8);
    return OkPtr(k) ? GetApi().class_name(k) : nullptr;
}
static bool LearnOne(void* self) {
    char hosts[160] = {0};
    bool isPlayer = false;
    Il2Cpp::SafeScan([&]() -> bool {
        const char* selfCn = KlassNameAt(self);  // self 自身类名
        LOGI(EF_LOG " learn self=%p selfCls=%s", self, selfCn ? selfCn : "?");
        int found = 0;
        for (size_t off = 0x08; off < 0x100 && found < 4; off += 8) {
            void* p = nullptr;
            memcpy(&p, (uint8_t*)self + off, 8);
            if (!OkPtr(p)) continue;
            const char* cn = KlassNameAt(p);
            if (!cn) continue;
            LOGI(EF_LOG " learn self=%p +0x%zx -> %s", self, off, cn);
            for (const char* key : kPlayerKeys)
                if (strstr(cn, key)) isPlayer = true;
            if (found) strlcat(hosts, ",", sizeof(hosts));
            strlcat(hosts, cn, sizeof(hosts));
            found++;
            // 二层深挖：数据类（如 AbilitySystemData）里爬宿主单位
            if (strstr(cn, "Data") || strstr(cn, "Component") || strstr(cn, "System")) {
                void* host = nullptr;
                const char* hcn = nullptr;
                for (size_t o2 = 0x08; o2 < 0x80; o2 += 8) {
                    memcpy(&host, (uint8_t*)p + o2, 8);
                    if (!OkPtr(host)) continue;
                    hcn = KlassNameAt(host);
                    if (!hcn || strstr(hcn, "Delegate") || strstr(hcn, "Data")) continue;
                    LOGI(EF_LOG " learn deep self=%p %s+%#zx -> %s", self, cn, o2, hcn);
                    for (const char* key : kPlayerKeys)
                        if (strstr(hcn, key)) isPlayer = true;
                    break;
                }
            }
        }
        return true;
    });
    uint8_t kind = isPlayer ? 1 : 2;
    {
        std::lock_guard<std::mutex> lk(g_learnMtx);
        g_selfKind[self] = kind;
    }
    LOGI(EF_LOG " learned self=%p kind=%s hosts=%s", self,
         isPlayer ? "PLAYER" : "other", hosts);
    return isPlayer;
}
// v11c：类普查（只读，学习线程 SafeScan 内执行）
static const char* kUnitNames[] = {"Character", "Unit", "Monster", "Enemy", "Actor",
                                   "Entity", "HpComponent", "UnitBase", "CharacterBase",
                                   "MonsterBase", "BattleUnit", "Damageable"};
static void DumpClassFields(const char* tag, const char* imgName, void* k, int* counter) {
    auto& api = GetApi();
    const char* cn = api.class_name(k);
    if (!cn || !api.class_fields || !api.field_name) return;
    void* iter = nullptr;
    void* f = nullptr;
    int fc = 0;
    char fl[512] = {0};
    while ((f = api.class_fields(k, &iter)) && fc < 40) {
        const char* fn = api.field_name(f);
        size_t off = api.field_offset(f);
        if (fn) {
            char one[48];
            snprintf(one, sizeof one, "%s@+0x%zx ", fn, off);
            strlcat(fl, one, sizeof fl);
            fc++;
        }
    }
    LOGI(EF_LOG " dump %s img=%s name=%s n=%d [%s]", tag, imgName, cn, fc, fl);
    (*counter)++;
}
static void ClassCensus() {
    auto& api = GetApi();
    if (!api.ready) return;
    static const char* kAsms[] = {
        "Assembly-CSharp", "Gameplay.Beyond", "AbilityUnsafe.Gameplay.Beyond",
        "DynamicSceneUnsafe.Gameplay.Beyond", "FactoryUnsafe.Gameplay.Beyond",
        "Entry.Beyond", "Base.Beyond", "Audio.Beyond", "Input.Beyond",
        "Lua.Beyond", "mscorlib"};
    void* dom = api.domain_get();
    if (!dom) return;
    int hits = 0;
    Il2Cpp::SafeScan([&]() -> bool {
        void* entityK = nullptr;
        // 第一遍：找 Entity klass + AbilitySystemData
        for (const char* an : kAsms) {
            void* as = api.domain_assembly_open(dom, an);
            if (!as) continue;
            void* img = (void*)api.assembly_image(as);
            if (!img) continue;
            uint32_t n = api.image_class_count(img);
            for (uint32_t i = 0; i < n; i++) {
                void* k = api.image_class(img, i);
                if (!k) continue;
                const char* cn = api.class_name(k);
                if (!cn) continue;
                if (!strcmp(cn, "Entity") && !entityK) entityK = k;
                if (!strcmp(cn, "AbilitySystemData")) {
                    DumpClassFields("ability-data", an, k, &hits);
                }
            }
        }
        LOGI(EF_LOG " census pass1 entity=%p hits=%d", entityK, hits);
        if (!entityK || !api.class_parent) return true;
        // 第二遍：祖先链含 Entity 的类（跨中间基类），dump 字段名含 hp/damage 的
        for (const char* an : kAsms) {
            void* as = api.domain_assembly_open(dom, an);
            if (!as) continue;
            void* img = (void*)api.assembly_image(as);
            if (!img) continue;
            uint32_t n = api.image_class_count(img);
            for (uint32_t i = 0; i < n; i++) {
                void* k = api.image_class(img, i);
                if (!k) continue;
                const char* cn = api.class_name(k);
                if (!cn) continue;
                // v11f：类名含 Hp/Health/Attribute 的类全 dump（HP 载体在此）
                char cnLow[64];
                int cl = 0;
                for (; cn[cl] && cl < 62; cl++) cnLow[cl] = (char)tolower(cn[cl]);
                cnLow[cl] = 0;
                if (!strstr(cnLow, "hp") && !strstr(cnLow, "health") && !strstr(cnLow, "attribute"))
                    continue;
                DumpClassFields("hpcls", an, k, &hits);
                if (hits > 60) return true;  // 日志限流
            }
        }
        LOGI(EF_LOG " census pass1b done hits=%d", hits);
        // pass2：AbilitySystemData 字段类型名（找属性/数值容器引用链）
        if (api.field_type && api.type_name) {
            for (const char* an : kAsms) {
                void* as = api.domain_assembly_open(dom, an);
                if (!as) continue;
                void* img = (void*)api.assembly_image(as);
                if (!img) continue;
                uint32_t n = api.image_class_count(img);
                for (uint32_t i = 0; i < n; i++) {
                    void* k = api.image_class(img, i);
                    if (!k) continue;
                    const char* cn = api.class_name(k);
                    if (!cn || strcmp(cn, "AbilitySystemData")) continue;
                    void* iter = nullptr;
                    void* f = nullptr;
                    int fc = 0;
                    char fl[440] = {0};
                    while ((f = api.class_fields(k, &iter)) && fc < 18) {
                        const char* fn = api.field_name(f);
                        void* t = api.field_type(f);
                        char* tn = t ? api.type_name(t) : nullptr;
                        if (fn && tn) {
                            char one[72];
                            snprintf(one, sizeof one, "%s:%s@+0x%zx ", fn, tn, api.field_offset(f));
                            strlcat(fl, one, sizeof fl);
                            fc++;
                        }
                        if (tn) free(tn);
                    }
                    LOGI(EF_LOG " ability-types [%s]", fl);
                    if (++hits > 30) return true;
                }
            }
        }
        return true;
    });
    LOGI(EF_LOG " census done hits=%d", hits);
}
static bool EnsureApi();  // 前向声明（定义在文件后部）
static void* ResolveClass(const char* ns, const char* cls);  // 同上
// ================= v12 数据写（TP 安全：零 .text 补丁、零 inline hook）=================
// metadata 实证：Beyond.Gameplay.Core.AbilitySystem 有 get_hp/get_maxHp/
// get_factionIndex/SetHpDirectly；敌方子类 AbilitySystemForEnemyPart/Npc/God。
// 敌我判定 = FactionIndex 枚举值（字段名关键字解析；fail-safe：解析不出不动手，
// 防止阵营值猜反把玩家秒了/给怪无敌）。
struct DwKlassMethods {
    void* mHp = nullptr;
    void* mMaxHp = nullptr;
    void* mFac = nullptr;
    void* mSetHp = nullptr;   // SetHpDirectly：argc 1（float）或 2（float,bool）
    int setHpArgc = 0;
};
static std::unordered_map<void*, DwKlassMethods> g_dwCache;
static std::unordered_map<void*, int> g_dwFaction;    // obj -> faction（信任缓存）
static std::unordered_set<void*> g_dwBlack;           // invoke 失败/换型对象，永久跳过

static DwKlassMethods& DwResolve(void* klass) {
    auto it = g_dwCache.find(klass);
    if (it != g_dwCache.end()) return it->second;
    DwKlassMethods m;
    m.mHp = GetApi().class_method(klass, "get_hp", 0);
    m.mMaxHp = GetApi().class_method(klass, "get_maxHp", 0);
    m.mFac = GetApi().class_method(klass, "get_factionIndex", 0);
    m.mSetHp = GetApi().class_method(klass, "SetHpDirectly", 1);
    m.setHpArgc = 1;
    if (!m.mSetHp) { m.mSetHp = GetApi().class_method(klass, "SetHpDirectly", 2); m.setHpArgc = 2; }
    return g_dwCache.emplace(klass, m).first->second;
}
// runtime_invoke 封装：返回 boxed 结果；exc 非空 = 调用失败
static void* DwInvoke(void* m, void* obj, void** args, bool* ok) {
    void* exc = nullptr;
    void* r = GetApi().runtime_invoke(m, obj, args, &exc);
    if (ok) *ok = (exc == nullptr);
    return r;
}
// 单遍扫描 rw 映射，命中任一 klass 指针的词 → 对象指针（对象头在 -8）
static void DwScanInstances(const std::vector<void*>& klasses, std::vector<void*>& out) {
    std::vector<uintptr_t> pats;
    for (void* k : klasses) if (k) pats.push_back((uintptr_t)k);
    if (pats.empty()) return;
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        uintptr_t start = 0, end = 0;
        char perms[8] = {0}, path[256] = {0};
        sscanf(line, "%lx-%lx %7s %*s %*s %*s %255s", &start, &end, perms, path);
        if (perms[0] != 'r' || perms[1] != 'w') continue;
        if (path[0] != 0 && strncmp(path, "[anon", 5) != 0 && strcmp(path, "[heap]") != 0) continue;
        uintptr_t size = end - start;
        // ⚠️ 不能加大小下限：Boehm GC 堆按 ~1MB 块映射，加下限会把真实例全排除
        // （v13.1 教训：≥8MB 过滤导致 player=0/enemy=0）。假阳性由后续闸门拦截。
        if (size == 0 || size > ((uintptr_t)1536 << 20)) continue;
        for (uintptr_t a = start; a + 8 <= end; a += 8) {
            uintptr_t w;
            memcpy(&w, (void*)a, 8);
            for (uintptr_t p : pats)
                if (w == p) { out.push_back((void*)(a - 8)); break; }
        }
    }
    fclose(f);
}
// FactionIndex 枚举：按字段名关键字定敌我阵营值
static void DwResolveFaction(int* playerVal, int* enemyVal) {
    *playerVal = -1; *enemyVal = -1;
    auto& api = GetApi();
    void* k = ResolveClass("Beyond.Gameplay.Core", "FactionIndex");
    if (!k || !api.class_fields || !api.field_name || !api.field_static_get) return;
    void* iter = nullptr, *fld = nullptr;
    char buf[300] = {0};
    while ((fld = api.class_fields(k, &iter))) {
        const char* fn = api.field_name(fld);
        if (!fn || !strcmp(fn, "value__")) continue;
        int32_t v = 0;
        api.field_static_get(fld, &v);
        char one[40];
        snprintf(one, sizeof one, "%s=%d ", fn, v);
        strlcat(buf, one, sizeof buf);
        if (strstr(fn, "har") || strstr(fn, "layer") || strstr(fn, "ain") || strstr(fn, "ood")) *playerVal = v;
        if (strstr(fn, "nem") || strstr(fn, "onster") || strstr(fn, "Bad")) *enemyVal = v;
    }
    LOGI(EF_LOG " dw faction [%s] player=%d enemy=%d", buf, *playerVal, *enemyVal);
}
static void DataWriteLoop() {
    auto& api = GetApi();
    if (!api.thread_attach || !api.domain_get()) { LOGW(EF_LOG " dw skip: thread_attach missing"); return; }
    api.thread_attach(api.domain_get());  // 非 il2cpp 线程 invoke 前必须 attach

    void* kP = ResolveClass("Beyond.Gameplay.Core", "AbilitySystem");
    LOGI(EF_LOG " dw klass AbilitySystem=%p", kP);
    if (!kP) return;
    std::vector<void*> klasses = {kP};
    for (const char* n : {"AbilitySystemForEnemyPart", "AbilitySystemForNpc", "AbilitySystemForGod"}) {
        void* k = ResolveClass("Beyond.Gameplay.Core", n);
        if (k) klasses.push_back(k);
    }
    int pv = -1, ev = -1;
    DwResolveFaction(&pv, &ev);
    const bool facOk = (pv >= 0 && ev >= 0 && pv != ev);
    LOGI(EF_LOG " dw ready facOk=%d (inv=玩家周期写满HP, dmg=怪HP写0)", facOk);

    std::vector<void*> insts;
    int cycle = 0;
    for (;;) {
        usleep(500 * 1000);
        const bool wantInv = g_inv.load(), wantKill = g_dmg.load();
        if ((!wantInv && !wantKill) || !GetApi().ready) { cycle++; continue; }
        ++cycle;
        if (insts.empty() || (cycle % 20) == 0) {  // 10s 重扫实例表（降低扫描频率）
            insts.clear();
            g_dwFaction.clear();   // 对象池会复用地址，缓存随之失效
            g_dwBlack.clear();
            Il2Cpp::SafeScan([&] { DwScanInstances(klasses, insts); });
        }
        int healed = 0, killed = 0, pSeen = 0, eSeen = 0;
        float pHp = 0.f, pMax = 0.f;
        int invoked = 0;
        for (size_t idx = 0; idx < insts.size(); idx++) {
            void* obj = insts[idx];
            if (g_dwBlack.count(obj)) continue;
            if (invoked >= 600) break;  // 假阳性风暴限流，剩余对象下轮处理
            void* k = nullptr;
            if (!Il2Cpp::SafeProbe([&] { memcpy(&k, obj, 8); return true; }) || !OkPtr(k)) continue;
            bool known = false;
            for (void* kk : klasses) if (k == kk) { known = true; break; }
            if (!known) { g_dwBlack.insert(obj); continue; }  // 已回收/换型
            DwKlassMethods& m = DwResolve(k);
            if (!m.mFac) { g_dwBlack.insert(obj); continue; }
            // 阵营：失败即拉黑（v12 教训：失败值 -1 绝不能参与敌我判定）
            bool ok = false;
            void* r = nullptr;
            Il2Cpp::SafeProbe([&] { r = DwInvoke(m.mFac, obj, nullptr, &ok); return true; });
            invoked++;
            if (!ok || !r) { g_dwBlack.insert(obj); continue; }
            int faction = Il2Cpp::ReadI32(r, 0x10);
            g_dwFaction[obj] = faction;
            if (faction != pv && faction != ev) continue;
            if (faction == pv) pSeen++;
            if (faction == ev) eSeen++;
            // 三关一致性：hp/maxHp 双读取成功且数值合理（垃圾对象几乎全出局）
            float hp = -1.f, maxHp = -1.f;
            if (m.mHp && invoked < 600) {
                bool ok2 = false;
                void* r2 = nullptr;
                Il2Cpp::SafeProbe([&] { r2 = DwInvoke(m.mHp, obj, nullptr, &ok2); return true; });
                invoked++;
                if (r2 && ok2) hp = Il2Cpp::ReadF32(r2, 0x10);
            }
            if (m.mMaxHp && invoked < 600) {
                bool ok2 = false;
                void* r2 = nullptr;
                Il2Cpp::SafeProbe([&] { r2 = DwInvoke(m.mMaxHp, obj, nullptr, &ok2); return true; });
                invoked++;
                if (r2 && ok2) maxHp = Il2Cpp::ReadF32(r2, 0x10);
            }
            bool plausible = (hp == hp) && (maxHp == maxHp) && maxHp >= 100.f &&
                             maxHp < 1e8f && hp >= 0.f && hp <= maxHp;
            if (!plausible) { g_dwBlack.insert(obj); continue; }
            if (wantInv && faction == pv) {  // 无敌：HP 低于 98% 才写满
                if (hp < maxHp * 0.98f && m.mSetHp) {
                    bool ok2 = false;
                    float target = maxHp;
                    bool extra = false;
                    void* args[2] = { &target, &extra };
                    Il2Cpp::SafeProbe([&] { DwInvoke(m.mSetHp, obj, args, &ok2); return true; });
                    if (ok2) { healed++; pHp = hp; pMax = maxHp; }
                }
            }
            if (wantKill && faction == ev && killed < 2) {  // 秒杀：每轮限 2
                if (hp > 0.f && m.mSetHp) {
                    bool ok2 = false;
                    float hp0 = 0.f;
                    bool extra = false;
                    void* args[2] = { &hp0, &extra };
                    Il2Cpp::SafeProbe([&] { DwInvoke(m.mSetHp, obj, args, &ok2); return true; });
                    if (ok2) killed++;
                }
            }
        }
        static int logDiv = 0;
        if ((logDiv++ % 10) == 0)
            LOGI(EF_LOG " dw cyc=%d insts=%zu player=%d enemy=%d healed=%d killed=%d hp=%.0f/%.0f",
                 cycle, insts.size(), pSeen, eSeen, healed, killed, pHp, pMax);
    }
}

static void* LearnWorker(void*) {
    Il2Cpp::SetScanThread(pthread_self());  // SEGV longjmp 仅登记线程有效
    LOGI(EF_LOG " learn worker up (idle — v14 回到按名 hook 路线)");
    // v12/v13 复盘：堆扫实例全是元数据假阳性，真实例从未命中，invoke 路线废弃。
    // 线程保留挂起待命，不再扫描/invoke。
    for (;;) pause();
    return nullptr;
}
static void StartLearnWorker() {
    static std::once_flag once;
    std::call_once(once, [] {
        pthread_t t;
        if (pthread_create(&t, nullptr, LearnWorker, nullptr) == 0)
            pthread_detach(t);
    });
}

// 玩家侧类名关键词：宿主类名命中则 self 判为玩家（无敌拦截）

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(GameUnity::TARGET_LIB);
}

static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

static void* ResolveClass(const char* ns, const char* cls) {
    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    return k;
}

static void* MethodPtr(void* mi) {
    if (!mi) return nullptr;
    return *reinterpret_cast<void**>(mi);
}

static void* FindMethodTryArgc(void* klass, const char* name, int* outArgc, int maxTry = 10) {
    auto& api = GetApi();
    for (int a = 0; a <= maxTry; a++) {
        void* mi = api.GetMethod(klass, name, a);
        if (mi && MethodPtr(mi)) {
            if (outArgc) *outArgc = a;
            return mi;
        }
    }
    return nullptr;
}

// ---- hooks: damage ----
// 未知精确签名：按安装期探测的 argc 调对应 typed stub；float 返回则 ×100 / 无敌置 0
static float ScaleDmg(float v) {
    if (g_reenter.load()) return v;
    g_hitsDmg.fetch_add(1, std::memory_order_relaxed);
    if (g_dmg.load()) {
        if (v > 0.f && v < 1e12f) v *= 100.f;
        EF_TRACE("dmg scale hits=%u", g_hitsDmg.load());
    }
    return v;
}

static float Hooked_CalcF4(void* a0, void* a1, void* a2, void* a3, void* method) {
    float r = g_origCalc ? ((F4_t)g_origCalc)(a0, a1, a2, a3, method) : 0.f;
    return ScaleDmg(r);
}
static float Hooked_CalcF5(void* a0, void* a1, void* a2, void* a3, void* a4, void* method) {
    float r = g_origCalc ? ((F5_t)g_origCalc)(a0, a1, a2, a3, a4, method) : 0.f;
    return ScaleDmg(r);
}
// v14.3：CalculateDamage 真实签名（CalcSig 实测）ret=double(d0)，参数全 byref
// 指针（x 寄存器，无 FP 参数）。float/int 桩全部位型不匹配 → 伤害恒 0 的教训。
typedef double (*D5_t)(void*, void*, void*, void*, void*);
typedef double (*D6_t)(void*, void*, void*, void*, void*, void*);
static double Hooked_CalcD5(void* a0, void* a1, void* a2, void* a3, void* method) {
    double r = g_origCalc ? ((D5_t)g_origCalc)(a0, a1, a2, a3, method) : 0.0;
    if (g_dmg.load() && r > 0.0 && r < 1e15) {
        g_hitsDmg.fetch_add(1, std::memory_order_relaxed);
        r *= 100.0;
    }
    return r;
}
static double Hooked_CalcD6(void* a0, void* a1, void* a2, void* a3, void* a4, void* method) {
    double r = g_origCalc ? ((D6_t)g_origCalc)(a0, a1, a2, a3, a4, method) : 0.0;
    if (g_dmg.load() && r > 0.0 && r < 1e15) {
        g_hitsDmg.fetch_add(1, std::memory_order_relaxed);
        r *= 100.0;
    }
    return r;
}
static float Hooked_CalcF3(void* a0, void* a1, void* a2, void* method) {
    float r = g_origCalc ? ((F3_t)g_origCalc)(a0, a1, a2, method) : 0.f;
    return ScaleDmg(r);
}
static float Hooked_CalcF2(void* a0, void* a1, void* method) {
    float r = g_origCalc ? ((F2_t)g_origCalc)(a0, a1, method) : 0.f;
    return ScaleDmg(r);
}
static float Hooked_CalcF1(void* a0, void* method) {
    float r = g_origCalc ? ((F1_t)g_origCalc)(a0, method) : 0.f;
    return ScaleDmg(r);
}

// void 返回：主线程零解引用（v9）——a0/a1 指针投递学习线程读类名，
// 不再做盲扫描 patch（V4 签名未实证，盲写 float 槽风险 > 收益）。
// 百倍伤害待 dmgcls 日志揭示 DamageData 结构后 v10 精准实现。
static void PatchDamageObj(void* obj, int slot) {
    if (!OkPtr(obj)) return;
    if (!g_dmg.load()) return;
    EnqueueDmgLearn(obj);
    (void)slot;
}

static void Hooked_CalcV4(void* a0, void* a1, void* a2, void* a3, void* method) {
    if (g_origCalc) ((V4_t)g_origCalc)(a0, a1, a2, a3, method);
    PatchDamageObj(a0, 0);
    if (a1) PatchDamageObj(a1, 1);
}
static void Hooked_CalcV3(void* a0, void* a1, void* a2, void* method) {
    if (g_origCalc) ((V3_t)g_origCalc)(a0, a1, a2, method);
    PatchDamageObj(a0, 0);
    if (a1) PatchDamageObj(a1, 1);
}
static void Hooked_CalcV2(void* a0, void* a1, void* method) {
    if (g_origCalc) ((V2_t)g_origCalc)(a0, a1, method);
    PatchDamageObj(a0, 0);
}
static void Hooked_CalcV1(void* a0, void* method) {
    if (g_origCalc) ((V1_t)g_origCalc)(a0, method);
    PatchDamageObj(a0, 0);
}

// ---- ApplyModifier 敌我判定（v14.2）：挂点=按名解析的 AbilitySystem.ApplyModifier，
// self 必为 AbilitySystem 系实例。v14 教训：直接调基类 get_factionIndex 指针，
// 怪（派生类 AbilitySystemForEnemyPart 等）走不到 override → 读出非 3 → 被误拦。
// 改为按对象实际 klass 解析 override（每 klass 缓存一次，纯元数据查询）。
// Good=2 玩家侧拦截（无敌），Bad=3 怪放行（正常扣血）。----
static std::unordered_map<void*, Faction_t> g_facOverride;  // klass -> override 原生指针
static Faction_t FacNativeFor(void* self) {
    void* k = nullptr;
    memcpy(&k, self, 8);  // 挂点保证 self 有效，对象头 klass 指针单次 8B 读安全
    if (!k) return nullptr;
    auto it = g_facOverride.find(k);
    if (it != g_facOverride.end()) return it->second;
    Faction_t f = nullptr;
    void* mi = GetApi().GetMethod(k, "get_factionIndex", 0);
    if (mi) f = (Faction_t)MethodPtr(mi);
    g_facOverride.emplace(k, f);
    return f;
}
static bool InvGate(void* self) {
    if (!g_inv.load()) return false;
    Faction_t f = FacNativeFor(self);
    if (!f) f = g_facNative;
    if (f) {
        int fac = f(self, g_miFaction);
        g_hitsInv.fetch_add(1, std::memory_order_relaxed);
        return fac != 3;  // 只有 Bad(怪) 放行，其余全护住
    }
    return true;
}
static void Hooked_ApplyV2(void* self, void* mod, void* method) {
    if (InvGate(self)) return;
    if (g_origApplyMod) ((V2_t)g_origApplyMod)(self, mod, method);
}
static void Hooked_ApplyV3(void* self, void* a1, void* a2, void* method) {
    if (InvGate(self)) return;
    if (g_origApplyMod) ((V3_t)g_origApplyMod)(self, a1, a2, method);
}
static void Hooked_ApplyV1(void* self, void* method) {
    if (InvGate(self)) return;
    if (g_origApplyMod) ((V1_t)g_origApplyMod)(self, method);
}

// ---- jump / skill bool ----
static uint8_t Hooked_CanJumpAgain(void* self, void* method) {
    if (g_jump.load()) {
        g_hitsJump.fetch_add(1, std::memory_order_relaxed);
        return 1;
    }
    return g_origJump ? g_origJump(self, method) : 0;
}

static uint8_t Hooked_CheckSkillCd(void* self, void* method) {
    // Check*Cd：常见「在 CD 中」→ 无限技能时强制 false（不在 CD）
    if (g_skill.load()) {
        g_hitsSkill.fetch_add(1, std::memory_order_relaxed);
        return 0;
    }
    return g_origCheckSkillCd ? g_origCheckSkillCd(self, method) : 0;
}

static uint8_t Hooked_CheckCd(void* self, void* method) {
    if (g_skill.load()) {
        g_hitsSkill.fetch_add(1, std::memory_order_relaxed);
        return 0;
    }
    return g_origCheckCd ? g_origCheckCd(self, method) : 0;
}

static uint8_t Hooked_CheckCost(void* self, void* method) {
    // CheckCost：有消耗？无限时强制 true（可放）
    if (g_skill.load()) {
        g_hitsSkill.fetch_add(1, std::memory_order_relaxed);
        return 1;
    }
    return g_origCheckCost ? g_origCheckCost(self, method) : 0;
}

static void Hooked_OnTick(void* self, void* method) {
    if (g_origOnTick) g_origOnTick(self, method);
    // OnTick 后不额外写字段（缺偏移）；CD 旁路靠 Check*
}

static bool HookBool0(const char* tag, const char* ns, const char* cls, const char* method,
                      void* hook, B0_t* orig) {
    if (*orig) return true;
    if (!EnsureApi()) return false;
    void* k = ResolveClass(ns, cls);
    if (!k) {
        LOGW(EF_LOG " %s class miss %s.%s", tag, ns, cls);
        return false;
    }
    int argc = -1;
    void* mi = FindMethodTryArgc(k, method, &argc, 4);
    if (!mi || argc != 0) {
        // 允许 argc=0 为主；若只有 argc>0 则暂不装（避免签名错）
        if (!mi) LOGW(EF_LOG " %s method miss %s", tag, method);
        else LOGW(EF_LOG " %s %s argc=%d (want 0) — degrade", tag, method, argc);
        return false;
    }
    void* fn = MethodPtr(mi);
    if (!fn || DobbyHook(fn, hook, (void**)orig) != 0) {
        LOGW(EF_LOG " %s dobby fail %s", tag, method);
        return false;
    }
    LOGI(EF_LOG " %s hooked %s.%s argc=0", tag, cls, method);
    return true;
}

static bool InstallCalcDamage() {
    if (g_origCalc) return true;
    if (g_failCalc >= 3) return false;  // 熔断：TP 反制/重复 patch 禁止
    if (!EnsureApi()) return false;
    static bool sigLogged = false;
    void* k = ResolveClass("Beyond.Gameplay.Core", "BattleFormula");
    if (!k) {
        LOGW(EF_LOG " BattleFormula miss");
        return false;
    }
    int argc = -1;
    void* mi = FindMethodTryArgc(k, "CalculateDamage", &argc, 8);
    if (!mi) {
        LOGW(EF_LOG " CalculateDamage miss");
        return false;
    }
    // v14.2：先拿真实签名（返回+逐参类型），匹配桩再装——整数桩打坏 FP
    // 参数寄存器导致伤害恒 0 的教训（v14/v14.1 + v5 时代就从未生效）。
    auto& api = GetApi();
    if (!sigLogged && api.method_ret_type && api.method_param && api.type_name) {
        sigLogged = true;
        char sig[280] = {0};
        void* rt = api.method_ret_type(mi);
        char* rn = rt ? api.type_name(rt) : nullptr;
        snprintf(sig, sizeof sig, "ret=%s", rn ? rn : "?");
        if (rn) free(rn);
        for (uint32_t i = 0; argc >= 0 && i < (uint32_t)argc && i < 8; i++) {
            void* pt = api.method_param(mi, i);
            char* tn = pt ? api.type_name(pt) : nullptr;
            char one[72];
            snprintf(one, sizeof one, " p%u=%s", i, tn ? tn : "?");
            strlcat(sig, one, sizeof sig);
            if (tn) free(tn);
        }
        LOGI(EF_LOG " CalcSig argc=%d [%s]", argc, sig);
    }
    if (!sigLogged) return false;  // 符号缺失打不出签名：保持不装
    // v14.3：按 CalcSig 实测签名配桩——ret=double、参数全 byref 指针。
    // static 判定 flags&0x10：static → (p0..p3, method)=5 槽；instance → +this=6 槽。
    uint32_t flags = api.method_flags ? api.method_flags(mi, nullptr) : 0;
    bool isStatic = (flags & 0x0010u) != 0;
    void* fn = MethodPtr(mi);
    void* hook = isStatic ? (void*)Hooked_CalcD5 : (void*)Hooked_CalcD6;
    if (DobbyHook(fn, hook, &g_origCalc) != 0) {
        g_origCalc = nullptr;
        if (++g_failCalc >= 3)
            LOGW(EF_LOG " CalculateDamage give up after %d fails (fuse)", g_failCalc);
        else
            LOGW(EF_LOG " CalculateDamage dobby fail");
        return false;
    }
    g_failCalc = 0;
    LOGI(EF_LOG " CalculateDamage hooked argc=%d static=%d double_ret", argc, isStatic ? 1 : 0);
    return true;
}

static bool InstallApplyModifier() {
    if (g_origApplyMod) return true;
    if (g_failApply >= 3) return false;  // 熔断：TP 反制/重复 patch 禁止
    if (!EnsureApi()) return false;
    void* k = ResolveClass("Beyond.Gameplay.Core", "AbilitySystem");
    if (!k) {
        LOGW(EF_LOG " AbilitySystem miss");
        return false;
    }
    int argc = -1;
    void* mi = FindMethodTryArgc(k, "ApplyModifier", &argc, 6);
    if (!mi) {
        LOGW(EF_LOG " ApplyModifier miss");
        return false;
    }
    void* fn = MethodPtr(mi);
    void* hook = nullptr;
    int pre = 1 + argc;  // instance
    if (pre <= 1) hook = (void*)Hooked_ApplyV1;
    else if (pre == 2) hook = (void*)Hooked_ApplyV2;
    else hook = (void*)Hooked_ApplyV3;
    g_applyArgc = argc;
    if (DobbyHook(fn, hook, &g_origApplyMod) != 0) {
        g_origApplyMod = nullptr;
        if (++g_failApply >= 3)
            LOGW(EF_LOG " ApplyModifier give up after %d fails (fuse)", g_failApply);
        else
            LOGW(EF_LOG " ApplyModifier dobby fail");
        return false;
    }
    g_failApply = 0;
    LOGI(EF_LOG " ApplyModifier hooked managed_argc=%d", argc);
    return true;
}

static void InstallDmgHooks() {
    if (InstallCalcDamage()) g_anyHooked.store(true);
}

static void InstallInvHooks() {
    if (InstallApplyModifier()) {
        // 敌我判定所需：get_factionIndex 原生指针（同 klass 按名解析）
        void* k = ResolveClass("Beyond.Gameplay.Core", "AbilitySystem");
        if (k) {
            g_miFaction = GetApi().GetMethod(k, "get_factionIndex", 0);
            g_facNative = (Faction_t)MethodPtr(g_miFaction);
        }
        LOGI(EF_LOG " faction native=%d (Good=2 玩家 Bad=3 怪)", g_facNative ? 1 : 0);
        g_anyHooked.store(true);
    }
}

static void InstallJumpHooks() {
    if (HookBool0("jump", "Beyond.Gameplay.Core", "MovePipelineAir", "CanJumpAgain",
                  (void*)Hooked_CanJumpAgain, &g_origJump))
        g_anyHooked.store(true);
}

static void InstallSkillHooks() {
    // CheckSkillCd 可能不在 Skill 类上
    if (HookBool0("skillCd", "Beyond.Gameplay.Core", "Skill", "CheckSkillCd",
                  (void*)Hooked_CheckSkillCd, &g_origCheckSkillCd))
        g_anyHooked.store(true);
    else {
        // 短名兜底
        if (!EnsureApi()) return;
        void* k = GetApi().FindClassByShortName("Skill");
        int argc = -1;
        void* mi = k ? FindMethodTryArgc(k, "CheckSkillCd", &argc, 2) : nullptr;
        if (mi && argc == 0) {
            void* fn = MethodPtr(mi);
            if (fn && DobbyHook(fn, (void*)Hooked_CheckSkillCd, (void**)&g_origCheckSkillCd) == 0) {
                LOGI(EF_LOG " CheckSkillCd hooked shortname");
                g_anyHooked.store(true);
            }
        }
    }
    if (HookBool0("cd", "Beyond.Gameplay.Core", "Skill", "CheckCd",
                  (void*)Hooked_CheckCd, &g_origCheckCd))
        g_anyHooked.store(true);
    if (HookBool0("cost", "Beyond.Gameplay.Core", "Skill", "CheckCost",
                  (void*)Hooked_CheckCost, &g_origCheckCost))
        g_anyHooked.store(true);
    // OnTick：argc=0 instance void
    if (!g_origOnTick && EnsureApi()) {
        void* k = ResolveClass("Beyond.Gameplay.Core", "Skill");
        int argc = -1;
        void* mi = k ? FindMethodTryArgc(k, "OnTick", &argc, 2) : nullptr;
        if (mi && argc == 0) {
            void* fn = MethodPtr(mi);
            if (fn && DobbyHook(fn, (void*)Hooked_OnTick, (void**)&g_origOnTick) == 0) {
                LOGI(EF_LOG " Skill.OnTick hooked");
                g_anyHooked.store(true);
            }
        }
    }
}

// ---- v14.4（2026-10-16 小米真机「第二次打开就闪退」复盘，见 10_docs/终末地/闪退_二次打开_20261016.md）----
// 旧实现只在「apply&&jump 装上」或「DobbyHook 失败 3 次」时落定 done；而**按名解析失败**
// （AbilitySystem/ApplyModifier miss、jump/skillCd/cd/cost 的 class|method miss）**不计数**
// ⇒ 解析不到时每 300ms 重跑一次「全程序集解析」，il2cpp 调用贯穿整个进程生命周期；
// 长时间运行 / 后台恢复 / 热更阶段一旦 il2cpp 侧状态变化，就崩在 il2cpp 内部
// （实测 fault addr 0x138、null 解引用；帧 libstarry_magic.so +0x1829c → +0x13c38 →
//  +0x126dc → +0x124f8 → +0x12010 = 框架 MOD 线程 300ms 轮询）。
// 现改为：**前 10 次每 tick 试**（≈0–3s，覆盖正常启动）→ **之后指数退避**（每 7/14/28/56/
// 112/224 个 tick ≈ 2s→4s→8s→17s→34s→67s 封顶）；累计 kHookMaxAttempts 次仍未装成则
// **落定并告警** ⇒ 稳态 il2cpp 调用 = 0（根治该崩溃路）。用户切换开关会重新武装（见 OnFeatureChanged）。
static std::atomic<bool> g_hookDone{false};
static std::atomic<int>  g_hookAttempts{0};
static std::atomic<int>  g_hookTickNo{0};
static std::atomic<int>  g_hookNextTryAt{0};
static const int         kHookMaxAttempts = 60;

static void SyncHooks() {
    // v14：恢复 v5 已验证稳定的按名 hook 集（抓点文档实证挂点）。
    // v6-v10 闪退根因复盘修正：不是 hook 本身（v5 同挂点稳定生效），而是
    // hook 回调里的解引用学习 + 浮窗/主线程并发重复装载。v14：回调零解引用
    // （敌我=直调 get_factionIndex 纯字段读）+ g_hookMtx 串行 + 装成即停重试。
    if (g_hookDone.load()) return;
    if (!g_libReady.load()) return;
    // 退避节拍：框架每 ~300ms 调一次本函数 ⇒ 用 tick 序号当节拍器，不引入时钟依赖。
    const int tickNo = g_hookTickNo.fetch_add(1) + 1;
    if (tickNo < g_hookNextTryAt.load()) return;   // 退避期：一次 il2cpp 调用都不做
    std::lock_guard<std::mutex> lk(g_hookMtx);
    if (!EnsureApi()) return;
    const int attempts = g_hookAttempts.fetch_add(1) + 1;
    {
        const int shift = attempts <= 10 ? 0 : (attempts - 10 > 5 ? 5 : attempts - 10);
        g_hookNextTryAt.store(tickNo + (1 << shift) * 7);
    }
    InstallDmgHooks();
    InstallInvHooks();
    InstallJumpHooks();
    InstallSkillHooks();
    const bool coreOk = (g_origApplyMod && g_origJump);
    LOGI(EF_LOG " hooks sync calc=%d apply=%d jump=%d skillCd=%d (try %d)",
         g_origCalc ? 1 : 0, g_origApplyMod ? 1 : 0,
         g_origJump ? 1 : 0, g_origCheckSkillCd ? 1 : 0, attempts);
    // 核心 hook 就位 → 完成（calc v14.2 起暂缓，不阻塞 done）；熔断 / 试满 → 也停
    if (coreOk || g_failApply >= 3 || attempts >= kHookMaxAttempts) {
        g_hookDone.store(true);
        if (!coreOk)
            LOGW(EF_LOG " hooks give up after %d tries (apply=%d jump=%d) —— 稳态不再触碰 il2cpp",
                 attempts, g_origApplyMod ? 1 : 0, g_origJump ? 1 : 0);
    }
}

bool Endfield_TryInit() {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    g_libReady.store(true);
    StartLearnWorker();
    SyncHooks();
    LOGI(EF_LOG " lib ready (by-name, lazy hooks) dmg=%d inv=%d jump=%d skill=%d",
         g_dmg.load(), g_inv.load(), g_jump.load(), g_skill.load());
    return true;
}

bool Endfield_IsReady() { return g_libReady.load(); }

void Endfield_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ENDFIELD_FEAT_DMG100: g_dmg.store(enabled); break;
        case ENDFIELD_FEAT_INV: g_inv.store(enabled); break;
        case ENDFIELD_FEAT_JUMP: g_jump.store(enabled); break;
        case ENDFIELD_FEAT_SKILL: g_skill.store(enabled); break;
        default: break;
    }
    // v14.4：用户动开关 ⇒ 重新武装**有界**重试（退避期/give-up 之后仍有机会装上）
    g_hookDone.store(false);
    g_hookAttempts.store(0);
    g_hookTickNo.store(0);
    g_hookNextTryAt.store(0);
    SyncHooks();
}

void Endfield_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Endfield_Tick(const bool* features) {
    if (features) {
        g_dmg.store(features[ENDFIELD_FEAT_DMG100]);
        g_inv.store(features[ENDFIELD_FEAT_INV]);
        g_jump.store(features[ENDFIELD_FEAT_JUMP]);
        g_skill.store(features[ENDFIELD_FEAT_SKILL]);
    }
    if (!g_libReady.load() && isLibraryLoaded(GameUnity::TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Endfield_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_jump.store(false);
    g_skill.store(false);
}
