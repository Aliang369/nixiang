var m = Process.findModuleByName('libil2cpp.so');
var p = function (n) { return m.getExportByName(n); };
var domainGet = new NativeFunction(p('il2cpp_domain_get'), 'pointer', []);
var threadAttach = new NativeFunction(p('il2cpp_thread_attach'), 'pointer', ['pointer']);
var asmsGet = new NativeFunction(p('il2cpp_domain_get_assemblies'), 'pointer', ['pointer', 'pointer']);
var asmImage = new NativeFunction(p('il2cpp_assembly_get_image'), 'pointer', ['pointer']);
var classFromName = new NativeFunction(p('il2cpp_class_from_name'), 'pointer', ['pointer', 'pointer', 'pointer']);
var getMethod = new NativeFunction(p('il2cpp_class_get_method_from_name'), 'pointer', ['pointer', 'pointer', 'int']);
var strNew = new NativeFunction(p('il2cpp_string_new'), 'pointer', ['pointer']);
var invoke = new NativeFunction(p('il2cpp_runtime_invoke'), 'pointer', ['pointer', 'pointer', 'pointer', 'pointer']);

var dom = domainGet();
threadAttach(dom);                       // 必须先附着，否则解释器 TLS 访问崩
var cnt = Memory.alloc(8), asms = asmsGet(dom, cnt);
var n = cnt.readU64().toNumber(), k = NULL;
for (var i = 0; i < n; i++) {
  var img = asmImage(asms.add(i * 8).readPointer());
  if (!img) continue;
  k = classFromName(img, Memory.allocUtf8String('Starry'), Memory.allocUtf8String('StarryConfig'));
  if (k && !k.isNull()) break;
}
if (!k || k.isNull()) { console.log('[X] not found'); }
else {
  var gb = getMethod(k, Memory.allocUtf8String('GetBool'), 1);
  var gi = getMethod(k, Memory.allocUtf8String('GetInt'), 2);
  ['inv', 'inv_full', 'nocd'].forEach(function (key) {
    var a = Memory.alloc(8);
    a.writePointer(strNew(Memory.allocUtf8String(key)));
    var r = invoke(gb, NULL, a, NULL);
    console.log('  GetBool("' + key + '") = ' + (r.isNull() ? 'null' : r.add(0x10).readU8()));
  });
  var a2 = Memory.alloc(16);
  a2.writePointer(strNew(Memory.allocUtf8String('dmg_mul')));
  a2.add(8).writeS32(-1);
  var r2 = invoke(gi, NULL, a2, NULL);
  console.log('  GetInt("dmg_mul",-1) = ' + (r2.isNull() ? 'null' : r2.add(0x10).readS32()));
}
