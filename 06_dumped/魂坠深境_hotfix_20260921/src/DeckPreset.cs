using System.Collections.Generic;
using System.Linq;

public class DeckPreset
{
	[SaveField(null, null)]
	public List<DeckPresetComp> comps;

	public int SumDiyPoint
	{
		get
		{
			int num = 0;
			foreach (DeckPresetComp allExpandedComp in GetAllExpandedComps())
			{
				num += allExpandedComp.def.DIYPoint;
			}
			return num;
		}
	}

	public int DeckItemCount => GetAllExpandedComps().Count((DeckPresetComp c) => c.def.type == DeckPresetCompType.Item);

	public DeckPreset()
	{
		comps = new List<DeckPresetComp>();
	}

	public List<Item> GetAllItems()
	{
		return (from comp in GetAllExpandedComps()
			where comp.def.type == DeckPresetCompType.Item
			select comp.GenerateItem()).ToList();
	}

	public List<Influence> GetAllInfluences()
	{
		return (from comp in GetAllExpandedComps()
			where comp.def.type == DeckPresetCompType.Influence
			select comp.GenerateInfluence()).ToList();
	}

	public List<DeckPresetComp> GetAllExpandedComps()
	{
		List<DeckPresetComp> list = new List<DeckPresetComp>();
		foreach (DeckPresetComp comp in comps)
		{
			list.Add(comp);
			if (comp.subComps != null)
			{
				list.AddRange(comp.subComps);
			}
		}
		return list;
	}

	public bool IsLegalDeckPreset(PVPEnemyAvatarConfig config)
	{
		if (IsDiyPointLegal(config))
		{
			return IsItemCountLegal(config);
		}
		return false;
	}

	public bool IsDiyPointLegal(PVPEnemyAvatarConfig config)
	{
		return SumDiyPoint <= config.GenerateBaseDefenderProperty(this).diyPoint;
	}

	public bool IsItemCountLegal(PVPEnemyAvatarConfig config)
	{
		return DeckItemCount >= config.GenerateBaseDefenderProperty(this).minimalDeckSize;
	}

	public void RemoveComp(DeckPresetComp comp)
	{
		if (comps.Contains(comp))
		{
			comps.Remove(comp);
			return;
		}
		foreach (DeckPresetComp comp2 in comps)
		{
			if (comp2.subComps != null && comp2.subComps.Contains(comp))
			{
				comp2.subComps.Remove(comp);
				break;
			}
		}
	}

	public DeckPreset Copy()
	{
		DeckPreset deckPreset = new DeckPreset();
		foreach (DeckPresetComp comp in comps)
		{
			deckPreset.comps.Add(comp.Copy());
		}
		return deckPreset;
	}
}
