# IL2CPP ABI 坑清单

> 📍 路径：`re-implement/il2cpp-abi-pitfalls.md`
> 🎯 触发场景：写 Dobby hook 前，对照本清单核对函数签名与 delegate 调用方式
> 来源：我本布衣世家录 2026-09-28 会话三轮返工总结；失落城堡 2026-10-01（坑 6）

---

## 坑 1：delegate 对象指针当函数指针调用 → SIGSEGV

**症状**：Hook 免广告/回调类函数后，开启功能立刻 SIGSEGV（SEGV_ACCERR）。

**根因**：C# `Action<int>` / `Action<bool>` 等 delegate 传入 native hook 时，
`void* onSuccess` 是一个 IL2CPP 托管对象指针，不是可直接调用的函数地址。
该对象的布局是：

```
偏移 0x00: Il2CppClass* klass
偏移 0x08: void* monitor
偏移 0x10: intptr_t method_ptr      ← 不是普通函数地址，是 IL2CPP 编译的 thunk
偏移 0x18: intptr_t invoke_impl
偏移 0x20: Il2CppObject* m_target
...
```

直接 `reinterpret_cast<void(*)(int32_t)>(onSuccess)(1)` 会跳到 method_ptr 附近
的错误地址 → 段错误。

**正确做法**：用 `il2cpp_runtime_invoke` 调用 delegate 对象的 `Invoke` 方法。

```cpp
// 参考 DkzjMod.cpp InvokeActionBool（已验证安全模式）
bool InvokeActionBool(void* action) {
    if (!OkPtr(action) || !AttachCurrentThreadToIl2Cpp()) return false;
    auto& api = GetApi();
    if (!api.runtime_invoke || !api.object_class) return false;

    void* klass = api.object_class(action);
    if (!klass) return false;
    void* methodInfo = api.GetMethod(klass, "Invoke", 1);
    if (!methodInfo) return false;

    uint8_t success = 1;
    void* args[1] = {&success};
    void* exception = nullptr;
    api.runtime_invoke(methodInfo, action, args, &exception);
    if (exception) return false;
    return true;
}
```

**关键点**：
- `api.GetMethod(klass, "Invoke", 1)` 中 argc=1 是参数个数（bool 是 1 个参数）
- `runtime_invoke(methodInfo, action, args, &exception)` 的 `action` 是 this
- 如果 delegate 是 `Action<int>`，args[0] 是 `&int32_t`，值不能直接传
- 失败时回退调原函数（走真广告），不要吞异常继续假装成功

---

## 坑 2：Vector3Int 按 AArch64 ABI 是拆开的 3 个 int，不是指针

**症状**：Hook 接受 `Vector3Int` 参数的方法后，调原函数转发崩溃或参数错乱。

**根因**：C# `Vector3Int`（或任何 12-byte struct）在 AArch64 ABI 下走 HPR
（Homogeneous Password Register）拆参：X1=int_x, X2=int_y, X3=int_z。
如果 hook 签名写成 `void* pos`，转发时会把一个寄存器值当指针解引用。

**正确做法**：

```cpp
// dump.cs: public static bool CanAffordBuyLand(long familyId, Vector3Int pos)
// C++ hook 签名：
typedef bool (*CanAffordBuyLand_t)(int64_t familyId, int32_t posX, int32_t posY, int32_t posZ, void* method);

static bool Hooked_CanAffordBuyLand(int64_t familyId, int32_t posX, int32_t posY, int32_t posZ, void* method) {
    return g_origCanAffordBuyLand(familyId, posX, posY, posZ, method);
}
```

**判断规则**：
- dump.cs 中参数是 C# struct 且 ≤ 16 bytes 且全部同类型 int/float → AArch64 ABI 拆寄存器
- 16 bytes 以上或混合类型 → 可能走栈或传引用，需要看反汇编确认
- 不确定时看 Il2CppDumper `script.json` 的 `TypeSignature` 字段

---

## 坑 3：instance 方法漏 this 参数

**症状**：调用原函数后数据错乱、崩溃、或第一次调用正常后续异常。

**根因**：dump.cs 中 `public void InitAd()` 没有显式参数，但它是 instance 方法，
native 桩实际签名是 `void InitAd(void* self, void* method)`（有隐含 this + MethodInfo）。
如果 hook typedef 写成 `void InitAd(void* method)`，转发时把 MethodInfo 当 this 用。

**正确做法**：所有非 static 的 dump.cs 方法，hook typedef 首参数都是 `void* self`。

```cpp
// dump.cs: public void InitAd()  ← instance 方法（无 static 关键字）
typedef void (*InitAd_t)(void* self, void* method);
static void Hooked_InitAd(void* self, void* method) {
    if (g_origInitAd) g_origInitAd(self, method);
}
```

**判断规则**：
- dump.cs 有 `public static` → 无 self，首参就是第一个真实参数
- dump.cs 无 `static` → 有 self，首参是 `void* self`，之后才是真实参数
- 末尾总有 `void* method`（MethodInfo*）

---

## 坑 4：void 返回值被声明成 int32_t → 放大逻辑失效或栈破坏

**症状**：hook 返回值无效果（如返回放大后的 int 但游戏数值不变），或栈错乱。

