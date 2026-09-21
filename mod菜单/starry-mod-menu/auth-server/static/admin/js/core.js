/* 管理后台通用层：全局常量/状态 + 基础工具（从 index.html 拆出，2026-09-21）
   依赖：无；被后续业务脚本使用。顶层声明均为全局。 */
    const API = location.origin;
    const PAGE_META = {
      dashboard: ['数据看板', '在线/今日活跃（按设备）与卡密概况'],
      games: ['游戏列表', '维护 game_id；点「功能管理」开关/删除该游戏功能'],
      templates: ['菜单模板', '维护功能清单，创建游戏时一键同步'],
      cards: ['卡密管理', '生成、筛选、封禁与解绑'],
      announcements: ['远程公告', '全游戏 / 单游戏远程公告'],
      ota: ['游戏补丁', 'JSON + so 合并发布；每游戏只保留 latest'],
      toasts: ['开屏 Toast', '启动时依次弹出的提示'],
      audit: ['操作日志', '管理操作审计记录'],
      settings: ['后台设置', '设置项整理中'],
    };
    const STATUS_CN = { unused: '未使用', active: '使用中', expired: '已过期', banned: '已封禁', exhausted: '次数用尽' };
    const TYPE_CN = { hour: '小时', day: '天', month: '月', year: '年', permanent: '永久', count: '次数' };
    const VIS_CN = { always: '始终', logged_out: '仅未登录', logged_in: '仅已登录' };
    const POPUP_CN = { every_open: '每次打开', once_device: '每设备一次' };

    let token = localStorage.getItem('sg_admin_token') || '';
    let sessionExpiredHandling = false;
    let gamesCache = []; // 下拉/侧栏用目录（最多 500）
    let gamesRows = [];  // 游戏管理页当前页
    let menuTemplates = [];
    let selectedFeatGameId = '';
    let featuresCache = [];
    let cardsCache = [];
    let cardsPage = 1;
    let cardsPageSize = 20;
    let cardsMeta = { total: 0, page: 1, page_size: 20 };
    let announcementsCache = [];
    let toastsCache = [];
    let auditCache = [];
    let dashOnlineCache = [];
    let gamesPage = 1, gamesPageSize = 20, gamesMeta = { total: 0, page: 1, page_size: 20 };
    let annPage = 1, annPageSize = 20, annMeta = { total: 0, page: 1, page_size: 20 };
    let toastPage = 1, toastPageSize = 20, toastMeta = { total: 0, page: 1, page_size: 20 };
    let auditPage = 1, auditPageSize = 20, auditMeta = { total: 0, page: 1, page_size: 20 };
    let featPage = 1, featPageSize = 20, featMeta = { total: 0, page: 1, page_size: 20 };
    let selectedCardIds = new Set();
    let expandedDevices = new Set();
    let currentPage = 'dashboard';

    function textMatch(q, ...parts) {
      const n = (q || '').trim().toLowerCase();
      if (!n) return true;
      return parts.some(p => String(p == null ? '' : p).toLowerCase().includes(n));
    }

    function renderListStatus(elId, shown, total) {
      const el = document.getElementById(elId);
      if (!el) return;
      if (!total && !shown) {
        el.innerHTML = '<span class="muted">共 0 条</span>';
        return;
      }
      el.innerHTML = shown === total
        ? `<span class="muted">共 ${total} 条</span>`
        : `<span class="muted">显示 ${shown} / 共 ${total} 条</span>`;
    }

    function renderPager(elId, meta, onPrev, onNext) {
      const el = document.getElementById(elId);
      if (!el) return;
      const total = meta.total || 0;
      const page = meta.page || 1;
      const size = meta.page_size || 50;
      const pages = Math.max(1, Math.ceil(total / size) || 1);
      el.innerHTML = `
        <span class="muted">共 ${total} 条 · 第 ${page}/${pages} 页</span>
        <div class="pager-actions">
          <button class="btn sm ghost" ${page <= 1 ? 'disabled' : ''} onclick="${onPrev}">上一页</button>
          <button class="btn sm ghost" ${page >= pages ? 'disabled' : ''} onclick="${onNext}">下一页</button>
        </div>`;
    }

    function boolParam(v) {
      if (v === '1') return 'true';
      if (v === '0') return 'false';
      return '';
    }

    function toast(msg) {
      const el = document.getElementById('toast');
      el.textContent = msg;
      el.classList.add('show');
      setTimeout(() => el.classList.remove('show'), 2200);
    }

    function chineseApiError(status, message) {
      const msg = (message || '').trim();
      if (msg && /[\u4e00-\u9fff]/.test(msg)) return msg;
      const byRaw = {
        'unauthorized': '未登录或登录已过期',
        'forbidden': '没有权限',
        'not found': '资源不存在',
        'bad request': '请求参数有误',
        'internal server error': '服务器内部错误',
        'invalid response': '无效响应',
        'network error': '网络错误',
        'failed to fetch': '网络请求失败',
        'load failed': '网络请求失败',
        'networkerror when attempting to fetch resource.': '网络请求失败',
        'the operation was aborted.': '请求已取消',
        'timeout': '请求超时',
        'ssl error': '证书校验失败',
        'certificate': '证书校验失败',
      };
      if (msg) {
        const key = msg.toLowerCase().replace(/^http\s+\d+\s*/, '').trim();
        if (byRaw[key]) return byRaw[key];
        if (/failed to fetch|networkerror|load failed/i.test(msg)) return '网络请求失败';
        if (/ssl|certificate|cert /i.test(msg)) return '证书校验失败';
        if (/timeout|timed out/i.test(msg)) return '请求超时';
        if (/[\u4e00-\u9fff]/.test(msg)) return msg;
        // 纯英文短消息 → 中文包装，避免 toast 直接弹出英文
        if (/^[A-Za-z0-9][\w\s\-.:'/()]+$/.test(msg) && msg.length < 120) {
          if (/^HTTP\s+\d+$/i.test(msg)) {
            /* fall through to byStatus */
          } else if (!/^(error|ok|fail(ed)?|true|false)$/i.test(msg)) {
            return '请求失败：' + msg;
          }
        }
      }
      const byStatus = {
        400: '请求参数有误',
        401: '未登录或登录已过期',
        403: '没有权限',
        404: '资源不存在',
        409: '冲突，请刷新后重试',
        422: '数据校验失败',
        429: '请求过于频繁',
        500: '服务器内部错误',
        502: '网关错误',
        503: '服务暂时不可用',
      };
      return byStatus[status] || (msg ? ('请求失败：' + msg) : ('请求失败（' + status + '）'));
    }

    async function api(path, opts = {}) {
      const headers = Object.assign({ 'Content-Type': 'application/json' }, opts.headers || {});
      if (token) headers.Authorization = 'Bearer ' + token;
      let res;
      try {
        res = await fetch(API + path, { ...opts, headers });
      } catch (e) {
        throw new Error(chineseApiError(0, e && e.message === 'Failed to fetch' ? 'failed to fetch' : 'network error'));
      }
      const data = await res.json().catch(() => ({ ok: false, message: '' }));
      if (res.status === 401 && !String(path).includes('/admin/login')) {
        forceLogout('登录已过期，请重新登录');
        throw new Error('登录已过期，请重新登录');
      }
      if (!res.ok || data.ok === false) {
        const err = new Error(chineseApiError(res.status, data.message));
        err.status = res.status;  // delGame 等需要区分 409 冲突
        throw err;
      }
      return data;
    }

    function forceLogout(msg) {
      if (sessionExpiredHandling) return;
      sessionExpiredHandling = true;
      token = '';
      localStorage.removeItem('sg_admin_token');
      closeModal();
      const app = document.getElementById('app-view');
      const login = document.getElementById('login-view');
      if (app) app.classList.add('hidden');
      if (login) login.classList.remove('hidden');
      toast(msg || '登录已过期，请重新登录');
      setTimeout(() => { sessionExpiredHandling = false; }, 800);
    }

    function statusTag(s) {
      const map = { active: 'ok', unused: '', banned: 'bad', expired: 'warn', exhausted: 'warn' };
      return `<span class="tag ${map[s] || ''}">${STATUS_CN[s] || s}</span>`;
    }

    function escapeHtml(s) {
      return String(s || '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
    }

    function copyText(text) {
      if (!text) return;
      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).then(() => toast('已复制')).catch(() => fallbackCopy(text));
      } else fallbackCopy(text);
    }
    function fallbackCopy(text) {
      const ta = document.createElement('textarea');
      ta.value = text; document.body.appendChild(ta); ta.select();
      try { document.execCommand('copy'); toast('已复制'); } catch (e) { toast('复制失败'); }
      document.body.removeChild(ta);
    }

    function downloadText(filename, text) {
      const blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = filename;
      a.click();
      URL.revokeObjectURL(a.href);
    }

