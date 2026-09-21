using System;
using System.Linq;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class EnemyIndexPanel : IndexPagePanel
{
	[SerializeField]
	private EnemyIndexBookCell enemyCellPrefab;

	[SerializeField]
	private Transform enemyCellParent;

	[SerializeField]
	private EnemyInfoDisplay enemyInfoDisplay;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	private CellUIPrefabPool<EnemyIndexBookCell, EnemyInfoData> enemyCellPool;

	[NonSerialized]
	private EnemyIndexBookCell selected;

	public override string TASubPanelTagName => "档案馆-图鉴-敌人";

	public override IndexBookPage GetIndexPageType()
	{
		return IndexBookPage.Enemy;
	}

	public override void ClearTransientSelection()
	{
		selected = null;
		HideEnemyInfoDisplay();
	}

	public override bool TryFocusFirstClaimableSingleReward()
	{
		return TryFocusFirstClaimableSingleReward(layoutGroup, (object data) => data as EnemyInfoData);
	}

	protected override void Init()
	{
		InitUI(enemyInfoDisplay);
		enemyCellPool = GetPrefabPool<EnemyIndexBookCell, EnemyInfoData>(enemyCellPrefab, enemyCellParent);
	}

	protected override void UpdateDisplay()
	{
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(GetIndexPageAllContent<EnemyInfoData>().ToList(), enemyCellPool, layoutGroup);
		layoutGroup.RefreshLayout();
		if (selected == null)
		{
			HideEnemyInfoDisplay();
		}
		else
		{
			ShowEnemyInfoDisplay();
		}
	}

	public void ShowEnemyInfoDisplay()
	{
		if (enemyInfoDisplay.enemy != selected.enemy)
		{
			enemyInfoDisplay.gameObject.SetActive(value: true);
			enemyInfoDisplay.Setup(selected.enemy);
			SetDirty();
		}
	}

	public void HideEnemyInfoDisplay()
	{
		if (enemyInfoDisplay.enemy != null)
		{
			enemyInfoDisplay.Unset();
			enemyInfoDisplay.gameObject.SetActive(value: false);
			SetDirty();
		}
	}

	public void Select(EnemyIndexBookCell cell)
	{
		if (!(selected == cell))
		{
			selected = cell;
			SetDirty();
		}
	}

	public void UnSelect()
	{
		if (!(selected == null))
		{
			selected = null;
			SetDirty();
		}
	}
}
