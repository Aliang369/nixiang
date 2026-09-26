using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHubTabCommunity
{
	private zbHubWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnhykb;

	private Button m_btnTapTap;

	private Button m_btnDy;

	private Button m_btnXhs;

	private Button m_btnWx;

	public void OnEnable(zbHubWnd wndParent, VisualElement root)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnhykb = UQueryExtensions.Q<Button>(m_RootElement, "btn_hykb", (string)null);
		m_btnTapTap = UQueryExtensions.Q<Button>(m_RootElement, "btn_taptap", (string)null);
		m_btnDy = UQueryExtensions.Q<Button>(m_RootElement, "btn_dy", (string)null);
		m_btnXhs = UQueryExtensions.Q<Button>(m_RootElement, "btn_xhs", (string)null);
		m_btnWx = UQueryExtensions.Q<Button>(m_RootElement, "btn_wx", (string)null);
		((VisualElement)m_btnhykb).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!Application.isEditor && zbLoginUtil.appChannel != EChannelType.Hykb) ? 1 : 0));
		((VisualElement)m_btnTapTap).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!Application.isEditor && zbLoginUtil.appChannel != EChannelType.TapTap) ? 1 : 0));
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnhykb, (Action)OnHykb, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTapTap, (Action)OnTapTap, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDy, (Action)OnDy, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnXhs, (Action)OnXhs, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWx, (Action)OnWx, "", (TrickleDown)0);
	}

	private void OnHykb()
	{
		zbSdkHelper.OpenURL("https://bbs.3839.com/forum-73205.htm");
	}

	private void OnTapTap()
	{
		zbSdkHelper.OpenURL("https://www.taptap.cn/app/895385/topic");
	}

	private void OnDy()
	{
		zbSdkHelper.OpenURL("https://v.douyin.com/LxNmOyW6nw4/ 9@8.com");
	}

	private void OnXhs()
	{
		zbSdkHelper.OpenURL("https://www.xiaohongshu.com/user/profile/68c7f551000000000500294e");
	}

	private void OnWx()
	{
		zbUIMgr.Instance.BubbleInfo("请前往微信搜索：爆裂防线");
	}
}
