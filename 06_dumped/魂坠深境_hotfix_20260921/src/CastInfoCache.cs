using System.Collections.Generic;

public class CastInfoCache
{
	public Item spellItem;

	public bool canSpell;

	public bool coolDownCastAble;

	public int baseCost;

	public int finalCost;

	public List<EffectMarkAbility.EffectMarkInfo> effectMarkInfos_Mask;

	public List<EffectMarkNFX> effectMarkInfos_Effect;

	public CastInfoCache(Item spellItem)
	{
		this.spellItem = spellItem;
		canSpell = DisplaySpellCastInfo.CheckSpellCastResolve(Bootstrap.gameState, spellItem, inBattleDisplay: true);
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(Bootstrap.gameState, inBattleDisplay: true)
		{
			spellItem = spellItem
		};
		coolDownCastAble = displaySpellCastInfo.IsFocusSpellCastTimingAble(GameEvent.TriggerType.InBattleDisplay);
		baseCost = spellItem.GetDisplayBaseCost(Bootstrap.applicationState);
		finalCost = spellItem.GetDisplayFinalCost(Bootstrap.applicationState, GameEvent.TriggerType.InBattleDisplay);
		List<EffectMarkAbility.EffectMarkInfo> obj = Bootstrap.gameState.gameEventManager.TriggerEffectMarkResolve(spellItem)?.effectMarkInfos;
		effectMarkInfos_Mask = new List<EffectMarkAbility.EffectMarkInfo>();
		effectMarkInfos_Effect = new List<EffectMarkNFX>();
		foreach (EffectMarkAbility.EffectMarkInfo item in obj)
		{
			if (item.showType.HasFlag(EffectMarkAbility.ShowType.Mark))
			{
				effectMarkInfos_Mask.Add(item);
			}
			if (item.showType.HasFlag(EffectMarkAbility.ShowType.Effect) && item.nfx?.setup != null)
			{
				effectMarkInfos_Effect.Add(item.nfx);
			}
		}
	}
}
