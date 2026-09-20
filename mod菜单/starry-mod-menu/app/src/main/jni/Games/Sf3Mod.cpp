/* ================================================================
 *  Sf3Mod.cpp — 暗影格斗3 (com.wanda.sf3.wd / TapTap 1.45.3 / vc907)
 *
 *  四项功能（2026-09-19 按用户实测反馈删除无限货币：假 UI 无效）：
 *    1 伤害倍率(1-10)  IKHOBFONGIE.OGFLOMPOMCI           file 0x5F51968
 *                      —— 斌哥改包同款挂点：受作用单位 == self+0x68(玩家)
 *                         / self+0x70(敌人) 时改「伤害值容器」LJMJLLJNEMK；
 *                         容器值 XOR 加密，必须走 getter/setter，禁止直写 +0x10。
 *    2 上帝模式        同入口：玩家受击把伤害值清零。
 *    3 战斗速度(1-10)  UnityEngine.Time.set_timeScale  file 0x6BB5C6C
 *                      —— 游戏自带时间管理器只允许 [0,1]（减速），故 hook 底层
 *                         setter；value==0（暂停）原样放行，滑块变更时用 orig 重放。
 *    4 无限体力        IGCAMALALAJ.{HJJFFHOJJFB,OBPEJEPEJMF,EIPOOCIBMBO}
 *                      file 0x6012A88 / 0x6012CF0 / 0x6012BAC
 *
 *  边界（已验证 / 未验证）：
 *    - 两包同源实锤（libil2cpp.so md5 53345cdb… 与原版逐字节一致）⇒ RVA 通用
 *    - 体力是服务端下发的本地镜像，本实现只保证「客户端口径」；
 *      服务端是否认账 = NOT PROVEN，不做任何服务端写操作。
 *
 *  设计铁律：
 *    - RVA 优先 + 序言校验；校验失败才按类名/方法名回退
 *    - 开关全关 = 功能 hook 不安装（0 hook）
 *    - 所有功能 hook 必须调用 orig（返回值改写型除外）
 *    - 类未就绪不挂战斗 hook，等 Tick 重试；不扫描堆、不盲写字段
 *    - 不修改 MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk
 * ================================================================ */

#include "Sf3Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>
#include <link.h>
#include <mutex>
#include <pthread.h>
#include <string>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define SF3_LOG "[sf3]"
#define SF3_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与 orig
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_damageHooked{false};
static std::atomic<bool> g_speedHooked{false};
static std::atomic<bool> g_energyHooked{false};
static std::atomic<int>  g_dmgMult{1};    // Slider: 1=原伤害, 2..10=倍率
static std::atomic<bool> g_god{false};
static std::atomic<int>  g_speed{1};      // Slider: 1=原速, 2..10=倍率
static std::atomic<bool> g_energy{false};
static std::atomic<int64_t> g_lastInstallMs{0};
static std::mutex g_installMutex;

static thread_local bool t_inDamageHook = false;
// RAII 重入保护：任何提前 return / 栈展开都会复位标志（裸 true/false 在早退路径会残留）。
struct ReentryGuard {
    bool& flag;
    explicit ReentryGuard(bool& f) : flag(f) { flag = true; }
    ~ReentryGuard() { flag = false; }
    ReentryGuard(const ReentryGuard&) = delete;
    ReentryGuard& operator=(const ReentryGuard&) = delete;
};
static std::atomic<int> s_hitLogged{0};   // 伤害命中诊断日志已打印次数（上限 5）

// timeScale 重放状态（mod 线程改滑块后立刻生效，不等游戏再调一次）
static std::atomic<float> g_lastBaseScale{1.0f};   // 游戏最近一次请求的原值
static std::atomic<float> g_appliedScale{-1.0f};   // 最近一次真正写下去的值

typedef void  (*ChangeLife_t)(void* self, void* a1, void* a2, void* unit, void* value,
                              void* a5, void* a6, void* a7);
