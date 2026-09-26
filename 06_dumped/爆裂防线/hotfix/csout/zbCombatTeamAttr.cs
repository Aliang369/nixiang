using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbCombatTeamAttr
{
	private zbCombatTeam team;

	public List<stTeamAttrData> teamBaseAttrList = new List<stTeamAttrData>();

	public List<stTeamAttrData> teamPercentAttrList = new List<stTeamAttrData>();

	public List<stTeamAttrData> teamFixedAttrList = new List<stTeamAttrData>();

	public List<stAttrTeamTypeData> attrTeamTypeAttrList = new List<stAttrTeamTypeData>();

	public float attrSum;

	private uint baseSnapKey;

	private uint baseSnapObf;

	private uint percentSnapKey;

	private uint percentSnapObf;

	private uint fixedSnapKey;

	private uint fixedSnapObf;

	private uint teamTypeSnapKey;

	private uint teamTypeSnapObf;

	private bool attrSnapValid;

	public Dictionary<EAttrTeamType, EAttrDefenseType> AttrTeamTypeToDefenseType = new Dictionary<EAttrTeamType, EAttrDefenseType>
	{
		{
			EAttrTeamType.DefenseHp,
			EAttrDefenseType.DefenseHp
		},
		{
			EAttrTeamType.ShieldHp,
			EAttrDefenseType.ShieldHp
		},
		{
			EAttrTeamType.DamageDecr,
			EAttrDefenseType.DamageDecr
		},
		{
			EAttrTeamType.DamageImmunity,
			EAttrDefenseType.DamageImmunity
		},
		{
			EAttrTeamType.EnergyRegen,
			EAttrDefenseType.EnergyRegen
		}
	};

	public void Init(zbCombatTeam _team)
	{
		team = _team;
		foreach (BattleSkillDetail skill in team.battleTeamInfo.SkillList)
		{
			foreach (BattleSkillAttrDetail attr in skill.AttrList)
			{
				stTeamAttrData item = new stTeamAttrData
				{
					eSkillType = skill.SkillType,
					eAttrSkillType = attr.Type,
					eValue = attr.Value
				};
				teamBaseAttrList.Add(item);
			}
		}
		zbSelf.Instance.combatHandler.RecordTeamBaseAttr(teamBaseAttrList);
		RecordAttrSnapshots();
	}

	public void RefreshAttr(zbModCsvRecord modRecord)
	{
		attrSum = 0f;
		RefreshTeamAttr(modRecord);
		foreach (zbHero hero in team.heroList)
		{
			hero.heroAttr.RefreshHeroAttr(ref attrSum);
		}
	}

	private void RefreshTeamAttr(zbModCsvRecord modRecord)
	{
		if (modRecord.serverAttr == 1)
		{
			return;
		}
		if (modRecord.attrType == 2)
		{
			int num = 0;
			int[][] attrList = modRecord.attrList;
			foreach (int[] array in attrList)
			{
				if (array[2] == 1)
				{
					stTeamAttrData item = new stTeamAttrData
					{
						eSkillType = (ESkillType)array[0],
						eAttrSkillType = (EAttrSkillType)array[1],
						eValue = modRecord.valueList[num]
					};
					teamFixedAttrList.Add(item);
				}
				else
				{
					stTeamAttrData item2 = new stTeamAttrData
					{
						eSkillType = (ESkillType)array[0],
						eAttrSkillType = (EAttrSkillType)array[1],
						eValue = modRecord.valueList[num]
					};
					teamPercentAttrList.Add(item2);
				}
				num++;
			}
		}
		else if (modRecord.attrType == 1)
		{
			int num2 = 0;
			int[][] attrList = modRecord.attrList;
			foreach (int[] array2 in attrList)
			{
				bool flag = false;
				foreach (stAttrTeamTypeData attrTeamTypeAttr in attrTeamTypeAttrList)
				{
					if (attrTeamTypeAttr.eAttrTeamType == (EAttrTeamType)array2[1] && attrTeamTypeAttr.eAttrAddType == (EAttrAddType)array2[2])
					{
						attrTeamTypeAttr.eValue += modRecord.valueList[num2];
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					stAttrTeamTypeData stAttrTeamTypeData2 = new stAttrTeamTypeData();
					stAttrTeamTypeData2.eAttrTeamType = (EAttrTeamType)array2[1];
					stAttrTeamTypeData2.eAttrAddType = (EAttrAddType)array2[2];
					stAttrTeamTypeData2.eValue = modRecord.valueList[num2];
					attrTeamTypeAttrList.Add(stAttrTeamTypeData2);
				}
				num2++;
			}
		}
		RefreshSideAttrSnapshots();
	}

	private static uint AttrHashStep(uint h, uint v)
	{
		h ^= v;
		h *= 16777619;
		return h;
	}

	private static uint ComputeAttrListHash(List<stTeamAttrData> list, uint seed)
	{
		uint h = seed ^ 0x9E3779B9u;
		h = AttrHashStep(h, (uint)list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			h = AttrHashStep(h, (uint)list[i].eSkillType);
			h = AttrHashStep(h, (uint)list[i].eAttrSkillType);
			h = AttrHashStep(h, (uint)BitConverter.SingleToInt32Bits(list[i].eValue));
		}
		return h;
	}

	private static uint ComputeTeamTypeListHash(List<stAttrTeamTypeData> list, uint seed)
	{
		uint h = seed ^ 0x85EBCA6Bu;
		h = AttrHashStep(h, (uint)list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			h = AttrHashStep(h, (uint)list[i].eAttrTeamType);
			h = AttrHashStep(h, (uint)list[i].eAttrAddType);
			h = AttrHashStep(h, (uint)BitConverter.SingleToInt32Bits(list[i].eValue));
		}
		return h;
	}

	private void RecordAttrSnapshots()
	{
		baseSnapKey = (uint)Rand.NextInt();
		baseSnapObf = ComputeAttrListHash(teamBaseAttrList, baseSnapKey) ^ baseSnapKey;
		RefreshSideAttrSnapshots();
		attrSnapValid = true;
	}

	private void RefreshSideAttrSnapshots()
	{
		percentSnapKey = (uint)Rand.NextInt();
		percentSnapObf = ComputeAttrListHash(teamPercentAttrList, percentSnapKey) ^ percentSnapKey;
		fixedSnapKey = (uint)Rand.NextInt();
		fixedSnapObf = ComputeAttrListHash(teamFixedAttrList, fixedSnapKey) ^ fixedSnapKey;
		teamTypeSnapKey = (uint)Rand.NextInt();
		teamTypeSnapObf = ComputeTeamTypeListHash(attrTeamTypeAttrList, teamTypeSnapKey) ^ teamTypeSnapKey;
	}

	public bool CheckAttrTableSnapshot(out int tableMask, out uint snapHash, out uint liveHash)
	{
		tableMask = 0;
		snapHash = 0u;
		liveHash = 0u;
		if (!attrSnapValid)
		{
			return false;
		}
		uint num = baseSnapObf ^ baseSnapKey;
		uint num2 = ComputeAttrListHash(teamBaseAttrList, baseSnapKey);
		if (num2 != num)
		{
			tableMask |= 1;
			snapHash = num;
			liveHash = num2;
		}
		num = percentSnapObf ^ percentSnapKey;
		num2 = ComputeAttrListHash(teamPercentAttrList, percentSnapKey);
		if (num2 != num)
		{
			tableMask |= 2;
			if (snapHash == 0 && liveHash == 0)
			{
				snapHash = num;
				liveHash = num2;
			}
		}
		num = fixedSnapObf ^ fixedSnapKey;
		num2 = ComputeAttrListHash(teamFixedAttrList, fixedSnapKey);
		if (num2 != num)
		{
			tableMask |= 4;
			if (snapHash == 0 && liveHash == 0)
			{
				snapHash = num;
				liveHash = num2;
			}
		}
		num = teamTypeSnapObf ^ teamTypeSnapKey;
		num2 = ComputeTeamTypeListHash(attrTeamTypeAttrList, teamTypeSnapKey);
		if (num2 != num)
		{
			tableMask |= 8;
			if (snapHash == 0 && liveHash == 0)
			{
				snapHash = num;
				liveHash = num2;
			}
		}
		if (CheckBaseAttrAgainstProto(out var baseCount, out var protoCount))
		{
			tableMask |= 16;
			if (snapHash == 0 && liveHash == 0)
			{
				snapHash = (uint)baseCount;
				liveHash = (uint)protoCount;
			}
		}
		return tableMask != 0;
	}

	private bool CheckBaseAttrAgainstProto(out int baseCount, out int protoCount)
	{
		baseCount = teamBaseAttrList.Count;
		protoCount = 0;
		if (team == null || team.battleTeamInfo == null)
		{
			return false;
		}
		int num = 0;
		foreach (BattleSkillDetail skill in team.battleTeamInfo.SkillList)
		{
			foreach (BattleSkillAttrDetail attr in skill.AttrList)
			{
				protoCount++;
				if (num >= teamBaseAttrList.Count)
				{
					return true;
				}
				stTeamAttrData stTeamAttrData2 = teamBaseAttrList[num];
				if (stTeamAttrData2.eSkillType != skill.SkillType || stTeamAttrData2.eAttrSkillType != attr.Type || BitConverter.SingleToInt32Bits(stTeamAttrData2.eValue) != BitConverter.SingleToInt32Bits(attr.Value))
				{
					return true;
				}
				num++;
			}
		}
		return num != teamBaseAttrList.Count;
	}
}
