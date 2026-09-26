/* ================================================================
 *  Unity IL2CPP 公共层 — 下一款游戏复用这里，游戏逻辑放 Games/*.cpp
 *
 *  约定：
 *  - 等 libil2cpp.so 映射后再 Load()
 *  - HybridCLR 字段偏移 = il2cpp_field_get_offset + bias（战魂 bias=8）
 *  - 禁止 Dobby 内联 Hook 过短/相邻的导出桩（如 runtime_invoke 旁紧挨
 *    object_init*），优先堆扫描 + runtime_invoke 直调 setter
 * ================================================================ */
#ifndef STARRY_IL2CPP_API_H
#define STARRY_IL2CPP_API_H

#include <csetjmp>
#include <csignal>
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <pthread.h>
#include <vector>

namespace Il2Cpp {

typedef void* (*class_from_name_t)(void*, const char*, const char*);
typedef void* (*class_get_field_from_name_t)(void*, const char*);
typedef size_t (*field_get_offset_t)(void*);
typedef void (*field_static_get_value_t)(void*, void*);
typedef void* (*domain_get_t)();
typedef void** (*domain_get_assemblies_t)(void*, size_t*);
typedef void* (*domain_assembly_open_t)(void*, const char*);
typedef const void* (*assembly_get_image_t)(const void*);
typedef void* (*object_get_class_t)(void*);
typedef const char* (*class_get_name_t)(void*);
typedef void* (*class_get_method_from_name_t)(void*, const char*, int);
typedef void* (*runtime_invoke_t)(void*, void*, void**, void**);
typedef void* (*string_new_t)(const char*);
typedef const char* (*method_get_name_t)(void*);
typedef uint32_t (*image_get_class_count_t)(void*);
typedef void* (*image_get_class_t)(void*, uint32_t);
typedef void* (*value_box_t)(void*, void*);
typedef void* (*get_corlib_t)();
typedef void* (*class_get_fields_t)(void*, void**);   // 迭代器：iter 初始 *iter=nullptr
typedef const char* (*field_get_name_t)(void*);
typedef void* (*class_get_parent_t)(void*);
typedef void* (*field_get_type_t)(void*);             // FieldInfo -> Il2CppType*
typedef char* (*type_get_name_t)(void*);              // Il2CppType* -> 全名（需 free）
typedef void* (*thread_attach_t)(void*);              // domain -> Il2CppThread*（非 il2cpp 线程 invoke 前必须）
typedef void* (*method_get_return_type_t)(void*);     // MethodInfo -> Il2CppType*
typedef void* (*method_get_param_t)(void*, uint32_t); // MethodInfo, idx -> Il2CppType*
typedef uint32_t (*method_get_flags_t)(void*);        // MethodInfo -> flags（0x10=static）

struct Api {
    class_from_name_t            class_from_name = nullptr;
    class_get_field_from_name_t  class_get_field = nullptr;
    field_get_offset_t           field_offset = nullptr;
    field_static_get_value_t     field_static_get = nullptr;
    domain_get_t                 domain_get = nullptr;
    domain_get_assemblies_t      domain_assemblies = nullptr;
    domain_assembly_open_t       domain_assembly_open = nullptr;
    assembly_get_image_t         assembly_image = nullptr;
    object_get_class_t           object_class = nullptr;
    class_get_name_t             class_name = nullptr;
    class_get_method_from_name_t class_method = nullptr;
    runtime_invoke_t             runtime_invoke = nullptr;
    string_new_t                 string_new = nullptr;
    image_get_class_count_t      image_class_count = nullptr;
    image_get_class_t            image_class = nullptr;
    value_box_t                  value_box = nullptr;
    method_get_name_t            method_get_name = nullptr;
    get_corlib_t                 get_corlib = nullptr;
    class_get_fields_t           class_fields = nullptr;   // 可空：老包无此导出
    field_get_name_t             field_name = nullptr;     // 可空
    class_get_parent_t           class_parent = nullptr;   // 可空
    field_get_type_t             field_type = nullptr;     // 可空
    type_get_name_t              type_name = nullptr;      // 可空
    thread_attach_t              thread_attach = nullptr;  // 可空
    method_get_return_type_t     method_ret_type = nullptr; // 可空
    method_get_param_t           method_param = nullptr;    // 可空
    method_get_flags_t           method_flags = nullptr;    // 可空