// ↑ 8 个寄存器位的 ABI 推导（2026-09-19 静态实证，别再"修"成 7 参）：
//   dump: OGFLOMPOMCI(AJJGHOHIMGI p1, HJGEAPMLNAF unit, LJMJLLJNEMK value, string p4,
//                         Action<LJMJLLJNEMK> p5)
//   AJJGHOHIMGI = struct { HAHPPPNKIGP }；HAHPPPNKIGP = struct { int @0x0, AKNBGHNHLAD @0x8 }
//   ⇒ AJJGHOHIMGI 共 16B，按值占 **两个** 整型寄存器 x1+x2
//   ⇒ x0=self, x1/x2=p1 两半, x3=unit, x4=value(容器), x5=string, x6=Action, x7=MethodInfo
//   交叉印证：函数内把 (Action,self,unit,container) 存成闭包后 `ldr x0,[closure+0x28]`
//   调 0x5F3EA60 = LJMJLLJNEMK.BAALGFECCBG(容器)→float 并与 0 比较 ⇒ x4 确为伤害容器
//   两个调用点（AELNHOJJINC/BPEFACODFLO）为 tail-branch，x1..x6 原样透传，同步印证。
//   value 容器取/设值另走实例方法 JDMGBLEBMAJ/JBEFILOFEKD（同一实例，非虚调安全）。
typedef float (*ValueGet_t)(void* self, void* method);
typedef void  (*ValueSet_t)(void* self, float value, void* method);
typedef void  (*SetTimeScale_t)(float value, void* method);
typedef int64_t (*EnergyCurrent_t)(void* self, void* method);
typedef bool  (*EnergyEnough_t)(void* self, int64_t cost, void* method);
typedef bool  (*EnergyRegen_t)(void* self, void* method);

static ChangeLife_t    g_origChangeLife = nullptr;
static SetTimeScale_t  g_origSetTimeScale = nullptr;
static EnergyCurrent_t g_origEnergyCurrent = nullptr;
static EnergyEnough_t  g_origEnergyEnough = nullptr;
static EnergyRegen_t   g_origEnergyRegen = nullptr;

// 伤害值容器 getter/setter（不 hook，只当函数指针调用；XOR 解密由游戏自己做）
static ValueGet_t g_valueGet = nullptr;
static ValueSet_t g_valueSet = nullptr;
static void* g_containerClass = nullptr;  // LJMJLLJNEMK
static void* g_unitClass = nullptr;       // HJGEAPMLNAF

// ============================================================
// ② 字段偏移与 file offset（= vaddr - 0x4000，il2cpp 段 p_offset≠p_vaddr）
//    来源：06_dumped/暗影格斗3/static_inspect/key_files/libil2cpp.so 反汇编实测
// ============================================================
enum : size_t {
    kOffPlayerUnit = 0x68,   // IKHOBFONGIE.JOLEGCBKHKI : HJGEAPMLNAF
    kOffEnemyUnit  = 0x70,   // IKHOBFONGIE.AJBHLLFGAEA : HJGEAPMLNAF
};

enum : uintptr_t {
    kFileChangeLife   = 0x5F51968,   // IKHOBFONGIE.OGFLOMPOMCI           (vaddr 0x5F55968)
    kFileValueGet     = 0x5F3AAE8,   // LJMJLLJNEMK.JDMGBLEBMAJ           (vaddr 0x5F3EAE8)
    kFileValueSet     = 0x5F3AB5C,   // LJMJLLJNEMK.JBEFILOFEKD           (vaddr 0x5F3EB5C)
    kFileSetTimeScale = 0x6BB5C6C,   // UnityEngine.Time.set_timeScale    (vaddr 0x6BB9C6C)
    kFileEnergyNow    = 0x6012A88,   // IGCAMALALAJ.HJJFFHOJJFB           (vaddr 0x6016A88)
    kFileEnergyEnough = 0x6012CF0,   // IGCAMALALAJ.OBPEJEPEJMF(long)     (vaddr 0x6016CF0)
    kFileEnergyRegen  = 0x6012BAC,   // IGCAMALALAJ.EIPOOCIBMBO()         (vaddr 0x6016BAC)
};

