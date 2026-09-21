using System.Collections.Generic;

public class PVPEnemyAvatarConfig
{
	public int baseTurnDrawCount;

	public int baseHealth;

	public int basePointLimit;

	public int baseMinimalDeckSize;

	public List<PVPEnemyDeckEditRule> deckEditRules;

	public PVPDefenderProperty GenerateBaseDefenderProperty(DeckPreset preset = null)
	{
		PVPDefenderProperty pVPDefenderProperty = new PVPDefenderProperty
		{
			health = baseHealth,
			diyPoint = basePointLimit,
			turnDrawCount = baseTurnDrawCount,
			minimalDeckSize = baseMinimalDeckSize
		};
		if (preset != null)
		{
			foreach (PVPEnemyDeckEditRule deckEditRule in deckEditRules)
			{
				if (deckEditRule.condition.IsMatch(preset))
				{
					deckEditRule.effect.Apply(pVPDefenderProperty);
				}
			}
		}
		return pVPDefenderProperty;
	}
}
