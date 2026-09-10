using System.Collections;
using UnityEngine;

public class HideMapButton : UIBase
{
	[SerializeField]
	private GameButton showHintButton;

	protected override void Init()
	{
		showHintButton.onClick.AddListener(_OnHideMapButtonClicked);
	}

	private void _OnHideMapButtonClicked()
	{
		UIBase.gameState.StartCoroutine(HideMap);
	}

	private IEnumerator HideMap()
	{
		UIHelper.DisableAllButton();
		yield return UIBase.gameState.UIManager.showMapFogPanel.ShowFog();
		UIBase.gameState.UIManager.basePanels.gameObject.SetActive(value: true);
		UIBase.gameState.UIManager.secondaryPanels.gameObject.SetActive(value: true);
		if (UIBase.gameState.currentEncounter?.triggerEvent is EnterBattleEvent)
		{
			UIBase.gameState.UIManager.gameVirtualCamera.SetState(VirtualCamera.State.Battle);
			UIBase.gameState.StartCoroutine(() => UIBase.gameState.UIManager.battlePanel.ShowPanel(anim: false));
		}
		else
		{
			UIBase.gameState.UIManager.gameVirtualCamera.SetState(VirtualCamera.State.Event);
		}
		UIBase.gameState.currentEncounter?.room?.gameObject.SetActive(value: true);
		UIBase.gameState.currentEncounter?.enemyAI?.BftDisplay?.gameObject.SetActive(value: true);
		UIBase.gameState.currentEncounter?.majorRoomObjectDisplay?.gameObject.SetActive(value: true);
		UIBase.gameState.UIManager.scene3DRoot.gameObject.SetActive(value: true);
		UIBase.gameState.UIManager.dungeonMapPanel_Preview.HidePanel();
		yield return UIBase.gameState.UIManager.showMapFogPanel.HideFog();
		UIBase.gameState.UIManager.SetDirty();
		UIHelper.EnableAllButton();
	}
}
