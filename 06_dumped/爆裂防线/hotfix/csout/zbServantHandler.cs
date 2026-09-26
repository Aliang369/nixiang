using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbServantHandler
{
	public int skillLevelLimit;

	public JToken servantBase;

	public zbServantCsvRecord[] servantRecords;

	public zbServantEggsCsvRecord[] eggsRecords;

	public zbServantSkillUpgradeCsvRecord[] skillUpgradeRecords;

	public int[] eggIds;

	public JArray subSkillUpgradeCost;

	public void Init()
	{
		servantBase = (JToken)(object)zbDataConfig.Instance.servantBaseJsonTable.GetRecord();
		servantRecords = zbDataConfig.Instance.servantCsvTable.GetRecords().records.Values.ToArray();
		eggsRecords = zbDataConfig.Instance.servantEggsCsvTable.GetRecords().records.Values.ToArray();
		skillUpgradeRecords = zbDataConfig.Instance.servantSkillUpgradeCsvTable.GetRecords().records.Values.ToArray();
		eggIds = eggsRecords.Select((zbServantEggsCsvRecord e) => e.id).ToArray();
		JToken obj = zbSelf.Instance.servantHandler.servantBase[(object)"subSkill"][(object)"upgradeCost"];
		subSkillUpgradeCost = (JArray)(object)((obj is JArray) ? obj : null);
	}

	public ServantDetail GetServantDetail(string servantId)
	{
		foreach (ServantDetail servant in zbServerData.sGameData.ServantInfo.ServantList)
		{
			if (servant.ServantId == servantId)
			{
				return servant;
			}
		}
		return null;
	}

	public string GetColorByQuality(EQuality quality)
	{
		return zbUtil.ConvertToHexColor(zbDataConfig.Instance.qualityCsvTable.GetRecordByKey((int)quality).color);
	}

	public zbServantCsvRecord GetServantRecord(int tid)
	{
		zbServantCsvRecord[] array = servantRecords;
		foreach (zbServantCsvRecord zbServantCsvRecord2 in array)
		{
			if (zbServantCsvRecord2.id == tid)
			{
				return zbServantCsvRecord2;
			}
		}
		return null;
	}

	public ServantDetail GetCurrentServantBySkillType(ESkillType type)
	{
		ServantDetail result = null;
		foreach (ServantDetail servant in zbServerData.sGameData.ServantInfo.ServantList)
		{
			if (GetServantRecord(servant.ServantTid).skillType == (int)type && servant.Deploy == EBoolState.True)
			{
				result = servant;
			}
		}
		return result;
	}

	public float GetAttrValByQuality(EQuality quality, zbServantAttrCsvRecord record)
	{
		float result = 0f;
		switch ((int)quality)
		{
		case 1:
			result = record.attrVal1;
			break;
		case 2:
			result = record.attrVal2;
			break;
		case 3:
			result = record.attrVal3;
			break;
		case 4:
			result = record.attrVal4;
			break;
		case 5:
			result = record.attrVal5;
			break;
		case 6:
			result = record.attrVal6;
			break;
		case 7:
			result = record.attrVal7;
			break;
		}
		return result;
	}

	public string FormatAttrVal(int attrId, EQuality quality, int attrLevel)
	{
		string result = "";
		zbServantAttrCsvRecord recordByKey = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(attrId);
		float num = GetAttrValByQuality(quality, recordByKey) * (float)attrLevel;
		if (recordByKey.attrType == 1)
		{
			if (zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey(recordByKey.attr[1]).showPercent > 0)
			{
				result = num.ToString((num % 1f == 0f) ? "0" : "0.00");
				result += "%";
			}
			else
			{
				result = num.ToString((num % 1f == 0f) ? "0" : "0.00");
			}
		}
		else if (recordByKey.attrType == 2)
		{
			if (zbDataConfig.Instance.skillAttrCsvTable.GetRecordByKey(recordByKey.attr[1]).showPercent > 0)
			{
				result = num.ToString((num % 1f == 0f) ? "0" : "0.00");
				result += "%";
			}
			else
			{
				result = num.ToString((num % 1f == 0f) ? "0" : "0.00");
			}
		}
		return result;
	}

	public bool HasServantRedPoint()
	{
		foreach (ServantDetail item in ((IEnumerable<ServantDetail>)(zbServerData.sGameData?.ServantInfo?.ServantList)).Where((ServantDetail s) => s.Deploy == EBoolState.True))
		{
			if (CanServantUpgradeAttr(item) || CanServantUpgradeMainSkill(item) || CanServantUpgradeSubSkill(item) || CanServantUpgrade(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanServantUpgradeAttr(ServantDetail detail)
	{
		if (detail == null)
		{
			return false;
		}
		return detail.SubAttrUpgradeCount - detail.SubAttrUpgradeCounted > 0;
	}

	public bool CanServantUpgradeMainSkill(ServantDetail detail)
	{
		if (detail == null)
		{
			return false;
		}
		zbServantSkillUpgradeCsvRecord recordByKey = zbDataConfig.Instance.servantSkillUpgradeCsvTable.GetRecordByKey(detail.MainSkill.Level + 1);
		if (recordByKey != null)
		{
			if (recordByKey.lvLimit > detail.Level)
			{
				return false;
			}
			if (IsMainSkillLevelMax(detail.MainSkill.Level))
			{
				return false;
			}
			int[][] costList = recordByKey.costList;
			foreach (int[] array in costList)
			{
				if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsMainSkillLevelMax(int level)
	{
		return level == skillUpgradeRecords.Length;
	}

	public bool CanServantUpgradeSubSkill(ServantDetail detail)
	{
		if (detail == null)
		{
			return false;
		}
		if (((IEnumerable<ServantSkillDetail>)detail.SubSkillList).All((ServantSkillDetail s) => s.Level == Extensions.Value<int>((IEnumerable<JToken>)servantBase[(object)"subSkill"][(object)"levelLimit"])))
		{
			return false;
		}
		foreach (JToken item in subSkillUpgradeCost)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)item[(object)0]), Extensions.Value<int>((IEnumerable<JToken>)item[(object)1]), Extensions.Value<int>((IEnumerable<JToken>)item[(object)2])))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanServantUpgrade(ServantDetail detail)
	{
		if (detail == null)
		{
			return false;
		}
		return GetUpgradeNeedHeroLv(detail) <= GetCurrentHeroLvByDetail(detail);
	}

	public int GetUpgradeNeedHeroLv(ServantDetail detail)
	{
		return zbDataConfig.Instance.servantUpgradeCsvTable.GetRecordByKey(detail.Level + 1)?.heroLevel ?? 0;
	}

	public int GetCurrentHeroLvByDetail(ServantDetail detail)
	{
		zbServantCsvRecord recordByKey = zbDataConfig.Instance.servantCsvTable.GetRecordByKey(detail.ServantTid);
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)recordByKey.skillType);
		return zbSelf.Instance.heroHandler.GetHeroLevel(warRoomDetailBySkillType.HeroTid);
	}

	public bool HasIncubateRedPoint()
	{
		if (!CanIncubateReceive())
		{
			return CanIncubate();
		}
		return true;
	}

	private bool CanIncubateReceive()
	{
		IncubateInfo incubateInfo = zbServerData.sGameData?.IncubateInfo;
		if (incubateInfo?.SlotList == null)
		{
			return false;
		}
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		foreach (IncubateSlotDetail slot in incubateInfo.SlotList)
		{
			if (slot.PropId > 0 && serverTime > slot.EndTm)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanIncubate()
	{
		int[] array = eggIds;
		foreach (int tid in array)
		{
			if (zbSelf.Instance.itemHandler.GetCountByPropTid(tid) > 0)
			{
				return true;
			}
		}
		return false;
	}
}
