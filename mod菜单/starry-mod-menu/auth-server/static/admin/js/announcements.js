/* 管理后台 - 远程公告（从 index.html 拆出，2026-09-21）
   依赖：core.js 等在前。 */
    /* -------- announcements -------- */
    function toLocalInput(iso) {
      // 经 parseServerTime：裸串按 UTC 解析再转本地，避免回填值早 8 小时、保存即吃掉 8 小时
      const d = parseServerTime(iso);
      if (!d) return '';
      const pad = n => String(n).padStart(2, '0');
      return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }

    async function loadAnnouncements() {
      const qs = new URLSearchParams();
      const q = document.getElementById('a-q')?.value.trim() || '';
      const gameId = getGamePickerValue('a-filter-game');
      const scope = document.getElementById('a-scope')?.value || '';
      const en = boolParam(document.getElementById('a-enabled')?.value || '');
      const vis = document.getElementById('a-vis')?.value || '';
      if (q) qs.set('q', q);
      if (gameId) qs.set('game_id', gameId);
      if (scope) qs.set('scope', scope);
      if (en) qs.set('enabled', en);
      if (vis) qs.set('visibility', vis);
      qs.set('page', String(annPage));
      qs.set('page_size', String(annPageSize));
      const data = await api('/v1/admin/announcements?' + qs.toString());
      announcementsCache = data.data || [];
      annMeta = data.meta || { total: announcementsCache.length, page: annPage, page_size: annPageSize };
      annPage = annMeta.page || annPage;
      document.getElementById('ann-body').innerHTML = announcementsCache.map(a => {
        // 状态列 = 实际展示状态：开关开 + 在时间窗内 → 「显示」；否则「未显示」+原因
        const nowMs = Date.now();
        const sT = parseServerTime(a.start_at)?.getTime() ?? null;
        const eT = parseServerTime(a.end_at)?.getTime() ?? null;
        const showing = a.enabled && (sT === null || nowMs >= sT) && (eT === null || nowMs <= eT);
        return `
        <tr>
          <td><strong>${escapeHtml(a.title)}</strong><div class="muted">${escapeHtml(a.content).slice(0,80)}</div>
            ${a.action_url ? '<div class="muted">跳转：' + escapeHtml(a.action_url) + '</div>' : ''}</td>
          <td>${a.scope === 'global' ? '全游戏' : '单游戏'}${a.game_id ? ' / ' + escapeHtml(gameLabel(a.game_id)) : ''}</td>
          <td>${VIS_CN[a.visibility] || a.visibility || '始终'}</td>
          <td>${POPUP_CN[a.popup_mode] || a.popup_mode || '每次打开'}</td>
          <td>${a.start_at ? fmtLocal(a.start_at) : '<span class="muted">—</span>'}</td>
          <td>${a.end_at ? fmtLocal(a.end_at) : '<span class="muted">—</span>'}</td>
          <td>${showing ? '<span class="tag ok">显示</span>' : '<span class="tag warn">未显示</span>'}</td>
          <td class="row-actions">
            <button class="btn sm ghost" onclick='openAnnModal(${JSON.stringify(a)})'>编辑</button>
            <button class="btn sm ghost" onclick="toggleAnn(${a.id}, ${!a.enabled})">${a.enabled ? '停用' : '启用'}</button>
            <button class="btn sm danger" onclick="delAnn(${a.id})">删</button>
          </td>
        </tr>`;
      }).join('') || '<tr><td colspan="8" class="empty">暂无公告或无匹配项</td></tr>';
      renderPager('ann-pager', annMeta, `annPage=${annPage - 1};loadAnnouncements()`, `annPage=${annPage + 1};loadAnnouncements()`);
    }

    function updateAnnPreview() {
      const title = document.getElementById('m-a-title')?.value || '';
      const content = document.getElementById('m-a-content')?.value || '';
      const url = (document.getElementById('m-a-url')?.value || '').trim();
      const label = (document.getElementById('m-a-label')?.value || '').trim() || '立即前往';
      const box = document.getElementById('m-a-preview');
      if (!box) return;
      const actions = url
        ? `<div class="ann-preview-actions">
             <button class="btn sm ghost" type="button">关闭</button>
             <button class="btn sm" type="button">${escapeHtml(label)}</button>
           </div>`
        : `<div class="ann-preview-actions">
             <button class="btn sm ghost" type="button">关闭</button>
           </div>`;
      box.innerHTML = `
        <div class="ann-preview-title">${escapeHtml(title) || '<span class="muted">标题预览</span>'}</div>
        <p class="ann-preview-body">${escapeHtml(content) || '内容预览…'}</p>
        ${actions}`;
    }

    function openAnnModal(a) {
      const isEdit = !!(a && a.id);
      const vis = a?.visibility || 'always';
      const popup = a?.popup_mode || 'every_open';
      openModal(`
        <h3>${isEdit ? '编辑公告' : '发布公告'}</h3>
        <div class="form-grid">
          <div><label>范围</label>
            <select id="m-a-scope" onchange="onAnnScopeChange()"><option value="global" ${!a || a.scope==='global'?'selected':''}>全游戏</option>
            <option value="game" ${a&&a.scope==='game'?'selected':''}>单游戏</option></select></div>
          <div id="m-a-game-wrap"><label>游戏</label>${gamePickerMarkup('m-a-game', { emptyLabel: '请选择 / 可空', allowEmpty: true })}</div>
          <div><label>可见性</label>
            <select id="m-a-vis">
              <option value="always" ${vis==='always'?'selected':''}>始终</option>
              <option value="logged_out" ${vis==='logged_out'?'selected':''}>仅未登录</option>
              <option value="logged_in" ${vis==='logged_in'?'selected':''}>仅已登录</option>
            </select></div>
          <div><label>弹出方式</label>
            <select id="m-a-popup">
              <option value="every_open" ${popup==='every_open'?'selected':''}>每次打开</option>
              <option value="once_device" ${popup==='once_device'?'selected':''}>每设备一次</option>
            </select></div>
          <div><label>排序（越小越先展示）</label>
            <input id="m-a-sort" type="number" min="0" step="1" value="${a?.sort_order ?? 1}" /></div>
          <div><label>开始时间（可选）</label>
            ${timePickerMarkup('m-a-start', { value: toLocalInput(a?.start_at) })}</div>
          <div><label>结束时间（可选）</label>
            ${timePickerMarkup('m-a-end', { value: toLocalInput(a?.end_at) })}</div>
          <div class="full"><label>标题</label>
            <input id="m-a-title" value="${escapeHtml(a?.title || '')}" oninput="updateAnnPreview()" /></div>
          <div class="full"><label>内容</label>
            <textarea id="m-a-content" oninput="updateAnnPreview()">${escapeHtml(a?.content || '')}</textarea></div>
          <div><label>跳转链接（可选）</label>
            <input id="m-a-url" type="url" placeholder="https://… 不填则只显示关闭"
              value="${escapeHtml(a?.action_url || '')}" oninput="updateAnnPreview()" /></div>
          <div><label>按钮文案</label>
            <input id="m-a-label" placeholder="立即前往" value="${escapeHtml(a?.action_label || '')}"
              oninput="updateAnnPreview()" /></div>
          <div class="full"><label>预览</label>
            <div class="ann-preview" id="m-a-preview"></div>
            <p class="muted" style="margin:8px 0 0">填写跳转链接后客户端才显示「立即前往」；留空则仅关闭按钮。</p></div>
        </div>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn" onclick="saveAnn(${isEdit ? a.id : 'null'})">保存</button>
        </div>`);
      fillGameSelects();
      if (a?.game_id) setGamePickerValue('m-a-game', a.game_id, true);
      onAnnScopeChange();
      updateAnnPreview();
    }

    /** 范围=全游戏时隐藏并清空游戏选择器（与 Toast 弹窗同模式） */
    function onAnnScopeChange() {
      const scope = document.getElementById('m-a-scope')?.value;
      const wrap = document.getElementById('m-a-game-wrap');
      if (wrap) wrap.style.display = scope === 'game' ? '' : 'none';
      if (scope !== 'game') setGamePickerValue('m-a-game', '', true);
    }

    async function saveAnn(id) {
      try {
        const urlRaw = document.getElementById('m-a-url').value.trim();
        const labelRaw = document.getElementById('m-a-label').value.trim();
        if (urlRaw && !/^https?:\/\//i.test(urlRaw)) {
          toast('跳转链接需以 http:// 或 https:// 开头');
          return;
        }
        const scope = document.getElementById('m-a-scope').value;
        const body = {
          scope,
          game_id: scope === 'game' ? (getGamePickerValue('m-a-game') || null) : null,
          title: document.getElementById('m-a-title').value.trim(),
          content: document.getElementById('m-a-content').value.trim(),
          visibility: document.getElementById('m-a-vis').value,
          popup_mode: document.getElementById('m-a-popup').value,
          sort_order: parseInt(document.getElementById('m-a-sort').value, 10) || 0,
          start_at: tpISOString('m-a-start'),
          end_at: tpISOString('m-a-end'),
          action_url: urlRaw || null,
          action_label: labelRaw || null,
        };
        if (!body.title || !body.content) { toast('请填写标题和内容'); return; }
        if (id) await api('/v1/admin/announcements/' + id, { method: 'PATCH', body: JSON.stringify(body) });
        else await api('/v1/admin/announcements', { method: 'POST', body: JSON.stringify(body) });
        closeModal(); toast('已保存'); loadAnnouncements();
      } catch (e) { toast(e.message); }
    }
    async function toggleAnn(id, enabled) {
      try { await api('/v1/admin/announcements/' + id, { method: 'PATCH', body: JSON.stringify({ enabled }) }); loadAnnouncements(); }
      catch (e) { toast(e.message); }
    }
    async function delAnn(id) {
      if (!confirm('删除公告？')) return;
      try { await api('/v1/admin/announcements/' + id, { method: 'DELETE' }); loadAnnouncements(); }
      catch (e) { toast(e.message); }
    }

