window.__require = function t(e, i, n) {
function a(r, o) {
if (!i[r]) {
if (!e[r]) {
var c = r.split("/");
c = c[c.length - 1];
if (!e[c]) {
var l = "function" == typeof __require && __require;
if (!o && l) return l(c, !0);
if (s) return s(c, !0);
throw new Error("Cannot find module '" + r + "'");
}
r = c;
}
var u = i[r] = {
exports: {}
};
e[r][0].call(u.exports, function(t) {
return a(e[r][1][t] || t);
}, u, u.exports, t, e, i, n);
}
return i[r].exports;
}
for (var s = "function" == typeof __require && __require, r = 0; r < n.length; r++) a(n[r]);
return a;
}({
BgAdapter: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "5d8e6iyDmBJAZeaAcXVITn6", "BgAdapter");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = cc._decorator, a = n.ccclass, s = n.menu, r = function(t) {
__extends(e, t);
function e() {
return null !== t && t.apply(this, arguments) || this;
}
i = e;
e.prototype.onLoad = function() {
this.node.scale = i.getScale();
};
e.getScale = function() {
var t = cc.view.getVisibleSize().height, e = cc.view.getDesignResolutionSize().height;
return t > e ? t / e : 1;
};
var i;
return i = __decorate([ a, s("公共组件/BgAdapter") ], e);
}(cc.Component);
i.default = r;
cc._RF.pop();
}, {} ],
FitUI: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "38371B+oxVBfJMYEJ6meXFx", "FitUI");
var n, a = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), s = this && this.__decorate || function(t, e, i, n) {
var a, s = arguments.length, r = s < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) r = Reflect.decorate(t, e, i, n); else for (var o = t.length - 1; o >= 0; o--) (a = t[o]) && (r = (s < 3 ? a(r) : s > 3 ? a(e, i, r) : a(e, i)) || r);
return s > 3 && r && Object.defineProperty(e, i, r), r;
};
Object.defineProperty(i, "__esModule", {
value: !0
});
i.FitUIOrientation = i.FitUIPlatform = void 0;
var r, o, c = cc._decorator, l = c.ccclass, u = c.property, h = c.executeInEditMode, p = c.menu, d = c.requireComponent;
(function(t) {
t[t.ANDROID = 0] = "ANDROID";
t[t.IOS = 1] = "IOS";
t[t.OTHER = 2] = "OTHER";
})(r = i.FitUIPlatform || (i.FitUIPlatform = {}));
(function(t) {
t[t.HORIZONTAL = 0] = "HORIZONTAL";
t[t.VERTICAL = 1] = "VERTICAL";
})(o = i.FitUIOrientation || (i.FitUIOrientation = {}));
var f = function() {
return this.orientation == o.HORIZONTAL;
}, g = function() {
return this.orientation == o.VERTICAL;
}, _ = function(t) {
a(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.orientation = o.VERTICAL;
e.left = !1;
e.right = !1;
e.top = !1;
e.bottom = !1;
e.iLeft = !1;
e.iRight = !1;
e.iTop = !1;
e.iBottom = !1;
e.oLeft = !1;
e.oRight = !1;
e.oTop = !1;
e.oBottom = !1;
e.deltaNodes = [];
return e;
}
Object.defineProperty(e.prototype, "a", {
get: function() {
return r.ANDROID;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "i", {
get: function() {
return r.IOS;
},
enumerable: !1,
configurable: !0
});
Object.defineProperty(e.prototype, "o", {
get: function() {
return r.OTHER;
},
enumerable: !1,
configurable: !0
});
e.prototype.onLoad = function() {
this.applySettings();
};
e.prototype.applySettings = function() {
var t, e, i, n = cc.view.getVisibleSize(), a = cc.sys.platform, s = n.width / n.height;
if ((s = this.orientation == o.VERTICAL ? s : 1 / s) >= .5) this.updateFitStrategy(!0, !0); else {
this.orientation == o.VERTICAL && this.updateFitStrategy(!0, !1);
this.orientation == o.HORIZONTAL && this.updateFitStrategy(!1, !0);
var r = {
top: this.top,
left: this.left,
right: this.right,
bottom: this.bottom
}, c = {
top: this.iTop,
left: this.iLeft,
right: this.iRight,
bottom: this.iBottom
};
this.oTop, this.oLeft, this.oRight, this.oBottom;
switch (a) {
case cc.sys.ANDROID:
this.fitPhone(r);
break;

case cc.sys.IPHONE:
this.fitPhone(c);
}
null === (i = null === (e = null === (t = this.getComponent(cc.ScrollView)) || void 0 === t ? void 0 : t.content) || void 0 === e ? void 0 : e.parent.getComponent(cc.Widget)) || void 0 === i || i.updateAlignment();
}
};
e.prototype.fitPhone = function(t) {
var e = cc.view.getVisibleSize(), i = this.orientation == o.VERTICAL ? e.height : e.width, n = 44 * i / 812, a = 34 * i / 812;
switch (this.orientation) {
case o.VERTICAL:
t.top && this.updateWidget("top", n);
t.bottom && this.updateWidget("bottom", a);
break;

case o.HORIZONTAL:
t.left && this.updateWidget("left", a);
t.right && this.updateWidget("right", n);
}
};
e.prototype.updateWidget = function(t, e) {
var i = this.getComponent(cc.Widget);
if (i) {
i["default_" + t] || (i["default_" + t] = i["" + t]);
i["" + t] = i["default_" + t] + e;
}
e > 0 && this.deltaNodes.forEach(function(t) {
t && (t.y -= e);
});
};
e.prototype.updateFitStrategy = function(t, e) {
var i = this.node.getComponent(cc.Canvas);
if (i) {
i.fitWidth = t;
i.fitHeight = e;
i.applySettings();
}
};
s([ u({
displayName: "适配方向",
type: cc.Enum(o)
}) ], e.prototype, "orientation", void 0);
s([ u({
displayName: "安卓设备",
type: cc.Enum(r)
}) ], e.prototype, "a", null);
s([ u({
displayName: "left",
visible: f
}) ], e.prototype, "left", void 0);
s([ u({
displayName: "right",
visible: f
}) ], e.prototype, "right", void 0);
s([ u({
displayName: "top",
visible: g
}) ], e.prototype, "top", void 0);
s([ u({
displayName: "bottom",
visible: g
}) ], e.prototype, "bottom", void 0);
s([ u({
displayName: "苹果设备",
type: cc.Enum(r)
}) ], e.prototype, "i", null);
s([ u({
displayName: "left",
visible: f
}) ], e.prototype, "iLeft", void 0);
s([ u({
displayName: "right",
visible: f
}) ], e.prototype, "iRight", void 0);
s([ u({
displayName: "top",
visible: g
}) ], e.prototype, "iTop", void 0);
s([ u({
displayName: "bottom",
visible: g
}) ], e.prototype, "iBottom", void 0);
s([ u({
displayName: "其它设备",
type: cc.Enum(r)
}) ], e.prototype, "o", null);
s([ u({
displayName: "left",
visible: f
}) ], e.prototype, "oLeft", void 0);
s([ u({
displayName: "right",
visible: f
}) ], e.prototype, "oRight", void 0);
s([ u({
displayName: "top",
visible: g
}) ], e.prototype, "oTop", void 0);
s([ u({
displayName: "bottom",
visible: g
}) ], e.prototype, "oBottom", void 0);
s([ u({
type: [ cc.Node ],
visible: function() {
return this.iTop;
},
tooltip: "节点Y轴下移刘海高度"
}) ], e.prototype, "deltaNodes", void 0);
return s([ l, p("公用组件/FitUI"), h, d(cc.Widget) ], e);
}(cc.Component);
i.default = _;
cc._RF.pop();
}, {} ],
HelloSandglass: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "631a6o6HJFHD77Iu9xMbLSs", "HelloSandglass");
var n, a = this && this.__extends || (n = function(t, e) {
return (n = Object.setPrototypeOf || {
__proto__: []
} instanceof Array && function(t, e) {
t.__proto__ = e;
} || function(t, e) {
for (var i in e) Object.prototype.hasOwnProperty.call(e, i) && (t[i] = e[i]);
})(t, e);
}, function(t, e) {
n(t, e);
function i() {
this.constructor = t;
}
t.prototype = null === e ? Object.create(e) : (i.prototype = e.prototype, new i());
}), s = this && this.__decorate || function(t, e, i, n) {
var a, s = arguments.length, r = s < 3 ? e : null === n ? n = Object.getOwnPropertyDescriptor(e, i) : n;
if ("object" == typeof Reflect && "function" == typeof Reflect.decorate) r = Reflect.decorate(t, e, i, n); else for (var o = t.length - 1; o >= 0; o--) (a = t[o]) && (r = (s < 3 ? a(r) : s > 3 ? a(e, i, r) : a(e, i)) || r);
return s > 3 && r && Object.defineProperty(e, i, r), r;
}, r = this && this.__awaiter || function(t, e, i, n) {
return new (i || (i = Promise))(function(a, s) {
function r(t) {
try {
c(n.next(t));
} catch (t) {
s(t);
}
}
function o(t) {
try {
c(n.throw(t));
} catch (t) {
s(t);
}
}
function c(t) {
t.done ? a(t.value) : (e = t.value, e instanceof i ? e : new i(function(t) {
t(e);
})).then(r, o);
var e;
}
c((n = n.apply(t, e || [])).next());
});
}, o = this && this.__generator || function(t, e) {
var i, n, a, s, r = {
label: 0,
sent: function() {
if (1 & a[0]) throw a[1];
return a[1];
},
trys: [],
ops: []
};
return s = {
next: o(0),
throw: o(1),
return: o(2)
}, "function" == typeof Symbol && (s[Symbol.iterator] = function() {
return this;
}), s;
function o(t) {
return function(e) {
return c([ t, e ]);
};
}
function c(s) {
if (i) throw new TypeError("Generator is already executing.");
for (;r; ) try {
if (i = 1, n && (a = 2 & s[0] ? n.return : s[0] ? n.throw || ((a = n.return) && a.call(n), 
0) : n.next) && !(a = a.call(n, s[1])).done) return a;
(n = 0, a) && (s = [ 2 & s[0], a.value ]);
switch (s[0]) {
case 0:
case 1:
a = s;
break;

case 4:
r.label++;
return {
value: s[1],
done: !1
};

case 5:
r.label++;
n = s[1];
s = [ 0 ];
continue;

case 7:
s = r.ops.pop();
r.trys.pop();
continue;

default:
if (!(a = r.trys, a = a.length > 0 && a[a.length - 1]) && (6 === s[0] || 2 === s[0])) {
r = 0;
continue;
}
if (3 === s[0] && (!a || s[1] > a[0] && s[1] < a[3])) {
r.label = s[1];
break;
}
if (6 === s[0] && r.label < a[1]) {
r.label = a[1];
a = s;
break;
}
if (a && r.label < a[2]) {
r.label = a[2];
r.ops.push(s);
break;
}
a[2] && r.ops.pop();
r.trys.pop();
continue;
}
s = e.call(t, r);
} catch (t) {
s = [ 6, t ];
n = 0;
} finally {
i = a = 0;
}
if (5 & s[0]) throw s[1];
return {
value: s[0] ? s[1] : void 0,
done: !0
};
}
};
Object.defineProperty(i, "__esModule", {
value: !0
});
var c = t("./language/StartWords"), l = t("./ThinkingAnalyticsController"), u = cc._decorator, h = u.ccclass, p = u.property, d = window.wx || window.tt;
cc.macro.CLEANUP_IMAGE_CACHE = !1;
cc.dynamicAtlasManager.enabled = !0;
cc.dynamicAtlasManager.maxAtlasCount = 4;
cc.dynamicAtlasManager.maxFrameSize = 410;
cc.dynamicAtlasManager.textureSize = 2048;
cc.macro.ENABLE_NATIVE_TTF_RENDERER = !1;
if (null == cc.dynamicAtlasManager.__insertSpriteFrame) {
cc.dynamicAtlasManager.__insertSpriteFrame = cc.dynamicAtlasManager.insertSpriteFrame;
cc.dynamicAtlasManager.__reset = cc.dynamicAtlasManager.reset;
cc.dynamicAtlasManager.insertSpriteFrame = function(t) {
var e = cc.dynamicAtlasManager.__insertSpriteFrame(t);
e && cc.dynamicAtlasManager.holdTextureSet.add(t._texture._uuid);
return e;
};
cc.dynamicAtlasManager.reset = function() {
cc.dynamicAtlasManager.holdTextureSet = new Set();
cc.dynamicAtlasManager.__reset();
};
}
cc.dynamicAtlasManager.holdTextureSet = new Set();
cc.SL_CONFIG_LABEL_LAZY_INIT = !0;
cc.SL_CONFIG_LABEL_LAZY_INIT_ONCE = !0;
var f = function(t) {
a(e, t);
function e() {
var e = null !== t && t.apply(this, arguments) || this;
e.node_loading = null;
e.progressBar = null;
e.txt_loading = null;
e.loadingProgress = 0;
e.loadingDuration = 12;
e.allLoadingComplete = !1;
return e;
}
i = e;
e.prototype.onLoad = function() {
cc.assetManager.downloader.maxRequestsPerFrame = 128;
l.ThinkingAnalyticsController.ins.init();
l.ThinkingAnalyticsController.ins.launchEvent(l.LaunchEvent.Loading, {
type: "start"
});
if (cc.sys.platform == cc.sys.WECHAT_GAME) {
this.checkUpdateVersion(function() {});
this.onCompleteOneEvent();
d.showShareMenu({
menus: [ "shareAppMessage", "shareTimeline" ],
withShareTicket: !0
});
var t = cc.sys.os == cc.sys.OS_IOS ? 40 : 60;
d.setPreferredFramesPerSecond(t);
cc.game.setFrameRate(t);
d.setKeepScreenOn({
keepScreenOn: !0
});
} else this.onCompleteOneEvent();
this.node.on("size-changed", this.resetFitMode, this);
this.resetFitMode();
};
e.prototype.resetFitMode = function() {
var t = cc.view.getDesignResolutionSize();
cc.winSize.height / cc.winSize.width < 1.77 && cc.view.setDesignResolutionSize(t.width, t.height, cc.ResolutionPolicy.SHOW_ALL);
};
e.prototype.onCompleteOneEvent = function() {
this.node_loading.active = !0;
this.loadRes();
cc.sys.platform, cc.sys.WECHAT_GAME;
};
e.prototype.setLoadingText = function(t) {
this.txt_loading.string = t;
};
e.prototype.setLoadingProgress = function(t) {
this.progressBar.getComponent(cc.Sprite).fillRange = t;
};
e.prototype.loadRes = function() {
return r(this, void 0, void 0, function() {
return o(this, function(t) {
switch (t.label) {
case 0:
this.scheduleOnce(function() {
cc.sys.platform == cc.sys.WECHAT_GAME && d.showModal({
title: "提示",
content: "您的网络不稳定，请检查网络连接。",
confirmText: "重启游戏",
success: function(t) {
if (t.confirm) {
console.log("用户点击确定");
d.restartMiniProgram && d.restartMiniProgram();
} else t.cancel && console.log("用户点击取消");
}
});
}, 30);
return [ 4, this.loadSubPackage("ui", c.StartWords[1], .2) ];

case 1:
t.sent();
return [ 4, Promise.all([ this.loadSubPackage("textures", c.StartWords[1], .3), this.loadSubPackage("spine", c.StartWords[1], .5), this.loadSubPackage("scripts", c.StartWords[1], .65), this.loadSubPackage("configs", c.StartWords[1], .85), this.loadSubPackage("prefab", c.StartWords[1], .95) ]) ];

case 2:
t.sent();
return [ 4, this.loadScene(c.StartWords[2], 1) ];

case 3:
t.sent();
this.loadingDuration = .5;
this.allLoadingComplete = !0;
l.ThinkingAnalyticsController.ins.launchEvent(l.LaunchEvent.LoadComplete, {});
return [ 2 ];
}
});
});
};
e.prototype.loadSubPackage = function(t, e, i) {
void 0 === i && (i = .2);
l.ThinkingAnalyticsController.ins.launchEvent(l.LaunchEvent.Loading, {
type: t
});
this.setLoadingText(e);
return new Promise(function(e) {
cc.assetManager.loadBundle(t, function(i) {
if (i) {
l.ThinkingAnalyticsController.ins.launchEvent(l.LaunchEvent.Loading, {
type: t + "_error"
});
console.error("loadSubPackage" + t, i);
} else {
l.ThinkingAnalyticsController.ins.launchEvent(l.LaunchEvent.Loading, {
type: t + "_end"
});
e(null);
}
});
});
};
e.prototype.loadScene = function(t) {
var e = this;
l.ThinkingAnalyticsController.ins.launchEvent(l.LaunchEvent.Loading, {
type: "loadScene"
});
return new Promise(function(i) {
e.setLoadingText(t);
cc.director.preloadScene("GameScene", function() {
cc.assetManager.getBundle("prefab").load("login/LoginWin", function() {
i(null);
});
});
});
};
e.prototype.checkUpdateVersion = function(t) {
var e = this;
if (d.getUpdateManager) {
if (cc.sys.os == cc.sys.OS_WINDOWS) {
t && t();
return;
}
var i = d.getUpdateManager();
console.log("检查版本-----------");
if (!i.onCheckForUpdate) {
t && t();
return;
}
i.onCheckForUpdate(function(n) {
console.log("是否获取版本", n);
e.unscheduleAllCallbacks();
if (n.hasUpdate) {
i.onUpdateReady(function() {
d.showModal({
title: "更新提示",
content: "新版本已经下载好，请重启应用。",
success: function(e) {
e.confirm ? i.applyUpdate() : t && t();
}
});
});
i.onUpdateFailed(function() {
d.showModal({
title: "已经有新版本喽~",
content: "请您删除当前小程序，到微信 “发现-小程序” 页，重新搜索打开哦~",
success: function() {
t && t();
}
});
});
} else t && t();
});
} else {
console.log("检查版本2-----------");
d.showModal({
title: "溫馨提示",
content: "当前微信版本过低，无法使用该功能，请升级到最新微信版本后重试。"
});
t && t();
}
};
e.prototype.cc_onClickRepair = function() {
i.clickRepair();
};
e.prototype.update = function(t) {
this.loadingProgress += t / this.loadingDuration;
0 == this.allLoadingComplete && (this.loadingProgress = Math.min(.9, this.loadingProgress));
this.setLoadingProgress(this.loadingProgress);
this.loadingProgress >= 1 && this.gotoEntry();
};
e.prototype.gotoEntry = function() {
0 != this.allLoadingComplete && cc.director.loadScene("GameScene");
};
e.clickRepair = function() {
var t;
if (cc.sys.platform != cc.sys.WECHAT_GAME) if (cc.sys.isNative) ; else {
null === (t = cc.assetManager.cacheManager) || void 0 === t || t.clearCache();
setTimeout(function() {
cc.game.restart();
}, 100);
} else d.showModal({
title: c.StartWords[3],
content: c.StartWords[4],
success: function(t) {
var e;
if (t.confirm) {
console.log("用户点击确定");
null === (e = cc.assetManager.cacheManager) || void 0 === e || e.clearCache();
d.restartMiniProgram && d.restartMiniProgram();
} else t.cancel && console.log("用户点击取消");
}
});
};
var i;
s([ p(cc.Node) ], e.prototype, "node_loading", void 0);
s([ p(cc.Node) ], e.prototype, "progressBar", void 0);
s([ p(cc.Label) ], e.prototype, "txt_loading", void 0);
return i = s([ h ], e);
}(cc.Component);
i.default = f;
cc._RF.pop();
}, {
"./ThinkingAnalyticsController": "ThinkingAnalyticsController",
"./language/StartWords": "StartWords"
} ],
LanguageConf: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "923e5rNTctKzKZVKCXa4rNV", "LanguageConf");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.LanguageConf = void 0;
i.LanguageConf = {
allLG: [ "zh_CN" ],
useLG: "zh_CN"
};
cc._RF.pop();
}, {} ],
LanguageMgr: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "fbdbcGqevZEh4Zk/FM43FvH", "LanguageMgr");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.LanguageMgr = i.LanguageName = void 0;
var n, a = t("./LanguageConf");
(function(t) {
t.zh_CN = "zh_CN";
t.zh_TW = "zh_TW";
t.en_US = "en_US";
t.th_TH = "th_TH";
t.vi_VN = "vi_VN";
})(n = i.LanguageName || (i.LanguageName = {}));
var s = function() {
function t() {
this.curLanguage = n.zh_CN;
this.map2Url = null;
this.initLanguage();
console.warn("当前语言：", this.curLanguage);
}
t.getInstance = function() {
this.ins || (this.ins = new t());
return this.ins;
};
t.prototype.initLanguage = function() {
if (this.isSupportSwitchLG()) {
this.map2Url = new Map();
for (var t in a.LanguageConf) t && a.LanguageConf[t] && this.map2Url.set(a.LanguageConf[t].url, a.LanguageConf[t]);
var e = cc.sys.localStorage.getItem("language");
e && a.LanguageConf.allLG.includes(e) ? this.curLanguage = e : this.curLanguage = a.LanguageConf.useLG;
} else this.curLanguage = n.zh_CN;
};
t.prototype.isSupportSwitchLG = function() {
return a.LanguageConf.allLG.length > 1;
};
t.prototype.getMobileLanguage = function() {
var t = n.zh_CN;
if (this.isSupportSwitchLG()) {
var e = cc.sys.languageCode;
t = e.indexOf("zh_") >= 0 || e.indexOf("zh-") >= 0 ? e.indexOf("tw") >= 0 ? n.zh_TW : n.zh_CN : n.zh_TW;
}
return t;
};
t.prototype.getCurrentLanguage = function() {
return this.curLanguage;
};
t.prototype.changeLanguage = function(t) {
if (!a.LanguageConf.allLG.includes(t)) return !1;
if (this.curLanguage == t) return !1;
this.curLanguage = t;
cc.sys.localStorage.setItem("language", t);
return !0;
};
t.prototype.getLanguageImageUUID = function(t) {
if (!this.isSupportSwitchLG()) return t;
if (this.getCurrentLanguage() == n.zh_CN) return t;
var e = a.LanguageConf[t];
return null == e ? t : e[this.getCurrentLanguage()] || t;
};
t.prototype.getLanguageImageUrl = function(t) {
if (!this.isSupportSwitchLG()) return t;
var e = this.map2Url.get(t);
return null == e ? t : e[this.getCurrentLanguage()] ? this.getCurrentLanguage() + "/" + t : n.zh_CN + "/" + t;
};
return t;
}();
i.LanguageMgr = s;
cc._RF.pop();
}, {
"./LanguageConf": "LanguageConf"
} ],
StartWords_cn: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "ffa18pcOD5FtrW4nHnLrk7Z", "StartWords_cn");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.StartWords_cn = void 0;
i.StartWords_cn = {
1: "正在初始化...",
2: "正在加载场景",
3: "提示",
4: "清理本地缓存，完成后会退出游戏，是否继续？",
5: "修复"
};
cc._RF.pop();
}, {} ],
StartWords: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "46f5d4bRmVMR4HtILykFq+y", "StartWords");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.StartWords = void 0;
var n = t("./StartWords_cn");
i.StartWords = n.StartWords_cn;
cc._RF.pop();
}, {
"./StartWords_cn": "StartWords_cn"
} ],
ThinkingAnalyticsController: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "58a53wvZsRIBKhN7fHpUcm+", "ThinkingAnalyticsController");
Object.defineProperty(i, "__esModule", {
value: !0
});
i.ThinkingAnalyticsController = i.LaunchEvent = void 0;
var n;
(function(t) {
t.Init = "launch_init";
t.FirstScene = "launch_first_scene";
t.Loading = "launch_loading";
t.LoadComplete = "launch_load_complete";
t.LoginScene = "launch_login_scene";
t.SDKInit = "launch_sdk_init";
t.SDKLogin = "launch_sdk_login";
t.RequestGateWay = "launch_request_gateway";
t.RequestGateWayComplete = "launch_request_gateway_complete";
t.RequestServerList = "launch_request_server_list";
t.RequestServerListComplete = "launch_request_server_list_complete";
t.RequestWhiteList = "launch_request_white_list";
t.RequestWhiteListComplete = "launch_request_white_list_complete";
t.ShowNotice = "launch_show_notice";
t.CloseNotice = "launch_close_notice";
t.ClickEnter = "launch_click_enter";
t.Subscribe = "launch_subscribe";
t.SubscribeComplete = "launch_subscribe_complete";
t.Config = "launch_config";
t.ConfigComplete = "launch_config_complete";
t.Prefab = "launch_prefab";
t.PrefabComplete = "launch_prefab_complete";
t.Connected = "launch_connected";
t.LoginError = "launch_login_error";
t.LoginSuccess = "launch_login_success";
t.EnterGame = "launch_enter_game";
t.GuideMovie = "launch_guide_movie";
t.HomeCity = "launch_home_city";
t.OnHook = "launch_on_hook";
t.ClickAttack = "launch_click_attack";
t.TakeTask = "launch_take_task";
})(n = i.LaunchEvent || (i.LaunchEvent = {}));
var a = function() {
function t() {
this._ta = null;
}
Object.defineProperty(t, "ins", {
get: function() {
null == t._instance && (t._instance = new t());
return t._instance;
},
enumerable: !1,
configurable: !0
});
t.prototype.init = function() {
if (window.wx) {
var t = window.wx._GlobalTA;
if (t) {
this._ta = t;
return;
}
}
this._ta = TDAnalytics;
TDAnalytics.init({
appId: "64d01dd929bf4e7f9ec2b392a342789a",
serverUrl: "https://log.shalougame.com:8991",
enableLog: !1,
autoTrack: {
appShow: !0,
appHide: !0
}
});
this.launchEvent(n.Init, {});
};
t.prototype.getDistinctId = function() {
return this._ta ? this._ta.getDistinctId() : "0000";
};
t.prototype.track = function(t, e) {
if (this._ta) {
this._ta.track({
eventName: t,
properties: e || {}
});
cc.log(t, e);
}
};
t.prototype.launchEvent = function(t, e) {
this._ta && this.track(t, e);
};
t.prototype.setAccount = function(t) {
this._ta && this._ta.login(t);
};
return t;
}();
i.ThinkingAnalyticsController = a;
cc._RF.pop();
}, {} ],
Version: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "8852bGQCmdPqIvhYK4YrFk2", "Version");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = {
appVersion: "",
appPlat: "",
commitID: ""
};
n.appVersion = "1.13.08";
n.appPlat = "todo";
n.commitID = "3af3396f0";
i.default = n;
cc._RF.pop();
}, {} ],
parseImport: [ function(t, e, i) {
"use strict";
cc._RF.push(e, "4e366lmDI5AhqyWft5JDNtQ", "parseImport");
Object.defineProperty(i, "__esModule", {
value: !0
});
var n = t("../language/LanguageMgr");
cc.assetManager.parser.register("import", function(t, e, i) {
cc.assetManager.parser.parseImport(t, e, function(t, e) {
if (e && e.__depends__) for (var a = 0; a < e.__depends__.length; a++) {
var s = e.__depends__[a];
s.uuid = n.LanguageMgr.getInstance().getLanguageImageUUID(s.uuid);
}
i && i(t, e);
});
});
cc._RF.pop();
}, {
"../language/LanguageMgr": "LanguageMgr"
} ],
"spine-assember": [ function(t, e, i) {
"use strict";
cc._RF.push(e, "aa2fdh/hNxObafYqoF8nACP", "spine-assember");
i.__esModule = !0;
i.default = void 0;
var n;
function a(t, e) {
t.prototype = Object.create(e.prototype);
t.prototype.constructor = t;
s(t, e);
}
function s(t, e) {
return (s = Object.setPrototypeOf ? Object.setPrototypeOf.bind() : function(t, e) {
t.__proto__ = e;
return t;
})(t, e);
}
var r = cc.gfx.VertexFormat.XY_UV_Color, o = cc.gfx.VertexFormat.XY_UV_Two_Color, c = cc.gfx, l = cc._decorator;
l.ccclass, l.property;
function u(t) {
return (t.a << 24 >>> 0) + (t.b << 16) + (t.g << 8) + t.r;
}
var h = (0, l.executeInEditMode)(n = function(t) {
a(e, t);
function e() {
for (var e, i = arguments.length, n = new Array(i), a = 0; a < i; a++) n[a] = arguments[a];
(e = t.call.apply(t, [ this ].concat(n)) || this)._handleVal = 0;
e._quadTriangles = [ 0, 1, 2, 2, 3, 0 ];
e._slotColor = cc.color(0, 0, 255, 255);
e._boneColor = cc.color(255, 0, 0, 255);
e._originColor = cc.color(0, 255, 0, 255);
e._meshColor = cc.color(255, 255, 0, 255);
e._finalColor = null;
e._darkColor = null;
e._tempPos = null;
e._tempUv = null;
e._premultipliedAlpha = void 0;
e._multiplier = void 0;
e._slotRangeStart = void 0;
e._slotRangeEnd = void 0;
e._useTint = void 0;
e._debugSlots = void 0;
e._debugBones = void 0;
e._debugMesh = void 0;
e._nodeR = void 0;
e._nodeG = void 0;
e._nodeB = void 0;
e._nodeA = void 0;
e._finalColor32 = void 0;
e._darkColor32 = void 0;
e._vertexFormat = void 0;
e._perVertexSize = void 0;
e._perClipVertexSize = void 0;
e._vertexFloatCount = 0;
e._vertexCount = 0;
e._vertexFloatOffset = 0;
e._vertexOffset = 0;
e._indexCount = 0;
e._indexOffset = 0;
e._vfOffset = 0;
e._tempr = void 0;
e._tempg = void 0;
e._tempb = void 0;
e._inRange = void 0;
e._mustFlush = void 0;
e._x = void 0;
e._y = void 0;
e._m00 = void 0;
e._m04 = void 0;
e._m12 = void 0;
e._m01 = void 0;
e._m05 = void 0;
e._m13 = void 0;
e._r = void 0;
e._g = void 0;
e._b = void 0;
e._fr = void 0;
e._fg = void 0;
e._fb = void 0;
e._fa = void 0;
e._dr = void 0;
e._dg = void 0;
e._db = void 0;
e._da = void 0;
e._comp = void 0;
e._buffer = void 0;
e._renderer = void 0;
e._node = void 0;
e._needColor = void 0;
e._vertexEffect = void 0;
return e;
}
var i = e.prototype;
i._getSlotMaterial = function(t, e) {
var i, n;
switch (e) {
case sp.spine.BlendMode.Additive:
i = this._premultipliedAlpha ? cc.macro.ONE : cc.macro.SRC_ALPHA;
n = cc.macro.ONE;
break;

case sp.spine.BlendMode.Multiply:
i = cc.macro.DST_COLOR;
n = cc.macro.ONE_MINUS_SRC_ALPHA;
break;

case sp.spine.BlendMode.Screen:
i = cc.macro.ONE;
n = cc.macro.ONE_MINUS_SRC_COLOR;
break;

case sp.spine.BlendMode.Normal:
default:
i = this._premultipliedAlpha ? cc.macro.ONE : cc.macro.SRC_ALPHA;
n = cc.macro.ONE_MINUS_SRC_ALPHA;
}
var a = !this._comp.enableBatch, s = this._comp._materials[0];
if (!s) return null;
var r = t.getId() + i + n + this._useTint + a, o = this._comp._materialCache, l = o[r];
if (!l) {
if (o.baseMaterial) l = cc.MaterialVariant.create(s); else {
l = s;
o.baseMaterial = s;
}
l.define("CC_USE_MODEL", a);
l.define("USE_TINT", this._useTint);
l.setProperty("texture", t);
l.setBlend(!0, c.BLEND_FUNC_ADD, i, n, c.BLEND_FUNC_ADD, i, n);
o[r] = l;
}
return l;
};
i._handleColor = function(t) {
this._fa = t.fa * this._nodeA;
this._multiplier = this._premultipliedAlpha ? this._fa / 255 : 1;
this._r = this._nodeR * this._multiplier;
this._g = this._nodeG * this._multiplier;
this._b = this._nodeB * this._multiplier;
this._fr = t.fr * this._r;
this._fg = t.fg * this._g;
this._fb = t.fb * this._b;
this._finalColor32 = (this._fa << 24 >>> 0) + (this._fb << 16) + (this._fg << 8) + this._fr;
this._dr = t.dr * this._r;
this._dg = t.dg * this._g;
this._db = t.db * this._b;
this._da = this._premultipliedAlpha ? 255 : 0;
this._darkColor32 = (this._da << 24 >>> 0) + (this._db << 16) + (this._dg << 8) + this._dr;
};
i.updateRenderData = function(t) {
if (!t.isAnimationCached()) {
var e = t._skeleton;
e && e.updateWorldTransform();
}
};
i.fillVertices = function(t, e, i, n, a) {
var s, r = this._buffer._vData, o = this._buffer._iData, c = this._buffer._uintVData;
this._finalColor.a = i.a * e.a * t.a * this._nodeA * 255;
this._multiplier = this._premultipliedAlpha ? this._finalColor.a : 255;
this._tempr = this._nodeR * e.r * t.r * this._multiplier;
this._tempg = this._nodeG * e.g * t.g * this._multiplier;
this._tempb = this._nodeB * e.b * t.b * this._multiplier;
this._finalColor.r = this._tempr * i.r;
this._finalColor.g = this._tempg * i.g;
this._finalColor.b = this._tempb * i.b;
if (null == a.darkColor) this._darkColor.set(0, 0, 0, 1); else {
this._darkColor.r = a.darkColor.r * this._tempr;
this._darkColor.g = a.darkColor.g * this._tempg;
this._darkColor.b = a.darkColor.b * this._tempb;
}
this._darkColor.a = this._premultipliedAlpha ? 255 : 0;
if (n.isClipping()) {
var l = r.subarray(this._vertexFloatOffset + 2);
n.clipTriangles(r.subarray(this._vertexFloatOffset), this._vertexFloatCount, o.subarray(this._indexOffset), this._indexCount, l, this._finalColor, this._darkColor, this._useTint, this._perVertexSize);
var h = new Float32Array(n.clippedVertices), p = n.clippedTriangles;
this._indexCount = p.length;
this._vertexFloatCount = h.length / this._perClipVertexSize * this._perVertexSize;
s = this._buffer.request(this._vertexFloatCount / this._perVertexSize, this._indexCount);
this._indexOffset = s.indiceOffset, this._vertexOffset = s.vertexOffset, this._vertexFloatOffset = s.byteOffset >> 2;
r = this._buffer._vData, o = this._buffer._iData;
c = this._buffer._uintVData;
o.set(p, this._indexOffset);
if (this._vertexEffect) for (var d = 0, f = h.length, g = this._vertexFloatOffset; d < f; d += this._perClipVertexSize, 
g += this._perVertexSize) {
this._tempPos.x = h[d];
this._tempPos.y = h[d + 1];
this._finalColor.set(h[d + 2], h[d + 3], h[d + 4], h[d + 5]);
this._tempUv.x = h[d + 6];
this._tempUv.y = h[d + 7];
this._useTint ? this._darkColor.set(h[d + 8], h[d + 9], h[d + 10], h[d + 11]) : this._darkColor.set(0, 0, 0, 0);
this._vertexEffect.transform(this._tempPos, this._tempUv, this._finalColor, this._darkColor);
r[g] = this._tempPos.x;
r[g + 1] = this._tempPos.y;
r[g + 2] = this._tempUv.x;
r[g + 3] = this._tempUv.y;
c[g + 4] = u(this._finalColor);
this._useTint && (c[g + 5] = u(this._darkColor));
} else for (var _ = 0, m = h.length, v = this._vertexFloatOffset; _ < m; _ += this._perClipVertexSize, 
v += this._perVertexSize) {
r[v] = h[_];
r[v + 1] = h[_ + 1];
r[v + 2] = h[_ + 6];
r[v + 3] = h[_ + 7];
this._finalColor32 = (h[_ + 5] << 24 >>> 0) + (h[_ + 4] << 16) + (h[_ + 3] << 8) + h[_ + 2];
c[v + 4] = this._finalColor32;
if (this._useTint) {
this._darkColor32 = (h[_ + 11] << 24 >>> 0) + (h[_ + 10] << 16) + (h[_ + 9] << 8) + h[_ + 8];
c[v + 5] = this._darkColor32;
}
}
} else if (this._vertexEffect) for (var y = this._vertexFloatOffset, b = this._vertexFloatOffset + this._vertexFloatCount; y < b; y += this._perVertexSize) {
this._tempPos.x = r[y];
this._tempPos.y = r[y + 1];
this._tempUv.x = r[y + 2];
this._tempUv.y = r[y + 3];
this._vertexEffect.transform(this._tempPos, this._tempUv, this._finalColor, this._darkColor);
r[y] = this._tempPos.x;
r[y + 1] = this._tempPos.y;
r[y + 2] = this._tempUv.x;
r[y + 3] = this._tempUv.y;
c[y + 4] = u(this._finalColor);
this._useTint && (c[y + 5] = u(this._darkColor));
} else {
this._finalColor32 = u(this._finalColor);
this._darkColor32 = u(this._darkColor);
for (var k = this._vertexFloatOffset, S = this._vertexFloatOffset + this._vertexFloatCount; k < S; k += this._perVertexSize) {
c[k + 4] = this._finalColor32;
this._useTint && (c[k + 5] = this._darkColor32);
}
}
};
i.realTimeTraverse = function(t) {
var e, i, n, a, s, r, o, c, l, u, h, p, d = this._comp._skeleton, f = d.color, g = this._comp._debugRenderer, _ = this._comp._clipper, m = null;
this._slotRangeStart = this._comp._startSlotIndex;
this._slotRangeEnd = this._comp._endSlotIndex;
this._inRange = !1;
-1 == this._slotRangeStart && (this._inRange = !0);
this._debugSlots = this._comp.debugSlots;
this._debugBones = this._comp.debugBones;
this._debugMesh = this._comp.debugMesh;
if (g && (this._debugBones || this._debugSlots || this._debugMesh)) {
g.clear();
g.lineWidth = 2;
}
this._perClipVertexSize = this._useTint ? 12 : 8;
this._vertexFloatCount = 0;
this._vertexFloatOffset = 0;
this._vertexOffset = 0;
this._indexCount = 0;
this._indexOffset = 0;
var v = this._comp._followSlotMap || {};
for (var y in v) v[y]._renderFlag |= cc.RenderFlow.FLAG_BREAK_FLOW;
for (var b = 0, k = d.drawOrder.length; b < k; b++) if (null != (h = d.drawOrder[b]) && h.bone.active) {
this._slotRangeStart >= 0 && this._slotRangeStart == h.data.index && (this._inRange = !0);
if (this._inRange) {
this._slotRangeEnd >= 0 && this._slotRangeEnd == h.data.index && (this._inRange = !1);
this._vertexFloatCount = 0;
this._indexCount = 0;
if (n = h.getAttachment()) {
c = n instanceof spine.RegionAttachment;
l = n instanceof spine.MeshAttachment;
if (n instanceof spine.ClippingAttachment) _.clipStart(h, n); else if (c || l) {
var S = (this._comp._followSlotMap || {})[h.data.name];
if (S) {
this._renderer._flush();
this._renderer.worldMatDirty++;
S._updateLocalMatrix();
var C = new cc.Mat4(), I = C.m;
I[0] = h.bone.a;
I[1] = h.bone.c;
I[4] = h.bone.b;
I[5] = h.bone.d;
I[12] = h.bone.worldX;
I[13] = h.bone.worldY;
cc.Mat4.mul(C, C, S._matrix);
cc.Mat4.mul(S._worldMatrix, this._node._worldMatrix, C);
var A = h.color.a * n.color.a * f.a * this._nodeA * 255;
S.opacity = A;
var P = S._color._val;
S._color._fastSetA(A);
S._renderFlag &= ~(cc.RenderFlow.FLAG_TRANSFORM | cc.RenderFlow.FLAG_BREAK_FLOW);
cc.RenderFlow.visitRootNode(S);
S._renderFlag |= cc.RenderFlow.FLAG_BREAK_FLOW;
S._color._fastSetA(P);
this._renderer.worldMatDirty--;
this._renderer._flush();
this._renderer.node = this._comp.node;
} else if (m = this._getSlotMaterial(n.region.texture._texture, h.data.blendMode)) {
if (this._mustFlush || m.getHash() !== this._renderer.material.getHash()) {
this._mustFlush = !1;
this._renderer._flush();
this._renderer.node = this._node;
this._renderer.material = m;
}
if (c) {
o = this._quadTriangles;
this._vertexFloatCount = 4 * this._perVertexSize;
this._indexCount = 6;
u = this._buffer.request(4, 6);
this._indexOffset = u.indiceOffset, this._vertexOffset = u.vertexOffset, this._vertexFloatOffset = u.byteOffset >> 2;
e = this._buffer._vData, i = this._buffer._iData;
n.computeWorldVertices(h.bone, e, this._vertexFloatOffset, this._perVertexSize);
if (g && this._debugSlots) {
g.strokeColor = this._slotColor;
g.moveTo(e[this._vertexFloatOffset], e[this._vertexFloatOffset + 1]);
for (var O = this._vertexFloatOffset + this._perVertexSize, x = this._vertexFloatOffset + this._vertexFloatCount; O < x; O += this._perVertexSize) g.lineTo(e[O], e[O + 1]);
g.close();
g.stroke();
}
} else if (l) {
o = n.triangles;
this._vertexFloatCount = (n.worldVerticesLength >> 1) * this._perVertexSize;
this._indexCount = o.length;
u = this._buffer.request(this._vertexFloatCount / this._perVertexSize, this._indexCount);
this._indexOffset = u.indiceOffset, this._vertexOffset = u.vertexOffset, this._vertexFloatOffset = u.byteOffset >> 2;
e = this._buffer._vData, i = this._buffer._iData;
n.computeWorldVertices(h, 0, n.worldVerticesLength, e, this._vertexFloatOffset, this._perVertexSize);
if (g && this._debugMesh) {
g.strokeColor = this._meshColor;
for (var w = 0, N = o.length; w < N; w += 3) {
var T = o[w] * this._perVertexSize + this._vertexFloatOffset, M = o[w + 1] * this._perVertexSize + this._vertexFloatOffset, L = o[w + 2] * this._perVertexSize + this._vertexFloatOffset;
g.moveTo(e[T], e[T + 1]);
g.lineTo(e[M], e[M + 1]);
g.lineTo(e[L], e[L + 1]);
g.close();
g.stroke();
}
}
}
if (0 != this._vertexFloatCount && 0 != this._indexCount) {
i.set(o, this._indexOffset);
r = n.uvs;
for (var D = this._vertexFloatOffset, F = this._vertexFloatOffset + this._vertexFloatCount, j = 0; D < F; D += this._perVertexSize, 
j += 2) {
e[D + 2] = r[j];
e[D + 3] = r[j + 1];
}
a = n.color, s = h.color;
this.fillVertices(f, a, s, _, h);
e = this._buffer._vData, i = this._buffer._iData;
if (this._indexCount > 0) {
for (var E = this._indexOffset, U = this._indexOffset + this._indexCount; E < U; E++) i[E] += this._vertexOffset;
if (t) {
p = t.m;
this._m00 = p[0];
this._m04 = p[4];
this._m12 = p[12];
this._m01 = p[1];
this._m05 = p[5];
this._m13 = p[13];
for (var R = this._vertexFloatOffset, q = this._vertexFloatOffset + this._vertexFloatCount; R < q; R += this._perVertexSize) {
this._x = e[R];
this._y = e[R + 1];
e[R] = this._x * this._m00 + this._y * this._m04 + this._m12;
e[R + 1] = this._x * this._m01 + this._y * this._m05 + this._m13;
}
}
this._buffer.adjust(this._vertexFloatCount / this._perVertexSize, this._indexCount);
}
_.clipEndWithSlot(h);
} else _.clipEndWithSlot(h);
} else _.clipEndWithSlot(h);
} else _.clipEndWithSlot(h);
} else _.clipEndWithSlot(h);
} else _.clipEndWithSlot(h);
}
_.clipEnd();
if (g && this._debugBones) {
var W;
g.strokeColor = this._boneColor;
g.fillColor = this._slotColor;
for (var J = 0, V = d.bones.length; J < V; J++) {
var H = (W = d.bones[J]).data.length * W.a + W.worldX, B = W.data.length * W.c + W.worldY;
g.moveTo(W.worldX, W.worldY);
g.lineTo(H, B);
g.stroke();
g.circle(W.worldX, W.worldY, 1.5 * Math.PI);
g.fill();
0 === J && (g.fillColor = this._originColor);
}
}
};
i.cacheTraverse = function(t) {
var e = this._comp._curFrame;
if (e) {
var i = e.segments;
if (0 != i.length) {
var n, a, s, r, o, c, l = e.vertices, u = e.indices, h = 0, p = 0, d = 0;
if (t) {
c = t.m;
this._m00 = c[0];
this._m01 = c[1];
this._m04 = c[4];
this._m05 = c[5];
this._m12 = c[12];
this._m13 = c[13];
}
var f = 1 === this._m00 && 0 === this._m01 && 0 === this._m04 && 1 === this._m05, g = 16 & this._handleVal, _ = g && f, m = 0, v = e.colors, y = v[m++], b = y.vfOffset;
this._handleColor(y);
for (var k = 0, S = i.length; k < S; k++) {
var C = i[k];
if (r = this._getSlotMaterial(C.tex, C.blendMode)) {
if (this._mustFlush || r.getHash() !== this._renderer.material.getHash()) {
this._mustFlush = !1;
this._renderer._flush();
this._renderer.node = this._node;
this._renderer.material = r;
}
this._vertexCount = C.vertexCount;
this._indexCount = C.indexCount;
o = this._buffer.request(this._vertexCount, this._indexCount);
this._indexOffset = o.indiceOffset;
this._vertexOffset = o.vertexOffset;
this._vfOffset = o.byteOffset >> 2;
n = this._buffer._vData;
a = this._buffer._iData;
s = this._buffer._uintVData;
for (var I = this._indexOffset, A = this._indexOffset + this._indexCount; I < A; I++) a[I] = this._vertexOffset + u[p++];
d = C.vfCount;
n.set(l.subarray(h, h + d), this._vfOffset);
h += d;
if (_) for (var P = this._vfOffset, O = this._vfOffset + d; P < O; P += 6) {
n[P] += this._m12;
n[P + 1] += this._m13;
} else if (g) for (var x = this._vfOffset, w = this._vfOffset + d; x < w; x += 6) {
this._x = n[x];
this._y = n[x + 1];
n[x] = this._x * this._m00 + this._y * this._m04 + this._m12;
n[x + 1] = this._x * this._m01 + this._y * this._m05 + this._m13;
}
this._buffer.adjust(this._vertexCount, this._indexCount);
if (this._needColor) for (var N = h - d, T = this._vfOffset + 4, M = this._vfOffset + 4 + d; T < M; T += 6, 
N += 6) {
if (N >= b) {
y = v[m++];
this._handleColor(y);
b = y.vfOffset;
}
s[T] = this._finalColor32;
s[T + 1] = this._darkColor32;
}
}
}
}
}
};
i.fillBuffers = function(t, e) {
var i = t.node;
i._renderFlag |= cc.RenderFlow.FLAG_UPDATE_RENDER_DATA;
if (t._skeleton) {
var n = i._color;
this._nodeR = n.r / 255;
this._nodeG = n.g / 255;
this._nodeB = n.b / 255;
this._nodeA = n.a / 255;
this._useTint = t.useTint || t.isAnimationCached();
this._vertexFormat = this._useTint ? o : r;
this._perVertexSize = this._useTint ? 6 : 5;
this._node = t.node;
this._buffer = e.getBuffer("spine", this._vertexFormat);
this._renderer = e;
this._comp = t;
this._mustFlush = !0;
this._premultipliedAlpha = t.premultipliedAlpha;
this._multiplier = 1;
this._handleVal = 0;
this._needColor = !1;
this._vertexEffect = t._effectDelegate && t._effectDelegate.this._vertexEffect;
(4294967295 !== n._val || this._premultipliedAlpha) && (this._needColor = !0);
this._useTint && (this._handleVal |= 1);
var a = void 0;
if (this._comp.enableBatch) {
a = this._node._worldMatrix;
this._mustFlush = !1;
this._handleVal |= 16;
}
if (t.isAnimationCached()) this.cacheTraverse(a); else {
this._vertexEffect && this._vertexEffect.begin(t._skeleton);
this.realTimeTraverse(a);
this._vertexEffect && this._vertexEffect.end();
}
e.worldMatDirty++;
t.attachUtil._syncAttachedNode();
this._node = void 0;
this._buffer = void 0;
this._renderer = void 0;
this._comp = void 0;
this._vertexEffect = null;
}
};
i.postFillBuffers = function(t, e) {
e.worldMatDirty--;
};
return e;
}(cc.Assembler)) || n;
i.default = h;
cc.Assembler.register(sp.Skeleton, h);
e.exports = i.default;
cc._RF.pop();
}, {} ],
spine: [ function(t, e) {
"use strict";
cc._RF.push(e, "7a9f7cFwctFu4S4dXK5sJWg", "spine");
var i = sp.Skeleton, n = function() {}, a = function(t) {
Object.defineProperty(t, "_worldMatDirty", {
get: function() {
return !0;
},
set: function() {}
});
t._calculWorldMatrix = n;
t._mulMat = n;
};
Object.defineProperty(i.prototype, "followSlotMap", {
set: function(t) {
this._followSlotMap = t;
},
get: function() {
null == this._followSlotMap && (this._followSlotMap = {});
return this._followSlotMap;
},
configurable: !0
});
Object.defineProperty(i.prototype, "followBoneMap", {
set: function(t) {
this._followBoneMap = t;
},
get: function() {
null == this._followBoneMap && (this._followBoneMap = {});
return this._followBoneMap;
},
configurable: !0
});
i.prototype.findFollowSlot = function(t) {
return this.followSlotMap[t];
};
i.prototype.findFollowBone = function(t) {
return this.followBoneMap[t];
};
i.prototype.addChildFollowSlot = function(t, e) {
var i = this.followSlotMap[t];
i && i.removeFromParent();
a(e);
this.followSlotMap[t] = e;
e.parent = this.node;
e._renderFlag |= cc.RenderFlow.FLAG_BREAK_FLOW;
e.name = t;
e.x = 0;
e.y = 0;
this._skeleton && this._skeleton.addSlotBindNode && this._skeleton.addSlotBindNode(t);
};
i.prototype.removeChildFollowSlot = function(t) {
var e = this.followSlotMap[t];
e && e.removeFromParent();
delete this.followSlotMap[t];
this._skeleton && this._skeleton.removeSlotBindNode && this._skeleton.removeSlotBindNode(t);
};
i.prototype.addChildFollowBone = function(t, e) {
var i = this.followBoneMap[t];
i && i.removeFromParent();
this.followBoneMap[t] = e;
e.parent = this.node;
};
i.prototype.removeChildFollowBone = function(t) {
var e = this.followBoneMap[t];
e && e.removeFromParent();
delete this.followBoneMap[t];
};
var s = i.prototype.setAnimation;
i.prototype.setAnimation = function(t, e, i) {
if (null == e || "" == e) {
cc.error("spine animation name can not be empty");
return null;
}
this["@waitSetAnimation"] = null;
if (null == this._skeleton) {
this["@waitSetAnimation"] = {
trackIndex: t,
name: e,
loop: i
};
return null;
}
return s.call(this, t, e, i);
};
var r = i.prototype.setSkeletonData;
i.prototype.setSkeletonData = function(t) {
var e = this;
r.call(this, t);
if (this._skeleton && cc.sys.isNative) {
var i = Object.keys(this.followSlotMap);
if (i.length <= 0) return;
i.forEach(function(t) {
e._skeleton && e._skeleton.addSlotBindNode && e._skeleton.addSlotBindNode(t);
});
}
};
var o = i.prototype.update;
i.prototype.update = function(t) {
o.call(this, t);
this._updateFollowBone();
};
i.prototype._updateFollowBone = function() {
if (!(Object.keys(this.followBoneMap).length <= 0) && this._skeleton) for (var t = void 0, e = void 0, i = 0, n = this._skeleton.bones.length; i < n; i++) {
t = this._skeleton.bones[i];
if (e = this.followBoneMap[t.data.name]) {
e.setPosition(t.worldX, t.worldY);
e.angle = t.rotation;
}
}
};
i.prototype.getBonePosition = function(t) {
if (this._skeleton) {
var e = this.findBone(t);
if (e) {
var i = this.node;
e.updateWorldTransform();
return 0 == e.worldY && 0 == e.worldX ? cc.v2(e.x * i.scaleX, e.y * i.scaleY) : cc.v2(e.worldX * i.scaleX, e.worldY * i.scaleY);
}
}
return cc.v2(-1, -1);
};
i.prototype.onSpineLoaded = function() {
var t = this;
if (this._skeleton && cc.sys.isNative) {
var e = Object.keys(this.followSlotMap);
if (e.length <= 0) return;
var i = [];
e.forEach(function(e) {
null == t.findSlot(e) && i.push(e);
});
i.forEach(function(e) {
t.removeChildFollowSlot(e);
});
}
if (this["@waitSetAnimation"]) {
var n = this["@waitSetAnimation"], a = n.trackIndex, s = n.name, r = n.loop;
this.setAnimation(a, s, r);
}
};
var c = sp.spine.AnimationState;
c.prototype.trackEntry;
c.prototype.trackEntry = function(t, e, i, n) {
var a = this.trackEntryPool.obtain();
a.trackIndex = t;
a.animation = e;
a.loop = i;
a.holdPrevious = !1;
a.eventThreshold = 0;
a.attachmentThreshold = 0;
a.drawOrderThreshold = 0;
a.animationStart = 0;
a.animationEnd = e.duration;
a.animationLast = -1;
a.nextAnimationLast = -1;
a.delay = 0;
a.trackTime = 0;
a.trackLast = -1;
a.nextTrackLast = -1;
a.trackEnd = Number.MAX_VALUE;
a.timeScale = 1;
a.alpha = 1;
a.interruptAlpha = 1;
a.mixTime = 0;
a.mixDuration = null == n ? 0 : this.data.getMix(n.animation, e);
return a;
};
c.prototype.setAnimationWith;
c.prototype.setAnimationWith = function(t, e, i) {
var n;
if (null == e) throw new Error("animation cannot be null.");
var a = !0, s = this.expandToIndex(t);
if (null != s) {
(null === (n = s.mixingFrom) || void 0 === n ? void 0 : n.mixingFrom) && (s.mixingFrom.mixingFrom = null);
if (-1 == s.nextTrackLast) {
this.tracks[t] = s.mixingFrom;
this.queue.interrupt(s);
this.queue.end(s);
this.disposeNext(s);
s = s.mixingFrom;
a = !1;
} else this.disposeNext(s);
}
var r = this.trackEntry(t, e, i, s);
this.setCurrent(t, r, a);
this.queue.drain();
return r;
};
cc._RF.pop();
}, {} ],
"tdanalytics.mg.cocoscreator.min": [ function(t, e) {
(function(t) {
"use strict";
cc._RF.push(e, "28ce7ta1m5BwppvkFBw2IdB", "tdanalytics.mg.cocoscreator.min");
function i(t) {
return (i = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function(t) {
return typeof t;
} : function(t) {
return t && "function" == typeof Symbol && t.constructor === Symbol && t !== Symbol.prototype ? "symbol" : typeof t;
})(t);
}
function n(t, e) {
if (!(t instanceof e)) throw new TypeError("Cannot call a class as a function");
}
function a(t, e) {
for (var i = 0; i < e.length; i++) {
var n = e[i];
n.enumerable = n.enumerable || !1, n.configurable = !0, "value" in n && (n.writable = !0), 
Object.defineProperty(t, n.key, n);
}
}
function s(t, e, i) {
return e && a(t.prototype, e), i && a(t, i), t;
}
var r = {}, o = Array.prototype, c = Object.prototype, l = o.slice, u = c.toString, h = Object.prototype.hasOwnProperty, p = o.forEach, d = Array.isArray, f = {}, g = [ "utm_source", "utm_medium", "utm_campaign", "utm_content", "utm_term" ];
r.each = function(t, e, i) {
if (null == t) return !1;
if (p && t.forEach === p) t.forEach(e, i); else if (t.length === +t.length) {
for (var n = 0, a = t.length; n < a; n++) if (n in t && e.call(i, t[n], n, t) === f) return !1;
} else for (var s in t) if (h.call(t, s) && e.call(i, t[s], s, t) === f) return !1;
}, r.extend = function(t) {
return r.each(l.call(arguments, 1), function(e) {
for (var i in e) void 0 !== e[i] && (t[i] = e[i]);
}), t;
}, r.extend2Layers = function(t) {
return r.each(l.call(arguments, 1), function(e) {
for (var i in e) void 0 !== e[i] && (r.isObject(e[i]) && r.isObject(t[i]) ? r.extend(t[i], e[i]) : t[i] = e[i]);
}), t;
}, r.isArray = d || function(t) {
return "[object Array]" === u.call(t);
}, r.isFunction = function(t) {
try {
return "function" == typeof t;
} catch (t) {
return !1;
}
}, r.isPromise = function(t) {
return "[object Promise]" === u.call(t) && null != t;
}, r.isObject = function(t) {
return "[object Object]" === u.call(t) && null != t;
}, r.isEmptyObject = function(t) {
if (r.isObject(t)) {
for (var e in t) if (h.call(t, e)) return !1;
return !0;
}
return !1;
}, r.isUndefined = function(t) {
return void 0 === t;
}, r.isString = function(t) {
return "[object String]" === u.call(t);
}, r.isDate = function(t) {
return "[object Date]" === u.call(t);
}, r.isBoolean = function(t) {
return "[object Boolean]" === u.call(t);
}, r.isNumber = function(t) {
return "[object Number]" === u.call(t) && /[\d\.]+/.test(String(t));
}, r.isJSONString = function(t) {
try {
JSON.parse(t);
} catch (t) {
return !1;
}
return !0;
}, r.decodeURIComponent = function(t) {
var e = "";
try {
e = decodeURIComponent(t);
} catch (i) {
e = t;
}
return e;
}, r.encodeURIComponent = function(t) {
var e = "";
try {
e = encodeURIComponent(t);
} catch (i) {
e = t;
}
return e;
}, r.utf8Encode = function(t) {
for (var e, i = "", n = e = 0, a = (t = (t + "").replace(/\r\n/g, "\n").replace(/\r/g, "\n")).length, s = 0; s < a; s++) {
var r = t.charCodeAt(s), o = null;
r < 128 ? e++ : o = 127 < r && r < 2048 ? String.fromCharCode(r >> 6 | 192, 63 & r | 128) : String.fromCharCode(r >> 12 | 224, r >> 6 & 63 | 128, 63 & r | 128), 
null !== o && (n < e && (i += t.substring(n, e)), i += o, n = e = s + 1);
}
return n < e && (i += t.substring(n, t.length)), i;
}, r.base64Encode = function(t) {
var e, i, n, a, s = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=", o = 0, c = 0, l = "", u = [];
if (!t) return t;
for (t = r.utf8Encode(t); e = (a = t.charCodeAt(o++) << 16 | t.charCodeAt(o++) << 8 | t.charCodeAt(o++)) >> 18 & 63, 
i = a >> 12 & 63, n = a >> 6 & 63, a &= 63, u[c++] = s.charAt(e) + s.charAt(i) + s.charAt(n) + s.charAt(a), 
o < t.length; ) ;
switch (l = u.join(""), t.length % 3) {
case 1:
l = l.slice(0, -2) + "==";
break;

case 2:
l = l.slice(0, -1) + "=";
}
return l;
}, r.encodeDates = function(t) {
return r.each(t, function(e, i) {
if (r.isDate(e)) t[i] = r.formatDate(e); else if (r.isObject(e)) t[i] = r.encodeDates(e); else if (r.isArray(e)) for (var n = 0; n < e.length; n++) r.isDate(e[n]) && (t[i][n] = r.formatDate(e[n]));
}), t;
}, r.formatDate = function(t) {
function e(t) {
return t < 10 ? "0" + t : t;
}
return t.getFullYear() + "-" + e(t.getMonth() + 1) + "-" + e(t.getDate()) + " " + e(t.getHours()) + ":" + e(t.getMinutes()) + ":" + e(t.getSeconds()) + "." + ((i = t.getMilliseconds()) < 100 && 9 < i ? "0" + i : i < 10 ? "00" + i : i);
var i;
}, r.formatTimeZone = function(t, e) {
if ("number" != typeof e) return t;
var i = t.getTime();
t = 6e4 * t.getTimezoneOffset();
return new Date(i + t + 36e5 * e);
}, r.getTimeZone = function(t, e) {
return "number" == typeof e ? e : 0 - t.getTimezoneOffset() / 60;
}, r.searchObjDate = function(t, e) {
try {
(r.isObject(t) || r.isArray(t)) && r.each(t, function(i, n) {
r.isObject(i) || r.isArray(i) ? r.searchObjDate(t[n], e) : r.isDate(i) && (t[n] = r.formatDate(r.formatTimeZone(i, e)));
});
} catch (t) {
_.warn(t);
}
}, r.UUID = function() {
var t = new Date().getTime();
return String(Math.random()).replace(".", "").slice(1, 11) + "-" + t;
}, r.UUIDv4 = function() {
return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(t) {
var e = 16 * Math.random() | 0;
return ("x" === t ? e : 3 & e | 8).toString(16);
});
}, r.setMpPlatform = function(t) {
r.mpPlatform = t;
}, r.getMpPlatform = function() {
return r.mpPlatform;
}, r.createExtraHeaders = function() {
return {
"TA-Integration-Type": "MG",
"TA-Integration-Version": "3.3.0-beta.2",
"TA-Integration-Count": "1",
"TA-Integration-Extra": r.getMpPlatform()
};
}, r.checkAppId = function(t) {
if (t) return t.replace(/\s+/g, "");
}, r.checkUrl = function(t) {
if (t) return t = t.replace(/\s+/g, ""), r.url("basic", t);
}, r.url = function() {
function t() {
return new RegExp(/(.*?)\.?([^.]*?)\.(com|net|org|biz|ws|in|me|co\.uk|co|org\.uk|ltd\.uk|plc\.uk|me\.uk|edu|mil|br\.com|cn\.com|eu\.com|hu\.com|no\.com|qc\.com|sa\.com|se\.com|se\.net|us\.com|uy\.com|ac|co\.ac|gv\.ac|or\.ac|ac\.ac|af|am|as|at|ac\.at|co\.at|gv\.at|or\.at|asn\.au|com\.au|edu\.au|org\.au|net\.au|id\.au|be|ac\.be|adm\.br|adv\.br|am\.br|arq\.br|art\.br|bio\.br|cng\.br|cnt\.br|com\.br|ecn\.br|eng\.br|esp\.br|etc\.br|eti\.br|fm\.br|fot\.br|fst\.br|g12\.br|gov\.br|ind\.br|inf\.br|jor\.br|lel\.br|med\.br|mil\.br|net\.br|nom\.br|ntr\.br|odo\.br|org\.br|ppg\.br|pro\.br|psc\.br|psi\.br|rec\.br|slg\.br|tmp\.br|tur\.br|tv\.br|vet\.br|zlg\.br|br|ab\.ca|bc\.ca|mb\.ca|nb\.ca|nf\.ca|ns\.ca|nt\.ca|on\.ca|pe\.ca|qc\.ca|sk\.ca|yk\.ca|ca|cc|ac\.cn|net\.cn|com\.cn|edu\.cn|gov\.cn|org\.cn|bj\.cn|sh\.cn|tj\.cn|cq\.cn|he\.cn|nm\.cn|ln\.cn|jl\.cn|hl\.cn|js\.cn|zj\.cn|ah\.cn|gd\.cn|gx\.cn|hi\.cn|sc\.cn|gz\.cn|yn\.cn|xz\.cn|sn\.cn|gs\.cn|qh\.cn|nx\.cn|xj\.cn|tw\.cn|hk\.cn|mo\.cn|cn|cx|cz|de|dk|fo|com\.ec|tm\.fr|com\.fr|asso\.fr|presse\.fr|fr|gf|gs|co\.il|net\.il|ac\.il|k12\.il|gov\.il|muni\.il|ac\.in|co\.in|org\.in|ernet\.in|gov\.in|net\.in|res\.in|is|it|ac\.jp|co\.jp|go\.jp|or\.jp|ne\.jp|ac\.kr|co\.kr|go\.kr|ne\.kr|nm\.kr|or\.kr|li|lt|lu|asso\.mc|tm\.mc|com\.mm|org\.mm|net\.mm|edu\.mm|gov\.mm|ms|nl|no|nu|pl|ro|org\.ro|store\.ro|tm\.ro|firm\.ro|www\.ro|arts\.ro|rec\.ro|info\.ro|nom\.ro|nt\.ro|se|si|com\.sg|org\.sg|net\.sg|gov\.sg|sk|st|tf|ac\.th|co\.th|go\.th|mi\.th|net\.th|or\.th|tm|to|com\.tr|edu\.tr|gov\.tr|k12\.tr|net\.tr|org\.tr|com\.tw|org\.tw|net\.tw|ac\.uk|uk\.com|uk\.net|gb\.com|gb\.net|vg|sh|kz|ch|info|ua|gov|name|pro|ie|hk|com\.hk|org\.hk|net\.hk|edu\.hk|us|tk|cd|by|ad|lv|eu\.lv|bz|es|jp|cl|ag|mobi|eu|co\.nz|org\.nz|net\.nz|maori\.nz|iwi\.nz|io|la|md|sc|sg|vc|tw|travel|my|se|tv|pt|com\.pt|edu\.pt|asia|fi|com\.ve|net\.ve|fi|org\.ve|web\.ve|info\.ve|co\.ve|tel|im|gr|ru|net\.ru|org\.ru|hr|com\.hr|ly|xyz)$/);
}
function e(t, e) {
var i = t.charAt(0);
e = e.split(i);
return i === t ? e : e[(t = parseInt(t.substring(1), 10)) < 0 ? e.length + t : t - 1];
}
function i(t, e) {
for (var i, n, a = t.charAt(0), s = e.split("&"), o = [], c = {}, l = t.substring(1), u = 0, h = s.length; u < h; u++) if ("" !== (o = (o = s[u].match(/(.*?)=(.*)/)) || [ s[u], s[u], "" ])[1].replace(/\s/g, "")) {
if (o[2] = (n = o[2] || "", r.decodeURIComponent(n.replace(/\+/g, " "))), l === o[1]) return o[2];
(i = o[1].match(/(.*)\[([0-9]+)\]/)) ? (c[i[1]] = c[i[1]] || [], c[i[1]][i[2]] = o[2]) : c[o[1]] = o[2];
}
return a === t ? c : c[l];
}
return function(n, a) {
var s = {};
if ("tld?" === n) return t();
if (a = a || window.location.toString(), !n) return a;
if (n = n.toString(), a.match(/^mailto:([^/].+)/)) r = a.match(/^mailto:([^/].+)/), 
s.protocol = "mailto", s.email = r[1]; else {
if (a.match(/(.*?)\/#!(.*)/) && (a = (r = a.match(/(.*?)\/#!(.*)/))[1] + r[2]), 
a.match(/(.*?)#(.*)/) && (r = a.match(/(.*?)#(.*)/), s.hash = r[2], a = r[1]), s.hash && n.match(/^#/)) return i(n, s.hash);
if (a.match(/(.*?)\?(.*)/) && (r = a.match(/(.*?)\?(.*)/), s.query = r[2], a = r[1]), 
s.query && n.match(/^\?/)) return i(n, s.query);
if (a.match(/(.*?):?\/\/(.*)/) && (r = a.match(/(.*?):?\/\/(.*)/), s.protocol = r[1].toLowerCase(), 
a = r[2]), a.match(/(.*?)(\/.*)/) && (r = a.match(/(.*?)(\/.*)/), s.path = r[2], 
a = r[1]), s.path = (s.path || "").replace(/^([^/])/, "/$1").replace(/\/$/, ""), 
n.match(/^[-0-9]+$/) && (n = n.replace(/^([^/])/, "/$1")), n.match(/^\//)) return e(n, s.path.substring(1));
if ((r = (r = e("/-1", s.path.substring(1))) && r.match(/(.*?)\.(.*)/)) && (s.file = r[0], 
s.filename = r[1], s.fileext = r[2]), a.match(/(.*):([0-9]+)$/) && (r = a.match(/(.*):([0-9]+)$/), 
s.port = r[2], a = r[1]), a.match(/(.*?)@(.*)/) && (r = a.match(/(.*?)@(.*)/), s.auth = r[1], 
a = r[2]), s.auth && (r = s.auth.match(/(.*):(.*)/), s.user = r ? r[1] : s.auth, 
s.pass = r ? r[2] : void 0), s.hostname = a.toLowerCase(), "." === n.charAt(0)) return e(n, s.hostname);
t() && (r = s.hostname.match(t())) && (s.tld = r[3], s.domain = r[2] ? r[2] + "." + r[3] : void 0, 
s.sub = r[1] || void 0);
var r = s.port ? ":" + s.port : "";
s.protocol = s.protocol || window.location.protocol.replace(":", ""), s.port = s.port || ("https" === s.protocol ? "443" : "80"), 
s.protocol = s.protocol || ("443" === s.port ? "https" : "http"), s.basic = s.protocol + "://" + s.hostname + r;
}
return n in s ? s[n] : "{}" === n ? s : "";
};
}(), r.createString = function(t) {
for (var e = t, i = Math.random().toString(36).substr(2); i.length < e; ) i += Math.random().toString(36).substr(2);
return i.substr(0, t);
}, r.createAesKey = function() {
return r.createString(16);
}, r.generateEncryptyData = function(t, e) {
if (void 0 === e) return t;
var i = e.publicKey, n = e.version;
if (void 0 === i || void 0 === n) return t;
if ("undefined" == typeof CryptoJS || "undefined" == typeof JSEncrypt) return t;
var a = r.createAesKey();
try {
var s = CryptoJS.enc.Utf8.parse(a), o = CryptoJS.enc.Utf8.parse(JSON.stringify(t)), c = r.isUndefined(CryptoJS.pad.Pkcs7) ? CryptoJS.pad.PKCS7 : CryptoJS.pad.Pkcs7;
s = CryptoJS.AES.encrypt(o, s, {
mode: CryptoJS.mode.ECB,
padding: c
}).toString();
(c = new JSEncrypt()).setPublicKey(i);
return !1 === (c = c.encrypt(a)) ? (_.warn("Encryption failed, return the original data"), 
t) : {
pkv: n,
ekey: c,
payload: s
};
} catch (t) {
_.warn("Encryption failed, return the original data: " + t);
}
return t;
}, r.getUtm = function() {
var t = {};
return r.each(g, function(e) {
try {
var i = r.getQueryParam(location.href, e);
i.length && (t[e] = i);
} catch (e) {
_.warn("get utm fail: " + e);
}
}), JSON.stringify(t);
}, r.getQueryParam = function(t, e) {
e = e.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]"), t = r.decodeURIComponent(t);
return null === (t = new RegExp("[\\?&]" + e + "=([^&#]*)").exec(t)) || t && "string" != typeof t[1] && t[1].length ? "" : r.decodeURIComponent(t[1]);
}, r.getUtmFromQuery = function(t) {
var e = {};
return r.each(g, function(i) {
t[i] && (e[i] = t[i]);
}), JSON.stringify(e);
}, r.indexOf = function(t, e) {
var i = t.indexOf;
if (i) return i.call(t, e);
for (var n = 0; n < t.length; n++) if (e === t[n]) return n;
return -1;
}, r.checkCalibration = function(t) {
return t;
}, r.isClickType = function(t) {
return {
tap: 1,
longpress: 1,
longtap: 1
}[t];
}, r.getCurrentTimeStamp = function() {
return Date.now();
}, r.getCurrentDate = function() {
return new Date(Date.now());
};
var _ = "object" === i(_) ? _ : {};
_.info = function() {
if ("object" === ("undefined" == typeof console ? "undefined" : i(console)) && console.log && _.enabled) try {
return arguments[0] = "[ThinkingData] Info: " + arguments[0], _.listener && _.listener(arguments[0]), 
console.log.apply(console, arguments);
} catch (t) {
_.listener && _.listener(arguments[0]), console.log("[ThinkingData] Info: " + arguments[0]);
}
}, _.warn = function() {
if ("object" === ("undefined" == typeof console ? "undefined" : i(console)) && console.log && _.enabled) try {
return arguments[0] = "[ThinkingData] Warning: " + arguments[0], console.warn.apply(console, arguments);
} catch (t) {
console.warn("[ThinkingData] Warning: " + arguments[0]);
}
};
var m = function() {
function t() {
n(this, t), this.config = {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_mg"
};
}
return s(t, [ {
key: "getConfig",
value: function() {
return this.config;
}
}, {
key: "initSdkConfig",
value: function() {}
}, {
key: "getStorage",
value: function(t, e, i) {
t = localStorage.getItem(t);
if (!e) return r.isJSONString(t) ? JSON.parse(t) : {};
r.isJSONString(t) ? i(JSON.parse(t)) : i({});
}
}, {
key: "setStorage",
value: function(t, e) {
localStorage.setItem(t, e);
}
}, {
key: "removeStorage",
value: function(t) {
localStorage.removeItem(t);
}
}, {
key: "_setSystemProxy",
value: function(t) {
this._sysCallback = t;
}
}, {
key: "getSystemInfo",
value: function(t) {
var e = {
mp_platform: "web",
system: this._getOs(),
screenWidth: window.screen.width,
screenHeight: window.screen.height,
systemLanguage: navigator.language
};
this._sysCallback && (e = r.extend(e, this._sysCallback(t))), t.success(e), t.complete();
}
}, {
key: "_getOs",
value: function() {
var t = navigator.userAgent;
return /Windows/i.test(t) ? /Phone/.test(t) || /WPDesktop/.test(t) ? "Windows Phone" : "Windows" : /(iPhone|iPad|iPod)/.test(t) ? "iOS" : /Android/.test(t) ? "Android" : /(BlackBerry|PlayBook|BB10)/i.test(t) ? "BlackBerry" : /Mac/i.test(t) ? "MacOS" : /Linux/.test(t) ? "Linux" : /CrOS/.test(t) ? "ChromeOS" : "";
}
}, {
key: "getNetworkType",
value: function(t) {
t.complete();
}
}, {
key: "onNetworkStatusChange",
value: function() {}
}, {
key: "request",
value: function(t) {
var e = {}, i = new XMLHttpRequest();
if (i.open(t.method, t.url), t.header) for (var n in t.header) i.setRequestHeader(n, t.header[n]);
return i.onreadystatechange = function() {
4 === i.readyState && 200 === i.status ? (e.statusCode = 200, r.isJSONString(i.responseText) && (e.data = JSON.parse(i.responseText)), 
t.success(e)) : 200 !== i.status && (e.errMsg = "network error", t.fail(e));
}, i.ontimeout = function() {
e.errMsg = "timeout", t.fail(e);
}, i.send(t.data), i;
}
}, {
key: "initAutoTrackInstance",
value: function(t, e) {
this.instance = t, this.autoTrack = e.autoTrack;
var i = this;
i.onPageShow(), i.autoTrack.appHide && i.instance.timeEvent("ta_page_hide"), "onvisibilitychange" in document && (document.onvisibilitychange = function() {
document.hidden ? i.onPageHide(!0) : (i.onPageShow(), i.autoTrack.appHide && i.instance.timeEvent("ta_page_hide"));
});
}
}, {
key: "setGlobal",
value: function(t, e) {
window[e] = t;
}
}, {
key: "getAppOptions",
value: function() {}
}, {
key: "showToast",
value: function() {}
}, {
key: "onPageShow",
value: function() {
var t;
this.autoTrack.appShow && (t = {}, r.extend(t, this.autoTrack.properties), r.isFunction(this.autoTrack.callback) && r.extend(t, this.autoTrack.callback("appShow")), 
this.instance._internalTrack("ta_page_show", t));
}
}, {
key: "onPageHide",
value: function(t) {
var e;
this.autoTrack.appHide && (e = {}, r.extend(e, this.autoTrack.properties), r.isFunction(this.autoTrack.callback) && r.extend(e, this.autoTrack.callback("appHide")), 
this.instance._internalTrack("ta_page_hide", e, new Date(), null, t));
}
}, {
key: "setGlobalData",
value: function() {}
} ], [ {
key: "createInstance",
value: function() {
return new t();
}
} ]), t;
}(), v = {
data: 1,
onLoad: 1,
onShow: 1,
onReady: 1,
onPullDownRefresh: 1,
onShareAppMessage: 1,
onShareTimeline: 1,
onReachBottom: 1,
onPageScroll: 1,
onResize: 1,
onTabItemTap: 1,
onHide: 1,
onUnload: 1,
onAddToFavorites: 1
}, y = function() {
function t(e, i) {
n(this, t), this.taInstance = e, this.config = i.autoTrack || {}, this.disablePresetList = i.disablePresetProperties || [], 
this.referrer = "Directly open", this.config.isPlugin ? (e.App = function() {
App.apply(this, arguments);
}, inension(e.Page)) : (e = App, App = this._initAppExtention(e), e = Page, Page = this._initPageExtension(e));
}
return s(t, [ {
key: "_initPageExtension",
value: function(t) {
var e = this;
return function(i) {
var n = i.onShow, a = i.onShareAppMessage, s = i.onUnload, r = i.onAddToFavorites;
return i.onShow = function(t) {
e.onPageShow(), "function" == typeof n && n.call(this, t);
}, "function" == typeof a && (i.onShareAppMessage = function(t) {
t = a.call(this, t);
return e.onPageShare(t);
}), i.onUnload = function() {
e.onPageUnload(), "function" == typeof s && s.call(this);
}, i.onAddToFavorites = function() {
e.onPageAddToFavorites(), "function" == typeof r && r.call(this);
}, e._handleClickProxy(i), t(i);
};
}
}, {
key: "_handleClickProxy",
value: function(t) {
if (this.config.mpClick) {
var e, i = [];
for (e in t) "function" != typeof t[e] || v[e] || i.push(e);
for (var n = 0; n < i.length; n++) this.clickMethodProxy(t, i[n]);
}
}
}, {
key: "clickMethodProxy",
value: function(t, e) {
var i = this, n = t[e];
t[e] = function() {
var t = n.call(this, arguments), e = arguments[0];
return r.isObject(e) && i._trackClickEvent(e), t;
};
}
}, {
key: "_trackClickEvent",
value: function(t) {
var e = t.currentTarget || {}, i = t.target || {};
i.id && e.id && i.id !== e.id || (i = {}, (t = t.type) && r.isClickType(t) && (t = e.dataset || {}, 
this.disablePresetList.includes("#element_id") || (i["#element_id"] = e.id), this.disablePresetList.includes("#element_type") || (i["#element_type"] = t.type), 
this.disablePresetList.includes("#element_content") || (i["#element_content"] = t.content), 
this.disablePresetList.includes("#element_name") || (i["#element_name"] = t.name), 
this.disablePresetList.includes("#url_path") || (i.$url_path = this._getCurrentPath()), 
r.extend(i, this.config.properties), r.isFunction(this.config.callback) && r.extend(i, this.config.callback("mpClick")), 
this.taInstance._internalTrack("ta_mp_click", i)));
}
}, {
key: "_initAppExtention",
value: function(t) {
var e = this;
return function(i) {
var n = i.onLaunch, a = i.onShow, s = i.onHide;
return i.onLaunch = function(t) {
e.onAppLaunch(t, this), "function" == typeof n && n.call(this, t);
}, i.onShow = function(t) {
e.onAppShow(t), "function" == typeof a && a.call(this, t);
}, i.onHide = function() {
e.onAppHide(), "function" == typeof s && s.call(this);
}, t(i);
};
}
}, {
key: "onAppLaunch",
value: function(t, e) {
this._setAutoTrackProperties(t), r.isUndefined(e) || (e[this.taInstance.name] = this.taInstance), 
this.config.appLaunch && (e = {}, this.disablePresetList.includes("#url_path") || t && t.path && (e["#url_path"] = this._getPath(t.path)), 
t && (this.disablePresetList.includes("#utm") || t.query && (e["#utm"] = r.getUtmFromQuery(t.query)), 
this.disablePresetList.includes("#start_reason") || (e["#start_reason"] = JSON.stringify(t))), 
r.extend(e, this.config.properties), r.isFunction(this.config.callback) && r.extend(e, this.config.callback("appLaunch")), 
this.taInstance._internalTrack("ta_mp_launch", e));
}
}, {
key: "onAppShow",
value: function(t) {
var e;
this.config.appHide && this.taInstance.timeEvent("ta_mp_hide"), this._setAutoTrackProperties(t), 
this.config.appShow && (e = {}, this.disablePresetList.includes("#url_path") || t && t.path && (e["#url_path"] = this._getPath(t.path)), 
t && (this.disablePresetList.includes("#utm") || t.query && (e["#utm"] = r.getUtmFromQuery(t.query)), 
this.disablePresetList.includes("#start_reason") || (e["#start_reason"] = JSON.stringify(t))), 
r.extend(e, this.config.properties), r.isFunction(this.config.callback) && r.extend(e, this.config.callback("appShow")), 
this.taInstance._internalTrack("ta_mp_show", e));
}
}, {
key: "onAppHide",
value: function() {
var t;
this.config.appHide && (t = {}, this.disablePresetList.includes("#url_path") || (t["#url_path"] = this._getCurrentPath()), 
r.extend(t, this.config.properties), r.isFunction(this.config.callback) && r.extend(t, this.config.callback("appHide")), 
this.taInstance._internalTrack("ta_mp_hide", t), this.taInstance.flush());
}
}, {
key: "_getCurrentPath",
value: function() {
var t = "Not to get";
try {
var e = getCurrentPages();
t = e[e.length - 1].route;
} catch (t) {
_.info(t);
}
return t;
}
}, {
key: "_setAutoTrackProperties",
value: function(t) {
var e = {};
this.disablePresetList.includes("#scene") || (e["#scene"] = t.scene), this.taInstance._setAutoTrackProperties(e);
}
}, {
key: "_getPath",
value: function(t) {
return "string" == typeof t ? t.replace(/^\//, "") : "Abnormal values";
}
}, {
key: "_generateShareInfo",
value: function() {
return JSON.stringify({
distinctId: this.taInstance.getDistinctId()
});
}
}, {
key: "onPageShare",
value: function(t) {
var e = r.isObject(t) ? t : {};
return this.config.pageShare && (t = {}, this.disablePresetList.includes("#url_path") || (t["#url_path"] = this._getCurrentPath()), 
r.extend(t, this.config.properties), r.isFunction(this.config.callback) && r.extend(t, this.config.callback("pageShare")), 
this.taInstance._internalTrack("ta_mp_share", t), !r.isUndefined(e.path) && "" !== e.path || (e.path = this._getCurrentPath()), 
r.isString(e.path) && (-1 === e.path.indexOf("?") ? e.path = e.path + "?" : "&" !== e.path.slice(-1) && (e.path = e.path + "&"), 
e.path = e.path + "tdshare=" + encodeURIComponent(this._generateShareInfo()))), 
e;
}
}, {
key: "onPageShow",
value: function() {
var t, e;
this.config.pageLeave && this.taInstance.timeEvent("ta_page_leave"), this.config.pageShow && (t = this._getCurrentPath(), 
e = {}, this.disablePresetList.includes("#url_path") || (e["#url_path"] = t || "The system did not get a value"), 
this.disablePresetList.includes("#referrer") || (e["#referrer"] = this.referrer), 
r.extend(e, this.config.properties), r.isFunction(this.config.callback) && r.extend(e, this.config.callback("pageShow")), 
this.referrer = t, this.taInstance._internalTrack("ta_mp_view", e));
}
}, {
key: "onPageUnload",
value: function() {
var t, e;
this.config.pageLeave && (t = this._getCurrentPath(), e = {}, this.disablePresetList.includes("#url_path") || (e["#url_path"] = t || "The system did not get a value"), 
r.extend(e, this.config.properties), r.isFunction(this.config.callback) && r.extend(e, this.config.callback("pageLeave")), 
this.taInstance._internalTrack("ta_page_leave", e));
}
}, {
key: "onPageAddToFavorites",
value: function() {
var t, e;
this.config.mpFavorite && (t = this._getCurrentPath(), e = {}, this.disablePresetList.includes("#url_path") || (e["#url_path"] = t || "The system did not get a value"), 
r.extend(e, this.config.properties), r.isFunction(this.config.callback) && r.extend(e, this.config.callback("mpFavorite")), 
this.taInstance._internalTrack("ta_add_favorite", e));
}
} ]), t;
}(), b = function() {
function t(e, i, a) {
var s = this;
n(this, t), this.taInstance = e, this.config = i.autoTrack || {}, this.disablePresetList = i.disablePresetProperties || [];
i = a.getLaunchOptionsSync();
this._onLaunch(i), this._onShow(i), this.startTracked = !0, a.onShow(function(t) {
s._onShow(t);
}), a.onHide(function() {
var t;
s.startTracked = !1, s.config.appHide && (t = {}, r.extend(t, s.config.properties), 
r.isFunction(s.config.callback) && r.extend(t, s.config.callback("appHide")), s.taInstance._internalTrack("ta_mg_hide", t), 
s.taInstance.flush());
});
}
return s(t, [ {
key: "_onLaunch",
value: function(t) {
var e;
this.disablePresetList.includes("#scene") || t && t.scene && this.taInstance._setAutoTrackProperties({
"#scene": t.scene
}), this.config.appLaunch && (e = {}, r.extend(e, this.config.properties), r.isFunction(this.config.callback) && r.extend(e, this.config.callback("appLaunch")), 
this.disablePresetList.includes("#start_reason") || t && (e["#start_reason"] = JSON.stringify(t)), 
this.taInstance._internalTrack("ta_mg_launch", e));
}
}, {
key: "_onShow",
value: function(t) {
var e;
this.startTracked || (this.config.appHide && this.taInstance.timeEvent("ta_mg_hide"), 
this.config.appShow && (e = {}, r.extend(e, this.config.properties), r.isFunction(this.config.callback) && r.extend(e, this.config.callback("appShow")), 
this.disablePresetList.includes("#start_reason") || t && (e["#start_reason"] = JSON.stringify(t)), 
this.taInstance._internalTrack("ta_mg_show", e)));
}
} ]), t;
}(), k = function() {
function e(t, i, a) {
n(this, e), this.api = t, this.config = i, this._config = a;
}
return s(e, [ {
key: "getConfig",
value: function() {
return this.config;
}
}, {
key: "initSdkConfig",
value: function() {}
}, {
key: "getStorage",
value: function(t, e, i) {
if (e) this.api.getStorage({
key: t,
success: function(t) {
t = r.isJSONString(t.data) ? JSON.parse(t.data) : {};
i(t);
},
fail: function() {
_.warn("getStorage faild"), i({});
}
}); else try {
if ("dd_mp" === this._config.platform || "ali_mp" === this._config.platform || "ali_mg" === this._config.platform) {
var n = this.api.getStorageSync({
key: t
});
return r.isJSONString(n.data) ? JSON.parse(n.data) : {};
}
n = this.api.getStorageSync(t);
return r.isJSONString(n) ? JSON.parse(n) : {};
} catch (t) {
return {};
}
}
}, {
key: "setStorage",
value: function(t, e) {
try {
"ali_mp" === this._config.platform || "tb_mp" === this._config.platform || "dd_mp" === this._config.platform || "ali_mg" === this._config.platform ? this.api.setStorageSync({
key: t,
data: e
}) : this.api.setStorageSync(t, e);
} catch (t) {}
}
}, {
key: "removeStorage",
value: function(t) {
try {
r.isFunction(this.api.removeStorage) ? this.api.removeStorage({
key: t
}) : r.isFunction(this.api.deleteStorage) && this.api.deleteStorage({
key: t
});
} catch (t) {}
}
}, {
key: "_getPlatform",
value: function() {
return "";
}
}, {
key: "getSystemInfo",
value: function(t) {
var e = this._config.mpPlatform, i = this;
this.api.getSystemInfo({
success: function(n) {
var a;
r.isFunction(e) ? n.mp_platform = e(n) : n.mp_platform = e, "ali_mp" !== i._config.platform && "ali_mg" !== i._config.platform || (n.system = n.platform + " " + n.system), 
"wechat_mp" === i._config.platform || "wechat_mg" === i._config.platform ? (a = i.api.getAccountInfoSync(), 
n.appVersion = a.miniProgram.version) : "tt_mg" !== i._config.platform && "tt_mg" !== i._config.platform || (n.appVersion = i.api.getEnvInfoSync().microapp.mpVersion), 
t.success(n), "wechat" === e && t.complete();
},
complete: function() {
t.complete();
}
});
}
}, {
key: "getNetworkType",
value: function(t) {
r.isFunction(this.api.getNetworkType) ? this.api.getNetworkType({
success: function(e) {
t.success(e);
},
complete: function() {
t.complete();
}
}) : (t.success({}), t.complete());
}
}, {
key: "onNetworkStatusChange",
value: function(t) {
r.isFunction(this.api.onNetworkStatusChange) ? this.api.onNetworkStatusChange(t) : t({});
}
}, {
key: "request",
value: function(t) {
if ("ali_mp" !== this._config.platform && "dd_mp" !== this._config.platform && "ali_mg" !== this._config.platform) return this.api.request(t);
var e = r.extend({}, t);
return e.headers = t.header, e.header = void 0, e.success = function(e) {
e.statusCode = e.status, t.success(e);
}, e.fail = function(e) {
e.errMsg = e.errorMessage, t.fail(e);
}, "dd_mp" === this._config.platform ? this.api.httpRequest(e) : this.api.request(e);
}
}, {
key: "initAutoTrackInstance",
value: function(t, e) {
return r.isObject(e.autoTrack) && (e.autoTrack.isPlugin = e.is_plugin), new (this._config.mp ? y : b)(t, e, this.api);
}
}, {
key: "setGlobal",
value: function(t, e) {
this._config.mp ? _.warn("ThinkingAnalytics: we do not set global name for TA instance when you do not enable auto track.") : "ali_mg" !== this._config.platform && (GameGlobal[e] = t);
}
}, {
key: "getAppOptions",
value: function(t) {
var e = {};
try {
e = this.api.getLaunchOptionsSync();
} catch (t) {
_.warn("Cannot get launch options.");
}
if (r.isFunction(t)) try {
this._config.mp ? this.api.onAppShow(t) : this.api.onShow(t);
} catch (t) {
_.warn("Cannot register onShow callback.");
}
return e;
}
}, {
key: "showToast",
value: function(t) {
var e;
r.isFunction(this.api.showToast) && (e = {
title: t
}, "dd_mp" !== this._config.platform && "ali_mp" !== this._config.platform || (e.content = t), 
this.api.showToast(e));
}
}, {
key: "setGlobalData",
value: function(e) {
"wechat_mg" === this._config.platform ? GameGlobal && (GameGlobal.tdanalytics2024 = e) : "ali_mp" === this._config.platform ? t.tdanalytics2024 = e : "tt_mp" === this._config.platform || "kuaishou_mp" === this._config.platform ? this.api.tdanalytics2024 = e : globalThis.tdanalytics2024 = e;
}
} ], [ {
key: "createInstance",
value: function() {
return this._createInstance("R_CURRENT_PLATFORM");
}
}, {
key: "_createInstance",
value: function(t) {
switch (t) {
case "wechat_mp":
return new e(wx, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_wechat",
plat: "wx"
}, {
mpPlatform: "wechat",
mp: !0,
platform: t
});

case "wechat_mg":
return new e(wx, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_wechat_game",
plat: "wx"
}, {
mpPlatform: "wechat",
platform: t
});

case "qq_mp":
return new e(qq, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qq"
}, {
mpPlatform: "qq",
mp: !0,
platform: t
});

case "qq_mg":
return new e(qq, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qq_game"
}, {
mpPlatform: "qq",
platform: t
});

case "baidu_mp":
return new e(swan, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_swan"
}, {
mpPlatform: function(t) {
return t.host;
},
mp: !0,
platform: t
});

case "baidu_mg":
return new e(swan, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_swan_game"
}, {
mpPlatform: function(t) {
return t.host;
},
platform: t
});

case "tt_mg":
return new e(tt, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_tt_game"
}, {
mpPlatform: function(t) {
return t.appName;
},
platform: t
});

case "tt_mp":
return new e(tt, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_tt"
}, {
mpPlatform: function(t) {
return t.appName;
},
mp: !0,
platform: t
});

case "ali_mp":
return new e(my, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_ali"
}, {
mpPlatform: function(t) {
return t.app;
},
mp: !0,
platform: t
});

case "ali_mg":
return new e(my, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_ali_game"
}, {
mpPlatform: function(t) {
return t.app;
},
platform: t
});

case "dd_mp":
return new e(dd, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_dd"
}, {
mpPlatform: "dingding",
mp: !0,
platform: t
});

case "bl_mg":
return new e(bl, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_mg"
}, {
mpPlatform: "bilibili",
platform: t
});

case "kuaishou_mp":
return new e(ks, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_kuaishou"
}, {
mpPlatform: "kuaishou",
mp: !0,
platform: t
});

case "kuaishou_mg":
return new e(ks, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_kuaishou"
}, {
mpPlatform: "kuaishou",
platform: t
});

