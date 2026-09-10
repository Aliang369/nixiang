using UnityEngine;

public class FortressRoomSetup : MonoBehaviour
{
	public GameObject drawSkillPanelObject;

	public GameObject indexPanelObject;

	public GameObject challengeStarMapPanelObject;

	public GameObject achievementPanelObject;

	[SerializeField]
	private GameButton enterGameButton;

	[SerializeField]
	private GameButton skillButton;

	[SerializeField]
	private GameButton indexButton;

	[SerializeField]
	private GameButton challengeButton;

	private FortressStateInfo fortressState => Bootstrap.instance.CurrentApplicationState.fortressState;

	private void Awake()
	{
		enterGameButton?.onClick.AddListener(fortressState.uiManager.mainUIPanel.mainPanelMainRoom._OnToBaseGameButtonClicked);
		skillButton?.onClick.AddListener(fortressState.uiManager.mainUIPanel.mainPanelMainRoom._OnToGachaButtonClicked);
	}
}
