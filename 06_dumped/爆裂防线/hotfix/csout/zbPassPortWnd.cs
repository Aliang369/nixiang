using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPassPortWnd : zbTabWnd
{
	private Button m_btnBack;

	private Button[] m_btns = (Button[])(object)new Button[2];

	private VisualElement[] m_layouts = (VisualElement[])(object)new VisualElement[2];

	private zbPassPortPanel m_passportPanel;

	private VisualElement m_passportRedDot;

	private zbMainQuestPanel m_mainQuesetPanel;

	private VisualElement m_mainQuestRedDot;

	private zbSpineScene m_spineScene;

	private Sprite m_iconSelect;

	private int m_index;

	public Sprite mainQuestIcon_Cur;

	public Sprite mainQuestIcon_Pre;

	public Sprite mainQuestIcon_Next;

	protected override void Awake()
	{
		base.Awake();
		m_passportPanel = new zbPassPortPanel();
		m_mainQuesetPanel = new zbMainQuestPanel();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 2; i++)
		{
			m_layouts[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"layout{i}", (string)null);
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn{i}", (string)null);
		}
		m_passportRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[0], "red_dot", (string)null);
		m_mainQuestRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[1], "red_dot", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_iconSelect = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		mainQuestIcon_Cur = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_current.png");
		mainQuestIcon_Next = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_next.png");
		mainQuestIcon_Pre = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_pre.png");
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_passportPanel.OnEnable(this, m_layouts[0]);
		m_mainQuesetPanel.OnEnable(this, m_layouts[1]);
		InitTab(m_btns, m_layouts, OnSelect);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnUpdateEventActivityUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Passport_Data, OnUpdateEventActivityUpdate);
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	public void RefreshCustom(int index)
	{
		SelectTab(index);
	}

	public override void DoSelectTab()
	{
		base.DoSelectTab();
		switch (m_index)
		{
		case 0:
			m_passportPanel.Refresh();
			break;
		case 1:
			m_mainQuesetPanel.Refresh();
			break;
		}
		SwitchScreenAndAnimator();
	}

	private void SwitchScreenAndAnimator()
	{
		switch (m_index)
		{
		case 0:
			m_spineScene.SetSceneBg("ui/image/bg/tongxing_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/tongxingzheng_bg2/tongxingzheng_bg2_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		case 1:
			m_spineScene.SetSceneBg("ui/image/bg/mainQuestPassport_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/zhanling_bg6/zhanling_bg6_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		}
	}

	private void OnSelect(int index)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		m_index = index;
		for (int i = 0; i < 2; i++)
		{
			((VisualElement)m_btns[i]).style.backgroundImage = new StyleBackground((i == m_index) ? m_iconSelect : null);
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (m_index == 0)
		{
			m_passportPanel.UpdatePerSecond();
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_index == 0)
		{
			m_passportPanel.UpdatePassportPanel();
		}
		if (m_index == 1)
		{
			m_mainQuesetPanel.UpdateMainQuestPanel();
		}
		UpdateRedDot();
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
		case 0:
			m_passportPanel.Refresh();
			break;
		case 1:
			m_mainQuesetPanel.Refresh();
			break;
		}
	}

	private void OnClose()
	{
		Hide();
		if ((Object)(object)m_spineScene != (Object)null)
		{
			m_spineScene.ClearSpine();
		}
	}
}
