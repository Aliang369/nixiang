/* 管理后台 - 游戏列表（从 index.html 拆出，2026-09-21）
   依赖：core.js、dashboard.js 加载顺序在前。 */
    /* -------- games -------- */
    async function loadGamesCatalog() {
      const data = await api('/v1/admin/games?page=1&page_size=500');
      gamesCache = data.data || [];
      fillGameSelects();
      return gamesCache;
    }

    async function loadGames(renderTable = true) {
      await loadGamesCatalog();
      if (renderTable) await loadGamesTable();
    }

    async function loadGamesTable() {
      const qs = new URLSearchParams();
      const q = document.getElementById('g-q')?.value.trim() || '';
      const en = boolParam(document.getElementById('g-enabled')?.value || '');
      const ca = boolParam(document.getElementById('g-card-auth')?.value || '');
      if (q) qs.set('q', q);
      if (en) qs.set('enabled', en);
      if (ca) qs.set('card_auth_enabled', ca);
      qs.set('page', String(gamesPage));
      qs.set('page_size', String(gamesPageSize));
      const data = await api('/v1/admin/games?' + qs.toString());
      gamesRows = data.data || [];
      gamesMeta = data.meta || { total: gamesRows.length, page: gamesPage, page_size: gamesPageSize };
      gamesPage = gamesMeta.page || gamesPage;
      document.getElementById('games-body').innerHTML = gamesRows.map(g => `
        <tr>
          <td><code>${escapeHtml(g.game_id)}</code></td>
          <td>${escapeHtml(g.name)}</td>
          <td>${g.channel ? `<span class="tag">${escapeHtml(g.channel)}</span>` : '<span class="muted">—</span>'}</td>
          <td>${g.menu_template ? `<code>${escapeHtml(g.menu_template)}</code>` : '<span class="muted">未绑定</span>'}</td>
          <td>${g.card_auth_enabled !== false
            ? '<span class="tag ok">需卡密</span>'
            : '<span class="tag warn">仅联网</span>'}</td>
          <td>${g.portal_visible !== false
            ? '<span class="tag ok">展示中</span>'
            : '<span class="tag muted">已隐藏</span>'}</td>
          <td>${g.enabled ? statusTag('active') : statusTag('banned')}</td>
          <td class="row-actions">
            <button class="btn sm ghost" onclick="openGameRowMenu(event, ${g.id})">操作 ▾</button>
          </td>
        </tr>`).join('') || '<tr><td colspan="8" class="empty">暂无游戏或无匹配项</td></tr>';
      renderPager('games-pager', gamesMeta, `gamesPage=${gamesPage - 1};loadGamesTable()`, `gamesPage=${gamesPage + 1};loadGamesTable()`);
    }

    /* -------- 游戏「操作」下拉（同卡密交互，2026-09-21） -------- */
    let gameRowMenuEl = null;
    function closeGameRowMenu() {
      if (gameRowMenuEl) { gameRowMenuEl.remove(); gameRowMenuEl = null; }
    }
    function openGameRowMenu(ev, id) {
      ev.stopPropagation(); ev.preventDefault();
      if (gameRowMenuEl && gameRowMenuEl.dataset.forId === String(id)) { closeGameRowMenu(); return; }
      closeGameRowMenu();
      const g = gamesRows.find(x => x.id === id) || gamesCache.find(x => x.id === id);
      if (!g) { toast('游戏数据不存在'); return; }
      const noCard = g.card_auth_enabled === false;
      const hidden = g.portal_visible === false;
      const disabled = !g.enabled;
      const menu = document.createElement('div');
      menu.className = 'card-row-menu';
      menu.dataset.forId = String(id);
      menu.innerHTML = `
        <div class="crm-item" data-act="edit">编辑</div>
        <div class="crm-item" data-act="features">功能管理</div>
        <div class="crm-item" data-act="payload">载荷 sha</div>
        <div class="crm-item" data-act="card">${noCard ? '开卡密' : '关卡密'}</div>
        <div class="crm-item" data-act="portal">${hidden ? '前端展示' : '前端隐藏'}</div>
        <div class="crm-item" data-act="toggle">${disabled ? '启用' : '停用'}</div>
        <div class="crm-item crm-danger" data-act="del">删除</div>`;
      menu.addEventListener('click', e => e.stopPropagation());
      menu.addEventListener('click', e => {
        const item = e.target.closest('.crm-item');
        if (!item) return;
        const act = item.dataset.act;
        closeGameRowMenu();
        if (act === 'edit') openGameModal(g);
        else if (act === 'features') goManageFeatures(g.game_id);
        else if (act === 'payload') openGamePayloadModal(g.id);
        else if (act === 'card') toggleCardAuth(g.id, noCard);
        else if (act === 'portal') togglePortalVisible(g.id, hidden);
        else if (act === 'toggle') toggleGame(g.id, !disabled);
        else if (act === 'del') delGame(g.id, g.game_id);
      });
      document.body.appendChild(menu);
      gameRowMenuEl = menu;
      const r = ev.currentTarget.getBoundingClientRect();
      menu.style.left = Math.max(8, Math.min(r.left, window.innerWidth - 148)) + 'px';
      menu.style.top = Math.min(r.bottom + 4, window.innerHeight - 220) + 'px';
    }

    function openGameModal(game) {
      const isEdit = !!(game && game.id);
      openModal(`
        <h3>${isEdit ? '编辑游戏' : '添加游戏'}</h3>
        <p class="sub">${isEdit ? '可改名称、模板；点同步可从模板补齐功能' : '输入 game_id 后会自动匹配同名模板并带出名称；无匹配则保持空白'}</p>
        <div class="form-grid">
          <div class="full"><label>game_id</label>
            <input id="m-g-id" value="${escapeHtml(game?.game_id || '')}" ${isEdit ? 'disabled' : ''}
              placeholder="建议与模板同名，如 zhmr"
              oninput="onGameIdInput(this.value)" /></div>
          <div class="full"><label>名称</label>
            <input id="m-g-name" value="${escapeHtml(game?.name || '')}" placeholder="如 战魂铭人"
              oninput="onGameNameInput()" /></div>
          <div class="full"><label>菜单模板</label>
            ${gamePickerMarkup('m-g-template', {
              kind: 'templates',
              emptyLabel: '不绑定（稍后手动同步）',
              allowEmpty: true,
              onchange: 'onGameTemplateChange',
              searchPlaceholder: '搜索模板名称 / id',
            })}
          </div>
          <div class="full"><label>卡密启用</label>
            <select id="m-g-card-auth">
              <option value="1" ${(!game || game.card_auth_enabled !== false) ? 'selected' : ''}>开启（需卡密 + 显示账号 Tab）</option>
              <option value="0" ${(game && game.card_auth_enabled === false) ? 'selected' : ''}>关闭（隐藏账号 Tab，仅需联网）</option>
            </select>
            <p class="muted" style="margin:6px 0 0">关闭后客户端不走卡密登录，但仍需成功拉取服务器配置才能用功能。</p>
          </div>
          <div class="full"><label>下载渠道（前端下载弹窗展示，最多 3 个，可空）</label>
            <div id="m-g-dl-rows"></div>
            <button type="button" class="btn sm ghost" id="m-g-dl-add" onclick="addGameDownloadRow()"
              style="width:100%;border-style:dashed;margin-top:2px">+ 添加一行</button>
            <p class="muted" style="margin:6px 0 0">每行 = 渠道 + 链接；链接留空的行保存时会被忽略。</p>
          </div>
          <div class="full"><label>游戏渠道（前端卡片名字下展示）</label>
            <select id="m-g-channel" onchange="onGameChannelChange()">
              <option value="">不设置</option>
              ${GAME_CHANNELS.map(c => `<option value="${escapeHtml(c)}" ${c === (game?.channel || '') ? 'selected' : ''}>${escapeHtml(c)}</option>`).join('')}
            </select>
            <input id="m-g-channel-custom" placeholder="输入自定义渠道名" style="display:none;margin-top:8px"
              value="${escapeHtml(game?.channel && !GAME_CHANNELS.includes(game.channel) ? game.channel : '')}" />
          </div>
          ${isEdit ? `<div class="full"><label>游戏启用</label>
            <select id="m-g-enabled"><option value="1" ${game.enabled ? 'selected' : ''}>启用</option>
            <option value="0" ${!game.enabled ? 'selected' : ''}>停用</option></select></div>` : ''}
          <div class="full" id="m-g-preview" style="padding:10px;border-radius:8px;background:rgba(0,0,0,.2);font-size:13px;line-height:1.7"></div>
        </div>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          ${isEdit ? `<button class="btn ghost" onclick="saveGame(${game.id}, true)">保存并同步菜单</button>` : ''}
          <button class="btn" onclick="saveGame(${isEdit ? game.id : 'null'})">保存</button>
        </div>`);
      fillGameSelects();
      // 重置自动识别状态：弹窗打开时不预选模板、不预填名称、不封锁 game_id 自动识别。
      // 修复 2026-09-05：此前打开即调 onGameTemplateChange() 把 touched 置 1，
      // 且创建模式固定猜测 zhmr → 自动识别永远失效。
      const tplRoot = getPickerRoot('m-g-template');
      if (tplRoot) tplRoot.dataset.touched = '0';
      const nameEl = document.getElementById('m-g-name');
      if (nameEl) delete nameEl.dataset.autofilled;
      renderGameDownloadRows(parseGameDownloads(game?.download_url));
      // 编辑回填：历史渠道不在预定义列表（旧「其它」自定义值）→ 选中其它并展开输入框
      if (game?.channel && !GAME_CHANNELS.includes(game.channel)) {
        const sel = document.getElementById('m-g-channel');
        if (sel) sel.value = '其它';
      }
      onGameChannelChange();
      if (isEdit) {
        // 编辑模式：回填现有绑定并渲染功能预览（非静默会触发 onchange，见下）
        setGamePickerValue('m-g-template', game?.menu_template || '', true);
        previewTemplateFeatures(game?.menu_template || '');
      } else {
        previewTemplateFeatures('');
      }
    }

    /** 模板 picker 用户手动选择时触发（组件无参回调）；程序化预览走 previewTemplateFeatures 直调，不走这里 */
    function onGameTemplateChange() {
      const tid = getGamePickerValue('m-g-template');
      const root = getPickerRoot('m-g-template');
      if (root) root.dataset.touched = '1';   // 手动选择后，game_id 自动识别让位
      previewTemplateFeatures(tid);
      fillNameFromTemplate(tid);
    }

    function onGameIdInput(gid) {
      gid = (gid || '').trim();
      const root = getPickerRoot('m-g-template');
      if (!root || root.dataset.touched === '1') return;
      if (menuTemplates.some(t => t.id === gid)) {
        setGamePickerValue('m-g-template', gid, true);
        previewTemplateFeatures(gid);
        fillNameFromTemplate(gid);
      }
    }

    /** 由模板带出名称：仅当名称为空或此前是自动带出的（用户手改过则不覆盖） */
    function fillNameFromTemplate(tid) {
      const nameEl = document.getElementById('m-g-name');
      if (!nameEl) return;
      const t = menuTemplates.find(x => x.id === tid);
      if (!t) return;
      if (nameEl.value.trim() && nameEl.dataset.autofilled !== '1') return;
      nameEl.value = t.name;
      nameEl.dataset.autofilled = '1';
    }

    /** 用户手动改名称后，取消自动带出标记 */
    function onGameNameInput() {
      const nameEl = document.getElementById('m-g-name');
      if (nameEl) delete nameEl.dataset.autofilled;
    }

    // ----- 游戏来源渠道（单选；「其它」可自定义输入） -----
    const GAME_CHANNELS = ['TapTap', '九游', '4399', '好游快爆', 'B站', '国际服', '其它'];

    /** 渠道选「其它」时显示自定义输入框 */
    function onGameChannelChange() {
      const sel = document.getElementById('m-g-channel');
      const custom = document.getElementById('m-g-channel-custom');
      if (!sel || !custom) return;
      custom.style.display = sel.value === '其它' ? '' : 'none';
      if (sel.value !== '其它') custom.value = '';
    }

    /** 收集渠道值：预定义原样；其它取自定义输入；未设置/清空返回 ''（PATCH 空串=清空） */
    function collectGameChannel() {
      const sel = document.getElementById('m-g-channel');
      if (!sel || !sel.value) return '';
      if (sel.value !== '其它') return sel.value;
      return (document.getElementById('m-g-channel-custom')?.value || '').trim();
    }

    // ----- 游戏下载渠道多行编辑（最多 3 行） -----
    const DL_CHANNELS = ['123网盘', '百度网盘', '夸克网盘', '蓝奏云', '其它'];

    /** 解析 download_url 历史数据：JSON 数组 → 行列表；纯 URL → 单行「其它」 */
    function parseGameDownloads(raw) {
      const s = (raw || '').trim();
      if (!s) return [];
      if (s.startsWith('[')) {
        try {
          const arr = JSON.parse(s);
          if (Array.isArray(arr)) {
            return arr.map(it => {
              if (it && typeof it === 'object') return { name: it.name || '其它', url: it.url || '' };
              return { name: '其它', url: String(it) };
            }).filter(it => it.url).slice(0, 3);
          }
        } catch (e) { /* 非法 JSON 按纯 URL 处理 */ }
      }
      return [{ name: '其它', url: s }];
    }

    /** 渲染下载渠道行（保留当前已输入值可选传入） */
    function renderGameDownloadRows(list) {
      const box = document.getElementById('m-g-dl-rows');
      if (!box) return;
      const rows = list && list.length ? list : [{ name: '百度网盘', url: '' }];
      box.innerHTML = rows.map(it => `
        <div class="dl-row" style="display:flex;gap:8px;margin-bottom:8px;align-items:center">
          <select class="dl-ch" style="flex:none;width:96px">
            ${DL_CHANNELS.map(c => `<option value="${escapeHtml(c)}" ${c === it.name ? 'selected' : ''}>${escapeHtml(c)}</option>`).join('')}
          </select>
          <input class="dl-url" style="flex:1" placeholder="下载链接" value="${escapeHtml(it.url || '')}" />
          <button type="button" class="btn sm ghost" onclick="removeGameDownloadRow(this)">删除</button>
        </div>`).join('');
      refreshGameDownloadAddBtn();
    }

    function readGameDownloadRows() {
      const box = document.getElementById('m-g-dl-rows');
      if (!box) return [];
      return Array.from(box.querySelectorAll('.dl-row')).map(row => ({
        name: row.querySelector('.dl-ch').value,
        url: row.querySelector('.dl-url').value.trim()
      }));
    }

    function refreshGameDownloadAddBtn() {
      const btn = document.getElementById('m-g-dl-add');
      if (!btn) return;
      const n = readGameDownloadRows().length;
      btn.style.display = n >= 3 ? 'none' : '';
    }

    function addGameDownloadRow() {
      const rows = readGameDownloadRows();
      if (rows.length >= 3) { toast('最多 3 个渠道'); return; }
      rows.push({ name: '其它', url: '' });
      renderGameDownloadRows(rows);
    }

    function removeGameDownloadRow(btn) {
      const row = btn.closest('.dl-row');
      const box = document.getElementById('m-g-dl-rows');
      row.remove();
      if (!box.querySelector('.dl-row')) renderGameDownloadRows(null); // 至少保留一行
    }

    /** 收集为 JSON 字符串：过滤空链接行；全部为空返回 null */
    function collectGameDownloads() {
      const rows = readGameDownloadRows().filter(it => it.url);
      if (!rows.length) return null;
      return JSON.stringify(rows.map(it => ({ name: it.name, url: it.url })));
    }

    async function saveGame(id, alsoSync) {
      try {
        const game_id = document.getElementById('m-g-id').value.trim();
        const name = document.getElementById('m-g-name').value.trim();
        const menu_template = getGamePickerValue('m-g-template') || null;
        const card_auth_enabled = document.getElementById('m-g-card-auth').value === '1';
        // 空渠道传空串而非 null：PATCH 中 null=不更新，无法清空
        const download_url = collectGameDownloads() || '';
        const channel = collectGameChannel();
        if (!game_id || !name) { toast('请填写完整'); return; }
        if (id) {
          const enabled = document.getElementById('m-g-enabled').value === '1';
          await api('/v1/admin/games/' + id, {
            method: 'PATCH',
            body: JSON.stringify({ name, enabled, menu_template, card_auth_enabled, download_url, channel })
          });
          if (alsoSync) {
            const g = gamesCache.find(x => x.id === id) || { game_id };
            const sync = await api('/v1/admin/games/' + encodeURIComponent(g.game_id) + '/sync-features', {
              method: 'POST', body: JSON.stringify({ mode: 'replace', menu_template })
            });
            toast(`已保存，同步 +${sync.data?.added || 0} / 更新 ${sync.data?.updated || 0}`);
          } else toast('已保存');
          closeModal();
          await loadGames();
        } else {
          const data = await api('/v1/admin/games', {
            method: 'POST',
            body: JSON.stringify({ game_id, name, menu_template, card_auth_enabled, download_url, channel, sync_features: true })
          });
          closeModal();
          await loadGames();
          const synced = data.data?.synced_features;
          const count = synced?.features?.length || 0;
          openModal(`
            <h3>游戏已创建</h3>
            <p class="sub">${escapeHtml(name)}（${escapeHtml(game_id)}）
              ${count ? `已自动写入 <strong>${count}</strong> 个菜单功能` : '未绑定模板，可稍后在功能管理中同步'}</p>
            <div class="modal-actions">
              <button class="btn ghost" onclick="closeModal()">稍后</button>
              <button class="btn" onclick="closeModal(); goManageFeatures('${escapeHtml(game_id)}')">去开关功能</button>
            </div>`);
        }
      } catch (e) { toast(e.message); }
    }

    async function toggleGame(id, enabled) {
      try {
        await api('/v1/admin/games/' + id, { method: 'PATCH', body: JSON.stringify({ enabled }) });
        await loadGames(); toast('已更新');
      } catch (e) { toast(e.message); }
    }

    async function toggleCardAuth(id, enabled) {
      try {
        await api('/v1/admin/games/' + id, { method: 'PATCH', body: JSON.stringify({ card_auth_enabled: enabled }) });
        await loadGames();
        toast(enabled ? '已开启卡密验证' : '已关闭卡密验证（仍需联网）');
      } catch (e) { toast(e.message); }
    }

    /** 前端显隐切换：仅控制 /portal/ 展示，不影响客户端授权 */
    async function togglePortalVisible(id, visible) {
      try {
        await api('/v1/admin/games/' + id, { method: 'PATCH', body: JSON.stringify({ portal_visible: visible }) });
        await loadGames();
        toast(visible ? '已在前端展示' : '已从前端隐藏（客户端不受影响）');
      } catch (e) { toast(e.message); }
    }

    // ===== P2-2：该游戏的功能载荷 SHA256（按游戏维护，改完即时生效、无需重建镜像）=====
    // 这个值决定服务端派生载荷密钥 K_build；库里没登记 = 该游戏不签 v3 票（客户端只剩诱饵）。
    async function openGamePayloadModal(id) {
      const g = gamesRows.find(x => x.id === id) || gamesCache.find(x => x.id === id);
      if (!g) { toast('游戏数据不存在'); return; }
      let info = {};
      try {
        const r = await api('/v1/admin/games/' + encodeURIComponent(g.game_id) + '/payload', { method: 'GET' });
        info = r.data || r || {};
      } catch (e) { toast('读取失败：' + e.message); return; }

      openModal(`
        <h3>载荷 SHA256 · ${escapeHtml(g.game_id)}</h3>
        <p class="sub">该值用于服务端派生载荷密钥 K_build。<b>改它 = 旧包功能全部失效</b>，必须同步出新包。</p>
        <div class="form-grid">
          <div class="full"><label>当前状态</label>
            <div class="sub">${info.v3_ready
              ? '✅ 已登记，可签 v3 票（K_build 指纹 <code>' + escapeHtml(info.k_build_fp || '') + '</code>）'
              : '⚠️ 未登记：该游戏目前不签 v3 票（客户端拿不到载荷，只剩诱饵）'}</div></div>
          <div class="full"><label>载荷 SHA256</label>
            <input id="m-g-payload-sha" value="${escapeHtml(info.payload_sha || '')}"
              placeholder="64 位十六进制；留空并选文件可自动计算" /></div>
          <div class="full"><label>或上传密文文件自动计算</label>
            <input id="m-g-payload-file" type="file"
              onchange="computePayloadSha(this)" /></div>
        </div>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn ghost danger" onclick="clearGamePayload('${escapeHtml(g.game_id)}')">清空</button>
          <button class="btn" onclick="saveGamePayload('${escapeHtml(g.game_id)}')">保存</button>
        </div>`);
    }

    async function computePayloadSha(input) {
      const f = input.files && input.files[0];
      if (!f) return;
      try {
        const buf = await f.arrayBuffer();
        const dig = await crypto.subtle.digest('SHA-256', buf);
        const hex = Array.from(new Uint8Array(dig)).map(b => b.toString(16).padStart(2, '0')).join('');
        document.getElementById('m-g-payload-sha').value = hex;
        toast('已按文件计算 sha256');
      } catch (e) { toast('计算失败：' + e.message); }
    }

    async function saveGamePayload(gameId) {
      const sha = (document.getElementById('m-g-payload-sha').value || '').trim().toLowerCase();
      if (!/^[0-9a-f]{64}$/.test(sha)) { toast('sha 必须是 64 位十六进制'); return; }
      if (!confirm('确认保存？\n改这一项会让该游戏所有旧包功能失效，必须同步出新包。')) return;
      try {
        const r = await api('/v1/admin/games/' + encodeURIComponent(gameId) + '/payload?sha=' + sha, { method: 'PUT' });
        const d = r.data || r || {};
        toast(d.v3_ready ? '已保存，v3 票可用' : '已保存（服务端尚未就绪）');
        closeModal();
        await loadGames();
      } catch (e) { toast(e.message); }
    }

    async function clearGamePayload(gameId) {
      if (!confirm('清空后该游戏不再签 v3 票，客户端会退回诱饵。确认？')) return;
      try {
        await api('/v1/admin/games/' + encodeURIComponent(gameId) + '/payload?sha=clear', { method: 'PUT' });
        toast('已清空');
        closeModal();
        await loadGames();
      } catch (e) { toast(e.message); }
    }


    async function delGame(id, gameId) {
      if (!confirm(`删除游戏「${gameId}」？\n将同时删除该游戏的功能门控、在线统计、授权事件与会话，不可恢复。\n建议先「停用」观察几天再删。`)) return;
      try {
        const r = await api('/v1/admin/games/' + id, { method: 'DELETE' });
        toast(r.message || '已删除');
        await loadGames();
        loadDashboard(true);
      } catch (e) {
        if (e.status === 409 && confirm(e.message + '\n\n是否强制删除（连同该游戏下全部卡密）？')) {
          try {
            const r2 = await api('/v1/admin/games/' + id + '?force=1', { method: 'DELETE' });
            toast(r2.message || '已强制删除');
            await loadGames();
            loadDashboard(true);
          } catch (e2) { toast(e2.message); }
        } else {
          toast(e.message);
        }
      }
    }

