/* ================================================================
 *  TlgdTapTapMod.cpp — 弹力果冻 TapTap 渠道 2.4.0（com.ginicat.sweet）
 *  5 功能：秒杀 / 全屏秒杀 / 无敌 / 无限技能 / 移速
 *
 *  —— 设计基线：零 hook、零 invoke 的纯数据型实现 ——
 *  旧 4399 版三个血功能的闪退，复核结论都指向同一类动作：
 *    R-1 未 attach 的 mod 线程调热更托管方法（get_Stuff / FindBattleUnitByTeam）
 *    R-2 批量 il2cpp_runtime_invoke(BattleServer.HpHandle) + 栈上自造 handle
 *    R-7 按地址 hook 共享 MethodPointer 入口（0xBAAB0C 落在解释器主体内）
 *  本实现把三者结构性去掉：单例读静态 _Stuff / 全堆扫，单位读 battleUnits 列表，
 *  功能只做字段读写，不调任何热更方法、不装任何 hook。
 *
 *  ================================================================
 *  2026-09-11 首轮实机（授权放行、Tick 执行）暴露的问题与修正 —— 全部按反编译定案
 *  ================================================================
 *  ① 秒杀/全屏秒杀写不出去（压血计数 0）。自检日志只打印出 1 个单位，另两个单位的
 *     heroAttribute 取到 null。反编译定案：BattleUnitAttribute 里是**两套属性**
 *       private HeroAttribute heroAttribute;   // 只有玩家主角
 *       private PetAttribute  petAttribute;    // 敌人/宠物/怪物用这套
 *       public HeroAttribute HeroAttr => attrType == Hero ? heroAttribute : null;
 *       public PetAttribute  PetAttr  => attrType == Pet  ? petAttribute  : null;
 *     且 enum BattleAttributeType { Hero, Weapon, Pet }。
 *     → 修正：按 unitAttr.attrType 分派属性视图，两套属性的 Hp/FullHp/Shield/FullShield/
 *       MoveSpeed 偏移分别按名解析（同名不同布局，不能共用）。
 *
 *  ② 无限技能近乎无效（CD 写入仅 2~3 次）。反编译定案：Skill.skillState 是**算出来的**
 *       get { if (skillTimeCounter > 0) return Running;
 *             if (CanUseTimes > 0)      return Usable;   // ← 可用判据
 *             return CD; }
 *     而 Skill.CanUse() 只看 skillState == Usable；cdCounter 只是回复进度
 *     （OnUpdate 里 cdCounter<0 才 AddCanUseTimes()，且要求 CanUseTimes != MaxCanUseTimes）。
 *     → 修正：直接写 CanUseTimes（并清零 cdCounter 让 UI 同步），且遍历 SkillContainer._skills
 *       这个 Dictionary<long,Skill> 把**所有**技能都点亮（只改 curSkill 不够：技能选择循环
 *       里 CanUse() 为假会被跳过，永远轮不到它成为 curSkill）。
 *
 *  ③ 移速不生效（日志 `移速=0(x10)`）。框架里 Slider_ 这类条目**只写 values[下标]**，
 *     不置 features[下标]；既有游戏（YsjYz 的 SeekBar）也是按取值判定开关。
 *     → 修正：移速按 values[5] ≥ 2 判定开启（倍率 1 = 关），features[5] 仅作附加通路。
 *
 *  ④ HeroAttribute 字段偏移实测呈严格 24 字节步长
 *     （FullHp@0x18→Hp@0x30→Defense@0x48→MoveSpeed@0x60→Shield@0x78→FullShield@0x90），
 *     与内联 ObscuredFloat（24 字节）自洽；实机读到写进去的 1e9 护盾 → 加解密规则正确。
 *     ObscuredFloat 布局按名解析得 key@16 hidden@20 fake@32（= 相对偏移 0/4/16）。
 *
 *
 *  ⑤ 【决定"怎么写才有效"的关键】死亡链只在**受伤回调**里跑，没有任何血量轮询：
 *       HpHandle 尾部:  battleUnit.onDamEvent?.Invoke(handle);
 *                       if (battleUnit.unitAttr.isDead) { battleUnit2?.onKillEvent?.Invoke(...);
 *                                                         battleUnit.onDeadEvent?.Invoke(); }
 *       BattleEnemyUnit.OnCreate: onDeadEvent += OnDead;   // 真正的死亡处理（回收/特效/记账）
 *     推论（两条都实测踩过）：
 *       a) 外部写 Hp=0 **不会**让敌人死；反而 HpHandle 开头的 `if (unitAttr.isDead) return;`
 *          会把后续伤害全部丢掉 → 敌人变成"打不到也死不了"的僵尸。全屏秒杀原先写 0 就是这个后果。
 *       b) 正确做法是压到 **1 血**（isDead 仍为假）→ 下一次伤害事件进来结算后 isDead 为真
 *          → onDeadEvent → OnDead 真死亡链（含 onKillEvent 击杀记账），且伤害数字保持正常量级。
 *       c) 佐证：游戏自带清场 GM `BattleUtility.KillAllEnemy` 也是先 `item.unitAttr.Hp = 1f`
 *          再 `atk=1e8 + isReal=true` 造一次 CreateDamage —— 连官方清场都必须借一次伤害事件。
 *       d) 因此"零 invoke 的瞬间全灭"在本作不可得：要么借玩家自己的攻击（本实现，无可检测面），
 *          要么调游戏方法（GM 同款 1e8 真实伤害会写进 TotalDamageCounter → 结算上报 🔴）。
 *
 *  ⑥ 技能类指针自检不能用"类相等"：Skill 有几十个子类（ActiveSkill/Skill_4010701/EasySkill…），
 *     相等比较会把全部真实技能判掉 → 必须沿 class_parent 走继承链判断。
 *
 *  方法论：任何 effect 都必须有写入计数（logcat 可核对），否则等于没装。
 * ================================================================ */
#include "TlgdTapTapMod.h"

#include <atomic>
#include <cmath>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <pthread.h>
#include <unistd.h>
#include <vector>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"   // 必须在 Il2CppApi.h 之前（其默认参数用 sec_lib_il2cpp()）
#include "Unity/Il2CppApi.h"

#define TLGDF     "[TLGDF]"
#define TLGD_TAG  "[Tlgd]"   // 框架约定：每款 so 必含 [Tag] 字面量，出包脚本按字节复核
#define TLGD_LIB  "libil2cpp.so"

