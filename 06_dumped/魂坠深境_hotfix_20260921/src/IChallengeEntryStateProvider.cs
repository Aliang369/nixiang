public interface IChallengeEntryStateProvider
{
	ChallengeEntryRuntimeState GetState(ChallengeEntryLayoutConfig.EntryDefinition definition);
}