// 序言首字（llvm-objdump 原样值，小端读入即此值；不匹配 = 版本漂移，拒绝挂）
enum : uint32_t {
    kInsChangeLife   = 0xD10383FF,
    kInsValueGet     = 0xA9BE57FE,
    kInsValueSet     = 0xFC1D0FE8,
    kInsSetTimeScale = 0xFC1E0FE8,
    kInsEnergyNow    = 0xA9BD5FFE,
    kInsEnergyEnough = 0xA9BF4FFE,
    kInsEnergyRegen  = 0xA9BF4FFE,
};

// 伤害入口完整 12 字节序言（ff8303d1 e83b00fd fd7b08a9）
static const uint8_t kPrologueChangeLife[12] = {
    0xFF, 0x83, 0x03, 0xD1, 0xE8, 0x3B, 0x00, 0xFD, 0xFD, 0x7B, 0x08, 0xA9,
};

enum : int64_t {
    kEnergyValue = 9999,
};

enum : uint64_t {
    kInstallRetryMs = 1000,
};

static constexpr float kSpeedMax = 100.0f;

// ============================================================
// ③ 安全助手（防闪退核心，勿删）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static int64_t NowMs() {
    struct timespec ts {};
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<int64_t>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

// ============================================================
// ④ 双路径挂点机器（双路径：file offset→运行时 VA；失败转按名）
// ============================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

// file_off → 运行时 VA：只在 RX 段找（libil2cpp 分段映射，base+RVA 会挂错）
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return found;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(SF3_TARGET_LIB);
}

// 序言不符 = 版本漂移/空页，拒绝挂（防挂错地址闪退），由调用方转按名
static void* VaAtFileOffChecked(const char* tag, uintptr_t fileOff, uint32_t expectInsn) {
    void* va = FileOffToVa(SF3_TARGET_LIB, fileOff);
    if (!va) {
        LOGW("%s file_off 0x%lX not in RX — try by-name", tag, (unsigned long)fileOff);
        return nullptr;
    }
    if (expectInsn) {
        uint32_t got = 0;
        memcpy(&got, va, sizeof(got));
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW("%s drift got=0x%08X want=0x%08X — try by-name", tag, got, expectInsn);
            return nullptr;
        }
    }
    return va;
}

// 伤害入口更强校验：比对完整 12 字节序言
static void* VaAtFileOffBytes(const char* tag, uintptr_t fileOff,
                              const uint8_t* expect, size_t n) {
    void* va = FileOffToVa(SF3_TARGET_LIB, fileOff);
    if (!va) {
        LOGW("%s file_off 0x%lX not in RX — try by-name", tag, (unsigned long)fileOff);
        return nullptr;
    }
    if (expect && n && memcmp(va, expect, n) != 0) {
        uint32_t got = 0;
        memcpy(&got, va, sizeof(got));
        LOGW("%s prologue drift got=0x%08X — try by-name", tag, got);
        return nullptr;
    }
    return va;
}

static bool DobbyInstall(const char* tag, void* va, void* hook, void** orig) {
    if (DobbyHook(va, hook, orig) != 0) {
        LOGW("%s dobby fail @%p", tag, va);
        return false;
    }
    LOGI("%s hooked @%p", tag, va);
    return true;
}

// 按名路径：类/方法缺失只 log 降级，不整包失败（hook-rules §17）
static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (asms && n) {
        for (size_t i = 0; i < n; i++) {
            void* img = api.ImageOf(asms[i]);
            if (!img) continue;
            void* k = api.ClassFromName(img, ns, name);
            if (k) return k;
        }
    }
    return api.FindClassByShortName(name);
}