// BattleTeamFlag: Neutrality=0 / PlayerTeam=1 / EnemyTeam=2
static constexpr int32_t kTeamPlayer = 1;
static constexpr int32_t kTeamEnemy  = 2;
// BattleEntityType: Hero=0 / Weapon=1 / Pet=2 ...
static constexpr int32_t kTypeHero   = 0;
static constexpr int32_t kTypePet    = 2;
// BattleAttributeType: Hero=0 / Weapon=1 / Pet=2（反编译 110923）
static constexpr int32_t kAttrHero  = 0;
static constexpr int32_t kAttrPet   = 2;

static constexpr float kBigShield = 1.0e9f;  // 无敌：护盾量级（伤害先扣盾）
static constexpr float kBigHpCache = 1.0e9f; // 无敌：属性快照里的 Hp（重建时同时成为 FullHp）
static constexpr float kOneHp     = 1.0f;    // 秒杀：敌血压到 1
static constexpr int32_t kUnlimitedTimes = 9;
static constexpr float kCdReady = -0.01f;    // 游戏自己的"就绪"哨兵值（反编译 117838/117922）

// ---------------- 状态 ----------------
static std::atomic<bool>     g_ready{false};
static std::atomic<bool>     g_attached{false};
static std::atomic<uint32_t> g_tick{0};
static uint32_t g_battleTick = 0;   // 进战斗基准 tick（0 = 不在战斗）
static std::atomic<bool>     g_kill{false};
static std::atomic<bool>     g_aoe{false};
static std::atomic<bool>     g_inv{false};
static std::atomic<bool>     g_nocd{false};
static std::atomic<bool>     g_speed{false};
static std::atomic<int>      g_speedMul{3};   // 滑块 1..10（1 = 关）

// 取证计数（每次写入都计数，logcat 可核对"装了到底有没有生效"）
static std::atomic<uint32_t> g_acSeen{0}, g_hcSeen{0}, g_hcW{0};   // 远端缓存命中/代理缓存命中/缓存写入
static std::atomic<uint32_t> g_wKill{0}, g_wAoe{0}, g_wEs{0}, g_wInvHp{0}, g_wInvSh{0}, g_wCd{0}, g_wSpd{0};
static std::atomic<uint32_t> g_seenUnits{0}, g_seenPlayers{0}, g_seenEnemies{0};
static std::atomic<uint32_t> g_acquire{0}, g_noAttr{0};
static std::atomic<bool>     g_dumped{false};

// ---------------- 解析结果 ----------------
static void*  g_clsMgr = nullptr;
static void*  g_clsEntity = nullptr;
static void*  g_clsUnit = nullptr;
static void*  g_clsBUAttr = nullptr;
static void*  g_clsHeroAttr = nullptr;
static void*  g_clsPetAttr = nullptr;
static void*  g_clsSkill = nullptr;
static void*  g_clsSkillDriver = nullptr;
static void*  g_clsSkillContainer = nullptr;

static size_t offMgrUnits = 0;         // BattleEntityManager.battleUnits : List<BattleUnit>
static size_t offEntTeam = 0;          // BattleEntity.team
static size_t offEntType = 0;          // BattleEntity.entityType
static size_t offEntEid = 0;           // BattleEntity.EntityID
static size_t offUnitAttr = 0;         // BattleUnit.unitAttr
static size_t offUnitSkillDriver = 0;  // BattleUnit._skillDriver
static size_t offBUAAttrType = 0;      // BattleUnitAttribute.attrType
static size_t offBUAHero = 0;          // BattleUnitAttribute.heroAttribute
static size_t offBUAPet = 0;           // BattleUnitAttribute.petAttribute
// 两套属性各自的偏移（字段名相同、布局不同，必须分别解析）
static size_t offHeroHp = 0, offHeroFullHp = 0, offHeroShield = 0, offHeroFullShield = 0,
              offHeroMoveSpeed = 0;
static size_t offPetHp = 0, offPetFullHp = 0, offPetShield = 0, offPetFullShield = 0,
              offPetMoveSpeed = 0;
static size_t offSkillCd = 0, offSkillCanUseTimes = 0;
static size_t offSdRunner = 0, offScCurSkill = 0, offScSkills = 0;
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
static void*  g_clsMovmentDriver = nullptr;  // MovmentDriver（每帧被 BattleDriver 驱动的移动组件）
static size_t offMvUnit = 0;                 // MovmentDriver.unit（可能继承自基类 → 深查找）
static size_t offObjCachedPtr = 0;           // UnityEngine.Object.m_CachedPtr（0 = 原生对象已销毁 ⇒ 死对象）
static size_t offBUMovDriver = 0;            // BattleUnit._movmentDriver（私有字段，按名可取）
static void*  g_clsHeroInfoCache = nullptr; // BattleHeroInfoCache（含 attribute : BattleAttributeCache）
static void*  g_clsPetInfoCache  = nullptr; // BattlePetInfoCache
static size_t offHeroInfoAttr = 0, offPetInfoAttr = 0;   // InfoCache.attribute
static size_t offRAHp = 0, offRAFullHp = 0, offRAShield = 0, offRAFullShield = 0, offRAMoveSpeed = 0;
static size_t offHcHp = 0, offHcFullHp = 0, offHcShield = 0, offHcFullShield = 0;
static size_t offObKey = 0, offObHidden = 0, offObFake = 0;  // ObscuredFloat 结构
static bool   g_obByName = false;

static void*  g_mgr = nullptr;         // BattleEntityManager 单例
static uint32_t g_mgrTick = 0;         // 上次取单例的 tick（重扫节流）

// 玩家侧还原用的原始值
static void*  g_speedHeroAttr = nullptr;
static float  g_spdBase = 0.f, g_spdLast = 0.f;
static float  g_invOrigShield = 0.f, g_invOrigFullShield = 0.f;
static void*  g_invOwner = nullptr;
static bool   g_invCaptured = false;
static bool   g_invOwnerIsPet = false;   // 还原必须走同一套属性偏移（Hero/Pet 字段位置不同）

// ================================================================
//  基础读写
// ================================================================
static inline bool PtrOk(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }
static inline int32_t ReadI32(void* p, size_t o) { return Il2Cpp::ReadI32(p, o); }
static inline int64_t ReadI64(void* p, size_t o) {
    int64_t v = 0; memcpy(&v, (const void*)((uintptr_t)p + o), 8); return v;
}
static inline void WriteI64(void* p, size_t o, int64_t v) {
    memcpy((void*)((uintptr_t)p + o), &v, 8);
}
static inline float  ReadF32(void* p, size_t o) {
    float v = 0.f; memcpy(&v, (const void*)((uintptr_t)p + o), 4); return v;
}
static inline void*  ReadPtr(void* p, size_t o) { return Il2Cpp::ReadPtr(p, o); }
static inline void   WriteI32(void* p, size_t o, int32_t v) { Il2Cpp::WriteI32(p, o, v); }
static inline void   WriteF32(void* p, size_t o, float v) { Il2Cpp::WriteF32(p, o, v); }
static inline int32_t F2I(float f) { int32_t i = 0; memcpy(&i, &f, 4); return i; }
static inline float   I2F(int32_t i) { float f = 0.f; memcpy(&f, &i, 4); return f; }
static inline void*   ClassOf(void* obj) { return obj ? ReadPtr(obj, 0) : nullptr; }

