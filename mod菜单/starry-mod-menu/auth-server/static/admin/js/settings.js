/* 管理后台 - 账号安全（修改密码弹窗，从后台设置页迁入账号菜单，2026-09-21）
   依赖：core.js 等在前。 */
    /* -------- 修改密码（账号下拉菜单弹窗） -------- */
    function openPasswordModal() {
      openModal(`
        <h3>修改密码</h3>
        <p class="sub">修改成功后需重新登录</p>
        <div class="field">
          <label for="pw-old">原密码</label>
          <input id="pw-old" type="password" autocomplete="current-password" />
        </div>
        <div class="field" style="margin-top:12px">
          <label for="pw-new">新密码（至少 6 位）</label>
          <input id="pw-new" type="password" autocomplete="new-password" />
        </div>
        <div class="field" style="margin-top:12px">
          <label for="pw-new2">确认新密码</label>
          <input id="pw-new2" type="password" autocomplete="new-password" />
        </div>
        <div class="modal-actions">
          <button class="btn ghost" onclick="closeModal()">取消</button>
          <button class="btn" onclick="changePassword()">保存新密码</button>
        </div>`);
    }

    async function changePassword() {
      try {
        const old_password = document.getElementById('pw-old').value;
        const new_password = document.getElementById('pw-new').value;
        const new2 = document.getElementById('pw-new2').value;
        if (new_password.length < 6) { toast('新密码至少 6 位'); return; }
        if (new_password !== new2) { toast('两次新密码不一致'); return; }
        await api('/v1/admin/change-password', { method: 'POST', body: JSON.stringify({ old_password, new_password }) });
        toast('密码已更新，请重新登录');
        // 改密成功直接退出，无需再走退出确认弹窗
        setTimeout(doLogout, 800);
      } catch (e) { toast(e.message); }
    }
