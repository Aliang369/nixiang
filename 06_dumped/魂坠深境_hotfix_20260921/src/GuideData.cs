using System.Collections.Generic;
using UnityEngine;

public class GuideData : ScriptableObjectSingleton<GuideData>
{
	[SerializeField]
	public EnemyAssetRef introEnemy;

	public EnemyAssetRef activeIntroEnemy;

	[SerializeField]
	public EnemyAssetRef finalEnemy;

	[SerializeField]
	public CharacterAIComponent.EnemySpawnSetup finalEnemySetup;

	[SerializeField]
	public Level introLevel;

	[SerializeField]
	public DraftMapRef afterIntroFirstDraftMapRef;

	[SerializeField]
	public DraftMapRef afterIntroSecondDraftMapRef;

	[SerializeField]
	public GuideModule_Intro introGuide;

	[SerializeField]
	public ItemDefinition swordItemDef;

	[SerializeField]
	public ItemDefinition fistItemDef;

	[SerializeField]
	public ItemDefinition armorItemDef;

	[SerializeField]
	public ItemDefinition energyItemDef;

	[SerializeField]
	public StoryPoint afterFirstGameStoryPoint;

	[SerializeField]
	public DraftMapRef secondFormalGameFirstStageDraftMapRef;

	[SerializeField]
	public StoryPoint afterFirstGameTalentStoryPoint;

	[SerializeField]
	public ItemDefinition silentCrownItemDef;

	[SerializeField]
	public ItemSkinDef silentCrownCybSkinDef;

	[SerializeField]
	public List<LocalizeString> loadingHintTextA1;

	[SerializeField]
	public List<LocalizeString> loadingHintTextA2;

	[SerializeField]
	public List<LocalizeString> loadingHintTextA3;

	[SerializeField]
	public List<LocalizeString> loadingHintTextB;

	public List<ItemDefinition> guideLoots;

	public List<EnemyAssetRef> guideEnemies;

	public List<ItemDefinition> guideWarriorCharacterItems;

	public List<InfluenceDef> guideBless;

	public List<RuneDef> guideRunes;

	[SerializeField]
	public List<GuideModule> guideModules;

	[SerializeField]
	public SkillDef initForerunnerSkill;

	public int initForerunnerStage;

	public Level initForerunnerLevel;

	[TextArea]
	[LocalizeText("初始先行者信件文本")]
	public string initForerunnerNote;

	public T GetGuideModule<T>() where T : GuideModule
	{
		foreach (GuideModule guideModule in guideModules)
		{
			if (guideModule is T result)
			{
				return result;
			}
		}
		return null;
	}
}
