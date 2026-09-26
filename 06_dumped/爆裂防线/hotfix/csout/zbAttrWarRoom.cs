using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbAttrWarRoom
{
	private Dictionary<(ESkillType, EAttrSkillType), bool> m_attrShow = new Dictionary<(ESkillType, EAttrSkillType), bool>();

	private Dictionary<(ESkillType, EAttrSkillType), float> m_cache = new Dictionary<(ESkillType, EAttrSkillType), float>();

	public void Init()
	{
		zbSkillTypeAttrCsvRecord[] records = zbDataConfig.Instance.skillTypeAttrCsvTable.GetRecords().records;
		foreach (zbSkillTypeAttrCsvRecord zbSkillTypeAttrCsvRecord2 in records)
		{
			if (zbSkillTypeAttrCsvRecord2.show == 1)
			{
				(ESkillType, EAttrSkillType) key = ((ESkillType)zbSkillTypeAttrCsvRecord2.id, (EAttrSkillType)zbSkillTypeAttrCsvRecord2.attrId);
				m_attrShow[key] = true;
			}
		}
	}

	private bool IsNeedShow(ESkillType skillType, EAttrSkillType attrType)
	{
		m_attrShow.TryGetValue((skillType, attrType), out var value);
		return value;
	}

	private void AddValue(ESkillType skillType, EAttrSkillType attrType, float newValue)
	{
		(ESkillType, EAttrSkillType) key = (skillType, attrType);
		if (m_cache.ContainsKey(key))
		{
			m_cache[key] += newValue;
		}
		else
		{
			m_cache[key] = newValue;
		}
	}

	public void Build()
	{
		m_cache.Clear();
		foreach (WarRoomDetail room in zbServerData.sGameData.WarRoomInfo.RoomList)
		{
			foreach (WarRoomAttrDetail attr in room.AttrList)
			{
				if (IsNeedShow(room.SkillType, attr.Type))
				{
					AddValue(room.SkillType, attr.Type, attr.Value);
				}
			}
		}
	}

	public bool DataChange()
	{
		bool result = false;
		Dictionary<(ESkillType, EAttrSkillType), float> dictionary = new Dictionary<(ESkillType, EAttrSkillType), float>(m_cache);
		Build();
		foreach (KeyValuePair<(ESkillType, EAttrSkillType), float> item in m_cache)
		{
			(ESkillType, EAttrSkillType) key = item.Key;
			float value = item.Value;
			if (!dictionary.TryGetValue(key, out var value2))
			{
				continue;
			}
			float value3 = value - value2;
			if (Math.Abs(value3) > 0.001f && IsNeedShow(key.Item1, key.Item2))
			{
				zbSkillAttrCsvRecord recordByKey = zbDataConfig.Instance.skillAttrCsvTable.GetRecordByKey((int)key.Item2);
				if (recordByKey != null)
				{
					bool isPercentageDisplay = recordByKey.showPercent > 0;
					string msg = zbAttrHelper.GetAttrName(EAttrType.Skill, (int)key.Item1, (int)key.Item2) + " " + zbAttrHelper.FormatAttrValue(value3, isPercentageDisplay, isDisplayAdd: true);
					zbUIMgr.Instance.BubbleInfo(msg);
					result = true;
				}
			}
		}
		return result;
	}
}
