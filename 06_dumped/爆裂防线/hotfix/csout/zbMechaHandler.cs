using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbMechaHandler
{
	public int mechaSetId;

	public int resetDiamond;

	private string[] typeStrings = new string[6] { "DianXi", "HuoXi", "BingXi", "WuLiXi", "NengLiangXi", "FengXi" };

	private zbMechaCsvRecord[] mechaCsvRecord;

	private Dictionary<int, zbMechaCsvRecord> m_dictMechaRecord;

	public void Init()
	{
		resetDiamond = (int)zbDataConfig.Instance.gameBaseJsonTable.GetRecord()["mechaBase"][(object)"resetDiamond"];
		mechaCsvRecord = (from r in zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values
			where zbVersionMgr.IsReleased(r.preShow)
			orderby r.sort
			select r).ToArray();
	}

	public zbMechaCsvRecord[] GetAllMechaRecords()
	{
		return mechaCsvRecord;
	}

	public zbMechaCsvRecord GetMechaRecord(int mechaId)
	{
		if (m_dictMechaRecord == null)
		{
			m_dictMechaRecord = new Dictionary<int, zbMechaCsvRecord>();
			foreach (zbMechaCsvRecord value2 in zbDataConfig.Instance.mechaCsvTable.GetRecords().records.Values)
			{
				m_dictMechaRecord[value2.id] = value2;
			}
		}
		m_dictMechaRecord.TryGetValue(mechaId, out var value);
		return value;
	}

	public string GetMechaTypeString(int mechaId)
	{
		zbMechaCsvRecord mechaRecord = GetMechaRecord(mechaId);
		if (mechaRecord != null)
		{
			return zbUtil.Language(typeStrings[mechaRecord.type - 1]);
		}
		return "";
	}

	public void UpdateMechaInfo(MechaInfo mechaInfo)
	{
		MechaInfo mechaInfo2 = zbServerData.sGameData.MechaInfo;
		if (mechaInfo2 != null)
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (MechaDetail mecha in mechaInfo2.MechaList)
			{
				if (mecha.SkillList == null)
				{
					continue;
				}
				foreach (MechaSkillDetail skill in mecha.SkillList)
				{
					hashSet.Add(skill.SkillId);
				}
			}
			foreach (MechaDetail mecha2 in mechaInfo.MechaList)
			{
				if (mecha2.SkillList == null)
				{
					continue;
				}
				foreach (MechaSkillDetail skill2 in mecha2.SkillList)
				{
					if (!hashSet.Contains(skill2.SkillId))
					{
						zbSelf.Instance.newTagHandler.AddNewMechaSkill(skill2.SkillId);
					}
				}
			}
		}
		zbServerData.sGameData.MechaInfo = mechaInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MechaInfo);
	}

	public MechaDetail GetMechaDetail(int mechaId)
	{
		if (zbServerData.sGameData.MechaInfo != null)
		{
			foreach (MechaDetail mecha in zbServerData.sGameData.MechaInfo.MechaList)
			{
				if (mecha.MechaId == mechaId)
				{
					return mecha;
				}
			}
		}
		return null;
	}

	public MechaSkillDetail GetMechaSkillDetailBySkillId(int skillId)
	{
		foreach (MechaDetail mecha in zbServerData.sGameData.MechaInfo.MechaList)
		{
			foreach (MechaSkillDetail skill in mecha.SkillList)
			{
				if (skill.SkillId == skillId)
				{
					return skill;
				}
			}
		}
		return null;
	}

	public MechaSkillDetail GetMechaSkillDetail(int mechaId, int index)
	{
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		if (mechaDetail != null)
		{
			foreach (MechaSkillDetail skill in mechaDetail.SkillList)
			{
				if (skill.Index == index)
				{
					return skill;
				}
			}
		}
		return null;
	}

	public string GetCurMechaSkillDesc(int skillId)
	{
		MechaSkillDetail mechaSkillDetailBySkillId = GetMechaSkillDetailBySkillId(skillId);
		if (mechaSkillDetailBySkillId != null)
		{
			return GetMechaSkillDesc(skillId, mechaSkillDetailBySkillId.Level);
		}
		return GetMechaSkillDesc(skillId);
	}

	public string GetMechaSkillDesc(int skillId, int skillLevel = 1)
	{
		zbMechaSkillCsvRecord recordByKey = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(skillId);
		if (recordByKey != null)
		{
			float damageRate = GetDamageRate(skillId, skillLevel);
			float[] param = recordByKey.param;
			int num = ((param != null) ? param.Length : 0);
			object[] array = new object[1 + num];
			array[0] = damageRate;
			for (int i = 0; i < num; i++)
			{
				array[1 + i] = param[i];
			}
			return string.Format(recordByKey.desc, array);
		}
		return "";
	}

	public float GetDamageRate(int skillId, int skillLevel = 1)
	{
		zbMechaSkillCsvRecord recordByKey = zbDataConfig.Instance.mechaSkillCsvTable.GetRecordByKey(skillId);
		if (recordByKey != null)
		{
			return recordByKey.damageRate + recordByKey.damageGrowth * (float)(skillLevel - 1);
		}
		return 0f;
	}

	public bool HasMechaActiveNotify(int mechaId)
	{
		zbMechaCsvRecord mechaRecord = GetMechaRecord(mechaId);
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		if (mechaRecord != null && mechaDetail == null && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, mechaRecord.propId, 1))
		{
			return true;
		}
		return false;
	}

	public bool HasMechaUpgradeNotify(int mechaId)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha))
		{
			return false;
		}
		if (zbSelf.Instance.newTagHandler.IsNotNotifyMecha(mechaId))
		{
			return false;
		}
		zbMechaCsvRecord record = GetMechaRecord(mechaId);
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		if (record != null && mechaDetail != null)
		{
			zbMechaUpgradeCsvRecord zbMechaUpgradeCsvRecord2 = zbDataConfig.Instance.mechaUpgradeCsvTable.GetRecords()?.records.FirstOrDefault((zbMechaUpgradeCsvRecord r) => r.quality == record.quality && r.lv == mechaDetail.Level + 1);
			if (zbMechaUpgradeCsvRecord2 != null && zbSelf.Instance.GetLevel() >= zbMechaUpgradeCsvRecord2.playerLv && zbSelf.Instance.dataHandler.GetCostListLackItem(zbMechaUpgradeCsvRecord2.costList) == null && zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)record.upgradeCost[0], record.upgradeCost[1], zbMechaUpgradeCsvRecord2.costNum))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanUpgradeMultiLv(int mechaId)
	{
		zbMechaCsvRecord record = GetMechaRecord(mechaId);
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		if (record == null || mechaDetail == null)
		{
			return false;
		}
		int curLv = mechaDetail.Level;
		int num = 0;
		zbMechaUpgradeCsvRecord[] array = zbDataConfig.Instance.mechaUpgradeCsvTable.GetRecords()?.records;
		if (array == null)
		{
			return false;
		}
		int i;
		for (i = 1; i <= 2; i++)
		{
			zbMechaUpgradeCsvRecord zbMechaUpgradeCsvRecord2 = array.FirstOrDefault((zbMechaUpgradeCsvRecord r) => r.quality == record.quality && r.lv == curLv + i);
			if (zbMechaUpgradeCsvRecord2 == null)
			{
				return false;
			}
			if (zbSelf.Instance.GetLevel() < zbMechaUpgradeCsvRecord2.playerLv)
			{
				return false;
			}
			if (zbSelf.Instance.dataHandler.GetCostListLackItem(zbMechaUpgradeCsvRecord2.costList) != null)
			{
				return false;
			}
			num += zbMechaUpgradeCsvRecord2.costNum;
		}
		return zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)record.upgradeCost[0], record.upgradeCost[1], num);
	}

	public bool HasMechaStarNotify(int mechaId)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha))
		{
			return false;
		}
		if (zbSelf.Instance.newTagHandler.IsNotNotifyMecha(mechaId))
		{
			return false;
		}
		zbMechaCsvRecord mechaRecord = GetMechaRecord(mechaId);
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		if (mechaRecord != null && mechaDetail != null)
		{
			if (mechaDetail.Star >= 5)
			{
				return false;
			}
			zbMechaStarUpgradeCsvRecord recordByKey = zbDataConfig.Instance.mechaStarUpgradeCsvTable.GetRecordByKey(mechaDetail.Star + 1);
			if (recordByKey != null && zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, mechaRecord.propId) + zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Prop, mechaRecord.convertPropId) >= recordByKey.costPropNum)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMechaSkillNotify(int mechaId)
	{
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		if (mechaDetail != null)
		{
			foreach (MechaSkillDetail skill in mechaDetail.SkillList)
			{
				if (HasSkillUpgradeNotify(mechaId, skill.Index))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasSkillUpgradeNotify(int mechaId, int index)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MechaSkill))
		{
			return false;
		}
		if (zbSelf.Instance.newTagHandler.IsNotNotifyMecha(mechaId))
		{
			return false;
		}
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		zbMechaCsvRecord mechaRecord = GetMechaRecord(mechaId);
		MechaSkillDetail mechaSkillDetail = GetMechaSkillDetail(mechaId, index);
		if (mechaDetail != null && mechaRecord != null && mechaSkillDetail != null && mechaDetail.Star >= index)
		{
			zbMechaSkillUpgradeCsvRecord zbMechaSkillUpgradeCsvRecord2 = zbDataConfig.Instance.mechaSkillUpgradeCsvTable.GetRecords()?.records.FirstOrDefault((zbMechaSkillUpgradeCsvRecord r) => r.quality == mechaRecord.quality && r.lv == mechaSkillDetail.Level + 1);
			if (zbMechaSkillUpgradeCsvRecord2 != null && mechaDetail.Level >= zbMechaSkillUpgradeCsvRecord2.mechaLv && zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)mechaRecord.skillCost[0], mechaRecord.skillCost[1], zbMechaSkillUpgradeCsvRecord2.costNum))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMechaNotify(int mechaId)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha))
		{
			return false;
		}
		if (!HasMechaActiveNotify(mechaId) && !HasMechaUpgradeNotify(mechaId) && !HasMechaStarNotify(mechaId))
		{
			return HasMechaSkillNotify(mechaId);
		}
		return true;
	}

	public void RemoveMechaTabSkillNew(int mechaId)
	{
		zbMechaCsvRecord recordByKey = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(mechaId);
		if (recordByKey == null)
		{
			return;
		}
		int[] array = recordByKey.skillList.ToArray();
		foreach (int mechaSkillId in array)
		{
			if (zbSelf.Instance.newTagHandler.IsNewMechaSkill(mechaSkillId))
			{
				zbSelf.Instance.newTagHandler.RemoveNewMechaSkill(mechaSkillId);
			}
		}
	}

	public bool HasRedPoint()
	{
		zbMechaCsvRecord[] allMechaRecords = GetAllMechaRecords();
		foreach (zbMechaCsvRecord zbMechaCsvRecord2 in allMechaRecords)
		{
			if (HasMechaNotify(zbMechaCsvRecord2.id))
			{
				return true;
			}
		}
		return false;
	}

	public List<MechaDetail> GetMechaShouldConvert()
	{
		List<MechaDetail> list = new List<MechaDetail>();
		if (zbServerData.sGameData.MechaInfo != null)
		{
			foreach (MechaDetail mecha in zbServerData.sGameData.MechaInfo.MechaList)
			{
				zbMechaCsvRecord mechaRecord = GetMechaRecord(mecha.MechaId);
				if (mechaRecord != null)
				{
					bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, mechaRecord.propId, 1);
					if ((mecha.Star == 5) & flag)
					{
						list.Add(mecha);
					}
				}
			}
		}
		return list;
	}

	public bool CanMechaReset(int mechaId)
	{
		MechaDetail mechaDetail = GetMechaDetail(mechaId);
		if (mechaDetail != null)
		{
			if (mechaDetail.Level > 1)
			{
				return true;
			}
			foreach (MechaSkillDetail skill in mechaDetail.SkillList)
			{
				if (skill.Level > 1)
				{
					return true;
				}
			}
		}
		return false;
	}
}
