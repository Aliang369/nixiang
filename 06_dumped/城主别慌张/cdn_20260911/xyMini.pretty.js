window.xyMini = function() {
    return {

        plat: 'minigame',

        /**是否自定义账号 */
        customAccount: true,

        autoEnter: true,

        cdnRoot: 'https://tf-cdn.shryou.com/',
        serviceUrl: 'https://tf-api.shryou.com/',
        serviceUrl2: 'https://tf-api.shryou.com/',
        payServiceUrl: 'https://tf-api.shryou.com/',
        dotServiceUrl: 'https://tf-data.shryou.com/',
        battleServiceUrl: 'https://tf-battle.shryou.com/',

        /**登录接口 */
        login(cb) {},

        /**登出接口 */
        logout(cb) {},

        /**支付接口 */
        pay(payParams) {},

        /**分享接口 */
        share(params) {},

        /**提交角色信息 */
        commitRoleInfo(params) {},

        /**复制到粘贴板 */
        copyToClipBoard(cb, content) {},

        burialPoint(param) {
            var url = xyMini.dotServiceUrl + 'data/upload';
            xyMini.httpPost(url, function(res) {
                // console.log('burialPoint==>result', res);
            }, param);
        },

        //#################### Utils ################################################################

        //获取浏览器缓存
        getCache(key, defaultValue) {
            var value = window.localStorage.getItem(key);
            if (value == null || value == void 0) return defaultValue;
            return value;
        },
        //设置缓存
        setCache(key, value) {
            if (key == '') return false;
            window.localStorage.setItem(key, value);
        },

        /**加载单个js文件 */
        loadScript(url, callback) {
            var s = document.createElement('script');
            s.async = false;
            s.src = url;
            s.addEventListener('load', function() {
                s.parentNode.removeChild(s);
                s.removeEventListener('load', arguments.callee, false);
                callback && callback();
            }, false);
            document.body.appendChild(s);

        },

        /**加载js文件列表 */
        loadScriptList(list, callback) {
            var loaded = 0;
            var loadNext = function() {
                xyMini.loadScript(list[loaded], function() {
                    loaded++;
                    if (loaded >= list.length) {
                        callback();
                    } else {
                        loadNext();
                    }
                });
            };
            loadNext();
        },

        /**获取url参数 */
        getHttpParams(name, def, obj) {
            var _def = def || "";
            var obj = obj || window;
            var r = new RegExp("(\\?|#|&)" + name + "=([^&#]*)(&|#|$)");
            var m = [];
            if (window && window.OPEN_DATA) { //玩吧
                m = window.OPEN_DATA.appurl.match(r);
            } else {
                if (obj && obj.location && obj.location.href) {
                    m = obj.location.href.match(r);
                } else {
                    return _def;
                }
            }
            return decodeURIComponent(!m ? _def : m[2]);
        },

        httpGet(url, callback, option) {
            var request = null;
            if (window.XMLHttpRequest) {
                request = new XMLHttpRequest();
            } else {
                request = new ActiveXObject("Microsoft.XMLHTTP");
            }
            if (request) {
                request.onreadystatechange = function() {
                    if (request.readyState == 4) {
                        if (callback) {
                            callback(option == "json" ? JSON.parse(request.responseText) : request.responseText);
                        }
                    }
                };
                request.open("GET", url, true);
                request.send();
            }
        },

        httpPost(url, callback, parm) {
            var xmlhttp;
            if (window.XMLHttpRequest) {
                xmlhttp = new XMLHttpRequest();
            } else {
                xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
            }
            xmlhttp.onreadystatechange = function() {
                if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                    if (callback) {
                        callback(JSON.parse(xmlhttp.responseText));
                    }
                }
            };
            xmlhttp.open("POST", url, true);
            xmlhttp.setRequestHeader("Content-type", "application/json");
            xmlhttp.send(JSON.stringify(parm));
        },

        getRandomStr(num) {
            var str = "";
            var chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            for (var i = 0; i < num; i++) {
                str = str + chars[Math.floor(Math.random() * 62)];
            }
            return str;
        },

        isiOS() {
            var u = navigator.userAgent;
            return u.indexOf("iPad") > -1 || u.indexOf("iPhone") > -1 || u.indexOf("Mac OS") > -1;
        },

        isAndroid() {
            var u = navigator.userAgent;
            return u.indexOf('Android') > -1 || u.indexOf('Linux') > -1;
        },

        isIphoneX() {
            if (/iphone/gi.test(navigator.userAgent)) {
                return (screen.height == 812 && screen.width == 375) || (screen.height == 896 && screen.width == 414);
            }
            return false;
        },

        /**拷贝到粘贴板 */
        copyToClipBoard(cb, content) {
            var input = content;
            const el = document.createElement('textarea');
            el.value = input;
            el.setAttribute('readonly', '');
            el.style.contain = 'strict';
            el.style.position = 'absolute';
            el.style.left = '-9999px';
            el.style.fontSize = '12pt'; // Prevent zooming on iOS
            const selection = getSelection();
            var originalRange = false;
            if (selection.rangeCount > 0) {
                originalRange = selection.getRangeAt(0);
            }
            document.body.appendChild(el);
            el.select();
            el.selectionStart = 0;
            el.selectionEnd = input.length;
            var success = false;
            try {
                success = document.execCommand('copy');
                cb();
            } catch (err) {}
            document.body.removeChild(el);
            if (originalRange) {
                selection.removeAllRanges();
                selection.addRange(originalRange);
            }
        },

    };
}();

