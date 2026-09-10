using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;

public class ClavisManager : GlobalGameManager
{
	[SaveField(null, null)]
	public new ApplicationStateInfo applicationState;

	[SaveField(null, null)]
	public List<ClavisDef> unlockedClavis;

	[SaveField(null, null)]
	public List<ClavisDef> activetedClavis;

	[SaveField(null, null)]
	public Dictionary<string, int> clavisUnlockGameCounts;

	public List<ClavisDef> UnlockedVisibleClavis => unlockedClavis.FindAll((ClavisDef x) => x != null && !x.hiddenClavis);

	public List<ClavisDef> AllVisibleClavis => applicationState.assetCatalog.clavisDefDefCatalog.ValidAssets.FindAll((ClavisDef x) => !x.hiddenClavis).OrderBy(GetDisplaySortIndex).ThenBy((ClavisDef x) => x.SOindex)
		.ToList();

	private static int GetDisplaySortIndex(ClavisDef clavisDef)
	{
		if (clavisDef.displaySortIndex <= 0)
		{
			return clavisDef.SOindex;
		}
		return clavisDef.displaySortIndex;
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
		if (unlockedClavis == null)
		{
			unlockedClavis = new List<ClavisDef>();
		}
		if (activetedClavis == null)
		{
			activetedClavis = new List<ClavisDef>();
		}
		if (clavisUnlockGameCounts == null)
		{
			clavisUnlockGameCounts = new Dictionary<string, int>();
		}
		NormalizeLoadedClavisState();
		foreach (ClavisDef validAsset in this.applicationState.assetCatalog.clavisDefDefCatalog.ValidAssets)
		{
			validAsset.InitObserver(this.applicationState);
		}
		if (applicationState.bootStrapLoadData.skipStory)
		{
			TestUnlockAll();
		}
	}

