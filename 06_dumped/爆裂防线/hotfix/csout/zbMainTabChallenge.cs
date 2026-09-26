using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbMainTabChallenge
{
	private zbMainLevelWnd m_wndParent;

	private VisualElement m_RootElement;

	public Button m_btnWorld;

	public Button m_btnElite;

	public Button m_btnLegendary;

	public Button m_btnFacility;

	private SpineVisualElement m_spineWorld;

	private SpineVisualElement m_spineElite;

	private SpineVisualElement m_spineLegendary;

	private SpineVisualElement m_spineFacility;

	public VisualElement m_doubleElite;

	public VisualElement m_doubleLegendary;

	public VisualElement m_doubleFacility;

	private zbItem[] m_items = new zbItem[6];

	private Label m_lbName;

	private Label m_lbProgress;

	public Button m_btnGoto;

	private Label m_lbGoto;

	private Label m_lbCombatCount;

	private VisualElement m_notifyWorld;

	private VisualElement m_notifyElite;

	private VisualElement m_notifyLegendary;

	private VisualElement m_notifyFacility;

	private EFuncType m_selFuncType = EFuncType.Explore;

	private bool m_isFirstIn = true;

	public void OnEnable(zbMainLevelWnd wndParent, VisualElement root)
	{
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnWorld = UQueryExtensions.Q<Button>(m_RootElement, "btn_world", (string)null);
		m_btnElite = UQueryExtensions.Q<Button>(m_RootElement, "btn_elite", (string)null);
		m_btnLegendary = UQueryExtensions.Q<Button>(m_RootElement, "btn_legendary", (string)null);
		m_btnFacility = UQueryExtensions.Q<Button>(m_RootElement, "btn_facility", (string)null);
		m_spineWorld = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_world", (string)null);
		m_spineElite = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_elite", (string)null);
		m_spineLegendary = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_legendary", (string)null);
		m_spineFacility = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine_facility", (string)null);
		m_doubleElite = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_elite", (string)null);
		m_doubleLegendary = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_legendary", (string)null);
		m_doubleFacility = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_facility", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWorld, (Action)OnWorld, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnElite, (Action)OnElite, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegendary, (Action)OnLegendary, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFacility, (Action)OnFacility, "", (TrickleDown)0);
		for (int i = 0; i < 6; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_lbGoto = UQueryExtensions.Q<Label>(m_RootElement, "lb_goto", (string)null);
		m_lbCombatCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_combatCount", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		m_notifyWorld = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnWorld, (Vector2?)new Vector2(25f, 35f), 1f, true);
		m_notifyElite = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnElite, (Vector2?)new Vector2(25f, 35f), 1f, true);
		m_notifyLegendary = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLegendary, (Vector2?)new Vector2(25f, 35f), 1f, true);
		m_notifyFacility = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFacility, (Vector2?)new Vector2(25f, 35f), 1f, true);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateEventQuestRefresh);
		((VisualElement)m_btnWorld).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Explore) ? 1 : 0));
		((VisualElement)m_btnElite).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EliteQuest) ? 1 : 0));
		((VisualElement)m_btnLegendary).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Legendary) ? 1 : 0));
	}

	private void OnUpdateEventQuestRefresh(object data)
	{
		Refresh();
	}

	public void Refresh()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		SkeletonExtensions.SetColor(m_spineWorld.skeleton, zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Explore) ? Color.gray : Color.white);
		SkeletonExtensions.SetColor(m_spineElite.skeleton, zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EliteQuest) ? Color.gray : Color.white);
		SkeletonExtensions.SetColor(m_spineLegendary.skeleton, zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Legendary) ? Color.gray : Color.white);
		SkeletonExtensions.SetColor(m_spineFacility.skeleton, zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Facility) ? Color.gray : Color.white);
		((VisualElement)m_btnFacility).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.IsEnd()));
		RefreshBtnsShow();
		RefreshBtnGoTo();
		if (m_isFirstIn)
		{
			m_isFirstIn = false;
			m_wndParent.m_spineChallenge.AnimationState.ClearTrack(0);
			m_wndParent.m_spineChallenge.AnimationState.SetAnimation(0, "in", false);
			m_wndParent.m_spineChallenge.AnimationState.AddAnimation(0, "loop", true, 0f);
			RefreshDetail();
			if (!zbGuide.Instance.IsGuiding())
			{
				((MonoBehaviour)m_wndParent).StartCoroutine(PlayFirstIn());
			}
			else
			{
				PlayFirstIn_2();
			}
		}
		RefreshNotify();
	}

	private IEnumerator PlayFirstIn()
	{
		((VisualElement)m_btnWorld).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnElite).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnLegendary).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnFacility).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		yield return (object)new WaitForSecondsRealtime(13f / 30f);
		PlayFirstIn_2();
	}

	private void RefreshBtnGoTo()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_selFuncType == EFuncType.EliteQuest;
		((VisualElement)m_lbGoto).style.bottom = StyleLength.op_Implicit(flag ? 12f : 24f);
		((VisualElement)m_lbCombatCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			EliteQuestInfo eliteQuestInfo = zbServerData.sGameData.EliteQuestInfo;
			int num = eliteQuestInfo.DailyCount - eliteQuestInfo.CombatCount;
			int buyCombat = eliteQuestInfo.BuyCombat;
			int num2 = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.EliteQuestBuy, zbSelf.Instance.GetVipLevel());
			if (num > 0)
			{
				((TextElement)m_lbCombatCount).text = "剩余次数: " + num;
			}
			else if (num2 - buyCombat > 0)
			{
				((TextElement)m_lbCombatCount).text = "可购买次数: " + (num2 - buyCombat);
			}
			else
			{
				((TextElement)m_lbCombatCount).text = "剩余次数: " + num;
			}
		}
	}

	private void RefreshBtnsShow()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnWorld).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Explore) ? 1 : 0));
		((VisualElement)m_btnElite).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EliteQuest) ? 1 : 0));
		((VisualElement)m_btnLegendary).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Legendary) ? 1 : 0));
		((VisualElement)m_btnFacility).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.IsEnd()));
	}

	private void PlayFirstIn_2()
	{
		RefreshBtnsShow();
		PlayInLoopSpine(m_spineWorld, m_selFuncType != EFuncType.Explore);
		PlayInLoopSpine(m_spineElite, m_selFuncType != EFuncType.EliteQuest);
		PlayInLoopSpine(m_spineLegendary, m_selFuncType != EFuncType.Legendary);
		PlayInLoopSpine(m_spineFacility, m_selFuncType != EFuncType.Facility);
	}

	private void PlayInLoopSpine(SpineVisualElement spine, bool action)
	{
		spine.skeleton.SetToSetupPose();
		spine.AnimationState.ClearTracks();
		if (action)
		{
			spine.AnimationState.SetAnimation(0, "in", false);
			spine.AnimationState.AddAnimation(0, "loop", true, 0f);
		}
		else
		{
			spine.AnimationState.SetAnimation(0, "act", false);
			spine.AnimationState.AddAnimation(0, "act_loop", true, 0f);
		}
	}

	private void RefreshSelect(EFuncType funcType)
	{
		if (m_selFuncType != funcType)
		{
			m_selFuncType = funcType;
			RefreshSelectSpine(m_spineWorld, m_selFuncType == EFuncType.Explore);
			RefreshSelectSpine(m_spineElite, m_selFuncType == EFuncType.EliteQuest);
			RefreshSelectSpine(m_spineLegendary, m_selFuncType == EFuncType.Legendary);
			RefreshSelectSpine(m_spineFacility, m_selFuncType == EFuncType.Facility);
			RefreshDetail();
			RefreshBtnGoTo();
		}
	}

	private void RefreshSelectSpine(SpineVisualElement spine, bool isSelected)
	{
		spine.skeleton.SetToSetupPose();
		spine.AnimationState.ClearTracks();
		if (isSelected)
		{
			spine.AnimationState.SetAnimation(0, "act", false);
			spine.AnimationState.AddAnimation(0, "act_loop", true, 0f);
		}
		else
		{
			spine.AnimationState.SetAnimation(0, "loop", true);
		}
	}

	private void RefreshDetail()
	{
		((TextElement)m_lbName).text = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)m_selFuncType).name;
		List<int[]> list = new List<int[]>();
		switch (m_selFuncType)
		{
		case EFuncType.Explore:
			((TextElement)m_lbProgress).text = zbUtil.Language("lang.10867") + ": " + zbSelf.Instance.worldHandler.GetExploredCount();
			foreach (zbWorldBuildingCsvRecord value in zbDataConfig.Instance.worldBuildingCsvTable.GetRecords().records.Values)
			{
				if (value.resOut.Length != 0)
				{
					int[] item = new int[3]
					{
						value.resOut[0],
						value.resOut[1],
						1
					};
					list.Add(item);
				}
			}
			list = (from s in list.Select((int[] arr) => string.Join(",", arr)).Distinct()
				select s.Split(',').Select(int.Parse).ToArray()).ToList();
			RefreshAwardList(list.ToArray());
			break;
		case EFuncType.EliteQuest:
		{
			((TextElement)m_lbProgress).text = zbResourceHelper.GetEliteLevelName(zbServerData.sGameData.EliteQuestInfo.NextLevel);
			zbEliteLevelCsvRecord recordByKey2 = zbDataConfig.Instance.eliteLevelCsvTable.GetRecordByKey(zbServerData.sGameData.EliteQuestInfo.NextLevel);
			RefreshAwardList(recordByKey2.awardShow, zbSelf.Instance.vipHandler.GetDoubleAwardFunType() == EFuncType.EliteQuest);
			break;
		}
		case EFuncType.Legendary:
		{
			((TextElement)m_lbProgress).text = string.Format(zbUtil.Language("lang.10842"), zbServerData.sGameData.LegendaryInfo.NextLevel);
			zbLegendaryLevelCsvRecord recordByKey = zbDataConfig.Instance.legendaryLevelCsvTable.GetRecordByKey(zbServerData.sGameData.LegendaryInfo.NextLevel);
			RefreshAwardList(recordByKey.awardShow, zbSelf.Instance.vipHandler.GetDoubleAwardFunType() == EFuncType.Legendary);
			break;
		}
		case EFuncType.Facility:
		{
			((TextElement)m_lbProgress).text = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey(62).name;
			int[][] mainTabChallengeAwardList = zbSelf.Instance.facilityHandler.GetMainTabChallengeAwardList();
			RefreshAwardList(mainTabChallengeAwardList, zbSelf.Instance.vipHandler.GetDoubleAwardFunType() == EFuncType.Facility, showCount: false);
			break;
		}
		}
	}

	private void RefreshAwardList(int[][] awardList, bool isDouble = false, bool showCount = true)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			if (i < awardList.Length && awardList[i] != null)
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				int num = ((!isDouble || (awardList[i][0] == 1 && awardList[i][1] == 5)) ? 1 : 2);
				m_items[i].Refresh(new zbItemData((EResourceType)awardList[i][0], awardList[i][1], awardList[i][2] * num), isNameVis: false, showCount);
			}
		}
	}

	public void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		m_notifyWorld.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.worldHandler.HasRedPoint()));
		m_notifyElite.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.eliteQuestHandler.HasRedPoint()));
		m_notifyLegendary.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasRedPoint()));
		m_notifyFacility.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.facilityHandler.HasRedPoint()));
		m_doubleElite.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.EliteQuest));
		m_doubleLegendary.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Legendary));
		m_doubleFacility.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.Facility));
	}

	public void OnWorld()
	{
		RefreshSelect(EFuncType.Explore);
	}

	public void OnElite()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.EliteQuest))
		{
			RefreshSelect(EFuncType.EliteQuest);
		}
	}

	public void OnLegendary()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Legendary))
		{
			RefreshSelect(EFuncType.Legendary);
		}
	}

	public void OnFacility()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Facility))
		{
			RefreshSelect(EFuncType.Facility);
		}
	}

	private void OnGoto()
	{
		switch (m_selFuncType)
		{
		case EFuncType.Explore:
			zbLinkHelper.OpenWorld();
			break;
		case EFuncType.EliteQuest:
			zbUIMgr.Instance.ShowEliteQuest(zbServerData.sGameData.EliteQuestInfo.NextLevel);
			break;
		case EFuncType.Legendary:
			zbLinkHelper.OpenLegendary();
			break;
		case EFuncType.Facility:
			zbLinkHelper.OpenFacility();
			break;
		}
	}

	public void RefreshWhenAutoVisible()
	{
		if (m_selFuncType == EFuncType.Facility)
		{
			RefreshDetail();
		}
	}
}
