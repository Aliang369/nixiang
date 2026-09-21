/* 管理后台（从 index.html 拆出，2026-09-21）
   依赖：core.js 等在前。 */
    /* -------- features (modal for one game) -------- */
    async function openFeatureManageModal(gameId) {
      if (!gameId) { toast('缺少 game_id'); return; }
      try {
        await Promise.all([loadGamesCatalog(), loadMenuTemplates()]);
      } catch (e) { /* forceLogout handles 401 */ }

      const g = gamesCache.find(x => x.game_id === gameId);
      if (!g && !gamesCache.length) {
        toast('请先在游戏列表添加游戏');
        return;
      }
      selectedFeatGameId = gameId;
      featPage = 1;
      const titleName = g ? g.name : gameId;
      const tpl = g?.menu_template ? `模板 ${g.menu_template}` : '未绑定模板';
      const count = Number(g?.feature_count) || 0;

      openModal(`
        <h3>${escapeHtml(titleName)} · 功能管理</h3>
        <p class="sub">game_id: <code>${escapeHtml(gameId)}</code> · ${escapeHtml(tpl)} · ${count} 项<br/>改模板后点「同步菜单」补缺/改名，不改已有开关。</p>
        <div class="filters" style="margin-bottom:10px">
          <div class="field grow">
            <label for="m-feat-q">搜索功能</label>
            <input id="m-feat-q" placeholder="名称 / 编号" onkeydown="if(event.key==='Enter'){featPage=1;loadFeaturesForGame(selectedFeatGameId)}" />
          </div>
          <div class="field">
            <label for="m-feat-enabled">可用</label>
            <select id="m-feat-enabled">
              <option value="">全部</option>
              <option value="1">开启</option>
              <option value="0">关闭</option>
            </select>
          </div>
          <div class="field actions"><label>&nbsp;</label>
            <button class="btn ghost" type="button" onclick="featPage=1;loadFeaturesForGame(selectedFeatGameId)">筛选</button>
          </div>
        </div>
        <div class="actions" style="margin-bottom:12px;justify-content:flex-start">
          <button class="btn ghost" type="button" onclick="syncSelectedGameFeatures(false)">同步菜单</button>
          <button class="btn ghost" type="button" onclick="syncSelectedGameFeatures(true)" title="按当前模板补齐/改名，并删除模板外多余功能">同步并清理多余</button>
        </div>
        <div class="table-wrap">
          <table>
            <thead><tr><th>编号</th><th>名称</th><th>可用</th><th>操作</th></tr></thead>
            <tbody id="feat-body"><tr><td colspan="4" class="empty">加载中…</td></tr></tbody>
          </table>
        </div>
        <div class="pager" id="feat-pager"></div>
        <div class="modal-actions">
          <button class="btn ghost" type="button" onclick="closeModal()">关闭</button>
        </div>`, 'feat');

      await loadFeaturesForGame(gameId);
    }

    async function loadFeaturesForGame(gameId) {
      const body = document.getElementById('feat-body');
      if (!body) return;
      if (!gameId) {
        body.innerHTML = '<tr><td colspan="4" class="empty">缺少游戏</td></tr>';
        renderPager('feat-pager', { total: 0, page: 1, page_size: featPageSize }, '', '');
        return;
      }
      const qs = new URLSearchParams();
      qs.set('game_id', gameId);
      const q = document.getElementById('m-feat-q')?.value.trim() || '';
      const en = boolParam(document.getElementById('m-feat-enabled')?.value || '');
      if (q) qs.set('q', q);
      if (en) qs.set('enabled', en);
      qs.set('page', String(featPage));
      qs.set('page_size', String(featPageSize));
      try {
        const data = await api('/v1/admin/features?' + qs.toString());
        featuresCache = data.data || [];
        featMeta = data.meta || { total: featuresCache.length, page: featPage, page_size: featPageSize };
        featPage = featMeta.page || featPage;
        body.innerHTML = featuresCache.map(f => `
          <tr>
            <td><code>${f.feat_num}</code></td>
            <td>${escapeHtml(f.name)}</td>
            <td>${f.enabled ? statusTag('active') : statusTag('banned')}</td>
            <td class="row-actions">
              <button class="btn sm ghost" onclick="toggleFeat(${f.id}, ${!f.enabled})">${f.enabled ? '关闭' : '开启'}</button>
              <button class="btn sm ghost" onclick='deleteFeat(${f.id}, ${f.feat_num}, ${JSON.stringify(f.name)})'>删除</button>
            </td>
          </tr>`).join('') || `<tr><td colspan="4" class="empty">暂无功能<br/><button class="btn sm" style="margin-top:8px" onclick="syncSelectedGameFeatures(false)">从菜单模板同步</button></td></tr>`;
        renderPager(
          'feat-pager',
          featMeta,
          `featPage=${featPage - 1};loadFeaturesForGame(selectedFeatGameId)`,
          `featPage=${featPage + 1};loadFeaturesForGame(selectedFeatGameId)`
        );
      } catch (e) {
        body.innerHTML = `<tr><td colspan="4" class="empty">${escapeHtml(e.message)}</td></tr>`;
        toast(e.message);
      }
    }

    async function syncSelectedGameFeatures(pruneExtra) {
      if (!selectedFeatGameId) { toast('缺少游戏'); return; }
      const g = gamesCache.find(x => x.game_id === selectedFeatGameId);
      let menu_template = g?.menu_template || null;
      if (!menu_template) {
        const pick = menuTemplates.map(t => t.id).join(' / ');
        const input = prompt('该游戏未绑定模板，输入模板 id：\n' + pick, menuTemplates[0]?.id || 'zhmr');
        if (!input) return;
        menu_template = input.trim();
      }
      if (pruneExtra) {
        const ok = confirm(
          `将按模板「${menu_template}」同步，并删除该游戏中不在模板里的多余功能。\n已有功能的开关状态会保留。\n\n确认继续？`
        );
        if (!ok) return;
      }
      try {
        const data = await api('/v1/admin/games/' + encodeURIComponent(selectedFeatGameId) + '/sync-features', {
          method: 'POST',
          body: JSON.stringify({ mode: pruneExtra ? 'prune' : 'replace', menu_template })
        });
        const r = data.data || {};
        toast(`同步完成：+${r.added || 0} 新增，${r.updated || 0} 改名，${r.removed || 0} 删除多余`);
        await loadGamesCatalog();
        featPage = 1;
        await loadFeaturesForGame(selectedFeatGameId);
      } catch (e) { toast(e.message); }
    }

    async function toggleFeat(id, enabled) {
      try {
        await api('/v1/admin/features/' + id, { method: 'PATCH', body: JSON.stringify({ enabled }) });
        await loadFeaturesForGame(selectedFeatGameId);
        await loadGamesCatalog();
      } catch (e) { toast(e.message); }
    }

    async function deleteFeat(id, featNum, name) {
      if (!confirm(`删除功能 #${featNum}「${name}」？\n客户端若仍有该编号项，将因门控缺失而不可用。`)) return;
      try {
        await api('/v1/admin/features/' + id, { method: 'DELETE' });
        toast('已删除');
        await loadFeaturesForGame(selectedFeatGameId);
        await loadGamesCatalog();
      } catch (e) { toast(e.message); }
    }

