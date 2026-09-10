using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;
using Newtonsoft.Json;

public sealed class DailyChallengeFixtureDataProvider : IDailyChallengeAsyncDataProvider, IDailyChallengeDataProvider
{
	private readonly string configJson;

	private readonly string statusJson;

	private readonly string rankingJson;

	private readonly Func<string, string> recordJson;

	private readonly ApplicationStateInfo applicationState;

	private readonly DailyChallengeClientConfig clientConfig;

	private readonly IDailyChallengeClock clock;

	private DailyChallengePageState current;

	public DailyChallengeFixtureDataProvider(string configJson, string statusJson, string rankingJson, Func<string, string> recordJson, ApplicationStateInfo applicationState, DailyChallengeClientConfig clientConfig, IDailyChallengeClock clock = null)
	{
		this.configJson = configJson ?? throw new ArgumentNullException("configJson");
		this.statusJson = statusJson ?? throw new ArgumentNullException("statusJson");
		this.rankingJson = rankingJson ?? throw new ArgumentNullException("rankingJson");
		this.recordJson = recordJson;
		this.applicationState = applicationState;
		this.clientConfig = clientConfig;
		this.clock = clock ?? new SystemDailyChallengeClock();
		Refresh();
	}

	public DailyChallengePageState GetCurrentState()
	{
		return current;
	}

	public Task<DailyChallengePageState> RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		Refresh();
		return Task.FromResult(current);
	}

	public Task<GameRecord> GetRecordDetailAsync(long playerId, string recordId, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(GetRecordDetail(recordId));
	}

	public GameRecord GetRecordDetail(string recordId)
	{
		string value = recordJson?.Invoke(recordId);
		if (!string.IsNullOrWhiteSpace(value))
		{
			return DailyChallengeClientMapper.ToRecordDetail(JsonConvert.DeserializeObject<DailyChallengeRecordDetailDTO>(value), applicationState, clientConfig);
		}
		return null;
	}

	public void Refresh()
	{
		current = DailyChallengeClientMapper.ToPageState(JsonConvert.DeserializeObject<DailyChallengeConfigDTO>(configJson), JsonConvert.DeserializeObject<DailyChallengePlayerStatusDTO>(statusJson), JsonConvert.DeserializeObject<List<DailyChallengeRankEntryDTO>>(rankingJson), applicationState, clientConfig, clock.UtcNow);
	}
}
