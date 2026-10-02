var done=false;
var iv=setInterval(function(){
  var m=Process.findModuleByName('libil2cpp.so');
  if(!m) return;
  done=true; clearInterval(iv);
  function hex(p,n){ return Array.prototype.map.call(new Uint8Array(p.readByteArray(n)),function(b){return ('0'+b.toString(16)).slice(-2);}).join(' '); }
  [[0x11832AC,'Assembly.Load(byte[])'],[0x11832E0,'Assembly.Load(byte[],byte[])'],[0x14F1CFC,'LoadMetadataAOT'],[0x118331C,'Assembly.Load(b,b,Ev)']].forEach(function(c){
    var a=m.base.add(c[0]);
    send({t:'log', m:c[1]+' @ '+a+' : '+hex(a,32)});
  });
},500);
setTimeout(function(){ send({t:'done',v:done}); },60000);
