using Common.UI;
using UnityEngine;

public class ConstellationRewardDetailPanel : UIBase
{
	public DiffucltyRewardDisplayCell cell;

	public RectTransform cellParent;

	public GameButton backButton;

	[SerializeField]
	private CellUIPrefabPool<DiffucltyRewardDisplayCell, (DifficultyInfo, ContractDef)> pool;

	protected override void Init()
	{
		pool = GetPrefabPool<DiffucltyRewardDisplayCell, (DifficultyInfo, ContractDef)>(cell, cellParent);
		backButton.onClick.AddListener(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	public void Setup(ContractDef contract)
	{
		pool.DespawnAll();
		foreach (DifficultyInfo validAsset in UIBase.applicationState.assetCatalog.difficultyCatalog.ValidAssets)
		{
			pool.Spawn((validAsset, contract));
		}
		base.gameObject.SetActive(value: true);
		SetDirty();
	}
}
