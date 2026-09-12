System.register("chunks:///_virtual/AngleOvertimeModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DDefine.ts", "./EasyP2DUtils.ts", "./EasyP2DModuleBase.ts"], (function(e) {
    var n, t, i, r, a, o, l, c, s, u, b;
    return {
        setters: [function(e) {
            n = e.applyDecoratedDescriptor, t = e.inheritsLoose, i = e.initializerDefineProperty, r = e.assertThisInitialized
        }, function(e) {
            a = e.cclegacy, o = e._decorator, l = e.math, c = e.CurveRange
        }, function(e) {
            s = e.DirectionType
        }, function(e) {
            u = e.calcValueOnCurve
        }, function(e) {
            b = e.EasyP2DModuleBase
        }],
        execute: function() {
            var f, p, y, m, d, h, v, g, D, w, I;
            a._RF.push({}, "06ca1MyHwVPH7cNIsTDYfGR", "AngleOvertimeModule", void 0);
            var M = o.property,
                P = o.ccclass;
            e("AngleOvertimeModule", (f = P("AngleOvertimeModule"), p = M({
                visible: function() {
                    return this.enabled && !this.byIncrement
                }
            }), y = M({
                visible: function() {
                    return this.enabled
                }
            }), m = M({
                visible: function() {
                    return this.enabled && this.byIncrement
                }
            }), d = M({
                visible: function() {
                    return this.enabled
                }
            }), f((g = n((v = function(e) {
                function n() {
                    var n;
                    return n = e.call(this) || this, i(n, "angle", g, r(n)), i(n, "byIncrement", D, r(n)), i(n, "increment", w, r(n)), i(n, "scale", I, r(n)), n.enableOnUpdate = !0, n.enableOnEmitter = !0, n.scale.constant = 1, n
                }
                t(n, e);
                var a = n.prototype;
                return a.onEmitter = function(e, n) {
                    this.byIncrement && (e._increment = l.toRadian(u(this.increment, n)))
                }, a.onUpdate = function(e, n, t) {
                    if (this._easyP2D.system.rotationType == s.None) {
                        if (this.byIncrement) e.rotation = e.rotation + e._increment * n;
                        else {
                            var i = l.toRadian(u(this.angle, t));
                            e.rotation = i
                        }
                        e.rotation *= u(this.scale, t)
                    }
                }, n
            }(b)).prototype, "angle", [p], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new c
                }
            }), D = n(v.prototype, "byIncrement", [y], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), w = n(v.prototype, "increment", [m], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new c
                }
            }), I = n(v.prototype, "scale", [d], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new c
                }
            }), h = v)) || h));
            a._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/AnimationModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DUtils.ts", "./EasyP2DModuleBase.ts"], (function(e) {
    var t, r, i, a, n, s, o, u, l, m, c, p;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, r = e.inheritsLoose, i = e.initializerDefineProperty, a = e.assertThisInitialized, n = e.createClass
        }, function(e) {
            s = e.cclegacy, o = e._decorator, u = e.CCInteger, l = e.Vec2, m = e.CurveRange
        }, function(e) {
            c = e.calcValueOnCurve
        }, function(e) {
            p = e.EasyP2DModuleBase
        }],
        execute: function() {
            var h, y, f, b, F, _, d, M, v, N, g, D;
            s._RF.push({}, "2595fLSdcVGbZNfN16EVsxP", "AnimationModule", void 0);
            var P = o.property,
                w = o.ccclass;
            e("AnimationModule", (h = w("AnimationModule"), y = P({
                type: u,
                min: 1
            }), f = P({
                step: 1,
                visible: !1
            }), b = P({
                step: 1
            }), F = P({
                type: u,
                min: 0
            }), _ = P({
                min: 0
            }), h((v = t((M = function(e) {
                function t() {
                    var t;
                    return t = e.call(this) || this, i(t, "repeat", v, a(t)), i(t, "_tiles", N, a(t)), i(t, "frameNum", g, a(t)), i(t, "startFrame", D, a(t)), t._realFrameNum = 0, t._totalFrameNum = 0, t.enableOnUpdate = !0, t.enableOnEmitter = !0, t
                }
                r(t, e);
                var s = t.prototype;
                return s.reset = function() {
                    this._easyP2D.system._syncAspect()
                }, s.updateUVs = function() {
                    var e = this._easyP2D.system;
                    e.needReplaceUVs = !0;
                    var t = e.uv,
                        r = e.replaceUVs;
                    r[0] = Math.min(t[0], t[6]), r[1] = Math.abs(t[0] - t[6]), r[2] = Math.min(t[4], t[2]), r[3] = Math.abs(t[4] - t[2]), r[4] = Math.min(t[1], t[7]), r[5] = Math.abs(t[1] - t[7]), r[6] = Math.min(t[3], t[5]), r[7] = Math.abs(t[3] - t[5]), r[8] = t[0] <= t[6] ? 0 : 1, r[9] = t[1] <= t[7] ? 0 : 1, r[10] = t[2] <= t[4] ? 0 : 1, r[11] = t[3] <= t[5] ? 0 : 1, r[12] = t[2] > t[4] ? 0 : 1, r[13] = t[3] > t[5] ? 0 : 1, r[14] = t[0] > t[6] ? 0 : 1, r[15] = t[1] > t[7] ? 0 : 1
                }, s.onEmitter = function(e, t) {
                    var r = this._easyP2D.system;
                    if (r.spriteFrame) {
                        var i = this.tiles;
                        this._totalFrameNum = i.x * i.y, this._realFrameNum = this.frameNum > 0 ? Math.min(this.frameNum, this._totalFrameNum) : this._totalFrameNum, r.spriteFrame.rotated ? (e.tiles.x = i.y, e.tiles.y = i.x) : (e.tiles.x = i.x, e.tiles.y = i.y), e.startFrame = Math.round(c(this.startFrame, t)), e.currentFrame = -1
                    }
                }, s.onUpdate = function(e, t, r) {
                    var i = Math.floor(this._realFrameNum * this.repeat * (1 - e.timeToLive / e.totalTime) + e.startFrame) % this._totalFrameNum;
                    i !== e.currentFrame && (e.currentFrame = i, e.frameIsDirty = !0)
                }, n(t, [{
                    key: "tiles",
                    get: function() {
                        return this._tiles
                    },
                    set: function(e) {
                        this._tiles = e, this.reset()
                    }
                }]), t
            }(p)).prototype, "repeat", [y], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 1
                }
            }), N = t(M.prototype, "_tiles", [f], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new l(1, 1)
                }
            }), t(M.prototype, "tiles", [b], Object.getOwnPropertyDescriptor(M.prototype, "tiles"), M.prototype), g = t(M.prototype, "frameNum", [F], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 0
                }
            }), D = t(M.prototype, "startFrame", [_], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new m
                }
            }), d = M)) || d));
            s._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/AttrConst.ts", ["cc"], (function(_) {
    var s;
    return {
        setters: [function(_) {
            s = _.cclegacy
        }],
        execute: function() {
            s._RF.push({}, "14f3c0ibx9Fjbi0hKpv5ZVL", "AttrConst", void 0);
            _("AttribDefine", function(_) {
                return _[_.cur_hp = 1e3] = "cur_hp", _[_.hp = 1001] = "hp", _[_.att = 1002] = "att", _[_.def = 1003] = "def", _[_.brave = 1004] = "brave", _[_.speed = 1005] = "speed", _[_.att_speed = 1006] = "att_speed", _[_.damage = 1009] = "damage", _[_.cur_anger = 1100] = "cur_anger", _[_.anger_max = 1101] = "anger_max", _[_.anger_recovery = 1111] = "anger_recovery", _[_.anger_recovery_damage = 1112] = "anger_recovery_damage", _[_.anger_recovery_kill = 1113] = "anger_recovery_kill", _[_.shield = 1200] = "shield", _[_.all_hp = 1201] = "all_hp", _[_.all_att = 1202] = "all_att", _[_.hp_bonus = 2001] = "hp_bonus", _[_.att_bonus = 2002] = "att_bonus", _[_.def_bonus = 2003] = "def_bonus", _[_.brave_bonus = 2004] = "brave_bonus", _[_.speed_bonus = 2005] = "speed_bonus", _[_.att_speed_bonus = 2006] = "att_speed_bonus", _[_.all_hp_bonus = 2201] = "all_hp_bonus", _[_.all_att_bonus = 2202] = "all_att_bonus", _[_.dam_bonus = 3001] = "dam_bonus", _[_.dam_resist = 3002] = "dam_resist", _[_.anger_efficiency = 3011] = "anger_efficiency", _[_.anger_efficiency_reduce = 3012] = "anger_efficiency_reduce", _[_.parry_anger_efficiency_bonus = 3013] = "parry_anger_efficiency_bonus", _[_.crit_rate = 3021] = "crit_rate", _[_.ignore_crit = 3022] = "ignore_crit", _[_.continuous_crit = 3023] = "continuous_crit", _[_.continuous_crit_resist = 3024] = "continuous_crit_resist", _[_.crit_dam = 3031] = "crit_dam", _[_.ignore_crit_dam = 3032] = "ignore_crit_dam", _[_.parry_dam_bonus = 3051] = "parry_dam_bonus", _[_.parry_resist = 3052] = "parry_resist", _[_.skill_dam = 3061] = "skill_dam", _[_.skill_resist = 3062] = "skill_resist", _[_.continuous_dam = 3071] = "continuous_dam", _[_.continuous_resist = 3072] = "continuous_resist", _[_.boss_dam = 3081] = "boss_dam", _[_.boss_dam_resist = 3082] = "boss_dam_resist", _[_.heal_bonus = 3091] = "heal_bonus", _[_.curse_effect = 3098] = "curse_effect", _[_.shield_dam = 3101] = "shield_dam", _[_.shield_resist = 3102] = "shield_resist", _[_.hit_rate = 3111] = "hit_rate", _[_.dodge_rate = 3112] = "dodge_rate", _[_.suck_hp = 3201] = "suck_hp", _[_.parry_suck_hp = 3202] = "parry_suck_hp", _[_.skill_suck_hp = 3203] = "skill_suck_hp", _[_.continuous_suck_hp = 3204] = "continuous_suck_hp", _[_.secret_dam = 3211] = "secret_dam", _[_.secret_dam_resist = 3212] = "secret_dam_resist", _[_.control_hit = 3221] = "control_hit", _[_.control_resist = 3222] = "control_resist", _[_.ignore_def = 3301] = "ignore_def", _[_.coefficient_bonus = 4001] = "coefficient_bonus", _[_.global_hp_bonus = 22001] = "global_hp_bonus", _[_.global_att_bonus = 22002] = "global_att_bonus", _[_.global_def_bonus = 22003] = "global_def_bonus", _[_.global_brave_bonus = 22004] = "global_brave_bonus", _[_.global_speed_bonus = 22005] = "global_speed_bonus", _[_.global_att_speed_bonus = 22006] = "global_att_speed_bonus", _[_.book_hp = 32001] = "book_hp", _[_.book_att = 32002] = "book_att", _[_.book_def = 32003] = "book_def", _[_.book_brave = 32004] = "book_brave", _[_.book_speed = 32005] = "book_speed", _[_.book_att_speed = 32006] = "book_att_speed", _
            }({}));
            var a = _("NewAttribDefine", function(_) {
                return _[_.cur_hp = 1e3] = "cur_hp", _[_.hp = 1001] = "hp", _[_.att = 1002] = "att", _[_.speed = 1005] = "speed", _[_.speed10 = 10050] = "speed10", _[_.cur_anger = 1100] = "cur_anger", _[_.anger_max = 1101] = "anger_max", _[_.anger_recovery_frame = 1102] = "anger_recovery_frame", _[_.skill_anger_min = 1103] = "skill_anger_min", _[_.shield = 1200] = "shield", _[_.all_hp = 20001] = "all_hp", _[_.all_att = 20002] = "all_att", _[_.hp_bonus = 2001] = "hp_bonus", _[_.att_bonus = 2002] = "att_bonus", _[_.speed_bonus = 2005] = "speed_bonus", _[_.all_hp_bonus = 22001] = "all_hp_bonus", _[_.all_att_bonus = 22002] = "all_att_bonus", _[_.dam_bonus = 3001] = "dam_bonus", _[_.dam_resist = 3002] = "dam_resist", _[_.buff_dam_bonus_active = 3003] = "buff_dam_bonus_active", _[_.crit_rate = 3011] = "crit_rate", _[_.ignore_crit = 3012] = "ignore_crit", _[_.crit_dam_bonus = 3013] = "crit_dam_bonus", _[_.ignore_crit_dam = 3014] = "ignore_crit_dam", _[_.hit_rate = 3021] = "hit_rate", _[_.dodge_rate = 3022] = "dodge_rate", _[_.phy_dam_bonus = 3031] = "phy_dam_bonus", _[_.phy_dam_resist = 3032] = "phy_dam_resist", _[_.mag_dam_bonus = 3033] = "mag_dam_bonus", _[_.mag_dam_resist = 3034] = "mag_dam_resist", _[_.final_dam_bonus = 3035] = "final_dam_bonus", _[_.final_dam_resist = 3036] = "final_dam_resist", _[_.heal_bonus = 3037] = "heal_bonus", _[_.curse_effect = 3038] = "curse_effect", _[_.control_hit = 3039] = "control_hit", _[_.control_resist = 3040] = "control_resist", _[_.negative_hit = 3041] = "negative_hit", _[_.negative_resist = 3042] = "negative_resist", _[_.cool_reduce = 3046] = "cool_reduce", _[_.cool_add = 3047] = "cool_add", _[_.parry_cool_reduce = 3048] = "parry_cool_reduce", _[_.parry_cool_add = 3049] = "parry_cool_add", _[_.shield_dam = 3071] = "shield_dam", _[_.shield_resist = 3072] = "shield_resist", _[_.continuous_dam_bonus = 3076] = "continuous_dam_bonus", _[_.continuous_dam_resist = 3077] = "continuous_dam_resist", _[_.suck_hp_rate = 3081] = "suck_hp_rate", _[_.coefficient_bonus = 5001] = "coefficient_bonus", _[_.in_battle_dam_bonus = 5011] = "in_battle_dam_bonus", _[_.in_battle_dam_resist = 5012] = "in_battle_dam_resist", _[_.pvp_debuff_resist = 5015] = "pvp_debuff_resist", _[_.red_dam_bonus = 3056] = "red_dam_bonus", _[_.blue_dam_bonus = 3057] = "blue_dam_bonus", _[_.dream_dam_bonus = 3058] = "dream_dam_bonus", _[_.angel_dam_bonus = 3059] = "angel_dam_bonus", _[_.dark_dam_bonus = 3060] = "dark_dam_bonus", _[_.warrior_dam_bonus = 3091] = "warrior_dam_bonus", _[_.knight_dam_bonus = 3092] = "knight_dam_bonus", _[_.mage_dam_bonus = 3093] = "mage_dam_bonus", _[_.assist_dam_bonus = 3094] = "assist_dam_bonus", _[_.final_hp = 30001] = "final_hp", _[_.final_att = 30002] = "final_att", _[_.pvp_dam_bonus = 4001] = "pvp_dam_bonus", _[_.pvp_dam_resist = 4002] = "pvp_dam_resist", _[_.pvp_buff_dam_bonus_active = 4003] = "pvp_buff_dam_bonus_active", _[_.pvp_crit_rate = 4011] = "pvp_crit_rate", _[_.pvp_ignore_crit = 4012] = "pvp_ignore_crit", _[_.pvp_crit_dam_bonus = 4013] = "pvp_crit_dam_bonus", _[_.pvp_ignore_crit_dam = 4014] = "pvp_ignore_crit_dam", _[_.pvp_hit_rate = 4021] = "pvp_hit_rate", _[_.pvp_dodge_rate = 4022] = "pvp_dodge_rate", _[_.pvp_phy_dam_bonus = 4031] = "pvp_phy_dam_bonus", _[_.pvp_phy_dam_resist = 4032] = "pvp_phy_dam_resist", _[_.pvp_mag_dam_bonus = 4033] = "pvp_mag_dam_bonus", _[_.pvp_mag_dam_resist = 4034] = "pvp_mag_dam_resist", _[_.pvp_final_dam_bonus = 4035] = "pvp_final_dam_bonus", _[_.pvp_final_dam_resist = 4036] = "pvp_final_dam_resist", _[_.pvp_heal_bonus = 4037] = "pvp_heal_bonus", _[_.pvp_curse_effect = 4038] = "pvp_curse_effect", _[_.pvp_control_hit = 4039] = "pvp_control_hit", _[_.pvp_control_resist = 4040] = "pvp_control_resist", _[_.pvp_negative_hit = 4041] = "pvp_negative_hit", _[_.pvp_negative_resist = 4042] = "pvp_negative_resist", _[_.pvp_cool_reduce = 4046] = "pvp_cool_reduce", _[_.pvp_cool_add = 4047] = "pvp_cool_add", _[_.pvp_shield_dam = 4091] = "pvp_shield_dam", _[_.pvp_shield_resist = 4092] = "pvp_shield_resist", _[_.pvp_continuous_dam_bonus = 4096] = "pvp_continuous_dam_bonus", _[_.pvp_continuous_dam_resist = 4097] = "pvp_continuous_dam_resist", _[_.pvp_red_dam_bonus = 4051] = "pvp_red_dam_bonus", _[_.pvp_blue_dam_bonus = 4052] = "pvp_blue_dam_bonus", _[_.pvp_dream_dam_bonus = 4053] = "pvp_dream_dam_bonus", _[_.pvp_angel_dam_bonus = 4054] = "pvp_angel_dam_bonus", _[_.pvp_dark_dam_bonus = 4055] = "pvp_dark_dam_bonus", _[_.pvp_red_dam_resist = 4061] = "pvp_red_dam_resist", _[_.pvp_blue_dam_resist = 4062] = "pvp_blue_dam_resist", _[_.pvp_dream_dam_resist = 4063] = "pvp_dream_dam_resist", _[_.pvp_angel_dam_resist = 4064] = "pvp_angel_dam_resist", _[_.pvp_dark_dam_resist = 4065] = "pvp_dark_dam_resist", _[_.pvp_warrior_dam_bonus = 4071] = "pvp_warrior_dam_bonus", _[_.pvp_knight_dam_bonus = 4072] = "pvp_knight_dam_bonus", _[_.pvp_mage_dam_bonus = 4073] = "pvp_mage_dam_bonus", _[_.pvp_assassin_dam_bonus = 4074] = "pvp_assassin_dam_bonus", _[_.pvp_warrior_dam_resist = 4081] = "pvp_warrior_dam_resist", _[_.pvp_knight_dam_resist = 4082] = "pvp_knight_dam_resist", _[_.pvp_mage_dam_resist = 4083] = "pvp_mage_dam_resist", _[_.pvp_assassin_dam_resist = 4084] = "pvp_assassin_dam_resist", _[_.pvp_suck_hp_rate = 4101] = "pvp_suck_hp_rate", _[_.pvp_wall_att_bonus = 5016] = "pvp_wall_att_bonus", _[_.elite_dam_bonus = 3303] = "elite_dam_bonus", _[_.counter_crit_dam_bonus = 3351] = "counter_crit_dam_bonus", _[_.gate_crit_dam_bonus = 3352] = "gate_crit_dam_bonus", _[_.warrior_crit_dam_bonus = 3353] = "warrior_crit_dam_bonus", _[_.mage_crit_dam_bonus = 3354] = "mage_crit_dam_bonus", _[_.knight_crit_dam_bonus = 3355] = "knight_crit_dam_bonus", _[_.assassin_crit_dam_bonus = 3356] = "assassin_crit_dam_bonus", _[_.counter_dam_bonus = 3357] = "counter_dam_bonus", _[_.all_gate_dam_bonus = 3358] = "all_gate_dam_bonus", _[_.burn_dam_bonus = 3359] = "burn_dam_bonus", _[_.stun_dam_bonus = 3360] = "stun_dam_bonus", _[_.freeze_dam_bonus = 3361] = "freeze_dam_bonus", _[_.explosive_dam_bonus = 5013] = "explosive_dam_bonus", _
            }({}));
            _("AttribDefineList", Object.values(a).filter((function(_) {
                return "number" == typeof _
            }))), _("PvpAttribList", [a.pvp_dam_bonus, a.pvp_dam_resist, a.pvp_buff_dam_bonus_active, a.pvp_crit_rate, a.pvp_ignore_crit, a.pvp_crit_dam_bonus, a.pvp_ignore_crit_dam, a.pvp_hit_rate, a.pvp_dodge_rate, a.pvp_phy_dam_bonus, a.pvp_phy_dam_resist, a.pvp_mag_dam_bonus, a.pvp_mag_dam_resist, a.pvp_final_dam_bonus, a.pvp_final_dam_resist, a.pvp_heal_bonus, a.pvp_curse_effect, a.pvp_control_hit, a.pvp_control_resist, a.pvp_negative_hit, a.pvp_negative_resist, a.pvp_cool_reduce, a.pvp_shield_dam, a.pvp_shield_resist, a.pvp_continuous_dam_bonus, a.pvp_continuous_dam_resist, a.pvp_red_dam_bonus, a.pvp_blue_dam_bonus, a.pvp_dream_dam_bonus, a.pvp_angel_dam_bonus, a.pvp_dark_dam_bonus, a.pvp_red_dam_resist, a.pvp_blue_dam_resist, a.pvp_dream_dam_resist, a.pvp_angel_dam_resist, a.pvp_dark_dam_resist, a.pvp_warrior_dam_bonus, a.pvp_knight_dam_bonus, a.pvp_mage_dam_bonus, a.pvp_assassin_dam_bonus, a.pvp_warrior_dam_resist, a.pvp_knight_dam_resist, a.pvp_mage_dam_resist, a.pvp_assassin_dam_resist, a.pvp_suck_hp_rate]), _("PveAttribList", [a.dam_bonus, a.dam_resist, a.buff_dam_bonus_active, a.buff_dam_bonus_active, a.crit_rate, a.ignore_crit, a.crit_dam_bonus, a.ignore_crit_dam, a.hit_rate, a.dodge_rate, a.phy_dam_bonus, a.phy_dam_resist, a.mag_dam_bonus, a.mag_dam_resist, a.final_dam_bonus, a.final_dam_resist, a.heal_bonus, a.curse_effect, a.control_hit, a.control_resist, a.negative_hit, a.negative_resist, a.cool_reduce, a.shield_dam, a.shield_resist, a.continuous_dam_bonus, a.continuous_dam_resist, a.suck_hp_rate]);
            s._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/BattleConst.ts", ["cc"], (function(e) {
    var t;
    return {
        setters: [function(e) {
            t = e.cclegacy
        }],
        execute: function() {
            var n;
            t._RF.push({}, "d9538dF2klGjphFr1EKsuSY", "BattleConst", void 0);
            e("TeamType", {
                Challenge: 0,
                Rival: 1
            }), e("UnityType", {
                Unknown: 0,
                Player: 1,
                Partner: 2,
                Monster: 3,
                EliteMonster: 4,
                Boss: 5,
                CallUnit: 6,
                Wall: 7,
                Long: 8,
                AdvancedLong: 9,
                MutatedLongSliver: 9999,
                MutatedLongCallUnit: 1e4
            }), e("JobType", {
                Unknown: 0,
                Warrior: 1,
                Thief: 2,
                Mage: 3,
                Assist: 4
            }), e("CampType", {
                Unknown: 0,
                Red: 1,
                Green: 2,
                Blue: 3,
                Yellow: 4,
                Purple: 5
            }), e("DamageType", {
                Invalid: 0,
                Attack: 101,
                Skill: 101,
                Continuous: 103,
                LifeLoss: 104,
                Average: 105,
                Kill: 106,
                Rebound: 107,
                Attack_Treat: 201,
                Skill_Treat: 202,
                Real_Treat: 203,
                Miss: 301,
                Immune: 302,
                Crit: 303,
                Vampire: 401,
                Burn: 108,
                Rebound_Burn: 101,
                Dead_Auto: 9999,
                Invincible: 204
            }), e("HpType", {
                Damage: 0,
                Heal: 1
            }), e("SucCondition", {
                KillAll: 0,
                AliveInMax: 1,
                DamageInMax: 2,
                HpInMax: 3,
                SureVictory: 4,
                WaveKillAll: 5
            }), e("BattleResultType", {
                UnKown: 0,
                Win: 1,
                Lose: 2
            }), e("LoseReason", {
                AllDead: 1,
                OverTime: 2
            });
            var o = e("ShapeType", function(e) {
                return e[e.Sector = 2] = "Sector", e[e.Circle = 4] = "Circle", e[e.Box = 8] = "Box", e[e.Point = 16] = "Point", e[e.Polygon = 32] = "Polygon", e[e.Rect = 64] = "Rect", e[e.Rect2 = 128] = "Rect2", e
            }({}));
            e("ShapeName", ((n = {})[o.Point] = "点", n[o.Box] = "矩形", n[o.Circle] = "圆形", n[o.Sector] = "扇形", n[o.Polygon] = "多边形", n)), e("FrontBackType", {
                Front: 0,
                Back: 1
            }), e("StateType", {
                Invalid: 0,
                Born: 1,
                Idle: 2,
                Move: 3,
                Skill: 4,
                Hit: 5,
                Over: 6,
                Dead: 7,
                HitBack: 8,
                Frozen: 9,
                HitThrow: 10
            }), e("ProjectileType", {
                Rebound: 1,
                Throw: 2
            }), e("AABBType", function(e) {
                return e[e.left = 0] = "left", e[e.right = 1] = "right", e[e.top = 2] = "top", e[e.bottom = 3] = "bottom", e
            }({})), e("TeamCenterType", function(e) {
                return e[e.None = 0] = "None", e[e.Single = 1] = "Single", e[e.Left = 2] = "Left", e[e.Right = 3] = "Right", e
            }({})), e("Max_Sence_Dis", 1780), e("DENOMINATOR", 1e4), e("Min_Dis_VALUE", 1);
            t._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/ColorOvertimeModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DUtils.ts", "./EasyP2DModuleBase.ts"], (function(e) {
    var o, r, t, i, n, l, a, c, u;
    return {
        setters: [function(e) {
            o = e.applyDecoratedDescriptor, r = e.inheritsLoose, t = e.initializerDefineProperty, i = e.assertThisInitialized
        }, function(e) {
            n = e.cclegacy, l = e._decorator, a = e.GradientRange
        }, function(e) {
            c = e.calcColorOnCurve
        }, function(e) {
            u = e.EasyP2DModuleBase
        }],
        execute: function() {
            var s, p, d, f, v;
            n._RF.push({}, "9998eXAxN9GuaPa27b/l18U", "ColorOvertimeModule", void 0);
            var y = l.property,
                b = l.ccclass;
            e("ColorOvertimeModule", (s = b("ColorOverTimeModule"), p = y({
                type: a,
                serializable: !0,
                visible: function() {
                    return this.enabled
                }
            }), s((v = o((f = function(e) {
                function o() {
                    var o;
                    return o = e.call(this) || this, t(o, "color", v, i(o)), o.enableOnUpdate = !0, o
                }
                return r(o, e), o.prototype.onUpdate = function(e, o, r) {
                    var t = c(this.color, r);
                    e.color[0] = t.x, e.color[1] = t.y, e.color[2] = t.z, e.color[3] = t.w
                }, o
            }(u)).prototype, "color", [p], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new a
                }
            }), d = f)) || d));
            n._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/Curve.ts", ["./rollupPluginModLoBabelHelpers.js", "cc"], (function(t) {
    var e, i, n, s, r, o, a;
    return {
        setters: [function(t) {
            e = t.applyDecoratedDescriptor, i = t.createClass, n = t.initializerDefineProperty
        }, function(t) {
            s = t.cclegacy, r = t._decorator, o = t.Vec2, a = t.math
        }],
        execute: function() {
            var h, c, u, l, p, f, v, g, _;
            s._RF.push({}, "d688b9j9fpFp5eNks7LwKF0", "Curve", void 0);
            var d = r.property,
                P = r.ccclass;
            t("BezierPosition", (h = P("BezierPosition"), c = d(o), u = d(o), l = d(o), h((v = e((f = function() {
                n(this, "position", v, this), n(this, "handle1", g, this), n(this, "handle2", _, this)
            }).prototype, "position", [c], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), g = e(f.prototype, "handle1", [u], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), _ = e(f.prototype, "handle2", [l], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), p = f)) || p));

            function y(t) {
                return t > -a.EPSILON && t < a.EPSILON
            }
            var m = function(t) {
                    return t[t.Point = 0] = "Point", t[t.Tangent = 1] = "Tangent", t[t.Normal = 2] = "Normal", t[t.Curvature = 3] = "Curvature", t
                }(m || {}),
                T = new o,
                x = (t("Curve", function() {
                    function t() {
                        this.points = [], this._cachePoints = [], this._cacheTimes = [], this._totalDistance = 0, this._cacheDistances = [], this._lengthStart2End = 0, this._curveMode = !0
                    }
                    var e = t.prototype;
                    return e.getRandomPos = function() {
                        if (0 === this._cachePoints.length) return T;
                        var t = Math.random();
                        return this.getPoint(t)
                    }, e.reset = function(t, e, i) {
                        if (void 0 === e && (e = 20), void 0 === i && (i = 100), this._curveMode || (i = (e = 2) * this.points.length * .5), this.points = t, t.length < 2) return this._cachePoints.length = 0, void(this._cacheTimes.length = 0);
                        this._calcCachePoints(e, i)
                    }, e.getTimeByDistance = function(t) {
                        if (0 === this._cacheDistances.length) return 0;
                        var e = this._totalDistance;
                        if (t <= 0) return 0;
                        if (t >= e) return 1;
                        for (var i = this._cacheDistances, n = this._cacheTimes, s = i.length, r = 0, o = 0; o < s; o++) {
                            var a = i[o],
                                h = n[o];
                            if (r + a >= t) return h + (t - r) / a * (1 - h);
                            r += a
                        }
                        return 1
                    }, e.getTangetByDistance = function(t) {
                        var e = this.getTimeByDistance(t);
                        return this.getTangent(e)
                    }, e.getTangent = function(t, e, i) {
                        void 0 === e && (e = !1), void 0 === i && (i = .01);
                        var n = this.getPoint(t),
                            s = n.x,
                            r = n.y,
                            o = 0,
                            a = 0;
                        return e ? t > 0 ? (o = (n = this.getPoint(t - i)).x, a = n.y) : (o = -(n = this.getPoint(i)).x, a = -n.y) : t < 1 ? (o = (n = this.getPoint(t + i)).x, a = n.y) : (o = -(n = this.getPoint(1 - i)).x, a = -n.y), x.set(o - s, a - r), x.normalize()
                    }, e.getPoint = function(t) {
                        if (0 === this._cachePoints.length) return T;
                        for (var e = 0; e < this._cacheTimes.length; e++) {
                            if (this._cacheTimes[e] >= t) {
                                if (0 === e) return this._cachePoints[0];
                                var i = this._cachePoints[e - 1],
                                    n = this._cachePoints[e],
                                    s = this._cacheTimes[e - 1],
                                    r = (t - s) / (this._cacheTimes[e] - s);
                                return x.set(i.x, i.y), x.lerp(n, r)
                            }
                        }
                        return this._cachePoints[this._cachePoints.length - 1]
                    }, e._calcCachePoints = function(t, e) {
                        void 0 === t && (t = 20), void 0 === e && (e = 100);
                        var i = this.points.length - 1,
                            n = 1 / (t - 1);
                        this._cachePoints.length = 0, this._cacheTimes.length = 0;
                        for (var s = [], r = 0, a = 0; a < i; a++) {
                            for (var h = 0, c = D, u = 0, l = 0; l < t; l++) {
                                var p = this.points[a],
                                    f = this.points[a + 1],
                                    v = this.evaluate(p, f, u, m.Point, !1);
                                x.set(v.x, v.y), l > 0 && (h += o.distance(x, c)), c.set(x.x, x.y), (u += n) > 1 && (u = 1)
                            }
                            s.push(h), r += h
                        }
                        this._totalDistance = r, this._cacheDistances = s;
                        var g = this.points[i].position,
                            _ = this.points[0].position;
                        this._lengthStart2End = o.distance(g, _);
                        var d = r / e;
                        n = 1 / e;
                        for (var P = 0, y = 0; y < i; y++)
                            for (var T = s[y], M = this.points[y], b = this.points[y + 1], w = 0, C = 1 / (w = this._curveMode ? Math.max(1, Math.floor(T / d)) : 1), S = 0, k = 0; k < w; k++) {
                                var z = this.evaluate(M, b, S, m.Point, !1).clone();
                                z && (this._cachePoints.push(z), this._cacheTimes.push(P), (P += n) > 1 && (P = 1)), (S += C) > 1 && (S = 1)
                            }
                        this._cachePoints.push(this.points[this.points.length - 1].position), this._cacheTimes.push(1)
                    }, e.evaluate = function(t, e, i, n, s) {
                        if (!this.curveMode) return x.set(e.position.x, e.position.y), x.subtract(t.position).multiplyScalar(i).add(t.position);
                        if (null == i || i < 0 || i > 1) return null;
                        var r = t.position.x,
                            o = t.position.y,
                            h = t.handle2.x + r,
                            c = t.handle2.y + o,
                            u = e.position.x,
                            l = e.position.y,
                            p = e.handle1.x + u,
                            f = e.handle1.y + l;
                        y(t.handle2.x) && y(t.handle2.y) && (h = r, c = o), y(e.handle1.x) && y(e.handle1.y) && (p = u, f = l);
                        var v, g, _ = 3 * (h - r),
                            d = 3 * (p - h) - _,
                            P = u - r - _ - d,
                            T = 3 * (c - o),
                            D = 3 * (f - c) - T,
                            M = l - o - T - D;
                        if (n === m.Point) v = 0 === i ? r : 1 === i ? u : ((P * i + d) * i + _) * i + r, g = 0 === i ? o : 1 === i ? l : ((M * i + D) * i + T) * i + o;
                        else {
                            var b = a.EPSILON,
                                w = 1 - b;
                            if (i < b ? (v = _, g = T) : i > w ? (v = 3 * (u - p), g = 3 * (l - f)) : (v = (3 * P * i + 2 * d) * i + _, g = (3 * M * i + 2 * D) * i + T), s) {
                                0 === v && 0 === g && (i < b || i > w) && (v = p - h, g = f - c);
                                var C = Math.sqrt(v * v + g * g);
                                C && (v /= C, g /= C)
                            }
                            if (n === m.Curvature) {
                                p = 6 * P * i + 2 * d, f = 6 * M * i + 2 * D;
                                var S = Math.pow(v * v + g * g, 1.5);
                                v = 0 !== S ? (v * f - g * p) / S : 0, g = 0
                            }
                        }
                        return n === m.Normal ? x.set(g, -v) : x.set(v, g), x
                    }, i(t, [{
                        key: "totalDistance",
                        get: function() {
                            return this._totalDistance
                        }
                    }, {
                        key: "lengthStart2End",
                        get: function() {
                            return this._lengthStart2End
                        }
                    }, {
                        key: "cacheDistances",
                        get: function() {
                            return this._cacheDistances
                        }
                    }, {
                        key: "cacheTimes",
                        get: function() {
                            return this._cacheTimes
                        }
                    }, {
                        key: "curveMode",
                        get: function() {
                            return this._curveMode
                        },
                        set: function(t) {
                            this._curveMode = t
                        }
                    }]), t
                }()), new o),
                D = new o;
            s._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EasyP2D.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./patch.ts", "./EasyP2DSimulator.ts", "./EasyP2DSystem.ts", "./EasyP2DUtils.ts", "./MeshBatcher.ts", "./MeshBatchData.ts", "./P2DSettings.ts"], (function(t) {
    var e, s, i, r, a, n, h, o, l, u, c, p, d;
    return {
        setters: [function(t) {
            e = t.applyDecoratedDescriptor, s = t.inheritsLoose, i = t.initializerDefineProperty, r = t.assertThisInitialized, a = t.createClass
        }, function(t) {
            n = t.cclegacy, h = t._decorator, o = t.UIRenderer
        }, null, function(t) {
            l = t.EasyP2DSimulator
        }, function(t) {
            u = t.EasyP2DSystem
        }, null, function(t) {
            c = t.MeshBatcher
        }, function(t) {
            p = t.MeshBatchData
        }, function(t) {
            d = t.P2DSettings
        }],
        execute: function() {
            var m, y, _, f, D, v, b, M, R, g, w, S, P;
            n._RF.push({}, "b517bT4pw9NHrelQ9g+P2zt", "EasyP2D", void 0);
            var E = h.ccclass,
                F = h.property,
                k = h.menu,
                I = h.executeInEditMode,
                A = h.playOnFocus;
            t("EasyP2D", (m = E("EasyP2D"), y = k("EasyP2D"), _ = F({
                min: .1,
                max: 10,
                step: .1,
                slide: !0
            }), f = F({
                visible: !0,
                override: !0,
                displayOrder: 0
            }), D = F({
                type: u,
                visible: !1
            }), v = F({
                type: u,
                visible: !0
            }), b = F({
                visible: !1
            }), M = F({
                visible: !1,
                override: !0
            }), m(R = y(R = A(R = I((w = e((g = function(t) {
                s(n, t);
                var e = n.prototype;

                function n() {
                    var e;
                    return (e = t.call(this) || this).QUAD_INDICES = null, e.onCompleted = null, e._swapMeshRenderDatas = [], e._swapMeshRenderDataIndex = 0, e._frameDelay = 2, e.parentMaster = void 0, e.rootMaster = void 0, e.children = [], e._batchData = new p, i(e, "timeScale", w, r(e)), e.delay = 0, i(e, "_system", S, r(e)), i(e, "_initialized", P, r(e)), e.simulator = void 0, e._stopped = !0, e._needPlayOnce = !1, e.simulator = new l(r(e)), e
                }
                return e.updateBatchData = function(t, e) {
                    c.batchProcess(this, t, e)
                }, e.calcParticleCount = function() {
                    for (var t = 0, e = 0; e < this.children.length; e++) {
                        var s = this.children[e];
                        s.simulator && s._system && (t += s._system.particleCount)
                    }
                    return t
                }, e.onLoad = function() {
                    t.prototype.onLoad.call(this), this._batchData.init(this);
                    var e = this.node.getComponentsInChildren(n);
                    this.children.length = 0;
                    for (var s = 0; s < e.length; s++) {
                        var i = e[s];
                        i != this && (i.system._preview = this.system._preview, i.parentMaster = this, i.rootMaster = this.rootMaster || this, this.children.push(i))
                    }
                    this._customMaterial || (this._instanceMaterialType = 1)
                }, e.replay = function() {
                    return this.node.active ? (this._resetSystem(this, !0), this) : (this._needPlayOnce = !0, this.node.active = !0, this)
                }, e.onEnable = function() {
                    t.prototype.onEnable.call(this), this.system.node || (this.system.node = this.node), this._initialized = !0, this.system._syncAspect(), this.initial()
                }, e.initial = function() {
                    this.initSystem()
                }, e.initSimulator = function() {
                    this.simulator || (this.simulator = new l(this))
                }, e.onDestroy = function() {
                    t.prototype.onDestroy.call(this), this.system.autoRemoveOnFinish && (this.system.autoRemoveOnFinish = !1);
                    for (var e = 0; e < this._swapMeshRenderDatas.length; e++) this._assembler.removeData(this._swapMeshRenderDatas[e]);
                    this._swapMeshRenderDatas.length = 0
                }, e.initSystem = function() {
                    this.updateParticle(), (this.system.playOnLoad || this._needPlayOnce) && this.resetSystem()
                }, e.onFocusInEditor = function() {
                    this._focused = !0, this.startPreview()
                }, e.onLostFocusInEditor = function() {
                    this._focused = !1, this._stopPreview()
                }, e.startPreview = function() {
                    this.system._preview && this.resetSystem()
                }, e._stopPreview = function() {
                    this.system._preview && (this.resetSystem(), this.stopSystem())
                }, e.__preload = function() {
                    t.prototype.__preload.call(this), this.system.spriteFrame && !this.system.renderFrame && this.applySpriteFrame()
                }, e._flushAssembler = function() {
                    var t = n.Assembler.getAssembler(this);
                    this._assembler !== t && (this._assembler = t), this._assembler && this._assembler.createData && this._createRenderData()
                }, e._createRenderData = function() {
                    if (this._swapMeshRenderDatas.length >= 2) return this.meshRenderData;
                    var t = this._assembler.createData(this);
                    return t.reset(), t.particleInitRenderDrawInfo(this.renderEntity), t.setRenderDrawInfoAttributes(), this._swapMeshRenderDatas.push(t), t
                }, e.swapBuffer = function() {
                    if (0 != this._swapMeshRenderDatas.length && d.ENABLE_BATCHER && d.ENABLE_SWAP_BATCHER) {
                        var t = !1;
                        if (this._swapMeshRenderDatas.length < 2 && (t = !0, this._createRenderData()), !t) {
                            var e = this.realRenderData,
                                s = this.meshRenderData;
                            e.reset(), e.request(s.vertexCount, s.indexCount), e.vData = new Float32Array(s.vData.buffer), e.iData = new Uint16Array(s.iData.buffer), this._swapMeshRenderDataIndex = 1 - this._swapMeshRenderDataIndex
                        }
                    }
                }, e.getTimeScale = function() {
                    return this.rootMaster ? this.rootMaster.timeScale : this.timeScale
                }, e.lateUpdate = function(t) {
                    this._frameDelay > 0 && this._frameDelay--, this.simulator && !this.simulator.finished && this.simulator.step(t * this.getTimeScale())
                }, e.stopSystem = function() {
                    if (this._stopped = !0, this._system.willStop = !0, !this.parentMaster)
                        for (var t = 0; t < this.children.length; ++t) {
                            this.children[t] !== this && this.children[t].stopSystem()
                        }
                }, e.resetSystem = function() {
                    this._resetSystem(null, !0)
                }, e._resetSystem = function(t, e) {
                    if (this.simulator && this.system) {
                        if (this._needPlayOnce = !1, this.delay = this.system.realDelay, this.initSimulator(), this.system.reset(), this.markForUpdateRenderData(), !t && !this.parentMaster || t == this)
                            for (var s = this.children, i = 0; i < s.length; ++i) {
                                var r = s[i];
                                r !== this && r._resetSystem(null, !1)
                            }
                        this.simulator.reset(), this._stopped = !1
                    }
                }, e.applySpriteFrame = function() {
                    var t = this.system;
                    t.renderFrame && t.renderFrame.texture ? (this.simulator && t.useAnimation && this._assembler.updateUVs(this), t._syncAspect(), this._updateMaterial(), this._stopped = !1, this.markForUpdateRenderData()) : this.resetSystem()
                }, e.getRenderMaterial = function(t) {
                    return 0 == this._swapMeshRenderDatas.length ? null : this.meshRenderData.material || this._materialInstances[t] || this._materials[t]
                }, e._finishedSimulation = function() {
                    this.resetSystem(), this.stopSystem(), this.markForUpdateRenderData(), this.system.autoRemoveOnFinish && this._stopped && this.node.destroy()
                }, e.updateParticle = function() {
                    this._updateMaterial(), this._updatePositionType()
                }, e._canRender = function() {
                    if (this._frameDelay > 0) return !1;
                    var e = this.system.renderFrame;
                    return t.prototype._canRender.call(this) && !this._stopped && null != e && this.batchData.vertexCount > 0 && (this.batchData.renderData == this.meshRenderData || this.batchData.renderData == this.realRenderData)
                }, e._render = function(t) {
                    var e = this.system;
                    0 != this.particleCount && t.commitComp(this, this.meshRenderData, e.renderFrame, this._assembler, null)
                }, e._updatePositionType = function() {}, e._updateMaterial = function() {}, a(n, [{
                    key: "meshRenderData",
                    get: function() {
                        return this._swapMeshRenderDatas[this._swapMeshRenderDataIndex]
                    }
                }, {
                    key: "realRenderData",
                    get: function() {
                        return this._swapMeshRenderDatas[1 - this._swapMeshRenderDataIndex]
                    }
                }, {
                    key: "batchData",
                    get: function() {
                        return this._batchData
                    }
                }, {
                    key: "isAllFinished",
                    get: function() {
                        for (var t = !0, e = 0; e < this.children.length; e++) {
                            var s = this.children[e];
                            if (s.simulator ? t && (t = s.simulator.finished) : t = !1, !t) break
                        }
                        return t
                    }
                }, {
                    key: "customMaterial",
                    get: function() {
                        return this._customMaterial
                    },
                    set: function(t) {
                        if (this._customMaterial != t && (this._customMaterial = t, t && this._swapMeshRenderDatas.length > 0 && this._assembler.updateMaterial))
                            for (var e = 0; e < this._swapMeshRenderDatas.length; e++) this._assembler.updateMaterial(this._swapMeshRenderDatas[e], t)
                    }
                }, {
                    key: "system",
                    get: function() {
                        return this._system || (this._system = new u, this._system.node = this.node), this._system.initial(this), this._system
                    }
                }, {
                    key: "particleCount",
                    get: function() {
                        return this.simulator.particles.length
                    }
                }, {
                    key: "stopped",
                    get: function() {
                        return this._stopped
                    }
                }, {
                    key: "active",
                    get: function() {
                        return this.simulator.active
                    }
                }, {
                    key: "assembler",
                    get: function() {
                        return this._assembler
                    }
                }, {
                    key: "color",
                    get: function() {
                        return this._color
                    },
                    set: function(t) {}
                }, {
                    key: "renderTransform",
                    get: function() {
                        return this._renderEntity._renderTransform
                    }
                }]), n
            }(o)).prototype, "timeScale", [_], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 1
                }
            }), e(g.prototype, "customMaterial", [f], Object.getOwnPropertyDescriptor(g.prototype, "customMaterial"), g.prototype), S = e(g.prototype, "_system", [D], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), e(g.prototype, "system", [v], Object.getOwnPropertyDescriptor(g.prototype, "system"), g.prototype), P = e(g.prototype, "_initialized", [b], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), e(g.prototype, "color", [M], Object.getOwnPropertyDescriptor(g.prototype, "color"), g.prototype), R = g)) || R) || R) || R) || R));
            n._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EasyP2DAssembler.ts", ["cc", "./patch.ts", "./EasyP2D.ts", "./EasyP2DDefine.ts", "./EasyP2DUtils.ts", "./P2DSettings.ts"], (function(t) {
    var e, a, r, n, s, i, o, l, f, c, u, y, m;
    return {
        setters: [function(t) {
            e = t.cclegacy, a = t.Vec3, r = t.math, n = t.Mat4, s = t.MeshRenderData, i = t.game, o = t.UIVertexFormat
        }, function(t) {
            l = t.UIVertexFormatX
        }, function(t) {
            f = t.EasyP2D
        }, function(t) {
            c = t.PositionType
        }, function(t) {
            u = t.calcCos, y = t.calcSin
        }, function(t) {
            m = t.P2DSettings
        }],
        execute: function() {
            e._RF.push({}, "65e6bX30SRDoIXvIhLyONXl", "EasyP2DAssembler", void 0);
            var D = o || l,
                v = new a,
                d = (new a, {
                    x: 0,
                    y: 0
                }),
                x = (new r.Quat, new n, D.getComponentPerVertex(D.vfmtPosUvColor)),
                P = t("EasyP2DAssembler", {
                    maxParticleDeltaTime: 0,
                    requestData: function(t) {
                        var e = 0,
                            a = !0;
                        t.system.trailEnabled && (a = t.system.trail.showMainParticle);
                        for (var r = 0; r < t.particleCount; r++) {
                            if (t.system.trailEnabled) {
                                var n = t.simulator.particles[r];
                                if (n.trail) e += n.trail.particleCount
                            }
                            a && e++
                        }
                        return this._requestData(t, 4 * e, 6 * e), !0
                    },
                    createData: function(t) {
                        return s.add()
                    },
                    removeData: function(t) {
                        s.remove(t)
                    },
                    updateRenderData: function() {},
                    fillBuffers: function(t, e) {},
                    _requestData: function(t, e, a) {
                        t.updateBatchData(e, a);
                        var r = t.batchData,
                            n = r.renderData;
                        if (n) {
                            for (var s = r.startIndexIndex, i = r.startVertexIndex, o = e / 4, l = n.iData, f = 0; f < o; f++) {
                                var c = 4 * f + i;
                                l[s++] = c, l[s++] = c + 1, l[s++] = c + 2, l[s++] = c + 1, l[s++] = c + 3, l[s++] = c + 2
                            }
                            n.setRenderDrawInfoAttributes()
                        }
                    },
                    updateUVs: function(t) {},
                    updateTrailBuffer: function(t, e, a) {
                        var r = t.batchData;
                        if (r.renderData && e.trail) {
                            var n = r.renderData.vData,
                                s = e.trail,
                                i = s.particleCount,
                                o = t.system.easyP2D.node.worldMatrix;
                            if (o) {
                                a += r.startVertexIndex * x;
                                for (var l = 0; l < i; l++)
                                    for (var f = s.particles[l], u = a + 4 * l * x, y = 0; y < 4; y++) {
                                        var D = u + y * x,
                                            v = f.verticles[y];
                                        n[D] = v.x, n[D + 1] = v.y, n[D + 2] = v.z;
                                        var d = f.uvs[y];
                                        n[D + 3] = d.x, n[D + 4] = d.y;
                                        var P = f.colors[y];
                                        n[D + 5] = P.x, n[D + 6] = P.y, n[D + 7] = P.z, n[D + 8] = P.w, !m.SUPPORT_PARTICLE_TRANSFORM || m.FIX_FREE_MODE_ROTATION && t.system.positionType === c.FREE || p(t, n, D, o)
                                    }
                            } else console.warn("updateTrailBuffer: worldMatrix is null or undefined")
                        }
                    },
                    updateParticleBuffer: function(t, e, a) {
                        var r = t.batchData;
                        if (r.renderData) {
                            var n = r.renderData.vData,
                                s = d,
                                i = t.system;
                            s.x = e.finalPos.x, s.y = e.finalPos.y, a += r.startVertexIndex * x;
                            var o = e.uv;
                            n[a + 3] = o[0], n[a + 4] = o[1], n[a + 12] = o[2], n[a + 13] = o[3], n[a + 21] = o[4], n[a + 22] = o[5], n[a + 30] = o[6], n[a + 31] = o[7], s.x += e.startPos.x, s.y += e.startPos.y;
                            var l = s.x,
                                f = s.y,
                                D = 2 * e.halfWidth,
                                v = 2 * e.halfHeight,
                                P = e.sys.anchor.x * D,
                                E = e.sys.anchor.y * v,
                                T = D - P,
                                h = v - E,
                                I = e.rotation + e.startRotation;
                            if (0 !== I) {
                                var _ = -P,
                                    A = -E,
                                    F = T,
                                    O = h,
                                    b = I,
                                    w = u(b),
                                    M = y(b);
                                n[a] = _ * w - A * M + l, n[a + 1] = _ * M + A * w + f, n[a + 2] = 0, n[a + 9] = F * w - A * M + l, n[a + 10] = F * M + A * w + f, n[a + 11] = 0, n[a + 18] = _ * w - O * M + l, n[a + 19] = _ * M + O * w + f, n[a + 20] = 0, n[a + 27] = F * w - O * M + l, n[a + 28] = F * M + O * w + f, n[a + 29] = 0
                            } else n[a + 0] = n[a + 18] = l - P, n[a + 1] = n[a + 10] = f - E, n[a + 9] = n[a + 27] = l + T, n[a + 19] = n[a + 28] = f + h, n[a + 2] = n[a + 11] = n[a + 8] = n[a + 29] = 0;
                            if (this.toArrays(n, e.color, a + 5, a + 14, a + 23, a + 32), m.SUPPORT_PARTICLE_TRANSFORM) {
                                var S = t.system,
                                    g = S.easyP2D.node.worldMatrix;
                                if (g)
                                    if (m.FIX_FREE_MODE_ROTATION && S.positionType === c.FREE) {
                                        var C = function(t, e) {
                                            var a = e;
                                            if (void 0 !== a.m12 && void 0 !== a.m13) return {
                                                x: a.m12,
                                                y: a.m13
                                            };
                                            if (a.m && a.m.length >= 14) return {
                                                x: a.m[12],
                                                y: a.m[13]
                                            };
                                            var r = t.worldPosition;
                                            return {
                                                x: r.x,
                                                y: r.y
                                            }
                                        }(S.easyP2D.node, g);
                                        R(n, a, C.x, C.y)
                                    } else p(t, n, a, g), p(t, n, a + 9, g), p(t, n, a + 18, g), p(t, n, a + 27, g)
                            } else if (m.FIX_FREE_MODE_ROTATION && i.positionType === c.FREE);
                            else {
                                var V = t.node.worldPosition;
                                R(n, a, V.x, V.y)
                            }
                        }
                    },
                    toArray: function(t, e, a) {
                        t[a] = e[0], t[a + 1] = e[1], t[a + 2] = e[2], t[a + 3] = e[3]
                    },
                    toArrays: function(t, e, a, r, n, s) {
                        t[a] = t[r] = t[n] = t[s] = e[0], t[a + 1] = t[r + 1] = t[n + 1] = t[s + 1] = e[1], t[a + 2] = t[r + 2] = t[n + 2] = t[s + 2] = e[2], t[a + 3] = t[r + 3] = t[n + 3] = t[s + 3] = e[3]
                    }
                }),
                E = t("EasyP2DSystemAssembler", {
                    getAssembler: function(t) {
                        return P.maxParticleDeltaTime || (P.maxParticleDeltaTime = i.frameTime / 1e3 * 2), P
                    }
                });

            function R(t, e, a, r) {
                t[e] += a, t[e + 1] += r, t[e + 9] += a, t[e + 10] += r, t[e + 18] += a, t[e + 19] += r, t[e + 27] += a, t[e + 28] += r
            }

            function p(t, e, r, n) {
                v.x = e[r], v.y = e[r + 1], v.z = e[r + 2], a.transformMat4(v, v, n), e[r] = v.x, e[r + 1] = v.y, e[r + 2] = 0
            }
            f.Assembler = E, e._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EasyP2DDefine.ts", ["cc"], (function(e) {
    var i, t;
    return {
        setters: [function(e) {
            i = e.cclegacy, t = e.Enum
        }],
        execute: function() {
            i._RF.push({}, "ec455bEDwFF/7iqXqFFIldL", "EasyP2DDefine", void 0);
            e("ImageFormat", function(e) {
                return e[e.JPG = 0] = "JPG", e[e.PNG = 1] = "PNG", e[e.TIFF = 2] = "TIFF", e[e.WEBP = 3] = "WEBP", e[e.PVR = 4] = "PVR", e[e.ETC = 5] = "ETC", e[e.S3TC = 6] = "S3TC", e[e.ATITC = 7] = "ATITC", e[e.TGA = 8] = "TGA", e[e.RAWDATA = 9] = "RAWDATA", e[e.UNKNOWN = 10] = "UNKNOWN", e
            }({})), e("ExpansionType", t({
                None: 0,
                Random: 1,
                Out: 2,
                In: 3
            })), e("Clockwise", t({
                None: 0,
                Clockwise: 1,
                AntiClockwise: 2
            })), e("DURATION_INFINITY", -1), e("START_SIZE_EQUAL_TO_END_SIZE", -1), e("START_RADIUS_EQUAL_TO_END_RADIUS", -1), e("EmitterShape", t({
                NONE: 0,
                BOX: 1,
                CIRCLE: 2,
                CUSTOM: 3
            })), e("EmitterMode", t({
                NONE: 0,
                GRAVITY: 1,
                TARGET: 2
            })), e("SpaceMode", t({
                Local: 0,
                World: 1
            })), e("PositionType", t({
                FREE: 0,
                RELATIVE: 1,
                GROUPED: 2
            })), e("VelocityType", t({
                StartDirection: 0,
                FireDirection: 1
            })), e("DirectionType", t({
                None: 0,
                MoveDir: 1,
                FireDir: 2
            })), e("ValueType", t({
                Constant: 0,
                Curve: 1,
                TwoCurves: 2,
                TwoConstants: 3
            })), e("PathEmitMode", t({
                Random: 0,
                ByTime: 1
            })), e("AccelRelationDirection", t({
                FireDir: 0,
                MoveDir: 1,
                CenterDir: 2
            }));
            i._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EasyP2DModuleBase.ts", ["./rollupPluginModLoBabelHelpers.js", "cc"], (function(e) {
    var t, n, i, o, a;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, n = e.createClass, i = e.initializerDefineProperty
        }, function(e) {
            o = e.cclegacy, a = e._decorator
        }],
        execute: function() {
            var r, s, l, c, u, p;
            o._RF.push({}, "3230a85UGpGm6KQKF12Iher", "EasyP2DModuleBase", void 0);
            var b = a.ccclass,
                f = a.property;
            e("EasyP2DModuleBase", (r = b("EasyP2DModuleBase"), s = f({
                serializable: !0,
                visible: !1
            }), l = f({
                visible: !0
            }), r((p = t((u = function() {
                function e() {
                    this._easyP2D = null, i(this, "_enabled", p, this), this.enableOnEmitter = !1, this.enableOnUpdate = !1, this.enableAfterEmitter = !1, this.enableAfterUpdate = !1, this.enableOnEnd = !1
                }
                var t = e.prototype;
                return t.bind = function(e) {
                    this._easyP2D = e, this.onInit()
                }, t.onInit = function() {}, t.onReset = function() {}, t.onEanble = function() {}, t.onDisable = function() {}, t.onEmitter = function(e, t) {}, t.afterEmitter = function(e, t) {}, t.onUpdate = function(e, t, n) {}, t.afterUpdate = function(e, t, n, i) {}, t.onEnd = function(e) {}, n(e, [{
                    key: "enabled",
                    get: function() {
                        return this._enabled
                    },
                    set: function(e) {
                        this._enabled !== e && (this._enabled = e, e ? this.onEanble() : this.onDisable())
                    }
                }]), e
            }()).prototype, "_enabled", [s], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), t(u.prototype, "enabled", [l], Object.getOwnPropertyDescriptor(u.prototype, "enabled"), u.prototype), c = u)) || c));
            o._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EasyP2DSimulator.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./patch.ts", "./EasyP2DDefine.ts", "./EasyP2DUtils.ts", "./Particle.ts"], (function(e) {
    var t, i, r, a, s, o, n, l, c, p, y, m, d, u, h, f, v, x, P, D, T;
    return {
        setters: [function(e) {
            t = e.inheritsLoose, i = e.createClass
        }, function(e) {
            r = e.cclegacy, a = e.Vec3, s = e.Vec2, o = e.math, n = e.js, l = e.UIVertexFormat
        }, function(e) {
            c = e.UIVertexFormatX
        }, function(e) {
            p = e.VelocityType, y = e.EmitterMode, m = e.PositionType, d = e.DirectionType, u = e.AccelRelationDirection
        }, function(e) {
            h = e.calcCos, f = e.calcSin, v = e.calcValueOnCurve, x = e.getRangePos, P = e.calcColorOnCurve, D = e.normalizeVec2
        }, function(e) {
            T = e.Particle
        }],
        execute: function() {
            r._RF.push({}, "e98acU5lwtAPoGFetIlLpKv", "EasyP2DSimulator", void 0);
            var g = l || c,
                R = new a,
                w = (new s, {
                    x: 0,
                    y: 0
                }),
                E = {
                    x: 0,
                    y: 0
                },
                S = {
                    x: 0,
                    y: 0
                },
                C = {
                    x: 0,
                    y: 0
                },
                A = (new o.Quat, g.getComponentPerVertex(g.vfmtPosUvColor)),
                M = new(function(e) {
                    function i() {
                        return e.apply(this, arguments) || this
                    }
                    return t(i, e), i.prototype.get = function() {
                        return this._get() || new T
                    }, i
                }(n.Pool))((function(e) {}), 4096);
            e("EasyP2DSimulator", function() {
                function e(e) {
                    this.particles = [], this.active = !1, this.finished = !1, this.system = null, this.stepIndex = 0, this._warming = !1, this.render = e, this.system = e.system, this.particles = [], this.active = !1, this.finished = !1
                }
                var t = e.prototype;
                return t.stop = function() {
                    this.active = !1, this.finished = !0
                }, t.reset = function() {
                    this.system = this.render.system, this.active = !0, this.finished = !1;
                    for (var e = this.particles, t = 0; t < e.length; ++t) M.put(e[t]);
                    e.length = 0
                }, t.warm = function(e) {
                    if (!this.render.rootMaster) {
                        this._warm(e);
                        for (var t = 0; t < this.render.children.length; t++) {
                            var i = this.render.children[t];
                            i.simulator && i.simulator.active && i.simulator._warm(e)
                        }
                    }
                }, t._warm = function(e) {
                    this._warming = !0, this.step(e), this._warming = !1
                }, t.updateDir = function(e, t) {
                    void 0 === t && (t = !1);
                    var i = e.sys,
                        r = 0;
                    e.preventDefaultSetDir || t && (r = i.velocityType == p.FireDirection || 0 == e.pos.x && 0 == e.pos.y ? e.fireRotation : Math.atan2(e.pos.y - i.sourcePos.x, e.pos.x - i.sourcePos.y), e.fireRotation = r, e.startDir.x = e.dir.x = h(r), e.startDir.y = e.dir.y = f(r))
                }, t.emitParticle = function(e, t) {
                    var i = t,
                        r = 0;
                    r = t.realEmitTurnDuration > 0 ? i.elapsed % t.realEmitTurnDuration / t.realEmitTurnDuration : i.particleCount % (t.emissionRate + 1) / t.emissionRate;
                    var a = M.get();
                    if (a.beforeEmit(t), a.timeToLive = v(t.startLifeTime, r), a.totalTime = a.timeToLive = Math.max(.05, a.timeToLive), t.emitterMode == y.TARGET && (a.targetTimeRate = v(t.targetTimeRate, r)), i.positionType === m.GROUPED ? (a.startPos.x = 0, a.startPos.y = 0) : (a.startPos.x = e.x, a.startPos.y = e.y), i.onEmitter(a, r), !a.preventDefaultSetPos) {
                        var s = x(i, i.sourcePos);
                        a.pos.x = s.x, a.pos.y = s.y
                    }
                    a.fromPos.x = a.pos.x, a.fromPos.y = a.pos.y;
                    var n = P(i.startColor, r);
                    a.color[0] = n.x, a.color[1] = n.y, a.color[2] = n.z, a.color[3] = n.w;
                    var l, c;
                    l = v(i.startScaleX, r), c = v(i.startScaleY, r), a.size = a.startSize = v(i.startSize, r), a.halfWidth = a.size * i.halfwidth * l, a.halfHeight = a.size * i.halfheight * c, a.preventDefaultSetSpeed || (a.speed = v(i.startSpeed, r)), a.startSpeed = a.speed;
                    var p = 0;
                    if (!a.preventDefaultSetDir)
                        if (i.emitterMode === y.GRAVITY) {
                            var u = v(i.fireAngle, r);
                            p = o.toRadian(u), a.dir.x = h(p), a.dir.y = f(p)
                        } else {
                            var D = Math.atan2(a.pos.y - i.sourcePos.y, a.pos.x - i.sourcePos.x);
                            a.dir.x = h(D), a.dir.y = f(D)
                        } var T = v(i.startAngle, r);
                    return a.startRotation = o.toRadian(T), i.emitterMode === y.GRAVITY && (a.gravity.x = i.gravity.x, a.gravity.y = i.gravity.y, a.radialAccel = v(i.radialAccel, r), a.tangentialAccel = v(i.tangentialAccel, r), i.rotationType == d.FireDir ? a.rotation = p : a.rotation = 0, a.fireRotation = p), this.updateDir(a, !0), i.particleCount++, this.particles.push(a), i.afterEmitter(a, r), a
                }, t.step = function(e) {
                    if (this.renderData && this.system) {
                        var t = this.system,
                            i = this.particles,
                            r = this.render.assembler;
                        this._warming || (e = e > r.maxParticleDeltaTime ? r.maxParticleDeltaTime : e), t && -1 !== t.realDuration && t.realDuration < t.elapsed && (t.willStop = !0);
                        var a = this.system;
                        if (a.willStop) {
                            if (0 == a.particleCount) {
                                a.active = !1, a.willStop = !1, this.finished = !0, a.elapsed = 0;
                                var s = a.easyP2D.parentMaster || a.easyP2D;
                                if (s && s.isAllFinished && 0 == s.calcParticleCount()) {
                                    var n = s.onCompleted;
                                    n && n(s), s._finishedSimulation()
                                }
                            }
                        } else if (a.active) {
                            var l = a.realEmitTurnDuration >= 0 ? a.realEmitTurnDuration : 1;
                            if (a.easyP2D.delay > 0) a.easyP2D.delay -= e;
                            else {
                                var c = a.node;
                                if (a.positionType === m.FREE ? (c.getWorldPosition(R), w.x = R.x, w.y = R.y) : a.positionType === m.RELATIVE ? (c.getPosition(R), w.x = R.x, w.y = R.y) : w.x = w.y = 0, a.sleepTimer <= 0) {
                                    var p = l / a.emissionRate;
                                    if (0 == p)
                                        for (var h = a.emissionRate; a.particleCount < a._totalParticles && h-- > 0;) this.emitParticle(w, a);
                                    else {
                                        for (0 == a.elapsed ? a.emitCounter = p : a.emitCounter += e; a.particleCount < a._totalParticles && a.emitCounter >= p;) this.emitParticle(w, a), a.emitCounter -= p;
                                        a.emitCounter > p && (a.emitCounter = a.emitCounter % p + p)
                                    }
                                }
                                a.elapsed += e, a.enableSwapTime && (a.sleepTimer > 0 ? (a.sleepTimer -= e, a.sleepTimer <= 0 && (a.sleepTimer = 0, a.awakeTimer = a.awakeTime)) : (a.awakeTimer <= 0 && (a.awakeTimer = a.awakeTime), a.awakeTimer -= e, a.awakeTimer <= 0 && (a.sleepTimer = a.sleepTime)))
                            }
                        }
                        for (this.stepIndex = 0; this.stepIndex < i.length;) {
                            var f = i[this.stepIndex],
                                v = o.clamp01(1 - f.timeToLive / f.totalTime);
                            w.x = f.pos.x, w.y = f.pos.y;
                            var x = f.sys;
                            if (f.preventDefaultSetPos = !1, f.preventDefaultSetDir = !1, x.onUpdate(f, e, v), f.preventDefaultSetSpeed || this.updateDir(f), !f.preventDefaultSetPos)
                                if (x.emitterMode === y.GRAVITY) {
                                    var P = C,
                                        T = E;
                                    a.accelRelativeDir == u.MoveDir ? (T.x = f.dir.x, T.y = f.dir.y) : a.accelRelativeDir == u.CenterDir ? (T.x = f.pos.x, T.y = f.pos.y) : a.accelRelativeDir == u.FireDir && (T.x = f.startDir.x, T.y = f.startDir.y), D(T), P.x = (T.x * f.radialAccel - T.y * f.tangentialAccel + f.gravity.x) * e, P.y = (T.y * f.radialAccel + T.x * f.tangentialAccel + f.gravity.y) * e, f.dir.x += P.x, f.dir.y += P.y, f.pos.x += e * f.dir.x * f.speed, f.pos.y += e * f.dir.y * f.speed, C.x = f.dir.x, C.y = f.dir.y, D(C), f.dir.x = C.x, f.dir.y = C.y
                                } else if (x.emitterMode === y.TARGET) {
                                var g = f.targetTimeRate <= 0 ? 1 : v / f.targetTimeRate;
                                g <= 1 && (f.pos.x = o.lerp(f.fromPos.x, f.targetPos.x, g), f.pos.y = o.lerp(f.fromPos.y, f.targetPos.y, g))
                            }
                            if (f.finalPos.x = f.pos.x, f.finalPos.y = f.pos.y, this.updateUVs(f), x.afterUpdate(f, e, v, w), x.rotationType == d.MoveDir && (S.x = f.finalPos.x - w.x, S.y = f.finalPos.y - w.y, 0 == S.x && 0 == S.y || (f.rotation = Math.atan2(S.y, S.x) + f.startRotation)), x.positionType == m.FREE) {
                                var V = x.easyP2D.node.worldPosition;
                                f.finalPos.x -= V.x, f.finalPos.y -= V.y
                            } else if (x.positionType == m.RELATIVE) {
                                var F = x.easyP2D.node.position;
                                f.finalPos.x -= F.x, f.finalPos.y -= F.y
                            }
                            f.timeToLive -= e, !a.active || f.timeToLive <= 0 ? (a.onEnd(f), this.stepIndex != i.length - 1 && (i[this.stepIndex] = i[i.length - 1]), i.length--, a.particleCount--, M.put(f)) : this.stepIndex++
                        }
                        this.render.assembler.requestData(this.render);
                        for (var I = 0, _ = 0; _ < i.length; _++) {
                            var U = i[_],
                                L = !0;
                            a.trailEnabled && (this.render.assembler.updateTrailBuffer(this.render, U, I), I += 4 * a.trail.getParticleCount(U) * A, L = a.trail.showMainParticle), L && (this.render.assembler.updateParticleBuffer(this.render, U, I), I += 4 * A)
                        }!this.renderData.material && (this.renderData.material = this.render.getRenderMaterial(0)), !this.renderData.frame && (this.renderData.frame = t.renderFrame), this.render.markForUpdateRenderData(!0)
                    }
                }, t.updateUVs = function(e) {
                    var t = e.sys,
                        i = e.uv;
                    if (t.needReplaceUVs) {
                        var r = e.tiles.x,
                            a = e.tiles.y,
                            s = e.currentFrame % r,
                            o = Math.floor(e.currentFrame / r),
                            n = t.replaceUVs,
                            l = n[0],
                            c = n[1],
                            p = n[2],
                            y = n[3],
                            m = n[4],
                            d = n[5],
                            u = n[6],
                            h = n[7];
                        i[0] = l + (s + n[8]) * c / r, i[1] = m + (o + n[9]) * d / a, i[2] = p + (s + n[10]) * y / r, i[3] = u + (o + n[11]) * h / a, i[4] = p + (s + n[12]) * y / r, i[5] = u + (o + n[13]) * h / a, i[6] = l + (s + n[14]) * c / r, i[7] = m + (o + n[15]) * d / a
                    } else {
                        var f = e.sys.uv;
                        i[0] = f[0], i[1] = f[1], i[2] = f[2], i[3] = f[3], i[4] = f[4], i[5] = f[5], i[6] = f[6], i[7] = f[7]
                    }
                }, i(e, [{
                    key: "renderData",
                    get: function() {
                        return this.render.meshRenderData
                    }
                }, {
                    key: "warming",
                    get: function() {
                        return this._warming
                    }
                }]), e
            }());
            r._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EasyP2DSystem.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DDefine.ts", "./EasyP2DUtils.ts", "./ColorOvertimeModule.ts", "./SizeOvertimeModule.ts", "./AnimationModule.ts", "./AngleOvertimeModule.ts", "./ImageEmitterModule.ts", "./PathEmitterModule.ts", "./SystemAngleOvertimeModule.ts", "./SpeedOvertimeModule.ts", "./TimerModule.ts", "./PathOvertimeModule.ts", "./TrailModule.ts"], (function(e) {
    var t, i, r, n, o, a, s, l, u, p, h, m, c, b, y, g, f, d, v, _, O, w;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, i = e.createClass, r = e.initializerDefineProperty
        }, function(e) {
            n = e.cclegacy, o = e._decorator, a = e.Enum, s = e.SpriteFrame, l = e.CCInteger, u = e.Vec2, p = e.CurveRange, h = e.GradientRange
        }, function(e) {
            m = e.DirectionType, c = e.EmitterMode, b = e.EmitterShape, y = e.PositionType, g = e.VelocityType, f = e.AccelRelationDirection
        }, function(e) {
            d = e.normalizeVec2
        }, null, null, null, null, function(e) {
            v = e.ImageEmitterModule
        }, function(e) {
            _ = e.PathEmitterModule
        }, null, null, null, function(e) {
            O = e.PathOvertimeModule
        }, function(e) {
            w = e.TrailModule
        }],
        execute: function() {
            var z, E, T, A, P, D, M, S, R, k, F, C, j, L, V, I, G, U, Y, x, N, Z, X, B, H, q, J, K, Q, W, $, ee, te, ie, re, ne, oe, ae, se, le, ue, pe, he, me, ce, be, ye, ge, fe, de, ve, _e, Oe, we, ze, Ee, Te, Ae, Pe, De, Me, Se, Re, ke, Fe, Ce, je, Le, Ve, Ie, Ge, Ue, Ye, xe, Ne, Ze, Xe, Be, He, qe, Je, Ke, Qe, We, $e, et, tt, it, rt, nt, ot, at, st, lt, ut, pt, ht, mt, ct;
            n._RF.push({}, "d98ceMIHF9Ib7qaJ3Z/9E1S", "EasyP2DSystem", void 0);
            var bt = o.ccclass,
                yt = o.property,
                gt = {
                    x: 0,
                    y: 0
                },
                ft = a({
                    Life: "Life",
                    Color: "Color",
                    Angle: "Angle",
                    Size: "Size",
                    Speed: "Speed",
                    Emitter: "Emitter",
                    Animation: "Animation",
                    Trail: "Trail"
                });
            e("EasyP2DSystem", (z = bt("EasyP2DSystem"), E = yt({
                type: s
            }), T = yt({
                visible: !0,
                displayOrder: 0
            }), A = yt({
                group: ft.Life,
                visible: function() {
                    return !this._timer || !this._timer.enabled
                }
            }), P = yt({
                group: ft.Life,
                visible: function() {
                    return !this._timer || !this._timer.enabled
                }
            }), D = yt({
                type: l,
                group: ft.Life
            }), M = yt({
                group: ft.Life,
                visible: function() {
                    return !this._timer || !this._timer.enabled
                }
            }), S = yt({
                type: l,
                group: ft.Life
            }), R = yt({
                group: ft.Life
            }), k = yt({
                visible: !1,
                serializable: !0
            }), F = yt({
                group: ft.Life,
                visible: !0
            }), C = yt({
                group: ft.Color
            }), j = yt({
                group: ft.Color,
                visible: !1,
                serializable: !0
            }), L = yt({
                group: ft.Color,
                visible: !0
            }), V = yt({
                group: ft.Angle,
                range: [-360, 360]
            }), I = yt({
                group: ft.Angle,
                range: [-360, 360]
            }), G = yt({
                type: m,
                group: ft.Angle,
                visible: function() {
                    return this.emitterMode !== c.NONE
                }
            }), U = yt({
                visible: !1,
                serializable: !0
            }), Y = yt({
                group: ft.Angle,
                visible: !0
            }), x = yt({
                visible: !1,
                serializable: !0
            }), N = yt({
                group: ft.Angle,
                visible: function() {
                    return this.emitterMode !== c.TARGET
                }
            }), Z = yt({
                group: ft.Size
            }), X = yt({
                group: ft.Size
            }), B = yt({
                group: ft.Size
            }), H = yt({
                visible: !1,
                serializable: !0
            }), q = yt({
                group: ft.Size,
                visible: !0
            }), J = yt({
                group: ft.Emitter,
                type: a(b)
            }), K = yt({
                group: ft.Emitter
            }), Q = yt({
                group: ft.Emitter,
                visible: function() {
                    return this.shapeMode !== b.NONE
                }
            }), W = yt({
                group: ft.Emitter,
                visible: function() {
                    return this.shapeMode !== b.NONE
                }
            }), $ = yt({
                group: ft.Emitter,
                type: a(y)
            }), ee = yt({
                type: a(c),
                group: ft.Emitter
            }), te = yt({
                visible: function() {
                    return this.emitterMode === c.TARGET
                },
                group: ft.Emitter,
                slide: !0,
                range: [0, 1]
            }), ie = yt({
                visible: function() {
                    return this.emitterMode === c.GRAVITY
                },
                group: ft.Speed
            }), re = yt({
                visible: function() {
                    return this.emitterMode === c.GRAVITY || this.emitterMode === c.TARGET
                },
                group: ft.Speed
            }), ne = yt({
                type: g,
                visible: function() {
                    return this.emitterMode === c.GRAVITY
                },
                group: ft.Speed
            }), oe = yt({
                visible: function() {
                    return this.emitterMode === c.GRAVITY
                },
                group: ft.Speed
            }), ae = yt({
                visible: function() {
                    return this.emitterMode === c.GRAVITY
                },
                group: ft.Speed
            }), se = yt({
                type: f,
                visible: function() {
                    return this.emitterMode === c.GRAVITY
                },
                group: ft.Speed
            }), le = yt({
                visible: !1,
                serializable: !0
            }), ue = yt({
                group: ft.Speed,
                visible: !0
            }), pe = yt({
                type: v,
                visible: !1,
                serializable: !0
            }), he = yt({
                visible: !0,
                group: ft.Emitter
            }), me = yt({
                type: _,
                visible: !1,
                serializable: !0
            }), ce = yt({
                visible: !0,
                group: ft.Emitter
            }), be = yt({
                visible: !1,
                serializable: !0
            }), ye = yt({
                visible: !0,
                group: ft.Animation
            }), ge = yt({
                type: O,
                visible: !1,
                serializable: !0
            }), fe = yt({
                visible: !0,
                group: ft.Animation
            }), de = yt({
                type: w,
                visible: !1,
                serializable: !0
            }), ve = yt({
                visible: !0,
                group: ft.Trail
            }), _e = yt({
                visible: !1
            }), Oe = yt({
                visible: !1
            }), we = yt({
                visible: !1
            }), ze = yt({
                visible: !1
            }), z((t((Te = function() {
                var e = t.prototype;

                function t() {
                    this.generatePos = null, this._easyP2D = null, this.node = void 0, this._modules = [], r(this, "_anchor", Ae, this), r(this, "delay", Pe, this), r(this, "duration", De, this), r(this, "emissionRate", Me, this), r(this, "emitTurnDuration", Se, this), r(this, "startLifeTime", Re, this), r(this, "_timer", ke, this), r(this, "playOnLoad", Fe, this), r(this, "autoRemoveOnFinish", Ce, this), r(this, "startColor", je, this), r(this, "_colorOvertime", Le, this), r(this, "fireAngle", Ve, this), r(this, "startAngle", Ie, this), r(this, "rotationType", Ge, this), r(this, "_angleOvertime", Ue, this), r(this, "_sysAngleOvertime", Ye, this), r(this, "startSize", xe, this), r(this, "startScaleX", Ne, this), r(this, "startScaleY", Ze, this), r(this, "_sizeOvertime", Xe, this), r(this, "shapeMode", Be, this), r(this, "sourcePos", He, this), r(this, "rangeSize", qe, this), r(this, "innerRangeSize", Je, this), r(this, "emitterMode", Ke, this), r(this, "targetTimeRate", Qe, this), r(this, "gravity", We, this), r(this, "startSpeed", $e, this), r(this, "velocityType", et, this), r(this, "tangentialAccel", tt, this), r(this, "radialAccel", it, this), r(this, "accelRelativeDir", rt, this), r(this, "_speedOvertime", nt, this), r(this, "_imageEmitter", ot, this), r(this, "_pathEmitter", at, this), r(this, "_animation", st, this), r(this, "_pathOvertime", lt, this), r(this, "_trail", ut, this), r(this, "_preview", pt, this), r(this, "renderFrame", ht, this), r(this, "_totalParticles", mt, this), r(this, "_positionType", ct, this), this.usePhy = !1, this.active = !0, this.willStop = !1, this.particleCount = 0, this.halfwidth = 1, this.halfheight = 1, this.elapsed = 0, this.emitCounter = 0, this.replaceUVs = [], this.needReplaceUVs = !1, this.uv = [], this.sleepTimer = 0, this.awakeTimer = 0, this.realDelay = 0, this.realDuration = 0, this.realEmitTurnDuration = 0, this.startSize.constant = 100, this.startLifeTime.constant = 1, this.startSpeed.constant = 500, this.startAngle.constant = 0, this.startScaleX.constant = 1, this.startScaleY.constant = 1, this.targetTimeRate.constant = 1
                }
                return e.reset = function() {
                    this.elapsed = 0, this.emitCounter = 0, this.particleCount = 0, this.sleepTimer = 0, this.willStop = !1, this.active = !0, this.realDelay = this.delay, this.realDuration = this.duration, this.realEmitTurnDuration = this.emitTurnDuration;
                    for (var e = 0; e < this._modules.length; e++) {
                        var t = this._modules[e];
                        t._enabled && t.onReset()
                    }
                }, e.initial = function(e) {
                    this._easyP2D || (this._easyP2D = e, this.bindModules())
                }, e.registerModule = function(e) {
                    e.bind(this._easyP2D), this._modules.push(e)
                }, e.bindModules = function() {
                    this._timer && this._timer.enabled && this.registerModule(this._timer), this._colorOvertime && this._colorOvertime.enabled && this.registerModule(this._colorOvertime), this._sizeOvertime && this._sizeOvertime.enabled && this.registerModule(this._sizeOvertime), this._speedOvertime && this._speedOvertime.enabled && this.registerModule(this._speedOvertime), this._animation && this._animation.enabled && this.registerModule(this._animation), this._angleOvertime && this._angleOvertime.enabled && this.registerModule(this._angleOvertime), this._imageEmitter && this._imageEmitter.enabled && this.registerModule(this._imageEmitter), this._pathEmitter && this._pathEmitter.enabled && this.registerModule(this._pathEmitter), this._pathOvertime && this._pathOvertime.enabled && this.registerModule(this._pathOvertime), this._sysAngleOvertime && this._sysAngleOvertime.enabled && this.registerModule(this._sysAngleOvertime), this._trail && this._trail.enabled && this.registerModule(this._trail)
                }, e.onEmitter = function(e, t) {
                    for (var i = 0; i < this._modules.length; i++) {
                        var r = this._modules[i];
                        r.enableOnEmitter && r.onEmitter(e, t)
                    }
                }, e.afterEmitter = function(e, t) {
                    for (var i = 0; i < this._modules.length; i++) {
                        var r = this._modules[i];
                        r.enableAfterEmitter && r.afterEmitter(e, t)
                    }
                }, e.onUpdate = function(e, t, i) {
                    for (var r = 0; r < this._modules.length; r++) {
                        var n = this._modules[r];
                        n.enableOnUpdate && n.onUpdate(e, t, i)
                    }
                }, e.afterUpdate = function(e, t, i, r) {
                    for (var n = 0; n < this._modules.length; n++) {
                        var o = this._modules[n];
                        o.enableAfterUpdate && o.afterUpdate(e, t, i, r)
                    }
                }, e.onEnd = function(e) {
                    for (var t = 0; t < this._modules.length; t++) {
                        var i = this._modules[t];
                        i.enableOnEnd && i.onEnd(e)
                    }
                }, e._syncAspect = function() {
                    if (this.renderFrame) {
                        var e = this.renderFrame.rect,
                            t = e.width / e.height,
                            i = 1,
                            r = 1;
                        t > 1 ? i = r / t : r = i * t, this.uv = this.renderFrame.uv, this.needReplaceUVs = !1, this.halfheight = i / 2, this.halfwidth = r / 2;
                        var n = 1,
                            o = 1;
                        this.useAnimation && (this._animation.updateUVs(), n = this._animation.tiles.x, o = this._animation.tiles.y), gt.x = n, gt.y = o, d(gt);
                        var a = 1 / Math.max(gt.x, gt.y);
                        this.halfwidth *= gt.y * a, this.halfheight *= gt.x * a
                    }
                }, i(t, [{
                    key: "easyP2D",
                    get: function() {
                        return this._easyP2D
                    }
                }, {
                    key: "spriteFrame",
                    get: function() {
                        return this.renderFrame
                    },
                    set: function(e) {
                        this.renderFrame !== e && (this.renderFrame = e, e && !e._uuid || (this.renderFrame = e), this._easyP2D.applySpriteFrame())
                    }
                }, {
                    key: "anchor",
                    get: function() {
                        return this._anchor
                    },
                    set: function(e) {
                        this._anchor = e
                    }
                }, {
                    key: "totalParticles",
                    get: function() {
                        return this._totalParticles
                    },
                    set: function(e) {
                        this._totalParticles !== e && (this._totalParticles = e)
                    }
                }, {
                    key: "timer",
                    get: function() {
                        return this._timer
                    }
                }, {
                    key: "enableSwapTime",
                    get: function() {
                        return this._timer && this._timer.enbleTimeSwap
                    }
                }, {
                    key: "awakeTime",
                    get: function() {
                        return this._timer && this._timer.awakeTime
                    }
                }, {
                    key: "sleepTime",
                    get: function() {
                        return this._timer && this._timer.sleepTime
                    }
                }, {
                    key: "preview",
                    get: function() {
                        return this._preview
                    },
                    set: function(e) {
                        e && this._easyP2D.startPreview(), this._preview = e
                    }
                }, {
                    key: "changeColor",
                    get: function() {
                        return this._colorOvertime && this._colorOvertime._enabled
                    }
                }, {
                    key: "colorOvertime",
                    get: function() {
                        return this._colorOvertime
                    }
                }, {
                    key: "angleOvertime",
                    get: function() {
                        return this._angleOvertime
                    }
                }, {
                    key: "sysAngleOvertime",
                    get: function() {
                        return this._sysAngleOvertime
                    }
                }, {
                    key: "useSysAngleOvertime",
                    get: function() {
                        return this._sysAngleOvertime && this._sysAngleOvertime._enabled
                    }
                }, {
                    key: "sizeOvertime",
                    get: function() {
                        return this._sizeOvertime
                    }
                }, {
                    key: "positionType",
                    get: function() {
                        return this._positionType
                    },
                    set: function(e) {
                        this._positionType = e, this._easyP2D.updateParticle()
                    }
                }, {
                    key: "speedOvertime",
                    get: function() {
                        return this._speedOvertime
                    }
                }, {
                    key: "imageEmitter",
                    get: function() {
                        return this._imageEmitter
                    }
                }, {
                    key: "pathEmitter",
                    get: function() {
                        return this._pathEmitter
                    }
                }, {
                    key: "useAnimation",
                    get: function() {
                        return this._animation && this._animation.enabled
                    }
                }, {
                    key: "animation",
                    get: function() {
                        return this._animation
                    }
                }, {
                    key: "pathOvertime",
                    get: function() {
                        return this._pathOvertime
                    }
                }, {
                    key: "trail",
                    get: function() {
                        return this._trail
                    }
                }, {
                    key: "trailEnabled",
                    get: function() {
                        return this._trail && this._trail._enabled
                    }
                }]), t
            }()).prototype, "spriteFrame", [E], Object.getOwnPropertyDescriptor(Te.prototype, "spriteFrame"), Te.prototype), Ae = t(Te.prototype, "_anchor", [T], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new u(.5, .5)
                }
            }), Pe = t(Te.prototype, "delay", [A], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 0
                }
            }), De = t(Te.prototype, "duration", [P], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return -1
                }
            }), Me = t(Te.prototype, "emissionRate", [D], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 10
                }
            }), Se = t(Te.prototype, "emitTurnDuration", [M], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 1
                }
            }), t(Te.prototype, "totalParticles", [S], Object.getOwnPropertyDescriptor(Te.prototype, "totalParticles"), Te.prototype), Re = t(Te.prototype, "startLifeTime", [R], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), ke = t(Te.prototype, "_timer", [k], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "timer", [F], Object.getOwnPropertyDescriptor(Te.prototype, "timer"), Te.prototype), t(Te.prototype, "preview", [yt], Object.getOwnPropertyDescriptor(Te.prototype, "preview"), Te.prototype), Fe = t(Te.prototype, "playOnLoad", [yt], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !0
                }
            }), Ce = t(Te.prototype, "autoRemoveOnFinish", [yt], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), je = t(Te.prototype, "startColor", [C], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new h
                }
            }), Le = t(Te.prototype, "_colorOvertime", [j], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "colorOvertime", [L], Object.getOwnPropertyDescriptor(Te.prototype, "colorOvertime"), Te.prototype), Ve = t(Te.prototype, "fireAngle", [V], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), Ie = t(Te.prototype, "startAngle", [I], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), Ge = t(Te.prototype, "rotationType", [G], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return m.None
                }
            }), Ue = t(Te.prototype, "_angleOvertime", [U], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "angleOvertime", [Y], Object.getOwnPropertyDescriptor(Te.prototype, "angleOvertime"), Te.prototype), Ye = t(Te.prototype, "_sysAngleOvertime", [x], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "sysAngleOvertime", [N], Object.getOwnPropertyDescriptor(Te.prototype, "sysAngleOvertime"), Te.prototype), xe = t(Te.prototype, "startSize", [Z], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), Ne = t(Te.prototype, "startScaleX", [X], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), Ze = t(Te.prototype, "startScaleY", [B], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), Xe = t(Te.prototype, "_sizeOvertime", [H], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "sizeOvertime", [q], Object.getOwnPropertyDescriptor(Te.prototype, "sizeOvertime"), Te.prototype), Be = t(Te.prototype, "shapeMode", [J], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return b.BOX
                }
            }), He = t(Te.prototype, "sourcePos", [K], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return u.ZERO.clone()
                }
            }), qe = t(Te.prototype, "rangeSize", [Q], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return u.ZERO.clone()
                }
            }), Je = t(Te.prototype, "innerRangeSize", [W], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return u.ZERO.clone()
                }
            }), t(Te.prototype, "positionType", [$], Object.getOwnPropertyDescriptor(Te.prototype, "positionType"), Te.prototype), Ke = t(Te.prototype, "emitterMode", [ee], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return c.GRAVITY
                }
            }), Qe = t(Te.prototype, "targetTimeRate", [te], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), We = t(Te.prototype, "gravity", [ie], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return u.ZERO.clone()
                }
            }), $e = t(Te.prototype, "startSpeed", [re], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), et = t(Te.prototype, "velocityType", [ne], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return g.FireDirection
                }
            }), tt = t(Te.prototype, "tangentialAccel", [oe], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), it = t(Te.prototype, "radialAccel", [ae], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new p
                }
            }), rt = t(Te.prototype, "accelRelativeDir", [se], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return f.FireDir
                }
            }), nt = t(Te.prototype, "_speedOvertime", [le], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "speedOvertime", [ue], Object.getOwnPropertyDescriptor(Te.prototype, "speedOvertime"), Te.prototype), ot = t(Te.prototype, "_imageEmitter", [pe], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "imageEmitter", [he], Object.getOwnPropertyDescriptor(Te.prototype, "imageEmitter"), Te.prototype), at = t(Te.prototype, "_pathEmitter", [me], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "pathEmitter", [ce], Object.getOwnPropertyDescriptor(Te.prototype, "pathEmitter"), Te.prototype), st = t(Te.prototype, "_animation", [be], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "animation", [ye], Object.getOwnPropertyDescriptor(Te.prototype, "animation"), Te.prototype), lt = t(Te.prototype, "_pathOvertime", [ge], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "pathOvertime", [fe], Object.getOwnPropertyDescriptor(Te.prototype, "pathOvertime"), Te.prototype), ut = t(Te.prototype, "_trail", [de], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), t(Te.prototype, "trail", [ve], Object.getOwnPropertyDescriptor(Te.prototype, "trail"), Te.prototype), pt = t(Te.prototype, "_preview", [_e], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !0
                }
            }), ht = t(Te.prototype, "renderFrame", [Oe], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), mt = t(Te.prototype, "_totalParticles", [we], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 150
                }
            }), ct = t(Te.prototype, "_positionType", [ze], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return y.FREE
                }
            }), Ee = Te)) || Ee));
            n._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EasyP2DUtils.ts", ["cc", "./EasyP2DDefine.ts", "./P2DSettings.ts"], (function(e) {
    var n, t, r, a, o, i, u, l, d;
    return {
        setters: [function(e) {
            n = e.cclegacy, t = e.CurveRange, r = e.math, a = e.GradientRange, o = e.Color, i = e.assetManager
        }, function(e) {
            u = e.ImageFormat, l = e.EmitterShape
        }, function(e) {
            d = e.P2DSettings
        }],
        execute: function() {
            e({
                calcColorOnCurve: function(e, n) {
                    if (e.mode === a.Mode.Color) return e.color;
                    if (e.mode === a.Mode.TwoColors) {
                        var t = Math.random();
                        return o.lerp(A, e.colorMin, e.colorMax, t)
                    }
                    if (e.mode == a.Mode.RandomColor || e.mode === a.Mode.RandomColor) {
                        var r = Math.random();
                        return e.evaluate(n, r)
                    }
                    if (!d.ENABLE_CALCULATE_CACHE) {
                        var i = m(e) ? Math.random() : 0;
                        return e.evaluate(n, i)
                    }
                    var u = p(e);
                    void 0 === e.__oldMode ? e.__oldMode = e.mode : e.__oldMode !== e.mode && (E.delete(u), e.__oldMode = e.mode, delete e.__hash);
                    u = p(e);
                    var l = f(n);
                    n = l[1], n = Math.min(.9999, n);
                    var s = l[0],
                        c = E.get(u);
                    void 0 === c && (c = [], E.set(u, c));
                    if (c[s]) return c[s].clone();
                    var h = m(e) ? Math.random() : 0,
                        _ = e.evaluate(n, h);
                    return c[s] = _.clone(), _
                },
                calcCos: function(e) {
                    if (!d.ENABLE_CALCULATE_CACHE) return Math.cos(e);
                    e %= 2 * Math.PI;
                    var n = Math.round(100 * e),
                        t = c[n];
                    void 0 === t && (t = c[n] = Math.cos(e));
                    return t
                },
                calcSin: function(e) {
                    if (!d.ENABLE_CALCULATE_CACHE) return Math.sin(e);
                    e %= 2 * Math.PI;
                    var n = Math.round(100 * e),
                        t = s[n];
                    void 0 === t && (t = s[n] = Math.sin(e));
                    return t
                },
                calcValueOnCurve: function(e, n) {
                    if (e.mode === t.Mode.Constant) return e.constant;
                    if (e.mode === t.Mode.TwoConstants) {
                        var a = Math.random();
                        return r.lerp(e.constantMin, e.constantMax, a)
                    }
                    if (!d.ENABLE_CALCULATE_CACHE) {
                        var o = h(e) ? Math.random() : 0;
                        return e.evaluate(n, o)
                    }
                    var i = C(e);
                    void 0 === e.__oldMode ? e.__oldMode = e.mode : e.__oldMode !== e.mode && (g.delete(i), e.__oldMode = e.mode, delete e.__hash);
                    i = C(e);
                    var u = f(n);
                    n = u[1];
                    var l = u[0],
                        s = g.get(i);
                    void 0 === s && (s = [], g.set(i, s));
                    if (void 0 !== s[l]) return s[l];
                    var c = h(e) ? Math.random() : 0,
                        _ = e.evaluate(n, c);
                    return s[l] = _, _
                },
                clampf: function(e, n, t) {
                    return e < n ? n : e < t ? e : t
                },
                getBatchParticle: function(e, n, t) {
                    var r = e.parent;
                    if (r) {
                        var a = r.getComponentsInChildren(t),
                            o = e.getComponent(t),
                            i = a.length;
                        if (0 === i) return null;
                        for (var u = 0; u < i; u++) {
                            var l = a[u];
                            if (l.node.active && l != o && l.system.renderFrame.texture == n.texture) return l
                        }
                        return null
                    }
                    return null
                },
                getComponentInChildren: function(e, n) {
                    for (var t = e.children, r = 0; r < t.length; r++) {
                        var a = t[r].getComponent(n);
                        if (a) return a
                    }
                    return null
                },
                getComponentInParent: function(e, n) {
                    var t = e.parent;
                    for (; t;) {
                        var r = t.getComponent(n);
                        if (r) return r;
                        t = t.parent
                    }
                    return null
                },
                getImageFormatByData: function(e) {
                    if (e.length > 8 && 137 === e[0] && 80 === e[1] && 78 === e[2] && 71 === e[3] && 13 === e[4] && 10 === e[5] && 26 === e[6] && 10 === e[7]) return u.PNG;
                    if (e.length > 2 && (73 === e[0] && 73 === e[1] || 77 === e[0] && 77 === e[1] || 255 === e[0] && 216 === e[1])) return u.TIFF;
                    return u.UNKNOWN
                },
                getP2DHash: function(e, n) {
                    return 1e4 * e.getHash() + n.getMaterialInstance(0).hash
                },
                getParticleComponents: function e(n, t) {
                    var r = n.parent,
                        a = n.getComponent(t);
                    if (!r || !a) return n.getComponentsInChildren(t);
                    return e(r, t)
                },
                getRangePos: function(e, n) {
                    T.x = 0, T.y = 0;
                    var t = e.innerRangeSize,
                        a = e.rangeSize;
                    if (e.shapeMode === l.BOX) t ? (T.x = r.randomRange(n.x - a.x + t.x, n.x + a.x - t.x), T.y = r.randomRange(n.y - a.y + t.y, n.y + a.y - t.y)) : (T.x = r.randomRange(n.x - a.x, n.x + a.x), T.y = r.randomRange(n.y - a.y, n.y + a.y));
                    else if (e.shapeMode === l.CIRCLE) {
                        var o = 2 * Math.random() * Math.PI,
                            i = Math.sin(o),
                            u = Math.cos(o);
                        if (t) {
                            var d = r.randomRange(t.x, a.x),
                                s = r.randomRange(t.y, a.y);
                            T.x = n.x + d * u - s * i, T.y = n.y + d * i + s * u
                        } else T.x = n.x + a.x * u - a.y * i, T.y = n.y + a.x * i + a.y * u
                    } else T.x = n.x, T.y = n.y;
                    return T
                },
                getWorldRotation: function(e) {
                    var n = 0,
                        t = e;
                    for (; t;) n += t.eulerAngles.z, t = t.parent;
                    return n
                },
                isCurveTwoValues: h,
                isGradientTwoValues: m,
                loadSF: function(e) {
                    return new Promise((function(n, t) {
                        i.loadAny({
                            uuid: e
                        }, (function(e, t) {
                            e ? (console.error("Load Custom Mat Error", e), n && n(null)) : n && n(t)
                        }))
                    }))
                },
                normalizeVec2: function(e) {
                    var n = Math.sqrt(e.x * e.x + e.y * e.y);
                    0 === n ? e.x = e.y = 0 : (e.x = e.x / n, e.y = e.y / n)
                },
                pointInPolygon: function(e, n) {
                    for (var t = 0, r = 0; r < n.length; r++) {
                        var a = n[r],
                            o = n[(r + 1) % n.length];
                        a.y > e.y != o.y > e.y && e.x < (o.x - a.x) * (e.y - a.y) / (o.y - a.y) + a.x && t++
                    }
                    return t % 2 == 1
                },
                resetRenderdata: function(e) {
                    e._vc = 0, e._ic = 0, e._byteLength = 0, e.vertexStart = 0, e.vertexRange = 0, e.indexStart = 0, e.indexRange = 0, e.lastFilledIndex = 0, e.lastFilledVertex = 0, e.freeIAPool()
                }
            }), n._RF.push({}, "a939dWLgc5EJ4Mnt5VraNwK", "EasyP2DUtils", void 0);
            var s = [];
            var c = [];

            function h(e) {
                var n = t.Mode;
                switch (e.mode) {
                    case n.TwoCurves:
                    case n.TwoConstants:
                        return !0;
                    default:
                        return !1
                }
            }

            function _(e) {
                var n = d.CALCULATE_CACHE_PERCENT,
                    t = Math.pow(10, n);
                return Math.round(e * t)
            }

            function v(e) {
                var n = d.CALCULATE_CACHE_PERCENT,
                    t = Math.pow(10, n);
                return Math.round(e * t) / t
            }

            function f(e) {
                var n = d.CALCULATE_CACHE_PERCENT,
                    t = Math.pow(10, n),
                    r = Math.round(e * t);
                return [r, r / t]
            }

            function M(e) {
                if (e.__hash) return e.__hash;
                for (var n, t = e._times, r = e._values, a = "", o = 0; o < t.length; o++) {
                    var i = t[o],
                        u = r[o];
                    a += i + "_" + (_((n = u).value) << 7 | n.easingMethod << 6 | n.interpolationMode << 5 | n.leftTangent << 4 | n.rightTangent << 3 | n.leftTangentWeight << 2 | n.rightTangentWeight << 1 | n.tangentWeightMode)
                }
                return e.__hash = a, a
            }

            function C(e) {
                if (e.__hash) return e.__hash;
                var n = e.mode,
                    r = "";
                switch (n) {
                    case t.Mode.Constant:
                        r = n + "_" + e.multiplier + "_" + _(e.constant);
                        break;
                    case t.Mode.TwoConstants:
                        r = n + "_" + e.multiplier + "_" + _(e.constantMin) + "_" + _(e.constantMax);
                        break;
                    case t.Mode.Curve:
                        r = n + "_" + e.multiplier + "_" + M(e.spline);
                        break;
                    case t.Mode.TwoCurves:
                        r = n + "_" + e.multiplier + "_" + M(e.splineMin) + "_" + M(e.splineMax)
                }
                return e.__hash = r, r
            }
            var g = new Map;

            function m(e) {
                var n = a.Mode;
                switch (e.mode) {
                    case n.TwoGradients:
                    case n.TwoColors:
                        return !0;
                    default:
                        return !1
                }
            }

            function y(e) {
                if (e.__hash) return e.__hash;
                for (var n = e.alphaKeys, t = e.colorKeys, r = "", a = 0; a < n.length; a++) {
                    var o = n[a];
                    r += v(o.time) + "_" + v(o.alpha)
                }
                for (var i = 0; i < t.length; i++) {
                    var u = t[i];
                    r += v(u.time) + "_" + x(u.color)
                }
                return e.__hash = r, r
            }

            function x(e) {
                return e.r + "_" + e.g + "_" + e.b + "_" + e.a
            }

            function p(e) {
                if (e.__hash) return e.__hash;
                var n = e.mode,
                    t = "";
                switch (n) {
                    case a.Mode.Color:
                        t = n + "_" + x(e.color);
                        break;
                    case a.Mode.TwoColors:
                        t = n + "_" + x(e.colorMin) + "_" + x(e.colorMax);
                        break;
                    case a.Mode.Gradient:
                        t = n + "_" + y(e.gradient);
                        break;
                    case a.Mode.TwoGradients:
                        t = n + "_" + y(e.gradientMin) + "_" + y(e.gradientMax)
                }
                return e.__hash = t, t
            }
            var E = new Map,
                A = new o;
            var T = {
                x: 0,
                y: 0
            };
            n._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/EventType.ts", ["cc"], (function(e) {
    var t;
    return {
        setters: [function(e) {
            t = e.cclegacy
        }],
        execute: function() {
            t._RF.push({}, "7e0demz9YdBXLH5coDt0lkR", "EventType", void 0);
            e("RegEventType", function(e) {
                return e[e.Set_Cfg = 1] = "Set_Cfg", e[e.Prepare_Data = 2] = "Prepare_Data", e[e.Set_AutoBattle = 3] = "Set_AutoBattle", e[e.Unit_Attr_Change = 4] = "Unit_Attr_Change", e[e.Unit_Skill_Change = 5] = "Unit_Skill_Change", e[e.Unit_Reborn_Change = 6] = "Unit_Reborn_Change", e[e.Do_Battle_Tick = 7] = "Do_Battle_Tick", e[e.Do_Battle_Tick_Small = 17] = "Do_Battle_Tick_Small", e[e.Set_External_Communicator = 8] = "Set_External_Communicator", e[e.Escape_Exit = 9] = "Escape_Exit", e[e.Skip_Battle = 10] = "Skip_Battle", e[e.Unit_Use_Skill = 11] = "Unit_Use_Skill", e[e.Add_Unit = 12] = "Add_Unit", e[e.Unit_Move = 99] = "Unit_Move", e[e.Set_RVO = 100] = "Set_RVO", e
            }({})), e("BattleEvent", function(e) {
                return e[e.InitScene = 999] = "InitScene", e[e.StartBattle = 1e3] = "StartBattle", e[e.UnitCreate = 1001] = "UnitCreate", e[e.UseSkill = 1002] = "UseSkill", e[e.Hurt = 1003] = "Hurt", e[e.RoleUpdate = 1004] = "RoleUpdate", e[e.BattleResult = 2e3] = "BattleResult", e[e.UnitMove = 1005] = "UnitMove", e[e.SyncMove = 1111] = "SyncMove", e[e.UnitMoveEnd = 1006] = "UnitMoveEnd", e[e.SkillEffect = 1007] = "SkillEffect", e[e.BuffAdd = 1008] = "BuffAdd", e[e.BuffRemove = 1009] = "BuffRemove", e[e.SkillEffectRemove = 1010] = "SkillEffectRemove", e[e.UnitHpUpdate = 1011] = "UnitHpUpdate", e[e.UnitEnergyUpdate = 1012] = "UnitEnergyUpdate", e[e.UnitSkillCdUpdate = 1013] = "UnitSkillCdUpdate", e[e.EventTips = 1014] = "EventTips", e[e.SkillBullet = 1015] = "SkillBullet", e[e.UnitShieldUpdate = 1016] = "UnitShieldUpdate", e[e.SkillEffectDecide = 1017] = "SkillEffectDecide", e[e.SkillDurationUpdate = 1018] = "SkillDurationUpdate", e[e.SkillBulletSpeedUpdate = 1019] = "SkillBulletSpeedUpdate", e[e.SkillRangeUpdate = 1020] = "SkillRangeUpdate", e[e.SkillBulletInteract = 1021] = "SkillBulletInteract", e[e.SyncShape = 99999] = "SyncShape", e[e.FrameEnd = 98888] = "FrameEnd", e[e.AutoBattle = 99998] = "AutoBattle", e[e.SyncBuffData = 1022] = "SyncBuffData", e[e.SyncState = 1e4] = "SyncState", e[e.SyncPos = 10001] = "SyncPos", e[e.SyncAngle = 10002] = "SyncAngle", e[e.CreateBullet = 10003] = "CreateBullet", e[e.CreateTrap = 10004] = "CreateTrap", e[e.DestroyBulletTrap = 10005] = "DestroyBulletTrap", e[e.CreatSkillEffect = 10006] = "CreatSkillEffect", e[e.ShipShape = 10007] = "ShipShape", e[e.MonsterTemplate = 10008] = "MonsterTemplate", e[e.SceneAngle = 100090] = "SceneAngle", e[e.DivideLine = 1000100] = "DivideLine", e[e.MapSize = 1000101] = "MapSize", e[e.StraightQuadTree = 1000102] = "StraightQuadTree", e[e.UpdateUnitQuadTree = 1000103] = "UpdateUnitQuadTree", e[e.Frame = 1000104] = "Frame", e
            }({}));
            t._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/ImageEmitterModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DDefine.ts", "./EasyP2DModuleBase.ts", "./EasyP2DUtils.ts"], (function(e) {
    var t, i, r, n, a, o, s, u, h, l, p, f, x, c, d, y, g;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, i = e.inheritsLoose, r = e.initializerDefineProperty, n = e.assertThisInitialized, a = e.createClass
        }, function(e) {
            o = e.cclegacy, s = e._decorator, u = e.Texture2D, h = e.Vec2, l = e.Rect, p = e.math, f = e.errorID, x = e.gfx
        }, function(e) {
            c = e.ExpansionType, d = e.EmitterMode
        }, function(e) {
            y = e.EasyP2DModuleBase
        }, function(e) {
            g = e.getRangePos
        }],
        execute: function() {
            var v, m, w, _, P, b, D, E, M, T, I;
            o._RF.push({}, "b862fvREFtJjZaHhVI2JYZ9", "ImageEmitterModule", void 0);
            var R = s.property,
                z = s.ccclass,
                B = (e("ImageEmitterModule", (v = z("ImageEmitterModule"), m = R({
                    type: u,
                    visible: !1
                }), w = R(u), _ = R({
                    type: c
                }), v((D = t((b = function(e) {
                    function t() {
                        var t;
                        return t = e.call(this) || this, r(t, "_texture", D, n(t)), r(t, "scale", E, n(t)), r(t, "flipY", M, n(t)), r(t, "expansion", T, n(t)), r(t, "updateDirection", I, n(t)), t._validPixels = [], t.enableOnEmitter = !0, t.enableOnUpdate = !0, t
                    }
                    i(t, e);
                    var o = t.prototype;
                    return o.onInit = function() {
                        if (this._texture) {
                            var e = t.readTexturePixels(this._texture);
                            e = t.getAlphaBuffer(new l(0, 0, this._texture.width, this._texture.height), e, this.flipY);
                            for (var i = .5 * this._texture.width, r = .5 * this._texture.height, n = 0; n < e.length; n++)
                                if (e[n] > 0) {
                                    var a = n % this._texture.width,
                                        o = Math.floor(n / this._texture.width);
                                    this._validPixels.push(new h(a - i, o - r))
                                } e = null
                        }
                    }, o.onEmitter = function(e, t) {
                        this._texture && !e.preventDefaultSetPos && (this._easyP2D.system.emitterMode == d.TARGET ? (this.generatePos(this._easyP2D, t, e.targetPos), this.updateExpansion(e, !0)) : (this.generatePos(this._easyP2D, t, e.pos), e.preventDefaultSetPos = !0, this.updateExpansion(e, !0)))
                    }, o.updateExpansion = function(e, t) {
                        switch (void 0 === t && (t = !1), this.expansion) {
                            case c.Random:
                                t && (B.x = p.randomRange(-1, 1), B.y = p.randomRange(-1, 1));
                                break;
                            case c.Out:
                                B.x = e.pos.x, B.y = e.pos.y;
                                break;
                            case c.In:
                                B.x = -e.pos.x, B.y = -e.pos.y
                        }
                        this.expansion !== c.None && (B.normalize(), e.dir.x = B.x, e.dir.y = B.y, e.preventDefaultSetDir = !0)
                    }, o.onUpdate = function(e, t, i) {
                        var r = this._easyP2D.system;
                        this.updateDirection && r.emitterMode != d.TARGET && this.updateExpansion(e, !1)
                    }, o.generatePos = function(e, t, i) {
                        i = i || B;
                        var r = Math.floor(Math.random() * this._validPixels.length),
                            n = this._validPixels[r];
                        i.x = n.x * this.scale, i.y = n.y * this.scale;
                        var a = e.system,
                            o = g(a, i);
                        return i.x = o.x, i.y = o.y, i
                    }, t.readTexturePixels = function(e, t, i) {
                        (t = t || new l(0, 0, e.width, e.height)).x = Math.floor(t.x), t.y = Math.floor(t.y), t.width = Math.floor(t.width), t.height = Math.floor(t.height);
                        var r = e.getGFXTexture();
                        if (!r) return f(7606), null;
                        var n = 4 * t.width * t.height;
                        if (void 0 === i) i = new Uint8Array(n);
                        else if (i.length < n) return f(7607, n), null;
                        var a = [],
                            o = [],
                            s = new x.BufferTextureCopy;
                        s.texOffset.x = t.x, s.texOffset.y = t.y, s.texExtent.width = t.width, s.texExtent.height = t.height, o.push(s), a.push(i);
                        var u = e._getGFXDevice();
                        return null == u || u.copyTextureToBuffers(r, a, o), i
                    }, t.getAlphaBuffer = function(e, t, i, r) {
                        void 0 === i && (i = !1);
                        var n = e.width * e.height;
                        if (void 0 === r) r = new Uint8Array(n);
                        else if (r.length < n) return f(7607, n), null;
                        if (i)
                            for (var a = 0; a < e.height; a++)
                                for (var o = 0; o < e.width; o++) {
                                    r[(e.height - a - 1) * e.width + o] = t[4 * (a * e.width + o) + 3]
                                } else
                                    for (var s = 0; s < n; s++) r[s] = t[4 * s + 3];
                        return r
                    }, a(t, [{
                        key: "texture",
                        get: function() {
                            return this._texture
                        },
                        set: function(e) {
                            this._texture !== e && (this._texture = e, this._texture && this.onInit())
                        }
                    }]), t
                }(y)).prototype, "_texture", [m], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return null
                    }
                }), t(b.prototype, "texture", [w], Object.getOwnPropertyDescriptor(b.prototype, "texture"), b.prototype), E = t(b.prototype, "scale", [R], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return 1
                    }
                }), M = t(b.prototype, "flipY", [R], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return !0
                    }
                }), T = t(b.prototype, "expansion", [_], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return c.None
                    }
                }), I = t(b.prototype, "updateDirection", [R], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return !1
                    }
                }), P = b)) || P)), new h);
            o._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/index.ts", ["cc", "./EasyP2D.ts"], (function(n) {
    var t;
    return {
        setters: [function(n) {
            t = n.cclegacy
        }, function(t) {
            n("EasyP2D", t.EasyP2D)
        }],
        execute: function() {
            t._RF.push({}, "b2802cRCMRDaJ5PGxw8Dhn2", "index", void 0), t._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/interface.ts", ["cc"], (function() {
    var e;
    return {
        setters: [function(t) {
            e = t.cclegacy
        }],
        execute: function() {
            e._RF.push({}, "d7bd9zzAV9M6I19Wg143dtR", "interface", void 0), e._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/interfaces.ts", ["cc"], (function() {
    var e;
    return {
        setters: [function(c) {
            e = c.cclegacy
        }],
        execute: function() {
            e._RF.push({}, "810c5ZHS+REa6+YCoWKRGv3", "interfaces", void 0), e._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/LabelQmitCom.ts", ["./rollupPluginModLoBabelHelpers.js", "cc"], (function(t) {
    var i, n, e, o, s;
    return {
        setters: [function(t) {
            i = t.inheritsLoose
        }, function(t) {
            n = t.cclegacy, e = t._decorator, o = t.Label, s = t.Component
        }],
        execute: function() {
            var r;
            n._RF.push({}, "7c967ZAIm9D+KnHx4INV55H", "LabelQmitCom", void 0);
            t("LabelQmitCom", e.ccclass("LabelQmitCom")(r = function(t) {
                function n() {
                    for (var i, n = arguments.length, e = new Array(n), o = 0; o < n; o++) e[o] = arguments[o];
                    return (i = t.call.apply(t, [this].concat(e)) || this)._formatStr = void 0, i._qmitLen = void 0, i._isLaunch = void 0, i
                }
                i(n, t);
                var e = n.prototype;
                return e.start = function() {
                    this.schedule(this.updateLabel.bind(this), 1)
                }, e.setFormat = function(t, i) {
                    this._formatStr = t, this._qmitLen = i, this._isLaunch = !0, this.getComponent(o).string = t
                }, e.removeQmit = function() {
                    this._isLaunch = !1
                }, e.updateLabel = function() {
                    if (this._isLaunch) {
                        var t, i = this.getComponent(o).string,
                            n = (null != (t = this._qmitLen) ? t : 0) + this._formatStr.length,
                            e = i.length >= n ? this._formatStr : i + ".";
                        this.getComponent(o).string = e
                    }
                }, n
            }(s)) || r);
            n._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/LanguageData.ts", ["cc"], (function(e) {
    var n, t;
    return {
        setters: [function(e) {
            n = e.cclegacy, t = e.director
        }],
        execute: function() {
            e({
                init: r,
                t: function(e) {
                    var n = window;
                    if (!n.languages) return e;
                    for (var t = e.split("."), r = n.languages[a], i = 0; i < t.length; i++)
                        if (!(r = r[t[i]])) return "";
                    return "string" == typeof r ? r : ""
                },
                updateSceneRenderers: i
            }), n._RF.push({}, "b3f01X4Ik5LEbIPKXZr3JWD", "LanguageData", void 0);
            var a = e("_language", "zh");
            e("ready", !1);

            function r(n) {
                e("ready", !0), a = e("_language", n)
            }

            function i() {
                for (var e = t.getScene().children, n = [], a = 0; a < e.length; ++a) {
                    var r = e[a].getComponentsInChildren("LocalizedLabel");
                    Array.prototype.push.apply(n, r)
                }
                for (var i = 0; i < n.length; ++i) {
                    var o = n[i];
                    o.node.active && o.updateLabel()
                }
                for (var u = [], g = 0; g < e.length; ++g) {
                    var c = e[g].getComponentsInChildren("LocalizedSprite");
                    Array.prototype.push.apply(u, c)
                }
                for (var l = 0; l < u.length; ++l) {
                    var p = u[l];
                    p.node.active && p.updateSprite()
                }
            }
            window._languageData = {
                get language() {
                    return a
                },
                init: function(e) {
                    r(e)
                },
                updateSceneRenderers: function() {
                    i()
                }
            }, n._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/LocalizedLabel.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./LanguageData.ts"], (function(e) {
    var t, i, n, o, r, a, s, c, p, l, u, y, h;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, i = e.inheritsLoose, n = e.initializerDefineProperty, o = e.assertThisInitialized, r = e.createClass
        }, function(e) {
            a = e.cclegacy, s = e._decorator, c = e.Label, p = e.RichText, l = e.Component
        }, function(e) {
            u = e.ready, y = e.init, h = e.t
        }],
        execute: function() {
            var d, f, g, b, L, k;
            a._RF.push({}, "edf98QH2D1AUoCrtQlbxFrU", "LocalizedLabel", void 0);
            var v = s.ccclass,
                m = s.property,
                z = s.executeInEditMode;
            e("LocalizedLabel", (d = v("LocalizedLabel"), f = m({
                visible: !1
            }), g = m({
                displayName: "Key",
                visible: !0
            }), d(b = z((k = t((L = function(e) {
                function t() {
                    for (var t, i = arguments.length, r = new Array(i), a = 0; a < i; a++) r[a] = arguments[a];
                    return t = e.call.apply(e, [this].concat(r)) || this, n(t, "key", k, o(t)), t
                }
                i(t, e);
                var a = t.prototype;
                return a.onLoad = function() {
                    u || y("zh"), this.updateText()
                }, a.updateText = function() {
                    this.getComponent(c) ? this.getComponent(c).string = h(this.key) : this.getComponent(p) && (this.getComponent(p).string = h(this.key))
                }, r(t, [{
                    key: "_key",
                    get: function() {
                        return this.key
                    },
                    set: function(e) {
                        this.key = e, this.updateText()
                    }
                }]), t
            }(l)).prototype, "key", [f], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return ""
                }
            }), t(L.prototype, "_key", [g], Object.getOwnPropertyDescriptor(L.prototype, "_key"), L.prototype), b = L)) || b) || b));
            a._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/LocalizedSprite.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./LanguageData.ts"], (function(e) {
    var t, i, r, n, a, o, p, s, c, u, l;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, i = e.initializerDefineProperty, r = e.inheritsLoose, n = e.assertThisInitialized
        }, function(e) {
            a = e.cclegacy, o = e._decorator, p = e.SpriteFrame, s = e.Component
        }, function(e) {
            c = e.ready, u = e.init, l = e._language
        }],
        execute: function() {
            var f, h, d, g, y, z, L, m, b, v, S;
            a._RF.push({}, "c05c25C4xNAupWYMB93tIzp", "LocalizedSprite", void 0);
            var F = o.ccclass,
                _ = o.property,
                w = o.executeInEditMode,
                D = (f = F("LocalizedSpriteItem"), h = _({
                    type: p
                }), f((y = t((g = function() {
                    i(this, "language", y, this), i(this, "spriteFrame", z, this)
                }).prototype, "language", [_], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return "zh"
                    }
                }), z = t(g.prototype, "spriteFrame", [h], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return null
                    }
                }), d = g)) || d);
            e("LocalizedSprite", (L = F("LocalizedSprite"), m = _({
                type: D
            }), L(b = w((S = t((v = function(e) {
                function t() {
                    for (var t, r = arguments.length, a = new Array(r), o = 0; o < r; o++) a[o] = arguments[o];
                    return (t = e.call.apply(e, [this].concat(a)) || this).sprite = null, i(t, "spriteList", S, n(t)), t
                }
                r(t, e);
                var a = t.prototype;
                return a.onLoad = function() {
                    c || u("zh"), this.fetchRender()
                }, a.fetchRender = function() {
                    var e = this.getComponent("cc.Sprite");
                    if (e) return this.sprite = e, void this.updateSprite()
                }, a.updateSprite = function() {
                    for (var e = 0; e < this.spriteList.length; e++) {
                        var t = this.spriteList[e];
                        if (t.language === l) {
                            this.sprite.spriteFrame = t.spriteFrame;
                            break
                        }
                    }
                }, t
            }(s)).prototype, "spriteList", [m], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return []
                }
            }), b = v)) || b) || b));
            a._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/main", ["./EasyP2D.ts", "./EasyP2DAssembler.ts", "./EasyP2DDefine.ts", "./EasyP2DSimulator.ts", "./EasyP2DSystem.ts", "./EasyP2DUtils.ts", "./MeshBatchData.ts", "./MeshBatcher.ts", "./AngleOvertimeModule.ts", "./AnimationModule.ts", "./ColorOvertimeModule.ts", "./Curve.ts", "./EasyP2DModuleBase.ts", "./ImageEmitterModule.ts", "./PathEmitterModule.ts", "./PathOvertimeModule.ts", "./SizeOvertimeModule.ts", "./SpeedOvertimeModule.ts", "./SystemAngleOvertimeModule.ts", "./TimerModule.ts", "./TrailCreator.ts", "./TrailModule.ts", "./P2DSettings.ts", "./Particle.ts", "./index.ts", "./interfaces.ts", "./patch.ts", "./AttrConst.ts", "./BattleConst.ts", "./EventType.ts", "./interface.ts", "./LabelQmitCom.ts", "./LanguageData.ts", "./LocalizedLabel.ts", "./LocalizedSprite.ts"], (function() {
    return {
        setters: [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
        execute: function() {}
    }
}));

