/* ===== chunks:///_virtual/XyNative.ts ===== */
System.register("chunks:///_virtual/XyNative.ts", ["cc", "./Core.ts", "./GlobalVar.ts"], (function(e) {
    var t, i, n, o, a, c;
    return {
        setters: [function(e) {
            t = e.cclegacy, i = e.native, n = e.game
        }, function(e) {
            o = e.xy
        }, function(e) {
            a = e.GlobalVar, c = e.GamePkgType
        }],
        execute: function() {
            t._RF.push({}, "10e3aZdaaNKNbOzYZJ3i/lw", "XyNative", void 0), e("XyNative", function() {
                function e() {}
                return e.init = function() {
                    window.xyMini = {
                        yyPlate: "minigame",
                        customAccount: !1,
                        cdnRoot: "https://tf-cdn.shryou.com/",
                        serviceUrl: "https://tf-api.shryou.com/",
                        serviceUrl2: "https://tf-api.shryou.com/",
                        payServiceUrl: "https://tf-api.shryou.com/",
                        dotServiceUrl: "https://tf-api.shryou.com/",
                        battleServiceUrl: "https://tf-cdn.shryou.com/",
                        resetDomin: function() {},
                        checkOnReviewMode: function() {
                            i.jsbBridgeWrapper.dispatchEventToNative("doNativeCheckReviewMode")
                        },
                        getDeviceUUID: function() {
                            i.jsbBridgeWrapper.dispatchEventToNative("doNativeGetDeviceUUID")
                        },
                        login: function(e) {
                            window.xyMini.loginCb = e, a.pkgType === c.DevLocal ? (window.xyMini.plat = "xiaoyou", window.xyMini.customAccount = !0, e({
                                code: 0,
                                data: {
                                    package_id: 9999
                                }
                            })) : i.jsbBridgeWrapper.dispatchEventToNative("doNativeLogin")
                        },
                        logout: function(e) {
                            i.jsbBridgeWrapper.dispatchEventToNative("doNativeLogout")
                        },
                        pay: function(e) {},
                        restartGame: function() {
                            n.restart()
                        },
                        share: function(e) {},
                        commitRoleInfo: function(e, t) {
                            t.eventType = e, i.jsbBridgeWrapper.dispatchEventToNative("doNativeCommitRoleInfo", JSON.stringify(t))
                        },
                        burialPoint: function(e) {
                            var t = xyMini.dotServiceUrl + "api/v1/log?",
                                i = Object.keys(e);
                            i.map((function(n, o) {
                                o == i.length - 1 ? t += n + "=" + e[n] : t += n + "=" + e[n] + "&"
                            })), xyMini.httpGet(t, (function(e) {
                                console.log("burialPoint==>result:[" + e.code + "]", e)
                            }), "json")
                        },
                        copyToClipBoard: function(e, t) {},
                        getHttpParams: function(e, t, i) {
                            var n = t || "",
                                o = (i = i || window, new RegExp("(\\?|#|&)" + e + "=([^&#]*)(&|#|$)")),
                                a = [];
                            if (window && window.OPEN_DATA) a = window.OPEN_DATA.appurl.match(o);
                            else {
                                if (!(i && i.location && i.location.href)) return n;
                                a = i.location.href.match(o)
                            }
                            return decodeURIComponent(a ? a[2] : n)
                        },
                        httpGet: function(e, t, i) {
                            var n = null;
                            (n = window.XMLHttpRequest ? new XMLHttpRequest : new ActiveXObject("Microsoft.XMLHTTP")) && (n.onreadystatechange = function() {
                                4 == n.readyState && t && t("json" == i ? JSON.parse(n.responseText) : n.responseText)
                            }, n.open("GET", e, !0), n.send())
                        },
                        httpPost: function(e, t, i) {
                            var n;
                            (n = window.XMLHttpRequest ? new XMLHttpRequest : new ActiveXObject("Microsoft.XMLHTTP")).onreadystatechange = function() {
                                4 == n.readyState && 200 == n.status && t && t(JSON.parse(n.responseText))
                            }, n.open("POST", e, !0), n.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                            var o = "",
                                a = Object.keys(i);
                            a.map((function(e, t) {
                                t == a.length - 1 ? o += e + "=" + i[e] : o += e + "=" + i[e] + "&"
                            })), n.send(o)
                        },
                        getRandomStr: function(e) {
                            for (var t = "", i = 0; i < e; i++) t += "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789" [Math.floor(62 * Math.random())];
                            return t
                        }
                    }, i.jsbBridgeWrapper.addNativeEventListener("onNativeReviewModeCb", (function(e) {})), i.jsbBridgeWrapper.addNativeEventListener("onNativeGetDeviceUUIDCb", (function(e) {
                        e && (console.log("设备唯一ID==>" + e), window.visitorId = e)
                    })), i.jsbBridgeWrapper.addNativeEventListener("onNativeLoginCb", (function(e) {
                        if (window.xyMini.loginCb) {
                            var t = JSON.parse(e),
                                i = {
                                    type: "android",
                                    access_token: t.access_token,
                                    appid: t.app_id,
                                    cch_id: t.package_id
                                },
                                n = xyMini.serviceUrl + "auth/appLogin";
                            window.xyMini.httpPost(n, (function(e) {
                                console.log("reqUrl==>" + n + "  result===>" + JSON.stringify(e)), e && e.data && window.xyMini.loginCb({
                                    code: e.code,
                                    msg: e.msg,
                                    data: {
                                        unique_id: e.data.openid,
                                        package_id: t.package_id,
                                        access_token: t.access_token,
                                        app_id: t.app_id,
                                        application_id: t.application_id
                                    }
                                })
                            }), i)
                        }
                    })), i.jsbBridgeWrapper.addNativeEventListener("onNativePayCb", (function(e) {})), i.jsbBridgeWrapper.addNativeEventListener("onNativeLogout", (function(e) {
                        o.net.rejectReconnect(), o.net.close(!1), o.timer.scheduleOnce((function() {
                            n.restart()
                        }), .3)
                    })), i.jsbBridgeWrapper.addNativeEventListener("onNativeCloseAd", (function(e) {})), xyMini.checkOnReviewMode(), xyMini.getDeviceUUID()
                }, e
            }()).priceMap = {}, t._RF.pop()
        }
    }
}));