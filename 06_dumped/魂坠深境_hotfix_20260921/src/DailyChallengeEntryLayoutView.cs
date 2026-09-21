using UnityEngine;
using UnityEngine.Events;

public sealed class DailyChallengeEntryLayoutView : MonoBehaviour
{
	[SerializeField]
	private ChallengeEntryLayoutConfig layoutConfig;

	[SerializeField]
	private ChallengeEntrySlotView treasureSlot;

	[SerializeField]
	private ChallengeEntrySlotView weeklySlot;

	[SerializeField]
	private ChallengeEntrySlotView highDifficultySlot;

	[SerializeField]
	private ChallengeEntrySlotView dailyChallengeSlot;

	private StarMapPanel host;

	private DailyChallengeClientConfig dailyChallengeConfig;

	private IChallengeEntryStateProvider staticProvider;

	private IChallengeEntryStateProvider dailyProvider;

	private float nextTimeRefresh;

	public ChallengeEntryLayoutConfig LayoutConfig => layoutConfig;

	public void Setup(StarMapPanel host, DailyChallengeClientConfig config)
	{
		SetupEntries(host, config);
	}

	public void SetupEntries(StarMapPanel host, DailyChallengeClientConfig config, IDailyChallengeClock clock = null)
	{
		this.host = host;
		dailyChallengeConfig = config;
		staticProvider = new StaticChallengeEntryStateProvider(clock);
		dailyProvider = new DailyChallengeEntryStateProvider(config, clock);
		RefreshEntries();
	}

	private void Update()
	{
		if (!(Time.unscaledTime < nextTimeRefresh))
		{
			nextTimeRefresh = Time.unscaledTime + 1f;
			RefreshTimeEntries();
		}
	}

	private void RefreshEntries()
	{
		if (layoutConfig == null)
		{
			Debug.LogError("DailyChallengeEntryLayoutView is missing ChallengeEntryLayoutConfig.", this);
			return;
		}
		BindStaticEntry(treasureSlot, ChallengeEntrySlotId.Treasure);
		BindStaticEntry(weeklySlot, ChallengeEntrySlotId.Weekly);
		BindStaticEntry(highDifficultySlot, ChallengeEntrySlotId.HighDifficulty);
		RefreshDailyChallengeEntry();
	}

	private void RefreshTimeEntries()
	{
		if (!(layoutConfig == null) && staticProvider != null)
		{
			BindStaticEntry(highDifficultySlot, ChallengeEntrySlotId.HighDifficulty);
			RefreshDailyChallengeEntry();
		}
	}

	private void BindStaticEntry(ChallengeEntrySlotView slot, ChallengeEntrySlotId slotId)
	{
		BindEntry(slot, staticProvider.GetState(layoutConfig.Get(slotId)));
	}

	private void RefreshDailyChallengeEntry()
	{
		if (!(layoutConfig == null) && dailyProvider != null)
		{
			BindEntry(dailyChallengeSlot, dailyProvider.GetState(layoutConfig.Get(ChallengeEntrySlotId.DailyChallenge)));
		}
	}

	private void BindEntry(ChallengeEntrySlotView slot, ChallengeEntryRuntimeState state)
	{
		if (slot == null)
		{
			return;
		}
		ChallengeEntryRuntimeState state2 = state;
		ChallengeEntryDestination? challengeEntryDestination = state?.destination;
		if (!challengeEntryDestination.HasValue)
		{
			goto IL_00c7;
		}
		ChallengeEntryDestination valueOrDefault = challengeEntryDestination.GetValueOrDefault();
		UnityAction onClick;
		if (valueOrDefault != ChallengeEntryDestination.Constellation)
		{
			if (valueOrDefault != ChallengeEntryDestination.DailyChallenge || !(host != null) || !(dailyChallengeConfig != null))
			{
				goto IL_00c7;
			}
			onClick = host.OpenDailyChallenge;
		}
		else
		{
			if (!(host != null) || !(state.targetConstellation != null))
			{
				goto IL_00c7;
			}
			onClick = delegate
			{
				host.OpenConstellation(state.targetConstellation);
			};
		}
		goto IL_00c9;
		IL_00c7:
		onClick = null;
		goto IL_00c9;
		IL_00c9:
		slot.Bind(state2, onClick);
	}
}
