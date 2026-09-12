/* ===== chunks:///_virtual/FightResultComponent.ts ===== */
System.register("chunks:///_virtual/FightResultComponent.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./MComponent.ts", "./Core.ts", "./FightDataComponent.ts", "./ScenePool.ts"], (function(t) {
    var n, e, i, o, c, s, h;
    return {
        setters: [function(t) {
            n = t.inheritsLoose
        }, function(t) {
            e = t.cclegacy, i = t._decorator
        }, function(t) {
            o = t.MComponent
        }, function(t) {
            c = t.xy
        }, function(t) {
            s = t.FightDataComponent
        }, function(t) {
            h = t.ScenePool
        }],
        execute: function() {
            var l;
            e._RF.push({}, "cc33cDmnftOPZbkfQGAUNIy", "FightResultComponent", void 0);
            t("FightResultComponent", i.ccclass("FightResultComponent")(l = function(t) {
                function e() {
                    for (var n, e = arguments.length, i = new Array(e), o = 0; o < e; o++) i[o] = arguments[o];
                    return (n = t.call.apply(t, [this].concat(i)) || this).clientShow = void 0, n.clientLogicEnd = void 0, n.isSendEnd = void 0, n
                }
                n(e, t);
                var i = e.prototype;
                return i.onRecycle = function() {
                    t.prototype.onRecycle.call(this), this.reset()
                }, i.reset = function() {
                    this.clientShow = void 0, this.clientLogicEnd = void 0, this.isSendEnd = void 0
                }, i.setClientShowEnd = function(t) {
                    this.clientShow = t, this.checkEnd()
                }, i.setClientLogicEnd = function(t) {
                    this.clientLogicEnd = t, this.checkEnd()
                }, i.checkEnd = function() {
                    if ((this.clientShow || null == this.clientShow) && this.clientLogicEnd && !this.isSendEnd) {
                        var t = this.getRoot().getComponent(s);
                        if (t.param.isQuick && c.fight.debugPveFightAngle()) return this.isSendEnd = !0, void h.recycle(this.getRoot());
                        c.battle.onBattleEnd(t.param, t.battleResult), this.isSendEnd = !0
                    }
                }, i.setSendEnd = function(t) {
                    this.isSendEnd = t
                }, e
            }(o)) || l);
            e._RF.pop()
        }
    }
}));