using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateCharacterPoolItems : NewItemGenerator
{
	public enum CompareType
	{
		Equal,
		Lower,
		Higher
	}

	[SerializeField]
	private ItemTargetSelector raritySelector;

	[SerializeField]
	private bool overrideRarity;

	[SerializeField]
	private QualityLevel rarity;

	[SerializeField]
	private ValueGenerator valueGenerator;

	public CompareType type;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		QualityLevel filterQuality = QualityLevel.None;
		if (eve.gameState?.generationManager == null)
		{
			return null;
		}
		int? num = valueGenerator.GenerateValue(eve);
		int count = 1;
		if (num.HasValue)
		{
			count = num.Value;
		}
		Item item = raritySelector?.GetTargetItems(eve)?.FirstOrDefault();
		if (item != null)
		{
			filterQuality = type switch
			{
				CompareType.Equal => item.qualityLevel, 
				CompareType.Lower => item.qualityLevel - 1, 
				CompareType.Higher => item.qualityLevel + 1, 
				_ => throw new Exception($"type error: {(int)type}"), 
			};
		}
		if (overrideRarity)
		{
			filterQuality = rarity;
		}
		return eve.gameState.generationManager.GenerateCharacterPoolItems(count, eve.gameState.difficultyState.CurrentStage, filterQuality, eve.GetOwnerContainer(GameEvent.EventBFTType.Owner, GameEvent.ContainerType.DrawPileContainer));
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
