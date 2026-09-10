using Common.UI;
using ContainerOfSoul.Display.Fortress.GameConfig;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.ContractSelectDisplay;

public class FortressBaseGameUIPanel : UIBase, IShowAsync
{
	[SerializeField]
	public GameConfigPanel gameConfigPanel;

	[SerializeField]
	private GameButton backButton;

	private NewGameConfigData config => UIBase.applicationState.newGameConfigManager?.currentNewGameConfig;

	protected override void Init()
	{
		InitUI(gameConfigPanel);
		DifficultyLevel difficultyLevel = config.contract?.difficultyLevel ?? UIBase.applicationState.contractManager.defaultGameContract.difficultyLevel;
		config.contract = UIBase.applicationState.contractManager.defaultGameContract;
		config.contract.difficultyLevel = difficultyLevel;
	}

	void IShowAsync.OnShowStart()
	{
		base.gameObject.SetActive(value: true);
		NewGameConfigData currentNewGameConfig = UIBase.applicationState.newGameConfigManager.currentNewGameConfig;
		DifficultyLevel difficultyLevel = currentNewGameConfig.contract?.difficultyLevel ?? UIBase.applicationState.contractManager.defaultGameContract.difficultyLevel;
		currentNewGameConfig.contract = UIBase.applicationState.contractManager.defaultGameContract;
		currentNewGameConfig.contract.difficultyLevel = difficultyLevel;
		gameConfigPanel.Setup(currentNewGameConfig);
		manager.ShowUI(gameConfigPanel);
	}
}
