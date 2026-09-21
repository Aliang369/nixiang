using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObsessionLevelInfoPanel : UIBase
{
	public GameButton backButton;

	public Image characterIconImage;

	public TextMeshProUGUI currentTokenUpgradeCountText;

	public CharacterTokenUpgradeCell tokenUpgradeCellPrefab;

	public RectTransform tokenUpgradeParent;

	public CellUIPrefabPool<CharacterTokenUpgradeCell, (PlayerCharacterState, GameConfigEffect)> pool;

	public PlayerCharacterState characterState;

	public int targetLevel;

	protected override void Init()
	{
		targetLevel = -1;
		backButton?.onClick.AddListener(_OnBackButtonClicked);
		GetPrefabPool(ref pool, tokenUpgradeCellPrefab);
	}

	public void Setup(PlayerCharacterState characterState)
	{
		this.characterState = characterState;
		targetLevel = characterState.ActiveCharacterObsessionLevels.Count;
		SetDirty();
	}

	public void Setup(PlayerCharacterState characterState, int targetLevel)
	{
		Setup(characterState);
		this.targetLevel = targetLevel;
	}

	protected override void UpdateDisplay()
	{
	}

	private void _OnBackButtonClicked()
	{
		base.gameObject.SetActive(value: false);
	}
}
