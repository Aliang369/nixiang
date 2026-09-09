/* ================================================================
 *  山河旅探 1.4.38 / vc7
 *
 *  玩法在 ILRuntime HotFix_Project.dll（YooAsset 会再拉 CDN 包
 *  2025-02-14-1015，APK 内 1.4.38 热更会被盖掉）。
 *
 *  钩 ILRuntime.AppDomain.LoadAssembly（只在启动加载 DLL 时进一次），
 *  在解析前把购买/关卡 IL 改成恒真。禁止钩 ILIntepreter.Execute。
 * ================================================================ */

#include "ShltMod.h"

#include <atomic>
#include <cstdint>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

enum : size_t {
    kOffMemStreamBuffer = 0x28,  // MemoryStream._buffer
    kOffMemStreamOrigin = 0x30,  // int
    kOffMemStreamLength = 0x38,  // int
    kOffArrayLen        = 0x18,
    kOffArrayData       = 0x20,
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_chapter{false};
static std::atomic<bool> g_skin{false};
static std::atomic<int> g_patched{0};

typedef void (*LoadAssembly_t)(void* thiz, void* stream, void* symbol, void* provider,
                               void* method);
static LoadAssembly_t g_origLoadAssembly = nullptr;

static uint8_t* MemMem(uint8_t* hay, int hlen, const uint8_t* needle, int nlen) {
    if (!hay || hlen < nlen || nlen <= 0) return nullptr;
    for (int i = 0; i <= hlen - nlen; i++) {
        if (memcmp(hay + i, needle, (size_t)nlen) == 0) return hay + i;
    }
    return nullptr;
}

static int PatchOne(uint8_t* hay, int hlen, const uint8_t* pat, int plen, uint8_t a, uint8_t b,
                    const char* tag) {
    uint8_t* p = MemMem(hay, hlen, pat, plen);
    if (!p) return 0;
    p[0] = a;
    p[1] = b;
    LOGI("[Shlt] patched %s", tag);
    return 1;
}

static int PatchSkinTuple(uint8_t* hay, int hlen) {
    static const uint8_t kPat[] = {0x00, 0x00, 0x16, 0x0A, 0x16, 0x0B, 0x28, 0xCE, 0x00, 0x00, 0x0A};
    uint8_t* p = MemMem(hay, hlen, kPat, (int)sizeof(kPat));
    if (!p) return 0;
    int window = 512;
    int remain = (int)((hay + hlen) - p);
    if (remain < window) window = remain;
    int bestCnt = 0;
    uint8_t bestTok[5] = {0x73, 0x75, 0x0E, 0x00, 0x0A};
    for (int i = 0; i <= window - 5; i++) {
        if (p[i] != 0x73 || p[i + 4] != 0x0A) continue;
        int cnt = 0;
        for (int j = 0; j <= window - 5; j++) {
            if (memcmp(p + j, p + i, 5) == 0) cnt++;
        }
        if (cnt > bestCnt) {
            bestCnt = cnt;
            memcpy(bestTok, p + i, 5);
        }
    }
    p[0] = 0x17;
    p[1] = 0x17;
    memcpy(p + 2, bestTok, 5);
    p[7] = 0x2A;
    LOGI("[Shlt] patched IsSkinPurchased newobj=%02x%02x%02x%02x%02x", bestTok[0], bestTok[1],
         bestTok[2], bestTok[3], bestTok[4]);
    return 1;
}

static void PatchHotfixDll(uint8_t* data, int len) {
    if (!data || len < 0x400 || data[0] != 'M' || data[1] != 'Z') return;
    int n = 0;
    static const uint8_t kChapter[] = {0x00, 0x00, 0x03, 0x72, 0xA1, 0x0E, 0x00, 0x70,
                                       0x6F, 0xC7, 0x00, 0x00, 0x0A, 0x0A, 0x06, 0x2C};
    static const uint8_t kLimited[] = {0x00, 0x02, 0x7B, 0x9C, 0x1C, 0x00, 0x04, 0x03,
                                       0x6F, 0x60, 0x02, 0x00, 0x0A};
    static const uint8_t kAchieve[] = {0x00, 0x02, 0x03, 0x28, 0x86, 0x23, 0x00, 0x06,
                                       0x0A, 0x06, 0x2C, 0x12};
    static const uint8_t kStage[] = {0x00, 0x03, 0x72, 0xA1, 0x0E, 0x00, 0x70, 0x6F,
                                     0xC7, 0x00, 0x00, 0x0A, 0x2C, 0x0D};
    n += PatchOne(data, len, kChapter, (int)sizeof(kChapter), 0x17, 0x2A, "IsChapterPurchased");
    n += PatchOne(data, len, kLimited, (int)sizeof(kLimited), 0x16, 0x2A, "IsActivityLimitedSkin");
    n += PatchOne(data, len, kAchieve, (int)sizeof(kAchieve), 0x17, 0x2A, "GetAchievementUnLock");
    n += PatchOne(data, len, kStage, (int)sizeof(kStage), 0x17, 0x2A, "CheckChapterStageIsUnlocked");
    n += PatchSkinTuple(data, len);
    if (n > 0) {
        g_patched.store(n);
        LOGI("[Shlt] hotfix IL patched %d sites len=%d", n, len);
    }
}

static void PatchFromStream(void* stream) {
    if (!stream || !Il2Cpp::IsValidHeapPtr(stream)) return;
    const char* cn = GetApi().ClassName(stream);
    if (!cn || strcmp(cn, "MemoryStream") != 0) return;
    void* arr = Il2Cpp::ReadPtr(stream, kOffMemStreamBuffer);
    if (!arr || !Il2Cpp::IsValidHeapPtr(arr)) return;
    int origin = Il2Cpp::ReadI32(stream, kOffMemStreamOrigin);
    int length = Il2Cpp::ReadI32(stream, kOffMemStreamLength);
    int alen = Il2Cpp::ReadI32(arr, kOffArrayLen);
    if (alen < 0x400 || alen > 16 * 1024 * 1024) return;
    if (origin < 0 || length < 0x400 || origin > alen || length > alen - origin) {
        origin = 0;
        length = alen;
    }
    auto* data = reinterpret_cast<uint8_t*>(arr) + kOffArrayData + origin;
    PatchHotfixDll(data, length);
}

static void Hooked_LoadAssembly(void* thiz, void* stream, void* symbol, void* provider,
                                void* method) {
    PatchFromStream(stream);
    if (g_origLoadAssembly) g_origLoadAssembly(thiz, stream, symbol, provider, method);
}

static void* MethodPointer(void* mi) { return mi ? *reinterpret_cast<void**>(mi) : nullptr; }

static void* FindIlRuntimeAppDomain() {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.ClassFromName(img, "ILRuntime.Runtime.Enviorment", "AppDomain");
        if (k) return k;
    }
    return nullptr;
}

