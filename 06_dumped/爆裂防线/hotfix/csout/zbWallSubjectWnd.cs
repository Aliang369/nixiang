using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallSubjectWnd : zbTabWnd
{
	private SkeletonAnimation skeletonAnimation;

	private GameObject m_gameObjectLevel;

	private zbWallAvatar wallAvatar;

	public SpriteRenderer[] m_spriteLevels = (SpriteRenderer[])(object)new SpriteRenderer[20];

	public SkeletonAnimation spineCurrent;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Button m_btnHandbook;

	private VisualElement m_notifyHandbook;

	private Button m_btnRule;

	private Button m_btnClose;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_notifyTabs = (VisualElement[])(object)new VisualElement[3];

	private zbWallSubjectTabLevel m_moduleLevel;

	private zbWallSubjectTabZhanLing m_moduleZhanLing;

	private zbWallSubjectTabRoulette m_moduleRoulette;

	public WallSubjectDetail m_data;

	public zbWallSubjectCsvRecord wallSubjectRecord;

	private bool m_isFirstIn = true;

	private int m_lastBgTab = -1;

	protected override void OnEnable()
	{
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spineScene").Find("SkeletonAnimation")).gameObject;
		skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
		m_gameObjectLevel = ((Component)GetMyGameObjectSceneRoot().transform.Find("gameObjectLevel")).gameObject;
		wallAvatar = ((Component)m_gameObjectLevel.transform.Find("wallAvatar")).GetComponent<zbWallAvatar>();
		for (int i = 0; i < 5; i++)
		{
			SpriteRenderer component = ((Component)m_gameObjectLevel.transform.Find("chapter" + (i + 1))).GetComponent<SpriteRenderer>();
			for (int j = 0; j < 4; j++)
			{
				m_spriteLevels[i * 4 + j] = ((Component)((Component)component).transform.Find("level" + j)).GetComponent<SpriteRenderer>();
			}
		}
		spineCurrent = ((Component)m_gameObjectLevel.transform.Find("spineCurrent")).GetComponent<SkeletonAnimation>();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnHandbook = UQueryExtensions.Q<Button>(m_RootElement, "btn_handbook", (string)null);
		m_notifyHandbook = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnHandbook, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHandbook, (Action)OnHandbook, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action)OnRule, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int k = 0; k < 3; k++)
		{
			m_tabLayers[k] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + k, (string)null);
			m_tabButtons[k] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + k, (string)null);
			m_notifyTabs[k] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[k]);
		}
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.CrystalBall
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WallSubject, OnEventUpdate);
	}

	public void RefreshCustom(WallSubjectDetail data, int tabIndex = 0)
	{
		if (data == null)
		{
			return;
		}
		m_data = data;
		wallSubjectRecord = zbDataConfig.Instance.wallSubjectCsvTable.GetRecordByKey(data.SubjectId);
		if (m_isFirstIn)
		{
			wallAvatar.RefreshCustom(wallSubjectRecord.wallId);
			if (zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallSubjectRecord.wallId).quality >= 6)
			{
				wallAvatar.SetSuperWallInAnimation(isFirst: true);
			}
			m_isFirstIn = false;
		}
		RefreshTime();
		SelectTab(tabIndex);
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	public override void DoSelectTab()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		m_gameObjectLevel.SetActive(m_currTab == 0);
		refreshScene();
		switch (m_currTab)
		{
		case 0:
			if (m_moduleLevel == null)
			{
				m_moduleLevel = new zbWallSubjectTabLevel();
				m_moduleLevel.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleLevel.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_moduleZhanLing == null)
			{
				m_moduleZhanLing = new zbWallSubjectTabZhanLing();
				m_moduleZhanLing.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleZhanLing.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			break;
		case 2:
			if (m_moduleRoulette == null)
			{
				m_moduleRoulette = new zbWallSubjectTabRoulette();
				m_moduleRoulette.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleRoulette.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			break;
		}
		RefreshNotify();
	}

	private void refreshScene()
	{
		if (m_lastBgTab != m_currTab)
		{
			m_lastBgTab = m_currTab;
			switch (m_currTab)
			{
			case 0:
				zbSelf.Instance.wallSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "wallSubjectRukoubg");
				break;
			case 1:
				zbSelf.Instance.wallSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "wallSubjectZhanlingbg");
				break;
			case 2:
				zbSelf.Instance.wallSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "wallSubjectZhuanpanbg");
				break;
			}
		}
	}

	private void RefreshNotify()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		m_notifyHandbook.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.handbookHandler.HasRedPoint() && !zbSelf.Instance.handbookHandler.HasModRedPoint()) ? 1 : 0));
		if (m_data != null)
		{
			m_notifyTabs[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasLevelNotify(m_data)));
			m_notifyTabs[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasZhanLingNotify(m_data)));
			m_notifyTabs[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasRouletteNotify(m_data)));
		}
	}

	private void OnHandbook()
	{
		zbLinkHelper.OpenHandbook();
	}

	private void OnRule()
	{
		string[] langKeys = new string[13]
		{
			"lang.11401", "lang.11402", "lang.11403", "", "lang.11404", "lang.11405", "lang.11406", "lang.11407", "lang.11408", "",
			"lang.11409", "lang.11410", "lang.11411"
		};
		zbUIMgr.Instance.ShowRule(langKeys);
	}

	private void OnClose()
	{
		if (m_currTab != 2 || m_moduleRoulette == null || !m_moduleRoulette.isRoration)
		{
			Hide();
		}
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		switch (m_currTab)
		{
		case 0:
			m_moduleLevel?.Update();
			break;
		case 1:
			m_moduleZhanLing?.Update();
			break;
		case 2:
			m_moduleRoulette?.Update();
			break;
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		OnEventUpdate(null);
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		OnEventUpdate(null);
	}

	private void OnEventUpdate(object data)
	{
		if (m_data != null)
		{
			WallSubjectDetail wallSubjectDetail = zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail();
			RefreshCustom(wallSubjectDetail, m_currTab);
		}
	}
}