// 字段偏移深查找：class_get_field_from_name 不搜基类，继承字段必须逐级往上找
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

// 类相等或为其子类（Skill 有几十个子类，相等比较会把真实技能全判掉）
static bool IsSameOrSubclass(void* klass, void* base) {
    if (!klass || !base) return false;
    if (klass == base) return true;
    auto& api = Il2Cpp::GetApi();
    if (!api.class_parent) return true;          // 无该 API 时放宽（后续还有字段级守卫）
    for (int d = 0; d < 32 && klass; d++) {
        klass = api.class_parent(klass);
        if (klass == base) return true;
    }
    return false;
}

// 走通道：BattleUnit.proxy → Proxy.cacheSystem → CacheSystem.cacheDic（Dictionary<Type,Cache>）
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
}

// ================================================================
//  官方伤害管线 invoke（2026-09-12 定案）
//  依据：全游戏只有伤害事件里检查 isDead，没有血量轮询；官方清场 GM 自己也必须
//        「Hp=1 → BattleServer.CreateDamage(DamageInfo{atk=1e8,isReal})」才生效。
//        所以纯改字段无法秒杀，必须调用游戏自己的管线。
//  取 BattleUtility.KillAllEnemy()（静态零参数）＝最小风险的 invoke：
//        不需要构造任何托管对象、不需要文件偏移，只用 MethodInfo。
// ================================================================
static std::atomic<uint32_t> g_invoked{0}, g_invokeErr{0};

// il2cpp 集合布局约定：List<T> _items@0x10 / _size@0x18；数组 data@0x20 / max_length@0x18
static inline int32_t ListSize(void* list) { return list ? ReadI32(list, 0x18) : 0; }
static inline void*   ListItems(void* list) { return list ? ReadPtr(list, 0x10) : nullptr; }
static inline int32_t ArrLen(void* arr) { return arr ? ReadI32(arr, 0x18) : 0; }
static inline void*   ArrAt(void* arr, int32_t i) { return ReadPtr(arr, 0x20 + (size_t)i * 8); }

// ---- ACTk ObscuredFloat：currentCryptoKey@0 / hiddenValue@4（相对偏移按名解析）----
// 实测：写进去的 1e9 护盾能被自己读回、游戏侧表现一致 → hidden ^ key 规则正确。
static float ObscRead(void* base, size_t off) {
    const void* p = (const void*)((uintptr_t)base + off);
    return I2F(ReadI32((void*)p, offObHidden) ^ ReadI32((void*)p, offObKey));
}
static void ObscWrite(void* base, size_t off, float v) {
    void* p = (void*)((uintptr_t)base + off);
    const int32_t key = ReadI32(p, offObKey);
    WriteI32(p, offObHidden, F2I(v) ^ key);
    if (offObFake) WriteF32(p, offObFake, v);   // 与 ACTk 构造一致（fakeValue 明文镜像）
}

// ================================================================
//  属性视图：按 attrType 选 Hero/Pet 属性，并带上该套属性自己的字段偏移
//  （反编译定案：HeroAttr=>attrType==Hero?heroAttribute:null；PetAttr=>attrType==Pet?petAttribute:null）
// ================================================================
struct AttrView {
    void*  obj = nullptr;
    size_t hp = 0, fullHp = 0, shield = 0, fullShield = 0, moveSpeed = 0;
    int32_t attrType = -1;
    bool    isPet = false;
};

static bool ResolveAttr(void* unit, AttrView& v) {
    void* ua = ReadPtr(unit, offUnitAttr);
    if (!PtrOk(ua)) return false;
    const int32_t t = offBUAAttrType ? ReadI32(ua, offBUAAttrType) : -1;
    void* ha = offBUAHero ? ReadPtr(ua, offBUAHero) : nullptr;
    void* pa = offBUAPet  ? ReadPtr(ua, offBUAPet)  : nullptr;
    v.attrType = t;
    if (t == kAttrHero && PtrOk(ha)) {
        v.obj = ha; v.isPet = false;
        v.hp = offHeroHp; v.fullHp = offHeroFullHp; v.shield = offHeroShield;
        v.fullShield = offHeroFullShield; v.moveSpeed = offHeroMoveSpeed;
        return true;
    }
    if (t == kAttrPet && PtrOk(pa)) {
        v.obj = pa; v.isPet = true;
        v.hp = offPetHp; v.fullHp = offPetFullHp; v.shield = offPetShield;
        v.fullShield = offPetFullShield; v.moveSpeed = offPetMoveSpeed;
        return true;
    }
    // attrType 缺失/异常：谁非空用谁（自检日志会把 attrType 打出来）
    if (PtrOk(ha)) {
        v.obj = ha; v.isPet = false;
        v.hp = offHeroHp; v.fullHp = offHeroFullHp; v.shield = offHeroShield;
        v.fullShield = offHeroFullShield; v.moveSpeed = offHeroMoveSpeed;
        return true;
    }
    if (PtrOk(pa)) {
        v.obj = pa; v.isPet = true;
        v.hp = offPetHp; v.fullHp = offPetFullHp; v.shield = offPetShield;
        v.fullShield = offPetFullShield; v.moveSpeed = offPetMoveSpeed;
        return true;
    }
    return false;
}

// ================================================================
//  类查找：遍历**全量** image（热更程序集最后加载，禁止截断）
// ================================================================
static void* FindClassAnyImage(const char* shortName) {
    auto& api = Il2Cpp::GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.FindClassInImage(img, shortName);
        if (k) return k;
    }
    return nullptr;
}

