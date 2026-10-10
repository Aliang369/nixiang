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
typedef const char* (*class_get_namespace_t)(void*);
typedef void* (*class_get_method_from_name_t)(void*, const char*, int);
typedef void* (*class_get_methods_t)(void*, void**);  // 迭代器：iter 初始 *iter=nullptr（可空：老包无此导出）
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
/* ★2026-10-05 增加：`il2cpp_method_get_param_count(const MethodInfo*)` 确实存在
 *（离线实测 nhsy2 的 libil2cpp dynsym idx=928 / st_value=0x2fe61d8，落在本模块可执行段内）。
 * 旧注释「class_methods 迭代器没有配套 param_count 导出」是错的。 */
typedef uint32_t (*method_get_param_count_t)(void*);
/* ★2026-10-03 修正（MuMu 实机血证，务必不要再改回单参）：
 *  `il2cpp_method_get_flags(const MethodInfo* method, uint32_t* iflags)` 是**双参** API，
 *  第二参是「输出型 iflags 指针」。旧声明只有 `(void*)`，调用方只传 1 个参数 ⇒ callee 的 x1
 *  带着**上一条指令留下的残留值**（可能是任何地址）→ callee 末尾 `str w0,[x20]` 写那个地址。
 *  实测崩在 `il2cpp_method_get_flags+0x1c`（`str w0,[x20]`）SIGSEGV_ACCERR，x1 = libil2cpp 代码段
 *  内的只读地址 `0x400020807ac6`（反汇编：`20b16b8 cbz x1,20b16cc` / `20b16c8 str w0,[x20]`）。
 *  ⇒ 调用时第二参写 `nullptr` 是**安全**的：`cbz x1` 会跳过那次写入（同上反汇编实证）。
 *  返回值为 `flags`（`0x10` = METHOD_ATTRIBUTE_STATIC），第二参只收 `iflags`，我们不关心。 */
typedef uint32_t (*method_get_flags_t)(void*, uint32_t* iflags);  // MethodInfo -> flags（0x10=static）

/* ⚠️ 成员实名速查（写游戏模块时最易记错；2026-10-01 失落城堡因写 `class_get` 编译失败）：
 *   class_get_field      ← 不是 class_get / class_get_field_from_name（底层 typedef 才长那样）
 *   class_method         ← 不是 class_get_method
 *   class_name / class_parent / class_fields
 *   field_offset / field_static_get / field_name / field_type
 *   method_get_name / method_ret_type / method_param / method_flags
 *   object_class / runtime_invoke / value_box / type_name / thread_attach
 *   domain_get / domain_assemblies / domain_assembly_open / assembly_image
 *   string_new / get_corlib / image_class_count / image_class */
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
    class_get_namespace_t        class_namespace = nullptr; // 可空：ClassFromName 降级扫描用
    class_get_method_from_name_t class_method = nullptr;
    method_get_param_count_t     method_param_count = nullptr; // 可空：GetMethod 降级扫描用
    runtime_invoke_t             runtime_invoke = nullptr;
    string_new_t                 string_new = nullptr;
    image_get_class_count_t      image_class_count = nullptr;
    image_get_class_t            image_class = nullptr;
    value_box_t                  value_box = nullptr;
    method_get_name_t            method_get_name = nullptr;
    get_corlib_t                 get_corlib = nullptr;
    class_get_fields_t           class_fields = nullptr;   // 可空：老包无此导出
    class_get_methods_t          class_methods = nullptr;  // 可空：GetMethodBySig 的重载消歧用
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
    /** 降级类查找（2026-10-05 增加）。专治「`il2cpp_class_from_name` 的 dynsym
     *  st_value 被保护壳伪造成指向自己（themis-v8）的负偏移」的场景：该符号被
     *  `ResolveIl2CppSym` 的模块范围校验置空后，用**未被污染**的三件套
     *  `image_get_class_count` + `image_class` + `class_name`（+ `class_namespace`）
     *  做线性扫描，并**按 image 懒建一次完整哈希表**（后续 O(1)）。
     *  正常包（class_from_name 可用）不会走到这里。 */
    void* ClassFromNameScan(void* image, const char* ns, const char* name) const;
    /** 降级方法查找（2026-10-05 增加）。同上，`il2cpp_class_get_method_from_name`
     *  被置空时改用 `class_methods` 迭代器 + `method_get_name` +
     *  `method_param_count` 线性匹配（argc<0 表示不校验参数个数）。 */
    void* GetMethodScan(void* klass, const char* name, int argc) const;
    void* ClassFromName(void* image, const char* ns, const char* name) const;
    void* GetMethod(void* klass, const char* name, int argc) const;
    /** 重载消歧（2026-10-01 增加）。`GetMethod(name, argc)` 只能返回匹配 argc 的**第一个**候选，
     *  同参数个数的重载无法区分 —— 失落城堡实测 `op_Implicit` 有 4 个 argc=1 重载，选错静默行为异常。
     *
     *  本方法先遍历类方法按 name 收候选，再用「类型全名后缀」筛：
     *    retSuffix  期望的**返回类型**名后缀（空/null 表示不校验）
     *    p0Suffix   期望的**第 0 个参数**类型名后缀（空/null 表示不校验）
     *  后缀比较用尾部子串（如 `"ObscuredInt"` 可匹配 `CodeStage.AntiCheat.ObscuredTypes.ObscuredInt`）。
     *
     *  返回规则：唯一命中 → 该 MethodInfo；0 命中 / 多命中 / 迭代器或类型 API 不可用
     *  → **退回 `GetMethod(klass, name, argc)`**（行为与改造前一致，不引入新的不确定性，
     *  但调用方应按需自行校验拿到的方法，别假定一定对）。两个后缀都为空时直接走 GetMethod。
     *  ⚠️ 本方法不校验 argc；如 `method_param_count` 可用，`GetMethod` 自身的降级
     *  路径会校验（见 `GetMethodScan`）。
     *  例：`api.GetMethodBySig(k, "op_Implicit", 1, "ObscuredInt", "Int32")` */
    void* GetMethodBySig(void* klass, const char* name, int argc,
                         const char* retSuffix, const char* p0Suffix) const;
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

/** maps 级可读/可执行判定：[addr, addr+len) 必须完整落在单个映射内。
 *  装 inline hook / 读目标方法前 12 字节之前必须先过这一关（HybridCLR 解释方法的
 *  methodPointer 可能是 0x400000000000 这类「标记值」，直接解引用即致命故障）。
 *  内部 fopen /proc/self/maps，仅低频/装钩路径可用。 */
bool IsReadableRange(uintptr_t addr, size_t len);
bool IsExecutableRange(uintptr_t addr, size_t len);

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

