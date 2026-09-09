# 经验：HybridCLR/IL2CPP 直写三铁律（挂点计数 / invoke 禁用 / stride 自检）

> 首次发现：战魂铭人 3.5.0（2026-08-29，完整证据链见 `10_docs/战魂铭人/战魂铭人_3.5.0_复刻记录_20260829.md`）
> 适用：所有 Unity IL2CPP + HybridCLR 热更游戏（音舞光年/魂坠深境/掌门下山/弹力果冻/战魂铭人…）

---

## 铁律一：MethodPointer hook 必须用「命中计数」证实，装上 ≠ 命中

- Dobby 装在 `MethodInfo.methodPointer` 上，序言校验通过（真函数 STP 序言），
  但游戏全程 **命中=0**——HybridCLR 游戏内调用走解释器分发，不经过该指针。
- **做法**：每个 hook 加原子命中计数，Tick 节流打印；`hits=0` = 挂点无效，
  立即换路（公共跳板/直写），禁止「装上就算成功」。
- 佐证：魂坠深境「部分 MethodPointer 钩不到」的旧教训，实测可能全军覆没。

## 铁律二：外部线程对热更方法用 runtime_invoke = 解释器 SIGSEGV

- tombstone 实锤：`#0-9 libil2cpp 解释器 / #10-12 我方线程`。
  il2cpp_thread_attach 之后依旧会崩；典型形态是「第一个生效、批量的后面崩」。
- **做法**：热更方法一律 **0 invoke**。
  - 有条目 → 原地改字段/字典 value 字节；
  - 缺条目 → 仅 ref 类型 TValue 的字典（`Dictionary<string,EncryptValue>` 等，
    corlib 共享 AOT 实现）可用「methodPointer 直调 set_Item + il2cpp_object_new」创建；
  - **值类型 TValue（bool/enum/int）的闭包泛型 set_Item 直调 = 解释器桩 = 无效执行**
    （返回 true 但没写入），只能走原地翻转。

## 铁律三：字典 Entry stride 随 TValue 类型变化，禁止假设 0x18

| TValue | stride | value 偏移 |
|---|---|---|
| 引用类型（class/string） | 0x18 | 0x10 |
| bool（4+4+4+1 对齐） | **0x10** | **0xC** |
| 大结构体（如 HeroSkinConfig 是 struct） | 实测 **0x78** | 不可 assumable |

- **做法**：步长自检——遍历候选步长，校验「全 key 落在合理值域且互不相同」，
  首个通过者即为真；或已知 key0/key1 定位差值。
- 战魂铭人 126/158 皮肤「名字缺失」即 0x18 假设踩中 0x78 结构体字典。
- 注意 `Dictionary<HeroSkillStyleName,bool>` 这类：游戏创建英雄时**已把全部
  key 以 false 预填**，解锁 = 原地翻转 value 字节，无需插入。

## 附加两条（同场实锤）

4. **单例实例禁止跨局缓存**：换角色/新开一局会换 LevelProcess/Hero 实例，
   klass 相同的旧指针照样通过校验 → 写旧对象=无效+腐蚀。每秒经
   StaticRefField 重新解析（attach 后多会话实证稳定）；未 attach 时高频
   field_static_get_value 会随机 SIGSEGV。跨对象读一律 `process_vm_readv`
   安全读（区间检查挡不住映射外指针）。
5. **配置表驱动的解锁必须运行时枚举配置**（`DataManager.HeroSkinConfigData` /
   `Archive.HeroBodySkillNameLibrary` 等）：基础 CDN dll 与线上热更差 52 个皮肤，
   硬编码枚举名表永远追不上；枚举名→值映射用 `il2cpp_class_get_fields` +
   `il2cpp_field_get_name` + `il2cpp_field_get_value_object`（本地 dlsym）。

## 偏移结论（修正旧文档）

`il2cpp_field_get_offset` **原值即真偏移**。旧文档「HybridCLR +8」来自旧工程
堆扫描的基址偏差（运行值 484/360 vs 原值 476/352，恒差 8）；
`unlock-pattern.md` 的「字典直写 0x18 步长」伪码同样要按本卡修正。
