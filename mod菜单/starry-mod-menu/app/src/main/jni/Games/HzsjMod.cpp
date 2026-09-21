/* ================================================================
 *  魂坠深境 — 稳定优先（2026-08-17）
 *
 *  货币：SceneInfoSingleton.instance → gameState → playerState
 *        每 Tick 重取活地址 + ProtectedInt 直写（不挂跳板、不扫堆）
 *  秒杀/无敌/卡牌：功能开启才晚挂 invoke；功能全关零开销
 *  进游禁止常驻挂跳板（否则 HybridCLR 死循环 SIGSEGV）
 * ================================================================ */

#include "HzsjMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <dlfcn.h>
#include <pthread.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;
using Il2Cpp::ReadI32;
using Il2Cpp::ReadPtr;
using Il2Cpp::ReadU8;
using Il2Cpp::WriteI32;

enum : uintptr_t {
    // 1.1.0 (vc41) 专属签名桥（file_off，探针 2026-09-03 实抓；VA = file + 0x4000）
    BRIDGE_FILE_GD   = 0x26B0F0C,  // Damage.GenerateDamage (this, cmd, int, mi) -> ref
    BRIDGE_FILE_ICCA = 0x26A3E0C,  // Item.IsCooldownCastAble / IsManualSelectCost -> bool
    BRIDGE_FILE_GFCV = 0x268B660,  // ItemRequirement.GetFinalCostValue / GetDisplayFinalCost -> int
    BRIDGE_FILE_GS   = 0x26A3A28,  // GameSkill.IsCastAble 等 bool(this,ref) 桥（类名过滤）
    ELF_BIAS = 0x4000,
};

/** 今日热更实抓兜底（FieldOffset 失败时用） */
static constexpr size_t OFF_CMD_BASE = 16;
static constexpr size_t OFF_CMD_BFT = 56;
static constexpr size_t OFF_BFT_IS_PLAYER = 108;
static constexpr size_t OFF_BFT_GAME_STATE = 88;
static constexpr size_t OFF_REQ_COST = 24;
static constexpr size_t OFF_ITEM_CD = 32;
static constexpr size_t OFF_ITEM_OWNER = 136;
static constexpr size_t OFF_OWNER_BFT = 40;
static constexpr size_t OFF_SKILL_CD = 16;
static constexpr size_t OFF_PGSI_COIN = 32;
static constexpr size_t OFF_PGSI_CRYSTAL = 44;
static constexpr int32_t DMG_CAP = 999999;
static constexpr int32_t CURRENCY_VAL = 999999;
static constexpr int32_t PROT_SALT = 0x13579BDF;
static constexpr int32_t PROT_MAGIC = 0x5A17C0DE;

static std::atomic<bool> g_ready{false};
static std::atomic<uintptr_t> g_libExecBase{0};  // r-xp 段执行域基址（双映射下唯一正确）
static std::atomic<uintptr_t> g_bridgeGd{BRIDGE_FILE_GD};
static std::atomic<uintptr_t> g_bridgeIcca{BRIDGE_FILE_ICCA};
static std::atomic<uintptr_t> g_bridgeGfcv{BRIDGE_FILE_GFCV};
static std::atomic<uintptr_t> g_bridgeGs{BRIDGE_FILE_GS};
static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_infCard{false};
static std::atomic<bool> g_infCoin{false};
static std::atomic<bool> g_infCrystal{false};
static std::atomic<bool> g_autoWin{false};     // feat7：进战斗写敌血 0 走自然死亡链
static std::atomic<uint32_t> g_gd{0};
static std::atomic<uint32_t> g_card{0};
static std::atomic<uint32_t> g_tick{0};
static std::atomic<uint32_t> g_currWrote{0};
static std::atomic<void*> g_playerGs{nullptr};
static std::atomic<uint32_t> g_pgsiWaitLog{0};


static void* g_clsBft = nullptr;     // BattleFieldTransform
static void* g_clsPgsi = nullptr;    // PlayerGameStateInfo
static void* g_clsScene = nullptr;   // SceneInfoSingleton
static void* g_clsGs = nullptr;      // GameStateInfo
static void* g_clsUiMgr = nullptr;   // GameUIManager
static void* g_clsPanel = nullptr;   // BattlePanel
static void* g_clsBftDisp = nullptr; // BftUIDisplay
static void* g_clsCont = nullptr;    // ItemContainer
static void* g_clsItem = nullptr;
static void* g_clsItemPtr = nullptr;  // 自学习验证的 Item 类    // Item
static bool g_fieldsReady = false;
static bool g_singletonReady = false;
// —— feat7 自动胜利门控（20260910 实测通过版）：加载期不写、仅战斗内写 ——
static size_t g_offBattleState = 0;  // GameStateInfo.battleState (BattleStateInfo)
static size_t g_offInBattle = 0;     // BattleStateInfo.isInBattle (bool)

// —— 1.1.0 直写链字段偏移（全部运行时按名解析，ilspy 源码核对 2026-09-03）——
static size_t g_offCoin = OFF_PGSI_COIN;
static size_t g_offCrystal = OFF_PGSI_CRYSTAL;
static size_t g_offSceneGs = 0;      // SceneInfoSingleton._gameState
static size_t g_offGsPlayer = 0;     // GameStateInfo.playerState
static size_t g_offGsUiMgr = 0;      // GameStateInfo.UIManager
static size_t g_offUiPanel = 0;      // GameUIManager.battlePanel
static size_t g_offPanelEnemy = 0;   // BattlePanel.enemyBftUIDisplay
static size_t g_offDispBft = 0;      // BftUIDisplay.bft
static size_t g_offPgsiBft = 0;      // PlayerGameStateInfo.bft
static size_t g_offBftHealth = 0;    // BattleFieldTransform.health (ProtectedInt)
static size_t g_offBftMaxHp = 0;     // BattleFieldTransform.maxHealth
static size_t g_offBftExtra = 0;     // BattleFieldTransform.extraHealthPoint
static size_t g_offBftIsP = 0;       // BattleFieldTransform.isPlayer (bool)
static size_t g_offBftTable = 0;     // BattleFieldTransform.tableContainer
static size_t g_offBftComp = 0;      // BattleFieldTransform.companionContainer
static size_t g_offBftDraw = 0;      // BattleFieldTransform.drawPileItemContainer
static size_t g_offBftWaste = 0;     // BattleFieldTransform.wastePileItemContainer
static size_t g_offBftExile = 0;     // BattleFieldTransform.exilePileItemContainer
static size_t g_offContItems = 0;    // ItemContainer.items (List<Item>)
static size_t g_offItemCd = 0;       // Item.cooldown (ProtectedInt)
static size_t g_offItemDelta = 0;    // Item.deltaCost (ProtectedInt)
static size_t g_offReqCost = 0;      // ItemRequirement.costCount (int)
static std::atomic<bool> g_clsItemSelf{false};  // v19：自学习验证的 Item 类


