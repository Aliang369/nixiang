using System.Collections.Generic;
using UnityEngine;

public class CheckItemAddedAsCost : CheckNode
{
	[SerializeField]
	private ItemTargetSelector targetSelector;

	public bool byEffectSpellOnly = true;

	public override bool? Check(GameEvent eve)
	{
		SpellCastInfo spellCastInfo = (byEffectSpellOnly ? eve.spellInfo : eve.gameState?.playerState?.playerSpellInfo);
		if (spellCastInfo == null)
		{
			return null;
		}
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return null;
		}
		if (list.Count == 0)
		{
			return false;
		}
		foreach (Item item in list)
		{
			if (!spellCastInfo.addedCosts.Contains(item))
			{
				return false;
			}
		}
		return true;
	}
}
