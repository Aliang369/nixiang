#pragma once

// ============================================================
//  YsxzDecoyJs.h — 诱饵层（P2-3）
//
//  谁会用上它：把门禁改成恒真、或没有有效票/密钥的**改包版本**。
//  它必须满足四条：
//    ① 是**合法 JS**：能 eval、不报错、不闪退（不闪退才不会告诉对方"你被识别了"）；
//    ② **静默无效**：语法通过、挂一个同名标记，但功能一个都不干活；
//    ③ 看起来像真的：带 `starry_ysxz_v1` 标记（也用来防重复注入，行为与真载荷一致）；
//    ④ **必须补上 splice 契约要求的 `window.__starry_capture`**：
//       原生 splice 把主 bundle 的注册表调用改写成
//         }(window.__starry_capture({ … }), {}, [ …
//       真载荷在末尾定义它（`patch_v2_ac.js` → `return reg;`）。诱饵若缺这一条，
//       主 bundle 一 eval 就 `TypeError: window.__starry_capture is not a function`
//       ⇒ `ScriptEngine::evalString … failed` ⇒ `Failed to invoke require`
//       ⇒ 引擎永远停在「正在为您加载游戏资源」。
//       2026-10-27 卸载重装事故就是这个：诱饵"静默无效"变成了"直接打死游戏"，
//       既害了正版用户（清数据后没票），也等于告诉改包的人"你被识别了"。
//       透传实现必须**原样返回 reg**，绝不能返回 undefined（那会让整个 bundle 的
//       模块注册表变成空，等于换一种方式打死游戏）。
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
/* splice 契约（必须存在、必须原样返回注册表；见文件头 ④） */
window.__starry_capture = function (reg) {
    return reg;
};
)YSXZDECOY";
