using System;
using UnityEngine;

[RequireComponent(typeof(GameButton))]
public class SelectableModule_GameButton : SelectableModuleBase
{
	[NonSerialized]
	private GameButton button;

	public override void Init()
	{
		base.Init();
		button = GetComponent<GameButton>();
		button.onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		selectableArea.OnClicked(this);
		if (!selectableArea.IsSelected(this))
		{
			if (selectableArea.CanSelect(this))
			{
				selectableArea.Select(this);
			}
		}
		else
		{
			selectableArea.Unselect(this);
		}
	}
}
