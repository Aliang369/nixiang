using System;

public sealed class DailyChallengeEntryStateProvider : IChallengeEntryStateProvider
{
	private readonly DailyChallengeClientConfig config;

	private readonly IDailyChallengeClock clock;

	public DailyChallengeEntryStateProvider(DailyChallengeClientConfig config, IDailyChallengeClock clock = null)
	{
		this.config = config;
		this.clock = clock ?? new SystemDailyChallengeClock();
	}

	public ChallengeEntryRuntimeState GetState(ChallengeEntryLayoutConfig.EntryDefinition definition)
	{
		DateTime utcNow = clock.UtcNow;
		ChallengeEntryRuntimeState challengeEntryRuntimeState = StaticChallengeEntryStateProvider.CreateBaseState(definition, utcNow);
		if (config == null)
		{
			challengeEntryRuntimeState.unlocked = false;
			return challengeEntryRuntimeState;
		}
		TimeSpan timeSpan = DailyChallengeClock.Remaining(DailyChallengeClock.GetNextDailyResetUtc(utcNow, config.utcOffsetHours, config.dailyResetHour), utcNow);
		string text = ScriptableObjectSingleton<TextDataPreset>.instance?.term_refresh_countdown ?? string.Empty;
		challengeEntryRuntimeState.timeText = $"{text} {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		return challengeEntryRuntimeState;
	}
}
