using System.Collections.Generic;

public class GameContractState : InGameManager
{
	[SaveField(null, null)]
	public new GameStateInfo gameState;

	[SaveField(null, null)]
	public Contract selectedContract;

	[SaveField(null, null)]
	public int stage;

	[SaveField(null, null)]
	public Level currentLevel;

	[SaveField(null, null)]
	public List<ForerunnerLegacy> forerunnerLegacies;

	public bool IsChallengeContractGame
	{
		get
		{
			if (selectedContract.contractDef.contractType != ContractDef.ContractType.Challenge && selectedContract.contractDef.contractType != ContractDef.ContractType.HardChallenge)
			{
				return selectedContract.contractDef.contractType == ContractDef.ContractType.WeekShuffle;
			}
			return true;
		}
	}

	public override void Init(GameStateInfo gameState)
	{
		this.gameState = gameState;
		if (forerunnerLegacies == null)
		{
			forerunnerLegacies = new List<ForerunnerLegacy>();
			if (gameState.startBootStrapData.newGameConfigData.startGameInfo.forerunnerLegacyThisGame != null)
			{
				forerunnerLegacies.AddRange(gameState.startBootStrapData.newGameConfigData.startGameInfo.forerunnerLegacyThisGame);
			}
		}
	}
}
