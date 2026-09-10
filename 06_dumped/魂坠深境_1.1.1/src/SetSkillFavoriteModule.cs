public class SetSkillFavoriteModule : SkillInfoDisplay.DisplayModule
{
	public GameButton favorButton;

	public GameButton unfavorButton;

	private ProfileSkill sourceProfileSkill
	{
		get
		{
			ProfileSkill obj = base.profileSkill;
			if (obj == null)
			{
				GameSkill obj2 = base.gameSkill;
				if (obj2 == null)
				{
					return null;
				}
				obj = obj2.profileSkill;
			}
			return obj;
		}
	}

	public override void Init()
	{
		favorButton.onClick.AddListener(_OnFavorButtonClicked);
		unfavorButton.onClick.AddListener(_OnUnfavorButtonClicked);
	}

	public override void UpdateDisplay()
	{
		ProfileSkill profileSkill = sourceProfileSkill;
		if (profileSkill == null)
		{
			favorButton.gameObject.SetActive(value: false);
			unfavorButton.gameObject.SetActive(value: false);
		}
		else
		{
			favorButton.gameObject.SetActive(!profileSkill.isFavorite);
			unfavorButton.gameObject.SetActive(profileSkill.isFavorite);
		}
	}

	private void _OnFavorButtonClicked()
	{
		if (sourceProfileSkill != null)
		{
			sourceProfileSkill.isFavorite = true;
			UIDisplayModule.fortressState.applicationState.globalSkillManager.FavoriteSkillDirty = true;
			base.display.SetDirty();
			if (UIDisplayModule.fortressState.uiManager != null)
			{
				UIDisplayModule.fortressState.uiManager.SetDirty();
			}
		}
	}

	private void _OnUnfavorButtonClicked()
	{
		if (sourceProfileSkill != null)
		{
			sourceProfileSkill.isFavorite = false;
			UIDisplayModule.fortressState.applicationState.globalSkillManager.FavoriteSkillDirty = true;
			base.display.SetDirty();
			if (UIDisplayModule.fortressState.uiManager != null)
			{
				UIDisplayModule.fortressState.uiManager.SetDirty();
			}
		}
	}
}
