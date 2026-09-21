/* 管理后台 - 卡密管理（含操作下拉 + 统一编辑弹窗，从 index.html 拆出，2026-09-21）
   依赖：core.js、games.js 加载顺序在前。 */
    /* -------- cards -------- */
    function openCardCreateModal() {
      if (!gamesCache.length) { toast('请先在「游戏」里添加游戏'); switchPage('games'); return; }
      openModal(`
        <h3>生成卡密</h3>
        <p class="sub">生成后可一键复制或导出 txt</p>
        <div class="form-grid">
          <div><label for="m-c-mode">生成方式</label>
            <select id="m-c-mode" onchange="onCardCreateModeChange()">
              <option value="random" selected>随机生成</option>
              <option value="custom">自定义卡密</option>
            </select></div>
          <div id="m-c-count-wrap"><label for="m-c-count">数量</label><input id="m-c-count" type="number" value="5" min="1" max="500" /></div>
          <div id="m-c-prefix-wrap"><label for="m-c-prefix">前缀</label><input id="m-c-prefix" value="SG" maxlength="8" oninput="updatePrefixPreview()" /></div>
          <div class="full" id="m-c-custom-wrap" style="display:none">
            <label for="m-c-custom">自定义卡密（每行一条，也可用空格/逗号分隔，单次≤500）</label>
            <textarea id="m-c-custom" rows="6" style="width:100%;font-family:ui-monospace,monospace;font-size:12.5px;padding:10px;border-radius:8px;background:rgba(0,0,0,.25);border:1px solid rgba(255,255,255,.08);color:inherit"
              placeholder="MY-CARD-001&#10;MY-CARD-002&#10;GIFT_2026"></textarea>
            <p class="muted" style="margin:6px 0 0">输入什么就是什么。限字母/数字/-/_，4~64 位；与已有卡密或本批内重复将整批拒绝。</p>
          </div>
          <div><label for="m-c-scope">范围</label>
            <select id="m-c-scope" onchange="onCardScopeChange()">
              <option value="game">单游戏</option><option value="global">全游戏</option>
            </select></div>
          <div id="m-c-game-wrap"><label>游戏</label>
            ${gamePickerMarkup('m-c-game', { emptyLabel: '请选择游戏', allowEmpty: false })}</div>
          <div><label for="m-c-type">类型</label>
            <select id="m-c-type">
              <option value="hour">小时</option><option value="day" selected>天</option>
              <option value="month">月</option><option value="year">年</option>
              <option value="permanent">永久</option><option value="count">次数</option>
            </select></div>
          <div><label for="m-c-uses">次数上限（0=不限）</label><input id="m-c-uses" type="number" value="0" min="0" /></div>
          <div class="full"><label for="m-c-note">备注</label><input id="m-c-note" placeholder="可选，如批次号" /></div>
          <div class="full"><div class="muted">卡密预览</div><div class="prefix-preview" id="m-c-preview">SG-••••-••••</div></div>
        </div>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn" onclick="createCards()">生成</button>
        </div>`);
      updatePrefixPreview();
      fillGameSelects();
      if (gamesCache[0]) setGamePickerValue('m-c-game', gamesCache[0].game_id, true);
      onCardScopeChange();
      onCardCreateModeChange();
    }

    /** 生成方式切换：随机=数量+前缀；自定义=卡密文本域 */
    function onCardCreateModeChange() {
      const mode = document.getElementById('m-c-mode')?.value || 'random';
      const custom = mode === 'custom';
      const show = (id, on) => { const el = document.getElementById(id); if (el) el.style.display = on ? '' : 'none'; };
      show('m-c-count-wrap', !custom);
      show('m-c-prefix-wrap', !custom);
      show('m-c-custom-wrap', custom);
      show('m-c-preview', !custom);
    }

    /** 范围=全游戏时隐藏并清空游戏选择器（此前只降透明度仍可点，会带出脏数据） */
    function onCardScopeChange() {
      const scope = document.getElementById('m-c-scope')?.value;
      const wrap = document.getElementById('m-c-game-wrap');
      if (wrap) wrap.style.display = scope === 'game' ? '' : 'none';
      if (scope !== 'game') setGamePickerValue('m-c-game', '', true);
    }

    function updatePrefixPreview() {
      const p = (document.getElementById('m-c-prefix')?.value || 'SG').trim() || 'SG';
      const el = document.getElementById('m-c-preview');
      if (el) el.textContent = p.toUpperCase() + '-A1B2-C3D4';
    }

    /* -------- 卡密「操作」下拉 + 统一编辑弹窗（2026-09-06） -------- */
    let cardRowMenuEl = null;
    function closeCardRowMenu() {
      if (cardRowMenuEl) { cardRowMenuEl.remove(); cardRowMenuEl = null; }
    }
    function openCardRowMenu(ev, id, status) {
      ev.stopPropagation(); ev.preventDefault();
      if (cardRowMenuEl && cardRowMenuEl.dataset.forId === String(id)) { closeCardRowMenu(); return; }
      closeCardRowMenu();
      const banned = status === 'banned';
      const menu = document.createElement('div');
      menu.className = 'card-row-menu';
      menu.dataset.forId = String(id);
      menu.innerHTML = `
        <div class="crm-item" data-act="edit">编辑</div>
        <div class="crm-item" data-act="unbind">解绑设备</div>
        <div class="crm-item${banned ? '' : ' crm-danger'}" data-act="ban">${banned ? '解封' : '封禁'}</div>
        <div class="crm-item crm-danger" data-act="del">删除</div>`;
      menu.addEventListener('click', e => e.stopPropagation());
      menu.addEventListener('click', e => {
        const item = e.target.closest('.crm-item');
        if (!item) return;
        const act = item.dataset.act;
        closeCardRowMenu();
        if (act === 'edit') openCardEditModal(id);
        else if (act === 'unbind') forceUnbind(id);
        else if (act === 'ban') banCard(id, banned ? 'active' : 'banned');
        else if (act === 'del') delCard(id);
      });
      document.body.appendChild(menu);
      cardRowMenuEl = menu;
      const r = ev.currentTarget.getBoundingClientRect();
      menu.style.left = Math.max(8, Math.min(r.left, window.innerWidth - 148)) + 'px';
      menu.style.top = Math.min(r.bottom + 4, window.innerHeight - 170) + 'px';
    }

    const CARD_STATUS_CN = { unused: '未使用', active: '正常', expired: '已过期', banned: '封禁', exhausted: '已用尽' };

    /** 统一编辑弹窗：卡密只读；范围/游戏/类型/到期/次数/状态/备注可改。换游戏 → 下次心跳生效 */
    function openCardEditModal(id) {
      const card = cardsCache.find(c => c.id === id);
      if (!card) { toast('卡密不存在'); return; }
      openModal(`
        <h3>编辑卡密</h3>
        <p class="sub">卡密 <code>${escapeHtml(card.key_code)}</code>（不可修改） · 状态 ${escapeHtml(CARD_STATUS_CN[card.status] || card.status)}</p>
        <div class="form-grid">
          <div><label>范围</label>
            <select id="m-e2-scope" onchange="onCardEditScopeChange()">
              <option value="game" ${card.scope === 'game' ? 'selected' : ''}>单游戏</option>
              <option value="global" ${card.scope === 'global' ? 'selected' : ''}>全游戏</option>
            </select></div>
          <div id="m-e2-game-wrap"><label>游戏（换成哪个就只能登哪个）</label>
            ${gamePickerMarkup('m-e2-game', { emptyLabel: '请选择游戏', allowEmpty: false })}</div>
          <div><label>类型</label>
            <select id="m-e2-type">
              ${['hour','day','month','year','permanent','count'].map(t => `<option value="${t}" ${card.duration_type === t ? 'selected' : ''}>${TYPE_CN[t] || t}</option>`).join('')}
            </select></div>
          <div><label>状态（由到期/次数/封禁自动判定）</label>
            <div style="padding-top:8px">${statusTag(card.status)}</div></div>
          <div><label>到期时间</label>
            ${timePickerMarkup('m-e2-expire', { value: toLocalInput(card.expire_at), emptyLabel: '未设置（不限时）' })}</div>
          <div><label>次数上限（0=不限）</label>
            <input id="m-e2-max" type="number" min="0" value="${Number(card.max_uses || 0)}" /></div>
          <div><label>已用次数</label>
            <input id="m-e2-used" type="number" min="0" value="${Number(card.used_count || 0)}" /></div>
          <div class="full"><label>备注</label>
            <input id="m-e2-note" value="${escapeHtml(card.note || '')}" placeholder="可选，如批次号" /></div>
        </div>
        <p class="muted" style="margin:10px 0 0">换游戏后：原游戏下次心跳校验时失效，新登录只能登新游戏。清除到期时间并保存 = 取消时间限制。</p>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn" onclick="saveCardEdit(${id})">保存</button>
        </div>`);
      fillGameSelects();
      if (card.game_id) setGamePickerValue('m-e2-game', card.game_id, true);
      onCardEditScopeChange();
    }

    function onCardEditScopeChange() {
      const scope = document.getElementById('m-e2-scope')?.value;
      const wrap = document.getElementById('m-e2-game-wrap');
      if (wrap) wrap.style.display = scope === 'game' ? '' : 'none';
    }

    async function saveCardEdit(id) {
      try {
        const scope = document.getElementById('m-e2-scope').value;
        const body = {
          scope,
          game_id: scope === 'game' ? (getGamePickerValue('m-e2-game') || null) : null,
          duration_type: document.getElementById('m-e2-type').value,
          expire_at: tpISOString('m-e2-expire'),
          max_uses: Number(document.getElementById('m-e2-max').value || 0),
          used_count: Number(document.getElementById('m-e2-used').value || 0),
          note: document.getElementById('m-e2-note').value.trim() || null,
        };
        if (body.scope === 'game' && !body.game_id) { toast('请选择游戏'); return; }
        await api('/v1/admin/cards/' + id, { method: 'PATCH', body: JSON.stringify(body) });
        closeModal(); toast('已保存'); loadCards(); loadDashboard(true);
      } catch (e) { toast(e.message); }
    }

    async function createCards() {
      try {
        const customMode = (document.getElementById('m-c-mode')?.value || 'random') === 'custom';
        const customKeys = customMode
          ? document.getElementById('m-c-custom').value.split('\n').map(x => x.trim()).filter(Boolean)
          : null;
        if (customMode && !customKeys.length) { toast('请输入自定义卡密'); return; }
        const body = {
          count: Number(document.getElementById('m-c-count').value || 1),
          prefix: (document.getElementById('m-c-prefix').value || 'SG').trim() || 'SG',
          scope: document.getElementById('m-c-scope').value,
          game_id: document.getElementById('m-c-scope').value === 'game'
            ? (getGamePickerValue('m-c-game') || null) : null,
          duration_type: document.getElementById('m-c-type').value,
          max_uses: Number(document.getElementById('m-c-uses').value || 0),
          note: document.getElementById('m-c-note').value.trim() || null,
        };
        if (customMode) body.custom_keys = customKeys;
        if (body.scope === 'game' && !body.game_id) { toast('请选择游戏'); return; }
        const data = await api('/v1/admin/cards', { method: 'POST', body: JSON.stringify(body) });
        const keys = (data.data || []).map(c => c.key_code);
        window.__lastGeneratedKeys = keys;
        openModal(`
          <h3>已生成 ${keys.length} 张</h3>
          <p class="sub">可复制全部或导出为 txt</p>
          <div class="key-list">${keys.map(k => `<code>${escapeHtml(k)}</code>`).join('')}</div>
          <div class="modal-actions">
            <button class="btn ghost" onclick="closeModal()">关闭</button>
            <button class="btn ghost" onclick="downloadText('cards.txt', (window.__lastGeneratedKeys||[]).join('\\n'))">导出 txt</button>
            <button class="btn" onclick="copyText((window.__lastGeneratedKeys||[]).join('\\n'))">全部复制</button>
          </div>`, true);
        loadCards(); loadDashboard(true);
      } catch (e) { toast(e.message); }
    }

    async function loadCards() {
      const q = document.getElementById('c-q').value.trim();
      const status = document.getElementById('c-status').value;
      const scope = document.getElementById('c-filter-scope')?.value || '';
      const game_id = getGamePickerValue('c-filter-game');
      const bound = document.getElementById('c-bound').value;
      const unbind = document.getElementById('c-unbind').value;
      const qs = new URLSearchParams();
      if (q) qs.set('q', q);
      if (status) qs.set('status', status);
      if (scope) qs.set('scope', scope);
      if (game_id) qs.set('game_id', game_id);
      if (bound !== '') qs.set('bound', bound === '1' ? 'true' : 'false');
      if (unbind !== '') qs.set('unbind_used', unbind === '1' ? 'true' : 'false');
      qs.set('page', String(cardsPage));
      qs.set('page_size', String(cardsPageSize));
      const data = await api('/v1/admin/cards?' + qs.toString());
      cardsCache = data.data || [];
      cardsMeta = data.meta || { total: cardsCache.length, page: cardsPage, page_size: cardsPageSize };
      cardsPage = cardsMeta.page || cardsPage;
      selectedCardIds = new Set([...selectedCardIds].filter(id => cardsCache.some(c => c.id === id)));
      renderCardsTable();
      renderCardsPager();
    }

    function renderCardsPager() {
      const el = document.getElementById('cards-pager');
      if (!el) return;
      const total = cardsMeta.total || 0;
      const page = cardsMeta.page || cardsPage;
      const size = cardsMeta.page_size || cardsPageSize;
      const pages = Math.max(1, Math.ceil(total / size) || 1);
      el.innerHTML = `
        <span class="muted">共 ${total} 条 · 第 ${page}/${pages} 页</span>
        <div class="pager-actions">
          <button class="btn sm ghost" ${page <= 1 ? 'disabled' : ''} onclick="cardsPage=${page - 1};loadCards()">上一页</button>
          <button class="btn sm ghost" ${page >= pages ? 'disabled' : ''} onclick="cardsPage=${page + 1};loadCards()">下一页</button>
        </div>`;
    }

    function gameLabel(gameId) {
      if (!gameId) return '—';
      const g = gamesCache.find(x => x.game_id === gameId);
      return g ? g.name : gameId;
    }

    function renderCardsTable() {
      updateBatchBar();
      const all = document.getElementById('c-check-all');
      if (all) all.checked = cardsCache.length > 0 && selectedCardIds.size === cardsCache.length;
      document.getElementById('cards-body').innerHTML = cardsCache.map(c => {
        const exp = c.expire_at ? fmtLocal(c.expire_at) : (c.duration_type === 'permanent' ? '永久' : '-');
        const note = c.note ? escapeHtml(c.note) : '<span class="muted">点击添加</span>';
        const deviceShort = c.device_id ? escapeHtml(c.device_id.slice(0, 12)) + '…' : '未绑定';
        const deviceFull = expandedDevices.has(c.id)
          ? `<div class="device-full">${escapeHtml(c.device_id || '')}</div>` : '';
        const checked = selectedCardIds.has(c.id) ? 'checked' : '';
        return `
        <tr class="${checked ? 'is-selected' : ''}" data-id="${c.id}">
          <td><input type="checkbox" ${checked} onchange="toggleCardSelect(${c.id}, this.checked)" /></td>
          <td><code class="card-key" title="点击复制" onclick="copyText('${c.key_code}')">${c.key_code}</code></td>
          <td>${c.scope === 'global' ? '全游戏' : '单游戏'}</td>
          <td>${c.scope === 'global' ? '<span class="muted">全部</span>' : escapeHtml(gameLabel(c.game_id))}</td>
          <td>${TYPE_CN[c.duration_type] || c.duration_type}${c.max_uses ? ' · ' + c.used_count + '/' + c.max_uses : ''}</td>
          <td>${statusTag(c.status)}</td>
          <td>${exp}</td>
          <td class="note-cell" onclick='editCardNote(${c.id}, ${JSON.stringify(c.note || '')})'>${note}</td>
          <td>
            <span style="cursor:pointer" onclick="toggleDevice(${c.id})">${deviceShort}</span>
            ${c.unbind_used ? '<div class="muted">已解绑过</div>' : ''}
            ${deviceFull}
          </td>
          <td class="row-actions">
            <button class="btn sm ghost" onclick="openCardRowMenu(event, ${c.id}, '${c.status}')">操作 ▾</button>
          </td>
        </tr>`;
      }).join('') || '<tr><td colspan="10" class="empty">暂无卡密</td></tr>';
    }

    function toggleCardSelect(id, on) {
      if (on) selectedCardIds.add(id); else selectedCardIds.delete(id);
      updateBatchBar();
      const tr = document.querySelector(`tr[data-id="${id}"]`);
      if (tr) tr.classList.toggle('is-selected', on);
    }
    function toggleAllCards(on) {
      selectedCardIds.clear();
      if (on) cardsCache.forEach(c => selectedCardIds.add(c.id));
      document.querySelectorAll('#cards-body input[type=checkbox]').forEach(cb => { cb.checked = on; });
      document.querySelectorAll('#cards-body tr[data-id]').forEach(tr => tr.classList.toggle('is-selected', on));
      updateBatchBar();
    }
    function clearCardSelection() {
      selectedCardIds.clear();
      const all = document.getElementById('c-check-all');
      if (all) all.checked = false;
      renderCardsTable();
    }
    function updateBatchBar() {
      const bar = document.getElementById('card-batch-bar');
      const n = selectedCardIds.size;
      if (!bar) return;
      bar.classList.toggle('show', n > 0);
      document.getElementById('card-batch-count').textContent = '已选 ' + n;
    }
    async function batchBanCards(status) {
      if (!selectedCardIds.size) return;
      if (!confirm(`对选中的 ${selectedCardIds.size} 张执行${status === 'banned' ? '封禁' : '解封'}？`)) return;
      try {
        for (const id of [...selectedCardIds]) {
          await api('/v1/admin/cards/' + id, { method: 'PATCH', body: JSON.stringify({ status }) });
        }
        toast('批量操作完成'); loadCards(); loadDashboard(true);
      } catch (e) { toast(e.message); }
    }
    async function batchExtendCards(days) {
      if (!selectedCardIds.size) return;
      if (!confirm(`对选中的 ${selectedCardIds.size} 张延期 ${days} 天？`)) return;
      try {
        await api('/v1/admin/cards/batch', {
          method: 'POST',
          body: JSON.stringify({ ids: [...selectedCardIds], extend_days: days })
        });
        toast(`已批量延期 ${days} 天`); loadCards(); loadDashboard(true);
      } catch (e) { toast(e.message); }
    }
    function batchExportCards() {
      const keys = cardsCache.filter(c => selectedCardIds.has(c.id)).map(c => c.key_code);
      if (!keys.length) return;
      downloadText('cards-selected.txt', keys.join('\n'));
      toast('已导出 ' + keys.length + ' 张');
    }
    async function batchDeleteCards() {
      if (!selectedCardIds.size) return;
      if (!confirm(`确认删除选中的 ${selectedCardIds.size} 张卡密？已绑定设备会被踢下线，删除后不可恢复。`)) return;
      try {
        await api('/v1/admin/cards/batch-delete', {
          method: 'POST',
          body: JSON.stringify({ ids: [...selectedCardIds] })
        });
        selectedCardIds.clear();
        toast('批量删除完成'); loadCards(); loadDashboard(true);
      } catch (e) { toast(e.message); }
    }
    function toggleDevice(id) {
      if (expandedDevices.has(id)) expandedDevices.delete(id); else expandedDevices.add(id);
      renderCardsTable();
    }
    async function editCardNote(id, oldNote) {
      openModal(`
        <h3>编辑备注</h3>
        <label for="m-note">备注</label>
        <input id="m-note" value="${escapeHtml(oldNote || '')}" placeholder="批次 / 渠道等" />
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn" onclick="saveCardNote(${id})">保存</button>
        </div>`);
    }
    async function saveCardNote(id) {
      try {
        const note = document.getElementById('m-note').value.trim() || null;
        await api('/v1/admin/cards/' + id, { method: 'PATCH', body: JSON.stringify({ note }) });
        closeModal(); toast('备注已更新'); loadCards();
      } catch (e) { toast(e.message); }
    }

    async function banCard(id, status) {
      try { await api('/v1/admin/cards/' + id, { method: 'PATCH', body: JSON.stringify({ status }) }); toast('已更新'); loadCards(); loadDashboard(true); }
      catch (e) { toast(e.message); }
    }
    async function saveCardUses(id) {
      try {
        const max_uses = Number(document.getElementById('m-max-uses').value || 0);
        const used_count = Number(document.getElementById('m-used-count').value || 0);
        if (max_uses < 0 || used_count < 0) { toast('次数不能为负'); return; }
        await api('/v1/admin/cards/' + id, {
          method: 'PATCH',
          body: JSON.stringify({ max_uses, used_count })
        });
        closeModal(); toast('次数已更新'); loadCards();
      } catch (e) { toast(e.message); }
    }
    async function forceUnbind(id) {
      if (!confirm('强制解绑并重置自助解绑额度？')) return;
      try {
        await api('/v1/admin/cards/' + id, { method: 'PATCH', body: JSON.stringify({ force_unbind: true, reset_unbind: true }) });
        toast('已强制解绑'); loadCards();
      } catch (e) { toast(e.message); }
    }
    async function delCard(id) {
      if (!confirm('确认删除该卡密？')) return;
      try { await api('/v1/admin/cards/' + id, { method: 'DELETE' }); toast('已删除'); loadCards(); loadDashboard(true); }
      catch (e) { toast(e.message); }
    }

