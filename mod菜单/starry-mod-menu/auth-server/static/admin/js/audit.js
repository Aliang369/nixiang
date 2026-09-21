/* 管理后台（从 index.html 拆出，2026-09-21）
   依赖：core.js 等在前。 */
    /* -------- audit -------- */
    const AUDIT_ACTION_LABELS = {
      'admin.change_password': '修改密码',
      'games.create': '创建游戏',
      'games.update': '更新游戏',
      'games.sync_features': '同步功能',
      'cards.create': '生成卡密',
      'cards.batch': '批量更新卡密',
      'cards.batch_delete': '批量删除卡密',
      'cards.update': '更新卡密',
      'cards.delete': '删除卡密',
      'features.create': '创建功能',
      'features.update': '更新功能',
      'features.delete': '删除功能',
      'announcements.create': '创建公告',
      'announcements.reorder': '调整公告顺序',
      'announcements.update': '更新公告',
      'announcements.delete': '删除公告',
      'site_settings.update': '更新站点设置',
      'toasts.create': '创建开屏 Toast',
      'toasts.update': '更新开屏 Toast',
      'toasts.delete': '删除开屏 Toast',
    };
    const AUDIT_STATUS_LABELS = {
      active: '正常', banned: '封禁', expired: '过期', exhausted: '用尽', unused: '未使用',
    };
    const AUDIT_KEY_LABELS = {
      id: 'ID', ids: '选中数量', updated: '已更新', status: '状态',
      force_unbind: '强制解绑', reset_unbind: '重置解绑', key: '卡密',
      count: '数量', scope: '范围', game_id: '游戏', title: '标题',
      name: '名称', extend_days: '延期天数', template: '模板', mode: '模式',
    };

    function formatAuditAction(action) {
      return AUDIT_ACTION_LABELS[action] || action || '—';
    }

    function formatAuditDetail(detail) {
      if (!detail) return '—';
      if (/[\u4e00-\u9fff]/.test(detail) && detail.indexOf('=') < 0) return detail;
      return detail.split(/\s+/).filter(Boolean).map(part => {
        const eq = part.indexOf('=');
        if (eq < 0) return part;
        const k = part.slice(0, eq);
        let v = part.slice(eq + 1);
        const label = AUDIT_KEY_LABELS[k] || k;
        if (k === 'status') v = AUDIT_STATUS_LABELS[v] || v;
        if (k === 'scope') v = v === 'global' ? '全局' : (v === 'game' ? '单游戏' : v);
        if (k === 'force_unbind' || k === 'reset_unbind') v = (v === 'true' || v === '1') ? '是' : '否';
        if (k === 'mode') {
          if (v === 'fill') v = '仅补缺';
          else if (v === 'upsert') v = '覆盖更新';
        }
        return `${label}=${v}`;
      }).join('；');
    }

    async function loadAudit() {
      try {
        const qs = new URLSearchParams();
        const q = document.getElementById('au-q')?.value.trim() || '';
        const action = document.getElementById('au-action')?.value || '';
        const fromIso = tpISOString('au-from');
        const toIso = tpISOString('au-to', true);   // 结束日期含当天 23:59:59
        if (q) qs.set('q', q);
        if (action) qs.set('action', action);
        if (fromIso) qs.set('from_at', fromIso);
        if (toIso) qs.set('to_at', toIso);
        qs.set('page', String(auditPage));
        qs.set('page_size', String(auditPageSize));
        const data = await api('/v1/admin/audit-logs?' + qs.toString());
        auditCache = data.data || [];
        auditMeta = data.meta || { total: auditCache.length, page: auditPage, page_size: auditPageSize };
        auditPage = auditMeta.page || auditPage;
        document.getElementById('audit-body').innerHTML = auditCache.map(r => `
          <tr>
            <td>${r.created_at ? escapeHtml(fmtLocal(r.created_at)) : '—'}</td>
            <td>${escapeHtml(r.admin_username)}</td>
            <td>${escapeHtml(formatAuditAction(r.action))}</td>
            <td>${escapeHtml(formatAuditDetail(r.detail))}</td>
            <td class="muted">${escapeHtml(r.ip || '—')}</td>
          </tr>`).join('') || '<tr><td colspan="5" class="empty">暂无审计记录或无匹配项</td></tr>';
        renderPager('audit-pager', auditMeta, `auditPage=${auditPage - 1};loadAudit()`, `auditPage=${auditPage + 1};loadAudit()`);
      } catch (e) {
        auditCache = [];
        document.getElementById('audit-body').innerHTML =
          `<tr><td colspan="5" class="empty">${escapeHtml(e.message)}</td></tr>`;
        renderPager('audit-pager', { total: 0, page: 1, page_size: auditPageSize }, '', '');
        toast(e.message);
      }
    }