xyMini.plat = xyMini.getHttpParams("yyplat") || 'xiaoyou'; //分发平台
xyMini.customLoginBg = null;

var yyGcid = xyMini.getHttpParams("gcid") || 9999;
var yyUid = xyMini.getHttpParams('yyUid');
if (yyUid) {
    var [gcid, snum, account] = yyUid.split('_');
    yyGcid = gcid;
}

xyMini.yyGcid = yyGcid;

// xyMini.plat && xyMini.loadScript(xyMini.cdnRoot + 'sdk/js/' + xyMini.plat + '.js?v=' + Math.random());
xyMini.plat && xyMini.loadScript('sdk/js/' + xyMini.plat + '.js?v=' + Math.random());


var filesTools = {
    jsFileList: {},
    insertScript: function(url, callback, delay) {
        this.jsFileList = this.jsFileList || {};

        function loadJs() {
            const d = document;
            let s = d.createElement('script');
            s.type = 'text/javascript';
            s.charset = 'utf-8';
            if (s.readyState) {
                // IE
                s.onreadystatechange = function() {
                    if (s.readyState === 'loaded' || s.readyState === 'complete') {
                        s.onreadystatechange = null;
                        Pass.jsFileList[url] = true;
                        callback && callback();
                    }
                };
            } else {
                s.onload = function() {
                    this.jsFileList[url] = true;
                    callback && callback();
                };
            }
            s.src = url;
            d.getElementsByTagName('head')[0].appendChild(s);
        }

        if (!this.jsFileList[url]) {
            if (delay) {
                setTimeout(function() {
                    loadJs();
                }, 100);
            } else {
                loadJs();
            }
        } else {
            callback && callback();
        }
    },
    loadCssFileW: function(url, callback) {
        window._loadedFilesW = window._loadedFilesW || {};
        if (!window._loadedFilesW[url]) {
            window._loadedFilesW[url] = true;
            let l = document.createElement('link');
            l.rel = 'stylesheet';
            l.type = 'text/css';
            l.href = url;
            document.getElementsByTagName('head')[0].appendChild(l);
            if (l.readyState) {
                // IE
                l.onreadystatechange = function() {
                    if (l.readyState === 'loaded' || l.readyState === 'complete') {
                        l.onreadystatechange = null;
                        callback && callback();
                    }
                };
            } else {
                l.onload = function() {
                    callback && callback();
                };
            }
        } else {
            callback && callback();
        }
    }
};

var gdiv = document.getElementById('GameDiv');
var gameContainer = document.getElementById('Cocos3dGameContainer');
var gCanvas = document.getElementById('GameCanvas');
var designAspect = 720 / 1500;
var shortAspect = 720 / 1280;
var foldScreenAspect = 0.5625;
var foldScreenMaxAspect = 1.414;
var isSucai = xyMini.getHttpParams('isSucai') == 1;
xyMini.isSucai = isSucai;

var isMobile = !!(navigator.userAgentData && navigator.userAgentData.mobile) ||
    /Mobi|Android|iPhone|iPad|iPod/i.test(navigator.userAgent);
xyMini.isMobile = isMobile;

var viewportScaleEpsilon = 0.01;
var viewportRecoverDelay = 600;
var viewportRecoverTimer = 0;
var viewportResetPending = false;
var activeCocosEditBox = null;
var viewportMeta = document.getElementById('GameViewport') ||
    document.querySelector('meta[name="viewport"]');
var canonicalViewportContent = viewportMeta ? viewportMeta.getAttribute('content') : '';

function isCocosEditBox(target) {
    if (!target || !target.classList || !target.classList.contains('cocosEditBox')) {
        return false;
    }
    var tagName = target.tagName ? target.tagName.toLowerCase() : '';
    return tagName === 'input' || tagName === 'textarea';
}

function replaceViewportScale(content, name, value) {
    var pattern = new RegExp('(^|,)\\s*' + name + '\\s*=\\s*[^,]*', 'i');
    if (pattern.test(content)) {
        return content.replace(pattern, '$1' + name + '=' + value);
    }
    return content + (content ? ',' : '') + name + '=' + value;
}

function buildViewportScaleContent(content, value) {
    content = replaceViewportScale(content, 'initial-scale', value);
    content = replaceViewportScale(content, 'minimum-scale', value);
    return replaceViewportScale(content, 'maximum-scale', value);
}

function dispatchViewportResize() {
    var resizeEvent;
    if (typeof Event === 'function') {
        resizeEvent = new Event('resize');
    } else {
        resizeEvent = document.createEvent('Event');
        resizeEvent.initEvent('resize', false, false);
    }
    window.dispatchEvent(resizeEvent);
}

