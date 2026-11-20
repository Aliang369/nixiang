// 鹦鹉团 QGSDK 换票失败定位 —— 只读 hook（不改任何逻辑）
// 目标包 com.jinsaina.auditionsgo.gw，pid 由 python 侧 attach
// 输出统一前缀 [YWT]，便于 grep
'use strict';

function say(tag, s) {
  console.log(tag + ' ' + s);
}

function safe(tag, fn) {
  try { fn(); } catch (e) { say('[!]', tag + ' hook 失败: ' + e); }
}

Java.perform(function () {
  say('[INIT]', 'Java.perform ok, loader=' + Java.classFactory.loader);

  // ---- d) P6 落地后的签名四元组 -----------------------------------------
  safe('Check.getSignInfo', function () {
    var Check = Java.use('com.game.apkverify.Check');
    Check.getSignInfo.implementation = function (obj) {
      var r = this.getSignInfo(obj);
      var out;
      try { out = (r === null) ? 'null' : JSON.stringify(Array.prototype.slice.call(r)); }
      catch (e) { out = String(r); }
      say('[SIGN]', 'getSignInfo() = ' + out);
      return r;
    };
  });

  // 同一类的另一个 native（保持原样，只观察是否被调用）
  safe('Check.getAPKCharacteristic', function () {
    var Check2 = Java.use('com.game.apkverify.Check');
    Check2.getAPKCharacteristic.implementation = function (ctx, s) {
      var r = this.getAPKCharacteristic(ctx, s);
      say('[SIGN]', 'getAPKCharacteristic(arg=' + s + ') = ' + r);
      return r;
    };
  });

  // ---- b1) QGParameter.create()：最终 query string（含 signMd5）----------
  safe('QGParameter.create', function () {
    var P = Java.use('com.quickgamesdk.net.QGParameter');
    P.create.implementation = function () {
      var s = this.create();
      say('[PARAM]', 'create() = ' + s);
      return s;
    };
  });

  // ---- b2) ThirdManager.thridLogin 全重载 -------------------------------
  safe('ThirdManager.thridLogin', function () {
    var TM = Java.use('com.quickgamesdk.manager.ThirdManager');
    var ovs = TM.thridLogin.overloads;
    ovs.forEach(function (ov) {
      var sig = ov.argumentTypes.map(function (t) { return t.className; }).join(' | ');
      ov.implementation = function () {
        var args = [];
        for (var i = 0; i < arguments.length; i++) {
          var a = arguments[i];
          try { args.push(String(a)); } catch (e) { args.push('<obj>'); }
        }
        say('[THRID]', 'thridLogin(' + sig + ') args=' + JSON.stringify(args));
        return ov.apply(this, arguments);
      };
    });
  });

  // ---- a) 失败/成功回调 -------------------------------------------------
  safe('ThirdManager$2', function () {
    var T2 = Java.use('com.quickgamesdk.manager.ThirdManager$2');
    T2.onFailed.implementation = function (code, msg) {
      say('[FAIL]', 'onFailed code=' + code + ' msg=' + msg);
      return this.onFailed(code, msg);
    };
    var okOv = T2.onSuccess.overload('com.quickgamesdk.entity.QGUserInfo');
    okOv.implementation = function (u) {
      var s = '';
      try { s = u.toString(); } catch (e) { s = '<no toString>'; }
      say('[OK]', 'onSuccess QGUserInfo = ' + s);
      return okOv.call(this, u);
    };
  });

  // ---- c) jkhttp 请求/响应 ----------------------------------------------
  safe('HttpClient', function () {
    var HC = Java.use('com.quickjoy.lib.jkhttp.HttpClient');
    HC.newCall.implementation = function (req) {
      try {
        var url = req.getUrl();
        var m = req.getMethod();
        say('[HTTP-REQ]', m + ' ' + url);
        var p = req.getParameter();
        if (p !== null) say('[HTTP-PARAM]', String(p.toString()));
        var h = req.getHeader();
        if (h !== null) say('[HTTP-HEAD]', String(h.toString()));
      } catch (e) {
        say('[!]', 'newCall 取值失败: ' + e);
      }
      return this.newCall(req);
    };
    HC.excute.implementation = function () {
      var resp = this.excute();
      try {
        var code = resp.getResponseCode();
        var body = resp.asString();
        say('[HTTP-RESP]', 'code=' + code + ' body=' + body);
      } catch (e) {
        say('[!]', 'excute 读取响应失败: ' + e);
      }
      return resp;
    };
  });

  // ---- c2) jkhttp 底层 Connect.doConnect（HttpClient 不走时的兜底）----
  safe('Connect.doConnect', function () {
    var C = Java.use('com.quickjoy.lib.jkhttp.Connect');
    C.doConnect.implementation = function () {
      var r = this.doConnect();
      try {
        say('[CONN-RESP]', 'code=' + r.getResponseCode() + ' body=' + r.asString());
      } catch (e) {
        say('[!]', 'doConnect 读响应失败: ' + e);
      }
      return r;
    };
  });

  // ---- c3) 兜底：okhttp3（TapSDK 自带一份，QGSDK 若改用它也能抓到）------
  safe('okhttp3.RealCall', function () {
    var RC = Java.use('okhttp3.RealCall');
    RC.execute.implementation = function () {
      var resp = this.execute();
      try {
        var req = resp.request();
        say('[OKHTTP-REQ]', String(req.method()) + ' ' + String(req.url()));
        var b = req.body();
        if (b !== null && b !== undefined) {
          try { say('[OKHTTP-BODY]', String(b.toString())); } catch (e2) {}
        }
        var rb = resp.body();
        if (rb !== null && rb !== undefined) say('[OKHTTP-RESP]', String(rb.string()));
      } catch (e) {
        say('[!]', 'okhttp 读取失败: ' + e);
      }
      return resp;
    };
  });

  say('[INIT]', '全部 hook 安装完成（只读，不改逻辑）');
});
