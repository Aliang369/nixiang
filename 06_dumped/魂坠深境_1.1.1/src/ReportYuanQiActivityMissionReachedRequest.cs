using System;

[Serializable]
public sealed class ReportYuanQiActivityMissionReachedRequest
{
	public string activityId;

	public string missionId;

	public string evidenceType;

	public int progress;

	public DateTime clientEventTimeUtc;

	public string gameGuid;

	public string characterId;
}