static void* MethodFn(void* klass, const char* method, int argc) {
    if (!klass || !method) return nullptr;
    void* mi = GetApi().GetMethod(klass, method, argc);
    if (!mi) return nullptr;
    void* fn = *reinterpret_cast<void**>(mi);
    return OkPtr(fn) ? fn : nullptr;
}

static bool HookByName(const char* tag, const char* ns, const char* cls, const char* method,
                       int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW("%s api not ready — %s will retry via Tick", tag, method);
        return false;
    }
    void* k = FindClassNs(ns, cls);
    if (!k) {
        LOGW("%s class miss %s — degrade", tag, cls);
        return false;
    }
    void* fn = MethodFn(k, method, argc);
    if (!fn) {
        LOGW("%s method miss %s.%s — degrade, install others", tag, cls, method);
        return false;
    }
    return DobbyInstall(tag, fn, hook, orig);
}

struct HookSlot {
    const char* tag;
    uintptr_t   fileOff;     // 0 = 只走按名
    uint32_t    expectInsn;  // 0 = 不校验序言
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

// 幂等 + 双路径：*orig 非空 = 已挂过（防双 Dobby 叠钩 SIGSEGV）
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.fileOff) {
        void* va = VaAtFileOffChecked(s.tag, s.fileOff, s.expectInsn);
        if (va && DobbyInstall(s.tag, va, s.hookFn, s.origPtr)) return true;
    }
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

static void InstallTable(const HookSlot* slots, size_t count) {
    for (size_t i = 0; i < count; i++) HookSlotInstall(slots[i]);
}

// ============================================================
// ④' 版权校验补丁（登录可用性，与功能开关无关，必须尽早挂）
//
//  现象（2026-09-20 用户反馈）：卸载重装后第一次能进，第二次启动/登录
//  弹「非正版应用，请下载正版应用」或「SDK init error」→ 登录挂掉。
//
//  链：Hoolai/Nekki AccessSDK 的 HNAccessCore::InitServerCfg 会调
//  HNAccessCore::checkCopyright()，拿沙箱指纹文件与
//  HNConfigMgr::BinaryFingerprint() 比对；不等就写日志
//  "Config Signature Md5: NOT_GENUINE_APP" 并返回假，SDK 初始化失败。
//  首次安装沙箱为空 → 走「文件不存在」放行分支（返回 1），所以第一次能进。
//
//  修法依据（反汇编实测，非猜）：该函数是单出口 `mov w0, w19; ret`，
//  「沙箱无该文件」路径 w19=1；调用方 InitServerCfg 用 `tbz w20,#0,0x1632c`，
//  ret==1 落到 0x15F54 的配置 map 加载（正常路径），ret==0 才进异常分支。
//  故恒定返回 1 = 让所有设备复现「干净安装」的行为，不引入新语义。
//
//  注：本 hook 是登录补丁不是玩法开关，不随菜单开关装卸。
// ============================================================
static std::atomic<bool> g_copyrightHooked{false};
static std::atomic<bool> g_copyrightThreadStarted{false};
static std::atomic<int>  g_copyrightAttempts{0};
static int (*g_origCheckCopyright)(void* self) = nullptr;

static const char* Sf3LibCpp() {
    static const std::string s = OBFUSCATE("libcpp.so");
    return s.c_str();
}

static const char* Sf3SymCheckCopyright() {
    static const std::string s = OBFUSCATE("_ZNK12HNAccessCore14checkCopyrightEv");
    return s.c_str();
}

// 从 /proc/self/maps 取库的绝对路径（dlopen 按 soname 失败时的回退）
static std::string Sf3MappedLibPath(const char* lib) {
    std::string out;
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return out;
    char line[1024];
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib) || !strstr(line, ".so")) continue;
        char* p = strchr(line, '/');
        if (!p) continue;
        char* nl = strchr(p, '\n');
        if (nl) *nl = '\0';
        out.assign(p);
        break;
    }
    fclose(fp);
    return out;
}

