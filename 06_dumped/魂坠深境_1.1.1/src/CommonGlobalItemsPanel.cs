using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class CommonGlobalItemsPanel : CellUI<List<IGlobalItemDisplaySetup>>
{
	[SerializeField]
	private GlobalItemDisplay prefab;

	[SerializeField]
	private GameButton bachButton;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	protected override void Init()
	{
		pool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(prefab);
		bachButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	protected override void OnSetup(List<IGlobalItemDisplaySetup> setupData)
	{
		base.gameObject.SetActive(value: true);
		CellUI<List<IGlobalItemDisplaySetup>>.fortressState?.uiManager.universalCommonInfoPanel.HideAll();
		pool.Sync(setupData);
	}
}