// ================================================================
//  单例获取：静态 _Stuff 优先，兜底全堆扫
// ================================================================
static bool ValidateMgr(void* m) {
    if (!PtrOk(m) || !offMgrUnits) return false;
    void* list = ReadPtr(m, offMgrUnits);
    if (!PtrOk(list)) return false;
    const int32_t sz = ListSize(list);
    void* items = ListItems(list);
    if (!PtrOk(items)) return false;
    const int32_t cap = ArrLen(items);
    if (sz < 0 || sz > 512 || cap < 0 || cap > 4096) return false;
    const int32_t step = (sz > 3) ? (sz / 3) : 1;
    int checked = 0, ok = 0;
    for (int32_t i = 0; i < sz && checked < 3; i += step, checked++) {
        void* u = ArrAt(items, i);
        if (!PtrOk(u)) continue;
        const int32_t t = ReadI32(u, offEntTeam);
        if (t >= 0 && t <= 2) ok++;
    }
    return checked == 0 || ok > 0;
}

static bool AcquireMgr() {
    auto& api = Il2Cpp::GetApi();
    if (!api.ready || !g_clsMgr) return false;

    // ① BattleStuff<T> 的 private static T _Stuff（框架成员函数内部自判空函数指针）
    {
        void* m = api.StaticRefField(g_clsMgr, "_Stuff");
        if (ValidateMgr(m)) {
            g_mgr = m; g_mgrTick = g_tick.load();
            LOGI(TLGDF " manager 取自静态 _Stuff = %p", m);
            return true;
        }
    }
    // ② 静态为空 = 当前不在战斗（或战斗未初始化）→ **直接放弃本次**，等下一轮重试。
    // 【教训｜2026-09-12 闪退】此处原本兜底"全堆扫 BattleEntityManager 实例"，实测在
    // 主菜单（_Stuff 为空）时触发，该扫描要在活动堆上遍历约 2GB，且每 10 秒连扫，
    // 最终在 mod 线程 SIGSEGV（fault addr 0x7a000012a1，非堆指针）闪退。
    // 静态 _Stuff 在战斗内必然可取（实战验证），因此兜底扫描全面移除。
    g_acquire.fetch_add(1);
    return false;
}

// ================================================================
//  类型/字段解析（字段从**声明类**取：框架 class_get_field_from_name 不搜基类）
// ================================================================
static bool Resolve() {
    auto& api = Il2Cpp::GetApi();
    if (!api.ready && !api.Load(TLGD_LIB)) return false;

    // 就绪判据（实测教训）：il2cpp 会在初始化完成前就发布 domain，仅凭 domain!=null
    // 去 attach 会在 libunity.so 段错误并 abort 整个进程。用程序集列表已填充当信号。
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return false;

    if (!g_attached.load() && api.thread_attach && api.domain_get) {
        void* dom = api.domain_get();
        if (!dom) return false;
        api.thread_attach(dom);
        g_attached.store(true);
    }

    g_clsMgr            = FindClassAnyImage("BattleEntityManager");
    g_clsEntity         = FindClassAnyImage("BattleEntity");
    g_clsUnit           = FindClassAnyImage("BattleUnit");
    g_clsBUAttr         = FindClassAnyImage("BattleUnitAttribute");
    g_clsHeroAttr       = FindClassAnyImage("HeroAttribute");
    g_clsPetAttr        = FindClassAnyImage("PetAttribute");
    g_clsSkill          = FindClassAnyImage("Skill");
    g_clsSkillDriver    = FindClassAnyImage("SkillDriver");
    g_clsSkillContainer = FindClassAnyImage("SkillContainer");

    if (!g_clsMgr || !g_clsEntity || !g_clsUnit || !g_clsBUAttr || !g_clsHeroAttr) {
        LOGW(TLGDF " 关键类缺失 mgr=%p ent=%p unit=%p bua=%p hero=%p",
             g_clsMgr, g_clsEntity, g_clsUnit, g_clsBUAttr, g_clsHeroAttr);
        return false;
    }

    offMgrUnits = api.FieldOffset(g_clsMgr, "battleUnits");
    offEntTeam  = api.FieldOffset(g_clsEntity, "team");
    offEntType  = api.FieldOffset(g_clsEntity, "entityType");
    offEntEid   = api.FieldOffset(g_clsEntity, "EntityID");
    offUnitAttr = api.FieldOffset(g_clsUnit, "unitAttr");
    offUnitSkillDriver = api.FieldOffset(g_clsUnit, "_skillDriver");
    offBUAAttrType = api.FieldOffset(g_clsBUAttr, "attrType");
    offBUAHero  = api.FieldOffset(g_clsBUAttr, "heroAttribute");
    offBUAPet   = api.FieldOffset(g_clsBUAttr, "petAttribute");

    offHeroHp          = api.FieldOffset(g_clsHeroAttr, "Hp");
    offHeroFullHp      = api.FieldOffset(g_clsHeroAttr, "FullHp");
    offHeroShield      = api.FieldOffset(g_clsHeroAttr, "Shield");
    offHeroFullShield  = api.FieldOffset(g_clsHeroAttr, "FullShield");
    offHeroMoveSpeed   = api.FieldOffset(g_clsHeroAttr, "MoveSpeed");
    if (g_clsPetAttr) {
        offPetHp         = api.FieldOffset(g_clsPetAttr, "Hp");
        offPetFullHp     = api.FieldOffset(g_clsPetAttr, "FullHp");
        offPetShield     = api.FieldOffset(g_clsPetAttr, "Shield");
        offPetFullShield = api.FieldOffset(g_clsPetAttr, "FullShield");
        offPetMoveSpeed  = api.FieldOffset(g_clsPetAttr, "MoveSpeed");
    }
    if (g_clsSkill) {
        offSkillCd          = api.FieldOffset(g_clsSkill, "cdCounter");
        offSkillCanUseTimes = api.FieldOffset(g_clsSkill, "CanUseTimes");
    }
    // 代理流通道：BattleUnit.proxy → Proxy.cacheSystem → CacheSystem.cacheDic
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
    g_clsMovmentDriver  = FindClassAnyImage("MovmentDriver");
    if (g_clsMovmentDriver) offMvUnit = FieldOffsetDeep(g_clsMovmentDriver, "unit");
    offObjCachedPtr = FieldOffsetDeep(g_clsUnit, "m_CachedPtr");   // 继承自 UnityEngine.Object
    offBUMovDriver  = FieldOffsetDeep(g_clsUnit, "_movmentDriver");
    g_clsHeroInfoCache = FindClassAnyImage("BattleHeroInfoCache");
    g_clsPetInfoCache  = FindClassAnyImage("BattlePetInfoCache");
    if (g_clsHeroInfoCache) offHeroInfoAttr = api.FieldOffset(g_clsHeroInfoCache, "attribute");
    if (g_clsPetInfoCache)  offPetInfoAttr  = api.FieldOffset(g_clsPetInfoCache, "attribute");
    if (g_clsUnitHpCache) {
        offHcHp         = api.FieldOffset(g_clsUnitHpCache, "Hp");
        offHcFullHp     = api.FieldOffset(g_clsUnitHpCache, "FullHp");
        offHcShield     = api.FieldOffset(g_clsUnitHpCache, "Shield");
        offHcFullShield = api.FieldOffset(g_clsUnitHpCache, "FullShield");
    }
    if (g_clsSkillDriver)    offSdRunner = api.FieldOffset(g_clsSkillDriver, "runner");
    if (g_clsSkillContainer) {
        offScCurSkill = api.FieldOffset(g_clsSkillContainer, "curSkill");
        offScSkills   = api.FieldOffset(g_clsSkillContainer, "_skills");
    }

    // ObscuredFloat 结构布局：按字段名解析（AOT 类）
    if (void* oc = FindClassAnyImage("ObscuredFloat")) {
        const size_t k = api.FieldOffset(oc, "currentCryptoKey");
        const size_t h = api.FieldOffset(oc, "hiddenValue");
        const size_t f = api.FieldOffset(oc, "fakeValue");
        if (h) { offObKey = k; offObHidden = h; offObFake = f; g_obByName = true; }
    }
    if (!offObHidden) { offObKey = 0; offObHidden = 4; offObFake = 8; }   // 兜底：ACTk 标准布局

    if (!offMgrUnits || !offEntTeam || !offUnitAttr || !offBUAHero || !offHeroHp) {
        LOGW(TLGDF " 关键字段缺失 units=%zu team=%zu attr=%zu buaHero=%zu hp=%zu",
             offMgrUnits, offEntTeam, offUnitAttr, offBUAHero, offHeroHp);
        return false;
    }

    LOGI(TLGD_TAG " 解析完成：units=0x%zx team=0x%zx type=0x%zx attr=0x%zx attrType=0x%zx "
                  "heroAttr=0x%zx petAttr=0x%zx",
         offMgrUnits, offEntTeam, offEntType, offUnitAttr, offBUAAttrType, offBUAHero, offBUAPet);
    LOGI(TLGD_TAG "   HeroAttribute hp=0x%zx full=0x%zx shield=0x%zx fshield=0x%zx spd=0x%zx | "
                  "PetAttribute hp=0x%zx full=0x%zx shield=0x%zx fshield=0x%zx spd=0x%zx",
         offHeroHp, offHeroFullHp, offHeroShield, offHeroFullShield, offHeroMoveSpeed,
         offPetHp, offPetFullHp, offPetShield, offPetFullShield, offPetMoveSpeed);
    LOGI(TLGD_TAG "   代理通道 proxy=0x%zx cacheSystem=0x%zx cacheDic=0x%zx remoteCache=0x%zx "
                  "(cls proxy=%p cachesys=%p hp=%p ra=%p)",
         offUnitProxy, offProxyCacheSystem, offCacheDic, offBUARemoteCache,
         g_clsProxy, g_clsCacheSystem, g_clsUnitHpCache, g_clsRemoteCache);
    LOGI(TLGD_TAG "   移动驱动 MovmentDriver=%p unitOff=0x%zx | 存活判据 m_CachedPtr=0x%zx _movmentDriver=0x%zx",
         g_clsMovmentDriver, offMvUnit, offObjCachedPtr, offBUMovDriver);
    LOGI(TLGD_TAG "   属性缓存 HeroInfoCache=%p attrOff=0x%zx | PetInfoCache=%p attrOff=0x%zx",
         g_clsHeroInfoCache, offHeroInfoAttr, g_clsPetInfoCache, offPetInfoAttr);
    LOGI(TLGD_TAG "   远端缓存 RACache hp=0x%zx full=0x%zx shield=0x%zx fshield=0x%zx spd=0x%zx | "
                  "代理缓存 HpCache hp=0x%zx full=0x%zx shield=0x%zx fshield=0x%zx",
         offRAHp, offRAFullHp, offRAShield, offRAFullShield, offRAMoveSpeed,
         offHcHp, offHcFullHp, offHcShield, offHcFullShield);
    LOGI(TLGD_TAG "   技能链 sdriver=0x%zx runner=0x%zx curSkill=0x%zx skills=0x%zx cd=0x%zx times=0x%zx | "
                  "ObscuredFloat key=%zu hidden=%zu fake=%zu(%s)",
         offUnitSkillDriver, offSdRunner, offScCurSkill, offScSkills, offSkillCd, offSkillCanUseTimes,
         offObKey, offObHidden, offObFake, g_obByName ? "按名" : "兜底");
    return true;
}

