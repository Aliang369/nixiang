using System.Collections.Generic;
using System.Linq;
using GameRandom;

public sealed class DancerCharmRandomPositiveEnchantGenerator : EnchantDefGenerator
{
	public static List<EnchantDef> GetVisiblePositiveEnchantCandidates(GameEvent eve)
	{
		return (eve?.gameState?.applicationState?.assetCatalog?.enchantDefCatalog)?.ValidAssets?.Where((EnchantDef def) => def != null && def.type == EnchantDef.EnchantType.Positive && !def.hideInIndex).ToList() ?? new List<EnchantDef>();
	}

	public override List<EnchantDef> GenerateEnchants(GameEvent eve)
	{
		List<EnchantDef> visiblePositiveEnchantCandidates = GetVisiblePositiveEnchantCandidates(eve);
		if (visiblePositiveEnchantCandidates.Count == 0)
		{
			return new List<EnchantDef>();
		}
		if (eve?.gameState?.randomManager == null || eve.IsDisplayEvent())
		{
			return new List<EnchantDef> { visiblePositiveEnchantCandidates[0] };
		}
		return new List<EnchantDef> { visiblePositiveEnchantCandidates.SelectRandom(eve.gameState.randomManager.NextDouble) };
	}
}
