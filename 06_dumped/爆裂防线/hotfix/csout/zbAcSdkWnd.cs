using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbAcSdkWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnTaptap;

	private Button m_btnGuest;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btntouch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)Hide, "", (TrickleDown)0);
		m_btnTaptap = UQueryExtensions.Q<Button>(m_RootElement, "btntaptap", (string)null);
		m_btnGuest = UQueryExtensions.Q<Button>(m_RootElement, "btnguest", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTaptap, (Action<ClickEvent>)delegate
		{
			OnSdkLogin("1");
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGuest, (Action<ClickEvent>)delegate
		{
			OnSdkLogin("3");
		}, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(bool changeAct = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnTaptap).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnGuest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbLoginUtil.appChannel == EChannelType.TapTap || zbLoginUtil.appChannel == EChannelType.Ios || zbLoginUtil.appChannel == EChannelType.Dyunion)
		{
			((VisualElement)m_btnTaptap).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	public void OnSdkLogin(string loginType)
	{
		Hide();
		if (loginType.Equals("3"))
		{
			zbHttpMsgHandler.Instance.User_VisitorInit_Req();
		}
		else
		{
			zbSdkHelper.LoginGame();
		}
	}
}
