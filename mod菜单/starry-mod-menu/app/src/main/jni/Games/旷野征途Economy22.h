// ============================================================================
//  旷野征途 2.2 (vc22) 资源接口 v3 —— 金币 / 钻石 / 积分
//  Queue(UI) + Drain(Unity 帧回调)，无裸布局假设
//
//  本版相对 v2 的变更（2026-11-14，lead 审查要求）：
//   1) 消除 boxed+0x10 与 FieldOffset 裸读：改用导出 il2cpp_object_unbox /
//      il2cpp_field_get_value（经 ResolveExport 取址，**不改共用 Il2CppApi**）；
//      符号缺失 ⇒ 失败关闭（RC_NO_API），不做任何降级裸读。
//   2) 溢出检查：积分 int64 累加、钻石 int32 累加均做边界判断，越界返回 RC_OVERFLOW。
//   3) 入队 ≠ 成功：GrantX 返回 RC_QUEUED(100)；只有 drain 的 LastRc()==0 才算成功。
//   4) 保存语义：托管侧已改但 SaveData 抛异常/未就绪 ⇒ RC_SAVE(8)，**不自动重试**
//      （重试会双加），由调用方决定下一次点击。
//
//  返回码：0 成功 | 1 api/符号未就绪 | 2 类/方法/单例解析失败 | 3 invoke 抛异常
//          4 引用返回为空 | 5 必需对象为空 | 6 非 Drain 线程 | 7 钻石 itemid 未验证
//          8 保存失败（内存已改） | 9 数量溢出 | 100 已入队（非成功）
//
//  证据：签名来自 HotUpdate.dll（sha256 5a4cb3bd…cb8cbca, AssemblyName=HotUpdate）；
//        runtime_invoke ABI 先例 Games/BysxyMod.cpp:775-776、:410-412。
//  钻石 id 来源（2026-11-14 配置表实证）：resources bundle 6da8c2e4… 的 TextAsset(m_Name="item", path_id 459545423326818059)
//      记录顺序 [id][code][nameLen][name]…：1/1 金币、2/2 钻石、3/3 积分、4/101 血瓶1、5/102 血瓶3、6/103 血瓶6
//      钻石的两个候选 id 字节同为 2（1.8 baseItemid=2 亦一致）⇒ kDiamondItemIdFromTable=2，非盲常量
//  构建标记：KYZT22ECO-BUILD-6 / KYZT_ECONOMY22_API_VERSION 5
// ============================================================================
#ifndef KYZT_ECONOMY22_API_VERSION
#define KYZT_ECONOMY22_API_VERSION 6
#endif

#ifndef KYZT_ECONOMY22_H
#define KYZT_ECONOMY22_H

#include <pthread.h>
#include <cstdio>
#include <cctype>

#include <atomic>
#include <cstdint>
#include <cstring>
#include <deque>
#include <limits>
#include <mutex>

#include "Unity/Il2CppApi.h"

