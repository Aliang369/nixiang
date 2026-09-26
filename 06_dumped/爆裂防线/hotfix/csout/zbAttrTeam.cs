using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbAttrTeam
{
	private Dictionary<EAttrTeamType, float> m_cache = new Dictionary<EAttrTeamType, float>();

	public void Init()
	{
	}

	private void SetValue(EAttrTeamType attrType, float value)
	{
		m_cache[attrType] = value;
	}

	private float GetValue(EAttrTeamType attrType)
	{
		m_cache.TryGetValue(attrType, out var value);
		return value;
	}

	public float GetDiffAndUpdate(EAttrTeamType attrType, float newValue)
	{
		float value = GetValue(attrType);
		float result = newValue - value;
		SetValue(attrType, newValue);
		return result;
	}

	public void Build()
	{
		m_cache.Clear();
		foreach (BattleTeamAttrDetail attr in zbServerData.sGameData.RoleTeamInfo.AttrList)
		{
			SetValue(attr.Type, attr.Value);
		}
	}

	public bool DataChange()
	{
		bool result = false;
		foreach (BattleTeamAttrDetail attr in zbServerData.sGameData.RoleTeamInfo.AttrList)
		{
			float diffAndUpdate = GetDiffAndUpdate(attr.Type, attr.Value);
			if (Math.Abs(diffAndUpdate) > 0.001f)
			{
				zbTeamAttrCsvRecord recordByKey = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)attr.Type);
				if (recordByKey != null)
				{
					bool isPercentageDisplay = recordByKey.showPercent > 0;
					string msg = zbAttrHelper.GetAttrName(EAttrType.Team, 0, (int)attr.Type) + " " + zbAttrHelper.FormatAttrValue(diffAndUpdate, isPercentageDisplay, isDisplayAdd: true);
					zbUIMgr.Instance.BubbleInfo(msg);
					result = true;
				}
			}
		}
		Build();
		return result;
	}
}
