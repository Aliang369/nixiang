using UnityEngine;

public class PauseButton : UIBase
{
	[SerializeField]
	private GameButton showHintButton;

	protected override void Init()
	{
		showHintButton.onClick.AddListener(_PauseButtonClicked);
	}

	private void _PauseButtonClicked()
	{
		UIBase.gameState.UIManager.settingPanel.SetActiveAndDirtyWithFadeAnim(active: true);
	}
}
