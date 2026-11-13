#!/usr/bin/env python3
# script-policy: {"purpose": "把星光菜单 JS 侧补丁追加到吾辈小熊猫 assets/main.js（仅免广告；内购(GM) 2026-11-13 用户定删除）", "inputs": "03_decompiled/吾辈小熊猫/js_assets/assets/main.js（明文，4,970 B）", "outputs": "07_patched/吾辈小熊猫/assets/main.js（含补丁；幂等）", "boundary": "只追加一段 IIFE，不改原启动链；不动 index.jsc（jsc 由 patch_wbxxm_jsc.py 单独处理）", "prior_art": "10_docs/吾辈小熊猫/功能挂点分析.md §5.1（运行时补丁 VideoAdMgr.prototype.showVideoAd）", "difference": "补丁用 starry_state.txt 门控（feat#1），不是恒开；模块导入带重试等 main bundle 注册", "evidence": "10_docs/吾辈小熊猫/星光落地.md"}
"""把星光 JS 补丁追加到吾辈小熊猫 assets/main.js。

2026-11-13 用户定：功能只留「免广告」。
  · feat#2 内购(GM)：main.js 的 GameStartMgr.showGMLogin 分流已删除；jsc 侧 IsGmLogin
    也不再改（surgical 直接用底包原版 index.jsc）。
  · feat#3 无限体力：此前已删除（服务端字段，本地抬高被回滚）。

用法：
    python3 patch_wbxxm_mainjs.py [--src <main.js>] [--out <main.js>]

幂等：输出里已有 /* starry-mod:wbxxm:v1 标记则先剔旧再追加（改补丁后重跑即可）。
"""
import argparse
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))
DEFAULT_SRC = os.path.join(ROOT, '03_decompiled/吾辈小熊猫/js_assets/assets/main.js')
DEFAULT_OUT = os.path.join(ROOT, '07_patched/吾辈小熊猫/assets/main.js')

MARK_BEGIN = '/* ==== starry-mod:wbxxm:v1 begin'
MARK_END = '/* ==== starry-mod:wbxxm:v1 end'

