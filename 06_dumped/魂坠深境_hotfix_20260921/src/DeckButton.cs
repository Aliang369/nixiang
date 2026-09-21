using UnityEngine;

public class DeckButton : UIBase
{
	[SerializeField]
	private GameButton button;

	protected override void Init()
	{
		button.onClick.AddListener(_OnButtonClicked);
		SetDirty();
	}

	private void _OnButtonClicked()
	{
		UIHelper.HideAllInfo(null);
		UIBase.gameState.UIManager.deckPanel.Show();
		UIBase.gameState.UIManager.deckPanel.SetDirty();
	}
}
