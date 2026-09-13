/*
 * dcsl_patch.js - 星空版《地城狩猎》无限技能补丁（v2）
 *
 * Runtime: LayaAir/Conch H5, injected by com.starry.magic.conch.StarryConch
 * through ConchJNI.RunJS(...).
 *
 * Three client-side gates are handled:
 *   1. GSkillUtil.checkCanUseSkill: the server-facing usability gate.
 *   2. CFightMgr.inst.getAutoSkillId: the auto-battle selector.
 *   3. MainFubenView skill button components: the UI keeps its own cdEndTime
 *      and returns before SkillExecutor is called.
 *
 * This does not alter the server-authoritative C2S skill request or S2C result.
 */
(function () {
    'use strict';

    var w = window;
    if (!w) { return; }

    var G = w.GSkillUtil;
    var F = w.CFightMgr;
    var S = w.SkillMgr;
    var C = w.CSkillService;
    var B = w.CBattleMgr;

    function stateOn() {
        var s = w.__starryDcslState;
        return !!(s && s.live && s.skill);
    }

    function inBattle() {
        try {
            return !!(B && B.inst && B.inst.isStart);
        } catch (e) {
            return false;
        }
    }

    function ack(detail) {
        try {
            var c = w.conch || (typeof conch !== 'undefined' ? conch : null);
            if (c && typeof c.callMethod === 'function') {
                c.callMethod(0, true,
                    'com/starry/magic/conch/StarryConch', 'ack',
                    JSON.stringify([String(detail)]));
            }
        } catch (e) {
        }
    }

    function currentMyself() {
        try {
            return (w.CEntityService && w.CEntityService.inst)
                ? w.CEntityService.inst.myself : null;
        } catch (e) {
            return null;
        }
    }

    function isDead(obj) {
        try {
            return !!(w.GEntityUtils && w.GEntityUtils.isDie && w.GEntityUtils.isDie(obj));
        } catch (e) {
            return false;
        }
    }

    function canUseIgnoringCd(uuid, skillId, isEvolve, ignoreNearby) {
        if (!stateOn() || !inBattle()) { return false; }
        if (!skillId || skillId <= 0) { return false; }

        var myself = currentMyself();
        if (!myself || !myself.uuid || isDead(myself)) { return false; }
        if (!uuid) { uuid = myself.uuid; }

        try {
            var cfg = w.ConfigMgr && w.ConfigMgr.inst
                ? w.ConfigMgr.inst.getSkillCfg(skillId) : null;
            if (!cfg) { return false; }

            if (C && C.inst) {
                if (!C.inst.hasSkill(skillId)
                        && !isEvolve
                        && !(cfg.model && cfg.stype === 2)) {
                    return false;
                }
                if (!C.inst.hasEnemyNearby(uuid, skillId, !!ignoreNearby)) {
                    return false;
                }
            }
            if (S && S.inst) {
                if (!S.inst.canUseOnFight(skillId)) { return false; }
                if (!S.inst.canUseSummonSkill(skillId, uuid)) { return false; }
                if (!S.inst.canUseTransfigurationSkill(skillId, uuid)) { return false; }
            }
            return true;
        } catch (e) {
            return false;
        }
    }

    function installManualGate() {
        try {
            if (!G || typeof G.checkCanUseSkill !== 'function') { return 0; }
            if (G.checkCanUseSkill.__dcsl === 1) { return 2; }

            var orig = G.checkCanUseSkill;
            var wrap = function (uuid, skillId, isEvolve, ignoreNearby) {
                var normal = false;
                try {
                    normal = orig.apply(this, arguments);
                } catch (e) {
                    normal = false;
                }
                if (normal) { return true; }
                if (!canUseIgnoringCd(uuid, skillId, isEvolve, ignoreNearby)) {
                    return normal;
                }
                return true;
            };
            wrap.__dcsl = 1;
            wrap.__dcsl_orig = orig;
            G.checkCanUseSkill = wrap;
            return 1;
        } catch (e) {
            return 0;
        }
    }

    function installAutoGate() {
        try {
            if (!F || !F.inst || typeof F.inst.getAutoSkillId !== 'function') {
                return 0;
            }
            if (F.inst.getAutoSkillId.__dcsl === 1) { return 2; }

            var target = F.inst;
            var orig = target.getAutoSkillId;
            var wrap = function () {
                var normal = -1;
                try {
                    normal = orig.apply(this, arguments);
                } catch (e) {
                    normal = -1;
                }
                if (normal !== -1 || !stateOn() || !inBattle()) { return normal; }

                try {
                    var mgr = F.inst;
                    var sm = S && S.inst;
                    var cs = C && C.inst;
                    var myself = currentMyself();
                    if (!mgr || !sm || !cs || !myself) { return normal; }

                    var list = mgr.onlyNormalAttack
                        ? sm.getNormalAttackSkills()
                        : sm.getAutoBattle();
                    if (!list || !list.length) { return normal; }

                    var doing = mgr.doingSkillId;
                    var next = mgr._nextSkillId;
                    var last = mgr._lastSkillId;
                    for (var i = 0; i < list.length; i++) {
                        var id = list[i];
                        if (!id || id <= 0 || id === doing || id === next) {
                            continue;
                        }
                        if (doing && id === last) { continue; }
                        if (!canUseIgnoringCd(myself.uuid, id, false, false)) {
                            continue;
                        }
                        return id;
                    }
                } catch (e) {
                    return normal;
                }
                return normal;
            };
            wrap.__dcsl = 1;
            wrap.__dcsl_orig = orig;
            target.getAutoSkillId = wrap;
            return 1;
        } catch (e) {
            return 0;
        }
    }

    function installNetProbe() {
        try {
            if (w.__dcsl_net_probe) { return 2; }
            var svc = w.CFightService && w.CFightService.inst;
            if (!svc) { return 0; }

            if (typeof svc.C2SBattleSkillMulti === 'function'
                    && svc.C2SBattleSkillMulti.__dcsl_net !== 1) {
                var tx = svc.C2SBattleSkillMulti;
                var wrapTx = function () {
                    var n = (w.__dcsl_tx = (w.__dcsl_tx || 0) + 1);
                    var sid = arguments.length > 2 ? arguments[2] : '?';
                    if (n <= 10 || n % 20 === 0) { ack('tx=' + n + ',sid=' + sid); }
                    return tx.apply(this, arguments);
                };
                wrapTx.__dcsl_net = 1;
                svc.C2SBattleSkillMulti = wrapTx;
            }

            if (typeof svc.S2CBattleSkill === 'function'
                    && svc.S2CBattleSkill.__dcsl_net !== 1) {
                var rx = svc.S2CBattleSkill;
                var wrapRx = function (data) {
                    var n = (w.__dcsl_rx = (w.__dcsl_rx || 0) + 1);
                    var err = data && data.error_code;
                    var h0 = data && data.harms && data.harms.length ? data.harms[0] : null;
                    var harm = h0 && typeof h0.harm !== 'undefined' ? h0.harm : '?';
                    var hp = h0 && typeof h0.hp !== 'undefined' ? h0.hp : '?';
                    var dhp = h0 && typeof h0.d_hp !== 'undefined' ? h0.d_hp : '?';
                    var rsid = h0 && typeof h0.skill !== 'undefined' ? h0.skill : '?';
                    var ex = h0 && h0.extra_harm && h0.extra_harm.length ? h0.extra_harm[0] : null;
                    var xharm = ex && typeof ex.harm !== 'undefined' ? ex.harm : '?';
                    var keys = h0 ? Object.keys(h0).join('|') : '';
                    if (n <= 10 || n % 20 === 0 || err) {
                        ack('rx=' + n + ',sid=' + rsid + ',err=' + err + ',harm=' + harm + ',hp=' + hp + ',dhp=' + dhp + ',x=' + xharm + ',keys=' + keys);
                    }
                    return rx.apply(this, arguments);
                };
                wrapRx.__dcsl_net = 1;
                svc.S2CBattleSkill = wrapRx;
            }

            if ((svc.C2SBattleSkillMulti && svc.C2SBattleSkillMulti.__dcsl_net === 1)
                    || (svc.S2CBattleSkill && svc.S2CBattleSkill.__dcsl_net === 1)) {
                w.__dcsl_net_probe = 1;
                return 1;
            }
            return 0;
        } catch (e) {
            return 0;
        }
    }

    function clearUiCd(item) {
        if (!item) { return; }
        item.cdEndTime = 0;
        item._lastCdText = '';
        try {
            if (item.ui_txt_cd) { item.ui_txt_cd.text = ''; }
            if (item.ui_cdMask && typeof item.ui_cdMask.clearCD === 'function') {
                item.ui_cdMask.clearCD();
            }
            if (w.Laya && w.Laya.timer && typeof w.Laya.timer.clear === 'function'
                    && typeof item.loop === 'function') {
                w.Laya.timer.clear(item, item.loop);
            }
        } catch (e) {
        }
    }

    function patchSkillItem(item) {
        if (!item) { return 0; }
        var proto = Object.getPrototypeOf(item);
        if (!proto) { return 0; }
        if (proto.__dcsl_ui === 1) { return 2; }

        var click = proto.onClickSkillItem;
        var update = proto.doUpdateSkillCd;
        var onUpdate = proto.onUpdateTime;
        if (typeof click !== 'function'
                && typeof update !== 'function'
                && typeof onUpdate !== 'function') {
            return 0;
        }

        if (typeof click === 'function') {
            proto.onClickSkillItem = function () {
                if (stateOn()) { clearUiCd(this); }
                return click.apply(this, arguments);
            };
        }
        if (typeof update === 'function') {
            proto.doUpdateSkillCd = function () {
                var result = update.apply(this, arguments);
                if (stateOn()) { clearUiCd(this); }
                return result;
            };
        }
        if (typeof onUpdate === 'function') {
            proto.onUpdateTime = function () {
                var result = onUpdate.apply(this, arguments);
                if (stateOn()) { clearUiCd(this); }
                return result;
            };
        }
        proto.__dcsl_ui = 1;
        return 1;
    }

    function collectSkillItems(node, depth, seen, out) {
        if (!node || depth > 10 || out.length >= 5 || seen.length > 3000) {
            return;
        }
        try {
            if (seen.indexOf(node) >= 0) { return; }
            seen.push(node);
            if (typeof node.onClickSkillItem === 'function') {
                out.push(node);
                return;
            }
            var count = 0;
            try {
                if (typeof node.numChildren === 'number') { count = node.numChildren; }
            } catch (e) {
            }
            if (count > 0 && typeof node.getChildAt === 'function') {
                for (var i = 0; i < count; i++) {
                    collectSkillItems(node.getChildAt(i), depth + 1, seen, out);
                }
            }

            var children = node._children;
            if (children && children.length) {
                for (var j = 0; j < children.length; j++) {
                    collectSkillItems(children[j], depth + 1, seen, out);
                }
            }
        } catch (e) {
        }
    }

    function installUiGate() {
        try {
            if (w.__dcsl_ui_ready) { return 2; }
            var roots = [];
            if (w.Laya && w.Laya.stage) { roots.push(w.Laya.stage); }

            try {
                var uiMgr = w.UIManager || w.Dot;
                var map = uiMgr && uiMgr.inst && uiMgr.inst.m_viewMap;
                if (map) {
                    for (var key in map) {
                        if (!Object.prototype.hasOwnProperty.call(map, key)) { continue; }
                        var record = map[key];
                        if (record) { roots.push(record.view || record); }
                    }
                    if (!w.__dcsl_view_diag) {
                        var diag = [];
                        for (var dk in map) {
                            if (Object.prototype.hasOwnProperty.call(map, dk)) {
                                diag.push(dk);
                                if (diag.length >= 20) { break; }
                            }
                        }
                        w.__dcsl_view_diag = 1;
                        ack('views=' + diag.join(','));
                    }
                }
            } catch (e) {
            }

            var items = [];
            var seen = [];
            for (var r = 0; r < roots.length; r++) {
                collectSkillItems(roots[r], 0, seen, items);
                if (items.length >= 5) { break; }
            }
            if (!items.length) { return 0; }

            var installed = 0;
            for (var i = 0; i < items.length; i++) {
                if (patchSkillItem(items[i]) === 1) { installed = 1; }
            }
            if (installed || items.length > 0) { w.__dcsl_ui_ready = 1; }
            return installed ? 1 : 2;
        } catch (e) {
            return 0;
        }
    }

    var manual = 0;
    var auto = 0;
    var ui = 0;
    var net = 0;
    try { manual = installManualGate(); } catch (e) { manual = 0; }
    try { auto = installAutoGate(); } catch (e) { auto = 0; }
    try { ui = installUiGate(); } catch (e) { ui = 0; }
    try { net = installNetProbe(); } catch (e) { net = 0; }

    if (manual !== 0 || auto !== 0 || ui !== 0 || net !== 0) {
        w.__dcsl_v1 = 1;
    }
    if (manual === 1 && !w.__dcsl_manual_ack) {
        w.__dcsl_manual_ack = 1;
        ack('manual=1');
    }
    if (auto === 1 && !w.__dcsl_auto_ack) {
        w.__dcsl_auto_ack = 1;
        ack('auto=1');
    }
    if (ui === 1 && !w.__dcsl_ui_ack) {
        w.__dcsl_ui_ack = 1;
        ack('ui=1');
    }
    if (net === 1 && !w.__dcsl_net_ack) {
        w.__dcsl_net_ack = 1;
        ack('net=1');
    }
})();