static int Hooked_CheckCopyright(void* self) {
    (void)self;
    return 1;   // 真品：与原版「沙箱无指纹文件」时的返回值一致
}

static void TryInstallCopyrightHook() {
    if (g_copyrightHooked.load(std::memory_order_acquire)) return;
    // 上限防御：库始终不出现时，Tick 兜底路径不会无限扫 maps
    if (g_copyrightAttempts.fetch_add(1, std::memory_order_relaxed) >= 2000) return;
    const char* lib = Sf3LibCpp();
    if (!isLibraryLoaded(lib)) return;
    void* h = dlopen(lib, RTLD_NOW);
    if (!h) {
        const std::string abs = Sf3MappedLibPath(lib);
        if (!abs.empty()) h = dlopen(abs.c_str(), RTLD_NOW);
    }
    if (!h) {
        LOGW(SF3_LOG " checkCopyright: dlopen(%s) failed", lib);
        return;
    }
    void* sym = dlsym(h, Sf3SymCheckCopyright());
    if (!sym) {
        LOGW(SF3_LOG " checkCopyright: symbol miss in %s", lib);
        return;
    }
    if (DobbyHook(sym, (dobby_dummy_func_t)Hooked_CheckCopyright,
                  (dobby_dummy_func_t*)&g_origCheckCopyright) == 0) {
        g_copyrightHooked.store(true, std::memory_order_release);
        LOGI(SF3_LOG " checkCopyright hooked @%p (force genuine)", sym);
    } else {
        LOGW(SF3_LOG " checkCopyright DobbyHook failed @%p", sym);
    }
}

// libcpp.so 可能比 il2cpp 更早载入，也可能晚；独立轮询线程保证在
// AccessSDK 跑 InitServerCfg 之前挂上（不能等 initDelayMs=5s 的 tryInit）。
static void* CopyrightPatchThread(void* /*arg*/) {
    for (int i = 0; i < 900; i++) {          // 约 90s
        TryInstallCopyrightHook();
        if (g_copyrightHooked.load(std::memory_order_acquire)) return nullptr;
        usleep(100 * 1000);
    }
    LOGW(SF3_LOG " checkCopyright 未安装（libcpp.so 未出现或符号缺失）");
    return nullptr;
}

void Sf3_OnLoad(JavaVM* /*vm*/) {
    bool expected = false;
    if (!g_copyrightThreadStarted.compare_exchange_strong(expected, true)) return;
    pthread_t th {};
    if (pthread_create(&th, nullptr, CopyrightPatchThread, nullptr) == 0) {
        pthread_detach(th);
        LOGI(SF3_LOG " copyright patch thread started");
    } else {
        g_copyrightThreadStarted.store(false);
        LOGW(SF3_LOG " copyright patch thread create failed");
    }
}

