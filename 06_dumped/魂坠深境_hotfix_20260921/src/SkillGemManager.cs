using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;
using GameRandom;
using TMPro;
using UnityEngine;

public class SkillGemManager : GlobalGameManager
{
	[SaveField(null, null)]
	public List<SkillGem> allSkillGems;

	[SaveField(null, null)]
	public ApplicationStateInfo applicationStateInfo;

	private List<SkillGem> deepCopySkillGems = new List<SkillGem>();

	private List<(string, Func<List<SkillGem>, List<SkillGem>>)> _sortMethods;

	public List<(string, Func<List<SkillGem>, List<SkillGem>>)> sortMethods
	{
		get
		{
			if (_sortMethods == null)
			{
				_sortMethods = new List<(string, Func<List<SkillGem>, List<SkillGem>>)>
				{
					(base.textData.term_sort_by_level_decrease, (List<SkillGem> l) => (from s in l
						orderby s.def.level descending, s.def.GemColorType, s.def.skillGemName
						select s).ToList()),
					(base.textData.term_sort_by_level, (List<SkillGem> l) => (from s in l
						orderby s.def.level, s.def.GemColorType, s.def.skillGemName
						select s).ToList()),
					(base.textData.term_sort_by_type, (List<SkillGem> l) => (from s in l
						orderby s.def.GemColorType, s.def.level, s.def.skillGemName
						select s).ToList())
				};
			}
			return _sortMethods;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		applicationStateInfo = applicationState;
		if (allSkillGems == null)
		{
			allSkillGems = new List<SkillGem>();
		}
	}

	public int GetRandomSlotCount(int currentSlotCount)
	{
		return ScriptableObjectSingleton<FortressDataManager>.instance.addSlotMatrixData.slotMatrix.Find((AddSlotMatrixData.SlotProbabilityRow row) => row.currentSlot == currentSlotCount)?.targetSlotProbabilities.SelectRandomWeight((AddSlotMatrixData.SlotProbability p) => p.probability).targetSlot ?? 0;
	}

	public int GetMaxSlotCount(int currentSlotCount)
	{
		return ScriptableObjectSingleton<FortressDataManager>.instance.addSlotMatrixData.slotMatrix.Find((AddSlotMatrixData.SlotProbabilityRow row) => row.currentSlot == currentSlotCount).targetSlotProbabilities.Where((AddSlotMatrixData.SlotProbability p) => p.probability != 0f).Max((AddSlotMatrixData.SlotProbability p) => p.targetSlot);
	}

	public void ConfirmAddSlotToSkill(Action<ChillyRoom.Services.ContainerOfSoul.ProfileSkill> cb)
	{
		GeneralGameAsyncOperationResultOnly<ConfirmAddSlotToSkillResult> op = new GeneralGameAsyncOperationResultOnly<ConfirmAddSlotToSkillResult>(async () => await applicationStateInfo.ChillyRoomSDK.Skill.ConfirmAddSlotToSkillAsync());
		op.OnComplete(delegate
		{
			cb?.Invoke(op.OpResult.Skill);
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_updating_skill_gem_slots);
	}

	public HexShape ConvertHexShapeFromServer(HexShapeSvr serverShape)
	{
		List<Vector2Int> points = serverShape.Points.Select((Vector2IntSvr vec) => new Vector2Int(vec.X, vec.Y)).ToList();
		return new HexShape
		{
			points = points,
			offset = new Vector2Int(serverShape.Offset.X, serverShape.Offset.Y)
		};
	}

	public void ConfirmMountSkillGem(string skillGuid, List<ChillyRoom.Services.ContainerOfSoul.SkillGem> skillGems, Action cb)
	{
		ConfirmMountSkillGemRequest confirmMountSkillGemRequest = new ConfirmMountSkillGemRequest();
		confirmMountSkillGemRequest.SkillGuid = skillGuid;
		confirmMountSkillGemRequest.SkillGems = skillGems;
		new GeneralGameAsyncOperationRequestOnly<ConfirmMountSkillGemRequest>(confirmMountSkillGemRequest, async delegate(ConfirmMountSkillGemRequest c)
		{
			await applicationStateInfo.ChillyRoomSDK.Skill.ConfirmMountSkillGemAsync(c);
		}).OnComplete(delegate
		{
			cb?.Invoke();
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_mounting_skill_gems);
	}

	public List<SkillGem> DeepCopyAllSkillGems()
	{
		deepCopySkillGems.Clear();
		List<SkillGem> collection = allSkillGems.Select((SkillGem s) => new SkillGem(s.def, s.rotateIndex, s.offset, s.amount)).ToList();
		deepCopySkillGems.AddRange(collection);
		return deepCopySkillGems;
	}

	public SkillGemDef GetRandomGem(int level)
	{
		return base.applicationState.assetCatalog.skillGemCatalog.ValidAssets.Where((SkillGemDef s) => s.level == level).ToList().SelectRandom();
	}

	public async Task Combine(SkillGem skillGem1, SkillGem skillGem2, SkillGem skillGem3)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			GeneralGameAsyncOperation<CombineGemRequest, CombineGemResult> op = new GeneralGameAsyncOperation<CombineGemRequest, CombineGemResult>(new CombineGemRequest
			{
				Gem1 = skillGem1.def.FullID,
				Gem2 = skillGem2.def.FullID,
				Gem3 = skillGem3.def.FullID
			}, base.applicationState.ChillyRoomSDK.Skill.CombineGemAsync);
			op.OnComplete(delegate
			{
				OnComplete(new SkillGem(base.applicationState.assetCatalog.FullIdToCTAsset<SkillGemDef>(op.OpResult.Gem)), op.OpResult.MatterInfo.ToClient(new NetworkConvertContext(base.applicationState)));
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_combining_skill_gems);
		}
		void OnComplete(SkillGem newGem, MatterInfo cost)
		{
			RemoveGem(skillGem1);
			RemoveGem(skillGem2);
			RemoveGem(skillGem3);
			base.applicationState.matterManager.ConsumeMatter(cost, "合并宝石");
			AddGem(newGem);
			base.applicationState.savingManager.SetProfileDirtyToSave();
			if (base.applicationState.fortressState?.uiManager != null)
			{
				base.applicationState.fortressState.uiManager.SetDirty();
				base.applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(new GlobalRewardPackage(new GlobalRewardInfo
				{
					skillGem = newGem
				}));
			}
			applicationStateInfo.eventManager.combineGemEvent.TriggerEvent(newGem);
			ThinkingAnalyticsExtends.TrackFortressCombineGem(base.applicationState, new List<SkillGem> { skillGem1, skillGem2, skillGem3 }, newGem);
			TraceLog.Debug("[SkillGemManager] - Combine.OnComplete \n " + allSkillGems.ToString((SkillGem s) => s?.def?.skillGemName + s?.amount));
		}
	}

	public MatterInfo GetCombineCost(int level)
	{
		return new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.commonCurrencyMatter, ScriptableObjectSingleton<FortressDataManager>.instance.gemCombineCost.TryGetValue(level, 0));
	}

	private int GetCombineNewLevel(int level)
	{
		Dictionary<int, int> probability = ScriptableObjectSingleton<FortressDataManager>.instance.gemCombineProbability[level];
		return probability.Keys.ToList().SelectRandomWeight((int l) => probability[l]);
	}

	private void RemoveGem(SkillGem gem, int count = 1)
	{
		RemoveGem(gem.def, count);
	}

	private void RemoveGem(SkillGemDef gem, int count = 1)
	{
		TraceLog.Debug($"[SkillGemManager] - RemoveGem({gem?.skillGemName}, {count}) - \n {allSkillGems.ToString((SkillGem s) => s?.def?.skillGemName + s?.amount)}");
		SkillGem skillGem = allSkillGems.Find((SkillGem g) => g.def == gem);
		if (skillGem != null && skillGem.amount >= count)
		{
			skillGem.amount -= count;
			if (skillGem.amount <= 0)
			{
				allSkillGems.Remove(skillGem);
			}
		}
	}

	public void AddGem(SkillGem gem, int count = 1)
	{
		AddGem(gem.def, count);
	}

	public void AddGem(SkillGemDef gem, int count = 1)
	{
		TraceLog.Debug($"[SkillGemManager] - AddGem({gem?.skillGemName}, {count}) - \n {allSkillGems.ToString((SkillGem s) => s?.def?.skillGemName + s?.amount)}");
		base.applicationState.discoverManager.Discover(gem);
		base.applicationState.globalUnlockManager.RecordGotObject(gem);
		SkillGem skillGem = allSkillGems.Find((SkillGem g) => g.def == gem);
		if (skillGem == null)
		{
			skillGem = new SkillGem(gem, 0);
			allSkillGems.Add(skillGem);
		}
		skillGem.amount += count;
	}

	public Sprite GetGemRewardIcon(int level)
	{
		return ScriptableObjectSingleton<GameDataManager>.instance.gemLevelIcons.IndexOrNull(level - 1) ?? ScriptableObjectSingleton<GameDataManager>.instance.itemPlaceHolderIcon;
	}

	public List<TMP_Dropdown.OptionData> GetSortOptions()
	{
		return sortMethods.Select(((string, Func<List<SkillGem>, List<SkillGem>>) c) => new TMP_Dropdown.OptionData(c.Item1)).ToList();
	}

	public void TestUnlockAll()
	{
		foreach (SkillGemDef validAsset in base.applicationState.assetCatalog.skillGemCatalog.ValidAssets)
		{
			AddGem(validAsset, 99);
		}
	}
}
