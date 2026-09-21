public interface IDailyChallengeDataProvider
{
	DailyChallengePageState GetCurrentState();

	GameRecord GetRecordDetail(string recordId);

	void Refresh();
}
