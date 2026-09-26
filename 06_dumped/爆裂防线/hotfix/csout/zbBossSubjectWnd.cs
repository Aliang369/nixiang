using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBossSubjectWnd : zbTabWnd
{
	private VisualElement m_nodeTab;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	public Button[] m_tabButtons = (Button[])(object)new Button[3];

	private Button m_btnBack;

	private Button m_btnTask;

	private zbBossSubjectMainWnd m_moduleBossSubjectMain;

	private zbBossSubjectRouletteWnd m_moduleBossSubjectRoulette;

	private zbBossSubjectZhanlingWnd m_moduleBossSubjectZhanling;

	public BossSubjectDetail bossSubjectDetail;

	public zbBossSubjectCsvRecord bossSubjectRecord;

	private VisualElement m_zhanlingNotify;

	private VisualElement m_rouletteNotify;

	private VisualElement m_taskNotify;

	private Transform mainObject;

	private Transform rouletteObject;

	private Transform zhanlingObject;

	private bool fromOut;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_nodeTab = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + i, (string)null);
		}
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_task", (string)null);
		m_taskNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTask);
		m_zhanlingNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[2]);
		m_rouletteNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[1]);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTask, (Action)OnTask, "", (TrickleDown)0);
		mainObject = GetMyGameObjectSceneRoot().transform.Find("mainObject");
		rouletteObject = GetMyGameObjectSceneRoot().transform.Find("rouletteObject");
		zhanlingObject = GetMyGameObjectSceneRoot().transform.Find("zhanlingObject");
		InitTab(m_tabButtons, m_tabLayers, null);
	}

	public override void Refresh()
	{
	}

	public void RefreshCustom(BossSubjectDetail _bossSubjectDetail, bool _fromOut = true, int tabIndex = 0)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		fromOut = _fromOut;
		bossSubjectDetail = _bossSubjectDetail;
		bossSubjectRecord = zbDataConfig.Instance.bossSubjectCsvTable.GetRecordByKey(bossSubjectDetail.BossSubject.SubjectId);
		if (fromOut)
		{
			m_currTab = tabIndex;
		}
		SelectTab(m_currTab);
		m_taskNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.bossSubjectHandler.HasTaskNotify(bossSubjectDetail)));
	}

	public override void DoSelectTab()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			if (m_moduleBossSubjectMain == null)
			{
				m_moduleBossSubjectMain = new zbBossSubjectMainWnd();
				m_moduleBossSubjectMain.OnEnable(this, m_tabLayers[m_currTab]);
			}
			if (bossSubjectDetail != null)
			{
				m_moduleBossSubjectMain.RefreshCustom(bossSubjectDetail, fromOut);
			}
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.Tili
			});
			zbUtil.SetActive(((Component)mainObject).gameObject, active: true);
			zbUtil.SetActive(((Component)rouletteObject).gameObject, active: false);
			zbUtil.SetActive(((Component)zhanlingObject).gameObject, active: false);
			break;
		case 1:
			if (m_moduleBossSubjectRoulette == null)
			{
				m_moduleBossSubjectRoulette = new zbBossSubjectRouletteWnd();
				m_moduleBossSubjectRoulette.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleBossSubjectRoulette.RefreshCustom(bossSubjectDetail, bossSubjectRecord);
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.CrystalBall
			});
			zbUtil.SetActive(((Component)mainObject).gameObject, active: false);
			zbUtil.SetActive(((Component)rouletteObject).gameObject, active: true);
			zbUtil.SetActive(((Component)zhanlingObject).gameObject, active: false);
			break;
		case 2:
			if (m_moduleBossSubjectZhanling == null)
			{
				m_moduleBossSubjectZhanling = new zbBossSubjectZhanlingWnd();
				m_moduleBossSubjectZhanling.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleBossSubjectZhanling.RefreshCustom(bossSubjectDetail, bossSubjectRecord);
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.Diamond,
				EMoneyType.CrystalBall
			});
			zbUtil.SetActive(((Component)mainObject).gameObject, active: false);
			zbUtil.SetActive(((Component)rouletteObject).gameObject, active: false);
			zbUtil.SetActive(((Component)zhanlingObject).gameObject, active: true);
			break;
		}
		RefreshNotify();
	}

	private void RefreshNotify(object obj = null)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (bossSubjectDetail != null)
		{
			m_zhanlingNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.bossSubjectHandler.HasZhanLingNotify(bossSubjectDetail)));
			m_rouletteNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.bossSubjectHandler.HasRouletteNotify(bossSubjectDetail)));
			m_taskNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.bossSubjectHandler.HasTaskNotify(bossSubjectDetail)));
		}
	}

	private void OnClose()
	{
		if (m_currTab != 1 || m_moduleBossSubjectRoulette == null || !m_moduleBossSubjectRoulette.isRoration)
		{
			Hide();
		}
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowBossSubjectTask(bossSubjectDetail);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		switch (m_currTab)
		{
		case 0:
			if (m_moduleBossSubjectMain != null)
			{
				m_moduleBossSubjectMain.UpdateAtShow();
			}
			break;
		case 1:
			if (m_moduleBossSubjectRoulette != null)
			{
				m_moduleBossSubjectRoulette.UpdateAtShow();
			}
			break;
		case 2:
			if (m_moduleBossSubjectZhanling != null)
			{
				m_moduleBossSubjectZhanling.UpdateAtShow();
			}
			break;
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		switch (m_currTab)
		{
		case 0:
			if (m_moduleBossSubjectMain != null)
			{
				m_moduleBossSubjectMain.UpdatePerSecond();
			}
			break;
		case 1:
			if (m_moduleBossSubjectRoulette != null)
			{
				m_moduleBossSubjectRoulette.UpdatePerSecond();
			}
			break;
		case 2:
			if (m_moduleBossSubjectZhanling != null)
			{
				m_moduleBossSubjectZhanling.UpdatePerSecond();
			}
			break;
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail != null)
		{
			RefreshCustom(bossSubjectDetail, _fromOut: false);
		}
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail != null)
		{
			RefreshCustom(bossSubjectDetail, _fromOut: false);
		}
	}
}
