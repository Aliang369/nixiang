using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbHandbookHandler
{
	private string[] typeStrings = new string[6] { "DianXi", "HuoXi", "BingXi", "WuLiXi", "NengLiangXi", "FengXi" };

	public void Init()
	{
	}

	public string GetElementTypeString(int elementType)
	{
		if (elementType <= typeStrings.Length)
		{
			return zbUtil.Language(typeStrings[elementType - 1]);
		}
		return "";
	}

	public MonsterHandbookDetail GetMonsterMonsterHandbookDetail(int handbookId)
	{
		foreach (MonsterHandbookDetail monster in zbServerData.sGameData.HandbookInfo.MonsterList)
		{
			if (monster.HandbookId == handbookId)
			{
				return monster;
			}
		}
		return null;
	}

	public bool CanMonsterActive(int monsterId)
	{
		zbHandbookCsvRecord recordByKey = zbDataConfig.Instance.handbookCsvTable.GetRecordByKey(monsterId);
		if (recordByKey != null)
		{
			MonsterHandbookDetail monsterMonsterHandbookDetail = GetMonsterMonsterHandbookDetail(recordByKey.id);
			if (monsterMonsterHandbookDetail != null && monsterMonsterHandbookDetail.Level == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanMonsterTypeActive(int monsterType)
	{
		foreach (zbHandbookCsvRecord value in zbDataConfig.Instance.handbookCsvTable.GetRecords().records.Values)
		{
			if (value.type == monsterType && value.id > 0 && CanMonsterActive(value.id))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckMonsterIsActive(int monsterId)
	{
		zbMonsterCsvRecord recordByKey = zbDataConfig.Instance.monsterCsvTable.GetRecordByKey(monsterId);
		zbHandbookCsvRecord recordByKey2 = zbDataConfig.Instance.handbookCsvTable.GetRecordByKey(recordByKey.handbookId);
		if (recordByKey2 == null)
		{
			return false;
		}
		int id = recordByKey2.id;
		foreach (MonsterHandbookDetail monster in zbServerData.sGameData.HandbookInfo.MonsterList)
		{
			if (monster.HandbookId == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!CanMonsterTypeActive(1) && !CanMonsterTypeActive(2))
		{
			return CanMonsterTypeActive(3);
		}
		return true;
	}

	public ModHandbookDetail GetModHandbookDetail(int handbookId)
	{
		foreach (ModHandbookDetail mod in zbServerData.sGameData.HandbookInfo.ModHandbookInfo.ModList)
		{
			if (mod.HandbookId == handbookId)
			{
				return mod;
			}
		}
		return null;
	}

	public bool CanModActive(int modId)
	{
		ModHandbookDetail modHandbookDetail = GetModHandbookDetail(modId);
		if (modHandbookDetail != null && modHandbookDetail.State == EActiveState.Active)
		{
			return true;
		}
		return false;
	}

	public bool HasModRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HandbookMod))
		{
			return false;
		}
		for (int i = 1; i <= 14; i++)
		{
			WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)i);
			if (warRoomDetailBySkillType == null || warRoomDetailBySkillType.HeroTid <= 0)
			{
				continue;
			}
			if (CanGetHandbookTypeTargetAward(i))
			{
				return true;
			}
			foreach (zbModHandbookCsvRecord value in zbDataConfig.Instance.modHandbookCsvTable.GetRecords().records.Values)
			{
				if (value.type == i && CanModActive(value.id))
				{
					return true;
				}
			}
		}
		List<zbModHandbookTargetCsvRecord> list = zbDataConfig.Instance.modHandbookTargetCsvTable.GetRecords().records.Values.ToList();
		ModHandbookTarget targetInfo = zbServerData.sGameData.HandbookInfo.ModHandbookInfo.TargetInfo;
		for (int j = 0; j < list.Count; j++)
		{
			if (targetInfo.TotalCount >= list[j].target && targetInfo.TargetAward < list[j].target)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanModDone(int modId)
	{
		ModHandbookDetail modHandbookDetail = GetModHandbookDetail(modId);
		if (modHandbookDetail != null && modHandbookDetail.State == EActiveState.Done)
		{
			return true;
		}
		return false;
	}

	public bool CheckModIsActive(int modId)
	{
		ModHandbookDetail modHandbookDetail = GetModHandbookDetail(modId);
		if (modHandbookDetail != null && modHandbookDetail.State == EActiveState.Active)
		{
			return false;
		}
		return true;
	}

	public ModHandbookTypeTarget GetModHandbookTypeTargetDetail(int skillType)
	{
		foreach (ModHandbookTypeTarget typeTarget in zbServerData.sGameData.HandbookInfo.ModHandbookInfo.TypeTargetList)
		{
			if (typeTarget.SkillType == (ESkillType)skillType)
			{
				return typeTarget;
			}
		}
		return null;
	}

	public bool CanGetHandbookTypeTargetAward(int skillType)
	{
		if (skillType != 1)
		{
			ModHandbookTypeTarget modHandbookTypeTargetDetail = GetModHandbookTypeTargetDetail(skillType);
			zbModHandbookTypeCsvRecord recordByKey = zbDataConfig.Instance.modHandbookTypeCsvTable.GetRecordByKey(skillType);
			if (modHandbookTypeTargetDetail != null && modHandbookTypeTargetDetail.Count >= recordByKey.unlock && modHandbookTypeTargetDetail.State == EActiveState.Active)
			{
				return true;
			}
		}
		return false;
	}
}
