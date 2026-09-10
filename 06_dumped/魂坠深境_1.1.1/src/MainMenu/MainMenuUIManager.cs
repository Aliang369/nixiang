using UnityEngine;

namespace MainMenu;

public class MainMenuUIManager : UIManager
{
	[SerializeField]
	public MainMenuPanel mainMenuPanel;

	protected override void OnInit()
	{
		InitUI(mainMenuPanel);
	}

	private void OnDestroy()
	{
		Dispose();
	}

	private void Update()
	{
		FrameUpdate();
	}
}
