using System.Collections.Generic;
using DamageNumbersPro;
using UnityEngine;

public class VFXDataPreset : ScriptableObjectSingleton<VFXDataPreset>
{
	public class IntentTypeDisplayInfo
	{
		public Sprite activeSlotIntentImage;

		public Color color;

		public Sprite icon;

		public Sprite inactiveSlotIntentImage;

		public Color inactiveTextColor;

		public IntentType intentType;

		public Color textColor;

		public Sprite textIcon;
	}

	public class QualityDisplayInfo
	{
		[LocalizeText("品阶名字")]
		public string qualityName;

		public Color qualityBaseColor;

		public Sprite skillBackGroundSprite;

		public Color skillBackGroundColor;
	}

	public class TalentTypeDisplayInfo
	{
		public TalentDef.TalentType type;

		[LocalizeText("天赋类型名")]
		public string typeName;

		[LocalizeText("天赋类型描述")]
		public string typeDescription;
	}

	public class MatterGroupDisplayInfo
	{
		public Sprite icon;

		[LocalizeText("素材组名称")]
		public string name;
	}

	public class GemTypeDisplayInfo
	{
		public SkillGemDef.GemType type;

		public Sprite gemSprite;

		public Color color;
	}

	private const string tabConfigString = "VFXTitle/Sub";

	private const string displayConfigGroup = "Display配置参数";

	private const string othersConfigGroup = "其它配置";

	private const string typeDisplayConfigGroup = "类型显示配置";

	private const string UIGroup = "UI参数";

	private const string cameraGroup = "镜头";

	private const string tempGroup = "临时";

	public string VFXTitle;

	public float shootDuration = 0.25f;

	public BezierLineHelper.BeizerType bezierType;

	public BezierLineHelper.BeizerControlDirType controlDirType;

	public BezierLineHelper.BeizerControlLengthType controlLengthType;

	public Vector3 startDirection;

	public Vector3 endDirection;

	public float controlScale;

	public bool controlInvertByScreen;

	public Color damagedVignetteColor;

	public float damagedVignetteIntensity;

	public float damagedVignetteTime;

	public Color defaultVignetteColor;

	public float defaultVignetteIntensity;

	public DamageNumberGUI defaultPopupTextPrefab;

	public DamageNumberGUI healthDamagePopupTextPrefab;

	public DamageNumberGUI playerHealthDamagePopupTextPrefab;

	public DamageNumberGUI enemyHealthDamagePopupTextPrefab;

	public DamageNumberGUI defenceDamagePopupTextPrefab;

	public DamageNumberGUI healPopupTextPrefab;

	public DamageNumberGUI playerHealPopupTextPrefab;

	public DamageNumberGUI enemyHealPopupTextPrefab;

	public DamageNumberGUI tempHealthPopupTextPrefab;

	public DamageNumberGUI changeCounterPopupTextPrefab;

	public DamageNumberGUI enchantPopupTextPrefab;

	public DamageNumberGUI coinPopupTextPrefab;

	public DamageNumberGUI crystalPopupTextPrefab;

	public float subtitleFadeTime;

	public float darkScreenFadeTime;

	public float darkScreenTextFadeTime;

	public float darkScreenTextStayTime;

	public float itemSlotDissolveTime;

	public float itemSlotFlickerTime;

	public float itemDefenceDelayTime;

	public TextEmergeEffect.EffectConfig textEmergeEffect;

	public ItemDisplayBase.PlayerAnimConfig playerItemAnimConfig;

	public ItemDisplayBase.EnemyAnimConfig enemyItemAnimConfig;

	public ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig;

	public ItemInfoDisplayBase.Config itemInfoDisplayConfig;

	public PlayerBftUIDisplay.Config playerBftUIDisplayConfig;

	public DungeonMapDisplay.Config dungeonMapDisplayConfig;

	public UpgradeRoomEventPanel.Config upgradeRoomDisplayConfig;

	public QuickHintPanel.Config battleHintPanelConfig;

	public SkillPanel.Config skillPaneConfig;

	public LoadingPanel.Config loadingPanelConfig;

	public MultiInfoDisplayPanel.MultiInfoPanelDisplayConfig multiInfoPanelDisplayConfig;

