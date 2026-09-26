using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLotteryWnd : zbBaseWnd
{
	private GameObject m_spineSceneBar;

	public SkeletonAnimation chouKaDiZuoHero;

	public SkeletonAnimation chouKaDiZuoIcon;

	public SkeletonAnimation chouKaDiZuo1;

	public SkeletonAnimation chouKaDiZuo2;

	public SkeletonAnimation chouKaDiZuoUp;

	private VisualElement m_subTab2;

	private VisualElement m_subTab2Sel;

	private Button[] m_subTab2Buttons = (Button[])(object)new Button[2];

	private VisualElement[] m_subTab2Notifys = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_subTab3;

	private VisualElement m_subTab3Sel;

	private Button[] m_subTab3Buttons = (Button[])(object)new Button[3];

	private VisualElement[] m_subTab3Notifys = (VisualElement[])(object)new VisualElement[3];

	private VisualElement[] m_subTabLayers = (VisualElement[])(object)new VisualElement[3];

	private zbVipTabBarNormal m_moduleNormal;

	private zbVipTabBarSpecial m_moduleSpecial;

	private zbVipTabBarOperate m_moduleOperate;

	private Button m_btnBack;

	public int m_subCurIndex;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineSceneBar = ((Component)m_GameObjectSceneRoot.transform.Find("spineSceneBar")).gameObject;
		chouKaDiZuoHero = ((Component)m_spineSceneBar.transform.Find("chouKaHero")).GetComponent<SkeletonAnimation>();
		chouKaDiZuoIcon = ((Component)m_spineSceneBar.transform.Find("chouKaDiZuoIcon")).GetComponent<SkeletonAnimation>();
		chouKaDiZuo1 = ((Component)m_spineSceneBar.transform.Find("chouKaDiZuo1")).GetComponent<SkeletonAnimation>();
		chouKaDiZuo2 = ((Component)m_spineSceneBar.transform.Find("chouKaDiZuo2")).GetComponent<SkeletonAnimation>();
		chouKaDiZuoUp = ((Component)m_spineSceneBar.transform.Find("chouKaDiZuoUp")).GetComponent<SkeletonAnimation>();
		((Component)chouKaDiZuoUp).gameObject.SetActive(false);
		m_subTab2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "barTab2", (string)null);
		m_subTab2Sel = UQueryExtensions.Q<VisualElement>(m_subTab2, "bar_selectTab2", (string)null);
		m_subTab3 = UQueryExtensions.Q<VisualElement>(m_RootElement, "barTab3", (string)null);
		m_subTab3Sel = UQueryExtensions.Q<VisualElement>(m_subTab3, "bar_selectTab3", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_subTabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "bar" + i, (string)null);
			int curIndex = i;
			if (i < 2)
			{
				m_subTab2Buttons[i] = UQueryExtensions.Q<Button>(m_subTab2, "bar_tabBar" + i, (string)null);
				m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_subTab2Buttons[i], (Action<ClickEvent>)delegate
				{
					OnSubTabs(curIndex);
				}, "", (TrickleDown)0);
				m_subTab2Notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_subTab2Buttons[i]);
			}
			m_subTab3Buttons[i] = UQueryExtensions.Q<Button>(m_subTab3, "bar_tabBar" + i, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_subTab3Buttons[i], (Action<ClickEvent>)delegate
			{
				OnSubTabs(curIndex);
			}, "", (TrickleDown)0);
			m_subTab3Notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_subTab3Buttons[i]);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Bar, OnEventUpdateBar);
		m_gameEevent.RegisterEvent(EGameEvent.Event_LimitTimeHeroDraw_Ack, OnOperateLimitTimeHeroDrawAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdateBar);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdateBar);
	}

	private void OnEventUpdateBar(object data)
	{
		Refresh(m_subCurIndex);
	}

	public void Refresh(int tab)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		m_subTab2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.operateHandler.GetTimeHeroCount() > 0));
		m_subTab3.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.operateHandler.GetTimeHeroCount() <= 0));
		m_subTab2Notifys[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.barHandler.HasBarRedPointByBarType(EBarType.Normal)));
		m_subTab2Notifys[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.barHandler.HasBarRedPointByBarType(EBarType.Special)));
		m_subTab3Notifys[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.barHandler.HasBarRedPointByBarType(EBarType.Normal)));
		m_subTab3Notifys[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.barHandler.HasBarRedPointByBarType(EBarType.Special)));
		m_subTab3Notifys[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.operateHandler.HasLimitTimeHeroGift(isHoliday: true) && !zbSelf.Instance.operateHandler.HasLimitTimeHeroGift()) ? 1 : 0));
		if (zbSelf.Instance.operateHandler.GetTimeHeroCount() <= 0 && tab == 2)
		{
			OnSubTabs(0);
		}
		else
		{
			OnSubTabs(tab);
		}
	}

	public void OnSubTabs(int index)
	{
		SelectSubTab(index);
	}

	public void SelectSubTab(int subTabIndex)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (i < 2)
			{
				((VisualElement)m_subTab2Buttons[i]).SetEnabled(true);
				((VisualElement)m_subTab2Buttons[i]).style.opacity = StyleFloat.op_Implicit(0.7f);
			}
			((VisualElement)m_subTab3Buttons[i]).SetEnabled(true);
			((VisualElement)m_subTab3Buttons[i]).style.opacity = StyleFloat.op_Implicit(0.7f);
			m_subTabLayers[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (subTabIndex < 2)
		{
			((VisualElement)m_subTab2Buttons[subTabIndex]).SetEnabled(false);
			((VisualElement)m_subTab2Buttons[subTabIndex]).style.opacity = StyleFloat.op_Implicit(1f);
		}
		((VisualElement)m_subTab3Buttons[subTabIndex]).SetEnabled(false);
		((VisualElement)m_subTab3Buttons[subTabIndex]).style.opacity = StyleFloat.op_Implicit(1f);
		m_subTabLayers[subTabIndex].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_subCurIndex = subTabIndex;
		DoSelectSubTab();
	}

	private void DoSelectSubTab()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Invalid comparison between Unknown and I4
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		RefreshSpineScene();
		switch (m_subCurIndex)
		{
		case 0:
			if ((int)Application.platform == 8)
			{
				CreateMoneyPanel(new EMoneyType[1] { EMoneyType.BarNormal });
			}
			else
			{
				CreateMoneyPanel(new EMoneyType[2]
				{
					EMoneyType.Diamond,
					EMoneyType.BarNormal
				});
			}
			if (m_moduleNormal == null)
			{
				m_moduleNormal = new zbVipTabBarNormal();
				m_moduleNormal.OnEnable(this, m_subTabLayers[0]);
			}
			m_moduleNormal.Refresh();
			m_subTab2Sel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			m_subTab3Sel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if ((int)Application.platform == 8)
			{
				CreateMoneyPanel(new EMoneyType[1] { EMoneyType.BarSpecial });
			}
			else
			{
				CreateMoneyPanel(new EMoneyType[2]
				{
					EMoneyType.Diamond,
					EMoneyType.BarSpecial
				});
			}
			if (m_moduleSpecial == null)
			{
				m_moduleSpecial = new zbVipTabBarSpecial();
				m_moduleSpecial.OnEnable(this, m_subTabLayers[1]);
			}
			m_moduleSpecial.Refresh();
			m_subTab2Sel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			m_subTab3Sel.style.left = StyleLength.op_Implicit(new Length(240f, (LengthUnit)0));
			break;
		case 2:
			if ((int)Application.platform == 8)
			{
				CreateMoneyPanel(new EMoneyType[1] { EMoneyType.LimitTimeHero });
			}
			else
			{
				CreateMoneyPanel(new EMoneyType[2]
				{
					EMoneyType.Diamond,
					EMoneyType.LimitTimeHero
				});
			}
			if (m_moduleOperate == null)
			{
				m_moduleOperate = new zbVipTabBarOperate();
				m_moduleOperate.OnEnable(this, m_subTabLayers[2]);
			}
			m_moduleOperate.Refresh();
			m_subTab3Sel.style.left = StyleLength.op_Implicit(new Length(480f, (LengthUnit)0));
			break;
		}
	}

	private void RefreshSpineScene()
	{
		string spinePath = "animation/ui/chouka_dizuo/chouka_dizuo_icon1/chouka_dizuo_icon1_SkeletonData.asset";
		if (m_subCurIndex == 1)
		{
			spinePath = "animation/ui/chouka_dizuo/chouka_dizuo_icon2/chouka_dizuo_icon2_SkeletonData.asset";
		}
		if ((Object)(object)chouKaDiZuoIcon != (Object)null)
		{
			zbUtil.RefreshSpine(chouKaDiZuoIcon, spinePath, "animation", loop: true);
			((Component)chouKaDiZuoIcon).gameObject.SetActive(m_subCurIndex != 2);
			((Component)chouKaDiZuoHero).gameObject.SetActive(m_subCurIndex == 2);
		}
		if (m_subCurIndex == 2)
		{
			zbUtil.RefreshSpine(chouKaDiZuo1, "animation/ui/chouka_dizuo/chouka_dizuo_3/chouka_dizuo_3_SkeletonData.asset", "animation", loop: true);
			zbUtil.RefreshSpine(chouKaDiZuo2, "animation/ui/chouka_dizuo/chouka_dizuo_4/chouka_dizuo_4_SkeletonData.asset", "animation", loop: true);
		}
		else
		{
			zbUtil.RefreshSpine(chouKaDiZuo1, "animation/ui/chouka_dizuo/chouka_dizuo_1/chouka_dizuo_1_SkeletonData.asset", "animation", loop: true);
			zbUtil.RefreshSpine(chouKaDiZuo2, "animation/ui/chouka_dizuo/chouka_dizuo_2/chouka_dizuo_2_SkeletonData.asset", "animation", loop: true);
		}
	}

	protected override void UpdateAtShow()
	{
		if (m_moduleOperate != null)
		{
			m_moduleOperate.Update();
		}
	}

	public void OnClose()
	{
		Hide();
	}

	private void OnOperateLimitTimeHeroDrawAck(object data)
	{
		if (data is OperateLimitTimeHeroDrawAck ack)
		{
			ActionLimitTimeHeroDrawAnim(ack);
		}
	}

	public void ActionLimitTimeHeroDrawAnim(OperateLimitTimeHeroDrawAck ack)
	{
		zbUIMgr.Instance.ShowModel();
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		((Component)chouKaDiZuoUp).gameObject.SetActive(true);
		chouKaDiZuoUp.AnimationState.SetAnimation(0, "animation", false);
		((MonoBehaviour)this).StartCoroutine(DelayHideModel(ack));
	}

	private IEnumerator DelayHideModel(OperateLimitTimeHeroDrawAck ack)
	{
		yield return (object)new WaitForSeconds(0.7f);
		zbServerData.sGameData.OperateInfo.LimitTimeHeroList.Clear();
		zbServerData.sGameData.OperateInfo.LimitTimeHeroList.AddRange((IEnumerable<LimitTimeHeroDetail>)ack.LimitTimeHeroList);
		zbSelf.Instance.dataHandler.DataChange(ack.DataChange, ((IEnumerable<AwardDetail>)ack.AwardList).ToArray());
		yield return (object)new WaitForSeconds(0.3f);
		((Component)chouKaDiZuoUp).gameObject.SetActive(false);
		zbUIMgr.Instance.HideModel();
	}
}
