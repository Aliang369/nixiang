using System;
using System.Collections.Generic;

[Serializable]
public sealed class GetYuanQiActivityStateResponse
{
	public string activityId;

	public bool isOpen;

	public DateTime? activityEndTimeUtc;

	public List<YuanQiActivityMissionServerState> missions = new List<YuanQiActivityMissionServerState>();
}
