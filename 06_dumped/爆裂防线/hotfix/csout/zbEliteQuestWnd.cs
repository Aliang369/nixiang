using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbEliteQuestWnd : zbBaseWnd
{
	private zbPlayerAvatar m_spineAvatar0;

	private zbPlayerAvatar m_spineAvatar1;

	private zbSpineScene m_spineScene;

	private VisualElement m_nodeTop;

	private Label m_lbForce;

	private VisualElement m_spriteForce;

	private zbItem[] m_itemScores = new zbItem[6];

	private Button[] m_boxButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_getted = (VisualElement[])(object)new VisualElement[3];

	private VisualElement m_spriteProgress;

	private Button m_btnQuestList;

	private VisualElement m_QuestListRedDot;

	public VisualElement ve_joinplayGuide;

	private Label m_lbTili;

	private Button m_btnFight;

	private Button m_btnFightx5;

	private Button m_btnClose;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Button m_btnGift;

	private Button m_btnWeekCard;

	private VisualElement ve_joinplay;

	public Button m_btnJoinPlayer;

	private Button m_btnJoinClose;

	private Label m_lbSelfName;

	private Label m_lbPlayerName;

	private Button m_btnWhy;

	private VisualElement m_doubleAward;

	private zbItem[] m_items = new zbItem[5];

	private Label m_lbTip;

	private Button m_btnAuto;

	private VisualElement m_spriteOk;

	private VisualElement m_bgPick;

	private List<zbMonsterPreview> m_monsterList = new List<zbMonsterPreview>();

	private VisualElement m_notifyGift;

	private VisualElement m_notifyWeekCard;

	private int m_curLevel;

	private zbEliteLevelCsvRecord eliteLevelRecord;

	private GameObject nightScene;

	private Coroutine m_autoGetLevelAwardCoroutine;

	private int m_costTili = 5;

	private GameObject join_player;

	private RoleBaseInfo m_roleBaseInfo;

	private Label m_lbCount;

	private Button m_btnAdd;

	protected override void OnEnable()
	{
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_07aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b0: Invalid comparison between Unknown and I4
		//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_spineScene = m_GameObjectSceneRoot.GetComponentInChildren<zbSpineScene>();
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		m_nodeTop = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_top", (string)null);
		m_lbForce = UQueryExtensions.Q<Label>(m_RootElement, "lb_force", (string)null);
		m_spriteForce = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_force", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_btnQuestList = UQueryExtensions.Q<Button>(m_RootElement, "btn_questList", (string)null);
		m_QuestListRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnQuestList, "red_dot", (string)null);
		m_lbTili = UQueryExtensions.Q<Label>(m_RootElement, "lb_tili", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "button_fight", (string)null);
		m_btnFightx5 = UQueryExtensions.Q<Button>(m_RootElement, "button_fightx5", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_btnGift = UQueryExtensions.Q<Button>(m_RootElement, "btn_gift", (string)null);
		m_btnWeekCard = UQueryExtensions.Q<Button>(m_RootElement, "btn_weekCard", (string)null);
		ve_joinplay = UQueryExtensions.Q<VisualElement>(m_RootElement, "ve_joinplay", (string)null);
		ve_joinplayGuide = UQueryExtensions.Q<VisualElement>(m_RootElement, "ve_joinplayGuide", (string)null);
		m_btnJoinPlayer = UQueryExtensions.Q<Button>(m_RootElement, "btn_joinplay", (string)null);
		m_btnJoinClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_joinclose", (string)null);
		m_lbSelfName = UQueryExtensions.Q<Label>(m_RootElement, "lb_selfName", (string)null);
		m_lbPlayerName = UQueryExtensions.Q<Label>(m_RootElement, "lb_playerName", (string)null);
		m_btnWhy = UQueryExtensions.Q<Button>(m_RootElement, "btn_why", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_auto", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_doubleAward = UQueryExtensions.Q<VisualElement>(m_RootElement, "double_award", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(this);
		}
		for (int j = 0; j < 3; j++)
		{
			int currentIndex = j + 1;
			m_boxButtons[j] = UQueryExtensions.Q<Button>(m_RootElement, "btn_box" + currentIndex, (string)null);
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_boxButtons[j], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnBoxButtons(evt, currentIndex);
			}, "", (TrickleDown)0);
			m_getted[j] = UQueryExtensions.Q<VisualElement>(m_RootElement, "getted" + currentIndex, (string)null);
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "award" + j, (string)null);
			m_itemScores[j * 2] = UQueryExtensions.Q<zbItem>(val, "zbItem0", (string)null);
			m_itemScores[j * 2].OnEnable(this);
			m_itemScores[j * 2 + 1] = UQueryExtensions.Q<zbItem>(val, "zbItem1", (string)null);
			m_itemScores[j * 2 + 1].OnEnable(this);
		}
		for (int num = 1; num <= 4; num++)
		{
			zbMonsterPreview component = zbUtil.FindGameObjectRecursive(((Component)this).transform, "monsterPreview" + num).GetComponent<zbMonsterPreview>();
			if (Object.op_Implicit((Object)(object)component))
			{
				m_monsterList.Add(component);
			}
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action)OnAuto, "", (TrickleDown)0);
		nightScene = zbUtil.FindGameObjectRecursive(((Component)this).transform, "nightScene");
		m_spineAvatar0 = zbUtil.FindGameObjectRecursive(((Component)this).transform, "playerAvatar0").GetComponent<zbPlayerAvatar>();
		m_spineAvatar1 = zbUtil.FindGameObjectRecursive(((Component)this).transform, "playerAvatar1").GetComponent<zbPlayerAvatar>();
		m_notifyGift = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGift, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_notifyWeekCard = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnWeekCard, (Vector2?)new Vector2(1f, 7f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action<ClickEvent>)delegate
		{
			OnFight();
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgPick, (Action<ClickEvent>)OnMonsterPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action<ClickEvent>)delegate
		{
			OnGotoNext(-1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action<ClickEvent>)delegate
		{
			OnGotoNext(1);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWhy, (Action<ClickEvent>)OnQuestView, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnJoinPlayer, (Action<ClickEvent>)OnJoinPlayer, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnJoinClose, (Action<ClickEvent>)OnJoinClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnQuestList, (Action<ClickEvent>)OnQuestList, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFightx5, (Action<ClickEvent>)delegate
		{
			OnFight(isX5: true);
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGift, (Action)OnGift, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWeekCard, (Action)OnWeekCard, "", (TrickleDown)0);
		join_player = zbUtil.FindGameObjectRecursive(((Component)this).transform, "join_player");
		m_gameEevent.RegisterEvent(EGameEvent.Event_Quest_Refresh, OnUpdateEventQuestRefresh);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ReminderList, RefreshNotify);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, delegate
		{
			RefreshNotify();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnUpdateEventQuestRefresh);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnUpdateEventQuestRefresh);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[2] { m_btnGift, m_btnWeekCard });
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
		m_curLevel = zbServerData.sGameData.EliteQuestInfo.NextLevel;
		if ((int)Application.platform == 8)
		{
			m_nodeTop.style.top = StyleLength.op_Implicit(20f);
		}
	}

	private void OnUpdateEventQuestRefresh(object data)
	{
		RefreshDetail();
	}

	public void RefreshCustom(int level)
	{
		zbSelf.Instance.eliteQuestHandler.m_firstEliteIn = true;
		m_roleBaseInfo = null;
		if (level != -1)
		{
			m_curLevel = level;
		}
		m_costTili = 20;
		((TextElement)m_lbTili).text = "x" + m_costTili;
		m_spineAvatar0.RefreshCustom(zbSelf.Instance.GetAvatarId(), zbSelf.Instance.GetGunId());
		((TextElement)m_lbSelfName).text = zbSelf.Instance.GetRoleName();
		RefreshDetail();
		RefreshNotify();
		UpdatePerSecond();
		AutoFontSize();
		RefreshAuto();
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Elite);
	}

	private void AutoFontSize()
	{
		zbUtil.AutoFontSize((TextElement)(object)m_lbTip, 18f, 12f, 600f);
	}

	private void RefreshNotify(object data = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		m_notifyGift.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.HelpAward)));
		m_notifyWeekCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.vipHandler.HasVipCardNotifyByTab(EVipCardTab.Week)));
	}

	public void AutoGetLevelAward(int level)
	{
		if (m_autoGetLevelAwardCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_autoGetLevelAwardCoroutine);
			m_autoGetLevelAwardCoroutine = null;
		}
		m_autoGetLevelAwardCoroutine = ((MonoBehaviour)this).StartCoroutine(AutoGetLevelAwardCoroutine(level));
	}

	private void RefreshAuto()
	{
		zbSelf.Instance.combatHandler.SetAutoOKDisplay(EBattleFuncType.EliteQuest, m_spriteOk);
	}

	private void OnAuto()
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.AutoFight, zbSelf.Instance.GetVipLevel()) <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.AutoFight)) + "【" + zbUtil.Language("lang.11494") + "】");
			return;
		}
		zbSelf.Instance.combatHandler.SetAutoSelectMod(EBattleFuncType.EliteQuest);
		RefreshAuto();
	}

	private IEnumerator AutoGetLevelAwardCoroutine(int level)
	{
		yield return null;
		yield return null;
		while (zbUIMgr.Instance.IsShow(EWndId.LevelUp))
		{
			yield return null;
		}
		yield return null;
		yield return null;
		while (zbUIMgr.Instance.IsShow(EWndId.Func))
		{
			yield return null;
		}
		for (int i = 0; i < 3; i++)
		{
			if (zbSelf.Instance.eliteQuestHandler.CanClaimReward(level, i))
			{
				zbUIMgr.Instance.ShowMainQuestReceiveAward(isMain: false, level);
				break;
			}
		}
		yield return null;
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (m_autoGetLevelAwardCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(m_autoGetLevelAwardCoroutine);
			m_autoGetLevelAwardCoroutine = null;
		}
	}

	public void RefreshDetail()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10868") + ": " + (zbServerData.sGameData.EliteQuestInfo.DailyCount - zbServerData.sGameData.EliteQuestInfo.CombatCount) + "/" + (zbServerData.sGameData.EliteQuestInfo.DailyCount - zbServerData.sGameData.EliteQuestInfo.BuyCombat);
		m_doubleAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.EliteQuest));
		((VisualElement)m_btnLeft).visible = m_curLevel > 1;
		((VisualElement)m_btnRight).visible = m_curLevel < zbServerData.sGameData.EliteQuestInfo.NextLevel;
		((VisualElement)m_btnFightx5).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		RefreshJoinPlayer();
		eliteLevelRecord = zbDataConfig.Instance.eliteLevelCsvTable.GetRecordByKey(m_curLevel);
		RefreshAwardDetail(eliteLevelRecord.mst1, eliteLevelRecord.mst2, eliteLevelRecord.mst3, eliteLevelRecord.mst4, eliteLevelRecord.awardShow);
		((TextElement)m_lbForce).text = zbResourceHelper.GetEliteLevelName(m_curLevel);
		m_spineScene.Load(eliteLevelRecord.sceneId);
		m_spineScene.RefreshSceneBgGray(isGray: true);
		MainQuestDetail mainQuestDetail = null;
		bool flag = m_curLevel < zbServerData.sGameData.EliteQuestInfo.NextLevel;
		m_spriteForce.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag2 = eliteLevelRecord.descVal >= 0 && !flag;
		((VisualElement)m_lbTip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		if (flag2)
		{
			if (eliteLevelRecord.descVal > 0)
			{
				((TextElement)m_lbTip).text = string.Format(zbUtil.Language(eliteLevelRecord.desc), eliteLevelRecord.descVal);
			}
			else if (eliteLevelRecord.descVal == 0)
			{
				((TextElement)m_lbTip).text = zbUtil.Language(eliteLevelRecord.desc);
			}
		}
		int num = 0;
		int[][] awardStar = eliteLevelRecord.awardStar1;
		foreach (int[] array in awardStar)
		{
			zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
			((VisualElement)m_itemScores[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_itemScores[num].Refresh(data);
			num++;
		}
		awardStar = eliteLevelRecord.awardStar2;
		foreach (int[] array2 in awardStar)
		{
			zbItemData data2 = new zbItemData((EResourceType)array2[0], array2[1], array2[2]);
			((VisualElement)m_itemScores[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_itemScores[num].Refresh(data2);
			num++;
		}
		awardStar = eliteLevelRecord.awardStar3;
		foreach (int[] array3 in awardStar)
		{
			zbItemData data3 = new zbItemData((EResourceType)array3[0], array3[1], array3[2]);
			((VisualElement)m_itemScores[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_itemScores[num].Refresh(data3);
			num++;
		}
		if (flag)
		{
			mainQuestDetail = zbSelf.Instance.eliteQuestHandler.GetEliteQuestDetail(m_curLevel);
			bool flag3 = mainQuestDetail.Score == EBattleResultScore.Win3;
			UQueryExtensions.Q<VisualElement>(m_spriteForce, "icon_perfect", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			UQueryExtensions.Q<VisualElement>(m_spriteForce, "defenseHp", (string)null).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag3 ? 1 : 0));
			if (!flag3)
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
					((VisualElement)m_boxButtons[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
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
	}

	public void OnBoxButtons(ClickEvent evt, int index)
	{
		if (zbSelf.Instance.eliteQuestHandler.CanClaimReward(m_curLevel, index))
		{
			zbWsMsgHandler.Instance.EliteQuest_ScoreAward_Req(m_curLevel);
		}
	}

	public void RefreshAwardDetail(int mst1, int mst2, int mst3, int mst4, int[][] awardShow)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		m_monsterList[0].Refresh(mst1);
		m_monsterList[1].Refresh(mst2);
		m_monsterList[2].Refresh(mst3);
		((Component)m_monsterList[2]).transform.position = new Vector3(((Component)m_monsterList[2]).transform.position.x, 1.16f, ((Component)m_monsterList[2]).transform.position.z);
		m_monsterList[3].Refresh(mst4);
		((Component)m_monsterList[3]).transform.position = new Vector3(((Component)m_monsterList[3]).transform.position.x, 1.79f, ((Component)m_monsterList[3]).transform.position.z);
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (int[] array in awardShow)
		{
			if (num < m_items.Length)
			{
				int num2 = ((zbSelf.Instance.vipHandler.GetDoubleAwardFunType() != EFuncType.EliteQuest || (array[0] == 1 && array[1] == 5)) ? 1 : 2);
				zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2] * num2);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
			}
			num++;
		}
		VisualElement[] items = (VisualElement[])(object)m_items;
		zbUtil.CenterNodeItems(items);
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		m_QuestListRedDot.visible = zbSelf.Instance.eliteQuestHandler.HasRedPoint() && !zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.HelpAward);
	}

	public void OnFight(bool isX5 = false)
	{
		zbSelf.Instance.eliteQuestHandler.fightCount = ((!isX5) ? 1 : 5);
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, m_costTili * zbSelf.Instance.eliteQuestHandler.fightCount))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, m_costTili);
			return;
		}
		zbEliteLevelCsvRecord recordByKey = zbDataConfig.Instance.eliteLevelCsvTable.GetRecordByKey(m_curLevel);
		if (zbServerData.sGameData.MainQuestInfo.MainLevel < eliteLevelRecord.mainLevel)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10616"), recordByKey.mainLevel));
			return;
		}
		long num = ((m_roleBaseInfo != null) ? m_roleBaseInfo.RoleId : 0);
		zbSelf.Instance.eliteQuestHandler.playerId = num;
		if (zbSelf.Instance.mainQuestHandler.isPerfectPass(m_curLevel, isElite: true) && zbSelf.Instance.mainQuestHandler.shouldShowMainQuestTip)
		{
			zbUIMgr.Instance.ShowMainLevelPerfectPassTip(m_curLevel, isElite: true, num);
		}
		else if (num == 0L)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10662"), OnFightEliteQuest, zbUtil.Language("lang.10206"), zbUtil.Language("lang.10287"));
		}
		else
		{
			zbWsMsgHandler.Instance.EliteQuest_FightStart_Req(m_curLevel, num, zbSelf.Instance.eliteQuestHandler.fightCount);
		}
	}

	private void OnFightEliteQuest(bool isFight)
	{
		if (isFight && m_roleBaseInfo == null)
		{
			zbWsMsgHandler.Instance.EliteQuest_FightStart_Req(m_curLevel, 0L, zbSelf.Instance.eliteQuestHandler.fightCount);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
		if ((Object)(object)m_spineScene != (Object)null)
		{
			m_spineScene.ClearSpine();
		}
	}

	public void OnGotoNext(int index)
	{
		m_curLevel += index;
		m_curLevel = Math.Clamp(m_curLevel, 1, zbServerData.sGameData.EliteQuestInfo.NextLevel);
		RefreshDetail();
	}

	private void OnGift()
	{
		zbWsMsgHandler.Instance.EliteQuest_HelpRecordList_Req();
	}

	private void OnWeekCard()
	{
		zbUIMgr.Instance.ShowWeekCardWnd();
	}

	public void OnQuestView(ClickEvent evt)
	{
		int[][] record = eliteLevelRecord.baseAward.Concat(eliteLevelRecord.growthAward).Concat(eliteLevelRecord.spAward1).Concat(eliteLevelRecord.spAward2)
			.Concat(eliteLevelRecord.spAward3)
			.Concat(eliteLevelRecord.spAward4)
			.Concat(eliteLevelRecord.winAward)
			.ToArray();
		int[] source = new int[3] { eliteLevelRecord.mst1, eliteLevelRecord.mst2, eliteLevelRecord.mst3 };
		zbUIMgr.Instance.ShowQuestView(record, source.Where((int id) => id != 0).Distinct().ToArray(), m_curLevel, isMain: false);
	}

	public void OnMonsterPreview(ClickEvent evt)
	{
		zbSelf.Instance.monsterHandler.OnShowMonsterPreview(evt);
	}

	public void OnJoinPlayer(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowMainQuestJoin();
	}

	public void OnJoinClose(ClickEvent evt)
	{
		m_roleBaseInfo = null;
		RefreshJoinPlayer();
	}

	public void SetJoinPlayer(RoleBaseInfo info)
	{
		m_roleBaseInfo = info;
		RefreshJoinPlayer();
	}

	private void OnQuestList(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowQuestList(1);
	}

	public void RefreshJoinPlayer()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (m_roleBaseInfo != null)
		{
			ve_joinplay.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnJoinClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((Component)m_spineAvatar1).gameObject.SetActive(true);
			m_spineAvatar1.RefreshCustom(m_roleBaseInfo.AvatarId, m_roleBaseInfo.GunId);
			((TextElement)m_lbPlayerName).text = m_roleBaseInfo.RoleName;
		}
		else
		{
			ve_joinplay.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnJoinClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((Component)m_spineAvatar1).gameObject.SetActive(false);
			((TextElement)m_lbPlayerName).text = "";
		}
	}

	private void OnAdd()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.EliteQuest);
	}
}
