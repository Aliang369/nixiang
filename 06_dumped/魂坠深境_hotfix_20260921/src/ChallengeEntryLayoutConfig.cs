using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "挑战入口布局配置", menuName = "星图/挑战入口布局配置")]
public sealed class ChallengeEntryLayoutConfig : ScriptableObject
{
	[Serializable]
	public sealed class EntryDefinition
	{
		public ChallengeEntrySlotId slotId;

		public bool visible = true;

		public bool unlocked;

		public Sprite artwork;

		public ChallengeEntryTitleSource titleSource;

		public ChallengeEntryDestination destination;

		public Constellation targetConstellation;
	}

	public List<EntryDefinition> entries = new List<EntryDefinition>();

	public EntryDefinition Get(ChallengeEntrySlotId slotId)
	{
		return entries.FirstOrDefault((EntryDefinition entry) => entry != null && entry.slotId == slotId);
	}
}
