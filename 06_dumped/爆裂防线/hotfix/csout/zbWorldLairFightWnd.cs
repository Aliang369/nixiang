using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbWorldLairFightWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbCountValue;

	private VisualElement m_killCount;

	private VisualElement m_optimalKillCount;

	private VisualElement m_bgPick;

	private zbSpineScene m_spineScene;

	private Button m_btnLog;

	private zbItem[] m_awards = new zbItem[5];

	private zbMonsterPreview[] m_monsters = new zbMonsterPreview[15];

	private Button m_btnSweep;

	private Button m_btnFight;

	private Button m_btnBack;

	private Button m_btnAdd;

	private VisualElement m_vsSweep;

	private zbWorldLairCsvRecord m_record;

	private zbWorldLairLevelCsvRecord m_lairLevelRecord;

	private JToken m_worldBase;

	private bool m_isHelp;

	private WorldLairDetail m_worldLairInfo;

	private Button m_btnAuto;

	private VisualElement m_spriteOk;

	private VisualElement m_progress;

	private VisualElement m_caller;

	private VisualElement m_headFrame;

	private Label m_lbCallerName;

	private RoleBaseInfo m_helpRoleInfo;

	private Button m_btnHead;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		m_killCount = UQueryExtensions.Q<VisualElement>(m_RootElement, "killCount", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnLog = UQueryExtensions.Q<Button>(m_RootElement, "btn_log", (string)null);
		m_lbCountValue = UQueryExtensions.Q<Label>(m_RootElement, "lb_countVal", (string)null);
		m_optimalKillCount = UQueryExtensions.Q<VisualElement>(m_RootElement, "optimalKillCount", (string)null);
		m_spineScene = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene")).GetComponent<zbSpineScene>();
		m_vsSweep = UQueryExtensions.Q<VisualElement>(m_RootElement, "sweep", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_auto", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_ok", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_caller = UQueryExtensions.Q<VisualElement>(m_RootElement, "caller", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "headFrame", (string)null);
		m_lbCallerName = UQueryExtensions.Q<Label>(m_RootElement, "lb_callerName", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_RootElement, "btn_head", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>(m_RootElement, $"award{i}", (string)null);
			m_awards[i].OnEnable(this);
		}
		Transform val = m_GameObjectSceneRoot.transform.Find("monsterAvatar");
		for (int j = 0; j < 15; j++)
		{
			m_monsters[j] = ((Component)((Component)val).transform.Find($"monsterPreview{j}")).GetComponent<zbMonsterPreview>();
		}
		m_worldBase = (JToken)(object)zbDataConfig.Instance.worldBaseJsonTable.GetRecord();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgPick, (Action<ClickEvent>)OnMonsterPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action)OnSweep, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLog, (Action)OnToLog, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action)OnAuto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		zbLanguage.Instance.SetButtonTextVisible((Button[])(object)new Button[1] { m_btnLog });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnUpdateEventWorldBoss);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldlairBuyNum, OnUpdateEventBuyNum);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldLairSaoDang, OnUpdateEventSaodang);
	}

	public void RefreshCustom(WorldLairDetail worldLairInfo, bool isHelp = false, RoleBaseInfo helpRoleInfo = null)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		zbGameSave.Instance.SetString("WorldLairFirstDate" + zbSelf.Instance.GetRoleId(), zbSelf.Instance.timeHandler.GetServerDateStr());
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_WorldLair_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.WorldLair))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_WorldLair_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_WorldLair_2);
		}
		m_isHelp = isHelp;
		((VisualElement)m_btnAuto).style.top = StyleLength.op_Implicit(m_isHelp ? 913f : 844f);
		m_helpRoleInfo = helpRoleInfo;
		m_worldLairInfo = worldLairInfo;
		if (m_worldLairInfo == null)
		{
			return;
		}
		zbWorldLairCsvRecord recordByKey = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(m_worldLairInfo.LairId);
		m_spineScene.Load(recordByKey.sceneId);
		if (isHelp && zbSelf.Instance.newTagHandler.isNewLair(m_worldLairInfo.UUID))
		{
			zbSelf.Instance.newTagHandler.SetNewLairViewed(m_worldLairInfo.UUID);
		}
		m_record = zbDataConfig.Instance.worldLairCsvTable.GetRecordByKey(m_worldLairInfo.LairId);
		((TextElement)m_lbTitle).text = zbUtil.Language(m_record.name);
		if (m_record != null)
		{
			int[] array = GenerateRandomArray(new int[3] { m_record.mst1, m_record.mst2, m_record.mst3 });
			GenerateRandomArray(new int[3] { m_record.mst1, m_record.mst2, m_record.mst3 });
			for (int i = 0; i < 15; i++)
			{
				m_monsters[i].Refresh(array[i]);
				m_monsters[i].RandomAnimation();
			}
		}
		m_lairLevelRecord = zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey(m_worldLairInfo.Level);
		for (int j = 0; j < 5; j++)
		{
			bool flag = j < m_lairLevelRecord.awardShow.Length;
			((VisualElement)m_awards[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				int[] array2 = m_lairLevelRecord.awardShow[j];
				zbItemData data = new zbItemData((EResourceType)array2[0], array2[1], array2[2]);
				m_awards[j].Refresh(data);
			}
		}
		int num = m_worldLairInfo.PassTarget - m_worldLairInfo.KillCount;
		((TextElement)UQueryExtensions.Q<Label>(m_killCount, "lb_killCount", (string)null)).text = ((num > 0) ? num.ToString() : zbUtil.Language("lang.10544"));
		float num2 = (float)(m_worldLairInfo.PassTarget - m_worldLairInfo.KillCount) / (float)m_worldLairInfo.PassTarget * 100f;
		m_progress.style.width = new StyleLength(Length.Percent(num2));
		m_optimalKillCount.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_isHelp ? 1 : 0));
		if (!m_isHelp)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_optimalKillCount, "lb_optimeKillCountVal", (string)null)).text = m_worldLairInfo.BestKill.ToString();
		}
		RefreshFightNum();
		RefreshAuto();
		RefreshCaller();
	}

	private void RefreshCaller()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		m_caller.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!m_isHelp || m_helpRoleInfo == null) ? 1 : 0));
		if (m_isHelp && m_helpRoleInfo != null)
		{
			((TextElement)m_lbCallerName).text = string.Format(zbUtil.Language("lang.11240"), m_helpRoleInfo.RoleName);
			zbSelf.Instance.worldLairHandler.RefreshHead(m_headFrame, m_helpRoleInfo);
		}
	}

	private void RefreshFightNum()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.WorldLairInfo != null)
		{
			int num = Extensions.Value<int>((IEnumerable<JToken>)m_worldBase[(object)"lair"][(object)"freeNum"]);
			((TextElement)m_lbCountValue).text = (num + zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.BuyNum - zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.FightNum).ToString();
			if (!m_isHelp)
			{
				m_vsSweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_worldLairInfo.BestKill <= 0));
			}
			else
			{
				m_vsSweep.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private int[] GenerateRandomArray(int[] monsterIds, int repeatCount = 5)
	{
		int[] array = new int[monsterIds.Length * repeatCount];
		int num = 0;
		foreach (int num2 in monsterIds)
		{
			for (int j = 0; j < repeatCount; j++)
			{
				array[num++] = num2;
			}
		}
		for (int num3 = array.Length - 1; num3 > 0; num3--)
		{
			int num4 = Rand.Next(num3 + 1);
			int num5 = array[num3];
			array[num3] = array[num4];
			array[num4] = num5;
		}
		return array;
	}

	private void OnAdd()
	{
		if (!m_isHelp)
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.WorldLair);
		}
		else
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.WorldLairHelp);
		}
	}

	private bool CheckWorldLairAvailability()
	{
		if (m_worldLairInfo.PassState == EBoolState.True)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10673"));
			return false;
		}
		if (Extensions.Value<int>((IEnumerable<JToken>)m_worldBase[(object)"lair"][(object)"freeNum"]) + zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.BuyNum - zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.FightNum <= 0)
		{
			_ = zbDataConfig.Instance.worldBaseJsonTable.GetRecord()["lair"];
			if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldLairBuy, zbSelf.Instance.GetVipLevel()) - zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.BuyNum > 0)
			{
				zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.WorldLair);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10398"));
			}
			return false;
		}
		return true;
	}

	private void RefreshAuto()
	{
		zbSelf.Instance.combatHandler.SetAutoOKDisplay(EBattleFuncType.WorldLair, m_spriteOk);
	}

	private void OnAuto()
	{
		if (zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.AutoFight, zbSelf.Instance.GetVipLevel()) <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11492"), zbSelf.Instance.vipHandler.CheckVipFuncUnlock(EVipFuncType.AutoFight)) + "【" + zbUtil.Language("lang.11494") + "】");
			return;
		}
		zbSelf.Instance.combatHandler.SetAutoSelectMod(EBattleFuncType.WorldLair);
		RefreshAuto();
	}

	private void OnSweep()
	{
		if (CheckWorldLairAvailability())
		{
			zbWsMsgHandler.Instance.WorldLair_SaoDang_Req();
		}
	}

	public void OnFight()
	{
		if (CheckWorldLairAvailability())
		{
			if (m_isHelp)
			{
				zbWsMsgHandler.Instance.WorldLair_HelpFightStart_Req(m_worldLairInfo.UUID);
			}
			else
			{
				zbWsMsgHandler.Instance.WorldLair_FightStart_Req();
			}
			zbSelf.Instance.worldLairHandler.UpdateWorldLairLevel(m_worldLairInfo.Level);
			zbSelf.Instance.worldLairHandler.UpdateWorldLairInfo(m_worldLairInfo);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnToLog()
	{
		zbWsMsgHandler.Instance.WorldLair_FightRecord_Req(m_worldLairInfo.UUID);
	}

	private void OnHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_helpRoleInfo.RoleId);
	}

	private void OnUpdateEventSaodang(object data)
	{
		RefreshCustom(zbServerData.sGameData.WorldLairInfo.SelfLairInfo);
	}

	private void OnUpdateEventWorldBoss(object data)
	{
		RefreshFightNum();
	}

	private void OnUpdateEventBuyNum(object data)
	{
		RefreshFightNum();
	}

	public void OnMonsterPreview(ClickEvent evt)
	{
		zbSelf.Instance.monsterHandler.OnShowMonsterPreview(evt);
	}
}
