using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AchievementDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public enum AchievementLayer
	{
		Intro,
		Progress,
		Special,
		CharacterClass
	}

	public enum AchievementType
	{
		GlobalAchievement,
		GameAchievement
	}

	[CTSOAssetName]
	[LocalizeText("成就名称")]
	public string achievementName;

	[LocalizeText("成就文本")]
	public string rawDescription;

	public AchievementType type;

	public AchievementLayer layer;

	public Sprite icon;

	public int expBonus;

	public List<Ability> abilities;

	[FormerlySerializedAs("achievementObserver")]
	public GlobalTriggerObserver globalTriggerObserver;

	public bool triggerableInIntro;

	public CharacterClassDef attachCharacterClass;

	public bool uploadImmediatelyOnAchieve;

	public List<GlobalRewardSetup> rewards;

	public override string CTAssetIDPrepend => "ACH";

	public List<GlobalRewardSetup> AchieveRewards => rewards;

	public string GetDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatAchievementDescription(applicationState, this, out generator);
	}

	NodeDisplayInfo ITextDisplay.GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (abilities != null)
		{
			foreach (Ability ability in abilities)
			{
				list.Add(ability);
			}
		}
		if (globalTriggerObserver != null)
		{
			list.Add(globalTriggerObserver);
		}
		return list;
	}
}