typedef void* (*Invoke_t)(void* a0, void* a1, void* a2, void* a3);
typedef const char* (*method_get_name_t)(void* method);
typedef void* (*method_get_class_t)(void* method);
typedef const char* (*class_get_name_t)(void* klass);
typedef const char* (*image_get_name_t)(void* image);

// DamageAction.DealDamage 的 methodPointer 槽（探针 fileoff；2 参 void 桥，mi=x2）
static constexpr uintptr_t BRIDGE_FILE_DD = 0x26BD29C;
// 解释器主循环入口：桥内 bl 目标 VA 0x2734F50，file_off = VA-0x4000
static constexpr uintptr_t BRIDGE_FILE_EXECUTE = 0x2730F50;
static method_get_name_t g_methodGetName = nullptr;
static method_get_class_t g_methodGetClass = nullptr;
static class_get_name_t g_classGetName = nullptr;

static bool StrContains(const char* hay, const char* needle) {
    if (!hay || !needle || !*needle) return false;
    for (size_t i = 0; hay[i]; i++) {
        size_t j = 0;
        while (needle[j] && hay[i + j] == needle[j]) j++;
        if (!needle[j]) return true;
    }
    return false;
}

static bool SoftPtr(void* p) {
    if (!p) return false;
    uintptr_t v = (uintptr_t)p & 0x00FFFFFFFFFFFFFFULL;
    return v >= 0x1000000000ULL && v <= 0x8000000000ULL;
}

static bool AnyGameplayOn() {
    return g_seckill.load() || g_inv.load() || g_infCard.load() || g_infCoin.load() || g_infCrystal.load() || g_autoWin.load();
}

static bool CurrencyOn() { return g_infCoin.load() || g_infCrystal.load(); }

static bool NeedInvokeHookExec() {
    return g_seckill.load() || g_inv.load() || g_infCard.load();
}

static void* FindClassExact(const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (asms) {
        for (size_t i = 0; i < n; i++) {
            void* img = api.ImageOf(asms[i]);
            if (!img) continue;
            void* k = api.ClassFromName(img, "", name);
            if (k) return k;
        }
    }
    return api.FindClassByShortName(name);
}

static size_t ResolveOff(void* klass, const char* field, size_t fallback) {
    if (!klass || !field) return fallback;
    size_t o = GetApi().FieldOffset(klass, field, 0);
    return o ? o : fallback;
}

