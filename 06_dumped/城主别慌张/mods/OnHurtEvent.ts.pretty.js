/* ===== chunks:///_virtual/OnHurtEvent.ts ===== */
System.register("chunks:///_virtual/OnHurtEvent.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./BattleConst.ts", "./Core.ts", "./RandomUtils.ts", "./BehaviorMgrComponent.ts", "./IBattleEvent.ts", "./index5.ts", "./index8.ts", "./SkillRecordComponent.ts", "./HurtFloatingText.ts", "./SkillDef.ts", "./MoveBehavior.ts", "./UnitDataComponent.ts", "./UnitHpComponent.ts", "./AiComponent.ts", "./DebugEntityMgrComponent.ts", "./FightDamageStatsComponent.ts", "./FightDataComponent.ts", "./FloatingTextMgrComponent.ts", "./RelicHurtMergeComponent.ts", "./UnitMgrComponent.ts", "./BattleParam.ts", "./SceneDef.ts", "./RoomComponent.ts", "./GPUFightTextTypes.ts", "./GPUFightTextManager.ts"], (function(t) {
    var e, n, i, o, r, a, f, u, l, s, c, g, p, m, h, v, d, C, y, T, _, x, F, k, M, U, H, S, D, b;
    return {
        setters: [function(t) {
            e = t.inheritsLoose
        }, function(t) {
            n = t.cclegacy, i = t.Vec3
        }, function(t) {
            o = t.DamageType, r = t.UnityType
        }, function(t) {
            a = t.xy
        }, function(t) {
            f = t.RandomUtils
        }, function(t) {
            u = t.BehaviorMgrComponent
        }, function(t) {
            l = t.SceneEvent
        }, null, null, function(t) {
            s = t.SkillRecordComponent
        }, function(t) {
            c = t.FloatingValueType
        }, function(t) {
            g = t.SkillEffectShowType
        }, function(t) {
            p = t.MoveBehavior
        }, function(t) {
            m = t.UnitDataComponent
        }, function(t) {
            h = t.UnitHpComponent
        }, function(t) {
            v = t.AiComponent
        }, function(t) {
            d = t.DebugEntityMgrComponent
        }, function(t) {
            C = t.FightDamageStatsComponent
        }, function(t) {
            y = t.FightDataComponent
        }, function(t) {
            T = t.FloatingTextMgrComponent
        }, function(t) {
            _ = t.RelicHurtMergeComponent
        }, function(t) {
            x = t.UnitMgrComponent
        }, function(t) {
            F = t.TrajectoryType
        }, function(t) {
            k = t.MonsterFlag, M = t.FightDesignHurtHeight, U = t.PartTargetType, H = t.PartType
        }, function(t) {
            S = t.RoomComponent
        }, function(t) {
            D = t.GPUFightTextMotionType
        }, function(t) {
            b = t.GPUFightTextManager
        }],
        execute: function() {
            var E;
            n._RF.push({}, "53228fLSoNPlYIEiFI9o2eF", "OnHurtEvent", void 0);
            var B = ((E = {})[o.Crit] = !0, E[o.Skill] = !0, E[o.Attack] = !0, E),
                P = t("OnHurtEvent", function(t) {
                    function n() {
                        return t.apply(this, arguments) || this
                    }
                    return e(n, t), n.getGpuFightTextBornOffset = function(t, e) {
                        if (e.z = 0, 10001 == t) return e.x = 0, void(e.y = 0);
                        var n = t == o.Crit,
                            i = 0 == f.randint(0, 1) ? -1 : 1,
                            r = n ? 56 : 40,
                            u = n ? 96 : 82,
                            l = n ? 36 : 28,
                            s = n ? 70 : 60;
                        e.x = i * a.U.math.random(r, u), e.y = a.U.math.random(l, s)
                    }, n.flushPending = function(t) {
                        var e = t.getComponent(_);
                        if (e) {
                            var n = e.flush();
                            if (0 != n.length) {
                                e.enterFlush();
                                try {
                                    for (var i = 0; i < n.length; i++) this.run(t, n[i])
                                } finally {
                                    e.leaveFlush()
                                }
                            }
                        }
                    }, n.run = function(t, e) {
                        var n = t.getComponent(_);
                        if (null == n || !n.tryPush(e)) {
                            var i = t.getComponent(y),
                                l = t.getComponent(C),
                                F = t.getComponent(x),
                                E = t.getComponent(v).getFightAi(),
                                P = (E.battleType, e.target_id + ""),
                                w = e.caster_id + "",
                                j = F.getUnit(P),
                                O = F.getUnit(w);
                            if (l.updateDamage(w, e.value), a.fight.isTest() && (e.effect_id && l.updateEffectDamage(w, e.effect_id, e.value), e.buff_id && l.updateBuffDamage(w, e.buff_id, e.value)), null != e.after_hp && i.updateUnitHp(P, e.after_hp), j) {
                                var V, A = !!O && O.unitType == r.Monster;
                                B[e.value_type] && j.hurt(e);
                                var G = j.getComponent(h);
                                if (G) {
                                    var I = i.getUnitHp(P);
                                    I && G.updateHp(I[0], I[1])
                                }
                                E.onUpdateUnitHp(P, e);
                                var N = null == (V = t.getComponent(S)) ? void 0 : V.isCatchUp();
                                if (j.addHurtTxt() && !N && a.fight.tmpHurtShow && e.value_type != o.Dead_Auto) {
                                    t.getComponent(T);
                                    if ((!A || A) && !E.isSkip) {
                                        var L = e.value_type || (0 == e.hp_type ? o.Skill : o.Skill_Treat),
                                            W = e.bockHurt,
                                            K = null != W && 0 !== W,
                                            z = K ? Math.abs(W) : 0,
                                            Y = L == o.Attack_Treat || L == o.Skill_Treat || L == o.Real_Treat || L == o.Vampire;
                                        j.unitType != r.Wall || Y || (L = 10001);
                                        var q = j.getComponent(m).unitCreate.code,
                                            J = a.cfg.get("Monster.Monster").get(q);
                                        J && 0 != (J.flag & k.HitFloatTextRed) && (L = 10001);
                                        var Q = e.value ? e.value : 0,
                                            X = K && this.hasBlockHurtTrajectory(e);
                                        X && (Q += z), L == o.Immune && (Q = e.after_hp), 0 != Q || K || (L = o.Miss);
                                        var Z, $, tt = a.cfg.get("Fight.FightPosConfig"),
                                            et = tt.sct_setting_offset,
                                            nt = et[L] || et[R],
                                            it = null != e.x ? e.x : j.x,
                                            ot = null != e.y ? e.y : j.y;
                                        if (e.x = void 0) {
                                            var rt = j.partMgr;
                                            rt && (it = rt.x + it, ot = rt.y + ot)
                                        }
                                        if (ot > M && (ot = M), j && j.unitType != r.Wall) {
                                            var at, ft = U[j.unitType] || H.Monster,
                                                ut = null == (at = j.partMgr) ? void 0 : at.getPart(ft);
                                            if (ut) ut.getHitPos(this.tmpVec3, !1) && (Z = this.tmpVec3.x + it, $ = this.tmpVec3.y + ot)
                                        }
                                        null == Z && (Z = it, $ = ot), j.unitType == r.Wall && Y && (Z = 0);
                                        for (var lt = j.addOrGetComponent(s), st = 0; st < 5; st++) {
                                            var ct = f.randint(0, nt.length - 1);
                                            if (ct != lt.recordCount) {
                                                lt.recordCount = ct;
                                                var gt = nt[ct];
                                                Z += gt[0] || 0, $ += gt[1] || 0;
                                                break
                                            }
                                        }
                                        var pt, mt = tt.sct_setting_new,
                                            ht = this.tmpFightTextOffset;
                                        this.getGpuFightTextBornOffset(L, ht);
                                        var vt, dt = D.Default;
                                        10001 == L ? (pt = c[L], dt = D.Upward) : pt = mt[L] || c[L] || mt.default;
                                        var Ct = this.partNumber(Q),
                                            yt = X ? "(-" + this.partNumber(z) + ")" : "";
                                        switch (L) {
                                            case o.Crit:
                                                vt = "b" + Ct + yt;
                                                break;
                                            case o.Miss:
                                                vt = "m";
                                                break;
                                            case o.Kill:
                                                vt = "K";
                                                break;
                                            case o.Immune:
                                                vt = "M";
                                                break;
                                            case o.Vampire:
                                            case o.Attack_Treat:
                                            case o.Skill_Treat:
                                                vt = "+" + Ct;
                                                break;
                                            default:
                                                vt = "" + Ct + yt
                                        }
                                        b.instance.show(pt, vt, Z + ht.x, $ + ht.y, e.target_id, dt)
                                    }
                                }
                                if (e.after_hp <= 0 && !j.isDie) {
                                    var Tt, _t = null == (Tt = a.cfg.get("SkillEffect.EffectConfig").get(e.effect_id)) ? void 0 : Tt.getFirst();
                                    if (_t && _t.effect_show && _t.effect_show[g.DelayShow]) {
                                        var xt = j.getComponent(u).get(p);
                                        null == xt || xt.finish()
                                    }
                                    j.die(), E.onDieBefore(j);
                                    var Ft = t.getComponent(d);
                                    if (Ft) {
                                        var kt, Mt = null == (kt = j.getComponent(m)) || null == (kt = kt.unitCreate) ? void 0 : kt.id;
                                        Ft.destroyUnit(Mt)
                                    }
                                }
                            } else;
                        }
                    }, n.hasBlockHurtTrajectory = function(t) {
                        return this.effectHasTrajectory29(t.bockEffectId)
                    }, n.effectHasTrajectory29 = function(t) {
                        if (!t) return !1;
                        var e = a.cfg.get("SkillEffect.EffectConfig").get(t);
                        if (!e) {
                            var n = a.cfg.get("Skill.SkillEffect").get(t);
                            if (n && n.expression && (e = a.cfg.get("SkillEffect.EffectConfig").get(n.expression)), !e) return !1
                        }
                        for (var i = e.values(), o = 0; o < i.length; o++) {
                            var r;
                            if ((null == (r = i[o]) ? void 0 : r.trajectory) === F.Trajectory29) return !0
                        }
                        return !1
                    }, n.partNumber = function(t) {
                        if (t < w) return t.toString();
                        var e = a.U.math.getNumExpression(!0).units;
                        return t < O ? Math.floor(t / j) + e[0] : t < V ? this.formatHurtUnit(t, O, e[1]) : t < G ? this.formatHurtUnit(t, A, e[2]) : this.formatHurtUnit(t, I, e[3])
                    }, n.formatHurtUnit = function(t, e, n) {
                        var i = Math.floor(t / e * 10);
                        return i % 10 == 0 ? i / 10 + n : (i / 10).toFixed(1) + n
                    }, n
                }(l));
            P.tmpVec3 = new i, P.tmpFightTextOffset = new i;
            var R = "default",
                w = 1e6,
                j = 1e4,
                O = 1e8,
                V = 1e14,
                A = 1e12,
                G = 1e18,
                I = 1e16;
            n._RF.pop()
        }
    }
}));