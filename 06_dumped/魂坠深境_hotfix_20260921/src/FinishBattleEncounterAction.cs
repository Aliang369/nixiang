using System.Linq;
using Sirenix.Utilities;

public class FinishBattleEncounterAction : ExecuteAction
{
	public enum FinishType
	{
		Win,
		Lose,
		Escape,
		Break
	}

	public FinishType type;

	public FinishBattleEncounterAction(FinishType type)
	{
		this.type = type;
	}

	public static string GetFinishBattleTitle(FinishType type)
	{
		return type switch
		{
			FinishType.Win => ScriptableObjectSingleton<TextDataPreset>.instance.term_battle_win, 
			FinishType.Lose => ScriptableObjectSingleton<TextDataPreset>.instance.term_battle_lose, 
			FinishType.Escape => ScriptableObjectSingleton<TextDataPreset>.instance.term_battle_escape, 
			_ => "", 
		};
	}

	protected override void Update()
	{
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		EnqueueAction(new StopBattleAction(type));
		if (gameState.IsCharacterTrialGame)
		{
			if (type == FinishType.Win)
			{
				EnqueueAction(new FinishHintAction(ScriptableObjectSingleton<TextDataPreset>.instance.term_battle_win, halt: true));
				EnqueueAction(new ActionAction(delegate
				{
					gameState.LoadToFortressAfterCharacterTrial(completed: true);
				}));
			}
			else
			{
				gameState.LoadToFortressAfterCharacterTrial(completed: false);
			}
			return;
		}
		gameState.gameEventManager.TriggerBeforeBattlEncountEnd();
		if (gameState.IsPvpEnemyTestGame)
		{
			Bootstrap.instance.Load(BootStrapLoadData.LoadToFortress(Bootstrap.instance.CurrentAccountID, BootStrapLoadData.EnterFortressType.PvpEnemyPanel));
			return;
		}
		if (type == FinishType.Lose)
		{
			EnqueueAction(new EndGameAction(EndGameAction.EndGameType.Death));
			return;
		}
		EnqueueAction(new ActionAction(delegate
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Bootstrap.instance.audioManager.PlaySustained(gameState.currentEncounter.level.peaceMusicRef, "BgmTag");
			UIHelper.HideAllInfo(null);
		}));
		EnqueueAction(new ActionAction(delegate
		{
			gameState.UIManager.enemyBftUIDisplay.Unset();
		}));
		EnqueueAction(new WaitAction(1.5f));
		BattleBonus finalBonus = GetFinalBonus();
		bool flag = false;
		if (gameState.dungeonMapManager.currentLocation != null && gameState.dungeonMapManager.currentLocation.type == PathLocation.LocationType.Boss)
		{
			bool num = ShouldFinishAtBaseGameBoss();
			bool flag2 = !gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableLongGameStage) && gameState.contractState.stage >= ScriptableObjectSingleton<DungeonGenerationManager>.instance.initFinishGameStage;
			bool isIntroGame = gameState.IsIntroGame;
			bool flag3 = gameState.contractState.stage >= ScriptableObjectSingleton<DungeonGenerationManager>.instance.finalFinishGameStage;
			NewGameConfigData newGameConfigData = gameState.newGameConfigData;
			bool flag4 = newGameConfigData != null && newGameConfigData.ResolvedFixedContractRoute?.IsCompletedAfterStage(gameState.contractState.stage) == true;
			flag = num | flag2 | isIntroGame | flag3 | flag4;
			if (flag)
			{
				finalBonus.bonuses.OfType<GemBonus>().ForEach(delegate(GemBonus gb)
				{
					gb.TriggerBonus(gameState, null);
				});
				EnqueueAction(new EndGameAction(EndGameAction.EndGameType.Win));
			}
			else
			{
				bool variableFlag = gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.AfterBossHealHalfHealth);
				int num2 = gameState.playerState.bft.maxHealth;
				int healAmount = (variableFlag ? (num2 / 2) : num2);
				EnqueueAction(new HealAction(null, gameState.playerState.bft, healAmount));
			}
		}
		EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_victory_stinger));
		if (!flag)
		{
			if (finalBonus != null && !finalBonus.bonuses.IsNullOrEmpty())
			{
				finalBonus.title = GetFinishBattleTitle(type);
				EnqueueAction(new TriggerBonusAction(finalBonus, gameState.currentEncounter.autoEndAfterFinished, isBattle: true));
			}
			else if (gameState.currentEncounter.autoEndAfterFinished)
			{
				EnqueueAction(new EndCurrentEncounterAction(finish: true));
			}
		}
	}

	private bool ShouldFinishAtBaseGameBoss()
	{
		if (gameState.contractState.stage < ScriptableObjectSingleton<DungeonGenerationManager>.instance.baseFinishGameStage)
		{
			return false;
		}
		if (gameState.difficultyState.contractDifficulty < DifficultyLevel.Nightmare)
		{
			return true;
		}
		if (gameState.contractState.stage >= ScriptableObjectSingleton<DungeonGenerationManager>.instance.finalFinishGameStage)
		{
			return false;
		}
		return !FinalEndingKeyUtility.HasPlayerRepairedFinalEndingKey(gameState);
	}

	private BattleBonus GetFinalBonus()
	{
		BattleRewardContext context = new BattleRewardContext(gameState, type);
		return (gameState.contractState?.selectedContract?.contractDef?.GetBattleRewardRule(gameState) ?? BattleRewardRule.Default).BuildFinalBonus(context);
	}

	public static void AddNormalFightBonus(GameStateInfo gameState, BattleBonus bonus)
	{
		BattleRewardRule.Default.AddNormalFightBonus(gameState, bonus);
	}

	public static void AddEliteFightBonus(GameStateInfo gameState, BattleBonus bonus, bool includeOptionalRewards = true)
	{
		BattleRewardRule.Default.AddEliteFightBonus(gameState, bonus, includeOptionalRewards);
	}
}
