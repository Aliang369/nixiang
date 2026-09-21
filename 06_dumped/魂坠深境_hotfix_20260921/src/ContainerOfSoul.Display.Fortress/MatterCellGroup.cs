using System.Collections.Generic;
using Common.UI;
using ContainerOfSoul.Display.Fortress.MatterCells;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress;

public class MatterCellGroup : UIBase
{
	public MatterCell matterCellPrefab;

	public RectTransform matterCellParent;

	[SerializeField]
	private bool highlightInsufficientMatter;

	[SerializeField]
	private Material normalCounterMaterial;

	[SerializeField]
	private Material insufficientCounterMaterial;

	private CellUIPrefabPool<MatterCell, MatterInfo> matterCellPool;

	private List<MatterInfo> matterInfos;

	protected override void Init()
	{
		matterCellPool = GetPrefabPool<MatterCell, MatterInfo>(matterCellPrefab, matterCellParent);
	}

	protected override void UpdateDisplay()
	{
		matterCellPool.Sync(matterInfos);
		ApplyInsufficientMatterStyle();
	}

	private void ApplyInsufficientMatterStyle()
	{
		if (!highlightInsufficientMatter)
		{
			return;
		}
		foreach (MatterCell activeElement in matterCellPool.GetActiveElements())
		{
			ApplyInsufficientMatterStyle(activeElement);
		}
	}

	private void ApplyInsufficientMatterStyle(MatterCell cell)
	{
		if (highlightInsufficientMatter)
		{
			ContainerOfSoul.Display.Fortress.MatterCells.CounterModule module = cell.GetModule<ContainerOfSoul.Display.Fortress.MatterCells.CounterModule>();
			if (!(module == null) && !(cell.setupData?.matterDef == null))
			{
				Material counterMaterial = (cell.setupData.IsPayable(UIBase.applicationState) ? normalCounterMaterial : insufficientCounterMaterial);
				module.SetCounterMaterial(counterMaterial);
			}
		}
	}

	public void Setup(List<MatterInfo> t)
	{
		matterInfos = t;
		SetDirty();
	}

	public void Unset()
	{
	}
}
