using Proto.Enums;

public static class zbAttrHelper
{
	public static string GetAttrName(EAttrType attrType, int funcId, int attrId)
	{
		return attrType switch
		{
			EAttrType.Team => GetTeamAttrName(attrId), 
			EAttrType.Skill => GetSkillTypeName(funcId) + GetSkillAttrName(attrId), 
			EAttrType.Defense => GetWallTypeName(funcId) + GetWallAttrName(attrId), 
			EAttrType.Monster => GetMonsterAttrName(attrId), 
			_ => "", 
		};
	}

	public static string GetTeamAttrName(int id)
	{
		zbTeamAttrCsvRecord recordByKey = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.name;
		}
		return "";
	}

	private static string GetSkillTypeName(int id)
	{
		zbSkillTypeCsvRecord recordByKey = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.name;
		}
		return "";
	}

	private static string GetSkillAttrName(int id)
	{
		zbSkillAttrCsvRecord recordByKey = zbDataConfig.Instance.skillAttrCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.name;
		}
		return "";
	}

	private static string GetMonsterAttrName(int id)
	{
		zbMonsterAttrCsvRecord recordByKey = zbDataConfig.Instance.monsterAttrCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.name;
		}
		return "";
	}

	private static string GetWallTypeName(int id)
	{
		zbDefenseTypeCsvRecord recordByKey = zbDataConfig.Instance.defenseTypeCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.name;
		}
		return "";
	}

	private static string GetWallAttrName(int id)
	{
		zbDefenseAttrCsvRecord recordByKey = zbDataConfig.Instance.defenseAttrCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.name;
		}
		return "";
	}

	public static string FormatAttrValue(float value, bool isPercentageDisplay, bool isDisplayAdd = false)
	{
		string text = "";
		if (isPercentageDisplay)
		{
			value *= 100f;
			text = ((value % 1f == 0f) ? value.ToString("0") : value.ToString("0.00"));
			text += "%";
		}
		else
		{
			text = ((value % 1f == 0f) ? value.ToString("0") : value.ToString("0.00"));
		}
		if (isDisplayAdd && value > 0f)
		{
			text = "+" + text;
		}
		return text;
	}
}
