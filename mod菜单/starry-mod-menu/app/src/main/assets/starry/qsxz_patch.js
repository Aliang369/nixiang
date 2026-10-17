/* ============================================================================
 * qsxz_patch.js — 星光版《炁术行者》页面内 JS 补丁（v2）
 *
 * 载体：assets/starry/qsxz_patch.js（外科注入塞进游戏 APK）
 * 注入：com.starry.magic.h5box.StarryH5 —— 每 1s（ack 前）/ 5s（ack 后）用
 *       evaluateJavascript 灌一次；页面重载后自动重装。
 *       （**不要**改回 loadUrl 拼 javascript: URL：那条路径会把脚本当 URL 解析，
 *        百分号会被解码、百分号编码的井号会变成片段起点，于是脚本被当场截断、
 *        WebView 静默丢弃。本文件因此**禁止出现百分号与井号**，注释里也不行。）
 *
 * ★ 关键设计（2026-10-07 真机踩坑后定）：**投值动作必须在每次注入时执行**
 *   —— 星光菜单是独立 Activity 浮层，盖住游戏时宿主 WebView 变不可见，
 *      浏览器随即把页面定时器节流/冻结。只靠页面内 setInterval 轮询会出现
 *      「拖完滑块游戏没反应」：值一直在变，但页面里的心跳根本没跑。
 *      Java 侧守护跑在 Handler 上，不受页面节流影响，所以每次注入都重读
 *      一次状态并推值，才是可靠通路；页面内 setInterval 只作为前台兜底。
 *
 * 其他铁律（沿用城主别慌张已验版）
 *   1) 一次性装点（挂钩/定时器）用 window.__qsxz_v1 守卫；**投值不做守卫**。
 *   2) 自延迟：主 JS(__QSW_APPLY) 未就绪就等下一轮注入。
 *   3) 授权门控：state.live=0 时回落原版口径（k=1, dr=0）。
 *   4) 不抛异常：每步独立 try/catch。
 *
 * 数值口径（2026-10-18 用户定制放宽，勿再擅改）
 *   · 伤害倍率 1~100：默认 1 = 原版（上限 100 = 用户定制）
 *   · 减伤 1~100：默认 1 = 最低档；0 仅作「未授权原版」哨兵（上限 100 = 用户定制）
 *   · 战斗加速 1~10：1 = 完全不动（原版）；上限 10 = 用户定制（>3 服务端 9998 拒判风险已知）
 *   · 战力一律不动（放大战力 9987 拒判）
 *
 * 日志：window.StarryH5.log(...) → logcat tag "StarryJs_qsxz"
 * ==========================================================================*/
(function () {
    'use strict';

    /* ---- 防御：注入可能早于页面初始化（此时还没有 JS 桥），别占守卫 ---- */
    if (!window.StarryH5) { return; }        // 桥未就绪，等下一次注入
    if (!window.__QSW_APPLY) { return; }     // 主 JS 未就绪，等下一次注入

    /* ---- 功能位号：必须与 Games/QsxzEntry.cpp 的 featureList 下标一致 ---- */
    var F_DMG = 1;   // SeekBar_伤害倍率_1_100_1
    var F_DR = 2;    // SeekBar_减伤_1_100_1
    var F_SPD = 3;   // SeekBar_战斗加速_1_10_1（1 = 原版，不干预；上限 10）

    var K_MAX = 100, DR_MAX = 100, SPD_MAX = 10, POLL_MS = 500;   // 2026-10-18：K_MAX 5→100、DR_MAX 50→100、SPD_MAX 3→10（用户定制）

    /* ---- 跨注入共享状态挂在 window 上：每次注入都是新闭包，局部变量靠不住 ---- */
    var W = window;
    if (!W.__qsxz_st) {
        W.__qsxz_st = { k: -1, d: -1, s: -1, beats: 0, raw: null };
    }
    var st = W.__qsxz_st;
    var state = { live: 0, specs: [], bools: [], vals: [] };

    function log(s) {
        try { window.StarryH5.log(s); } catch (e) { /* 日志失败绝不影响游戏 */ }
    }

    function refresh() {
        try {
            var raw = window.StarryH5.getState();
            if (!raw) { return; }
            /* 状态原文变化就记一条：排障时能直接看到 live/specs/bools/vals 的真实内容 */
            if (raw !== st.raw) {
                st.raw = raw;
                log('state ' + String(raw).substring(0, 240));
            }
            var o = JSON.parse(raw);
            if (o && typeof o.live !== 'undefined') { state = o; }
        } catch (e) { log('getState 失败: ' + e); }
    }

    /* 注：bools/vals 按 featureList 数组下标（specs[i] 对 vals[i]；Category 占 0） */
    function pick() {
        var k = 1, d = 0, s = 1;
        if (state.live) {
            k = state.vals ? (state.vals[F_DMG] | 0) : 1;
            d = state.vals ? (state.vals[F_DR] | 0) : 0;
            s = state.vals ? (state.vals[F_SPD] | 0) : 1;
        }
        if (k < 1) { k = 1; } else if (k > K_MAX) { k = K_MAX; }
        if (d < 0) { d = 0; } else if (d > DR_MAX) { d = DR_MAX; }
        if (s < 1) { s = 1; } else if (s > SPD_MAX) { s = SPD_MAX; }
        return { k: k, d: d, s: s };
    }

    /* 投值：每次注入都跑（见文件头 ★ 说明）。未授权 → live=0 → 回落原版。 */
    function applyNow() {
        try {
            refresh();
            var p = pick();
            /* 无条件重写：即使页面某处改过 __QSWK，下一轮注入也会纠回来 */
            window.__QSW_APPLY(p.k, p.d, p.s);
            if (p.k !== st.k || p.d !== st.d || p.s !== st.s) {
                st.k = p.k;
                st.d = p.d;
                st.s = p.s;
                log('apply k=' + p.k + ' dr=' + p.d + ' spd=' + p.s + ' live=' + (state.live ? 1 : 0));
            } else if ((++st.beats & 15) === 0) {
                log('beat k=' + p.k + ' dr=' + p.d + ' spd=' + p.s + ' live=' + (state.live ? 1 : 0) + ' n=' + st.beats);
            }
        } catch (e) { log('applyNow 失败: ' + e); }
    }

    /* ---- 本次注入：先投值（不做守卫），再补一次性装点 ---- */
    applyNow();

    if (!W.__qsxz_v1) {
        W.__qsxz_v1 = 1;
        /* 前台兜底：页面可见时 500ms 一跟，拖滑块更跟手 */
        try { setInterval(applyNow, POLL_MS); } catch (e) { /* 不影响游戏 */ }
        try { window.StarryH5.ack('qsxz v2'); } catch (e) { /* 不影响游戏 */ }
    }
})();
