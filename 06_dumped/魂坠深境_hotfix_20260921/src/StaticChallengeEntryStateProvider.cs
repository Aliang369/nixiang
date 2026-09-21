using System;

public sealed class StaticChallengeEntryStateProvider : IChallengeEntryStateProvider
{
	private readonly IDailyChallengeClock clock;

	public StaticChallengeEntryStateProvider(IDailyChallengeClock clock = null)
	{
		this.clock = clock ?? new SystemDailyChallengeClock();
	}

	public ChallengeEntryRuntimeState GetState(ChallengeEntryLayoutConfig.EntryDefinition definition)
	{
		return CreateBaseState(definition, clock.UtcNow);
	}

	internal static ChallengeEntryRuntimeState CreateBaseState(ChallengeEntryLayoutConfig.EntryDefinition definition, DateTime utcNow)
	{
		if (definition == null)
		{
			return new ChallengeEntryRuntimeState();
		}
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		bool flag = definition.unlocked && definition.destination != ChallengeEntryDestination.Locked && (definition.destination != ChallengeEntryDestination.Constellation || definition.targetConstellation != null) && (definition.destination != ChallengeEntryDestination.Constellation || definition.targetConstellation.CanOpen(utcNow));
		string text = ((flag || definition.destination != ChallengeEntryDestination.Constellation) ? (definition.titleSource switch
		{
			ChallengeEntryTitleSource.HighDifficulty => instance?.term_season_fixed_hard_challenge_constellation, 
			ChallengeEntryTitleSource.DailyChallenge => instance?.term_daily_challenge_title, 
			_ => instance?.term_coming_soon, 
		}) : instance?.term_coming_soon);
		string text2 = text;
		return new ChallengeEntryRuntimeState
		{
			visible = definition.visible,
			unlocked = flag,
			artwork = definition.artwork,
			title = (text2 ?? string.Empty),
			timeText = GetConstellationRemainingTimeText(definition, utcNow),
			destination = definition.destination,
			targetConstellation = definition.targetConstellation
		};
	}

	private static string GetConstellationRemainingTimeText(ChallengeEntryLayoutConfig.EntryDefinition definition, DateTime utcNow)
	{
		if (definition.destination != ChallengeEntryDestination.Constellation || definition.targetConstellation == null)
		{
			return string.Empty;
		}
		UnityDateTime? expireDateTime = definition.targetConstellation.GetExpireDateTime();
		if (!expireDateTime.HasValue)
		{
			return string.Empty;
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance?.term_contract_remain_time_arg0_to_expire.FormatWith(TextDisplayHelper.GetAdaptiveCountdownText(expireDateTime.Value.UtcDateTime - utcNow)) ?? string.Empty;
	}
}
