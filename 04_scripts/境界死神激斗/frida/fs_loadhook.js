// 定位本作热更 DLL 装载点：hook 一组候选 AOT 方法，打印 byte[] 参数特征
var T0 = Date.now();
var CAND = [
  ['Assembly.Load(byte[])',        0x11832AC],
  ['Assembly.Load(byte[],byte[])', 0x11832E0],
  ['Assembly.Load(bytes,bytes,Ev)',0x118331C],
  ['Assembly.LoadAssemblyRaw',     0xD7EE60],
  ['RuntimeApi.LoadMetadataAOT',   0x14F1CFC],
];
var hooked = false;
function tryHook() {
  if (hooked) return;
  var m = Process.findModuleByName('libil2cpp.so');
  if (!m) return;
  hooked = true;
  send({t:'log', m:'libil2cpp base=' + m.base + ' size=' + m.size});
  CAND.forEach(function (c) {
    var name = c[0], rva = c[1];
    try {
      var addr = m.base.add(rva);
      Interceptor.attach(addr, {
        onEnter: function (args) {
          var a0 = args[0], a1 = args[1];
          var out = { t:'log', m:(Date.now()-T0)+'ms HIT ' + name + ' x0=' + a0 + ' x1=' + a1 };
          try {
            var p = a0;
            if (p && !p.isNull()) {
              var len = p.add(0x18).readS32();
              var data = p.add(0x20);
              var head = 'n/a';
              try { head = data.readByteArray(8); } catch (e) {}
              out.m += ' arr.len=' + len + ' head=' + (head ? Array.prototype.map.call(new Uint8Array(head), function(b){return ('0'+b.toString(16)).slice(-2);}).join(' ') : '?');
            }
          } catch (e) { out.m += ' ERR ' + e; }
          send(out);
        }
      });
      send({t:'log', m:'attached ' + name + ' @ ' + addr});
    } catch (e) {
      send({t:'log', m:'attach FAIL ' + name + ' : ' + e});
    }
  });
  // 同时打印 il2cpp 里 MZ 大数组区域，便于对照
}
var iv = setInterval(function () {
  tryHook();
  if (hooked) clearInterval(iv);
}, 500);
setTimeout(function () { send({t:'done', v: hooked}); }, 90000);
