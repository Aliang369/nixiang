using System.Collections.Generic;
using ChillyRoom.Services.ContainerOfSoul;

public class ActivateStartGameBuffAction : GameAction
{
	public StartGameBuff startGameBuff;

	private GeneralGameAsyncOperation<ActivateStartGameBuffRequest, ActivateStartGameBuffResult> op;

	public ActivateStartGameBuffAction(StartGameBuff startGameBuff)
	{
		this.startGameBuff = startGameBuff;
	}

	protected override void Update()
	{
		if (!GlobalConfigManager.instance.enableNetwork || op != null)
		{
			return;
		}
		StartGameBuff item = StartGameBuff.CreateBuffWithExpireDayPeriod(startGameBuff.def, startGameBuff.expireDayPeriod);
		ActivateStartGameBuffRequest request = new ActivateStartGameBuffRequest(new List<StartGameBuff> { item });
		op = new GeneralGameAsyncOperation<ActivateStartGameBuffRequest, ActivateStartGameBuffResult>(request, gameState.applicationState.ChillyRoomSDK.Gameplay.ActivateStartGameBuffAsync);
		op.OnComplete(delegate
		{
			List<StartGameBuff> startGameBuffs = op.OpResult.GetStartGameBuffs(gameState.applicationState);
			foreach (StartGameBuff item2 in startGameBuffs)
			{
				item2.Activate(gameState);
			}
			gameState.startGameBuffs = startGameBuffs;
		}).OnOver(base.FinishAction).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_activating_start_game_buff);
	}
}
