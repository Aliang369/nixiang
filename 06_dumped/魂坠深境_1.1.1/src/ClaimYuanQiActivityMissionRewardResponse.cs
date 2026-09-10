using System;
using System.Collections.Generic;

[Serializable]
public sealed class ClaimYuanQiActivityMissionRewardResponse
{
	public GetYuanQiActivityStateResponse state;

	public List<GlobalRewardInfo> rewards = new List<GlobalRewardInfo>();

	public bool isDuplicateClaim;
}
