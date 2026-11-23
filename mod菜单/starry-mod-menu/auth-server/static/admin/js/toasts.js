/* 管理后台 - 开屏 Toast（从 index.html 拆出，2026-09-21；原「游戏补丁 OTA」段已随 OTA 体系于 2026-11-21 删除）
   依赖：core.js 等在前。 */
    /* -------- toasts（开屏） -------- */
    async function loadToasts() {
      const qs = new URLSearchParams();
      const q = document.getElementById('t-q')?.value.trim() || '';
      const gameId = getGamePickerValue('t-filter-game');
      const scope = document.getElementById('t-scope')?.value || '';
      const en = boolParam(document.getElementById('t-enabled')?.value || '');
      if (q) qs.set('q', q);
      if (gameId) qs.set('game_id', gameId);
      if (scope) qs.set('scope', scope);
      if (en) qs.set('enabled', en);
      qs.set('page', String(toastPage));
      qs.set('page_size', String(toastPageSize));
      const data = await api('/v1/admin/toasts?' + qs.toString());
      toastsCache = data.data || [];
      toastMeta = data.meta || { total: toastsCache.length, page: toastPage, page_size: toastPageSize };
      toastPage = toastMeta.page || toastPage;
      document.getElementById('toast-body').innerHTML = toastsCache.map(t => `
        <tr>
          <td>${Number(t.sort_order) || 0}</td>
          <td><strong>${escapeHtml(t.content)}</strong></td>
          <td>${t.scope === 'global' ? '全游戏' : '单游戏'}${t.game_id ? ' / ' + escapeHtml(gameLabel(t.game_id)) : ''}</td>
          <td>${t.enabled ? statusTag('active') : statusTag('banned')}</td>
          <td class="row-actions">
            <button class="btn sm ghost" onclick='openToastModal(${JSON.stringify(t)})'>编辑</button>
            <button class="btn sm ghost" onclick="toggleToast(${t.id}, ${!t.enabled})">${t.enabled ? '停用' : '启用'}</button>
            <button class="btn sm danger" onclick="delToast(${t.id})">删</button>
          </td>
        </tr>`).join('') || '<tr><td colspan="5" class="empty">暂无开屏 Toast 或无匹配项</td></tr>';
      renderPager('toast-pager', toastMeta, `toastPage=${toastPage - 1};loadToasts()`, `toastPage=${toastPage + 1};loadToasts()`);
    }

    function openToastModal(t) {
      const isEdit = !!(t && t.id);
      openModal(`
        <h3>${isEdit ? '编辑开屏 Toast' : '添加开屏 Toast'}</h3>
        <p class="sub">启动时按「排序」从小到大依次弹出；样式固定为菜单默认 Toast，无需选类型</p>
        <div class="form-grid">
          <div><label for="m-t-scope">范围</label>
            <select id="m-t-scope" onchange="onToastScopeChange()">
              <option value="global" ${!t||t.scope==='global'?'selected':''}>全游戏</option>
              <option value="game" ${t&&t.scope==='game'?'selected':''}>单游戏</option>
            </select>
            <p class="muted" style="margin:6px 0 0">全游戏：所有 game_id 启动都显示；单游戏：只对选中的那款游戏显示（如只给某款游戏）</p>
          </div>
          <div id="m-t-game-wrap"><label>游戏</label>
            ${gamePickerMarkup('m-t-game', { emptyLabel: '请选择游戏', allowEmpty: true })}</div>
          <div><label for="m-t-order">排序</label>
            <input id="m-t-order" type="number" value="${Number(t?.sort_order) || 0}" /></div>
          <div><label for="m-t-enabled">启用</label>
            <select id="m-t-enabled">
              <option value="1" ${!t || t.enabled !== false ? 'selected' : ''}>启用</option>
              <option value="0" ${t && t.enabled === false ? 'selected' : ''}>停用</option>
            </select></div>
          <div class="full"><label for="m-t-content">内容</label>
            <input id="m-t-content" value="${escapeHtml(t?.content || '')}" placeholder="如：欢迎使用星光魔法" /></div>
        </div>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn" onclick="saveToast(${isEdit ? t.id : 'null'})">保存</button>
        </div>`);
      fillGameSelects();
      if (t?.game_id) setGamePickerValue('m-t-game', t.game_id, true);
      onToastScopeChange();
    }
    function onToastScopeChange() {
      const scope = document.getElementById('m-t-scope')?.value;
      const wrap = document.getElementById('m-t-game-wrap');
      if (wrap) wrap.style.display = scope === 'game' ? '' : 'none';
    }

    async function saveToast(id) {
      try {
        const scope = document.getElementById('m-t-scope').value;
        const body = {
          scope,
          game_id: scope === 'game' ? (getGamePickerValue('m-t-game') || null) : null,
          toast_type: 'info',
          content: document.getElementById('m-t-content').value.trim(),
          sort_order: parseInt(document.getElementById('m-t-order').value, 10) || 0,
          enabled: document.getElementById('m-t-enabled').value === '1',
        };
        if (!body.content) { toast('请填写内容'); return; }
        if (body.scope === 'game' && !body.game_id) { toast('请选择游戏'); return; }
        if (id) await api('/v1/admin/toasts/' + id, { method: 'PATCH', body: JSON.stringify(body) });
        else await api('/v1/admin/toasts', { method: 'POST', body: JSON.stringify(body) });
        closeModal(); toast('已保存'); loadToasts();
      } catch (e) { toast(e.message); }
    }
    async function toggleToast(id, enabled) {
      try {
        await api('/v1/admin/toasts/' + id, { method: 'PATCH', body: JSON.stringify({ enabled }) });
        loadToasts();
      } catch (e) { toast(e.message); }
    }
    async function delToast(id) {
      if (!(await confirmDialog({ title: '删除开屏 Toast', message: '删除后客户端启动时不再弹出这条提示。', okText: '删除', danger: true }))) return;
      try { await api('/v1/admin/toasts/' + id, { method: 'DELETE' }); loadToasts(); }
      catch (e) { toast(e.message); }
    }
