using System;
using System.Linq;
using UnityEngine;

public class RuneDisplay_ItemInfo : RuneDisplay.DisplayModule
{
	[SerializeField]
	private GameButton button;

	[NonSerialized]
	private ItemInfoDisplayBase itemInfoDisplay;

	public override void Init()
	{
		button.onClick.AddListener(delegate
		{
			ItemInfoDisplayBase itemInfoDisplayBase = GetComponentsInParent<ItemInfoDisplayBase>(includeInactive: true).FirstOrDefault();
			if (itemInfoDisplayBase != null)
			{
				itemInfoDisplayBase.GetModule<ItemInfoReferenceModule>()?.ToggleSideReference();
			}
		});
	}
}