// ================================================================
//  功能应用
// ================================================================
// 逐单位通道探针：把 proxy→cacheSystem→cacheDic 的内容（含每个缓存的类名）打出来
static void ProbeUnit(int32_t idx, void* u) {
    const char* cn = Il2Cpp::GetApi().ClassName(u);
    void* px = offUnitProxy ? ReadPtr(u, offUnitProxy) : nullptr;
    void* cs = (PtrOk(px) && offProxyCacheSystem) ? ReadPtr(px, offProxyCacheSystem) : nullptr;
    void* dict = nullptr;
    const int32_t cnt = CacheDictOf(u, &dict);
    // 存活判据：① 原生对象指针（0 = 已销毁）② 自身移动驱动是否反向指认自己
    const uintptr_t nptr = offObjCachedPtr ? (uintptr_t)ReadPtr(u, offObjCachedPtr) : 0;
    void* mvd = offBUMovDriver ? ReadPtr(u, offBUMovDriver) : nullptr;
    void* mvdUnit = (PtrOk(mvd) && offMvUnit) ? ReadPtr(mvd, offMvUnit) : nullptr;
    LOGI(TLGDF " 探针[%d] %s proxy=%p cs=%p dict=%p cnt=%d | 原生对象=%p 移动驱动=%p 驱动指向自己=%s",
         idx, cn ? cn : "?", px, cs, dict, cnt, (void*)nptr, mvd,
         (mvdUnit == u) ? "是" : (PtrOk(mvd) ? "否" : "无驱动"));
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

// 敌人压血：**只能压到 1，绝不能压 0**（2026-09-11 实机 + 反编译双证，见文件头 ⑤）
//   秒杀     = 压到 1 血（保留护盾：带盾敌人需先破盾，随后一击必杀）
//   全屏秒杀 = 压到 1 血 + 清空护盾（任何一次伤害事件即死 → 一轮攻击清屏）
static void ApplyEnemy(void* unit, bool aoe, bool kill) {
    AttrView av;
    if (!ResolveAttr(unit, av)) { g_noAttr.fetch_add(1); return; }

    // 【判定版 2026-09-12】秒杀只对"已经被游戏打过"的敌人（掉过血或掉过盾）压血，并记录写入前的原值。
    // 依据：全游戏只有 HpHandle 在伤害事件里检查 isDead，压血本身不杀人，必须借一次真实命中；
    //      而官方 GM 清场 BattleUtility.KillAllEnemy() 也是 Hp=1 + CreateDamage(1e8,isReal) 同一套路。
    // 判据：若整场战斗 g_wKill(压血1) 恒为 0 ⇒ 游戏结算伤害的对象不是 battleUnits 里的这批实例；
    //      若有增长 ⇒ 对象正确、机制成立（压血后下一击必死）。
    const float hp0   = ObscRead(av.obj, av.hp);
    const float full0 = av.fullHp ? ObscRead(av.obj, av.fullHp) : hp0;
    const float sh0   = av.shield ? ObscRead(av.obj, av.shield) : 0.f;
    const float fsh0  = av.fullShield ? ObscRead(av.obj, av.fullShield) : sh0;
    const bool damaged = (hp0 < full0 - 0.01f) || (sh0 < fsh0 - 0.01f);

    if (aoe) {
        // 全屏秒杀（你选定的语义：压血1 + 清盾，一轮攻击清屏）—— 全体生效
        if (av.shield && sh0 > 0.f) { ObscWrite(av.obj, av.shield, 0.f); g_wEs.fetch_add(1); }
        if (hp0 > kOneHp) { ObscWrite(av.obj, av.hp, kOneHp); g_wAoe.fetch_add(1); }
    } else {
        if (!damaged) return;                       // 未被游戏打过 → 不动它（判定核心）
        if (hp0 > kOneHp) {
            ObscWrite(av.obj, av.hp, kOneHp);        // 通道 A：本地属性 → 下次伤害结算时 isDead → onDeadEvent → OnDead
            const uint32_t n = g_wKill.fetch_add(1) + 1;
            if (n <= 8 || (n % 20) == 0) {
                LOGI(TLGD_TAG " 判定：敌[eid=%lld] 原值 hp=%.1f/%.1f 盾=%.1f/%.1f → 压血1（第%u次）",
                     (long long)ReadI64(unit, offEntEid), hp0, full0, sh0, fsh0, n);
            }
        }
    }

    // 通道 D：InfoCache.attribute（float 属性快照）—— 游戏定期用它重建本地属性，是"被改回"的来源
    if (void* ac = AttrCacheOf(unit)) {
        g_acSeen.fetch_add(1);
        if (offRAHp)     WriteF32(ac, offRAHp, kOneHp);          // cache.Hp → 重建时赋给 Hp 与 FullHp
        if (offRAShield) WriteF32(ac, offRAShield, 0.f);         // cache.Shield → 重建时赋给 Shield/FullShield
    }
    // 通道 C：代理 long 血量缓存
    if (void* hc = UnitHpCacheOf(unit)) {
        g_hcSeen.fetch_add(1);
        if (offHcHp)            { WriteI64(hc, offHcHp, 1); g_hcW.fetch_add(1); }
        if (offHcShield) { WriteI64(hc, offHcShield, 0); g_hcW.fetch_add(1); }
    }
}

// 无限技能：一个单位身上的**所有**技能都点亮
// 依据：Skill.skillState => CanUseTimes > 0 才 Usable；CanUse() 只看 skillState。
// 只改 curSkill 不够——技能选择循环里 CanUse() 为假会被跳过，永远轮不到它当 curSkill。
static void ApplyUnlimitedSkill(void* unit) {
    if (!offUnitSkillDriver) return;
    void* sd = ReadPtr(unit, offUnitSkillDriver);
    if (!PtrOk(sd) || !offSdRunner) return;
    void* sc = ReadPtr(sd, offSdRunner);
    if (!PtrOk(sc)) return;

    int touched = 0;
    // ① 字典遍历（Dictionary<long,Skill>：entries@0x18 / count@0x20 / entry 24 字节 / value@+0x10）
    if (offScSkills) {
        void* dict = ReadPtr(sc, offScSkills);
        if (PtrOk(dict)) {
            void* entries = ReadPtr(dict, 0x18);
            const int32_t cnt = ReadI32(dict, 0x20);
            const int32_t cap = ArrLen(entries);
            if (PtrOk(entries) && cnt > 0 && cnt <= 64 && cap >= cnt) {
                for (int32_t i = 0; i < cnt; i++) {
                    void* entry = (void*)((uintptr_t)entries + 0x20 + (size_t)i * 24);
                    void* sk = ReadPtr(entry, 0x10);
                    if (!PtrOk(sk)) continue;
                    if (g_clsSkill && !IsSameOrSubclass(ClassOf(sk), g_clsSkill)) continue;   // 铁律③：布局自检（含子类）
                    if (offSkillCanUseTimes) WriteI32(sk, offSkillCanUseTimes, kUnlimitedTimes);
                    if (offSkillCd) WriteF32(sk, offSkillCd, kCdReady);
                    touched++;
                }
            }
        }
    }
    // ② 当前技能兜底（curSkill 字段直取，不依赖字典遍历）
    if (offScCurSkill) {
        void* sk = ReadPtr(sc, offScCurSkill);
        if (PtrOk(sk) && (!g_clsSkill || IsSameOrSubclass(ClassOf(sk), g_clsSkill))) {
            if (offSkillCanUseTimes) WriteI32(sk, offSkillCanUseTimes, kUnlimitedTimes);
            if (offSkillCd) WriteF32(sk, offSkillCd, kCdReady);
            touched++;
        }
    }
    if (touched) g_wCd.fetch_add((uint32_t)touched);
}

static void ApplyPlayerUnit(void* unit) {
    AttrView av;
    if (!ResolveAttr(unit, av)) { g_noAttr.fetch_add(1); return; }

    // ---- 无敌：护盾拉满 + 锁血 ----
    if (g_inv.load() && av.shield) {
        if (g_invOwner != av.obj) {           // 换单位/新战斗：记录原值用于还原
            g_invOrigShield = ObscRead(av.obj, av.shield);
            g_invOrigFullShield = av.fullShield ? ObscRead(av.obj, av.fullShield) : 0.f;
            g_invOwner = av.obj;
            g_invCaptured = true;
            g_invOwnerIsPet = av.isPet;      // 决定还原时用 Hero 还是 Pet 偏移
        }
        ObscWrite(av.obj, av.shield, kBigShield);
        g_wInvSh.fetch_add(1);
        if (av.fullShield) ObscWrite(av.obj, av.fullShield, kBigShield);
        if (av.fullHp) {
            const float hp = ObscRead(av.obj, av.hp);
            const float full = ObscRead(av.obj, av.fullHp);
            if (full > 0.f && hp < full) { ObscWrite(av.obj, av.hp, full); g_wInvHp.fetch_add(1); }
        }
        // 通道 D：属性快照（重建源）—— Shield 写满即 FullShield 也满，Hp 写 cache 值
        if (void* ac = AttrCacheOf(unit)) {
            g_acSeen.fetch_add(1);
            if (offRAShield) WriteF32(ac, offRAShield, kBigShield);
            if (offRAHp)     WriteF32(ac, offRAHp, kBigHpCache);
        }
        if (void* hc = UnitHpCacheOf(unit)) {
            g_hcSeen.fetch_add(1);
            if (offHcShield)     WriteI64(hc, offHcShield, (int64_t)kBigShield);
            if (offHcFullShield) WriteI64(hc, offHcFullShield, (int64_t)kBigShield);
            if (offHcHp && offHcFullHp) WriteI64(hc, offHcHp, ReadI64(hc, offHcFullHp));
            g_hcW.fetch_add(1);
        }
    }

    // ---- 移速倍率（只作用于主角的 HeroAttribute.MoveSpeed）----
    if (g_speed.load() && !av.isPet && av.moveSpeed) {
        const float cur = ObscRead(av.obj, av.moveSpeed);
        if (std::fabs(cur - g_spdLast) > 0.01f) g_spdBase = cur;   // 被游戏改写 → 更新基准
        if (g_spdBase <= 0.f) g_spdBase = cur;
        const float want = g_spdBase * (float)g_speedMul.load();
        if (cur > 0.f && std::fabs(cur - want) > 0.001f) {
            ObscWrite(av.obj, av.moveSpeed, want);
            g_spdLast = want;
            g_wSpd.fetch_add(1);
        }
        g_speedHeroAttr = av.obj;
        // 通道 D：属性快照里的移速（游戏重建本地属性时会从这里覆盖回来 —— 位移计数暴涨即此现象）
        if (void* ac = AttrCacheOf(unit)) {
            g_acSeen.fetch_add(1);
            if (offRAMoveSpeed) WriteF32(ac, offRAMoveSpeed, want);
        }
    }

    // ---- 无限技能 ----
    if (g_nocd.load()) ApplyUnlimitedSkill(unit);
}

// 关掉无敌/移速时还原（避免把拉满的护盾/倍率留在存档态）
static void RestorePlayerIfOff() {
    if (!g_inv.load() && g_invCaptured && PtrOk(g_invOwner)) {
        const size_t sh = g_invOwnerIsPet ? offPetShield     : offHeroShield;
        const size_t fs = g_invOwnerIsPet ? offPetFullShield : offHeroFullShield;
        // 量级判断：本 tick 写 1e9，其间可能被伤害扣减，但仍在同一量级（原生护盾是百位）
        const bool ours = sh && ObscRead(g_invOwner, sh) >= kBigShield * 0.5f;
        if (ours) {
            if (g_invOrigShield >= 0.f) ObscWrite(g_invOwner, sh, g_invOrigShield);
            if (fs) ObscWrite(g_invOwner, fs, g_invOrigFullShield);
            LOGI(TLGDF " 无敌已关：护盾还原 %.1f（%s通道）", g_invOrigShield, g_invOwnerIsPet ? "Pet" : "Hero");
        } else {
            LOGI(TLGDF " 无敌已关：目标已非本次写入对象（疑似对象池复用），跳过还原");
        }
        g_invCaptured = false;
        g_invOwner = nullptr;
    }
    if (!g_speed.load() && PtrOk(g_speedHeroAttr) && offHeroMoveSpeed && g_spdBase > 0.f) {
        const float cur = ObscRead(g_speedHeroAttr, offHeroMoveSpeed);
        if (std::fabs(cur - g_spdLast) <= 0.5f) {         // 仍是我们写过的值 → 还原基准
            ObscWrite(g_speedHeroAttr, offHeroMoveSpeed, g_spdBase);
            LOGI(TLGDF " 移速已关：还原 %.2f", g_spdBase);
        }
        g_spdLast = 0.f;
    }
}

// ================================================================
//  主 Tick
// ================================================================
static void WorkTick() {
    const bool anyOn = g_kill.load() || g_aoe.load() || g_inv.load() || g_nocd.load() || g_speed.load();
    if (!anyOn) { RestorePlayerIfOff(); return; }

    if (!ValidateMgr(g_mgr)) {
        g_mgr = nullptr;
        AcquireMgr();   // 每拍重试：扫描已移除，静态读取零风险 —— 进战斗当拍即生效，不再等 10 秒
        return;
    }

    void* list = ReadPtr(g_mgr, offMgrUnits);
    if (!PtrOk(list)) return;
    void* items = ListItems(list);
    const int32_t sz = ListSize(list);
    const int32_t cap = ArrLen(items);
    if (!PtrOk(items) || sz <= 0) {
        g_dumped.store(false);          // 离开战斗 → 下次进战斗重新打自检
        g_invOwner = nullptr;           // 对象池会复用地址：跨局旧指针必须作废
        g_invCaptured = false;
        g_speedHeroAttr = nullptr;
        g_spdLast = 0.f;
        return;
    }
    int32_t n = (sz < cap) ? sz : cap;
    if (n > 512) n = 512;

    const bool kill = g_kill.load(), aoe = g_aoe.load();
    uint32_t players = 0, enemies = 0;
    for (int32_t i = 0; i < n; i++) {
        void* u = ArrAt(items, i);
        if (!PtrOk(u)) continue;
        const int32_t team = ReadI32(u, offEntTeam);
        if (team == kTeamEnemy) {
            enemies++;
            if (aoe || kill) ApplyEnemy(u, aoe, kill);
        } else if (team == kTeamPlayer) {
            players++;
            const int32_t type = offEntType ? ReadI32(u, offEntType) : kTypeHero;
            if (type < 0 || type == kTypeHero || type == kTypePet) ApplyPlayerUnit(u);
        }
    }
    g_seenUnits.store((uint32_t)n);
    g_seenPlayers.store(players);
    g_seenEnemies.store(enemies);

    // 探针：以"进战斗那一刻"为基准触发（按启动 tick 触发会错过进战斗时机）
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
    }

    // 进战斗首拍：打印每个单位的解码自检（证明属性通道/ObscuredFloat 布局正确）
    if (!g_dumped.load()) {
        g_dumped.store(true);
        LOGI(TLGDF " 战场自检 units=%d 我方=%u 敌方=%u | 逐单位解码：", n, players, enemies);
        int shown = 0;
        for (int32_t i = 0; i < n && shown < 8; i++) {
            void* u = ArrAt(items, i);
            if (!PtrOk(u)) continue;
            AttrView av;
            const bool ok = ResolveAttr(u, av);
            const char* cn = Il2Cpp::GetApi().ClassName(u);
            const int32_t team = ReadI32(u, offEntTeam);
            if (!ok) {
                void* ua = ReadPtr(u, offUnitAttr);
                LOGI(TLGDF "   单位[%d] %s team=%d 无可用属性（hero/pet 皆空 attrType=%d）",
                     i, cn ? cn : "?", team,
                     (PtrOk(ua) && offBUAAttrType) ? ReadI32(ua, offBUAAttrType) : -1);
                shown++;
                continue;
            }
            {
                void* ac = AttrCacheOf(u);
                void* hc = UnitHpCacheOf(u);
                LOGI(TLGDF "      通道[%d] proxy=%p AttrCache(hp=%.1f spd=%.2f shield=%.1f) HpCache(hp=%lld/%lld shield=%lld)",
                     i, ReadPtr(u, offUnitProxy),
                     ac && offRAHp ? ReadF32(ac, offRAHp) : -1.f,
                     ac && offRAMoveSpeed ? ReadF32(ac, offRAMoveSpeed) : -1.f,
                     ac && offRAShield ? ReadF32(ac, offRAShield) : -1.f,
                     (long long)(hc && offHcHp ? ReadI64(hc, offHcHp) : -1),
                     (long long)(hc && offHcFullHp ? ReadI64(hc, offHcFullHp) : -1),
                     (long long)(hc && offHcShield ? ReadI64(hc, offHcShield) : -1));
            }
            LOGI(TLGDF "   单位[%d] %s team=%d eid=%lld attrType=%d(%s) hp=%.1f/%.1f shield=%.1f move=%.2f",
                 i, cn ? cn : "?", team, (long long)ReadI64(u, offEntEid),
                 av.attrType, av.isPet ? "Pet" : "Hero",
                 ObscRead(av.obj, av.hp), av.fullHp ? ObscRead(av.obj, av.fullHp) : -1.f,
                 av.shield ? ObscRead(av.obj, av.shield) : -1.f,
                 av.moveSpeed ? ObscRead(av.obj, av.moveSpeed) : -1.f);
            shown++;
        }
    }
}

