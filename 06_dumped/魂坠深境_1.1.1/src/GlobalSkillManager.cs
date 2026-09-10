using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using UnityEngine;

public class GlobalSkillManager : GlobalGameManager, IFrameUpdate
{
	[SaveField(null, null)]
	public List<ProfileSkill> allSkills;

	[SaveField(null, null)]
	public List<SkillWordDef> pickUpgradableSkillWords;

	[SaveField(null, null)]
	public List<SkillDef> unlockedSkills;

	private const float favoriteSkillDirtyUpdateInterval = 10f;

	public bool FavoriteSkillDirty { get; set; }

	private float favoriteSkillDirtySetTimer { get; set; }

	public ProfileSkillPreset currentPreset => base.applicationState.newGameConfigManager?.currentNewGameConfig?.GetCharacterState(base.applicationState).skillPreset;

	public List<ProfileSkill> SortedSkills
	{
		get
		{
			List<ProfileSkill> list = new List<ProfileSkill>(allSkills);
			list.Sort(delegate(ProfileSkill a, ProfileSkill b)
			{
				int num = b.QualityLevel.CompareTo(a.QualityLevel);
				if (num != 0)
				{
					return num;
				}
				int num2 = string.Compare(a.Name, b.Name, StringComparison.Ordinal);
				if (num2 != 0)
				{
					return num2;
				}
				int num3 = a.archetypeIndex.CompareTo(b.archetypeIndex);
				return (num3 != 0) ? num3 : a.profileSkillGUID.CompareTo(b.profileSkillGUID);
			});
			return list;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (unlockedSkills == null)
		{
			unlockedSkills = new List<SkillDef>();
		}
		if (allSkills == null)
		{
			allSkills = new List<ProfileSkill>();
		}
		if (pickUpgradableSkillWords == null)
		{
			pickUpgradableSkillWords = new List<SkillWordDef>();
		}
		ValidateAllSkills();
	}

	public void AddProfileSkill(ProfileSkill ps)
	{
		TryGenerateInitialHexShape(ps);
		DiscoverSkillQualityIndexEntry(ps);
		unlockedSkills.AddUnique(ps.def);
		allSkills.Add(ps);
		base.applicationState.globalUnlockManager.RecordGotObject(ps.def);
		ValidateAllSkills();
		base.applicationState.eventManager.drawSkillEvent.TriggerEvent(ps);
	}

	private void TryGenerateInitialHexShape(ProfileSkill ps)
	{
		if (!GlobalConfigManager.instance.enableNetwork && ps != null && ps.hexShape == null && ps.QualityLevel != QualityLevel.None)
		{
			int randomSlotCount = base.applicationState.skillGemManager.GetRandomSlotCount(0);
			if (randomSlotCount <= 0)
			{
				TraceLog.Error($"GlobalSkillManager: generate initial hex shape failed, slot count is {randomSlotCount}!");
			}
			else
			{
				ps.hexShape = HexagonHelper.GenerateRandomSpreadHexShape(randomSlotCount, HexagonHelper.HexShapeMaxRadius);
			}
		}
	}

	public ProfileSkill AddSkillDef(SkillDef skillDef, int archetypeIndex, int level)
	{
		ProfileSkill profileSkill = new ProfileSkill(skillDef, archetypeIndex);
		AddProfileSkill(profileSkill);
		return profileSkill;
	}

	public void DeleteSkills(IEnumerable<ProfileSkill> skills, out GlobalRewardPackage outRewardPack)
	{
		List<MatterInfo> list = new List<MatterInfo>();
		foreach (ProfileSkill skill in skills)
		{
			allSkills.Remove(skill);
			list.AddRange(skill.GetDeleteSkillGainMatter());
			base.applicationState.eventManager.deleteSkillEvent.TriggerEvent(skill);
		}
		list = MatterInfo.FoldMatterInfo(list);
		foreach (MatterInfo item in list)
		{
			base.applicationState.matterManager.AddMatter(item, "分解护符");
		}
		outRewardPack = new GlobalRewardPackage(list.ConvertAll((MatterInfo m) => new GlobalRewardInfo
		{
			matter = m
		}));
		ValidateAllSkills();
	}

	public void DeleteSkills(RecycleSkillResult result, IEnumerable<ProfileSkill> skills, out GlobalRewardPackage outRewardPack)
	{
		foreach (ProfileSkill skill in skills)
		{
			base.applicationState.globalSkillManager.allSkills.Remove(skill);
		}
		List<MatterInfo> source = result.GainMatters.Select((ChillyRoom.Services.ContainerOfSoul.MatterInfo m) => m.ToClient(new NetworkConvertContext(base.applicationState))).ToList();
		source = MatterInfo.FoldMatterInfo(source);
		foreach (MatterInfo item in source)
		{
			base.applicationState.matterManager.AddMatter(item, "分解护符");
		}
		outRewardPack = new GlobalRewardPackage(source.ConvertAll((MatterInfo m) => new GlobalRewardInfo
		{
			matter = m
		}));
		ValidateAllSkills();
	}

	public void MarkSkillWordAsFixAttachable(SkillWordDef word)
	{
		if (!pickUpgradableSkillWords.Contains(word))
		{
			pickUpgradableSkillWords.Add(word);
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public void TestUnlockAll()
	{
		foreach (SkillDef item in base.applicationState.assetCatalog.gachaPoolCatalog.ValidAssets.SelectMany((GachaPoolDef g) => g.baseGachaWeightContents.Keys).OfType<SkillDef>().Distinct())
		{
			foreach (var item2 in item.AllSkillArchetypeTuple)
			{
				AddProfileSkill(new ProfileSkill(item2.Item1, item2.Item2, 0));
			}
		}
		SetAllSkillInspected();
		foreach (SkillWordDef validAsset in base.applicationState.assetCatalog.skillWordCatalog.ValidAssets)
		{
			pickUpgradableSkillWords.Add(validAsset);
		}
		ValidateAllSkills();
	}

	public void Breakthrough(ProfileSkill skill, ProfileSkill skill1, ProfileSkill skill2, Action<ProfileSkill> onComplete = null)
	{
		if (skill == null || !skill.CanBreakthrough() || skill1 == null || skill2 == null)
		{
			return;
		}
		MatterInfo breakthroughCost = GetBreakthroughCost(skill);
		if ((breakthroughCost != null && !base.applicationState.matterManager.HasEnoughMatter(breakthroughCost)) || !GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		GeneralGameAsyncOperation<BreakthroughSkillRequest, BreakthroughSkillResult> op = new GeneralGameAsyncOperation<BreakthroughSkillRequest, BreakthroughSkillResult>(new BreakthroughSkillRequest
		{
			BaseSkillGuid = skill.profileSkillGUID,
			Skill1Guid = skill1.profileSkillGUID,
			Skill2Guid = skill2.profileSkillGUID
		}, base.applicationState.ChillyRoomSDK.Skill.BreakthroughSkillAsync);
		op.OnComplete(delegate
		{
			List<SkillGem> droppedGems = op.OpResult.Gems.Select((string s) => new SkillGem(base.applicationState.assetCatalog.FullIdToCTAsset<SkillGemDef>(s))).ToList();
			OnComplete(op.OpResult.MatterInfo?.ToClient(new NetworkConvertContext(base.applicationState)), droppedGems);
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_breaking_through_skill);
		void OnComplete(MatterInfo matterInfo, List<SkillGem> droppedGems)
		{
			skill.Breakthrough();
			DiscoverSkillQualityIndexEntry(skill);
			allSkills.Remove(skill1);
			allSkills.Remove(skill2);
			foreach (SkillGem droppedGem in droppedGems)
			{
				base.applicationState.skillGemManager.AddGem(droppedGem);
			}
			if (matterInfo != null)
			{
				base.applicationState.matterManager.ConsumeMatter(matterInfo, "突破护符");
			}
			base.applicationState.savingManager.SetProfileDirtyToSave();
			ValidateAllSkills();
			if (base.fortressState.uiManager != null)
			{
				base.fortressState.uiManager.SetDirty();
				base.applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(new GlobalRewardPackage(new GlobalRewardInfo
				{
					skill = skill
				}));
			}
			base.applicationState.eventManager.breakThroughSkillEvent.TriggerEvent(skill);
			ThinkingAnalyticsExtends.TrackFortressSkillBreakthrough(base.applicationState, skill);
			onComplete?.Invoke(skill);
		}
	}

	public MatterInfo GetBreakthroughCost(ProfileSkill skill)
	{
		MatterDef attachLootMatter = skill.def.attachLootMatter;
		if (attachLootMatter == null)
		{
			return null;
		}
		return new MatterInfo(attachLootMatter, GetCount(skill.QualityLevel));
		static int GetCount(QualityLevel qualityLevel)
		{
			if (!ScriptableObjectSingleton<FortressDataManager>.instance.skillBreakthroughMatterCount.ContainsKey(qualityLevel))
			{
				return ScriptableObjectSingleton<FortressDataManager>.instance.skillBreakthroughMatterCount[QualityLevel.Base];
			}
			return ScriptableObjectSingleton<FortressDataManager>.instance.skillBreakthroughMatterCount[qualityLevel];
		}
	}

	public void ValidateAllSkills()
	{
		bool flag = false;
		foreach (PlayerCharacterState characterState in base.applicationState.characterManager.characterStates)
		{
			List<ProfileSkillPreset> skillPresets = characterState.skillPresets;
			foreach (ProfileSkillPreset item3 in skillPresets)
			{
				foreach (PresetSkillSlot slot in item3.configSkillSlots)
				{
					if (slot.skill != null && allSkills?.Find((ProfileSkill p) => p == slot?.skill) == null)
					{
						slot.skill = null;
					}
				}
				foreach (var item4 in item3.NormalizeDuplicateSkillDefinitions())
				{
					PresetSkillSlot item = item4.slot;
					ProfileSkill item2 = item4.skill;
					flag = true;
					TraceLog.Warn($"[GlobalSkillManager] Remove duplicate skill from preset. character={characterState.def?.id}, preset={item3.index}, slot={item.index}, skillDef={item2.def?.id}, profileSkill={item2.profileSkillGUID}");
				}
			}
			if (!skillPresets.Contains(characterState.skillPreset))
			{
				characterState.skillPreset = skillPresets[0];
			}
		}
		if (flag)
		{
			base.applicationState.savingManager?.SetProfileDirtyToSave();
		}
	}

	public void SetAllSkillInspected()
	{
		foreach (ProfileSkill allSkill in allSkills)
		{
			allSkill.inspected = true;
		}
		base.applicationState.savingManager.SetProfileDirtyToSave();
	}

	private string SkillsToString(List<ProfileSkill> skills)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ProfileSkill skill in skills)
		{
			stringBuilder.Append(skill.Name + " ");
		}
		return stringBuilder.ToString();
	}

	public bool IsNew(SkillDef skillDef)
	{
		return !unlockedSkills.Contains(skillDef);
	}

	private void DiscoverSkillQualityIndexEntry(ProfileSkill skill)
	{
		SkillQualityIndexEntry skillQualityIndexEntry = base.applicationState.assetCatalog.GetSkillQualityIndexEntry(skill?.def, skill?.archetypeIndex ?? (-1));
		base.applicationState.discoverManager.Discover(skillQualityIndexEntry);
	}

	public void FrameUpdate()
	{
		favoriteSkillDirtySetTimer += Time.deltaTime;
		if (!FavoriteSkillDirty || favoriteSkillDirtySetTimer < 10f)
		{
			return;
		}
		favoriteSkillDirtySetTimer = 0f;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			new GeneralGameAsyncOperationRequestOnly<UpdateFavoriteSkillsRequest>(new UpdateFavoriteSkillsRequest(base.applicationState.globalSkillManager.allSkills), base.applicationState.ChillyRoomSDK.Skill.UpdateFavoriteSkillsAsync).OnComplete(delegate
			{
				FavoriteSkillDirty = false;
			}).EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_favorite_skills);
		}
	}
}
