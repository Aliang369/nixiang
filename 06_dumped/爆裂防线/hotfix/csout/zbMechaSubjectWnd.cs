using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSubjectWnd : zbTabWnd
{
	private SpriteRenderer sceneBg;

	private SkeletonAnimation skeletonAnimation;

	private GameObject m_gameObjectLevel;

	public SpriteRenderer[] m_spriteLevels = (SpriteRenderer[])(object)new SpriteRenderer[20];

	public SkeletonAnimation spineCurrent;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Button m_btnClose;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_notifyTabs = (VisualElement[])(object)new VisualElement[3];

	private zbMechaSubjectTabLevel m_moduleLevel;

	private zbMechaSubjectTabZhanLing m_moduleZhanLing;

	private zbMechaSubjectTabRoulette m_moduleRoulette;

	public MechaSubjectDetail m_data;

	public zbMechaSubjectCsvRecord mechaSubjectRecord;

	private bool m_isFirstIn = true;

	private int m_lastBgTab = -1;

	protected override void OnEnable()
	{
		base.OnEnable();
		sceneBg = ((Component)GetMyGameObjectSceneRoot().transform.Find("spineScene").Find("SceneBg")).GetComponent<SpriteRenderer>();
		GameObject gameObject = ((Component)GetMyGameObjectSceneRoot().transform.Find("spineScene").Find("SkeletonAnimation")).gameObject;
		skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
		m_gameObjectLevel = ((Component)GetMyGameObjectSceneRoot().transform.Find("gameObjectLevel")).gameObject;
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
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
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
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MechaSubject, OnEventUpdate);
	}

	public void RefreshCustom(MechaSubjectDetail data, int tabIndex = 0)
	{
		if (data != null)
		{
			m_data = data;
			mechaSubjectRecord = zbDataConfig.Instance.mechaSubjectCsvTable.GetRecordByKey(data.SubjectId);
			if (m_isFirstIn)
			{
				zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(mechaSubjectRecord.mechaId);
				m_isFirstIn = false;
			}
			RefreshTime();
			SelectTab(tabIndex);
		}
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
				m_moduleLevel = new zbMechaSubjectTabLevel();
				m_moduleLevel.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleLevel.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_moduleZhanLing == null)
			{
				m_moduleZhanLing = new zbMechaSubjectTabZhanLing();
				m_moduleZhanLing.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleZhanLing.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			break;
		case 2:
			if (m_moduleRoulette == null)
			{
				m_moduleRoulette = new zbMechaSubjectTabRoulette();
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
				zbSelf.Instance.mechaSubjectHandler.SetGameObjectSprite(sceneBg, "mechaSubject_levelBg.jpg");
				zbSelf.Instance.mechaSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "mechaSubjectRukoubg");
				break;
			case 1:
				zbSelf.Instance.mechaSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "mechaSubjectZhanlingbg");
				break;
			case 2:
				zbSelf.Instance.mechaSubjectHandler.SetGameObjectSprite(sceneBg, "mechaSubject_rouletteBg.jpg");
				zbSelf.Instance.mechaSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "mechaSubjectZhuanpanbg");
				break;
			}
		}
	}

	private void RefreshNotify()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (m_data != null)
		{
			m_notifyTabs[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaSubjectHandler.HasLevelNotify(m_data)));
			m_notifyTabs[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaSubjectHandler.HasZhanLingNotify(m_data)));
			m_notifyTabs[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaSubjectHandler.HasRouletteNotify(m_data)));
		}
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
			MechaSubjectDetail mechaSubjectDetail = zbSelf.Instance.mechaSubjectHandler.GetMechaSubjectDetail();
			RefreshCustom(mechaSubjectDetail, m_currTab);
		}
	}
}
