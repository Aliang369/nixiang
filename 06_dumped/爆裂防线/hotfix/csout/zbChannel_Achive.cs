using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbChannel_Achive
{
	private static readonly Dictionary<int, string> chapterAchiveIDs = new Dictionary<int, string>
	{
		{ 1, "CgkIyYeRod0UEAIQAA" },
		{ 10, "CgkIyYeRod0UEAIQAQ" },
		{ 30, "CgkIyYeRod0UEAIQAg" },
		{ 50, "CgkIyYeRod0UEAIQAw" },
		{ 100, "CgkIyYeRod0UEAIQBA" },
		{ 150, "CgkIyYeRod0UEAIQBQ" },
		{ 200, "CgkIyYeRod0UEAIQCg" },
		{ 300, "CgkIyYeRod0UEAIQCw" }
	};

	private static readonly Dictionary<int, string> herosAchiveIDs = new Dictionary<int, string>
	{
		{ 5, "CgkIyYeRod0UEAIQDA" },
		{ 10, "CgkIyYeRod0UEAIQDQ" },
		{ 15, "CgkIyYeRod0UEAIQDg" },
		{ 30, "CgkIyYeRod0UEAIQDw" }
	};

	private static readonly Dictionary<int, string> floorAchiveIDs = new Dictionary<int, string>
	{
		{ 7, "CgkIyYeRod0UEAIQEA" },
		{ 9, "CgkIyYeRod0UEAIQEQ" },
		{ 12, "CgkIyYeRod0UEAIQEg" }
	};

	private static void ReportAchieves(Dictionary<int, string> achieveDict, int value)
	{
		if (achieveDict == null || achieveDict.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, string> item in achieveDict)
		{
			if (value >= item.Key)
			{
				zbSdkHelper.Option("playgames_achive", item.Value);
			}
		}
	}

	public static void ReportAchive_Channel()
	{
		GameData sGameData = zbServerData.sGameData;
		if (sGameData != null && (zbLoginUtil.appChannel == EChannelType.TapTap || zbLoginUtil.appChannel == EChannelType.Ios || zbLoginUtil.appChannel == EChannelType.Hykb || zbLoginUtil.appChannel == EChannelType.Dyunion))
		{
			int value = ((sGameData.MainQuestInfo != null) ? sGameData.MainQuestInfo.MainLevel : 0);
			ReportAchieves(chapterAchiveIDs, value);
			int value2 = ((sGameData.HeroList != null) ? sGameData.HeroList.Count : 0);
			ReportAchieves(herosAchiveIDs, value2);
			int value3 = ((sGameData.HomelandInfo != null) ? sGameData.HomelandInfo.BaseInfo.FloorList.Count : 0);
			ReportAchieves(floorAchiveIDs, value3);
		}
	}
}
