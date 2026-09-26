using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbMainTabLevel
{
	private zbMainLevelWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_bgPick;

	private List<zbMonsterPreview> m_monsterList = new List<zbMonsterPreview>();

	private Label m_lbForce;

	private VisualElement m_spriteForce;

	private zbItem[] m_itemScores = new zbItem[6];

	private Button[] m_boxButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_getted = (VisualElement[])(object)new VisualElement[3];

	private VisualElement m_spriteProgress;

	public VisualElement m_iconFightGuide;

	public Button m_btnFight;

	private VisualElement m_doubleMainQuest;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Button m_btnQuestList;

	private VisualElement m_QuestListRedDot;

	private zbMainLevelCsvRecord mainLevelRecord;

	private int m_costTili = 5;

	public void OnEnable(zbMainLevelWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_bgPick, (Action<ClickEvent>)OnMonsterPreview, "", (TrickleDown)0);
		for (int i = 1; i <= 4; i++)
		{
			zbMonsterPreview component = zbUtil.FindGameObjectRecursive(((Component)m_wndParent).transform, "monsterPreview" + i).GetComponent<zbMonsterPreview>();
			if (Object.op_Implicit((Object)(object)component))
			{
				m_monsterList.Add(component);
			}
		}
		for (int j = 0; j < 3; j++)
		{
			int currentIndex = j + 1;
			m_boxButtons[j] = UQueryExtensions.Q<Button>(m_RootElement, "btn_box" + currentIndex, (string)null);
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_boxButtons[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnBoxButtons(evt, currentIndex);
			}, "", (TrickleDown)0);
			m_getted[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "getted" + currentIndex, (string)null);
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "award" + j, (string)null);
			m_itemScores[j * 2] = UQueryExtensions.Q<zbItem>(val, "zbItem0", (string)null);
			m_itemScores[j * 2].OnEnable(m_wndParent);
			m_itemScores[j * 2 + 1] = UQueryExtensions.Q<zbItem>(val, "zbItem1", (string)null);
			m_itemScores[j * 2 + 1].OnEnable(m_wndParent);
		}
		m_lbForce = UQueryExtensions.Q<Label>(m_RootElement, "lb_force", (string)null);
		m_spriteForce = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_force", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_iconFightGuide = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_fightGuide", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "button_fight", (string)null);
		m_doubleMainQuest = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_mainQuest", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_btnQuestList = UQueryExtensions.Q<Button>(m_RootElement, "btn_questList", (string)null);
		m_QuestListRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnQuestList, "red_dot", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action<ClickEvent>)delegate
		{
			OnGotoNext(-1);
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action<ClickEvent>)delegate
		{
			OnGotoNext(1);
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnQuestList, (Action<ClickEvent>)OnQuestList, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		RefreshNotify();
		((VisualElement)m_btnLeft).visible = m_wndParent.m_curLevel > 1;
		((VisualElement)m_btnRight).visible = m_wndParent.m_curLevel < zbServerData.sGameData.MainQuestInfo.NextLevel;
		mainLevelRecord = zbDataConfig.Instance.mainLevelCsvTable.GetRecordByKey(m_wndParent.m_curLevel);
		RefreshAwardDetail(mainLevelRecord.mst1, mainLevelRecord.mst2, mainLevelRecord.mst3, mainLevelRecord.mst4, mainLevelRecord.awardShow);
		((TextElement)m_lbForce).text = zbResourceHelper.GetMainLevelName(m_wndParent.m_curLevel);
		MainQuestDetail mainQuestDetail = null;
		bool flag = m_wndParent.m_curLevel < zbServerData.sGameData.MainQuestInfo.NextLevel;
		m_spriteForce.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		int num = 0;
		int[][] awardStar = mainLevelRecord.awardStar1;
		foreach (int[] array in awardStar)
		{
			zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
			((VisualElement)m_itemScores[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_itemScores[num].Refresh(data);
			num++;
		}
		awardStar = mainLevelRecord.awardStar2;
		foreach (int[] array2 in awardStar)
		{
			zbItemData data2 = new zbItemData((EResourceType)array2[0], array2[1], array2[2]);
			((VisualElement)m_itemScores[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_itemScores[num].Refresh(data2);
			num++;
		}
		awardStar = mainLevelRecord.awardStar3;
		foreach (int[] array3 in awardStar)
		{
			zbItemData data3 = new zbItemData((EResourceType)array3[0], array3[1], array3[2]);
			((VisualElement)m_itemScores[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_itemScores[num].Refresh(data3);
			num++;
		}
		if (flag)
		{
			mainQuestDetail = zbSelf.Instance.mainQuestHandler.GetMainQuestDetail(m_wndParent.m_curLevel);
			bool flag2 = mainQuestDetail.Score == EBattleResultScore.Win3;
			UQueryExtensions.Q<VisualElement>(m_spriteForce, "icon_perfect", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			UQueryExtensions.Q<VisualElement>(m_spriteForce, "defenseHp", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
			if (!flag2)
			{
				((TextElement)UQueryExtensions.Q<Label>(m_spriteForce, "lb_successDesc", (string)null)).text = "HP:" + Mathf.Floor(mainQuestDetail.DefensePct) + "%";
			}
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(mainQuestDetail.DefensePct, (LengthUnit)1));
			int score = (int)mainQuestDetail.Score;
			int scoreAward = (int)mainQuestDetail.ScoreAward;
			for (int j = 0; j < 3; j++)
			{
				int num2 = j + 1;
				m_itemScores[j * 2].SetSpineVisible(vis: false);
				m_itemScores[j * 2 + 1].SetSpineVisible(vis: false);
				m_getted[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_boxButtons[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				if (num2 <= scoreAward)
				{
					m_getted[j].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
				else if (num2 <= score)
				{
					m_itemScores[j * 2].SetSpineVisible(vis: true);
					m_itemScores[j * 2 + 1].SetSpineVisible(vis: true);
					((VisualElement)m_boxButtons[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
			}
		}
		else
		{
			for (int k = 0; k < 3; k++)
			{
				m_itemScores[k * 2].SetSpineVisible(vis: false);
				m_itemScores[k * 2 + 1].SetSpineVisible(vis: false);
				m_getted[k].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_boxButtons[k]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(0f, (LengthUnit)1));
		}
		zbUtil.RefreshSpine(m_wndParent.m_spineDikuai, mainLevelRecord.mainScene + "_SkeletonData.asset", "in");
		m_wndParent.m_spineDikuai.AnimationState.AddAnimation(0, "loop", true, 0f);
	}

	public void RefreshNotify()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		m_doubleMainQuest.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.MainQuest));
		m_QuestListRedDot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.MainQuest)));
	}

	private void OnBoxButtons(ClickEvent evt, int index)
	{
		if (zbSelf.Instance.mainQuestHandler.CanClaimReward(m_wndParent.m_curLevel, index))
		{
			zbWsMsgHandler.Instance.MainQuest_ScoreAward_Req(m_wndParent.m_curLevel);
		}
	}

	private void RefreshAwardDetail(int mst1, int mst2, int mst3, int mst4, int[][] awardShow)
	{
		m_monsterList[0].Refresh(mst1);
		m_monsterList[1].Refresh(mst2);
		m_monsterList[2].Refresh(mst3);
		m_monsterList[3].Refresh(mst4);
	}

	public void OnFight(ClickEvent evt)
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_costTili))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, m_costTili);
		}
		else if (zbSelf.Instance.mainQuestHandler.isPerfectPass(m_wndParent.m_curLevel) && zbSelf.Instance.mainQuestHandler.shouldShowMainQuestTip)
		{
			zbUIMgr.Instance.ShowMainLevelPerfectPassTip(m_wndParent.m_curLevel, isElite: false, 0L);
		}
		else
		{
			zbWsMsgHandler.Instance.MainQuest_FightStart_Req(m_wndParent.m_curLevel);
		}
	}

	private void OnGotoNext(int index)
	{
		m_wndParent.m_curLevel += index;
		m_wndParent.m_curLevel = Math.Clamp(m_wndParent.m_curLevel, 1, zbServerData.sGameData.MainQuestInfo.NextLevel);
		Refresh();
	}

	private void OnQuestList(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowQuestList(0);
	}

	private void OnMonsterPreview(ClickEvent evt)
	{
		zbSelf.Instance.monsterHandler.OnShowMonsterPreview(evt);
	}
}
