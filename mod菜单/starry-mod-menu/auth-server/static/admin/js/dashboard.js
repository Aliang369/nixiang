/* 管理后台 - 数据看板（从 index.html 拆出，2026-09-21）
   依赖：core.js（api/toast/renderListStatus 等）。 */
    /* -------- dashboard -------- */
    async function loadDashboard(silent) {
      try {
        const data = await api('/v1/admin/dashboard');
        const d = data.data;
        const oc = d.online_card ?? 0;
        const og = d.online_guest ?? Math.max((d.total_online || 0) - oc, 0);
        const tc = d.today_card_devices ?? 0;
        const tg = d.today_guest_devices ?? 0;
        const cu = d.cards_unused ?? 0;
        const a7c = d.d7_card_devices ?? 0;
        const a7g = d.d7_guest_devices ?? 0;
        const a30c = d.d30_card_devices ?? 0;
        const a30g = d.d30_guest_devices ?? 0;
        const aAc = d.all_card_devices ?? 0;
        const aAg = d.all_guest_devices ?? 0;
        document.getElementById('stats').innerHTML = `
          <button class="stat" type="button">
            <div class="k">总在线</div><div class="v">${d.total_online}</div>
            <div class="s">设备 · 卡密 ${oc} · 游客 ${og}</div>
          </button>
          <button class="stat" onclick="switchPage('games')">
            <div class="k">游戏数</div><div class="v">${d.games_count ?? 0}</div>
          </button>
          <button class="stat" onclick="jumpCardsFilter('')">
            <div class="k">卡密总数</div><div class="v">${d.cards_total}</div>
            <div class="s">激活 ${d.cards_active} · 未激活 ${cu}</div>
          </button>
          <button class="stat" onclick="jumpCardsFilter('banned')">
            <div class="k">封禁</div><div class="v">${d.cards_banned}</div>
          </button>`;
        var todayEl = document.getElementById('stats-today');
        if (todayEl) {
          todayEl.style.display = '';
          todayEl.innerHTML = `
          <button class="stat" type="button">
            <div class="k">今日活跃</div><div class="v">${d.today_active_devices ?? 0}</div>
            <div class="s">卡密 ${tc} · 游客 ${tg}</div>
          </button>
          <button class="stat" type="button">
            <div class="k">7 日活跃</div><div class="v">${d.d7_active_devices ?? 0}</div>
            <div class="s">卡密 ${a7c} · 游客 ${a7g}</div>
          </button>
          <button class="stat" type="button">
            <div class="k">30 日活跃</div><div class="v">${d.d30_active_devices ?? 0}</div>
            <div class="s">卡密 ${a30c} · 游客 ${a30g}</div>
          </button>
          <button class="stat" type="button">
            <div class="k">历史活跃</div><div class="v">${d.all_active_devices ?? 0}</div>
            <div class="s">卡密 ${aAc} · 游客 ${aAg}</div>
          </button>`;
        }
        dashOnlineCache = d.online_by_game || [];
        renderDashOnline();
      } catch (e) { if (!silent) toast(e.message); }
    }

    let dashOnlinePage = 1; // 分游戏活跃本地分页（20/页，与 core.js 各列表一致）

    function renderDashOnline() {
      const q = document.getElementById('d-q')?.value || '';
      const gameId = getGamePickerValue('d-filter-game');
      const rows = dashOnlineCache.filter(r => {
        if (gameId && r.game_id !== gameId) return false;
        const g = gamesCache.find(x => x.game_id === r.game_id);
        const name = g ? g.name : r.game_id;
        return textMatch(q, name, r.game_id);
      });
      const pageSize = 20;
      const total = rows.length;
      const pages = Math.max(1, Math.ceil(total / pageSize));
      if (dashOnlinePage > pages) dashOnlinePage = pages;
      const pageRows = rows.slice((dashOnlinePage - 1) * pageSize, dashOnlinePage * pageSize);
      document.getElementById('online-by-game').innerHTML = pageRows.length
        ? pageRows.map(r => {
            const g = gamesCache.find(x => x.game_id === r.game_id);
            const name = g ? g.name : r.game_id;
            return `<tr>
              <td><code>${escapeHtml(r.game_id)}</code></td>
              <td><strong>${escapeHtml(name)}</strong></td>
              <td>${r.online ?? 0}</td>
              <td>${r.online_card ?? 0}</td>
              <td>${r.online_guest ?? 0}</td>
              <td><strong>${r.d7_active ?? 0}</strong></td>
              <td>${r.d30_active ?? 0}</td>
              <td>${r.all_active ?? 0}</td>
            </tr>`;
          }).join('')
        : `<tr><td colspan="8" class="empty">${dashOnlineCache.length ? '无匹配游戏' : '暂无游戏数据'}</td></tr>`;
      renderPager('dash-pager', { total, page: dashOnlinePage, page_size: pageSize },
        `dashOnlinePage=${dashOnlinePage - 1};renderDashOnline()`,
        `dashOnlinePage=${dashOnlinePage + 1};renderDashOnline()`);
    }