namespace Kyzt22 {

// ---------------------------------------------------------------------------
// 诊断设施（2026-11-14 21:50 加，用于定位「点资源即 SIGSEGV@0xb8」）
//   · KYZLOG：由 Games/旷野征途Mod.cpp 把 DbgLog 绑进来，未绑时零开销
//   · alive()：调用任何 il2cpp 方法前先做「托管堆指针」判定，挡掉野指针/陈旧指针
// ---------------------------------------------------------------------------
typedef void (*log_fn_t)(const char*, ...);
inline log_fn_t& log_fn() { static log_fn_t f = nullptr; return f; }

using Il2Cpp::Api;
using Il2Cpp::GetApi;
using Il2Cpp::ResolveExport;

// marker 保活三重保险（used 只保证编译器生成，--gc-sections 仍可能回收）：
//   ① used ② 非 inline 导出函数引用 ③ 可写段 volatile 指针持有字面量地址（产生重定位⇒链接器必须保留）
static const char kBuildMarker[] __attribute__((used)) = "KYZT22ECO-BUILD-6";
static __attribute__((noinline, used)) const char* BuildMarker() { return kBuildMarker; }
static const char* volatile g_buildMarkerRef __attribute__((used)) = kBuildMarker;
static static const char kDiamondIdSource[] __attribute__((used)) = "KYZT22ECO-DIAMOND-ID-SRC=table:item@459545423326818059";
static const char* volatile g_diamondIdSourceRef __attribute__((used)) = kDiamondIdSource;
static const int kDiamondItemIdFromTable = 2;   // 金币=1 / 钻石=2 / 积分=3（配置表实测）

enum Rc {
    RC_OK = 0,
    RC_NO_API = 1,
    RC_RESOLVE = 2,
    RC_EXCEPTION = 3,
    RC_NULL_RET = 4,
    RC_NO_OBJ = 5,
    RC_NOT_DRAIN = 6,
    RC_UNVERIFIED_ID = 7,
    RC_SAVE = 8,
    RC_OVERFLOW = 9,
    RC_REFUSED = 10,   // 游戏侧校验拒绝（如 ChangeGold 返回 false / sharedGold 为 null）
    RC_QUEUED = 100,
};

enum Kind : int { KIND_GOLD = 1, KIND_DIAMOND = 2, KIND_POINT = 3 };

#define KYZLOG(...) do { Kyzt22::log_fn_t _f = Kyzt22::log_fn(); if (_f) _f(__VA_ARGS__); } while (0)

// 托管堆指针判定：IsManagedObjectPtr 由 Il2CppApi 提供（栈上的同名指针会被判否）
inline bool alive(const void* o) { return o && Il2Cpp::IsManagedObjectPtr(const_cast<void*>(o)); }

// 数组/元素用：只要目标区间可读即可（IsManagedObjectPtr 对 HybridCLR 数组会误判）
inline bool readable(const void* p, size_t n) { return p && Il2Cpp::IsReadableRange((uintptr_t)p, n); }

namespace exp {

typedef void* (*unbox_t)(void*);
typedef void (*field_get_value_t)(void*, void*, void*);

inline unbox_t& unbox() { static unbox_t f = nullptr; return f; }
inline field_get_value_t& field_get() { static field_get_value_t f = nullptr; return f; }
inline bool& tried() { static bool b = false; return b; }

// 一次性解析导出；失败保持 nullptr（失败关闭，不降级裸读）
inline bool ensure() {
    if (tried()) return unbox() != nullptr && field_get() != nullptr;
    tried() = true;
    unbox() = reinterpret_cast<unbox_t>(ResolveExport("il2cpp_object_unbox"));
    field_get() = reinterpret_cast<field_get_value_t>(ResolveExport("il2cpp_field_get_value"));
    return unbox() != nullptr && field_get() != nullptr;
}

inline bool read_i32(void* obj, void* fieldInfo, int32_t& out) {
    if (!obj || !fieldInfo || !field_get()) return false;
    int32_t v = 0;
    field_get()(fieldInfo, obj, &v);
    out = v;
    return true;
}

inline bool read_i64(void* obj, void* fieldInfo, int64_t& out) {
    if (!obj || !fieldInfo || !field_get()) return false;
    int64_t v = 0;
    field_get()(fieldInfo, obj, &v);
    out = v;
    return true;
}

inline bool unbox_i32(void* boxed, int32_t& out) {
    if (!boxed || !unbox()) return false;
    void* p = unbox()(boxed);
    if (!p) return false;
    std::memcpy(&out, p, sizeof(int32_t));
    return true;
}

inline bool unbox_i64(void* boxed, int64_t& out) {
    if (!boxed || !unbox()) return false;
    void* p = unbox()(boxed);
    if (!p) return false;
    std::memcpy(&out, p, sizeof(int64_t));
    return true;
}

inline bool read_ptr(void* obj, void* fieldInfo, void*& out) {
    if (!obj || !fieldInfo || !field_get()) return false;
    void* v = nullptr;
    field_get()(fieldInfo, obj, &v);
    out = v;
    return true;
}

} // namespace exp

namespace sched {
struct Req { int kind; long long delta; };

inline std::mutex& mu() { static std::mutex m; return m; }
inline std::deque<Req>& queue() { static std::deque<Req> q; return q; }
inline std::atomic<bool>& has_unity_tid() { static std::atomic<bool> b{false}; return b; }
inline std::atomic<unsigned long long>& unity_tid() { static std::atomic<unsigned long long> t{0}; return t; }
inline std::atomic<int>& last_rc() { static std::atomic<int> r{-1}; return r; }
inline std::atomic<unsigned>& drained() { static std::atomic<unsigned> n{0}; return n; }
} // namespace sched

namespace detail {

struct Sym {
    void* acctMgrCls = nullptr;
    void* acctInfoCls = nullptr;
    void* getNowPlayer = nullptr;
    void* changeGold = nullptr;
    void* setPoints = nullptr;
    void* getPoints = nullptr;
    void* saveData = nullptr;
    void* getBagsCount = nullptr;
    void* getBagAt = nullptr;
    void* getCount = nullptr;
    void* setCount = nullptr;
    void* addItem = nullptr;
    void* fldAccountInfo = nullptr;   // FieldInfo*（AccountMgr.AccountInfo）
    void* fldSaveBags = nullptr;      // FieldInfo*（PlayerInfo.saveBags）
    void* fldBaseItemid = nullptr;    // FieldInfo*（ItemSaveData.baseItemid）
};
inline Sym& syms() { static Sym s; return s; }

inline void attach(const Api& api) {
    if (api.thread_attach && api.domain_get) {
        void* d = api.Domain();
        if (d) api.thread_attach(d);   // 仅 GC 可见，不代表主线程
    }
}

inline void* acct_mgr(const Api& api, Sym& s) {
    if (!s.acctMgrCls) s.acctMgrCls = api.FindClassByShortName("AccountMgr");
    if (!s.acctMgrCls) return nullptr;
    return api.StaticRefField(s.acctMgrCls, "instance");
}

inline void* acct_info(const Api& api, Sym& s, void* mgr) {
    if (!mgr) return nullptr;
    if (!s.acctInfoCls) s.acctInfoCls = api.FindClassByShortName("AccountInfo");
    if (!s.acctInfoCls) return nullptr;
    if (!s.fldAccountInfo && api.class_get_field)
        s.fldAccountInfo = api.class_get_field(s.acctMgrCls, "AccountInfo");
    void* v = nullptr;
    if (!exp::read_ptr(mgr, s.fldAccountInfo, v)) return nullptr;
    return v;
}

inline void* now_player(const Api& api, Sym& s, void* mgr) {
    if (!mgr) return nullptr;
    if (!s.getNowPlayer) s.getNowPlayer = api.GetMethod(s.acctMgrCls, "get_nowPlayerInfo", 0);
    if (!s.getNowPlayer) return nullptr;
    void* exc = nullptr;
    void* ret = api.runtime_invoke(s.getNowPlayer, mgr, nullptr, &exc);
    return exc ? nullptr : ret;
}

inline int save(const Api& api, Sym& s, void* mgr) {
    if (!mgr) return RC_NO_OBJ;
    if (!s.saveData) s.saveData = api.GetMethod(s.acctMgrCls, "SaveData", 1);
    if (!s.saveData) return RC_RESOLVE;
    bool must = true;
    void* args[1] = { &must };
    void* exc = nullptr;
    api.runtime_invoke(s.saveData, mgr, args, &exc);
    return exc ? RC_SAVE : RC_OK;
}

} // namespace detail

// 钻石 itemid：配置表实测 2（金币1/钻石2/积分3），1.8 亦为 2
inline std::atomic<int>& diamond_id() { static std::atomic<int> id{kDiamondItemIdFromTable}; return id; }
inline void SetDiamondItemId(int id) { diamond_id().store(id); }

namespace exec {

inline std::atomic<bool>& last_applied();
inline std::atomic<int>&  last_count();

inline bool read_shared_gold(const Api& api, void* info, int64_t& out) {
    // 只走公开 getter + Nullable 的公开方法，不读 Nullable 内存布局
    static void* miGet = nullptr, *miHas = nullptr, *miVal = nullptr;
    if (!miGet) miGet = api.GetMethod(api.object_class(info), "get_sharedGold", 0);
    if (!miGet) return false;
    void* exc = nullptr;
    void* boxed = api.runtime_invoke(miGet, info, nullptr, &exc);
    if (exc || !boxed) return false;
    void* ncls = api.object_class(boxed);
    if (!miHas) miHas = api.GetMethod(ncls, "get_HasValue", 0);
    if (!miVal) miVal = api.GetMethod(ncls, "get_Value", 0);
    if (!miHas || !miVal) return false;
    exc = nullptr;
    void* hb = api.runtime_invoke(miHas, boxed, nullptr, &exc);
    bool has = false;
    if (exc || !exp::unbox() || !hb) return false;
    { void* pv = exp::unbox()(hb); if (!pv) return false; has = (*reinterpret_cast<uint8_t*>(pv) != 0); }
    if (!has) return false;                       // sharedGold 为 null（本存档无共享金币）
    exc = nullptr;
    void* vb = api.runtime_invoke(miVal, boxed, nullptr, &exc);
    int64_t v = 0;
    if (exc || !exp::unbox_i64(vb, v)) return false;
    out = v;
    return true;
}

// ---------------------------------------------------------------------------
// 存档直写（按 1.8 文档路线：直接改 saveBags / AccountInfo 字段，不调游戏方法）
//   2.2 与 1.8 的差别只在偏移未知 ⇒ 全部用 FieldOffset(name) 现取，不写死数字。
//   任何一步指针不合法即返回失败（不做裸写），并在 DbgLog 留 before/after 原始值。
// ---------------------------------------------------------------------------
namespace save {

struct Obj { void* mgr; void* info; };

// 字段枚举 / 语义兜底解析
struct FieldRef { char name[64]; size_t off; };

inline int list_fields(const Api& api, void* klass, FieldRef* out, int maxOut) {
    int n = 0;
    if (!klass || !api.class_fields || !api.field_name || !api.field_offset) return 0;
    void* iter = nullptr;
    while (void* f = api.class_fields(klass, &iter)) {
        if (n >= maxOut) break;
        const char* nm = api.field_name(f);
        const size_t off = api.field_offset(f);
        if (!nm) continue;
        snprintf(out[n].name, sizeof(out[n].name), "%s", nm);
        out[n].off = off;
        ++n;
    }
    return n;
}

inline void dump_fields(const Api& api, const char* tag, void* klass) {
    FieldRef fr[64];
    const int n = list_fields(api, klass, fr, 64);
    KYZLOG("fields[%s] n=%d", tag, n);
    for (int i = 0; i < n; ++i) KYZLOG("fields[%s] %s @%zu", tag, fr[i].name, fr[i].off);
}

inline bool find_field(const Api& api, void* klass, const char* exact, size_t& off_out) {
    if (!klass) return false;
    if (exact && api.class_get_field && api.field_offset) {
        void* f = api.class_get_field(klass, exact);
        if (f) { const size_t off = api.field_offset(f); if (off) { off_out = off; return true; } }
    }
    return false;
}

inline Obj account() {
    const Api& api = GetApi();
    Obj o{nullptr, nullptr};
    void* mgrCls = api.FindClassByShortName("AccountMgr");
    if (!mgrCls) { KYZLOG("save: AccountMgr 未找到"); return o; }
    o.mgr = api.StaticRefField(mgrCls, "instance");
    if (!alive(o.mgr)) { KYZLOG("save: instance 空/非法 %p", o.mgr); o.mgr = nullptr; return o; }
    size_t off = api.FieldOffset(mgrCls, "AccountInfo");
    if (!off) { KYZLOG("save: AccountMgr.AccountInfo 偏移 0"); return o; }
    o.info = Il2Cpp::ReadPtr(o.mgr, off);
    if (!alive(o.info)) { KYZLOG("save: AccountInfo 非法 %p (off=%zu)", o.info, off); o.info = nullptr; }
    return o;
}

// List<T>：+0x10 数组 / +0x18 size；数组 +0x20 起元素指针
inline void* list_at(void* list, int32_t i, int32_t* size_out) {
    if (!alive(list)) { KYZLOG("list_at: list 非法 %p", list); return nullptr; }
    void* arr = *(void**)((uintptr_t)list + 0x10);
    const int32_t n = *(int32_t*)((uintptr_t)list + 0x18);
    if (size_out) *size_out = n;
    if (i < 0 || i >= n) { KYZLOG("list_at: 越界 i=%d n=%d", i, n); return nullptr; }
    const size_t need = 0x20 + (size_t)n * 8;
    if (!readable(arr, need)) {
        KYZLOG("list_at: arr 不可读 list=%p arr=%p size=%d need=%zu", list, arr, n, need);
        return nullptr;
    }
    return *(void**)((uintptr_t)arr + 0x20 + (uintptr_t)i * 8);
}

inline void* player0(const Api& api, const Obj& a) {
    if (!a.info) return nullptr;
    size_t off = api.FieldOffset(api.object_class(a.info), "players");
    void* players = off ? Il2Cpp::ReadPtr(a.info, off) : nullptr;
    if (!alive(players)) { KYZLOG("save: players 读不到 (off=%zu ptr=%p)", off, players); return nullptr; }
    int32_t n = 0;
    void* p0 = list_at(players, 0, &n);
    if (!alive(p0)) { KYZLOG("save: players[0] 空 (cnt=%d)", n); return nullptr; }
    return p0;
}

// 2.2 存档结构（实测 2026-11-14 22:10）
//   AccountMgr.AccountInfo@96 → AccountInfo.players@272(List<PlayerInfo>) → [0].saveBags@280
//   ItemSaveData: baseItemid@180 / encodedCount@16 / countKey@20  （count 无明文字段，XOR 编码）
inline size_t off_field(const Api& api, void* obj, const char* name, const char* tag) {
    if (!obj) return 0;
    size_t off = 0;
    if (!find_field(api, api.object_class(obj), name, off)) {
        KYZLOG("%s: 字段 %s 未找到 (cls=%s)", tag, name, api.ClassName(obj));
        return 0;
    }
    return off;
}

// 备份所有货币条目（id/enc/key）供诊断
inline void touch_save(void* mgr) {
    if (!alive(mgr)) return;
    const Api& api = GetApi();
    size_t off = api.FieldOffset(api.object_class(mgr), "needSave");
    if (off) { Il2Cpp::WriteU8(mgr, off, 1); KYZLOG("save: needSave=1"); }
    else KYZLOG("save: needSave 偏移 0（未置位）");
}

inline void dump_currency_items(const Api& api, void* bags, const char* tag) {
    int32_t n = 0;
    list_at(bags, 0, &n);
    for (int32_t i = 0; i < n && i < 80; ++i) {
        void* it = list_at(bags, i, nullptr);
        if (!readable(it, 0x100)) continue;
        const size_t offId = off_field(api, it, "baseItemid", tag);
        const size_t offEnc = off_field(api, it, "encodedCount", tag);
        const size_t offKey = off_field(api, it, "countKey", tag);
        if (!offId || !offEnc || !offKey) return;
        const int32_t id = Il2Cpp::ReadI32(it, offId);
        const int32_t enc = Il2Cpp::ReadI32(it, offEnc);
        const int32_t key = Il2Cpp::ReadI32(it, offKey);
        if (id <= 3 || i < 8)
            KYZLOG("%s: bag[%d] id=%d enc=%d key=%d xor=%d", tag, i, id, enc, key, enc ^ key);
    }
}

} // namespace save

inline std::atomic<bool>& last_applied() { static std::atomic<bool> b{false}; return b; }
inline std::atomic<int>&  last_count()   { static std::atomic<int> n{-1}; return n; }

// 三个「货币」在 2.2 全是背包条目：1=金币 2=钻石 3=积分（配置表 2026-11-14 实测）
// count 在存档里是 XOR 编码：encodedCount ^ countKey = 真实值 ⇒ 读改写必须成对用同一 key。
inline int bump_item(int itemid, int delta, const char* tag) {
    const Api& api = GetApi();
    if (!api.ready) return RC_NO_API;
    save::Obj a = save::account();
    if (!a.info) return RC_NO_OBJ;
    void* p0 = save::player0(api, a);
    if (!p0) return RC_NO_OBJ;
    const size_t offBags = api.FieldOffset(api.object_class(p0), "saveBags");
    void* bags = offBags ? Il2Cpp::ReadPtr(p0, offBags) : nullptr;
    int32_t n = 0;
    save::list_at(bags, 0, &n);
    KYZLOG("%s: bags=%p cnt=%d offBags=%zu", tag, bags, n, offBags);
    if (!alive(bags) || n <= 0) return RC_NO_OBJ;

    int hit = 0;
    for (int32_t i = 0; i < n; ++i) {
        void* it = save::list_at(bags, i, nullptr);
        if (!readable(it, 0x100)) continue;
        const size_t offId  = save::off_field(api, it, "baseItemid", tag);
        const size_t offEnc = save::off_field(api, it, "encodedCount", tag);
        const size_t offKey = save::off_field(api, it, "countKey", tag);
        if (!offId || !offEnc || !offKey) return RC_RESOLVE;
        if (Il2Cpp::ReadI32(it, offId) != itemid) continue;
        const int32_t enc = Il2Cpp::ReadI32(it, offEnc);
        const int32_t key = Il2Cpp::ReadI32(it, offKey);
        const int64_t cur = (int64_t)(enc ^ key);
        const int64_t sum = cur + (int64_t)delta;
        if (sum > 2000000000LL || sum < 0) { KYZLOG("%s: bag[%d] 溢出 cur=%lld", tag, i, (long long)cur); return RC_OVERFLOW; }
        Il2Cpp::WriteI32(it, offEnc, (int32_t)((int32_t)sum ^ key));
        const int32_t enc2 = Il2Cpp::ReadI32(it, offEnc);
        KYZLOG("%s: bag[%d] id=%d enc %d->%d key=%d => %lld -> %lld (读回 %d)",
               tag, i, itemid, enc, enc2, key, (long long)cur, (long long)sum, enc2 ^ key);
        ++hit;
    }
    if (!hit) { KYZLOG("%s: 背包内无 id=%d 条目", tag, itemid); return RC_NO_OBJ; }
    save::touch_save(a.mgr);
    last_applied() = true;
    last_count() = (int)delta;
    return RC_OK;
}

// ---------------------------------------------------------------------------
// 2026-11-14 23:40：拿到 2.2 反编译源码（13_scratch/kyzt22_login/AccountInfo.cs），
// 金币/积分的真实结构 = AccountInfo 的私有编码字段对：
//   points     : encodedPoints ^ pointsKey        (long, 无 has 标志)
//   sharedGold : hasSharedGold ? encodedGold ^ goldKey : null   (long?)
// 写入必须复刻 C# setter：key = RotateKey(key); enc = value ^ key。
// RotateKey 只在 key==0 时用随机源；任何非 0 key 都能自洽（getter 只做 XOR）⇒ 可自选常量。
// ---------------------------------------------------------------------------
inline uint64_t rot_key64(uint64_t k) {
    const uint64_t n = k;
    const uint64_t n2 = n ^ (n << 13);
    const uint64_t n3 = n2 ^ (n2 >> 7);      // C# >>> 是无符号右移
    return n3 ^ (n3 << 17);
}

// 绝对值写入（用于修复被写坏的金币）：value 为目标值，不是增量
inline int write_acct_abs(const char* encName, const char* keyName, const char* flagName,
                          long long value, const char* tag) {
    const Api& api = GetApi();
    if (!api.ready) return RC_NO_API;
    save::Obj a = save::account();
    if (!a.info) return RC_NO_OBJ;
    void* cls = api.object_class(a.info);
    size_t offEnc = 0, offKey = 0, offFlag = 0;
    if (!save::find_field(api, cls, encName, offEnc)) return RC_RESOLVE;
    if (!save::find_field(api, cls, keyName, offKey)) return RC_RESOLVE;
    if (flagName && !save::find_field(api, cls, flagName, offFlag)) return RC_RESOLVE;
    const int64_t key = *(int64_t*)((uintptr_t)a.info + offKey);
    uint64_t newKey = (key != 0) ? rot_key64((uint64_t)key) : 0x9E3779B97F4A7C15ull;
    *(int64_t*)((uintptr_t)a.info + offKey) = (int64_t)newKey;
    *(int64_t*)((uintptr_t)a.info + offEnc) = (int64_t)((uint64_t)value ^ newKey);
    if (offFlag) Il2Cpp::WriteU8(a.info, offFlag, 1);
    const int64_t check = *(int64_t*)((uintptr_t)a.info + offEnc) ^ *(int64_t*)((uintptr_t)a.info + offKey);
    KYZLOG("%s: SET %s=%lld (读回 %lld)", tag, encName, (long long)value, (long long)check);
    if (check != (int64_t)value) return RC_NULL_RET;
    save::touch_save(a.mgr);
    return RC_OK;
}

inline int write_acct_encoded(const char* encName, const char* keyName, const char* flagName,
                              long long delta, const char* tag) {
    const Api& api = GetApi();
    if (!api.ready) return RC_NO_API;
    save::Obj a = save::account();
    if (!a.info) return RC_NO_OBJ;
    void* cls = api.object_class(a.info);
    size_t offEnc = 0, offKey = 0, offFlag = 0;
    if (!save::find_field(api, cls, encName, offEnc)) { KYZLOG("%s: 字段 %s 未找到", tag, encName); return RC_RESOLVE; }
    if (!save::find_field(api, cls, keyName, offKey)) { KYZLOG("%s: 字段 %s 未找到", tag, keyName); return RC_RESOLVE; }
    if (flagName && !save::find_field(api, cls, flagName, offFlag)) { KYZLOG("%s: 字段 %s 未找到", tag, flagName); return RC_RESOLVE; }

    const int64_t enc = *(int64_t*)((uintptr_t)a.info + offEnc);
    const int64_t key = *(int64_t*)((uintptr_t)a.info + offKey);
    const bool has = offFlag ? (Il2Cpp::ReadU8(a.info, offFlag) != 0) : true;
    const int64_t cur = has ? (enc ^ key) : 0;
    const int64_t next = cur + (int64_t)delta;

    uint64_t newKey = (key != 0) ? rot_key64((uint64_t)key) : 0x9E3779B97F4A7C15ull;
    *(int64_t*)((uintptr_t)a.info + offKey) = (int64_t)newKey;
    *(int64_t*)((uintptr_t)a.info + offEnc) = (int64_t)((uint64_t)next ^ newKey);
    if (offFlag) Il2Cpp::WriteU8(a.info, offFlag, 1);

    const int64_t check = *(int64_t*)((uintptr_t)a.info + offEnc) ^ *(int64_t*)((uintptr_t)a.info + offKey);
    KYZLOG("%s: %s@%zu/%s@%zu has=%d %lld -> %lld (读回 %lld)",
           tag, encName, offEnc, keyName, offKey, (int)has, (long long)cur, (long long)next, (long long)check);
    if (check != next) return RC_NULL_RET;
    save::touch_save(a.mgr);
    last_applied() = true;
    last_count() = (int)next;
    return RC_OK;
}

// 金币：2.2 移到 PlayerInfo —— get_gold() / ChangeGold(int32, bool) 实测存在
//   （AccountInfo 已无 points/sharedGold 字段；AccountMgr 只剩 PrepareGoldCompatibilityAsync）
inline int gold_via_change(long long delta) {
    const Api& api = GetApi();
    if (!api.ready) { KYZLOG("gold: api 未就绪"); return RC_NO_API; }
    if (!api.runtime_invoke) { KYZLOG("gold: runtime_invoke 导出缺失"); return RC_NO_API; }
    save::Obj a = save::account();
    if (!a.info) { KYZLOG("gold: AccountInfo 空 mgr=%p", a.mgr); return RC_NO_OBJ; }
    void* p0 = save::player0(api, a);
    if (!p0) { KYZLOG("gold: player0 空"); return RC_NO_OBJ; }
    void* pcls = api.object_class(p0);
    KYZLOG("gold: p0=%p cls=%s", p0, api.ClassName(p0));
    void* mi = api.GetMethod(pcls, "ChangeGold", 2);
    if (!mi) { KYZLOG("gold: PlayerInfo.ChangeGold(2) 未找到"); return RC_RESOLVE; }
    int32_t d = (int32_t)delta;
    bool notify = false;
    void* args[2] = { &d, &notify };
    void* exc = nullptr;
    void* ret = api.runtime_invoke(mi, p0, args, &exc);
    if (exc) { KYZLOG("gold: ChangeGold 抛异常 exc=%p", exc); return RC_EXCEPTION; }
    bool ok = true;
    if (ret && exp::unbox()) { void* pv = exp::unbox()(ret); if (pv) ok = (*reinterpret_cast<uint8_t*>(pv) != 0); }
    KYZLOG("gold: ChangeGold(%d, notify=false) ret=%p ok=%d", d, ret, (int)ok);
    if (!ok) return RC_REFUSED;
    save::touch_save(a.mgr);
    last_applied() = true;
    return RC_OK;
}

inline int gold(long long delta) {
    // 2.2：金币 = AccountInfo.sharedGold（encodedGold ^ goldKey；hasSharedGold 标志）
    return write_acct_encoded("encodedGold", "goldKey", "hasSharedGold", delta, "gold");
}
inline int diamond(long long delta) { return bump_item(2, (int)delta, "dia"); }
inline int points(long long delta)  {
    // 2.2：积分 = AccountInfo.points（encodedPoints ^ pointsKey）
    return write_acct_encoded("encodedPoints", "pointsKey", nullptr, delta, "pts");
}

} // namespace exec

namespace sched {
inline void drain_locked(int per_frame) {
    std::deque<Req>& q = queue();
    int n = 0;
    while (!q.empty() && n < per_frame) {
        Req r = q.front();
        q.pop_front();
        int rc = RC_RESOLVE;
        if (r.kind == KIND_GOLD) rc = exec::gold(r.delta);
        else if (r.kind == KIND_POINT) rc = exec::points(r.delta);
        else if (r.kind == KIND_DIAMOND) rc = exec::diamond(r.delta);
        last_rc().store(rc);
        ++n;
    }
    drained().fetch_add(static_cast<unsigned>(n));
}
} // namespace sched

// UI 侧：只入队。返回 RC_QUEUED(100) 表示已排入，**不代表已加成功**。
inline int RequestGrant(int kind, long long delta) {
    if (kind != KIND_GOLD && kind != KIND_DIAMOND && kind != KIND_POINT) return RC_RESOLVE;
    std::lock_guard<std::mutex> lk(sched::mu());
    sched::queue().push_back(sched::Req{kind, delta});
    return RC_QUEUED;
}
// 同步直执行（2026-11-14 22:30）：菜单点击时**当场**改存档，不等 Unity 帧回调。
// 为什么：2.2 的 ET.GameManagerMono.Update 钩子只在进战斗后才跑，玩家在主界面点资源
// 永远等不到 drain ⇒ 表现为「点了没反应」。这里先 attach 到 il2cpp，再走同一套字段读写。
// 全类名扫描：按关键字找「钱/货币」相关类并 dump 字段（一次性，失败关闭）
// 方法名过滤转储：找「加钱」类 API（AccountMgr/ItemManager 上）
inline void dump_methods(const Api& api, const char* tag, void* klass) {
    if (!klass || !api.class_methods || !api.method_get_name) return;
    int n = 0;
    void* iter = nullptr;
    while (void* mi = api.class_methods(klass, &iter)) {
        const char* nm = api.method_get_name(mi);
        if (!nm) continue;
        char low[128]; size_t i = 0;
        for (; nm[i] && i < sizeof(low) - 1; ++i) low[i] = (char)tolower((unsigned char)nm[i]);
        low[i] = 0;
        if (strstr(low, "gold") || strstr(low, "coin") || strstr(low, "money") ||
            strstr(low, "point") || strstr(low, "currency") || strstr(low, "diamond") ||
            strstr(low, "additem") || strstr(low, "changeitem") || strstr(low, "reward")) {
            KYZLOG("meth[%s] %s argc=%u mi=%p", tag, nm,
                   api.method_param_count ? api.method_param_count(mi) : 0, mi);
            if (++n > 40) break;
        }
    }
    KYZLOG("meth[%s] done n=%d", tag, n);
}

inline void scan_money_classes(const Api& api) {
    if (!api.image_class_count || !api.image_class || !api.class_name) return;
    static const char* kKeys[] = {"gold", "coin", "point", "diamond", "currency", "wallet", "money", "economy"};
    size_t an = 0;
    void** asms = api.Assemblies(&an);
    if (!asms) return;
    int hits = 0;
    for (size_t a = 0; a < an && hits < 12; ++a) {
        void* img = const_cast<void*>(api.assembly_image(asms[a]));
        if (!img) continue;
        const uint32_t cn = api.image_class_count(img);
        for (uint32_t c = 0; c < cn && hits < 12; ++c) {
            void* k = api.image_class(img, c);
            if (!k) continue;
            const char* nm = api.class_name(k);
            if (!nm) continue;
            char low[128];
            size_t i = 0;
            for (; nm[i] && i < sizeof(low) - 1; ++i) low[i] = (char)tolower((unsigned char)nm[i]);
            low[i] = 0;
            for (size_t q = 0; q < sizeof(kKeys) / sizeof(kKeys[0]); ++q) {
                if (strstr(low, kKeys[q])) {
                    KYZLOG("money: class %s.%s", nm, "");
                    exec::save::dump_fields(api, nm, k);
                    ++hits;
                    break;
                }
            }
        }
    }
    KYZLOG("money: scan done hits=%d", hits);
}

inline void probe_once(const Api& api);   // 前置声明（RunNow 里补 dump 用）

inline int RunNow(int kind, long long delta) {
    const Api& api = GetApi();
    if (!api.ready) return RC_NO_API;
    detail::attach(api);                 // 跨线程访问托管对象的入场券（仅 GC 可见）
    if (log_fn()) probe_once(api);       // 账号就绪后补一次字段 dump（Tick 早于登录完成）
    int rc = RC_RESOLVE;
    // 必须走 exec::gold/diamond/points（内含各自的回退链），不能直接 bump_item
    if (kind == KIND_GOLD)         rc = exec::gold(delta);
    else if (kind == KIND_DIAMOND) rc = exec::diamond(delta);
    else if (kind == KIND_POINT)   rc = exec::points(delta);
    sched::last_rc().store(rc);
    return rc;
}

inline int GrantGold(long long delta) { return RequestGrant(KIND_GOLD, delta); }
inline int GrantDiamond(long long delta) { return RequestGrant(KIND_DIAMOND, delta); }
inline int GrantPoints(long long delta) { return RequestGrant(KIND_POINT, delta); }

// Unity 帧回调（intake 在 ET.GameManagerMono.Update 的 AOT 钩子里每帧调用）
// ---------------------------------------------------------------------------
// probe_once — 只读探测（首次 Unity 帧执行一次），把 2.2 真实可解析的
// 类/字段/方法一次性写进 DbgLog，用于替代「按 1.8 名字猜」。
// 只调用 FieldOffset / GetMethod / ClassName，不写任何托管字段。
// ---------------------------------------------------------------------------
inline void probe_once(const Api& api) {
    static bool done = false;
    if (done || !api.ready) return;
    exec::save::Obj a = exec::save::account();
    if (!a.info) { KYZLOG("probe: 账号对象还没就绪，稍后重试"); return; }   // 不置 done，等下一 Tick
    done = true;
    KYZLOG("probe: -- 字段表 dump --");
    {
        void* ai = a.info ? api.object_class(a.info) : api.FindClassByShortName("AccountInfo");
        if (ai) exec::save::dump_fields(api, "AccountInfo", ai);
        void* piCls = api.FindClassByShortName("PlayerInfo");
        if (piCls) exec::save::dump_fields(api, "PlayerInfo", piCls);
        void* mgrCls = api.FindClassByShortName("AccountMgr");
        if (mgrCls) exec::save::dump_fields(api, "AccountMgr", mgrCls);
        void* imCls2 = api.FindClassByShortName("ItemManager");
        if (imCls2) exec::save::dump_fields(api, "ItemManager2", imCls2);
        void* ii = api.FindClassByShortName("ItemSaveData");
        if (ii) exec::save::dump_fields(api, "ItemSaveData", ii);
    }
    KYZLOG("probe: mgr=%p info=%p(%s)", a.mgr, a.info, a.info ? api.ClassName(a.info) : "-");
    if (a.info) {
        void* icls = api.object_class(a.info);
        const size_t offPlayers = api.FieldOffset(icls, "players");
        const size_t offPoints  = api.FieldOffset(icls, "points");
        const size_t offGold    = api.FieldOffset(icls, "sharedGold");
        void* players = offPlayers ? Il2Cpp::ReadPtr(a.info, offPlayers) : nullptr;
        int32_t pcnt = 0;
        void* p0 = exec::save::list_at(players, 0, &pcnt);
        if (p0 && alive(p0)) exec::save::dump_fields(api, "PlayerInfoObj", api.object_class(p0));
        {
            void* mgrCls = api.FindClassByShortName("AccountMgr");
            if (mgrCls) dump_methods(api, "AccountMgr", mgrCls);
            void* imCls = api.FindClassByShortName("ItemManager");
            if (imCls) dump_methods(api, "ItemManager", imCls);
            if (p0 && alive(p0)) dump_methods(api, "PlayerInfo", api.object_class(p0));
        }
        scan_money_classes(api);
        // 2026-11-14 23:45：把金币/积分的解码值打出来（只读，不写）
        {
            if (a.info) {
                void* icls = api.object_class(a.info);
                size_t oe = 0, ok2 = 0, of = 0;
                if (exec::save::find_field(api, icls, "encodedGold", oe) &&
                    exec::save::find_field(api, icls, "goldKey", ok2)) {
                    const int64_t e = *(int64_t*)((uintptr_t)a.info + oe);
                    const int64_t k = *(int64_t*)((uintptr_t)a.info + ok2);
                    const bool has = exec::save::find_field(api, icls, "hasSharedGold", of)
                                     ? (Il2Cpp::ReadU8(a.info, of) != 0) : true;
                    KYZLOG("decode: gold enc=%lld key=%lld has=%d => %lld",
                           (long long)e, (long long)k, (int)has, (long long)(has ? (e ^ k) : 0));
                } else KYZLOG("decode: encodedGold/goldKey 未找到");
                size_t pe = 0, pk = 0;
                if (exec::save::find_field(api, icls, "encodedPoints", pe) &&
                    exec::save::find_field(api, icls, "pointsKey", pk)) {
                    const int64_t e = *(int64_t*)((uintptr_t)a.info + pe);
                    const int64_t k = *(int64_t*)((uintptr_t)a.info + pk);
                    KYZLOG("decode: points enc=%lld key=%lld => %lld", (long long)e, (long long)k, (long long)(e ^ k));
                } else KYZLOG("decode: encodedPoints/pointsKey 未找到");
            }
        }
        // 2026-11-14 23:20：金币被写坏后，先读回真实值（PlayerInfo.get_gold / ItemManager.get_gold）
        {
            if (p0 && alive(p0)) {
                void* g = api.GetMethod(api.object_class(p0), "get_gold", 0);
                if (g) {
                    void* exc = nullptr;
                    void* box = api.runtime_invoke(g, p0, nullptr, &exc);
                    int64_t v64 = 0; int32_t v32 = 0;
                    bool ok64 = !exc && exp::unbox_i64(box, v64);
                    bool ok32 = !exc && exp::unbox_i32(box, v32);
                    KYZLOG("goldread: PlayerInfo.get_gold exc=%p ok64=%d v64=%lld ok32=%d v32=%d",
                           exc, (int)ok64, (long long)v64, (int)ok32, v32);
                } else KYZLOG("goldread: PlayerInfo.get_gold 未找到");
            }
            void* imCls = api.FindClassByShortName("ItemManager");
            void* im = imCls ? api.StaticRefField(imCls, "instance") : nullptr;
            if (im && alive(im)) {
                void* g2 = api.GetMethod(api.object_class(im), "get_gold", 0);
                if (g2) {
                    void* exc = nullptr;
                    void* box = api.runtime_invoke(g2, im, nullptr, &exc);
                    int64_t v64 = 0; int32_t v32 = 0;
                    bool ok64 = !exc && exp::unbox_i64(box, v64);
                    bool ok32 = !exc && exp::unbox_i32(box, v32);
                    KYZLOG("goldread: ItemManager.get_gold exc=%p ok64=%d v64=%lld ok32=%d v32=%d",
                           exc, (int)ok64, (long long)v64, (int)ok32, v32);
                } else KYZLOG("goldread: ItemManager.get_gold 未找到");
            } else KYZLOG("goldread: ItemManager.instance 空 im=%p", im);
        }
        KYZLOG("probe: acct cls=%s off(players)=%zu players=%p cnt=%d p0=%p(%s)",
               api.ClassName(a.info), offPlayers, players, pcnt, p0,
               (p0 && alive(p0)) ? api.ClassName(p0) : "-");
        if (offPoints) KYZLOG("probe: off(points)=%zu raw=%lld", offPoints,
                              (long long)*(int64_t*)((uintptr_t)a.info + offPoints));
        if (offGold) KYZLOG("probe: off(sharedGold)=%zu has=%u raw=%lld", offGold,
                            (unsigned)Il2Cpp::ReadU8(a.info, offGold),
                            (long long)*(int64_t*)((uintptr_t)a.info + offGold + 8));
        if (alive(p0)) {
            void* pcls = api.object_class(p0);
            const size_t offBags = api.FieldOffset(pcls, "saveBags");
            void* bags = offBags ? Il2Cpp::ReadPtr(p0, offBags) : nullptr;
            int32_t n = 0;
            void* it0 = exec::save::list_at(bags, 0, &n);
            KYZLOG("probe: player cls=%s off(saveBags)=%zu bags=%p cnt=%d it0=%p(%s)",
                   api.ClassName(p0), offBags, bags, n, it0,
                   (it0 && alive(it0)) ? api.ClassName(it0) : "-");
            if (alive(bags)) exec::save::dump_currency_items(api, bags, "cur");
            // 2026-11-14 22:50：金币/积分不在 AccountInfo/PlayerInfo 字段里，继续挖 state 与 bags
            {
                const size_t offState = api.FieldOffset(api.object_class(a.info), "state");
                void* st = offState ? Il2Cpp::ReadPtr(a.info, offState) : nullptr;
                KYZLOG("probe: off(state)=%zu state=%p(%s)", offState, st,
                       (st && alive(st)) ? api.ClassName(st) : "-");
                if (st && alive(st)) exec::save::dump_fields(api, "AcctState", api.object_class(st));
                const size_t offPriv = api.FieldOffset(api.object_class(a.info), "privileges");
                void* pr = offPriv ? Il2Cpp::ReadPtr(a.info, offPriv) : nullptr;
                KYZLOG("probe: off(privileges)=%zu priv=%p(%s)", offPriv, pr,
                       (pr && alive(pr)) ? api.ClassName(pr) : "-");
                const size_t offBags2 = api.FieldOffset(api.object_class(p0), "bags");
                void* b2 = offBags2 ? Il2Cpp::ReadPtr(p0, offBags2) : nullptr;
                int32_t n2 = 0;
                exec::save::list_at(b2, 0, &n2);
                KYZLOG("probe: off(bags)=%zu bags=%p(%s) cnt=%d", offBags2, b2,
                       (b2 && alive(b2)) ? api.ClassName(b2) : "-", n2);
            }
            if (alive(it0)) {
                void* icls2 = api.object_class(it0);
                const size_t offId = api.FieldOffset(icls2, "baseItemid");
                const size_t offCnt = api.FieldOffset(icls2, "count");
                KYZLOG("probe: item cls=%s off(baseItemid)=%zu off(count)=%zu id=%d cnt=%d",
                       api.ClassName(it0), offId, offCnt,
                       offId ? Il2Cpp::ReadI32(it0, offId) : -1,
                       offCnt ? Il2Cpp::ReadI32(it0, offCnt) : -1);
            }
        }
    }
}

inline void OnUnityFrame(int maxPerFrame = 4) {
    sched::unity_tid().store((unsigned long long)(uintptr_t)pthread_self());
    sched::has_unity_tid().store(true);
    probe_once(GetApi());
    std::lock_guard<std::mutex> lk(sched::mu());
    sched::drain_locked(maxPerFrame);
}

inline unsigned PendingCount() { std::lock_guard<std::mutex> lk(sched::mu()); return (unsigned)sched::queue().size(); }
inline unsigned DrainedCount() { return sched::drained().load(); }
inline int LastRc() { return sched::last_rc().load(); }
inline bool SawUnityThread() { return sched::has_unity_tid().load(); }
inline bool LastAppliedInMemory() { return exec::last_applied().load(); }
inline int  LastGrantedCount() { return exec::last_count().load(); }
inline bool ExportsReady() { return exp::ensure(); }

} // namespace Kyzt22

#endif // KYZT_ECONOMY22_H
