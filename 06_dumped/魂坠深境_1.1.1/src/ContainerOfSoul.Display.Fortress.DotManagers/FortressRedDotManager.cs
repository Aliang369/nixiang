using System.Collections.Generic;

namespace ContainerOfSoul.Display.Fortress.DotManagers;

public class FortressRedDotManager : FortressGameManager
{
	public RedDotModule toCrewButtonModule;

	public Dictionary<PlayerCharacterState, CharacterDisplayModule> characterDisplayModules;

	public Dictionary<PlayerCharacterDef, CharacterInfoPanel_ToTokenDetailButtonModule> characterInfoPanel_ToTokenDetailButtonModules;

	public override void Init(FortressStateInfo fortressState)
	{
		toCrewButtonModule = new RedDotModule();
		characterDisplayModules = new Dictionary<PlayerCharacterState, CharacterDisplayModule>();
		characterInfoPanel_ToTokenDetailButtonModules = new Dictionary<PlayerCharacterDef, CharacterInfoPanel_ToTokenDetailButtonModule>();
		foreach (PlayerCharacterState characterState in base.applicationState.characterManager.characterStates)
		{
			CharacterInfoPanel_ToTokenDetailButtonModule value = new CharacterInfoPanel_ToTokenDetailButtonModule
			{
				playerCharacterState = characterState
			};
			characterInfoPanel_ToTokenDetailButtonModules[characterState.def] = value;
		}
		foreach (PlayerCharacterState characterState2 in base.applicationState.characterManager.characterStates)
		{
			CharacterDisplayModule characterDisplayModule = new CharacterDisplayModule
			{
				playerCharacterState = characterState2
			};
			characterDisplayModule.children.Add(characterInfoPanel_ToTokenDetailButtonModules[characterState2.def]);
			characterDisplayModules[characterState2] = characterDisplayModule;
			toCrewButtonModule.children.Add(characterDisplayModule);
		}
	}
}
