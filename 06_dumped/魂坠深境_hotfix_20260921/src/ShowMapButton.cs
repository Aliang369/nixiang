using System.Collections;
using UnityEngine;

public class ShowMapButton : UIBase
{
	[SerializeField]
	public GameButton showHintButton;

	private bool showMapAble
	{
		get
		{
			if (!UIBase.gameState.IsCharacterTrialGame && UIBase.gameState.PlayerControlAble && UIBase.gameState.actionManager.isEmpty && !ShowingEventAnim())
			{
				EncounterInfo currentEncounter = UIBase.gameState.currentEncounter;
				if (currentEncounter == null)
				{
					return true;
				}
				return currentEncounter.triggerEvent?.finishing != true;
			}
			return false;
		}
	}

	protected override void Init()
	{
		showHintButton.onClick.AddListener(_OnShowMapButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		if (UIBase.gameState.IsCharacterTrialGame)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (UIBase.gameState.dungeonMapManager.inMapMode)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void _OnShowMapButtonClicked()
	{
		if (showMapAble)
		{
			UIBase.gameState.StartCoroutine(ShowMap);
		}
	}

	private IEnumerator ShowMap()
	{
		UIHelper.DisableAllButton();
		yield return UIBase.gameState.UIManager.showMapFogPanel.ShowFog();
		if (!UIBase.gameState.UIManager.dungeonMapPanel_Preview.EnsureSetup())
		{
			yield return UIBase.gameState.UIManager.showMapFogPanel.HideFog();
			UIHelper.EnableAllButton();
			yield break;
		}
		UIBase.gameState.UIManager.gameVirtualCamera.SetState(VirtualCamera.State.Map);
		yield return EnumeratorExtends.WaitFrame(2f);
		UIBase.gameState.UIManager.basePanels.gameObject.SetActive(value: false);
		UIBase.gameState.UIManager.secondaryPanels.gameObject.SetActive(value: false);
		UIBase.gameState.StartCoroutine(() => UIBase.gameState.UIManager.battlePanel.HidePanel(anim: false));
		UIBase.gameState.currentEncounter?.room?.gameObject.SetActive(value: false);
		UIBase.gameState.currentEncounter?.enemyAI?.BftDisplay?.gameObject.SetActive(value: false);
		UIBase.gameState.currentEncounter?.majorRoomObjectDisplay?.gameObject.SetActive(value: false);
		UIBase.gameState.UIManager.scene3DRoot.gameObject.SetActive(value: false);
		UIBase.gameState.UIManager.dungeonMapPanel_Preview.ShowPanel();
		UIBase.gameState.UIManager.dungeonMapPanel_Preview.dungeonMapDisplay.SetCurrentCenter();
		yield return UIBase.gameState.UIManager.showMapFogPanel.HideFog();
		UIHelper.EnableAllButton();
	}

	private bool ShowingEventAnim()
	{
		if (UIBase.gameState.UIManager.currentEventPanel != null)
		{
			return UIBase.gameState.UIManager.currentEventPanel.HasAnim();
		}
		return false;
	}
}
