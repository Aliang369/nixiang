using UnityEngine;

namespace ContainerOfSoul.Display.Fortress;

public class FortressMainBaseUI : UIBase
{
	[SerializeField]
	public CharacterSpineDisplay characterSpineDisplay;

	protected override void Init()
	{
		InitUI(characterSpineDisplay);
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.AddListener(_OnNewGameConfigChange);
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.RemoveListener(_OnNewGameConfigChange);
	}

	protected override void UpdateDisplay()
	{
		characterSpineDisplay.Setup(UIBase.applicationState.newGameConfigManager.currentNewGameConfig.GetCharacterState(UIBase.applicationState));
	}

	private void _OnNewGameConfigChange(NewGameConfigData config)
	{
		SetDirty();
	}
}