static void LogStat() {
    LOGI(TLGDF " 状态 秒杀=%d 全屏=%d 无敌=%d 无限技能=%d 移速=%d(x%d) | 写入 压血1=%u 全屏压血=%u "
              "敌盾清=%u 锁血=%u 护盾=%u 技能=%u 位移=%u | 缓存 ac=%u hc=%u hc写=%u | 无属性单位=%u | "
              "战场 units=%u 我方=%u 敌方=%u",
         g_kill.load() ? 1 : 0, g_aoe.load() ? 1 : 0, g_inv.load() ? 1 : 0,
         g_nocd.load() ? 1 : 0, g_speed.load() ? 1 : 0, g_speedMul.load(),
         g_wKill.load(), g_wAoe.load(), g_wEs.load(),
         g_wInvHp.load(), g_wInvSh.load(), g_wCd.load(), g_wSpd.load(),
g_acSeen.load(), g_hcSeen.load(), g_hcW.load(),
         g_noAttr.load(),
         g_seenUnits.load(), g_seenPlayers.load(), g_seenEnemies.load());
}

// ================================================================
//  对外接口
// ================================================================
bool TlgdTapTap_TryInit(JavaVM* /*vm*/) {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(TLGD_LIB)) return false;
    if (!Resolve()) return false;
    g_ready.store(true);
    LOGI(TLGD_TAG " 就绪：零 hook / 零 invoke 数据型实现（秒杀/全屏秒杀/无敌/无限技能/移速）");
    return true;
}

