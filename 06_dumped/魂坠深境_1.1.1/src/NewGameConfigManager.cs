using System;
using ChillyRoom.Services.ContainerOfSoul;
using GameRandom;
using UnityEngine;

public class NewGameConfigManager : GlobalGameManager
{
	[SaveField(null, null)]
	public NewGameConfigData currentNewGameConfig;

	[SaveField(null, null)]
	private new ApplicationStateInfo applicationState;

	[SaveField(null, null)]
	private DifficultyLevel lastDailyChallengeDifficulty = DifficultyLevel.Normal;

	public DifficultyLevel LastDailyChallengeDifficulty => ClampDailyChallengeDifficulty(lastDailyChallengeDifficulty, DifficultyLevel.NightmareD10);

	public override void Init(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
		if (currentNewGameConfig == null)
		{
			currentNewGameConfig = NewGameConfigData.DefaultStartGameConfig(this.applicationState);
		}
		else
		{
			RestorePersistentConfigAfterDailyChallenge(currentNewGameConfig);
		}
	}

	public void StartGame(NewGameConfigData config)
	{
		SanitizeLimitedDeploymentSelection(config);
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		Bootstrap.instance.adsManager.LoadAll();
		StartGameRequest request = new StartGameRequest(config, applicationState);
		GeneralGameAsyncOperation<StartGameRequest, StartGameInfo> op = new GeneralGameAsyncOperation<StartGameRequest, StartGameInfo>(request, applicationState.ChillyRoomSDK.Gameplay.StartGameAsync);
		op.ManualRetry().OnComplete(delegate
		{
			if (!TryApplyDailyChallengeBinding(op.OpResult, config))
			{
				Bootstrap.instance.Load(BootStrapLoadData.ToMainMenu());
			}
			else
			{
				StartGameImpl(op.OpResult, config);
			}
		}).OnFault(delegate
		{
			Bootstrap.instance.Load(BootStrapLoadData.ToMainMenu());
		})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_updating_game_config);
	}

	public DifficultyLevel GetDailyChallengeInitialDifficulty(NewGameConfigData config)
	{
		DifficultyLevel maxSelectable = DifficultyLevel.NightmareD10;
		CharacterClassDef characterClassDef = config?.currentCharacter?.attachClass;
		if (characterClassDef != null && applicationState?.idolManager != null)
		{
			maxSelectable = applicationState.idolManager.GetMaxSelectableMainlineDifficulty(characterClassDef.mClass);
		}
		return ClampDailyChallengeDifficulty(lastDailyChallengeDifficulty, maxSelectable);
	}

	public void RememberDailyChallengeDifficulty(NewGameConfigData config)
	{
		if (config != null && config.IsDailyChallenge && config.contract != null)
		{
			DifficultyLevel difficultyLevel = ClampDailyChallengeDifficulty(config.contract.difficultyLevel, DifficultyLevel.NightmareD10);
			if (lastDailyChallengeDifficulty != difficultyLevel)
			{
				lastDailyChallengeDifficulty = difficultyLevel;
				applicationState?.savingManager?.SetProfileDirtyToSave();
			}
		}
	}

	public NewGameConfigData DetachDailyChallengeRunConfig(NewGameConfigData config)
	{
		if (config == null || !config.IsDailyChallenge || config != currentNewGameConfig)
		{
			return config;
		}
		NewGameConfigData result = NewGameConfigData.CreateDailyChallengeRunConfig(config);
		if (!RestorePersistentConfigAfterDailyChallenge(config))
		{
			return config;
		}
		return result;
	}

	public bool RestorePersistentConfigAfterDailyChallenge(NewGameConfigData config)
	{
		Contract contract = applicationState?.contractManager?.defaultGameContract;
		if (config != currentNewGameConfig || config?.dailyChallengeContext == null || contract == null)
		{
			return false;
		}
		config.contract = contract;
		config.ClearDailyChallengeContext();
		applicationState.savingManager?.SetProfileDirtyToSave();
		return true;
	}

	private static DifficultyLevel ClampDailyChallengeDifficulty(DifficultyLevel preferredDifficulty, DifficultyLevel maxSelectable)
	{
		DifficultyLevel max = (DifficultyLevel)Mathf.Clamp((int)maxSelectable, 1, 15);
		return (DifficultyLevel)Mathf.Clamp((int)preferredDifficulty, 1, (int)max);
	}

	private static bool TryApplyDailyChallengeBinding(StartGameInfo response, NewGameConfigData config)
	{
		DailyChallengeRunContext dailyChallengeContext = config.dailyChallengeContext;
		if (dailyChallengeContext == null || !dailyChallengeContext.isDailyChallenge)
		{
			return true;
		}
		string text = config.contract?.contractDef?.FullID;
		if (string.IsNullOrWhiteSpace(response?.ChallengeId) || string.IsNullOrWhiteSpace(response.ChallengeRuleContractId) || !string.Equals(response.ChallengeId, dailyChallengeContext.challengeId, StringComparison.Ordinal) || !string.Equals(response.ChallengeRuleContractId, text, StringComparison.Ordinal))
		{
			Debug.LogError("Daily challenge StartGame binding mismatch. Requested=" + dailyChallengeContext.challengeId + "/" + text + ", Received=" + response?.ChallengeId + "/" + response?.ChallengeRuleContractId);
			return false;
		}
		dailyChallengeContext.challengeId = response.ChallengeId;
		if (response.ChallengeEndTime.HasValue)
		{
			dailyChallengeContext.settlementUtc = (dailyChallengeContext.rotationResetUtc = ((response.ChallengeEndTime.Value.Kind == DateTimeKind.Utc) ? response.ChallengeEndTime.Value : DateTime.SpecifyKind(response.ChallengeEndTime.Value, DateTimeKind.Utc)));
		}
		return true;
	}

	private void StartGameImpl(NewGameConfigData config)
	{
		config.startGameInfo = NewGameConfigData.GenerateLocalPrepareInfo(applicationState);
		ConsumeLimitedDeploymentAfterGameCreated(config);
		applicationState.savingManager.SaveProfile();
		Level newGameLevel = Level.GetStageLevels(1, applicationState.assetCatalog).SelectRandom();
		Bootstrap.instance.Load(BootStrapLoadData.ToStartNewGame(config, Bootstrap.instance.CurrentAccountID, newGameLevel));
	}

	public void StartGameImpl(StartGameInfo startGameInfo, NewGameConfigData config)
	{
		config.startGameInfo = startGameInfo.ToClient(new NetworkConvertContext(applicationState));
		ConsumeLimitedDeploymentAfterGameCreated(config);
		applicationState.savingManager.SaveProfile();
		Level newGameLevel = Level.GetStageLevels(1, applicationState.assetCatalog).SelectRandom();
		Bootstrap.instance.Load(BootStrapLoadData.ToStartNewGame(config, Bootstrap.instance.CurrentAccountID, newGameLevel));
	}

	public bool SanitizeLimitedDeploymentSelection(NewGameConfigData config = null)
	{
		if (config == null)
		{
			config = currentNewGameConfig;
		}
		if (config == null || config.characterUsageMode != CharacterUsageMode.LimitedDeployment)
		{
			return false;
		}
		if (config.IsLimitedDeploymentSelected(applicationState))
		{
			return false;
		}
		config.ResetToDefaultCharacter(applicationState);
		return true;
	}

	public void ResetAfterLimitedDeploymentRun(NewGameConfigData finishedRunConfig)
	{
		if (finishedRunConfig != null && finishedRunConfig.IsLimitedDeploymentRun)
		{
			LimitedCharacterDeploymentService.MarkConsumed(applicationState);
			if (currentNewGameConfig == null)
			{
				currentNewGameConfig = NewGameConfigData.DefaultStartGameConfig(applicationState);
			}
			currentNewGameConfig.ResetToDefaultCharacter(applicationState);
		}
	}

	private void ConsumeLimitedDeploymentAfterGameCreated(NewGameConfigData config)
	{
		if (config != null && config.IsLimitedDeploymentRun)
		{
			LimitedCharacterDeploymentService.MarkConsumed(applicationState);
		}
	}
}
