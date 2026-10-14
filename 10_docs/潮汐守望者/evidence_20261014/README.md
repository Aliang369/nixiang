# 潮汐守望者 — 决定性证据（2026-10-14 · 官包升版 1.2.65.385.1 星光包）

> 只列最短判断链与复现命令；**完整原始日志不入库**（`09_logs/`、`06_dumped/`、`*.log` 已被 .gitignore 排除，本地保留）。
> 设备：小米 `96f0d883`（M2102K1AC，arm64-v8a，Android 14，**无 root**）。日志：`09_logs/潮汐守望者/starry_xiaomi_run10_1.2.65.385.1.log`。

## 1. 两功能生效（用户 m01468「功能都有效果啦」+ 设备日志独立闭环）

```
hpd inline fix: 运行时偏移整体 +0x10(值类型装箱头) slotGap=0x78 == cn 长度 0x78 ⇒ 采用真实内联布局 fValue=0x0 src=0x8 settle=0xC show=0x10 skill=0x28 formula=0x2C atk=1C/20/24
D#0 hd=0x70e3998720 raw(@0x0)=0xfffffffffff62000 V0=0xfffffffffff62000 Vinl=0xfffffffffff62000 sz=1
P#1 e=0x73e54ba960 uid=6 mon=1 twr=-1 cs=-1 sz=1 raw=-647168 v=-158.000 src=0 set=0 show=1 atk=4/1 u2001 sk=200117 fm=103
DMG x10 e=0x73e54ba960 uid=6 raw=-647168->-6471680
DMG x10 e=0x73e54ba5a0 uid=7 raw=-1056768->-10567680
INV e=0x73e544f1e0 uid=4 raw=-708608->0
```

统计：**INV 5 / DMG 4 / P# 29**；崩溃关键字（`signal 11|signal 6|FATAL EXCEPTION|E CRASH`）**0**；布局解析 17 ms（无长帧）。

```bash
L=09_logs/潮汐守望者/starry_xiaomi_run10_1.2.65.385.1.log
grep -cE "signal 11|signal 6|FATAL EXCEPTION|E CRASH" "$L"   # 0
grep -E "hpd inline fix" "$L"
echo "INV: $(grep -c 'INV e=' "$L")  DMG: $(grep -c 'DMG x' "$L")"
```

## 2. 根因：HybridCLR 热更程序集给值类型报的字段偏移整体 +0x10

`stHpChangeData` 是 `HpChangeEntitySystem` 的**内联值类型**（`hData @0x60`，槽长 `mAtkEntity(0xD8) − hData(0x60) = 0x78`），但运行时 `class_fields` 报的偏移整体比旧 cn 表大 0x10（值类型按**装箱对象头**计）：

| 字段 | cn 锚点 | 运行时 | Δ |
|---|---|---|---|
| fValue | 0x00 | 0x10 | +0x10 |
| eDamageSourceType | 0x08 | 0x18 | +0x10 |
| eDamageSettleType | 0x0C | 0x1C | +0x10 |
| eTextShowType | 0x10 | 0x20 | +0x10 |
| iAttackSkillID | 0x28 | 0x38 | +0x10 |
| iFormularID | 0x2C | 0x3C | +0x10 |

真 `fValue` 在记录 **+0x00**：`0xfffffffffff62000` = -647168（Q20.12）= **-158.0**，负=伤害。此前读 `+0x10`（恒正）⇒ `raw<0` 分支从未触发 ⇒「不闪退但没效果」。

**类布局原始 dump**（设备拉回，208 行，`09_test/潮汐守望者/cxswz_tables_run10.txt`，md5 `750af2e4ccdff5aeccc40bdb1b3aa9ec`）关键锚点：

```
kSys/HpChangeEntitySystem  +0x050 com_hp HPChangeComponent  +0x060 hData stHpChangeData  +0x0D8 mAtkEntity Entity  +0x0E0 fDamage Fix64
kComp/HPChangeComponent    +0x020 lValues List<stHpChangeData>
kHpd/stHpChangeData        +0x010 fValue Fix64  +0x018 eDamageSourceType ... （rt 口径，含 +0x10 偏置）
kEnt/Entity                +0x010 <m_uID>k__BackingField UInt32
```

```bash
adb -s 96f0d883 pull /sdcard/Android/data/com.gf.cxswz/files/cxswz_tables.txt 09_test/潮汐守望者/cxswz_tables_run10.txt
```

**版本漂移实证**（相对 1.2.56.375.3）：`SYS_MATK 0xD0→0xD8`、`SYS_FDAMAGE 0xD8→0xE0` 各 +8 ⇒ runtime 按名解析是必需品，静态 RVA 已全部降级为 `*_DOC_ONLY`。

## 3. 打怪闪退（run8）＝诊断探针越界，非改写逻辑

```
10-14 22:21:59.234 I/Zygote: Process 5596 exited due to signal 11 (Segmentation fault)
10-14 22:21:59.235 I/ActivityManager: Process com.gf.cxswz (pid 5596) has died: fg TOP
```

SIGSEGV ⇒ ART 不留 `data_app_crash`（`dumpsys dropbox --print` 最新仍是 14:09、`logcat -b crash` 空），无原生栈；当次 hook **0 条 D#/P#** ⇒ 崩在探针里。根因：e8 新增的 `compMask(40..70)` 探针把 `Entity.HasComponent(entity,v)` 连调 31 次，而 `Entity._componentPools` 是**按枚举下标索引的数组**（`+0x040 _componentPools Stack<IComponent>[]`），越界枚举值 = 越界读。run7 无此探针不崩、run8 崩。修复：删除该扫描 + 所有解引用前置 `Il2Cpp::IsReadableRange` 闸门。

## 4. NOT PROVEN

- `-37` 服务端封禁判定（沿用 2026-09-30 结论，本轮未做换账号/换机实验）。
- 服务端战斗结算重放校验：本轮未做「结算差异」实验。
- HybridCLR 热更再次漂移时，+0x10 判据（六字段同平移 + 槽长 0x78==记录长）是否仍成立 —— 代码内有双条件自校验兜底，不成立则警告并安全退回不改落点。