bool TlgdTapTap_IsReady() { return g_ready.load(); }

void TlgdTapTap_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case TLGDTT_FEAT_KILL:  g_kill.store(enabled);  break;
        case TLGDTT_FEAT_AOE:   g_aoe.store(enabled);   break;
        case TLGDTT_FEAT_INV:   g_inv.store(enabled);   break;
        case TLGDTT_FEAT_NOCD:  g_nocd.store(enabled);  break;
        case TLGDTT_FEAT_SPEED: g_speed.store(enabled); break;   // 滑块也会回调，取值见 OnValueChanged
        default: break;
    }
    LOGI(TLGD_TAG " 功能 %d → %s", featNum, enabled ? "开" : "关");
    // 不在本回调里取单例：首次取单例要做一次全堆扫描（可能秒级），
    // 本回调可能跑在菜单/UI 线程上，会卡帧。统一放到 Tick（mod 线程）里做。
}

void TlgdTapTap_OnValueChanged(int featNum, int value) {
    if (featNum == TLGDTT_FEAT_SPEED) {
        g_speedMul.store(value);
        g_speed.store(value >= 2);   // Slider_ 只写 values[]，不置 features[] → 用取值判开关
        LOGI(TLGD_TAG " 移速倍率 → ×%d（%s）", value, value >= 2 ? "开" : "关");
    }
}

