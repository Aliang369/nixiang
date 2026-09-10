using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class RuneModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private VerticalLayoutGroup runeDisplayParent;

	[SerializeField]
	private RuneDisplay runeDisplayPrefab;

	[NonSerialized]
	private CellUIPrefabPool<RuneDisplay, Rune> runeDisplayPool;

	public override void Init()
	{
		runeDisplayPool = base.slotDisplay.GetPrefabPool<RuneDisplay, Rune>(runeDisplayPrefab, runeDisplayParent.transform);
	}

	public override void Setup()
	{
		runeDisplayPrefab.gameObject.SetActive(value: false);
	}

	public override void UpdateDisplay()
	{
		IReadOnlyList<Rune> runes = base.slotDisplay.item.Runes;
		runeDisplayPool.DespawnAll();
		for (int i = 0; i < runes.Count; i++)
		{
			runeDisplayPool.Spawn(runes[i]);
		}
		runeDisplayParent.AutoFitLayoutSpacing();
	}

	public override void Unset()
	{
		runeDisplayPool.DespawnAll();
	}

	public RuneDisplay GetRuneDisplay(Rune rune)
	{
		foreach (RuneDisplay activeElement in runeDisplayPool.GetActiveElements())
		{
			if (activeElement.rune == rune)
			{
				return activeElement;
			}
		}
		return null;
	}
}
