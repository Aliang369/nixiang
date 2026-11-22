/* 管理后台 - 设备日志（2026-11-16 重做：复用操作日志的时间选择器/表格/分页与全局弹窗）
   依赖 core.js（api/escapeHtml/fmtLocal/renderPager/toast/copyText/openModal）
   依赖 time-picker.js（tpISOString）。只读接口：
     GET /v1/admin/events       列表（管理员；返回 card_key 明文，仅现存卡）
     GET /v1/admin/devices/{id} 单设备详情（管理员） */
    let eventsPage = 1;
    let eventsPageSize = 20;

    const EVENT_TYPE_LABELS = {
      login_ok: '登录成功', login_fail: '登录失败', heartbeat_fail: '心跳失败',
      bootstrap_reject: '启动配置被拒', activate: '激活', device_migrate: '设备迁移',
      unbind: '解绑', unbind_fail: '解绑失败', heartbeat_device_drift: '设备漂移',
      request_invalid: '请求参数非法',
    };
    const EVENT_RESULT_LABELS = { ok: '成功', deny: '拒绝', error: '错误' };
    // 原因码中文：覆盖后端所有已知 code；未知码保底显示中文 + 原码（不隐藏根因）
    const EVENT_REASON_LABELS = {
      bad_device: '设备号格式不合法',
      not_found: '卡密不存在',
      banned: '卡密已封禁',
      expired: '卡密已过期',
      exhausted: '卡密次数用尽',
      device_mismatch: '设备与绑定不一致',
      not_bound: '卡密尚未绑定设备',
      unauthorized: '未认证或会话失效',
      rate_limited: '请求过于频繁',
      validation_error: '请求参数不合法',
      denied: '被拒绝',
    };
    // 同一 code 在不同事件下含义不同（not_found：登录=卡密不存在；心跳=会话/卡已失效）
    const REASON_BY_TYPE = {
      login_fail: { not_found: '卡密不存在', device_mismatch: '设备与绑定不一致', unauthorized: '未认证' },
      heartbeat_fail: {
        not_found: '卡密已不存在（会话失效）', device_mismatch: '设备绑定已变更',
        unauthorized: '会话无效或已过期',
      },
      unbind_fail: {
        not_found: '卡密不存在', not_bound: '卡密尚未绑定设备',
        device_mismatch: '只能由当前绑定设备解绑',
      },
      bootstrap_reject: { bad_device: '设备号格式不合法' },
      request_invalid: { validation_error: '请求参数不合法' },
    };
    function formatEventType(t) { return EVENT_TYPE_LABELS[t] || (`未知事件（${t || '—'}）`); }
    function formatResult(r) { return EVENT_RESULT_LABELS[r] || (r ? `未知结果（${r}）` : '—'); }
    function formatReason(type, code) {
      if (!code) return '—';
      const ctx = REASON_BY_TYPE[type] || {};
      return ctx[code] || EVENT_REASON_LABELS[code] || `未知原因（${code}）`;
    }

    // 与卡密管理同款视觉/文案：code.card-key + title「点击复制」；复制走 document 事件委托
    // （不拼内联 JS，避免引号注入；escapeHtml 只做 HTML 编码，值走 data-copy 属性传回）
    function keyCellHtml(key) {
      const v = escapeHtml(key);
      return `<code class="card-key" title="点击复制" tabindex="0" data-copy="${v}">${v}</code>`;
    }

    function cardCell(r) {
      if (r.card_key) return keyCellHtml(r.card_key);
      if (r.card_id) return `<span class="muted">已删除（card#${r.card_id}）</span>`;
      return '—';
    }

    async function loadEvents() {
      try {
        const qs = new URLSearchParams();
        const device = document.getElementById('ev-device')?.value.trim() || '';
        const cardId = document.getElementById('ev-card-id')?.value.trim() || '';
        const type = document.getElementById('ev-type')?.value || '';
        const result = document.getElementById('ev-result')?.value || '';
        const fromIso = tpISOString('ev-from');            // 与操作日志同一组件/解析
        const toIso = tpISOString('ev-to', true);          // 结束时间含当天 23:59:59
        if (device) qs.set('device_id', device);
        if (/^\d+$/.test(cardId)) qs.set('card_id', cardId);  // 只传内部 ID，不传卡密明文
        if (type) qs.set('event_type', type);
        if (result) qs.set('result', result);
        if (fromIso) qs.set('from_at', fromIso);
        if (toIso) qs.set('to_at', toIso);
        qs.set('page', String(eventsPage));
        qs.set('page_size', String(eventsPageSize));
        const data = await api('/v1/admin/events?' + qs.toString());
        const rows = data.data || [];
        const meta = data.meta || { total: rows.length, page: eventsPage, page_size: eventsPageSize };
        eventsPage = meta.page || eventsPage;
        document.getElementById('events-body').innerHTML = rows.length
          ? rows.map(r => `
            <tr>
              <td>${r.created_at ? escapeHtml(fmtLocal(r.created_at)) : '—'}</td>
              <td>${escapeHtml(formatEventType(r.event_type))}</td>
              <td>${escapeHtml(formatResult(r.result))}</td>
              <td>${r.device_id_full
                ? `<code class="card-key" title="点击复制完整设备号" tabindex="0" data-copy="${escapeHtml(r.device_id_full)}">${escapeHtml(r.device_id || '')}</code>`
                : `<code>${escapeHtml(r.device_id || '—')}</code>`}</td>
              <td>${escapeHtml(r.game_id || '—')}</td>
              <td>${cardCell(r)}</td>
              <td>${escapeHtml(formatReason(r.event_type, r.reason))}</td>
              <td class="muted">${escapeHtml(r.ip || '—')}</td>
              <td>${r.suppressed ? '+' + r.suppressed : '—'}</td>
              <td>${r.device_id_full ? `<button class="btn ghost" type="button" data-dev="${escapeHtml(r.device_id_full)}">详情</button>` : ''}</td>
            </tr>`).join('')
          : '<tr><td colspan="10" class="empty">暂无事件或无匹配项</td></tr>';
        renderPager('events-pager', meta, `eventsPage=${eventsPage - 1};loadEvents()`, `eventsPage=${eventsPage + 1};loadEvents()`);
      } catch (e) {
        document.getElementById('events-body').innerHTML =
          `<tr><td colspan="10" class="empty">${escapeHtml(e.message)}</td></tr>`;
        renderPager('events-pager', { total: 0, page: 1, page_size: eventsPageSize }, '', '');
        toast(e.message);
      }
    }

    // 复制：与卡密管理同一 copyText；事件委托 + 键盘 Enter/Space，不拼内联 JS
    // 复制：document 事件委托（capture 阶段）。openModal 的 .modal 带 onclick=stopPropagation，
    // 冒泡阶段到不了 document，所以用 capture；仍然只有一个 document 级 listener，不拼内联 JS。
    document.addEventListener('click', (e) => {
      const t = e.target;
      const el = t && t.closest ? t.closest('code[data-copy], [data-copy]') : null;
      if (el) copyText(el.dataset.copy);
    }, true);
    document.addEventListener('keydown', (e) => {
      if (e.key !== 'Enter' && e.key !== ' ') return;
      const el = e.target.closest && e.target.closest('[data-copy]');
      if (!el) return;
      e.preventDefault();
      copyText(el.dataset.copy);
    });

    // 详情用全局 modal（遮罩点击/Escape 关闭由 openModal 自带）
    document.addEventListener('click', (e) => {
      const btn = e.target.closest && e.target.closest('button[data-dev]');
      if (btn) openDeviceDetail(btn.dataset.dev);
    });

    // 卡状态中文（枚举来自 CardKey.status 实际取值；未知保底原码，不猜）
    const CARD_STATUS_LABELS = {
      active: '使用中', unused: '未使用', used: '已使用', expired: '已过期',
      banned: '已封禁', exhausted: '次数用尽', revoked: '已撤销', disabled: '已停用',
    };
    function cardStatusLabel(s) {
      if (!s) return '未关联卡密';
      return CARD_STATUS_LABELS[s] || `未知状态（${s}）`;
    }
    // 在线状态由后端按看板同口径计算（需卡=认证时间，免卡=last_seen），前端不自行推断
    function onlineLabel(isOnline) {
      if (isOnline === true) return '在线';
      if (isOnline === false) return '离线';
      return '—';
    }

    /** 在线记录 + 绑定卡合并成同一组行：按 card_id 对齐，绑定时间/到期/状态取自 bindings */
    function mergeDetailRows(d) {
      const binds = {};
      (d.bindings || []).forEach(b => { if (b.card_id) binds[b.card_id] = b; });
      // 去重键用 (游戏, 卡)：通用卡可同时挂在多个游戏的 presence 上，不能只按 card_id 去重
      const used = new Set();
      const rows = (d.presences || []).map(p => {
        const b = p.card_id ? binds[p.card_id] : null;
        if (p.card_id) used.add(`${p.game_id}|${p.card_id}`);
        return {
          game: p.game_id, cardId: p.card_id || null, bind: b || null,
          online: p.is_online, verified: p.card_verified_at,
          seen: p.last_seen_at, created: p.created_at,
        };
      });
      (d.bindings || []).forEach(b => {
        const g = b.game_id || '全局';
        if (used.has(`${g}|${b.card_id}`)) return;   // 同一游戏下已有该卡的行，不重复
        rows.push({
          game: g, cardId: b.card_id, bind: b,
          online: null, verified: null, seen: null, created: null,
        });
      });
      return rows;
    }

    function openDeviceDetail(deviceId) {
      openModal('<div id="dev-detail-body" class="empty">加载中…</div>', true);
      api('/v1/admin/devices/' + encodeURIComponent(deviceId))
        .then(data => {
          const d = data.data || {};
          const body = document.getElementById('dev-detail-body');
          if (!body) return;
          const rows = mergeDetailRows(d).map(r => `
            <tr>
              <td>${escapeHtml(r.game || '—')}</td>
              <td>${r.cardId ? (r.bind && r.bind.key_code ? keyCellHtml(r.bind.key_code)
                    : `<span class="muted">已删除（card#${r.cardId}）</span>`) : '—'}</td>
              <td>${escapeHtml(cardStatusLabel(r.bind && r.bind.status))}</td>
              <td>${onlineLabel(r.online)}</td>
              <td>${r.verified ? escapeHtml(fmtLocal(r.verified)) : '未验证'}</td>
              <td>${r.seen ? escapeHtml(fmtLocal(r.seen)) : '—'}</td>
              <td>${r.created ? escapeHtml(fmtLocal(r.created)) : '未知（历史行）'}</td>
              <td>${r.bind && r.bind.bound_at ? escapeHtml(fmtLocal(r.bind.bound_at)) : '—'}</td>
              <td>${r.bind && r.bind.expire_at ? escapeHtml(fmtLocal(r.bind.expire_at)) : '—'}</td>
            </tr>`).join('');
          const evs = (d.events || []).slice(0, 10).map(e => `
            <tr><td>${e.created_at ? escapeHtml(fmtLocal(e.created_at)) : '—'}</td>
            <td>${escapeHtml(formatEventType(e.event_type))}</td>
            <td>${escapeHtml(formatResult(e.result))}</td>
            <td>${escapeHtml(formatReason(e.event_type, e.reason))}</td>
            <td>${e.card_key ? escapeHtml(e.card_key) : (e.card_id ? '已删除（card#' + e.card_id + '）' : '—')}</td></tr>`).join('');
          body.classList.remove('empty');
          body.innerHTML = `
            <h3>设备 ${d.device_id_full
              ? `<code class="card-key" title="点击复制完整设备号" tabindex="0" data-copy="${escapeHtml(d.device_id_full)}">${escapeHtml(d.device_id_full)}</code>`
              : escapeHtml(d.device_id || '')}</h3>
            <p class="sub">列表里设备号打码显示；点这里的设备号可复制完整值。</p>
            <p class="sub">最近成功：${d.last_success_at ? escapeHtml(fmtLocal(d.last_success_at)) : '—'} ·
               最近失败：${d.last_failure_at ? escapeHtml(fmtLocal(d.last_failure_at)) : '—'}</p>
            <div class="table-wrap"><table>
              <thead><tr><th>游戏</th><th>卡密</th><th>状态</th><th>在线</th><th>最近验证</th>
                <th>最后在线</th><th>首次观察</th><th>绑定时间</th><th>到期</th></tr></thead>
              <tbody>${rows || '<tr><td colspan="9" class="empty">无该设备记录</td></tr>'}</tbody></table></div>
            <div class="table-wrap"><table>
              <thead><tr><th>时间</th><th>事件</th><th>结果</th><th>原因</th><th>卡密</th></tr></thead>
              <tbody>${evs || '<tr><td colspan="5" class="empty">无事件</td></tr>'}</tbody></table></div>
            <p class="sub">说明：设备数≠人数；「未验证」表示本服务没有其成功验证记录；首次观察为空的属历史行；已删除的卡只能显示内部 ID。</p>`;
        })
        .catch(err => {
          const body = document.getElementById('dev-detail-body');
          if (body) body.textContent = err.message || '加载失败';
        });
    }
