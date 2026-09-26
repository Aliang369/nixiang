using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRankHandler
{
	public zbRankAwardCsvRecord[] rankAwardRecord = new zbRankAwardCsvRecord[0];

	private Sprite m_normalRankBg;

	private zbRankAwardCsvRecord[] m_mainRankRecord;

	private int m_mainTopReach;

	public RankDetail m_opsRankTopDetail;

	public RankDetail m_rankTopDetail;

	public RankListAck m_oldMainRankListAck;

	public RankListAck m_newMainRankListAck;

	public RankListAck m_mainQuestRankListAck;

	public RankListAck m_endlessHordeRankListAck;

	public ArenaRankListAck arenaRankListAck;

	public void Init()
	{
		rankAwardRecord = zbDataConfig.Instance.rankAwardCsvTable.GetRecords().records;
		m_normalRankBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/rank/rank_icon.png");
		m_mainRankRecord = zbDataConfig.Instance.rankAwardCsvTable.GetRecords().records.Where((zbRankAwardCsvRecord r) => r.type == 2).ToArray();
		m_oldMainRankListAck = null;
		m_newMainRankListAck = null;
	}

	public void UpdateMainTopReach(RankTopDetail topDetail)
	{
		if (topDetail == null)
		{
			return;
		}
		m_rankTopDetail = topDetail.RankInfo;
		RankDetail rankInfo = topDetail.RankInfo;
		if (rankInfo == null)
		{
			return;
		}
		RankData rankData = rankInfo.RankData;
		if (rankData != null && m_mainRankRecord != null && m_mainRankRecord.Length != 0)
		{
			long value = rankData.Value;
			zbRankAwardCsvRecord zbRankAwardCsvRecord2 = (from r in m_mainRankRecord
				where r.value <= value
				orderby r.value descending
				select r).FirstOrDefault();
			if (zbRankAwardCsvRecord2 != null)
			{
				m_mainTopReach = zbRankAwardCsvRecord2.id;
			}
			else
			{
				m_mainTopReach = 0;
			}
		}
	}

	public zbRankAwardCsvRecord[] GetRankAwardRecordByType(ERankType rankType)
	{
		return rankAwardRecord.Where((zbRankAwardCsvRecord r) => rankType == (ERankType)r.type).ToArray();
	}

	public Sprite GetRankSpriteByRank(int rank)
	{
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/rank/icon_rank{rank}.png");
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		return zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/rank/icon_rank1.png");
	}

	public void InitPlayerInfo(VisualElement element, RoleBaseInfo roleBaseInfo)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_name", (string)null)).text = roleBaseInfo.RoleName;
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_power", (string)null)).text = roleBaseInfo.TeamAttack.ToString();
		zbPlayerHeadCsvRecord recordByKey = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(roleBaseInfo.HeadId);
		if (recordByKey != null)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
			UQueryExtensions.Q<VisualElement>(element, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		}
		zbPlayerHeadFrameCsvRecord recordByKey2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(roleBaseInfo.HeadFrameId);
		if (recordByKey2 != null)
		{
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon);
			UQueryExtensions.Q<VisualElement>(element, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		}
		((TextElement)UQueryExtensions.Q<Label>(element, "lb_level", (string)null)).text = roleBaseInfo.Level.ToString();
	}

	public void InitRank(Label lbRank, int value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)lbRank).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
		((TextElement)lbRank).text = null;
		if (value <= 3)
		{
			((VisualElement)lbRank).style.backgroundImage = new StyleBackground(GetRankSpriteByRank(value));
		}
		else
		{
			((TextElement)lbRank).text = value.ToString();
		}
	}

	public void InitRankInOpenServerRank(Label lbRank, int value)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)lbRank).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
		((TextElement)lbRank).text = null;
		if (value <= 3)
		{
			((VisualElement)lbRank).style.backgroundImage = new StyleBackground(GetRankSpriteByRank(value));
			return;
		}
		((VisualElement)lbRank).style.backgroundImage = new StyleBackground(m_normalRankBg);
		((TextElement)lbRank).text = value.ToString();
	}

	public bool IsOpenServerOpen()
	{
		if (zbServerData.sGameData.OpenServerInfo != null && zbServerData.sGameData.OpenServerInfo.RankEndTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			return true;
		}
		return false;
	}

	public void UpdateOpenServerRankTop(OpsRankTopDetail[] list)
	{
		foreach (OpsRankTopDetail opsRankTopDetail in list)
		{
			if (opsRankTopDetail.RankType == ERankType.MainQuest)
			{
				m_opsRankTopDetail = opsRankTopDetail.RankInfo;
				break;
			}
		}
	}

	public void UpdateMainQuestRankListAck(RankListAck ack, bool isForce = false)
	{
		m_newMainRankListAck = ack;
		if (m_oldMainRankListAck == null || isForce)
		{
			m_oldMainRankListAck = ack;
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MainQuestRank);
	}

	public bool isMainQuestRankUp()
	{
		if (m_oldMainRankListAck == null || m_newMainRankListAck == null)
		{
			return false;
		}
		((IEnumerable<RankDetail>)m_oldMainRankListAck.RankList).ToList();
		((IEnumerable<RankDetail>)m_newMainRankListAck.RankList).ToList();
		int num = ((IEnumerable<RankDetail>)m_oldMainRankListAck.RankList).FirstOrDefault((RankDetail v) => v.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId())?.RankData.Rank ?? 0;
		int num2 = ((IEnumerable<RankDetail>)m_newMainRankListAck.RankList).FirstOrDefault((RankDetail v) => v.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId())?.RankData.Rank ?? 0;
		if (num > 0 && num2 > 0 && num2 < num)
		{
			return true;
		}
		return false;
	}

	public RankDetail GetLastMainQuestRankDetail()
	{
		if (zbServerData.sGameData.MainQuestInfo.MainLevel >= 3)
		{
			RankListAck newMainRankListAck = m_newMainRankListAck;
			if (newMainRankListAck != null && newMainRankListAck.SelfRank?.Rank > 0)
			{
				return (from v in (IEnumerable<RankDetail>)m_newMainRankListAck?.RankList
					where v.RankData.Value == zbServerData.sGameData.MainQuestInfo.MainLevel && v.RoleBaseInfo.RoleId != zbSelf.Instance.GetRoleId()
					orderby v.RankData.Rank
					select v).FirstOrDefault();
			}
		}
		return null;
	}

	public bool HasRankFirstReachRedPoint()
	{
		if (m_mainTopReach <= 0)
		{
			return false;
		}
		RepeatedField<RankFirstReachAwardDetail> rankList = zbServerData.sGameData.AuxAwardInfo.RankList;
		if (rankList == null)
		{
			return false;
		}
		RankFirstReachAwardDetail rankFirstReachAwardDetail = null;
		for (int i = 0; i < rankList.Count; i++)
		{
			if (rankList[i].RankType == ERankType.MainQuest)
			{
				rankFirstReachAwardDetail = rankList[i];
				break;
			}
		}
		if (rankFirstReachAwardDetail == null)
		{
			return true;
		}
		return rankFirstReachAwardDetail.RecordId < m_mainTopReach;
	}

	public bool HasRedPoint()
	{
		return HasRankFirstReachRedPoint();
	}

	public bool HasOpenServerRankRedDot()
	{
		foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
		{
			if (freeGift.FreeType == 3 && freeGift.State == EBoolState.False && m_opsRankTopDetail != null)
			{
				return true;
			}
		}
		return false;
	}
}