// ============================================================
// ⑤ hook 实现
// ============================================================
static void Hooked_ChangeLife(void* self, void* a1, void* a2, void* unit, void* value,
                              void* a5, void* a6, void* a7) {
    ReentryGuard guard(t_inDamageHook);   // 未发生自递归才动值；发生则直接透传原调用
    const bool wantGod = g_god.load(std::memory_order_relaxed);
    const int mult = g_dmgMult.load(std::memory_order_relaxed);

    if ((wantGod || mult > 1) &&
        OkPtr(self) && OkPtr(unit) && OkPtr(value) &&
        g_valueGet && g_valueSet && g_containerClass) {
        auto& api = GetApi();
        // 容器类校验：类型不符就是挂点语义漂移，宁可不改也不写坏内存
        if (api.object_class && api.object_class(value) == g_containerClass) {
            void* player = Il2Cpp::ReadPtr(self, kOffPlayerUnit);
            void* enemy  = Il2Cpp::ReadPtr(self, kOffEnemyUnit);
            if (OkPtr(player) && OkPtr(enemy) && player != enemy) {
                // 两个单位字段都必须是单位类（偏移漂移时直接放弃，不猜）
                const bool fieldsOk = !g_unitClass ||
                        (api.object_class(player) == g_unitClass && api.object_class(enemy) == g_unitClass);
                if (fieldsOk) {
                    const bool hitPlayer = wantGod && unit == player;
                    const bool hitEnemy  = (mult > 1) && unit == enemy;
                    // 只写一次：2026-09-19 修重复落值（原代码两份相同块 ⇒ 敌人吃两层倍率=平方）。
                    if (hitPlayer || hitEnemy) {
                        const float v = g_valueGet(value, nullptr);
                        if (v > 0.0f && v == v) {
                            const float nv = hitPlayer ? 0.0f : v * static_cast<float>(mult);
                            int n = s_hitLogged.load(std::memory_order_relaxed);
                            if (n < 5 && s_hitLogged.compare_exchange_strong(n, n + 1)) {
                                LOGI(SF3_LOG " hit %s v=%.3f→%.3f (self=%p unit=%p)",
                                     hitPlayer ? "player" : "enemy", v, nv, self, unit);
                            }
                            g_valueSet(value, nv, nullptr);
                        }
                    }
                }
            }
        }
    }

    if (g_origChangeLife) g_origChangeLife(self, a1, a2, unit, value, a5, a6, a7);
}

static void Hooked_SetTimeScale(float value, void* method) {
    const int speed = g_speed.load(std::memory_order_relaxed);
    if (value >= 0.0f && value < 1000.0f) {
        g_lastBaseScale.store(value, std::memory_order_relaxed);
        if (speed > 1 && value > 0.0f) {
            float nv = value * static_cast<float>(speed);
            if (nv > kSpeedMax) nv = kSpeedMax;
            g_appliedScale.store(nv, std::memory_order_relaxed);
            if (g_origSetTimeScale) g_origSetTimeScale(nv, method);
            return;
        }
        g_appliedScale.store(value, std::memory_order_relaxed);
    }
    if (g_origSetTimeScale) g_origSetTimeScale(value, method);
}

static int64_t Hooked_EnergyCurrent(void* self, void* method) {
    if (g_energy.load(std::memory_order_relaxed) && OkPtr(self)) return kEnergyValue;
    return g_origEnergyCurrent ? g_origEnergyCurrent(self, method) : 0;
}

static bool Hooked_EnergyEnough(void* self, int64_t cost, void* method) {
    if (g_energy.load(std::memory_order_relaxed) && OkPtr(self)) return true;
    return g_origEnergyEnough ? g_origEnergyEnough(self, cost, method) : true;
}

static bool Hooked_EnergyRegen(void* self, void* method) {
    if (g_energy.load(std::memory_order_relaxed) && OkPtr(self)) return false;
    return g_origEnergyRegen ? g_origEnergyRegen(self, method) : false;
}


static const HookSlot kDamageSlots[] = {
    {"ChangeLife(OGFLOMPOMCI)", kFileChangeLife, kInsChangeLife, "", "IKHOBFONGIE", "OGFLOMPOMCI", 5,
     (void*)Hooked_ChangeLife, (void**)&g_origChangeLife},
};

