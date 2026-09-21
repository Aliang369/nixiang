using System;

public class SoundModule : UIDisplayModule
{
	public override void Init()
	{
		InputModule module = parent.GetModule<InputModule>();
		if (module != null)
		{
			module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, new Action<InputModuleBase>(HandleInput));
		}
	}

	private void HandleInput(InputModuleBase inputModule)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (inputModule.enables.HasFlag(InputModuleBase.EventType.PointerDown))
		{
			if (inputModule.pointerDown)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			}
		}
		else if (inputModule.enables.HasFlag(InputModuleBase.EventType.PointerClick) && inputModule.pointerClicked)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
		}
	}
}
