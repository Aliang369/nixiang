using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class PlayerCharacterDef : CTScriptableObject, IAssetIcon, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	[CTSOAssetName]
	[LocalizeText("角色名字")]
	public string characterName;

	public Sprite fullBody;

	public Sprite halfBody;

	public Sprite halfBodyB;

	public Sprite headIcon;

	public Sprite globalItemIcon;

	public SkeletonGraphic spine;

	[TextArea]
	[LocalizeText("角色介绍")]
	public string description;

	[HideInInspector]
	[SerializeField]
	public string characterDetailDescription;

	[HideInInspector]
	[SerializeField]
	public string characterFeatureDescription;

	[SerializeField]
	public int sortDisplayOrderIndex;

	public List<FeatureInfoData> features;

	public CharacterClassDef attachClass;

	public List<CharacterClassDef> secondaryClasses;

	public List<ItemDefinition> initDeck;

	public List<CharacterMastery> characterMasteries;

	public List<InfluenceDef> initInfluenceDefs = new List<InfluenceDef>();

	public CharacterMastery.MasteryNode characterMasteryBaseNode;

	public TalentDef sharePotential;

	public LevelLineDef levelLineDef;

	[SerializeField]
	public int itemSlotCount;

	[SerializeField]
	public bool retainDefenceItemsOnOverflowConsume;

	[SerializeField]
	public int initDrawCount;

	[SerializeField]
	public int initHealth;

	[SerializeField]
	public int initMaxHealth;

	[SerializeField]
	public int initSkillSlotCount;

	[SerializeField]
	public int initCoin;

	[SerializeField]
	public int initCrystal;

	[SerializeField]
	public int initWillPower;

	public List<SkillDef> characterSkill;

	public List<SkillDef> innateSkill;

	public bool initLock;

	public TalentTree talentTree;

	[SerializeField]
	public MatterDef obsessionDebris;

	[SerializeField]
	public MatterDef obsession;

	[SerializeField]
	[LocalizeText("执念等级配置")]
	public List<CharacterObsessionLevelConfig> obsessionLevelDefs;

	public override string CTAssetIDPrepend => "CHAD";

	public int MaxSkillSlotCount => 5;

	public Sprite IconAsset => fullBody;

	public PlayerCharacterState state => CTScriptableObject.applicationState.characterManager.GetCharacterState(this);

	public PlayerCharacterState GetState(ApplicationStateInfo applicationState)
	{
		return applicationState.characterManager.GetCharacterState(this);
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		return new List<IEffectNode>();
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}
}
