using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;

public sealed class YuanQiActivityServerStub : IYuanQiActivityServer
{
	private readonly ApplicationStateInfo applicationState;

	public YuanQiActivityServerStub(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState ?? throw new ArgumentNullException("applicationState");
	}

	public async Task<GetYuanQiActivityStateResponse> GetActivityStateAsync(GetYuanQiActivityStateRequest request)
	{
		return ToClient((await applicationState.ChillyRoomSDK.YuanQiActivity.GetActivityStateAsync(new GetYuanQiActivityStateRequestDto
		{
			ActivityId = request.activityId
		}))?.State);
	}

	public async Task<ReportYuanQiActivityMissionReachedResponse> ReportMissionReachedAsync(ReportYuanQiActivityMissionReachedRequest request)
	{
		ReportYuanQiActivityMissionReachedResponseDto reportYuanQiActivityMissionReachedResponseDto = await applicationState.ChillyRoomSDK.YuanQiActivity.ReportMissionReachedAsync(new ReportYuanQiActivityMissionReachedRequestDto
		{
			ActivityId = request.activityId,
			MissionId = request.missionId,
			EvidenceType = request.evidenceType,
			Progress = request.progress,
			ClientEventTimeUtc = request.clientEventTimeUtc,
			GameGuid = request.gameGuid,
			CharacterId = request.characterId
		});
		return new ReportYuanQiActivityMissionReachedResponse
		{
			state = ToClient(reportYuanQiActivityMissionReachedResponseDto?.State)
		};
	}

	public async Task<ClaimYuanQiActivityMissionRewardResponse> ClaimMissionRewardAsync(ClaimYuanQiActivityMissionRewardRequest request)
	{
		ClaimYuanQiActivityMissionRewardResponseDto claimYuanQiActivityMissionRewardResponseDto = await applicationState.ChillyRoomSDK.YuanQiActivity.ClaimMissionRewardAsync(new ClaimYuanQiActivityMissionRewardRequestDto
		{
			ActivityId = request.activityId,
			MissionId = request.missionId
		});
		return new ClaimYuanQiActivityMissionRewardResponse
		{
			state = ToClient(claimYuanQiActivityMissionRewardResponseDto?.State),
			rewards = ToClientRewards(claimYuanQiActivityMissionRewardResponseDto?.Rewards),
			isDuplicateClaim = (claimYuanQiActivityMissionRewardResponseDto?.IsDuplicateClaim ?? false)
		};
	}

	private GetYuanQiActivityStateResponse ToClient(YuanQiActivityStateDto state)
	{
		if (state == null)
		{
			return new GetYuanQiActivityStateResponse();
		}
		return new GetYuanQiActivityStateResponse
		{
			activityId = state.ActivityId,
			isOpen = state.IsOpen,
			activityEndTimeUtc = state.ActivityEndTimeUtc?.UtcDateTime,
			missions = (state.Missions?.Where((YuanQiActivityMissionStateDto mission) => mission != null).Select(ToClient).ToList() ?? new List<YuanQiActivityMissionServerState>())
		};
	}

	private static YuanQiActivityMissionServerState ToClient(YuanQiActivityMissionStateDto state)
	{
		return new YuanQiActivityMissionServerState
		{
			missionId = state.MissionId,
			progress = state.Progress,
			status = ToMissionStatus(state.Status)
		};
	}

	private List<GlobalRewardInfo> ToClientRewards(IEnumerable<ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo> rewards)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		if (rewards == null)
		{
			return list;
		}
		NetworkConvertContext context = new NetworkConvertContext(applicationState);
		foreach (ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo reward in rewards)
		{
			GlobalRewardInfo globalRewardInfo = reward?.ToClient(context);
			if (globalRewardInfo != null)
			{
				list.Add(globalRewardInfo);
			}
		}
		return list;
	}

	private static YuanQiActivityMissionStatus ToMissionStatus(int status)
	{
		if (!Enum.IsDefined(typeof(YuanQiActivityMissionStatus), status))
		{
			return YuanQiActivityMissionStatus.Locked;
		}
		return (YuanQiActivityMissionStatus)status;
	}
}