	public BattleStateInfo.Config battleStateInfoConfig;

	public float idleDialogueTimeInterval;

	public ChallengeStarMap.StarMapDisplayConfig starMapDisplayConfig;

	public float dialogueBaseReadSpeed;

	public float dialogueBaseWaitTime;

	public float dialogueRollStrength;

	public float dialogueRollSpeed;

	public float dialogueJitterStrength;

	public Color inactiveDialogueAvatarColor;

	public Dictionary<QualityLevel, QualityDisplayInfo> qualityDisplayInfos;

	public Dictionary<TalentDef.TalentType, TalentTypeDisplayInfo> talentTypeDisplayInfos;

	public List<IntentTypeDisplayInfo> intentDisplayInfos;

	public Dictionary<MatterDef.MatterDisplayGroup, MatterGroupDisplayInfo> matterGroupDisplayInfos;

	public Dictionary<SkillGemDef.GemType, GemTypeDisplayInfo> gemTypeDisplayInfos;

	public Color crystalColor = new Color(0.38039216f, 0.8901961f, 50f / 51f);

	public Color coinColor = new Color(84f / 85f, 0.96862745f, 0.3372549f);

	public Color positiveAttachColor = new Color(84f / 85f, 0.96862745f, 0.3372549f);

	public Color negativeAttachColor = new Color(0.5568628f, 0.21960784f, 0.6509804f);

	public Color matterColor_Normal;

	public Color matterColor_Lack;

	public Color difficultyDescriptionHighlightLineColor;

	public Vector3 deathDropPosition1;

	public Vector3 deathDropRotation1;

	public float deathDropTime1;

	public Vector3 deathDropPosition2;

	public Vector3 deathDropRotation2;

	public float deathDropTime2;

	public float deathDropTime3;

	public Color deathRattleFilterColor;

	public float deathRattleFadeTime;

	public float deathRattlePanelFadeTime;

	public ShakeCameraConfig shakeCameraConfig;

	public NFXSetup effect_defaultTransform;

	public NFXSetup effect_defaultHit3D;

	public NFXSetup effect_bigBlock_Exile;

	public NFXSetup effect_bigBlock_Upgrade;

	public NFXSetup effect_collectCoin;

	public NFXSetup effect_collectCrystal;

	public NFXSetup effect_collectGem;

	public NFXSetup effect_drawPileButtonFlicker;

	public NFXSetup effect_UI_ActivateSkill_a;

	public NFXSetup effect_UI_ActivateSkill_b;

	public NFXSetup effect_itemFullFilledCost;

	public NFXSetup effect_multiInfoDisplayPanel_ShowItem;

	public NFXSetup effect_wastePileButtonFlicker;

	public NFXSetup effect_talentUnlock;

	public NFXSetup effect_talent_to_unlockable;

	public NFXSetup effect_itemSpell;

	public NFXSetup effect_heal;

	public NFXSetup effect_heal_UI;

	public NFXSetup effect_exile;

	public NFXSetup effect_discard;

	public NFXSetup effect_permanentExile;

	public NFXSetup effect_slot_addChapter;

	public NFXSetup effect_slot_DefendBreak;

	public NFXSetup effect_slot_Defend;

	public NFXSetup effect_recycleWastePile;

	public NFXSetup effect_enemy_defence;

	public NFXSetup effect_enemy_defence_break;

	public NFXSetup Block_coolDown;

	public NFXSetup getWorldVFX;

	public NFXSetup upgradeSkillVFX;

	public NFXSetup upgradeObsessionLevelVFX;

	public NFXSetup crystalShieldVFX;

	public DisplayUnit.AnimationMoveInfo enemyPullMoveInfo;

	public DisplayUnit.AnimationMoveInfo enemyPushMoveInfo;

	public DisplayUnit.AnimationMoveInfo enemyReturnMoveInfo;

	public TextmeshproEffect.Config textMeshProEffectConfig;

	public Sprite icon_global_item_random_skill;

	public Sprite icon_global_item_random_loot_matter;

	public Sprite icon_global_item_gacha_draw_stone;

	public Sprite icon_global_item_random_skin;

