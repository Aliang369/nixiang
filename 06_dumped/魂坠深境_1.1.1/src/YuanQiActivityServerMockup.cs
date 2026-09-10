using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class YuanQiActivityServerMockup : IYuanQiActivityServer
{
	private readonly ApplicationStateInfo applicationState;

	public YuanQiActivityServerMockup(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState ?? throw new ArgumentNullException("applicationState");
	}

	public Task<GetYuanQiActivityStateResponse> GetActivityStateAsync(GetYuanQiActivityStateRequest request)
	{
		return Task.FromResult(BuildState(request.activityId));
	}

	public Task<ReportYuanQiActivityMissionReachedResponse> ReportMissionReachedAsync(ReportYuanQiActivityMissionReachedRequest request)
	{
		GetYuanQiActivityStateResponse getYuanQiActivityStateResponse = BuildState(request.activityId);
		if (!getYuanQiActivityStateResponse.isOpen || !TryGetCurrentMission(request.missionId, out var mission))
		{
			return Task.FromResult(new ReportYuanQiActivityMissionReachedResponse
			{
				state = getYuanQiActivityStateResponse
			});
		}
		YuanQiActivityMissionServerState yuanQiActivityMissionServerState = getYuanQiActivityStateResponse.missions.FirstOrDefault((YuanQiActivityMissionServerState m) => m.missionId == request.missionId);
		if (yuanQiActivityMissionServerState != null && yuanQiActivityMissionServerState.status == YuanQiActivityMissionStatus.InProgress)
		{
			yuanQiActivityMissionServerState.progress = mission.Data.targetProgress;
			yuanQiActivityMissionServerState.status = YuanQiActivityMissionStatus.Reached;
		}
		return Task.FromResult(new ReportYuanQiActivityMissionReachedResponse
		{
			state = getYuanQiActivityStateResponse
		});
	}

	public Task<ClaimYuanQiActivityMissionRewardResponse> ClaimMissionRewardAsync(ClaimYuanQiActivityMissionRewardRequest request)
	{
		GetYuanQiActivityStateResponse getYuanQiActivityStateResponse = BuildState(request.activityId);
		ClaimYuanQiActivityMissionRewardResponse claimYuanQiActivityMissionRewardResponse = new ClaimYuanQiActivityMissionRewardResponse
		{
			state = getYuanQiActivityStateResponse
		};
		if (!getYuanQiActivityStateResponse.isOpen || !TryGetCurrentMission(request.missionId, out var mission))
		{
			return Task.FromResult(claimYuanQiActivityMissionRewardResponse);
		}
		claimYuanQiActivityMissionRewardResponse.isDuplicateClaim = mission.Status == YuanQiActivityMissionStatus.Claimed;
		if (mission.Status != YuanQiActivityMissionStatus.Reached)
		{
			return Task.FromResult(claimYuanQiActivityMissionRewardResponse);
		}
		applicationState.yuanQiActivityManager?.Model?.Claim(request.missionId);
		claimYuanQiActivityMissionRewardResponse.state = BuildState(request.activityId);
		claimYuanQiActivityMissionRewardResponse.rewards = mission.Data.rewards?.Where((MatterReward reward) => reward != null).Select((MatterReward reward) => reward.GenerateReward(applicationState, GlobalRewardSourceType.Other, request.activityId)).ToList() ?? new List<GlobalRewardInfo>();
		return Task.FromResult(claimYuanQiActivityMissionRewardResponse);
	}

	private GetYuanQiActivityStateResponse BuildState(string activityId)
	{
		return new GetYuanQiActivityStateResponse
		{
			activityId = activityId,
			isOpen = IsActivityOpen(activityId),
			activityEndTimeUtc = GetActivityEndTime(activityId),
			missions = (applicationState.yuanQiActivityManager?.Model?.CreateServerStateSnapshot().ToList() ?? new List<YuanQiActivityMissionServerState>())
		};
	}

	private bool IsActivityOpen(string activityId)
	{
		if (string.IsNullOrEmpty(activityId))
		{
			return false;
		}
		return applicationState.limitedTimeActivityManager?.IsOpen(activityId) ?? false;
	}

	private DateTime? GetActivityEndTime(string activityId)
	{
		if (applicationState.limitedTimeActivityManager == null || !applicationState.limitedTimeActivityManager.TryGetRuntimeConfig(activityId, out var config))
		{
			return null;
		}
		return config.EndTimeUtc;
	}

	private bool TryGetCurrentMission(string missionId, out YuanQiActivityMissionItem mission)
	{
		mission = applicationState.yuanQiActivityManager?.Model?.GetCurrentMission();
		if (mission != null)
		{
			return string.Equals(mission.Data.missionId, missionId, StringComparison.Ordinal);
		}
		return false;
	}
}
