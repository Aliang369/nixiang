/* 前端「未改时间不提交 expire_at」回归（2026-11-12）。
   直接 vm 加载真实 static/admin/js/cards.js，调用 cardExpirePatch；
   parseServerTime 按 static/admin/index.html:785 原样复制（浏览器里由 index.html 提供）。 */
import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';

const cardsPath = new URL('../static/admin/js/cards.js', import.meta.url);
const src = fs.readFileSync(cardsPath, 'utf8');

const ctx = vm.createContext({
  console,
  document: { getElementById: () => null, querySelector: () => null },
  window: {},
  parseServerTime(v) {
    if (!v) return null;
    let s = String(v).trim();
    if (!s) return null;
    if (!/[zZ]$|[+-]\d{2}:?\d{2}$/.test(s)) s = s.replace(' ', 'T') + 'Z';
    const d = new Date(s);
    return isNaN(d.getTime()) ? null : d;
  },
});
vm.runInContext(src, ctx, { filename: 'cards.js' });
const patch = ctx.cardExpirePatch;
assert.equal(typeof patch, 'function', 'cardExpirePatch 未定义');

const orig = '2026-11-12T16:00:00.123456+00:00';      // 服务端原值（带微秒）
const sameMinute = '2026-11-12T16:00:00.000Z';          // 选择器回填后原样保存的分钟值
const changed = '2026-11-13T16:00:00.000Z';
const earlier = '2026-11-11T16:00:00.000Z';

assert.equal(patch(orig, sameMinute), undefined, '同分钟应不提交 expire_at');
assert.equal(patch(orig, changed), changed, '改了时间应提交新值');
assert.equal(patch(orig, earlier), earlier, '改早也应提交（不静默忽略用户输入）');
assert.equal(patch(orig, null), null, '清除到期时间应提交 null');
assert.equal(patch(null, null), undefined, '原本无到期时间且未选 → 不提交');
assert.equal(patch(null, changed), changed, '原本无到期时间、新选 → 提交');
assert.equal(patch('2026-11-12T16:00:00+08:00', '2026-11-12T08:00:00.000Z'), undefined, '时区不同但同一时刻应视为未改');

console.log('expire_patch: 7/7 passed');
