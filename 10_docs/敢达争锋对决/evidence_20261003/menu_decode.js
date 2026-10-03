// 菜单 exp 解码：在 vm 沙箱里跑 app1.js（自返回 Proxy 假 DOM），调用 initGame/strencode 提取功能表
const fs = require('fs'), vm = require('vm'), path = require('path');
const DIR = __dirname;
const html = fs.readFileSync(path.join(DIR, 'menu_578.html'), 'utf8');
const js = fs.readFileSync(path.join(DIR, 'menu_app1.js'), 'utf8');
const m = html.match(/var\s+exp\s*=\s*"([^"]+)"/);
const exp = m[1];
const gid = (html.match(/game_id\s*=\s*"([^"]+)"/) || [])[1];

const calls = [];
let chars = 0, stop = false;
function ser(a) {
  if (a === null || a === undefined) return String(a);
  if (typeof a === 'string') return JSON.stringify(a.slice(0, 400));
  if (typeof a === 'number' || typeof a === 'boolean') return String(a);
  if (typeof a === 'function') return '[fn]';
  try { if (a.__desc) return a.__desc; } catch (e) {}
  return Object.prototype.toString.call(a);
}
function rec(name, args) {
  if (stop) return;
  const line = name + '  < ' + args.map(ser).join(' , ');
  chars += line.length;
  if (chars > 4000000 || calls.length > 20000) { stop = true; return; }
  calls.push(line);
}
function fake(name) {
  const f = function () {};
  return new Proxy(f, {
    get(t, p) {
      if (p === Symbol.toPrimitive) return () => name;
      if (p === 'toString') return () => name;
      if (p === 'valueOf') return () => name;
      if (p === Symbol.iterator) return function* () {};
      if (p === 'then') return undefined;
      if (p === 'length') return 0;
      if (p === '__desc') return name;
      if (p === Symbol.toStringTag) return 'Object';
      return fake(name + '.' + String(p));
    },
    set() { return true; },
    apply(t, ths, args) { rec(name + '()', args); return fake(name + '()'); },
    construct(t, args) { rec('new ' + name, args); return fake('new ' + name); },
    has() { return true; }
  });
}
const sandbox = {};
const P = new Proxy(sandbox, {
  get(t, p) {
    if (p in t) return t[p];
    if (p === 'window' || p === 'self' || p === 'globalThis' || p === 'top' || p === 'parent') return P;
    if (p === 'document') return fake('document');
    if (p === '$' || p === 'jQuery') return fake('$');
    if (p === 'console') return { log: (...a) => rec('console.log', a), error: (...a) => rec('console.error', a), warn: () => {}, info: () => {} };
    if (p === 'alert' || p === 'confirm' || p === 'prompt') return (...a) => { rec(p, a); return true; };
    if (p === 'setTimeout' || p === 'setInterval') return (fn) => { rec(p, [String(fn).slice(0, 80)]); return 0; };
    if (p === 'clearTimeout' || p === 'clearInterval') return () => {};
    if (p === 'JSON' || p === 'Math' || p === 'Object' || p === 'Array' || p === 'String' || p === 'Number' || p === 'Boolean' || p === 'Date' || p === 'RegExp' || p === 'Error' || p === 'Promise' || p === 'parseInt' || p === 'parseFloat' || p === 'isNaN' || p === 'encodeURIComponent' || p === 'decodeURIComponent' || p === 'unescape' || p === 'escape' || p === 'atob' || p === 'btoa') return globalThis[p];
    if (p === 'navigator') return fake('navigator');
    if (p === 'location') return fake('location');
    if (p === 'localStorage' || p === 'sessionStorage') return fake(String(p));
    if (p === 'game_id') return gid;
    if (p === 'version') return '1';
    if (p === 'version_tip' || p === 'tUrl') return '';
    if (p === 'Android') return fake('Android');
    if (p === Symbol.unscopables) return undefined;
    return undefined;
  },
  set(t, p, v) {
    t[p] = v;
    if (typeof v === 'function' && !/^_0x/.test(String(p))) rec('DEF ' + String(p), []);
    return true;
  },
  has() { return true; }
});
const ctx = vm.createContext(P);
try { vm.runInContext(js, ctx, { filename: 'app1.js', timeout: 30000 }); }
catch (e) { console.log('## load error:', e.message); }
console.log('## game_id =', gid, ' exp len =', exp.length);
for (const fn of ['strencode']) {
  try { const r = ctx[fn](exp); console.log('## ' + fn + '(exp) =>', (typeof r === 'string' ? r.slice(0, 8000) : String(r))); }
  catch (e) { console.log('## ' + fn + ' threw:', e.message); }
}
try { ctx.initGame(exp); } catch (e) { console.log('## initGame threw:', e.message); }
console.log('## captured calls:', calls.length);
fs.writeFileSync(path.join(DIR, 'menu_calls.txt'), calls.join('\n'), 'utf8');
console.log(calls.slice(0, 60).join('\n'));