	public Sprite icon_global_item_random_start_game_buff;

	public Sprite icon_global_item_unlock_vip;

	public Sprite icon_global_item_unlock_monthly_card;

	public Sprite icon_global_item_exp_bonus;

	public DisplayUnit.AnimConfig displayUnitAnimConfig;

	public LineLevelCell.Config lineLevelCellConfig;

	public PileItemArea.AnimConfig pileItemAreaAnimConfig;

	public PlayerOverloadDisplay.Config playerOverloadDisplayConfig;

	public NormalEventPanel.AnimConfig normalEventPanelAnimConfig;

	private float passiveSpellTime1
	{
		get
		{
			return playerItemAnimConfig.passiveSpellTime1;
		}
		set
		{
			playerItemAnimConfig.passiveSpellTime1 = value;
		}
	}

	private Vector2 passiveOffset1
	{
		get
		{
			return playerItemAnimConfig.passiveOffset1;
		}
		set
		{
			playerItemAnimConfig.passiveOffset1 = value;
		}
	}

	private Vector2 passiveOffset2
	{
		get
		{
			return playerItemAnimConfig.passiveOffset2;
		}
		set
		{
			playerItemAnimConfig.passiveOffset2 = value;
		}
	}

	private float enemyConfig_showInfoTime
	{
		get
		{
			return enemyItemAnimConfig.showInfoTime;
		}
		set
		{
			enemyItemAnimConfig.showInfoTime = value;
		}
	}

	private float itemInfoDisplayConfig_enemyShowInfoShowTime
	{
		get
		{
			return itemInfoDisplayConfig.enemyShowInfoShowTime;
		}
		set
		{
			itemInfoDisplayConfig.enemyShowInfoShowTime = value;
		}
	}

	private float enemyConfig_enemyShowInfoStayTime
	{
		get
		{
			return itemInfoDisplayConfig.enemyShowInfoStayTime;
		}
		set
		{
			itemInfoDisplayConfig.enemyShowInfoStayTime = value;
		}
	}

	private float enemyConfig_enemyShowInfoHideTime
	{
		get
		{
			return itemInfoDisplayConfig.enemyShowInfoHideTime;
		}
		set
		{
			itemInfoDisplayConfig.enemyShowInfoHideTime = value;
		}
	}

	private Vector3 enemyConfig_showInfoScale
	{
		get
		{
			return itemInfoDisplayConfig.enemyShowInfoScale1;
		}
		set
		{
			itemInfoDisplayConfig.enemyShowInfoScale1 = value;
		}
	}

	private Vector3 enemyConfig_showInfoRotation
	{
		get
		{
			return itemInfoDisplayConfig.enemyShowInfoRotation1;
		}
		set
		{
			itemInfoDisplayConfig.enemyShowInfoRotation1 = value;
		}
	}

	private Vector3 playerItemAnimConfig_idleScale
	{
		get
		{
			return playerItemAnimConfig.idleScale;
		}
		set
		{
			playerItemAnimConfig.idleScale = value;
		}
	}

	private Vector3 playerItemAnimConfig_idleRotation
	{
		get
		{
			return playerItemAnimConfig.idleRotation;
		}
		set
		{
			playerItemAnimConfig.idleRotation = value;
		}
	}

	private float playerItemAnimConfig_drawTime
	{
		get
		{
			return playerItemAnimConfig.drawTime;
		}
		set
		{
			playerItemAnimConfig.drawTime = value;
		}
	}

	private Vector3 playerItemAnimConfig_unDrawScale
	{
		get
		{
			return playerItemAnimConfig.unDrawScale;
		}
		set
		{
			playerItemAnimConfig.unDrawScale = value;
		}
	}

	private Vector3 playerItemAnimConfig_unDrawRotation
	{
		get
		{
			return playerItemAnimConfig.unDrawRotation;
		}
		set
		{
			playerItemAnimConfig.unDrawRotation = value;
		}
	}

	private Vector3 enemyItemAnimConfig_idleRotation
	{
		get
		{
			return enemyItemAnimConfig.idleRotation;
		}
		set
		{
			enemyItemAnimConfig.idleRotation = value;
		}
	}
}
