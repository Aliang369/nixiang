using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class PlayerCharacterState : IValidate
{
	[SaveField(null, null)]
	public PlayerCharacterDef def;

	[SaveField(null, null)]
	public bool locked;

	[SaveField(null, null)]
	public CharacterMastery currentMastery;

	[SaveField(null, null)]
	public List<ProfileSkillPreset> skillPresets;

	[SaveField(null, null)]
	public List<TalentDef> unlockedTalents;

	[SaveField(null, null)]
	public ProfileSkillPreset skillPreset;

	[SaveField(null, null)]
	public int obsessionLevel;

	public bool isPreview;

	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public IEnumerable<CharacterMastery> UnlockedMasteries
	{
		get
		{
			if (AllCharacterGameConfigEffects.OfType<UnlockAllMastery>().Any())
			{
				return def.characterMasteries;
			}
			return from c in AllCharacterGameConfigEffects.OfType<UnlockCharacterMastery>()
				select c.mastery;
		}
	}

	public bool EnableSharePotential => AllCharacterGameConfigEffects.OfType<MarkSharePotential>().Any();

	public MatterInfo obsessionInfo => applicationState.matterManager.GetMatterInfo(def.obsession) ?? new MatterInfo(def.obsession, 0);

	public MatterInfo obsessionDebrisInfo => applicationState.matterManager.GetMatterInfo(def.obsessionDebris) ?? new MatterInfo(def.obsessionDebris, 0);

	public MatterInfo classObsessionInfo => applicationState.matterManager.GetMatterInfo(def.attachClass.classObsessionMatter) ?? new MatterInfo(def.attachClass.classObsessionMatter, 0);

	public int obsessionCount => obsessionInfo?.count ?? 0;

	public int obsessionDebrisCount => obsessionDebrisInfo?.count ?? 0;

	public int classObsessionCount => classObsessionInfo?.count ?? 0;

	public int obsessionDebrisUnlockRequiredCount => ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUnlockCount;

	public int obsessionDebrisUpgradeRequiredCount => ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUpgradeCount;

	public int currentObsessionDebrisRequiredCount
	{
		get
		{
			if (!locked)
			{
				return obsessionDebrisUpgradeRequiredCount;
			}
			return obsessionDebrisUnlockRequiredCount;
		}
	}

	public int CurrentSkillPresetIndex => skillPresets.IndexOf(skillPreset);

	public string Name => def.characterName;

	public string Description => def.description;

	public Sprite Avatar => def.fullBody;

	public CharacterClassState characterClass => applicationState.characterManager.characterClassStates.Find((CharacterClassState c) => c.classDef == def.attachClass);

	public List<CharacterObsessionLevelConfig> ActiveCharacterObsessionLevels => def.obsessionLevelDefs.GetRange(0, Mathf.Min(def.obsessionLevelDefs.Count, obsessionLevel));

	public List<GameConfigEffect> AllCharacterGameConfigEffects => ActiveCharacterObsessionLevels.Select((CharacterObsessionLevelConfig o) => o.effect).Concat(unlockedTalents.Select((TalentDef t) => t?.node)).WhereNotNull()
		.ToList();

	public int CharacterTalentInnateSkillUpgradeCount => unlockedTalents.Where((TalentDef talent) => talent != null).SelectMany((TalentDef talent) => talent.GetChildren()).OfType<UpgradeCharacterSkill>()
		.Count();

	public bool CharacterInnateSkillUnlocked => unlockedTalents.Where((TalentDef talent) => talent != null).SelectMany((TalentDef talent) => talent.GetChildren()).OfType<ActiveCharacterSkill>()
		.Any();

	public bool CharacterInnateSkillInitialActivateUnlocked => ActiveCharacterObsessionLevels.Select((CharacterObsessionLevelConfig level) => level.effect).OfType<InitialActivateCharacterInnateSkill>().Any();

	private PlayerCharacterState()
	{
	}

	public PlayerCharacterState(PlayerCharacterDef def)
	{
		this.def = def;
		locked = def.initLock;
		currentMastery = null;
		unlockedTalents = new List<TalentDef>();
		skillPresets = new List<ProfileSkillPreset>();
		for (int i = 0; i < ScriptableObjectSingleton<FortressDataManager>.instance.characterSkillPresetCount; i++)
		{
			skillPresets.Add(new ProfileSkillPreset(i, this));
		}
		skillPreset = skillPresets[0];
	}

	public void Unlock(string unlockMethod)
	{
		locked = false;
		ThinkingAnalyticsExtends.TrackUnlockCharacter(applicationState, def, unlockMethod);
		applicationState.eventManager.unlockCharacterClassEvent.TriggerEvent(def);
		applicationState.matterManager.AddMatter(characterClass.classDef.soulMatter, ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.newCharacterReward, "解锁角色");
	}

	public void SetMastery(CharacterMastery mastery)
	{
		currentMastery = mastery;
	}

	public List<TalentDef> GetActivateAblePotentialTalents()
	{
		List<TalentDef> list = new List<TalentDef>();
		if (unlockedTalents == null)
		{
			return list;
		}
		foreach (TalentDef unlockedTalent in unlockedTalents)
		{
			if (unlockedTalent != null && unlockedTalent.IsPotentialTalent)
			{
				list.Add(unlockedTalent);
			}
		}
		return list;
	}

	public List<TalentDef> GetTalents()
	{
		List<TalentDef> list = new List<TalentDef>();
		TalentSlotCell[] talentSlotCells = def.talentTree.TalentSlotCells;
		foreach (TalentSlotCell talentSlotCell in talentSlotCells)
		{
			list.Add(talentSlotCell.talent);
		}
		return list;
	}

	public List<TalentDef> GetPotentialTalents()
	{
		List<TalentDef> list = new List<TalentDef>();
		if (unlockedTalents == null)
		{
			return list;
		}
		foreach (TalentDef talent in GetTalents())
		{
			if (talent != null && talent.IsPotentialTalent)
			{
				list.Add(talent);
			}
		}
		return list;
	}

	public List<ProfileSkill> GetCharacterCurrentInnateProfileSkills()
	{
		return GetCharacterCurrentInnateProfileSkillsOfUpgradeCount(CharacterTalentInnateSkillUpgradeCount);
	}

	public List<ProfileSkill> GetCharacterCurrentInnateProfileSkillsOfUpgradeCount(int upgradeCount, bool showLocked = false)
	{
		List<ProfileSkill> list = new List<ProfileSkill>();
		if (CharacterInnateSkillUnlocked | showLocked)
		{
			foreach (SkillDef item in def.innateSkill)
			{
				QualityLevel qualityLevel = item.BaseQualityLevel + upgradeCount;
				if (qualityLevel > item.MaxQualityLevel)
				{
					qualityLevel = item.MaxQualityLevel;
				}
				ProfileSkill profileSkill = new ProfileSkill(item, 0);
				profileSkill.SetArchetypeIndex(qualityLevel);
				list.Add(profileSkill);
			}
		}
		return list;
	}

	public int? ObsessionLevelNum(CharacterObsessionLevelConfig config)
	{
		try
		{
			return def.obsessionLevelDefs.IndexOf(config) + 1;
		}
		catch
		{
			return null;
		}
	}

	public bool IsUnlocked(CharacterObsessionLevelConfig config)
	{
		int? num = ObsessionLevelNum(config);
		if (!num.HasValue)
		{
			return false;
		}
		return obsessionLevel >= num;
	}

	public bool CanUnlock(CharacterObsessionLevelConfig config)
	{
		if (IsUnlocked(config))
		{
			return false;
		}
		int? num = ObsessionLevelNum(config);
		if (!num.HasValue)
		{
			return false;
		}
		return num == obsessionLevel + 1;
	}

	public CharacterGameInitConfig GenerateCharacterConfigInit(ApplicationStateInfo applicationState, CharacterMastery mastery = null)
	{
		if (mastery == null)
		{
			mastery = currentMastery;
		}
		CharacterGameInitConfig config = new CharacterGameInitConfig();
		config.characterDef = def;
		config.mastery = mastery;
		config.initDeck = def.initDeck.ConvertAll((ItemDefinition itd) => new Item(itd));
		config.initInfluence = (from ifd in def.initInfluenceDefs.OrEmptyIfNull()
			select new Influence(ifd)).ToList();
		config.initCoin = def.initCoin;
		config.initCrystal = def.initCrystal;
		config.initHealth = def.initHealth;
		config.initWillPower = def.initWillPower;
		config.itemSlotCount = def.itemSlotCount;
		config.turnDrawCount = def.initDrawCount;
		config.unlockedSkillPresetSlotCount = def.initSkillSlotCount;
		config.activateSkillSlotCount = ScriptableObjectSingleton<FortressDataManager>.instance.initCharacterSkillLimit;
		if (def.characterMasteryBaseNode != null)
		{
			def.characterMasteryBaseNode.Modify(new GameEvent(GameEvent.Type.None, GameEvent.TriggerType.Real), config, applicationState);
		}
		if (mastery != null)
		{
			mastery.node.Modify(new GameEvent(GameEvent.Type.None, GameEvent.TriggerType.Real), config, applicationState);
		}
		AllCharacterGameConfigEffects.ForEach(delegate(GameConfigEffect node)
		{
			node.Prepare(config);
		});
		return config;
	}

	public bool IsMasteryUnlocked(CharacterMastery mastery)
	{
		return UnlockedMasteries.Contains(mastery);
	}

	public void UnlockTalent(TalentDef talent, ApplicationStateInfo applicationState)
	{
		if (!unlockedTalents.Contains(talent))
		{
			unlockedTalents.Add(talent);
			applicationState.eventManager.unlockTalentEvent.TriggerEvent(talent);
			applicationState.eventManager.activityMissionTalentUnlockedEvent.TriggerEvent(new YuanQiActivityMissionTalentEventData(this, talent));
		}
	}

	public bool IsTalentUnlocked(TalentDef talent)
	{
		return unlockedTalents.Contains(talent);
	}

	public PlayerCharacterState GetPreview()
	{
		PlayerCharacterState playerCharacterState = new PlayerCharacterState();
		playerCharacterState.def = def;
		playerCharacterState.locked = false;
		playerCharacterState.currentMastery = null;
		playerCharacterState.skillPresets = new List<ProfileSkillPreset>();
		playerCharacterState.unlockedTalents = new List<TalentDef>();
		TalentSlotCell[] talentSlotCells = playerCharacterState.def.talentTree.TalentSlotCells;
		foreach (TalentSlotCell talentSlotCell in talentSlotCells)
		{
			playerCharacterState.unlockedTalents.Add(talentSlotCell.talent);
		}
		playerCharacterState.skillPreset = new ProfileSkillPreset(0, playerCharacterState);
		playerCharacterState.obsessionLevel = 6;
		playerCharacterState.isPreview = true;
		return playerCharacterState;
	}

	public static PlayerCharacterState CreateTrialPreview(PlayerCharacterDef def, CharacterMastery mastery = null)
	{
		PlayerCharacterState playerCharacterState = new PlayerCharacterState();
		playerCharacterState.def = def;
		playerCharacterState.locked = false;
		playerCharacterState.currentMastery = mastery;
		playerCharacterState.skillPresets = new List<ProfileSkillPreset>();
		playerCharacterState.unlockedTalents = new List<TalentDef>();
		TalentSlotCell[] talentSlotCells = def.talentTree.TalentSlotCells;
		foreach (TalentSlotCell talentSlotCell in talentSlotCells)
		{
			playerCharacterState.unlockedTalents.Add(talentSlotCell.talent);
		}
		playerCharacterState.skillPreset = new ProfileSkillPreset(0, playerCharacterState);
		playerCharacterState.obsessionLevel = 6;
		playerCharacterState.isPreview = true;
		return playerCharacterState;
	}

	public static PlayerCharacterState CreateLimitedDeploymentPreview(PlayerCharacterDef def, CharacterMastery mastery, SkillDef fixedSkill)
	{
		if (def == null)
		{
			throw new ArgumentNullException("def");
		}
		if (mastery == null)
		{
			throw new ArgumentNullException("mastery");
		}
		if (fixedSkill == null)
		{
			throw new ArgumentNullException("fixedSkill");
		}
		PlayerCharacterState obj = new PlayerCharacterState(def)
		{
			locked = false,
			currentMastery = mastery,
			obsessionLevel = 0,
			isPreview = true,
			unlockedTalents = def.talentTree.TalentSlotCells.Select((TalentSlotCell cell) => cell.talent).WhereNotNull().ToList()
		};
		int num = fixedSkill.skillArchetypes.FindIndex((SkillDef.SkillArchetype archetype) => archetype.qualityLevel == QualityLevel.Rare);
		if (num < 0)
		{
			throw new InvalidOperationException(fixedSkill.skillName + " 未配置 Rare 品级");
		}
		if (obj.skillPreset.configSkillSlots.IsNullOrEmpty())
		{
			throw new InvalidOperationException(def.characterName + " 未配置护符槽");
		}
		obj.skillPreset.configSkillSlots[0].SetSkill(new ProfileSkill(fixedSkill, num));
		return obj;
	}

	public bool CheckCanUnlockCharacterByObsessionDebris()
	{
		if (!locked)
		{
			return false;
		}
		if (obsessionDebrisCount < obsessionDebrisUnlockRequiredCount)
		{
			return false;
		}
		return true;
	}

	public bool CheckCanUpgrade()
	{
		if (locked || obsessionLevel >= MatterDef.obsessionMaxLevel)
		{
			return false;
		}
		if (obsessionDebrisCount >= obsessionDebrisUpgradeRequiredCount)
		{
			return true;
		}
		if (classObsessionCount >= 1)
		{
			return true;
		}
		return false;
	}

	public (MatterInfo, MatterInfo) GetRemainObsession()
	{
		if (Math.Clamp(obsessionLevel, 0, MatterDef.obsessionMaxLevel) == MatterDef.obsessionMaxLevel)
		{
			return (new MatterInfo(def.obsession, 0), new MatterInfo(def.obsessionDebris, obsessionDebrisInfo.count));
		}
		return (new MatterInfo(def.obsession, 0), new MatterInfo(def.obsessionDebris, 0));
	}

	public void UpgradeCharacter(CharacterObsessionLevelConfig config, MatterInfo matterInfo)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			UpgradeCharacterRequest upgradeCharacterRequest = new UpgradeCharacterRequest();
			upgradeCharacterRequest.CharacterId = def.FullID;
			upgradeCharacterRequest.MatterId = matterInfo.matterDef.FullID;
			new GeneralGameAsyncOperationRequestOnly<UpgradeCharacterRequest>(upgradeCharacterRequest, applicationState.ChillyRoomSDK.Character.UpgradeCharacterAsync).OnComplete(OnComplete).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_upgrading_character);
		}
		void OnComplete()
		{
			TraceLog.TryDebug(() => "[ChillyRoomServiceExtends] UpgradeCharacter.Complete()");
			obsessionLevel++;
			applicationState.eventManager.upgradeCharacterObsessionLevelEvent.TriggerEvent((this, config));
			applicationState.matterManager.ConsumeMatter(matterInfo, "升级执念");
			applicationState.matterManager.AddMatter(characterClass.classDef.soulMatter, 1, "升级执念");
			ThinkingAnalyticsExtends.TrackFortressUpgradeCharacterObsessionLevel(applicationState, this, obsessionLevel, matterInfo);
			if (applicationState.fortressState != null)
			{
				applicationState.fortressState.uiManager.SetDirty();
				applicationState.fortressState.uiManager.characterObsessionPanel.PlayUpgradeVFX(obsessionLevel);
			}
		}
	}

	public bool CanConfigSkill(PresetSkillSlot slot, ProfileSkill skill)
	{
		if (slot == null || skill?.def == null || skillPreset?.configSkillSlots == null)
		{
			return false;
		}
		if (slot.characterInnateSkillSlot)
		{
			return false;
		}
		CharacterClassDef attachCharacterClass = skill.def.attachCharacterClass;
		if (attachCharacterClass != null && attachCharacterClass != def.attachClass && !def.secondaryClasses.OrEmptyIfNull().Contains(attachCharacterClass))
		{
			return false;
		}
		if (!skillPreset.configSkillSlots.Contains(slot))
		{
			return false;
		}
		return !skillPreset.ContainsSkillDefinition(skill.def, slot);
	}

	public IdolState GetIdolState()
	{
		return applicationState.idolManager.GetIdol(def.attachClass.mClass);
	}

	bool IValidate.IsValidate()
	{
		return def != null;
	}
}
