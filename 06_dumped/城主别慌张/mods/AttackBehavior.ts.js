/* ===== chunks:///_virtual/AttackBehavior.ts ===== */
System.register("chunks:///_virtual/AttackBehavior.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./BattleConst.ts", "./Core.ts", "./Direction.ts", "./PointUtil.ts", "./Behavior.ts", "./ActName.ts", "./UnitHpComponent.ts", "./UnitBuffComponent.ts", "./ScenePool.ts", "./BaseArray.ts", "./SceneDef.ts", "./SkillDef.ts", "./UnitDataComponent.ts", "./BulletMgrComponent.ts", "./SkillEndMgrComponent.ts", "./Handler.ts", "./UnitSkillLvShowMgrComponent.ts", "./SoundComponent.ts", "./UnitAngleComponent.ts", "./FightDataComponent.ts", "./FightDef.ts"], (function(t) {
    var i, e, n, l, o, s, r, a, f, c, h, u, p, v, d, g, y, m, k, S, C, T, E, A, D, M, _, w;
    return {
        setters: [function(t) {
            i = t.inheritsLoose, e = t.createForOfIteratorHelperLoose
        }, function(t) {
            n = t.cclegacy, l = t._decorator
        }, function(t) {
            o = t.UnityType
        }, function(t) {
            s = t.xy
        }, function(t) {
            r = t.Direction, a = t.Direction5Map
        }, function(t) {
            f = t.PointUtil
        }, function(t) {
            c = t.Behavior
        }, function(t) {
            h = t.ActName
        }, function(t) {
            t.UnitHpComponent
        }, function(t) {
            u = t.UnitBuffComponent
        }, function(t) {
            p = t.ScenePool
        }, function(t) {
            v = t.BaseArray
        }, function(t) {
            d = t.IntervalTime2, g = t.MonsterFlag
        }, function(t) {
            y = t.SkillEffectShowType, m = t.SkillProcessType, k = t.SkillProcessTiming
        }, function(t) {
            S = t.UnitDataComponent
        }, function(t) {
            C = t.BulletMgrComponent
        }, function(t) {
            T = t.SkillEndMgrComponent
        }, function(t) {
            E = t.Handler
        }, function(t) {
            A = t.UnitSkillLvShowMgrComponent
        }, function(t) {
            D = t.SoundComponent
        }, function(t) {
            M = t.UnitAngleComponent
        }, function(t) {
            _ = t.FightDataComponent
        }, function(t) {
            w = t.ClientUiFightType
        }],
        execute: function() {
            var U;
            n._RF.push({}, "57657PB7LpJ7ZYRYSFMMeZ2", "AttackBehavior", void 0);
            t("AttackBehavior", l.ccclass("AttackBehavior")(U = function(t) {
                function n() {
                    for (var i, e = arguments.length, n = new Array(e), l = 0; l < e; l++) n[l] = arguments[l];
                    return (i = t.call.apply(t, [this].concat(n)) || this).curTarget = void 0, i.entity = void 0, i.skillId = void 0, i.attSpeed = void 0, i.attackDir = void 0, i.isNoBreak = void 0, i.skillArray = void 0, i.curElapseTime = void 0, i.loopTime = void 0, i.loopAllTime = void 0, i.progressType = void 0, i.unitSkillLvShowMgrComponent = void 0, i
                }
                i(n, t);
                var l = n.prototype;
                return l.onRecycle = function() {
                    if (this.curTarget = void 0, this.skillId = void 0, this.attSpeed = void 0, this.curElapseTime = void 0, this.loopTime = void 0, this.loopAllTime = void 0, this.attackDir = void 0, this.isNoBreak = void 0, this.progressType = void 0, this.entity) {
                        var i, e = this.entity.getComponent(S);
                        if (null != e && e.unitData) null == (i = this.unitSkillLvShowMgrComponent) || i.removeProgress(e.unitData.teamId, this.entity.pos)
                    }
                    this.unitSkillLvShowMgrComponent = void 0, this.skillArray && (p.recycle(this.skillArray), this.skillArray = void 0), t.prototype.onRecycle.call(this)
                }, l.setData = function(t, i, e, n) {
                    var l = this;
                    l.curTarget = t, l.skillId = i, l.attSpeed = e, l.attackDir = n
                }, l.onStart = function() {
                    t.prototype.onStart.call(this);
                    var i, e, n = this,
                        l = n.entity;
                    if (n.unitSkillLvShowMgrComponent = l.getRoot().getComponent(A), null != n.skillId) {
                        var o, f = s.cfg.get("SkillEffect.EffectConfig").get(this.skillId),
                            c = p.fetch(v);
                        if (this.curElapseTime = 0, f)
                            for (var u = 1; u < 10 && f.has(u); u++) {
                                var g = f.get(u);
                                if (g.skill_action) {
                                    var m = p.fetch(v),
                                        k = c[c.length - 1],
                                        C = k ? +k[1] : 0;
                                    m.push(g), m.push((g.delay_play ? g.delay_play * d : 0) + C);
                                    var T = void 0;
                                    if (null != g.effect_show) {
                                        var E = g.effect_show[y.ActLoop];
                                        if (null != E) l.getComponent(S).unitData.hasSkill(E[0][0], E[0][1]) && (this.isNoBreak = !0, T = 999999999999)
                                    }
                                    null == T && (T = g.continued_act ? g.continued_act * d : void 0), m.push(T), c.push(m)
                                }
                                g.up_model && (o = g.up_model)
                            }
                        this.skillArray = c, c && c.length > 0 ? this.playOne() : i = h.Attack, o && (e = i, this.entity.topDepth = 1)
                    } else i = h.Attack;
                    if (null != i && l.setAct(i, this.attSpeed), null != this.attackDir ? this.entity.dir = 1 == this.attackDir ? r.LEFT_UP : r.RIGHT_UP : n.checkDir(), null != e) {
                        var D, M = this.entity.dir;
                        e = (null != (D = a[M]) ? D : M) + "_" + e, this.entity.tmpCompleteActName = e
                    } else this.entity.topDepth;
                    null != this.skillArray && 0 != this.skillArray.length || n.finish()
                }, l.checkDir = function() {
                    var t = this.entity,
                        i = this.curTarget;
                    if (null != i && i != this.entity) {
                        var e = t.x,
                            n = t.y,
                            l = i.x,
                            a = i.y,
                            c = this.entity;
                        if (c.unitType != o.Player && c.unitType != o.RedGrid && c.unitType != o.God)
                            if (c.unitType == o.Partner) c.dir = e > l ? r.LEFT_UP : r.RIGHT_UP;
                            else if (c.unitType == o.CallUnit) {
                            var h, u = l - e,
                                p = a - n,
                                v = Math.atan2(p, u),
                                d = v * f.DEG - 90;
                            this.entity.addOrGetComponent(M).angle = d;
                            var y = f.directionRadian(v),
                                m = null == (h = c.getComponent(S)) || null == (h = h.unitData) ? void 0 : h.model,
                                k = m ? s.cfg.get("Monster.Monster").get(m) : void 0;
                            !!k && 0 != (k.flag & g.CallAngle) && y == c.dir ? (c.partMgr.updateDir(y), c.partMgr.setScale(1)) : c.dir = y
                        }
                    }
                }, l.onFinish = function() {
                    t.prototype.onFinish.call(this), this.entity.onAttackEnd()
                }, l.onUpdate = function(i) {
                    if (t.prototype.onUpdate.call(this, i), null != this.curElapseTime)
                        if (this.curElapseTime += i, this.skillArray && this.skillArray.length)
                            if (null != this.loopTime)
                                if (this.loopTime -= i, this.loopTime <= 0) {
                                    this.loopTime = void 0;
                                    var e, n = this.entity.getComponent(S);
                                    if (null != n && n.unitData) null == (e = this.unitSkillLvShowMgrComponent) || e.removeProgress(n.unitData.teamId, this.entity.pos);
                                    var l = this.skillArray[0];
                                    this.curElapseTime < +l[1] && (this.entity.act = h.Stand)
                                } else {
                                    var o;
                                    null != this.progressType && (null == (o = this.unitSkillLvShowMgrComponent) || o.updateProgress(this.entity, this.loopTime, this.loopAllTime, 2 == this.progressType))
                                }
                    else {
                        var s = this.skillArray[0][1];
                        (null == s || this.curElapseTime >= +s) && this.playOne()
                    } else this.finish()
                }, l.playOne = function() {
                    var t = this.skillArray.shift(),
                        i = t[0],
                        e = i.skill_action,
                        n = t[2];
                    if (this.progressType = i.continued_show, this.applySwitchModel(i), null != n) {
                        var l, o = this.checkEffectLoopTime(i, +n);
                        this.loopAllTime = this.loopTime = o, this.entity.setAct(e, this.attSpeed, !0), null != this.progressType && (null == (l = this.unitSkillLvShowMgrComponent) || l.updateProgress(this.entity, this.loopTime, this.loopAllTime, 2 == this.progressType))
                    } else {
                        this.loopTime = void 0, this.loopAllTime = void 0, this.entity.setAct(e, this.attSpeed);
                        var s, r = this.entity.getComponent(S);
                        if (null != r && r.unitData) null == (s = this.unitSkillLvShowMgrComponent) || s.removeProgress(r.unitData.teamId, this.entity.pos)
                    }
                    if (i.voice) {
                        var a, f = null == (a = this.entity) || null == (a = a.getRoot()) || null == (a = a.getComponent(_)) ? void 0 : a.fight_type;
                        if (!w[f]) this.entity.addOrGetComponent(D).play(i.voice, this.loopAllTime)
                    }
                    this.checkEffectShow(i), p.recycle(t)
                }, l.applySwitchModel = function(t) {
                    var i;
                    if (this.entity.unitType !== o.RedGrid && this.entity.unitType !== o.God) {
                        var e = null == t || null == (i = t.effect_show) ? void 0 : i[y.SwitchModel];
                        if (e && Array.isArray(e)) {
                            var n = e[0],
                                l = e[1];
                            null != n && this.entity.changeModel(n), this.entity.setModelResourceSuffix(l)
                        }
                    }
                }, l.checkEffectShow = function(t) {
                    var i = this,
                        n = t.effect_show;
                    if (n) {
                        var l = this.entity,
                            o = l.getRoot(),
                            r = n[y.SkillEffectProcess],
                            a = l.getComponent(S);
                        if (null != r)
                            for (var f = function() {
                                    var t, n = r[c],
                                        l = n[0],
                                        f = n[1],
                                        h = n[2],
                                        u = n[3],
                                        p = o.getComponent(C).getBulletMap();
                                    if (!(null == a || null == (t = a.unitData) ? void 0 : t.hasSkill(l, f))) return 1;
                                    if (u === m.ReplaceEffect) {
                                        var v = n[4],
                                            g = n[5];
                                        if (h === k.Start)
                                            for (var y, S = e(p); !(y = S()).done;) {
                                                var A, D = y.value,
                                                    M = (D[0], D[1]);
                                                if (null != M.skillEffectCfg && M.skillEffectCfg.id == v) {
                                                    var _ = null == (A = s.cfg.get("SkillEffect.EffectConfig").get(g)) ? void 0 : A.getFirst();
                                                    _ && M.showEft(_.effect_resource)
                                                }
                                            } else if (h === k.End) {
                                                o.getComponent(T).addSkillEnd(l, E.create(i, (function() {
                                                    for (var t, i = e(p); !(t = i()).done;) {
                                                        var n, l = t.value,
                                                            o = (l[0], l[1]);
                                                        if (null != o.skillEffectCfg && o.skillEffectCfg.id == v) {
                                                            var r = null == (n = s.cfg.get("SkillEffect.EffectConfig").get(v)) ? void 0 : n.getFirst();
                                                            r && o.showEft(r.effect_resource)
                                                        }
                                                    }
                                                })))
                                            }
                                    } else if (u === m.ChangeEffectDuration) {
                                        var w = n[4],
                                            U = n[5] * d;
                                        if (h === k.Start)
                                            for (var B, P = e(p); !(B = P()).done;) {
                                                var L = B.value,
                                                    R = (L[0], L[1]);
                                                null != R.skillEffectCfg && (R.skillEffectCfg.id == w && R.addRemoveTime(U))
                                            } else if (h === k.End) {
                                                o.getComponent(T).addSkillEnd(l, E.create(i, (function() {
                                                    for (var t, i = e(p); !(t = i()).done;) {
                                                        var n = t.value,
                                                            l = (n[0], n[1]);
                                                        null != l.skillEffectCfg && (l.skillEffectCfg.id == w && l.addRemoveTime(U))
                                                    }
                                                })))
                                            }
                                    } else if (u === m.ChangeEffectScale) {
                                        var F = n[4],
                                            I = (n[5] || 0) / 1e4,
                                            G = (n[6] || 0) / 1e4;
                                        if (h === k.Start)
                                            for (var H, N = e(p); !(H = N()).done;) {
                                                var O = H.value,
                                                    x = (O[0], O[1]);
                                                if (null != x.skillEffectCfg && x.skillEffectCfg.id == F) {
                                                    var Y = x.getSkillScaleX(),
                                                        X = x.getSkillScaleY(),
                                                        Z = Y > 0 ? Y + I : Y - I,
                                                        b = X > 0 ? X + G : X - G;
                                                    x.setSkillScale(Z, b)
                                                }
                                            } else if (h === k.End) {
                                                o.getComponent(T).addSkillEnd(l, E.create(i, (function() {
                                                    for (var t, i = e(p); !(t = i()).done;) {
                                                        var n = t.value,
                                                            l = (n[0], n[1]);
                                                        if (null != l.skillEffectCfg && l.skillEffectCfg.id == F) {
                                                            var o = l.getSkillScaleX(),
                                                                s = l.getSkillScaleY(),
                                                                r = o > 0 ? o + I : o - I,
                                                                a = s > 0 ? s + G : s - G;
                                                            l.setSkillScale(r, a)
                                                        }
                                                    }
                                                })))
                                            }
                                    }
                                }, c = 0, h = r.length; c < h; c++) f()
                    }
                }, l.checkEffectLoopTime = function(t, i) {
                    var e = i,
                        n = t.effect_show;
                    if (null == n) return e;
                    var l = this.entity,
                        o = n[y.ChangeKeepTime];
                    if (null != o) {
                        var s = l.getComponent(u);
                        if (s)
                            for (var r = 0, a = o.length; r < a; r++) {
                                var f = o[r],
                                    c = f[0],
                                    h = f[1],
                                    p = f[2];
                                s.hasBuff(c) && (0 === h ? e += p * d : 1 === h && (e += i * (p / 1e4)))
                            }
                    }
                    return e
                }, n
            }(c)) || U);
            n._RF.pop()
        }
    }
}));