/** 热更 DLL 晚于 libil2cpp 映射：字段/单例分别重试，禁止一次失败锁死 */
static void EnsureFields() {
    auto& api = GetApi();

    // —— 货币单例链（与战斗字段无关，可先就绪）——
    if (!g_singletonReady) {
        if (!g_clsScene) g_clsScene = FindClassExact("SceneInfoSingleton");
        if (!g_clsGs) g_clsGs = FindClassExact("GameStateInfo");
        if (!g_clsPgsi) g_clsPgsi = FindClassExact("PlayerGameStateInfo");
        if (g_clsPgsi) {
            g_offCoin = ResolveOff(g_clsPgsi, "coinCount", g_offCoin);
            g_offCrystal = ResolveOff(g_clsPgsi, "crystalCount", g_offCrystal);
        }
        if (g_clsScene) {
            size_t gs = api.FieldOffset(g_clsScene, "_gameState", 0);
            if (gs) g_offSceneGs = gs;
        }
        if (g_clsGs) {
            size_t ps = api.FieldOffset(g_clsGs, "playerState", 0);
            if (ps) g_offGsPlayer = ps;
        }
        // —— feat7 自动胜利门控：isInBattle（加载期不写，v28 日志实证 battleState@40 isInBattle@76）——
        if (g_clsGs && !g_offBattleState) {
            g_offBattleState = api.FieldOffset(g_clsGs, "battleState", 0);
            void* bsCls = FindClassExact("BattleStateInfo");
            if (bsCls) g_offInBattle = api.FieldOffset(bsCls, "isInBattle", 0);
            if (g_offBattleState && g_offInBattle)
                LOGI("[Hzsj] auto-win gate battleState@%zu isInBattle@%zu", g_offBattleState, g_offInBattle);
        }
        // 2026-09-21 热更：只认直读 _gameState。
        // 禁止在 mod 线程 runtime_invoke 托管 getter get_gameState：热更切换窗口该调用
        // 抛执行引擎异常，非托管线程无人接管 → il2cpp RaiseExecutionEngineException → abort
        // （09-21 21:47:46 实锤；C# getter 本身也只是返回 _gameState）。
        if (g_clsScene && g_clsGs && g_clsPgsi && g_offSceneGs && g_offGsPlayer) {
            g_singletonReady = true;
            LOGI("[Hzsj] currency path SceneInfoSingleton._gameState→playerState "
                 "(offGs=%zu offPlayer=%zu coin@%zu cry@%zu, direct-read no invoke)",
                 g_offSceneGs, g_offGsPlayer, g_offCoin, g_offCrystal);
        }
    }

    if (g_fieldsReady) return;

    if (!g_clsBft) g_clsBft = FindClassExact("BattleFieldTransform");
    if (!g_clsPgsi) g_clsPgsi = FindClassExact("PlayerGameStateInfo");
    if (!g_clsUiMgr) g_clsUiMgr = FindClassExact("GameUIManager");
    if (!g_clsPanel) g_clsPanel = FindClassExact("BattlePanel");
    if (!g_clsBftDisp) g_clsBftDisp = FindClassExact("BftUIDisplay");
    if (!g_clsCont) g_clsCont = FindClassExact("ItemContainer");


    if (g_clsBft) {
        g_offBftHealth = ResolveOff(g_clsBft, "health", 0);
        g_offBftMaxHp = ResolveOff(g_clsBft, "maxHealth", 0);
        g_offBftExtra = ResolveOff(g_clsBft, "extraHealthPoint", 0);
        g_offBftIsP = ResolveOff(g_clsBft, "isPlayer", 0);
        g_offBftTable = ResolveOff(g_clsBft, "tableContainer", 0);
        g_offBftComp = ResolveOff(g_clsBft, "companionContainer", 0);
        g_offBftDraw = ResolveOff(g_clsBft, "drawPileItemContainer", 0);
        g_offBftWaste = ResolveOff(g_clsBft, "wastePileItemContainer", 0);
        g_offBftExile = ResolveOff(g_clsBft, "exilePileItemContainer", 0);
    }
    if (g_clsPgsi) {
        g_offPgsiBft = ResolveOff(g_clsPgsi, "bft", 0);
        g_offCoin = ResolveOff(g_clsPgsi, "coinCount", g_offCoin);
        g_offCrystal = ResolveOff(g_clsPgsi, "crystalCount", g_offCrystal);
    }
    if (g_clsGs) g_offGsUiMgr = api.FieldOffset(g_clsGs, "UIManager", 0);
    if (g_clsUiMgr) g_offUiPanel = api.FieldOffset(g_clsUiMgr, "battlePanel", 0);
    if (g_clsPanel) g_offPanelEnemy = api.FieldOffset(g_clsPanel, "enemyBftUIDisplay", 0);
    if (g_clsBftDisp) g_offDispBft = api.FieldOffset(g_clsBftDisp, "bft", 0);
    if (g_clsCont) g_offContItems = api.FieldOffset(g_clsCont, "items", 0);
    if (g_clsItem) {
        g_offItemCd = api.FieldOffset(g_clsItem, "cooldown", 0);
        g_offItemDelta = api.FieldOffset(g_clsItem, "deltaCost", 0);
    }

    // 战斗链就绪判定：无敌/秒杀/卡牌全靠它（isPlayer + health + 容器必须齐）
    bool bftCore = g_clsBft && g_offBftHealth && g_offBftMaxHp && g_offBftIsP;
    bool enemyChain = g_clsGs && g_offGsUiMgr && g_clsUiMgr && g_offUiPanel &&
                      g_clsPanel && g_offPanelEnemy && g_clsBftDisp && g_offDispBft;
    bool cardChain = bftCore && g_offBftTable && g_clsCont && g_offContItems &&
                     g_clsItem && g_offItemCd;
    if (bftCore && enemyChain && cardChain) {
        g_fieldsReady = true;
        LOGI("[Hzsj] fields v110 bft(hp@%zu max@%zu extra@%zu isP@%zu) pgsiBft@%zu "
             "uiMgr@%zu panel@%zu enemyDisp@%zu dispBft@%zu cont@%zu items@%zu "
             "itemCd@%zu itemDelta@%zu",
             g_offBftHealth, g_offBftMaxHp, g_offBftExtra, g_offBftIsP, g_offPgsiBft,
             g_offGsUiMgr, g_offUiPanel, g_offPanelEnemy, g_offDispBft, g_offBftTable,
             g_offContItems, g_offItemCd, g_offItemDelta);
    } else {
        static uint32_t diag = 0;
        uint32_t d = diag++;
        if (d <= 2 || (d % 60) == 0) {
            LOGI("[Hzsj] fields wait bftCore=%d enemyChain=%d cardChain=%d "
                 "(clsBft=%p clsUiMgr=%p clsPanel=%p clsBftDisp=%p clsCont=%p clsItem=%p "
                 "hp@%zu isP@%zu table@%zu items@%zu cd@%zu delta@%zu)",
                 bftCore ? 1 : 0, enemyChain ? 1 : 0, cardChain ? 1 : 0,
                 g_clsBft, g_clsUiMgr, g_clsPanel, g_clsBftDisp, g_clsCont, g_clsItem,
                 g_offBftHealth, g_offBftIsP, g_offBftTable, g_offContItems, g_offItemCd, g_offItemDelta);
        }
        // 一次性：迭代 Item 全部字段（v17：进入即打 begin，失败打指针状态）
        static bool itemDumped = false;
        if (!itemDumped && g_clsItem) {
            itemDumped = true;
            typedef void* (*class_get_fields_t)(void* klass, void** iter);
            typedef const char* (*field_get_name_t)(void* field);
            void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
            LOGI("[Hzsj] ItemFieldDump begin cls=%p h=%p", g_clsItem, h);
            if (h) {
                class_get_fields_t cgf = (class_get_fields_t)dlsym(h, "il2cpp_class_get_fields");
                field_get_name_t fgn = (field_get_name_t)dlsym(h, "il2cpp_field_get_name");
                Il2Cpp::field_get_offset_t fgo = (Il2Cpp::field_get_offset_t)dlsym(h, "il2cpp_field_get_offset");
                LOGI("[Hzsj] ItemFieldDump syms cgf=%p fgn=%p fgo=%p", cgf, fgn, fgo);
                if (cgf && fgn && fgo) {
                    void* it = nullptr;
                    void* f;
                    int cnt = 0;
                    while ((f = cgf(g_clsItem, &it)) != nullptr && cnt < 256) {
                        const char* fn = fgn(f);
                        size_t off = fgo(f);
                        if (fn) LOGI("[Hzsj] ItemField[%d] %s @%zu", cnt, fn, off);
                        cnt++;
                    }
                    LOGI("[Hzsj] ItemFieldDump done total>=%d", cnt);
                }
            }
        }
    }
}

static bool EnsureSyms() {
    if (g_methodGetName && g_classGetName) return true;
    void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
    if (!h) return false;
    g_methodGetName = (method_get_name_t)dlsym(h, "il2cpp_method_get_name");
    g_methodGetClass = (method_get_class_t)dlsym(h, "il2cpp_method_get_class");
    g_classGetName = (class_get_name_t)dlsym(h, "il2cpp_class_get_name");
    return g_methodGetName != nullptr;
}

static bool CopyMethodName(void* method, char* buf, size_t cap) {
    if (!buf || cap < 2 || !method || !g_methodGetName) return false;
    buf[0] = 0;
    const char* m = g_methodGetName(method);
    if (!m) return false;
    size_t i = 0;
    for (; i + 1 < cap; i++) {
        char c = m[i];
        buf[i] = c;
        if (c == 0) break;
    }
    buf[i] = 0;
    return i > 0 && buf[0] != 0;
}

static const char* MethodClassName(void* method) {
    if (!method || !g_methodGetClass || !g_classGetName) return nullptr;
    void* k = g_methodGetClass(method);
    if (!k) return nullptr;
    return g_classGetName(k);
}

static bool NameIs(const char* got, const char* want) {
    if (!got || !want) return false;
    for (size_t i = 0;; i++) {
        if (got[i] != want[i]) return false;
        if (want[i] == 0) return true;
        if (i > 80) return false;
    }
}

