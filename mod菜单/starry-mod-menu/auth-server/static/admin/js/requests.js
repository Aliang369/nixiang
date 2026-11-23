/* 管理后台 - 请求更新（2026-11-22）
   依赖 core.js（api / escapeHtml / fmtLocal / toast）
   依赖 index.html 内联（openModal / closeModal）
   接口：
     GET /v1/admin/update-requests        外层聚合，一行一个游戏（只含有请求记录的）
     GET /v1/admin/update-requests/{gid}  详情，列出该游戏全部请求人
   「已处理」「拉黑」功能本期不做，按钮先占位（后端 status 字段已预留）。 */
    let requestsCache = [];

    const RQ_STATUS_CN = { pending: '未处理', done: '已处理', blacklisted: '已拉黑' };
    const RQ_STATUS_TAG = { pending: 'muted', done: 'ok', blacklisted: 'bad' };

    /** 渠道合集 → 标签串 */
    function rqChannelsHtml(list) {
      if (!list || !list.length) return '<span class="muted">—</span>';
      return list.map(c => `<span class="tag">${escapeHtml(c)}</span>`).join(' ');
    }

    async function loadUpdateRequests() {
      const body = document.getElementById('requests-body');
      if (!body) return;
      try {
        const q = (document.getElementById('rq-q')?.value || '').trim();
        const data = await api('/v1/admin/update-requests' + (q ? '?q=' + encodeURIComponent(q) : ''));
        requestsCache = data.data || [];
        if (!requestsCache.length) {
          body.innerHTML = '<tr><td colspan="6" class="empty">暂无请求</td></tr>';
          return;
        }
        body.innerHTML = requestsCache.map(r => `
          <tr>
            <td>${escapeHtml(r.game_name)}</td>
            <td><code>${escapeHtml(r.game_id)}</code></td>
            <td>${rqChannelsHtml(r.channels)}</td>
            <td><strong>${Number(r.people) || 0}</strong> <span class="muted">人</span></td>
            <td><button class="btn sm ghost" onclick="openRequestDetail('${encodeURIComponent(r.game_id)}')">详情</button></td>
            <td class="row-actions">
              <button class="btn sm ghost" onclick="markRequestsDone('${encodeURIComponent(r.game_id)}')">处理</button>
            </td>
          </tr>`).join('');
      } catch (e) {
        body.innerHTML = `<tr><td colspan="6" class="empty">${escapeHtml(e.message)}</td></tr>`;
      }
    }

    /** 处理：把该游戏当前所有未处理请求标记为已处理，列表随即不再显示它 */
    async function markRequestsDone(gameIdEnc) {
      const gameId = decodeURIComponent(gameIdEnc);
      const item = requestsCache.find(x => x.game_id === gameId);
      const label = item ? item.game_name : gameId;
      if (!(await confirmDialog({
        title: '处理请求',
        message: `把「${label}」当前所有请求标记为已处理？\n\n处理后会从列表消失；之后有人再次请求，这个游戏会重新出现。`,
        okText: '标记已处理'
      }))) return;
      try {
        const res = await api('/v1/admin/update-requests/' + encodeURIComponent(gameId) + '/done', { method: 'PATCH' });
        toast(res.message || '已处理');
        await loadUpdateRequests();
      } catch (e) {
        toast(e.message);
      }
    }

    async function openRequestDetail(gameIdEnc) {
      const gameId = decodeURIComponent(gameIdEnc);
      openModal('<div class="empty">加载中…</div>', true);
      try {
        const res = await api('/v1/admin/update-requests/' + encodeURIComponent(gameId));
        const d = res.data || {};
        const rows = (d.items || []).map(it => `
          <tr>
            <td>${escapeHtml(it.wp_username || '—')}</td>
            <td>${it.channel ? `<span class="tag">${escapeHtml(it.channel)}</span>` : '<span class="muted">—</span>'}</td>
            <td><code>${escapeHtml(it.ip || '—')}</code></td>
            <td class="rq-problem">${escapeHtml(it.problem || '')}</td>
            <td><span class="tag ${RQ_STATUS_TAG[it.status] || 'muted'}">${escapeHtml(RQ_STATUS_CN[it.status] || it.status || '—')}</span></td>
            <td class="muted rq-time">${fmtLocal(it.created_at)}</td>
            <td><button class="btn sm ghost" onclick="rqNotReady('拉黑')">拉黑</button></td>
          </tr>`).join('');
        openModal(`
          <h3>请求详情 · ${escapeHtml(d.game_name || gameId)}</h3>
          <p class="sub">共 ${Number(d.total) || 0} 条提交 · 去重 ${Number(d.people) || 0} 人</p>
          <div class="table-wrap">
            <table>
              <thead><tr><th>提交人</th><th>渠道</th><th>IP</th><th>游戏问题</th><th>状态</th><th>时间</th><th>操作</th></tr></thead>
              <tbody>${rows || '<tr><td colspan="7" class="empty">暂无记录</td></tr>'}</tbody>
            </table>
          </div>
          <div class="modal-actions">
            <button class="btn ghost" onclick="closeModal()">关闭</button>
          </div>`, true);
      } catch (e) {
        openModal(`
          <h3>请求详情</h3>
          <p class="sub">${escapeHtml(e.message)}</p>
          <div class="modal-actions"><button class="btn ghost" onclick="closeModal()">关闭</button></div>`);
      }
    }

    /** 本期未开放的两个动作统一提示，避免点了没反应 */
    function rqNotReady(name) {
      toast(name + '功能暂未开放');
    }