static const HookSlot kSpeedSlots[] = {
    {"Time.set_timeScale", kFileSetTimeScale, kInsSetTimeScale, "UnityEngine", "Time", "set_timeScale", 1,
     (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale},
};

static const HookSlot kEnergySlots[] = {
    {"Energy.HJJFFHOJJFB", kFileEnergyNow, kInsEnergyNow, "", "IGCAMALALAJ", "HJJFFHOJJFB", 0,
     (void*)Hooked_EnergyCurrent, (void**)&g_origEnergyCurrent},
    {"Energy.OBPEJEPEJMF", kFileEnergyEnough, kInsEnergyEnough, "", "IGCAMALALAJ", "OBPEJEPEJMF", 1,
     (void*)Hooked_EnergyEnough, (void**)&g_origEnergyEnough},
    {"Energy.EIPOOCIBMBO", kFileEnergyRegen, kInsEnergyRegen, "", "IGCAMALALAJ", "EIPOOCIBMBO", 0,
     (void*)Hooked_EnergyRegen, (void**)&g_origEnergyRegen},
};

// ============================================================
// ⑥ 运行期解析（伤害容器 getter/setter + 类指针）
// ============================================================
static bool ResolveDamageBits() {
    if (!EnsureApi()) return false;
    auto& api = GetApi();
    if (!g_containerClass) g_containerClass = api.FindClassByShortName("LJMJLLJNEMK");
    if (!g_unitClass) g_unitClass = api.FindClassByShortName("HJGEAPMLNAF");
    if (!g_valueGet) {
        void* va = VaAtFileOffChecked("ValueGet", kFileValueGet, kInsValueGet);
        if (!va) {
            void* k = FindClassNs("", "LJMJLLJNEMK");
            va = MethodFn(k, "JDMGBLEBMAJ", 0);
        }
        g_valueGet = reinterpret_cast<ValueGet_t>(va);
    }
    if (!g_valueSet) {
        void* va = VaAtFileOffChecked("ValueSet", kFileValueSet, kInsValueSet);
        if (!va) {
            void* k = FindClassNs("", "LJMJLLJNEMK");
            va = MethodFn(k, "JBEFILOFEKD", 1);
        }
        g_valueSet = reinterpret_cast<ValueSet_t>(va);
    }
    if (!g_valueGet || !g_valueSet) {
        LOGW(SF3_LOG " value get/set unresolved — 伤害倍率/上帝模式降级");
        return false;
    }
    if (!g_containerClass) {
        LOGW(SF3_LOG " container class miss — 伤害倍率/上帝模式降级（防写坏内存）");
        return false;
    }
    LOGI(SF3_LOG " damage bits ok (container=%p unit=%p get=%p set=%p)",
         g_containerClass, g_unitClass, (void*)g_valueGet, (void*)g_valueSet);
    return true;
}

// 滑块变更后立刻生效：用 orig 重放 base*speed（value==0 暂停语义不变）
static void ApplyTimeScaleNow() {
    if (!g_origSetTimeScale) return;
    float base = g_lastBaseScale.load(std::memory_order_relaxed);
    if (!(base >= 0.0f) || base >= 1000.0f) base = 1.0f;
    const int speed = g_speed.load(std::memory_order_relaxed);
    float target = base * static_cast<float>(speed);
    if (target > kSpeedMax) target = kSpeedMax;
    if (g_appliedScale.load(std::memory_order_relaxed) == target) return;
    g_appliedScale.store(target, std::memory_order_relaxed);
    g_origSetTimeScale(target, nullptr);
    LOGI(SF3_LOG " timeScale replay base=%.3f speed=%d → %.3f", base, speed, target);
}

// ============================================================
// ⑦ 分组安装 + 懒装（开关全关 = 0 hook）
// ============================================================
static void SyncHooks(bool force) {
    if (!g_libReady.load()) return;
    std::lock_guard<std::mutex> lock(g_installMutex);
    const int64_t now = NowMs();
    if (!force) {
        const int64_t last = g_lastInstallMs.load(std::memory_order_relaxed);
        if (last && now > last && now - last < static_cast<int64_t>(kInstallRetryMs)) return;
    }
    g_lastInstallMs.store(now, std::memory_order_relaxed);

    const bool wantDamage = g_god.load() || g_dmgMult.load() > 1;
    if (wantDamage && (!g_valueGet || !g_valueSet || !g_containerClass)) ResolveDamageBits();
    if (wantDamage) {
        InstallTable(kDamageSlots, sizeof(kDamageSlots) / sizeof(kDamageSlots[0]));
        g_damageHooked.store(g_origChangeLife != nullptr);
    }
    if (g_speed.load() > 1) {
        InstallTable(kSpeedSlots, sizeof(kSpeedSlots) / sizeof(kSpeedSlots[0]));
        g_speedHooked.store(g_origSetTimeScale != nullptr);
        ApplyTimeScaleNow();
    }
    if (g_energy.load()) {
        InstallTable(kEnergySlots, sizeof(kEnergySlots) / sizeof(kEnergySlots[0]));
        g_energyHooked.store(g_origEnergyCurrent != nullptr);
    }
}

// ============================================================
// ⑧ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Sf3_TryInit(JavaVM* /*vm*/) {
    TryInstallCopyrightHook();   // 版权补丁不依赖 il2cpp，独立于功能开关先试一次
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(SF3_TARGET_LIB)) return false;
    g_libReady.store(true);
    SyncHooks(true);  // 已 ON 的开关立即装；全关则保持 0 hook
    LOGI(SF3_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Sf3_IsReady() { return g_libReady.load(); }

void Sf3_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case SF3_FEAT_DMG: {
            const int cur = g_dmgMult.load();
            g_dmgMult.store(enabled ? (cur > 1 ? cur : 10) : 1);
            break;
        }
        case SF3_FEAT_GOD:    g_god.store(enabled); break;
        case SF3_FEAT_SPEED: {
            const int cur = g_speed.load();
            g_speed.store(enabled ? (cur > 1 ? cur : 5) : 1);
            break;
        }
        case SF3_FEAT_ENERGY: g_energy.store(enabled); break;
        default: break;
    }
    SyncHooks(true);
}

