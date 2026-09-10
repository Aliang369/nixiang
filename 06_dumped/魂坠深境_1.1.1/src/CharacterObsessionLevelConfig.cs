using System.Collections.Generic;
using UnityEngine;

public class CharacterObsessionLevelConfig
{
	[SerializeField]
	public Sprite icon;

	[SerializeField]
	[LocalizeText("角色执念名称")]
	public string title;

	[SerializeField]
	[LocalizeText("角色执念故事")]
	public List<string> storyTexts;

	[SerializeField]
	public GameConfigEffect effect;

	public PlayerCharacterState GetAttachedCharacter(ApplicationStateInfo applicationState)
	{
		return applicationState.characterManager.characterStates.Find((PlayerCharacterState s) => s.def.obsessionLevelDefs.Contains(this));
	}

	public string GetEffectDescription(PlayerCharacterState character)
	{
		TextDataGenerator generator;
		return GetEffectDescription(character, out generator);
	}

	public string GetEffectDescription(PlayerCharacterState character, out TextDataGenerator generator)
	{
		generator = TextDataGenerator.Create(Bootstrap.applicationState, character?.def);
		return effect?.GenerateObsessionDisplayDescription(character, this, generator) ?? string.Empty;
	}

	public int GetLevelNum(ApplicationStateInfo applicationState)
	{
		return GetAttachedCharacter(applicationState).def.obsessionLevelDefs.IndexOf(this) + 1;
	}

	public string GetStoryText()
	{
		string text = string.Empty;
		if (storyTexts.IsNullOrEmpty())
		{
			return text;
		}
		for (int i = 0; i < storyTexts.Count; i++)
		{
			if (i != 0)
			{
				text += "\n";
			}
			text += storyTexts[i];
		}
		return text;
	}
}
