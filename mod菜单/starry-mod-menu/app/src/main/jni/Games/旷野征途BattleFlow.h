// 帧入口控制流（游戏私有）：cpp 与宿主测试**共用同一模板**，避免"测试另写一遍算法"。
// 结构约束（2026-11-14 lead 口径）：
//   A 双开关关 → 直接 orig；B 再入 → 直接 orig；
//   C 其余在 Guard 作用域内做 Init/查询/改写；**scope 内不出现 orig/return**；
//   D scope 结束后唯一 orig。
// 注：C++ 里 `return orig(x)` 会先求值 orig 再析构局部对象，所以 scope 内 return orig
//     会让 orig 在 t_inHook 仍为 true 时执行 —— 因此 scope 内一律不调 orig。
#ifndef KYZT_BATTLE_FLOW_H
#define KYZT_BATTLE_FLOW_H
namespace kb {

struct FlowGuard {
    bool& flag;
    bool prev;
    explicit FlowGuard(bool& f) : flag(f), prev(f) { flag = true; }
    ~FlowGuard() { flag = prev; }
};

template <typename FnReady, typename FnInit, typename FnWork, typename FnOrig>
inline void* RunFrameFlow(bool godOn, bool x100On, bool& tInHook,
                          FnReady ready, FnInit init, FnWork work, FnOrig orig) {
    if (!godOn && !x100On) return orig();      // A
    if (tInHook) return orig();                // B（内层不改写，避免二次 ×100）
    {
        FlowGuard g(tInHook);                  // C 作用域：Init + 查询 + 改写
        if (ready() || init()) {
            work();
        }
    }                                          // 析构：tInHook 还原
    return orig();                             // D 唯一出口（tInHook 已复位）
}

}  // namespace kb
#endif
