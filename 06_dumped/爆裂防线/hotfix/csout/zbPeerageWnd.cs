using System;
using Proto.Enums;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPeerageWnd : zbTabWnd
{
	private Button m_btnBack;

	private Button m_btnPassport;

	private VisualElement m_nodeTabs;

	private Button[] m_btns = (Button[])(object)new Button[2];

	private VisualElement[] m_layouts = (VisualElement[])(object)new VisualElement[2];

	private zbPeeragePanel m_pannelPeerage;

	private zbDailyTaskPanel m_pannelDailyTask;

	private zbSpineScene m_spineScene;

	private Sprite m_iconSelect;

	private Sprite m_iconUnSelect;

	private SpineVisualElement m_spineShuzi;

	private VisualElement m_peerageRedDot;

	private VisualElement m_dailyRedDot;

	private VisualElement m_passportRedDot;

	private int m_index;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_nodeTabs = UQueryExtensions.Q<VisualElement>(m_RootElement, "btns", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_layouts[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, $"layout{i}", (string)null);
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, $"btn{i}", (string)null);
		}
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_iconSelect = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		m_iconUnSelect = null;
		m_spineShuzi = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spineShuzi", (string)null);
		m_btnPassport = UQueryExtensions.Q<Button>(m_RootElement, "btn_passport", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPassport, (Action)OnPassPort, "", (TrickleDown)0);
		m_peerageRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[0], "red_dot", (string)null);
		m_dailyRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btns[1], "red_dot", (string)null);
		m_passportRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnPassport, "red_dot", (string)null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Peerage_Data, OnUpdateEventPeerageData);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DailyTask_Data, OnUpdateEventDailyTaskData);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Peerage_Upgrade, OnUpdateEventPeerageUpgrade);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		InitTab(m_btns, m_layouts, OnSelect);
	}

	public override void Refresh()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		m_nodeTabs.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Task) ? 1 : 0));
		((VisualElement)m_btnPassport).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbServerData.sGameData.MainQuestInfo.MainLevel < 3));
		SelectTab(0);
		RefreshPeerageIcon();
		UpdateRedPoint();
	}

	private void OnSelect(int index)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		m_index = index;
		for (int i = 0; i < 2; i++)
		{
			((VisualElement)m_btns[i]).style.backgroundImage = new StyleBackground((i == m_index) ? m_iconSelect : m_iconUnSelect);
		}
	}

	public override void DoSelectTab()
	{
		switch (m_index)
		{
		case 0:
			if (m_pannelPeerage == null)
			{
				m_pannelPeerage = new zbPeeragePanel();
				m_pannelPeerage.OnEnable(this, m_layouts[0]);
			}
			zbWsMsgHandler.Instance.Peerage_Data_Req();
			break;
		case 1:
			if (m_pannelDailyTask == null)
			{
				m_pannelDailyTask = new zbDailyTaskPanel();
				m_pannelDailyTask.OnEnable(this, m_layouts[1]);
			}
			zbWsMsgHandler.Instance.DailyTask_Data_Req();
			break;
		}
		SwitchScreenAndAnimator();
	}

	private void OnUpdateEventPeerageData(object data)
	{
		if (m_pannelPeerage != null)
		{
			m_pannelPeerage.Refresh();
		}
		UpdateRedPoint();
	}

	private void OnUpdateEventPeerageUpgrade(object data)
	{
		RefreshPeerageIcon();
		UpdateRedPoint();
	}

	public void OnUpdateEventDailyTaskData(object data)
	{
		RefreshDailyTask();
	}

	private void OnClose()
	{
		Hide();
		if ((Object)(object)m_spineScene != (Object)null)
		{
			m_spineScene.ClearSpine();
		}
	}

	private void OnPassPort()
	{
		zbUIMgr.Instance.ShowVip(4, 1);
	}

	private void SwitchScreenAndAnimator()
	{
		switch (m_index)
		{
		case 0:
			m_spineScene.SetSceneBg("ui/image/bg/juewei_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/juewei_bg/juewei_bg_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		case 1:
			m_spineScene.SetSceneBg("ui/image/bg/meiri_bg.jpg");
			m_spineScene.SetSkeletonDataAsset("animation/ui/meiri_bg/meirirenwu_bg_SkeletonData.asset");
			m_spineScene.SetAnimation("animation", isLoop: true);
			break;
		}
	}

	public void RefreshPeerageIcon()
	{
		zbPeerageCsvRecord recordByKey = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(zbServerData.sGameData.PeerageInfo.PeerageId);
		if (recordByKey != null)
		{
			m_spineShuzi.AnimationState.SetAnimation(0, recordByKey.id.ToString(), true);
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (m_currTab == 1)
		{
			m_pannelDailyTask.UpdatePerSecond();
		}
		UpdateRedPoint();
	}

	private void UpdateRedPoint()
	{
		m_peerageRedDot.visible = zbSelf.Instance.taskHandler.HasPeerageRedPoint();
		m_dailyRedDot.visible = zbSelf.Instance.taskHandler.HasDailyTaskRedPoint();
		m_passportRedDot.visible = zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Passport);
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		if (m_currTab == 0)
		{
			zbWsMsgHandler.Instance.Peerage_Data_Req();
		}
		else if (m_currTab == 1)
		{
			zbWsMsgHandler.Instance.DailyTask_Data_Req();
		}
	}

	public void RefreshDailyTask()
	{
		if (m_pannelDailyTask != null)
		{
			m_pannelDailyTask.Refresh();
		}
		UpdateRedPoint();
	}
}
