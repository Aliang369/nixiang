using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonHandler
{
	private zbSeasonAreaCsvRecord[] m_seasonAreaRecords;

	public SeasonEnterAck seasonEnterAck;

	public SeasonDataAck seasonDataAck;

	public SeasonCityWarInfoAck seasonCityWarInfoAck;

	public SeasonAirdropListAck seasonAirdropListAck;

	public SeasonAirdropProg seasonAirdropProg;

	public SeasonMyTerritoryAck seasonMyTerritoryAck;

	public SeasonAreaInfo[] areaList;

	public RepeatedField<SeasonMemberInfo> memberList;

	public SeasonDeclareInfo[] seasonDeclareList;

	public SeasonTeamPosInfo[] teamPosList;

	public void Init()
	{
		m_seasonAreaRecords = zbDataConfig.Instance.seasonAreaCsvTable.GetRecords().records.Values.ToArray();
	}

	public zbSeasonAreaCsvRecord[] GetSeasonMapRecords(int mapId)
	{
		List<zbSeasonAreaCsvRecord> list = new List<zbSeasonAreaCsvRecord>();
		zbSeasonAreaCsvRecord[] seasonAreaRecords = m_seasonAreaRecords;
		foreach (zbSeasonAreaCsvRecord zbSeasonAreaCsvRecord2 in seasonAreaRecords)
		{
			if (zbSeasonAreaCsvRecord2.mapId == mapId)
			{
				list.Add(zbSeasonAreaCsvRecord2);
			}
		}
		return list.ToArray();
	}

	public zbSeasonMap GetSeasonMap()
	{
		zbSeasonWnd zbSeasonWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Season) as zbSeasonWnd;
		if ((Object)(object)zbSeasonWnd2 != (Object)null)
		{
			return zbSeasonWnd2.GetSeasonMap();
		}
		return null;
	}

	public zbSeasonAreaCsvRecord GetSeasonMapRecord(int cellId, int mapId)
	{
		zbSeasonAreaCsvRecord[] seasonAreaRecords = m_seasonAreaRecords;
		foreach (zbSeasonAreaCsvRecord zbSeasonAreaCsvRecord2 in seasonAreaRecords)
		{
			if (zbSeasonAreaCsvRecord2.mapId == mapId && zbSeasonAreaCsvRecord2.id == cellId)
			{
				return zbSeasonAreaCsvRecord2;
			}
		}
		return null;
	}

	public ESeasonAreaType GetAreaType(int areaType)
	{
		return areaType switch
		{
			0 => ESeasonAreaType.Birth, 
			1 => ESeasonAreaType.Neutral, 
			2 => ESeasonAreaType.Center, 
			_ => ESeasonAreaType.Neutral, 
		};
	}

	public void UpdateSeasonMemberInfo(long targetRoleId, ESeasonOffice office)
	{
		foreach (SeasonMemberInfo member in memberList)
		{
			if (member.RoleBaseInfo.RoleId == targetRoleId)
			{
				member.Office = office;
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SeasonMember);
				break;
			}
		}
	}

	public SeasonAreaInfo GetAreaInfoById(int id)
	{
		if (areaList == null || areaList.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < areaList.Length; i++)
		{
			if (id == areaList[i].AreaId)
			{
				return areaList[i];
			}
		}
		return null;
	}

	public ESeasonForce GetMySeasonForce()
	{
		if (seasonEnterAck == null)
		{
			return ESeasonForce.ForceNeutral;
		}
		return seasonEnterAck.ForceId;
	}

	public ESeasonForce GetForceByAreaId(int areaId)
	{
		for (int i = 0; i < areaList.Length; i++)
		{
			if (areaList[i].AreaId == areaId)
			{
				return areaList[i].ForceId;
			}
		}
		return ESeasonForce.ForceNeutral;
	}

	public StyleBackground GetResourceIconByType(ESeasonResType eSeasonResType)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		zbAssetLoader instance = zbAssetLoader.Instance;
		int num = (int)eSeasonResType;
		Sprite val = instance.LoadResouce<Sprite>("ui/image/season/resource_" + num + ".png");
		if ((Object)(object)val == (Object)null)
		{
			val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/season/resource_0.png");
		}
		return new StyleBackground(val);
	}

	public StyleBackground GetForceSmallFlagByForceId(ESeasonForce force)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(zbSeasonMapDefine.GetForceSmallFlagUrl(force)));
	}

	public StyleBackground GetForceBigFlagByForceId(ESeasonForce force)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(zbSeasonMapDefine.GetForceBigFlagUrl(force)));
	}

	public StyleBackground GetForceBtnFlagByForceId(ESeasonForce force)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(zbSeasonMapDefine.GetForceBtnFlagUrl(force)));
	}

	public int GetAirdropLeftCount(ESeasonAirdropQuality quality)
	{
		if (seasonAirdropListAck != null)
		{
			foreach (SeasonAirdropInfo item in seasonAirdropListAck.List)
			{
				if (item.Quality == quality)
				{
					return item.DailyLimit - item.DailyWin;
				}
			}
		}
		return 0;
	}

	public SeasonAirdropInfo GetSeasonAirdropInfo(int dropId)
	{
		if (seasonAirdropListAck == null)
		{
			foreach (SeasonAirdropInfo item in seasonAirdropListAck.List)
			{
				if (item.DropId == dropId)
				{
					return item;
				}
			}
		}
		return null;
	}

	public void UpdateSeasonAirdropInfo(SeasonAirdropInfo info)
	{
		if (seasonAirdropListAck == null)
		{
			seasonAirdropListAck = new SeasonAirdropListAck();
		}
		for (int i = 0; i < seasonAirdropListAck.List.Count; i++)
		{
			if (seasonAirdropListAck.List[i].DropId == info.DropId)
			{
				seasonAirdropListAck.List[i] = info;
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SeasonAirdrop);
				break;
			}
		}
	}

	public void UpdateSeasonAirdropProgress(int dropId, int curProg, int personalProg)
	{
		if (seasonAirdropListAck == null)
		{
			seasonAirdropListAck = new SeasonAirdropListAck();
		}
		foreach (SeasonAirdropInfo item in seasonAirdropListAck.List)
		{
			if (item.DropId == dropId)
			{
				item.CurProg = curProg;
				break;
			}
		}
		seasonAirdropProg.PersonalProg = personalProg;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SeasonAirdrop);
	}

	public void AddLine(SeasonMoveDetail seasonMoveDetial)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float num = zbSelf.Instance.timeHandler.GetServerTime();
		float num2 = seasonMoveDetial.ArrivalTm - seasonMoveDetial.StartTm;
		float num3 = num - (float)seasonMoveDetial.StartTm;
		num3 = Mathf.Clamp(num3, 0f, num2);
		Vector3 start = default(Vector3);
		((Vector3)(ref start))._002Ector(seasonMoveDetial.FromX, seasonMoveDetial.FromY, 0f);
		Vector3 end = default(Vector3);
		((Vector3)(ref end))._002Ector(seasonMoveDetial.ToX, seasonMoveDetial.ToY, 0f);
		zbSeasonLineMgr.Instance.AddLine(start, end, zbSeasonMapDefine.GetForceColor(seasonMoveDetial.ForceId), 0.2f, num2, num3);
	}

	public zbSeasonBuild GetCityByAreaId(int areaId)
	{
		return zbSeasonMgr.Instance.SeasonMap.m_seasonAreas.FirstOrDefault((zbSeasonArea cell) => cell.areaId == areaId).cityBuild;
	}

	public SeasonTeamPosInfo GetSeasonTeamInfoByIndex(int index)
	{
		if (teamPosList == null)
		{
			return null;
		}
		return teamPosList[index];
	}

	public Vector2 TransformLogicPosToClientRenderWorldPos(float logicX, float logicY)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		float num = (logicX - (float)zbSeasonMapDefine.gridCount / 2f + 0.5f) * zbSeasonMapDefine.cellSize;
		float num2 = ((float)zbSeasonMapDefine.gridCount / 2f - logicY - 0.5f) * zbSeasonMapDefine.cellSize;
		return new Vector2(num, num2);
	}

	public Vector2 TransformWorldPosToLogicPos(float worldX, float worldY)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = worldX / zbSeasonMapDefine.cellSize + (float)zbSeasonMapDefine.gridCount / 2f;
		float num2 = (float)zbSeasonMapDefine.gridCount / 2f - worldY / zbSeasonMapDefine.cellSize;
		float num3 = (float)Math.Round(num, 4);
		num2 = (float)Math.Round(num2, 4);
		return new Vector2(num3, num2);
	}

	public Vector2 TransformLogicPosToWorldPos(float logicX, float logicY)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float num = (logicX - (float)zbSeasonMapDefine.gridCount / 2f) * zbSeasonMapDefine.cellSize;
		float num2 = ((float)zbSeasonMapDefine.gridCount / 2f - logicY) * zbSeasonMapDefine.cellSize;
		return new Vector2(num, num2);
	}
}
