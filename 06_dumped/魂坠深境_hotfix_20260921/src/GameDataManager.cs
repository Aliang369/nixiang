using System.Collections.Generic;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameDataManager : ScriptableObjectSingleton<GameDataManager>
{
	public ItemTag companionTag;

	public ItemTag defenceCompanionTag;

	public ItemTag healthTag;

	public ItemTag curseTag;

	public ItemTag oneOffTag;

	public ItemTag attackTag;

	public ItemTag defendTag;

	public ItemTag ChapterTag;

	public ItemTag enemyAttackIntentTag;

	public CutsceneRef gachaCutSceneRef;

	public EnchantDef spawnItemEnchantDef;

	public ClavisDef lockStargazingRoom;

	public ClavisDef lockIdolRoom;

	public ClavisDef lockLibraryRoom;

	public ClavisDef lockForgetShrine;

	public ItemSkinBaseDef defaultItemSkin;

	public Sprite emptySkillWordSlotIcon;

	public Sprite crystalBonusIcon;

	public RewardUncollectedTag rewardUncollectedTagPrefab;

	public Sprite[] gemLevelIcons;

	public ItemDefinition finalEndingKeyItem;

	public RoomObjectDisplay finalEndingHealFireNPC;

	public Sprite generalClassIcon;

	[FormerlySerializedAs("DecreaseIcon")]
	public Sprite decreaseTextIcon;

	[FormerlySerializedAs("IncreaseIcon")]
	public Sprite increaseTextIcon;

	public Sprite viewTextIcon;

	public Sprite yangTextIcon;

	public Sprite yinTextIcon;

	[FormerlySerializedAs("coinIcon")]
	public Sprite coinTextIcon;

	public Sprite crystalInTextIcon;

	public Sprite healthInTextIcon;

	public Sprite defenceInTextIcon;

	public Sprite itemPlaceHolderIcon;

	public Sprite skillWordPlaceHolderIcon;

	public Sprite enchantPlaceHolderIcon;

	public Sprite influencePlaceHolderIcon;

	public Sprite skillPlaceHolderIcon;

	public Sprite enemyAvatarPlaceHolderIcon;

	public Sprite tagPlaceHolderIcon;

	public Sprite forgetShrinePointIcon;

	public Sprite currencyIcon;

	public Sprite ticketIcon;

	public TMP_FontAsset[] allFontAssets;

	public int forerunnerLegacyPerGame;

	public AnimationCurve timeScaleCurve;

	public Material fastPostProcessMat;

	public Material itemOutlineMaterial;

	public Material simpleMaterial;

	[FormerlySerializedAs("bakedSpriteShader")]
	public Shader bakedSpriteShader_transparent;

	public Shader bakedSpriteShader_clip;

	public Shader noiseFogShader;

	public CrystalUpgradeWorldEvent.CrystalUpgradeConfig crystalUpgradeConfig;

	public MintageRuneStoneEvent.MintageRuneRoomConfig mintageRuneRoomConfig;

	public StoreEvent.StoreConfig storeConfig;

	public UpgradeRoomEvent.UpgradeRoomConfig upgradeConfig;

	public StartGameBuffConfigPanelConfig startGameBuffConfigPanelConfig;

	public List<Vector2Int> bossInfluenceWillPowerRange;

	public int highDifficultyUnlockSkillCrystalCost;

	public int stanceBaseMaxValue;

	public Dictionary<ItemTag, StanceDef> stanceTagsBindings;

	public Sprite attackGemSprite;

	public Sprite defenceGemSprite;

	public Sprite economyGemSprite;

	public Sprite healGemSprite;

	public Sprite abilityGemSprite;

	public Sprite[] gemMasks;

	public ClavisDef finalBossFixGenerateBeastFixClavis;

	public ClavisDef finalBossRandomGenerateClaivs;

	public EnemyAssetRef knightfinalBoss;

	public EnemyAssetRef beastfinalBoss;

	public TextAnimatorSettings textAnimatorSettings;

	public Color GetQualityBaseColor(QualityLevel level)
	{
		if (ScriptableObjectSingleton<VFXDataPreset>.instance.qualityDisplayInfos.ContainsKey(level))
		{
			return ScriptableObjectSingleton<VFXDataPreset>.instance.qualityDisplayInfos[level].qualityBaseColor;
		}
		return Color.white;
	}

	public Sprite GetCharacterClassSoulTextIcon(CharacterClassType classType)
	{
		return (Bootstrap.applicationState?.assetCatalog?.characterClassDefCatalog?.ValidAssets?.Find((CharacterClassDef c) => c.mClass == classType))?.soulMatter?.icon;
	}
}
