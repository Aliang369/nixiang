# 独奏骑士 · Solo.GameCode.dll 热更解密

> 日期：2026-08-20  
> 状态：**已完成**（非 THEMIS 加密；UnityPy 读坏曾误判）

---

## 结论

| 项 | 内容 |
|----|------|
| 来源 | YooAsset bundle `e7eec7b977e5db53503fe7de978220c3` → TextAsset `Solo.GameCode.dll` |
| 产物 | `06_dumped/独奏骑士/hotdlls/Solo.GameCode.dll`（7,695,872 B，合法 PE/COR20） |
| 反编译 | `03_decompiled/独奏骑士/Game_Event.cs` / `Game_Role.cs` / `Game_RoleSkill.cs` |
| 提取脚本 | `04_scripts/独奏骑士/extract_dlls.py`（已修：`get_raw_data` + 手动解析字节数组） |

**此前误判**：文档写「THEMIS/YooAsset 层加密」。实际 bundle 内 dll **已是明文 PE**；用 UnityPy `m_Script`（str + `U+DCxx` 代理项）再 `encode('utf-8','surrogatepass')` 会插入 `ED Bx xx`，导致 `e_lfanew=0`、ilspy 报 Invalid COR20。

正确做法：`obj.get_raw_data()` → 读对齐 string + byte[]，直接写盘。

---

## 目标方法归属与签名

| 功能（斌哥） | 方法 | 类型 | 签名（C#） |
|--------------|------|------|------------|
| 百倍伤害 | `calTotalDamage` | `SGame.Game_Event` | `static float calTotalDamage(Game_Role caster, Game_Role target, Game_RoleSkill skill, float exDamage_x, float exDamageReduce, out eDamageType damageType)` |
| 百倍伤害 / 无敌 | `calDamageBuffer` | `SGame.Game_Event` | `static float calDamageBuffer(float totalDamage, Game_Role_Property[] targetFData, Game_Role target)` |
| 无敌加血 | `AddRtHPTo` | `SGame.Game_Role` | `float AddRtHPTo(float resHP, bool noDead = true)` |
| 无限技能 CD | `isCdOK` | `SGame.Game_RoleSkill` | `bool isCdOK()` → 看 `rBaseData.getProp(zCd)` |
| 无限技能蓝 | `isManaOk` | `SGame.Game_RoleSkill` | `bool isManaOk(float dTime = -1f)` |

敌我相关字段（`Game_Role`）：`_isFriendly` / `rIsFriendly`（斌哥壳 `+0xa8` 是否即此字段 ⚠️ 待运行时 Trace 对齐）。

---

## 与斌哥 hook 语义的差异（落地时注意）

斌哥挂在 HybridCLR 公共入口，按**方法名**过滤后改参数槽；C# 签名与壳里看到的 `w4`/`x2+0x18` 不是一一对应的静态约定。

- `calDamageBuffer` 只有 3 个形参，壳里 `w4∈[2,8]` 更可能是 **target 对象上的阵营字段**，不是第 4 个 C# 参数。
- `isCdOK` 逻辑是读 `zCd` prop；壳写 `skill+0x2ac←0` 是运行时对象布局，需 Trace 证实后再写星光。
- 落地仍建议：**HybridCLR 跳板 + 方法名过滤**，参数/偏移以 StarryTrace 为准，本 dll 用于理解语义与选挂点。

---

## 同包其它产物

- `Solo.BeforePatch.dll` — 含 `LoadDllBytes` / `CreateDecryptor` / `IDecryptionServices` / `HybridCLR_Loader`（加载链；本包 GameCode 本身未再套一层）
- 大量 AOT/依赖 dll 同 bundle 可一并提取（`extract_dlls.py`）
EOF
