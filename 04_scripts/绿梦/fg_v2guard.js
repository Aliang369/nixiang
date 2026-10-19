// fg_v2guard.js — 把 v2 签名块所在映射置 PROT_NONE，让读取者直接 SIGSEGV（墓碑 PC = 读取指令）
// 用法: frida -U -f com.zhuziplay.lvmengnew.op -l 04_scripts/绿梦/fg_v2guard.js -q
const TARGET_OFFSET = 'ab36000';
console.log('[load] fg_v2guard 已注入');

function findRange() {
  try {
    const maps = File.readAllText('/proc/self/maps').split('\n');
    for (const line of maps) {
      if (line.indexOf('base.apk') < 0) continue;
      const p = line.trim().split(/\s+/);
      if (p.length < 3) continue;
      if (parseInt(p[2], 16) !== 0xab36000) continue;
      const se = p[0].split('-');
      return { base: ptr('0x' + se[0]), end: ptr('0x' + se[1]), line: line.trim() };
    }
  } catch (e) { console.log('[!] maps 读取失败: ' + e); }
  return null;
}

setImmediate(function () {
  let r = null;
  for (let i = 0; i < 400 && r === null; i++) {
    r = findRange();
    if (r === null) Thread.sleep(0.2);
  }
  if (r === null) { console.log('[!] 80s 内没等到 base.apk 映射'); return; }
  const size = r.end.sub(r.base).toInt32();
  console.log('[*] range ' + r.base + '..' + r.end + '  ' + r.line);
  try {
    Memory.protect(r.base, size, '---');
    console.log('[*] 已置 PROT_NONE（' + size + ' B），等读取者 SIGSEGV');
  } catch (e) { console.log('[!] protect 失败: ' + e); }
  setInterval(function () {}, 1000);
});
