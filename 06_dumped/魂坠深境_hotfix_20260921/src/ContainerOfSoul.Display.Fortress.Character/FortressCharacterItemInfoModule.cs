using System;

namespace ContainerOfSoul.Display.Fortress.Character;

public class FortressCharacterItemInfoModule : ItemDisplayBase.DisplayModule
{
	[NonSerialized]
	private InputModule inputHandle;

	public override void Setup()
	{
		inputHandle = base.slotDisplay.itemSlotInputModule;
		if (inputHandle != null)
		{
			InputModule inputModule = inputHandle;
			inputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(inputModule.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
		}
	}

	public override void Unset()
	{
		if (inputHandle != null)
		{
			InputModule itemSlotInputModule = base.slotDisplay.itemSlotInputModule;
			itemSlotInputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Remove(itemSlotInputModule.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
		}
	}

	public void OnSelect()
	{
		_ = base.slotDisplay.indexHide;
	}

	public void OnUnselect()
	{
	}

	private void _OnHandleInput(InputModuleBase inputModule)
	{
		if (inputModule.pointerClicked)
		{
			OnSelect();
		}
	}
}
