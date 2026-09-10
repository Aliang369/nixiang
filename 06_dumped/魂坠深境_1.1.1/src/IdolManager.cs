using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class IdolManager : GlobalGameManager
{
	public class SoulExpInfo
	{
		public IdolState idolState;

		public int old;

		public int newExp;

		public int add;

		public int count;

		public int oldSeasonSoul;

		public int newSeasonSoul;

		public MatterInfo matter => new MatterInfo(idolState.soulDef, count);
	}

	[SaveField(null, null)]
	public List<IdolState> idolStates;

	public IdolSystemConfig config => ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig;

	public bool idolSystemUnlocked => ScriptableObjectSingleton<GameDataManager>.instance.lockIdolRoom.isUnlocked;

	public bool forgetShrineUnlocked => ScriptableObjectSingleton<GameDataManager>.instance.lockForgetShrine.isUnlocked;

	public override void Init(ApplicationStateInfo applicationState)
	{
		ValidateIdolState();
	}

	public void ApplyPlayerData(PlayerProfileData profileData, NetworkConvertContext context)
	{
		if (profileData.IdolStates != null)
		{
			base.applicationState.idolManager.idolStates = profileData.IdolStates.Select((ChillyRoom.Services.ContainerOfSoul.IdolState isv) => isv.ToClient(context)).WhereNotNull().ToList();
		}
		ValidateIdolState();
	}

	private void ValidateIdolState()
	{
		if (idolStates == null)
		{
			idolStates = new List<IdolState>();
		}
		idolStates.RemoveAll((IdolState i) => i?.idolDef == null);
		foreach (IdolDef idolDef in base.applicationState.assetCatalog.idolDefCatalog.ValidAssets)
		{
			IdolState idolState = idolStates.Find((IdolState s) => s.idolDef == idolDef);
			if (idolState == null)
			{
				idolState = new IdolState(idolDef);
				idolStates.Add(idolState);
			}
		}
	}

	public IdolState GetIdol(CharacterClassType characterClass)
	{
		return idolStates.Find((IdolState idol) => idol.idolDef.attachClass == characterClass);
	}

	public SoulExpInfo OnExperienceGain(CharacterClassType classType, int experience)
	{
		SoulExpInfo soulExpInfo = new SoulExpInfo();
		IdolState idolState = GetIdolState(classType);
		if (idolState == null)
		{
			return null;
		}
		int experienceToSoulSacrificeRatio = config.experienceToSoulSacrificeRatio;
		int seasonExperience = idolState.seasonExperience;
		int num = seasonExperience + experience;
		soulExpInfo.idolState = idolState;
		soulExpInfo.old = seasonExperience;
		soulExpInfo.oldSeasonSoul = idolState.seasonSoul;
		soulExpInfo.add = experience;
		soulExpInfo.count = 0;
		if (num < experienceToSoulSacrificeRatio)
		{
			idolState.seasonExperience = num;
			soulExpInfo.newExp = idolState.seasonExperience;
			soulExpInfo.newSeasonSoul = idolState.seasonSoul;
			base.applicationState.savingManager.SetProfileDirtyToSave();
			return soulExpInfo;
		}
		int num2 = num / experienceToSoulSacrificeRatio;
		num %= experienceToSoulSacrificeRatio;
		idolState.seasonExperience = num;
		soulExpInfo.count = num2;
		if (num2 > 0)
		{
			if (num2 + idolState.seasonSoul > config.seasonExperienceLimit)
			{
				num2 = config.seasonExperienceLimit - idolState.seasonSoul;
			}
			idolState.seasonSoul += num2;
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
			globalRewardInfo.matter = new MatterInfo(idolState.characterClassDef.soulMatter, num2);
			globalRewardInfo.rewardSourceType = GlobalRewardSourceType.SoulSacrificeReward;
			globalRewardInfo.Give(base.applicationState);
		}
		soulExpInfo.newExp = idolState.seasonExperience;
		soulExpInfo.newSeasonSoul = idolState.seasonSoul;
		base.applicationState.savingManager.SetProfileDirtyToSave();
		return soulExpInfo;
	}

	public GlobalRewardInfo OnDifficultyClear(CharacterClassType classType, int difficultyLevel)
	{
		IdolState idolState = GetIdolState(classType);
		if (idolState == null)
		{
			return null;
		}
		int num = Mathf.Max((int)idolState.currentDifficultyLevel, 0);
		difficultyLevel = Mathf.Max(difficultyLevel, 0);
		if (difficultyLevel <= num)
		{
			return null;
		}
		idolState.currentDifficultyLevel = (DifficultyLevel)difficultyLevel;
		base.applicationState.savingManager.SetProfileDirtyToSave();
		int num2 = 0;
		for (int i = num + 1; i <= difficultyLevel; i++)
		{
			int difficultyReward = config.GetDifficultyReward(i);
			if (difficultyReward > 0)
			{
				num2 += difficultyReward;
			}
		}
		if (num2 > 0)
		{
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
			globalRewardInfo.matter = new MatterInfo(idolState.GetCharacterClassDef().soulMatter, num2);
			globalRewardInfo.rewardSourceType = GlobalRewardSourceType.SoulSacrificeReward;
			globalRewardInfo.Give(base.applicationState);
			return globalRewardInfo;
		}
		return null;
	}

	public void OnObsessionLevelUp((PlayerCharacterState characterState, CharacterObsessionLevelConfig config) data)
	{
		if (data.characterState == null)
		{
			return;
		}
		CharacterClassType mClass = data.characterState.def.attachClass.mClass;
		IdolState idolState = GetIdolState(mClass);
		if (idolState != null)
		{
			int obsessionLevelUpReward = config.obsessionLevelUpReward;
			if (obsessionLevelUpReward > 0)
			{
				GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
				globalRewardInfo.matter = new MatterInfo(idolState.GetCharacterClassDef().soulMatter, obsessionLevelUpReward);
				globalRewardInfo.rewardSourceType = GlobalRewardSourceType.SoulSacrificeReward;
				globalRewardInfo.Give(base.applicationState);
			}
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public void OnNewCharacterObtained(PlayerCharacterDef characterDef)
	{
		if (characterDef == null)
		{
			return;
		}
		CharacterClassType mClass = characterDef.attachClass.mClass;
		IdolState idolState = GetIdolState(mClass);
		if (idolState != null)
		{
			int newCharacterReward = config.newCharacterReward;
			if (newCharacterReward > 0)
			{
				GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
				globalRewardInfo.matter = new MatterInfo(idolState.GetCharacterClassDef().soulMatter, newCharacterReward);
				globalRewardInfo.rewardSourceType = GlobalRewardSourceType.SoulSacrificeReward;
				globalRewardInfo.Give(base.applicationState);
			}
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public IdolState GetIdolState(CharacterClassType classType)
	{
		return idolStates.Find((IdolState idolState) => idolState.idolDef.attachClass == classType);
	}

	public int GetUnofferedSoulSacrifice(CharacterClassType classType)
	{
		return GetIdolState(classType)?.GetUnofferedSoulSacrifice() ?? 0;
	}

	public bool CanOffer(CharacterClassType classType)
	{
		return GetIdolState(classType)?.CanOffer() ?? false;
	}

	public bool IsIdolSystemUnlocked()
	{
		return idolSystemUnlocked;
	}

	public void ClearSeasonExperience()
	{
		foreach (IdolState idolState in base.applicationState.idolManager.idolStates)
		{
			idolState.seasonExperience = 0;
		}
	}

	public void ResetSeasonData()
	{
		foreach (IdolState idolState in base.applicationState.idolManager.idolStates)
		{
			idolState.currentLevel = 0;
			idolState.seasonExperience = 0;
		}
	}

	public bool IsDifficultyCleared(CharacterClassType classType, int difficultyLevel)
	{
		IdolState idolState = GetIdolState(classType);
		if (idolState == null)
		{
			return false;
		}
		return difficultyLevel <= (int)idolState.currentDifficultyLevel;
	}

	public DifficultyLevel GetMainlineDifficulty(CharacterClassType classType)
	{
		return GetIdolState(classType)?.mainlineDifficulty ?? DifficultyLevel.Not;
	}

	public void AdvanceMainlineDifficulty(CharacterClassType classType, DifficultyLevel level)
	{
		IdolState idolState = GetIdolState(classType);
		if (idolState != null && level > idolState.mainlineDifficulty)
		{
			idolState.mainlineDifficulty = level;
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public DifficultyLevel GetMaxSelectableMainlineDifficulty(CharacterClassType classType)
	{
		DifficultyLevel mainlineDifficulty = GetMainlineDifficulty(classType);
		if (mainlineDifficulty >= DifficultyLevel.Normal)
		{
			return mainlineDifficulty + 1;
		}
		return DifficultyLevel.Normal;
	}

	public int GetMaxOfferLevel(CharacterClassType classType)
	{
		return GetIdolState(classType)?.GetMaxOfferLevel() ?? 0;
	}

	public int GetRequiredSoulSacrificeForLevel(CharacterClassType classType, int targetLevel)
	{
		return GetIdolState(classType)?.GetRequiredSoulSacrificeForLevel(targetLevel) ?? 0;
	}

	private List<GlobalRewardInfo> UnlockIdolContents(CharacterClassType classType, int oldLevel, int newLevel)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		IdolState idolState = GetIdolState(classType);
		if (idolState == null)
		{
			return list;
		}
		for (int i = oldLevel + 1; i <= newLevel; i++)
		{
			foreach (GlobalRewardSetup item in idolState.idolDef.GetLevel(i).reward)
			{
				list.Add(item.GenerateReward(base.applicationState, GlobalRewardSourceType.IdolUpgrade, "神像升级"));
			}
		}
		foreach (GlobalRewardInfo item2 in list)
		{
			item2.Give(base.applicationState);
		}
		return list;
	}

	public bool ShouldShowRedDot(CharacterClassType classType)
	{
		if (!idolSystemUnlocked)
		{
			return false;
		}
		IdolState idolState = GetIdolState(classType);
		if (idolState == null)
		{
			return false;
		}
		if (idolState.GetUnofferedSoulSacrifice() > 0)
		{
			return idolState.currentLevel < idolState.idolDef.maxLevel;
		}
		return false;
	}

	private void UpdateRedDotState(CharacterClassType classType)
	{
		string redDotId = $"IdolOffer_{classType}";
		bool show = ShouldShowRedDot(classType);
		SoulSacrificeEvents.RedDotStateChangeEvent.Trigger(redDotId, show);
	}

	public void UpdateAllRedDotStates()
	{
		foreach (IdolState idolState in base.applicationState.idolManager.idolStates)
		{
			UpdateRedDotState(idolState.idolDef.attachClass);
		}
	}

	public List<GlobalRewardInfo> GetUnlockedContentsByLevel(CharacterClassType classType, int level)
	{
		IdolState idolState = GetIdolState(classType);
		if (idolState == null)
		{
			return new List<GlobalRewardInfo>();
		}
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		foreach (GlobalRewardSetup item in idolState.idolDef.GetLevel(level).reward)
		{
			list.Add(item.GenerateReward(base.applicationState, GlobalRewardSourceType.IdolUpgrade, "神像升级"));
		}
		return list;
	}

	public int GetForgetPoints(CharacterClassType classType)
	{
		return GetIdolState(classType)?.GetForgetShrinePoint() ?? 0;
	}

	public void UpdateForgetShrineRedDot(CharacterClassType classType)
	{
		string redDotId = $"ForgetShrine_{classType}";
		IdolState idolState = GetIdolState(classType);
		bool show = forgetShrineUnlocked && idolState != null && idolState.currentLevel > 0;
		SoulSacrificeEvents.RedDotStateChangeEvent.Trigger(redDotId, show);
	}

	public void UpdateAllForgetShrineRedDots()
	{
		foreach (IdolState idolState in base.applicationState.idolManager.idolStates)
		{
			UpdateForgetShrineRedDot(idolState.idolDef.attachClass);
		}
	}
}
