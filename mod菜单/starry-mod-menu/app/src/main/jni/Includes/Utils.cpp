#include "obfuscate.h"
#include "Utils.hpp"
#include <xDL/xDL.h>
#include "Includes/StarryStamp.h"

// ===== 星光包构建戳（OTA 守卫判新旧用）=====
// 定义放这里是因为 32/32 个 Android_*.mk 都编译 Includes/Utils.cpp，无需改各个 mk。
//
// ⚠️ 2026-10-16 修正（小米 byron「覆盖安装后一开就崩」事故）：**光编译到还不够**。
//   戳是个无人引用的 const —— 唯一引用点 starry_build_stamp_marker() 只被 legacy Main.cpp
//   调用，而 modern 入口 MainModern.cpp 零引用 ⇒ 链接期 `-Wl,--gc-sections,--strip-all`
//   把该 section 整段回收 ⇒ so 里根本没有 "STARRYSTAMP1:EPOCH=…" ⇒ Java 侧
//   OtaSoStamp.apkStamp() 恒为 0 ⇒ PlayOtaManager.invalidateOtaIfApkUpdated 的
//   `if (apkStamp > 0 …)` 恒假 ⇒ **覆盖安装后永不清** starry_ota 旧 so ⇒ 运行时按「优先 OTA」
//   加载玩家旧的 so（症状：新包装上了、跑的却是旧 so，一开就崩；清数据/清补丁即恢复）。
//   故必须显式声明 retained：即便无人引用也保留（clang≥13 / NDK≥25 支持 retain）。
#if defined(__has_attribute)
#  if __has_attribute(retain)
#    define STARRY_BUILD_STAMP_KEEP __attribute__((used, retain))
#  else
#    define STARRY_BUILD_STAMP_KEEP __attribute__((used))
#  endif
#else
#  define STARRY_BUILD_STAMP_KEEP __attribute__((used))
#endif
extern "C" STARRY_BUILD_STAMP_KEEP const char kStarryBuildStamp[] = STARRY_STAMP_MARKER;
extern "C" const char *starry_build_stamp_marker(void) { return kStarryBuildStamp; }
extern "C" long long starry_build_epoch(void) { return (long long) STARRY_BUILD_EPOCH; }

std::map<std::string, uintptr_t> lib_links;
bool mainLibLoaded = false;

static uintptr_t getLibraryAddressFromMaps(const char *libraryName) {
    if (!libraryName || !libraryName[0]) return 0;
    FILE *fp = fopen(OBFUSCATE("/proc/self/maps"), OBFUSCATE("rt"));
    if (!fp) return 0;
    char line[512] = {0};
    uintptr_t base = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, libraryName)) continue;
        // 取该 so 最低映射地址（通常为 ELF load bias）
        uintptr_t start = strtoul(line, nullptr, 16);
        if (start == 0) continue;
        if (base == 0 || start < base) base = start;
    }
    fclose(fp);
    return base;
}

uintptr_t getLibraryAddress(const char *libraryName) {
    if (lib_links.count(libraryName) && lib_links[libraryName] != 0) {
        return lib_links[libraryName];
    }
    xdl_info_t info;
    void *handle = xdl_open(libraryName, XDL_DEFAULT);
    memset(&info, 0, sizeof(xdl_info_t));
    if (handle && 0 <= xdl_info(handle, XDL_DI_DLINFO, &info) && info.dli_fbase) {
        lib_links[libraryName] = (uintptr_t) info.dli_fbase;
        xdl_close(handle);
        return lib_links[libraryName];
    }
    if (handle) xdl_close(handle);
    LOGI(OBFUSCATE(">>> xdl_info failed for %s, fallback maps"), libraryName);
    uintptr_t mapsBase = getLibraryAddressFromMaps(libraryName);
    if (mapsBase) {
        lib_links[libraryName] = mapsBase;
        return mapsBase;
    }
    // 不缓存 0，便于后续重试
    return 0;
}

