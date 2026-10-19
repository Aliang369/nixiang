// fg_v2watch.js — 抓「谁读 APK v2 签名块」的读取者 PC（绿梦 FairGuard）
// 用法: frida -U -f com.zhuziplay.lvmengnew.op -l 04_scripts/绿梦/fg_v2watch.js -q
// 原理: 轮询 /proc/self/maps 直到 v2 块所在的那段 base.apk 映射出现，再用
//       MemoryAccessMonitor 接管缺页 —— details.from = 发起访问的指令地址。
//       只读脚本，不改任何字节；命中 64 次自动关闭。

const TARGET_OFFSET = 'ab36000';
const DEDUP = {};
let hits = 0;
let armed = false;

function findRange() {
  const maps = File.readAllText('/proc/self/maps').split('\n');
  for (const line of maps) {
    if (line.indexOf('base.apk') < 0) continue;
    const p = line.trim().split(/\s+/);
    if (p.length < 3) continue;
    if (p[2].toLowerCase().indexOf(TARGET_OFFSET) !== 0) continue;
    const se = p[0].split('-');
    return { base: ptr('0x' + se[0]), end: ptr('0x' + se[1]), line: line };
  }
  return null;
}

function describe(addr) {
  try {
    const m = Process.findModuleByAddress(addr);
    if (m) return m.name + '+0x' + addr.sub(m.base).toString(16);
    return addr.toString();
  } catch (e) { return String(addr); }
}

function arm(r) {
  console.log('[*] target range ' + r.base + '..' + r.end + '  (' + r.line.trim() + ')');
  MemoryAccessMonitor.enable({ base: r.base, size: r.end.sub(r.base).toInt32() }, {
    onAccess(d) {
      hits++;
      const key = d.from.toString();
      if (DEDUP[key] === undefined) {
        DEDUP[key] = hits;
        console.log('[HIT #' + hits + '] op=' + d.operation +
                    '  from=' + d.from + ' (' + describe(d.from) + ')' +
                    '  access=' + d.address + '  range=' + d.first + '..' + d.last);
      }
      if (hits >= 64) { MemoryAccessMonitor.disable(); console.log('[*] 64 hits，已关闭监视'); }
    }
  });
  console.log('[*] 监视已开启');
}

let tries = 0;
const iv = setInterval(() => {
  tries++;
  const r = findRange();
  if (r !== null) { clearInterval(iv); arm(r); armed = true; return; }
  if (tries % 25 === 0) console.log('[*] 等待 base.apk 映射… ' + (tries * 0.2).toFixed(0) + 's');
  if (tries > 300) { clearInterval(iv); console.log('[!] 60s 内没找到映射'); }
}, 200);
