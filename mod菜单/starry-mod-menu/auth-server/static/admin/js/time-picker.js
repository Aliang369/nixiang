/* ============================================================
 * 自定义时间选择器组件（time-picker）— 从 index.html 抽出
 * 日历 + 时分 + 快捷预设一体；依赖主脚本运行时的 escapeHtml/closeAllPickers
 * 挂载：弹窗内 openModal → initTimePickers；静态区 showApp → initStaticTimePickers
 * ============================================================ */
    /* -------- 自定义时间选择器（2026-09-06 重写） --------
       原生 datetime-local 在暗色主题/老内核下不可用（图标不可见、降级文本），弃用。
       组件：触发器（未设置=空占位）→ 下拉面板 = 日历网格 + 时/分下拉 + 快捷预设，一体。
       值保存在根节点 data-value（本地 'YYYY-MM-DDTHH:MM'），保存时经 tpISOString 转 UTC。 */
    const TP_WEEKDAYS = ['一', '二', '三', '四', '五', '六', '日'];

    function tpParseLocal(v) {
      const m = /^(\d{4})-(\d{2})-(\d{2})[T ](\d{2}):(\d{2})/.exec(String(v || '').trim());
      return m ? new Date(+m[1], +m[2] - 1, +m[3], +m[4], +m[5]) : null;
    }
    function tpFmt(d) {
      const p = n => String(n).padStart(2, '0');
      return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
    }
    /** 本月网格（周一起始），固定 42 格，前后月补位由调用方标 is-out */
    function tpBuildMonth(y, m) {
      const first = new Date(y, m, 1);
      const start = new Date(y, m, 1 - ((first.getDay() + 6) % 7));
      const cells = [];
      for (let i = 0; i < 42; i++) {
        const d = new Date(start.getFullYear(), start.getMonth(), start.getDate() + i);
        cells.push({ y: d.getFullYear(), m: d.getMonth(), d: d.getDate() });
      }
      return cells;
    }
    function tpPresetValue(v, act) {
      if (act === 'clear') return '';
      const base = tpParseLocal(v) || new Date();
      if (act === 'now') return tpFmt(new Date());
      const step = { h1: 3600, d1: 86400, d7: 604800 }[act];
      if (!step) return v || '';
      return tpFmt(new Date(base.getTime() + step * 1000));
    }
    const TP_PRESETS_DATETIME = [['clear', '清除'], ['now', '此刻'], ['h1', '+1小时'], ['d1', '+1天'], ['d7', '+7天']];
    function timePickerMarkup(id, opts = {}) {
      const presets = TP_PRESETS_DATETIME
        .map(([k, label]) => `<button type="button" class="btn ghost" data-act="${k}">${label}</button>`).join('');
      return `<div class="time-picker" data-tp="${escapeHtml(id)}" data-value="${escapeHtml(opts.value || '')}" data-empty="${escapeHtml(opts.emptyLabel || '未设置')}">
        <button type="button" class="time-picker-trigger"><span class="tp-label is-placeholder"></span><span class="gp-caret">▾</span></button>
        <div class="time-picker-panel">
          <div class="tp-head">
            <button type="button" class="btn ghost tp-nav" data-act="prev">‹</button>
            <span class="tp-title"></span>
            <button type="button" class="btn ghost tp-nav" data-act="next">›</button>
          </div>
          <div class="tp-grid"></div>
          <div class="tp-time">
            <select class="tp-h"></select><span class="tp-colon">:</span><select class="tp-m"></select>
          </div>
          <div class="tp-presets">${presets}</div>
        </div>
      </div>`;
    }
    function getPickerRootTp(id) {
      return document.querySelector(`.time-picker[data-tp="${id}"]`);
    }
    /** 保存用：未设置 → null；已设置 → 本地时间转 UTC ISO。
     *  endOfDay=true 用于"结束时间"筛选：当选的是 00:00（只点了日期没调时间）时
     *  自动扩到当天 23:59:59.999 保证含当天；用户显式选了具体时刻则按所选时刻。 */
    function tpISOString(id, endOfDay) {
      const root = getPickerRootTp(id);
      const v = root ? root.dataset.value : '';
      const d = tpParseLocal(v);
      if (!d) return null;
      if (endOfDay && d.getHours() === 0 && d.getMinutes() === 0) d.setHours(23, 59, 59, 999);
      return d.toISOString();
    }
    function tpRefreshTrigger(root) {
      const label = root.querySelector('.tp-label');
      const v = root.dataset.value || '';
      const d = tpParseLocal(v);
      if (d) {
        const p = n => String(n).padStart(2, '0');
        label.textContent = `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())} ${p(d.getHours())}:${p(d.getMinutes())}`;
        label.classList.remove('is-placeholder');
      } else {
        label.textContent = root.dataset.empty || '未设置';
        label.classList.add('is-placeholder');
      }
    }
    function tpSyncControls(root) {
      const v = root.dataset.value || '';
      const d = tpParseLocal(v);
      // 关键：面板打开时挂在 body 上（fixed 定位），不在 root 子树里——
      // 必须从 root._panel 查询日历/标题/时分，否则查到 null → 月份切换/选中高亮全部静默失效
      const panel = root._panel || root.querySelector('.time-picker-panel');
      if (!panel) return;
      const h = panel.querySelector('.tp-h'), m = panel.querySelector('.tp-m');
      if (h && m) {
        if (d) { h.value = String(d.getHours()); m.value = String(d.getMinutes()); }
        else { h.value = ''; m.value = ''; }
      }
      const title = panel.querySelector('.tp-title');
      if (title) title.textContent = `${root._view.y}年${root._view.m + 1}月`;
      const grid = panel.querySelector('.tp-grid');
      if (!grid) return;
      const today = new Date();
      const sel = tpParseLocal(v);
      grid.innerHTML = TP_WEEKDAYS.map(w => `<div class="tp-wd">${w}</div>`).join('')
        + tpBuildMonth(root._view.y, root._view.m).map(c => {
          const out = c.m !== root._view.m;
          const isToday = c.y === today.getFullYear() && c.m === today.getMonth() && c.d === today.getDate();
          const isSel = sel && c.y === sel.getFullYear() && c.m === sel.getMonth() && c.d === sel.getDate();
          return `<div class="tp-day${out ? ' is-out' : ''}${isToday ? ' is-today' : ''}${isSel ? ' is-sel' : ''}" data-d="${c.y}-${String(c.m + 1).padStart(2, '0')}-${String(c.d).padStart(2, '0')}">${c.d}</div>`;
        }).join('');
    }
    function tpSetValue(root, dateOrNull) {
      root.dataset.value = dateOrNull ? tpFmt(dateOrNull) : '';
      tpRefreshTrigger(root);
      tpSyncControls(root);
    }
    function ensureTimePicker(root) {
      if (root.dataset.ready === '1') return;
      root.dataset.ready = '1';
      const panel = root.querySelector('.time-picker-panel');
      root._panel = panel;
      panel.addEventListener('click', e => e.stopPropagation());
      panel.addEventListener('mousedown', e => e.stopPropagation());
      const h = panel.querySelector('.tp-h'), m = panel.querySelector('.tp-m');
      h.innerHTML = '<option value="">时</option>' + Array.from({ length: 24 }, (_, i) => `<option value="${i}">${String(i).padStart(2, '0')}</option>`).join('');
      m.innerHTML = '<option value="">分</option>' + Array.from({ length: 60 }, (_, i) => `<option value="${i}">${String(i).padStart(2, '0')}</option>`).join('');
      h.addEventListener('change', () => {
        if (h.value === '') return;
        const cur = tpParseLocal(root.dataset.value) || new Date();
        cur.setHours(+h.value);
        tpSetValue(root, cur);
      });
      m.addEventListener('change', () => {
        if (m.value === '') return;
        const cur = tpParseLocal(root.dataset.value) || new Date();
        cur.setMinutes(+m.value);
        tpSetValue(root, cur);
      });
      root.querySelector('.time-picker-trigger').addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        const willOpen = !root.classList.contains('open');
        closeAllPickers(root);
        if (!willOpen) return;
        root.classList.add('open');
        const base = tpParseLocal(root.dataset.value) || new Date();
        root._view = { y: base.getFullYear(), m: base.getMonth() };
        tpSyncControls(root);
        tpPlacePanel(root);
      });
      panel.addEventListener('click', (e) => {
        const day = e.target.closest('.tp-day:not(.is-out)');
        if (day) {
          const cur = tpParseLocal(root.dataset.value);
          const [y, mo, d] = day.dataset.d.split('-').map(Number);
          const next = new Date(y, mo - 1, d, cur ? cur.getHours() : 0, cur ? cur.getMinutes() : 0);
          tpSetValue(root, next);
          closeTimePicker(root);   // 选完日期自动收起（时分保留可再开细调）
          return;
        }
        const act = e.target.closest('[data-act]')?.dataset.act;
        if (!act) return;
        if (act === 'prev') { root._view.m--; if (root._view.m < 0) { root._view.m = 11; root._view.y--; } tpSyncControls(root); return; }
        if (act === 'next') { root._view.m++; if (root._view.m > 11) { root._view.m = 0; root._view.y++; } tpSyncControls(root); return; }
        if (act === 'clear') { tpSetValue(root, null); closeTimePicker(root); return; }
        tpSetValue(root, tpParseLocal(tpPresetValue(root.dataset.value, act)));
        closeTimePicker(root);     // 预设（此刻/+N）选完自动收起
      });
      tpRefreshTrigger(root);
    }
    function tpPlacePanel(root) {
      const panel = root._panel;
      const trigger = root.querySelector('.time-picker-trigger');
      if (!panel || !trigger) return;
      document.body.appendChild(panel);
      panel.classList.add('is-open');
      const rect = trigger.getBoundingClientRect();
      let left = rect.left, top = rect.bottom + 6;
      panel.style.width = '288px';
      panel.style.left = left + 'px';
      panel.style.top = top + 'px';
      panel.style.bottom = 'auto';
      panel.style.right = 'auto';
      const pr = panel.getBoundingClientRect(), pad = 8;
      if (pr.right > window.innerWidth - pad) { left = Math.max(pad, window.innerWidth - pr.width - pad); panel.style.left = left + 'px'; }
      if (pr.bottom > window.innerHeight - pad) {
        const upTop = rect.top - pr.height - 6;
        panel.style.top = (upTop >= pad ? upTop : pad) + 'px';
      }
    }
    function closeTimePicker(root) {
      root.classList.remove('open');
      const panel = root._panel;
      if (!panel) return;
      panel.classList.remove('is-open');
      if (panel.parentElement !== root) root.appendChild(panel);
    }
    function initTimePickers() {
      document.querySelectorAll('.time-picker[data-tp]').forEach(root => {
        ensureTimePicker(root);
        tpRefreshTrigger(root);
      });
    }
