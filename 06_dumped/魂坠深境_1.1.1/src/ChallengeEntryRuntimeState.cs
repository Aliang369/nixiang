using UnityEngine;

public sealed class ChallengeEntryRuntimeState
{
	public bool visible;

	public bool unlocked;

	public Sprite artwork;

	public string title;

	public string timeText;

	public ChallengeEntryDestination destination;

	public Constellation targetConstellation;

	public string countdown
	{
		get
		{
			return timeText;
		}
		set
		{
			timeText = value;
		}
	}
}