void Sf3_OnValueChanged(int featNum, int value) {
    if (featNum == SF3_FEAT_DMG) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        g_dmgMult.store(value);
        LOGI(SF3_LOG " damage slider=%d", value);
        SyncHooks(true);
        return;
    }
    if (featNum == SF3_FEAT_SPEED) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        g_speed.store(value);
        LOGI(SF3_LOG " speed slider=%d", value);
        SyncHooks(true);
        return;
    }
}

void Sf3_Tick(const bool* features, const int* values) {
    // 只做：同步开关 + 兜底安装。禁止在这里扫堆/遍历对象/逐对象写。
    TryInstallCopyrightHook();   // 兜底：libcpp.so 晚载时补挂版权补丁
    if (features) {
        if (values && values[SF3_FEAT_DMG] > 0) {
            int mult = values[SF3_FEAT_DMG];
            if (mult < 1) mult = 1;
            if (mult > 10) mult = 10;
            g_dmgMult.store(mult);
        }
        g_god.store(features[SF3_FEAT_GOD]);
        if (values && values[SF3_FEAT_SPEED] > 0) {
            int speed = values[SF3_FEAT_SPEED];
            if (speed < 1) speed = 1;
            if (speed > 10) speed = 10;
            g_speed.store(speed);
        }
        g_energy.store(features[SF3_FEAT_ENERGY]);
    }
    if (!g_libReady.load() && isLibraryLoaded(SF3_TARGET_LIB)) g_libReady.store(true);
    SyncHooks(false);
    if (g_origSetTimeScale && g_speed.load() > 1) ApplyTimeScaleNow();
}

void Sf3_ForceClear() {
    g_dmgMult.store(1);
    g_god.store(false);
    g_speed.store(1);
    g_energy.store(false);
    if (g_origSetTimeScale) {
        float base = g_lastBaseScale.load();
        if (!(base >= 0.0f) || base >= 1000.0f) base = 1.0f;
        g_appliedScale.store(base);
        g_origSetTimeScale(base, nullptr);
    }
}
