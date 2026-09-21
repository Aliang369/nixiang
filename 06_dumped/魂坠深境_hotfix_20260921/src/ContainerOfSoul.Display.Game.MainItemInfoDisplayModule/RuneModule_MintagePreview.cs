using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class RuneModule_MintagePreview : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private RuneDisplay runePrefab;

	[SerializeField]
	private VerticalLayoutGroup runeParent;

	[NonSerialized]
	private List<Rune> previewRunes;

	[NonSerialized]
	private CellUIPrefabPool<RuneDisplay, Rune> runePool;

	public override void Init()
	{
		runePrefab.gameObject.SetActive(value: false);
		runePool = base.mainItemInfoDisplay.GetPrefabPool<RuneDisplay, Rune>(runePrefab, runeParent.transform);
	}

	public override void Setup()
	{
		SetupRune();
	}

	public override void Unset()
	{
		base.Unset();
		previewRunes = null;
	}

	public void SetPreviewRunes(List<Rune> runes)
	{
		previewRunes = runes;
		SetupRune();
	}

	public void OnMintage()
	{
		foreach (RuneDisplay activeElement in runePool.GetActiveElements())
		{
			activeElement.GetModule<RuneDisplay_MintagePreview>().OnMintage();
		}
	}

	private void SetupRune()
	{
		runePool.DespawnAll();
		if (base.item.Runes == null)
		{
			return;
		}
		foreach (Rune item in base.item.Runes.OrEmptyIfNull())
		{
			runePool.Spawn(item);
		}
		foreach (Rune item2 in previewRunes.OrEmptyIfNull())
		{
			runePool.Spawn(item2).GetModule<RuneDisplay_MintagePreview>().SetAsPreview();
		}
	}
}
