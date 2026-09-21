using System;
using System.Collections.Generic;
using System.Text;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.ContainerOfSoul;

public class LimitedTimeActivityManager : GlobalGameManager, ILimitedTimeActivityService
{
	private readonly Dictionary<string, LimitedTimeActivityRuntimeConfig> runtimeConfigs = new Dictionary<string, LimitedTimeActivityRuntimeConfig>(StringComparer.Ordinal);

	public bool IsReady { get; private set; }

	public bool IsFetching { get; private set; }

	public override void Init(ApplicationStateInfo applicationState)
	{
		ClearRuntimeConfigs();
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			ApplyLocalRuntimeConfigs();
		}
	}

	public bool IsOpen(string activityId)
	{
		return IsOpen(activityId, GetCurrentUtcTime());
	}

	public bool IsOpen(string activityId, DateTime utcNow)
	{
		if (!IsReady || !TryGetRuntimeConfig(activityId, out var config))
		{
			return false;
		}
		return config.Contains(utcNow);
	}

	public bool TryGetRuntimeConfig(string activityId, out LimitedTimeActivityRuntimeConfig config)
	{
		config = null;
		if (!string.IsNullOrWhiteSpace(activityId))
		{
			return runtimeConfigs.TryGetValue(activityId, out config);
		}
		return false;
	}

	public void ApplyRuntimeConfigs(IEnumerable<LimitedTimeActivityRuntimeConfig> configs)
	{
		runtimeConfigs.Clear();
		if (configs != null)
		{
			foreach (LimitedTimeActivityRuntimeConfig config in configs)
			{
				if (config == null || !config.IsValid)
				{
					TraceLog.Error("[LimitedTimeActivityManager] Skip invalid runtime config.");
					continue;
				}
				if (runtimeConfigs.ContainsKey(config.ActivityId))
				{
					TraceLog.Error("[LimitedTimeActivityManager] Duplicate activity id: " + config.ActivityId);
				}
				runtimeConfigs[config.ActivityId] = config;
			}
		}
		IsReady = true;
	}

	private void ApplyResponse(GetLimitedTimeActivityConfigsResponse response)
	{
		List<LimitedTimeActivityRuntimeConfig> list = new List<LimitedTimeActivityRuntimeConfig>();
		if (response?.Activities != null)
		{
			foreach (LimitedTimeActivityRuntimeConfigDto activity in response.Activities)
			{
				if (TryCreateConfig(activity, out var config))
				{
					list.Add(config);
				}
			}
		}
		ApplyRuntimeConfigs(list);
		TraceLog.TryDebug(BuildRuntimeConfigLog);
	}

	private string BuildRuntimeConfigLog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"[LimitedTimeActivityManager] Runtime configs ready. Count: {runtimeConfigs.Count}");
		foreach (LimitedTimeActivityRuntimeConfig value in runtimeConfigs.Values)
		{
			stringBuilder.AppendLine($"- {value.ActivityId}: {value.StartTimeUtc:O} -> {value.EndTimeUtc:O}");
		}
		return stringBuilder.ToString();
	}

	private bool TryCreateConfig(LimitedTimeActivityRuntimeConfigDto dto, out LimitedTimeActivityRuntimeConfig config)
	{
		config = null;
		if (dto == null || string.IsNullOrWhiteSpace(dto.ActivityId))
		{
			TraceLog.Error("[LimitedTimeActivityManager] Skip runtime config with empty activity id.");
			return false;
		}
		config = new LimitedTimeActivityRuntimeConfig(dto.ActivityId, dto.StartTimeUtc, dto.EndTimeUtc, dto.Enabled, dto.Title, dto.Description, dto.HelpText);
		if (config.IsValid)
		{
			return true;
		}
		TraceLog.Error("[LimitedTimeActivityManager] Skip invalid runtime config: " + dto.ActivityId);
		config = null;
		return false;
	}

	private void ApplyLocalRuntimeConfigs()
	{
		List<LimitedTimeActivityRuntimeConfig> list = new List<LimitedTimeActivityRuntimeConfig>();
		List<ActivityDef> list2 = base.applicationState.assetCatalog?.activityCatalog?.ValidAssets;
		if (list2 != null)
		{
			foreach (ActivityDef item in list2)
			{
				if (!(item == null))
				{
					list.Add(new LimitedTimeActivityRuntimeConfig(item.activityId, item.startTime.UtcDateTime, item.endTime.UtcDateTime));
				}
			}
		}
		ApplyRuntimeConfigs(list);
		TraceLog.TryDebug(BuildRuntimeConfigLog);
	}

	private void ClearRuntimeConfigs()
	{
		runtimeConfigs.Clear();
		IsReady = false;
		IsFetching = false;
	}

	private DateTime GetCurrentUtcTime()
	{
		try
		{
			return ServerTimePoller.Instance.GetCurrentTime().UtcDateTime;
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
			return DateTime.UtcNow;
		}
	}

	public GameAsyncOperation CreateFetchRuntimeConfigsOperation()
	{
		IsReady = false;
		IsFetching = true;
		GeneralGameAsyncOperation<GetLimitedTimeActivityConfigsRequest, GetLimitedTimeActivityConfigsResponse> op = GameAsyncOperation.Get(new GetLimitedTimeActivityConfigsRequest(), (GetLimitedTimeActivityConfigsRequest req) => base.applicationState.ChillyRoomSDK.LimitedTimeActivity.GetLimitedTimeActivityConfigsAsync(req));
		op.HideFaultPanel().OnComplete(delegate
		{
			ApplyResponse(op.OpResult);
		}).OnFault(delegate
		{
			ClearRuntimeConfigs();
			TraceLog.Error("[LimitedTimeActivityManager] Fetch runtime configs failed.");
		})
			.OnOver(delegate
			{
				IsFetching = false;
			});
		return op;
	}
}
