// 目标：定位 Singleton<LevelManager> / Singleton<GameManager> 的实例化类与 get_Instance
'use strict';
var il2cppMod = null, api = {};
function bind() {
  if (il2cppMod) return true;
  il2cppMod = Process.findModuleByName('libil2cpp.so');
  if (!il2cppMod) return false;
  function E(n) { return il2cppMod.findExportByName(n); }
  api.domain_get   = new NativeFunction(E('il2cpp_domain_get'), 'pointer', []);
  api.domain_asm   = new NativeFunction(E('il2cpp_domain_get_assemblies'), 'pointer', ['pointer','pointer']);
  api.asm_img      = new NativeFunction(E('il2cpp_assembly_get_image'), 'pointer', ['pointer']);
  api.img_name     = new NativeFunction(E('il2cpp_image_get_name'), 'pointer', ['pointer']);
  api.img_nclass   = new NativeFunction(E('il2cpp_image_get_class_count'), 'uint', ['pointer']);
  api.img_class    = new NativeFunction(E('il2cpp_image_get_class'), 'pointer', ['pointer','uint']);
  api.cls_name     = new NativeFunction(E('il2cpp_class_get_name'), 'pointer', ['pointer']);
  api.cls_ns       = new NativeFunction(E('il2cpp_class_get_namespace'), 'pointer', ['pointer']);
  api.cls_getm     = new NativeFunction(E('il2cpp_class_get_method_from_name'), 'pointer', ['pointer','pointer','int']);
  api.cls_static   = new NativeFunction(E('il2cpp_class_get_static_field_data'), 'pointer', ['pointer']);
  return true;
}
setTimeout(function () {
  if (!bind()) { send({t:'log', m:'no libil2cpp'}); return; }
  var d = api.domain_get(), n = ptr(0);
  var asms = api.domain_asm(d, n);
  var cnt = n.readU32(), want = null;
  for (var i = 0; i < cnt; i++) {
    var img = api.asm_img(asms.add(i * Process.pointerSize).readPointer());
    var nm = api.img_name(img).readUtf8String();
    if (nm && nm.indexOf('Assembly-CSharp') === 0 && nm.indexOf('firstpass') < 0) { want = img; break; }
  }
  if (!want) { send({t:'log', m:'no Assembly-CSharp image'}); return; }
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
  send({t:'log', m:'classes=' + cc});
  out.forEach(function (s) { send({t:'log', m:'[S] ' + s}); });
  send({t:'done', n: out.length});
}, 1500);