PATCH = r'''
/* ==== starry-mod:wbxxm:v1 begin ============================================
 * 星光菜单 JS 侧补丁（吾辈小熊猫 5.0 / Cocos Creator 3.7.1）
 * 由 04_scripts/吾辈小熊猫/patch_wbxxm_mainjs.py 追加，勿手改。
 *
 * 为什么写在 main.js：本文件不在 assets/build-in.project.manifest 热更清单内
 * （清单 3,651 条只含 assets/** 与 src/** 的部分条目，无 main.js），
 * 而 assets/assets/main/index.jsc 在清单内、会被远端热更覆盖。
 *
 * 开关来源：{filesDir}/starry_state.txt（Java StateMirror 落盘，空格分隔 8 个 int，
 *           下标 = featNum）。jsb.fileUtils.getWritablePath() 就是 filesDir。
 *           [1]=免广告（本版唯一功能；原 feat#2/#3 已于 2026-11-13 删除）
 * 全部补丁失败只打日志，不抛错、不阻塞启动（任何异常都不能影响游戏本身的启动链）。
 * ========================================================================= */
(function () {
    'use strict';
    var TAG = '[StarryMod]';
    var FEAT_AD = 1;
    var STATE_FILE = 'starry_state.txt';
    var RETRY_MAX = 240, RETRY_GAP = 250;   /* 最多等 60s：等 main bundle 把模块注册进来 */

    if (typeof System === 'undefined' || typeof jsb === 'undefined') {
        console.warn(TAG + ' 无 System/jsb，跳过（不是 Cocos 原生环境？）');
        return;
    }

    var _vals = null, _ts = 0;
    function readState() {
        var now = Date.now();
        if (_vals && (now - _ts) < 400) return _vals;
        _ts = now;
        var out = [0, 0, 0, 0, 0, 0, 0, 0];
        try {
            var p = jsb.fileUtils.getWritablePath() || '';
            if (p && p.charAt(p.length - 1) !== '/') p += '/';
            p += STATE_FILE;
            if (jsb.fileUtils.isFileExist(p)) {
                var txt = jsb.fileUtils.getStringFromFile(p) || '';
                var parts = txt.replace(/\s+/g, ' ').replace(/^\s+|\s+$/g, '').split(' ');
                for (var i = 0; i < 8 && i < parts.length; i++) {
                    var n = parseInt(parts[i], 10);
                    out[i] = isNaN(n) ? 0 : n;
                }
            }
        } catch (e) {
            out = [0, 0, 0, 0, 0, 0, 0, 0];
        }
        _vals = out;
        return out;
    }
    function on(feat) {
        try { return readState()[feat] ? true : false; } catch (e) { return false; }
    }

    function imp(name, tries) {
        tries = tries || 0;
        try {
            return System.import('chunks:///_virtual/' + name).catch(function (err) {
                if (tries < RETRY_MAX) {
                    return new Promise(function (r) { setTimeout(r, RETRY_GAP); })
                        .then(function () { return imp(name, tries + 1); });
                }
                console.warn(TAG + ' 模块导入放弃: ' + name + ' :: ' + (err && err.message ? err.message : err));
                throw err;
            });
        } catch (e) {
            /* System.import 可能同步抛（模块未注册 / URL 解析失败）——退化成 rejected promise，
               让上层 .catch 走日志，绝不把异常抛回游戏启动链 */
            if (tries < RETRY_MAX) {
                return new Promise(function (r) { setTimeout(r, RETRY_GAP); })
                    .then(function () { return imp(name, tries + 1); });
            }
            return Promise.reject(e);
        }
    }
    function warn(what, e) {
        try { console.warn(TAG + ' ' + what + ' 失败: ' + (e && e.stack ? e.stack : e)); } catch (_) {}
    }

    /* ---- 0) 挂载时机：必须等引擎场景启动完成，不能在启动同 tick 内 import ----
     * 事故（2026-11-13 真机）：main.js 末尾 System.import('./src/application.js') 是**异步**
     * 启动链，本补丁原版在同一 tick 同步 System.import('chunks:///_virtual/*.ts') 时，bundle
     * 尚未注册 ⇒ Cocos SystemJS 抛 "Unable to instantiate ... from undefined"，并击穿引擎启动
     * promise（native 连打 rejectAfterPromiseResolved）⇒ 主 bundle 永不加载 ⇒ **进游戏黑屏**。
     * A/B 实测（同包仅换 main.js）：直连版 13 条 reject / 0 场景；延后版 0 条 / entry.scene 正常。
     * ⇒ 统一延后到 EVENT_AFTER_SCENE_LAUNCH（此时 _virtual 模块已注册），失败只 warn。 */
    function whenReady(fn) {
        var waited = 0, STEP = 100, MAX_WAIT = 30000;
        function arm() {
            try {
                if (typeof cc !== 'undefined' && cc && cc.game && cc.director) {
                    var D = cc.Director;
                    if (D && D.EVENT_AFTER_SCENE_LAUNCH) {
                        cc.director.once(D.EVENT_AFTER_SCENE_LAUNCH, function () { setTimeout(fn, 0); });
                        return;
                    }
                    setTimeout(fn, 0);
                    return;
                }
            } catch (e) { /* 引擎未就绪，继续轮询 */ }
            if (waited >= MAX_WAIT) { console.warn(TAG + ' 引擎 30s 未就绪，放弃挂载补丁（不影响游戏启动）'); return; }
            waited += STEP;
            setTimeout(arm, STEP);
        }
        setTimeout(arm, STEP);   /* 关键：绝不在启动同 tick 内 import */
    }
    console.log(TAG + ' 补丁待命中（等引擎场景启动后挂载，开关来自 ' + STATE_FILE + '）');

    whenReady(function () {
    try {

    /* ---- 1) 免广告：VideoAdMgr.showVideoAd 短路为直接发奖 --------------------
     * 原逻辑：AdFreeCard>0 / 月卡未过期 → 直接发奖；XI_PU → 拉起 TapAD 激励视频，
     *          回 onXIPUAdRewardG → emit(AdFinished,true) → cb()（发奖走 CS1602_BuyShopItem）。
     * 补丁：开关打开时跳过平台/广告/锁定/超时，直接执行 cb() —— 与原「免广告卡」分支同语义。 */
    imp('VideoAdMgr.ts').then(function (mod) {
        var C = mod && mod.VideoAdMgr;
        if (!C || !C.prototype) { console.warn(TAG + ' VideoAdMgr 形态不符，免广告未装'); return; }
        if (C.prototype.__starryAdPatch) return;
        var orig = C.prototype.showVideoAd;
        C.prototype.showVideoAd = function (adShopItemId, rewardName, cb) {
            if (!on(FEAT_AD)) return orig.apply(this, arguments);
            try {
                this.locked = false;
                this._rewardCallback = null;
                console.log(TAG + ' 免广告：跳过广告直接发奖 ad=' + adShopItemId + ' reward=' + rewardName);
                if (typeof cb === 'function') cb();
            } catch (e) { warn('免广告发奖', e); }
        };
        C.prototype.__starryAdPatch = 1;
        console.log(TAG + ' 免广告补丁已装（feat#' + FEAT_AD + '）');
    }).catch(function (e) { warn('免广告补丁', e); });

        console.log(TAG + ' 补丁链已挂载（开关来自 ' + STATE_FILE + '）');

    } catch (e) { warn('补丁链挂载', e); }
    });
})();
/* ==== starry-mod:wbxxm:v1 end ============================================ */
'''


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--src', default=DEFAULT_SRC)
    ap.add_argument('--out', default=DEFAULT_OUT)
    args = ap.parse_args()

    if not os.path.isfile(args.src):
        sys.exit('ERROR: 缺输入 %s' % args.src)
    raw = open(args.src, 'r', encoding='utf-8').read()
    if 'starry-mod:wbxxm:v1' in raw:
        raw = re.sub(re.escape(MARK_BEGIN) + r'.*?' + re.escape(MARK_END) + r'[^\n]*\n?',
                     '', raw, flags=re.S)
        print('[main.js] 已剔除旧补丁块（重打）')

    # 断言：原启动链没被换版本改掉（换版本要重新核对注入点）
    for need in ('System.import(\'./src/application.js\')', 'application.start()'):
        if need not in raw:
            sys.exit('ERROR: 原 main.js 缺关键段 %r（底包换代？需重新核对注入点）' % need)

    out = raw.rstrip('\n') + '\n' + PATCH + '\n'
    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    open(args.out, 'w', encoding='utf-8').write(out)
    print('[main.js] 写出 %s（%d B，原 %d B，+%d B）'
          % (args.out, len(out.encode()), len(raw.encode()), len(out.encode()) - len(raw.encode())))
    print('[main.js] 补丁标记: %d 处 begin / %d 处 end'
          % (out.count(MARK_BEGIN), out.count(MARK_END)))


if __name__ == '__main__':
    main()
