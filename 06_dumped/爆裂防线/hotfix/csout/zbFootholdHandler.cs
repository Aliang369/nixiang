using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbFootholdHandler
{
	public readonly int MaxCost = 15;

	public List<int> selectIds = new List<int>();

	public int guardIdleSettleSecond;

	public int CombatIndex;

	public int m_preGuardLevel;

	public int m_preGuardScore;

	public int maxFootholdStar = 10;

	public RolePvpTipsInfo pvpTipsInfo;

	public void Init()
	{
	}

	public void UpdateFootholdInfo(FootholdInfo footholdInfo)
	{
		zbServerData.sGameData.FootholdInfo = footholdInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_FootholdInfo);
	}

	public string GetFootholdEffectDesc(zbFootholdCsvRecord record, int toStar = 1)
	{
		if (record != null)
		{
			if (record.effectId > 0)
			{
				zbLegendaryEffectCsvRecord recordByKey = zbDataConfig.Instance.legendaryEffectCsvTable.GetRecordByKey(record.effectId);
				if (recordByKey != null)
				{
					return recordByKey.desc;
				}
			}
			else
			{
				zbFootholdEffectCsvRecord footholdEffectRecord = zbSelf.Instance.guardHandler.GetFootholdEffectRecord(record.id, toStar);
				if (footholdEffectRecord != null)
				{
					return string.Format(footholdEffectRecord.desc, footholdEffectRecord.param.Cast<object>().ToArray());
				}
			}
		}
		return "";
	}

	public FootholdDetail GetFootholdDetail(int footholdId)
	{
		foreach (FootholdDetail foothold in zbServerData.sGameData.FootholdInfo.FootholdList)
		{
			if (foothold.FootholdId == footholdId)
			{
				return foothold;
			}
		}
		return null;
	}

	public zbFootholdEffectCsvRecord GetFootholdEffectRecord(int footholdId, int toStar = 1)
	{
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(footholdId);
		if (recordByKey == null)
		{
			return null;
		}
		int num = 0;
		switch (toStar)
		{
		case 1:
			num = recordByKey.footholdEffectId1;
			break;
		case 2:
			num = recordByKey.footholdEffectId2;
			break;
		case 3:
			num = recordByKey.footholdEffectId3;
			break;
		case 4:
			num = recordByKey.footholdEffectId4;
			break;
		case 5:
			num = recordByKey.footholdEffectId5;
			break;
		case 6:
			num = recordByKey.footholdEffectId6;
			break;
		case 7:
			num = recordByKey.footholdEffectId7;
			break;
		case 8:
			num = recordByKey.footholdEffectId8;
			break;
		case 9:
			num = recordByKey.footholdEffectId9;
			break;
		case 10:
			num = recordByKey.footholdEffectId10;
			break;
		}
		if (num == 0)
		{
			num = recordByKey.footholdEffectId1;
		}
		return zbDataConfig.Instance.footholdEffectCsvTable.GetRecordByKey(num);
	}

	public zbFootholdStarUpgradeCsvRecord GetFootholdStarUpgradeRecord(int footholdId, int toStar)
	{
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(footholdId);
		if (recordByKey == null)
		{
			return null;
		}
		zbFootholdStarUpgradeCsvRecord[] records = zbDataConfig.Instance.footholdStarUpgradeCsvTable.GetRecords().records;
		foreach (zbFootholdStarUpgradeCsvRecord zbFootholdStarUpgradeCsvRecord2 in records)
		{
			if (zbFootholdStarUpgradeCsvRecord2.id == recordByKey.upgradeId && zbFootholdStarUpgradeCsvRecord2.star == toStar)
			{
				return zbFootholdStarUpgradeCsvRecord2;
			}
		}
		return null;
	}

	public float GetAllDamageUp()
	{
		float num = 0f;
		foreach (FootholdDetail foothold in zbServerData.sGameData.FootholdInfo.FootholdList)
		{
			zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord = GetFootholdStarUpgradeRecord(foothold.FootholdId, foothold.Star);
			if (footholdStarUpgradeRecord != null)
			{
				num += footholdStarUpgradeRecord.dmg;
			}
		}
		return num;
	}

	public bool HasFootholdNotify(int footholdId)
	{
		zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(footholdId);
		if (recordByKey == null || recordByKey.open == 1)
		{
			return false;
		}
		int num = 1;
		FootholdDetail footholdDetail = GetFootholdDetail(footholdId);
		if (footholdDetail != null)
		{
			num = footholdDetail.Star + 1;
		}
		if (num > maxFootholdStar)
		{
			return false;
		}
		zbFootholdStarUpgradeCsvRecord footholdStarUpgradeRecord = GetFootholdStarUpgradeRecord(footholdId, num);
		if (footholdStarUpgradeRecord == null)
		{
			return false;
		}
		long num2 = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.propId);
		if (num != 1)
		{
			num2 += zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, recordByKey.convertPropId);
		}
		return num2 >= footholdStarUpgradeRecord.costProp;
	}

	public bool HasFootholdsNotify()
	{
		foreach (zbFootholdCsvRecord value in zbDataConfig.Instance.footholdCsvTable.GetRecords().records.Values)
		{
			if (HasFootholdNotify(value.id))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasGurarIdleNotify()
	{
		zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.GuardIdle);
		return false;
	}

	public bool HasRedPoint()
	{
		return false;
	}

	public void Update()
	{
		if (zbIntervalTimer.Instance.IsOneSecondPassed())
		{
			guardIdleSettleSecond++;
		}
	}

	public List<FootholdDetail> GetFootholdsShouldConvert()
	{
		List<FootholdDetail> list = new List<FootholdDetail>();
		foreach (FootholdDetail foothold in zbServerData.sGameData.FootholdInfo.FootholdList)
		{
			zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(foothold.FootholdId);
			bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, 1);
			if ((foothold.Star == maxFootholdStar) & flag)
			{
				list.Add(foothold);
			}
		}
		return list;
	}
}