function resetVisualViewportScale() {
    var visualViewport = window.visualViewport;
    if (!isMobile || !visualViewport || viewportResetPending) {
        return false;
    }
    var currentScale = Number(visualViewport.scale);
    if (!isFinite(currentScale) ||
        Math.abs(currentScale - 1) <= viewportScaleEpsilon) {
        return false;
    }

    viewportMeta = document.getElementById('GameViewport') ||
        document.querySelector('meta[name="viewport"]');
    if (!viewportMeta) {
        return false;
    }
    canonicalViewportContent = canonicalViewportContent ||
        viewportMeta.getAttribute('content') ||
        '';
    viewportResetPending = true;

    try {
        viewportMeta.setAttribute(
            'content',
            buildViewportScaleContent(canonicalViewportContent, '0.99')
        );
        // 强制浏览器提交一次不同的初始缩放约束，再恢复项目的标准约束。
        document.documentElement.clientWidth;
        viewportMeta.setAttribute('content', canonicalViewportContent);
        document.documentElement.clientWidth;
        window.scrollTo(0, 0);
    } catch (error) {
        viewportMeta.setAttribute('content', canonicalViewportContent);
        viewportResetPending = false;
        return false;
    }

    var requestFrame = window.requestAnimationFrame || function(callback) {
        return window.setTimeout(callback, 16);
    };
    requestFrame(function() {
        requestFrame(function() {
            viewportResetPending = false;
            window.scrollTo(0, 0);
            applyGameDivSize();
            dispatchViewportResize();
        });
    });
    return true;
}

function onCocosEditBoxFocusIn(event) {
    if (!isMobile || !isCocosEditBox(event.target)) {
        return;
    }
    activeCocosEditBox = event.target;
    if (viewportRecoverTimer) {
        window.clearTimeout(viewportRecoverTimer);
        viewportRecoverTimer = 0;
    }
}

function onCocosEditBoxFocusOut(event) {
    if (!isMobile || !isCocosEditBox(event.target)) {
        return;
    }
    if (activeCocosEditBox === event.target) {
        activeCocosEditBox = null;
    }
    if (viewportRecoverTimer) {
        window.clearTimeout(viewportRecoverTimer);
    }
    viewportRecoverTimer = window.setTimeout(function() {
        viewportRecoverTimer = 0;
        if (activeCocosEditBox || isCocosEditBox(document.activeElement)) {
            return;
        }
        if (!resetVisualViewportScale()) {
            window.scrollTo(0, 0);
        }
    }, viewportRecoverDelay);
}

function applyGameDivSize() {
    // GameDiv 的初始高度可能先触发纵向滚动条。必须先隐藏滚动条再读取
    // clientWidth，否则 Cocos SubFrame 会按扣除滚动条后的宽度初始化。
    document.documentElement.style.overflow = 'hidden';
    document.body.style.overflow = 'hidden';

    var clientWidth = document.documentElement.clientWidth;
    var clientHeight = document.documentElement.clientHeight;
    var aspect = clientHeight > 0 ? clientWidth / clientHeight : 0;
    var isFoldScreen = !isSucai &&
        aspect > foldScreenAspect &&
        aspect <= foldScreenMaxAspect;
    var useFullWindow = isMobile || aspect < designAspect || isFoldScreen;
    xyMini.isFoldScreen = isFoldScreen;

    gdiv.style.margin = '0px auto';
    if (useFullWindow) {
        filesTools.loadCssFileW('./style.mobile.css?v=2');
        gdiv.style.display = 'flex';
        gdiv.style.justifyContent = 'center';
        gdiv.style.alignItems = 'center';
        // 移动端旋转必须交给 Cocos ScreenAdapter，确保画面与触摸坐标使用同一方向。
        if (!isMobile) {
            gdiv.style.transform = 'rotate(0deg)';
            gdiv.style.webkitTransform = 'rotate(0deg)';
        }
        gdiv.style.width = Math.floor(clientWidth) + 'px';
        gdiv.style.height = Math.floor(clientHeight) + 'px';
        gdiv.setAttribute('width', '100%');
        gdiv.setAttribute('height', '100%');
    } else {
        gdiv.style.display = 'block';
        gdiv.style.justifyContent = '';
        gdiv.style.alignItems = '';
        gdiv.style.transform = '';
        gdiv.style.webkitTransform = '';
        gdiv.style.height = Math.floor(clientHeight) + 'px';
        gdiv.style.width = Math.floor((isSucai ? shortAspect : designAspect) * clientHeight) + 'px';
        gdiv.removeAttribute('width');
        gdiv.removeAttribute('height');
    }
}

xyMini.refreshGameDivSize = applyGameDivSize;
window.addEventListener('resize', applyGameDivSize);
document.addEventListener('focusin', onCocosEditBoxFocusIn, true);
document.addEventListener('focusout', onCocosEditBoxFocusOut, true);
resetVisualViewportScale();
applyGameDivSize();

if (window['FingerprintJS']) {
    window.FingerprintJS.load()
        .then(fp => fp.get())
        .then(result => {
            const visitorId = result.visitorId;
            window['visitorId'] = visitorId;
        });
}