**根因**：dump.cs 写 `public static void AddItem(int templateId, int count)`，
返回类型是 void。如果 hook typedef 写成 `int32_t (*)(...)`，AArch64 上
hook 替换函数会在 w0 里返回一个垃圾值，原函数也不关心 w0，所以大多数时候
不崩溃但放大逻辑完全无效——你以为返回了放大值，游戏根本没读。

**正确做法**：严格按 dump.cs 的返回类型写 typedef。

```cpp
// dump.cs: public static void ModifyResearchPoints(int delta)  ← void
typedef void (*ModifyResearch_t)(int32_t delta, void* method);
static void Hooked_ModifyResearch(int32_t delta, void* method) {
    if (g_research.load() && delta > 0) delta *= 10;
    if (g_origModifyResearch) g_origModifyResearch(delta, method);
}
```

**特殊：** 如果需要"让返回值变大"，必须改写真正返回 int 的方法，或者改成
"拦截消费路径 + 改写 GetCount 返回值"的模式（见 economy-pattern.md）。

---

## 坑 5（附加）：GetMaxAP 类似 getter 的 RVA 可能差 4 字节

**症状**：RVA 直挂时序言校验 fail，但地址附近看起来合理。

**根因**：Il2CppDumper 的 dump.cs 有时相邻方法的 RVA 差 4 字节，容易看错行。
我本布衣 GetMaxAP 正确 RVA 是 `0x12F82F8`（第一眼容易看成 `0x12F82F4`）。

**对策**：从 script.json 按完整类名+方法名提取 Address，不要手抄 dump.cs 的 RVA。
序言也从 ELF 文件偏移提取（用 `extract_il2cpp_preambles.py`），不要手编。

---

## 坑 6：值类型（struct）参数用 `runtime_invoke` 传「装箱对象」→ 静默写坏内存（2026-10-01 失落城堡事故）

**症状**：hook 装上了、调用没抛异常、日志显示「写入成功」，但游戏内毫无效果；更糟的是**随后会触发游戏自己的反作弊**（本次弹「档案数据存在异常，即将退出游戏!如有错误检测，请联系客服!」）。

**根因**：`il2cpp_runtime_invoke(methodInfo, obj, void** params, &exc)` 里，**值类型参数必须传「未装箱的结构体数据指针」**，引用类型参数才传「对象指针」。两者在同一参数位上无法靠类型自动区分。

本次场景：`Tera_NF.ContentUnlockCtrl.set_PrivilegeLevel(ACTk 的 ObscuredInt)`、`set_isCanPlayXxx(ObscuredBool)` —— 参数是 struct。我用 `api.BoxInt32()` / 手写装箱把值做成 `Il2CppObject*` 再塞进 `params[]`，il2cpp 把**对象头 16 B（`Il2CppClass* klass` + `void* monitor`）当成结构体内容**写进了后备字段。不报错、不崩溃，只是字段值变垃圾，随后随存档一起被校验抓到。

**判断规则**：

- 参数类型是 C# `struct` / 枚举的**装箱形态**（`ObscuredInt`、`Vector3`、`DateTime`…）→ 传 `&value`（结构体在栈上的地址）
- 参数类型是 class / interface / string / 数组 / delegate → 传对象指针
- 如果只拿到一个「装箱对象」但又必须按值传：`il2cpp_object_unbox(obj)` 取回内联数据指针（对应 `api` 里没有现成封装时需自行 `class_get_field` 求偏移，**不要**假设对象头后面紧跟数据）
- 拿不准时**放弃这个 hook**：换一个参数全是引用类型/int 的挂点，或改走「读侧」路线

**读回值类型字段**：只读方向有可靠办法 —— `ObscuredInt.GetDecrypted() -> int`（argc=0，唯一名）。**写方向不要碰**：

> 🔴 **ACTk（CodeStage Anti-Cheat Toolkit）的 `Obscured*` 字段只读不写。** 它的 `hiddenValue`/`currentCryptoKey`/`fakeValue` 是一套自校验结构，写错不是「功能无效」而是「反作弊自爆」。详见 `anti-cheat-pattern.md` 模式六。

**验证手段**：写值类型字段的功能，必须靠**游戏内可见状态**验收（打开对应 UI 看数字/图标），不能只靠 logcat 的「已写入」日志 —— 本次正是「日志说成功、界面无变化」拖了几轮。

---

## 装机前快速自检

写完 hook 后过一遍：

- [ ] 每个非 static 方法的 typedef 首参是 `void* self`
- [ ] 每个带 C# struct 参数的方法确认了是拆寄存器还是传引用
- [ ] **每个值类型（struct / 枚举装箱形态，如 `ObscuredInt`）参数按值传 `&value`，绝不传装箱对象指针**
- [ ] **没有对 ACTk `Obscured*` 字段做写操作**（只读方向走 `GetDecrypted()`）
- [ ] 写值类型字段的功能用**游戏内可见状态**验收，不只靠 logcat 的「已写入」日志
- [ ] 每个返回 void 的方法 typedef 是 void 而不是 int
- [ ] 每个接收 delegate 参数的方法用 runtime_invoke 而不是直接跳函数指针
- [ ] 每个序言值来自 `extract_il2cpp_preambles.py` 而不是手抄
- [ ] delegate invoke 失败时有 fallback 走原函数（不吞异常）
- [ ] 只消费不生产的功能（如"无限货币"）同时 hook 了显示 + 判定 + 扣费三层
