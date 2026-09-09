# 经验卡片：对照改包只借玩法 · 运行时 Hook

> 状态：可汇总素材（待并入总「逆向经验」）  
> 首例：音舞光年 `0.300.41` / `com.uniugame.ywgn`（2026-08-10）  
> 详记：`10_docs/音舞光年/音舞光年_0.300.41_星光菜单_模拟真人_20260810.md`  
> 抓点：`10_docs/音舞光年/音舞光年_菜单功能抓点_20260809.md` §5.9 / play2 BSS

---

## 1. 原则（边界）

| 借 | 不借 |
|----|------|
| 游戏内**改分 / 改状态**的 hook 点、过滤条件、写内存动作 | 对方壳、菜单 UI、授权、整包 so、handler 二进制 |
| memdiff / BSS / stub 反推出来的**玩法链** | 「整壳移植」当产品路径 |

落地载体永远是自己的框架（星光 `Games/*Mod` + Dobby），**只复刻玩法语义**。

---

## 2. 方法为何优于静态

1. **对照改包找「钩哪、改什么」**（B-stub / trampoline / handler 明文），比盲搜 dump.cs 快。  
2. **运行时 Hook**（Dobby + 原函数尾调）比静态改指令：  
   - 不绑死某次 patch 字节；  
   - 可按方法名过滤，少误伤共用入口；  
   - 开关可做（feat ON 才改参）。  
3. HybridCLR / 热更下，堆扫字段、`runtime_invoke` 托管方法往往**对局无效果**；钩**实际出分路径**更稳。

首例对比：

- ❌ 写 `BubbleConfig.isAutoPlay` / 堆扫 `AutoPlay` → log 有、局内无感  
- ✅ 钩 invoke 点，过滤 `SendScore`，参数改 Perfect(5) → 局内生效

---

## 3. 标准作业（可复用检查单）

1. **ON/OFF/play memdiff** → 找 il2cpp（或引擎主 so）上的 B-stub。  
2. **file_off ↔ RVA**：注意 ELF RX bias（本例 `+0x4000`：`0x2660BDC` → `0x2664BDC`）。  
3. **dump handler / BSS 明文** → 确认：方法名字符串、写偏移、枚举值（如 Perfect=5）。  
4. **星光落地**：`DobbyHook(RVA)` → `method_get_name`（+可选 class）过滤 → 改参 → **必须**调原函数。  
5. **禁止**：无过滤钩 HybridCLR 共用解释器入口（易断连服）。  
6. **过检另线**：签名/PM 用自建（如 SignSpoof），勿为过检乱钩 Quit / 清 CriticalRisk。  
7. **验证 log**：`hooked @ rva=…` → feat ON → 局内命中计数（如 `SendScore→Perfect #N`）。

---

## 4. 首例锚点（音舞光年）

| 项 | 值 |
|----|-----|
| 钩点 | `libil2cpp` RVA `0x2664BDC`（内部 invoke，非 ScriptMethod） |
| 过滤 | `SendScore` + class `*KeyJudgment*` / `AKeyJudgment` / `BubbleNodeController` |
| 改写 | 参数块 `+0x8` → `ScoreType.Perfect = 5` |
| 菜单 | 仅星光 `Toggle_模拟真人`；与对照壳菜单无关 |

---

## 5. 以后汇总时怎么用

并入总经验时建议抽三条口号：

1. **对照改包 = 情报，不是产品。**  
2. **玩法优先钩「出结果」的那一跳，不钩「看起来像开关」的托管字段。**  
3. **有过滤的 invoke/方法钩 > 静态 patch；共用入口无过滤 = 禁区。**

缺什么补什么：新游戏若引擎不同，只替换 §3 第 1–3 步的取证对象（Cocos evalString / xLua / 自研），§1 边界与 §2 优劣不变。
