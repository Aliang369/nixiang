/**
 * 勇士之心 · 单文件补丁（补丁.js）
 * ------------------------------------------------------------------
 * 结构照 jsdex新.js 来，方便你后面接云更新 + dex 加载 + 卡密验证：
 *   1) 热更新适配（游戏换包/换资源后自动复位，保证补丁还挂着）
 *   2) 卡密网络验证（微验：MD5 签名 + RC4 + 心跳 + 公告）—— 这段保留你现有逻辑
 *   3) 悬浮窗菜单（纯 Cocos 绘制：球 → 面板 → 功能行；外观照弹壳战机面板）
 *   4) 游戏 Hook（伤害 / 锁血 / 锁蓝 / 金卡与自动战斗 / 人物移速）
 *   5) 统一 tick：dex 侧每 300ms 调 window.__WX_TICK__() 驱动（引擎里 setInterval 不保证被调度）
 *
 * 说明：
 *   - 全部在内存里完成，不改游戏任何文件 → 不会触发资源完整性校验（这条实测过：改文件必被游戏自己退出）。
 *   - Java 层的过检测（EmutorUtils/NativeInfo/CocosBridgeAPI 伪造 + System.exit 拦截）放在 dex 里，
 *     JS 只能做 JS 层的事，Java 层 hook 必须由 dex 完成。
 *   - 卡密/加密：本文件把卡密流程与 RC4 保留；加密壳（XXTEA/zlib 的 *ze* 容器）在 dex 里解密后再 evalString 注入。
 *
 * v66（华为鸿蒙 HarmonyOS / Android 16 适配）改了什么：
 *   [1] 新增 §0.5 平台适配层：环境探测（build.prop，不依赖 Java 反射）、安全区钳制、
 *       四路 tick 驱动、Canvas 挂载点探测、落盘装机指纹、诊断接口 __WX_DIAG__()。
 *   [2] 悬浮窗自愈：登录后球/面板丢了会自己重建，不再"一次没建起来就永远没有悬浮窗"。
 *   [3] 卡密窗看门狗限速重试，不会 500ms 一次猛刷。
 *   [4] 坐标全部过安全区：Android 15/16 强制 edge-to-edge、鸿蒙挖孔屏，球不会被顶到状态栏/挖孔下面。
 *   [5] 设置落盘双写：localStorage 不可用时退回 wx_cfg.json，鸿蒙上设置不会丢。
 *   [6] 保持手动登录：不做自动登录，卡密仍然必须手动输入 + 点"立即登录"。
 */
;/* 勇士之心补丁 v66 · harmony/android16 */
(function () {
    if (typeof window === 'undefined') return 'no-window';
    var WX_BOOT_VERSION = 'v102-res-1.1.3.2-repeat-temple';
    if (window.__WX_PATCH__ && window.__WX_PATCH_VERSION__ === WX_BOOT_VERSION) {
        // v98-fix：原来这里无条件 return 'already'，把加载器的重试彻底挡死。
        // 上次注入时引擎没就绪 / 定时器被吞，卡密窗没挂上时，这次注入补建一次，
        // 并把结果用独立字符串回给加载器（加载器把 'already' 当成功、其它当“还没好”时可继续重试）。
        try {
            if (!window.__WX_AUTH_OK__ && typeof window.__WX_AUTH_REVIVE__ === 'function') {
                return window.__WX_AUTH_REVIVE__() ? 'wx-patch-auth-revive-ok' : 'wx-patch-auth-revive-retry';
            }
        } catch (eRevive) {}
        return 'already';
    }
    if (window.__WX_PATCH__) {
        // 允许同一游戏进程覆盖旧补丁。上一版没有版本标记时也按旧版处理，
        // 否则加载器会命中 already，磁盘上的新 JS 永远没有机会执行。
        try {
            if (typeof window.__WX_PATCH_DISPOSE__ === 'function') window.__WX_PATCH_DISPOSE__();
        } catch (eDispose) {}
        try {
            if (typeof cc !== 'undefined' && cc && cc.director && cc.director.getScene) {
                var oldScene = cc.director.getScene();
                (function purgeOldUi(n) {
                    if (!n) return;
                    var children = [];
                    try { children = (n.children || []).slice(); } catch (eChildren) {}
                    for (var iOld = 0; iOld < children.length; iOld++) purgeOldUi(children[iOld]);
                    var nm = '';
                    try { nm = String(n.name || ''); } catch (eName) {}
                    if (nm === 'wx_auth_root' || nm === 'wx_float_root' || nm === 'wx_panel_root') {
                        try { n.removeFromParent(false); n.destroy(); } catch (eRemove) {}
                    }
                })(oldScene);
            }
        } catch (ePurge) {}
        window.__WX_PATCH__ = false;
        window.__WX_UI_READY__ = false;
    }
    // 只有完整走到启动末尾才标记成功；首次注入中途异常时允许加载器再次注入。

    /* v98-fix2：引擎比补丁晚就绪时的“零定时器”唤醒。
     * 典型场景就是重装/热更后的头几次启动：游戏在做首包解压，dex 很早把补丁 evalString 进来，
     * 此时 cc 还不存在、setInterval/setTimeout 又被解压卡住 → 5 条唤醒源全灭，卡密窗永远不建。
     * 这里在 window 上给 cc 装一个访问器：引擎一给 cc 赋值，立刻挂驱动并建卡密窗，全程不需要定时器。
     * getter 原样返回引擎赋的值，所以对引擎和游戏代码完全透明。 */
    (function () {
        function onEngineReady() {
            try { if (typeof watchSceneAuth === 'function') watchSceneAuth(); } catch (e0) {}
            try { if (typeof startDrivers === 'function') startDrivers(); } catch (e1) {}
            try { if (typeof authBump === 'function') authBump(); } catch (e2) {}
        }
        window.__WX_CC_WAKE__ = onEngineReady;
        try {
            if (typeof cc === 'undefined' && typeof Object.defineProperty === 'function') {
                var _cc;
                Object.defineProperty(window, 'cc', {
                    configurable: true,
                    get: function () { return _cc; },
                    set: function (v) { _cc = v; try { onEngineReady(); } catch (e3) {} }
                });
            }
        } catch (e4) {}
    })();

    /* ================= 0. 配置区（你后面改这里） ================= */
    var WX = {
        title: '勇士之星',
        ballIcon: 'TX',                  // 悬浮球里的字标（弹壳战机那颗球就是"TX"，想换就改这一处）
        skipAuth: false,                 // 调试用：true = 跳过卡密直接进菜单
        // ---- 微验（wy.llua.cn）配置：和 jsdex新.js 里的完全一致 ----
        noticeAPI: 'https://wy.llua.cn/api/?id=notice',
        loginAPI: 'https://wy.llua.cn/api/?id=kmlogon',
        appId: '44846',
        appCode: '1.0',
        okCode: 74945,
        appKey: '85PvP3tH5tZR4P5D',
        rc4_2: '007938632b3354adf3c37db48bb0aab2',
        pkg: 'com.fightingstars.games.warrior'
    };
    // 默认设置（也会写到本地，重启保留）
    var DEF = { on: false, value: 99999, lockHp: false, lockMp: false, autoFight: false, speed: 1.0 };

    /*<<<WX_COMPAT_BEGIN>>>*/
    /* ================================================================
     * 0.5 平台适配层（v66 新增）：华为鸿蒙 HarmonyOS + Android 16(API 36)
     * ----------------------------------------------------------------
     * 为什么这么写（这几条是本次改动的全部理由，别删）：
     *   a) 悬浮窗画在游戏自己的 Cocos 场景里，**不用 SYSTEM_ALERT_WINDOW**。
     *      鸿蒙 NEXT(5/6) 系统层面就没给第三方"显示在其他应用上层"的入口，
     *      原生 WindowManager 悬浮窗在那边永远开不出来；场景内悬浮球是鸿蒙上唯一稳的形态。
     *   b) 环境探测优先读 /system/build.prop（普通应用可读），不依赖 Java 反射：
     *      鸿蒙兼容层里 jsb.reflection 可能被裁剪，反射一失败整个适配就瞎了。
     *   c) 悬浮窗能不能出来，取决于 tick 有没有被调度。dex 没注入 / 被拦 / 引擎吞 setInterval
     *      都会导致"球不出现"。这里四路驱动并联（Cocos 调度器 / 每帧回调 / rAF / setTimeout 链）。
     *   d) Android 15/16 强制 edge-to-edge、鸿蒙机器普遍挖孔，悬浮球会被顶到状态栏/挖孔下面点不到，
     *      所以所有坐标统一过一遍安全区钳制。
     * ================================================================ */
    var WXV = WX_BOOT_VERSION;
    // 低负载兼容：重活只做低频巡检；诊断落盘默认关闭，需要排错时可在注入前设 window.__WX_DIAG_IO__ = true。
    var PERF = { tickMs: 450, healMs: 1500 };
    var ENV = {
        platform: '', sdk: 0, release: '', brand: '', model: '', manufacturer: '',
        harmony: false, harmonyVer: '', emui: '', emu: false,
        insets: { l: 0, r: 0, t: 0, b: 0 }, insetsSrc: 'none',
        visible: [0, 0], frame: [0, 0], dpr: 1, safe: null,
        ref: false, refFail: '', fileIO: false, editBox: false,
        buildProp: '', notes: []
    };
    function nstr(v) { return (v === null || typeof v === 'undefined') ? '' : String(v); }

    /* ---- Java 反射：一层安全壳，失败只记账不抛 ---- */
    function hasRef() {
        try { return !!(typeof jsb !== 'undefined' && jsb && jsb.reflection && typeof jsb.reflection.callStaticMethod === 'function'); }
        catch (e) { return false; }
    }
    function jcall(cls, m, sig) {
        if (!ENV.ref) return null;
        try {
            var a = [cls, m, sig], i;
            for (i = 3; i < arguments.length; i++) a.push(arguments[i]);
            return jsb.reflection.callStaticMethod.apply(jsb.reflection, a);
        } catch (e) {
            if (!ENV.refFail) ENV.refFail = cls + '.' + m + ': ' + (e && e.message);
            return null;
        }
    }

    /* ---- 文件 IO：build.prop 探测 + 配置落盘都靠它 ---- */
    function readFile(p) {
        try { if (typeof jsb === 'undefined' || !jsb.fileUtils) return ''; return nstr(jsb.fileUtils.getStringFromFile(p)); }
        catch (e) { return ''; }
    }
    function writeFile(p, s) {
        try { return !!jsb.fileUtils.writeStringToFile(String(s), p); } catch (e) { return false; }
    }
    function writablePath() {
        try { return nstr(jsb.fileUtils.getWritablePath()); } catch (e) { return ''; }
    }
    function pkgName() {
        try { if (window.__WX_PKG__) return String(window.__WX_PKG__); } catch (e) {}
        var p = '';
        try { p = nstr(jcall('org/cocos2dx/lib/Cocos2dxHelper', 'getPackageName', '()Ljava/lang/String;')); } catch (e) {}
        return p || WX.pkg;
    }

    /* ---- /system/build.prop：不依赖 Java 的机型/版本来源 ---- */
    var __PROPS = null;
    function props() {
        if (__PROPS) return __PROPS;
        __PROPS = {};
        var cand = ['/system/build.prop', '/vendor/build.prop', '/product/build.prop',
                    '/system_ext/build.prop', '/odm/etc/build.prop'];
        for (var i = 0; i < cand.length; i++) {
            var t = readFile(cand[i]);
            if (!t || t.indexOf('=') < 0) continue;
            __PROPS.__files = (__PROPS.__files ? (__PROPS.__files + ',') : '') + cand[i];
            var lines = t.split(/\r?\n/);
            for (var j = 0; j < lines.length; j++) {
                var ln = lines[j];
                if (!ln || ln.charAt(0) === '#') continue;
                var k = ln.indexOf('=');
                if (k <= 0) continue;
                var key = ln.substring(0, k).replace(/^\s+|\s+$/g, '');
                if (__PROPS[key] === undefined) __PROPS[key] = ln.substring(k + 1);   // /system 先读到的最权威
            }
        }
        ENV.buildProp = __PROPS.__files || '';
        return __PROPS;
    }
    function prop(k) { var p = props(); return p[k] === undefined ? '' : String(p[k]); }

    /* ---- 版本号 → API level（build.prop 读不到时用 UA 兜底） ---- */
    function relToSdk(rel) {
        var t = { '9': 28, '10': 29, '11': 30, '12': 31, '12L': 32, '12.1': 32, '13': 33, '14': 34, '15': 35, '16': 36, '17': 37 };
        return t[String(rel)] || 0;
    }
    function uaProbe() {
        var ua = '';
        try {
            var ctx = jcall('org/cocos2dx/lib/Cocos2dxHelper', 'getActivity', '()Landroid/app/Activity;');
            if (ctx) ua = nstr(jcall('android/webkit/WebSettings', 'getDefaultUserAgent', '(Landroid/content/Context;)Ljava/lang/String;', ctx));
        } catch (e) {}
        if (!ua) { try { ua = nstr(navigator && navigator.userAgent); } catch (e) {} }
        ENV.ua = ua.slice(0, 160);
        var m = ua.match(/Android\s+([0-9]+(?:\.[0-9]+)?)/i);
        if (m) { if (!ENV.release) ENV.release = m[1]; if (!ENV.sdk) ENV.sdk = relToSdk(m[1]); }
        var m2 = ua.match(/;\s*([A-Za-z0-9_\- ]+?)\s+Build\//);
        if (m2 && !ENV.model) ENV.model = m2[1].replace(/^\s+|\s+$/g, '');
        var m3 = ua.match(/Build\/([A-Za-z0-9_\-]+)/);
        if (m3) ENV.uaBuild = m3[1];
        return ua;
    }
    function applyInsets(v, src) {
        try {
            var l, t, r, b;
            if (v && typeof v.length === 'number' && v.length >= 4) { l = +v[0]; t = +v[1]; r = +v[2]; b = +v[3]; }
            else if (v && typeof v === 'object') { l = +v.left || 0; t = +v.top || 0; r = +v.right || 0; b = +v.bottom || 0; }
            else return false;
            if (!isFinite(l) || !isFinite(t) || !isFinite(r) || !isFinite(b)) return false;
            ENV.insets = { l: l, r: r, t: t, b: b };
            ENV.insetsSrc = src || 'dex';
            return true;
        } catch (e) { return false; }
    }
    function detectEnv() {
        try { ENV.platform = nstr(cc.sys && cc.sys.platform); } catch (e) {}
        ENV.ref = hasRef();
        try { ENV.fileIO = !!(typeof jsb !== 'undefined' && jsb && jsb.fileUtils); } catch (e) {}
        try { ENV.editBox = !!(typeof cc !== 'undefined' && cc && cc.EditBox); } catch (e) {}

        // ① dex 侧优先：window.__WX_ENV_JSON__ = {"sdk":36,"brand":"HUAWEI","insets":[l,t,r,b]}
        var ex = null;
        try {
            if (window.__WX_ENV_JSON__) ex = (typeof window.__WX_ENV_JSON__ === 'string') ? JSON.parse(window.__WX_ENV_JSON__) : window.__WX_ENV_JSON__;
        } catch (e) { ex = null; }
        if (ex && typeof ex === 'object') {
            ENV.sdk = parseInt(ex.sdk || ex.sdkInt || 0, 10) || 0;
            ENV.release = nstr(ex.release); ENV.brand = nstr(ex.brand);
            ENV.model = nstr(ex.model); ENV.manufacturer = nstr(ex.manufacturer);
            ENV.harmonyVer = nstr(ex.harmonyVer || ex.harmony || '');
            ENV.emui = nstr(ex.emui);
            if (ex.insets) applyInsets(ex.insets, 'dex');
            if (ex.emulator !== undefined) ENV.emu = !!ex.emulator;
            ENV.notes.push('env-from-dex');
        }
        // ② build.prop（不依赖 Java；鸿蒙兼容层同样有这些文件）
        if (!ENV.sdk) ENV.sdk = parseInt(prop('ro.build.version.sdk'), 10) || 0;
        if (!ENV.release) ENV.release = prop('ro.build.version.release');
        if (!ENV.brand) ENV.brand = prop('ro.product.brand') || prop('ro.product.vendor.brand');
        if (!ENV.model) ENV.model = prop('ro.product.model') || prop('ro.product.vendor.model');
        if (!ENV.manufacturer) ENV.manufacturer = prop('ro.product.manufacturer');
        if (!ENV.harmonyVer) ENV.harmonyVer = prop('hw_sc.build.platform.version') || prop('ro.build.harmony.version') || prop('ro.build.version.harmony');
        if (!ENV.emui) ENV.emui = prop('ro.build.version.emui');
        // ③ UA 兜底
        if (!ENV.sdk || !ENV.model) uaProbe();
        // ④ 静态方法能拿到的最后一块：Build.getSerial()（API 26+，要 READ_PHONE_STATE，失败就是空串）
        if (!ENV.serial) { try { ENV.serial = nstr(jcall('android/os/Build', 'getSerial', '()Ljava/lang/String;')); } catch (e) {} }

        var blob = (ENV.brand + '|' + ENV.manufacturer + '|' + ENV.model + '|' + ENV.emui + '|' + ENV.harmonyVer + '|' +
                    prop('ro.build.fingerprint') + '|' + prop('ro.build.display.id') + '|' + (ENV.uaBuild || '')).toUpperCase();
        ENV.harmony = /HUAWEI|HONOR|HARMONY|EMUI|HMSCORE/.test(blob) || !!(ENV.harmonyVer);
        ENV.emu = !!prop('ro.kernel.qemu') || /GENERIC|EMULATOR|SDK_GPHONE|VBOX|ANDROID_X86|MICROVIRT|ZHUOYITONG/.test(blob);
        try { window.__WX_ENV__ = ENV; } catch (e) {}
        try { refreshGeometry(); } catch (e) {}          // 安全区尽早算出来，状态文件里能直接看到
        try { console.log('[PATCH] env ' + diagLine()); } catch (e) {}
        return ENV;
    }
    function diagLine() {
        return 'sdk=' + (ENV.sdk || '?') + ' rel=' + (ENV.release || '?') + ' brand=' + (ENV.brand || '?') +
               ' model=' + (ENV.model || '?') + ' harmony=' + (ENV.harmony ? 1 : 0) + (ENV.harmonyVer ? ('/' + ENV.harmonyVer) : '') +
               ' emu=' + (ENV.emu ? 1 : 0) + ' ref=' + (ENV.ref ? 1 : 0) + ' editbox=' + (ENV.editBox ? 1 : 0) +
               ' insets=' + ENV.insetsSrc + ' safe=' + (ENV.safe ? ENV.safe.src : '-');
    }

    /* ---- 可见尺寸 / 帧尺寸 / 安全区 ---- */
    function visibleSize() {
        var w = 0, h = 0;
        try { var s = cc.view.getVisibleSize(); w = s.width; h = s.height; } catch (e) {}
        if (!(w > 0) || !(h > 0)) { try { w = cc.winSize.width; h = cc.winSize.height; } catch (e) {} }
        if (!(w > 0) || !(h > 0)) { w = 1280; h = 720; }
        return { width: w, height: h };
    }
    function frameSize() {
        var w = 0, h = 0;
        try { var f = cc.view.getFrameSize(); w = f.width; h = f.height; } catch (e) {}
        if (!(w > 0) || !(h > 0)) { try { w = window.innerWidth; h = window.innerHeight; } catch (e) {} }
        return { width: w || 0, height: h || 0 };
    }
    function dpr() { try { return cc.view.getDevicePixelRatio() || 1; } catch (e) { return 1; } }

    function computeSafe(vs, fr) {
        var hw = vs.width / 2, hh = vs.height / 2;
        var s = { x0: -hw, y0: -hh, x1: hw, y1: hh, src: 'full' }, got = false;
        // ① dex 给的 insets（像素）换算到设计分辨率
        if (ENV.insetsSrc === 'dex' && fr.width > 0 && fr.height > 0) {
            var sx = vs.width / fr.width, sy = vs.height / fr.height;
            s.x0 += ENV.insets.l * sx; s.x1 -= ENV.insets.r * sx;
            s.y1 -= ENV.insets.t * sy; s.y0 += ENV.insets.b * sy;
            s.src = 'dex-insets'; got = true;
        }
        // ② 引擎自带安全区（刘海屏，Cocos 2.4+）
        if (!got) {
            var r = null;
            try { if (cc.sys && typeof cc.sys.getSafeAreaRect === 'function') r = cc.sys.getSafeAreaRect(); } catch (e) {}
            if (r && r.width > 0 && r.height > 0) {
                // 2.4 各版本返回值坐标系不一致：落在 [0,visible] 内当"左下原点"，否则当"中心原点"
                var bl = (r.x >= -1 && r.y >= -1 && (r.x + r.width) <= vs.width + 2 && (r.y + r.height) <= vs.height + 2);
                var x0, y0, x1, y1;
                if (bl) { x0 = r.x - hw; y0 = r.y - hh; x1 = r.x + r.width - hw; y1 = r.y + r.height - hh; }
                else { x0 = r.x; y0 = r.y; x1 = r.x + r.width; y1 = r.y + r.height; }
                if ((x1 - x0) > vs.width * 0.55 && (y1 - y0) > vs.height * 0.35 && x0 >= -hw - 2 && x1 <= hw + 2) {
                    s.x0 = x0; s.x1 = x1; s.y0 = y0; s.y1 = y1; s.src = 'safeAreaRect'; got = true;
                }
            }
        }
        // ③ 兜底启发式：设计分辨率长宽比 > 1.9 基本是挖孔/长条屏。
        //    注意横竖屏不一样：横屏全屏游戏里状态栏被隐藏，挖孔在左右某一侧、手势条在底部；
        //    竖屏才是"顶部状态栏 + 底部手势条"。按同一个方向收边会把球挤到中间，所以分开处理。
        if (!got) {
            var long = Math.max(vs.width, vs.height), short = Math.min(vs.width, vs.height);
            var ratio = short > 0 ? (long / short) : 1.78;
            if (ratio > 1.9) {
                if (vs.height <= vs.width) {
                    var padSide = Math.round(short * 0.030), padBotL = Math.round(short * 0.048);
                    s.x0 += padSide; s.x1 -= padSide; s.y0 += padBotL;
                    s.src = 'heur-land(' + ratio.toFixed(2) + ')';
                } else {
                    var padTopP = Math.round(short * 0.085), padBotP = Math.round(short * 0.045);
                    s.y1 -= padTopP; s.y0 += padBotP;
                    s.src = 'heur-port(' + ratio.toFixed(2) + ')';
                }
            }
        }
        // ④ 手动微调：wx_pad_l / wx_pad_t / wx_pad_r / wx_pad_b（设计分辨率单位）
        var pl = parseFloat(storageGet('wx_pad_l', '0')) || 0, pt = parseFloat(storageGet('wx_pad_t', '0')) || 0;
        var pr = parseFloat(storageGet('wx_pad_r', '0')) || 0, pb = parseFloat(storageGet('wx_pad_b', '0')) || 0;
        if (pl || pt || pr || pb) { s.x0 += pl; s.y1 -= pt; s.x1 -= pr; s.y0 += pb; s.src += '+pad'; }
        // 算错也不能把界面挤没：任一边缩掉超过一半就整条回滚
        if (s.x1 - s.x0 < vs.width * 0.5) { s.x0 = -hw; s.x1 = hw; s.src += '|x-reset'; }
        if (s.y1 - s.y0 < vs.height * 0.5) { s.y0 = -hh; s.y1 = hh; s.src += '|y-reset'; }
        return s;
    }
    function refreshGeometry() {
        var vs = visibleSize(), fr = frameSize();
        ENV.visible = [Math.round(vs.width), Math.round(vs.height)];
        ENV.frame = [Math.round(fr.width), Math.round(fr.height)];
        ENV.dpr = dpr();
        ENV.safe = computeSafe(vs, fr);
        return ENV.safe;
    }
    function safeRect() {
        if (!ENV.safe) refreshGeometry();
        return ENV.safe || { x0: -640, y0: -360, x1: 640, y1: 360, src: 'default' };
    }
    function clampToSafe(x, y, hw, hh) {
        var s = safeRect();
        var minX = s.x0 + (hw || 0), maxX = s.x1 - (hw || 0);
        var minY = s.y0 + (hh || 0), maxY = s.y1 - (hh || 0);
        return {
            x: (minX > maxX) ? (s.x0 + s.x1) / 2 : Math.max(minX, Math.min(maxX, x)),
            y: (minY > maxY) ? (s.y0 + s.y1) / 2 : Math.max(minY, Math.min(maxY, y))
        };
    }

    /* ---- 挂载点：必须落在当前活动场景的 Canvas 子树里 ---- */
    function nodeMounted(n) {
        try {
            if (!nodeOk(n) || n.activeInHierarchy === false) return false;
            var scene = cc.director && cc.director.getScene && cc.director.getScene();
            if (!scene) return false;
            var p = n, depth = 0;
            while (p && depth++ < 64) {
                if (p.activeInHierarchy === false) return false;
                if (p === scene) return true;
                p = p.parent;
            }
        } catch (e) {}
        return false;
    }
    // 该游戏自身使用 cc.macro.MAX_ZINDEX 放置顶层 HUD；补丁沿用同一引擎常量，
    // 再配合 siblingIndex 置顶，避免 OxygenOS/ColorOS 多 Canvas 下节点存在但被全屏 UI 覆盖。
    var UI_LAYER = { ball: 32000, panel: 32010, auth: 32760 };
    function safeLayer(z) {
        var max = 32760;
        try {
            var m = Number(cc.macro && cc.macro.MAX_ZINDEX);
            if (isFinite(m) && m > max) max = Math.min(m, 999999999);
        } catch (e) {}
        if (z === UI_LAYER.auth) return max;
        if (z === UI_LAYER.panel) return max - 2;
        if (z === UI_LAYER.ball) return max - 4;
        z = parseInt(z, 10);
        return isFinite(z) ? Math.min(max, z) : max - 4;
    }
    function canvasScore(n) {
        try {
            if (!n || !nodeMounted(n) || !cc.Canvas || !n.getComponent) return -1;
            var canvas = n.getComponent(cc.Canvas);
            if (!canvas || canvas.enabled === false) return -1;
            var score = 1000;
            try { if (cc.Canvas.instance && cc.Canvas.instance.node === n) score += 120; } catch (e0) {}
            try {
                var camera = canvas.camera || canvas._camera;
                if (camera && camera.enabled === false) return -1;
                if (camera) {
                    score += 80;
                    var depth = Number(camera.depth);
                    if (!isFinite(depth)) depth = Number(camera._depth);
                    if (isFinite(depth)) score += Math.max(-200, Math.min(200, depth * 20));
                }
            } catch (e1) {}
            try {
                var priority = Number(canvas.priority);
                if (!isFinite(priority)) priority = Number(canvas._priority);
                if (isFinite(priority)) score += Math.max(-100, Math.min(100, priority));
            } catch (e1b) {}
            try {
                var size = n.getContentSize();
                var vs = visibleSize();
                if (size && size.width >= vs.width * 0.5 && size.height >= vs.height * 0.5) score += 60;
            } catch (e2) {}
            try { score += Math.max(-40, Math.min(40, (+n.zIndex || 0) / 100)); } catch (e3) {}
            return score;
        } catch (e) { return -1; }
    }
    function findCanvasNode() {
        try {
            var scene = cc.director.getScene();
            if (!scene || scene.activeInHierarchy === false) return null;
            var best = null, bestScore = -1;
            function consider(n) {
                var score = canvasScore(n);
                if (score > bestScore) { best = n; bestScore = score; }
            }
            try { if (cc.Canvas && cc.Canvas.instance) consider(cc.Canvas.instance.node); } catch (e0) {}
            (function walk(n, d) {
                if (!n || d > 64 || n.activeInHierarchy === false) return;
                consider(n);
                var ch = [];
                try { ch = n.children || []; } catch (e1) {}
                for (var i = 0; i < ch.length; i++) walk(ch[i], d + 1);
            })(scene, 0);
            if (best) return best;

            // 老版本没有可查询的 Canvas 组件时才走兼容入口；不再优先使用可能位于 Mask 下的业务节点。
            try {
                var named = cc.find && cc.find('Canvas', scene);
                if (nodeMounted(named)) return named;
            } catch (e2) {}
            try { if (window.g_uiTopNode && nodeMounted(window.g_uiTopNode)) return window.g_uiTopNode; } catch (e3) {}
            // Creator 小版本切换期间可能尚未注册 Canvas.instance；活动场景仍可作为短时挂载点，
            // 下一次看门狗会在 Canvas 可用后重新校正。
            return nodeMounted(scene) ? scene : null;
        } catch (e) { return null; }
    }
    function floatGroup(p) {
        var name = null, index = -1, mask = null;
        try { name = p.group; index = p.groupIndex; } catch (e0) {}
        try {
            var canvas = cc.Canvas && p.getComponent && p.getComponent(cc.Canvas);
            var camera = canvas && (canvas.camera || canvas._camera);
            if (camera) mask = camera.cullingMask !== undefined ? camera.cullingMask : camera._cullingMask;
        } catch (e1) {}
        if (mask === null || mask === undefined || (index >= 0 && (mask & (1 << index)) !== 0)) return name;
        try {
            var groups = cc.game && (cc.game.groupList || cc.game._groupList);
            if (groups) for (var i = 0; i < groups.length && i < 32; i++) {
                if (groups[i] && (mask & (1 << i)) !== 0) return groups[i];
            }
        } catch (e2) {}
        return name;
    }
    function bringFloatToFront(node, z) {
        if (!node || !node.parent) return false;
        try {
            var best = findCanvasNode();
            if (best && best !== node.parent) {
                var bestCanvas = cc.Canvas && best.getComponent && best.getComponent(cc.Canvas);
                var curCanvas = cc.Canvas && node.parent.getComponent && node.parent.getComponent(cc.Canvas);
                if (bestCanvas && (!curCanvas || canvasScore(best) > canvasScore(node.parent))) {
                    node.removeFromParent(false);
                    best.addChild(node, safeLayer(z));
                    node.__wxFloatGroup = floatGroup(best);
                }
            }
        } catch (eMove) {}
        var layer = safeLayer(z);
        try { node.active = true; node.opacity = 255; } catch (e0) {}
        try { node.zIndex = layer; } catch (e1) {}
        try { if (typeof node.setLocalZOrder === 'function') node.setLocalZOrder(layer); } catch (e2) {}
        try {
            var p = node.parent;
            var count = p.childrenCount;
            if (!(count >= 0) && p.children) count = p.children.length;
            if (count > 0 && typeof node.setSiblingIndex === 'function') node.setSiblingIndex(count - 1);
        } catch (e3) {}
        try { syncFloatTree(node); } catch (e4) {}
        return nodeMounted(node);
    }
    function attachFloat(node, z) {
        var p = findCanvasNode();
        if (!p || !node) return null;
        var layer = safeLayer(z);
        // OxygenOS/部分原生渲染后端不继承父节点分组，必须选 Camera 确实可见的组。
        try { node.__wxFloatGroup = floatGroup(p); if (node.__wxFloatGroup != null) node.group = node.__wxFloatGroup; } catch (e) {}
        try { node.active = true; node.opacity = 255; } catch (e) {}
        try { p.addChild(node, layer); }
        catch (e) { try { p.addChild(node); } catch (e2) { return null; } }
        try { bringFloatToFront(node, z); } catch (e) {}
        if (!nodeMounted(node)) {
            try { node.removeFromParent(false); } catch (e) {}
            return null;
        }
        return p;
    }
    function syncFloatGroup(node, group) {
        if (!node) return;
        try { node.group = group; } catch (e) {}
        var ch = [];
        try { ch = node.children || []; } catch (e) {}
        for (var i = 0; i < ch.length; i++) syncFloatGroup(ch[i], group);
    }
    function syncFloatTree(node) {
        try {
            if (!node || !node.parent) return;
            var group = node.__wxFloatGroup != null ? node.__wxFloatGroup : node.parent.group;
            if (group === undefined || group === null) return;
            syncFloatGroup(node, group);
        } catch (e) {}
    }

    /* ---- 四路 tick 驱动：任何一路活着，悬浮窗就能建起来 ---- */
    var DRV = { started: false, sched: false, after: false, raf: false, timeout: false, target: null, n: 0, last: 0, lastRun: 0, lastHeal: 0, busy: false, eng: 0, err: '' };
    function safeTick(src) {
        var now = Date.now();
        // RAF, scheduler, timeout and the DEX ticker may fire in the same frame.
        // Throttle the whole health check, not just the inner feature tick.
        if (DRV.busy || now - DRV.lastRun < PERF.tickMs) return;
        DRV.lastRun = now;
        DRV.n++;
        DRV.busy = true;
        try {
            if (typeof window.__WX_TICK__ === 'function') window.__WX_TICK__();
            if (now - DRV.lastHeal >= PERF.healMs) {
                DRV.lastHeal = now;
                healFloat();
            }
        } catch (e) { if (!DRV.err) DRV.err = src + ':' + (e && e.message); }
        finally { DRV.busy = false; }
    }
    function attachEngineDrivers() {
        var ok = false;
        if (typeof cc === 'undefined' || !cc || !cc.director) return false;
        try {
            var sch = cc.director.getScheduler && cc.director.getScheduler();
            if (sch && typeof sch.schedule === 'function' && !DRV.sched) {
                DRV.target = DRV.target || { __wx: 1 };
                var forever = (cc.macro && cc.macro.REPEAT_FOREVER !== undefined) ? cc.macro.REPEAT_FOREVER : 2147483647;
                sch.schedule(function () { safeTick('sched'); }, DRV.target, 0.5, forever, 0, false);
                DRV.sched = true; ok = true;
            }
        } catch (e) {}
        try {
            if (cc.Director && cc.Director.EVENT_AFTER_UPDATE && !DRV.after) {
                cc.director.on(cc.Director.EVENT_AFTER_UPDATE, function () {
                    var now = Date.now();
                    if (now - DRV.last < PERF.tickMs) return;
                    DRV.last = now; safeTick('afterUpdate');
                });
                DRV.after = true; ok = true;
            }
        } catch (e) {}
        if (ok) DRV.eng = Date.now();
        return DRV.sched || DRV.after;
    }
    function startDrivers() {
        // 每一路单独判重；早注入时没有可用驱动，不会再被 started 永久锁死。
        if (!DRV.timeout) try {
            var st = (typeof setTimeout === 'function') ? setTimeout : null;
            if (st) {
                var timeoutLoop = function () {
                    safeTick('timeout');
                    if (!DRV.sched && !DRV.after) attachEngineDrivers();
                    try { st(timeoutLoop, (DRV.sched || DRV.after) ? 1200 : PERF.tickMs); }
                    catch (e0) { DRV.timeout = false; DRV.started = !!(DRV.sched || DRV.after || DRV.raf); }
                };
                st(timeoutLoop, 0);
                DRV.timeout = true;
            }
        } catch (e) { DRV.timeout = false; }
        // C) requestAnimationFrame
        if (!DRV.raf) try {
            var raf = (typeof requestAnimationFrame === 'function') ? requestAnimationFrame
                    : (typeof window !== 'undefined' && window.requestAnimationFrame) ? window.requestAnimationFrame : null;
            if (raf) {
                DRV.raf = true;
                raf(function loop() {
                    if (DRV.after || DRV.sched) { DRV.raf = false; DRV.started = true; return; }
                    DRV.raf = true;
                    safeTick('raf');
                    raf(loop);
                });
            }
        } catch (e) { DRV.raf = false; }
        try { attachEngineDrivers(); } catch (e) {}
        DRV.started = !!(DRV.timeout || DRV.raf || DRV.sched || DRV.after);
    }
    function installTickThrottle() {
        try {
            var orig = window.__WX_TICK__;
            if (typeof orig !== 'function' || orig.__wxThrottled) return;
            var wrapped = function () {
                var now = Date.now();
                if (now - (window.__WX_TICK_T__ || 0) < PERF.tickMs - 25) return 0;    // 四路驱动会重叠，这里统一节流
                window.__WX_TICK_T__ = now;
                return orig.apply(this, arguments);
            };
            wrapped.__wxThrottled = true; wrapped.__wxOrig = orig;
            window.__WX_TICK__ = wrapped;
            window.__DMG_TICK__ = wrapped;
        } catch (e) {}
    }
    function hookResize() {
        function onResize() { try { refreshGeometry(); clampFloatIntoView(); } catch (e) {} }
        try { if (cc.view && cc.view.on) cc.view.on('canvas-resize', onResize); } catch (e) {}
        try { if (cc.view && cc.view.on) cc.view.on('design-resolution-changed', onResize); } catch (e) {}
        try { if (typeof window !== 'undefined' && window.addEventListener) window.addEventListener('resize', onResize, false); } catch (e) {}
    }
    function clampFloatIntoView() {
        try {
            refreshGeometry();
            if (UI.root && nodeOk(UI.root)) {
                var p = clampToSafe(UI.root.x, UI.root.y, 48, 48);
                UI.root.x = p.x; UI.root.y = p.y; saveFloat(p.x, p.y);
            }
            if (UI.proot && nodeOk(UI.proot)) {
                var g = UI.geo || { W: 600, H: 650 };
                var q = clampToSafe(UI.proot.x, UI.proot.y, -g.W / 2 + 90, -g.H / 2 + 90);   // 允许只露 90，保证能拖回来
                UI.proot.x = q.x; UI.proot.y = q.y; savePanelState(q.x, q.y);
            }
        } catch (e) {}
    }

    /* ---- 设备码：多路 + 落盘装机指纹（鸿蒙上 android_id 可能取不到或每次装都变） ---- */
    var DEV = { id: '', src: '', file: '' };
    function devFile() {
        if (!DEV.file) { var w = writablePath(); DEV.file = w ? (w + 'wx_dev.dat') : ''; }
        return DEV.file;
    }
    function localFingerprint() {
        var f = devFile(), s = '';
        try { if (f) s = nstr(readFile(f)).replace(/[^0-9a-zA-Z]/g, ''); } catch (e) {}
        if (s && s.length >= 16) return s.slice(0, 32);
        // 首次生成后落盘：同一台机器 mark 保持不变，卡密不会被判成"换机"
        var seed = prop('ro.build.fingerprint') + '|' + prop('ro.serialno') + '|' + prop('ro.product.model') + '|' +
                   (ENV.serial || '') + '|' + Date.now() + '|' + Math.random();
        s = hex_md5(seed).slice(0, 24);
        try { if (f) writeFile(f, s); } catch (e) {}
        return s;
    }
    var HEAL = { miss: 0, last: 0, rebuilds: 0, sceneAt: 0 };
    function floatBuildAllowed() {
        return !HEAL.sceneAt || Date.now() - HEAL.sceneAt >= 1500;
    }
    function healFloat() {
        try {
            if (!window.__WX_AUTH_OK__) { HEAL.miss = 0; try { authBump(); } catch (eAuth) {} return; }
            if (!floatBuildAllowed()) { HEAL.miss = 0; return; }
            var rootAlive = UI.root && nodeMounted(UI.root);
            var ballAlive = UI.ball && nodeOk(UI.ball) && UI.ball.parent;
            var panelAlive = UI.panel && nodeOk(UI.panel) && UI.panel.parent;
            if (rootAlive && ballAlive && panelAlive && UI.proot && nodeMounted(UI.proot)) {
                // An inactive ball is expected only while the panel is actually open.
                if (UI.open && !UI.panel.active) UI.panel.active = true;
                if (!UI.open && !UI.ball.active) UI.ball.active = true;
                if (UI.open) bringFloatToFront(UI.proot, UI_LAYER.panel);
                else bringFloatToFront(UI.root, UI_LAYER.ball);
                HEAL.miss = 0;
                return;
            }
            HEAL.miss++;
            if (HEAL.miss < 2) return;                       // 两次低频检查都缺才重建，避开切图瞬间的临时失挂
            var now = Date.now();
            if (now - HEAL.last < 3000) return;
            HEAL.last = now; HEAL.miss = 0; HEAL.rebuilds++;
            buildMenu();
        } catch (e) {}
    }

    /* ---- 诊断：出问题时一条命令把现场导出来 ---- */
    function diagObj() {
        return {
            patch: WXV, pkg: pkgName(), sdk: ENV.sdk, release: ENV.release, brand: ENV.brand, model: ENV.model,
            manufacturer: ENV.manufacturer, harmony: ENV.harmony, harmonyVer: ENV.harmonyVer, emui: ENV.emui,
            emulator: ENV.emu, ua: ENV.ua || '', buildProp: ENV.buildProp,
            insets: [ENV.insets.l, ENV.insets.t, ENV.insets.r, ENV.insets.b], insetsSrc: ENV.insetsSrc,
            visible: ENV.visible, frame: ENV.frame, dpr: ENV.dpr, safe: ENV.safe,
            reflection: ENV.ref, refFail: ENV.refFail, editBox: ENV.editBox, fileIO: ENV.fileIO,
            drv: { sched: DRV.sched, after: DRV.after, raf: DRV.raf, timeout: DRV.timeout, n: DRV.n, err: DRV.err },
            heal: HEAL.rebuilds, dev: { src: DEV.src, id: DEV.id },
            auth: !!window.__WX_AUTH_OK__, ui: window.__WX_UI_STATUS__ || '',
            authUiReady: !!window.__WX_UI_READY__, authRebuilds: window.__WX_AUTH_REBUILDS__ || 0,
            patchFlag: !!window.__WX_PATCH__,
            ball: !!(UI.ball && nodeOk(UI.ball) && UI.ball.active),
            panel: !!(UI.panel && nodeOk(UI.panel) && UI.panel.active),
            panelBuilt: !!(UI.panel && nodeOk(UI.panel) && UI.panel.parent), open: !!UI.open,
            pos: UI.root ? [Math.round(UI.root.x), Math.round(UI.root.y)] : null,
            notes: ENV.notes
        };
    }
    window.__WX_DIAG__ = function (pretty) { try { return JSON.stringify(diagObj(), null, pretty ? 2 : 0); } catch (e) { return '{}'; } };
    window.__WX_DIAG_COPY__ = function () {
        var s = window.__WX_DIAG__();
        try { if (typeof jsb !== 'undefined' && jsb && typeof jsb.copyTextToClipboard === 'function') { jsb.copyTextToClipboard(s); return true; } } catch (e) {}
        try { if (typeof window.__WX_NATIVE_CALL__ === 'function') return !!window.__WX_NATIVE_CALL__('setClipboard', [s]); } catch (e) {}
        return false;
    };
    /*<<<WX_COMPAT_END>>>*/

    /* ================= 1. 热更新适配 =================
     * 我们的补丁本身不落盘，所以热更新不会把补丁"冲掉"；真正要处理的是：
     *   a) 引擎/JS 重启后补丁要重新注入（dex 侧的重试循环负责，这里给个标记）
     *   b) 游戏换 res 版本后缓存目录不一致时，清掉自己的状态，避免读到旧版本号
     */
    window.__WX_DISABLE_HOTUPDATE_GUARD__ = true;
    function hotUpdateAdapt() {
        try {
            var storage = cc && cc.sys && cc.sys.localStorage;
            if (!storage) return false;
            if (storage.getItem('__WX_VER__') !== storage.getItem('__WX_VER_SEEN__')) {
                storage.setItem('__WX_VER_SEEN__', storage.getItem('__WX_VER__') || '');
            }
        } catch (e) {}
        return true;
    }

    /* ================= 2. 卡密验证（保留原逻辑：MD5 签名 + RC4 + 心跳 + 公告） ================= */
    /* v66：鸿蒙兼容层里 cc.sys.localStorage 可能是空壳（读回 null / 写入不落盘），
     * 所以每次读写都再过一份 wx_cfg.json，两边取到哪个算哪个。 */
    var __KV = { file: '', mem: null };
    function kvFile() {
        if (!__KV.file) { var w = writablePath(); __KV.file = w ? (w + 'wx_cfg.json') : ''; }
        return __KV.file;
    }
    function kvLoad() {
        if (__KV.mem) return __KV.mem;
        __KV.mem = {};
        var f = kvFile();
        if (f) {
            try {
                var t = readFile(f);
                if (t && t.charAt(0) === '{') __KV.mem = JSON.parse(t) || {};
            } catch (e) { __KV.mem = {}; }
        }
        return __KV.mem;
    }
    function storageGet(k, d) {
        try {
            var v = cc.sys.localStorage.getItem(k);
            if (v !== null && typeof v !== 'undefined') return v;
        } catch (e) {}
        var m = kvLoad();
        return (m[k] === undefined) ? d : m[k];
    }
    function storageSet(k, v) {
        var ok = false;
        try { cc.sys.localStorage.setItem(k, String(v)); ok = true; } catch (e) {}
        try { var m = kvLoad(); m[k] = String(v); var f = kvFile(); if (f) writeFile(f, JSON.stringify(m)); ok = true; } catch (e) {}
        return ok;
    }
    function log(s) { try { console.log('[验证] ' + s); } catch (e) {} }

    /* --- MD5（标准实现，签名用） --- */
    function hex_md5(s) {
        function add32(a, b) { return (a + b) & 0xffffffff; }
        function cmn(q, a, b, x, s2, t) { a = add32(add32(a, q), add32(x, t)); return add32((a << s2) | (a >>> (32 - s2)), b); }
        function ff(a, b, c, d, x, s2, t) { return cmn((b & c) | ((~b) & d), a, b, x, s2, t); }
        function gg(a, b, c, d, x, s2, t) { return cmn((b & d) | (c & (~d)), a, b, x, s2, t); }
        function hh(a, b, c, d, x, s2, t) { return cmn(b ^ c ^ d, a, b, x, s2, t); }
        function ii(a, b, c, d, x, s2, t) { return cmn(c ^ (b | (~d)), a, b, x, s2, t); }
        function md5cycle(x, k) {
            var a = x[0], b = x[1], c = x[2], d = x[3];
            a = ff(a, b, c, d, k[0], 7, -680876936); d = ff(d, a, b, c, k[1], 12, -389564586); c = ff(c, d, a, b, k[2], 17, 606105819); b = ff(b, c, d, a, k[3], 22, -1044525330);
            a = ff(a, b, c, d, k[4], 7, -176418897); d = ff(d, a, b, c, k[5], 12, 1200080426); c = ff(c, d, a, b, k[6], 17, -1473231341); b = ff(b, c, d, a, k[7], 22, -45705983);
            a = ff(a, b, c, d, k[8], 7, 1770035416); d = ff(d, a, b, c, k[9], 12, -1958414417); c = ff(c, d, a, b, k[10], 17, -42063); b = ff(b, c, d, a, k[11], 22, -1990404162);
            a = ff(a, b, c, d, k[12], 7, 1804603682); d = ff(d, a, b, c, k[13], 12, -40341101); c = ff(c, d, a, b, k[14], 17, -1502002290); b = ff(b, c, d, a, k[15], 22, 1236535329);
            a = gg(a, b, c, d, k[1], 5, -165796510); d = gg(d, a, b, c, k[6], 9, -1069501632); c = gg(c, d, a, b, k[11], 14, 643717713); b = gg(b, c, d, a, k[0], 20, -373897302);
            a = gg(a, b, c, d, k[5], 5, -701558691); d = gg(d, a, b, c, k[10], 9, 38016083); c = gg(c, d, a, b, k[15], 14, -660478335); b = gg(b, c, d, a, k[4], 20, -405537848);
            a = gg(a, b, c, d, k[9], 5, 568446438); d = gg(d, a, b, c, k[14], 9, -1019803690); c = gg(c, d, a, b, k[3], 14, -187363961); b = gg(b, c, d, a, k[8], 20, 1163531501);
            a = gg(a, b, c, d, k[13], 5, -1444681467); d = gg(d, a, b, c, k[2], 9, -51403784); c = gg(c, d, a, b, k[7], 14, 1735328473); b = gg(b, c, d, a, k[12], 20, -1926607734);
            a = hh(a, b, c, d, k[5], 4, -378558); d = hh(d, a, b, c, k[8], 11, -2022574463); c = hh(c, d, a, b, k[11], 16, 1839030562); b = hh(b, c, d, a, k[14], 23, -35309556);
            a = hh(a, b, c, d, k[1], 4, -1530992060); d = hh(d, a, b, c, k[4], 11, 1272893353); c = hh(c, d, a, b, k[7], 16, -155497632); b = hh(b, c, d, a, k[10], 23, -1094730640);
            a = hh(a, b, c, d, k[13], 4, 681279174); d = hh(d, a, b, c, k[0], 11, -358537222); c = hh(c, d, a, b, k[3], 16, -722521979); b = hh(b, c, d, a, k[6], 23, 76029189);
            a = hh(a, b, c, d, k[9], 4, -640364487); d = hh(d, a, b, c, k[12], 11, -421815835); c = hh(c, d, a, b, k[15], 16, 530742520); b = hh(b, c, d, a, k[2], 23, -995338651);
            a = ii(a, b, c, d, k[0], 6, -198630844); d = ii(d, a, b, c, k[7], 10, 1126891415); c = ii(c, d, a, b, k[14], 15, -1416354905); b = ii(b, c, d, a, k[5], 21, -57434055);
            a = ii(a, b, c, d, k[12], 6, 1700485571); d = ii(d, a, b, c, k[3], 10, -1894986606); c = ii(c, d, a, b, k[10], 15, -1051523); b = ii(b, c, d, a, k[1], 21, -2054922799);
            a = ii(a, b, c, d, k[8], 6, 1873313359); d = ii(d, a, b, c, k[15], 10, -30611744); c = ii(c, d, a, b, k[6], 15, -1560198380); b = ii(b, c, d, a, k[13], 21, 1309151649);
            a = ii(a, b, c, d, k[4], 6, -145523070); d = ii(d, a, b, c, k[11], 10, -1120210379); c = ii(c, d, a, b, k[2], 15, 718787259); b = ii(b, c, d, a, k[9], 21, -343485551);
            x[0] = add32(a, x[0]); x[1] = add32(b, x[1]); x[2] = add32(c, x[2]); x[3] = add32(d, x[3]);
        }
        function md5blk(s2) {
            var md5blks = [], i;
            for (i = 0; i < 64; i += 4) md5blks[i >> 2] = s2.charCodeAt(i) + (s2.charCodeAt(i + 1) << 8) + (s2.charCodeAt(i + 2) << 16) + (s2.charCodeAt(i + 3) << 24);
            return md5blks;
        }
        function md51(s2) {
            s2 = unescape(encodeURIComponent(s2));
            var n = s2.length, state = [1732584193, -271733879, -1732584194, 271733878], i;
            for (i = 64; i <= n; i += 64) md5cycle(state, md5blk(s2.substring(i - 64, i)));
            s2 = s2.substring(i - 64);
            var tail = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            for (i = 0; i < s2.length; i++) tail[i >> 2] |= s2.charCodeAt(i) << ((i % 4) << 3);
            tail[i >> 2] |= 0x80 << ((i % 4) << 3);
            if (i > 55) { md5cycle(state, tail); for (i = 0; i < 16; i++) tail[i] = 0; }
            tail[14] = n * 8;
            md5cycle(state, tail);
            return state;
        }
        function rhex(n) { var s2 = '', j; for (j = 0; j < 4; j++) s2 += ('0' + ((n >> (j * 8 + 4)) & 0x0F).toString(16)).slice(-1) + ('0' + ((n >> (j * 8)) & 0x0F).toString(16)).slice(-1); return s2; }
        return md51(s).map(rhex).join('');
    }

    /* --- RC4（请求/响应加解密，和验证服务端保持一致） --- */
    function rc4(data, key, encrypt) {
        if (encrypt === true) return strToHex(rc4Core(utf8Bytes(String(data)), key));
        data = String(data == null ? '' : data).trim();
        if (/^[0-9a-fA-F]+$/.test(data) && data.length % 2 === 0) data = hexToStr(data);
        return utf8Text(rc4Core(data, key));
    }
    function rc4Core(data, key) {
        var s2 = [], j = 0, x, res = '', i;
        for (i = 0; i < 256; i++) s2[i] = i;
        for (i = 0; i < 256; i++) { j = (j + s2[i] + key.charCodeAt(i % key.length)) & 255; x = s2[i]; s2[i] = s2[j]; s2[j] = x; }
        i = 0; j = 0;
        for (var y = 0; y < data.length; y++) {
            i = (i + 1) & 255; j = (j + s2[i]) & 255;
            x = s2[i]; s2[i] = s2[j]; s2[j] = x;
            res += String.fromCharCode((data.charCodeAt(y) & 255) ^ s2[(s2[i] + s2[j]) & 255]);
        }
        return res;
    }
    function utf8Bytes(s2) { return unescape(encodeURIComponent(s2)); }
    function fmtDate(ts) {
        var d = new Date(ts * 1000), m = d.getMonth() + 1, day = d.getDate();
        return d.getFullYear() + '-' + (m < 10 ? '0' : '') + m + '-' + (day < 10 ? '0' : '') + day;
    }

    /* 自动页会员等级只信任本次登录响应，不从本地缓存恢复，避免普通卡继承旧会话状态。 */
    function detectPermanentCard(msg, vip) {
        msg = (msg && typeof msg === 'object') ? msg : {};
        var boolKeys = ['permanent', 'isPermanent', 'is_permanent', 'forever', 'isForever', 'is_forever', 'lifetime'];
        for (var i = 0; i < boolKeys.length; i++) {
            var bv = msg[boolKeys[i]];
            if (bv === true || bv === 1 || String(bv).toLowerCase() === 'true' || String(bv) === '1') return true;
        }
        var typeKeys = ['cardType', 'card_type', 'cardtype', 'kamiType', 'kami_type', 'vipType', 'vip_type', 'typeName', 'type_name', 'name'];
        for (var j = 0; j < typeKeys.length; j++) {
            var tv = msg[typeKeys[j]];
            if (tv != null && /(永久|permanent|forever|lifetime)/i.test(String(tv))) return true;
        }
        // 微验旧接口通常只有 vip 到期秒数；永久卡按远期时间戳返回。
        return Number(vip) >= 3786912000; // 2090-01-01 00:00:00 UTC
    }
    function setCardAccess(msg, vip) {
        var permanent = detectPermanentCard(msg, vip);
        window.__WX_AUTH_VIP__ = Number(vip) || 0;
        window.__WX_AUTH_CARD_TYPE__ = permanent ? '永久卡' : '普通卡';
        window.__WX_CARD_PERMANENT__ = permanent;
        window.__Z6_IS_PERMANENT__ = permanent;
        window.__WX_AUTO_GATE_CLEARED__ = false;
        return permanent;
    }
    function isPermanentCard() {
        try { return window.__WX_AUTH_OK__ === true && window.__WX_CARD_PERMANENT__ === true; }
        catch (e) { return false; }
    }

    function strToHex(s2) { var r = ''; for (var i = 0; i < s2.length; i++) { var v = s2.charCodeAt(i) & 255; r += (v < 16 ? '0' : '') + v.toString(16); } return r; }
    function hexToStr(h) { var r = ''; for (var i = 0; i + 1 < h.length; i += 2) r += String.fromCharCode(parseInt(h.substr(i, 2), 16)); return r; }
    function utf8Text(s2) { try { return decodeURIComponent(escape(s2)); } catch (e) { return s2; } }

    function xhr(method, url, body, cb) {
        try {
            var x = new XMLHttpRequest();
            var finished = false;
            function finish(err, text) {
                if (finished) return;
                finished = true;
                cb(err, text || '');
            }
            x.open(method, url, true);
            x.timeout = 15000;
            // 这几行照参考工程 jsdex新.js：微验会检查 UA，缺了/不对容易被判"网络环境异常禁止登录"
            try { x.setRequestHeader && x.setRequestHeader('User-Agent', 'Dalvik/2.1.0 (Linux; U; Android 12)'); } catch (e1) {}
            try { x.setRequestHeader && x.setRequestHeader('Accept', '*/*'); } catch (e2) {}
            if (method === 'POST') {
                try { x.setRequestHeader && x.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded'); } catch (e3) {}
            }
            x.onreadystatechange = function () {
                if (x.readyState !== 4) return;
                var st = x.status;
                var txt = x.responseText || '';
                try { window.__WX_LAST_HTTP__ = [method, st, String(txt).slice(0, 120)]; } catch (e4) {}
                finish(st === 200 ? null : ('HTTP ' + st), txt);
            };
            x.onerror = function () { finish('网络连接失败', x.responseText || ''); };
            x.ontimeout = function () { finish('网络请求超时', ''); };
            x.onabort = function () { finish('网络请求中断', ''); };
            x.send(body || null);
        } catch (e) { cb(e, ''); }
    }
    function androidId() {
        // 反编译核对过（smali）：
        //   org.cocos2dx.javascript.AppActivity 里**没有**任何 static 方法 → 之前调 getContext 必然失败
        //   org.cocos2dx.lib.Cocos2dxHelper 有 public static getActivity()Landroid/app/Activity;  ← 正解
        var id = '';

        // mark 一旦成功使用就固定到本地。切换 Wi-Fi/移动网络后加载器提供值发生抖动，也不能被当成换机。
        try {
            var savedMark = String(storageGet('__WX_STABLE_MARK__', '') || '').trim();
            if (savedMark.length >= 6 && savedMark.length <= 128 && /^[0-9A-Za-z_.:-]+$/.test(savedMark)) {
                DEV.id = savedMark; DEV.src = 'saved-mark';
                window.__WX_MARK__ = savedMark;
                return savedMark;
            }
        } catch (eSaved) {}

        // 路 0：如果加载器（dex）已经把设备码喂进来，直接用
        try { if (window.__WX_ANDROID_ID__) { id = String(window.__WX_ANDROID_ID__); } } catch (e0) {}

        // 路 1：Cocos2dxHelper.getActivity() → getContentResolver() → Settings.Secure
        if (!id) {
            try {
                var act = jsb.reflection.callStaticMethod('org/cocos2dx/lib/Cocos2dxHelper', 'getActivity', '()Landroid/app/Activity;');
                if (act) {
                    var cr = act.getContentResolver();
                    id = String(jsb.reflection.callStaticMethod('android/provider/Settings$Secure', 'getString',
                        '(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;', cr, 'android_id') || '');
                }
            } catch (e1) { log('取设备码(路1 Cocos2dxHelper.getActivity)失败: ' + (e1 && e1.message)); }
        }

        // 路 2：有的版本提供 getContext()
        if (!id) {
            try {
                var ctx = jsb.reflection.callStaticMethod('org/cocos2dx/lib/Cocos2dxHelper', 'getContext', '()Landroid/content/Context;');
                if (ctx) {
                    var cr2 = ctx.getContentResolver();
                    id = String(jsb.reflection.callStaticMethod('android/provider/Settings$Secure', 'getString',
                        '(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;', cr2, 'android_id') || '');
                }
            } catch (e2) { log('取设备码(路2 getContext)失败: ' + (e2 && e2.message)); }
        }

        // 路 3：Application
        if (!id) {
            try {
                var app = jsb.reflection.callStaticMethod('android/app/ActivityThread', 'currentApplication', '()Landroid/app/Application;');
                if (app) {
                    id = String(jsb.reflection.callStaticMethod('android/provider/Settings$Secure', 'getString',
                        '(Landroid/content/ContentResolver;Ljava/lang/String;)Ljava/lang/String;', app.getContentResolver(), 'android_id') || '');
                }
            } catch (e3) { log('取设备码(路3 Application)失败: ' + (e3 && e3.message)); }
        }

        // 路 4：Build.getSerial()（API 26+ 的静态方法，鸿蒙/安卓16 上只要有 READ_PHONE_STATE 就能拿到）
        if (!id) {
            try {
                var ser = nstr(jcall('android/os/Build', 'getSerial', '()Ljava/lang/String;'));
                if (ser && ser !== 'unknown' && ser.length >= 6) { id = hex_md5('wx|serial|' + ser).slice(0, 16); DEV.src = 'build-serial'; }
            } catch (e3b) { log('取设备码(路4 Build.getSerial)失败: ' + (e3b && e3b.message)); }
        }

        // 路 5：全都失败时，用"落盘的装机指纹"兜底。
        // 原来这里是 Date.now()+Math.random()，每次启动都变 → 同一台机器每次进游戏 mark 都不一样，
        // 卡密系统会当成换机；鸿蒙上 android_id 取不到时这条就是主路径，所以必须稳定。
        if (!id) {
            try {
                var lf = localFingerprint();
                id = hex_md5('wx|' + (cc.sys.platform || '') + '|' + prop('ro.build.fingerprint') + '|' + lf).slice(0, 16);
                window.__WX_DEVICE_ID__ = lf;
                DEV.src = DEV.src || 'local-fp';
                log('设备码改用落盘装机指纹: ' + id);
            } catch (e4) { id = ''; }
        }

        DEV.id = id;
        window.__WX_MARK__ = id;
        if (id) storageSet('__WX_STABLE_MARK__', id);
        return id;
    }

    /* --- 验证 UI（Cocos 绘制：卡密输入 + 登录按钮 + 提示） --- */
    var authRoot = null, authMsg = null, authEdit = null, kamiBuf = '';
    function ready() {
        if (typeof cc === 'undefined' || !cc || !cc.Node || !cc.Label || !cc.Graphics || !cc.director || !cc.director.getScene) return false;
        try { return !!(cc.director.getScene() && findCanvasNode()); } catch (e) { return false; }
    }
    function mkLabel(str, fs, w, h, color) {
        var n = new cc.Node(); n.setContentSize(w || 300, h || 44);
        var l = n.addComponent(cc.Label); l.string = str; l.fontSize = fs || 22;
        l.horizontalAlign = cc.Label.HorizontalAlign.CENTER; l.verticalAlign = cc.Label.VerticalAlign.CENTER;
        if (color) n.color = color;
        return n;
    }
    /* ---- 天选配色：和弹壳战机面板逐个值对齐 ---- */
    var TH = {
        accent: [0, 230, 118],        // #00E676 主色亮绿
        panel: [7, 18, 12, 247],      // #07120C 面板底（深绿黑）
        panelBrd: [30, 90, 58],       // #1E5A3A 面板描边
        card: [12, 26, 19],           // #0C1A13 功能块底
        cardBrd: [28, 58, 42],        // #1C3A2A 功能块描边
        input: [8, 20, 14],           // #08140E 输入框底
        tabBg: [16, 42, 28],          // #102A1C 次级按钮底
        tabText: [159, 232, 196],     // #9FE8C4 次级文字
        tabTextOn: [6, 33, 15],       // #06210F 亮绿底上的深色字
        title: [232, 255, 243],       // #E8FFF3 标题
        name: [214, 245, 228],        // #D6F5E4 功能名
        trackOff: [30, 58, 42]        // #1E3A2A 开关/滑条关闭态
    };
    function col(c, a) { return new cc.Color(c[0], c[1], c[2], a == null ? (c[3] == null ? 255 : c[3]) : a); }
    function accentC(a) { return col(TH.accent, a); }
    function subC(a) { return col(TH.tabText, a); }

    /* 圆角胶囊：只给开关/滑条轨道用（弹壳战机的开关本身就是胶囊） */
    function mkRound(g, x, y, w, h, r) { if (g.roundRect) g.roundRect(x, y, w, h, r); else g.rect(x, y, w, h); }
    /* 切角矩形：天选风格的关键 —— 面板/卡片/按钮/输入框一律切角，不用圆角 */
    function mkCut(g, x, y, w, h, k) {
        k = Math.max(0, Math.min(k, Math.min(w, h) / 2));
        g.moveTo(x + k, y); g.lineTo(x + w - k, y); g.lineTo(x + w, y + k);
        g.lineTo(x + w, y + h - k); g.lineTo(x + w - k, y + h);
        g.lineTo(x + k, y + h); g.lineTo(x, y + h - k); g.lineTo(x, y + k);
        g.close();
    }
    /* 主按钮＝亮绿底+深色字，次按钮＝深底+绿字（对应弹壳战机页签的两种状态） */
    function mkBtn(text, w, h, cb, primary) {
        var n = new cc.Node(); n.setContentSize(w, h);
        var g = n.addComponent(cc.Graphics);
        var cut = Math.max(3, Math.min(6, Math.round(h * 0.14)));
        function paint(down) {
            g.clear();
            if (primary) {
                g.fillColor = down ? new cc.Color(0, 200, 103, 255) : accentC();
                mkCut(g, -w / 2, -h / 2, w, h, cut); g.fill();
            } else {
                g.fillColor = down ? accentC(46) : col(TH.tabBg);
                mkCut(g, -w / 2, -h / 2, w, h, cut); g.fill();
                g.strokeColor = col(TH.cardBrd); g.lineWidth = 1;
                mkCut(g, -w / 2, -h / 2, w, h, cut); g.stroke();
            }
        }
        paint(false);
        n.addChild(mkLabel(text, Math.min(24, Math.round(h * 0.5)), w, h,
                primary ? col(TH.tabTextOn) : col(TH.tabText)), 2);
        n.on(cc.Node.EventType.TOUCH_START, function (e) { paint(true); e.stopPropagation && e.stopPropagation(); });
        n.on(cc.Node.EventType.TOUCH_END, function (e) { paint(false); cb && cb(); e.stopPropagation && e.stopPropagation(); });
        n.on(cc.Node.EventType.TOUCH_CANCEL, function () { paint(false); });
        return n;
    }
    /* 左侧页签：当前页＝亮绿底深色字，另一个＝深底绿字（对齐弹壳战机的标签栏） */
    function railTab(parent, x, y, w, h, label, active, cb) {
        var n = new cc.Node('wx_rail_tab');
        n.setContentSize(w, h); n.x = x; n.y = y;
        parent.addChild(n, 6);
        var g = n.addComponent(cc.Graphics);
        var textNode = mkLabel(label, Math.max(14, Math.min(24, Math.round(h * 0.5))), w, h, col(TH.tabText));
        n.addChild(textNode, 2);
        function paint(on) {
            active = !!on;
            g.clear();
            g.fillColor = active ? accentC() : col(TH.tabBg);
            mkCut(g, -w / 2, -h / 2, w, h, 6); g.fill();
            if (!active) {
                g.strokeColor = col(TH.cardBrd); g.lineWidth = 1;
                mkCut(g, -w / 2, -h / 2, w, h, 6); g.stroke();
            }
            textNode.color = active ? col(TH.tabTextOn) : col(TH.tabText);
        }
        n.__setActive = paint;
        paint(active);
        if (cb) n.on(cc.Node.EventType.TOUCH_END, function (e) { cb(); e.stopPropagation && e.stopPropagation(); });
        return n;
    }

    function makeAuth(expire) {
        try {
            if (authRoot && nodeMounted(authRoot)) {
                bringFloatToFront(authRoot, UI_LAYER.auth);
                window.__WX_UI_READY__ = true;
                wxStatus('auth-visible');
                return true;
            }
            if (!ready()) { wxStatus('wait-engine'); window.__WX_UI_READY__ = false; return; }
            wxStatus('auth-building');

            authRoot = new cc.Node('wx_auth_root');
            // 尺寸铺满（甚至超出）屏幕：这样触摸命中一定落在遮罩上
            var authW = 1280, authH = 720;
            try {
                var vsz = (cc.view && cc.view.getVisibleSize) ? cc.view.getVisibleSize() : { width: 1280, height: 720 };
                authW = vsz.width || authW; authH = vsz.height || authH;
                authRoot.setContentSize(authW * 4, authH * 4);
            } catch (e) { authRoot.setContentSize(5120, 2880); }
            var g = authRoot.addComponent(cc.Graphics);
            g.fillColor = new cc.Color(0, 0, 0, 160); g.rect(-authW * 2, -authH * 2, authW * 4, authH * 4); g.fill();
            // 吃掉所有触摸事件：验证通过前，游戏里的东西点不动
            (function () {
                function swallow(e) { try { e.stopPropagation && e.stopPropagation(); } catch (er) {} }
                authRoot.on(cc.Node.EventType.TOUCH_START, swallow);
                authRoot.on(cc.Node.EventType.TOUCH_MOVE, swallow);
                authRoot.on(cc.Node.EventType.TOUCH_END, swallow);
                authRoot.on(cc.Node.EventType.TOUCH_CANCEL, swallow);
                try { if (authRoot._touchListener && authRoot._touchListener.setSwallowTouches) authRoot._touchListener.setSwallowTouches(true); } catch (er) {}
            })();

            var CW = 560, CH = 480;
            var card = new cc.Node('wx_auth_card');
            card.setContentSize(CW, CH);
            var cardScale = Math.min(1, Math.max(0.55, Math.min((authW - 28) / CW, (authH - 28) / CH)));
            card.scale = cardScale;
            authRoot.addChild(card, 2);
            var cg = card.addComponent(cc.Graphics);
            cg.fillColor = new cc.Color(0, 0, 0, 90); mkCut(cg, -CW / 2 + 4, -CH / 2 - 8, CW, CH, 14); cg.fill();
            cg.fillColor = col(TH.panel); mkCut(cg, -CW / 2, -CH / 2, CW, CH, 14); cg.fill();
            cg.strokeColor = col(TH.panelBrd); cg.lineWidth = 2; mkCut(cg, -CW / 2, -CH / 2, CW, CH, 14); cg.stroke();
            cg.fillColor = accentC(); mkCut(cg, -CW / 2 + 22, CH / 2 - 52, 3, 20, 2); cg.fill();
            cg.fillColor = accentC(77); mkCut(cg, -CW / 2 + 12, CH / 2 - 74, CW - 24, 1, 0); cg.fill();

            var t1 = mkLabel('卡密验证中心', 30, CW - 60, 44, col(TH.title));
            try { t1.getComponent(cc.Label).horizontalAlign = cc.Label.HorizontalAlign.LEFT; } catch (e) {}
            card.addChild(t1, 5); t1.x = 18; t1.y = CH / 2 - 40;

            // ---- 输入框（点击即可输入卡密）----
            var boxNode = new cc.Node('wx_auth_box');
            boxNode.setContentSize(440, 62);
            boxNode.y = CH / 2 - 132;
            card.addChild(boxNode, 4);
            var bg = boxNode.addComponent(cc.Graphics);
            bg.fillColor = col(TH.input); mkCut(bg, -220, -31, 440, 62, 4); bg.fill();
            bg.strokeColor = col(TH.cardBrd); bg.lineWidth = 1; mkCut(bg, -220, -31, 440, 62, 4); bg.stroke();

            kamiBuf = String(storageGet('kami', '') || '');
            authEdit = null;
            if (cc.EditBox) {
                try {
                    authEdit = boxNode.addComponent(cc.EditBox);
                    authEdit.fontSize = 24;
                    authEdit.placeholder = '点击这里输入卡密';
                    authEdit.placeholderFontSize = 22;
                    authEdit.string = kamiBuf;
                    try { authEdit.inputMode = cc.EditBox.InputMode.ANY; } catch (e) {}
                    try { if (authEdit.textLabel) { authEdit.textLabel.color = col(TH.title); authEdit.textLabel.fontSize = 24; } } catch (e) {}
                    try { if (authEdit.placeholderLabel) authEdit.placeholderLabel.color = subC(140); } catch (e) {}
                    // EditBox 自带的 textLabel 可能不生效 → 自己在框里再画一层同色文字
            var shownLabel = mkLabel(kamiBuf || '', 24, 400, 46, col(TH.title));
            boxNode.addChild(shownLabel, 6);
            function refreshShown() {
                try {
                    kamiBuf = String((authEdit && authEdit.string) || kamiBuf || '');
                    shownLabel.getComponent(cc.Label).string = kamiBuf;
                } catch (e) {}
            }
            authEdit.node.on('text-changed', function () { kamiBuf = String(authEdit.string || ''); refreshShown(); });
            authEdit.node.on('editing-did-began', function () { try { authEdit.textLabel.color = col(TH.title); } catch (e) {} });
                    authEdit.node.on('editing-did-ended', function () { kamiBuf = String(authEdit.string || ''); });
                } catch (e) { authEdit = null; }
            }
            window.__WX_KAMI_GET__ = function () { return (authEdit && authEdit.string) ? String(authEdit.string) : kamiBuf; };

            authMsg = mkLabel(cc.EditBox ? '请输入卡密后点击登录' : '本机没有输入组件，无法输入卡密', 19, 460, 30, subC(153));
            authMsg.y = CH / 2 - 186; card.addChild(authMsg, 4);
            wxStatus('auth-card');

            // ---- 两个按钮：立即登录 → 退出 ----
            var loginBtn = mkBtn('立即登录', 440, 60, function () {
                try { doLogin(); } catch (e) { setMsg('登录异常：' + (e && e.message)); }
            }, true);
            loginBtn.y = -14; card.addChild(loginBtn, 5);


            var exitBtn = mkBtn('退出', 440, 60, function () {
                try { cc.game.end(); } catch (e) {}
            });
            exitBtn.y = -104; card.addChild(exitBtn, 5);

            var foot = mkLabel('验证成功后显示功能面板', 19, 500, 28, subC(120));
            foot.y = -CH / 2 + 26; card.addChild(foot, 4);

            var ap = null;
            try { ap = attachFloat(authRoot, UI_LAYER.auth); } catch (e) { ap = null; }
            if (ap && nodeMounted(authRoot)) {
                try { bringFloatToFront(authRoot, UI_LAYER.auth); } catch (e) {}
                syncFloatTree(authRoot);
                window.__WX_UI_READY__ = true;
                wxStatus('auth-visible');
                try { console.log('[PATCH] auth ui ready (editbox=' + (!!cc.EditBox) + ')'); } catch (e) {}
                if (expire) try { authMsg.getComponent(cc.Label).string = '有效期至 ' + expire; } catch (e) {}
                return true;
            } else {
                try { if (authRoot && authRoot.parent) authRoot.removeFromParent(false); } catch (e) {}
                window.__WX_UI_READY__ = false;
                wxStatus('auth-no-parent');
                return false;
            }
        } catch (e) {
            wxStatus('auth-err:' + (e && e.message));
            log('auth ui: ' + e);
            return false;
        }
    }

    /* v98-fix：卡密窗“不依赖定时器”的兜底重建。
     * 原来卡密窗只有三条唤醒路径：启动时 ready()、50ms 轮询、500ms setInterval + __WX_TICK__。
     * 本文件第 9/462 行自己就写明“引擎里 setInterval 不保证被调度”，所以一旦注入发生在引擎就绪前，
     * 而又没有任何一路驱动活下来，卡密窗就永远建不出来（但脚本仍然标记加载成功）。
     * 这个函数被引擎事件、dex tick、healFloat 以及加载器重注入四条路调用，任一路活着卡密窗就能出来。 */
    function authBump() {
        try {
            if (window.__WX_AUTH_OK__) { window.__WX_UI_READY__ = true; return true; }
            if (!ready()) { wxStatus('wait-engine'); window.__WX_UI_READY__ = false; return false; }
            if (authRoot && nodeMounted(authRoot)) {
                bringFloatToFront(authRoot, UI_LAYER.auth);
                window.__WX_UI_READY__ = true;
                wxStatus('auth-visible');
                return true;
            }
            var now = Date.now(), hasParent = false;
            try { hasParent = !!findCanvasNode(); } catch (eP) {}
            // v98-fix3：只有“连挂载点都还没有”时才按 700ms 限速（防加载器密集重注入猛刷）。
            // 场景里已经有 Canvas 时必须立刻重建 —— 否则重注入/场景事件送来的唯一一次机会会被限速吃掉，
            // 而这正是“时有时无”的根因之一（mock T1 复现过）。
            if (now - (window.__WX_AUTH_BUMP_T__ || 0) < (hasParent ? 200 : 700)) return false;
            window.__WX_AUTH_BUMP_T__ = now;
            try { if (authRoot) { authRoot.removeFromParent(false); authRoot.destroy(); } } catch (e0) {}
            authRoot = null; wxStatus('auth-bump'); makeAuth();
            var up = !!(authRoot && nodeMounted(authRoot));
            window.__WX_UI_READY__ = up;
            return up;
        } catch (e) { return false; }
    }
    window.__WX_AUTH_REVIVE__ = authBump;

    function setMsg(t) { try { authMsg && (authMsg.getComponent(cc.Label).string = t); } catch (e) {} log(t); }

    var __loginBusy = false;
    function isTransientLoginError(err) {
        return /HTTP\s*(0|408|425|429|5\d\d)|网络|超时|连接|中断|Failed to fetch/i.test(String(err || ''));
    }
    function doLogin() {
        var kami = String((window.__WX_KAMI_GET__ && window.__WX_KAMI_GET__()) || (authEdit && authEdit.string) || storageGet('kami', '') || '').trim();
        if (!kami) { setMsg('请先输入卡密'); return; }
        if (__loginBusy) { setMsg('网络正在重连，请稍候...'); return; }
        var sk = WX.appKey, sr = WX.rc4_2;
        var mark = androidId();
        if (!mark) { setMsg('拿不到设备码，请把状态文件里 mark 字段发我'); return; }
        var attempt = 0, maxAttempts = 4;
        __loginBusy = true;

        function retry(reason) {
            if (attempt >= maxAttempts) {
                __loginBusy = false;
                setMsg('卡密验证失败：' + reason);
                return;
            }
            var delay = [800, 1600, 3000][attempt - 1] || 3000;
            setMsg('网络已切换，正在重连（' + (attempt + 1) + '/' + maxAttempts + '）...');
            setTimeout(sendAttempt, delay);
        }

        function sendAttempt() {
            attempt++;
            setMsg(attempt === 1 ? '正在提交验证，请稍候...' : '正在重新验证，请稍候...');
            // 每次尝试都重新生成时间戳、签名和随机值，不能复用切网前已经过期的请求。
            var tm = Math.floor(Date.now() / 1000);
            var signs = hex_md5('kami=' + kami + '&markcode=' + mark + '&t=' + tm + '&' + sk);
            var request = WX.loginAPI + '&app=' + WX.appId + '&kami=' + encodeURIComponent(kami)
                        + '&markcode=' + encodeURIComponent(mark) + '&t=' + tm + '&sign=' + signs;
            var random = (Math.random().toString(16).slice(2) + Date.now().toString(16)) + sk + mark;
            var data = 'data=' + encodeURIComponent(rc4(request, sr, true)) + '&value=' + encodeURIComponent(random);
            // 微验这个应用的历史可用版本要求 POST 地址末尾再次携带 app；
            // request 本身仍必须保持不变，因为它同时参与 RC4 数据和 sign 校验。
            var postUrl = request + '&app=' + WX.appId;
            xhr('POST', postUrl, data, function (err, res) {
                if (err) {
                    if (isTransientLoginError(err)) { retry(err); return; }
                    __loginBusy = false; setMsg('卡密验证失败：' + err); return;
                }
                try {
                    res = rc4(res, sr);
                    var jo = JSON.parse(res);
                    var code = parseInt(jo.code, 10), check = String(jo.check || ''), timee = parseInt(jo.time || 0, 10);
                    var rawMsg = jo.msg;
                    var msg = typeof rawMsg === 'string' ? rawMsg :
                              (rawMsg && (rawMsg.message || rawMsg.msg || rawMsg.error)) ?
                              String(rawMsg.message || rawMsg.msg || rawMsg.error) : '';
                    try {
                        window.__WX_LAST_LOGIN__ = {
                            attempt: attempt,
                            code: isNaN(code) ? null : code,
                            hasTime: timee > 0,
                            hasCheck: !!check,
                            markSource: String((DEV && DEV.src) || '')
                        };
                    } catch (eDiag) {}

                    // 错误响应经常不带 time/check；先展示服务端原始原因，避免误报“非法操作”。
                    // 此分支绝不进入授权成功流程，因此不需要接受或信任它的签名字段。
                    if (code !== WX.okCode) {
                        msg = msg || '服务端拒绝请求（代码 ' + (isNaN(code) ? '未知' : code) + '）';
                        if (/网络|超时|连接|繁忙|稍后/.test(msg) && attempt < maxAttempts) { retry(msg); return; }
                        __loginBusy = false;
                        setMsg('登录失败：' + msg);
                        return;
                    }

                    // 该应用所用的微验服务返回 32 位 check，但当前服务端算法与旧公开公式不同。
                    // 原生 DEX 的实际判定也是：check 等于旧公式时视为非法；不等时继续验证成功码和时间窗。
                    if (!(timee > 0) || !/^[0-9a-fA-F]{32}$/.test(check)) {
                        __loginBusy = false;
                        setMsg('登录失败：服务器响应签名缺失');
                        return;
                    }
                    if (timee - tm > 30 || timee - tm < -30) {
                        if (attempt < maxAttempts) { retry('数据已过期'); return; }
                        __loginBusy = false; setMsg('登录失败：数据已过期'); return;
                    }
                    if (check === hex_md5(String(timee) + sk + WX.appId)) {
                        __loginBusy = false;
                        setMsg('登录失败：服务器响应签名校验失败');
                        return;
                    }
                    if (code === WX.okCode) {
                        __loginBusy = false;
                        var loginMsg = (jo.msg && typeof jo.msg === 'object') ? jo.msg : {};
                        var vip = parseInt(loginMsg.vip || 0);
                        var exp = fmtDate(vip);
                        storageSet('kami', kami);
                        setCardAccess(loginMsg, vip);
                        window.__WX_AUTH_OK__ = true;
                        wxStatus('auth-ok');
                        window.__WX_AUTH_EXPIRE__ = exp;
                        setMsg('登录成功，到期时间：' + exp);
                        startHeartbeat();
                        startFeatures();
                        // 衔接：先建面板再收卡密窗，中间不留空档（原来是先等 800ms 再建，会闪一下桌面）
                        UI.open = false;                // 登录成功先出悬浮球（面板收着，点球再打开）
                        UI.min = true;
                        var built = false;
                        try { built = buildMenu(); } catch (e) {}
                        try { if (UI.ball) UI.ball.active = !UI.open; } catch (e) {}
                        try { if (UI.panel) UI.panel.active = !!UI.open; } catch (e) {}
                        try { if (UI.root) bringFloatToFront(UI.root, UI_LAYER.ball); } catch (e) {}
                        try { authRoot.active = false; authRoot.destroy(); authRoot = null; } catch (e) {}
                        wxStatus('panel');
                    }
                } catch (e) {
                    if (attempt < maxAttempts) { retry((e && e.message) || '响应解析失败'); return; }
                    __loginBusy = false; setMsg('卡密验证失败：' + (e && e.message));
                }
            });
        }
        sendAttempt();
    }

    function startHeartbeat() {
        // 微验这套只要求校验服务器时间差，这里按参考文件的做法对时
        setInterval(function () {
            xhr('GET', 'https://vv.video.qq.com/checktime?otype=json', null, function (err, res) {
                if (err || !res) return;
                try {
                    var m = String(res).match(/"t":\s*(\d+)/);
                    if (m) window.__WX_SERVER_T__ = parseInt(m[1], 10);
                } catch (e) {}
            });
        }, 300000);
    }

    function noticeTextOf(msg) {
        if (typeof msg === 'string' || typeof msg === 'number') return String(msg);
        if (!msg || typeof msg !== 'object') return '';
        var keys = ['app_gg', 'notice', 'content', 'message', 'text'];
        for (var i = 0; i < keys.length; i++) {
            var value = msg[keys[i]];
            if (typeof value === 'string' || typeof value === 'number') {
                value = String(value).replace(/^\s+|\s+$/g, '');
                if (value) return value;
            }
        }
        return '';
    }

    function currentNoticeText() {
        var text = '';
        try { text = String(window.__WX_NOTICE_TEXT__ || ''); } catch (e) {}
        if (!text) text = String(storageGet('__WX_NOTICE_CACHE__', '') || '');
        return text || '公告加载中…';
    }

    function refreshNoticePage() {
        try {
            if (!UI.noticeLabel || !nodeOk(UI.noticeLabel)) return;
            var label = UI.noticeLabel.getComponent(cc.Label);
            if (label) label.string = currentNoticeText();
        } catch (e) {}
    }

    function notice() {
        var now = Date.now();
        try {
            if (now - (window.__WX_NOTICE_FETCH_T__ || 0) < 15000) return;
            window.__WX_NOTICE_FETCH_T__ = now;
        } catch (e) {}
        xhr('GET', WX.noticeAPI + '&app=' + WX.appId, null, function (err, res) {
            if (err || !res) return;
            try {
                var jo = JSON.parse(rc4(res, WX.rc4_2));
                if (jo && parseInt(jo.code) === 200 && jo.msg) {
                    var text = noticeTextOf(jo.msg);
                    if (!text) return;
                    window.__WX_NOTICE_TEXT__ = text;
                    storageSet('__WX_NOTICE_CACHE__', text);
                    refreshNoticePage();
                    log('公告：' + text);
                }
            } catch (e) {}
        });
    }

    /* ================= 3. 悬浮窗菜单（纯 Cocos 绘制）
     * 外观照弹壳战机面板：深绿黑底 + 切角矩形 + 细亮描边 + #00E676 亮绿，
     * 功能块左侧一条硬边（开=亮绿/关=压暗），开关是胶囊+白点。
     * 结构：圆球 → 面板（标题区 + 遮罩视口 + 功能行）。
     * ================= */
    var UI = { root: null, panel: null, view: null, content: null, autoPage: null, autoContent: null, noticePage: null, noticeLabel: null, gateToast: null, ball: null, open: false };
    function floatState() { return { x: parseFloat(storageGet('__WX_X__', '0')) || 0, y: parseFloat(storageGet('__WX_Y__', '0')) || 0 }; }
    function saveFloat(x, y) { storageSet('__WX_X__', x); storageSet('__WX_Y__', y); }
    function clamp(v, a, b) { return a > b ? (a + b) / 2 : Math.max(a, Math.min(b, v)); }
    /* v66 修正：原式 `!n.isValid || n.isValid !== false` 在 isValid===false（节点已销毁）时
     * 因为 !false === true 而返回 true —— 于是 buildMenu() 看到"面板还活着"直接 return true，
     * 销毁过的球/面板永远不会重建，表现就是"悬浮窗打不开/点了没反应"。
     * 正确语义只有一条：isValid 不是 false 才算活着。 */
    function nodeOk(n) { try { return !!n && n.isValid !== false; } catch (e) { return !!n; } }

    function showPanelToast(text) {
        text = String(text || '');
        try {
            if (UI.gateToast && nodeOk(UI.gateToast)) {
                try { UI.gateToast.removeFromParent(false); UI.gateToast.destroy(); } catch (e0) {}
            }
            if (!UI.panel || !nodeOk(UI.panel)) throw new Error('panel unavailable');
            var w = Math.max(240, Math.min(390, Math.round(((UI.geo && UI.geo.W) || 600) * 0.58)));
            var h = 52;
            var toast = new cc.Node('wx_permission_toast');
            toast.setContentSize(w, h);
            toast.x = (UI.geo && UI.geo.viewX) || 45;
            toast.y = 0;
            var bg = toast.addComponent(cc.Graphics);
            bg.fillColor = new cc.Color(30, 25, 5, 245);
            mkCut(bg, -w / 2, -h / 2, w, h, 7); bg.fill();
            bg.strokeColor = new cc.Color(255, 210, 45, 255); bg.lineWidth = 2;
            mkCut(bg, -w / 2, -h / 2, w, h, 7); bg.stroke();
            toast.addChild(mkLabel(text, 22, w - 16, h, new cc.Color(255, 220, 70, 255)), 2);
            UI.panel.addChild(toast, 100);
            UI.gateToast = toast;
            setTimeout(function () {
                try {
                    if (UI.gateToast === toast) UI.gateToast = null;
                    if (nodeOk(toast)) { toast.removeFromParent(false); toast.destroy(); }
                } catch (e1) {}
            }, 1800);
            return;
        } catch (e) {}
        try {
            if (typeof $eZ !== 'undefined' && $eZ.fguiMgr && typeof $eZ.fguiMgr.cTg === 'function') $eZ.fguiMgr.cTg(text);
        } catch (e2) {}
    }

    var AutoBridge = (function () {
        var items = [
            ['自动主线', '自动执行主线任务与任务内动作', '__WX_AUTO_TASK__', 'auto'],
            ['自动翻牌', '结算时自动完成免费及金币翻牌', '__WX_AUTO_FLIP__', 'flag'],
            ['自动重复挑战', '结算后自动再次挑战当前关卡', '__WX_AUTO_REPEAT__', 'flag'],
            ['自动复活', '战斗失败时使用自动复活', '__WX_AUTO_REVIVE__', 'flag'],
            ['自动出售', '回城后自动出售选定品质装备', '__WX_AUTO_SELL__', 'flag'],
            ['自动分解', '回城后自动分解选定品质装备', '__WX_AUTO_DECOMPOSE__', 'flag'],
            ['自动穿戴', '回城后自动换上更优装备', '__WX_AUTO_WEAR__', 'flag'],
            ['自动升级技能', '回城后自动升级可提升技能', '__WX_AUTO_SKILL__', 'flag'],
            ['自动转职', '任务需要转职时自动选择已解锁职业', '__WX_AUTO_JOBSWITCH__', 'flag']
        ];

        function enabled(key) { return isPermanentCard() && storageGet(key, '0') === '1'; }
        function execute(command, payload) {
            try {
                if (typeof window.__REMOTE_EXECUTE__ === 'function') return window.__REMOTE_EXECUTE__(command, payload || {});
                var commands = window.__REMOTE_COMMANDS__;
                if (commands && typeof commands[command] === 'function') return commands[command](payload || {});
            } catch (e) { featureError('autoBridge', e); }
            return null;
        }
        function set(key, on, kind) {
            on = !!on;
            if (on && !isPermanentCard()) {
                disableAll();
                showPanelToast('只能永久卡可使用');
                return false;
            }
            storageSet(key, on ? '1' : '0');
            try { window[key] = on; } catch (e) {}
            // remote.js 的两个历史键在不同路径被读取；同步写入，避免自动转职显示开启但任务侧没读到。
            if (key === '__WX_AUTO_JOBSWITCH__') storageSet('__WX_AUTO_JOB__', on ? '1' : '0');
            if (kind === 'auto') {
                // 自动主线进副本后复用本文件的新版自动战斗模块，不加载 remote.js 的旧战斗 Hook。
                if (on) {
                    try { CFG.autoFight = true; save(); unlockAuto(true); } catch (e1) {}
                }
                var result = execute('wx.auto', { value: on, checked: on });
                if (!result) {
                    try {
                        var state = window.__WX_AUTO_OBJ__;
                        if (state) { state.enabled = on; state.state = 'IDLE'; }
                    } catch (e2) {}
                }
            } else execute('wx.flag', { key: key, value: on });
            return true;
        }
        function disableAll() {
            for (var i = 0; i < items.length; i++) {
                var key = items[i][2];
                storageSet(key, '0');
                try { window[key] = false; } catch (e0) {}
            }
            storageSet('__WX_AUTO_JOB__', '0');
            try { window.__WX_AUTO_JOB__ = false; window.__WX_AUTO_ENABLED__ = false; } catch (e1) {}
            try {
                var state = window.__WX_AUTO_OBJ__;
                if (state) { state.enabled = false; state.state = 'IDLE'; }
            } catch (e2) {}
        }
        return { items: items, enabled: enabled, set: set, disableAll: disableAll, permitted: isPermanentCard };
    })();

    function bindVerticalPageScroll(view, content, contentH, viewH, viewW, stateKey) {
        var maxScroll = Math.max(0, contentH - viewH), scroll = 0;
        var bar = new cc.Node('wx_' + stateKey + '_scrollbar');
        var barW = Math.max(14, Math.round(viewW * 0.026)), barH = Math.max(40, viewH - 12);
        bar.setContentSize(barW, barH); bar.x = viewW / 2 - barW / 2 - 2; bar.y = 0;
        view.addChild(bar, 20);
        var g = bar.addComponent(cc.Graphics);
        var thumbH = maxScroll > 0 ? Math.max(28, Math.round(barH * viewH / contentH)) : barH;
        var travel = Math.max(0, barH - thumbH);

        function apply() {
            content.y = viewH / 2 + scroll;
            g.clear();
            if (maxScroll <= 0) { bar.active = false; return; }
            bar.active = true;
            g.fillColor = col(TH.trackOff, 150); mkRound(g, -2, -barH / 2, 4, barH, 2); g.fill();
            var y = travel / 2 - (scroll / maxScroll) * travel;
            g.fillColor = accentC(235);
            mkRound(g, -barW / 2 + 2, y - thumbH / 2, barW - 4, thumbH, Math.max(2, (barW - 4) / 2)); g.fill();
            try { window['__WX_SCROLL_' + stateKey.toUpperCase() + '__'] = [Math.round(scroll), Math.round(maxScroll), Math.round(contentH), Math.round(viewH)]; } catch (e) {}
        }
        function setScroll(value) { scroll = clamp(value, 0, maxScroll); apply(); }
        function hitsHorizontalSlider(e) {
            var n = e && e.target, depth = 0;
            while (n && n !== view && depth++ < 12) {
                if (n.__wxHorizontalSlider) return true;
                n = n.parent;
            }
            return false;
        }
        var dragging = false, moved = false, startX = 0, startY = 0, startScroll = 0;
        view.on(cc.Node.EventType.TOUCH_START, function (e) {
            if (maxScroll <= 0 || hitsHorizontalSlider(e)) return;
            var p = e.getLocation();
            dragging = true; moved = false; startX = p.x; startY = p.y; startScroll = scroll;
        });
        view.on(cc.Node.EventType.TOUCH_MOVE, function (e) {
            if (!dragging) return;
            var p = e.getLocation(), dx = p.x - startX, dy = p.y - startY;
            if (!moved) {
                if (Math.abs(dy) < 6) return;
                if (Math.abs(dx) > Math.abs(dy)) { dragging = false; return; }
                moved = true;
            }
            setScroll(startScroll + dy); // 手指向上浏览下面内容，方向与上一版修正保持一致。
            e.stopPropagation && e.stopPropagation();
        });
        view.on(cc.Node.EventType.TOUCH_END, function (e) {
            if (moved) e.stopPropagation && e.stopPropagation();
            dragging = false; moved = false;
        });
        view.on(cc.Node.EventType.TOUCH_CANCEL, function () { dragging = false; moved = false; });

        function setByBar(e) {
            if (maxScroll <= 0 || travel <= 0) return;
            try {
                var w = e.getLocation();
                var p = bar.convertToNodeSpaceAR ? bar.convertToNodeSpaceAR(cc.v2(w.x, w.y)) : w;
                setScroll(clamp((travel / 2 - p.y) / travel, 0, 1) * maxScroll);
            } catch (er) {}
            e.stopPropagation && e.stopPropagation();
        }
        bar.on(cc.Node.EventType.TOUCH_START, setByBar);
        bar.on(cc.Node.EventType.TOUCH_MOVE, setByBar);
        bar.on(cc.Node.EventType.TOUCH_END, function (e) { e.stopPropagation && e.stopPropagation(); });
        bar.on(cc.Node.EventType.TOUCH_CANCEL, function (e) { e.stopPropagation && e.stopPropagation(); });
        apply();
        return { reset: function () { setScroll(0); }, scrollBy: function (d) { setScroll(scroll + d); }, max: maxScroll };
    }

    /* v98-fix：卡密窗原来没有场景钩子（watchScene 只在 buildMenu 里调，而 buildMenu 又要求已通过验证），
     * 于是“加载场景 → 主场景”切换时窗口一旦被销毁就只能等定时器。这里补一个事件驱动的重建入口。 */
    function watchSceneAuth() {
        try {
            if (typeof cc === 'undefined' || !cc || !cc.director || !cc.Director) return false;
            if (cc.director.__wxAuthSceneHooked) return true;
            var ev = cc.Director.EVENT_AFTER_SCENE_LAUNCH || cc.Director.EVENT_AFTER_SCENE_SWITCH;
            if (!ev || typeof cc.director.on !== 'function') return false;
            cc.director.on(ev, function () {
                try { if (!window.__WX_AUTH_OK__) authBump(); } catch (e1) {}
                // 新场景首帧 Canvas 还没建好时上面的调用会失败，引擎调度器/下一帧再补一次
                try { if (!window.__WX_AUTH_OK__) setTimeout(function () { try { authBump(); } catch (e2) {} }, 900); } catch (e3) {}
            });
            cc.director.__wxAuthSceneHooked = true;
            return true;
        } catch (e) { return false; }
    }

    function watchScene() {
        try {
            if (!cc || !cc.director || cc.director.__wxSceneHooked) return;
            cc.director.__wxSceneHooked = true;
            cc.director.on(cc.Director.EVENT_AFTER_SCENE_LAUNCH, function () {
                // 新场景首帧仍在创建 Canvas/Graphics，立即重建会在部分 GPU 上造成黑闪。
                HEAL.sceneAt = Date.now(); HEAL.miss = 0;
                try { setTimeout(function () { try { if (!UI.root || !nodeMounted(UI.root)) buildMenu(); } catch (e) {} }, 1600); } catch (e) {}
            });
        } catch (e) {}
    }

    /* 状态标记 / 卡密窗看门狗 */
    function wxStatus(t) { try { if (t) window.__WX_UI_STATUS__ = t; } catch (e) {} }
    var __authWbLast = 0, __authWbN = 0;
    function wxAuthWatchdog() {
        try {
            if (window.__WX_AUTH_OK__) return;
            if (!ready()) return;
            if (authRoot && nodeMounted(authRoot)) {
                bringFloatToFront(authRoot, UI_LAYER.auth);
                window.__WX_UI_READY__ = true;
                wxStatus('auth-visible');
                return;
            }
            if (!authRoot || !nodeMounted(authRoot)) {
                // 鸿蒙上首帧场景可能还没就绪，建窗会失败：限速重试，别 500ms 一次猛刷
                var now = Date.now();
                if (now - __authWbLast < 1200) return;
                __authWbLast = now; __authWbN++;
                window.__WX_AUTH_REBUILDS__ = __authWbN;
                try { if (authRoot) { authRoot.removeFromParent(false); authRoot.destroy(); } } catch (e) {}
                authRoot = null; wxStatus('auth-rebuild'); makeAuth();
            }
        } catch (e) {}
    }


    /* 面板自己的位置（和悬浮球互不影响） */
    function panelState() {
        var x = storageGet('__WX_PX2__', ''), y = storageGet('__WX_PY2__', '');
        return { x: x === '' ? 0 : (parseFloat(x) || 0), y: y === '' ? 0 : (parseFloat(y) || 0) };
    }
    function savePanelState(x, y) { storageSet('__WX_PX2__', x); storageSet('__WX_PY2__', y); }
    function clampPanelRoot() {
        try {
            if (!UI.proot) return;
            var W = (UI.geo && UI.geo.W) || 620, H = (UI.geo && UI.geo.H) || 500;
            var _s = safeRect();
            // 允许拖到只露 90 个单位（保证任何一行都能拖进屏幕），但不许整个跑出安全区
            UI.proot.x = clamp(UI.proot.x, _s.x0 - W / 2 + 90, _s.x1 + W / 2 - 90);
            UI.proot.y = clamp(UI.proot.y, _s.y0 - H / 2 + 90, _s.y1 + H / 2 - 90);
            savePanelState(UI.proot.x, UI.proot.y);
        } catch (e) {}
    }
    function clampPanelForOpen() {
        try {
            if (!UI.proot) return;
            var W = (UI.geo && UI.geo.W) || 600, H = (UI.geo && UI.geo.H) || 650;
            // 打开时完整拉回安全区；面板大于安全区的轴由 clampToSafe 自动居中。
            var p = clampToSafe(UI.proot.x, UI.proot.y, W / 2, H / 2);
            UI.proot.x = p.x; UI.proot.y = p.y;
            savePanelState(p.x, p.y);
        } catch (e) {}
    }

    function buildMenu() {
        try {
            if (!window.__WX_AUTH_OK__) return false;      // 卡密没过：不出面板
            if (UI.root && nodeMounted(UI.root) && UI.proot && nodeMounted(UI.proot)
                && UI.panel && nodeOk(UI.panel) && UI.panel.parent
                && UI.ball && nodeOk(UI.ball) && UI.ball.parent) return true;
            if (!ready()) return false;
            var scene = cc.director.getScene();
            if (!scene) return false;
            watchScene();

            // Root and panel root are siblings. Leaving either mounted across a rebuild
            // stacks Cocos Graphics nodes and can flash over the battle scene.
            var staleRoot = UI.root, stalePanelRoot = UI.proot;
            if (staleRoot && nodeMounted(staleRoot)) {
                try { staleRoot.removeFromParent(false); staleRoot.destroy(); } catch (e) {}
            }
            if (stalePanelRoot && nodeMounted(stalePanelRoot)) {
                try { stalePanelRoot.removeFromParent(false); stalePanelRoot.destroy(); } catch (e) {}
            }
            UI.root = null; UI.proot = null; UI.panel = null; UI.view = null; UI.content = null;
            UI.autoPage = null; UI.autoContent = null; UI.noticePage = null; UI.noticeLabel = null; UI.gateToast = null; UI.ball = null;

            refreshGeometry();                // v66：每次建面板都重算可见区/安全区
            var st = floatState();
            var root = new cc.Node('wx_float_root');
            root.x = st.x ? st.x : -330;      // 默认偏左（避开右上角原生悬浮窗）
            root.y = st.y ? st.y : -120;
            var _rp = clampToSafe(root.x, root.y, 48, 48);   // 挖孔/状态栏/手势条都不许压
            root.x = _rp.x; root.y = _rp.y;
            attachFloat(root, UI_LAYER.ball);
            if (!root.parent) { wxStatus('float-no-parent'); try { root.destroy(); } catch (e) {} return false; }
            UI.root = root;

            /* ---- 圆球 ---- */
            var ball = new cc.Node('wx_ball');
            ball.setContentSize(96, 96);
            var bg = ball.addComponent(cc.Graphics);
            // 天选悬浮球：深绿黑圆盘 + 亮绿圆环 + 亮绿字标（就是弹壳战机那颗球：
            // ringBg(0xF207120C, #00E676, 3.2) + text("TX", 16sp, #00E676)）
            bg.fillColor = col(TH.panel, 242); bg.circle(0, 0, 44); bg.fill();
            bg.strokeColor = accentC(); bg.lineWidth = 3; bg.circle(0, 0, 44); bg.stroke();
            var mark = mkLabel(WX.ballIcon, 28, 96, 96, accentC());
            try { mark.getComponent(cc.Label).letterSpacing = 2; } catch (e) {}
            ball.addChild(mark, 2);
            root.addChild(ball, 10);
            UI.ball = ball;

            /* ---- 面板 ---- */
            // 尺寸自适应：按屏幕可见区算，保证 5 行全部露出来（以前写死 620x440，小屏会把最后一行挤出屏幕）
            var vsz = (cc.view && cc.view.getVisibleSize) ? cc.view.getVisibleSize() : { width: 1280, height: 720 };
            // 大小照弹壳战机：它的面板是 340dp×380dp（宽:高 = 0.895），竖屏手机上
            // 高度几乎占满、宽度不到屏宽的一半 —— 也就是"贴着短边撑满、长边按比例"。
            // 这个游戏是**横屏**：短边是高度，所以
            //   高度 = 安全区高度的 96%（沿用刚才 refreshGeometry 算好的安全区，避开挖孔/手势条）
            //   宽度 = 高度 × 0.895（直接用它 340:380 的宽高比，而不是拿屏宽乘百分比）
            // 下限 390：6 行功能块 + 标题至少要这么多（列表滑动是关的，放不下就等于丢一行）。
            var sr = safeRect();
            var safeH = (sr && sr.y1 > sr.y0) ? Math.round(sr.y1 - sr.y0) : vsz.height;
            var H = Math.min(Math.round(safeH * 0.96), Math.round(vsz.height - 16));
            if (H < 390) H = Math.min(390, Math.round(vsz.height - 16));
            var W = Math.min(Math.round(H * 0.895), Math.round(vsz.width - 16));
            var headerH = Math.round(Math.min(84, Math.max(44, H * 0.12)));
            var railW = Math.round(W * 0.17);                            // 左侧页签栏（弹壳战机是 58/340）
            var insetX = Math.round(W * 0.029);                          // 页签栏左边距
            var padR = Math.round(W * 0.03);                             // 内容区右边距
            var viewW = W - railW - insetX - padR;                       // 内容视口：页签栏右侧到右边距
            var viewX = -W / 2 + railW + insetX + viewW / 2;             // 视口中心（整体右移，给页签栏让位）
            var viewH = H - headerH - Math.round(H * 0.06);
            var ROW_COUNT = 6;                                                       // 一屏显示 6 行；多出的功能通过右侧纵向滑动条浏览
            var TOP_PAD = 6;                                               // 第一行离顶部留白（英雄攻击的位置，保持不动）
            var gap = 14;                                                  // ★ 行间间隙：7 个间隔全部用这个值
            var rowH = Math.max(40, Math.floor((viewH - TOP_PAD - (ROW_COUNT - 1) * gap) / ROW_COUNT));   // ★ 卡高按行数自适应
            var spacing = rowH + gap;                                       // 每行占位 = 卡高 + 间隙
            var sx = viewW / 520;
            var sy = rowH / 60;
            UI.geo = { W: W, H: H, viewW: viewW, viewH: viewH, viewX: viewX, railW: railW,
                       spacing: spacing, rowH: rowH, sx: sx, sy: sy, headerH: headerH, topPad: TOP_PAD };
            var panel = new cc.Node('wx_panel');
            panel.setContentSize(W, H);
            panel.active = false;
            var pg = panel.addComponent(cc.Graphics);
            // 面板：深绿黑底 + 切角 + 细亮描边（弹壳战机的面板底色/描边/切角值）
            pg.fillColor = col(TH.panel); mkCut(pg, -W / 2, -H / 2, W, H, 14); pg.fill();
            pg.strokeColor = col(TH.panelBrd); pg.lineWidth = 2; mkCut(pg, -W / 2, -H / 2, W, H, 14); pg.stroke();
            pg.fillColor = accentC(); mkCut(pg, -W / 2 + 16, H / 2 - headerH / 2 - 9, 3, 18, 2); pg.fill();   // 标题前的亮绿硬边
            pg.fillColor = accentC(77); mkCut(pg, -W / 2 + 12, H / 2 - headerH, W - 24, 1, 0); pg.fill();   // 标题区分隔线
            // 面板挂在自己的根节点上（不再挂在悬浮球下面），这样"拖悬浮球 → 面板跟着动"就不会发生了
            var proot = new cc.Node('wx_panel_root');
            var pst = panelState();
            proot.x = pst.x; proot.y = pst.y;
            var _pp = clampToSafe(proot.x, proot.y, -W / 2 + 90, -H / 2 + 90);
            proot.x = _pp.x; proot.y = _pp.y;
            attachFloat(proot, UI_LAYER.panel);
            if (!proot.parent) {
                wxStatus('panel-no-parent');
                try { root.destroy(); } catch (e) {}
                try { proot.destroy(); } catch (e) {}
                UI.root = null; UI.ball = null;
                return false;
            }
            proot.addChild(panel, 0);
            panel.x = 0; panel.y = 0;
            UI.proot = proot;
            UI.panel = panel;

            // 顶部拖动条：按住标题栏拖 → 只挪面板，不动悬浮球
            (function () {
                var handle = new cc.Node('wx_panel_drag');
                handle.setContentSize(W, headerH + 8);   // 整条标题栏都能拖（再多给 8 的余量）
                handle.y = H / 2 - headerH / 2;
                panel.addChild(handle, 5);
                var down = false, lx = 0, ly = 0;
                handle.on(cc.Node.EventType.TOUCH_START, function (e) {
                    down = true; var p = e.getLocation(); lx = p.x; ly = p.y;
                    e.stopPropagation && e.stopPropagation();
                });
                handle.on(cc.Node.EventType.TOUCH_MOVE, function (e) {
                    if (!down || !UI.proot) return;
                    var p = e.getLocation();
                    UI.proot.x += (p.x - lx); UI.proot.y += (p.y - ly);
                    lx = p.x; ly = p.y;
                    clampPanelRoot();
                    e.stopPropagation && e.stopPropagation();
                });
                handle.on(cc.Node.EventType.TOUCH_END, function () { down = false; clampPanelRoot(); savePanelState(UI.proot.x, UI.proot.y); });
                handle.on(cc.Node.EventType.TOUCH_CANCEL, function () { down = false; });
            })();

            // 标题：左对齐贴着亮绿硬边（弹壳战机的标题区形态）
            var title = mkLabel(WX.title, 30, W - 180, 44, col(TH.title));
            try { title.getComponent(cc.Label).horizontalAlign = cc.Label.HorizontalAlign.LEFT; } catch (e) {}
            // 标题文字本身也能拖着走
            (function () {
                var d = false, lx = 0, ly = 0;
                title.on(cc.Node.EventType.TOUCH_START, function (e) { d = true; var p = e.getLocation(); lx = p.x; ly = p.y; e.stopPropagation && e.stopPropagation(); });
                title.on(cc.Node.EventType.TOUCH_MOVE, function (e) {
                    if (!d || !UI.proot) return;
                    var p = e.getLocation();
                    UI.proot.x += (p.x - lx); UI.proot.y += (p.y - ly); lx = p.x; ly = p.y;
                    clampPanelRoot();
                    e.stopPropagation && e.stopPropagation();
                });
                title.on(cc.Node.EventType.TOUCH_END, function () { d = false; if (UI.proot) savePanelState(UI.proot.x, UI.proot.y); });
                title.on(cc.Node.EventType.TOUCH_CANCEL, function () { d = false; });
            })();
            
            title.y = H / 2 - headerH / 2; panel.addChild(title, 6);   // 标题在标题区里居中（跟着 headerH 走）
            title.x = -(W / 2) + 27 + (W - 180) / 2;      // 左边距 27：紧贴标题前的亮绿硬边

            // 左侧页签栏：公告在功能上方；两页复用同一个内容区，不重复创建功能节点。
            var bodyTop = H / 2 - headerH, bodyBot = -H / 2 + Math.round(H * 0.045);
            var bodyH = bodyTop - bodyBot;
            var tabH = Math.round(Math.max(26, Math.min(64, bodyH * 0.117)));
            var tabGap = Math.round(Math.max(6, bodyH * 0.027));
            var tabW = railW - insetX - Math.round(W * 0.014);
            var tabX = -W / 2 + insetX + tabW / 2;
            var tabY = bodyTop - Math.round(bodyH * 0.04) - tabH / 2;
            var noticeTab = null, featureTab = null, autoTab = null;

            
            /* ---- 滚动列表：view(遮罩) + content(内容) + 滚动条 ---- */
            var viewW = UI.geo.viewW, viewH = UI.geo.viewH;
            var view = new cc.Node('wx_view');
            view.setContentSize(viewW, viewH);
            view.x = UI.geo.viewX;                                                          // 右移，给左侧页签栏让位
            view.y = (H / 2 - headerH) - viewH / 2 - Math.min(30, Math.round(H * 0.06));   // 视口位置复原
            try { var mask = view.addComponent(cc.Mask); mask.type = cc.Mask.Type.RECT; } catch (e) {}
            panel.addChild(view, 4);
            UI.view = view;


            var content = new cc.Node('wx_content');
            content.setAnchorPoint(0.5, 1);        // 顶部对齐：第一行就贴在视口顶部
            view.addChild(content, 4);
            UI.content = content;
            var y = -(Math.round(UI.geo.rowH / 2) + (UI.geo.topPad || 6));   // 第一行位置固定
            y = addRowInput(content, y, '英雄攻击', '输入数值', CFG.value,
                function (v) { CFG.value = v; CFG.on = true; save(); try { FeatureManager.install(true); } catch (e) {} },
                function (on) { CFG.on = on; save(); try { FeatureManager.install(true); } catch (e) {} });
            y = addRowSwitch(content, y, '英雄锁血', '开启后英雄不会受到伤害', function (on) { CFG.lockHp = on; save(); try { FeatureManager.install(true); } catch (e) {} });
            y = addRowSwitch(content, y, '锁定蓝量', '开启后英雄蓝量保持满值', function (on) { CFG.lockMp = on; save(); try { LockManaFeature.setEnabled(on); FeatureManager.install(true); } catch (e) {} });
            y = addRowSwitch(content, y, '技能无CD', '开启后技能冷却为0，可连续释放', function (on) { CFG.noCd = on; save(); try { FeatureManager.install(true); } catch (e) {} });
            y = addRowSwitch(content, y, '解锁金卡与自动战斗', '开启后获得金卡权限并解锁自动战斗', function (on) { CFG.autoFight = on; save(); unlockAuto(true); });
            y = addRowSlider(content, y, '人物移速', '拖动调节移动速度', CFG.speed, function (v) {
                CFG.speed = v; save();
                hookBattleSpeed();
            });
            y = addRowSlider(content, y, '图内全局加速', '最高 10 倍，整局一起加速', CFG.gspeed, function (v) {
                CFG.gspeed = v; save(); applyGlobalSpeed();
            }, 10.0);

            var autoPage = new cc.Node('wx_auto_view');
            autoPage.setContentSize(viewW, viewH);
            autoPage.x = UI.geo.viewX;
            autoPage.y = view.y;
            autoPage.active = false;
            try { var autoMask = autoPage.addComponent(cc.Mask); autoMask.type = cc.Mask.Type.RECT; } catch (e) {}
            panel.addChild(autoPage, 4);
            UI.autoPage = autoPage;

            var autoContent = new cc.Node('wx_auto_content');
            autoContent.setAnchorPoint(0.5, 1);
            autoPage.addChild(autoContent, 4);
            UI.autoContent = autoContent;
            var autoHeadH = Math.max(32, Math.round(UI.geo.rowH * 0.58));
            var autoHead = mkLabel('自动功能', Math.max(15, Math.round(autoHeadH * 0.48)), Math.round(viewW * 0.42), autoHeadH, col(TH.title));
            autoHead.x = -Math.round(viewW * 0.25); autoHead.y = -autoHeadH / 2;
            try { autoHead.getComponent(cc.Label).horizontalAlign = cc.Label.HorizontalAlign.LEFT; } catch (e) {}
            autoContent.addChild(autoHead, 5);
            var autoBadge = mkLabel('永久卡可使用', Math.max(14, Math.round(autoHeadH * 0.44)), Math.round(viewW * 0.46), autoHeadH, new cc.Color(255, 210, 45, 255));
            autoBadge.x = Math.round(viewW * 0.23); autoBadge.y = -autoHeadH / 2;
            autoContent.addChild(autoBadge, 5);
            var autoY = -(autoHeadH + Math.round(UI.geo.rowH / 2) + (UI.geo.topPad || 6));
            for (var ai = 0; ai < AutoBridge.items.length; ai++) {
                (function (item) {
                    autoY = addBoundSwitch(autoContent, autoY, item[0], item[1], AutoBridge.enabled(item[2]), function (on) {
                        return AutoBridge.set(item[2], on, item[3]);
                    });
                })(AutoBridge.items[ai]);
            }

            var noticePage = new cc.Node('wx_notice_page');
            noticePage.setContentSize(viewW, viewH);
            noticePage.x = UI.geo.viewX;
            noticePage.y = view.y;
            noticePage.active = false;
            panel.addChild(noticePage, 4);
            UI.noticePage = noticePage;

            var noticeCard = new cc.Node('wx_notice_card');
            var noticeW = Math.max(120, viewW - 8), noticeH = Math.max(80, viewH - 8);
            noticeCard.setContentSize(noticeW, noticeH);
            noticePage.addChild(noticeCard, 2);
            var noticeBg = noticeCard.addComponent(cc.Graphics);
            noticeBg.fillColor = col(TH.card);
            mkCut(noticeBg, -noticeW / 2, -noticeH / 2, noticeW, noticeH, 8); noticeBg.fill();
            noticeBg.strokeColor = col(TH.cardBrd); noticeBg.lineWidth = 1;
            mkCut(noticeBg, -noticeW / 2, -noticeH / 2, noticeW, noticeH, 8); noticeBg.stroke();
            noticeBg.fillColor = accentC();
            mkCut(noticeBg, -noticeW / 2 + 10, noticeH / 2 - 30, 3, 20, 1); noticeBg.fill();

            var noticeTitle = mkLabel('公告', Math.max(17, Math.min(26, Math.round(noticeH * 0.075))), noticeW - 38, 38, col(TH.title));
            try { noticeTitle.getComponent(cc.Label).horizontalAlign = cc.Label.HorizontalAlign.LEFT; } catch (e) {}
            noticeTitle.x = 8; noticeTitle.y = noticeH / 2 - 24;
            noticeCard.addChild(noticeTitle, 3);

            var noticeViewport = new cc.Node('wx_notice_view');
            var noticeViewW = noticeW - 28, noticeViewH = noticeH - 66;
            noticeViewport.setContentSize(noticeViewW, noticeViewH);
            noticeViewport.y = -17;
            try { var noticeMask = noticeViewport.addComponent(cc.Mask); noticeMask.type = cc.Mask.Type.RECT; } catch (e) {}
            noticeCard.addChild(noticeViewport, 3);

            var noticeBody = mkLabel(currentNoticeText(), Math.max(13, Math.min(20, Math.round(noticeH * 0.052))), noticeViewW - 16, noticeViewH, col(TH.name));
            noticeBody.setAnchorPoint(0.5, 1);
            try {
                var noticeComponent = noticeBody.getComponent(cc.Label);
                noticeComponent.horizontalAlign = cc.Label.HorizontalAlign.LEFT;
                noticeComponent.verticalAlign = cc.Label.VerticalAlign.TOP;
                noticeComponent.overflow = cc.Label.Overflow.CLAMP;
                noticeComponent.enableWrapText = true;
                noticeComponent.lineHeight = Math.round(noticeComponent.fontSize * 1.45);
            } catch (e) {}
            noticeViewport.addChild(noticeBody, 3);
            UI.noticeLabel = noticeBody;
            noticeBody.y = noticeViewH / 2;

            function showPage(name) {
                var isNotice = name === 'notice';
                var isFeature = name === 'feature';
                var isAuto = name === 'auto';
                view.active = isFeature;
                autoPage.active = isAuto;
                noticePage.active = isNotice;
                if (noticeTab && noticeTab.__setActive) noticeTab.__setActive(isNotice);
                if (featureTab && featureTab.__setActive) featureTab.__setActive(isFeature);
                if (autoTab && autoTab.__setActive) autoTab.__setActive(isAuto);
                if (isNotice) { refreshNoticePage(); notice(); }
            }
            noticeTab = railTab(panel, tabX, tabY, tabW, tabH, '公告', false, function () { showPage('notice'); });
            featureTab = railTab(panel, tabX, tabY - tabH - tabGap, tabW, tabH, '功能', true, function () { showPage('feature'); });
            autoTab = railTab(panel, tabX, tabY - (tabH + tabGap) * 2, tabW, tabH, '自动', false, function () { showPage('auto'); });
            railTab(panel, tabX, tabY - (tabH + tabGap) * 3, tabW, tabH, '隐藏', false, function () {
                panel.active = false; UI.open = false; ball.active = true;
                bringFloatToFront(root, UI_LAYER.ball);
            });

            // 内容高度 = 行数 × 固定行距（装不下才滚）
            var contentH = Math.max(viewH, (-y) + Math.round(UI.geo.spacing) - Math.round(UI.geo.rowH / 2) - UI.geo.spacing);
            content.setContentSize(viewW, contentH);
            var featureScroll = bindVerticalPageScroll(view, content, contentH, viewH, viewW, 'feature');
            window.__WX_SCROLL_BY__ = featureScroll.scrollBy;

            var autoContentH = Math.max(viewH, (-autoY) + Math.round(UI.geo.spacing) - Math.round(UI.geo.rowH / 2) - UI.geo.spacing);
            autoContent.setContentSize(viewW, autoContentH);
            bindVerticalPageScroll(autoPage, autoContent, autoContentH, viewH, viewW, 'auto');

            /* 拖动面板 / 点击圆球 */
            bindBall(ball, root, panel);
            layoutPanelIntoView(root, panel, W, H);
            // 子节点是在根节点挂到 Canvas 后才创建的，必须再次递归同步分组。
            syncFloatTree(root);
            syncFloatTree(proot);
            wxStatus('float-ready');
            return true;
        } catch (e) { log('menu: ' + e); return false; }
    }

    /** 面板整体保持在屏幕可见范围内（设计分辨率 1280x720 的三分之二以内） */
    function layoutPanelIntoView(root, panel, W, H) {
        try {
            refreshGeometry();
            var _s = safeRect();
            var bm = 60;                       // 悬浮球半宽 + 边距
            var minX = _s.x0 + bm, maxX = _s.x1 - bm;
            var minY = _s.y0 + bm, maxY = _s.y1 - bm;
            root.x = clamp(root.x, minX, maxX);
            root.y = clamp(root.y, minY, maxY);
            clampPanelRoot();
            saveFloat(root.x, root.y);
        } catch (e) {}
    }

    /* 开关尺寸：基准 80×40，按行高缩放并设下限；但横向不超过行宽的 16%
       —— 弹壳战机的开关是 44/282 = 15.6%，超过这个比例就会挤掉左边的文字区。 */
    function switchSize(row) {
        var sy = (UI.geo && UI.geo.sy) || 1;
        var w = Math.max(60, Math.round(80 * sy)), h = Math.max(30, Math.round(40 * sy));
        if (h > row.__rh * 0.86) { h = Math.round(row.__rh * 0.86); w = h * 2; }
        var mx = Math.round(row.__rw * 0.16);
        if (w > mx) { w = Math.max(48, mx); h = Math.round(w / 2); }
        return { w: w, h: h };
    }

    function addRowBase(parent, y, name, sub) {
        var g0 = UI.geo || { viewW: 520, rowH: 60, sx: 1, sy: 1 };
        var rw = Math.max(120, g0.viewW - 8), rh = g0.rowH, sy = g0.sy || 1, sx = g0.sx || 1;
        var row = new cc.Node('wx_row');
        row.setContentSize(rw, rh);
        row.y = y;
        parent.addChild(row, 3);
        var rg = row.addComponent(cc.Graphics);
        // 功能块：深绿黑底 + 切角 + 细描边（对齐弹壳战机的功能块）
        rg.fillColor = col(TH.card); mkCut(rg, -rw / 2, -rh / 2, rw, rh, 6); rg.fill();
        rg.strokeColor = col(TH.cardBrd); rg.lineWidth = 1; mkCut(rg, -rw / 2, -rh / 2, rw, rh, 6); rg.stroke();

        // 左侧硬边：关着压暗、开着亮绿 —— 弹壳战机功能块的标志
        var barH = rh - 16;
        var edge = new cc.Node('wx_edge');
        edge.setContentSize(3, barH);
        edge.x = -rw / 2 + 9;
        row.addChild(edge, 5);
        var eg = edge.addComponent(cc.Graphics);
        row.__edge = function (on) {
            eg.clear();
            eg.fillColor = accentC(on ? 255 : 89);
            mkCut(eg, -1.5, -barH / 2, 3, barH, 2);
            eg.fill();
        };
        row.__edge(false);

        var pad = Math.round(rw * 0.05);
        var textW = Math.round(rw * 0.44);                                  // 文字区（剩下给输入框+开关）
        // 字号按行高来（比之前大）：名字 14~22，说明 11~14；名字太长时只缩这一行，其它行保持大字
        var tfs = Math.max(14, Math.min(24, Math.round(rh * 0.34)));
        if (name.length * tfs > textW - 4) tfs = Math.max(12, Math.floor((textW - 4) / name.length));
        var sfs = Math.max(10, Math.min(15, Math.round(rh * 0.22)));
        if (sub.length * sfs > textW - 4) sfs = Math.max(9, Math.floor((textW - 4) / sub.length));
        // 左边：功能名（上）＋ 说明（下），都左对齐
        var t1 = mkLabel(name, tfs, textW, Math.round(rh * 0.44), col(TH.name));
        try { t1.getComponent(cc.Label).horizontalAlign = cc.Label.HorizontalAlign.LEFT; } catch (e) {}
        t1.x = -rw / 2 + pad + textW / 2; t1.y = rh * 0.20; row.addChild(t1, 4);
        var t2 = mkLabel(sub, sfs, textW, Math.round(rh * 0.36), subC(153));
        try { t2.getComponent(cc.Label).horizontalAlign = cc.Label.HorizontalAlign.LEFT; } catch (e) {}
        t2.x = -rw / 2 + pad + textW / 2; t2.y = -rh * 0.22; row.addChild(t2, 4);

        row.__rw = rw; row.__rh = rh; row.__sx = sx; row.__sy = sy; row.__pad = pad; row.__textW = textW;
        return row;
    }

    function addSwitch(parent, row, on, cb) {
        var sz = switchSize(row), swW = sz.w, swH = sz.h;
        var sw = new cc.Node('wx_sw');
        sw.setContentSize(swW, swH);
        var pd = row.__pad || 8, rw = row.__rw || 520;
        sw.x = rw / 2 - pd - swW / 2;                                  // 所有行的开关同一条竖线
        row.addChild(sw, 5);
        var g = sw.addComponent(cc.Graphics);
        // 开关比例照弹壳战机：胶囊轨道 + 白色圆点，圆点半径 = 半高 - 边距（边距按 2/24 缩放）
        var gap = Math.max(1, Math.round(swH * 0.083));
        var tr = swH / 2 - gap;
        var travel = swW / 2 - gap - tr;
        var state = { v: !!on };
        function paint() {
            g.clear();
            g.fillColor = state.v ? accentC() : col(TH.trackOff);
            mkRound(g, -swW / 2, -swH / 2, swW, swH, swH / 2); g.fill();
            g.fillColor = new cc.Color(255, 255, 255, 255);
            g.circle(state.v ? travel : -travel, 0, tr); g.fill();
            if (row.__edge) row.__edge(state.v);
        }
        paint();
        sw.on(cc.Node.EventType.TOUCH_START, function (e) { e.stopPropagation && e.stopPropagation(); });
        sw.on(cc.Node.EventType.TOUCH_END, function (e) {
            var next = !state.v;
            var accepted = cb ? cb(next) : undefined;
            if (accepted !== false) state.v = next;
            paint();
            e.stopPropagation && e.stopPropagation();
        });
        return sw;
    }

    function addRowSwitch(parent, y, name, sub, cb) {
        return addBoundSwitch(parent, y, name, sub, !!CFG[keyOf(name)], cb);
    }

    function addBoundSwitch(parent, y, name, sub, on, cb) {
        var row = addRowBase(parent, y, name, sub);
        addSwitch(parent, row, !!on, cb);
        return y - UI.geo.spacing;
    }

    function addRowInput(parent, y, name, sub, value, onInput, onSwitch) {
        var row = addRowBase(parent, y, name, sub);
        var rw = row.__rw, rh = row.__rh, pad = row.__pad, textW = row.__textW;
        var shown = { v: value > 0 ? value : 99999 };

        addSwitch(parent, row, !!CFG.on, onSwitch);
        var sz2 = switchSize(row), swW2 = sz2.w, swH2 = sz2.h;
        var swX = rw / 2 - pad - swW2 / 2;

        // 输入框：紧贴开关左边（"输入框在按钮前面"）
        var bw = Math.max(46, Math.round(rw * 0.24));
        var bh = Math.max(22, Math.round(rh * 0.52));
        var bx = swX - swW2 / 2 - Math.round(rw * 0.03) - bw / 2;   // 注意用 swW2（本函数内的），之前误写成 swW 直接抛 ReferenceError
        var box = new cc.Node('wx_input');
        box.setContentSize(bw, bh); box.x = bx;
        row.addChild(box, 6);
        var bg = box.addComponent(cc.Graphics);
        // 输入框：深底 + 切角 + 细描边（对齐弹壳战机的数字输入框）
        bg.fillColor = col(TH.input);
        mkCut(bg, -bw / 2, -bh / 2, bw, bh, 4); bg.fill();
        bg.strokeColor = col(TH.cardBrd); bg.lineWidth = 1;
        mkCut(bg, -bw / 2, -bh / 2, bw, bh, 4); bg.stroke();

        var valLabel = mkLabel(String(shown.v), Math.max(12, Math.round(bh * 0.62)), bw - 10, bh, col(TH.title));
        box.addChild(valLabel, 3);
        var eb = null;
        try {
            if (cc.EditBox) {
                eb = box.addComponent(cc.EditBox);
                eb.fontSize = Math.max(10, Math.round(bh * 0.56));
                eb.string = String(shown.v);
                try { eb.inputMode = cc.EditBox.InputMode.NUMERIC; } catch (e) {}
                try { if (eb.textLabel) eb.textLabel.node.active = false; } catch (e) {}   // 用我们自己的数字标签
                eb.node.on('text-changed', function () {
                    valLabel.getComponent(cc.Label).string = String(eb.string || '');
                    var v = parseInt(eb.string, 10); if (v > 0) { shown.v = v; onInput(v); }
                });
            }
        } catch (e) { eb = null; }
        return y - UI.geo.spacing;
    }

    function addRowSlider(parent, y, name, sub, value, cb, maxArg) {
        var row = addRowBase(parent, y, name, sub);
        row.__wxHorizontalSlider = true;
        var rw = row.__rw, rh = row.__rh, pad = row.__pad, textW = row.__textW;
        var min = 1.0, max = (typeof maxArg === 'number' && maxArg > min) ? maxArg : 20.0;
        var v = value > 1 ? value : 1.0;
        if (v > max) v = max;

        var valW = Math.max(34, Math.round(rw * 0.16));
        var valLabel = mkLabel(v.toFixed(1) + 'x', Math.max(10, Math.round(rh * 0.30)), valW, Math.round(rh * 0.5), accentC());
        valLabel.x = rw / 2 - pad - valW / 2; row.addChild(valLabel, 6);

        // 滑条：从文字区右边到数值左边
        var x0 = -rw / 2 + pad + textW + Math.round(rw * 0.02);
        var x1 = rw / 2 - pad - valW - Math.round(rw * 0.03);
        var bw = Math.max(40, x1 - x0);
        var track = new cc.Node('wx_slider');
        track.setContentSize(bw, Math.max(20, Math.round(rh * 0.5)));
        track.x = (x0 + x1) / 2; row.addChild(track, 5);
        var g = track.addComponent(cc.Graphics);
        var hw = bw / 2;
        function paint() {
            var r = (v - min) / (max - min);
            var kx = -hw + r * bw;
            g.clear();
            // 滑条：关闭态深绿灰轨道 + 亮绿已选段 + 亮绿圆点（对齐弹壳战机的倍速条）
            g.fillColor = col(TH.trackOff); mkRound(g, -hw, -3, bw, 6, 3); g.fill();
            g.fillColor = accentC(); mkRound(g, -hw, -3, 10 + r * (bw - 10), 6, 3); g.fill();
            g.fillColor = col(TH.panel); g.circle(kx, 0, 10); g.fill();
            g.fillColor = accentC(); g.circle(kx, 0, 8); g.fill();
            valLabel.getComponent(cc.Label).string = v.toFixed(1) + 'x';
        }
        function setV(nv) {
            nv = Math.round(Math.max(min, Math.min(max, nv)) * 10) / 10;
            if (nv === v) return;
            v = nv; paint(); cb && cb(v);
        }
        paint();
        function byTouch(e) {
            try {
                var p = e.getLocation();
                var world = row.parent ? row.parent.convertToWorldSpaceAR(row.getPosition()) : row.getPosition();
                var left = world.x + x0;
                setV(min + clamp((p.x - left) / bw, 0, 1) * (max - min));
            } catch (err) {}
        }
        track.on(cc.Node.EventType.TOUCH_START, byTouch);
        track.on(cc.Node.EventType.TOUCH_MOVE, byTouch);
        row.on(cc.Node.EventType.TOUCH_START, byTouch);
        row.on(cc.Node.EventType.TOUCH_MOVE, byTouch);
        return y - UI.geo.spacing;
    }

    function bindBall(ball, root, panel) {
        var down = false, dragged = false, finished = false;
        var sx = 0, sy = 0, rx = 0, ry = 0, startedAt = 0;
        var dragThreshold = Math.max(10, Math.min(24, 8 * dpr()));
        function pointInParent(e) {
            var p = e.getLocation();
            try {
                if (root.parent && root.parent.convertToNodeSpaceAR) return root.parent.convertToNodeSpaceAR(cc.v2(p.x, p.y));
            } catch (err) {}
            return p;
        }
        function openPanel() {
            // 面板可能被换场景销毁了：先检查，必要时立刻重建，再打开。
            var alive = UI.panel && nodeOk(UI.panel) && UI.panel.parent && UI.proot && nodeOk(UI.proot) && UI.proot.parent;
            if (!alive) {
                try { buildMenu(); } catch (er) {}
            }
            if (UI.panel && nodeOk(UI.panel) && UI.panel.parent) {
                UI.panel.active = true; UI.open = true;
                if (UI.ball && nodeOk(UI.ball)) UI.ball.active = false;
                bringFloatToFront(UI.proot, UI_LAYER.panel);
                clampPanelForOpen();
                wxStatus('panel-open');
            } else {
                UI.open = false;
                wxStatus('panel-open-failed');
            }
        }
        ball.on(cc.Node.EventType.TOUCH_START, function (e) {
            down = true; dragged = false; finished = false; startedAt = Date.now();
            var p = pointInParent(e); sx = p.x; sy = p.y; rx = root.x; ry = root.y;
            e.stopPropagation && e.stopPropagation();
        });
        ball.on(cc.Node.EventType.TOUCH_MOVE, function (e) {
            if (!down) return;
            var p = pointInParent(e), dx = p.x - sx, dy = p.y - sy;
            if (!dragged && (dx * dx + dy * dy) >= dragThreshold * dragThreshold) dragged = true;
            if (dragged) {
                root.x = rx + dx; root.y = ry + dy;
                var q = clampToSafe(root.x, root.y, 48, 48);
                root.x = q.x; root.y = q.y;
            }
            e.stopPropagation && e.stopPropagation();
        });
        function finishTouch(e, cancelled) {
            if (!down || finished) return;
            finished = true; down = false;
            if (dragged) saveFloat(root.x, root.y);
            // 某些鸿蒙触摸栈把正常轻点以 CANCEL 结束；短按且未移动时仍按点击处理。
            else if (!cancelled || Date.now() - startedAt < 800) openPanel();
            e.stopPropagation && e.stopPropagation();
        }
        ball.on(cc.Node.EventType.TOUCH_END, function (e) { finishTouch(e, false); });
        ball.on(cc.Node.EventType.TOUCH_CANCEL, function (e) { finishTouch(e, true); });
    }


    /* ================= 4.2 过检测（JS 层这部分） =================
     * JS 只能拦"通过 JS bridge 发起的退出"；Java 层的环境检测伪造
     * （EmutorUtils / NativeInfo / CocosBridgeAPI 的 boolean 全改 false，
     *  以及 System.exit / Runtime.exit / halt / Process.killProcess 拦截）
     * 必须在 dex 里做 —— 见 dex_loader/Hooks.java。
     * ================= */
    function guardExit() {
        try {
            var g = window.gdkjsb;
            if (!g || g.__wxGuard || typeof g.exitProgram !== 'function') return;
            var orig = g.exitProgram;
            g.exitProgram = function () {
                if (CFG.noExit === false) return orig.apply(this, arguments);
                try {
                    window.__WX_EXIT_BLOCKED__ = (window.__WX_EXIT_BLOCKED__ || 0) + 1;
                    window.__WX_EXIT_STACK__ = (new Error()).stack || '';
                } catch (e) {}
                return undefined;
            };
            g.__wxGuard = true;
            window.__WX_NOEXIT__ = true;
        } catch (e) {}
    }

    /* ================= 4. 游戏 Hook（本游戏真实字段） ================= */
    var CFG = {
        on: false, value: parseInt(storageGet('wx_value', DEF.value), 10) || DEF.value,
        lockHp: storageGet('wx_lockHp', '0') === '1',
        lockMp: storageGet('wx_lockMp', '0') === '1',
        autoFight: storageGet('wx_autoFight', '0') === '1',
        noCd: storageGet('wx_noCd', '0') === '1',
        gspeed: parseFloat(storageGet('wx_gspeed', '1')) || 1.0,
        speed: parseFloat(storageGet('wx_speed', '1')) || 1.0,
        noExit: storageGet('wx_noExit', '1') !== '0'
    };
    function keyOf(name) {
        if (name === '英雄锁血') return 'lockHp';
        if (name === '锁定蓝量') return 'lockMp';
        if (name === '解锁金卡与自动战斗') return 'autoFight';
        if (name === '技能无CD') return 'noCd';
        if (name === '图内全局加速') return 'gspeed';
        return 'on';
    }
    function save() {
        storageSet('wx_value', CFG.value); storageSet('wx_on', CFG.on ? '1' : '0');
        storageSet('wx_lockHp', CFG.lockHp ? '1' : '0');
        storageSet('wx_lockMp', CFG.lockMp ? '1' : '0');
        storageSet('wx_noCd', CFG.noCd ? '1' : '0');
        storageSet('wx_gspeed', String(CFG.gspeed));
        storageSet('wx_autoFight', CFG.autoFight ? '1' : '0'); storageSet('wx_speed', CFG.speed);
        try { if (window.__WX_SAVE_HOOK__) window.__WX_SAVE_HOOK__(); } catch (e) {}
    }
    CFG.on = storageGet('wx_on', '0') === '1';

    /*
     * ================= 4. 战斗功能层（资源版本 1.1.3.2） =================
     * 每个功能只写自己的字段：
     *   伤害/免伤共用唯一 hY 分发器，避免多层包装；
     *   无 CD 只挂 bil；移速只挂 bij(2)；全局加速只挂 hz.mainTimeScale；
     *   自动战斗只挂 fDn.fDj；金卡持有权限只挂 fDn.fDc("gold")。
     *   每项功能使用独立、唯一入口。
     */
    var ST = {
        hits: 0, lock: 0, lockMp: 0, mpHook: false, noDmg: 0, noCd: false, auto: false, gold: false,
        moveHook: false, moveSource: '', gsEnts: 0,
        battleSerial: 0, errors: {}
    };
    var BATTLE = null;
    var entMark = 0;          // entities() 的 O(1) 去重标记（见下面的 add）

    function featureError(name, err) {
        try { ST.errors[name] = String(err && err.message || err); } catch (e) {}
    }

    var BattleCore = (function () {
        function controller() {
            try {
                var mgr = window.$eZ && window.$eZ.battleMgr;
                return (mgr && mgr.bEu) || null;
            } catch (e) { return null; }
        }

        function world() {
            var ctrl = controller(), out = null;
            try { if (ctrl && typeof ctrl.nM === 'function') out = ctrl.nM(); } catch (e) {}
            return out || (ctrl && (ctrl.nh || ctrl.ni)) || null;
        }

        function bind() {
            var next = world();
            if (next !== BATTLE) {
                BATTLE = next;
                ST.battleSerial++;
                ST.lock = 0;
                try { FeatureManager.onBattleChanged(); } catch (e) {}
            }
            return BATTLE;
        }

        function stat(ent) {
            var value = ent && ent.hw;
            return value && typeof value === 'object' ? value : null;
        }

        function time(ent) {
            var value = ent && ent.hz;
            return value && typeof value === 'object' ? value : null;
        }

        function hero() {
            var ctrl = controller(), value = null;
            try { if (ctrl && typeof ctrl.ny === 'function') value = ctrl.ny(); } catch (e) {}
            if (!value || !stat(value)) return null;
            try {
                value.__wxHero = true;
                stat(value).__wxHero = true;
            } catch (e2) {}
            return value;
        }

        function isHero(ent) {
            if (!ent) return false;
            try {
                if (ent.__wxHero) return true;
                var s = stat(ent);
                if (s && s.__wxHero) return true;
                var current = hero();
                if (current === ent) return true;
            } catch (e) {}
            return false;
        }

        function entityFromRef(b, ref) {
            if (!b || typeof b.gi !== 'function') return null;
            var id = ref;
            try {
                if (ref && typeof ref === 'object') id = ref.ddZ;
                return b.gi(id) || null;
            } catch (e) { return null; }
        }

        function entities() {
            var out = [], b = BATTLE || world(), mark = ++entMark;
            function add(ent) {
                if (!ent || !stat(ent)) return;
                // O(1) 去重：原来是 out.indexOf(ent)，实体一多就是 O(n²)。
                // 这个方法被 HitBus.install（每秒一次）和全局加速扫描（每 700ms）反复调用，
                // 进图那一刻实体表刚建好、数量最大，就是"进图第一下卡住"的主要开销。
                try {
                    if (ent.__wxEntMark === mark) return;
                    ent.__wxEntMark = mark;
                } catch (e) {
                    if (out.indexOf(ent) >= 0) return;      // 对象被冻结时退回原来的写法
                }
                out.push(ent);
            }
            try {
                var refs = b && b.fF;
                if (refs && typeof refs.length === 'number') {
                    for (var i = 0; i < refs.length; i++) add(entityFromRef(b, refs[i]));
                }
            } catch (e2) {}
            add(hero());
            return out;
        }

        function hp(s) {
            try { return s && typeof s.bhd === 'number' ? s.bhd : -1; } catch (e) { return -1; }
        }

        function maxHp(s) {
            try { return s && typeof s.bhe === 'number' ? s.bhe : -1; } catch (e) { return -1; }
        }

        function setHp(s, value) {
            try {
                if (s && typeof s.bhd === 'number' && value >= 0) {
                    s.bhd = value;
                    return true;
                }
            } catch (e) {}
            return false;
        }

        function mp(s) {
            try { return s && typeof s.bhf === 'number' ? s.bhf : -1; } catch (e) { return -1; }
        }

        function maxMp(s) {
            try { return s && typeof s.bhg === 'number' ? s.bhg : -1; } catch (e) { return -1; }
        }

        function setMp(s, value) {
            try {
                if (s && typeof s.bhf === 'number' && value >= 0) {
                    s.bhf = value;
                    return true;
                }
            } catch (e) {}
            return false;
        }

        return {
            controller: controller,
            world: world,
            bind: bind,
            stat: stat,
            time: time,
            hero: hero,
            isHero: isHero,
            entities: entities,
            hp: hp,
            maxHp: maxHp,
            setHp: setHp,
            mp: mp,
            maxMp: maxMp,
            setMp: setMp
        };
    })();

    var GuardFeature = (function () {
        var pending = [];
        var flushQueued = false;

        function beforeHit(ctx) {
            if (!CFG.lockHp) return;
            if (!BattleCore.isHero(ctx.target) || !ctx.targetStat || !(ctx.beforeHp > 0)) return;
            ctx.protected = true;
            var s = ctx.targetStat;
            try {
                if (!s.__wxGuardQueued) s.__wxGuardOldMin = Number(s.lockMinHP) || 0;
                s.__wxGuardRestoreHp = Math.max(Number(s.__wxGuardRestoreHp) || 0, ctx.beforeHp);
                if (typeof s.lockMinHP === 'number') s.lockMinHP = Math.max(1, s.lockMinHP || 0);
            } catch (e) {}
        }

        function restore(item) {
            var s = item && item.stat;
            if (!s) return;
            try {
                var wanted = Number(s.__wxGuardRestoreHp) || item.hp || 1;
                BattleCore.setHp(s, wanted);
                if (typeof s.lockMinHP === 'number') s.lockMinHP = Number(s.__wxGuardOldMin) || 0;
                s.__wxGuardRestoreHp = 0;
                s.__wxGuardQueued = false;
                ST.noDmg++;
            } catch (e) {
                try { s.__wxGuardQueued = false; } catch (e2) {}
                featureError('guardRestore', e);
            }
        }

        function flush() {
            if (!pending.length) return;
            var list = pending.splice(0, pending.length);
            for (var i = 0; i < list.length; i++) restore(list[i]);
        }

        function afterHit(ctx) {
            if (!ctx.protected || !ctx.targetStat) return;
            var s = ctx.targetStat;
            try {
                if (!s.__wxGuardQueued) {
                    s.__wxGuardQueued = true;
                    pending.push({ stat: s, hp: ctx.beforeHp });
                }
                // 一次命中风暴里会有几百次回调，原来每次都要挂一个定时器。
                // 合并成一个：pending 在 tick 里也会被 flush，不会漏。
                if (typeof setTimeout === 'function') {
                    if (!flushQueued) {
                        flushQueued = true;
                        setTimeout(function () { flushQueued = false; flush(); }, 0);
                    }
                } else flush();
            } catch (e) { featureError('guardQueue', e); }
        }

        function tick() { flush(); BattleCore.hero(); }
        function reset() { flush(); pending.length = 0; }

        return { beforeHit: beforeHit, afterHit: afterHit, tick: tick, reset: reset };
    })();

    var DamageFeature = (function () {
        function beforeHit(ctx) {
            if (!CFG.on || !(ctx.damage > 0)) return;
            if (!BattleCore.isHero(ctx.attacker) || BattleCore.isHero(ctx.target)) return;
            var wanted = Math.floor(Number(CFG.value));
            if (!(wanted > 0) || !isFinite(wanted)) return;
            ctx.damage = wanted;
            ST.hits++;
            DamageDisplayFeature.note(ctx.attacker, ctx.target, wanted);
        }

        return { beforeHit: beforeHit };
    })();

    var HitBus = (function () {
        function hookPrototype(proto) {
            if (!proto || typeof proto.hY !== 'function') return false;
            if (proto.hY.__wxHitBus) return true;

            var nativeHit = proto.hY;
            proto.hY = function (attacker, damage, hitInfo) {
                var originalDamage = Number(damage) || 0;
                var s = BattleCore.stat(this);
                var ctx = {
                    target: this,
                    attacker: attacker,
                    damage: originalDamage,
                    hitInfo: hitInfo,
                    targetStat: s,
                    beforeHp: BattleCore.hp(s),
                    protected: false
                };

                /* ★ 过检测（自 remote.js installAttr 迁移）：放大值不能进上报字段。
                   1.1.3.2 里这两个字段的消费点还在：
                     · battleMgr.bET({damage: hw.finalDamage, takenDamage: hw.finalHurtValue, ...}) 上报
                     · 伤害统计面板 o.cZq/c.cZr
                   而 hY 内部会把本次实际伤害累加进攻击方 hw.finalDamage 与受击者 hw.finalHurtValue。
                   所以调用前记原值，调用后只把【原始伤害】补回去；只在数值确实被改动过时才写回。 */
                var atkStat = null, vicStat = ctx.targetStat;
                var atkBefore = null, vicBefore = null;
                try {
                    atkStat = BattleCore.stat(attacker);
                    if (atkStat && typeof atkStat.finalDamage === 'number') atkBefore = atkStat.finalDamage;
                    if (vicStat && typeof vicStat.finalHurtValue === 'number') vicBefore = vicStat.finalHurtValue;
                } catch (eA) {}

                try { DamageFeature.beforeHit(ctx); } catch (e) { featureError('damageBefore', e); }
                try { GuardFeature.beforeHit(ctx); } catch (e2) { featureError('guardBefore', e2); }

                var result = nativeHit.call(this, attacker, ctx.damage, hitInfo);

                try {
                    if (ctx.damage !== originalDamage) {
                        if (atkBefore !== null && atkStat && typeof atkStat.finalDamage === 'number' && atkStat.finalDamage !== atkBefore) {
                            atkStat.finalDamage = atkBefore + originalDamage;
                        }
                        if (vicBefore !== null && vicStat && typeof vicStat.finalHurtValue === 'number' && vicStat.finalHurtValue !== vicBefore) {
                            vicStat.finalHurtValue = vicBefore + originalDamage;
                        }
                        ST.acFixed = (ST.acFixed || 0) + 1;
                    }
                } catch (eC) { featureError('acRestore', eC); }

                try { GuardFeature.afterHit(ctx); } catch (e4) { featureError('guardAfter', e4); }
                return result;
            };
            proto.hY.__wxHitBus = true;
            proto.hY.__wxOrig = nativeHit;
            ST.hitBus = true;
            return true;
        }

        function install() {
            var entities = BattleCore.entities(), protos = [];
            for (var i = 0; i < entities.length; i++) {
                var proto = null;
                try { proto = Object.getPrototypeOf(entities[i]); } catch (e) {}
                if (!proto || protos.indexOf(proto) >= 0) continue;
                protos.push(proto);
                try { hookPrototype(proto); } catch (e2) { featureError('hitBusProto', e2); }
            }
            return protos.length > 0;
        }

        return { install: install };
    })();

    /* 1.1.3.2：battleMgr.bEu.ne.jg(event) 直接读取 event.finalDamage。 */
    var DamageDisplayFeature = (function () {
        var pending = [];

        function note(attacker, target, wanted) {
            pending.push({
                t: Date.now(), attacker: attacker, target: target,
                wanted: Number(wanted) || 0
            });
            if (pending.length > 48) pending.splice(0, pending.length - 48);
        }

        function take(event) {
            if (!event || typeof event.finalDamage !== 'number') return null;
            var now = Date.now();
            while (pending.length && now - pending[0].t > 1200) pending.shift();
            if (!pending.length) return null;
            var source = event.atkEntity;
            var target = event.target;
            if (!source || !BattleCore.isHero(source) || !target || BattleCore.isHero(target)) return null;

            for (var i = pending.length - 1; i >= 0; i--) {
                var item = pending[i];
                if (source !== item.attacker || target !== item.target) continue;
                pending.splice(i, 1);
                return item;
            }
            return null;
        }

        function install() {
            var ctrl = BattleCore.controller(), renderer = ctrl && ctrl.ne;
            var owner = null;
            try {
                owner = renderer && Object.getPrototypeOf(renderer);
                if (!owner || typeof owner.jg !== 'function') owner = renderer;
            } catch (e0) { owner = renderer; }
            if (owner && typeof owner.jg === 'function') {
                var nativeJg = owner.jg;
                if (!nativeJg.__wxDamageRender) {
                    owner.jg = function (event) {
                        var item = null;
                        try { item = take(event); } catch (e1) { featureError('damageRenderTake', e1); }
                        if (item && event && typeof event.finalDamage === 'number') {
                            var old = event.finalDamage;
                            event.finalDamage = item.wanted;
                            ST.renderHits = (ST.renderHits || 0) + 1;
                            ST.renderFields = (ST.renderFields || 0) + 1;
                            ST.renderDamage = item.wanted;
                            try { return nativeJg.apply(this, arguments); }
                            finally { try { event.finalDamage = old; } catch (e2) {} }
                        }
                        return nativeJg.apply(this, arguments);
                    };
                    owner.jg.__wxDamageRender = true;
                    owner.jg.__wxOrig = nativeJg;
                    ST.renderHooks = 1;
                } else ST.renderHooks = 1;
                return true;
            }

            return false;
        }

        function reset() { pending.length = 0; }
        return { note: note, install: install, reset: reset };
    })();

    var LockHpFeature = (function () {
        var last = 0;

        function tick(now, force) {
            if (!CFG.lockHp) return;
            if (!force && now - last < 300) return;
            last = now;
            try {
                var hero = BattleCore.hero();
                var s = BattleCore.stat(hero);
                var max = BattleCore.maxHp(s);
                // 已经是满血就别再写：每 300ms 无条件写一次血量会触发游戏自己的
                // "血量变化"链路（血条/特效/状态重算），进图那一刻尤其明显。
                if (max > 0 && BattleCore.hp(s) !== max && BattleCore.setHp(s, max)) ST.lock++;
            } catch (e) { featureError('lockHp', e); }
        }

        function reset() { last = 0; }
        return { tick: tick, reset: reset };
    })();

    var LockManaFeature = (function () {
        var last = 0;

        function install() {
            if (!CFG.lockMp) return false;
            var hero = BattleCore.hero();
            var s = BattleCore.stat(hero);
            if (!s || typeof s.bhf !== 'number' || typeof s.bhg !== 'number') return false;
            if (s.__wxManaLock && typeof s.__wxManaLock === 'object') { ST.mpHook = true; return true; }
            try {
                var desc = Object.getOwnPropertyDescriptor(s, 'bhf');
                if (!desc || !Object.prototype.hasOwnProperty.call(desc, 'value') || desc.configurable === false || desc.writable === false) return false;
                var state = { value: Number(desc.value), enumerable: desc.enumerable !== false, writable: desc.writable !== false };
                Object.defineProperty(s, 'bhf', {
                    configurable: true,
                    enumerable: state.enumerable,
                    get: function () {
                        var max = Number(this.bhg);
                        if (CFG.lockMp && max > 0 && state.value < max) state.value = max;
                        return state.value;
                    },
                    set: function (next) {
                        var n = Number(next);
                        if (!isFinite(n)) return;
                        var max = Number(this.bhg);
                        if (CFG.lockMp && max > 0 && n < max) {
                            state.value = max;
                            ST.lockMp++;
                            return;
                        }
                        state.value = n;
                    }
                });
                try { Object.defineProperty(s, '__wxManaLock', { value: state, configurable: true, enumerable: false }); }
                catch (e1) { s.__wxManaLock = state; }
                ST.mpHook = true;
                if (s.bhg > 0) s.bhf = s.bhg;
                return true;
            } catch (e) {
                featureError('lockMpInstall', e);
                return false;
            }
        }

        function uninstall() {
            try {
                var s = BattleCore.stat(BattleCore.hero());
                var state = s && s.__wxManaLock;
                if (!state || typeof state !== 'object') { ST.mpHook = false; return true; }
                Object.defineProperty(s, 'bhf', {
                    value: Number(state.value) || 0,
                    writable: state.writable !== false,
                    configurable: true,
                    enumerable: state.enumerable !== false
                });
                try { delete s.__wxManaLock; } catch (e0) {}
                ST.mpHook = false;
                return true;
            } catch (e) { featureError('lockMpUninstall', e); return false; }
        }

        function setEnabled(on) {
            last = 0;
            if (!on) return uninstall();
            if (!install()) return tick(Date.now(), true);
            return true;
        }

        function tick(now, force) {
            if (!CFG.lockMp) return;
            if (!force && now - last < 250) return;
            last = now;
            try {
                install();
                var s = BattleCore.stat(BattleCore.hero());
                var max = BattleCore.maxMp(s);
                if (max > 0 && BattleCore.mp(s) !== max && BattleCore.setMp(s, max)) ST.lockMp++;
            } catch (e) { featureError('lockMp', e); }
        }

        function reset() { last = 0; ST.mpHook = false; }
        return { install: install, uninstall: uninstall, setEnabled: setEnabled, tick: tick, reset: reset };
    })();

    var NoCdFeature = (function () {
        function install() {
            var hero = BattleCore.hero();
            var s = BattleCore.stat(hero);
            var proto = s && Object.getPrototypeOf(s);
            if (!proto || typeof proto.bil !== 'function') return false;
            if (proto.bil.__wxNoCd) { ST.noCd = true; return true; }

            var nativeCd = proto.bil;
            proto.bil = function () {
                // 先把原函数跑了：冷却不只是读一个数，游戏会顺带推进技能状态，
                // 跳过它（旧版写法）会让技能状态机停在半路。
                var cd = nativeCd.apply(this, arguments);
                try { if (CFG.noCd && this && this.__wxHero) return 0; } catch (e) {}
                return cd;
            };
            proto.bil.__wxNoCd = true;
            proto.bil.__wxOrig = nativeCd;
            ST.noCd = true;
            return true;
        }

        return { install: install };
    })();

    function effectiveMoveScale() {
        var move = parseFloat(CFG.speed);
        var global = parseFloat(CFG.gspeed);
        if (!(move > 1)) return 1;
        if (!(global > 1)) return move;
        return Math.max(1, move / global);
    }

    var MoveSpeedFeature = (function () {
        function install() {
            var hero = BattleCore.hero();
            var s = BattleCore.stat(hero);
            var proto = s && Object.getPrototypeOf(s);
            if (!proto || typeof proto.bij !== 'function') return false;
            if (proto.bij.__wxMove) {
                ST.moveHook = true;
                ST.moveSource = 'bij';
                return true;
            }

            var nativeSpeed = proto.bij;
            proto.bij = function (type) {
                var result = nativeSpeed.apply(this, arguments);
                try {
                    var scale = effectiveMoveScale();
                    if (type === 2 && this && this.__wxHero && scale > 1 && result > 0) result *= scale;
                } catch (e) {}
                return result;
            };
            proto.bij.__wxMove = true;
            proto.bij.__wxOrig = nativeSpeed;
            ST.moveHook = true;
            ST.moveSource = 'bij';
            return true;
        }

        return { install: install };
    })();

    var GlobalSpeedFeature = (function () {
        var lastScan = 0;

        /* 当前是否该加倍速。
         * 换战斗后的头 3 秒一律返回 1（不加速）：入场那一段是游戏自己在推进状态机
         * （进图卡死、点自动战斗卡死都出在这里），乘上去就会把它卡停在半路。 */
        function scaleNow() {
            var scale = parseFloat(CFG.gspeed);
            if (!(scale > 1)) return 1;
            if (Date.now() - (ST.lastBattleAt || 0) < 3000) return 1;
            return scale;
        }

        function hookTime(t) {
            if (!t || typeof t.mainTimeScale !== 'number') return false;
            if (t.__wxGlobalTime) return true;
            try {
                var base = t.mainTimeScale;
                Object.defineProperty(t, 'mainTimeScale', {
                    configurable: true,
                    enumerable: true,
                    get: function () {
                        var s = scaleNow();
                        // 只有游戏自己处于正常速率（base=1）时才乘。
                        // base<1 说明游戏正在入场动画/顿帧/慢动作/暂停里自己推进状态，
                        // 那些值原样返回，绝不参与乘法 —— 否则状态机停在半路 = 卡死。
                        return (s > 1 && base >= 1) ? base * s : base;
                    },
                    set: function (value) {
                        // 上限压到 1：游戏自己的写法是"读出来改一改再写回"
                        // （t.x = t.x * k、t.x -= dt），不压住会把倍速一次次乘进去跑飞。
                        base = (typeof value === 'number' && isFinite(value) && value >= 0)
                                ? Math.min(value, 1) : 1;
                    }
                });
                Object.defineProperty(t, '__wxGlobalTime', { value: true, configurable: true, enumerable: false });
                return true;
            } catch (e) {
                featureError('globalHook', e);
                return false;
            }
        }

        function tick(now, force) {
            if (!(parseFloat(CFG.gspeed) > 1)) { ST.gsEnts = 0; return; }
            if (!force && now - lastScan < 700) return;
            lastScan = now;
            var list = BattleCore.entities(), count = 0;
            for (var i = 0; i < list.length; i++) {
                var t = BattleCore.time(list[i]);
                if (t && hookTime(t)) count++;
            }
            ST.gsEnts = count;
        }

        function reset() { lastScan = 0; }
        return { tick: tick, reset: reset };
    })();

    var AutoFightFeature = (function () {
        function installCard() {
            var data = window.$eZ && window.$eZ.monCard && window.$eZ.monCard.fDn;
            var proto = data && Object.getPrototypeOf(data);
            if (!proto || typeof proto.fDj !== 'function') return false;
            if (!proto.fDj.__wxAutoFight) {
                var nativePermission = proto.fDj;
                proto.fDj = function () {
                    var allowed = nativePermission.apply(this, arguments);
                    try { if (!allowed && CFG.autoFight) return true; } catch (e) {}
                    return allowed;
                };
                proto.fDj.__wxAutoFight = true;
                proto.fDj.__wxOrig = nativePermission;
            }
            return true;
        }

        function install() {
            try {
                var ok = installCard();
                ST.auto = !!ok;
                return !!ok;
            } catch (e) { featureError('autoFight', e); return false; }
        }

        return { install: install };
    })();

    var GoldCardFeature = (function () {
        function install() {
            try {
                var data = window.$eZ && window.$eZ.monCard && window.$eZ.monCard.fDn;
                var proto = data && Object.getPrototypeOf(data);
                if (!proto || typeof proto.fDc !== 'function') return false;
                if (!proto.fDc.__wxGoldCard) {
                    var nativeOwned = proto.fDc;
                    proto.fDc = function (cardType) {
                        var owned = nativeOwned.apply(this, arguments);
                        try { if (cardType === 'gold' && CFG.autoFight) return true; } catch (e) {}
                        return owned;
                    };
                    proto.fDc.__wxGoldCard = true;
                    proto.fDc.__wxOrig = nativeOwned;
                }
                ST.gold = true;
                return true;
            } catch (e) { featureError('goldCard', e); return false; }
        }

        return { install: install };
    })();

    var FeatureManager = (function () {
        var lastInstall = 0;

        function onBattleChanged() {
            lastInstall = 0;
            ST.lastBattleAt = Date.now();      // 进图窗口计时（全局加速前 3 秒不加速）
            GuardFeature.reset();
            LockHpFeature.reset();
            LockManaFeature.reset();
            GlobalSpeedFeature.reset();
            DamageDisplayFeature.reset();
        }

        function install(force) {
            var now = Date.now();
            if (!force && now - lastInstall < 1000) return;
            lastInstall = now;
            BattleCore.hero();
            if (CFG.on || CFG.lockHp) HitBus.install();
            if (CFG.on) DamageDisplayFeature.install();
            if (CFG.lockMp) LockManaFeature.install();
            if (CFG.noCd) NoCdFeature.install();
            if (parseFloat(CFG.speed) > 1) MoveSpeedFeature.install();
            if (CFG.autoFight) {
                AutoFightFeature.install();
                GoldCardFeature.install();
            }
        }

        function tick(now) {
            BattleCore.bind();
            install(false);
            GuardFeature.tick();
            LockHpFeature.tick(now, false);
            LockManaFeature.tick(now, false);
            GlobalSpeedFeature.tick(now, false);
        }

        return { onBattleChanged: onBattleChanged, install: install, tick: tick };
    })();

    // 菜单回调只转发到对应独立模块，不再包含旧 Hook。
    function hookBattleSpeed() {
        try {
            BattleCore.bind();
            window.__WX_MOVE_V__ = parseFloat(CFG.speed) || 1;
            window.__WX_MOVE_EFFECTIVE__ = effectiveMoveScale();
            if (!(parseFloat(CFG.speed) > 1)) return true;
            return MoveSpeedFeature.install();
        } catch (e) { featureError('moveMenu', e); return false; }
    }
    function applyGlobalSpeed() {
        try { BattleCore.bind(); GlobalSpeedFeature.tick(Date.now(), true); return true; }
        catch (e) { featureError('globalMenu', e); return false; }
    }
    function unlockAuto() {
        try {
            if (!CFG.autoFight) return true;
            var autoOk = AutoFightFeature.install();
            var goldOk = GoldCardFeature.install();
            return autoOk && goldOk;
        }
        catch (e) { featureError('autoMenu', e); return false; }
    }

    /*<WX_AUTO_ENGINE_BEGIN>*/
    /* 自动引擎仅迁移 remote.js 的任务/背包/结算逻辑；旧战斗、加速、锁血和检测 Hook 均未合并。 */
    (function (W) {
      if (!W || W.__WX_AUTO_ENGINE__) return;
      var REMOTE_BUILD = 'res-1.1.3.2';
      function lsGet(k, d) { return storageGet(k, d); }
      function lsSet(k, v) { return storageSet(k, v); }
      function wxFlag(k) { return W.__WX_CARD_PERMANENT__ === true && String(lsGet(k, '0')) === '1'; }
      function log(text) { try { console.log('[AUTO] ' + String(text == null ? '' : text)); } catch (e) {} }
      function nativeCall() { return null; }
      W.RemoteUI = W.RemoteUI || {};
      W.RemoteUI.notify = W.RemoteUI.notify || function () {};
      W.RemoteUI.mergeState = W.RemoteUI.mergeState || function (patch) { try { W.__WX_AUTO_STATE__ = patch || {}; } catch (e) {} };
      W.RemoteUI.updateState = W.RemoteUI.updateState || W.RemoteUI.mergeState;
      function state(patch) {
        try {
          var out = W.__WX_AUTO_STATE__ || (W.__WX_AUTO_STATE__ = {}), p = patch || {};
          for (var k in p) if (Object.prototype.hasOwnProperty.call(p, k)) out[k] = p[k];
        } catch (e) {}
      }
      W.__REMOTE_COMMANDS__ = W.__REMOTE_COMMANDS__ || {};
      W.__REMOTE_COMMANDS__['wx.flag'] = function (payload) {
        var key = payload && payload.key;
        if (!key) return { lastAction: 'no-key' };
        var enabled = !!(payload && payload.value);
        if (enabled && W.__WX_CARD_PERMANENT__ !== true) {
          lsSet(key, '0'); W[key] = false;
          return { lastAction: 'permanent-required', value: false };
        }
        lsSet(key, enabled ? '1' : '0'); W[key] = enabled;
        return { lastAction: key + '=' + enabled, value: enabled };
      };
      W.__REMOTE_COMMANDS__['wx.num'] = function (payload) {
        var key = payload && payload.key, value = payload && payload.value;
        if (!key) return { lastAction: 'no-key' };
        lsSet(key, String(value)); W[key] = parseFloat(value) || 0;
        return { lastAction: key + '=' + W[key] };
      };
  function get(m) { try { var x = __require(m); return x && (x.default || x); } catch (e) { return null; } }

  /* 1.1.3.2 全局管理器固定为 $eZ；未就绪时由后续 tick 重试。 */
  function F() {
    try { return globalThis.$eZ && globalThis.$eZ.battleMgr ? globalThis.$eZ : null; }
    catch (e) { return null; }
  }

  /* v26d：清掉旧格式残留（避免上一版的失败计数把任务永久封死） */
  try {
    var _ls = (W.cc || cc) && (W.cc || cc).sys && (W.cc || cc).sys.localStorage;
    if (_ls) {
      var _kill = [];
      for (var _i = 0; _i < (_ls.length || 0); _i++) {
        var _k = _ls.key(_i);
        if (_k && _k.indexOf("__WX_SUBFAIL_") === 0 && _k.indexOf(REMOTE_BUILD) < 0) _kill.push(_k);
      }
      for (var _j = 0; _j < _kill.length; _j++) _ls.removeItem(_kill[_j]);
      if (String(_ls.getItem("__WX_UNK_SKIP_CNT_VER__") || "") !== REMOTE_BUILD) {
        _ls.setItem("__WX_UNK_SKIP_CNT__", "0");
        _ls.setItem("__WX_UNK_SKIP_CNT_VER__", REMOTE_BUILD);
      }
    }
  } catch (e) {}

      var CFG = {
        autoFlip: function () { return wxFlag('__WX_AUTO_FLIP__'); },
        autoRepeat: function () { return wxFlag('__WX_AUTO_REPEAT__'); },
        autoRevive: function () { return wxFlag('__WX_AUTO_REVIVE__'); },
        autoDecompose: function () { return wxFlag('__WX_AUTO_DECOMPOSE__'); },
        autoSell: function () { return wxFlag('__WX_AUTO_SELL__'); },
        autoWear: function () { return wxFlag('__WX_AUTO_WEAR__'); },
        autoSkill: function () { return wxFlag('__WX_AUTO_SKILL__'); },
        autoJob: function () { return wxFlag('__WX_AUTO_JOBSWITCH__') || wxFlag('__WX_AUTO_JOB__'); },
        sellQualities: function () { var v=lsGet('__WX_SELL_Q__',''); return v === '' ? '1,2,3' : v; },
        decompQualities: function () { var v=lsGet('__WX_DECOMP_Q__',''); return v === '' ? '1,2,3' : v; },
        jobSelect: function () { return lsGet('__WX_JOB_SELECT__',''); }
      };
  function getRoleCur() {
    try {
      var roleJ = F() && F().role;
      if (!roleJ || !roleJ.fBb) return null;
      var fzc = roleJ.fBb.fAQ || {};
      return { mgr: roleJ, confId: fzc.confId || 0, level: fzc.level || 0, group: roleJ.fBb.fAU };
    } catch (e) { return null; }
  }
  function getJobName(fcJ, id) {
    try {
      var t = fcJ && fcJ.cfgMgr && fcJ.cfgMgr.tbl;
      var rd = t && t.RoleDesTable && t.RoleDesTable.findByKey ? t.RoleDesTable.findByKey(id) : null;
      if (rd) {
        if (rd.nameText) return autoGetText(rd.nameText);
        if (rd.name) return autoGetText(rd.name);
      }
    } catch (e) {}
    return "职业#" + id;
  }
  W.__REMOTE_COMMANDS__["wx.jobs"] = function () {
    try {
      var fcJ = F();
      var r = getRoleCur();
      if (!r) return { lastAction: "jobs-no-role" };
      var curConf = r.confId, lv = r.level, grp = r.group;
      var list = [];
      try { if (typeof r.mgr.fBe === "function") list = r.mgr.fBe(grp) || []; } catch (e) {}
      var jobs = [];
      var t = fcJ.cfgMgr && fcJ.cfgMgr.tbl;
      for (var i = 0; i < list.length; i++) {
        var jid = list[i];
        var nm = getJobName(fcJ, jid);
        var unlockLv = 0;
        try {
          var rd = t && t.RoleDesTable && t.RoleDesTable.findByKey ? t.RoleDesTable.findByKey(jid) : null;
          if (rd && typeof rd.unlockLevel === "number") unlockLv = rd.unlockLevel;
        } catch (e) {}
        jobs.push({ id: jid, name: nm, unlocked: (lv >= unlockLv), unlockLevel: unlockLv });
      }
      var selStr = lsGet("__WX_JOB_SELECT__", "");
      var selArr = [];
      try { selArr = selStr.split(",").map(Number).filter(function (x) { return x > 0; }); } catch (e) {}
      var jobNames = [];
      for (var j2 = 0; j2 < jobs.length; j2++) {
        jobNames.push(jobs[j2].name + "[" + jobs[j2].id + "]" + (jobs[j2].unlocked ? "" : "(未解锁Lv" + jobs[j2].unlockLevel + ")") + (selArr.indexOf(jobs[j2].id) >= 0 ? " ✓" : ""));
      }
      var statusText = "当前职业：" + getJobName(fcJ, curConf) + " Lv" + lv;
      if (jobNames.length) statusText += "\n可转职：" + jobNames.join(" / ");
      else statusText += "\n暂无可转职职业";
      log("[JOBS] 当前conf=" + curConf + " grp=" + grp + " Lv=" + lv + " 可转职=" + JSON.stringify(jobs));
      return { lastAction: "jobs", current: curConf, jobs: jobs, jobStatus: statusText };
    } catch (e) { return { lastAction: "jobs-err", err: e.message }; }
  };
  /* ===== 转职：多选（复选框，可勾选多个职业作为候选，按顺序尝试）===== */
  W.__REMOTE_COMMANDS__["wx.jobSelect"] = function (payload) {
    try {
      var jid = payload && payload.value;
      var checked = !!(payload && payload.checked);
      var selArr = [];
      try { selArr = String(lsGet("__WX_JOB_SELECT__", "")).split(",").map(Number).filter(function (x) { return x > 0; }); } catch (e) {}
      var idx = selArr.indexOf(jid);
      if (checked) { if (idx < 0 && jid) selArr.push(jid); }
      else { if (idx >= 0) selArr.splice(idx, 1); }
      lsSet("__WX_JOB_SELECT__", selArr.join(","));
      log("[JOBS] 多选更新=" + JSON.stringify(selArr));
      return { lastAction: "jobSelect", jobId: jid, checked: checked, selected: selArr.join(",") };
    } catch (e) { return { lastAction: "jobSelect-err", err: e.message }; }
  };
  /* ===== 转职：输入目标职业ID（逗号分隔可多个）===== */
  W.__REMOTE_COMMANDS__["wx.jobInput"] = function (payload) {
    try {
      var v = payload && payload.value;
      var arr = String(v || "").split(/[,，\s]+/).map(Number).filter(function (x) { return x > 0; });
      lsSet("__WX_JOB_SELECT__", arr.join(","));
      log("[JOBS] 目标职业=" + JSON.stringify(arr));
      return { lastAction: "jobInput", selected: arr.join(",") };
    } catch (e) { return { lastAction: "jobInput-err", err: e.message }; }
  };
  /* ===== ★ v31 品质复选（出售/分解共用逻辑）=====
     品质：1普通 2高级 3稀有 4史诗 5传说 6神话
     payload: { value: 品质数字, checked: true/false, key: "__WX_SELL_Q__"|"__WX_DECOMP_Q__" } */
  W.__REMOTE_COMMANDS__["wx.quality"] = function (payload) {
    try {
      var key = (payload && payload.key) || "__WX_SELL_Q__";
      var q = parseInt(payload && payload.value, 10);
      var checked = !!(payload && payload.checked);
      if (!(q >= 1 && q <= 6)) return { lastAction: "quality-bad", value: q };
      var arr = [];
      try { arr = String(lsGet(key, "")).split(",").map(Number).filter(function (x) { return x > 0; }); } catch (e) {}
      var idx = arr.indexOf(q);
      if (checked) { if (idx < 0) arr.push(q); }
      else { if (idx >= 0) arr.splice(idx, 1); }
      arr.sort(function (a, b) { return a - b; });
      lsSet(key, arr.join(","));
      /* ★v33N 修正：品质与开关【解耦】
         · 勾品质 → 顺带把开关打开（符合直觉："我勾了就是要用"）
         · 取消勾选 → 【不】自动关开关（旧逻辑在这里把用户刚打开的开关关掉了！）
           用户要关开关自己去关。 */
      var swKey = (key === "__WX_DECOMP_Q__") ? "__WX_AUTO_DECOMPOSE__" : "__WX_AUTO_SELL__";
      var swTouched = false;
      if (checked && arr.length > 0) {
        lsSet(swKey, "1"); W[swKey] = true; swTouched = true;
      }
      log("[QUALITY] " + key + " = [" + arr.join(",") + "] 开关 " + swKey +
          "=" + (wxFlag(swKey) ? "开" : "关") + (swTouched ? "(因勾选自动开)" : "(未改动)"));
      return { lastAction: "quality", key: key, selected: arr.join(","),
               switchKey: swKey, switchOn: wxFlag(swKey), switchTouched: swTouched };
    } catch (e) { return { lastAction: "quality-err", err: e.message }; }
  };
  /* ===== ★ v32 手动触发：立即执行一次 出售/分解/穿戴/技能/转职 ===== */
  W.__REMOTE_COMMANDS__["wx.cleanNow"] = function (payload) {
    var what = (payload && payload.value) || "all";
    var r = { lastAction: "cleanNow", what: what };
    try {
      if (what === "sell" || what === "all") { r.sell = autoSellDecomposeNow(); }
      if (what === "sd") { r.sd = autoSellDecomposeNow(); r.sdDump = (W.__WX_SDQ__ || {}); }
      if (what === "wear" || what === "all") { r.wear = autoWearBestEquip(0); }
      if (what === "skill" || what === "all") { r.skill = autoUpgradeSkills(); }
      if (what === "job" || what === "all") { r.job = autoDoJobChange(); }
    } catch (e) { r.err = "" + e.message; }
    return r;
  };

  /* ===== ★ v32 配置自检：一眼看清"开关/品质/游戏内 flag"是否一致 =====
     用途：排查"UI 开了但没效果"。输出：
       · localStorage 开关值（wx.flag 写入）
       · 品质勾选列表
       · 游戏内 localMgr.autoSellEquipFlag / autoDecomposeEquipFlag 的实际位
       · localMgr.cnp(q)/cnq(q) 逐品质的判定结果 */
  W.__REMOTE_COMMANDS__["wx.cfgDiag"] = function () {
    var d = { lastAction: "cfgDiag" };
    try {
      d.switch = {
        sell: String(lsGet("__WX_AUTO_SELL__", "?")),
        decompose: String(lsGet("__WX_AUTO_DECOMPOSE__", "?")),
        wear: String(lsGet("__WX_AUTO_WEAR__", "?")),
        skill: String(lsGet("__WX_AUTO_SKILL__", "?")),
        job: String(lsGet("__WX_AUTO_JOBSWITCH__", "?"))
      };
      d.quality = { sell: String(lsGet("__WX_SELL_Q__", "?")), decompose: String(lsGet("__WX_DECOMP_Q__", "?")) };
      var fc = F();
      var lm = fc && fc.localMgr;
      if (lm && (lm.cms || typeof lm.cnp === "function")) {
        d.gameFlag = { sell: (lm.cms && lm.cms.autoSellEquipFlag) || 0, decompose: (lm.cms && lm.cms.autoDecomposeEquipFlag) || 0 };
        var sq = [], dq = [];
        try {
          for (var q = 1; q <= 6; q++) {
            if (typeof lm.cnp === "function" && lm.cnp(q)) sq.push(q);
            if (typeof lm.cnq === "function" && lm.cnq(q)) dq.push(q);
          }
        } catch (e) {}
        d.gameQual = { sell: sq.join(","), decompose: dq.join(",") };
      } else d.gameFlag = "localMgr 未就绪";
      d.api = {
        cnr: !!(lm && typeof lm.cnr === "function"),
        bagSell: !!(fc && fc.bag && typeof fc.bag.bYK === "function"),
        bagDecompose: !!(fc && fc.bag && typeof fc.bag.bYJ === "function"),
        equipBestList: !!(fc && fc.equip && typeof fc.equip.ftA === "function"),
        equipQuickWear: !!(fc && fc.equip && typeof fc.equip.fvH === "function"),
        skillUpgrade: !!(fc && fc.skill && typeof fc.skill.cuw === "function"),
        roleJobChange: !!(fc && fc.role && typeof fc.role.fBm === "function")
      };
      autoLog("[CFGDIAG] " + JSON.stringify(d));
    } catch (e) { d.err = "" + (e && e.message); }
    return d;
  };

  /* ===== 自动主线诊断命令 ===== */
  W.__REMOTE_COMMANDS__["wx.autoDiag"] = function () {
    try {
      var info = { scene: "?", battle: "?", town: "?", task: "?" };
      var fc = F();
      // 战斗状态
      try {
        var bm = fc && fc.battleMgr;
        var JE = bm && bm.bEu;
        var inBattle = !!(JE && typeof JE.no === "function" && JE.no());
        info.battle = inBattle ? "IN_BATTLE" : "NOT_BATTLE";
        if (inBattle && bm && bm.bCK) info.settle = bm.bCK.b;
        if (inBattle && JE && JE.ny) {
          var me = JE.ny();
          if (me) { info.dcv = me.dcv; info.myInst = me.bCD && me.bCD.instanceId; }
        }
      } catch (e) { info.battle += " err:" + e.message; }
      // 城镇状态
      try {
        var ts = fc && fc.townScene;
        info.town = ts ? "OK" : "NULL";
      } catch (e) { info.town += " err:" + e.message; }
      // 1.1.3.2 主线任务缓存
      try {
        var task = fc && fc.task;
        var taskApi = resolveTaskApi(task);
        if (taskApi) {
          var grp = taskApi.getGroup.call(taskApi.grpMgr, { taskType: 3 });
          if (grp && grp.taskList) {
            var cur = null;
            for (var i = 0; i < grp.taskList.length; i++) {
              var tk = grp.taskList[i];
              // $pA: b=0 未开始 c=1 进行中 d=2 可提交 e=3 已完成
              if (tk.status === 1 || tk.status === 2) { cur = tk; break; }
            }
            info.task = "grp=OK count=" + grp.taskList.length + (cur ? " cur=" + cur.id + " st=" + cur.status + " pro=" + cur.process : " allDone");
            if (cur) {
              try {
                var cfg = taskApi.getCfg.call(taskApi.grpMgr, cur.id);
                if (cfg) {
                  info.taskCfg = {};
                  for (var kk in cfg) {
                    var vv = cfg[kk];
                    if (typeof vv !== "function") {
                      if (vv && typeof vv === "object") {
                        try { info.taskCfg[kk] = JSON.stringify(vv).slice(0, 200); } catch (e) { info.taskCfg[kk] = "[obj]"; }
                      } else info.taskCfg[kk] = vv;
                    }
                  }
                }
              } catch (e) {}
              // 同时 dump 到日志文件，方便查看
              try {
                var w2 = jsb && jsb.fileUtils;
                if (w2 && typeof w2.writeStringToFile === "function") {
                  var p2 = (w2.getWritablePath ? w2.getWritablePath() : "/data/user/0/com.fightingstars.games.warrior/files/") + "wx_task_cfg.txt";
                  var all2 = "taskId=" + cur.id + "\ncfg=" + JSON.stringify(info.taskCfg) + "\ntask=" + JSON.stringify(cur.task) + "\n";
                  w2.writeStringToFile(all2, p2);
                }
              } catch (e) {}
            }
          } else info.task = "grp=NULL";
        } else info.task = "task/cvg NULL";
      } catch (e) { info.task += " err:" + e.message; }
      log("[AUTODIAG] " + JSON.stringify(info));
      return { lastAction: "autoDiag", info: info };
    } catch (e) {
      return { lastAction: "autoDiag-err", err: e.message };
    }
  };

  /* ===== 当前面板方法 dump（★ 2026-09-23 新增）
     用途：拿到面板实例的真实方法名/属性名（如 RewardPanel 的关闭方法）。
     输出：顶层面板名 + 实例的所有方法名 + 关键属性值。 */
  W.__REMOTE_COMMANDS__["wx.panelDiag"] = function () {
    var d = { ts: Date.now() };
    try {
      var fc = F();
      var fgm = fc && fc.fguiMgr;
      if (!fgm) { d.err = "no-fguiMgr"; return { lastAction: "panelDiag", info: d }; }
      var p = null;
      try { p = fgm.cTU ? fgm.cTU() : null; } catch (e) {}
      if (!p) { d.err = "no-panel"; return { lastAction: "panelDiag", info: d }; }
      var inst = p;
      d.viewName = String((p.vJ && p.vJ.cUV) || "");
      // 收集实例自身的方法（含原型链一层）
      var methods = [], props = [];
      try {
        for (var k in inst) {
          try {
            var v = inst[k];
            if (typeof v === "function") methods.push(k);
            else {
              // 只记录简单值，避免大对象
              var tv = typeof v;
              if (tv === "string" || tv === "number" || tv === "boolean" || v === null) props.push(k + "=" + String(v));
              else if (v && tv === "object") {
                var cs = Object.prototype.toString.call(v);
                props.push(k + ":" + cs.replace(/\[object |\]/g, ""));
              }
            }
          } catch (e) {}
        }
      } catch (e) {}
      d.methods = methods;
      d.props = props.slice(0, 40);
      // 回调对象 dHw 探测
      try { d.has_dHw = !!inst.dHy; if (inst.dHy) d.dHwKeys = Object.keys(inst.dHy); } catch (e) {}
      try { d.has_continue = !!inst.continue; } catch (e) {}
      try { d.has_mask = !!inst._mask; } catch (e) {}
      log("[PANELDIAG] " + JSON.stringify(d));
    } catch (e) { d.err = "" + (e && e.message); }
return { lastAction: "panelDiag", info: d };
  };

  /* ===== 全量 API 探测（★ 2026-09-24 新增）
     用途：热更新后混淆名会变，用它 dump 出关键管理器的「真实方法/属性名」，
           以便对照修正脚本里的硬编码混淆名。
     输出：task / battleMgr / role / townScene / fguiMgr / battleMgr.bEu 等
           各自的方法名列表 + 关键属性探测。 */
  W.__REMOTE_COMMANDS__["wx.apiDiag"] = function () {
    var d = {};
    try {
      var fc = F();
      if (!fc) { d.err = "no-FC"; return { lastAction: "apiDiag", info: d }; }
      d.fcKeys = Object.keys(fc).slice(0, 40);
      // 各管理器的方法名（取前 60 个，按名排序便于比对）
      var mgrs = ["task", "battleMgr", "role", "townScene", "fguiMgr", "bag", "equip", "troop", "chapter", "destroyTower", "guide", "monCard", "localMgr", "gameHelper", "gsMgr"];
      for (var i = 0; i < mgrs.length; i++) {
        var m = fc[mgrs[i]];
        if (!m) { d[mgrs[i]] = "null"; continue; }
        var fns = [];
        try { for (var k in m) { try { if (typeof m[k] === "function") fns.push(k); } catch (e) {} } } catch (e) {}
        d[mgrs[i]] = fns.sort().slice(0, 60).join(",");
      }
      // battleMgr 内的战斗控制器
      try {
        var bm = fc.battleMgr;
        var inner = [];
        for (var k2 in bm) {
          try {
            var v2 = bm[k2];
            if (v2 && typeof v2 === "object" && (v2.ne || v2.mz || v2.my || v2.mw)) inner.push(k2);
          } catch (e) {}
        }
        d.battleMgr_innerCtrl = inner.join(",");
      } catch (e) { d.battleMgr_innerCtrl = "err:" + e.message; }
      // 1.1.3.2 task.cvg 缓存的方法
      try {
        var th = fc.task && fc.task.cvg;
        if (th) { var tf = []; for (var k3 in th) { try { if (typeof th[k3] === "function") tf.push(k3); } catch (e) {} } d.task_cvg = tf.sort().join(","); }
        else d.task_cvg = "null";
      } catch (e) { d.task_cvg = "err:" + e.message; }
      log("[APIDIAG] " + JSON.stringify(d));
    } catch (e) { d.err = "" + (e && e.message); }
    return { lastAction: "apiDiag", info: d };
  };

  /* ===== 副本跳转/类型反查诊断（★ 2026-09-23 新增）
     用途：排查"主线任务跳去错误系统（如远古副本/未解锁的塔）"。
     输出当前任务的 param1/param2/jumpId、JumpTable 详情、InstanceTable 反查结果、
     InstanceTypeTable 结构样例，一次看全局。 */
  W.__REMOTE_COMMANDS__["wx.instDiag"] = function () {
    var d = {};
    try {
      var fc = F();
      var task = fc && fc.task;
      var tbl = fc && fc.cfgMgr && fc.cfgMgr.tbl;
      // 1. 当前主线任务配置
      try {
        var cur = autoGetCurrentTask();
        if (cur && cur.cfg) {
          var c = cur.cfg;
          d.task = { id: c.id, eventType: c.eventType, param1: c.param1, param2: c.param2, param3: c.param3, jumpId: c.jumpId, nextId: c.nextId };
          d.taskAnalysis = autoAnalyzeTask(cur);
        } else d.task = "none";
      } catch (e) { d.task = "err:" + e.message; }
      // 2. JumpTable 详情
      try {
        var jid = (d.task && d.task.jumpId) || 0;
        if (jid && tbl && tbl.JumpTable && tbl.JumpTable.findByKey) {
          var jp = tbl.JumpTable.findByKey(jid);
          if (jp) {
            d.jump = {};
            for (var k in jp) { var v = jp[k]; if (typeof v !== "function") { try { d.jump[k] = (v && typeof v === "object") ? JSON.stringify(v) : v; } catch (e) {} } }
          } else d.jump = "null";
        } else d.jump = "no-jid-or-table";
      } catch (e) { d.jump = "err:" + e.message; }
      // 3. InstanceTable 反查（副本 id 是否存在）
      try {
        var iid = (d.taskAnalysis && d.taskAnalysis.instId) || 0;
        if (iid && tbl && tbl.InstanceTable && tbl.InstanceTable.findByKey) {
          var it = tbl.InstanceTable.findByKey(iid);
          d.instance = it ? { id: it.id, name: it.name, instanceType: it.instanceType, typeId: it.typeId, jumpId: it.jumpId } : "not-in-InstanceTable";
        } else d.instance = "no-id-or-table";
      } catch (e) { d.instance = "err:" + e.message; }
      // 4. InstanceTypeTable 全部条目（前 20 个）
      try {
        var itt = tbl && tbl.InstanceTypeTable;
        if (itt) {
          var arr = [];
          if (itt.list && itt.list.length) arr = itt.list;
          else if (itt.findFirst) { for (var i = 1; i <= 20; i++) { var x = itt.findFirst(function(y){ return y.id === i; }); if (x) arr.push(x); } }
          d.instanceTypeTable = arr.slice(0, 20).map(function(x){ return { id: x.id, name: x.name, funcOpen: x.funcOpen, funcId: x.funcId, typeId: x.typeId, sceneType: x.sceneType }; });
        } else d.instanceTypeTable = "no-table";
      } catch (e) { d.instanceTypeTable = "err:" + e.message; }
      log("[INSTDIAG] " + JSON.stringify(d));
    } catch (e) { d.err = "" + (e && e.message); }
    return { lastAction: "instDiag", info: d };
  };

  /* ===== UI 通道测试（★ 2026-09-23 新增，排查"JS 推了但 UI 不刷新"）
     依次用 3 种方式向 autoStatus 推同一个标记文本，观察 UI 最终显示哪一种、
     或是否都不显示，以定位是"桥接方法不对"还是"控件没绑定"。
     用法：调用后在 UI 的"自动任务"页看 autoStatus 文本。 */
  W.__REMOTE_COMMANDS__["wx.uiTest"] = function () {
    var r = { ts: Date.now(), steps: [] };
    try {
      var tag = "T" + String(r.ts).slice(-4);
      // 方式1：mergeState（原地刷新 refreshBoundViews）
      try {
        nativeCall("mergeState", "(Ljava/lang/String;)V", JSON.stringify({ autoStatus: "M" + tag }));
        r.steps.push("mergeState->M" + tag);
      } catch (e) { r.steps.push("mergeState ERR:" + e.message); }
      // 方式2：updateState（仅存值，不刷新）
      try {
        nativeCall("updateState", "(Ljava/lang/String;)V", JSON.stringify({ autoStatus: "U" + tag }));
        r.steps.push("updateState->U" + tag);
      } catch (e) { r.steps.push("updateState ERR:" + e.message); }
      // 方式3：updateUi（源码里存在的方法，可能是"刷新UI"专用）
      try {
        nativeCall("updateUi", "(Ljava/lang/String;)V", JSON.stringify({ autoStatus: "X" + tag }));
        r.steps.push("updateUi->X" + tag);
      } catch (e) { r.steps.push("updateUi ERR:" + e.message); }
      // 方式4：mergeState 再补一次（最后的赢家应显示在 UI 上）
      try {
        nativeCall("mergeState", "(Ljava/lang/String;)V", JSON.stringify({ autoStatus: "FIN-" + tag }));
        r.steps.push("mergeState(FIN)->FIN-" + tag);
      } catch (e) { r.steps.push("mergeState FIN ERR:" + e.message); }
      log("[UITEST] " + JSON.stringify(r));
    } catch (e) { r.err = "" + e.message; }
    return { lastAction: "uiTest", info: r };
  };
  W.__REMOTE_COMMANDS__["wx.autoStateDiag"] = function () {
    var d = {};
    try {
      d.autoEnabled = AUTO.enabled;                          // 状态机真实是否开启
      d.autoState = AUTO.state;                              // 状态机当前状态
      d.autoLog = AUTO.log;                                  // 最近一条日志
      d.lsSwitch = lsGet("__WX_AUTO_TASK__", "?");           // localStorage 开关值
      d.autoObjPersist = !!(W.__WX_AUTO_OBJ__ && W.__WX_AUTO_OBJ__ === AUTO);  // AUTO 是否全局持久
      d.installTimer = !!W.__WX_INSTALL_TIMER__;             // 单例定时器是否建立
      d.stateStats = __WX_ST.stats;                          // state 推送统计
      d.curTaskId = AUTO.curTaskId;
      d.curDesc = AUTO.curDesc;
      d.curProgress = AUTO.curProgress;
      log("[AUTOSTATEDIAG] " + JSON.stringify(d));
    } catch (e) { d.err = "" + (e && e.message); }
    return { lastAction: "autoStateDiag", info: d };
  };

  /* ===== 自动主线任务系统 =====
     状态机：
       IDLE → 城镇读任务 → 判断目标类型 → 寻路 → 到达 → 交互/进副本 → 副本内自动战斗 → 通关 → 结算 → 回城 → 循环
     1.1.3.2 核心接口：task.cvs/cvg.cva/cvk、townScene.fGW、battleMgr.bFo/bEu/bEv。
   */
  /* ★ 2026-09-23 修复（UI 不刷新根因）：
     AUTO 必须挂在 globalThis 上持久化。原实现 var AUTO = {...} 是脚本局部变量，
     只要 remote.js 被重新注入（热更新 / keep-alive 重载 / 守卫被清），就会 new 一个新 AUTO，
     其 __inited 为 undefined → 触发"强制关闭"分支 → 把用户刚开启的 enabled 复位成 false，
     于是状态机被 if(!AUTO.enabled) return 掐断：人物靠其他 hook 继续跑，但 autoStatus/
     autoLog/任务信息永远停在初始值。改为复用全局对象后，重注入不会丢 enable 状态。 */
  var AUTO = W.__WX_AUTO_OBJ__ || (W.__WX_AUTO_OBJ__ = {
    enabled: false,
    state: "IDLE",        // IDLE / READ_TASK / GOTO / IN_BATTLE / SETTLE / SUBMIT / AUTO_STRENGTH
    lastAction: "",
    lastTick: 0,
    step: 0,
    curTaskId: 0,
    curTaskIndex: 0,
    curDesc: "",
    curTarget: "",
    curProgress: "0/0",
    gotoInfo: null,       // 当前寻路目标
    gotoStart: 0,
    battleStart: 0,
    settleStart: 0,
    strengthStart: 0,
    strengthLast: 0,
    log: ""
  });
  var AUTO_LOG_THROTTLE = { t: 0, msg: "" };

  /* 1.1.3.2 进入副本唯一入口。 */
  function battleEnter(instId, playMode) {
    try {
      var fc = F();
      var bm = fc && fc.battleMgr;
      if (!bm || !instId || typeof bm.bFo !== "function") return false;
      var rr = null;
      try {
        rr = bm.bFo({ id: instId, playMode: playMode == null ? 0 : playMode });
        if (rr && rr.then) {
          rr.then(function (res) {
            log("[enter] 进副本 Promise 返回: " + JSON.stringify(res && res.code !== undefined ? { code: res.code, desc: res.desc } : res));
          }, function (err) { log("[enter] 进副本 Promise 失败: " + err); });
        }
      } catch (e) { log("[enter] 进副本调用异常: " + (e && e.message)); }
      return true;
    } catch (e) { log("[enter] 异常: " + (e && e.message)); return false; }
  }
  function autoLog(t) {
    try {
      var nowT = Date.now();
      var sameMsg = (t === AUTO_LOG_THROTTLE.msg && (nowT - AUTO_LOG_THROTTLE.t) < 2000);
      AUTO.log = t;
      if (!sameMsg) {
        AUTO_LOG_THROTTLE.msg = t;
        AUTO_LOG_THROTTLE.t = nowT;
      }
      // ★ 2026-09-23 三次修正（防闪烁）：UI 推送节流到 1 秒。
      //   状态机运行时 autoLog 可能每 300ms 就变一次，高频推送会让面板文本持续重绘
      //   （配合 updateState 时更会整体重建 → 闪烁）。这里限制最多 1 秒推一次 UI，
      //   最新内容仍保留在 AUTO.log（tick 每轮也会统一推一次，不会丢）。
      if ((AUTO.__logPushT || 0) + 1000 <= nowT) {
        AUTO.__logPushT = nowT;
        state({ autoLog: AUTO.log, autoStatus: AUTO.enabled ? ("运行中·" + AUTO.state) : "已停止" });
      }
    } catch (e) {}
    log("[AUTO] " + t);
    // 也写入诊断文件
    try {
      var w = jsb && jsb.fileUtils;
      if (w && typeof w.writeStringToFile === "function") {
        var p = (w.getWritablePath ? w.getWritablePath() : "/data/user/0/com.fightingstars.games.warrior/files/") + "wx_auto_log.txt";
        var old = "";
        try { if (typeof w.getStringFromFile === "function") old = w.getStringFromFile(p) || ""; } catch (e) {}
        var all = (old + "[" + new Date().toISOString().slice(11,19) + "] " + t + "\n");
        if (all.length > 8000) all = all.slice(-6000);
        w.writeStringToFile(all, p);
      }
    } catch (e) {}
  }
  /* 1.1.3.2 任务缓存：task.cvg；读组：task.cvs；拉组：task.cvo。 */
  var TXAPI = null;
  function resolveTaskApi(task) {
    if (TXAPI) return TXAPI;
    if (!task || !task.cvg || typeof task.cvs !== "function") return null;
    TXAPI = {
      grpMgr: task.cvg,
      getGroup: function (idx) { return task.cvs(idx); },
      getCfg: function (id) {
        if (task.cvg && typeof task.cvg.cva === "function") return task.cvg.cva(id);
        var tbl = F() && F().cfgMgr && F().cfgMgr.tbl;
        return tbl && tbl.TaskTable && tbl.TaskTable.findByKey ? tbl.TaskTable.findByKey(id) : null;
      },
      pull: typeof task.cvo === "function" ? task.cvo : null
    };
    return TXAPI;
  }

  /* 控制器 no() 返回当前是否在战斗。 */
  function hasBattleActive(JE) {
    try {
      return !!(JE && typeof JE.no === "function" && JE.no());
    } catch (e) { return false; }
  }
  /* 1.1.3.2 结算结果：控制器.mz.bDK / battleMgr.bEv.bDK。 */
  function getSettle() {
    try {
      var fc = F(); if (!fc) return null;
      var Kd = fc.battleMgr && fc.battleMgr.bEu;
      var mz = Kd && Kd.mz;
      if (mz && mz.bDK) return mz.bDK;
      var bm = fc.battleMgr;
      if (bm && bm.bEv && bm.bEv.bDK) return bm.bEv.bDK;
      return null;
    } catch (e) { return null; }
  }
  /* 1.1.3.2 拉取任务组。 */
  var TASK_REFRESH_T = 0;
  function refreshTaskGroup(force) {
    try {
      var nowT = Date.now();
      if (!force && nowT - TASK_REFRESH_T < 4000) return false;
      TASK_REFRESH_T = nowT;
      var task = F() && F().task;
      if (!task) return false;
      var idx = { taskType: 3 };
      if (typeof task.cvo !== "function") return false;
      task.cvo(idx);
      AUTO.__taskRefreshAt = nowT;
      return true;
    } catch (e) { return false; }
  }
  function autoGetCurrentTask() {
    try {
      var fc = F();
      var task = fc && fc.task;
      if (!task) return null;
      var roleMgr = fc && fc.role;
      var roleReady = !!(roleMgr && roleMgr.fBb && roleMgr.fBb.fAQ);
      var api = resolveTaskApi(task);
      var grp = null;
      var idx = { taskType: 3 };  // MAINLINE_TASK=3
      try {
        if (api && api.getGroup) grp = api.getGroup.call(api.grpMgr, idx);
      } catch (e) {}
      if (!grp || !grp.taskList) {
        if (roleReady) refreshTaskGroup(false);
        return null;
      }
      for (var i = 0; i < grp.taskList.length; i++) {
        var tk = grp.taskList[i];
        if (tk.status === 1 || tk.status === 2) {
          var cfg = null;
          try {
            if (api && api.getCfg) cfg = api.getCfg.call(api.grpMgr, tk.id);
          } catch (e) {}
          /* v25f：服务端需要原始 taskIndex（{taskType, actId?, actCycle?}）
             源码 cuR: ""+taskType，有 actId 时拼 "actId_actCycle"。
             自己拼 {taskType:3} 会丢 actId/actCycle → 参数错误 */
          return { task: tk, index: i, taskIndex: (grp.taskIndex || idx),
                   rawTaskIndex: grp.taskIndex || null, taskGroup: grp, cfg: cfg };
        }
      }
      return null;  // 主线全部完成
    } catch (e) { return null; }
  }
  /* 判断任务目标类型：返回 {type:'npc'|'instance'|'submit'|'unknown', info}。 */
  var I18N_FN = null;
  function autoGetText(id) {
    try {
      if (id == null) return "";
      if (typeof id === "string" && id.indexOf("#") !== 0) {
        // 已是文本（非纯数字 id）？也可能是文本id数字字符串，仍尝试翻译
      }
      var i18n = F() && F().i18n;
      if (i18n) {
        if (!I18N_FN) {
          var cands = ["Mv", "Mw"];
          for (var i = 0; i < cands.length; i++) {
            if (typeof i18n[cands[i]] === "function") { I18N_FN = i18n[cands[i]]; break; }
          }
        }
        if (I18N_FN) {
          var t = I18N_FN.call(i18n, id);
          if (t && typeof t === "string" && t.length > 0 && t !== "" + id && t.indexOf("\uFFFD") < 0) return t;
        }
      }
      return "" + id;
    } catch (e) { return "" + id; }
  }
  function autoAnalyzeTask(cur) {
    try {
      if (!cur) return { type: "unknown" };
      var tk = cur.task, cfg = cur.cfg;
      // status=2 可提交 → 提交
      if (tk.status === 2) return { type: "submit", taskId: tk.id, taskIndex: cur.index };
      if (!cfg) return { type: "unknown" };
      var desc = cfg.desc ? autoGetText(cfg.desc) : ("任务 #" + tk.id);
      var name = cfg.name ? autoGetText(cfg.name) : desc;
      var target = "";
      var et = cfg.eventType;
      var arr0 = function (a) { try { if (a && typeof a === "object" && a.length) return parseInt(a[0], 10); if (typeof a === "number") return a; } catch (e) {} return 0; };
      /* 按 eventType 分类（EventTableEnum，已从源码 main/index.js 35872-35956 确认）
         1000 ChallengeInstance 挑战副本 → ★ 副本id 在 param2[0]（源码 47626/47677 行确认）
         1008 EnterInstance     进副本   → 源码业务层从不处理，语义靠服务端；
                                            param1[0] 可能是"副本id或类型id"，需 InstanceTable 校验
         2100 TalkNpc           NPC对话  → param1[0]=NPCid
       */
      if (et === 1000) {
        // 官方写法：只从 param2 取（param1 不是副本id）
        var instId1000 = arr0(cfg.param2);
        var instJump1000 = cfg.jumpId || 0;
        if (instId1000) return { type: "instance", instId: instId1000, desc: desc, name: name, jumpId: instJump1000, eventType: et };
        // param2 缺失时兜底 param1（但要标记，交由进副本前校验）
        var instId1000b = arr0(cfg.param1);
        if (instId1000b) return { type: "instance", instId: instId1000b, desc: desc, name: name, jumpId: instJump1000, eventType: et, fallbackParam1: true };
      }
      if (et === 1008) {
        /* ★ 2026-09-23：1008 的关键 —— 先判断 param1 是不是"真实副本id"。
           实测任务 param1=[1200]，而 1200 是 InstanceTypeTableEnum.TOWER（塔类型id），
           不是 InstanceTable 里的副本 → 旧代码把它当 instId 硬进，跳错系统。
           处理：param1/param2 分别校验 InstanceTable，
             命中 → 当作副本任务（type=instance）；
             未命中但有 jumpId → 当作"玩法跳转任务"（type=guide，只跳转不硬进）。 */
        var fcA = F();
        var tblA = fcA && fcA.cfgMgr && fcA.cfgMgr.tbl;
        var checkInst = function (v) {
          if (!v || !tblA || !tblA.InstanceTable || !tblA.InstanceTable.findByKey) return false;
          try { return !!tblA.InstanceTable.findByKey(v); } catch (e) { return false; }
        };
        var p1 = arr0(cfg.param1), p2 = arr0(cfg.param2);
        var eJump = cfg.jumpId || 0;
        if (p2 && checkInst(p2)) return { type: "instance", instId: p2, desc: desc, name: name, jumpId: eJump, eventType: et };
        if (p1 && checkInst(p1)) return { type: "instance", instId: p1, desc: desc, name: name, jumpId: eJump, eventType: et };
        // 都不在副本表：1008 更像"进入某玩法"（塔/秘境等），用跳转完成
        if (eJump) return { type: "guide", desc: desc, name: name, npcId: 0, jumpId: eJump, eventType: et, note: "1008-玩法跳转" };
        // 无 jumpId 也无有效副本 → 未知
        return { type: "unknown", desc: desc, name: name, eventType: et };
      }
      if (et === 2100) {
        var npcId = arr0(cfg.param1);
        var jumpId = cfg.jumpId || 0;
        return { type: "npc", desc: desc, name: name, npcId: npcId, jumpId: jumpId };
      }
      // ★ 引导完成：GuideDone=901（需要跳转到对应界面完成引导，如交易行）
      //   param1=[NPCid]（可选），jumpId=跳转目标（JumpTable）
      //   处理：跳转到 jumpId 对应界面 → 引导完成（服务端判定）→ 任务推进
      if (et === 901) {
        var gNpcId = arr0(cfg.param1);
        var gJumpId = cfg.jumpId || 0;
        return { type: "guide", desc: desc, name: name, npcId: gNpcId, jumpId: gJumpId };
      }
      // 转职任务由当前版本的 getRoleCur()/fBe()/fBm() 链路处理。
      if (et === 106) {
        var jNpcId = arr0(cfg.param1);
        return { type: "jobChange", desc: desc, name: name, npcId: jNpcId };
      }
      // 功能开启：FuncOpen=900（类似引导，跳转解锁新功能界面）
      if (et === 900) {
        var fNpcId = arr0(cfg.param1);
        var fJumpId = cfg.jumpId || 0;
        return { type: "guide", desc: desc, name: name, npcId: fNpcId, jumpId: fJumpId };
      }
      // 强化装备任务：EquipUpgradeTimes=1600 / EquipUpgradeTo=1601 / EquipAllUpgradeTo=1608
      /* ★★ v25d 关键修正（TaskTable 字段映射已从 main/index.js 逐位确认）：
           eventType = e[13]，num = e[14]，jumpId = e[21]
           实测任务 #33100110（附魔任务）：
             eventType = ★1606   （不是 1990706！1990706 是它的 jumpId，即"跳转到附魔界面"）
             num       = 1      （只需 1 次）
             jumpId    = 1990706
           所以装备类任务用 16xx 这一组；1990xxx 只是 jumpId/红点。 */
      /* v25e 权威映射（main/index.js EventTableEnum 逐条确认）：
           1600 EquipUpgradeTimes  强化次数   1601 EquipUpgradeTo 强化到等级
           1602 EquipDecompose     分解       1603 EquipBlessTimes 祝福
           1604 EquipSell          出售       1605 EquipCraft 打造
           1606 EquipSoulEnchant   ★附魔      1607 EquipAdjustment 品级调整
           1608 EquipAllUpgradeTo  全部强化    1609 GemInlay ★宝石镶嵌 */
      if (et === 1606) return { type: "enchant", desc: desc, name: name, eventType: et };
      if (et === 1609) return { type: "gev", desc: desc, name: name, eventType: et };
      if (et === 1600 || et === 1601 || et === 1608) {
        return { type: "strengthen", desc: desc, name: name, target: (et === 1600 ? "次数" : "等级"), eventType: et };
      }
      if (et === 1602) return { type: "decompose", desc: desc, name: name, eventType: et };
      if (et === 1603) return { type: "bless", desc: desc, name: name, eventType: et };
      if (et === 1604) return { type: "sell", desc: desc, name: name, eventType: et };
      if (et === 1605) return { type: "craft", desc: desc, name: name, eventType: et };
      if (et === 1607) return { type: "adjust", desc: desc, name: name, eventType: et };
      /* ★ v25 新增：装备类事件（TblEnum.EventTableEnum，已从 main/index.js 确认）
         1990700 EQUIP_ENHANCE  装备强化
         1990704 GEM_INLAY     宝石镶嵌
         1990706 SOUL_ENCHANT  ★装备附魔（"魂"镶嵌，itemType SOUL=16）
         1990708 FUSION_STONE   刻印石
         这些用 equip 管理器的官方"一键"接口完成，见状态机 ENCHANT/STRENGTH 分支 */
      /* 1990xxx 属于 jumpId/红点体系（保留兜底，优先级低于上面的 16xx/11xx） */
      if (et === 1990706) return { type: "enchant", desc: desc, name: name, eventType: et, viaJumpId: true };
      if (et === 1990700) return { type: "strengthen", desc: desc, name: name, target: "等级", eventType: et, viaJumpId: true };
      if (et === 1990704 || et === 1990708) return { type: "gev", desc: desc, name: name, eventType: et, viaJumpId: true };
      /* ★★★ v33g 重大修正：1101/1102/1103/1308 从来就不是【任务事件】！
         它们全部来自 ReddotTableEnum（红点表，game/index.js:227520-227530）：
           1101 EQUIP_WEAR / 1102 EQUIP_STRENGTHEN / 1103 EQUIP_GEM / 1308 EQUIP_FUSION
         （旧代码把它们当 EventTableEnum 用 → 完全映射错误）
         而 EventTableEnum（main/index.js EventTableEnum 段，已逐条核对）里：
           1100 = GiveFriendReward  赠送好友奖励
           1101 = ReceiveFriendReward 领取好友奖励
           1102 = AddFriend         添加好友
           1103 / 1308 = 【不存在】
         正确做法：1100/1101/1102 走好友事件（当前环境无好友系统 → 上报推进） */
      if (et === 1100) return { type: "clientEvent", desc: desc, name: name, eventType: et, note: "赠送好友奖励" };
      if (et === 1101) return { type: "clientEvent", desc: desc, name: name, eventType: et, note: "领取好友奖励" };
      if (et === 1102) return { type: "clientEvent", desc: desc, name: name, eventType: et, note: "添加好友" };
      // 章节星级奖励：ChapterStarReward=1007，param1=[chapterId]
      if (et === 1007) {
        var chId = arr0(cfg.param1);
        return { type: "chapterReward", desc: desc, name: name, chapterId: chId };
      }
      // 药水配置：ConfigMedicine=108（需要实际配置血药x2+蓝药x1）
      if (et === 108) {
        return { type: "medicine", desc: desc, name: name, eventType: et };
      }
      /* ★ v30 新增：商店购买物品 ShopBuyGoods=801
           event_config[801] = "商店购买物品" param1=商店类型 param2=商品ID
           实例：9981001（账号永久任务）evt=801 p1=[7] p2=70900 → 购买"试用月卡"(0元)
           这是"激活银卡特权"主线的前置任务。 */
      if (et === 801) {
        var sType = arr0(cfg.param1);
        var gId = (typeof cfg.param2 === "number") ? cfg.param2 : arr0(cfg.param2);
        return { type: "shopBuy", desc: desc, name: name, eventType: et, shopType: sType, goodsId: gId };
      }
      /* ★ v30 新增：完成任务 CompleteTask=300
           event_config[300] = "完成任务" param1=任务类型 param2=[目标任务ID]
           实例：33100230（主线"领取月卡特权"）evt=300 p1=[900] p2=[9981001]
           → 去查 9981001 的配置，按它的要求做（这里是 801 商店购买）。 */
      if (et === 300) {
        var refId = arr0(cfg.param2);
        var refCfg = null;
        try {
          var t3 = F() && F().cfgMgr && F().cfgMgr.tbl && F().cfgMgr.tbl.TaskTable;
          if (t3 && t3.findByKey) refCfg = t3.findByKey(refId);
        } catch (e) {}
        if (refCfg && refCfg.eventType === 801) {
          var sType2 = arr0(refCfg.param1);
          var gId2 = (typeof refCfg.param2 === "number") ? refCfg.param2 : arr0(refCfg.param2);
          return { type: "shopBuy", desc: desc, name: name, eventType: 801, shopType: sType2, goodsId: gId2, refTaskId: refId };
        }
        // 目标任务是"特权激活"(104) 等 → 上报事件推进
        return { type: "clientEvent", desc: desc, name: name, eventType: et, refTaskId: refId,
                 refEventType: refCfg ? refCfg.eventType : null, derived: true };
      }
      /* 特权激活 OnBuffActivated=104（购买月卡/试用后由服务端触发，客户端无需处理） */
      if (et === 104) {
        return { type: "clientEvent", desc: desc, name: name, eventType: et, derived: true };
      }
      /* ===== v33g 新增：宠物蛋孵化（PetEggHatch=1503）=====
         源码链路（game/index.js）：
           蛋 = BagTableEnum.BOX(102) 物品（BoxTable.subType === BoxTypeTableEnum.PET_BOX）
           红点判定 cqm()：遍历背包 BOX，取 BoxTable.findByKey(itemId)，
                          subType === PET_BOX 且 costItems 足够 → 可孵化
           孵化 = $eZ.openBox.cpI({itemId, amount}) → 协议 SprotocolMap.OpenBox.open
         所以本任务是【开箱类】，交给 PET_EGG 状态机处理。 */
      if (et === 1503) return { type: "petEgg", desc: desc, name: name, eventType: et };
      /* ===== v33g 新增：宠物养成系列（挂机可做，交给 PET_OP 状态机）=====
           1500 PetUpgradeTimes 升级次数 / 1501 PetUpgradeTo 升到等级
           1502 PetCollectedCount 收集数量 / 1504 PetStarTo 升星到
           1505 PetStarUpTimes 升星次数 / 1506 PetCampLevelTo 宠物营地等级 */
      if (et === 1500 || et === 1501 || et === 1504 || et === 1505 || et === 1506)
        return { type: "petOp", desc: desc, name: name, eventType: et };
      if (et === 1502) return { type: "petOp", desc: desc, name: name, eventType: et, collect: true };
      /* ===== v33g 新增：角色成长类（多数由服务端/升级自动推进）=====
           101 FightPointTo 战力达到 / 102 UpdatePersonal 更新个人信息
           103 UpdateTroop   更新阵容     / 105 OnBuffExpired buff 过期 */
      if (et === 101 || et === 102 || et === 103 || et === 105)
        return { type: "clientEvent", desc: desc, name: name, eventType: et, autoPass: true };
      /* ===== v33g 新增：物品/合成类 =====
           701 GetItem 获得物品 / 702 OnItemExpired 物品过期
           703 GemComposeTimes 宝石合成次数 / 705 GemComposeSuccess 宝石合成成功
           1610 FusionStoneInlay 刻印石镶嵌 / 1613 FusionStoneCraft 刻印石打造
           1615 GemWornLevelSum 穿戴宝石等级和 / 1616 EquipWornBlessSum 穿戴祝福和
           交给 EQUIP_MISC 状态机（复用 equip 一键接口） */
      if (et === 701 || et === 702) return { type: "clientEvent", desc: desc, name: name, eventType: et, autoPass: true };
      if (et === 703 || et === 705 || et === 1610 || et === 1613 || et === 1615 || et === 1616)
        return { type: "equipMisc", desc: desc, name: name, eventType: et };
      /* ===== v33g 新增：商店类（802 领奖 / 800 用货币 / 804 刷新）===== */
      if (et === 802 || et === 800 || et === 804) {
        var sType3 = arr0(cfg.param1);
        var gId3 = (typeof cfg.param2 === "number") ? cfg.param2 : arr0(cfg.param2);
        return { type: "shopBuy", desc: desc, name: name, eventType: et, shopType: sType3, goodsId: gId3 };
      }
      /* ===== v33g 新增：公会/技能/冒险团 =====
           1200 JoinGuild 加入公会（无公会环境 → 上报推进）
           1700 SkillUpgrade 技能升级 / 1701 SkillUpgradeTo 技能升到
           1800 AdventureTeamExpChange 冒险团经验 / 1802 AdventureTeamLevelTo 冒险团等级 */
      if (et === 1200) return { type: "clientEvent", desc: desc, name: name, eventType: et, note: "加入公会" };
      if (et === 1700 || et === 1701) return { type: "skillUp", desc: desc, name: name, eventType: et };
      if (et === 1800 || et === 1802) return { type: "clientEvent", desc: desc, name: name, eventType: et, autoPass: true };
      // 通用客户端事件上报（eventType < 1000 且 param 无副本/NPC 特征）
      if (et && et < 1000 && et !== 500 && et !== 700 && et !== 900 && et !== 901) {
        return { type: "clientEvent", desc: desc, name: name, eventType: et };
      }
      // 兜底：param2[0] 副本 / param1[0] NPC / levelStory 剧情
      var instId2 = arr0(cfg.param2);
      if (instId2) return { type: "instance", instId: instId2, desc: desc, name: name };
      if (cfg.levelStory && cfg.levelStory.length) {
        return { type: "npc", desc: desc, name: name, story: cfg.levelStory };
      }
      var npcId2 = arr0(cfg.param1);
      if (npcId2) return { type: "npc", desc: desc, name: name, npcId: npcId2, jumpId: cfg.jumpId || 0 };
      return { type: "unknown", desc: desc, name: name };
    } catch (e) { return { type: "unknown" }; }
  }
  /* 1.1.3.2：cvk 一键领取；cvi/cvj 单任务领取。 */
  function autoSubmitTask(cur) {
    try {
      var fc = F();
      var task = fc && fc.task;
      if (!task || !cur) return false;
      /* v26b（sproto 结构已从 assets/sproto/*.bin 确认）：
           .task.TaskIndex  { taskType:0, actId:1, actCycle:2 }
           .task.TaskReward { taskIndex:0(TaskIndex), taskId:1, adsDouble:2 }  <- 领取奖励
           .task.TaskOper   { taskIndex:0(TaskIndex), taskId:1 }               <- 刷新任务
         getTaskReward(7425) 的 request 是 TaskReward（不是 TaskIndex）！
         必须补 adsDouble(boolean)，否则结构不完整 -> 参数错误。 */
      var tIdx = cur.rawTaskIndex || cur.taskIndex;
      if (!tIdx || typeof tIdx !== "object") {
        try { tIdx = (cur.task && cur.task.taskIndex) || null; } catch (e) { tIdx = null; }
      }
      if (!tIdx || typeof tIdx !== "object") {
        var grpRef = cur.taskGroup;
        if (grpRef && grpRef.taskIndex) tIdx = grpRef.taskIndex;
      }
      if (!tIdx || typeof tIdx !== "object") {
        var tt0 = (cur.cfg && cur.cfg.taskType) || 3;
        tIdx = { taskType: tt0 };
      }
      tIdx = {
        taskType: (typeof tIdx.taskType === "number") ? tIdx.taskType : 3,
        actId:    (typeof tIdx.actId === "number")    ? tIdx.actId    : 0,
        actCycle: (typeof tIdx.actCycle === "number") ? tIdx.actCycle : 0
      };
      var tId = cur.task ? cur.task.id : cur.taskId;
      /* v25f：与官方一致（TaskBaseComp.dBL）：
           dBL() { return this.dBJ() ? [2] : [4, task.cvi({taskIndex, taskId})]; }
           dBJ() { return task.status == $pB.e; }   // e=3 已领取
         只要不是已领取(3)就可以提交，并不要求 status==2。 */
      var st = cur.task ? cur.task.status : -1;
      if (st === 3) {
        autoLog("提交跳过: 任务 #" + tId + " 已领取(status=3)");
        return false;
      }
      /* v26d：失败计数带 60 秒过期 + KEY 带 build 号（换版本自动失效），
         避免旧残留把任务永久封死。 */
      var failKey = "__WX_SUBFAIL_" + REMOTE_BUILD + "_" + tId;
      var failRec = null;
      try { failRec = JSON.parse(lsGet(failKey, "null") || "null"); } catch (e) {}
      var fails = (failRec && typeof failRec.n === "number") ? failRec.n : 0;
      var failAt = (failRec && typeof failRec.t === "number") ? failRec.t : 0;
      if (Date.now() - failAt > 60000) fails = 0;
      if (fails >= 3) {
        if (AUTO.__subFailLogged !== tId) {
          AUTO.__subFailLogged = tId;
          autoLog("任务 #" + tId + " 已失败 " + fails + " 次，60 秒内暂停");
        }
        return false;
      }
      /* ★ v27 核心修复：主线任务奖励的【官方领取方式】终于找到了
         官方 TaskBaseComp（主线任务面板）的「领取奖励」按钮 = fdc()：
             fdc() {
               // 若本组存在 status==$pB.d(可领取=2) 的任务，或活动宝箱可领 → 
               return $eZ.task.cvk(this.fcS);        // ★ fcS = { taskType: X }（只有 taskType！）
               // 否则 cTg("暂无可领取奖励")
             }
         cvk(t) → rB(t, {merge:false}) → 协议 oneClickReceive(id=7426)
                  request = task.TaskIndex = { taskType, actId?, actCycle? }   ← 结构简单
         而 cvi(t) → rA(t) → 协议 getTaskReward(id=7425)
                  request = task.TaskReward = { taskIndex(TaskIndex), taskId, adsDouble }  ← 结构复杂
         ★ 之前一直「参数错误」的真正原因：主线任务在服务端只接受 oneClickReceive(cvk)，
           getTaskReward(cvi) 对主线会被判参数错误。所以【cvk 优先，cvi 仅作非主线兜底】。 */
      var cvaIdx = { taskType: tIdx.taskType };
      if (typeof tIdx.actId === "number" && tIdx.actId > 0) cvaIdx.actId = tIdx.actId;
      if (typeof tIdx.actCycle === "number" && tIdx.actCycle > 0) cvaIdx.actCycle = tIdx.actCycle;
      if (typeof task.cvk === "function") {
        autoLog("[提交] cvk(一键领取) req=" + JSON.stringify(cvaIdx));
        var r3 = null;
        try { r3 = task.cvk(cvaIdx); } catch (eC3) { autoLog("cvk 抛出: " + (eC3 && eC3.message)); }
        if (r3 && r3.then) {
          r3.then(function (ok) {
            if (ok) { lsSet(failKey, JSON.stringify({ n: 0, t: Date.now() })); autoLog("提交任务成功(cvk) #" + tId); }
            else {
              lsSet(failKey, JSON.stringify({ n: fails + 1, t: Date.now() }));
              autoLog("提交任务失败(cvk) #" + tId + " 第" + (fails + 1) + "次 req=" + JSON.stringify(cvaIdx) + "（若无奖励可领属正常）");
            }
          }, function () {});
        }
        else autoLog("提交任务 #" + tId + " (cvk)");
        return true;
      }
      if (typeof task.cvi === "function") {
        var reqR = { taskIndex: tIdx, taskId: tId, adsDouble: false };
        autoLog("[提交] cvi req=" + JSON.stringify(reqR));
        var r1 = null;
        try { r1 = task.cvi(reqR); } catch (eC) { autoLog("cvi 抛出: " + (eC && eC.message)); }
        if (r1 && r1.then) {
          r1.then(function (ok) {
            if (ok) { lsSet(failKey, JSON.stringify({ n: 0, t: Date.now() })); autoLog("提交任务成功(cvi) #" + tId); }
            else {
              lsSet(failKey, JSON.stringify({ n: fails + 1, t: Date.now() }));
              autoLog("提交任务失败(cvi) #" + tId + " 第" + (fails + 1) + "次 req=" + JSON.stringify({ taskIndex: tIdx, taskId: tId, adsDouble: false }));
              autoLog("  来源 raw=" + JSON.stringify(cur.rawTaskIndex) + " cur=" + JSON.stringify(cur.taskIndex) +
                      " task=" + JSON.stringify(cur.task && cur.task.taskIndex) +
                      " grp=" + JSON.stringify(cur.taskGroup && cur.taskGroup.taskIndex));
            }
          }, function () {});
        }
        else autoLog("提交任务 #" + tId + " (cvi)");
        return true;
      }
      if (typeof task.cvj === "function") {
        var r2 = task.cvj({ taskIndex: tIdx, taskId: tId, adsDouble: false });
        if (r2 && r2.then) r2.then(function (ok) { autoLog(ok ? ("提交任务成功(cvj) #" + tId) : ("提交任务失败(cvj) #" + tId)); }, function () {});
        else autoLog("提交任务 #" + tId + " (cvj)");
        return true;
      }
      autoLog("autoSubmitTask: 1.1.3.2 提交接口不可用");
      return false;
    } catch (e) { autoLog("autoSubmitTask 异常: " + (e && e.message)); return false; }
  }
  /* 1.1.3.2 上报 NPC 对话。 */
  function reportTalkNpc(npcId) {
    try {
      var task = F() && F().task;
      if (!task || !npcId) return false;
      if (typeof task.cvr !== "function") return false;
      task.cvr(npcId);
      return true;
    } catch (e) { return false; }
  }
  /* 1.1.3.2 上报客户端事件。 */
  function reportClientEvent(evType, params) {
    try {
      var task = F() && F().task;
      if (!task || !evType) return null;
      return typeof task.cvq === "function" ? task.cvq(evType, params || []) : null;
    } catch (e) { return null; }
  }
  /* 1.1.3.2 装备附魔：equip.fsy() 生成匹配列表，equip.fsE(list) 逐件提交。 */
  /* ================= v33g 新增：宠物蛋孵化 =================
     源码（game/index.js）：
       蛋 = BagTableEnum.BOX(102) 物品；BoxTable.findByKey(itemId).subType === PET_BOX 才是宠物蛋
       背包格：$eZ.bag.bYn(BagTableEnum.BOX) → { d: [gridId...], c: [...] }
       取物品：$eZ.bag.bYo(gridId) → { itemId, ... }
       孵化  ：$eZ.openBox.cpI({ itemId, amount }) → 协议 OpenBox.open
     返回：true 表示成功发起了一次孵化。 */
  var EGG_DIAG_DONE = 0;
  /* 常数兜底（main/index.js 已确认）
       BagTableEnum.BOX = 102 / BoxTableEnum.PET_BOX = 42001 / BoxTypeTableEnum.PET_BOX = 1
       CountingTableEnum.GUIDE_FREE_PET_EGG = 1008 */
  function _eggConsts() {
    var c = { BOX: 102, PET_BOX_ITEM: 42001, PET_BOX_TYPE: 1, FREE_EGG_CNT: 1008 };
    try {
      var te = F() && F().cfgMgr && F().cfgMgr.tblEnum;
      if (te) {
        if (te.BagTableEnum && te.BagTableEnum.BOX) c.BOX = te.BagTableEnum.BOX;
        if (te.BoxTableEnum && te.BoxTableEnum.PET_BOX) c.PET_BOX_ITEM = te.BoxTableEnum.PET_BOX;
        if (te.BoxTypeTableEnum && te.BoxTypeTableEnum.PET_BOX != null) c.PET_BOX_TYPE = te.BoxTypeTableEnum.PET_BOX;
        if (te.CountingTableEnum && te.CountingTableEnum.GUIDE_FREE_PET_EGG != null) c.FREE_EGG_CNT = te.CountingTableEnum.GUIDE_FREE_PET_EGG;
      }
    } catch (e) {}
    return c;
  }
  /* 每日免费宠物蛋剩余次数（源码 88553 行的判定） */
  function petEggFreeRemain() {
    try {
      var fc = F();
      var cnt = fc && fc.counting;
      if (!cnt || typeof cnt.ceD !== "function") return 0;
      var r = cnt.ceD(_eggConsts().FREE_EGG_CNT);
      var n = r && (typeof r.remainCount === "number" ? r.remainCount : 0);
      return n > 0 ? n : 0;
    } catch (e) { return 0; }
  }
  /* 从 BOX 容器元素里取 itemId
     ★ 源码 88572：n.d.forEach(e => ... e.b[0].itemId)
       即元素结构 = { b: [ { itemId, ... } ] }
     兼容：元素直接是 gridId（数字）的旧结构 */
  function eggItemIdOf(bag, node) {
    try {
      if (node == null) return 0;
      if (typeof node === "object") {
        var arr = node.b;
        if (arr && arr.length) {
          var f0 = arr[0];
          if (f0 && typeof f0 === "object" && f0.itemId) return f0.itemId;
          if (typeof f0 === "number") {
            var it0 = bag.bYo ? bag.bYo(f0) : null;
            if (it0 && it0.itemId) return it0.itemId;
          }
        }
        if (typeof node.itemId === "number") return node.itemId;
        return 0;
      }
      /* 数字/字符串 gridId */
      var it = bag.bYo ? bag.bYo(node) : null;
      return (it && it.itemId) ? it.itemId : 0;
    } catch (e) { return 0; }
  }
  /* 找一枚可孵化的蛋（背包 BOX × subType===PET_BOX × 材料充足） */
  function petEggFindOne() {
    try {
      var fc = F();
      var bag = fc && fc.bag;
      if (!bag) return null;
      var C = _eggConsts();
      var box = null;
      try { box = bag.bYn ? bag.bYn(C.BOX) : null; } catch (e) { box = null; }
      var tbl = fc.cfgMgr && fc.cfgMgr.tbl && fc.cfgMgr.tbl.BoxTable;
      /* ★ 诊断（只打一次）：把 BOX 容器结构 dump 出来 */
      if (!EGG_DIAG_DONE) {
        EGG_DIAG_DONE = 1;
        try {
          var ds = [];
          var dArr = box && box.d;
          if (dArr && dArr.length) {
            for (var q = 0; q < dArr.length && q < 3; q++) {
              var nd = dArr[q];
              var iid = eggItemIdOf(bag, nd);
              var cfgQ = null;
              try { cfgQ = tbl && tbl.findByKey ? tbl.findByKey(iid) : null; } catch (e) {}
              ds.push(q + ":{type=" + (typeof nd) + ",itemId=" + iid + ",subType=" + (cfgQ ? cfgQ.subType : "?") + "}");
            }
          }
          autoLog("[egg-diag] BOX容器 d=" + (dArr ? dArr.length : "null") +
                  " 结构=[" + ds.join(" | ") + "] PET_BOX_ITEM=" + C.PET_BOX_ITEM +
                  " PET_BOX_TYPE=" + C.PET_BOX_TYPE + " 免费次数=" + petEggFreeRemain());
        } catch (e) { autoLog("[egg-diag] dump 异常 " + (e && e.message)); }
      }
      if (!box || !box.d || !box.d.length) return null;
      for (var i = 0; i < box.d.length; i++) {
        var iid2 = eggItemIdOf(bag, box.d[i]);
        if (!iid2) continue;
        var cfg = null;
        try { cfg = tbl && tbl.findByKey ? tbl.findByKey(iid2) : null; } catch (e) { cfg = null; }
        var isEgg = false;
        /* 判定条件（宽松但安全）：① subType === PET_BOX  或  ② itemId === 常量 PET_BOX */
        if (cfg && cfg.subType != null && cfg.subType === C.PET_BOX_TYPE) isEgg = true;
        if (!isEgg && iid2 === C.PET_BOX_ITEM) isEgg = true;
        if (!isEgg) continue;
        /* 材料充足性（源码 88560-88562 同款） */
        var enough = true;
        try {
          var cost = (cfg && cfg.costItems) || [];
          if (cost.length && bag.bXI && bag.bXJ && bag.bXM) {
            var need = bag.bXI(cost);
            var have = bag.bXJ((need && need.b) || []);
            enough = !!(have && have.isEnough);
          }
        } catch (e) { enough = true; }
        if (enough) return { itemId: iid2, node: box.d[i], cfg: cfg, free: false };
      }
      return null;
    } catch (e) { return null; }
  }
  function autoHatchEggOnce() {
    try {
      var fc = F();
      var ob = fc && fc.openBox;
      if (!ob) {
        if (!AUTO.__eggNoApi) { AUTO.__eggNoApi = 1; autoLog("孵化: 无 openBox 组件"); }
        return false;
      }
      var C = _eggConsts();
      /* 官方孵化入口 = openBox.cpI（game 88432/88541 行确认） */
      var fn = (typeof ob.cpI === "function") ? ob.cpI : null;
      if (!fn) {
        if (!AUTO.__eggNoApi2) { AUTO.__eggNoApi2 = 1; autoLog("孵化: openBox.cpI 不可用"); }
        return false;
      }
      /* ★★ 优先「每日免费蛋」（源码 88553 行）：直接用常量 42001，不消耗背包 */
      var targetId = 0, isFree = false, eggRef = null;
      var freeN = petEggFreeRemain();
      if (freeN > 0) {
        targetId = C.PET_BOX_ITEM;
        isFree = true;
      } else {
        var egg = petEggFindOne();
        if (!egg) {
          if (!AUTO.__eggEmptyLogged) {
            AUTO.__eggEmptyLogged = 1;
            autoLog("孵化: 无可用蛋（背包无宠物蛋 且 免费次数=0）");
          }
          return false;
        }
        targetId = egg.itemId;
        eggRef = egg;
      }
      if (AUTO.__eggBusy) return false;
      AUTO.__eggBusy = true;
      var p = null;
      try {
        p = fn.call(ob, { itemId: targetId, amount: 1 });
      } catch (e) {
        AUTO.__eggBusy = false;
        autoLog("孵化调用异常: " + (e && e.message));
        return false;
      }
      var fin = function (ok, extra) {
        AUTO.__eggBusy = false;
        if (ok) {
          AUTO.__eggLastAt = Date.now();
          AUTO.eggCount = (AUTO.eggCount || 0) + 1;
          autoLog("孵化成功 ×" + AUTO.eggCount + "（itemId=" + targetId +
                  (isFree ? " 免费蛋 剩余" + freeN : "") + "）" + (extra || ""));
        } else {
          autoLog("孵化失败（itemId=" + targetId + "）" + (extra ? ": " + extra : ""));
        }
      };
      if (p && p.then) {
        p.then(function (r) { fin(!!r && r.code === 0, r && r.msg); },
               function (e) { fin(false, e && e.message); });
      } else { fin(true, ""); }
      return true;
    } catch (e) { AUTO.__eggBusy = false; return false; }
  }

  /* ================= v33g 新增：宠物养成（升级/升星）=================
     PetComp 的官方"一键"入口（game/index.js 84021 行）：
       $eZ.pet.cqu() 返回"可操作的宠物列表"
       $eZ.pet.cqO(list) → 协议 Pet.quickWear
     这里优先用 cqu+cqO；不可用时返回 false，由状态机走「上报推进」。 */
  /* ================= v33N：宠物升级（真实协议实现）=================
     源码链路：
       宠物列表：$eZ.pet.cqF.cpU(BagTableEnum.PET) / PET_BATTLE / PET_ASSIST 的 d.bds()
       所在容器：$eZ.pet.cqF.cpZ(pet) → PET(600) / PET_BATTLE(601) / PET_ASSIST(602)
       升级调用：$eZ.pet.cqQ({bagType, gridId, itemId, itemUid})
                 → qm(t) → SprotocolMap.Pet.levelUp
       官方按钮：PetLevelUpComp.btn_upLevel → eGC()（game:88157） */
  var PET_DIAG_DONE = 0;
  function _petConsts() {
    var c = { PET: 600, PET_BATTLE: 601, PET_ASSIST: 602 };
    try {
      var te = F() && F().cfgMgr && F().cfgMgr.tblEnum;
      if (te && te.BagTableEnum) {
        if (te.BagTableEnum.PET != null) c.PET = te.BagTableEnum.PET;
        if (te.BagTableEnum.PET_BATTLE != null) c.PET_BATTLE = te.BagTableEnum.PET_BATTLE;
        if (te.BagTableEnum.PET_ASSIST != null) c.PET_ASSIST = te.BagTableEnum.PET_ASSIST;
      }
    } catch (e) {}
    return c;
  }
  /* 收集所有宠物（出战/助战/背包，去重） */
  function collectPets() {
    var out = [];
    try {
      var fc = F();
      var pet = fc && fc.pet;
      if (!pet || !pet.cqF || typeof pet.cqF.cpU !== "function") return out;
      var C = _petConsts();
      var seen = {};
      var types = [C.PET_BATTLE, C.PET_ASSIST, C.PET];
      for (var ti = 0; ti < types.length; ti++) {
        var cont = pet.cqF.cpU(types[ti]);
        var pets = cont && cont.petInfos || [];
        for (var i = 0; i < pets.length; i++) {
          var p = pets[i];
          if (!p || !p.itemId) continue;
          var uid = p.itemUid || (p.itemId + "_" + (p.gridId || i));
          if (seen[uid]) continue;
          seen[uid] = 1;
          out.push({ pet: p, bagType: types[ti] });
        }
      }
    } catch (e) {}
    return out;
  }
  /* 升级一次（官方 Pet.levelUp 协议） */
  function autoPetOpOnce() {
    try {
      var fc = F();
      var pet = fc && fc.pet;
      if (!pet || !pet.cqF || typeof pet.cqF.cpZ !== "function" || typeof pet.cqQ !== "function") {
        if (!AUTO.__petNoApi) { AUTO.__petNoApi = 1; autoLog("宠物升级: 1.1.3.2 接口不可用"); }
        return false;
      }
      if (AUTO.__petBusy) return false;
      var list = collectPets();
      /* ★ 诊断（只打一次）：暴露宠物清单结构 */
      if (!PET_DIAG_DONE) {
        PET_DIAG_DONE = 1;
        try {
          var ds = [];
          for (var q = 0; q < list.length && q < 5; q++) {
            var pp = list[q].pet;
            ds.push((pp.itemId || "?") + "@" + list[q].bagType + (pp.pet ? (",lv" + pp.pet.level) : ",lv?"));
          }
          autoLog("[pet-diag] 宠物数=" + list.length + " [" + ds.join(" | ") + "] cqQ=" + (typeof pet.cqQ));
        } catch (e) {}
      }
      if (!list.length) {
        if (!AUTO.__petEmptyLogged) { AUTO.__petEmptyLogged = 1; autoLog("宠物升级: 未找到宠物"); }
        return false;
      }
      /* 选第一个带 gridId 的 */
      var pick2 = null;
      for (var k = 0; k < list.length; k++) {
        var p0 = list[k].pet;
        if (!p0 || !p0.itemId || !p0.gridId) continue;
        pick2 = list[k]; break;
      }
      if (!pick2) return false;
      var p1 = pick2.pet;
      var bagType = pet.cqF.cpZ(p1);
      if (typeof bagType !== "number") bagType = pick2.bagType;
      AUTO.__petBusy = true;
      var payload = {
        bagType: bagType,
        gridId: p1.gridId,
        itemId: p1.itemId,
        itemUid: p1.itemUid
      };
      var pr = null;
      try {
        pr = pet.cqQ(payload);
      } catch (e) {
        AUTO.__petBusy = false;
        autoLog("宠物升级调用异常: " + (e && e.message));
        return false;
      }
      var fin = function (ok, extra) {
        AUTO.__petBusy = false;
        if (ok) {
          AUTO.petOpCount = (AUTO.petOpCount || 0) + 1;
          autoLog("宠物升级成功 ×" + AUTO.petOpCount + "（itemId=" + p1.itemId + " bagType=" + bagType + "）");
        } else {
          if (!AUTO.__petFailLogged) {
            AUTO.__petFailLogged = 1;
            autoLog("宠物升级失败（itemId=" + p1.itemId + "）" + (extra ? ": " + extra : "") + " —— 可能材料不足");
          }
        }
      };
      if (pr && pr.then) {
        pr.then(function (r) { fin(!!r, ""); }, function (e) { fin(false, e && e.message); });
      } else { fin(!!pr, ""); }
      return true;
    } catch (e) { AUTO.__petBusy = false; return false; }
  }

  /* 1.1.3.2 刻印添加：fuO(equip) 选兼容刻印，fuU(equipUid, gridId) 提交。 */
  function autoFusionStoneInlayOnce() {
    try {
      if (AUTO.__fusionBusy) return false;
      var fc = F();
      var eq = fc && fc.equip;
      var bag = fc && fc.bag;
      if (!eq || !bag || typeof bag.bYn !== "function" || typeof eq.fuO !== "function" || typeof eq.fuU !== "function") return false;
      var te = fc.cfgMgr && fc.cfgMgr.tblEnum;
      var woreType = te && te.BagTableEnum && te.BagTableEnum.WORE_EQUIP != null ? te.BagTableEnum.WORE_EQUIP : 201;
      var wore = bag.bYn(woreType);
      var list = wore && wore.c ? wore.c : [];
      for (var i = 0; i < list.length; i++) {
        var item = list[i];
        if (!item || !item.itemUid || (typeof eq.fuP === "function" && eq.fuP(item) > 0)) continue;
        var stones = eq.fuO(item) || [];
        if (!stones.length || !stones[0] || stones[0].gridId == null) continue;
        AUTO.__fusionBusy = true;
        var pr = eq.fuU(item.itemUid, stones[0].gridId);
        var done = function (ok) {
          AUTO.__fusionBusy = false;
          if (ok) AUTO.miscCount = (AUTO.miscCount || 0) + 1;
        };
        if (pr && pr.then) pr.then(function (res) { done(!res || res.code === 0); }, function () { done(false); });
        else done(true);
        return true;
      }
      return false;
    } catch (e) { AUTO.__fusionBusy = false; return false; }
  }

  function autoEquipMiscOnce(eventType) {
    if (eventType === 1610) return autoFusionStoneInlayOnce();
    if (eventType === 1615) return autoGemInlayOnce();
    return false;
  }

  function autoEnchantOnce() {
    try {
      var fc = F();
      var eq = fc && fc.equip;
      if (!eq) return false;
      if (typeof eq.fsy !== "function" || typeof eq.fsE !== "function") {
        if (!AUTO.__enchantNoApi) { AUTO.__enchantNoApi = 1; autoLog("附魔: equip.fsy/fsz 不可用"); }
        return false;
      }
      if (AUTO.__enchantBusy) return false;
      var list = [];
      try { list = eq.fsy() || []; } catch (e) { list = []; }
      if (!list.length) {
        // 没有可附魔的组合：可能是背包没魂魄 / 都已镶嵌
        if (!AUTO.__enchantEmptyLogged) {
          AUTO.__enchantEmptyLogged = 1;
          autoLog("附魔: 暂无可穿戴的魂魄（equip.fsy 返回空）");
        }
        return false;
      }
      AUTO.__enchantBusy = true;
      var pr = eq.fsE(list);
      var fin = function (n) {
        AUTO.__enchantBusy = false;
        AUTO.__enchantCount = (AUTO.__enchantCount || 0) + (n || 0);
        autoLog("附魔完成: 本次 " + n + " 件, 累计 " + AUTO.__enchantCount + " 件");
      };
      if (pr && pr.then) pr.then(fin, function (e) { AUTO.__enchantBusy = false; autoLog("附魔异常: " + e); });
      else fin(list.length);
      return true;
    } catch (e) { AUTO.__enchantBusy = false; autoLog("autoEnchantOnce 异常: " + (e && e.message)); return false; }
  }
  /* 1.1.3.2 晶石镶嵌：fva(equip, slot) 取兼容晶石，fvS(request) 提交。 */
  function autoGemInlayOnce() {
    try {
      if (AUTO.__gemBusy) return false;
      var fc = F();
      var eq = fc && fc.equip;
      var bag = fc && fc.bag;
      if (!eq || !bag || typeof bag.bYn !== "function" || typeof eq.fuW !== "function" || typeof eq.fva !== "function" || typeof eq.fvS !== "function") return false;
      var te = fc.cfgMgr && fc.cfgMgr.tblEnum;
      var woreType = te && te.BagTableEnum && te.BagTableEnum.WORE_EQUIP != null ? te.BagTableEnum.WORE_EQUIP : 201;
      var wore = bag.bYn(woreType);
      var list = wore && wore.c ? wore.c : [];
      for (var i = 0; i < list.length; i++) {
        var item = list[i];
        if (!item || !item.itemUid || !item.itemId) continue;
        var slotCfg = eq.fuW(item.itemId);
        var count = slotCfg && slotCfg.count ? slotCfg.count : 0;
        var gems = item.equip && item.equip.gemList ? item.equip.gemList : [];
        for (var slot = 1; slot <= count; slot++) {
          if (gems[slot - 1]) continue;
          var choices = eq.fva(item, slot) || [];
          if (!choices.length || !choices[0] || !choices[0].itemId) continue;
          AUTO.__gemBusy = true;
          var pr = eq.fvS({ equipUid: item.itemUid, gemItemId: choices[0].itemId });
          var done = function (ok) {
            AUTO.__gemBusy = false;
            if (ok) AUTO.miscCount = (AUTO.miscCount || 0) + 1;
          };
          if (pr && pr.then) pr.then(function (res) { done(!res || res.code === 0); }, function () { done(false); });
          else done(true);
          return true;
        }
      }
      return false;
    } catch (e) { AUTO.__gemBusy = false; autoLog("autoGemInlayOnce 异常: " + (e && e.message)); return false; }
  }

  /* ★ v25：一键强化（equip 侧通用入口，fpI 已在 autoStrengthenOnce 里用） */
  /* 领取章节星级奖励：Chapter.quickReceive(6658) request=章节id(数字) */
  function autoChapterReward(chapterId) {
    try {
      var fc = F();
      var ch = fc && fc.chapter;
      if (!ch || !chapterId) return false;
      // 1.1.3.2 官方入口：chapter.caS(章节id)
      if (typeof ch.caS === "function") {
        ch.caS(chapterId);
        autoLog("领取章节奖励 chapterId=" + chapterId);
        return true;
      }
      return false;
    } catch (e) { return false; }
  }

  /* 1.1.3.2 自动配置药水：bag.bYp(5)，troop.cxu/cxv/cxw/cxy。 */
  /* ★ v30 新增：自动购买商店商品（如 0元"试用月卡" 70900）
     官方链路（SilverMonthCardComp.gaR，@4380320）：
       t && (e = ShopGoodsTableEnum.TRIAL_MONTHLY_CARD);   // 试用 → 70900
       n = shop.shopGoodsInfo.fDU(e);   // 取商品信息
       shop.reqShopGoodsBuy(n);         // 购买
     购买成功后服务端完成 801 任务 → 触发 104 特权激活 → 主线 300 完成 → 可领奖。
     本函数做安全校验：只购买【0 元/免费】商品（避免误充值）。 */
  function autoShopBuy(goodsId) {
    try {
      var fc = F();
      var shop = fc && fc.shop;
      if (!shop || !goodsId) { autoLog("购买失败: shop 不可用或 goodsId 为空"); return false; }
      var info = null;
      try {
        if (shop.shopGoodsInfo && typeof shop.shopGoodsInfo.fDU === "function") {
          info = shop.shopGoodsInfo.fDU(goodsId);
        }
      } catch (e) {}
      // 安全校验：只买免费（0元）商品
      if (info && info.fDy) {
        var cp = info.fDy.chargePrice;
        if (typeof cp === "number" && cp > 0) {
          autoLog("购买中止: 商品 #" + goodsId + " 需付费 " + cp + " 元，脚本不自动充值");
          return false;
        }
      }
      if (!info) { autoLog("购买失败: 商品 #" + goodsId + " 信息为空(fDI)"); return false; }
      var pr = null;
      try { pr = shop.reqShopGoodsBuy(info); } catch (e) { autoLog("reqShopGoodsBuy 抛出: " + (e && e.message)); }
      if (pr && pr.then) {
        pr.then(function (res) {
          var ok = res && (res.code === 0 || res.code === undefined);
          autoLog("购买商品 #" + goodsId + (ok ? " 成功" : (" 返回 code=" + (res && res.code))));
          try { var m = F() && F().monCard; if (m && m.fDn && typeof m.fDn.fCX === "function") m.fDn.fCX(); } catch (e) {}
        }, function (err) { autoLog("购买商品 #" + goodsId + " 拒绝: " + err); });
      } else {
        autoLog("购买商品 #" + goodsId + " 已发送");
      }
      return true;
    } catch (e) { autoLog("autoShopBuy 异常: " + (e && e.message)); return false; }
  }
  /* 1.1.3.2 官方一键穿戴链路：equip.ftA() 计算 itemUid 列表，equip.fvH(list) 提交。 */
  function autoWearBestEquip(retry) {
    try {
      retry = retry || 0;
      var eq = F() && F().equip;
      if (!eq) { autoLog("穿戴失败: equip 不可用"); return false; }
      var list = [];
      try { if (typeof eq.ftA === "function") list = eq.ftA() || []; } catch (e) { autoLog("ftA 异常: " + (e && e.message)); }
      if (!list || !list.length) { autoLog("一键穿戴: 当前已是最优装备（无需替换）"); return false; }
      var fn = typeof eq.fvH === "function" ? eq.fvH : null;
      if (!fn) { autoLog("穿戴失败: equip.fvH 不存在"); return false; }
      autoLog("一键穿戴: 待替换 " + list.length + " 件");
      var pr = null;
      try { pr = fn.call(eq, list); } catch (e) { autoLog("quickWear 抛出: " + (e && e.message)); }
      if (pr && pr.then) {
        pr.then(function (res) {
          var code = res && res.code;
          if (code === 0 || code === undefined) { autoLog("一键穿戴最优装备 完成"); return; }
          // code=15 network → 延迟重试（最多 3 次）
          if ((code === 15 || code === 3) && retry < 3) {
            autoLog("一键穿戴 code=" + code + "(网络未就绪) → " + (retry + 1) + "s 后重试");
            setTimeout(function () { autoWearBestEquip(retry + 1); }, 1500 + retry * 1000);
          } else {
            autoLog("一键穿戴最优装备 code=" + code);
          }
        }, function () { autoLog("一键穿戴 拒绝"); });
      } else autoLog("一键穿戴 已发送");
      return true;
    } catch (e) { autoLog("autoWearBestEquip 异常: " + (e && e.message)); return false; }
  }
  /* 1.1.3.2 技能缓存为 skill.cus.ctZ，升级入口为 skill.cuw。 */
  function autoUpgradeSkills() {
    try {
      var fc = F();
      var sk = fc && fc.skill;
      if (!sk || typeof sk.cuw !== "function") { autoLog("技能升级: skill.cuw 不可用"); return false; }
      var SKILL_PVE = 1, SKILL_PVP = 2;
      var getList = function () {
        try {
          var c = sk.cus && sk.cus.ctZ;
          if (c && typeof c.bdt === "function") return c.bdt() || [];
        } catch (e) {}
        return [];
      };
      var doOne = function (type) {
        var list = getList();
        if (!list || !list.length) return 0;
        var n = 0;
        for (var i = 0; i < list.length && n < 40; i++) {
          var item = list[i];
          try {
            if (!item || !item.ctK) continue;
            var cur = 0;
            try { cur = (typeof item.ctT === "function") ? (item.ctT(type) || 0) : 0; } catch (e) { cur = 0; }
            if (cur <= 0) continue;                                  // 未学会 → 跳过
            if (typeof sk.cuH === "function") {
              var chk = sk.cuH(type, item);
              if (!chk || chk.code !== 0) continue;                   // 等级/前置/觉醒不满足
            }
            if (typeof sk.cuI === "function" && !sk.cuI(type, item)) continue;  // 技能点不足
            sk.cuw(type, item, 1); n++;
          } catch (e) {}
        }
        return n;
      };
      var a = doOne(SKILL_PVE), b = doOne(SKILL_PVP);
      autoLog("技能升级: PVE+" + a + " PVP+" + b + (getList().length ? "" : "(技能列表为空)"));
      return (a + b) > 0;
    } catch (e) { autoLog("autoUpgradeSkills 异常: " + (e && e.message)); return false; }
  }
  /* ★ v31：手动触发一次"自动出售/分解"（兜底：游戏自带的在拾取时触发，
     这里在结算后主动跑一次，确保刷完图立即清理背包） */
  /* ================= v33j：出售 / 分解（复刻官方 + 诊断）=================
     官方（game:47585）：
       A = [1,2,3]；x = A.filter(q => localMgr.cnp(q)) → bag.bYK(x)（出售）
                    N = A.filter(q => localMgr.cnq(q)) → bag.bYJ(N)（分解）
     bYK/bYJ 内部再按【锁定】【品质】【可分解性】过滤，空列表则不发送。
     ★「分解要 50 级」的根因 = bXY()：EquipTable.decompose 为空即不可分解。 */
  var SD_DIAG = {};
  function _sdQualitiesOf(lm, useSell) {
    /* ★v33M：取【游戏内位标记(lm.cnp/cnq)】与【UI 勾选】的【并集】
       · 旧的"优先游戏内、空了才兜底 UI"有坑：cnr 若没成功写入，游戏内恒为空，
         而 UI 明明勾了 → 走到兜底也许对，但一旦游戏内残留旧位就会覆盖 UI 意愿。
       · 并集语义更符合用户预期："我勾了就该生效"。 */
    var set = {};
    /* ① 游戏内位标记 */
    try {
      var fn = useSell ? lm.cnp : lm.cnq;
      if (typeof fn === "function") {
        for (var q = 1; q <= 6; q++) { try { if (fn.call(lm, q)) set[q] = 1; } catch (e) {} }
      }
    } catch (e) {}
    var gameQs = Object.keys(set).map(Number).sort();
    /* ② UI 勾选 */
    var uiQs = [];
    try {
      var str = useSell ? CFG.sellQualities() : CFG.decompQualities();
      uiQs = String(str || "").split(",").map(Number).filter(function (x) { return x > 0; });
    } catch (e) { uiQs = []; }
    for (var i = 0; i < uiQs.length; i++) set[uiQs[i]] = 1;
    var out = Object.keys(set).map(Number).filter(function (x) { return x >= 1 && x <= 6; }).sort();
    /* 记录一次来源，便于诊断 */
    try {
      W.__WX_SDQ__ = W.__WX_SDQ__ || {};
      W.__WX_SDQ__[useSell ? "sell" : "deco"] = {
        game: gameQs.join(","), ui: uiQs.join(","), final: out.join(",")
      };
    } catch (e) {}
    return out;
  }
  function _sdDump(useSell) {
    /* 诊断：把装备容器的真实情况打出来（每类只打一次） */
    var key = useSell ? "sell" : "deco";
    if (SD_DIAG[key]) return;
    try {
      var fc = F();
      var bag = fc && fc.bag;
      if (!bag) { autoLog("[sd-diag] bag 缺失"); SD_DIAG[key] = 1; return; }
      var EQUIP = 200;
      try {
        var te = fc.cfgMgr && fc.cfgMgr.tblEnum;
        if (te && te.BagTableEnum && te.BagTableEnum.EQUIP) EQUIP = te.BagTableEnum.EQUIP;
      } catch (e) {}
      var cont = null;
      try { cont = bag.bXk ? bag.bXk(EQUIP) : null; } catch (e) {}
      if (!cont) { try { cont = bag.bYn ? bag.bYn(EQUIP) : null; } catch (e) {} }
      var total = 0, locked = 0, qs = {}, decoOK = 0, qMiss = 0;
      try {
        if (cont && cont.d && cont.d.bdm) {
          cont.d.bdm(function (node) {
            total++;
            if (node && node.equip && node.equip.isLock) { locked++; return; }
            var cfg = null;
            try { cfg = bag.bXh ? bag.bXh(node.itemId) : null; } catch (e) {}
            if (!cfg) { qMiss++; return; }
            qs[cfg.quality] = (qs[cfg.quality] || 0) + 1;
            try { if (bag.bXY && bag.bXY(node)) decoOK++; } catch (e) {}
          });
        }
      } catch (e) {}
      var qlist = [];
      for (var k in qs) qlist.push(k + ":" + qs[k]);
      autoLog("[sd-diag] 装备容器 total=" + total + " locked=" + locked +
              " 品质{ " + qlist.join(" ") + " } 可取品质失败=" + qMiss +
              " 可分解=" + decoOK +
              " | bYK=" + (typeof bag.bYK) + " bYJ=" + (typeof bag.bYJ) +
              " bXk=" + (typeof bag.bXk) + " bXh=" + (typeof bag.bXh) + " bXY=" + (typeof bag.bXY));
      SD_DIAG[key] = 1;
    } catch (e) { autoLog("[sd-diag] 异常 " + (e && e.message)); SD_DIAG[key] = 1; }
  }
  function autoSellDecomposeNow() {
    try {
      var fc = F();
      var bag = fc && fc.bag;
      var lm = fc && fc.localMgr;
      if (!bag) return false;
      _sdDump(true);
      _sdDump(false);

      var wantSell = CFG.autoSell(), wantDeco = CFG.autoDecompose();
      var sellQ = wantSell ? _sdQualitiesOf(lm, true) : [];
      var decoQ = wantDeco ? _sdQualitiesOf(lm, false) : [];
      /* ★v33N：开关开着却没品质 → 用默认 [1,2,3] 兜底（否则等于空转） */
      if (wantSell && !sellQ.length) { sellQ = [1, 2, 3]; if (!SD_DIAG.__sellDef) { SD_DIAG.__sellDef = 1; autoLog("[sd] 出售品质为空，使用默认[1,2,3]"); } }
      if (wantDeco && !decoQ.length) { decoQ = [1, 2, 3]; if (!SD_DIAG.__decoDef) { SD_DIAG.__decoDef = 1; autoLog("[sd] 分解品质为空，使用默认[1,2,3]"); } }
      var did = "";

      /* ---- 出售 ---- */
      if (wantSell) {
        var okSell = false;
        if (sellQ.length && typeof bag.bYK === "function") {
          try { bag.bYK(sellQ); did += "出售[" + sellQ.join(",") + "] "; okSell = true; }
          catch (e) { autoLog("[sd] 出售异常: " + (e && e.message)); }
        }
        if (!okSell && !sellQ.length) {
          if (!SD_DIAG.__sellEmpty) { SD_DIAG.__sellEmpty = 1;
            autoLog("[sd] 出售跳过：品质列表为空（UI 未勾选 或 localMgr.cnp 全 false）"); }
        } else if (!okSell && typeof bag.bYK !== "function") {
          if (!SD_DIAG.__sellNoApi) { SD_DIAG.__sellNoApi = 1; autoLog("[sd] 出售跳过：bag.bYK 不可用"); }
        }
      }

      /* ---- 分解 ---- */
      if (wantDeco) {
        var okDeco = false;
        if (decoQ.length && typeof bag.bYJ === "function") {
          try { bag.bYJ(decoQ); did += "分解[" + decoQ.join(",") + "]"; okDeco = true; }
          catch (e) { autoLog("[sd] 分解异常: " + (e && e.message)); }
        }
        if (!okDeco && !decoQ.length) {
          if (!SD_DIAG.__decoEmpty) { SD_DIAG.__decoEmpty = 1;
            autoLog("[sd] 分解跳过：品质列表为空（UI 未勾选 或 localMgr.cnq 全 false）"); }
        } else if (!okDeco && typeof bag.bYJ !== "function") {
          if (!SD_DIAG.__decoNoApi) { SD_DIAG.__decoNoApi = 1; autoLog("[sd] 分解跳过：bag.bYJ 不可用"); }
        }
      }

      /* ★v33M：每次清理都输出一行「开关 + 品质 + 来源」，方便现场核对 */
      var _sq = (W.__WX_SDQ__ && W.__WX_SDQ__.sell) || {};
      var _dq = (W.__WX_SDQ__ && W.__WX_SDQ__.deco) || {};
      autoLog("[sd] 开关 出售=" + (wantSell ? "开" : "关") + " 分解=" + (wantDeco ? "开" : "关") +
              " | 出售品质[" + sellQ.join(",") + "](游戏内:" + (_sq.game || "-") + " UI:" + (_sq.ui || "-") + ")" +
              " | 分解品质[" + decoQ.join(",") + "](游戏内:" + (_dq.game || "-") + " UI:" + (_dq.ui || "-") + ")" +
              " | 结果:" + (did || "无动作"));
      return !!did;
    } catch (e) { autoLog("autoSellDecomposeNow 异常: " + (e && e.message)); return false; }
  }

  /* 1.1.3.2 转职：role.fBb.fAQ/fAU，fBe() 列表，fBm(confId) 执行。 */
  function autoDoJobChange() {
    try {
      var fcJ = F();
      var r = getRoleCur();
      if (!r || !r.mgr) { autoLog("转职: role 未就绪"); return false; }
      if (typeof r.mgr.fBm !== "function") { autoLog("转职: role.fBm 不可用"); return false; }
      var jobList = [];
      try { if (typeof r.mgr.fBe === "function") jobList = r.mgr.fBe(r.group) || []; } catch (e) {}
      if (!jobList.length) { autoLog("转职: 无可转职职业（当前 conf=" + r.confId + " Lv" + r.level + "）"); return false; }
      var tbl = fcJ && fcJ.cfgMgr && fcJ.cfgMgr.tbl;
      var unlocked = [];
      try {
        for (var i = 0; i < jobList.length; i++) {
          var rd = tbl && tbl.RoleDesTable && tbl.RoleDesTable.findByKey ? tbl.RoleDesTable.findByKey(jobList[i]) : null;
          if (!rd || typeof rd.unlockLevel !== "number" || r.level >= rd.unlockLevel) unlocked.push(jobList[i]);
        }
      } catch (e) { unlocked = jobList; }
      if (!unlocked.length) { autoLog("转职: 无已解锁职业（等级不足）"); return false; }
      var target = 0;
      try {
        var selArr = String(lsGet("__WX_JOB_SELECT__", "")).split(",").map(Number).filter(function (x) { return x > 0; });
        for (var j = 0; j < selArr.length; j++) { if (unlocked.indexOf(selArr[j]) >= 0) { target = selArr[j]; break; } }
      } catch (e) {}
      if (!target) target = unlocked[0];
      autoLog("转职: 选择 " + getJobName(fcJ, target) + " #" + target + " → fBm 执行");
      var pr = r.mgr.fBm(target);
      if (pr && pr.then) pr.then(function (res) {
        autoLog("转职结果: " + ((res && (res.code === 0 || res.code === undefined)) ? "成功！" : ("失败 code=" + (res && res.code))));
      }, function () { autoLog("转职异常回调"); });
      return true;
    } catch (e) { autoLog("autoDoJobChange 异常: " + (e && e.message)); return false; }
  }
  function autoConfigMedicine() {
    try {
      var fc = F();
      var bag = fc && fc.bag;
      var troop = fc && fc.troop;
      var cfgMgr = fc && fc.cfgMgr;
      if (!bag || !troop || !cfgMgr) return false;
      // 1.1.3.2 ItemTypeTableEnum.MEDICINE = 5。
      var meds = [];
      try {
        if (typeof bag.bYp === "function") meds = bag.bYp(5) || [];
      } catch (e) {}
      if (!meds.length) { autoLog("背包没有药水"); return false; }
      autoLog("背包药水 " + meds.length + " 个: " + meds.map(function(m){ return (m.itemId!=null?m.itemId:"?")+"x"+(m.amount!=null?m.amount:"?"); }).join(", "));
      // ===== 2. 区分血药/蓝药（MedicineTable.type）=====
      var hpMeds = [], mpMeds = [];
      for (var i = 0; i < meds.length; i++) {
        var m = meds[i];
        var iid = m && (m.itemId != null ? m.itemId : m.id);
        if (iid == null || iid <= 0) continue;   // 过滤无效 itemId（旧 bug 根源）
        try {
          var medCfg = cfgMgr.tbl.MedicineTable && cfgMgr.tbl.MedicineTable.findByKey && cfgMgr.tbl.MedicineTable.findByKey(iid);
          if (!medCfg) { hpMeds.push(m); continue; }  // 查不到配置也当血药候选
          // type: 需确认枚举。常见 1=血 2=蓝（若反了只需调这里）
          if (medCfg.type === 2) mpMeds.push(m);
          else hpMeds.push(m);
        } catch (e) { hpMeds.push(m); }
      }
      autoLog("血药候选 " + hpMeds.length + " 蓝药候选 " + mpMeds.length);
      // 若某类为空，用全部候选兜底（避免落空）
      if (!hpMeds.length && meds.length) hpMeds = meds;
      if (!mpMeds.length && meds.length > 1) mpMeds = [meds[1]];
      // ===== 3. 配置槽位：1 血 + 1 蓝 =====
      var slots = [];
      function getIid(x) { return x && (x.itemId != null ? x.itemId : x.id); }
      var h1 = getIid(hpMeds[0]);
      var m1 = getIid(mpMeds[0]);
      if (h1 != null && h1 > 0) slots.push({ itemId: h1, isAuto: true, applyValue: 75 });
      if (m1 != null && m1 > 0 && m1 !== h1) slots.push({ itemId: m1, isAuto: true, applyValue: 50 });
      // 若只有1种药水，补一个同类型（保证2槽）
      if (slots.length < 2 && h1 != null && h1 > 0) slots.push({ itemId: h1, isAuto: true, applyValue: 75 });
      if (!slots.length) { autoLog("背包没有有效药水 itemId"); return false; }
      autoLog("目标槽位: " + JSON.stringify(slots));
      // 使用新版 troop 缓存和保存链路。
      try {
        if (!troop.cxt && typeof troop.cxu === "function") {
          autoLog("troop.cxt 未加载，触发 cxu 拉取");
          var pm0 = troop.cxu();
          if (pm0 && pm0.then) {
            pm0.then(function () {
              try { autoConfigMedicine(); } catch (e) {}
            }, function () {});
          }
          return true;
        }
        if (typeof troop.cxv !== "function" || typeof troop.cxw !== "function") {
          autoLog("药水配置失败: troop.cxv/cxw 不可用");
          return false;
        }
        for (var s = 0; s < slots.length; s++) troop.cxv(s, slots[s]);
        var saved = troop.cxw();
        if (saved && saved.then) saved.then(function(){ autoLog("药水配置已保存(cxw)"); }, function(err){ autoLog("药水配置保存失败: " + err); });
      } catch (e) { autoLog("设置槽位异常: " + (e && e.message)); }
      autoLog("已配置药水: " + JSON.stringify(slots));
      return true;
    } catch (e) { autoLog("药水配置异常: " + (e && e.message)); return false; }
  }

  /* 1.1.3.2 跳转入口：gameHelper.chL(JumpTableConfig)。 */
  function autoJumpByConfig(jumpId) {
    try {
      if (!jumpId) return false;
      var fc = F();
      var gh = fc && fc.gameHelper;
      var cfgMgr = fc && fc.cfgMgr;
      var jumpFn = gh && typeof gh.chL === "function" ? gh.chL : null;
      var hasJumpTbl = !!(cfgMgr && cfgMgr.tbl && cfgMgr.tbl.JumpTable);
      var jumpCfg = null;
      if (hasJumpTbl) { try { jumpCfg = cfgMgr.tbl.JumpTable.findByKey(jumpId); } catch (e) {} }
      if (jumpFn) {
        jumpFn.call(gh, jumpCfg || jumpId);
        autoLog("跳转(chL) jumpId=" + jumpId + " sceneType=" + (jumpCfg ? jumpCfg.sceneTypeId : "?") + " hasTbl=" + hasJumpTbl);
        return true;
      }
      // ★ 2026-09-24 兜底寻路：新版是 townScene.fFi（fAJ 在新版是"秘密副本判断"，不是寻路！）
      var ts = fc && fc.townScene;
      if (ts && typeof ts.fGW === "function") {
        ts.fGW({ actType: 1, jumpId: jumpId || 0 });
        autoLog("跳转兜底 fGW jumpId=" + jumpId + " jumpTbl=" + hasJumpTbl);
        return true;
      }
      autoLog("跳转失败: gameHelper.chL && townScene.fGW 均不可用 jumpId=" + jumpId + " ghMethods=" + (gh ? Object.keys(gh).filter(function(k){return typeof gh[k]==='function';}).slice(0,20).join(",") : "null") + " tsMethods=" + (ts ? Object.keys(ts).filter(function(k){return typeof ts[k]==='function';}).slice(0,15).join(",") : "null"));
      return false;
    } catch (e) { autoLog("跳转异常: " + (e && e.message)); return false; }
  }
  /* 寻路到章节入口。 */
  function autoGotoChapter(chapterId) {
    try {
      // 优先用 JumpTable 统一跳转（最可靠）
      var fc = F();
      var gh = fc && fc.gameHelper;
      var jp = null;
      var cfgMgr = fc && fc.cfgMgr;
      if (cfgMgr && cfgMgr.tbl && cfgMgr.tbl.JumpTable) { try { jp = cfgMgr.tbl.JumpTable.findByKey(chapterId); } catch (e) {} }
      var goFn = gh && typeof gh.chL === "function" ? gh.chL : null;
      if (goFn && jp) { goFn.call(gh, jp); return true; }
      var ts = fc && fc.townScene;
      if (!ts || typeof ts.fGW !== "function") return false;
      ts.fGW({ actType: 1, chapter: chapterId });
      return true;
    } catch (e) { return false; }
  }
  /* 寻路到 NPC。 */
  function autoGotoNpc(npcId, jumpId) {
    try {
      // 优先用 JumpTable 统一跳转。
      if (jumpId) {
        var fcN = F();
        var ghN = fcN && fcN.gameHelper;
        var cfgMgrN = fcN && fcN.cfgMgr;
        var jpN = null;
        if (cfgMgrN && cfgMgrN.tbl && cfgMgrN.tbl.JumpTable) { try { jpN = cfgMgrN.tbl.JumpTable.findByKey(jumpId); } catch (e) {} }
        var goFnN = ghN && typeof ghN.chL === "function" ? ghN.chL : null;
        if (goFnN && jpN) { goFnN.call(ghN, jpN); return true; }
      }
      var ts = F() && F().townScene;
      if (!ts || typeof ts.fGW !== "function") return false;
      ts.fGW({ actType: 2, npcId: npcId, jumpId: jumpId || 0 });
      return true;
    } catch (e) { return false; }
  }

  /* 1.1.3.2 离开副本：bEW 判断、bEY 退出、bEX 回城。 */
  function autoExitBattle() {
    try {
      var bm = F() && F().battleMgr;
      if (!bm) return;
      if (typeof bm.bEW === "function" && bm.bEW() && typeof bm.bEY === "function") {
        try { bm.bEY(); autoLog("退出副本(bEY)"); return; } catch (e) {}
      }
      if (typeof bm.bEX === "function") { try { bm.bEX(); autoLog("退出副本(bEX 回城)"); return; } catch (e) {} }
      autoLog("autoExitBattle: 未找到退出接口");
    } catch (e) { autoLog("autoExitBattle 异常: " + (e && e.message)); }
  }
  /* 检查是否在战斗/副本。 */
  function autoInBattle() {
    try {
      var bm = F() && F().battleMgr;
      if (!bm) return false;
      var JE = bm.bEu;
      if (hasBattleActive(JE)) return true;
      return false;
    } catch (e) { return false; }
  }
  /* 检查副本是否通关（结算 WIN）
     结算结果在【控制器.mz.bDK】/【battleMgr.bEv.bDK】
       （game 源码：b = c.default.Kd.mz.bDK; bDK.b = BattleSettleResult）
       BattleSettleResult = { NONE:-1, WIN:1, LOSE:0, DRAW:2 }
       v21/v20 查的 battleMgr.bAB / bCK 在 latest 不存在 → autoSettled 恒 false。 */
  function autoSettled() {
    try {
      var s = getSettle();
      if (s && typeof s.b === "number") return s.b === 1;   // WIN
      return false;
    } catch (e) { return false; }
  }
  /* 自动强化装备一次：读取穿戴装备，选第一个可强化的强化
     return true=强化了一次 */
  function autoStrengthenOnce() {
    try {
      var fc = F();
      var bag = fc && fc.bag;
      var eq = fc && fc.equip;
      if (!bag || !eq || typeof eq.fvK !== "function" || typeof bag.bYn !== "function") return false;
      var wear = [];
      try {
        var te = fc && fc.cfgMgr && fc.cfgMgr.tblEnum;
        var woreType = te && te.BagTableEnum ? te.BagTableEnum.WORE_EQUIP : 201;
        var cont = bag.bYn(woreType);
        wear = cont && cont.c || [];
      } catch (e) {}
      if (!wear.length) { autoLog("没有穿戴装备可强化"); return false; }
      var target = null;
      for (var i = 0; i < wear.length; i++) {
        var w = wear[i];
        try {
          if (!w || !w.itemUid || !w.itemId) continue;
          // 检查是否满级：equip.upgrade 达到 maxUpgrade
          var eqCfg = fc && fc.cfgMgr && fc.cfgMgr.tbl && fc.cfgMgr.tbl.EquipTable && fc.cfgMgr.tbl.EquipTable.findByKey ? fc.cfgMgr.tbl.EquipTable.findByKey(w.itemId) : null;
          var curUp = w.equip && w.equip.upgrade || 0;
          var maxUp = eqCfg ? (eqCfg.maxUpgrade || 999) : 999;
          if (curUp < maxUp) { target = w; break; }  // 找到可强化的
        } catch (e) {}
      }
      if (!target) return false;
      if (!target || !target.itemUid || !target.itemId) return false;
      var param = {
        itemUid: target.itemUid,
        isProtection: false
      };
      try {
        var pr = eq.fvK(param);
        if (pr && pr.then) pr.then(function (res) {
          if (res && res.code !== undefined && res.code !== 0) autoLog("强化失败 code=" + res.code);
        }, function (e) { autoLog("强化请求失败: " + (e && e.message)); });
        autoLog("强化装备: " + target.itemId + " uid=" + target.itemUid);
        return true;
      } catch (e) {
        autoLog("强化调用异常: " + e.message);
        return false;
      }
    } catch (e) { return false; }
  }

  /* 检查强化任务是否完成（process >= num） */
  function autoStrengthDone() {
    try {
      var cur = autoGetCurrentTask();
      if (!cur) return true;  // 任务没了=完成
      var cfg = cur.cfg;
      if (cfg && typeof cfg.num === "number" && typeof cur.task.process === "number") {
        return cur.task.process >= cfg.num;
      }
      // 兜底：任务状态变化（不是进行中）也视为完成
      if (cur.task.status !== 1 && cur.task.status !== 2) return true;
      return false;
    } catch (e) { return true; }
  }

  /* 自动任务主 tick（在 install() 里每 300ms 调用） */
  // ★ 统一停止自动任务：关闭 + localStorage同步 + UI状态
  function autoStop(reason) {
    AUTO.enabled = false;
    AUTO.state = "IDLE";
    lsSet("__WX_AUTO_TASK__", "0");
    // ★ 不推 __WX_AUTO_TASK__ 给 UI（避免驱动 switch 回传）
    state({ autoStatus: reason || "已停止" });
    autoLog("自动任务已停止：" + (reason || ""));
  }
  function autoTaskTick() {
    try {
      /* ★ 2026-09-23 二次修复（"上游戏自动开启"根因）：
         localStorage 里的开关值在"用户本次点击"和"上次会话残留"之间无法区分。
         上次改成"从本地恢复"后，历史残留的 "1" 导致每次进游戏自动开启。
         正确语义：开关只在"用户本次会话点击 wx.auto"后才为开；
         新会话（脚本首次加载）一律从"关"开始，并清掉残留值。
         （"点开被立刻关掉"的问题已在 wx.auto 的容错取值中修复，不再需要靠强制关来规避） */
      if (!AUTO.__inited) {
        AUTO.__inited = true;
        AUTO.enabled = false;
        AUTO.state = "IDLE";
        lsSet("__WX_AUTO_TASK__", "0");
        W.__WX_AUTO_ENABLED__ = false;
        log("[AUTO] 新会话初始化：开关默认关（已清除 localStorage 残留值）");
      } else {
        // 后续 tick：以 localStorage 为准（用户点击 wx.auto 会实时写入）
        var lsOn = (String(lsGet("__WX_AUTO_TASK__", "0")) === "1");
        if (lsOn !== AUTO.enabled) {
          AUTO.enabled = lsOn;
          if (!lsOn) AUTO.state = "IDLE";
        }
      }
      // ★ 修复2：角色未就绪（登录/选角界面）不跑状态机（防止登录期疯狂调任务协议→网络异常）
      var fc0 = F();
      var role0 = fc0 && fc0.role;
      // 首次记录 1.1.3.2 角色数据结构。
      if (!W.__WX_ROLEDIAG__ && role0) {
        W.__WX_ROLEDIAG__ = 1;
        try {
          var rk = Object.keys(role0).slice(0, 30).join(",");
          var roleData = role0.fBb;
          var roleDataKeys = roleData ? Object.keys(roleData).slice(0, 20).join(",") : "null";
          log("[role-diag] roleKeys=[" + rk + "] fBb=" + (roleData ? "ok" : "null") + " fBbKeys=[" + roleDataKeys + "]");
        } catch (e) { log("[role-diag] err:" + e.message); }
      }
      var roleReadyNew = !!(role0 && role0.fBb && role0.fBb.fAQ);
      if (!roleReadyNew) return;
      if (!AUTO.enabled) return;
      var now = Date.now();
      if (now - AUTO.lastTick < 300) return;
      AUTO.lastTick = now;
      /* 1.1.3.2：bRG 是全部引导完成标记，bSq() 关闭 GuidePanel。 */
      try {
        var fcG = F();
        var guideG = fcG && fcG.guide;
        if (guideG && !AUTO.__guideBlocked) {
          AUTO.__guideBlocked = 1;
          guideG.bRG = true;
          if (typeof guideG.bSq === "function") guideG.bSq();
          autoLog("引导已按 1.1.3.2 接口关闭");
        }
      } catch (e) {}
      // ===== 副本结算/奖励弹窗自动处理（★ 2026-09-22 重写，修正接口）=====
      // 游戏原生通关流程：bDK 结算 → AdventureResultPanel → 翻牌/选关。
      //     → 有 cardReward → AdventureTurnCardPanel（翻牌）→ 翻完 → AdventureFinishPanel（选关）
      //     → 无 cardReward → AdventureFinishPanel（选关）直接
      //   RewardPanel（通用"恭喜获得"弹窗）正确关闭方式：
      //     dDA() → 触发 dDj.callFunc()（后续流程），或 dDw()（mask点击，二次关闭）
      //   AdventureFinishPanel（选关界面）：
      //     下一关 eff / 下一层 efe / 选章节 efk / 回城 efd
      try {
        var fcR = F();
        var fgmR = fcR && fcR.fguiMgr;
        if (!fgmR) return;
        // 获取当前最顶层面板类型名
        var topTk = "";
        try {
          var pR = fgmR.cTU ? fgmR.cTU() : null;
          if (pR) topTk = String((pR.vJ && pR.vJ.cUV) || "");
        } catch (e) {}
        // 诊断：输出当前顶层面板（★ 2026-09-24：改为输出全部面板，便于确认跳转后弹什么）
        try {
          if (AUTO.enabled && topTk) {
            // 每 3 秒打一次，避免刷屏
            if ((W.__WX_PANEL_DIAG__ || 0) + 3000 < Date.now()) {
              W.__WX_PANEL_DIAG__ = Date.now();
              var openList = [];
              try {
                var probe = ["AdventureTurnCardPanel", "AdventureResultPanel", "AdventureFinishPanel", "AdventureSelectPanel", "RewardPanel", "InstanceRangePanel", "InstanceCenterPanel", "TowerPanel"];
                for (var pi = 0; pi < probe.length; pi++) { if (isPanelOpen(fgmR, probe[pi])) openList.push(probe[pi]); }
              } catch (e) {}
              autoLog("当前面板: " + topTk + " 已开=[" + (openList.join(",") || "-") + "] (state=" + AUTO.state + ")");
            }
          }
        } catch (e) {}
        /* ---- A. RewardPanel（"恭喜获得"）----
           1.1.3.2：dKu() 继续，dKq() 遮罩回调；先保留 2.6 秒展示时间。 */
        if (isPanelOpen(fgmR, "RewardPanel")) {
          var nowR = Date.now();
          if (!AUTO.__rwdSeenAt) { AUTO.__rwdSeenAt = nowR; AUTO.__rwdDone = false; }
          var waitedR = nowR - AUTO.__rwdSeenAt;
          if (!AUTO.__rwdDone && waitedR > 2600) {
            AUTO.__rwdDone = true;
            try {
              var candsR = getPanelCandidates(fgmR, "RewardPanel");
              var ctrlR = pickPanel(candsR, "dKu") || pickPanel(candsR, "dKq");
              var didR = "";
              if (ctrlR && typeof ctrlR.dKu === "function") {
                ctrlR.dKu(); didR = "dKu(继续)";
              } else if (ctrlR && typeof ctrlR.dKq === "function") {
                ctrlR.dKq(); didR = "dKq(遮罩)";
              }
              if (didR) autoLog("恭喜获得→" + didR + " 停留=" + Math.round(waitedR / 100) / 10 + "s 候选=" + candsR.length);
              else autoLog("恭喜获得: 未找到 dKu/dKq，候选方法=" +
                candsR.map(function (o) { return Object.keys(o).filter(function (k) { return typeof o[k] === "function"; }).slice(0, 8).join("/"); }).join(" | "));
            } catch (e) { autoLog("恭喜获得处理异常: " + (e && e.message)); }
          } else if (!AUTO.__rwdDone && waitedR > 10000) {
            // 极端兜底（理论上不会走到这里）
            try { closePanel(fgmR, "RewardPanel"); autoLog("恭喜获得: 超时兜底关闭"); } catch (e) {}
            AUTO.__rwdDone = true;
          }
        } else {
          // 面板已关闭 → 重置
          if (AUTO.__rwdSeenAt) { AUTO.__rwdSeenAt = 0; AUTO.__rwdDone = false; }
        }
        /* ---- B. 自动翻牌（★ v23 按官方点击 egc 的真实语义重写）----
           官方"点某张牌"的真身 = AdventureTurnCardPanel 里**每张牌的** egc()（源码 @1358707）：
             egc() {
               if (this.efM.length >= this.efN) { 提示"最多仅允许抽取N次"; return; }
               t = this.ega();                       // 计算消耗串/够不够
               e = this.efL;                         // ★这张牌的类型($Dv.c=1/d=2/e=3)
               if (this.efL == $Dv.d) e = $Dv.c;     // ★金卡特权(2) 实际按 免费(1) 发！
               else if (this.efL == $Dv.e && t.cost) {
                 if (!bag.bXM([{itemId,amount}]).isEnough) return;   // 金币不够就别发
               }
               bFh({type: e}) → 成功后播 card_normal(免费/特权) / card_gold(金币) 音效
                              → battleMgr.ks($lH.b, this.efL, this.efK, data)
             }
           另外 eeZ() 决定每个列表显示几张：
             list_free.numItems      = cardReward 含 type=1 ? 4 : 0
             list_privilege.numItems = cardReward 含 type=2 ? 4 : 0
             list_costProp.numItems  = cardReward 含 type=3 ? 4 : 0
           → 所以"这个副本有没有这类牌"完全由 InstanceRewardTable.cardReward 决定。
           本脚本据此实现（先查表 → 再按类型发，避免对没有的牌型乱发导致服务端报错）：
             ① 免费(1)：总是翻（省去 5 秒倒计时）
             ② 金币(3)：UI 开关 __WX_AUTO_FLIP__ 打开 且 消耗足够 才翻
             ③ 特权(2)：金卡可用时，按官方语义用 type=1 翻（额外一次）
           每场每类型只发一次；离开面板自动重置。 */

        /* ---- B. 自动翻牌（★ v28 重写：免费直翻 + 金币由 UI 开关控制，串行发送）----
           官方 AdventureTurnCardPanel 机制（源码 $BJU @1298173 逐行确认）：
             · 免费牌：面板自带倒计时 eeT（初始 5 秒）→ onUIUpdate 里时间到 → efh() 自动翻【免费】
                       efh() 发 bFh({type: $Dv.c=1})，成功后 ks($lH.b, 1, idx, data) → 进选关
             · 金币牌：efa() 自动翻【金币】，条件 = localMgr.cnn($NL.d=4) 且 有金卡(fCL("gold"))
                       每 300ms 检查一次，发 bFh({type: $Dv.e=3})，先校验 bag.bXM(消耗).isEnough
             · egc()（手动点牌）：特权(2) 实际按 免费(1) 发
           ★ 关键：官方免费与金币是【两条独立通道、绝不同 tick 同时发】。
             旧脚本却在一个 tick 里连发 type=1 和 type=3 → 第二次被服务端拒/丢 → 金币翻不了。
           本脚本 v28 策略（串行，一次只发一个）：
             ① 免费(1)：面板一开就直接翻（不等 5 秒倒计时）；本场只发一次
             ② 金币(3)：等免费那次的【响应回来】后，若 UI 开关 __WX_AUTO_FLIP__ 打开、
                        本场有金币牌、且消耗足够 → 再发 type=3；本场只发一次
             ③ 特权(2)：金卡可用时按官方语义用 type=1 补翻一次
           每场每类型只发一次；离开面板自动重置；发送带 3 秒超时保护。 */

        try {
          // cTK 为 1.1.3.2 面板打开判断；打开后才发送翻牌协议。
          var turnReady = panelOpen("AdventureTurnCardPanel");
          if (!turnReady && topTk.indexOf("TurnCard") >= 0) turnReady = true;    // 兜底
          if (turnReady) {
            var bmC = fcR && fcR.battleMgr;
            var stC = AUTO.__card || (AUTO.__card = { free: 0, gold: 0, pending: 0, pendingAt: 0, noApi: 0, noCost: 0 });
            // 卡死保护：发送后 3 秒没响应就解锁，避免 pending 永久卡住
            if (stC.pending && stC.pendingAt && (Date.now() - stC.pendingAt > 3000)) { stC.pending = 0; }
            // 读本副本可用牌型 + 金币消耗项
            var typesC = [], goldItemC = null;
            try {
              var instC = (bmC && bmC.bEu && bmC.bEu.mz && bmC.bEu.mz.bDD && bmC.bEu.mz.bDD.instanceId) || 0;
              var iCfgC = fcR.cfgMgr.tbl.InstanceTable.findByKey(instC);
              var rwC = iCfgC && fcR.cfgMgr.tbl.InstanceRewardTable.findByKey(iCfgC.reward);
              var arrC = (rwC && rwC.cardReward) || [];
              for (var c1 = 0; c1 < arrC.length; c1++) {
                typesC.push(arrC[c1][0]);
                if (arrC[c1][0] === 3) goldItemC = arrC[c1];
              }
            } catch (e) {}
            if (!typesC.length) typesC = [1];
            var hasApiC = !!(bmC && typeof bmC.bFh === "function");
            if (!hasApiC) {
              if (!stC.noApi) { stC.noApi = 1; autoLog("翻牌: battleMgr.bFh 不可用"); }
            } else if (!stC.pending) {
              /* ★ v29：服务端 code=20 = RequestTooFast（请求太快）。
                 免费与金币两次 selectCard 之间必须有足够间隔，否则第二次被拒。
                 这里强制"距上次发送 ≥600ms"才允许发下一次。 */
              var sinceLastC = Date.now() - (stC.lastAt || 0);
              // ① 免费牌：直接翻（不等倒计时）——一次只发一个
              if (!stC.free && (typesC.indexOf(1) >= 0 || typesC.indexOf(2) >= 0)) {
                stC.pending = 1; stC.pendingAt = Date.now(); stC.lastAt = Date.now();
                var idxF = Math.floor(4 * Math.random());
                var pF = bmC.bFh({ type: 1 });
                if (pF && pF.then) {
                  pF.then(function (res) {
                    stC.pending = 0;
                    if (res && res.code === 0) {
                      stC.free = 1;
                      try { bmC.ks(getSelectCardEvent(), 1, idxF, res.data); } catch (e) {}
                      autoLog("翻牌(免费) 成功 type=1");
                    } else { autoLog("翻牌(免费) 返回 code=" + (res && res.code) + (res && res.code === 20 ? "(请求太快)" : "")); }
                  }, function (err) { stC.pending = 0; autoLog("翻牌(免费) 拒绝: " + err); });
                } else { stC.pending = 0; stC.free = 1; autoLog("翻牌(免费) 已发送 type=1"); }
              }
              // ② 金币牌：等免费那次结束后 + 间隔≥600ms，UI 开关开 + 有金币牌 + 消耗够 → 翻 type=3
              else if (!stC.gold && sinceLastC >= 600 && CFG.autoFlip() && typesC.indexOf(3) >= 0 && goldItemC) {
                var enoughC = false;
                try {
                  var chkC = fcR.bag.bXM([{ itemId: goldItemC[1], amount: goldItemC[2] }], false, false, true);
                  enoughC = !!(chkC && chkC.isEnough);
                } catch (e) {}
                if (!enoughC) {
                  if (!stC.noCost) { stC.noCost = 1; autoLog("翻牌(金币) 消耗不足，跳过"); }
                } else {
                  stC.pending = 1; stC.pendingAt = Date.now(); stC.lastAt = Date.now();
                  var idxG = Math.floor(4 * Math.random());
                  var pG = bmC.bFh({ type: 3 });
                  if (pG && pG.then) {
                    pG.then(function (res) {
                      stC.pending = 0;
                      if (res && res.code === 0) {
                        stC.gold = 1;
                        try { bmC.ks(getSelectCardEvent(), 3, idxG, res.data); } catch (e) {}
                        autoLog("翻牌(金币) 成功 type=3");
                      } else if (res && res.code === 20) {
                        stC.lastAt = Date.now();   // 太快 → 下轮再试（不标记完成）
                        autoLog("翻牌(金币) code=20 请求太快，稍后重试");
                      } else { autoLog("翻牌(金币) 返回 code=" + (res && res.code)); }
                    }, function (err) { stC.pending = 0; autoLog("翻牌(金币) 拒绝: " + err); });
                  } else { stC.pending = 0; stC.gold = 1; autoLog("翻牌(金币) 已发送 type=3"); }
                }
              }
              // ③ 特权牌(2) 且本场没有免费牌时，金卡可用则按官方语义补翻 type=1
              else if (!stC.free && !stC.gold && typesC.indexOf(2) >= 0 && typesC.indexOf(1) < 0) {
                var hasGold2 = false;
                try { hasGold2 = !!(fcR.monCard && fcR.monCard.fDn && fcR.monCard.fDn.fDc && fcR.monCard.fDn.fDc("gold")); } catch (e) {}
                if (hasGold2) {
                  stC.pending = 1; stC.pendingAt = Date.now();
                  var pP = bmC.bFh({ type: 1 });
                  if (pP && pP.then) {
                    pP.then(function (res) { stC.pending = 0; if (res && res.code === 0) { stC.free = 1; autoLog("翻牌(金卡特权) 成功 type=1"); } }, function () { stC.pending = 0; });
                  } else { stC.pending = 0; stC.free = 1; }
                }
              }
            }
          } else {
            // 离开翻牌面板 → 重置本场标记（供下一场使用）
            AUTO.__card = null;
          }
        } catch (e) { autoLog("翻牌处理异常: " + (e && e.message)); }
        /* ---- B2. AdventureResultPanel（结算结果面板）----
           1.1.3.2：efA() 继续；超时才用新版 cTE() 关闭。 */
        if (panelOpen("AdventureResultPanel") || topTk.indexOf("AdventureResultPanel") >= 0) {
          if (!AUTO.__resultSeenAt) AUTO.__resultSeenAt = Date.now();
          if (Date.now() - AUTO.__resultSeenAt > 4000 && !AUTO.__resultForced) {
            AUTO.__resultForced = true;
            try {
              var fcRes = F();
              var fgmRes = fcRes && fcRes.fguiMgr;
              var instRes = pickPanel(getPanelCandidates(fgmRes, "AdventureResultPanel"), "efA");
              if (instRes && typeof instRes.efA === "function") {
                instRes.efA();
                autoLog("结算结果面板停留超时→继续(efA)");
              } else if (closePanel(fgmRes, "AdventureResultPanel")) autoLog("结算结果面板停留超时→关闭(cTE)");
            } catch (e) { autoLog("结算面板推进异常: " + (e && e.message)); }
          }
        } else {
          AUTO.__resultSeenAt = 0; AUTO.__resultForced = false;
        }
        // ---- C. AdventureFinishPanel（选关界面）自动处理 ----
        // 自动刷图：优先调用新版下一关/下一层/选章节入口，无法继续时返回城镇。
        if (!CFG.autoRepeat() && (panelOpen("AdventureFinishPanel") || topTk.indexOf("AdventureFinishPanel") >= 0) && !AUTO.__finishScheduled) {
          AUTO.__finishScheduled = true;
          setTimeout(function(){
            AUTO.__finishScheduled = false;
            try {
              if (CFG.autoRepeat()) return;
              var fcF = F();
              var fgmF = fcF && fcF.fguiMgr;
              if (!fgmF) return;
              /* 1.1.3.2 onUILoad：eff 下一关、efe 下一层、efk 选章节、efd 回城。 */
              var fp = getFinishPanel(fgmF);
              if (!fp) return;
              /* ★ v22：不再只取单一对象，改为在候选列表里找"真正实现了目标方法"的那个 */
              var candsF = fp.cands || getPanelCandidates(fgmF, "AdventureFinishPanel");
              function pickFn(names) {
                for (var ci2 = 0; ci2 < candsF.length; ci2++) {
                  for (var nj = 0; nj < names.length; nj++) {
                    if (typeof candsF[ci2][names[nj]] === "function") return { obj: candsF[ci2], fn: names[nj] };
                  }
                }
                return null;
              }
              var panelF = fp.panel;
              var realF = panelF;
              /* 官方推进方法自身会处理下一关和回城判断，脚本不重复猜任务状态。 */
              var tInfo = "";
              try {
                var curF = autoGetCurrentTask();
                if (curF) {
                  var tF = autoAnalyzeTask(curF);
                  tInfo = "task=" + curF.task.id + "/" + (tF && tF.type) + (tF && tF.instId ? ("#" + tF.instId) : "") + "/" + (tF && tF.eventType);
                } else tInfo = "task=none";
              } catch (e) { tInfo = "task=err"; }
              /* ★ v22：同一面板实例只点一次（否则 300ms tick 会重复点 → 重复发进副本协议），
                 面板被销毁/更换（重新弹出）后自动解除。 */
              if (AUTO.__finishHandled === panelF) return;
              AUTO.__finishHandled = panelF;
              var advanced = false;
              try {
                var nx = pickFn(["eff", "efe", "efk"]);
                if (nx) {
                  nx.obj[nx.fn](nx.fn === "eff" || nx.fn === "efe" ? null : undefined, nx.fn === "eff" || nx.fn === "efe" ? 1 : undefined);
                  advanced = true;
                  /* ★v33k：记录"点了下一关"的时刻，SETTLE 分支据此识别场景加载窗口 */
                  AUTO.__nextStageAt = Date.now();
                  AUTO.__loadingLogged = 0;
                  autoLog("选关→" + nx.fn + " [" + tInfo + "]");
                }
              } catch (e) {}
              if (!advanced) {
                try {
                  var rt = pickFn(["efd"]);
                  if (rt) { rt.obj[rt.fn](); autoLog("选关→" + rt.fn + "(返回城镇) [" + tInfo + "]"); advanced = true; }
                } catch (e) {}
              }
              if (!advanced) {
                autoLog("选关→未找到下一关/返回方法 候选方法=" +
                        candsF.map(function (o) { return Object.keys(o).filter(function (k) { return typeof o[k] === 'function'; }).slice(0, 8).join("/"); }).join(" | ") + " [" + tInfo + "]");
              }
              // 诊断：输出真面板方法名（前 20 个，便于核对）
              try {
                if ((AUTO.__finishDiagT || 0) + 15000 < Date.now()) {
                  AUTO.__finishDiagT = Date.now();
                  var fm = panelF ? Object.keys(panelF).filter(function(k){return typeof panelF[k]==='function';}).slice(0,20) : [];
                  autoLog("[FINISH-DIAG] realF=" + (realF?"有":"无") + " methods=[" + fm.join(",") + "]");
                }
              } catch (e) {}
            } catch (e) {}
          }, 1500);
        }
        /* ---- E9. FuncOpenPanel（功能开启提示）自动关闭 ----
           ★ v25 新增：做完任务/进新章节会弹"功能开启"提示，挡住后续流程（日志实测卡住）。 */
        if (panelOpen("FuncOpenPanel") || topTk.indexOf("FuncOpenPanel") >= 0) {
          if ((AUTO.__fopLastAt || 0) + 1500 <= Date.now()) {
            AUTO.__fopLastAt = Date.now();
            var didFop = false;
            try {
              var cFop = pickPanel(getPanelCandidates(fgmR, "FuncOpenPanel"), "eeV");
              if (cFop && typeof cFop.eeV === "function") { cFop.eeV(); didFop = "eeV"; }
            } catch (e) {}
            if (!didFop) { if (closePanel(fgmR, "FuncOpenPanel")) didFop = "cTE"; }
            if (didFop) autoLog("功能开启提示→" + didFop);
          }
        }
        /* ---- F. ScrollNoticePanel（滚动公告）自动关闭 ----
           ★ 2026-09-23 优化（修"疯狂刷屏"）：公告面板关闭加 1 秒冷却，
             避免"关了又弹→再关"高频循环（实测 40 秒刷 40 次）。 */
        if ((panelOpen("ScrollNoticePanel") || topTk.indexOf("ScrollNoticePanel") >= 0) && !AUTO.__noticeScheduled) {
          AUTO.__noticeScheduled = true;
          // ★ 冷却：1 秒内不重复关（避免刷屏）；但不 return，免得跳过后面面板处理
          if ((AUTO.__noticeLastAt || 0) + 1000 <= Date.now()) {
          AUTO.__noticeLastAt = Date.now();
          setTimeout(function(){
            AUTO.__noticeScheduled = false;
            try {
              var fcN = F();
              var fgmN = fcN && fcN.fguiMgr;
              if (!fgmN) return;
              var pN = pickPanel(getPanelCandidates(fgmN, "ScrollNoticePanel"), "eVH");
              if (!pN) return;
              var tkN = "";
              try { tkN = String((pN.vJ && pN.vJ.cUV) || "ScrollNoticePanel"); } catch (e) {}
              if (tkN.indexOf("ScrollNoticePanel") < 0) return;
              /* ★ 2026-09-23 按源码修正公告关闭（game/index.js 101653 + main/index.js 14936）：
                 面板控制器方法：
                   eQl()  → 隐藏公告（alpha=0, eQh=false）
                 公告队列（$eZ.account.fsL）：
                   bdd()  → ★ 真正清空队列（bcV=[], bcW=0）  ← 正确方法！
                   bcZ()  → 只 pop 最后一个（❌ 旧代码用错，导致永远清不空→疯狂刷屏）
                   bdc()  → 剩余数量
                 处理：先清空队列（bdd）再隐藏面板（eQl），否则下一条公告又弹。 */
              var closed = false;
              try { if (typeof pN.eVH === "function") { pN.eVH(); closed = true; } } catch (e) {}
              if (!closed) closed = closePanel(fgmN, "ScrollNoticePanel");
              try {
                var nw2 = jsb && jsb.fileUtils;
                if (nw2 && typeof nw2.writeStringToFile === "function") {
                  var np2 = (nw2.getWritablePath ? nw2.getWritablePath() : "/data/user/0/com.fightingstars.games.warrior/files/") + "wx_auto_log.txt";
                  var nold2 = "";
                  try { if (typeof nw2.getStringFromFile === "function") nold2 = nw2.getStringFromFile(np2) || ""; } catch (e) {}
                  nw2.writeStringToFile((nold2 + "[" + new Date().toISOString().slice(11,19) + "] [公告] 已自动关闭\n"), np2);
                }
              } catch (e) {}
             } catch (e) {}
           }, 1500);
          }  // ★ 闭合公告冷却 if
        }
        // ---- D. InstanceRangePanel（副本详情）自动点挑战 ----
        // 日常/远古副本：跳转到副本中心后，在详情面板调用新版挑战入口。
        //  ★ v23：控制器由 getPanelCandidates() 取得（cSj 是"打开面板"，会导致误触发）
        if ((panelOpen("InstanceRangePanel") || topTk.indexOf("InstanceRangePanel") >= 0) && !AUTO.__instanceFightScheduled) {
          AUTO.__instanceFightScheduled = true;
          setTimeout(function(){
            AUTO.__instanceFightScheduled = false;
            try {
              var fcI = F();
              var fgmI = fcI && fcI.fguiMgr;
              if (!fgmI) return;
              var pI = pickPanel(getPanelCandidates(fgmI, "InstanceRangePanel"), "enZ");
              if (!pI) return;
              var tkI = "";
              try { tkI = String((pI.vJ && pI.vJ.cUV) || "InstanceRangePanel"); } catch (e) {}
              if (tkI.indexOf("InstanceRangePanel") < 0) return;
              // ★ 2026-09-23：点挑战加"重试上限 + 冷却"，防止未解锁/条件不满足时无限点击死循环。
              //   同一轮实例目标（instId）最多点 5 次；超过则放弃并回城重读任务。
              var fightKey = "F_" + (AUTO.gotoInfo && AUTO.gotoInfo.instId || 0);
              if (AUTO.__fightKey !== fightKey) { AUTO.__fightKey = fightKey; AUTO.__fightCount = 0; }
              AUTO.__fightCount = (AUTO.__fightCount || 0) + 1;
              if (AUTO.__fightCount > 5) {
                if (!AUTO.__fightGaveUp) {
                  AUTO.__fightGaveUp = true;
                  autoLog("副本详情：点挑战超过 5 次仍无法进入（可能未解锁/条件不足），跳过本任务");
                }
                AUTO.state = "IDLE";
                setTimeout(function(){ AUTO.__fightGaveUp = false; AUTO.state = "READ_TASK"; }, 5000);
                return;
              }
              if (AUTO.__fightCount > 1) {
                // 重试间隔放慢到 2.5 秒，避免疯狂点
                if ((AUTO.__fightLastAt || 0) + 2500 > Date.now()) return;
              }
              AUTO.__fightLastAt = Date.now();
              // 1.1.3.2 InstanceRangePanel.btn_fight 绑定 enZ。
              if (typeof pI.enZ === "function") {
                try { pI.enZ(); autoLog("副本详情→自动点挑战(enZ) 第" + AUTO.__fightCount + "次"); }
                catch (e) { autoLog("副本详情→挑战调用异常(enZ): " + e); }
              } else {
                autoLog("InstanceRangePanel: enZ 不可用，控制器=" + tkI);
              }
            } catch (e) {}
          }, 1500);
        }
        /* 1.1.3.2 TowerPanel.btn_fight 绑定 fhE，内部完成解锁判断并调用 destroyTower.cgh。 */
        if ((panelOpen("TowerPanel") || topTk.indexOf("TowerPanel") >= 0) && !AUTO.__towerScheduled) {
          AUTO.__towerScheduled = true;
          setTimeout(function(){
            AUTO.__towerScheduled = false;
            try {
              var fcT = F();
              var fgmT = fcT && fcT.fguiMgr;
              if (!fgmT) return;
              var pT = pickPanel(getPanelCandidates(fgmT, "TowerPanel"), "fhE");
              if (!pT) return;
              var tkT = "";
              try { tkT = String((pT.vJ && pT.vJ.cUV) || "TowerPanel"); } catch (e) {}
              if (tkT.indexOf("TowerPanel") < 0) return;
              // 出战重试上限（防死循环）
              if ((AUTO.__towerCount || 0) > 4) {
                if (!AUTO.__towerGaveUp) {
                  AUTO.__towerGaveUp = true;
                  autoLog("毁灭之塔：出战超过 4 次未成功（可能前置楼层未通关），跳过");
                }
                AUTO.__cooldownUntil = Date.now() + 8000;
                AUTO.state = "IDLE";
                return;
              }
              AUTO.__towerCount = (AUTO.__towerCount || 0) + 1;
              pT.fhE();
              autoLog("毁灭之塔→出战(fhE) 第" + AUTO.__towerCount + "次");
            } catch (e) { autoLog("毁灭之塔处理异常: " + (e && e.message)); }
          }, 1500);
        }
        // ---- E. InstanceCenterPanel（副本中心）打开时，自动选中目标副本打开详情 ----
        // 日常/远古副本：到副本中心后需打开对应详情面板。
        // 通过 jumpId 反查 InstanceTypeTable，再用 cTA 打开详情。
        if ((panelOpen("InstanceCenterPanel") || topTk.indexOf("InstanceCenterPanel") >= 0) && !AUTO.__centerScheduled) {
          AUTO.__centerScheduled = true;
          setTimeout(function(){
            AUTO.__centerScheduled = false;
            try {
              var fcC = F();
              var fgmC = fcC && fcC.fguiMgr;
              if (!fgmC) return;
              // 获取当前任务的 jumpId
              var cj = 0;
              try { cj = (AUTO.gotoInfo && AUTO.gotoInfo.jumpId) || 0; } catch (e) {}
              if (!cj) {
                try {
                  var curC = autoGetCurrentTask();
                  if (curC && curC.cfg && curC.cfg.jumpId) cj = curC.cfg.jumpId;
                } catch (e) {}
              }
              if (!cj) { autoLog("副本中心: 无 jumpId，无法定位副本"); return; }
              // 反查副本类型：JumpTable[jumpId].funcId → InstanceTypeTable.findFirst(funcOpen===funcId)
              var jpC = null, typeC = null;
              try {
                var tblC = fcC && fcC.cfgMgr && fcC.cfgMgr.tbl;
                jpC = tblC && tblC.JumpTable && tblC.JumpTable.findByKey ? tblC.JumpTable.findByKey(cj) : null;
                if (jpC && jpC.funcId && tblC && tblC.InstanceTypeTable && tblC.InstanceTypeTable.findFirst) {
                  typeC = tblC.InstanceTypeTable.findFirst(function(t){ return t.funcOpen === jpC.funcId; });
                }
              } catch (e) {}
              if (!typeC) { autoLog("副本中心: 未反查到副本类型 jumpId=" + cj + " funcId=" + (jpC && jpC.funcId)); return; }
              // 打开副本详情（InstanceRangePanel）
              if (fgmC && typeof fgmC.cTA === "function") {
                closePanel(fgmC, "InstanceCenterPanel");
                var rangeCfgC = getPanelCfg("InstanceRangePanel");
                if (!rangeCfgC) return;
                fgmC.cTA(rangeCfgC, typeC.id);
                autoLog("副本中心→打开详情 InstanceType=" + typeC.id + " (jumpId=" + cj + ")");
              }
            } catch (e) { autoLog("副本中心处理异常: " + (e && e.message)); }
          }, 1500);
        }
// ---- G. AdventureSelectPanel（冒险选择/主线副本）自动点确认进入 ----
        // 主线副本打开 AdventureSelectPanel 后调用 dhl 确认进入。
        if ((panelOpen("AdventureSelectPanel") || topTk.indexOf("AdventureSelectPanel") >= 0) && !AUTO.__advSelectScheduled) {
          AUTO.__advSelectScheduled = true;
          setTimeout(function(){
            AUTO.__advSelectScheduled = false;
            try {
              var fcA = F();
              var fgmA = fcA && fcA.fguiMgr;
              if (!fgmA) return;
              var pA = pickPanel(getPanelCandidates(fgmA, "AdventureSelectPanel"), "dhl");
              if (!pA) return;
              var tkA = "";
              try { tkA = String((pA.vJ && pA.vJ.cUV) || "AdventureSelectPanel"); } catch (e) {}
              if (tkA.indexOf("AdventureSelectPanel") < 0 && !panelOpen("AdventureSelectPanel")) return;
              /* ★ v22：同一面板实例只确认一次（否则 300ms tick 会重复发「进入副本」协议） */
              var candsA = getPanelCandidates(fgmA, "AdventureSelectPanel");
              var pAny = pickPanel(candsA, "dhl") || pA;
              if (AUTO.__advHandled === pAny) return;
              AUTO.__advHandled = pAny;
              pA = pAny;
              // 1.1.3.2 btn_confirm 绑定 dhl。
              var done = false;
              if (typeof pA.dhl === "function") {
                try { pA.dhl(); autoLog("冒险选择→点确认挑战(dhl)"); done = true; } catch (e) {}
              }
              if (!done && pA.btn_confirm) {
                try {
                  // fgui 标准点击：onClick(handler, target, sender) —— 用面板自身当 sender
                  if (typeof pA.btn_confirm.fire === "function") { pA.btn_confirm.fire(cc.Event.EventCustom ? "onClick" : "click"); done = true; }
                } catch (e) {}
              }
              if (!done) {
                autoLog("AdventureSelectPanel: 未找到确认方法(期望 dhl) 控制器=" + tkA +
                        " 方法=" + Object.keys(pA).filter(function(k){return typeof pA[k]==='function';}).join(","));
              }
            } catch (e) {}
          }, 1500);
        }
      } catch (e) {}
      var inBattle = autoInBattle();
      switch (AUTO.state) {
        case "IDLE":
          // ★ 2026-09-23：冷却期内不进入 READ_TASK（修复 guide 等分支的 300ms 死循环）
          if (AUTO.__cooldownUntil && now < AUTO.__cooldownUntil) break;
          AUTO.__cooldownUntil = 0;
          AUTO.state = "READ_TASK";
          autoLog("开始读取主线任务...");
          break;
        case "READ_TASK": {
          // 拉取 1.1.3.2 任务组，并用冷却避免重复请求。
          refreshTaskGroup(false);
          var cur = autoGetCurrentTask();
          if (!cur) {
            // 异步任务数据尚未同步时等待下一轮，不误判为全部完成。
            // ★ 节流：该提示 5 秒只打一次（避免每 300ms 刷屏）
            if ((AUTO.__loadMsgLast || 0) + 5000 < now) {
              AUTO.__loadMsgLast = now;
              autoLog("任务数据加载中，稍后重试...");
            }
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 2000);
            return;
          }
          var t = autoAnalyzeTask(cur);
          var cfg = cur.cfg;
          AUTO.curTaskId = cur.task.id;
          AUTO.curTaskIndex = cur.index;
          AUTO.curDesc = t.desc || (cfg && cfg.desc ? autoGetText(cfg.desc) : ("任务 #" + cur.task.id));
          AUTO.curProgress = (cur.task.process || 0) + "/" + (cfg && typeof cfg.num === "number" ? cfg.num : "?");
          state({ autoTaskDesc: AUTO.curDesc, autoTaskProgress: AUTO.curProgress, autoTaskTarget: t.type });
          if (t.type === "submit") {
            var okSub = autoSubmitTask(cur);
            AUTO.state = "IDLE";
            if (okSub) {
              AUTO.__cooldownUntil = Date.now() + 2500;
              autoLog("已发起提交，等待刷新 #" + (cur.task && cur.task.id));
            } else {
              var subSkipCnt = parseInt(lsGet("__WX_UNK_SKIP_CNT__", "0"), 10) || 0;
              if (subSkipCnt < 8) {
                lsSet("__WX_UNK_SKIP_CNT__", String(subSkipCnt + 1));
                autoLog("任务 #" + (cur.task && cur.task.id) + " 暂不可领取，跳过 (" + (subSkipCnt + 1) + "/8)");
              } else {
                autoLog("连续 8 次无法领取任务，停止自动任务");
                autoStop("任务无法领取");
              }
              AUTO.__cooldownUntil = Date.now() + 5000;
            }
          } else if (t.type === "instance") {
            // 有 jumpId 的特殊副本优先走新版跳转链路。
            AUTO.curTarget = "副本 #" + t.instId;
            state({ autoTaskTarget: AUTO.curTarget });
            var instJumpId = t.jumpId || (cfg && cfg.jumpId);
            if (inBattle) {
              AUTO.state = "IN_BATTLE";
              // ★ 2026-09-23 修复：进入 IN_BATTLE 必须刷新 battleStart，
              //   否则沿用上次战斗的旧时间戳 → 立刻误判"战斗超时180s"→ 强退 → 网络异常。
              AUTO.battleStart = now;
              autoLog("已在副本中，开始自动战斗");
            } else if (instJumpId) {
              /* ★ 2026-09-24 重写：优先"直接进副本"（官方 ecD 逻辑），跳转作为兜底。
                 InstanceTable 能解析目标时直接调用 battleMgr.bFo；否则才走入口跳转。 */
              AUTO.state = "GOTO";
              AUTO.gotoInfo = { type: "instance", instId: t.instId, jumpId: instJumpId };
              AUTO.gotoStart = now;
              // ① 先尝试直接进副本（InstanceTable 存在才进）
              var enteredDirect = false;
              try {
                var fcD = F();
                var tblD = fcD && fcD.cfgMgr && fcD.cfgMgr.tbl;
                var bmD = fcD && fcD.battleMgr;
                var instCfgD = (tblD && tblD.InstanceTable && tblD.InstanceTable.findByKey) ? tblD.InstanceTable.findByKey(t.instId) : null;
                if (instCfgD && battleEnter(t.instId, 0)) {
                  enteredDirect = true;
                  autoLog("直接进入副本 id=" + t.instId + " (InstanceTable 命中)");
                } else if (!AUTO.__instDirectDiag) {
                  AUTO.__instDirectDiag = 1;
                  autoLog("[inst-direct-diag] instId=" + t.instId + " instCfg=" + (instCfgD ? "hit" : "miss") +
                          " hasTbl=" + !!(tblD && tblD.InstanceTable) + " enter=" + (!!ENTER_FN) +
                          " tblKeys=" + (tblD ? Object.keys(tblD).slice(0,25).join(",") : "null"));
                }
              } catch (e) { autoLog("直接进副本次异常: " + e.message); }
              if (!enteredDirect) {
                autoLog("跳转到副本 #" + t.instId + " (jumpId=" + instJumpId + ")");
                autoJumpByConfig(instJumpId);
              }
              // ★ 2026-09-23 修复（"进不去副本 / 主动打开详情失败"）：
              //   原逻辑硬编码 tbl.InstanceTypeTable.findFirst(funcOpen===funcId) 反查副本类型，
              //   实际游戏里该表/字段名不一定叫这个 → typeJ2 为空 → 打开详情失败。
              //   改为：① 先判断 jumpId 对应的 sceneType；
              //         ② sceneType=107(ADVENTURE 主线副本) → 走 AdventureSelectPanel.dhl 确认，
              //            不要走 InstanceRangePanel 那套（反查本来就对冒险副本无效）；
              //         ③ 其他副本（日常/远古 走副本中心）才做反查，且多字段兜底 + dump 诊断。
              var jpJ2_0 = null;
              try {
                var tblJ0 = F() && F().cfgMgr && F().cfgMgr.tbl;
                if (tblJ0 && tblJ0.JumpTable && tblJ0.JumpTable.findByKey) jpJ2_0 = tblJ0.JumpTable.findByKey(instJumpId);
              } catch (e) {}
              var sceneT = jpJ2_0 ? (jpJ2_0.sceneTypeId || jpJ2_0.sceneType || 0) : 0;
              if (sceneT === 107) {
                // 主线冒险副本：等待 AdventureSelectPanel.dhl 自动确认进入。
                autoLog("主线冒险副本(sceneType=107)，等待 AdventureSelectPanel 自动确认进入");
              } else {
              setTimeout(function(){
                try {
                  var fcJ2 = F();
                  var fgmJ2 = fcJ2 && fcJ2.fguiMgr;
                  var tblJ2 = fcJ2 && fcJ2.cfgMgr && fcJ2.cfgMgr.tbl;
                  var jpJ2 = tblJ2 && tblJ2.JumpTable && tblJ2.JumpTable.findByKey ? tblJ2.JumpTable.findByKey(instJumpId) : null;
                  var typeJ2 = null;
                  // ★ 多字段反查：InstanceTypeTable 用 funcOpen / funcId / id / typeId 任一匹配都试
                  if (jpJ2 && tblJ2 && tblJ2.InstanceTypeTable && tblJ2.InstanceTypeTable.findFirst) {
                    var fid = jpJ2.funcId;
                    if (fid != null) {
                      typeJ2 = tblJ2.InstanceTypeTable.findFirst(function(x){ return x.funcOpen === fid || x.funcId === fid; });
                    }
                    if (!typeJ2 && jpJ2.typeId != null) {
                      typeJ2 = tblJ2.InstanceTypeTable.findFirst(function(x){ return x.id === jpJ2.typeId || x.typeId === jpJ2.typeId; });
                    }
                    // 兜底：用副本实例 id 前缀/分类反查
                    if (!typeJ2 && jpJ2.instanceType != null) {
                      typeJ2 = tblJ2.InstanceTypeTable.findFirst(function(x){ return x.id === jpJ2.instanceType; });
                    }
                  }
                  // 诊断：dump 表是否可用 + 表项样例（每 10 秒一次，避免刷屏）
                  try {
                    if ((AUTO.__instTblDiag || 0) + 10000 < Date.now()) {
                      AUTO.__instTblDiag = Date.now();
                      var hasTbl = !!(tblJ2 && tblJ2.InstanceTypeTable);
                      var sample = "";
                      try {
                        var it = tblJ2 && tblJ2.InstanceTypeTable;
                        if (it) {
                          var first = null;
                          if (it.findFirst) first = it.findFirst(function(){ return true; });
                          else if (it.list && it.list.length) first = it.list[0];
                          if (first) sample = JSON.stringify({ id: first.id, funcOpen: first.funcOpen, funcId: first.funcId, typeId: first.typeId, name: first.name }).slice(0,300);
                        }
                      } catch (e) {}
                      autoLog("[副本诊断] hasTbl=" + hasTbl + " jp.funcId=" + (jpJ2 && jpJ2.funcId) + " jp.typeId=" + (jpJ2 && jpJ2.typeId) + " sceneType=" + (jpJ2 && jpJ2.sceneTypeId) + " sample=" + sample);
                    }
                  } catch (e) {}
                  if (typeJ2 && fgmJ2 && typeof fgmJ2.cTA === "function") {
                    closePanel(fgmJ2, "InstanceCenterPanel");
                    var rangeCfgJ2 = getPanelCfg("InstanceRangePanel");
                    if (!rangeCfgJ2) return;
                    fgmJ2.cTA(rangeCfgJ2, typeJ2.id);
                    autoLog("主动打开副本详情 InstanceType=" + typeJ2.id + " (jumpId=" + instJumpId + ")");
                  } else {
                    // 反查失败属于"可接受"：主线副本会由 AdventureSelectPanel 分支兜住，
                    // 这里只记录，不再当作错误（避免刷屏干扰判断）
                    autoLog("副本详情反查未命中(将由面板自动流程兜底): jp.funcId=" + (jpJ2 && jpJ2.funcId) + " sceneType=" + (jpJ2 && jpJ2.sceneTypeId));
                  }
                } catch (e) { autoLog("主动打开详情异常: " + (e && e.message)); }
              }, 2500);
              }
            } else {
              AUTO.state = "GOTO";
              AUTO.gotoInfo = { type: "instance", instId: t.instId };
              AUTO.gotoStart = now;
              autoLog("寻路到副本 #" + t.instId + " 入口...");
              autoGotoChapter(0);  // 先尝试打开章节面板
            }
          } else if (t.type === "decompose" || t.type === "sell" || t.type === "bless" ||
                     t.type === "craft" || t.type === "adjust") {
            /* 装备分解/出售/祝福/打造/品级调整：
               有批量接口就调，否则上报事件推进（这些需要手动选材料） */
            var nameMapX = { decompose: "装备分解", sell: "装备出售", bless: "装备祝福", craft: "装备打造", adjust: "品级调整" };
            AUTO.curTarget = nameMapX[t.type] || t.type;
            state({ autoTaskTarget: AUTO.curTarget });
            var didX = false;
            try {
              if ((t.type === "decompose" && CFG.autoDecompose()) || (t.type === "sell" && CFG.autoSell())) {
                didX = !!autoSellDecomposeNow();
              }
            } catch (e) {}
            if (!didX) {
              try { reportClientEvent(t.eventType || 0, []); } catch (e) {}
              autoLog(AUTO.curTarget + ": 无自动接口，已上报事件 " + (t.eventType || "?") + "（可手动完成后继续）");
            }
            AUTO.state = "IDLE";
            AUTO.__cooldownUntil = Date.now() + 3000;
            break;
          } else if (t.type === "wear") {
            AUTO.curTarget = "装备穿戴";
            state({ autoTaskTarget: AUTO.curTarget });
            try {
              var didW = autoWearBestEquip(0);
              if (didW) autoLog("装备穿戴: 已调用 equip.ftA/fvH");
              else {
                try { reportClientEvent(t.eventType || 1101, []); } catch (e) {}
                autoLog("装备穿戴: 无自动接口，已上报事件 " + (t.eventType || 1101));
              }
            } catch (e) { autoLog("装备穿戴异常: " + (e && e.message)); }
            AUTO.state = "IDLE";
            AUTO.__cooldownUntil = Date.now() + 3000;
            break;
          } else if (t.type === "petEgg" || t.type === "petOp" || t.type === "equipMisc" || t.type === "skillUp") {
            var __nst = (t.type === "petEgg") ? "PET_EGG" : (t.type === "petOp" ? "PET_OP" : "EQUIP_MISC");
            AUTO.state = __nst;
            autoLog("新任务类型 " + t.type + " → " + __nst + "（" + (t.desc || t.name || "") + "）");
          } else if (t.type === "enchant") {
            AUTO.curTarget = "装备附魔";
            state({ autoTaskTarget: AUTO.curTarget });
            AUTO.state = "ENCHANT";
            AUTO.enchantStart = 0;
            autoLog("进入附魔流程（一键镶嵌魂魄）");
          } else if (t.type === "gev") {
            AUTO.curTarget = "宝石镶嵌/刻印";
            state({ autoTaskTarget: AUTO.curTarget });
            AUTO.state = "GEM_INLAY";
            AUTO.gemStart = 0;
            autoLog("进入镶嵌/刻印流程");
          } else if (t.type === "npc") {
            AUTO.curTarget = "NPC 对话";
            state({ autoTaskTarget: AUTO.curTarget });
            // 对话任务通过 1.1.3.2 task.cvr 上报。
            var npcId = t.npcId;
            var jumpId = t.jumpId;
            if (!npcId && t.story && t.story[0]) npcId = t.story[0][2];
            if (npcId) {
              try {
                var fcN = F();
                if (reportTalkNpc(npcId)) {
                  autoLog("对话任务直接上报完成 (NPC #" + npcId + ")");
                } else {
                  autoLog("上报 NPC 接口不可用(cvr)，尝试跳转对话");
                  autoGotoNpc(npcId, jumpId);
                }
              } catch (e) {
                autoLog("对话上报异常，尝试跳转: " + e.message);
                autoGotoNpc(npcId, jumpId);
              }
            } else {
              autoLog("NPC id 未知，尝试跳转");
              autoGotoNpc(npcId, jumpId);
            }
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 3000);
          } else if (t.type === "guide") {
            /* ★ 2026-09-23：区分两类 guide：
               ① GuideDone(901)/FuncOpen(900)：需要"打开面板/上报事件"完成引导
               ② 1008 玩法跳转（如"进毁灭之塔"）：**只需跳转**，后续由对应面板
                  （TowerPanel/InstanceRangePanel）自动处理，不做 901 上报
                  （实测对 1008 做 901 上报永远失败，且会造成无效刷屏） */
            AUTO.curTarget = "引导: " + (t.name || ("#" + t.jumpId));
            state({ autoTaskTarget: AUTO.curTarget });
            var gj = t.jumpId;
            var guideDone = false;
            // ---- ② 1008 玩法跳转：只跳转 + 等待对应面板处理 ----
            if (t.eventType === 1008 && gj) {
              autoLog("1008玩法跳转→仅跳转 jumpId=" + gj + "，等待面板处理");
              autoJumpByConfig(gj);
              // 等待面板（TowerPanel 等）自动处理；给足时间后重读任务
              AUTO.__cooldownUntil = Date.now() + 12000;
              AUTO.state = "IDLE";
              break;
            }
            // 常规引导：先按任务跳转，再用 1.1.3.2 cvq 上报事件。
            var fcGd = F();
            if (gj) {
              autoLog("引导任务跳转 jumpId=" + gj);
              guideDone = autoJumpByConfig(gj);
            } else if (t.npcId) {
              autoLog("引导任务尝试跳转 NPC #" + t.npcId);
              guideDone = autoGotoNpc(t.npcId, 0);
            }
            try {
              var evParams = [];
              if (t.npcId) evParams = [{ kind: 1, intValue: t.npcId }];
              var guideEvent = t.eventType || 901;
              var reGd = reportClientEvent(guideEvent, evParams);
              if (reGd) {
                if (reGd.then) reGd.then(function(ok){ autoLog("上报引导事件 " + guideEvent + " " + (ok ? "成功" : "失败")); }, function(){});
                else autoLog("上报引导事件 " + guideEvent);
              } else {
                autoLog("task.cvq 不可用，无法上报引导事件");
              }
            } catch (e) { autoLog("上报GuideDone异常: " + (e && e.message)); }
            try {
              var guideG2 = fcGd && fcGd.guide;
              if (guideG2 && typeof guideG2.bSq === "function") guideG2.bSq();
            } catch (e) { autoLog("引导完成兜底异常: " + (e && e.message)); }
            if (!guideDone) autoLog("引导任务无可用打开方式，等待手动完成...");
            /* ★ 2026-09-23 修复死循环：原来回 IDLE + setTimeout 5s 是无效的！
               因为下一轮 tick 的 case "IDLE" 会立刻把 state 设回 READ_TASK，
               5000ms 的 setTimeout 根本没机会生效 → 每 300ms 重跑一遍（实测刷屏）。
               改为：设置"冷却截止时间"，tick 在 READ_TASK 开头统一检查。 */
            AUTO.__cooldownUntil = Date.now() + 5000;
            AUTO.state = "IDLE";
          } else if (t.type === "jobChange") {
            // 1.1.3.2：role.fBe(职业组) 取列表，role.fBm(confId) 执行。
            AUTO.curTarget = "转职";
            state({ autoTaskTarget: AUTO.curTarget });
            try { if (W.__REMOTE_COMMANDS__["wx.jobs"]) W.__REMOTE_COMMANDS__["wx.jobs"](); } catch (e) {}
            if (String(lsGet("__WX_AUTO_JOBSWITCH__", "1")) !== "1") {
              autoLog("转职: 自动转职开关关闭，跳过");
              AUTO.state = "IDLE";
              setTimeout(function(){ AUTO.state = "READ_TASK"; }, 5000);
            } else {
            var jobDone = false;
            try {
              var fcJ = F();
              var rj0 = getRoleCur();
              if (!rj0 || typeof rj0.mgr.fBm !== "function") {
                autoLog("转职: role.fBm 不可用（role 未就绪）");
              } else {
                var jobList = [];
                try { if (typeof rj0.mgr.fBe === "function") jobList = rj0.mgr.fBe(rj0.group) || []; } catch (e) {}
                // 只保留已解锁的
                var unlocked = [];
                try {
                  var tbl = fcJ.cfgMgr && fcJ.cfgMgr.tbl;
                  for (var ji2 = 0; ji2 < jobList.length; ji2++) {
                    var rd2 = tbl && tbl.RoleDesTable && tbl.RoleDesTable.findByKey ? tbl.RoleDesTable.findByKey(jobList[ji2]) : null;
                    if (!rd2 || typeof rd2.unlockLevel !== "number" || rj0.level >= rd2.unlockLevel) unlocked.push(jobList[ji2]);
                  }
                } catch (e) { unlocked = jobList; }
                autoLog("转职: 当前conf=" + rj0.confId + " Lv=" + rj0.level + " 可转职=" + JSON.stringify(jobList) + " 已解锁=" + JSON.stringify(unlocked));
                if (unlocked && unlocked.length) {
                  var targetJob = 0;
                  try {
                    var selArr = String(lsGet("__WX_JOB_SELECT__", "")).split(",").map(Number).filter(function (x) { return x > 0; });
                    for (var ji = 0; ji < selArr.length; ji++) {
                      if (unlocked.indexOf(selArr[ji]) >= 0) { targetJob = selArr[ji]; break; }
                    }
                  } catch (e) {}
                  if (!targetJob) targetJob = unlocked[0];
                  var jobName = getJobName(fcJ, targetJob);
                  autoLog("转职: 选择 " + jobName + " #" + targetJob + " → fBa 执行");
                  var rj = rj0.mgr.fBm(targetJob);
                  if (rj && rj.then) {
                    rj.then(function(res){
                      var ok = !!(res && (res.code === 0 || res.code === undefined));
                      autoLog("转职结果: " + (ok ? "成功！" : "失败 code=" + (res && res.code)));
                      if (ok) jobDone = true;
                    }, function(){ autoLog("转职异常回调"); });
                  } else { autoLog("转职调用完成(无Promise)"); jobDone = true; }
                } else {
                  autoLog("转职: 无可转职职业（等级不足或已满级）");
                }
              }
            } catch (e) { autoLog("转职异常: " + (e && e.message)); }
            }  // 自动转职开关 else 闭合
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 5000);
          } else if (t.type === "strengthen") {
            // 强化装备任务：自动强化穿戴装备直到达标
            AUTO.curTarget = "自动强化装备";
            state({ autoTaskTarget: AUTO.curTarget });
            AUTO.state = "AUTO_STRENGTH";
            AUTO.strengthStart = now;
            autoLog("开始自动强化装备...");
          } else if (t.type === "chapterReward") {
            // 章节奖励领取
            AUTO.curTarget = "领取章节奖励";
            state({ autoTaskTarget: AUTO.curTarget });
            if (autoChapterReward(t.chapterId)) {
              autoLog("章节奖励领取成功");
            } else {
              autoLog("章节奖励领取失败(尝试跳转)");
              autoGotoChapter(0);
            }
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 3000);
          } else if (t.type === "medicine") {
            // 配置药水（血药x2 + 蓝药x1）
            AUTO.curTarget = "配置药水";
            state({ autoTaskTarget: AUTO.curTarget });
            if (autoConfigMedicine()) {
              autoLog("药水配置完成");
            } else {
              autoLog("药水配置失败");
            }
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 3000);
          } else if (t.type === "shopBuy") {
            /* ★ v30 新增：商店购买物品（如 0元"试用月卡" 70900）
               官方链路（SilverMonthCardComp.gaR）：
                 n = shop.shopGoodsInfo.fDU(goodsId)   // 取商品信息
                 shop.reqShopGoodsBuy(n)               // 购买（0元直接成功）
               购买成功后服务端完成 801 任务 → 触发 104 特权激活 → 主线 300 完成。
               ★ 去重：同一商品只买一次（购买成功后任务会推进，避免重复购买）。 */
            AUTO.curTarget = "购买商品 #" + t.goodsId;
            state({ autoTaskTarget: AUTO.curTarget });
            var shopKey = "g" + t.goodsId;
            var triedShop = AUTO.__shopTried || (AUTO.__shopTried = {});
            if (triedShop[shopKey]) {
              autoLog("商品 #" + t.goodsId + " 已购买过 → 跳过该任务，继续下一个");
              AUTO.state = "IDLE";
              setTimeout(function(){ AUTO.state = "READ_TASK"; }, 2500);
              break;
            }
            triedShop[shopKey] = 1;
            autoShopBuy(t.goodsId);
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 4000);
          } else if (t.type === "clientEvent" && t.autoPass) {
            /* ★ v33g：autoPass 类事件（战力达标/个人信息/物品获得等）
               客户端无对应操作 → 直接上报推进，失败则跳过不卡死 */
            if (!AUTO.__apLogged) { AUTO.__apLogged = {}; }
            if (!AUTO.__apLogged[t.eventType]) {
              AUTO.__apLogged[t.eventType] = 1;
              autoLog("自动推进事件 eventType=" + t.eventType + "（" + (t.name || t.desc || "") + "）");
            }
            try { reportClientEvent(t.eventType, []); } catch (e) {}
            AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 2000;
          } else if (t.type === "clientEvent") {
            // 客户端事件上报（药水配置=108 等）
            /* ★ v30：derived 事件（300 完成任务 / 104 特权激活）是【服务端派生】的，
               客户端上报无效（服务端只认 801 商店购买 / 特权激活本身）。
               对这类事件：上报一次后【不停止自动任务】，改为跳过继续下一个任务。 */
            AUTO.curTarget = "客户端事件 #" + t.eventType;
            state({ autoTaskTarget: AUTO.curTarget });
            var evTriedKey = "__WX_EVT_TRIED_" + t.eventType + "_" + (cur.task ? cur.task.id : 0);
            var alreadyTried = String(lsGet(evTriedKey, "0")) === "1";
            if (alreadyTried) {
              if (t.derived) {
                autoLog("派生事件 #" + t.eventType + " 已尝试过 → 跳过该任务，继续下一个");
                AUTO.state = "IDLE";
                setTimeout(function(){ AUTO.state = "READ_TASK"; }, 2500);
                break;
              }
              autoLog("客户端事件 #" + t.eventType + " 已上报过且未成功，自动关闭自动任务");
              autoStop("");
              break;
            }
            lsSet(evTriedKey, "1");
            try {
              var fcE = F();
              var taskE = fcE && fcE.task;
              var evReported = false;
              var re = reportClientEvent(t.eventType, []);
              if (re) {
                if (re && re.then) {
                  evReported = true;
                  re.then(function(ok){
                    autoLog("上报客户端事件 #" + t.eventType + (ok ? " 完成" : " 失败"));
                    if (!ok) {
                      if (t.derived) { autoLog("派生事件上报无效 → 跳过该任务继续"); AUTO.state = "IDLE"; setTimeout(function(){ AUTO.state = "READ_TASK"; }, 2500); }
                      else { autoLog("客户端事件上报失败，自动关闭自动任务"); autoStop(""); }
                    }
                  }, function(){ if (t.derived) { AUTO.state = "IDLE"; setTimeout(function(){ AUTO.state = "READ_TASK"; }, 2500); } else { autoLog("上报异常，自动关闭"); autoStop("自动关闭"); } });
                } else { autoLog("上报客户端事件 #" + t.eventType + " (cvq)"); evReported = true; }
              } else {
                autoLog("上报接口不可用(cvq)");
              }
              if (!evReported) {
                if (t.derived) { autoLog("派生事件接口不可用 → 跳过该任务继续"); }
                else { autoLog("客户端事件上报接口不可用，自动关闭自动任务"); autoStop(""); }
              }
            } catch (e) {
              if (t.derived) { autoLog("派生事件上报异常 → 跳过继续: " + e.message); }
              else { autoLog("客户端事件上报异常: " + e.message + "，自动关闭自动任务"); autoStop(""); }
            }
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 3000);
          } else {
            /* ★★ v25b 重写：未知任务不再"一次失败就停自动任务"（那会导致整条链死掉）。
               新策略（多路尝试 + 有限次跳过）：
                 ① 若配置里带 eventType → 上报该事件
                 ② 再对"常见装备类事件"逐个试（1101/1102/1103/1308 + 1990700/04/06/08）
                 ③ 都失败 → 跳过该任务（记住跳过，避免死循环），继续往后跑
                 ④ 最多跳过 5 个任务，超过才停（防止无限打转） */
            var unkKey = "__WX_UNK_TRIED_" + (cur.task ? cur.task.id : 0);
            var unkTried = String(lsGet(unkKey, "0")) === "1";
            var skipCnt = parseInt(lsGet("__WX_UNK_SKIP_CNT__", "0"), 10) || 0;

            if (unkTried) {
              // 已试过 → 跳过它，继续任务链（不再关自动任务）
              if (skipCnt < 5) {
                lsSet("__WX_UNK_SKIP_CNT__", String(skipCnt + 1));
                autoLog("未知任务 #" + (cur.task && cur.task.id) + " 已尝试过 → 跳过（累计 " + (skipCnt + 1) + "/5）");
              } else {
                autoLog("未知任务已跳过 5 个 → 停止自动任务（请检查任务类型）");
                autoStop("未知任务过多");
                break;
              }
              lsSet(unkKey, "1");
              AUTO.state = "IDLE";
              AUTO.__cooldownUntil = Date.now() + 4000;
              break;
            }
            lsSet(unkKey, "1");
            autoLog("未知任务目标，尝试多路上报事件一次...");
            try {
              var tried = [];
              if (t.eventType) {
                var ru0 = reportClientEvent(t.eventType, []);
                tried.push(t.eventType + (ru0 ? "(ok)" : "(no)"));
              }
              // 常见装备/玩法类事件兜底
              var fallbackEv = [1101, 1102, 1103, 1308, 1990700, 1990704, 1990706, 1990708];
              for (var fi = 0; fi < fallbackEv.length; fi++) {
                try { reportClientEvent(fallbackEv[fi], []); } catch (e) {}
              }
              autoLog("未知任务已尝试上报: " + (tried.join(",") || "-") + " + 装备类兜底 " + fallbackEv.length + " 个");
            } catch (e) { autoLog("未知任务上报异常: " + (e && e.message)); }
            AUTO.state = "IDLE";
            AUTO.__cooldownUntil = Date.now() + 4000;
          }
          break;
        }
        case "GOTO": {
          /* ★ 2026-09-24 重写（"耐心模式"）：
             chL 跳转会触发游戏自带寻路（人物走过去 → 到入口 → 自动弹面板），这需要时间。
             原逻辑 8/15 秒就重试，反复跳转把寻路打断 → 永远到不了。
             新逻辑：
               - 首次进入 GOTO 时做一次"跳转"（或直接进副本）
               - 之后【不再重复跳转】，耐心等待（最长 60 秒）
               - 等待期间若进入战斗 → IN_BATTLE；若任务推进 → 回 READ_TASK
               - 超时 60 秒才放弃 */
          var gotoKey = AUTO.gotoInfo ? (AUTO.gotoInfo.type + "_" + (AUTO.gotoInfo.instId || AUTO.gotoInfo.npcId || 0) + "_" + (AUTO.gotoInfo.jumpId || 0)) : "none";
          if (AUTO.__gotoKey !== gotoKey) {
            AUTO.__gotoKey = gotoKey; AUTO.__gotoFirstAt = now; AUTO.__gotoDidJump = false;
          }
          // 首次：执行一次进入动作（跳转 / 直接进副本 / 去 NPC）
          if (!AUTO.__gotoDidJump && AUTO.gotoInfo) {
            AUTO.__gotoDidJump = true;
            if (AUTO.gotoInfo.type === "instance") {
              var okEnter = false;
              try { okEnter = battleEnter(AUTO.gotoInfo.instId, 0); } catch (e) {}
              if (!okEnter && AUTO.gotoInfo.jumpId) {
                autoJumpByConfig(AUTO.gotoInfo.jumpId);
              }
            } else if (AUTO.gotoInfo.type === "npc") {
              autoGotoNpc(AUTO.gotoInfo.npcId, AUTO.gotoInfo.jumpId);
            }
          }
          // 进入战斗 → IN_BATTLE
          if (inBattle) {
            AUTO.state = "IN_BATTLE";
            AUTO.battleStart = now;
            autoLog("进入副本，自动战斗中...");
            break;
          }
          // 任务已推进 → 回 READ_TASK
          try {
            var curG = autoGetCurrentTask();
            if (curG && curG.task.id !== AUTO.curTaskId) {
              autoLog("任务已推进，回读任务");
              AUTO.state = "IDLE";
              AUTO.__cooldownUntil = Date.now() + 1000;
              break;
            }
          } catch (e) {}
          // 总超时 60 秒 → 放弃（不再中途打断寻路）
          if (now - (AUTO.__gotoFirstAt || now) > 60000) {
            autoLog("进入/寻路超时(60s)，可能未解锁或无法到达，停止自动任务");
            AUTO.gotoInfo = null;
            autoStop("寻路超时");
            break;
          }
          break;
        }
        case "IN_BATTLE": {
          /* ★ 2026-09-23 修复（"打完直接退出+网络异常"根因）：
             battleStart 只在 GOTO / READ_TASK 的 instance 分支赋值，
             而"塔出战"等路径进入 IN_BATTLE 时没赋值 → 沿用旧时间戳
             → now-battleStart 立刻 >180s → 误判超时 → autoExitBattle 强退
             → 战斗未结束强发退出协议 → 网络异常。
             兜底：进入 IN_BATTLE 时若 battleStart 无效（0 或远超合理范围）则重置为 now。 */
          if (!AUTO.battleStart || (now - AUTO.battleStart) < 0) {
            AUTO.battleStart = now;
            autoLog("IN_BATTLE: battleStart 异常，已重置");
          }
          /* ★ 2026-09-23 新增：识别"毁灭之塔"战斗。
             塔战斗特点：打完只是清空怪物，**不弹奖励弹窗**（奖励在 TowerPanel 内手动领），
             战斗结束后会直接回到 TowerPanel。所以塔战斗不能用副本的 SETTLE 流程处理，
             应判定结束 = autoInBattle 变 false → 直接回 IDLE 重读任务。 */
          var inTowerBattle = false;
          try {
            var bmTb = F() && F().battleMgr;
            var myTb = bmTb && bmTb.bEu && bmTb.bEu.my;
            var tpTb = myTb && myTb.bCW && myTb.bCW.type;
            // bCW.type 是 InstanceTypeTableEnum（TOWER=1200 / MYSTIC_REALM=1400 / ABYSS_DEMON=1600）
            if (tpTb === 1200 || tpTb === 1400 || tpTb === 1600) inTowerBattle = true;
            AUTO.__inTowerBattle = inTowerBattle;
          } catch (e) {}
          // 塔/秘境/深渊：控制器 no() 变 false 后直接重读任务，不走普通副本结算。
          if (inTowerBattle && !autoInBattle()) {
            AUTO.state = "IDLE";
            AUTO.__cooldownUntil = Date.now() + 2500;
            autoLog("塔/秘境战斗结束(清怪完成)，返回重读任务");
            break;
          }
          // 检测结算（通关）
          if (autoSettled()) {
            AUTO.state = "SETTLE";
            AUTO.settleStart = now;
            autoLog("副本通关！处理结算/翻牌...");
          }
          /* 战斗超时 180 秒：超时前先检查任务是否已推进（推进了=其实打完了，等结算即可）。
             ★ 额外安全：仅当"确实还在战斗中"且"任务未推进"时才退出；
               且退出前二次确认 autoInBattle()，避免在结算临界态强退造成网络异常。 */
          if (now - AUTO.battleStart > 180000) {
            var curB = autoGetCurrentTask();
            if (curB && curB.task.id !== AUTO.curTaskId) {
              AUTO.state = "SETTLE";
              AUTO.settleStart = now;
              autoLog("任务已推进，等待结算...");
            } else if (!autoInBattle()) {
              // 其实已不在战斗 → 直接回城，不发退出协议（避免网络异常）
              AUTO.state = "IDLE";
              AUTO.__cooldownUntil = Date.now() + 2000;
              autoLog("副本已不在战斗中，跳过退出，直接回城");
            } else {
              autoLog("副本超时(180s)，退出重试...");
              autoExitBattle();
              AUTO.state = "IDLE";
              AUTO.__cooldownUntil = Date.now() + 3000;
            }
          }
          break;
        }
        case "SETTLE": {
/* 结算后自动处理（出售/分解/穿戴/技能）
             ⚠️ 这些都是【联网协议】，必须等【已回城】且错峰执行，
                否则会撞车产生网络异常：
                  0.8s 出售/分解 → 2.0s 穿戴 → 3.2s 技能 */
          if (!autoInBattle()) {
            if (!AUTO.__postDone) {
              AUTO.__postDone = 1;
              try {
                setTimeout(function () {
                  try { if (CFG.autoSell() || CFG.autoDecompose()) autoSellDecomposeNow(); } catch (e) {}
                }, 800);
                if (CFG.autoWear()) setTimeout(function () { try { autoWearBestEquip(0); } catch (e) {} }, 2000);
                if (CFG.autoSkill()) setTimeout(function () { try { autoUpgradeSkills(); } catch (e) {} }, 3200);
              } catch (e) {}
            }
            AUTO.state = "IDLE";
            AUTO.__cooldownUntil = Date.now() + 2000;
            setTimeout(function () { AUTO.__postDone = 0; }, 6000);   // 本场结束 → 6s 后复位，下一场再处理
            autoLog("已离开副本，回城（延后执行清理/穿戴/技能）");
            break;
          }
          /* 选关后加载新场景时：
             点了下一关之后会短暂无结算、也尚未进入战斗：
               · autoSettled() 已为 false（结算面板已关）
               · autoInBattle() 还没变 true（场景还没加载完）
             旧逻辑直接撞上 25s 超时 → 强退 → 玩家看到"打着打着突然回城"。

             新逻辑：
               ① 若在"点了下一关后"的加载窗口（20s）内 → 重置计时，安心等待
               ② 超窗口仍未进入战斗 → 才判定真卡住，走原超时逻辑 */
          var sinceNext = now - (AUTO.__nextStageAt || 0);
          var loadingNext = (AUTO.__nextStageAt && sinceNext < 20000);
          if (loadingNext) {
            /* 场景加载中：不计入超时，并把计时起点往后推，避免出窗口瞬间立刻超时 */
            AUTO.settleStart = now - Math.min(sinceNext, 5000);
            if (!AUTO.__loadingLogged) {
              AUTO.__loadingLogged = 1;
              autoLog("选关后场景加载中，等待进入下一关…");
            }
          } else if (now - AUTO.settleStart > 25000) {
            if (autoInBattle()) {
              autoExitBattle();
              autoLog("结算超时，退出副本...");
            } else {
              autoLog("结算超时但已不在战斗，跳过退出");
            }
            AUTO.state = "IDLE";
            AUTO.__cooldownUntil = Date.now() + 4000;
          }
          /* 状态推进后清掉加载标记，避免影响下一场 */
          if (!loadingNext && AUTO.__nextStageAt) {
            AUTO.__nextStageAt = 0;
            AUTO.__loadingLogged = 0;
          }
          break;
        }
        /* ============ v33g 新增：宠物蛋孵化（PetEggHatch=1503）============
           官方链路：蛋(BOX物品,PET_BOX) → $eZ.openBox.cpI({itemId,amount})
           流程：找到可用蛋 → 孵化 → 等 1.5s 重读任务；不可用 → 上报推进/跳过 */
        case "PET_EGG": {
          try {
            var curEG = autoGetCurrentTask();
            if (!curEG) { AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1500; break; }
            if (curEG.task.id !== AUTO.curTaskId) {
              autoLog("孵化任务已推进，回读任务");
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1200;
              break;
            }
            var cfgEG = curEG.cfg || {};
            var proEG = (typeof curEG.task.process === "number") ? curEG.task.process : 0;
            var stEG = curEG.task ? curEG.task.status : -1;
            if (stEG === 2) {
              autoLog("孵化任务可领取(status=2)");
              autoSubmitTask(curEG);
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 2500;
              break;
            }
            if (proEG > 0 && proEG !== AUTO.__eggLastPro) {
              AUTO.__eggLastPro = proEG;
              autoLog("孵化进度: " + proEG);
            }
            var lastEG = AUTO.eggTickLast || 0;
            if (now - lastEG >= 1600) {
              AUTO.eggTickLast = now;
              var didEG = autoHatchEggOnce();
              if (!didEG && !AUTO.__eggWaitLogged) {
                AUTO.__eggWaitLogged = 1;
                autoLog("孵化: 无可用蛋/材料不足，等待中");
              }
            }
            if (!AUTO.eggStart) AUTO.eggStart = now;
            if (now - AUTO.eggStart > 60000) {
              autoLog("孵化 60s 无进展，上报事件 " + (cfgEG.eventType || 1503));
              try { reportClientEvent(cfgEG.eventType || 1503, []); } catch (e) {}
              AUTO.eggStart = 0;
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 3000;
            }
          } catch (e) { autoLog("孵化异常: " + (e && e.message)); AUTO.state = "IDLE"; }
          break;
        }
        /* ============ v33g 新增：宠物养成（1500/1501/1502/1504/1505/1506）============
           优先调 PetComp 一键接口；失败则上报事件推进（宠物系统未开时也能过任务） */
        case "PET_OP": {
          try {
            var curPO = autoGetCurrentTask();
            if (!curPO) { AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1500; break; }
            if (curPO.task.id !== AUTO.curTaskId) {
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1200; break;
            }
            var cfgPO = curPO.cfg || {};
            var stPO = curPO.task ? curPO.task.status : -1;
            if (stPO === 2) {
              autoLog("宠物任务可领取(status=2)");
              autoSubmitTask(curPO);
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 2500;
              break;
            }
            var lastPO = AUTO.petOpLast || 0;
            if (now - lastPO >= 2000) {
              AUTO.petOpLast = now;
              autoPetOpOnce();
            }
            if (!AUTO.petOpStart) AUTO.petOpStart = now;
            /* ★v33N：材料不足时快速推进，不再干等 */
            if (now - AUTO.petOpStart > 30000) {
              autoLog("宠物任务 30s 无进展，上报事件 " + (cfgPO.eventType || 1500) + " 并重读");
              try { reportClientEvent(cfgPO.eventType || 1500, []); } catch (e) {}
              AUTO.petOpStart = 0;
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 3000;
            }
          } catch (e) { AUTO.state = "IDLE"; }
          break;
        }
        /* ============ v33g 新增：装备杂项 / 技能升级 ============
           刻印石镶嵌、宝石合成、穿戴等级和、技能升级等：
           先试官方一键接口，取不到组合 → 上报事件推进 */
        case "EQUIP_MISC": {
          try {
            var curEM = autoGetCurrentTask();
            if (!curEM) { AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1500; break; }
            if (curEM.task.id !== AUTO.curTaskId) {
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1200; break;
            }
            var cfgEM = curEM.cfg || {};
            var stEM = curEM.task ? curEM.task.status : -1;
            if (stEM === 2) {
              autoLog("杂项任务可领取(status=2)");
              autoSubmitTask(curEM);
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 2500;
              break;
            }
            var lastEM = AUTO.miscLast || 0;
            if (now - lastEM >= 2500) {
              AUTO.miscLast = now;
              autoEquipMiscOnce(cfgEM.eventType || 0);
            }
            if (!AUTO.miscStart) AUTO.miscStart = now;
            if (now - AUTO.miscStart > 45000) {
              autoLog("杂项任务 45s 无进展，上报事件 " + (cfgEM.eventType || "?"));
              try { reportClientEvent(cfgEM.eventType || 1610, []); } catch (e) {}
              AUTO.miscStart = 0;
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 3000;
            }
          } catch (e) { AUTO.state = "IDLE"; }
          break;
        }
        case "ENCHANT": {
          /* 装备附魔（任务事件 SOUL_ENCHANT=1990706）
             1.1.3.2 官方一键链路：equip.fsE(equip.fsy())。
             完成判定：任务推进（task.id 变化）或 process 达标 → 回 READ_TASK。 */
          try {
            var curE = autoGetCurrentTask();
            if (!curE) { AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1500; break; }
            if (curE.task.id !== AUTO.curTaskId) {
              autoLog("附魔任务已推进，回读任务");
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1200;
              break;
            }
            var cfgE = curE.cfg || {};
            var proE = (typeof curE.task.process === "number") ? curE.task.process : 0;
            var numE = (typeof cfgE.num === "number") ? cfgE.num : 1;
            var stE = curE.task ? curE.task.status : -1;
            // v25f：与官方一致 —— 只要没领过(3)就可以尝试领取
            if (stE !== 3 && (stE === 2 || (numE > 0 && proE >= numE))) {
              autoLog("附魔任务可领取(status=" + stE + " pro=" + proE + "/" + numE + ")");
              autoSubmitTask(curE);
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 3000;
              break;
            }
            /* v25c 重要修正：装备类任务的 cfg.num 不是"次数"！
               实测 #33100110 配置 = [33100110,26005,26563,3,[3302310],...,1606,...,1990706,null]
               同组其它任务 num=1000/300（均为事件子类型语义），
               真正进度看 task.process 与"任务是否推进"。故这里不再用 num 判定达标。 */
            if (proE > 0 && proE !== AUTO.__enchantLastPro) {
              AUTO.__enchantLastPro = proE;
              autoLog("附魔进度: " + proE + " (eventType=" + (cfgE.eventType || "?") + " num=" + (cfgE.num || "?") + " 非次数仅供参考)");
            }
            var lastE = AUTO.enchantLast || 0;
            if (now - lastE >= 2500) {
              AUTO.enchantLast = now;
              var didE = autoEnchantOnce();
              if (!didE && !AUTO.__enchantWaitLogged) {
                AUTO.__enchantWaitLogged = 1;
                autoLog("附魔: 暂无可附魔的组合（背包无魂魄 / 已全部镶嵌），等待中");
              }
            }
            if (!AUTO.enchantStart) AUTO.enchantStart = now;
            if (now - AUTO.enchantStart > 90000) {
              autoLog("附魔 90s 无进展，上报事件 " + (cfgE.eventType || 1990706));
              try { reportClientEvent(cfgE.eventType || 1990706, []); } catch (e) {}
              AUTO.enchantStart = 0;
              var ek = "__WX_UNK_TRIED_" + curE.task.id;
              var ecnt = parseInt(lsGet("__WX_UNK_SKIP_CNT__", "0"), 10) || 0;
              if (String(lsGet(ek, "0")) === "1") {
                if (ecnt < 8) {
                  lsSet("__WX_UNK_SKIP_CNT__", String(ecnt + 1));
                  autoLog("附魔任务 #" + curE.task.id + " 仍无进展，跳过 (" + (ecnt + 1) + "/8)");
                } else {
                  autoLog("附魔连续跳过 8 次，停止自动任务（请手动确认附魔条件）");
                  autoStop("附魔无法推进");
                }
              } else { lsSet(ek, "1"); }
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 4000;
            }
          } catch (e) { autoLog("附魔异常: " + (e && e.message)); AUTO.state = "IDLE"; }
          break;
        }
        case "GEM_INLAY": {
          /* ★ v25：宝石镶嵌/刻印（1990704 / 1990708）—— 尝试官方批量接口 */
          try {
            var curG = autoGetCurrentTask();
            if (!curG) { AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1500; break; }
            if (curG.task.id !== AUTO.curTaskId) {
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 1200; break;
            }
            var lastG = AUTO.gemLast || 0;
            if (now - lastG >= 2500) {
              AUTO.gemLast = now;
              autoGemInlayOnce();
            }
            if (!AUTO.gemStart) AUTO.gemStart = now;
            if (now - AUTO.gemStart > 90000) {
              autoLog("镶嵌超时，上报事件并重读任务");
              try { reportClientEvent((curG.cfg && curG.cfg.eventType) || 1990704, []); } catch (e) {}
              AUTO.gemStart = 0;
              AUTO.state = "IDLE"; AUTO.__cooldownUntil = Date.now() + 2000;
            }
          } catch (e) { AUTO.state = "IDLE"; }
          break;
        }
        case "AUTO_STRENGTH": {
          // 自动强化装备直到任务完成
          try {
            // 节流：每 2 秒强化一次（避免过快触发服务端限制）
            var lastS = AUTO.strengthLast || 0;
            if (now - lastS >= 2000) {
              AUTO.strengthLast = now;
              if (autoStrengthDone()) {
                autoLog("强化任务完成！提交奖励...");
                // 提交任务领取奖励（status=2 可提交）
                var curS = autoGetCurrentTask();
                if (curS && curS.task.status === 2) {
                  autoSubmitTask(curS);
                  autoLog("已提交强化任务奖励");
                }
                AUTO.state = "IDLE";
                setTimeout(function(){ AUTO.state = "READ_TASK"; }, 3000);
              } else {
                autoStrengthenOnce();
              }
            }
          } catch (e) {
            autoLog("自动强化异常: " + e.message);
            AUTO.state = "IDLE";
            setTimeout(function(){ AUTO.state = "READ_TASK"; }, 3000);
          }
          break;
        }
      }
      /* 同步 UI 状态（★ 2026-09-23 修复：统一状态格式 + 去掉 snap 掐断）
         - 原来推送 autoStatus: AUTO.state（如 "IDLE"），与 autoLog()/autoSyncTaskInfo()
           推送的 "运行中·IDLE" 格式不一致，两处互相打架，UI 会卡在最初的值；
         - 原来还有 __WX_LAST_TICK_SNAP__ 去重，snap 一旦写入就不会再推，导致
           格式变化时也无法纠正。
         现在统一为 "运行中·<state>" / "已停止"，并交给 state() 内部节流合并去重。 */
      try {
        state({
          __WX_AUTO_UI__: !!AUTO.enabled,   // 同步 UI 开关显示（与 __WX_AUTO_TASK__ 逻辑值同源）
          autoStatus: AUTO.enabled ? ("运行中·" + AUTO.state) : "已停止",
          autoLog: AUTO.log || "等待自动任务启动"
        });
      } catch (e) {}
    } catch (e) {
      autoLog("自动任务异常: " + e.message);
    }
  }

  /* ===== 自动任务开关命令 =====
     ★ 2026-09-23 二次修复（"按开关无效 / 立刻自己关"根因）：
       原实现 var on = !!(payload && payload.value) —— 当 ui-config 的 switch 没有 id 时，
       Java 端回传的 payload 里没有 value（只有 {}），导致 on 恒为 false：
       用户每次点击都被判定成"关"，于是"启动→1秒后停止"。
     修复：
       1) 容错取值：value / checked / isChecked 任一皆可；
       2) 若 payload 完全没有开关值（既不是 true/false），则视为"切换"（对当前状态取反），
          这样即使 UI 不回传值，点击也一定能切换；
       3) 开关状态写 __WX_AUTO_TASK__（逻辑权威）；
          同时推 __WX_AUTO_UI__ 给 UI switch 显示（该 key 与命令 id 不同名，不会形成回传死循环）。 */
  W.__REMOTE_COMMANDS__["wx.auto"] = function (payload) {
    try {
      var p = payload || {};
      var hasVal = (p.value === true || p.value === false || p.value === "1" || p.value === "0" ||
                    typeof p.checked === "boolean" || typeof p.isChecked === "boolean");
      var on;
      if (hasVal) {
        if (p.value === true || p.value === "1") on = true;
        else if (p.value === false || p.value === "0") on = false;
        else if (typeof p.checked === "boolean") on = p.checked;
        else on = !!p.isChecked;
      } else {
        // UI 没传开关值 → 当作"切换"（对当前状态取反）
        on = !AUTO.enabled;
        log("[AUTO] wx.auto 未收到开关值，按切换处理 → " + on);
      }
      if (on && W.__WX_CARD_PERMANENT__ !== true) {
        AUTO.enabled = false;
        AUTO.state = "IDLE";
        lsSet("__WX_AUTO_TASK__", "0");
        W.__WX_AUTO_ENABLED__ = false;
        return { lastAction: "permanent-required" };
      }
      AUTO.enabled = on;
      AUTO.state = "IDLE";
      lsSet("__WX_AUTO_TASK__", on ? "1" : "0");
      if (on) {
        autoLog("自动主线已启动");
        state({ __WX_AUTO_UI__: true, autoStatus: "运行中·IDLE", autoLog: "自动主线已启动" });
      } else {
        autoLog("自动主线已停止");
        state({ __WX_AUTO_UI__: false, autoStatus: "已停止", autoLog: "自动主线已停止" });
      }
      // ★ 2026-09-23：开关切换是低频人工操作，延后 350ms 用 updateState（rerender）
      //   强刷一次，确保状态行/日志一定立刻可见（之后靠 tick 的 mergeState 维持，不再重建）。
      try {
        setTimeout(function () {
          try {
            W.RemoteUI.updateState({
              __WX_AUTO_UI__: !!AUTO.enabled,
              autoStatus: AUTO.enabled ? ("运行中·" + AUTO.state) : "已停止",
              autoLog: AUTO.log || (AUTO.enabled ? "自动主线已启动" : "自动主线已停止")
            });
          } catch (e) {}
        }, 350);
      } catch (e) {}
      return { lastAction: on ? "auto-on" : "auto-off" };
    } catch (e) {
      log("[AUTO] wx.auto 异常: " + (e && e.message));
      return { lastAction: "auto-err", err: "" + (e && e.message) };
    }
  };

  /* ===== 任务信息持续同步（不开自动也显示当前主线任务）===== */
  var AUTO_TASK_LAST_SYNC = 0;
  function autoSyncTaskInfo() {
    try {
      // ★ 角色就绪判断：选角色/登录界面不拉任务（避免触发"角色未初始化"错误）
      var fcChk = F();
      var roleMgr = fcChk && fcChk.role;
      var roleReady = !!(roleMgr && roleMgr.fBb && roleMgr.fBb.fAQ);
      if (!roleReady) return;  // 角色数据未加载（选角色界面）→ 不拉任务
      // 每 5 秒同步一次，减少对游戏 UI 的干扰
      var now = Date.now();
      if (now - AUTO_TASK_LAST_SYNC < 5000) return;
      AUTO_TASK_LAST_SYNC = now;
      // ★ 排除实验：不再每 5 秒同步职业列表到 UI（转职推送疑似破坏 UI 刷新）
      // （需要看职业时点"刷新职业列表"按钮手动触发）
      var cur = autoGetCurrentTask();
      if (!cur) {
        // 任务数据未加载 → 触发普通拉取，不覆盖 UI（等本地缓存更新后下轮再读）
        // 角色未就绪时按 30 秒冷却拉取，避免网络请求过密。
        var nowP = Date.now();
        if ((AUTO.__syncCtPLast || 0) + 30000 < nowP) {
          AUTO.__syncCtPLast = nowP;
          try {
            var fcRf = F();
            var taskRf = fcRf && fcRf.task;
            refreshTaskGroup(false);
          } catch (e) {}
        }
        // 诊断：写文件确认读不到任务的原因
        try {
          var wd = jsb && jsb.fileUtils;
          if (wd && typeof wd.writeStringToFile === "function") {
            var pd = (wd.getWritablePath ? wd.getWritablePath() : "/data/user/0/com.fightingstars.games.warrior/files/") + "wx_sync_diag.txt";
            var fcD = F();
            var taskD = fcD && fcD.task;
            var infoD = "roleReady=" + roleReady + " task=" + (taskD ? "ok" : "null") + " cvg=" + (taskD && taskD.cvg ? "ok" : "null") + " ts=" + Date.now() + "\n";
            try {
              if (taskD && taskD.cvg) infoD += "cvg keys=" + Object.keys(taskD.cvg).join(",") + "\n";
            } catch (e) {}
            wd.writeStringToFile(infoD, pd);
          }
        } catch (e) {}
        // 不覆盖 UI（保留当前显示，避免误显示"已完成"）
        return;
      }
      var cfg = cur.cfg;
      var t = autoAnalyzeTask(cur);
      var desc = t.desc || (cfg && cfg.desc ? autoGetText(cfg.desc) : ("任务 #" + cur.task.id));
      var num = (cfg && typeof cfg.num === "number") ? cfg.num : "?";
      var pro = (cur.task.process || 0) + "/" + num;
      var target = "-";
      if (t.type === "submit") target = "提交任务";
      else if (t.type === "instance") target = "副本 #" + t.instId;
      else if (t.type === "npc") target = "NPC 对话";
      else if (t.type === "guide") target = "引导完成";
      else if (t.type === "jobChange") target = "转职";
      else if (t.type === "strengthen") target = "自动强化装备";
      else if (t.type === "chapterReward") target = "领取章节奖励";
      else target = "未知目标";
      var patch = { autoTaskDesc: desc, autoTaskTarget: target, autoTaskProgress: pro };
      // 目标显示优化：副本显示中文名
      try {
        if (t.type === "instance" && t.instId) {
          var fc2 = F();
          var instCfg = fc2 && fc2.cfgMgr && fc2.cfgMgr.tbl && fc2.cfgMgr.tbl.InstanceTable && fc2.cfgMgr.tbl.InstanceTable.findByKey ? fc2.cfgMgr.tbl.InstanceTable.findByKey(t.instId) : null;
          if (instCfg) {
            var instName = instCfg.name ? autoGetText(instCfg.name) : ("#" + t.instId);
            patch.autoTaskTarget = instName;
          } else {
            patch.autoTaskTarget = "副本 #" + t.instId;
          }
        }
      } catch (e) {}
      // 只有 AUTO 未启用时更新状态行（启用时由状态机管理）
      // ★ 2026-09-22 修复：不再显示误导性的"待机"，始终推送真实状态 + autoLog
      patch.autoStatus = AUTO.enabled ? ("运行中·" + AUTO.state) : "已停止";
      patch.autoLog = (AUTO.enabled && AUTO.log) ? AUTO.log : (AUTO.log || "等待自动任务启动");
      state(patch);
      // 诊断：记录本次同步的权威值（每 30 秒一次，便于排查"UI 不刷新"）
      try {
        if ((AUTO.__syncDiagT || 0) + 30000 < now) {
          AUTO.__syncDiagT = now;
          log("[AUTOSYNC] enabled=" + AUTO.enabled + " state=" + AUTO.state + " log=" + (AUTO.log || "") + " ls=" + lsGet("__WX_AUTO_TASK__", "?"));
        }
      } catch (e) {}
      // 每次同步都 dump 任务配置（便于调试副本id提取）
      try {
        if (cfg) {
          var w2 = jsb && jsb.fileUtils;
          if (w2 && typeof w2.writeStringToFile === "function") {
            var p2 = (w2.getWritablePath ? w2.getWritablePath() : "/data/user/0/com.fightingstars.games.warrior/files/") + "wx_task_cfg.txt";
            var cc = {
              id: cfg.id, name: cfg.name, desc: cfg.desc, taskType: cfg.taskType,
              num: cfg.num, eventType: cfg.eventType,
              param1: cfg.param1, param2: cfg.param2, param3: cfg.param3,
              param4: cfg.param4, param5: cfg.param5, jumpId: cfg.jumpId,
              levelStory: cfg.levelStory, nextId: cfg.nextId
            };
            var all2 = "taskId=" + cur.task.id + " status=" + cur.task.status + " process=" + cur.task.process + "\ncfg=" + JSON.stringify(cc) + "\n";
            w2.writeStringToFile(all2, p2);
          }
        }
      } catch (e) {}
    } catch (e) {}
  }

  /* 1.1.3.2 面板实例解析。 */
  function getPanelCandidates(fgm, prefix) {
    var out = [];
    try {
      if (!fgm) return out;
      var push = function (o) { if (o && typeof o === "object" && out.indexOf(o) < 0) out.push(o); };
      if (prefix) {
        var cfg0 = getPanelCfg(prefix);
        if (cfg0 && typeof fgm.cTO === "function") { try { push(fgm.cTO(cfg0)); } catch (e) {} }
      }
      var top = (fgm.cTU) ? fgm.cTU() : null;
      if (top) {
        var tk = String((top.vJ && top.vJ.cUV) || "");
        if (!prefix || (tk && tk.indexOf(prefix) >= 0)) {
          push(top);
        }
      }
      var topName = top ? String((top.vJ && top.vJ.cUV) || "") : "";
      out.__tk = prefix || topName;
      out.__topName = topName;
    } catch (e) {}
    return out;
  }

  /* ★ v29b：翻牌结果事件名。
     官方：$eZ.battleMgr.ks(u.$lH.b, type, index, data)，其中 $lH.b = "SELECT_CARD_RESULT"。
     旧脚本传了数字 1 → 事件名错误 → 翻牌面板收不到结果 → 不播动画/不更新牌面（看着"没效果"）。 */
  var _selCardEvt = null;
  function getSelectCardEvent() {
    try {
      if (_selCardEvt) return _selCardEvt;
      var qf = get("$QF") || (W.__require && W.__require("$QF"));
      var lh = qf && (qf.$lH || (qf.default && qf.default.$lH));
      if (lh && typeof lh.b === "string") { _selCardEvt = lh.b; return _selCardEvt; }
    } catch (e) {}
    return "SELECT_CARD_RESULT";
  }
  /* 兼容旧调用：返回第一个候选（优先含指定方法的） */
  function pickPanel(cands, fname) {
    try {
      if (!cands || !cands.length) return null;
      if (fname) { for (var i = 0; i < cands.length; i++) if (typeof cands[i][fname] === "function") return cands[i]; }
      return cands[0];
    } catch (e) { return null; }
  }
  function getPanelByPrefix(fgm, prefix) {
    return pickPanel(getPanelCandidates(fgm, prefix), null);
  }
  function getFinishPanel(fgmF) {
    try {
      var candsF = getPanelCandidates(fgmF, "AdventureFinishPanel");
      if (!candsF.length) return null;
      return { cands: candsF, panel: candsF[0], tk: candsF.__tk || "AdventureFinishPanel" };
    } catch (e) { return null; }
  }
  var _JX = null, _PANEL_CFG = null;
  function getJX() {
    try {
      if (_JX) return _JX;
      var r = (W.__require && W.__require("$JX")) || null;
      if (r && r.$fb) _JX = r;
    } catch (e) {}
    return _JX;
  }
  /* $RJ.default 本身就是 1.1.3.2 游戏面板配置表。 */
  function getGamePanelCfg() {
    try {
      if (_PANEL_CFG) return _PANEL_CFG;
      var mod = W.__require && W.__require("$RJ");
      _PANEL_CFG = mod && (mod.default || mod);
      return _PANEL_CFG;
    } catch (e) {}
    return null;
  }
  function getPanelCfg(name) {
    try {
      var jx = getJX();
      var p = jx && jx.$fb && jx.$fb.cvConfig && jx.$fb.cvConfig.panels;
      if (p && p[name]) return p[name];
      var gamePanels = getGamePanelCfg();
      if (gamePanels && gamePanels[name]) return gamePanels[name];
      var fc = F();
      if (fc && fc.cvConfig && fc.cvConfig.panels && fc.cvConfig.panels[name]) return fc.cvConfig.panels[name];
    } catch (e) {}
    return null;
  }
  /* 1.1.3.2 面板打开判断：cTK(SceneConfig)。 */
  function isPanelOpen(fgm, name) {
    try {
      if (!fgm || !name) return false;
      var cfg = getPanelCfg(name);
      if (cfg && typeof fgm.cTK === "function") return !!fgm.cTK(cfg);
      // 退化为"顶层组件名字相等"
      var top = (fgm.cTU) ? fgm.cTU() : null;
      var tk = top ? String((top.vJ && top.vJ.cUV) || "") : "";
      return tk === name;
    } catch (e) { return false; }
  }
  /* 便捷：当前 fguiMgr 下某面板是否打开 */
  function panelOpen(name) {
    try { return isPanelOpen(F() && F().fguiMgr, name); } catch (e) { return false; }
  }
  /* 1.1.3.2 关闭面板：cTE(SceneConfig)。 */
  function closePanel(fgm, name) {
    try {
      var cfg = getPanelCfg(name);
      if (cfg && fgm && typeof fgm.cTE === "function") { fgm.cTE(cfg); return true; }
    } catch (e) {}
    return false;
  }
  var POSTCLEAN = W.__WX_POSTCLEAN__ || (W.__WX_POSTCLEAN__ = { inBattle: false, lastRun: 0 });
  function autoPostCleanupTick() {
    try {
      if (AUTO.enabled) return;                       // 主线在跑 → 交给状态机
      if (!(CFG.autoSell() || CFG.autoDecompose() || CFG.autoWear() || CFG.autoSkill())) return;
      var inB = autoInBattle();
      if (inB) { POSTCLEAN.inBattle = true; return; }  // 记下"进过战斗"
      if (!POSTCLEAN.inBattle) return;                 // 从没进过战斗 → 不触发
      POSTCLEAN.inBattle = false;                      // 战斗 → 城镇，触发
      var now = Date.now();
      if (now - POSTCLEAN.lastRun < 5000) return;      // 5s 内不重复
      POSTCLEAN.lastRun = now;
      setTimeout(function () { try { if (CFG.autoSell() || CFG.autoDecompose()) autoSellDecomposeNow(); } catch (e) {} }, 900);
      if (CFG.autoWear()) setTimeout(function () { try { autoWearBestEquip(0); } catch (e) {} }, 2100);
      if (CFG.autoSkill()) setTimeout(function () { try { autoUpgradeSkills(); } catch (e) {} }, 3300);
      autoLog("回城兜底清理（独立于自动主线开关）");
    } catch (e) {}
  }
      function syncEmbeddedAutoConfig() {
        try {
          var fc = F(), lm = fc && fc.localMgr;
          if (!lm || (!lm.cms && typeof lm.cnp !== 'function')) return;
          if (!lm.cms) lm.cms = {};
          var flag = Number(lm.cms.autoBattleFlag) || 0;
          flag = CFG.autoRepeat() ? (flag | (1 << 1)) : (flag & ~(1 << 1));
          flag = CFG.autoRevive() ? (flag | (1 << 2)) : (flag & ~(1 << 2));
          lm.cms.autoBattleFlag = flag & ~(1 << 4);
          if (typeof lm.cnr === 'function') {
            var sq = CFG.autoSell() ? String(CFG.sellQualities()).split(',').map(Number).filter(function(x){return x>=1&&x<=6;}) : [];
            var dq = CFG.autoDecompose() ? String(CFG.decompQualities()).split(',').map(Number).filter(function(x){return x>=1&&x<=6;}) : [];
            var stamp = sq.join(',') + '|' + dq.join(',');
            if (syncEmbeddedAutoConfig.last !== stamp) { syncEmbeddedAutoConfig.last = stamp; lm.cnr(sq, dq); }
          }
        } catch (e) { log('配置同步失败: ' + (e && e.message)); }
      }
      function autoRepeatFinishTick() {
        try {
          if (!CFG.autoRepeat()) {
            AUTO.__repeatFinishScheduled = null;
            AUTO.__repeatFinishHandled = null;
            AUTO.__repeatFinishToken = (AUTO.__repeatFinishToken || 0) + 1;
            return;
          }
          if (!panelOpen("AdventureFinishPanel")) {
            if (AUTO.__repeatFinishScheduled || AUTO.__repeatFinishHandled) {
              AUTO.__repeatFinishScheduled = null;
              AUTO.__repeatFinishHandled = null;
              AUTO.__repeatFinishToken = (AUTO.__repeatFinishToken || 0) + 1;
            }
            return;
          }
          var fc = F(), fgm = fc && fc.fguiMgr;
          if (!fgm) return;
          var cands = getPanelCandidates(fgm, "AdventureFinishPanel"), ctrl = null;
          for (var i = 0; i < cands.length; i++) {
            if (typeof cands[i].efi === "function") { ctrl = cands[i]; break; }
          }
          if (!ctrl || AUTO.__repeatFinishScheduled === ctrl || AUTO.__repeatFinishHandled === ctrl) return;
          AUTO.__repeatFinishScheduled = ctrl;
          var token = AUTO.__repeatFinishToken = (AUTO.__repeatFinishToken || 0) + 1;
          setTimeout(function () {
            try {
              if (token !== AUTO.__repeatFinishToken) return;
              if (AUTO.__repeatFinishScheduled === ctrl) AUTO.__repeatFinishScheduled = null;
              if (!CFG.autoRepeat() || !panelOpen("AdventureFinishPanel")) return;
              var fc2 = F(), fgm2 = fc2 && fc2.fguiMgr;
              if (!fgm2) return;
              var now = getPanelCandidates(fgm2, "AdventureFinishPanel"), current = null;
              for (var j = 0; j < now.length; j++) {
                if (typeof now[j].efi === "function") { current = now[j]; break; }
              }
              if (!current || current !== ctrl) return;
              AUTO.__repeatFinishScheduled = null;
              AUTO.__repeatFinishHandled = current;
              current.efi();
              AUTO.__nextStageAt = Date.now();
              AUTO.__loadingLogged = 0;
              autoLog("重复挑战→efi");
            } catch (e) { log('重复挑战: ' + (e && e.message)); }
          }, 1500);
        } catch (e) { log('重复挑战: ' + (e && e.message)); }
      }
      // 引擎随 YSZX 一次性初始化；避免 remote.js 原先“首个 tick 强制清零”覆盖用户刚点开的开关。
      AUTO.__inited = true;
      AUTO.enabled = wxFlag('__WX_AUTO_TASK__');
      if (!AUTO.enabled) AUTO.state = 'IDLE';
      /* ===== 过检测（自 remote.js v33N 迁移，按 res 1.1.3.2 实况裁剪）=====
         上游 remote.js 那套是照 res 1.1.2.0 写的，整段搬过来在本版本只有一半能生效，
         所以这里只保留「在本版本明文源码里核实过、且不依赖混淆短名」的部分：
           ① 反作弊闸门：ANTI_CHEAT_ENABLED 恒 false。
              1.1.3.2 里 $PZ / $WS 两个模块仍在读它；定义处用 __require 包装 + 已知名 + 全局扫描覆盖。
           ② 上报 useTime 一致性补偿：挂 $eZ.battleServer.sendMsg（**不是** bEP/bEQ）。
           ③ 伤害字段还原：已并入 HitBus 的 hY 分发器（见 hookPrototype，不另挂一层）。
         明确不搬（原因写在这里，避免以后又被复制回来）：
           · computeCheckNum 包装 —— 1.1.3.2 明文包里这个方法已不存在（出现 0 次）。
           · bEP / bEQ / gV / bDQ 包装 —— 这几个是逐版本重新混淆的短名。1.1.3.2 里
             checkNum 的上报点是 $eZ.battleMgr.bEQ(fi,{frameId,randomSeed,checkNum:gV()})，
             bEP 变成了「按实体推帧」的热路径函数 bEP(entity, frame)。
             照搬 remote 的 bEP 包装会挂在热路径上，而且 e.frameCheck 永远取不到 → 静默失效。
           · checkNum 重算 —— 旧的 Σ fF[].hw.finalDamage 公式在 1.1.3.2 已不存在（gV() 换了算法），
             改写一个自己无法核实的 checkNum 反而更容易被判异常；源头伤害已由 ③ 还原。
           · exitProgram 拦截 —— 本文件已有 guardExit()，不重复挂。
         Java 层（EmutorUtils/NativeInfo/CocosBridgeAPI/System.exit）仍由 dex 负责。 */
      function acGateMod(m, tag) {
        try {
          if (!m || typeof m !== 'object' || !('ANTI_CHEAT_ENABLED' in m)) return false;
          Object.defineProperty(m, 'ANTI_CHEAT_ENABLED', {
            get: function () { return false; }, set: function () {}, configurable: true
          });
          ST.acGated = true;
          if (!ST.acMod) ST.acMod = tag;
          return true;
        } catch (e) { return false; }
      }
      function installAntiCheatGate() {
        try {
          var req = W.__require;
          if (typeof req === 'function' && !req.__wxAcLocked) {
            var wrap = function () { var m = req.apply(this, arguments); acGateMod(m, 'require'); return m; };
            for (var k in req) { try { wrap[k] = req[k]; } catch (e0) {} }
            wrap.__wxAcLocked = true;
            W.__require = wrap;
          }
          // 已知名：1.1.2.0 的旧名单 + 1.1.3.2 实测在读该标志的模块
          var names = ['$LZ', '$KF', '$KM', '$KK', '$KE', '$PZ', '$WS',
                       '$DOB', '$DOC', '$DOD', '$DOE', '$hU', '$hV', '$hW', '$hZ'];
          for (var i = 0; i < names.length; i++) { try { acGateMod(get(names[i]), names[i]); } catch (e1) {} }
          // 兜底：全局扫描（只在还没关闸时做，最快 5 秒一次）
          if (!ST.acGated) {
            var nowS = Date.now();
            if (nowS - (ST.acScanT || 0) > 5000) {
              ST.acScanT = nowS;
              for (var g in globalThis) { try { if (acGateMod(globalThis[g], 'scan:' + g)) break; } catch (e2) {} }
            }
          }
          // baR（"战斗进行中"标志，仅诊断）
          try {
            var bm = F() && F().battleMgr;
            var cands = bm ? [bm.bEu, bm.bEv, bm.bEw, bm] : [];
            for (var c = 0; c < cands.length; c++) {
              if (cands[c] && typeof cands[c].baR === 'boolean') { ST.baR = cands[c].baR; break; }
            }
          } catch (e3) {}
          if (ST.acGated && !ST.acLogged) {
            ST.acLogged = true;
            log('[AC] 闸门已加固（' + ST.acMod + ' ANTI_CHEAT_ENABLED=false）');
          }
        } catch (e) {}
      }
      /* 战斗状态：优先用 BattleCore 已维护的 BATTLE（fF 实体表 + gi 取实体 + fk 逻辑时间） */
      function findBattleState() {
        try {
          if (BATTLE && BATTLE.fF && typeof BATTLE.gi === 'function') return BATTLE;
          var bmS = F() && F().battleMgr;
          if (!bmS) return null;
          var cands = [bmS, bmS.bEt, bmS.bEu, bmS.bEv, bmS.bEw];
          for (var k2 in bmS) { try { cands.push(bmS[k2]); } catch (e2) {} }
          for (var i2 = 0; i2 < cands.length; i2++) {
            var o2 = cands[i2];
            if (o2 && typeof o2 === 'object' && o2.fF && typeof o2.gi === 'function') return o2;
          }
        } catch (e) {}
        return null;
      }
      function acTimeScale() {
        try { var v = parseFloat(lsGet('wx_gspeed', '1')); if (isFinite(v) && v > 0) return v; } catch (e) {}
        return 1;
      }
      /* 结算上报的 useTime 一致性补偿（逻辑同 remote v33e，公式未变）：
         开加速时游戏按【逻辑时长 fk】上报，而服务端能看到的是真实墙钟，两者对不上就是特征。 */
      function acFixReport(payload) {
        try {
          var cr = payload && payload.completeResult;
          if (!cr || typeof cr.useTime !== 'number') return;
          var st = findBattleState();
          var fk = (st && typeof st.fk === 'number') ? st.fk : 0;
          var spd = acTimeScale();
          var fkMs = Math.floor(fk * 1000);
          var cur = cr.useTime, want;
          if (spd > 1 && ST.riStartWall > 0) {
            var realMs = Date.now() - ST.riStartWall;
            var est = Math.floor(fk / spd * 1000);
            if (realMs < est) realMs = est;
            want = Math.max(1000, Math.min(realMs, fkMs > 0 ? fkMs : realMs));
          } else {
            want = cur;
            if (!isFinite(want) || want < 0) want = fkMs > 0 ? fkMs : 0;
            else if (fkMs > 0 && want > fkMs) want = fkMs;
          }
          want = Math.floor(want);
          if (want !== cur) ST.riFixedUt = (ST.riFixedUt || 0) + 1;
          cr.useTime = want;
          ST.riNote = 'useTime ' + cur + '->' + want + ' fk=' + fkMs + ' spd=' + spd;
        } catch (e) {}
      }
      /* 只挂命名的稳定入口（sendMsg / gsMgr.cQA），不碰逐版本重混淆的短名方法 */
      function acHookSink(obj, key) {
        try {
          var fn = obj && obj[key];
          if (typeof fn !== 'function' || fn.__wxAcSink) return false;
          var of = fn;
          var w = function (proto, payload) {
            try { acFixReport(payload); } catch (e) {}
            return of.apply(this, arguments);
          };
          w.__wxAcSink = true;
          obj[key] = w;
          return true;
        } catch (e) { return false; }
      }
      function installReportIntegrity() {
        try {
          var ok1 = acHookSink(W.$eZ && W.$eZ.battleServer, 'sendMsg');
          var ok2 = acHookSink(W.$eZ && W.$eZ.gsMgr, 'cQA');
          if ((ok1 || ok2) && !ST.riHooked) {
            ST.riHooked = true;
            log('[AC] 上报出口已挂钩（sendMsg=' + (ok1 ? 1 : 0) + ' cQA=' + (ok2 ? 1 : 0) + '）');
          }
        } catch (e) {}
      }
      function installAntiDetection() {
        try { installAntiCheatGate(); } catch (e0) {}
        try { installReportIntegrity(); } catch (e1) {}
        // 新一场战斗：回溯真实开战时刻（fk 是逻辑时间，除以时间倍率≈已进行的真实秒数）
        try {
          if (BATTLE !== ST.acBattle) {
            ST.acBattle = BATTLE;
            var fk0 = (BATTLE && typeof BATTLE.fk === 'number') ? BATTLE.fk : 0;
            ST.riStartWall = BATTLE ? (Date.now() - Math.floor(fk0 / acTimeScale() * 1000)) : 0;
          }
        } catch (e2) {}
      }
      W.__WX_INSTALL_AC__ = installAntiDetection;
      W.__WX_AC_STATE__ = function () {
        return {
          gated: !!ST.acGated, mod: ST.acMod || '', baR: !!ST.baR, ri: !!ST.riHooked,
          fixedUt: ST.riFixedUt || 0, dmgFixed: ST.acFixed || 0, note: ST.riNote || ''
        };
      };
      W.__REMOTE_EXECUTE__ = function (command, payload) {
        var fn = W.__REMOTE_COMMANDS__[command];
        if (typeof fn !== 'function') return { lastAction: 'unknown-command', command: command };
        return fn(payload || {}) || {};
      };
      W.__WX_AUTO_ENGINE_TICK__ = function () {
        try { autoRepeatFinishTick(); } catch (e0) { log('重复挑战: ' + (e0 && e0.message)); }
        try { autoTaskTick(); } catch (e) { log('主线: ' + (e && e.message)); }
        try { autoPostCleanupTick(); } catch (e2) { log('清理: ' + (e2 && e2.message)); }
        try { syncEmbeddedAutoConfig(); } catch (e3) {}
        try { autoSyncTaskInfo(); } catch (e4) {}
        return 1;
      };
      W.__WX_AUTO_ENGINE__ = { build: REMOTE_BUILD, state: AUTO, tick: W.__WX_AUTO_ENGINE_TICK__ };
    })(window);
    /*<WX_AUTO_ENGINE_END>*/
    /* ================= 4.5 轻量诊断 ================= */
    function diagAppend(line) {
        try {
            if (window.__WX_DIAG_IO__ !== true || !window.jsb || !jsb.fileUtils) return false;
            var path = writablePath() + 'wx_diag.txt';
            var old = jsb.fileUtils.isFileExist(path) ? (jsb.fileUtils.getStringFromFile(path) || '') : '';
            if (old.length > 12000) old = old.slice(-8000);
            return !!jsb.fileUtils.writeStringToFile(old + line + '\n', path);
        } catch (e) { return false; }
    }

    function diagTick() {
        try {
            if (window.__WX_DIAG_IO__ !== true) return;
            var now = Date.now();
            if (now - (window.__WX_DIAG_T__ || 0) < 5000) return;
            window.__WX_DIAG_T__ = now;
            diagAppend([
                new Date(now).toISOString().slice(11, 19),
                'ver=' + WXV,
                'battle=' + (BATTLE ? 1 : 0),
                'hits=' + ST.hits,
                'renderHooks=' + (ST.renderHooks || 0),
                'renderHits=' + (ST.renderHits || 0),
                'lock=' + ST.lock,
                'lockMp=' + ST.lockMp,
                'mpHook=' + (ST.mpHook ? 1 : 0),
                'guard=' + ST.noDmg,
                'move=' + (ST.moveHook ? ST.moveSource : 'off'),
                'global=' + ST.gsEnts,
                'auto=' + (ST.auto ? 1 : 0),
                'gold=' + (ST.gold ? 1 : 0),
                'errors=' + Object.keys(ST.errors).join(',')
            ].join(' '));
        } catch (e) {}
    }

    function writeState() {
        try {
            if (window.__WX_DIAG_IO__ !== true) return;
            var now = Date.now();
            if (now - (window.__WX_STATE_T__ || 0) < 5000) return;
            window.__WX_STATE_T__ = now;
            var state = {
                t: now,
                ver: WXV,
                auth: !!window.__WX_AUTH_OK__,
                menu: !!UI.root,
                battle: !!BATTLE,
                cfg: {
                    damage: CFG.on,
                    value: CFG.value,
                    lockHp: CFG.lockHp,
                    lockMp: CFG.lockMp,
                    noCd: CFG.noCd,
                    autoFight: CFG.autoFight,
                    speed: CFG.speed,
                    globalSpeed: CFG.gspeed
                },
                status: {
                    hitBus: !!ST.hitBus,
                    hits: ST.hits,
                    renderHooks: ST.renderHooks || 0,
                    renderHits: ST.renderHits || 0,
                    renderFields: ST.renderFields || 0,
                    renderDamage: ST.renderDamage || 0,
                    lock: ST.lock,
                    guard: ST.noDmg,
                    noCd: ST.noCd,
                    auto: ST.auto,
                    gold: ST.gold,
                    move: ST.moveSource,
                    globalEntities: ST.gsEnts,
                    battleSerial: ST.battleSerial
                },
                errors: ST.errors,
                env: diagObj()
            };
            writeFile(writablePath() + 'wx_state.txt', JSON.stringify(state));
        } catch (e) {}
    }

    /* ================= 5. 统一 tick ================= */
    window.__WX_TICK__ = function () {
        var now = Date.now();
        try { wxAuthWatchdog(); } catch (e) {}
        try { if ((!UI.root || !nodeMounted(UI.root)) && floatBuildAllowed()) buildMenu(); } catch (e2) {}
        try { FeatureManager.tick(now); } catch (e3) { featureError('featureTick', e3); }
        try {
            if (window.__WX_AUTH_OK__ && !isPermanentCard()) {
                if (!window.__WX_AUTO_GATE_CLEARED__) {
                    AutoBridge.disableAll();
                    window.__WX_AUTO_GATE_CLEARED__ = true;
                }
            } else if (window.__WX_AUTH_OK__ && window.__WX_AUTO_ENGINE_TICK__) {
                var autoEngine = window.__WX_AUTO_ENGINE__;
                if (autoEngine && autoEngine.state && autoEngine.state.enabled && !CFG.autoFight) {
                    CFG.autoFight = true; save(); unlockAuto(true);
                }
                window.__WX_AUTO_ENGINE_TICK__();
            }
        } catch (eAuto) { featureError('autoEngineTick', eAuto); }
        try { if (now - (ST.exitT || 0) > 1000) { ST.exitT = now; guardExit(); } } catch (e4) {}
        // 过检测：热更/换场景后模块会重建，每秒重装一次（各钩子都带幂等标记，不会叠加）
        try { if (now - (ST.acT || 0) > 1000) { ST.acT = now; if (typeof window.__WX_INSTALL_AC__ === 'function') window.__WX_INSTALL_AC__(); } } catch (eAc) {}
        try { diagTick(); } catch (e5) {}
        try { writeState(); } catch (e6) {}
        return 1;
    };

    window.__DMG_TICK__ = window.__WX_TICK__;

    /* ================= 6. 启动 ================= */
    function startFeatures() {
        var tries = 0;
        var t = null;
        function attempt() {
            tries++;
            try { BattleCore.bind(); FeatureManager.install(true); } catch (e) { featureError('startFeatures', e); }
            var done = false;
            try { done = buildMenu(); } catch (e) {}
            if ((done || tries > 600) && t !== null) { try { clearInterval(t); } catch (e) {} }
            return done;
        }
        if (attempt()) return;
        try { if (typeof setInterval === 'function') t = setInterval(attempt, 300); } catch (e) {}
    }

    window.__WX_AUTH_OK__ = false;
    window.__WX_CARD_PERMANENT__ = false;
    window.__Z6_IS_PERMANENT__ = false;
    window.__WX_AUTH_CARD_TYPE__ = '未登录';
    window.__WX_AUTO_GATE_CLEARED__ = false;
    if (WX.skipAuth) {                      // 调试：跳过卡密直接进菜单
        window.__WX_AUTH_OK__ = true;
        startFeatures();
        window.__WX_PATCH__ = true;
        window.__WX_PATCH_VERSION__ = WXV;
        return 'wx-patch-1-skipauth';
    }
    try { detectEnv(); } catch (e) {}                       // v66：鸿蒙 / 安卓16 环境探测
    hotUpdateAdapt();
    notice();
    window.__WX_AUTH_OK__ = false;                 // 卡密必须在面板之前
    // v66：**不做自动登录**，卡密仍然手动输入 + 点"立即登录"。
    // 这里只保证"卡密窗一定出得来"：50ms 快轮询 + 看门狗限速重建，鸿蒙上也不会卡在没窗口的状态。
    var authTries = 0;
    var waitAuth = null;
    try { if (ready()) makeAuth(); } catch (e) {}
    try {
        if (typeof setInterval === 'function') waitAuth = setInterval(function () {
            authTries++;
            if (ready()) { try { clearInterval(waitAuth); } catch (e) {} makeAuth(); }
            else if (authTries > 400) { try { clearInterval(waitAuth); } catch (e) {} }   // 20 秒还没就绪就交给看门狗
        }, 50);
    } catch (e) {}
    try { if (typeof setInterval === 'function') setInterval(function () { wxAuthWatchdog(); }, 500); } catch (e) {}
    hookResize();                                          // 折叠屏/旋转/分屏换尺寸时重新钳位
    installTickThrottle();
    startDrivers();                                        // v66：四路驱动，不再指望 dex 的 tick
    try { watchSceneAuth(); } catch (e) {}                  // v98-fix：切场景补建卡密窗（事件驱动）
    try { attachEngineDrivers(); } catch (e) {}             // v98-fix：引擎在就直接上调度器，不等 setTimeout 轮询

    window.__WX_PATCH__ = true;
    window.__WX_PATCH_VERSION__ = WXV;
    window.__WX_UI_READY__ = !!(window.__WX_AUTH_OK__ || (authRoot && nodeMounted(authRoot)));
    try { console.log('[PATCH] 勇士之心补丁 loaded ' + WXV + ' uiReady=' + window.__WX_UI_READY__); } catch (e) {}
    // 卡密窗还没挂上时返回独立字符串：加载器可据此决定“再注入一次”，而不是当成加载完成。
    return (window.__WX_UI_READY__ ? ('wx-patch-' + WXV) : ('wx-patch-need-auth-' + WXV));
})();
