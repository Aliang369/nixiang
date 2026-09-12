/* ===== chunks:///_virtual/FightPlayComponent.ts ===== */
System.register("chunks:///_virtual/FightPlayComponent.ts", ["./rollupPluginModLoBabelHelpers.js", "cc", "./EventType.ts", "./Core.ts", "./MComponent.ts", "./FightUtil.ts", "./OnFrameEndEvent.ts", "./OnHurtEvent.ts", "./SceneRegDef.ts", "./AiComponent.ts", "./SceneDef.ts", "./Oops.ts", "./FightDataComponent.ts"], (function(t) {
    var i, e, a, s, n, o, l, r, h, m, u, c, g, p, d, f, v, T, F, _;
    return {
        setters: [function(t) {
            i = t.inheritsLoose, e = t.createClass
        }, function(t) {
            a = t.cclegacy, s = t._decorator
        }, function(t) {
            n = t.BattleEvent, o = t.RegEventType
        }, function(t) {
            l = t.xy
        }, function(t) {
            r = t.MComponent
        }, function(t) {
            h = t.IntervalTime30, m = t.FightUtil
        }, function(t) {
            u = t.OnFrameEndEvent
        }, function(t) {
            c = t.OnHurtEvent
        }, function(t) {
            g = t.SkipBattleEventMap, p = t.BattleEventDebugLogTextMap, d = t.DebugBattleEventMap, f = t.BattleEventMap
        }, function(t) {
            v = t.AiComponent
        }, function(t) {
            T = t.IntervalTime2
        }, function(t) {
            F = t.oops
        }, function(t) {
            _ = t.FightDataComponent
        }],
        execute: function() {
            var S;
            a._RF.push({}, "ea7b2XJFa9LtoDHrXdp1C2k", "FightPlayComponent", void 0);
            t("FightPlayComponent", s.ccclass("FightPlayComponent")(S = function(t) {
                function a() {
                    for (var i, e = arguments.length, a = new Array(e), s = 0; s < e; s++) a[s] = arguments[s];
                    return (i = t.call.apply(t, [this].concat(a)) || this).catchQueue = [], i.playQueue = [], i.lastTime = void 0, i._isOpen = void 0, i._timeScale = 1, i.tmpTimeScale = void 0, i.roomPresentationScale = void 0, i.uuid = void 0, i._curFrame = 1, i._curTime = 0, i.samllFrame = void 0, i.smallFrameTotal = void 0, i.ai = void 0, i.fightDataComp = void 0, i.tickStart = void 0, i.tickStartTime = void 0, i.frameAdd = void 0, i._isTimeWaiting = !1, i.timeWaitTime = void 0, i._timeWaitCasterId = void 0, i.lastMoveEndId = void 0, i.lastMoveEndFrame = void 0, i.lastMoveEndX = void 0, i.lastMoveEndY = void 0, i.tmpTime = void 0, i.tmpCount = void 0, i
                }
                i(a, t);
                var s = a.prototype;
                return s.onRecycle = function() {
                    this.catchQueue.length = 0, this.playQueue.length = 0, this.lastTime = 0, this._isOpen = !1, this._timeScale = 1, this.tmpTimeScale = void 0, this.roomPresentationScale = void 0, this.samllFrame = void 0, this.smallFrameTotal = void 0, this._curTime = 0, this.ai = void 0, this.fightDataComp = void 0, this.tickStart = void 0, this.tickStartTime = void 0, this.frameAdd = void 0, this._isTimeWaiting = !1, this.timeWaitTime = void 0, this._timeWaitCasterId = void 0, t.prototype.onRecycle.call(this)
                }, s.onFetch = function() {
                    t.prototype.onFetch.call(this), this._curFrame = 1, this.roomPresentationScale = void 0
                }, s.onStart = function() {
                    t.prototype.onStart.call(this), this.ai = this.getRoot().getComponent(v).getFightAi(), this.fightDataComp = this.getRoot().getComponent(_), this.lastTime = this._curFrame
                }, s.updateUuid = function(t) {
                    this.uuid = t
                }, s.cleanFrame = function() {
                    this._curFrame = 1
                }, s.onFightUpdate = function(t) {
                    if (this._isTimeWaiting) {
                        if (this.timeWaitTime -= t, this.timeWaitTime > 0) return;
                        this.stopTimeWait()
                    }
                    if (this._isOpen && !this.fightDataComp.isEnd) {
                        if (this._curTime += t, this._curTime >= this.lastTime) {
                            this.tickStart, this.tickStart = !0, l.fight.debugLog && (this.tickStartTime = Date.now()), null != this.tmpTimeScale && (this._timeScale = this.tmpTimeScale, this.tmpTimeScale = void 0, this.getRoot().onTimeScale(this.timeTrueScale));
                            var i = l.fight.intervalTime * this.timeTrueScale;
                            if (null == this.smallFrameTotal) {
                                var e = l.battleClient.DoBattleTickSmallMsg.alloc(this.uuid, this.curFrame, 1, 1);
                                l.fight.newTickLog && Logger.logCustom("tick curFrame:1 samllFrame:1 smallFrameTotal:1"), this.dispatchBattleTickSmall(e)
                            } else {
                                var a = l.battleClient.DoBattleTickSmallMsg.alloc(this.uuid, this.curFrame, this.samllFrame, this.smallFrameTotal);
                                l.fight.newTickLog && Logger.logCustom("tick curFrame:" + this.curFrame + " samllFrame:" + this.samllFrame + " smallFrameTotal:" + this.smallFrameTotal), this.dispatchBattleTickSmall(a)
                            }
                            if (l.fight.intervalTime == h) this.samllFrame = 1, this.smallFrameTotal = 1;
                            else {
                                var s = Math.max(Math.floor((T + (this.lastTime - this._curTime)) / i), 0);
                                this.samllFrame = 1, this.smallFrameTotal = s + 1
                            }
                            return this.lastTime += T, this._curFrame++, this.ai.ticker(this._curFrame), void this.ticker()
                        }
                        if (1 != this.smallFrameTotal) {
                            var n = l.battleClient.DoBattleTickSmallMsg.alloc(this.uuid, this.curFrame, this.samllFrame, this.smallFrameTotal);
                            l.fight.newTickLog && Logger.logCustom("update curFrame:" + this.curFrame + " samllFrame:" + this.samllFrame + " smallFrameTotal:" + this.smallFrameTotal), this.dispatchBattleTickSmall(n), this.samllFrame++
                        }
                    }
                    this.ticker()
                }, s.onRoomTick = function(t) {
                    var i = l.battleClient.DoBattleTickSmallMsg.alloc(this.uuid, t.frameId, 1, 1);
                    this.dispatchBattleTickSmall(i), this._curFrame = t.frameId, this.ai.ticker(this._curFrame)
                }, s.skipTicker = function() {
                    if (this.tickStart) return !1;
                    var t = l.battleClient.DoBattleTickSmallMsg.alloc(this.uuid, this.curFrame, 1, 1);
                    return this.dispatchBattleTickSmall(t), this._curFrame++, !0
                }, s.setRoomPresentationScale = function(t) {
                    var i;
                    this.roomPresentationScale = t, null == (i = this.getRoot()) || i.onTimeScale(this.timeTrueScale)
                }, s.onEvent = function(t) {
                    if (this.ai.curFightUuid) {
                        if (this.ai.isSkip) {
                            if (t.event_type == n.FrameEnd && u.run(this.getRoot(), t.data), null == g[t.event_type]) return void(null == t.free || t.free());
                            t.event_type == n.BattleResult && (Logger.warnCustom("跳过总帧速:" + t.frame), this.ai.onSkipEnd())
                        } else if (t.event_type == n.FrameEnd) return u.run(this.getRoot(), t.data), void(null == t.free || t.free())
                    } else if (t.event_type == n.FrameEnd && u.run(this.getRoot(), t.data), t.event_type != n.BattleResult) return void(null == t.free || t.free());
                    this.catchQueue.push(t)
                }, s.dispatchBattleTickSmall = function(t) {
                    m.eventDispatcher.dispatchEvent(o.Do_Battle_Tick_Small, t)
                }, s.ticker = function() {
                    if (l.fight.debugLog) {
                        var t = Math.floor(Date.now() / 1e3);
                        null == this.tmpTime && (this.tmpTime = t, this.tmpCount = 0), this.tmpCount++, t != this.tmpTime && (this.tmpTime = t, this.tmpCount = 0)
                    }
                    var i = this.catchQueue;
                    if (0 != i.length) {
                        for (var e = 0; e < i.length; e++) this.playQueue.push(i[e]);
                        i.length = 0
                    }
                    if (0 != this.playQueue.length) {
                        if (l.fight.debugFightMsg) {
                            for (var a = "消息队列长度：" + this.playQueue.length + "\n", s = {}, n = 0; n < this.playQueue.length; n++) {
                                var o = this.playQueue[n];
                                null == s[o.event_type] && (s[o.event_type] = 0), s[o.event_type] += 1
                            }
                            for (var r in s) {
                                var h;
                                a += (null != (h = p[r]) ? h : "未定义") + " 数量：" + s[r] + "\n"
                            }
                            Logger.warnCustom(a)
                        }
                        for (var m = 0, u = this.playQueue.length; m < u; m++)
                            if (this._isTimeWaiting) this.catchQueue.push(this.playQueue[m]);
                            else {
                                var g = this.playQueue[m];
                                if (F.config.query.data.debugFight) {
                                    var v = d[g.event_type];
                                    v && v.run(this.parent, g.data, g.frame)
                                }
                                var T = f[g.event_type];
                                try {
                                    T && T.run(this.parent, g.data, g.frame)
                                } catch (t) {
                                    Logger.errorCustom(t)
                                } finally {
                                    null == g.free || g.free()
                                }
                            } this.playQueue.length = 0, c.flushPending(this.parent)
                    }
                }, s.clean = function() {
                    this.catchQueue.length = 0, this.playQueue.length = 0, this.frameAdd = void 0, this.tickStart = !1, this.samllFrame = void 0, this.smallFrameTotal = void 0
                }, s.frameEnd = function() {
                    this.tickStart = !1
                }, s.setTimeWait = function(t, i) {
                    l.battle.isGveRoomMode(l.battle.curFightInput) || this._isTimeWaiting || (this.timeWaitTime = t * T, this._isTimeWaiting = !0, this._timeWaitCasterId = i, this.getRoot().onTimeWait())
                }, s.stopTimeWait = function() {
                    this._isTimeWaiting = !1, this._timeWaitCasterId = void 0, this.getRoot().onTimeWait()
                }, e(a, [{
                    key: "isOpen",
                    get: function() {
                        return this._isOpen || l.battle.isGveRoomMode(l.battle.curFightInput)
                    },
                    set: function(t) {
                        this.fightDataComp && !l.battle.isGveRoomMode(l.battle.curFightInput) && this._isOpen != t && (t ? this.getRoot().onResume() : this.getRoot().onPause()), this._isOpen = t, this.lastTime = this._curTime, F.config.query.data.debugFight && Logger.logCustom("isOpen:" + t)
                    }
                }, {
                    key: "timeScale",
                    get: function() {
                        return this._timeScale
                    },
                    set: function(t) {
                        this.tmpTimeScale = t
                    }
                }, {
                    key: "timeTrueScale",
                    get: function() {
                        if (null != this.roomPresentationScale) return this.roomPresentationScale;
                        var t = l.cfg.get("Fight.FightPosConfig").combat_speed[this.timeScale];
                        return t || 1
                    }
                }, {
                    key: "curFrame",
                    get: function() {
                        return this._curFrame
                    },
                    set: function(t) {
                        this._curFrame = t
                    }
                }, {
                    key: "isTimeWaiting",
                    get: function() {
                        return this._isTimeWaiting
                    }
                }, {
                    key: "timeWaitCasterId",
                    get: function() {
                        return this._timeWaitCasterId
                    }
                }]), a
            }(r)) || S);
            a._RF.pop()
        }
    }
}));