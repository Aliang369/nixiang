using System.Collections.Generic;

public class YuanQiActivityDef : ActivityDef
{
	public List<YuanQiActivityMissionData> activityMissionConfigs = YuanQiActivityMissionDataDefault.BuildLocalPreviewConfigs();

	public void Rebuild()
	{
		activityMissionConfigs = YuanQiActivityMissionDataDefault.BuildLocalPreviewConfigs();
	}
}
