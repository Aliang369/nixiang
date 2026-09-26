using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarEnemyWnd : zbBaseWnd
{
	private VisualElement m_bgPick;

	private zbMonsterFollowFoothold m_monster;

	private zbSkillSelected skillSelected;

	private VisualElement m_nodeFoothold;

	private Label m_lbDesc;

	private Label m_lbCount;

	private Button m_btnAdd;

	private Button m_btnFight;

	private Button m_btnHead;

	private Button m_btnTouchTest;

	private MineWarPointDataAck mineWarPointDataAck;

	private CrossMinePointDataAck crossMinePointDataAck;

	private MineWarPointDetail mineWarPointDetail;

	private RoleBaseInfo roleBaseInfo;

	private int serverId;

	private int leftTime = 5;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_monster = zbUtil.FindGameObjectRecursive(((Component)this).transform, "monsterFollowFoothold").GetComponent<zbMonsterFollowFoothold>();
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		skillSelected = new zbSkillSelected();
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "skillSelected", (string)null);
		skillSelected.OnEnable(this, root);
		m_nodeFoothold = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_foothold", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_RootElement, "btn_head", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgPick, (Action<ClickEvent>)OnMonsterPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnTouchTest, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MineWarBaseInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_CommFight_Ack, OnCommFightAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MineWarBaseInfo, RefreshArenaInfo);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_PvpTipsInfo, delegate
		{
			RefreshPvpTipsInfo();
		});
	}

	public void RefreshCustom(MineWarPointDataAck _mineWarPointDataAck, CrossMinePointDataAck _crossMinePointDataAck)
	{
		leftTime = 5;
		mineWarPointDataAck = _mineWarPointDataAck;
		crossMinePointDataAck = _crossMinePointDataAck;
		serverId = 0;
		if (mineWarPointDataAck != null)
		{
			mineWarPointDetail = mineWarPointDataAck.PointInfo;
			roleBaseInfo = mineWarPointDataAck.RoleBaseInfo;
		}
		else
		{
			serverId = crossMinePointDataAck.ServerId;
			mineWarPointDetail = crossMinePointDataAck.PointInfo;
			roleBaseInfo = crossMinePointDataAck.RoleBaseInfo;
		}
		skillSelected.Refresh(roleBaseInfo);
		RefreshArenaInfo();
		RefreshMonsterInfo();
		UpdatePerSecond();
		zbWsMsgHandler.Instance.Role_PvpTipsInfo_Req((int)roleBaseInfo.RoleId);
	}

	private void RefreshArenaInfo(object data = null)
	{
		int num = 1;
		num = ((serverId <= 0) ? 1 : zbSelf.Instance.mineWarHandler.GetCostNum(serverId));
		((TextElement)m_lbCount).enableRichText = true;
		((TextElement)m_lbCount).text = zbUtil.GetNeedHaveRichString(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum, num);
	}

	private void RefreshMonsterInfo()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (roleBaseInfo != null)
		{
			zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(roleBaseInfo.FootholdId);
			if (recordByKey != null)
			{
				string footholdEffectDesc = zbSelf.Instance.guardHandler.GetFootholdEffectDesc(recordByKey);
				((TextElement)m_lbDesc).text = recordByKey.name + footholdEffectDesc;
				m_nodeFoothold.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(footholdEffectDesc != "")));
				zbUtil.AutoFontSize((TextElement)(object)m_lbDesc, 22f, 14f, 660f, 140f);
			}
			else
			{
				m_nodeFoothold.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			zbMineNodeCsvRecord recordByKey2 = zbDataConfig.Instance.mineNodeCsvTable.GetRecordByKey(mineWarPointDetail.PointId);
			if (recordByKey != null)
			{
				List<int> monsterIdList = new List<int> { recordByKey2.mst1, recordByKey2.mst2, recordByKey2.mst3, recordByKey2.mst4 };
				m_monster.Refresh(roleBaseInfo.FootholdId, monsterIdList);
			}
		}
	}

	private void OnMonsterPreview(ClickEvent evt)
	{
		zbSelf.Instance.monsterHandler.OnShowMonsterPreview(evt);
	}

	private bool GetFightState()
	{
		if (zbSelf.Instance.timeHandler.GetServerTime() < zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.NextCombatTm)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10983") + ":" + zbUtil.FormatTimeMMSS(zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.NextCombatTm - zbSelf.Instance.timeHandler.GetServerTime()));
			return true;
		}
		int layerByPoint = zbSelf.Instance.mineWarHandler.GetLayerByPoint(mineWarPointDetail.PointId);
		if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
		{
			if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.CrossOpenTm <= zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart < zbSelf.Instance.timeHandler.GetServerTime() && zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.32023"));
				return true;
			}
			if (mineWarPointDetail.PlayerId == 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.32027"));
				return true;
			}
			if (mineWarPointDetail.PlayerId != zbSelf.Instance.GetRoleId())
			{
				if (layerByPoint < zbSelf.Instance.mineWarHandler.selfLayerNum - 1)
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10969"));
					return true;
				}
				if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10963"));
					return true;
				}
				if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10966"));
					return true;
				}
				if (mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10964"));
					return true;
				}
			}
		}
		else
		{
			if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossStart >= zbSelf.Instance.timeHandler.GetServerTime() || zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarOpenInfo.NextCrossEnd <= zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10990"));
				return true;
			}
			if (zbSelf.Instance.mineWarHandler.selfLayerNum > 4)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.32025"));
				return true;
			}
			if (layerByPoint != zbSelf.Instance.mineWarHandler.canCuihuiLayer)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10968"));
				return true;
			}
			if (mineWarPointDetail.DestroyTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10963"));
				return true;
			}
			if (mineWarPointDetail.ProtectTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10966"));
				return true;
			}
			if (mineWarPointDetail.LockedTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10964"));
				return true;
			}
		}
		return false;
	}

	private void OnAdd()
	{
		zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.MineWar);
	}

	private void OnFight()
	{
		if (GetFightState())
		{
			Hide();
			return;
		}
		int costNum = zbSelf.Instance.mineWarHandler.GetCostNum(zbSelf.Instance.mineWarHandler.currServerId);
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.CombatNum >= costNum)
		{
			if (serverId > 0)
			{
				zbWsMsgHandler.Instance.CrossMine_FightStart_Req(mineWarPointDetail.PointId, mineWarPointDetail.PlayerId, serverId);
			}
			else
			{
				zbWsMsgHandler.Instance.MineWar_FightStart_Req(mineWarPointDetail.PointId, mineWarPointDetail.PlayerId);
			}
			return;
		}
		JObject record = zbDataConfig.Instance.mineBaseJsonTable.GetRecord();
		if (zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.BuyCombat < Extensions.Value<int>((IEnumerable<JToken>)record["buyAttackLimit"]))
		{
			OnAdd();
		}
	}

	private void OnHead()
	{
		if (!zbUtil.IsNpc(roleBaseInfo.RoleId))
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)roleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10882"));
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		((TextElement)m_btnFight).text = zbUtil.Language("lang.11473") + " (" + leftTime + ")";
		if (leftTime == 0)
		{
			OnFight();
		}
		leftTime--;
	}

	private void OnTouchTest()
	{
		if (zbSelf.Instance.mineWarHandler.currServerId == zbSelf.Instance.GetServerId())
		{
			zbWsMsgHandler.Instance.MineWar_PointData_Req(mineWarPointDetail.PointId);
		}
		else
		{
			zbWsMsgHandler.Instance.CrossMine_PointData_Req(mineWarPointDetail.PointId, zbSelf.Instance.mineWarHandler.currServerId);
		}
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(mineWarPointDataAck, crossMinePointDataAck);
	}

	private void OnCommFightAck(object obj)
	{
		Hide();
	}

	private void RefreshPvpTipsInfo()
	{
		skillSelected.refreshSkill();
	}
}
