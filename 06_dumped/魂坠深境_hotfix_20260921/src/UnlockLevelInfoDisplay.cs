using TMPro;

public class UnlockLevelInfoDisplay : CellUI<TalentTree.LevelLockConfig>
{
	public GameButton backButton;

	public TextMeshProUGUI title;

	public TextMeshProUGUI description;

	protected override void Init()
	{
		backButton.onClick.AddListener(delegate
		{
			(parent as CharacterTalentUnlockPanel)?.infoDisplayPool.Despawn(this);
		});
	}

	protected override void OnSetup(TalentTree.LevelLockConfig config)
	{
		title.text = config.title;
		description.text = config.description;
	}
}