/**
 * 每 Tick 从单例重取活 PGSI（进出副本自动换新实例）。
 * 等价于 GG：固定静态入口 → 跟指针 → 写字段；不扫堆、不依赖跳板缓存。
 */
static void* ResolveLivePgsi() {
    if (!g_singletonReady || !g_clsScene) return nullptr;
    auto& api = GetApi();

    // 低频（每 8 tick），但也不装信号处理器：全部靠 SoftPtr + null 检查
    void* scene = api.StaticRefField(g_clsScene, "instance");
    if (!scene || !SoftPtr(scene)) return nullptr;

    // 直读 _gameState（同 C# getter 语义）；不调托管 getter，避免热更窗口异常 abort
    void* gs = g_offSceneGs ? ReadPtr(scene, g_offSceneGs) : nullptr;
    if (!gs || !SoftPtr(gs) || !g_offGsPlayer) return nullptr;

    void* ps = ReadPtr(gs, g_offGsPlayer);
    if (!ps || !SoftPtr(ps)) return nullptr;
    if (g_clsPgsi) {
        void* k = ReadPtr(ps, 0);
        if (k != g_clsPgsi) return nullptr;
    }
    return ps;
}

static void CachePgsi(void* obj) {
    if (!obj || !SoftPtr(obj)) return;
    void* prev = g_playerGs.load();
    if (prev == obj) return;
    g_playerGs.store(obj);
    LOGI("[Hzsj] PGSI cached %p (was %p)", obj, prev);
}

static void InvalidatePgsi(const char* why) {
    void* prev = g_playerGs.exchange(nullptr);
    if (prev)
        LOGI("[Hzsj] PGSI invalidate %p (%s)", prev, why ? why : "?");
}

/** ProtectedInt 12 字节自洽写入（salt/encoded/check 三元组） */
static void WriteProtectedInt(void* obj, size_t off, int32_t value) {
    WriteI32(obj, off + 0, PROT_SALT);
    WriteI32(obj, off + 4, value ^ PROT_SALT);
    WriteI32(obj, off + 8, value ^ PROT_SALT ^ PROT_MAGIC);
}

/** 回读校验：写入后立刻按 ProtectedInt.Decode 语义验证，能区分“写失败”和“被游戏改回” */
static bool ReadProtectedInt(void* obj, size_t off, int32_t* out) {
    if (!obj || !out || !off) return false;
    int32_t salt = ReadI32(obj, off + 0);
    int32_t encoded = ReadI32(obj, off + 4);
    int32_t check = ReadI32(obj, off + 8);
    int32_t value = encoded ^ salt;
    if (salt == 0 && encoded == 0 && check == 0) {
        *out = 0;
        return true;
    }
    if ((value ^ salt ^ PROT_MAGIC) != check) return false;
    *out = value;
    return true;
}

static bool WriteProtectedIntChecked(void* obj, size_t off, int32_t value, const char* tag) {
    WriteProtectedInt(obj, off, value);
    int32_t got = 0;
    if (!ReadProtectedInt(obj, off, &got) || got != value) {
        static uint32_t bad = 0;
        uint32_t n = ++bad;
        if (n <= 8 || (n % 100) == 0)
            LOGI("[Hzsj] PROT_WRITE_BAD#%u %s obj=%p off=%zu want=%d got=%d (读回失败/被改写)",
                 n, tag ? tag : "?", obj, off, value, got);
        return false;
    }
    return true;
}

static bool WriteCurrencyAt(void* obj, const char* tag) {
    if (!obj || !SoftPtr(obj)) return false;
    if (!CurrencyOn()) return false;
    bool wrote = false;
    if (g_infCoin.load()) {
        WriteProtectedIntChecked(obj, g_offCoin, CURRENCY_VAL, "coin");
        wrote = true;
    }
    if (g_infCrystal.load()) {
        WriteProtectedIntChecked(obj, g_offCrystal, CURRENCY_VAL, "crystal");
        wrote = true;
    }
    if (wrote) {
        uint32_t n = g_currWrote.fetch_add(1) + 1;
        if (n <= 8 || (n % 40) == 0)
            LOGI("[Hzsj] CURR#%u %s obj=%p coin@%zu cry@%zu =%d (ProtectedInt)", n, tag ? tag : "?", obj,
                 g_offCoin, g_offCrystal, CURRENCY_VAL);
    }
    return wrote;
}


// —— il2cpp List<T> 标准布局：_items@0x10, _size@0x18；数组 vector@0x20 ——
static void* ListArray(void* list) { return list ? ReadPtr(list, 0x10) : nullptr; }
static int32_t ListSize(void* list) { return list ? ReadI32(list, 0x18) : 0; }
static void* ListAt(void* array, int32_t i) { return ReadPtr(array, 0x20 + (size_t)i * 8); }

/** 对象 klass 校验（防链断误写） */
static bool IsKlass(void* obj, void* klass) {
    return obj && SoftPtr(obj) && klass && ReadPtr(obj, 0) == klass;
}

/**
 * v14 战斗直写（货币同模式，零方法 hook）：
 *   无敌  = 己方 bft.health = 999999（ProtectedInt 自洽）
 *   秒杀  = 敌方 bft.health/maxHealth/extraHealthPoint = 0（enemyBftUIDisplay.bft）
 *   卡牌  = 己方各容器 items：cooldown=0、deltaCost=-99999（免费+免 CD）
 * 字段偏移全部运行时按名解析（ilspy 源码核对字段名，2026-09-03）。
 */
