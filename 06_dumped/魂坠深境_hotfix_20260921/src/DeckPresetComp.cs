using System.Collections.Generic;

public class DeckPresetComp
{
	[SaveField(null, null)]
	public DeckPresetCompDef def;

	[SaveField(null, null)]
	public List<DeckPresetComp> subComps;

	private DeckPresetComp()
	{
	}

	public DeckPresetComp(DeckPresetCompDef def)
	{
		this.def = def;
		subComps = new List<DeckPresetComp>();
	}

	public Item GenerateItem()
	{
		if (def.item == null)
		{
			return null;
		}
		Item item = new Item(def.item);
		foreach (DeckPresetComp subComp in subComps)
		{
			if (subComp.def.type == DeckPresetCompType.Rune)
			{
				Rune rune = subComp.GenerateRune();
				item.AddRune(rune);
			}
		}
		return item;
	}

	public Rune GenerateRune()
	{
		if (def.rune == null)
		{
			return null;
		}
		Rune rune = new Rune(def.rune);
		if (def.rune.attachGenerationInfo != null)
		{
			rune.value = def.rune.attachGenerationInfo.stack;
		}
		return rune;
	}

	public Influence GenerateInfluence()
	{
		if (def.influence == null)
		{
			return null;
		}
		return new Influence(def.influence);
	}

	public object GenerateTargetObject()
	{
		if (def.item != null)
		{
			return GenerateItem();
		}
		if (def.rune != null)
		{
			return GenerateRune();
		}
		if (def.influence != null)
		{
			return GenerateInfluence();
		}
		return null;
	}

	public DeckPresetComp Copy()
	{
		DeckPresetComp deckPresetComp = new DeckPresetComp(def);
		foreach (DeckPresetComp subComp in subComps)
		{
			deckPresetComp.subComps.Add(subComp.Copy());
		}
		return deckPresetComp;
	}
}