case "qh360_mg":
return new e(qh, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qh360"
}, {
mpPlatform: "qh360",
platform: t
});

case "tb_mp":
return new e(my, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_tb"
}, {
mpPlatform: "tb",
mp: !0,
platform: t
});

case "jd_mp":
return new e(jd, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_jd"
}, {
mpPlatform: "jd",
mp: !0,
platform: t
});

case "qh360_mp":
return new e(qh, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qh360"
}, {
mpPlatform: "qh360",
mp: !0,
platform: t
});

case "WEB":
return new m.createInstance();
}
}
} ]), e;
}(), S = function t(e, i) {
var a = this;
n(this, t), this.taInstance = e, this.config = i || {}, this.config.appShow && this.taInstance._internalTrack("ta_mg_show"), 
this.config.appHide && this.taInstance.timeEvent("ta_mg_hide"), qg.onShow(function() {
var t;
a.config.appHide && a.taInstance.timeEvent("ta_mg_hide"), a.config.appShow && (t = {}, 
r.extend(t, a.config.properties), r.isFunction(a.config.callback) && r.extend(t, a.config.callback("appShow")), 
a.taInstance._internalTrack("ta_mg_show"));
}), qg.onHide(function() {
var t;
a.config.appHide && (t = {}, r.extend(t, a.config.properties), r.isFunction(a.config.callback) && r.extend(t, a.config.callback("appHide")), 
a.taInstance._internalTrack("ta_mg_hide"));
});
}, C = function() {
function t() {
n(this, t), this.config = {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qg_vivo_game",
asyncPersistence: !0
};
}
return s(t, [ {
key: "getConfig",
value: function() {
return this.config || {};
}
}, {
key: "initSdkConfig",
value: function() {}
}, {
key: "getStorage",
value: function(t, e, i) {
if (!e) {
e = qg.getStorageSync({
key: t
});
return r.isJSONString(e) ? JSON.parse(e) : {};
}
qg.getStorage({
key: t,
success: function(t) {
t = r.isJSONString(t) ? JSON.parse(t) : {};
i(t);
},
fail: function() {
i({});
}
});
}
}, {
key: "setStorage",
value: function(t, e) {
qg.setStorage({
key: t,
value: e
});
}
}, {
key: "removeStorage",
value: function(t) {
qg.deleteStorage({
key: t
});
}
}, {
key: "getSystemInfo",
value: function(t) {
qg.getSystemInfo({
success: function(e) {
var i = e, n = [ e.osType, e.osVersionName ].join(" ");
i.brand = e.manufacturer, i.system = n, i.mp_platform = "vivo_qg", t.success(i);
},
complete: function() {
t.complete();
}
});
}
}, {
key: "getNetworkType",
value: function(t) {
qg.getNetworkType({
success: function(e) {
var i = e;
i.networkType = e.type, t.success(i);
},
complete: function() {
t.complete();
}
});
}
}, {
key: "onNetworkStatusChange",
value: function(t) {
qg.subscribeNetworkStatus({
callback: function(e) {
var i = e;
i.networkType = e.type, t(i);
}
});
}
}, {
key: "request",
value: function(t) {
return qg.request({
url: t.url,
data: t.data,
method: t.method,
header: t.header,
success: function(e) {
t.success(e);
},
fail: function(e) {
t.fail(e);
}
});
}
}, {
key: "initAutoTrackInstance",
value: function(t, e) {
return new S(t, e.autoTrack);
}
}, {
key: "setGlobal",
value: function(t, e) {
globalThis[e] = t;
}
}, {
key: "getAppOptions",
value: function() {
return {};
}
}, {
key: "showToast",
value: function(t) {
qg.showToast({
message: t,
duration: 0
});
}
}, {
key: "setGlobalData",
value: function() {}
} ], [ {
key: "createInstance",
value: function() {
return new t();
}
} ]), t;
}(), I = function t(e, i, a) {
var s = this;
n(this, t), this.taInstance = e, this.config = i || {}, this.config.appShow && (i = {}, 
r.extend(i, this.config.properties), r.isFunction(this.config.callback) && r.extend(i, this.config.callback("appShow")), 
this.taInstance._internalTrack("ta_mg_show", i)), this.config.appHide && this.taInstance.timeEvent("ta_mg_hide"), 
a.onShow(function() {
var t;
s.config.appHide && s.taInstance.timeEvent("ta_mg_hide"), s.config.appShow && (t = {}, 
r.extend(t, s.config.properties), r.isFunction(s.config.callback) && r.extend(t, s.config.callback("appShow")), 
s.taInstance._internalTrack("ta_mg_show", t));
}), a.onHide(function() {
var t;
s.config.appHide && (t = {}, r.extend(t, s.config.properties), r.isFunction(s.config.callback) && r.extend(t, s.config.callback("appHide")), 
s.taInstance._internalTrack("ta_mg_hide", t));
});
}, A = function() {
function t(e, i, a) {
n(this, t), this.api = e, this.config = i, this._config = a;
}
return s(t, [ {
key: "getConfig",
value: function() {
return this.config || {};
}
}, {
key: "initSdkConfig",
value: function() {}
}, {
key: "getStorage",
value: function(t, e, i) {
t = localStorage.getItem(t);
if (!e) return r.isJSONString(t) ? JSON.parse(t) : {};
r.isJSONString(t) ? i(JSON.parse(t)) : i({});
}
}, {
key: "setStorage",
value: function(t, e) {
localStorage.setItem(t, e);
}
}, {
key: "removeStorage",
value: function(t) {
localStorage.removeItem(t);
}
}, {
key: "getSystemInfo",
value: function(t) {
var e = this._config.mpPlatform;
this.api.getSystemInfo({
success: function(i) {
i.mp_platform = e, t.success(i);
},
complete: function() {
t.complete();
}
});
}
}, {
key: "getNetworkType",
value: function(t) {
this.api.getNetworkType({
success: function(e) {
t.success(e);
},
complete: function() {
t.complete();
}
});
}
}, {
key: "onNetworkStatusChange",
value: function(t) {
this.api.onNetworkStatusChange({
callback: function(e) {
t(e);
}
});
}
}, {
key: "request",
value: function(t) {
var e = {}, i = new XMLHttpRequest();
if (i.open(t.method, t.url), t.header) for (var n in t.header) i.setRequestHeader(n, t.header[n]);
return i.onreadystatechange = function() {
4 === i.readyState && 200 === i.status ? (e.statusCode = 200, r.isJSONString(i.responseText) && (e.data = JSON.parse(i.responseText)), 
t.success(e)) : 200 !== i.status && (e.errMsg = "network error", t.fail(e));
}, i.ontimeout = function() {
e.errMsg = "timeout", t.fail(e);
}, i.send(t.data), i;
}
}, {
key: "initAutoTrackInstance",
value: function(t, e) {
return new I(t, e.autoTrack, this.api);
}
}, {
key: "setGlobal",
value: function(t, e) {
globalThis[e] = t;
}
}, {
key: "getAppOptions",
value: function() {
return this.api.getLaunchOptionsSync();
}
}, {
key: "showToast",
value: function(t) {
this.api.showToast({
title: t,
icon: "none",
duration: 2e3
});
}
}, {
key: "setGlobalData",
value: function() {}
} ], [ {
key: "createInstance",
value: function() {
return this._createInstance("R_CURRENT_PLATFORM");
}
}, {
key: "_createInstance",
value: function(e) {
switch (e) {
case "oppo":
return new t(qg, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qg_oppo_game"
}, {
mpPlatform: "oppo_qg"
});

case "huawei":
return new t(hbs, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qg_huawei_game"
}, {
mpPlatform: "huawei_qg"
});

case "mz":
return new t(qg, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qg_mz_game"
}, {
mpPlatform: "mz"
});

case "xiaomi":
return new t(qg, {
persistenceName: "thinkingdata",
persistenceNameOld: "thinkingdata_qg"
}, {
mpPlatform: "xiaomi"
});
}
}
} ]), t;
}(), P = function() {
function t() {
n(this, t);
}
return s(t, null, [ {
key: "createInstance",
value: function() {
var t = Object.freeze({
WECHAT_GAME: 104,
QQ_PLAY: 105,
BAIDU_GAME: 107,
VIVO_GAME: 108,
OPPO_GAME: 109,
HUAWEI_GAME: 110,
XIAOMI_GAME: 111,
BYTEDANCE_GAME: 117,
QTT_GAME: 116,
LINKSURE: 119,
ALI_GAME: 113,
WECHAT_MINI_GAME: "WECHAT_GAME",
BAIDU_MINI_GAME: "BAIDU_MINI_GAME",
XIAOMI_QUICK_GAME: "XIAOMI_QUICK_GAME",
OPPO_MINI_GAME: "OPPO_MINI_GAME",
VIVO_MINI_GAME: "VIVO_MINI_GAME",
HUAWEI_QUICK_GAME: "HUAWEI_QUICK_GAME",
BYTEDANCE_MINI_GAME: "BYTEDANCE_MINI_GAME",
QTT_MINI_GAME: "QTT_MINI_GAME",
LINKSURE_MINI_GAME: "LINKSURE_MINI_GAME",
ALIPAY_MINI_GAME: "ALIPAY_MINI_GAME"
});
if (cc.sys.platform === t.WECHAT_GAME || cc.sys.platform === t.WECHAT_MINI_GAME) return k._createInstance("wechat_mg");
if (cc.sys.platform === t.BAIDU_GAME || cc.sys.platform === t.BAIDU_MIN_GAME) return k._createInstance("baidu_mg");
if (cc.sys.platform === t.VIVO_GAME || cc.sys.platform === t.VIVO_MINI_GAME) return C.createInstance();
if (cc.sys.platform === t.QQ_PLAY) return k._createInstance("qq_mg");
if (cc.sys.platform === t.OPPO_GAME || cc.sys.platform === t.OPPO_MINI_GAME) return A._createInstance("oppo");
if (cc.sys.platform === t.HUAWEI_GAME || cc.sys.platform === t.HUAWEI_QUICK_GAME) return A._createInstance("huawei");
if (cc.sys.platform === t.XIAOMI_GAME || cc.sys.platform === t.XIAOMI_QUICK_GAME) return A._createInstance("xiaomi");
if (cc.sys.platform === t.BYTEDANCE_GAME || cc.sys.platform === t.BYTEDANCE_MINI_GAME) return k._createInstance("tt_mg");
if (cc.sys.platform === t.ALI_GAME || cc.sys.platform === t.ALIPAY_MINI_GAME) return k._createInstance("ali_mg");
var e = m.createInstance();
return e._sysCallback = function() {
return {
system: cc.sys.os.replace(" ", "") + " " + cc.sys.osVersion
};
}, e.getNetworkType = function(t) {
var e = {};
switch (cc.sys.getNetworkType()) {
case cc.sys.NetworkType.LAN:
e.networkType = "WIFI";
break;

case cc.sys.NetworkType.WWAN:
e.networkType = "WWAN";
break;

default:
e.networkType = "NONE";
}
t.success(e), t.complete();
}, e.getSystemInfo = function(t) {
var i = {
mp_platform: cc.sys.platform.toString(),
system: e._getOs(),
screenWidth: window.screen.width,
screenHeight: window.screen.height
};
e._sysCallback && (i = r.extend(i, e._sysCallback(t))), t.success(i), t.complete();
}, e;
}
} ]), t;
}(), O = function() {
function t() {
n(this, t);
}
return s(t, null, [ {
key: "_getCurrentPlatform",
value: function() {
return this.currentPlatform || (this.currentPlatform = P.createInstance());
}
}, {
key: "getConfig",
value: function() {
return this._getCurrentPlatform().getConfig();
}
}, {
key: "initConfig",
value: function(t) {
this._getCurrentPlatform().initSdkConfig(t);
}
}, {
key: "isWxPlat",
value: function() {
return "wx" === this.getConfig().plat;
}
}, {
key: "getStorage",
value: function(t, e, i) {
return this._getCurrentPlatform().getStorage(t, e, i);
}
}, {
key: "setStorage",
value: function(t, e) {
return this._getCurrentPlatform().setStorage(t, e);
}
}, {
key: "removeStorage",
value: function(t) {
return this._getCurrentPlatform().removeStorage(t);
}
}, {
key: "getSystemInfo",
value: function(t) {
return this._getCurrentPlatform().getSystemInfo(t);
}
}, {
key: "getNetworkType",
value: function(t) {
return this._getCurrentPlatform().getNetworkType(t);
}
}, {
key: "onNetworkStatusChange",
value: function(t) {
this._getCurrentPlatform().onNetworkStatusChange(t);
}
}, {
key: "request",
value: function(t) {
return this._getCurrentPlatform().request(t);
}
}, {
key: "initAutoTrackInstance",
value: function(t, e) {
return this._getCurrentPlatform().initAutoTrackInstance(t, e);
}
}, {
key: "setGlobal",
value: function(t, e) {
t && e && this._getCurrentPlatform().setGlobal(t, e);
}
}, {
key: "getAppOptions",
value: function(t) {
return this._getCurrentPlatform().getAppOptions(t);
}
}, {
key: "showDebugToast",
value: function(t) {
this._getCurrentPlatform().showToast(t);
}
}, {
key: "setGlobalData",
value: function(t) {
this._getCurrentPlatform().setGlobalData(t);
}
} ]), t;
}(), x = /^[a-zA-Z][a-zA-Z0-9_]{0,49}$/, w = function() {
function t() {
n(this, t);
}
return s(t, null, [ {
key: "stripProperties",
value: function(t) {
return r.isObject(t) && r.each(t, function(t, e) {
r.isString(t) || r.isNumber(t) || r.isDate(t) || r.isBoolean(t) || r.isArray(t) || r.isObject(t) || _.warn("Your data -", e, t, "- format does not meet requirements and may not be stored correctly. Attribute values only support String, Number, Date, Boolean, Array, Object");
}), t;
}
}, {
key: "_checkPropertiesKey",
value: function(t) {
var e = !0;
return r.each(t, function(t, i) {
x.test(i) || (_.warn("Invalid KEY: " + i), e = !1);
}), e;
}
}, {
key: "event",
value: function(t) {
return !(!r.isString(t) || !x.test(t)) || (_.warn("Check the parameter format. The eventName must start with an English letter and contain no more than 50 characters including letters, digits, and underscores: " + t), 
!1);
}
}, {
key: "propertyName",
value: function(t) {
return !(!r.isString(t) || !x.test(t)) || (_.warn("Check the parameter format. PropertyName must start with a letter and contain letters, digits, and underscores (_). The value is a string of no more than 50 characters: " + t), 
!1);
}
}, {
key: "properties",
value: function(t) {
return this.stripProperties(t), !(t && (r.isObject(t) ? !this._checkPropertiesKey(t) && (_.warn("Check the parameter format. The properties key must start with a letter, contain digits, letters, and underscores (_), and contain a maximum of 50 characters"), 
1) : (_.warn("properties can be none, but it must be an object"), 1)));
}
}, {
key: "propertiesMust",
value: function(t) {
return this.stripProperties(t), void 0 === t || !r.isObject(t) || r.isEmptyObject(t) ? (_.warn("properties must be an object with a value"), 
!1) : !!this._checkPropertiesKey(t) || (_.warn("Check the parameter format. The properties key must start with a letter, contain digits, letters, and underscores (_), and contain a maximum of 50 characters"), 
!1);
}
}, {
key: "userId",
value: function(t) {
return !(!r.isString(t) || !/^.{1,64}$/.test(t)) || (_.warn("The user ID must be a string of less than 64 characters and cannot be null"), 
!1);
}
}, {
key: "userAddProperties",
value: function(t) {
if (!this.propertiesMust(t)) return !1;
for (var e in t) if (!r.isNumber(t[e])) return _.warn("The attributes of userAdd need to be Number"), 
!1;
return !0;
}
}, {
key: "userAppendProperties",
value: function(t) {
if (!this.propertiesMust(t)) return !1;
for (var e in t) if (!r.isArray(t[e])) return _.warn("The attribute of userAppend must be Array"), 
!1;
return !0;
}
} ]), t;
}(), N = function() {
function t(e, i, a, s, o) {
n(this, t), this.data = e, this.serverUrl = i, this.callback = o, this.tryCount = r.isNumber(a) ? a : 1, 
this.timeout = r.isNumber(s) ? s : 3e3, this.taClassName = "HttpTask";
}
return s(t, [ {
key: "run",
value: function() {
var t = this, e = r.createExtraHeaders();
e["content-type"] = "application/json", this.runTime = r.getCurrentTimeStamp(), 
O.request({
url: this.serverUrl,
method: "POST",
data: this.data,
header: e,
success: function(e) {
t.onSuccess(e);
},
fail: function(e) {
t.onFailed(e);
}
});
}
}, {
key: "onSuccess",
value: function(t) {
if (!this.sendTimeout()) if (r.isObject(t) && 200 === t.statusCode) {
var e;
switch ((r.isUndefined(t.data) || r.isUndefined(t.data.code)) && (t.data = {
code: 0
}), t.data.code) {
case 0:
e = "success";
break;

case -1:
e = "invalid data";
break;

case -2:
e = "invalid APP ID";
break;

default:
e = "Unknown return code";
}
this.callback({
code: t.data.code,
msg: e
});
} else this.callback({
code: -3,
msg: r.isObject(t) ? t.statusCode : "Unknown error"
});
}
}, {
key: "onFailed",
value: function(t) {
this.sendTimeout() || (0 < --this.tryCount ? this.run() : this.callback({
code: -3,
msg: r.isObject(t) ? t.errMsg : "Unknown error"
}));
}
}, {
key: "sendTimeout",
value: function() {
return r.getCurrentTimeStamp() - this.runTime > this.timeout;
}
} ]), t;
}(), T = function() {
function t(e, i, a, s, o, c, l) {
n(this, t), this.data = e, this.serverDebugUrl = i, this.callback = l, this.tryCount = r.isNumber(a) ? a : 1, 
this.timeout = r.isNumber(s) ? s : 3e3, this.dryrun = o, this.deviceId = c, this.taClassName = "HttpTaskDebug";
}
return s(t, [ {
key: "run",
value: function() {
var t = this, e = "appid=" + this.data["#app_id"] + "&source=client&dryRun=" + this.dryrun + "&deviceId=" + this.deviceId + "&data=" + encodeURIComponent(JSON.stringify(this.data.data[0])), i = r.createExtraHeaders();
i["content-type"] = "application/x-www-form-urlencoded";
var n = O.request({
url: this.serverDebugUrl,
method: "POST",
data: e,
header: i,
success: function(e) {
t.onSuccess(e), clearTimeout(a);
},
fail: function(e) {
t.onFailed(e), clearTimeout(a);
}
}), a = setTimeout(function() {
(r.isObject(n) || r.isPromise(n)) && r.isFunction(n.abort) && n.abort();
}, this.timeout);
}
}, {
key: "onSuccess",
value: function(t) {
if (r.isObject(t) && 200 === t.statusCode) {
var e;
if ((r.isUndefined(t.data) || r.isUndefined(t.data.errorLevel)) && (t.data = {
errorLevel: 0
}), 0 === t.data.errorLevel) e = "Verify data success."; else if (1 === t.data.errorLevel) {
for (var i = t.data.errorProperties, n = "", a = 0; a < i.length; a++) {
var s = i[a].errorReason;
n = n + " propertyName:" + i[a].propertyName + " errorReasons:" + s + "\n";
}
e = "Debug data error. errorLevel:" + t.data.errorLevel + " reason:" + n;
} else 2 !== t.data.errorLevel && -1 !== t.data.errorLevel || (e = "Debug data error. errorLevel:" + t.data.errorLevel + " reason:" + t.data.errorReasons);
_.info(e), this.callback({
code: t.data.errorLevel,
msg: e
});
} else this.callback({
code: -3,
msg: r.isObject(t) ? t.statusCode : "Unknown error"
});
}
}, {
key: "onFailed",
value: function(t) {
0 < --this.tryCount ? this.run() : this.callback({
code: -3,
msg: r.isObject(t) ? t.errMsg : "Unknown error"
});
}
} ]), t;
}(), M = new (function() {
function t() {
n(this, t), this.items = [], this.isRunning = !1, this.showDebug = !1;
}
return s(t, [ {
key: "enqueue",
value: function(t, e, i, n) {
var a = this, s = (n = !(3 < arguments.length && void 0 !== n) || n, this);
e = "debug" === i.debugMode ? new T(t, e, i.maxRetries, i.sendTimeout, 0, i.deviceId, function(t) {
s.isRunning = !1, delete a.runTime, r.isFunction(i.callback) && i.callback(t), s._runNext(), 
!1 === s.showDebug && (0 !== t.code && 1 !== t.code && 2 !== t.code || (s.showDebug = !0, 
r.isFunction(O.showDebugToast) && O.showDebugToast("The current mode is Debug")));
}) : "debugOnly" === i.debugMode ? new T(t, e, i.maxRetries, i.sendTimeout, 1, i.deviceId, function(t) {
s.isRunning = !1, delete a.runTime, r.isFunction(i.callback) && i.callback(t), s._runNext(), 
!1 === s.showDebug && (0 !== t.code && 1 !== t.code && 2 !== t.code || (s.showDebug = !0, 
r.isFunction(O.showDebugToast) && O.showDebugToast("The current mode is debugOnly")));
}) : new N(JSON.stringify(t), e, i.maxRetries, i.sendTimeout, function(t) {
s.isRunning = !1, delete a.runTime, r.isFunction(i.callback) && i.callback(t), s._runNext();
});
!0 === n ? (this.items.push(e), this._runNext()) : e.run();
}
}, {
key: "_dequeue",
value: function() {
return this.items.shift();
}
}, {
key: "_runNext",
value: function() {
if (0 < this.items.length && !this.isRunning) if (this.isRunning = !0, this.runTime = r.getCurrentDate(), 
"HttpTask" !== this.items[0].taClassName) this._dequeue().run(); else {
var t = this.items.splice(0, this.items.length), e = t[0], i = JSON.parse(e.data), n = i["#app_id"], a = [];
a.push(e.callback);
for (var s = 1; s < t.length; s++) {
var o = t[s], c = JSON.parse(o.data);
c["#app_id"] === n && e.serverUrl === o.serverUrl ? (i.data = i.data.concat(c.data), 
a.push(o.callback)) : this.items.push(o);
}
var l = r.getCurrentTimeStamp();
i["#flush_time"] = l, new N(JSON.stringify(i), e.serverUrl, e.tryCount, e.timeout, function(t) {
for (var e in a) Object.hasOwnProperty.call(a, e) && (0, a[e])(t);
}).run();
}
}
}, {
key: "runTimeout",
value: function(t) {
return !!(r.isDate(this.runTime) && r.getCurrentDate().getTime() - this.runTime.getTime() > t);
}
}, {
key: "resetTimeout",
value: function() {
this.isRunning = !1, delete this.runTime;
}
} ]), t;
}())(), L = {
name: "thinkingdata",
is_plugin: !1,
maxRetries: 3,
sendTimeout: 3e3,
enablePersistence: !0,
asyncPersistence: !1,
enableLog: !0,
strict: !1,
debugMode: "none",
enableCalibrationTime: !1,
enableBatch: !1,
disablePresetProperties: [],
cloudEnv: "online",
reportingToTencentSdk: 3
}, D = {
properties: {},
disableList: [],
initDisableList: function(t) {
this.disableList = t, this.disableList.includes("#lib") || (this.properties["#lib"] = "MG"), 
this.disableList.includes("#lib_version") || (this.properties["#lib_version"] = "3.3.0-beta.2");
},
initDeviceId: function(t) {
r.isString(t) && (this.disableList.includes("#device_id") || (this.properties["#device_id"] = t));
},
getSystemInfo: function(t) {
var e = this;
O.onNetworkStatusChange(function(t) {
e.disableList.includes("#network_type") || (e.properties["#network_type"] = t.networkType);
}), O.getNetworkType({
success: function(t) {
e.disableList.includes("#network_type") || (e.properties["#network_type"] = t.networkType);
},
complete: function() {
O.getSystemInfo({
success: function(t) {
var i = t.system ? t.system.replace(/\s+/g, " ").split(" ") : [], n = {};
e.disableList.includes("#manufacturer") || (n["#manufacturer"] = t.brand), e.disableList.includes("#device_model") || (n["#device_model"] = t.model), 
e.disableList.includes("#screen_width") || (n["#screen_width"] = Number(t.screenWidth)), 
e.disableList.includes("#screen_height") || (n["#screen_height"] = Number(t.screenHeight)), 
e.disableList.includes("#os") || (n["#os"] = i[0]), e.disableList.includes("#os_version") || (n["#os_version"] = i[1]), 
e.disableList.includes("#mp_platform") || (n["#mp_platform"] = t.mp_platform), e.disableList.includes("#system_language") || (n["#system_language"] = t.systemLanguage), 
e.disableList.includes("#app_version") || (n["#app_version"] = t.appVersion), r.extend(e.properties, n), 
r.setMpPlatform(t.mp_platform);
},
complete: function() {
t();
}
});
}
});
}
}, F = function() {
function t(e, i) {
var a = this;
n(this, t), this.enabled = e.enablePersistence, this.enabled ? (e.isChildInstance ? (this.name = e.persistenceName + "_" + e.name, 
this.nameOld = e.persistenceNameOld + "_" + e.name) : (this.name = e.persistenceName, 
this.nameOld = e.persistenceNameOld), e.asyncPersistence ? (this._state = {}, O.getStorage(this.name, !0, function(t) {
r.isEmptyObject(t) ? O.getStorage(a.nameOld, !0, function(t) {
a._state = r.extend2Layers({}, t, a._state), a._init(e, i), a._save();
}) : (a._state = r.extend2Layers({}, t, a._state), a._init(e, i), a._save());
})) : (this._state = O.getStorage(this.name) || {}, r.isEmptyObject(this._state) && (this._state = O.getStorage(this.nameOld) || {}), 
this._init(e, i))) : (this._state = {}, this._init(e, i));
}
return s(t, [ {
key: "_init",
value: function(t, e) {
this.getDistinctId() || this.setDistinctId(r.UUID()), t.isChildInstance || (this.getDeviceId() || this._setDeviceId(r.UUID()), 
D.initDeviceId(this.getDeviceId())), this.initComplete = !0, "function" == typeof e && e(), 
this._save();
}
}, {
key: "_save",
value: function() {
this.enabled && this.initComplete && O.setStorage(this.name, JSON.stringify(this._state));
}
}, {
key: "_set",
value: function(t, e) {
var n, a = this;
"string" == typeof t ? (n = {})[t] = e : "object" === i(t) && (n = t), r.each(n, function(t, e) {
a._state[e] = t;
}), this._save();
}
}, {
key: "_get",
value: function(t) {
return this._state[t];
}
}, {
key: "setEventTimer",
value: function(t, e) {
var i = this._state.event_timers || {};
i[t] = e, this._set("event_timers", i);
}
}, {
key: "removeEventTimer",
value: function(t) {
var e = (this._state.event_timers || {})[t];
return r.isUndefined(e) || (delete this._state.event_timers[t], this._save()), e;
}
}, {
key: "getDeviceId",
value: function() {
return this._state.device_id;
}
}, {
key: "_setDeviceId",
value: function(t) {
this.getDeviceId() ? _.warn("cannot modify the device id.") : this._set("device_id", t);
}
}, {
key: "getDistinctId",
value: function() {
return this._state.distinct_id;
}
}, {
key: "setDistinctId",
value: function(t) {
this._set("distinct_id", t);
}
}, {
key: "getAccountId",
value: function() {
return this._state.account_id;
}
}, {
key: "setAccountId",
value: function(t) {
this._set("account_id", t);
}
}, {
key: "getSuperProperties",
value: function() {
return this._state.props || {};
}
}, {
key: "setSuperProperties",
value: function(t, e) {
t = e ? t : r.extend(this.getSuperProperties(), t);
this._set("props", t);
}
} ]), t;
}(), j = "ta_mpsdk_", E = "tab_tampsdk_", U = function() {
function t(e, i) {
n(this, t), this.config = e, this.ta = i, this.timer = null, this.batchConfig = r.extend({
size: 6,
interval: 6e3,
maxLimit: 500
}, this.config.batchConfig), this.batchConfig.size < 1 && (this.batchConfig.size = 1), 
30 < this.batchConfig.size && (this.batchConfig.size = 30), this.storageKey = j + this.config.appId, 
this.maxLimit = this.batchConfig.maxLimit, this.batchList = [];
i = O.getStorage(this.storageKey);
r.isArray(i) && (this.batchList = i);
i = E + this.config.appId;
var a = O.getStorage(i);
if (r.isArray(a)) {
for (var s = 0; s < a.length; s++) {
var o = O.getStorage(a[s]);
this.batchList.push(o), O.removeStorage(a[s]);
}
O.removeStorage(i);
}
this.dataHasChange = !1, this.dataSendTimeStamp = 0;
}
return s(t, [ {
key: "batchInterval",
value: function() {
this.loopWrite(), this.loopSend();
}
}, {
key: "loopWrite",
value: function() {
var t = this;
setTimeout(function() {
t.batchWrite(), t.loopWrite();
}, 500);
}
}, {
key: "batchWrite",
value: function() {
this.dataHasChange && (this.dataHasChange = !1, O.setStorage(this.storageKey, JSON.stringify(this.batchList)));
}
}, {
key: "loopSend",
value: function() {
var t = this;
t.timer = setTimeout(function() {
t.batchSend(), clearTimeout(t.timer), t.loopSend();
}, this.batchConfig.interval);
}
}, {
key: "add",
value: function(t) {
this.batchList.length > this.maxLimit && this.batchList.shift(), this.batchList.push(t), 
this.dataHasChange = !0, this.batchList.length > this.batchConfig.size && this.batchSend();
}
}, {
key: "flush",
value: function() {
clearTimeout(this.timer), this.batchSend(), this.loopSend();
}
}, {
key: "batchSend",
value: function() {
var t, e, i, n = r.getCurrentTimeStamp();
0 !== this.dataSendTimeStamp && n - this.dataSendTimeStamp < this.config.sendTimeout + 500 || (this.dataSendTimeStamp = r.getCurrentTimeStamp(), 
0 < (t = (n = 30 < this.batchList.length ? this.batchList.slice(0, 30) : this.batchList).length) && ((e = {}).data = n, 
e["#app_id"] = this.config.appId, e["#flush_time"] = r.getCurrentTimeStamp(), i = this, 
M.enqueue(e, this.ta.serverUrl, {
maxRetries: 1,
sendTimeout: this.config.sendTimeout,
callback: function(n) {
0 === n.code && (_.info("Flush success: " + JSON.stringify(e, null, 4)), i.batchRemove(t));
},
debugMode: this.config.debugMode,
deviceId: this.ta.getDeviceId()
}, !1)));
}
}, {
key: "batchRemove",
value: function(t) {
this.dataSendTimeStamp = 0, this.batchList.splice(0, t), this.dataHasChange = !0, 
this.batchWrite();
}
} ]), t;
}(), R = function() {
function t(e) {
if (n(this, t), e) {
if (!O.isWxPlat() || 1 !== e.reportingToTencentSdk && 2 !== e.reportingToTencentSdk || (i = e.tgaInitParams.tgaSDK, 
e.tgaInitParams && i && ("debug" !== e.debugMode && "debugOnly" !== e.debugMode || i.setDebug(!0), 
this.wxSdk = new i({
user_action_set_id: e.tgaInitParams.user_action_set_id,
secret_key: e.tgaInitParams.secret_key,
appid: e.tgaInitParams.appid
}), e.tgaInitParams.openId ? this.wxSdk.setOpenId(e.tgaInitParams.openId) : e.tgaInitParams.unionId && this.wxSdk.setUnionId(e.tgaInitParams.unionId))), 
this.isTADisable = 1 === e.reportingToTencentSdk, e.appId = e.appId ? r.checkAppId(e.appId) : r.checkAppId(e.appid), 
e.serverUrl = e.serverUrl ? r.checkUrl(e.serverUrl) : r.checkUrl(e.server_url), 
!e.appId || !e.serverUrl) throw new Error("appId or serverUrl can not be empty");
var i = r.extend({}, L, O.getConfig());
r.isObject(e) ? this.config = r.extend(i, e) : this.config = i, this._init(this.config);
}
}
return s(t, [ {
key: "_init",
value: function(t) {
var e = this;
this.name = t.name, this.appId = t.appId || t.appid;
var i = t.serverUrl || t.server_url;
this.serverUrl = i + "/sync_xcx", this.serverDebugUrl = i + "/data_debug", this.configUrl = i + "/config", 
this.autoTrackProperties = {}, O.initConfig(t), this._queue = [], this.observers = [], 
this.updateConfig(this.configUrl, this.appId), t.isChildInstance ? this._state = {} : (_.enabled = t.enableLog, 
this.instances = [], this._state = {
getSystemInfo: !1,
initComplete: !1
}, O.setGlobal(this, this.name)), D.initDisableList(this.config.disablePresetProperties), 
this.store = new F(t, function() {
e.config.asyncPersistence && r.isFunction(e.config.persistenceComplete) && e.config.persistenceComplete(e), 
e._updateState();
}), this.enabled = !r.isBoolean(this.store._get("ta_enabled")) || this.store._get("ta_enabled"), 
this.isOptOut = !!r.isBoolean(this.store._get("ta_isOptOut")) && this.store._get("ta_isOptOut"), 
!t.isChildInstance && t.autoTrack && (this.autoTrack = O.initAutoTrackInstance(this, t)), 
void 0 !== this.config.enableBatch && !1 !== this.config.enableBatch && (this.batchConsumer = new U(this.config, this), 
this.batchConsumer.batchInterval());
}
}, {
key: "initSystemInfo",
value: function() {
var t = this;
this.config.isChildInstance || D.getSystemInfo(function() {
t._updateState({
getSystemInfo: !0
});
});
}
}, {
key: "updateConfig",
value: function(t, e) {
var i = this, n = r.createExtraHeaders();
n["content-type"] = "application/json";
var a = O.request({
url: t + "?appid=" + e,
method: "GET",
header: n,
success: function(t) {
r.isUndefined(t) || r.isUndefined(t.data) || (_.info("Get remote config success(" + e + ") :" + JSON.stringify(t.data)), 
r.isUndefined(t.data.data) || (i.config.syncBatchSize = t.data.data.sync_batch_size, 
i.config.syncInterval = t.data.data.sync_interval, i.config.disableEventList = t.data.data.disable_event_list, 
r.isUndefined(t.data.data.secret_key) || (t = t.data.data.secret_key, i.config.secretKey = {
publicKey: t.key,
version: t.version
})));
},
fail: function(t) {
_.info("Get remote config fail(" + e + ") :" + t.errMsg);
}
});
setTimeout(function() {
(r.isObject(a) || r.isPromise(a)) && r.isFunction(a.abort) && a.abort();
}, 3e3);
}
}, {
key: "initInstance",
value: function(e, i) {
if (!O.isWxPlat() || !this.isTADisable) if (this.config.isChildInstance) _.warn("initInstance() cannot be called on child instance"); else {
if (r.isString(e) && e !== this.name && r.isUndefined(this[e])) {
i = new t(r.extend({}, this.config, {
enablePersistence: !1,
isChildInstance: !0,
name: e
}, i));
return this[e] = i, this.instances.push(e), this[e]._state = this._state, i;
}
_.warn("initInstance() failed due to the name is invalid: " + e);
}
}
}, {
key: "lightInstance",
value: function(t) {
if (!O.isWxPlat() || !this.isTADisable) return this[t];
}
}, {
key: "_setAutoTrackProperties",
value: function(t) {
r.extend(this.autoTrackProperties, t);
}
}, {
key: "init",
value: function() {
if (!O.isWxPlat() || !this.isTADisable) {
if (this.initSystemInfo(), this._state.initComplete) return !1;
this._updateState({
initComplete: !0
}), _.info("TDAnalytics SDK initialize success, AppId = " + this.config.appId + ", ServerUrl = " + this.config.serverUrl + ", Mode = " + this.config.model + ", DeviceId = " + this.getDeviceId() + ", Lib = MG, LibVersion = 3.3.0-beta.2");
}
}
}, {
key: "_isReady",
value: function() {
return this._state.getSystemInfo && this._state.initComplete && this.store.initComplete;
}
}, {
key: "_updateState",
value: function(t) {
var e = this;
r.isObject(t) && r.extend(this._state, t), this._onStateChange(), r.each(this.instances, function(t) {
e[t]._onStateChange();
});
}
}, {
key: "_onStateChange",
value: function() {
var t = this;
this._isReady() && this._queue && 0 < this._queue.length && (r.each(this._queue, function(e) {
t[e[0]].apply(t, l.call(e[1]));
}), this._queue = []);
}
}, {
key: "_hasDisabled",
value: function() {
var t = !this.enabled || this.isOptOut;
return t && _.info("SDK is Pause or Stop!"), t;
}
}, {
key: "_sendRequest",
value: function(t, e, i) {
if (!this._hasDisabled()) if (r.isUndefined(this.config.disableEventList) || !this.config.disableEventList.includes(t.eventName)) {
e = r.isDate(e) ? e : r.getCurrentDate();
var n = {
data: [ {
"#type": t.type,
"#time": r.formatDate(r.formatTimeZone(e, this.config.zoneOffset)),
"#distinct_id": this.store.getDistinctId()
} ]
};
if (this.store.getAccountId() && (n.data[0]["#account_id"] = this.store.getAccountId()), 
"track" === t.type || "track_update" === t.type || "track_overwrite" === t.type ? (n.data[0]["#event_name"] = t.eventName, 
"track_update" === t.type || "track_overwrite" === t.type ? n.data[0]["#event_id"] = t.extraId : t.firstCheckId && (n.data[0]["#first_check_id"] = t.firstCheckId), 
n.data[0].properties = r.extend({
"#zone_offset": r.getTimeZone(e, this.config.zoneOffset)
}, D.properties, this.autoTrackProperties, this.store.getSuperProperties(), this.dynamicProperties ? this.dynamicProperties() : {}), 
e = this.store.removeEventTimer(t.eventName), r.isUndefined(e) || (a = r.getCurrentTimeStamp() - e, 
86400 < (a = parseFloat((a / 1e3).toFixed(3))) ? a = 86400 : a < 0 && (a = 0), n.data[0].properties["#duration"] = a)) : n.data[0].properties = {}, 
r.isObject(t.properties) && !r.isEmptyObject(t.properties) && r.extend(n.data[0].properties, t.properties), 
r.searchObjDate(n.data[0], this.config.zoneOffset), 1 < this.config.maxRetries && (n.data[0]["#uuid"] = r.UUIDv4()), 
n["#app_id"] = this.appId, this.notifyAllObserver("onDataEnqueue", {
appId: this.appId,
event: n.data[0]
}), _.info("Enqueue data, " + JSON.stringify(n, null, 4)), "debug" === t.debugMode || "debugOnly" === t.debugMode) return M.runTimeout(this.config.sendTimeout) && M.resetTimeout(), 
void M.enqueue(n, this.serverDebugUrl, {
maxRetries: this.config.maxRetries,
sendTimeout: this.config.sendTimeout,
callback: t.onComplete,
debugMode: t.debugMode,
deviceId: this.getDeviceId()
});
var a = "debug" === this.config.debugMode || "debugOnly" === this.config.debugMode ? this.serverDebugUrl : this.serverUrl;
if (r.isBoolean(this.config.enableEncrypt) && !0 === this.config.enableEncrypt && (n.data[0] = r.generateEncryptyData(n.data[0], this.config.secretKey)), 
this.batchConsumer && "none" === this.config.debugMode && !i) return this.batchConsumer.add(n.data[0]), 
void (r.isFunction(t.onComplete) && t.onComplete({
code: 0,
msg: "success"
}));
i ? (i = new FormData(), "debug" === this.config.debugMode || "debugOnly" === this.config.debugMode ? (i.append("source", "client"), 
i.append("appid", this.appId), i.append("dryRun", "debugOnly" === this.config.debugMode ? 1 : 0), 
i.append("deviceId", this.getDeviceId()), i.append("data", JSON.stringify(n.data[0])), 
navigator.sendBeacon(a, i)) : (i = r.getCurrentTimeStamp(), n["#flush_time"] = i, 
navigator.sendBeacon(a, JSON.stringify(n))), r.isFunction(t.onComplete) && t.onComplete({
statusCode: 200
})) : (M.runTimeout(this.config.sendTimeout) && M.resetTimeout(), M.enqueue(n, a, {
maxRetries: this.config.maxRetries,
sendTimeout: this.config.sendTimeout,
callback: t.onComplete,
debugMode: this.config.debugMode,
deviceId: this.getDeviceId()
}));
} else _.info("Disabled Event : " + t.eventName);
}
}, {
key: "_isObjectParams",
value: function(t) {
return r.isObject(t) && r.isFunction(t.onComplete);
}
}, {
key: "track",
value: function(t, e, i, n) {
var a;
O.isWxPlat() && (this.wxSdk && (e = e || {}, this.wxSdk.track(t, e)), this.isTADisable) || this._hasDisabled() || (this._isObjectParams(t) && (t = (a = t).eventName, 
e = a.properties, i = a.time, n = a.onComplete), w.event(t) && w.properties(e) || !this.config.strict ? this._internalTrack(t, e, i, n, !1, !0) : r.isFunction(n) && n({
code: -1,
msg: "invalid parameters"
}));
}
}, {
key: "trackInternal",
value: function(t) {
this._hasDisabled() || this._internalTrack(t.eventName, t.properties, t.time, t.onComplete, !1, !0, t.debugMode);
}
}, {
key: "trackUpdate",
value: function(t) {
var e, i;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (t && t.eventId && (w.event(t.eventName) && w.properties(t.properties) || !this.config.strict) ? this._isReady() ? (e = r.checkCalibration(t.properties, t.time, this.config.enableCalibrationTime), 
i = r.isDate(t.time) ? t.time : r.getCurrentDate(), this._sendRequest({
type: "track_update",
eventName: t.eventName,
properties: e,
onComplete: t.onComplete,
extraId: t.eventId
}, i)) : this._queue.push([ "trackUpdate", [ t ] ]) : (_.warn("Invalide parameter for trackUpdate: you should pass an object contains eventId to trackUpdate()"), 
r.isFunction(t.onComplete) && t.onComplete({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "trackOverwrite",
value: function(t) {
var e, i;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (t && t.eventId && (w.event(t.eventName) && w.properties(t.properties) || !this.config.strict) ? this._isReady() ? (e = r.checkCalibration(t.properties, t.time, this.config.enableCalibrationTime), 
i = r.isDate(t.time) ? t.time : r.getCurrentDate(), this._sendRequest({
type: "track_overwrite",
eventName: t.eventName,
properties: e,
onComplete: t.onComplete,
extraId: t.eventId
}, i)) : this._queue.push([ "trackOverwrite", [ t ] ]) : (_.warn("Invalide parameter for trackOverwrite: you should pass an object contains eventId to trackOverwrite()"), 
r.isFunction(t.onComplete) && t.onComplete({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "trackFirstEvent",
value: function(t) {
var e, i;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (t && t.eventName && (w.event(t.eventName) && w.properties(t.properties) || !this.config.strict) ? this._isReady() ? (e = r.checkCalibration(t.properties, t.time, this.config.enableCalibrationTime), 
i = r.isDate(t.time) ? t.time : r.getCurrentDate(), this._sendRequest({
type: "track",
eventName: t.eventName,
properties: e,
onComplete: t.onComplete,
firstCheckId: t.firstCheckId || this.getDeviceId()
}, i)) : this._queue.push([ "trackFirstEvent", [ t ] ]) : (_.warn("Invalide parameter for trackFirstEvent: you should pass an object contains eventName to trackFirstEvent()"), 
r.isFunction(t.onComplete) && t.onComplete({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "_internalTrack",
value: function(t, e, i, n, a, s, o) {
!s && (this.wxSdk && ((e = e || {}).trackBy = "ThinkingData", this.wxSdk.track(t, e)), 
this.isTADisable) || this._hasDisabled() || (s = r.checkCalibration(e, i, this.config.enableCalibrationTime), 
i = r.isDate(i) ? i : r.getCurrentDate(), this._isReady() ? this._sendRequest({
type: "track",
eventName: t,
debugMode: o,
properties: s,
onComplete: n
}, i, a) : this._queue.push([ "_internalTrack", [ t, e, i, n, a, !0 ] ]));
}
}, {
key: "userSet",
value: function(t, e, i) {
var n;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this._isObjectParams(t) && (t = (n = t).properties, 
e = n.time, i = n.onComplete), w.propertiesMust(t) || !this.config.strict ? (e = r.isDate(e) ? e : r.getCurrentDate(), 
this._isReady() ? this._sendRequest({
type: "user_set",
properties: t,
onComplete: i
}, e) : this._queue.push([ "userSet", [ t, e, i ] ])) : (_.warn("calling userSet failed due to invalid arguments"), 
r.isFunction(i) && i({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "userSetOnce",
value: function(t, e, i) {
var n;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this._isObjectParams(t) && (t = (n = t).properties, 
e = n.time, i = n.onComplete), w.propertiesMust(t) || !this.config.strict ? (e = r.isDate(e) ? e : r.getCurrentDate(), 
this._isReady() ? this._sendRequest({
type: "user_setOnce",
properties: t,
onComplete: i
}, e) : this._queue.push([ "userSetOnce", [ t, e, i ] ])) : (_.warn("calling userSetOnce failed due to invalid arguments"), 
r.isFunction(i) && i({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "userUnset",
value: function(t, e, i) {
var n;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this._isObjectParams(n) && (t = n.property, 
e = n.time, i = n.onComplete), w.propertyName(t) || !this.config.strict ? (e = r.isDate(e) ? e : r.getCurrentDate(), 
this._isReady() ? ((n = {})[t] = 0, this._sendRequest({
type: "user_unset",
properties: n,
onComplete: i
}, e)) : this._queue.push([ "userUnset", [ t, i, e ] ])) : (_.warn("calling userUnset failed due to invalid arguments"), 
r.isFunction(i) && i({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "userDel",
value: function(t, e) {
var i;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this._isObjectParams(t) && (t = (i = t).time, 
e = i.onComplete), t = r.isDate(t) ? t : r.getCurrentDate(), this._isReady() ? this._sendRequest({
type: "user_del",
onComplete: e
}, t) : this._queue.push([ "userDel", [ t, e ] ]));
}
}, {
key: "userAdd",
value: function(t, e, i) {
var n;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this._isObjectParams(t) && (t = (n = t).properties, 
e = n.time, i = n.onComplete), w.userAddProperties(t) || !this.config.strict ? (e = r.isDate(e) ? e : r.getCurrentDate(), 
this._isReady() ? this._sendRequest({
type: "user_add",
properties: t,
onComplete: i
}, e) : this._queue.push([ "userAdd", [ t, e, i ] ])) : (_.warn("calling userAdd failed due to invalid arguments"), 
r.isFunction(i) && i({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "userAppend",
value: function(t, e, i) {
var n;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this._isObjectParams(t) && (t = (n = t).properties, 
e = n.time, i = n.onComplete), w.userAppendProperties(t) || !this.config.strict ? (e = r.isDate(e) ? e : r.getCurrentDate(), 
this._isReady() ? this._sendRequest({
type: "user_append",
properties: t,
onComplete: i
}, e) : this._queue.push([ "userAppend", [ t, e, i ] ])) : (_.warn("calling userAppend failed due to invalid arguments"), 
r.isFunction(i) && i({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "userUniqAppend",
value: function(t, e, i) {
var n;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this._isObjectParams(t) && (t = (n = t).properties, 
e = n.time, i = n.onComplete), w.userAppendProperties(t) || !this.config.strict ? (e = r.isDate(e) ? e : r.getCurrentDate(), 
this._isReady() ? this._sendRequest({
type: "user_uniq_append",
properties: t,
onComplete: i
}, e) : this._queue.push([ "userUniqAppend", [ t, e, i ] ])) : (_.warn("calling userAppend failed due to invalid arguments"), 
r.isFunction(i) && i({
code: -1,
msg: "invalid parameters"
})));
}
}, {
key: "flush",
value: function() {
O.isWxPlat() && this.isTADisable || this.batchConsumer && "none" === this.config.debugMode && this.batchConsumer.flush();
}
}, {
key: "authorizeOpenID",
value: function(t) {
this.identify(t);
}
}, {
key: "identify",
value: function(t) {
if (!(O.isWxPlat() && this.isTADisable || this._hasDisabled() || void 0 === t || "" === t.trim())) {
if ("number" == typeof t) t = String(t); else if ("string" != typeof t) return !1;
this.store.setDistinctId(t), _.info("Setting distinct ID, DistinctId = " + t), this.notifyAllObserver("onAccountChanged", {
accountId: this.getAccountId(),
distinctId: t
});
}
}
}, {
key: "getDistinctId",
value: function() {
return O.isWxPlat() && this.isTADisable ? "" : this.store.getDistinctId();
}
}, {
key: "login",
value: function(t) {
if (!(O.isWxPlat() && this.isTADisable || this._hasDisabled() || void 0 === t || "" === t.trim())) {
if ("number" == typeof t) t = String(t); else if ("string" != typeof t) return !1;
this.store.setAccountId(t), _.info("Login SDK, AccountId = " + t), this.notifyAllObserver("onAccountChanged", {
accountId: t,
distinctId: this.getDistinctId()
});
}
}
}, {
key: "getAccountId",
value: function() {
return O.isWxPlat() && this.isTADisable ? "" : this.store.getAccountId();
}
}, {
key: "logout",
value: function() {
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (this.store.setAccountId(null), 
_.info("Logout SDK"), this.notifyAllObserver("onAccountChanged", {
accountId: "",
distinctId: this.getDistinctId()
}));
}
}, {
key: "notifyAllObserver",
value: function(t, e) {
for (var i = 0; i < this.observers.length; i++) this.observers[i](t, e);
}
}, {
key: "setSuperProperties",
value: function(t) {
O.isWxPlat() && this.isTADisable || this._hasDisabled() || (w.propertiesMust(t) || !this.config.strict ? this.store.setSuperProperties(t) : _.warn("setSuperProperties parameter must be a valid property value"));
}
}, {
key: "clearSuperProperties",
value: function() {
O.isWxPlat() && this.isTADisable || this._hasDisabled() || this.store.setSuperProperties({}, !0);
}
}, {
key: "unsetSuperProperty",
value: function(t) {
var e;
O.isWxPlat() && this.isTADisable || this._hasDisabled() || r.isString(t) && (delete (e = this.getSuperProperties())[t], 
this.store.setSuperProperties(e, !0));
}
}, {
key: "getSuperProperties",
value: function() {
return O.isWxPlat() && this.isTADisable ? {} : this.store.getSuperProperties();
}
}, {
key: "getPresetProperties",
value: function() {
if (O.isWxPlat() && this.isTADisable) return {};
var t, e, i, n = D.properties, a = {};
this.config.disablePresetProperties.includes("#os") || (t = n["#os"], a.os = r.isUndefined(t) ? "" : t), 
this.config.disablePresetProperties.includes("#screen_width") || (t = n["#screen_width"], 
a.screenWidth = r.isUndefined(t) ? 0 : t), this.config.disablePresetProperties.includes("#screen_height") || (e = n["#screen_height"], 
a.screenHeight = r.isUndefined(e) ? 0 : e), this.config.disablePresetProperties.includes("#network_type") || (e = n["#network_type"], 
a.networkType = r.isUndefined(e) ? "" : e), this.config.disablePresetProperties.includes("#device_model") || (i = n["#device_model"], 
a.deviceModel = r.isUndefined(i) ? "" : i), this.config.disablePresetProperties.includes("#os_version") || (i = n["#os_version"], 
a.osVersion = r.isUndefined(i) ? "" : i), this.config.disablePresetProperties.includes("#device_id") || (a.deviceId = this.getDeviceId());
var s = r.getTimeZone(r.getCurrentDate(), this.config.zoneOffset);
return a.zoneOffset = s, this.config.disablePresetProperties.includes("#manufacturer") || (n = n["#manufacturer"], 
a.manufacturer = r.isUndefined(n) ? "" : n), a.toEventPresetProperties = function() {
var t = {};
return a.deviceModel && (t["#device_model"] = a.deviceModel), a.deviceId && (t["#device_id"] = a.deviceId), 
a.screenWidth && (t["#screen_width"] = a.screenWidth), a.screenHeight && (t["#screen_height"] = a.screenHeight), 
a.os && (t["#os"] = a.os), a.osVersion && (t["#os_version"] = a.osVersion), a.networkType && (t["#network_type"] = a.networkType), 
t["#zone_offset"] = s, a.manufacturer && (t["#manufacturer"] = a.manufacturer), 
t;
}, a;
}
}, {
key: "setDynamicSuperProperties",
value: function(t) {
if (O.isWxPlat() && this.isTADisable) return {};
this._hasDisabled() || ("function" == typeof t ? w.properties(t()) || !this.config.strict ? this.dynamicProperties = t : _.warn("A dynamic public property must return a valid property value") : _.warn("setDynamicSuperProperties parameter must be a function type"));
}
}, {
key: "registerAnalyticsObserver",
value: function(t) {
this._hasDisabled() || ("function" == typeof t ? this.observers.push(t) : _.warn("registerAnalyticsObserver parameter must be a function type"));
}
}, {
key: "timeEvent",
value: function(t, e) {
if (O.isWxPlat() && this.isTADisable) return {};
this._hasDisabled() || (e = r.isDate(e) ? e : r.getCurrentDate(), this._isReady() ? w.event(t) || !this.config.strict ? this.store.setEventTimer(t, e.getTime()) : _.warn("calling timeEvent failed due to invalid eventName: " + t) : this._queue.push([ "timeEvent", [ t, e ] ]));
}
}, {
key: "getDeviceId",
value: function() {
return O.isWxPlat() && this.isTADisable ? "" : D.properties["#device_id"];
}
}, {
key: "enableTracking",
value: function(t) {
O.isWxPlat() && this.isTADisable || (this.enabled = t, this.store._set("ta_enabled", t));
}
}, {
key: "optOutTracking",
value: function() {
O.isWxPlat() && this.isTADisable || (this.store.setSuperProperties({}, !0), this.store.setDistinctId(r.UUID()), 
this.store.setAccountId(null), this._queue.splice(0, this._queue.length), this.isOptOut = !0, 
this.store._set("ta_isOptOut", !0));
}
}, {
key: "optOutTrackingAndDeleteUser",
value: function() {
var t;
O.isWxPlat() && this.isTADisable || (t = r.getCurrentDate(), this._sendRequest({
type: "user_del"
}, t), this.optOutTracking());
}
}, {
key: "optInTracking",
value: function() {
O.isWxPlat() && this.isTADisable || (this.isOptOut = !1, this.store._set("ta_isOptOut", !1));
}
}, {
key: "setTrackStatus",
value: function(t) {
if (!O.isWxPlat() || !this.isTADisable) {
switch (t) {
case "PAUSE":
this.eventSaveOnly = !1, this.optInTracking(), this.enableTracking(!1);
break;

case "STOP":
this.eventSaveOnly = !1, this.optOutTracking(!0);
break;

case "SAVE_ONLY":
break;

case "NORMAL":
default:
this.eventSaveOnly = !1, this.optInTracking(), this.enableTracking(!0);
}
_.info("Change Status to " + t);
}
}
} ]), t;
}(), q = {
name: "thinkingdata",
enableLog: !0,
enableNative: !1
}, W = function() {
function t(e) {
n(this, t), e.appId = e.appId ? r.checkAppId(e.appId) : r.checkAppId(e.appid), e.serverUrl = e.serverUrl ? r.checkUrl(e.serverUrl) : r.checkUrl(e.server_url);
var i = r.extend({}, q, O.getConfig());
r.isObject(e) ? this.config = r.extend(i, e) : this.config = i, this._init(this.config);
}
return s(t, [ {
key: "_isNativePlatform",
value: function() {
return !(!(this._isIOS() || this._isAndroid() || this._isOpenHarmony()) || !this.config.enableNative);
}
}, {
key: "_isIOS",
value: function() {
return !(!cc.sys.isNative || "iOS" !== cc.sys.os);
}
}, {
key: "_isAndroid",
value: function() {
return !(!cc.sys.isNative || "Android" !== cc.sys.os);
}
}, {
key: "_isOpenHarmony",
value: function() {
return !(!cc.sys.isNative || "OpenHarmony" !== cc.sys.os);
}
}, {
key: "_init",
value: function(t) {
this.name = t.name, this.appId = t.appId || t.appid, this._isNativePlatform() ? (this.initInstanceForNative(t), 
this._readStorage(t)) : this.taJs = new ThinkingAnalyticsAPIForJS(t);
}
}, {
key: "_readStorage",
value: function(t) {
var e = this, i = t.persistenceName, n = t.persistenceNameOld;
t.isChildInstance && (i = t.persistenceName + "_" + t.name, n = t.persistenceNameOld + "_" + t.name), 
this._state = O.getStorage(i) || {}, r.isEmptyObject(this._state) && (this._state = O.getStorage(n) || {}), 
r.isEmptyObject(this._state) ? O.getStorage(i, !0, function(t) {
r.isEmptyObject(t) ? O.getStorage(n, !0, function(t) {
e._state = r.extend2Layers({}, t, e._state);
}) : e._state = r.extend2Layers({}, t, e._state), e._state.distinct_id && e.identifyForNative(e._state.distinct_id), 
e._state.account_id && e.loginForNative(e._state.account_id);
}) : (this._state.distinct_id && this.identifyForNative(this._state.distinct_id), 
this._state.account_id && this.loginForNative(this._state.account_id));
}
}, {
key: "initInstance",
value: function(t, e) {
return this._isNativePlatform() ? r.isUndefined(e) ? this[t] = new ThinkingAnalyticsAPI(this.config) : this[t] = new ThinkingAnalyticsAPI(e) : this[t] = this.taJs.initInstance(t, e), 
this[t];
}
}, {
key: "lightInstance",
value: function() {
return this._isNativePlatform() ? this.lightInstanceForNative(this.appId) : this;
}
}, {
key: "init",
value: function() {
if (this._isNativePlatform()) {
var t = window, e = this;
return t.__autoTrackCallback = function(t) {
if (r.isFunction(e.config.autoTrack.callback)) {
t = e.config.autoTrack.callback(t);
return JSON.stringify(t);
}
return "{}";
}, void this.startThinkingAnalyticsForNative(this.appId);
}
this.taJs.init();
}
}, {
key: "track",
value: function(t, e, i, n) {
this._isNativePlatform() ? this.trackForNative(t, e, i, this.appId) : this.taJs.track(t, e, i, n);
}
}, {
key: "trackUpdate",
value: function(t) {
this._isNativePlatform() ? this.trackUpdateForNative(t, this.appId) : this.taJs.trackUpdate(t);
}
}, {
key: "trackOverwrite",
value: function(t) {
this._isNativePlatform() ? this.trackOverwriteForNative(t, this.appId) : this.taJs.trackOverwrite(t);
}
}, {
key: "trackFirstEvent",
value: function(t) {
this._isNativePlatform() ? this.trackFirstEventForNative(t, this.appId) : this.taJs.trackFirstEvent(t);
}
}, {
key: "userSet",
value: function(t, e, i) {
this._isNativePlatform() ? this.userSetForNative(t, this.appId) : this.taJs.userSet(t, e, i);
}
}, {
key: "userSetOnce",
value: function(t, e, i) {
this._isNativePlatform() ? this.userSetOnceForNative(t, this.appId) : this.taJs.userSetOnce(t, e, i);
}
}, {
key: "userUnset",
value: function(t, e, i) {
this._isNativePlatform() ? this.userUnsetForNative(t, this.appId) : this.taJs.userUnset(t, e, i);
}
}, {
key: "userDel",
value: function(t, e) {
this._isNativePlatform() ? this.userDelForNative(this.appId) : this.taJs.userDel(t, e);
}
}, {
key: "userAdd",
value: function(t, e, i) {
this._isNativePlatform() ? this.userAddForNative(t, this.appId) : this.taJs.userAdd(t, e, i);
}
}, {
key: "userAppend",
value: function(t, e, i) {
this._isNativePlatform() ? this.userAppendForNative(t, this.appId) : this.taJs.userAppend(t, e, i);
}
}, {
key: "userUniqAppend",
value: function(t, e, i) {
this._isNativePlatform() ? this.userUniqAppendForNative(t, this.appId) : this.taJs.userUniqAppend(t, e, i);
}
}, {
key: "flush",
value: function() {
this._isNativePlatform() ? this.flushForNative(this.appId) : this.taJs.flush();
}
}, {
key: "authorizeOpenID",
value: function(t) {
this.identify(t);
}
}, {
key: "identify",
value: function(t) {
this._isNativePlatform() ? this.identifyForNative(t, this.appId) : this.taJs.identify(t);
}
}, {
key: "getDistinctId",
value: function() {
return this._isNativePlatform() ? this.getDistinctIdForNative(this.appId) : this.taJs.getDistinctId();
}
}, {
key: "login",
value: function(t) {
this._isNativePlatform() ? this.loginForNative(t, this.appId) : this.taJs.login(t);
}
}, {
key: "getAccountId",
value: function() {
return this._isNativePlatform() ? this.getAccountIdForNative(this.appId) : this.taJs.getAccountId();
}
}, {
key: "logout",
value: function() {
this._isNativePlatform() ? this.logoutForNative(this.appId) : this.taJs.logout();
}
}, {
key: "setSuperProperties",
value: function(t) {
this._isNativePlatform() ? this.setSuperPropertiesForNative(t, this.appId) : this.taJs.setSuperProperties(t);
}
}, {
key: "clearSuperProperties",
value: function() {
this._isNativePlatform() ? this.clearSuperPropertiesForNative(this.appId) : this.taJs.clearSuperProperties();
}
}, {
key: "unsetSuperProperty",
value: function(t) {
this._isNativePlatform() ? this.unsetSuperPropertyForNative(t, this.appId) : this.taJs.unsetSuperProperty(t);
}
}, {
key: "getSuperProperties",
value: function() {
return this._isNativePlatform() ? this.getSuperPropertiesForNative(this.appId) : this.taJs.getSuperProperties();
}
}, {
key: "getPresetProperties",
value: function() {
if (this._isNativePlatform()) {
var t = this.getPresetPropertiesForNative(this.appId), e = {}, i = t["#os"];
e.os = r.isUndefined(i) ? "" : i;
i = t["#screen_width"];
e.screenWidth = r.isUndefined(i) ? 0 : i;
i = t["#screen_height"];
e.screenHeight = r.isUndefined(i) ? 0 : i;
i = t["#network_type"];
e.networkType = r.isUndefined(i) ? "" : i;
i = t["#device_model"];
e.deviceModel = r.isUndefined(i) ? "" : i;
i = t["#os_version"];
e.osVersion = r.isUndefined(i) ? "" : i, e.deviceId = this.getDeviceId();
var n = 0 - new Date().getTimezoneOffset() / 60;
e.zoneOffset = n;
t = t["#manufacturer"];
return e.manufacturer = r.isUndefined(t) ? "" : t, e.toEventPresetProperties = function() {
return {
"#device_model": e.deviceModel,
"#device_id": e.deviceId,
"#screen_width": e.screenWidth,
"#screen_height": e.screenHeight,
"#os": e.os,
"#os_version": e.osVersion,
"#network_type": e.networkType,
"#zone_offset": n,
"#manufacturer": e.manufacturer
};
}, e;
}
return this.taJs.getPresetProperties();
}
}, {
key: "setDynamicSuperProperties",
value: function(t) {
this._isNativePlatform() ? "function" == typeof t ? (this.dynamicProperties = t, 
window.__dynamicPropertiesForNative = function(e) {
console.log("__dynamicPropertiesForNative: native msg: ", e);
e = t(), e = r.encodeDates(e);
return JSON.stringify(e);
}, this.setDynamicSuperPropertiesForNative("__dynamicPropertiesForNative")) : logger.warn("setDynamicSuperProperties parameter must be a function type") : this.taJs.setDynamicSuperProperties(t);
}
}, {
key: "timeEvent",
value: function(t, e) {
return this._isNativePlatform() ? this.timeEventForNative(t, this.appId) : this.taJs.timeEvent(t, e);
}
}, {
key: "getDeviceId",
value: function() {
return this._isNativePlatform() ? this.getDeviceIdForNative(this.appId) : this.taJs.getDeviceId();
}
}, {
key: "enableTracking",
value: function(t) {
this._isNativePlatform() ? this.enableTrackingForNative(t, this.appId) : this.taJs.enableTracking(t);
}
}, {
key: "optOutTracking",
value: function() {
this._isNativePlatform() ? this.optOutTrackingForNative(this.appId) : this.taJs.optOutTracking();
}
}, {
key: "optOutTrackingAndDeleteUser",
value: function() {
this._isNativePlatform() ? this.optOutTrackingAndDeleteUserForNative(this.appId) : this.taJs.optOutTrackingAndDeleteUser();
}
}, {
key: "optInTracking",
value: function() {
this._isNativePlatform() ? this.optInTrackingForNative(this.appId) : this.taJs.optInTracking();
}
}, {
key: "setTrackStatus",
value: function(t) {
this._isNativePlatform() ? this.setTrackStatusForNative(t, this.appId) : this.taJs.setTrackStatus(t);
}
}, {
key: "trackForNative",
value: function(t, e, i, n) {
i = r.isDate(i) ? r.formatDate(i) : "";
r.isUndefined(e) && (e = {}), e = r.extend(e, this.dynamicProperties ? this.dynamicProperties() : {}), 
e = r.encodeDates(e), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "track", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V", t, JSON.stringify(e), i, n) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "track:properties:time:appId:", t, JSON.stringify(e), i, n) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "track", JSON.stringify({
eventName: t,
properties: e,
appId: n
}), !0);
}
}, {
key: "trackUpdateForNative",
value: function(t, e) {
t.properties = r.extend(r.isUndefined(t.properties) ? {} : t.properties, this.dynamicProperties ? this.dynamicProperties() : {}), 
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "trackUpdate", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "trackUpdate:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "trackUpdate", JSON.stringify({
event: t,
appId: e
}), !0);
}
}, {
key: "trackFirstEventForNative",
value: function(t, e) {
t.properties = r.extend(r.isUndefined(t.properties) ? {} : t.properties, this.dynamicProperties ? this.dynamicProperties() : {}), 
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "trackFirstEvent", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "trackFirstEvent:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "trackFirst", JSON.stringify({
event: t,
appId: e
}), !0);
}
}, {
key: "trackOverwriteForNative",
value: function(t, e) {
t.properties = r.extend(r.isUndefined(t.properties) ? {} : t.properties, this.dynamicProperties ? this.dynamicProperties() : {}), 
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "trackOverwrite", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "trackOverwrite:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "trackOverwrite", JSON.stringify({
event: t,
appId: e
}), !0);
}
}, {
key: "timeEventForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "timeEvent", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "timeEvent:appId:", t, e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "timeEvent", JSON.stringify({
eventName: t,
appId: e
}), !0);
}
}, {
key: "loginForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "login", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "login:appId:", t, e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "login", JSON.stringify({
accountId: t,
appId: e
}), !0);
}
}, {
key: "logoutForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "logout", "(Ljava/lang/String;)V", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "logout:", t) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "logout", t, !0);
}
}, {
key: "setSuperPropertiesForNative",
value: function(t, e) {
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "setSuperProperties", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "setSuperProperties:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "setSuperProperties", JSON.stringify({
properties: t,
appId: e
}), !0);
}
}, {
key: "getSuperPropertiesForNative",
value: function(t) {
var e = "{}";
return this._isAndroid() ? e = jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "getSuperProperties", "(Ljava/lang/String;)Ljava/lang/String;", t) : this._isIOS() ? e = jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "getSuperProperties:", t) : this._isOpenHarmony() && (e = jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "getSuperProperties", t, !0)), 
JSON.parse(e);
}
}, {
key: "unsetSuperPropertyForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "unsetSuperProperty", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "unsetSuperProperty:appId:", t, e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "unsetSuperProperty", JSON.stringify({
property: t,
appId: e
}), !0);
}
}, {
key: "clearSuperPropertiesForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "clearSuperProperties", "(Ljava/lang/String;)V", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "clearSuperProperties:", t) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "clearSuperProperties", t, !0);
}
}, {
key: "userSetForNative",
value: function(t, e) {
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "userSet", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "userSet:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "userSet", JSON.stringify({
properties: t,
appId: e
}), !0);
}
}, {
key: "userSetOnceForNative",
value: function(t, e) {
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "userSetOnce", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "userSetOnce:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "userSetOnce", JSON.stringify({
properties: t,
appId: e
}), !0);
}
}, {
key: "userAppendForNative",
value: function(t, e) {
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "userAppend", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "userAppend:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "userAppend", JSON.stringify({
properties: t,
appId: e
}), !0);
}
}, {
key: "userUniqAppendForNative",
value: function(t, e) {
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "userUniqAppend", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "userUniqAppend:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "userUniqAppend", JSON.stringify({
properties: t,
appId: e
}), !0);
}
}, {
key: "userAddForNative",
value: function(t, e) {
t = r.encodeDates(t), this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "userAdd", "(Ljava/lang/String;Ljava/lang/String;)V", JSON.stringify(t), e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "userAdd:appId:", JSON.stringify(t), e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "userAdd", JSON.stringify({
properties: t,
appId: e
}), !0);
}
}, {
key: "userUnsetForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "userUnset", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "userUnset:appId:", t, e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "userUnset", JSON.stringify({
property: t,
appId: e
}), !0);
}
}, {
key: "userDelForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "userDel", "(Ljava/lang/String;)V", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "userDel:", t) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "userDel", t, !0);
}
}, {
key: "flushForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "flush", "(Ljava/lang/String;)V", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "flush:", t) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "flush", t, !0);
}
}, {
key: "authorizeOpenIDForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "authorizeOpenID", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "authorizeOpenID:appId:", t, e);
}
}, {
key: "identifyForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "identify", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "identify:appId:", t, e) : this._isOpenHarmony() && jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "setDistinctId", JSON.stringify({
distinctId: t,
appId: e
}), !0);
}
}, {
key: "initInstanceForNative",
value: function(t) {
this._isAndroid() ? (jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "setCustomerLibInfo", "(Ljava/lang/String;Ljava/lang/String;)V", "MG", "3.3.0-beta.2"), 
jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "initWithConfig", "(Ljava/lang/String;)V", JSON.stringify(t))) : this._isIOS() ? (jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "setCustomerLibInfoWithLibName:libVersion:", "MG", "3.3.0-beta.2"), 
jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "initWithConfig:", JSON.stringify(t))) : this._isOpenHarmony() && (jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "setCustomerLibInfo", JSON.stringify({
lib: "MG",
version: "3.3.0-beta.2"
}), !0), jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "initWithConfig", JSON.stringify(t), !0));
}
}, {
key: "lightInstanceForNative",
value: function(t) {
return this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "lightInstance", "(Ljava/lang/String;)Ljava/lang/String;", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "lightInstance:", t) : void 0;
}
}, {
key: "startThinkingAnalyticsForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "startThinkingAnalytics", "(Ljava/lang/String;)V", t) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "startThinkingAnalytics:", t);
}
}, {
key: "setDynamicSuperPropertiesForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "setDynamicSuperProperties", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "setDynamicSuperProperties:appId:", t, e);
}
}, {
key: "getDeviceIdForNative",
value: function(t) {
return this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "getDeviceId", "(Ljava/lang/String;)Ljava/lang/String;", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "getDeviceId:", t) : this._isOpenHarmony() ? jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "getDeviceId", t, !0) : void 0;
}
}, {
key: "getDistinctIdForNative",
value: function(t) {
return this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "getDistinctId", "(Ljava/lang/String;)Ljava/lang/String;", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "getDistinctId:", t) : this._isOpenHarmony() ? jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "getDistinctId", t, !0) : void 0;
}
}, {
key: "getAccountIdForNative",
value: function(t) {
return this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "getAccountId", "(Ljava/lang/String;)Ljava/lang/String;", t) : this._isIOS() ? jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "getAccountId:", t) : this._isOpenHarmony() ? jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "getAccountId", t, !0) : void 0;
}
}, {
key: "getPresetPropertiesForNative",
value: function(t) {
var e = "{}";
return this._isAndroid() ? e = jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "getPresetProperties", "(Ljava/lang/String;)Ljava/lang/String;", t) : this._isIOS() ? e = jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "getPresetProperties:", t) : this._isOpenHarmony() && (e = jsb.reflection.callStaticMethod("entry/src/main/ets/CocosCreatorProxyApi", "getPresetProperties", t, !0)), 
JSON.parse(e);
}
}, {
key: "enableTrackingForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "enableTracking", "(Ljava/lang/String;Ljava/lang/String;)V", t.toString(), e) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "enableTracking:appId:", t.toString(), e);
}
}, {
key: "optOutTrackingForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "optOutTracking", "(Ljava/lang/String;)V", t) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "optOutTracking:", t);
}
}, {
key: "optOutTrackingAndDeleteUserForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "optOutTrackingAndDeleteUser", "(Ljava/lang/String;)V", t) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "optOutTrackingAndDeleteUser:", t);
}
}, {
key: "optInTrackingForNative",
value: function(t) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "optInTracking", "(Ljava/lang/String;)V", t) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "optInTracking:", t);
}
}, {
key: "setTrackStatusForNative",
value: function(t, e) {
this._isAndroid() ? jsb.reflection.callStaticMethod("com/cocos/game/CocosCreatorProxyApi", "setTrackStatus", "(Ljava/lang/String;Ljava/lang/String;)V", t, e) : this._isIOS() && jsb.reflection.callStaticMethod("CocosCreatorProxyApi", "setTrackStatus:appId:", t, e);
}
} ]), t;
}();
window.ThinkingAnalyticsAPI = W, window.ThinkingAnalyticsAPIForJS = R;
var J = function() {
function t() {
n(this, t);
}
return s(t, null, [ {
key: "_shareInstance",
value: function(t) {
return void 0 !== this._instanceMaps[t] ? this._instanceMaps[t] : void 0 !== this._defaultInstance ? this._defaultInstance : void 0;
}
}, {
key: "init",
value: function(t) {
var e = new W(t);
e.init(), void 0 !== e && (void 0 === this._defaultInstance && (this._defaultInstance = e, 
this._instanceMaps = {}), this._instanceMaps[t.appId] = e);
}
}, {
key: "lightInstance",
value: function(t) {
return this._shareInstance(t).lightInstance();
}
}, {
key: "track",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).track(t.eventName, t.properties, t.time, t.onComplete);
}
}, {
key: "trackFirst",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).trackFirstEvent(t);
}
}, {
key: "trackUpdate",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).trackUpdate(t);
}
}, {
key: "trackOverwrite",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).trackOverwrite(t);
}
}, {
key: "timeEvent",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).timeEvent(t.eventName, t.time);
}
}, {
key: "userSet",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).userSet(t.properties, t.time, t.onComplete);
}
}, {
key: "userSetOnce",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).userSetOnce(t.properties, t.time, t.onComplete);
}
}, {
key: "userUnset",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).userUnset(t.property, t.time, t.onComplete);
}
}, {
key: "userAdd",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).userAdd(t.properties, t.time, t.onComplete);
}
}, {
key: "userAppend",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).userAppend(t.properties, t.time, t.onComplete);
}
}, {
key: "userUniqAppend",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).userUniqAppend(t.properties, t.time, t.onComplete);
}
}, {
key: "userDelete",
value: function(t, e) {
t = 0 < arguments.length && void 0 !== t ? t : {}, e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).userDel(t.time, t.onComplete);
}
}, {
key: "setSuperProperties",
value: function(t, e) {
e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).setSuperProperties(t);
}
}, {
key: "unsetSuperProperty",
value: function(t, e) {
e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).unsetSuperProperty(t);
}
}, {
key: "clearSuperProperties",
value: function(t) {
this._shareInstance(t).clearSuperProperties();
}
}, {
key: "getSuperProperties",
value: function(t) {
return this._shareInstance(t).getSuperProperties();
}
}, {
key: "setDynamicSuperProperties",
value: function(t, e) {
e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).setDynamicSuperProperties(t);
}
}, {
key: "getPresetProperties",
value: function(t) {
return this._shareInstance(t).getPresetProperties();
}
}, {
key: "login",
value: function(t, e) {
e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).login(t);
}
}, {
key: "logout",
value: function(t) {
this._shareInstance(t).logout();
}
}, {
key: "setDistinctId",
value: function(t, e) {
e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).identify(t);
}
}, {
key: "getDistinctId",
value: function(t) {
return this._shareInstance(t).getDistinctId();
}
}, {
key: "getAccountId",
value: function(t) {
return this._shareInstance(t).getAccountId();
}
}, {
key: "getSDKVersion",
value: function() {
return "1.0.0";
}
}, {
key: "getDeviceId",
value: function(t) {
return this._shareInstance(t).getDeviceId();
}
}, {
key: "flush",
value: function(t) {
this._shareInstance(t).flush();
}
}, {
key: "setTrackStatus",
value: function(t, e) {
e = 1 < arguments.length && void 0 !== e ? e : "";
this._shareInstance(e).setTrackStatus(t);
}
}, {
key: "ThinkingDataAPI",
value: function() {
return W;
}
} ]), t;
}();
window.TDAnalytics = J, e.exports = J;
cc._RF.pop();
}).call(this, "undefined" != typeof global ? global : "undefined" != typeof self ? self : "undefined" != typeof window ? window : {});
}, {} ],
use_reversed_rotateBy: [ function(t, e) {
"use strict";
cc._RF.push(e, "376cc8Ey+lOopNVgREQRuvL", "use_reversed_rotateBy");
cc.RotateBy._reverse = !0;
cc._RF.pop();
}, {} ]
}, {}, [ "use_reversed_rotateBy", "BgAdapter", "FitUI", "HelloSandglass", "ThinkingAnalyticsController", "Version", "parseImport", "spine-assember", "spine", "LanguageConf", "LanguageMgr", "StartWords", "StartWords_cn", "tdanalytics.mg.cocoscreator.min" ]);