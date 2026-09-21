using System;
using Common.UI;
using UnityEngine;

public class ForerunnerBasePanel : UIBase
{
	[SerializeField]
	private ForerunnerPanel prefab;

	[NonSerialized]
	private CellUIPrefabPool<ForerunnerPanel, int> pool;

	protected override void Init()
	{
		base.gameObject.SetActive(value: true);
		pool = GetPrefabPool<ForerunnerPanel, int>(prefab);
	}

	public void ShowForeRunnerPanel()
	{
		pool.DespawnAll();
		pool.DestroyInActive();
		ForerunnerPanel forerunnerPanel = pool.Spawn(0);
		UIBase.gameState.StartCoroutine(forerunnerPanel.ShowCor);
	}
}