static void ApplyBattleTick(uint32_t t) {
    if (!g_singletonReady || !g_clsScene) return;
    auto& api = GetApi();

    void* scene = api.StaticRefField(g_clsScene, "instance");
    if (!scene || !SoftPtr(scene)) return;
    // 直读 _gameState（同 C# getter 语义）；不调托管 getter，避免热更窗口异常 abort
    void* gs = g_offSceneGs ? ReadPtr(scene, g_offSceneGs) : nullptr;
    if (!gs || !SoftPtr(gs)) return;

    // —— 无敌：playerState.bft.health ——
    if (g_inv.load() && g_offPgsiBft && g_offBftHealth) {
        void* ps = ReadPtr(gs, g_offGsPlayer);
        if (ps && SoftPtr(ps)) {
            void* myBft = ReadPtr(ps, g_offPgsiBft);
            if (IsKlass(myBft, g_clsBft)) {
                uint8_t isP = ReadU8(myBft, g_offBftIsP);
                if (isP) {
                    WriteProtectedIntChecked(myBft, g_offBftHealth, 999999, "inv");
                    static uint32_t invLog = 0;
                    uint32_t n = ++invLog;
                    if (n <= 4 || (n % 200) == 0) LOGI("[Hzsj] INV#%u myBft=%p health=999999", n, myBft);
                }
            }
        }
    }

    // —— 秒杀：UIManager.battlePanel.enemyBftUIDisplay.bft ——
    if (g_seckill.load() && g_offGsUiMgr && g_offUiPanel && g_offPanelEnemy && g_offDispBft &&
        g_offBftHealth && g_offBftMaxHp) {
        void* uiMgr = ReadPtr(gs, g_offGsUiMgr);
        if (uiMgr && SoftPtr(uiMgr)) {
            void* panel = ReadPtr(uiMgr, g_offUiPanel);
            if (panel && SoftPtr(panel)) {
                void* disp = ReadPtr(panel, g_offPanelEnemy);
                if (disp && SoftPtr(disp)) {
                    void* eBft = ReadPtr(disp, g_offDispBft);
                    if (IsKlass(eBft, g_clsBft)) {
                        uint8_t isP = ReadU8(eBft, g_offBftIsP);
                        if (!isP) {  // 确认是敌人，防链错写自己
                            WriteProtectedIntChecked(eBft, g_offBftHealth, 1, "sek");
                            if (g_offBftExtra) WriteProtectedInt(eBft, g_offBftExtra, 0);
                            static uint32_t sekLog = 0;
                            uint32_t n = ++sekLog;
                            if (n <= 4 || (n % 200) == 0) LOGI("[Hzsj] SEK#%u eBft=%p hp=1", n, eBft);
                        }
                    }
                }
            }
        }
    }

    // —— 自动胜利（feat7 实测通过版）：敌血 0 走自然死亡链；isInBattle 门控防加载期误写 ——
    if (g_autoWin.load() && g_offBattleState && g_offInBattle && g_offGsUiMgr && g_offUiPanel &&
        g_offPanelEnemy && g_offDispBft && g_offBftHealth && g_offBftMaxHp) {
        void* bs = ReadPtr(gs, g_offBattleState);
        uint8_t inBattle = (bs && SoftPtr(bs)) ? ReadU8(bs, g_offInBattle) : 0;
        if (inBattle) {
            void* uiMgr = ReadPtr(gs, g_offGsUiMgr);
            if (uiMgr && SoftPtr(uiMgr)) {
                void* panel = ReadPtr(uiMgr, g_offUiPanel);
                if (panel && SoftPtr(panel)) {
                    void* disp = ReadPtr(panel, g_offPanelEnemy);
                    if (disp && SoftPtr(disp)) {
                        void* eBft = ReadPtr(disp, g_offDispBft);
                        if (IsKlass(eBft, g_clsBft)) {
                            uint8_t isP = ReadU8(eBft, g_offBftIsP);
                            if (!isP) {
                                WriteProtectedIntChecked(eBft, g_offBftHealth, 0, "autowin_hp");
                                WriteProtectedIntChecked(eBft, g_offBftMaxHp, 0, "autowin_max");
                                if (g_offBftExtra) WriteProtectedInt(eBft, g_offBftExtra, 0);
                                static uint32_t awLog = 0;
                                uint32_t n = ++awLog;
                                if (n <= 4 || (n % 200) == 0) LOGI("[Hzsj] AUTO_WIN#%u eBft=%p hp=0（自然死亡链）", n, eBft);
                            }
                        }
                    }
                }
            }
        }
    }

    // —— 无限卡牌：己方容器 items 的 cooldown/deltaCost（v19 自学习 Item 类）——
    if (g_infCard.load() && g_offPgsiBft && g_offBftTable && g_offContItems) {
        void* ps = ReadPtr(gs, g_offGsPlayer);
        if (ps && SoftPtr(ps)) {
            void* myBft = ReadPtr(ps, g_offPgsiBft);
            if (IsKlass(myBft, g_clsBft) && ReadU8(myBft, g_offBftIsP)) {
                // 自学习：从容器第一个元素反查真实 Item 类（一次性）
                if (!g_clsItemSelf.load()) {
                    size_t offs[5] = {g_offBftTable, g_offBftComp, g_offBftDraw, g_offBftWaste, g_offBftExile};
                    for (int ci = 0; ci < 5 && !g_clsItemSelf.load(); ci++) {
                        if (!offs[ci]) continue;
                        void* cont = ReadPtr(myBft, offs[ci]);
                        if (!cont || !SoftPtr(cont)) continue;
                        void* lst = ReadPtr(cont, g_offContItems);
                        void* arr = ListArray(lst);
                        int32_t cnt = ListSize(lst);
                        if (!arr || cnt <= 0 || cnt > 64) continue;
                        for (int32_t i = 0; i < cnt; i++) {
                            void* it = ListAt(arr, i);
                            if (!it || !SoftPtr(it)) continue;
                            void* k = ReadPtr(it, 0);
                            if (!k || !SoftPtr(k)) continue;
                            // 验证：该类必须有 cooldown 和 deltaCost 字段
                            typedef void* (*cgfn_t)(void*, const char*);
                            void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
                            if (!h) break;
                            cgfn_t cgfn = (cgfn_t)dlsym(h, "il2cpp_class_get_field_from_name");
                            if (!cgfn) break;
                            if (cgfn(k, "cooldown") && cgfn(k, "deltaCost") && cgfn(k, "owner")) {
                                g_clsItem = k;
                                g_clsItemSelf.store(true);
                                g_clsItemPtr = k;
                                LOGI("[Hzsj] Item class self-learned @%p (from container[%d][%d])", k, ci, i);
                                // 按名解析字段偏移（对真类）
                                g_offItemCd = api.FieldOffset(k, "cooldown", 0);
                                g_offItemDelta = api.FieldOffset(k, "deltaCost", 0);
                                // 解析失败 → 字段迭代匹配
                                if (!g_offItemCd || !g_offItemDelta) {
                                    typedef void* (*cgfa_t)(void*, void**);
                                    typedef const char* (*cgn_t)(void*);
                                    typedef size_t (*cgo_t)(void*);
                                    cgfa_t cgfa = (cgfa_t)dlsym(h, "il2cpp_class_get_fields");
                                    cgn_t cgn = (cgn_t)dlsym(h, "il2cpp_field_get_name");
                                    cgo_t cgo = (cgo_t)dlsym(h, "il2cpp_field_get_offset");
                                    if (cgfa && cgn && cgo) {
                                        void* it2 = nullptr;
                                        void* f;
                                        while ((f = cgfa(k, &it2)) != nullptr) {
                                            const char* fn = cgn(f);
                                            size_t off = cgo(f);
                                            if (fn) {
                                                if (!g_offItemCd && NameIs(fn, "cooldown")) g_offItemCd = off;
                                                if (!g_offItemDelta && NameIs(fn, "deltaCost")) g_offItemDelta = off;
                                            }
                                        }
                                    }
                                }
                                LOGI("[Hzsj] Item self-learn offsets cd@%zu delta@%zu", g_offItemCd, g_offItemDelta);
                                break;
                            }
                        }
                    }
                }
                // 写入（类已自学习且偏移有效）
                if (g_clsItemSelf.load() && g_offItemCd) {
                    int32_t patched = 0;
                    size_t offs2[5] = {g_offBftTable, g_offBftComp, g_offBftDraw, g_offBftWaste, g_offBftExile};
                    for (int ci = 0; ci < 5; ci++) {
                        size_t off = offs2[ci];
                        if (!off) continue;
                        void* cont = ReadPtr(myBft, off);
                        if (!cont || !SoftPtr(cont)) continue;
                        void* lst = ReadPtr(cont, g_offContItems);
                        void* arr = ListArray(lst);
                        int32_t cnt = ListSize(lst);
                        if (!arr || cnt <= 0 || cnt > 64) continue;
                        for (int32_t i = 0; i < cnt; i++) {
                            void* it = ListAt(arr, i);
                            if (!it || !SoftPtr(it)) continue;
                            if (!IsKlass(it, g_clsItemPtr)) continue;
                            WriteProtectedIntChecked(it, g_offItemCd, 0, "card_cd");
                            if (g_offItemDelta) WriteProtectedIntChecked(it, g_offItemDelta, -99999, "card_cost");
                            patched++;
                        }
                    }
                    static uint32_t cardLog = 0;
                    uint32_t n = ++cardLog;
                    if (patched && (n <= 4 || (n % 100) == 0))
                        LOGI("[Hzsj] CARD#%u patched=%d (cd=0 delta=-99999)", n, patched);
                }
            }
        }
    }
}

