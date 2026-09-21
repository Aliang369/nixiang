using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public sealed class YuanQiActivityModel
{
	[SaveField(null, null)]
	private List<YuanQiActivityMissionItem> missionItems = new List<YuanQiActivityMissionItem>();

	[SaveField(null, null)]
	private int resetVersion;

	[NonSerialized]
	private Dictionary<string, YuanQiActivityMissionItem> missionById = new Dictionary<string, YuanQiActivityMissionItem>();

	[NonSerialized]
	private long endTimeTicks;

	[NonSerialized]
	private long lastSyncTimeTicks;

	public IReadOnlyList<YuanQiActivityMissionItem> MissionItems => missionItems;

	public int ResetVersion => resetVersion;

	public long EndTimeTicks => endTimeTicks;

	public long LastSyncTimeTicks => lastSyncTimeTicks;

	public bool IsActivityEnabled
	{
		get
		{
			if (endTimeTicks > 0)
			{
				return DateTime.UtcNow.Ticks < endTimeTicks;
			}
			return true;
		}
	}

	public void Initialize(IReadOnlyList<YuanQiActivityMissionData> configs)
	{
		List<YuanQiActivityMissionData> list = (from config in configs ?? Array.Empty<YuanQiActivityMissionData>()
			where config != null && !string.IsNullOrEmpty(config.missionId)
			orderby config.order
			select config).ToList();
		if (missionItems == null)
		{
			missionItems = new List<YuanQiActivityMissionItem>();
		}
		while (missionItems.Count < list.Count)
		{
			missionItems.Add(new YuanQiActivityMissionItem());
		}
		if (missionItems.Count > list.Count)
		{
			missionItems.RemoveRange(list.Count, missionItems.Count - list.Count);
		}
		missionById = new Dictionary<string, YuanQiActivityMissionItem>(list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			List<YuanQiActivityMissionItem> list2 = missionItems;
			int index = num;
			if (list2[index] == null)
			{
				YuanQiActivityMissionItem yuanQiActivityMissionItem = (list2[index] = new YuanQiActivityMissionItem());
			}
			missionItems[num].BindConfig(list[num]);
			missionById[list[num].missionId] = missionItems[num];
		}
		RefreshUnlocks();
	}

	public YuanQiActivityMissionItem GetPreviousMissionItem(int order)
	{
		if (missionItems == null)
		{
			return null;
		}
		YuanQiActivityMissionItem result = null;
		int num = int.MinValue;
		foreach (YuanQiActivityMissionItem missionItem in missionItems)
		{
			if (missionItem?.Data != null && missionItem.Data.order < order && missionItem.Data.order > num)
			{
				result = missionItem;
				num = missionItem.Data.order;
			}
		}
		return result;
	}

	public YuanQiActivityMissionItem GetCurrentMission()
	{
		return (from mission in missionItems
			where mission?.Data != null && mission.Status != YuanQiActivityMissionStatus.Claimed
			orderby mission.Data.order
			select mission).FirstOrDefault();
	}

	public IReadOnlyList<YuanQiActivityMissionServerState> CreateServerStateSnapshot()
	{
		return (from mission in missionItems
			where mission?.Data != null
			orderby mission.Data.order
			select new YuanQiActivityMissionServerState
			{
				missionId = mission.Data.missionId,
				progress = mission.Progress,
				status = mission.Status
			}).ToList();
	}

	public void SetActivityState(bool enabled, long activityEndTimeTicks)
	{
		endTimeTicks = (enabled ? activityEndTimeTicks : DateTime.UtcNow.Ticks);
		lastSyncTimeTicks = DateTime.UtcNow.Ticks;
	}

	public TimeSpan GetRemainTime()
	{
		if (endTimeTicks <= 0)
		{
			return TimeSpan.Zero;
		}
		TimeSpan timeSpan = new DateTime(endTimeTicks, DateTimeKind.Utc) - DateTime.UtcNow;
		if (!(timeSpan > TimeSpan.Zero))
		{
			return TimeSpan.Zero;
		}
		return timeSpan;
	}

	public bool TryGetMission(string missionId, out YuanQiActivityMissionItem mission)
	{
		mission = null;
		if (!string.IsNullOrEmpty(missionId) && missionById != null)
		{
			return missionById.TryGetValue(missionId, out mission);
		}
		return false;
	}

	public bool HasRewardToClaim()
	{
		return missionItems.Any((YuanQiActivityMissionItem mission) => mission.CanClaim);
	}

	public YuanQiActivityMissionItem DebugCompleteCurrentMissionLocally()
	{
		YuanQiActivityMissionItem currentMission = GetCurrentMission();
		if (currentMission == null || !currentMission.Complete())
		{
			return null;
		}
		return currentMission;
	}

	public bool Claim(string missionId)
	{
		if (!TryGetMission(missionId, out var mission) || !mission.Claim())
		{
			return false;
		}
		RefreshUnlocks();
		return true;
	}

	public bool ApplyServerStates(IEnumerable<YuanQiActivityMissionServerState> serverStates)
	{
		if (serverStates == null)
		{
			return false;
		}
		bool flag = false;
		foreach (YuanQiActivityMissionServerState serverState in serverStates)
		{
			if (serverState != null && TryGetMission(serverState.missionId, out var mission))
			{
				flag |= mission.ApplyServerState(serverState.progress, serverState.status);
			}
		}
		RefreshUnlocks();
		lastSyncTimeTicks = DateTime.UtcNow.Ticks;
		return flag;
	}

	public IReadOnlyList<YuanQiActivityMissionItem> CollectMissions(YuanQiActivityMissionType type, Func<YuanQiActivityMissionData, bool> predicate)
	{
		List<YuanQiActivityMissionItem> list = new List<YuanQiActivityMissionItem>();
		foreach (YuanQiActivityMissionItem missionItem in missionItems)
		{
			YuanQiActivityMissionData data = missionItem.Data;
			if (data.type == type && predicate(data) && missionItem.AddProgress())
			{
				list.Add(missionItem);
			}
		}
		return list;
	}

	public void MarkReset(int version)
	{
		resetVersion = version;
	}

	public void ResetAllProgress()
	{
		foreach (YuanQiActivityMissionItem missionItem in missionItems)
		{
			missionItem?.Reset();
		}
		RefreshUnlocks();
	}

	private void RefreshUnlocks()
	{
		if (missionItems.Count == 0)
		{
			return;
		}
		missionItems[0].Unlock();
		for (int i = 1; i < missionItems.Count; i++)
		{
			if (missionItems[i - 1].Status == YuanQiActivityMissionStatus.Claimed)
			{
				missionItems[i].Unlock();
			}
		}
	}
}
