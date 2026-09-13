#!/usr/bin/env node
'use strict';

const fs = require('fs');
const path = require('path');
const vm = require('vm');

const ROOT = path.resolve(__dirname, '../..');
const PATCH = path.join(
    ROOT,
    'mod菜单/starry-mod-menu/app/src/main/assets/starry/dcsl_patch.js'
);
const source = fs.readFileSync(PATCH, 'utf8');

let passed = 0;
let failed = 0;
function check(name, got, want) {
    const ok = got === want;
    if (ok) {
        passed++;
    } else {
        failed++;
        console.error(`FAIL ${name}: got=${JSON.stringify(got)} want=${JSON.stringify(want)}`);
    }
}

function makeWorld(opts) {
    opts = opts || {};
    const state = {
        live: opts.live === undefined ? 1 : opts.live,
        skill: opts.skill === undefined ? 1 : opts.skill,
    };
    const calls = {
        manual: 0,
        auto: 0,
    };
    const GSkillUtil = {
        checkCanUseSkill: function () {
            calls.manual++;
            return false;
        },
    };
    const CFightMgr = {
        inst: {
            isStart: opts.inBattle === undefined ? true : !!opts.inBattle,
            doingSkillId: opts.doingSkillId || null,
            _nextSkillId: opts.nextSkillId || null,
            _lastSkillId: opts.lastSkillId || null,
            onlyNormalAttack: false,
            getAutoSkillId: function () {
                calls.auto++;
                return opts.normalAuto === undefined ? -1 : opts.normalAuto;
            },
        },
    };
    const SkillMgr = {
        inst: {
            getAutoBattle: function () { return [101, 102]; },
            getNormalAttackSkills: function () { return [201]; },
            canUseOnFight: function () { return true; },
            canUseSummonSkill: function () { return true; },
            canUseTransfigurationSkill: function () { return true; },
        },
    };
    const CSkillService = {
        inst: {
            hasSkill: function (id) { return id !== 900; },
            hasEnemyNearby: function () { return true; },
        },
    };
    const CBattleMgr = {
        inst: { isStart: opts.inBattle === undefined ? true : !!opts.inBattle },
    };
    const ConfigMgr = {
        inst: {
            getSkillCfg: function (id) {
                return id === 900 ? null : { id: id, model: 0, stype: 0 };
            },
        },
    };
    const CEntityService = {
        inst: { myself: opts.noMyself ? null : { uuid: 'me' } },
    };
    const GEntityUtils = {
        isDie: function () { return !!opts.dead; },
    };
    function SkillItem() {}
    SkillItem.prototype.onClickSkillItem = function () {
        return this.cdEndTime > 0 ? 'blocked' : 'clicked';
    };
    SkillItem.prototype.doUpdateSkillCd = function () {
        this.cdEndTime = 5000;
    };
    SkillItem.prototype.onUpdateTime = function () {};
    const skillItem = new SkillItem();
    skillItem.cdEndTime = 5000;
    skillItem._lastCdText = '5';
    skillItem.ui_txt_cd = { text: '5' };
    skillItem.ui_cdMask = { cleared: false, clearCD: function () { this.cleared = true; } };
    const Dot = {
        inst: {
            getView: function () {
                return { view: { ui_skillComp: { ui_skillItem0: skillItem } } };
            },
        },
    };
    const Laya = {
        timer: { clear: function () {} },
        stage: { _children: [{ _children: [skillItem] }] },
    };
    const window = {
        __starryDcslState: state,
        GSkillUtil: GSkillUtil,
        CFightMgr: CFightMgr,
        SkillMgr: SkillMgr,
        CSkillService: CSkillService,
        CBattleMgr: CBattleMgr,
        ConfigMgr: ConfigMgr,
        CEntityService: CEntityService,
        GEntityUtils: GEntityUtils,
        UIManager: Dot,
        Laya: Laya,
    };
    return { window, state, calls, skillItem };
}

function runPatch(world) {
    const context = {
        window: world.window,
        console: { log: function () {} },
        JSON: JSON,
        Object: Object,
        Array: Array,
    };
    vm.runInNewContext(source, context, { filename: PATCH });
}

function main() {
    {
        const w = makeWorld();
        delete w.window.GSkillUtil;
        delete w.window.CFightMgr;
        delete w.window.UIManager;
        delete w.window.Laya;
        runPatch(w);
        check('missing-deps does not set guard', w.window.__dcsl_v1, undefined);
    }
    {
        const w = makeWorld();
        runPatch(w);
        check('patch guard installed', w.window.__dcsl_v1, 1);
        check('manual hook installed', w.window.GSkillUtil.checkCanUseSkill.__dcsl, 1);
        check('auto hook installed', w.window.CFightMgr.inst.getAutoSkillId.__dcsl, 1);
        check('manual ON bypasses CD', w.window.GSkillUtil.checkCanUseSkill('me', 101, false, false), true);
        check('manual original was called', w.calls.manual, 1);
        w.state.skill = 0;
        check('manual OFF falls back', w.window.GSkillUtil.checkCanUseSkill('me', 101, false, false), false);
        w.state.skill = 1;
        w.state.live = 0;
        check('live=0 falls back', w.window.GSkillUtil.checkCanUseSkill('me', 101, false, false), false);
        w.state.live = 1;
        w.window.CBattleMgr.inst.isStart = false;
        check('out of battle falls back', w.window.GSkillUtil.checkCanUseSkill('me', 101, false, false), false);
        w.window.CBattleMgr.inst.isStart = true;
        check('unknown skill falls back', w.window.GSkillUtil.checkCanUseSkill('me', 900, false, false), false);
        w.window.GEntityUtils.isDie = function () { return true; };
        check('dead player falls back', w.window.GSkillUtil.checkCanUseSkill('me', 101, false, false), false);
    }
    {
        const w = makeWorld();
        runPatch(w);
        check('auto fallback ignores CD', w.window.CFightMgr.inst.getAutoSkillId(), 101);
        w.window.CFightMgr.inst.doingSkillId = 101;
        check('auto fallback rotates away from doing skill', w.window.CFightMgr.inst.getAutoSkillId(), 102);
        // Keep the wrapper in place; the next insertion exercises idempotence.
        w.window.CFightMgr.inst.doingSkillId = null;
        const before = w.calls.auto;
        runPatch(w);
        check('patch is idempotent', w.calls.auto - before, 0);
        check('manual wrapper not double wrapped',
            w.window.GSkillUtil.checkCanUseSkill.__dcsl_orig.__dcsl === undefined, true);
    }
    {
        const w = makeWorld();
        runPatch(w);
        check('UI cd hook installed', Object.getPrototypeOf(w.skillItem).__dcsl_ui, 1);
        check('UI ON bypasses button cd', w.skillItem.onClickSkillItem(), 'clicked');
        check('UI ON clears cd time', w.skillItem.cdEndTime, 0);
        w.state.skill = 0;
        w.skillItem.cdEndTime = 5000;
        check('UI OFF preserves button cd', w.skillItem.onClickSkillItem(), 'blocked');
        w.state.skill = 1;
        w.skillItem.doUpdateSkillCd();
        check('UI ON clears refresh cd', w.skillItem.cdEndTime, 0);
    }
    {
        const w = makeWorld({ normalAuto: 777 });
        runPatch(w);
        check('normal auto result preserved', w.window.CFightMgr.inst.getAutoSkillId(), 777);
    }

    console.log(`dcsl offline regression: ${passed} passed, ${failed} failed`);
    if (failed) { process.exit(1); }
}

main();
