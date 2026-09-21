using System;

public class ItemSlotSound : ItemDisplayBase.DisplayModule
{
	public override void Init()
	{
		InputModule itemSlotInputModule = base.slotDisplay.itemSlotInputModule;
		itemSlotInputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(itemSlotInputModule.onHandleInput, new Action<InputModuleBase>(HandleInput));
	}

	private void HandleInput(InputModuleBase inputModule)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (inputModule.pointerDown && UIDisplayModule.gameState != null)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_select);
		}
	}
}
