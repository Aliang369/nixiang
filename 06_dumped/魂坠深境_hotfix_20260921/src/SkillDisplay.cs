using Common.UI;

public class SkillDisplay : CellUI<GameSkill>, IDisplayModulesController
{
	public abstract class DisplayModule : UIDisplayModule
	{
		protected SkillDisplay display => parent as SkillDisplay;

		protected GameSkill gameSkill => display?.setupData;

		protected ProfileSkill profileSkill => gameSkill.profileSkill;
	}

	public class ProfileSkillConvertor : CellUIPrefabPool<SkillDisplay, GameSkill>.IDataConvertor<ProfileSkill>
	{
		public GameSkill ToSetupData(ProfileSkill t)
		{
			return new GameSkill(t);
		}

		public ProfileSkill FromSetupData(GameSkill setupData)
		{
			return setupData.profileSkill;
		}
	}

	public static ProfileSkillConvertor profileSkillConvertor = new ProfileSkillConvertor();

	protected override void Init()
	{
		RankModule component = GetComponent<RankModule>();
		if (component != null)
		{
			component.fQuality = () => base.setupData.QualityLevel;
		}
	}

	protected override void OnSetup(GameSkill _)
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}
}