static void ApplyMetaTick() {
    uint32_t t = g_tick.fetch_add(1) + 1;

    if (!CurrencyOn()) return;

    // 低端机：单例解析降频（每 8 tick 重取活地址）；中间用缓存直写
    void* obj = g_playerGs.load();
    bool refresh = (t % 8) == 1 || !obj || !SoftPtr(obj);
    if (refresh) {
        obj = ResolveLivePgsi();
        if (obj) CachePgsi(obj);
    }
    if (!obj || !SoftPtr(obj)) {
        uint32_t w = g_pgsiWaitLog.fetch_add(1) + 1;
        if (w <= 3 || (w % 120) == 0)
            LOGI("[Hzsj] CURR wait SceneInfoSingleton.gameState.playerState");
        return;
    }
    if ((t % 4) == 0) WriteCurrencyAt(obj, "tick");
}

bool Hzsj_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    if (!GetApi().Load(GameUnity::TARGET_LIB)) return false;
    // 故意不在此处挂桥：进游常驻 hook 热路径会 SIGSEGV（2026-08-17 实锤），1.1.0 同样晚挂
    EnsureFields();
    g_ready.store(true);
    LOGI("[Hzsj] ready v14 direct-write (currency+invincible+seckill+card, no method hook)");
#ifdef STARRY_HZSJ_PROBE
    Hzsj_StartProbe();
#endif
    return true;
}

bool Hzsj_IsReady() { return g_ready.load(); }

