using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class RuneModule : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private RuneDisplay runePrefab;

	[SerializeField]
	private VerticalLayoutGroup runeParent;

	[NonSerialized]
	private CellUIPrefabPool<RuneDisplay, Rune> runePool;

	public override void Init()
	{
		runePrefab.gameObject.SetActive(value: false);
		runePool = base.mainItemInfoDisplay.GetPrefabPool<RuneDisplay, Rune>(runePrefab, runeParent.transform);
	}

	public override void UpdateDisplay()
	{
		IReadOnlyList<Rune> runes = base.item.Runes;
		if (runes != null)
		{
			runePool.DespawnAll();
			for (int i = 0; i < runes.Count; i++)
			{
				runePool.Spawn(runes[i]);
			}
			runeParent.AutoFitLayoutSpacing();
		}
	}
}
