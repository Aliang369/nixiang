using System.Collections.Generic;
using UnityEngine;

public class NewbieFestivalDef : ActivityDef
{
	[CTSOAssetName]
	[LocalizeText("新生游艺活动标题")]
	public string title;

	[TextArea]
	[LocalizeText("新生游艺活动说明")]
	public string description;

	[TextArea]
	[LocalizeText("新生游艺活动帮助")]
	public string helpText;

	[LocalizeText("新生游艺活动入口")]
	public string entranceText;

	public int totalDays = 12;

	public int dailyRewardCount = 8;

	public MatterDef freeTicketMatter;

	public MatterDef paidTicketMatter;

	public List<RoutineQuestDef> dailyTasks = new List<RoutineQuestDef>();

	public List<NewbieFestivalDailyRewardGroup> dailyRewardGroups = new List<NewbieFestivalDailyRewardGroup>();
}
