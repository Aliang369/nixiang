using System.Collections.Generic;
using System.Linq;

public class ChangeEnchant : Effect
{
	public EnchantDefGenerator enchantGenerator;

	public ValueGenerator deltaValueGenerator;

	public ItemTargetSelector targetSelector;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		int num = ((deltaValueGenerator == null) ? 1 : deltaValueGenerator.GenerateValue(eve).Value);
		List<EnchantDef> list2 = ResolveEnchantDefs(eve, enchantGenerator?.GenerateEnchants(eve), num > 0);
		if (list == null || list2 == null)
		{
			return NodeCastResult.Failure;
		}
		object source = eve.CasterItem ?? eve.CasterSkill ?? eve.Caster;
		foreach (Item item in list)
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			foreach (EnchantDef item2 in list2)
			{
				Effect.EnqueueAction(eve, new ChangeEnchantAction(item, source, item2, num, popup: true, eve.spellInfo));
			}
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { enchantGenerator, deltaValueGenerator, targetSelector };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int num = deltaValueGenerator?.GenerateValue(eve) ?? 1;
		List<EnchantDef> list = ResolveEnchantDefs(eve, enchantGenerator?.GenerateEnchants(eve), num > 0);
		int resolveDelta = GetResolveDelta(eve, num, list);
		int enchantValue = num + resolveDelta;
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		nodeDisplayInfo.baseValue = new NodeDisplayInfo.Element<int?>(num);
		nodeDisplayInfo.deltaValue = new NodeDisplayInfo.Element<int?>(resolveDelta);
		nodeDisplayInfo.value = new NodeDisplayInfo.Element<int?>(enchantValue);
		nodeDisplayInfo.refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(list);
		if (list != null && enchantValue >= 0)
		{
			List<Enchant> value = list.ConvertAll((EnchantDef ecd) => new Enchant(ecd, enchantValue));
			nodeDisplayInfo.refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(value);
		}
		return nodeDisplayInfo;
	}

	private int GetResolveDelta(GameEvent eve, int baseEnchantValue, List<EnchantDef> enchantDefs)
	{
		Item item = targetSelector?.GetTargetItems(eve)?.FirstOrDefault();
		EnchantDef enchantDef = enchantDefs?.FirstOrDefault();
		if (item == null || enchantDef == null || eve?.gameState?.gameEventManager == null)
		{
			return 0;
		}
		object source = eve.CasterItem ?? eve.CasterSkill ?? eve.Caster;
		Enchant targetEnchant = new Enchant(enchantDef, baseEnchantValue);
		return eve.gameState.gameEventManager.TriggerChangeEnchantResolve(baseEnchantValue, item, targetEnchant, source, eve.triggerType).deltaValue;
	}

	private static List<EnchantDef> ResolveEnchantDefs(GameEvent eve, List<EnchantDef> enchantDefs, bool shouldReplaceNegative)
	{
		if (enchantDefs == null || !shouldReplaceNegative || eve?.negativeEnchantReplacementGenerator == null)
		{
			return enchantDefs;
		}
		List<EnchantDef> list = new List<EnchantDef>();
		foreach (EnchantDef enchantDef2 in enchantDefs)
		{
			if ((object)enchantDef2 == null || enchantDef2.type != EnchantDef.EnchantType.Negative)
			{
				list.Add(enchantDef2);
				continue;
			}
			EnchantDef enchantDef = eve.negativeEnchantReplacementGenerator.GenerateEnchants(eve)?.FirstOrDefault();
			if (enchantDef != null)
			{
				list.Add(enchantDef);
			}
		}
		return list;
	}
}
