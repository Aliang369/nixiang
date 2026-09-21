using System;

[Serializable]
public sealed class YuanQiActivityMissionItem
{
	[SaveField(null, null)]
	private int progress;

	[SaveField(null, null)]
	private YuanQiActivityMissionStatus status;

	public YuanQiActivityMissionData Data { get; private set; }

	public int Progress => progress;

	public YuanQiActivityMissionStatus Status => status;

	public bool CanClaim => status == YuanQiActivityMissionStatus.Reached;

	public bool CanJump
	{
		get
		{
			if (status == YuanQiActivityMissionStatus.InProgress)
			{
				return Data.jumpType != YuanQiActivityMissionJumpType.None;
			}
			return false;
		}
	}

	public bool HasClaim => status == YuanQiActivityMissionStatus.Claimed;

	public void BindConfig(YuanQiActivityMissionData missionData)
	{
		Data = missionData ?? throw new ArgumentNullException("missionData");
		NormalizeState();
	}

	public void Reset()
	{
		progress = 0;
		status = YuanQiActivityMissionStatus.Locked;
	}

	public bool Unlock()
	{
		if (status != YuanQiActivityMissionStatus.Locked)
		{
			return false;
		}
		status = ((progress < Data.targetProgress) ? YuanQiActivityMissionStatus.InProgress : YuanQiActivityMissionStatus.Reached);
		return true;
	}

	public bool AddProgress(int value = 1)
	{
		if (status != YuanQiActivityMissionStatus.InProgress || value <= 0)
		{
			return false;
		}
		int num = Math.Min(Data.targetProgress, progress + value);
		if (num == progress)
		{
			return false;
		}
		progress = num;
		if (progress >= Data.targetProgress)
		{
			status = YuanQiActivityMissionStatus.Reached;
		}
		return true;
	}

	public bool Complete()
	{
		if (status != YuanQiActivityMissionStatus.InProgress)
		{
			return false;
		}
		progress = Data.targetProgress;
		status = YuanQiActivityMissionStatus.Reached;
		return true;
	}

	public bool SyncProgressFromLocal(int localProgress)
	{
		if (status != YuanQiActivityMissionStatus.InProgress || localProgress <= progress)
		{
			return false;
		}
		progress = Math.Min(Data.targetProgress, localProgress);
		if (progress >= Data.targetProgress)
		{
			status = YuanQiActivityMissionStatus.Reached;
		}
		return true;
	}

	public bool Claim()
	{
		if (status != YuanQiActivityMissionStatus.Reached)
		{
			return false;
		}
		status = YuanQiActivityMissionStatus.Claimed;
		return true;
	}

	public bool ApplyServerState(int serverProgress, YuanQiActivityMissionStatus serverStatus)
	{
		int num = progress;
		YuanQiActivityMissionStatus yuanQiActivityMissionStatus = status;
		progress = ((serverStatus >= YuanQiActivityMissionStatus.Reached) ? serverProgress : Math.Max(progress, serverProgress));
		status = serverStatus;
		NormalizeState();
		if (num == progress)
		{
			return yuanQiActivityMissionStatus != status;
		}
		return true;
	}

	private void NormalizeState()
	{
		progress = Math.Max(0, Math.Min(progress, Data.targetProgress));
		if (status < YuanQiActivityMissionStatus.Locked || status > YuanQiActivityMissionStatus.Claimed)
		{
			status = YuanQiActivityMissionStatus.Locked;
		}
		if (status == YuanQiActivityMissionStatus.InProgress && progress >= Data.targetProgress)
		{
			status = YuanQiActivityMissionStatus.Reached;
		}
		if (status == YuanQiActivityMissionStatus.Reached || status == YuanQiActivityMissionStatus.Claimed)
		{
			progress = Data.targetProgress;
		}
	}
}
