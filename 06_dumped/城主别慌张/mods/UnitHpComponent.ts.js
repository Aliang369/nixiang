/* ===== chunks:///_virtual/UnitHpComponent.ts ===== */
System.register("chunks:///_virtual/UnitHpComponent.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./BattleConst.ts", "./AttrEnum.ts", "./MComponent.ts", "./AiComponent.ts", "./FightDataComponent.ts", "./HpItemMgrComponent.ts", "./SceneDef.ts", "./BaseVec3.ts", "./ScenePool.ts", "./Core.ts", "./FightDef.ts", "./UnitDataComponent.ts"], (function(t) {
    var i, e, n, o, s, r, h, p, a, u, l, d, f, c, m, g, v, y;
    return {
        setters: [function(t) {
            i = t.inheritsLoose, e = t.createClass
        }, function(t) {
            n = t.cclegacy, o = t._decorator
        }, function(t) {
            s = t.UnityType
        }, function(t) {
            r = t.AttrEnum
        }, function(t) {
            h = t.MComponent
        }, function(t) {
            p = t.AiComponent
        }, function(t) {
            a = t.FightDataComponent
        }, function(t) {
            u = t.HpItemMgrComponent
        }, function(t) {
            l = t.PartTargetType, d = t.PartType
        }, function(t) {
            f = t.BaseVec3
        }, function(t) {
            c = t.ScenePool
        }, function(t) {
            m = t.xy
        }, function(t) {
            g = t.Pv, v = t.FightType
        }, function(t) {
            y = t.UnitDataComponent
        }],
        execute: function() {
            var I;
            n._RF.push({}, "62d28BWuldMbLFiLwVK9gxo", "UnitHpComponent", void 0);
            t("UnitHpComponent", o.ccclass("UnitHpComponent")(I = function(t) {
                function n() {
                    for (var i, e = arguments.length, n = new Array(e), o = 0; o < e; o++) n[o] = arguments[o];
                    return (i = t.call.apply(t, [this].concat(n)) || this).hpItemMgr = void 0, i.unitEntityId = void 0, i._curHp = void 0, i._maxHp = void 0, i.shield = void 0, i.isMonster = void 0, i._visible = !0, i.alwaysShow = !1, i.offX = void 0, i.offY = void 0, i
                }
                i(n, t);
                var o = n.prototype;
                return o.onInit = function() {
                    t.prototype.onInit.call(this)
                }, o.updateXY = function(t, i) {
                    if (this.unitEntityId) {
                        var e = this.getParent();
                        if (null != t) this.hpItemMgr.updateXY(this.unitEntityId, t, i, e.scaleY);
                        else if (null == this.offX) {
                            var n = l[e.unitType] || d.Monster,
                                o = e.partMgr.getPart(n);
                            if (o) {
                                var r = c.fetch(f);
                                if (e.unitType == s.CallUnit ? o.getFootPos(r, !1) : o.getHeadPos(r, !1)) {
                                    r.z = 0;
                                    var h = this.getHpOffset(e),
                                        p = h[0],
                                        a = h[1];
                                    this.offX = p, this.offY = r.y + a, this.visible = !0, this.hpItemMgr.updateXY(this.unitEntityId, e.x + this.offX, e.y + this.offY, e.scaleY)
                                } else this.visible = !1;
                                c.recycle(r)
                            } else this.visible = !1
                        } else this.hpItemMgr.updateXY(this.unitEntityId, e.x + this.offX, e.y + this.offY, e.scaleY)
                    }
                }, o.getHpOffset = function(t) {
                    var i, e, n = t.getComponent(y),
                        o = null == n || null == (i = n.unitCreate) ? void 0 : i.code;
                    if (null == o) return [0, 0];
                    var s = m.cfg.get("Monster.Monster").get(o),
                        r = null == (e = m.cfg.get("Monster.Monster_image").get(null == s ? void 0 : s.resource)) ? void 0 : e.hp_offset;
                    return [Number(null == r ? void 0 : r[0]) || 0, Number(null == r ? void 0 : r[1]) || 0]
                }, o.getHpItem = function() {
                    return this.hpItemMgr.getHpItem(this.unitEntityId)
                }, o.updateHp = function(t, i) {
                    this._curHp = t, this._maxHp = i, this.hpItemMgr.updateHp(this.unitEntityId, t, i), this.hpItemMgr.removeRebornTime(this.unitEntityId)
                }, o.updateShield = function(t) {
                    this.shield = t, this.hpItemMgr.updateShield(this.unitEntityId, t)
                }, o.setRebornTime = function(t) {
                    this.hpItemMgr.setRebornTime(this.unitEntityId, t)
                }, o.removeRebornTime = function() {
                    this.hpItemMgr.removeRebornTime(this.unitEntityId)
                }, o.onRecycle = function() {
                    this.unitEntityId && (this.hpItemMgr.removeHp(this.unitEntityId), this.hpItemMgr = void 0, this.unitEntityId = void 0), this._curHp = void 0, this._maxHp = void 0, this.shield = void 0, this._visible = !0, this.isMonster = void 0, this.alwaysShow = !1, this.offX = this.offY = void 0, t.prototype.onRecycle.call(this)
                }, o.onStart = function() {
                    t.prototype.onStart.call(this);
                    var i = this,
                        e = i.getRoot(),
                        n = e.getComponent(a),
                        o = i.hpItemMgr = e.getComponent(u),
                        h = i.getParent(),
                        l = this.unitEntityId = h.entityId,
                        d = h.getComponent(y),
                        f = d.unitCreate,
                        c = d.battleUnit,
                        I = n.getUnitHp(l),
                        H = f.type == s.Partner;
                    if (H && !(m.battle.getPvType(n.param.fight_id) == g.P)) return void(this.unitEntityId = void 0);
                    if (I) this._curHp = I[0], this._maxHp = I[1];
                    else {
                        var M;
                        if (null == c) return;
                        var _ = null != (M = c.attr[r.Attr.cur_hp]) ? M : c.attr[r.Attr.hp],
                            C = c.attr[r.Attr.hp];
                        this._curHp = +_, this._maxHp = +C
                    }
                    var b = c && c.type == s.Boss,
                        E = f.type == s.CallUnit;
                    this.isMonster = c && c.type == s.Monster;
                    var P = n.getTeamType(f.team_id);
                    if (b) {
                        if (n.param.battle_type == v.KingdomBoss) return;
                        o.addBossHp(l, P, void 0, this.alwaysShow)
                    } else H ? o.addPartnerHp(l, P, void 0, this.alwaysShow) : E ? o.addCallUnitHp(l, P, void 0, this.alwaysShow) : o.addHp(l, P, void 0, this.alwaysShow);
                    var S = n.getShieldUpdateInfo(l);
                    if (null != S && this.updateShield(S.shield_value), this.updateHp(this._curHp, this._maxHp), H) {
                        var T = this.getParent();
                        this.updateXY(T.x, T.y + 130)
                    } else E ? this.updateXY() : this.updateXY(0, -1e3);
                    e.getComponent(p).getFightAi().onUpdateUnitHp(l)
                }, e(n, [{
                    key: "visible",
                    get: function() {
                        return this._visible
                    },
                    set: function(t) {
                        this._visible = t, this.hpItemMgr.setVisible(this.unitEntityId, t)
                    }
                }, {
                    key: "maxHp",
                    get: function() {
                        return this._maxHp
                    }
                }, {
                    key: "curHp",
                    get: function() {
                        return this._curHp
                    }
                }]), n
            }(h)) || I);
            n._RF.pop()
        }
    }
}));