System.register("chunks:///_virtual/MeshBatchData.ts", ["cc"], (function(t) {
    var e;
    return {
        setters: [function(t) {
            e = t.cclegacy
        }],
        execute: function() {
            e._RF.push({}, "16aeemceRBC6YQ6tOPEo2Ry", "MeshBatchData", void 0);
            t("MeshBatchData", function() {
                function t() {
                    this.startVertexIndex = 0, this.startIndexIndex = 0, this.vertexCount = 0, this.indexCount = 0, this.renderData = null, this.lastVertexCount = 0, this.lastIndexCount = 0, this.isRendering = !1, this._p2d = null
                }
                var e = t.prototype;
                return e.init = function(t) {
                    this._p2d = t
                }, e.reset = function() {
                    this.isRendering = !1, this.lastVertexCount = this.vertexCount, this.lastIndexCount = this.indexCount, this.startVertexIndex = 0, this.startIndexIndex = 0, this.vertexCount = 0, this.indexCount = 0
                }, e.request = function(t, e, n) {
                    this.renderData = t;
                    var s = this._p2d.meshRenderData;
                    t == s ? (this.vertexCount = s.vertexCount, this.indexCount = s.indexCount) : (this.startIndexIndex = n, this.startVertexIndex = e, this.vertexCount = t.vertexCount - e, this.indexCount = t.indexCount - n)
                }, t
            }());
            e._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/MeshBatcher.ts", ["cc", "./P2DSettings.ts"], (function(e) {
    var t, r, n, a;
    return {
        setters: [function(e) {
            t = e.cclegacy, r = e.director, n = e.Director
        }, function(e) {
            a = e.P2DSettings
        }],
        execute: function() {
            t._RF.push({}, "f348fuDajpFHYJC805wlz4N", "MeshBatcher", void 0);
            r.on(n.EVENT_BEGIN_FRAME, (function() {
                s.init()
            }));
            var s = e("MeshBatcher", function() {
                function e() {}
                return e.init = function() {
                    this._start = !0, this._currentInfo.p2d = null, this._currentInfo.renderData = null
                }, e.batchProcess = function(e, t, r) {
                    var n = this._currentInfo;
                    if (e.batchData.reset(), e.meshRenderData && !(t <= 0)) {
                        var s = n.renderData,
                            i = !0,
                            c = 0,
                            u = 0,
                            l = e.system.renderFrame,
                            o = e.node.layer,
                            d = null;
                        s && (d = s.material, c = s.vertexCount, u = s.indexCount, a.ENABLE_BATCHER && ((i = !n.p2d || n.material !== d || n.layer != o || n.renderFrame.texture !== l.texture || n.stencilStage !== e.stencilStage) || s.request(t, r) || (i = !0))), i && (e.swapBuffer(), s = e.meshRenderData, n.p2d = e, n.renderFrame = l, n.material = d, n.layer = o, n.renderFrame = e.system.renderFrame, n.assembler = e.assembler, n.stencilStage = e.stencilStage, n.renderData = s, s.vertexCount < t ? (s.reset(), s.request(t, r)) : s.resize(t, r), c = 0, u = 0), e.batchData.request(s, c, u)
                    }
                }, e
            }());
            s._currentInfo = new function() {
                this.p2d = void 0, this.renderData = void 0, this.material = null, this.renderFrame = void 0, this.assembler = null, this.layer = 0, this.render = void 0, this.stencilStage = 0
            }, s._start = !0, t._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/P2DSettings.ts", ["cc"], (function(t) {
    var E;
    return {
        setters: [function(t) {
            E = t.cclegacy
        }],
        execute: function() {
            E._RF.push({}, "86913n1fmpMybtRSaVl9Nc/", "P2DSettings", void 0);
            t("P2DSettings", {
                ENABLE_CALCULATE_CACHE: !1,
                CALCULATE_CACHE_PERCENT: 2,
                ENABLE_BATCHER: !0,
                ENABLE_SWAP_BATCHER: !1,
                SUPPORT_PARTICLE_TRANSFORM: !0,
                FIX_FREE_MODE_ROTATION: !0
            });
            E._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/Particle.ts", ["cc"], (function(t) {
    var i;
    return {
        setters: [function(t) {
            i = t.cclegacy
        }],
        execute: function() {
            i._RF.push({}, "f2b35enMJBBWLO2dXnrmBfs", "Particle", void 0);
            t("Particle", function() {
                function t() {
                    this.index = -1, this.data = null, this.preventDefaultSetPos = !1, this.preventDefaultSetDir = !1, this.preventDefaultSetSpeed = !1, this.sys = void 0, this.pos = {
                        x: 0,
                        y: 0
                    }, this.startPos = {
                        x: 0,
                        y: 0
                    }, this.fromPos = {
                        x: 0,
                        y: 0
                    }, this.targetPos = {
                        x: 0,
                        y: 0
                    }, this.targetTimeRate = 1, this.finalPos = {
                        x: 0,
                        y: 0
                    }, this.color = [1, 1, 1, 1], this.isColorDirty = !1, this.halfWidth = 0, this.halfHeight = 0, this.startSize = 0, this.size = 0, this.rotation = 0, this.startRotation = 0, this.fireRotation = 0, this.totalTime = 0, this.timeToLive = 0, this.dir = {
                        x: 0,
                        y: 0
                    }, this.startDir = {
                        x: 0,
                        y: 0
                    }, this.gravity = {
                        x: 0,
                        y: 0
                    }, this.radialAccel = 0, this.tangentialAccel = 0, this.speed = 0, this.startSpeed = 0, this.currentFrame = -1, this.tiles = {
                        x: 0,
                        y: 0
                    }, this.uv = [], this.verticles = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
                }
                return t.prototype.beforeEmit = function(t) {
                    this.frameIsDirty = !0, this.sys = t, this.startPos.x = 0, this.startPos.y = 0, this.fromPos.x = 0, this.fromPos.y = 0, this.targetPos.x = 0, this.targetPos.y = 0, this.targetTimeRate = 1, this.preventDefaultSetPos = !1, this.preventDefaultSetDir = !1, this.preventDefaultSetSpeed = !1, this.rotation = 0, this.startRotation = 0, this.fireRotation = 0, this.totalTime = 0, this.timeToLive = 0
                }, t
            }());
            i._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/patch.ts", ["cc"], (function(t) {
    var c;
    return {
        setters: [function(t) {
            c = t.cclegacy
        }],
        execute: function() {
            c._RF.push({}, "6fb72Id0e1LGIItsYyXG5cn", "patch", void 0);
            t("UIVertexFormatX", {});
            c._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/PathEmitterModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DDefine.ts", "./EasyP2DModuleBase.ts", "./EasyP2DUtils.ts", "./Curve.ts"], (function(e) {
    var t, i, n, r, s, o, a, l, u, h, p, c, y, d, g, f, v, _;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, i = e.inheritsLoose, n = e.initializerDefineProperty, r = e.assertThisInitialized, s = e.createClass
        }, function(e) {
            o = e.cclegacy, a = e._decorator, l = e.CurveRange, u = e.Vec2, h = e.math
        }, function(e) {
            p = e.PathEmitMode, c = e.ExpansionType, y = e.EmitterMode
        }, function(e) {
            d = e.EasyP2DModuleBase
        }, function(e) {
            g = e.calcValueOnCurve, f = e.getRangePos
        }, function(e) {
            v = e.BezierPosition, _ = e.Curve
        }],
        execute: function() {
            var m, P, b, x, M, z, D, E, I, w, C, T, O, R, k, B, j;
            o._RF.push({}, "d419dzj3yBGF7hSGDuTzIdB", "PathEmitterModule", void 0);
            var S = a.ccclass,
                G = a.property,
                L = (e("PathEmitterModule", (m = S("PathEmitterModule"), P = G({
                    type: [v],
                    visible: !1
                }), b = G({
                    type: [v],
                    visible: function() {
                        return this.enabled
                    }
                }), x = G({
                    type: l,
                    visible: function() {
                        return this.enabled
                    }
                }), M = G({
                    visible: !1
                }), z = G({
                    type: p,
                    visible: function() {
                        return this.online
                    }
                }), D = G({
                    type: c
                }), E = G({
                    visible: !1
                }), m((C = t((w = function(e) {
                    function t() {
                        var t;
                        return t = e.call(this) || this, n(t, "_poses", C, r(t)), n(t, "scale", T, r(t)), n(t, "_online", O, r(t)), n(t, "emitMode", R, r(t)), n(t, "expansion", k, r(t)), n(t, "updateDirection", B, r(t)), n(t, "_curveMode", j, r(t)), t._triangleIndices = [], t._bezierCurve = new _, t._samplePoints = [], t.scale.constant = 1, t.enableOnEmitter = !0, t.enableOnUpdate = !0, t
                    }
                    i(t, e);
                    var o = t.prototype;
                    return o.onInit = function() {
                        this.validPoses && (this._bezierCurve.curveMode = this.curveMode, this._bezierCurve.reset(this.poses), this.online || this.triangulate())
                    }, o.onEmitter = function(e, t) {
                        this.validPoses && !e.preventDefaultSetPos && (this._easyP2D.system.emitterMode == y.TARGET ? (this.generatePos(this._easyP2D, t, e.targetPos), this.updateExpansion(e, !0)) : (this.generatePos(this._easyP2D, t, e.pos), e.preventDefaultSetPos = !0, this.updateExpansion(e, !0)))
                    }, o.updateExpansion = function(e, t) {
                        switch (void 0 === t && (t = !1), this.expansion) {
                            case c.Random:
                                t && (L.x = h.randomRange(-1, 1), L.y = h.randomRange(-1, 1));
                                break;
                            case c.Out:
                                L.x = e.pos.x, L.y = e.pos.y;
                                break;
                            case c.In:
                                L.x = -e.pos.x, L.y = -e.pos.y
                        }
                        this.expansion !== c.None && (L.normalize(), e.dir.x = L.x, e.dir.y = L.y, e.preventDefaultSetDir = !0)
                    }, o.onUpdate = function(e, t, i) {
                        var n = this._easyP2D.system;
                        this.updateDirection && n.emitterMode != y.TARGET && this.updateExpansion(e, !1)
                    }, o.generatePos = function(e, t, i) {
                        var n = L,
                            r = g(this.scale, t);
                        if (this.online)
                            if (this.emitMode == p.ByTime) {
                                var s = this._bezierCurve.getPoint(t);
                                n.x = s.x * r, n.y = s.y * r
                            } else this.randomPosOnLine(n), n.x = L.x * r, n.y = L.y * r;
                        else this.randomPosInPolygon(n), n.x = n.x * r, n.y = n.y * r;
                        (i = i || L).x = n.x, i.y = n.y;
                        var o = e.system,
                            a = f(o, i);
                        return i.x = a.x, i.y = a.y, i
                    }, o.randomPosOnLine = function(e) {
                        var t = this._bezierCurve.getRandomPos();
                        return e.x = t.x, e.y = t.y, e
                    }, o.triangulate = function() {
                        if (this._triangleIndices.length = 0, !(this.poses.length < 3)) {
                            this._samplePoints.length = 0;
                            for (var e = this._bezierCurve.cacheTimes.length - 1, t = 0; t < e; t++) {
                                var i = this._bezierCurve.cacheTimes[t],
                                    n = this._bezierCurve.getPoint(i).clone();
                                this._samplePoints.push(n)
                            }
                            for (var r = [], s = 0; s < this._samplePoints.length; s++) r.push(s);
                            if (3 == r.length) this._triangleIndices.push(r);
                            else {
                                for (; r.length > 3;) {
                                    var o = this.findEar(r);
                                    if (o < 0) break;
                                    var a = [r[(o - 1 + r.length) % r.length], r[o], r[(o + 1) % r.length]];
                                    this._triangleIndices.push(a), r.splice(o, 1)
                                }
                                3 == r.length && this._triangleIndices.push(r)
                            }
                        }
                    }, o.findEar = function(e) {
                        for (var t = 0; t < e.length; t++) {
                            var i = this._samplePoints[e[(t - 1 + e.length) % e.length]],
                                n = this._samplePoints[e[t]],
                                r = this._samplePoints[e[(t + 1) % e.length]];
                            if (this.isTriangle(i, n, r)) {
                                for (var s = !0, o = 0; o < e.length; o++)
                                    if (o != t && o != (t - 1 + e.length) % e.length && o != (t + 1) % e.length && this.isPointInTriangle(this._samplePoints[e[o]], i, n, r)) {
                                        s = !1;
                                        break
                                    } if (s) return t
                            }
                        }
                        return -1
                    }, o.isPointInTriangle = function(e, t, i, n) {
                        var r = i.x - t.x,
                            s = i.y - t.y,
                            o = n.x - t.x,
                            a = n.y - t.y,
                            l = e.x - t.x,
                            u = e.y - t.y,
                            h = r * a - s * o;
                        if (0 == h) return !1;
                        var p = (a * l - s * u) / h,
                            c = (-o * l + r * u) / h;
                        return p >= 0 && c >= 0 && p + c <= 1
                    }, o.isTriangle = function(e, t, i) {
                        var n = t.x - e.x,
                            r = t.y - e.y,
                            s = i.x - e.x;
                        return n * (i.y - e.y) - r * s > 0
                    }, o.randomPosInPolygon = function(e) {
                        if (0 == this._triangleIndices.length) return e;
                        var t = Math.floor(Math.random() * this._triangleIndices.length),
                            i = this._triangleIndices[t],
                            n = this._samplePoints[i[0]],
                            r = this._samplePoints[i[1]],
                            s = this._samplePoints[i[2]],
                            o = r.x - n.x,
                            a = r.y - n.y,
                            l = s.x - n.x,
                            u = s.y - n.y,
                            h = Math.random(),
                            p = Math.random();
                        return h + p > 1 && (h = 1 - h, p = 1 - p), e.x = n.x + h * o + p * l, e.y = n.y + h * a + p * u, e
                    }, s(t, [{
                        key: "poses",
                        get: function() {
                            return this._poses
                        },
                        set: function(e) {
                            this._poses !== e && (this._poses = e, this._poses && this.onInit())
                        }
                    }, {
                        key: "online",
                        get: function() {
                            return this._online
                        },
                        set: function(e) {
                            this._online !== e && (this._online = e, this._online || this.triangulate())
                        }
                    }, {
                        key: "curveMode",
                        get: function() {
                            return this._curveMode
                        },
                        set: function(e) {
                            this._curveMode !== e && (this._curveMode = e, this._bezierCurve.curveMode = e, this._bezierCurve.reset(this.poses), this._online || this.triangulate())
                        }
                    }, {
                        key: "validPoses",
                        get: function() {
                            return this._poses && this._poses.length >= 2
                        }
                    }]), t
                }(d)).prototype, "_poses", [P], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return []
                    }
                }), t(w.prototype, "poses", [b], Object.getOwnPropertyDescriptor(w.prototype, "poses"), w.prototype), T = t(w.prototype, "scale", [x], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return new l
                    }
                }), O = t(w.prototype, "_online", [M], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return !0
                    }
                }), t(w.prototype, "online", [G], Object.getOwnPropertyDescriptor(w.prototype, "online"), w.prototype), R = t(w.prototype, "emitMode", [z], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return p.Random
                    }
                }), k = t(w.prototype, "expansion", [D], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return c.None
                    }
                }), B = t(w.prototype, "updateDirection", [G], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return !1
                    }
                }), j = t(w.prototype, "_curveMode", [E], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return !0
                    }
                }), t(w.prototype, "curveMode", [G], Object.getOwnPropertyDescriptor(w.prototype, "curveMode"), w.prototype), I = w)) || I)), new u);
            new u;
            o._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/PathOvertimeModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DModuleBase.ts", "./Curve.ts", "./EasyP2DUtils.ts"], (function(e) {
    var t, i, r, s, n, o, a, u, c, l, p, h, v, f;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, i = e.inheritsLoose, r = e.initializerDefineProperty, s = e.assertThisInitialized, n = e.createClass
        }, function(e) {
            o = e.cclegacy, a = e._decorator, u = e.CurveRange, c = e.CCInteger, l = e.Vec2
        }, function(e) {
            p = e.EasyP2DModuleBase
        }, function(e) {
            h = e.BezierPosition, v = e.Curve
        }, function(e) {
            f = e.calcValueOnCurve
        }],
        execute: function() {
            var _, b, y, d, m, g, M, z, D, P, C, T, w, x, O, R;
            o._RF.push({}, "0b33f6oJFVB0KTN0C5qk84r", "PathOvertimeModule", void 0);
            var B = a.property,
                E = a.ccclass,
                k = (e("PathOvertimeModule", (_ = E("PathOvertimeModule"), b = B({
                    type: [h],
                    visible: !1
                }), y = B({
                    type: [h],
                    visible: function() {
                        return this.enabled
                    }
                }), d = B({
                    type: u,
                    visible: function() {
                        return this.enabled && this.useTimeMode
                    },
                    range: [0, 1, .01]
                }), m = B({
                    type: c,
                    min: 1,
                    visible: function() {
                        return this.enabled && this.useTimeMode
                    }
                }), g = B({
                    visible: function() {
                        return this.enabled
                    }
                }), M = B({
                    type: u,
                    visible: function() {
                        return this.enabled && this.useTimeMode
                    }
                }), z = B({
                    visible: !1
                }), _((C = t((P = function(e) {
                    function t() {
                        var t;
                        return t = e.call(this) || this, r(t, "_poses", C, s(t)), r(t, "targetTimeRate", T, s(t)), r(t, "repeat", w, s(t)), r(t, "reverse", x, s(t)), r(t, "scale", O, s(t)), r(t, "_curveMode", R, s(t)), t.useTimeMode = !0, t._bezierCurve = new v, t._moveDistance = 0, t._lastPos = new l, t.targetTimeRate.constant = 1, t.scale.constant = 1, t.enableOnUpdate = !0, t.enableAfterEmitter = !0, t
                    }
                    i(t, e);
                    var o = t.prototype;
                    return o.onInit = function() {
                        this._bezierCurve.reset(this.poses)
                    }, o.afterEmitter = function(e, t) {
                        if (!(this._bezierCurve.points.length < 2)) {
                            e._targetTimeRate = f(this.targetTimeRate, t);
                            var i = e.fromPos,
                                r = this._bezierCurve.getPoint(0);
                            r && i && (e._path_offset || (e._path_offset = new l), e._path_offset.set(i.x - r.x, i.y - r.y), e.preventDefaultSetPos = !0, this.useTimeMode || (this._moveDistance = 0, this._lastPos.set(i.x, i.y)))
                        }
                    }, o.onUpdate = function(e, t, i) {
                        if (!(this._bezierCurve.points.length < 2))
                            if (this.useTimeMode) {
                                var r = f(this.scale, i),
                                    s = e._targetTimeRate <= 0 ? 1 : i / e._targetTimeRate;
                                this.reverse && (s = 1 - s), this.repeat > 0 && (s = s * this.repeat % 1);
                                var n = this._bezierCurve.getPoint(s);
                                if (!n || !e._path_offset) return;
                                e.pos.x = (n.x + e._path_offset.x) * r, e.pos.y = (n.y + e._path_offset.y) * r, e.preventDefaultSetPos = !0
                            } else {
                                var o = e.pos;
                                k.set(o.x, o.y), this._moveDistance += l.distance(k, this._lastPos), this._lastPos.set(o.x, o.y), this._moveDistance > this._bezierCurve.totalDistance && (this._moveDistance %= this._bezierCurve.totalDistance);
                                var a = this._bezierCurve.getTimeByDistance(this._moveDistance),
                                    u = this._bezierCurve.getTangent(a, this.reverse, t);
                                e.dir.x = u.x, e.dir.y = u.y, e.preventDefaultSetDir = !0
                            }
                    }, n(t, [{
                        key: "poses",
                        get: function() {
                            return this._poses
                        },
                        set: function(e) {
                            this._poses !== e && (this._poses = e, this._poses && this.onInit())
                        }
                    }, {
                        key: "curveMode",
                        get: function() {
                            return this._curveMode
                        },
                        set: function(e) {
                            this._curveMode !== e && (this._curveMode = e, this._bezierCurve.curveMode = e, this._bezierCurve.reset(this.poses))
                        }
                    }]), t
                }(p)).prototype, "_poses", [b], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return []
                    }
                }), t(P.prototype, "poses", [y], Object.getOwnPropertyDescriptor(P.prototype, "poses"), P.prototype), T = t(P.prototype, "targetTimeRate", [d], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return new u
                    }
                }), w = t(P.prototype, "repeat", [m], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return 1
                    }
                }), x = t(P.prototype, "reverse", [g], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return !1
                    }
                }), O = t(P.prototype, "scale", [M], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return new u
                    }
                }), R = t(P.prototype, "_curveMode", [z], {
                    configurable: !0,
                    enumerable: !0,
                    writable: !0,
                    initializer: function() {
                        return !0
                    }
                }), t(P.prototype, "curveMode", [B], Object.getOwnPropertyDescriptor(P.prototype, "curveMode"), P.prototype), D = P)) || D)), new l);
            o._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/SizeOvertimeModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DUtils.ts", "./EasyP2DModuleBase.ts"], (function(e) {
    var i, t, n, r, a, l, s, c, o;
    return {
        setters: [function(e) {
            i = e.applyDecoratedDescriptor, t = e.inheritsLoose, n = e.initializerDefineProperty, r = e.assertThisInitialized
        }, function(e) {
            a = e.cclegacy, l = e._decorator, s = e.CurveRange
        }, function(e) {
            c = e.calcValueOnCurve
        }, function(e) {
            o = e.EasyP2DModuleBase
        }],
        execute: function() {
            var u, b, f, h, p, m, v, y, z, d, S, g, w, X, Y, D, M, O, _;
            a._RF.push({}, "357a47s4WtF5aRY+Q+tA9X+", "SizeOvertimeModule", void 0);
            var I = l.property,
                P = l.ccclass;
            e("SizeOvertimeModule", (u = P("SizeOvertimeModule"), b = I({
                visible: function() {
                    return this.enabled
                }
            }), f = I({
                visible: function() {
                    return this.enabled
                }
            }), h = I({
                visible: function() {
                    return this.enabled
                }
            }), p = I({
                visible: function() {
                    return this.enabled && this.byIncrement
                }
            }), m = I({
                visible: function() {
                    return this.enabled
                }
            }), v = I({
                visible: function() {
                    return this.enabled && this.enableScaleX
                }
            }), y = I({
                visible: function() {
                    return this.enabled
                }
            }), z = I({
                visible: function() {
                    return this.enabled && this.enableScaleY
                }
            }), u((g = i((S = function(e) {
                function i() {
                    var i;
                    return i = e.call(this) || this, n(i, "isScale", g, r(i)), n(i, "size", w, r(i)), n(i, "byIncrement", X, r(i)), n(i, "increment", Y, r(i)), n(i, "enableScaleX", D, r(i)), n(i, "scaleX", M, r(i)), n(i, "enableScaleY", O, r(i)), n(i, "scaleY", _, r(i)), i.enableOnUpdate = !0, i.enableOnEmitter = !0, i.scaleX.constant = 1, i.scaleY.constant = 1, i.size.constant = 10, i
                }
                t(i, e);
                var a = i.prototype;
                return a.onEmitter = function(e, i) {
                    this.byIncrement && (e._increment = c(this.increment, i))
                }, a.onUpdate = function(e, i, t) {
                    var n = this._easyP2D.system,
                        r = !1;
                    if (this.byIncrement) e.size += e._increment * i, r = !0;
                    else {
                        var a = c(this.size, t);
                        this.isScale ? e.size = e.startSize * a : e.size = a, r = !0
                    }
                    var l = 1,
                        s = 1;
                    this.enableScaleX && (l = c(this.scaleX, t), r = !0), this.enableScaleY && (s = c(this.scaleY, t), r = !0), r && (e.halfWidth = e.size * n.halfwidth * l, e.halfHeight = e.size * n.halfheight * s)
                }, i
            }(o)).prototype, "isScale", [b], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !0
                }
            }), w = i(S.prototype, "size", [f], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new s
                }
            }), X = i(S.prototype, "byIncrement", [h], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), Y = i(S.prototype, "increment", [p], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new s
                }
            }), D = i(S.prototype, "enableScaleX", [m], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), M = i(S.prototype, "scaleX", [v], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new s
                }
            }), O = i(S.prototype, "enableScaleY", [y], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), _ = i(S.prototype, "scaleY", [z], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new s
                }
            }), d = S)) || d));
            a._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/SpeedOvertimeModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DUtils.ts", "./EasyP2DModuleBase.ts"], (function(e) {
    var t, n, i, r, a, l, s, c, o;
    return {
        setters: [function(e) {
            t = e.applyDecoratedDescriptor, n = e.inheritsLoose, i = e.initializerDefineProperty, r = e.assertThisInitialized
        }, function(e) {
            a = e.cclegacy, l = e._decorator, s = e.CurveRange
        }, function(e) {
            c = e.calcValueOnCurve
        }, function(e) {
            o = e.EasyP2DModuleBase
        }],
        execute: function() {
            var u, p, b, d, f, m, v, y, h, S, g, w, M;
            a._RF.push({}, "3148b0LwtFGpakMXJCPjiS/", "SpeedOvertimeModule", void 0);
            var z = l.property,
                D = l.ccclass;
            e("SpeedOvertimeModule", (u = D("SpeedOvertimeModule"), p = z({
                visible: function() {
                    return this.enabled && !this.byIncrement
                }
            }), b = z({
                visible: function() {
                    return this.enabled
                }
            }), d = z({
                visible: function() {
                    return this.enabled
                }
            }), f = z({
                visible: function() {
                    return this.enabled && this.byIncrement
                }
            }), m = z({
                visible: function() {
                    return this.enabled
                }
            }), u((h = t((y = function(e) {
                function t() {
                    var t;
                    return t = e.call(this) || this, i(t, "speed", h, r(t)), i(t, "isScale", S, r(t)), i(t, "byIncrement", g, r(t)), i(t, "increment", w, r(t)), i(t, "scale", M, r(t)), t.enableOnUpdate = !0, t.enableOnEmitter = !0, t.scale.constant = 1, t
                }
                n(t, e);
                var a = t.prototype;
                return a.onEmitter = function(e, t) {
                    this.byIncrement && (e._increment = c(this.increment, t))
                }, a.onUpdate = function(e, t, n) {
                    if (this.byIncrement) e.speed = e.speed + e._increment * t;
                    else {
                        var i = c(this.speed, n);
                        this.isScale ? e.speed = i * e.startSpeed : e.speed = i
                    }
                    e.speed *= c(this.scale, n), e.preventDefaultSetSpeed = !0
                }, t
            }(o)).prototype, "speed", [p], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new s
                }
            }), S = t(y.prototype, "isScale", [b], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !0
                }
            }), g = t(y.prototype, "byIncrement", [d], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), w = t(y.prototype, "increment", [f], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new s
                }
            }), M = t(y.prototype, "scale", [m], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new s
                }
            }), v = y)) || v));
            a._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/SystemAngleOvertimeModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DUtils.ts", "./EasyP2DModuleBase.ts", "./EasyP2DDefine.ts"], (function(t) {
    var e, i, n, o, r, a, s, l, c, u, d, f, y, p, h, m;
    return {
        setters: [function(t) {
            e = t.applyDecoratedDescriptor, i = t.inheritsLoose, n = t.initializerDefineProperty, o = t.assertThisInitialized, r = t.createClass
        }, function(t) {
            a = t.cclegacy, s = t._decorator, l = t.Vec2, c = t.math, u = t.CurveRange
        }, function(t) {
            d = t.calcValueOnCurve, f = t.calcCos, y = t.calcSin
        }, function(t) {
            p = t.EasyP2DModuleBase
        }, function(t) {
            h = t.SpaceMode, m = t.EmitterMode
        }],
        execute: function() {
            var b, R, w, v, P, _, g, x, M, D, E, I, O, z;
            a._RF.push({}, "2ad7c/Xr7ZPhbsbU4wpx42g", "SystemAngleOvertimeModule", void 0);
            var A = s.property,
                S = s.ccclass,
                T = new l;
            t("SystemAngleOvertimeModule", (b = S("SystemAngleOvertimeModule"), R = A({
                visible: function() {
                    return this.enabled && this.space != h.World
                },
                range: [-360, 360]
            }), w = A({
                visible: !1
            }), v = A({
                visible: function() {
                    return this.enabled
                }
            }), P = A({
                visible: function() {
                    return this.enabled
                }
            }), _ = A({
                visible: function() {
                    return this.enabled && this.space != h.World
                }
            }), g = A({
                type: h,
                visible: function() {
                    return this.enabled
                }
            }), b((D = e((M = function(t) {
                function e() {
                    var e;
                    return e = t.call(this) || this, n(e, "angle", D, o(e)), n(e, "_increment", E, o(e)), n(e, "scale", I, o(e)), n(e, "byIncrement", O, o(e)), n(e, "space", z, o(e)), e.cosRotation = 0, e.sinRotation = 0, e._worldRotation = 0, e.enableAfterUpdate = !0, e.enableOnEmitter = !0, e.scale.constant = 1, e
                }
                i(e, t);
                var a = e.prototype;
                return a.onReset = function() {
                    this._worldRotation = 0
                }, a.onEmitter = function(t, e) {
                    t.worldRotation = 0, this._easyP2D.system.emitterMode != m.TARGET && this.byIncrement && (t._incrementValue = c.toRadian(d(this._increment, e)))
                }, a.afterUpdate = function(t, e, i, n) {
                    if (this._easyP2D.system.emitterMode != m.TARGET)
                        if (this.space == h.World) {
                            if (0 == this._easyP2D.simulator.stepIndex) {
                                var o = c.toRadian(d(this._increment, 0));
                                this._worldRotation += o * e, this._worldRotation %= 2 * Math.PI, this.cosRotation = f(this._worldRotation), this.sinRotation = y(this._worldRotation)
                            }
                            T.x = t.finalPos.x, T.y = t.finalPos.y, t.finalPos.x = T.x * this.cosRotation - T.y * this.sinRotation, t.finalPos.y = T.x * this.sinRotation + T.y * this.cosRotation
                        } else {
                            if (t.worldRotation = t.worldRotation || 0, this.byIncrement) {
                                var r = d(this.scale, i);
                                t.worldRotation += t._incrementValue * e * r, t.worldRotation %= 2 * Math.PI
                            } else {
                                var a = d(this.angle, i);
                                t.worldRotation = c.toRadian(a), t.worldRotation *= d(this.scale, i)
                            }
                            var s = f(t.worldRotation),
                                l = y(t.worldRotation);
                            T.x = t.finalPos.x, T.y = t.finalPos.y, t.finalPos.x = T.x * s - T.y * l, t.finalPos.y = T.x * l + T.y * s
                        }
                }, r(e, [{
                    key: "increment",
                    get: function() {
                        return this._increment
                    },
                    set: function(t) {
                        this._increment = t
                    }
                }]), e
            }(p)).prototype, "angle", [R], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new u
                }
            }), E = e(M.prototype, "_increment", [w], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new u
                }
            }), e(M.prototype, "increment", [v], Object.getOwnPropertyDescriptor(M.prototype, "increment"), M.prototype), I = e(M.prototype, "scale", [P], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new u
                }
            }), O = e(M.prototype, "byIncrement", [_], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), z = e(M.prototype, "space", [g], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return h.Local
                }
            }), x = M)) || x));
            a._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/TimerModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DUtils.ts", "./EasyP2DModuleBase.ts"], (function(e) {
    var i, t, n, r, a, u, l, o, s, c;
    return {
        setters: [function(e) {
            i = e.applyDecoratedDescriptor, t = e.inheritsLoose, n = e.initializerDefineProperty, r = e.assertThisInitialized, a = e.createClass
        }, function(e) {
            u = e.cclegacy, l = e._decorator, o = e.CurveRange
        }, function(e) {
            s = e.calcValueOnCurve
        }, function(e) {
            c = e.EasyP2DModuleBase
        }],
        execute: function() {
            var p, m, b, f, d, T, h, w, y, v, D, S, g, z, _, O, k, E, M;
            u._RF.push({}, "5bb65JWz1lMfoJHN0971pGD", "TimerModule", void 0);
            var P = l.property,
                R = l.ccclass;
            e("TimerModule", (p = R("TimerModule"), m = P({
                visible: function() {
                    return this.enabled
                }
            }), b = P({
                visible: function() {
                    return this.enabled
                }
            }), f = P({
                visible: function() {
                    return this.enabled
                }
            }), d = P({
                visible: function() {
                    return this.enabled
                }
            }), T = P({
                visible: function() {
                    return this.enabled
                }
            }), h = P({
                visible: function() {
                    return this.enabled
                }
            }), w = P({
                visible: function() {
                    return this.enbleTimeSwap
                }
            }), y = P({
                visible: function() {
                    return this.enbleTimeSwap
                }
            }), p((S = i((D = function(e) {
                function i() {
                    var i;
                    return i = e.call(this) || this, n(i, "delay", S, r(i)), n(i, "timeOffset", g, r(i)), n(i, "duration", z, r(i)), n(i, "emitTurnDuration", _, r(i)), n(i, "updateSwapTimeOnEmit", O, r(i)), i.awakeTime = 0, n(i, "_enbleTimeSwap", k, r(i)), n(i, "turnAwakeTime", E, r(i)), i.sleepTime = 0, n(i, "turnSleepTime", M, r(i)), i.enableOnUpdate = !0, i.delay.constant = 0, i.duration.constant = 1, i.emitTurnDuration.constant = 1, i
                }
                t(i, e);
                var u = i.prototype;
                return u.onReset = function() {
                    var e = this._easyP2D.system,
                        i = s(this.timeOffset, 0);
                    i > 0 && this._easyP2D.simulator.warm(i), e.realDelay = s(this.delay, 0), e.realDuration = s(this.duration, 0), e.realEmitTurnDuration = s(this.emitTurnDuration, 0), this.updateTime()
                }, u.updateTime = function() {
                    this._enabled ? this.updateSwapTimeOnEmit && (this.awakeTime = s(this.turnAwakeTime, 0), this.sleepTime = s(this.turnSleepTime, 0)) : (this.awakeTime = 0, this.sleepTime = 0)
                }, u.onUpdate = function(e, i, t) {
                    this.updateSwapTimeOnEmit && this.updateTime()
                }, a(i, [{
                    key: "enbleTimeSwap",
                    get: function() {
                        return this._enabled && this._enbleTimeSwap
                    }
                }]), i
            }(c)).prototype, "delay", [m], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), g = i(D.prototype, "timeOffset", [b], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), z = i(D.prototype, "duration", [f], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), _ = i(D.prototype, "emitTurnDuration", [d], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), O = i(D.prototype, "updateSwapTimeOnEmit", [T], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), k = i(D.prototype, "_enbleTimeSwap", [h], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), E = i(D.prototype, "turnAwakeTime", [w], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), M = i(D.prototype, "turnSleepTime", [y], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new o
                }
            }), v = D)) || v));
            u._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/TrailCreator.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DUtils.ts"], (function(t) {
    var s, e, i, l, r, a, o, c, n, h, p, u;
    return {
        setters: [function(t) {
            s = t.createClass
        }, function(t) {
            e = t.cclegacy, i = t.Enum, l = t.Pool, r = t.Color, a = t.Vec2, o = t.Vec3, c = t.CurveRange, n = t.math, h = t.GradientRange
        }, function(t) {
            p = t.calcColorOnCurve, u = t.calcValueOnCurve
        }],
        execute: function() {
            e._RF.push({}, "2f417NRJPBM2YBHQ1cTKKCu", "TrailCreator", void 0);
            t("TrailMode", i({
                TRAIL: 0,
                PARTICLE: 1
            }));
            var v = function() {
                    this.pos = new o, this.lastPos = new o, this.verticles = [new o, new o, new o, new o], this.uvs = [new a, new a, new a, new a], this.colors = [new r, new r, new r, new r], this.time = 0, this.distance = 0
                },
                f = new l((function() {
                    return new r
                }), 32),
                _ = new l((function() {
                    return new a
                }), 32),
                y = new l((function() {
                    return new o
                }), 32),
                m = new l((function() {
                    return new v
                }), 32),
                x = [0, 1, 1, 1, 0, 0, 1, 0];
            t("TRAIL_INDICES", [0, 1, 2, 1, 3, 2]);

            function P(t) {
                return t >= 1e-5 ? 1 : -1
            }
            t("TrailCreator", function() {
                function t() {
                    this.normal = new o(0, 0, 1), this.minLength = .1, this.colorGradient = new h, this.widthCurve = new c, this.particleAliveTime = .2, this.spriteFrame = null, this.maxParticleCount = 6, this.useVerticalUVLayout = !0, this.uv = null, this.sizeScale = 1, this.tintColor = new r(255, 255, 255, 255), this._particles = [], this._segmentSqrLen = 0, this._lastCount = 0, this._isConstWidth = !0, this._recycleSpeed = .1, this._totalDistance = 0, this._lastMoveTime = -1, this._vertices = [], this._uvs = [], this._colors = []
                }
                var e = t.prototype;
                return e.clear = function() {
                    for (var t = 0; t < this._particles.length; t++) m.free(this._particles[t]);
                    this._particles.length = 0;
                    for (var s = 0; s < this._vertices.length; s++) _.free(this._vertices[s]);
                    this._vertices.length = 0;
                    for (var e = 0; e < this._uvs.length; e++) _.free(this._uvs[e]);
                    this._uvs.length = 0;
                    for (var i = 0; i < this._colors.length; i++) f.free(this._colors[i]);
                    this._colors.length = 0
                }, e.updateColors = function() {
                    for (var t = this.particles.length, s = 0; s < t; s++) {
                        var e = s / (t + 1),
                            i = p(this.colorGradient, e),
                            l = f.alloc();
                        l.set(i), l.multiply(this.tintColor);
                        var r = this._particles[s];
                        if (0 == s) r.colors[0].set(l), r.colors[1].set(l), e = (s + 1) / (t + 1), l = p(this.colorGradient, e), r.colors[2].set(l), r.colors[3].set(l);
                        else {
                            var a = this._particles[s - 1];
                            r.colors[0].set(a.colors[2]), r.colors[1].set(a.colors[3]), r.colors[2].set(l), r.colors[3].set(l)
                        }
                    }
                }, e.updateUVs = function() {
                    var t = this.uv;
                    t || (this.spriteFrame && (t = this.spriteFrame.uv), t || (t = x));
                    var s = t[0],
                        e = t[5],
                        i = t[2],
                        l = t[3],
                        r = this.spriteFrame && this.spriteFrame.rotated;
                    r && (s = t[0], e = t[1], i = t[4], l = t[3]);
                    var a = this._particles.length;
                    if (this.useVerticalUVLayout && !r)
                        for (var o = a > 1 ? (l - e) / (a - 1) : l - e, c = 0; c < a; c++) {
                            var n = this._particles[c],
                                h = e + o * c;
                            if (0 == c) n.uvs[0].set(s, h), n.uvs[1].set(i, h);
                            else {
                                var p = this._particles[c - 1];
                                n.uvs[0].set(p.uvs[2]), n.uvs[1].set(p.uvs[3])
                            }
                            h = e + o * (c + 1), n.uvs[2].set(s, h), n.uvs[3].set(i, h)
                        } else
                            for (var u = a > 1 ? (i - s) / (a - 1) : i - s, v = 0; v < a; v++) {
                                var f = this._particles[v],
                                    _ = s + u * v;
                                if (0 == v) f.uvs[0].set(_, e), f.uvs[1].set(_, l);
                                else {
                                    var y = this._particles[v - 1];
                                    f.uvs[0].set(y.uvs[2]), f.uvs[1].set(y.uvs[3])
                                }
                                _ = s + u * (v + 1), f.uvs[2].set(_, e), f.uvs[3].set(_, l)
                            }
                }, e.reset = function() {
                    this.clear(), this._recycleSpeed = this.minLength / this.particleAliveTime, this._segmentSqrLen = this.minLength * this.minLength, this._isConstWidth = this.widthCurve.mode === c.Mode.Constant, this.sizeScale = 1, this.tintColor.set(255, 255, 255, 255)
                }, e.updateParticle = function(t) {
                    if (0 != this._particles.length) {
                        var s = this._particles[this._particles.length - 1];
                        s.time += t;
                        var e = y.alloc(),
                            i = s.time >= .1 && (1 == this._particles.length && this._lastMoveTime >= this.particleAliveTime || this._particles.length > 1);
                        e.set(s.pos.x - s.lastPos.x, s.pos.y - s.lastPos.y);
                        var l = e.lengthSqr();
                        if (l > .01 && i) {
                            var r = Math.min(this._recycleSpeed * t, Math.sqrt(l));
                            e.normalize();
                            var a = y.alloc().set(s.lastPos).add(e.multiplyScalar(r));
                            s.lastPos.set(a), y.free(a), l = r * r
                        }
                        if (l <= this.minLength && s.time >= this.particleAliveTime) {
                            var o = this._particles.pop();
                            m.free(o)
                        }
                        y.free(e)
                    }
                }, e.updateVertices = function(t) {
                    if (0 != this._particles.length)
                        if (1 == this._particles.length) {
                            var s = this._particles[t],
                                e = y.alloc().set(s.pos.x - s.lastPos.x, s.pos.y - s.lastPos.y, 0).normalize(),
                                i = y.alloc();
                            o.cross(i, e, this.normal);
                            var l = .5 * (u(this.widthCurve, 0) * this.sizeScale);
                            i.multiplyScalar(l);
                            var r = s.pos.x + i.x,
                                a = s.pos.y + i.y,
                                c = s.pos.x - i.x,
                                h = s.pos.y - i.y,
                                p = s.lastPos.x + i.x,
                                v = s.lastPos.y + i.y,
                                f = s.lastPos.x - i.x,
                                _ = s.lastPos.y - i.y;
                            s.verticles[0].set(r, a), s.verticles[1].set(c, h), s.verticles[2].set(p, v), s.verticles[3].set(f, _), y.free(e), y.free(i)
                        } else {
                            var m = this._particles[t],
                                x = t / (this._particles.length - 1),
                                g = .5 * (u(this.widthCurve, x) * this.sizeScale);
                            if (0 == t) {
                                var d = this._particles[t + 1],
                                    w = y.alloc(),
                                    C = y.alloc().set(m.pos.x - m.lastPos.x, m.pos.y - m.lastPos.y, 0).normalize(),
                                    S = y.alloc().set(d.pos.x - d.lastPos.x, d.pos.y - d.lastPos.y, 0).normalize(),
                                    z = o.cross(y.alloc(), S, C),
                                    T = P(z.z);
                                n.equals(z.z, 0) ? o.cross(w, C, this.normal) : w.set(S).subtract(C).normalize().multiplyScalar(T), w.normalize().multiplyScalar(g);
                                var L = m.pos.x + w.x,
                                    V = m.pos.y + w.y,
                                    M = m.pos.x - w.x,
                                    q = m.pos.y - w.y,
                                    R = m.lastPos.x + w.x,
                                    A = m.lastPos.y + w.y,
                                    F = m.lastPos.x - w.x,
                                    U = m.lastPos.y - w.y;
                                m.verticles[0].set(L, V), m.verticles[1].set(M, q), m.verticles[2].set(R, A), m.verticles[3].set(F, U), y.free(w), y.free(C), y.free(S), y.free(z)
                            } else {
                                var I = this._particles[t - 1],
                                    b = y.alloc().set(m.pos.x - m.lastPos.x, m.pos.y - m.lastPos.y, 0).normalize(),
                                    k = y.alloc().set(I.pos.x - I.lastPos.x, I.pos.y - I.lastPos.y, 0).normalize(),
                                    E = y.alloc(),
                                    G = o.cross(y.alloc(), b, k),
                                    B = P(G.z);
                                n.equals(G.z, 0) ? o.cross(E, b, this.normal) : E.set(b).subtract(k).normalize().multiplyScalar(B), E.multiplyScalar(g);
                                var D = m.pos.x + E.x,
                                    H = m.pos.y + E.y,
                                    K = m.pos.x - E.x,
                                    N = m.pos.y - E.y,
                                    O = m.lastPos.x + E.x,
                                    W = m.lastPos.y + E.y,
                                    j = m.lastPos.x - E.x,
                                    J = m.lastPos.y - E.y;
                                m.verticles[0].set(D, H), m.verticles[1].set(K, N), I.verticles[2].set(D, H), I.verticles[3].set(K, N), m.verticles[2].set(O, W), m.verticles[3].set(j, J), y.free(E), y.free(b), y.free(k), y.free(G)
                            }
                        }
                }, e.updatePos = function(t, s) {
                    var e = !1,
                        i = !1;
                    if (0 == this._particles.length) {
                        var l = m.alloc();
                        l.time = 0, l.pos.set(s.x, s.y, 0), l.lastPos.set(s.x, s.y, 0), this._particles.push(l), i = !0
                    } else {
                        this._particles[0].pos.set(s.x, s.y, 0), e = !0
                    }
                    var r = this._particles[0],
                        a = y.alloc().set(r.pos).subtract(r.lastPos),
                        o = a.lengthSqr();
                    if (!i && o > .01) {
                        if (o >= this._segmentSqrLen)
                            if (this._particles.length < this.maxParticleCount) this.addParticle(s);
                            else {
                                var c = this._particles.pop();
                                m.free(c)
                            } e = !0, this._lastMoveTime = 0
                    }
                    if (this._lastCount = this._particles.length, e) {
                        this.updateParticle(t), this.updateColors(), this.updateUVs();
                        for (var n = 0; n < this._particles.length; n++) this.updateVertices(n)
                    } else this.uv && this.updateUVs();
                    this._lastMoveTime += t, y.free(a)
                }, e.addParticle = function(t) {
                    var s = this._particles.shift(),
                        e = m.alloc();
                    e.time = 0, e.pos.set(s.pos), e.lastPos.set(s.lastPos), this._particles.unshift(e), s.pos.set(t.x, t.y, 0), s.lastPos.set(t.x, t.y, 0), s.time = 0, this._particles.unshift(s)
                }, s(t, [{
                    key: "particleCount",
                    get: function() {
                        return this.particles.length
                    }
                }, {
                    key: "vertexCount",
                    get: function() {
                        return 4 * this.particles.length
                    }
                }, {
                    key: "particles",
                    get: function() {
                        return this._particles
                    }
                }]), t
            }());
            e._RF.pop()
        }
    }
}));

