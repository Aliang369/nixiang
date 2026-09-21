using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;

public class CharacterTrialManager : GlobalGameManager
{
	[SaveField(null, null)]
	public List<string> completedCharacterIds;

	[SaveField(null, null)]
	public string pendingRewardCharacterId;

	[SaveField(null, null)]
	public List<GlobalRewardInfo> pendingRewardInfos;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (completedCharacterIds == null)
		{
			completedCharacterIds = new List<string>();
		}
		if (CanRepeatCompletedTrialsInLocalPackage())
		{
			completedCharacterIds.Clear();
		}
	}

	public bool IsTrialCompleted(PlayerCharacterDef character)
	{
		if (character == null)
		{
			return false;
		}
		if (completedCharacterIds == null)
		{
			completedCharacterIds = new List<string>();
		}
		return completedCharacterIds.Contains(character.FullID);
	}

	public bool ShouldLockTrialEntry(PlayerCharacterDef character)
	{
		if (character == null)
		{
			return true;
		}
		if (character == ScriptableObjectSingleton<FortressDataManager>.instance?.defaultCharacter)
		{
			return true;
		}
		if (!HasTrialEntry(character))
		{
			return true;
		}
		if (IsTrialCompleted(character))
		{
			return !CanRepeatCompletedTrials();
		}
		return false;
	}

	private static bool HasTrialEntry(PlayerCharacterDef character)
	{
		CharacterTrialConfig instance = ScriptableObjectSingleton<CharacterTrialConfig>.instance;
		if (instance?.entries != null)
		{
			return instance.entries.Any((CharacterTrialConfig.TrialEntry e) => e?.character == character);
		}
		return false;
	}

	private static bool CanRepeatCompletedTrials()
	{
		if (CanRepeatCompletedTrialsInLocalPackage())
		{
			return true;
		}
		return false;
	}

	private static bool CanRepeatCompletedTrialsInLocalPackage()
	{
		if (GlobalConfigManager.instance != null)
		{
			return !GlobalConfigManager.instance.enableNetwork;
		}
		return false;
	}

	public void SetCompletedCharacterIds(IEnumerable<string> characterIds)
	{
		completedCharacterIds = characterIds?.Where((string id) => !string.IsNullOrEmpty(id)).Distinct().ToList() ?? new List<string>();
	}

	public void CompleteTrialAndGrantRewards(PlayerCharacterDef character, Action onComplete = null, Action onFault = null)
	{
		if (IsTrialCompleted(character))
		{
			onComplete?.Invoke();
			return;
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			CompleteTrialAndGrantRewardsRemote(character, onComplete, onFault);
			return;
		}
		CompleteTrialAndGrantRewardsLocal(character);
		onComplete?.Invoke();
	}

	private void CompleteTrialAndGrantRewardsLocal(PlayerCharacterDef character)
	{
		List<GlobalRewardInfo> list = ScriptableObjectSingleton<CharacterTrialConfig>.instance.GetEntry(character).completionRewards.Select((GlobalRewardSetup reward) => reward.GenerateReward(base.applicationState, GlobalRewardSourceType.CharacterTrial, character)).ToList();
		completedCharacterIds.Add(character.FullID);
		if (list.Count > 0)
		{
			new GlobalRewardPackage(list).GiveAll(base.applicationState);
			pendingRewardCharacterId = character.FullID;
			pendingRewardInfos = list;
		}
		base.applicationState.savingManager.SaveProfile();
	}

	private void CompleteTrialAndGrantRewardsRemote(PlayerCharacterDef character, Action onComplete, Action onFault)
	{
		CompleteCharacterTrialRequest request = new CompleteCharacterTrialRequest(character);
		GeneralGameAsyncOperation<CompleteCharacterTrialRequest, CompleteCharacterTrialResult> op = new GeneralGameAsyncOperation<CompleteCharacterTrialRequest, CompleteCharacterTrialResult>(request, base.applicationState.ChillyRoomSDK.Character.CompleteCharacterTrialAsync);
		op.ManualRetry().OnComplete(delegate
		{
			ApplyRemoteCompleteResult(character, op.OpResult);
			onComplete?.Invoke();
		}).OnFault(delegate
		{
			onFault?.Invoke();
		})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_character_trial);
	}

	private void ApplyRemoteCompleteResult(PlayerCharacterDef character, CompleteCharacterTrialResult result)
	{
		if (result.CompletedCharacterTrialIds != null)
		{
			SetCompletedCharacterIds(result.CompletedCharacterTrialIds);
		}
		else
		{
			AddCompletedCharacterId(character.FullID);
		}
		GlobalRewardPackage globalRewardPackage = result.GiveRewards(base.applicationState);
		if (globalRewardPackage?.rewards != null && globalRewardPackage.rewards.Count > 0)
		{
			pendingRewardCharacterId = character.FullID;
			pendingRewardInfos = globalRewardPackage.rewards;
		}
		base.applicationState.savingManager.SaveProfile();
	}

	private void AddCompletedCharacterId(string characterId)
	{
		if (completedCharacterIds == null)
		{
			completedCharacterIds = new List<string>();
		}
		if (!completedCharacterIds.Contains(characterId))
		{
			completedCharacterIds.Add(characterId);
		}
	}

	public void TryShowPendingCompletionRewards(PlayerCharacterDef character)
	{
		base.fortressState.StartCoroutine(() => ShowPendingCompletionRewardsCor(character));
	}

	public IEnumerator ShowPendingCompletionRewardsCor(PlayerCharacterDef character)
	{
		if (!(pendingRewardCharacterId != character.FullID))
		{
			List<GlobalRewardInfo> list = pendingRewardInfos;
			if (list == null || list.Count == 0)
			{
				ClearPendingCompletionRewards();
				yield break;
			}
			GlobalRewardPackage pack = new GlobalRewardPackage(list);
			yield return base.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(pack);
			ClearPendingCompletionRewards();
			base.fortressState.uiManager.SetDirty();
		}
	}

	private void ClearPendingCompletionRewards()
	{
		pendingRewardCharacterId = null;
		pendingRewardInfos = null;
		base.applicationState.savingManager.SaveProfile();
	}
}
