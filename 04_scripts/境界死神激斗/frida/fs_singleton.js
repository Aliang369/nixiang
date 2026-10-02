// 目标：定位 Singleton<LevelManager> / Singleton<GameManager> 的实例化类与 get_Instance
// 约定见同目录 README.md（Frida 17 兼容：不依赖 Module.findExportByName；调 il2cpp 前必 thread_attach）
'use strict';

// ── Frida 17 兼容的导出解析 ────────────────────────────────────────────────
// Frida 17 移除了 Module.findExportByName 静态方法：旧写法返回 null，
// 紧接着 new NativeFunction(null, …) 会以 access violation accessing 0x0 形式崩掉。
function resolveExport(mod, name) {
  var cands = [];
  if (typeof Module.getGlobalExportByName === 'function') cands.push(['Module.getGlobalExportByName', function () { return Module.getGlobalExportByName(name); }]);
  if (typeof Module.findGlobalExportByName === 'function') cands.push(['Module.findGlobalExportByName', function () { return Module.findGlobalExportByName(name); }]);
  if (typeof mod.getExportByName === 'function') cands.push(['module.getExportByName', function () { return mod.getExportByName(name); }]);
  if (typeof mod.findExportByName === 'function') cands.push(['module.findExportByName', function () { return mod.findExportByName(name); }]);
  for (var i = 0; i < cands.length; i++) {
    try { var p = cands[i][1](); if (p && !p.isNull()) return p; } catch (e) { /* 换下一个 */ }
  }
  return null;
}
function resolveApiName(mod) {
  if (typeof Module.getGlobalExportByName === 'function') return 'Module.getGlobalExportByName';
  if (typeof Module.findGlobalExportByName === 'function') return 'Module.findGlobalExportByName';
  if (typeof mod.getExportByName === 'function') return 'module.getExportByName';
  if (typeof mod.findExportByName === 'function') return 'module.findExportByName';
  return '(无可用导出 API！)';
}

var il2cppMod = null, api = {};

// [apiKey, 导出名, 返回类型, 参数类型]
var WANT = [
  ['domain_get',    'il2cpp_domain_get',                  'pointer', []],
  ['thread_attach', 'il2cpp_thread_attach',               'pointer', ['pointer']],
  ['domain_asm',    'il2cpp_domain_get_assemblies',       'pointer', ['pointer', 'pointer']],
  ['asm_img',       'il2cpp_assembly_get_image',          'pointer', ['pointer']],
  ['img_name',      'il2cpp_image_get_name',              'pointer', ['pointer']],
  ['img_nclass',    'il2cpp_image_get_class_count',       'uint',    ['pointer']],
  ['img_class',     'il2cpp_image_get_class',             'pointer', ['pointer', 'uint']],
  ['cls_name',      'il2cpp_class_get_name',              'pointer', ['pointer']],
  ['cls_ns',        'il2cpp_class_get_namespace',         'pointer', ['pointer']],
  ['cls_getm',      'il2cpp_class_get_method_from_name',  'pointer', ['pointer', 'pointer', 'int']],
  ['cls_static',    'il2cpp_class_get_static_field_data', 'pointer', ['pointer']]
];

function bind() {
  if (il2cppMod) return true;
  il2cppMod = Process.findModuleByName('libil2cpp.so');
  if (!il2cppMod) { send({ t: 'log', m: '[env] libil2cpp.so 未加载' }); return false; }

  // 自检：开头就把「解析通道 + 导出可用性」打出来，避免一声不响地崩。
  send({ t: 'log', m: '[env] frida=' + Frida.version + ' | 导出解析走 ' + resolveApiName(il2cppMod) });

  var missing = [];
  WANT.forEach(function (w) {
    var p = resolveExport(il2cppMod, w[1]);
    if (!p) { missing.push(w[1]); return; }
    api[w[0]] = new NativeFunction(p, w[2], w[3]);
  });
  send({ t: 'log', m: '[env] 导出 ' + (WANT.length - missing.length) + '/' + WANT.length +
                     ' 可用' + (missing.length ? '，缺: ' + missing.join(',') : '') });
  return missing.length === 0;
}

function findImage(prefix, excludeFirstpass) {
  var d = api.domain_get();
  api.thread_attach(d);            // ★ 调 il2cpp 前必 attach，否则解释器访问 TLS 崩（0x10）
  var n = ptr(0);
  var asms = api.domain_asm(d, n);
  var cnt = n.readU32();
  for (var i = 0; i < cnt; i++) {
    var img = api.asm_img(asms.add(i * Process.pointerSize).readPointer());
    var nm = api.img_name(img).readUtf8String();
    if (nm && nm.indexOf(prefix) === 0 && (!excludeFirstpass || nm.indexOf('firstpass') < 0)) return img;
  }
  return null;
}

setTimeout(function () {
  if (!bind()) { send({ t: 'log', m: 'probe aborted' }); send({ t: 'done', n: 0 }); return; }
  var want = findImage('Assembly-CSharp', true);
  if (!want) { send({ t: 'log', m: 'no Assembly-CSharp image' }); send({ t: 'done', n: 0 }); return; }
  var cc = api.img_nclass(want), out = [];
  for (var j = 0; j < cc; j++) {
    var k = api.img_class(want, j);
    if (k.isNull()) continue;
    var cn = api.cls_name(k).readUtf8String() || '';
    if (cn.indexOf('Singleton') >= 0 || cn === 'LevelManager' || cn === 'GameManager' || cn === 'BattleMgr') {
      var ns = api.cls_ns(k).readUtf8String() || '';
      var mi = api.cls_getm(k, Memory.allocUtf8String('get_Instance'), 0);
      var sd = api.cls_static(k);
      out.push(ns + '.' + cn + ' | klass=' + k + ' | get_Instance=' + (mi.isNull() ? '-' : mi) +
               ' | static=' + (sd.isNull() ? '-' : sd));
    }
  }
  send({ t: 'log', m: 'classes=' + cc });
  out.forEach(function (s) { send({ t: 'log', m: '[S] ' + s }); });
  send({ t: 'done', n: out.length });
}, 1500);
