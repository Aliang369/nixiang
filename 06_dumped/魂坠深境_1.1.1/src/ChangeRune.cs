using System.Collections.Generic;

public class ChangeRune : Effect
{
	public RuneGenerator runeGenerator;

	public ItemTargetSelector targetSelector;

	public bool sourceDeckAdd;

	public bool showMultiPanel;

	public bool showChangedItems;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> source = targetSelector?.GetTargetItems(eve);
		List<Rune> source2 = runeGenerator?.GenerateRunes(eve);
		List<Item> list = new List<Item>();
		foreach (Rune item in source2.OrEmptyIfNull())
		{
			foreach (Item item2 in source.OrEmptyIfNull())
			{
				if (item2 != null && !(item?.def == null) && item.value != 0)
				{
					list.Add(item2);
					if (eve.CasterItem != null && item2 != eve.CasterItem)
					{
						Effect.EnqueueAction(eve, new PlayTransformVFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_defaultTransform, eve.CasterItem, item2, is3d: false));
					}
					transformVFXEffect?.Play(eve, eve.CasterPivot, item2);
					Effect.EnqueueAction(eve, new ChangeRuneAction(item2, item.def, item.value, eve.CasterItem, sourceDeckAdd));
					if (showMultiPanel)
					{
						Effect.EnqueueAction(eve, new ShowMultiInfoAction(new MultiInfoDisplayPanel.SetupInfo
						{
							runes = new List<(Rune, Item, int)> { (new Rune(item.def, item.value), item2, item.value) }
						}));
					}
					hitEffect?.Play(eve, item2);
				}
			}
		}
		if (showChangedItems && list.Count > 0)
		{
			Effect.EnqueueAction(eve, new NotifyItemModificationsAction(list));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { runeGenerator, targetSelector };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Rune> value = runeGenerator?.GenerateRunes(eve);
		return new NodeDisplayInfo
		{
			refRunes = new NodeDisplayInfo.Element<List<Rune>>(value)
		};
	}
}
