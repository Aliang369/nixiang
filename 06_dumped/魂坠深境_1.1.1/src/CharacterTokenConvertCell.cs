using Common.UI;

public class CharacterTokenConvertCell : CellUI<(SkillDef, PlayerCharacterDef)>
{
	public SkillDisplay display;

	public GameButton button;

	private CharacterTokenConvertPanel parentPanel;

	protected override void Init()
	{
		InitUI(display);
		parentPanel = GetUIInParents<CharacterTokenConvertPanel>();
		display.GetModule<InfoModule>().infoPanel = CellUI<(SkillDef, PlayerCharacterDef)>.fortressState.uiManager.universalCommonInfoPanel;
		button.onClick.AddListener(_OnConvertButtonClicked);
	}

	protected override void OnSetup((SkillDef, PlayerCharacterDef) setupData)
	{
		ProfileSkill profileSkill = new ProfileSkill(setupData.Item1, 0);
		PlayerCharacterState characterState = CellUI<(SkillDef, PlayerCharacterDef)>.applicationState.characterManager.GetCharacterState(setupData.Item2);
		display.Setup(profileSkill.GetGameSkill());
		button.interactable = characterState.obsessionLevel > 0;
	}

	private void _OnConvertButtonClicked()
	{
	}
}