	public void UnlockClavis(ClavisDef clavisDef, bool silent = false)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			UnlockClavisRequest request = new UnlockClavisRequest(clavisDef);
			GeneralGameAsyncOperation<UnlockClavisRequest, UnlockClavisResult> op = new GeneralGameAsyncOperation<UnlockClavisRequest, UnlockClavisResult>(request, applicationState.ChillyRoomSDK.Progress.UnlockClavisAsync);
			op.OnComplete(delegate
			{
				UnlockClavisImpl(op.OpResult, applicationState, silent);
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_unlocking_clavis);
		}
	}

	private void UnlockClavisImpl(ClavisDef clavisDef, bool silent)
	{
		if (!applicationState.clavisManager.unlockedClavis.Contains(clavisDef))
		{
			SetUnlockClavis(clavisDef);
			if (!silent && !clavisDef.activateAfterUnlock)
			{
				EnqueueShowClavisHint(clavisDef, ClavisHint.HintType.Unlock);
			}
			if (clavisDef.activateAfterUnlock)
			{
				ActivatedClavisImpl(clavisDef, silent);
			}
		}
	}

	public void UnlockClavisImpl(UnlockClavisResult result, ApplicationStateInfo applicationState, bool silent)
	{
		ClavisDef clavisDef = new NetworkConvertContext(applicationState).FullIdToCTAsset<ClavisDef>(result.Id);
		SetUnlockClavis(clavisDef);
		if (!silent && result.ActivateResult == null)
		{
			EnqueueShowClavisHint(clavisDef, ClavisHint.HintType.Unlock);
		}
		if (result.ActivateResult != null)
		{
			ActivatedClavisImpl(result.ActivateResult, applicationState, silent);
		}
	}

	public void EnqueueShowClavisHint(ClavisDef clavisDef, ClavisHint.HintType hintType)
	{
		if (!clavisDef.hiddenClavis)
		{
			if (applicationState.runningGameState != null)
			{
				applicationState.runningGameState.UIManager.clavisHint.EnqueueShowClavisHint(clavisDef, hintType);
			}
			if (applicationState.fortressState != null)
			{
				applicationState.fortressState.uiManager.clavisHint.EnqueueShowClavisHint(clavisDef, hintType);
			}
		}
	}

	private void EnqueueShowActivatedClavisHint(ClavisDef clavisDef)
	{
		EnqueueShowClavisHint(clavisDef, ClavisHint.HintType.Activate);
	}

	public void ActivatedClavis(ClavisDef clavisDef, bool silent = false, Action afterActivate = null)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			ActivateClavisRequest request = new ActivateClavisRequest(clavisDef);
			GeneralGameAsyncOperation<ActivateClavisRequest, ActivateClavisResult> op = new GeneralGameAsyncOperation<ActivateClavisRequest, ActivateClavisResult>(request, applicationState.ChillyRoomSDK.Progress.ActivateClavisAsync);
			op.OnComplete(delegate
			{
				ActivatedClavisImpl(op.OpResult, applicationState, silent);
				afterActivate?.Invoke();
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_activating_clavis);
		}
	}

	private void ActivatedClavisImpl(ClavisDef clavisDef, bool silent)
	{
		if (!IsActivated(clavisDef))
		{
			new GlobalRewardPackage(clavisDef.activateRewards.Select((GlobalRewardSetup r) => r.GenerateReward(applicationState, GlobalRewardSourceType.Clavis, clavisDef)).ToList()).GiveAll(applicationState);
			applicationState.eventManager.ActivateClavisEvent.TriggerEvent(clavisDef);
			applicationState.clavisManager.activetedClavis.AddUnique(clavisDef);
			if (!silent)
			{
				EnqueueShowActivatedClavisHint(clavisDef);
			}
		}
	}

	private void ActivatedClavisImpl(ActivateClavisResult result, ApplicationStateInfo applicationState, bool silent)
	{
		NetworkConvertContext context = new NetworkConvertContext(applicationState);
		ClavisDef clavisDef = context.FullIdToCTAsset<ClavisDef>(result.Id);
		if (result.Rewards != null)
		{
			new GlobalRewardPackage(result.Rewards.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(context)).ToList()).GiveAll(applicationState);
		}
		applicationState.eventManager.ActivateClavisEvent.TriggerEvent(clavisDef);
		applicationState.clavisManager.activetedClavis.AddUnique(clavisDef);
		if (!silent)
		{
			EnqueueShowActivatedClavisHint(clavisDef);
		}
	}

	public void SetUnlockClavis(ClavisDef clavisDef)
	{
		applicationState.eventManager.UnlockClavisEvent.TriggerEvent(clavisDef);
		unlockedClavis.AddUnique(clavisDef);
		if (clavisDef != null)
		{
			clavisUnlockGameCounts.TryAdd(clavisDef.FullID, applicationState.gameStatisticsManager.gameCount);
		}
	}

	public bool IsUnlocked(ClavisDef clavisDef)
	{
		return unlockedClavis.Contains(clavisDef);
	}

	public bool IsActivated(ClavisDef clavisDef)
	{
		return activetedClavis.Contains(clavisDef);
	}

	public ClavisDef FindActivateRewardClavis(GlobalVariableSetup setup)
	{
		if (setup == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(setup.id))
		{
			ApplicationGlobalVariableManager.InitSetup();
		}
		return (applicationState?.assetCatalog?.clavisDefDefCatalog?.ValidAssets)?.FirstOrDefault((ClavisDef clavis) => clavis != null && clavis.IsValidAsset() && clavis.activateRewards != null && clavis.activateRewards.OfType<SetGlobalVariableGlobalReward>().Any((SetGlobalVariableGlobalReward reward) => reward.globalVariable == setup.id));
	}

	public string GetGlobalVariableUnlockHint(GlobalVariableSetup setup)
	{
		ClavisDef clavisDef = FindActivateRewardClavis(setup);
		if (!(clavisDef != null) || clavisDef.hiddenClavis)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_room_lock_need_explor;
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_continue_game_find_clavis_to_unlock.FormatWith(clavisDef.NameWithColor);
	}

	public bool IsUnlockedBeforeCurrentGame(ClavisDef clavisDef)
	{
		if (!IsUnlocked(clavisDef))
		{
			return false;
		}
		if (clavisDef == null || !clavisUnlockGameCounts.TryGetValue(clavisDef.FullID, out var value))
		{
			return true;
		}
		return value < applicationState.gameStatisticsManager.gameCount;
	}

	private void NormalizeLoadedClavisState()
	{
		unlockedClavis = unlockedClavis.Where((ClavisDef c) => c != null).Distinct().ToList();
		activetedClavis = activetedClavis.Where((ClavisDef c) => c != null).Distinct().ToList();
		foreach (ClavisDef activetedClavi in activetedClavis)
		{
			unlockedClavis.AddUnique(activetedClavi);
		}
		foreach (ClavisDef unlockedClavi in unlockedClavis)
		{
			clavisUnlockGameCounts.TryAdd(unlockedClavi.FullID, 0);
		}
	}

	public void TestUnlockAll()
	{
		foreach (ClavisDef validAsset in applicationState.assetCatalog.clavisDefDefCatalog.ValidAssets)
		{
			if (!IsUnlocked(validAsset))
			{
				SetUnlockClavis(validAsset);
			}
			ActivatedClavisImpl(validAsset, silent: true);
		}
	}
}
