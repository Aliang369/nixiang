using System;
using System.Collections.Generic;

public class YuanQiActivityMissionGiftCodeEventData
{
	public readonly string code;

	public readonly List<GlobalRewardInfo> rewards;

	public readonly DateTime triggerTime;

	public YuanQiActivityMissionGiftCodeEventData(string code, List<GlobalRewardInfo> rewards)
	{
		this.code = code;
		this.rewards = rewards;
		triggerTime = DateTime.UtcNow;
	}
}