void* getSymAddress(const char *libraryName, const char *SymName, bool relative) {
    xdl_info_t info;
    void *handle = xdl_open(libraryName, XDL_DEFAULT);
    if (handle == nullptr) {
        LOGE(OBFUSCATE("xdl_open failed for %s"), libraryName);
        return nullptr;
    }

    memset(&info, 0, sizeof(xdl_info_t));
    if (0 > xdl_info(handle, XDL_DI_DLINFO, &info)) {
        LOGE(OBFUSCATE(">>> getsym_xdl_info(XDL_DI_DLINFO, %llx, %s" ") : FAILED"), (uintptr_t) handle, SymName);
    }

    void *symbol_addr = xdl_sym(handle, SymName, nullptr); // lookup "dynamic link symbols" in .dynsym

    if (symbol_addr == nullptr) {
        LOGW(OBFUSCATE(">>> !xdl_sym -> xdl_dsym..."));
        symbol_addr = xdl_dsym(handle, SymName, nullptr); // lookup "debugging symbols" in .symtab and ".symtab in .gnu_debugdata
    }

    xdl_close(handle);

    if (relative) {
        return (void*)((uintptr_t) symbol_addr - (uintptr_t) info.dli_fbase);
    } else return symbol_addr;
}

void* getAbsAddress(const char *libraryName, uintptr_t relativeAddr) {
    uintptr_t base = getLibraryAddress(libraryName);
    if (!base) return nullptr;
    return (void*)(base + relativeAddr);
}

void* getRelativeAddress(const char *libraryName, const char *rootOffset, const char *addOffset) {
    uintptr_t offset = str2offset(rootOffset);
    uintptr_t offset2 = str2offset(addOffset);

    if(offset != 0) {
        return getAbsAddress(libraryName, offset + offset2);
    } else {
        return getSymAddress(libraryName, rootOffset, true);
    }
}

void* getAbsoluteAddress(const char *libraryName, const char *relative) {
    uintptr_t offset = str2offset(relative);

    if(offset != 0) {
        return getAbsAddress(libraryName, offset);
    } else {
        return getSymAddress(libraryName, relative, false);
        // ElfScanner is still available... you can use it for advanced searches
    }
}

jboolean isGameLibLoaded(JNIEnv *env, jobject thiz) {
    return mainLibLoaded;
}

bool isLibraryLoaded(const char *libraryName) {
    if (!libraryName || !libraryName[0]) return false;
    char line[512] = {0};
    FILE *fp = fopen(OBFUSCATE("/proc/self/maps"), OBFUSCATE("rt"));
    if (fp != nullptr) {
        while (fgets(line, sizeof(line), fp)) {
            if (strstr(line, libraryName)) {
                // LOGI(OBFUSCATE("main library (%s) loaded: 0x%llx"), libraryName, getLibraryAddress(libraryName));
                mainLibLoaded = true;
                fclose(fp);
                return true;
            }
        }
        fclose(fp);
    }
    // extractNativeLibs=false 的游戏（如超维进化）：so 未解压、直接从 base.apk 内映射，
    // /proc/self/maps 里只有 "base.apk" 没有库名。走 xdl（dl_iterate_phdr，
    // linker 记录的 dlpi_name 为 base.apk!/lib/<abi>/<lib>.so，按后缀可匹配）。
    void *h = xdl_open(libraryName, XDL_DEFAULT);
    if (h) {
        xdl_close(h);
        mainLibLoaded = true;
        return true;
    }
    return false;
}

uintptr_t str2offset(const char *c) {
    int base = 16;
    // See if this function catches all possibilities.
    // If it doesn't, the function would have to be amended
    // whenever you add a combination of architecture and
    // compiler that is not yet addressed.
    static_assert(sizeof(uintptr_t) == sizeof(unsigned long)
                  || sizeof(uintptr_t) == sizeof(unsigned long long));

    // Now choose the correct function ...
    if (sizeof(uintptr_t) == sizeof(unsigned long)) {
        return strtoul(c, nullptr, base);
    }

    // All other options exhausted, sizeof(uintptr_t) == sizeof(unsigned long long))
    return strtoull(c, nullptr, base);
}