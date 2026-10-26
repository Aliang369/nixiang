#pragma once

// ============================================================
//  YsxzDecoyJs.h — 诱饵层（P2-3）
//
//  谁会用上它：把门禁改成恒真、或没有有效票/密钥的**改包版本**。
//  它必须满足三条：
//    ① 是**合法 JS**：能 eval、不报错、不闪退（不闪退才不会告诉对方"你被识别了"）；
//    ② **静默无效**：语法通过、挂一个同名标记，但功能一个都不干活；
//    ③ 看起来像真的：带 `starry_ysxz_v1` 标记（也用来防重复注入，行为与真载荷一致）。
//
//  注意：这里**故意**不做任何联网/上报动作；上报由上层（指纹上报）单独做，
//  免得诱饵层一跑就暴露。
// ============================================================

static const char* kYsxzDecoyJs = R"YSXZDECOY(
/*starry_ysxz_v1*/
/* ============================================================================
   星空增强层 v1（资源层）
   ========================================================================== */
(function () {
    try {
        if (typeof window === 'undefined') return;
        if (window.__starry_layer_ready) return;
        window.__starry_layer_ready = 1;
        var noop = function () { return undefined; };
        var 星层 = {
            version: 'v1',
            ready: true,
            enable: noop, disable: noop, toggle: noop, apply: noop,
            onFrame: noop, onTick: noop, update: noop
        };
        window.__starry_layer = 星层;
        window.starryLayer = 星层;
    } catch (e) {
        /* 静默：诱饵绝不抛到游戏里 */
    }
})();
)YSXZDECOY";
