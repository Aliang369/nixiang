/* 管理后台 - 开屏 Toast + 游戏补丁 OTA（从 index.html 拆出，2026-09-21）
   依赖：core.js 等在前。 */
    /* -------- toasts（开屏） -------- */
    /* -------- 游戏补丁 OTA -------- */
    let otaListCache = [];

    function pickOtaFile(kind) {
      const input = document.getElementById(kind === 'so' ? 'ota-so' : 'ota-json');
      if (input) input.click();
    }

    function onOtaFilePicked(kind, input) {
      const nameEl = document.getElementById(kind === 'so' ? 'ota-so-name' : 'ota-json-name');
      const drop = document.getElementById(kind === 'so' ? 'ota-drop-so' : 'ota-drop-json');
      const f = input?.files?.[0];
      if (nameEl) nameEl.textContent = f ? f.name : '未选择文件';
      if (drop) drop.classList.toggle('has-file', !!f);
    }

    function bindOtaDropZones() {
      [['ota-drop-json', 'ota-json', 'json'], ['ota-drop-so', 'ota-so', 'so']].forEach(([dropId, inputId, kind]) => {
        const drop = document.getElementById(dropId);
        const input = document.getElementById(inputId);
        if (!drop || !input || drop.dataset.dnd === '1') return;
        drop.dataset.dnd = '1';
        ['dragenter', 'dragover'].forEach(ev => {
          drop.addEventListener(ev, e => {
            e.preventDefault();
            e.stopPropagation();
            drop.classList.add('dragover');
          });
        });
        ['dragleave', 'drop'].forEach(ev => {
          drop.addEventListener(ev, e => {
            e.preventDefault();
            e.stopPropagation();
            drop.classList.remove('dragover');
          });
        });
        drop.addEventListener('drop', e => {
          const files = e.dataTransfer?.files;
          if (!files?.length) return;
          const dt = new DataTransfer();
          dt.items.add(files[0]);
          input.files = dt.files;
          onOtaFilePicked(kind, input);
        });
      });
    }

    async function loadOtaPage() {
      if (!gamesCache.length) await loadGamesCatalog();
      try {
        const data = await api('/v1/admin/ota');
        otaListCache = data.data || [];
      } catch (e) {
        otaListCache = [];
        toast(e.message || '加载补丁列表失败');
      }
      renderOtaTable();
    }

    function renderOtaTable() {
      const body = document.getElementById('ota-body');
      if (!body) return;
      const rows = (otaListCache || []).slice().sort((a, b) => {
        const ga = gamesCache.find(g => g.game_id === a.game_id);
        const gb = gamesCache.find(g => g.game_id === b.game_id);
        const ta = ga?.created_at ? Date.parse(ga.created_at) : 0;
        const tb = gb?.created_at ? Date.parse(gb.created_at) : 0;
        if (tb !== ta) return tb - ta;
        return String(a.game_id || '').localeCompare(String(b.game_id || ''));
      });
      body.innerHTML = rows.map(r => {
        const gid = r.game_id || '';
        const name = gameLabel(gid);
        const jsha = (r.json_sha256 || '').slice(0, 10);
        const ssha = (r.so_sha256 || '').slice(0, 10);
        return `<tr>
          <td><strong>${escapeHtml(name)}</strong><div class="muted"><code>${escapeHtml(gid)}</code></div></td>
          <td>v${escapeHtml(r.ver)}</td>
          <td>${r.has_json ? `<span class="tag ok">有</span> <code class="muted">${escapeHtml(jsha)}</code>` : '<span class="tag warn">无</span>'}</td>
          <td>${r.has_so ? `<span class="tag ok">有</span> <code class="muted">${escapeHtml(ssha)}</code>` : '<span class="tag warn">无</span>'}</td>
          <td>${r.note ? escapeHtml(r.note) : '<span class="muted">—</span>'}</td>
          <td class="row-actions">
            <button type="button" class="btn sm ghost" onclick='openOtaPublishModal(${JSON.stringify(gid)})'>更新</button>
            <button type="button" class="btn sm danger" onclick='openOtaDeleteModal(${JSON.stringify(gid)})'>删除</button>
          </td>
        </tr>`;
      }).join('') || '<tr><td colspan="6" class="empty">暂无补丁，点右上角「添加补丁」发布</td></tr>';
    }

    async function openOtaPublishModal(presetGameId) {
      if (!gamesCache.length) await loadGamesCatalog();
      const gid0 = presetGameId || '';
      openModal(`
        <h3>${gid0 ? '更新补丁' : '添加补丁'}</h3>
        <p class="sub">${gid0
          ? '给该游戏发更高 ver；只传 JSON 或 so 之一也可，未传的一侧保留原文件。'
          : '发布新游戏补丁；只传 JSON 或 so 之一也可。'}</p>
        <div class="form-grid">
          <div class="full"><label>游戏</label>
            ${gamePickerMarkup('ota-game', { emptyLabel: '请选择游戏', allowEmpty: true, onchange: 'onOtaGameChange' })}
          </div>
          <div><label for="ota-ver">新版本号</label>
            <input id="ota-ver" type="number" min="1" placeholder="须大于当前 ver" /></div>
          <div><label for="ota-note">说明（玩家可见）</label>
            <input id="ota-note" placeholder="如：热更偏移 / 修闪退" maxlength="200" /></div>
          <div class="full ota-status" id="ota-current" style="padding:12px;border-radius:12px;border:1px solid var(--line);background:rgba(10,8,28,.35);font-size:13px;line-height:1.6">
            <div class="muted">选择游戏后显示当前 latest</div>
          </div>
          <div class="full">
            <div class="ota-drop-grid">
              <div class="ota-drop" id="ota-drop-json">
                <h4>patch.json</h4>
                <p>小补丁。不选则保留已有 JSON。</p>
                <div class="ota-file-name" id="ota-json-name">未选择文件</div>
                <button type="button" class="ota-pick" onclick="pickOtaFile('json')">选择 JSON</button>
                <input id="ota-json" class="ota-file-input" type="file" accept=".json,application/json" onchange="onOtaFilePicked('json', this)" />
              </div>
              <div class="ota-drop" id="ota-drop-so">
                <h4>libstarry_magic.so</h4>
                <p>玩法 so。不选则保留已有 so。</p>
                <div class="ota-file-name" id="ota-so-name">未选择文件</div>
                <button type="button" class="ota-pick" onclick="pickOtaFile('so')">选择 so</button>
                <input id="ota-so" class="ota-file-input" type="file" accept=".so,application/octet-stream" onchange="onOtaFilePicked('so', this)" />
              </div>
            </div>
          </div>
        </div>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn" id="ota-publish-btn" onclick="publishOta()">发布</button>
        </div>`, 'wide');
      fillGameSelects();
      bindOtaDropZones();
      if (gid0) {
        // 更新：锁定到该行游戏
        setGamePickerValue('ota-game', gid0, true);
        const root = getPickerRoot('ota-game');
        if (root) {
          const trigger = root.querySelector('.game-picker-trigger');
          if (trigger) trigger.disabled = true;
        }
      }
      await onOtaGameChange();
    }

    function openOtaDeleteModal(gid) {
      if (!gid) { toast('无游戏 id'); return; }
      const name = gameLabel(gid);
      openModal(`
        <h3>删除补丁</h3>
        <p class="sub"><strong>${escapeHtml(name)}</strong>（<code>${escapeHtml(gid)}</code>）</p>
        <p class="muted" style="margin:0 0 12px">可只删一侧，或清空该游戏全部补丁。</p>
        <div class="modal-actions" style="flex-wrap:wrap;justify-content:flex-start">
          <button type="button" class="btn ghost" onclick='clearOta(${JSON.stringify(gid)}, "json")'>只删 JSON</button>
          <button type="button" class="btn ghost" onclick='clearOta(${JSON.stringify(gid)}, "so")'>只删 so</button>
          <button type="button" class="btn danger" onclick='clearOta(${JSON.stringify(gid)}, "all")'>全部删除</button>
          <span class="spacer" style="flex:1"></span>
          <button type="button" class="btn ghost" onclick="closeModal()">取消</button>
        </div>`);
    }

    async function onOtaGameChange() {
      const gid = getGamePickerValue('ota-game');
      const box = document.getElementById('ota-current');
      const verEl = document.getElementById('ota-ver');
      if (!box) return;
      if (!gid) {
        box.innerHTML = '<div class="muted">选择游戏后显示当前 latest</div>';
        if (verEl) verEl.value = '';
        return;
      }
      box.innerHTML = '<div class="muted">加载中…</div>';
      try {
        const data = await api('/v1/admin/ota/' + encodeURIComponent(gid) + '/manifest');
        const m = data.data;
        if (!m) {
          box.innerHTML = '<div><strong>尚无补丁</strong></div><div class="muted">可直接上传 JSON 和/或 so 发布 v1</div>';
          if (verEl) verEl.value = '1';
          return;
        }
        box.innerHTML = `
          <div><strong>当前 v${escapeHtml(m.ver)}</strong> · JSON ${m.has_json ? '有' : '无'} · so ${m.has_so ? '有' : '无'}</div>
          <div class="muted">${m.note ? escapeHtml(m.note) : '无说明'}</div>`;
        if (verEl) verEl.value = String((m.ver || 0) + 1);
      } catch (e) {
        box.innerHTML = `<span class="tag bad">${escapeHtml(e.message)}</span>`;
      }
    }

    async function publishOta() {
      const gid = getGamePickerValue('ota-game');
      const ver = parseInt(document.getElementById('ota-ver')?.value || '0', 10);
      const note = document.getElementById('ota-note')?.value || '';
      const jf = document.getElementById('ota-json')?.files?.[0];
      const sf = document.getElementById('ota-so')?.files?.[0];
      const btn = document.getElementById('ota-publish-btn');
      if (!gid) { toast('请选择游戏'); return; }
      if (!ver || ver < 1) { toast('请填写版本号'); return; }
      if (!jf && !sf) { toast('请至少选择 JSON 或 so 之一（另一侧会自动保留）'); return; }
      const fd = new FormData();
      fd.append('ver', String(ver));
      fd.append('note', note);
      if (jf) fd.append('patch_json', jf);
      if (sf) fd.append('so_file', sf);
      if (btn) { btn.disabled = true; btn.textContent = '发布中…'; }
      try {
        const headers = {};
        if (token) headers.Authorization = 'Bearer ' + token;
        const res = await fetch(API + '/v1/admin/ota/' + encodeURIComponent(gid) + '/publish', {
          method: 'POST', headers, body: fd
        });
        const data = await res.json().catch(() => ({ ok: false, message: '' }));
        if (res.status === 401) {
          forceLogout('登录已过期，请重新登录');
          return;
        }
        if (!res.ok || data.ok === false) throw new Error(chineseApiError(res.status, data.message || data.detail));
        const m = data.data || {};
        toast(data.message || ('已发布 v' + m.ver));
        closeModal();
        await loadOtaPage();
      } catch (e) {
        toast(e.message || '发布失败');
      } finally {
        if (btn) { btn.disabled = false; btn.textContent = '发布'; }
      }
    }

    async function clearOta(gid, what) {
      gid = gid || getGamePickerValue('ota-game');
      what = what || 'all';
      if (!gid) { toast('请选择游戏'); return; }
      const labels = {
        all: '删除该游戏全部补丁（JSON+so）？',
        json: '只删 JSON，保留 so？',
        so: '只删 so，保留 JSON？',
      };
      if (!confirm((labels[what] || '清理') + '\n游戏：' + gid)) return;
      try {
        const data = await api('/v1/admin/ota/' + encodeURIComponent(gid) + '?what=' + encodeURIComponent(what), {
          method: 'DELETE'
        });
        toast(data.message || '已删除');
        closeModal();
        await loadOtaPage();
      } catch (e) {
        toast(e.message || '删除失败');
      }
    }

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
            <p class="muted" style="margin:6px 0 0">全游戏：所有 game_id 启动都显示；单游戏：只对选中的那款游戏显示（如只给战魂 zhmr）</p>
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
      if (!confirm('删除这条开屏 Toast？')) return;
      try { await api('/v1/admin/toasts/' + id, { method: 'DELETE' }); loadToasts(); }
      catch (e) { toast(e.message); }
    }

