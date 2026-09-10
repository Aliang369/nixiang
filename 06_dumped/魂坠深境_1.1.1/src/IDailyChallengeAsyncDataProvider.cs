using System.Threading;
using System.Threading.Tasks;

public interface IDailyChallengeAsyncDataProvider : IDailyChallengeDataProvider
{
	Task<DailyChallengePageState> RefreshAsync(CancellationToken cancellationToken = default(CancellationToken));

	Task<GameRecord> GetRecordDetailAsync(long playerId, string recordId, CancellationToken cancellationToken = default(CancellationToken));
}
