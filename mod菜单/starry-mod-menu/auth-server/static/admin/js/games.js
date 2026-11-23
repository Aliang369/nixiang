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
          <td>${(g.channels && g.channels.length)
            ? g.channels.map(c => `<span class="tag">${escapeHtml(c)}</span>`).join(' ')
            : '<span class="muted">—</span>'}</td>
          <td>${g.menu_template ? `<code>${escapeHtml(g.menu_template)}</code>` : '<span class="muted">未绑定</span>'}</td>
          <td>${g.card_auth_enabled !== false
            ? '<span class="tag ok">需卡密</span>'
            : '<span class="tag warn">仅联网</span>'}</td>
          <td>${g.enabled ? statusTag('active') : statusTag('banned')}</td>
          <td>${g.request_updates_enabled === false
            ? '<span class="tag bad">更新已停用</span>'
            : '<span class="tag ok">接收中</span>'}</td>
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
      const disabled = !g.enabled;
      // 「请求更新」展示开关（2026-11-22）：默认开；false=已停用更新
      const reqOff = g.request_updates_enabled === false;
      const menu = document.createElement('div');
      menu.className = 'card-row-menu';
      menu.dataset.forId = String(id);
      menu.innerHTML = `
        <div class="crm-item" data-act="edit">编辑游戏</div>
        <div class="crm-item" data-act="features">功能管理</div>
        <div class="crm-item" data-act="card">${noCard ? '开启卡密' : '关闭卡密'}</div>
        <div class="crm-item" data-act="toggle">${disabled ? '开启游戏' : '停止游戏'}</div>
        <div class="crm-item" data-act="update">${reqOff ? '启用更新' : '停用更新'}</div>
        <div class="crm-item crm-danger" data-act="del">删除游戏</div>`;
      menu.addEventListener('click', e => e.stopPropagation());
      menu.addEventListener('click', e => {
        const item = e.target.closest('.crm-item');
        if (!item) return;
        const act = item.dataset.act;
        closeGameRowMenu();
        if (act === 'edit') openGameModal(g);
        else if (act === 'features') goManageFeatures(g.game_id);
        else if (act === 'card') toggleCardAuth(g.id, noCard);
        else if (act === 'toggle') toggleGame(g.id, disabled);
        else if (act === 'update') toggleRequestUpdates(g.id, reqOff);
        else if (act === 'del') delGame(g.id, g.game_id);
      });
      document.body.appendChild(menu);
      gameRowMenuEl = menu;
      const r = ev.currentTarget.getBoundingClientRect();
      menu.style.left = Math.max(8, Math.min(r.left, window.innerWidth - 148)) + 'px';
      menu.style.top = Math.min(r.bottom + 4, window.innerHeight - 260) + 'px';
    }

    function openGameModal(game) {
      const isEdit = !!(game && game.id);
      openModal(`
        <h3>${isEdit ? '编辑游戏' : '添加游戏'}</h3>
        <p class="sub">${isEdit ? '可改名称、模板；点同步可从模板补齐功能' : '输入 game_id 后会自动匹配同名模板并带出名称；无匹配则保持空白'}</p>
        <div class="form-grid">
          <div class="full"><label>game_id</label>
            <input id="m-g-id" value="${escapeHtml(game?.game_id || '')}" ${isEdit ? 'disabled' : ''}
              placeholder="建议与模板同名，如 starry_self"
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
              <option value="1" ${(game && game.card_auth_enabled === true) ? 'selected' : ''}>开启（需卡密 + 显示账号 Tab）</option>
              <option value="0" ${(!game || game.card_auth_enabled === false) ? 'selected' : ''}>关闭（隐藏账号 Tab，仅需联网）</option>
            </select>
            <p class="muted" style="margin:6px 0 0">关闭后客户端不走卡密登录，但仍需成功拉取服务器配置才能用功能；游戏未在后台注册时功能一律不可用。</p>
          </div>
          <div class="full"><label>下载渠道（前端下载弹窗展示，最多 3 个，可空）</label>
            <div id="m-g-dl-rows"></div>
            <button type="button" class="btn sm ghost" id="m-g-dl-add" onclick="addGameDownloadRow()"
              style="width:100%;border-style:dashed;margin-top:2px">+ 添加一行</button>
            <p class="muted" style="margin:6px 0 0">每行 = 渠道 + 链接；链接留空的行保存时会被忽略。</p>
          </div>
          <div class="full"><label>游戏渠道（可多选）</label>
            <div class="chk-grid" id="m-g-channels">
              ${GAME_CHANNELS.map(c => `<label class="chk"><input type="checkbox" value="${escapeHtml(c)}"${gameChannelPreset(game).includes(c) ? ' checked' : ''} />${escapeHtml(c)}</label>`).join('')}
            </div>
            <input id="m-g-channels-custom" placeholder="其它渠道：多个用英文逗号分隔，可留空" style="margin-top:10px"
              value="${escapeHtml(gameChannelCustom(game))}" />
            <p class="muted" style="margin:6px 0 0">前台「请求更新」只列出【已启用 + 未停用更新 + 至少 1 个渠道】的游戏；一个游戏可配多个渠道，访客提交时只能选其中一个。</p>
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
      // 且创建模式固定猜测默认模板 → 自动识别永远失效。
      const tplRoot = getPickerRoot('m-g-template');
      if (tplRoot) tplRoot.dataset.touched = '0';
      const nameEl = document.getElementById('m-g-name');
      if (nameEl) delete nameEl.dataset.autofilled;
      renderGameDownloadRows(parseGameDownloads(game?.download_url));
      // 渠道多选已在模板里按 gameChannelPreset / gameChannelCustom 回填（含旧单一 channel 兼容），
      // 这里不再需要额外的 onGameChannelChange 调用。
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

    // ----- 游戏来源渠道（多选；2026-11-22 起一个游戏可配多个渠道） -----
    // 预定义列表不含「其它」：其它/自定义渠道统一走下面的自定义输入框
    const GAME_CHANNELS = ['TapTap', '九游', '4399', '好游快爆', 'B站', '国际服'];
    const GAME_CHANNELS_MAX = 12;

    /** 取游戏当前渠道数组（2026-11-22 起只有 channels 多值字段，旧的单一 channel 已删除） */
    function gameChannelsOf(game) {
      return (game && Array.isArray(game.channels)) ? game.channels.slice() : [];
    }

    /** 回填：命中预定义列表的部分 → 勾选 */
    function gameChannelPreset(game) {
      return gameChannelsOf(game).filter(c => GAME_CHANNELS.includes(c));
    }

    /** 回填：不在预定义列表的部分（旧「其它」自定义值）→ 自定义输入框，逗号拼接 */
    function gameChannelCustom(game) {
      return gameChannelsOf(game).filter(c => !GAME_CHANNELS.includes(c)).join('，');
    }

    /** 收集本次勾选 + 自定义输入 → 数组（去重、去空、限 12 个） */
    function collectGameChannels() {
      const box = document.getElementById('m-g-channels');
      const picked = box
        ? Array.from(box.querySelectorAll('input[type="checkbox"]')).filter(i => i.checked).map(i => i.value)
        : [];
      const raw = (document.getElementById('m-g-channels-custom')?.value || '');
      const custom = raw.split(/[,，;；\n]/).map(s => s.trim()).filter(Boolean);
      const out = [];
      for (const c of picked.concat(custom)) {
        const v = String(c).slice(0, 64);
        if (v && !out.includes(v)) out.push(v);
        if (out.length >= GAME_CHANNELS_MAX) break;
      }
      return out;
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
        // 渠道多选：始终传数组；空数组=清空该游戏全部渠道
        const channels = collectGameChannels();
        if (!game_id || !name) { toast('请填写完整'); return; }
        if (!channels.length) { toast('请至少选择一个渠道（否则该游戏不会出现在「请求更新」页）'); return; }
        if (id) {
          const enabled = document.getElementById('m-g-enabled').value === '1';
          await api('/v1/admin/games/' + id, {
            method: 'PATCH',
            body: JSON.stringify({ name, enabled, menu_template, card_auth_enabled, download_url, channels })
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
            body: JSON.stringify({ game_id, name, menu_template, card_auth_enabled, download_url, channels, sync_features: true })
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
        await loadGames(); toast(enabled ? '已开启游戏' : '已停止游戏');
      } catch (e) { toast(e.message); }
    }

    async function toggleCardAuth(id, enabled) {
      try {
        await api('/v1/admin/games/' + id, { method: 'PATCH', body: JSON.stringify({ card_auth_enabled: enabled }) });
        await loadGames();
        toast(enabled ? '已开启卡密验证' : '已关闭卡密验证（仍需联网）');
      } catch (e) { toast(e.message); }
    }

    /** 「请求更新」展示开关（2026-11-22）：停用后前台该游戏不再出现，可随时再开 */
    async function toggleRequestUpdates(id, enabled) {
      try {
        await api('/v1/admin/games/' + id, {
          method: 'PATCH',
          body: JSON.stringify({ request_updates_enabled: enabled })
        });
        await loadGames();
        toast(enabled ? '已启用更新（前台可见）' : '已停用更新（前台不再展示）');
      } catch (e) { toast(e.message); }
    }

    async function delGame(id, gameId) {
      if (!(await confirmDialog({
        title: '删除游戏',
        message: `删除游戏「${gameId}」？\n将同时删除该游戏的功能门控、在线统计、授权事件与会话，不可恢复。\n建议先「停止游戏」观察几天再删。`,
        okText: '删除游戏', danger: true
      }))) return;
      try {
        const r = await api('/v1/admin/games/' + id, { method: 'DELETE' });
        toast(r.message || '已删除');
        await loadGames();
        loadDashboard(true);
      } catch (e) {
        if (e.status === 409 && (await confirmDialog({
          title: '强制删除游戏',
          message: e.message + '\n\n是否强制删除（连同该游戏下全部卡密）？',
          okText: '强制删除', danger: true
        }))) {
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