void Hzsj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case HZSJ_FEAT_SECKILL: g_seckill.store(enabled); break;
        case HZSJ_FEAT_INVINCIBLE: g_inv.store(enabled); break;
        case HZSJ_FEAT_INF_CARD: g_infCard.store(enabled); break;
        case HZSJ_FEAT_INF_COIN: g_infCoin.store(enabled); break;
        case HZSJ_FEAT_INF_CRYSTAL: g_infCrystal.store(enabled); break;
        case HZSJ_FEAT_AUTO_WIN: g_autoWin.store(enabled); break;
        default: break;
    }
    LOGI("[Hzsj] feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Hzsj_OnValueChanged(int, int) {}

void Hzsj_Tick(const bool* features) {
    if (!g_ready.load()) Hzsj_TryInit();
    if (features) {
        g_seckill.store(features[HZSJ_FEAT_SECKILL]);
        g_inv.store(features[HZSJ_FEAT_INVINCIBLE]);
        g_infCard.store(features[HZSJ_FEAT_INF_CARD]);
        g_infCoin.store(features[HZSJ_FEAT_INF_COIN]);
        g_infCrystal.store(features[HZSJ_FEAT_INF_CRYSTAL]);
        g_autoWin.store(features[HZSJ_FEAT_AUTO_WIN]);
    }
    if (!g_ready.load()) return;
    EnsureFields();
    if (!AnyGameplayOn()) return;
    uint32_t t = g_tick.load();
    ApplyMetaTick();
    ApplyBattleTick(t);
}

void Hzsj_ForceClear() {
    g_seckill.store(false);
    g_inv.store(false);
    g_infCard.store(false);
    g_infCoin.store(false);
    g_infCrystal.store(false);
    g_autoWin.store(false);
    InvalidatePgsi("force clear");
}

static bool JsonFindNumber(const char* json, const char* key, long* out) {
    if (!json || !key || !out) return false;
    char pat[64];
    snprintf(pat, sizeof(pat), "\"%s\"", key);
    const char* p = strstr(json, pat);
    if (!p) return false;
    p = strchr(p + strlen(pat), ':');
    if (!p) return false;
    p++;
    while (*p == ' ' || *p == '\t' || *p == '"') p++;
    char* end = nullptr;
    long v = strtol(p, &end, 0);
    if (end == p) return false;
    *out = v;
    return true;
}

/** 取 JSON 字符串字段 "key":"value"（简易，仅探针用） */
static bool JsonFindString(const char* json, const char* key, char* out, size_t outCap) {
    if (!json || !key || !out || outCap < 2) return false;
    char pat[64];
    snprintf(pat, sizeof(pat), "\"%s\"", key);
    const char* p = strstr(json, pat);
    if (!p) return false;
    p = strchr(p + strlen(pat), ':');
    if (!p) return false;
    p++;
    while (*p == ' ' || *p == '\t') p++;
    if (*p != '"') return false;
    p++;
    size_t n = 0;
    while (*p && *p != '"' && n + 1 < outCap) {
        out[n++] = *p++;
    }
    out[n] = '\0';
    return n > 0;
}

void Hzsj_ApplyPlayPatch(const char* json) {
    if (!json || !*json) return;
    char probe[96] = {};
    if (JsonFindString(json, "ota_probe", probe, sizeof(probe))) {
        LOGI("[Hzsj] OTA_JSON_PROBE=%s", probe);
    }
    long v = 0;
    // 1.1.0 桥 file_off OTA（新键；file_off 口径 = IDA/探针文件偏移）
    if (JsonFindNumber(json, "bridge_gd", &v) && v > 0) {
        g_bridgeGd.store((uintptr_t)v);
        LOGI("[Hzsj] OTA bridge_gd=0x%lX", (unsigned long)v);
    }
    if (JsonFindNumber(json, "bridge_icca", &v) && v > 0) {
        g_bridgeIcca.store((uintptr_t)v);
        LOGI("[Hzsj] OTA bridge_icca=0x%lX", (unsigned long)v);
    }
    if (JsonFindNumber(json, "bridge_gfcv", &v) && v > 0) {
        g_bridgeGfcv.store((uintptr_t)v);
        LOGI("[Hzsj] OTA bridge_gfcv=0x%lX", (unsigned long)v);
    }
    if (JsonFindNumber(json, "bridge_gs", &v) && v > 0) {
        g_bridgeGs.store((uintptr_t)v);
        LOGI("[Hzsj] OTA bridge_gs=0x%lX", (unsigned long)v);
    }
    // offsets 对象内字段
    if (JsonFindNumber(json, "coin", &v) && v >= 0) g_offCoin = (size_t)v;
    if (JsonFindNumber(json, "crystal", &v) && v >= 0) g_offCrystal = (size_t)v;
    if (JsonFindNumber(json, "bft_is_player", &v) && v >= 0) g_offBftIsP = (size_t)v;
    if (JsonFindNumber(json, "req_cost", &v) && v >= 0) g_offReqCost = (size_t)v;
    if (JsonFindNumber(json, "item_cd", &v) && v >= 0) g_offItemCd = (size_t)v;
    LOGI("[Hzsj] OTA patch applied coin=%zu crystal=%zu", g_offCoin, g_offCrystal);
}

/* ================================================================
 *  PROBE（1.1.0 专用，STARRY_HZSJ_PROBE 编译开关）
 *  目的：HybridCLR 热更方法 methodPointer 重复聚集 → 公共 invoke 桥 RVA
 *  纯只读：枚举 assemblies → image → class → method，不写不 hook
 * ================================================================ */
#ifdef STARRY_HZSJ_PROBE

static bool DamageWordHit(const char* cls, const char* mtd) {
    static const char* kWords[] = {"Damage", "Health", "Hurt", "Attack", "Kill", "Die"};
    for (auto w : kWords) {
        if (StrContains(cls, w) || StrContains(mtd, w)) return true;
    }
    return false;
}

typedef void* (*class_get_methods_t)(void* klass, void** iter);

static constexpr size_t kProbeNodes = 24576;
static constexpr size_t kProbeBuckets = 8192;
struct ProbeNode { void* ptr; uint32_t cnt; uint32_t next; };
static ProbeNode g_pnodes[kProbeNodes];
static uint32_t g_pbuckets[kProbeBuckets];
static size_t g_pnNodes;

static void ProbeRecord(void* p) {
    if (!p) return;
    uintptr_t a = (uintptr_t)p;
    if (a < 0x100000000ULL || a > 0x8000000000ULL) return;
    uint32_t b = (uint32_t)((a >> 3) & (kProbeBuckets - 1));
    uint32_t cur = g_pbuckets[b];
    while (cur) {
        if (g_pnodes[cur - 1].ptr == p) { g_pnodes[cur - 1].cnt++; return; }
        cur = g_pnodes[cur - 1].next;
    }
    if (g_pnNodes >= kProbeNodes) return;
    ProbeNode& nd = g_pnodes[g_pnNodes];
    nd.ptr = p; nd.cnt = 1; nd.next = g_pbuckets[b];
    g_pbuckets[b] = (uint32_t)(++g_pnNodes);
}

static void ProbeReset() {
    memset(g_pnodes, 0, sizeof(g_pnodes));
    memset(g_pbuckets, 0, sizeof(g_pbuckets));
    g_pnNodes = 0;
}

/** 取重复次数最高的地址（>= minCnt），返回是否找到；取出后清零避免重复报 */
static bool ProbeTop(void** outPtr, uint32_t* outCnt, uint32_t minCnt) {
    size_t best = kProbeNodes;
    uint32_t bc = minCnt;
    for (size_t i = 0; i < g_pnNodes; i++) {
        if (g_pnodes[i].cnt > bc) { bc = g_pnodes[i].cnt; best = i; }
    }
    if (best == kProbeNodes) return false;
    *outPtr = g_pnodes[best].ptr;
    *outCnt = g_pnodes[best].cnt;
    g_pnodes[best].cnt = 0;
    return true;
}

static uintptr_t ProbeLibBase() {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return 0;
    char line[512];
    uintptr_t base = 0;
    while (fgets(line, sizeof(line), f)) {
        if (strstr(line, GameUnity::TARGET_LIB)) {
            uintptr_t s = strtoull(line, nullptr, 16);
            if (s && (!base || s < base)) base = s;
        }
    }
    fclose(f);
    return base;
}

/** 绝对地址 → 文件偏移：用 libil2cpp r-xp 行换算（双映射下唯一正确口径） */
static uintptr_t ProbeAbsToFileoff(void* p) {
    if (!p) return 0;
    uintptr_t a = (uintptr_t)p;
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return 0;
    char line[512];
    uintptr_t out = 0;
    while (fgets(line, sizeof(line), f)) {
        if (!strstr(line, GameUnity::TARGET_LIB)) continue;
        if (!strstr(line, "r-xp")) continue;
        uintptr_t s = strtoull(line, nullptr, 16);
        uintptr_t e = strtoull(strchr(line, '-') + 1, nullptr, 16);
        if (a < s || a >= e) continue;
        uintptr_t fileoff = strtoull(strchr(line, ' ') + 5, nullptr, 16);
        out = a - s + fileoff;
        break;
    }
    fclose(f);
    return out;
}

static void* HzsjProbeThread(void*) {
    sleep(75);  // 等用户进主界面 + HybridCLR 载热更 DLL
    auto& api = GetApi();
    void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
    if (!h) { LOGI("[HzsjProbe] dlopen fail"); return nullptr; }
    class_get_methods_t cgm = (class_get_methods_t)dlsym(h, "il2cpp_class_get_methods");
    image_get_name_t ign = (image_get_name_t)dlsym(h, "il2cpp_image_get_name");
    method_get_class_t mgc = (method_get_class_t)dlsym(h, "il2cpp_method_get_class");
    if (!cgm || !ign) { LOGI("[HzsjProbe] dlsym fail cgm=%p ign=%p", cgm, ign); return nullptr; }
    EnsureSyms();  // CopyMethodName 依赖 g_methodGetName（v2 修样例 bug）
    LOGI("[HzsjProbe] v2 start cgm=%p", cgm);

    for (int round = 1; round <= 12; round++) {
        size_t nasm = 0;
        void** asms = api.Assemblies(&nasm);
        if (!asms) { sleep(15); continue; }
        int hotImgs = 0;
        for (size_t i = 0; i < nasm; i++) {
            void* img = api.ImageOf(asms[i]);
            if (!img) continue;
            const char* iname = ign(img);
            if (!iname) iname = "?";
            // 跳过明显 AOT 系统库，减小扫描量
            if (strstr(iname, "System") || strstr(iname, "mscorlib") ||
                strstr(iname, "Unity") || strstr(iname, "netstandard") ||
                strstr(iname, "Mono.") || strstr(iname, "Microsoft")) continue;

            ProbeReset();
            size_t ncls = api.image_class_count ? api.image_class_count(img) : 0;
            size_t mTotal = 0;
            bool isMain = strstr(iname, "Assembly-CSharp") != nullptr;
            for (size_t c = 0; c < ncls; c++) {
                void* k = api.image_class ? api.image_class(img, c) : nullptr;
                if (!k) continue;
                const char* cn = api.class_name ? api.class_name(k) : nullptr;
                void* it = nullptr;
                void* m;
                while ((m = cgm(k, &it)) != nullptr) {
                    mTotal++;
                    void* mp = ReadPtr(m, 0);  // MethodInfo.methodPointer
                    ProbeRecord(mp);
                    // v2：按名直搜目标方法，打印桥地址
                    if (isMain && mp) {
                        char nb[64];
                        if (CopyMethodName(m, nb, sizeof(nb))) {
                            bool nameHit =
                                NameIs(nb, "GenerateDamage") || NameIs(nb, "GetFinalCostValue") ||
                                NameIs(nb, "IsCooldownCastAble") || NameIs(nb, "HasEnoughCoin") ||
                                NameIs(nb, "HasEnoughCrystal") || NameIs(nb, "HasEnoughSoulCrystal");
                            bool clsHit = cn && (
                                NameIs(cn, "DamageCommand") || NameIs(cn, "BattleFieldTransform") ||
                                NameIs(cn, "PlayerGameStateInfo") || NameIs(cn, "ItemRequirement") ||
                                NameIs(cn, "GameSkill") || NameIs(cn, "Item") ||
                                NameIs(cn, "SceneInfoSingleton") || NameIs(cn, "GameStateInfo"));
                            if (nameHit || clsHit) {
                                LOGI("[HzsjProbe] HIT img=%s cls=%s mtd=%s ptr=%p fileoff=0x%lX",
                                     iname, cn ? cn : "?", nb, mp,
                                     (unsigned long)ProbeAbsToFileoff(mp));
                            }
                            // 词根候选：1.1.0 真实伤害方法搜索
                            if (cn && nb && DamageWordHit(cn, nb)) {
                                LOGI("[HzsjProbe] CAND cls=%s mtd=%s ptr=%p fileoff=0x%lX",
                                     cn, nb, mp, (unsigned long)ProbeAbsToFileoff(mp));
                            }
                        }
                    }
                }
            }
            if (mTotal < 40) continue;

            // 报 top3 聚集
            void* t1 = nullptr; uint32_t c1 = 0;
            void* t2 = nullptr; uint32_t c2 = 0;
            void* t3 = nullptr; uint32_t c3 = 0;
            bool a1 = ProbeTop(&t1, &c1, 7);
            bool a2 = ProbeTop(&t2, &c2, 7);
            bool a3 = ProbeTop(&t3, &c3, 7);
            if (a1 || a2 || a3) {
                hotImgs++;
                LOGI("[HzsjProbe] R%d img=%s classes=%zu methods=%zu", round, iname, ncls, mTotal);
                if (a1) LOGI("[HzsjProbe]   T1 ptr=%p fileoff=0x%lX cnt=%u", t1,
                             (unsigned long)ProbeAbsToFileoff(t1), c1);
                if (a2) LOGI("[HzsjProbe]   T2 ptr=%p fileoff=0x%lX cnt=%u", t2,
                             (unsigned long)ProbeAbsToFileoff(t2), c2);
                if (a3) LOGI("[HzsjProbe]   T3 ptr=%p fileoff=0x%lX cnt=%u", t3,
                             (unsigned long)ProbeAbsToFileoff(t3), c3);
                // 采样：T1 的 3 个样例（类名.方法名）
                if (a1 && mgc) {
                    int sampled = 0;
                    for (size_t c = 0; c < ncls && sampled < 3; c++) {
                        void* k = api.image_class ? api.image_class(img, c) : nullptr;
                        if (!k) continue;
                        void* it = nullptr;
                        void* m;
                        while ((m = cgm(k, &it)) != nullptr && sampled < 3) {
                            if (ReadPtr(m, 0) == t1) {
                                const char* cn = api.class_name ? api.class_name(k) : "?";
                                char nb[64];
                                if (CopyMethodName(m, nb, sizeof(nb))) {
                                    LOGI("[HzsjProbe]   T1 sample %s.%s", cn, nb);
                                    sampled++;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (hotImgs > 0) {
            LOGI("[HzsjProbe] round %d done hotImgs=%d (fileoff = ProbeAbsToFileoff 口径: r-xp 行换算)",
                 round, hotImgs);
        }
        sleep(15);
    }
    LOGI("[HzsjProbe] finished");
    return nullptr;
}

void Hzsj_StartProbe() {
    static bool started = false;
    if (started) return;
    started = true;
    pthread_t t;
    if (pthread_create(&t, nullptr, HzsjProbeThread, nullptr) == 0) {
        pthread_detach(t);
        LOGI("[HzsjProbe] thread started");
    }
}

#endif  // STARRY_HZSJ_PROBE
