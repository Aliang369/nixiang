/* ===== chunks:///_virtual/AdModel.ts ===== */
System.register("chunks:///_virtual/AdModel.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./BaseModel.ts", "./Core.ts", "./Handler.ts", "./module_key.ts", "./system_define.ts", "./Functions.ts", "./GlobalVar.ts"], (function(t) {
    var e, n, o, i, a, c, r, s, l, u;
    return {
        setters: [function(t) {
            e = t.inheritsLoose
        }, function(t) {
            n = t.cclegacy
        }, function(t) {
            o = t.BaseModel
        }, function(t) {
            i = t.xy
        }, function(t) {
            a = t.Handler
        }, function(t) {
            c = t.ModuleKey
        }, function(t) {
            r = t.PrivilegeType, s = t.AdType
        }, function(t) {
            l = t.assign
        }, function(t) {
            u = t.GlobalVar
        }],
        execute: function() {
            var d;
            t("AdVal", void 0), n._RF.push({}, "0619f2jfXFKvqkl2J/2oa1a", "AdModel", void 0);
            var h, f, A, m = ((d = {})[c.Ad.LastResetDate] = 0, d);
            t("AdModel", function(t) {
                function n() {
                    for (var e, n = arguments.length, o = new Array(n), i = 0; i < n; i++) o[i] = arguments[i];
                    return (e = t.call.apply(t, [this].concat(o)) || this)._coolTimer = null, e._resetKey = 1111, e
                }
                e(n, t);
                var o = n.prototype;
                return o.reset = function() {
                    t.prototype.reset.call(this), this._coolTimer && (i.timer.unschedule(this._coolTimer), this._coolTimer = null)
                }, o.initModuleData = function(e) {
                    t.prototype.initModuleData.call(this, e), this.setModuleDefaultDatas(m)
                }, o.setData = function(e, n, o) {
                    this.get(e) === n || (t.prototype.setData.call(this, e, n, o), u.isEnterGame)
                }, o.onSetMetaData = function(t, e) {
                    t === this._resetKey && e && this.get(this._resetKey) != e && this.resetData()
                }, o.onSetMetaDatas = function(t, e) {
                    this.checkRed()
                }, o.resetData = function() {
                    var t = this;
                    t.forEachData((function(e, n) {
                        e != t._resetKey && t.setData(e, 0)
                    }), t)
                }, o.getWatchTimes = function(t) {
                    var e;
                    switch (t) {
                        case s.ZHANDOU:
                            e = 0;
                            break;
                        default:
                            e = 0
                    }
                    return e
                }, o.getLastWatchTime = function(t) {
                    var e;
                    return +(null != (e = this.get(100 * t)) ? e : 0)
                }, o.canSkipAdWatch = function() {
                    return i.D.privilege.getPrivilegeValue(r.SkipAd) > 0
                }, o.getAdInfo = function(t) {
                    var e, n = i.cfg.get("Video.Advertisement").get(t),
                        o = this.getWatchTimes(t),
                        a = Math.max((null != (e = null == n ? void 0 : n.times) ? e : 0) + 0 - o, 0);
                    return l({
                        type: t,
                        watchedTimes: o,
                        leftTimes: a,
                        lastWatchedTime: undefined,
                        countDownTime: 0,
                        canWatch: a > 0 && !0
                    }, null != n ? n : {}, {
                        times: n.times + 0
                    })
                }, o.getAdRed = function(t) {
                    var e, n;
                    return null != (e = null == (n = this.getAdInfo(t)) ? void 0 : n.red_id) ? e : 0
                }, o.checkRed = function(t) {
                    return !1
                }, o.uploadAdAction = function(t, e) {
                    var n = this.getAdInfo(t);
                    return n.canWatch && i.U.sdk.uploadAdAction(t, e), n
                }, o.checkWatchAd = function(t, e) {
                    var n = this.getAdInfo(t);
                    return n.canWatch ? null == e || e.runWith([n]) : n.leftTimes <= 0 ? i.module.toast(900404, !0) : n.countDownTime > 0 && i.module.toast(i.U.game.localized("common.ad_cool_time", i.U.date.getFormatBySecond11(n.countDownTime))), n
                }, o.tryWatchAd = function(t, e, n) {
                    return this.checkWatchAd(t, a.create(this, this.doWatchAd, [t, e, n]))
                }, o.doWatchAd = function(t, e, n) {
                    i.U.sdk.uploadAdAction(t, h.UploadSrc.Click), i.U.sdk.watchAd(a.create(this, this.onWatchAdDone, [t, e, n]))
                }, o.onWatchAdDone = function(t, e, n) {
                    i.U.sdk.uploadAdAction(t, i.V.Ad.UploadSrc.Over), i.proxy.module.sendMsg(11801, {
                        type: t,
                        params: e
                    }), null == n || n.runWith([t, e])
                }, n
            }(o));
            f = h || (h = t("AdVal", {})), A = function(t) {
                return t[t.Show = 1] = "Show", t[t.Click = 2] = "Click", t[t.Over = 3] = "Over", t
            }({}), f.UploadSrc = A, n._RF.pop()
        }
    }
}));