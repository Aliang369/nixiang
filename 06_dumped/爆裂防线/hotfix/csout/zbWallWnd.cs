using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWallWnd : zbTabWnd
{
	private zbSpineScene m_spineScene;

	private Label m_lbName;

	private VisualElement m_nodeStar;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnClose;

	private Button m_btnPreview;

	private Button m_btnCar;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	public Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_notifyTabs = (VisualElement[])(object)new VisualElement[3];

	private zbWallTabUpgrade m_moduleUpgrade;

	public zbWallTabSet m_moduleSet;

	public zbWallTabSkill m_moduleSkill;

	private VisualElement m_newTabSkill;

	private int m_curWallId;

	private List<int> openedSuperWallIds = new List<int>();

	private zbWallAvatar wallAvatar;

	protected GameObject animationCarBg;

	protected SkeletonAnimation animationCar;

	protected override void OnEnable()
	{
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		wallAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("wallAvatar")).GetComponent<zbWallAvatar>();
		animationCarBg = ((Component)m_GameObjectSceneRoot.transform.Find("car_bg")).gameObject;
		animationCar = ((Component)m_GameObjectSceneRoot.transform.Find("car_bg").Find("zhuangjiache")).GetComponent<SkeletonAnimation>();
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_star", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_nodeStar, "star" + i, (string)null);
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnPreview = UQueryExtensions.Q<Button>(m_RootElement, "btn_preview", (string)null);
		m_btnCar = UQueryExtensions.Q<Button>(m_RootElement, "btn_car", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPreview, (Action)OnPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCar, (Action)OnPointerUp, "", (TrickleDown)0);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int j = 0; j < 3; j++)
		{
			m_tabLayers[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + j, (string)null);
			m_tabButtons[j] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + j, (string)null);
			m_notifyTabs[j] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[j]);
		}
		m_newTabSkill = zbUtil.CreateNewTag((VisualElement)(object)m_tabButtons[2], (Vector2?)new Vector2(-10f, -20f), 0.9f);
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WallInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
		zbGameSave.Instance.SetBool("NewFuncWallOpened" + zbSelf.Instance.GetRoleId(), value: true);
	}

	public override void Refresh()
	{
		openedSuperWallIds.Clear();
	}

	public void RefreshCustom(int tab = 0)
	{
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_Wall_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.WallDefense))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Wall_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_Wall_2);
		}
		CheckShouldConvert();
		RefreshNotify();
		SelectTab(tab);
	}

	private void CheckShouldConvert()
	{
		List<WallDetail> wallShouldConvert = zbSelf.Instance.wallHandler.GetWallShouldConvert();
		if (wallShouldConvert.Count > 0)
		{
			zbUIMgr.Instance.ShowWallConvert(wallShouldConvert.ToArray());
		}
	}

	public void RefreshDetail(int wallId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbName).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_currTab == 2));
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_currTab == 2));
		zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallId);
		if (recordByKey != null)
		{
			m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 2 || recordByKey.open == 1) ? 1 : 0));
			if (m_curWallId != wallId)
			{
				((TextElement)m_lbName).text = recordByKey.name;
				wallAvatar.RefreshCustom(wallId);
				if (recordByKey.backGround != "")
				{
					m_spineScene.SetSceneBg(recordByKey.backGround);
					m_spineScene.SetSkeletonDataAsset(recordByKey.backEffect + "_SkeletonData.asset", defaultZ: false);
					m_spineScene.SetAnimation("animation", isLoop: true);
				}
				else
				{
					m_spineScene.SetSceneBg("ui/image/wall/wall_bg.jpg");
					m_spineScene.skeletonObject.SetActive(false);
				}
				if (recordByKey.quality >= 6)
				{
					wallAvatar.SetSuperWallInAnimation(!openedSuperWallIds.Contains(wallId));
					openedSuperWallIds.Add(wallId);
				}
				m_curWallId = wallId;
				if ((Object)(object)animationCar != (Object)null)
				{
					zbUtil.RefreshSpine(animationCar, recordByKey.carAnimation + "_SkeletonData.asset", "idle", loop: true);
					((Component)animationCar).transform.localPosition = new Vector3(0f, recordByKey.carOffset / 2f, -2f);
				}
			}
		}
		WallDetail wallDetail = zbSelf.Instance.wallHandler.GetWallDetail(wallId);
		zbUIEffect.SetStars(m_stars, wallDetail?.Star ?? 0);
	}

	private void RefreshNotify()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		m_notifyTabs[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 0 || !zbSelf.Instance.wallHandler.HasUpgradeNotify()) ? 1 : 0));
		m_notifyTabs[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_currTab == 1 || !zbSelf.Instance.wallHandler.HasTabSetNotify()) ? 1 : 0));
		m_newTabSkill.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasNewPoint()));
	}

	private void OnPreview()
	{
		if (m_curWallId != 1)
		{
			zbUIMgr.Instance.ShowPlayerSuitUpgradePreview(m_curWallId, EPreviewType.Wall);
		}
	}

	public override void DoSelectTab()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			if (m_moduleUpgrade == null)
			{
				m_moduleUpgrade = new zbWallTabUpgrade();
				m_moduleUpgrade.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleUpgrade.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			animationCarBg.SetActive(true);
			break;
		case 1:
			if (m_moduleSet == null)
			{
				m_moduleSet = new zbWallTabSet();
				m_moduleSet.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleSet.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			animationCarBg.SetActive(true);
			break;
		case 2:
			if (m_moduleSkill == null)
			{
				m_moduleSkill = new zbWallTabSkill();
				m_moduleSkill.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleSkill.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			animationCarBg.SetActive(false);
			break;
		}
		RefreshNotify();
	}

	public void OnClose()
	{
		Hide();
	}

	private void OnPointerUp()
	{
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11455"));
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_currTab);
	}
}