    bool ready = false;
    /* Assemblies 降级缓存：TP 删枚举符号时按名 open 程序集（const 方法内写 → mutable） */
    mutable std::vector<void*> asm_fallback_;
    mutable bool asm_fallback_done_ = false;

    bool Load(const char* libName = sec_lib_il2cpp());

    /* xdl_sym 在 TP 重排 dynsym 后会静默返回「错误的非空地址」。
     * 本方法用进程内自解析的 dynsym 表重取核心符号并覆盖；返回是否有改动。
     * 仅由游戏模块在 Domain()/Assemblies() 失败时按需调用（opt-in，不影响其它游戏）。 */
    bool ReprobeCore();

    void* Domain() const;
    void** Assemblies(size_t* count) const;
    void* ImageOf(void* assembly) const;

    void* FindClassInImage(void* image, const char* shortName) const;
    void* FindClassByShortName(const char* shortName) const;
    void* ClassFromName(void* image, const char* ns, const char* name) const;
    void* GetMethod(void* klass, const char* name, int argc) const;
    size_t FieldOffset(void* klass, const char* name, size_t bias = 0) const;
    /** 读引用类型静态字段（如 SceneInfoSingleton.instance） */
    void* StaticRefField(void* klass, const char* name) const;

    const char* ClassName(void* obj) const;
    void* GetCorlib() const;
    void* BoxInt32(int32_t v) const;
    void* BoxBool(bool v) const;

    void ScanInstances(void* klass, std::vector<void*>& out, size_t maxScanMB = 2048) const;
};

Api& GetApi();

bool IsValidHeapPtr(void* p);

inline int32_t ReadI32(void* p, size_t off) { return *(int32_t*)((uintptr_t)p + off); }
inline void WriteI32(void* p, size_t off, int32_t v) { *(int32_t*)((uintptr_t)p + off) = v; }
inline float ReadF32(void* p, size_t off) { return *(float*)((uintptr_t)p + off); }
inline void WriteF32(void* p, size_t off, float v) { *(float*)((uintptr_t)p + off) = v; }
inline uint8_t ReadU8(void* p, size_t off) { return *(uint8_t*)((uintptr_t)p + off); }
inline void WriteU8(void* p, size_t off, uint8_t v) { *(uint8_t*)((uintptr_t)p + off) = v; }
inline void* ReadPtr(void* p, size_t off) { return *(void**)((uintptr_t)p + off); }

void SetScanThread(pthread_t t);

namespace detail {
bool PushJmp(sigjmp_buf* buf);
void PopJmp();
void InstallHandlers(struct sigaction* oldSegv, struct sigaction* oldBus);
void RestoreHandlers(const struct sigaction* oldSegv, const struct sigaction* oldBus);
void SetInScan(int v);
} // namespace detail

template <typename F>
bool SafeProbe(F fn) {
    sigjmp_buf buf;
    if (!detail::PushJmp(&buf)) return false;
    struct sigaction oldSegv, oldBus;
    detail::InstallHandlers(&oldSegv, &oldBus);
    bool ok = false;
    if (sigsetjmp(buf, 1) == 0) {
        detail::SetInScan(1);
        ok = fn();
        detail::SetInScan(0);
    } else {
        detail::SetInScan(0);
        ok = false;
    }
    detail::RestoreHandlers(&oldSegv, &oldBus);
    detail::PopJmp();
    return ok;
}

template <typename F>
void SafeScan(F fn) {
    SafeProbe([&]() -> bool { fn(); return true; });
}

} // namespace Il2Cpp

#endif // STARRY_IL2CPP_API_H#include "Includes/SecStrings.hpp"

