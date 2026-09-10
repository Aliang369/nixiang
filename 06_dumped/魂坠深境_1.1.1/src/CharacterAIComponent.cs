using System;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using GameRandom;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterAIComponent : CTPrefabAsset, IAssetIcon, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	[Serializable]
	public class DiffcultyInfo
	{
		[SaveField(null, null)]
		public List<EnemyEncounterType> allowEncounterTypes;

		[SaveField(null, null)]
		public float healthPercent = 1f;

		[SaveField(null, null)]
		public int intentSlotCount = 1;
	}

	[Serializable]
	public class EnemySpawnSetup
	{
		public DiffcultyInfo difficultyInfo;

		public List<ItemGeneration> itemGenerations;

		public int FlowIntentSlotCount
		{
			get
			{
				int num = itemGenerations.Count((ItemGeneration i) => i.blackBoardValueSetups.FindIndex((ItemGeneration.ItemBlackBoardValueSetup t) => t.type == ItemBlackBoard.ValueType.StayInTable) != -1);
				return difficultyInfo.intentSlotCount - num;
			}
		}
	}

	[HideInInspector]
	[LocalizeText("敌人名字")]
	public string enemyName;

	[LocalizeText("敌人描述")]
	[TextArea(3, 6)]
	public string description;

	public Sprite avater;

	public Sprite thumbnail;

	public bool hideInIndex;

	public bool globalSpawn;

	public EventReference battleBgm;

	public bool isEliteVariant;

	public List<EnemySpawnSetup> setups;

	public List<ItemDefinition> loots;

	public NFXSetup attackNfxSetup;

	public NFXSetup damagedNfxSetup;

	public DifficultyLevel unlockDifficulty = DifficultyLevel.Normal;

	[SerializeField]
	public List<DialogueTrigger> dialogueTriggers;

	public DialoguePlayer dialoguePlayer;

	public CutsceneRef debutCutscene;

	[TextArea]
	[LocalizeText("被动能力")]
	public string abilityDescription;

	[FormerlySerializedAs("abilities")]
	public List<Ability> secondPhaseAbilities;

	public List<Ability> stableAbilities;

	public PVPEnemyAvatarConfig pvpEnemyAvatarConfig;

	private BattleFieldTransformDisplay _bftDisplay;

	public string EnemyName
	{
		get
		{
			return enemyName;
		}
		set
		{
			enemyName = value;
			base.name = enemyName;
		}
	}

	public bool IsPvpEnemy => pvpEnemyAvatarConfig != null;

	public override string CTAssetIDPrepend => "ENEMY";

	public BattleFieldTransformDisplay BftDisplay
	{
		get
		{
			if (_bftDisplay == null)
			{
				_bftDisplay = GetComponent<BattleFieldTransformDisplay>();
			}
			return _bftDisplay;
		}
	}

	public CharacterAI attachAI => BftDisplay.attachBft.characterAI;

	public Sprite IconAsset => thumbnail;

	public DialoguePlayer GetOrAddDialoguePlayer()
	{
		if (dialoguePlayer == null)
		{
			if (BftDisplay.lookPivot != null)
			{
				dialoguePlayer = BftDisplay.lookPivot.gameObject.GetOrAddComponent<DialoguePlayer>();
			}
			else if (BftDisplay.effectUpperPivot != null)
			{
				dialoguePlayer = BftDisplay.effectUpperPivot.transform.gameObject.GetOrAddComponent<DialoguePlayer>();
			}
			else
			{
				dialoguePlayer = BftDisplay.transform.gameObject.GetOrAddComponent<DialoguePlayer>();
			}
		}
		return dialoguePlayer;
	}

	public EnemySpawnSetup GetRandomSetup(GameStateInfo gameState)
	{
		return setups.SelectRandom(gameState.randomManager.NextDouble);
	}

	public EnemyInfoData GetInfoData(ApplicationStateInfo applicationState)
	{
		return applicationState.assetCatalog.enemyInfoDataCatalog.GetInfo(this);
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public new List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (stableAbilities != null)
		{
			list.AddRange(stableAbilities);
		}
		if (secondPhaseAbilities != null)
		{
			list.AddRange(secondPhaseAbilities);
		}
		return list;
	}
}
