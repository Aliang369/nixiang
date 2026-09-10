using System;
using ContainerOfSoul.Display.Fortress.DotManagers;
using UnityEngine;

namespace Script.GameScenes.Fortress.MatterCells;

public class ReddotModule_MatterPanel : MatterCell.DisplayModule
{
	[SerializeField]
	private HintRedDot hintRedDot;

	public override void Init()
	{
		parent.InitUI(hintRedDot);
		InputModule module = parent.GetModule<InputModule>();
		module.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(module.onPointerClick, (Action<InputModuleBase>)delegate
		{
			UIDisplayModule.applicationState.playerInfoManager.newLoots.Remove(base.matterCell.matterInfo.matterDef);
			UIDisplayModule.applicationState.savingManager.SetProfileDirtyToSave();
			UIDisplayModule.fortressState.uiManager.storehousePanel.SetDirty();
		});
	}

	public override void Setup()
	{
		hintRedDot.Setup(new CustomModule
		{
			shouldShow = () => UIDisplayModule.applicationState.playerInfoManager.newLoots.Contains(base.matterCell.matterInfo.matterDef)
		});
	}

	public override void Unset()
	{
		hintRedDot.Unset();
	}
}
