window.__require = function e(t, o, n) {
function i(c, r) {
if (!o[c]) {
if (!t[c]) {
var s = c.split("/");
s = s[s.length - 1];
if (!t[s]) {
var l = "function" == typeof __require && __require;
if (!r && l) return l(s, !0);
if (a) return a(s, !0);
throw new Error("Cannot find module '" + c + "'");
}
}
var d = o[c] = {
exports: {}
};
t[c][0].call(d.exports, function(e) {
return i(t[c][1][e] || e);
}, d, d.exports, e, t, o, n);
}
return o[c].exports;
}
for (var a = "function" == typeof __require && __require, c = 0; c < n.length; c++) i(n[c]);
return i;
}({
"3DCamera": [ function(e, t, o) {
"use strict";
cc._RF.push(t, "f7b35jgUZdGorkk9XPMUfR8", "3DCamera");
cc.Class({
extends: cc.Component,
properties: {},
onLoad: function() {
this.adjustViewPort();
window.GameLogic || (window.GameLogic = {});
var e = this;
window.GameLogic.adjust3DCamera = function() {
e.adjustViewPort();
};
},
adjustViewPort: function() {
var e = cc.view.getCanvasSize();
e.width /= cc.view.getScaleX();
e.height /= cc.view.getScaleY();
var t = this.node.z / 800 * e.height;
this.node.z = t;
this.node.is3DNode = !0;
console.log("[3DCamera] ", e, t);
},
start: function() {}
});
cc._RF.pop();
}, {} ],
Entry: [ function(e, t, o) {
"use strict";
cc._RF.push(t, "e8175O/dcRBF4KjZ3tFPt5A", "Entry");
cc.Class({
extends: cc.Component,
properties: {
progressBar: cc.ProgressBar,
progressText: cc.Label,
bgSprite: cc.Sprite,
logoName: cc.Sprite,
bgChar: cc.Sprite,
logText: cc.Label,
bgMask: cc.Sprite,
spineNode: cc.Node
},
checkServerAvailable: function(e, t) {
var o = window.GameLogic.getChannel(), n = window.GameLogic.getSubChannel(), i = window.GameLogic.getApkVersion(), a = "";
a = "td_openew_aborad" == o ? "https://rings.aborad1.openew.com:5010/server_status/fetch?channel=" + o + "&subChannel=" + n + "&timestamp=" + Date.now() : "td_openew_jp" == o ? "https://rings.jpapi.openew.com:5010/server_status/fetch?channel=" + o + "&subChannel=" + n + "&timestamp=" + Date.now() : "xgw_habby" == o && window.GameLogic.isHabby ? "https://gameapi-xgwhb.openew.com/server_status/fetch?channel=" + o + "&subChannel=" + n + "&timestamp=" + Date.now() : "https://gameapi.jinhaibuluo2.com/server_status/fetch?channel=" + o + "&subChannel=" + n + "&timestamp=" + Date.now();
a += "&server=examine";
var c = cc.sys.localStorage.getItem("TapTrackId"), r = cc.sys.os == cc.sys.OS_ANDROID && "taptap" == n && i >= 23;
if (!c && t && (r || cc.sys.os == cc.sys.OS_IOS)) {
a += "&idfa=";
if (cc.sys.os == cc.sys.OS_IOS) {
var s = jsb.reflection.callStaticMethod("AppController", "getIDFA");
if (s) {
a += s;
console.log("curIDFA: " + s);
cc.sys.localStorage.setItem("idfa", s);
}
}
a += "&imei=";
if (cc.sys.os == cc.sys.OS_ANDROID) {
var l = jsb.reflection.callStaticMethod("com/openew/game/utils/NativeCall", "getIMEI", "()Ljava/lang/String;");
a += l;
console.log("curIMEI: " + l);
cc.sys.localStorage.setItem("imei", l);
}
a += "&oaid=";
if (cc.sys.os == cc.sys.OS_ANDROID) {
var d = jsb.reflection.callStaticMethod("com/openew/game/utils/NativeCall", "getOAID", "()Ljava/lang/String;");
a += d;
console.log("curOAID: " + d);
cc.sys.localStorage.setItem("oaid", d);
}
a += "&device=";
if (cc.sys.os == cc.sys.OS_ANDROID) {
a += "0";
cc.sys.localStorage.setItem("device", "0");
} else if (cc.sys.os == cc.sys.OS_IOS) {
a += "1";
cc.sys.localStorage.setItem("device", "1");
}
a += "&device_model=";
if (cc.sys.os == cc.sys.OS_ANDROID) {
var u = jsb.reflection.callStaticMethod("com/openew/game/utils/NativeCall", "getDeviceModel", "()Ljava/lang/String;");
a += u;
console.log("curDeviceModel: " + u);
cc.sys.localStorage.setItem("device_model", u);
}
a += "&device_brand=";
if (cc.sys.os == cc.sys.OS_ANDROID) {
var g = jsb.reflection.callStaticMethod("com/openew/game/utils/NativeCall", "getDeviceBrand", "()Ljava/lang/String;");
a += g;
console.log("curDeviceBrand: " + g);
cc.sys.localStorage.setItem("device_brand", g);
}
a += "&os_version=" + cc.sys.osVersion;
cc.sys.localStorage.setItem("os_version", cc.sys.osVersion);
console.log("check url: " + a);
}
var f = cc.loader.getXMLHttpRequest();
f.open("GET", a);
f.responseType = "text";
f.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
f.onreadystatechange = function() {
if (4 == f.readyState) {
if (f.status >= 200 && f.status < 300) {
var o = JSON.parse(f.responseText);
if (0 == o.status) {
if (!c && t) {
console.log("response.tapTrackId: " + o.tapTrackId);
console.log("response.tapProjectId: " + o.tapProjectId);
console.log("response.tapDeepCallbackUrl: " + o.tapDeepCallbackUrl);
if (o.tapTrackId && "" != o.tapTrackId) {
cc.sys.localStorage.setItem("TapTrackId", o.tapTrackId);
cc.sys.localStorage.setItem("TapProjectId", o.tapProjectId);
cc.sys.localStorage.setItem("TapDeepCallbackUrl", o.tapDeepCallbackUrl);
}
}
window.GameLogic.userVars = o.vars;
e(!0, "");
} else {
var n = o.lang2Messages;
console.log(n);
for (var i = window.GameLogic.getLocale(), a = 0; a < n.length; ++a) if (n[a].lang == i) {
e(!1, n[a].message);
return;
}
var r = window.GameLogic.confirmLoginFailed().confirMessage;
e(!1, r);
}
}
console.log("checkServerAvailable request error status: ", f.status);
}
};
f.onload = function() {};
f.ontimeout = function() {
var t = window.GameLogic.confirmLoginFailed().noNetwork;
e(!1, t);
};
f.onerror = function() {
var t = window.GameLogic.confirmLoginFailed().noNetwork;
e(!1, t);
};
f.send();
},
showResetResDialog: function() {
if (cc.sys.os == cc.sys.OS_ANDROID) {
var e = function() {
jsb.reflection.callStaticMethod("com/openew/game/utils/NativeCall", "cleanRes", "()V");
setTimeout(function() {
cc.audioEngine.stopAll();
cc.game.restart();
}, 1e3);
};
cc.loader.loadRes("Prefabs/alertPanel", function(t, o) {
if (t) {
cc.log("载入预制资源失败, 原因:" + t);
e();
} else if (o instanceof cc.Prefab) {
var n = cc.instantiate(o);
n.x = 0;
n.y = 0;
cc.find("Canvas").addChild(n);
var i = n.getComponent("alertPanel");
i.setHint("确定");
i.setTitle("游戏资源加载失败，现进行清除缓存并重新启动？(如果资源加载依然失败，请先退出游戏，并且进行手动清除缓存操作)");
i.setCallback(e);
if (cc.sys.platform != cc.sys.WECHAT_GAME) {
n.scale = 0;
var a = cc.scaleTo(.4, .5).easing(cc.easeBackOut());
n.runAction(a);
}
} else {
cc.log("你载入的不是预制资源!");
e();
}
});
}
},
showServerUnavailbleAlertPanel: function(e, t, o) {
cc.loader.loadRes("Prefabs/alertPanel", function(n, i) {
if (n) {
cc.log("载入预制资源失败, 原因:" + n);
o(!0);
} else if (i instanceof cc.Prefab) {
var a = cc.instantiate(i);
a.x = 0;
a.y = 0;
cc.find("Canvas").addChild(a);
var c = a.getComponent("alertPanel"), r = window.GameLogic.confirmLoginFailed().tip;
c.setHint(e);
c.setTitle(t);
c.setCallback(o);
c.setTip(r);
if (cc.sys.platform != cc.sys.WECHAT_GAME) {
a.scale = 0;
var s = cc.scaleTo(.4, .5).easing(cc.easeBackOut());
a.runAction(s);
}
} else {
cc.log("你载入的不是预制资源!");
o(!0);
}
});
},
wxLoadSubpackage: function(t) {
this.updateLoadingProgress(0, "载入中");
var o = this;
(function(t, n) {
if (wx.loadSubpackage) wx.loadSubpackage({
name: t,
success: function(e) {
n && n();
},
fail: function(e) {
console.error(e);
}
}).onProgressUpdate(function(e) {
console.log("下载进度", t, e.progress);
console.log("已经下载的数据长度", t, e.totalBytesWritten);
console.log("预期需要下载的数据总长度", t, e.totalBytesExpectedToWrite);
o.updateLoadingProgress(e.progress / 100, "载入中");
}); else {
e("subpackages/" + t + "/game.js");
n && n();
}
})("GameLogic", function() {
o.updateLoadingProgress(0, "载入完成");
t();
});
},
checkSvrAndLoadGame: function() {
var e = this;
this.checkServerAvailable(function(t, o) {
console.log("检测服务器状态：", t, o);
try {
if (t) e.startGame(); else {
var n = window.GameLogic.confirmLoginFailed().confirTitle;
e.showServerUnavailbleAlertPanel(n, o, function() {
cc.audioEngine.stopAll();
cc.game.restart();
});
}
} catch (e) {
console.log("checkSvrAndLoadGame js error!" + e);
window.GameLogic.showResetResDialog && window.GameLogic.showResetResDialog();
} finally {
console.log("checkSvrAndLoadGame js finish!");
}
}, !1);
},
startGame: function() {
console.log("startGame....");
this.updateLoadingProgress(0);
if ((!cc.sys.isNative || cc.sys.platform == cc.sys.OPPO_GAME || cc.sys.platform == cc.sys.HUAWEI_GAME || cc.sys.platform == cc.sys.VIVO_GAME) && cc.sys.platform == cc.sys.WECHAT_GAME) {
cc.macro.ENABLE_CULLING = !1;
cc.game.once(cc.game.EVENT_RENDERER_INITED, function() {
var e = cc.Texture2D.prototype.handleLoadedTexture;
cc.Texture2D.prototype.handleLoadedTexture = function(t) {
e.call(this, t);
this._image.src = "";
};
});
}
window.GameLogic.entry();
},
shouldLoadSubPackage: function() {
return cc.sys.platform == cc.sys.WECHAT_GAME && !(window.GameLogic && window.GameLogic.isTTGame && window.GameLogic.isTTGame());
},
uploadLaunchOptions: function() {},
fillLogo: function(e) {
var t = this, o = e;
o || (o = window.GameLogic.getLocale());
var n = window.GameLogic.getChannel();
if ("td_openew_aborad" == n) {
var i = "logo/logo_openew_en";
"hk" == o && (o = "tw");
"tw" != o && "kr" != o || (i = "logo/logo_openew_" + o);
cc.loader.loadRes(i, cc.SpriteFrame, function(e, o) {
if (e) console.error(e); else {
t.logoName.spriteFrame = o;
t.logoName.node.opacity = 255;
console.log(o);
}
});
} else if ("td_openew_jp" == n) {
i = "logo/logo_openew_jp";
cc.loader.loadRes(i, cc.SpriteFrame, function(e, o) {
if (e) console.error(e); else {
t.logoName.spriteFrame = o;
t.logoName.node.opacity = 255;
console.log(o);
}
});
} else if ("td_openew_dameng" == n) {
var a = this, c = window.GameLogic.getExtraGameId();
if ("openew" == c || "mofa" == c) {
i = "logo/logo_" + c;
cc.loader.loadRes(i, cc.SpriteFrame, function(e, t) {
if (e) console.error(e); else {
a.logoName.spriteFrame = t;
a.logoName.node.opacity = 255;
console.log(t);
}
});
} else a.logoName.node.opacity = 255;
} else if ("td_openew_tianjing" == n) {
a = this, i = "logo/logo_tianjing_xiaoxiao";
var r = window.GameLogic.getGameId();
console.log("gameId: " + r);
if (r) if ("20085" == r || "20094" == r || "20097" == r || "20100" == r || "20112" == r || "20115" == r || "20142" == r) {
i = "logo/logo_tianjing_xiaoxiao";
a.bgSprite.node.opacity = 255;
} else if ("20088" == r || "20091" == r || "20109" == r) {
i = "logo/logo_tianjing_shilaimu";
a.bgSprite.node.opacity = 255;
} else if ("20118" == r || "20124" == r || "20130" == r || "20136" == r) {
i = "logo/logo_tianjing_zhijian";
a.bgSprite.node.opacity = 255;
} else if ("20121" == r || "20127" == r || "20133" == r || "20139" == r) {
i = "logo/logo_tianjing_baowei";
a.bgSprite.node.opacity = 255;
} else if ("20175" == r) {
i = "logo/logo_tianjing_mofawudi";
cc.loader.loadRes("logo/loading_tianjing_mofawudi", cc.SpriteFrame, function(e, t) {
if (e) console.error(e); else {
a.bgSprite.spriteFrame = t;
a.bgSprite.node.opacity = 255;
console.log(t);
}
});
} else a.bgSprite.node.opacity = 255;
cc.loader.loadRes(i, cc.SpriteFrame, function(e, t) {
if (e) console.error(e); else {
a.logoName.spriteFrame = t;
a.logoName.node.opacity = 255;
console.log(t);
}
});
} else if ("td_openew_qianguo" == n) {
window.GameLogic.getApkVersion() > 200 && (this.logoName.node.opacity = 255);
} else if (("xgw_habby" == n || "debug" == n) && "cn" != o) {
i = "logo/logo_openew_" + o;
cc.loader.loadRes(i, cc.SpriteFrame, function(e, o) {
if (e) console.error(e); else {
t.logoName.spriteFrame = o;
t.logoName.node.opacity = 255;
console.log(o);
}
});
}
this.checkSvrAndLoadGame();
},
onLoad: function() {
this.uploadLaunchOptions();
cc.sys.HUAWEI_GAME || (cc.sys.HUAWEI_GAME = 110);
cc.sys.isNative && cc.sys.os == cc.sys.OS_ANDROID && (cc.sys.mainLoopUpdate = jsb.mainLoopUpdate);
this.updateLoadingProgress(0);
this.adjustBg();
cc.sys.os == cc.sys.OS_ANDROID ? setTimeout(function() {
var e = JSON.stringify({
msg: "onShowLoading",
args: {}
});
cc.native.callNative(e);
}, 2e3) : setTimeout(function() {
var e = JSON.stringify({
msg: "onShowLoading",
args: {}
});
cc.native.callNative(e);
}, 1);
window.GameLogic || (window.GameLogic = {});
window.GameLogic.updateLoadingProgress = this.updateLoadingProgress.bind(this);
window.GameLogic.updateLoadingHint = this.updateLoadingHint.bind(this);
window.GameLogic.destroyLoading = this.destroyLoading.bind(this);
window.GameLogic.checkServerAvailable = this.checkServerAvailable.bind(this);
window.GameLogic.showResetResDialog = this.showResetResDialog.bind(this);
window.GameLogic.HabbyID = HabbyID;
window.GameLogic.HabbyStore = HabbyStore;
if (this.shouldLoadSubPackage()) {
var e = this;
this.wxLoadSubpackage(function() {
window.GameLogic.prepareUpdate(e.fillLogo.bind(e));
});
} else window.GameLogic.prepareUpdate(this.fillLogo.bind(this));
},
start: function() {
window.GameLogic && window.GameLogic.start && window.GameLogic.start();
},
update: function(e) {
window.GameLogic && window.GameLogic.update && window.GameLogic.update(e);
},
onDestroy: function() {
window.GameLogic && window.GameLogic.destroy && window.GameLogic.destroy();
},
adjustBg: function() {
var e = cc.view.getCanvasSize();
e.width /= cc.view.getScaleX();
e.height /= cc.view.getScaleY();
var t = this.bgSprite.node.width, o = this.bgSprite.node.height, n = e.width / t, i = e.height / o, a = n > i ? n : i;
this.bgSprite.node.scaleX = a;
this.bgSprite.node.scaleY = a;
console.log(this.bgSprite, e, n, i);
var c = e.width / this.bgChar.node.width;
this.bgChar.node.scaleX = c;
this.bgChar.node.scaleY = c;
var r = cc.view.getFrameSize();
if (r.width / r.height > .6 && this.logoName && this.logoName.node) {
this.logoName.node.scaleX *= .65;
this.logoName.node.scaleY *= .65;
}
},
updateLoadingProgress: function(e, t) {
if (void 0 != e) if (e >= 1 || e <= 0) {
this.progressBar.progress = 0;
this.progressBar.node.opacity = 0;
this.progressText.string = t || "";
} else {
this.progressBar.progress = e;
this.progressBar.node.opacity = 255;
this.progressText.string = t || Math.floor(100 * e) + "%";
}
},
updateLoadingHint: function(e) {
this.progressText.string = e;
},
destroyLoading: function() {
var e = cc.find("Canvas");
if (this.progressBar) {
this.progressBar.destroy();
this.progressBar = null;
}
if (this.progressText) {
this.progressText.destroy();
this.progressText = null;
}
if (this.bgSprite) {
this.bgSprite.destroy();
this.bgSprite = null;
}
if (this.logText) {
this.logText.destroy();
this.logText = null;
}
if (this.logoName) {
this.logoName.destroy();
this.logoName = null;
}
if (this.bgChar) {
this.bgChar.destroy();
this.bgChar = null;
}
if (this.spineNode) {
this.spineNode.destroy();
this.spineNode = null;
}
if (this.bgMask) {
this.bgMask.destroy();
this.bgMask = null;
}
console.log("destroy loading " + e.children.length);
}
});
cc._RF.pop();
}, {} ],
MyLog: [ function(e, t, o) {
"use strict";
cc._RF.push(t, "7882fCVVAVC2b5+8qt2ndzs", "MyLog");
cc.Class({
extends: cc.Component,
properties: {
label: cc.Node,
view: cc.Node
},
onLoad: function() {
console.log("查看执行顺序mylog");
window.GameLogic.addText = this.AddLog.bind(this);
},
start: function() {
cc.systemEvent.on("keydown", this.onKeyDown, this);
},
onKeyDown: function(e) {
e.keyCode == cc.macro.KEY.j && window.GameLogic.addText("----");
},
AddLog: function(e) {
if (this.label) try {
var t = cc.instantiate(this.label);
t.getComponent(cc.Label).string = e;
t.active = !0;
this.view.addChild(t);
t.setSiblingIndex(0);
this.view.setContentSize(this.view.width, t.height * this.view.childrenCount);
} catch (e) {
console.error(e);
} else console.log("没绑自己啊");
},
update: function(e) {}
});
cc._RF.pop();
}, {} ],
alertPanel: [ function(e, t, o) {
"use strict";
cc._RF.push(t, "f4046kZcFdIYb/TcQLNq/1M", "alertPanel");
cc.Class({
extends: cc.Component,
properties: {
hint: cc.Label,
title: cc.Label,
tip: cc.Label,
confirmBtn: cc.Button,
_callback: Function
},
onConfirm: function() {
if (this._callback) {
this._callback();
this._callback = null;
}
this.node.parent.removeChild(this.node);
},
setHint: function(e) {
this.hint.string = e;
},
setTitle: function(e) {
this.title.string = e;
},
setCallback: function(e) {
this._callback = e;
},
setTip: function(e) {
this.tip.string = e;
}
});
cc._RF.pop();
}, {} ],
"dm_tt_mg.min": [ function(e, t, o) {
"use strict";
cc._RF.push(t, "8912aWl8D5CDLM1KkpMK+zH", "dm_tt_mg.min");
var n = function() {
function e(e, t) {
for (var o = 0; o < t.length; o++) {
var n = t[o];
n.enumerable = n.enumerable || !1;
n.configurable = !0;
"value" in n && (n.writable = !0);
Object.defineProperty(e, n.key, n);
}
}
return function(t, o, n) {
o && e(t.prototype, o);
n && e(t, n);
return t;
};
}(), i = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function(e) {
return typeof e;
} : function(e) {
return e && "function" == typeof Symbol && e.constructor === Symbol && e !== Symbol.prototype ? "symbol" : typeof e;
};
function a(e, t) {
if (!(e instanceof t)) throw new TypeError("Cannot call a class as a function");
}
if (window.tt) {
!function(e, n) {
"object" == ("undefined" == typeof o ? "undefined" : i(o)) && "object" == ("undefined" == typeof t ? "undefined" : i(t)) ? t.exports = n() : "function" == typeof define && define.amd ? define([], n) : "object" == ("undefined" == typeof o ? "undefined" : i(o)) ? o.DM_TT_MG = n() : e.DM_TT_MG = n();
}(window, function() {
return function() {
var e = {
d: function(t, o) {
for (var n in o) e.o(o, n) && !e.o(t, n) && Object.defineProperty(t, n, {
enumerable: !0,
get: o[n]
});
},
o: function(e, t) {
return Object.prototype.hasOwnProperty.call(e, t);
},
r: function(e) {
"undefined" != typeof Symbol && Symbol.toStringTag && Object.defineProperty(e, Symbol.toStringTag, {
value: "Module"
}), Object.defineProperty(e, "__esModule", {
value: !0
});
}
}, t = {};
e.r(t), e.d(t, {
default: function() {
return p;
}
});
var o = {}, c = Array.prototype, r = Object.prototype.toString, s = Object.prototype.hasOwnProperty, l = c.forEach, d = Array.isArray, u = {};
o.each = function(e, t, o) {
if (null === e) return !1;
if (l && e.forEach == l) e.forEach(t, o); else if (e.length === +e.length) {
for (var n = 0; n < e.length; n++) if (n in e && t.call(o, e[n], n, e) === u) return !1;
} else for (var i in e) s.call(e, i) && t.call(o, e[i], i, e);
}, o.isArray = d || function(e) {
return "[object Array]" === r.call(e);
}, o.isPromise = function(e) {
return "[object Promise]" === r.call(e) && null !== e;
}, o.isObject = function(e) {
return "[object Object]" === r.call(e) && null !== e;
}, o.isEmptyObject = function(e) {
if (!o.isObject(e)) return !1;
for (var t in e) if (s.call(e, t)) return !1;
}, o.isUndefined = function(e) {
return void 0 === e;
}, o.isString = function(e) {
return "[object String]" === r.call(e);
}, o.isDate = function(e) {
return "[object Date]" === r.call(e);
}, o.isBoolean = function(e) {
return "[object Boolean]" === r.call(e);
}, o.isNumber = function(e) {
return "[object Number]" === r.call(e) && /[\d\.]+/.test(String(e));
}, o.isJSONString = function(e) {
try {
JSON.parse(e);
} catch (e) {
return !1;
}
return !0;
}, o.decodeURIComponent = function(e) {
var t = "";
try {
t = decodeURIComponent(e);
} catch (o) {
t = e;
}
return t;
}, o.encodeDates = function(e) {
return o.each(e, function(t, n) {
o.isDate(t) ? e[n] = o.formatDate(t) : o.isObject(t) && (e[n] = o.encodeDates(t));
}), e;
}, o.formatDate = function(e) {
function t(e) {
return e < 10 ? "0" + e : e;
}
return e.getFullYear() + "-" + t(e.getMonth() + 1) + "-" + t(e.getDate()) + " " + t(e.getHours()) + ":" + t(e.getMinutes()) + ":" + t(e.getSeconds()) + "." + ((o = e.getMilliseconds()) < 100 && o > 9 ? "0" + o : o < 10 ? "00" + o : o);
var o;
}, o.searchObjDate = function(e) {
try {
(o.isObject(e) || o.isArray(e)) && o.each(e, function(t, n) {
o.isObject(t) || o.isArray(t) ? o.searchObjDate(e[n]) : o.isDate(t) && (e[n] = o.formatDate(t));
});
} catch (e) {
g.warn(e);
}
}, o.UUID = function() {
var e = new Date().getTime();
return String(Math.random()).replace(".", "").slice(1, 11) + "-" + e;
}, o.UUIDv4 = function() {
return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(e) {
var t = 16 * Math.random() | 0;
return ("x" == e ? t : 3 & t | 8).toString(16);
});
}, o.getMpPlatform = function() {
return o.mpPlatform;
};
var g = "object" == ("undefined" == typeof g ? "undefined" : i(g)) ? g : {};
function f(e) {
return new Promise(function(t, o) {
tt.request({
url: e.url,
method: e.method || "GET",
data: e.data,
success: function(e) {
t(e);
},
fail: function(e) {
o(e);
}
});
});
}
g.info = function() {
if ("object" == ("undefined" == typeof console ? "undefined" : i(console)) && console.log && g.enabled) try {
return console.log.apply(console, arguments);
} catch (e) {
console.log(arguments[0]);
}
}, g.warn = function() {
if ("object" == ("undefined" == typeof console ? "undefined" : i(console)) && console.log && g.enabled) try {
return console.warn.apply(console, arguments);
} catch (e) {
console.warn(arguments[0]);
}
}, function() {
if (!tt.getStorageSync("uuid")) {
var e = o.UUIDv4();
tt.setStorageSync("uuid", e);
}
}();
var p = function() {
function e(t, o, n) {
a(this, e);
this.appid = t, this.base_url = o, this.coin_rate = n, this.uin = "", this.open_id = "", 
this.tt_appid = "", this.token = "";
}
n(e, [ {
key: "init",
value: function() {
this.checkUpdate();
}
}, {
key: "checkUpdate",
value: function() {
var e = tt.getUpdateManager();
e.onUpdateReady(function(t) {
tt.showModal({
title: "更新提示",
content: "新版本已经准备好，是否重启小程序？",
success: function(t) {
t.confirm && e.applyUpdate();
}
});
}), e.onUpdateFailed(function(e) {
tt.showToast({
title: "新版本下载失败，请稍后再试",
icon: "none"
});
});
}
}, {
key: "login",
value: function() {
var e = this, t = tt.getStorageSync("uuid");
return new Promise(function(o, n) {
tt.login({
success: function(i) {
e.getServiceInfo(e.appid, i.code, t).then(function(t) {
e.open_id = t.data.open_id || "", e.uin = t.data.uin || "", e.tt_appid = t.data.tt_appid || "", 
e.token = t.data.token || "", o(t.data);
}).catch(function(e) {
n(e);
});
}
});
});
}
}, {
key: "pay",
value: function(e, t, o, n) {
var i = this, a = o * i.coin_rate, c = tt.getSystemInfoSync();
console.log(c), this.createOrder(e, i.appid, i.uin, t, o, n).then(function(e) {
0 == e.data.ret ? "android" == c.platform ? tt.requestGamePayment({
mode: "game",
env: 0,
currencyType: "CNY",
platform: "android",
buyQuantity: a,
zoneId: "1",
customId: e.data.orderid,
extraInfo: i.appid,
success: function(e) {
console.log("支付成功", e);
},
fail: function(e) {
console.log("支付失败", e);
}
}) : (c.platform, tt.openAwemeCustomerService({
currencyType: "CNY",
buyQuantity: a,
zoneId: "1",
customId: e.data.orderid,
success: function(e) {
console.log("支付成功", e);
},
fail: function(e) {
console.log("支付失败", e);
}
})) : tt.showToast({
title: e.info,
icon: "none"
});
});
}
}, {
key: "getServiceInfo",
value: function(e, t, o) {
return f({
url: this.base_url + "/login/login_dymg.php",
method: "GET",
data: {
appid: e || "",
code: t || "",
uuid: o || ""
}
});
}
}, {
key: "createOrder",
value: function(e, t, o, n, i, a) {
return f({
url: this.base_url + "/charge/create_sdk_order.php",
method: "GET",
data: {
productid: e,
appid: t,
uin: o,
app_order_id: n,
price: i,
product_name: a,
from: "dy"
}
});
}
} ]);
return e;
}();
return t;
}();
});
var c = e("dm_tt_mg.min");
window.DM_TTSDK = c;
}
cc._RF.pop();
}, {
"dm_tt_mg.min": "dm_tt_mg.min"
} ],
"dm_wxmg.min": [ function(e, t, o) {
"use strict";
cc._RF.push(t, "7fa25nRXhBKo6xBZyW2E+8C", "dm_wxmg.min");
var n = function() {
function e(e, t) {
for (var o = 0; o < t.length; o++) {
var n = t[o];
n.enumerable = n.enumerable || !1;
n.configurable = !0;
"value" in n && (n.writable = !0);
Object.defineProperty(e, n.key, n);
}
}
return function(t, o, n) {
o && e(t.prototype, o);
n && e(t, n);
return t;
};
}(), i = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function(e) {
return typeof e;
} : function(e) {
return e && "function" == typeof Symbol && e.constructor === Symbol && e !== Symbol.prototype ? "symbol" : typeof e;
};
function a(e, t) {
if (!(e instanceof t)) throw new TypeError("Cannot call a class as a function");
}
!function(e, n) {
"object" == ("undefined" == typeof o ? "undefined" : i(o)) && "object" == ("undefined" == typeof t ? "undefined" : i(t)) ? t.exports = n() : "function" == typeof define && define.amd ? define([], n) : "object" == ("undefined" == typeof o ? "undefined" : i(o)) ? o.DM_WXMG = n() : e.DM_WXMG = n();
}(window, function() {
return function() {
var e = {
d: function(t, o) {
for (var n in o) e.o(o, n) && !e.o(t, n) && Object.defineProperty(t, n, {
enumerable: !0,
get: o[n]
});
},
o: function(e, t) {
return Object.prototype.hasOwnProperty.call(e, t);
},
r: function(e) {
"undefined" != typeof Symbol && Symbol.toStringTag && Object.defineProperty(e, Symbol.toStringTag, {
value: "Module"
}), Object.defineProperty(e, "__esModule", {
value: !0
});
}
}, t = {};
e.r(t), e.d(t, {
default: function() {
return c;
}
});
var o = "https://h5apiv2.hnlyplay.cn", i = new (function() {
function e() {
a(this, e);
}
n(e, [ {
key: "getStorage",
value: function(e) {
return wx.getStorageSync(e);
}
}, {
key: "setStorage",
value: function(e, t) {
wx.setStorage({
key: e,
data: t
});
}
}, {
key: "setStorageSync",
value: function(e, t) {
wx.setStorageSync(e, t);
}
}, {
key: "getLaunchOptionsSync",
value: function() {
return wx.getLaunchOptionsSync();
}
}, {
key: "request",
value: function(e) {
return new Promise(function(t, o) {
wx.request({
url: e.url,
data: e.data,
method: e.method,
success: function(e) {
200 == e.statusCode ? t(e) : o(e.data);
}
});
});
}
}, {
key: "login",
value: function(e) {
return wx.login(e);
}
}, {
key: "pay",
value: function(e) {
return wx.requestPayment(e);
}
}, {
key: "requestPayment",
value: function(e) {
return wx.requestPayment(e);
}
}, {
key: "showToast",
value: function(e) {
return wx.showToast(e);
}
}, {
key: "showModal",
value: function(e) {
return wx.showModal(e);
}
}, {
key: "navigateToMiniProgram",
value: function(e) {
return wx.navigateToMiniProgram(e);
}
}, {
key: "getSystemInfoSync",
value: function(e) {
return wx.getSystemInfoSync(e);
}
}, {
key: "getUpdateManager",
value: function() {
return wx.getUpdateManager();
}
}, {
key: "canIUse",
value: function(e) {
return wx.canIUse(e);
}
}, {
key: "requestMidasPayment",
value: function(e) {
return wx.requestMidasPayment(e);
}
}, {
key: "openCustomerServiceConversation",
value: function(e) {
return wx.openCustomerServiceConversation(e);
}
}, {
key: "onTouchEnd",
value: function(e) {
return wx.onTouchEnd(e);
}
}, {
key: "showLoading",
value: function(e) {
return wx.showLoading(e);
}
}, {
key: "hideLoading",
value: function() {
return wx.hideLoading();
}
} ]);
return e;
}())(), c = function() {
function e(t) {
a(this, e);
this.appid = t, this.uin = "", this.open_id = "", this.wx_appid = "", this.token = "";
}
n(e, [ {
key: "init",
value: function() {
if (this.getUpdate(), !i.getStorage("isFromAd")) {
var e = i.getLaunchOptionsSync();
"{}" === JSON.stringify(e.query) ? i.setStorageSync("isFromAd", 0) : i.setStorageSync("isFromAd", 1);
}
}
}, {
key: "login",
value: function() {
var e = this, t = i.getStorage("uuid") || "", o = i.getLaunchOptionsSync();
return new Promise(function(n, a) {
i.login({
success: function(c) {
e.getServiceInfo(e.appid, c.code, t).then(function(t) {
e.open_id = t.data.open_id || "", e.uin = t.data.uin || "", e.wx_appid = t.data.wx_appid || "", 
e.token = t.data.token || "", 1 == t.data.is_skip_game && 1 == i.getStorage("isFromAd") ? t.data.isFromAd = 1 : t.data.isFromAd = 0, 
t.data.extraData = JSON.stringify(o.query);
for (var a = Object.keys(t.data), c = 0; c < a.length; c++) t.data[a[c]] = t.data[a[c]] + "";
console.log("------sdk-----", t.data), n(JSON.stringify(t.data));
}).catch(function(e) {
a(e);
});
}
});
});
}
}, {
key: "pay",
value: function(e, t, n, a, c, r, s, l) {
var d = this, u = i.getSystemInfoSync();
this.getPayWay(e, t, c, u.platform).then(function(e) {
if (0 == e.data.ret) {
var t = e.data.wx_appid;
e.data.offer_id ? d.chargeCallback(1, 1, d.uin, c, r, n, d.appid).then(function(t) {
0 == t.data.ret && (t.data.info >= c ? d.chargeCallback(1, 2, d.uin, c, r, n, d.appid) : "windows" == u.platform || "devtools" == u.platform ? i.requestMidasPayment({
mode: "game",
env: e.data.mds_env,
offerId: e.data.offer_id,
currencyType: "CNY",
platform: "windows",
buyQuantity: c,
zoneId: 1,
success: function(e) {
d.chargeCallback(1, 2, d.uin, c, r, n, d.appid);
},
fail: function(e) {
console.log(e);
},
complete: function(e) {
console.log(e);
}
}) : i.requestMidasPayment({
mode: "game",
env: e.data.mds_env,
offerId: e.data.offer_id,
currencyType: "CNY",
platform: "android",
buyQuantity: c,
zoneId: 1,
success: function(e) {
d.chargeCallback(1, 2, d.uin, c, r, n, d.appid);
},
fail: function(e) {
console.log(e);
},
complete: function(e) {
console.log(e);
}
}));
}) : e.data.wx_appid ? i.navigateToMiniProgram({
appId: t,
path: "pages/pay/pay?platformId=" + n + "&appid=" + d.appid + "&uin=" + d.uin + "&orderId=" + r + "&price=" + Number(c) + "&goodName=" + a + "&roleName=" + s + "&serverName=" + l + "&wx_appid=" + t,
envVersion: "release",
success: function(e) {
console.log("成功跳转到小程序：", e);
},
fail: function(e) {
console.log(e);
}
}) : i.request({
url: o + "/charge/create_sdk_order.php",
method: "GET",
data: {
productid: n,
appid: d.appid,
uin: d.uin,
app_order_id: r,
price: c,
product_name: a
}
}).then(function(t) {
i.openCustomerServiceConversation({
showMessageCard: !0,
sendMessageTitle: e.data.card_title || "点我充值",
sendMessageImg: e.data.image_card_url || o + "/help/images/wxkf_charge_tip.png",
sendMessagePath: "index.html?platformId=${good_id}&appid=${that.appid}&uin=${that.uin}&orderId=${order_id}&price=${Number(price)}&goodName=${good_name}&roleName=${role_name}&serverName=${server_name}&wx_appid=${wx_appid}",
success: function(e) {
console.log(e);
},
fail: function(e) {
console.log(e);
}
});
});
} else i.showToast({
icon: "none",
title: "暂不支持支付！"
});
});
}
}, {
key: "getServiceInfo",
value: function(e, t, n) {
return i.request({
url: o + "/login/login_wxmg.php",
method: "GET",
data: {
appid: e || "",
js_code: t || "",
uuid: n || ""
}
});
}
}, {
key: "getPayWay",
value: function(e, t, n, a) {
return i.request({
url: o + "/charge/check_can_yap_type.php",
method: "GET",
data: {
appid: this.appid || "",
uin: this.uin || "",
channel: e || "",
worldid: t || "",
price: n || 0,
os: a
}
});
}
}, {
key: "rechargeInMg",
value: function(e) {
return i.requestMidasPayment(e);
}
}, {
key: "getUpdate",
value: function() {
var e = this, t = i.getUpdateManager();
t.onCheckForUpdate(function(o) {
o.hasUpdate && t.onUpdateReady(function() {
i.showModal({
title: "更新提示",
content: "新版本已经准备好，是否重启应用？",
success: function(o) {
o.confirm ? e.downLoadAndUpdate(t) : o.cancel && i.showModal({
title: "温馨提示~",
content: "本次版本更新涉及到新的功能添加，旧版本无法正常访问的哦~",
showCancel: !1,
confirmText: "确定更新",
success: function(o) {
o.confirm && e.downLoadAndUpdate(t);
}
});
}
});
});
});
}
}, {
key: "chargeCallback",
value: function(e, t, n, a, c, r, s) {
return new Promise(function(l, d) {
i.login({
success: function(u) {
i.request({
url: o + "/charge/mds/wx_mds_charge.php",
method: "get",
data: {
zone_id: e,
req_type: t,
uin: n,
price: a,
app_order_id: c,
product_id: r,
appid: s,
js_code: u.code
}
}).then(function(e) {
l(e);
}).catch(function(e) {
d(e);
});
}
});
});
}
}, {
key: "sendCode",
value: function(e) {
i.request({
url: o + "/login/login_bind_mobile_send_sms.php?mobile=" + e + "&appid=" + this.appid,
method: "get"
}).then(function(e) {
0 == e.data.ret ? i.showToast({
icon: "none",
title: "验证码发送成功!"
}) : i.showToast({
icon: "none",
title: e.data.info
});
});
}
}, {
key: "bindPhone",
value: function(e, t, n, a, c, r) {
var s = this;
return new Promise(function(l, d) {
i.request({
url: o + "/login/login_bind_mobile_h5.php?mobile=" + e + "&verify_code=" + t + "&vfy_token=" + encodeURIComponent(s.token) + "&worldid=" + n + "&roleid=" + a + "&notify_url=" + c + "&game_uin=" + r + "&appid=" + s.appid
}).then(function(e) {
l(JSON.stringify(e.data));
}).catch(function(e) {
d(e);
});
});
}
} ]);
return e;
}();
return t;
}();
});
var c = e("dm_wxmg.min");
window.DM_WXSDK = c;
cc._RF.pop();
}, {
"dm_wxmg.min": "dm_wxmg.min"
} ]
}, {}, [ "3DCamera", "Entry", "MyLog", "alertPanel", "dm_tt_mg.min", "dm_wxmg.min" ]);