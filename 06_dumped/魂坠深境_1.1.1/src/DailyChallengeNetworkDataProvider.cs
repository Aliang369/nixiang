using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;

public sealed class DailyChallengeNetworkDataProvider : IDailyChallengeAsyncDataProvider, IDailyChallengeDataProvider
{
	private readonly ApplicationStateInfo applicationState;

	private readonly DailyChallengeClientConfig clientConfig;

	private readonly IDailyChallengeClock clock;

	private readonly DailyChallengeClient client;

	private DailyChallengePageState current;

	public DailyChallengeNetworkDataProvider(ApplicationStateInfo applicationState, DailyChallengeClientConfig clientConfig, IDailyChallengeClock clock = null)
	{
		this.applicationState = applicationState ?? throw new ArgumentNullException("applicationState");
		this.clientConfig = clientConfig ?? throw new ArgumentNullException("clientConfig");
		this.clock = clock ?? new SystemDailyChallengeClock();
		client = applicationState.ChillyRoomSDK?.DailyChallenge ?? throw new InvalidOperationException("DailyChallengeClient has not been initialized.");
	}

	public DailyChallengePageState GetCurrentState()
	{
		return current;
	}

	public async Task<DailyChallengePageState> RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		Task<DailyChallengeConfigDTO> configTask = client.CurrentConfigAsync(cancellationToken);
		Task<DailyChallengePlayerStatusDTO> statusTask = client.PlayerStatusAsync(cancellationToken);
		Task<ICollection<DailyChallengeRankEntryDTO>> rankingTask = client.RankingAsync(cancellationToken);
		await Task.WhenAll(configTask, statusTask, rankingTask);
		current = DailyChallengeClientMapper.ToPageState(await configTask, await statusTask, await rankingTask, applicationState, clientConfig, clock.UtcNow);
		return current;
	}

	public async Task<GameRecord> GetRecordDetailAsync(long playerId, string recordId, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(recordId))
		{
			return null;
		}
		DailyChallengeRecordDetailRequest body = new DailyChallengeRecordDetailRequest
		{
			PlayerId = playerId,
			GameRecordId = recordId
		};
		return DailyChallengeClientMapper.ToRecordDetail(await client.RecordDetailAsync(body, cancellationToken), applicationState, clientConfig);
	}

	public void Refresh()
	{
		throw new InvalidOperationException("Use RefreshAsync for the network provider.");
	}

	public GameRecord GetRecordDetail(string recordId)
	{
		throw new InvalidOperationException("Use GetRecordDetailAsync for the network provider.");
	}
}