bool Shlt_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    auto& api = GetApi();
    if (!api.Load()) return false;
    if (!api.Domain()) return false;

    if (!g_hooked.load()) {
        void* klass = FindIlRuntimeAppDomain();
        if (!klass) {
            LOGI("[Shlt] wait ILRuntime AppDomain class");
            return false;
        }
        void* mi = api.GetMethod(klass, "LoadAssembly", 3);
        void* fp = MethodPointer(mi);
        if (!fp) {
            LOGI("[Shlt] LoadAssembly MethodPointer empty");
            return false;
        }
        if (DobbyHook(fp, (void*)Hooked_LoadAssembly, (void**)&g_origLoadAssembly) != 0) {
            LOGE("[Shlt] DobbyHook LoadAssembly failed @ %p", fp);
            return false;
        }
        g_hooked.store(true);
        LOGI("[Shlt] hooked LoadAssembly @ %p", fp);
    }

    g_ready.store(true);
    LOGI("[Shlt] ready package=%s", GameUnity::PACKAGE);
    return true;
}

bool Shlt_IsReady() { return g_ready.load(); }

void Shlt_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == SHLT_FEAT_CHAPTER) {
        g_chapter.store(enabled);
        LOGI("[Shlt] 解锁所有章节 %s（LoadAssembly IL / 热更缓存）", enabled ? "ON" : "OFF");
    }
    if (featNum == SHLT_FEAT_SKIN) {
        g_skin.store(enabled);
        LOGI("[Shlt] 解锁所有服饰 %s（LoadAssembly IL / 热更缓存）", enabled ? "ON" : "OFF");
    }
}

void Shlt_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Shlt_Tick(const bool* features) {
    if (!features) return;
    g_chapter.store(features[SHLT_FEAT_CHAPTER]);
    g_skin.store(features[SHLT_FEAT_SKIN]);
    if (!g_ready.load()) Shlt_TryInit();
}

void Shlt_ForceClear() {
    g_chapter.store(false);
    g_skin.store(false);
}
