using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldEventChoice
{
	public string buttonText;

	public string descriptionText;

	public List<Enchant> displayEnchants;

	public List<Rune> displayRunes;

	public List<Influence> displayInfluences;

	public List<Item> displayItems;

	public List<GameSkill> displaySkills;

	public Sprite displayImage;

	public Func<bool> executeAbleChecker;

	public bool stayAfterChoose;

	public Action executeAction;

	public void AddDisplayItem(Item item)
	{
		if (displayItems == null)
		{
			displayItems = new List<Item>();
		}
		displayItems.Add(item);
	}

	public void AddDisplayInfluence(Influence influence)
	{
		if (displayInfluences == null)
		{
			displayInfluences = new List<Influence>();
		}
		displayInfluences.Add(influence);
	}

	public void AddDisplayEnchant(Enchant enchant)
	{
		if (displayEnchants == null)
		{
			displayEnchants = new List<Enchant>();
		}
		displayEnchants.Add(enchant);
	}

	public void AddDisplayRune(Rune rune)
	{
		if (displayRunes == null)
		{
			displayRunes = new List<Rune>();
		}
		displayRunes.Add(rune);
	}

	public void AddDisplaySkill(GameSkill skill)
	{
		if (displaySkills == null)
		{
			displaySkills = new List<GameSkill>();
		}
		displaySkills.Add(skill);
	}
}
