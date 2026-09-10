using Common.UI;
using ContainerOfSoul.Display.Fortress.DotManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectorDisplay : CellUI<PlayerCharacterState>, IDisplayModulesController
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image unselectedBackground;

	[SerializeField]
	private Image selectedBackground;

	[SerializeField]
	private Image selectedMark;

	[SerializeField]
	private GameObject unlockArea;

	[SerializeField]
	private GameObject obsessionArea;

	[SerializeField]
	private ObsessionProgressDisplay obsessionBar;

	[SerializeField]
	private HintRedDot redDot;

	[SerializeField]
	private Material iconMaterial;

	private TextMeshProUGUI unlockText;

	private string defaultUnlockText;

	private Color defaultUnlockColor;

	private bool defaultUnlockVertexGradient;

	private VertexGradient defaultUnlockColorGradient;

	private FontStyles defaultUnlockFontStyle;

	private Color32 defaultUnlockOutlineColor;

	private float defaultUnlockOutlineWidth;

	public PlayerCharacterState character => base.setupData;

	public bool isSelected => (parent as CharacterSelectorArea)?.IsSelected(character) ?? false;

	protected override void Init()
	{
		InitUI(obsessionBar);
		unlockText = unlockArea.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		defaultUnlockText = CellUI<PlayerCharacterState>.textData.term_locked;
		if (unlockText != null)
		{
			defaultUnlockColor = unlockText.color;
			defaultUnlockVertexGradient = unlockText.enableVertexGradient;
			defaultUnlockColorGradient = unlockText.colorGradient;
			defaultUnlockFontStyle = unlockText.fontStyle;
			defaultUnlockOutlineColor = unlockText.outlineColor;
			defaultUnlockOutlineWidth = unlockText.outlineWidth;
		}
		if (redDot != null)
		{
			InitUI(redDot);
		}
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		icon.sprite = character.def.headIcon;
		obsessionBar.Setup(setupData);
		if (redDot != null)
		{
			redDot.Setup(new CustomModule
			{
				shouldShow = () => LimitedCharacterDeploymentService.ShouldShowNewHint(CellUI<PlayerCharacterState>.applicationState, character) || (character != null && !character.locked && CellUI<PlayerCharacterState>.applicationState.playerInfoManager.showUnlockTalent && RedDotModule.HasAnyUnlockableTalent(character))
			});
		}
	}

	protected override void UpdateDisplay()
	{
		bool flag = isSelected;
		bool locked = character.locked;
		bool flag2 = LimitedCharacterDeploymentService.IsAvailable(CellUI<PlayerCharacterState>.applicationState, character);
		int obsessionDebrisCount = character.obsessionDebrisCount;
		selectedMark.gameObject.SetActive(flag);
		selectedBackground.gameObject.SetActive(flag);
		unselectedBackground.gameObject.SetActive(!flag);
		bool flag3 = locked && !flag2 && obsessionDebrisCount > 0;
		obsessionArea.SetActive(flag3);
		obsessionBar.gameObject.SetActive(flag3);
		unlockArea.SetActive(flag2 || (locked && !flag3));
		UpdateUnlockText(flag2);
		if (locked && !flag2)
		{
			icon.material = iconMaterial;
		}
		else
		{
			icon.material = null;
		}
	}

	private void UpdateUnlockText(bool limitedDeployment)
	{
		if (!(unlockText == null))
		{
			unlockText.text = (limitedDeployment ? CellUI<PlayerCharacterState>.textData.term_limited_character_deployment : defaultUnlockText);
			unlockText.color = (limitedDeployment ? Color.white : defaultUnlockColor);
			unlockText.enableVertexGradient = limitedDeployment || defaultUnlockVertexGradient;
			unlockText.colorGradient = (limitedDeployment ? new VertexGradient(new Color32(byte.MaxValue, 248, 197, byte.MaxValue), new Color32(byte.MaxValue, 248, 197, byte.MaxValue), new Color32(byte.MaxValue, 166, 53, byte.MaxValue), new Color32(byte.MaxValue, 166, 53, byte.MaxValue)) : defaultUnlockColorGradient);
			unlockText.fontStyle = (limitedDeployment ? (defaultUnlockFontStyle | FontStyles.Bold) : defaultUnlockFontStyle);
			unlockText.outlineColor = (limitedDeployment ? new Color32(105, 20, 0, byte.MaxValue) : defaultUnlockOutlineColor);
			unlockText.outlineWidth = (limitedDeployment ? 0.22f : defaultUnlockOutlineWidth);
		}
	}
}
