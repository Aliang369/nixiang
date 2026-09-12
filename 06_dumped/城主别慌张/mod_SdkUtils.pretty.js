/* ===== chunks:///_virtual/SdkUtils.ts ===== */
System.register("chunks:///_virtual/SdkUtils.ts", ["cc", "./Core.ts", "./Oops.ts", "./Base64Utils.ts", "./PayModel.ts", "./GameEvent.ts", "./GameUIID.ts", "./StorageKey.ts", "./Functions.ts", "./GlobalConst.ts", "./GlobalVar.ts", "./ActPanels.ts", "./BsTypes.ts", "./PlatformJudge.ts"], (function(e) {
    var i, n, t, o, a, r, c, l, u, s, d, y, f, h, m, g, p, M;
    return {
        setters: [function(e) {
            i = e.cclegacy, n = e.sys, t = e.game
        }, function(e) {
            o = e.xy
        }, function(e) {
            a = e.oops
        }, function(e) {
            r = e.Base64Utils
        }, function(e) {
            c = e.INTERNAL_VOUCHER_ID, l = e.EXTERNAL_VOUCHER_ID
        }, function(e) {
            u = e.GameEvent
        }, function(e) {
            s = e.UIID
        }, function(e) {
            d = e.StorageKey
        }, function(e) {
            y = e.isValid
        }, function(e) {
            f = e.Currency
        }, function(e) {
            h = e.GlobalVar, m = e.GamePkgType
        }, function(e) {
            g = e.ActPanelName
        }, function(e) {
            p = e.BsPayLimitType
        }, function(e) {
            M = e.PlatformJudge
        }],
        execute: function() {
            i._RF.push({}, "7f58aorHqNLdZqLQtQIBRPB", "SdkUtils", void 0);
            var x = e("SdkUtils", function() {
                function e() {}
                return e.initClientInfo = function() {
                    xyMini && h.pkgType != m.DevLocal ? o.D.server.setPathInfo(xyMini) : o.D.server.setPathInfo({
                        cdnRoot: "192.168.0.119:8090/client/xianyu_httpres/",
                        serviceUrl: "http://192.168.53.50:9667/",
                        serviceUrl2: "http://192.168.53.50:9667/",
                        payServiceUrl: "http://192.168.53.50:9667/",
                        dotServiceUrl: "http://192.168.10.34:9668/",
                        battleServiceUrl: "http://192.168.0.132/"
                    });
                    var e = a.config.query.data;
                    1 == e.isReview && (xyMini.pkgInfo || (xyMini.pkgInfo = {}), xyMini.pkgInfo.isReview = !0), e.testTag && o.D.server.setTestTag(e.testTag), xyMini.dispatchGameEvent = o.U.sdk.onResSdkEvent, xyMini.dispatchXySdkMsg = o.U.sdk.onResXySdkMsg
                }, e.onPauseGame = function() {}, e.onResumeGame = function() {
                    o.D.gameClub.fetchClubData(), this.onResumeCheckShare(), this.checkOnShowInfo()
                }, e.checkOnShowInfo = function() {
                    var e;
                    if (xyMini.getOnShowOption) {
                        var i = xyMini.getOnShowOption();
                        if (!i) return;
                        switch (n.platform) {
                            case n.Platform.ALIPAY_MINI_GAME:
                                "gamecenter" == (null == i || null == (e = i.query) ? void 0 : e.sourceChannel) && o.gms.dispatchEvent(u.AlipayGameCenterVisitSuc)
                        }
                    }
                }, e.checkShouldBan = function(e) {
                    if (o.U.sdk.isOnReviewMode()) {
                        return !![].includes(e) && (o.module.toast("common.jqqd", !0), !0)
                    }
                    return !1
                }, e.login = function(e) {
                    h.isBanshu ? o.module.open(s.BsLoginWin, {
                        onLoginSuc: e
                    }) : xyMini ? xyMini.login(e) : e({
                        code: 0,
                        data: {
                            unique_id: null,
                            package_id: 9999,
                            token: "none",
                            time: "0"
                        }
                    })
                }, e.pay = function(e, i) {
                    var t = this,
                        r = e < 2e6 ? o.cfg.get("Charge").get("ChargeConfig").get(e) : o.cfg.get("Charge").get("GiftConfig").get(e);
                    if (r) {
                        var u = o.D.server;
                        if (o.D.pay.checkCanBuy(e)) {
                            var d;
                            this.burialActivityAction(11, e);
                            var y = function() {
                                var y;
                                if (0 != r.money)
                                    if (!o.U.game.isEnoughItem(c, r.money, !1, !1) && !o.U.game.isEnoughItem(l, r.money, !1, !1) || r.props.some((function(e) {
                                            return e[0] == l
                                        })) || d || null != (y = i) && y.includes("discount"))
                                        if (h.isBanshu) o.bsMgr.checkPay(r.money, (function(i) {
                                            switch (i.d.payLimit) {
                                                case p.NORMAL:
                                                    o.proxy.pay.reqCharge(e, 100 * r.money);
                                                    break;
                                                default:
                                                    o.module.open(s.BsPayAlertWin, {
                                                        tips: i.d.toastMsg
                                                    })
                                            }
                                        }), (function(e) {
                                            o.module.toast(e.d.toastMsg, !0)
                                        }));
                                        else {
                                            var f = o.U.game.getChargeMoney(e);
                                            d && (f -= d);
                                            var g = 100 * f;
                                            if (h.pkgType !== m.DevLocal && 1 != a.config.query.data.showGM || n.isNative) {
                                                var M;
                                                if (!xyMini.pay) return void o.module.toast("支付接口不可用");
                                                var x = r.name,
                                                    v = {
                                                        role: t.makePayParamRole(),
                                                        product: {
                                                            product_id: "" + e,
                                                            product_name: x,
                                                            product_desc: x,
                                                            price: g,
                                                            goods_count: 1,
                                                            goods_name: r.goods_name
                                                        },
                                                        extra: null != (M = i) ? M : ""
                                                    };
                                                u.extendInfo && u.extendInfo.pay_pre && (v.product.iap_id = "" + u.extendInfo.pay_pre + r.money), xyMini.pay(v)
                                            } else o.proxy.pay.reqCharge(e, g, i)
                                        }
                                else o.module.open(s.VoucherBuyWin, {
                                    chargeId: e
                                });
                                else o.proxy.pay.reqBuyExternalVoucher(e)
                            };
                            if (o.U.cfg.isZfbCfgPlan()) {
                                var f = o.cfg.get("Charge.Config").discount_item;
                                if (f && f[r.money]) {
                                    var g = f[r.money],
                                        M = g[0],
                                        x = g[1];
                                    if (o.U.game.isEnoughItem(M, 1, !1, !1)) {
                                        var v = o.D.props.getOne(M);
                                        return void o.U.game.openAlertWin({
                                            titleName: o.U.game.localized("pay.tips2"),
                                            content: o.U.game.localized("pay.tips3", [v.name]),
                                            sureObj: {
                                                func: function() {
                                                    d = x, i = "discount," + M, y()
                                                }
                                            },
                                            cancelObj: {
                                                func: function() {
                                                    y()
                                                }
                                            }
                                        })
                                    }
                                }
                            }
                            y()
                        } else o.module.toast(o.U.game.localized("pay.buyLimit"))
                    }
                }, e.isDqjItemID = function(e) {
                    return e > 50 && e < 100
                }, e.makePayParamRole = function() {
                    var e = o.D.account,
                        i = o.D.role,
                        n = o.D.server;
                    return {
                        unique_id: e.uid,
                        package_id: e.gcid,
                        role_id: i.roleIdStr,
                        role_name: i.roleName,
                        server_id: n.server.id,
                        server_num: n.server.number,
                        server_name: n.server.name,
                        level: i.position,
                        vip_level: 0,
                        wallet: o.D.props.getCount(f.XianYu)
                    }
                }, e.commitRoleInfo = function(e, i, n) {
                    o.adPlayer.captureAdLocalFlag(), o.adPlayer.shouldDeferBurialPoint() ? this.deferCommitRoleInfo(e, i, n) : this.sendCommitRoleInfoNow(e, i, n)
                }, e.sendCommitRoleInfoNow = function(e, i, n) {
                    var t, a, r = o.D.account,
                        c = o.D.server,
                        l = o.D.role,
                        u = o.timer.getServerTime(),
                        s = null != (t = l.roleIdStr) ? t : "";
                    if (s && "-1" != s) {
                        var d = {
                            unique_id: r.uid,
                            package_id: "" + r.gcid,
                            specific_server_id: "" + c.server.server_id,
                            role_id: s,
                            role_name: null != (a = l.roleName) ? a : "",
                            combat: l.power,
                            server_id: "" + c.server.getLoginServerNid(),
                            server_num: "" + c.server.number,
                            server_name: c.server.name,
                            level: "" + (h.isEnterGame ? l.position : 1),
                            vip_level: "" + o.D.vip.vip,
                            wallet: "" + o.D.props.getCount(f.XianYu),
                            gender: "" + l.sex,
                            guild_name: "none",
                            server_opentime: "" + c.getFormattedServerOpenTime(),
                            now: "" + u,
                            role_create_time: "" + (h.isEnterGame ? o.D.role.createRoleTime : u)
                        };
                        "task" === e && (d.task_id = i, d.task_name = n, d.task_time = "0", d.task_type = "10"), xyMini && xyMini.commitRoleInfo ? xyMini.commitRoleInfo(e, d) : console.warn("commitRoleInfo", e, d)
                    }
                }, e.commitAlipayGameEvent = function(e) {
                    var i, n;
                    xyMini.commitAlipayGameEvent && (i = o.timer.getLocalTimeFormat(), n = o.D.role.roleIdStr + "_" + e + "_" + i, Logger.logBusiness("commitAlipayGameEvent", "action_code: " + e + ", action_finish_date: " + i + ", out_biz_no: " + n), xyMini.commitAlipayGameEvent(e, i, n))
                }, e.commitHuaweiQuickGameEvent = function(e) {
                    if (h.pkgType == m.DevLocal && console.log("commitHuaweiQuickGameEvent===>" + e), M.isHwQuick && o.D.role.roleID) {
                        var i = o.D.account;
                        xyMini.commitHuaweiQuickGameEvent && xyMini.commitHuaweiQuickGameEvent({
                            mark: o.D.role.roleIdStr + "_" + e + "_" + o.U.string.getRandomStr(10),
                            data: JSON.stringify([{
                                uid: i.uid,
                                roleId: o.D.role.roleIdStr,
                                eventKey: e,
                                eventTime: 1e3 * o.timer.getServerTime()
                            }])
                        })
                    }
                }, e.tellSdkReOnline = function() {
                    xyMini && xyMini.tellSdkReOnline && xyMini.tellSdkReOnline()
                }, e.tellSdkOffline = function() {
                    xyMini && xyMini.tellSdkOffline && xyMini.tellSdkOffline()
                }, e.nativeKeyboardEnable = function() {
                    return xyMini && xyMini.showNativeKeyboard
                }, e.shareEnable = function() {
                    return !(h.pkgType !== m.DevLocal && "xiaoyou" !== xyMini.plat && !xyMini.wxShare)
                }, e.wxShare = function(e, i, n) {
                    var t, a, c, l, u, s, d, y, f = this,
                        g = o.D.server,
                        p = o.D.role,
                        M = {
                            openid: null != (t = null == (a = xyMini.wxInfo) ? void 0 : a.open_id) ? t : o.U.string.getRandomStr(8),
                            appid: null != (c = null == (l = xyMini.pkgInfo) ? void 0 : l.appid) ? c : "wxappid",
                            nid: g.server.id,
                            role_id: p.roleIdStr,
                            role_name: p.roleName,
                            head: p.head,
                            head_frame: p.headFrame,
                            src_type: i,
                            invite_code: 2 == i ? null == n ? void 0 : n.invite_code : void 0
                        };
                    2 == i && h.pkgType == m.DevLocal && Logger.logBusiness("受邀请用户在登录前设置==> xyMini.share_query={role_id:" + p.roleID + "};");
                    var x = (null == (u = o.cfg.get("Share")) || null == (u = u.get("Share")) ? void 0 : u.get(i)) || {},
                        v = r.encode(encodeURIComponent(JSON.stringify(M))),
                        b = {
                            title: null != (s = x.dec) ? s : "这游戏，摸鱼必玩！",
                            imageUrlId: null != (d = x.imageUrlId) ? d : "F1K2Q3a4Temj9FOD3v+LBg==",
                            imageUrl: null != (y = x.imageUrl) ? y : "https://mmocgame.qpic.cn/wechatgame/UCvqVlJWyxsibVfZTuYnU3N1iaUZ18Ovgfib2ica7dnpswjiaYaQgibQZSys7LfC9yjiaVr/0",
                            share_query: v,
                            nid: g.server.id,
                            role_id: p.roleIdStr,
                            src_type: i
                        };
                    console.warn("wxShare", e, M, b), this._clickShareData = b, xyMini && xyMini.wxShare ? xyMini.wxShare(e, b, (function(e) {
                        e ? f.onResumeCheckShare() : f._clickShareData = null
                    })) : this.onResumeCheckShare()
                }, e.onResumeCheckShare = function() {
                    if (this._clickShareData) {
                        var e = this._clickShareData.src_type;
                        e || (e = 999), o.proxy.module.sendMsg(10013, {
                            type: e
                        }), this._clickShareData = null
                    }
                    this.checkShareQuery()
                }, e.checkShareQuery = function() {
                    if (xyMini && xyMini.share_query && xyMini.share_query.query) try {
                        var e = o.D.role,
                            i = JSON.parse(decodeURIComponent(r.decode(xyMini.share_query.query)));
                        i.role_id != e.roleIdStr && (console.warn("share_query", i), i.src_type)
                    } catch (e) {
                        console.error(e)
                    }
                }, e.adEnable = function() {
                    return xyMini.adEnable ? xyMini.adEnable() : !(!xyMini.watchVideoAd && h.pkgType !== m.DevLocal)
                }, e.watchAd = function(e) {
                    o.D.ad.canSkipAdWatch() ? e.run() : M.isAlipayPcWeb() ? o.module.openAct(g.ChaoZhiTeQuan) : (h.pkgType === m.DevLocal && o.module.toast("正在调用拉起广告接口"), xyMini && xyMini.watchVideoAd ? (xyMini.watchVideoAdCallback = function() {
                        o.sound.resumeAll(), e.run()
                    }, xyMini.watchVideoAdErrorCallback = function() {
                        o.sound.resumeAll(), xyMini.watchVideoAdCallback = null, xyMini.watchVideoAdErrorCallback = null
                    }, o.sound.pauseAll(), xyMini.watchVideoAd((function() {
                        xyMini.watchVideoAdCallback && o.timer.scheduleOnce((function() {
                            xyMini.watchVideoAdCallback && o.net.isConnected() && o.U.sdk.checkWatchVideoAdCb()
                        }), 2.8)
                    }), xyMini.watchVideoAdErrorCallback)) : e.run())
                }, e.burialPoint = function(e, i) {
                    o.adPlayer.captureAdLocalFlag(), o.adPlayer.shouldDeferBurialPoint() ? this.deferBurialPoint(e, i) : this.sendBurialPointNow(e, i)
                }, e.flushDeferredBurialPoints = function() {
                    var e = this,
                        i = this._deferredBurialPoints.concat();
                    this._deferredBurialPoints.length = 0, i.forEach((function(i) {
                        e.sendBurialPointNow(i.id, i.dec, i.created_at)
                    })), this.flushDeferredCommitRoleInfos()
                }, e.deferBurialPoint = function(e, i) {
                    this._deferredBurialPoints.some((function(i) {
                        return i.id === e
                    })) || this._deferredBurialPoints.push({
                        id: e,
                        dec: i,
                        created_at: this.getLocalTimeFormat()
                    })
                }, e.deferCommitRoleInfo = function(e, i, n) {
                    this._deferredCommitRoleInfos.push({
                        type: e,
                        taskId: i,
                        taskName: n
                    })
                }, e.flushDeferredCommitRoleInfos = function() {
                    var e = this;
                    if (!(this._deferredCommitRoleInfos.length <= 0)) {
                        var i = this._deferredCommitRoleInfos.concat();
                        this._deferredCommitRoleInfos.length = 0, i.forEach((function(i) {
                            e.sendCommitRoleInfoNow(i.type, i.taskId, i.taskName)
                        }))
                    }
                }, e.sendBurialPointNow = function(e, i, n) {
                    var t, r, c;
                    if (!(h.pkgType > m.DevLocal && "xiaoyou" === xyMini.plat)) {
                        var l = d.GameBurialPoint + "_" + e,
                            u = o.D.account,
                            s = null != (t = null != (r = null != (c = u.uid) ? c : xyMini.xyDeviceId) ? r : window.visitorId) ? t : "device_unique_id";
                        if (!a.storage.get(l, 0, s)) {
                            var f, g, p, M, x, v, b, S, k;
                            a.storage.set(l, "1", s);
                            var _ = o.D.server,
                                C = null != (f = null != (g = xyMini.xyServerId) ? g : null == (p = _.server) ? void 0 : p.server_id) ? f : "0",
                                w = null != (M = null != (x = xyMini.xyGcid) ? x : u.gcid) ? M : "0",
                                D = {
                                    code: e,
                                    created_at: null != n ? n : this.getLocalTimeFormat(),
                                    event: y(u.uid) ? "register_dot" : "device_dot",
                                    gcid: +w,
                                    nid: null != (v = null == (b = _.server) ? void 0 : b.id) ? v : 0,
                                    player_id: o.D.role ? o.D.role.roleID : 0,
                                    server_id: +C,
                                    uid: s,
                                    unique_id: s,
                                    version: "1.0.0"
                                };
                            i = null != (S = i) ? S : h.isEnterGame ? null == (k = o.cfg.get("DaDian.dot").get(e)) ? void 0 : k.dec : "", console.log("burialPoint===>[" + e + "]:" + i + "~~~" + s, D), xyMini.burialPoint && xyMini.burialPoint(D), 18 == e && this.commitRoleInfo("guide_done")
                        }
                    }
                }, e.burialActivityAction = function(e, i) {
                    var n, t;
                    if (!(h.pkgType > m.DevLocal && "xiaoyou" === xyMini.plat)) {
                        var a = o.D.account,
                            r = o.D.server;
                        if (!(r.open_days > 7 && e < 13)) {
                            var c = {
                                code: +("" + e + i),
                                created_at: this.getLocalTimeFormat(),
                                event: "active_dot",
                                gcid: +("" + a.gcid),
                                nid: null != (n = null == (t = r.server) ? void 0 : t.id) ? n : 0,
                                player_id: o.D.role ? o.D.role.roleID : 0,
                                server_id: r.server.id,
                                uid: a.uid
                            };
                            console.log("burialActivityAction===>[" + e + "]:" + i, JSON.stringify(c)), xyMini.burialPoint && xyMini.burialPoint(c)
                        }
                    }
                }, e.getLocalTimeFormat = function() {
                    var e = new Date;
                    return e.getFullYear() + "-" + String(e.getMonth() + 1).padStart(2, "0") + "-" + String(e.getDate()).padStart(2, "0") + " " + String(e.getHours()).padStart(2, "0") + ":" + String(e.getMinutes()).padStart(2, "0") + ":" + String(e.getSeconds()).padStart(2, "0")
                }, e.uploadAdAction = function(e, i) {}, e.onResSdkEvent = function(e) {
                    switch (console.log("onResSdkEvent", e), e.action) {
                        case "logout":
                            o.U.sdk.restartGame(!1);
                            break;
                        case "initAd":
                            xyMini.jhAdEnable = !0
                    }
                }, e.onResXySdkMsg = function(e) {
                    switch (console.warn("onResXySdkMsg", e), e.event) {
                        case "toast":
                            o.module.toast(e.data);
                            break;
                        case "adPlayerStatusChange":
                        case "onAdPlayerStatusChange":
                            o.adPlayer.onSdkAdPlayerStatusChange(e);
                            break;
                        case "onNetworkStatusChange":
                            e.params.isConnected && h.isEnterGame && o.timer.scheduleOnce((function() {
                                o.gameMgr.probeServerAlive(!0, "network-status-change")
                            }), .5);
                            break;
                        case "wxClubBtnOnTap":
                            o.gms.dispatchEvent(u.WXClubBtnOnTap);
                            break;
                        case "onMemoryWarning":
                            o.res.forceRelease();
                            break;
                        case "onAudioInterruptionEnd":
                            o.sound.resumeAll(!0);
                            break;
                        case "TTEditComp":
                            o.timer.scheduleOnce((function() {
                                o.gms.dispatchEvent(u.TTEditComp, {
                                    tag: o.U.sdk.getTTEditTag(),
                                    str: e.params
                                })
                            }), .1);
                            break;
                        case "onNativeLoginCb":
                            if (window.xyMini.loginCb) {
                                var i;
                                console.log("onNativeLoginCb==>  ===@@@>" + e.data);
                                var n = JSON.parse(e.data),
                                    t = null != (i = a.config.query.data.gcid) ? i : 9999,
                                    r = {
                                        token: n.token,
                                        game: n.game
                                    },
                                    c = xyMini.serviceUrl + "auth/token";
                                console.log("authReqUrl0000==>  ===>" + n.token), console.log("authReqUrl0001==>  ===>" + n.game), console.log("authReqUrl==>" + c + "  ===>" + JSON.stringify(r)), window.xyMini.httpPost(c, (function(e) {
                                    console.log("reqUrl==>" + c + "  result===>" + JSON.stringify(e)), e && e.data && window.xyMini.loginCb({
                                        code: e.code,
                                        msg: e.msg,
                                        data: {
                                            unique_id: e.data.auth_token_info.uid,
                                            time: e.data.auth_token_info.time,
                                            token: e.data.auth_token_info.token,
                                            package_id: t,
                                            game: n.game,
                                            application_id: n.application_id
                                        }
                                    })
                                }), r)
                            }
                            break;
                        case "onNativeKeyboradInput":
                            window.xyMini.onNativeKeyboardEditCb && (window.xyMini.onNativeKeyboardEditCb(e.data), window.xyMini.onNativeKeyboardEditCb = null);
                            break;
                        case "onNativeKeyboardHideCb":
                            window.xyMini.onNativeKeyboardHideCb = null;
                            break;
                        case "onNativePayCb":
                            break;
                        case "onNativeLogout":
                            o.net.rejectReconnect(), o.net.close(!1), o.timer.scheduleOnce((function() {
                                console.log("location.reload();"), location.reload()
                            }), .3);
                            break;
                        case "onNativeCheckSensitive":
                            if (xyMini.sensitiveCallback) {
                                var l = JSON.parse(e.data),
                                    s = !1;
                                l.d && "pass" != l.d.suggest && (s = !0), xyMini.sensitiveCallback({
                                    risky: s
                                }), xyMini.sensitiveCallback = null
                            }
                            break;
                        case "onNativeShowAdCb":
                            if (xyMini.watchVideoAdCallback) try {
                                if (2 == JSON.parse(e.data).code) return void(xyMini.watchVideoAdErrorCallback && xyMini.watchVideoAdErrorCallback());
                                o.timer.scheduleOnce((function() {
                                    o.net.isConnected() && o.U.sdk.checkWatchVideoAdCb()
                                }), 1)
                            } catch (e) {
                                console.error(e)
                            }
                    }
                }, e.checkWatchVideoAdCb = function() {
                    var e = !1;
                    return xyMini.watchVideoAdCallback && (xyMini.watchVideoAdCallback(), e = !0), xyMini.watchVideoAdCallback = null, xyMini.watchVideoAdErrorCallback = null, e
                }, e.listenTTEditComp = function() {
                    return this._ttEditTag = o.U.string.getRandomStr(5), this._ttEditTag
                }, e.getTTEditTag = function() {
                    return this._ttEditTag
                }, e.triggerGC = function() {
                    xyMini && xyMini.triggerGC && xyMini.triggerGC()
                }, e.checkSensitive = function(e) {
                    xyMini && xyMini.checkSensitive && null != e && e.content ? (xyMini.sensitiveCallback = function(i) {
                        e.callback && e.callback(i)
                    }, xyMini.checkSensitive(e)) : e.callback({
                        risky: !1
                    })
                }, e.restartGame = function(e) {
                    void 0 === e && (e = !0), h.isEnterGame = !1, xyMini && xyMini.restartGame ? xyMini.restartGame() : e && xyMini && xyMini.logout ? xyMini.logout((function() {})) : t.restart()
                }, e.copyToClipBoard = function(e, i) {
                    xyMini && xyMini.copyToClipBoard && xyMini.copyToClipBoard(e, i)
                }, e.isOnReviewMode = function() {
                    if (null == this._isReviewMode) {
                        var e, i = "1" == a.config.query.data.isReview,
                            n = i || !(null == (e = xyMini.pkgInfo) || !e.isReview);
                        this._isReviewMode = i || n && 0 != xyMini.showReview && M.isIOS() && !xyMini.customAccount
                    }
                    return this._isReviewMode
                }, e.reqSubscribe = function(e, i) {
                    void 0 === i && (i = !0), xyMini && xyMini.reqSubscribe && e.length > 0 ? (console.log("reqSubscribe"), xyMini.reqSubscribe({
                        types: e,
                        role_id: o.D.role.roleIdStr,
                        nid: o.D.server.server.id,
                        success: function() {
                            o.module.toast("订阅成功")
                        }
                    }, i)) : console.warn((i ? "" : "取消") + "订阅模板ID:" + e.toString())
                }, e.fetchSubInfoFromBackEnd = function() {
                    xyMini && xyMini.fetchSubInfoFromBackEnd && xyMini.fetchSubInfoFromBackEnd(o.D.role.roleIdStr)
                }, e.gameClubEnable = function() {
                    return !M.isAlipayPcWeb() && !(h.pkgType !== m.DevLocal && !xyMini.showWeChatGameClubButton)
                }, e.showWxGameClubBtn = function(e, i) {
                    xyMini && xyMini.showWeChatGameClubButton && xyMini.showWeChatGameClubButton({
                        width: e.width,
                        height: e.height,
                        left: i.x,
                        bottom: i.y
                    })
                }, e.hideWxGameClubBtn = function() {
                    xyMini && xyMini.hideWeChatGameClubButton && xyMini.hideWeChatGameClubButton()
                }, e.fetchWxClubData = function(e, i, n) {
                    void 0 === n && (n = !1), xyMini && xyMini.fetchWxClubData ? xyMini.fetchWxClubData(e, (function(e) {
                        i && i.runWith(e)
                    }), n) : i && i.runWith({
                        dataList: [{
                            dataType: 1,
                            value: o.timer.getServerTime()
                        }]
                    })
                }, e.wxKefuEnable = function() {
                    return !(h.pkgType !== m.DevLocal && !xyMini.openKefuChat)
                }, e.openKefuChat = function() {
                    xyMini && xyMini.openKefuChat && xyMini.openKefuChat()
                }, e.alertExit = function(e) {
                    xyMini && xyMini.alertExit && xyMini.alertExit(e)
                }, e.wxLog = function(e, i) {
                    xyMini && xyMini.wxLog ? xyMini.wxLog(e, i) : console.error(i)
                }, e.isOpenByDeskTopIcon = function() {
                    if (M.isMiniGame()) {
                        var e = xyMini.getLaunchScene();
                        if (M.isMeituanGame() && xyMini.getOnShowOption) {
                            var i, t = null == (i = xyMini.getOnShowOption()) ? void 0 : i.innerSource,
                                o = ["deskicon", "desk22card", "desk"];
                            return o.includes(t) || o.some((function(e) {
                                return null == t ? void 0 : t.endsWith("_" + e)
                            }))
                        }
                        switch (n.platform) {
                            case n.Platform.WECHAT_GAME:
                                return 1023 == e || 1223 == e;
                            case n.Platform.ALIPAY_MINI_GAME:
                                return 1023 == e;
                            case n.Platform.BYTEDANCE_MINI_GAME:
                                return ("" + e).includes("1020");
                            default:
                                return !1
                        }
                    }
                    return !1
                }, e.isOpenByLeftBar = function() {
                    if (M.isMiniGame()) {
                        var e = xyMini.getLaunchScene();
                        switch (n.platform) {
                            case n.Platform.BYTEDANCE_MINI_GAME:
                                return ("" + e).includes("1036");
                            default:
                                return !1
                        }
                    }
                    return !1
                }, e.isOpenByDyFeed = function() {
                    if (M.isMiniGame()) {
                        var e = xyMini.getLaunchScene();
                        switch (n.platform) {
                            case n.Platform.BYTEDANCE_MINI_GAME:
                                return ("" + e).includes("3041");
                            default:
                                return !1
                        }
                    }
                    return !1
                }, e.reportDyScene = function(e) {
                    xyMini.reportDyScene && xyMini.reportDyScene(e), console.log("xyMini.reportDyScene", e)
                }, e.qgHasShortcutInstalled = function(e) {
                    xyMini && xyMini.qgHasShortcutInstalled && xyMini.qgHasShortcutInstalled((function(i) {
                        e && e.runWith(i)
                    }))
                }, e.jumpToHuaweiH5Shop = function() {
                    M.isHwQuick && xyMini.openURL && xyMini.openURL("https://gtup-drcn.game.dbankcloud.cn/propsMall?source=1&appId=118163153&showBack=false&hwFullScreen=2")
                }, e.subscribeSecondFloorChannelEnable = function() {
                    return !(h.pkgType !== m.DevLocal && !xyMini.subscribeSecondFloorChannel)
                }, e.fetchSecondFloorSubState = function(e) {
                    xyMini.fetchSecondFloorSubState && xyMini.fetchSecondFloorSubState((function(i) {
                        e && e.runWith(i)
                    }))
                }, e.subscribeSecondFloorChannel = function(e) {
                    if (xyMini.subscribeSecondFloorChannel) {
                        xyMini.subscribeSecondFloorChannel((function(i) {
                            e && e.runWith(i)
                        }))
                    }
                }, e.isOpenByMtSecondFloor = function() {
                    var e, i = null == (e = xyMini.getOnShowOption()) ? void 0 : e.innerSource;
                    return !!i && i.includes("syxlel")
                }, e.addShortCutEnable = function() {
                    return !M.isAlipayPcWeb() && !(h.pkgType !== m.DevLocal && !xyMini.addShortCut)
                }, e.addShortCut = function(e) {
                    xyMini && xyMini.addShortCut && xyMini.addShortCut((function(i) {
                        o.module.toast("添加桌面快捷方式" + (0 === i.code ? "成功" : "失败") + "!"), e && e.runWith(i)
                    }))
                }, e.alipayReportCustomEvent = function(e) {
                    xyMini && xyMini.alipayReportCustomEvent && xyMini.alipayReportCustomEvent(e)
                }, e.addGameCenterEnable = function() {
                    return !M.isAlipayPcWeb() && !(h.pkgType !== m.DevLocal && !xyMini.addGameCenterEnable)
                }, e.hasAddGameCenterToMyApps = function(e) {
                    xyMini.hasAddGameCenterToMyApps ? xyMini.hasAddGameCenterToMyApps((function(i) {
                        e.runWith(i)
                    })) : e.runWith(!1)
                }, e.addGameCenterToMyApps = function(e) {
                    xyMini.addGameCenterToMyApps ? xyMini.addGameCenterToMyApps((function(i) {
                        i || o.module.toast("添加首页失败，请重试"), e.runWith(i)
                    })) : e.runWith(!0)
                }, e.goFufang = function() {
                    xyMini.goFufang && xyMini.goFufang()
                }, e.openURL = function(e) {
                    xyMini && xyMini.openURL ? xyMini.openURL(e) : n.openURL(e)
                }, e.dyLeftBarEnable = function() {
                    return !(!xyMini.openDyLeftBar && h.pkgType !== m.DevLocal)
                }, e.openDyLeftBar = function() {
                    xyMini && xyMini.openDyLeftBar && xyMini.openDyLeftBar()
                }, e.feedSubscribeEnable = function() {
                    return !(!xyMini.requestFeedSubscribe && h.pkgType !== m.DevLocal)
                }, e.checkFeedSubscribeStatus = function(e) {
                    xyMini.checkFeedSubscribeStatus ? xyMini.checkFeedSubscribeStatus((function(i) {
                        e && e.runWith(i)
                    })) : e && e.runWith(!1)
                }, e.requestFeedSubscribe = function(e) {
                    xyMini.requestFeedSubscribe ? xyMini.requestFeedSubscribe((function(i) {
                        o.module.toast("订阅成功"), e && e.runWith(i)
                    }), {
                        role_id: o.D.role.roleIdStr,
                        nid: o.D.server.server.id
                    }) : e && e.runWith(!0)
                }, e.showTTKefuBtn = function(e, i) {
                    xyMini && xyMini.showTTKefuBtn && xyMini.showTTKefuBtn({
                        width: e.width,
                        height: e.height,
                        left: i.x,
                        bottom: i.y
                    })
                }, e.hideTTKefuButton = function() {
                    xyMini && xyMini.hideTTKefuButton && xyMini.hideTTKefuButton()
                }, e.isGetNotchHeightEnable = function() {
                    return !!xyMini.getNotchHeight
                }, e.getNotchHeight = function(e) {
                    if (xyMini.getNotchHeight) xyMini.getNotchHeight((function(i) {
                        if (i && i.data && i.data.notch_screen_height) {
                            var n = parseInt("" + i.data.notch_screen_height);
                            e && e.runWith(n)
                        } else e && e.runWith(0)
                    }));
                    else {
                        var i, n = null != (i = a.config.query.data.barH) ? i : "0";
                        e && e.runWith(+n)
                    }
                }, e.requestPost = function(e) {
                    xyMini && xyMini.requestPost && xyMini.requestPost(e)
                }, e
            }());
            x._deferredBurialPoints = [], x._deferredCommitRoleInfos = [], x._clickShareData = void 0, x._ttEditTag = void 0, x._isReviewMode = null, i._RF.pop()
        }
    }
}));