void TlgdTapTap_Tick(const bool* features, const int* values) {
    if (!g_ready.load()) {
        if (!TlgdTapTap_TryInit(nullptr)) return;
    }
    if (features) {   // 以框架数组为准（菜单/授权同步）
        g_kill.store(features[TLGDTT_FEAT_KILL]);
        g_aoe.store(features[TLGDTT_FEAT_AOE]);
        g_inv.store(features[TLGDTT_FEAT_INV]);
        g_nocd.store(features[TLGDTT_FEAT_NOCD]);
        // 移速是 Slider_：框架不置特征位，按取值判定（1 = 关）
        if (values) {
            const int v = values[TLGDTT_FEAT_SPEED];
            g_speedMul.store(v);
            g_speed.store(v >= 2 || features[TLGDTT_FEAT_SPEED]);
        } else {
            g_speed.store(features[TLGDTT_FEAT_SPEED]);
        }
    }
    const uint32_t t = g_tick.fetch_add(1) + 1;
    WorkTick();
    if (t <= 3 || (t % 25) == 0) LogStat();   // 前 3 拍 + 每 5 秒一条取证日志
}

void TlgdTapTap_ForceClear() {
    g_kill.store(false);
    g_aoe.store(false);
    g_inv.store(false);
    g_nocd.store(false);
    g_speed.store(false);
    RestorePlayerIfOff();
    LOGW(TLGD_TAG " ForceClear：功能全关");
}
