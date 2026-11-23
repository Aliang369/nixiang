/* 管理后台 - 许愿游戏（2026-11-22）
   依赖 core.js（api / escapeHtml / fmtLocal / toast / confirmDialog）
   依赖 index.html 内联（openModal / closeModal）
   接口：
     GET   /v1/admin/wish-requests             外层聚合，一行一个游戏（只含有未处理的）
     GET   /v1/admin/wish-requests/{key}       详情，列出该游戏全部许愿人（含 IP）
     PATCH /v1/admin/wish-requests/{key}/done  把该游戏所有未处理许愿标记为已处理
   「拉黑」本期不做，按钮先占位（后端 status 字段已预留 blacklisted）。 */
    let wishCache = [];

    const WQ_STATUS_CN = { pending: '未处理', done: '已处理', blacklisted: '已拉黑' };
    const WQ_STATUS_TAG = { pending: 'muted', done: 'ok', blacklisted: 'bad' };

    /** 渠道合集 → 标签串 */
    function wqChannelsHtml(list) {
      if (!list || !list.length) return '<span class="muted">—</span>';
      return list.map(c => `<span class="tag">${escapeHtml(c)}</span>`).join(' ');
    }

    async function loadWishRequests() {
      const body = document.getElementById('wishes-body');
      if (!body) return;
      try {
        const q = (document.getElementById('wq-q')?.value || '').trim();
        const data = await api('/v1/admin/wish-requests' + (q ? '?q=' + encodeURIComponent(q) : ''));
        wishCache = data.data || [];
        if (!wishCache.length) {
          body.innerHTML = '<tr><td colspan="5" class="empty">暂无许愿</td></tr>';
          return;
        }
        body.innerHTML = wishCache.map(r => `
          <tr>
            <td>${escapeHtml(r.game_name)}</td>
            <td>${wqChannelsHtml(r.channels)}</td>
            <td><strong>${Number(r.people) || 0}</strong> <span class="muted">人</span></td>
            <td><button class="btn sm ghost" onclick="openWishDetail('${encodeURIComponent(r.game_key)}')">详情</button></td>
            <td class="row-actions">
              <button class="btn sm ghost" onclick="markWishesDone('${encodeURIComponent(r.game_key)}')">已处理</button>
            </td>
          </tr>`).join('');
      } catch (e) {
        body.innerHTML = `<tr><td colspan="5" class="empty">${escapeHtml(e.message)}</td></tr>`;
      }
    }

    /** 已处理：把该游戏当前所有未处理许愿标记为已处理，列表随即不再显示它 */
    async function markWishesDone(keyEnc) {
      const gameKey = decodeURIComponent(keyEnc);
      const item = wishCache.find(x => x.game_key === gameKey);
      const label = item ? item.game_name : gameKey;
      if (!(await confirmDialog({
        title: '标记已处理',
        message: `把「${label}」当前所有许愿标记为已处理？\n\n处理后会从列表消失；之后有人再许愿，这个游戏会重新出现。`,
        okText: '标记已处理'
      }))) return;
      try {
        const res = await api('/v1/admin/wish-requests/' + encodeURIComponent(gameKey) + '/done', { method: 'PATCH' });
        toast(res.message || '已处理');
        await loadWishRequests();
      } catch (e) {
        toast(e.message);
      }
    }

    async function openWishDetail(keyEnc) {
      const gameKey = decodeURIComponent(keyEnc);
      openModal('<div class="empty">加载中…</div>', true);
      try {
        const res = await api('/v1/admin/wish-requests/' + encodeURIComponent(gameKey));
        const d = res.data || {};
        const rows = (d.items || []).map(it => `
          <tr>
            <td>${escapeHtml(it.wp_username || '—')}</td>
            <td>${it.channel ? `<span class="tag">${escapeHtml(it.channel)}</span>` : '<span class="muted">—</span>'}</td>
            <td><code>${escapeHtml(it.ip || '—')}</code></td>
            <td><span class="tag ${WQ_STATUS_TAG[it.status] || 'muted'}">${escapeHtml(WQ_STATUS_CN[it.status] || it.status || '—')}</span></td>
            <td class="muted rq-time">${fmtLocal(it.created_at)}</td>
            <td><button class="btn sm ghost" onclick="wqNotReady('拉黑')">拉黑</button></td>
          </tr>`).join('');
        openModal(`
          <h3>许愿详情 · ${escapeHtml(d.game_name || gameKey)}</h3>
          <p class="sub">共 ${Number(d.total) || 0} 条许愿 · 去重 ${Number(d.people) || 0} 人</p>
          <div class="table-wrap">
            <table>
              <thead><tr><th>许愿人</th><th>渠道</th><th>IP</th><th>状态</th><th>时间</th><th>操作</th></tr></thead>
              <tbody>${rows || '<tr><td colspan="6" class="empty">暂无记录</td></tr>'}</tbody>
            </table>
          </div>
          <div class="modal-actions">
            <button class="btn ghost" onclick="closeModal()">关闭</button>
          </div>`, true);
      } catch (e) {
        openModal(`
          <h3>许愿详情</h3>
          <p class="sub">${escapeHtml(e.message)}</p>
          <div class="modal-actions"><button class="btn ghost" onclick="closeModal()">关闭</button></div>`);
      }
    }

    /** 本期未开放的动作统一提示，避免点了没反应 */
    function wqNotReady(name) {
      toast(name + '功能暂未开放');
    }
