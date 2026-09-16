# 经验：HybridCLR 热更程序集里 DllImport 不可用（热更 C# 取状态走文件镜像）

> 首次发现：宝石战争 6.0.012（2026-09-16，完整证据链见
> `10_docs/宝石战争/星光落地_6.0.012_20260916.md` §5.3–5.5 与
> `10_docs/宝石战争/evidence_20260916_freeze*/`）
> 适用：所有 Unity IL2CPP + HybridCLR 热更游戏，凡是要在**热更 C#** 里读菜单开关/功能状态、
> 或反过来调 native 的场景。

---

## 现象（三步走偏，值得抄作业）

1. 注入 IL 调 `[DllImport("starry_magic", EntryPoint="AdjustDamage")]` —— 入口名写成短名、
   so 实际导出 `StarryGw_AdjustDamage` → 对局**回合结束时卡死**（画面停住，无崩溃、无 ANR、无 SIGSEGV）。
2. 入口名改成 `StarryGw_*` 后**仍然卡** —— 说明「名字对不上」不是唯一原因；
   此时 `debuggerd -b` 看到 UnityMain 停在 libunity 等待态 = 协程被打断，不是死循环。
3. 给注入调用加 `try/catch` 兜底后：**不卡了，但三项功能全部无效**；
   native 侧「前 N 次调用日志」计数 = **0**，Unity 侧异常 = 0。

## 根因

**IL2CPP 的 P/Invoke 表是构建期生成的**；热更程序集（HybridCLR 解释执行）里新加的
`[DllImport]` 在 AOT 侧没有登记，解释器解析不到符号 → 调用必抛异常。

- 异常被游戏自己的协程吞掉 → 表现是「卡死」，不是报错；
- 被我们自己的 `catch` 吞掉 → 表现是「功能无效」，也不是报错。

**结论：热更 C# 不能反向调 native。** 把 P/Invoke 声明留着当文档可以，运行期不要调。

## 正确通道：文件镜像（Java 写 / 热更 C# 读）

1. 菜单 Java 侧维护功能值快照（宝石战争：`com.starry.magic.menu.StateMirror`），
   在 toggle / slider / spinner / forceOff / reapply **五处**同步，
   原子写 `{filesDir}/starry_state.txt`（单行空格分隔）。
2. 热更 C# 侧读文件并打包成位域整数，便于解析：

   ```csharp
   static int StarryState() {
       var p = File.ReadAllText("/data/data/<pkg>/files/starry_state.txt").Split(' ');
       return int.Parse(p[1]) + int.Parse(p[2]) * 1000 + int.Parse(p[3]) * 2000;
   }
   // 倍率 = state % 1000；上帝模式 = (state / 1000) % 2；无限技能 = state / 2000
   ```

3. **只用游戏热更代码自身也在用的 BCL API**（`File.ReadAllText` / `String.Split` / `int.Parse`），
   降低被 IL2CPP 裁剪导致「方法不存在」的风险。

## 连带铁律

- **桥接调用一律 try/catch 兜底**：异常打断战斗协程 = 玩家看到「卡死」，
  比功能无效难排查得多。
- **桥接无效果时先看 native 侧调用计数**：计数 0 = 根本没打通（别再查开关逻辑/数值映射）；
  有计数但功能不对 = 才是语义问题。
- 同类通道可选：`AndroidJavaClass`（游戏自身热更代码用过，可用作备选）、
  Java 侧写文件（本卡主推，零 interop）。

## 反面教材

把「入口名写错」当成唯一根因，改完发现还卡，又去怀疑 HybridCLR 热更差异、棋盘补位逻辑——
浪费了两轮装机。正确顺序应是：
**先加兜底（保证不卡）→ 再加 native 侧命中计数（判定是否打通）→ 再谈语义**。
