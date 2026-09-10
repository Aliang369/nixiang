using System.Collections.Generic;
using System.Linq;

public sealed class DancerCharmReplaceNegativeEnchantsEffect : Effect
{
	private sealed class Replacement
	{
		public Item target;

		public List<Enchant> negatives;

		public EnchantDef positive;

		public int value;
	}

	public ItemTargetSelector targetSelector;

	public DancerCharmRandomPositiveEnchantGenerator enchantGenerator;

	internal static List<Item> ResolveTargets(GameEvent eve, ItemTargetSelector selector)
	{
		if (eve == null)
		{
			return null;
		}
		List<Item> list = selector?.GetTargetItems(eve);
		if (selector != null)
		{
			return list?.Where((Item item) => item != null).Distinct().ToList();
		}
		List<Item> list2 = new List<Item>();
		if (eve.TargetItem != null)
		{
			list2.Add(eve.TargetItem);
		}
		if (eve.NodeTempItem != null)
		{
			list2.Add(eve.NodeTempItem);
		}
		if (eve.SelectedItems != null)
		{
			list2.AddRange(eve.SelectedItems);
		}
		return list2.Where((Item item) => item != null).Distinct().ToList();
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = ResolveTargets(eve, targetSelector);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		DancerCharmRandomPositiveEnchantGenerator dancerCharmRandomPositiveEnchantGenerator = enchantGenerator ?? new DancerCharmRandomPositiveEnchantGenerator();
		List<Replacement> list2 = new List<Replacement>();
		foreach (Item item in list)
		{
			List<Enchant> list3 = item.Enchants?.Where((Enchant enchant) => enchant != null && enchant.def?.type == EnchantDef.EnchantType.Negative && (int)enchant.value > 0).ToList() ?? new List<Enchant>();
			if (list3.Count != 0)
			{
				EnchantDef enchantDef = dancerCharmRandomPositiveEnchantGenerator.GenerateEnchants(eve)?.FirstOrDefault();
				if (enchantDef == null)
				{
					return NodeCastResult.Failure;
				}
				list2.Add(new Replacement
				{
					target = item,
					negatives = list3,
					positive = enchantDef,
					value = list3.Sum((Enchant enchant) => enchant.value)
				});
			}
		}
		object source = eve.CasterItem ?? eve.CasterSkill ?? eve.Caster;
		foreach (Replacement item2 in list2)
		{
			foreach (Enchant negative in item2.negatives)
			{
				Effect.EnqueueAction(eve, new ChangeEnchantAction(item2.target, source, negative.def, -(int)negative.value, popup: true, eve.spellInfo));
			}
			Effect.EnqueueAction(eve, new ChangeEnchantAction(item2.target, source, item2.positive, item2.value, popup: true, eve.spellInfo));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, enchantGenerator };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Item> list = ResolveTargets(eve, targetSelector);
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo
		{
			refItems = new NodeDisplayInfo.Element<List<Item>>(list),
			times = new NodeDisplayInfo.Element<int?>(list?.Count)
		};
		if (list == null)
		{
			return nodeDisplayInfo;
		}
		DancerCharmRandomPositiveEnchantGenerator dancerCharmRandomPositiveEnchantGenerator = enchantGenerator ?? new DancerCharmRandomPositiveEnchantGenerator();
		List<EnchantDef> list2 = new List<EnchantDef>();
		List<Enchant> list3 = new List<Enchant>();
		foreach (Item item in list)
		{
			List<Enchant> list4 = item.Enchants?.Where((Enchant enchant) => enchant != null && enchant.def?.type == EnchantDef.EnchantType.Negative && (int)enchant.value > 0).ToList() ?? new List<Enchant>();
			if (list4.Count == 0)
			{
				continue;
			}
			EnchantDef enchantDef = dancerCharmRandomPositiveEnchantGenerator.GenerateEnchants(eve)?.FirstOrDefault();
			if (!(enchantDef == null))
			{
				int value = list4.Sum((Enchant enchant) => enchant.value);
				list2.Add(enchantDef);
				list3.Add(new Enchant(enchantDef, value));
			}
		}
		nodeDisplayInfo.refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(list2);
		nodeDisplayInfo.refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(list3);
		nodeDisplayInfo.outputBool = new NodeDisplayInfo.Element<bool?>(list3.Count > 0);
		return nodeDisplayInfo;
	}
}
