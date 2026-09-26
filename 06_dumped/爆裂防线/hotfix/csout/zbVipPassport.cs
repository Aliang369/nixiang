using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipPassport
{
	private Button m_btnBack;

	private Button[] m_btns = (Button[])(object)new Button[2];

	private VisualElement[] m_layouts = (VisualElement[])(object)new VisualElement[2];

	private zbPassPortPanel m_passportPanel;

	private VisualElement m_passportRedDot;

	private zbMainQuestPanel m_mainQuesetPanel;

	private VisualElement m_mainQuestRedDot;

	private zbSpineScene m_spineScene;

	private zbVipWnd m_wndParent;

	private Sprite m_iconSelect;

	public int m_index;

	public void OnEnable(zbVipWnd wndParent, VisualElement m_RootElement)
	{
		m_wndParent = wndParent;
		for (int i = 0; i < 2; i++)
		{
			m_layouts[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"layout{i}", (string)null);
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn{i}", (string)null);
			int tempIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btns[i], (Action<ClickEvent>)delegate
			{
				SelectTab(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_passportRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[1], "red_dot", (string)null);
		m_mainQuestRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[0], "red_dot", (string)null);
		m_spineScene = ((Component)m_wndParent.GetMyGameObjectSceneRoot().transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_iconSelect = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnUpdateEventActivityUpdate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnUpdateEventActivityUpdate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Passport_Data, OnUpdateEventActivityUpdate);
	}

	public void RefreshCustom(int index = 0)
	{
		m_index = index;
		SelectTab(m_index);
	}

	private void SelectTab(int index)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		m_index = index;
		switch (m_index)
		{
		case 1:
			if (m_passportPanel == null)
			{
				m_passportPanel = new zbPassPortPanel();
				m_passportPanel.OnEnable(m_wndParent, m_layouts[1]);
			}
			m_passportPanel.Refresh();
			break;
		case 0:
			if (m_mainQuesetPanel == null)
			{
				m_mainQuesetPanel = new zbMainQuestPanel();
				m_mainQuesetPanel.OnEnable(m_wndParent, m_layouts[0]);
			}
			m_mainQuesetPanel.Refresh();
			break;
		}
		for (int i = 0; i < 2; i++)
		{
			((VisualElement)m_btns[i]).style.backgroundImage = new StyleBackground((i == m_index) ? m_iconSelect : null);
			m_layouts[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i != index) ? 1 : 0));
		}
		SwitchScreenAndAnimator();
	}

	private void SwitchScreenAndAnimator()
	{
		switch (m_index)
		{
		case 1:
			m_spineScene.SetSceneBg("ui/image/bg2/tongxing_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/tongxingzheng_bg2/renwutongxing_bg_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		case 0:
			m_spineScene.SetSceneBg("ui/image/bg/mainQuestPassport_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/zhanling_bg6/zhanling_bg6_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		}
	}

	public void UpdateAtShow()
	{
		if (m_index == 1 && m_passportPanel != null)
		{
			m_passportPanel.UpdatePassportPanel();
		}
		if (m_index == 0 && m_mainQuesetPanel != null)
		{
			m_mainQuesetPanel.UpdateMainQuestPanel();
		}
		UpdateRedDot();
	}

	public void UpdatePerSecond()
	{
		if (m_index == 1 && m_passportPanel != null)
		{
			m_passportPanel.UpdatePerSecond();
		}
	}

	private void UpdateRedDot()
	{
		m_passportRedDot.visible = zbSelf.Instance.passportHandler.HasPassportRedPoint();
		m_mainQuestRedDot.visible = zbSelf.Instance.passportHandler.HasMainQuestRedPoint();
	}

	private void OnUpdateEventActivityUpdate(object data)
	{
		switch (m_index)
		{
		case 1:
			m_passportPanel.Refresh();
			break;
		case 0:
			m_mainQuesetPanel.Refresh();
			break;
		}
	}
}