System.register("chunks:///_virtual/TrailModule.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EasyP2DModuleBase.ts", "./TrailCreator.ts", "./EasyP2DDefine.ts", "./EasyP2DUtils.ts"], (function(e) {
    var i, t, r, n, a, l, o, u, s, c, f, b, p, h, d, m, v;
    return {
        setters: [function(e) {
            i = e.applyDecoratedDescriptor, t = e.inheritsLoose, r = e.initializerDefineProperty, n = e.assertThisInitialized
        }, function(e) {
            a = e.cclegacy, l = e._decorator, o = e.Pool, u = e.Vec2, s = e.Vec3, c = e.CCInteger, f = e.GradientRange, b = e.CurveRange, p = e.SpriteFrame
        }, function(e) {
            h = e.EasyP2DModuleBase
        }, function(e) {
            d = e.TrailCreator
        }, function(e) {
            m = e.SpaceMode
        }, function(e) {
            v = e.calcValueOnCurve
        }],
        execute: function() {
            var y, P, g, w, z, C, S, F, A, T, V, L, M, D, E, O, R, U, _, x, G, B, I, j, k;
            a._RF.push({}, "149a4CfqzhI9ZXoiOsARRlO", "TrailModule", void 0);
            var q = l.property,
                H = l.ccclass,
                W = new o((function() {
                    return new d
                }), 10),
                X = new u;
            new s, e("TrailModule", (y = H("TrailModule"), P = q({
                type: c,
                visible: function() {
                    return this.enabled
                }
            }), g = q({
                visible: function() {
                    return this.enabled
                }
            }), w = q({
                type: f,
                visible: function() {
                    return this.enabled
                }
            }), z = q({
                type: b,
                visible: function() {
                    return this.enabled
                }
            }), C = q({
                type: p,
                visible: function() {
                    return this.enabled && !this.useParticleSpriteFrame
                }
            }), S = q({
                visible: function() {
                    return this.enabled
                }
            }), F = q({
                visible: function() {
                    return this.enabled
                }
            }), A = q({
                visible: function() {
                    return this.enabled
                }
            }), T = q({
                visible: function() {
                    return this.enabled
                }
            }), V = q({
                visible: function() {
                    return this.enabled
                }
            }), L = q({
                visible: function() {
                    return this.enabled
                }
            }), y((E = i((D = function(e) {
                function i() {
                    var i;
                    return i = e.call(this) || this, r(i, "maxParticleCount", E, n(i)), r(i, "minLength", O, n(i)), r(i, "colorGradient", R, n(i)), r(i, "widthCurve", U, n(i)), r(i, "renderFrame", _, n(i)), r(i, "particleAliveTime", x, n(i)), r(i, "showMainParticle", G, n(i)), r(i, "useVerticalUVLayout", B, n(i)), r(i, "useParticleSpriteFrame", I, n(i)), r(i, "useParticleColor", j, n(i)), r(i, "useParticleSize", k, n(i)), i._startSize = 0, i.widthCurve.constant = 10, i.enableAfterEmitter = !0, i.enableAfterUpdate = !0, i.enableOnEnd = !0, i
                }
                t(i, e);
                var a = i.prototype;
                return a.getParticleCount = function(e) {
                    return e.trail ? e.trail.particleCount : 0
                }, a.afterEmitter = function(e, i) {
                    var t = e.trail = W.alloc();
                    if (t) {
                        var r = e.sys;
                        t.minLength = this.minLength, t.colorGradient = this.colorGradient, t.widthCurve = this.widthCurve, t.particleAliveTime = this.particleAliveTime, t.useVerticalUVLayout = this.useVerticalUVLayout, t.reset(), this._startSize = e.size || v(r.startSize, 0) || 1, this.useParticleSpriteFrame ? t.spriteFrame = e.sys.spriteFrame : t.spriteFrame = this.renderFrame, t.maxParticleCount = this.maxParticleCount
                    }
                }, a.afterUpdate = function(e, i, t, r) {
                    var n = e.trail;
                    if (n) {
                        var a = e.sys;
                        this.useParticleSpriteFrame ? n.uv = e.uv : n.uv = null, a.useSysAngleOvertime && a.sysAngleOvertime.space == m.World && (a.sysAngleOvertime.cosRotation, a.sysAngleOvertime.sinRotation), this.useParticleSize && (n.sizeScale = e.size / this._startSize), this.useParticleColor && n.tintColor.set(255 * e.color[0], 255 * e.color[1], 255 * e.color[2], 255 * e.color[3]);
                        var l = e.finalPos.x,
                            o = e.finalPos.y;
                        X.set(l, o), n.updatePos(i, X)
                    }
                }, a.onEnd = function(e) {
                    W.free(e.trail), e.trail = null
                }, i
            }(h)).prototype, "maxParticleCount", [P], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 6
                }
            }), O = i(D.prototype, "minLength", [g], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return 2
                }
            }), R = i(D.prototype, "colorGradient", [w], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new f
                }
            }), U = i(D.prototype, "widthCurve", [z], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return new b
                }
            }), _ = i(D.prototype, "renderFrame", [C], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return null
                }
            }), x = i(D.prototype, "particleAliveTime", [S], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return .2
                }
            }), G = i(D.prototype, "showMainParticle", [F], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !0
                }
            }), B = i(D.prototype, "useVerticalUVLayout", [A], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !0
                }
            }), I = i(D.prototype, "useParticleSpriteFrame", [T], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), j = i(D.prototype, "useParticleColor", [V], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), k = i(D.prototype, "useParticleSize", [L], {
                configurable: !0,
                enumerable: !0,
                writable: !0,
                initializer: function() {
                    return !1
                }
            }), M = D)) || M));
            a._RF.pop()
        }
    }
}));

(function(r) {
    r('virtual:///prerequisite-imports/main', 'chunks:///_virtual/main');
})(function(mid, cid) {
    System.register(mid, [cid], function(_export, _context) {
        return {
            setters: [function(_m) {
                var _exportObj = {};

                for (var _key in _m) {
                    if (_key !== "default" && _key !== "__esModule") _exportObj[_key] = _m[_key];
                }

                _export(_exportObj);
            }],
            execute: function() {}
        };
    });
});